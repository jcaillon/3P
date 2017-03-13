#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiExtensions.cs) is part of YamuiFramework.
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
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YamuiFramework.Helper {
    public static class YamuiExtensions {
        #region ui thread safe invoke

        /// <summary>
        /// Executes a function on the thread of the given object
        /// </summary>
        public static TResult SafeInvoke<T, TResult>(this T isi, Func<T, TResult> call) where T : ISynchronizeInvoke {
            if (isi.InvokeRequired) {
                IAsyncResult result = isi.BeginInvoke(call, new object[] {isi});
                object endResult = isi.EndInvoke(result);
                return (TResult) endResult;
            }
            return call(isi);
        }

        /// <summary>
        /// Executes an action on the thread of the given object
        /// </summary>
        public static void SafeInvoke<T>(this T isi, Action<T> call) where T : ISynchronizeInvoke {
            if (isi.InvokeRequired) isi.BeginInvoke(call, new object[] {isi});
            else
                call(isi);
        }

        /// <summary>
        /// Executes a function on the thread of the given object
        /// </summary>
        public static object SafeSyncInvoke<T, TResult>(this T isi, Func<T, TResult> call) where T : ISynchronizeInvoke {
            if (isi.InvokeRequired) {
                return isi.Invoke(call, new object[] {isi});
            }
            return call(isi);
        }

        /// <summary>
        /// Executes an action on the thread of the given object
        /// </summary>
        public static void SafeSyncInvoke<T>(this T isi, Action<T> call) where T : ISynchronizeInvoke {
            if (isi.InvokeRequired) isi.Invoke(call, new object[] {isi});
            else
                call(isi);
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying window handle.
        /// </summary>
        /// <param name="control">The control whose window handle the delegate should be invoked on.</param>
        /// <param name="method">A delegate that contains a method to be called in the control's thread context.</param>
        public static void SafeSyncInvoke(this Control control, Action method) {
            if (control.InvokeRequired) {
                control.Invoke(method);
            } else {
                method();
            }
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying window handle, returning a
        /// value.
        /// </summary>
        /// <param name="control">The control whose window handle the delegate should be invoked on.</param>
        /// <param name="method">A delegate that contains a method to be called in the control's thread context and
        /// that returns a value.</param>
        /// <returns>The return value from the delegate being invoked.</returns>
        public static TResult SafeSyncInvoke<TResult>(this Control control, Func<TResult> method) {
            if (control.InvokeRequired) {
                return (TResult) control.Invoke(method);
            }
            return method();
        }

        #endregion

        #region Simple math

        /// <summary>
        /// Forces a value between a minimum and a maximum
        /// </summary>
        public static int Clamp(this int value, int min, int max) {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        /// <summary>
        /// Forces a value above a minimum
        /// </summary>
        public static int ClampMin(this int value, int min) {
            if (value < min)
                return min;
            return value;
        }

        /// <summary>
        /// Forces a value under a maximum
        /// </summary>
        public static int ClampMax(this int value, int max) {
            if (value > max)
                return max;
            return value;
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts a string to an object of the given type
        /// </summary>
        public static object ConvertFromStr(this string value, Type destType) {
            try {
                if (destType == typeof(string))
                    return value;
                return TypeDescriptor.GetConverter(destType).ConvertFromInvariantString(value);
            } catch (Exception) {
                return destType.IsValueType ? Activator.CreateInstance(destType) : null;
            }
        }

        /// <summary>
        /// Converts an object to a string
        /// </summary>
        public static string ConvertToStr(this object value) {
            if (value == null)
                return string.Empty;
            return TypeDescriptor.GetConverter(value).ConvertToInvariantString(value);
        }

        #endregion

        #region GetRoundedRect

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="diameter">The diameter of the corners</param>
        /// <returns>A round cornered rectagle path</returns>
        public static GraphicsPath GetRoundedRect(this RectangleF rect, float diameter) {
            GraphicsPath path = new GraphicsPath();

            if (diameter > 0) {
                RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
            } else {
                path.AddRectangle(rect);
            }

            return path;
        }

        #endregion

        #region Colors extensions

        /// <summary>
        /// Replace all the occurences of @alias thx to the aliasDictionnary
        /// </summary>
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
        public static string ApplyColorFunctions(this string htmlColor) {
            if (htmlColor.Contains("(")) {
                var functionName = htmlColor.Substring(0, htmlColor.IndexOf("(", StringComparison.CurrentCultureIgnoreCase));
                var splitValues = htmlColor.GetBetweenMostNested("(", ")").Split(',');
                float ratio;
                if (!float.TryParse(splitValues[1].Trim().Replace("%", ""), out ratio))
                    ratio = 0;

                // Apply the color function to the base color (in case this is another function embedded in this one)
                var baseColor = splitValues[0].Trim().ApplyColorFunctions();

                if (functionName.StartsWith("dark"))
                    return baseColor.ModifyColorLuminosity(-1*ratio/100);
                if (functionName.StartsWith("light"))
                    return baseColor.ModifyColorLuminosity(ratio/100);

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
        /// Get string value between [first] a and [last] b (not included)
        /// </summary>
        public static string GetBetweenMostNested(this string value, string a, string b, StringComparison comparer = StringComparison.CurrentCultureIgnoreCase) {
            int posA = value.LastIndexOf(a, comparer);
            int posB = value.IndexOf(b, comparer);
            return posB == -1 ? value.Substring(posA + 1) : value.Substring(posA + 1, posB - posA - 1);
        }

        #endregion

        #region image manipulation

        /// <summary>
        /// Returns the image in grey scale...
        /// </summary>
        public static Image MakeGreyscale3(this Image original) {
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
    }
}