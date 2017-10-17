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

        private int _includeDepth;

        public SyntaxHighlightVisitor(int includeDepth) {
            _includeDepth = includeDepth;
        }

        public void PreVisit(Lexer lexer) {
        }

        public void Visit(TokenComment tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Comment);
        }

        public void Visit(TokenEol tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Default);
        }

        public void Visit(TokenEos tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Default);
        }

        public void Visit(TokenInclude tok) {
            _includeDepth++;
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Delimiter3);
        }

        public void Visit(TokenPreProcVariable tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.KeyWordsList5);
        }

        public void Visit(TokenNumber tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Number);
        }

        public void Visit(TokenString tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Delimiter1);
        }

        public void Visit(TokenStringDescriptor tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Default);
        }

        public void Visit(TokenSymbol tok) {
            if (_includeDepth > 0 && tok.Value == "}") {
                _includeDepth--;
                SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Delimiter3);
            } else 
                SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Default);
        }

        public void Visit(TokenEof tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Default);
        }

        public void Visit(TokenWord tok) {
            if (_includeDepth > 0)
                SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Delimiter3);
            else
                SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.KeyWordsList2);
        }

        public void Visit(TokenWhiteSpace tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Default);
        }

        public void Visit(TokenUnknown tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.Default);
        }

        public void Visit(TokenPreProcDirective tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, UdlStyles.KeyWordsList5);
        }

        public void PostVisit() {
        }

        private void SetStyling(int length, UdlStyles style) {
            Sci.SetStyling(length, (int)style);
        }
    }
}