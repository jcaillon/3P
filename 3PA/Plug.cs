#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Text;
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
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro;
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
            Npp.SetCommand(cmdIndex++, "Show main menu  [Ctrl + Right click]", AppliMenu.ShowMainMenuAtCursor);
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
                if (!string.IsNullOrEmpty(Npp.SoftwareVersion) && !Npp.SoftwareVersion.IsHigherVersionThan("7.2")) {
                    if (!Config.Instance.NppOutdatedVersion) {
                        UserCommunication.Notify("Dear user,<br><br>Your version of Notepad++ (" + Npp.SoftwareVersion + ") is outdated.<br>3P releases are always tested with the most updated major version of Notepad++.<br>Using an outdated version, you might encounter bugs that would not occur otherwise, <b>there are known issues with inferior versions</b>.<br><br>Please upgrade to an up-to-date version of Notepad++ or use 3P at your own risks.<br><br><a href='https://notepad-plus-plus.org/download/'>Download the lastest version of Notepad++ here</a>", MessageImg.MsgError, "Outdated version", "3P requirements are not met");
                        Config.Instance.NppOutdatedVersion = true;
                    }
                } else
                    Config.Instance.NppOutdatedVersion = false;

                // Check if an update has been done and start checking for new updates
                UpdateHandler.CheckForUpdateDone();
                UpdateHandler.StartCheckingForUpdate(); // async

                // Try to update the configuration from the distant shared folder
                ShareExportConf.StartCheckingForUpdates();

                // ReSharper disable once ObjectCreationAsStatement
                RecurentAction.StartNew(User.Ping, 1000*60*120);

                // code explorer
                if (Config.Instance.CodeExplorerAutoHideOnNonProgressFile) {
                    CodeExplorer.Instance.Toggle(Npp.NppFileInfo.GetFullPathApi.TestAgainstListOfPatterns(Config.Instance.ProgressFilesPattern));
                } else if (Config.Instance.CodeExplorerVisible) {
                    CodeExplorer.Instance.Toggle();
                }

                // File explorer
                if (Config.Instance.FileExplorerAutoHideOnNonProgressFile) {
                    FileExplorer.Instance.Toggle(Npp.NppFileInfo.GetFullPathApi.TestAgainstListOfPatterns(Config.Instance.ProgressFilesPattern));
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

            // subscribe to static events
            ProEnvironment.OnEnvironmentChange += FileExplorer.Instance.RebuildFileList;
            ProEnvironment.OnEnvironmentChange += DataBase.UpdateDatabaseInfo;
            ProEnvironment.OnEnvironmentChange += ParserHandler.ClearStaticData;

            Keywords.Instance.OnImport += ParserHandler.UpdateKnownStaticItems;
            DataBase.OnDatabaseUpdate += ParserHandler.UpdateKnownStaticItems;
            Keywords.Instance.OnImport += AutoCompletion.SetStaticItems;
            DataBase.OnDatabaseUpdate += AutoCompletion.SetStaticItems;

            ParserHandler.OnEndSendCompletionItems += AutoCompletion.OnParseEnded;
            ParserHandler.OnStart += CodeExplorer.Instance.OnStart;
            ParserHandler.OnEndSendParserItems += CodeExplorer.Instance.OnParseEndParserItems;
            ParserHandler.OnEndSendCodeExplorerItems += CodeExplorer.Instance.OnParseEndCodeExplorerItems;
            ParserHandler.OnEnd += CodeExplorer.Instance.OnParseEnd;

            // Clear the %temp% directory if we didn't do it properly last time
            Utils.DeleteDirectory(Config.FolderTemp, true);

            //Snippets.Init();
            FileTag.Import();

            // Make sure to give the focus to scintilla on startup
            Sci.GrabFocus();

            // ask to disable the default autocompletion
            DelayedAction.StartNew(100, Npp.ConfXml.AskToDisableAutocompletion);
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

                // unsubscribe to static events
                ProEnvironment.OnEnvironmentChange -= FileExplorer.Instance.RebuildFileList;
                ProEnvironment.OnEnvironmentChange -= DataBase.UpdateDatabaseInfo;
                ProEnvironment.OnEnvironmentChange -= ParserHandler.ClearStaticData;
                ProEnvironment.OnEnvironmentChange -= ParserHandler.UpdateKnownStaticItems;

                ParserHandler.OnEndSendCompletionItems -= AutoCompletion.OnParseEnded;

                ParserHandler.OnStart -= CodeExplorer.Instance.OnStart;
                ParserHandler.OnEndSendParserItems -= CodeExplorer.Instance.OnParseEndParserItems;
                ParserHandler.OnEndSendCodeExplorerItems -= CodeExplorer.Instance.OnParseEndCodeExplorerItems;
                ParserHandler.OnEnd -= CodeExplorer.Instance.OnParseEnd;

                // clean up timers
                RecurentAction.CleanAll();
                DelayedAction.CleanAll();
                AsapButDelayableAction.CleanAll();

                // export modified conf
                FileTag.Export();

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
                    if (Npp.CurrentFile.IsProgress) {
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
                        // we need the cursor to be in scintilla but not on the application or the auto-completion!
                        if ((!Appli.IsVisible || !Appli.IsMouseIn()) &&
                            (!InfoToolTip.IsVisible || !InfoToolTip.IsMouseIn()) &&
                            (!AutoCompletion.IsVisible || !AutoCompletion.IsMouseIn())) {
                            AppliMenu.ShowMainMenuAtCursor();
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
                    (item.Generic || Npp.CurrentFile.IsProgress)) {
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
            if (_openedFileList.Contains(Npp.CurrentFile.Path)) {
                _openedFileList.Remove(Npp.CurrentFile.Path);
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
            if (!_openedFileList.Contains(Npp.CurrentFile.Path)) {
                _openedFileList.Add(Npp.CurrentFile.Path);

                // need to auto change encoding?
                if (Config.Instance.AutoSwitchEncodingTo != NppEncodingFormat._Automatic_default && !string.IsNullOrEmpty(Config.Instance.AutoSwitchEncodingForFilePatterns)) {
                    if (Npp.CurrentFile.Path.TestAgainstListOfPatterns(Config.Instance.AutoSwitchEncodingForFilePatterns)) {
                        NppMenuCmd cmd;
                        if (Enum.TryParse(((int) Config.Instance.AutoSwitchEncodingTo).ToString(), true, out cmd))
                            Npp.RunCommand(cmd);
                    }
                }
            }

            // activate show space for conf files / deactivate if coming from a conf file
            if (ShareExportConf.IsFileExportedConf(Npp.PreviousFile.Path))
                if (Sci.ViewWhitespace != WhitespaceMode.Invisible && !Sci.ViewEol)
                    Sci.ViewWhitespace = _whitespaceMode;
            if (ShareExportConf.IsFileExportedConf(Npp.CurrentFile.Path))
                Sci.ViewWhitespace = WhitespaceMode.VisibleAlways;

            // close popups..
            ClosePopups();

            if (initiating) {
                // make sure to use the ProEnvironment and colorize the error counter
                FilesInfo.UpdateFileStatus();
            } else {
                if (Config.Instance.CodeExplorerAutoHideOnNonProgressFile) {
                    CodeExplorer.Instance.Toggle(Npp.CurrentFile.IsProgress);
                }
                if (Config.Instance.FileExplorerAutoHideOnNonProgressFile) {
                    FileExplorer.Instance.Toggle(Npp.CurrentFile.IsProgress);
                }
            }

            // Update info on the current file
            FilesInfo.UpdateErrorsInScintilla();
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
            ShareExportConf.TryToImportFile(Npp.CurrentFile.Path);
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

        #endregion

        #region On char typed

        /// <summary>
        /// Called when the user is still typing a word
        /// Called after the UI has updated, allows to correctly read the text style, to correct 
        /// the indentation w/o it being erased and so on...
        /// </summary>
        public static void OnCharAdded(char c) {
            try {
                // handles the autocompletion
                AutoCompletion.UpdateAutocompletion(c);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCharAdded");
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

        #region OnSciModified

        /// <summary>
        /// This notification is sent when the text or styling of the document changes or is about to change
        /// </summary>
        public static void OnSciModified(SCNotification nc) {
            // the user pressed UNDO or REDO
            if ((nc.modificationType & (int) SciModificationMod.SC_LASTSTEPINUNDOREDO) != 0) {
                ClosePopups();
            }
        }

        /// <summary>
        /// Called when the text in scintilla is modified by the user
        /// </summary>
        public static void OnTextModified(SCNotification nc, bool insertedText, bool deletedText) {
            ParserHandler.ParseDocumentAsap();

            // did the user supress 1 char? (or one line)
            if (deletedText && (nc.length == 1 || (nc.length == 2 && nc.linesAdded == -1))) {
                AutoCompletion.UpdateAutocompletion();
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// When the user click on the margin
        /// </summary>
        public static void OnSciMarginClick(SCNotification nc) {
            if (!Npp.CurrentFile.IsProgress)
                return;

            // click on the error margin
            if (nc.margin == FilesInfo.ErrorMarginNumber) {
                // if it's an error symbol that has been clicked, the error on the line will be cleared
                if (!FilesInfo.ClearLineErrors(Sci.LineFromPosition(nc.position))) {
                    // if nothing has been cleared, we go to the next error position
                    FilesInfo.GoToNextError(Sci.LineFromPosition(nc.position));
                }
            }
        }

        /// <summary>
        /// When the user leaves his cursor inactive on npp
        /// </summary>
        public static void OnSciDwellStart() {
            if (!Npp.CurrentFile.IsProgress)
                return;

            if (WinApi.GetForegroundWindow() == Npp.Handle)
                InfoToolTip.ShowToolTipFromDwell();
        }

        /// <summary>
        /// When the user moves his cursor
        /// </summary>
        public static void OnSciDwellEnd() {
            if (!KeyboardMonitor.GetModifiers.IsCtrl)
                InfoToolTip.Cloak(true);
        }

        /// <summary>
        /// called when the user changes its selection in npp (the caret moves)
        /// </summary>
        public static void OnUpdateSelection() {
            // close popup windows
            ClosePopups();
            //Snippets.FinalizeCurrent();

            if (!Npp.CurrentFile.IsProgress)
                return;

            // update scope of code explorer (the selection img)
            CodeExplorer.Instance.UpdateCurrentScope();
        }

        /// <summary>
        /// called when the user scrolls..
        /// </summary>
        public static void OnPageScrolled() {
            ClosePopups();
        }

        /// <summary>
        /// Called when the user saves the current document (just before it saves itself)
        /// </summary>
        public static void OnNppFileBeforeSaved() {
            if (!Npp.CurrentFile.IsProgress)
                return;

            // check for block that are too long and display a warning
            if (Abl.IsCurrentFileFromAppBuilder && !FilesInfo.CurrentFileInfoObject.WarnedTooLong) {
                var warningMessage = new StringBuilder();

                var explorerItemsList = CodeExplorer.Instance.ParsedExplorerItemsList;
                if (explorerItemsList != null) {
                    foreach (var codeExplorerItem in explorerItemsList.Where(codeExplorerItem => codeExplorerItem.Flags.HasFlag(ParseFlag.IsTooLong)))
                        warningMessage.AppendLine("<div><img src='IsTooLong'><img src='" + codeExplorerItem.Branch + "' style='padding-right: 10px'><a href='" + codeExplorerItem.GoToLine + "'>" + codeExplorerItem.DisplayText + "</a></div>");
                    if (warningMessage.Length > 0) {
                        warningMessage.Insert(0, "<h2>Friendly warning :</h2>It seems that your file can be opened in the appbuilder as a structured procedure, but i detected that one or several procedure/function blocks contains more than " + Config.Instance.GlobalMaxNbCharInBlock + " characters. A direct consequence is that you won't be able to open this file in the appbuilder, it will generate errors and it will be unreadable. Below is a list of incriminated blocks :<br><br>");
                        warningMessage.Append("<br><i>To prevent this, reduce the number of chararacters in the above blocks, deleting dead code and trimming spaces is a good place to start!</i>");
                        var curPath = Npp.CurrentFile.Path;
                        UserCommunication.NotifyUnique("AppBuilderLimit", warningMessage.ToString(), MessageImg.MsgHighImportance, "File saved", "Appbuilder limitations", args => {
                            Npp.Goto(curPath, int.Parse(args.Link));
                            UserCommunication.CloseUniqueNotif("AppBuilderLimit");
                        }, 20);
                        FilesInfo.CurrentFileInfoObject.WarnedTooLong = true;
                    }
                }
            }

            // for debug purposes, check if the document can be parsed
            if (Config.IsDevelopper) {
                var parserErrors = ParserHandler.GetLastParseErrorsInHtml();
                if (!string.IsNullOrEmpty(parserErrors))
                    UserCommunication.Notify("The parser found errors on this file:<br>" + parserErrors, MessageImg.MsgInfo, "Parser message", "Errors found", 3);
            }

            // update function prototypes
            if (Npp.CurrentFile.IsProgress)
                ProGenerateCode.UpdateFunctionPrototypesIfNeeded(true);
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

            if (Npp.CurrentFile.IsProgress)
                ApplyPluginOptionsForScintilla();
            else
                ApplyDefaultOptionsForScintilla();
        }

        internal static void ApplyPluginOptionsForScintilla() {
            if (!_hasBeenInit || !Npp.PreviousFile.IsProgress) {
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
            Style.SetSyntaxStyles();
            var currentStyle = Style.Current;
            Sci.SetIndentGuideColor(currentStyle.WhiteSpace.BackColor, currentStyle.WhiteSpace.ForeColor);
            Sci.SetWhiteSpaceColor(true, Color.Transparent, currentStyle.WhiteSpace.ForeColor);
            Sci.SetSelectionColor(true, currentStyle.Selection.BackColor, Color.Transparent);
            Sci.CaretLineBackColor = currentStyle.CaretLine.BackColor;
            Sci.CaretColor = currentStyle.CaretColor.ForeColor;
            Sci.SetFoldMarginColors(true, currentStyle.FoldMargin.BackColor, currentStyle.FoldMargin.BackColor);
            Sci.SetFoldMarginMarkersColor(currentStyle.FoldMargin.ForeColor, currentStyle.FoldMargin.BackColor, currentStyle.FoldActiveMarker.ForeColor);
        }

        internal static void ApplyDefaultOptionsForScintilla() {
            // nothing has been done yet, no need to reset anything! same if we already were on a non progress file
            if (!_hasBeenInit || !Npp.PreviousFile.IsProgress)
                return;

            // apply default options
            Sci.TabWidth = _tabWidth;
            Sci.UseTabs = _indentWithTabs;
            Sci.AnnotationVisible = AnnotationMode;

            if (Sci.ViewWhitespace != WhitespaceMode.Invisible && !Sci.ViewEol)
                Sci.ViewWhitespace = _whitespaceMode;

            // read npp's stylers.xml file
            Sci.SetIndentGuideColor(Npp.StylersXml.IndentGuideLineBg, Npp.StylersXml.IndentGuideLineFg);
            Sci.SetWhiteSpaceColor(true, Color.Transparent, Npp.StylersXml.WhiteSpaceFg);
            Sci.SetSelectionColor(true, Npp.StylersXml.SelectionBg, Color.Transparent);
            Sci.CaretLineBackColor = Npp.StylersXml.CaretLineBg;
            Sci.CaretColor = Npp.StylersXml.CaretFg;
            Sci.SetFoldMarginColors(true, Npp.StylersXml.FoldMarginBg, Npp.StylersXml.FoldMarginFg);
            Sci.SetFoldMarginMarkersColor(Npp.StylersXml.FoldMarginMarkerFg, Npp.StylersXml.FoldMarginMarkerBg, Npp.StylersXml.FoldMarginMarkerActiveFg);
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