#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.FileExplorer;
using _3PA.MainFeatures.InfoToolTip;
using _3PA.MainFeatures.ModificationsTag;
using _3PA.MainFeatures.Parser.Pro;
using _3PA.MainFeatures.Pro;
using _3PA.MainFeatures.Pro.Deploy;
using _3PA.MainFeatures.SyntaxHighlighting;
using _3PA.NppCore;
using _3PA.WindowsCore;
using _3PA._Resource;
using MenuItem = _3PA.MainFeatures.MenuItem;

namespace _3PA {

    /// <summary>
    /// The entry points for this plugin are the following :<br></br>
    /// - Main (through UnmanagedExports)<br></br>
    /// - OnNppNotification (through UnmanagedExports)<br></br>
    /// - OnMouseMessage<br></br>
    /// - OnKeyDown<br></br>
    /// </summary>
    internal static class Plug {

        #region events

        /// <summary>
        /// Subscribe to this event, published when the current document in changed (on document open or tab switched)
        /// </summary>
        public static event Action OnDocumentChangedEnd;

        /// <summary>
        /// Published when the Npp windows is being moved
        /// </summary>
        public static event Action OnNppWindowsMove;

        // NOTE : be aware that if you subscribe to one of those events, a reference to the subscribing object is held by the publisher (this class). That means that you have to be very careful about explicitly unsubscribing from static events as they will keep the subscriber alive forever, i.e., you may end up with the managed equivalent of a memory leak.

        /// <summary>
        /// Published when Npp is shutting down, do your clean up actions
        /// </summary>
        public static event Action OnShutDown;

        /// <summary>
        /// Event published when the plugin is ready
        /// </summary>
        public static event Action OnPlugReady;

        #endregion

        #region Fields

        /// <summary>
        /// Allows us to know when a file is opened for the first time (to change encoding for instance)
        /// </summary>
        private static HashSet<string> _openedFileList = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region Called with no conditions

        #region Start

        /// <summary>
        /// Called by notepad++ when the plugin is loaded
        /// </summary>
        internal static void DoPlugLoad() {
            // Triggered when the resolution of an assembly fails, gives us the opportunity to feed the required assembly
            AppDomain.CurrentDomain.AssemblyResolve += LibLoader.AssemblyResolver;

            // catch unhandled errors to log them
            AppDomain.CurrentDomain.UnhandledException += ErrorHandler.UnhandledErrorHandler;
            Application.ThreadException += ErrorHandler.ThreadErrorHandler;
            TaskScheduler.UnobservedTaskException += ErrorHandler.UnobservedErrorHandler;
        }

        /// <summary>
        /// Called when the plugin menu of the plugin needs to be filled
        /// We can call UnmanagedExports.NppFuncItems.RefreshItems(); later on if we had stuff
        /// in the plugin menu via Npp.SetCommand
        /// </summary>
        internal static void DoFuncItemsNeeded() {
            var cmdIndex = 0;
            AppliMenu.MainMenuCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex++, "Show main menu  [Ctrl + Right click]", () => AppliMenu.ShowMainMenu(true));
            CodeExplorer.Instance.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex++, "Toggle code explorer", CodeExplorer.Instance.Toggle);
            FileExplorer.Instance.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex, "Toggle file explorer", FileExplorer.Instance.Toggle);
        }

        /// <summary>
        /// Called when the plugin can set new shorcuts to the toolbar in notepad++
        /// </summary>
        internal static void DoNppNeedToolbarImages() {
            Npp.SetToolbarImage(ImageResources.Logo16x16, AppliMenu.MainMenuCommandIndex);
            Npp.SetToolbarImage(ImageResources.FileExplorer16x16, FileExplorer.Instance.DockableCommandIndex);
            Npp.SetToolbarImage(ImageResources.CodeExplorer16x16, CodeExplorer.Instance.DockableCommandIndex);
        }

        /// <summary>
        /// Called on npp ready
        /// </summary>
        internal static bool DoNppReady() {
            try {
                // need to set some values in the yamuiThemeManager
                ThemeManager.OnStartUp();

                // code explorer
                if (Config.Instance.CodeExplorerAutoHideOnNonProgressFile) {
                    CodeExplorer.Instance.Toggle(Npp.NppFileInfo.GetFullPathApi.TestAgainstListOfPatterns(Config.Instance.FilesPatternProgress));
                } else if (Config.Instance.CodeExplorerVisible) {
                    CodeExplorer.Instance.Toggle();
                }

                // File explorer
                if (Config.Instance.FileExplorerAutoHideOnNonProgressFile) {
                    FileExplorer.Instance.Toggle(Npp.NppFileInfo.GetFullPathApi.TestAgainstListOfPatterns(Config.Instance.FilesPatternProgress));
                } else if (Config.Instance.FileExplorerVisible) {
                    FileExplorer.Instance.Toggle();
                }

                return true;
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Plugin startup");
            }
            return false;
        }

        internal static void DoPlugStart() {
            if (OnPlugReady != null)
                OnPlugReady();

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            // subscribe to static events
            ProEnvironment.OnEnvironmentChange += FileExplorer.Instance.RebuildFileList;
            ProEnvironment.OnEnvironmentChange += DataBase.Instance.UpdateDatabaseInfo;
            ProEnvironment.OnEnvironmentChange += ParserHandler.ClearStaticData;

            Keywords.Instance.OnImport += AutoCompletion.SetStaticItems;
            DataBase.Instance.OnDatabaseUpdate += AutoCompletion.SetStaticItems;
            AutoCompletion.OnUpdateStaticItems += ParserHandler.UpdateKnownStaticItems;

            ParserHandler.OnStart += CodeExplorer.Instance.OnStart;
            ParserHandler.OnEndSendCompletionItems += AutoCompletion.SetDynamicItems;
            ParserHandler.OnEndSendParserItems += CodeExplorer.Instance.OnParseEndParserItems;
            ParserHandler.OnEndSendParserItems += SyntaxFolding.OnParseEndParserItems;
            ParserHandler.OnEndSendCodeExplorerItems += CodeExplorer.Instance.OnParseEndCodeExplorerItems;
            ParserHandler.OnEnd += CodeExplorer.Instance.OnParseEnd;

            ProExecutionHandleCompilation.OnEachCompilationOk += OpenedFilesInfo.ProExecutionHandleCompilationOnEachCompilationOk;

            // Clear the %temp% directory if we didn't do it properly last time
            Utils.DeleteDirectory(Config.FolderTemp, true);

            //Snippets.Init();
            FileCustomInfo.Import();

            DelayedAction.StartNew(100, () => {
                if (Config.Instance.InstallStep == 0) {
                    Config.Instance.InstallStep++;

                    // we are at the first notepad++ start
                    Npp.ConfXml.FinishPluginInstall(); // will apply npp options and restart npp
                    return;
                }
                if (Config.Instance.InstallStep == 1) {
                    Config.Instance.InstallStep++;

                    // global options applied, we are at the second startup
                    UserCommunication.NotifyUnique("welcome", "Thank you for installing 3P, you are awesome!<br><br>If this is your first look at 3P you should probably read the <b>getting started</b> section of the home page by clicking " + "go".ToHtmlLink("on this link right here") + ".<br><br><div align='right'>And as always... Enjoy!</div>", MessageImg.MsgInfo, "Fresh install", "Hello and welcome aboard!", args => {
                        Appli.ToggleView();
                        UserCommunication.CloseUniqueNotif("welcome");
                        args.Handled = true;
                    });
                } else if (!Config.Instance.NppStoppedCorrectly) {
                    // Npp didn't stop correctly, if the backup mode is activated, inform the user
                    if (Npp.ConfXml.BackupMode > 0) {
                        UserCommunication.Notify("It seems that notepad++ didn't stop correctly.<br>If you lost some modifications, don't forget that you have a backup folder here :<br><br><div>" + Npp.ConfXml.BackupDirectory.ToHtmlLink() + "</div>" + (Npp.ConfXml.BackupUseCustomDir ? "<div>" + Npp.ConfXml.CustomBackupDirectory.ToHtmlLink() + "</div>" : ""), MessageImg.MsgInfo, "Notepad++ crashed", "Backup folder location");
                    }
                }

                Config.Instance.NppStoppedCorrectly = false;

                // check if an update was done and start checking for new updates
                Updater<MainUpdaterWrapper>.Instance.CheckForUpdateDoneAndStartCheckingForUpdates();

                if (Updater<ProlintUpdaterWrapper>.Instance.LocalVersion.IsHigherVersionThan("v0"))
                    Updater<ProlintUpdaterWrapper>.Instance.StartCheckingForUpdate();
                if (Updater<ProparseUpdaterWrapper>.Instance.LocalVersion.IsHigherVersionThan("v0"))
                    Updater<ProparseUpdaterWrapper>.Instance.StartCheckingForUpdate();

                // Try to update the configuration from the distant shared folder
                ShareExportConf.StartCheckingForUpdates();
            });
            
            // check if npp version is meeting current recommended version
            if (!string.IsNullOrEmpty(Npp.SoftwareVersion) && !Npp.SoftwareVersion.IsHigherOrEqualVersionThan(Config.RequiredNppVersion)) {
                if (!Config.Instance.NppOutdatedVersion) {
                    UserCommunication.Notify("This version of 3P has been developed for Notepad++ " + Config.RequiredNppVersion + ", your version (" + Npp.SoftwareVersion + ") is outdated.<br><br>Using an outdated version, you might encounter bugs that would not occur otherwise.<br>Try to update your version of Notepad++ as soon as possible, or use 3P at your own risks.<br><br><a href='https://notepad-plus-plus.org/download/'>Download the latest version of Notepad++ here</a>", MessageImg.MsgHighImportance, "Outdated version", "3P requirements are not met");
                    Config.Instance.NppOutdatedVersion = true;
                }
            } else
                Config.Instance.NppOutdatedVersion = false;
            
            // ReSharper disable once ObjectCreationAsStatement
            RecurentAction.StartNew(User.Ping, 1000 * 60 * 120);

            // Make sure to give the focus to scintilla on startup
            Sci.GrabFocus();

            DelayedAction.StartNew(1000, Config.Save);
        }

        #endregion

        #region Die

        /// <summary>
        /// Called on Npp shutdown
        /// </summary>
        internal static void DoNppShutDown() {
            try {
                if (OnShutDown != null)
                    OnShutDown();

                // clean up timers
                RecurentAction.CleanAll();
                DelayedAction.CleanAll();
                AsapButDelayableAction.CleanAll();

                // Triggered when the resolution of an assembly fails, gives us the opportunity to feed the required assembly
                AppDomain.CurrentDomain.AssemblyResolve -= LibLoader.AssemblyResolver;

                // catch unhandled errors to log them
                AppDomain.CurrentDomain.UnhandledException -= ErrorHandler.UnhandledErrorHandler;
                Application.ThreadException -= ErrorHandler.ThreadErrorHandler;
                TaskScheduler.UnobservedTaskException -= ErrorHandler.UnobservedErrorHandler;

                // unsubscribe to static events
                ProEnvironment.OnEnvironmentChange -= FileExplorer.Instance.RebuildFileList;
                ProEnvironment.OnEnvironmentChange -= DataBase.Instance.UpdateDatabaseInfo;
                ProEnvironment.OnEnvironmentChange -= ParserHandler.ClearStaticData;

                Keywords.Instance.OnImport -= AutoCompletion.SetStaticItems;
                DataBase.Instance.OnDatabaseUpdate -= AutoCompletion.SetStaticItems;
                AutoCompletion.OnUpdateStaticItems -= ParserHandler.UpdateKnownStaticItems;

                ParserHandler.OnEndSendCompletionItems -= AutoCompletion.SetDynamicItems;
                ParserHandler.OnStart -= CodeExplorer.Instance.OnStart;
                ParserHandler.OnEndSendParserItems -= CodeExplorer.Instance.OnParseEndParserItems;
                ParserHandler.OnEndSendCodeExplorerItems -= CodeExplorer.Instance.OnParseEndCodeExplorerItems;
                ParserHandler.OnEnd -= CodeExplorer.Instance.OnParseEnd;

                ProExecutionHandleCompilation.OnEachCompilationOk -= OpenedFilesInfo.ProExecutionHandleCompilationOnEachCompilationOk;

                // export modified conf
                FileCustomInfo.Export();

                // Npp stopped correctly
                Config.Instance.NppStoppedCorrectly = true;

                // save config (should be done but just in case)
                Config.Save();

                // remember the most used keywords
                Keywords.Instance.SaveRanking();

                // close every form
                FileExplorer.Instance.ForceClose();
                CodeExplorer.Instance.ForceClose();
                AutoCompletion.ForceClose();
                InfoToolTip.ForceClose();
                Appli.ForceClose();
                UserCommunication.ForceClose();
                AppliMenu.ForceClose();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Stop");
            }
        }

        #endregion

        #region On mouse message

        public static bool MouseMessageHandler(WinApi.Messages message, Win32Api.MOUSEHOOKSTRUCT mouseStruct) {
            switch (message) {
                // middle click : go to definition
                case WinApi.Messages.WM_MBUTTONDOWN:
                    if (Npp.CurrentFileInfo.IsProgress) {
                        if (KeyboardMonitor.GetModifiers.IsCtrl) {
                            Npp.GoBackFromDefinition();
                        } else {
                            ProMisc.GoToDefinition(true);
                        }
                    }
                    return true;
                //break;
                // (CTRL + ) Right click : show main menu
                case WinApi.Messages.WM_RBUTTONUP:
                    if (KeyboardMonitor.GetModifiers.IsCtrl) {
                        // we need the cursor to be in scintilla but not on the application or the autocompletion!
                        if ((!Appli.IsVisible || !Appli.IsMouseIn()) &&
                            (!InfoToolTip.IsVisible || !InfoToolTip.IsMouseIn()) &&
                            (!AutoCompletion.IsVisible || !AutoCompletion.IsMouseIn())) {
                            AppliMenu.ShowMainMenu(true);
                            return true;
                        }
                    }
                    break;
            }

            // HACK: The following is to handle the MOVE/RESIZE event of npp's window. 
            // It would be cleaner to use a WndProc bypass but it costs too much... this is a cheaper solution
            switch (message) {
                case WinApi.Messages.WM_NCLBUTTONDOWN:
                    if (!WinApi.GetWindowRect(Npp.CurrentSci.Handle).Contains(Cursor.Position)) {
                        MouseMonitor.Instance.Add(WinApi.Messages.WM_MOUSEMOVE);
                    }
                    break;
                case WinApi.Messages.WM_LBUTTONUP:
                case WinApi.Messages.WM_NCLBUTTONUP:
                    if (MouseMonitor.Instance.Remove(WinApi.Messages.WM_MOUSEMOVE)) {
                        if (OnNppWindowsMove != null)
                            OnNppWindowsMove();
                    }
                    break;
                case WinApi.Messages.WM_MOUSEMOVE:
                    if (OnNppWindowsMove != null)
                        OnNppWindowsMove();
                    break;
            }

            return false;
        }

        #endregion

        #region On key down

        /// <summary>
        /// Called when the user presses a key
        /// </summary>
        // ReSharper disable once RedundantAssignment
        public static bool KeyDownHandler(KeyEventArgs e) {
            // if set to true, the keyinput is completly intercepted, otherwise npp sill does its stuff
            bool handled = false;

            MenuItem menuItem = null;
            try {
                // Since it's a keydown message, we can receive this a lot if the user let a button pressed
                var isSpamming = Utils.IsSpamming(e.KeyCode.ToString(), 100, true);

                // check if the user triggered a 3P function defined in the AppliMenu
                menuItem = TriggeredMenuItem(AppliMenu.Instance.ShortcutableItemList, isSpamming, e, ref handled);
                if (handled)
                    return true;

                // Autocompletion 
                if (AutoCompletion.IsVisible) {
                    handled = AutoCompletion.PerformKeyDown(e);
                }

                // next tooltip
                if (!handled && InfoToolTip.IsVisible && e.Control && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)) {
                    if (e.KeyCode == Keys.Up)
                        InfoToolTip.IndexToShow--;
                    else
                        InfoToolTip.IndexToShow++;
                    InfoToolTip.TryToShowIndex();
                    handled = true;
                }

                if (handled)
                    return true;

                // Ok so... when we open a form in notepad++, we can't use the overrides PreviewKeyDown / KeyDown
                // like we normally can, for some reasons, they don't react to certain keys (like enter!)
                // It only works "almost normally" if we ShowDialog() the form?!
                // So i gave up and handle things here!
                // Each control / form that should use a key not handled by Npp should implement a method 
                // "PerformKeyDown" that will be triggered from here (see below)
                var curControl = Win32Api.GetFocusedControl();
                if (curControl != null) {
                    var invokeResponse = curControl.InvokeMethod("PerformKeyDown", new[] {(object) e});
                    if (invokeResponse != null && (bool) invokeResponse)
                        return true;
                }
                var curWindow = Control.FromHandle(WinApi.GetForegroundWindow());
                if (curWindow != null) {
                    var invokeResponse = curWindow.InvokeMethod("PerformKeyDown", new[] {(object) e});
                    if (invokeResponse != null && (bool) invokeResponse)
                        return true;
                }

                // Close interfacePopups
                if (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.Next || e.KeyCode == Keys.Prior)
                    ClosePopups();
            } catch (Exception ex) {
                ErrorHandler.ShowErrors(ex, "Occurred in : " + (menuItem == null ? new ShortcutKey(e.Control, e.Alt, e.Shift, e.KeyCode).ToString() : menuItem.ItemId));
            }

            return handled;
        }

        /// <summary>
        /// Check if the key/keymodifiers correspond to a item in the menu, if yes, returns this item and execute .Do()
        /// </summary>
        private static MenuItem TriggeredMenuItem(List<MenuItem> list, bool isSpamming, KeyEventArgs e, ref bool handled) {
            // check if the user triggered a 3P function defined in the AppliMenu
            foreach (var item in list) {
                // shortcut corresponds to the item?
                if ((byte) e.KeyCode == item.Shortcut._key &&
                    e.Control == item.Shortcut.IsCtrl &&
                    e.Shift == item.Shortcut.IsShift &&
                    e.Alt == item.Shortcut.IsAlt &&
                    (item.Generic || Npp.CurrentFileInfo.IsProgress)) {
                    if (!isSpamming && item.OnClic != null) {
                        try {
                            item.OnClic(item);
                        } catch (Exception ex) {
                            ErrorHandler.ShowErrors(ex, "Error in : " + item.DisplayText);
                        }
                    }
                    handled = true;
                    return item;
                }
            }
            return null;
        }

        #endregion

        #endregion

        #region Called when PluginIsReady

        #region OnNppFileBeforeClose

        /// <summary>
        /// Called when a file is about to be closed in notepad++
        /// </summary>
        public static void OnNppFileBeforeClose() {
            // remove the file from the opened files list
            if (_openedFileList.Contains(Npp.CurrentFileInfo.Path)) {
                _openedFileList.Remove(Npp.CurrentFileInfo.Path);
            }
        }

        #endregion

        #region OnNppFileOpened

        /// <summary>
        /// Called when a new file is opened in notepad++
        /// </summary>
        public static void OnNppFileOpened() {}

        #endregion

        #region On document switch

        /// <summary>
        /// Called when the user switches tab document
        /// You can use Npp.CurrentFile and Npp.PreviousFile in this method
        /// </summary>
        public static void DoNppBufferActivated(bool initiating = false) {
            // Apply options to npp and scintilla depending if we are on a progress file or not
            ApplyOptionsForScintilla();

            // if the file has just been opened
            if (!_openedFileList.Contains(Npp.CurrentFileInfo.Path)) {
                _openedFileList.Add(Npp.CurrentFileInfo.Path);

                // need to auto change encoding?
                if (Config.Instance.AutoSwitchEncodingTo != NppEncodingFormat._Automatic_default && !string.IsNullOrEmpty(Config.Instance.AutoSwitchEncodingForFilePatterns)) {
                    if (Npp.CurrentFileInfo.Path.TestAgainstListOfPatterns(Config.Instance.AutoSwitchEncodingForFilePatterns)) {
                        NppMenuCmd cmd;
                        if (Enum.TryParse(((int) Config.Instance.AutoSwitchEncodingTo).ToString(), true, out cmd))
                            Npp.RunCommand(cmd);
                    }
                }
            }

            // activate show space for conf files / deactivate if coming from a conf file
            if (ShareExportConf.IsFileExportedConf(Npp.PreviousFileInfo.Path))
                if (Sci.ViewWhitespace != WhitespaceMode.Invisible && !Sci.ViewEol)
                    Sci.ViewWhitespace = _whitespaceMode;
            if (ShareExportConf.IsFileExportedConf(Npp.CurrentFileInfo.Path))
                Sci.ViewWhitespace = WhitespaceMode.VisibleAlways;

            // close popups..
            ClosePopups();

            if (initiating) {
                // make sure to use the ProEnvironment and colorize the error counter
                OpenedFilesInfo.UpdateFileStatus();
            } else {
                if (Config.Instance.CodeExplorerAutoHideOnNonProgressFile) {
                    CodeExplorer.Instance.Toggle(Npp.CurrentFileInfo.IsProgress);
                }
                if (Config.Instance.FileExplorerAutoHideOnNonProgressFile) {
                    FileExplorer.Instance.Toggle(Npp.CurrentFileInfo.IsProgress);
                }
            }

            // Update info on the current file
            OpenedFilesInfo.UpdateErrorsInScintilla();
            ProEnvironment.Current.ReComputeProPath();

            AutoCompletion.SetStaticItems();

            // Parse the document
            ParserHandler.ParseDocumentNow();

            // publish the event
            if (OnDocumentChangedEnd != null)
                OnDocumentChangedEnd();
        }

        #endregion

        #region OnNppDocumentSaved

        /// <summary>
        /// Called when the current document is saved, 
        /// no matter if the document is a Progress file or not
        /// </summary>
        public static void DoNppDocumentSaved() {
            // if it's a conf file, import it
            ShareExportConf.TryToImportFile(Npp.CurrentFileInfo.Path);

            if (!Npp.CurrentFileInfo.IsProgress)
                return;

            // Display parser errors if any
            if (Config.Instance.DisplayParserErrorsOnSave && Npp.CurrentFileInfo.IsCompilable) {
                ProCodeFormat.DisplayParserErrors(true);
            }

            // update function prototypes
            ProGenerateCode.Factory.UpdateFunctionPrototypesIfNeeded(true);
        }

        #endregion

        #region OnNppFileBeforeLoad

        /// <summary>
        /// Called when a new file is being opened in notepad++
        /// </summary>
        public static void DoNppFileBeforeLoad() {
            // Reset the scintilla option for the indentation mode, as crazy as this is, it DESTROYS the performances
            // when opening big files in scintilla...
            Sci.AnnotationVisible = AnnotationMode;
        }

        #endregion

        #region OnLangChanged

        /// <summary>
        /// Called when the user changed the language for the current file
        /// </summary>
        public static void OnLangChanged() {
            Npp.CurrentFileInfo.SetAsNonProgress();
            // Npp.CurrentFileInfo.Lang.LangName
        }

        #endregion

        #region OnStyleNeeded

        /// <summary>
        /// If an container lexer is used, scintilla will call this event when a portion of the text needs to be styled
        /// </summary>
        public static void OnStyleNeeded(int startPos, int endPos){
            SyntaxHighlight.Colorize(startPos, endPos);
        }

        #endregion

        #endregion

        #region On char typed

        /// <summary>
        /// Called when a single char is added (in case of a new line in window format, this will be called but only with \r)
        /// Called after the UI has updated, allows to correctly read the text style, to correct 
        /// the indentation w/o it being erased and so on...
        /// </summary>
        public static void OnCharAdded(char c, int position) {
            try {
                // handles the autocompletion
                AutoCompletion.UpdateAutocompletion(c, position);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCharAdded");
            }
        }

        /// <summary>
        /// Called when a single char is deleted
        /// </summary>
        public static void OnCharDeleted(char c, int position) {
            try {
                // handles the autocompletion
                AutoCompletion.UpdateAutocompletion();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCharDeleted");
            }
        }

        #endregion

        #region OnSciUpdateUi

        public static void OnSciUpdateUi(SCNotification nc) {
            if (nc.updated == (int) SciMsg.SC_UPDATE_V_SCROLL ||
                nc.updated == (int) SciMsg.SC_UPDATE_H_SCROLL) {
                // user scrolled
                OnPageScrolled();
            } else if (nc.updated == (int) SciMsg.SC_UPDATE_SELECTION) {
                // the user changed its selection
                OnUpdateSelection();
            }
        }

        #endregion

        #region OnTextModified

        /// <summary>
        /// Called when the text in scintilla is modified (added/deleted) by the user (called after UI update)
        /// </summary>
        public static void OnTextModified(SCNotification nc, bool insertedText, bool deletedText, bool singleCharModification, bool undo, bool redo) {
            ParserHandler.ParseDocumentAsap();
            if (!singleCharModification) {
                ClosePopups();
            }
        }

        #endregion

        #region OnSciMarginClick

        /// <summary>
        /// When the user click on the margin
        /// </summary>
        public static void OnSciMarginClick(SCNotification nc) {
            if (!Npp.CurrentFileInfo.IsProgress)
                return;

            // click on the error margin
            if (nc.margin == OpenedFilesInfo.ErrorMarginNumber) {
                // if it's an error symbol that has been clicked, the error on the line will be cleared
                if (!OpenedFilesInfo.ClearLineErrors(Sci.LineFromPosition(nc.position))) {
                    // if nothing has been cleared, we go to the next error position
                    OpenedFilesInfo.GoToNextError(Sci.LineFromPosition(nc.position));
                }
            }
        }

        #endregion

        #region OnSciDwellStart

        /// <summary>
        /// When the user leaves his cursor inactive on npp
        /// </summary>
        public static void OnSciDwellStart() {
            if (WinApi.GetForegroundWindow() == Npp.Handle)
                InfoToolTip.ShowToolTipFromDwell();
        }

        #endregion

        #region OnSciDwellEnd

        /// <summary>
        /// When the user moves his cursor
        /// </summary>
        public static void OnSciDwellEnd() {
            if (!KeyboardMonitor.GetModifiers.IsCtrl)
                InfoToolTip.Cloak(true);
        }

        #endregion

        #region OnUpdateSelection

        /// <summary>
        /// called when the user changes its selection in npp (the caret moves)
        /// </summary>
        public static void OnUpdateSelection() {
            // close popup windows
            ClosePopups();
            //Snippets.FinalizeCurrent();

            if (!Npp.CurrentFileInfo.IsProgress)
                return;

            // update scope of code explorer (the selection img)
            CodeExplorer.Instance.UpdateCurrentScope();
        }

        #endregion

        #region OnPageScrolled

        /// <summary>
        /// called when the user scrolls..
        /// </summary>
        public static void OnPageScrolled() {
            ClosePopups();
            // parse the current part of the document
            if (!Npp.CurrentFileInfo.IsProgress)
                ParserHandler.ParseDocumentAsap();
        }

        #endregion
        
        #region Apply Scintilla options

        public static Annotation AnnotationMode {
            get { return _annotationMode; }
            set {
                // we want to set to our value
                if (value == Annotation.Indented) {
                    Sci.AnnotationVisible = Annotation.Indented;
                } else {
                    _annotationMode = value;
                }
            }
        }

        private static Annotation _annotationMode = Annotation.Hidden;

        private static bool _indentWithTabs;
        private static int _tabWidth = -1;
        private static WhitespaceMode _whitespaceMode = WhitespaceMode.Invisible;

        private static int[] _initiatedScintilla = {0, 0};
        private static bool _hasBeenInit;

        /// <summary>
        /// We need certain options to be set to specific values when running this plugin, make sure to set everything back to normal
        /// when switch tab or when we leave npp, param can be set to true to force the default values
        /// </summary>
        internal static void ApplyOptionsForScintilla() {
            // need to do this stuff only once for each scintilla

            var curScintilla = Npp.CurrentSciId; // 0 or 1
            if (_initiatedScintilla[curScintilla] == 0) {
                // Extra settings at the start
                Sci.MouseDwellTime = Config.Instance.ToolTipmsBeforeShowing;
                Sci.EndAtLastLine = false;
                //Npp.EventMask = (int) (SciModificationMod.SC_MOD_INSERTTEXT | SciModificationMod.SC_MOD_DELETETEXT | SciModificationMod.SC_PERFORMED_USER | SciModificationMod.SC_PERFORMED_UNDO | SciModificationMod.SC_PERFORMED_REDO);                
                _initiatedScintilla[curScintilla] = 1;
            }

            if (Npp.CurrentFileInfo.IsProgress)
                ApplyPluginOptionsForScintilla();
            else
                ApplyDefaultOptionsForScintilla();
        }

        internal static void ApplyPluginOptionsForScintilla() {
            if (!_hasBeenInit || !Npp.PreviousFileInfo.IsProgress) {
                // read default options
                _tabWidth = Sci.TabWidth;
                _indentWithTabs = Sci.UseTabs;
                _whitespaceMode = Sci.ViewWhitespace;
                AnnotationMode = Sci.AnnotationVisible;
                _hasBeenInit = true;
            }

            Sci.TabWidth = Config.Instance.CodeTabSpaceNb;
            Sci.UseTabs = false;
            if (Config.Instance.CodeShowSpaces)
                Sci.ViewWhitespace = WhitespaceMode.VisibleAlways;

            // apply style
            ScintillaTheme.CurrentTheme.SetScintillaStyles();

            // activate syntax highlighting
            SyntaxHighlight.ActivateHighlight();
        }

        internal static void ApplyDefaultOptionsForScintilla() {
            // nothing has been done yet, no need to reset anything! same if we already were on a non progress file
            if (!_hasBeenInit || !Npp.PreviousFileInfo.IsProgress)
                return;

            // apply default options
            Sci.TabWidth = _tabWidth;
            Sci.UseTabs = _indentWithTabs;
            Sci.AnnotationVisible = AnnotationMode;

            if (Sci.ViewWhitespace != WhitespaceMode.Invisible && !Sci.ViewEol)
                Sci.ViewWhitespace = _whitespaceMode;

            ScintillaTheme.SetDefaultScintillaStyles();
        }

        #endregion

        #region utils

        /// <summary>
        /// Call this method to close all popup/autocompletion form and alike
        /// </summary>
        public static void ClosePopups() {
            AutoCompletion.Cloak();
            InfoToolTip.Cloak();
        }

        #endregion

    }
}