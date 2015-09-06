using System;
using System.Collections.Generic;
using System.Windows.Forms;
using _3PA.Lib;

namespace _3PA.Interop {
    public partial class Plug {

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
                        UserCommunication.MessageUser("You successfully changed a shortcut, a restart of notepad++ is needed to correctly take this change into account");
                        _alreadyWarnedUserAboutShortkey = true;
                    }
                }
            }
        }

        public static string ShortcutKey2String(ShortcutKey shortcut) {
            return (shortcut.IsCtrl ? "Ctrl+" : "") + (shortcut.IsShift ? "Shift+" : "") + (shortcut.IsAlt ? "Alt+" : "") + Enum.GetName(typeof(Keys), shortcut._key);
        }
    }
}