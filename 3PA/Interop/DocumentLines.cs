using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace _3PA.Interop {

    /// <summary>
    /// This class is a rip off from https://github.com/jacobslusser/ScintillaNET/!
    /// Freaking awesome work, i just adapted it to make it work with Npp's scintilla
    /// </summary>
    public class DocumentLines {

        #region Fields

        private GapBuffer<PerLine> _perLineData;

        // The 'step' is a break in the continuity of our line starts. It allows us
        // to delay the updating of every line start when text is inserted/deleted.
        private int _stepLine;
        private int _stepLength;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Adjust the number of CHARACTERS in a line.
        /// </summary>
        private void AdjustLineLength(int index, int delta) {
            MoveStep(index);
            _stepLength += delta;

            // Invalidate multibyte flag
            var perLine = _perLineData[index];
            perLine.ContainsMultibyte = ContainsMultibyte.Unkown;
            _perLineData[index] = perLine;
        }

        /// <summary>
        /// Converts a BYTE offset to a CHARACTER offset.
        /// </summary>
        internal int ByteToCharPosition(int pos) {

            var line = Npp.Msg(SciMsg.SCI_LINEFROMPOSITION, new IntPtr(pos)).ToInt32();
            var byteStart = Npp.Msg(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(line)).ToInt32();
            var count = CharPositionFromLine(line) + GetCharCount(byteStart, pos - byteStart);

            return count;
        }

        /// <summary>
        /// Returns the number of CHARACTERS in a line.
        /// </summary>
        internal int CharLineLength(int index) {

            // A line's length is calculated by subtracting its start offset from
            // the start of the line following. We keep a terminal (faux) line at
            // the end of the list so we can calculate the length of the last line.

            if (index + 1 <= _stepLine)
                return _perLineData[index + 1].Start - _perLineData[index].Start;
            else if (index <= _stepLine)
                return (_perLineData[index + 1].Start + _stepLength) - _perLineData[index].Start;
            else
                return (_perLineData[index + 1].Start + _stepLength) - (_perLineData[index].Start + _stepLength);
        }

        /// <summary>
        /// Returns the CHARACTER offset where the line begins.
        /// </summary>
        internal int CharPositionFromLine(int index) {

            var start = _perLineData[index].Start;
            if (index > _stepLine)
                start += _stepLength;

            return start;
        }

        internal int CharToBytePosition(int pos) {

            // Adjust to the nearest line start
            var line = LineFromCharPosition(pos);
            var bytePos = Npp.Msg(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(line)).ToInt32();
            pos -= CharPositionFromLine(line);

            // Optimization when the line contains NO multibyte characters
            if (!LineContainsMultibyteChar(line))
                return (bytePos + pos);

            while (pos > 0) {
                // Move char-by-char
                bytePos = Npp.Msg(SciMsg.SCI_POSITIONRELATIVE, new IntPtr(bytePos), new IntPtr(1)).ToInt32();
                pos--;
            }

            return bytePos;
        }

        private void DeletePerLine(int index) {

            MoveStep(index);

            // Subtract the line length
            _stepLength -= CharLineLength(index);

            // Remove the line
            _perLineData.RemoveAt(index);

            // Move the step to the line before the one removed
            _stepLine--;
        }

        /// <summary>
        /// Gets the number of CHARACTERS int a BYTE range.
        /// </summary>
        private int GetCharCount(int pos, int length) {
            var ptr = Npp.Msg(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(pos), new IntPtr(length));
            return GetCharCount(ptr, length, Npp.Encoding);
        }

        /// <summary>
        /// Gets the number of CHARACTERS in a BYTE range.
        /// </summary>
        private static unsafe int GetCharCount(IntPtr text, int length, Encoding encoding) {
            if (text == IntPtr.Zero || length == 0)
                return 0;

            // Never use SCI_COUNTCHARACTERS. It counts CRLF as 1 char!
            var count = encoding.GetCharCount((byte*)text, length);
            return count;
        }

        private bool LineContainsMultibyteChar(int index) {
            var perLine = _perLineData[index];
            if (perLine.ContainsMultibyte == ContainsMultibyte.Unkown) {
                perLine.ContainsMultibyte =
                    (Npp.Msg(SciMsg.SCI_LINELENGTH, new IntPtr(index)).ToInt32() == CharLineLength(index))
                    ? ContainsMultibyte.No
                    : ContainsMultibyte.Yes;

                _perLineData[index] = perLine;
            }

            return (perLine.ContainsMultibyte == ContainsMultibyte.Yes);
        }

        /// <summary>
        /// Returns the line index containing the CHARACTER position.
        /// </summary>
        internal int LineFromCharPosition(int pos) {
            Debug.Assert(pos >= 0);

            // Iterative binary search
            // http://en.wikipedia.org/wiki/Binary_search_algorithm
            // System.Collections.Generic.ArraySortHelper.InternalBinarySearch

            var low = 0;
            var high = Count - 1;

            while (low <= high) {
                var mid = low + ((high - low) / 2);
                var start = CharPositionFromLine(mid);

                if (pos == start)
                    return mid;
                else if (start < pos)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            // After while exit, 'low' will point to the index where 'pos' should be
            // inserted (if we were creating a new line start). The line containing
            // 'pos' then would be 'low - 1'.
            return low - 1;
        }

        /// <summary>
        /// Tracks a new line with the given CHARACTER length.
        /// </summary>
        private void InsertPerLine(int index, int length = 0) {
            MoveStep(index);

            PerLine data;

            // Add the new line length to the existing line start
            data = _perLineData[index];
            var lineStart = data.Start;
            data.Start += length;
            _perLineData[index] = data;

            // Insert the new line
            data = new PerLine { Start = lineStart };
            _perLineData.Insert(index, data);

            // Move the step
            _stepLength += length;
            _stepLine++;
        }

        private void MoveStep(int line) {
            if (_stepLength == 0) {
                _stepLine = line;
            } else if (_stepLine < line) {
                PerLine data;
                while (_stepLine < line) {
                    _stepLine++;
                    data = _perLineData[_stepLine];
                    data.Start += _stepLength;
                    _perLineData[_stepLine] = data;
                }
            } else if (_stepLine > line) {
                PerLine data;
                while (_stepLine > line) {
                    data = _perLineData[_stepLine];
                    data.Start -= _stepLength;
                    _perLineData[_stepLine] = data;
                    _stepLine--;
                }
            }
        }

        internal void RebuildLineData() {
            _stepLine = 0;
            _stepLength = 0;

            _perLineData = new GapBuffer<PerLine> {new PerLine {Start = 0}, new PerLine {Start = 0}};
            // Terminal

            // Fake an insert notification
            var scn = new SCNotification {
                linesAdded = Npp.Msg(SciMsg.SCI_GETLINECOUNT).ToInt32() - 1,
                position = 0,
                length = Npp.Msg(SciMsg.SCI_GETLENGTH).ToInt32()
            };
            scn.text = Npp.Msg(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(scn.position), new IntPtr(scn.length));
            TrackInsertText(scn);
        }

        public void ScnModified(SCNotification scn) {
            if ((scn.modificationType & (int)SciMsg.SC_MOD_DELETETEXT) > 0) {
                TrackDeleteText(scn);
            }

            if ((scn.modificationType & (int)SciMsg.SC_MOD_INSERTTEXT) > 0) {
                TrackInsertText(scn);
            }
        }

        private void TrackDeleteText(SCNotification scn) {
            var startLine = Npp.Msg(SciMsg.SCI_LINEFROMPOSITION, new IntPtr(scn.position)).ToInt32();
            if (scn.linesAdded == 0) {
                // That was easy
                var delta = GetCharCount(scn.text, scn.length, Npp.Encoding);
                AdjustLineLength(startLine, delta * -1);
            } else {
                // Adjust the existing line
                var lineByteStart = Npp.Msg(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(startLine)).ToInt32();
                var lineByteLength = Npp.Msg(SciMsg.SCI_LINELENGTH, new IntPtr(startLine)).ToInt32();
                AdjustLineLength(startLine, GetCharCount(lineByteStart, lineByteLength) - CharLineLength(startLine));

                var linesRemoved = scn.linesAdded * -1;
                for (int i = 0; i < linesRemoved; i++) {
                    // Deleted line
                    DeletePerLine(startLine + 1);
                }
            }
        }

        private void TrackInsertText(SCNotification scn) {
            var startLine = Npp.Msg(SciMsg.SCI_LINEFROMPOSITION, new IntPtr(scn.position)).ToInt32();
            if (scn.linesAdded == 0) {
                // That was easy
                var delta = GetCharCount(scn.position, scn.length);
                AdjustLineLength(startLine, delta);
            } else {
                // Adjust existing line
                var lineByteStart = Npp.Msg(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(startLine)).ToInt32();
                var lineByteLength = Npp.Msg(SciMsg.SCI_LINELENGTH, new IntPtr(startLine)).ToInt32();
                AdjustLineLength(startLine, GetCharCount(lineByteStart, lineByteLength) - CharLineLength(startLine));

                for (int i = 1; i <= scn.linesAdded; i++) {
                    var line = startLine + i;

                    // Insert new line
                    lineByteStart += lineByteLength;
                    lineByteLength = Npp.Msg(SciMsg.SCI_LINELENGTH, new IntPtr(line)).ToInt32();
                    InsertPerLine(line, GetCharCount(lineByteStart, lineByteLength));
                }
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets a value indicating whether all the document lines are visible (not hidden).
        /// </summary>
        /// <returns>true if all the lines are visible; otherwise, false.</returns>
        public bool AllLinesVisible {
            get {
                return (Npp.Msg(SciMsg.SCI_GETALLLINESVISIBLE) != IntPtr.Zero);
            }
        }

        /// <summary>
        /// Gets the number of lines.
        /// </summary>
        /// <returns>The number of lines</returns>
        public int Count {
            get {
                // Subtract the terminal line
                return (_perLineData.Count - 1);
            }
        }

        /// <summary>
        /// Gets the number of CHARACTERS in the document.
        /// </summary>
        internal int TextLength {
            get {
                // Where the terminal line begins
                return CharPositionFromLine(_perLineData.Count - 1);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public DocumentLines() {
            _perLineData = new GapBuffer<PerLine> { new PerLine { Start = 0 }, new PerLine { Start = 0 } };
            // Terminal
        }

        #endregion Constructors

        #region Types

        /// <summary>
        /// Stuff we track for each line.
        /// </summary>
        private struct PerLine {
            /// <summary>
            /// The CHARACTER position where the line begins.
            /// </summary>
            public int Start;

            /// <summary>
            /// 1 if the line contains multibyte (Unicode) characters; -1 if not; 0 if undetermined.
            /// </summary>
            /// <remarks>Using an enum instead of Nullable because it uses less memory per line...</remarks>
            public ContainsMultibyte ContainsMultibyte;
        }

        private enum ContainsMultibyte {
            No = -1,
            Unkown,
            Yes
        }

        #endregion Types

    }

    /// <summary>
    /// This class is a rip off from https://github.com/jacobslusser/ScintillaNET/!
    /// Freaking awesome work, i just adapted it to make it work with Npp's scintilla
    /// </summary>
    internal sealed class GapBuffer<T> : IEnumerable<T> {
        private T[] _buffer;
        private int _gapStart;
        private int _gapEnd;

        public void Add(T item) {
            Insert(Count, item);
        }

        public void AddRange(ICollection<T> collection) {
            InsertRange(Count, collection);
        }

        private void EnsureGapCapacity(int length) {
            if (length > (_gapEnd - _gapStart)) {
                // How much to grow the buffer is a tricky question.
                // Our current algo will double the capacity unless that's not enough.
                var minCapacity = Count + length;
                var newCapacity = (_buffer.Length * 2);
                if (newCapacity < minCapacity) {
                    newCapacity = minCapacity;
                }

                var newBuffer = new T[newCapacity];
                var newGapEnd = newBuffer.Length - (_buffer.Length - _gapEnd);

                Array.Copy(_buffer, 0, newBuffer, 0, _gapStart);
                Array.Copy(_buffer, _gapEnd, newBuffer, newGapEnd, newBuffer.Length - newGapEnd);
                _buffer = newBuffer;
                _gapEnd = newGapEnd;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            var count = Count;
            for (int i = 0; i < count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Insert(int index, T item) {
            PlaceGapStart(index);
            EnsureGapCapacity(1);

            _buffer[index] = item;
            _gapStart++;
        }

        public void InsertRange(int index, ICollection<T> collection) {
            var count = collection.Count;
            if (count > 0) {
                PlaceGapStart(index);
                EnsureGapCapacity(count);

                collection.CopyTo(_buffer, _gapStart);
                _gapStart += count;
            }
        }

        private void PlaceGapStart(int index) {
            if (index != _gapStart) {
                if ((_gapEnd - _gapStart) == 0) {
                    // There is no gap
                    _gapStart = index;
                    _gapEnd = index;
                } else if (index < _gapStart) {
                    // Move gap left (copy contents right)
                    var length = (_gapStart - index);
                    var deltaLength = (_gapEnd - _gapStart < length ? _gapEnd - _gapStart : length);
                    Array.Copy(_buffer, index, _buffer, _gapEnd - length, length);
                    _gapStart -= length;
                    _gapEnd -= length;

                    Array.Clear(_buffer, index, deltaLength);
                } else {
                    // Move gap right (copy contents left)
                    var length = (index - _gapStart);
                    var deltaIndex = (index > _gapEnd ? index : _gapEnd);
                    Array.Copy(_buffer, _gapEnd, _buffer, _gapStart, length);
                    _gapStart += length;
                    _gapEnd += length;

                    Array.Clear(_buffer, deltaIndex, _gapEnd - deltaIndex);
                }
            }
        }

        public void RemoveAt(int index) {
            PlaceGapStart(index);
            _buffer[_gapEnd] = default(T);
            _gapEnd++;
        }

        public void RemoveRange(int index, int count) {
            if (count > 0) {
                PlaceGapStart(index);
                Array.Clear(_buffer, _gapEnd, count);
                _gapEnd += count;
            }
        }

        public int Count {
            get {
                return _buffer.Length - (_gapEnd - _gapStart);
            }
        }

        public T this[int index] {
            get {
                if (index < _gapStart)
                    return _buffer[index];

                return _buffer[index + (_gapEnd - _gapStart)];
            }
            set {
                if (index >= _gapStart)
                    index += (_gapEnd - _gapStart);

                _buffer[index] = value;
            }
        }

        public GapBuffer(int capacity = 0) {
            _buffer = new T[capacity];
            _gapEnd = _buffer.Length;
        }
    }
}


