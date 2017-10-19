#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (SciHeader.cs) is part of 3P.
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
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace _3PA.NppCore {

    /*
     * THIS FILE TARGETS NOTEPAD++ 7.3.3
     * Extracted from scintilla\include\Scintilla.iface
     */

    #region notifications and other structures

    /// <summary>
    /// Structure for notifications coming from scintilla as well as notepad++ 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SCNotification {
        public Sci_NotifyHeader nmhdr;
        public int position; /* SCN_STYLENEEDED, SCN_MODIFIED, SCN_DWELLSTART, SCN_DWELLEND */
        public int ch; /* SCN_CHARADDED, SCN_KEY */
        public int modifiers; /* SCN_KEY */
        public int modificationType; /* SCN_MODIFIED */
        public IntPtr text; /* SCN_MODIFIED, SCN_USERLISTSELECTION, SCN_AUTOCSELECTION */
        public int length; /* SCN_MODIFIED */
        public int linesAdded; /* SCN_MODIFIED */
        public int message; /* SCN_MACRORECORD */
        public IntPtr wParam; /* SCN_MACRORECORD */
        public IntPtr lParam; /* SCN_MACRORECORD */
        public int line; /* SCN_MODIFIED */
        public int foldLevelNow; /* SCN_MODIFIED */
        public int foldLevelPrev; /* SCN_MODIFIED */
        public int margin; /* SCN_MARGINCLICK */
        public int listType; /* SCN_USERLISTSELECTION */
        public int x; /* SCN_DWELLSTART, SCN_DWELLEND */
        public int y; /* SCN_DWELLSTART, SCN_DWELLEND */
        public int token; /* SCN_MODIFIED with SC_MOD_CONTAINER */
        public int annotationLinesAdded; /* SC_MOD_CHANGEANNOTATION */
        public int updated; /* SCN_UPDATEUI */
        public int listCompletionMethod; /* SCN_AUTOCSELECTION, SCN_AUTOCCOMPLETED, SCN_USERLISTSELECTION */

        public SCNotification(uint code) : this() {
            nmhdr = new Sci_NotifyHeader {
                code = code
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_NotifyHeader {
        /* Compatible with Windows NMHDR.
         * hwndFrom is really an environment specific window handle or pointer
         * but most clients of Scintilla.h do not have this type visible. */
        public IntPtr hwndFrom;
        public IntPtr idFrom;
        public uint code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_CharacterRange {
        public Sci_CharacterRange(int cpmin, int cpmax) {
            cpMin = cpmin;
            cpMax = cpmax;
        }

        public int cpMin;
        public int cpMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_TextRange {
        public Sci_CharacterRange chrg;
        public IntPtr lpstrText;
    }

    public class Sci_TextToFind : IDisposable {
        _Sci_TextToFind _sciTextToFind;
        IntPtr _ptrSciTextToFind;
        bool _disposed;

        public Sci_TextToFind(Sci_CharacterRange chrRange, string searchText) {
            _sciTextToFind.chrg = chrRange;
            _sciTextToFind.lpstrText = Marshal.StringToHGlobalAnsi(searchText);
        }

        public Sci_TextToFind(int cpmin, int cpmax, string searchText) {
            _sciTextToFind.chrg.cpMin = cpmin;
            _sciTextToFind.chrg.cpMax = cpmax;
            _sciTextToFind.lpstrText = Marshal.StringToHGlobalAnsi(searchText);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct _Sci_TextToFind {
            public Sci_CharacterRange chrg;
            public IntPtr lpstrText;
            public Sci_CharacterRange chrgText;
        }

        public IntPtr NativePointer {
            get {
                _initNativeStruct();
                return _ptrSciTextToFind;
            }
        }

        public string lpstrText {
            set {
                _freeNativeString();
                _sciTextToFind.lpstrText = Marshal.StringToHGlobalAnsi(value);
            }
        }

        public Sci_CharacterRange chrg {
            get {
                _readNativeStruct();
                return _sciTextToFind.chrg;
            }
            set {
                _sciTextToFind.chrg = value;
                _initNativeStruct();
            }
        }

        public Sci_CharacterRange chrgText {
            get {
                _readNativeStruct();
                return _sciTextToFind.chrgText;
            }
        }

        void _initNativeStruct() {
            if (_ptrSciTextToFind == IntPtr.Zero)
                _ptrSciTextToFind = Marshal.AllocHGlobal(Marshal.SizeOf(_sciTextToFind));
            Marshal.StructureToPtr(_sciTextToFind, _ptrSciTextToFind, false);
        }

        void _readNativeStruct() {
            if (_ptrSciTextToFind != IntPtr.Zero)
                _sciTextToFind = (_Sci_TextToFind) Marshal.PtrToStructure(_ptrSciTextToFind, typeof(_Sci_TextToFind));
        }

        void _freeNativeString() {
            if (_sciTextToFind.lpstrText != IntPtr.Zero) Marshal.FreeHGlobal(_sciTextToFind.lpstrText);
        }

        public void Dispose() {
            if (!_disposed) {
                _freeNativeString();
                if (_ptrSciTextToFind != IntPtr.Zero) Marshal.FreeHGlobal(_ptrSciTextToFind);
                _disposed = true;
            }
        }

        ~Sci_TextToFind() {
            Dispose();
        }
    }

    #endregion

    #region SciNotif enum

    public enum SciNotif : uint {
        SCN_NOTIF_BEGIN = SCN_STYLENEEDED - 1,
        SCN_NOTIF_END = SCN_AUTOCCOMPLETED + 1,

        SCN_STYLENEEDED = SciMsg.SCN_STYLENEEDED,
        SCN_CHARADDED = SciMsg.SCN_CHARADDED,
        SCN_SAVEPOINTREACHED = SciMsg.SCN_SAVEPOINTREACHED,
        SCN_SAVEPOINTLEFT = SciMsg.SCN_SAVEPOINTLEFT,
        SCN_MODIFYATTEMPTRO = SciMsg.SCN_MODIFYATTEMPTRO,
        SCN_KEY = SciMsg.SCN_KEY,
        SCN_DOUBLECLICK = SciMsg.SCN_DOUBLECLICK,
        SCN_UPDATEUI = SciMsg.SCN_UPDATEUI,
        SCN_MODIFIED = SciMsg.SCN_MODIFIED,
        SCN_MACRORECORD = SciMsg.SCN_MACRORECORD,
        SCN_MARGINCLICK = SciMsg.SCN_MARGINCLICK,
        SCN_NEEDSHOWN = SciMsg.SCN_NEEDSHOWN,
        SCN_PAINTED = SciMsg.SCN_PAINTED,
        SCN_USERLISTSELECTION = SciMsg.SCN_USERLISTSELECTION,
        SCN_URIDROPPED = SciMsg.SCN_URIDROPPED,
        SCN_DWELLSTART = SciMsg.SCN_DWELLSTART,
        SCN_DWELLEND = SciMsg.SCN_DWELLEND,
        SCN_ZOOM = SciMsg.SCN_ZOOM,
        SCN_HOTSPOTCLICK = SciMsg.SCN_HOTSPOTCLICK,
        SCN_HOTSPOTDOUBLECLICK = SciMsg.SCN_HOTSPOTDOUBLECLICK,
        SCN_CALLTIPCLICK = SciMsg.SCN_CALLTIPCLICK,
        SCN_AUTOCSELECTION = SciMsg.SCN_AUTOCSELECTION,
        SCN_INDICATORCLICK = SciMsg.SCN_INDICATORCLICK,
        SCN_INDICATORRELEASE = SciMsg.SCN_INDICATORRELEASE,
        SCN_AUTOCCANCELLED = SciMsg.SCN_AUTOCCANCELLED,
        SCN_AUTOCCHARDELETED = SciMsg.SCN_AUTOCCHARDELETED,
        SCN_HOTSPOTRELEASECLICK = SciMsg.SCN_HOTSPOTRELEASECLICK,
        SCN_FOCUSIN = SciMsg.SCN_FOCUSIN,
        SCN_FOCUSOUT = SciMsg.SCN_FOCUSOUT,
        SCN_AUTOCCOMPLETED = SciMsg.SCN_AUTOCCOMPLETED
    }

    [Flags]
    public enum SciModificationMod : uint {
        SC_MOD_INSERTTEXT = 0x1,
        SC_MOD_DELETETEXT = 0x2,
        SC_MOD_CHANGESTYLE = 0x4,
        SC_MOD_CHANGEFOLD = 0x8,
        SC_PERFORMED_USER = 0x10,
        SC_PERFORMED_UNDO = 0x20,
        SC_PERFORMED_REDO = 0x40,
        SC_MULTISTEPUNDOREDO = 0x80,
        SC_LASTSTEPINUNDOREDO = 0x100,
        SC_MOD_CHANGEMARKER = 0x200,
        SC_MOD_BEFOREINSERT = 0x400,
        SC_MOD_BEFOREDELETE = 0x800,
        SC_MULTILINEUNDOREDO = 0x1000,
        SC_STARTACTION = 0x2000,
        SC_MOD_CHANGEINDICATOR = 0x4000,
        SC_MOD_CHANGELINESTATE = 0x8000,
        SC_MOD_CHANGEMARGIN = 0x10000,
        SC_MOD_CHANGEANNOTATION = 0x20000,
        SC_MOD_CONTAINER = 0x40000,
        SC_MOD_LEXERSTATE = 0x80000,
        SC_MOD_INSERTCHECK = 0x100000,
        SC_MOD_CHANGETABSTOPS = 0x200000,
        SC_MODEVENTMASKALL = 0x3FFFFF
    }

    #endregion
 
    #region SciLexicalStates

    [Flags]
    public enum SciLexicalStates : uint {
        // Ada
        SCE_ADA_DEFAULT = 0,
        SCE_ADA_WORD = 1,
        SCE_ADA_IDENTIFIER = 2,
        SCE_ADA_NUMBER = 3,
        SCE_ADA_DELIMITER = 4,
        SCE_ADA_CHARACTER = 5,
        SCE_ADA_CHARACTEREOL = 6,
        SCE_ADA_STRING = 7,
        SCE_ADA_STRINGEOL = 8,
        SCE_ADA_LABEL = 9,
        SCE_ADA_COMMENTLINE = 10,
        SCE_ADA_ILLEGAL = 11,

        // ASM
        SCE_ASM_DEFAULT = 0,
        SCE_ASM_COMMENT = 1,
        SCE_ASM_NUMBER = 2,
        SCE_ASM_STRING = 3,
        SCE_ASM_OPERATOR = 4,
        SCE_ASM_IDENTIFIER = 5,
        SCE_ASM_CPUINSTRUCTION = 6,
        SCE_ASM_MATHINSTRUCTION = 7,
        SCE_ASM_REGISTER = 8,
        SCE_ASM_DIRECTIVE = 9,
        SCE_ASM_DIRECTIVEOPERAND = 10,
        SCE_ASM_COMMENTBLOCK = 11,
        SCE_ASM_CHARACTER = 12,
        SCE_ASM_STRINGEOL = 13,
        SCE_ASM_EXTINSTRUCTION = 14,
        SCE_ASM_COMMENTDIRECTIVE = 15,

        // Batch
        SCE_BAT_DEFAULT = 0,
        SCE_BAT_COMMENT = 1,
        SCE_BAT_WORD = 2,
        SCE_BAT_LABEL = 3,
        SCE_BAT_HIDE = 4,
        SCE_BAT_COMMAND = 5,
        SCE_BAT_IDENTIFIER = 6,
        SCE_BAT_OPERATOR = 7,

        // CPP
        SCE_C_DEFAULT = 0,
        SCE_C_COMMENT = 1,
        SCE_C_COMMENTLINE = 2,
        SCE_C_COMMENTDOC = 3,
        SCE_C_NUMBER = 4,
        SCE_C_WORD = 5,
        SCE_C_STRING = 6,
        SCE_C_CHARACTER = 7,
        SCE_C_UUID = 8,
        SCE_C_PREPROCESSOR = 9,
        SCE_C_OPERATOR = 10,
        SCE_C_IDENTIFIER = 11,
        SCE_C_STRINGEOL = 12,
        SCE_C_VERBATIM = 13,
        SCE_C_REGEX = 14,
        SCE_C_COMMENTLINEDOC = 15,
        SCE_C_WORD2 = 16,
        SCE_C_COMMENTDOCKEYWORD = 17,
        SCE_C_COMMENTDOCKEYWORDERROR = 18,
        SCE_C_GLOBALCLASS = 19,
        SCE_C_STRINGRAW = 20,
        SCE_C_TRIPLEVERBATIM = 21,
        SCE_C_HASHQUOTEDSTRING = 22,
        SCE_C_PREPROCESSORCOMMENT = 23,
        SCE_C_PREPROCESSORCOMMENTDOC = 24,
        SCE_C_USERLITERAL = 25,
        SCE_C_TASKMARKER = 26,
        SCE_C_ESCAPESEQUENCE = 27,

        // CSS
        SCE_CSS_DEFAULT = 0,
        SCE_CSS_TAG = 1,
        SCE_CSS_CLASS = 2,
        SCE_CSS_PSEUDOCLASS = 3,
        SCE_CSS_UNKNOWN_PSEUDOCLASS = 4,
        SCE_CSS_OPERATOR = 5,
        SCE_CSS_IDENTIFIER = 6,
        SCE_CSS_UNKNOWN_IDENTIFIER = 7,
        SCE_CSS_VALUE = 8,
        SCE_CSS_COMMENT = 9,
        SCE_CSS_ID = 10,
        SCE_CSS_IMPORTANT = 11,
        SCE_CSS_DIRECTIVE = 12,
        SCE_CSS_DOUBLESTRING = 13,
        SCE_CSS_SINGLESTRING = 14,
        SCE_CSS_IDENTIFIER2 = 15,
        SCE_CSS_ATTRIBUTE = 16,
        SCE_CSS_IDENTIFIER3 = 17,
        SCE_CSS_PSEUDOELEMENT = 18,
        SCE_CSS_EXTENDED_IDENTIFIER = 19,
        SCE_CSS_EXTENDED_PSEUDOCLASS = 20,
        SCE_CSS_EXTENDED_PSEUDOELEMENT = 21,
        SCE_CSS_MEDIA = 22,
        SCE_CSS_VARIABLE = 23,

        // Fortran
        SCE_F_DEFAULT = 0,
        SCE_F_COMMENT = 1,
        SCE_F_NUMBER = 2,
        SCE_F_STRING1 = 3,
        SCE_F_STRING2 = 4,
        SCE_F_STRINGEOL = 5,
        SCE_F_OPERATOR = 6,
        SCE_F_IDENTIFIER = 7,
        SCE_F_WORD = 8,
        SCE_F_WORD2 = 9,
        SCE_F_WORD3 = 10,
        SCE_F_PREPROCESSOR = 11,
        SCE_F_OPERATOR2 = 12,
        SCE_F_LABEL = 13,
        SCE_F_CONTINUATION = 14,

        // HTML
        SCE_H_DEFAULT = 0,
        SCE_H_TAG = 1,
        SCE_H_TAGUNKNOWN = 2,
        SCE_H_ATTRIBUTE = 3,
        SCE_H_ATTRIBUTEUNKNOWN = 4,
        SCE_H_NUMBER = 5,
        SCE_H_DOUBLESTRING = 6,
        SCE_H_SINGLESTRING = 7,
        SCE_H_OTHER = 8,
        SCE_H_COMMENT = 9,
        SCE_H_ENTITY = 10,
        SCE_H_TAGEND = 11,
        SCE_H_XMLSTART = 12,
        SCE_H_XMLEND = 13,
        SCE_H_SCRIPT = 14,
        SCE_H_ASP = 15,
        SCE_H_ASPAT = 16,
        SCE_H_CDATA = 17,
        SCE_H_QUESTION = 18,
        SCE_H_VALUE = 19,
        SCE_H_XCCOMMENT = 20,

        // Lisp
        SCE_LISP_DEFAULT = 0,
        SCE_LISP_COMMENT = 1,
        SCE_LISP_NUMBER = 2,
        SCE_LISP_KEYWORD = 3,
        SCE_LISP_KEYWORD_KW = 4,
        SCE_LISP_SYMBOL = 5,
        SCE_LISP_STRING = 6,
        SCE_LISP_STRINGEOL = 8,
        SCE_LISP_IDENTIFIER = 9,
        SCE_LISP_OPERATOR = 10,
        SCE_LISP_SPECIAL = 11,
        SCE_LISP_MULTI_COMMENT = 12,

        // Lua
        SCE_LUA_DEFAULT = 0,
        SCE_LUA_COMMENT = 1,
        SCE_LUA_COMMENTLINE = 2,
        SCE_LUA_COMMENTDOC = 3,
        SCE_LUA_NUMBER = 4,
        SCE_LUA_WORD = 5,
        SCE_LUA_STRING = 6,
        SCE_LUA_CHARACTER = 7,
        SCE_LUA_LITERALSTRING = 8,
        SCE_LUA_PREPROCESSOR = 9,
        SCE_LUA_OPERATOR = 10,
        SCE_LUA_IDENTIFIER = 11,
        SCE_LUA_STRINGEOL = 12,
        SCE_LUA_WORD2 = 13,
        SCE_LUA_WORD3 = 14,
        SCE_LUA_WORD4 = 15,
        SCE_LUA_WORD5 = 16,
        SCE_LUA_WORD6 = 17,
        SCE_LUA_WORD7 = 18,
        SCE_LUA_WORD8 = 19,
        SCE_LUA_LABEL = 20,

        SCE_PAS_DEFAULT = 0,
        SCE_PAS_IDENTIFIER = 1,
        SCE_PAS_COMMENT = 2,
        SCE_PAS_COMMENT2 = 3,
        SCE_PAS_COMMENTLINE = 4,
        SCE_PAS_PREPROCESSOR = 5,
        SCE_PAS_PREPROCESSOR2 = 6,
        SCE_PAS_NUMBER = 7,
        SCE_PAS_HEXNUMBER = 8,
        SCE_PAS_WORD = 9,
        SCE_PAS_STRING = 10,
        SCE_PAS_STRINGEOL = 11,
        SCE_PAS_CHARACTER = 12,
        SCE_PAS_OPERATOR = 13,
        SCE_PAS_ASM = 14,

        // Perl
        SCE_PL_DEFAULT = 0,
        SCE_PL_ERROR = 1,
        SCE_PL_COMMENTLINE = 2,
        SCE_PL_POD = 3,
        SCE_PL_NUMBER = 4,
        SCE_PL_WORD = 5,
        SCE_PL_STRING = 6,
        SCE_PL_CHARACTER = 7,
        SCE_PL_PUNCTUATION = 8,
        SCE_PL_PREPROCESSOR = 9,
        SCE_PL_OPERATOR = 10,
        SCE_PL_IDENTIFIER = 11,
        SCE_PL_SCALAR = 12,
        SCE_PL_ARRAY = 13,
        SCE_PL_HASH = 14,
        SCE_PL_SYMBOLTABLE = 15,
        SCE_PL_VARIABLE_INDEXER = 16,
        SCE_PL_REGEX = 17,
        SCE_PL_REGSUBST = 18,
        SCE_PL_LONGQUOTE = 19,
        SCE_PL_BACKTICKS = 20,
        SCE_PL_DATASECTION = 21,
        SCE_PL_HERE_DELIM = 22,
        SCE_PL_HERE_Q = 23,
        SCE_PL_HERE_QQ = 24,
        SCE_PL_HERE_QX = 25,
        SCE_PL_STRING_Q = 26,
        SCE_PL_STRING_QQ = 27,
        SCE_PL_STRING_QX = 28,
        SCE_PL_STRING_QR = 29,
        SCE_PL_STRING_QW = 30,
        SCE_PL_POD_VERB = 31,
        SCE_PL_SUB_PROTOTYPE = 40,
        SCE_PL_FORMAT_IDENT = 41,
        SCE_PL_FORMAT = 42,
        SCE_PL_STRING_VAR = 43,
        SCE_PL_XLAT = 44,
        SCE_PL_REGEX_VAR = 54,
        SCE_PL_REGSUBST_VAR = 55,
        SCE_PL_BACKTICKS_VAR = 57,
        SCE_PL_HERE_QQ_VAR = 61,
        SCE_PL_HERE_QX_VAR = 62,
        SCE_PL_STRING_QQ_VAR = 64,
        SCE_PL_STRING_QX_VAR = 65,
        SCE_PL_STRING_QR_VAR = 66,

        // Properties
        SCE_PROPS_DEFAULT = 0,
        SCE_PROPS_COMMENT = 1,
        SCE_PROPS_SECTION = 2,
        SCE_PROPS_ASSIGNMENT = 3,
        SCE_PROPS_DEFVAL = 4,
        SCE_PROPS_KEY = 5,

        // PHP script
        SCE_HPHP_COMPLEX_VARIABLE = 104,
        SCE_HPHP_DEFAULT = 118,
        SCE_HPHP_HSTRING = 119,
        SCE_HPHP_SIMPLESTRING = 120,
        SCE_HPHP_WORD = 121,
        SCE_HPHP_NUMBER = 122,
        SCE_HPHP_VARIABLE = 123,
        SCE_HPHP_COMMENT = 124,
        SCE_HPHP_COMMENTLINE = 125,
        SCE_HPHP_HSTRING_VARIABLE = 126,
        SCE_HPHP_OPERATOR = 127,

        // SQL
        SCE_SQL_DEFAULT = 0,
        SCE_SQL_COMMENT = 1,
        SCE_SQL_COMMENTLINE = 2,
        SCE_SQL_COMMENTDOC = 3,
        SCE_SQL_NUMBER = 4,
        SCE_SQL_WORD = 5,
        SCE_SQL_STRING = 6,
        SCE_SQL_CHARACTER = 7,
        SCE_SQL_SQLPLUS = 8,
        SCE_SQL_SQLPLUS_PROMPT = 9,
        SCE_SQL_OPERATOR = 10,
        SCE_SQL_IDENTIFIER = 11,
        SCE_SQL_SQLPLUS_COMMENT = 13,
        SCE_SQL_COMMENTLINEDOC = 15,
        SCE_SQL_WORD2 = 16,
        SCE_SQL_COMMENTDOCKEYWORD = 17,
        SCE_SQL_COMMENTDOCKEYWORDERROR = 18,
        SCE_SQL_USER1 = 19,
        SCE_SQL_USER2 = 20,
        SCE_SQL_USER3 = 21,
        SCE_SQL_USER4 = 22,
        SCE_SQL_QUOTEDIDENTIFIER = 23,
        SCE_SQL_QOPERATOR = 24,

        // Python
        SCE_P_DEFAULT = 0,
        SCE_P_COMMENTLINE = 1,
        SCE_P_NUMBER = 2,
        SCE_P_STRING = 3,
        SCE_P_CHARACTER = 4,
        SCE_P_WORD = 5,
        SCE_P_TRIPLE = 6,
        SCE_P_TRIPLEDOUBLE = 7,
        SCE_P_CLASSNAME = 8,
        SCE_P_DEFNAME = 9,
        SCE_P_OPERATOR = 10,
        SCE_P_IDENTIFIER = 11,
        SCE_P_COMMENTBLOCK = 12,
        SCE_P_STRINGEOL = 13,
        SCE_P_WORD2 = 14,
        SCE_P_DECORATOR = 15,

        // Ruby
        SCE_RB_DEFAULT = 0,
        SCE_RB_ERROR = 1,
        SCE_RB_COMMENTLINE = 2,
        SCE_RB_POD = 3,
        SCE_RB_NUMBER = 4,
        SCE_RB_WORD = 5,
        SCE_RB_STRING = 6,
        SCE_RB_CHARACTER = 7,
        SCE_RB_CLASSNAME = 8,
        SCE_RB_DEFNAME = 9,
        SCE_RB_OPERATOR = 10,
        SCE_RB_IDENTIFIER = 11,
        SCE_RB_REGEX = 12,
        SCE_RB_GLOBAL = 13,
        SCE_RB_SYMBOL = 14,
        SCE_RB_MODULE_NAME = 15,
        SCE_RB_INSTANCE_VAR = 16,
        SCE_RB_CLASS_VAR = 17,
        SCE_RB_BACKTICKS = 18,
        SCE_RB_DATASECTION = 19,
        SCE_RB_HERE_DELIM = 20,
        SCE_RB_HERE_Q = 21,
        SCE_RB_HERE_QQ = 22,
        SCE_RB_HERE_QX = 23,
        SCE_RB_STRING_Q = 24,
        SCE_RB_STRING_QQ = 25,
        SCE_RB_STRING_QX = 26,
        SCE_RB_STRING_QR = 27,
        SCE_RB_STRING_QW = 28,
        SCE_RB_WORD_DEMOTED = 29,
        SCE_RB_STDIN = 30,
        SCE_RB_STDOUT = 31,
        SCE_RB_STDERR = 40,
        SCE_RB_UPPER_BOUND = 41,

        // Smalltalk
        SCE_ST_DEFAULT = 0,
        SCE_ST_STRING = 1,
        SCE_ST_NUMBER = 2,
        SCE_ST_COMMENT = 3,
        SCE_ST_SYMBOL = 4,
        SCE_ST_BINARY = 5,
        SCE_ST_BOOL = 6,
        SCE_ST_SELF = 7,
        SCE_ST_SUPER = 8,
        SCE_ST_NIL = 9,
        SCE_ST_GLOBAL = 10,
        SCE_ST_RETURN = 11,
        SCE_ST_SPECIAL = 12,
        SCE_ST_KWSEND = 13,
        SCE_ST_ASSIGN = 14,
        SCE_ST_CHARACTER = 15,
        SCE_ST_SPEC_SEL = 16,

        // Basic / VB
        SCE_B_DEFAULT = 0,
        SCE_B_COMMENT = 1,
        SCE_B_NUMBER = 2,
        SCE_B_KEYWORD = 3,
        SCE_B_STRING = 4,
        SCE_B_PREPROCESSOR = 5,
        SCE_B_OPERATOR = 6,
        SCE_B_IDENTIFIER = 7,
        SCE_B_DATE = 8,
        SCE_B_STRINGEOL = 9,
        SCE_B_KEYWORD2 = 10,
        SCE_B_KEYWORD3 = 11,
        SCE_B_KEYWORD4 = 12,
        SCE_B_CONSTANT = 13,
        SCE_B_ASM = 14,
        SCE_B_LABEL = 15,
        SCE_B_ERROR = 16,
        SCE_B_HEXNUMBER = 17,
        SCE_B_BINNUMBER = 18,
        SCE_B_COMMENTBLOCK = 19,
        SCE_B_DOCLINE = 20,
        SCE_B_DOCBLOCK = 21,
        SCE_B_DOCKEYWORD = 22,

        // Markdown
        SCE_MARKDOWN_DEFAULT = 0,
        SCE_MARKDOWN_LINE_BEGIN = 1,
        SCE_MARKDOWN_STRONG1 = 2,
        SCE_MARKDOWN_STRONG2 = 3,
        SCE_MARKDOWN_EM1 = 4,
        SCE_MARKDOWN_EM2 = 5,
        SCE_MARKDOWN_HEADER1 = 6,
        SCE_MARKDOWN_HEADER2 = 7,
        SCE_MARKDOWN_HEADER3 = 8,
        SCE_MARKDOWN_HEADER4 = 9,
        SCE_MARKDOWN_HEADER5 = 10,
        SCE_MARKDOWN_HEADER6 = 11,
        SCE_MARKDOWN_PRECHAR = 12,
        SCE_MARKDOWN_ULIST_ITEM = 13,
        SCE_MARKDOWN_OLIST_ITEM = 14,
        SCE_MARKDOWN_BLOCKQUOTE = 15,
        SCE_MARKDOWN_STRIKEOUT = 16,
        SCE_MARKDOWN_HRULE = 17,
        SCE_MARKDOWN_LINK = 18,
        SCE_MARKDOWN_CODE = 19,
        SCE_MARKDOWN_CODE2 = 20,
        SCE_MARKDOWN_CODEBK = 21,

        // R
        SCE_R_DEFAULT = 0,
        SCE_R_COMMENT = 1,
        SCE_R_KWORD = 2,
        SCE_R_BASEKWORD = 3,
        SCE_R_OTHERKWORD = 4,
        SCE_R_NUMBER = 5,
        SCE_R_STRING = 6,
        SCE_R_STRING2 = 7,
        SCE_R_OPERATOR = 8,
        SCE_R_IDENTIFIER = 9,
        SCE_R_INFIX = 10,
        SCE_R_INFIXEOL = 11,

        // Verilog
        SCE_V_DEFAULT = 0,
        SCE_V_COMMENT = 1,
        SCE_V_COMMENTLINE = 2,
        SCE_V_COMMENTLINEBANG = 3,
        SCE_V_NUMBER = 4,
        SCE_V_WORD = 5,
        SCE_V_STRING = 6,
        SCE_V_WORD2 = 7,
        SCE_V_WORD3 = 8,
        SCE_V_PREPROCESSOR = 9,
        SCE_V_OPERATOR = 10,
        SCE_V_IDENTIFIER = 11,
        SCE_V_STRINGEOL = 12,
        SCE_V_USER = 19,
        SCE_V_COMMENT_WORD = 20,
        SCE_V_INPUT = 21,
        SCE_V_OUTPUT = 22,
        SCE_V_INOUT = 23,
        SCE_V_PORT_CONNECT = 24
    }

    #endregion
}