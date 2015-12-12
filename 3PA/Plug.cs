#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
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
using _3PA.MainFeatures.FilesInfo;
using _3PA.MainFeatures.InfoToolTip;
using _3PA.MainFeatures.ProgressExecution;
using _3PA.MainFeatures.SyntaxHighlighting;

namespace _3PA {

    public class Plug {

        #region Fields
        public static bool PluginIsFullyLoaded;
        public static NppData NppData;
        public static FuncItems FuncItems = new FuncItems();

        /// <summary>
        /// this is a delegate to defined actions that must be taken after updating the ui (example is indentation)
        /// </summary>
        public static Action ActionAfterUpdateUi { get; set; }

        /// <summary>
        /// true if the current file is a progress file, false otherwise
        /// </summary>
        public static bool IsCurrentFileProgress { get; set; }

        /// <summary>
        /// Gets the temporary directory to use
        /// </summary>
        public static string TempDir {
            get {
                var dir = Path.Combine(Path.GetTempPath(), AssemblyInfo.ProductTitle);
                if (!Directory.Exists(dir))
                    try {
                        Directory.CreateDirectory(dir);
                    } catch (Exception e) {
                        ErrorHandler.ShowErrors(e, "Permission denied when creating " + dir);
                    }
                return dir;
            }
        }
        #endregion

        #region Init and clean up
        /// <summary>
        /// Called on notepad++ setinfo
        /// </summary>
        static internal void CommandMenuInit() {

            int cmdIndex = 0;
            var uniqueKeys = new Dictionary<Keys, int>();

            Interop.Plug.SetCommand(cmdIndex++, "Show auto-complete suggestions", AutoComplete.OnShowCompleteSuggestionList, "Show_Suggestion_List:Ctrl+Space", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "Open main window", Appli.ToggleView, "Open_main_window:Alt+Space", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "---", null);
            Interop.Plug.SetCommand(cmdIndex++, "Toggle comment", ProgressCodeUtils.ToggleComment, "Toggle_Comment:Ctrl+Q", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "Test", Test, "Test:Ctrl+D", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "Go to definition", ProgressCodeUtils.GoToDefinition, "Go_To_Definition:Ctrl+B", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "Go backwards", Npp.GoBackFromDefinition, "Go_Backwards:Ctrl+Shift+B", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "About", Appli.GoToAboutPage);

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

            // Npp already intercepts these shortcuts so we need to hook keyboard messages
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
                CodeExplorer.UpdateMenuItemChecked();
                Config.Save();

                // remember the most used keywords
                Keywords.Save();

                // dispose of all popup
                ForceCloseAllWindows();
                PluginIsFullyLoaded = false;

                // runs exit program if any
                UpdateHandler.OnNotepadExit();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "CleanUp");
            }
        }

        #region init

        /// <summary>
        /// Called on npp ready
        /// </summary>
        internal static void OnNppReady() {
            try {
                // This allows to correctly feed the dll with dependencies
                LibLoader.Init();

                // catch unhandled errors
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += ErrorHandler.UnhandledErrorHandler;
                Application.ThreadException += ErrorHandler.ThreadErrorHandler;
                TaskScheduler.UnobservedTaskException += ErrorHandler.UnobservedErrorHandler;

                // initialize plugin (why another method for this? because otherwise the LibLoader can't do his job...)
                InitPlugin();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "OnNppReady");
            }
        }

        internal static void InitPlugin() {

            #region Themes

            // themes
            ThemeManager.CurrentThemeIdToUse = Config.Instance.ThemeId;
            ThemeManager.AccentColor = Config.Instance.AccentColor;
            ThemeManager.TabAnimationAllowed = Config.Instance.AppliAllowTabAnimation;
            ThemeManager.ThemeXmlPath = Path.Combine(Npp.GetConfigDir(), "Themes.xml");
            Highlight.ThemeXmlPath = Path.Combine(Npp.GetConfigDir(), "SyntaxHighlight.xml");
            LocalHtmlHandler.Init();

            #endregion

            // Init appli form, this gives us a Form to hook onto if we want to do stuff on the UI thread
            // from a back groundthread, use : Appli.Form.BeginInvoke() for this
            Appli.Init();

            // code explorer
            if (Config.Instance.CodeExplorerVisible && !CodeExplorer.IsVisible)
                CodeExplorer.Toggle();

            // Try to update 3P
            UpdateHandler.OnNotepadStart();

            //Task.Factory.StartNew(() => {

            Snippets.Init();
            Keywords.Init();
            Config.Save();
            FileTags.Init();
            DataBase.Init();

            // initialize the list of objects of the autocompletion form
            AutoComplete.FillStaticItems(true);

            // make sure the UDL is present, also display the welcome message
            Highlight.CheckUdl();

            PluginIsFullyLoaded = true;

            // Simulates a OnDocumentSwitched when we start this dll
            OnDocumentSwitched();

            //});
        }

        #endregion

        #endregion

        #region public OnEvents

        #region on key down

        /// <summary>
        /// Called when the user presses a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="repeatCount"></param>
        /// <param name="handled"></param>
        // ReSharper disable once RedundantAssignment
        public static void OnKeyDown(Keys key, int repeatCount, ref bool handled) {
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
                        InfoToolTip.IndexToShow--;
                    else
                        InfoToolTip.IndexToShow++;
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

        #endregion

        #region on char typed

        /// <summary>
        /// Called when the user enters any character in npp
        /// </summary>
        /// <param name="c"></param>
        public static void OnCharTyped(char c) {
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
                if (!Config.Instance.AutoCompleteShowInCommentsAndStrings && !Highlight.IsCarretInNormalContext(Npp.CurrentPosition)) {
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
                var curPos = Npp.CurrentPosition;
                int offset;
                if (c == '\n') {
                    offset = curPos - Npp.GetLine().Position;
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
                if (c == ';' && Config.Instance.AutoCompleteReplaceSemicolon && isNormalContext)
                    Npp.ModifyTextAroundCaret(-1, 0, ".");

                // handles the autocompletion
                AutoComplete.UpdateAutocompletion();

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in OnCharAddedWordEnd");
            }
        }

        #endregion

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

            // rebuild lines info
            Npp.RebuildLinesInfo();

            // close popups..
            ClosePopups();

            if (IsCurrentFileProgress) {
                // Syntax Highlight
                Highlight.SetCustomStyles();

                // Update info on the current file
                FilesInfo.DisplayCurrentFileInfo();
            }

            // Apply options to npp and scintilla depending if we are on a progress file or not
            ApplyPluginSpecificOptions(false);

            // Parse the document
            if (PluginIsFullyLoaded)
                AutoComplete.ParseCurrentDocument(true);
        }

        /// <summary>
        /// Called when the user saves the current document
        /// </summary>
        public static void OnFileSaved() {
            // check for block that are too long and display a warning
            if (Abl.IsCurrentFileFromAppBuilder() && !FilesInfo.GetFileInfo().WarnedTooLong) {
                var warningMessage = new StringBuilder();
                foreach (var codeExplorerItem in ParserHandler.ParsedExplorerItemsList.Where(codeExplorerItem => codeExplorerItem.Flag.HasFlag(CodeExplorerFlag.IsTooLong)))
                    warningMessage.AppendLine("<div><img src='IsTooLong'><img src='" + codeExplorerItem.Branch + "' style='padding-right: 10px'><a href='" + codeExplorerItem.GoToLine + "'>" + codeExplorerItem.DisplayText + "</a></div>");
                if (warningMessage.Length > 0) {
                    warningMessage.Insert(0, "<h2>Friendly warning :</h2>It seems that your file can be opened in the appbuilder as a structured procedure, but i detected that one or several procedure/function blocks contains more than " + Config.Instance.GlobalMaxNbCharInBlock + " characters. A direct consequence is that you won't be able to open this file in the appbuilder, it will generate errors and it will be unreadable. Below is a list of incriminated blocks :<br><br>");
                    warningMessage.Append("<br><i>To prevent this, reduce the number of chararacters in the above blocks, deleting dead code and trimming spaces is a good place to start!</i>");
                    var curPath = Npp.GetCurrentFilePath();
                    UserCommunication.Notify(warningMessage.ToString(), MessageImg.MsgHighImportance, "File saved", args => {
                        Npp.Goto(curPath, int.Parse(args.Link));
                    }, "Appbuilder limitations", 20);
                    FilesInfo.GetFileInfo().WarnedTooLong = true;
                }
            }
        }

        #endregion

        #region public

        #region Apply Npp options

        private static bool _indentWithTabs;
        private static int _indentWidth;
        private static Annotation _annotationMode;

        /// <summary>
        /// We need certain options to be set to specific values when running this plugin, make sure to set everything back to normal
        /// when switch tab or when we leave npp, param can be set to true to force the default values
        /// </summary>
        /// <param name="forceToDefault"></param>
        public static void ApplyPluginSpecificOptions(bool forceToDefault) {

            if (_indentWidth == 0) {
                _indentWidth = Npp.IndentWidth;
                _indentWithTabs = Npp.UseTabs;
                _annotationMode = Npp.AnnotationVisible;

                // Extra settings at the start
                Npp.MouseDwellTime = Config.Instance.ToolTipmsBeforeShowing;
                Npp.EndAtLastLine = false;
                Npp.ViewWhitespace = WhitespaceMode.VisibleAlways;
                Npp.EventMask = (int)(SciMsg.SC_MOD_INSERTTEXT | SciMsg.SC_MOD_DELETETEXT | SciMsg.SC_PERFORMED_USER | SciMsg.SC_PERFORMED_UNDO | SciMsg.SC_PERFORMED_REDO);
            }

            if (!IsCurrentFileProgress || forceToDefault) {
                Npp.AutoCStops("");
                Npp.AnnotationVisible = _annotationMode;
                Npp.UseTabs = _indentWithTabs;
                Npp.IndentWidth = _indentWidth;
            } else {
                // barbarian method to force the default autocompletion window to hide, it makes npp slows down when there is too much text...
                // TODO: find a better technique to hide the autocompletion!!! this slows npp down
                Npp.AutoCStops(@"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                Npp.AnnotationVisible = Annotation.Boxed;
                Npp.UseTabs = false;
                Npp.IndentWidth = Config.Instance.AutoCompleteIndentNbSpaces;
            }
        }

        #endregion

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
            FileTags.ForceClose();
        }

        #endregion

        #region tests
        public static void Test() {

            var progressExec = new ProgressExecution();
            progressExec.Do(ExecutionType.Run);
            UserCommunication.Notify("<a href='" + progressExec.ExecutionDir + "'>" + progressExec.ExecutionDir + "</a>");

            return;

            /*
            var derp = FilesInfo.ReadErrorsFromFile(@"C:\Work\3PA_side\ProgressFiles\compile\sc80lbeq.log", false);
            foreach (var kpv in derp) {
                FilesInfo.UpdateFileErrors(kpv.Key, kpv.Value);
            }*/

            var canIndent = ParserHandler.CanIndent();
            UserCommunication.Notify(canIndent ? "This document can be reindented!" : "Oups can't reindent the code...<br>Log : <a href='" + Path.Combine(TempDir, "lines.log") + "'>" + Path.Combine(TempDir, "lines.log") + "</a>", canIndent ? MessageImg.MsgOk : MessageImg.MsgError, "Parser state", "Can indent?", 20);
            if (!canIndent) {
                StringBuilder x = new StringBuilder();
                var i = 0;
                var dic = ParserHandler.GetLineInfo();
                while (dic.ContainsKey(i)) {
                    x.AppendLine((i + 1) + " > " + dic[i].BlockDepth + " , " + dic[i].Scope + " , " + dic[i].CurrentScopeName);
                    //x.AppendLine(item.Key + " > " + item.Value.BlockDepth + " , " + item.Value.Scope);
                    i++;
                }
                File.WriteAllText(Path.Combine(TempDir, "lines.log"), x.ToString());
            }

            var properties = typeof(ConfigObject).GetFields();

            /* loop through fields */
            foreach (var property in properties) {
                if (property.IsPrivate) continue;

                var listCustomAttr = property.GetCustomAttributes(typeof(DisplayAttribute), false);
                if (listCustomAttr.Any()) {
                    var displayAttr = (DisplayAttribute)listCustomAttr.First();
                }
            }

            FileTags.UnCloak();

            //var x = 0;
            //var y = 1/x;
        }
        #endregion
    }
}