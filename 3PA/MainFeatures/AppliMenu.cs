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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Forms;
using _3PA.Images;
using _3PA.Interop;
using _3PA.MainFeatures.AutoCompletion;

namespace _3PA.MainFeatures {

    internal class AppliMenu {

        #region Static

        private static AppliMenu _instance;

        public static AppliMenu Instance {
            get { return _instance ?? (_instance = new AppliMenu()); }
        }

        public static int DockableCommandIndex { get; set; }

        public static void DoShowMenuAtCursor() {
            Instance.ShowMenuAtCursor();
        }

        #endregion

        #region fields



        public List<MenuItem> MenuContent { get; private set; }

        #endregion

        #region Life and death

        private AppliMenu() {
            MenuContent = new List<MenuItem> {
                new MenuItem("Show main window", Appli.Appli.ToggleView, "Open_main_window", "Alt+Space", ImageResources.MainBlock) {
                    AlwaysShow = true,
                    //Children = new List<YamuiMenuItem> {
                        
                    //},
                },
                new MenuItem("Show auto-complete suggestions", AutoComplete.OnShowCompleteSuggestionList, "Show_Suggestion_List", "Ctrl+Space", ImageResources.MainBlock),
                new MenuItem("Open 4GL help", ProCodeUtils.Open4GlHelp, "Open_4GL_help", "F1", ImageResources.MainBlock) {
                    AlwaysShow = true,
                },
                new MenuItem("Check syntax", ProCodeUtils.CheckSyntaxCurrent, "Check_syntax", "Shift+F1", ImageResources.MainBlock),
                new MenuItem("Compile", ProCodeUtils.CompileCurrent, "Compile", "Alt+F1", ImageResources.MainBlock),
                new MenuItem("Run program", ProCodeUtils.RunCurrent, "Run_program", "Ctrl+F1", ImageResources.MainBlock),
                //new MenuItem("Prolint code", ProgressCodeUtils.NotImplemented, "Prolint", "F12", ImageResources.MainBlock),
                new MenuItem("Search file", FileExplorer.FileExplorer.StartSearch, "Search_file", "Alt+Q", ImageResources.MainBlock) {
                    AlwaysShow = true,
                },
                new MenuItem("Go to definition", ProCodeUtils.GoToDefinition, "Go_To_Definition", "Ctrl+B", ImageResources.MainBlock),
                new MenuItem("Go backwards", Npp.GoBackFromDefinition, "Go_Backwards", "Ctrl+Shift+B", ImageResources.MainBlock) {
                    AlwaysShow = true,
                },
                new MenuItem("Toggle comment line", ProCodeUtils.ToggleComment, "Toggle_Comment", "Ctrl+Q", ImageResources.MainBlock),
                //new MenuItem("Insert mark", ProgressCodeUtils.NotImplemented, "Insert_mark", "Ctrl+T", ImageResources.MainBlock),
                //new MenuItem("Format document", CodeBeautifier.CorrectCodeIndentation, "Format_document, Ctrl+I", ImageResources.MainBlock),
                //new MenuItem("Send to AppBuilder", ProgressCodeUtils.NotImplemented, "Send_appbuilder", "Alt+O", ImageResources.MainBlock),
                new MenuItem("Edit current file info", Appli.Appli.GoToFileInfo, "Edit_file_info", "Ctrl+Shift+M", ImageResources.MainBlock),
                //new MenuItem("Insert title block", ProgressCodeUtils.NotImplemented, "Insert_title_block", "Ctrl+Alt+M", ImageResources.MainBlock),
                new MenuItem("Surround with modification tags", ProCodeUtils.SurroundSelectionWithTag, "Modif_tags", "Ctrl+M", ImageResources.MainBlock),
                new MenuItem("Generate code", Appli.Appli.ToggleView, "Generate_code", "Alt+Insert", ImageResources.MainBlock) {
                    Children = new List<YamuiMenuItem> {
                        new MenuItem("Insert new function", null, "Insert_new_func", "", ImageResources.MainBlock),
                        new MenuItem("Insert new internal procedure", null, "Insert_new_proc", "", ImageResources.MainBlock),
                    },
                },

                new MenuItem("Test", Plug.Test, "Test", "Ctrl+D", ImageResources.MainBlock),
                new MenuItem("DEBUG", Plug.StartDebug, "DEBUG", "Ctrl+F12", ImageResources.MainBlock),
            };
        }

        #endregion

        public void ShowMenuAtCursor() {
            if (YamuiMenu.ListOfOpenededMenuHandle != null && YamuiMenu.ListOfOpenededMenuHandle.Count > 0) {
                var curCtrl = (Control.FromHandle(YamuiMenu.ListOfOpenededMenuHandle[0]));
                var curMenu = curCtrl as YamuiMenu;
                if (curMenu != null) {
                    curMenu.CloseAll();
                }
            }
            List<YamuiMenuItem> content;
            if (Abl.IsCurrentFileFromAppBuilder()) {
                content = MenuContent.Select(item => (YamuiMenuItem) item).ToList();
            } else {
                content = MenuContent.Where(item => item.AlwaysShow).Select(item => (YamuiMenuItem)item).ToList();
            }
            var menu = new YamuiMenu(Cursor.Position, content, "<div class='contextMenuTitle'><img src='logo16x16' style='padding-right: 7px; padding-top: 1px;'>Main menu</span>");
            menu.Show();
        }
    }

    internal class MenuItem : YamuiMenuItem {

        /// <summary>
        /// Key of the item in the shortcut dictionnary stored in the config
        /// </summary>
        public string ItemId { get; set; }
        public string ItemSpec { get; set; }
        public ShortcutKey Shortcut { get; set; }
        public bool AlwaysShow { get; set; }

        public MenuItem(string name, ClicAction action, string itemId, string defaultKey, Image img) {
            ItemName = name;
            ItemImage = img;
            ItemId = itemId;
            OnClic = action;
            ItemSpec = defaultKey;
            if (!string.IsNullOrEmpty(defaultKey)) {
                SubText = ItemSpec;
                if (Config.Instance.ShortCuts.ContainsKey(ItemId)) {
                    ItemSpec = Config.Instance.ShortCuts[ItemId];
                    Config.Instance.ShortCuts.Remove(ItemId);
                }
                Shortcut = new ShortcutKey(ItemSpec);
            }
        }

        public MenuItem(string name, Image img, List<YamuiMenuItem> children) {
            ItemName = name;
            ItemImage = img;
            Children = children;
        }
    }
}
