#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Sci.cs) is part of 3P.
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
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.WindowsCore;

namespace _3PA.NppCore {
    /// <summary>
    /// This class should be used to control the instances of scintilla in notepad++<br />
    /// - Npp uses 2 instances of scintilla, a main and a secondary (one for each view)<br />
    /// - This static class sends all its messages to the current scintilla handle (Npp.CurrentSci)<br />
    /// - For every scintilla message that involves a position, the expect position (expected by scintilla) is the<br />
    /// f***** BYTE position, not the character position (as anyone would assume at first!). This messes up with all your<br />
    /// function<br />
    /// calls when the document is encoded in UTF8 (for example), as character can be encoded on 2 bytes... Every methods<br />
    /// defined in this class handle the position as CHAR position and convert what's needed for scintilla to cooperate<br />
    /// So you can this class safely without having headaches ;)<br />
    /// - This class also uses the direct function call to scintilla, as described in scintilla's documention. It allows<br />
    /// faster execution than with SendMessage<br />
    /// It stays static also for performance reasons
    /// </summary>
    internal static partial class Sci {

        #region const

        public const int KeywordMaxLength = 60;

        #endregion

        #region Critical Core

        /// <summary>
        /// Instance of scintilla, the class that allows communication with the current scintilla
        /// </summary>
        private static SciApi Api {
            get { return Npp.CurrentSci; }
        }

        #endregion

        #region Class accessors

        /// <summary>
        /// Is scintilla currently focused?
        /// </summary>
        public static bool IsScintillaFocused {
            get { return WinApi.GetForegroundWindow() == Api.Handle; }
        }

        /// <summary>
        /// Returns a Line object representing the given line
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Line GetLine(int index) {
            return new Line(index);
        }

        /// <summary>
        /// Returns a Line object representing the current line
        /// </summary>
        /// <returns></returns>
        public static Line GetLine() {
            return new Line();
        }

        /// <summary>
        /// Returns a selection object
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Selection GetSelection(int index) {
            return new Selection(index);
        }

        /// <summary>
        /// Returns a marker object
        /// There are 32 markers, numbered 0 to MARKER_MAX (31), and you can assign any combination of them to each line in the
        /// document
        /// Marker numbers 25 to 31 are used by Scintilla in folding margins
        /// Marker numbers 0 to 24 have no pre-defined function; you can use them
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Marker GetMarker(int index) {
            return new Marker(index);
        }

        /// <summary>
        /// Returns a margin object
        /// The margins are numbered 0 to 4. Using a margin number outside the valid range has no effect. By default, margin 0
        /// is set to display line
        /// numbers, but is given a width of 0, so it is hidden. Margin 1 is set to display non-folding symbols and is given a
        /// width of 16 pixels,
        /// so it is visible. Margin 2 is set to display the folding symbols, but is given a width of 0, so it is hidden. Of
        /// course,
        /// you can set the margins to be whatever you wish.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Margin GetMargin(int index) {
            return new Margin(index);
        }

        /// <summary>
        /// Returns an indicator object
        /// Range of indicator id to use is from 8=INDIC_CONTAINER .. to 31=INDIC_IME-1
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Indicator GetIndicator(int index) {
            return new Indicator(index);
        }

        /// <summary>
        /// Returns a style object
        /// There are 256 lexer styles that can be set, numbered 0 to STYLE_MAX (255). There are also some predefined numbered
        /// styles starting at 32.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Style GetStyle(byte index) {
            return new Style(index);
        }

        #endregion

        #region Misc

        /// <summary>
        /// Undo previous action
        /// </summary>
        public static void Undo() {
            Api.Send(SciMsg.SCI_UNDO);
        }

        /// <summary>
        /// Mark the beginning of a set of operations that you want to undo all as one operation but that you have to generate
        /// as several operations. Alternatively, you can use these to mark a set of operations that you do not want to have
        /// combined with the preceding or following operations if they are undone.
        /// </summary>
        public static void BeginUndoAction() {
            Api.Send(SciMsg.SCI_BEGINUNDOACTION);
        }

        /// <summary>
        /// Mark the end of a set of operations that you want to undo all as one operation but that you have to generate
        /// as several operations. Alternatively, you can use these to mark a set of operations that you do not want to have
        /// combined with the preceding or following operations if they are undone.
        /// </summary>
        public static void EndUndoAction() {
            Api.Send(SciMsg.SCI_ENDUNDOACTION);
        }

        /// <summary>
        /// Specifies the characters that will automatically cancel autocompletion without the need to call AutoCCancel.
        /// </summary>
        /// <param name="chars">A String of the characters that will cancel autocompletion. The default is empty.</param>
        /// <remarks>Characters specified should be limited to printable ASCII characters.</remarks>
        public static void AutoCStops(string chars) {
            Win32Api.SendMessage(Api.Handle, SciMsg.SCI_AUTOCSTOPS, 0, chars);
        }

        /// <summary>
        /// returns a rectangle representing the location and size of the scintilla window
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetScintillaRectangle() {
            return WinApi.GetWindowRect(Api.Handle);
        }

        /// <summary>
        /// allows scintilla to grab focus
        /// </summary>
        /// <returns></returns>
        public static void GrabFocus() {
            Api.Send(SciMsg.SCI_GRABFOCUS);
        }

        /// <summary>
        /// to be tested!!!!
        /// </summary>
        /// <returns></returns>
        public static bool GetFocus() {
            return Api.Send(SciMsg.SCI_GETFOCUS).IsTrue();
        }

        /// <summary>
        /// Cancels any displayed autocompletion list.
        /// </summary>
        public static void AutoCCancel() {
            Api.Send(SciMsg.SCI_AUTOCCANCEL);
        }

        /// <summary>
        /// to be tested!!!!
        /// </summary>
        /// <returns></returns>
        public static bool AutoCActive() {
            return Api.Send(SciMsg.SCI_AUTOCACTIVE).IsTrue();
        }

        /// <summary>
        /// Performs the specified command
        /// </summary>
        /// <param name="sciCommand">The command to perform.</param>
        public static void ExecuteCmd(Command sciCommand) {
            Api.Send((SciMsg) sciCommand);
        }

        /// <summary>
        /// Changes all end-of-line characters in the document to the format specified.
        /// </summary>
        /// <param name="eolMode">One of the Eol enumeration values.</param>
        public static void ConvertEols(Eol eolMode) {
            var eol = (int) eolMode;
            Api.Send(SciMsg.SCI_CONVERTEOLS, new IntPtr(eol));
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// </summary>
        public static void Copy() {
            Api.Send(SciMsg.SCI_COPY);
        }

        /// <summary>
        /// Pastes the contents of the clipboard into the current selection.
        /// </summary>
        public static void Paste() {
            Api.Send(SciMsg.SCI_PASTE);
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// If the selection is empty the current line is copied.
        /// </summary>
        /// <remarks>
        /// If the selection is empty and the current line copied, an extra "MSDEVLineSelect" marker is added to the
        /// clipboard which is then used in Paste to paste the whole line before the current line.
        /// </remarks>
        public static void CopyAllowLine() {
            Api.Send(SciMsg.SCI_COPYALLOWLINE);
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
            start = Api.Lines.CharToBytePosition(start);
            end = Api.Lines.CharToBytePosition(end);

            Api.Send(SciMsg.SCI_COPYRANGE, new IntPtr(start), new IntPtr(end));
        }

        /// <summary>
        /// Cuts the selected text from the document and places it on the clipboard.
        /// </summary>
        public static void Cut() {
            Api.Send(SciMsg.SCI_CUT);
        }

        /// <summary>
        /// Returns the zero-based document line index from the specified display line index.
        /// </summary>
        /// <param name="displayLine">The zero-based display line index.</param>
        /// <returns>The zero-based document line index.</returns>
        public static int DocLineFromVisible(int displayLine) {
            displayLine = Clamp(displayLine, 0, Api.Lines.Count);
            return Api.Send(SciMsg.SCI_DOCLINEFROMVISIBLE, new IntPtr(displayLine)).ToInt32();
        }

        /// <summary>
        /// Measures the width in pixels of the specified string when rendered in the specified style.
        /// </summary>
        /// <param name="style">The index of the Style to use when rendering the text to measure.</param>
        /// <param name="text">The text to measure.</param>
        /// <returns>The width in pixels.</returns>
        public static unsafe int TextWidth(int style, string text) {
            style = Clamp(style, 0, 255);
            var bytes = GetBytes(text ?? String.Empty, Encoding, true);
            fixed (byte* bp = bytes)
                return Api.Send(SciMsg.SCI_TEXTWIDTH, new IntPtr(style), new IntPtr(bp)).ToInt32();
        }

        /// <summary>
        /// Retrieve the height of a particular line of text in pixels.
        /// </summary>
        public static int TextHeight(int line) {
            return (int) Api.Send(SciMsg.SCI_TEXTHEIGHT, new IntPtr(line));
        }

        /// <summary>
        /// Gets or sets whether vertical scrolling ends at the last line or can scroll past.
        /// </summary>
        /// <returns>true if the maximum vertical scroll position ends at the last line; otherwise, false. The default is true.</returns>
        public static bool EndAtLastLine {
            get { return Api.Send(SciMsg.SCI_GETENDATLASTLINE).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETENDATLASTLINE, value.ToPointer()); }
        }

        /// <summary>
        /// Gets or sets the end-of-line mode, or rather, the characters added into
        /// the document when the user presses the Enter key.
        /// </summary>
        /// <returns>One of the Eol enumeration values. The default is Eol.CrLf.</returns>
        public static Eol EolMode {
            get { return (Eol) Api.Send(SciMsg.SCI_GETEOLMODE); }
            set { Api.Send(SciMsg.SCI_SETEOLMODE, new IntPtr((int) value)); }
        }

        /// <summary>
        /// Returns a string representing the current eol used
        /// </summary>
        public static string GetEolString {
            get {
                switch (EolMode) {
                    case Eol.Cr:
                        return "\r";
                    case Eol.CrLf:
                        return "\r\n";
                    default:
                        return "\n";
                }
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
            get { return Api.Send(SciMsg.SCI_LINESONSCREEN).ToInt32(); }
        }

        /// <summary>
        /// Gets a value indicating whether the document has been modified (is dirty)
        /// since the last call to SetSavePoint.
        /// </summary>
        /// <returns>true if the document has been modified; otherwise, false.</returns>
        public static bool GetModify {
            get { return Api.Send(SciMsg.SCI_GETMODIFY).IsTrue(); }
        }

        /// <summary>
        /// Passes the specified property name-value pair to the current <see cref="Lexer" />.
        /// </summary>
        /// <param name="name">The property name to set.</param>
        /// <param name="value">
        /// The property value. Values can refer to other property names using the syntax $(name), where 'name' is another property
        /// name for the current <see cref="Lexer" />. When the property value is retrieved by a call to <see cref="GetPropertyExpanded" />
        /// the embedded property name macro will be replaced (expanded) with that current property value.
        /// </param>
        /// <remarks>Property names are case-sensitive.</remarks>
        public static unsafe void SetProperty(string name, string value) {
            if (string.IsNullOrEmpty(name))
                return;

            var nameBytes = GetBytes(name, Encoding.ASCII, true);
            var valueBytes = GetBytes(value ?? string.Empty, Encoding.ASCII, true);

            fixed (byte* nb = nameBytes)
            fixed (byte* vb = valueBytes) {
                Api.Send(SciMsg.SCI_SETPROPERTY, new IntPtr(nb), new IntPtr(vb));
            }
        }

        /// <summary>
        /// Gets or sets the automatic folding flags.
        /// </summary>
        /// <returns>
        /// A bitwise combination of the AutomaticFold enumeration.
        /// </returns>
        public static AutomaticFold AutomaticFold {
            get {
                return (AutomaticFold)Api.Send(SciMsg.SCI_GETAUTOMATICFOLD);
            }
            set {
                var automaticFold = (int)value;
                Api.Send(SciMsg.SCI_SETAUTOMATICFOLD, new IntPtr(automaticFold));
            }
        }

        /// <summary>
        /// Gets or sets the ability to switch to rectangular selection mode while making a selection with the mouse.
        /// </summary>
        /// <returns>
        /// true if the current mouse selection can be switched to a rectangular selection by pressing the ALT key; otherwise,
        /// false.
        /// The default is false.
        /// </returns>
        public static bool MouseSelectionRectangularSwitch {
            get { return Api.Send(SciMsg.SCI_GETMOUSESELECTIONRECTANGULARSWITCH).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETMOUSESELECTIONRECTANGULARSWITCH, value.ToPointer()); }
        }

        /// <summary>
        /// These messages set and get an event mask that determines which document change events are notified to the container
        /// with SCN_MODIFIED
        /// and SCEN_CHANGE. For example, a container may decide to see only notifications about changes to text and not
        /// styling changes
        /// by calling SCI_SETMODEVENTMASK(SC_MOD_INSERTTEXT|SC_MOD_DELETETEXT).
        /// The possible notification types are of the type SciModificationMod
        /// </summary>
        public static int EventMask {
            get { return Api.Send(SciMsg.SCI_GETMODEVENTMASK).ToInt32(); }
            set { Api.Send(SciMsg.SCI_SETMODEVENTMASK, new IntPtr(value)); }
        }

        /// <summary>
        /// Gets or sets whether multiple selection is enabled.
        /// </summary>
        /// <returns>
        /// true if multiple selections can be made by holding the CTRL key and dragging the mouse; otherwise, false.
        /// The default is false.
        /// </returns>
        public static bool MultipleSelection {
            get { return Api.Send(SciMsg.SCI_GETMULTIPLESELECTION).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETMULTIPLESELECTION, value.ToPointer()); }
        }

        /// <summary>
        /// Gets or sets the behavior when pasting text into multiple selections.
        /// </summary>
        /// <returns>One of the Interop.MultiPaste enumeration values. The default is Interop.MultiPaste.Once.</returns>
        public static MultiPaste MultiPaste {
            get { return (MultiPaste) Api.Send(SciMsg.SCI_GETMULTIPASTE); }
            set { Api.Send(SciMsg.SCI_SETMULTIPASTE, new IntPtr((int) value)); }
        }

        /// <summary>
        /// Gets or sets whether to write over text rather than insert it.
        /// </summary>
        /// <return>true to write over text; otherwise, false. The default is false.</return>
        public static bool Overtype {
            get { return Api.Send(SciMsg.SCI_GETOVERTYPE).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETOVERTYPE, value.ToPointer()); }
        }

        /// <summary>
        /// Gets or sets whether line endings in pasted text are convereted to the document EolMode.
        /// </summary>
        /// <returns>true to convert line endings in pasted text; otherwise, false. The default is true.</returns>
        public static bool PasteConvertEndings {
            get { return Api.Send(SciMsg.SCI_GETPASTECONVERTENDINGS).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETPASTECONVERTENDINGS, value.ToPointer()); }
        }

        /// <summary>
        /// Gets or sets whether the document is read-only.
        /// </summary>
        /// <returns>true if the document is read-only; otherwise, false. The default is false.</returns>
        public static bool ReadOnly {
            get { return Api.Send(SciMsg.SCI_GETREADONLY).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETREADONLY, value.ToPointer()); }
        }

        /// <summary>
        /// Gets or sets the mouse dwell time
        /// The time the mouse must sit still, in milliseconds, to generate a SCN_DWELLSTART notification.
        /// If set to SC_TIME_FOREVER, the default, no dwell events are generated.
        /// </summary>
        public static int MouseDwellTime {
            get { return Api.Send(SciMsg.SCI_GETMOUSEDWELLTIME).ToInt32(); }
            set { Api.Send(SciMsg.SCI_SETMOUSEDWELLTIME, new IntPtr(value)); }
        }

        /// <summary>
        /// Gets or sets how to display whitespace characters.
        /// </summary>
        /// <returns>One of the WhitespaceMode enumeration values. The default is WhitespaceMode.Invisible.</returns>
        public static WhitespaceMode ViewWhitespace {
            get { return (WhitespaceMode) Api.Send(SciMsg.SCI_GETVIEWWS); }
            set { Api.Send(SciMsg.SCI_SETVIEWWS, new IntPtr((int) value)); }
        }

        /// <summary>
        /// Gets or sets the visibility of end-of-line characters.
        /// </summary>
        /// <returns>true to display end-of-line characters; otherwise, false. The default is false.</returns>
        public static bool ViewEol {
            get { return Api.Send(SciMsg.SCI_GETVIEWEOL).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETVIEWEOL, value.ToPointer()); }
        }

        /// <summary>
        /// Gets or sets the zoom factor.
        /// </summary>
        /// <returns>The zoom factor measured in points.</returns>
        /// <remarks>For best results, values should range from -10 to 20 points.</remarks>
        public static int Zoom {
            get { return Api.Send(SciMsg.SCI_GETZOOM).ToInt32(); }
            set { Api.Send(SciMsg.SCI_SETZOOM, new IntPtr(value)); }
        }

        /// <summary>
        /// Increases the zoom factor by 1 until it reaches 20 points.
        /// </summary>
        public static void ZoomIn() {
            Api.Send(SciMsg.SCI_ZOOMIN);
        }

        /// <summary>
        /// Decreases the zoom factor by 1 until it reaches -10 points.
        /// </summary>
        public static void ZoomOut() {
            Api.Send(SciMsg.SCI_ZOOMOUT);
        }

        /// <summary>
        /// Gets or sets font quality (anti-aliasing method) used to render fonts.
        /// </summary>
        /// <returns>
        /// One of the Interop.FontQuality enumeration values.
        /// The default is Interop.FontQuality.Default.
        /// </returns>
        public static FontQuality FontQuality {
            get { return (FontQuality) Api.Send(SciMsg.SCI_GETFONTQUALITY); }
            set { Api.Send(SciMsg.SCI_SETFONTQUALITY, new IntPtr((int) value)); }
        }

        /// <summary>
        /// Gets or sets the line wrapping mode.
        /// </summary>
        /// <returns>
        /// One of the WrapMode enumeration values.
        /// The default is WrapMode.None.
        /// </returns>
        public static WrapMode WrapMode {
            get { return (WrapMode) Api.Send(SciMsg.SCI_GETWRAPMODE); }
            set { Api.Send(SciMsg.SCI_SETWRAPMODE, new IntPtr((int) value)); }
        }

        /// <summary>
        /// Gets or sets the indented size in pixels of wrapped subApi.Lines.
        /// </summary>
        /// <returns>The indented size of wrapped sublines measured in pixels. The default is 0.</returns>
        /// <remarks>
        /// Setting WrapVisualFlags to Interop.WrapVisualFlags.Start will add an
        /// additional 1 pixel to the value specified.
        /// </remarks>
        public static int WrapStartIndent {
            get { return Api.Send(SciMsg.SCI_GETWRAPSTARTINDENT).ToInt32(); }
            set {
                value = ClampMin(value, 0);
                Api.Send(SciMsg.SCI_SETWRAPSTARTINDENT, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the line wrapping indent mode.
        /// </summary>
        /// <returns>
        /// One of the Interop.WrapIndentMode enumeration values.
        /// The default is Interop.WrapIndentMode.Fixed.
        /// </returns>
        public static WrapIndentMode WrapIndentMode {
            get { return (WrapIndentMode) Api.Send(SciMsg.SCI_GETWRAPINDENTMODE); }
            set { Api.Send(SciMsg.SCI_SETWRAPINDENTMODE, new IntPtr((int) value)); }
        }

        /// <summary>
        /// Gets or sets the wrap visual flags.
        /// </summary>
        /// <returns>
        /// A bitwise combination of the Interop.WrapVisualFlags enumeration.
        /// The default is Interop.WrapVisualFlags.None.
        /// </returns>
        public static WrapVisualFlags WrapVisualFlags {
            get { return (WrapVisualFlags) Api.Send(SciMsg.SCI_GETWRAPVISUALFLAGS); }
            set { Api.Send(SciMsg.SCI_SETWRAPVISUALFLAGS, new IntPtr((int) value)); }
        }

        /// <summary>
        /// Gets or sets additional location options when displaying wrap visual flags.
        /// </summary>
        /// <returns>
        /// One of the Interop.WrapVisualFlagLocation enumeration values.
        /// The default is Interop.WrapVisualFlagLocation.Default.
        /// </returns>
        public static WrapVisualFlagLocation WrapVisualFlagLocation {
            get { return (WrapVisualFlagLocation) Api.Send(SciMsg.SCI_GETWRAPVISUALFLAGSLOCATION); }
            set { Api.Send(SciMsg.SCI_SETWRAPVISUALFLAGSLOCATION, new IntPtr((int) value)); }
        }

        public static IntPtr StartRecordMacro() {
            return Api.Send(SciMsg.SCI_STARTRECORD);
        }

        #endregion

        #region folding and view

        /// <summary>
        /// Performs the specified fold action on the entire document.
        /// </summary>
        /// <param name="action">One of the FoldAction enumeration values.</param>
        /// <remarks>
        /// When using FoldAction.Toggle the first fold header in the document is examined to decide whether to expand or
        /// contract.
        /// </remarks>
        public static void FoldAll(FoldAction action) {
            Api.Send(SciMsg.SCI_FOLDALL, new IntPtr((int) action));
        }

        /// <summary>
        /// Hides the range of lines specified.
        /// </summary>
        /// <param name="lineStart">The zero-based index of the line range to start hiding.</param>
        /// <param name="lineEnd">The zero-based index of the line range to end hiding.</param>
        public static void HideLines(int lineStart, int lineEnd) {
            lineStart = Clamp(lineStart, 0, Api.Lines.Count);
            lineEnd = Clamp(lineEnd, lineStart, Api.Lines.Count);

            Api.Send(SciMsg.SCI_HIDELINES, new IntPtr(lineStart), new IntPtr(lineEnd));
        }

        /// <summary>
        /// Gets a value indicating whether all the document lines are visible (not hidden).
        /// </summary>
        /// <returns>true if all the lines are visible; otherwise, false.</returns>
        public static bool AllLinesVisible {
            get { return Api.Send(SciMsg.SCI_GETALLLINESVISIBLE).IsTrue(); }
        }

        /// <summary>
        /// Gets or sets the first visible line on screen.
        /// </summary>
        /// <returns>The zero-based index of the first visible screen line.</returns>
        /// <remarks>The value is a visible line, not a document line.</remarks>
        public static int FirstVisibleLine {
            get { return Api.Send(SciMsg.SCI_GETFIRSTVISIBLELINE).ToInt32(); }
            set {
                value = ClampMin(value, 0);
                Api.Send(SciMsg.SCI_SETFIRSTVISIBLELINE, new IntPtr(value));
            }
        }

        /// <summary>
        /// Shows the range of lines specified.
        /// </summary>
        /// <param name="lineStart">The zero-based index of the line range to start showing.</param>
        /// <param name="lineEnd">The zero-based index of the line range to end showing.</param>
        public static void ShowLines(int lineStart, int lineEnd) {
            lineStart = Clamp(lineStart, 0, Api.Lines.Count);
            lineEnd = Clamp(lineEnd, lineStart, Api.Lines.Count);

            Api.Send(SciMsg.SCI_SHOWLINES, new IntPtr(lineStart), new IntPtr(lineEnd));
        }

        #endregion

        #region indentation

        /// <summary>
        /// Gets or sets whether to use a mixture of tabs and spaces for indentation or purely spaces.
        /// </summary>
        /// <returns>true to use tab characters; otherwise, false. The default is true.</returns>
        public static bool UseTabs {
            get { return Api.Send(SciMsg.SCI_GETUSETABS).IsTrue(); }
            set { Win32Api.SendMessage(Api.Handle, SciMsg.SCI_SETUSETABS, value ? 1 : 0, 0); }
        }

        /// <summary>
        /// Gets or sets the width of a tab as a multiple of a space character.
        /// </summary>
        /// <returns>The width of a tab measured in characters. The default is 4.</returns>
        public static int TabWidth {
            get { return Api.Send(SciMsg.SCI_GETTABWIDTH).ToInt32(); }
            set {
                value = ClampMin(value, 0);
                Win32Api.SendMessage(Api.Handle, SciMsg.SCI_SETTABWIDTH, value, 0);
            }
        }

        /// <summary>
        /// Gets or sets the size of indentation in terms of space characters.
        /// </summary>
        /// <returns>The indentation size measured in characters. The default is 0.</returns>
        /// <remarks> A value of 0 will make the indent width the same as the tab width.</remarks>
        public static int IndentWidth {
            get { return Api.Send(SciMsg.SCI_GETINDENT).ToInt32(); }
            set {
                value = ClampMin(value, 0);
                Win32Api.SendMessage(Api.Handle, SciMsg.SCI_SETINDENT, value, 0);
            }
        }

        /// <summary>
        /// returns the indent value as a string, can be either a \t or a number of ' '
        /// </summary>
        /// <returns></returns>
        public static string GetIndentString() {
            return UseTabs ? "\t" : new string(' ', TabWidth);
        }

        #endregion

        #region Braces

        /// <summary>
        /// Styles the specified character position with the Style.BraceBad style when there is an unmatched brace.
        /// </summary>
        /// <param name="position">
        /// The zero-based document position of the unmatched brace character or InvalidPosition to remove
        /// the highlight.
        /// </param>
        public static void BraceBadLight(int position) {
            position = Clamp(position, -1, TextLength);
            if (position > 0)
                position = Api.Lines.CharToBytePosition(position);

            Api.Send(SciMsg.SCI_BRACEBADLIGHT, new IntPtr(position));
        }

        /// <summary>
        /// Styles the specified character positions with the Style.BraceLight style.
        /// </summary>
        /// <param name="position1">The zero-based document position of the open brace character.</param>
        /// <param name="position2">The zero-based document position of the close brace character.</param>
        /// <remarks>
        /// Brace highlighting can be removed by specifying InvalidPosition for <paramref name="position1" /> and
        /// <paramref name="position2" />.
        /// </remarks>
        public static void BraceHighlight(int position1, int position2) {
            var textLength = TextLength;

            position1 = Clamp(position1, -1, textLength);
            if (position1 > 0)
                position1 = Api.Lines.CharToBytePosition(position1);

            position2 = Clamp(position2, -1, textLength);
            if (position2 > 0)
                position2 = Api.Lines.CharToBytePosition(position2);

            Api.Send(SciMsg.SCI_BRACEHIGHLIGHT, new IntPtr(position1), new IntPtr(position2));
        }

        /// <summary>
        /// Finds a corresponding matching brace starting at the position specified.
        /// The brace characters handled are '(', ')', '[', ']', '{', '}', '&lt;', and '&gt;'.
        /// </summary>
        /// <param name="position">
        /// The zero-based document position of a brace character to start the search from for a matching
        /// brace character.
        /// </param>
        /// <returns>
        /// The zero-based document position of the corresponding matching brace or InvalidPosition it no matching brace
        /// could be found.
        /// </returns>
        /// <remarks>
        /// A match only occurs if the style of the matching brace is the same as the starting brace. Nested braces are
        /// handled correctly.
        /// </remarks>
        public static int BraceMatch(int position) {
            position = Clamp(position, 0, TextLength);
            position = Api.Lines.CharToBytePosition(position);

            var match = Api.Send(SciMsg.SCI_BRACEMATCH, new IntPtr(position), IntPtr.Zero).ToInt32();
            if (match > 0)
                match = Api.Lines.ByteToCharPosition(match);

            return match;
        }

        #endregion

        #region mouse and screen

        /// <summary>
        /// Gets the current screen location of the caret.
        /// </summary>
        /// <returns><c>Point</c> representing the coordinates of the screen location.</returns>
        public static Point GetCaretScreenLocation(int pos = -1) {
            if (pos < 0)
                pos = CurrentPosition;
            var point = GetPointXyFromPosition(pos);
            Win32Api.ClientToScreen(Api.Handle, ref point);
            return point;
        }

        /// <summary>
        /// returns the x,y point location of the character at the position given
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point GetPointXyFromPosition(int position) {
            var pos = Api.Lines.CharToBytePosition(position);
            var x = (int) Api.Send(SciMsg.SCI_POINTXFROMPOSITION, IntPtr.Zero, new IntPtr(pos));
            var y = (int) Api.Send(SciMsg.SCI_POINTYFROMPOSITION, IntPtr.Zero, new IntPtr(pos));
            return new Point(x, y);
        }

        /// <summary>
        /// Finds the closest character position to the specified display point or returns -1
        /// if the point is outside the window or not close to any characters.
        /// </summary>
        /// <param name="x">The x pixel coordinate within the client rectangle of the control.</param>
        /// <param name="y">The y pixel coordinate within the client rectangle of the control.</param>
        /// <returns>
        /// The zero-based document position of the nearest character to the point specified when near a character;
        /// otherwise, -1.
        /// </returns>
        public static int CharPositionFromPointClose(int x, int y) {
            var pos = Api.Send(SciMsg.SCI_CHARPOSITIONFROMPOINTCLOSE, new IntPtr(x), new IntPtr(y)).ToInt32();
            if (pos > -1)
                pos = Api.Lines.ByteToCharPosition(pos);
            return pos;
        }

        /// <summary>
        /// Self explaining
        /// </summary>
        /// <returns></returns>
        public static int GetPositionFromMouseLocation() {
            var point = Cursor.Position;
            Win32Api.ScreenToClient(Api.Handle, ref point);
            return CharPositionFromPointClose(point.X, point.Y);
        }

        /// <summary>
        /// Returns the X display pixel location of the specified document position.
        /// </summary>
        /// <param name="pos">The zero-based document character position.</param>
        /// <returns>The x-coordinate of the specified <paramref name="pos" /> within the client rectangle of the control.</returns>
        public static int PointXFromPosition(int pos) {
            pos = Clamp(pos, 0, TextLength);
            pos = Api.Lines.CharToBytePosition(pos);
            return Api.Send(SciMsg.SCI_POINTXFROMPOSITION, IntPtr.Zero, new IntPtr(pos)).ToInt32();
        }

        /// <summary>
        /// Returns the Y display pixel location of the specified document position.
        /// </summary>
        /// <param name="pos">The zero-based document character position.</param>
        /// <returns>The y-coordinate of the specified <paramref name="pos" /> within the client rectangle of the control.</returns>
        public static int PointYFromPosition(int pos) {
            pos = Clamp(pos, 0, TextLength);
            pos = Api.Lines.CharToBytePosition(pos);
            return Api.Send(SciMsg.SCI_POINTYFROMPOSITION, IntPtr.Zero, new IntPtr(pos)).ToInt32();
        }

        #endregion

        #region text

        /// <summary>
        /// Gets or sets the current document text in the Sci control.
        /// </summary>
        /// <returns>The text displayed in the control.</returns>
        /// <remarks>Depending on the length of text get or set, this operation can be expensive.</remarks>
        public static unsafe string Text {
            get {
                var length = Api.Send(SciMsg.SCI_GETTEXTLENGTH).ToInt32();
                var ptr = Api.Send(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(0), new IntPtr(length));
                if (ptr == IntPtr.Zero)
                    return String.Empty;

                // Assumption is that moving the gap will always be equal to or less expensive
                // than using one of the APIs which requires an intermediate buffer.
                var text = new string((sbyte*) ptr, 0, length, Encoding);
                return text;
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    Api.Send(SciMsg.SCI_CLEARALL);
                } else {
                    fixed (byte* bp = GetBytes(value, Encoding, true))
                        Api.Send(SciMsg.SCI_SETTEXT, IntPtr.Zero, new IntPtr(bp));
                }
            }
        }

        /// <summary>
        /// Deletes all document text, unless the document is read-only.
        /// </summary>
        public static void ClearAll() {
            Api.Send(SciMsg.SCI_CLEARALL);
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <returns>The selected text if there is any; otherwise, an empty string.</returns>
        public static unsafe string SelectedText {
            get {
                // NOTE: For some reason the length returned by this API includes the terminating NULL
                var length = Api.Send(SciMsg.SCI_GETSELTEXT).ToInt32() - 1;
                if (length <= 0)
                    return String.Empty;

                var bytes = new byte[length + 1];
                fixed (byte* bp = bytes) {
                    Api.Send(SciMsg.SCI_GETSELTEXT, IntPtr.Zero, new IntPtr(bp));
                    return GetString(new IntPtr(bp), length, Encoding);
                }
            }
        }

        /// <summary>
        /// Replaces the current selection with the specified text.
        /// </summary>
        /// <param name="text">The text that should replace the current selection.</param>
        /// <remarks>
        /// If there is not a current selection, the text will be inserted at the current caret position.
        /// Following the operation the caret is placed at the end of the inserted text and scrolled into view.
        /// Does nothing if string is null or empty?
        /// </remarks>
        public static unsafe void ReplaceSelection(string text) {
            fixed (byte* bp = GetBytes(text ?? String.Empty, Encoding, true))
                Api.Send(SciMsg.SCI_REPLACESEL, IntPtr.Zero, new IntPtr(bp));
        }

        /// <summary>
        /// Removes the selected text from the document.
        /// </summary>
        public static void Clear() {
            Api.Send(SciMsg.SCI_CLEAR);
        }

        /// <summary>
        /// Returns a part of the text of the current document; you specify how much length max (in bytes)
        /// you want to get and it returns the text. The text is retrieved around the first visible line,
        /// by default we try maxLenght/2 before and after and it will adjust
        /// </summary>
        public static string GetTextAroundFirstVisibleLine(int maxLenght) {
            var docLength = Api.Send(SciMsg.SCI_GETTEXTLENGTH).ToInt32();
            int start = 0;
            int lgt = docLength;
            if (docLength > maxLenght) {
                var firstVisPos = Api.Send(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(Api.Send(SciMsg.SCI_GETFIRSTVISIBLELINE).ToInt32())).ToInt32();
                start = (firstVisPos - maxLenght / 2).Clamp(0, firstVisPos);
                if (start + maxLenght > docLength)
                    start = (docLength - maxLenght).Clamp(0, firstVisPos);
                lgt = maxLenght;
            }
            var ptr = Api.Send(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(start), new IntPtr(lgt));
            if (ptr == IntPtr.Zero)
                return String.Empty;
            return GetString(ptr, lgt, Encoding);
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
            var byteStartPos = Api.Lines.CharToBytePosition(position);
            var byteEndPos = Api.Lines.CharToBytePosition(position + length);

            var ptr = Api.Send(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(byteStartPos), new IntPtr(byteEndPos - byteStartPos));
            if (ptr == IntPtr.Zero)
                return String.Empty;

            return GetString(ptr, byteEndPos - byteStartPos, Encoding);
        }

        /// <summary>
        /// Gets a range of text from the document.
        /// </summary>
        /// <param name="start">The zero-based starting character position of the range to get.</param>
        /// <param name="end">The zero-based ending character position of the range to get.</param>
        /// <returns>A string representing the text range.</returns>
        public static string GetTextByRange(int start, int end) {
            return GetTextRange(start, end - start);
        }

        /// <summary>
        /// Inserts text at the specified position.
        /// </summary>
        /// <param name="position">
        /// The zero-based character position to insert the text. Specify -1 to use the current caret
        /// position.
        /// </param>
        /// <param name="text">The text to insert into the document.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position" /> less than zero and not equal to -1. -or-
        /// <paramref name="position" /> is greater than the document length.
        /// </exception>
        /// <remarks>No scrolling is performed.</remarks>
        public static unsafe void InsertText(int position, string text) {
            if (position < -1)
                position = -1;
            var textLength = TextLength;
            if (position > textLength)
                position = textLength;
            position = Api.Lines.CharToBytePosition(position);
            fixed (byte* bp = GetBytes(text ?? String.Empty, Encoding, true))
                Api.Send(SciMsg.SCI_INSERTTEXT, new IntPtr(position), new IntPtr(bp));
        }

        /// <summary>
        /// Replaces the target defined by TargetStart and TargetEnd with the specified <paramref name="text" />.
        /// </summary>
        /// <param name="text">The text that will replace the current target.</param>
        /// <returns>The length of the replaced text.</returns>
        /// <remarks>
        /// The TargetStart and TargetEnd properties will be updated to the start and end positions of the replaced text.
        /// The recommended way to delete text in the document is to set the target range to be removed and replace the target
        /// with an empty string.
        /// </remarks>
        public static unsafe int ReplaceTarget(string text) {
            if (text == null)
                text = String.Empty;
            var bytes = GetBytes(text, Encoding, false);
            fixed (byte* bp = bytes)
                Api.Send(SciMsg.SCI_REPLACETARGET, new IntPtr(bytes.Length), new IntPtr(bp));
            return text.Length;
        }

        /// <summary>
        /// Replaces the target text defined by TargetStart and TargetEnd with the specified value after first substituting
        /// "\1" through "\9" macros in the <paramref name="text" /> with the most recent regular expression capture groups.
        /// </summary>
        /// <param name="text">
        /// The text containing "\n" macros that will be substituted with the most recent regular expression
        /// capture groups and then replace the current target.
        /// </param>
        /// <returns>The length of the replaced text.</returns>
        /// <remarks>
        /// The "\0" macro will be substituted by the entire matched text from the most recent search.
        /// The TargetStart and TargetEnd properties will be updated to the start and end positions of the replaced text.
        /// </remarks>
        public static unsafe int ReplaceTargetRe(string text) {
            var bytes = GetBytes(text ?? String.Empty, Encoding, false);
            fixed (byte* bp = bytes)
                Api.Send(SciMsg.SCI_REPLACETARGETRE, new IntPtr(bytes.Length), new IntPtr(bp));

            return Math.Abs(TargetEnd - TargetStart);
        }

        /// <summary>
        /// Deletes the given range of text
        /// </summary>
        public static void DeleteTextByRange(int start, int end) {
            SetTextByRange(start, end, null);
        }

        /// <summary>
        /// Sets the text of a specific range, can and must be used to delete text from range
        /// </summary>
        public static void SetTextByRange(int start, int end, string text) {
            if (end < start) {
                var buff = start;
                start = end;
                end = buff;
            }
            SetTargetRange(start, end);

            // I have no explanation for this crap, but when replacing one char, the ReplaceTarget function goes wild...
            // so we delete the existing text first and then insert the new one...
            if (Math.Abs(start - end) == 1)
                ReplaceTarget(null);
            ReplaceTarget(text);
        }

        /// <summary>
        /// Use this method to modify the text around the caret, it's good because :<br></br>
        /// - it's wrapped around beginundo/endundo which allows the user to CTRL+Z all the actions as one<br></br>
        /// - it handles the modification around ALL the carets -> good for multiselection
        /// returns the new position of the main caret
        /// </summary>
        /// <param name="offsetStart">offset relative to the current caret position</param>
        /// <param name="offsetEnd">offset relative to the current caret position</param>
        /// <param name="text"></param>
        public static int ModifyTextAroundCaret(int offsetStart, int offsetEnd, string text) {
            BeginUndoAction();
            int newCaretPos = 0;
            // for each selection
            for (var i = 0; i < Selection.Count; i++) {
                var selection = GetSelection(i);
                var curPos = selection.Caret;
                SetTextByRange(curPos + offsetStart, curPos + offsetEnd, text);
                // reposition carret
                curPos = curPos - (offsetEnd - offsetStart) + text.Length;
                selection.SetPosition(curPos);
                if (i == 0)
                    newCaretPos = curPos;
            }
            EndUndoAction();
            return newCaretPos;
        }

        /// <summary>
        /// returns the text on the left of the position... it will always return empty string at minima
        /// </summary>
        /// <param name="curPos"></param>
        /// <param name="maxLenght"></param>
        /// <returns></returns>
        public static string GetTextOnLeftOfPos(int curPos, int maxLenght = KeywordMaxLength) {
            var startPos = curPos - maxLenght;
            startPos = startPos > 0 ? startPos : 0;
            return curPos - startPos > 0 ? GetTextByRange(startPos, curPos) : string.Empty;
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
            endPos = endPos < fullLength ? endPos : fullLength;
            return endPos - curPos > 0 ? GetTextByRange(curPos, endPos) : string.Empty;
        }

        /// <summary>
        /// Replaces the left part of the keyword found at (CurrentPosition + offset) by
        /// the keyword given
        /// (all wrapped in an undo action + handles multiselection) 
        /// returns the new position of the caret
        /// </summary>
        public static int ReplaceWordWrapped(string keyword, HashSet<char> additionalWordChar, int offset) {
            return ModifyTextAroundCaret(offset - GetWord(additionalWordChar, CurrentPosition + offset).Length, offset, keyword);
        }

        /// <summary>
        /// Reads a a word, either starting from the end (readRightToLeft = true) of the start of the input string 
        /// stopAtPoint or not, if not, output the nbPoints found
        /// </summary>
        private static string ReadWord(string input, HashSet<char> additionalWordChar = null, bool readRightToLeft = true) {
            if (additionalWordChar == null)
                additionalWordChar = new HashSet<char> {'_'};
            var max = input.Length - 1;
            int count = 0;
            while (count <= max) {
                var pos = readRightToLeft ? max - count : count;
                var ch = input[pos];
                // normal word
                if (char.IsLetterOrDigit(ch) || additionalWordChar.Contains(ch))
                    count++;
                else
                    break;
            }
            return count == 0 ? string.Empty : input.Substring(readRightToLeft ? input.Length - count : 0, count);
        }

        /// <summary>
        /// Gets the keyword at given position (reading only on the left of the position)
        /// </summary>
        public static string GetWord(HashSet<char> additionalWordChar = null, int curPos = -1) {
            if (curPos < 0)
                curPos = CurrentPosition;
            return ReadWord(GetTextOnLeftOfPos(curPos), additionalWordChar);
        }

        /// <summary>
        /// Returns the ABL word at the given position (read on left and right)
        /// </summary>
        public static string GetWordAtPosition(int position, HashSet<char> additionalWordChar) {
            return ReadWord(GetTextOnLeftOfPos(position), additionalWordChar) + ReadWord(GetTextOnRightOfPos(position), additionalWordChar, false);
        }

        /// <summary>
        /// Returns the ABL word at the given position (read on left and right with different additional chars)
        /// </summary>
        public static string GetWordAtPosition(int position, HashSet<char> leftPartadditionalWordChar, HashSet<char> rightPartadditionalWordChar) {
            return ReadWord(GetTextOnLeftOfPos(position), leftPartadditionalWordChar) + ReadWord(GetTextOnRightOfPos(position), rightPartadditionalWordChar, false);
        }

        public static string GetTextBetween(Point point) {
            return GetTextByRange(point.X, point.Y);
        }

        /// <summary>
        /// Get document length (number of character!!)
        /// </summary>
        public static int TextLength {
            get { return Api.Lines.TextLength; }
        }

        #endregion

        #region search

        /// <summary>
        /// Sets the TargetStart and TargetEnd properties in a single call.
        /// </summary>
        /// <param name="start">The zero-based character position within the document to start a search or replace operation.</param>
        /// <param name="end">The zero-based character position within the document to end a search or replace operation.</param>
        public static void SetTargetRange(int start, int end) {
            var textLength = TextLength;
            start = Clamp(start, 0, textLength);
            end = Clamp(end, 0, textLength);
            start = Api.Lines.CharToBytePosition(start);
            end = Api.Lines.CharToBytePosition(end);
            Api.Send(SciMsg.SCI_SETTARGETRANGE, new IntPtr(start), new IntPtr(end));
        }

        /// <summary>
        /// Gets or sets the end position used when performing a search or replace.
        /// </summary>
        /// <returns>The zero-based character position within the document to end a search or replace operation.</returns>
        public static int TargetEnd {
            get {
                // The position can become stale and point to a place outside of the document so we must clamp it
                var bytePos = Clamp(Api.Send(SciMsg.SCI_GETTARGETEND).ToInt32(), 0, Api.Send(SciMsg.SCI_GETTEXTLENGTH).ToInt32());
                return Api.Lines.ByteToCharPosition(bytePos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                value = Api.Lines.CharToBytePosition(value);
                Api.Send(SciMsg.SCI_SETTARGETEND, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the start position used when performing a search or replace.
        /// </summary>
        /// <returns>The zero-based character position within the document to start a search or replace operation.</returns>
        public static int TargetStart {
            get {
                // The position can become stale and point to a place outside of the document so we must clamp it
                var bytePos = Clamp(Api.Send(SciMsg.SCI_GETTARGETSTART).ToInt32(), 0, Api.Send(SciMsg.SCI_GETTEXTLENGTH).ToInt32());
                return Api.Lines.ByteToCharPosition(bytePos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                value = Api.Lines.CharToBytePosition(value);
                Api.Send(SciMsg.SCI_SETTARGETSTART, new IntPtr(value));
            }
        }

        /// <summary>
        /// Sets the TargetStart and TargetEnd to the start and end positions of the selection.
        /// </summary>
        public static void TargetFromSelection() {
            Api.Send(SciMsg.SCI_TARGETFROMSELECTION);
        }

        /// <summary>
        /// Sets the TargetStart and TargetEnd to the start and end positions of the document.
        /// </summary>
        public static void TargetWholeDocument() {
            Api.Send(SciMsg.SCI_TARGETWHOLEDOCUMENT);
        }

        /// <summary>
        /// Gets or sets the search flags used when searching text.
        /// </summary>
        /// <returns>A bitwise combination of Interop.SearchFlags values. The default is Interop.SearchFlags.None.</returns>
        public static SearchFlags SearchFlags {
            get { return (SearchFlags) Api.Send(SciMsg.SCI_GETSEARCHFLAGS).ToInt32(); }
            set {
                var searchFlags = (int) value;
                Api.Send(SciMsg.SCI_SETSEARCHFLAGS, new IntPtr(searchFlags));
            }
        }

        /// <summary>
        /// Returns the capture group text of the most recent regular expression search.
        /// </summary>
        /// <param name="tagNumber">The capture group (1 through 9) to get the text for.</param>
        /// <returns>A String containing the capture group text if it participated in the match; otherwise, an empty string.</returns>
        public static unsafe string GetTag(int tagNumber) {
            tagNumber = Clamp(tagNumber, 1, 9);
            var length = Api.Send(SciMsg.SCI_GETTAG, new IntPtr(tagNumber), IntPtr.Zero).ToInt32();
            if (length <= 0)
                return String.Empty;

            var bytes = new byte[length + 1];
            fixed (byte* bp = bytes) {
                Api.Send(SciMsg.SCI_GETTAG, new IntPtr(tagNumber), new IntPtr(bp));
                return GetString(new IntPtr(bp), length, Encoding);
            }
        }

        /// <summary>
        /// Searches for the first occurrence of the specified text in the target defined by TargetStart and TargetEnd.
        /// </summary>
        /// <param name="text">
        /// The text to search for. The interpretation of the text (i.e. whether it is a regular expression) is
        /// defined by the SearchFlags property.
        /// </param>
        /// <returns>The zero-based start position of the matched text within the document if successful; otherwise, -1.</returns>
        /// <remarks>
        /// If successful, the TargetStart and TargetEnd properties will be updated to the start and end positions of the
        /// matched text.
        /// Searching can be performed in reverse using a TargetStart greater than the TargetEnd.
        /// </remarks>
        public static unsafe int SearchInTarget(string text) {
            int bytePos;
            var bytes = GetBytes(text ?? String.Empty, Encoding, false);
            fixed (byte* bp = bytes)
                bytePos = Api.Send(SciMsg.SCI_SEARCHINTARGET, new IntPtr(bytes.Length), new IntPtr(bp)).ToInt32();

            if (bytePos == -1)
                return bytePos;

            return Api.Lines.ByteToCharPosition(bytePos);
        }

        #endregion

        #region position

        /// <summary>
        /// Gets or sets the current caret position.
        /// </summary>
        /// <returns>The zero-based character position of the caret.</returns>
        /// <remarks>
        /// Setting the current caret position will create a selection between it and the current AnchorPosition.
        /// The caret is not scrolled into view.
        /// </remarks>
        public static int CurrentPosition {
            get {
                var bytePos = Api.Send(SciMsg.SCI_GETCURRENTPOS).ToInt32();
                return Api.Lines.ByteToCharPosition(bytePos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                var bytePos = Api.Lines.CharToBytePosition(value);
                Api.Send(SciMsg.SCI_SETCURRENTPOS, new IntPtr(bytePos));
            }
        }

        /// <summary>
        /// Returns the current line/column in the form of a point
        /// </summary>
        public static Point CurrentPoint {
            get {
                var curPos = CurrentPosition;
                return new Point(LineFromPosition(curPos), GetColumn(curPos));
            }
        }

        /// <summary>
        /// Gets or sets the current anchor position.
        /// </summary>
        /// <returns>The zero-based character position of the anchor.</returns>
        /// <remarks>
        /// Setting the current anchor position will create a selection between it and the CurrentPosition.
        /// The caret is not scrolled into view.
        /// </remarks>
        public static int AnchorPosition {
            get {
                var bytePos = Api.Send(SciMsg.SCI_GETANCHOR).ToInt32();
                return Api.Lines.ByteToCharPosition(bytePos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                var bytePos = Api.Lines.CharToBytePosition(value);
                Api.Send(SciMsg.SCI_SETANCHOR, new IntPtr(bytePos));
            }
        }

        /// <summary>
        /// Returns the column number of the specified document position, taking the width of tabs into account.
        /// </summary>
        /// <param name="position">The zero-based document position to get the column for.</param>
        /// <returns>The number of columns from the start of the line to the specified document <paramref name="position" />.</returns>
        public static int GetColumn(int position) {
            position = Clamp(position, 0, TextLength);
            position = Api.Lines.CharToBytePosition(position);
            return Api.Send(SciMsg.SCI_GETCOLUMN, new IntPtr(position)).ToInt32();
        }

        /// <summary>
        /// Returns the line that contains the document position specified.
        /// </summary>
        /// <param name="position">The zero-based document character position.</param>
        /// <returns>The zero-based document line index containing the character <paramref name="position" />.</returns>
        public static int LineFromPosition(int position) {
            position = Clamp(position, 0, TextLength);
            return Api.Lines.LineFromCharPosition(position);
        }

        /// <summary>
        /// This message returns the position of a column on a line taking the width of tabs into account.
        /// It treats a multi-byte character as a single column. Column numbers, like lines start at 0.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static int GetPosFromLineColumn(int line, int column) {
            return Api.Lines.ByteToCharPosition(Api.Send(SciMsg.SCI_FINDCOLUMN, new IntPtr(line), new IntPtr(column)).ToInt32());
        }

        /// <summary>
        /// Returns the !! BYTE !! position of the start of given line
        /// Don't use THIS unless you know what you are doing with it!!!
        /// </summary>
        public static int StartBytePosOfLine(int line) {
            return Api.Send(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(line)).ToInt32();
        }

        #endregion

        #region scroll

        /// <summary>
        /// Move the caret and the view to the specified line (lines starts 0!)
        /// </summary>
        /// <param name="line"></param>
        public static void GoToLine(int line) {
            GetLine(line).EnsureVisible();
            var linesOnScreen = LinesOnScreen;
            Api.Send(SciMsg.SCI_GOTOLINE, new IntPtr(Math.Max(line + linesOnScreen, 0)));
            FirstVisibleLine = Math.Max(line - 1, 0);
            Api.Send(SciMsg.SCI_GOTOLINE, new IntPtr(line));
            GrabFocus();
        }

        /// <summary>
        /// Navigates the caret to the document position specified.
        /// </summary>
        /// <param name="position">The zero-based document character position to navigate to.</param>
        /// <remarks>Any selection is discarded.</remarks>
        public static void GotoPosition(int position) {
            position = Clamp(position, 0, TextLength);
            position = Api.Lines.CharToBytePosition(position);
            Api.Send(SciMsg.SCI_GOTOPOS, new IntPtr(position));
        }

        /// <summary>
        /// Scrolls the current position into view, if it is not already visible.
        /// </summary>
        public static void ScrollCaret() {
            Api.Send(SciMsg.SCI_SCROLLCARET);
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
            start = Api.Lines.CharToBytePosition(start);
            end = Api.Lines.CharToBytePosition(end);

            Api.Send(SciMsg.SCI_SCROLLRANGE, new IntPtr(end), new IntPtr(start));
        }

        /// <summary>
        /// Scrolls the display the number of lines and columns specified.
        /// </summary>
        /// <param name="lines">The number of lines to scroll.</param>
        /// <param name="columns">The number of columns to scroll.</param>
        /// <remarks>
        /// Negative values scroll in the opposite direction.
        /// A column is the width in pixels of a space character in the Style.Default style.
        /// </remarks>
        public static void LineScroll(int lines, int columns) {
            Api.Send(SciMsg.SCI_LINESCROLL, new IntPtr(columns), new IntPtr(lines));
        }

        #endregion

        #region selection

        /// <summary>
        /// Gets or sets the start position of the selection.
        /// </summary>
        /// <returns>The zero-based document position where the selection starts.</returns>
        /// <remarks>
        /// When getting this property, the return value is <code>Math.Min(AnchorPosition, CurrentPosition)</code>.
        /// When setting this property, AnchorPosition is set to the value specified and CurrentPosition set to
        /// <code>Math.Max(CurrentPosition, <paramref name="value" />)</code>.
        /// The caret is not scrolled into view.
        /// </remarks>
        public static int SelectionStart {
            get {
                var pos = Api.Send(SciMsg.SCI_GETSELECTIONSTART).ToInt32();
                return Api.Lines.ByteToCharPosition(pos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                value = Api.Lines.CharToBytePosition(value);
                Api.Send(SciMsg.SCI_SETSELECTIONSTART, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the end position of the selection.
        /// </summary>
        /// <returns>The zero-based document position where the selection ends.</returns>
        /// <remarks>
        /// When getting this property, the return value is <code>Math.Max(AnchorPosition, CurrentPosition)</code>.
        /// When setting this property, CurrentPosition is set to the value specified and AnchorPosition set to
        /// <code>Math.Min(AnchorPosition, <paramref name="value" />)</code>.
        /// The caret is not scrolled into view.
        /// </remarks>
        public static int SelectionEnd {
            get {
                var pos = Api.Send(SciMsg.SCI_GETSELECTIONEND).ToInt32();
                return Api.Lines.ByteToCharPosition(pos);
            }
            set {
                value = Clamp(value, 0, TextLength);
                value = Api.Lines.CharToBytePosition(value);
                Api.Send(SciMsg.SCI_SETSELECTIONEND, new IntPtr(value));
            }
        }

        /// <summary>
        /// Sets the anchor and current position.
        /// </summary>
        /// <param name="anchorPos">The zero-based document position to start the selection.</param>
        /// <param name="currentPos">The zero-based document position to end the selection.</param>
        /// <remarks>
        /// A negative value for <paramref name="currentPos" /> signifies the end of the document.
        /// A negative value for <paramref name="anchorPos" /> signifies no selection (set the <paramref name="anchorPos" /> to
        /// the same as the <paramref name="currentPos" />).
        /// The current position is scrolled into view following this operation.
        /// </remarks>
        public static void SetSel(int anchorPos, int currentPos) {
            var textLength = TextLength;

            if (anchorPos >= 0) {
                anchorPos = Clamp(anchorPos, 0, textLength);
                anchorPos = Api.Lines.CharToBytePosition(anchorPos);
            }

            if (currentPos >= 0) {
                currentPos = Clamp(currentPos, 0, textLength);
                currentPos = Api.Lines.CharToBytePosition(currentPos);
            }

            Api.Send(SciMsg.SCI_SETSEL, new IntPtr(anchorPos), new IntPtr(currentPos));
        }

        /// <summary>
        /// Sets the current carret position + the current anchor position to the same position
        /// </summary>
        /// <param name="pos"></param>
        public static void SetSel(int pos) {
            SetSelection(pos, pos);
        }

        /// <summary>
        /// Set a single selection from anchor to caret as the ONLY selection.
        /// </summary>
        /// <param name="caret">The zero-based document position to end the selection.</param>
        /// <param name="anchor">The zero-based document position to start the selection.</param>
        public static void SetSelection(int caret, int anchor) {
            var textLength = TextLength;

            caret = Clamp(caret, 0, textLength);
            anchor = Clamp(anchor, 0, textLength);

            caret = Api.Lines.CharToBytePosition(caret);
            anchor = Api.Lines.CharToBytePosition(anchor);

            Api.Send(SciMsg.SCI_SETSELECTION, new IntPtr(caret), new IntPtr(anchor));
        }

        /// <summary>
        /// Set a single selection from anchor to caret as the ONLY selection.
        /// </summary>
        /// <param name="caret">The zero-based document position to end the selection.</param>
        public static void SetSelection(int caret) {
            var textLength = TextLength;
            caret = Clamp(caret, 0, textLength);
            caret = Api.Lines.CharToBytePosition(caret);
            Api.Send(SciMsg.SCI_SETSELECTION, new IntPtr(caret), new IntPtr(caret));
        }

        /// <summary>
        /// Moves the caret to the opposite end of the main selection.
        /// </summary>
        public static void SwapMainAnchorCaret() {
            Api.Send(SciMsg.SCI_SWAPMAINANCHORCARET);
        }

        /// <summary>
        /// Gets or sets the main selection when they are multiple selections.
        /// </summary>
        /// <returns>The zero-based main selection index.</returns>
        public static int MainSelection {
            get { return Api.Send(SciMsg.SCI_GETMAINSELECTION).ToInt32(); }
            set {
                value = ClampMin(value, 0);
                Api.Send(SciMsg.SCI_SETMAINSELECTION, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets whether additional typing affects multiple selections.
        /// Whether typing, backspace, or delete works with multiple selection simultaneously.
        /// </summary>
        /// <returns>
        /// true if typing will affect multiple selections instead of just the main selection; otherwise, false. The
        /// default is false.
        /// </returns>
        public static bool AdditionalSelectionTyping {
            get { return Api.Send(SciMsg.SCI_GETADDITIONALSELECTIONTYPING).IsTrue(); }
            set {
                var additionalSelectionTyping = value ? new IntPtr(1) : IntPtr.Zero;
                Api.Send(SciMsg.SCI_SETADDITIONALSELECTIONTYPING, additionalSelectionTyping);
            }
        }

        /// <summary>
        /// Selects all the text in the document.
        /// </summary>
        /// <remarks>The current position is not scrolled into view.</remarks>
        public static void SelectAll() {
            Api.Send(SciMsg.SCI_SELECTALL);
        }

        /// <summary>
        /// Sets a single empty selection at the start of the document.
        /// </summary>
        public static void ClearSelections() {
            Api.Send(SciMsg.SCI_CLEARSELECTIONS);
        }

        /// <summary>
        /// Removes any selection and places the caret at the specified position.
        /// </summary>
        /// <param name="pos">The zero-based document position to place the caret at.</param>
        /// <remarks>The caret is not scrolled into view.</remarks>
        public static void SetEmptySelection(int pos) {
            pos = Clamp(pos, 0, TextLength);
            pos = Api.Lines.CharToBytePosition(pos);
            Api.Send(SciMsg.SCI_SETEMPTYSELECTION, new IntPtr(pos));
        }

        /// <summary>
        /// Makes the next selection the main selection.
        /// </summary>
        public static void RotateSelection() {
            Api.Send(SciMsg.SCI_ROTATESELECTION);
        }

        /// <summary>
        /// Adds an additional selection range to the existing main selection.
        /// </summary>
        /// <param name="caret">The zero-based document position to end the selection.</param>
        /// <param name="anchor">The zero-based document position to start the selection.</param>
        /// <remarks>A main selection must first have been set by a call to SetSelection.</remarks>
        public static void AddSelection(int caret, int anchor) {
            var textLength = TextLength;
            caret = Clamp(caret, 0, textLength);
            anchor = Clamp(anchor, 0, textLength);
            caret = Api.Lines.CharToBytePosition(caret);
            anchor = Api.Lines.CharToBytePosition(anchor);
            Api.Send(SciMsg.SCI_ADDSELECTION, new IntPtr(caret), new IntPtr(anchor));
        }

        /// <summary>
        /// If there are multiple selections, removes the specified selection.
        /// </summary>
        /// <param name="selection">The zero-based selection index.</param>
        public static void DropSelection(int selection) {
            selection = ClampMin(selection, 0);
            Api.Send(SciMsg.SCI_DROPSELECTIONN, new IntPtr(selection));
        }

        #endregion

        #region ProLexer stuff

        // Set style 
        private static int _stylingPosition;
        private static int _stylingBytePosition;

        /// <summary>
        /// Gets or sets the current lexer.
        /// </summary>
        /// <returns>One of the ProLexer enumeration values. The default is Container.</returns>
        public static Lexer Lexer {
            get { return (Lexer) Api.Send(SciMsg.SCI_GETLEXER); }
            set {
                var lexer = (int) value;
                Api.Send(SciMsg.SCI_SETLEXER, new IntPtr(lexer));
            }
        }

        /// <summary>
        /// Gets or sets the current lexer by name.
        /// </summary>
        /// <returns>A String representing the current lexer.</returns>
        /// <remarks>ProLexer names are case-sensitive.</remarks>
        public static unsafe string LexerLanguage {
            get {
                var length = Api.Send(SciMsg.SCI_GETLEXERLANGUAGE).ToInt32();
                if (length == 0)
                    return String.Empty;

                var bytes = new byte[length + 1];
                fixed (byte* bp = bytes) {
                    Api.Send(SciMsg.SCI_GETLEXERLANGUAGE, IntPtr.Zero, new IntPtr(bp));
                    return GetString(new IntPtr(bp), length, Encoding.ASCII);
                }
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    Api.Send(SciMsg.SCI_SETLEXERLANGUAGE, IntPtr.Zero, IntPtr.Zero);
                } else {
                    var bytes = GetBytes(value, Encoding.ASCII, true);
                    fixed (byte* bp = bytes)
                        Api.Send(SciMsg.SCI_SETLEXERLANGUAGE, IntPtr.Zero, new IntPtr(bp));
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
            startPos = Api.Lines.CharToBytePosition(startPos);
            endPos = Api.Lines.CharToBytePosition(endPos);
            Api.Send(SciMsg.SCI_COLOURISE, new IntPtr(startPos), new IntPtr(endPos));
        }

        /// <summary>
        /// Styles the specified length of characters.
        /// </summary>
        /// <param name="length">The number of characters to style.</param>
        /// <param name="style">The Style definition index to assign each character.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length" /> or <paramref name="style" /> is less than zero. -or-
        /// The sum of a preceeding call to StartStyling or <see name="SetStyling" /> and <paramref name="length" /> is greater
        /// than the document length. -or-
        /// <paramref name="style" /> is greater than or equal to the number of style definitions.
        /// </exception>
        /// <remarks>
        /// The styling position is advanced by <paramref name="length" /> after each call allowing multiple
        /// calls to SetStyling for a single call to StartStyling.
        /// </remarks>
        public static void SetStyling(int length, int style) {
            var endPos = _stylingPosition + length;
            var endBytePos = Api.Lines.CharToBytePosition(endPos);
            if (endBytePos - _stylingBytePosition > 0)
                Api.Send(SciMsg.SCI_SETSTYLING, new IntPtr(endBytePos - _stylingBytePosition), new IntPtr(style));

            // Track this for the next call
            _stylingPosition = endPos;
            _stylingBytePosition = endBytePos;
        }

        /// <summary>
        /// Prepares for styling by setting the styling <paramref name="position" /> to start at.
        /// </summary>
        /// <param name="position">The zero-based character position in the document to start styling.</param>
        /// <remarks>
        /// After preparing the document for styling, use successive calls to SetStyling
        /// to style the document.
        /// </remarks>
        public static void StartStyling(int position) {
            position = Clamp(position, 0, TextLength);
            var pos = Api.Lines.CharToBytePosition(position);
            Api.Send(SciMsg.SCI_STARTSTYLING, new IntPtr(pos));

            // Track this so we can validate calls to SetStyling
            _stylingPosition = position;
            _stylingBytePosition = pos;
        }

        /// <summary>
        /// Returns the last document position likely to be styled correctly.
        /// </summary>
        /// <returns>The zero-based document position of the last styled character.</returns>
        public static int GetEndStyled() {
            var pos = Api.Send(SciMsg.SCI_GETENDSTYLED).ToInt32();
            return Api.Lines.ByteToCharPosition(pos);
        }

        /// <summary>
        /// Gets the style of the specified document position.
        /// </summary>
        /// <param name="position">The zero-based document position of the character to get the style for.</param>
        /// <returns>The zero-based Style index used at the specified <paramref name="position" />.</returns>
        public static int GetStyleAt(int position) {
            position = Clamp(position, 0, TextLength);
            position = Api.Lines.CharToBytePosition(position);
            return Api.Send(SciMsg.SCI_GETSTYLEAT, new IntPtr(position)).ToInt32();
        }

        /// <summary>
        /// TODO: UNTESTED
        /// set the style of a text from startPos to startPos + styleArray.Length,
        /// the styleArray is a array of bytes, each byte is the style number to the corresponding text byte
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="styleArray"></param>
        public static unsafe void StyleTextEx(int startPos, byte[] styleArray) {
            // start styling from start pos
            Api.Send(SciMsg.SCI_STARTSTYLING, new IntPtr(startPos));

            TargetStart = startPos;
            TargetEnd = startPos + styleArray.Length;

            var length = Api.Send(SciMsg.SCI_GETTARGETTEXT).ToInt32();
            if (length == 0)
                return;

            var bytes = new byte[length + 1];
            fixed (byte* bp = bytes) {
                Api.Send(SciMsg.SCI_GETTARGETTEXT, IntPtr.Zero, new IntPtr(bp));

                var styles = CharToByteStyles(styleArray, bp, length, Encoding);
                fixed (byte* stylePtr = styles)
                    Api.Send(SciMsg.SCI_SETSTYLINGEX, new IntPtr(length), new IntPtr(stylePtr));
            }
        }

        #endregion

        #region Set Other Styles

        /// <summary>
        /// Removes all styling from the document and resets the folding state.
        /// </summary>
        public static void ClearDocumentStyle() {
            Api.Send(SciMsg.SCI_CLEARDOCUMENTSTYLE);
        }

        /// <summary>
        /// Resets all style properties to those currently configured for the Style.Default style.
        /// </summary>
        public static void StyleClearAll() {
            Api.Send(SciMsg.SCI_STYLECLEARALL);
        }

        /// <summary>
        /// Resets the Style.Default style to its initial state.
        /// </summary>
        public static void StyleResetDefault() {
            Api.Send(SciMsg.SCI_STYLERESETDEFAULT);
        }

        /// <summary>
        /// The colour of the caret
        /// </summary>
        public static Color CaretForeColor {
            get { return ColorTranslator.FromWin32(Api.Send(SciMsg.SCI_GETCARETFORE).ToInt32()); }
            set { Api.Send(SciMsg.SCI_STYLESETFORE, new IntPtr(ColorTranslator.ToWin32(value.IsEmpty ? Color.Black : value))); }
        }

        /// <summary>
        /// Sets a global override to the selection background + foreground color.
        /// </summary>
        public static void SetSelectionColor(bool use, Color bg, Color fg) {
            Api.Send(SciMsg.SCI_SETSELBACK, (use && bg != Color.Transparent).ToPointer(), new IntPtr(ColorTranslator.ToWin32(bg)));
            Api.Send(SciMsg.SCI_SETSELFORE, (use && fg != Color.Transparent).ToPointer(), new IntPtr(ColorTranslator.ToWin32(fg)));
        }

        /// <summary>
        /// The selection can be drawn translucently in the selection background colour by setting an alpha value.
        /// </summary>
        public static int SelectionBackAlpha {
            get { return Api.Send(SciMsg.SCI_GETSELALPHA).ToInt32(); }
            set { Api.Send(SciMsg.SCI_SETSELALPHA, new IntPtr(value)); }
        }

        /// <summary>
        /// Sets a global override to the additional selections background + foreground color.
        /// </summary>
        public static void SetAdditionalSelectionColor(bool use, Color bg, Color fg) {
            Api.Send(SciMsg.SCI_SETADDITIONALSELBACK, (use && bg != Color.Transparent).ToPointer(), new IntPtr(ColorTranslator.ToWin32(bg)));
            Api.Send(SciMsg.SCI_SETADDITIONALSELFORE, (use && fg != Color.Transparent).ToPointer(), new IntPtr(ColorTranslator.ToWin32(fg)));
        }

        /// <summary>
        /// sets the fore/background color of the whitespaces, overriding the lexer's
        /// </summary>
        public static void SetWhiteSpaceColor(bool use, Color bg, Color fg) {
            Api.Send(SciMsg.SCI_SETWHITESPACEBACK, (use && bg != Color.Transparent).ToPointer(), new IntPtr(ColorTranslator.ToWin32(bg)));
            Api.Send(SciMsg.SCI_SETWHITESPACEFORE, (use && fg != Color.Transparent).ToPointer(), new IntPtr(ColorTranslator.ToWin32(fg)));
        }

        /// <summary>
        /// sets the fore/background color of the IndentGuide, overriding the lexer's
        /// </summary>
        public static void SetIndentGuideColor(Color bg, Color fg) {
            // we also set the indent line color here
            new Style(Style.IndentGuide) {
                BackColor = bg,
                ForeColor = fg
            };
        }

        /// <summary>
        /// You can choose to make the background colour of the line containing the caret different with these messages
        /// See CaretLineVisible to activate
        /// </summary>
        public static Color CaretLineBackColor {
            get { return ColorTranslator.FromWin32(Api.Send(SciMsg.SCI_GETCARETLINEBACK).ToInt32()); }
            set { Api.Send(SciMsg.SCI_SETCARETLINEBACK, new IntPtr(ColorTranslator.ToWin32(value.IsEmpty ? Color.Black : value))); }
        }

        /// <summary>
        /// You can choose to make the background colour of the line containing the caret different with these messages
        /// See CaretLineBackColor to set the color and use this to activate
        /// </summary>
        public static bool CaretLineVisible {
            get { return Api.Send(SciMsg.SCI_GETCARETLINEVISIBLE).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETCARETLINEVISIBLE, value.ToPointer()); }
        }

        /// <summary>
        /// The caret line may also be drawn translucently which allows other background colours to show through.
        /// This is done by setting the alpha (translucency) value by calling SCI_SETCARETLINEBACKALPHA. When the alpha is not
        /// SC_ALPHA_NOALPHA (256), the caret line is drawn after all other features so will affect the colour of all other
        /// features.
        /// Alpha goes from 0 (transparent) to 256 (opaque)
        /// </summary>
        public static int CaretLineBackAlpha {
            get { return Api.Send(SciMsg.SCI_GETCARETLINEBACKALPHA).ToInt32(); }
            set { Api.Send(SciMsg.SCI_SETCARETLINEBACKALPHA, new IntPtr(value)); }
        }

        /// <summary>
        /// Set the caret color
        /// </summary>
        public static Color CaretColor {
            set { Api.Send(SciMsg.SCI_SETCARETFORE, new IntPtr(ColorTranslator.ToWin32(value.IsEmpty ? Color.Black : value))); }
        }

        /// <summary>
        /// allow changing the colour of the fold margin and fold margin highlight
        /// </summary>
        public static void SetFoldMarginColors(bool use, Color bgColor, Color fgColor) {
            Api.Send(SciMsg.SCI_SETFOLDMARGINHICOLOUR, use.ToPointer(), new IntPtr(ColorTranslator.ToWin32(fgColor)));
            Api.Send(SciMsg.SCI_SETFOLDMARGINCOLOUR, use.ToPointer(), new IntPtr(ColorTranslator.ToWin32(bgColor)));
        }

        /// <summary>
        /// allow changing the colour of the fold margin and fold margin highlight
        /// </summary>
        public static void SetFoldMarginMarkersColor(Color bgColor, Color fgColor, Color activeColor) {
            for (int i = 0; i < 7; i++) {
                var marker = GetMarker(i + (int) SciMsg.SC_MARKNUM_FOLDEREND);
                marker.SetBackColor(bgColor);
                marker.SetForeColor(fgColor);
                marker.SetBackSelectedColor(activeColor);
            }
        }

        /// <summary>
        /// While the cursor hovers over text in a style with the hotspot attribute set. Single line mode stops a hotspot from
        /// wrapping onto next line.
        /// </summary>
        public static bool HotSpotSingleLine {
            get { return Api.Send(SciMsg.SCI_GETHOTSPOTSINGLELINE).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETHOTSPOTSINGLELINE, value.ToPointer()); }
        }

        /// <summary>
        /// While the cursor hovers over text in a style with the hotspot attribute set, an underline can be drawn
        /// </summary>
        public static bool HotSpotActiveUnderline {
            get { return Api.Send(SciMsg.SCI_GETHOTSPOTACTIVEUNDERLINE).IsTrue(); }
            set { Api.Send(SciMsg.SCI_SETHOTSPOTACTIVEUNDERLINE, value.ToPointer()); }
        }

        /// <summary>
        /// While the cursor hovers over text in a style with the hotspot attribute set, the default colouring can be modified
        /// </summary>
        public static void SetHotSpotActiveColor(bool use, Color fg, Color bg) {
            Api.Send(SciMsg.SCI_SETHOTSPOTACTIVEFORE, use.ToPointer(), new IntPtr(ColorTranslator.ToWin32(fg)));
            Api.Send(SciMsg.SCI_SETHOTSPOTACTIVEBACK, use.ToPointer(), new IntPtr(ColorTranslator.ToWin32(bg)));
        }

        #endregion

        #region annotations

        /// <summary>
        /// Clear all annotations in one go
        /// </summary>
        public static void AnnotationClearAll() {
            Win32Api.SendMessage(Api.Handle, SciMsg.SCI_ANNOTATIONCLEARALL, 0, 0);
        }

        /// <summary>
        /// Gets or sets the display of annotations
        /// BE CAREFULE : any else than HIDDEN slows down the opening of files by a TON
        /// </summary>
        /// <returns>One of the <see cref="Annotation" /> enumeration values. The default is <see cref="Annotation.Hidden" />.</returns>
        public static Annotation AnnotationVisible {
            get { return (Annotation) Api.Send(SciMsg.SCI_ANNOTATIONGETVISIBLE).ToInt32(); }
            set {
                var visible = (int) value;
                Api.Send(SciMsg.SCI_ANNOTATIONSETVISIBLE, new IntPtr(visible));
            }
        }

        #endregion

        #region helper

        /// <summary>
        /// Returns scintilla's encoding
        /// </summary>
        internal static Encoding Encoding {
            get {
                // Should always be UTF-8 unless someone has done an end run around us
                var codePage = (int) Api.Send(SciMsg.SCI_GETCODEPAGE);
                return codePage == 0 ? Encoding.Default : Encoding.GetEncoding(codePage);
            }
        }

        /// <summary>
        /// Returns a string's bytes array with given encoding
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <param name="zeroTerminated"></param>
        /// <returns></returns>
        public static unsafe byte[] GetBytes(string text, Encoding encoding, bool zeroTerminated) {
            if (String.IsNullOrEmpty(text))
                return new byte[0];

            var count = encoding.GetByteCount(text);
            var buffer = new byte[count + (zeroTerminated ? 1 : 0)];

            fixed (byte* bp = buffer)
            fixed (char* ch = text) {
                encoding.GetBytes(ch, text.Length, bp, count);
            }

            if (zeroTerminated)
                buffer[buffer.Length - 1] = 0;

            return buffer;
        }

        /// <summary>
        /// Returns char array's bytes array with given encoding
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <param name="encoding"></param>
        /// <param name="zeroTerminated"></param>
        /// <returns></returns>
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

        public static byte[] BitmapToArgb(Bitmap image) {
            var bytes = new byte[4 * image.Width * image.Height];
            var i = 0;
            for (var y = 0; y < image.Height; y++) {
                for (var x = 0; x < image.Width; x++) {
                    var color = image.GetPixel(x, y);
                    bytes[i++] = color.R;
                    bytes[i++] = color.G;
                    bytes[i++] = color.B;
                    bytes[i++] = color.A;
                }
            }
            return bytes;
        }

        public static unsafe byte[] ByteToCharStyles(byte* styles, byte* text, int length, Encoding encoding) {
            // This is used by annotations and margins to get all the styles in one call.
            // It converts an array of styles where each element corresponds to a BYTE
            // to an array of styles where each element corresponds to a CHARACTER.

            var bytePos = 0; // Position within text BYTES and style BYTES (should be the same)
            var charPos = 0; // Position within style CHARACTERS
            var decoder = encoding.GetDecoder();
            var result = new byte[encoding.GetCharCount(text, length)];

            while (bytePos < length) {
                if (decoder.GetCharCount(text + bytePos, 1, false) > 0)
                    result[charPos++] = *(styles + bytePos); // New char

                bytePos++;
            }

            return result;
        }

        public static unsafe byte[] CharToByteStyles(byte[] styles, byte* text, int length, Encoding encoding) {
            // This is used by annotations and margins to style all the text in one call.
            // It converts an array of styles where each element corresponds to a CHARACTER
            // to an array of styles where each element corresponds to a BYTE.

            var bytePos = 0; // Position within text BYTES and style BYTES (should be the same)
            var charPos = 0; // Position within style CHARACTERS
            var decoder = encoding.GetDecoder();
            var result = new byte[length];

            while (bytePos < length && charPos < styles.Length) {
                result[bytePos] = styles[charPos];
                if (decoder.GetCharCount(text + bytePos, 1, false) > 0)
                    charPos++; // Move a char

                bytePos++;
            }

            return result;
        }

        /// <summary>
        /// Get string from pointer
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static unsafe string GetString(IntPtr bytes, int length, Encoding encoding) {
            if (length <= 0)
                return String.Empty;
            var ptr = (sbyte*) bytes;
            var str = new string(ptr, 0, length, encoding);
            return str;
        }

        /// <summary>
        /// Forces a value between a minimum and a maximum
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Clamp(int value, int min, int max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        /// <summary>
        /// Forces a value to a minimum
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static int ClampMin(int value, int min) {
            if (value < min)
                return min;

            return value;
        }

        #endregion
    }
}