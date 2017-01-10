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
namespace _3PA.MainFeatures.SyntaxHighlighting {
    internal class SyntaxHighlight {

        #region real colorization todo
        /*
        /// <summary>
        /// Called on STYLENEEDED notification
        /// </summary>
        /// <param name="endPos"></param>
        public static void Colorize(int startPos, int endPos) {
            //------------
            var watch = Stopwatch.StartNew();
            //------------

            // redefine the styles
            SetGeneralStyles();

            Lexer tok = new Lexer(Npp.GetDocumentText());
            tok.Tokenize();
            SynthaxHighlightVisitor vis = new SynthaxHighlightVisitor {
                FromLine = Npp.LineFromPosition(startPos),
                ToLine = Npp.LineFromPosition(endPos)
            };
            tok.Accept(vis);

            //--------------
            watch.Stop();
            Npp.SetStatusbarLabel("derp = " + derp + "startPos = " + startPos + ", endPos = " + endPos + ", done in " + watch.ElapsedMilliseconds + " ms");
            //------------
            derp++;
        }
        */
        #endregion

    }
}
