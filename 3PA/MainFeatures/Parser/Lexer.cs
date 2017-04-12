#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Lexer.cs) is part of 3P.
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
    internal class Lexer {

        #region private const

        protected const int LineStartAt = 0;
        protected const int ColumnStartAt = 0;
        protected const char Eof = (char) 0;

        #endregion

        #region private fields

        protected string _data;
        protected int _dataLength;
        protected int _pos;
        protected int _line = LineStartAt;
        protected int _column = ColumnStartAt;

        protected int _startCol;
        protected int _startLine;
        protected int _startPos;

        protected int _tokenPos = -1;
        
        protected IList<Token> _tokenList;

        #endregion

        #region public accessor

        /// <summary>
        /// returns the last line number found
        /// </summary>
        public int MaxLine {
            get { return _line; }
        }

        /// <summary>
        /// Returns the tokens list
        /// </summary>
        public IList<Token> GetTokensList {
            get { return _tokenList; }
        }

        #endregion

        #region Constructor

        public Lexer() { }

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// call Tokenize() to do the work
        /// </summary>
        public Lexer(string data) {
            Construct(data);
        }

        protected void Construct(string data) {
            if (data == null)
                throw new ArgumentNullException("data");

            _data = data;
            _dataLength = _data.Length;

            Tokenize();
        }

        #endregion

        #region Visitor/browser

        /// <summary>
        /// Feed this method with a visitor implementing ILexerVisitor to visit all the tokens of the input string
        /// (you must call the Tokenize() methode before that!)
        /// </summary>
        /// <param name="visitor"></param>
        public virtual void Accept(ILexerVisitor visitor) {
            visitor.PreVisit(this);
            foreach (Token token in _tokenList) {
                token.Accept(visitor);
            }
            visitor.PostVisit();
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// Move to the next token, return true if it can
        /// </summary>
        public virtual bool MoveNextToken() {
            return ++_tokenPos < _tokenList.Count;
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// peek at the current pos + x token of the list, returns a new TokenEof if can't find
        /// </summary>
        public virtual Token PeekAtToken(int x) {
            return _tokenPos + x >= _tokenList.Count || _tokenPos + x < 0 ? new TokenEof("", _startLine, _startCol, _startPos, _pos) : _tokenList[_tokenPos + x];
        }

        #endregion

        #region Tokenize

        /// <summary>
        /// Call this method to actually tokenize the string
        /// </summary>
        protected void Tokenize() {
            if (_data == null)
                return;

            if (_tokenList == null)
                _tokenList = new List<Token>();

            Token token;
            do {
                token = GetNextToken();
                _tokenList.Add(token);
            } while (!(token is TokenEof));

            // clean
            _data = null;
        }

        /// <summary>
        /// Peek forward x chars
        /// </summary>
        protected char PeekAtChr(int x) {
            return _pos + x >= _dataLength ? Eof : _data[_pos + x];
        }

        /// <summary>
        /// peek backward x chars
        /// </summary>
        protected char PeekAtChrReverse(int x) {
            return _pos - x < 0 ? Eof : _data[_pos - x];
        }

        /// <summary>
        /// Read to the next char,
        /// indirectly adding the current char (_data[_pos]) to the current token
        /// </summary>
        protected void ReadChr() {
            _pos++;
            _column++;
        }

        /// <summary>
        /// Read the End Of Line character (can read \r\n in one go), add it to the current token
        /// </summary>
        /// <param name="eol"></param>
        protected virtual void ReadEol(char eol = '\n') {
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
        protected string GetTokenValue() {
            return _data.Substring(_startPos, _pos - _startPos);
        }

        /// <summary>
        /// Is the char valid for a word
        /// </summary>
        protected virtual bool IsCharWord(char ch) {
            return char.IsLetterOrDigit(ch) || ch == '_';
        }

        /// <summary>
        /// returns the next token of the string
        /// </summary>
        /// <returns></returns>
        protected virtual Token GetNextToken() {
            _startLine = _line;
            _startCol = _column;
            _startPos = _pos;

            ReadChr();

            return new TokenUnknown(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }
        
        protected virtual Token CreateEolToken(char ch) {
            ReadEol(ch);
            return new TokenEol(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        protected virtual Token CreateUnknownToken() {
            ReadChr();
            return new TokenUnknown(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        protected virtual Token CreateEosToken() {
            ReadChr();
            return new TokenEos(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        protected virtual Token CreateSymbolToken() {
            ReadChr();
            return new TokenSymbol(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// Read a word
        /// </summary>
        protected virtual Token CreateWordToken() {
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
            return new TokenWord(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// create a whitespace token (successions of either ' ' or '\t')
        /// </summary>
        protected virtual Token CreateWhitespaceToken() {
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
        /// create a number token, accepts decimal value with a '.' or ',' and hexadecimal notation 0xNNN
        /// </summary>
        protected virtual Token CreateNumberToken() {
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
                else if ((ch == '.' || ch == ',') && !hasPoint && char.IsDigit(PeekAtChr(1))) {
                    hasPoint = true;
                    ReadChr();
                } else
                    break;
            }
            return new TokenNumber(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// reads a quotes string (either simple of double quote), takes into account escape char ~
        /// </summary>
        protected virtual Token CreateStringToken(char strChar) {
            ReadChr();
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == Eof)
                    break;

                // new line
                if (ch == '\r' || ch == '\n') {
                    ReadEol(ch);
                    continue;
                }
                // quote char
                if (ch == strChar) {
                    ReadChr();
                    break; // done reading
                    // keep on reading
                }

                ReadChr();
            }
            return new TokenString(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        #endregion
    }
}