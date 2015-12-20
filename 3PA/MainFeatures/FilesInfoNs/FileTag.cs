#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileTags.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using _3PA.Lib;
using System.Text;

namespace _3PA.MainFeatures.FilesInfoNs {

    public class FileTag {

        #region Form handling

        public static FileTagsForm Form;
        private static Point _location;

        /// <summary>
        /// Call this method to show the form
        /// </summary>
        public static void UnCloak() {
            try {
                if (!Plug.AllowFeatureExecution())
                    return;

                if (Form != null && Form.Visible)
                    ForceClose();

                // init if needed
                if (Form == null) {
                    Form = new FileTagsForm {
                        UnfocusedOpacity = 1,
                        FocusedOpacity = 1,
                        OncloseAction = Cloak
                    };
                    Form.Show(Npp.Win32WindowNpp);
                    _location = Form.Location;
                }

                if (Form != null) {
                    Form.Location = _location;
                    Form.UpdateForm();
                    Form.UnCloack();
                    Form.Focus();
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when uncloaking file info");
            }
        }

        /// <summary>
        /// Call this method to hide the form
        /// </summary>
        public static void Cloak() {
            // toggle visibility
            if (Form != null && Form.Visible) {
                _location = Form.Location;
                Form.Cloack();
            }
        }

        /// <summary>
        /// Forces the form to close, only when leaving npp
        /// </summary>
        public static void ForceClose() {
            try {
                if (Form != null)
                    Form.ForceClose();
                Form = null;
            } catch (Exception x) {
                ErrorHandler.DirtyLog(x);
            }
        }

        #endregion

        #region fields

        private static string FilePath { get { return Path.Combine(Npp.GetConfigDir(), "FilesInfo.dump"); } }
        private static Dictionary<string, List<FileTagObject>> _filesInfo = new Dictionary<string, List<FileTagObject>>(StringComparer.CurrentCultureIgnoreCase);
        public const string DefaultTag = "DefaultTag";
        public const string LastTag = "LastTag";

        #endregion

        /// <summary>
        /// Load the dictionnary of file info
        /// </summary>
        public static void Import() {
            _filesInfo.Clear();
            try {
                ConfLoader.ForEachLine(FilePath, new byte[0], Encoding.Default, s => {
                    var items = s.Split('\t');
                    if (items.Count() == 8) {
                        var fileName = items[0].Trim();
                        var fileInfo = new FileTagObject() {
                            Nb = items[1],
                            Date = items[2],
                            Text = items[3],
                            NomAppli = items[4],
                            Version = items[5],
                            Chantier = items[6],
                            Jira = items[7]
                        };
                        // add to dictionnary
                        if (_filesInfo.ContainsKey(FilePath)) {
                            _filesInfo[fileName].Add(fileInfo);
                        } else {
                            _filesInfo.Add(fileName, new List<FileTagObject>() {
                                fileInfo
                            });
                        }
                    }
                });
                if (!_filesInfo.ContainsKey(DefaultTag))
                    SetFileTags(DefaultTag, "", "", "", "AFC", "", "", "");
                if (!_filesInfo.ContainsKey(LastTag))
                    SetFileTags(LastTag, "", "", "", "AFC", "", "", "");
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "", FilePath);
            }
        }

        /// <summary>
        /// Save the dicitonnary containing the file info
        /// </summary>
        public static void Export() {
            try {
                using (var writer = new StreamWriter(FilePath, false)) {
                    foreach (var kpv in _filesInfo) {
                        foreach (var obj in kpv.Value) {
                            writer.WriteLine(string.Join("\t", kpv.Key, obj.Nb, obj.Date, obj.Text, obj.NomAppli, obj.Version, obj.Chantier, obj.Jira));
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while saving the file info!");
            }
        }

        public static bool Contains(string filename) {
            return (!string.IsNullOrWhiteSpace(filename)) && _filesInfo.ContainsKey(filename);
        }

        public static List<FileTagObject> GetFileTagsList(string filename) {
            return Contains(filename) ? _filesInfo[filename].OrderByDescending(o => o.Nb).ToList() : new List<FileTagObject>();
        }

        public static FileTagObject GetLastFileTag(string filename) {
            return Contains(filename) ? _filesInfo[filename].Last() : new FileTagObject();
        }

        public static FileTagObject GetFileTags(string filename, string nb) {
            return (filename == LastTag || filename == DefaultTag)
                ? GetFileTagsList(filename).First()
                : GetFileTagsList(filename).Find(x => (x.Nb.Equals(nb)));
        }

        public static void SetFileTags(string filename, string nb, string date, string text, string nomAppli,
            string version, string chantier, string jira) {
            if (string.IsNullOrWhiteSpace(filename)) return;

            try {
                // filename exists
                lock (_filesInfo) {
                    if (Contains(filename)) {
                        if (filename == LastTag || filename == DefaultTag)
                            _filesInfo[filename].Clear();

                        // modif number exists
                        var found = false;
                        var idk = 0;
                        foreach (var item in _filesInfo[filename]) {
                            if (item.Nb == nb) {
                                found = true;
                                break;
                            }
                            idk++;
                        }
                        if (found)
                            _filesInfo[filename].RemoveAt(idk);

                        _filesInfo[filename].Add(new FileTagObject {
                            Nb = nb,
                            Date = date,
                            Text = text,
                            NomAppli = nomAppli,
                            Version = version,
                            Chantier = chantier,
                            Jira = jira
                        });
                    } else {
                        var newlist = new List<FileTagObject> {
                            new FileTagObject {
                                Nb = nb,
                                Date = date,
                                Text = text,
                                NomAppli = nomAppli,
                                Version = version,
                                Chantier = chantier,
                                Jira = jira
                            }
                        };
                        _filesInfo.Add(filename, newlist);
                    }
                }
                Export();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while setting a file info!");
            }
        }

    }

    public struct FileTagObject {
        public string Nb { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }
        public string NomAppli { get; set; }
        public string Version { get; set; }
        public string Chantier { get; set; }
        public string Jira { get; set; }
    }

}
