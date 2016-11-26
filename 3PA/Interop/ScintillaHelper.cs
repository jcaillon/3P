#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ScintillaHelper.cs) is part of 3P.
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

namespace _3PA.Interop {

    #region notifications

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
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_CharacterRange {
        public Sci_CharacterRange(int cpmin, int cpmax) {
            cpMin = cpmin; cpMax = cpmax;
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

        public IntPtr NativePointer { get { _initNativeStruct(); return _ptrSciTextToFind; } }
        public string lpstrText { set { _freeNativeString(); _sciTextToFind.lpstrText = Marshal.StringToHGlobalAnsi(value); } }
        public Sci_CharacterRange chrg { get { _readNativeStruct(); return _sciTextToFind.chrg; } set { _sciTextToFind.chrg = value; _initNativeStruct(); } }
        public Sci_CharacterRange chrgText { get { _readNativeStruct(); return _sciTextToFind.chrgText; } }
        void _initNativeStruct() {
            if (_ptrSciTextToFind == IntPtr.Zero)
                _ptrSciTextToFind = Marshal.AllocHGlobal(Marshal.SizeOf(_sciTextToFind));
            Marshal.StructureToPtr(_sciTextToFind, _ptrSciTextToFind, false);
        }
        void _readNativeStruct() {
            if (_ptrSciTextToFind != IntPtr.Zero)
                _sciTextToFind = (_Sci_TextToFind)Marshal.PtrToStructure(_ptrSciTextToFind, typeof(_Sci_TextToFind));
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

    #region Enum containing SciMsg

    /// <summary>
    /// Visibility and location of annotations in a Scintilla instance
    /// </summary>
    public enum Annotation : uint {
        /// <summary>
        /// Annotations are not displayed. This is the default.
        /// </summary>
        Hidden = SciMsg.ANNOTATION_HIDDEN,

        /// <summary>
        /// Annotations are drawn left justified with no adornment.
        /// </summary>
        Standard = SciMsg.ANNOTATION_STANDARD,

        /// <summary>
        /// Annotations are indented to match the text and are surrounded by a box.
        /// </summary>
        Boxed = SciMsg.ANNOTATION_BOXED,

        /// <summary>
        /// Annotations are indented to match the text.
        /// </summary>
        Indented = SciMsg.ANNOTATION_INDENTED
    }

    /// <summary>
    /// Additional location options for line wrapping visual indicators.
    /// </summary>
    public enum WrapVisualFlagLocation : uint {
        /// <summary>
        /// Wrap indicators are drawn near the border. This is the default.
        /// </summary>
        Default = SciMsg.SC_WRAPVISUALFLAGLOC_DEFAULT,

        /// <summary>
        /// Wrap indicators are drawn at the end of sublines near the text.
        /// </summary>
        EndByText = SciMsg.SC_WRAPVISUALFLAGLOC_END_BY_TEXT,

        /// <summary>
        /// Wrap indicators are drawn at the beginning of sublines near the text.
        /// </summary>
        StartByText = SciMsg.SC_WRAPVISUALFLAGLOC_START_BY_TEXT
    }

    /// <summary>
    /// The visual indicator used on a wrapped line.
    /// </summary>
    [Flags]
    public enum WrapVisualFlags : uint {
        /// <summary>
        /// No visual indicator is displayed. This the default.
        /// </summary>
        None = SciMsg.SC_WRAPVISUALFLAG_NONE,

        /// <summary>
        /// A visual indicator is displayed at th end of a wrapped subline.
        /// </summary>
        End = SciMsg.SC_WRAPVISUALFLAG_END,

        /// <summary>
        /// A visual indicator is displayed at the beginning of a subline.
        /// The subline is indented by 1 pixel to make room for the display.
        /// </summary>
        Start = SciMsg.SC_WRAPVISUALFLAG_START,

        /// <summary>
        /// A visual indicator is displayed in the number margin.
        /// </summary>
        Margin = SciMsg.SC_WRAPVISUALFLAG_MARGIN
    }

    /// <summary>
    /// The line wrapping strategy.
    /// </summary>
    public enum WrapMode : uint {
        /// <summary>
        /// Line wrapping is disabled. This is the default.
        /// </summary>
        None = SciMsg.SC_WRAP_NONE,

        /// <summary>
        /// Lines are wrapped on word or style boundaries.
        /// </summary>
        Word = SciMsg.SC_WRAP_WORD,

        /// <summary>
        /// Lines are wrapped between any character.
        /// </summary>
        Char = SciMsg.SC_WRAP_CHAR,

        /// <summary>
        /// Lines are wrapped on whitespace.
        /// </summary>
        Whitespace = SciMsg.SC_WRAP_WHITESPACE
    }

    /// <summary>
    /// Indenting behavior of wrapped sublines.
    /// </summary>
    public enum WrapIndentMode : uint {
        /// <summary>
        /// Wrapped sublines aligned to left of window plus the amount set by WrapStartIndent
        /// This is the default.
        /// </summary>
        Fixed,

        /// <summary>
        /// Wrapped sublines are aligned to first subline indent.
        /// </summary>
        Same,

        /// <summary>
        /// Wrapped sublines are aligned to first subline indent plus one more level of indentation.
        /// </summary>
        Indent = SciMsg.SC_WRAPINDENT_INDENT
    }

    public enum WhitespaceMode : uint {
        /// <summary>
        /// The normal display mode with whitespace displayed as an empty background color.
        /// </summary>
        Invisible = SciMsg.SCWS_INVISIBLE,

        /// <summary>
        /// Whitespace characters are drawn as dots and arrows.
        /// </summary>
        VisibleAlways = SciMsg.SCWS_VISIBLEALWAYS,

        /// <summary>
        /// Whitespace used for indentation is displayed normally but after the first visible character,
        /// it is shown as dots and arrows.
        /// </summary>
        VisibleAfterIndent = SciMsg.SCWS_VISIBLEAFTERINDENT
    }

    /// <summary>
    /// Specifies the how patterns are matched when performing a search in a Scintilla instance.
    /// </summary>
    /// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
    [Flags]
    public enum SearchFlags : uint {
        /// <summary>
        /// Matches every instance of the search string.
        /// </summary>
        None = 0,

        /// <summary>
        /// A match only occurs with text that matches the case of the search string.
        /// </summary>
        MatchCase = SciMsg.SCFIND_MATCHCASE,

        /// <summary>
        /// A match only occurs if the characters before and after are not word characters.
        /// </summary>
        WholeWord = SciMsg.SCFIND_WHOLEWORD,

        /// <summary>
        /// A match only occurs if the character before is not a word character.
        /// </summary>
        WordStart = SciMsg.SCFIND_WORDSTART,

        /// <summary>
        /// The search string should be interpreted as a regular expression.
        /// Regular expressions will only match ranges within a single line, never matching over multiple lines.
        /// </summary>
        Regex = SciMsg.SCFIND_REGEXP,

        /// <summary>
        /// Treat regular expression in a more POSIX compatible manner by interpreting bare '(' and ')' for tagged sections rather than "\(" and "\)".
        /// </summary>
        Posix = SciMsg.SCFIND_POSIX
    }

    /// <summary>
    /// Specifies the behavior of pasting into multiple selections.
    /// </summary>
    public enum MultiPaste : uint {
        /// <summary>
        /// Pasting into multiple selections only pastes to the main selection. This is the default.
        /// </summary>
        Once = SciMsg.SC_MULTIPASTE_ONCE,

        /// <summary>
        /// Pasting into multiple selections pastes into each selection.
        /// </summary>
        Each = SciMsg.SC_MULTIPASTE_EACH
    }

    /// <summary>
    /// Specifies the lexer to use for syntax highlighting in a Scintilla instance.
    /// </summary>
    public enum Lexer : uint {
        /// <summary>
        /// Lexing is performed by the Scintilla instance container (host) using
        /// the Scintilla.StyleNeeded event.
        /// </summary>
        Container = SciMsg.SCLEX_CONTAINER,

        /// <summary>
        /// No lexing should be performed.
        /// </summary>
        Null = SciMsg.SCLEX_NULL,

        /// <summary>
        /// The Ada (95) language lexer.
        /// </summary>
        Ada = SciMsg.SCLEX_ADA,

        /// <summary>
        /// The assembly language lexer.
        /// </summary>
        Asm = SciMsg.SCLEX_ASM,

        /// <summary>
        /// The batch file lexer.
        /// </summary>
        Batch = SciMsg.SCLEX_BATCH,

        /// <summary>
        /// The C language family (C++, C, C#, Java, JavaScript, etc...) lexer.
        /// </summary>
        Cpp = SciMsg.SCLEX_CPP,

        /// <summary>
        /// The Cascading Style Sheets (CSS, SCSS) lexer.
        /// </summary>
        Css = SciMsg.SCLEX_CSS,

        /// <summary>
        /// The Fortran language lexer.
        /// </summary>
        Fortran = SciMsg.SCLEX_FORTRAN,

        /// <summary>
        /// The FreeBASIC language lexer.
        /// </summary>
        FreeBasic = SciMsg.SCLEX_FREEBASIC,

        /// <summary>
        /// The HyperText Markup Language (HTML) lexer.
        /// </summary>
        Html = SciMsg.SCLEX_HTML,

        /// <summary>
        /// The Lisp language lexer.
        /// </summary>
        Lisp = SciMsg.SCLEX_LISP,

        /// <summary>
        /// The Lua scripting language lexer.
        /// </summary>
        Lua = SciMsg.SCLEX_LUA,

        /// <summary>
        /// The Pascal language lexer.
        /// </summary>
        Pascal = SciMsg.SCLEX_PASCAL,

        /// <summary>
        /// The Perl language lexer.
        /// </summary>
        Perl = SciMsg.SCLEX_PERL,

        /// <summary>
        /// The PHP: Hypertext Preprocessor (PHP) script lexer.
        /// </summary>
        PhpScript = SciMsg.SCLEX_PHPSCRIPT,

        /// <summary>
        /// Properties file (INI) lexer.
        /// </summary>
        Properties = SciMsg.SCLEX_PROPERTIES,

        /// <summary>
        /// The PureBasic language lexer.
        /// </summary>
        PureBasic = SciMsg.SCLEX_PUREBASIC,

        /// <summary>
        /// The Python language lexer.
        /// </summary>
        Python = SciMsg.SCLEX_PYTHON,

        /// <summary>
        /// The Ruby language lexer.
        /// </summary>
        Ruby = SciMsg.SCLEX_RUBY,

        /// <summary>
        /// The SmallTalk language lexer.
        /// </summary>
        Smalltalk = SciMsg.SCLEX_SMALLTALK,

        /// <summary>
        /// The Structured Query Language (SQL) lexer.
        /// </summary>
        Sql = SciMsg.SCLEX_SQL,

        /// <summary>
        /// The Visual Basic (VB) lexer.
        /// </summary>
        Vb = SciMsg.SCLEX_VB,

        /// <summary>
        /// The Visual Basic Script (VBScript) lexer.
        /// </summary>
        VbScript = SciMsg.SCLEX_VBSCRIPT,

        /// <summary>
        /// The Verilog hardware description language lexer.
        /// </summary>
        Verilog = SciMsg.SCLEX_VERILOG,

        /// <summary>
        /// The Extensible Markup Language (XML) lexer.
        /// </summary>
        Xml = SciMsg.SCLEX_XML,

        /// <summary>
        /// The Blitz (Blitz3D, BlitzMax, etc...) variant of Basic lexer.
        /// </summary>
        BlitzBasic = SciMsg.SCLEX_BLITZBASIC,

        /// <summary>
        /// The Markdown syntax lexer.
        /// </summary>
        Markdown = SciMsg.SCLEX_MARKDOWN,

        /// <summary>
        /// The R programming language lexer.
        /// </summary>
        R = SciMsg.SCLEX_R
    }

    public enum FontQuality : uint {
        /// <summary>
        /// Specifies that the character quality of the font does not matter; so the lowest quality can be used.
        /// This is the default.
        /// </summary>
        Default = SciMsg.SC_EFF_QUALITY_DEFAULT,

        /// <summary>
        /// Specifies that anti-aliasing should not be used when rendering text.
        /// </summary>
        NonAntiAliased = SciMsg.SC_EFF_QUALITY_NON_ANTIALIASED,

        /// <summary>
        /// Specifies that anti-aliasing should be used when rendering text, if the font supports it.
        /// </summary>
        AntiAliased = SciMsg.SC_EFF_QUALITY_ANTIALIASED,

        /// <summary>
        /// Specifies that ClearType anti-aliasing should be used when rendering text, if the font supports it.
        /// </summary>
        LcdOptimized = SciMsg.SC_EFF_QUALITY_LCD_OPTIMIZED
    }

    /// <summary>
    /// End-of-line format.
    /// </summary>
    public enum Eol : uint {
        /// <summary>
        /// Carriage Return, Line Feed pair "\r\n" (0x0D0A).
        /// </summary>
        CrLf = SciMsg.SC_EOL_CRLF,

        /// <summary>
        /// Carriage Return '\r' (0x0D).
        /// </summary>
        Cr = SciMsg.SC_EOL_CR,

        /// <summary>
        /// Line Feed '\n' (0x0A).
        /// </summary>
        Lf = SciMsg.SC_EOL_LF
    }

    /// <summary>
    /// Actions which can be performed by the application or bound to keys in a Scintilla instance.
    /// </summary>
    public enum Command : uint {
        /// <summary>
        /// When bound to keys performs the standard platform behavior.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Performs no action and when bound to keys prevents them from propagating to the parent window.
        /// </summary>
        Null = SciMsg.SCI_NULL,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret down one line.
        /// </summary>
        LineDown = SciMsg.SCI_LINEDOWN,

        /// <summary>
        /// Extends the selection down one line.
        /// </summary>
        LineDownExtend = SciMsg.SCI_LINEDOWNEXTEND,

        /// <summary>
        /// Extends the rectangular selection down one line.
        /// </summary>
        LineDownRectExtend = SciMsg.SCI_LINEDOWNRECTEXTEND,

        /// <summary>
        /// Scrolls down one line.
        /// </summary>
        LineScrollDown = SciMsg.SCI_LINESCROLLDOWN,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret up one line.
        /// </summary>
        LineUp = SciMsg.SCI_LINEUP,

        /// <summary>
        /// Extends the selection up one line.
        /// </summary>
        LineUpExtend = SciMsg.SCI_LINEUPEXTEND,

        /// <summary>
        /// Extends the rectangular selection up one line.
        /// </summary>
        LineUpRectExtend = SciMsg.SCI_LINEUPRECTEXTEND,

        /// <summary>
        /// Scrolls up one line.
        /// </summary>
        LineScrollUp = SciMsg.SCI_LINESCROLLUP,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret down one paragraph.
        /// </summary>
        ParaDown = SciMsg.SCI_PARADOWN,

        /// <summary>
        /// Extends the selection down one paragraph.
        /// </summary>
        ParaDownExtend = SciMsg.SCI_PARADOWNEXTEND,

        /// <summary>
        /// Moves the caret up one paragraph.
        /// </summary>
        ParaUp = SciMsg.SCI_PARAUP,

        /// <summary>
        /// Extends the selection up one paragraph.
        /// </summary>
        ParaUpExtend = SciMsg.SCI_PARAUPEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret left one character.
        /// </summary>
        CharLeft = SciMsg.SCI_CHARLEFT,

        /// <summary>
        /// Extends the selection left one character.
        /// </summary>
        CharLeftExtend = SciMsg.SCI_CHARLEFTEXTEND,

        /// <summary>
        /// Extends the rectangular selection left one character.
        /// </summary>
        CharLeftRectExtend = SciMsg.SCI_CHARLEFTRECTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret right one character.
        /// </summary>
        CharRight = SciMsg.SCI_CHARRIGHT,

        /// <summary>
        /// Extends the selection right one character.
        /// </summary>
        CharRightExtend = SciMsg.SCI_CHARRIGHTEXTEND,

        /// <summary>
        /// Extends the rectangular selection right one character.
        /// </summary>
        CharRightRectExtend = SciMsg.SCI_CHARRIGHTRECTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the start of the previous word.
        /// </summary>
        WordLeft = SciMsg.SCI_WORDLEFT,

        /// <summary>
        /// Extends the selection to the start of the previous word.
        /// </summary>
        WordLeftExtend = SciMsg.SCI_WORDLEFTEXTEND,

        /// <summary>
        /// Moves the caret to the start of the next word.
        /// </summary>
        WordRight = SciMsg.SCI_WORDRIGHT,

        /// <summary>
        /// Extends the selection to the start of the next word.
        /// </summary>
        WordRightExtend = SciMsg.SCI_WORDRIGHTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the end of the previous word.
        /// </summary>
        WordLeftEnd = SciMsg.SCI_WORDLEFTEND,

        /// <summary>
        /// Extends the selection to the end of the previous word.
        /// </summary>
        WordLeftEndExtend = SciMsg.SCI_WORDLEFTENDEXTEND,

        /// <summary>
        /// Moves the caret to the end of the next word.
        /// </summary>
        WordRightEnd = SciMsg.SCI_WORDRIGHTEND,

        /// <summary>
        /// Extends the selection to the end of the next word.
        /// </summary>
        WordRightEndExtend = SciMsg.SCI_WORDRIGHTENDEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the previous word segment (case change or underscore).
        /// </summary>
        WordPartLeft = SciMsg.SCI_WORDPARTLEFT,

        /// <summary>
        /// Extends the selection to the previous word segment (case change or underscore).
        /// </summary>
        WordPartLeftExtend = SciMsg.SCI_WORDPARTLEFTEXTEND,

        /// <summary>
        /// Moves the caret to the next word segment (case change or underscore).
        /// </summary>
        WordPartRight = SciMsg.SCI_WORDPARTRIGHT,

        /// <summary>
        /// Extends the selection to the next word segment (case change or underscore).
        /// </summary>
        WordPartRightExtend = SciMsg.SCI_WORDPARTRIGHTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the start of the line.
        /// </summary>
        Home = SciMsg.SCI_HOME,

        /// <summary>
        /// Extends the selection to the start of the line.
        /// </summary>
        HomeExtend = SciMsg.SCI_HOMEEXTEND,

        /// <summary>
        /// Extends the rectangular selection to the start of the line.
        /// </summary>
        HomeRectExtend = SciMsg.SCI_HOMERECTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the start of the display line.
        /// </summary>
        HomeDisplay = SciMsg.SCI_HOMEDISPLAY,

        /// <summary>
        /// Extends the selection to the start of the display line.
        /// </summary>
        HomeDisplayExtend = SciMsg.SCI_HOMEDISPLAYEXTEND,

        /// <summary>
        /// Moves the caret to the start of the display or document line.
        /// </summary>
        HomeWrap = SciMsg.SCI_HOMEWRAP,

        /// <summary>
        /// Extends the selection to the start of the display or document line.
        /// </summary>
        HomeWrapExtend = SciMsg.SCI_HOMEWRAPEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the first non-whitespace character of the line.
        /// </summary>
        VcHome = SciMsg.SCI_VCHOME,

        /// <summary>
        /// Extends the selection to the first non-whitespace character of the line.
        /// </summary>
        VcHomeExtend = SciMsg.SCI_VCHOMEEXTEND,

        /// <summary>
        /// Extends the rectangular selection to the first non-whitespace character of the line.
        /// </summary>
        VcHomeRectExtend = SciMsg.SCI_VCHOMERECTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the first non-whitespace character of the display or document line.
        /// </summary>
        VcHomeWrap = SciMsg.SCI_VCHOMEWRAP,

        /// <summary>
        /// Extends the selection to the first non-whitespace character of the display or document line.
        /// </summary>
        VcHomeWrapExtend = SciMsg.SCI_VCHOMEWRAPEXTEND,

        /// <summary>
        /// Moves the caret to the first non-whitespace character of the display line.
        /// </summary>
        VcHomeDisplay = SciMsg.SCI_VCHOMEDISPLAY,

        /// <summary>
        /// Extends the selection to the first non-whitespace character of the display line.
        /// </summary>
        VcHomeDisplayExtend = SciMsg.SCI_VCHOMEDISPLAYEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the end of the document line.
        /// </summary>
        LineEnd = SciMsg.SCI_LINEEND,

        /// <summary>
        /// Extends the selection to the end of the document line.
        /// </summary>
        LineEndExtend = SciMsg.SCI_LINEENDEXTEND,

        /// <summary>
        /// Extends the rectangular selection to the end of the document line.
        /// </summary>
        LineEndRectExtend = SciMsg.SCI_LINEENDRECTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the end of the display line.
        /// </summary>
        LineEndDisplay = SciMsg.SCI_LINEENDDISPLAY,

        /// <summary>
        /// Extends the selection to the end of the display line.
        /// </summary>
        LineEndDisplayExtend = SciMsg.SCI_LINEENDDISPLAYEXTEND,

        /// <summary>
        /// Moves the caret to the end of the display or document line.
        /// </summary>
        LineEndWrap = SciMsg.SCI_LINEENDWRAP,

        /// <summary>
        /// Extends the selection to the end of the display or document line.
        /// </summary>
        LineEndWrapExtend = SciMsg.SCI_LINEENDWRAPEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret to the start of the document.
        /// </summary>
        DocumentStart = SciMsg.SCI_DOCUMENTSTART,

        /// <summary>
        /// Extends the selection to the start of the document.
        /// </summary>
        DocumentStartExtend = SciMsg.SCI_DOCUMENTSTARTEXTEND,

        /// <summary>
        /// Moves the caret to the end of the document.
        /// </summary>
        DocumentEnd = SciMsg.SCI_DOCUMENTEND,

        /// <summary>
        /// Extends the selection to the end of the document.
        /// </summary>
        DocumentEndExtend = SciMsg.SCI_DOCUMENTENDEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret up one page.
        /// </summary>
        PageUp = SciMsg.SCI_PAGEUP,

        /// <summary>
        /// Extends the selection up one page.
        /// </summary>
        PageUpExtend = SciMsg.SCI_PAGEUPEXTEND,

        /// <summary>
        /// Extends the rectangular selection up one page.
        /// </summary>
        PageUpRectExtend = SciMsg.SCI_PAGEUPRECTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret down one page.
        /// </summary>
        PageDown = SciMsg.SCI_PAGEDOWN,

        /// <summary>
        /// Extends the selection down one page.
        /// </summary>
        PageDownExtend = SciMsg.SCI_PAGEDOWNEXTEND,

        /// <summary>
        /// Extends the rectangular selection down one page.
        /// </summary>
        PageDownRectExtend = SciMsg.SCI_PAGEDOWNRECTEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret up one window or page.
        /// </summary>
        StutteredPageUp = SciMsg.SCI_STUTTEREDPAGEUP,

        /// <summary>
        /// Extends the selection up one window or page.
        /// </summary>
        StutteredPageUpExtend = SciMsg.SCI_STUTTEREDPAGEUPEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the caret down one window or page.
        /// </summary>
        StutteredPageDown = SciMsg.SCI_STUTTEREDPAGEDOWN,

        /// <summary>
        /// Extends the selection down one window or page.
        /// </summary>
        StutteredPageDownExtend = SciMsg.SCI_STUTTEREDPAGEDOWNEXTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Deletes the character left of the caret.
        /// </summary>
        DeleteBack = SciMsg.SCI_DELETEBACK,

        /// <summary>
        /// Deletes the character (excluding line breaks) left of the caret.
        /// </summary>
        DeleteBackNotLine = SciMsg.SCI_DELETEBACKNOTLINE,

        // --------------------------------------------------------------------

        /// <summary>
        /// Deletes from the caret to the start of the previous word.
        /// </summary>
        DelWordLeft = SciMsg.SCI_DELWORDLEFT,

        /// <summary>
        /// Deletes from the caret to the start of the next word.
        /// </summary>
        DelWordRight = SciMsg.SCI_DELWORDRIGHT,

        /// <summary>
        /// Deletes from the caret to the end of the next word.
        /// </summary>
        DelWordRightEnd = SciMsg.SCI_DELWORDRIGHTEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Deletes the characters left of the caret to the start of the line.
        /// </summary>
        DelLineLeft = SciMsg.SCI_DELLINELEFT,

        /// <summary>
        /// Deletes the characters right of the caret to the start of the line.
        /// </summary>
        DelLineRight = SciMsg.SCI_DELLINERIGHT,

        /// <summary>
        /// Deletes the current line.
        /// </summary>
        LineDelete = SciMsg.SCI_LINEDELETE,

        // --------------------------------------------------------------------

        /// <summary>
        /// Removes the current line and places it on the clipboard.
        /// </summary>
        LineCut = SciMsg.SCI_LINECUT,

        /// <summary>
        /// Copies the current line and places it on the clipboard.
        /// </summary>
        LineCopy = SciMsg.SCI_LINECOPY,

        /// <summary>
        /// Transposes the current and previous lines.
        /// </summary>
        LineTranspose = SciMsg.SCI_LINETRANSPOSE,

        /// <summary>
        /// Duplicates the current line.
        /// </summary>
        LineDuplicate = SciMsg.SCI_LINEDUPLICATE,

        // --------------------------------------------------------------------

        /// <summary>
        /// Converts the selection to lowercase.
        /// </summary>
        Lowercase = SciMsg.SCI_LOWERCASE,

        /// <summary>
        /// Converts the selection to uppercase.
        /// </summary>
        Uppercase = SciMsg.SCI_UPPERCASE,

        /// <summary>
        /// Cancels autocompletion, calltip display, and drops any additional selections.
        /// </summary>
        Cancel = SciMsg.SCI_CANCEL,

        /// <summary>
        /// Toggles overtype. See Scintilla.Overtype.
        /// </summary>
        EditToggleOvertype = SciMsg.SCI_EDITTOGGLEOVERTYPE,

        // --------------------------------------------------------------------

        /// <summary>
        /// Inserts a newline character.
        /// </summary>
        NewLine = SciMsg.SCI_NEWLINE,

        /// <summary>
        /// Inserts a form feed character.
        /// </summary>
        FormFeed = SciMsg.SCI_FORMFEED,

        /// <summary>
        /// Adds a tab (indent) character.
        /// </summary>
        Tab = SciMsg.SCI_TAB,

        /// <summary>
        /// Removes a tab (indent) character from the start of a line.
        /// </summary>
        BackTab = SciMsg.SCI_BACKTAB,

        // --------------------------------------------------------------------

        /// <summary>
        /// Duplicates the current selection.
        /// </summary>
        SelectionDuplicate = SciMsg.SCI_SELECTIONDUPLICATE,

        /// <summary>
        /// Moves the caret vertically to the center of the screen.
        /// </summary>
        VerticalCenterCaret = SciMsg.SCI_VERTICALCENTRECARET,

        // --------------------------------------------------------------------

        /// <summary>
        /// Moves the selected lines up.
        /// </summary>
        MoveSelectedLinesUp = SciMsg.SCI_MOVESELECTEDLINESUP,

        /// <summary>
        /// Moves the selected lines down.
        /// </summary>
        MoveSelectedLinesDown = SciMsg.SCI_MOVESELECTEDLINESDOWN,

        // --------------------------------------------------------------------

        /// <summary>
        /// Scrolls to the start of the document without changing the selection.
        /// </summary>
        ScrollToStart = SciMsg.SCI_SCROLLTOSTART,

        /// <summary>
        /// Scrolls to the end of the document without changing the selection.
        /// </summary>
        ScrollToEnd = SciMsg.SCI_SCROLLTOEND,

        // --------------------------------------------------------------------

        /// <summary>
        /// Command equivalent to Scintilla.ZoomIn.
        /// </summary>
        ZoomIn = SciMsg.SCI_ZOOMIN,

        /// <summary>
        /// Command equivalent to Scintilla.ZoomOut.
        /// </summary>
        ZoomOut = SciMsg.SCI_ZOOMOUT,

        /// <summary>
        /// Command equivalent to Scintilla.Undo.
        /// </summary>
        Undo = SciMsg.SCI_UNDO,

        /// <summary>
        /// Command equivalent to Scintilla.Redo.
        /// </summary>
        Redo = SciMsg.SCI_REDO,

        /// <summary>
        /// Command equivalent to Scintilla.SwapMainAnchorCaret
        /// </summary>
        SwapMainAnchorCaret = SciMsg.SCI_SWAPMAINANCHORCARET,

        /// <summary>
        /// Command equivalent to Scintilla.RotateSelection
        /// </summary>
        RotateSelection = SciMsg.SCI_ROTATESELECTION,

        /// <summary>
        /// Command equivalent to Scintilla.MultipleSelectAddNext
        /// </summary>
        MultipleSelectAddNext = SciMsg.SCI_MULTIPLESELECTADDNEXT,

        /// <summary>
        /// Command equivalent to Scintilla.MultipleSelectAddEach
        /// </summary>
        MultipleSelectAddEach = SciMsg.SCI_MULTIPLESELECTADDEACH,

        /// <summary>
        /// Command equivalent to Scintilla.SelectAll
        /// </summary>
        SelectAll = SciMsg.SCI_SELECTALL
    }

    /// <summary>
    /// Fold actions.
    /// </summary>
    public enum FoldAction : uint {
        /// <summary>
        /// Contract the fold.
        /// </summary>
        Contract = SciMsg.SC_FOLDACTION_CONTRACT,

        /// <summary>
        /// Expand the fold.
        /// </summary>
        Expand = SciMsg.SC_FOLDACTION_EXPAND,

        /// <summary>
        /// Toggle between contracted and expanded.
        /// </summary>
        Toggle = SciMsg.SC_FOLDACTION_TOGGLE
    }

    /// <summary>
    /// The possible casing styles of a style.
    /// </summary>
    public enum StyleCase : uint {
        /// <summary>
        /// Display the text normally.
        /// </summary>
        Mixed = SciMsg.SC_CASE_MIXED,

        /// <summary>
        /// Display the text in upper case.
        /// </summary>
        Upper = SciMsg.SC_CASE_UPPER,

        /// <summary>
        /// Display the text in lower case.
        /// </summary>
        Lower = SciMsg.SC_CASE_LOWER,

        /// <summary>
        /// Display the text in camel case.
        /// </summary>
        Camel = SciMsg.SC_CASE_CAMEL
    }

    /// <summary>
    /// Flags associated with a Npp.Indicator.
    /// </summary>
    /// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
    [Flags]
    public enum IndicatorFlags : uint {
        /// <summary>
        /// No flags. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// When set, will treat an indicator value as a RGB color that has been OR'd with Npp.Indicator.ValueBit
        /// and will use that instead of the value specified in the Npp.Indicator.ForeColor property. This allows
        /// an indicator to display more than one color.
        /// </summary>
        ValueFore = SciMsg.SC_INDICFLAG_VALUEFORE
    }

    /// <summary>
    /// The visual appearance of an indicator.
    /// </summary>
    public enum IndicatorStyle : uint {
        /// <summary>
        /// Underlined with a single, straight line.
        /// </summary>
        Plain = SciMsg.INDIC_PLAIN,

        /// <summary>
        /// A squiggly underline. Requires 3 pixels of descender space.
        /// </summary>
        Squiggle = SciMsg.INDIC_SQUIGGLE,

        /// <summary>
        /// A line of small T shapes.
        /// </summary>
        TT = SciMsg.INDIC_TT,

        /// <summary>
        /// Diagonal hatching.
        /// </summary>
        Diagonal = SciMsg.INDIC_DIAGONAL,

        /// <summary>
        /// Strike out.
        /// </summary>
        Strike = SciMsg.INDIC_STRIKE,

        /// <summary>
        /// An indicator with no visual effect.
        /// </summary>
        Hidden = SciMsg.INDIC_HIDDEN,

        /// <summary>
        /// A rectangle around the text.
        /// </summary>
        Box = SciMsg.INDIC_BOX,

        /// <summary>
        /// A rectangle around the text with rounded corners. The rectangle outline and fill transparencies can be adjusted using
        /// Npp.Indicator.Alpha and Npp.Indicator.OutlineAlpha.
        /// </summary>
        RoundBox = SciMsg.INDIC_ROUNDBOX,

        /// <summary>
        /// A rectangle around the text. The rectangle outline and fill transparencies can be adjusted using
        /// Npp.Indicator.Alpha and <see cref="Npp.Indicator.OutlineAlpha"/>.
        /// </summary>
        StraightBox = SciMsg.INDIC_STRAIGHTBOX,

        /// <summary>
        /// A dashed underline.
        /// </summary>
        Dash = SciMsg.INDIC_DASH,

        /// <summary>
        /// A dotted underline.
        /// </summary>
        Dots = SciMsg.INDIC_DOTS,

        /// <summary>
        /// Similar to Squiggle but only using 2 vertical pixels so will fit under small fonts.
        /// </summary>
        SquiggleLow = SciMsg.INDIC_SQUIGGLELOW,

        /// <summary>
        /// A dotted rectangle around the text. The dots transparencies can be adjusted using
        /// Npp.Indicator.Alpha and Npp.Indicator.OutlineAlpha.
        /// </summary>
        DotBox = SciMsg.INDIC_DOTBOX,

        // PIXMAP

        /// <summary>
        /// A 2-pixel thick underline with 1 pixel insets on either side.
        /// </summary>
        CompositionThick = SciMsg.INDIC_COMPOSITIONTHICK,

        /// <summary>
        /// A 1-pixel thick underline with 1 pixel insets on either side.
        /// </summary>
        CompositionThin = SciMsg.INDIC_COMPOSITIONTHIN,

        /// <summary>
        /// A rectangle around the entire character area. The rectangle outline and fill transparencies can be adjusted using
        /// Npp.Indicator.Alpha and <see cref="Npp.Indicator.OutlineAlpha"/>.
        /// </summary>
        FullBox = SciMsg.INDIC_FULLBOX,

        /// <summary>
        /// An indicator that will change the foreground color of text to the foreground color of the indicator.
        /// </summary>
        TextFore = SciMsg.INDIC_TEXTFORE
    }

    /// <summary>
    /// The display of a cursor when over a margin.
    /// </summary>
    public enum MarginCursor : uint {
        /// <summary>
        /// A normal arrow.
        /// </summary>
        Arrow = 2, // SC_CURSORARROW

        /// <summary>
        /// A reversed arrow.
        /// </summary>
        ReverseArrow = 7 // SC_CURSORREVERSEARROW
    }

    public enum SciCursor {
        Normal = -1, // SC_CURSORNORMAL
        Wait = 4 // SC_CURSORWAIT
    }

    /// <summary>
    /// The behavior and appearance of a margin.
    /// </summary>
    public enum MarginType : uint {
        /// <summary>
        /// Margin can display symbols.
        /// </summary>
        Symbol = SciMsg.SC_MARGIN_SYMBOL,

        /// <summary>
        /// Margin displays line numbers.
        /// </summary>
        Number = SciMsg.SC_MARGIN_NUMBER,

        /// <summary>
        /// Margin can display symbols and has a background color equivalent to the default style background color.
        /// </summary>
        BackColor = SciMsg.SC_MARGIN_BACK,

        /// <summary>
        /// Margin can display symbols and has a background color equivalent to the default style foreground color.
        /// </summary>
        ForeColor = SciMsg.SC_MARGIN_FORE,

        /// <summary>
        /// Margin can display application defined text.
        /// </summary>
        Text = SciMsg.SC_MARGIN_TEXT,

        /// <summary>
        /// Margin can display application defined text right-justified.
        /// </summary>
        RightText = SciMsg.SC_MARGIN_RTEXT
    }

    /// <summary>
    /// The symbol displayed by a Npp.Marker
    /// </summary>
    public enum MarkerSymbol : uint {
        /// <summary>
        /// A circle. This symbol is typically used to indicate a breakpoint.
        /// </summary>
        Circle = SciMsg.SC_MARK_CIRCLE,

        /// <summary>
        /// A rectangel with rounded edges.
        /// </summary>
        RoundRect = SciMsg.SC_MARK_ROUNDRECT,

        /// <summary>
        /// An arrow (triangle) pointing right.
        /// </summary>
        Arrow = SciMsg.SC_MARK_ARROW,

        /// <summary>
        /// A rectangle that is wider than it is tall.
        /// </summary>
        SmallRect = SciMsg.SC_MARK_SMALLRECT,

        /// <summary>
        /// An arrow and tail pointing right. This symbol is typically used to indicate the current line of execution.
        /// </summary>
        ShortArrow = SciMsg.SC_MARK_SHORTARROW,

        /// <summary>
        /// An invisible symbol useful for tracking the movement of lines.
        /// </summary>
        Empty = SciMsg.SC_MARK_EMPTY,

        /// <summary>
        /// An arrow (triangle) pointing down.
        /// </summary>
        ArrowDown = SciMsg.SC_MARK_ARROWDOWN,

        /// <summary>
        /// A minus (-) symbol.
        /// </summary>
        Minus = SciMsg.SC_MARK_MINUS,

        /// <summary>
        /// A plus (+) symbol.
        /// </summary>
        Plus = SciMsg.SC_MARK_PLUS,

        /// <summary>
        /// A thin vertical line. This symbol is typically used on the middle line of an expanded fold block.
        /// </summary>
        VLine = SciMsg.SC_MARK_VLINE,

        /// <summary>
        /// A thin 'L' shaped line. This symbol is typically used on the last line of an expanded fold block.
        /// </summary>
        LCorner = SciMsg.SC_MARK_LCORNER,

        /// <summary>
        /// A thin 't' shaped line. This symbol is typically used on the last line of an expanded nested fold block.
        /// </summary>
        TCorner = SciMsg.SC_MARK_TCORNER,

        /// <summary>
        /// A plus (+) symbol with surrounding box. This symbol is typically used on the first line of a collapsed fold block.
        /// </summary>
        BoxPlus = SciMsg.SC_MARK_BOXPLUS,

        /// <summary>
        /// A plus (+) symbol with surrounding box and thin vertical line. This symbol is typically used on the first line of a collapsed nested fold block.
        /// </summary>
        BoxPlusConnected = SciMsg.SC_MARK_BOXPLUSCONNECTED,

        /// <summary>
        /// A minus (-) symbol with surrounding box. This symbol is typically used on the first line of an expanded fold block.
        /// </summary>
        BoxMinus = SciMsg.SC_MARK_BOXMINUS,

        /// <summary>
        /// A minus (-) symbol with surrounding box and thin vertical line. This symbol is typically used on the first line of an expanded nested fold block.
        /// </summary>
        BoxMinusConnected = SciMsg.SC_MARK_BOXMINUSCONNECTED,

        /// <summary>
        /// Similar to a LCorner, but curved.
        /// </summary>
        LCornerCurve = SciMsg.SC_MARK_LCORNERCURVE,

        /// <summary>
        /// Similar to a TCorner, but curved.
        /// </summary>
        TCornerCurve = SciMsg.SC_MARK_TCORNERCURVE,

        /// <summary>
        /// Similar to a BoxPlus but surrounded by a circle.
        /// </summary>
        CirclePlus = SciMsg.SC_MARK_CIRCLEPLUS,

        /// <summary>
        /// Similar to a BoxPlusConnected, but surrounded by a circle.
        /// </summary>
        CirclePlusConnected = SciMsg.SC_MARK_CIRCLEPLUSCONNECTED,

        /// <summary>
        /// Similar to a BoxMinus, but surrounded by a circle.
        /// </summary>
        CircleMinus = SciMsg.SC_MARK_CIRCLEMINUS,

        /// <summary>
        /// Similar to a BoxMinusConnected, but surrounded by a circle.
        /// </summary>
        CircleMinusConnected = SciMsg.SC_MARK_CIRCLEMINUSCONNECTED,

        /// <summary>
        /// A special marker that displays no symbol but will affect the background color of the line.
        /// </summary>
        Background = SciMsg.SC_MARK_BACKGROUND,

        /// <summary>
        /// Three dots (ellipsis).
        /// </summary>
        DotDotDot = SciMsg.SC_MARK_DOTDOTDOT,

        /// <summary>
        /// Three bracket style arrows.
        /// </summary>
        Arrows = SciMsg.SC_MARK_ARROWS,

        // PixMap = SciMsg.SC_MARK_PIXMAP,

        /// <summary>
        /// A rectangle occupying the entire marker space.
        /// </summary>
        FullRect = SciMsg.SC_MARK_FULLRECT,

        /// <summary>
        /// A rectangle occupying only the left edge of the marker space.
        /// </summary>
        LeftRect = SciMsg.SC_MARK_LEFTRECT,

        /// <summary>
        /// A special marker left available to plugins.
        /// </summary>
        Available = SciMsg.SC_MARK_AVAILABLE,

        /// <summary>
        /// A special marker that displays no symbol but will underline the current line text.
        /// </summary>
        Underline = SciMsg.SC_MARK_UNDERLINE,

        /// <summary>
        /// A user-defined image. Images can be set using the Npp.Marker.DefineRgbaImage method.
        /// </summary>
        RgbaImage = SciMsg.SC_MARK_RGBAIMAGE,

        /// <summary>
        /// A left-rotated bookmark.
        /// </summary>
        Bookmark = SciMsg.SC_MARK_BOOKMARK

        // Character = SciMsg.SC_MARK_CHARACTER
    }

    /// <summary>
    /// Flags for additional line fold level behavior.
    /// </summary>
    [Flags]
    public enum FoldLevelFlags : uint {
        /// <summary>
        /// Indicates that the line is blank and should be treated slightly different than its level may indicate;
        /// otherwise, blank lines should generally not be fold points.
        /// </summary>
        White = SciMsg.SC_FOLDLEVELWHITEFLAG,

        /// <summary>
        /// Indicates that the line is a header (fold point).
        /// </summary>
        Header = SciMsg.SC_FOLDLEVELHEADERFLAG
    }

    #endregion

    [Flags]
    public enum SciMarkerStyle : uint {
        SC_MARK_CIRCLE = 0,
        SC_MARK_ROUNDRECT = 1,
        SC_MARK_ARROW = 2,
        SC_MARK_SMALLRECT = 3,
        SC_MARK_SHORTARROW = 4,
        SC_MARK_EMPTY = 5,
        SC_MARK_ARROWDOWN = 6,
        SC_MARK_MINUS = 7,
        SC_MARK_PLUS = 8,
        SC_MARK_VLINE = 9,
        SC_MARK_LCORNER = 10,
        SC_MARK_TCORNER = 11,
        SC_MARK_BOXPLUS = 12,
        SC_MARK_BOXPLUSCONNECTED = 13,
        SC_MARK_BOXMINUS = 14,
        SC_MARK_BOXMINUSCONNECTED = 15,
        SC_MARK_LCORNERCURVE = 16,
        SC_MARK_TCORNERCURVE = 17,
        SC_MARK_CIRCLEPLUS = 18,
        SC_MARK_CIRCLEPLUSCONNECTED = 19,
        SC_MARK_CIRCLEMINUS = 20,
        SC_MARK_CIRCLEMINUSCONNECTED = 21,
        SC_MARK_BACKGROUND = 22,
        SC_MARK_DOTDOTDOT = 23,
        SC_MARK_ARROWS = 24,
        SC_MARK_PIXMAP = 25,
        SC_MARK_FULLRECT = 26,
        SC_MARK_LEFTRECT = 27,
        SC_MARK_AVAILABLE = 28,
        SC_MARK_UNDERLINE = 29,
        SC_MARK_RGBAIMAGE = 30,
        SC_MARK_CHARACTER = 10000
    }

    [Flags]
    public enum SciMarginType : uint {
        SC_MARGIN_SYMBOL = 0,
        SC_MARGIN_NUMBER = 1,
        SC_MARGIN_BACK = 2,
        SC_MARGIN_FORE = 3,
        SC_MARGIN_TEXT = 4,
        SC_MARGIN_RTEXT = 5
    }

    [Flags]
    public enum SciIndicatorType : uint {
        INDIC_PLAIN = 0,
        INDIC_SQUIGGLE = 1,
        INDIC_TT = 2,
        INDIC_DIAGONAL = 3,
        INDIC_STRIKE = 4,
        INDIC_HIDDEN = 5,
        INDIC_BOX = 6,
        INDIC_ROUNDBOX = 7,
        INDIC_STRAIGHTBOX = 8,
        INDIC_DASH = 9,
        INDIC_DOTS = 10,
        INDIC_SQUIGGLELOW = 11,
        INDIC_DOTBOX = 12,
        INDIC_SQUIGGLEPIXMAP = 13,
        INDIC_COMPOSITIONTHICK = 14,
        INDIC_COMPOSITIONTHIN = 15,
        INDIC_TEXTFORE = 17
    }

    public enum SciNotif : uint {
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
    public enum SciMsg : uint {
        // Autocompletions
        SC_AC_FILLUP = 1,
        SC_AC_DOUBLECLICK = 2,
        SC_AC_TAB = 3,
        SC_AC_NEWLINE = 4,
        SC_AC_COMMAND = 5,

        // Annotations
        ANNOTATION_HIDDEN = 0,
        ANNOTATION_STANDARD = 1,
        ANNOTATION_BOXED = 2,
        ANNOTATION_INDENTED = 3,

        // Indentation 
        SC_IV_NONE = 0,
        SC_IV_REAL = 1,
        SC_IV_LOOKFORWARD = 2,
        SC_IV_LOOKBOTH = 3,

        // Keys
        SCMOD_NORM = 0,
        SCMOD_SHIFT = 1,
        SCMOD_CTRL = 2,
        SCMOD_ALT = 4,
        SCMOD_SUPER = 8,
        SCMOD_META = 16,

        SCI_NORM = 0,
        SCI_SHIFT = SCMOD_SHIFT,
        SCI_CTRL = SCMOD_CTRL,
        SCI_ALT = SCMOD_ALT,
        SCI_META = SCMOD_META,
        SCI_CSHIFT = (SCI_CTRL | SCI_SHIFT),
        SCI_ASHIFT = (SCI_ALT | SCI_SHIFT),

        // Caret styles
        CARETSTYLE_INVISIBLE = 0,
        CARETSTYLE_LINE = 1,
        CARETSTYLE_BLOCK = 2,

        // Line edges
        EDGE_NONE = 0,
        EDGE_LINE = 1,
        EDGE_BACKGROUND = 2,

        // Indicators
        INDIC_PLAIN = 0,
        INDIC_SQUIGGLE = 1,
        INDIC_TT = 2,
        INDIC_DIAGONAL = 3,
        INDIC_STRIKE = 4,
        INDIC_HIDDEN = 5,
        INDIC_BOX = 6,
        INDIC_ROUNDBOX = 7,
        INDIC_STRAIGHTBOX = 8,
        INDIC_DASH = 9,
        INDIC_DOTS = 10,
        INDIC_SQUIGGLELOW = 11,
        INDIC_DOTBOX = 12,
        INDIC_SQUIGGLEPIXMAP = 13,
        INDIC_COMPOSITIONTHICK = 14,
        INDIC_COMPOSITIONTHIN = 15,
        INDIC_FULLBOX = 16,
        INDIC_TEXTFORE = 17,
        INDIC_MAX = 31,
        INDIC_CONTAINER = 8,

        // Phases
        SC_PHASES_ONE = 0,
        SC_PHASES_TWO = 1,
        SC_PHASES_MULTIPLE = 2,

        // Indicator flags
        SC_INDICFLAG_VALUEFORE = 1,
        SC_INDICVALUEBIT = 0x1000000,
        SC_INDICVALUEMASK = 0xFFFFFF,

        // public const int INDIC0_MASK = 0x20,
        // public const int INDIC1_MASK = 0x40,
        // public const int INDIC2_MASK = 0x80,
        // public const int INDICS_MASK = 0xE0,

        KEYWORDSET_MAX = 8,

        // Alpha ranges
        SC_ALPHA_TRANSPARENT = 0,
        SC_ALPHA_OPAQUE = 255,
        SC_ALPHA_NOALPHA = 256,

        // Automatic folding
        SC_AUTOMATICFOLD_SHOW = 0x0001,
        SC_AUTOMATICFOLD_CLICK = 0x0002,
        SC_AUTOMATICFOLD_CHANGE = 0x0004,

        // Caret sticky behavior
        SC_CARETSTICKY_OFF = 0,
        SC_CARETSTICKY_ON = 1,
        SC_CARETSTICKY_WHITESPACE = 2,

        // Encodings
        SC_CP_UTF8 = 65001,

        // Font quality
        SC_EFF_QUALITY_DEFAULT = 0,
        SC_EFF_QUALITY_NON_ANTIALIASED = 1,
        SC_EFF_QUALITY_ANTIALIASED = 2,
        SC_EFF_QUALITY_LCD_OPTIMIZED = 3,

        // End-of-line
        SC_EOL_CRLF = 0,
        SC_EOL_CR = 1,
        SC_EOL_LF = 2,

        // Fold action
        SC_FOLDACTION_CONTRACT = 0,
        SC_FOLDACTION_EXPAND = 1,
        SC_FOLDACTION_TOGGLE = 2,

        // Fold level
        SC_FOLDLEVELBASE = 0x400,
        SC_FOLDLEVELWHITEFLAG = 0x1000,
        SC_FOLDLEVELHEADERFLAG = 0x2000,
        SC_FOLDLEVELNUMBERMASK = 0x0FFF,

        // Fold flags
        SC_FOLDFLAG_LINEBEFORE_EXPANDED = 0x0002,
        SC_FOLDFLAG_LINEBEFORE_CONTRACTED = 0x0004,
        SC_FOLDFLAG_LINEAFTER_EXPANDED = 0x0008,
        SC_FOLDFLAG_LINEAFTER_CONTRACTED = 0x0010,
        SC_FOLDFLAG_LEVELNUMBERS = 0x0040,
        SC_FOLDFLAG_LINESTATE = 0x0080,

        // Line end type
        SC_LINE_END_TYPE_DEFAULT = 0,
        SC_LINE_END_TYPE_UNICODE = 1,

        // Margins
        SC_MAX_MARGIN = 4,

        SC_MARGIN_SYMBOL = 0,
        SC_MARGIN_NUMBER = 1,
        SC_MARGIN_BACK = 2,
        SC_MARGIN_FORE = 3,
        SC_MARGIN_TEXT = 4,
        SC_MARGIN_RTEXT = 5,

        SC_MARGINOPTION_NONE = 0,
        SC_MARGINOPTION_SUBLINESELECT = 1,

        // Markers
        MARKER_MAX = 31,
        SC_MARK_CIRCLE = 0,
        SC_MARK_ROUNDRECT = 1,
        SC_MARK_ARROW = 2,
        SC_MARK_SMALLRECT = 3,
        SC_MARK_SHORTARROW = 4,
        SC_MARK_EMPTY = 5,
        SC_MARK_ARROWDOWN = 6,
        SC_MARK_MINUS = 7,
        SC_MARK_PLUS = 8,
        SC_MARK_VLINE = 9,
        SC_MARK_LCORNER = 10,
        SC_MARK_TCORNER = 11,
        SC_MARK_BOXPLUS = 12,
        SC_MARK_BOXPLUSCONNECTED = 13,
        SC_MARK_BOXMINUS = 14,
        SC_MARK_BOXMINUSCONNECTED = 15,
        SC_MARK_LCORNERCURVE = 16,
        SC_MARK_TCORNERCURVE = 17,
        SC_MARK_CIRCLEPLUS = 18,
        SC_MARK_CIRCLEPLUSCONNECTED = 19,
        SC_MARK_CIRCLEMINUS = 20,
        SC_MARK_CIRCLEMINUSCONNECTED = 21,
        SC_MARK_BACKGROUND = 22,
        SC_MARK_DOTDOTDOT = 23,
        SC_MARK_ARROWS = 24,
        SC_MARK_PIXMAP = 25,
        SC_MARK_FULLRECT = 26,
        SC_MARK_LEFTRECT = 27,
        SC_MARK_AVAILABLE = 28,
        SC_MARK_UNDERLINE = 29,
        SC_MARK_RGBAIMAGE = 30,
        SC_MARK_BOOKMARK = 31,
        SC_MARK_CHARACTER = 10000,
        SC_MARKNUM_FOLDEREND = 25,
        SC_MARKNUM_FOLDEROPENMID = 26,
        SC_MARKNUM_FOLDERMIDTAIL = 27,
        SC_MARKNUM_FOLDERTAIL = 28,
        SC_MARKNUM_FOLDERSUB = 29,
        SC_MARKNUM_FOLDER = 30,
        SC_MARKNUM_FOLDEROPEN = 31,
        //SC_MASK_FOLDERS = (int)0xFE000000,

        SC_MULTIPASTE_ONCE = 0,
        SC_MULTIPASTE_EACH = 1,

        SC_ORDER_PRESORTED = 0,
        SC_ORDER_PERFORMSORT = 1,
        SC_ORDER_CUSTOM = 2,

        // Update notification reasons
        SC_UPDATE_CONTENT = 0x01,
        SC_UPDATE_SELECTION = 0x02,
        SC_UPDATE_V_SCROLL = 0x04,
        SC_UPDATE_H_SCROLL = 0x08,

        // Modified notification types
        SC_MOD_INSERTTEXT = 0x1,
        SC_MOD_DELETETEXT = 0x2,
        SC_MOD_BEFOREINSERT = 0x400,
        SC_MOD_BEFOREDELETE = 0x800,
        SC_MOD_CHANGEANNOTATION = 0x20000,
        SC_MOD_INSERTCHECK = 0x100000,

        // Modified flags
        SC_PERFORMED_USER = 0x10,
        SC_PERFORMED_UNDO = 0x20,
        SC_PERFORMED_REDO = 0x40,

        // Status codes
        SC_STATUS_OK = 0,
        SC_STATUS_FAILURE = 1,
        SC_STATUS_BADALLOC = 2,

        // Dwell
        SC_TIME_FOREVER = 10000000,

        // Property types
        SC_TYPE_BOOLEAN = 0,
        SC_TYPE_INTEGER = 1,
        SC_TYPE_STRING = 2,

        // Search flags
        SCFIND_WHOLEWORD = 0x2,
        SCFIND_MATCHCASE = 0x4,
        SCFIND_WORDSTART = 0x00100000,
        SCFIND_REGEXP = 0x00200000,
        SCFIND_POSIX = 0x00400000,
        SCFIND_CXX11REGEX = 0x00800000,

        // Functions
        SCI_POSITIONAFTER = 2418,
        SCI_START = 2000,
        SCI_OPTIONAL_START = 3000,
        SCI_LEXER_START = 4000,
        SCI_ADDTEXT = 2001,
        SCI_ADDSTYLEDTEXT = 2002,
        SCI_INSERTTEXT = 2003,
        SCI_CHANGEINSERTION = 2672,
        SCI_CLEARALL = 2004,
        SCI_DELETERANGE = 2645,
        SCI_CLEARDOCUMENTSTYLE = 2005,
        SCI_GETLENGTH = 2006,
        SCI_GETCHARAT = 2007,
        SCI_GETCURRENTPOS = 2008,
        SCI_GETANCHOR = 2009,
        SCI_GETSTYLEAT = 2010,
        SCI_REDO = 2011,
        SCI_SETUNDOCOLLECTION = 2012,
        SCI_SELECTALL = 2013,
        SCI_SETSAVEPOINT = 2014,
        SCI_GETSTYLEDTEXT = 2015,
        SCI_CANREDO = 2016,
        SCI_MARKERLINEFROMHANDLE = 2017,
        SCI_MARKERDELETEHANDLE = 2018,
        SCI_GETUNDOCOLLECTION = 2019,
        SCI_GETVIEWWS = 2020,
        SCI_SETVIEWWS = 2021,
        SCI_POSITIONFROMPOINT = 2022,
        SCI_POSITIONFROMPOINTCLOSE = 2023,
        SCI_GOTOLINE = 2024,
        SCI_GOTOPOS = 2025,
        SCI_SETANCHOR = 2026,
        SCI_GETCURLINE = 2027,
        SCI_GETENDSTYLED = 2028,
        SCI_CONVERTEOLS = 2029,
        SCI_GETEOLMODE = 2030,
        SCI_SETEOLMODE = 2031,
        SCI_STARTSTYLING = 2032,
        SCI_SETSTYLING = 2033,
        SCI_GETBUFFEREDDRAW = 2034,
        SCI_SETBUFFEREDDRAW = 2035,
        SCI_SETTABWIDTH = 2036,
        SCI_GETTABWIDTH = 2121,
        SCI_CLEARTABSTOPS = 2675,
        SCI_ADDTABSTOP = 2676,
        SCI_GETNEXTTABSTOP = 2677,
        SCI_SETCODEPAGE = 2037,
        SCI_MARKERDEFINE = 2040,
        SCI_MARKERSETFORE = 2041,
        SCI_MARKERSETBACK = 2042,
        SCI_MARKERSETBACKSELECTED = 2292,
        SCI_MARKERENABLEHIGHLIGHT = 2293,
        SCI_MARKERADD = 2043,
        SCI_MARKERDELETE = 2044,
        SCI_MARKERDELETEALL = 2045,
        SCI_MARKERGET = 2046,
        SCI_MARKERNEXT = 2047,
        SCI_MARKERPREVIOUS = 2048,
        SCI_MARKERDEFINEPIXMAP = 2049,
        SCI_MARKERADDSET = 2466,
        SCI_MARKERSETALPHA = 2476,
        SCI_SETMARGINTYPEN = 2240,
        SCI_GETMARGINTYPEN = 2241,
        SCI_SETMARGINWIDTHN = 2242,
        SCI_GETMARGINWIDTHN = 2243,
        SCI_SETMARGINMASKN = 2244,
        SCI_GETMARGINMASKN = 2245,
        SCI_SETMARGINSENSITIVEN = 2246,
        SCI_GETMARGINSENSITIVEN = 2247,
        SCI_SETMARGINCURSORN = 2248,
        SCI_GETMARGINCURSORN = 2249,
        SCI_STYLECLEARALL = 2050,
        SCI_STYLESETFORE = 2051,
        SCI_STYLESETBACK = 2052,
        SCI_STYLESETBOLD = 2053,
        SCI_STYLESETITALIC = 2054,
        SCI_STYLESETSIZE = 2055,
        SCI_STYLESETFONT = 2056,
        SCI_STYLESETEOLFILLED = 2057,
        SCI_STYLERESETDEFAULT = 2058,
        SCI_STYLESETUNDERLINE = 2059,
        SCI_STYLEGETFORE = 2481,
        SCI_STYLEGETBACK = 2482,
        SCI_STYLEGETBOLD = 2483,
        SCI_STYLEGETITALIC = 2484,
        SCI_STYLEGETSIZE = 2485,
        SCI_STYLEGETFONT = 2486,
        SCI_STYLEGETEOLFILLED = 2487,
        SCI_STYLEGETUNDERLINE = 2488,
        SCI_STYLEGETCASE = 2489,
        SCI_STYLEGETCHARACTERSET = 2490,
        SCI_STYLEGETVISIBLE = 2491,
        SCI_STYLEGETCHANGEABLE = 2492,
        SCI_STYLEGETHOTSPOT = 2493,
        SCI_STYLESETCASE = 2060,
        SCI_STYLESETSIZEFRACTIONAL = 2061,
        SCI_STYLEGETSIZEFRACTIONAL = 2062,
        SCI_STYLESETWEIGHT = 2063,
        SCI_STYLEGETWEIGHT = 2064,
        SCI_STYLESETCHARACTERSET = 2066,
        SCI_STYLESETHOTSPOT = 2409,
        SCI_SETSELFORE = 2067,
        SCI_SETSELBACK = 2068,
        SCI_GETSELALPHA = 2477,
        SCI_SETSELALPHA = 2478,
        SCI_GETSELEOLFILLED = 2479,
        SCI_SETSELEOLFILLED = 2480,
        SCI_SETCARETFORE = 2069,
        SCI_ASSIGNCMDKEY = 2070,
        SCI_CLEARCMDKEY = 2071,
        SCI_CLEARALLCMDKEYS = 2072,
        SCI_SETSTYLINGEX = 2073,
        SCI_STYLESETVISIBLE = 2074,
        SCI_GETCARETPERIOD = 2075,
        SCI_SETCARETPERIOD = 2076,
        SCI_SETWORDCHARS = 2077,
        SCI_GETWORDCHARS = 2646,
        SCI_BEGINUNDOACTION = 2078,
        SCI_ENDUNDOACTION = 2079,
        SCI_INDICSETSTYLE = 2080,
        SCI_INDICGETSTYLE = 2081,
        SCI_INDICSETFORE = 2082,
        SCI_INDICGETFORE = 2083,
        SCI_INDICSETUNDER = 2510,
        SCI_INDICGETUNDER = 2511,
        SCI_INDICSETHOVERSTYLE = 2680,
        SCI_INDICGETHOVERSTYLE = 2681,
        SCI_INDICSETHOVERFORE = 2682,
        SCI_INDICGETHOVERFORE = 2683,
        SCI_INDICSETFLAGS = 2684,
        SCI_INDICGETFLAGS = 2685,
        SCI_SETWHITESPACEFORE = 2084,
        SCI_SETWHITESPACEBACK = 2085,
        SCI_SETWHITESPACESIZE = 2086,
        SCI_GETWHITESPACESIZE = 2087,
        SCI_SETLINESTATE = 2092,
        SCI_GETLINESTATE = 2093,
        SCI_GETMAXLINESTATE = 2094,
        SCI_GETCARETLINEVISIBLE = 2095,
        SCI_SETCARETLINEVISIBLE = 2096,
        SCI_GETCARETLINEBACK = 2097,
        SCI_SETCARETLINEBACK = 2098,
        SCI_STYLESETCHANGEABLE = 2099,
        SCI_AUTOCSHOW = 2100,
        SCI_AUTOCCANCEL = 2101,
        SCI_AUTOCACTIVE = 2102,
        SCI_AUTOCPOSSTART = 2103,
        SCI_AUTOCCOMPLETE = 2104,
        SCI_AUTOCSTOPS = 2105,
        SCI_AUTOCSETSEPARATOR = 2106,
        SCI_AUTOCGETSEPARATOR = 2107,
        SCI_AUTOCSELECT = 2108,
        SCI_AUTOCSETCANCELATSTART = 2110,
        SCI_AUTOCGETCANCELATSTART = 2111,
        SCI_AUTOCSETFILLUPS = 2112,
        SCI_AUTOCSETCHOOSESINGLE = 2113,
        SCI_AUTOCGETCHOOSESINGLE = 2114,
        SCI_AUTOCSETIGNORECASE = 2115,
        SCI_AUTOCGETIGNORECASE = 2116,
        SCI_USERLISTSHOW = 2117,
        SCI_AUTOCSETAUTOHIDE = 2118,
        SCI_AUTOCGETAUTOHIDE = 2119,
        SCI_AUTOCSETDROPRESTOFWORD = 2270,
        SCI_AUTOCGETDROPRESTOFWORD = 2271,
        SCI_REGISTERIMAGE = 2405,
        SCI_CLEARREGISTEREDIMAGES = 2408,
        SCI_AUTOCGETTYPESEPARATOR = 2285,
        SCI_AUTOCSETTYPESEPARATOR = 2286,
        SCI_AUTOCSETMAXWIDTH = 2208,
        SCI_AUTOCGETMAXWIDTH = 2209,
        SCI_AUTOCSETMAXHEIGHT = 2210,
        SCI_AUTOCGETMAXHEIGHT = 2211,
        SCI_SETINDENT = 2122,
        SCI_GETINDENT = 2123,
        SCI_SETUSETABS = 2124,
        SCI_GETUSETABS = 2125,
        SCI_SETLINEINDENTATION = 2126,
        SCI_GETLINEINDENTATION = 2127,
        SCI_GETLINEINDENTPOSITION = 2128,
        SCI_GETCOLUMN = 2129,
        SCI_COUNTCHARACTERS = 2633,
        SCI_SETHSCROLLBAR = 2130,
        SCI_GETHSCROLLBAR = 2131,
        SCI_SETINDENTATIONGUIDES = 2132,
        SCI_GETINDENTATIONGUIDES = 2133,
        SCI_SETHIGHLIGHTGUIDE = 2134,
        SCI_GETHIGHLIGHTGUIDE = 2135,
        SCI_GETLINEENDPOSITION = 2136,
        SCI_GETCODEPAGE = 2137,
        SCI_GETCARETFORE = 2138,
        SCI_GETREADONLY = 2140,
        SCI_SETCURRENTPOS = 2141,
        SCI_SETSELECTIONSTART = 2142,
        SCI_GETSELECTIONSTART = 2143,
        SCI_SETSELECTIONEND = 2144,
        SCI_GETSELECTIONEND = 2145,
        SCI_SETEMPTYSELECTION = 2556,
        SCI_SETPRINTMAGNIFICATION = 2146,
        SCI_GETPRINTMAGNIFICATION = 2147,
        SCI_SETPRINTCOLOURMODE = 2148,
        SCI_GETPRINTCOLOURMODE = 2149,
        SCI_FINDTEXT = 2150,
        SCI_FORMATRANGE = 2151,
        SCI_GETFIRSTVISIBLELINE = 2152,
        SCI_GETLINE = 2153,
        SCI_GETLINECOUNT = 2154,
        SCI_SETMARGINLEFT = 2155,
        SCI_GETMARGINLEFT = 2156,
        SCI_SETMARGINRIGHT = 2157,
        SCI_GETMARGINRIGHT = 2158,
        SCI_GETMODIFY = 2159,
        SCI_SETSEL = 2160,
        SCI_GETSELTEXT = 2161,
        SCI_GETTEXTRANGE = 2162,
        SCI_HIDESELECTION = 2163,
        SCI_POINTXFROMPOSITION = 2164,
        SCI_POINTYFROMPOSITION = 2165,
        SCI_LINEFROMPOSITION = 2166,
        SCI_POSITIONFROMLINE = 2167,
        SCI_LINESCROLL = 2168,
        SCI_SCROLLCARET = 2169,
        SCI_SCROLLRANGE = 2569,
        SCI_REPLACESEL = 2170,
        SCI_SETREADONLY = 2171,
        SCI_NULL = 2172,
        SCI_CANPASTE = 2173,
        SCI_CANUNDO = 2174,
        SCI_EMPTYUNDOBUFFER = 2175,
        SCI_UNDO = 2176,
        SCI_CUT = 2177,
        SCI_COPY = 2178,
        SCI_PASTE = 2179,
        SCI_CLEAR = 2180,
        SCI_SETTEXT = 2181,
        SCI_GETTEXT = 2182,
        SCI_GETTEXTLENGTH = 2183,
        SCI_GETDIRECTFUNCTION = 2184,
        SCI_GETDIRECTPOINTER = 2185,
        SCI_SETOVERTYPE = 2186,
        SCI_GETOVERTYPE = 2187,
        SCI_SETCARETWIDTH = 2188,
        SCI_GETCARETWIDTH = 2189,
        SCI_SETTARGETSTART = 2190,
        SCI_GETTARGETSTART = 2191,
        SCI_SETTARGETEND = 2192,
        SCI_GETTARGETEND = 2193,
        SCI_REPLACETARGET = 2194,
        SCI_REPLACETARGETRE = 2195,
        SCI_SEARCHINTARGET = 2197,
        SCI_SETSEARCHFLAGS = 2198,
        SCI_GETSEARCHFLAGS = 2199,
        SCI_CALLTIPSHOW = 2200,
        SCI_CALLTIPCANCEL = 2201,
        SCI_CALLTIPACTIVE = 2202,
        SCI_CALLTIPPOSSTART = 2203,
        SCI_CALLTIPSETPOSSTART = 2214,
        SCI_CALLTIPSETHLT = 2204,
        SCI_CALLTIPSETBACK = 2205,
        SCI_CALLTIPSETFORE = 2206,
        SCI_CALLTIPSETFOREHLT = 2207,
        SCI_CALLTIPUSESTYLE = 2212,
        SCI_CALLTIPSETPOSITION = 2213,
        SCI_VISIBLEFROMDOCLINE = 2220,
        SCI_DOCLINEFROMVISIBLE = 2221,
        SCI_WRAPCOUNT = 2235,
        SCI_SETFOLDLEVEL = 2222,
        SCI_GETFOLDLEVEL = 2223,
        SCI_GETLASTCHILD = 2224,
        SCI_GETFOLDPARENT = 2225,
        SCI_SHOWLINES = 2226,
        SCI_HIDELINES = 2227,
        SCI_GETLINEVISIBLE = 2228,
        SCI_GETALLLINESVISIBLE = 2236,
        SCI_SETFOLDEXPANDED = 2229,
        SCI_GETFOLDEXPANDED = 2230,
        SCI_TOGGLEFOLD = 2231,
        SCI_FOLDLINE = 2237,
        SCI_FOLDCHILDREN = 2238,
        SCI_EXPANDCHILDREN = 2239,
        SCI_FOLDALL = 2662,
        SCI_ENSUREVISIBLE = 2232,
        SCI_SETAUTOMATICFOLD = 2663,
        SCI_GETAUTOMATICFOLD = 2664,
        SCI_SETFOLDFLAGS = 2233,
        SCI_ENSUREVISIBLEENFORCEPOLICY = 2234,
        SCI_SETTABINDENTS = 2260,
        SCI_GETTABINDENTS = 2261,
        SCI_SETBACKSPACEUNINDENTS = 2262,
        SCI_GETBACKSPACEUNINDENTS = 2263,
        SCI_SETMOUSEDWELLTIME = 2264,
        SCI_GETMOUSEDWELLTIME = 2265,
        SCI_WORDSTARTPOSITION = 2266,
        SCI_WORDENDPOSITION = 2267,
        SCI_ISRANGEWORD = 2691,
        SCI_SETWRAPMODE = 2268,
        SCI_GETWRAPMODE = 2269,
        SCI_SETWRAPVISUALFLAGS = 2460,
        SCI_GETWRAPVISUALFLAGS = 2461,
        SCI_SETWRAPVISUALFLAGSLOCATION = 2462,
        SCI_GETWRAPVISUALFLAGSLOCATION = 2463,
        SCI_SETWRAPSTARTINDENT = 2464,
        SCI_GETWRAPSTARTINDENT = 2465,
        SCI_SETWRAPINDENTMODE = 2472,
        SCI_GETWRAPINDENTMODE = 2473,
        SCI_SETLAYOUTCACHE = 2272,
        SCI_GETLAYOUTCACHE = 2273,
        SCI_SETSCROLLWIDTH = 2274,
        SCI_GETSCROLLWIDTH = 2275,
        SCI_SETSCROLLWIDTHTRACKING = 2516,
        SCI_GETSCROLLWIDTHTRACKING = 2517,
        SCI_TEXTWIDTH = 2276,
        SCI_SETENDATLASTLINE = 2277,
        SCI_GETENDATLASTLINE = 2278,
        SCI_TEXTHEIGHT = 2279,
        SCI_SETVSCROLLBAR = 2280,
        SCI_GETVSCROLLBAR = 2281,
        SCI_APPENDTEXT = 2282,
        SCI_GETTWOPHASEDRAW = 2283,
        SCI_SETTWOPHASEDRAW = 2284,
        SCI_GETPHASESDRAW = 2673,
        SCI_SETPHASESDRAW = 2674,
        SCI_SETFONTQUALITY = 2611,
        SCI_GETFONTQUALITY = 2612,
        SCI_SETFIRSTVISIBLELINE = 2613,
        SCI_SETMULTIPASTE = 2614,
        SCI_GETMULTIPASTE = 2615,
        SCI_GETTAG = 2616,
        SCI_TARGETFROMSELECTION = 2287,
        SCI_TARGETWHOLEDOCUMENT = 2690,
        SCI_LINESJOIN = 2288,
        SCI_LINESSPLIT = 2289,
        SCI_SETFOLDMARGINCOLOUR = 2290,
        SCI_SETFOLDMARGINHICOLOUR = 2291,
        SCI_LINEDOWN = 2300,
        SCI_LINEDOWNEXTEND = 2301,
        SCI_LINEUP = 2302,
        SCI_LINEUPEXTEND = 2303,
        SCI_CHARLEFT = 2304,
        SCI_CHARLEFTEXTEND = 2305,
        SCI_CHARRIGHT = 2306,
        SCI_CHARRIGHTEXTEND = 2307,
        SCI_WORDLEFT = 2308,
        SCI_WORDLEFTEXTEND = 2309,
        SCI_WORDRIGHT = 2310,
        SCI_WORDRIGHTEXTEND = 2311,
        SCI_HOME = 2312,
        SCI_HOMEEXTEND = 2313,
        SCI_LINEEND = 2314,
        SCI_LINEENDEXTEND = 2315,
        SCI_DOCUMENTSTART = 2316,
        SCI_DOCUMENTSTARTEXTEND = 2317,
        SCI_DOCUMENTEND = 2318,
        SCI_DOCUMENTENDEXTEND = 2319,
        SCI_PAGEUP = 2320,
        SCI_PAGEUPEXTEND = 2321,
        SCI_PAGEDOWN = 2322,
        SCI_PAGEDOWNEXTEND = 2323,
        SCI_EDITTOGGLEOVERTYPE = 2324,
        SCI_CANCEL = 2325,
        SCI_DELETEBACK = 2326,
        SCI_TAB = 2327,
        SCI_BACKTAB = 2328,
        SCI_NEWLINE = 2329,
        SCI_FORMFEED = 2330,
        SCI_VCHOME = 2331,
        SCI_VCHOMEEXTEND = 2332,
        SCI_ZOOMIN = 2333,
        SCI_ZOOMOUT = 2334,
        SCI_DELWORDLEFT = 2335,
        SCI_DELWORDRIGHT = 2336,
        SCI_DELWORDRIGHTEND = 2518,
        SCI_LINECUT = 2337,
        SCI_LINEDELETE = 2338,
        SCI_LINETRANSPOSE = 2339,
        SCI_LINEDUPLICATE = 2404,
        SCI_LOWERCASE = 2340,
        SCI_UPPERCASE = 2341,
        SCI_LINESCROLLDOWN = 2342,
        SCI_LINESCROLLUP = 2343,
        SCI_DELETEBACKNOTLINE = 2344,
        SCI_HOMEDISPLAY = 2345,
        SCI_HOMEDISPLAYEXTEND = 2346,
        SCI_LINEENDDISPLAY = 2347,
        SCI_LINEENDDISPLAYEXTEND = 2348,
        SCI_HOMEWRAP = 2349,
        SCI_HOMEWRAPEXTEND = 2450,
        SCI_LINEENDWRAP = 2451,
        SCI_LINEENDWRAPEXTEND = 2452,
        SCI_VCHOMEWRAP = 2453,
        SCI_VCHOMEWRAPEXTEND = 2454,
        SCI_LINECOPY = 2455,
        SCI_MOVECARETINSIDEVIEW = 2401,
        SCI_LINELENGTH = 2350,
        SCI_BRACEHIGHLIGHT = 2351,
        SCI_BRACEHIGHLIGHTINDICATOR = 2498,
        SCI_BRACEBADLIGHT = 2352,
        SCI_BRACEBADLIGHTINDICATOR = 2499,
        SCI_BRACEMATCH = 2353,
        SCI_GETVIEWEOL = 2355,
        SCI_SETVIEWEOL = 2356,
        SCI_GETDOCPOINTER = 2357,
        SCI_SETDOCPOINTER = 2358,
        SCI_SETMODEVENTMASK = 2359,
        SCI_GETEDGECOLUMN = 2360,
        SCI_SETEDGECOLUMN = 2361,
        SCI_GETEDGEMODE = 2362,
        SCI_SETEDGEMODE = 2363,
        SCI_GETEDGECOLOUR = 2364,
        SCI_SETEDGECOLOUR = 2365,
        SCI_SEARCHANCHOR = 2366,
        SCI_SEARCHNEXT = 2367,
        SCI_SEARCHPREV = 2368,
        SCI_LINESONSCREEN = 2370,
        SCI_USEPOPUP = 2371,
        SCI_SELECTIONISRECTANGLE = 2372,
        SCI_SETZOOM = 2373,
        SCI_GETZOOM = 2374,
        SCI_CREATEDOCUMENT = 2375,
        SCI_ADDREFDOCUMENT = 2376,
        SCI_RELEASEDOCUMENT = 2377,
        SCI_GETMODEVENTMASK = 2378,
        SCI_SETFOCUS = 2380,
        SCI_GETFOCUS = 2381,
        SCI_SETSTATUS = 2382,
        SCI_GETSTATUS = 2383,
        SCI_SETMOUSEDOWNCAPTURES = 2384,
        SCI_GETMOUSEDOWNCAPTURES = 2385,
        SCI_SETCURSOR = 2386,
        SCI_GETCURSOR = 2387,
        SCI_SETCONTROLCHARSYMBOL = 2388,
        SCI_GETCONTROLCHARSYMBOL = 2389,
        SCI_WORDPARTLEFT = 2390,
        SCI_WORDPARTLEFTEXTEND = 2391,
        SCI_WORDPARTRIGHT = 2392,
        SCI_WORDPARTRIGHTEXTEND = 2393,
        SCI_SETVISIBLEPOLICY = 2394,
        SCI_DELLINELEFT = 2395,
        SCI_DELLINERIGHT = 2396,
        SCI_SETXOFFSET = 2397,
        SCI_GETXOFFSET = 2398,
        SCI_CHOOSECARETX = 2399,
        SCI_GRABFOCUS = 2400,
        SCI_SETXCARETPOLICY = 2402,
        SCI_SETYCARETPOLICY = 2403,
        SCI_SETPRINTWRAPMODE = 2406,
        SCI_GETPRINTWRAPMODE = 2407,
        SCI_SETHOTSPOTACTIVEFORE = 2410,
        SCI_GETHOTSPOTACTIVEFORE = 2494,
        SCI_SETHOTSPOTACTIVEBACK = 2411,
        SCI_GETHOTSPOTACTIVEBACK = 2495,
        SCI_SETHOTSPOTACTIVEUNDERLINE = 2412,
        SCI_GETHOTSPOTACTIVEUNDERLINE = 2496,
        SCI_SETHOTSPOTSINGLELINE = 2421,
        SCI_GETHOTSPOTSINGLELINE = 2497,
        SCI_PARADOWN = 2413,
        SCI_PARADOWNEXTEND = 2414,
        SCI_PARAUP = 2415,
        SCI_PARAUPEXTEND = 2416,
        SCI_POSITIONRELATIVE = 2670,
        SCI_COPYRANGE = 2419,
        SCI_COPYTEXT = 2420,
        SCI_SETSELECTIONMODE = 2422,
        SCI_GETSELECTIONMODE = 2423,
        SCI_GETLINESELSTARTPOSITION = 2424,
        SCI_GETLINESELENDPOSITION = 2425,
        SCI_LINEDOWNRECTEXTEND = 2426,
        SCI_LINEUPRECTEXTEND = 2427,
        SCI_CHARLEFTRECTEXTEND = 2428,
        SCI_CHARRIGHTRECTEXTEND = 2429,
        SCI_HOMERECTEXTEND = 2430,
        SCI_VCHOMERECTEXTEND = 2431,
        SCI_LINEENDRECTEXTEND = 2432,
        SCI_PAGEUPRECTEXTEND = 2433,
        SCI_PAGEDOWNRECTEXTEND = 2434,
        SCI_STUTTEREDPAGEUP = 2435,
        SCI_STUTTEREDPAGEUPEXTEND = 2436,
        SCI_STUTTEREDPAGEDOWN = 2437,
        SCI_STUTTEREDPAGEDOWNEXTEND = 2438,
        SCI_WORDLEFTEND = 2439,
        SCI_WORDLEFTENDEXTEND = 2440,
        SCI_WORDRIGHTEND = 2441,
        SCI_WORDRIGHTENDEXTEND = 2442,
        SCI_SETWHITESPACECHARS = 2443,
        SCI_GETWHITESPACECHARS = 2647,
        SCI_SETPUNCTUATIONCHARS = 2648,
        SCI_GETPUNCTUATIONCHARS = 2649,
        SCI_SETCHARSDEFAULT = 2444,
        SCI_AUTOCGETCURRENT = 2445,
        SCI_AUTOCGETCURRENTTEXT = 2610,
        SCI_AUTOCSETCASEINSENSITIVEBEHAVIOUR = 2634,
        SCI_AUTOCGETCASEINSENSITIVEBEHAVIOUR = 2635,
        SCI_AUTOCSETMULTI = 2636,
        SCI_AUTOCGETMULTI = 2637,
        SCI_AUTOCSETORDER = 2660,
        SCI_AUTOCGETORDER = 2661,
        SCI_ALLOCATE = 2446,
        SCI_TARGETASUTF8 = 2447,
        SCI_SETLENGTHFORENCODE = 2448,
        SCI_ENCODEDFROMUTF8 = 2449,
        SCI_FINDCOLUMN = 2456,
        SCI_GETCARETSTICKY = 2457,
        SCI_SETCARETSTICKY = 2458,
        SCI_TOGGLECARETSTICKY = 2459,
        SCI_SETPASTECONVERTENDINGS = 2467,
        SCI_GETPASTECONVERTENDINGS = 2468,
        SCI_SELECTIONDUPLICATE = 2469,
        SCI_SETCARETLINEBACKALPHA = 2470,
        SCI_GETCARETLINEBACKALPHA = 2471,
        SCI_SETCARETSTYLE = 2512,
        SCI_GETCARETSTYLE = 2513,
        SCI_SETINDICATORCURRENT = 2500,
        SCI_GETINDICATORCURRENT = 2501,
        SCI_SETINDICATORVALUE = 2502,
        SCI_GETINDICATORVALUE = 2503,
        SCI_INDICATORFILLRANGE = 2504,
        SCI_INDICATORCLEARRANGE = 2505,
        SCI_INDICATORALLONFOR = 2506,
        SCI_INDICATORVALUEAT = 2507,
        SCI_INDICATORSTART = 2508,
        SCI_INDICATOREND = 2509,
        SCI_SETPOSITIONCACHE = 2514,
        SCI_GETPOSITIONCACHE = 2515,
        SCI_COPYALLOWLINE = 2519,
        SCI_GETCHARACTERPOINTER = 2520,
        SCI_GETRANGEPOINTER = 2643,
        SCI_GETGAPPOSITION = 2644,
        SCI_INDICSETALPHA = 2523,
        SCI_INDICGETALPHA = 2524,
        SCI_INDICSETOUTLINEALPHA = 2558,
        SCI_INDICGETOUTLINEALPHA = 2559,
        SCI_SETEXTRAASCENT = 2525,
        SCI_GETEXTRAASCENT = 2526,
        SCI_SETEXTRADESCENT = 2527,
        SCI_GETEXTRADESCENT = 2528,
        SCI_MARKERSYMBOLDEFINED = 2529,
        SCI_MARGINSETTEXT = 2530,
        SCI_MARGINGETTEXT = 2531,
        SCI_MARGINSETSTYLE = 2532,
        SCI_MARGINGETSTYLE = 2533,
        SCI_MARGINSETSTYLES = 2534,
        SCI_MARGINGETSTYLES = 2535,
        SCI_MARGINTEXTCLEARALL = 2536,
        SCI_MARGINSETSTYLEOFFSET = 2537,
        SCI_MARGINGETSTYLEOFFSET = 2538,
        SCI_SETMARGINOPTIONS = 2539,
        SCI_GETMARGINOPTIONS = 2557,
        SCI_ANNOTATIONSETTEXT = 2540,
        SCI_ANNOTATIONGETTEXT = 2541,
        SCI_ANNOTATIONSETSTYLE = 2542,
        SCI_ANNOTATIONGETSTYLE = 2543,
        SCI_ANNOTATIONSETSTYLES = 2544,
        SCI_ANNOTATIONGETSTYLES = 2545,
        SCI_ANNOTATIONGETLINES = 2546,
        SCI_ANNOTATIONCLEARALL = 2547,
        SCI_ANNOTATIONSETVISIBLE = 2548,
        SCI_ANNOTATIONGETVISIBLE = 2549,
        SCI_ANNOTATIONSETSTYLEOFFSET = 2550,
        SCI_ANNOTATIONGETSTYLEOFFSET = 2551,
        SCI_RELEASEALLEXTENDEDSTYLES = 2552,
        SCI_ALLOCATEEXTENDEDSTYLES = 2553,
        SCI_ADDUNDOACTION = 2560,
        SCI_CHARPOSITIONFROMPOINT = 2561,
        SCI_CHARPOSITIONFROMPOINTCLOSE = 2562,
        SCI_SETMOUSESELECTIONRECTANGULARSWITCH = 2668,
        SCI_GETMOUSESELECTIONRECTANGULARSWITCH = 2669,
        SCI_SETMULTIPLESELECTION = 2563,
        SCI_GETMULTIPLESELECTION = 2564,
        SCI_SETADDITIONALSELECTIONTYPING = 2565,
        SCI_GETADDITIONALSELECTIONTYPING = 2566,
        SCI_SETADDITIONALCARETSBLINK = 2567,
        SCI_GETADDITIONALCARETSBLINK = 2568,
        SCI_SETADDITIONALCARETSVISIBLE = 2608,
        SCI_GETADDITIONALCARETSVISIBLE = 2609,
        SCI_GETSELECTIONS = 2570,
        SCI_GETSELECTIONEMPTY = 2650,
        SCI_CLEARSELECTIONS = 2571,
        SCI_SETSELECTION = 2572,
        SCI_ADDSELECTION = 2573,
        SCI_DROPSELECTIONN = 2671,
        SCI_SETMAINSELECTION = 2574,
        SCI_GETMAINSELECTION = 2575,
        SCI_SETSELECTIONNCARET = 2576,
        SCI_GETSELECTIONNCARET = 2577,
        SCI_SETSELECTIONNANCHOR = 2578,
        SCI_GETSELECTIONNANCHOR = 2579,
        SCI_SETSELECTIONNCARETVIRTUALSPACE = 2580,
        SCI_GETSELECTIONNCARETVIRTUALSPACE = 2581,
        SCI_SETSELECTIONNANCHORVIRTUALSPACE = 2582,
        SCI_GETSELECTIONNANCHORVIRTUALSPACE = 2583,
        SCI_SETSELECTIONNSTART = 2584,
        SCI_GETSELECTIONNSTART = 2585,
        SCI_SETSELECTIONNEND = 2586,
        SCI_GETSELECTIONNEND = 2587,
        SCI_SETRECTANGULARSELECTIONCARET = 2588,
        SCI_GETRECTANGULARSELECTIONCARET = 2589,
        SCI_SETRECTANGULARSELECTIONANCHOR = 2590,
        SCI_GETRECTANGULARSELECTIONANCHOR = 2591,
        SCI_SETRECTANGULARSELECTIONCARETVIRTUALSPACE = 2592,
        SCI_GETRECTANGULARSELECTIONCARETVIRTUALSPACE = 2593,
        SCI_SETRECTANGULARSELECTIONANCHORVIRTUALSPACE = 2594,
        SCI_GETRECTANGULARSELECTIONANCHORVIRTUALSPACE = 2595,
        SCI_SETVIRTUALSPACEOPTIONS = 2596,
        SCI_GETVIRTUALSPACEOPTIONS = 2597,
        SCI_SETRECTANGULARSELECTIONMODIFIER = 2598,
        SCI_GETRECTANGULARSELECTIONMODIFIER = 2599,
        SCI_SETADDITIONALSELFORE = 2600,
        SCI_SETADDITIONALSELBACK = 2601,
        SCI_SETADDITIONALSELALPHA = 2602,
        SCI_GETADDITIONALSELALPHA = 2603,
        SCI_SETADDITIONALCARETFORE = 2604,
        SCI_GETADDITIONALCARETFORE = 2605,
        SCI_ROTATESELECTION = 2606,
        SCI_SWAPMAINANCHORCARET = 2607,
        SCI_MULTIPLESELECTADDNEXT = 2688,
        SCI_MULTIPLESELECTADDEACH = 2689,
        SCI_CHANGELEXERSTATE = 2617,
        SCI_CONTRACTEDFOLDNEXT = 2618,
        SCI_VERTICALCENTRECARET = 2619,
        SCI_MOVESELECTEDLINESUP = 2620,
        SCI_MOVESELECTEDLINESDOWN = 2621,
        SCI_SETIDENTIFIER = 2622,
        SCI_GETIDENTIFIER = 2623,
        SCI_RGBAIMAGESETWIDTH = 2624,
        SCI_RGBAIMAGESETHEIGHT = 2625,
        SCI_RGBAIMAGESETSCALE = 2651,
        SCI_MARKERDEFINERGBAIMAGE = 2626,
        SCI_REGISTERRGBAIMAGE = 2627,
        SCI_SCROLLTOSTART = 2628,
        SCI_SCROLLTOEND = 2629,
        SCI_SETTECHNOLOGY = 2630,
        SCI_GETTECHNOLOGY = 2631,
        SCI_CREATELOADER = 2632,
        SCI_FINDINDICATORSHOW = 2640,
        SCI_FINDINDICATORFLASH = 2641,
        SCI_FINDINDICATORHIDE = 2642,
        SCI_VCHOMEDISPLAY = 2652,
        SCI_VCHOMEDISPLAYEXTEND = 2653,
        SCI_GETCARETLINEVISIBLEALWAYS = 2654,
        SCI_SETCARETLINEVISIBLEALWAYS = 2655,
        SCI_SETLINEENDTYPESALLOWED = 2656,
        SCI_GETLINEENDTYPESALLOWED = 2657,
        SCI_GETLINEENDTYPESACTIVE = 2658,
        SCI_SETREPRESENTATION = 2665,
        SCI_GETREPRESENTATION = 2666,
        SCI_CLEARREPRESENTATION = 2667,
        SCI_SETTARGETRANGE = 2686,
        SCI_GETTARGETTEXT = 2687,
        SCI_STARTRECORD = 3001,
        SCI_STOPRECORD = 3002,
        SCI_SETLEXER = 4001,
        SCI_GETLEXER = 4002,
        SCI_COLOURISE = 4003,
        SCI_SETPROPERTY = 4004,
        SCI_SETKEYWORDS = 4005,
        SCI_SETLEXERLANGUAGE = 4006,
        SCI_LOADLEXERLIBRARY = 4007,
        SCI_GETPROPERTY = 4008,
        SCI_GETPROPERTYEXPANDED = 4009,
        SCI_GETPROPERTYINT = 4010,
        SCI_GETLEXERLANGUAGE = 4012,
        SCI_PRIVATELEXERCALL = 4013,
        SCI_PROPERTYNAMES = 4014,
        SCI_PROPERTYTYPE = 4015,
        SCI_DESCRIBEPROPERTY = 4016,
        SCI_DESCRIBEKEYWORDSETS = 4017,
        SCI_GETLINEENDTYPESSUPPORTED = 4018,
        SCI_ALLOCATESUBSTYLES = 4020,
        SCI_GETSUBSTYLESSTART = 4021,
        SCI_GETSUBSTYLESLENGTH = 4022,
        SCI_GETSTYLEFROMSUBSTYLE = 4027,
        SCI_GETPRIMARYSTYLEFROMSTYLE = 4028,
        SCI_FREESUBSTYLES = 4023,
        SCI_SETIDENTIFIERS = 4024,
        SCI_DISTANCETOSECONDARYSTYLES = 4025,
        SCI_GETSUBSTYLEBASES = 4026,

        // Keys
        SCK_DOWN = 300,
        SCK_UP = 301,
        SCK_LEFT = 302,
        SCK_RIGHT = 303,
        SCK_HOME = 304,
        SCK_END = 305,
        SCK_PRIOR = 306,
        SCK_NEXT = 307,
        SCK_DELETE = 308,
        SCK_INSERT = 309,
        SCK_ESCAPE = 7,
        SCK_BACK = 8,
        SCK_TAB = 9,
        SCK_RETURN = 13,
        SCK_ADD = 310,
        SCK_SUBTRACT = 311,
        SCK_DIVIDE = 312,
        SCK_WIN = 313,
        SCK_RWIN = 314,
        SCK_MENU = 315,

        // Notifications
        SCN_STYLENEEDED = 2000,
        SCN_CHARADDED = 2001,
        SCN_SAVEPOINTREACHED = 2002,
        SCN_SAVEPOINTLEFT = 2003,
        SCN_MODIFYATTEMPTRO = 2004,
        SCN_KEY = 2005,
        SCN_DOUBLECLICK = 2006,
        SCN_UPDATEUI = 2007,
        SCN_MODIFIED = 2008,
        SCN_MACRORECORD = 2009,
        SCN_MARGINCLICK = 2010,
        SCN_NEEDSHOWN = 2011,
        SCN_PAINTED = 2013,
        SCN_USERLISTSELECTION = 2014,
        SCN_URIDROPPED = 2015,
        SCN_DWELLSTART = 2016,
        SCN_DWELLEND = 2017,
        SCN_ZOOM = 2018,
        SCN_HOTSPOTCLICK = 2019,
        SCN_HOTSPOTDOUBLECLICK = 2020,
        SCN_CALLTIPCLICK = 2021,
        SCN_AUTOCSELECTION = 2022,
        SCN_INDICATORCLICK = 2023,
        SCN_INDICATORRELEASE = 2024,
        SCN_AUTOCCANCELLED = 2025,
        SCN_AUTOCCHARDELETED = 2026,
        SCN_HOTSPOTRELEASECLICK = 2027,
        SCN_FOCUSIN = 2028,
        SCN_FOCUSOUT = 2029,
        SCN_AUTOCCOMPLETED = 2030,

        // Line wrapping
        SC_WRAP_NONE = 0,
        SC_WRAP_WORD = 1,
        SC_WRAP_CHAR = 2,
        SC_WRAP_WHITESPACE = 3,

        SC_WRAPVISUALFLAG_NONE = 0x0000,
        SC_WRAPVISUALFLAG_END = 0x0001,
        SC_WRAPVISUALFLAG_START = 0x0002,
        SC_WRAPVISUALFLAG_MARGIN = 0x0004,

        SC_WRAPVISUALFLAGLOC_DEFAULT = 0x0000,
        SC_WRAPVISUALFLAGLOC_END_BY_TEXT = 0x0001,
        SC_WRAPVISUALFLAGLOC_START_BY_TEXT = 0x0002,

        SC_WRAPINDENT_FIXED = 0,
        SC_WRAPINDENT_SAME = 1,
        SC_WRAPINDENT_INDENT = 2,

        // Virtual space
        SCVS_NONE = 0,
        SCVS_RECTANGULARSELECTION = 1,
        SCVS_USERACCESSIBLE = 2,

        // Styles constants
        STYLE_DEFAULT = 32,
        STYLE_LINENUMBER = 33,
        STYLE_BRACELIGHT = 34,
        STYLE_BRACEBAD = 35,
        STYLE_CONTROLCHAR = 36,
        STYLE_INDENTGUIDE = 37,
        STYLE_CALLTIP = 38,
        STYLE_LASTPREDEFINED = 39,
        STYLE_MAX = 255,

        SC_FONT_SIZE_MULTIPLIER = 100,
        SC_CASE_MIXED = 0,
        SC_CASE_UPPER = 1,
        SC_CASE_LOWER = 2,
        SC_CASE_CAMEL = 3,

        // Technology
        SC_TECHNOLOGY_DEFAULT = 0,
        SC_TECHNOLOGY_DIRECTWRITE = 1,
        SC_TECHNOLOGY_DIRECTWRITERETAIN = 2,
        SC_TECHNOLOGY_DIRECTWRITEDC = 3,

        // Undo
        UNDO_MAY_COALESCE = 1,

        // Whitespace
        SCWS_INVISIBLE = 0,
        SCWS_VISIBLEALWAYS = 1,
        SCWS_VISIBLEAFTERINDENT = 2,

        // Window messages
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_SETCURSOR = 0x0020,
        WM_NOTIFY = 0x004E,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_RBUTTONDBLCLK = 0x0206,
        WM_MBUTTONDBLCLK = 0x0209,
        WM_XBUTTONDBLCLK = 0x020D,
        WM_USER = 0x0400,
        WM_REFLECT = WM_USER + 0x1C00,

        // Window styles
        WS_BORDER = 0x00800000,
        WS_EX_CLIENTEDGE = 0x00000200,

        // Lexers
        SCLEX_CONTAINER = 0,
        SCLEX_NULL = 1,
        SCLEX_PYTHON = 2,
        SCLEX_CPP = 3,
        SCLEX_HTML = 4,
        SCLEX_XML = 5,
        SCLEX_PERL = 6,
        SCLEX_SQL = 7,
        SCLEX_VB = 8,
        SCLEX_PROPERTIES = 9,
        SCLEX_ERRORLIST = 10,
        SCLEX_MAKEFILE = 11,
        SCLEX_BATCH = 12,
        SCLEX_XCODE = 13,
        SCLEX_LATEX = 14,
        SCLEX_LUA = 15,
        SCLEX_DIFF = 16,
        SCLEX_CONF = 17,
        SCLEX_PASCAL = 18,
        SCLEX_AVE = 19,
        SCLEX_ADA = 20,
        SCLEX_LISP = 21,
        SCLEX_RUBY = 22,
        SCLEX_EIFFEL = 23,
        SCLEX_EIFFELKW = 24,
        SCLEX_TCL = 25,
        SCLEX_NNCRONTAB = 26,
        SCLEX_BULLANT = 27,
        SCLEX_VBSCRIPT = 28,
        SCLEX_BAAN = 31,
        SCLEX_MATLAB = 32,
        SCLEX_SCRIPTOL = 33,
        SCLEX_ASM = 34,
        SCLEX_CPPNOCASE = 35,
        SCLEX_FORTRAN = 36,
        SCLEX_F77 = 37,
        SCLEX_CSS = 38,
        SCLEX_POV = 39,
        SCLEX_LOUT = 40,
        SCLEX_ESCRIPT = 41,
        SCLEX_PS = 42,
        SCLEX_NSIS = 43,
        SCLEX_MMIXAL = 44,
        SCLEX_CLW = 45,
        SCLEX_CLWNOCASE = 46,
        SCLEX_LOT = 47,
        SCLEX_YAML = 48,
        SCLEX_TEX = 49,
        SCLEX_METAPOST = 50,
        SCLEX_POWERBASIC = 51,
        SCLEX_FORTH = 52,
        SCLEX_ERLANG = 53,
        SCLEX_OCTAVE = 54,
        SCLEX_MSSQL = 55,
        SCLEX_VERILOG = 56,
        SCLEX_KIX = 57,
        SCLEX_GUI4CLI = 58,
        SCLEX_SPECMAN = 59,
        SCLEX_AU3 = 60,
        SCLEX_APDL = 61,
        SCLEX_BASH = 62,
        SCLEX_ASN1 = 63,
        SCLEX_VHDL = 64,
        SCLEX_CAML = 65,
        SCLEX_BLITZBASIC = 66,
        SCLEX_PUREBASIC = 67,
        SCLEX_HASKELL = 68,
        SCLEX_PHPSCRIPT = 69,
        SCLEX_TADS3 = 70,
        SCLEX_REBOL = 71,
        SCLEX_SMALLTALK = 72,
        SCLEX_FLAGSHIP = 73,
        SCLEX_CSOUND = 74,
        SCLEX_FREEBASIC = 75,
        SCLEX_INNOSETUP = 76,
        SCLEX_OPAL = 77,
        SCLEX_SPICE = 78,
        SCLEX_D = 79,
        SCLEX_CMAKE = 80,
        SCLEX_GAP = 81,
        SCLEX_PLM = 82,
        SCLEX_PROGRESS = 83,
        SCLEX_ABAQUS = 84,
        SCLEX_ASYMPTOTE = 85,
        SCLEX_R = 86,
        SCLEX_MAGIK = 87,
        SCLEX_POWERSHELL = 88,
        SCLEX_MYSQL = 89,
        SCLEX_PO = 90,
        SCLEX_TAL = 91,
        SCLEX_COBOL = 92,
        SCLEX_TACL = 93,
        SCLEX_SORCUS = 94,
        SCLEX_POWERPRO = 95,
        SCLEX_NIMROD = 96,
        SCLEX_SML = 97,
        SCLEX_MARKDOWN = 98,
        SCLEX_TXT2TAGS = 99,
        SCLEX_A68K = 100,
        SCLEX_MODULA = 101,
        SCLEX_COFFEESCRIPT = 102,
        SCLEX_TCMD = 103,
        SCLEX_AVS = 104,
        SCLEX_ECL = 105,
        SCLEX_OSCRIPT = 106,
        SCLEX_VISUALPROLOG = 107,
        SCLEX_LITERATEHASKELL = 108,
        SCLEX_STTXT = 109,
        SCLEX_KVIRC = 110,
        SCLEX_RUST = 111,
        SCLEX_DMAP = 112,
        SCLEX_AS = 113,
        SCLEX_DMIS = 114,
        SCLEX_REGISTRY = 115,
        SCLEX_BIBTEX = 116,
        SCLEX_SREC = 117,
        SCLEX_IHEX = 118,
        SCLEX_TEHEX = 119,
        SCLEX_AUTOMATIC = 1000,

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
}