#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (ProgressCodeUtils.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using YamuiFramework.Forms;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures {
    class ProgressCodeUtils {

        #region Go to definition

        /// <summary>
        /// handles a stack of points to go back to where we came from when we "goto definition"
        /// </summary>
        private static Stack<Tuple<string, Point>> _goToHistory = new Stack<Tuple<string, Point>>();

        /// <summary>
        /// This method allows the user to GOTO a word definition, if a tooltip is opened then it tries to 
        /// go to the definition of the displayed word, otherwise it tries to find the declaration of the parsed word under the
        /// caret. At last, it tries to find a file in the propath
        /// </summary>
        public static void GoToDefinition() {
            // if a tooltip is opened, try to execute the "go to definition" of the tooltip first
            if (InfoToolTip.InfoToolTip.IsVisible) {
                if (!string.IsNullOrEmpty(InfoToolTip.InfoToolTip.GoToDefinitionFile)) {
                    RememberCurrentPosition();
                    Npp.Goto(InfoToolTip.InfoToolTip.GoToDefinitionFile, InfoToolTip.InfoToolTip.GoToDefinitionPoint.X, InfoToolTip.InfoToolTip.GoToDefinitionPoint.Y);
                    InfoToolTip.InfoToolTip.Close();
                    return;
                }
                InfoToolTip.InfoToolTip.Close();
            }

            // try to go to the definition of the selected word
            var position = Npp.GetCaretPosition();
            var curWord = Npp.GetWordAtPosition(position);


            // match a word in the autocompletion? go to definition
            var data = AutoComplete.FindInCompletionData(curWord, position);
            if (data != null && data.Count > 0) {
                foreach (var completionData in data) {
                    if (completionData.FromParser) {
                        RememberCurrentPosition();
                        Npp.Goto(completionData.ParsedItem.FilePath, completionData.ParsedItem.Line, completionData.ParsedItem.Column);
                        return;
                    }
                }
            }

            // last resort, try to find a matching file in the propath
            var i = 0;
            var output = new StringBuilder();
            string lastFullPath = "";
            output.Append(@"Found several files matching this name, please choose the correct one and i will open it for you :<br>");
            foreach (var extension in Config.Instance.GlobalProgressExtension.Split(',')) {
                var fullPath = ProgressEnv.FindFileInPropath(curWord + extension);
                if (!string.IsNullOrEmpty(fullPath)) {
                    i++;
                    lastFullPath = fullPath;
                    output.Append("<br><a class='ToolGotoDefinition' href='" + fullPath + "'>" + fullPath + "</a>");
                }
            }
            if (i > 1) {
                UserCommunication.Notify(output.ToString(), MessageImage.Question, "Question", args => { 
                    Npp.Goto(args.Link);
                    args.Handled = true;
                },"Open a file", 0, 500);
                return;
            } 
            if (i == 1) {
                Npp.Goto(lastFullPath);
                return;
            }

            UserCommunication.Notify("Sorry pal, couldn't go to the definition of <b>" + curWord + "</b>", MessageImage.Info, "info", "Don't be mad", 5);
        }

        /// <summary>
        /// When you use the GoToDefinition method, you stack points of your position before the jump,
        /// this method allows you to navigate back to where you were
        /// </summary>
        public static void GoBackFromDefinition() {
            if (_goToHistory.Count > 0) {
                var lastPoint = _goToHistory.Pop();
                Npp.Goto(lastPoint.Item1, lastPoint.Item2.X, lastPoint.Item2.Y);
            }
        }

        private static void RememberCurrentPosition() {
            _goToHistory.Push(new Tuple<string, Point>(Npp.GetCurrentFilePath(), new Point(Npp.GetLineFromPosition(Npp.GetCaretPosition()), Npp.GetColumnFromPos(Npp.GetCaretPosition()))));
        }

        #endregion



        /// <summary>
        /// Toggle comments on and off on selected lines
        /// </summary>
        public static void ToggleComment() {
            int mode = 0; // 0: null, 1: toggle off; 2: toggle on
            var selectionRange = Npp.GetSelectionRange();
            var startLine = Npp.GetLineFromPosition(selectionRange.cpMin);
            var endLine = Npp.GetLineFromPosition(selectionRange.cpMax);

            Npp.BeginUndoAction();

            // for each line in the selection
            for (int iLine = startLine; iLine <= endLine; iLine++) {
                var startPos = Npp.GetLineIndentPosition(iLine);
                var endPos = Npp.GetLineEndPosition(iLine);

                // the line is essentially empty
                if ((endPos - startPos) == 0) {
                    // only one line selected, 
                    if (startLine == endLine) {
                        Npp.SetTextByRange(startPos, startPos, "/*  */");
                        Npp.SetCaretPosition(startPos + 3);
                    }
                    continue;
                }

                // line is surrounded by /* */
                if (Npp.GetTextOnRightOfPos(startPos, 2).Equals("/*") && Npp.GetTextOnLeftOfPos(endPos, 2).Equals("*/")) {
                    if (mode == 0) mode = 1; // toggle off

                    // add /* */ ?
                    if (mode == 2 && (endPos - 4 - startPos) > 0) {

                        Npp.SetTextByRange(endPos, endPos, "*/");
                        Npp.SetTextByRange(startPos, startPos, "/*");
                    } else {
                        // delete /* */
                        Npp.SetTextByRange(endPos - 2, endPos, string.Empty);
                        Npp.SetTextByRange(startPos, startPos + 2, string.Empty);
                    }

                } else {
                    if (mode == 0) mode = 2; // toggle on

                    // there are no /* */ but we need to put some
                    if (mode == 2) {
                        Npp.SetTextByRange(endPos, endPos, "*/");
                        Npp.SetTextByRange(startPos, startPos, "/*");
                    }
                }
            }

            // correct selection...
            if (mode == 2) {
                selectionRange = Npp.GetSelectionRange();
                if (selectionRange.cpMax - selectionRange.cpMin > 0) {
                    if (Npp.GetAnchorPosition() > Npp.GetCaretPosition())
                        Npp.SetSelectionOrdered(selectionRange.cpMax + 2, selectionRange.cpMin);
                    else
                        Npp.SetSelectionOrdered(selectionRange.cpMin, selectionRange.cpMax + 2);
                }
            }

            Npp.EndUndoAction();
        }

    }
}
