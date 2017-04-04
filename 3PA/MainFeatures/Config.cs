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
        /// Set AutoGenerateField to refresh certain options that need special treatment to appear changed (see option page)
        /// </summary>
        internal class ConfigObject {
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

            [Display(Name = "Progress 4GL file patterns",
                Description = "A comma separated list of patterns that identify a Progress file<br>It is used to check if you can activate a 3P feature on the file currently opened<br>You can use wild-cards * and ?, the pattern is applied on the complete file path<br>Example of patterns : *.p,*\\my_sub_directory\\*,*",
                GroupName = "General",
                AutoGenerateField = false)]
            public string FilesPatternProgress = "*.p,*.i,*.w,*.t,*.cls,*.dbg,*.df";

            [Display(Name = "Npp files patterns",
                Description = "A comma separated list of patterns that identify a file that must be open by Npp from the file explorer<br>It is used to check if you can activate a 3P feature on the file currently opened<br>You can use wild-cards * and ?, the pattern is applied on the complete file path<br>Example of patterns : *.p,*\\my_sub_directory\\*,*",
                GroupName = "General",
                AutoGenerateField = false)]
            public string FilesPatternNppOpenable = "*.txt,*.boi,*.sh,*.cmd,*.xrf,*.lis,*.xml";

            [Display(Name = "Path to the help file",
                Description = "Should point to the progress documentation file (lgrfeng.chm)",
                GroupName = "General",
                AutoGenerateField = false)]
            public string GlobalHelpFilePath = "";

            [Display(Name = "Never use Prowin in batch mode",
                Description = "For performance considerations and in order to avoid the Progress splash screen,<br>3P starts the Prowin process for compilation in batch mode (-b option)<br>If you absolutely want to avoid having the Prowin process creating a window in the taskbar,<br>you can toggle on this option and the -b option will never be used<br><br><i>The batch mode is slighty faster than its counterpart!</i>",
                GroupName = "General",
                AutoGenerateField = false)]
            public bool NeverUseProwinInBatchMode = false;

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

            [Display(Name = "Allow tab animation",
                Description = "Allow the main application window to animate the transition between pages with a fade in / fade out animation",
                GroupName = "General",
                AutoGenerateField = true)]
            public bool AppliAllowTabAnimation = true;

            [Display(Name = "Show tree branches",
                Description = "Whether or not you want to draw the branches of the trees displayed in 3P (for the file and code explorer)",
                GroupName = "General",
                AutoGenerateField = true)]
            public bool ShowTreeBranches = false;

            public bool GlobalShowDetailedHelpForErrors = true;

            public bool UserFirstUse = true;
            public bool NppOutdatedVersion = false;

            #endregion

            #region COMPILATION

            [Display(Name = "Compilable file patterns",
                Description = "A comma separated list of patterns that identify a compilable Progress file<br>It is used to check if you can compile / check syntax / execute the current file<br>You can use wildcards * and ?, the pattern is applied on the complete file path<br>Example of patterns : *.p,*\\my_sub_directory\\*,*",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public string FilesPatternCompilable = "*.p,*.w,*.t,*.cls";

            [Display(Name = "Always use a temp directory to compile",
                Description = "Toggle on to compile your code locally, in your %temp% folder and <b>then</b> move it to its destination<br>This option allows you to not immediatly replace your existing *.r / *.lst files as they are only<br>copied to their destination if the compilation went ok<br><br>This option can be used with no impact if your compilation folder is in a local disk,<br>but if you compile your files on a distant windows server, it will probably slow down the compilation",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public bool CompileForceUseOfTemp = false;

            [Display(Name = "Compile with debug-list",
                Description = "A compilation option, see the help for the COMPILE statement",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public bool CompileWithDebugList = false;

            [Display(Name = "Compile with xref",
                Description = "A compilation option, see the help for the COMPILE statement",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public bool CompileWithXref = false;

            [Display(Name = "Compile with listing",
                Description = "A compilation option, see the help for the COMPILE statement",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public bool CompileWithListing = false;

            [Display(Name = "Get xref in xml format",
                Description = "Compile with the XREF-XML option instead of XREF",
                GroupName = "Compilation",
                AutoGenerateField = false)]
            public bool CompileUseXmlXref = false;

            public string CompileDirectoriesHistoric = "";

            public string CurrentDeployProfile = "";

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
            [Display(Name = "Show auto completion on key input",
                Description = "Automatically show the auto completion list when you start entering characters",
                GroupName = "Autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteOnKeyInputShowSuggestions = true;

            [Display(Name = "Start showing after X char",
                Description = "If you chose to display the list on key input,<br> you can set the minimum number of char necessary before showing the list ",
                GroupName = "Autocompletion",
                AutoGenerateField = false)]
            [Range(1, 99)]
            public int AutoCompleteStartShowingListAfterXChar = 1;

            [Display(Name = "Hide auto completion if empty",
                Description = "If the list was displayed automatically and there are no suggestions matching your input,<br>this option will automatically hide the list instead of showing it empty",
                GroupName = "Autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteOnKeyInputHideIfEmpty = true;

            [Display(Name = "Show children on separator input",
                Description = "Choose this option to immediately show the auto completion after a '.' or a ':'<br>Otherwise 3P waits for the set number of characters to show it",
                GroupName = "Autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteShowChildrenAfterSeparator = true;

            [Display(Name = "Use TAB to accept a suggestion",
                Description = "Whether or not to allow the TAB key to accept the suggestion",
                GroupName = "Autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteUseTabToAccept = true;

            [Display(Name = "User ENTER to accept a suggestion",
                Description = "Whether or not to allow the ENTER key to accept the suggestion",
                GroupName = "Autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteUseEnterToAccept = true;

            [Display(Name = "Unfocused opacity",
                Description = "The opacity of the list when unfocused",
                GroupName = "Autocompletion",
                AutoGenerateField = true)]
            [Range(0.1, 1)]
            public double AutoCompleteUnfocusedOpacity = 0.92;

            [Display(Name = "Focused opacity",
                Description = "The opacity of the list when focused",
                GroupName = "Autocompletion",
                AutoGenerateField = true)]
            [Range(0.1, 1)]
            public double AutoCompleteFocusedOpacity = 0.92;

            /// <summary>
            /// NPP AUTOCOMPLETION
            /// </summary>
            [Display(Name = "Maximum length for the parser",
                Description = "The maximum length of text that should be analyzed by the parser<br>Please note that this is a length relative to your position in the file.<br>If you scroll down on a big text, 3P will parse the text around your current location",
                GroupName = "Default autocompletion replacement",
                AutoGenerateField = false)]
            public int NppAutoCompleteMaxLengthToParse = 5000000;

            [Display(Name = "Ignore numbers",
                Description = "Should the autocompletion ignore numbers when parsing for words to suggest?",
                GroupName = "Default autocompletion replacement",
                AutoGenerateField = false)]
            public bool NppAutoCompleteIgnoreNumbers = false;

            [Display(Name = "Insert current suggestion on word end",
                Description = "You can check this option to automatically insert the currently selected suggestion<br>(if the list is opened)<br>when you enter any character that is not a letter/digit/_/-",
                GroupName = "Default autocompletion replacement",
                AutoGenerateField = false)]
            public bool NppAutoCompleteInsertSelectedSuggestionOnWordEnd = false;

            [Display(Name = "Auto-case as I'm typing",
                Description = "Let 3P automatically correct the case of each word you type<br><i>If there is a match between the word you typed and an item in the autocompletion,<br>the case will be corrected with the autocompletion value</i>",
                GroupName = "Default autocompletion replacement",
                AutoGenerateField = false)]
            public bool NppAutoCompleteAutoCase = false;

            [Display(Name = "Minimum length required for suggested words",
                Description = "Words in your document that have a length strictly inferior to this value<br>will not appear in the autocompletion",
                GroupName = "Default autocompletion replacement",
                AutoGenerateField = false)]
            public int NppAutoCompleteMinWordLengthRequired = 2;

            [Display(Name = "Filter case sensitivity",
                Description = "As you type, the autocompletion list will be filtered to the best matches,<br>this option let you chose the behavior of the filter.<br><br><i>The value to mimic the behavior of notepad++ would be sensitive</i>",
                GroupName = "Default autocompletion replacement",
                AutoGenerateField = false)]
            public AutoCompletion.CaseMode NppAutoCompleteFilterCaseMode = AutoCompletion.CaseMode.Insensitive;

            [Display(Name = "Keywords list case sensitivity",
                Description = "Chose how keywords are added to the autocompletion list :<br>- a word can appear several times with different cases (sensitive)<br>- or only once no matter its case (insensitive)<br><br><i>The value to mimic the behavior of notepad++ would be sensitive</i>",
                GroupName = "Default autocompletion replacement",
                AutoGenerateField = true)]
            public AutoCompletion.CaseMode NppAutoCompleteParserCaseMode = AutoCompletion.CaseMode.Sensitive;

            /// <summary>
            /// PROGRESS AUTOCOMPLETION
            /// </summary>
            [Display(Name = "Show list in comments and strings",
                Description = "By default, the auto completion list is hidden in comments and strings<br>you can still show the completion list manually!",
                GroupName = "Progress autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteShowInCommentsAndStrings = false;

            [Display(Name = "Insert current suggestion on word end",
                Description = "You can check this option to automatically insert the currently selected suggestion<br>(if the list is opened)<br>when you enter any character that is not a letter/digit/_/-",
                GroupName = "Progress autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteInsertSelectedSuggestionOnWordEnd = true;

            [Display(Name = "Auto-case as I'm typing",
                Description = "Let 3P automatically correct the case of each word you type<br><i>If there is a match between the word you typed and an item in the autocompletion,<br>the case will be corrected with the autocompletion value</i>",
                GroupName = "Progress autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteAutoCase = true;

            [Display(Name = "Keywords case",
                Description = "Change the case of each word displayed in the auto completion to :<br>UPPERCASED (1), lowercased (2), CamelCased (3) or set as it appears in the documentation (0)",
                GroupName = "Progress autocompletion",
                AutoGenerateField = true)]
            [Range(0, 3)]
            public int AutoCompleteKeywordCaseMode = 0; // 0 = default, 1 = upper, 2 = lower, 3 = camel

            [Display(Name = "Database words case",
                Description = "Change the case of each information extracted from the database (db name, tables, fields, sequences) to :<br>UPPERCASED (1), lowercased (2) or CamelCased (3), or set as it appears in the database (0)",
                GroupName = "Progress autocompletion",
                AutoGenerateField = true)]
            [Range(0, 3)]
            public int AutoCompleteDatabaseWordCaseMode = 0; // 0 = default, 1 = upper, 2 = lower, 3 = camel

            [Display(Name = "Insert full word instead of abbreviations",
                Description = "Automatically replaces abbreviations by their full length counterparts",
                GroupName = "Progress autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteReplaceAbbreviations = true;

            [Display(Name = "Auto replace semicolon",
                Description = "Check to replace automatically ; by . <br><i>useful if you come from any other language!!!</i>",
                GroupName = "Progress autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteReplaceSemicolon = true;

            [Display(Name = "Only show already defined variables",
                Description = "By default, 3P filters the autocompletion list to only show you<br>the items that are available at the line where you activate<br>the autocompletion list.<br>You can set this option to false to show an item even if,<br>for the line where your cursor is, it is not yet defined.",
                GroupName = "Progress autocompletion",
                AutoGenerateField = false)]
            public bool AutoCompleteOnlyShowDefinedVar = true;

            // Height of the autocompletion form
            public int AutoCompleteShowListOfXSuggestions = 12;

            // Width of the autocompletion form
            public int AutoCompleteWidth = 310;

            public bool AutoCompleteNeverAskToDisableDefault = false;

            public string AutoCompleteItemPriorityList = "";

            #endregion

            #region CODE EDITION

            /// <summary>
            /// CODE EDITION
            /// </summary>
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

            [Display(Name = "Disable auto update of function prototypes",
                Description = "Toggle on to prevent 3P from automatically updating your functions prototypes according to their implementation<br>You are still able to manually trigger the update through the menu",
                GroupName = "Code edition",
                AutoGenerateField = false)]
            public bool DisablePrototypeAutoUpdate = false;

            [Display(Name = "Display parser errors on save",
                Description = "Each time you save a progress file, this will validate that it can be read by the AppBuilder.<br>It checks the length of each block and the syntax of AppBuilder pre-processed statements",
                GroupName = "Code edition",
                AutoGenerateField = false)]
            public bool DisplayParserErrorsOnSave = true;

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

            // Current folder mode for the file explorer : local/compilation/propath/everywhere
            public int FileExplorerDirectoriesToExplore = 0;

            #endregion

            #region CODE EXPLORER

            /// <summary>
            /// CODE EXPLORER
            /// </summary>
            public bool CodeExplorerVisible = true;

            public string CodeExplorerItemPriorityList = "";
            public bool CodeExplorerDisplayExternalItems = false;

            [Display(Name = "Auto-hide/show for progress documents",
                Description = "Check this option to automatically hide the Code explorer when the current isn't<br>a progress file, and automatically show it when it is",
                GroupName = "Code explorer",
                AutoGenerateField = false)]
            public bool CodeExplorerAutoHideOnNonProgressFile = false;

            public SortingType CodeExplorerSortingType;

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

            #region MISC

            public string AutoSwitchEncodingForFilePatterns = "";

            public NppEncodingFormat AutoSwitchEncodingTo = NppEncodingFormat._Automatic_default;

            #endregion

            // Shared configuration last folder selected
            public string SharedConfFolder = "";
            // a list of Label corresponding to confLine(s) that are auto-updated
            public string AutoUpdateConfList = "";
            // a list of folders used in the shared/exported conf
            public string SharedConfHistoric = "";

            // Values for the Tags
            public string TagModifOpener =
                "/* --- Modif #{&n} --- {&da} --- CS PROGRESS SOPRA ({&u}) --- [{&w} - {&b}] --- */";

            public string TagModifCloser =
                "/* --- Fin modif #{&n} --- */";

            public string TagTitleBlock1 =
                "/* |      |            |           |                                                                | */\n" +
                "/* | {&n }| {&da     } | CS-SOPRA  | {&w } - {&b                                                  } | */\n" +
                "/* |      | {&v      } | {&u     } |                                                                | */";

            public string TagTitleBlock2 =
                "/* |      |            |           | {&de                                                         } | */";

            public string TagTitleBlock3 =
                "/* |______|____________|___________|________________________________________________________________| */";

            // Current environment
            public string EnvLastDbInfoUsed = "";
            public string EnvName = "";
            public string EnvSuffix = "";

            // last ping time
            public string TechnicalLastPing = "";
            public string TechnicalMyUuid = Guid.NewGuid().ToString();
            public int TechnicalPingEveryXMin = 4 * 60;

            // last update check
            public string TechnicalLastCheckUpdate = "";
            public int TechnicalCheckUpdateEveryXMin = 6 * 60;

            // did the last update check went ok?
            public bool TechnicalLastCheckUpdateOk = true;

            // THEMES
            public int ThemeId = 0;
            public Color AccentColor = ColorTranslator.FromHtml("#647687");
            public int SyntaxHighlightThemeId = 0;
            public bool UseSyntaxHighlightTheme = true;

            public string InstalledDataDiggerVersion = "";

            // SHORTCUTS (id, spec)
            public Dictionary<string, string> ShortCuts = new Dictionary<string, string>();

            #region methods

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
                    return (T) Convert.ChangeType(null, typeof(T));
                }
                var listCustomAttr = property.GetCustomAttributes(typeof(T), false);
                if (!listCustomAttr.Any()) {
                    return (T) Convert.ChangeType(null, typeof(T));
                }
                var displayAttr = (T) listCustomAttr.FirstOrDefault();
                return displayAttr;
            }

            /// <summary>
            /// Returns the shortcut specs corresponding to the itemId given
            /// </summary>
            public string GetShortcutSpecFromName(string itemId) {
                return (ShortCuts.ContainsKey(itemId) ? ShortCuts[itemId] : "Unknown shortcut?");
            }

            #endregion
        }

        #endregion

        #region public fields

        public static string RequiredNppVersion {
            get { return "v7.3.2"; }
        }

        /// <summary>
        /// To update when updating the version of datadigger
        /// </summary>
        public static string EmbeddedDataDiggerVersion {
            get { return @"v22"; }
        }

        public static string DataDiggerVersionUrl {
            get { return @"https://datadigger.wordpress.com/"; }
        }

        /// <summary>
        /// Singleton instance of ConfigObject
        /// </summary>
        public static ConfigObject Instance {
            get { return _instance ?? (_instance = Init()); }
        }

        public static string GetUserAgent {
            get { return "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)"; }
        }

        /// <summary>
        /// Url for the github webservices
        /// </summary>
        public static string ReleasesApi {
            get { return @"https://api.github.com/repos/jcaillon/3P/releases"; }
        }

        public static string IssueUrl {
            get { return @"https://github.com/jcaillon/3P/issues"; }
        }

        // Convert.ToBase64String(Encoding.ASCII.GetBytes("user:mdp"));
        public static string _3PUserCredentials {
            get { return @"M3BVc2VyOnJhbmRvbXBhc3N3b3JkMTIz"; }
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
        public static string PingPostWebWervice {
            get { return @"http://noyac.fr/3pWebService/v1.6.4/?action=ping&softName=3p"; }
        }

        public static string BugsPostWebWervice {
            get { return @"http://noyac.fr/3pWebService/v1.6.4/?action=bugs&softName=3p"; }
        }

        public static string PingGetWebWervice {
            get { return @"http://noyac.fr/3pWebService/v1.6.4/?action=getPing&softName=3p"; }
        }

        public static string BugsGetWebWervice {
            get { return @"http://noyac.fr/3pWebService/v1.6.4/?action=getBugs&softName=3p"; }
        }

        /// <summary>
        /// Is developper = the file debug exists
        /// </summary>
        public static bool IsDevelopper {
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

        public static string FolderUpdate {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "Update")); }
        }

        public static string FolderTemplates {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "Templates")); }
        }

        public static string FolderThemes {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "Themes")); }
        }

        public static string FolderTemp {
            get { return CreateDirectory(Path.Combine(Path.GetTempPath(), AssemblyInfo.AssemblyProduct)); }
        }

        public static string FolderDataDigger {
            get { return CreateDirectory(Path.Combine(Npp.ConfigDirectory, "DataDigger")); }
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

        public static string FileProEnv {
            get { return Path.Combine(Npp.ConfigDirectory, "_ProgressEnvironnement.xml"); }
        }

        public static string FileSnippets {
            get { return Path.Combine(Npp.ConfigDirectory, "_SnippetList.conf"); }
        }

        public static string FileSettings {
            get { return Path.Combine(Npp.ConfigDirectory, "settings.xml"); }
        }

        public static string FileStartProlint {
            get { return Path.Combine(Npp.ConfigDirectory, "StartProlint.p"); }
        }

        public static string FileDeploymentHook {
            get { return Path.Combine(Npp.ConfigDirectory, "DeploymentHookProc.p"); }
        }

        public static string FileDeployProfiles {
            get { return Path.Combine(Npp.ConfigDirectory, "_DeploymentProfiles.conf"); }
        }

        // updates related
        public static string FileVersionLog {
            get { return Path.Combine(Npp.ConfigDirectory, "version.log"); }
        }

        public static string FilePreviousVersion {
            get { return Path.Combine(FolderUpdate, "previous.ver"); }
        }

        public static string FileDownloadedPlugin {
            get { return Path.Combine(FolderUpdate, "3P.dll"); }
        }

        public static string FileDownloadedPdb {
            get { return Path.Combine(FolderUpdate, "3P.pdb"); }
        }

        public static string FileUpdaterExe {
            get { return Path.Combine(FolderUpdate, "3pUpdater.exe"); }
        }

        public static string FileUpdaterLst {
            get { return Path.Combine(FolderUpdate, "3pUpdater.lst"); }
        }

        public static string FileLatestReleaseZip {
            get { return Path.Combine(FolderUpdate, "3P_latestRelease" + (Environment.Is64BitProcess ? "_x64" : "") + ".zip"); }
        }

        // name of the zip file containing the release in the assets of the release
        public static string FileGitHubAssetName {
            get { return @"3P" + (Environment.Is64BitProcess ? "_x64" : "") + ".zip"; }
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