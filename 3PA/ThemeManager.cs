#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (ThemeManager.cs) is part of YamuiFramework.
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

using System.Collections.Generic;
using System.Drawing;
using YamuiFramework.Themes;
using _3PA.Data;
using _3PA.MainFeatures;

namespace _3PA {

    public static class ThemeManager {

        /// <summary>
        /// Return the current Theme object 
        /// </summary>
        public static Theme Current {
            get {
                if (_currentTheme == null) {
                    Current = GetThemesList().Find(theme => theme.UniqueId == Config.Instance.ThemeId) ?? GetThemesList()[0];
                }
                return _currentTheme;
            }
            set {
                _currentTheme = value;
                YamuiThemeManager.Current = _currentTheme;
                //ThemeManager.Current.GetThemeImage() =;
            }
        }
        private static Theme _currentTheme;

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        /// <returns></returns>
        public static List<Theme> GetThemesList() {
            if (_listOfThemes.Count == 0) {
                Class2Xml<Theme>.LoadFromRaw(_listOfThemes, DataResources.ThemesList, true);
            }
            return _listOfThemes;
        }
        private static List<Theme> _listOfThemes = new List<Theme>();

        /// <summary>
        /// Returns a list of accent colors to choose from
        /// </summary>
        public static Color[] GetAccentColors {
            get {
                return new[] {
                    Color.FromArgb(164, 196, 0),
                    Color.FromArgb(96, 169, 23),
                    Color.FromArgb(0, 138, 0),
                    Color.FromArgb(0, 171, 169),
                    Color.FromArgb(27, 161, 226),
                    Color.FromArgb(0, 80, 239),
                    Color.FromArgb(106, 0, 255),
                    Color.FromArgb(170, 0, 255),
                    Color.FromArgb(244, 114, 208),
                    Color.FromArgb(216, 0, 115),
                    Color.FromArgb(162, 0, 37),
                    Color.FromArgb(229, 20, 0),
                    Color.FromArgb(250, 104, 0),
                    Color.FromArgb(240, 163, 10),
                    Color.FromArgb(227, 200, 0),
                    Color.FromArgb(130, 90, 44),
                    Color.FromArgb(109, 135, 100),
                    Color.FromArgb(100, 118, 135),
                    Color.FromArgb(118, 96, 138),
                    Color.FromArgb(135, 121, 78)
                };
            }
        }

        public class Theme : YamuiTheme {

            // special for 3P
            public Color AutoCompletionNormalBackColor = Color.FromArgb(250, 250, 250);
            public Color AutoCompletionNormalAlternateBackColor = Color.FromArgb(230, 230, 230);
            public Color AutoCompletionNormalForeColor = Color.FromArgb(30, 30, 30);
            public Color AutoCompletionNormalSubTypeForeColor = Color.FromArgb(100, 154, 209);
            public Color AutoCompletionHoverBackColor = Color.FromArgb(206, 226, 252);
            public Color AutoCompletionHoverForeColor = Color.FromArgb(0, 0, 0);
            public Color AutoCompletionFocusBackColor = Color.FromArgb(154, 194, 249);
            public Color AutoCompletionFocusForeColor = Color.FromArgb(0, 0, 0);
            public Color AutoCompletionHighlightBack = Color.FromArgb(254, 228, 101);
            public Color AutoCompletionHighlightBorder = Color.FromArgb(255, 171, 0);

            public Color GenericLinkColor = Color.FromArgb(95, 158, 142);
            public Color GenericErrorColor = Color.OrangeRed;
        }
    }
}