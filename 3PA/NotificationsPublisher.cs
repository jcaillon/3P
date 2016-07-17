#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (NotificationsPublisher.cs) is part of 3P.
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
using System.Windows.Forms;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;

namespace _3PA {

    internal static partial class Plug {

        #region Members

        /// <summary>
        /// Set to true after the plugin has been init
        /// </summary>
        public static bool PluginIsReady { get; private set; }

        /// <summary>
        /// Set to true after the plugin has been fully loaded
        /// </summary>
        public static bool PluginIsFullyLoaded { get; private set; }

        /// <summary>
        /// Set to false to only get notifications of file switched/saved/loaded, allowing to 
        /// filter notifications on certain files that doesn't concern the plugin
        /// </summary>
        public static bool CurrentFileAllowed { get; set; }

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
                        DoNppNeedToolbarImages();
                        return;

                    case (uint) NppNotif.NPPN_READY:
                        // notify plugins that all the procedures of launchment of notepad++ are done
                        // call OnNppReady then OnPlugReady if it all went ok
                        PluginIsReady = DoNppReady();
                        if (PluginIsReady) {

                            DoPlugStart();

                            // set hooks on mouse/keyboard
                            SetHooks();

                            PluginIsFullyLoaded = true;
                        }
                        return;

                    case (uint) NppNotif.NPPN_SHUTDOWN:
                        // uninstall hooks on mouse/keyboard
                        UninstallHooks();

                        DoNppShutDown();

                        PluginIsReady = false;
                        return;
                }

                #endregion

                // Only do stuff when the dll is fully loaded
                if (!PluginIsReady) 
                    return;

                switch (code) {
                    // the user changed the current document
                    case (uint) NppNotif.NPPN_BUFFERACTIVATED:
                        DoNppDocumentSwitched();
                        return;

                    case (uint)NppNotif.NPPN_FILESAVED:
                        DoNppDocumentSaved();
                        return;

                    case (uint)NppNotif.NPPN_FILEBEFORELOAD:
                        // fire when a file is opened (the event NPPN_FILEBEFOREOPEN is fired after SciNotif.SCN_MODIFIED
                        // and just before NppNotif.NPPN_BUFFERACTIVATED so it's not very useful...)
                        DoNppFileBeforeLoad();
                        return;
                }

                // only do extra stuff if we are in a progress file
                if (!CurrentFileAllowed) 
                    return;

                #region extra

                switch (code) {

                    case (uint)NppNotif.NPPN_FILEBEFORESAVE:
                        // on file saved
                        OnNppFileBeforeSaved();
                        return;

                    case (uint)NppNotif.NPPN_SHORTCUTREMAPPED:
                        // notify plugins that plugin command shortcut is remapped
                        //NppMenu.ShortcutsUpdated((int) nc.nmhdr.idFrom, (ShortcutKey) Marshal.PtrToStructure(nc.nmhdr.hwndFrom, typeof (ShortcutKey)));
                        return;

                    // --------------------------------------------------------
                    // Scintilla message
                    // --------------------------------------------------------

                    case (uint) SciNotif.SCN_CHARADDED:
                        // called each time the user add a char in the current scintilla
                        OnSciCharTyped((char) nc.ch);
                        return;

                    case (uint) SciNotif.SCN_UPDATEUI:
                        OnSciUpdateUi(nc);
                        return;

                    case (uint) SciNotif.SCN_MODIFIED:
                        OnSciModified(nc);
                        return;

                    case (uint) SciNotif.SCN_STYLENEEDED:
                        // if we use the contained lexer, we will receive this notification and we will have to style the text
                        //Style.Colorize(Npp.GetSylingNeededStartPos(), nc.position);
                        return;

                    case (uint) SciNotif.SCN_MARGINCLICK:
                        // called each time the user click on a margin
                        OnSciMarginClick(nc);
                        return;


                    case (uint) SciNotif.SCN_MODIFYATTEMPTRO:
                        // Code a checkout when trying to modify a read-only file

                        return;

                    case (uint) SciNotif.SCN_DWELLSTART:
                        // when the user hover at a fixed position for too long
                        OnSciDwellStart();
                        return;

                    case (uint) SciNotif.SCN_DWELLEND:
                        // when he moves his cursor
                        OnSciDwellEnd();
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
            AppliMenu.Instance = null; // make sure to recompute the menu
            if (AppliMenu.Instance != null) {
                KeyboardMonitor.Instance.Add(AppliMenu.Instance.GetMenuKeysList.ToArray());
            }
            if (!KeyboardMonitor.Instance.IsInstalled) {
                KeyboardMonitor.Instance.KeyDown += KeyDownHandler;
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
                MouseMonitor.Instance.GetMouseMessage += MouseMessageHandler;
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
