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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Pro {

    internal static class ProCodeFormat {

        /// <summary>
        /// Tries to re-indent the code of the whole document
        /// </summary>
        public static void CorrectCodeIndentation() {

            // handle spam (1s min between 2 indent)
            if (Utils.IsSpamming("CorrectCodeIndentation", 1000))
                return;

            var parser = new Parser.Parser(Sci.Text, Npp.CurrentFile.Path, null, false);

            // in case of an incorrect document, warn the user
            var parserErrors = parser.ParseErrorsInHtml;
            if (!string.IsNullOrEmpty(parserErrors)) {
                if (UserCommunication.Message("The internal parser of 3P has found inconsistencies in your document :<br>" + parserErrors + "<br>You can still try to format your code but the result is not guaranteed (worst case scenario you can press CTRL+Z).<br>Please confirm that you want to proceed", MessageImg.MsgQuestion, "Correct indentation", "Problems spotted", new List<string> { "Continue", "Abort" }) != 0)
                    return;
            }

            var linesLogFile = Path.Combine(Config.FolderTemp, "lines.log");
            var canIndentSafely = string.IsNullOrEmpty(parserErrors);

            // start indenting
            Sci.BeginUndoAction();

            StringBuilder x = new StringBuilder();
            var indentWidth = Sci.TabWidth;
            var i = 0;
            var dic = parser.LineInfo;
            while (dic.ContainsKey(i)) {
                Sci.GetLine(i).Indentation = dic[i].BlockDepth * indentWidth;
                if (!canIndentSafely)
                    x.AppendLine(i + 1 + " > " + dic[i].BlockDepth + " , " + dic[i].Scope.ScopeType + " , " + dic[i].Scope.Name);
                i++;
            }
            Sci.EndUndoAction();

            // if we didn't parse the code correctly or if there are grammar errors
            if (!canIndentSafely) {
                Utils.FileWriteAllText(linesLogFile, x.ToString());
                UserCommunication.Notify("If the code compiles successfully and the document is incorrectly formatted, please make sure to create an issue on the project's github and (if possible) include the incriminating code so i can fix this problem : <br>" + Config.IssueUrl.ToHtmlLink() + (Config.IsDevelopper ? "<br><br>Lines report log :<br>" + linesLogFile.ToHtmlLink() : ""), MessageImg.MsgRip, "Correct indentation", "Incorrect grammar", null, 10);
            }
        }

        /// <summary>
        /// Returns true if the document starts with & ANALYZE-SUSPEND _VERSION-NUMBER
        /// which indicates that it will be opened as a structured proc in the appbuilder
        /// </summary>
        /// <returns></returns>
        public static bool IsCurrentFileFromAppBuilder {
            get {
                if (!Sci.GetLine(0).LineText.Trim().StartsWith("&ANALYZE-SUSPEND _VERSION-NUMBER", StringComparison.CurrentCultureIgnoreCase))
                    return false;
                return true;
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

        private static HashSet<string> _displayParserErrorsIgnoredFiles = new HashSet<string>();

        /// <summary>
        /// Check the validity of a progress code in the point of view of the appbuilder (make sure it can be opened within the appbuilder)
        /// </summary>
        public static void DisplayParserErrors(bool silent = false) {
            if (Npp.CurrentFile.IsProgress && !_displayParserErrorsIgnoredFiles.Contains(Npp.CurrentFile.Path)) {
                Task.Factory.StartNew(() => {
                    var currentFilePath = Npp.CurrentFile.Path;
                    var message = new StringBuilder();
                    message.Append("The analyzed file was :<br>" + currentFilePath.ToHtmlLink() + "<br>");

                    var parser = new Parser.Parser(Sci.Text, currentFilePath, null, false);

                    var parserErrors = parser.ParseErrorsInHtml;
                    if (!string.IsNullOrEmpty(parserErrors)) {
                        message.Append("<br>The parser found the following syntax errors :<br>");
                        message.Append(parserErrors);
                    }

                    var blockTooLong = new StringBuilder();
                    foreach (var scope in parser.ParsedItemsList.Where(item => item is ParsedImplementation || item is ParsedProcedure || item is ParsedOnStatement).Cast<ParsedScopeItem>()) {
                        if (CheckForTooMuchChar(scope)) {
                            blockTooLong.AppendLine("<div>");
                            blockTooLong.AppendLine(" - " + (scope.FilePath + "|" + scope.Line).ToHtmlLink("Line " + (scope.Line + 1) + " : <b>" + scope.Name + "</b>") + " (" + NbExtraCharBetweenLines(scope.Line, scope.EndBlockLine) + " extra chars)");
                            blockTooLong.AppendLine("</div>");
                        }
                    }
                    if (blockTooLong.Length > 0) {
                        message.Append("<br>This file is currently unreadable in the AppBuilder.<br>The following blocks contain more characters than the max limit (" + Config.Instance.GlobalMaxNbCharInBlock + " characters) :<br>");
                        message.Append(blockTooLong);
                        message.Append("<br><i>To prevent this, reduce the number of characters in the above blocks.<br>Deleting dead code and trimming spaces is a good place to start!</i>");
                    }

                    // no errors
                    var noProb = blockTooLong.Length == 0 && string.IsNullOrEmpty(parserErrors);
                    if (noProb) {
                        if (silent)
                            return;
                        message.Append("No problems found!");
                    } else {
                        if (silent)
                            message.Append("<br><br>" + "disable".ToHtmlLink("Click here to disable the automatic check for this file"));
                    }

                    UserCommunication.NotifyUnique("DisplayParserErrors", message.ToString(), noProb ? MessageImg.MsgOk : MessageImg.MsgWarning, "Check code validity", "Analysis results", args => {
                        if (args.Link.Equals("disable")) {
                            args.Handled = true;
                            UserCommunication.CloseUniqueNotif("DisplayParserErrors");
                            if (!_displayParserErrorsIgnoredFiles.Contains(currentFilePath))
                                _displayParserErrorsIgnoredFiles.Add(currentFilePath);
                        }
                    }, noProb ? 5 : 0);
                });
            }
        }


        /// <summary>
        /// Check the parse scope has too much char to allow it to be displayed in the appbuilder
        /// </summary>
        /// <param name="pars"></param>
        private static bool CheckForTooMuchChar(ParsedScopeItem pars) {
            // check length of block
            if (!pars.Flags.HasFlag(ParseFlag.FromInclude)) {
                pars.TooLongForAppbuilder = NbExtraCharBetweenLines(pars.Line, pars.EndBlockLine) > 0;
                if (pars.TooLongForAppbuilder)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// returns the number of chars between two lines in the current document
        /// </summary>
        private static int NbExtraCharBetweenLines(int startLine, int endLine) {
            return (Sci.StartBytePosOfLine(endLine) - Sci.StartBytePosOfLine(startLine)) - Config.Instance.GlobalMaxNbCharInBlock;
        }


    }
}