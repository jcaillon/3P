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
using System.Diagnostics;
using System.Drawing;
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
using _3PA.MainFeatures.FilesInfoNs;
using _3PA.MainFeatures.InfoToolTip;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA {

    public class Plug {

        #region Fields

        #region Npp plugin mandatory objects
        /// <summary>
        /// Contain information about the plugin and about the plugin items menu, info passed to Npp on startup
        /// </summary>
        public static NppData NppData;
        public static FuncItems FuncItems = new FuncItems();

        #endregion

        /// <summary>
        /// this is a delegate to defined actions that must be taken after updating the ui
        /// </summary>
        public static Action ActionAfterUpdateUi { get; set; }

        /// <summary>
        /// Set to true after the plugin has been fully loaded
        /// </summary>
        public static bool PluginIsFullyLoaded;

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

        #region Current file info
        // We don't want to recompute those values all the time so we store them when the buffer (document) changes

        /// <summary>
        /// true if the current file is a progress file, false otherwise
        /// </summary>
        public static bool IsCurrentFileProgress { get; private set; }

        /// <summary>
        /// Stores the current filename when switching document
        /// </summary>
        public static string CurrentFilePath { get; private set; }

        /// <summary>
        /// Information on the current file
        /// </summary>
        public static FileInfoObject CurrentFileObject { get; private set; }

        #endregion

        #endregion

        #region Init and clean up
        /// <summary>
        /// Called on notepad++ setinfo
        /// </summary>
        static internal void CommandMenuInit() {

            var menu = new NppMenu();
            menu.SetCommand("Open main window", Appli.ToggleView, "Open_main_window:Alt+Space", false);
            menu.SetCommand("Show auto-complete suggestions", AutoComplete.OnShowCompleteSuggestionList, "Show_Suggestion_List:Ctrl+Space", false);

            menu.SetSeparator();

            menu.SetCommand("Open 4GL help", ProCodeUtils.Open4GlHelp, "Open_4GL_help:F1", false);
            menu.SetCommand("Check syntax", ProCodeUtils.CheckSyntaxCurrent, "Check_syntax:Shift+F1", false);
            menu.SetCommand("Compile", ProCodeUtils.CompileCurrent, "Compile:Alt+F1", false);
            menu.SetCommand("Run program", ProCodeUtils.RunCurrent, "Run_program:Ctrl+F1", false);
            //menu.SetCommand("Prolint code", ProgressCodeUtils.NotImplemented, "Prolint:F12", false);

            menu.SetSeparator();

            menu.SetCommand("Search file", FileExplorer.StartSearch, "Search_file:Alt+Q", false);
            menu.SetCommand("Go to definition", ProCodeUtils.GoToDefinition, "Go_To_Definition:Ctrl+B", false);
            menu.SetCommand("Go backwards", Npp.GoBackFromDefinition, "Go_Backwards:Ctrl+Shift+B", false);
            menu.SetCommand("Toggle comment line", ProCodeUtils.ToggleComment, "Toggle_Comment:Ctrl+Q", false);
            //menu.SetCommand("Insert mark", ProgressCodeUtils.NotImplemented, "Insert_mark:Ctrl+T", false);
            //menu.SetCommand("Format document", CodeBeautifier.CorrectCodeIndentation, "Format_document:Ctrl+I", false);
            //menu.SetCommand("Send to AppBuilder", ProgressCodeUtils.NotImplemented, "Send_appbuilder:Alt+O", false);

            menu.SetSeparator();

            menu.SetCommand("Edit current file info", FileTag.UnCloak, "Edit_file_info:Ctrl+Shift+M", false);
            //menu.SetCommand("Insert title block", ProgressCodeUtils.NotImplemented, "Insert_title_block:Ctrl+Alt+M", false);
            menu.SetCommand("Surround with modification tags", ProCodeUtils.SurroundSelectionWithTag, "Modif_tags:Ctrl+M", false);

            menu.SetSeparator();

            //menu.SetCommand("Insert new function", ProgressCodeUtils.NotImplemented);
            //menu.SetCommand("Insert new internal procedure", ProgressCodeUtils.NotImplemented);

            //menu.SetSeparator();

            menu.SetCommand("Toggle code explorer", CodeExplorer.Toggle);
            CodeExplorer.DockableCommandIndex = menu.CmdIndex - 1;
            menu.SetCommand("Toggle file explorer", FileExplorer.Toggle);
            FileExplorer.DockableCommandIndex = menu.CmdIndex - 1;

            menu.SetSeparator();
            
            if (Config.Instance.UserName.Equals("JCA"))
                menu.SetCommand("Test", Test, "Test:Ctrl+D", false);

            if (Config.Instance.UserName.Equals("JCA"))
                menu.SetCommand("DEBUG", StartDebug, "DEBUG:Ctrl+F12", false);

            //menu.SetSeparator();

            menu.SetCommand("Options", Appli.GoToOptionPage);
            menu.SetCommand("About", Appli.GoToAboutPage);
            
            // Npp already intercepts these shortcuts so we need to hook keyboard messages
            foreach (var key in menu.UniqueKeys.Keys)
                KeyboardMonitor.Instance.Add(key);
            KeyboardMonitor.Instance.Add(Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Tab, Keys.Return, Keys.Escape, Keys.Back, Keys.PageDown, Keys.PageUp, Keys.Next, Keys.Prior);
            KeyboardMonitor.Instance.KeyPressed += OnKeyPressed;
            KeyboardMonitor.Instance.Install();
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
                // uninstall hooks
                CallWndProcMonitor.Instance.Uninstall();
                KeyboardMonitor.Instance.Uninstall();
                CbtMonitor.Instance.Uninstall();

                // set options back to client's default
                ApplyPluginSpecificOptions(true);

                // save config (should be done but just in case)
                CodeExplorer.UpdateMenuItemChecked();
                FileExplorer.UpdateMenuItemChecked();
                Config.Save();

                // remember the most used keywords
                Keywords.Export();

                // close every form
                AutoComplete.ForceClose();
                InfoToolTip.ForceClose();
                Appli.ForceClose();
                FileTag.ForceClose();
                FileExplorer.ForceClose();
                CodeExplorer.ForceClose();
                UserCommunication.Close();

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

            #region Themes
            
            // themes and html
            ThemeManager.CurrentThemeIdToUse = Config.Instance.ThemeId;
            ThemeManager.AccentColor = Config.Instance.AccentColor;
            ThemeManager.TabAnimationAllowed = Config.Instance.AppliAllowTabAnimation;
            //ThemeManager.ThemeXmlPath = Path.Combine(Npp.GetConfigDir(), "Themes.xml");
            //Style.ThemeXmlPath = Path.Combine(Npp.GetConfigDir(), "SyntaxHighlight.xml");
            LocalHtmlHandler.RegisterToYamui();

            #endregion
            
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

            Npp.GrabFocus();

            // Try to update 3P
            UpdateHandler.OnNotepadStart();

            // everything else can be async
            //Task.Factory.StartNew(() => {

                Keywords.Import();
                Snippets.Init();
                FileTag.Import();
                ProCompilePath.Import();

                // initialize the list of objects of the autocompletion form
                AutoComplete.FillStaticItems(true);

                // init database info
                DataBase.Init();

                PluginIsFullyLoaded = true;

                // Simulates a OnDocumentSwitched when we start this dll
                OnDocumentSwitched(true);
            //});

            // hook onto messages sent to npp, to be able to correctly refresh the location/size of the explorers
            CallWndProcMonitor.Instance.Add(WindowsMessage.WM_MOUSEMOVE, WindowsMessage.WM_MBUTTONDOWN);
            CallWndProcMonitor.Instance.GetMessage += InstanceOnGetMessage;
            CallWndProcMonitor.Instance.Install();

            CbtMonitor.Instance.GetCode += InstanceOnGetCode;
            CbtMonitor.Instance.Install();

            // this is done async anyway
            FileExplorer.RebuildItemList();
        }

        #endregion

        #endregion

        #region public OnEvents

        #region on key down

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
                    if ((byte) key == kpv.Key._key && 
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
                    foreach (var shortcut in NppMenu.InternalShortCuts.Keys.Where(shortcut => (byte)key == shortcut._key  && keyModifiers.IsCtrl == shortcut.IsCtrl && keyModifiers.IsShift == shortcut.IsShift && keyModifiers.IsAlt == shortcut.IsAlt)) {
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

        #region on char typed

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
                    if (replacementWord == null && Config.Instance.CodeReplaceAbbreviations) {
                        var fullKeyword = Keywords.GetFullKeyword(keyword);
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
                Npp.TabWidth = _indentWidth;
            } else {
                // barbarian method to force the default autocompletion window to hide, it makes npp slows down when there is too much text...
                // TODO: find a better technique to hide the autocompletion!!! this slows npp down
                Npp.AutoCStops(@"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_");
                Npp.AnnotationVisible = Annotation.Boxed;
                Npp.UseTabs = false;
                Npp.TabWidth = Config.Instance.CodeTabSpaceNb;
            }
        }

        #endregion

        /// <summary>
        /// This method needs to be called by each function that the user can trigger, to check if he has the possibility to execute it,n
        /// Set "spamInterval" to a value in milliseconds, if the user called this feature less that x ms ago, it will
        /// not allow the execution and return false (set to 0 to disable spam check)
        /// </summary>
        /// <returns></returns>
        public static bool AllowFeatureExecution(int spamInterval = 300) {
            // Prevent the user from spamming the keys
            if (spamInterval > 0) {
                var method = new StackFrame(1).GetMethod();
                if (Utils.IsSpamming(method.DeclaringType + method.Name, spamInterval))
                    return false;
            }

            // correct info since it doesn't cost too much here (can be wrong when creating a new file, saving as .p, since the buffer didn't change we didn't execute the OnDocumentSwitched method)
            if (!CurrentFilePath.EqualsCi(Npp.GetCurrentFilePath()))
                OnDocumentSwitched();

            return PluginIsFullyLoaded && IsCurrentFileProgress;
        }

        /// <summary>
        /// Call this method to close all popup/autocompletion form and alike
        /// </summary>
        public static void ClosePopups() {
            AutoComplete.Close();
            InfoToolTip.Close();
        }

        #endregion

        #region private

        /// <summary>
        /// Handles the messages we receive from npp's hook
        /// </summary>
        private static void InstanceOnGetMessage(MSG message, out bool handled) {
            switch (message.message) {
                case (uint) WindowsMessage.WM_MOUSEMOVE:
                    var buttPressed = message.wParam.ToInt32();
                    // left or right click pressed
                    if (buttPressed == 1 || buttPressed == 2) {
                        UpdateExplorersPos();
                    
                    }
                    break;
                case (uint) WindowsMessage.WM_MBUTTONDOWN:
                    //Rectangle scintillaRectangle = Rectangle.Empty;
                    //Win32.GetWindowRect(Npp.HandleScintilla, ref scintillaRectangle);
                    //if (scintillaRectangle.Contains(Cursor.Position))
                    if (KeyboardMonitor.GetModifiers.IsCtrl)
                        ProCodeUtils.GoToDefinition(true);
                    handled = true;
                    return;
                default:
                    UpdateExplorersPos();
                    break;
            }
            handled = false;
        }

        /// <summary>
        /// Handles the messages we receive from npp
        /// </summary>
        private static void InstanceOnGetCode(HCBT code) {
            switch (code) {
                case HCBT.MoveSize:
                    UpdateExplorersPos();
                    break;
            }
        }

        private static void UpdateExplorersPos() {
            if (FileExplorer.IsVisible)
                FileExplorer.Form.RefreshPosAndLoc();
            if (CodeExplorer.IsVisible)
                CodeExplorer.Form.RefreshPosAndLoc();
        }

        #endregion

        #region tests

        public static void StartDebug() {
            Debug.Assert(false);
        }

        public static void Test() {

            var ii = UserCommunication.Message(("# What's new in this version? #\n\n" + File.ReadAllText(@"C:\Users\Julien\Desktop\content.md", TextEncodingDetect.GetFileEncoding(@"C:\Users\Julien\Desktop\content.md"))).MdToHtml(),
                    MessageImg.MsgUpdate,
                    "A new version has been installed!",
                    "Updated to version " + AssemblyInfo.Version,
                    new List<string> { "ok", "cancel" },
                    true);
            UserCommunication.Notify(ii.ToString());
            return;

            //------------
            var watch = Stopwatch.StartNew();
            //------------
            var inputFile = @"C:\Users\Julien\Desktop\in.p";
            Parser tok = new Parser(File.ReadAllText(inputFile), inputFile, null, true);

            OutputVis vis = new OutputVis();
            tok.Accept(vis);

            //--------------
            watch.Stop();
            //------------

            // OUPUT OF VISITOR
            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", vis.Output.AppendLine("\n\nDONE in " + watch.ElapsedMilliseconds + " ms").ToString());


            UserCommunication.Notify(AutoComplete.KnownStaticItems.Count.ToString());

            UserCommunication.Notify(Npp.GetLine().Position + " vs " + Npp.Sci.Send(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(Npp.GetLine().Index)).ToInt32() + " and " + Npp.GetLine().Length + " vs " + Npp.Sci.Send(SciMsg.SCI_LINELENGTH, new IntPtr(Npp.GetLine().Index)).ToInt32() + " and " + Npp.LineFromPosition(Npp.GetLine().Position) + " vs " + Npp.Sci.Send(SciMsg.SCI_LINEFROMPOSITION, new IntPtr(Npp.Sci.Send(SciMsg.SCI_GETCURRENTPOS).ToInt32())).ToInt32() + " and " + Npp.Line.Count + " vs " + Npp.Sci.Send(SciMsg.SCI_GETLINECOUNT).ToInt32());

            return;
            UserCommunication.Notify(@"<h2>I require your attention!</h2><br>
                        The update didn't go as expected, i couldn't replace the old plugin file by the new one!<br>
                        It is very likely because i didn't get the rights to write a file in your /plugins/ folder, don't panic!<br>
                        You will have to manually copy the new file and delete the old file :<br><br>
                        Copy this file : <a href='" + Path.GetDirectoryName(@"L:\cnaf 2014\Production\BAO\BOI\65.100\325-2\04-Développement\BOI 325-2 - Liste des composants.xls") + "'>" + @"L:\cnaf 2014\Production\BAO\BOI\65.100\325-2\04-Développement\BOI 325-2 - Liste des composants.xls" + @"</a></b><br>" + @"
                        In this folder (replacing the old file) : <b><a href='" + Path.GetDirectoryName(AssemblyInfo.Location) + "'>" + Path.GetDirectoryName(AssemblyInfo.Location) + @"</a></b><br>
                        Please do it as soon as possible, as i will stop checking for more updates until this problem is fixed.<br>
                        Thank you for your patience!<br>", MessageImg.MsgUpdate, "Update", "Problem during the update!");

            UserCommunication.Notify("<a href='" + AssemblyInfo.Location.ProgressQuoter() + "'>" + AssemblyInfo.Location.ProgressQuoter() + "</a><br>" + AssemblyInfo.IsPreRelease + "<br><a href='" + @"C:\Users\Julien\Desktop\saxo2jira.p" + "'>" + @"C:\Users\Julien\Desktop\saxo2jira.p" + "</a>" + "<br><a href='" + @"C:\Work\3P\3PA\Interop" + "'>" + @"C:\Work\3P\3PA\Interop" + "</a>" + "<br><a href='" + @"https://github.com/jcaillon/3P/releases" + "'>" + @" https://github.com/jcaillon/3P/releases" + "</a>");
       
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

            //var x = 0;
            //var y = 1/x;
        }
        #endregion
    }

    public class OutputVis : IParserVisitor {
        public void Visit(ParsedBlock pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > BLOCK," + pars.Name + "," + pars.BranchType);
        }

        public void Visit(ParsedLabel pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name);
        }

        public void Visit(ParsedFunctionCall pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.ExternalCall);
        }

        public void Visit(ParsedFoundTableUse pars) {
            Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.Name);
        }

        public StringBuilder Output = new StringBuilder();
        public void Visit(ParsedOnEvent pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.On);
        }

        public void Visit(ParsedFunction pars) {
            return;
            Output.AppendLine(pars.Line + "," + pars.Column + " > FUNCTION," + pars.Name + "," + pars.ReturnType + "," + pars.Scope + "," + pars.OwnerName + "," + pars.Parameters + "," + pars.IsPrivate + "," + pars.PrototypeLine + "," + pars.PrototypeColumn + "," + pars.IsExtended + "," + pars.EndLine);
        }

        public void Visit(ParsedProcedure pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.EndLine + "," + pars.Left);
        }

        public void Visit(ParsedIncludeFile pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name);
        }

        public void Visit(ParsedPreProc pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.Flag + "," + pars.UndefinedLine);
        }

        public void Visit(ParsedDefine pars) {
            return;
            //if (pars.PrimitiveType == ParsedPrimitiveType.Buffer || pars.Type == ParseDefineType.Buffer)
            //if (pars.Type == ParseDefineType.Parameter)
            //if (string.IsNullOrEmpty(pars.ViewAs))
            Output.AppendLine(pars.Line + "," + pars.Column + " > " + ((ParseDefineTypeAttr)pars.Type.GetAttributes()).Value + "," + pars.LcFlagString + "," + pars.Name + "," + pars.AsLike + "," + pars.TempPrimitiveType + "," + pars.Scope + "," + pars.IsDynamic + "," + pars.ViewAs + "," + pars.BufferFor + "," + pars.Left + "," + pars.IsExtended + "," + pars.OwnerName);
        }

        public void Visit(ParsedTable pars) {
            return;
            Output.Append(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.LcLikeTable + "," + pars.OwnerName + "," + pars.UseIndex + ">");
            foreach (var field in pars.Fields) {
                Output.Append(field.Name + "|" + field.AsLike + "|" + field.Type + ",");
            }
            Output.AppendLine("");
        }

        public void Visit(ParsedRun pars) {
            return;
            Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.Left + "," + pars.HasPersistent);
        }
    }

}