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
using YamuiFramework.Helper;

namespace _3PA.Lib {

    /// <summary>
    /// This class regroups all the extension methods
    /// </summary>
    public static class Extensions {

        #region object

        /// <summary>
        /// Use : var name = player.GetAttributeFrom DisplayAttribute>("PlayerDescription").Name;
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

        #endregion

        #region Enumerable

        /// <summary>
        /// Same as ToList but returns an empty list on Null instead of an exception
        /// </summary>
        public static List<T> ToNonNullList<T>(this IEnumerable<T> obj) {
            return obj == null ? new List<T>() : obj.ToList();
        }

        /// <summary>
        /// Find the index of the first element satisfaying the predicate
        /// </summary>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
            if (predicate == null) throw new ArgumentNullException("predicate");
            int retVal = 0;
            foreach (var item in items) {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        /// <summary>
        /// Find the index of the first element equals to itemToFind
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> items, T itemToFind) {
            int retVal = 0;
            foreach (var item in items) {
                if (item.Equals(itemToFind)) return retVal;
                retVal++;
            }
            return -1;
        }

        #endregion

        #region int

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

        #region Colors

        /// <summary>
        /// returns true if the color can be considered as dark
        /// </summary>
        public static bool IsColorDark(this Color color) {
            return color.GetBrightness() < 0.5;
        }

        #endregion

        #region ui thread safe invoke

        /// <summary>
        /// Executes a function on the thread of the given object
        /// </summary>
        public static TResult SafeInvoke<T, TResult>(this T isi, Func<T, TResult> call) where T : ISynchronizeInvoke {
            if (isi.InvokeRequired) {
                IAsyncResult result = isi.BeginInvoke(call, new object[] { isi });
                object endResult = isi.EndInvoke(result);
                return (TResult)endResult;
            }
            return call(isi);
        }

        /// <summary>
        /// Executes an action on the thread of the given object
        /// </summary>
        public static void SafeInvoke<T>(this T isi, Action<T> call) where T : ISynchronizeInvoke {
            if (isi.InvokeRequired) isi.BeginInvoke(call, new object[] { isi });
            else
                call(isi);
        }

        #endregion

        #region Enum and attributes extensions

        /// <summary>
        /// Returns the attribute array for the given Type T and the given value,
        /// not to self : dont use that on critical path -> reflection is costly
        /// </summary>
        public static T[] GetAttributes<T>(this Enum value) where T : Attribute {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null) {
                FieldInfo field = type.GetField(name);
                if (field != null) {
                    var attributeArray = (T[])Attribute.GetCustomAttributes(field, typeof(T), true);
                    return attributeArray;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the attribute for the given Type T and the given value,
        /// not to self : dont use that on critical path -> reflection is costly
        /// </summary>
        public static T GetAttribute<T>(this Enum value) where T : Attribute {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null) {
                FieldInfo field = type.GetField(name);
                if (field != null) {
                    var attribute = Attribute.GetCustomAttribute(field, typeof(T), true) as T;
                    return attribute;
                }
            }
            return null;
        }

        /// <summary>
        /// Allows to describe a field of an enum like this :
        /// [EnumAttribute(Value = "DATA-SOURCE")]
        /// and use the value "Value" with :
        /// currentOperation.GetAttribute!EnumAttribute>().Value 
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class EnumAttribute : Attribute { }

        /// <summary>
        /// Decorate enum values with [Description("Description for Foo")] and get their description with x.Foo.GetDescription()
        /// </summary>
        public static string GetDescription(this Enum value) {
            var attr = value.GetAttribute<DescriptionAttribute>();
            return attr != null ? attr.Description : null;
        }

        /// <summary>
        /// Returns a collection of all the values of a given Enum
        /// </summary>
        public static IEnumerable<T> GetEnumValues<T>(this Enum value) {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Returns an array of all the names of a given Enum
        /// </summary>
        public static string[] GetEnumNames<T>(this Enum value) {
            return Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// MyEnum tester = MyEnum.FlagA | MyEnum.FlagB;
        /// if(tester.IsSet(MyEnum.FlagA))
        /// </summary>
        public static bool IsFlagSet(this Enum input, Enum matchTo) {
            return (Convert.ToUInt32(input) & Convert.ToUInt32(matchTo)) != 0;
        }
        
        //flags |= flag;// SetFlag
        //flags &= ~flag; // ClearFlag 

        #endregion

        #region string extensions

        /// <summary>
        /// Allows to test a string with a regular expression, uses the IgnoreCase option by default
        /// good website : https://regex101.com/
        /// </summary>
        public static bool RegexMatch(this string source, string regex, RegexOptions options = RegexOptions.IgnoreCase) {
            var reg = new Regex(regex, options);
            return reg.Match(source).Success;
        }

        /// <summary>
        /// Allows to replace a string with a regular expression, uses the IgnoreCase option by default,
        /// replacementStr can contains $1, $2...
        /// </summary>
        public static string RegexReplace(this string source, string regexString, string replacementStr, RegexOptions options = RegexOptions.IgnoreCase) {
            var regex = new Regex(regexString, options);
            return regex.Replace(source, replacementStr);
        }

        /// <summary>
        /// Allows to replace a string with a regular expression, uses the IgnoreCase option by default
        /// </summary>
        public static string RegexReplace(this string source, string regexString, MatchEvaluator matchEvaluator, RegexOptions options = RegexOptions.IgnoreCase) {
            var regex = new Regex(regexString, options);
            return regex.Replace(source, matchEvaluator);
        }

        /// <summary>
        /// Allows to find a string with a regular expression, uses the IgnoreCase option by default, returns a match collection,
        /// to be used foreach (Match match in collection) { with match.Groups[1].Value being the first capture [2] etc...
        /// </summary>
        public static MatchCollection RegexFind(this string source, string regexString, RegexOptions options = RegexOptions.IgnoreCase) {
            var regex = new Regex(regexString, options);
            return regex.Matches(source);
        }

        /// <summary>
        /// Allows to tranform a matching string using * and ? (wildcards) into a valid regex expression
        /// it escapes regex special char so it will work as you expect!
        /// Ex: foo*.xls? will become ^foo.*\.xls.$
        /// if the pattern doesn't start with a * and doesn't end with a *, it adds both
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string WildCardToRegex(this string pattern) {
            if (string.IsNullOrEmpty(pattern))
                return ".*";
            var startStar = pattern[0].Equals('*');
            var endStar = pattern[pattern.Length - 1].Equals('*');
            return (!startStar ? (endStar ? "^" : "") : "") + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + (!endStar ? (startStar ? "$" : "") : "");
        }

        /// <summary>
        /// Returns the html link representation from a url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="urlName"></param>
        /// <returns></returns>
        public static string ToHtmlLink(this string url, string urlName = null) {
            return String.Format("<a href='{0}'>{1}</a>", url, urlName ?? url);
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
        /// Replaces " by ~", replaces new lines by spaces and add extra " at the start and end of the string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ProQuoter(this string text) {
            return "\"" + (text ?? "").Replace("\"", "~\"").Replace("\n", " ").Replace("\r", "") + "\"";
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
            var lines = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(w => (charCount += w.Length + 1) / lineLength)
                .Select(g => String.Join(" ", g));
            return String.Join(eolString, lines.ToArray());
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
                    if (Int32.Parse(splitLocal[i]) > Int32.Parse(splitDistant[i]))
                        return true;
                    if (Int32.Parse(splitLocal[i]) < Int32.Parse(splitDistant[i]))
                        return false;
                    i++;
                }
            } catch (Exception) {
                // would happen if the input strings are incorrect
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
                if (Char.IsLetter(word[count]))
                    return true;
                count++;
            }
            return false;
        }

        /// <summary>
        /// autocase the keyword according to the mode given
        /// </summary>
        public static string ConvertCase(this string keyword, int mode, string naturalCase = null) {
            switch (mode) {
                case 1:
                    return keyword.ToUpper();
                case 2:
                    return keyword.ToLower();
                case 3:
                    return keyword.ToTitleCase();
                case 4:
                    return naturalCase ?? keyword;
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
        /// case insensitive contains
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        public static bool ContainsFast(this string source, string toCheck) {
            return source.IndexOf(toCheck, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        #endregion

        #region string builder

        /// <summary>
        /// Append a text X times
        /// </summary>
        public static StringBuilder Append(this StringBuilder builder, string text, int count) {
            for (int i = 0; i < count; i++)
                builder.Append(text);
            return builder;
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

        public static StringBuilder TrimEnd(this StringBuilder builder) {
            if (builder.Length > 0) {
                int i;
                for (i = builder.Length - 1; i >= 0; i--)
                    if (!Char.IsWhiteSpace(builder[i]))
                        break;

                builder.Length = i + 1;
            }
            return builder;
        }

        #endregion
    }
}
