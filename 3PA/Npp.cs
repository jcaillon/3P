#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Npp.cs) is part of 3P.
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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using _3PA.Html;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;

namespace _3PA {
    /// <summary>
    ///     This class contains very generic wrappers for basic Notepad++ functionality.
    /// </summary>
    internal static partial class Npp {

        #region fields

        private const int SwShownoactivate = 4;
        private const uint SwpNoactivate = 0x0010;
        public const int SbSettext = 1035;
        public const int SbSetparts = 1028;
        public const int SbGetparts = 1030;
        private const uint WmUser = 0x0400;
        private const uint SbGettextlength = WmUser + 12;
        private const uint SbGettext = WmUser + 13;

        #endregion

        /// <summary>
        /// Returns the encoding used by Npp for the current document, it should be used to 
        /// encode the string coming from and to Scintilla
        /// </summary>
        /// <remarks>This is very weird but we only need to encode/decode strings from/to scintilla
        /// when the current Encoding is UTF-8, in all other case, we can read/write the strings
        /// as they are (</remarks>
        /// <returns></returns>
        public static Encoding GetCurrentEncoding() {
            var curBufferId = WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETCURRENTBUFFERID, 0, 0);
            int nppEncoding = (int)WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETBUFFERENCODING, curBufferId, 0);
            return nppEncoding < 1 ? Encoding.Default : Encoding.UTF8;
            /*
            // Logically, we should identify the correct encoding as follow, but in reality
            // we only need to convert To/From UTF8/ANSI
            Encoding encoding = Encoding.Default;
            switch(nppEncoding) {
                case 1:
                case 4:
                    // UTF-8
                    encoding = Encoding.UTF8;
                    break;
                case 2:
                case 6:
                    // UTF-16 Big Endian
                    encoding = Encoding.BigEndianUnicode;
                    break;
                case 3:
                case 7:
                    // UTF-16 Little Endian
                    encoding = Encoding.Unicode;
                    break;
                case 5:
                    // not sure about that (uni7Bit?)
                    encoding = Encoding.UTF7;
                    break;
                default:
                    // ANSI (chars in the range 0-255 range)
                    encoding = Encoding.GetEncoding(1252);
                    break;
            }
            */
        }

        /// <summary>
        ///     Gets the Notepad++ main window handle.
        /// </summary>
        /// <value>
        ///     The Notepad++ main window handle.
        /// </value>
        public static IntPtr HandleNpp {
            get { return UnmanagedExports.NppData._nppHandle; }
        }

        /// <summary>
        /// Returns the current instance of scintilla used
        /// 0/1 corresponding to the main/seconday scintilla currently used
        /// </summary>
        public static int CurrentScintilla {
            get {
                int curScintilla;
                WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
                return curScintilla;
            }
        }

        /// <summary>
        /// Returns the screen on which npp is displayed
        /// </summary>
        /// <returns></returns>
        public static Screen GetNppScreen() {
            Rectangle output = new Rectangle();
            WinApi.GetWindowRect(HandleScintilla, ref output);
            return Screen.FromPoint(output.Location);
        }

        /// <summary>
        ///     Get the IWin32Window of the Npp window
        ///     Must be used as an input for forms.Show() in order to link the create form to the Npp window
        ///     if the user switches applications, the dialog hides with Notepad++
        /// </summary>
        public static IWin32Window Win32WindowNpp {
            get { return new WindowWrapper(HandleNpp); }
        }

        public static void SaveCurrentSession(string file) {
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_SAVECURRENTSESSION, 0, file);
        }

        public static void LoadCurrentSession(string file) {
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_LOADSESSION, 0, file);
        }

        /// <summary>
        /// Switch to a document, can be already opended or not
        /// </summary>
        /// <param name="document"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        public static void Goto(string document, int line = -1, int column = -1) {
            Goto(document, line, column, true);
        }

        /// <summary>
        /// Switch to a document, can be already opended or not, can decide to remember the current position to jump back to it
        /// </summary>
        /// <param name="document"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="saveHistoric"></param>
        public static void Goto(string document, int line, int column, bool saveHistoric) {
            if (!File.Exists(document)) {
                UserCommunication.Notify(@"Can't find/open the following file :<br>" + document, MessageImg.MsgHighImportance, "Warning", "File not found", 5);
                return;
            }
            if (saveHistoric)
                _goToHistory.Push(new Tuple<string, int, Point>(GetCurrentFilePath(), FirstVisibleLine, new Point(LineFromPosition(CurrentPosition), GetColumn(CurrentPosition))));
            if (!String.IsNullOrEmpty(document) && !document.Equals(GetCurrentFilePath())) {
                if (GetOpenedFiles().Contains(document))
                    SwitchToDocument(document);
                else
                    OpenFile(document);
            }
            if (line >= 0) {
                GoToLine(line);
                if (column >= 0)
                    SetSel(GetPosFromLineColumn(line, column));
            }
            GrabFocus();
        }

        /// <summary>
        /// handles a stack of points to go back to where we came from when we "goto definition"
        /// document path, first line visible, caret point
        /// </summary>
        private static Stack<Tuple<string, int, Point>> _goToHistory = new Stack<Tuple<string, int, Point>>();

        /// <summary>
        /// When you use the GoToDefinition method, you stack points of your position before the jump,
        /// this method allows you to navigate back to where you were
        /// </summary>
        public static void GoBackFromDefinition() {
            try {
                if (!Plug.AllowFeatureExecution())
                    return;

                if (_goToHistory.Count > 0) {
                    var lastPoint = _goToHistory.Pop();
                    Goto(lastPoint.Item1, lastPoint.Item3.X, lastPoint.Item3.Y, false);
                    FirstVisibleLine = lastPoint.Item2;
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in GoBackFromDefinition");
            }
        }


        /// <summary>
        /// Helper to add a clickable icon in the toolbar
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pluginId"></param>
        public static void SetToolbarImage(Bitmap image, int pluginId) {
            var tbIcons = new toolbarIcons { hToolbarBmp = image.GetHbitmap() };
            var pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_ADDTOOLBARICON, UnmanagedExports.FuncItems.Items[pluginId]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }


        /// <summary>
        /// Creates entry in the FuncItems list, which list the menu entry displayed in Npp's plugin menu
        /// </summary>
        public static void SetCommand(int index, string commandName, Action functionPointer, ShortcutKey shortcut = new ShortcutKey(), bool checkOnInit = false) {
            var funcItem = new FuncItem {
                _cmdID = index,
                _itemName = commandName
            };
            if (functionPointer != null)
                funcItem._pFunc = functionPointer;
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            UnmanagedExports.FuncItems.Add(funcItem);
        }

        /// <summary>
        /// Gets the file path of each file currently opened
        /// </summary>
        /// <returns></returns>
        public static List<string> GetOpenedFiles() {
            var output = new List<string>();
            int nbFile = (int)WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETNBOPENFILES, 0, 0);
            using (WinApi.ClikeStringArray cStrArray = new WinApi.ClikeStringArray(nbFile, WinApi.MaxPath)) {
                if (WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETOPENFILENAMES, cStrArray.NativePointer, nbFile) != IntPtr.Zero)
                    output.AddRange(cStrArray.ManagedStringsUnicode);
            }
            return output;
        }

        /// <summary>
        /// Gets the file path of each file in the session file, return
        /// the files separated by a new line
        /// </summary>
        /// <param name="sessionFilePath"></param>
        /// <returns></returns>
        public static string GetSessionFiles(string sessionFilePath) {
            var output = new StringBuilder();
            int nbFile = (int)WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETNBSESSIONFILES, 0, sessionFilePath);
            if (nbFile > 0) {
                using (WinApi.ClikeStringArray cStrArray = new WinApi.ClikeStringArray(nbFile, WinApi.MaxPath)) {
                    if (WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETSESSIONFILES, cStrArray.NativePointer, sessionFilePath) != IntPtr.Zero)
                        foreach (string file in cStrArray.ManagedStringsUnicode) output.AppendLine(file);
                }
            }
            return output.ToString();
        }

        /// <summary>
        /// Saves the session into a file
        /// </summary>
        /// <param name="sessionFilePath"></param>
        /// <returns></returns>
        public static bool SaveSession(string sessionFilePath) {
            string sessionPath = Marshal.PtrToStringUni(WinApi.SendMessage(HandleNpp, NppMsg.NPPM_SAVECURRENTSESSION, 0, sessionFilePath));
            return !String.IsNullOrEmpty(sessionPath);
        }

        /// <summary>
        /// Gets the path of the current document.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentFilePath() {
            var path = new StringBuilder(WinApi.MaxPath);
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            return path.ToString();
        }

        /// <summary>
        ///     Saves the current document.
        /// </summary>
        public static void SaveCurrentDocument() {
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
        }

        /// <summary>
        /// Opens given file in notepad++
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool OpenFile(string file) {
            if (!File.Exists(file)) {
                UserCommunication.Notify(@"Can't find/open the following file :<br>" + file, MessageImg.MsgHighImportance, "Warning", "File not found", 5);
                return false;
            }
            if (GetOpenedFiles().Contains(file)) {
                SwitchToDocument(file);
                return true;
            } 
            return ((int) WinApi.SendMessage(HandleNpp, NppMsg.NPPM_DOOPEN, 0, file)) > 0;
        }

        /// <summary>
        /// Returns the current file base name (uses GetFileName)
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentFileName() {
            return Path.GetFileName(GetCurrentFilePath());
        }

        /// <summary>
        /// Returns the current file folder (uses GetDirectoryName)
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentFileFolder() {
            return Path.GetDirectoryName(GetCurrentFilePath());
        }

        /// <summary>
        /// displays the input text into a new document
        /// </summary>
        /// <param name="text"></param>
        public static void NewDocument(string text) {
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.FileNew);
            GrabFocus();
        }

        /// <summary>
        ///     Determines whether the current file has the specified extension (e.g. ".cs").
        ///     <para>Note it is case insensitive.</para>
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static bool IsCurrentFileHasExtension(string extension) {
            var path = new StringBuilder(WinApi.MaxPath);
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
            var file = path.ToString();
            return !String.IsNullOrWhiteSpace(file) && file.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// returns the current file's extension
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentFileExtension() {
            string currentFileExtension;
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETEXTPART, 0, out currentFileExtension);
            return currentFileExtension;
        }

        /// <summary>
        /// returns npp's folder path
        /// </summary>
        /// <returns></returns>
        public static string GetNppDirectory() {
            string pathNotepadFolder;
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETNPPDIRECTORY, 0, out pathNotepadFolder);
            return pathNotepadFolder;
        }

        /// <summary>
        /// Switch to given document
        /// </summary>
        /// <param name="doc"></param>
        public static void SwitchToDocument(string doc) {
            WinApi.SendMessage(HandleNpp, NppMsg.NPPM_SWITCHTOFILE, 0, doc);
        }

        /// <summary>
        /// returns npp.exe path
        /// </summary>
        /// <returns></returns>
        public static string GetNppExePath() {
            return Path.Combine(GetNppDirectory(), "notepad++.exe");
        }

        public static string GetNppVersion() {
            var output = "v?.?";
            try {
                var changeLogPath = Path.Combine(GetNppDirectory(), "change.log");
                if (File.Exists(changeLogPath)) {
                    string firstLine;
                    using (var stream = new StreamReader(changeLogPath)) {
                        firstLine = stream.ReadLine();
                    }
                    if (firstLine != null) {
                        firstLine = firstLine.Replace("Notepad++ ", "");
                        output = firstLine.Substring(0, firstLine.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase));
                    }
                }
            } catch (Exception e) {
                ErrorHandler.DirtyLog(e);
            }
            return output;
        }

        /// <summary>
        /// Returns the configuration directory path e.g. /plugins/config/{ProductTitle}
        /// </summary>
        /// <returns></returns>
        public static string GetConfigDir() {
            if (string.IsNullOrEmpty(_configDir)) {
                var buffer = new StringBuilder(WinApi.MaxPath);
                WinApi.SendMessage(HandleNpp, NppMsg.NPPM_GETPLUGINSCONFIGDIR, WinApi.MaxPath, buffer);
                _configDir = Path.Combine(buffer.ToString(), AssemblyInfo.ProductTitle);
                if (!Directory.Exists(_configDir))
                    Directory.CreateDirectory(_configDir);
            }
            return _configDir;
        }

        private static string _configDir;

        /// <summary>
        /// Leaves npp
        /// </summary>
        public static void Exit() {
            const int wmCommand = 0x111;
            WinApi.SendMessage(HandleNpp, (NppMsg)wmCommand, (int)NppMenuCmd.FileExit, 0);
        }

        public static void ShowInactiveTopmost(Form frm) {
            WinApi.ShowWindow(frm.Handle, WinApi.ShowWindowCommands.ShowNoActivate);
            //SetWindowPos(frm.Handle.ToInt32(), HWND_TOPMOST, frm.Left, frm.Top, frm.Width, frm.Height, SWP_NOACTIVATE);
            WinApi.SetWindowPos(frm.Handle.ToInt32(), HandleNpp.ToInt32(), frm.Left, frm.Top, frm.Width, frm.Height,
                SwpNoactivate);
        }

        /// <summary>
        /// Sets the label in the status of npp (bottom right)
        /// </summary>
        /// <remarks>WARNING : THIS METHOD IS HIGHLY UNSTABLE, USE ONLY FOR DEBUG!</remarks>
        /// <param name="labelText"></param>
        /// <returns></returns>
        public static string SetStatusbarLabel(string labelText) {
            string retval = null;
            var mainWindowHandle = HandleNpp;
            // find status bar control on the main window of the application
            var statusBarHandle = WinApi.FindWindowEx(mainWindowHandle, IntPtr.Zero, "msctls_statusbar32", IntPtr.Zero);
            if (statusBarHandle != IntPtr.Zero) {
                //cut current text
                var size = (int)WinApi.SendMessage(statusBarHandle, SbGettextlength, 0, IntPtr.Zero);
                var buffer = new StringBuilder(size);
                WinApi.SendMessage(statusBarHandle, (NppMsg)SbGettext, 0, buffer);
                retval = buffer.ToString();

                // set text for the existing part with index 0
                var text = Marshal.StringToHGlobalAuto(labelText);
                WinApi.SendMessage(statusBarHandle, SbSettext, 0, text);
                Marshal.FreeHGlobal(text);

                //the foolowing may be needed for puture features
                // create new parts width array
                var nParts = WinApi.SendMessage(statusBarHandle, SbGetparts, 0, IntPtr.Zero).ToInt32();
                nParts++;
                var memPtr = Marshal.AllocHGlobal(sizeof(int) * nParts);
                var partWidth = 100; // set parts width according to the form size
                for (var i = 0; i < nParts; i++) {
                    Marshal.WriteInt32(memPtr, i * sizeof(int), partWidth);
                    partWidth += partWidth;
                }
                WinApi.SendMessage(statusBarHandle, SbGetparts, nParts, memPtr);
                Marshal.FreeHGlobal(memPtr);

                //// set text for the new part
                var text0 = Marshal.StringToHGlobalAuto(labelText);
                WinApi.SendMessage(statusBarHandle, SbGetparts, nParts - 1, text0);
                Marshal.FreeHGlobal(text0);
            }
            return retval;
        }
    }
}