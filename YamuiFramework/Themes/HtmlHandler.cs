using System;
using System.ComponentModel;
using System.Drawing;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.Core.Core;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Resources;

namespace YamuiFramework.Themes {
    public class HtmlHandler {

        private static CssData _baseCssData;

        private static event EventHandler<HtmlImageLoadEventArgs> OnImageNeeded;

        /// <summary>
        /// Register to this event and handle the image needed by the html (<img src='img'></img>)
        /// in panels/labels/tooltips
        /// Ex: 
        /// Image tryImg = ImageGetter.GetInstance().Get(e.Src);
        /// if (tryImg == null) return;
        /// e.Handled = true;
        /// e.Callback(tryImg);
        /// </summary>
        public static event EventHandler<HtmlImageLoadEventArgs> ImageNeeded {
            add { OnImageNeeded += value; }
            remove { OnImageNeeded -= value; }
        }

        /// <summary>
        /// Define the extra css sheet to use in all html panels/labels/tooltips, you can use those keywords that will
        /// be replaced by the theme's colors :
        /// %FGcolor%
        /// %BGcolor%
        /// </summary>
        public static string ExtraCssSheet { get; set; }

        /// <summary>
        /// Updates the colors of the basic css sheet shared by all html panels/labels
        /// </summary>
        public static void UpdateBaseCssData() {
            var baseCss = Resources.Resources.BaseStyleSheet + "\n" + ExtraCssSheet;
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
                return;
            }
            if (OnImageNeeded != null) {
                OnImageNeeded(null, e);
            }
        }
    }
}
