#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System;
using System.Collections.Generic;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;
using Lexer = _3PA.NppCore.Lexer;

namespace _3PA.MainFeatures.SyntaxHighlighting {

    internal class SyntaxHighlight {

        /// <summary>
        /// Is the caret not in : an include, a string, a comment
        /// </summary>
        public static bool IsCarretInNormalContext(int curPos) {
            try {
                var curContext = (SciStyleId)Sci.GetStyleAt(curPos);
                if (curPos <= 0)
                    return true;
                if (IsNormalContext(curContext))
                    return true;
                var prevContext = (SciStyleId)Sci.GetStyleAt(curPos - 1);
                return IsNormalContext(prevContext);
            } catch (Exception) {
                // we can be here if the style ID isn't in the UdlStyles enum
                return true;
            }
        }

        /// <summary>
        /// Is the caret not in : a string, a comment
        /// </summary>
        private static bool IsNormalContext(SciStyleId context) {
            return context != SciStyleId.Comment
                   && context != SciStyleId.DoubleQuote
                   && context != SciStyleId.SimpleQuote
                   && context != SciStyleId.SingleLineComment;
        }

        /// <summary>
        /// Called on STYLENEEDED notification
        /// </summary>
        public static void Colorize(int startPos, int endPos) {
            var startLine = Sci.LineFromPosition(startPos);
            var startLinePos = Sci.GetLine(startLine).Position;
            var startingLineInfo = _lineInfo.ContainsKey(startLine) ? _lineInfo[startLine] : new LexerLineInfo(0, 0, false, false);

            ProLexer tok = new ProLexer(Sci.GetTextByRange(startLinePos, endPos), startLinePos, startLine, 0, startingLineInfo.CommentDepth, startingLineInfo.IncludeDepth, startingLineInfo.InDoubleQuoteString, startingLineInfo.InSimpleQuoteString, PushLineInfo);

            SyntaxHighlightVisitor vis = new SyntaxHighlightVisitor();
            vis.PreVisit(tok);
            tok.Accept(vis);
            vis.PostVisit();
        }

        /// <summary>
        /// Set scintilla to use the contained lexer
        /// </summary>
        public static void ActivateHighlight() {
            Sci.Lexer = Lexer.Container;
            Sci.SetProperty("fold", "1");

            // Configure a margin to display folding symbols
            Sci.GetMargin(2).Type = MarginType.Symbol;
            Sci.GetMargin(2).Mask = Sci.Marker.MaskFolders;
            Sci.GetMargin(2).Sensitive = true;
            Sci.GetMargin(2).Width = 20;

            // Enable folding
            Sci.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;
            
            _lineInfo.Clear();
            Sci.Colorize(0, -1);
        }

        /// <summary>
        /// Allows to keeps track of certain info at the beginning of each line
        /// </summary>
        private static void PushLineInfo(int line, int commentDepth, int includeDepth, bool inDoubleQuoteString, bool inSimpleQuoteString) {
            if (_lineInfo.ContainsKey(line))
                _lineInfo[line] = new LexerLineInfo(commentDepth, includeDepth, inDoubleQuoteString, inSimpleQuoteString);
            else
                _lineInfo.Add(line, new LexerLineInfo(commentDepth, includeDepth, inDoubleQuoteString, inSimpleQuoteString));
        }
        
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