#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Pro;

namespace _3PA {

    /// <summary>
    /// This class contains very generic wrappers for basic Notepad++ functionality
    /// </summary>
    internal static partial class Npp {

        #region CurrentFile Info

        #region private

        private static NppFile _currentFile;

        #endregion

        #region Accessors

        /// <summary>
        /// CurrentFile
        /// </summary>
        public static NppFile CurrentFile {
            get { return _currentFile ?? (_currentFile = new NppFile()); }
        }

        #endregion

        #region NppFileInfo

        /// <summary>
        /// We don't want to recompute those values all the time so we store them when the buffer (document) changes
        /// </summary>
        internal class NppFile {
            /// <summary>
            /// true if the current file is a progress file, false otherwise
            /// </summary>
            public bool IsProgress { get; set; }

            /// <summary>
            /// Stores the current file path when switching document
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Information on the current file
            /// </summary>
            public FileInfoObject FileInfoObject { get; set; }

            public void Update() {
                CurrentFile.Path = PathFromApi;
                CurrentFile.IsProgress = CurrentFile.Path.TestAgainstListOfPatterns(Config.Instance.ProgressFilesPattern);
            }

            /// <summary>
            /// Is the file a progress + compilable file?
            /// </summary>
            public bool IsCompilable {
                get { return Path.TestAgainstListOfPatterns(Config.Instance.CompilableFilesPattern); }
            }

            /// <summary>
            /// file name
            /// </summary>
            public string FileName { get { return System.IO.Path.GetFileName(Path); } }

            /// <summary>
            /// Directory of file
            /// </summary>
            public string DirectoryName { get { return System.IO.Path.GetDirectoryName(Path); } }

            /// <summary>
            /// Extension of file
            /// </summary>
            public string Extension { get { return System.IO.Path.GetExtension(Path); } }

            /// <summary>
            /// Gets the path of the current document
            /// </summary>
            /// <returns></returns>
            public static string PathFromApi {
                get {
                    var path = new StringBuilder(Win32Api.MaxPath);
                    Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
                    return path.ToString();
                }
            }

            /// <summary>
            /// Saves the current document
            /// </summary>
            public static void Save() {
                Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_SAVECURRENTFILE, 0, 0);
            }

            /// <summary>
            /// Returns the encoding used by Npp for the current document, it should be used to 
            /// encode the string coming from and to Scintilla
            /// </summary>
            /// <remarks>This is very weird but we only need to encode/decode strings from/to scintilla
            /// when the current Encoding is UTF-8, in all other case, we can read/write the strings
            /// as they are (</remarks>
            /// <returns></returns>
            public Encoding BufferEncoding {
                get {
                    var curBufferId = Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETCURRENTBUFFERID, 0, 0);
                    int nppEncoding = (int) Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETBUFFERENCODING, curBufferId, 0);
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
            }
        }

        #endregion
        
        #endregion

        /// <summary>
        ///     Gets the Notepad++ main window handle.
        /// </summary>
        /// <value>
        ///     The Notepad++ main window handle.
        /// </value>
        public static IntPtr HandleNpp {
            get { return UnmanagedExports.NppData._nppHandle; }
        }

        public static bool IsNppWindowFocused {
            get {
                return (Win32Api.GetForegroundWindow() == HandleNpp);
            }
        }

        public static bool IsScintillaFocused {
            get {
                return (Win32Api.GetForegroundWindow() == HandleScintilla);
            }
        }

        /// <summary>
        /// Returns the screen on which npp is displayed
        /// </summary>
        /// <returns></returns>
        public static Screen NppScreen {
            get {
                Rectangle nppRect =  Win32Api.GetWindowRect(HandleScintilla);
                var nppLoc = nppRect.Location;
                nppLoc.Offset(nppRect.Width / 2, nppRect.Height / 2);
                return Screen.FromPoint(nppLoc);
            }
        }

        /// <summary>
        /// Get the IWin32Window of the Npp window
        /// Must be used as an input for forms.Show() in order to link the create form to the Npp window
        /// if the user switches applications, the dialog hides with Notepad++
        /// </summary>
        public static IWin32Window Win32WindowNpp {
            get { return new WindowWrapper(HandleNpp); }
        }

        public static void SaveCurrentSession(string file) {
            Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_SAVECURRENTSESSION, 0, file);
        }

        public static void LoadCurrentSession(string file) {
            Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_LOADSESSION, 0, file);
        }

        /// <summary>
        /// Switch to a document, can be already opended or not
        /// </summary>
        public static void Goto(string document, int line = -1, int column = -1) {
            Goto(document, -1, line, column, true);
        }

        /// <summary>
        /// Switch to a document, can be already opended or not
        /// </summary>
        public static void GotoPos(string document, int position) {
            Goto(document, position, -1, -1, true);
        }

        /// <summary>
        /// Switch to a document, can be already opended or not, can decide to remember the current position to jump back to it
        /// </summary>
        public static void Goto(string document, int position, int line, int column, bool saveHistoric) {
            if (!File.Exists(document)) {
                UserCommunication.Notify(@"Can't find/open the following file :<br>" + document, MessageImg.MsgHighImportance, "Warning", "File not found", 5);
                return;
            }
            if (saveHistoric && CurrentFile.IsProgress)
                _goToHistory.Push(new Tuple<string, int, Point>(CurrentFile.Path, FirstVisibleLine, new Point(LineFromPosition(CurrentPosition), GetColumn(CurrentPosition))));
            if (!string.IsNullOrEmpty(document) && !document.Equals(CurrentFile.Path)) {
                if (GetOpenedFiles().Contains(document))
                    SwitchToDocument(document);
                else
                    OpenFile(document);
            }
            if (position >= 0) {
                GoToLine(LineFromPosition(position));
                SetSel(position);
            } else if (line >= 0) {
                GoToLine(line);
                if (column >= 0)
                    SetSel(GetPosFromLineColumn(line, column));
                else
                    SetSel(GetLine(line).Position);

            }
            GrabFocus();
            Plug.OnUpdateSelection();
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
                if (_goToHistory.Count > 0) {
                    var lastPoint = _goToHistory.Pop();
                    Goto(lastPoint.Item1, -1, lastPoint.Item3.X, lastPoint.Item3.Y, false);
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
            Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_ADDTOOLBARICON, UnmanagedExports.FuncItems.Items[pluginId]._cmdID, pTbIcons);
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
        /// Gets the file path of each file currently opened in the primary view
        /// </summary>
        /// <returns></returns>
        public static List<string> GetOpenedFilesPrimary() {
            return GetOpenedFiles(NppMsg.PRIMARY_VIEW, NppMsg.NPPM_GETOPENFILENAMESPRIMARY);
        }

        /// <summary>
        /// Gets the file path of each file currently opened in the secondary view
        /// </summary>
        /// <returns></returns>
        public static List<string> GetOpenedFilesSecondary() {
            return GetOpenedFiles(NppMsg.SECOND_VIEW, NppMsg.NPPM_GETOPENFILENAMESSECOND);
        }

        /// <summary>
        /// Gets the file path of each file currently opened
        /// </summary>
        /// <returns></returns>
        public static List<string> GetOpenedFiles() {
            return GetOpenedFiles(NppMsg.ALL_OPEN_FILES, NppMsg.NPPM_GETOPENFILENAMES);
        }

        /// <summary>
        /// Gets the file path of each file currently opened in the secondary view
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOpenedFiles(NppMsg view, NppMsg mode) {
            var output = new List<string>();
            int nbFile = (int)Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETNBOPENFILES, 0, (int)view);
            using (Win32Api.ClikeStringArray cStrArray = new Win32Api.ClikeStringArray(nbFile, Win32Api.MaxPath)) {
                if (Win32Api.SendMessage(HandleNpp, mode, cStrArray.NativePointer, nbFile) != IntPtr.Zero)
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
            int nbFile = (int)Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETNBSESSIONFILES, 0, sessionFilePath);
            if (nbFile > 0) {
                using (Win32Api.ClikeStringArray cStrArray = new Win32Api.ClikeStringArray(nbFile, Win32Api.MaxPath)) {
                    if (Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETSESSIONFILES, cStrArray.NativePointer, sessionFilePath) != IntPtr.Zero)
                        foreach (string file in cStrArray.ManagedStringsUnicode) output.AppendLine(file);
                }
            }
            return output.ToString();
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
            return ((int) Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_DOOPEN, 0, file)) > 0;
        }
        


        /// <summary>
        /// returns npp's folder path
        /// </summary>
        /// <returns></returns>
        public static string GetNppDirectory() {
            string pathNotepadFolder;
            Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETNPPDIRECTORY, 0, out pathNotepadFolder);
            return pathNotepadFolder;
        }

        /// <summary>
        /// displays the input text into a new document
        /// </summary>
        /// <param name="text"></param>
        public static void NewDocument(string text) {
            RunCommand(NppMenuCmd.FileNew);
            GrabFocus();
        }

        /// <summary>
        /// Switch to given document
        /// </summary>
        /// <param name="doc"></param>
        public static void SwitchToDocument(string doc) {
            Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_SWITCHTOFILE, 0, doc);
        }

        /// <summary>
        /// returns npp.exe path
        /// </summary>
        /// <returns></returns>
        public static string GetNppExePath {
            get {
                return Path.Combine(GetNppDirectory(), "notepad++.exe");
            }
            
        }

        /// <summary>
        /// Returns the current version of notepad++ (format vX.X.X)
        /// </summary>
        /// <returns></returns>
        public static string GetNppVersion {
            get {
                if (string.IsNullOrEmpty(_nppVersion)) {
                var nppVersion = Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETNPPVERSION, 0, 0).ToInt64();
                var lowWord = (nppVersion & 0x0000FFFF).ToString();
                _nppVersion = "v" + (nppVersion >> 16 & 0x0000FFFF) + "." + lowWord.Substring(0, 1) + "." + (string.IsNullOrEmpty(lowWord.Substring(1)) ? "0" : lowWord.Substring(1));
                }
                return _nppVersion;
            }

        }

        private static string _nppVersion;

        /// <summary>
        /// Get the number of instances of Notepad++ currently running
        /// </summary>
        /// <returns></returns>
        public static int NumberOfNppStarted {
            get {
                try {
                    return Process.GetProcesses().Count(clsProcess => clsProcess.ProcessName.Contains("notepad++"));
                } catch {
                    return 1;
                }
            }
        }

        /// <summary>
        /// Returns the configuration directory path e.g. /plugins/config/{AssemblyProduct}
        /// </summary>
        /// <returns></returns>
        public static string GetConfigDir() {
            if (string.IsNullOrEmpty(_configDir)) {
                var buffer = new StringBuilder(Win32Api.MaxPath);
                Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32Api.MaxPath, buffer);
                _configDir = Path.Combine(buffer.ToString(), AssemblyInfo.AssemblyProduct);
                Utils.CreateDirectory(_configDir);
            }
            return _configDir;
        }

        private static string _configDir;

        /// <summary>
        /// Leaves npp
        /// </summary>
        public static void Exit() {
            RunCommand(NppMenuCmd.FileExit);
        }

        /// <summary>
        /// Allows to execute one of Npp's command
        /// </summary>
        /// <param name="cmd"></param>
        public static void RunCommand(NppMenuCmd cmd) {
            Win32Api.SendMessage(HandleNpp, NppMsg.NPPM_MENUCOMMAND, 0, cmd);
        }
    }
}