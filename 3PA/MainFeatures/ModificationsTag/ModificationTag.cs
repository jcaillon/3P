#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ModificationTag.cs) is part of 3P.
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.NppCore;

namespace _3PA.MainFeatures.ModificationsTag {

    internal static class ModificationTag {

        #region public

        /// <summary>
        /// Call this method to replace the variables inside your tags template (e.g. ${a }) to their actual values
        /// </summary>
        public static string ReplaceTokens(FileTagObject fileTagObject, string tagString) {
            var output = tagString;
            foreach (var tuple in new List<Tuple<string, string>> {
                new Tuple<string, string>(@"(\${a\s*})", fileTagObject.ApplicationName),
                new Tuple<string, string>(@"(\${v\s*})", fileTagObject.ApplicationVersion),
                new Tuple<string, string>(@"(\${b\s*})", fileTagObject.BugId),
                new Tuple<string, string>(@"(\${da\s*})", fileTagObject.CorrectionDate),
                new Tuple<string, string>(@"(\${de\s*})", fileTagObject.CorrectionDecription),
                new Tuple<string, string>(@"(\${n\s*})", fileTagObject.CorrectionNumber),
                new Tuple<string, string>(@"(\${w\s*})", fileTagObject.WorkPackage),
                new Tuple<string, string>(@"(\${u\s*})", Config.Instance.UserName)
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

                var opener = ReplaceTokens(fileInfo, ModificationTagTemplate.Instance.TagOpener);
                var eol = Sci.GetEolString;
                output.Append(opener);
                output.Append(eol);
                output.Append(indent);
                output.Append(Sci.SelectedText);
                output.Append(eol);
                output.Append(indent);
                output.Append(ReplaceTokens(fileInfo, ModificationTagTemplate.Instance.TagCloser));

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
                output.Append(ReplaceTokens(fileInfo, ModificationTagTemplate.Instance.TitleBlockHeader));
                output.Append(eol);

                // description
                var regex = new Regex(@"(\${de\s*})");
                var match = regex.Match(ModificationTagTemplate.Instance.TitleBlockLine);
                if (match.Success && !String.IsNullOrEmpty(fileInfo.CorrectionDecription)) {
                    var matchedStr = match.Groups[1].Value;
                    foreach (var line in fileInfo.CorrectionDecription.BreakText(matchedStr.Length).Split('\n')) {
                        output.Append(ModificationTagTemplate.Instance.TitleBlockLine.Replace(matchedStr, String.Format("{0,-" + matchedStr.Length + @"}", line)));
                        output.Append(eol);
                    }
                }

                output.Append(ReplaceTokens(fileInfo, ModificationTagTemplate.Instance.TitleBlockFooter));
                output.Append(eol);

                Sci.SetTextByRange(Sci.CurrentPosition, Sci.CurrentPosition, output.ToString());
                Sci.SetSel(Sci.CurrentPosition + output.Length);
            });
        }

        #endregion

        #region Private

        private static void CommonTagAction(Action<FileTagObject> performAction) {
            var filename = Npp.CurrentFileInfo.FileName;
            if (FileCustomInfo.Contains(filename)) {
                var fileInfo = FileCustomInfo.GetLastFileTag(filename);
                Sci.BeginUndoAction();
                performAction(fileInfo);
                Sci.EndUndoAction();
            } else {
                UserCommunication.Notify("No info available for this file, please fill the file info form first!", MessageImg.MsgToolTip, "Insert modification tags", "No info available", 5);
                Appli.Appli.GoToPage(PageNames.FileInfo);
            }
        }

        #endregion

    }
}
