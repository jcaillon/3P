#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using YamuiFramework.Helper;
using _3PA.MainFeatures;
using _3PA.NppCore;

namespace _3PA.WindowsCore {
    /// <summary>
    /// This class calls the appropriate methods depending on the notifications received from both notepad++ and scintilla
    /// </summary>
    internal static class NotificationsPublisher {
        #region Members

        /// <summary>
        /// Set to true after the plugin has been init
        /// </summary>
        public static bool PluginIsReady { get; private set; }

        #endregion

        #region Npp notifications

        /// <summary>
        /// handles the notifications send by npp and scintilla to the plugin
        /// </summary>
        public static void OnNppNotification(SCNotification nc) {
            try {
                uint code = nc.nmhdr.code;

                // Plugin waiting to be started...
                if (!PluginIsReady) {
                    switch (code) {
                        case (uint) NppNotif.NPPN_TBMODIFICATION:
                            // this is the event that we want to respond to, it sets the toolbar icons
                            Plug.DoNppNeedToolbarImages();
                            return;

                        case (uint) NppNotif.NPPN_READY:
                            // notify plugins that all the procedures of launchment of notepad++ are done
                            // call OnNppReady then OnPlugReady if it all went ok
                            PluginIsReady = Plug.DoNppReady();
                            if (PluginIsReady) {
                                Plug.DoPlugStart();

                                // set hooks on mouse/keyboard
                                SetHooks();
                            }
                            return;
                    }
                } else {
                    // the plugin is fully loaded and ready to do stuff

                    if ((uint) SciNotif.SCN_NOTIF_BEGIN < code && code < (uint) SciNotif.SCN_NOTIF_END) {
                        switch (code) {
                            // --------------------------------------------------------
                            // Scintilla message
                            // --------------------------------------------------------
                            case (uint) SciNotif.SCN_CHARADDED:
                                // called each time the user add a char in the current scintilla
                                Plug.OnSciCharTyped((char) nc.ch);
                                return;

                            case (uint) SciNotif.SCN_UPDATEUI:
                                Plug.OnSciUpdateUi(nc);
                                return;

                            case (uint) SciNotif.SCN_MODIFIED:
                                Plug.OnSciModified(nc);
                                return;

                            case (uint) SciNotif.SCN_STYLENEEDED:
                                // if we use the contained lexer, we will receive this notification and we will have to style the text
                                //Style.Colorize(Npp.GetSylingNeededStartPos(), nc.position);
                                return;

                            case (uint) SciNotif.SCN_MARGINCLICK:
                                // called each time the user click on a margin
                                Plug.OnSciMarginClick(nc);
                                return;

                            case (uint) SciNotif.SCN_MODIFYATTEMPTRO:
                                // Code a checkout when trying to modify a read-only file
                                return;

                            case (uint) SciNotif.SCN_DWELLSTART:
                                // when the user hover at a fixed position for too long
                                Plug.OnSciDwellStart();
                                return;

                            case (uint) SciNotif.SCN_DWELLEND:
                                // when he moves his cursor
                                Plug.OnSciDwellEnd();
                                return;
                        }
                    } else if ((uint) NppNotif.NPPN_NOTIF_BEGIN < code && code < (uint) NppNotif.NPPN_NOTIF_END) {
                        // --------------------------------------------------------
                        // Npp message
                        // --------------------------------------------------------
                        switch (code) {
                            case (uint) NppNotif.NPPN_SHUTDOWN:
                                // uninstall hooks on mouse/keyboard
                                UninstallHooks();
                                PluginIsReady = false;
                                Plug.DoNppShutDown();
                                return;

                            // the user changed the current document
                            case (uint) NppNotif.NPPN_BUFFERACTIVATED:
                                Plug.DoNppBufferActivated();
                                return;

                            case (uint) NppNotif.NPPN_FILERENAMED:
                                // the user can open a .txt and rename it as a .p
                                Plug.DoNppBufferActivated();
                                return;

                            case (uint) NppNotif.NPPN_FILESAVED:
                                // the user can open a .txt and save it as a .p
                                Plug.DoNppBufferActivated();

                                Plug.DoNppDocumentSaved();
                                return;

                            case (uint) NppNotif.NPPN_FILEBEFORELOAD:
                                // fire when a file is opened (the event NPPN_FILEBEFOREOPEN is fired after SciNotif.SCN_MODIFIED
                                // and just before NppNotif.NPPN_BUFFERACTIVATED so it's not very useful...)
                                Plug.DoNppFileBeforeLoad();
                                return;

                            case (uint) NppNotif.NPPN_FILEOPENED:
                                // on file opened
                                Plug.OnNppFileOpened();
                                return;

                            case (uint) NppNotif.NPPN_FILEBEFORECLOSE:
                                // on file closed
                                Plug.OnNppFileBeforeClose();
                                return;

                            case (uint) NppNotif.NPPN_FILEBEFORESAVE:
                                // on file saved
                                Plug.OnNppFileBeforeSaved();
                                return;

                            case (uint) NppNotif.NPPN_LANGCHANGED:
                                // on lang type changed
                                UserCommunication.Notify("lang changed " + Npp.CurrentInternalLangName);
                                return;

                            case (uint) NppNotif.NPPN_WORDSTYLESUPDATED:
                                // The styles have been modified
                                NppConfig.Reload();
                                // unfortunatly, if the user changed of styler.xml file (he selected another theme) then we 
                                // will incorrectly read the styles since we have to wait for the config.xml to be updated
                                // and it only updates on npp shutdown
                                return;
                        }
                    }
                }
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
                Keys.Prior,
                Keys.Home,
                Keys.End
                );

            // we also add the key that are used as shortcut for 3P functions
            AppliMenu.Instance = null; // make sure to recompute the menu
            if (AppliMenu.Instance != null) {
                KeyboardMonitor.Instance.Add(AppliMenu.Instance.GetMenuKeysList.ToArray());
            }
            if (!KeyboardMonitor.Instance.IsInstalled) {
                KeyboardMonitor.Instance.KeyDown += Plug.KeyDownHandler;
                KeyboardMonitor.Instance.Install();
            }

            // Install a mouse hook
            if (!MouseMonitor.Instance.IsInstalled) {
                MouseMonitor.Instance.Clear();
                MouseMonitor.Instance.Add(
                    WinApi.Messages.WM_NCLBUTTONDOWN,
                    WinApi.Messages.WM_NCLBUTTONUP,
                    WinApi.Messages.WM_LBUTTONUP,
                    WinApi.Messages.WM_MBUTTONDOWN,
                    WinApi.Messages.WM_RBUTTONUP);
                MouseMonitor.Instance.GetMouseMessage += Plug.MouseMessageHandler;
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