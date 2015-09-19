using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace _3PA.Lib {

    public static class Extensions {

        public static List<T> ToNonNullList<T>(this IEnumerable<T> obj) {
            return obj == null ? new List<T>() : obj.ToList();
        }

        #region " misc "
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items) {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> items, T itemToFind) {
            int retVal = 0;
            foreach (var item in items) {
                if (item.Equals(itemToFind)) return retVal;
                retVal++;
            }
            return -1;
        }

        #endregion

        #region " string extensions "
        /// <summary>
        /// returns a list of CharacterRange representing the matches found with the given filter
        /// applied to the string
        /// It works like the text matching of resharper autocompletion...
        /// WARNING : CASE SENSITIVE!!!
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<CharacterRange> FindAllMatchedRanges(this string input, string filter) {
            var ranges = new List<CharacterRange>();
            if (string.IsNullOrEmpty(filter)) return ranges;
            int pos = 0;
            int posFilter = 0;
            bool matching = false;
            int startMatch = 0;
            while (pos < input.Length) {
                // remember matching state at the beginning of the loop
                bool wasMatching = matching;
                // we match the current char of the filter
                if (input[pos] == filter[posFilter]) {
                    if (!matching) {
                        matching = true;
                        startMatch = pos;
                    }
                    posFilter++;
                    // we matched the entire filter
                    if (posFilter >= filter.Length) {
                        ranges.Add(new CharacterRange(startMatch, pos - startMatch + 1));
                        break;
                    }
                } else
                    matching = false;
                // we stopped matching, remember matching range
                if (!matching && wasMatching)
                    ranges.Add(new CharacterRange(startMatch, pos - startMatch));
                pos++;
            }
            // we reached the end of the input, if we were matching stuff, remember matching range
            if (pos >= input.Length && matching)
                ranges.Add(new CharacterRange(startMatch, pos - 1 - startMatch));
            return ranges;
        }

        /// <summary>
        /// Tests if the string contains the given filter, uses the text matching of resharper autocompletion (like)
        /// WARNING : CASE SENSITIVE!!!
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static bool FullyMatchFilter(this string input, string filter) {
            if (string.IsNullOrEmpty(filter)) return true;
            int pos = 0;
            int posFilter = 0;
            while (pos < input.Length) {
                // we match the current char of the filter
                if (char.ToLower(input[pos]) == filter[posFilter]) {
                    posFilter++;
                    // we matched the entire filter
                    if (posFilter >= filter.Length)
                        return true;
                }
                pos++;
            }
            return false;
        }

        /// <summary>
        /// Equivalent to Equals but case insensitive
        /// </summary>
        /// <param name="s"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool EqualsCi(this string s, string comp) {
            return s.Equals(comp, StringComparison.OrdinalIgnoreCase); 
        }
         

        /// <summary>
        /// convert the word to Title Case
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string s) {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower()); 
        }

        /// <summary>
        ///     Converts and ANSI string to Unicode.
        /// </summary>
        public static string AnsiToUnicode(this string str) {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(str));
        }

        /// <summary>
        ///     Converts a Unicode string to ANSI
        /// </summary>
        public static string UnicodeToAnsi(this string str) {
            return Encoding.Default.GetString(Encoding.UTF8.GetBytes(str));
        }

        private static readonly string[] LineDelimiters = new string[] { "\r\n", "\n" };

        /// <summary>
        /// Normalizes the line breaks by replacing a single-"\n" breaks with "\r\n".
        /// </summary>
        /// <param name="text">The text to be normalized.</param>
        /// <returns></returns>
        public static string NormalizeLineBreaks(this string text) {
            return text == null ? null : text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }


        public static int MatchingStartChars(this string text, string pattern, bool ignoreCase = false) {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
                return 0;

            if (ignoreCase) {
                text = text.ToLower();
                pattern = pattern.ToLower();
            }

            for (int i = 0; i < pattern.Length && i < text.Length; i++) {
                if (text[i] != pattern[i])
                    return i;
            }
            return Math.Min(pattern.Length, text.Length);
        }

        public static string TruncateLines(this string text, int maxLineCount, string truncationPrompt) {
            if (!string.IsNullOrEmpty(text)) {
                string[] lines = text.Split(LineDelimiters, maxLineCount + 1, StringSplitOptions.None);

                if (lines.Count() > maxLineCount)
                    return string.Join("\n", lines.Take(maxLineCount)) + "\n" + truncationPrompt;
            }
            return text;
        }

        //http://www.softcircuits.com/Blog/post/2010/01/10/Implementing-Word-Wrap-in-C.aspx
        public static string WordWrap(this string text, int width) {
            int pos, next;
            StringBuilder sb = new StringBuilder();

            // Lucidity check
            if (width < 1)
                return text;
            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next) {
                // Find end of line
                int eol = text.IndexOf(Environment.NewLine, pos, StringComparison.Ordinal);
                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + Environment.NewLine.Length;
                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos) {
                    do {
                        int len = eol - pos;
                        if (len > width)
                            len = BreakLine(text, pos, width);
                        sb.Append(text, pos, len);
                        sb.Append(Environment.NewLine);
                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && Char.IsWhiteSpace(text[pos]))
                            pos++;
                    } while (eol > pos);
                } else sb.Append(Environment.NewLine); // Empty line
            }
            return sb.ToString();
        }

        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        public static int BreakLine(this string text, int pos, int max) {
            // Find last whitespace in line
            int i = max - 1;
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool IsScriptFile(this string file) {
            return file.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) ||
                   file.EndsWith(".csx", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsToken(this string text, string pattern, int position) {
            if (position < text.Length) {
                int endPos = position;
                for (; endPos < text.Length; endPos++)
                    if (char.IsWhiteSpace(text[endPos])) {
                        break;
                    }

                int startPos = position - 1;
                for (; startPos >= 0; startPos--)
                    if (char.IsWhiteSpace(text[startPos])) {
                        startPos = startPos + 1;
                        break;
                    }

                if (startPos == -1)
                    startPos = 0;

                if ((endPos - startPos) == pattern.Length)
                    return (text.IndexOf(pattern, startPos, StringComparison.Ordinal) == startPos);
            }
            return false;
        }

        public static bool IsOneOf(this char ch, params char[] patterns) {
            foreach (char c in patterns)
                if (c == ch)
                    return true;
            return false;
        }

        public static bool IsNonWhitespaceNext(this string text, string pattern, int startPos) {
            if (startPos < text.Length)
                for (int i = startPos; i < text.Length; i++) {
                    if (!char.IsWhiteSpace(text[i]))
                        return (text.IndexOf(pattern, i, StringComparison.Ordinal) == i);
                }
            return false;
        }

        public static int GetByteCount(this string text) {
            return Encoding.Default.GetByteCount(text);
        }

        public static int GetUtf8ByteCount(this string text) {
            return Encoding.UTF8.GetByteCount(text);
        }

        public static bool IsControlStatement(this string text) {
            text = text.TrimEnd();

            if (text.EndsWith(")")) {
                if (Regex.Match(text, @"\s*foreach\s*\(").Success)
                    return true;
                else if (Regex.Match(text, @"\s*for\s*\(").Success)
                    return true;
                else if (Regex.Match(text, @"\s*while\s*\(").Success)
                    return true;
                else if (Regex.Match(text, @"\s*if\s*\(").Success)
                    return true;
                //else if (Regex.Match(text, @"\s*else\s*\(").Success)
                //    return true;
            }

            return false;
        }

        public static bool IsInlineElseIf(this string text) {
            text = text.TrimEnd();

            if (text.EndsWith(")")) {
                if (Regex.Match(text, @"\s*else\s*if \s*\(").Success)
                    return text.EndsWith("}") || text.EndsWith(";");
            }

            return false;
        }

        public static StringBuilder Append(this StringBuilder builder, string text, int count) {
            for (int i = 0; i < count; i++)
                builder.Append(text);
            return builder;
        }

        public static string MultiplyBy(this string text, int count) {
            string retval = "";
            for (int i = 0; i < count; i++)
                retval += text;
            return retval;
        }

        public static bool IsSameLine(this StringBuilder builder, int startPos, int endPos) {
            if (builder.Length > startPos && builder.Length > endPos) {
                for (int i = startPos; i <= endPos; i++)
                    if (builder[i] == '\n')
                        return false;
                return true;
            } else
                return false;
        }

        public static bool EndsWith(this StringBuilder builder, string pattern) {
            if (builder.Length >= pattern.Length) {
                for (int i = 0; i < pattern.Length; i++)
                    if (pattern[i] != builder[builder.Length - pattern.Length + i])
                        return false;
                return true;
            } else
                return false;
        }

        public static bool EndsWithEscapeChar(this StringBuilder builder, char escapeChar) {
            if (builder.Length > 0) {
                int matchCount = 0;
                for (int i = builder.Length - 1; i >= 0; i--) {
                    if (builder[i] == escapeChar)
                        matchCount++;
                    else
                        break;
                }

                return matchCount % 2 != 0;
            } else
                return false;
        }

        //public static bool EndsWith(this StringBuilder builder, params char[] patterns)
        //{
        //    if (builder.Length > 0)
        //    {
        //        char endChar = builder[builder.Length - 1];

        //        foreach(char c in patterns)
        //            if (c == endChar)
        //                return false;
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        public static bool ContainsAt(this StringBuilder builder, string pattern, int pos) {
            if ((builder.Length - pos) >= pattern.Length) {
                for (int i = 0; i < pattern.Length; i++)
                    if (pattern[i] != builder[pos + i])
                        return false;
                return true;
            } else
                return false;
        }

        public static bool EndsWithWhiteSpacesLine(this StringBuilder builder) {
            if (builder.Length > 0) {
                for (int i = builder.Length - 1; i >= 0 && builder[i] != '\n'; i--)
                    if (!char.IsWhiteSpace(builder[i]))
                        return false;
                return true;
            } else
                return false;
        }

        public static string GetLastLine(this StringBuilder builder) {
            return builder.GetLineFrom(builder.Length - 1);
        }

        public static char LastChar(this StringBuilder builder) {
            return builder[builder.Length - 1];
        }

        public static string GetLineFrom(this StringBuilder builder, int position) {
            if (position == (builder.Length - 1) && builder[position] == '\n')
                return "";

            if (builder.Length > 0 && position < builder.Length) {
                int lineEnd = position;
                for (; lineEnd < builder.Length; lineEnd++) {
                    if (builder[lineEnd] == '\n') {
                        lineEnd -= Environment.NewLine.Length - 1;
                        break;
                    }
                }

                int lineStart = position - 1;
                for (; lineStart >= 0; lineStart--)
                    if (builder[lineStart] == '\n') {
                        lineStart = lineStart + 1;
                        break;
                    }

                if (lineStart == -1)
                    lineStart = 0;

                var chars = new char[lineEnd - lineStart];

                builder.CopyTo(lineStart, chars, 0, chars.Length);
                return new string(chars);
            } else
                return null;
        }

        public static StringBuilder TrimEmptyEndLines(this StringBuilder builder, int maxLineToLeave = 1) {
            int lastNonWs = builder.LastNonWhiteSpace();

            if (lastNonWs == -1)
                builder.Length = 0; //the whole content was empty lines only
            else {
                int count = 0;
                int maxLineBreak = maxLineToLeave + 1;

                for (int i = lastNonWs + 1; i < builder.Length; i++) {
                    if (builder.ContainsAt(Environment.NewLine, i))
                        count++;
                    if (count > maxLineBreak) {
                        builder.Length = i;
                        break;
                    }
                }
            }
            return builder;
        }

        public static int LastNonWhiteSpace(this StringBuilder builder) {
            for (int i = builder.Length - 1; i >= 0; i--)
                if (!char.IsWhiteSpace(builder[i]))
                    return i;
            return -1;
        }

        public static bool IsLastWhiteSpace(this StringBuilder builder) {
            if (builder.Length != 0)
                return char.IsWhiteSpace(builder[builder.Length - 1]);
            return false;
        }

        public static bool LastNonWhiteSpaceToken(this StringBuilder builder, string expected) {
            int pos = builder.LastNonWhiteSpace();

            if (pos != -1 && pos >= expected.Length) {
                int startPos = pos - (expected.Length - 1);
                for (int i = 0; i < expected.Length; i++) {
                    if (expected[i] != builder[startPos + i])
                        return false;
                }

                if (startPos == 0 || char.IsWhiteSpace(builder[startPos - 1]))
                    return true;
            }

            return false;
        }

        public static StringBuilder TrimEnd(this StringBuilder builder) {
            if (builder.Length > 0) {
                int i;
                for (i = builder.Length - 1; i >= 0; i--)
                    if (!char.IsWhiteSpace(builder[i]))
                        break;

                builder.Length = i + 1;
            }
            return builder;
        }

        public static StringBuilder TrimLineEnd(this StringBuilder builder) {
            if (builder.Length > 0) {
                int i;
                for (i = builder.Length - 1; i >= 0 && builder[i] != '\n'; i--)
                    if (!char.IsWhiteSpace(builder[i]))
                        break;

                builder.Length = i + 1;
            }
            return builder;
        }

        #endregion
    }
}
