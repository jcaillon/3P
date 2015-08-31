using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YamuiFramework.Fonts {
    public enum LabelFunction {
        Small,
        Normal,
        Heading,
        Title,
        FormTitle,
        TopLink,
        Link
    }

    public enum TabFunction {
        Main,
        Secondary,
        SecondaryNotSelected
    }

    class FontManager {

        public static Font GetStandardFont() {
            return GetFont(FontStyle.Regular, 12f);
        }

        public static Font GetLabelFont(LabelFunction lbFunction) {
            switch (lbFunction) {
                case LabelFunction.Title:
                    return GetFont(FontStyle.Bold, 18f);
                case LabelFunction.Heading:
                    return GetFont(FontStyle.Bold, 14f);
                case LabelFunction.FormTitle:
                    return GetFont(FontStyle.Bold, 13f);
                case LabelFunction.TopLink:
                    return GetFont(FontStyle.Regular, 11f);
                case LabelFunction.Link:
                    return GetFont(FontStyle.Regular | FontStyle.Underline, 12f);
                case LabelFunction.Small:
                    return GetFont(FontStyle.Regular, 10f);
                default:
                    return GetFont(FontStyle.Regular, 12f);
            }
        }

        public static Font GetTabControlFont(TabFunction tabFunction) {
            switch (tabFunction) {
                case TabFunction.Main:
                    return GetFont(FontStyle.Regular, 24f);
                case TabFunction.Secondary:
                    return GetFont(FontStyle.Bold, 11f);
                default:
                    return GetFont(FontStyle.Regular, 11f);
            }
        }

        public static Font GetStandardWaterMarkFont() {
            return GetFont(FontStyle.Italic, 12f);
        }

        public static Font GetFont(FontStyle fontStyle, float size) {
            return _fontResolver.ResolveFont("Segoe UI", size, fontStyle, GraphicsUnit.Pixel);
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
                if (fontTester.Name == familyName || !TryResolve(ref familyName, ref fontStyle)) {
                    return fontTester;
                }
                fontTester.Dispose();

                FontFamily fontFamily = GetFontFamily(familyName);
                return new Font(fontFamily, emSize, fontStyle, unit);
            }

            private const string SegoeRegular = "SEGOEUI";
            private const string SegoeItalic = "SEGOEUII";
            private const string SegoeBold = "SEGOEUIB";

            private readonly PrivateFontCollection _fontCollection = new PrivateFontCollection();

            private static bool TryResolve(ref string familyName, ref FontStyle fontStyle) {
                if (familyName == "Segoe UI Light") {
                    familyName = SegoeItalic;
                    if (fontStyle != FontStyle.Bold) fontStyle = FontStyle.Regular;
                    return true;
                }

                if (familyName != "Segoe UI") return false;
                switch (fontStyle) {
                    case FontStyle.Bold:
                        familyName = SegoeBold;
                        return true;
                    case FontStyle.Italic:
                        familyName = SegoeItalic;
                        return true;
                }

                familyName = SegoeRegular;
                return true;
            }

            private FontFamily GetFontFamily(string familyName) {
                lock (_fontCollection) {
                    foreach (FontFamily fontFamily in _fontCollection.Families)
                        if (fontFamily.Name == familyName) return fontFamily;

                    string resourceName = GetType().Namespace + ".Fonts." + familyName.Replace(' ', '_') + ".ttf";

                    Stream fontStream = null;
                    IntPtr data = IntPtr.Zero;
                    try {
                        fontStream = GetType().Assembly.GetManifestResourceStream(resourceName);
                        if (fontStream != null) {
                            int bytes = (int) fontStream.Length;
                            data = Marshal.AllocCoTaskMem(bytes);
                            byte[] fontdata = new byte[bytes];
                            fontStream.Read(fontdata, 0, bytes);
                            Marshal.Copy(fontdata, 0, data, bytes);
                            _fontCollection.AddMemoryFont(data, bytes);
                        }
                        return _fontCollection.Families[_fontCollection.Families.Length - 1];
                    } finally {
                        if (fontStream != null) fontStream.Dispose();
                        if (data != IntPtr.Zero) Marshal.FreeCoTaskMem(data);
                    }
                }
            }
        }

        #endregion

    }


}
