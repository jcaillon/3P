#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (SyntaxHighlightVisitor.cs) is part of 3P.
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
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.SyntaxHighlighting {
    internal class SyntaxHighlightVisitor : ILexerVisitor {
        /// <summary>
        /// Only colorize from this line!
        /// </summary>
        public int FromLine { get; set; }

        public int ToLine { get; set; }

        public void Visit(TokenComment tok) {
            //if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Comment, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenEol tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenEos tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenInclude tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenPreProcVariable tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenNumber tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenString tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.String, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenStringDescriptor tok) {
            throw new NotImplementedException();
        }

        public void Visit(TokenSymbol tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenEof tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenWord tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenWhiteSpace tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenUnknown tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenPreProcDirective tok) {}

        public void PreVisit(Lexer lexer) {
        }

        public void PostVisit() {
        }
    }
}