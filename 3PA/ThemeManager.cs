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
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Themes;
using _3PA.Data;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.FileExplorer;

namespace _3PA {

    public static class ThemeManager {

        #region Themes list

        /// <summary>
        /// Return the current Theme object 
        /// </summary>
        public static Theme Current {
            get {
                if (_currentTheme == null) {
                    Current = GetThemesList().ElementAt(Config.Instance.ThemeId);
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
                Theme curTheme = null;
                ConfLoader.ForEachLine(Config.FileApplicationThemes, DataResources.ApplicationThemes, Encoding.Default, s => {
                    // beggining of a new theme, read its name
                    if (s.Length > 2 && s[0] == '>') {
                        _listOfThemes.Add(new Theme());
                        curTheme = _listOfThemes.Last();
                        curTheme.ThemeName = s.Substring(2).Trim();
                    }
                    if (curTheme == null)
                        return;

                    // fill the theme
                    var items = s.Split('\t');
                    if (items.Count() == 2) {
                        curTheme.SetValueOf(items[0].Trim(), items[1].Trim());
                    }
                });

                // get background image event
                YamuiTheme.OnImageNeeded = YamuiThemeOnOnImageNeeded;
            }

            if (Config.Instance.SyntaxHighlightThemeId < 0 || Config.Instance.SyntaxHighlightThemeId >= _listOfThemes.Count)
                Config.Instance.SyntaxHighlightThemeId = 0;

            return _listOfThemes;
        }

        private static List<Theme> _listOfThemes = new List<Theme>();

        #endregion

        #region public

        /// <summary>
        /// force verything to redraw to apply a new theme
        /// </summary>
        public static void PlsRefresh() {

            // Allows to refresh stuff corrrectly (mainly, it sets the baseCssData to null so it can be recomputed)
            Current = Current;

            Style.SetGeneralStyles();

            // force the autocomplete to redraw
            AutoComplete.ForceClose();

            // force the dockable to redraw
            CodeExplorer.ApplyColorSettings();
            FileExplorer.ApplyColorSettings();

            Application.DoEvents();
            Appli.Refresh();
        }

        public static void ImportList() {
            _listOfThemes.Clear();
            _currentTheme = null;
            PlsRefresh();
        }

        #endregion


        #region private

        /// <summary>
        /// Event called when the YamuiFramework requests the background image,
        /// Tries to find the image in the ressources of the assembly, otherwise look for a file
        /// in the Config/3P/Themes folder
        /// </summary>
        private static Image YamuiThemeOnOnImageNeeded(string imageToLoad) {
            try {
                Image tryImg = (Image) ImageResources.ResourceManager.GetObject(imageToLoad);
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

        #endregion

        #region List of accent colors

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

        #endregion

        #region Theme class

        public class Theme : YamuiTheme {

            // special for 3P
            public Color AutoCompletionHighlightBack = Color.FromArgb(254, 228, 101);
            public Color AutoCompletionHighlightBorder = Color.FromArgb(255, 171, 0);

            public Color GenericLinkColor = Color.FromArgb(95, 158, 142);
            public Color GenericErrorColor = Color.OrangeRed;


            /// <summary>
            /// Set a value to this instance, by its property name
            /// </summary>
            /// <param name="propertyName"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool SetValueOf(string propertyName, object value) {
                var property = typeof (Theme).GetFields().FirstOrDefault(info => info.Name.Equals(propertyName));
                if (property == null) {
                    return false;
                }

                if (property.FieldType == typeof (Color)) {
                    property.SetValue(this, ColorTranslator.FromHtml((string) value));
                } else {
                    property.SetValue(this, value);
                }
                return true;
            }
        }

        #endregion

    }
}