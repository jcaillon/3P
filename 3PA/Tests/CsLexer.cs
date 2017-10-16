using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using _3PA.NppCore;

namespace _3PA.Tests {

    public static class TestLexer {

        private static CSharpLexer cSharpLexer = new CSharpLexer("class const int namespace partial public static string using void");

        public static void Style(int startPos, int endPos) {
            cSharpLexer.Style(startPos, endPos);
        }

    }

    public class CSharpLexer {

        public const int StyleDefault = 0;
        public const int StyleKeyword = 1;
        public const int StyleIdentifier = 2;
        public const int StyleNumber = 3;
        public const int StyleString = 4;

        private const int STATE_UNKNOWN = 0;
        private const int STATE_IDENTIFIER = 1;
        private const int STATE_NUMBER = 2;
        private const int STATE_STRING = 3;

        private HashSet<string> keywords;

        public void Style(int startPos, int endPos) {
            // Back up to the line start
            var line = Sci.LineFromPosition(startPos);
            startPos = Sci.GetLine(line).Position;

            var length = 0;
            var state = STATE_UNKNOWN;

            // Start styling
            Sci.StartStyling(startPos);
            var txt = Sci.Text;

            while (startPos < endPos) {
                var c = (char)txt[startPos];

                REPROCESS:
                switch (state) {
                    case STATE_UNKNOWN:
                        if (c == '"') {
                            // Start of "string"
                            Sci.SetStyling(1, StyleString);
                            state = STATE_STRING;
                        } else if (Char.IsDigit(c)) {
                            state = STATE_NUMBER;
                            goto REPROCESS;
                        } else if (Char.IsLetter(c)) {
                            state = STATE_IDENTIFIER;
                            goto REPROCESS;
                        } else {
                            // Everything else
                            Sci.SetStyling(1, StyleDefault);
                        }
                        break;

                    case STATE_STRING:
                        if (c == '"') {
                            length++;
                            Sci.SetStyling(length, StyleString);
                            length = 0;
                            state = STATE_UNKNOWN;
                        } else {
                            length++;
                        }
                        break;

                    case STATE_NUMBER:
                        if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == 'x') {
                            length++;
                        } else {
                            Sci.SetStyling(length, StyleNumber);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        break;

                    case STATE_IDENTIFIER:
                        if (Char.IsLetterOrDigit(c)) {
                            length++;
                        } else {
                            var style = StyleIdentifier;
                            var identifier = Sci.GetTextRange(startPos - length, length);
                            if (keywords.Contains(identifier))
                                style = StyleKeyword;

                            Sci.SetStyling(length, style);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        break;
                }

                startPos++;
            }
        }

        public CSharpLexer(string keywords) {
            // Put keywords in a HashSet
            var list = Regex.Split(keywords ?? string.Empty, @"\s+").Where(l => !string.IsNullOrEmpty(l));
            this.keywords = new HashSet<string>(list);
        }
    }
}
