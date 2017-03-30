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
using _3PA.MainFeatures.Parser;

namespace _3PA.Lib {

    /// <summary>
    /// Quick and dirty class to read json
    /// </summary>
    internal class JsonParser : Lexer {

        private char[] _symbolChars;

        /// <summary>
        /// constructor, data is the input string to tokenize
        /// call Tokenize() to do the work
        /// </summary>
        /// <param name="data"></param>
        public JsonParser(string data) {
            _pos = 0;
            _tokenPos = 0;
            _symbolChars = new[] {'[', ']', '{', '}', ',', ':'};
            Construct(data);
        }
        
        /// <summary>
        /// Returns a List of list of key/value pairs...
        /// </summary>
        /// <returns></returns>
        public List<List<Tuple<string, string>>> GetList() {
            var releaseJsonontent = new List<List<Tuple<string, string>>> {new List<Tuple<string, string>>()};
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

            switch (ch) {
                case '"':
                    ReadString(ch);
                    return new TokenWord(GetTokenValue(), _startLine, _startCol, _startPos, _pos);

                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    // whitespaces or tab
                    return CreateWhitespaceToken();

                default:
                    // keyword
                    if (IsCharWord(ch)) {
                        ReadWord();
                        return new TokenWord(GetTokenValue(), _startLine, _startCol, _startPos, _pos);
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
        /// reads a word with this format : [a-Z_&]+[\w_-]*((\.[\w_-]*)?){1,}
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
        /// reads a string " "
        /// </summary>
        /// <param name="strChar"></param>
        private void ReadString(char strChar) {
            ReadChr();
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == Eof)
                    break;
                // quote char
                if (ch == strChar) {
                    ReadChr();
                    break; // done reading
                }
                // escape char (read anything as part of the string after that)
                if (ch == '\\')
                    ReadChr();
                ReadChr();
            }
        }
    }
}