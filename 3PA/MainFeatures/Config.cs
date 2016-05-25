#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Linq;
using System.Net;
using _3PA.Lib;

// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures {

    /// <summary>
    /// Holds the configuration of the application, this class is a singleton and
    /// you should call it like this : Config.Instance.myparam
    /// </summary>
    internal static class Config {

        #region config Object

        /// <summary>
        /// The config object, should not be used
        /// Each field can have display attributes, they are used in the options pages to automatically
        /// generates the pages
        /// Set AutoGenerateField to refresh certain options that need special treatment to appear changed (see option page)
        /// </summary>
        public class ConfigObject {

            //[StringLength(15)]
            //[RegularExpression(@"^\$?\d+(\.(\d{2}))?$")]

            #region GENERAL

            /// <summary>
            /// GENERAL
            /// </summary>

            [Display(Name = "User name",
                Description = "Used for modification tags",
                GroupName = "General",
                AutoGenerateField = false)]
            public string UserName = GetTrigramFromPa();

            [Display(Name = "Use simple right click for the main menu",
                Description = "Toggle this option on to replace the default right of Notepad++ by 3P's main menu<br>You then can access the default Notepad++ context menu with CTRL + Right click",
                GroupName = "General",
                AutoGenerateField = false)]
            public bool AppliSimpleRightClickForMenu = false;

            [Display(Name = "Progress 4GL files extension list",
                Description = "A comma separated list of valid progress file extensions : <br>It is used to check if you can activate a 3P feature on the file currently opened",
                GroupName = "General",
                AutoGenerateField = false)]
            public string KnownProgressExtension = ".p,.i,.w,.t,.d,.lst,.df,.cls";

            [Display(Name = "Npp openable extension",
                Description = "A comma separated list of file extensions, describes the type of files that should be opened with notepad++ from the file explorer<br>If a file is associated to npp in the shell, it will also be opened with npp, no worries!",
                GroupName = "General",
                AutoGenerateField = false)]
            public string GlobalNppOpenableExtension = ".txt,.boi,.sh,.cmd";

            [Display(Name = "Path to the help file",
                Description = "Should point to the progress documentation file (lgrfeng.chm)",
                GroupName = "General",
                AutoGenerateField = false)]
            public string GlobalHelpFilePath = "";

            [Display(Name = "Use batch mode to compile",
                Description = "In order to avoid the prowin32 splash screen when compiling, 3P starts the process in<br>batch mode (-b option).<br>However, if you manually disabled the splash screen (by renaming the file<br>/dlc/bin/splashscreen.bmp for instance), you can set this option to false to avoid<br>having the prowin32 process creating a window in the taskbar",
                GroupName = "General",
                AutoGenerateField = false)]
            public bool UseBatchModeToCompile = true;

            [Display(Name = "Use default values in file info",
                Description = "Set to true and the <b>default</b> option will be selected when you open a new file info,<br>set to false and the option <b>last values</b> will be selected",
                GroupName = "General",
                AutoGenerateField = false)]
            public bool UseDefaultValuesInsteadOfLastValuesInEditTags = false;

            [Display(Name = "Always show a notification after a compilation",
                Description = "Whether or not to systematically show a notification after a compilation<br>By default, a notification is shown if notepad++ doesn't have the focus or if they are errors",
                GroupName = "General",
                AutoGenerateField = false)]
            public bool CompileAlwaysShowNotification = true;


            [Display(Name = "Use alternate back color for lists",
                Description = "Use alternate back color for the autocompletion, the code explorer, the file explorer and so on...",
                GroupName = "General",
                AutoGenerateField = true)]
            public bool GlobalUseAlternateBackColorOnGrid = false;

            [Display(Name = "Application focused opacity",
                Description = "Set the opacity that the main application window will have when activated",
                GroupName = "General",
                AutoGenerateField = false)]
            [Range(0.1, 1)]
            public double AppliOpacityUnfocused = 1;

            [Display(Name = "Allow tab animation",
                Description = "Allow the main application window to animate the transition between pages with a fade in / fade out animation",
                GroupName = "General",
                AutoGenerateField = true)]
            public bool AppliAllowTabAnimation = true;

            public bool GlobalShowDetailedHelpForErrors = true;
            public bool GlobalCompileFilesLocally = false;
            public bool UserFirstUse = true;

            #endregion

            #region COMPILATION

            [Display(Name = "Compilable files extension list",
                Description = "A comma separated list of progress file extensions that can be compiled : <br>It is used to check if you can compile / check syntax / execute the current file",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public string CompileKnownExtension = ".p,.w,.t,.cls";

            [Display(Name = "Compile with .lst",
                Description = "Toggle on to compile your code with the listing option, generating a .lst file<br>see the DEBUG-LIST option of the Progress compiler for more info",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public bool CompileWithLst = true;

            [Display(Name = "Number of processes by core",
                Description = "This parameter is used when compiling multiple files, it determines how many<br>Prowin processes can be started to handle compilation<br>The total number of processes started is actually multiplied by your number of cores<br><br>Be aware that as you increase the number or processes for the compilation, you<br>decrease the potential time of compilation but you also increase the number of connection<br>needed to your database (if you have one defined!)<br>You might have an error on certain processes that can't connect to the database<br>if you try to increase this number too much",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public int NbOfProcessesByCore = 3;

            [Display(Name = "Force mono process for mass compilation",
                Description = "Toggle on to only use a single process when compiling multiple files through the mass compiler page<br>Obviously, this will slow down the process by a lot<br>You can use this option if you want to limit the number of connections made to your database during compilation...",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public bool CompileForceMonoProcess = false;

            public string CompileDirectoriesHistoric = "";

            public bool CompileExploreDirRecursiv = true;

            #endregion

            #region UPDATES

            [Display(Name = "Do not check for updates",
                Description = "Check this option to prevent 3P from fetching the latest version on github<br><b>You will not have access to the latest features and will not enjoy bug corrections!</b>",
                GroupName = "Updates",
                AutoGenerateField = false)]
            public bool GlobalDontCheckUpdates = false;

            [Display(Name = "I want to get beta releases",
                Description = "Check this option if you want to update 3P with the latest beta version <b>(i.e. NOT STABLE)</b><br>Otherwise, you will only have update notifications for stable releases",
                GroupName = "Updates",
                AutoGenerateField = false)]
            public bool UserGetsPreReleases = AssemblyInfo.IsPreRelease;

            [Display(Name = "Do not automatically post .log file",
                Description = "Check this option to prevent 3P from sending your error.log file automatically on github<br><b>Doing this slows the debugging process for 3P's developpers as bugs are not detected if you don't create an issue!</b>",
                GroupName = "Updates",
                AutoGenerateField = false)]
            public bool GlobalDontAutoPostLog = false;

            [Display(Name = "Do not install syntax highlighting on update",
                Description = "Check this option to prevent 3P from installing the latest syntax highlighting on soft update<br><b>Please let this option unckecked if you are not sure what it does or you will miss on new features!</b>",
                GroupName = "Updates",
                AutoGenerateField = false)]
            public bool GlobalDontUpdateUdlOnUpdate = false;

            #endregion

            #region AUTOCOMPLETION

            /// <summary>
            /// AUTOCOMPLETION
            /// </summary>

            [Display(Name = "Show autocompletion on key input",
                Description = "Automatically show the autocompletion list when you start entering characters",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            public bool AutoCompleteOnKeyInputShowSuggestions = true;

            [Display(Name = "Start showing after X char",
                Description = "If you chose to display the list on key input,<br> you can set the minimum number of char necessary before showing the list ",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            [Range(1, 99)]
            public int AutoCompleteStartShowingListAfterXChar = 1;

            [Display(Name = "Hide autocompletion if empty",
                Description = "If the list was displayed automatically and there are no suggestions matching your input,<br>this option will automatically hide the list instead of showing it empty",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            public bool AutoCompleteOnKeyInputHideIfEmpty = true;

            [Display(Name = "Show list in comments and strings",
                Description = "By default, the autocompletion list is hidden in comments and strings<br>you can still show the completion list manually!",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            public bool AutoCompleteShowInCommentsAndStrings = false;

            [Display(Name = "Use TAB to accept a suggestion",
                Description = "Whether or not to allow the TAB key to accept the suggestion",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            public bool AutoCompleteUseTabToAccept = true;

            [Display(Name = "User ENTER to accept a suggestion",
                Description = "Whether or not to allow the ENTER key to accept the suggestion",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            public bool AutoCompleteUseEnterToAccept = false;

            [Display(Name = "Number of suggestions",
                Description = "The number of suggestions shown in the list",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            [Range(3, 20)]
            public int AutoCompleteShowListOfXSuggestions = 12;

            [Display(Name = "Insert current suggestion on word end",
                Description = "You can check this option to automatically insert the currently selected suggestion<br>(if the list is opened)<br>when you enter any character that is not a letter/digit/_/-",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            public bool AutoCompleteInsertSelectedSuggestionOnWordEnd = true;

            [Display(Name = "Only show already defined variables",
                Description = "By default, 3P filters the auto-completion list to only show you<br>the items that are available at the line where you activate<br>the auto-completion list.<br>You can set this option to false to show an item even if,<br>for the line where your cursor is, it is not yet defined.",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            public bool AutoCompleteOnlyShowDefinedVar = true;

            [Display(Name = "Hide list scroll bar",
                Description = "Check to remove the scrollbar, you can still move up/down with arrow keys<br>and next/previous pages, this is only to have a prettier list in dark themes",
                GroupName = "Auto-completion",
                AutoGenerateField = false)]
            public bool AutoCompleteHideScrollBar = false;

            [Display(Name = "Unfocused opacity",
                Description = "The opacity of the list when unfocused",
                GroupName = "Auto-completion",
                AutoGenerateField = true)]
            [Range(0.1, 1)]
            public double AutoCompleteUnfocusedOpacity = 0.92;

            [Display(Name = "Focused opacity",
                Description = "The opacity of the list when focused",
                GroupName = "Auto-completion",
                AutoGenerateField = true)]
            [Range(0.1, 1)]
            public double AutoCompleteFocusedOpacity = 0.92;

            public string AutoCompletePriorityList = "2,4,5,12,7,8,11,3,6,15,9,14,10,13,0,1";

            #endregion

            #region CODE EDITION

            /// <summary>
            /// CODE EDITION
            /// </summary>


            [Display(Name = "Auto-case mode",
                Description = "When you finished entering a keyword, it can be automatically be :<br>UPPERCASED (1), lowercased (2) or CamelCased (3)<br>Set to 0 to deactivate",
                GroupName = "Code edition",
                AutoGenerateField = false)]
            [Range(0, 3)]
            public int CodeChangeCaseMode = 1; // 0 = inactive, 1 = upper, 2 = lower, 3 = camel

            [Display(Name = "Auto replace abbreviations",
                Description = "Automatically replaces abbreviations by their full lenght counterparts",
                GroupName = "Code edition",
                AutoGenerateField = false)]
            public bool CodeReplaceAbbreviations = true;

            [Display(Name = "Auto replace semicolon",
                Description = "Check to replace automatically ; by . <br><i>useful if you come from any other language!!!</i>",
                GroupName = "Code edition",
                AutoGenerateField = false)]
            public bool CodeReplaceSemicolon = true;

            [Display(Name = "Max number of characters in a block",
                Description = "The appbuilder is limited in the number of character that a block (procedure, function...) can contain<br>This value allows to show a warning when you overpass the limit in notepad++",
                GroupName = "Code edition",
                AutoGenerateField = false)]
            public int GlobalMaxNbCharInBlock = 31190;

            [Display(Name = "Tab width",
                Description = "The number of spaces that will be inserted when you press TAB and re-indent the code",
                GroupName = "Code edition",
                AutoGenerateField = true)]
            [Range(0, 10)]
            public int CodeTabSpaceNb = 4;

            [Display(Name = "Activate 'Show spaces' on progress files",
                Description = "Toggle on/off to activate the 'show spaces' notepad++ option when switching to a progress file<br>This option let the user see the spaces as dots and tabs as arrows in its document",
                GroupName = "Code edition",
                AutoGenerateField = false)]
            public bool CodeShowSpaces = true;
            

            #endregion

            #region FILE EXPLORER

            /// <summary>
            /// FILE EXPLORER
            /// </summary>
            /// 
            public bool FileExplorerVisible = true;

            [Display(Name = "Ignore unix hidden folder",
                Description = "Check to ignore all the files/folders starting with a dot '.'",
                GroupName = "File explorer",
                AutoGenerateField = false)]
            public bool FileExplorerIgnoreUnixHiddenFolders = true;

            [Display(Name = "Auto-hide/show for progress documents",
                Description = "Check this option to automatically hide the File explorer when the current isn't<br>a progress file, and automatically show it when it is",
                GroupName = "File explorer",
                AutoGenerateField = false)]
            public bool FileExplorerAutoHideOnNonProgressFile = false;

            [Display(Name = "File listing time out",
                Description = "The maximum time given to 3P to list the files of your current directory<br>This option is here to limit the amount of time spent to build<br>a list of you files. This time can be huge if you select<br>a folder with a lot of files or if you select a folder on a windows server",
                GroupName = "File explorer",
                AutoGenerateField = false)]
            [Range(1000, 30000)]
            public int FileExplorerListFilesTimeOutInMs = 3000;

            #endregion

            #region CODE EXPLORER

            /// <summary>
            /// CODE EXPLORER
            /// </summary>

            public bool CodeExplorerVisible = true;
            public string CodeExplorerPriorityList = "0,1,2,12,6,3,4,5,7,8,9,10,11,13";
            public bool CodeExplorerDisplayExternalItems = false;

            [Display(Name = "Auto-hide/show for progress documents",
                Description = "Check this option to automatically hide the Code explorer when the current isn't<br>a progress file, and automatically show it when it is",
                GroupName = "Code explorer",
                AutoGenerateField = false)]
            public bool CodeExplorerAutoHideOnNonProgressFile = false;

            #endregion

            #region TOOLTIP

            /// <summary>
            /// TOOLTIPS
            /// </summary>

            [Display(Name = "Deactivate all tooltips",
                Description = "Don't do that, it would be a shame to not use them!",
                GroupName = "Tooltip",
                AutoGenerateField = false)]
            public bool ToolTipDeactivate = false;

            [Display(Name = "Idle time to spawn",
                Description = "The amount of time in milliseconds that you have to left your<br>mouse over a word before it shows its tooltip",
                GroupName = "Tooltip",
                AutoGenerateField = true)]
            [Range(0, 5000)]
            public int ToolTipmsBeforeShowing = 500;

            [Display(Name = "Opacity",
                Description = "The tooltip opacity",
                GroupName = "Tooltip",
                AutoGenerateField = true)]
            [Range(0.1, 1)]
            public double ToolTipOpacity = 0.92;

            #endregion

            // SHARED CONF
            public string SharedConfFolder = "";
            // a list of Label corresponding to confLine(s) that are auto-updated
            public string AutoUpdateConfList = "";
            // a list of folders used in the shared/exported conf
            public string SharedConfHistoric = "";

            // TAGS
            public string TagModifOpener = "/* --- Modif #{&n} --- {&da} --- CS PROGRESS SOPRA ({&u}) --- [{&w} - {&b}] --- */";
            public string TagModifCloser = "/* --- Fin modif #{&n} --- */";
            public string TagTitleBlock1 = 
                "/* |      |            |           |                                                                | */\n" +
                "/* | {&n }| {&da     } | CS-SOPRA  | {&w } - {&b                                                  } | */\n" +
                "/* |      | {&v      } | {&u     } |                                                                | */";
            public string TagTitleBlock2 =
                "/* |      |            |           | {&de                                                         } | */";

            public string TagTitleBlock3 =
                "/* |______|____________|___________|________________________________________________________________| */";

            // ENV
            public string EnvLastDbInfoUsed = "";
            public string EnvName = "";
            public string EnvSuffix = "";
            public string EnvDatabase = "";

            // TECHNICAL
            public string TechnicalLastPing = "";
            public bool LastCheckUpdateOk = true;

            // THEMES
            public int ThemeId = 0;
            public Color AccentColor = ColorTranslator.FromHtml("#647687");
            public int SyntaxHighlightThemeId = 1;
            public bool GlobalDontUseSyntaxHighlightTheme = false;
            public bool GlobalOverrideNppTheme = true;

            // SHORTCUTS (id, spec)
            public Dictionary<string, string> ShortCuts = new Dictionary<string, string>();

            /// <summary>
            /// Returns the proxy defined by the user in the configuration
            /// </summary>
            /// <returns></returns>
            public IWebProxy GetWebClientProxy() {
                IWebProxy proxy;
                //proxy = new WebProxy {
                //    Address = new Uri("http://8.8.8.8:2015/"),
                //    Credentials = new NetworkCredential("usernameHere", "pa****rdHere"),
                //    UseDefaultCredentials = false,
                //    BypassProxyOnLocal = false
                //};
                proxy = WebRequest.DefaultWebProxy;
                proxy.Credentials = CredentialCache.DefaultCredentials;
                return proxy;
            }

            /// <summary>
            /// Get a value from this instance, by its property name
            /// </summary>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            public object GetValueOf(string propertyName) {
                var property = typeof(ConfigObject).GetFields().FirstOrDefault(info => info.Name.Equals(propertyName));
                if (property == null) {
                    return null;
                }
                return property.GetValue(this);
            }

            /// <summary>
            /// Set a value to this instance, by its property name
            /// </summary>
            /// <param name="propertyName"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool SetValueOf(string propertyName, object value) {
                var property = typeof(ConfigObject).GetFields().FirstOrDefault(info => info.Name.Equals(propertyName));
                if (property == null) {
                    return false;
                }
                property.SetValue(this, value);
                //var converter = TypeDescriptor.GetConverter(property.FieldType);
                //property.SetValue(this, converter.);
                return true;
            }

            /// <summary>
            /// Gets the DisplayAttribute of the given property
            /// </summary>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            public T GetAttributeOf<T>(string propertyName) {
                var property = typeof(ConfigObject).GetFields().FirstOrDefault(info => info.Name.Equals(propertyName));
                if (property == null) {
                    return (T)Convert.ChangeType(null, typeof(T));
                }
                var listCustomAttr = property.GetCustomAttributes(typeof(T), false);
                if (!listCustomAttr.Any()) {
                    return (T)Convert.ChangeType(null, typeof(T));
                }
                var displayAttr = (T)listCustomAttr.FirstOrDefault();
                return displayAttr;
            }

            /// <summary>
            /// Returns the shortcut specs corresponding to the itemId given
            /// </summary>
            public string GetShortcutSpecFromName(string itemId) {
                return (ShortCuts.ContainsKey(itemId) ? ShortCuts[itemId] : "Unknown shortcut?");
            }
        }

        #endregion

        #region public fields

        /// <summary>
        /// Singleton instance of ConfigObject
        /// </summary>
        public static ConfigObject Instance {
            get { return _instance ?? (_instance = Init()); }
        }

        /// <summary>
        /// Url to request to get info on the latest releases
        /// </summary>
        public static string ReleasesApi { get { return @"https://api.github.com/repos/jcaillon/3P/releases"; } }

        /// <summary>
        /// Url to post logs
        /// </summary>
        public static string SendLogApi { get { return @"https://api.github.com/repos/jcaillon/3p/issues/2/comments"; } }

        public static string IssueUrl { get { return @"https://github.com/jcaillon/3P/issues"; } }

        public static string UrlHelpSetEnv { get { return @"http://jcaillon.github.io/3P/#/set_environment"; } }

        public static string UrlHelpCustomThemes { get { return @"http://jcaillon.github.io/3P/#/custom-themes"; } }

        /// <summary>
        /// Url for the ping webservice
        /// </summary>
        public static string PingWebWervice { get { return @"http://noyac.fr/3pWebService/v1.4.1/?action=ping"; } }

        public static bool IsDevelopper { get { return Instance.UserName.Equals("JCA"); } }

        /// <summary>
        /// Path to important files / folders
        /// </summary>
        public static string FolderLog { get { return CreateDirectory(Path.Combine(Npp.GetConfigDir(), "Log")); } }
        public static string FolderTechnical { get { return CreateDirectory(Path.Combine(Npp.GetConfigDir(), "Technical")); } }
        public static string FolderDatabase { get { return CreateDirectory(Path.Combine(Npp.GetConfigDir(), "DatabaseInfo")); } }
        public static string FolderLibrary { get { return CreateDirectory(Path.Combine(Npp.GetConfigDir(), "Libraries")); } }
        public static string FolderUpdate { get { return Path.Combine(Npp.GetConfigDir(), "Update"); } }
        public static string FolderTemplates { get { return CreateDirectory(Path.Combine(Npp.GetConfigDir(), "Templates")); } }
        public static string FolderThemes { get { return CreateDirectory(Path.Combine(Npp.GetConfigDir(), "Themes")); } }
        public static string FolderTemp { get { return CreateDirectory(Path.Combine(Path.GetTempPath(), AssemblyInfo.AssemblyProduct)); } }

        // themes
        public static string FileSyntaxThemes { get { return Path.Combine(FolderThemes, "_SyntaxThemes.conf"); } }
        public static string FileApplicationThemes { get { return Path.Combine(FolderThemes, "_ApplicationThemes.conf"); } }

        // errors
        public static string FileErrorLog { get { return Path.Combine(FolderLog, "error.log"); } }
        public static string FileErrorToSend { get { return Path.Combine(FolderLog, "error_.log"); } }

        // dumps
        public static string FileFilesInfo { get { return Path.Combine(FolderTechnical, "filesInfo.dump"); } }
        public static string FileKeywordsRank { get { return Path.Combine(FolderTechnical, "keywordsRank.dump"); } }

        // general config
        public static string FileKeywordsList { get { return Path.Combine(Npp.GetConfigDir(), "_KeywordsList.conf"); } }
        public static string FileKeywordsHelp { get { return Path.Combine(Npp.GetConfigDir(), "_KeywordsHelp.conf"); } }
        public static string FileAbbrev { get { return Path.Combine(Npp.GetConfigDir(), "_Abbreviations.conf"); } }
        public static string FileCompilPath { get { return Path.Combine(Npp.GetConfigDir(), "_CompilationPath.conf"); } }
        public static string FileProEnv { get { return Path.Combine(Npp.GetConfigDir(), "_ProgressEnvironnement.xml"); } }
        public static string FileSnippets { get { return Path.Combine(Npp.GetConfigDir(), "_SnippetList.conf"); } }
        public static string FileUdl { get { return Path.Combine(Npp.GetConfigDir(), @"../../../userDefineLang.xml"); } }
        public static string FileSettings { get { return Path.Combine(Npp.GetConfigDir(), "settings.xml"); } }
        public static string FileStartProlint { get { return Path.Combine(Npp.GetConfigDir(), "StartProlint.p"); } }

        // updates related
        public static string FileVersionLog { get { return Path.Combine(Npp.GetConfigDir(), "version.log"); } }
        public static string FileDownloadedPlugin { get { return Path.Combine(FolderUpdate, "3P.dll"); } }
        public static string FileZipExe { get { return Path.Combine(FolderUpdate, "7z.exe"); } }
        public static string FileZipDll { get { return Path.Combine(FolderUpdate, "7z.dll"); } }
        public static string FileUpdaterExe { get { return Path.Combine(FolderUpdate, "3pUpdater.exe"); } }
        public static string FileUpdaterLst { get { return Path.Combine(FolderUpdate, "3pUpdater.lst"); } }
        public static string FileLatestReleaseZip { get { return Path.Combine(FolderUpdate, "latestRelease.zip"); } }

        #endregion

        #region static helper

        /// <summary>
        /// Takes a list of priority like AutoCompletePriorityList and return the expected list
        /// </summary>
        /// <param name="enumerationType"></param>
        /// <param name="configPropertyName"></param>
        /// <returns></returns>
        public static List<int> GetPriorityList(Type enumerationType, string configPropertyName) {
            var value = (string)Instance.GetValueOf(configPropertyName);
            if (Enum.GetNames(enumerationType).Length != value.Split(',').Length) {
                var defaultConf = new ConfigObject();
                value = (string)defaultConf.GetValueOf(configPropertyName);
                Instance.SetValueOf(configPropertyName, value);
            }
            var output = new List<int>();
            var temp = value.Split(',').Select(Int32.Parse).ToList();
            for (int i = 0; i < Enum.GetNames(enumerationType).Length; i++)
                output.Add(temp.IndexOf(i));
            return output;
        }

        public static string GetTrigramFromPa() {
            // default values
            string paIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProgressAssist", "pa.ini");
            if (File.Exists(paIniPath)) {
                IniReader ini = new IniReader(paIniPath);
                return ini.GetValue("Trigram", "");
            }
            return "";
        }

        #endregion

        #region private

        private static ConfigObject _instance;

        private static string CreateDirectory(string dir) {
            Utils.CreateDirectory(dir);
            return dir;
        }

        #endregion

        #region mechanics

        /// <summary>
        /// init the instance by either reading the values from an existing file or 
        /// creating one with the default values of the object
        /// </summary>
        /// <returns></returns>
        private static ConfigObject Init() {
            _instance = new ConfigObject();
            if (File.Exists(FileSettings)) {
                try {
                    Object2Xml<ConfigObject>.LoadFromFile(_instance, FileSettings);
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error when loading settings", FileSettings);
                    _instance = new ConfigObject();
                }
            }
            return _instance;
        }

        /// <summary>
        /// Call this method to save the content of the config.instance into an .xml file
        /// </summary>
        public static void Save() {
            try {
                if (!String.IsNullOrWhiteSpace(FileSettings))
                    Object2Xml<ConfigObject>.SaveToFile(_instance, FileSettings);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when saving settings");
            }
        }

        #endregion
    }
}