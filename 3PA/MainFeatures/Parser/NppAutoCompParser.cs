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
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;

namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// This class "tokenize" the input data into tokens of various types,
    /// it implements a visitor pattern
    /// </summary>
    internal class NppAutoCompParser {

        #region private const

        private const char Eof = (char) 0;

        #endregion

        #region private fields

        private string _data;
        private int _dataLength;
        private int _pos;
        //private int _line = 0;
        //private int _column = 0;

        //private int _startCol;
        //private int _startLine;
        private int _startPos;

        // we could use a List here, but this GapBuffer class is more appropriate to do insertions
        private List<CompletionItem> _wordsList = new List<CompletionItem>();

        #endregion

        #region Public properties

        /// <summary>
        /// Additionnal characters that will count as a char from a word
        /// </summary>
        public Char[] AdditionnalCharacters { get; private set; }

        /// <summary>
        /// False to add the number parsed to the output list
        /// </summary>
        public bool IgnoreNumbers { get; private set; }

        /// <summary>
        /// Allows to have a unique list of words and not add twice the same
        /// </summary>
        public HashSet<string> KnownWords { get; private set; }

        #endregion

        #region public accessors

        /// <summary>
        /// Returns the tokens list
        /// </summary>
        public List<CompletionItem> ParsedCompletionItemsList {
            get { return _wordsList; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// call Tokenize() to do the work
        /// </summary>
        public NppAutoCompParser(string data, char[] additionnalCharacters, bool ignoreNumbers) {
            if (data == null)
                throw new ArgumentNullException("data");
            _data = data;
            _dataLength = _data.Length;

            IgnoreNumbers = ignoreNumbers;
            AdditionnalCharacters = additionnalCharacters;

            if (KnownWords == null)
                KnownWords = new HashSet<string>();

            // create the list of keywords
            Parse();

            // clean
            _data = null;
        }

        #endregion

        #region Parse

        /// <summary>
        /// Call this method to actually parse the string
        /// </summary>
        private void Parse() {
            while (GetNextCompItem()) {}
        }

        /// <summary>
        /// Is the char valid for a word
        /// </summary>
        private bool IsCharWord(char ch) {
            return char.IsLetter(ch) || ch == '_' || (AdditionnalCharacters != null && AdditionnalCharacters.Contains(ch));
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
            //_column++;
        }

        /// <summary>
        /// Read the End Of Line character (can read \r\n in one go), add it to the current token
        /// </summary>
        /// <param name="eol"></param>
        private void ReadEol(char eol = '\n') {
            ReadChr();
            if (eol == '\r' && PeekAtChr(0) == '\n')
                ReadChr();

            //_line++;
            //_column = ColumnStartAt;
        }

        private void ReadWhiteSpace() {
            ReadChr();
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == '\t' || ch == ' ')
                    ReadChr();
                else
                    break;
            }
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
        private bool GetNextCompItem() {
            //_startLine = _line;
            //_startCol = _column;
            _startPos = _pos;

            var ch = PeekAtChr(0);

            // END OF FILE reached
            if (ch == Eof)
                return false;

            switch (ch) {
                case ' ':
                case '\t':
                    // whitespaces or tab
                    ReadWhiteSpace();
                    return true;

                case '\r':
                case '\n':
                    // end of line
                    ReadEol(ch);
                    return true;

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
                    ReadNumber();

                    if (!IgnoreNumbers) {
                        var valnum = GetTokenValue();
                        if (!KnownWords.Contains(valnum)) {
                            KnownWords.Add(valnum);
                            _wordsList.Add(new CompletionItem {
                                DisplayText = valnum
                            });
                        }
                    }
                    return true;

                default:
                    // keyword = [a-Z_~]+[\w_-]*
                    if (IsCharWord(ch)) {
                        ReadWord();
                        var val = GetTokenValue();
                        if (!KnownWords.Contains(val)) {
                            KnownWords.Add(val);
                            _wordsList.Add(new CompletionItem {
                                DisplayText = val
                            });
                        }
                        return true;
                    }
                    // unknown char
                    ReadChr();
                    return true;
            }
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
        /// create a number token, accepts decimal value with a '.' and hexadecimal notation 0xNNN
        /// </summary>
        /// <returns></returns>
        private void ReadNumber() {
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
        }

        #endregion
    }
}