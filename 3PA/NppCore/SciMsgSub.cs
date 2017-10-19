using System;

namespace _3PA.NppCore {

    #region Sub enums using SciMsg

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
        /// Npp.Indicator.Alpha and <see cref="Sci.Indicator.OutlineAlpha"/>.
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
        /// Npp.Indicator.Alpha and <see cref="Sci.Indicator.OutlineAlpha"/>.
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
        None = 0,

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


    /// <summary>
    /// Configuration options for automatic code folding.
    /// </summary>
    /// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
    [Flags]
    public enum AutomaticFold : uint {
        /// <summary>
        /// Automatic folding is disabled. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Automatically show lines as needed. The NeedShown event is not raised when this value is used.
        /// </summary>
        Show = SciMsg.SC_AUTOMATICFOLD_SHOW,

        /// <summary>
        /// Handle clicks in fold margin automatically. The MarginClick event is not raised for folding margins when this value is used.
        /// </summary>
        Click = SciMsg.SC_AUTOMATICFOLD_CLICK,

        /// <summary>
        /// Show lines as needed when the fold structure is changed.
        /// </summary>
        Change = SciMsg.SC_AUTOMATICFOLD_CHANGE
    }

    #endregion

}
