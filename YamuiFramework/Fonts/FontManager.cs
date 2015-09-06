using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YamuiFramework.Fonts {
    public enum LabelFunction {
        AppliTitle,
        Small,
        Normal,
        Heading,
        Title,
        FormTitle,
        TopLink,
        Link,
        AutoCompletion
    }

    public enum TabFunction {
        Main,
        Secondary,
        SecondaryNotSelected
    }

    public class FontManager {

        public static Font GetStandardFont() {
            return GetFont(FontStyle.Regular, 12f);
        }

        public static Font GetLabelFont(LabelFunction lbFunction) {
            switch (lbFunction) {
                case LabelFunction.AppliTitle:
                    return GetOtherFont(@"STRENUOUS", FontStyle.Regular, 22f);
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
                case LabelFunction.AutoCompletion:
                    return GetFont(FontStyle.Regular, 12f);
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
                // TODO: we should load a custom font here...
                return fontTester;
                fontTester.Dispose();
                //FontFamily fontFamily = GetFontFamily(familyName);
                return new Font("Times New Roman", emSize, fontStyle, unit);
            }

            //private readonly PrivateFontCollection _fontCollection = new PrivateFontCollection();

            //private FontFamily GetFontFamily(string familyName) {
            //    lock (_fontCollection) {
            //        foreach (FontFamily fontFamily in _fontCollection.Families)
            //            if (fontFamily.Name == familyName) return fontFamily;
            //        if (!File.Exists(@"C:\Work\3PA_side\SMALLFONT.ttf"))
            //            File.WriteAllBytes(@"C:\Work\3PA_side\SMALLFONT.ttf", FontResources.SMALLFONT);
            //        _fontCollection.AddFontFile("C:\\Work\\3PA_side\\SMALLFONT.ttf");
            //        return _fontCollection.Families[_fontCollection.Families.Length - 1];
            //        //FontResources.ResourceManager.GetObject(familyName.Replace(' ', '_') + ".ttf");
            //        //string resourceName = GetType().Namespace + "." + familyName.Replace(' ', '_') + ".ttf";
            //    }
            //}
        }

        #endregion

    }


}
