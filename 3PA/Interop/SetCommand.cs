#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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
using YamuiFramework.Forms;
using _3PA.Lib;
using _3PA.MainFeatures;

namespace _3PA.Interop {
    public class Plug {

        /// <summary>
        /// new Tuple Action, int, string = (functionPointer, index, shortcutName)
        /// </summary>
        public static Dictionary<ShortcutKey, Tuple<Action, int, string>> InternalShortCuts { get; set; }

        /// <summary>
        ///     Main SetCommand
        /// </summary>
        /// <param name="index">index of the command</param>
        /// <param name="commandName">Name</param>
        /// <param name="functionPointer">Method to call on click</param>
        /// <param name="shortcutSpec">
        ///     Composed of the name of the shortcut + the shortcut itself
        ///     Ex : "_ShowSuggestionList:Ctrl+Space"
        /// </param>
        /// <param name="checkOnInit"></param>
        /// <param name="uniqueKeys">Dictionnary to add all the keys in the KeyInterceptor instance</param>
        public static void SetCommand(int index, string commandName, Action functionPointer, string shortcutSpec,
            bool checkOnInit, Dictionary<Keys, int> uniqueKeys) {
            try {
                if (InternalShortCuts == null)
                    InternalShortCuts = new Dictionary<ShortcutKey, Tuple<Action, int, string>>();
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
                InternalShortCuts.Add(thisShortcut, new Tuple<Action, int, string>(functionPointer, index, shortcutName));
                Config.Instance.ShortCuts.Add(shortcutName, shortcutData);

                var key = (Keys) thisShortcut._key;
                if (!uniqueKeys.ContainsKey(key))
                    uniqueKeys.Add(key, 0);

                if (!String.IsNullOrWhiteSpace(commandName))
                    SetCommand(index, commandName, functionPointer, thisShortcut, checkOnInit);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in SetCommand");
            }
        }

        /// <summary>
        ///  Used only for separators!
        /// </summary>
        /// <param name="index"></param>
        /// <param name="commandName"></param>
        /// <param name="functionPointer"></param>
        internal static void SetCommand(int index, string commandName, Action functionPointer) {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), false);
        }
        
        internal static void SetCommand(int index, string commandName, Action functionPointer, ShortcutKey shortcut, bool checkOnInit) {
            var funcItem = new FuncItem();
            funcItem._cmdID = index;
            funcItem._itemName = commandName;
            if (functionPointer != null)
                funcItem._pFunc = functionPointer;
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            _3PA.Plug.FuncItems.Add(funcItem);
        }

        private static bool _alreadyWarnedUserAboutShortkey;

        /// <summary>
        /// Called when the user changes the shortkey for a function
        /// </summary>
        /// <param name="cmdId"></param>
        /// <param name="shortcut"></param>
        public static void ShortcutsUpdated(int cmdId, ShortcutKey shortcut) {
            int index = 0;
            foreach (var item in _3PA.Plug.FuncItems.Items) {
                if (cmdId == _3PA.Plug.FuncItems.Items[index]._cmdID) {
                    break;
                }
                index++;
            }
            string shortcutName = "";
            foreach (var locshortcut in InternalShortCuts.Keys) {
                if (index == InternalShortCuts[locshortcut].Item2) {
                    shortcutName = InternalShortCuts[locshortcut].Item3;
                    break;
                }
            }
            if (!String.IsNullOrWhiteSpace(shortcutName)) {
                if (Config.Instance.ShortCuts.ContainsKey(shortcutName)) {
                    Config.Instance.ShortCuts.Remove(shortcutName);
                    Config.Instance.ShortCuts.Add(shortcutName, ShortcutKey2String(shortcut));
                    Config.Save();
                    if (!_alreadyWarnedUserAboutShortkey) {
                        UserCommunication.Notify("You successfully changed a shortcut.<br>Your change will be taken into account at the next notepad++ restart!", MessageImage.Pin, "Information", "Shortcut modification");
                        _alreadyWarnedUserAboutShortkey = true;
                    }
                }
            }
        }

        /// <summary>
        /// Converts a shortcut into a string
        /// </summary>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        public static string ShortcutKey2String(ShortcutKey shortcut) {
            return (shortcut.IsCtrl ? "Ctrl+" : "") + (shortcut.IsShift ? "Shift+" : "") + (shortcut.IsAlt ? "Alt+" : "") + Enum.GetName(typeof(Keys), shortcut._key);
        }

        /// <summary>
        /// Returns the shortcut specs corresponding to the shortcut name given
        /// </summary>
        /// <param name="shortcutName"></param>
        /// <returns></returns>
        public static string GetShortcutSpecFromName(string shortcutName) {
            return (Config.Instance.ShortCuts.ContainsKey(shortcutName) ? Config.Instance.ShortCuts[shortcutName] : "Unknown shortcut");
        }
    }
}