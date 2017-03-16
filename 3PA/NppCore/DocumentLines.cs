#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (DocumentLines.cs) is part of 3P.
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
using System.Diagnostics;
using System.Text;
using YamuiFramework.Helper;
using _3PA.MainFeatures;

namespace _3PA.NppCore {

    /// <summary>
    /// For every scintilla message that involves a position, the expect position (expected by scintilla) is the
    /// BYTE position, not the CHAR position (as anyone would assume at first!)
    /// This class enables you to easily get the correspondence between a BYTE position and a CHAR position,
    /// it keeps tracks of inserted/deleted lines and register each line's start position, this
    /// information allows us to quickly convert BYTE to CHAR position and vice-versa
    /// </summary>
    /// <remarks>
    /// For OnInsertedText/OnDeletedText it would make sense to directly use scn.text instead of getting a range
    /// pointer from scintilla (in GetCharCount(int, int)); but this would imply two things: we would need to 
    /// split the scn.text byte[] into a byte[] for each line since we need the length of each line + we would
    /// need to also store the starting BYTE position of each line... So i didn't do that; 500ms to reset the lines
    /// on a 200Mo document sounds good enough to me...
    /// </remarks>
    internal class DocumentLines {

        #region Fields

        /// <summary>
        /// This is basically an array of int (except that we don't a List or Array, see GapBuffer for more details)
        /// We store the starting CHAR position of each line of the document 
        /// A line's length is calculated by subtracting its start position from the start of the following line
        /// thus, a 'phantom' line is added at the end of the document so we can know the length of the last line as well
        /// </summary>
        private GapBuffer<int> _linesList;

        private Encoding _lastEncoding;
        private bool _oneByteCharEncoding;

        /// <summary>
        /// When we insert/delete 1 or x char in a given line, we don't want to immediately update the 
        /// starting char position of all the following lines (because adding 1 char is called every time we press
        /// a key!!), so instead we allow to have a 'hole' in our lines info, we remember on which the hole is
        /// and its length, so we can compute everything accordingly
        /// </summary>
        private int _holeLine;
        private int _holeLenght;

        #endregion

        #region Life and death

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public DocumentLines() {
            Init();
        }

        /// <summary>
        /// Reset the lines list
        /// </summary>
        private void Init() {
            _linesList = new GapBuffer<int> { 0, 0 };
            _holeLine = 0;
            _holeLenght = 0;
            _oneByteCharEncoding = false;
        }

        #endregion

        #region Private API

        /// <summary>
        /// Instance of scintilla, the class that allows communication with the current scintilla
        /// </summary>
        private static SciApi Api {
            get { return Npp.CurrentSci; }
        }

        #endregion

        #region Public utils

        /// <summary>
        /// Simulates the insertion of the whole text, use this to reset the lines info 
        /// (when switching document for instance)
        /// </summary>
        public void Reset() {
            Init();
            var scn = new SCNotification {
                linesAdded = SciGetLineCount() - 1,
                position = 0,
                length = SciGetLength()
            };
            scn.text = Api.Send(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(scn.position), new IntPtr(scn.length));
            OnScnModified(scn, true);
        }

        /// <summary>
        /// makes sure that the nb of lines from the API and calculed in the this class are the same
        /// </summary>
        public void CheckLines() {
            if (_linesList.Count - 1 != SciGetLineCount()) {
                ErrorHandler.LogError(new Exception("DocumentLines, nb lines failed"), (_linesList.Count - 1) + " vs " + SciGetLineCount());
                Reset();
            }
        }

        #endregion

        #region Register document modifications

        /// <summary>
        /// When receiving a modification notification by scintilla
        /// </summary>
        public void OnScnModified(SCNotification scn, bool isInsertion) {
            _lastEncoding = Sci.Encoding;
            //_oneByteCharEncoding = _lastEncoding.Equals(Encoding.Default);
            _oneByteCharEncoding = false; // for 1.7.4

            // bypass the hard work for simple encoding
            if (_oneByteCharEncoding)
                return;

            if (isInsertion) {
                OnInsertedText(scn);
            } else {
                OnDeletedText(scn);
            }
        }

        /// <summary>
        /// updates the line info when deleting text
        /// </summary>
        /// <param name="scn"></param>
        private void OnDeletedText(SCNotification scn) {
            var startLine = SciLineFromPosition(scn.position);
            if (scn.linesAdded == 0) {
                var delCharLenght = GetCharCount(scn.text, scn.length);
                SetHoleInLine(startLine, -delCharLenght);
            } else {
                var lineByteStart = SciPositionFromLine(startLine);
                var lineByteLength = SciLineLength(startLine);
                var delCharLenght = -(GetCharCount(lineByteStart, lineByteLength) - LineCharLength(startLine));
                FillTheHole();
                for (int i = 0; i < -scn.linesAdded; i++) {
                    delCharLenght += LineCharLength(startLine + 1);
                    _linesList.RemoveAt(startLine + 1);
                }
                SetHoleInLine(startLine, -delCharLenght);
                FillTheHole();
            }
        }

        /// <summary>
        /// Updates the line info when inserting text
        /// </summary>
        /// <param name="scn"></param>
        private void OnInsertedText(SCNotification scn) {
            var startLine = SciLineFromPosition(scn.position);
            if (scn.linesAdded == 0) {
                var insCharLenght = GetCharCount(scn.text, scn.length);
                SetHoleInLine(startLine, insCharLenght);
            } else {
                var startCharPos = CharPositionFromLine(startLine);
                var lineByteStart = SciPositionFromLine(startLine);
                var lineByteLength = SciLineLength(startLine);
                var lineCharLenght = GetCharCount(lineByteStart, lineByteLength);
                var insCharLenght = lineCharLenght - LineCharLength(startLine);
                FillTheHole();
                for (int i = 0; i < scn.linesAdded; i++) {
                    startCharPos += lineCharLenght;
                    var line = startLine + i + 1;
                    lineByteStart += lineByteLength;
                    lineByteLength = SciLineLength(line);
                    lineCharLenght = GetCharCount(lineByteStart, lineByteLength);
                    insCharLenght += lineCharLenght;
                    _linesList.Insert(line, startCharPos);
                }
                SetHoleInLine(startLine + scn.linesAdded, insCharLenght);
                FillTheHole();

                // We should not have a null length, but we actually can :
                // when a file is modified outside npp, npp suggests to reload it, a modified notification is sent
                // but is it sent BEFORE the text is actually put into scintilla! So what we do here doesn't work at all
                // so in that case, we need to refresh the info when the text is actually inserted, that is after updateui
                // Clarification : the notification sent is correct (nb lines > 0, length, text are ok), but calling SciLineLength
                // will always return 0 at this moment!
                if (scn.length > 0 && TextLength == 0)
                    Plug.ActionsAfterUpdateUi.Enqueue(Reset);
            }
        }

        /// <summary>
        /// Creates a hole in a given line (charLength is added to the existing _holeLenght)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="charLength"></param>
        private void SetHoleInLine(int line, int charLength) {
            // since a hole is only for one line and there can be only one hole, fill the previous hole if it exists
            if (line != _holeLine) {
                FillTheHole();
            }
            _holeLine = line;
            _holeLenght += charLength;
        }

        /// <summary>
        /// Fixes the hole we created in a line by correcting the start char position or all the following lines
        /// (by the amount of _holeLenght) and then rests the _holeLenght
        /// </summary>
        private void FillTheHole() {
            // is there even a hole?
            if (_holeLenght == 0)
                return;
            var totalNbLines = _linesList.Count;
            for (int i = _holeLine + 1; i < totalNbLines; i++) {
                _linesList[i] += _holeLenght;
            }
            _holeLenght = 0;
        }

        #endregion

        #region public

        /// <summary>
        /// Gets the number of lines
        /// </summary>
        /// <returns>The number of lines</returns>
        public int Count {
            get {
                // bypass the hard work for simple encoding
                if (_oneByteCharEncoding)
                    return SciGetLineCount();

                return _linesList.Count - 1;
            }
        }

        /// <summary>
        /// Gets the number of CHAR in the document
        /// </summary>
        internal int TextLength {
            get {
                // bypass the hard work for simple encoding
                if (_oneByteCharEncoding)
                    return SciGetLength();

                // text lenght is the start pos of the last "phantom" line
                return CharPositionFromLine(_linesList.Count - 1);
            }
        }

        /// <summary>
        /// Returns the CHAR position where the line begins,
        /// this is THE method of this class (since it is the only info we keep on the lines!)
        /// </summary>
        public int CharPositionFromLine(int index) {
            if (index == 0)
                return 0;

            // bypass the hard work for simple encoding
            if (_oneByteCharEncoding)
                return SciPositionFromLine(index);

            try {
                return PrivateCharPositionFromLine(index);
            } catch (Exception e) {
                try {
                    if (index < 0 || index >= _linesList.Count) {
                        ErrorHandler.LogError(e, "INDEX ERROR : " + index + GetDebugString());
                        index = index.Clamp(0, _linesList.Count - 1);
                    }
                    Reset();
                    if (_oneByteCharEncoding)
                        return SciPositionFromLine(index);
                    return PrivateCharPositionFromLine(index);
                } catch (Exception x) {
                    ErrorHandler.LogError(x, "FAILED TO RESET DocumentLines for " + GetDebugString());
                    if (Config.IsDevelopper)
                        Debug.Assert(false);
                }
                return SciPositionFromLine(index);
            }
        }

        /// <summary>
        /// Returns the number of CHAR in a line.
        /// </summary>
        public int LineCharLength(int index) {
            // bypass the hard work for simple encoding
            if (_oneByteCharEncoding)
                return SciLineLength(index);

            return CharPositionFromLine(index + 1) - CharPositionFromLine(index);
        }

        /// <summary>
        /// Returns the line index containing the CHAR position.
        /// </summary>
        public int LineFromCharPosition(int pos) {
            // bypass the hard work for simple encoding
            if (_oneByteCharEncoding)
                return SciLineFromPosition(pos);

            // Dichotomic algo to find the line containing the char pos
            var low = 0;
            var high = Count - 1;
            while (low <= high) {
                var mid = low + ((high - low)/2);
                var start = CharPositionFromLine(mid);
                if (pos == start)
                    return mid;
                if (start < pos)
                    low = mid + 1;
                else
                    high = mid - 1;
            }
            return low - 1;
        }

        /// <summary>
        /// Converts a CHAR position to a BYTE position
        /// </summary>
        public int CharToBytePosition(int pos) {
            // bypass the hard work for simple encoding
            if (_oneByteCharEncoding)
                return pos;

            // nearest line start
            var line = LineFromCharPosition(pos);
            var byteStartPos = SciPositionFromLine(line);
            var posInLine = pos - CharPositionFromLine(line);

            // The lines contains as much BYTES as CHAR
            if (SciLineLength(line) == LineCharLength(line))
                return (byteStartPos + posInLine);

            while (posInLine > 0) {
                // Move char-by-char, Count a number of whole CHAR before or after the argument position and return that position
                byteStartPos = SciPositionRelative(byteStartPos, 1);
                posInLine--;
            }

            return byteStartPos;
        }

        /// <summary>
        /// Converts a BYTE position to a CHAR position.
        /// </summary>
        public int ByteToCharPosition(int pos) {
            // bypass the hard work for simple encoding
            if (_oneByteCharEncoding)
                return pos;

            var line = SciLineFromPosition(pos);
            var byteStart = SciPositionFromLine(line);
            var count = CharPositionFromLine(line) + GetCharCount(byteStart, pos - byteStart);
            return count;
        }

        #endregion Methods

        #region private methods

        /// <summary>
        /// Returns the CHAR position where the line begins,
        /// this is THE SINGLE MOST IMPORTANT method of this class 
        /// (since it is the only info we keep on the lines!)
        /// </summary>
        private int PrivateCharPositionFromLine(int index) {
            if (_holeLenght != 0 && index > _holeLine) {
                return _linesList[index] + _holeLenght;
            }
            return _linesList[index];
        }

        /// <summary>
        /// Returns the document lenght in BYTES
        /// </summary>
        /// <returns></returns>
        private int SciGetLength() {
            return Api.Send(SciMsg.SCI_GETLENGTH).ToInt32();
        }

        /// <summary>
        /// Count a number of whole characters before or after the argument position and return that position
        /// The minimum position returned is 0 and the maximum is the last position in the document
        /// </summary>
        /// <returns></returns>
        private int SciPositionRelative(int position, int nb) {
            return Api.Send(SciMsg.SCI_POSITIONRELATIVE, new IntPtr(position), new IntPtr(nb)).ToInt32();
        }

        /// <summary>
        /// returns the number of lines in the document
        /// An empty document has 1 line, a document with only one \n has 2 lines
        /// </summary>
        /// <returns></returns>
        private int SciGetLineCount() {
            return Api.Send(SciMsg.SCI_GETLINECOUNT).ToInt32();
        }

        /// <summary>
        /// returns the length (nb of BYTE) of the line, including any line end CHARs
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private int SciLineLength(int line) {
            return Api.Send(SciMsg.SCI_LINELENGTH, new IntPtr(line)).ToInt32();
        }

        /// <summary>
        /// returns the line that contains the BYTE position pos in the document
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int SciLineFromPosition(int pos) {
            return pos == 0 ? 0 : Api.Send(SciMsg.SCI_LINEFROMPOSITION, new IntPtr(pos)).ToInt32();
        }

        /// <summary>
        /// returns the document BYTE position that corresponds with the start of the line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private int SciPositionFromLine(int line) {
            return line == 0 ? 0 : Api.Send(SciMsg.SCI_POSITIONFROMLINE, new IntPtr(line)).ToInt32();
        }

        /// <summary>
        /// Gets the number of CHAR int a BYTE range
        /// </summary>
        private int GetCharCount(int pos, int length) {
            // don't use SCI_COUNTCHAR, it counts CRLF as 1 char
            var ptr = Api.Send(SciMsg.SCI_GETRANGEPOINTER, new IntPtr(pos), new IntPtr(length));
            return GetCharCount(ptr, length);
        }

        /// <summary>
        /// Gets the number of CHAR in a BYTE range
        /// </summary>
        private unsafe int GetCharCount(IntPtr text, int length) {
            if (text == IntPtr.Zero || length == 0)
                return 0;
            return _lastEncoding.GetCharCount((byte*)text, length);
        }

        private string GetDebugString() {
            return "\r\n_holeLenght = " + _holeLenght +
                   "\r\n_holeLine = " + _holeLine +
                   "\r\n_linesList.Count = " + _linesList.Count +
                   "\r\nSciGetLineCount = " + SciGetLineCount() +
                   "\r\nSciGetLength = " + SciGetLength() +
                   "\r\nTextLength = " + TextLength + "<br>" +
                   "\r\nSciLineFromPosition(SciGetLength()) = " + SciLineFromPosition(SciGetLength()) +
                   "\r\nLineFromCharPosition(TextLength) = " + LineFromCharPosition(TextLength) +
                   "\r\nCurrentPosition = " + Sci.CurrentPosition +
                   "\r\nSCI_GETCURRENTPOS = " + Api.Send(SciMsg.SCI_GETCURRENTPOS).ToInt32() +
                   "\r\nSciPositionFromLine(SciGetLineCount()) = " + SciPositionFromLine(SciGetLineCount()) +
                   "\r\nCharPositionFromLine(SciGetLineCount()) = " + CharPositionFromLine(SciGetLineCount());
        }

        #endregion
        
        #region To use the scn.text directly?
        /*
        private unsafe int GetCharCount(IntPtr text, int pos, int lenght) {
            var bytes = (byte*) text;
            int lgth = lenght - pos;
            byte[] arrbyte = new byte[lgth];
            int index;
            for (index = 0; index < lgth; index++)
                arrbyte[index] = bytes[index + pos];
            return _lastEncoding.GetCharCount(arrbyte, 0, lgth);
        }

        public unsafe byte[][] Separate(IntPtr text, int lenght) {
            var bytes = (byte*) text;
            int lgth = lenght;
            byte[] arrbyte = new byte[lgth];
            int index;
            for (index = 0; index < lgth; index++)
                arrbyte[index] = bytes[index];
            return Separate(arrbyte, _lastEncoding.GetBytes(new[] {'\n'}));
        }

        public byte[][] Separate(byte[] source, byte[] separator) {
            var Parts = new List<byte[]>();
            var Index = 0;
            byte[] Part;
            for (var I = 0; I < source.Length; ++I) {
                if (Equals(source, separator, I)) {
                    Part = new byte[I - Index];
                    Array.Copy(source, Index, Part, 0, Part.Length);
                    Parts.Add(Part);
                    Index = I + separator.Length;
                    I += separator.Length - 1;
                }
            }
            Part = new byte[source.Length - Index];
            Array.Copy(source, Index, Part, 0, Part.Length);
            Parts.Add(Part);
            return Parts.ToArray();
        }

        bool Equals(byte[] source, byte[] separator, int index) {
            for (int i = 0; i < separator.Length; ++i)
                if (index + i >= source.Length || source[index + i] != separator[i])
                    return false;
            return true;
        }
        */
        #endregion
        
    }
}