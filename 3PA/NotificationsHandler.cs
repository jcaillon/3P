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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Forms;
using YamuiFramework.Themes;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.FileExplorer;
using _3PA.MainFeatures.InfoToolTip;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecutionNs;
using MenuItem = _3PA.MainFeatures.MenuItem;

namespace _3PA {

    internal static partial class Plug {

        #region Static events

        // NOTE : be aware that if you subscribe to one of those events, a reference to the subscribing object is held by the publisher (this class). That means that you have to be very careful about explicitly unsubscribing from static events as they will keep the subscriber alive forever, i.e., you may end up with the managed equivalent of a memory leak.

        /// <summary>
        /// Published when Npp is shutting down, do your clean up actions
        /// </summary>
        public static event Action OnNppShutDown;

        /// <summary>
        /// Subscribe to this event, published when the current document in changed (on document open or tab switched)
        /// </summary>
        public static event Action OnDocumentChangedEnd;

        /// <summary>
        /// Published when the Npp windows is being moved
        /// </summary>
        public static event Action OnNppWindowsMove;

        #endregion


        #region Members

        /// <summary>
        /// this is a delegate to defined actions that must be taken after updating the ui
        /// </summary>
        public static Queue<Action> ActionsAfterUpdateUi = new Queue<Action>();

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
                        OnNppReady();
                        return;

                    case (uint) NppNotif.NPPN_SHUTDOWN:
                        if (OnNppShutDown != null)
                            OnNppShutDown();
                        return;
                }

                #endregion

                // Only do stuff when the dll is fully loaded
                if (!PluginIsFullyLoaded) return;

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

        #region On mouse message

        private static void OnMouseMessage(WinApi.WindowsMessageMouse message, WinApi.MOUSEHOOKSTRUCT mouseStruct, out bool handled) {
            handled = false;
            switch (message) {
                // middle click
                case WinApi.WindowsMessageMouse.WM_MBUTTONDOWN:
                    Rectangle scintillaRectangle = Rectangle.Empty;
                    WinApi.GetWindowRect(Npp.HandleScintilla, ref scintillaRectangle);
                    if (scintillaRectangle.Contains(Cursor.Position)) {
                        ProCodeUtils.GoToDefinition(true);
                        handled = true;
                        return;
                    }
                    break;
                // (CTRL + ) Right click : show main menu
                case WinApi.WindowsMessageMouse.WM_RBUTTONUP:
                    if (Config.Instance.AppliSimpleRightClickForMenu && !KeyboardMonitor.GetModifiers.IsCtrl ||
                        !Config.Instance.AppliSimpleRightClickForMenu && KeyboardMonitor.GetModifiers.IsCtrl) {
                        // we need the cursor to be in scintilla but not on the application or the auto-completion!
                        if (Npp.GetScintillaRectangle().Contains(Cursor.Position) && 
                            (!Appli.IsVisible || !Appli.IsMouseIn()) &&
                            (!InfoToolTip.IsVisible || !InfoToolTip.IsMouseIn()) &&
                            (!AutoComplete.IsVisible ||  !AutoComplete.IsMouseIn())) {
                            AppliMenu.ShowMainMenuAtCursor();
                            handled = true;
                            return;
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
                    if (MouseMonitor.Instance.Remove(WinApi.WindowsMessageMouse.WM_MOUSEMOVE) && OnNppWindowsMove != null) {
                        OnNppWindowsMove();
                    }
                    break;
                case WinApi.WindowsMessageMouse.WM_MOUSEMOVE:
                    if (OnNppWindowsMove != null) {
                        OnNppWindowsMove();
                    }
                    break;
            }
            
        }

        #endregion

        #region On key down

        /// <summary>
        /// Called when the user presses a key
        /// </summary>
        // ReSharper disable once RedundantAssignment
        private static void OnKeyDown(Keys key, KeyModifiers keyModifiers, ref bool handled) {
            // if set to true, the keyinput is completly intercepted, otherwise npp sill does its stuff
            handled = false;

            MenuItem menuItem = null;
            try {
                // Since it's a keydown message, we can receive this a lot if the user let a button pressed
                var isSpamming = Utils.IsSpamming(key.ToString(), 100, true);

                //HACK:
                // Ok so... when we open a form in notepad++, we can't use the overrides PreviewKeyDown / KeyDown
                // like we normally can, for some reasons, they don't react to certain keys (like enter!)
                // It only works "almost normally" if we ShowDialog() the form?! Wtf right?
                // So i gave up and handle things here!
                if (Appli.IsFocused()) {
                    handled = Appli.Form.HandleKeyPressed(key, keyModifiers);
                } else {
                    // same shit for the YamuiMenu
                    var curMenu = (Control.FromHandle(WinApi.GetForegroundWindow()));
                    var menu = curMenu as YamuiMenu;
                    if (menu != null) {
                        menu.OnKeyDown(key);
                    }
                }

                // check if the user triggered a 3P function defined in the AppliMenu
                menuItem = TriggeredMenuItem(AppliMenu.Instance.ShortcutableItemList, isSpamming, key, keyModifiers, ref handled);
                if (handled) {
                    return;
                }

                // The following is specific to 3P so don't go further if we are not on a valid file
                if (!IsCurrentFileProgress) {
                    return;
                }

                // Close interfacePopups
                if (key == Keys.PageDown || key == Keys.PageUp || key == Keys.Next || key == Keys.Prior) {
                    ClosePopups();
                }

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
                if (keyModifiers.IsCtrl && InfoToolTip.IsVisible && (key == Keys.Up || key == Keys.Down)) {
                    if (key == Keys.Up)
                        InfoToolTip.IndexToShow--;
                    else
                        InfoToolTip.IndexToShow++;
                    InfoToolTip.TryToShowIndex();
                    handled = true;
                }

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Occured in : " + (menuItem == null ? (new ShortcutKey(keyModifiers.IsCtrl, keyModifiers.IsAlt, keyModifiers.IsShift, key)).ToString() : menuItem.ItemId));
            }
        }

        /// <summary>
        /// Check if the key/keymodifiers correspond to a item in the menu, if yes, returns this item and execute .Do()
        /// </summary>
        private static MenuItem TriggeredMenuItem(List<MenuItem> list, bool isSpamming, Keys key, KeyModifiers keyModifiers, ref bool handled) {

            // check if the user triggered a 3P function defined in the AppliMenu
            foreach (var item in list) {
                // shortcut corresponds to the item?
                if ((byte)key == item.Shortcut._key &&
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

        #region On char typed

        /// <summary>
        /// Called when the user enters any character in npp
        /// </summary>
        /// <param name="c"></param>
        public static void OnCharTyped(char c) {

            // CTRL + S : char code 19
            if (c == (char)19) {
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

                if (!string.IsNullOrWhiteSpace(keyword) && isNormalContext) {
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
                    if (Config.Instance.CodeChangeCaseMode != 0) {
                        var casedKeyword = AutoComplete.CorrectKeywordCase(replacementWord ?? keyword, searchWordAt);
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


        #region On document switch

        /// <summary>
        /// Called when the user switches tab document
        /// </summary>
        public static void OnDocumentSwitched(bool initiating = false) {

            // update current file info
            IsCurrentFileProgress = Abl.IsCurrentProgressFile();
            CurrentFilePath = Npp.GetCurrentFilePath();
            CurrentFileObject = FilesInfo.GetFileInfo(CurrentFilePath);

            // update current scintilla
            Npp.UpdateScintilla();

            // rebuild lines info
            Npp.RebuildLinesInfo();

            // close popups..
            ClosePopups();

            if (IsCurrentFileProgress && Config.Instance.UseSyntaxHighlightTheme) {
                // Syntax Style
                Style.SetSyntaxStyles();
            } else {
                Style.ResetSyntaxStyles();
            }

            // set general styles (useful for the file explorer > current status)
            Style.SetGeneralStyles();

            // Update info on the current file
            FilesInfo.UpdateErrorsInScintilla();

            // Need to compute the propath again, because we take into account relative path
            ProEnvironment.Current.ReComputeProPath();

            if (!initiating) {
                if (Config.Instance.CodeExplorerAutoHideOnNonProgressFile) {
                    CodeExplorer.Toggle(IsCurrentFileProgress);
                }
                if (Config.Instance.FileExplorerAutoHideOnNonProgressFile) {
                    FileExplorer.Toggle(IsCurrentFileProgress);
                }
            }

            // Apply options to npp and scintilla depending if we are on a progress file or not
            ApplyPluginSpecificOptions(false);

            // refresh file explorer currently opened file
            FileExplorer.RedrawFileExplorerList();

            // Parse the document
            if (PluginIsFullyLoaded)
                AutoComplete.ParseCurrentDocument(true);

            // publish an event
            if (OnDocumentChangedEnd != null) {
                OnDocumentChangedEnd();
            }
        }

        #endregion


        #region On misc

        /// <summary>
        /// When the user leaves his cursor inactive on npp
        /// </summary>
        public static void OnDwellStart() {
            if (WinApi.GetForegroundWindow() == Npp.HandleNpp)
                InfoToolTip.ShowToolTipFromDwell();
        }

        /// <summary>
        /// When the user moves his cursor
        /// </summary>
        public static void OnDwellEnd() {
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
        /// Called when the user saves the current document
        /// </summary>
        public static void OnFileSaved() {
    
            // check for block that are too long and display a warning
            if (Abl.IsCurrentFileFromAppBuilder() && !CurrentFileObject.WarnedTooLong) {
                var warningMessage = new StringBuilder();
                var explorerItemsList = ParserHandler.GetParsedExplorerItemsList();

                if (explorerItemsList != null) {
                    foreach (var codeExplorerItem in explorerItemsList.Where(codeExplorerItem => codeExplorerItem.Flag.HasFlag(CodeExplorerFlag.IsTooLong)))
                        warningMessage.AppendLine("<div><img src='IsTooLong'><img src='" + codeExplorerItem.Branch + "' style='padding-right: 10px'><a href='" + codeExplorerItem.GoToLine + "'>" + codeExplorerItem.DisplayText + "</a></div>");
                    if (warningMessage.Length > 0) {
                        warningMessage.Insert(0, "<h2>Friendly warning :</h2>It seems that your file can be opened in the appbuilder as a structured procedure, but i detected that one or several procedure/function blocks contains more than " + Config.Instance.GlobalMaxNbCharInBlock + " characters. A direct consequence is that you won't be able to open this file in the appbuilder, it will generate errors and it will be unreadable. Below is a list of incriminated blocks :<br><br>");
                        warningMessage.Append("<br><i>To prevent this, reduce the number of chararacters in the above blocks, deleting dead code and trimming spaces is a good place to start!</i>");
                        var curPath = CurrentFilePath;
                        UserCommunication.NotifyUnique("AppBuilderLimit", warningMessage.ToString(), MessageImg.MsgHighImportance, "File saved", "Appbuilder limitations", args => {
                            Npp.Goto(curPath, int.Parse(args.Link));
                            UserCommunication.CloseUniqueNotif("AppBuilderLimit");
                        }, 20);
                        CurrentFileObject.WarnedTooLong = true;
                    }
                }
            }
        }

        #endregion
    }
}
