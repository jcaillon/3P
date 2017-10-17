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

using System.Collections.Generic;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;
using Lexer = _3PA.NppCore.Lexer;

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

            var startLine = Sci.LineFromPosition(startPos);
            var startLinePos = Sci.GetLine(startLine).Position;
            var startingLineInfo = _lineInfo.ContainsKey(startLine) ? _lineInfo[startLine] : new LexerLineInfo(0, 0, false, false);
            _lineInfo.Clear();

            Sci.StartStyling(startLinePos);

            ProLexer tok = new ProLexer(Sci.GetTextByRange(startLinePos, endPos), startLinePos, startLine, 0, startingLineInfo.CommentDepth, startingLineInfo.IncludeDepth, startingLineInfo.InDoubleQuoteString, startingLineInfo.InSimpleQuoteString, PushLineInfo);
            SyntaxHighlightVisitor vis = new SyntaxHighlightVisitor();
            vis.PreVisit(tok);
            tok.Accept(vis);
            vis.PostVisit();


            //--------------
            //watch.Stop();
            //UserCommunication.Notify("startPos = " + startPos + ", endPos = " + endPos + ", done in " + watch.ElapsedMilliseconds + " ms");
            //------------


        }

        public static void ActivateHighlight() {
            Sci.Lexer = Lexer.Container;
            _lineInfo.Clear();
            Sci.Colorize(0, -1);
        }

        private static void PushLineInfo(int line, int commentDepth, int includeDepth, bool inDoubleQuoteString, bool inSimpleQuoteString) {
            _lineInfo.Add(line, new LexerLineInfo(commentDepth, includeDepth, inDoubleQuoteString, inSimpleQuoteString));
        }

        #endregion

        private static Dictionary<int, LexerLineInfo> _lineInfo = new Dictionary<int, LexerLineInfo>();

        internal class LexerLineInfo {

            public int CommentDepth { get; set; }
            public int IncludeDepth { get; set; }
            public bool InDoubleQuoteString { get; set; }
            public bool InSimpleQuoteString { get; set; }

            public LexerLineInfo(int commentDepth, int includeDepth, bool inDoubleQuoteString, bool inSimpleQuoteString) {
                CommentDepth = commentDepth;
                IncludeDepth = includeDepth;
                InDoubleQuoteString = inDoubleQuoteString;
                InSimpleQuoteString = inSimpleQuoteString;
            }
        }

    }
}