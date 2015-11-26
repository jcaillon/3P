#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Scintilla.cs) is part of 3P.
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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using _3PA.Interop;

namespace _3PA {
    /// <summary>
    ///     This class contains very generic wrappers for basic Notepad++ functionality.
    /// </summary>
    public partial class Npp {

        #region fields

        public const int KeywordMaxLength = 30;
        private const int IndicatorMatch = 31;
        private const int BookmarkMarker = 24;
        private static IntPtr _curScintilla;

        #endregion


        #region misc for npp/scintilla

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


        /// <summary>
        ///  barbarian method to force the default autocompletion window to hide
        /// <remarks>This is a very bad technique, it makes npp slows down when there is too much text!
        /// I need to find something else but... meh i can't deactivate the default autocomplete</remarks>
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
        /// returns a rectangle representing the location and size of the scintilla window
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetWindowRect() {
            var r = new Rectangle();
            Win32.GetWindowRect(HandleScintilla, ref r);
            return r;
        }

        /// <summary>
        /// allows scintilla to grab focus
        /// </summary>
        /// <returns></returns>
        public static void GrabFocus() {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
        }

        /// <summary>
        /// to be tested!!!!
        /// </summary>
        /// <returns></returns>
        public static bool IsNppFocused() {
            return Call(SciMsg.SCI_GETFOCUS, 0, 0) == 1;
        }

        #endregion


        #region indentation

        /// <summary>
        /// set the indentation of the current line relatively to the previous indentation
        /// </summary>
        /// <param name="indent"></param>
        public static void SetCurrentLineRelativeIndent(int indent) {
            int curPos = GetCaretPosition();
            int line = GetLineFromPosition(curPos);
            Call(SciMsg.SCI_SETLINEINDENTATION, line, GetPreviousLineIndent(curPos) + indent);
            SetCaretPosition(GetLineEndPosition(line));
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
            int lineToIndent = GetLineFromPosition(curPos);
            string curLineText;
            int nbindent;
            int line = lineToIndent;
            do {
                line = line - 1;
                curLineText = GetLineText(line);
                nbindent = GetLineIndent(line);
                //Call(SciMsg.SCI_SETLINEINDENTATION, lineToIndent, nbindent + 4);
            } while (line >= 0 && String.IsNullOrWhiteSpace(curLineText.Trim()) && (lineToIndent - line) < 50);
            return nbindent;
        }

        /// <summary>
        /// returns the indent value as a string, can be either a \t or a number of ' '
        /// </summary>
        /// <returns></returns>
        public static string GetIndentString() {
            return GetUseTabs() ? "\t" : new string(' ', GetTabWidth());
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
        ///     Determines whether indentation should be created out of a mixture of tabs and spaces or be based purely on spaces.
        ///     Set useTabs to false (0) to create all tabs and indents out of spaces. The default is true.
        ///     You can use SCI_GETCOLUMN to get the column of a position taking the width of a tab into account.
        /// </summary>
        public static bool GetUseTabs() {
            var retval = (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETUSETABS, 0, 0);
            return (retval == 1);
        }

        #endregion


        #region mouse

        /// <summary>
        /// returns the x,y point location of the character at the position given
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point GetPointXyFromPosition(int position) {
            int x = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_POINTXFROMPOSITION, 0, position);
            int y = (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_POINTYFROMPOSITION, 0, position);
            return new Point(x, y);
        }

        /// <summary>
        /// Self explaining
        /// </summary>
        /// <returns></returns>
        public static int GetPositionFromMouseLocation() {
            var point = Cursor.Position;
            Win32.ScreenToClient(HandleScintilla, ref point);
            return (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_CHARPOSITIONFROMPOINTCLOSE, point.X, point.Y);
        }

        #endregion


        #region text
        /// <summary>
        /// Returns the text between the positions start and end
        /// </summary>
        public static string GetTextByRange(int start, int end) {
            start = (start > 0) ? start : 0;
            if ((end - start) < 1) return String.Empty;
            using (var textRange = new Sci_TextRange(start, end, end - start + 1)) {
                Call(SciMsg.SCI_GETTEXTRANGE, 0, textRange.NativePointer);
                return IsUtf8() ? textRange.lpstrText.AnsiToUtf8() : textRange.lpstrText;
            }
        }

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam);

        /// <summary>
        /// Replaces a range of text with new text.
        /// Note that the recommended way to delete text in the document is to set the target to the text to be removed, and to
        /// perform a replace target with an empty string.
        /// </summary>
        /// <returns> The length of the replacement string.</returns>
        public static int SetTextByRange(int start, int end, string text) {
            SetTargetRange(start, end);
            if (IsUtf8()) text = text.Utf8ToAnsi();
            return Call(SciMsg.SCI_REPLACETARGET, text.Length, text);
        }

        /// <summary>
        /// Sets the text for the entire document (replacing any existing text).
        /// </summary>
        /// <param name="text">The document text to set.</param>
        public static void SetDocumentText(string text) {
            if (IsUtf8()) text = text.Utf8ToAnsi();
            Call(SciMsg.SCI_SETTEXT, 0, text);
        }

        /// <summary>
        /// Gets the entire document text
        /// </summary>
        public static string GetDocumentText() {
            var length = GetDocumentLength();
            var text = new StringBuilder(length + 1);
            if (length > 0) Call(SciMsg.SCI_GETTEXT, length + 1, text);
            return IsUtf8() ? text.ToString().AnsiToUtf8() : text.ToString();
        }

        /// <summary>
        /// Gets all lines of the current document
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllLines() {
            return GetDocumentText().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        /// <summary>
        /// Returns the text currently selected (highlighted).
        /// </summary>
        /// <returns>Currently selected text.</returns>
        public static string GetSelectedText() {
            var selLength = Call(SciMsg.SCI_GETSELTEXT);
            var selectedText = new StringBuilder(selLength);
            if (selLength > 0) Call(SciMsg.SCI_GETSELTEXT, 0, selectedText);
            var ret = selectedText.ToString();
            return IsUtf8() ? ret.AnsiToUtf8() : ret;
        }

        /// <summary>
        /// The currently selected text is replaced with text. If no text is selected the
        /// text is inserted at current cursor postion.
        /// </summary>
        /// <param name="text">The document text to set.</param>
        public static void SetSelectedText(string text) {
            if (IsUtf8()) text = text.Utf8ToAnsi();
            Call(SciMsg.SCI_REPLACESEL, 0, text);
        }


        /// <summary>
        /// returns the text on the left of the position... it will always return empty string at minima
        /// </summary>
        /// <param name="curPos"></param>
        /// <param name="maxLenght"></param>
        /// <returns></returns>
        public static string GetTextOnLeftOfPos(int curPos, int maxLenght = KeywordMaxLength) {
            var startPos = curPos - maxLenght;
            startPos = (startPos > 0) ? startPos : 0;
            return curPos - startPos > 0 ? GetTextByRange(startPos, curPos) : String.Empty;
        }

        /// <summary>
        /// returns the text on the right of the position... it will always return empty string at minima
        /// </summary>
        /// <param name="curPos"></param>
        /// <param name="maxLenght"></param>
        /// <returns></returns>
        public static string GetTextOnRightOfPos(int curPos, int maxLenght = KeywordMaxLength) {
            var endPos = curPos + maxLenght;
            var fullLength = GetDocumentLength();
            endPos = (endPos < fullLength) ? endPos : fullLength;
            return endPos - curPos > 0 ? GetTextByRange(curPos, endPos) : String.Empty;
        }

        /// <summary>
        /// get the content of the line specified
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetLineText(int line) {
            var length = (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_LINELENGTH, line, 0);
            var buffer = new StringBuilder(length + 1);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETLINE, line, buffer);
            buffer.Length = length; //NPP may inject some rubbish at the end of the line
            return IsUtf8() ? buffer.ToString().AnsiToUtf8() : buffer.ToString();
        }

        /// <summary>
        /// replace the line specified by the text specified
        /// </summary>
        /// <param name="line"></param>
        /// <param name="text"></param>
        public static void SetLineText(int line, string text) {
            SetTextByRange(GetPositionFromLine(line), GetLineEndPosition(line), text);
        }

        /// <summary>
        /// Gets the keyword at given position (reading only on the left of the position)
        /// </summary>
        /// <param name="curPos"></param>
        /// <returns></returns>
        public static string GetKeyword(int curPos = -1) {
            if (curPos < 0) curPos = GetCaretPosition();
            return Abl.ReadAblWord(GetTextOnLeftOfPos(curPos), true);
        }

        /// <summary>
        /// replace the keyword at curPos by the given keyword
        /// (replacing only on the left of the position)
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="curPos"></param>
        public static void SetKeyword(string keyword, int curPos = -1) {
            SetTextByRange(curPos - GetKeyword(curPos).Length, curPos, keyword);
        }

        /// <summary>
        /// Used by the autocompletion to change the case of the current keyword, or replace
        /// the partial keyword by the autocompleted one
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="offset">offset relative to the current carret position</param>
        public static void ReplaceKeywordWrapped(string keyword, int offset) {
            BeginUndoAction();
            var nbCarrets = Call(SciMsg.SCI_GETSELECTIONS);
            for (int i = 0; i < nbCarrets; i++) {
                var curPos = Call(SciMsg.SCI_GETSELECTIONNCARET, i) + offset;
                var word = GetKeyword(curPos);
                SetTextByRange(curPos - word.Length, curPos, keyword);

                // reposition carret
                curPos = curPos - offset + keyword.Length - word.Length;
                Call(SciMsg.SCI_SETSELECTIONNANCHOR, i, curPos);
                Call(SciMsg.SCI_SETSELECTIONNCARET, i, curPos);
            }
            EndUndoAction();
        }

        /// <summary>
        /// returns the first keyword right after the point (reading from right to left)
        /// it is useful to get a table name when we enter a field, or a database name when we enter a table name,
        /// also, if you analyse DATABASE.TABLE.CURFIELD, if returns TABLE and not DATABASE!
        /// </summary>
        /// <param name="curPos"></param>
        /// <returns></returns>
        public static string GetFirstWordRightAfterPoint(int curPos) {
            int nbPoints;
            var wholeWord = Abl.ReadAblWord(GetTextOnLeftOfPos(curPos), false, out nbPoints);
            switch (nbPoints) {
                case 1:
                    return wholeWord.Split('.')[0];
                case 2:
                    return wholeWord.Split('.')[1];
            }
            return String.Empty;
        }

        /// <summary>
        /// At text at the given position
        /// </summary>
        /// <param name="curPos"></param>
        /// <param name="text"></param>
        public static void AddTextAt(int curPos, string text) {
            SetTextByRange(curPos, curPos, text);
            SetCaretPosition(curPos + text.Length);
        }

        /// <summary>
        /// returns the ABL word at cursor
        /// </summary>
        /// <returns></returns>
        public static string GetWordAtCursor() {
            return GetWordAtPosition(GetCaretPosition());
        }

        /// <summary>
        /// Returns the ABL word at the given position (read on left and right) (stops at points)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string GetWordAtPosition(int position) {
            return Abl.ReadAblWord(GetTextOnLeftOfPos(position), true) + Abl.ReadAblWord(GetTextOnRightOfPos(position), true, false);
        }

        public static string GetTextBetween(Point point) {
            return GetTextByRange(point.X, point.Y);
        }

        /// <summary>
        /// This returns the character at pos in the document or 0 if pos is negative or past the end of the document.
        /// </summary>
        public static char GetCharAt(int pos) {
            var bytes = new List<byte>();
            // PositionAfter helps detect high Unicode characters, get up to 2 more bytes
            var end = Math.Min(PositionAfter(pos), pos + 2);
            for (var i = pos; i < end; i++) {
                bytes.Add((byte)Call(SciMsg.SCI_GETCHARAT, i));
            }
            return IsUtf8()
                ? Encoding.UTF8.GetChars(bytes.ToArray())[0]
                : Encoding.Default.GetChars(bytes.ToArray())[0];
        }

        #endregion


        #region selection and position

        /// <summary>
        ///     Sets the start and end positions for an upcoming operation.
        /// </summary>
        public static void SetTargetRange(int start, int end) {
            Call(SciMsg.SCI_SETTARGETSTART, start);
            Call(SciMsg.SCI_SETTARGETEND, end);
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
        /// Clears current selection
        /// </summary>
        public static void ClearSelection() {
            var currentPos = GetCaretPosition();
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTIONEND, currentPos, 0);
        }

        /// <summary>
        /// Sets both the anchor and the current position. If end is negative, it means the end of the document.
        /// If start is negative, it means remove any selection (i.e. set the start to the same position as end).
        /// The caret is scrolled into view after this operation.
        /// </summary>
        /// <param name="start">The selection start (anchor) position.</param>
        /// <param name="end">The selection end (current) position.</param>
        public static void SetSelectionOrdered(int start, int end) {
            Call(SciMsg.SCI_SETSEL, start, end);
        }

        /// <summary>
        /// Sets the selection
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static void SetSelection(int start, int end) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTIONSTART, start, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSELECTIONEND, end, 0);
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
        /// Returns the current carret position
        /// </summary>
        /// <returns></returns>
        public static int GetCaretPosition() {
            return (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
        }

        /// <summary>
        /// Sets the current carret position
        /// </summary>
        /// <param name="pos"></param>
        public static void SetCaretPosition(int pos) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSEL, pos, pos);
        }

        /// <summary>
        /// Returns the current line number
        /// </summary>
        /// <returns></returns>
        public static int GetCaretLineNumber() {
            return GetLineFromPosition(GetCaretPosition());
        }

        /// <summary>
        /// Returns the current anchor position
        /// </summary>
        /// <returns></returns>
        public static int GetAnchorPosition() {
            return (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETANCHOR, 0, 0);
        }

        /// <summary>
        /// This returns the document position that corresponds with the start of the line. If line is negative,
        /// the position of the line holding the start of the selection is returned. If line is greater than the
        /// lines in the document, the return value is -1. If line is equal to the number of lines in the document
        /// (i.e. 1 line past the last line), the return value is the end of the document.
        /// </summary>
        public static int GetPositionFromLine(int line) {
            return Call(SciMsg.SCI_POSITIONFROMLINE, line);
        }

        /// <summary>
        /// returns the position at the end of the line x
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static int GetLineEndPosition(int line) {
            return Call(SciMsg.SCI_GETLINEENDPOSITION, line);
        }


        /// <summary>
        ///     Returns the line that contains the position pos in the document. The return value is 0 if pos &lt;= 0.
        ///     The return value is the last line if pos is beyond the end of the document.
        /// </summary>
        public static int GetLineFromPosition(int pos) {
            return Call(SciMsg.SCI_LINEFROMPOSITION, pos);
        }

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
        /// Gets the first visible line
        /// </summary>
        /// <returns></returns>
        public static int GetFirstVisibleLine() {
            return (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_GETFIRSTVISIBLELINE, 0, 0);
        }

        /// <summary>
        /// Sets the first visible line
        /// </summary>
        /// <param name="line"></param>
        public static void SetFirstVisibleLine(int line) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETFIRSTVISIBLELINE, line, 0);
        }

        public static void ScrollToCaret() {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SCROLLCARET, 0, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_LINESCROLL, 0, 1); //bottom scrollbar can hide the line
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SCROLLCARET, 0, 0);
        }


        /// <summary>
        /// Move the caret and the view to the specified line (lines starts 0!)
        /// </summary>
        /// <param name="line"></param>
        public static void GoToLine(int line) {
            EnsureRangeVisible(line, line);
            var linesOnScreen = GetNumberOfLinesOnScreen();
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GOTOLINE, Math.Max(line + linesOnScreen, 0), 0);
            SetFirstVisibleLine(Math.Max(line - 1, 0));
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GOTOLINE, line, 0);
            GrabFocus();
        }

        /// <summary>
        /// Returns the number of lines displayed in the scintilla view
        /// </summary>
        /// <returns></returns>
        public static int GetNumberOfLinesOnScreen() {
            return (int) Win32.SendMessage(HandleScintilla, SciMsg.SCI_LINESONSCREEN, 0, 0);
        }

        /// <summary>
        ///     Retrieve the height of a particular line of text in pixels.
        /// </summary>
        public static int GetTextHeight(int line) {
            return (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_TEXTHEIGHT, line, 0);
        }

        /// <summary>
        ///     Returns the length of the document in bytes.
        /// </summary>
        public static int GetDocumentLength() {
            return Call(SciMsg.SCI_GETLENGTH);
        }

        /// <summary>
        ///     Make a range visible by scrolling to the last line of the range.
        ///     A line may be hidden because more than one of its parent lines is contracted. Both these message travels up the
        ///     fold hierarchy, expanding any contracted folds until they reach the top level. The line will then be visible.
        /// </summary>
        public static void EnsureRangeVisible(int start, int end) {
            var lineStart = GetLineFromPosition(Math.Min(start, end));
            var lineEnd = GetLineFromPosition(Math.Max(start, end));
            for (var line = lineStart; line <= lineEnd; line++) {
                Call(SciMsg.SCI_ENSUREVISIBLE, line);
            }
        }

        /// <summary>
        /// This searches for the first occurrence of a text string in the target defined by startPosition and endPosition.
        /// The text string is not zero terminated; the size is set by length.
        /// The search is modified by the search flags set by SCI_SETSEARCHFLAGS.
        /// If the search succeeds, the target is set to the found text and the return value is the position of the start
        /// of the matching text. If the search fails, the result is -1.
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
        /// This returns the number of lines in the document. An empty document contains 1 line. A document holding only an
        /// end of line sequence has 2 lines.
        /// </summary>
        public static int GetLineCount() {
            return Call(SciMsg.SCI_GETLINECOUNT);
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
            var searchFlags = (matchWholeWord ? (int)SciMsg.SCFIND_WHOLEWORD : 0) |
                              (matchCase ? (int)SciMsg.SCFIND_MATCHCASE : 0) |
                              (useRegularExpression ? (int)SciMsg.SCFIND_REGEXP : 0) |
                              (usePosixRegularExpressions ? (int)SciMsg.SCFIND_POSIX : 0);
            Call(SciMsg.SCI_SETSEARCHFLAGS, searchFlags);
        }

        /// <summary>
        /// return the position after another position in the document taking into account the current code page.
        /// The maximum is the last position in the document. If called with a position within a multi byte character will
        /// return the position of the end of that character.
        /// </summary>
        public static int PositionAfter(int pos) {
            return Call(SciMsg.SCI_POSITIONAFTER, pos);
        }

        /// <summary>
        /// This message returns the position of a column on a line taking the width of tabs into account. 
        /// It treats a multi-byte character as a single column. Column numbers, like lines start at 0.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static int GetPosFromLineColumn(int line, int column) {
            return Call(SciMsg.SCI_FINDCOLUMN, line, column);
        }

        /// <summary>
        /// This message returns the column number of a position pos within the document taking the width of tabs into account. 
        /// This returns the column number of the last tab on the line before pos, plus the number of characters between the last tab and pos. 
        /// If there are no tab characters on the line, the return value is the number of characters up to the position on the line. 
        /// In both cases, double byte characters count as a single character. 
        /// This is probably only useful with monospaced fonts.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static int GetColumnFromPos(int position) {
            return Call(SciMsg.SCI_GETCOLUMN, position);
        }

        #endregion



        #region others

        /// <summary>
        ///  sets the scroll range so that maximum scroll position has the last line at the bottom of the view (default)
        /// Setting this to false allows scrolling one page below the last line
        /// </summary>
        /// <param name="val"></param>
        public static void SetEndAtLastLine(bool val) {
            Call(SciMsg.SCI_SETENDATLASTLINE, val ? 1 : 0);
        }

        /// <summary>
        /// White space characters are drawn as dots and arrows
        /// SCWS_INVISIBLE	0	The normal display mode with white space displayed as an empty background colour.
        /// SCWS_VISIBLEALWAYS	1	White space characters are drawn as dots and arrows,
        /// SCWS_VISIBLEAFTERINDENT	2	White space used for indentation is displayed normally but after the first visible character, it is shown as dots and arrows.
        /// SCWS_VISIBLEONLYININDENT	3	White space used for indentation is displayed as dots and arrows.
        /// </summary>
        public static void SetWhiteSpaceView() {
            Call(SciMsg.SCI_SETVIEWWS, (int) SciMsg.SCWS_VISIBLEALWAYS);
        }

        /// <summary>
        /// This message changes all the end of line characters in the document to match eolMode. 
        /// Valid values are: SC_EOL_CRLF (0), SC_EOL_CR (1), or SC_EOL_LF (2).
        /// </summary>
        public static void ConvertEolMode() {
            Call(SciMsg.SCI_CONVERTEOLS, (int)SciMsg.SC_EOL_CRLF);
        }

        /// <summary>
        /// sets the characters that are added into the document when the user presses the Enter key. 
        /// You can set eolMode to one of SC_EOL_CRLF (0), SC_EOL_CR (1), or SC_EOL_LF (2). 
        /// The SCI_GETEOLMODE message retrieves the current state
        /// </summary>
        public static void SetEolMode() {
            Call(SciMsg.SCI_SETEOLMODE, (int)SciMsg.SC_EOL_CRLF);
        }

        /// <summary>
        /// reverts the last change
        /// </summary>
        public static void Undo() {
            Call(SciMsg.SCI_UNDO);
        }

        #endregion



        #region indicators

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

        #endregion



        #region bookmarks

        /// <summary>
        ///     Add a bookmark at a specific line.
        /// </summary>
        /// <param name="lineNumber">The line number to add a bookmark to.</param>
        public static void AddBookmark(int lineNumber) {
            if (lineNumber == -1)
                lineNumber = GetCaretLineNumber();
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
                lineNumber = GetCaretLineNumber();
            var state = Call(SciMsg.SCI_MARKERGET, lineNumber);
            return (state & (1 << BookmarkMarker)) != 0;
        }

        #endregion



        #region marks

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

        #endregion



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
            return GetPositionFromLine(GetLineFromPosition(GetLastStyledPosition()));
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
        /// sets the fore/background color of the whitespaces, overriding the lexer's
        /// </summary>
        /// <param name="bg"></param>
        /// <param name="fg"></param>
        public static void SetWhiteSpaceStyle(Color bg, Color fg) {
            Call(SciMsg.SCI_SETWHITESPACEFORE, 1, (int)(new COLORREF(fg)).ColorDWORD);
            Call(SciMsg.SCI_SETWHITESPACEBACK, 1, (int)(new COLORREF(bg)).ColorDWORD);
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
        /// TODO: THIS IS UNTESTED SO FAR!!!
        /// set the style of a text from startPos to startPos + styleArray.Length,
        /// the styleArray is a array of bytes, each byte is the style number to the corresponding text byte
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="styleArray"></param>
        public static void StyleTextEx(int startPos, byte[] styleArray) {
            Call(SciMsg.SCI_STARTSTYLING, startPos, 0);
            Win32.SendData(HandleScintilla, SciMsg.SCI_SETSTYLINGEX, styleArray);
        }

        /// <summary>
        /// returns the style of the caret position
        /// </summary>
        /// <returns></returns>
        public static int GetStyleAt(int caretPos) {
            return Call(SciMsg.SCI_GETSTYLEAT, caretPos, 0);
        }
        #endregion



        #region helper
        /// <summary>
        /// returns true if the document hasn't changed since the last save, false otherwise
        /// </summary>
        /// <returns></returns>
        public static bool IsDocumentSaved() {
            return Call(SciMsg.SCI_GETMODIFY) == 0;
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
        ///     Returns true if the current document is displaying in unicode format or false for ANSI.
        ///     Note that all strings marshaled to and from Scintilla come in ANSI format so need to
        ///     be converted if using Unicode.
        /// </summary>
        public static bool IsUtf8() {
            var result = Call(SciMsg.SCI_GETCODEPAGE);
            return result == (int)SciMsg.SC_CP_UTF8;
        }

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

    static class StringExtension {
        /// <summary>
        /// Converts from ANSI to UTF8
        /// </summary>
        public static string AnsiToUtf8(this string str) {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(str));
        }

        /// <summary>
        /// Converts from UT8 to ANSI
        /// </summary>
        public static string Utf8ToAnsi(this string str) {
            return Encoding.Default.GetString(Encoding.UTF8.GetBytes(str));
        }
    }
}