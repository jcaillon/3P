#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileTag.cs) is part of 3P.
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
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.FilesInfoNs {

    internal static class FileTag {

        #region fields

        
        private static Dictionary<string, List<FileTagObject>> _filesInfo = new Dictionary<string, List<FileTagObject>>(StringComparer.CurrentCultureIgnoreCase);
        public const string DefaultTag = "DefaultTag";
        public const string LastTag = "LastTag";

        #endregion

        #region handle data

        /// <summary>
        /// Load the dictionnary of file info
        /// </summary>
        public static void Import() {
            _filesInfo.Clear();
            try {
                ConfLoader.ForEachLine(Config.FileFilesInfo, new byte[0], Encoding.Default, s => {
                    var items = s.Split('\t');
                    if (items.Count() == 8) {
                        var fileName = items[0].Trim();
                        var fileInfo = new FileTagObject {
                            CorrectionNumber = items[1],
                            CorrectionDate = items[2],
                            CorrectionDecription = items[3],
                            ApplicationName = items[4],
                            ApplicationVersion = items[5],
                            WorkPackage = items[6],
                            BugId = items[7]
                        };
                        // add to dictionnary
                        if (_filesInfo.ContainsKey(fileName)) {
                            _filesInfo[fileName].Add(fileInfo);
                        } else {
                            _filesInfo.Add(fileName, new List<FileTagObject> {
                                fileInfo
                            });
                        }
                    }
                });
                if (!_filesInfo.ContainsKey(DefaultTag))
                    SetFileTags(DefaultTag, "", "", "", "", "", "", "");
                if (!_filesInfo.ContainsKey(LastTag))
                    SetFileTags(LastTag, "", "", "", "", "", "", "");
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "", Config.FileFilesInfo);
            }
        }

        /// <summary>
        /// Save the dicitonnary containing the file info
        /// </summary>
        public static void Export() {
            try {
                using (var writer = new StreamWriter(Config.FileFilesInfo, false)) {
                    foreach (var kpv in _filesInfo) {
                        foreach (var obj in kpv.Value) {
                            writer.WriteLine(string.Join("\t", kpv.Key, obj.CorrectionNumber, obj.CorrectionDate, obj.CorrectionDecription, obj.ApplicationName, obj.ApplicationVersion, obj.WorkPackage, obj.BugId));
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
            return Contains(filename) ? _filesInfo[filename].OrderByDescending(o => o.CorrectionNumber).ToList() : new List<FileTagObject> ();
        }

        public static FileTagObject GetLastFileTag(string filename) {
            return Contains(filename) ? _filesInfo[filename].Last() : new FileTagObject();
        }

        public static FileTagObject GetFileTags(string filename, string nb) {
            return (filename == LastTag || filename == DefaultTag)
                ? GetFileTagsList(filename).First()
                : GetFileTagsList(filename).Find(x => (x.CorrectionNumber.Equals(nb)));
        }

        public static void SetFileTags(string filename, string nb, string date, string text, string nomAppli,
            string version, string chantier, string jira) {
            if (string.IsNullOrWhiteSpace(filename)) return;

            try {
                var obj = new FileTagObject {
                    CorrectionNumber = nb,
                    CorrectionDate = date,
                    CorrectionDecription = text,
                    ApplicationName = nomAppli,
                    ApplicationVersion = version,
                    WorkPackage = chantier,
                    BugId = jira
                };
                // filename exists
                if (Contains(filename)) {
                    if (filename == LastTag || filename == DefaultTag)
                        _filesInfo[filename].Clear();

                    // modif number exists
                    _filesInfo[filename].RemoveAll(o => o.CorrectionNumber == nb);
                    _filesInfo[filename].Add(obj);
                } else {
                    _filesInfo.Add(filename, new List<FileTagObject> { obj });
                }
                Export();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while setting a file info!");
            }
        }

        #endregion
    }

    #region File tag object

    internal struct FileTagObject {
        public string CorrectionNumber { get; set; }
        public string CorrectionDate { get; set; }
        public string CorrectionDecription { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; }
        public string WorkPackage { get; set; }
        public string BugId { get; set; }
    }

    #endregion


}
