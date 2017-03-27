#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Text.RegularExpressions;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Pro {
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

            Utils.ForEachLine(Config.FileFilesInfo, new byte[0], (i, line) => {
                    var items = line.Split('\t');
                    if (items.Count() == 8) {
                        var fileName = items[0].Trim();
                        var fileInfo = new FileTagObject {
                            CorrectionNumber = items[1],
                            CorrectionDate = items[2],
                            CorrectionDecription = items[3].Replace("~n", "\n"),
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
                },
                Encoding.Default);

            if (!_filesInfo.ContainsKey(DefaultTag))
                SetFileTags(DefaultTag, "", "", "", "", "", "", "");
            if (!_filesInfo.ContainsKey(LastTag))
                SetFileTags(LastTag, "", "", "", "", "", "", "");
        }

        /// <summary>
        /// Save the dicitonnary containing the file info
        /// </summary>
        public static void Export() {
            try {
                using (var writer = new StreamWriter(Config.FileFilesInfo, false, Encoding.Default)) {
                    foreach (var kpv in _filesInfo) {
                        foreach (var obj in kpv.Value) {
                            writer.WriteLine(string.Join("\t", kpv.Key, obj.CorrectionNumber, obj.CorrectionDate, obj.CorrectionDecription.Replace("\r", "").Replace("\n", "~n"), obj.ApplicationName, obj.ApplicationVersion, obj.WorkPackage, obj.BugId));
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
            return Contains(filename) ? _filesInfo[filename] : new List<FileTagObject>();
        }

        public static FileTagObject GetLastFileTag(string filename) {
            return GetFileTagsList(filename).Last();
        }

        public static FileTagObject GetFileTags(string filename, string nb) {
            return (filename == LastTag || filename == DefaultTag) ? GetFileTagsList(filename).First() : GetFileTagsList(filename).Find(x => (x.CorrectionNumber.Equals(nb)));
        }

        public static void SetFileTags(string filename, string nb, string date, string text, string nomAppli, string version, string chantier, string jira) {
            if (string.IsNullOrWhiteSpace(filename)) return;
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
                _filesInfo.Add(filename, new List<FileTagObject> {obj});
            }
        }

        public static bool DeleteFileTags(string filename, string correctionNumber) {
            if (string.IsNullOrWhiteSpace(filename) || filename == LastTag || filename == DefaultTag || !Contains(filename))
                return false;

            _filesInfo[filename].RemoveAll(o => o.CorrectionNumber == correctionNumber);
            if (_filesInfo[filename].Count == 0)
                _filesInfo.Remove(filename);
            return true;
        }

        #endregion

        #region public

        /// <summary>
        /// Call this method to replace the variables inside your tags template (e.g. {&a }) to their actual values
        /// </summary>
        public static string ReplaceTokens(FileTagObject fileTagObject, string tagString) {
            var output = tagString;
            foreach (var tuple in new List<Tuple<string, string>> {
                new Tuple<string, string>(@"({&a\s*})", fileTagObject.ApplicationName),
                new Tuple<string, string>(@"({&v\s*})", fileTagObject.ApplicationVersion),
                new Tuple<string, string>(@"({&b\s*})", fileTagObject.BugId),
                new Tuple<string, string>(@"({&da\s*})", fileTagObject.CorrectionDate),
                new Tuple<string, string>(@"({&de\s*})", fileTagObject.CorrectionDecription),
                new Tuple<string, string>(@"({&n\s*})", fileTagObject.CorrectionNumber),
                new Tuple<string, string>(@"({&w\s*})", fileTagObject.WorkPackage),
                new Tuple<string, string>(@"({&u\s*})", Config.Instance.UserName)
            }) {
                var regex = new Regex(tuple.Item1);
                var match = regex.Match(output);
                if (match.Success) {
                    var matchedStr = match.Groups[1].Value;
                    if (matchedStr.Contains(' ')) {
                        // need to replace the same amount of char
                        output = output.Replace(matchedStr, string.Format("{0,-" + matchedStr.Length + @"}", tuple.Item2 ?? ""));
                    } else {
                        output = output.Replace(matchedStr, tuple.Item2 ?? "");
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Allows the user to surround its selection with custom modification tags
        /// </summary>
        public static void SurroundSelectionWithTag() {
            CommonTagAction(fileInfo => {
                var output = new StringBuilder();

                Sci.TargetFromSelection();
                var indent = new String(' ', Sci.GetLine(Sci.LineFromPosition(Sci.TargetStart)).Indentation);

                var opener = ReplaceTokens(fileInfo, Config.Instance.TagModifOpener);
                var eol = Sci.GetEolString;
                output.Append(opener);
                output.Append(eol);
                output.Append(indent);
                output.Append(Sci.SelectedText);
                output.Append(eol);
                output.Append(indent);
                output.Append(ReplaceTokens(fileInfo, Config.Instance.TagModifCloser));

                Sci.TargetFromSelection();
                Sci.ReplaceTarget(output.ToString());

                Sci.SetSel(Sci.TargetStart + opener.Length + eol.Length);
            });
        }

        /// <summary>
        /// Allows the user to generate a title block at the caret location, using the current file info
        /// </summary>
        public static void AddTitleBlockAtCaret() {
            CommonTagAction(fileInfo => {
                var output = new StringBuilder();
                var eol = Sci.GetEolString;
                output.Append(ReplaceTokens(fileInfo, Config.Instance.TagTitleBlock1));
                output.Append(eol);

                // description
                var regex = new Regex(@"({&de\s*})");
                var match = regex.Match(Config.Instance.TagTitleBlock2);
                if (match.Success && !String.IsNullOrEmpty(fileInfo.CorrectionDecription)) {
                    var matchedStr = match.Groups[1].Value;
                    foreach (var line in fileInfo.CorrectionDecription.BreakText(matchedStr.Length).Split('\n')) {
                        output.Append(Config.Instance.TagTitleBlock2.Replace(matchedStr, String.Format("{0,-" + matchedStr.Length + @"}", line)));
                        output.Append(eol);
                    }
                }

                output.Append(ReplaceTokens(fileInfo, Config.Instance.TagTitleBlock3));
                output.Append(eol);

                Sci.SetTextByRange(Sci.CurrentPosition, Sci.CurrentPosition, output.ToString());
                Sci.SetSel(Sci.CurrentPosition + output.Length);
            });
        }

        #endregion

        #region Private

        private static void CommonTagAction(Action<FileTagObject> performAction) {
            var filename = Npp.CurrentFile.FileName;
            if (Contains(filename)) {
                var fileInfo = GetLastFileTag(filename);
                Sci.BeginUndoAction();
                performAction(fileInfo);
                Sci.EndUndoAction();
            } else {
                UserCommunication.Notify("No info available for this file, please fill the file info form first!", MessageImg.MsgToolTip, "Insert modification tags", "No info available", 4);
                Appli.Appli.GoToPage(PageNames.FileInfo);
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