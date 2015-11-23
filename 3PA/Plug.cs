#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (Plug.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using _3PA.MainFeatures.FileInfo;
using _3PA.MainFeatures.InfoToolTip;
using _3PA.MainFeatures.SyntaxHighlighting;

namespace _3PA {

    public class Plug {

        #region Fields
        public static string TempPath;

        public static bool PluginIsFullyLoaded;
        public static NppData NppData;
        public static FuncItems FuncItems = new FuncItems();

        private static bool _indentWithTabs;
        private static int _indentWidth;

        /// <summary>
        /// this is a delegate to defined actions that must be taken after updating the ui (example is indentation)
        /// </summary>
        public static Action ActionAfterUpdateUi { get; set; }

        /// <summary>
        /// true if the current file is a progress file, false otherwise
        /// </summary>
        public static bool IsCurrentFileProgress { get; set; }
        #endregion

        #region Init and clean up
        /// <summary>
        /// Called on notepad++ setinfo
        /// </summary>
        static internal void CommandMenuInit() {

            int cmdIndex = 0;
            var uniqueKeys = new Dictionary<Keys, int>();
            
            //                                                                      " name of the shortcut in config file : keys "
            Interop.Plug.SetCommand(cmdIndex++, "Show auto-complete suggestions", AutoComplete.OnShowCompleteSuggestionList, "Show_Suggestion_List:Ctrl+Space", false, uniqueKeys);
            //Interop.Plug.SetCommand(cmdIndex++, "Show code snippet list", AutoComplete.ShowSnippetsList, "Show_SnippetsList:Ctrl+Shift+Space", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "Open main window", Appli.ToggleView, "Open_main_window:Alt+Space", false, uniqueKeys);

            Interop.Plug.SetCommand(cmdIndex++, "---", null);

            Interop.Plug.SetCommand(cmdIndex++, "Toggle comment", ProgressCodeUtils.ToggleComment, "Toggle_Comment:Ctrl+Q", false, uniqueKeys);

            Interop.Plug.SetCommand(cmdIndex++, "Test", Test, "Test:Ctrl+D", false, uniqueKeys);

            Interop.Plug.SetCommand(cmdIndex++, "Go to definition", ProgressCodeUtils.GoToDefinition, "Go_To_Definition:Ctrl+B", false, uniqueKeys);

            Interop.Plug.SetCommand(cmdIndex++, "Go backwards", ProgressCodeUtils.GoBackFromDefinition, "Go_Backwards:Ctrl+Shift+B", false, uniqueKeys);
            
            
            /*
            SetCommand(cmdIndex++, "---", null);

            SetCommand(cmdIndex++, "Open 4GL help", hello, "4GL_Help:F1", false, uniqueKeys);
            SetCommand(cmdIndex++, "Check synthax", hello, "4GL_Check_synthax:Shift+F1", false, uniqueKeys);
            SetCommand(cmdIndex++, "Compile", hello, "4GL_Compile:Alt+F1", false, uniqueKeys);
            SetCommand(cmdIndex++, "Run!", hello, "4GL_Run:Ctrl+F1", false, uniqueKeys);
            SetCommand(cmdIndex++, "Pro-lint", hello, "4GL_prolint:Ctrl+F12", false, uniqueKeys);
            SetCommand(cmdIndex++, "Code beautifier", hello);
             
            SetCommand(cmdIndex++, "---", null);

            SetCommand(cmdIndex++, "Go to selection definition", hello, "Go_to_definition:Ctrl+B", false, uniqueKeys);
            SetCommand(cmdIndex++, "Open .lst file", hello);
            SetCommand(cmdIndex++, "Open in app builder", hello, "Open_in_appbuilder:F12", false, uniqueKeys);

            SetCommand(cmdIndex++, "---", null);

            SetCommand(cmdIndex++, "Insert trace", hello, "Insert_trace:Ctrl+T", false, uniqueKeys);
            SetCommand(cmdIndex++, "Insert complete traces", hello, "Insert_complete_traces:Shift+Ctrl+T", false, uniqueKeys);
            SetCommand(cmdIndex++, "Edit file info", hello, "Edit_file_info:Ctrl+Shift+M", false, uniqueKeys);
            SetCommand(cmdIndex++, "Insert title block", hello, "Insert_title_block:Ctrl+Alt+M", false, uniqueKeys);
            SetCommand(cmdIndex++, "Surround with modif tags", hello, "Surround_with_tags:Ctrl+M", false, uniqueKeys);
            
            SetCommand(cmdIndex++, "---", null);

            SetCommand(cmdIndex++, "Settings", hello);
            SetCommand(cmdIndex++, "About", hello);

            SetCommand(cmdIndex++, "Dockable Dialog Demo", DockableDlgDemo);
            */

            Interop.Plug.SetCommand(cmdIndex++, "Toggle code explorer", CodeExplorer.Toggle);
            CodeExplorer.DockableCommandIndex = cmdIndex - 1;

            Interop.Plug.SetCommand(cmdIndex++, "Toggle file explorer", FileExplorer.Toggle);
            FileExplorer.DockableCommandIndex = cmdIndex - 1;

            // NPP already intercepts these shortcuts so we need to hook keyboard messages
            KeyInterceptor.Instance.Install();

            foreach (var key in uniqueKeys.Keys)
                KeyInterceptor.Instance.Add(key);

            KeyInterceptor.Instance.Add(Keys.Up);
            KeyInterceptor.Instance.Add(Keys.Down);
            KeyInterceptor.Instance.Add(Keys.Left);
            KeyInterceptor.Instance.Add(Keys.Right);
            KeyInterceptor.Instance.Add(Keys.Tab);
            KeyInterceptor.Instance.Add(Keys.Return);
            KeyInterceptor.Instance.Add(Keys.Escape);
            KeyInterceptor.Instance.Add(Keys.Back);
            KeyInterceptor.Instance.Add(Keys.PageDown);
            KeyInterceptor.Instance.Add(Keys.PageUp);
            KeyInterceptor.Instance.Add(Keys.Next);
            KeyInterceptor.Instance.Add(Keys.Prior);
            KeyInterceptor.Instance.KeyDown += OnKeyDown;
        }
        
        /// <summary>
        /// display images in the npp toolbar
        /// </summary>
        static internal void InitToolbarImages() {
            Npp.SetToolbarImage(ImageResources.logo16x16_r, FileExplorer.DockableCommandIndex);
            Npp.SetToolbarImage(ImageResources.logo16x16, CodeExplorer.DockableCommandIndex);
        }

        /// <summary>
        /// Called on Npp shutdown
        /// </summary>
        static internal void CleanUp() {
            try {
                // set options back to client's default
                ApplyPluginSpecificOptions(true);
                // save config (should be done but just in case)
                Config.Save();
                // remember the most used keywords
                Keywords.Save();
                // dispose of all popup
                ForceCloseAllWindows();
                PluginIsFullyLoaded = false;
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "CleanUp");
            }
        }

        #region init

        /// <summary>
        /// Called on npp ready
        /// </summary>
        internal static void OnNppReady() {

            // This allows to correctly feed the dll with dependencies
            LibLoader.Init();

            // catch unhandled errors
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += ErrorHandler.UnhandledErrorHandler;
            Application.ThreadException += ErrorHandler.ThreadErrorHandler;
            TaskScheduler.UnobservedTaskException += ErrorHandler.UnobservedErrorHandler;

            // initialize plugin (why another method for this? because otherwise the LibLoader can't do his job...)
            InitPlugin();
        }

        internal static void InitPlugin() {

            #region Themes

            // themes
            ThemeManager.CurrentThemeIdToUse = Config.Instance.ThemeId;
            ThemeManager.AccentColor = Config.Instance.AccentColor;
            ThemeManager.TabAnimationAllowed = Config.Instance.AppliAllowTabAnimation;
            // TODO: delete when releasing! (we dont want the user to access those themes!)
            ThemeManager.ThemeXmlPath = Path.Combine(Npp.GetConfigDir(), "Themes.xml");
            LocalHtmlHandler.Init();

            //Highlight.ThemeXmlPath = Path.Combine(Npp.GetConfigDir(), "SynthaxHighlighting.xml");
            #endregion

            // Init appli form, this gives us a Form to hook into if we want to do stuff on the UI thread
            // from a back groundthread, use : Appli.Form.BeginInvoke() for this
            Appli.Init();

            Task.Factory.StartNew(() => {

                //// registry : temp folder path
                //tempPath = Path.Combine(Path.GetTempPath(), Resources.PluginFolderName);
                //if (!Directory.Exists(tempPath)) {
                //    Directory.CreateDirectory(tempPath);
                //}
                //Registry.SetValue(Resources.RegistryPath, "tempPath", tempPath, RegistryValueKind.String);
                Snippets.Init();
                Keywords.Init();
                Config.Save();
                FileTags.Init();

                // initialize the list of objects of the autocompletion form
                AutoComplete.FillStaticItems(true);

                // SCINTILLA
                // set the timer of dwell time, if the user let the mouse inactive for this period of time, npp fires the dwellstart notif
                Win32.SendMessage(Npp.HandleScintilla, SciMsg.SCI_SETMOUSEDWELLTIME, Config.Instance.ToolTipmsBeforeShowing, 0);
                // Set a mask for notifications received
                Win32.SendMessage(Npp.HandleScintilla, SciMsg.SCI_SETMODEVENTMASK,
                    SciMsg.SC_MOD_INSERTTEXT | SciMsg.SC_MOD_DELETETEXT | SciMsg.SC_PERFORMED_USER | SciMsg.SC_PERFORMED_UNDO | SciMsg.SC_PERFORMED_REDO, 0);
                Npp.SetEndAtLastLine(false);
                Npp.SetWhiteSpaceView();

                // dockable explorer
                if (Config.Instance.CodeExplorerVisible && !CodeExplorer.IsVisible)
                    Appli.Form.BeginInvoke((Action) CodeExplorer.Toggle);

                // Simulates a OnDocumentSwitched when we start this dll
                OnDocumentSwitched();

                // Fetch the info from the database, when its done, it will update the parser, if it needs
                // to extract the db info (and since it takes a lot of time), it will parse immediatly instead
                Task.Factory.StartNew(DataBase.FetchCurrentDbInfo);

                //TODO: notification qui demande à l'utilisateur de désactiver l'autocompletion de base de npp
                // ask the user to deactivate the default autocompletion of npp
                if (Config.Instance.GlobalShowNotifAboutDefaultAutoComp)
                    UserCommunication.NotifyUserAboutNppDefaultAutoComp();

                // make sure the UDL is present
                Highlight.Init();

                PluginIsFullyLoaded = true;
            });
        }

        #endregion

        #endregion

        #region OnEvents
        /// <summary>
        /// Called when the user presses a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="repeatCount"></param>
        /// <param name="handled"></param>
        // ReSharper disable once RedundantAssignment
        static void OnKeyDown(Keys key, int repeatCount, ref bool handled) {
            // if set to true, the keyinput is completly intercepted, otherwise npp sill does its stuff
            handled = false;

            if (!PluginIsFullyLoaded) return;

            // only do stuff if we are in a progress file
            if (!IsCurrentFileProgress) return;

            try {
                Modifiers modifiers = KeyInterceptor.GetModifiers();
                // Close interfacePopups
                if (key == Keys.PageDown || key == Keys.PageUp || key == Keys.Next || key == Keys.Prior) {
                    ClosePopups();
                }
                // Autocompletion 
                if (AutoComplete.IsVisible) {
                    if (key == Keys.Up || key == Keys.Down || key == Keys.Tab || key == Keys.Return || key == Keys.Escape)
                        handled = AutoComplete.OnKeyDown(key);
                    else {
                        
                        if ((key == Keys.Right || key == Keys.Left) && modifiers.IsAlt)
                            handled = AutoComplete.OnKeyDown(key);
                    }
                } else {
                    // snippet ?
                    if (key == Keys.Tab || key == Keys.Escape || key == Keys.Return) {
                        if (!modifiers.IsCtrl && !modifiers.IsAlt && !modifiers.IsShift) {
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
                if (modifiers.IsCtrl && InfoToolTip.IsVisible && (key == Keys.Up || key == Keys.Down)) {
                    if (key == Keys.Up) 
                        InfoToolTip.IndexToShow++;
                    else 
                        InfoToolTip.IndexToShow--;
                    InfoToolTip.TryToShowIndex();
                    handled = true;
                }


                // check if the user triggered a function for which we set a shortcut (internalShortcuts)
                foreach (var shortcut in Interop.Plug.InternalShortCuts.Keys) {
                    if ((byte) key == shortcut._key 
                        && modifiers.IsCtrl == shortcut.IsCtrl 
                        && modifiers.IsShift == shortcut.IsShift 
                        && modifiers.IsAlt == shortcut.IsAlt) {
                        handled = true;
                        var shortcut1 = shortcut;
                        Interop.Plug.InternalShortCuts[shortcut1].Item1();
                        break;
                    }
                }

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in Instance_KeyDown");
            }
        }

        /// <summary>
        /// Called when the user enters any character in npp
        /// </summary>
        /// <param name="c"></param>
        static public void OnCharTyped(char c) {
            // CTRL + S : char code 19
            if (c == (char) 19) {
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
                // dont show in string/comments..?
                if (!Config.Instance.AutoCompleteShowInCommentsAndStrings && !Highlight.IsCarretInNormalContext(Npp.GetCaretPosition())) {
                    AutoComplete.Close();
                } else {
                    // handles the autocompletion
                    AutoComplete.UpdateAutocompletion();
                }
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
                var curPos = Npp.GetCaretPosition();
                int offset;
                if (c == '\n') {
                    offset = curPos - Npp.GetPositionFromLine(Npp.GetLineFromPosition(curPos));
                    offset += (Npp.GetTextOnLeftOfPos(curPos - offset, 2).Equals("\r\n")) ? 2 : 1;
                } else
                    offset = 1;
                var searchWordAt = curPos - offset;
                var keyword = Npp.GetKeyword(searchWordAt);
                var isNormalContext = Highlight.IsCarretInNormalContext(searchWordAt);

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
                    if (Config.Instance.AutocompleteReplaceAbbreviations) {
                        var fullKeyword = Keywords.GetFullKeyword(keyword);
                        if (fullKeyword != null)
                            replacementWord = fullKeyword;
                    }

                    // replace the last keyword by the correct case
                    if (replacementWord == null && Config.Instance.AutoCompleteChangeCaseMode != 0) {
                        var casedKeyword = AutoComplete.CorrectKeywordCase(keyword, searchWordAt);
                        if (casedKeyword != null)
                            replacementWord = casedKeyword;
                    }

                    if (replacementWord != null)
                        Npp.ReplaceKeywordWrapped(replacementWord, -offset);
                }


                // replace semicolon by a point
                if (c == ';' && Config.Instance.AutoCompleteReplaceSemicolon && isNormalContext) {
                    curPos = Npp.GetCaretPosition();
                    Npp.BeginUndoAction();
                    Npp.SetTextByRange(curPos - 1, curPos, ".");
                    Npp.SetCaretPosition(curPos);
                    Npp.EndUndoAction();
                }

                // handles the autocompletion
                AutoComplete.UpdateAutocompletion();
                
            /*
            // trigger snippet insertion on space if the setting is activated (and the leave)
            if (c == ' ' && Config.Instance.AutoCompleteUseSpaceToInsertSnippet &&
                Snippets.Contains(keyword)) {
                Npp.BeginUndoAction();
                Npp.ReplaceText(curPos - offset, curPos, "");
                Npp.SetCaretPosition(curPos - offset);
                Snippets.TriggerCodeSnippetInsertion();
                Npp.EndUndoAction();
                Npp.SetStatusbarLabel("trigger"); //TODO
                return;
            }
            */

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCharAddedWordEnd");
            }
        }

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
            Modifiers modifiers = KeyInterceptor.GetModifiers();
            if (!modifiers.IsCtrl)
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
            CodeExplorer.RedrawCodeExplorer();
        }

        /// <summary>
        /// called when the user scrolls..
        /// </summary>
        public static void OnPageScrolled() {
            ClosePopups();
        }

        /// <summary>
        /// Called when the user switches tab document
        /// </summary>
        public static void OnDocumentSwitched() {
            // update current file .extension check
            IsCurrentFileProgress = Abl.IsCurrentProgressFile();

            // update current scintilla
            Npp.UpdateScintilla();

            ApplyPluginSpecificOptions(false);

            // close popups..
            ClosePopups();

            // Parse the document
            if (PluginIsFullyLoaded)
                AutoComplete.ParseCurrentDocument(true);

            // Syntax Highlight
            if (IsCurrentFileProgress)
                Highlight.SetCustomStyles();
        }

        /// <summary>
        /// Called when the user saves the current document
        /// </summary>
        public static void OnFileSaved() {
            // check for block that are too long and display a warning
        }

        #endregion

        #region public

        /// <summary>
        /// We need certain options to be set to specific values when running this plugin, make sure to set everything back to normal
        /// when switch tab or when we leave npp, param can be set to true to force the defautl values
        /// </summary>
        /// <param name="forceToDefault"></param>
        public static void ApplyPluginSpecificOptions(bool forceToDefault) {
            if (_indentWidth == 0) {
                _indentWidth = Npp.GetIndent();
                _indentWithTabs = Npp.GetUseTabs();
            }
            if (!IsCurrentFileProgress || forceToDefault) {
                Npp.ResetDefaultAutoCompletion();
                Npp.SetIndent(_indentWidth);
                Npp.SetUseTabs(_indentWithTabs);
            } else {
                Npp.HideDefaultAutoCompletion();
                Npp.SetIndent(Config.Instance.AutoCompleteIndentNbSpaces);
                Npp.SetUseTabs(false);
            }
        }

        /// <summary>
        /// Call this method to close all popup/autocompletion form and alike
        /// </summary>
        public static void ClosePopups() {
            AutoComplete.Close();
            InfoToolTip.Close();
        }

        /// <summary>
        /// Call this method to force close all popup/autocompletion form and alike
        /// </summary>
        public static void ForceCloseAllWindows() {
            AutoComplete.ForceClose();
            InfoToolTip.ForceClose();
            Appli.ForceClose();
        }

        #endregion

        #region tests
        static void Test() {
            FileTags.UnCloak();


            //UserCommunication.Notify(Npp.GetStyleAt(Npp.GetCaretPosition()).ToString());
            //UserCommunication.MessageToUser();
            //var x = 0;
            //var y = 1/x;
            //Npp.Goto(@"C:\Users\Julien\Desktop\in.p", 1000, 10);
            //ProgressCodeUtils.ToggleComment();
            /*
            Task.Factory.StartNew(() => {
                Appli.Form.BeginInvoke((Action)delegate {
                    var toastNotification2 = new YamuiNotifications(Npp.IsDocumentSaved().ToString(), 5);
                    toastNotification2.Show();
                });
            });*/
            //Highlight.Colorize(0, Npp.GetTextLenght());
        }
        #endregion
    }
}