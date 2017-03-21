#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProCodeFormat.cs) is part of 3P.
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

using System.IO;
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Pro {
    internal static class ProCodeFormat {
        /// <summary>
        /// Tries to re-indent the code of the whole document
        /// </summary>
        public static void CorrectCodeIndentation() {
            // handle spam (2s min between 2 indent)
            if (Utils.IsSpamming("CorrectCodeIndentation", 20000))
                return;

            // make sure to parse the current document before doing anything
            var linesLogFile = Path.Combine(Config.FolderTemp, "lines.log");
            var parseErrors = ParserHandler.GetLastParseErrorsInHtml();
            var canIndent = string.IsNullOrEmpty(parseErrors);

            // start indenting
            Sci.BeginUndoAction();

            StringBuilder x = new StringBuilder();
            var indentWidth = Sci.TabWidth;
            var i = 0;
            var dic = ParserHandler.LineInfo;
            while (dic.ContainsKey(i)) {
                if (canIndent)
                    Sci.GetLine(i).Indentation = dic[i].BlockDepth*indentWidth;
                else
                    x.AppendLine(i + 1 + " > " + dic[i].BlockDepth + " , " + dic[i].Scope.ScopeType + " , " + dic[i].Scope.Name);
                i++;
            }
            Utils.FileWriteAllText(linesLogFile, x.ToString());

            Sci.EndUndoAction();

            // Can we indent? We can't if we didn't parse the code correctly or if there are grammar errors
            if (!canIndent) {
                UserCommunication.Notify("This action can't be executed right now because it seems that your document contains grammatical errors.<br><br><i>If the code compiles successfully then i failed to parse your document correctly, please make sure to create an issue on the project's github and (if possible) include the incriminating code so i can fix this problem : <br>" + Config.IssueUrl.ToHtmlLink() + (Config.IsDevelopper ? "<br><br>Lines report log :<br>" + linesLogFile.ToHtmlLink() + "<br><br>" + parseErrors : ""), MessageImg.MsgRip, "Correct document indentation", "Incorrect grammar", null, 10);
            }
        }

        /// <summary>
        /// Allows to clear the appbuilder markup from a given string
        /// </summary>
        public static string StripAppBuilderMarkup(string inputSnippet) {
            // consist in suppressing the lines starting with :
            // &ANALYZE-SUSPEND
            // &ANALYZE-RESUME
            // /* _UIB-CODE-BLOCK-END */
            // and, for this method only, also strips :
            // &IF DEFINED(EXCLUDE-&{name}) = 0 &THEN
            // &ENDIF
            var outputSnippet = new StringBuilder();
            string line;
            using (StringReader reader = new StringReader(inputSnippet)) {
                while ((line = reader.ReadLine()) != null) {
                    if (line.Length == 0 || (line[0] != '&' && !line.Equals(@"/* _UIB-CODE-BLOCK-END */")))
                        outputSnippet.AppendLine(line);
                }
            }
            return outputSnippet.ToString();
        }
    }
}