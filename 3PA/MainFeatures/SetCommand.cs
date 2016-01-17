#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (SetCommand.cs) is part of 3P.
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
using System.Windows.Forms;
using _3PA.Html;
using _3PA.Interop;

namespace _3PA.MainFeatures {

    /* Handle the Npp menu the old way :
     *  _menu.SetCommand("Edit current file info", Appli.GoToFileInfo, "Edit_file_info:Ctrl+Shift+M", false);
        _menu.SetCommand("Surround with modification tags", ProCodeUtils.SurroundSelectionWithTag, "Modif_tags:Ctrl+M", false);
        _menu.SetSeparator();
     */
    internal class NppMenu {

        #region fields

        private Dictionary<Keys, int> _uniqueKeys = new Dictionary<Keys, int>();

        public int CmdIndex { get; set; }

        #endregion


        #region public methods

        /// <summary>
        /// Returns the dictionnary of unique keys used by the shortcuts
        /// </summary>
        public Dictionary<Keys, int> UniqueKeys {
            get { return _uniqueKeys; }
        }

        /// <summary>
        /// Allows to set a plugin's command
        /// </summary>
        /// <param name="commandName">Name</param>
        /// <param name="functionPointer">Method to call on click</param>
        /// <param name="shortcutSpec">
        /// Composed of the name of the shortcut + the shortcut itself
        /// Ex : "_ShowSuggestionList:Ctrl+Space"
        /// </param>
        /// <param name="checkOnInit"></param>
        public void SetCommand(string commandName, Action functionPointer, string shortcutSpec, bool checkOnInit) {
            try {
                var parts = shortcutSpec.Split(':');
                var shortcutName = parts[0];
                var shortcutData = parts[1];
                ShortcutKey thisShortcut;

                if (String.IsNullOrWhiteSpace(shortcutData)) {
                    thisShortcut = new ShortcutKey();
                } else {
                    if (Config.Instance.ShortCuts.ContainsKey(shortcutName)) {
                        // get the shortkey already defined for this shortcutName
                        shortcutData = Config.Instance.ShortCuts[shortcutName];
                        Config.Instance.ShortCuts.Remove(shortcutName);
                    }
                    thisShortcut = new ShortcutKey(shortcutData);
                }
                _internalShortCuts.Add(thisShortcut, new Tuple<Action, int, string>(functionPointer, CmdIndex, shortcutName));
                Config.Instance.ShortCuts.Add(shortcutName, shortcutData);

                var key = (Keys) thisShortcut._key;
                if (!_uniqueKeys.ContainsKey(key))
                    _uniqueKeys.Add(key, 0);

                if (!String.IsNullOrWhiteSpace(commandName))
                    Npp.SetCommand(CmdIndex++, commandName, functionPointer, thisShortcut, checkOnInit);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in SetCommand");
            }
        }

        /// <summary>
        /// Sets a menu item without shortcut
        /// </summary>
        public void SetCommand(string commandName, Action functionPointer) {
            Npp.SetCommand(CmdIndex++, commandName, functionPointer);
        }

        /// <summary>
        /// Inserts a sperator, will be visible as an horizontal in the npp menu
        /// </summary>
        public void SetSeparator() {
            Npp.SetCommand(CmdIndex++, "---", null);
        }

        #endregion


        #region static

        /// <summary>
        /// new Tuple Action, int, string = (functionPointer, index, shortcutName)
        /// </summary>
        private static Dictionary<ShortcutKey, Tuple<Action, int, string>> _internalShortCuts = new Dictionary<ShortcutKey, Tuple<Action, int, string>>();

        public static Dictionary<ShortcutKey, Tuple<Action, int, string>> InternalShortCuts {
            get { return _internalShortCuts; }
        }

        private static bool _alreadyWarnedUserAboutShortkey;

        /// <summary>
        /// Called when the user changes the shortkey for a function
        /// </summary>
        public static void ShortcutsUpdated(int cmdId, ShortcutKey shortcut) {
            int index = 0;
            foreach (var item in UnmanagedExports.FuncItems.Items) {
                if (cmdId == UnmanagedExports.FuncItems.Items[index]._cmdID) {
                    break;
                }
                index++;
            }
            string shortcutName = "";
            foreach (var locshortcut in _internalShortCuts.Keys) {
                if (index == _internalShortCuts[locshortcut].Item2) {
                    shortcutName = _internalShortCuts[locshortcut].Item3;
                    break;
                }
            }
            if (!String.IsNullOrWhiteSpace(shortcutName)) {
                if (Config.Instance.ShortCuts.ContainsKey(shortcutName)) {
                    Config.Instance.ShortCuts.Remove(shortcutName);
                    Config.Instance.ShortCuts.Add(shortcutName, ShortcutKey2String(shortcut));
                    Config.Save();
                    if (!_alreadyWarnedUserAboutShortkey) {
                        UserCommunication.Notify("You successfully changed a shortcut.<br>Your change will be taken into account at the next notepad++ restart!", MessageImg.MsgInfo, "Information", "Shortcut modification");
                        _alreadyWarnedUserAboutShortkey = true;
                    }
                }
            }
        }

        /// <summary>
        /// Converts a shortcut into a string
        /// </summary>
        public static string ShortcutKey2String(ShortcutKey shortcut) {
            return (shortcut.IsCtrl ? "Ctrl+" : "") + (shortcut.IsShift ? "Shift+" : "") + (shortcut.IsAlt ? "Alt+" : "") + Enum.GetName(typeof (Keys), shortcut._key);
        }

        /// <summary>
        /// Returns the shortcut specs corresponding to the shortcut name given
        /// </summary>
        public static string GetShortcutSpecFromName(string shortcutName) {
            return (Config.Instance.ShortCuts.ContainsKey(shortcutName) ? Config.Instance.ShortCuts[shortcutName] : "Unknown shortcut");
        }

        #endregion

    }
}