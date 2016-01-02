#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (FontManager.cs) is part of YamuiFramework.
// 
// // YamuiFramework is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // YamuiFramework is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YamuiFramework.Fonts {

    public enum FontFunction {
        AppliTitle,
        Small,
        Normal,
        Heading,
        Title,
        FormTitle,
        TopLink,
        Link,
        AutoCompletion,
        MenuMain,
        MenuSecondary,
        WaterMark
    }

    public class FontManager {

        public static Font GetStandardFont() {
            return GetFont(FontFunction.Normal);
        }

        public static Font GetFont(FontFunction lbFunction) {
            switch (lbFunction) {
                case FontFunction.AppliTitle:
                    return GetOtherFont(@"REDCIRCL", FontStyle.Regular, 20f);
                case FontFunction.Title:
                    return GetFont(FontStyle.Bold, 18f);
                case FontFunction.Heading:
                    return GetFont(FontStyle.Bold, 14f);
                case FontFunction.FormTitle:
                    return GetFont(FontStyle.Bold, 13f);
                case FontFunction.TopLink:
                    return GetFont(FontStyle.Regular, 11f);
                case FontFunction.Link:
                    return GetFont(FontStyle.Regular | FontStyle.Underline, 12f);
                case FontFunction.Small:
                    return GetFont(FontStyle.Regular, 10f);
                case FontFunction.AutoCompletion:
                    return GetFont(FontStyle.Regular, 12f);
                case FontFunction.MenuMain:
                    return GetFont(FontStyle.Regular, 24f);
                case FontFunction.MenuSecondary:
                    return GetFont(FontStyle.Bold, 11f);
                case FontFunction.WaterMark:
                    return GetFont(FontStyle.Italic, 12f);
                default:
                    return GetFont(FontStyle.Regular, 12f);
            }
        }

        public static Font GetFont(FontStyle fontStyle, float size) {
            return _fontResolver.ResolveFont(@"Segoe UI", size, fontStyle, GraphicsUnit.Pixel);
        }

        public static Font GetOtherFont(string familyName, FontStyle fontStyle, float size) {
            return _fontResolver.ResolveFont(familyName, size, fontStyle, GraphicsUnit.Pixel);
        }

        #region TextFormatForDrawText

        public static TextFormatFlags GetTextFormatFlags(ContentAlignment textAlign) {
            return GetTextFormatFlags(textAlign, false);
        }

        public static TextFormatFlags GetTextFormatFlags(ContentAlignment textAlign, bool wrapToLine) {
            TextFormatFlags controlFlags = TextFormatFlags.Default;
            switch (wrapToLine) {
                case true:
                    controlFlags = TextFormatFlags.WordBreak;
                    break;
                case false:
                    controlFlags = TextFormatFlags.EndEllipsis;
                    break;
            }
            switch (textAlign) {
                case ContentAlignment.TopLeft:
                    controlFlags |= TextFormatFlags.Top | TextFormatFlags.Left;
                    break;
                case ContentAlignment.TopCenter:
                    controlFlags |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.TopRight:
                    controlFlags |= TextFormatFlags.Top | TextFormatFlags.Right;
                    break;

                case ContentAlignment.MiddleLeft:
                    controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                    break;
                case ContentAlignment.MiddleCenter:
                    controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.MiddleRight:
                    controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                    break;

                case ContentAlignment.BottomLeft:
                    controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.Left;
                    break;
                case ContentAlignment.BottomCenter:
                    controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.BottomRight:
                    controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.Right;
                    break;
            }
            return controlFlags;
        }

        #endregion

        #region FontResolver

        internal interface IFontResolver {
            Font ResolveFont(string familyName, float emSize, FontStyle fontStyle, GraphicsUnit unit);
        }

        private static IFontResolver _fontResolver;

        static FontManager() {
            try {
                _fontResolver = new FontResolver();
                return;
            } catch (Exception) {
                // ignore
            }
            _fontResolver = new DefaultFontResolver();
        }

        internal class DefaultFontResolver : IFontResolver {
            public Font ResolveFont(string familyName, float emSize, FontStyle fontStyle, GraphicsUnit unit) {
                return new Font(familyName, emSize, fontStyle, unit);
            }
        }

        internal class FontResolver : IFontResolver {
            public Font ResolveFont(string familyName, float emSize, FontStyle fontStyle, GraphicsUnit unit) {
                Font fontTester = new Font(familyName, emSize, fontStyle, unit);
                if (fontTester.Name == familyName)
                    return fontTester;

                var familyFont = GetFontFamily(familyName);
                if (familyFont != null)
                    fontTester = new Font(familyFont, emSize, fontStyle, unit);
                return fontTester;
            }

            private PrivateFontCollection _pfc = new PrivateFontCollection();

            private FontFamily GetFontFamily(string familyName) {
                lock (_pfc) {
                    foreach (FontFamily fontFamily in _pfc.Families)
                        if (fontFamily.Name == familyName) return fontFamily;

                    byte[] fontdata;
                    switch (familyName) {
                        case @"REDCIRCL":
                            fontdata = ResourceFont.REDCIRCL;
                            break;
                        default:
                            return null;
                    }

                    int fontLength = fontdata.Length;

                    // create an unsafe memory block for the font data and copy the bytes to the unsafe memory block
                    IntPtr data = Marshal.AllocCoTaskMem(fontLength);
                    Marshal.Copy(fontdata, 0, data, fontLength);

                    // pass the font to the font collection
                    _pfc.AddMemoryFont(data, fontLength);

                    Marshal.FreeCoTaskMem(data);

                    return _pfc.Families[0];
                }
            }
        }

        #endregion

    }


}
