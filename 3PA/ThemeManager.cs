#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ThemeManager.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using YamuiFramework.Themes;
using _3PA.Data;
using _3PA.Images;
using _3PA.MainFeatures;

namespace _3PA {

    public static class ThemeManager {

        /// <summary>
        /// Return the current Theme object 
        /// </summary>
        public static Theme Current {
            get {
                if (_currentTheme == null) {
                    Current = GetThemesList().ElementAt(Config.Instance.ThemeId) ?? GetThemesList()[0];
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

                // get background image event
                YamuiTheme.OnImageNeeded = YamuiThemeOnOnImageNeeded;


            }
            return _listOfThemes;
        }

        private static List<Theme> _listOfThemes = new List<Theme>();

        /// <summary>
        /// Event called when the YamuiFramework requests the background image,
        /// Tries to find the image in the ressources of the assembly, otherwise look for a file
        /// in the Config/3P/Themes folder
        /// </summary>
        private static Image YamuiThemeOnOnImageNeeded(string imageToLoad) {
            try {
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(imageToLoad);
                if (tryImg != null)
                    return tryImg;
                var path = Path.Combine(Config.FolderThemes, imageToLoad);
                if (File.Exists(path)) {
                    return Image.FromFile(path);
                }
            } catch (Exception e) {
                ErrorHandler.Log(e.ToString());
            }
            return null;
        }

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
            public Color AutoCompletionHighlightBack = Color.FromArgb(254, 228, 101);
            public Color AutoCompletionHighlightBorder = Color.FromArgb(255, 171, 0);

            public Color GenericLinkColor = Color.FromArgb(95, 158, 142);
            public Color GenericErrorColor = Color.OrangeRed;
        }
    }
}