#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (NotificationsHandler.cs) is part of 3P.
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
using System.Linq;
using System.Windows.Forms;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA {

    internal static partial class Plug {

        #region Static events

        // NOTE : be aware that if you subscribe to one of those events, a reference to the subscribing object is held by the publisher (this class). That means that you have to be very careful about explicitly unsubscribing from static events as they will keep the subscriber alive forever, i.e., you may end up with the managed equivalent of a memory leak.

        /// <summary>
        /// Published when Npp is shutting down, do your clean up actions
        /// </summary>
        public static event Action OnNppShutDown;

        /// <summary>
        /// Event published when notepad++ is ready and the plugin can do its init, you must return true if the init went ok, false otherwise
        /// </summary>
        public static event Func<bool> OnNppReady;

        public static event Action OnSetFuncItems;

        /// <summary>
        /// Envent published when the plugin is ready
        /// </summary>
        public static event Action OnPlugReady;

        /// <summary>
        /// Event published on key down
        /// </summary>
        public static event Func<Keys, KeyModifiers, bool> OnKeyDown;

        /// <summary>
        /// Event published on mouse message
        /// </summary>
        public static event Func<WinApi.WindowsMessageMouse, WinApi.MOUSEHOOKSTRUCT, bool> OnMouseMessage;

        #endregion

        #region Members

        /// <summary>
        /// this is a delegate to defined actions that must be taken after updating the ui
        /// </summary>
        public static Queue<Action> ActionsAfterUpdateUi = new Queue<Action>();

        /// <summary>
        /// Set to true after the plugin has been fully loaded
        /// </summary>
        public static bool PluginIsReady { get; private set; }

        #endregion

        #region OnFuncItemsNeeded

        /// <summary>
        /// Method called when notepad++ requires the function item array to be filled
        /// </summary>
        public static void OnFuncItemsNeeded() {
            if (OnSetFuncItems != null)
                OnSetFuncItems();
        }

        #endregion

        #region OnPlugLoad

        /// <summary>
        /// Method called when notepad++ loads our plugin
        /// </summary>
        public static void OnPlugLoad() {
            Main();
        }

        #endregion

        #region Npp notifications

        /// <summary>
        /// handles the notifications send by npp and scintilla to the plugin
        /// </summary>
        public static void OnNppNotification(SCNotification nc) {
            try {

                uint code = nc.nmhdr.code;

                #region Basic notifications

                switch (code) {
                    case (uint) NppNotif.NPPN_TBMODIFICATION:
                        UnmanagedExports.FuncItems.RefreshItems();
                        InitToolbarImages();
                        return;

                    case (uint) NppNotif.NPPN_READY:
                        // notify plugins that all the procedures of launchment of notepad++ are done
                        // call OnNppReady then OnPlugReady if it all went ok
                        PluginIsReady = OnNppReady == null || OnNppReady();
                        if (PluginIsReady) {
                            if (OnPlugReady != null)
                                OnPlugReady();

                            // set hooks on mouse/keyboard
                            SetHooks();
                        }
                        return;

                    case (uint) NppNotif.NPPN_SHUTDOWN:
                        // uninstall hooks on mouse/keyboard
                        UninstallHooks();

                        if (OnNppShutDown != null)
                            OnNppShutDown();

                        PluginIsReady = false;
                        return;
                }

                #endregion

                // Only do stuff when the dll is fully loaded
                if (!PluginIsReady) return;

                // the user changed the current document
                switch (code) {
                    case (uint) NppNotif.NPPN_FILESAVED:
                    case (uint) NppNotif.NPPN_BUFFERACTIVATED:
                        OnDocumentSwitched();
                        return;
                }

                // only do extra stuff if we are in a progress file
                if (!IsCurrentFileProgress) return;

                #region extra

                switch (code) {
                    case (uint) SciNotif.SCN_CHARADDED:
                        // called each time the user add a char in the current scintilla
                        OnCharTyped((char) nc.ch);
                        return;

                    case (uint) SciNotif.SCN_UPDATEUI:
                        // we need to set the indentation when we received this notification, not before or it's overwritten
                        while (ActionsAfterUpdateUi.Any()) {
                            ActionsAfterUpdateUi.Dequeue()();
                        }

                        if (nc.updated == (int) SciMsg.SC_UPDATE_V_SCROLL ||
                            nc.updated == (int) SciMsg.SC_UPDATE_H_SCROLL) {
                            // user scrolled
                            OnPageScrolled();
                        } else if (nc.updated == (int) SciMsg.SC_UPDATE_SELECTION) {
                            // the user changed its selection
                            OnUpdateSelection();
                        }
                        return;

                    case (uint) SciNotif.SCN_MODIFIED:
                        // observe modification to lines
                        Npp.UpdateLinesInfo(nc);

                        // if the text has changed, parse
                        if ((nc.modificationType & (int) SciMsg.SC_MOD_DELETETEXT) != 0 ||
                            (nc.modificationType & (int) SciMsg.SC_MOD_INSERTTEXT) != 0) {
                            AutoComplete.ParseCurrentDocument();
                        }

                        // did the user supress 1 char?
                        if ((nc.modificationType & (int) SciMsg.SC_MOD_DELETETEXT) != 0 && nc.length == 1) {
                            AutoComplete.UpdateAutocompletion();
                        }
                        return;

                    case (uint) SciNotif.SCN_STYLENEEDED:
                        // if we use the contained lexer, we will receive this notification and we will have to style the text
                        //Style.Colorize(Npp.GetSylingNeededStartPos(), nc.position);
                        return;

                    case (uint) SciNotif.SCN_MARGINCLICK:
                        // called each time the user click on a margin
                        // click on the error margin
                        if (nc.margin == FilesInfo.ErrorMarginNumber) {
                            // if it's an error symbol that has been clicked, the error on the line will be cleared
                            if (!FilesInfo.ClearLineErrors(Npp.LineFromPosition(nc.position))) {
                                // if nothing has been cleared, we go to the next error position
                                FilesInfo.GoToNextError(Npp.LineFromPosition(nc.position));
                            }
                        }
                        return;

                    case (uint) NppNotif.NPPN_FILEBEFOREOPEN:
                        // fire when a file is opened

                        return;

                    case (uint) NppNotif.NPPN_SHORTCUTREMAPPED:
                        // notify plugins that plugin command shortcut is remapped
                        //NppMenu.ShortcutsUpdated((int) nc.nmhdr.idFrom, (ShortcutKey) Marshal.PtrToStructure(nc.nmhdr.hwndFrom, typeof (ShortcutKey)));
                        return;

                    case (uint) SciNotif.SCN_MODIFYATTEMPTRO:
                        // Code a checkout when trying to modify a read-only file

                        return;

                    case (uint) SciNotif.SCN_DWELLSTART:
                        // when the user hover at a fixed position for too long
                        OnDwellStart();
                        return;

                    case (uint) SciNotif.SCN_DWELLEND:
                        // when he moves his cursor
                        OnDwellEnd();
                        return;

                    case (uint) NppNotif.NPPN_FILEBEFORESAVE:
                        // on file saved
                        OnFileSaved();
                        return;
                }

                #endregion

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in beNotified : code = " + nc.nmhdr.code);
            }
        }

        #endregion

        #region Hooks and WndProc override

        /// <summary>
        /// Bascially, this method allows us to hook onto:
        /// - Keyboard (on key down only) messages  -> OnKeyDown
        /// - Mouse messages                        -> OnMouseMessage
        /// It either install the hooks (if they are not installed yet) or just refresh the keyboard keys / mouse messages
        /// to watch, so it can be called several times safely
        /// </summary>
        public static void SetHooks() {

            // Install a WM_KEYDOWN hook
            KeyboardMonitor.Instance.Clear();
            KeyboardMonitor.Instance.Add(
                Keys.Up,
                Keys.Down,
                Keys.Left,
                Keys.Right,
                Keys.Tab,
                Keys.Return,
                Keys.Escape,
                Keys.Back,
                Keys.PageDown,
                Keys.PageUp,
                Keys.Next,
                Keys.Prior);
            // we also add the key that are used as shortcut for 3P functions
            AppliMenu.Instance = null;
            if (AppliMenu.Instance != null) {
                KeyboardMonitor.Instance.Add(AppliMenu.Instance.GetMenuKeysList.ToArray());
            }
            if (!KeyboardMonitor.Instance.IsInstalled) {
                KeyboardMonitor.Instance.KeyDown += OnKeyDown;
                KeyboardMonitor.Instance.Install();
            }

            // Install a mouse hook
            if (!MouseMonitor.Instance.IsInstalled) {
                MouseMonitor.Instance.Clear();
                MouseMonitor.Instance.Add(
                    WinApi.WindowsMessageMouse.WM_NCLBUTTONDOWN,
                    WinApi.WindowsMessageMouse.WM_NCLBUTTONUP,
                    WinApi.WindowsMessageMouse.WM_LBUTTONUP,
                    WinApi.WindowsMessageMouse.WM_MBUTTONDOWN,
                    WinApi.WindowsMessageMouse.WM_RBUTTONUP);
                MouseMonitor.Instance.GetMouseMessage += OnMouseMessage;
                MouseMonitor.Instance.Install();
            }
        }

        private static void UninstallHooks() {
            KeyboardMonitor.Instance.Uninstall();
            MouseMonitor.Instance.Uninstall();
        }

        #endregion

    }
}
