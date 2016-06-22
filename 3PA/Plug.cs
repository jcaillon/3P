#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (Plug.cs) is part of 3P.
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
using System.Threading.Tasks;
using System.Windows.Forms;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.FileExplorer;
using _3PA.MainFeatures.InfoToolTip;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA {

    /// <summary>
    /// The entry points for this plugin are the following :<br></br>
    /// - OnCommandMenuInit (through UnmanagedExports)<br></br>
    /// - OnNppNotification (through UnmanagedExports)<br></br>
    /// - OnWndProcMessage<br></br>
    /// - OnMouseMessage<br></br>
    /// - OnKeyDown<br></br>
    /// </summary>
    internal static partial class Plug {

        #region Fields


        // We don't want to recompute those values all the time so we store them when the buffer (document) changes

        /// <summary>
        /// true if the current file is a progress file, false otherwise
        /// </summary>
        public static bool IsCurrentFileProgress { get; private set; }

        /// <summary>
        /// Stores the current file path when switching document
        /// </summary>
        public static string CurrentFilePath { get; private set; }

        /// <summary>
        /// Information on the current file
        /// </summary>
        public static FileInfoObject CurrentFileObject { get; private set; }

        #endregion

        #region Start

        /// <summary>
        /// Called on notepad++ setinfo
        /// </summary>
        internal static void OnCommandMenuInit() {
            var cmdIndex = 0;
            AppliMenu.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex++, "Show main menu  [Ctrl + Right click]", AppliMenu.ShowMainMenuAtCursor);
            CodeExplorer.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex++, "Toggle code explorer", CodeExplorer.Toggle);
            FileExplorer.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex, "Toggle file explorer", FileExplorer.Toggle);

            // subscribe to notorious events
            OnNppShutDown += PlugShutDown;
            OnNppReady += PlugStartup;

            // This allows to correctly feed the dll with its dependencies
            AppDomain.CurrentDomain.AssemblyResolve += LibLoader.AssemblyResolver;

            // catch unhandled errors to log them
            AppDomain.CurrentDomain.UnhandledException += ErrorHandler.UnhandledErrorHandler;
            Application.ThreadException += ErrorHandler.ThreadErrorHandler;
            TaskScheduler.UnobservedTaskException += ErrorHandler.UnobservedErrorHandler;
        }

        /// <summary>
        /// display images in the npp toolbar
        /// </summary>
        internal static void InitToolbarImages() {
            Npp.SetToolbarImage(ImageResources.logo16x16, AppliMenu.DockableCommandIndex);
            Npp.SetToolbarImage(ImageResources.FileExplorer16x16, FileExplorer.DockableCommandIndex);
            Npp.SetToolbarImage(ImageResources.CodeExplorer16x16, CodeExplorer.DockableCommandIndex);
        }

        /// <summary>
        /// Called on npp ready
        /// </summary>
        internal static bool PlugStartup() {
            try {
                ThemeManager.OnStartUp();

                // init an empty form, this gives us a Form to hook onto if we want to do stuff on the UI thread
                // from a back groundthread, use : BeginInvoke()
                UserCommunication.Init();

                // if the UDL is not installed
                if (!Style.InstallUdl(true)) {
                    Style.InstallUdl();
                } else {
                    // first use message?
                    if (Config.Instance.UserFirstUse) {
                        UserCommunication.NotifyUnique("welcome", "<div>Dear user,<br><br>Thank you for installing 3P, you are awesome!<br><br>If this is your first look at 3P I invite you to read the <b>Getting started</b> section of the home page by clicking <a href='go'>on this link right here</a>.<br><br></div><div align='right'>Enjoy!</div>", MessageImg.MsgInfo, "Information", "Hello and welcome aboard!", args => {
                            Appli.ToggleView();
                            UserCommunication.CloseUniqueNotif("welcome");
                            args.Handled = true;
                        });
                        Config.Instance.UserFirstUse = false;
                    }
                }

                // check Npp version, 3P requires version 6.8 or higher
                if (!string.IsNullOrEmpty(Npp.GetNppVersion) && !Npp.GetNppVersion.IsHigherVersionThan("6.7")) {
                    UserCommunication.Notify("Dear user,<br><br>Your version of notepad++ (" + Npp.GetNppVersion + ") is outdated.<br>3P <b>requires</b> the version <b>6.8</b> or above, <b>there are known issues with inferior versions</b>. Please upgrade to an up-to-date version of Notepad++ or use 3P at your own risks.<br><br><a href='https://notepad-plus-plus.org/download/'>Download the lastest version of Notepad++ here</a>", MessageImg.MsgError, "Outdated version", "3P requirements are not met");
                }

                // Check if an update has been done and start checking for new updates
                UpdateHandler.CheckForUpdateDone();
                UpdateHandler.StartCheckingForUpdate();

                // code explorer
                if (Config.Instance.CodeExplorerAutoHideOnNonProgressFile) {
                    CodeExplorer.Toggle(Abl.IsCurrentProgressFile());
                } else if (Config.Instance.CodeExplorerVisible) {
                    CodeExplorer.Toggle();
                }

                // File explorer
                if (Config.Instance.FileExplorerAutoHideOnNonProgressFile) {
                    FileExplorer.Toggle(Abl.IsCurrentProgressFile());
                } else if (Config.Instance.FileExplorerVisible) {
                    FileExplorer.Toggle();
                }

                // Try to update the configuration from the distant shared folder
                ShareExportConf.StartCheckingForUpdates();

                // everything else can be async
                //Task.Factory.StartNew(() => {

                Keywords.Import();
                Snippets.Init();
                FileTag.Import();

                // initialize the list of objects of the autocompletion form
                AutoComplete.RefreshStaticItems(true);

                // init database info
                DataBase.Init();

                SetHooks();
                //});

                // Start pinging
                // ReSharper disable once ObjectCreationAsStatement
                new ReccurentAction(User.Ping, 1000*60*120);

                // Make sure to give the focus to scintilla on startup
                WinApi.SetForegroundWindow(Npp.HandleNpp);

                // set the following operations
                OnPlugReady += AfterPlugStartUp;

                return true;

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Plugin startup");
            }
            return false;
        }

        internal static void AfterPlugStartUp() {

            // Simulates a OnDocumentSwitched when we start this dll
            OnDocumentSwitched(true);
        }

        #endregion

        #region Die

        /// <summary>
        /// Called on Npp shutdown
        /// </summary>
        internal static void PlugShutDown() {
            try {
                // export modified conf
                FileTag.Export();

                // uninstall hooks
                UninstallHooks();

                // set options back to client's default
                ApplyPluginSpecificOptions(true);

                // save config (should be done but just in case)
                CodeExplorer.UpdateMenuItemChecked();
                FileExplorer.UpdateMenuItemChecked();
                Config.Save();

                // remember the most used keywords
                Keywords.SaveRanking();

                // close every form
                AutoComplete.ForceClose();
                InfoToolTip.ForceClose();
                Appli.ForceClose();
                FileExplorer.ForceClose();
                CodeExplorer.ForceClose();
                UserCommunication.ForceClose();
                AppliMenu.ForceCloseMenu();

                PluginIsFullyLoaded = false;

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "CleanUp");
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
            MouseMonitor.Instance.Clear();
            MouseMonitor.Instance.Add(
                WinApi.WindowsMessageMouse.WM_NCLBUTTONDOWN,
                WinApi.WindowsMessageMouse.WM_NCLBUTTONUP,
                WinApi.WindowsMessageMouse.WM_LBUTTONUP,
                WinApi.WindowsMessageMouse.WM_MBUTTONDOWN, 
                WinApi.WindowsMessageMouse.WM_RBUTTONUP);

            if (!MouseMonitor.Instance.IsInstalled) {
                MouseMonitor.Instance.GetMouseMessage += OnMouseMessage;
                MouseMonitor.Instance.Install();
            }
        }

        private static void UninstallHooks() {
            KeyboardMonitor.Instance.Uninstall();
            MouseMonitor.Instance.Uninstall();
        }

        #endregion

        #region Apply Npp options

        private static bool _indentWithTabs;
        private static int _indentWidth = -1;
        private static Annotation _annotationMode;
        private static WhitespaceMode _whitespaceMode = WhitespaceMode.Invisible;

        /// <summary>
        /// We need certain options to be set to specific values when running this plugin, make sure to set everything back to normal
        /// when switch tab or when we leave npp, param can be set to true to force the default values
        /// </summary>
        /// <param name="forceToDefault"></param>
        public static void ApplyPluginSpecificOptions(bool forceToDefault) {

            if (_indentWidth == -1) {
                _indentWidth = Npp.IndentWidth;
                _indentWithTabs = Npp.UseTabs;
                _annotationMode = Npp.AnnotationVisible;
                _whitespaceMode = Npp.ViewWhitespace;

                // Extra settings at the start
                Npp.MouseDwellTime = Config.Instance.ToolTipmsBeforeShowing;
                Npp.EndAtLastLine = false;
                Npp.EventMask = (int) (SciMsg.SC_MOD_INSERTTEXT | SciMsg.SC_MOD_DELETETEXT | SciMsg.SC_PERFORMED_USER | SciMsg.SC_PERFORMED_UNDO | SciMsg.SC_PERFORMED_REDO);
            }

            if (!IsCurrentFileProgress || forceToDefault) {
                Npp.AutoCStops("");
                Npp.AnnotationVisible = _annotationMode;
                Npp.UseTabs = _indentWithTabs;
                Npp.TabWidth = _indentWidth;
                Npp.ViewWhitespace = _whitespaceMode;
            } else {
                // barbarian method to force the default autocompletion window to hide, it makes npp slows down when there is too much text...
                // TODO: find a better technique to hide the autocompletion!!! this slows npp down
                Npp.AutoCStops(@"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_");
                Npp.AnnotationVisible = Annotation.Indented;
                Npp.UseTabs = false;
                Npp.TabWidth = Config.Instance.CodeTabSpaceNb;
                if (Config.Instance.CodeShowSpaces) {
                    Npp.ViewWhitespace = WhitespaceMode.VisibleAlways;
                }
            }
        }

        #endregion

        #region utils

        /// <summary>
        /// Call this method to close all popup/autocompletion form and alike
        /// </summary>
        public static void ClosePopups() {
            AutoComplete.Close();
            InfoToolTip.Close();
        }

        #endregion

    }
}