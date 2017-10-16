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
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.Lib._3pUpdater;
using _3PA.MainFeatures;
using _3PA.WindowsCore;

namespace _3PA.NppCore {
    internal static partial class Npp {

        #region CurrentScintilla (scintilla instance)

        private static SciApi _primarySci;
        private static SciApi _secondarySci;
        private static int _curSciId;

        /// <summary>
        /// Returns the current scintilla instance, you can then use this object to send
        /// direct messages to scintilla
        /// </summary>
        public static SciApi CurrentSci { get; private set; }

        /// <summary>
        /// Updates the current scintilla handle for Npp's functions
        /// Called when the user changes the current document
        /// </summary>
        public static void UpdateCurrentSci() {
            if (_primarySci == null) {
                _primarySci = new SciApi(UnmanagedExports.NppData._scintillaMainHandle);
                _secondarySci = new SciApi(UnmanagedExports.NppData._scintillaSecondHandle);
            }

            long curScintilla;
            Win32Api.SendMessage(Handle, NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            _curSciId = ((int) curScintilla).ClampMax(1);
            CurrentSci = _curSciId == 0 ? _primarySci : _secondarySci;
        }

        /// <summary>
        /// Returns the current instance of scintilla used
        /// 0/1 corresponding to the main/seconday scintilla currently used
        /// </summary>
        public static int CurrentSciId {
            get { return _curSciId; }
        }

        /// <summary>
        /// Forces all next Sci commands to be sent to the 1st instance of scintilla
        /// </summary>
        public static void SetSciToPrimaryView() {
            _curSciId = 0;
            CurrentSci = _primarySci;
        }

        /// <summary>
        /// Forces all next Sci commands to be sent to the 2nd instance of scintilla
        /// </summary>
        public static void SetSciToSecondaryView() {
            _curSciId = 1;
            CurrentSci = _secondarySci;
        }

        #endregion

        #region General

        /// <summary>
        /// Gets the Notepad++ main window handle.
        /// </summary>
        public static IntPtr Handle {
            get { return UnmanagedExports.NppData._nppHandle; }
        }

        /// <summary>
        /// Is npp currently focused?
        /// </summary>
        public static bool IsNppWindowFocused {
            get { return WinApi.GetForegroundWindow() == Handle; }
        }

        /// <summary>
        /// Returns the screen on which npp is displayed (uses the point on the center of the application)
        /// </summary>
        public static Screen NppScreen {
            get {
                Rectangle nppRect = WinApi.GetWindowRect(Handle);
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
        public static IWin32Window Win32Handle {
            get { return new Win32Handle(Handle); }
        }

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

        #endregion

        #region Sessions

        /// <summary>
        /// Saves the current session into a file
        /// </summary>
        public static void SaveCurrentSession(string file) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_SAVECURRENTSESSION, 0, file);
        }

        /// <summary>
        /// Load a session from a file
        /// </summary>
        public static void LoadCurrentSession(string file) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_LOADSESSION, 0, file);
        }

        /// <summary>
        /// Gets the file path of each file in the session file, return
        /// the files separated by a new line
        /// </summary>
        /// <param name="sessionFilePath"></param>
        /// <returns></returns>
        public static List<string> GetFilesListFromSessionFile(string sessionFilePath) {
            var output = new List<string>();
            int nbFile = (int) Win32Api.SendMessage(Handle, NppMsg.NPPM_GETNBSESSIONFILES, 0, sessionFilePath);
            if (nbFile > 0) {
                using (Win32Api.UnmanagedStringArray cStrArray = new Win32Api.UnmanagedStringArray(nbFile, Win32Api.MaxPath)) {
                    if (Win32Api.SendMessage(Handle, NppMsg.NPPM_GETSESSIONFILES, cStrArray.NativePointer, sessionFilePath) != IntPtr.Zero)
                        output.AddRange(cStrArray.ManagedStringsUnicode);
                }
            }
            return output;
        }

        #endregion

        #region Open file and Go to

        /// <summary>
        /// Opens given file in notepad++
        /// </summary>
        public static bool OpenFile(string file) {
            if (!File.Exists(file)) {
                UserCommunication.Notify(@"Can't find/open the following file :<br>" + file, MessageImg.MsgHighImportance, "Warning", "File not found", 5);
                return false;
            }
            if (GetOpenedFiles.Contains(file)) {
                SwitchToDocument(file);
                return true;
            }
            return Win32Api.SendMessage(Handle, NppMsg.NPPM_DOOPEN, 0, file).ToInt64() > 0;
        }

        /// <summary>
        /// displays the input text into a new document
        /// </summary>
        public static void OpenNewDocument(string text) {
            RunCommand(NppMenuCmd.IDM_FILE_NEW);
            //GrabFocus();
        }

        /// <summary>
        /// Switch to given document
        /// </summary>
        public static void SwitchToDocument(string doc) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_SWITCHTOFILE, 0, doc);
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
            if (saveHistoric) {
                _goToHistory.Push(new Tuple<string, int, Point>(CurrentFileInfo.Path, Sci.FirstVisibleLine, Sci.CurrentPoint));
            }

            if (!String.IsNullOrEmpty(document) && !document.Equals(CurrentFileInfo.Path)) {
                if (GetOpenedFiles.Contains(document))
                    SwitchToDocument(document);
                else
                    OpenFile(document);
            }

            if (position >= 0) {
                Sci.GoToLine(Sci.LineFromPosition(position));
                Sci.SetSel(position);
            } else if (line >= 0) {
                Sci.GoToLine(line);
                if (column >= 0)
                    Sci.SetSel(Sci.GetPosFromLineColumn(line, column));
                else
                    Sci.SetSel(Sci.GetLine(line).Position);
            }

            Sci.GrabFocus();
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
                    Sci.FirstVisibleLine = lastPoint.Item2;
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in GoBackFromDefinition");
            }
        }

        #endregion

        #region Core npp methods

        /// <summary>
        /// Helper to add a clickable icon in the toolbar
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pluginId"></param>
        public static void SetToolbarImage(Bitmap image, int pluginId) {
            var tbIcons = new toolbarIcons {hToolbarBmp = image.GetHbitmap()};
            var pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32Api.SendMessage(Handle, NppMsg.NPPM_ADDTOOLBARICON, UnmanagedExports.NppFuncItems.Items[pluginId]._cmdID, pTbIcons);
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
            UnmanagedExports.NppFuncItems.Add(funcItem);
        }

        /// <summary>
        /// For each created dialog in your plugin, you should register it (and unregister while destroy it) to Notepad++ by using this message. 
        /// If this message is ignored, then your dialog won't react with the key stroke messages such as TAB key. 
        /// For the good functioning of your plugin dialog, you're recommended to not ignore this message.
        /// </summary>
        public static void RegisterToNpp(IntPtr handle) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_MODELESSDIALOG, (int) NppMsg.MODELESSDIALOGADD, handle);
        }

        /// <summary>
        /// For each created dialog in your plugin, you should register it (and unregister while destroy it) to Notepad++ by using this message. 
        /// If this message is ignored, then your dialog won't react with the key stroke messages such as TAB key. 
        /// For the good functioning of your plugin dialog, you're recommended to not ignore this message.
        /// </summary>
        public static void UnRegisterToNpp(IntPtr handle) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_MODELESSDIALOG, (int) NppMsg.MODELESSDIALOGREMOVE, handle);
        }

        /// <summary>
        /// This message passes the necessary data dockingData to Notepad++ in order to make your dialog dockable
        /// </summary>
        public static void RegisterDockableDialog(NppTbData nppTbData) {
            IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
            Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);
            Win32Api.SendMessage(Handle, NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);
        }

        /// <summary>
        /// This message is used for your plugin's dockable dialog. S
        /// Send this message to update (redraw) the dialog. hDlg is the handle of your dialog to be updated
        /// </summary>
        public static void RedrawDialog(IntPtr handle) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_DMMUPDATEDISPINFO, 0, handle);
        }

        /// <summary>
        /// Send this message to show the dialog. handle is the handle of your dialog to be shown
        /// </summary>
        public static void ShowDockableDialog(IntPtr handle) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_DMMSHOW, 0, handle);
        }

        /// <summary>
        /// Send this message to hide the dialog. handle is the handle of your dialog to be hidden.
        /// </summary>
        public static void HideDockableDialog(IntPtr handle) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_DMMHIDE, 0, handle);
        }

        /// <summary>
        /// Use this message to set/remove the check on menu item. cmdID is the command ID which corresponds to the menu item.
        /// </summary>
        public static void SetMenuItemCheck(int cmdId, bool @checked) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_SETMENUITEMCHECK, cmdId, @checked);
        }

        #endregion

        #region Opened files

        /// <summary>
        /// Gets the file path of each file currently opened in the primary view
        /// </summary>
        /// <returns></returns>
        public static List<string> OpenedFilesInPrimaryView {
            get { return GetOpenedFilesIn(NppMsg.PRIMARY_VIEW, NppMsg.NPPM_GETOPENFILENAMESPRIMARY); }
        }

        /// <summary>
        /// Gets the file path of each file currently opened in the secondary view
        /// </summary>
        /// <returns></returns>
        public static List<string> OpenedFilesInSecondaryView {
            get { return GetOpenedFilesIn(NppMsg.SECOND_VIEW, NppMsg.NPPM_GETOPENFILENAMESSECOND); }
        }

        /// <summary>
        /// Gets the file path of each file currently opened
        /// </summary>
        /// <returns></returns>
        public static List<string> GetOpenedFiles {
            get { return GetOpenedFilesIn(NppMsg.ALL_OPEN_FILES, NppMsg.NPPM_GETOPENFILENAMES); }
        }

        /// <summary>
        /// Gets the file path of each file currently opened in the secondary view
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOpenedFilesIn(NppMsg view, NppMsg mode) {
            var output = new List<string>();
            int nbFile = (int) Win32Api.SendMessage(Handle, NppMsg.NPPM_GETNBOPENFILES, 0, (int) view);
            using (Win32Api.UnmanagedStringArray cStrArray = new Win32Api.UnmanagedStringArray(nbFile, Win32Api.MaxPath)) {
                if (Win32Api.SendMessage(Handle, mode, cStrArray.NativePointer, nbFile) != IntPtr.Zero)
                    output.AddRange(cStrArray.ManagedStringsUnicode);
            }
            return output;
        }

        #endregion

        #region Doc index

        /// <summary>
        /// Sending this message to get the current index in the view that you indicates in iView : MAIN_VIEW or SUB_VIEW
        /// Returned value is -1 if the view is invisible (hidden), otherwise is the current index
        /// </summary>
        public static int CurrentDocIndexInView(int view) {
            return Win32Api.SendMessage(Handle, NppMsg.NPPM_GETCURRENTDOCINDEX, 0, view).ToInt32();
        }

        #endregion

        #region Buffers id

        /// <summary>
        /// Returns active document buffer ID
        /// </summary>
        public static int CurrentBufferId {
            get { return Win32Api.SendMessage(Handle, NppMsg.NPPM_GETCURRENTBUFFERID, 0, 0).ToInt32(); }
        }

        /// <summary>
        /// Get document's encoding from given buffer ID. 
        /// Returns value : if error -1, otherwise encoding number. 
        /// enum UniMode - uni8Bit 0, uniUTF8 1, uni16BE 2, uni16LE 3, uniCookie 4, uni7Bit 5, uni16BE_NoBOM 6, uni16LE_NoBOM 7
        /// </summary>
        public static int CurrentBufferEncoding {
            get { return (int) Win32Api.SendMessage(Handle, NppMsg.NPPM_GETBUFFERENCODING, CurrentBufferId, 0); }
        }

        #endregion

        #region Current lang

        /// <summary>
        /// Returns the current lang id
        /// </summary>
        public static int CurrentLangId {
            get {
                long langId;
                Win32Api.SendMessage(Handle, NppMsg.NPPM_GETCURRENTLANGTYPE, 0, out langId);
                return (int) langId;
            }
        }

        /// <summary>
        /// Returns the current REAL lang name (as it can be found in the langs.xml or api xml files...)
        /// </summary>
        public static string CurrentInternalLangName {
            get {
                var langName = CurrentLangName.ToLower();
                if (langName.StartsWith("udf - ")) {
                    return langName.Substring(6);
                }
                if (NppLangTypeInternal.Dictionary.ContainsKey(langName))
                    return NppLangTypeInternal.Dictionary[langName];
                return "normal";
            }
        }

        /// <summary>
        /// Returns the current Lang name
        /// </summary>
        public static string CurrentLangName {
            get {
                var currentLangId = CurrentLangId;
                var bufLenght = Win32Api.SendMessage(Handle, NppMsg.NPPM_GETLANGUAGENAME, currentLangId, 0);
                var buffer = new StringBuilder(bufLenght.ToInt32());
                Win32Api.SendMessage(Handle, NppMsg.NPPM_GETLANGUAGENAME, currentLangId, buffer);
                return buffer.ToString();
            }
        }

        /// <summary>
        /// Returns the current Lang description
        /// </summary>
        public static string CurrentLangDesc {
            get {
                var currentLangId = CurrentLangId;
                var bufLenght = Win32Api.SendMessage(Handle, NppMsg.NPPM_GETLANGUAGEDESC, currentLangId, 0);
                var buffer = new StringBuilder(bufLenght.ToInt32());
                Win32Api.SendMessage(Handle, NppMsg.NPPM_GETLANGUAGEDESC, currentLangId, buffer);
                return buffer.ToString();
            }
        }

        #endregion

        #region Npp properties

        /// <summary>
        /// full path of directory where located Notepad++ binary
        /// </summary>
        /// <returns></returns>
        public static string SoftwareInstallDirectory {
            get {
                var buffer = new StringBuilder(Win32Api.MaxPath);
                Win32Api.SendMessage(Handle, NppMsg.NPPM_GETNPPDIRECTORY, Win32Api.MaxPath, buffer);
                return buffer.ToString();
            }
        }

        /// <summary>
        /// returns npp.exe path
        /// </summary>
        /// <returns></returns>
        public static string SoftwareExePath {
            get { return Path.Combine(SoftwareInstallDirectory, "notepad++.exe"); }
        }

        /// <summary>
        /// Returns the current version of notepad++ (format vX.X.X)
        /// </summary>
        /// <returns></returns>
        public static string SoftwareVersion {
            get {
                if (string.IsNullOrEmpty(_nppVersion)) {
                    var nppVersion = Win32Api.SendMessage(Handle, NppMsg.NPPM_GETNPPVERSION, 0, 0).ToInt64();
                    var lowWord = (nppVersion & 0x0000FFFF).ToString();
                    _nppVersion = "v" + (nppVersion >> 16 & 0x0000FFFF) + "." + lowWord.Substring(0, 1) + "." + (String.IsNullOrEmpty(lowWord.Substring(1)) ? "0" : lowWord.Substring(1));
                }
                return _nppVersion;
            }
        }

        private static string _nppVersion;

        /// <summary>
        /// full path of directory where located Notepad++ binary
        /// </summary>
        /// <returns></returns>
        public static string SoftwarePluginConfigDirectory {
            get {
                var buffer = new StringBuilder(Win32Api.MaxPath);
                Win32Api.SendMessage(Handle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32Api.MaxPath, buffer);
                return buffer.ToString();
            }
        }

        /// <summary>
        /// Returns the configuration directory path e.g. /plugins/config/{AssemblyProduct}
        /// </summary>
        /// <returns></returns>
        public static string ConfigDirectory {
            get {
                if (string.IsNullOrEmpty(_configDir)) {
                    _configDir = Path.Combine(SoftwarePluginConfigDirectory, AssemblyInfo.AssemblyProduct);
                    Utils.CreateDirectory(_configDir);
                }
                return _configDir;
            }
        }

        private static string _configDir;

        /// <summary>
        /// The path to the choice file, containing the path to the cloud directory
        /// </summary>
        public static string FileNppCloudChoice {
            get { return Path.GetFullPath(Path.Combine(FolderNppDefaultBaseConf, @"cloud\choice")); }
        }

        /// <summary>
        /// default location of all the basic configuration files
        /// </summary>
        public static string FolderNppDefaultBaseConf {
            get { return Path.GetFullPath(Path.Combine(ConfigDirectory, @"..\..\..\")); }
        }

        /// <summary>
        /// Folder to plugins\Apis
        /// This is always taken in the notepad++ installation directory!
        /// </summary>
        public static string FolderNppAutocompApis {
            get { return Path.GetFullPath(Path.Combine(SoftwareInstallDirectory, @"plugins\APIs")); }
        }

        #endregion

        #region Misc commands

        /// <summary>
        /// Leaves npp
        /// </summary>
        public static void Exit() {
            RunCommand(NppMenuCmd.IDM_FILE_EXIT);
        }

        /// <summary>
        /// Restart npp
        /// </summary>
        public static void Restart() {
            _3PUpdater.Instance.ExecuteProgramAfterUpdate(SoftwareExePath);
            RunCommand(NppMenuCmd.IDM_FILE_EXIT);
        }

        /// <summary>
        /// Allows to execute one of Npp's command
        /// </summary>
        /// <param name="cmd"></param>
        public static void RunCommand(NppMenuCmd cmd) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_MENUCOMMAND, 0, cmd);
        }

        /// <summary>
        /// Reload given document
        /// </summary>
        public static void Reload(string path, bool askConfirmation) {
            Win32Api.SendMessage(Handle, NppMsg.NPPM_RELOADFILE, askConfirmation ? 1 : 0, path);
        }

        #endregion

    }
}