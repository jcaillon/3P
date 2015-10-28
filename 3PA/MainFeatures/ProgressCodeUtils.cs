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
namespace _3PA.MainFeatures {
    class ProgressCodeUtils {

        public static void GoToDefinition() {
            // handles a stack of points to go back to where we came from when we "goto definition"
        }


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
