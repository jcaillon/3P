#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiThemeManager.cs) is part of YamuiFramework.
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
using YamuiFramework.HtmlRenderer.Core.Core;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Themes {

    public static class YamuiThemeManager {

        /// <summary>
        /// Return the current Theme object 
        /// </summary>
        public static YamuiTheme Current {
            set {
                _currentTheme = value;
                BaseCssData = null;
            }
            get {
                if (_currentTheme != null) {
                    return _currentTheme;
                }
                // instanciation of current theme
                _currentTheme = GetThemesList()[0];
                BaseCssData = null;
                return _currentTheme;
            }
        }
        private static YamuiTheme _currentTheme;

        /// <summary>
        /// Set to false to deactivate tab animation
        /// </summary>
        public static bool TabAnimationAllowed { get; set; }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<YamuiTheme> GetThemesList() {
            if (_listOfThemes.Count == 0) {
                Class2Xml<YamuiTheme>.LoadFromRaw(_listOfThemes, Resources.Resources.themesXml, true);
            }
            return _listOfThemes;
        }
        private static List<YamuiTheme> _listOfThemes = new List<YamuiTheme>();

        /// <summary>
        /// Subscribe to this event to feed YamuiFramework with a css sheet of your making
        /// </summary>
        public static event GetCssSheet OnGetCssSheet;
        public static event Action OnCssSheetChanged;

        public static CssData BaseCssData {
            get {
                if (_baseCssData == null) {
                    var baseCss = Resources.Resources.BaseStyleSheet;
                    baseCss = baseCss.Replace("%FGcolor%", ColorTranslator.ToHtml(Current.LabelNormalFore));
                    baseCss = baseCss.Replace("%BGcolor%", ColorTranslator.ToHtml(Current.FormBack));
                    baseCss = baseCss.Replace("%FORMBORDER%", ColorTranslator.ToHtml(Current.FormBorder));

                    // load extra css from the user program
                    if (OnGetCssSheet != null) {
                        baseCss = string.Join("\n", baseCss, OnGetCssSheet());
                    }
                    _baseCssData = HtmlRender.ParseStyleSheet(baseCss);
                }
                return _baseCssData;
            }
            set {
                // The css has changed
                if (_baseCssData != null && OnCssSheetChanged != null) {
                    _baseCssData = value;
                    OnCssSheetChanged();
                } else {
                    _baseCssData = value;
                }
            }
        }

        private static CssData _baseCssData;
        public delegate string GetCssSheet();

        /// <summary>
        /// Subscribe to this event to feed the YamuiFramework with an image to load in a img tag
        /// The needed image is in HtmlImageLoadEventArgs.src and you need to feed back the Image
        /// on HtmlImageLoadEventArgs.Callback(MyImage);
        /// </summary>
        public static event EventHandler<HtmlImageLoadEventArgs> OnHtmlImageNeeded;

        public static void OnHtmlImageLoad(HtmlImageLoadEventArgs e) {
            // load image from user
            if (OnHtmlImageNeeded != null) {
                OnHtmlImageNeeded(null, e);
                if (e.Handled)
                    return;
            }
            // load image from yamui library
            Image tryImg = (Image)Resources.Resources.ResourceManager.GetObject(e.Src);
            if (tryImg == null) return;
            e.Handled = true;
            e.Callback(tryImg);
        }

    }
}