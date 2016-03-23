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
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures {

    /// <summary>
    /// This class handle the Main context menu (and its children)
    /// It also has knownledge of the shortcuts for each item in the menu
    /// </summary>
    internal class AppliMenu {

        #region Static

        private static AppliMenu _instance;

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

        public static int DockableCommandIndex { get; set; }

        /// <summary>
        /// Show the appli main menu at the cursor location
        /// </summary>
        public static void ShowMainMenuAtCursor() {
            if (!Plug.PluginIsFullyLoaded) {
                return;
            }
            ShowMenuAtCursor((Abl.IsCurrentProgressFile() ? Instance.MainMenuList : Instance.MainMenuList.Where(item => item.Generic)).Select(item => (YamuiMenuItem)item).ToList(), "Main menu");
        }

        /// <summary>
        /// Show the generate code menu at the cursor location
        /// </summary>
        public static void ShowGenerateCodeMenuAtCursor() {
            if (Abl.IsCurrentProgressFile()) {
                ShowMenuAtCursor(Instance.GenerateCodeMenuList.Select(item => (YamuiMenuItem)item).ToList(), "Generate code", "GenerateCode");
            }
        }

        /// <summary>
        /// Show a given menu
        /// </summary>
        public static void ShowMenuAtCursor(List<YamuiMenuItem> menuList, string menuTitle, string menuLogo = "logo16x16") {
            try {
                // Close any already opened menu
                ForceClose();

                // open requested menu
                menuList.Insert(0, new YamuiMenuItem { IsSeparator = true });
                var menu = new YamuiMenu(Cursor.Position, menuList, "<div class='contextMenuTitle'><img src='" + menuLogo +"' width='16' Height='16' style='padding-right: 5px; padding-top: 1px;'>" + menuTitle + "</span>");
                menu.Show();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in ShowMenuAtCursor");
            }
        }

        /// <summary>
        /// Closes the visible menu (if any)
        /// </summary>
        public static void ForceClose() {
            if (YamuiMenu.ListOfOpenededMenuHandle != null && YamuiMenu.ListOfOpenededMenuHandle.Count > 0) {
                var curCtrl = (Control.FromHandle(YamuiMenu.ListOfOpenededMenuHandle[0]));
                var curMenu = curCtrl as YamuiMenu;
                if (curMenu != null) {
                    curMenu.CloseAll();
                }
            }
        }

        /// <summary>
        /// Returns a list containing all the keys used in the menu's items
        /// </summary>
        public static List<Keys> GetMenuKeysList(List<MenuItem> menu) {
            var output = new List<Keys>();
            foreach (var item in menu) {
                if (item.Shortcut.IsSet) {
                    output.Add(item.Shortcut.Key);
                }
                if (item.ChildrenList != null) {
                    output.AddRange(GetMenuKeysList(item.ChildrenList));
                }
            }
            return output;
        }

        #endregion

        #region fields

        public List<MenuItem> MainMenuList { get; set; }

        public List<MenuItem> GenerateCodeMenuList { get; set; }

        #endregion

        #region Life and death

        private AppliMenu() {

            GenerateCodeMenuList = new List<MenuItem> {
                new MenuItem("Insert new function", null, "Insert_new_func", "", ImageResources.Function),
                new MenuItem("Insert new internal procedure", null, "Insert_new_proc", "", ImageResources.Procedure)
            };

            var goToDefItem = new MenuItem("Go to definition", ProCodeUtils.GoToDefinition, "Go_To_Definition", "Ctrl+B", ImageResources.GoToDefinition);
            goToDefItem.SubText = "Middle click  /  " + goToDefItem.SubText;

            MainMenuList = new List<MenuItem> {
                new MenuItem("Show main window", Appli.Appli.ToggleView, "Open_main_window", "Alt+Space", ImageResources.MainWindow) {
                    Generic = true
                },
                new MenuItem("Show auto-completion at caret", AutoComplete.OnShowCompleteSuggestionList, "Show_Suggestion_List", "Ctrl+Space", ImageResources.Autocompletion),

                new MenuItem(true), // --------------------------

                new MenuItem("Open 4GL help", ProCodeUtils.Open4GlHelp, "Open_4GL_help", "F1", ImageResources.ProgressHelp) {
                    Generic = true
                },
                new MenuItem("Check syntax", () => ProCodeUtils.StartProgressExec(ExecutionType.CheckSyntax), "Check_syntax", "Shift+F1", ImageResources.CheckCode),
                new MenuItem("Compile", () => ProCodeUtils.StartProgressExec(ExecutionType.Compile), "Compile", "Alt+F1", ImageResources.CompileCode),
                new MenuItem("Run program", () => ProCodeUtils.StartProgressExec(ExecutionType.Run), "Run_program", "Ctrl+F1", ImageResources.RunCode),

                new MenuItem("Prolint code",  () => ProCodeUtils.StartProgressExec(ExecutionType.Prolint), "Prolint", "F12", ImageResources.ProlintCode),
                new MenuItem("Open in the AppBuilder", ProCodeUtils.OpenCurrentInAppbuilder, "Send_appbuilder", "Alt+O", ImageResources.SendToAppbuilder),
                new MenuItem("Open progress dictionary", ProCodeUtils.OpenDictionary, "Open_dictionary", "Alt+D", ImageResources.Dictionary) {
                    Generic = true
                },

                new MenuItem(true), // --------------------------

                new MenuItem("Start searching files", FileExplorer.FileExplorer.StartSearch, "Search_file", "Alt+Q", ImageResources.Search) {
                    Generic = true
                },
                goToDefItem,
                new MenuItem("Go to previous jump point", Npp.GoBackFromDefinition, "Go_Backwards", "Ctrl+Shift+B", ImageResources.GoBackward) {
                    Generic = true
                },

                new MenuItem(true), // --------------------------

                //new MenuItem("New 4GL file", ShowNewFileAtCursor, "New_file", "Ctrl+Shift+N", ImageResources.GenerateCode) {
                //    Children = GenerateCodeMenuList.Select(item => (YamuiMenuItem)item).ToList(),
                //},

                new MenuItem("Toggle comment line", ProCodeUtils.ToggleComment, "Toggle_Comment", "Ctrl+Q", ImageResources.ToggleComment),
                
                //new MenuItem("Insert mark", null, "Insert_mark", "Ctrl+T", ImageResources.InsertMark),
                //new MenuItem("Format document", CodeBeautifier.CorrectCodeIndentation, "Format_document", "Ctrl+I", ImageResources.FormatCode),
                
                //new MenuItem("Generate code", ShowGenerateCodeMenuAtCursor, "Generate_code", "Alt+Insert", ImageResources.GenerateCode) {
                //    Children = GenerateCodeMenuList.Select(item => (YamuiMenuItem)item).ToList(),
                //},

                new MenuItem(true), // --------------------------

                new MenuItem("Edit current file info", () => Appli.Appli.GoToPage(PageNames.FileInfo), "Edit_file_info", "Ctrl+Shift+M", ImageResources.FileInfo),
               new MenuItem("Insert title block", ProCodeUtils.AddTitleBlockAtCaret, "Insert_title_block", "Ctrl+Alt+M", ImageResources.TitleBlock),
               new MenuItem("Surround with modification tags", ProCodeUtils.SurroundSelectionWithTag, "Modif_tags", "Ctrl+M", ImageResources.ModificationTag),

               new MenuItem(true), // --------------------------

               new MenuItem("Options", () => Appli.Appli.GoToPage(PageNames.OptionsGeneral), "Go_to_options", null, ImageResources.ShowOptions)
            };

            if (Config.IsDevelopper) {
                MainMenuList.Add(
                    new MenuItem("Tests", ImageResources.Tests, new List<MenuItem> {
                        new MenuItem("Test", Plug.Test, "Test", "Ctrl+D", ImageResources.TestTube),
                        new MenuItem("DEBUG", Plug.StartDebug, "DEBUG", "Ctrl+F12", ImageResources.TestTube)
                    }));
            }
        }

        #endregion
    }

    #region MenuItem

    internal class MenuItem : YamuiMenuItem {

        /// <summary>
        /// Key of the item in the shortcut dictionnary stored in the config
        /// </summary>
        public string ItemId { get; set; }
        public string ItemSpec { get; set; }
        public ShortcutKey Shortcut { get; set; }
        public bool Generic { get; set; }
        public List<MenuItem> ChildrenList { get; set; }

        public MenuItem(string name, Action action, string itemId, string defaultKey, Image img) {
            ItemName = name;
            ItemImage = img;
            ItemId = itemId;
            OnClic = action;
            ItemSpec = defaultKey;
            if (!string.IsNullOrEmpty(defaultKey)) {
                if (Config.Instance.ShortCuts.ContainsKey(ItemId)) {
                    ItemSpec = Config.Instance.ShortCuts[ItemId];
                    Config.Instance.ShortCuts.Remove(ItemId);
                }
                Config.Instance.ShortCuts.Add(ItemId, ItemSpec);
                Shortcut = new ShortcutKey(ItemSpec);
                SubText = ItemSpec;
            }

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

        public MenuItem(string name, Image img, List<MenuItem> children) {
            ItemName = name;
            ItemImage = img;
            ChildrenList = children;
            Children = children.Select(item => (YamuiMenuItem) item).ToList();
        }

        public MenuItem(bool isSeparator) {
            IsSeparator = isSeparator;
        }
    }

    #endregion

}
