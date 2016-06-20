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
using YamuiFramework.Helper;
using YamuiFramework.Themes;
using _3PA.Data;
using _3PA.Images;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.FileExplorer;

namespace _3PA {

    internal static class ThemeManager {

        #region Allows to initiate stuff 

        public static void OnStartUp() {
            YamuiThemeManager.OnCssNeeded += OnCssNeeded;
            YamuiThemeManager.OnImageNeeded += OnImageNeeded;
            Current.AccentColor = Config.Instance.AccentColor;
            YamuiThemeManager.TabAnimationAllowed = Config.Instance.AppliAllowTabAnimation;
            YamuiThemeManager.GlobalIcon = ImageResources._3p_icon;
        }

        #endregion

        #region Themes list

        private static Theme _currentTheme;
        private static List<Theme> _listOfThemes = new List<Theme>();

        /// <summary>
        /// Return the current Theme object 
        /// </summary>
        public static Theme Current {
            get {
                if (_currentTheme == null)
                    Current = GetThemesList.ElementAt(Config.Instance.ThemeId);
                return _currentTheme;
            }
            set {
                _currentTheme = value;
                try { 

                    YamuiThemeManager.Current = _currentTheme;
                    // we set the color for the YamuiTheme, but we also need to do it for the Theme...
                    _currentTheme.SetColorValues(typeof(Theme));

                } catch (Exception e) {
                    // either display the error immediatly or when the plugin is fully loaded...
                    if (Plug.PluginIsFullyLoaded)
                        ErrorHandler.ShowErrors(e, "Loading a theme");
                    else {
                        Plug.OnPlugReady += () => {
                            ErrorHandler.ShowErrors(e, "Loading a theme");
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of all available themes
        /// </summary>
        public static List<Theme> GetThemesList {
            get {
                // get the list of themes from the user's file or from the ressource by default
                if (_listOfThemes.Count == 0)
                    _listOfThemes = GenericThemeHolder.ReadThemeFile<Theme>(Config.FileApplicationThemes, DataResources.ApplicationThemes, Encoding.Default);
                if (Config.Instance.ThemeId < 0 || Config.Instance.ThemeId >= _listOfThemes.Count)
                    Config.Instance.ThemeId = 0;
                return _listOfThemes;
            }
        }

        /// <summary>
        /// Called when the list of themes is imported
        /// </summary>
        public static void ImportList() {
            _listOfThemes.Clear();
            _currentTheme = null;
            Current.AccentColor = Color.Empty;
            RefreshApplicationWithTheme(Current);
            Config.Instance.AccentColor = Current.AccentColor;
        }

        #endregion

        #region public

        /// <summary>
        /// force everything to redraw to apply a new theme
        /// </summary>
        public static void RefreshApplicationWithTheme(Theme theme) {
            Current = theme;
            Config.Instance.AccentColor = theme.AccentColor;
            Style.SetGeneralStyles();

            // force the autocomplete to redraw
            AutoComplete.ForceClose();
            CodeExplorer.ApplyColorSettings();
            FileExplorer.ApplyColorSettings();
            Application.DoEvents();
            Appli.Refresh();
        }

        /// <summary>
        /// Returns a formmatted html message with a title, subtitle and icon
        /// </summary>
        /// <param name="content"></param>
        /// <param name="image"></param>
        /// <param name="title"></param>
        /// <param name="subtitle"></param>
        /// <param name="forMessageBox"></param>
        /// <returns></returns>
        public static string FormatMessage(string content, MessageImg image, string title, string subtitle, bool forMessageBox = false) {
            return @"
            <div style='margin-bottom: 1px;'>
                <table style='margin-bottom: " + (forMessageBox ? "15px" : "5px") + @"; width: 100%'>
                    <tr>
                        <td rowspan='2' style='" + (forMessageBox ? "width: 95px; padding-left: 15px" : "width: 80px") + @"'><img src='" + image + @"' width='64' height='64' /></td>
                        <td class='NotificationTitle'><img src='" + GetLogo + @"' style='padding-right: 10px;'>" + title + @"</td>
                    </tr>
                    <tr>
                        <td class='NotificationSubTitle'>" + subtitle + @"</td>
                    </tr>
                </table>
                <div style='margin-left: 8px; margin-right: 8px; margin-top: 0px;'>
                    " + content + @"
                </div>
            </div>";
        }

        /// <summary>
        /// Returns the image of the logo (30x30)
        /// </summary>
        /// <returns></returns>
        public static string GetLogo {
            get { return "logo30x30"; }
        }

        #endregion

        #region private

        /// <summary>
        /// Event called when the YamuiFramework requests the background image,
        /// Tries to find the image in the ressources of the assembly, otherwise look for a file
        /// in the Config/3P/Themes folder
        /// </summary>
        private static Image OnImageNeeded(string imageToLoad) {
            Image tryImg = (Image) ImageResources.ResourceManager.GetObject(imageToLoad);
            if (tryImg == null) {
                var path = Path.Combine(Config.FolderThemes, imageToLoad);
                if (File.Exists(path))
                    tryImg = Image.FromFile(path);
            }
            return tryImg;
        }

        /// <summary>
        /// Called when the yamuiframework needs a css sheet
        /// </summary>
        private static string OnCssNeeded() {
            return Current.ReplaceAliasesByColor(DataResources.StyleSheet);
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

        }

        #endregion

    }

    #region Message image

    /// <summary>
    /// each value must correspond to an image in the ressources
    /// </summary>
    internal enum MessageImg {
        MsgDebug,
        MsgError,
        MsgHighImportance,
        MsgInfo,
        MsgOk,
        MsgPoison,
        MsgQuestion,
        MsgRip,
        MsgToolTip,
        MsgUpdate,
        MsgWarning
    }

    #endregion

}