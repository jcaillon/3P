#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (HtmlHandler.cs) is part of YamuiFramework.
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
        /// Define the extra css sheet to use in all html panels/labels/tooltips
        /// </summary>
        public static string ExtraCssSheet { get; set; }

        /// <summary>
        /// Updates the colors of the basic css sheet shared by all html panels/labels
        /// </summary>
        public static void UpdateBaseCssData() {
            var baseCss = Resources.Resources.BaseStyleSheet;
            baseCss = baseCss.Replace("%FGcolor%", ColorTranslator.ToHtml(ThemeManager.Current.LabelsColorsNormalForeColor));
            baseCss = baseCss.Replace("%BGcolor%", ColorTranslator.ToHtml(ThemeManager.Current.FormColorBackColor));
            _baseCssData = HtmlRender.ParseStyleSheet(baseCss + "\n" + ExtraCssSheet);
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
            // load image from user
            if (OnImageNeeded != null) {
                OnImageNeeded(null, e);
                if (e.Handled)
                    return;
            }
            // load image from yamui library
            Image tryImg = ImageGetter.GetInstance().Get(e.Src);
            if (tryImg == null) return;
            e.Handled = true;
            e.Callback(tryImg);
        }
    }
}
