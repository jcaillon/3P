#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Helper;
using _3PA.MainFeatures;
using _3PA.WindowsCore;

namespace _3PA.NppCore {
    /// <summary>
    /// This class calls the appropriate methods depending on the notifications received from both notepad++ and scintilla
    /// </summary>
    internal static class NotificationsPublisher {

        #region Members

        /// <summary>
        /// Set to true after the plugin has been init
        /// </summary>
        public static bool PluginIsReady { get; private set; }

        /// <summary>
        /// If true, the notification SCN_MODIFIED is disabled, we use this to temporary disable the 
        /// handling of modified notification when npp is loading a file, since we will reset the DocumentLines
        /// after the loading anyway
        /// </summary>
        private static bool ScnModifiedDisabled { get; set; }

        /// <summary>
        /// this is a delegate to defined actions that must be taken after updating the ui
        /// </summary>
        public static Queue<Action> ActionsAfterUpdateUi { get; set; }

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
                            UnmanagedExports.NppFuncItems.RefreshItems();
                            Plug.DoNppNeedToolbarImages();
                            return;

                        case (uint) NppNotif.NPPN_READY:
                            // notify plugins that all the procedures of launch of notepad++ are done
                            ActionsAfterUpdateUi = new Queue<Action>();
                            Npp.UpdateCurrentSci(); // init current scintilla
                            UiThread.Init();
                            PluginIsReady = Plug.DoNppReady();
                            // call OnNppReady then OnPlugReady if it all went ok
                            if (PluginIsReady) {
                                Plug.DoPlugStart();
                                OnNppNotification(new SCNotification((uint) NppNotif.NPPN_BUFFERACTIVATED)); // simulate buffer activated

                                // set hooks on mouse/keyboard
                                SetHooks();
                            }
                            return;

                        case (uint) NppNotif.NPPN_SHUTDOWN:
                            // uninstall hooks on mouse/keyboard
                            UninstallHooks();
                            UiThread.Close();
                            Plug.DoNppShutDown();
                            return;

                        case (uint) NppNotif.NPPN_CANCELSHUTDOWN:
                            PluginIsReady = true;
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
                                // It's actually better to use the SCI_MODIFIED instead, this notification
                                // is not always called when it should! (ex not called for /t)
                                return;

                            case (uint) SciNotif.SCN_UPDATEUI:
                                while (ActionsAfterUpdateUi.Any()) {
                                    ActionsAfterUpdateUi.Dequeue()();
                                }
                                Plug.OnSciUpdateUi(nc);
                                return;

                            case (uint) SciNotif.SCN_MODIFIED:
                                // This notification is sent when the text or styling of the document changes or is about to change
                                // (note : this notif isn't sent when the user SWITCHES to tab file (already opened in another tab) !
                                // But it is sent when the user opens a NEW file)
                                bool deletedText = (nc.modificationType & (int) SciModificationMod.SC_MOD_DELETETEXT) != 0;
                                bool insertedText = (nc.modificationType & (int) SciModificationMod.SC_MOD_INSERTTEXT) != 0;
                                bool undo = (nc.modificationType & (int) SciModificationMod.SC_PERFORMED_UNDO) != 0;
                                bool redo = (nc.modificationType & (int) SciModificationMod.SC_PERFORMED_REDO) != 0;
                                bool singleCharModification = false;

                                if ((insertedText || deletedText) && !ScnModifiedDisabled) {
                                    var encoding = Sci.Encoding;
                                    Npp.CurrentSci.Lines.OnScnModified(nc, !deletedText, encoding); // register line modifications
                                    if (!undo && !redo) {
                                        // if the text has changed
                                        unsafe {
                                            // only 1 char appears to be modified
                                            if (nc.length <= 2) {
                                                // get the char
                                                var bytes = (byte*) nc.text;
                                                var arrbyte = new byte[nc.length];
                                                int index;
                                                for (index = 0; index < nc.length; index++)
                                                    arrbyte[index] = bytes[index];
                                                var c = encoding.GetChars(arrbyte);
                                                var cLength = c.Length;
                                                // do we really have a 1 char input?
                                                if (cLength == 1 || (cLength == 2 && c[0] == '\r')) {
                                                    if (insertedText) {
                                                        ActionsAfterUpdateUi.Enqueue(() => Plug.OnCharAdded(c[0], nc.position));
                                                    } else {
                                                        ActionsAfterUpdateUi.Enqueue(() => Plug.OnCharDeleted(c[0], nc.position));
                                                    }
                                                    singleCharModification = true;
                                                }
                                            }
                                        }
                                    }
                                    ActionsAfterUpdateUi.Enqueue(() => Plug.OnTextModified(nc, insertedText, deletedText, singleCharModification, undo, redo));
                                }

                                return;

                            case (uint) SciNotif.SCN_STYLENEEDED:
                                // if we use the contained lexer, we will receive this notification and we will have to style the text
                                Plug.OnStyleNeeded(Sci.GetEndStyled(), nc.position);
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
                            case (uint) NppNotif.NPPN_BUFFERACTIVATED:
                                // the user changes the current document (this event is called when the current document is switched (via the tabs)
                                // and also when a new file is opened in npp
                                Npp.UpdateCurrentSci(); // update current scintilla
                                Npp.CurrentSci.Lines.Reset(); // register new lines
                                NppBufferActivated();
                                return;

                            case (uint) NppNotif.NPPN_FILERENAMED:
                                // the user can open a .txt and rename it as a .p
                                NppBufferActivated();
                                return;

                            case (uint) NppNotif.NPPN_FILESAVED:
                                // the user can open a .txt and save it as a .p
                                NppBufferActivated();

                                Plug.DoNppDocumentSaved();
                                return;

                            case (uint) NppNotif.NPPN_FILEBEFORELOAD:
                                // fire when a file is opened
                                // When loading a new file into NPP, the events fired are (in order) :
                                // NPPN_FILEBEFORELOAD > SCN_MODIFIED > NPPN_FILEBEFOREOPEN > NPPN_FILEOPENED > NPPN_BUFFERACTIVATED
                                // we deactivate the SCN_MODIFIED between NPPN_FILEBEFORELOAD and NPPN_FILEBEFOREOPEN
                                ScnModifiedDisabled = true;
                                Plug.DoNppFileBeforeLoad();
                                return;

                            case (uint) NppNotif.NPPN_FILEBEFOREOPEN:
                                ScnModifiedDisabled = false;
                                return;

                            case (uint) NppNotif.NPPN_FILEOPENED:
                                // on file opened
                                Plug.OnNppFileOpened();
                                return;

                            case (uint) NppNotif.NPPN_FILEBEFORECLOSE:
                                // on file closed
                                Plug.OnNppFileBeforeClose();
                                return;

                            case (uint) NppNotif.NPPN_LANGCHANGED:
                                // on lang type changed
                                Plug.OnLangChanged();
                                NppBufferActivated();
                                return;

                            case (uint) NppNotif.NPPN_WORDSTYLESUPDATED:
                                // The styles have been modified
                                Npp.StylersXml.Reload();
                                // unfortunatly, if the user changed of styler.xml file (he selected another theme) then we 
                                // will incorrectly read the styles since we have to wait for the config.xml to be updated
                                // and it only updates on npp shutdown
                                return;

                            case (uint) NppNotif.NPPN_BEFORESHUTDOWN:
                                // prevent the plugin from handling a lot of events when npp is about to shutdown
                                PluginIsReady = false;
                                return;
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in beNotified : code = " + nc.nmhdr.code);
            }
        }

        private static void NppBufferActivated() {
            Npp.CurrentFileInfo.Path = Npp.NppFileInfo.GetFullPathApi; // get info on the current file
            Plug.DoNppBufferActivated();
            Npp.PreviousFileInfo.Path = Npp.CurrentFileInfo.Path; // save info on the "previous" file for the next buffer activated event
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

            // Install messaging hook
            if (!CallWndProcMonitor.Instance.IsInstalled)
            {
                CallWndProcMonitor.Instance.Clear();
                CallWndProcMonitor.Instance.Add(
                    WinApi.Messages.WM_PAINT
                    );
                CallWndProcMonitor.Instance.GetMessage += Plug.HwndMessageHandler;
                CallWndProcMonitor.Instance.Install();
            }
        }

        private static void UninstallHooks() {
            KeyboardMonitor.Instance.Uninstall();
            MouseMonitor.Instance.Uninstall();
            CallWndProcMonitor.Instance.Uninstall();
        }

        #endregion
    }
}