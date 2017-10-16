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
using _3PA.NppCore;
using Lexer = _3PA.MainFeatures.Parser.Lexer;

namespace _3PA.MainFeatures.SyntaxHighlighting {
    internal class SyntaxHighlightVisitor : ILexerVisitor {
        /// <summary>
        /// Only colorize from this line!
        /// </summary>
        public int FromLine { get; set; }

        public int ToLine { get; set; }

        public void Visit(TokenComment tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Comment);
        }

        public void Visit(TokenEol tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenEos tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenInclude tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Delimiter2);
        }

        public void Visit(TokenPreProcVariable tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenNumber tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenString tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Delimiter5);
        }

        public void Visit(TokenStringDescriptor tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenSymbol tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenEof tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenWord tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenWhiteSpace tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenUnknown tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int)UdlStyles.Default);
        }

        public void Visit(TokenPreProcDirective tok) {
            Sci.SetStyling(tok.EndPosition - tok.StartPosition, (int) UdlStyles.Default);
        }

        public void PreVisit(Lexer lexer) {
        }

        public void PostVisit() {
        }
    }
}