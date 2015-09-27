using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using _3PA.Interop;
using _3PA.Lib;

namespace _3PA {
    /// <summary>
    ///     This class contains very generic wrappers for basic Notepad++ functionality.
    /// </summary>
    public partial class Npp {
        public const int KeywordMaxLength = 30;
        private const int IndicatorMatch = 31;
        private const int BookmarkMarker = 24;
        private const int SwShownoactivate = 4;
        private const uint SwpNoactivate = 0x0010;
        public const int SbSettext = 1035;
        public const int SbSetparts = 1028;
        public const int SbGetparts = 1030;
        private const uint WmUser = 0x0400;
        private const uint SbGettextlength = WmUser + 12;
        private const uint SbGettext = WmUser + 13;
        // "\\\t\n\r .,:;'\"[]{}()-/!?@$%^&*«»><#|~`"
        // "\t .,:;'\"[]{}()@+<>=/*%^?"
        public static char[] Delimiters = "\\\t\n\r .,:;'\"[]{}()/!?@$%^&*«»><#|~`".ToCharArray();
        private static readonly char[] StatementDelimiters = " ,:;'\"[]{}()".ToCharArray();
        private static IntPtr _curScintilla;

        public enum UdlStyles {
            Default = 0,
            Comment = 1,
            CommentLine = 2,
            Number = 3,
            KeyWordsList1 = 4,
            KeyWordsList2 = 5,
            KeyWordsList3 = 6,
            KeyWordsList4 = 7,
            KeyWordsList5 = 8,
            KeyWordsList6 = 9,
            KeyWordsList7 = 10,
            KeyWordsList8 = 11,
            FolderInCode1 = 13,
            FolderInCode2 = 14,
            FolderInComment = 15,
            Delimiter1 = 16,
            Delimiter2 = 17,
            Delimiter3 = 18,
            Delimiter4 = 19,
            Delimiter5 = 20,
            Delimiter6 = 21,
            Delimiter7 = 22,
            Delimiter8 = 23,
            Operators = 24,
        }

        /// <summary>
        ///     Gets the window handle to current Scintilla.
        /// </summary>
        /// <value>
        ///     The current window handle to scintilla.
        /// </value>
        public static IntPtr HandleScintilla {
            get {
                if (_curScintilla == IntPtr.Zero) {
                    UpdateScintilla();
                }
                return _curScintilla;
            }
        }

        /// <summary>
        /// Updates the current scintilla handle for Npp's functions
        /// </summary>
        public static void UpdateScintilla() {
            int curScintilla;
            Win32.SendMessage(HandleNpp, NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            _curScintilla = (curScintilla == 0)
                ? Plug.NppData._scintillaMainHandle
                : Plug.NppData._scintillaSecondHandle;
        }

        #region Lexer stuff

        /// <summary>
        /// returns a boolean to know if we are currently using the container lexer
        /// </summary>
        /// <returns></returns>
        public static bool IsUsingContainerLexer() {
            int x = Call(SciMsg.SCI_GETLEXER);
            return (x == 0);
        }

        /// <summary>
        /// Sets the current lexer to container lexer
        /// </summary>
        public static void SetLexerToContainerLexer() {
            Call(SciMsg.SCI_SETLEXER, 0, 2);
            //Call(SciMsg.SCI_COLOURISE, 0, -1);
        }

        /// <summary>
        /// returns the position at which we need to start the styling
        /// </summary>
        /// <returns></returns>
        public static int GetLastStyledPosition() {
            return Call(SciMsg.SCI_GETENDSTYLED);
        }

        /// <summary>
        /// Position of the starting line we need to style
        /// </summary>
        /// <returns></returns>
        public static int GetSylingNeededStartPos() {
            return PositionFromLine(LineFromPosition(GetLastStyledPosition()));
        }

        /// <summary>
        /// Defines a style
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bg"></param>
        /// <param name="fg"></param>
        public static void SetStyle(int id, Color bg, Color fg) {
            Call(SciMsg.SCI_STYLESETBACK, id, (int)(new COLORREF(bg)).ColorDWORD);
            Call(SciMsg.SCI_STYLESETFORE, id, (int)(new COLORREF(fg)).ColorDWORD);
        }

        /// <summary>
        /// Sets the SciMsg.STYLE_DEFAULT style and then
        /// sets all styles to have the same attributes as STYLE_DEFAULT
        /// </summary>
        /// <param name="bg"></param>
        /// <param name="fg"></param>
        public static void SetDefaultStyle(Color bg, Color fg) {
            SetStyle((int)SciMsg.STYLE_DEFAULT, bg, fg);
            Call(SciMsg.SCI_STYLECLEARALL);
        }

        /// <summary>
        /// Style the text between startPos and endPos with the styleId
        /// </summary>
        /// <param name="styleId"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        public static void StyleText(int styleId, int startPos, int endPos) {
            Call(SciMsg.SCI_STARTSTYLING, startPos, 0);
            Call(SciMsg.SCI_SETSTYLING, endPos - startPos, styleId);
        }

        /// <summary>
        /// returns the style of the caret position
        /// </summary>
        /// <returns></returns>
        public static int GetStyleAt(int caretPos) {
            return Call(SciMsg.SCI_GETSTYLEAT, caretPos, 0);
        }
        #endregion

        /// <summary>
        /// Is the caret not in : an include, a string, a comment
        /// </summary>
        /// <param name="caretPos"></param>
        /// <returns></returns>
        public static bool IsNormalContext(int caretPos) {
            UdlStyles curCntext = (UdlStyles) GetStyleAt(caretPos);
            return (curCntext != UdlStyles.Comment
                    && curCntext != UdlStyles.Delimiter1
                    && curCntext != UdlStyles.Delimiter2
                    && curCntext != UdlStyles.Delimiter3
                    && curCntext != UdlStyles.CommentLine
                    && curCntext != UdlStyles.Delimiter8);
        }

        /// <summary>
        /// replace the line specified by the text specified
        /// </summary>
        /// <param name="line"></param>
        /// <param name="text"></param>
        public static void ReplaceLine(int line, string text) {
            Call(SciMsg.SCI_GOTOLINE, line);
            SetSelection(GetCaretPosition(), Call(SciMsg.SCI_GETLINEENDPOSITION, line));
            SetSelectedText(text);
        }

        /// <summary>
        /// set the indentation of the current line relatively to the previous indentation
        /// </summary>
        /// <param name="indent"></param>
        public static void SetCurrentLineRelativeIndent(int indent) {
            int curPos = GetCaretPosition();
            int line = GetLineNumber(curPos);
            Call(SciMsg.SCI_SETLINEINDENTATION, line, GetPreviousLineIndent(curPos) + indent);
            SetCaretPosition(Call(SciMsg.SCI_GETLINEENDPOSITION, line));
        }

        /// <summary>
        /// set the indentation of the previous (-1) line relatively to its current indentation
        /// </summary>
        /// <param name="indent"></param>
        public static void SetPreviousLineRelativeIndent(int indent) {
            int line = GetCaretLineNumber();
            Call(SciMsg.SCI_SETLINEINDENTATION, line - 1, GetLineIndent(line - 1) + indent);
        }

        /// <summary>
        /// get current line indent
        /// </summary>
        /// <returns></returns>
        public static int GetLineIndent() {
            return Call(SciMsg.SCI_GETLINEINDENTATION, GetLineNumber(GetCaretPosition()), 0);
        }

        /// <summary>
        /// get the indentation of the specified line (in number of spaces)
        /// </summary>
        /// <returns></returns>
        public static int GetLineIndent(int line) {
            return Call(SciMsg.SCI_GETLINEINDENTATION, line, 0);
        }

        /// <summary>
        /// get the indentation of the first non null previous line from the specified position
        /// </summary>
        /// <param name="curPos"></param>
        /// <returns></returns>
        public static int GetPreviousLineIndent(int curPos) {
            int lineToIndent = GetLineNumber(curPos);
            string curLineText;
            int nbindent;
            int line = lineToIndent;
            do {
                line = line - 1;
                curLineText = GetLine(line);
                nbindent = GetLineIndent(line);
                //Call(SciMsg.SCI_SETLINEINDENTATION, lineToIndent, nbindent + 4);
            } while (line >= 0 && String.IsNullOrWhiteSpace(curLineText.Trim()) && (lineToIndent - line) < 50);
            return nbindent;
        }


        /// <summary>
        /// Same as ReplaceKeyword except it is surrounded by BeginUndo / EndUndo
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="keywordPos"></param>
        /// <param name="originalPos"></param>
        public static void WrappedKeywordReplace(string keyword, Point keywordPos, int originalPos) {
            BeginUndoAction();
            ReplaceKeyword(keyword, keywordPos);
            SetCaretPosition(originalPos);
            EndUndoAction();
        }

        /// <summary>
        /// returns the x,y point location of the character at the position given
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point GetPointXyFromPosition(int position) {
            int x = (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_POINTXFROMPOSITION, 0, position);
            int y = (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_POINTYFROMPOSITION, 0, position);
            return new Point(x, y);
        }

        public static void ReplaceKeyword(string keyword) {
            Point keywordPos;
            GetKeywordOnLeftOfPosition(GetCaretPosition(), out keywordPos);
            ReplaceKeyword(keyword, keywordPos);
        }

        public static void ReplaceKeyword(string keyword, Point keywordPos) {
            var curPos = GetCaretPosition();
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTION, keywordPos.X, keywordPos.Y);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_REPLACESEL, 0, keyword);
            curPos = keywordPos.X + keyword.Length + (curPos - keywordPos.Y);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSEL, curPos, curPos);
        }

        public static string GetKeyword() {
            Point pt;
            return GetKeywordOnLeftOfPosition(GetCaretPosition(), out pt);
        }

        public static string GetKeyword(int curPos) {
            Point pt;
            return GetKeywordOnLeftOfPosition(curPos, out pt);
        }

        public static string GetKeywordOnLeftOfPosition(int curPos, out Point point) {
            var word = "";
            var locPoint = new Point();

            var startPos = curPos - KeywordMaxLength;
            startPos = (startPos > 0) ? startPos : 0;

            // get the text on the left of the carret
            if (curPos - startPos > 0) {
                string leftText = GetTextByRange(startPos, curPos);
                int nbPts;
                word = Abl.ReadAblWord(leftText, true, out nbPts);
                locPoint.X = curPos - word.Length;
                locPoint.Y = curPos;
            }

            point = locPoint;
            return word;
        }

        public static string GetCurrentTable() {
            return GetCurrentTable(GetCaretPosition());
        }

        public static string GetCurrentTable(int curpos) {
            var textOnLeft = TextBeforePosition(curpos, KeywordMaxLength*2);
            return Regex.Match(textOnLeft, @".*[^\w-&]([\w-]{2,})\.[\w-]*$", RegexOptions.IgnoreCase).Groups[1].Value;
        }

        public static bool WeAreEnteringAField() {
            return WeAreEnteringAField(GetCaretPosition());
        }

        public static bool WeAreEnteringAField(int curpos) {
            var textOnLeft = TextBeforePosition(curpos, KeywordMaxLength*2);
            return Regex.IsMatch(textOnLeft, @".*[\w-]{2,}\.[\w-]*$", RegexOptions.IgnoreCase);
        }

        public static string GetIndentString() {
            if (GetUseTabs()) {
                return "\t";
            }
            var widthInChars = GetTabWidth();
            return new string(' ', widthInChars);
        }

        /// <summary>
        ///  barbarian method to force the default autocompletion window to hide
        /// </summary>
        public static void HideDefaultAutoCompletion() {
            //TODO: find a better technique to hide the autocompletion!!! this slows npp down
            //Call(SciMsg.SCI_AUTOCSETIGNORECASE, 1);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_AUTOCSTOPS, 0, @"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~");
        }

        /// <summary>
        ///  reset the autocompletion to default behavior (used with HideDefaultAutoCompletion)
        /// </summary>
        public static void ResetDefaultAutoCompletion() {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_AUTOCSTOPS, 0, "");
        }

        /// <summary>
        ///     Determines whether indentation should be created out of a mixture of tabs and spaces or be based purely on spaces.
        ///     Set useTabs to false (0) to create all tabs and indents out of spaces. The default is true.
        ///     You can use SCI_GETCOLUMN to get the column of a position taking the width of a tab into account.
        /// </summary>
        public static bool GetUseTabs() {
            var retval = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETUSETABS, 0, 0);
            return (retval == 1);
        }

        /// <summary>
        /// returns the text lenght in bytes
        /// </summary>
        /// <returns></returns>
        public static int GetTextLenght() {
            return Call(SciMsg.SCI_GETTEXTLENGTH);
        }

        /// <summary>
        ///     Gets all lines of the current document.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllLines() {
            return GetDocumentText().Split(new[] {Environment.NewLine}, StringSplitOptions.None);
        }

        ///// <summary>
        /////     Gets all text of the current document.
        ///// </summary>
        ///// <returns></returns>
        //public static string GetAllText() {
        //    var fullLength = GetDocumentLength();
        //    using (var tr = new Sci_TextRange(0, fullLength, fullLength + 1)) {
        //        Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
        //        return tr.lpstrText;
        //    }
        //}

        /// <summary>
        ///     Gets the current screen location of the caret.
        /// </summary>
        /// <returns><c>Point</c> representing the coordinates of the screen location.</returns>
        public static Point GetCaretScreenLocation() {
            var pos = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            var x = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_POINTXFROMPOSITION, 0, pos);
            var y = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_POINTYFROMPOSITION, 0, pos);

            var point = new Point(x, y);
            Win32.ClientToScreen(HandleScintilla, ref point);
            return point;
        }

        /// <summary>
        ///     Get text before caret.
        /// </summary>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        public static string TextBeforeCaret(int maxLength = 512) {
            return TextBeforePosition(GetCaretPosition(), maxLength);
        }

        /// <summary>
        ///     Returns text after the current caret position.
        /// </summary>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        public static string TextAfterCaret(int maxLength = 512) {
            var bufCapacity = maxLength + 1;
            var currentPos = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            var fullLength = GetDocumentLength();
            var startPos = currentPos;
            var endPos = Math.Min(currentPos + bufCapacity, fullLength);
            var size = endPos - startPos;

            if (size > 0) {
                using (var tr = new Sci_TextRange(startPos, endPos, bufCapacity)) {
                    Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);
                    return tr.lpstrText;
                }
            }
            return null;
        }

        public static void SetIndicatorStyle(int indicator, SciMsg style, Color color) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICSETSTYLE, indicator, (int) style);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICSETFORE, indicator, ColorTranslator.ToWin32(color));
        }

        public static void ClearIndicator(int indicator, int startPos, int endPos) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETINDICATORCURRENT, indicator, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATORCLEARRANGE, startPos, endPos - startPos);
        }

        public static void PlaceIndicator(int indicator, int startPos, int endPos) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETINDICATORCURRENT, indicator, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATORFILLRANGE, startPos, endPos - startPos);
        }


        public static Point[] FindIndicatorRanges(int indicator) {
            var ranges = new List<Point>();

            var testPosition = 0;

            while (true) {
                //finding the indicator ranges
                //For example indicator 4..6 in the doc 0..10 will have three logical regions:
                //0..4, 4..6, 6..10
                //Probing will produce following when outcome:
                //probe for 0 : 0..4
                //probe for 4 : 4..6
                //probe for 6 : 4..10

                var rangeStart =
                    (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATORSTART, indicator, testPosition);
                var rangeEnd =
                    (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATOREND, indicator, testPosition);
                var value =
                    (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATORVALUEAT, indicator, testPosition);
                if (value == 1) //indicator is present
                    ranges.Add(new Point(rangeStart, rangeEnd));

                if (testPosition == rangeEnd)
                    break;

                testPosition = rangeEnd;
            }

            return ranges.ToArray();
        }

        /// <summary>
        /// get the content of the line specified
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetLine(int line) {
            var length = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_LINELENGTH, line, 0);
            var buffer = new StringBuilder(length + 1);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETLINE, line, buffer);
            buffer.Length = length; //NPP may inject some rubbish at the end of the line
            return buffer.ToString();
        }

        /// <summary>
        /// return the line number according to the position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static int GetLineNumber(int position) {
            return (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_LINEFROMPOSITION, position, 0);
        }

        public static int GetLineStart(int line) {
            return (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_POSITIONFROMLINE, line, 0);
        }

        public static int GetFirstVisibleLine() {
            return (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETFIRSTVISIBLELINE, 0, 0);
        }

        public static void SetFirstVisibleLine(int line) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETFIRSTVISIBLELINE, line, 0);
        }

        public static int GetPositionFromMouseLocation() {
            var point = Cursor.Position;
            Win32.ScreenToClient(HandleScintilla, ref point);

            var pos = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_CHARPOSITIONFROMPOINTCLOSE, point.X, point.Y);

            return pos;
        }


        public static string GetTextBetween(Point point) {
            return GetTextByRange(point.X, point.Y);
        }

        public static void AddTextAtCaret(string text) {
            var curPos = GetCaretPosition();
            SetTextByRange(curPos, curPos, text);
            SetCaretPosition(curPos + text.Length);
        }

        public static string TextAfterPosition(int position, int maxLength) {
            return GetTextByRange(position, position + maxLength - 1);
        }

        public static string GetWordAtCursor() {
            return GetWordAtPosition(GetCaretPosition());
        }

        public static string GetWordAtPosition(int position) {
            // TODO:
            return "";
        }

        public static int GetCaretPosition() {
            return (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
        }

        public static int GetCaretLineNumber() {
            var currentPos = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);

            return (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_LINEFROMPOSITION, currentPos, 0);
        }

        public static void SetCaretPosition(int pos) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSEL, pos, pos);
        }

        public static void ClearSelection() {
            var currentPos = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTIONEND, currentPos, 0);
        }

        public static void SetSelection(int start, int end) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTIONSTART, start, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTIONEND, end, 0);
        }

        public static string TextBeforePosition(int position, int maxLength) {
            return GetTextByRange(position - maxLength + 1, position);
        }

        public static void ScrollToCaret() {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SCROLLCARET, 0, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_LINESCROLL, 0, 1); //bottom scrollbar can hide the line
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SCROLLCARET, 0, 0);
        }

        /// <summary>
        /// allows scintilla to grab focus
        /// </summary>
        /// <returns></returns>
        public static int GrabFocus() {
            var currentPos = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
            return currentPos;
        }

        /// <summary>
        /// to be tested!!!!
        /// </summary>
        /// <returns></returns>
        public static bool IsNppFocused() {
            return Call(SciMsg.SCI_GETFOCUS, 0, 0) == 1;
        }

        /// <summary>
        /// Move the caret and the view to the specified line (lines starts at 1 not 0)
        /// </summary>
        /// <param name="line"></param>
        public static void GoToLine(int line) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_ENSUREVISIBLE, line - 1, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GOTOLINE, line + (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_LINESONSCREEN, 0, 0), 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GOTOLINE, line - 1, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
        }

        /// <summary>
        /// displays the default npp call tip on the current carret position
        /// </summary>
        /// <param name="text"></param>
        public static void ShowCallTip(string text) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_CALLTIPSETPOSITION, 1, 0); // show tooltip above, 
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_CALLTIPSHOW, GetCaretPosition(), text);
        }

        /// <summary>
        /// forces to hide the call tip being displayed
        /// </summary>
        public static void HideCallTip() {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_CALLTIPCANCEL, 0, 0); 
        }

        /// <summary>
        ///     Retrieve the height of a particular line of text in pixels.
        /// </summary>
        public static int GetTextHeight(int line) {
            return (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_TEXTHEIGHT, line, 0);
        }

        /// <summary>
        /// returns a rectangle representing the location and size of the npp window
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetWindowRect() {
            var r = new Rectangle();
            Win32.GetWindowRect(HandleScintilla, ref r);
            return r;
        }


        /// <summary>
        ///     Returns the text between the positions start and end. If end is -1, text is returned to the end of the document.
        ///     The text is 0 terminated, so you must supply a buffer that is at least 1 character longer than the number of
        ///     characters
        ///     you wish to read. The return value is the length of the returned text not including the terminating 0.
        /// </summary>
        public static string GetTextByRange(int start, int end) {
            return GetTextByRange(start, end, end - start);
        }

        /// <summary>
        ///     Returns the text between the positions start and end. If end is -1, text is returned to the end of the document.
        ///     The text is 0 terminated, so you must supply a buffer that is at least 1 character longer than the number of
        ///     characters
        ///     you wish to read. The return value is the length of the returned text not including the terminating 0.
        /// </summary>
        public static string GetTextByRange(int start, int end, int bufCapacity) {
            using (var textRange = new Sci_TextRange(start, end, bufCapacity)) {
                Call(SciMsg.SCI_GETTEXTRANGE, 0, textRange.NativePointer);
                //return textRange.lpstrText;
                return IsUtf8() ? textRange.lpstrText.AnsiToUtf8() : textRange.lpstrText;
            }
        }

        /// <summary>
        ///     Replaces the current selected target range of text.
        /// </summary>
        /// <param name="text">The replacement text.</param>
        /// <param name="useRegularExpression">If true, uses a regular expressions replacement.</param>
        /// <returns> The length of the replacement string.</returns>
        public static int ReplaceText(string text, bool useRegularExpression) {
            if (IsUtf8())
                text = text.Utf8ToAnsi();
            return Call(useRegularExpression ? SciMsg.SCI_REPLACETARGETRE : SciMsg.SCI_REPLACETARGET, text.Length, text);
        }

        /// <summary>
        ///     Replaces a range of text with new text.
        ///     Note that the recommended way to delete text in the document is to set the target to the text to be removed, and to
        ///     perform a replace target with an empty string.
        /// </summary>
        /// <returns> The length of the replacement string.</returns>
        public static int SetTextByRange(int start, int end, string text) {
            SetTargetRange(start, end);
            if (IsUtf8())
                text = text.Utf8ToAnsi();
            // If length is -1, text is a zero terminated string, otherwise length sets the number of character to replace 
            // the target with. After replacement, the target range refers to the replacement text. The return value is the 
            // length of the replacement string.
            return Call(SciMsg.SCI_REPLACETARGET, text.Length, text);
        }

        /// <summary>
        ///     Returns true if the current document is displaying in unicode format or false for ANSI.
        ///     Note that all strings marshaled to and from Scintilla come in ANSI format so need to
        ///     be converted if using Unicode.
        /// </summary>
        public static bool IsUtf8() {
            var result = Call(SciMsg.SCI_GETCODEPAGE);
            return result == (int) SciMsg.SC_CP_UTF8;
        }

        /// <summary>
        ///     Size in bytes of the selection.
        /// </summary>
        public static int GetSelectionLength() {
            return Call(SciMsg.SCI_GETSELTEXT);
        }

        /// <summary>
        ///     Returns the text currently selected (highlighted).
        /// </summary>
        /// <returns>Currently selected text.</returns>
        public static string GetSelectedText() {
            var selLength = GetSelectionLength();
            // Todo: Use a string / char array as stringbuilder can't handle null characters?
            var selectedText = new StringBuilder(selLength);
            if (selLength > 0)
                Call(SciMsg.SCI_GETSELTEXT, 0, selectedText);
            var ret = selectedText.ToString();
            return IsUtf8() ? ret.AnsiToUtf8() : ret;
        }

        /// <summary>
        ///     Gets the selected text or if nothing is selected, gets whole document text.
        /// </summary>
        /// <returns>Selected or whole document text.</returns>
        public static string GetSelectedOrAllText() {
            var selectedText = GetSelectedText();
            return string.IsNullOrEmpty(selectedText) ? GetDocumentText() : selectedText;
        }

        /// <summary>
        ///     The currently selected text is replaced with text. If no text is selected the
        ///     text is inserted at current cursor postion.
        /// </summary>
        /// <param name="text">The document text to set.</param>
        public static void SetSelectedText(string text) {
            if (IsUtf8())
                text = text.Utf8ToAnsi();
            Call(SciMsg.SCI_REPLACESEL, 0, text);
        }

        /// <summary>
        ///     Sets the text for the entire document (replacing any existing text).
        /// </summary>
        /// <param name="text">The document text to set.</param>
        public static void SetDocumentText(string text) {
            if (IsUtf8())
                text = text.Utf8ToAnsi();
            Call(SciMsg.SCI_SETTEXT, 0, text);
        }

        /// <summary>
        ///     Gets the entire document text.
        /// </summary>
        public static string GetDocumentText() {
            var length = GetDocumentLength();
            var text = new StringBuilder(length + 1);
            if (length > 0)
                Call(SciMsg.SCI_GETTEXT, length + 1, text);
            var ret = text.ToString();
            return IsUtf8() ? ret.AnsiToUtf8() : ret;
        }

        /// <summary>
        ///     Add a bookmark at a specific line.
        /// </summary>
        /// <param name="lineNumber">The line number to add a bookmark to.</param>
        public static void AddBookmark(int lineNumber) {
            if (lineNumber == -1)
                lineNumber = GetCurrentLineNumber();
            if (!IsBookmarkPresent(lineNumber))
                Call(SciMsg.SCI_MARKERADD, lineNumber, BookmarkMarker);
        }

        /// <summary>
        ///     Remove all bookmarks from the document.
        /// </summary>
        public static void RemoveAllBookmarks() {
            Call(SciMsg.SCI_MARKERDELETEALL, BookmarkMarker);
        }

        /// <summary>
        ///     Is there a bookmark set on a line.
        /// </summary>
        /// <param name="lineNumber">The line number to check.</param>
        /// <returns>True if a bookmark is set.</returns>
        public static bool IsBookmarkPresent(int lineNumber) {
            if (lineNumber == -1)
                lineNumber = GetCurrentLineNumber();
            var state = Call(SciMsg.SCI_MARKERGET, lineNumber);
            return (state & (1 << BookmarkMarker)) != 0;
        }

        /// <summary>
        ///     Get the line number that the cursor is on.
        /// </summary>
        public static int GetCurrentLineNumber() {
            var currentPos = Call(SciMsg.SCI_GETCURRENTPOS);
            return Call(SciMsg.SCI_LINEFROMPOSITION, currentPos, 0);
        }

        /// <summary>
        ///     Remove all 'find' marks.
        /// </summary>
        public static void RemoveFindMarks() {
            Call(SciMsg.SCI_SETINDICATORCURRENT, IndicatorMatch);
            Call(SciMsg.SCI_INDICATORCLEARRANGE, 0, GetDocumentLength());
        }

        /// <summary>
        ///     Marks a range of text.
        /// </summary>
        public static void AddFindMark(int pos, int length) {
            Call(SciMsg.SCI_INDICATORFILLRANGE, pos, length);
        }

        /// <summary>
        ///     Returns the start and end of the selection without regard to which end is the current position and which is the
        ///     anchor.
        ///     SCI_GETSELECTIONSTART returns the smaller of the current position or the anchor position.
        /// </summary>
        /// <returns>A character range.</returns>
        public static Sci_CharacterRange GetSelectionRange() {
            return new Sci_CharacterRange(
                Call(SciMsg.SCI_GETSELECTIONSTART),
                Call(SciMsg.SCI_GETSELECTIONEND));
        }

        /// <summary>
        ///     Returns the current target start and end positions from a previous operation.
        /// </summary>
        public static Sci_CharacterRange GetTargetRange() {
            return new Sci_CharacterRange(
                Call(SciMsg.SCI_GETTARGETSTART),
                Call(SciMsg.SCI_GETTARGETEND));
        }

        /// <summary>
        ///     Sets the start and end positions for an upcoming operation.
        /// </summary>
        public static void SetTargetRange(int start, int end) {
            Call(SciMsg.SCI_SETTARGETSTART, start);
            Call(SciMsg.SCI_SETTARGETEND, end);
        }

        /// <summary>
        ///     Returns the length of the document in bytes.
        /// </summary>
        public static int GetDocumentLength() {
            return Call(SciMsg.SCI_GETLENGTH);
        }

        /// <summary>
        ///     Sets both the anchor and the current position. If end is negative, it means the end of the document.
        ///     If start is negative, it means remove any selection (i.e. set the start to the same position as end).
        ///     The caret is scrolled into view after this operation.
        /// </summary>
        /// <param name="start">The selection start (anchor) position.</param>
        /// <param name="end">The selection end (current) position.</param>
        public static void SetSelection2(int start, int end) {
            Call(SciMsg.SCI_SETSEL, start, end);
        }

        /// <summary>
        ///     Make a range visible by scrolling to the last line of the range.
        ///     A line may be hidden because more than one of its parent lines is contracted. Both these message travels up the
        ///     fold hierarchy, expanding any contracted folds until they reach the top level. The line will then be visible.
        /// </summary>
        public static void EnsureRangeVisible(int start, int end) {
            var lineStart = Call(SciMsg.SCI_LINEFROMPOSITION, Math.Min(start, end));
            var lineEnd = Call(SciMsg.SCI_LINEFROMPOSITION, Math.Max(start, end));
            for (var line = lineStart; line <= lineEnd; line++) {
                Call(SciMsg.SCI_ENSUREVISIBLE, line);
            }
        }

        /// <summary>
        ///     This searches for the first occurrence of a text string in the target defined by startPosition and endPosition.
        ///     The text string is not zero terminated; the size is set by length.
        ///     The search is modified by the search flags set by SCI_SETSEARCHFLAGS.
        ///     If the search succeeds, the target is set to the found text and the return value is the position of the start
        ///     of the matching text. If the search fails, the result is -1.
        /// </summary>
        /// <param name="findText">String to search for.</param>
        /// <param name="startPosition">Where to start searching from.</param>
        /// <param name="endPosition">Where to stop searching.</param>
        /// <returns>-1 if no match is found, otherwise the position (relative to start) of the first match.</returns>
        public static int FindInTarget(string findText, int startPosition, int endPosition) {
            SetTargetRange(startPosition, endPosition);
            if (IsUtf8())
                findText = findText.Utf8ToAnsi();
            return Call(SciMsg.SCI_SEARCHINTARGET, findText.Length, findText);
        }

        /// <summary>
        ///     Gets the size of a tab as a multiple of the size of a space character in STYLE_DEFAULT. The default tab width is 8
        ///     characters.
        ///     There are no limits on tab sizes, but values less than 1 or large values may have undesirable effects.
        /// </summary>
        public static int GetTabWidth() {
            return Call(SciMsg.SCI_GETTABWIDTH);
        }

        /// <summary>
        ///     Gets the size of indentation in terms of the width of a space in STYLE_DEFAULT. If you set a width of 0,
        ///     the indent size is the same as the tab size. There are no limits on indent sizes, but values less than 0 or
        ///     large values may have undesirable effects.
        /// </summary>
        public static int GetIndent() {
            return Call(SciMsg.SCI_GETINDENT);
        }

        /// <summary>
        ///     Sets the size of a tab as a multiple of the size of a space character in STYLE_DEFAULT. The default tab width is 8
        ///     characters.
        ///     There are no limits on tab sizes, but values less than 1 or large values may have undesirable effects.
        /// </summary>
        /// <param name="tabSize"></param>
        public static void SetTabWidth(int tabSize) {
            Call(SciMsg.SCI_SETTABWIDTH, tabSize);
        }

        /// <summary>
        ///     Sets the size of indentation in terms of the width of a space in STYLE_DEFAULT. If you set a width of 0,
        ///     the indent size is the same as the tab size. There are no limits on indent sizes, but values less than 0 or
        ///     large values may have undesirable effects.
        /// </summary>
        public static void SetIndent(int indentSize) {
            Call(SciMsg.SCI_SETINDENT, indentSize);
        }

        /// <summary>
        ///     Determines whether indentation should be created out of a mixture of tabs and spaces or be based purely on spaces.
        ///     Set useTabs to false (0) to create all tabs and indents out of spaces. The default is true.
        ///     You can use SCI_GETCOLUMN to get the column of a position taking the width of a tab into account.
        /// </summary>
        public static void SetUseTabs(bool useTabs) {
            Call(SciMsg.SCI_SETUSETABS, useTabs ? 1 : 0);
        }

        /// <summary>
        ///     Mark the beginning of a set of operations that you want to undo all as one operation but that you have to generate
        ///     as several operations. Alternatively, you can use these to mark a set of operations that you do not want to have
        ///     combined with the preceding or following operations if they are undone.
        /// </summary>
        public static void BeginUndoAction() {
            Call(SciMsg.SCI_BEGINUNDOACTION);
        }

        /// <summary>
        ///     Mark the end of a set of operations that you want to undo all as one operation but that you have to generate
        ///     as several operations. Alternatively, you can use these to mark a set of operations that you do not want to have
        ///     combined with the preceding or following operations if they are undone.
        /// </summary>
        public static void EndUndoAction() {
            Call(SciMsg.SCI_ENDUNDOACTION);
        }

        /// <summary>
        ///     This returns the number of lines in the document. An empty document contains 1 line. A document holding only an
        ///     end of line sequence has 2 lines.
        /// </summary>
        public static int GetLineCount() {
            return Call(SciMsg.SCI_GETLINECOUNT);
        }

        /// <summary>
        ///     This returns the document position that corresponds with the start of the line. If line is negative,
        ///     the position of the line holding the start of the selection is returned. If line is greater than the
        ///     lines in the document, the return value is -1. If line is equal to the number of lines in the document
        ///     (i.e. 1 line past the last line), the return value is the end of the document.
        /// </summary>
        public static int PositionFromLine(int line) {
            return Call(SciMsg.SCI_POSITIONFROMLINE, line);
        }

        /// <summary>
        ///     Returns the line that contains the position pos in the document. The return value is 0 if pos &lt;= 0.
        ///     The return value is the last line if pos is beyond the end of the document.
        /// </summary>
        public static int LineFromPosition(int pos) {
            return Call(SciMsg.SCI_LINEFROMPOSITION, pos);
        }

        /// <summary>
        ///     Returns the amount of indentation on a line. The indentation is measured in character columns, which correspond to
        ///     the width of space characters.
        /// </summary>
        public static int GetLineIndentation(int line) {
            return Call(SciMsg.SCI_GETLINEINDENTATION, line);
        }

        /// <summary>
        ///     This returns the position at the end of indentation of a line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static int GetLineIndentPosition(int line) {
            return Call(SciMsg.SCI_GETLINEINDENTPOSITION, line);
        }

        public static void SetSearchFlags(bool matchWholeWord, bool matchCase, bool useRegularExpression,
            bool usePosixRegularExpressions) {
            var searchFlags = (matchWholeWord ? (int) SciMsg.SCFIND_WHOLEWORD : 0) |
                              (matchCase ? (int) SciMsg.SCFIND_MATCHCASE : 0) |
                              (useRegularExpression ? (int) SciMsg.SCFIND_REGEXP : 0) |
                              (usePosixRegularExpressions ? (int) SciMsg.SCFIND_POSIX : 0);
            Call(SciMsg.SCI_SETSEARCHFLAGS, searchFlags);
        }

        /// <summary>
        ///     This returns the character at pos in the document or 0 if pos is negative or past the end of the document.
        /// </summary>
        public static char GetCharAt(int pos) {
            var bytes = new List<byte>();
            // PositionAfter helps detect high Unicode characters, get up to 2 more bytes
            var end = Math.Min(PositionAfter(pos), pos + 2);
            for (var i = pos; i < end; i++) {
                bytes.Add((byte) Call(SciMsg.SCI_GETCHARAT, i));
            }
            return IsUtf8()
                ? Encoding.UTF8.GetChars(bytes.ToArray())[0]
                : Encoding.Default.GetChars(bytes.ToArray())[0];
        }

        /// <summary>
        ///     return the position after another position in the document taking into account the current code page.
        ///     The maximum is the last position in the document. If called with a position within a multi byte character will
        ///     return the position of the end of that character.
        /// </summary>
        public static int PositionAfter(int pos) {
            return Call(SciMsg.SCI_POSITIONAFTER, pos);
        }

        #region private Call

        private static int Call(SciMsg msg, int wParam, IntPtr lParam) {
            return (int) Win32.SendMessage(HandleScintilla, msg, wParam, lParam);
        }

        private static int Call(SciMsg msg, int wParam, string lParam) {
            return (int) Win32.SendMessage(HandleScintilla, msg, wParam, lParam);
        }

        private static int Call(SciMsg msg, int wParam, StringBuilder lParam) {
            return (int) Win32.SendMessage(HandleScintilla, msg, wParam, lParam);
        }

        private static int Call(SciMsg msg, int wParam, int lParam) {
            return (int) Win32.SendMessage(HandleScintilla, msg, wParam, lParam);
        }

        private static int Call(SciMsg msg, int wParam) {
            return Call(msg, wParam, 0);
        }

        private static int Call(SciMsg msg) {
            return Call(msg, 0, 0);
        }

        #endregion
    }
}