#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProgressCodeUtils.cs) is part of 3P.
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
using System.Text;
using _3PA.Html;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.ProgressExecution;

namespace _3PA.MainFeatures {
    class ProgressCodeUtils {

        #region Go to definition

        /// <summary>
        /// This method allows the user to GOTO a word definition, if a tooltip is opened then it tries to 
        /// go to the definition of the displayed word, otherwise it tries to find the declaration of the parsed word under the
        /// caret. At last, it tries to find a file in the propath
        /// </summary>
        public static void GoToDefinition() {
            try {
                // if a tooltip is opened, try to execute the "go to definition" of the tooltip first
                if (InfoToolTip.InfoToolTip.IsVisible) {
                    if (!string.IsNullOrEmpty(InfoToolTip.InfoToolTip.GoToDefinitionFile)) {
                        Npp.Goto(InfoToolTip.InfoToolTip.GoToDefinitionFile, InfoToolTip.InfoToolTip.GoToDefinitionPoint.X, InfoToolTip.InfoToolTip.GoToDefinitionPoint.Y);
                        InfoToolTip.InfoToolTip.Close();
                        return;
                    }
                    InfoToolTip.InfoToolTip.Close();
                }

                // try to go to the definition of the selected word
                var position = Npp.CurrentPosition;
                var curWord = Npp.GetWordAtPosition(position);


                // match a word in the autocompletion? go to definition
                var data = AutoComplete.FindInCompletionData(curWord, position);
                if (data != null && data.Count > 0) {
                    foreach (var completionData in data) {
                        if (completionData.FromParser) {
                            Npp.Goto(completionData.ParsedItem.FilePath, completionData.ParsedItem.Line, completionData.ParsedItem.Column);
                            return;
                        }
                    }
                }

                // last resort, try to find a matching file in the propath
                // first look in the propath
                var fullPaths = ProgressEnv.FindLocalFiles(curWord, Config.Instance.GlobalProgressExtension);
                if (fullPaths.Count > 0) {
                    if (fullPaths.Count > 1) {
                        var output = new StringBuilder(@"Found several files matching this name, please choose the correct one and i will open it for you :<br>");
                        foreach (var fullPath in fullPaths) {
                            output.Append("<br><a class='ToolGotoDefinition' href='" + fullPath + "'>" + fullPath + "</a>");
                        }
                        UserCommunication.Notify(output.ToString(), MessageImg.MsgQuestion, "Question", "Open a file", args => {
                            Npp.Goto(args.Link);
                            args.Handled = true;
                        }, 0, 500);
                    } else
                        Npp.Goto(fullPaths[0]);
                    return;
                }

                UserCommunication.Notify("Sorry pal, couldn't go to the definition of <b>" + curWord + "</b>", MessageImg.MsgInfo, "information", "Failed to find an origin", 5);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in GoToDefinition");
            }
        }

        #endregion

        #region Toggle comment

        /// <summary>
        /// Toggle comments on and off on selected lines
        /// </summary>
        public static void ToggleComment() {
            try {
                int mode = 0; // 0: null, 1: toggle off; 2: toggle on
                var startLine = Npp.LineFromPosition(Npp.SelectionStart);
                var endLine = Npp.LineFromPosition(Npp.SelectionEnd);

                Npp.BeginUndoAction();

                // for each line in the selection
                for (int iLine = startLine; iLine <= endLine; iLine++) {
                    var thisLine = new Npp.Line(iLine);
                    var startPos = thisLine.IndentationPosition;
                    var endPos = thisLine.EndPosition;

                    // the line is essentially empty
                    if ((endPos - startPos) == 0) {
                        // only one line selected, 
                        if (startLine == endLine) {
                            Npp.SetTextByRange(startPos, startPos, "/*  */");
                            Npp.SetSel(startPos + 3);
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
                    if (Npp.SelectionEnd - Npp.SelectionStart > 0) {
                        if (Npp.AnchorPosition > Npp.CurrentPosition)
                            Npp.SetSel(Npp.SelectionEnd + 2, Npp.SelectionStart);
                        else
                            Npp.SetSel(Npp.SelectionStart, Npp.SelectionEnd + 2);
                    }
                }

                Npp.EndUndoAction();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when commenting");
            }
        }

        #endregion

        public static void Open4GlHelp() {
            
        }

    }
}
