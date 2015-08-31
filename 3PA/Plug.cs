using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Forms;
using YamuiFramework.Themes;
using _3PA.Appli;
using _3PA.Data;
using _3PA.Forms;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.Properties;
using AutoComplete = _3PA.MainFeatures.AutoComplete;
using Config = _3PA.Lib.Config;

#pragma warning disable 1591

namespace _3PA {

    public partial class Plug {

        #region " Properties "
        public static YamuiForm MainForm;

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
            Interop.Plug.SetCommand(cmdIndex++, "Show table names list", AutoComplete.ShowTablesSuggestions, "Show_TablesSuggestions:Alt+Space", false, uniqueKeys);
            Interop.Plug.SetCommand(cmdIndex++, "Show code snippet list", AutoComplete.ShowSnippetsList, "Show_SnippetsList:Ctrl+Shift+Space", false, uniqueKeys);
            
            Interop.Plug.SetCommand(cmdIndex++, "---", null);

            Interop.Plug.SetCommand(cmdIndex++, "Test", hello, "_Test:Ctrl+D", false, uniqueKeys);
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

        }

        /// <summary>
        /// Called on Npp shutdown
        /// </summary>
        static internal void CleanUp() {
            // save config (should be done but just in case)
            Config.Save();

            // set options back to client's default
            ApplyPluginSpecificOptions(false);

            // remember the most used keywords
            Keywords.Save();
        }

        /// <summary>
        /// Called on npp ready
        /// </summary>
        static internal void OnNppReady() {

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

            Npp.HideDefaultAutoCompletion();

            // initialize autocompletion (asynchrone)
            Task.Factory.StartNew(() => {
                try {
                    Snippets.Init();
                    Keywords.Init();
                    FileTags.Init();

                    DataBaseInfo.Init();

                    Config.Save();

                    // copy .p and .r needed for the progress heavy lifting
                    var _rootDir = Path.Combine(Npp.GetThisAssemblyPath(), Resources.PluginFolderName);
                    var _path = Path.Combine(_rootDir, @"compilUnCCL.p");
                    if (!File.Exists(_path))
                        File.WriteAllBytes(_path, DataResources.compilUnCCL);
                    _path = Path.Combine(_rootDir, @"nppTool.p");
                    if (!File.Exists(_path))
                        File.WriteAllBytes(_path, DataResources.nppTool);
                    _path = Path.Combine(_rootDir, @"progress_getini.r");
                    if (!File.Exists(_path))
                        File.WriteAllBytes(_path, DataResources.progress_getini);
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
                // only do stuff if we are in a progress file
                if (!Npp.IsCurrentProgressFile()) return;

                string newStr = c.ToString();
                Point keywordPos;
                string keyword;

                // we are currently entering a keyword
                if (new Regex(@"^[\w-&\{\}]$").Match(newStr).Success) {
                    AutoComplete.ActivatedAutoCompleteIfNeeded();

                // we finished entering a keyword
                } else { 

                    AutoComplete.CloseSuggestionList();

                    int offset = (newStr.Equals("\n") && Npp.TextBeforeCaret(2).Equals("\r\n")) ? 2 : 1; 
                    int curPos = Npp.GetCaretPosition();
                    bool isNormalContext = Npp.IsNormalContext(curPos);

                    // show suggestions on fields
                    if (c == '.' && Config.Instance.AutoCompleteShowFieldSuggestionsOnPointInput && DataBaseInfo.ContainsTable(Npp.GetCurrentTable()))
                        AutoComplete.ShowFieldsSuggestions(true);

                    // only do more stuff if we are not in a string/comment/include definition 
                    if (!isNormalContext) return;

                    keyword = Npp.GetKeywordOnLeftOfPosition(curPos - offset, out keywordPos);
                    bool lastWordInDico = AutoComplete.IsLastWordInDico(keyword, curPos, offset);

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

                    // replace the last keyword by the correct case, check the context of the caret, can't upper case in comments for example
                    if (Config.Instance.AutoCompleteChangeCaseMode != 0 && !string.IsNullOrWhiteSpace(keyword) && Npp.IsNormalContext(curPos) && lastWordInDico)
                        Npp.WrappedKeywordReplace(Npp.AutoCaseToUserLiking(keyword), keywordPos, curPos);

                    // replace semicolon by a point
                    if (c == ';' && Config.Instance.AutoCompleteReplaceSemicolon && Npp.IsNormalContext(curPos) && lastWordInDico)
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
                        else if (keyword.EqualsCi("end")) {
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
                ShowErrors(e, "Error in OnCharTyped");
            }
        }

        /// <summary>
        /// Called when the user presses a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="repeatCount"></param>
        /// <param name="handled"></param>
        static void Instance_KeyDown(Keys key, int repeatCount, ref bool handled) {
            // only do stuff if we are in a progress file
            handled = false;
            if (!Npp.IsCurrentProgressFile()) return;

            try {
                if (AutoComplete.IsShowingAutocompletion) {
                    Modifiers modifiers = KeyInterceptor.GetModifiers();
                    if (key == Keys.Up || key == Keys.Down || key == Keys.Right || key == Keys.Left || key == Keys.Tab || key == Keys.Return ||
                        key == Keys.Escape) {
                        handled = AutoComplete.GetForm.OnKeyDown(key);
                    } else if (key == Keys.PageDown || key == Keys.PageUp || key == Keys.Next || key == Keys.Prior) {
                        AutoComplete.CloseSuggestionList();
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
                ShowErrors(e, "Error in Instance_KeyDown");
            }
        }

        public static void OnUpdateSelection() {
            // close suggestions
            AutoComplete.CloseSuggestionList();
        }

        public static void OnPageScrolled() {
            AutoComplete.CloseSuggestionList();
        }

        public static void OnLineAddedOrRemoved() {
            if (!Npp.GetCurrentFile().Equals(CurrentFile) && Npp.IsCurrentProgressFile()) {
                CurrentFile = Npp.GetCurrentFile();
                //MessageBox.Show("time to parse this file");
            }
        }

        /// <summary>
        /// We need certain options to be set to specific values when running this plugin, make sure to set everything back to normal
        /// when the user modifiy the doc, change the selection or when we leave npp
        /// </summary>
        public static void ApplyPluginSpecificOptions() {
            ApplyPluginSpecificOptions(false);
        }

        /// <summary>
        /// We need certain options to be set to specific values when running this plugin, make sure to set everything back to normal
        /// when switch tab or when we leave npp, param can be set to true to force the defautl values
        /// </summary>
        /// <param name="forceToDefault"></param>
        public static void ApplyPluginSpecificOptions(bool forceToDefault) {
            string cur = Npp.GetCurrentFile();
            if (!cur.Equals(CurrentFile) || forceToDefault) {
                CurrentFile = cur;
                if (!Npp.IsCurrentProgressFile() || forceToDefault) {
                    Npp.ResetDefaultAutoCompletion();
                    Npp.SetIndent(_indentWidth);
                    Npp.SetUseTabs(_indentWithTabs);
                } else {
                    Npp.HideDefaultAutoCompletion();
                    _indentWidth = Npp.GetIndent();
                    _indentWithTabs = Npp.GetUseTabs();
                    Npp.SetIndent(Config.Instance.AutoCompleteIndentNbSpaces);
                    Npp.SetUseTabs(false);
                }
            }
        }
        #endregion

        #region " other functions "
        static void GoToDefinition() {
            var cursor = Cursor.Current;
            try {
                Cursor.Current = Cursors.WaitCursor;

                if (Npp.IsCurrentProgressFile()) {
                    //DomRegion region = ResolveMemberAtCaret();

                    //if (region != DomRegion.Empty)
                    //{
                    //    Npp.OpenFile(region.FileName);
                    //    Npp.GoToLine(region.BeginLine);
                    //    Npp.ScrollToCaret();
                    //    Npp.GrabFocus();
                    //}
                }
            } catch { } finally {
                Cursor.Current = cursor;
            }
        }



        /*
        static void ShowAboutBox()
        {
            using (var form = new AboutBox())
                form.ShowDialog();
        }
         

        static public void ShowConfig()
        {
            using (var form = new ConfigForm(Config.Instance))
            {
                form.ShowDialog();
                Config.Instance.Save();
                ReflectorExtensions.IgnoreDocumentationExceptions = Config.Instance.IgnoreDocExceptions;
            }
        }
        */    

        static void run_ext(string command, string currentFile) {

            string baseini = @"P:\appli\sac1\sacdev.ini";
            string basepf = @"P:\base\tmaprogress\newtmap.pf";
            string assemblies = @"C:\Progress\proparse.net";
            string tempfolder = @"C:\Temp";

            // save the path to the progress files in the registry
            Registry.SetValue(Resources.RegistryPath, "scriptLoc", Npp.GetConfigDir(), RegistryValueKind.String);

            StringBuilder args = new StringBuilder();

            args.Append(
                " -cpinternal ISO8859-1" +
                " -inp 20000 -tok 2048 -numsep 46" +
                " -p " + Quoter("nppTool.p"));

            if (Directory.Exists(tempfolder)) {
                args.Append(" -T " + Quoter(tempfolder));
            }

            StringBuilder param = new StringBuilder();
            param.Append(command + "," + currentFile);

            if (File.Exists(baseini) && File.Exists(basepf)) {
                args.Append(
                " -ini " + Quoter(baseini) +
                " -pf " + Quoter(basepf));
                param.Append(",1");
            } else {
                param.Append(",0");
            }

            if (Directory.Exists(assemblies)) {
                args.Append(" -assemblies " + Quoter(assemblies));
                param.Append(",1");
            } else {
                param.Append(",0");
            }

            args.Append(" -param " + Quoter(param.ToString()));

            // execute
            Process process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = Config.Instance.ProgressProwin32ExePath;
            process.StartInfo.Arguments = args.ToString();
            process.StartInfo.WorkingDirectory = Npp.GetConfigDir();
            process.EnableRaisingEvents = true;
            process.Exited += afterProgressExecution;
            process.Start();

            MessageBox.Show(args.ToString(), "yo", MessageBoxButtons.OK);
        }

        // Handle Exited event and display process information. 
        static void afterProgressExecution(object sender, EventArgs e) {
            MessageBox.Show("ok", "yo", MessageBoxButtons.OK);
        }
        #endregion

        #region " helper "

        public static string Quoter(string inString) { return "\"" + inString + "\""; }

        public static void ShowErrors(Exception e, string message, string fileName) {
            MessageBox.Show("Error in " + AssemblyInfo.ProductTitle + ", couldn't load the following file : \n" +
                            fileName +
                            "\nThe file has been renamed with the '_errors' suffix to avoid further problems.");
            if (File.Exists(fileName + "_errors"))
                File.Delete(fileName + "_errors");
            File.Move(fileName, fileName + "_errors");
            ShowErrors(e, message);
        }

        public static void ShowErrors(Exception e, string message) {
#if DEBUG
            MessageBox.Show("Custom error : " + message + "\n" + e.ToString());
#else
#endif
        }

        public static void MessageToUser(string text) {
            MessageBox.Show(text);
        }
        #endregion

        #region " demo "
        static string sessionFilePath = @"C:\text.session";
        static frmGoToLine frmGoToLine;
        static internal int idFrmGotToLine = 15;
        static Bitmap tbBmp = ImageResources.autocompletion_field;
        static Bitmap tbBmp_tbTab = ImageResources.autocompletion_field_pk;
        static Icon tbIcon;


        
        static void hello() {
            ThemeManager.TabAnimationAllowed = true;
            MainForm = new Form1 {
                Opacity = 0d,
                Tag = false
            };
            MainForm.Closing += (sender, args) => {
                if ((bool) MainForm.Tag) return;
                args.Cancel = true;
                MainForm.Tag = true;
                var t = new Transition(new TransitionType_Acceleration(200));
                t.add(MainForm, "Opacity", 0d);
                t.TransitionCompletedEvent += (o, args1) => { MainForm.Close(); };
                t.run();
            };
            Transition.run(MainForm, "Opacity", 1d, new TransitionType_Acceleration(200));
            Application.Run(MainForm);
        }

        static void getFileNamesDemo() {
            int nbFile = (int)Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_GETNBOPENFILES, 0, 0);
            MessageBox.Show(nbFile.ToString(), "Number of opened files:");

            using (ClikeStringArray cStrArray = new ClikeStringArray(nbFile, Win32.MAX_PATH)) {
                if (Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_GETOPENFILENAMES, cStrArray.NativePointer, nbFile) != IntPtr.Zero)
                    foreach (string file in cStrArray.ManagedStringsUnicode) MessageBox.Show(file);
            }
        }
        static void getSessionFileNamesDemo() {
            int nbFile = (int)Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_GETNBSESSIONFILES, 0, sessionFilePath);

            if (nbFile < 1) {
                MessageBox.Show("Please modify \"sessionFilePath\" in \"Demo.cs\" in order to point to a valid session file", "Error");
                return;
            }
            MessageBox.Show(nbFile.ToString(), "Number of session files:");

            using (ClikeStringArray cStrArray = new ClikeStringArray(nbFile, Win32.MAX_PATH)) {
                if (Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_GETSESSIONFILES, cStrArray.NativePointer, sessionFilePath) != IntPtr.Zero)
                    foreach (string file in cStrArray.ManagedStringsUnicode) MessageBox.Show(file);
            }
        }
        static void saveCurrentSessionDemo() {
            string sessionPath = Marshal.PtrToStringUni(Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SAVECURRENTSESSION, 0, sessionFilePath));
            if (!string.IsNullOrEmpty(sessionPath))
                MessageBox.Show(sessionPath, "Saved Session File :", MessageBoxButtons.OK);
        }

        static void DockableDlgDemo() {
            // Dockable Dialog Demo
            // 
            // This demonstration shows you how to do a dockable dialog.
            // You can create your own non dockable dialog - in this case you don't nedd this demonstration.
            if (frmGoToLine == null) {
                frmGoToLine = new frmGoToLine();

                using (Bitmap newBmp = new Bitmap(16, 16)) {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmGoToLine.Handle;
                _nppTbData.pszName = "Go To Line #";
                // the dlgDlg should be the index of funcItem where the current function pointer is in
                // this case is 15.. so the initial value of funcItem[15]._cmdID - not the updated internal one !
                _nppTbData.dlgID = idFrmGotToLine;
                // define the default docking behaviour
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = Assembly.GetExecutingAssembly().GetName().Name;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
                // Following message will toogle both menu item state and toolbar button
                Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETMENUITEMCHECK, FuncItems.Items[idFrmGotToLine]._cmdID, 1);
            } else {
                if (!frmGoToLine.Visible) {
                    Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMSHOW, 0, frmGoToLine.Handle);
                    Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETMENUITEMCHECK, FuncItems.Items[idFrmGotToLine]._cmdID, 1);
                } else {
                    Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMHIDE, 0, frmGoToLine.Handle);
                    Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETMENUITEMCHECK, FuncItems.Items[idFrmGotToLine]._cmdID, 0);
                }
            }
            frmGoToLine.textBox1.Focus();
        }

        #endregion

    }
}