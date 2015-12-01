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

namespace _3PA.MainFeatures.FileInfo {

    public class FileTags {

        #region Form handling

        public static FileTagsForm Form;
        private static Point _location;

        /// <summary>
        /// Call this method to show the form
        /// </summary>
        public static void UnCloak() {
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
                Form.ForceClose();
                Form = null;
            } catch (Exception) {
                // ignored
            }
        }

        #endregion

        #region fields

        private const string FileName = "filetags.data";
        public static SpecialDictionary<List<FileTag>> Map = new SpecialDictionary<List<FileTag>>();

        #endregion


        private static string ConfigFile {
            get { return Path.Combine(Npp.GetConfigDir(), FileName); }
        }

        public static List<string> Keys {
            get { return Map.Keys.ToList(); }
        }

        public static bool Contains(string filename) {
            lock (Map) {
                return (!string.IsNullOrWhiteSpace(filename)) && Map.ContainsKey(filename);
            }
        }

        public static void Init() {
            lock (Map) {
                if (File.Exists(ConfigFile)) {
                    Map.Clear();
                    try {
                        Map.Load(ConfigFile);
                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Error while loading file info!", ConfigFile);
                    }
                }
                try {
                    if (!Contains("default_tag"))
                        SetFileTags("default_tag", "", "", "", "AFC", "", "", "");
                    if (!Contains("last_tag"))
                        SetFileTags("last_tag", "", "", "", "AFC", "", "", "");
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error while setting default file info!");
                }
            }
        }

        public static List<FileTag> GetFileTagsList(string filename) {
            lock (Map) {
                return Contains(filename) ? Map[filename].OrderByDescending(o => o.Nb).ToList() : new List<FileTag>();
            }
        }

        public static FileTag GetLastFileTag(string filename) {
            lock (Map) {
                return Contains(filename) ? Map[filename].Last() : new FileTag();
            }
        }

        public static FileTag GetFileTags(string filename, string nb) {
            return (filename == "last_tag" || filename == "default_tag")
                ? GetFileTagsList(filename).First()
                : GetFileTagsList(filename).Find(x => (x.Nb.Equals(nb)));
        }

        public static void SetFileTags(string filename, string nb, string date, string text, string nomAppli,
            string version, string chantier, string jira) {
            if (string.IsNullOrWhiteSpace(filename)) return;

            try {
                // filename exists
                lock (Map) {
                    if (Contains(filename)) {
                        if (filename == "last_tag" || filename == "default_tag")
                            Map[filename].Clear();

                        // modif number exists
                        var found = false;
                        var idk = 0;
                        foreach (var item in Map[filename]) {
                            if (item.Nb == nb) {
                                found = true;
                                break;
                            }
                            idk++;
                        }
                        if (found)
                            Map[filename].RemoveAt(idk);

                        Map[filename].Add(new FileTag {
                            Nb = nb,
                            Date = date,
                            Text = text,
                            NomAppli = nomAppli,
                            Version = version,
                            Chantier = chantier,
                            Jira = jira
                        });
                    } else {
                        var newlist = new List<FileTag> {
                            new FileTag {
                                Nb = nb,
                                Date = date,
                                Text = text,
                                NomAppli = nomAppli,
                                Version = version,
                                Chantier = chantier,
                                Jira = jira
                            }
                        };
                        Map.Add(filename, newlist);
                    }
                }
                Save();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while setting a file info!");
            }
        }

        public static void Save() {
            lock (Map) {
                try {
                    Map.Save(ConfigFile);
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error while saving file info!");
                }
            }
        }

    }

    public struct FileTag {
        public string Nb { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }
        public string NomAppli { get; set; }
        public string Version { get; set; }
        public string Chantier { get; set; }
        public string Jira { get; set; }
    }

}
