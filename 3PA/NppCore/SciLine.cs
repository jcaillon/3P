using System;
using _3PA.WindowsCore;

namespace _3PA.NppCore {
    internal static partial class Sci { 

        /// <summary>
        /// Represents a line of text in a Scintilla control
        /// the first line has the index 0
        /// </summary>
        public class Line {
            #region Methods

            /// <summary>
            /// Expands any parent folds to ensure the line is visible.
            /// </summary>
            public void EnsureVisible() {
                Api.Send(SciMsg.SCI_ENSUREVISIBLE, new IntPtr(Index));
            }

            //public void ExpandChildren(int level)
            //{
            //}

            /// <summary>
            /// Performs the specified fold action on the current line and all child Api.Lines.
            /// </summary>
            /// <param name="action">One of the FoldAction enumeration values.</param>
            public void FoldChildren(FoldAction action) {
                Api.Send(SciMsg.SCI_FOLDCHILDREN, new IntPtr(Index), new IntPtr((int) action));
            }

            /// <summary>
            /// Performs the specified fold action on the current line.
            /// </summary>
            /// <param name="action">One of the FoldAction enumeration values.</param>
            public void FoldLine(FoldAction action) {
                Api.Send(SciMsg.SCI_FOLDLINE, new IntPtr(Index), new IntPtr((int) action));
            }

            /// <summary>
            /// Searches for the next line that has a folding level that is less than or equal to <paramref name="level" />
            /// and returns the previous line index.
            /// </summary>
            /// <param name="level">The level of the line to search for. A value of -1 will use the current line FoldLevel.</param>
            /// <returns>
            /// The zero-based index of the next line that has a FoldLevel less than or equal
            /// to <paramref name="level" />. If the current line is a fold point and <paramref name="level" /> is -1 the
            /// index returned is the last line that would be made visible or hidden by toggling the fold state.
            /// </returns>
            public int GetLastChild(int level) {
                return Api.Send(SciMsg.SCI_GETLASTCHILD, new IntPtr(Index), new IntPtr(level)).ToInt32();
            }

            /// <summary>
            /// Navigates the caret to the start of the line.
            /// </summary>
            /// <remarks>Any selection is discarded.</remarks>
            public void Goto() {
                Api.Send(SciMsg.SCI_GOTOLINE, new IntPtr(Index));
            }

            /// <summary>
            /// Adds the specified Marker to the line.
            /// </summary>
            /// <param name="marker">The zero-based index of the marker to add to the line.</param>
            /// <returns>A MarkerHandle which can be used to track the line.</returns>
            /// <remarks>This method does not check if the line already contains the <paramref name="marker" />.</remarks>
            public MarkerHandle MarkerAdd(int marker) {
                var handle = Api.Send(SciMsg.SCI_MARKERADD, new IntPtr(Index), new IntPtr(marker));
                return new MarkerHandle {Value = handle};
            }

            /// <summary>
            /// Adds one or more markers to the line in a single call using a bit mask.
            /// </summary>
            /// <param name="markerMask">
            /// An unsigned 32-bit value with each bit cooresponding to one of the 32 zero-based Margin
            /// indexes to add.
            /// </param>
            public void MarkerAddSet(uint markerMask) {
                var mask = unchecked((int) markerMask);
                Api.Send(SciMsg.SCI_MARKERADDSET, new IntPtr(Index), new IntPtr(mask));
            }

            /// <summary>
            /// Removes the specified Marker from the line.
            /// </summary>
            /// <param name="marker">
            /// The zero-based index of the marker to remove from the line or -1 to delete all markers from the
            /// line.
            /// </param>
            /// <remarks>If the same marker has been added to the line more than once, this will delete one copy each time it is used.</remarks>
            public void MarkerDelete(int marker) {
                Api.Send(SciMsg.SCI_MARKERDELETE, new IntPtr(Index), new IntPtr(marker));
            }

            /// <summary>
            /// Returns a bit mask indicating which markers are present on the line.
            /// </summary>
            /// <returns>An unsigned 32-bit value with each bit cooresponding to one of the 32 zero-based Margin indexes.</returns>
            public uint MarkerGet() {
                var mask = Api.Send(SciMsg.SCI_MARKERGET, new IntPtr(Index)).ToInt32();
                return unchecked((uint) mask);
            }

            /// <summary>
            /// Efficiently searches from the current line forward to the end of the document for the specified markers.
            /// </summary>
            /// <param name="markerMask">
            /// An unsigned 32-bit value with each bit cooresponding to one of the 32 zero-based Margin
            /// indexes.
            /// </param>
            /// <returns>
            /// If found, the zero-based line index containing one of the markers in <paramref name="markerMask" />;
            /// otherwise, -1.
            /// </returns>
            /// <remarks>For example, the mask for marker index 10 is 1 shifted left 10 times (1 &lt;&lt; 10).</remarks>
            public int MarkerNext(uint markerMask) {
                var mask = unchecked((int) markerMask);
                return Api.Send(SciMsg.SCI_MARKERNEXT, new IntPtr(Index), new IntPtr(mask)).ToInt32();
            }

            /// <summary>
            /// Efficiently searches from the current line backward to the start of the document for the specified markers.
            /// </summary>
            /// <param name="markerMask">
            /// An unsigned 32-bit value with each bit cooresponding to one of the 32 zero-based Margin
            /// indexes.
            /// </param>
            /// <returns>
            /// If found, the zero-based line index containing one of the markers in <paramref name="markerMask" />;
            /// otherwise, -1.
            /// </returns>
            /// <remarks>For example, the mask for marker index 10 is 1 shifted left 10 times (1 &lt;&lt; 10).</remarks>
            public int MarkerPrevious(uint markerMask) {
                var mask = unchecked((int) markerMask);
                return Api.Send(SciMsg.SCI_MARKERPREVIOUS, new IntPtr(Index), new IntPtr(mask)).ToInt32();
            }

            /// <summary>
            /// Toggles the folding state of the line; expanding or contracting all child Api.Lines.
            /// </summary>
            /// <remarks>The line must be set as a FoldLevelFlags.Header.</remarks>
            public void ToggleFold() {
                Api.Send(SciMsg.SCI_TOGGLEFOLD, new IntPtr(Index));
            }

            #endregion Methods

            #region Properties

            /// <summary>
            /// Gets the number of annotation lines of text.
            /// </summary>
            /// <returns>The number of annotation Api.Lines.</returns>
            public int AnnotationLines {
                get { return Api.Send(SciMsg.SCI_ANNOTATIONGETLINES, new IntPtr(Index)).ToInt32(); }
            }

            /// <summary>
            /// Gets or sets the style of the annotation text.
            /// </summary>
            /// <returns>
            /// The zero-based index of the annotation text Style or 256 when AnnotationStyles
            /// has been used to set individual character styles.
            /// </returns>
            public int AnnotationStyle {
                get { return Api.Send(SciMsg.SCI_ANNOTATIONGETSTYLE, new IntPtr(Index)).ToInt32(); }
                set { Api.Send(SciMsg.SCI_ANNOTATIONSETSTYLE, new IntPtr(Index), new IntPtr(value)); }
            }

            /// <summary>
            /// Gets or sets an array of style indexes corresponding to each charcter in the AnnotationText
            /// so that each character may be individually styled.
            /// </summary>
            /// <returns>
            /// An array of Style indexes corresponding with each annotation text character or an uninitialized
            /// array when AnnotationStyle has been used to set a single style for all characters.
            /// </returns>
            /// <remarks>
            /// AnnotationText must be set prior to setting this property.
            /// The <paramref name="value" /> specified should have a length equal to the AnnotationText length to properly style
            /// all characters.
            /// </remarks>
            public unsafe byte[] AnnotationStyles {
                get {
                    var length = Api.Send(SciMsg.SCI_ANNOTATIONGETTEXT, new IntPtr(Index)).ToInt32();
                    if (length == 0)
                        return new byte[0];

                    var text = new byte[length + 1];
                    var styles = new byte[length + 1];

                    fixed (byte* textPtr = text)
                    fixed (byte* stylePtr = styles) {
                        Api.Send(SciMsg.SCI_ANNOTATIONGETTEXT, new IntPtr(Index), new IntPtr(textPtr));
                        Api.Send(SciMsg.SCI_ANNOTATIONGETSTYLES, new IntPtr(Index), new IntPtr(stylePtr));

                        return ByteToCharStyles(stylePtr, textPtr, length, Encoding);
                    }
                }
                set {
                    var length = Api.Send(SciMsg.SCI_ANNOTATIONGETTEXT, new IntPtr(Index)).ToInt32();
                    if (length == 0)
                        return;

                    var text = new byte[length + 1];
                    fixed (byte* textPtr = text) {
                        Api.Send(SciMsg.SCI_ANNOTATIONGETTEXT, new IntPtr(Index), new IntPtr(textPtr));

                        var styles = CharToByteStyles(value ?? new byte[0], textPtr, length, Encoding);
                        fixed (byte* stylePtr = styles)
                            Api.Send(SciMsg.SCI_ANNOTATIONSETSTYLES, new IntPtr(Index), new IntPtr(stylePtr));
                    }
                }
            }

            /// <summary>
            /// Gets or sets the line annotation text.
            /// </summary>
            /// <returns>A String representing the line annotation text.</returns>
            public unsafe string AnnotationText {
                get {
                    var length = Api.Send(SciMsg.SCI_ANNOTATIONGETTEXT, new IntPtr(Index)).ToInt32();
                    if (length == 0)
                        return String.Empty;

                    var bytes = new byte[length + 1];
                    fixed (byte* bp = bytes) {
                        Api.Send(SciMsg.SCI_ANNOTATIONGETTEXT, new IntPtr(Index), new IntPtr(bp));
                        return GetString(new IntPtr(bp), length, Encoding);
                    }
                }
                set {
                    if (String.IsNullOrEmpty(value)) {
                        Win32Api.SendMessage(Api.Handle, SciMsg.SCI_ANNOTATIONSETTEXT, Index, null);
                    } else {
                        var bytes = GetBytes(value, Encoding, true);
                        fixed (byte* bp = bytes)
                            Api.Send(SciMsg.SCI_ANNOTATIONSETTEXT, new IntPtr(Index), new IntPtr(bp));
                    }
                }
            }

            /// <summary>
            /// Searches from the current line to find the index of the next contracted fold header.
            /// </summary>
            /// <returns>The zero-based line index of the next contracted folder header.</returns>
            /// <remarks>If the current line is contracted the current line index is returned.</remarks>
            public int ContractedFoldNext {
                get { return Api.Send(SciMsg.SCI_CONTRACTEDFOLDNEXT, new IntPtr(Index)).ToInt32(); }
            }

            /// <summary>
            /// Gets the zero-based index of the line as displayed in a Scintilla control
            /// taking into consideration folded (hidden) Api.Lines.
            /// </summary>
            /// <returns>The zero-based display line index.</returns>
            public int DisplayIndex {
                get { return Api.Send(SciMsg.SCI_VISIBLEFROMDOCLINE, new IntPtr(Index)).ToInt32(); }
            }

            /// <summary>
            /// Gets the zero-based character position in the document where the line ends, does not include eol char
            /// </summary>
            /// <returns>The equivalent of Position + Length.</returns>
            public int EndPosition {
                get { return Api.Lines.ByteToCharPosition(Api.Send(SciMsg.SCI_GETLINEENDPOSITION, new IntPtr(Index)).ToInt32()); }
            }

            /// <summary>
            /// Gets the zero-based character position in the document where the line ends (exclusive).
            /// includes any end of line char
            /// </summary>
            /// <returns>The equivalent of Position + Length.</returns>
            public int RealEndPosition {
                get { return Position + Length; }
            }

            /// <summary>
            /// Gets or sets the expanded state (not the visible state) of the line.
            /// </summary>
            /// <remarks>
            /// For toggling the fold state of a single line the ToggleFold method should be used.
            /// This property is useful for toggling the state of many folds without updating the display until finished.
            /// </remarks>
            public bool Expanded {
                get { return Api.Send(SciMsg.SCI_GETFOLDEXPANDED, new IntPtr(Index)) != IntPtr.Zero; }
                set {
                    var expanded = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_SETFOLDEXPANDED, new IntPtr(Index), expanded);
                }
            }

            /// <summary>
            /// Gets or sets the fold level of the line.
            /// </summary>
            /// <returns>The fold level ranging from 0 to 4095. The default is 1024.</returns>
            public int FoldLevel {
                get {
                    var level = Api.Send(SciMsg.SCI_GETFOLDLEVEL, new IntPtr(Index)).ToInt32();
                    return level & (int) SciMsg.SC_FOLDLEVELNUMBERMASK;
                }
                set {
                    var bits = (int) FoldLevelFlags;
                    bits |= value;

                    Api.Send(SciMsg.SCI_SETFOLDLEVEL, new IntPtr(Index), new IntPtr(bits));
                }
            }

            /// <summary>
            /// Gets or sets the fold level flags.
            /// </summary>
            /// <returns>A bitwise combination of the FoldLevelFlags enumeration.</returns>
            public FoldLevelFlags FoldLevelFlags {
                get {
                    var flags = Api.Send(SciMsg.SCI_GETFOLDLEVEL, new IntPtr(Index)).ToInt32();
                    return (FoldLevelFlags) (flags & ~(int) SciMsg.SC_FOLDLEVELNUMBERMASK);
                }
                set {
                    var bits = FoldLevel;
                    bits |= (int) value;

                    Api.Send(SciMsg.SCI_SETFOLDLEVEL, new IntPtr(Index), new IntPtr(bits));
                }
            }

            /// <summary>
            /// Gets the zero-based line index of the first line before the current line that is marked as
            /// FoldLevelFlags.Header and has a FoldLevel less than the current line.
            /// </summary>
            /// <returns>The zero-based line index of the fold parent if present; otherwise, -1.</returns>
            public int FoldParent {
                get { return Api.Send(SciMsg.SCI_GETFOLDPARENT, new IntPtr(Index)).ToInt32(); }
            }

            /// <summary>
            /// Gets the line index.
            /// </summary>
            /// <returns>The zero-based line index within the LineCollection that created it.</returns>
            public int Index { get; set; }

            /// <summary>
            /// Gets the length of the line.
            /// </summary>
            /// <returns>The number of characters in the line including any end of line characters.</returns>
            public int Length {
                get { return Api.Lines.LineCharLength(Index); }
            }

            /// <summary>
            /// Gets or sets the style of the margin text in a MarginType.Text or MarginType.RightText margin.
            /// </summary>
            /// <returns>
            /// The zero-based index of the margin text Style or 256 when MarginStyles
            /// has been used to set individual character styles.
            /// </returns>
            public int MarginStyle {
                get { return Api.Send(SciMsg.SCI_MARGINGETSTYLE, new IntPtr(Index)).ToInt32(); }
                set { Api.Send(SciMsg.SCI_MARGINSETSTYLE, new IntPtr(Index), new IntPtr(value)); }
            }

            /// <summary>
            /// Gets or sets an array of style indexes corresponding to each charcter in the MarginText
            /// so that each character may be individually styled.
            /// </summary>
            /// <returns>
            /// An array of Style indexes corresponding with each margin text character or an uninitialized
            /// array when MarginStyle has been used to set a single style for all characters.
            /// </returns>
            /// <remarks>
            /// MarginText must be set prior to setting this property.
            /// The <paramref name="value" /> specified should have a length equal to the MarginText length to properly style all
            /// characters.
            /// </remarks>
            public unsafe byte[] MarginStyles {
                get {
                    var length = Api.Send(SciMsg.SCI_MARGINGETTEXT, new IntPtr(Index)).ToInt32();
                    if (length == 0)
                        return new byte[0];

                    var text = new byte[length + 1];
                    var styles = new byte[length + 1];

                    fixed (byte* textPtr = text)
                    fixed (byte* stylePtr = styles) {
                        Api.Send(SciMsg.SCI_MARGINGETTEXT, new IntPtr(Index), new IntPtr(textPtr));
                        Api.Send(SciMsg.SCI_MARGINGETSTYLES, new IntPtr(Index), new IntPtr(stylePtr));

                        return ByteToCharStyles(stylePtr, textPtr, length, Encoding);
                    }
                }
                set {
                    var length = Api.Send(SciMsg.SCI_MARGINGETTEXT, new IntPtr(Index)).ToInt32();
                    if (length == 0)
                        return;

                    var text = new byte[length + 1];
                    fixed (byte* textPtr = text) {
                        Api.Send(SciMsg.SCI_MARGINGETTEXT, new IntPtr(Index), new IntPtr(textPtr));

                        var styles = CharToByteStyles(value ?? new byte[0], textPtr, length, Encoding);
                        fixed (byte* stylePtr = styles)
                            Api.Send(SciMsg.SCI_MARGINSETSTYLES, new IntPtr(Index), new IntPtr(stylePtr));
                    }
                }
            }

            /// <summary>
            /// Gets or sets the text displayed in the line margin when the margin type is
            /// MarginType.Text or MarginType.RightText.
            /// </summary>
            /// <returns>The text displayed in the line margin.</returns>
            public unsafe string MarginText {
                get {
                    var length = Api.Send(SciMsg.SCI_MARGINGETTEXT, new IntPtr(Index)).ToInt32();
                    if (length == 0)
                        return String.Empty;

                    var bytes = new byte[length + 1];
                    fixed (byte* bp = bytes) {
                        Api.Send(SciMsg.SCI_MARGINGETTEXT, new IntPtr(Index), new IntPtr(bp));
                        return GetString(new IntPtr(bp), length, Encoding);
                    }
                }
                set {
                    if (String.IsNullOrEmpty(value)) {
                        // Scintilla docs suggest that setting to NULL rather than an empty string will free memory
                        Api.Send(SciMsg.SCI_MARGINSETTEXT, new IntPtr(Index), IntPtr.Zero);
                    } else {
                        var bytes = GetBytes(value, Encoding, true);
                        fixed (byte* bp = bytes)
                            Api.Send(SciMsg.SCI_MARGINSETTEXT, new IntPtr(Index), new IntPtr(bp));
                    }
                }
            }

            /// <summary>
            /// Gets the zero-based character position in the document where the line begins.
            /// </summary>
            /// <returns>The document position of the first character in the line.</returns>
            public int Position {
                get { return Api.Lines.CharPositionFromLine(Index); }
            }

            /// <summary>
            /// Gets the line text. Includes any end of line char, use the extension TrimEndEol() => .TrimEnd('\r', '\n')
            /// </summary>
            /// <returns>A string representing the document line.</returns>
            /// <remarks>The returned text includes any end of line characters.</remarks>
            public unsafe string LineText {
                get {
                    var start = Api.Send(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(Index));
                    var length = Api.Send(SciMsg.SCI_LINELENGTH, new IntPtr(Index));
                    var ptr = Api.Send(SciMsg.SCI_GETRANGEPOINTER, start, length);
                    if (ptr == IntPtr.Zero)
                        return String.Empty;

                    var text = new string((sbyte*) ptr, 0, length.ToInt32(), Encoding);
                    return text;
                }
            }

            /// <summary>
            /// Sets or gets the line indentation.
            /// </summary>
            /// <returns>The indentation measured in character columns, which corresponds to the width of space characters.</returns>
            public int Indentation {
                get { return Api.Send(SciMsg.SCI_GETLINEINDENTATION, new IntPtr(Index)).ToInt32(); }
                set { Api.Send(SciMsg.SCI_SETLINEINDENTATION, new IntPtr(Index), new IntPtr(value)); }
            }

            /// <summary>
            /// This returns the position at the end of indentation of a line
            /// </summary>
            public int IndentationPosition {
                get { return Api.Lines.ByteToCharPosition(Api.Send(SciMsg.SCI_GETLINEINDENTPOSITION, new IntPtr(Index)).ToInt32()); }
            }

            /// <summary>
            /// Gets a value indicating whether the line is visible.
            /// </summary>
            /// <returns>true if the line is visible; otherwise, false.</returns>
            public bool Visible {
                get { return Api.Send(SciMsg.SCI_GETLINEVISIBLE, new IntPtr(Index)) != IntPtr.Zero; }
            }

            /// <summary>
            /// Gets the number of display lines this line would occupy when wrapping is enabled.
            /// </summary>
            /// <returns>The number of display lines needed to wrap the current document line.</returns>
            public int WrapCount {
                get { return Api.Send(SciMsg.SCI_WRAPCOUNT, new IntPtr(Index)).ToInt32(); }
            }

            #endregion Properties

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the Line class.
            /// </summary>
            /// <param name="index">The index of this line within the LineCollection that created it.</param>
            public Line(int index) {
                index = Clamp(index, 0, Count);
                Index = index;
            }

            /// <summary>
            /// New line objetc for the current line
            /// </summary>
            public Line() {
                Index = CurrentLine;
            }

            #endregion Constructors

            #region static

            /// <summary>
            /// Gets the current line index.
            /// </summary>
            /// <returns>The zero-based line index containing the CurrentPosition.</returns>
            public static int CurrentLine {
                get {
                    var currentPos = Api.Send(SciMsg.SCI_GETCURRENTPOS).ToInt32();
                    var line = Api.Send(SciMsg.SCI_LINEFROMPOSITION, new IntPtr(currentPos)).ToInt32();
                    return line;
                }
            }

            /// <summary>
            /// Get the total number of lines
            /// An empty document has 1 line, a document with only one \n has 2 lines
            /// </summary>
            public static int Count {
                get { return Api.Lines.Count; }
            }

            #endregion
        }
    }
}
