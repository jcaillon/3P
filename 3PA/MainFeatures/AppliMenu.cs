#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (AppliMenu.cs) is part of 3P.
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
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls.YamuiList;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.ModificationsTag;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro;
using _3PA.NppCore;
using _3PA.Tests;
using _3PA._Resource;

namespace _3PA.MainFeatures {
    /// <summary>
    /// This class handle the Main context menu (and its children)
    /// It also has knowledge of the shortcuts for each item in the menu
    /// </summary>
    internal class AppliMenu {
        #region Core

        private static AppliMenu _instance;

        private static YamuiMenu _popup;

        /// <summary>
        /// Set to null to reset the instance
        /// </summary>
        public static AppliMenu Instance {
            get { return _instance ?? (_instance = new AppliMenu()); }
            set {
                if (value == null)
                    _instance = null;
            }
        }

        /// <summary>
        /// Command index in the notepad++ plugin menu
        /// </summary>
        public static int MainMenuCommandIndex { get; set; }

        /// <summary>
        /// Closes the visible menu (if any)
        /// </summary>
        public static bool ForceClose() {
            if (_popup != null) {
                try {
                    _popup.Close();
                    _popup.Dispose();
                } catch (Exception) {
                    // ignored
                }
            }
            return false;
        }

        /// <summary>
        /// Show a given menu
        /// </summary>
        public static void ShowMenu(List<YamuiMenuItem> menuList, string menuTitle, string menuLogo, bool showAtCursor = false, int minWidth = 250) {
            try {
                // Close any already opened menu
                ForceClose();

                if (menuLogo == null)
                    menuLogo = Utils.GetNameOf(() => ImageResources.Logo16x16);

                // open requested menu
                _popup = new YamuiMenu {
                    HtmlTitle = "<div class='contextMenuTitle'><img src='" + menuLogo + "' width='16' Height='16' style='padding-right: 5px; padding-top: 1px;'>" + menuTitle + "</span>",
                    SpawnLocation = Cursor.Position,
                    MenuList = menuList,
                    DisplayNbItems = true,
                    DisplayFilterBox = true,
                    FormMinSize = new Size(minWidth, 0)
                };
                if (!showAtCursor) {
                    _popup.ParentWindowRectangle = WinApi.GetWindowRect(Npp.CurrentSci.Handle);
                }
                _popup.YamuiList.ShowTreeBranches = Config.Instance.ShowTreeBranches;
                _popup.ClicItemWrapper = item => {
                    if (item.OnClic != null) {
                        try {
                            item.OnClic(item);
                        } catch (Exception e) {
                            ErrorHandler.ShowErrors(e, "Error in : " + item.DisplayText);
                        }
                    }
                };
                _popup.Show(Npp.Win32Handle);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowMenuAtCursor");
            }
        }

        #endregion

        #region Show menus

        public static void ShowMainMenu(bool showAtCursor = false) {
            ShowMenu(DisableItemIfNeeded(Instance.MainMenuList).Select(item => (YamuiMenuItem) item).ToList(), "Main menu", null, showAtCursor);
        }

        public static void ShowGenerateCodeMenu() {
            ShowMenu(DisableItemIfNeeded(Instance._generateCodeMenuList).Select(item => (YamuiMenuItem) item).ToList(), "Generate code", "GenerateCode");
        }

        public static void ShowDatabaseToolsMenu() {
            ShowMenu(DisableItemIfNeeded(Instance._databaseTools).Select(item => (YamuiMenuItem) item).ToList(), "Database tools", "DatabaseTools");
        }

        public static void ShowEnvMenu(bool showAtCursor = false) {
            Instance.RebuildSwitchEnvMenu();
            ShowMenu(Instance._envMenuList.Cast<YamuiMenuItem>().ToList(), "Switch environment", "Env", showAtCursor);
        }

        public static void ShowMiscMenu() {
            ShowMenu(DisableItemIfNeeded(Instance._modificationTagMenuList).Select(item => (YamuiMenuItem) item).ToList(), "Miscellaneous", "Miscellaneous");
        }

        public static void ShowEditCodeMenu() {
            ShowMenu(DisableItemIfNeeded(Instance._editCodeList).Select(item => (YamuiMenuItem) item).ToList(), "Edit code", "EditCode");
        }

        public static void ShowProgressToolsMenu() {
            ShowMenu(DisableItemIfNeeded(Instance._progressTools).Select(item => (YamuiMenuItem) item).ToList(), "Progress tools", "ProgressTools");
        }

        /// <summary>
        /// Allows to disable the items in a list depending on the conditions (item must be generic or we must be on a progress file to Enable)
        /// </summary>
        /// <returns></returns>
        private static List<MenuItem> DisableItemIfNeeded(List<MenuItem> list) {
            var isCurrentFileProgressFile = Npp.CurrentFile.IsProgress;
            foreach (var menu in list) {
                menu.IsDisabled = !isCurrentFileProgressFile && !menu.Generic;
                if (menu.Children != null)
                    foreach (var subMenu in menu.Children.Cast<MenuItem>()) {
                        subMenu.IsDisabled = !isCurrentFileProgressFile && (!menu.Generic || !subMenu.Generic);
                    }
            }
            return list;
        }

        #endregion

        #region fields

        /// <summary>
        /// Returns a list containing all the keys used in the menu's items
        /// </summary>
        public List<Keys> GetMenuKeysList {
            get { return (from item in ShortcutableItemList where item.Shortcut.IsSet select item.Shortcut.Key).ToList(); }
        }

        /// <summary>
        /// List with ALL the menu items that have a ItemId (= have or can have a shortcut)
        /// </summary>
        public List<MenuItem> ShortcutableItemList { get; set; }

        public List<MenuItem> MainMenuList;

        private List<MenuItem> _generateCodeMenuList;

        private List<MenuItem> _modificationTagMenuList;

        private List<MenuItem> _editCodeList;

        private List<MenuItem> _progressTools;

        private List<MenuItem> _databaseTools;

        private List<MenuItem> _envMenuList;

        #endregion

        #region Life and death

        private AppliMenu() {
            // List of item that can be assigned to a shortcut
            ShortcutableItemList = new List<MenuItem> {
                // add the main menu here, so it can appear in the list of shortcut to set
                new MenuItem(null, "Open main menu", ImageResources.Logo20x20, item => ShowMainMenu(), "Show_main_menu_", "Alt+C") {
                    Generic = true
                }
            };

            #region Progress tools

            _progressTools = new List<MenuItem> {
                new MenuItem(this, "Progress desktop", ImageResources.ProDesktop, item => ProMisc.OpenProDesktop(), "Pro_desktop", "") {Generic = true},
                new MenuItem(this, "Open in the AppBuilder", ImageResources.SendToAppbuilder, item => ProMisc.OpenCurrentInAppbuilder(), "Send_appbuilder", "Alt+O"),
                new MenuItem(true) {Generic = true}, // --------------------------
                new MenuItem(this, "PROLINT code", ImageResources.ProlintCode, item => ProMisc.StartProgressExec(ExecutionType.Prolint), "Prolint", "F12"),
                new MenuItem(this, "Deploy current file", ImageResources.Deploy, item => ProMisc.DeployCurrentFile(), "Deploy", "Ctrl+Alt+Prior") {Generic = true},
                new MenuItem(this, "Generate DEBUG-LIST", ImageResources.ExtDbg, item => ProMisc.StartProgressExec(ExecutionType.GenerateDebugfile, compilation => compilation.CompileWithDebugList = true), "Generate_debug-list", null),
                new MenuItem(this, "Generate LISTING", ImageResources.ExtLis, item => ProMisc.StartProgressExec(ExecutionType.GenerateDebugfile, compilation => compilation.CompileWithListing = true), "Generate_listing", null),
                new MenuItem(this, "Generate XREF", ImageResources.ExtXrf, item => ProMisc.StartProgressExec(ExecutionType.GenerateDebugfile, compilation => compilation.CompileWithXref = true), "Generate_xref", null),
                new MenuItem(this, "Generate XREF-XML", ImageResources.ExtXml, item => ProMisc.StartProgressExec(ExecutionType.GenerateDebugfile, compilation => {
                    compilation.CompileWithXref = true;
                    compilation.UseXmlXref = true;
                }), "Generate_xrefxml", null),
            };

            #endregion

            #region Generate code

            _generateCodeMenuList = new List<MenuItem> {
                new MenuItem(this, "Insert new internal procedure", ImageResources.Procedure, item => ProGenerateCode.Factory.InsertCode<ParsedProcedure>(), "Insert_new_procedure", "Alt+P"),
                new MenuItem(this, "Insert new function", ImageResources.Function, item => ProGenerateCode.Factory.InsertCode<ParsedImplementation>(), "Insert_new_function", "Alt+F"),
                new MenuItem(true), // --------------------------
                new MenuItem(this, "Delete existing internal procedure", ImageResources.DeleteProcedure, item => ProGenerateCode.Factory.DeleteCode<ParsedProcedure>(), "Delete_procedure", ""),
                new MenuItem(this, "Delete existing function", ImageResources.DeleteFunction, item => ProGenerateCode.Factory.DeleteCode<ParsedImplementation>(), "Delete_function", ""),
                new MenuItem(true), // --------------------------
                new MenuItem(this, "Synchronize function prototypes", ImageResources.Synchronize, item => ProGenerateCode.Factory.UpdateFunctionPrototypesIfNeeded(), "Synchronize_prototypes", "Alt+S")
            };

            #endregion

            #region Edit code

            _editCodeList = new List<MenuItem> {
                new MenuItem(this, "Display parser errors", ImageResources.DisplayParserResults, item => ProCodeFormat.DisplayParserErrors(), "Check_parser_errors", null),
                new MenuItem(this, "Toggle comment line", ImageResources.ToggleComment, item => ProMisc.ToggleComment(), "Toggle_Comment", "Ctrl+Q"),
                new MenuItem(this, "Correct document indentation", ImageResources.IndentCode, item => ProCodeFormat.CorrectCodeIndentation(), "Reindent_document", "Ctrl+I")
                //new MenuItem(this, "Format document", ImageResources.FormatCode, CodeBeautifier.CorrectCodeIndentation, "Format_document", "Ctrl+I"),
            };

            #endregion

            #region Misc

            _modificationTagMenuList = new List<MenuItem> {
                new MenuItem(this, "Edit current file info", ImageResources.FileInfo, item => Appli.Appli.GoToPage(PageNames.FileInfo), "Edit_file_info", "Ctrl+Shift+M"),
                new MenuItem(this, "Insert title block", ImageResources.TitleBlock, item => ModificationTag.AddTitleBlockAtCaret(), "Insert_title_block", "Ctrl+Alt+M"),
                new MenuItem(this, "Surround with modification tags", ImageResources.ModificationTag, item => ModificationTag.SurroundSelectionWithTag(), "Modif_tags", "Ctrl+M")
                //new MenuItem(this, "Insert mark", ImageResources.InsertMark, null, "Insert_mark", "Ctrl+T"),
            };

            #endregion

            #region database tools

            _databaseTools = new List<MenuItem> {
                new MenuItem(this, "Open data administration", ImageResources.DataAdmin, item => ProMisc.OpenDbAdmin(), "Data_admin", "") {Generic = true},
                new MenuItem(this, "Open progress dictionary", ImageResources.Dictionary, item => ProMisc.OpenDictionary(), "Data_dictionary", "") {Generic = true},
                new MenuItem(true) {Generic = true}, // --------------------------
                new MenuItem(this, "Explore and modify your data", ImageResources.DataDigger, item => ProMisc.OpenDataDigger(), "Data_digger", "") {Generic = true},
                new MenuItem(this, "Explore (read-only) your data", ImageResources.DataReader, item => ProMisc.OpenDataReader(), "Data_reader", "") {Generic = true}
            };

            #endregion

            #region Main menu

            var goToDefItem = new MenuItem(this, "Go to definition", ImageResources.GoToDefinition, item => ProMisc.GoToDefinition(false), "Go_To_Definition", "Ctrl+B") { Generic = true };
            goToDefItem.SubText = "Middle click  /  " + goToDefItem.SubText;
            var goToPreviousJump = new MenuItem(this, "Go to previous jump point", ImageResources.GoBackward, item => Npp.GoBackFromDefinition(), "Go_Backwards", "Ctrl+Shift+B") {
                Generic = true
            };
            goToPreviousJump.SubText = "Ctrl + Middle click  /  " + goToPreviousJump.SubText;

            MainMenuList = new List<MenuItem> {
                new MenuItem(this, "Show main window", ImageResources.MainWindow, item => Appli.Appli.ToggleView(), "Open_main_window", "Alt+Space") {Generic = true},
                new MenuItem(this, "Show autocompletion at caret", ImageResources.Autocompletion, item => AutoCompletion.OnShowCompleteSuggestionList(), "Show_Suggestion_List", "Ctrl+Space") {Generic = true},
                new MenuItem(true) {Generic = true}, // --------------------------
                new MenuItem(this, "Open 4GL help", ImageResources.ProgressHelp, item => ProMisc.Open4GlHelp(), "Open_4GL_help", "F1") {Generic = true},
                new MenuItem(this, "Check syntax", ImageResources.CheckCode, item => ProMisc.StartProgressExec(ExecutionType.CheckSyntax), "Check_syntax", "Shift+F1"),
                new MenuItem(this, "Run program", ImageResources.RunCode, item => ProMisc.StartProgressExec(ExecutionType.Run), "Run_program", "Ctrl+F1"),
                new MenuItem(this, "Compile", ImageResources.CompileCode, item => ProMisc.StartProgressExec(ExecutionType.Compile), "Compile", "Alt+F1"),
                new MenuItem(this, "Compile options", ImageResources.CompileOptions, item => ProMisc.OpenCompilationOptions(), "Compile_options", null),
                new MenuItem(this, "Progress tools", ImageResources.ProgressTools, item => ShowProgressToolsMenu(), "Progress_tools", "Alt+T") {
                    Generic = true,
                    Children = _progressTools.Cast<FilteredTypeTreeListItem>().ToList()
                },
                new MenuItem(true) {Generic = true}, // --------------------------
                new MenuItem(this, "Start searching files", ImageResources.Search, item => FileExplorer.FileExplorer.Instance.StartSearch(), "Search_file", "Alt+Q") {Generic = true},
                goToDefItem,
                goToPreviousJump,
                //new MenuItem(this, "New 4GL file", ImageResources.GenerateCode, ShowNewFileAtCursor, "New_file", "Ctrl+Shift+N") {
                //    Children = GenerateCodeMenuList.Select(item => (YamuiMenuItem)item).ToList(),
                //},
                new MenuItem(true) {Generic = true}, // --------------------------
                new MenuItem(this, "Switch environment", ImageResources.Env, item => ShowEnvMenu(), "Switch_env", "Ctrl+E") {
                    Generic = true
                },
                new MenuItem(this, "Database tools", ImageResources.DatabaseTools, item => ShowDatabaseToolsMenu(), "DatabaseTools", "Alt+D") {
                    Generic = true,
                    Children = _databaseTools.Cast<FilteredTypeTreeListItem>().ToList()
                },
                new MenuItem(this, "Generate and revise code", ImageResources.GenerateCode, item => ShowGenerateCodeMenu(), "Generate_code", "Alt+Insert") {
                    Generic = true,
                    Children = _generateCodeMenuList.Cast<FilteredTypeTreeListItem>().ToList()
                },
                new MenuItem(this, "Edit code", ImageResources.EditCode, item => ShowEditCodeMenu(), "Edit_code", "Alt+E") {
                    Generic = true,
                    Children = _editCodeList.Cast<FilteredTypeTreeListItem>().ToList()
                },
                new MenuItem(this, "Modification tag", ImageResources.ModificationTagMenu, item => ShowMiscMenu(), "Modification_tag", "Alt+M") {
                    Generic = true,
                    Children = _modificationTagMenuList.Cast<FilteredTypeTreeListItem>().ToList()
                },
                new MenuItem(true) {Generic = true}, // --------------------------
                new MenuItem(this, "Options", ImageResources.ShowOptions, item => Appli.Appli.GoToPage(PageNames.OptionsGeneral), "Go_to_options", null) {Generic = true}
            };

            #endregion

            #region special dev

            if (Config.IsDeveloper) {
                MainMenuList.Add(
                    new MenuItem(this, "Tests", ImageResources.Tests, null, null, null, new List<MenuItem> {
                        new MenuItem(this, "DebugTest1", ImageResources.TestTube, item => PlugDebug.DebugTest1(), "DebugTest1", "Ctrl+OemQuotes") {Generic = true},
                        new MenuItem(this, "DebugTest2", ImageResources.TestTube, item => PlugDebug.DebugTest2(), "DebugTest2", "Alt+OemQuotes") {Generic = true},
                        new MenuItem(this, "DebugTest3", ImageResources.TestTube, item => PlugDebug.DebugTest3(), "DebugTest3", "Shift+OemQuotes") {Generic = true},
                        new MenuItem(this, "Parse current file", ImageResources.TestTube, item => PlugDebug.ParseCurrentFile(), "ParseCurrentFile", "") {Generic = true},
                        new MenuItem(this, "Parse reference file", ImageResources.TestTube, item => PlugDebug.ParseReferenceFile(), "ParseReferenceFile", "") {Generic = true},
                        new MenuItem(this, "Parse all files", ImageResources.TestTube, item => PlugDebug.ParseAllFiles(), "ParseAllFiles", "") {Generic = true}
                    }) {
                        Generic = true
                    });
            }

            #endregion
        }

        #endregion

        #region RebuildSwitchEnvMenu

        /// <summary>
        /// Called when an environement is modified/add or simply switched,
        /// rebuilds the environment menu
        /// </summary>
        public void RebuildSwitchEnvMenu() {
            _envMenuList = new List<MenuItem>();

            foreach (var env in ProEnvironment.GetList) {
                var name = env.Name;
                var suffix = env.Suffix;
                var existingItem = _envMenuList.FirstOrDefault(item => item.DisplayText.Equals(env.Name));
                // add a new suffix item
                if (existingItem != null) {
                    var newSub = new YamuiMenuItem {
                        DisplayText = env.Suffix,
                        ItemImage = ImageResources.EnvSuffix,
                        OnClic = item => ProEnvironment.SetCurrent(name, suffix, null),
                        IsSelectedByDefault = name.Equals(Config.Instance.EnvName) && suffix.Equals(Config.Instance.EnvSuffix),
                        IsExpanded = true
                    };
                    if (existingItem.Children != null)
                        existingItem.Children.Add(newSub);
                    else {
                        // also add the first sub item..
                        var firstItemSuffix = ((string) existingItem.Data) ?? "";
                        existingItem.Children = new List<FilteredTypeTreeListItem> {
                            new YamuiMenuItem {
                                DisplayText = firstItemSuffix,
                                ItemImage = ImageResources.EnvSuffix,
                                OnClic = item => ProEnvironment.SetCurrent(name, firstItemSuffix, null),
                                IsSelectedByDefault = name.Equals(Config.Instance.EnvName) && firstItemSuffix.Equals(Config.Instance.EnvSuffix),
                                IsExpanded = true
                            },
                            newSub
                        };
                    }
                    existingItem.SubText = existingItem.Children.Count.ToString();
                } else {
                    // add a new env item
                    _envMenuList.Add(new MenuItem(false) {
                        DisplayText = env.Name,
                        ItemImage = ImageResources.EnvName,
                        OnClic = item => ProEnvironment.SetCurrent(name, suffix, null),
                        Data = env.Suffix,
                        IsExpanded = true
                    });
                }
            }
        }

        #endregion
    }

    #region MenuItem

    internal sealed class MenuItem : YamuiMenuItem {
        /// <summary>
        /// Key of the item in the shortcut dictionnary stored in the config,
        /// must not be null if you want to be able to set a shortcut for the item
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Same as ItemId, it must be set if you want to be able to use/set shortcuts on children
        /// </summary>
        public List<MenuItem> ChildrenList { get; set; }

        /// <summary>
        /// Item shorcut (string format)
        /// </summary>
        public string ItemSpec { get; set; }

        /// <summary>
        /// Item shortcut
        /// </summary>
        public ShortcutKey Shortcut { get; set; }

        /// <summary>
        /// true if the item must appear no matter which doc we are on, otherwise the menu appears for 3P documents
        /// </summary>
        public bool Generic { get; set; }

        /// <summary>
        /// You can use this field to store any piece of info on this menu item
        /// </summary>
        public object Data { get; set; }

        public MenuItem(AppliMenu menuToRegisterTo, string name, Image img, Action<YamuiMenuItem> action, string itemId, string defaultKey, List<MenuItem> children) {
            DisplayText = name;
            ItemImage = img;

            // children?
            if (children != null) {
                ChildrenList = children;
                Children = children.Cast<FilteredTypeTreeListItem>().ToList();
            }

            // shortcut?
            if (!string.IsNullOrEmpty(itemId)) {
                ItemId = itemId;
                ItemSpec = defaultKey;

                if (Config.Instance.ShortCuts.ContainsKey(ItemId)) {
                    ItemSpec = Config.Instance.ShortCuts[ItemId];
                    Config.Instance.ShortCuts.Remove(ItemId);
                }

                Config.Instance.ShortCuts.Add(ItemId, ItemSpec);

                if (!string.IsNullOrEmpty(ItemSpec)) {
                    Shortcut = new ShortcutKey(ItemSpec);
                    SubText = ItemSpec;
                }

                // we set up a list of items to use in the shortcut page
                if (menuToRegisterTo != null)
                    menuToRegisterTo.ShortcutableItemList.Add(this);
            }

            // action?
            OnClic = action;

            // default is not expanded
            IsExpanded = false;
        }

        public MenuItem(AppliMenu menuToRegisterTo, string name, Image img, Action<YamuiMenuItem> action, string itemId, string defaultKey) :
            this(menuToRegisterTo, name, img, action, itemId, defaultKey, null) {}

        /// <summary>
        /// constructor for separators
        /// </summary>
        public MenuItem(bool isSeparator) {
            IsSeparator = isSeparator;
        }
    }

    #endregion
}