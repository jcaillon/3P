using System.Drawing;
using YamuiFramework.HtmlRenderer.Core.Core;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Resources;

namespace YamuiFramework.Themes {
    class HtmlHandler {
        private static CssData _baseCssData;

        /// <summary>
        /// Updates the colors of the basic css sheet shared by all html panels/labels
        /// </summary>
        public static void UpdateBaseCssData() {
            var baseCss = Resources.Resources.baseCss;
            baseCss = baseCss.Replace("%FGcolor%", ColorTranslator.ToHtml(ThemeManager.Current.LabelsColorsNormalForeColor));
            baseCss = baseCss.Replace("%BGcolor%", ColorTranslator.ToHtml(ThemeManager.Current.FormColorBackColor));
            _baseCssData = HtmlRender.ParseStyleSheet(baseCss);
        }

        /// <summary>
        /// returns the basic css sheet shared by all html panels/labels
        /// </summary>
        /// <returns></returns>
        public static CssData GetBaseCssData() {
            if (_baseCssData == null)
                UpdateBaseCssData();
            return _baseCssData;
        }

        public static void OnImageLoad(HtmlImageLoadEventArgs e) {
            Image tryImg = ImageGetter.GetInstance().Get(e.Src);
            if (tryImg != null) {
                e.Handled = true;
                e.Callback(tryImg);
            }
        }
    }
}
