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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using YamuiFramework.Helper;
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
    /// - Main (through UnmanagedExports)<br></br>
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
        /// true if the previous file was a progress file, false otherwise
        /// </summary>
        public static bool IsPreviousFileProgress { get; private set; }

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
        /// Called by notepad++ when the plugin is loaded
        /// </summary>
        internal static void Main() {
            // subscribe to notorious events
            OnSetFuncItems += SetPlugFuncItems;
            OnNppShutDown += PlugShutDown;
            OnNppReady += PlugInit;
            OnPlugReady += PlugStartUp;

            // This allows to correctly feed the dll with its dependencies
            AppDomain.CurrentDomain.AssemblyResolve += LibLoader.AssemblyResolver;

            // catch unhandled errors to log them
            AppDomain.CurrentDomain.UnhandledException += ErrorHandler.UnhandledErrorHandler;
            Application.ThreadException += ErrorHandler.ThreadErrorHandler;
            TaskScheduler.UnobservedTaskException += ErrorHandler.UnobservedErrorHandler;
        }

        /// <summary>
        /// Called when the plugin menu of the plugin needs to be filled
        /// </summary>
        internal static void SetPlugFuncItems() {
            var cmdIndex = 0;
            AppliMenu.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex++, "Show main menu  [Ctrl + Right click]", AppliMenu.ShowMainMenuAtCursor);
            CodeExplorer.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex++, "Toggle code explorer", CodeExplorer.Toggle);
            FileExplorer.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex, "Toggle file explorer", FileExplorer.Toggle);
        }

        /// <summary>
        /// Called when the plugin can set new shorcuts to the toolbar in notepad++
        /// </summary>
        internal static void InitToolbarImages() {
            Npp.SetToolbarImage(ImageResources.logo16x16, AppliMenu.DockableCommandIndex);
            Npp.SetToolbarImage(ImageResources.FileExplorer16x16, FileExplorer.DockableCommandIndex);
            Npp.SetToolbarImage(ImageResources.CodeExplorer16x16, CodeExplorer.DockableCommandIndex);
        }

        /// <summary>
        /// Called on npp ready
        /// </summary>
        internal static bool PlugInit() {
            try {
                // need to set some values in the yamuiThemeManager
                ThemeManager.OnStartUp();

                // init an empty form, this gives us a Form to hook onto if we want to do stuff on the UI thread
                // from a back groundthread with BeginInvoke()
                // once this method is done, we are able to publish notifications
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
                if (!String.IsNullOrEmpty(Npp.GetNppVersion) && !Npp.GetNppVersion.IsHigherVersionThan("6.7")) {
                    UserCommunication.Notify("Dear user,<br><br>Your version of notepad++ (" + Npp.GetNppVersion + ") is outdated.<br>3P <b>requires</b> the version <b>6.8</b> or above, <b>there are known issues with inferior versions</b>. Please upgrade to an up-to-date version of Notepad++ or use 3P at your own risks.<br><br><a href='https://notepad-plus-plus.org/download/'>Download the lastest version of Notepad++ here</a>", MessageImg.MsgError, "Outdated version", "3P requirements are not met");
                }

                // Check if an update has been done and start checking for new updates
                UpdateHandler.CheckForUpdateDone();
                UpdateHandler.StartCheckingForUpdate(); // async

                // Try to update the configuration from the distant shared folder
                ShareExportConf.StartCheckingForUpdates();

                // Start pinging
                // ReSharper disable once ObjectCreationAsStatement
                new ReccurentAction(User.Ping, 1000*60*120);

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

                return true;

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Plugin startup");
            }
            return false;
        }

        internal static void PlugStartUp() {

            // subscribe to static events
            ProEnvironment.OnEnvironmentChange += FileExplorer.RebuildFileList;
            ProEnvironment.OnEnvironmentChange += DataBase.UpdateDatabaseInfo;

            OnKeyDown += KeyDownHandler;
            OnMouseMessage += MouseMessageHandler;


            Keywords.Import();
            Snippets.Init();
            FileTag.Import();

            // initialize the list of objects of the autocompletion form
            AutoComplete.RefreshStaticItems();

            // Simulates a OnDocumentSwitched when we start this dll
            IsCurrentFileProgress = Abl.IsCurrentProgressFile(); // to correctly init isPreviousProgress
            OnDocumentSwitched(true);

            // Make sure to give the focus to scintilla on startup
            WinApi.SetForegroundWindow(Npp.HandleNpp);
        }

        #endregion

        #region Die

        /// <summary>
        /// Called on Npp shutdown
        /// </summary>
        internal static void PlugShutDown() {
            try {
                // clean up timers
                ReccurentAction.CleanAll();
                DelayedAction.CleanAll();

                // export modified conf
                FileTag.Export();

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

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Stop");
            }
        }

        #endregion

        #region Apply Scintilla options for plugin needs

        private static bool _indentWithTabs;
        private static int _tabWidth = -1;
        private static Annotation _annotationMode;
        private static WhitespaceMode _whitespaceMode = WhitespaceMode.Invisible;

        private static bool _hasBeenInit;
        private static bool _autoStopSet;
        private static bool _warnedAboutFailStylers;

        /// <summary>
        /// We need certain options to be set to specific values when running this plugin, make sure to set everything back to normal
        /// when switch tab or when we leave npp, param can be set to true to force the default values
        /// </summary>
        internal static void ApplyOptionsForScintilla() {
            if (IsCurrentFileProgress)
                ApplyPluginOptionsForScintilla();
            else
                ApplyDefaultOptionsForScintilla();
        }

        internal static void ApplyPluginOptionsForScintilla() {
            if (!_hasBeenInit || !IsPreviousFileProgress) {
                // read default options
                _tabWidth = Npp.TabWidth;
                _indentWithTabs = Npp.UseTabs;
                _annotationMode = Npp.AnnotationVisible;
                _whitespaceMode = Npp.ViewWhitespace;
            }
            
            if (!_hasBeenInit) {
                // Extra settings at the start
                Npp.MouseDwellTime = Config.Instance.ToolTipmsBeforeShowing;
                Npp.EndAtLastLine = false;
                Npp.EventMask = (int) (SciMsg.SC_MOD_INSERTTEXT | SciMsg.SC_MOD_DELETETEXT | SciMsg.SC_PERFORMED_USER | SciMsg.SC_PERFORMED_UNDO | SciMsg.SC_PERFORMED_REDO);
                _hasBeenInit = true;
            }

            Npp.TabWidth = Config.Instance.CodeTabSpaceNb;
            Npp.UseTabs = false;
            Npp.AnnotationVisible = Annotation.Indented;
            if (Config.Instance.CodeShowSpaces)
                Npp.ViewWhitespace = WhitespaceMode.VisibleAlways;

            // apply style
            var currentStyle = Style.Current;
            Npp.SetWhiteSpaceColor(true, currentStyle.WhiteSpace.BackColor, currentStyle.WhiteSpace.ForeColor);
            Npp.SetIndentGuideColor(currentStyle.WhiteSpace.BackColor, currentStyle.WhiteSpace.ForeColor);
            Npp.SetSelectionColor(true, currentStyle.Selection.BackColor, Color.Transparent);
            Npp.CaretLineBackColor = currentStyle.CaretLine.BackColor;

            // we want the default auto-completion to not show
            if (!_autoStopSet) {
                // barbarian method to force the default autocompletion window to hide, it makes npp slows down when there is too much text...
                // TODO: find a better technique to hide the autocompletion!!! this slows npp down
                Npp.AutoCStops(@"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_");
                _autoStopSet = true;
            }
        }

        internal static void ApplyDefaultOptionsForScintilla() {
            // nothing has been done yet, no need to reset anything! same if we already were on a non progress file
            if (!_hasBeenInit || !IsPreviousFileProgress)
                return;

            // apply default options
            Npp.TabWidth = _tabWidth;
            Npp.UseTabs = _indentWithTabs;
            Npp.AnnotationVisible = _annotationMode;
            if (Npp.ViewWhitespace != WhitespaceMode.Invisible)
                Npp.ViewWhitespace = _whitespaceMode;

            // apply default style...
            try {
                // read npp's stylers.xml file
                var widgetStyle = XDocument.Load(Config.FileNppStylersXml).Descendants("WidgetStyle");
                var xElements = widgetStyle as XElement[] ?? widgetStyle.ToArray();
                var wsFore = GetColorInStylers(xElements, "White space symbol", "fgColor");
                Npp.SetWhiteSpaceColor(true, Color.Transparent, wsFore);
                Npp.SetIndentGuideColor(GetColorInStylers(xElements, "Indent guideline style", "bgColor"), GetColorInStylers(xElements, "Indent guideline style", "fgColor"));
                Npp.SetSelectionColor(true, GetColorInStylers(xElements, "Selected text colour", "bgColor"), Color.Transparent);
                Npp.CaretLineBackColor = GetColorInStylers(xElements, "Current line background colour", "bgColor");
            } catch (Exception e) {
                ErrorHandler.Log(e.ToString());
                if (!_warnedAboutFailStylers) {
                    _warnedAboutFailStylers = true;
                    UserCommunication.Notify("Error while reading one of Notepad++ file :<div>" + Config.FileNppStylersXml.ToHtmlLink() + "</div>", MessageImg.MsgError, "Error reading stylers.xml", "Xml read error");
                }
            }
            
            // we wanted the default auto-completion to not show, but no more
            if (_autoStopSet) {
                Npp.AutoCStops("");
                _autoStopSet = false;
            }
        }

        private static Color GetColorInStylers(IEnumerable<XElement> widgetStyle, string attributeName, string attributeToGet) {
            try {
                return ColorTranslator.FromHtml("#" + (string)widgetStyle.First(x => x.Attribute("name").Value.Equals(attributeName)).Attribute(attributeToGet));
            } catch (Exception) {
                return Color.Transparent;
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