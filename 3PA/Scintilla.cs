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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using _3PA.Interop;

namespace _3PA {
    /// <summary>
    /// This class contains methods to control scintilla
    /// </summary>
    public partial class Npp {

        #region fields

        public const int KeywordMaxLength = 30;
        private const int IndicatorMatch = 31;
        private const int BookmarkMarker = 24;
        private static IntPtr _curScintilla;
        private static DocumentLines _documentLines;

        #endregion

        #region misc for npp/scintilla

        public static void GetDirectFunction() {
            /*
            // Get the native Scintilla direct function -- the only function the library exports
            var directFunctionPointer = NativeMethods.GetProcAddress(new HandleRef(this, moduleHandle), "Scintilla_DirectFunction");
            if (directFunctionPointer == IntPtr.Zero) {
                var message = "The Scintilla module has no export for the 'Scintilla_DirectFunction' procedure.";
                throw new Win32Exception(message, new Win32Exception()); // Calls GetLastError
            }

            // Create a managed callback
            directFunction = (NativeMethods.Scintilla_DirectFunction)Marshal.GetDelegateForFunctionPointer(
                directFunctionPointer,
                typeof(NativeMethods.Scintilla_DirectFunction));
             * */
        }

        public static DocumentLines Lines {
            get {
                if (_documentLines == null)
                    _documentLines = new DocumentLines();
                return _documentLines;
            }
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


        /// <summary>
        ///  barbarian method to force the default autocompletion window to hide
        /// <remarks>This is a very bad technique, it makes npp slows down when there is too much text!
        /// I need to find something else but... meh i can't deactivate the default autocomplete</remarks>
        /// </summary>
        public static void HideDefaultAutoCompletion() {
            //TODO: find a better technique to hide the autocompletion!!! this slows npp down
            // #$%&'()*+,-./:;<=>?[\]^_`{|}~@
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_AUTOCSTOPS, 0, @"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
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
        /// Retrieve the height of a particular line of text in pixels.
        /// </summary>
        public static int GetTextHeight(int line) {
            return (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_TEXTHEIGHT, line, 0);
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

        /// <summary>
        /// Cancels any displayed autocompletion list.
        /// </summary>
        public static void AutoCCancel() {
            Msg(SciMsg.SCI_AUTOCCANCEL);
        }

        /// <summary>
        /// Returns the zero-based document line index from the specified display line index.
        /// </summary>
        /// <param name="displayLine">The zero-based display line index.</param>
        /// <returns>The zero-based document line index.</returns>
        /// <seealso cref="Line.DisplayIndex" />
        public static int DocLineFromVisible(int displayLine) {
            displayLine = Clamp(displayLine, 0, Lines.Count);
            return Msg(SciMsg.SCI_DOCLINEFROMVISIBLE, new IntPtr(displayLine)).ToInt32();
        }

        /// <summary>
        /// Performs the specified command
        /// //TODO: à tester!!!
        /// </summary>
        /// <param name="sciCommand">The command to perform.</param>
        public static void ExecuteCmd(Command sciCommand) {
            Msg((SciMsg)sciCommand);
        }

        /// <summary>
        /// Measures the width in pixels of the specified string when rendered in the specified style.
        /// </summary>
        /// <param name="style">The index of the <see cref="Style" /> to use when rendering the text to measure.</param>
        /// <param name="text">The text to measure.</param>
        /// <returns>The width in pixels.</returns>
        public static unsafe int TextWidth(int style, string text) {
            style = Clamp(style, 0, 255);
            var bytes = GetBytes(text ?? string.Empty, Encoding, true);

            fixed (byte* bp = bytes) {
                return Msg(SciMsg.SCI_TEXTWIDTH, new IntPtr(style), new IntPtr(bp)).ToInt32();
            }
        }

        /// <summary>
        /// Increases the zoom factor by 1 until it reaches 20 points.
        /// </summary>
        /// <seealso cref="Zoom" />
        public static void ZoomIn() {
            Msg(SciMsg.SCI_ZOOMIN);
        }

        /// <summary>
        /// Decreases the zoom factor by 1 until it reaches -10 points.
        /// </summary>
        /// <seealso cref="Zoom" />
        public static void ZoomOut() {
            Msg(SciMsg.SCI_ZOOMOUT);
        }

        /// <summary>
        /// Gets or sets whether vertical scrolling ends at the last line or can scroll past.
        /// </summary>
        /// <returns>true if the maximum vertical scroll position ends at the last line; otherwise, false. The default is true.</returns>
        public bool EndAtLastLine {
            get {
                return (Msg(SciMsg.SCI_GETENDATLASTLINE) != IntPtr.Zero);
            }
            set {
                var endAtLastLine = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETENDATLASTLINE, endAtLastLine);
            }
        }

        /// <summary>
        /// Gets or sets the end-of-line mode, or rather, the characters added into
        /// the document when the user presses the Enter key.
        /// </summary>
        /// <returns>One of the <see cref="Eol" /> enumeration values. The default is <see cref="Eol.CrLf" />.</returns>
        public Eol EolMode {
            get {
                return (Eol)Msg(SciMsg.SCI_GETEOLMODE);
            }
            set {
                var eolMode = (int)value;
                Msg(SciMsg.SCI_SETEOLMODE, new IntPtr(eolMode));
            }
        }

        /// <summary>
        /// Gets or sets font quality (anti-aliasing method) used to render fonts.
        /// </summary>
        /// <returns>
        /// One of the <see cref="Interop.FontQuality" /> enumeration values.
        /// The default is <see cref="Interop.FontQuality.Default" />.
        /// </returns>
        public FontQuality FontQuality {
            get {
                return (FontQuality)Msg(SciMsg.SCI_GETFONTQUALITY);
            }
            set {
                var fontQuality = (int)value;
                Msg(SciMsg.SCI_SETFONTQUALITY, new IntPtr(fontQuality));
            }
        }

        /// <summary>
        /// Gets the number of lines that can be shown on screen given a constant
        /// line height and the space available.
        /// </summary>
        /// <returns>
        /// The number of screen lines which could be displayed (including any partial lines).
        /// </returns>
        public static int LinesOnScreen {
            get {
                return Msg(SciMsg.SCI_LINESONSCREEN).ToInt32();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document has been modified (is dirty)
        /// since the last call to <see cref="SetSavePoint" />.
        /// </summary>
        /// <returns>true if the document has been modified; otherwise, false.</returns>
        public bool Modified {
            get {
                return (Msg(SciMsg.SCI_GETMODIFY) != IntPtr.Zero);
            }
        }

        /// <summary>
        /// Gets or sets the ability to switch to rectangular selection mode while making a selection with the mouse.
        /// </summary>
        /// <returns>
        /// true if the current mouse selection can be switched to a rectangular selection by pressing the ALT key; otherwise, false.
        /// The default is false.
        /// </returns>
        public bool MouseSelectionRectangularSwitch {
            get {
                return Msg(SciMsg.SCI_GETMOUSESELECTIONRECTANGULARSWITCH) != IntPtr.Zero;
            }
            set {
                var mouseSelectionRectangularSwitch = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETMOUSESELECTIONRECTANGULARSWITCH, mouseSelectionRectangularSwitch);
            }
        }

        /// <summary>
        /// Gets or sets whether multiple selection is enabled.
        /// </summary>
        /// <returns>
        /// true if multiple selections can be made by holding the CTRL key and dragging the mouse; otherwise, false.
        /// The default is false.
        /// </returns>
        public bool MultipleSelection {
            get {
                return Msg(SciMsg.SCI_GETMULTIPLESELECTION) != IntPtr.Zero;
            }
            set {
                var multipleSelection = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETMULTIPLESELECTION, multipleSelection);
            }
        }

        /// <summary>
        /// Gets or sets the behavior when pasting text into multiple selections.
        /// </summary>
        /// <returns>One of the <see cref="Interop.MultiPaste" /> enumeration values. The default is <see cref="Interop.MultiPaste.Once" />.</returns>
        public MultiPaste MultiPaste {
            get {
                return (MultiPaste)Msg(SciMsg.SCI_GETMULTIPASTE);
            }
            set {
                var multiPaste = (int)value;
                Msg(SciMsg.SCI_SETMULTIPASTE, new IntPtr(multiPaste));
            }
        }

        /// <summary>
        /// Gets or sets whether to write over text rather than insert it.
        /// </summary>
        /// <return>true to write over text; otherwise, false. The default is false.</return>
        public bool Overtype {
            get {
                return (Msg(SciMsg.SCI_GETOVERTYPE) != IntPtr.Zero);
            }
            set {
                var overtype = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETOVERTYPE, overtype);
            }
        }

        /// <summary>
        /// Gets or sets whether line endings in pasted text are convereted to the document <see cref="EolMode" />.
        /// </summary>
        /// <returns>true to convert line endings in pasted text; otherwise, false. The default is true.</returns>
        public bool PasteConvertEndings {
            get {
                return (Msg(SciMsg.SCI_GETPASTECONVERTENDINGS) != IntPtr.Zero);
            }
            set {
                var convert = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETPASTECONVERTENDINGS, convert);
            }
        }

        /// <summary>
        /// Gets or sets whether the document is read-only.
        /// </summary>
        /// <returns>true if the document is read-only; otherwise, false. The default is false.</returns>
        /// <seealso cref="ModifyAttempt" />
        public bool ReadOnly {
            get {
                return (Msg(SciMsg.SCI_GETREADONLY) != IntPtr.Zero);
            }
            set {
                var readOnly = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETREADONLY, readOnly);
            }
        }

        /// <summary>
        /// Gets or sets how to display whitespace characters.
        /// </summary>
        /// <returns>One of the <see cref="WhitespaceMode" /> enumeration values. The default is <see cref="WhitespaceMode.Invisible" />.</returns>
        /// <seealso cref="SetWhitespaceForeColor" />
        /// <seealso cref="SetWhitespaceBackColor" />
        public WhitespaceMode ViewWhitespace {
            get {
                return (WhitespaceMode)Msg(SciMsg.SCI_GETVIEWWS);
            }
            set {
                var wsMode = (int)value;
                Msg(SciMsg.SCI_SETVIEWWS, new IntPtr(wsMode));
            }
        }

        /// <summary>
        /// Gets or sets the line wrapping indent mode.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapIndentMode" /> enumeration values. 
        /// The default is <see cref="ScintillaNET.WrapIndentMode.Fixed" />.
        /// </returns>
        public WrapIndentMode WrapIndentMode {
            get {
                return (WrapIndentMode)Msg(SciMsg.SCI_GETWRAPINDENTMODE);
            }
            set {
                var wrapIndentMode = (int)value;
                Msg(SciMsg.SCI_SETWRAPINDENTMODE, new IntPtr(wrapIndentMode));
            }
        }

        /// <summary>
        /// Gets or sets the line wrapping mode.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapMode" /> enumeration values. 
        /// The default is <see cref="ScintillaNET.WrapMode.None" />.
        /// </returns>
        public WrapMode WrapMode {
            get {
                return (WrapMode)Msg(SciMsg.SCI_GETWRAPMODE);
            }
            set {
                var wrapMode = (int)value;
                Msg(SciMsg.SCI_SETWRAPMODE, new IntPtr(wrapMode));
            }
        }

        /// <summary>
        /// Gets or sets the indented size in pixels of wrapped sublines.
        /// </summary>
        /// <returns>The indented size of wrapped sublines measured in pixels. The default is 0.</returns>
        /// <remarks>
        /// Setting <see cref="WrapVisualFlags" /> to <see cref="ScintillaNET.WrapVisualFlags.Start" /> will add an
        /// additional 1 pixel to the value specified.
        /// </remarks>
        public static int WrapStartIndent {
            get {
                return Msg(SciMsg.SCI_GETWRAPSTARTINDENT).ToInt32();
            }
            set {
                value = ClampMin(value, 0);
                Msg(SciMsg.SCI_SETWRAPSTARTINDENT, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the wrap visual flags.
        /// </summary>
        /// <returns>
        /// A bitwise combination of the <see cref="ScintillaNET.WrapVisualFlags" /> enumeration.
        /// The default is <see cref="ScintillaNET.WrapVisualFlags.None" />.
        /// </returns>
        public WrapVisualFlags WrapVisualFlags {
            get {
                return (WrapVisualFlags)Msg(SciMsg.SCI_GETWRAPVISUALFLAGS);
            }
            set {
                int wrapVisualFlags = (int)value;
                Msg(SciMsg.SCI_SETWRAPVISUALFLAGS, new IntPtr(wrapVisualFlags));
            }
        }

        /// <summary>
        /// Gets or sets additional location options when displaying wrap visual flags.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapVisualFlagLocation" /> enumeration values.
        /// The default is <see cref="ScintillaNET.WrapVisualFlagLocation.Default" />.
        /// </returns>
        public WrapVisualFlagLocation WrapVisualFlagLocation {
            get {
                return (WrapVisualFlagLocation)Msg(SciMsg.SCI_GETWRAPVISUALFLAGSLOCATION);
            }
            set {
                var location = (int)value;
                Msg(SciMsg.SCI_SETWRAPVISUALFLAGSLOCATION, new IntPtr(location));
            }
        }

        /// <summary>
        /// Gets or sets the visibility of end-of-line characters.
        /// </summary>
        /// <returns>true to display end-of-line characters; otherwise, false. The default is false.</returns>
        public bool ViewEol {
            get {
                return Msg(SciMsg.SCI_GETVIEWEOL) != IntPtr.Zero;
            }
            set {
                var visible = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETVIEWEOL, visible);
            }
        }

        /// <summary>
        /// Gets or sets the zoom factor.
        /// </summary>
        /// <returns>The zoom factor measured in points.</returns>
        /// <remarks>For best results, values should range from -10 to 20 points.</remarks>
        /// <seealso cref="ZoomIn" />
        /// <seealso cref="ZoomOut" />
        public static int Zoom {
            get {
                return Msg(SciMsg.SCI_GETZOOM).ToInt32();
            }
            set {
                Msg(SciMsg.SCI_SETZOOM, new IntPtr(value));
            }
        }

        #endregion

        #region folding

        /// <summary>
        /// Performs the specified fold action on the entire document.
        /// </summary>
        /// <param name="action">One of the <see cref="FoldAction" /> enumeration values.</param>
        /// <remarks>When using <see cref="FoldAction.Toggle" /> the first fold header in the document is examined to decide whether to expand or contract.</remarks>
        public static void FoldAll(FoldAction action) {
            Msg(SciMsg.SCI_FOLDALL, new IntPtr((int)action));
        }

        /// <summary>
        /// Hides the range of lines specified.
        /// </summary>
        /// <param name="lineStart">The zero-based index of the line range to start hiding.</param>
        /// <param name="lineEnd">The zero-based index of the line range to end hiding.</param>
        /// <seealso cref="ShowLines" />
        /// <seealso cref="Line.Visible" />
        public static void HideLines(int lineStart, int lineEnd) {
            lineStart = Clamp(lineStart, 0, Lines.Count);
            lineEnd = Clamp(lineEnd, lineStart, Lines.Count);

            Msg(SciMsg.SCI_HIDELINES, new IntPtr(lineStart), new IntPtr(lineEnd));
        }

        #endregion

        #region indentation

        /// <summary>
        /// Gets or sets whether to use a mixture of tabs and spaces for indentation or purely spaces.
        /// </summary>
        /// <returns>true to use tab characters; otherwise, false. The default is true.</returns>
        public bool UseTabs {
            get {
                return (Msg(SciMsg.SCI_GETUSETABS) != IntPtr.Zero);
            }
            set {
                var useTabs = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETUSETABS, useTabs);
            }
        }

        /// <summary>
        /// Gets or sets the width of a tab as a multiple of a space character.
        /// </summary>
        /// <returns>The width of a tab measured in characters. The default is 4.</returns>
        public static int TabWidth {
            get {
                return Msg(SciMsg.SCI_GETTABWIDTH).ToInt32();
            }
            set {
                Msg(SciMsg.SCI_SETTABWIDTH, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the size of indentation in terms of space characters.
        /// </summary>
        /// <returns>The indentation size measured in characters. The default is 0.</returns>
        /// <remarks> A value of 0 will make the indent width the same as the tab width.</remarks>
        public static int IndentWidth {
            get {
                return Msg(SciMsg.SCI_GETINDENT).ToInt32();
            }
            set {
                value = ClampMin(value, 0);
                Msg(SciMsg.SCI_SETINDENT, new IntPtr(value));
            }
        }

        /// <summary>
        /// set the indentation of the current line relatively to the previous indentation
        /// </summary>
        /// <param name="indent"></param>
        public static void SetCurrentLineRelativeIndent(int indent) {
            int curPos = CurrentPosition;
            int line = LineFromPosition(curPos);
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
            int lineToIndent = LineFromPosition(curPos);
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

        #region Braces

        /// <summary>
        /// Styles the specified character position with the <see cref="Style.BraceBad" /> style when there is an unmatched brace.
        /// </summary>
        /// <param name="position">The zero-based document position of the unmatched brace character or <seealso cref="InvalidPosition"/> to remove the highlight.</param>
        public static void BraceBadLight(int position) {
            position = Clamp(position, -1, TextLength);
            if (position > 0)
                position = Lines.CharToBytePosition(position);

            Msg(SciMsg.SCI_BRACEBADLIGHT, new IntPtr(position));
        }

        /// <summary>
        /// Styles the specified character positions with the <see cref="Style.BraceLight" /> style.
        /// </summary>
        /// <param name="position1">The zero-based document position of the open brace character.</param>
        /// <param name="position2">The zero-based document position of the close brace character.</param>
        /// <remarks>Brace highlighting can be removed by specifying <see cref="InvalidPosition" /> for <paramref name="position1" /> and <paramref name="position2" />.</remarks>
        /// <seealso cref="HighlightGuide" />
        public static void BraceHighlight(int position1, int position2) {
            var textLength = TextLength;

            position1 = Clamp(position1, -1, textLength);
            if (position1 > 0)
                position1 = Lines.CharToBytePosition(position1);

            position2 = Clamp(position2, -1, textLength);
            if (position2 > 0)
                position2 = Lines.CharToBytePosition(position2);

            Msg(SciMsg.SCI_BRACEHIGHLIGHT, new IntPtr(position1), new IntPtr(position2));
        }

        /// <summary>
        /// Finds a corresponding matching brace starting at the position specified.
        /// The brace characters handled are '(', ')', '[', ']', '{', '}', '&lt;', and '&gt;'.
        /// </summary>
        /// <param name="position">The zero-based document position of a brace character to start the search from for a matching brace character.</param>
        /// <returns>The zero-based document position of the corresponding matching brace or <see cref="InvalidPosition" /> it no matching brace could be found.</returns>
        /// <remarks>A match only occurs if the style of the matching brace is the same as the starting brace. Nested braces are handled correctly.</remarks>
        public static int BraceMatch(int position) {
            position = Clamp(position, 0, TextLength);
            position = Lines.CharToBytePosition(position);

            var match = Msg(SciMsg.SCI_BRACEMATCH, new IntPtr(position), IntPtr.Zero).ToInt32();
            if (match > 0)
                match = Lines.ByteToCharPosition(match);

            return match;
        }

        #endregion

        #region mouse

        /// <summary>
        /// returns the x,y point location of the character at the position given
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point GetPointXyFromPosition(int position) {
            var pos = Lines.CharToBytePosition(position);
            int x = (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_POINTXFROMPOSITION, 0, pos);
            int y = (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_POINTYFROMPOSITION, 0, pos);
            return new Point(x, y);
        }

        /// <summary>
        /// Finds the closest character position to the specified display point or returns -1
        /// if the point is outside the window or not close to any characters.
        /// </summary>
        /// <param name="x">The x pixel coordinate within the client rectangle of the control.</param>
        /// <param name="y">The y pixel coordinate within the client rectangle of the control.</param>
        /// <returns>The zero-based document position of the nearest character to the point specified when near a character; otherwise, -1.</returns>
        public static int CharPositionFromPointClose(int x, int y) {
            var pos = Msg(SciMsg.SCI_CHARPOSITIONFROMPOINTCLOSE, new IntPtr(x), new IntPtr(y)).ToInt32();
            if (pos > -1)
                pos = Lines.ByteToCharPosition(pos);
            return pos;
        }

        /// <summary>
        /// Self explaining
        /// </summary>
        /// <returns></returns>
        public static int GetPositionFromMouseLocation() {
            var point = Cursor.Position;
            Win32.ScreenToClient(HandleScintilla, ref point);
            return CharPositionFromPointClose(point.X, point.Y);
        }

        /// <summary>
        /// Returns the X display pixel location of the specified document position.
        /// </summary>
        /// <param name="pos">The zero-based document character position.</param>
        /// <returns>The x-coordinate of the specified <paramref name="pos" /> within the client rectangle of the control.</returns>
        public static int PointXFromPosition(int pos) {
            pos = Clamp(pos, 0, TextLength);
            pos = Lines.CharToBytePosition(pos);
            return Msg(SciMsg.SCI_POINTXFROMPOSITION, IntPtr.Zero, new IntPtr(pos)).ToInt32();
        }

        /// <summary>
        /// Returns the Y display pixel location of the specified document position.
        /// </summary>
        /// <param name="pos">The zero-based document character position.</param>
        /// <returns>The y-coordinate of the specified <paramref name="pos" /> within the client rectangle of the control.</returns>
        public static int PointYFromPosition(int pos) {
            pos = Clamp(pos, 0, TextLength);
            pos = Lines.CharToBytePosition(pos);
            return Msg(SciMsg.SCI_POINTYFROMPOSITION, IntPtr.Zero, new IntPtr(pos)).ToInt32();
        }

        #endregion

        #region text

        /// <summary>
        /// Gets or sets the current document text in the <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>The text displayed in the control.</returns>
        /// <remarks>Depending on the length of text get or set, this operation can be expensive.</remarks>
        public static unsafe string Text {
            get {
                var length = Msg(SciMsg.SCI_GETTEXTLENGTH).ToInt32();
                var ptr = Msg(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(0), new IntPtr(length));
                if (ptr == IntPtr.Zero)
                    return string.Empty;

                // Assumption is that moving the gap will always be equal to or less expensive
                // than using one of the APIs which requires an intermediate buffer.
                var text = new string((sbyte*)ptr, 0, length, Encoding);
                return text;
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    Msg(SciMsg.SCI_CLEARALL);
                } else {
                    fixed (byte* bp = GetBytes(value, Encoding, true))
                        Msg(SciMsg.SCI_SETTEXT, IntPtr.Zero, new IntPtr(bp));
                }
            }
        }

        /// <summary>
        /// Gets the current target text.
        /// </summary>
        /// <returns>A String representing the text between <see cref="TargetStart" /> and <see cref="TargetEnd" />.</returns>
        /// <remarks>Targets which have a start position equal or greater to the end position will return an empty String.</remarks>
        /// <seealso cref="TargetStart" />
        /// <seealso cref="TargetEnd" />
        public static unsafe string TargetText {
            get {
                var length = Msg(SciMsg.SCI_GETTARGETTEXT).ToInt32();
                if (length == 0)
                    return string.Empty;

                var bytes = new byte[length + 1];
                fixed (byte* bp = bytes) {
                    Msg(SciMsg.SCI_GETTARGETTEXT, IntPtr.Zero, new IntPtr(bp));
                    return GetString(new IntPtr(bp), length, Encoding);
                }
            }
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <returns>The selected text if there is any; otherwise, an empty string.</returns>
        public static unsafe string SelectedText {
            get {
                // NOTE: For some reason the length returned by this API includes the terminating NULL
                var length = Msg(SciMsg.SCI_GETSELTEXT).ToInt32() - 1;
                if (length <= 0)
                    return string.Empty;

                var bytes = new byte[length + 1];
                fixed (byte* bp = bytes) {
                    Msg(SciMsg.SCI_GETSELTEXT, IntPtr.Zero, new IntPtr(bp));
                    return GetString(new IntPtr(bp), length, Encoding);
                }
            }
        }

        /// <summary>
        /// Adds the specified text to the end of the document.
        /// </summary>
        /// <param name="text">The text to add to the document.</param>
        /// <remarks>The current selection is not changed and the new text is not scrolled into view.</remarks>
        public static unsafe void AppendText(string text) {
            var bytes = GetBytes(text ?? string.Empty, Encoding, false);
            fixed (byte* bp = bytes)
                Msg(SciMsg.SCI_APPENDTEXT, new IntPtr(bytes.Length), new IntPtr(bp));
        }

        /// <summary>
        /// Inserts the specified text at the current caret position.
        /// </summary>
        /// <param name="text">The text to insert at the current caret position.</param>
        /// <remarks>The caret position is set to the end of the inserted text, but it is not scrolled into view.</remarks>
        public static unsafe void AddText(string text) {
            var bytes = GetBytes(text ?? string.Empty, Encoding, false);
            fixed (byte* bp = bytes)
                Msg(SciMsg.SCI_ADDTEXT, new IntPtr(bytes.Length), new IntPtr(bp));
        }

        /// <summary>
        /// Removes the selected text from the document.
        /// </summary>
        public static void Clear() {
            Msg(SciMsg.SCI_CLEAR);
        }

        /// <summary>
        /// Deletes all document text, unless the document is read-only.
        /// </summary>
        public static void ClearAll() {
            Msg(SciMsg.SCI_CLEARALL);
        }

        /// <summary>
        /// Changes all end-of-line characters in the document to the format specified.
        /// </summary>
        /// <param name="eolMode">One of the <see cref="Eol" /> enumeration values.</param>
        public static void ConvertEols(Eol eolMode) {
            var eol = (int)eolMode;
            Msg(SciMsg.SCI_CONVERTEOLS, new IntPtr(eol));
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// </summary>
        public static void Copy() {
            Msg(SciMsg.SCI_COPY);
        }

        /// <summary>
        /// Pastes the contents of the clipboard into the current selection.
        /// </summary>
        public static void Paste() {
            Msg(SciMsg.SCI_PASTE);
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// If the selection is empty the current line is copied.
        /// </summary>
        /// <remarks>
        /// If the selection is empty and the current line copied, an extra "MSDEVLineSelect" marker is added to the
        /// clipboard which is then used in <see cref="Paste" /> to paste the whole line before the current line.
        /// </remarks>
        public static void CopyAllowLine() {
            Msg(SciMsg.SCI_COPYALLOWLINE);
        }

        /// <summary>
        /// Copies the specified range of text to the clipboard.
        /// </summary>
        /// <param name="start">The zero-based character position in the document to start copying.</param>
        /// <param name="end">The zero-based character position (exclusive) in the document to stop copying.</param>
        public static void CopyRange(int start, int end) {
            var textLength = TextLength;
            start = Clamp(start, 0, textLength);
            end = Clamp(end, 0, textLength);

            // Convert to byte positions
            start = Lines.CharToBytePosition(start);
            end = Lines.CharToBytePosition(end);

            Msg(SciMsg.SCI_COPYRANGE, new IntPtr(start), new IntPtr(end));
        }

        /// <summary>
        /// Cuts the selected text from the document and places it on the clipboard.
        /// </summary>
        public static void Cut() {
            Msg(SciMsg.SCI_CUT);
        }

        /// <summary>
        /// Deletes a range of text from the document.
        /// </summary>
        /// <param name="position">The zero-based character position to start deleting.</param>
        /// <param name="length">The number of characters to delete.</param>
        public static void DeleteRange(int position, int length) {
            var textLength = TextLength;
            position = Clamp(position, 0, textLength);
            length = Clamp(length, 0, textLength - position);

            // Convert to byte position/length
            var byteStartPos = Lines.CharToBytePosition(position);
            var byteEndPos = Lines.CharToBytePosition(position + length);

            Msg(SciMsg.SCI_DELETERANGE, new IntPtr(byteStartPos), new IntPtr(byteEndPos - byteStartPos));
        }

        /// <summary>
        /// Returns the character as the specified document position.
        /// </summary>
        /// <param name="position">The zero-based document position of the character to get.</param>
        /// <returns>The character at the specified <paramref name="position" />.</returns>
        public static unsafe int GetCharAt(int position) {
            position = Clamp(position, 0, TextLength);
            position = Lines.CharToBytePosition(position);

            var nextPosition = Msg(SciMsg.SCI_POSITIONRELATIVE, new IntPtr(position), new IntPtr(1)).ToInt32();
            var length = (nextPosition - position);
            if (length <= 1) {
                // Position is at single-byte character
                return Msg(SciMsg.SCI_GETCHARAT, new IntPtr(position)).ToInt32();
            }

            // Position is at multibyte character
            var bytes = new byte[length + 1];
            fixed (byte* bp = bytes) {
                Sci_TextRange* range = stackalloc Sci_TextRange[1];
                range->chrg.cpMin = position;
                range->chrg.cpMax = nextPosition;
                range->lpstrText = new IntPtr(bp);

                Msg(SciMsg.SCI_GETTEXTRANGE, IntPtr.Zero, new IntPtr(range));
                var str = GetString(new IntPtr(bp), length, Encoding);
                return str[0];
            }
        }

        /// <summary>
        /// Gets a range of text from the document.
        /// </summary>
        /// <param name="position">The zero-based starting character position of the range to get.</param>
        /// <param name="length">The number of characters to get.</param>
        /// <returns>A string representing the text range.</returns>
        public static string GetTextRange(int position, int length) {
            var textLength = TextLength;
            position = Clamp(position, 0, textLength);
            length = Clamp(length, 0, textLength - position);

            // Convert to byte position/length
            var byteStartPos = Lines.CharToBytePosition(position);
            var byteEndPos = Lines.CharToBytePosition(position + length);

            var ptr = Msg(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(byteStartPos), new IntPtr(byteEndPos - byteStartPos));
            if (ptr == IntPtr.Zero)
                return string.Empty;

            return GetString(ptr, (byteEndPos - byteStartPos), Encoding);
        }

        /// <summary>
        /// Gets a range of text from the document.
        /// </summary>
        /// <param name="start">The zero-based starting character position of the range to get.</param>
        /// <param name="end">The zero-based ending character position of the range to get.</param>
        /// <returns>A string representing the text range.</returns>
        public static string GetTextByRange(int start, int end) {
            SetTargetRange(start, end);
            return TargetText;
        }

        /// <summary>
        /// Inserts text at the specified position.
        /// </summary>
        /// <param name="position">The zero-based character position to insert the text. Specify -1 to use the current caret position.</param>
        /// <param name="text">The text to insert into the document.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position" /> less than zero and not equal to -1. -or-
        /// <paramref name="position" /> is greater than the document length.
        /// </exception>
        /// <remarks>No scrolling is performed.</remarks>
        public static unsafe void InsertText(int position, string text) {
            if (position < -1)
                position = 0;
            var textLength = TextLength;
            if (position > textLength)
                position = textLength;
            position = Lines.CharToBytePosition(position);
            fixed (byte* bp = GetBytes(text ?? string.Empty, Encoding, true))
                Msg(SciMsg.SCI_INSERTTEXT, new IntPtr(position), new IntPtr(bp));
        }

        /// <summary>
        /// Replaces the current selection with the specified text.
        /// </summary>
        /// <param name="text">The text that should replace the current selection.</param>
        /// <remarks>
        /// If there is not a current selection, the text will be inserted at the current caret position.
        /// Following the operation the caret is placed at the end of the inserted text and scrolled into view.
        /// </remarks>
        public static unsafe void ReplaceSelection(string text) {
            // TODO I don't like how using a null/empty string does nothing

            fixed (byte* bp = GetBytes(text ?? string.Empty, Encoding, true))
                Msg(SciMsg.SCI_REPLACESEL, IntPtr.Zero, new IntPtr(bp));
        }

        /// <summary>
        /// Replaces the target defined by <see cref="TargetStart" /> and <see cref="TargetEnd" /> with the specified <paramref name="text" />.
        /// </summary>
        /// <param name="text">The text that will replace the current target.</param>
        /// <returns>The length of the replaced text.</returns>
        /// <remarks>
        /// The <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties will be updated to the start and end positions of the replaced text.
        /// The recommended way to delete text in the document is to set the target range to be removed and replace the target with an empty string.
        /// </remarks>
        public static unsafe int ReplaceTarget(string text) {
            if (text == null)
                text = string.Empty;
            var bytes = GetBytes(text, Encoding, false);
            fixed (byte* bp = bytes)
                Msg(SciMsg.SCI_REPLACETARGET, new IntPtr(bytes.Length), new IntPtr(bp));
            return text.Length;
        }

        /// <summary>
        /// Replaces the target text defined by <see cref="TargetStart" /> and <see cref="TargetEnd" /> with the specified value after first substituting
        /// "\1" through "\9" macros in the <paramref name="text" /> with the most recent regular expression capture groups.
        /// </summary>
        /// <param name="text">The text containing "\n" macros that will be substituted with the most recent regular expression capture groups and then replace the current target.</param>
        /// <returns>The length of the replaced text.</returns>
        /// <remarks>
        /// The "\0" macro will be substituted by the entire matched text from the most recent search.
        /// The <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties will be updated to the start and end positions of the replaced text.
        /// </remarks>
        /// <seealso cref="GetTag" />
        public static unsafe int ReplaceTargetRe(string text) {
            var bytes = GetBytes(text ?? string.Empty, Encoding, false);
            fixed (byte* bp = bytes)
                Msg(SciMsg.SCI_REPLACETARGETRE, new IntPtr(bytes.Length), new IntPtr(bp));

            return Math.Abs(TargetEnd - TargetStart);
        }

        /// <summary>
        /// Replaces a range of text with new text.
        /// Note that the recommended way to delete text in the document is to set the target to the text to be removed, and to
        /// perform a replace target with an empty string.
        /// </summary>
        /// <returns> The length of the replacement string.</returns>
        public static int SetTextByRange(int start, int end, string text) {
            SetTargetRange(start, end);
            return ReplaceTarget(text); ;
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
            var fullLength = TextLength;
            endPos = (endPos < fullLength) ? endPos : fullLength;
            return endPos - curPos > 0 ? GetTextByRange(curPos, endPos) : String.Empty;
        }

        /// <summary>
        /// get the content of the line specified
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static unsafe string GetLineText(int line) {
            var start = Msg(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(line));
            var length = Msg(SciMsg.SCI_LINELENGTH, new IntPtr(line));
            var ptr = Msg(SciMsg.SCI_GETRANGEPOINTER, start, length);
            if (ptr == IntPtr.Zero)
                return string.Empty;

            var text = new string((sbyte*)ptr, 0, length.ToInt32(), Encoding);
            return text;
        }

        /// <summary>
        /// replace the line specified by the text specified
        /// </summary>
        /// <param name="line"></param>
        /// <param name="text"></param>
        public static void SetLineText(int line, string text) {
            SetTextByRange(PositionFromLine(line), GetLineEndPosition(line), text);
        }

        /// <summary>
        /// Gets the keyword at given position (reading only on the left of the position)
        /// </summary>
        /// <param name="curPos"></param>
        /// <returns></returns>
        public static string GetKeyword(int curPos = -1) {
            if (curPos < 0) curPos = CurrentPosition;
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
            return GetWordAtPosition(CurrentPosition);
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
        /// Get document lenght (number of character!!)
        /// </summary>
        public static int TextLength {
            get {
                return Lines.TextLength;
            }
        }

        #endregion

        #region search


        /// <summary>
        /// Sets the <see cref="TargetStart" /> and <see cref="TargetEnd" /> to the start and end positions of the selection.
        /// </summary>
        /// <seealso cref="TargetWholeDocument" />
        public static void TargetFromSelection() {
            Msg(SciMsg.SCI_TARGETFROMSELECTION);
        }

        /// <summary>
        /// Sets the <see cref="TargetStart" /> and <see cref="TargetEnd" /> to the start and end positions of the document.
        /// </summary>
        /// <seealso cref="TargetFromSelection" />
        public static void TargetWholeDocument() {
            Msg(SciMsg.SCI_TARGETWHOLEDOCUMENT);
        }

        /// <summary>
        /// Gets or sets the search flags used when searching text.
        /// </summary>
        /// <returns>A bitwise combination of <see cref="Interop.SearchFlags" /> values. The default is <see cref="Interop.SearchFlags.None" />.</returns>
        /// <seealso cref="SearchInTarget" />
        public SearchFlags SearchFlags {
            get {
                return (SearchFlags)Msg(SciMsg.SCI_GETSEARCHFLAGS).ToInt32();
            }
            set {
                var searchFlags = (int)value;
                Msg(SciMsg.SCI_SETSEARCHFLAGS, new IntPtr(searchFlags));
            }
        }

        /// <summary>
        /// Sets the <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties in a single call.
        /// </summary>
        /// <param name="start">The zero-based character position within the document to start a search or replace operation.</param>
        /// <param name="end">The zero-based character position within the document to end a search or replace operation.</param>
        /// <seealso cref="TargetStart" />
        /// <seealso cref="TargetEnd" />
        public static void SetTargetRange(int start, int end) {
            var textLength = TextLength;
            start = Clamp(start, 0, textLength);
            end = Clamp(end, 0, textLength);

            start = Lines.CharToBytePosition(start);
            end = Lines.CharToBytePosition(end);

            Msg(SciMsg.SCI_SETTARGETRANGE, new IntPtr(start), new IntPtr(end));
        }

        /// <summary>
        /// Gets or sets the end position used when performing a search or replace.
        /// </summary>
        /// <returns>The zero-based character position within the document to end a search or replace operation.</returns>
        /// <seealso cref="TargetStart"/>
        /// <seealso cref="SearchInTarget" />
        /// <seealso cref="ReplaceTarget" />
        public static int TargetEnd {
            get {
                // The position can become stale and point to a place outside of the document so we must clamp it
                var bytePos = Clamp(Msg(SciMsg.SCI_GETTARGETEND).ToInt32(), 0, Msg(SciMsg.SCI_GETTEXTLENGTH).ToInt32());
                return Lines.ByteToCharPosition(bytePos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                value = Lines.CharToBytePosition(value);
                Msg(SciMsg.SCI_SETTARGETEND, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the start position used when performing a search or replace.
        /// </summary>
        /// <returns>The zero-based character position within the document to start a search or replace operation.</returns>
        /// <seealso cref="TargetEnd"/>
        /// <seealso cref="SearchInTarget" />
        /// <seealso cref="ReplaceTarget" />
        public static int TargetStart {
            get {
                // The position can become stale and point to a place outside of the document so we must clamp it
                var bytePos = Clamp(Msg(SciMsg.SCI_GETTARGETSTART).ToInt32(), 0, Msg(SciMsg.SCI_GETTEXTLENGTH).ToInt32());
                return Lines.ByteToCharPosition(bytePos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                value = Lines.CharToBytePosition(value);
                Msg(SciMsg.SCI_SETTARGETSTART, new IntPtr(value));
            }
        }

        /// <summary>
        /// Returns the capture group text of the most recent regular expression search.
        /// </summary>
        /// <param name="tagNumber">The capture group (1 through 9) to get the text for.</param>
        /// <returns>A String containing the capture group text if it participated in the match; otherwise, an empty string.</returns>
        /// <seealso cref="SearchInTarget" />
        public static unsafe string GetTag(int tagNumber) {
            tagNumber = Clamp(tagNumber, 1, 9);
            var length = Msg(SciMsg.SCI_GETTAG, new IntPtr(tagNumber), IntPtr.Zero).ToInt32();
            if (length <= 0)
                return string.Empty;

            var bytes = new byte[length + 1];
            fixed (byte* bp = bytes) {
                Msg(SciMsg.SCI_GETTAG, new IntPtr(tagNumber), new IntPtr(bp));
                return GetString(new IntPtr(bp), length, Encoding);
            }
        }

        /// <summary>
        /// Searches for the first occurrence of the specified text in the target defined by <see cref="TargetStart" /> and <see cref="TargetEnd" />.
        /// </summary>
        /// <param name="text">The text to search for. The interpretation of the text (i.e. whether it is a regular expression) is defined by the <see cref="SearchFlags" /> property.</param>
        /// <returns>The zero-based start position of the matched text within the document if successful; otherwise, -1.</returns>
        /// <remarks>
        /// If successful, the <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties will be updated to the start and end positions of the matched text.
        /// Searching can be performed in reverse using a <see cref="TargetStart" /> greater than the <see cref="TargetEnd" />.
        /// </remarks>
        public static unsafe int SearchInTarget(string text) {
            int bytePos = 0;
            var bytes = GetBytes(text ?? string.Empty, Encoding, false);
            fixed (byte* bp = bytes)
                bytePos = Msg(SciMsg.SCI_SEARCHINTARGET, new IntPtr(bytes.Length), new IntPtr(bp)).ToInt32();

            if (bytePos == -1)
                return bytePos;

            return Lines.ByteToCharPosition(bytePos);
        }

        #endregion

        #region selection and position

        /// <summary>
        /// Gets or sets the start position of the selection.
        /// </summary>
        /// <returns>The zero-based document position where the selection starts.</returns>
        /// <remarks>
        /// When getting this property, the return value is <code>Math.Min(<see cref="AnchorPosition" />, <see cref="CurrentPosition" />)</code>.
        /// When setting this property, <see cref="AnchorPosition" /> is set to the value specified and <see cref="CurrentPosition" /> set to <code>Math.Max(<see cref="CurrentPosition" />, <paramref name="value" />)</code>.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="SelectionEnd" />
        public static int SelectionStart {
            get {
                var pos = Msg(SciMsg.SCI_GETSELECTIONSTART).ToInt32();
                return Lines.ByteToCharPosition(pos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                value = Lines.CharToBytePosition(value);
                Msg(SciMsg.SCI_SETSELECTIONSTART, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the end position of the selection.
        /// </summary>
        /// <returns>The zero-based document position where the selection ends.</returns>
        /// <remarks>
        /// When getting this property, the return value is <code>Math.Max(<see cref="AnchorPosition" />, <see cref="CurrentPosition" />)</code>.
        /// When setting this property, <see cref="CurrentPosition" /> is set to the value specified and <see cref="AnchorPosition" /> set to <code>Math.Min(<see cref="AnchorPosition" />, <paramref name="value" />)</code>.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="SelectionStart" />
        public static int SelectionEnd {
            get {
                var pos = Msg(SciMsg.SCI_GETSELECTIONEND).ToInt32();
                return Lines.ByteToCharPosition(pos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                value = Lines.CharToBytePosition(value);
                Msg(SciMsg.SCI_SETSELECTIONEND, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the main selection when they are multiple selections.
        /// </summary>
        /// <returns>The zero-based main selection index.</returns>
        public static int MainSelection {
            get {
                return Msg(SciMsg.SCI_GETMAINSELECTION).ToInt32();
            }
            set {
                value = ClampMin(value, 0);
                Msg(SciMsg.SCI_SETMAINSELECTION, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the first visible line on screen.
        /// </summary>
        /// <returns>The zero-based index of the first visible screen line.</returns>
        /// <remarks>The value is a visible line, not a document line.</remarks>
        public static int FirstVisibleLine {
            get {
                return Msg(SciMsg.SCI_GETFIRSTVISIBLELINE).ToInt32();
            }
            set {
                value = ClampMin(value, 0);
                Msg(SciMsg.SCI_SETFIRSTVISIBLELINE, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the current caret position.
        /// </summary>
        /// <returns>The zero-based character position of the caret.</returns>
        /// <remarks>
        /// Setting the current caret position will create a selection between it and the current <see cref="AnchorPosition" />.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="ScrollCaret" />
        public static int CurrentPosition {
            get {
                var bytePos = Msg(SciMsg.SCI_GETCURRENTPOS).ToInt32();
                return Lines.ByteToCharPosition(bytePos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                var bytePos = Lines.CharToBytePosition(value);
                Msg(SciMsg.SCI_SETCURRENTPOS, new IntPtr(bytePos));
            }
        }

        /// <summary>
        /// Sets the current carret position
        /// </summary>
        /// <param name="pos"></param>
        public static void SetCaretPosition(int pos) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSEL, pos, pos);
        }

        /// <summary>
        /// Gets the current line index.
        /// </summary>
        /// <returns>The zero-based line index containing the <see cref="CurrentPosition" />.</returns>
        public static int CurrentLine {
            get {
                var currentPos = Msg(SciMsg.SCI_GETCURRENTPOS).ToInt32();
                var line = Msg(SciMsg.SCI_LINEFROMPOSITION, new IntPtr(currentPos)).ToInt32();
                return line;
            }
        }

        /// <summary>
        /// Gets or sets the current anchor position.
        /// </summary>
        /// <returns>The zero-based character position of the anchor.</returns>
        /// <remarks>
        /// Setting the current anchor position will create a selection between it and the <see cref="CurrentPosition" />.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="ScrollCaret" />
        public static int AnchorPosition {
            get {
                var bytePos = Msg(SciMsg.SCI_GETANCHOR).ToInt32();
                return Lines.ByteToCharPosition(bytePos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                var bytePos = Lines.CharToBytePosition(value);
                Msg(SciMsg.SCI_SETANCHOR, new IntPtr(bytePos));
            }
        }

        /// <summary>
        /// Gets or sets whether additional typing affects multiple selections.
        /// Whether typing, backspace, or delete works with multiple selection simultaneously.
        /// </summary>
        /// <returns>true if typing will affect multiple selections instead of just the main selection; otherwise, false. The default is false.</returns>
        public bool AdditionalSelectionTyping {
            get {
                return Msg(SciMsg.SCI_GETADDITIONALSELECTIONTYPING) != IntPtr.Zero;
            }
            set {
                var additionalSelectionTyping = (value ? new IntPtr(1) : IntPtr.Zero);
                Msg(SciMsg.SCI_SETADDITIONALSELECTIONTYPING, additionalSelectionTyping);
            }
        }

        /// <summary>
        /// Moves the caret to the opposite end of the main selection.
        /// </summary>
        public static void SwapMainAnchorCaret() {
            Msg(SciMsg.SCI_SWAPMAINANCHORCARET);
        }

        /// <summary>
        /// Shows the range of lines specified.
        /// </summary>
        /// <param name="lineStart">The zero-based index of the line range to start showing.</param>
        /// <param name="lineEnd">The zero-based index of the line range to end showing.</param>
        /// <seealso cref="HideLines" />
        /// <seealso cref="Line.Visible" />
        public static void ShowLines(int lineStart, int lineEnd) {
            lineStart = Clamp(lineStart, 0, Lines.Count);
            lineEnd = Clamp(lineEnd, lineStart, Lines.Count);

            Msg(SciMsg.SCI_SHOWLINES, new IntPtr(lineStart), new IntPtr(lineEnd));
        }

        /// <summary>
        /// Sets the anchor and current position.
        /// </summary>
        /// <param name="anchorPos">The zero-based document position to start the selection.</param>
        /// <param name="currentPos">The zero-based document position to end the selection.</param>
        /// <remarks>
        /// A negative value for <paramref name="currentPos" /> signifies the end of the document.
        /// A negative value for <paramref name="anchorPos" /> signifies no selection (set the <paramref name="anchorPos" /> to the same as the <paramref name="currentPos" />).
        /// The current position is scrolled into view following this operation.
        /// </remarks>
        public static void SetSel(int anchorPos, int currentPos) {
            var textLength = TextLength;

            if (anchorPos >= 0) {
                anchorPos = Clamp(anchorPos, 0, textLength);
                anchorPos = Lines.CharToBytePosition(anchorPos);
            }

            if (currentPos >= 0) {
                currentPos = Clamp(currentPos, 0, textLength);
                currentPos = Lines.CharToBytePosition(currentPos);
            }

            Msg(SciMsg.SCI_SETSEL, new IntPtr(anchorPos), new IntPtr(currentPos));
        }

        /// <summary>
        /// Sets a single selection from anchor to caret.
        /// </summary>
        /// <param name="caret">The zero-based document position to end the selection.</param>
        /// <param name="anchor">The zero-based document position to start the selection.</param>
        public static void SetSelection(int caret, int anchor) {
            var textLength = TextLength;

            caret = Clamp(caret, 0, textLength);
            anchor = Clamp(anchor, 0, textLength);

            caret = Lines.CharToBytePosition(caret);
            anchor = Lines.CharToBytePosition(anchor);

            Msg(SciMsg.SCI_SETSELECTION, new IntPtr(caret), new IntPtr(anchor));
        }

        /// <summary>
        /// Selects all the text in the document.
        /// </summary>
        /// <remarks>The current position is not scrolled into view.</remarks>
        public static void SelectAll() {
            Msg(SciMsg.SCI_SELECTALL);
        }

        /// <summary>
        /// Sets a single empty selection at the start of the document.
        /// </summary>
        public static void ClearSelections() {
            Msg(SciMsg.SCI_CLEARSELECTIONS);
        }

        /// <summary>
        /// If there are multiple selections, removes the specified selection.
        /// </summary>
        /// <param name="selection">The zero-based selection index.</param>
        /// <seealso cref="Selections" />
        public static void DropSelection(int selection) {
            selection = ClampMin(selection, 0);
            Msg(SciMsg.SCI_DROPSELECTIONN, new IntPtr(selection));
        }

        /// <summary>
        /// Returns the column number of the specified document position, taking the width of tabs into account.
        /// </summary>
        /// <param name="position">The zero-based document position to get the column for.</param>
        /// <returns>The number of columns from the start of the line to the specified document <paramref name="position" />.</returns>
        public static int GetColumn(int position) {
            position = Clamp(position, 0, TextLength);
            position = Lines.CharToBytePosition(position);
            return Msg(SciMsg.SCI_GETCOLUMN, new IntPtr(position)).ToInt32();
        }

        /// <summary>
        /// Navigates the caret to the document position specified.
        /// </summary>
        /// <param name="position">The zero-based document character position to navigate to.</param>
        /// <remarks>Any selection is discarded.</remarks>
        public static void GotoPosition(int position) {
            position = Clamp(position, 0, TextLength);
            position = Lines.CharToBytePosition(position);
            Msg(SciMsg.SCI_GOTOPOS, new IntPtr(position));
        }

        /// <summary>
        /// Returns the line that contains the document position specified.
        /// </summary>
        /// <param name="position">The zero-based document character position.</param>
        /// <returns>The zero-based document line index containing the character <paramref name="position" />.</returns>
        public static int LineFromPosition(int position) {
            position = Clamp(position, 0, TextLength);
            return Lines.LineFromCharPosition(position);
        }

        /// <summary>
        /// Scrolls the display the number of lines and columns specified.
        /// </summary>
        /// <param name="lines">The number of lines to scroll.</param>
        /// <param name="columns">The number of columns to scroll.</param>
        /// <remarks>
        /// Negative values scroll in the opposite direction.
        /// A column is the width in pixels of a space character in the <see cref="Style.Default" /> style.
        /// </remarks>
        public static void LineScroll(int lines, int columns) {
            Msg(SciMsg.SCI_LINESCROLL, new IntPtr(columns), new IntPtr(lines));
        }

        /// <summary>
        /// Makes the next selection the main selection.
        /// </summary>
        public static void RotateSelection() {
            Msg(SciMsg.SCI_ROTATESELECTION);
        }

        /// <summary>
        /// Scrolls the current position into view, if it is not already visible.
        /// </summary>
        public static void ScrollCaret() {
            Msg(SciMsg.SCI_SCROLLCARET);
        }

        /// <summary>
        /// Scrolls the specified range into view.
        /// </summary>
        /// <param name="start">The zero-based document start position to scroll to.</param>
        /// <param name="end">
        /// The zero-based document end position to scroll to if doing so does not cause the <paramref name="start" />
        /// position to scroll out of view.
        /// </param>
        /// <remarks>This may be used to make a search match visible.</remarks>
        public static void ScrollRange(int start, int end) {
            var textLength = TextLength;
            start = Clamp(start, 0, textLength);
            end = Clamp(end, 0, textLength);

            // Convert to byte positions
            start = Lines.CharToBytePosition(start);
            end = Lines.CharToBytePosition(end);

            Msg(SciMsg.SCI_SCROLLRANGE, new IntPtr(end), new IntPtr(start));
        }

        /// <summary>
        /// Removes any selection and places the caret at the specified position.
        /// </summary>
        /// <param name="pos">The zero-based document position to place the caret at.</param>
        /// <remarks>The caret is not scrolled into view.</remarks>
        public static void SetEmptySelection(int pos) {
            pos = Clamp(pos, 0, TextLength);
            pos = Lines.CharToBytePosition(pos);
            Msg(SciMsg.SCI_SETEMPTYSELECTION, new IntPtr(pos));
        }

        /// <summary>
        /// Returns the current target start and end positions from a previous operation.
        /// </summary>
        public static Sci_CharacterRange GetTargetRange() {
            return new Sci_CharacterRange(
                Lines.ByteToCharPosition(Call(SciMsg.SCI_GETTARGETSTART)),
                Lines.ByteToCharPosition(Call(SciMsg.SCI_GETTARGETEND)));
        }

        /// <summary>
        /// Sets both the anchor and the current position. If end is negative, it means the end of the document.
        /// If start is negative, it means remove any selection (i.e. set the start to the same position as end).
        /// The caret is scrolled into view after this operation.
        /// </summary>
        /// <param name="start">The selection start (anchor) position.</param>
        /// <param name="end">The selection end (current) position.</param>
        public static void SetSelectionOrdered(int start, int end) {
            Call(SciMsg.SCI_SETSEL, Lines.CharToBytePosition(start), Lines.CharToBytePosition(end));
        }

        /// <summary>
        ///     Returns the start and end of the selection without regard to which end is the current position and which is the
        ///     anchor.
        ///     SCI_GETSELECTIONSTART returns the smaller of the current position or the anchor position.
        /// </summary>
        /// <returns>A character range.</returns>
        public static Sci_CharacterRange GetSelectionRange() {
            return new Sci_CharacterRange(
                Lines.ByteToCharPosition(Call(SciMsg.SCI_GETSELECTIONSTART)),
                Lines.ByteToCharPosition(Call(SciMsg.SCI_GETSELECTIONEND)));
        }

        /// <summary>
        /// Returns the current line number
        /// </summary>
        /// <returns></returns>
        public static int GetCaretLineNumber() {
            return LineFromPosition(CurrentPosition);
        }

        /// <summary>
        /// This returns the document position that corresponds with the start of the line. If line is negative,
        /// the position of the line holding the start of the selection is returned. If line is greater than the
        /// lines in the document, the return value is -1. If line is equal to the number of lines in the document
        /// (i.e. 1 line past the last line), the return value is the end of the document.
        /// </summary>
        public static int PositionFromLine(int line) {
            return Lines.ByteToCharPosition(Call(SciMsg.SCI_POSITIONFROMLINE, line));
        }

        /// <summary>
        /// returns the position at the end of the line x
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static int GetLineEndPosition(int line) {
            return Lines.ByteToCharPosition(Call(SciMsg.SCI_GETLINEENDPOSITION, line));
        }

        /// <summary>
        /// Gets the current screen location of the caret.
        /// </summary>
        /// <returns><c>Point</c> representing the coordinates of the screen location.</returns>
        public static Point GetCaretScreenLocation() {
            var point = GetPointXyFromPosition(CurrentPosition);
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

        /// <summary>
        /// Move the caret and the view to the specified line (lines starts 0!)
        /// </summary>
        /// <param name="line"></param>
        public static void GoToLine(int line) {
            EnsureRangeVisible(line, line);
            var linesOnScreen = LinesOnScreen;
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GOTOLINE, Math.Max(line + linesOnScreen, 0), 0);
            SetFirstVisibleLine(Math.Max(line - 1, 0));
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_GOTOLINE, line, 0);
            GrabFocus();
        }

        /// <summary>
        /// Returns the length of the document in bytes.
        /// </summary>
        public static int GetDocumentByteLength() {
            return Call(SciMsg.SCI_GETLENGTH);
        }

        /// <summary>
        ///     Make a range visible by scrolling to the last line of the range.
        ///     A line may be hidden because more than one of its parent lines is contracted. Both these message travels up the
        ///     fold hierarchy, expanding any contracted folds until they reach the top level. The line will then be visible.
        /// </summary>
        public static void EnsureRangeVisible(int start, int end) {
            var lineStart = LineFromPosition(Math.Min(start, end));
            var lineEnd = LineFromPosition(Math.Max(start, end));
            for (var line = lineStart; line <= lineEnd; line++) {
                Call(SciMsg.SCI_ENSUREVISIBLE, line);
            }
        }

        /// <summary>
        /// This returns the number of lines in the document. An empty document contains 1 line. A document holding only an
        /// end of line sequence has 2 lines.
        /// </summary>
        public static int GetLineCount() {
            return Call(SciMsg.SCI_GETLINECOUNT);
        }

        /// <summary>
        /// This returns the position at the end of indentation of a line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static int GetLineIndentPosition(int line) {
            return Lines.ByteToCharPosition(Call(SciMsg.SCI_GETLINEINDENTPOSITION, line));
        }

        /// <summary>
        /// This message returns the position of a column on a line taking the width of tabs into account. 
        /// It treats a multi-byte character as a single column. Column numbers, like lines start at 0.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static int GetPosFromLineColumn(int line, int column) {
            return Lines.ByteToCharPosition(Call(SciMsg.SCI_FINDCOLUMN, line, column));
        }

        /// <summary>
        /// Adds an additional selection range to the existing main selection.
        /// </summary>
        /// <param name="caret">The zero-based document position to end the selection.</param>
        /// <param name="anchor">The zero-based document position to start the selection.</param>
        /// <remarks>A main selection must first have been set by a call to <see cref="SetSelection" />.</remarks>
        public static void AddSelection(int caret, int anchor) {
            var textLength = TextLength;
            caret = Clamp(caret, 0, textLength);
            anchor = Clamp(anchor, 0, textLength);
            caret = Lines.CharToBytePosition(caret);
            anchor = Lines.CharToBytePosition(anchor);
            Msg(SciMsg.SCI_ADDSELECTION, new IntPtr(caret), new IntPtr(anchor));
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
            Call(SciMsg.SCI_SETVIEWWS, (int)SciMsg.SCWS_VISIBLEALWAYS);
        }

        /// <summary>
        /// reverts the last change
        /// </summary>
        public static void Undo() {
            Call(SciMsg.SCI_UNDO);
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
            Call(SciMsg.SCI_INDICATORCLEARRANGE, 0, GetDocumentByteLength());
        }

        /// <summary>
        ///     Marks a range of text.
        /// </summary>
        public static void AddFindMark(int pos, int length) {
            Call(SciMsg.SCI_INDICATORFILLRANGE, pos, length);
        }

        #endregion

        #region Lexer stuff

        // Set style 
        private static int stylingPosition;
        private static int stylingBytePosition;

        /// <summary>
        /// Gets or sets the current lexer.
        /// </summary>
        /// <returns>One of the <see cref="Lexer" /> enumeration values. The default is <see cref="Container" />.</returns>
        public Lexer Lexer {
            get {
                return (Lexer)Msg(SciMsg.SCI_GETLEXER);
            }
            set {
                var lexer = (int)value;
                Msg(SciMsg.SCI_SETLEXER, new IntPtr(lexer));
            }
        }

        /// <summary>
        /// Gets or sets the current lexer by name.
        /// </summary>
        /// <returns>A String representing the current lexer.</returns>
        /// <remarks>Lexer names are case-sensitive.</remarks>
        public static unsafe string LexerLanguage {
            get {
                var length = Msg(SciMsg.SCI_GETLEXERLANGUAGE).ToInt32();
                if (length == 0)
                    return string.Empty;

                var bytes = new byte[length + 1];
                fixed (byte* bp = bytes) {
                    Msg(SciMsg.SCI_GETLEXERLANGUAGE, IntPtr.Zero, new IntPtr(bp));
                    return GetString(new IntPtr(bp), length, Encoding.ASCII);
                }
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    Msg(SciMsg.SCI_SETLEXERLANGUAGE, IntPtr.Zero, IntPtr.Zero);
                } else {
                    var bytes = GetBytes(value, Encoding.ASCII, true);
                    fixed (byte* bp = bytes)
                        Msg(SciMsg.SCI_SETLEXERLANGUAGE, IntPtr.Zero, new IntPtr(bp));
                }
            }
        }

        /// <summary>
        /// Requests that the current lexer restyle the specified range.
        /// </summary>
        /// <param name="startPos">The zero-based document position at which to start styling.</param>
        /// <param name="endPos">The zero-based document position at which to stop styling (exclusive).</param>
        /// <remarks>This will also cause fold levels in the range specified to be reset.</remarks>
        public static void Colorize(int startPos, int endPos) {
            var textLength = TextLength;
            startPos = Clamp(startPos, 0, textLength);
            endPos = Clamp(endPos, 0, textLength);
            startPos = Lines.CharToBytePosition(startPos);
            endPos = Lines.CharToBytePosition(endPos);
            Msg(SciMsg.SCI_COLOURISE, new IntPtr(startPos), new IntPtr(endPos));
        }

        /// <summary>
        /// Styles the specified length of characters.
        /// </summary>
        /// <param name="length">The number of characters to style.</param>
        /// <param name="style">The <see cref="Style" /> definition index to assign each character.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length" /> or <paramref name="style" /> is less than zero. -or-
        /// The sum of a preceeding call to <see cref="StartStyling" /> or <see name="SetStyling" /> and <paramref name="length" /> is greater than the document length. -or-
        /// <paramref name="style" /> is greater than or equal to the number of style definitions.
        /// </exception>
        /// <remarks>
        /// The styling position is advanced by <paramref name="length" /> after each call allowing multiple
        /// calls to <see cref="SetStyling" /> for a single call to <see cref="StartStyling" />.
        /// </remarks>
        /// <seealso cref="StartStyling" />
        public static void SetStyling(int length, int style) {
            var endPos = stylingPosition + length;
            var endBytePos = Lines.CharToBytePosition(endPos);
            Msg(SciMsg.SCI_SETSTYLING, new IntPtr(endBytePos - stylingBytePosition), new IntPtr(style));

            // Track this for the next call
            stylingPosition = endPos;
            stylingBytePosition = endBytePos;
        }

        /// <summary>
        /// Prepares for styling by setting the styling <paramref name="position" /> to start at.
        /// </summary>
        /// <param name="position">The zero-based character position in the document to start styling.</param>
        /// <remarks>
        /// After preparing the document for styling, use successive calls to <see cref="SetStyling" />
        /// to style the document.
        /// </remarks>
        /// <seealso cref="SetStyling" />
        public static void StartStyling(int position) {
            position = Clamp(position, 0, TextLength);
            var pos = Lines.CharToBytePosition(position);
            Msg(SciMsg.SCI_STARTSTYLING, new IntPtr(pos));

            // Track this so we can validate calls to SetStyling
            stylingPosition = position;
            stylingBytePosition = pos;
        }

        /// <summary>
        /// Returns the last document position likely to be styled correctly.
        /// </summary>
        /// <returns>The zero-based document position of the last styled character.</returns>
        public static int GetEndStyled() {
            var pos = Msg(SciMsg.SCI_GETENDSTYLED).ToInt32();
            return Lines.ByteToCharPosition(pos);
        }

        /// <summary>
        /// Gets the style of the specified document position.
        /// </summary>
        /// <param name="position">The zero-based document position of the character to get the style for.</param>
        /// <returns>The zero-based <see cref="Style" /> index used at the specified <paramref name="position" />.</returns>
        public static int GetStyleAt(int position) {
            position = Clamp(position, 0, TextLength);
            position = Lines.CharToBytePosition(position);
            return Msg(SciMsg.SCI_GETSTYLEAT, new IntPtr(position)).ToInt32();
        }


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
        /// Position of the starting line we need to style
        /// </summary>
        /// <returns></returns>
        public static int GetSylingNeededStartPos() {
            return PositionFromLine(LineFromPosition(GetEndStyled()));
        }

        /// <summary>
        /// Style the text between startPos and endPos with the styleId
        /// </summary>
        /// <param name="styleId"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        public static void StyleText(byte styleId, int startPos, int endPos) {
            Call(SciMsg.SCI_STARTSTYLING, startPos, 0);
            Call(SciMsg.SCI_SETSTYLING, endPos - startPos, styleId);
        }

        /// <summary>
        /// set the style of a text from startPos to startPos + styleArray.Length,
        /// the styleArray is a array of bytes, each byte is the style number to the corresponding text byte
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="styleArray"></param>
        public static void StyleTextEx(int startPos, byte[] styleArray) {
            Call(SciMsg.SCI_STARTSTYLING, startPos, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETSTYLINGEX, styleArray.Length, styleArray);
        }


        #endregion

        #region set styles

        /* NOTABLE STYLES :
         * in enum SciSpecialStyles
         * STYLE_DEFAULT	32	This style defines the attributes that all styles receive when the SCI_STYLECLEARALL message is used.
         * STYLE_LINENUMBER	33	This style sets the attributes of the text used to display line numbers in a line number margin. 
         * Thebackground  colour  set  for this style also sets the background colour for all margins that do not have any folding maskbits set. 
         * Thatis  any   *margin for which    mask &  SC_MASK_FOLDERS is 0. See SCI_SETMARGINMASKN for more about masks.
         * STYLE_BRACELIGHT	34	This style sets the attributes used when highlighting braces with the SCI_BRACEHIGHLIGHT message and  
         * whenhighlighting  the  corresponding indentation with SCI_SETHIGHLIGHTGUIDE.
         * STYLE_BRACEBAD	35	This style sets the display attributes used when marking an unmatched brace with the SCI_BRACEBADLIGHT message.
         * STYLE_CONTROLCHAR	36	This style sets the font used when drawing control characters. Only the font, size, bold, italics,andcharacter 
         * set attributes are used and not the colour attributes. See also: SCI_SETCONTROLCHARSYMBOL.
         * STYLE_INDENTGUIDE	37	This style sets the foreground and background colours used when drawing the indentation guides.
         * */

        /// <summary>
        /// Removes all styling from the document and resets the folding state.
        /// </summary>
        public static void ClearDocumentStyle() {
            Msg(SciMsg.SCI_CLEARDOCUMENTSTYLE);
        }

        /// <summary>
        /// Sets a global override to the selection background color.
        /// </summary>
        /// <param name="use">true to override the selection background color; otherwise, false.</param>
        /// <param name="color">The global selection background color.</param>
        /// <seealso cref="SetSelectionForeColor" />
        public static void SetSelectionBackColor(bool use, Color color) {
            var colour = ColorTranslator.ToWin32(color);
            var useSelectionForeColour = (use ? new IntPtr(1) : IntPtr.Zero);

            Msg(SciMsg.SCI_SETSELBACK, useSelectionForeColour, new IntPtr(colour));
        }

        /// <summary>
        /// Sets a global override to the selection foreground color.
        /// </summary>
        /// <param name="use">true to override the selection foreground color; otherwise, false.</param>
        /// <param name="color">The global selection foreground color.</param>
        /// <seealso cref="SetSelectionBackColor" />
        public static void SetSelectionForeColor(bool use, Color color) {
            var colour = ColorTranslator.ToWin32(color);
            var useSelectionForeColour = (use ? new IntPtr(1) : IntPtr.Zero);

            Msg(SciMsg.SCI_SETSELFORE, useSelectionForeColour, new IntPtr(colour));
        }


        /// <summary>
        /// Resets all style properties to those currently configured for the <see cref="Style.Default" /> style.
        /// </summary>
        /// <seealso cref="StyleResetDefault" />
        public static void StyleClearAll() {
            Msg(SciMsg.SCI_STYLECLEARALL);
        }

        /// <summary>
        /// Resets the <see cref="Style.Default" /> style to its initial state.
        /// </summary>
        /// <seealso cref="StyleClearAll" />
        public static void StyleResetDefault() {
            Msg(SciMsg.SCI_STYLERESETDEFAULT);
        }


        /// <summary>
        /// Defines a style
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bg"></param>
        /// <param name="fg"></param>
        public static void SetStyle(byte id, Color bg, Color fg) {
            Call(SciMsg.SCI_STYLESETBACK, id, (int)(new Colorref(bg)).ColorDWORD);
            Call(SciMsg.SCI_STYLESETFORE, id, (int)(new Colorref(fg)).ColorDWORD);
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
        /// Sets the font for a particular style
        /// </summary>
        /// <param name="styleId"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSizeInPoints"></param>
        public static void SetStyleFont(byte styleId, string fontName, int fontSizeInPoints) {
            Call(SciMsg.SCI_STYLESETSIZE, styleId, fontSizeInPoints);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_STYLESETFONT, styleId, fontName);
        }

        /// <summary>
        /// Sets the font to bold on/off for given style
        /// </summary>
        /// <param name="styleIdn"></param>
        /// <param name="state"></param>
        public static void SetStyleFontBold(byte styleIdn, bool state) {
            Call(SciMsg.SCI_STYLESETBOLD, styleIdn, state ? 1 : 0);
        }

        /// <summary>
        /// Sets the font to italic on/off for given style
        /// </summary>
        /// <param name="styleIdn"></param>
        /// <param name="state"></param>
        public static void SetStyleFontItalic(byte styleIdn, bool state) {
            Call(SciMsg.SCI_STYLESETITALIC, styleIdn, state ? 1 : 0);
        }

        /// <summary>
        /// Sets the font to underline on/off for given style
        /// </summary>
        /// <param name="styleIdn"></param>
        /// <param name="state"></param>
        public static void SetStyleFontUnderline(byte styleIdn, bool state) {
            Call(SciMsg.SCI_STYLESETUNDERLINE, styleIdn, state ? 1 : 0);
        }

        /// <summary>
        /// The value of caseMode determines how text is displayed. 
        /// You can set upper case (SC_CASE_UPPER, 1) or lower case (SC_CASE_LOWER, 2) or camel case (SC_CASE_CAMEL, 3) 
        /// or display normally (SC_CASE_MIXED, 0). This does not change the stored text, only how it is displayed.
        /// </summary>
        /// <param name="styleIdn"></param>
        /// <param name="caseMode"></param>
        public static void SetStyleFontCase(byte styleIdn, int caseMode) {
            Call(SciMsg.SCI_STYLESETCASE, styleIdn, caseMode);
        }

        /// <summary>
        /// sets the fore/background color of the whitespaces, overriding the lexer's
        /// </summary>
        /// <param name="bg"></param>
        /// <param name="fg"></param>
        public static void SetWhiteSpaceStyle(Color bg, Color fg) {
            Call(SciMsg.SCI_SETWHITESPACEFORE, 1, (int)(new Colorref(fg)).ColorDWORD);
            Call(SciMsg.SCI_SETWHITESPACEBACK, 1, (int)(new Colorref(bg)).ColorDWORD);
        }

        /// <summary>
        /// Set style for selection
        /// </summary>
        /// <param name="bgColor"></param>
        /// <param name="fgColor"></param>
        public static void SetSelectionStyle(Color bgColor, Color fgColor) {
            Call(SciMsg.SCI_SETSELBACK, 1, (int)(new Colorref(bgColor)).ColorDWORD);
            Call(SciMsg.SCI_SETSELFORE, 1, (int)(new Colorref(fgColor)).ColorDWORD);
        }

        /// <summary>
        /// Set caret style
        /// </summary>
        /// <param name="fgColor"></param>
        public static void SetCaretStyle(Color fgColor) {
            Call(SciMsg.SCI_SETCARETFORE, (int)(new Colorref(fgColor)).ColorDWORD);
        }

        /// <summary>
        /// You can choose to make the background colour of the line containing the caret different with these messages. 
        /// To do this, set the desired background colour with SCI_SETCARETLINEBACK, then use SCI_SETCARETLINEVISIBLE(true) to enable the effect. 
        /// You can cancel the effect with SCI_SETCARETLINEVISIBLE(false). 
        /// This form of background colouring has highest priority when a line has markers that would otherwise change the background colour. 
        /// The caret line may also be drawn translucently which allows other background colours to show through. 
        /// This is done by setting the alpha (translucency) value by calling SCI_SETCARETLINEBACKALPHA. When the alpha is not 
        /// SC_ALPHA_NOALPHA (256), the caret line is drawn after all other features so will affect the colour of all other features. 
        /// Alpha goes from 0 (transparent) to 256 (opaque)
        /// </summary>
        /// <param name="bgColor"></param>
        /// <param name="alpha"></param>
        public static void SetCaretLineStyle(Color bgColor, int alpha) {
            Call(SciMsg.SCI_GETCARETLINEBACK, (int)(new Colorref(bgColor)).ColorDWORD);
            Call(SciMsg.SCI_SETCARETLINEVISIBLE, 1);
            Call(SciMsg.SCI_SETCARETLINEBACKALPHA, alpha);
        }

        /// <summary>
        /// Sets a style for a marker
        /// SciMsg.SC_MARK_FULLRECT
        /// </summary>
        /// <param name="marker"></param>
        /// <param name="bgColor"></param>
        /// <param name="fgColor"></param>
        /// <param name="markerStyle"></param>
        public static void SetMarkerStyle(byte marker, Color bgColor, Color fgColor, SciMarkerStyle markerStyle) {
            Call(SciMsg.SCI_MARKERDEFINE, marker, (int)markerStyle);
            Call(SciMsg.SCI_MARKERSETFORE, marker, (int)(new Colorref(fgColor)).ColorDWORD);
            Call(SciMsg.SCI_MARKERSETBACK, marker, (int)(new Colorref(bgColor)).ColorDWORD);
        }

        /// <summary>
        /// Gets the font size of given style
        /// </summary>
        /// <param name="styleId"></param>
        /// <returns></returns>
        public static int GetFontSize(byte styleId) {
            return Call(SciMsg.SCI_STYLEGETSIZE, styleId);
        }

        /// <summary>
        /// allow changing the colour of the fold margin and fold margin highlight
        /// </summary>
        /// <param name="color"></param>
        /// <param name="highColor"></param>
        public static void SetFoldMarginStyle(Color color, Color highColor) {
            Call(SciMsg.SCI_SETFOLDMARGINCOLOUR, 1, (int)(new Colorref(color)).ColorDWORD);
            Call(SciMsg.SCI_SETFOLDMARGINHICOLOUR, 1, (int)(new Colorref(highColor)).ColorDWORD);
        }

        /// <summary>
        /// While the cursor hovers over text in a style with the hotspot attribute set, the default colouring can be modified 
        /// and an underline drawn with these settings
        /// Single line mode stops a hotspot from wrapping onto next line
        /// </summary>
        /// <param name="fg"></param>
        /// <param name="bg"></param>
        /// <param name="underline"></param>
        /// <param name="singleLine"></param>
        public static void SetHotSpotStyle(Color fg, Color bg, bool underline, bool singleLine) {
            Call(SciMsg.SCI_SETHOTSPOTACTIVEFORE, 1, (int)(new Colorref(fg)).ColorDWORD);
            Call(SciMsg.SCI_SETHOTSPOTACTIVEBACK, 1, (int)(new Colorref(bg)).ColorDWORD);
            Call(SciMsg.SCI_SETHOTSPOTACTIVEUNDERLINE, underline ? 1 : 0);
            Call(SciMsg.SCI_GETHOTSPOTSINGLELINE, singleLine ? 1 : 0);
        }

        /// <summary>
        /// Sets the background color of additional selections.
        /// </summary>
        /// <param name="color">Additional selections background color.</param>
        /// <remarks>Calling <see cref="SetSelectionBackColor" /> will reset the <paramref name="color" /> specified.</remarks>
        public static void SetAdditionalSelBack(Color color) {
            var colour = ColorTranslator.ToWin32(color);
            Msg(SciMsg.SCI_SETADDITIONALSELBACK, new IntPtr(colour));
        }

        /// <summary>
        /// Sets the foreground color of additional selections.
        /// </summary>
        /// <param name="color">Additional selections foreground color.</param>
        /// <remarks>Calling <see cref="SetSelectionForeColor" /> will reset the <paramref name="color" /> specified.</remarks>
        public static void SetAdditionalSelFore(Color color) {
            var colour = ColorTranslator.ToWin32(color);
            Msg(SciMsg.SCI_SETADDITIONALSELFORE, new IntPtr(colour));
        }

        #endregion

        #region annotations

        /// <summary>
        /// set the style of a text from startPos to startPos + styleArray.Length,
        /// the styleArray is a array of bytes, each byte is the style number to the corresponding text byte    
        /// Example :
        /// Npp.SetAnnotationText(2, "aaaaa\nbbbbb");
        /// Npp.SetAnnotationStyles(2, new[] { (byte)250, (byte)250, (byte)250, (byte)250, (byte)250, (byte)250, (byte)253, (byte)253, (byte)253, (byte)253, (byte)253 });
        /// </summary>
        /// <param name="line"></param>
        /// <param name="styleArray"></param>
        public static void SetAnnotationStyles(int line, byte[] styleArray) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_ANNOTATIONSETSTYLES, line, styleArray);
        }

        /// <summary>
        /// Set a style for an annotation
        /// </summary>
        /// <param name="line"></param>
        /// <param name="style"></param>
        public static void SetAnnotationStyle(int line, byte style) {
            Call(SciMsg.SCI_ANNOTATIONSETSTYLE, line, style);
        }

        /// <summary>
        /// Sets the text of an annotation for a given line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="message"></param>
        public static void AddAnnotation(int line, string message) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_ANNOTATIONSETTEXT, line, (IsUtf8() ? message.Utf8ToAnsi() : message));
        }

        /// <summary>
        /// Delete annotation on given line
        /// </summary>
        /// <param name="line"></param>
        public static void DeleteAnnotation(int line) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_ANNOTATIONSETTEXT, line, (string)null);
        }

        /// <summary>
        /// Clear all annotations in one go
        /// </summary>
        public static void DeleteAllAnnotations() {
            Call(SciMsg.SCI_ANNOTATIONCLEARALL);
        }

        /// <summary>
        /// Sets a mode of display for the annotations
        /// ANNOTATION_HIDDEN	0	Annotations are not displayed.
        /// ANNOTATION_STANDARD	1	Annotations are drawn left justified with no adornment.
        /// ANNOTATION_BOXED	2	Annotations are indented to match the text and are surrounded by a box.
        /// ANNOTATION_INDENTED	3	Annotations are indented to match the text.
        /// </summary>
        /// <param name="mode"></param>
        public static void SetAnnotationVisible(int mode = 3) {
            Call(SciMsg.SCI_ANNOTATIONSETVISIBLE, mode);
        }

        /// <summary>
        /// Returns the mode of display for annotation (see SetAnnotationVisible)
        /// </summary>
        /// <returns></returns>
        public static int GetAnnotationVisible() {
            return Call(SciMsg.SCI_ANNOTATIONGETVISIBLE);
        }

        #endregion

        #region Margin and Marker

        /// <summary>
        /// for the displayType, you can use the predefined constants SC_MARGIN_SYMBOL (0) and SC_MARGIN_NUMBER (1) to set 
        /// a margin as either a line number or a symbol margin. A margin with application defined text may use 
        /// SC_MARGIN_TEXT (4) or SC_MARGIN_RTEXT (5) to right justify the text
        /// </summary>
        /// <param name="marginNumber"></param>
        /// <param name="displayType"></param>
        /// <param name="width"></param>
        /// <param name="sensitive"></param>
        public static void SetMargin(int marginNumber, SciMarginType displayType, int width, int sensitive) {
            Call(SciMsg.SCI_SETMARGINTYPEN, marginNumber, (int)displayType);
            Call(SciMsg.SCI_SETMARGINSENSITIVEN, marginNumber, sensitive);
            Call(SciMsg.SCI_SETMARGINWIDTHN, marginNumber, width);
        }

        /// <summary>
        /// Get a margin width
        /// </summary>
        /// <param name="marginNumber"></param>
        /// <returns></returns>
        public static int GetMarginWidth(int marginNumber) {
            return Call(SciMsg.SCI_GETMARGINWIDTHN, marginNumber);
        }

        public static int GetMarginSentivity(int marginNumber) {
            return Call(SciMsg.SCI_GETMARGINSENSITIVEN, marginNumber);
        }

        /// <summary>
        /// Add a single marker on a single line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="markerNumber"></param>
        public static void AddMarker(int line, int markerNumber) {
            Call(SciMsg.SCI_MARKERADD, line, markerNumber);
        }

        /// <summary>
        /// Delete a single marker on a single line
        /// set markerNumber to -1 to delete all markers on a line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="markerNumber"></param>
        public static void DeleteMarker(int line, int markerNumber) {
            Call(SciMsg.SCI_MARKERDELETE, line, markerNumber);
        }

        /// <summary>
        /// Delete all the markers of number markerNumber
        /// </summary>
        /// <param name="markerNumber"></param>
        public static void DeleteAllMarker(int markerNumber) {
            Call(SciMsg.SCI_MARKERDELETE, markerNumber);
        }

        /// <summary>
        /// The markers that can be displayed in each margin are set with SCI_SETMARGINMASKN
        /// Any markers not associated with a visible margin will be displayed as changes in background colour in the text
        /// If you want marker number 5 and 3 to be displayed in margin 4 : SetMarginMask(4, 2^5 + 2^3)
        /// </summary>
        /// <param name="marginNumber"></param>
        /// <param name="markerNumberMask"></param>
        public static void SetMarginMask(int marginNumber, int markerNumberMask) {
            Call(SciMsg.SCI_SETMARGINMASKN, marginNumber, markerNumberMask);
        }

        /// <summary>
        /// This returns a 32-bit integer that indicates which markers were present on the line
        /// Bit 0 is set if marker 0 is present, bit 1 for marker 1 and so on
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static int GetMarkers(int line) {
            return Call(SciMsg.SCI_MARKERGET, line);
        }

        /// <summary>
        /// Search the next line with the given marker, return -1 if not found
        /// The markerMask argument should have one bit set for each marker you wish to find
        /// Set bit 0 to find marker 0, bit 1 for marker 1 and so on
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="markerMask"></param>
        /// <returns></returns>
        public static int GetNextMarkerLine(int lineStart, int markerMask) {
            return Call(SciMsg.SCI_MARKERNEXT, lineStart, markerMask);
        }

        /// <summary>
        /// Search the previous line with the given marker, return -1 if not found
        /// The markerMask argument should have one bit set for each marker you wish to find
        /// Set bit 0 to find marker 0, bit 1 for marker 1 and so on
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="markerMask"></param>
        /// <returns></returns>
        public static int GetPreviousMarkerLine(int lineStart, int markerMask) {
            return Call(SciMsg.SCI_MARKERPREVIOUS, lineStart, markerMask);
        }

        #endregion

        #region Indicators

        /// <summary>
        /// Gets or sets the indicator used in a subsequent call to <see cref="IndicatorFillRange" /> or <see cref="IndicatorClearRange" />.
        /// </summary>
        /// <returns>The zero-based indicator index to apply when calling <see cref="IndicatorFillRange" /> or remove when calling <see cref="IndicatorClearRange" />.</returns>
        public static int IndicatorCurrent {
            get {
                return Msg(SciMsg.SCI_GETINDICATORCURRENT).ToInt32();
            }
            set {
                value = Clamp(value, 0, 31);
                Msg(SciMsg.SCI_SETINDICATORCURRENT, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the user-defined value used in a subsequent call to <see cref="IndicatorFillRange" />.
        /// </summary>
        /// <returns>The indicator value to apply when calling <see cref="IndicatorFillRange" />.</returns>
        public static int IndicatorValue {
            get {
                return Msg(SciMsg.SCI_GETINDICATORVALUE).ToInt32();
            }
            set {
                Msg(SciMsg.SCI_SETINDICATORVALUE, new IntPtr(value));
            }
        }

        /// <summary>
        /// Returns a bitmap representing the 32 indicators in use at the specified position.
        /// </summary>
        /// <param name="position">The zero-based character position within the document to test.</param>
        /// <returns>A bitmap indicating which of the 32 indicators are in use at the specified <paramref name="position" />.</returns>
        public uint IndicatorAllOnFor(int position) {
            position = Clamp(position, 0, TextLength);
            position = Lines.CharToBytePosition(position);

            var bitmap = Msg(SciMsg.SCI_INDICATORALLONFOR, new IntPtr(position)).ToInt32();
            return unchecked((uint)bitmap);
        }

        /// <summary>
        /// Removes the <see cref="IndicatorCurrent" /> indicator (and user-defined value) from the specified range of text.
        /// </summary>
        /// <param name="position">The zero-based character position within the document to start clearing.</param>
        /// <param name="length">The number of characters to clear.</param>
        public static void IndicatorClearRange(int position, int length) {
            var textLength = TextLength;
            position = Clamp(position, 0, textLength);
            length = Clamp(length, 0, textLength - position);

            var startPos = Lines.CharToBytePosition(position);
            var endPos = Lines.CharToBytePosition(position + length);

            Msg(SciMsg.SCI_INDICATORCLEARRANGE, new IntPtr(startPos), new IntPtr(endPos - startPos));
        }

        /// <summary>
        /// Adds the <see cref="IndicatorCurrent" /> indicator and <see cref="IndicatorValue" /> value to the specified range of text.
        /// </summary>
        /// <param name="position">The zero-based character position within the document to start filling.</param>
        /// <param name="length">The number of characters to fill.</param>
        public static void IndicatorFillRange(int position, int length) {
            var textLength = TextLength;
            position = Clamp(position, 0, textLength);
            length = Clamp(length, 0, textLength - position);

            var startPos = Lines.CharToBytePosition(position);
            var endPos = Lines.CharToBytePosition(position + length);

            Msg(SciMsg.SCI_INDICATORFILLRANGE, new IntPtr(startPos), new IntPtr(endPos - startPos));
        }

        /// <summary>
        /// Sets indicator style,
        /// Range of indicator id to use is from 8=INDIC_CONTAINER .. to 31=INDIC_IME-1
        /// Alpha is only useful for indicator type : INDIC_ROUNDBOX and INDIC_STRAIGHTBOX
        /// and should range from 0 (invisible) to 255 (opaque)
        /// </summary>
        /// <param name="indicatorId"></param>
        /// <param name="sciIndicatorType"></param>
        /// <param name="fg"></param>
        /// <param name="alpha"></param>
        public static void SetIndicatorStyle(byte indicatorId, SciIndicatorType sciIndicatorType, Color fg, byte alpha) {
            Call(SciMsg.SCI_INDICSETSTYLE, indicatorId, (int)sciIndicatorType);
            Call(SciMsg.SCI_INDICSETFORE, indicatorId, (int)(new Colorref(fg)).ColorDWORD);
            Call(SciMsg.SCI_INDICSETALPHA, indicatorId, alpha);
            Call(SciMsg.SCI_INDICSETOUTLINEALPHA, indicatorId, alpha);
        }

        /// <summary>
        /// Sets the indicator style when it is hovered by mouse or when the caret is in it
        /// Range of indicator id to use is from 8=INDIC_CONTAINER .. to 31=INDIC_IME-1
        /// </summary>
        /// <param name="indicatorId"></param>
        /// <param name="sciIndicatorType"></param>
        /// <param name="fg"></param>
        public static void SetIndicatorHoverStyle(byte indicatorId, SciIndicatorType sciIndicatorType, Color fg) {
            Call(SciMsg.SCI_INDICSETHOVERSTYLE, indicatorId, (int)sciIndicatorType);
            Call(SciMsg.SCI_INDICSETHOVERFORE, indicatorId, (int)(new Colorref(fg)).ColorDWORD);
        }

        /// <summary>
        /// Delete indicatorId at given position
        /// </summary>
        /// <param name="indicatorId"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        public static void DeleteIndicator(int indicatorId, int startPos, int endPos) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETINDICATORCURRENT, indicatorId, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATORCLEARRANGE, startPos, endPos - startPos);
        }

        /// <summary>
        /// Adds an indicator at given position
        /// </summary>
        /// <param name="indicatorId"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        public static void AddIndicator(int indicatorId, int startPos, int endPos) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_SETINDICATORCURRENT, indicatorId, 0);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATORFILLRANGE, startPos, endPos - startPos);
        }

        /// <summary>
        /// returns true if given indicator is present at given position
        /// </summary>
        /// <param name="indicatorId"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static bool IndicatorPresentAt(int indicatorId, int pos) {
            return (Call(SciMsg.SCI_INDICATORVALUEAT, indicatorId, pos) == 1);
        }

        /// <summary>
        /// Find the start of an indicator range from a position
        /// </summary>
        /// <param name="indicatorId"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int GetIndicatorStart(int indicatorId, int pos) {
            return Call(SciMsg.SCI_INDICATORSTART, indicatorId, pos);
        }

        /// <summary>
        /// Find the end of an indicator range from a position
        /// </summary>
        /// <param name="indicatorId"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int GetIndicatorEnd(int indicatorId, int pos) {
            return Call(SciMsg.SCI_INDICATOREND, indicatorId, pos);
        }

        /// <summary>
        /// List of points(start, end) that represents the range were the given indicator
        /// has been found
        /// </summary>
        /// <param name="indicator"></param>
        /// <returns></returns>
        public static Point[] FindIndicatorRanges(int indicator) {
            var ranges = new List<Point>();
            var testPosition = 0;
            while (true) {
                var rangeStart = (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATORSTART, indicator, testPosition);
                var rangeEnd = (int)Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICATOREND, indicator, testPosition);
                if (IndicatorPresentAt(indicator, testPosition))
                    ranges.Add(new Point(rangeStart, rangeEnd));
                if (testPosition == rangeEnd)
                    break;
                testPosition = rangeEnd;
            }
            return ranges.ToArray();
        }

        public static void SetIndicatorStyle(int indicator, SciMsg style, Color color) {
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICSETSTYLE, indicator, (int)style);
            Win32.SendMessage(HandleScintilla, SciMsg.SCI_INDICSETFORE, indicator, ColorTranslator.ToWin32(color));
        }

        #endregion

        #region helper

        internal static Encoding Encoding {
            get {
                // Should always be UTF-8 unless someone has done an end run around us
                int codePage = (int)Msg(SciMsg.SCI_GETCODEPAGE);
                return (codePage == 0 ? Encoding.Default : Encoding.GetEncoding(codePage));
            }
        }

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
            return (int)Win32.SendMessage(HandleScintilla, msg, wParam, lParam);
        }

        private static int Call(SciMsg msg, int wParam, string lParam) {
            return (int)Win32.SendMessage(HandleScintilla, msg, wParam, lParam);
        }

        private static int Call(SciMsg msg, int wParam, StringBuilder lParam) {
            return (int)Win32.SendMessage(HandleScintilla, msg, wParam, lParam);
        }

        private static int Call(SciMsg msg, int wParam, int lParam) {
            return (int)Win32.SendMessage(HandleScintilla, msg, wParam, lParam);
        }

        private static int Call(SciMsg msg, int wParam) {
            return Call(msg, wParam, 0);
        }

        private static int Call(SciMsg msg) {
            return Call(msg, 0, 0);
        }


        public static IntPtr Msg(SciMsg msg) {
            return Msg(msg, IntPtr.Zero, IntPtr.Zero);
        }

        public static IntPtr Msg(SciMsg msg, IntPtr wParam) {
            return Msg(msg, wParam, IntPtr.Zero);
        }

        public static IntPtr Msg(SciMsg msg, IntPtr wParam, IntPtr lParam) {
            // If the control handle, ptr, direct function, etc... hasn't been created yet, it will be now.
            var result = Msg(HandleScintilla, msg, wParam, lParam);
            return result;
        }

        public static IntPtr Msg(IntPtr sciPtr, SciMsg msg, IntPtr wParam, IntPtr lParam) {
            // Like Win32 SendMessage but directly to Scintilla
            var result = Win32.SendMessage(sciPtr, (int)msg, wParam, lParam);
            return result;
        }

        public static IntPtr Msg(int msg) {
            return Msg(msg, IntPtr.Zero, IntPtr.Zero);
        }

        public static IntPtr Msg(int msg, IntPtr wParam) {
            return Msg(msg, wParam, IntPtr.Zero);
        }

        public static IntPtr Msg(int msg, IntPtr wParam, IntPtr lParam) {
            // If the control handle, ptr, direct function, etc... hasn't been created yet, it will be now.
            var result = Msg(HandleScintilla, msg, wParam, lParam);
            return result;
        }

        public static IntPtr Msg(IntPtr sciPtr, int msg, IntPtr wParam, IntPtr lParam) {
            // Like Win32 SendMessage but directly to Scintilla
            var result = Win32.SendMessage(sciPtr, msg, wParam, lParam);
            return result;
        }

        public static unsafe byte[] GetBytes(string text, Encoding encoding, bool zeroTerminated) {
            if (string.IsNullOrEmpty(text))
                return new byte[0];

            int count = encoding.GetByteCount(text);
            byte[] buffer = new byte[count + (zeroTerminated ? 1 : 0)];

            fixed (byte* bp = buffer)
            fixed (char* ch = text) {
                encoding.GetBytes(ch, text.Length, bp, count);
            }

            if (zeroTerminated)
                buffer[buffer.Length - 1] = 0;

            return buffer;
        }

        public static unsafe byte[] GetBytes(char[] text, int length, Encoding encoding, bool zeroTerminated) {
            fixed (char* cp = text) {
                var count = encoding.GetByteCount(cp, length);
                var buffer = new byte[count + (zeroTerminated ? 1 : 0)];
                fixed (byte* bp = buffer)
                    encoding.GetBytes(cp, length, bp, buffer.Length);

                if (zeroTerminated)
                    buffer[buffer.Length - 1] = 0;

                return buffer;
            }
        }

        public static unsafe string GetString(IntPtr bytes, int length, Encoding encoding) {
            var ptr = (sbyte*)bytes;
            var str = new string(ptr, 0, length, encoding);

            return str;
        }

        public static int Clamp(int value, int min, int max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static int ClampMin(int value, int min) {
            if (value < min)
                return min;

            return value;
        }

        #endregion
    }

    #region Ansi to UTF8 extensions

    public static class StringExtension {
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

    #endregion

}