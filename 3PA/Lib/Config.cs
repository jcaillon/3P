#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Config.cs) is part of 3P.
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
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using YamuiFramework.Forms;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    #region config Object

    /// <summary>
    /// The config object, should not be used
    /// </summary>
    public class ConfigObject {
        // https://msdn.microsoft.com/en-us/library/dd901590(VS.95).aspx
        [Display(Name = "User trigram", Description = "This is your user trigram duh")] public string UserTrigram = LocalEnv.Instance.GetTrigramFromPa();

        // is the user from SOPRA?
        public bool UserFromSopra = !string.IsNullOrEmpty(LocalEnv.Instance.GetTrigramFromPa());

        public bool UserGetsPreReleases = true;

        public bool UseDefaultValuesInsteadOfLastValuesInEditTags = false;

        public int AutoCompleteStartShowingListAfterXChar = 1;
        public bool AutoCompleteUseTabToAccept = true;
        public bool AutoCompleteUseEnterToAccept = false;

        public bool AutoCompleteReplaceSemicolon = true;
        public bool AutoCompleteInsertEndAfterDo = true;

        public bool AutoCompleteShowInCommentsAndStrings = false;
        public int AutoCompleteIndentNbSpaces = 4;
        public int AutoCompleteShowListOfXSuggestions = 12;
        public double AutoCompleteUnfocusedOpacity = 0.92d;
        public double AutoCompleteFocusedOpacity = 0.92d;
        public int AutoCompleteChangeCaseMode = 1; // 0 = inactive, 1 = upper, 2 = lower, 3 = camel
        public bool AutoCompleteAlternateBackColor = false;

        public bool AutoCompleteOnKeyInputShowSuggestions = true;
        public bool AutoCompleteOnKeyInputHideIfEmpty = true;
        public bool AutoCompleteInsertSelectedSuggestionOnWordEnd = false;
        public bool AutoCompleteHideScrollBar = true;
        public bool AutocompleteReplaceAbbreviations = true;


        [Display(Name = "Display priority list", Description = "Defines the order in which the CompletionType are displayed")] public string AutoCompletePriorityList = "11,2,4,5,3,6,7,8,10,13,9,12,14,0,1";


        public bool CodeExplorerUseAlternateColors = false;
        public bool CodeExplorerVisible = true;
        [Display(Name = "Display priority list", Description = "Defines the order in which the ExplorerType are displayed")] public string CodeExplorerPriorityList = "0,1,2,12,6,3,4,5,7,8,9,10,11";
        public bool CodeExplorerDisplayExternalItems = false;

        public double AppliOpacityUnfocused = 0.5;
        public bool AppliAllowTabAnimation = true;


        public int ToolTipmsBeforeShowing = 1000;
        public double ToolTipUnfocusedOpacity = 0.9;
        public double ToolTipFocusedOpacity = 0.9;
        public bool ToolTipDeactivate = false;


        public string GlobalProgressExtension = ".p,.i,.w,.t,.ds,.lst";
        public int GlobalCurrentEnvironnement = 0;
        public int GlobalMaxNbCharInBlock = 31190;
        public bool GlobalShowNotifAboutDefaultAutoComp = true;
        public bool GlobalShowAllError = true;

        public string EnvCurrentAppli = "";
        public string EnvCurrentEnvLetter = "";
        public string EnvCurrentDatabase = "";


        public int ThemeId = 1;
        public Color AccentColor = Color.DarkOrange;

        public int SyntaxHighlightThemeId = 0;

        public Dictionary<string, string> ShortCuts = new Dictionary<string, string>();
    }

    #endregion


    /// <summary>
    /// Holds the configuration of the application, this class is a singleton and
    /// you should call it like this : Config.Instance.myparam
    /// </summary>
    public static class Config {

        #region private fields

        private static ConfigObject _instance;
        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileName = "settings.xml";

        #endregion

        #region public fields

        /// <summary>
        /// Url to request to get info on the latest releases
        /// </summary>
        public static string ReleasesUrl {
            get { return @"https://api.github.com/repos/jcaillon/3P/releases"; }
        }

        /// <summary>
        /// Singleton instance of ConfigObject
        /// </summary>
        public static ConfigObject Instance {
            get { return _instance ?? (_instance = Init()); }
        }

        #endregion

        #region mechanics

        /// <summary>
        /// init the instance by either reading the values from an existing file or 
        /// creating one form the default values of the object
        /// </summary>
        /// <returns></returns>
        private static ConfigObject Init() {
            _instance = new ConfigObject();
            _filePath = Path.Combine(_location, _fileName);
            if (File.Exists(_filePath)) {
                try {
                    Object2Xml<ConfigObject>.LoadFromFile(_instance, _filePath);
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error when loading settings", _filePath);
                }
            }
            SetupFileWatcher();
            return _instance;
        }

        /// <summary>
        /// Call this method to save the content of the config.instance into an .xml file
        /// </summary>
        public static void Save() {
            try {
                if (!string.IsNullOrWhiteSpace(_filePath))
                    Object2Xml<ConfigObject>.SaveToFile(_instance, _filePath);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when saving settings");
            }
        }

        private static FileSystemWatcher _configWatcher;

        private static void SetupFileWatcher() {
            _configWatcher = new FileSystemWatcher(_location, _fileName) {NotifyFilter = NotifyFilters.LastWrite};
            _configWatcher.Changed += configWatcher_Changed;
        }

        private static void configWatcher_Changed(object sender, FileSystemEventArgs e) {
            UserCommunication.Notify("Config changed", MessageImage.Ok, "EVENT", "File changed");
            Init();
        }

        #endregion

    }
}