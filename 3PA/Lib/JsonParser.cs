#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (JsonParser.cs) is part of 3P.
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
using _3PA.MainFeatures.Parser;

namespace _3PA.Lib {

    /// <summary>
    /// TODO: This class is too spcific and must be refactored later... for now it will do
    /// </summary>
    internal class JsonParser {
        private const char Eof = (char)0;
        private string _data;
        private int _pos;
        private int _startPos;
        private int _tokenPos;
        private char[] _symbolChars;
        private List<Token> _tokenList = new List<Token>();

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// call Tokenize() to do the work
        /// </summary>
        /// <param name="data"></param>
        public JsonParser(string data) {
            if (data == null)
                throw new ArgumentNullException("data");
            _data = data;
            _pos = 0;
            _tokenPos = 0;
            _symbolChars = new[] { '[', ']', '{', '}', ',', ':' };
        }

        /// <summary>
        /// Call this method to actually tokenize the string
        /// </summary>
        public void Tokenize() {
            Token token;
            do {
                token = GetNext();
                _tokenList.Add(token);
            } while (!(token is TokenEof));
        }

        /// <summary>
        /// Returns a List of list of key/value pairs...
        /// </summary>
        /// <returns></returns>
        public List<List<Tuple<string, string>>> GetList() {
            var releaseJsonontent = new List<List<Tuple<string, string>>> { new List<Tuple<string, string>>() };
            var outerI = 0;
            var bracketCount = 0;
            Token token;
            do {
                _tokenPos++;
                token = PeekAtToken(0);
                if (token is TokenSymbol) {
                    if (token.Value.Equals("{"))
                        bracketCount++;
                    if (token.Value.Equals("}")) {
                        bracketCount--;
                        if (bracketCount == 0) {
                            outerI++;
                            releaseJsonontent.Add(new List<Tuple<string, string>>());
                        }
                    }
                }
                if (token is TokenWord) {

                    if (PeekAtToken(1).Value.Equals(":")) {
                        var nextWordPos = -1;
                        if (PeekAtToken(2) is TokenWhiteSpace && PeekAtToken(3) is TokenWord)
                            nextWordPos = 3;
                        else if (PeekAtToken(2) is TokenWord)
                            nextWordPos = 2;
                        if (nextWordPos > 0) {
                            var varName = token.Value;
                            if (varName[0] == '"')
                                varName = varName.Substring(1, varName.Length - 2);
                            var varValue = PeekAtToken(nextWordPos).Value;
                            if (varValue[0] == '"')
                                varValue = varValue.Substring(1, varValue.Length - 2);
                            releaseJsonontent[outerI].Add(new Tuple<string, string>(varName, varValue));
                            _tokenPos = _tokenPos + nextWordPos;
                        }
                    }
                }
            } while (!(token is TokenEof));
            if (outerI == 0 && releaseJsonontent[outerI].Count == 0)
                return null;
            return releaseJsonontent;
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
            _startPos = _pos;

            var ch = PeekAt(0);

            // END OF FILE reached
            if (ch == Eof)
                return new TokenEof(GetTokenValue(), 0, 0, _startPos, _pos);

            switch (ch) {
                case '"':
                    ReadString(ch);
                    return new TokenWord(GetTokenValue(), 0, 0, _startPos, _pos);

                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    // whitespaces or tab
                    return CreateWhitespaceToken();

                default:
                    // keyword
                    if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-') {
                        ReadWord();
                        return new TokenWord(GetTokenValue(), 0, 0, _startPos, _pos);
                    }

                    // symbol
                    if (_symbolChars.Any(t => t == ch))
                        return CreateSymbolToken();
                    // unknown char
                    Read();
                    return new TokenUnknown(GetTokenValue(), 0, 0, _startPos, _pos);
            }
        }

        private Token CreateSymbolToken() {
            Read();
            return new TokenSymbol(GetTokenValue(), 0, 0, _startPos, _pos);
        }

        /// <summary>
        /// create a whitespace token (successions of either ' ' or '\t')
        /// </summary>
        /// <returns></returns>
        private Token CreateWhitespaceToken() {
            Read();
            while (true) {
                var ch = PeekAt(0);
                if (ch == '\t' || ch == ' ' || ch == '\r' || ch == '\n')
                    Read();
                else
                    break;
            }
            return new TokenWhiteSpace(GetTokenValue(), 0, 0, _startPos, _pos);
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
        /// reads a string " "
        /// </summary>
        /// <param name="strChar"></param>
        private void ReadString(char strChar) {
            Read();
            while (true) {
                var ch = PeekAt(0);
                if (ch == Eof)
                    break;
                // quote char
                if (ch == strChar) {
                    Read();
                    break; // done reading
                }
                // escape char (read anything as part of the string after that)
                if (ch == '\\')
                    Read();
                Read();
            }
        }
    }

}
