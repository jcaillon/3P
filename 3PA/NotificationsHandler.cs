using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using _3PA.Html;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.CodeExplorer;
using _3PA.MainFeatures.FileExplorer;
using _3PA.MainFeatures.FilesInfoNs;
using _3PA.MainFeatures.InfoToolTip;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA {

    internal static partial class Plug {

        #region public

        /// <summary>
        /// this is a delegate to defined actions that must be taken after updating the ui
        /// </summary>
        public static Action ActionAfterUpdateUi { private get; set; }

        #endregion


        #region Npp notifications

        /// <summary>
        /// handles the notifications send by npp and scintilla to the plugin
        /// </summary>
        public static void OnNppNotification(SCNotification nc) {
            uint code = nc.nmhdr.code;

            #region Basic notifications

            switch (code) {
                case (uint) NppMsg.NPPN_TBMODIFICATION:
                    UnmanagedExports.FuncItems.RefreshItems();
                    InitToolbarImages();
                    return;

                case (uint) NppMsg.NPPN_READY:
                    // notify plugins that all the procedures of launchment of notepad++ are done
                    OnNppReady();
                    return;

                case (uint) NppMsg.NPPN_SHUTDOWN:
                    OnNppShutdown();
                    return;
            }

            #endregion

            // Only do stuff when the dll is fully loaded
            if (!PluginIsFullyLoaded) return;

            // the user changed the current document
            if (code == (uint) NppMsg.NPPN_BUFFERACTIVATED) {
                OnDocumentSwitched();
                return;
            }

            // only do extra stuff if we are in a progress file
            if (!IsCurrentFileProgress) return;

            #region extra

            switch (code) {
                case (uint) SciMsg.SCN_CHARADDED:
                    // called each time the user add a char in the current scintilla
                    OnCharTyped((char) nc.ch);
                    return;

                case (uint) SciMsg.SCN_UPDATEUI:
                    // we need to set the indentation when we received this notification, not before or it's overwritten
                    if (ActionAfterUpdateUi != null) {
                        ActionAfterUpdateUi();
                        ActionAfterUpdateUi = null;
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

                case (uint) SciMsg.SCN_MODIFIED:
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

                    // if (nc.linesAdded != 0)
                    //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_USER) != 0;
                    //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_UNDO) != 0;
                    //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_REDO) != 0;
                    return;

                case (uint) SciMsg.SCN_STYLENEEDED:
                    // if we use the contained lexer, we will receive this notification and we will have to style the text
                    //Style.Colorize(Npp.GetSylingNeededStartPos(), nc.position);
                    return;

                case (uint) SciMsg.SCN_MARGINCLICK:
                    // called each time the user click on a margin
                    // click on the error margin
                    if (nc.margin == FilesInfo.ErrorMarginNumber) {
                        // if it's an error symbol that has been clicked, the error on the line will be cleared
                        if (!FilesInfo.ClearLineErrors(Npp.LineFromPosition(nc.position))) {
                            // if nothing has been cleared, we go to the next error position
                            FilesInfo.GoToNextError(Npp.LineFromPosition(nc.position));
                        }
                    }
                    // can also use : modifiers, the appropriate combination of SCI_SHIFT, SCI_CTRL and SCI_ALT to indicate the keys that were held down at the time of the margin click.
                    return;

                case (uint) NppMsg.NPPN_FILEBEFOREOPEN:
                    // fire when a file is opened

                    return;

                case (uint) NppMsg.NPPN_SHORTCUTREMAPPED:
                    // notify plugins that plugin command shortcut is remapped
                    NppMenu.ShortcutsUpdated((int) nc.nmhdr.idFrom, (ShortcutKey) Marshal.PtrToStructure(nc.nmhdr.hwndFrom, typeof (ShortcutKey)));
                    return;

                case (uint) SciMsg.SCN_MODIFYATTEMPTRO:
                    // Code a checkout when trying to modify a read-only file

                    return;

                case (uint) SciMsg.SCN_DWELLSTART:
                    // when the user hover at a fixed position for too long
                    OnDwellStart();
                    return;

                case (uint) SciMsg.SCN_DWELLEND:
                    // when he moves his cursor
                    OnDwellEnd();
                    return;

                case (uint) NppMsg.NPPN_FILESAVED:
                    // on file saved
                    OnFileSaved();
                    return;
            }

            #endregion
        }

        #endregion


        #region WndProc notifications

        private static IntPtr OnWndProcMessage(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam) {
            switch (uMsg) {
                //case (uint)WinApi.WindowsMessage.WM_SIZE:
                case (uint) WinApi.WindowsMessage.WM_EXITSIZEMOVE:
                case (uint) WinApi.WindowsMessage.WM_MOVE:
                    if (FileExplorer.IsVisible)
                        FileExplorer.Form.RefreshPosAndLoc();
                    if (CodeExplorer.IsVisible)
                        CodeExplorer.Form.RefreshPosAndLoc();
                    break;
            }
            return WinApi.CallWindowProc(_oldWindowProc, hwnd, uMsg, wParam, lParam);
        }

        #endregion


        #region On mouse message

        private static void InstanceOnGetMouseMessage(WinApi.WindowsMessage message, MOUSEHOOKSTRUCT mouseStruct, out bool handled) {
            switch (message) {
                // middle click
                case WinApi.WindowsMessage.WM_MBUTTONDOWN:
                    Rectangle scintillaRectangle = Rectangle.Empty;
                    WinApi.GetWindowRect(Npp.HandleScintilla, ref scintillaRectangle);
                    if (scintillaRectangle.Contains(Cursor.Position))
                        ProCodeUtils.GoToDefinition(true);
                    handled = true;
                    return;
            }
            handled = false;
        }

        #endregion


        #region On key down

        /// <summary>
        /// Called when the user presses a key
        /// </summary>
        // ReSharper disable once RedundantAssignment
        public static void OnKeyPressed(Keys key, KeyModifiers keyModifiers, ref bool handled) {
            // if set to true, the keyinput is completly intercepted, otherwise npp sill does its stuff
            handled = false;

            try {
                // check if the user triggered a function for which we set a shortcut (internalShortcuts)
                Tuple<Action, int, string> commandUsed = null;
                foreach (var kpv in NppMenu.InternalShortCuts) {
                    if ((byte)key == kpv.Key._key &&
                        keyModifiers.IsCtrl == kpv.Key.IsCtrl &&
                        keyModifiers.IsShift == kpv.Key.IsShift &&
                        keyModifiers.IsAlt == kpv.Key.IsAlt) {
                        commandUsed = kpv.Value;
                    }
                }

                // For the main window, we make an exception because we want to display no matter what
                if (commandUsed != null && commandUsed.Item3.Equals("Open_main_window")) {
                    if (Utils.IsSpamming("Open_main_window", 100))
                        return;
                    commandUsed.Item1();
                    handled = true;
                    return;
                }

                // Ok so... when we open a form in notepad++, we can't use the overrides PreviewKeyDown / KeyDown
                // like we normally can, for some reasons, they don't react to certain keys (like enter!)
                // It only works "almost normally" if we ShowDialog() the form?! Wtf right?
                // So i gave up and handle things here!
                if (Appli.IsFocused()) {
                    handled = Appli.Form.HandleKeyPressed(key, keyModifiers);
                }

                // check if allowed to execute
                if (!AllowFeatureExecution(0))
                    return;

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

                // we matched a shortcut, execute it
                if (commandUsed != null) {
                    commandUsed.Item1();
                    handled = true;
                }

            } catch (Exception e) {
                var shortcutName = "Instance_KeyDown";
                try {
                    foreach (var shortcut in NppMenu.InternalShortCuts.Keys.Where(shortcut => (byte)key == shortcut._key && keyModifiers.IsCtrl == shortcut.IsCtrl && keyModifiers.IsShift == shortcut.IsShift && keyModifiers.IsAlt == shortcut.IsAlt)) {
                        shortcutName = NppMenu.InternalShortCuts[shortcut].Item3;
                    }
                } catch (Exception x) {
                    ErrorHandler.DirtyLog(x);
                    // ignored, can't do much more
                }
                ErrorHandler.ShowErrors(e, "Error in " + shortcutName);
            }
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
                ActionAfterUpdateUi = () => {
                    OnCharAddedWordContinue(c);
                };
            } else {
                ActionAfterUpdateUi = () => {
                    OnCharAddedWordEnd(c);
                };
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
            ErrorHandler.Log("changing docu " + initiating.ToString());
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

            if (IsCurrentFileProgress) {
                // Syntax Style
                Style.SetSyntaxStyles();
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
        }

        #endregion


        #region On misc

        /// <summary>
        /// When the user leaves his cursor inactive on npp
        /// </summary>
        public static void OnDwellStart() {
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

            // close suggestions
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
                foreach (var codeExplorerItem in ParserHandler.GetParsedExplorerItemsList().Where(codeExplorerItem => codeExplorerItem.Flag.HasFlag(CodeExplorerFlag.IsTooLong)))
                    warningMessage.AppendLine("<div><img src='IsTooLong'><img src='" + codeExplorerItem.Branch + "' style='padding-right: 10px'><a href='" + codeExplorerItem.GoToLine + "'>" + codeExplorerItem.DisplayText + "</a></div>");
                if (warningMessage.Length > 0) {
                    warningMessage.Insert(0, "<h2>Friendly warning :</h2>It seems that your file can be opened in the appbuilder as a structured procedure, but i detected that one or several procedure/function blocks contains more than " + Config.Instance.GlobalMaxNbCharInBlock + " characters. A direct consequence is that you won't be able to open this file in the appbuilder, it will generate errors and it will be unreadable. Below is a list of incriminated blocks :<br><br>");
                    warningMessage.Append("<br><i>To prevent this, reduce the number of chararacters in the above blocks, deleting dead code and trimming spaces is a good place to start!</i>");
                    var curPath = CurrentFilePath;
                    UserCommunication.Notify(warningMessage.ToString(), MessageImg.MsgHighImportance, "File saved", "Appbuilder limitations", args => {
                        Npp.Goto(curPath, int.Parse(args.Link));
                    }, 20);
                    CurrentFileObject.WarnedTooLong = true;
                }
            }
        }

        #endregion


    }
}
