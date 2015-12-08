public class Npp {
    /// I keep those methods just in case, but they are either slower than their counterparts, 
    /// or their existence is unjustified...

    #region Dont use...

    /// <summary>
    ///     Deletes a range of text from the document.
    /// </summary>
    /// <param name="position">The zero-based character position to start deleting.</param>
    /// <param name="length">The number of characters to delete.</param>
    private static void DeleteRange(int position, int length) {
        var textLength = TextLength;
        position = Clamp(position, 0, textLength);
        length = Clamp(length, 0, textLength - position);

        // Convert to byte position/length
        var byteStartPos = Lines.CharToBytePosition(position);
        var byteEndPos = Lines.CharToBytePosition(position + length);

        Sci.Send(SciMsg.SCI_DELETERANGE, new IntPtr(byteStartPos), new IntPtr(byteEndPos - byteStartPos));
    }

    /// <summary>
    ///     Gets the current target text.
    /// </summary>
    /// <returns>A String representing the text between TargetStart and TargetEnd.</returns>
    /// <remarks>Targets which have a start position equal or greater to the end position will return an empty String.</remarks>
    private static unsafe string TargetText {
        get {
            var length = Sci.Send(SciMsg.SCI_GETTARGETTEXT).ToInt32();
            if (length == 0)
                return string.Empty;

            var bytes = new byte[length + 1];
            fixed (byte* bp = bytes) {
                Sci.Send(SciMsg.SCI_GETTARGETTEXT, IntPtr.Zero, new IntPtr(bp));
                return GetString(new IntPtr(bp), length, Encoding);
            }
        }
    }

    /// <summary>
    ///     Adds the specified text to the end of the document.
    /// </summary>
    /// <param name="text">The text to add to the document.</param>
    /// <remarks>The current selection is not changed and the new text is not scrolled into view.</remarks>
    private static unsafe void AppendText(string text) {
        var bytes = GetBytes(text ?? string.Empty, Encoding, false);
        fixed (byte* bp = bytes)
            Sci.Send(SciMsg.SCI_APPENDTEXT, new IntPtr(bytes.Length), new IntPtr(bp));
    }

    /// <summary>
    ///     Inserts the specified text at the current caret position.
    /// </summary>
    /// <param name="text">The text to insert at the current caret position.</param>
    /// <remarks>The caret position is set to the end of the inserted text, but it is not scrolled into view.</remarks>
    private static unsafe void AddText(string text) {
        var bytes = GetBytes(text ?? string.Empty, Encoding, false);
        fixed (byte* bp = bytes)
            Sci.Send(SciMsg.SCI_ADDTEXT, new IntPtr(bytes.Length), new IntPtr(bp));
    }

    /// <summary>
    ///     Returns the character as the specified document position.
    /// </summary>
    /// <param name="position">The zero-based document position of the character to get.</param>
    /// <returns>The character at the specified <paramref name="position" />.</returns>
    private static unsafe int GetCharAt(int position) {
        position = Clamp(position, 0, TextLength);
        position = Lines.CharToBytePosition(position);

        var nextPosition = Sci.Send(SciMsg.SCI_POSITIONRELATIVE, new IntPtr(position), new IntPtr(1)).ToInt32();
        var length = (nextPosition - position);
        if (length <= 1) {
            // Position is at single-byte character
            return Sci.Send(SciMsg.SCI_GETCHARAT, new IntPtr(position)).ToInt32();
        }

        // Position is at multibyte character
        var bytes = new byte[length + 1];
        fixed (byte* bp = bytes) {
            Sci_TextRange* range = stackalloc Sci_TextRange[1];
            range->chrg.cpMin = position;
            range->chrg.cpMax = nextPosition;
            range->lpstrText = new IntPtr(bp);

            Sci.Send(SciMsg.SCI_GETTEXTRANGE, IntPtr.Zero, new IntPtr(range));
            var str = GetString(new IntPtr(bp), length, Encoding);
            return str[0];
        }
    }

    #endregion

}