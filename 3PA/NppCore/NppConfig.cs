#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppConfig.cs) is part of 3P.
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
using _3PA.Lib;
using _3PA.Lib._3pUpdater;
using _3PA.MainFeatures;

namespace _3PA.NppCore {
    /// <summary>
    /// This class allows to get properties read from several config files of npp :
    /// config.xml
    /// and stylers.xml (or whichever style .xml is used at the moment)
    /// </summary>
    internal class NppConfig {
        #region private

        private NppStylers _stylers;

        private static NppConfig _instance;

        #endregion

        #region Singleton

        /// <summary>
        /// Get instance
        /// </summary>
        public static NppConfig Instance {
            get {
                if (_instance == null || Utils.HasFileChanged(Config.FileNppConfigXml)) {
                    _instance = new NppConfig();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Allows to reload the npp configuration from the files
        /// </summary>
        public static void Reload() {
            _instance = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Path to the stylers.xml file currently used
        /// </summary>
        public string FileNppStylersPath { get; set; }

        /// <summary>
        /// Should be 0 to deactivate
        /// </summary>
        public int AutocompletionMode { get; set; }

        /// <summary>
        /// Stylers class
        /// </summary>
        public NppStylers Stylers {
            get { return _stylers ?? (_stylers = new NppStylers(FileNppStylersPath)); }
        }

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public NppConfig() {
            // Get info from the config.xml
            try {
                var configs = new NanoXmlDocument(Utils.ReadAllText(Config.FileNppConfigXml)).RootNode["GUIConfigs"].SubNodes;
                FileNppStylersPath = configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("stylerTheme")).GetAttribute("path").Value;
                AutocompletionMode = int.Parse(configs.FirstOrDefault(x => x.GetAttribute("name").Value.Equals("auto-completion")).GetAttribute("autoCAction").Value);
            } catch (Exception e) {
                FileNppStylersPath = null;
                ErrorHandler.LogError(e, "Error parsing " + Config.FileNppConfigXml);
            }

            if (string.IsNullOrEmpty(FileNppStylersPath) || !File.Exists(FileNppStylersPath))
                FileNppStylersPath = Config.FileNppStylersXml;

            // update the styles accordingly
            _stylers = null;
        }

        #endregion

        #region public

        /// <summary>
        /// Ask the user to disable the default auto completion
        /// </summary>
        public void AskToDisableAutocompletion() {
            if (AutocompletionMode == 0 || Config.Instance.NeverAskToDisableDefaultAutoComp)
                return;

            var answer = UserCommunication.Message("3P (Progress Programmers Pal) <b>fully replaces the default auto-completion</b> offered by Notepad++ by a much better version.<br><br>If the default auto-completion isn't disabled, you will see 2 lists of suggestions!<br><br>I advise you to let 3P disable the default auto-completion now (restart required); otherwise, you can do it manually later", MessageImg.MsgInfo, "Auto-completion", "Deactivate default auto-completion now", new List<string> {"Yes, restart now", "No, never ask again", "I'll do it later myself"});
            if (answer == 1)
                Config.Instance.NeverAskToDisableDefaultAutoComp = true;
            if (answer != 0)
                return;

            var encoding = TextEncodingDetect.GetFileEncoding(Config.FileNppConfigXml);
            var fileContent = Utils.ReadAllText(Config.FileNppConfigXml, encoding);
            fileContent = fileContent.Replace("autoCAction=\"3\"", "autoCAction=\"0\"");
            var configCopyPath = Path.Combine(Config.FolderUpdate, "config.xml");
            if (!Utils.FileWriteAllText(configCopyPath, fileContent, encoding))
                return;

            // replace default config by its copy on npp shutdown
            _3PUpdater.Instance.AddFileToMove(configCopyPath, Config.FileNppConfigXml);

            Npp.Restart();
        }

        #endregion

        #region NppStylers

        /// <summary>
        /// Class that holds some properties extracted from the stylers.xml file
        /// </summary>
        internal class NppStylers {
            private static bool _warnedAboutFailStylers;
            public Color WhiteSpaceFg { get; set; }
            public Color IndentGuideLineBg { get; set; }
            public Color IndentGuideLineFg { get; set; }
            public Color SelectionBg { get; set; }
            public Color CaretLineBg { get; set; }
            public Color CaretFg { get; set; }
            public Color FoldMarginBg { get; set; }
            public Color FoldMarginFg { get; set; }
            public Color FoldMarginMarkerFg { get; set; }
            public Color FoldMarginMarkerActiveFg { get; set; }
            public Color FoldMarginMarkerBg { get; set; }

            public NppStylers(string stylersXmlPath) {
                // read npp's stylers.xml file
                try {
                    var widgetStyle = new NanoXmlDocument(Utils.ReadAllText(stylersXmlPath)).RootNode["GlobalStyles"].SubNodes;
                    WhiteSpaceFg = GetColorInStylers(widgetStyle, "White space symbol", "fgColor");
                    IndentGuideLineBg = GetColorInStylers(widgetStyle, "Indent guideline style", "bgColor");
                    IndentGuideLineFg = GetColorInStylers(widgetStyle, "Indent guideline style", "fgColor");
                    SelectionBg = GetColorInStylers(widgetStyle, "Selected text colour", "bgColor");
                    CaretLineBg = GetColorInStylers(widgetStyle, "Current line background colour", "bgColor");
                    CaretFg = GetColorInStylers(widgetStyle, "Caret colour", "fgColor");
                    FoldMarginBg = GetColorInStylers(widgetStyle, "Fold margin", "bgColor");
                    FoldMarginFg = GetColorInStylers(widgetStyle, "Fold margin", "fgColor");
                    FoldMarginMarkerFg = GetColorInStylers(widgetStyle, "Fold", "fgColor");
                    FoldMarginMarkerBg = GetColorInStylers(widgetStyle, "Fold", "bgColor");
                    FoldMarginMarkerActiveFg = GetColorInStylers(widgetStyle, "Fold active", "fgColor");
                } catch (Exception e) {
                    ErrorHandler.LogError(e, "Error parsing " + stylersXmlPath);
                    if (!_warnedAboutFailStylers) {
                        _warnedAboutFailStylers = true;
                        UserCommunication.Notify("Error while reading one of Notepad++ file :<div>" + Config.FileNppStylersXml.ToHtmlLink() + "</div><br>The xml isn't correctly formatted, Npp manages to read anyway but you should correct it.", MessageImg.MsgError, "Error reading stylers.xml", "Xml read error");
                    }
                }
            }

            private static Color GetColorInStylers(List<NanoXmlNode> widgetStyle, string attributeName, string attributeToGet) {
                try {
                    return ColorTranslator.FromHtml("#" + widgetStyle.First(x => x.GetAttribute("name").Value.EqualsCi(attributeName)).GetAttribute(attributeToGet).Value);
                } catch (Exception) {
                    return Color.Transparent;
                }
            }

            #endregion
        }
    }
}