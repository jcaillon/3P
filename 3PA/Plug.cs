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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
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
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro;
using MenuItem = _3PA.MainFeatures.MenuItem;

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
        /// Envent published when the plugin is ready
        /// </summary>
        public static event Action OnPlugReady;

        #endregion

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

        /// <summary>
        /// this is a delegate to defined actions that must be taken after updating the ui
        /// </summary>
        public static Queue<Action> ActionsAfterUpdateUi = new Queue<Action>();

        #endregion

        private static HashSet<string> _openedFileList = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase); 

        #region Called with no conditions

        #region Start

        /// <summary>
        /// Called by notepad++ when the plugin is loaded
        /// </summary>
        internal static void DoPlugLoad() {
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
        internal static void DoFuncItemsNeeded() {
            var cmdIndex = 0;
            AppliMenu.MainMenuCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex++, "Show main menu  [Ctrl + Right click]", AppliMenu.ShowMainMenuAtCursor);
            CodeExplorer.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex++, "Toggle code explorer", CodeExplorer.Toggle);
            FileExplorer.DockableCommandIndex = cmdIndex;
            Npp.SetCommand(cmdIndex, "Toggle file explorer", FileExplorer.Toggle);
        }

        /// <summary>
        /// Called when the plugin can set new shorcuts to the toolbar in notepad++
        /// </summary>
        internal static void DoNppNeedToolbarImages() {
            Npp.SetToolbarImage(ImageResources.logo16x16, AppliMenu.MainMenuCommandIndex);
            Npp.SetToolbarImage(ImageResources.FileExplorer16x16, FileExplorer.DockableCommandIndex);
            Npp.SetToolbarImage(ImageResources.CodeExplorer16x16, CodeExplorer.DockableCommandIndex);
        }

        /// <summary>
        /// Called on npp ready
        /// </summary>
        internal static bool DoNppReady() {
            try {
                // need to set some values in the yamuiThemeManager
                ThemeManager.OnStartUp();

                // init an empty form, this gives us a Form to hook onto if we want to do stuff on the UI thread
                // from a back groundthread with BeginInvoke()
                // once this method is done, we are able to publish notifications
                UserCommunication.Init();

                // Clear the %temp% directory if we didn't do it properly last time
                Utils.DeleteDirectory(Config.FolderTemp, true);

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

                // ReSharper disable once ObjectCreationAsStatement
                new ReccurentAction(User.Ping, 1000*60*120);

                // code explorer
                if (Config.Instance.CodeExplorerAutoHideOnNonProgressFile) {
                    CodeExplorer.Toggle(Abl.IsCurrentProgressFile);
                } else if (Config.Instance.CodeExplorerVisible) {
                    CodeExplorer.Toggle();
                }

                // File explorer
                if (Config.Instance.FileExplorerAutoHideOnNonProgressFile) {
                    FileExplorer.Toggle(Abl.IsCurrentProgressFile);
                } else if (Config.Instance.FileExplorerVisible) {
                    FileExplorer.Toggle();
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
            ProEnvironment.OnEnvironmentChange += FileExplorer.RebuildFileList;
            ProEnvironment.OnEnvironmentChange += DataBase.UpdateDatabaseInfo;
            DataBase.OnDatabaseInfoUpdated += AutoComplete.RefreshStaticItems;

            ParserHandler.OnParseStarted += () => { CodeExplorer.Refreshing = true; };
            ParserHandler.OnParseEnded += AutoComplete.RefreshDynamicItems;
            ParserHandler.OnParseEnded += CodeExplorer.UpdateCodeExplorer;

            AutoComplete.OnUpdatedStaticItems += Parser.UpdateKnownStaticItems;

            Keywords.Import();
            Snippets.Init();
            FileTag.Import();

            // initialize the list of objects of the autocompletion form
            AutoComplete.RefreshStaticItems();

            // Simulates a OnDocumentSwitched when we start this dll
            IsCurrentFileProgress = Abl.IsCurrentProgressFile; // to correctly init isPreviousProgress
            DoNppBufferActivated(true); // triggers OnEnvironmentChange via ProEnvironment.Current.ReComputeProPath();

            // Make sure to give the focus to scintilla on startup
            WinApi.SetForegroundWindow(Npp.HandleNpp);
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

        #region On mouse message

        private static bool MouseMessageHandler(WinApi.WindowsMessageMouse message, WinApi.MOUSEHOOKSTRUCT mouseStruct) {

            switch (message) {
                // middle click : go to definition
                case WinApi.WindowsMessageMouse.WM_MBUTTONDOWN:
                    //if (Npp.GetScintillaRectangle().Contains(Cursor.Position)) {
                    if (KeyboardMonitor.GetModifiers.IsCtrl) {
                        Npp.GoBackFromDefinition();
                    } else {
                        ProMisc.GoToDefinition(true);
                    }
                    return true;
                    //break;
                // (CTRL + ) Right click : show main menu
                case WinApi.WindowsMessageMouse.WM_RBUTTONUP:
                    if (KeyboardMonitor.GetModifiers.IsCtrl) {
                        // we need the cursor to be in scintilla but not on the application or the auto-completion!
                        if ((!Appli.IsVisible || !Appli.IsMouseIn()) &&
                            (!InfoToolTip.IsVisible || !InfoToolTip.IsMouseIn()) &&
                            (!AutoComplete.IsVisible || !AutoComplete.IsMouseIn())) {
                            AppliMenu.ShowMainMenuAtCursor();
                            return true;
                        }
                    }
                    break;
            }

            // HACK: The following is to handle the MOVE/RESIZE event of npp's window. 
            // It would be cleaner to use a WndProc bypass but it costs too much... this is a cheaper solution
            switch (message) {
                case WinApi.WindowsMessageMouse.WM_NCLBUTTONDOWN:
                    if (!WinApi.GetWindowRect(Npp.HandleScintilla).Contains(Cursor.Position)) {
                        MouseMonitor.Instance.Add(WinApi.WindowsMessageMouse.WM_MOUSEMOVE);
                    }
                    break;
                case WinApi.WindowsMessageMouse.WM_LBUTTONUP:
                case WinApi.WindowsMessageMouse.WM_NCLBUTTONUP:
                    if (MouseMonitor.Instance.Remove(WinApi.WindowsMessageMouse.WM_MOUSEMOVE)) {
                        if (OnNppWindowsMove != null)
                            OnNppWindowsMove();
                    }
                    break;
                case WinApi.WindowsMessageMouse.WM_MOUSEMOVE:
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
        private static bool KeyDownHandler(Keys key, KeyModifiers keyModifiers) {
            // if set to true, the keyinput is completly intercepted, otherwise npp sill does its stuff
            bool handled = false;

            MenuItem menuItem = null;
            try {
                // Since it's a keydown message, we can receive this a lot if the user let a button pressed
                var isSpamming = Utils.IsSpamming(key.ToString(), 100, true);

                // check if the user triggered a 3P function defined in the AppliMenu
                menuItem = TriggeredMenuItem(AppliMenu.Instance.ShortcutableItemList, isSpamming, key, keyModifiers, ref handled);
                if (handled)
                    return true;

                // hide window on escape
                if (Appli.IsFocused() && key == Keys.Escape) {
                    Appli.Form.Cloack();
                    return true;
                }

                // The following is specific to 3P files
                if (IsCurrentFileProgress) {

                    // Autocompletion 
                    if (AutoComplete.IsVisible) {
                        if (key == Keys.Up || key == Keys.Down || key == Keys.Tab || key == Keys.Return || key == Keys.Escape)
                            handled = AutoComplete.OnKeyDown(key);
                        else {

                            if ((key == Keys.Right || key == Keys.Left) && keyModifiers.IsAlt)
                                handled = AutoComplete.OnKeyDown(key);
                        }
                    } else {
                        // snippet ?
                        if (key == Keys.Tab || key == Keys.Escape || key == Keys.Return) {
                            if (!keyModifiers.IsCtrl && !keyModifiers.IsAlt && !keyModifiers.IsShift) {
                                if (!Snippets.InsertionActive) {
                                    //no snippet insertion in progress
                                    if (key == Keys.Tab) {
                                        if (Snippets.TriggerCodeSnippetInsertion()) {
                                            handled = true;
                                        }
                                    }
                                } else {
                                    //there is a snippet insertion in progress
                                    if (key == Keys.Tab) {
                                        if (Snippets.NavigateToNextParam())
                                            handled = true;
                                    } else if (key == Keys.Escape || key == Keys.Return) {
                                        Snippets.FinalizeCurrent();
                                        if (key == Keys.Return)
                                            handled = true;
                                    }
                                }
                            }
                        }
                    }

                    // next tooltip
                    if (!handled && InfoToolTip.IsVisible && keyModifiers.IsCtrl && (key == Keys.Up || key == Keys.Down)) {
                        if (key == Keys.Up)
                            InfoToolTip.IndexToShow--;
                        else
                            InfoToolTip.IndexToShow++;
                        InfoToolTip.TryToShowIndex();
                        handled = true;
                    }
                                        
                    if (handled)
                        return true;
                }
                
                //HACK:
                // Ok so... when we open a form in notepad++, we can't use the overrides PreviewKeyDown / KeyDown
                // like we normally can, for some reasons, they don't react to certain keys (like enter!)
                // It only works "almost normally" if we ShowDialog() the form?!
                // So i gave up and handle things here!

                // YamuiMenu
                var curMenu = (Control.FromHandle(WinApi.GetForegroundWindow()));
                var menu = curMenu as YamuiMenu;
                if (menu != null) {
                    handled = menu.OnKeyDown(key);
                } else {
                    var currentCtrl = WinApi.GetFocusedControl();

                    // YamuiButton
                    var currentButton = currentCtrl as YamuiButton;
                    if (currentButton != null) {
                        var evnt = new KeyEventArgs(key);
                        currentButton.OnSuperKeyDown(evnt);
                        handled = evnt.Handled;
                    } else {

                        // YamuiTextBox
                        var currentTxtBox = currentCtrl as YamuiTextBox;
                        if (currentTxtBox != null && key == Keys.Return) {
                            if (currentTxtBox.MultiLines) {
                                var initialPos = currentTxtBox.SelectionStart;
                                currentTxtBox.Text = currentTxtBox.Text.Substring(0, initialPos) + Environment.NewLine + (initialPos < currentTxtBox.TextLength ? currentTxtBox.Text.Substring(initialPos, currentTxtBox.TextLength - initialPos) : "");
                                currentTxtBox.SelectionStart = initialPos + 2;
                                currentTxtBox.SelectionLength = 0;
                                currentTxtBox.ScrollToCaret();
                            }
                        } 
                    }
                }

                if (handled)
                    return true;

                // Close interfacePopups
                if (key == Keys.PageDown || key == Keys.PageUp || key == Keys.Next || key == Keys.Prior)
                    ClosePopups();

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Occured in : " + (menuItem == null ? (new ShortcutKey(keyModifiers.IsCtrl, keyModifiers.IsAlt, keyModifiers.IsShift, key)).ToString() : menuItem.ItemId));
            }

            return handled;
        }

        /// <summary>
        /// Check if the key/keymodifiers correspond to a item in the menu, if yes, returns this item and execute .Do()
        /// </summary>
        private static MenuItem TriggeredMenuItem(List<MenuItem> list, bool isSpamming, Keys key, KeyModifiers keyModifiers, ref bool handled) {

            // check if the user triggered a 3P function defined in the AppliMenu
            foreach (var item in list) {
                // shortcut corresponds to the item?
                if ((byte) key == item.Shortcut._key &&
                    keyModifiers.IsCtrl == item.Shortcut.IsCtrl &&
                    keyModifiers.IsShift == item.Shortcut.IsShift &&
                    keyModifiers.IsAlt == item.Shortcut.IsAlt &&
                    (item.Generic || IsCurrentFileProgress)) {
                    if (!isSpamming) {
                        item.Do();
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
        private static void OnNppFileBeforeClose() {

            // remove the file from the opened files list
            if (_openedFileList.Contains(CurrentFilePath)) {
                _openedFileList.Remove(CurrentFilePath);
            }
        }

        #endregion

        #region OnNppFileOpened

        /// <summary>
        /// Called when a new file is opened in notepad++
        /// </summary>
        private static void OnNppFileOpened() {
        }

        #endregion

        #region On document switch

        /// <summary>
        /// Called when the user switches tab document, 
        /// no matter if the document is a Progress file or not
        /// </summary>
        public static void DoNppBufferActivated(bool initiating = false) {
            
            // if the file has just been opened
            var currentFile = Npp.GetCurrentFilePath();
            if (!_openedFileList.Contains(currentFile)) {
                _openedFileList.Add(currentFile);

                // need to auto change encoding?
                if (Config.Instance.AutoSwitchEncodingTo != NppEncodingFormat._Automatic_default && !string.IsNullOrEmpty(Config.Instance.AutoSwitchEncodingForFilePatterns)) {
                    if (Npp.GetCurrentFilePath().TestAgainstListOfPatterns(Config.Instance.AutoSwitchEncodingForFilePatterns)) {
                        NppMenuCmd cmd;
                        if (Enum.TryParse(((int)Config.Instance.AutoSwitchEncodingTo).ToString(), true, out cmd))
                            Npp.RunCommand(cmd);
                    }
                }
            }

            // deactivate show space for conf files
            if (ShareExportConf.IsFileExportedConf(CurrentFilePath))
                if (Npp.ViewWhitespace != WhitespaceMode.Invisible && !Npp.ViewEol)
                    Npp.ViewWhitespace = _whitespaceMode;

            DoNppDocumentSwitched(initiating);

            // activate show space for conf files
            if (ShareExportConf.IsFileExportedConf(CurrentFilePath))
                Npp.ViewWhitespace = WhitespaceMode.VisibleAlways;
        }

        public static void DoNppDocumentSwitched(bool initiating = false) {

            // update current file info
            IsPreviousFileProgress = IsCurrentFileProgress;
            IsCurrentFileProgress = Abl.IsCurrentProgressFile;
            CurrentFilePath = Npp.GetCurrentFilePath();
            CurrentFileObject = FilesInfo.GetFileInfo(CurrentFilePath);

            // accept advanced notifications only if the current file is a progress file
            CurrentFileAllowed = IsCurrentFileProgress;

            // update current scintilla
            Npp.UpdateScintilla();

            // Apply options to npp and scintilla depending if we are on a progress file or not
            ApplyOptionsForScintilla();

            // close popups..
            ClosePopups();

            // Update info on the current file
            FilesInfo.UpdateErrorsInScintilla();

            // refresh file explorer currently opened file
            FileExplorer.RedrawFileExplorerList();

            if (!initiating) {
                if (Config.Instance.CodeExplorerAutoHideOnNonProgressFile) {
                    CodeExplorer.Toggle(IsCurrentFileProgress);
                }
                if (Config.Instance.FileExplorerAutoHideOnNonProgressFile) {
                    FileExplorer.Toggle(IsCurrentFileProgress);
                }
            } else {
                // make sure to use the ProEnvironment and colorize the error counter
                FilesInfo.UpdateFileStatus();
                ProEnvironment.Current.ReComputeProPath();
            }

            if (IsCurrentFileProgress) {
               
                // Need to compute the propath again, because we take into account relative path
                ProEnvironment.Current.ReComputeProPath();

                // rebuild lines info (MANDATORY)
                Npp.RebuildLinesInfo();
            }

            // Parse the document
            ParserHandler.ParseCurrentDocument(true);

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

            // the user can open a .txt and save it as a .p
            DoNppDocumentSwitched();

            // if it's a conf file, import it
            ShareExportConf.TryToImportFile(CurrentFilePath);
        }

        #endregion

        #region OnNppFileBeforeLoad

        /// <summary>
        /// Called when a new file is being opened in notepad++
        /// </summary>
        private static void DoNppFileBeforeLoad() {
            // assume the file is not a progress file
            CurrentFileAllowed = false;

            // Reset the scintilla option for the indentation mode, as crazy as this is, it DESTROYS the performances
            // when opening big files in scintilla...
            Npp.AnnotationVisible = AnnotationMode;
        }

        #endregion

        #endregion

        #region Called when CurrentFileAllowed

        #region On char typed

        /// <summary>
        /// Called when the user enters any character in npp
        /// </summary>
        /// <param name="c"></param>
        public static void OnSciCharTyped(char c) {

            // CTRL + S : char code 19
            if (c == (char) 19) {
                Npp.Undo();
                Npp.SaveCurrentDocument();
                return;
            }

            // we are still entering a keyword
            if (Abl.IsCharAllowedInVariables(c)) {
                ActionsAfterUpdateUi.Enqueue(() => {
                    OnCharAddedWordContinue(c);
                });
            } else {
                ActionsAfterUpdateUi.Enqueue(() => {
                    OnCharAddedWordEnd(c);
                });
            }
        }

        /// <summary>
        /// Called when the user is still typing a word
        /// Called after the UI has updated, allows to correctly read the text style, to correct 
        /// the indentation w/o it being erased and so on...
        /// </summary>
        /// <param name="c"></param>
        public static void OnCharAddedWordContinue(char c) {
            try {
                // handles the autocompletion
                AutoComplete.UpdateAutocompletion();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCharAddedWordContinue");
            }
        }

        /// <summary>
        /// Called when the user has finished entering a word
        /// Called after the UI has updated, allows to correctly read the text style, to correct 
        /// the indentation w/o it being erased and so on...
        /// </summary>
        /// <param name="c"></param>
        public static void OnCharAddedWordEnd(char c) {
            try {
                // we finished entering a keyword
                var curPos = Npp.CurrentPosition;
                int offset;
                if (c == '\n') {
                    offset = curPos - Npp.GetLine().Position;
                    offset += (Npp.GetTextOnLeftOfPos(curPos - offset, 2).Equals("\r\n")) ? 2 : 1;
                } else
                    offset = 1;

                var searchWordAt = curPos - offset;
                var keyword = Npp.GetKeyword(searchWordAt);
                var isNormalContext = Style.IsCarretInNormalContext(searchWordAt);

                if (!String.IsNullOrWhiteSpace(keyword) && isNormalContext && AutoComplete.IsVisible) {
                    string replacementWord = null;

                    // automatically insert selected keyword of the completion list
                    if (Config.Instance.AutoCompleteInsertSelectedSuggestionOnWordEnd && keyword.ContainsAtLeastOneLetter()) {
                        if (AutoComplete.IsVisible) {
                            var lastSugg = AutoComplete.GetCurrentSuggestion();
                            if (lastSugg != null)
                                replacementWord = lastSugg.DisplayText;
                        }
                    }

                    // replace abbreviation by completekeyword
                    if (Config.Instance.CodeReplaceAbbreviations) {
                        var fullKeyword = Keywords.GetFullKeyword(replacementWord ?? keyword);
                        if (fullKeyword != null)
                            replacementWord = fullKeyword;
                    }

                    // replace the last keyword by the correct case
                    if (replacementWord == null) {
                        var casedKeyword = AutoComplete.CorrectKeywordCase(keyword, searchWordAt);
                        if (casedKeyword != null)
                            replacementWord = casedKeyword;
                    }

                    if (replacementWord != null)
                        Npp.ReplaceKeywordWrapped(replacementWord, -offset);
                }


                // replace semicolon by a point
                if (c == ';' && Config.Instance.CodeReplaceSemicolon && isNormalContext)
                    Npp.ModifyTextAroundCaret(-1, 0, ".");

                // handles the autocompletion
                AutoComplete.UpdateAutocompletion();

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCharAddedWordEnd");
            }
        }

        #endregion

        #region OnSciUpdateUi

        public static void OnSciUpdateUi(SCNotification nc) {
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
        }

        #endregion

        #region OnSciModified

        public static void OnSciModified(SCNotification nc) {
            bool deletedText = (nc.modificationType & (int) SciMsg.SC_MOD_DELETETEXT) != 0;

            // if the text has changed
            if (deletedText || (nc.modificationType & (int) SciMsg.SC_MOD_INSERTTEXT) != 0) {

                // observe modifications to lines (MANDATORY)
                Npp.UpdateLinesInfo(nc, !deletedText);

                // parse
                ParserHandler.ParseCurrentDocument();
            }

            // did the user supress 1 char?
            if (deletedText && nc.length == 1) {
                AutoComplete.UpdateAutocompletion();
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// When the user click on the margin
        /// </summary>
        public static void OnSciMarginClick(SCNotification nc) {

            // click on the error margin
            if (nc.margin == FilesInfo.ErrorMarginNumber) {
                // if it's an error symbol that has been clicked, the error on the line will be cleared
                if (!FilesInfo.ClearLineErrors(Npp.LineFromPosition(nc.position))) {
                    // if nothing has been cleared, we go to the next error position
                    FilesInfo.GoToNextError(Npp.LineFromPosition(nc.position));
                }
            }
        }

        /// <summary>
        /// When the user leaves his cursor inactive on npp
        /// </summary>
        public static void OnSciDwellStart() {
            if (WinApi.GetForegroundWindow() == Npp.HandleNpp)
                InfoToolTip.ShowToolTipFromDwell();
        }

        /// <summary>
        /// When the user moves his cursor
        /// </summary>
        public static void OnSciDwellEnd() {
            if (!KeyboardMonitor.GetModifiers.IsCtrl)
                InfoToolTip.Close(true);
        }

        /// <summary>
        /// called when the user changes its selection in npp (the carret moves)
        /// </summary>
        public static void OnUpdateSelection() {
            Npp.UpdateScintilla();

            // close popup windows
            ClosePopups();
            Snippets.FinalizeCurrent();

            // update scope of code explorer (the selection img)
            CodeExplorer.RedrawCodeExplorerList();
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

            // check for block that are too long and display a warning
            if (Abl.IsCurrentFileFromAppBuilder && !CurrentFileObject.WarnedTooLong) {
                var warningMessage = new StringBuilder();
                var explorerItemsList = ParserHandler.ParserVisitor.ParsedExplorerItemsList;

                if (explorerItemsList != null) {
                    foreach (var codeExplorerItem in explorerItemsList.Where(codeExplorerItem => codeExplorerItem.Flag.HasFlag(CodeExplorerFlag.IsTooLong)))
                        warningMessage.AppendLine("<div><img src='IsTooLong'><img src='" + codeExplorerItem.Branch + "' style='padding-right: 10px'><a href='" + codeExplorerItem.GoToLine + "'>" + codeExplorerItem.DisplayText + "</a></div>");
                    if (warningMessage.Length > 0) {
                        warningMessage.Insert(0, "<h2>Friendly warning :</h2>It seems that your file can be opened in the appbuilder as a structured procedure, but i detected that one or several procedure/function blocks contains more than " + Config.Instance.GlobalMaxNbCharInBlock + " characters. A direct consequence is that you won't be able to open this file in the appbuilder, it will generate errors and it will be unreadable. Below is a list of incriminated blocks :<br><br>");
                        warningMessage.Append("<br><i>To prevent this, reduce the number of chararacters in the above blocks, deleting dead code and trimming spaces is a good place to start!</i>");
                        var curPath = CurrentFilePath;
                        UserCommunication.NotifyUnique("AppBuilderLimit", warningMessage.ToString(), MessageImg.MsgHighImportance, "File saved", "Appbuilder limitations", args => {
                            Npp.Goto(curPath, Int32.Parse(args.Link));
                            UserCommunication.CloseUniqueNotif("AppBuilderLimit");
                        }, 20);
                        CurrentFileObject.WarnedTooLong = true;
                    }
                }
            }

            // for debug purposes, check if the document can be parsed
            if (Config.IsDevelopper && ParserHandler.AblParser.ParserErrors.Count > 0) {
                UserCommunication.Notify("The parser found erros on this file:<br>" + ProCodeFormat.GetParserErrorDescription(), MessageImg.MsgInfo, "Parser message", "Errors found", 3);
            }

            // update function prototypes
            if (IsCurrentFileProgress)
                ProGenerateCode.UpdateFunctionPrototypesIfNeeded(true);
        }

        #endregion

        #endregion

        #region Apply Scintilla options

        public static Annotation AnnotationMode {
            get { return _annotationMode; }
            set {
                // we want to set to our value
                if (value == Annotation.Indented) {
                    Npp.AnnotationVisible = Annotation.Indented;
                } else {
                    _annotationMode = value;
                }
            }
        }
        private static Annotation _annotationMode = Annotation.Hidden;

        private static bool _indentWithTabs;
        private static int _tabWidth = -1;
        private static WhitespaceMode _whitespaceMode = WhitespaceMode.Invisible;

        private static int[] _initiatedScintilla = {0,0};
        private static bool _hasBeenInit;
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
                _whitespaceMode = Npp.ViewWhitespace;
                AnnotationMode = Npp.AnnotationVisible;
            }
            _hasBeenInit = true;

            // need to do this stuff uniquely for both scintilla
            var curScintilla = Npp.CurrentScintilla; // 0 or 1
            if (_initiatedScintilla[curScintilla] == 0) {
                // Extra settings at the start
                Npp.MouseDwellTime = Config.Instance.ToolTipmsBeforeShowing;
                Npp.EndAtLastLine = false;
                Npp.EventMask = (int) (SciMsg.SC_MOD_INSERTTEXT | SciMsg.SC_MOD_DELETETEXT | SciMsg.SC_PERFORMED_USER | SciMsg.SC_PERFORMED_UNDO | SciMsg.SC_PERFORMED_REDO);
                _initiatedScintilla[curScintilla] = 1;
            }

            Npp.TabWidth = Config.Instance.CodeTabSpaceNb;
            Npp.UseTabs = false;
            if (Config.Instance.CodeShowSpaces)
                Npp.ViewWhitespace = WhitespaceMode.VisibleAlways;

            // apply style
            Style.SetSyntaxStyles();
            var currentStyle = Style.Current;
            Npp.SetIndentGuideColor(currentStyle.WhiteSpace.BackColor, currentStyle.WhiteSpace.ForeColor);
            Npp.SetWhiteSpaceColor(true, Color.Transparent, currentStyle.WhiteSpace.ForeColor);
            Npp.SetSelectionColor(true, currentStyle.Selection.BackColor, Color.Transparent);
            Npp.CaretLineBackColor = currentStyle.CaretLine.BackColor;
            Npp.CaretColor = currentStyle.CaretColor.ForeColor;
            Npp.SetFoldMarginColors(true, currentStyle.FoldMargin.BackColor, currentStyle.FoldMargin.BackColor);
            Npp.SetFoldMarginMarkersColor(currentStyle.FoldMargin.ForeColor, currentStyle.FoldMargin.BackColor, currentStyle.FoldActiveMarker.ForeColor);

            // we want the default auto-completion to not show
            // we block on a scintilla level (pretty bad solution because it slows down npp on big documents)
            Npp.AutoCStops(@"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_");
            // and we also block it in Npp (pull request on going for v6.9.?)
            if (Config.IsDevelopper)
                WinApi.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETAUTOCOMPLETIONDISABLEDONCHARADDED, 0, 1);
        }

        internal static void ApplyDefaultOptionsForScintilla() {
            // nothing has been done yet, no need to reset anything! same if we already were on a non progress file
            if (!_hasBeenInit || !IsPreviousFileProgress)
                return;

            // apply default options
            Npp.TabWidth = _tabWidth;
            Npp.UseTabs = _indentWithTabs;
            Npp.AnnotationVisible = AnnotationMode;

            if (Npp.ViewWhitespace != WhitespaceMode.Invisible && !Npp.ViewEol)
                Npp.ViewWhitespace = _whitespaceMode;

            // apply default style...
            try {
                // read the config.xml to know which theme is in use
                var guiConfig = XDocument.Load(Config.FileNppConfigXml).Descendants("GUIConfig");
                string themeXmlPath;
                try {
                    // ReSharper disable once PossibleNullReferenceException
                   themeXmlPath = (string) (guiConfig as XElement[] ?? guiConfig.ToArray()).FirstOrDefault(x => x.Attribute("name").Value.Equals("stylerTheme")).Attribute("path");
                } catch (Exception) {
                    themeXmlPath = null;
                }
                if (string.IsNullOrEmpty(themeXmlPath) || !File.Exists(themeXmlPath))
                    themeXmlPath = Config.FileNppStylersXml;
                
                // read npp's stylers.xml file
                var widgetStyle = XDocument.Load(themeXmlPath).Descendants("WidgetStyle");
                var xElements = widgetStyle as XElement[] ?? widgetStyle.ToArray();
                var wsFore = GetColorInStylers(xElements, "White space symbol", "fgColor");
                Npp.SetIndentGuideColor(GetColorInStylers(xElements, "Indent guideline style", "bgColor"), GetColorInStylers(xElements, "Indent guideline style", "fgColor"));
                Npp.SetWhiteSpaceColor(true, Color.Transparent, wsFore);
                Npp.SetSelectionColor(true, GetColorInStylers(xElements, "Selected text colour", "bgColor"), Color.Transparent);
                Npp.CaretLineBackColor = GetColorInStylers(xElements, "Current line background colour", "bgColor");
                Npp.CaretColor = GetColorInStylers(xElements, "Caret colour", "fgColor");
                Npp.SetFoldMarginColors(true, GetColorInStylers(xElements, "Fold margin", "bgColor"), GetColorInStylers(xElements, "Fold margin", "fgColor"));
                Npp.SetFoldMarginMarkersColor(GetColorInStylers(xElements, "Fold", "fgColor"), GetColorInStylers(xElements, "Fold", "bgColor"), GetColorInStylers(xElements, "Fold active", "fgColor"));

            } catch (Exception e) {
                ErrorHandler.LogError(e);
                if (!_warnedAboutFailStylers) {
                    _warnedAboutFailStylers = true;
                    UserCommunication.Notify("Error while reading one of Notepad++ file :<div>" + Config.FileNppStylersXml.ToHtmlLink() + "</div>", MessageImg.MsgError, "Error reading stylers.xml", "Xml read error");
                }
            }
            
            // we wanted the default auto-completion to not show, but no more
            Npp.AutoCStops("");
            if (Config.IsDevelopper)
                WinApi.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETAUTOCOMPLETIONDISABLEDONCHARADDED, 0, 0);
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