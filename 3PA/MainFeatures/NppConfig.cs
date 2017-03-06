using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using _3PA.Lib;
using _3PA.Lib._3pUpdater;

namespace _3PA.MainFeatures {

    /// <summary>
    /// This class allows to get properties read from several config files of npp
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
            get {
                if (_stylers == null || Utils.HasFileChanged(FileNppStylersPath)) {
                    _stylers = new NppStylers(FileNppStylersPath);
                }
                return _stylers;
            }
        }

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public NppConfig() {

            var configs = XDocument.Load(Config.FileNppConfigXml).Descendants("GUIConfig").ToList();

            // FileNppStylersPath
            try {
                FileNppStylersPath = (string)configs.FirstOrDefault(x => x.Attribute("name").Value.Equals("stylerTheme")).Attribute("path");
                AutocompletionMode = (int)configs.FirstOrDefault(x => x.Attribute("name").Value.Equals("auto-completion")).Attribute("autoCAction");
            } catch (Exception) {
                FileNppStylersPath = null;
            }
            if (string.IsNullOrEmpty(FileNppStylersPath) || !File.Exists(FileNppStylersPath))
                FileNppStylersPath = Config.FileNppStylersXml;
        }
       
        #endregion

        #region public

        /// <summary>
        /// Ask the user to disable the default auto completion
        /// </summary>
        public void AskToDisableAutocompletion() {
            if (AutocompletionMode == 0 || Config.Instance.NeverAskToDisableDefaultAutoComp)
                return;

            var answer = UserCommunication.Message("3P (Progress Programmers Pal) <b>fully replaces the default auto-completion</b> offered by Notepad++ by a much better version.<br><br>If the default auto-completion isn't disabled, you will see 2 lists of suggestions!<br><br>I advise you to let 3P disable the default auto-completion now (restart required); otherwise, you can do it manually later", MessageImg.MsgInfo, "Auto-completion", "Deactivate default auto-completion now", new List<string> {"Yes, do it now", "No, never ask again", "I'll do it later myself"});
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
            
            Npp.Exit();
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
                    var widgetStyle = XDocument.Load(stylersXmlPath).Descendants("WidgetStyle");
                    var xElements = widgetStyle as XElement[] ?? widgetStyle.ToArray();
                    WhiteSpaceFg = GetColorInStylers(xElements, "White space symbol", "fgColor");
                    IndentGuideLineBg = GetColorInStylers(xElements, "Indent guideline style", "bgColor");
                    IndentGuideLineFg = GetColorInStylers(xElements, "Indent guideline style", "fgColor");
                    SelectionBg = GetColorInStylers(xElements, "Selected text colour", "bgColor");
                    CaretLineBg = GetColorInStylers(xElements, "Current line background colour", "bgColor");
                    CaretFg = GetColorInStylers(xElements, "Caret colour", "fgColor");
                    FoldMarginBg = GetColorInStylers(xElements, "Fold margin", "bgColor");
                    FoldMarginFg = GetColorInStylers(xElements, "Fold margin", "fgColor");
                    FoldMarginMarkerFg = GetColorInStylers(xElements, "Fold", "fgColor");
                    FoldMarginMarkerBg = GetColorInStylers(xElements, "Fold", "bgColor");
                    FoldMarginMarkerActiveFg = GetColorInStylers(xElements, "Fold active", "fgColor");

                } catch (Exception e) {
                    ErrorHandler.LogError(e);
                    if (!_warnedAboutFailStylers) {
                        _warnedAboutFailStylers = true;
                        UserCommunication.Notify("Error while reading one of Notepad++ file :<div>" + Config.FileNppStylersXml.ToHtmlLink() + "</div><br>The xml isn't correctly formatted, Npp manages to read anyway but you should correct it.", MessageImg.MsgError, "Error reading stylers.xml", "Xml read error");
                    }
                }
            }

            private static Color GetColorInStylers(IEnumerable<XElement> widgetStyle, string attributeName, string attributeToGet) {
                try {
                    return ColorTranslator.FromHtml("#" + (string)widgetStyle.First(x => x.Attribute("name").Value.Equals(attributeName)).Attribute(attributeToGet));
                } catch (Exception) {
                    return Color.Transparent;
                }
            }

            #endregion

        }

    }
}
