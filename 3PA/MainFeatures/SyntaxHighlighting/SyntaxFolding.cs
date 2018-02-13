#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (SyntaxFolding.cs) is part of 3P.
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
using System.Collections.Generic;
using System.Threading.Tasks;
using _3PA.MainFeatures.Parser.Pro;
using _3PA.NppCore;

namespace _3PA.MainFeatures.SyntaxHighlighting {

    internal class SyntaxFolding {

        public static void OnParseEndParserItems(List<ParserError> arg1, Dictionary<int, LineInfo> lineInfos, List<ParsedItem> arg3) {
            if (lineInfos != null) {
                var lineInfoCopy = new Dictionary<int, LineInfo>(lineInfos);

                Task.Factory.StartNew(() => {
                    UiThread.Invoke(() => SetFolding(lineInfoCopy));
                });
            }
        }

        public static void SetFolding(Dictionary<int, LineInfo> lineInfos) {
            var i = 0;
            var lastIdent = 0;
            while (lineInfos.ContainsKey(i)) {
                var line = Sci.GetLine(i);
                line.SetFoldLevel(lineInfos[i].BlockDepth + lineInfos[i].ExtraStatementDepth, FoldLevelFlags.None);
                if (lineInfos[i].BlockDepth + lineInfos[i].ExtraStatementDepth > lastIdent) {
                    if (i > 0)
                        Sci.GetLine(i - 1).FoldLevelFlags = FoldLevelFlags.Header;
                    lastIdent = lineInfos[i].BlockDepth + lineInfos[i].ExtraStatementDepth;
                } else {
                    if (lineInfos[i].BlockDepth < lastIdent)
                        lastIdent = lineInfos[i].BlockDepth + lineInfos[i].ExtraStatementDepth;

                }
                i++;
            }
        }
    }
}
