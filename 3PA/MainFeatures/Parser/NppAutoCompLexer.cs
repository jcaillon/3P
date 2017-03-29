#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppAutoCompParser.cs) is part of 3P.
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

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class "tokenize" the input data into tokens of various types,
    /// it implements a visitor pattern
    /// </summary>
    internal class NppAutoCompLexer {

        #region private const

        private const int LineStartAt = 0;
        private const int ColumnStartAt = 0;
        private const char Eof = (char)0;

        #endregion

        #region private fields

        private string _data;
        private int _dataLength;
        private int _pos;
        private int _line = LineStartAt;
        private int _column = ColumnStartAt;

        private int _startCol;
        private int _startLine;
        private int _startPos;
        
        // we could use a List here, but this List class is more appropriate to do insertions
        private List<Token> _tokenList = new List<Token>();

        #endregion

        #region public accessor

        /// <summary>
        /// Additional characters that will count as a char from a word
        /// </summary>
        public HashSet<char> AdditionnalCharacters { get; set; }

        /// <summary>
        /// returns the last line number found, must be called after Tokenize() method
        /// </summary>
        public int MaxLine {
            get { return _line; }
        }

        /// <summary>
        /// Returns the tokens list
        /// </summary>
        public List<Token> GetTokensList {
            get { return _tokenList; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// call Tokenize() to do the work
        /// </summary>
        public NppAutoCompLexer(string data) {
            if (data == null)
                throw new ArgumentNullException("data");
            _data = data;
            _dataLength = _data.Length;

            // create the list of tokens
            Tokenize();

            // clean
            _data = null;
        }

        #endregion

        #region Tokenize

        /// <summary>
        /// Call this method to actually tokenize the string
        /// </summary>
        private void Tokenize() {
            Token token;
            do {
                token = GetNextToken();
                _tokenList.Add(token);
            } while (!(token is TokenEof));
        }

        /// <summary>
        /// Peek forward x chars
        /// </summary>
        private char PeekAtChr(int x) {
            return _pos + x >= _dataLength ? Eof : _data[_pos + x];
        }

        /// <summary>
        /// Read to the next char,
        /// indirectly adding the current char (_data[_pos]) to the current token
        /// </summary>
        private void ReadChr() {
            _pos++;
            _column++;
        }

        /// <summary>
        /// Read the End Of Line character (can read \r\n in one go), add it to the current token
        /// </summary>
        /// <param name="eol"></param>
        private void ReadEol(char eol = '\n') {
            ReadChr();
            if (eol == '\r' && PeekAtChr(0) == '\n')
                ReadChr();
            _line++;
            _column = ColumnStartAt;
        }

        /// <summary>
        /// Returns the current value of the token
        /// </summary>
        /// <returns></returns>
        private string GetTokenValue() {
            return _data.Substring(_startPos, _pos - _startPos);
        }

        /// <summary>
        /// Is the char valid for a word
        /// </summary>
        private bool IsCharWord(char ch) {
            return char.IsLetterOrDigit(ch) || ch == '_' || AdditionnalCharacters != null && AdditionnalCharacters.Contains(ch);
        }

        /// <summary>
        /// returns the next token of the string
        /// </summary>
        /// <returns></returns>
        private Token GetNextToken() {
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
                        ReadWord();
                        return new TokenWord(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
                    }
                    // unknown char
                    ReadChr();
                    return new TokenUnknown(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
            }
        }

        private Token CreateEolToken(char ch) {
            ReadEol(ch);
            return new TokenEol(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// reads a word with this format : .[\w_-]*((\.[\w_-~]*)?){1,}
        /// returns true if a ~ is used in the word
        /// </summary>
        private void ReadWord() {
            ReadChr();
            while (true) {
                var ch = PeekAtChr(0);

                // normal word
                if (IsCharWord(ch)) {
                    ReadChr();
                    continue;
                }
                break;
            }
        }
        
        /// <summary>
        /// create a whitespace token (successions of either ' ' or '\t')
        /// </summary>
        /// <returns></returns>
        private Token CreateWhitespaceToken() {
            ReadChr();
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == '\t' || ch == ' ')
                    ReadChr();
                else
                    break;
            }
            return new TokenWhiteSpace(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// create a number token, accepts decimal value with a '.' and hexadecimal notation 0xNNN
        /// </summary>
        /// <returns></returns>
        private Token CreateNumberToken() {
            var hasPoint = false;
            var isHexa = false;

            // we can also read hexadecimal notation (i.e. 0xNNN)
            if (PeekAtChr(0) == '0' && PeekAtChr(1) == 'x') {
                ReadChr();
                isHexa = true;
                hasPoint = true; // don't allow to have a point
            }
            ReadChr();

            while (true) {
                var ch = PeekAtChr(0);
                if (char.IsDigit(ch))
                    ReadChr();
                // for hexadecimal numbers, we don't check if the number is correct, we just read letters and digits...
                else if (isHexa && char.IsLetter(ch))
                    ReadChr();
                else if (ch == '.' && !hasPoint && char.IsDigit(PeekAtChr(1))) {
                    hasPoint = true;
                    ReadChr();
                } else
                    break;
            }
            return new TokenNumber(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }
        
        #endregion
    
    }
}