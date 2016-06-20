#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YamuiFramework.Helper {

    public static class Utilities {

        #region Colors extensions

        /// <summary>
        /// Replace all the occurences of @alias thx to the aliasDictionnary
        /// </summary>
        /// <param name="value"></param>
        /// <param name="aliasDictionnary"></param>
        /// <returns></returns>
        public static string ReplaceAliases(this string value, Dictionary<string, string> aliasDictionnary) {
            while (true) {
                if (value.Contains("@")) {
                    // try to replace a variable name by it's html color value
                    var regex = new Regex(@"@([a-zA-Z]*)", RegexOptions.IgnoreCase);
                    value = regex.Replace(value, match => {
                        if (aliasDictionnary.ContainsKey(match.Groups[1].Value))
                            return aliasDictionnary[match.Groups[1].Value];
                        throw new Exception("Couldn't find the color " + match.Groups[1].Value + "!");
                    });
                    continue;
                }
                return value;
            }
        }

        /// <summary>
        /// Allows to have the syntax : 
        /// lighten(#000000, 35%)
        /// darken(#FFFFFF, 35%)
        /// </summary>
        /// <param name="htmlColor"></param>
        /// <returns></returns>
        public static string ApplyColorFunctions(this string htmlColor) {
            if (htmlColor.Contains("(")) {
                var functionName = htmlColor.Substring(0, htmlColor.IndexOf("(", StringComparison.CurrentCultureIgnoreCase));
                var splitValues = htmlColor.GetValueBetween("(", ")").Split(',');
                float ratio;
                if (!float.TryParse(splitValues[1].Trim().Replace("%", ""), out ratio))
                    ratio = 0;

                // Apply the color function to the base color (in case this is another function embedded in this one)
                var baseColor = splitValues[0].Trim().ApplyColorFunctions();

                if (functionName.StartsWith("dark"))
                    return baseColor.ModifyColorLuminosity(-1 * ratio / 100);
                if (functionName.StartsWith("light"))
                    return baseColor.ModifyColorLuminosity(ratio / 100);

                return baseColor;
            }
            return htmlColor;
        }

        /// <summary>
        /// Lighten or darken a color, ratio + to lighten, - to darken
        /// </summary>
        public static string ModifyColorLuminosity(this string htmlColor, float ratio) {
            var color = ColorTranslator.FromHtml(htmlColor);
            return ColorTranslator.ToHtml(ratio > 0 ? ControlPaint.Light(color, ratio) : ControlPaint.Dark(color, ratio));
            /*
            var isBlack = color.R == 0 && color.G == 0 && color.B == 0;
            var red = (int)Math.Min(Math.Max(0, color.R + ((isBlack ? 255 : color.R) * ratio)), 255);
            var green = (int)Math.Min(Math.Max(0, color.G + ((isBlack ? 255 : color.G) * ratio)), 255);
            var blue = (int)Math.Min(Math.Max(0, color.B + ((isBlack ? 255 : color.B) * ratio)), 255);
            return ColorTranslator.ToHtml(Color.FromArgb(red, green, blue));
             */
        }

        /// <summary>
        /// Get string value between [first] a and [last] b (not included), returns null if it fails
        /// </summary>
        public static string GetValueBetween(this string value, string a, string b, StringComparison comparer = StringComparison.CurrentCultureIgnoreCase) {
            int posA = value.IndexOf(a, comparer);
            int posB = value.LastIndexOf(b, comparer);
            if (posA == -1 || posB == -1)
                return null;
            int adjustedPosA = posA + a.Length;
            return adjustedPosA >= posB ? null : value.Substring(adjustedPosA, posB - adjustedPosA);
        }

        #endregion

        #region image manipulation

        /// <summary>
        /// Returns the image in grey scale...
        /// </summary>
        public static Image MakeGreyscale3(Image original) {

            //create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
                new[] {
                    new[] {.3f, .3f, .3f, 0, 0},
                    new[] {.59f, .59f, .59f, 0, 0},
                    new[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            var attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        #endregion

        #region Read a configuration file line by line

        /// <summary>
        /// Reads all the line of either the filePath (if the file exists) or from byte array dataResources,
        /// Apply the action toApplyOnEachLine to each line
        /// Uses encoding as the Encoding to read the file or convert the byte array to a string
        /// Uses the char # as a comment in the file
        /// </summary>
        public static bool ForEachLine(string filePath, byte[] dataResources, Encoding encoding, Action<string> toApplyOnEachLine, Action<Exception> onException) {
            bool wentOk = true;
            try {
                SubForEachLine(filePath, dataResources, encoding, toApplyOnEachLine);
            } catch (Exception e) {
                wentOk = false;
                onException(e);

                // read default file, if it fails then we can't do much but to throw an exception anyway...
                if (dataResources != null)
                    SubForEachLine(null, dataResources, encoding, toApplyOnEachLine);
            }
            return wentOk;
        }

        public static void SubForEachLine(string filePath, byte[] dataResources, Encoding encoding, Action<string> toApplyOnEachLine) {
            using (StringReader reader = new StringReader((!string.IsNullOrEmpty(filePath) && File.Exists(filePath)) ? File.ReadAllText(filePath, encoding) : encoding.GetString(dataResources))) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    if (line.Length > 0 && line[0] != '#')
                        toApplyOnEachLine(line);
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
