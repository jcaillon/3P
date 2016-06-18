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
                if (_currentTheme == null)
                    Current = GetThemesList().ElementAt(Config.Instance.ThemeId);
                return _currentTheme;
            }
            set {
                _currentTheme = value;
                _currentTheme.ComputeColorValues();
                YamuiThemeManager.Current = _currentTheme;
            }
        }

        private static Theme _currentTheme;

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        public static List<Theme> GetThemesList() {

            if (_listOfThemes.Count == 0) {

                Theme curTheme = null;
                
                // the dico below will contain key -> values of the theme
                var valuesDictionnary = new Dictionary<string, string>();

                ConfLoader.ForEachLine(Config.FileApplicationThemes, DataResources.ApplicationThemes, Encoding.Default, s => {

                    // beggining of a new theme, read its name
                    if (s.Length > 2 && s[0] == '>') {
                        // Set the current them with the values we keep in the dico
                        if (curTheme != null)
                            curTheme.SetStringValues(valuesDictionnary);

                        _listOfThemes.Add(new Theme());
                        curTheme = _listOfThemes.Last();
                        curTheme.ThemeName = s.Substring(2).Trim();
                    } else if (curTheme == null)
                        return;

                    // fill the theme's dico
                    var items = s.Split('\t');
                    if (items.Count() == 2) {
                        var name = items[0].Trim();
                        if (!valuesDictionnary.ContainsKey(name))
                            valuesDictionnary.Add(name, items[1].Trim());
                        else
                            valuesDictionnary[name] = items[1].Trim();
                    }
                });

                // Set the current them with the values we keep in the dico
                if (curTheme != null)
                    curTheme.SetStringValues(valuesDictionnary);

                // get background image event
                if (YamuiTheme.OnImageNeeded == null)
                    YamuiTheme.OnImageNeeded = YamuiThemeOnOnImageNeeded;
            }

            if (Config.Instance.ThemeId < 0 || Config.Instance.ThemeId >= _listOfThemes.Count)
                Config.Instance.ThemeId = 0;

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

        /// <summary>
        /// Called when the list of themes is imported
        /// </summary>
        public static void ImportList() {
            _listOfThemes.Clear();
            _currentTheme = null;
            Current = Current;
            Current.AccentColor = Current.ThemeAccentColor;
            Config.Instance.AccentColor = Current.ThemeAccentColor;
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

            private Dictionary<string, string> _savedStringValues;

            /// <summary>
            /// Saves the info extracted from the .conf file for this instance, allows to recompute the Color values later
            /// </summary>
            /// <param name="values"></param>
            public void SetStringValues(Dictionary<string, string> values) {
                _savedStringValues = new Dictionary<string, string> {{"AccentColor", ""}};
                foreach (var kpv in values) 
                    _savedStringValues.Add(kpv.Key, kpv.Value);
            }

            /// <summary>
            /// Set the values of this instance, using a dictionnary of key -> values
            /// </summary>
            public void ComputeColorValues() {
                // update AccentColor
                _savedStringValues["AccentColor"] = ColorTranslator.ToHtml(Config.Instance.AccentColor);

                // for each field of this object, try to assign its value with the _savedStringValues dico
                foreach (var fieldInfo in typeof(Theme).GetFields()) {
                    if (_savedStringValues.ContainsKey(fieldInfo.Name)) {
                        try {
                            var value = _savedStringValues[fieldInfo.Name];
                            if (fieldInfo.FieldType == typeof(Color)) {
                                fieldInfo.SetValue(this, FindHtmlColor(value).GetColorFromHtml());
                            } else {
                                fieldInfo.SetValue(this, value);
                            }
                        } catch (Exception) {
                            ErrorHandler.Log("Couldn't convert the color : " + _savedStringValues[fieldInfo.Name].ProgressQuoter() + " for the field " + fieldInfo.Name.ProgressQuoter() + " and theme " + ThemeName.ProgressQuoter(), true);
                        }
                    }
                }
            }

            /// <summary>
            /// Allows to replace the links (@PropertyName) by their values 
            /// </summary>
            private string FindHtmlColor(string value) {
                if (value.ContainsFast("@")) {
                    // try to replace a variable name by it's html color value
                    value = value.RegexReplace(@"@([a-zA-Z]*)", match => {
                        if (_savedStringValues.ContainsKey(match.Groups[1].Value))
                            return _savedStringValues[match.Groups[1].Value];
                        throw new Exception("Couldn't find the color " + match.Groups[1].Value + "!");
                    });
                    return FindHtmlColor(value);
                }
                return value;
            }

        }

        #endregion

    }
}