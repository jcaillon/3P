#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppLexer.cs) is part of 3P.
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

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class "tokenize" the input data into tokens of various types,
    /// it implements a visitor pattern
    /// </summary>
    internal class TextLexer : Lexer {

        #region public accessor

        /// <summary>
        /// Additional characters that will count as a char from a word
        /// </summary>
        public HashSet<char> AdditionnalCharacters { get; private set; }

        #endregion
        
        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        public TextLexer(string data, HashSet<char> additionnalCharacters) {
            AdditionnalCharacters = additionnalCharacters;
            Construct(data);
        }

        #endregion

        #region Tokenize
        
        /// <summary>
        /// Is the char valid for a word
        /// </summary>
        protected override bool IsCharWord(char ch) {
            return char.IsLetterOrDigit(ch) || ch == '_' || AdditionnalCharacters != null && AdditionnalCharacters.Contains(ch);
        }

        /// <summary>
        /// returns the next token of the string
        /// </summary>
        /// <returns></returns>
        protected override Token GetNextToken() {
            _startLine = _line;
            _startCol = _column;
            _startPos = _pos;

            var ch = PeekAtChr(0);

            // END OF FILE reached
            if (ch == Eof)
                return new TokenEof(GetTokenValue(), _startLine, _startCol, _startPos, _pos);

            switch (ch) {
                case ' ':
                case '\t':
                    // whitespaces or tab
                    return CreateWhitespaceToken();

                case '\r':
                case '\n':
                    // end of line
                    return CreateEolToken(ch);

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    // number
                    return CreateNumberToken();

                default:
                    // keyword = [a-Z_~]+[\w_-]*
                    if (IsCharWord(ch)) {
                        return CreateWordToken();
                    }
                    // unknown char
                    return CreateUnknownToken();
            }
        }
        
        #endregion
    
    }
}