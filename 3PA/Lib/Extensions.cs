#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (Extensions.cs) is part of 3P.
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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MarkdownDeep;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    public static class Extensions {

        public static List<T> ToNonNullList<T>(this IEnumerable<T> obj) {
            return obj == null ? new List<T>() : obj.ToList();
        }

        #region misc
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

        /// <summary>
        /// Use : var name = player.GetAttributeFrom<DisplayAttribute>("PlayerDescription").Name;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetAttributeFrom<T>(this object instance, string propertyName) where T : Attribute {
            var attrType = typeof(T);
            var property = instance.GetType().GetProperty(propertyName);
            return (T)property.GetCustomAttributes(attrType, false).First();
        }

        /// <summary>
        /// Returns true if the bit at the given position is set to true
        /// </summary>
        public static bool IsBitSet(this int b, int pos) {
            return (b & (1 << pos)) != 0;
        }

        /// <summary>
        /// Returns true if the bit at the given position is set to true
        /// </summary>
        public static bool IsBitSet(this uint b, int pos) {
            return (b & (1 << pos)) != 0;
        }

        #endregion


        #region ui thread safe invoke

        /* http://www.codeproject.com/Articles/52752/Updating-Your-Form-from-Another-Thread-without-Cre */

        public static TResult SafeInvoke<T, TResult>(this T isi, Func<T, TResult> call) where T : ISynchronizeInvoke {
            if (isi.InvokeRequired) {
                IAsyncResult result = isi.BeginInvoke(call, new object[] {isi});
                object endResult = isi.EndInvoke(result);
                return (TResult) endResult;
            }
            return call(isi);
        }

        public static void SafeInvoke<T>(this T isi, Action<T> call) where T : ISynchronizeInvoke {
            if (isi.InvokeRequired) isi.BeginInvoke(call, new object[] {isi});
            else
                call(isi);
        }

        #endregion


        #region Enum extensions

        //flags |= flag;// SetFlag
        //flags &= ~flag; // ClearFlag 

        /// <summary>
        /// Allows to describe a field of an enum like this :
        /// [DescriptionAttribute(Value = "DATA-SOURCE")]
        /// and use the value "Value"
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class EnumAttr : Attribute {}

        public static EnumAttr GetAttributes(this Enum value) {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            var atts = (EnumAttr[]) fieldInfo.GetCustomAttributes(typeof (EnumAttr), false);
            return atts.Length > 0 ? atts[0] : null;
        }

        #endregion


        #region string extensions

        /// <summary>
        /// Returns the html link representation from a url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="urlName"></param>
        /// <returns></returns>
        public static string ToHtmlLink(this string url, string urlName = null) {
            return string.Format("<a href='{0}'>{1}</a>", url, urlName ?? url);
        }

        /// <summary>
        /// Transforms an md formatted string into an html text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string MdToHtml(this string text) {
            var md = new Markdown();
            return md.ConvertToHtml(text);
        }

        /// <summary>
        /// Replaces every forbidden char (forbidden for a filename) in the text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToValidFileName(this string text) {
            return Path.GetInvalidFileNameChars().Aggregate(text, (current, c) => current.Replace(c, '-'));
        }

        /// <summary>
        /// Replaces " by ~" and add extra " at the start and end of the string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ProgressQuoter(this string text) {
            return "\"" + (text ?? "").Replace("\"", "~\"") + "\"";
        }

        /// <summary>
        /// Delete every trailing \r or\n
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string TrimEol(this string text) {
            return text.TrimEnd('\r', '\n');
        }

        /// <summary>
        /// Breaks new lines every lineLength char, taking into account words to not
        /// split them
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lineLength"></param>
        /// <param name="eolString"></param>
        /// <returns></returns>
        public static string BreakText(this string text, int lineLength, string eolString = "\n") {
            var charCount = 0;
            var lines = text.Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(w => (charCount += w.Length + 1) / lineLength)
                .Select(g => string.Join(" ", g));
            return string.Join(eolString, lines.ToArray());
        }

        /// <summary>
        /// Compares two version string "1.0.0.0".IsHigherVersionThan("0.9") returns true
        /// Must be STRICTLY superior
        /// </summary>
        /// <param name="localVersion"></param>
        /// <param name="distantVersion"></param>
        /// <returns></returns>
        public static bool IsHigherVersionThan(this string localVersion, string distantVersion) {
            var splitLocal = (localVersion.StartsWith("v") ? localVersion.Remove(0, 1) : localVersion).Split('.');
            var splitDistant = (distantVersion.StartsWith("v") ? distantVersion.Remove(0, 1) : distantVersion).Split('.');
            try {
                var i = 0;
                while (i <= (splitLocal.Length - 1) && i <= (splitDistant.Length - 1)) {
                    if (int.Parse(splitLocal[i]) > int.Parse(splitDistant[i]))
                        return true;
                    i++;
                }
            } catch (Exception x) {
                ErrorHandler.DirtyLog(x);
            }
            return false;
        }

        /// <summary>
        /// Check if word contains at least one letter
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static bool ContainsAtLeastOneLetter(this string word) {
            var max = word.Length - 1;
            int count = 0;
            while (count <= max) {
                if (char.IsLetter(word[count]))
                    return true;
                count++;
            }
            return false;
        }

        /// <summary>
        /// autocase the keyword in input according to the user config
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static string AutoCaseToUserLiking(this string keyword) {
            switch (Config.Instance.CodeChangeCaseMode) {
                case 1:
                    return keyword.ToUpper();
                case 2:
                    return keyword.ToLower();
                case 3:
                    return keyword.ToTitleCase();
                default:
                    return keyword;
            }
        }

        /// <summary>
        /// Count the nb of occurrences...
        /// </summary>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <returns></returns>
        public static int CountOccurences(this string haystack, string needle) {
            return (haystack.Length - haystack.Replace(needle, "").Length) / needle.Length;
        }

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
        /// Returns the dispersion level of a input string compared to a filter
        /// </summary>
        public static int DispersionLevel(this string input, string filter) {
            int dispersion = 0;
            if (string.IsNullOrEmpty(filter)) return dispersion;
            int pos = 0;
            int posFilter = 0;
            while (pos < input.Length) {
                // we match the current char of the filter
                if (char.ToLower(input[pos]) == filter[posFilter]) {
                    posFilter++;
                    // we matched the entire filter
                    if (posFilter >= filter.Length)
                        return dispersion;
                } else {
                    // gap between match mean more penalty than finding the match later in the string
                    if (posFilter > 0) {
                        dispersion += 900;
                    } else {
                        dispersion += 30;
                    }
                }
                pos++;
            }
            return dispersion;
        }

        /// <summary>
        /// Equivalent to Equals but case insensitive
        /// </summary>
        /// <param name="s"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool EqualsCi(this string s, string comp) {
            //string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
            return s.Equals(comp, StringComparison.CurrentCultureIgnoreCase); 
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
        /// Converts from ANSI
        /// </summary>
        public static string AnsiToXencode(this string str, Encoding xencode) {
            return xencode.GetString(Encoding.Default.GetBytes(str));
        }

        /// <summary>
        /// Converts to ANSI
        /// </summary>
        public static string XencodeToAnsi(this string str, Encoding xencode) {
            return Encoding.Default.GetString(xencode.GetBytes(str));
        }

        /// <summary>
        /// case insensitive contains
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        public static bool ContainsFast(this string source, string toCheck) {
            return source.IndexOf(toCheck, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        #endregion


        #region region string misc

        private static readonly string[] LineDelimiters = { "\r\n", "\n" };

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

        public static bool IsOneOf(this char ch, params char[] patterns) {
            return patterns.Any(c => c == ch);
        }

        public static bool IsNonWhitespaceNext(this string text, string pattern, int startPos) {
            if (startPos < text.Length)
                for (int i = startPos; i < text.Length; i++) {
                    if (!char.IsWhiteSpace(text[i]))
                        return (text.IndexOf(pattern, i, StringComparison.CurrentCultureIgnoreCase) == i);
                }
            return false;
        }

        public static int GetByteCount(this string text) {
            return Encoding.Default.GetByteCount(text);
        }

        public static int GetUtf8ByteCount(this string text) {
            return Encoding.UTF8.GetByteCount(text);
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
            }
            return false;
        }

        public static bool EndsWith(this StringBuilder builder, string pattern) {
            if (builder.Length >= pattern.Length) {
                for (int i = 0; i < pattern.Length; i++)
                    if (pattern[i] != builder[builder.Length - pattern.Length + i])
                        return false;
                return true;
            }
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
            }
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
            }
            return false;
        }

        public static bool EndsWithWhiteSpacesLine(this StringBuilder builder) {
            if (builder.Length > 0) {
                for (int i = builder.Length - 1; i >= 0 && builder[i] != '\n'; i--)
                    if (!char.IsWhiteSpace(builder[i]))
                        return false;
                return true;
            }
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
            }
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
