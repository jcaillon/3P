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
using System.Linq;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// This class "tokenize" the input data into tokens of various types,
    /// it implements a visitor pattern
    /// </summary>
    internal class Lexer {
        #region private const

        private const int LineStartAt = 0;
        private const int ColumnStartAt = 0;
        private const char Eof = (char) 0;

        #endregion

        #region private fields

        private char[] _symbolChars = {'=', '+', '-', '/', ',', '.', '*', '~', '!', '@', '#', '$', '%', '^', '&', '(', ')', '{', '}', '[', ']', ':', ';', '<', '>', '?', '|', '\\', '`', '’'};

        private string _data;
        private int _dataLength;
        private int _pos;
        private int _line = LineStartAt;
        private int _column = ColumnStartAt;
        private int _commentDepth;

        private int _startCol;
        private int _startLine;
        private int _startPos;

        private int _tokenPos = -1;

        // we could use a List here, but this GapBuffer class is more appropriate to do insertions
        private GapBuffer<Token> _tokenList = new GapBuffer<Token>();

        // specific to progress, preprocess defined var can contain a ' or " but in that case,
        // the string ends at the end of the line no matter what. So we keep track on which line 
        // the the last preprocessed var was
        private int _definePreProcLastLine = -2;
        // line of the last ~ symbol
        private int _tildeLastLine = -2;

        #endregion

        #region public accessor

        /// <summary>
        /// returns the last line number found, must be called after Tokenize() method
        /// </summary>
        public int MaxLine {
            get { return _line; }
        }

        /// <summary>
        /// Returns the tokens list
        /// </summary>
        public GapBuffer<Token> GetTokensList {
            get { return _tokenList; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// call Tokenize() to do the work
        /// </summary>
        /// <param name="data"></param>
        public Lexer(string data) {
            if (data == null)
                throw new ArgumentNullException("data");
            _data = data;
            _dataLength = _data.Length;

            // create the list of tokens
            Tokenize();

            // clean
            _data = null;
        }

        /// <summary>
        /// Use this when you wish to tokenize only a partial string in a longer string
        /// Allows you to start with a comment depth different of 0
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="commentDepth"></param>
        public Lexer(string data, int pos, int line, int column, int commentDepth) : this(data) {
            _pos = pos;
            _line = line;
            _column = column;
            _commentDepth = commentDepth;
        }

        #endregion

        #region Visitor/browser

        /// <summary>
        /// Feed this method with a visitor implementing ILexerVisitor to visit all the tokens of the input string
        /// (you must call the Tokenize() methode before that!)
        /// </summary>
        /// <param name="visitor"></param>
        public void Accept(ILexerVisitor visitor) {
            foreach (Token token in _tokenList) {
                token.Accept(visitor);
            }
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// Move to the next token, return true if it can
        /// </summary>
        public bool MoveNextToken() {
            return ++_tokenPos < _tokenList.Count;
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// peek at the current pos + x token of the list, returns a new TokenEof if can't find
        /// </summary>
        public Token PeekAtToken(int x) {
            return (_tokenPos + x >= _tokenList.Count || _tokenPos + x < 0) ? new TokenEof("", _startLine, _startCol, _startPos, _pos) : _tokenList[_tokenPos + x];
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
        /// peek backward x chars
        /// </summary>
        private char PeekAtChrReverse(int x) {
            return _pos - x < 0 ? Eof : _data[_pos - x];
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

            // small exception for progress, to be able to interprete line like :
            // &scope-define varname l'appel~\r\nest bon " 
            // make the scope define line virtually continue on the next line
            if (_startLine == _tildeLastLine)
                _definePreProcLastLine++;

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

            // if we started in a comment, read this token as a comment
            if (_commentDepth > 0)
                return CreateCommentToken();

            switch (ch) {
                case '~':
                    _tildeLastLine = _startLine;
                    return CreateSymbolToken();

                case '/':
                    var nextChar = PeekAtChr(1);
                    // comment
                    if (nextChar == '*')
                        return CreateCommentToken();
                    // single line comment (if previous char is a whitespace)
                    if (nextChar == '/' && char.IsWhiteSpace(PeekAtChrReverse(1)))
                        return CreateSingleLineCommentToken();
                    // symbol
                    return CreateSymbolToken();

                case '{':
                    // case of a preprocessed {&variable}/{1} or an include
                    return CreatePreprocessedToken();

                case '&':
                    // pre-processed directive (i.e. &define, &analyse-suspend, &message)
                    _definePreProcLastLine = _startLine;
                    return new TokenPreProcDirective(
                        ReadWord() ? GetTokenValue().Replace("~", "").Replace("\n", "").Replace("\r", "") : GetTokenValue(),
                        _startLine, _startCol, _startPos, _pos);

                case ' ':
                case '\t':
                    // whitespaces or tab
                    return CreateWhitespaceToken();

                case '-':
                case '+':
                    // number
                    if (!char.IsDigit(PeekAtChr(1))) return CreateSymbolToken();
                    ReadChr();
                    return CreateNumberToken();

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

                case '\r':
                case '\n':
                    // end of line
                    return CreateEolToken(ch);

                case '"':
                case '\'':
                    // quoted string (read until unescaped ' or ")
                    return CreateStringToken(ch);

                case ':':
                    // EOS (if followed by any space/new line char)
                    if (char.IsWhiteSpace(PeekAtChr(1)))
                        return CreateEosToken();
                    // String descriptor?
                    var count = _tokenList.Count;
                    if (count > 0 && _tokenList[count - 1] is TokenString)
                        return CreateStringDescriptorToken();
                    // or a badly placed symbol
                    return CreateSymbolToken();

                case '.':
                    return (char.IsWhiteSpace(PeekAtChr(1)) || PeekAtChr(1) == Eof) ? CreateEosToken() : CreateSymbolToken();

                default:
                    // keyword = [a-Z_~]+[\w_-]*
                    if (char.IsLetter(ch) || ch == '_' || ch == '~') {
                        return new TokenWord(
                            ReadWord() ? GetTokenValue().Replace("~", "").Replace("\n", "").Replace("\r", "") : GetTokenValue(),
                            _startLine, _startCol, _startPos, _pos);
                    }
                    // symbol
                    if (_symbolChars.Contains(ch))
                        return CreateSymbolToken();
                    // unknown char
                    ReadChr();
                    return new TokenUnknown(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
            }
        }

        /// <summary>
        /// reads a word with this format : .[\w_-]*((\.[\w_-~]*)?){1,}
        /// returns true if a ~ is used in the word
        /// </summary>
        private bool ReadWord() {
            bool readTilde = false;

            if (PeekAtChr(0) == '~') {
                ReadTildeAndEol();
                readTilde = true;
            } else
                ReadChr();

            while (true) {
                var ch = PeekAtChr(0);

                // normal word
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-') {
                    ReadChr();
                    continue;
                }

                // reads a base.table.field as a single word
                if (ch == '.') {
                    var car = PeekAtChr(1);
                    if (char.IsLetterOrDigit(car) || car == '_' || car == '-') {
                        ReadChr();
                        continue;
                    }
                }

                // escape char (read anything as part of the string after that)
                if (ch == '~') {
                    ReadTildeAndEol();
                    readTilde = true;
                    continue;
                }

                break;
            }

            return readTilde;
        }

        private void ReadTildeAndEol() {
            // escape char (read anything as part of the string after that)
            ReadChr();
            var ch = PeekAtChr(0);
            if (ch == '\r' || ch == '\n')
                ReadEol(ch);
        }

        private Token CreateEolToken(char ch) {
            ReadEol(ch);
            return new TokenEol(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        private Token CreateEosToken() {
            ReadChr();
            return new TokenEos(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        private Token CreateSymbolToken() {
            ReadChr();
            return new TokenSymbol(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// reads '{' for an include and '{&' for a preprocessed variable
        /// </summary>
        private Token CreatePreprocessedToken() {
            ReadChr();
            var ch = PeekAtChr(0);

            // case of a preprocessed {&variable} or {1}
            if (ch == '&' || char.IsDigit(ch)) {
                if (ch == '&')
                    ReadChr();
                return new TokenPreProcVariable(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
            }

            // include file
            return new TokenInclude(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// create a comment token, take into account nested comment,
        /// reads until all comments tags are closed
        /// </summary>
        /// <returns></returns>
        private Token CreateCommentToken() {
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == Eof)
                    break;
                // we read a comment opening
                if (ch == '/' && PeekAtChr(1) == '*') {
                    ReadChr();
                    ReadChr();
                    _commentDepth++;
                }
                // we read a comment closing
                else if (ch == '*' && PeekAtChr(1) == '/') {
                    ReadChr();
                    ReadChr();
                    _commentDepth--;
                    // we finished reading the comment, leave
                    if (_commentDepth == 0)
                        break;
                    // read eol
                } else if (ch == '\r' || ch == '\n') {
                    ReadEol(ch);
                    // continue reading
                } else
                    ReadChr();
            }
            return new TokenComment(GetTokenValue(), _startLine, _startCol, _startPos, _pos, false);
        }

        /// <summary>
        /// create a comment token, for a single line comment
        /// </summary>
        /// <returns></returns>
        private Token CreateSingleLineCommentToken() {
            // read until the end of the line
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == Eof || ch == '\r' || ch == '\n')
                    break;
                ReadChr();
            }
            return new TokenComment(GetTokenValue(), _startLine, _startCol, _startPos, _pos, true);
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

        /// <summary>
        /// reads a quotes string (either simple of double quote), takes into account escape char ~
        /// </summary>
        /// <returns></returns>
        private Token CreateStringToken(char strChar) {
            ReadChr();
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == Eof)
                    break;

                // escape char (read anything as part of the string after that)
                if (ch == '~') {
                    ReadChr(); // read tilde
                    var nextCh = PeekAtChr(0);
                    if (nextCh == '\r' || nextCh == '\n') {
                        ReadEol(nextCh);
                    } else
                        ReadChr();
                    continue;
                }

                // new line
                if (ch == '\r' || ch == '\n') {
                    // a string continues at the next line... Except when it's on a &define line
                    if (_definePreProcLastLine == _startLine)
                        break;

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

        /// <summary>
        /// A character-string in progress can be described with different properties :
        /// "characters" [ : [ R | L | C | T ] [ U ] [ max-length ] ]
        /// </summary>
        /// <returns></returns>
        private Token CreateStringDescriptorToken() {
            ReadChr();
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == Eof)
                    break;

                // we don't care if the descriptor is valid or not, just read while it's a letter or digit
                if (!char.IsLetterOrDigit(ch))
                    break;
                ReadChr();
            }
            return new TokenStringDescriptor(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        #endregion
    }
}