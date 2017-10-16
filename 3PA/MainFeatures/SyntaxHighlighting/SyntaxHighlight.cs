#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (SyntaxHighlight.cs) is part of 3P.
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

using System.Diagnostics;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;

namespace _3PA.MainFeatures.SyntaxHighlighting {
    internal class SyntaxHighlight {

        #region real colorization todo
        
        /// <summary>
        /// Called on STYLENEEDED notification
        /// </summary>
        public static void Colorize(int startPos, int endPos) {
            //------------
            //var watch = Stopwatch.StartNew();
            //------------

            var line = Sci.LineFromPosition(startPos);
            var lineStartPos = Sci.GetLine(line).Position;
            var column = startPos - lineStartPos;
            
            Sci.StartStyling(lineStartPos);

            ProLexer tok = new ProLexer(Sci.GetTextByRange(lineStartPos, endPos), lineStartPos, line, column, 0, 0);
            SyntaxHighlightVisitor vis = new SyntaxHighlightVisitor();
            tok.Accept(vis);

            //--------------
            //watch.Stop();
            //UserCommunication.Notify("startPos = " + startPos + ", endPos = " + endPos + ", done in " + watch.ElapsedMilliseconds + " ms");
            //------------


        }

        #endregion

    }
}