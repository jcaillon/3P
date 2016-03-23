#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (NotepadHelper.cs) is part of 3P.
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace _3PA.Interop {

    [StructLayout(LayoutKind.Sequential)]
    internal struct NppData {
        public IntPtr _nppHandle;
        public IntPtr _scintillaMainHandle;
        public IntPtr _scintillaSecondHandle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct FuncItem {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string _itemName;
        public Action _pFunc;
        public int _cmdID;
        public bool _init2Check;
        public ShortcutKey _pShKey;
    }

    internal class FuncItems : IDisposable {
        List<FuncItem> _funcItems;
        int _sizeFuncItem;
        List<IntPtr> _shortCutKeys;
        IntPtr _nativePointer;
        bool _disposed;
        public FuncItems() {
            _funcItems = new List<FuncItem>();
            _sizeFuncItem = Marshal.SizeOf(typeof(FuncItem));
            _shortCutKeys = new List<IntPtr>();
        }
        [DllImport("kernel32")]
        static extern void RtlMoveMemory(IntPtr Destination, IntPtr Source, int Length);
        public void Add(FuncItem funcItem) {
            int oldSize = _funcItems.Count * _sizeFuncItem;
            _funcItems.Add(funcItem);
            int newSize = _funcItems.Count * _sizeFuncItem;
            IntPtr newPointer = Marshal.AllocHGlobal(newSize);
            if (_nativePointer != IntPtr.Zero) {
                RtlMoveMemory(newPointer, _nativePointer, oldSize);
                Marshal.FreeHGlobal(_nativePointer);
            }
            IntPtr ptrPosNewItem = (IntPtr)((int)newPointer + oldSize);
            byte[] aB = Encoding.Unicode.GetBytes(funcItem._itemName + "\0");
            Marshal.Copy(aB, 0, ptrPosNewItem, aB.Length);
            ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 128);
            IntPtr p = (funcItem._pFunc != null) ? Marshal.GetFunctionPointerForDelegate(funcItem._pFunc) : IntPtr.Zero;
            Marshal.WriteIntPtr(ptrPosNewItem, p);
            ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + IntPtr.Size);
            Marshal.WriteInt32(ptrPosNewItem, funcItem._cmdID);
            ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 4);
            Marshal.WriteInt32(ptrPosNewItem, Convert.ToInt32(funcItem._init2Check));
            ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 4);
            if (funcItem._pShKey._key != 0) {
                IntPtr newShortCutKey = Marshal.AllocHGlobal(4);
                Marshal.StructureToPtr(funcItem._pShKey, newShortCutKey, false);
                Marshal.WriteIntPtr(ptrPosNewItem, newShortCutKey);
            } else Marshal.WriteIntPtr(ptrPosNewItem, IntPtr.Zero);
            _nativePointer = newPointer;
        }
        public void RefreshItems() {
            IntPtr ptrPosItem = _nativePointer;
            for (int i = 0; i < _funcItems.Count; i++) {
                FuncItem updatedItem = new FuncItem();
                updatedItem._itemName = _funcItems[i]._itemName;
                ptrPosItem = (IntPtr)((int)ptrPosItem + 128);
                updatedItem._pFunc = _funcItems[i]._pFunc;
                ptrPosItem = (IntPtr)((int)ptrPosItem + IntPtr.Size);
                updatedItem._cmdID = Marshal.ReadInt32(ptrPosItem);
                ptrPosItem = (IntPtr)((int)ptrPosItem + 4);
                updatedItem._init2Check = _funcItems[i]._init2Check;
                ptrPosItem = (IntPtr)((int)ptrPosItem + 4);
                updatedItem._pShKey = _funcItems[i]._pShKey;
                ptrPosItem = (IntPtr)((int)ptrPosItem + IntPtr.Size);
                _funcItems[i] = updatedItem;
            }
        }
        public IntPtr NativePointer { get { return _nativePointer; } }
        public List<FuncItem> Items { get { return _funcItems; } }
        public void Dispose() {
            if (!_disposed) {
                foreach (IntPtr ptr in _shortCutKeys) Marshal.FreeHGlobal(ptr);
                if (_nativePointer != IntPtr.Zero) Marshal.FreeHGlobal(_nativePointer);
                _disposed = true;
            }
        }
        ~FuncItems() {
            Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ShortcutKey {
        public ShortcutKey(string data) {
            //Ctrl+Shift+Alt+Key
            var parts = data.Split('+');
            _key = Convert.ToByte(Enum.Parse(typeof(Keys), parts.Last()));
            parts = parts.Take(parts.Length - 1).ToArray();
            _isCtrl = Convert.ToByte(parts.Contains("Ctrl"));
            _isShift = Convert.ToByte(parts.Contains("Shift"));
            _isAlt = Convert.ToByte(parts.Contains("Alt"));
        }
        public ShortcutKey(bool isCtrl, bool isAlt, bool isShift, Keys key) {
            // the types 'bool' and 'char' have a size of 1 byte only!
            _isCtrl = Convert.ToByte(isCtrl);
            _isAlt = Convert.ToByte(isAlt);
            _isShift = Convert.ToByte(isShift);
            _key = Convert.ToByte(key);
        }
        public byte _isCtrl;
        public byte _isAlt;
        public byte _isShift;
        public byte _key;
        public bool IsCtrl { get { return _isCtrl != 0; } }
        public bool IsShift { get { return _isShift != 0; } }
        public bool IsAlt { get { return _isAlt != 0; } }
        public bool IsSet {
            get { return _key != 0; }
        }

        public Keys Key {
            get { return (Keys) _key; }
        }

        public override string ToString() {
            return (IsCtrl ? "Ctrl+" : "") + (IsShift ? "Shift+" : "") + (IsAlt ? "Alt+" : "") + Enum.GetName(typeof(Keys), _key);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT {
        public RECT(int left, int top, int right, int bottom) {
            Left = left; Top = top; Right = right; Bottom = bottom;
        }
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct NppTbData {
        public IntPtr hClient;            // HWND: client Window Handle
        public string pszName;            // TCHAR*: name of plugin (shown in window)
        public int dlgID;                // int: a funcItem provides the function pointer to start a dialog. Please parse here these ID
        // user modifications
        public NppTbMsg uMask;                // UINT: mask params: look to above defines
        public uint hIconTab;            // HICON: icon for tabs
        public string pszAddInfo;        // TCHAR*: for plugin to display additional informations
        // internal data, do not use !!!
        public RECT rcFloat;            // RECT: floating position
        public int iPrevCont;           // int: stores the privious container (toggling between float and dock)
        public string pszModuleName;    // const TCHAR*: it's the plugin file name. It's used to identify the plugin
    }

    internal enum LangType {
        L_TEXT, L_PHP, L_C, L_CPP, L_CS, L_OBJC, L_JAVA, L_RC,
        L_HTML, L_XML, L_MAKEFILE, L_PASCAL, L_BATCH, L_INI, L_ASCII, L_USER,
        L_ASP, L_SQL, L_VB, L_JS, L_CSS, L_PERL, L_PYTHON, L_LUA,
        L_TEX, L_FORTRAN, L_BASH, L_FLASH, L_NSIS, L_TCL, L_LISP, L_SCHEME,
        L_ASM, L_DIFF, L_PROPS, L_PS, L_RUBY, L_SMALLTALK, L_VHDL, L_KIX, L_AU3,
        L_CAML, L_ADA, L_VERILOG, L_MATLAB, L_HASKELL, L_INNO, L_SEARCHRESULT,
        L_CMAKE, L_YAML, L_COBOL, L_GUI4CLI, L_D, L_POWERSHELL, L_R, L_JSP,
        // The end of enumated language type, so it should be always at the end
        L_EXTERNAL
    }

    [Flags]
    internal enum WinMsg {
        WM_COMMAND = 0x111
    }

    [Flags]
    internal enum NppTbMsg : uint {
        // styles for containers
        //CAPTION_TOP                = 1,
        //CAPTION_BOTTOM            = 0,
        // defines for docking manager
        CONT_LEFT = 0,
        CONT_RIGHT = 1,
        CONT_TOP = 2,
        CONT_BOTTOM = 3,
        DOCKCONT_MAX = 4,
        // mask params for plugins of internal dialogs
        DWS_ICONTAB = 0x00000001,            // Icon for tabs are available
        DWS_ICONBAR = 0x00000002,            // Icon for icon bar are available (currently not supported)
        DWS_ADDINFO = 0x00000004,            // Additional information are in use
        DWS_PARAMSALL = (DWS_ICONTAB | DWS_ICONBAR | DWS_ADDINFO),
        // default docking values for first call of plugin
        DWS_DF_CONT_LEFT = (CONT_LEFT << 28),    // default docking on left
        DWS_DF_CONT_RIGHT = (CONT_RIGHT << 28),    // default docking on right
        DWS_DF_CONT_TOP = (CONT_TOP << 28),        // default docking on top
        DWS_DF_CONT_BOTTOM = (CONT_BOTTOM << 28),    // default docking on bottom
        DWS_DF_FLOATING = 0x80000000            // default state is floating
    }

    internal enum NppMsg : uint {
        //Here you can find how to use these messages : http://notepad-plus.sourceforge.net/uk/plugins-HOWTO.php
        NPPMSG = (0x400/*WM_USER*/ + 1000),
        NPPM_GETCURRENTSCINTILLA = (NPPMSG + 4),
        NPPM_GETCURRENTLANGTYPE = (NPPMSG + 5),
        NPPM_SETCURRENTLANGTYPE = (NPPMSG + 6),
        NPPM_GETNBOPENFILES = (NPPMSG + 7),
        ALL_OPEN_FILES = 0,
        PRIMARY_VIEW = 1,
        SECOND_VIEW = 2,
        NPPM_GETOPENFILENAMES = (NPPMSG + 8),
        NPPM_MODELESSDIALOG = (NPPMSG + 12),
        MODELESSDIALOGADD = 0,
        MODELESSDIALOGREMOVE = 1,
        NPPM_GETNBSESSIONFILES = (NPPMSG + 13),
        NPPM_GETSESSIONFILES = (NPPMSG + 14),
        NPPM_SAVESESSION = (NPPMSG + 15),
        NPPM_SAVECURRENTSESSION = (NPPMSG + 16),
        //struct sessionInfo {
        //    TCHAR* sessionFilePathName;
        //    int nbFile;
        //    TCHAR** files;
        //};
        NPPM_GETOPENFILENAMESPRIMARY = (NPPMSG + 17),
        NPPM_GETOPENFILENAMESSECOND = (NPPMSG + 18),
        NPPM_CREATESCINTILLAHANDLE = (NPPMSG + 20),
        NPPM_DESTROYSCINTILLAHANDLE = (NPPMSG + 21),
        NPPM_GETNBUSERLANG = (NPPMSG + 22),
        NPPM_GETCURRENTDOCINDEX = (NPPMSG + 23),
        MAIN_VIEW = 0,
        SUB_VIEW = 1,
        NPPM_SETSTATUSBAR = (NPPMSG + 24),
        STATUSBAR_DOC_TYPE = 0,
        STATUSBAR_DOC_SIZE = 1,
        STATUSBAR_CUR_POS = 2,
        STATUSBAR_EOF_FORMAT = 3,
        STATUSBAR_UNICODE_TYPE = 4,
        STATUSBAR_TYPING_MODE = 5,
        NPPM_GETMENUHANDLE = (NPPMSG + 25),
        NPPPLUGINMENU = 0,
        NPPM_ENCODESCI = (NPPMSG + 26),
        //ascii file to unicode
        //int NPPM_ENCODESCI(MAIN_VIEW/SUB_VIEW, 0)
        //return new unicodeMode
        NPPM_DECODESCI = (NPPMSG + 27),
        //unicode file to ascii
        //int NPPM_DECODESCI(MAIN_VIEW/SUB_VIEW, 0)
        //return old unicodeMode
        NPPM_ACTIVATEDOC = (NPPMSG + 28),
        //void NPPM_ACTIVATEDOC(int view, int index2Activate)
        NPPM_LAUNCHFINDINFILESDLG = (NPPMSG + 29),
        //void NPPM_LAUNCHFINDINFILESDLG(TCHAR * dir2Search, TCHAR * filtre)
        NPPM_DMMSHOW = (NPPMSG + 30),
        NPPM_DMMHIDE = (NPPMSG + 31),
        NPPM_DMMUPDATEDISPINFO = (NPPMSG + 32),
        //void NPPM_DMMxxx(0, tTbData->hClient)
        NPPM_DMMREGASDCKDLG = (NPPMSG + 33),
        //void NPPM_DMMREGASDCKDLG(0, &tTbData)
        NPPM_LOADSESSION = (NPPMSG + 34),
        //void NPPM_LOADSESSION(0, const TCHAR* file name)
        NPPM_DMMVIEWOTHERTAB = (NPPMSG + 35),
        //void WM_DMM_VIEWOTHERTAB(0, tTbData->pszName)
        NPPM_RELOADFILE = (NPPMSG + 36),
        //BOOL NPPM_RELOADFILE(BOOL withAlert, TCHAR *filePathName2Reload)
        NPPM_SWITCHTOFILE = (NPPMSG + 37),
        //BOOL NPPM_SWITCHTOFILE(0, TCHAR *filePathName2switch)
        NPPM_SAVECURRENTFILE = (NPPMSG + 38),
        //BOOL NPPM_SAVECURRENTFILE(0, 0)
        NPPM_SAVEALLFILES = (NPPMSG + 39),
        //BOOL NPPM_SAVEALLFILES(0, 0)
        NPPM_SETMENUITEMCHECK = (NPPMSG + 40),
        //void WM_PIMENU_CHECK(UINT    funcItem[X]._cmdID, TRUE/FALSE)
        NPPM_ADDTOOLBARICON = (NPPMSG + 41),
        //void WM_ADDTOOLBARICON(UINT funcItem[X]._cmdID, toolbarIcons icon)
        //struct toolbarIcons {
        //    HBITMAP    hToolbarBmp;
        //    HICON    hToolbarIcon;
        //};
        NPPM_GETWINDOWSVERSION = (NPPMSG + 42),
        //winVer NPPM_GETWINDOWSVERSION(0, 0)
        NPPM_DMMGETPLUGINHWNDBYNAME = (NPPMSG + 43),
        //HWND WM_DMM_GETPLUGINHWNDBYNAME(const TCHAR *windowName, const TCHAR *moduleName)
        // if moduleName is NULL, then return value is NULL
        // if windowName is NULL, then the first found window handle which matches with the moduleName will be returned
        NPPM_MAKECURRENTBUFFERDIRTY = (NPPMSG + 44),
        //BOOL NPPM_MAKECURRENTBUFFERDIRTY(0, 0)
        NPPM_GETENABLETHEMETEXTUREFUNC = (NPPMSG + 45),
        //BOOL NPPM_GETENABLETHEMETEXTUREFUNC(0, 0)
        NPPM_GETPLUGINSCONFIGDIR = (NPPMSG + 46),
        //void NPPM_GETPLUGINSCONFIGDIR(int strLen, TCHAR *str)
        NPPM_MSGTOPLUGIN = (NPPMSG + 47),
        //BOOL NPPM_MSGTOPLUGIN(TCHAR *destModuleName, CommunicationInfo *info)
        // return value is TRUE when the message arrive to the destination plugins.
        // if destModule or info is NULL, then return value is FALSE
        //struct CommunicationInfo {
        //    long internalMsg;
        //    const TCHAR * srcModuleName;
        //    void * info; // defined by plugin
        //};
        NPPM_MENUCOMMAND = (NPPMSG + 48),
        //void NPPM_MENUCOMMAND(0, int cmdID)
        // uncomment //#include "menuCmdID.h"
        // in the beginning of this file then use the command symbols defined in "menuCmdID.h" file
        // to access all the Notepad++ menu command items
        NPPM_TRIGGERTABBARCONTEXTMENU = (NPPMSG + 49),
        //void NPPM_TRIGGERTABBARCONTEXTMENU(int view, int index2Activate)
        NPPM_GETNPPVERSION = (NPPMSG + 50),
        // int NPPM_GETNPPVERSION(0, 0)
        // return version
        // ex : v4.6
        // HIWORD(version) == 4
        // LOWORD(version) == 6
        NPPM_HIDETABBAR = (NPPMSG + 51),
        // BOOL NPPM_HIDETABBAR(0, BOOL hideOrNot)
        // if hideOrNot is set as TRUE then tab bar will be hidden
        // otherwise it'll be shown.
        // return value : the old status value
        NPPM_ISTABBARHIDDEN = (NPPMSG + 52),
        // BOOL NPPM_ISTABBARHIDDEN(0, 0)
        // returned value : TRUE if tab bar is hidden, otherwise FALSE
        NPPM_GETPOSFROMBUFFERID = (NPPMSG + 57),
        // INT NPPM_GETPOSFROMBUFFERID(INT bufferID, 0)
        // Return VIEW|INDEX from a buffer ID. -1 if the bufferID non existing
        //
        // VIEW takes 2 highest bits and INDEX (0 based) takes the rest (30 bits)
        // Here's the values for the view :
        //  MAIN_VIEW 0
        //  SUB_VIEW  1
        NPPM_GETFULLPATHFROMBUFFERID = (NPPMSG + 58),
        // INT NPPM_GETFULLPATHFROMBUFFERID(INT bufferID, TCHAR *fullFilePath)
        // Get full path file name from a bufferID.
        // Return -1 if the bufferID non existing, otherwise the number of TCHAR copied/to copy
        // User should call it with fullFilePath be NULL to get the number of TCHAR (not including the nul character),
        // allocate fullFilePath with the return values + 1, then call it again to get  full path file name
        NPPM_GETBUFFERIDFROMPOS = (NPPMSG + 59),
        //wParam: Position of document
        //lParam: View to use, 0 = Main, 1 = Secondary
        //Returns 0 if invalid
        NPPM_GETCURRENTBUFFERID = (NPPMSG + 60),
        //Returns active Buffer
        NPPM_RELOADBUFFERID = (NPPMSG + 61),
        //Reloads Buffer
        //wParam: Buffer to reload
        //lParam: 0 if no alert, else alert
        NPPM_GETBUFFERLANGTYPE = (NPPMSG + 64),
        //wParam: BufferID to get LangType from
        //lParam: 0
        //Returns as int, see LangType. -1 on error
        NPPM_SETBUFFERLANGTYPE = (NPPMSG + 65),
        //wParam: BufferID to set LangType of
        //lParam: LangType
        //Returns TRUE on success, FALSE otherwise
        //use int, see LangType for possible values
        //L_USER and L_EXTERNAL are not supported
        NPPM_GETBUFFERENCODING = (NPPMSG + 66),
        //wParam: BufferID to get encoding from
        //lParam: 0
        //returns as int, see UniMode. -1 on error
        NPPM_SETBUFFERENCODING = (NPPMSG + 67),
        //wParam: BufferID to set encoding of
        //lParam: format
        //Returns TRUE on success, FALSE otherwise
        //use int, see UniMode
        //Can only be done on new, unedited files
        NPPM_GETBUFFERFORMAT = (NPPMSG + 68),
        //wParam: BufferID to get format from
        //lParam: 0
        //returns as int, see formatType. -1 on error
        NPPM_SETBUFFERFORMAT = (NPPMSG + 69),
        //wParam: BufferID to set format of
        //lParam: format
        //Returns TRUE on success, FALSE otherwise
        //use int, see formatType
        /*
        NPPM_ADDREBAR = (NPPMSG + 57),
        // BOOL NPPM_ADDREBAR(0, REBARBANDINFO *)
        // Returns assigned ID in wID value of struct pointer
        NPPM_UPDATEREBAR = (NPPMSG + 58),
        // BOOL NPPM_ADDREBAR(INT ID, REBARBANDINFO *)
        //Use ID assigned with NPPM_ADDREBAR
        NPPM_REMOVEREBAR = (NPPMSG + 59),
        // BOOL NPPM_ADDREBAR(INT ID, 0)
        //Use ID assigned with NPPM_ADDREBAR
        */
        NPPM_HIDETOOLBAR = (NPPMSG + 70),
        // BOOL NPPM_HIDETOOLBAR(0, BOOL hideOrNot)
        // if hideOrNot is set as TRUE then tool bar will be hidden
        // otherwise it'll be shown.
        // return value : the old status value
        NPPM_ISTOOLBARHIDDEN = (NPPMSG + 71),
        // BOOL NPPM_ISTOOLBARHIDDEN(0, 0)
        // returned value : TRUE if tool bar is hidden, otherwise FALSE
        NPPM_HIDEMENU = (NPPMSG + 72),
        // BOOL NPPM_HIDEMENU(0, BOOL hideOrNot)
        // if hideOrNot is set as TRUE then menu will be hidden
        // otherwise it'll be shown.
        // return value : the old status value
        NPPM_ISMENUHIDDEN = (NPPMSG + 73),
        // BOOL NPPM_ISMENUHIDDEN(0, 0)
        // returned value : TRUE if menu is hidden, otherwise FALSE
        NPPM_HIDESTATUSBAR = (NPPMSG + 74),
        // BOOL NPPM_HIDESTATUSBAR(0, BOOL hideOrNot)
        // if hideOrNot is set as TRUE then STATUSBAR will be hidden
        // otherwise it'll be shown.
        // return value : the old status value
        NPPM_ISSTATUSBARHIDDEN = (NPPMSG + 75),
        // BOOL NPPM_ISSTATUSBARHIDDEN(0, 0)
        // returned value : TRUE if STATUSBAR is hidden, otherwise FALSE
        NPPM_GETSHORTCUTBYCMDID = (NPPMSG + 76),
        // BOOL NPPM_GETSHORTCUTBYCMDID(int cmdID, ShortcutKey *sk)
        // get your plugin command current mapped shortcut into sk via cmdID
        // You may need it after getting NPPN_READY notification
        // returned value : TRUE if this function call is successful and shorcut is enable, otherwise FALSE
        NPPM_DOOPEN = (NPPMSG + 77),
        // BOOL NPPM_DOOPEN(0, const TCHAR *fullPathName2Open)
        // fullPathName2Open indicates the full file path name to be opened.
        // The return value is TRUE (1) if the operation is successful, otherwise FALSE (0).
        NPPM_SAVECURRENTFILEAS = (NPPMSG + 78),
        // BOOL NPPM_SAVECURRENTFILEAS (BOOL asCopy, const TCHAR* filename)
        NPPM_GETCURRENTNATIVELANGENCODING = (NPPMSG + 79),
        // INT NPPM_GETCURRENTNATIVELANGENCODING(0, 0)
        // returned value : the current native language enconding
        NPPM_ALLOCATESUPPORTED = (NPPMSG + 80),
        // returns TRUE if NPPM_ALLOCATECMDID is supported
        // Use to identify if subclassing is necessary
        NPPM_ALLOCATECMDID = (NPPMSG + 81),
        // BOOL NPPM_ALLOCATECMDID(int numberRequested, int* startNumber)
        // sets startNumber to the initial command ID if successful
        // Returns: TRUE if successful, FALSE otherwise. startNumber will also be set to 0 if unsuccessful
        NPPM_ALLOCATEMARKER = (NPPMSG + 82),
        // BOOL NPPM_ALLOCATEMARKER(int numberRequested, int* startNumber)
        // sets startNumber to the initial command ID if successful
        // Allocates a marker number to a plugin
        // Returns: TRUE if successful, FALSE otherwise. startNumber will also be set to 0 if unsuccessful
        RUNCOMMAND_USER = (0x400/*WM_USER*/ + 3000),
        NPPM_GETFULLCURRENTPATH = (RUNCOMMAND_USER + FULL_CURRENT_PATH),
        NPPM_GETCURRENTDIRECTORY = (RUNCOMMAND_USER + CURRENT_DIRECTORY),
        NPPM_GETFILENAME = (RUNCOMMAND_USER + FILE_NAME),
        NPPM_GETNAMEPART = (RUNCOMMAND_USER + NAME_PART),
        NPPM_GETEXTPART = (RUNCOMMAND_USER + EXT_PART),
        NPPM_GETCURRENTWORD = (RUNCOMMAND_USER + CURRENT_WORD),
        NPPM_GETNPPDIRECTORY = (RUNCOMMAND_USER + NPP_DIRECTORY),
        // BOOL NPPM_GETXXXXXXXXXXXXXXXX(size_t strLen, TCHAR *str)
        // where str is the allocated TCHAR array,
        //         strLen is the allocated array size
        // The return value is TRUE when get generic_string operation success
        // Otherwise (allocated array size is too small) FALSE
        NPPM_GETCURRENTLINE = (RUNCOMMAND_USER + CURRENT_LINE),
        // INT NPPM_GETCURRENTLINE(0, 0)
        // return the caret current position line
        NPPM_GETCURRENTCOLUMN = (RUNCOMMAND_USER + CURRENT_COLUMN),
        // INT NPPM_GETCURRENTCOLUMN(0, 0)
        // return the caret current position column
        VAR_NOT_RECOGNIZED = 0,
        FULL_CURRENT_PATH = 1,
        CURRENT_DIRECTORY = 2,
        FILE_NAME = 3,
        NAME_PART = 4,
        EXT_PART = 5,
        CURRENT_WORD = 6,
        NPP_DIRECTORY = 7,
        CURRENT_LINE = 8,
        CURRENT_COLUMN = 9,
        // Notification code
        NPPN_FIRST = 1000,
        NPPN_READY = (NPPN_FIRST + 1), // To notify plugins that all the procedures of launchment of notepad++ are done.
        //scnNotification->nmhdr.code = NPPN_READY;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = 0;
        NPPN_TBMODIFICATION = (NPPN_FIRST + 2), // To notify plugins that toolbar icons can be registered
        //scnNotification->nmhdr.code = NPPN_TB_MODIFICATION;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = 0;
        NPPN_FILEBEFORECLOSE = (NPPN_FIRST + 3), // To notify plugins that the current file is about to be closed
        //scnNotification->nmhdr.code = NPPN_FILEBEFORECLOSE;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        NPPN_FILEOPENED = (NPPN_FIRST + 4), // To notify plugins that the current file is just opened
        //scnNotification->nmhdr.code = NPPN_FILEOPENED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        NPPN_FILECLOSED = (NPPN_FIRST + 5), // To notify plugins that the current file is just closed
        //scnNotification->nmhdr.code = NPPN_FILECLOSED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        NPPN_FILEBEFOREOPEN = (NPPN_FIRST + 6), // To notify plugins that the current file is about to be opened
        //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        NPPN_FILEBEFORESAVE = (NPPN_FIRST + 7), // To notify plugins that the current file is about to be saved
        //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        NPPN_FILESAVED = (NPPN_FIRST + 8), // To notify plugins that the current file is just saved
        //scnNotification->nmhdr.code = NPPN_FILESAVED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        NPPN_SHUTDOWN = (NPPN_FIRST + 9), // To notify plugins that Notepad++ is about to be shutdowned.
        //scnNotification->nmhdr.code = NPPN_SHUTDOWN;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = 0;
        NPPN_BUFFERACTIVATED = (NPPN_FIRST + 10), // To notify plugins that a buffer was activated (put to foreground).
        //scnNotification->nmhdr.code = NPPN_BUFFERACTIVATED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = activatedBufferID;
        NPPN_LANGCHANGED = (NPPN_FIRST + 11), // To notify plugins that the language in the current doc is just changed.
        //scnNotification->nmhdr.code = NPPN_LANGCHANGED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = currentBufferID;
        NPPN_WORDSTYLESUPDATED = (NPPN_FIRST + 12), // To notify plugins that user initiated a WordStyleDlg change.
        //scnNotification->nmhdr.code = NPPN_WORDSTYLESUPDATED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = currentBufferID;
        NPPN_SHORTCUTREMAPPED = (NPPN_FIRST + 13), // To notify plugins that plugin command shortcut is remapped.
        //scnNotification->nmhdr.code = NPPN_SHORTCUTSREMAPPED;
        //scnNotification->nmhdr.hwndFrom = ShortcutKeyStructurePointer;
        //scnNotification->nmhdr.idFrom = cmdID;
        //where ShortcutKeyStructurePointer is pointer of struct ShortcutKey:
        //struct ShortcutKey {
        //    bool _isCtrl;
        //    bool _isAlt;
        //    bool _isShift;
        //    UCHAR _key;
        //};
        NPPN_FILEBEFORELOAD = (NPPN_FIRST + 14), // To notify plugins that the current file is about to be loaded
        //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = NULL;
        NPPN_FILELOADFAILED = (NPPN_FIRST + 15),  // To notify plugins that file open operation failed
        //scnNotification->nmhdr.code = NPPN_FILEOPENFAILED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        NPPN_READONLYCHANGED = (NPPN_FIRST + 16),  // To notify plugins that current document change the readonly status,
        //scnNotification->nmhdr.code = NPPN_READONLYCHANGED;
        //scnNotification->nmhdr.hwndFrom = bufferID;
        //scnNotification->nmhdr.idFrom = docStatus;
        // where bufferID is BufferID
        //       docStatus can be combined by DOCSTAUS_READONLY and DOCSTAUS_BUFFERDIRTY
        DOCSTAUS_READONLY = 1,
        DOCSTAUS_BUFFERDIRTY = 2,
        NPPN_DOCORDERCHANGED = (NPPN_FIRST + 16)  // To notify plugins that document order is changed
        //scnNotification->nmhdr.code = NPPN_DOCORDERCHANGED;
        //scnNotification->nmhdr.hwndFrom = newIndex;
        //scnNotification->nmhdr.idFrom = BufferID;
    }

    internal enum NppNotif : uint {
        NPPN_FIRST = NppMsg.NPPN_FIRST,
        NPPN_READY = NppMsg.NPPN_READY,
        NPPN_TBMODIFICATION = NppMsg.NPPN_TBMODIFICATION,
        NPPN_FILEBEFORECLOSE = NppMsg.NPPN_FILEBEFORECLOSE,
        NPPN_FILEOPENED = NppMsg.NPPN_FILEOPENED,
        NPPN_FILECLOSED = NppMsg.NPPN_FILECLOSED,
        NPPN_FILEBEFOREOPEN = NppMsg.NPPN_FILEBEFOREOPEN,
        NPPN_FILEBEFORESAVE = NppMsg.NPPN_FILEBEFORESAVE,
        NPPN_FILESAVED = NppMsg.NPPN_FILESAVED,
        NPPN_SHUTDOWN = NppMsg.NPPN_SHUTDOWN,
        NPPN_BUFFERACTIVATED = NppMsg.NPPN_BUFFERACTIVATED,
        NPPN_LANGCHANGED = NppMsg.NPPN_LANGCHANGED,
        NPPN_WORDSTYLESUPDATED = NppMsg.NPPN_WORDSTYLESUPDATED,
        NPPN_SHORTCUTREMAPPED = NppMsg.NPPN_SHORTCUTREMAPPED,
        NPPN_FILEBEFORELOAD = NppMsg.NPPN_FILEBEFORELOAD,
        NPPN_FILELOADFAILED = NppMsg.NPPN_FILELOADFAILED,
        NPPN_READONLYCHANGED = NppMsg.NPPN_READONLYCHANGED,
        DOCSTAUS_READONLY = NppMsg.DOCSTAUS_READONLY,
        DOCSTAUS_BUFFERDIRTY = NppMsg.DOCSTAUS_BUFFERDIRTY,
        NPPN_DOCORDERCHANGED = NppMsg.NPPN_DOCORDERCHANGED
    }

    internal class Idm {
        public const int Base = 40000;
        public const int File = Base + 1000;
        public const int Edit = Base + 2000;
        public const int Search = Base + 3000;
        public const int View = Base + 4000;
        public const int ViewFold = View + 50;
        public const int ViewUnfold = View + 60;
        public const int Format = Base + 5000;
        public const int Lang = Base + 6000;
        public const int About = Base + 7000;
        public const int Setting = Base + 8000;
        public const int Execute = Base + 9000;
    }

    /// <summary>
    /// A built-in Notepad++ menu command.
    /// </summary>
    internal enum NppMenuCmd {

        #region File
        FileNew = (Idm.File + 1),
        FileOpen = (Idm.File + 2),
        FileClose = (Idm.File + 3),
        FileCloseAll = (Idm.File + 4),
        FileCloseAllButCurrent = (Idm.File + 5),
        FileSave = (Idm.File + 6),
        FileSaveAll = (Idm.File + 7),
        FileSaveAs = (Idm.File + 8),
        FileAsianLang = (Idm.File + 9),
        FilePrint = (Idm.File + 10),
        FilePrintNow = 1001,
        FileExit = (Idm.File + 11),
        FileLoadSession = (Idm.File + 12),
        FileSaveSession = (Idm.File + 13),
        FileReload = (Idm.File + 14),
        FileSaveCopyAs = (Idm.File + 15),
        FileDelete = (Idm.File + 16),
        FileRename = (Idm.File + 17),
        FileOpenAllRecentFiles = (Idm.Edit + 40),
        FileCleanRecentFileList = (Idm.Edit + 41),
        #endregion

        #region Edit
        EditCut = (Idm.Edit + 1),
        EditCopy = (Idm.Edit + 2),
        EditUndo = (Idm.Edit + 3),
        EditRedo = (Idm.Edit + 4),
        EditPaste = (Idm.Edit + 5),
        EditDelete = (Idm.Edit + 6),
        EditSelectAll = (Idm.Edit + 7),
        EditInsertTab = (Idm.Edit + 8),
        EditRemoveTab = (Idm.Edit + 9),
        EditDuplicateLine = (Idm.Edit + 10),
        EditTransposeLine = (Idm.Edit + 11),
        EditSplitLines = (Idm.Edit + 12),
        EditJoinLines = (Idm.Edit + 13),
        EditLineUp = (Idm.Edit + 14),
        EditLineDown = (Idm.Edit + 15),
        EditUppercase = (Idm.Edit + 16),
        EditLowercase = (Idm.Edit + 17),
        EditBlockComment = (Idm.Edit + 22),
        EditStreamComment = (Idm.Edit + 23),
        EditTrimTrailing = (Idm.Edit + 24),
        EditRtl = (Idm.Edit + 26),
        EditLtr = (Idm.Edit + 27),
        EditSetReadOnly = (Idm.Edit + 28),
        EditFullPathToClipboard = (Idm.Edit + 29),
        EditFileNameToClipboard = (Idm.Edit + 30),
        EditCurrentDirToClipboard = (Idm.Edit + 31),
        EditClearReadOnly = (Idm.Edit + 33),
        EditColumnMode = (Idm.Edit + 34),
        EditBlockCommentSet = (Idm.Edit + 35),
        EditBlockUncomment = (Idm.Edit + 36),
        EditAutoComplete = (50000 + 0),
        EditAutoCompleteCurrentFile = (50000 + 1),
        EditFuncionCallTip = (50000 + 2),
        #endregion

        #region Search
        SearchFind = (Idm.Search + 1),
        SearchFindNext = (Idm.Search + 2),
        SearchReplace = (Idm.Search + 3),
        SearchGoToLine = (Idm.Search + 4),
        SearchToggleBookmark = (Idm.Search + 5),
        SearchNextBookmark = (Idm.Search + 6),
        SearchPrevBookmark = (Idm.Search + 7),
        SearchClearBookmarks = (Idm.Search + 8),
        SearchCutMarkedLines = (Idm.Search + 18),
        SearchCopyMarkedLines = (Idm.Search + 19),
        SearchPasteMarkedLines = (Idm.Search + 20),
        SearchDeleteMarkedLines = (Idm.Search + 21),
        SearchGoToMatchingBrace = (Idm.Search + 9),
        SearchFindPrev = (Idm.Search + 10),
        SearchFindIncrement = (Idm.Search + 11),
        SearchFindInFiles = (Idm.Search + 13),
        SearchVolatileFindNext = (Idm.Search + 14),
        SearchVolatileFindPrev = (Idm.Search + 15),
        SearchMarkAllExt1 = (Idm.Search + 22),
        SearchUnmarkAllExt1 = (Idm.Search + 23),
        SearchMarkAllExt2 = (Idm.Search + 24),
        SearchUnmarkAllExt2 = (Idm.Search + 25),
        SearchMarkAllExt3 = (Idm.Search + 26),
        SearchUnmarkAllExt3 = (Idm.Search + 27),
        SearchMarkAllExt4 = (Idm.Search + 28),
        SearchUnmarkAllExt4 = (Idm.Search + 29),
        SearchMarkAllExt5 = (Idm.Search + 30),
        SearchUnmarkAllExt5 = (Idm.Search + 31),
        SearchClearAllMarks = (Idm.Search + 32),
        #endregion

        #region View
        //IDM.ViewToolbarHide = (IDM.View + 1),
        ViewToolbarReduce = (Idm.View + 2),
        ViewToolbarEnlarge = (Idm.View + 3),
        ViewToolbarStandard = (Idm.View + 4),
        ViewReduceTabBar = (Idm.View + 5),
        ViewLockTabBar = (Idm.View + 6),
        ViewDrawTabBarTopBar = (Idm.View + 7),
        ViewDrawTabBarInactiveTab = (Idm.View + 8),
        ViewPostIt = (Idm.View + 9),
        ViewToggleFoldAll = (Idm.View + 10),
        ViewUserDialog = (Idm.View + 11),
        ViewLineNumber = (Idm.View + 12),
        ViewSymbolMargin = (Idm.View + 13),
        ViewFolderMargin = (Idm.View + 14),
        ViewFolderMarginSimple = (Idm.View + 15),
        ViewFolderMarginArrow = (Idm.View + 16),
        ViewFolderMarginCircle = (Idm.View + 17),
        ViewFolderMarginBox = (Idm.View + 18),
        ViewAllChars = (Idm.View + 19),
        ViewIndentGuide = (Idm.View + 20),
        ViewCurLineHilighting = (Idm.View + 21),
        ViewWrap = (Idm.View + 22),
        ViewZoomIn = (Idm.View + 23),
        ViewZoomOut = (Idm.View + 24),
        ViewTabSpace = (Idm.View + 25),
        ViewEol = (Idm.View + 26),
        ViewEdgeLine = (Idm.View + 27),
        ViewEdgeBackground = (Idm.View + 28),
        ViewToggleUnfoldAll = (Idm.View + 29),
        ViewFoldCurrent = (Idm.View + 30),
        ViewUnfoldCurrent = (Idm.View + 31),
        ViewFullScreenToggle = (Idm.View + 32),
        ViewZoomRestore = (Idm.View + 33),
        ViewAlwaysOnTop = (Idm.View + 34),
        ViewSyncScrollV = (Idm.View + 35),
        ViewSyncScrollH = (Idm.View + 36),
        ViewEdgeNone = (Idm.View + 37),
        ViewDrawTabBarCloseBottUn = (Idm.View + 38),
        ViewDrawTabBarDblClkToClose = (Idm.View + 39),
        ViewRefreshTabBar = (Idm.View + 40),
        ViewWrapSymbol = (Idm.View + 41),
        ViewHideLines = (Idm.View + 42),
        ViewDrawTabBarVertical = (Idm.View + 43),
        ViewDrawTabBarMultiLine = (Idm.View + 44),
        ViewDocChangeMargin = (Idm.View + 45),
        ViewFold1 = (Idm.ViewFold + 1),
        ViewFold2 = (Idm.ViewFold + 2),
        ViewFold3 = (Idm.ViewFold + 3),
        ViewFold4 = (Idm.ViewFold + 4),
        ViewFold5 = (Idm.ViewFold + 5),
        ViewFold6 = (Idm.ViewFold + 6),
        ViewFold7 = (Idm.ViewFold + 7),
        ViewFold8 = (Idm.ViewFold + 8),
        ViewUnfold1 = (Idm.ViewUnfold + 1),
        ViewUnfold2 = (Idm.ViewUnfold + 2),
        ViewUnfold3 = (Idm.ViewUnfold + 3),
        ViewUnfold4 = (Idm.ViewUnfold + 4),
        ViewUnfold5 = (Idm.ViewUnfold + 5),
        ViewUnfold6 = (Idm.ViewUnfold + 6),
        ViewUnfold7 = (Idm.ViewUnfold + 7),
        ViewUnfold8 = (Idm.ViewUnfold + 8),
        ViewGoToAnotherView = 10001,
        ViewCloneToAnotherView = 10002,
        ViewGoToNewInstance = 10003,
        ViewLoadInNewInstance = 10004,
        ViewSwitchToOtherView = (Idm.View + 72),
        #endregion

        #region Format
        FormatToDos = (Idm.Format + 1),
        FormatToUnix = (Idm.Format + 2),
        FormatToMac = (Idm.Format + 3),
        FormatAnsi = (Idm.Format + 4),
        FormatUtf8 = (Idm.Format + 5),
        FormatUnicodeBigEndian = (Idm.Format + 6),
        FormatUnicodeLittleEndian = (Idm.Format + 7),
        FormatAsUtf8 = (Idm.Format + 8),
        FormatConvertAnsi = (Idm.Format + 9),
        FormatConvertAsUtf8 = (Idm.Format + 10),
        FormatConvertUtf8 = (Idm.Format + 11),
        FormatConvertUnicodeBigEndian = (Idm.Format + 12),
        FormatConvertUnicodeLittleEndian = (Idm.Format + 13),
        #endregion

        #region Language
        LangStyleConfigDialog = (Idm.Lang + 1),
        LangC = (Idm.Lang + 2),
        LangCpp = (Idm.Lang + 3),
        LangJava = (Idm.Lang + 4),
        LangHtml = (Idm.Lang + 5),
        LangXml = (Idm.Lang + 6),
        LangJs = (Idm.Lang + 7),
        LangPhp = (Idm.Lang + 8),
        LangAsp = (Idm.Lang + 9),
        LangCss = (Idm.Lang + 10),
        LangPascal = (Idm.Lang + 11),
        LangPython = (Idm.Lang + 12),
        LangPerl = (Idm.Lang + 13),
        LangObjC = (Idm.Lang + 14),
        LangAscii = (Idm.Lang + 15),
        LangText = (Idm.Lang + 16),
        LangRc = (Idm.Lang + 17),
        LangMakeFile = (Idm.Lang + 18),
        LangIni = (Idm.Lang + 19),
        LangSql = (Idm.Lang + 20),
        LangVb = (Idm.Lang + 21),
        LangBatch = (Idm.Lang + 22),
        LangCs = (Idm.Lang + 23),
        LangLua = (Idm.Lang + 24),
        LangTex = (Idm.Lang + 25),
        LangFortran = (Idm.Lang + 26),
        LangSh = (Idm.Lang + 27),
        LangFlash = (Idm.Lang + 28),
        LangNsis = (Idm.Lang + 29),
        LangTcl = (Idm.Lang + 30),
        LangList = (Idm.Lang + 31),
        LangScheme = (Idm.Lang + 32),
        LangAsm = (Idm.Lang + 33),
        LangDiff = (Idm.Lang + 34),
        LangProps = (Idm.Lang + 35),
        LangPs = (Idm.Lang + 36),
        LangRuby = (Idm.Lang + 37),
        LangSmallTalk = (Idm.Lang + 38),
        LangVhdl = (Idm.Lang + 39),
        LangCaml = (Idm.Lang + 40),
        LangKix = (Idm.Lang + 41),
        LangAda = (Idm.Lang + 42),
        LangVerilog = (Idm.Lang + 43),
        LangAu3 = (Idm.Lang + 44),
        LangMatlab = (Idm.Lang + 45),
        LangHaskell = (Idm.Lang + 46),
        LangInno = (Idm.Lang + 47),
        LangCMake = (Idm.Lang + 48),
        LangYaml = (Idm.Lang + 49),
        LangExternal = (Idm.Lang + 50),
        LangExternalLimit = (Idm.Lang + 79),
        LangUser = (Idm.Lang + 80),
        LangUserLimit = (Idm.Lang + 110),
        #endregion

        #region About
        AboutHomePage = (Idm.About + 1),
        AboutProjectPage = (Idm.About + 2),
        AboutOnlineHelp = (Idm.About + 3),
        AboutForum = (Idm.About + 4),
        AboutPluginsHome = (Idm.About + 5),
        AboutUpdateNpp = (Idm.About + 6),
        AboutWikiFaq = (Idm.About + 7),
        AboutHelp = (Idm.About + 8),
        #endregion

        #region Settings
        SettingTabSize = (Idm.Setting + 1),
        SettingTabReplaceSpace = (Idm.Setting + 2),
        SettingHistorySize = (Idm.Setting + 3),
        SettingEdgeSize = (Idm.Setting + 4),
        SettingImportPlugin = (Idm.Setting + 5),
        SettingImportStyleThemes = (Idm.Setting + 6),
        SettingTrayIcon = (Idm.Setting + 8),
        SettingShortcutMapper = (Idm.Setting + 9),
        SettingRememberLastSession = (Idm.Setting + 10),
        SettingPreferences = (Idm.Setting + 11),
        SettingAutoCnbChar = (Idm.Setting + 15),
        #endregion

        #region Macro
        MacroStartRecording = (Idm.Edit + 18),
        MacroStopRecording = (Idm.Edit + 19),
        MacroPLaybackRecorded = (Idm.Edit + 21),
        MacroSaveCurrent = (Idm.Edit + 25),
        MacroRunMultiDialog = (Idm.Edit + 32)
        #endregion
    }

    [Flags]
    internal enum DockMgrMsg : uint {
        IDB_CLOSE_DOWN = 137,
        IDB_CLOSE_UP = 138,
        IDD_CONTAINER_DLG = 139,
        IDC_TAB_CONT = 1027,
        IDC_CLIENT_TAB = 1028,
        IDC_BTN_CAPTION = 1050,
        DMM_MSG = 0x5000,
        DMM_CLOSE = (DMM_MSG + 1),
        DMM_DOCK = (DMM_MSG + 2),
        DMM_FLOAT = (DMM_MSG + 3),
        DMM_DOCKALL = (DMM_MSG + 4),
        DMM_FLOATALL = (DMM_MSG + 5),
        DMM_MOVE = (DMM_MSG + 6),
        DMM_UPDATEDISPINFO = (DMM_MSG + 7),
        DMM_GETIMAGELIST = (DMM_MSG + 8),
        DMM_GETICONPOS = (DMM_MSG + 9),
        DMM_DROPDATA = (DMM_MSG + 10),
        DMM_MOVE_SPLITTER = (DMM_MSG + 11),
        DMM_CANCEL_MOVE = (DMM_MSG + 12),
        DMM_LBUTTONUP = (DMM_MSG + 13),
        DMN_FIRST = 1050,
        DMN_CLOSE = (DMN_FIRST + 1),
        //nmhdr.code = DWORD(DMN_CLOSE, 0));
        //nmhdr.hwndFrom = hwndNpp;
        //nmhdr.idFrom = ctrlIdNpp;
        DMN_DOCK = (DMN_FIRST + 2),
        DMN_FLOAT = (DMN_FIRST + 3)
        //nmhdr.code = DWORD(DMN_XXX, int newContainer);
        //nmhdr.hwndFrom = hwndNpp;
        //nmhdr.idFrom = ctrlIdNpp;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct toolbarIcons {
        public IntPtr hToolbarBmp;
        public IntPtr hToolbarIcon;
    }
}
