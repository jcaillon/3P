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
using YamuiFramework.Themes;
using _3PA.Html;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.FileExplorer;
using _3PA.MainFeatures.FilesInfoNs;
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

        /// <summary>
        /// Set to true after the plugin has been fully loaded
        /// </summary>
        public static bool PluginIsFullyLoaded { get; private set; }

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

        #region Init

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
        internal static void OnNppReady() {
            try {
                // This allows to correctly feed the dll with its dependencies
                LibLoader.Init();

                // catch unhandled errors to log them
                AppDomain.CurrentDomain.UnhandledException += ErrorHandler.UnhandledErrorHandler;
                Application.ThreadException += ErrorHandler.ThreadErrorHandler;
                TaskScheduler.UnobservedTaskException += ErrorHandler.UnobservedErrorHandler;

                // initialize plugin (why another method for this? because otherwise the LibLoader can't do his job...)
                InitPlugin();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "OnNppReady");
            }
        }

        internal static void InitPlugin() {

            // themes and html
            ThemeManager.Current.AccentColor = Config.Instance.AccentColor;
            YamuiThemeManager.TabAnimationAllowed = Config.Instance.AppliAllowTabAnimation;
            YamuiThemeManager.OnGetCssSheet += HtmlHandler.YamuiThemeManagerOnOnGetCssSheet;
            YamuiThemeManager.OnHtmlImageNeeded += HtmlHandler.YamuiThemeManagerOnOnHtmlImageNeeded;

            // init an empty form, this gives us a Form to hook onto if we want to do stuff on the UI thread
            // from a back groundthread, use : BeginInvoke()
            UserCommunication.Init();

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

            // Try to update 3P
            UpdateHandler.OnNotepadStart();

            // Try to update the configuration from the distant shared folder
            ShareExportConf.OnNotepadStart();

            // everything else can be async
            //Task.Factory.StartNew(() => {

            Keywords.Import();
            Snippets.Init();
            FileTag.Import();
            ProCompilePath.Import();

            // initialize the list of objects of the autocompletion form
            AutoComplete.RefreshStaticItems(true);

            // init database info
            DataBase.Init();

            PluginIsFullyLoaded = true;

            // Simulates a OnDocumentSwitched when we start this dll
            OnDocumentSwitched(true);

            SetHooks();
            //});

            // this is done async anyway
            FileExplorer.RebuildItemList();
        }

        #endregion

        #region Clean up

        /// <summary>
        /// Called on Npp shutdown
        /// </summary>
        internal static void OnNppShutdown() {
            try {
                // export modified conf
                FileTag.Export();

                // uninstall hooks
                UninstallHooks();

                // disable hour timer
                UpdateHandler.DeleteHourTimer();

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
                AppliMenu.ForceClose();

                PluginIsFullyLoaded = false;

                // runs exit program if any
                UpdateHandler.OnNotepadExit();

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
                KeyboardMonitor.Instance.Add(AppliMenu.GetMenuKeysList(AppliMenu.Instance.MainMenuList).ToArray());
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

        #region tests

        public static void StartDebug() {
            //Debug.Assert(false);
        }



        public static void Test() {
            //UserCommunication.Message(("# What's new in this version? #\n\n" + File.ReadAllText(@"d:\Profiles\jcaillon\Desktop\derp.md", Encoding.Default)).MdToHtml(),
            //        MessageImg.MsgUpdate,
            //        "A new version has been installed!",
            //        "Updated to version " + AssemblyInfo.Version,
            //        new List<string> { "ok", "cancel" },
            //        true);
        }

        #endregion
    }

}