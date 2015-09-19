using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using YamuiFramework.Themes;
using _3PA.Html;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.DockableExplorer;
using _3PA.MainFeatures.InfoToolTip;
using _3PA.MainFeatures.SynthaxHighlighting;
using _3PA.Properties;

#pragma warning disable 1591

namespace _3PA {

    public class Plug {

        #region " Properties "
        public static string tempPath;

        public static bool PluginIsFullyLoaded;
        public static NppData NppData;
        public static FuncItems FuncItems = new FuncItems();

        private static bool _indentWithTabs;
        private static int _indentWidth;
        public static string CurrentFile { get; set; }

        public static Action ActionAfterUpdateUi { get; set; } // this is a delegate to defined actions that must be taken after updating the ui (example is indentation)
        #endregion

        #region " Startup/CleanUp "
        /// <summary>
        /// Called on notepad++ setinfo
        /// </summary>
        static internal void CommandMenuInit() {

            int cmdIndex = 0;
            var uniqueKeys = new Dictionary<Keys, int>();
            
            //                                                                      " name of the shortcut in config file : keys "
            Interop.Plug.SetCommand(cmdIndex++, "Show auto-complete suggestions", AutoComplete.ShowCompleteSuggestionList, "Show_Suggestion_List:Ctrl+Space", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "Show code snippet list", AutoComplete.ShowSnippetsList, "Show_SnippetsList:Ctrl+Shift+Space", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "Open main window", Appli.ToggleView, "Open_main_window:Alt+Space", false, uniqueKeys);

            Interop.Plug.SetCommand(cmdIndex++, "---", null);

            Interop.Plug.SetCommand(cmdIndex++, "Test", Test, "_Test:Ctrl+D", false, uniqueKeys);
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
            Interop.Plug.SetCommand(cmdIndex++, "Dockable explorer", DockableExplorer.Toggle);
            DockableExplorer.DockableCommandIndex = cmdIndex - 1;

            //NPP already intercepts these shortcuts so we need to hook keyboard messages
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
            KeyInterceptor.Instance.KeyDown += Instance_KeyDown;
        }
        
        /// <summary>
        /// display images in the npp toolbar
        /// </summary>
        static internal void InitToolbarImages() {
            Npp.SetToolbarImage(ImageResources._3PA, DockableExplorer.DockableCommandIndex);
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

        /// <summary>
        /// Called on npp ready
        /// </summary>
        static internal void OnNppReady() {
            // This allows to correctly feed the dll with dependencies
            LibLoader.Init();

            // registry : temp folder path
            tempPath = Path.Combine(Path.GetTempPath(), Resources.PluginFolderName);
            if (!Directory.Exists(tempPath)) {
                Directory.CreateDirectory(tempPath);
            }
            Registry.SetValue(Resources.RegistryPath, "tempPath", tempPath, RegistryValueKind.String);

            // registry : notepad executable path
            string pathNotepadFolder;
            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_GETNPPDIRECTORY, 0, out pathNotepadFolder);
            Registry.SetValue(Resources.RegistryPath, "notepadPath", Path.Combine(pathNotepadFolder, "notepad++.exe"), RegistryValueKind.String);

            Dispatcher.Init();

            ApplyPluginSpecificOptions(false);

            // initialize autocompletion (asynchrone)
            Task.Factory.StartNew(() => {
                try {
                    Snippets.Init();
                    Keywords.Init();
                    FileTags.Init();
                    DataBaseInfo.Init();
                    Config.Save();
                    RegisterCssAndImages.Init();

                    // initialize the list of objects of the autocompletion form
                    AutoComplete.FillItems();

                    // themes
                    ThemeManager.CurrentThemeIdToUse = Config.Instance.ThemeId;
                    ThemeManager.AccentColor = Config.Instance.AccentColor;
                    ThemeManager.TabAnimationAllowed = Config.Instance.AppliAllowTabAnimation;
                    // TODO: delete when releasing! (we dont want the user to access those themes!)
                    ThemeManager.ThemeXmlPath = Path.Combine(Npp.GetConfigDir(), "Themes.xml");

                    // SCINTILLA
                    // set the timer of dwell time, if the user let the mouse inactive for this period of time, npp fires the dwellstart notif
                    Win32.SendMessage(Npp.HandleScintilla, SciMsg.SCI_SETMOUSEDWELLTIME, Config.Instance.ToolTipmsBeforeShowing, 0);
                    // Set a mask for notifications received
                    Win32.SendMessage(Npp.HandleScintilla, SciMsg.SCI_SETMODEVENTMASK,
                        SciMsg.SC_MOD_INSERTTEXT | SciMsg.SC_MOD_DELETETEXT | SciMsg.SC_PERFORMED_USER, 0);

                    // Simulates a OnDocumentSwitched when we start this dll
                    OnDocumentSwitched();
                } finally {
                    PluginIsFullyLoaded = true;
                }
            });
        }
        #endregion

        #region " events on CHARADD and INSTANCE.KEYDOWN "
        /// <summary>
        /// Called when the user enters any character in npp
        /// </summary>
        /// <param name="c"></param>
        static public void OnCharTyped(char c) {
            try {
                string newStr = c.ToString();
                Point keywordPos;
                string keyword;

                // we are currently entering a keyword
                if (new Regex(@"^[\w-&\{\}]$").Match(newStr).Success) {
                    AutoComplete.ActivatedAutoCompleteIfNeeded();

                // we finished entering a keyword
                } else {
                    AutoComplete.Close();
                    return;

                    int offset = (newStr.Equals("\n") && Npp.TextBeforeCaret(2).Equals("\r\n")) ? 2 : 1; 
                    int curPos = Npp.GetCaretPosition();
                    bool isNormalContext = Npp.IsNormalContext(curPos);

                    // show suggestions on fields
                    if (c == '.' && Config.Instance.AutoCompleteOnKeyInputShowSuggestions && DataBaseInfo.ContainsTable(Npp.GetCurrentTable()))
                        AutoComplete.ShowFieldsSuggestions(true);

                    // only do more stuff if we are not in a string/comment/include definition 
                    if (!isNormalContext) return;

                    keyword = Npp.GetKeywordOnLeftOfPosition(curPos - offset, out keywordPos);
                    bool lastWordInDico = AutoComplete.IsWordInSuggestionsList(keyword, curPos, offset);

                    // trigger snippet insertion on space if the setting is activated (and the leave)
                    Npp.SetStatusbarLabel(keyword + " " + Snippets.Contains(keyword));
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

                    // replace the last keyword by the correct case, check the context of the caret
                    if (Config.Instance.AutoCompleteChangeCaseMode != 0 && !string.IsNullOrWhiteSpace(keyword) && lastWordInDico)
                        Npp.WrappedKeywordReplace(Npp.AutoCaseToUserLiking(keyword), keywordPos, curPos);

                    // replace semicolon by a point
                    if (c == ';' && Config.Instance.AutoCompleteReplaceSemicolon && lastWordInDico)
                        Npp.WrappedKeywordReplace(".", new Point(curPos - 1, curPos), curPos);

                    // on DO: add an END
                    if (c == ':' && Config.Instance.AutoCompleteInsertEndAfterDo && (keyword.EqualsCi("do") || Npp.GetKeyword(curPos - offset - 1).EqualsCi("do"))) {
                        int nbPrevInd = Npp.GetLineIndent(Npp.GetLineNumber(curPos));
                        string repStr = new String(' ', nbPrevInd);
                        repStr = "\r\n" + repStr + new String(' ', Config.Instance.AutoCompleteIndentNbSpaces) + "\r\n" + repStr + Npp.AutoCaseToUserLiking("END.");
                        Npp.WrappedKeywordReplace(repStr, new Point(curPos, curPos), curPos + 2 + nbPrevInd + Config.Instance.AutoCompleteIndentNbSpaces);
                    }

                    // handle indentation
                    if (newStr.Equals("\n")) {
                        // indent once after then
                        if (keyword.EqualsCi("then"))
                            ActionAfterUpdateUi = () => {
                                Npp.SetCurrentLineRelativeIndent(Config.Instance.AutoCompleteIndentNbSpaces);
                            };

                        // add dot atfer an end
                        if (keyword.EqualsCi("end")) {
                            Npp.WrappedKeywordReplace(Npp.AutoCaseToUserLiking("END."), keywordPos, curPos + 1);
                            Npp.SetPreviousLineRelativeIndent(-Config.Instance.AutoCompleteIndentNbSpaces);
                            ActionAfterUpdateUi = () => {
                                Npp.SetCurrentLineRelativeIndent(0);
                            };
                        }
                    }

                    if (c == '.' && (keyword.EqualsCi("end"))) {
                        Npp.AddTextAtCaret("\r\n");
                        Npp.SetPreviousLineRelativeIndent(-Config.Instance.AutoCompleteIndentNbSpaces);
                        ActionAfterUpdateUi = () => {
                            Npp.SetCurrentLineRelativeIndent(0);
                        };
                    }

                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCharTyped");
            }
        }

        /// <summary>
        /// Called when the user presses a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="repeatCount"></param>
        /// <param name="handled"></param>
        // ReSharper disable once RedundantAssignment
        static void Instance_KeyDown(Keys key, int repeatCount, ref bool handled) {
            // if set to true, the keyinput is completly intercepted, otherwise npp sill do its stuff
            handled = false; 

            // only do stuff if we are in a progress file
            if (!Utils.IsCurrentProgressFile()) return;

            try {
                // Close interfacePopups
                if (key == Keys.PageDown || key == Keys.PageUp || key == Keys.Next || key == Keys.Prior) {
                    ClosePopups();
                }
                if (AutoComplete.IsVisible) {
                    if (key == Keys.Up || key == Keys.Down || key == Keys.Right || key == Keys.Left || key == Keys.Tab || key == Keys.Return || key == Keys.Escape) {
                        handled = AutoComplete.OnKeyDown(key);
                    }
                } else {
                    if (key == Keys.Tab || key == Keys.Escape || key == Keys.Return) {
                        Modifiers modifiers = KeyInterceptor.GetModifiers();
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


                // check if the user triggered a function for which we set a shortcut (internalShortcuts)
                foreach (var shortcut in Interop.Plug.InternalShortCuts.Keys) {
                    if ((byte) key == shortcut._key) {
                        Modifiers modifiers = KeyInterceptor.GetModifiers();
                        if (modifiers.IsCtrl == shortcut.IsCtrl && modifiers.IsShift == shortcut.IsShift &&
                            modifiers.IsAlt == shortcut.IsAlt) {
                            handled = true;
                            var shortcut1 = shortcut;
                            Interop.Plug.InternalShortCuts[shortcut1].Item1();
                            break;
                        }
                    }
                }

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in Instance_KeyDown");
            }
        }

        /// <summary>
        /// When the user leaves his cursor inactive on npp
        /// </summary>
        public static void OnDwellStart() {
            InfoToolTip.ShowToolTip(true);
        }

        /// <summary>
        /// When the user moves his cursor
        /// </summary>
        public static void OnDwellEnd() {
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
        }

        /// <summary>
        /// called when the user scrolls..
        /// </summary>
        public static void OnPageScrolled() {
            ClosePopups();
        }

        /// <summary>
        /// Called when a line is removed or added in the current doc
        /// </summary>
        public static void OnLineAddedOrRemoved() {
            if (!Npp.GetCurrentFile().Equals(CurrentFile) && Utils.IsCurrentProgressFile()) {
                CurrentFile = Npp.GetCurrentFile();
                //MessageBox.Show("time to parse this file");
            }
        }

        /// <summary>
        /// Called when the user switches tab document
        /// </summary>
        public static void OnDocumentSwitched() {
            Npp.UpdateScintilla();
            ApplyPluginSpecificOptions(false);
            ClosePopups();
            
            // TODO: FIX COLOR HIGHLIGHTING.?
            //// set the lexer to use
            //if (Config.Instance.GlobalUseContainedLexer && Utils.IsCurrentProgressFile())
            //    Highlight.Colorize(0, Npp.GetTextLenght());
            //    //Npp.SetLexerToContainerLexer();
        }

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
            if (!Utils.IsCurrentProgressFile() || forceToDefault) {
                Npp.ResetDefaultAutoCompletion();
                Npp.SetIndent(_indentWidth);
                Npp.SetUseTabs(_indentWithTabs);
            } else {
                Npp.HideDefaultAutoCompletion();
                Npp.SetIndent(Config.Instance.AutoCompleteIndentNbSpaces);
                Npp.SetUseTabs(false);
            }
        }
        #endregion

        #region public

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
            Highlight.SetCustomStyles();
            Npp.SetStatusbarLabel(Npp.GetStyleAt(Npp.GetCaretPosition()).ToString());
        }
        #endregion
    }
}