#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProLexer.cs) is part of 3P.
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

using _3PA.Lib;
using _3PA.NppCore;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// This class "tokenize" the input data into tokens of various types,
    /// it implements a visitor pattern
    /// </summary>
    internal class ProLexer : Lexer {

        #region private fields

        private char[] _symbolChars = {'=', '+', '-', '/', ',', '.', '*', '~', '!', '@', '#', '$', '%', '^', '&', '(', ')', '{', '}', '[', ']', ':', ';', '<', '>', '?', '|', '\\', '`', '’'};

        private int _commentDepth;
        private int _includeDepth;

        // specific to progress, preprocess defined var can contain a ' or " but in that case,
        // the string ends at the end of the line no matter what. So we keep track on which line 
        // the the last preprocessed var was
        private int _definePreProcLastLine = -2;
        // line of the last ~ symbol
        private int _tildeLastLine = -2;

        #endregion

        #region public accessor

        /// <summary>
        /// Returns the tokens list, we use a gap buffer because it costs less to insert/remove in the middle of the list, 
        /// which we do in the parser
        /// </summary>
        public new GapBuffer<Token> GetTokensList {
            get { return (GapBuffer<Token>) _tokenList; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        public ProLexer(string data) {
            _tokenList = new GapBuffer<Token>();
            Construct(data);
        }

        /// <summary>
        /// Use this when you wish to tokenize only a partial string in a longer string
        /// Allows you to start with a comment depth different of 0
        /// </summary>
        public ProLexer(string data, int pos, int line, int column, int commentDepth, int includeDepth) : this(data) {
            _pos = pos;
            _line = line;
            _column = column;
            _commentDepth = commentDepth;
            _includeDepth = includeDepth;
        }

        #endregion

        #region Tokenize
        
        /// <summary>
        /// Read the End Of Line character (can read \r\n in one go), add it to the current token
        /// </summary>
        /// <param name="eol"></param>
        protected override void ReadEol(char eol = '\n') {
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
        /// Is the char valid for a word
        /// </summary>
        protected override bool IsCharWord(char ch) {
            return char.IsLetterOrDigit(ch) || ch == '_' || ch == '-';
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
                    _includeDepth++;
                    return CreatePreprocessedToken();

                case '}':
                    // end of include
                    if (_includeDepth > 0)
                        _includeDepth--;
                    return CreateSymbolToken();

                case '&':
                    // pre-processed directive (i.e. &define, &analyse-suspend, &message)
                    _definePreProcLastLine = _startLine;
                    return CreatePreProcDirectiveToken();

                case ' ':
                case '\t':
                    // whitespaces or tab
                    return CreateWhitespaceToken();

                case '-':
                case '+':
                    // number
                    if (!char.IsDigit(PeekAtChr(1)))
                        return CreateSymbolToken();
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
                    if (IsCharWord(ch)) {
                        return CreateWordToken();
                    }

                    // symbol
                    if (_symbolChars.Contains(ch))
                        return CreateSymbolToken();

                    // unknown char
                    return CreateUnknownToken();
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

        /// <summary>
        /// reads a word with this format : .[\w_-]*((\.[\w_-~]*)?){1,}
        /// </summary>
        protected override Token CreateWordToken() {
            return new TokenWord(ReadWord() ? GetTokenValue().Replace("~", "").Replace("\n", "").Replace("\r", "") : GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        protected Token CreatePreProcDirectiveToken() {
            return new TokenPreProcDirective(ReadWord() ? GetTokenValue().Replace("~", "").Replace("\n", "").Replace("\r", "") : GetTokenValue(), _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// reads '{' for an include and '{&' for a preprocessed variable
        /// </summary>
        protected Token CreatePreprocessedToken() {
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
        protected Token CreateCommentToken() {
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
        protected Token CreateSingleLineCommentToken() {
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
        /// reads a quotes string (either simple of double quote), takes into account escape char ~
        /// </summary>
        /// <returns></returns>
        protected override Token CreateStringToken(char strChar) {
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
                    // BUT we don't want to do that when we are defining parameters for an include...
                    if (_definePreProcLastLine == _startLine && _includeDepth == 0)
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