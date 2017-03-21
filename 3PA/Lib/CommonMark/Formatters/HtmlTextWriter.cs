#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlTextWriter.cs) is part of 3P.
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

using System.IO;
using _3PA.Lib.CommonMark.Syntax;

namespace _3PA.Lib.CommonMark.Formatters {
    /// <summary>
    /// A wrapper for <see cref="HtmlFormatterSlim"/> that keeps track if the last symbol has been a newline.
    /// </summary>
    internal sealed class HtmlTextWriter {
        private readonly TextWriter _inner;
        private char _last = '\n';
        private readonly bool _windowsNewLine;
        private readonly char[] _newline;

        /// <summary>
        /// A reusable char buffer. This is used internally in <see cref="Write(StringPart)"/> (and thus will modify the buffer)
        /// but can also be used from <see cref="HtmlFormatterSlim"/> class.
        /// </summary>
        internal char[] Buffer = new char[256];

        public HtmlTextWriter(TextWriter inner) {
            _inner = inner;

            var nl = inner.NewLine;
            _newline = nl.ToCharArray();
            _windowsNewLine = nl == "\r\n";
        }

        public void WriteLine() {
            _inner.Write(_newline);
            _last = '\n';
        }

        public void WriteLine(char data) {
            if (data == '\n' && _windowsNewLine && _last != '\r')
                _inner.Write('\r');

            _inner.Write(data);
            _inner.Write(_newline);
            _last = '\n';
        }

        public void Write(StringPart value) {
            if (value.Length == 0)
                return;

            if (Buffer.Length < value.Length)
                Buffer = new char[value.Length];

            value.Source.CopyTo(value.StartIndex, Buffer, 0, value.Length);

            if (_windowsNewLine) {
                var lastPos = value.StartIndex;
                var pos = lastPos;

                while (-1 != (pos = value.Source.IndexOf('\n', pos, value.Length - pos + value.StartIndex))) {
                    var lastC = pos == 0 ? _last : value.Source[pos - 1];

                    if (lastC != '\r') {
                        _inner.Write(Buffer, lastPos - value.StartIndex, pos - lastPos);
                        _inner.Write('\r');
                        lastPos = pos;
                    }

                    pos++;
                }

                _inner.Write(Buffer, lastPos - value.StartIndex, value.Length - lastPos + value.StartIndex);
            } else {
                _inner.Write(Buffer, 0, value.Length);
            }

            _last = Buffer[value.Length - 1];
        }

        /// <summary>
        /// Writes a value that is known not to contain any newlines.
        /// </summary>
        public void WriteConstant(char[] value) {
            _last = 'c';
            _inner.Write(value, 0, value.Length);
        }

        /// <summary>
        /// Writes a value that is known not to contain any newlines.
        /// </summary>
        public void WriteConstant(char[] value, int startIndex, int length) {
            _last = 'c';
            _inner.Write(value, startIndex, length);
        }

        /// <summary>
        /// Writes a value that is known not to contain any newlines.
        /// </summary>
        public void WriteConstant(string value) {
            _last = 'c';
            _inner.Write(value);
        }

        /// <summary>
        /// Writes a value that is known not to contain any newlines.
        /// </summary>
        public void WriteLineConstant(string value) {
            _last = '\n';
            _inner.Write(value);
            _inner.Write(_newline);
        }

        public void Write(char[] value, int index, int count) {
            if (value == null || count == 0)
                return;

            if (_windowsNewLine) {
                var lastPos = index;
                var pos = index;

                while (pos < index + count) {
                    if (value[pos] != '\n') {
                        pos++;
                        continue;
                    }

                    var lastC = pos == index ? _last : value[pos - 1];

                    if (lastC != '\r') {
                        _inner.Write(value, lastPos, pos - lastPos);
                        _inner.Write('\r');
                        lastPos = pos;
                    }

                    pos++;
                }

                _inner.Write(value, lastPos, index + count - lastPos);
            } else {
                _inner.Write(value, index, count);
            }

            _last = value[index + count - 1];
        }

        public void Write(char value) {
            if (value == '\n' && _windowsNewLine && _last != '\r')
                _inner.Write('\r');

            _last = value;
            _inner.Write(value);
        }

        /// <summary>
        /// Adds a newline if the writer does not currently end with a newline.
        /// </summary>
        public void EnsureLine() {
            if (_last != '\n')
                WriteLine();
        }
    }
}