#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Linq;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class "tokenize" the input data into tokens of various types,
    /// it implements a visitor pattern
    /// </summary>
    public class Lexer {
        private const int LineStartAt = 0;
        private const int ColumnStartAt = 0;
        private const char Eof = (char)0;
        private int _commentDepth;
        private int _includeDepth;
        private int _column;
        private string _data;
        private int _line;
        private int _pos;
        private int _startCol;
        private int _startLine;
        private int _startPos;
        private char[] _symbolChars;
        private int _tokenPos;
        private List<Token> _tokenList = new List<Token>();
        private bool _forceCreateEos;

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// call Tokenize() to do the work
        /// </summary>
        /// <param name="data"></param>
        public Lexer(string data) {
            if (data == null)
                throw new ArgumentNullException("data");
            _data = data;
            _symbolChars = new[] { '=', '+', '-', '/', ',', '.', '*', '~', '!', '@', '#', '$', '%', '^', '&', '(', ')', '{', '}', '[', ']', ':', ';', '<', '>', '?', '|', '\\', '`', '’' };
            Reset();
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
        /// <param name="includeDepth"></param>
        public Lexer(string data, int pos, int line, int column, int commentDepth, int includeDepth) : this(data) {
            _pos = pos;
            _line = line;
            _column = column;
            _commentDepth = commentDepth;
            _includeDepth = includeDepth;
        }

        /// <summary>
        /// Call this method to actually tokenize the string
        /// </summary>
        public void Tokenize() {
            Token token;
            do {
                token = GetNext();
                _tokenList.Add(token);

                // in certain cases, we want to add an extra end of statement token!
                if (_forceCreateEos)
                    _tokenList.Add(new TokenEos(string.Empty, _startLine, _startCol, _startPos, _pos));
            } while (!(token is TokenEof));
        }

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
        /// returns the last line number found, must be called after Tokenize() method
        /// </summary>
        public int MaxLine { get { return _line; } }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// Move to the next token, return true if it can
        /// </summary>
        /// <returns></returns>
        public bool MoveNextToken() {
            return ++_tokenPos < _tokenList.Count;
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// peek at the current pos + x token of the list, returns a new TokenEof if can't find
        /// </summary>
        /// <returns></returns>
        public Token PeekAtToken(int x) {
            return (_tokenPos + x >= _tokenList.Count || _tokenPos + x < 0) ? new TokenEof("", 0, 0, 0, 0) : _tokenList[_tokenPos + x];
        }

        /// <summary>
        /// Resets the cursors position
        /// </summary>
        public void Reset() {
            _pos = 0;
            _line = LineStartAt;
            _column = ColumnStartAt;
            _commentDepth = 0;
            _includeDepth = 0;
            _tokenPos = -1;
        }
        
        /// <summary>
        /// Peek forward x chars
        /// </summary>
        private char PeekAt(int x) {
            return _pos + x >= _data.Length ? Eof : _data[_pos + x];
        }

        /// <summary>
        /// Read to the next char,
        /// indirectly adding the current char (_data[_pos]) to the current token
        /// </summary>
        private void Read() {
            _pos++;
            _column++;
        }

        /// <summary>
        /// Read the End Of Line character (can read \r\n in one go), add it to the current token
        /// </summary>
        /// <param name="eol"></param>
        private void ReadEol(char eol = '\n') {
            Read();
            if (eol == '\r' && PeekAt(0) == '\n')
                Read();
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
        private Token GetNext() {
            _forceCreateEos = false;
            _startLine = _line;
            _startCol = _column;
            _startPos = _pos;

            var ch = PeekAt(0);

            // END OF FILE reached
            if (ch == Eof)
                return new TokenEof(GetTokenValue(), _startLine, _startCol, _startPos, _pos);

            // if we started in a comment, read this token as a comment
            if (_commentDepth > 0) return CreateCommentToken();

            // if we started in a comment, read this token as a comment
            if (_includeDepth > 0) return CreateIncludeToken();

            switch (ch) {
                case '/':
                    // comment
                    return PeekAt(1) == '*' ? CreateCommentToken() : CreateSymbolToken();
               
                case '{':
                    // include file or preproc variable
                    return CreateIncludeToken();
                
                case '&':
                    // pre-processed &define, &analyse-suspend, &message
                    // Read the word, try to match it with define statement
                    ReadWord();
                    var word = GetTokenValue().ToUpper();
                    switch (word) {
                        case "&ENDIF":
                        case "&THEN":
                        case "&ELSE":
                            _forceCreateEos = true;
                            break;
                    }
                    switch (word) {
                        case "&ANALYZE-SUSPEND":
                        case "&ANALYZE-RESUME":
                        case "&GLOBAL-DEFINE":
                        case "&SCOPED-DEFINE":
                        case "&SCOPED":
                        case "&GLOB":
                        case "&GLOBAL":
                        case "&MESSAGE":
                        case "&UNDEFINE":
                            _forceCreateEos = true;
                            return CreatePreProcessedStatement();
                    }
                    return new TokenWord(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
                
                case ' ':
                case '\t':
                    // whitespaces or tab
                    return CreateWhitespaceToken();
                
                case '-':
                    // number
                    if (!char.IsDigit(PeekAt(1))) return CreateSymbolToken();
                    Read();
                    return CreateNumberToken();
                case '+':
                    // number
                    if (!char.IsDigit(PeekAt(1))) return CreateSymbolToken();
                    Read();
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
                    // end of statement (: or . followed by any space/new line char)
                    return (char.IsWhiteSpace(PeekAt(1))) ? CreateEosToken() : CreateSymbolToken();
                case '.':
                    return (char.IsWhiteSpace(PeekAt(1)) || PeekAt(1) == Eof) ? CreateEosToken() : CreateSymbolToken();

                default:
                    // keyword = [a-Z_&]+[\w_-]*
                    if (char.IsLetter(ch) || ch == '_' || ch == '&') {
                        ReadWord();
                        var word2 = GetTokenValue().ToLower();
                        switch (word2) {
                            case "then":
                            case "else":
                                _forceCreateEos = true;
                                break;
                        }
                        return new TokenWord(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
                    }
                    // symbol
                    if (_symbolChars.Any(t => t == ch))
                        return CreateSymbolToken();
                    // unknown char
                    Read();
                    return new TokenUnknown(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
            }
        }

        private Token CreateEolToken(char ch) {
            ReadEol(ch);
            return new TokenEol(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        private Token CreateEosToken() {
            Read();
            return new TokenEos(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        private Token CreateSymbolToken() {
            Read();
            return new TokenSymbol(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// reads a preproc definition
        /// </summary>
        /// <returns></returns>
        private Token CreatePreProcessedStatement() {
            while (true) {
                var ch = PeekAt(0);
                if (ch == Eof)
                    break;
                // escape char (read anything as part of the string after that)
                if (ch == '~') {
                    Read();
                    ch = PeekAt(0);
                    if (ch == '\r' || ch == '\n')
                        ReadEol(ch);
                    continue;
                }
                // break on new line (this means the code is not compilable anyway...)
                if (ch == '\r' || ch == '\n')
                    break;
                Read();
            }
            return new TokenPreProcStatement(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// reads an include declaration
        /// </summary>
        private Token CreateIncludeToken() {
            //TODO: handle this case better?
            // if this is a file include, we assume there will be an end of statement in it
            // so we force one, otherwise we might not read the next line correctly
            if (PeekAt(1) != '&' && !char.IsDigit(PeekAt(1))) _forceCreateEos = true;
            while (true) {
                var ch = PeekAt(0);
                if (ch == Eof)
                    break;
                // start of include
                if (ch == '{') {
                    Read();
                    _includeDepth++;
                }
                // end of include
                else if (ch == '}') {
                    Read();
                    _includeDepth--;
                    // we finished reading
                    if (_includeDepth == 0)
                        break;
                } 
                // new line
                else if (ch == '\r' || ch == '\n')
                    ReadEol(ch);
                else 
                    Read();
            }
            return new TokenInclude(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// create a comment token, take into account nested comment,
        /// reads until all comments tags are closed
        /// </summary>
        /// <returns></returns>
        private Token CreateCommentToken() {
            while (true) {
                var ch = PeekAt(0);
                if (ch == Eof)
                    break;
                // we read a comment opening
                if (ch == '/' && PeekAt(1) == '*') {
                    Read();
                    Read();
                    _commentDepth++;
                }
                // we read a comment closing
                else if (ch == '*' && PeekAt(1) == '/') {
                    Read();
                    Read();
                    _commentDepth--;
                    // we finished reading the comment, leave
                    if (_commentDepth == 0)
                        break;
                // read eol
                } else if (ch == '\r' || ch == '\n') {
                    ReadEol(ch);
                // continue reading
                } else
                    Read();
            }
            return new TokenComment(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// create a whitespace token (successions of either ' ' or '\t')
        /// </summary>
        /// <returns></returns>
        private Token CreateWhitespaceToken() {
            Read();
            while (true) {
                var ch = PeekAt(0);
                if (ch == '\t' || ch == ' ')
                    Read();
                else
                    break;
            }
            return new TokenWhiteSpace(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// create a number token, accepts decimal value with a '.'
        /// </summary>
        /// <returns></returns>
        private Token CreateNumberToken() {
            var isDecimal = false;
            Read();
            while (true) {
                var ch = PeekAt(0);
                if (char.IsDigit(ch))
                    Read();
                else if (ch == '.' && !isDecimal && char.IsDigit(PeekAt(1))) {
                    isDecimal = true;
                    Read();
                } else
                    break;
            }
            return new TokenNumber(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// reads a word with this format : [a-Z_&]+[\w_-]*((\.[\w_-]*)?){1,}
        /// </summary>
        private void ReadWord() {
            Read();
            while (true) {
                var ch = PeekAt(0);
                // normal word
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')
                    Read();
                else {
                    // reads a base.table.field as a single word
                    if (ch == '.') {
                        var car = PeekAt(1);
                        if (char.IsLetterOrDigit(car) || car == '_' || car == '-') {
                            Read();
                            continue;
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// reads a quotes string (either simple of double quote), takes into account escape char ~
        /// </summary>
        /// <returns></returns>
        private Token CreateStringToken(char strChar) {
            Read();
            while (true) {
                var ch = PeekAt(0);
                if (ch == Eof)
                    break;
                // new line
                if (ch == '\r' || ch == '\n') {
                    ReadEol(ch);
                    continue;
                }
                // quote char
                if (ch == strChar) {
                    Read();
                    break; // done reading
                    // keep on reading
                }
                // escape char (read anything as part of the string after that)
                if (ch == '~')
                    Read();
                Read();
            }
            return new TokenQuotedString(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }
    }
}