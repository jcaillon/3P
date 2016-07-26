#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using YamuiFramework.Forms;
using _3PA.Images;
using _3PA.Interop;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecutionNs;
using _3PA.Tests;

namespace _3PA.MainFeatures {
     
    /// <summary>
    /// This class handle the Main context menu (and its children)
    /// It also has knownledge of the shortcuts for each item in the menu
    /// </summary>
    internal class AppliMenu : IDisposable {

        #region Core

        private static AppliMenu _instance;

        /// <summary>
        /// Set to null to reset the instance
        /// </summary>
        public static AppliMenu Instance {
            get { return _instance ?? (_instance = new AppliMenu()); }
            set {
                if (value == null) {
                    if (_instance != null)
                        _instance.Dispose();
                    _instance = null;
                }
            }
        }

        /// <summary>
        /// Command index in the notepad++ plugin menu
        /// </summary>
        public static int MainMenuCommandIndex { get; set; }

        /// <summary>
        /// Closes the visible menu (if any)
        /// </summary>
        public static void ForceCloseMenu() {
            if (YamuiMenu.ListOfOpenededMenuHandle != null && YamuiMenu.ListOfOpenededMenuHandle.Count > 0) {
                var curCtrl = (Control.FromHandle(YamuiMenu.ListOfOpenededMenuHandle[0]));
                var curMenu = curCtrl as YamuiMenu;
                if (curMenu != null) {
                    curMenu.CloseAll();
                }
            }
        }

        /// <summary>
        /// Show a given menu
        /// </summary>
        public static void ShowMenuAtCursor(List<YamuiMenuItem> menuList, string menuTitle, string menuLogo = "logo16x16", int minSize = 150) {
            try {
                // Close any already opened menu
                ForceCloseMenu();

                // open requested menu
                var copyMenuList = menuList.ToList();
                copyMenuList.Insert(0, new YamuiMenuItem { IsSeparator = true });

                var menu = new YamuiMenu(Cursor.Position, copyMenuList, "<div class='contextMenuTitle'><img src='" + menuLogo + "' width='16' Height='16' style='padding-right: 5px; padding-top: 1px;'>" + menuTitle + "</span>", minSize);
                menu.Show();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowMenuAtCursor");
            }
        }

        #endregion

        #region Show menus

        /// <summary>
        /// Show the appli main menu at the cursor location
        /// </summary>
        public static void ShowMainMenuAtCursor() {

            ShowMenuAtCursor(DisableItemIfNeeded(Instance._mainMenuList).Select(item => (YamuiMenuItem) item).ToList(), "Main menu");
        }

        /// <summary>
        /// Show the generate code menu at the cursor location
        /// </summary>
        public static void ShowGenerateCodeMenuAtCursor() {
            ShowMenuAtCursor(DisableItemIfNeeded(Instance._generateCodeMenuList).Select(item => (YamuiMenuItem) item).ToList(), "Generate code", "GenerateCode");
        }

        /// <summary>
        /// Show the menu at the cursor location
        /// </summary>
        public static void ShowDatabaseToolsMenuAtCursor() {
            ShowMenuAtCursor(DisableItemIfNeeded(Instance._databaseTools).Select(item => (YamuiMenuItem)item).ToList(), "Database tools", "DatabaseTools");
        }

        /// <summary>
        /// Show the environment menu at the cursor location
        /// </summary>
        public static void ShowEnvMenuAtCursor() {
            ShowMenuAtCursor(Instance._envMenuList, "Switch environment", "Env", 185);
        }

        /// <summary>
        /// Allows to disable the items in a list depending on the conditions (item must be generic or we must be on a progress file to Enable)
        /// </summary>
        /// <returns></returns>
        private static List<MenuItem> DisableItemIfNeeded(List<MenuItem> list) {
            var isCurrentFileProgressFile = Abl.IsCurrentProgressFile;
            list.ForEach(item => item.IsDisabled = !isCurrentFileProgressFile && (!item.Generic));
            return list;
        }

        #endregion

        #region fields

        /// <summary>
        /// Returns a list containing all the keys used in the menu's items
        /// </summary>
        public List<Keys> GetMenuKeysList {
            get {
                return (from item in ShortcutableItemList where item.Shortcut.IsSet select item.Shortcut.Key).ToList();
            }
        }

        /// <summary>
        /// List with ALL the menu items that have a ItemId (= have or can have a shortcut)
        /// </summary>
        public List<MenuItem> ShortcutableItemList { get; set; }

        private List<MenuItem> _mainMenuList;

        private List<MenuItem> _generateCodeMenuList;

        private List<MenuItem> _databaseTools;

        private List<YamuiMenuItem> _envMenuList;

        private MenuItem _envMenu;

        #endregion

        #region Life and death

        private AppliMenu() {

            // List of item that can be assigned to a shortcut
            ShortcutableItemList = new List<MenuItem> {
                // add the main menu here, so it can appear in the list of shortcut to set
                new MenuItem(null, "Open main menu", ImageResources.logo20x20, ShowMainMenuAtCursor, "Show_main_menu", "F6") {
                    Generic = true
                }
            };

            #region Environments

            // subscribe to env change event so the Env menu is always up to date
            ProEnvironment.OnEnvironmentChange += RebuildSwitchEnvMenu;
            _envMenu = new MenuItem(this, "Switch environment", ImageResources.Env, ShowEnvMenuAtCursor, "Switch_env", "Ctrl+E") {
                Generic = true
            };
            RebuildSwitchEnvMenu();

            #endregion

            #region Generate code

            _generateCodeMenuList = new List<MenuItem> {
                new MenuItem(this, "Insert new internal procedure", ImageResources.Procedure, ProGenerateCode.InsertCode<ParsedProcedure>, "Insert_new_procedure", "Alt+P"),
                new MenuItem(this, "Insert new function", ImageResources.Function, ProGenerateCode.InsertCode<ParsedImplementation>, "Insert_new_function", "Alt+F"),
                new MenuItem(true), // --------------------------
                new MenuItem(this, "Delete existing internal procedure", ImageResources.DeleteProcedure, ProGenerateCode.DeleteCode<ParsedProcedure>, "Delete_procedure", ""),
                new MenuItem(this, "Delete existing function", ImageResources.DeleteFunction, ProGenerateCode.DeleteCode<ParsedImplementation>, "Delete_function", ""),
                new MenuItem(true), // --------------------------
                new MenuItem(this, "Synchronize fonction prototypes", ImageResources.Synchronize, () => ProGenerateCode.UpdateFunctionPrototypesIfNeeded(), "Synchronize_prototypes", "Alt+S")
            };

            #endregion

            #region database tools

            _databaseTools = new List<MenuItem> {
                new MenuItem(this, "Progress desktop", ImageResources.ProDesktop, ProUtils.OpenProDesktop, "Pro_desktop", "") {
                    Generic = true
                },
                new MenuItem(true), // --------------------------
                new MenuItem(this, "Open data administration", ImageResources.DataAdmin, ProUtils.OpenDbAdmin, "Data_admin", "") {
                    Generic = true
                },
                new MenuItem(this, "Open progress dictionary", ImageResources.Dictionary, ProUtils.OpenDictionary, "Data_dictionary", "") {
                    Generic = true
                },
                new MenuItem(true), // --------------------------
                new MenuItem(this, "Explore and modify your data", ImageResources.DataDigger, ProUtils.OpenDataDigger, "Data_digger", "") {
                    Generic = true
                },
                new MenuItem(this, "Explore (read-only) your data", ImageResources.DataReader, ProUtils.OpenDataReader, "Data_reader", "") {
                    Generic = true
                },


            };

            #endregion


            #region All

            var goToDefItem = new MenuItem(this, "Go to definition", ImageResources.GoToDefinition, ProUtils.GoToDefinition, "Go_To_Definition", "Ctrl+B");
            goToDefItem.SubText = "Middle click  /  " + goToDefItem.SubText;
            var goToPreviousJump = new MenuItem(this, "Go to previous jump point", ImageResources.GoBackward, Npp.GoBackFromDefinition, "Go_Backwards", "Ctrl+Shift+B") {
                Generic = true
            };
            goToPreviousJump.SubText = "Ctrl + Middle click  /  " + goToPreviousJump.SubText;

            _mainMenuList = new List<MenuItem> {
                new MenuItem(this, "Show main window", ImageResources.MainWindow, Appli.Appli.ToggleView, "Open_main_window", "Alt+Space") {
                    Generic = true
                },
                new MenuItem(this, "Show auto-completion at caret", ImageResources.Autocompletion, AutoComplete.OnShowCompleteSuggestionList, "Show_Suggestion_List", "Ctrl+Space"),
                _envMenu,

                new MenuItem(true), // --------------------------

                new MenuItem(this, "Open 4GL help", ImageResources.ProgressHelp, ProUtils.Open4GlHelp, "Open_4GL_help", "F1") {
                    Generic = true
                },
                new MenuItem(this, "Check syntax", ImageResources.CheckCode, () => ProUtils.StartProgressExec(ExecutionType.CheckSyntax), "Check_syntax", "Shift+F1"),
                new MenuItem(this, "Compile", ImageResources.CompileCode, () => ProUtils.StartProgressExec(ExecutionType.Compile), "Compile", "Alt+F1"),
                new MenuItem(this, "Run program", ImageResources.RunCode, () => ProUtils.StartProgressExec(ExecutionType.Run), "Run_program", "Ctrl+F1"),

                new MenuItem(this, "Prolint code", ImageResources.ProlintCode, () => ProUtils.StartProgressExec(ExecutionType.Prolint), "Prolint", "F12"),
                new MenuItem(this, "Open in the AppBuilder", ImageResources.SendToAppbuilder, ProUtils.OpenCurrentInAppbuilder, "Send_appbuilder", "Alt+O"),

                new MenuItem(true), // --------------------------

                new MenuItem(this, "Database tools", ImageResources.DatabaseTools, ShowDatabaseToolsMenuAtCursor, "DatabaseTools", "Alt+D") {
                    Generic = true,
                    Children = _databaseTools.Select(item => (YamuiMenuItem)item).ToList(),
                },

                new MenuItem(this, "Generate and revise code", ImageResources.GenerateCode, ShowGenerateCodeMenuAtCursor, "Generate_code", "Alt+Insert") {
                    Children = _generateCodeMenuList.Select(item => (YamuiMenuItem)item).ToList(),
                },

                new MenuItem(true), // --------------------------

                new MenuItem(this, "Start searching files", ImageResources.Search, FileExplorer.FileExplorer.StartSearch, "Search_file", "Alt+Q") {
                    Generic = true
                },
                goToDefItem,
                goToPreviousJump,

                new MenuItem(true), // --------------------------

                //new MenuItem(this, "New 4GL file", ImageResources.GenerateCode, ShowNewFileAtCursor, "New_file", "Ctrl+Shift+N") {
                //    Children = GenerateCodeMenuList.Select(item => (YamuiMenuItem)item).ToList(),
                //},

                new MenuItem(this, "Toggle comment line", ImageResources.ToggleComment, ProGenerateCode.ToggleComment, "Toggle_Comment", "Ctrl+Q"),

                //new MenuItem(this, "Insert mark", ImageResources.InsertMark, null, "Insert_mark", "Ctrl+T"),
                //new MenuItem(this, "Format document", ImageResources.FormatCode, CodeBeautifier.CorrectCodeIndentation, "Format_document", "Ctrl+I"),
                
                new MenuItem(true), // --------------------------

                new MenuItem(this, "Edit current file info", ImageResources.FileInfo, () => Appli.Appli.GoToPage(PageNames.FileInfo), "Edit_file_info", "Ctrl+Shift+M"),
                new MenuItem(this, "Insert title block", ImageResources.TitleBlock, ProGenerateCode.AddTitleBlockAtCaret, "Insert_title_block", "Ctrl+Alt+M"),
                new MenuItem(this, "Surround with modification tags", ImageResources.ModificationTag, ProGenerateCode.SurroundSelectionWithTag, "Modif_tags", "Ctrl+M"),

                new MenuItem(true), // --------------------------

                new MenuItem(this, "Options", ImageResources.ShowOptions, () => Appli.Appli.GoToPage(PageNames.OptionsGeneral), "Go_to_options", null) {Generic = true}
            };

            #endregion

            #region special dev

            if (Config.IsDevelopper) {
                _mainMenuList.Add(
                    new MenuItem(this, "Tests", ImageResources.Tests, null, null, null, new List<MenuItem> {
                            new MenuItem(this, "DebugTest1", ImageResources.TestTube, PlugDebug.DebugTest1, "DebugTest1", "Ctrl+OemQuotes") {Generic = true},
                            new MenuItem(this, "DebugTest2", ImageResources.TestTube, PlugDebug.DebugTest2, "DebugTest2", "Alt+OemQuotes") {Generic = true},
                            new MenuItem(this, "DebugTest3", ImageResources.TestTube, PlugDebug.DebugTest3, "DebugTest3", "Shift+OemQuotes") {Generic = true},
                        }) {
                        Generic = true
                    });
            }

            #endregion

        }

        public void Dispose() {
            ProEnvironment.OnEnvironmentChange -= RebuildSwitchEnvMenu;
        }

        #endregion

        #region RebuildSwitchEnvMenu

        /// <summary>
        /// Called when an environement is modified/add or simply switched,
        /// rebuilds the environment menu
        /// </summary>
        private void RebuildSwitchEnvMenu() {
            _envMenuList = new List<YamuiMenuItem>();

            foreach (var env in ProEnvironment.GetList) {
                var name = env.Name;
                var suffix = env.Suffix;
                var existingItem = _envMenuList.FirstOrDefault(item => item.ItemName.Equals(env.Name));
                // add a new suffix item
                if (existingItem != null) {
                    var newSub = new YamuiMenuItem() {
                        ItemName = env.Suffix,
                        ItemImage = ImageResources.EnvSuffix,
                        OnClic = () => ProEnvironment.SetCurrent(name, suffix, null),
                        IsSelectedByDefault = name.Equals(Config.Instance.EnvName) && suffix.Equals(Config.Instance.EnvSuffix)
                    };
                    if (existingItem.Children != null)
                        existingItem.Children.Add(newSub);
                    else {
                        // also add the first sub item..
                        var firstItemSuffix = ((string)existingItem.Data) ?? "";
                        existingItem.Children = new List<YamuiMenuItem> {
                            new YamuiMenuItem {
                                ItemName = firstItemSuffix,
                                ItemImage = ImageResources.EnvSuffix,
                                OnClic = () => ProEnvironment.SetCurrent(name, firstItemSuffix, null),
                                IsSelectedByDefault = name.Equals(Config.Instance.EnvName) && firstItemSuffix.Equals(Config.Instance.EnvSuffix)
                            },
                            newSub };
                    }
                    existingItem.SubText = existingItem.Children.Count.ToString();
                } else {
                    // add a new env item
                    _envMenuList.Add(new YamuiMenuItem() {
                        ItemName = env.Name,
                        ItemImage = ImageResources.EnvName,
                        OnClic = () => ProEnvironment.SetCurrent(name, suffix, null),
                        Data = env.Suffix,
                        IsSelectedByDefault = name.Equals(Config.Instance.EnvName)
                    });
                }
            }

            _envMenu.Children = _envMenuList;
        }

        #endregion

    }

    #region MenuItem

    internal class MenuItem : YamuiMenuItem {

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

        public MenuItem(AppliMenu menuToRegisterTo, string name, Image img, Action action, string itemId, string defaultKey, List<MenuItem> children) {
            ItemName = name;
            ItemImage = img;

            // children?
            if (children != null) {
                ChildrenList = children;
                Children = children.Select(item => (YamuiMenuItem)item).ToList();
            }

            // shortcut?
            if (!string.IsNullOrEmpty(itemId)) {

                ItemId = itemId;
                ItemSpec = defaultKey;

                if (Config.Instance.ShortCuts.ContainsKey(ItemId)) {
                    ItemSpec = Config.Instance.ShortCuts[ItemId];
                    Config.Instance.ShortCuts.Remove(ItemId);
                }

                if (!string.IsNullOrEmpty(ItemSpec)) {
                    Config.Instance.ShortCuts.Add(ItemId, ItemSpec);
                    Shortcut = new ShortcutKey(ItemSpec);
                    SubText = ItemSpec;
                }

                // we set up a list of items to use in the shortcut page
                if (menuToRegisterTo != null)
                    menuToRegisterTo.ShortcutableItemList.Add(this);
            }

            // action?
            if (action != null) {
                OnClic = action;
                // We set the Do() action, which is the "go through" action when the OnClic action is activated
                Do = () => {
                    if (OnClic != null) {
                        try {
                            OnClic();
                        } catch (Exception e) {
                            ErrorHandler.ShowErrors(e, "Error in : " + ItemName);
                        }
                    }
                };
            }
        }

        public MenuItem(AppliMenu menuToRegisterTo, string name, Image img, Action action, string itemId, string defaultKey) :
            this(menuToRegisterTo, name, img, action, itemId, defaultKey, null) { }

        /// <summary>
        /// constructor for separators
        /// </summary>
        public MenuItem(bool isSeparator) {
            IsSeparator = isSeparator;
        }
    }

    #endregion

}
