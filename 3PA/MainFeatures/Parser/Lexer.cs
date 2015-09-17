using System;
using System.Linq;

namespace _3PA.MainFeatures.Parser {

    /// <summary>
    /// Token object
    /// </summary>
    public class Token {
        public string Value { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int StartPosition { get; private set; }
        public int EndPosition { get; private set; }
        public TokenType Type { get; private set; }
        public Token(TokenType type, string value, int line, int column, int startPosition, int endPosition) {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
    }

    /// <summary>
    /// Token types...
    /// Eos is end of statement (a .)
    /// Eof is end of file
    /// Eol is end of line (\r\n or \n)
    /// QuotedString is either a simple or double quote string (handles ~ escape char)
    /// Symbol is a single char
    /// </summary>
    public enum TokenType {
        Comment,
        Eos,
        Unknown,
        Word,
        Number,
        QuotedString,
        WhiteSpace,
        Symbol,
        Eol,
        Eof,
    }

    /// <summary>
    /// This class "tokenize" the input data into tokens of various types
    /// </summary>
    public class Lexer {
        private const char Eof = (char) 0;
        private int _commentDepth;
        private int _column;
        private string _data;
        private int _line;
        private int _pos;
        private int _startCol;
        private int _startLine;
        private int _startPos;
        private char[] _symbolChars;

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// use GetNext() to get the next token, stop when token.type == Eof
        /// </summary>
        /// <param name="data"></param>
        public Lexer(string data) {
            if (data == null)
                throw new ArgumentNullException("data");
            _data = data;
            _symbolChars = new[] { '=', '+', '-', '/', ',', '.', '*', '~', '!', '@', '#', '$', '%', '^', '&', '(', ')', '{', '}', '[', ']', ':', ';', '<', '>', '?', '|', '\\', '`', '’' };
            _line = 1;
            _column = 1;
            _pos = 0;
        }

        /// <summary>
        /// Use this when you wish to tokenize only a partial string in a longer string
        /// Allows you to start with a comment depth different of 0
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="commentDepth"></param>
        public void SettingInitialStatus(int line, int column, int commentDepth) {
            _line = line;
            _column = column;
            _commentDepth = commentDepth;
        }

        /// <summary>
        /// Peek forward
        /// </summary>
        private char PeekAt(int count) {
            return _pos + count >= _data.Length ? Eof : _data[_pos + count];
        }

        /// <summary>
        /// Read to the next char,
        /// indirectly adding the current char (_data[_pos]) to the current token
        /// </summary>
        /// <returns></returns>
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
            _column = 1;
        }

        /// <summary>
        /// instanciate token object
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Token CreateToken(TokenType type) {
            var tokenData = _data.Substring(_startPos, _pos - _startPos);
            return new Token(type, tokenData, _startLine, _startCol, _startPos, _pos);
        }

        /// <summary>
        /// returns the next token of the string
        /// </summary>
        /// <returns></returns>
        public Token GetNext() {
            _startLine = _line;
            _startCol = _column;
            _startPos = _pos;

            // if we started in a comment, read this token as a comment
            if (_commentDepth > 0) return CreateCommentToken();

            var ch = PeekAt(0);
            switch (ch) {
                case Eof:
                    return CreateToken(TokenType.Eof);
                // comment
                case '/':
                    return PeekAt(1) == '*' ? CreateCommentToken() : CreateSymbolToken();
                // whitespaces or tab
                case ' ':
                case '\t':
                    return CreateWhitespaceToken();
                // number
                case '-': {
                    if (!char.IsDigit(PeekAt(1))) return CreateSymbolToken();
                    Read();
                    return CreateNumberToken();
                }
                case '+': {
                    if (!char.IsDigit(PeekAt(1))) return CreateSymbolToken();
                    Read();
                    return CreateNumberToken();
                }
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
                    return CreateNumberToken();
                // end of line
                case '\r':
                case '\n':
                    return CreateEolToken(ch);
                // quoted string (read until unescaped ' or ")
                case '"':
                case '\'':
                    return CreateStringToken(ch);
                // end of statement
                case '.':
                    return (char.IsWhiteSpace(PeekAt(1)) || PeekAt(1) == Eof) ? CreateEosToken() : CreateSymbolToken();
                default: {
                    // keyword = [a-Z_&]+[\w_-]*
                    if (char.IsLetter(ch) || ch == '_' || ch == '&')
                        return CreateWordToken();
                    // symbol
                    if (_symbolChars.Any(t => t == ch))
                        return CreateSymbolToken();
                    // unknown char
                    Read();
                    return CreateToken(TokenType.Unknown);
                }
            }
        }

        private Token CreateEolToken(char ch) {
            ReadEol(ch);
            return CreateToken(TokenType.Eol);
        }

        private Token CreateEosToken() {
            Read();
            return CreateToken(TokenType.Eos);
        }

        private Token CreateSymbolToken() {
            Read();
            return CreateToken(TokenType.Symbol);
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
                } else switch (ch) {
                    // read eol
                    case '\r':
                    case '\n':
                        ReadEol(ch);
                        break;
                    // continue reading
                    default:
                        Read();
                        break;
                }
            }
            return CreateToken(TokenType.Comment);
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
            return CreateToken(TokenType.WhiteSpace);
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
            return CreateToken(TokenType.Number);
        }

        /// <summary>
        /// reads a word with this format : [a-Z_&]+[\w_-]*
        /// </summary>
        private Token CreateWordToken() {
            Read();
            while (true) {
                var ch = PeekAt(0);
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')
                    Read();
                else
                    break;
            }
            return CreateToken(TokenType.Word);
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
                // break on new line (this means the code is not compilable anyway...)
                if (ch == '\r' || ch == '\n')
                    break;
                // escape char
                if (ch == '~') {
                    Read();
                    // if the following char is a quote, it was escaped so read it
                    if (PeekAt(0) == strChar)
                        Read();
                // quote char
                } else if (ch == strChar) {
                    Read();
                    break; // done reading
                // keep on reading
                } else
                    Read();
            }
            return CreateToken(TokenType.QuotedString);
        }
    }
}