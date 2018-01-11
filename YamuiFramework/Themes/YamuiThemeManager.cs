#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Text;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.Core.Core;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Themes {
    public static class YamuiThemeManager {
        #region public fields/events

        /// <summary>
        /// Set the icon to be used by every yamui form
        /// </summary>
        public static Icon GlobalIcon { get; set; }

        /// <summary>
        /// Set to false to deactivate tab animation
        /// </summary>
        public static bool TabAnimationAllowed { get; set; }

        /// <summary>
        /// Subscribe to this event to feed YamuiFramework with a css sheet of your making
        /// </summary>
        public static event Func<string> OnCssNeeded;

        /// <summary>
        /// Subscribe to this event to feed the YamuiFramework with an image to load in a img tag
        /// The needed image is in HtmlImageLoadEventArgs.src and you need to feed back the Image
        /// on HtmlImageLoadEventArgs.Callback(MyImage);
        /// </summary>
        public static event Func<string, Image> OnImageNeeded;

        #endregion

        #region private fields

        private static CssData _baseCssData;
        private static YamuiTheme _currentTheme;
        private static List<YamuiTheme> _listOfThemes;

        #endregion

        #region public

        /// <summary>
        /// Return the current Theme object 
        /// </summary>
        public static YamuiTheme Current {
            get {
                if (_currentTheme == null)
                    Current = GetThemesList[0];
                return _currentTheme;
            }
            set {
                _currentTheme = value;
                // compute the colors of the theme based on the their values found in the conf file
                _currentTheme.SetColorValues(typeof(YamuiTheme));

                // recompute the css sheet when it's needed
                CurrentThemeCss = null;

                // get the theme background image if any
                CurrentThemeImage = FindImage(_currentTheme.PageBackGroundImage);
            }
        }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<YamuiTheme> GetThemesList {
            get {
                if (_listOfThemes == null)
                    _listOfThemes = GenericThemeHolder.ReadThemeFile<YamuiTheme>(null, Resources.Resources.ApplicationThemes, Encoding.Default);
                return _listOfThemes;
            }
        }

        #endregion

        #region internal

        internal static Image CurrentThemeImage { private set; get; }

        /// <summary>
        /// Allows to add something to the Text property of every label
        /// </summary>
        internal static string WrapLabelText(string text) {
            return string.Format("<html>{0}</html>", text);
        }

        /// <summary>
        /// Allows to add something to the Text property of every label
        /// </summary>
        internal static string WrapToolTipText(string text) {
            return string.Format("<html><div class=\"yamui-tooltip\">{0}</div></html>", text);
        }

        /// <summary>
        /// Feeds the images to the html renderer
        /// </summary>
        internal static void GetHtmlImages(HtmlImageLoadEventArgs e) {
            Image img = FindImage(e.Src);
            if (img != null) {
                e.Callback(img);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event fired when the css sheet changed, allows the html label/panel to update themselves
        /// </summary>
        internal static event Action OnCssChanged;

        /// <summary>
        /// Feeds the CSS to the html renderer
        /// </summary>
        internal static CssData CurrentThemeCss {
            get {
                if (_baseCssData == null) {
                    // Get base css from the ressources
                    var baseCss = Resources.Resources.BaseStyleSheet;
                    baseCss = Current.ReplaceAliasesByColor(baseCss);

                    // load extra css from the user program
                    if (OnCssNeeded != null)
                        baseCss = string.Join("\r\n", baseCss, OnCssNeeded());

                    _baseCssData = HtmlRender.ParseStyleSheet(baseCss);
                }
                return _baseCssData;
            }
            set {
                // The css has changed
                if (_baseCssData != null && OnCssChanged != null) {
                    _baseCssData = value;
                    OnCssChanged();
                } else
                    _baseCssData = value;
            }
        }

        #endregion

        #region private

        /// <summary>
        /// Find the given image though the user's function or in the framework 
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        private static Image FindImage(string imageName) {
            Image img = null;
            if (!string.IsNullOrEmpty(imageName)) {
                // user custom function
                if (OnImageNeeded != null)
                    img = OnImageNeeded(imageName);

                // find in this framework
                if (img == null)
                    img = (Image) Resources.Resources.ResourceManager.GetObject(imageName);
            }
            return img;
        }

        #endregion
    }
}