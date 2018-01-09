#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.NppCore;

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
        /// Set NeedApplySetting to refresh certain options that need special treatment to appear changed (see option page)
        /// </summary>
        internal class ConfigObject {
            //[StringLength(15)]
            //[RegularExpression(@"^\$?\d+(\.(\d{2}))?$")]

            #region GENERAL

            /// <summary>
            /// GENERAL
            /// </summary>
            [ConfigAttribute(Label = "User name",
                Tooltip = "Used for modification tags",
                GroupName = "General")]
            public string UserName = "";

            [ConfigAttribute(Label = "Progress 4GL file patterns",
                Tooltip = "A comma separated list of patterns that identify a Progress file<br>It is used to check if you can activate a 3P feature on the file currently opened<br>You can use wild-cards * and ?, the pattern is applied on the complete file path<br>Example of patterns : *.p,*\\my_sub_directory\\*,*",
                GroupName = "General")]
            public string FilesPatternProgress = "*.p,*.i,*.w,*.t,*.cls,*.dbg,*.df";

            [ConfigAttribute(Label = "Npp files patterns",
                Tooltip = "A comma separated list of patterns that identify a file that must be open by Npp from the file explorer<br>It is used to check if you can activate a 3P feature on the file currently opened<br>You can use wild-cards * and ?, the pattern is applied on the complete file path<br>Example of patterns : *.p,*\\my_sub_directory\\*,*",
                GroupName = "General")]
            public string FilesPatternNppOpenable = "*.txt,*.boi,*.sh,*.cmd,*.xrf,*.lis,*.xml";

            [ConfigAttribute(Label = "Path to the help file",
                Tooltip = "Should point to the progress documentation file (lgrfeng.chm)",
                GroupName = "General")]
            public string GlobalHelpFilePath = "";

            [ConfigAttribute(Label = "Never use Prowin in batch mode",
                Tooltip = "For performance considerations and in order to avoid the Progress splash screen,<br>3P starts the Prowin process for compilation in batch mode (-b option)<br>If you absolutely want to avoid having the Prowin process creating a window in the taskbar,<br>you can toggle on this option and the -b option will never be used<br><br><i>The batch mode is slighty faster than its counterpart!</i>",
                GroupName = "General")]
            public bool NeverUseProwinInBatchMode = false;

            [ConfigAttribute(Label = "Use default values in file info",
                Tooltip = "Set to true and the <b>default</b> option will be selected when you open a new file info,<br>set to false and the option <b>last values</b> will be selected",
                GroupName = "General")]
            public bool UseDefaultValuesInsteadOfLastValuesInEditTags = false;

            [ConfigAttribute(Label = "Always show a notification after a compilation",
                Tooltip = "Whether or not to systematically show a notification after a compilation<br>By default, a notification is shown if notepad++ doesn't have the focus or if they are errors",
                GroupName = "General")]
            public bool CompileAlwaysShowNotification = true;

            [ConfigAttribute(Label = "Allow tab animation",
                Tooltip = "Allow the main application window to animate the transition between pages with a fade in / fade out animation",
                GroupName = "General",
                NeedApplySetting = true
                )]
            public bool AppliAllowTabAnimation = true;

            [Config(Label = "Show tree branches",
                Tooltip = "Whether or not you want to draw the branches of the trees displayed in 3P (for the file and code explorer)",
                GroupName = "General",
                NeedApplySetting = true)]
            public bool ShowTreeBranches = false;

            public bool GlobalShowDetailedHelpForErrors = true;
            public int InstallStep = 0; // 0 first lauch, 1 npp options applied
            public bool NppOutdatedVersion = false;

            #endregion

            #region COMPILATION

            [ConfigAttribute(Label = "Compilable file patterns",
                Tooltip = "A comma separated list of patterns that identify a compilable Progress file<br>It is used to check if you can compile / check syntax / execute the current file<br>You can use wildcards * and ?, the pattern is applied on the complete file path<br>Example of patterns : *.p,*\\my_sub_directory\\*,*",
                GroupName = "Compilation")]
            public string FilesPatternCompilable = "*.p,*.w,*.t,*.cls";

            [ConfigAttribute(Label = "Always use a temp directory to compile",
                Tooltip = "Toggle on to compile your code locally, in your %temp% folder and <b>then</b> move it to its destination<br>This option allows you to not immediatly replace your existing *.r / *.lst files as they are only<br>copied to their destination if the compilation went ok<br><br>This option can be used with no impact if your compilation folder is in a local disk,<br>but if you compile your files on a distant windows server, it will probably slow down the compilation",
                GroupName = "Compilation")]
            public bool CompileForceUseOfTemp = false;

            [ConfigAttribute(Label = "Compile with debug-list",
                Tooltip = "A compilation option, see the help for the COMPILE statement",
                GroupName = "Compilation")]
            public bool CompileWithDebugList = false;

            [ConfigAttribute(Label = "Compile with xref",
                Tooltip = "A compilation option, see the help for the COMPILE statement",
                GroupName = "Compilation")]
            public bool CompileWithXref = false;

            [ConfigAttribute(Label = "Compile with listing",
                Tooltip = "A compilation option, see the help for the COMPILE statement",
                GroupName = "Compilation")]
            public bool CompileWithListing = false;

            [ConfigAttribute(Label = "Get xref in xml format",
                Tooltip = "Compile with the XREF-XML option instead of XREF",
                GroupName = "Compilation")]
            public bool CompileUseXmlXref = false;

            public string CompileDirectoriesHistoric = "";

            public string CurrentDeployProfile = "";

            #endregion

            #region UPDATES

            [ConfigAttribute(Label = "I want to get beta releases",
                Tooltip = "Check this option if you want to update 3P with the latest beta version <b>(i.e. NOT STABLE)</b><br>Otherwise, you will only have update notifications for stable releases",
                GroupName = "Updates")]
            public bool UserGetsPreReleases = AssemblyInfo.IsPreRelease;

            [ConfigAttribute(Label = "No automatic updates for 3P",
                Tooltip = "Toggle this option to prevent 3P from automatically checking for a new version on github<br><b>You will not have access to the latest features and will not enjoy bug corrections!</b>",
                GroupName = "Updates")]
            public bool GlobalDontCheckUpdates = false;

            [ConfigAttribute(Label = "No automatic updates for prolint",
                Tooltip = "Toggle this option to prevent 3P from automatically checking for a new version of prolint on github<br><b>You will not have access to the latest features and will not enjoy bug corrections!</b>",
                GroupName = "Updates")]
            public bool GlobalDontCheckProlintUpdates = false;

            [ConfigAttribute(Label = "Use a webproxy for updates",
                Tooltip = "Toggle this option to use the http(s) proxy defined below when querying updates from github",
                GroupName = "Updates")]
            public bool WebUseProxy = false;

            [ConfigAttribute(Label = "Webproxy URI",
                Tooltip = "Configure your proxy here<br><i>http://host:port/</i>",
                GroupName = "Updates")]
            public string WebProxyUri = @"";

            [ConfigAttribute(Label = "Webproxy Username (if any)",
                Tooltip = "If your proxy is using authentication fill the username here, otherwise leave empty",
                GroupName = "Updates")]
            public string WebProxyUsername = @"";

            [ConfigAttribute(Label = "Webproxy Password (if any)",
                Tooltip = "If your proxy is using authentication fill the password here, otherwise leave empty<br><b>Please notice that your password will be stored in plain text!</b>",
                GroupName = "Updates")]
            public string WebProxyPassword = @"";

            public string DebugReleasesApi = @"";

            #endregion

            #region AUTOCOMPLETION

            /// <summary>
            /// AUTOCOMPLETION
            /// </summary>
            [ConfigAttribute(Label = "Show auto completion on key input",
                Tooltip = "Automatically show the auto completion list when you start entering characters",
                GroupName = "Autocompletion")]
            public bool AutoCompleteOnKeyInputShowSuggestions = true;

            [ConfigAttribute(Label = "Start showing after X char",
                Tooltip = "If you chose to display the list on key input,<br> you can set the minimum number of char necessary before showing the list ",
                GroupName = "Autocompletion")]
            [Range(1, 99)]
            public int AutoCompleteStartShowingListAfterXChar = 1;

            [ConfigAttribute(Label = "Hide auto completion if empty",
                Tooltip = "If the list was displayed automatically and there are no suggestions matching your input,<br>this option will automatically hide the list instead of showing it empty",
                GroupName = "Autocompletion")]
            public bool AutoCompleteOnKeyInputHideIfEmpty = true;

            [ConfigAttribute(Label = "Show children on separator input",
                Tooltip = "Choose this option to immediately show the auto completion after a '.' or a ':'<br>Otherwise 3P waits for the set number of characters to show it",
                GroupName = "Autocompletion")]
            public bool AutoCompleteShowChildrenAfterSeparator = true;

            [ConfigAttribute(Label = "Use TAB to accept a suggestion",
                Tooltip = "Whether or not to allow the TAB key to accept the suggestion",
                GroupName = "Autocompletion")]
            public bool AutoCompleteUseTabToAccept = true;

            [ConfigAttribute(Label = "User ENTER to accept a suggestion",
                Tooltip = "Whether or not to allow the ENTER key to accept the suggestion",
                GroupName = "Autocompletion")]
            public bool AutoCompleteUseEnterToAccept = true;

            [ConfigAttribute(Label = "Unfocused opacity",
                Tooltip = "The opacity of the list when unfocused",
                GroupName = "Autocompletion",
                NeedApplySetting = true)]
            [Range(0.1, 1)]
            public double AutoCompleteUnfocusedOpacity = 0.92;

            [ConfigAttribute(Label = "Focused opacity",
                Tooltip = "The opacity of the list when focused",
                GroupName = "Autocompletion",
                NeedApplySetting = true)]
            [Range(0.1, 1)]
            public double AutoCompleteFocusedOpacity = 0.92;


            /// <summary>
            /// PROGRESS AUTOCOMPLETION
            /// </summary>
            [ConfigAttribute(Label = "Show list in comments and strings",
                Tooltip = "By default, the auto completion list is hidden in comments and strings<br>you can still show the completion list manually!",
                GroupName = "Progress autocompletion")]
            public bool AutoCompleteShowInCommentsAndStrings = false;

            [ConfigAttribute(Label = "Insert current suggestion on word end",
                Tooltip = "You can check this option to automatically insert the currently selected suggestion<br>(if the list is opened)<br>when you enter any character that is not a letter/digit/_/-",
                GroupName = "Progress autocompletion")]
            public bool AutoCompleteInsertSelectedSuggestionOnWordEnd = true;

            [ConfigAttribute(Label = "Auto-case as I'm typing",
                Tooltip = "Let 3P automatically correct the case of each word you type<br><i>If there is a match between the word you typed and an item in the autocompletion,<br>the case will be corrected with the autocompletion value</i>",
                GroupName = "Progress autocompletion")]
            public bool AutoCompleteAutoCase = true;

            [ConfigAttribute(Label = "Keywords case",
                Tooltip = "Change the case of each word displayed in the auto completion to :<br>UPPERCASED (1), lowercased (2), CamelCased (3) or set as it appears in the documentation (0)",
                GroupName = "Progress autocompletion",
                NeedApplySetting = true)]
            public Extensions.CaseMode AutoCompleteKeywordCaseMode = Extensions.CaseMode.Default;

            [ConfigAttribute(Label = "Database words case",
                Tooltip = "Change the case of each information extracted from the database (db name, tables, fields, sequences) to :<br>UPPERCASED (1), lowercased (2) or CamelCased (3), or set as it appears in the database (0)",
                GroupName = "Progress autocompletion",
                NeedApplySetting = true)]
            public Extensions.CaseMode AutoCompleteDatabaseWordCaseMode = Extensions.CaseMode.Default;

            [ConfigAttribute(Label = "Insert full word instead of abbreviations",
                Tooltip = "Automatically replaces abbreviations by their full length counterparts",
                GroupName = "Progress autocompletion")]
            public bool AutoCompleteReplaceAbbreviations = true;

            [ConfigAttribute(Label = "Auto replace semicolon",
                Tooltip = "Check to replace automatically ; by . <br><i>useful if you come from any other language!!!</i>",
                GroupName = "Progress autocompletion")]
            public bool AutoCompleteReplaceSemicolon = true;

            [ConfigAttribute(Label = "Only show already defined variables",
                Tooltip = "By default, 3P filters the autocompletion list to only show you<br>the items that are available at the line where you activate<br>the autocompletion list.<br>You can set this option to false to show an item even if,<br>for the line where your cursor is, it is not yet defined.",
                GroupName = "Progress autocompletion")]
            public bool AutoCompleteOnlyShowDefinedVar = true;

            // Height of the autocompletion form
            public int AutoCompleteShowListOfXSuggestions = 12;

            // Width of the autocompletion form
            public int AutoCompleteWidth = 310;

            public bool AutoCompleteNeverAskToDisableDefault = false;

            public string AutoCompleteItemPriorityList = "";


            /// <summary>
            /// NPP AUTOCOMPLETION
            /// </summary>
            [ConfigAttribute(Label = "Maximum length for the parser",
                Tooltip = "The maximum length of text that should be analyzed by the parser<br>Please note that this is a length relative to your position in the file.<br>If you scroll down on a big text, 3P will parse the text around your current location",
                GroupName = "Default autocompletion replacement")]
            public int NppAutoCompleteMaxLengthToParse = 5000000;

            [ConfigAttribute(Label = "Ignore numbers",
                Tooltip = "Should the autocompletion ignore numbers when parsing for words to suggest?",
                GroupName = "Default autocompletion replacement")]
            public bool NppAutoCompleteIgnoreNumbers = false;

            [ConfigAttribute(Label = "Insert current suggestion on word end",
                Tooltip = "You can check this option to automatically insert the currently selected suggestion<br>(if the list is opened)<br>when you enter any character that is not a letter/digit/_/-",
                GroupName = "Default autocompletion replacement")]
            public bool NppAutoCompleteInsertSelectedSuggestionOnWordEnd = false;

            [ConfigAttribute(Label = "Auto-case as I'm typing",
                Tooltip = "Let 3P automatically correct the case of each word you type<br><i>If there is a match between the word you typed and an item in the autocompletion,<br>the case will be corrected with the autocompletion value</i>",
                GroupName = "Default autocompletion replacement")]
            public bool NppAutoCompleteAutoCase = false;

            [ConfigAttribute(Label = "Minimum length required for suggested words",
                Tooltip = "Words in your document that have a length strictly inferior to this value<br>will not appear in the autocompletion",
                GroupName = "Default autocompletion replacement")]
            public int NppAutoCompleteMinWordLengthRequired = 2;

            [ConfigAttribute(Label = "Filter case sensitivity",
                Tooltip = "As you type, the autocompletion list will be filtered to the best matches,<br>this option let you chose the behavior of the filter.<br><br><i>The value to mimic the behavior of notepad++ would be sensitive</i>",
                GroupName = "Default autocompletion replacement")]
            public AutoCompletion.CaseMode NppAutoCompleteFilterCaseMode = AutoCompletion.CaseMode.Insensitive;

            [ConfigAttribute(Label = "Keywords list case sensitivity",
                Tooltip = "Chose how keywords are added to the autocompletion list :<br>- a word can appear several times with different cases (sensitive)<br>- or only once no matter its case (insensitive)<br><br><i>The value to mimic the behavior of notepad++ would be sensitive</i>",
                GroupName = "Default autocompletion replacement",
                NeedApplySetting = true)]
            public AutoCompletion.CaseMode NppAutoCompleteParserCaseMode = AutoCompletion.CaseMode.Sensitive;

            #endregion

            #region CODE EDITION

            /// <summary>
            /// CODE EDITION
            /// </summary>
            [ConfigAttribute(Label = "Max number of characters in a block",
                Tooltip = "The appbuilder is limited in the number of character that a block (procedure, function...) can contain<br>This value allows to show a warning when you overpass the limit in notepad++",
                GroupName = "Code edition")]
            public int GlobalMaxNbCharInBlock = 31190;

            [ConfigAttribute(Label = "Tab width",
                Tooltip = "The number of spaces that will be inserted when you press TAB and re-indent the code",
                GroupName = "Code edition",
                NeedApplySetting = true)]
            [Range(0, 10)]
            public int CodeTabSpaceNb = 4;

            [ConfigAttribute(Label = "Activate 'Show spaces' on progress files",
                Tooltip = "Toggle on/off to activate the 'show spaces' notepad++ option when switching to a progress file<br>This option let the user see the spaces as dots and tabs as arrows in its document",
                GroupName = "Code edition")]
            public bool CodeShowSpaces = false;

            [ConfigAttribute(Label = "Disable auto update of function prototypes",
                Tooltip = "Toggle on to prevent 3P from automatically updating your functions prototypes according to their implementation<br>You are still able to manually trigger the update through the menu",
                GroupName = "Code edition")]
            public bool DisablePrototypeAutoUpdate = false;

            [ConfigAttribute(Label = "Display parser errors on save",
                Tooltip = "Each time you save a progress file, this will validate that it can be read by the AppBuilder.<br>It checks the length of each block and the syntax of AppBuilder pre-processed statements",
                GroupName = "Code edition")]
            public bool DisplayParserErrorsOnSave = true;

            #endregion

            #region FILE EXPLORER

            /// <summary>
            /// FILE EXPLORER
            /// </summary>
            /// 
            public bool FileExplorerVisible = true;

            [ConfigAttribute(Label = "Ignore unix hidden folder",
                Tooltip = "Check to ignore all the files/folders starting with a dot '.'",
                GroupName = "File explorer")]
            public bool FileExplorerIgnoreUnixHiddenFolders = true;

            [ConfigAttribute(Label = "Auto-hide/show for progress documents",
                Tooltip = "Check this option to automatically hide the File explorer when the current isn't<br>a progress file, and automatically show it when it is",
                GroupName = "File explorer")]
            public bool FileExplorerAutoHideOnNonProgressFile = false;

            [ConfigAttribute(Label = "File listing time out",
                Tooltip = "The maximum time given to 3P to list the files of your current directory<br>This option is here to limit the amount of time spent to build<br>a list of you files. This time can be huge if you select<br>a folder with a lot of files or if you select a folder on a windows server",
                GroupName = "File explorer")]
            [Range(1000, 30000)]
            public int FileExplorerListFilesTimeOutInMs = 3000;

            // Current folder mode for the file explorer : local/compilation/propath/everywhere
            public int FileExplorerDirectoriesToExplore = 0;

            #endregion

            #region CODE EXPLORER

            /// <summary>
            /// CODE EXPLORER
            /// </summary>
            public bool CodeExplorerVisible = true;

            public string CodeExplorerItemPriorityList = "";
            public bool CodeExplorerDisplayPersistentItems = false;
            public bool CodeExplorerDisplayItemsFromInclude = true;

            [ConfigAttribute(Label = "Auto-hide/show for progress documents",
                Tooltip = "Check this option to automatically hide the Code explorer when the current isn't<br>a progress file, and automatically show it when it is",
                GroupName = "Code explorer")]
            public bool CodeExplorerAutoHideOnNonProgressFile = false;

            public SortingType CodeExplorerSortingType;

            #endregion

            #region TOOLTIP

            /// <summary>
            /// TOOLTIPS
            /// </summary>
            [ConfigAttribute(Label = "Deactivate all tooltips",
                Tooltip = "Don't do that, it would be a shame to not use them!",
                GroupName = "Tooltip")]
            public bool ToolTipDeactivate = false;

            [ConfigAttribute(Label = "Idle time to spawn",
                Tooltip = "The amount of time in milliseconds that you have to left your<br>mouse over a word before it shows its tooltip",
                GroupName = "Tooltip",
                NeedApplySetting = true)]
            [Range(0, 5000)]
            public int ToolTipmsBeforeShowing = 500;

            [ConfigAttribute(Label = "Opacity",
                Tooltip = "The tooltip opacity",
                GroupName = "Tooltip",
                NeedApplySetting = true)]
            [Range(0.1, 1)]
            public double ToolTipOpacity = 0.92;

            #endregion
            
            #region SWITCH ENCODING

            [Config(Label = "Automatically change encoding on file opening",
                Tooltip = "<i>Leave empty to disable this feature (default)</i><br>A comma (,) separated list of filters :<br>when a file is opened, if it matches one of the filter, the selected encoding is applied<br><br>Example of filter :<div class='ToolTipcodeSnippet'>*.p,\\*my_sub_directory\\*,*.r</div>",
                GroupName = "Switch encoding")]
            public string AutoSwitchEncodingForFilePatterns = "";

            [Config(Label = "Encoding to apply",
                Tooltip = "Choose the encoding to apply to the files when they are opened<br><i>The default option is 'Automatic', to let Notepad++ select the encoding</i>",
                GroupName = "Switch encoding")]
            public NppEncodingFormat AutoSwitchEncodingTo = NppEncodingFormat._Automatic_default;

            #endregion

            // set to false when the plugin starts, and to true when it stops; if false when it starts then npp crashed
            public bool NppStoppedCorrectly; 

            // Shared configuration last folder selected
            public string SharedConfFolder = "";

            // a list of Label corresponding to confLine(s) that are auto-updated
            public string AutoUpdateConfList = "";

            // a list of folders used in the shared/exported conf
            public string SharedConfHistoric = "";

            // Current environment
            public string EnvLastDbInfoUsed = "";
            public string EnvName = "";
            public string EnvSuffix = "";

            // last ping time
            public string TechnicalLastPing = "";
            public string TechnicalMyUuid = Guid.NewGuid().ToString();

            // did the last update check went ok?
            public bool TechnicalLastWebserviceCallOk = true;

            // THEMES
            public int ThemeId = 0;
            public Color AccentColor = ColorTranslator.FromHtml("#647687");
            public int SyntaxHighlightThemeId = 0;
            
            // SHORTCUTS (id, shortcut spec)
            public Dictionary<string, string> ShortCuts = new Dictionary<string, string>();

            // stores at which did we last did a specific action
            public Dictionary<string, string> LastCallDateTime = new Dictionary<string, string>();

            // to know which file should be read as a progress and which not to
            // <path, 0/1 = is progress or not>
            public Dictionary<string, string> ProgressFileExeptions = new Dictionary<string, string>();

            #region methods

            /// <summary>
            /// Returns the proxy defined by the user in the configuration
            /// </summary>
            /// <returns></returns>
            public IWebProxy GetWebClientProxy() {
                if (WebUseProxy && !string.IsNullOrEmpty(WebProxyUri)) {
                    return new WebProxy(WebProxyUri) {
                        Credentials = new NetworkCredential(WebProxyUsername ?? "", WebProxyPassword ?? ""),
                        UseDefaultCredentials = false,
                        BypassProxyOnLocal = true
                    };
                }
                IWebProxy proxy = WebRequest.DefaultWebProxy;
                proxy.Credentials = CredentialCache.DefaultCredentials;
                return proxy;
            }
            
            /// <summary>
            /// Returns the shortcut specs corresponding to the itemId given
            /// </summary>
            public string GetShortcutSpecFromName(string itemId) {
                return ShortCuts.ContainsKey(itemId) ? ShortCuts[itemId] : "Unknown shortcut?";
            }

            #endregion
        }

        #endregion

        #region public fields

        public static string RequiredNppVersion {
            get { return "v7.5.1"; }
        }
        
        /// <summary>
        /// Singleton instance of ConfigObject
        /// </summary>
        public static ConfigObject Instance {
            get { return _instance ?? (_instance = Init()); }
        }

        public static string WebclientUserAgent {
            get { return "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)"; }
        }

        public static string UrlIssues {
            get { return @"https://github.com/jcaillon/3P/issues"; }
        }

        // HELP URL
        public static string UrlWebSite {
            get { return @"http://jcaillon.github.io/3P/"; }
        }

        public static string UrlHelpSetEnv {
            get { return @"http://jcaillon.github.io/3P/#/set-environment"; }
        }

        public static string UrlHelpCustomThemes {
            get { return @"http://jcaillon.github.io/3P/#/custom-themes"; }
        }

        public static string UrlCheckReleases {
            get { return @"https://github.com/jcaillon/3P/releases"; }
        }

        public static string UrlHelpDeploy {
            get { return @"http://jcaillon.github.io/3P/#/deployment"; }
        }

        public static string UrlHelpDeployRules {
            get { return @"http://jcaillon.github.io/3P/#/deployment-rules"; }
        }

        /// <summary>
        /// Url for the webservices
        /// </summary>
        public static string PostPingWebWervice {
            get { return @"https://greenzest.000webhostapp.com/ws/1.6.4/?action=ping&softName=3p"; }
        }

        public static string PostBugsWebWervice {
            get { return @"https://greenzest.000webhostapp.com/ws/1.6.4/?action=bugs&softName=3p"; }
        }

        public static string GetPingWebWervice {
            get { return @"https://greenzest.000webhostapp.com/ws/1.6.4/?action=getPing&softName=3p"; }
        }

        public static string GetBugsWebWervice {
            get { return @"https://greenzest.000webhostapp.com/ws/1.6.4/?action=getBugs&softName=3p"; }
        }

        public static int PostPingEveryXMin = 3 * 60;


        /// <summary>
        /// Is developer = the file debug exists
        /// </summary>
        public static bool IsDeveloper {
            get {
                if (_isDevelopper == null)
                    _isDevelopper = File.Exists(FileDebug);
                return (bool) _isDevelopper;
            }
        }

        private static bool? _isDevelopper;

        public static string FileDebug {
            get { return Path.Combine(Npp.ConfigDirectory, "debug"); }
        }

        /// <summary>
        /// Path to important files / folders
        /// </summary>
        public static string FolderLog {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "Log")); }
        }

        public static string FolderTechnical {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "Technical")); }
        }

        public static string FolderDatabase {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "DatabaseInfo")); }
        }

        public static string FolderTemplates {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "Templates")); }
        }

        public static string FolderThemes {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "Themes")); }
        }
        
        public static string FolderUpdate {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "Update")); }
        }

        public static string FolderTemp {
            get { return CreateDirectory(Path.Combine(Path.GetTempPath(), AssemblyInfo.AssemblyProduct)); }
        }

        // themes
        public static string FileSyntaxThemes {
            get { return Path.Combine(FolderThemes, "_ThemesForSyntax.conf"); }
        }

        public static string FileApplicationThemes {
            get { return Path.Combine(FolderThemes, "_ThemesForApplication.conf"); }
        }

        // errors
        public static string FileErrorLog {
            get { return Path.Combine(FolderLog, "error.log"); }
        }

        // dumps
        public static string FileFilesInfo {
            get { return Path.Combine(FolderTechnical, "filesInfo.dump"); }
        }

        public static string FileKeywordsRank {
            get { return Path.Combine(FolderTechnical, "keywordsRank.dump"); }
        }

        // general config
        public static string FileKeywordsList {
            get { return Path.Combine(Npp.ConfigDirectory, "_KeywordsList.conf"); }
        }

        public static string FileKeywordsHelp {
            get { return Path.Combine(Npp.ConfigDirectory, "_KeywordsHelp.conf"); }
        }

        public static string FileAbbrev {
            get { return Path.Combine(Npp.ConfigDirectory, "_Abbreviations.conf"); }
        }

        public static string FileDeploymentRules {
            get { return Path.Combine(Npp.ConfigDirectory, "_DeploymentRules.conf"); }
        }

        public static string FileModificationTags {
            get { return Path.Combine(Npp.ConfigDirectory, "_ModificationTags.conf"); }
        }

        public static string FileProEnv {
            get { return Path.Combine(Npp.ConfigDirectory, "_ProgressEnvironnement.xml"); }
        }

        public static string FileSnippets {
            get { return Path.Combine(Npp.ConfigDirectory, "_SnippetList.conf"); }
        }

        public static string FileSettings {
            get { return Path.Combine(Npp.ConfigDirectory, "settings.xml"); }
        }

        public static string FileDeploymentHook {
            get { return Path.Combine(Npp.ConfigDirectory, "DeploymentHookProc.p"); }
        }

        public static string FileDeployProfiles {
            get { return Path.Combine(Npp.ConfigDirectory, "_DeploymentProfiles.conf"); }
        }

        // updates related

        public static string FileUpdaterExe {
            get { return Path.Combine(FolderUpdate, "3pUpdater.exe"); }
        }

        public static string FileUpdaterLst {
            get { return Path.Combine(Path.GetDirectoryName(FileUpdaterExe) ?? "", "3pUpdater.lst"); }
        }

        // Convert.ToBase64String(Encoding.ASCII.GetBytes("user:mdp"));
        public static string GitHubBasicAuthenticationToken {
            get { return @"M3BVc2VyOnJhbmRvbXBhc3N3b3JkMTIz"; }
        }

        public static int UpdateCheckEveryXMin = 8 * 60;

        /// <summary>
        /// Url for the github webservices
        /// </summary>
        public static string UpdateReleasesApi {
            get { return IsDeveloper && !string.IsNullOrEmpty(Instance.DebugReleasesApi) ? Instance.DebugReleasesApi : @"https://api.github.com/repos/jcaillon/3P/releases"; }
        }

        public static string UpdateVersionLog {
            get { return Path.Combine(FolderUpdate, "version.log"); }
        }

        public static string UpdatePreviousVersion {
            get { return Path.Combine(FolderUpdate, "previous.version"); }
        }

        public static string UpdateReleaseUnzippedFolder {
            get { return Path.Combine(FolderUpdate, "latest"); }
        }

        // name of the zip file containing the release in the assets of the release
        public static string UpdateGitHubAssetName {
            get { return @"3P" + (Environment.Is64BitProcess ? "_x64" : "") + ".zip"; }
        }
        
        // Prolint
        public static string ProlintFolder {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "prolint")); }
        }

        public static string ProlintReleasesApi {
            get { return @"https://api.github.com/repos/jcaillon/prolint/releases"; }
        }
        
        public static string ProlintStartProcedure {
            get { return Path.Combine(ProlintFolder, "StartProlint.p"); }
        }
        
        public static string ProlintGitHubAssetName {
            get { return @"prolint.zip"; }
        }

        // Proparse.net
        public static string ProparseReleasesApi {
            get { return @"https://api.github.com/repos/jcaillon/proparse/releases"; }
        }

        public static string ProparseGitHubAssetName {
            get { return @"proparse.net.zip"; }
        }

        // Datadigger
        public static string DataDiggerFolder {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "DataDigger")); }
        }
        
        public static string DataDiggerReleasesApi {
            // Only get the latest release because they are not named as we would like them to be (Datadigger22, BETA2010404 and so on...)
            get { return @"https://api.github.com/repos/patrickTingen/DataDigger/releases/latest"; }
        }


        #endregion

        #region static helper

        /// <summary>
        /// Takes a list of priority like AutoCompletePriorityList and return the expected list
        /// </summary>
        public static Dictionary<int, int> GetPriorityList<T>(string configPropertyName) {
            var enumerationType = typeof(T);
            var value = (string) Instance.GetValueOf(configPropertyName);
            int enumLength = Enum.GetNames(enumerationType).Length;
            if (enumLength != value.Split(',').Length) {
                value = "";
                foreach (T val in Enum.GetValues(enumerationType)) {
                    value += Convert.ToInt64(val) + ",";
                }
                value = value.TrimEnd(',');
                Instance.SetValueOf(configPropertyName, value);
            }
            var output = new Dictionary<int, int>();
            var temp = value.Split(',').Select(int.Parse).ToList();
            for (int i = 0; i < enumLength; i++)
                output.Add(temp[i], temp.IndexOf(i));
            return output;
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

        #region ConfigAttribute

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class ConfigAttribute : Attribute {
            public ConfigAttribute() { }

            public ConfigAttribute(string label) {
                Label = label;
            }

            /// <summary>
            /// Gets or sets a value indicating whether this item is hidden and not displayed
            /// </summary>
            public bool Hidden { get; set; }
            
            /// <summary>
            /// Gets or sets the label to use as the label for this field or property
            /// </summary>
            public string GroupName { get; set; }

            /// <summary>
            /// Gets or sets the label to use as the label for this field or property
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Text to show in the tooltip
            /// </summary>
            public string Tooltip { get; set; }
            
            public bool NeedApplySetting { get; set; }
        }

        #endregion
    }
}