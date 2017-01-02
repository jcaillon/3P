#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Utilities.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;

namespace YamuiFramework.Helper {

    /// <summary>
    /// This class provides various utilies to use in YamuiFramework and outside
    /// </summary>
    public static class Utilities {

        #region Paint

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <returns>A round cornered rectagle path</returns>   
        public static GraphicsPath GetRoundedRect(float x, float y, float width, float height, float diameter) {
            return new RectangleF(x, y, width, height).GetRoundedRect(diameter);
        }

        #endregion


        #region Html utilities

        /// <summary>
        /// Returns a fair width in which an html can be displayed
        /// </summary>
        public static int MeasureHtmlPrefWidth(string htmlContent, int minWidth, int maxWidth) {

            // find max height taken by the html
            // Measure the size of the html
            using (var gImg = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(gImg)) {

                string content = YamuiThemeManager.WrapLabelText(htmlContent);

                // this should retrieve the best width, however it will not be the case if there are some width='100%' for instance
                var calcWidth = (int)HtmlRender.Measure(g, content, 99999, YamuiThemeManager.CurrentThemeCss, null, (sender, args) => YamuiThemeManager.GetHtmlImages(args)).Width;

                if (calcWidth >= 9999) {
                    // get the minimum size required to display everything
                    var sizef = HtmlRender.Measure(g, content, 10, YamuiThemeManager.CurrentThemeCss, null, (sender, args) => YamuiThemeManager.GetHtmlImages(args));
                    minWidth = ((int) sizef.Width).Clamp(minWidth, maxWidth);

                    // set to max Width, get the height at max Width
                    sizef = HtmlRender.Measure(g, content, maxWidth, YamuiThemeManager.CurrentThemeCss, null, (sender, args) => YamuiThemeManager.GetHtmlImages(args));
                    var prefHeight = sizef.Height;

                    // now we got the final height, resize width until height changes
                    int j = 0;
                    int detla = maxWidth / 2;
                    calcWidth = maxWidth;
                    do {
                        calcWidth -= detla;
                        calcWidth = calcWidth.Clamp(minWidth, maxWidth);

                        sizef = HtmlRender.Measure(g, content, calcWidth, YamuiThemeManager.CurrentThemeCss, null, (sender, args) => YamuiThemeManager.GetHtmlImages(args));

                        if (sizef.Height > prefHeight) {
                            calcWidth += detla;
                            detla /= 2;
                        }

                        if (calcWidth == maxWidth || calcWidth == minWidth)
                            break;

                        j++;
                    } while (j < 6);
                }

                return calcWidth.Clamp(minWidth, maxWidth);
            }
        }

        #endregion

        #region Validation and data type conversions

        public static readonly Dictionary<Type, char[]> KeyPressValidChars = new Dictionary<Type, char[]> {
            {typeof (byte), GetCultureChars(true, false, true)},
            {typeof (sbyte), GetCultureChars(true, true, true)},
            {typeof (short), GetCultureChars(true, true, true)},
            {typeof (ushort), GetCultureChars(true, false, true)},
            {typeof (int), GetCultureChars(true, true, true)},
            {typeof (uint), GetCultureChars(true, false, true)},
            {typeof (long), GetCultureChars(true, true, true)},
            {typeof (ulong), GetCultureChars(true, false, true)},
            {typeof (double), GetCultureChars(true, true, true, true, true, true)},
            {typeof (float), GetCultureChars(true, true, true, true, true, true)},
            {typeof (decimal), GetCultureChars(true, true, true, true, true)},
            {typeof (TimeSpan), GetCultureChars(true, true, false, new[] {'-'})},
            {typeof (Guid), GetCultureChars(true, false, false, "-{}()".ToCharArray())}
        };

        public static char[] GetCultureChars(bool digits, bool neg, bool pos, bool dec = false, bool grp = false, bool e = false) {
            var c = CultureInfo.CurrentCulture.NumberFormat;
            var l = new List<string>();
            if (digits) l.AddRange(c.NativeDigits);
            if (neg) l.Add(c.NegativeSign);
            if (pos) l.Add(c.PositiveSign);
            if (dec) l.Add(c.NumberDecimalSeparator);
            if (grp) l.Add(c.NumberGroupSeparator);
            if (e) l.Add("Ee");
            var sb = new StringBuilder();
            foreach (var s in l)
                sb.Append(s);
            char[] ca = sb.ToString().ToCharArray();
            Array.Sort(ca);
            return ca;
        }

        public static char[] GetCultureChars(bool timeChars, bool timeSep, bool dateSep, char[] other) {
            var c = CultureInfo.CurrentCulture;
            var l = new List<string>();
            if (timeChars) l.AddRange(c.NumberFormat.NativeDigits);
            if (timeSep) {
                l.Add(c.DateTimeFormat.TimeSeparator);
                l.Add(c.NumberFormat.NumberDecimalSeparator);
            }
            if (dateSep) l.Add(c.DateTimeFormat.DateSeparator);
            if (other != null && other.Length > 0) l.Add(new string(other));
            var sb = new StringBuilder();
            foreach (var s in l)
                sb.Append(s);
            char[] ca = sb.ToString().ToCharArray();
            Array.Sort(ca);
            return ca;
        }

        public static readonly Dictionary<Type, Predicate<string>> Validations = new Dictionary<Type, Predicate<string>> {
            {
                typeof (byte), s => {
                    byte n;
                    return Byte.TryParse(s, out n);
                }
            }, {
                typeof (sbyte), s => {
                    sbyte n;
                    return SByte.TryParse(s, out n);
                }
            }, {
                typeof (short), s => {
                    short n;
                    return Int16.TryParse(s, out n);
                }
            }, {
                typeof (ushort), s => {
                    ushort n;
                    return UInt16.TryParse(s, out n);
                }
            }, {
                typeof (int), s => {
                    int n;
                    return Int32.TryParse(s, out n);
                }
            }, {
                typeof (uint), s => {
                    uint n;
                    return UInt32.TryParse(s, out n);
                }
            }, {
                typeof (long), s => {
                    long n;
                    return Int64.TryParse(s, out n);
                }
            }, {
                typeof (ulong), s => {
                    ulong n;
                    return UInt64.TryParse(s, out n);
                }
            }, {
                typeof (char), s => {
                    char n;
                    return Char.TryParse(s, out n);
                }
            }, {
                typeof (double), s => {
                    double n;
                    return Double.TryParse(s, out n);
                }
            }, {
                typeof (float), s => {
                    float n;
                    return Single.TryParse(s, out n);
                }
            }, {
                typeof (decimal), s => {
                    decimal n;
                    return Decimal.TryParse(s, out n);
                }
            }, {
                typeof (DateTime), s => {
                    DateTime n;
                    return DateTime.TryParse(s, out n);
                }
            }, {
                typeof (TimeSpan), s => {
                    TimeSpan n;
                    return TimeSpan.TryParse(s, out n);
                }
            }, {
                typeof (Guid), s => {
                    try {
                        // ReSharper disable once ObjectCreationAsStatement
                        new Guid(s);
                        return true;
                    } catch {
                        return false;
                    }
                }
            }
        };

        public static bool IsSupportedType(Type type) {
            if (typeof (IConvertible).IsAssignableFrom(type))
                return true;
            var cvtr = TypeDescriptor.GetConverter(type);
            if (cvtr.CanConvertFrom(typeof (string)) && cvtr.CanConvertTo(typeof (string)))
                return true;
            return false;
        }

        public static bool IsSimpleType(this Type type) {
            return type.IsPrimitive || type.IsEnum || Array.Exists(SimpleTypes, t => t == type) || Convert.GetTypeCode(type) != TypeCode.Object ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
        }

        public static readonly Type[] SimpleTypes = {
            typeof (Enum), typeof (Decimal), typeof (DateTime),
            typeof (DateTimeOffset), typeof (String), typeof (TimeSpan), typeof (Guid)
        };

        public static bool IsInvalidKey(char keyChar, Type itemType) {
            if (Char.IsControl(keyChar))
                return false;
            char[] chars;
            KeyPressValidChars.TryGetValue(itemType, out chars);
            if (chars != null) {
                int si = Array.BinarySearch(chars, keyChar);
                if (si < 0)
                    return true;
            }
            return false;
        }

        #endregion

        #region Read a configuration file

        /// <summary>
        /// Reads all the line of either the filePath (if the file exists) or from byte array dataResources,
        /// Apply the action toApplyOnEachLine to each line
        /// Uses encoding as the Encoding to read the file or convert the byte array to a string
        /// Uses the char # as a comment in the file
        /// </summary>
        public static bool ForEachLine(string filePath, byte[] dataResources, Action<int, string> toApplyOnEachLine, Encoding encoding, Action<Exception> onException) {
            bool wentOk = true;
            try {
                SubForEachLine(filePath, dataResources, toApplyOnEachLine, encoding);
            } catch (Exception e) {
                wentOk = false;
                onException(e);

                // read default file, if it fails then we can't do much but to throw an exception anyway...
                if (dataResources != null)
                    SubForEachLine(null, dataResources, toApplyOnEachLine, encoding);
            }
            return wentOk;
        }

        private static void SubForEachLine(string filePath, byte[] dataResources, Action<int, string> toApplyOnEachLine, Encoding encoding) {

            string line;
            // to apply on each line
            Action<TextReader> action = reader => {
                int i = 0;
                while ((line = reader.ReadLine()) != null) {
                    if (line.Length > 0 && line[0] != '#')
                        toApplyOnEachLine(i, line);
                    i++;
                }
            };

            // either read from the file or from the byte array
            if (!String.IsNullOrEmpty(filePath) && File.Exists(filePath)) {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    using (var reader = new StreamReader(fileStream, encoding)) {
                        action(reader);
                    }
                }
            } else {
                // we use the default encoding for the resoures since we can control the encoding on this...
                using (StringReader reader = new StringReader(Encoding.Default.GetString(dataResources))) {
                    action(reader);
                }
            }
        }

        #endregion

        #region control

        /// <summary>
        /// List all the controls children of "control" of type "type"
        /// this is recursive, so it find them all
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Control> GetAll(Control control, Type type) {
            try {
                var controls = control.Controls.Cast<Control>();
                var enumerable = controls as IList<Control> ?? controls.ToList();
                return enumerable.SelectMany(ctrl => GetAll(ctrl, type)).Concat(enumerable).Where(c => c.GetType() == type);
            } catch (Exception) {
                return null;
            }
        }

        /// <summary>
        /// Get the first control of the type type it can find
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Control GetFirst(Control control, Type type) {
            try {
                return control.Controls.Cast<object>().Where(control1 => control1.GetType() == type).Cast<Control>().FirstOrDefault();
            } catch (Exception) {
                return null;
            }
        }

        #endregion
    }
}
