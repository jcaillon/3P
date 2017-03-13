#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (TabTextReader.cs) is part of 3P.
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
using System.Text;

namespace _3PA.Lib.CommonMark.Parser {
    internal sealed class TabTextReader {
        private const int _bufferSize = 4000;
        private readonly TextReader _inner;
        private readonly char[] _buffer;
        private int _bufferLength;
        private int _bufferPosition;
        private int _previousBufferLength;
        private readonly StringBuilder _builder;
        private bool _endOfStream;

        public TabTextReader(TextReader inner) {
            _inner = inner;
            _buffer = new char[_bufferSize];
            _builder = new StringBuilder(256);
        }

        private bool ReadBuffer() {
            if (_endOfStream)
                return false;

            _previousBufferLength += _bufferLength;
            _bufferLength = _inner.Read(_buffer, 0, _bufferSize);
            _endOfStream = _bufferLength == 0;
            _bufferPosition = 0;
            return !_endOfStream;
        }

        public void ReadLine(LineInfo line) {
            line.LineOffset = _previousBufferLength + _bufferPosition;
            line.LineNumber++;
            line.OffsetCount = 0;
            line.Line = null;
            var tabIncreaseCount = 0;

            if (_bufferPosition == _bufferLength && !ReadBuffer())
                return;

            bool useBuilder = false;
            int num;
            char c;

            while (true) {
                num = _bufferPosition;
                do {
                    c = _buffer[num];
                    if (c == '\r' || c == '\n')
                        goto IL_4A;

                    if (c == '\0')
                        _buffer[num] = '\uFFFD';

                    num++;
                } while (num < _bufferLength);

                num = _bufferLength - _bufferPosition;
                if (!useBuilder) {
                    useBuilder = true;
                    _builder.Length = 0;
                }

                _builder.Append(_buffer, _bufferPosition, num);
                if (!ReadBuffer()) {
                    _builder.Append('\n');
                    line.Line = _builder.ToString();
                    return;
                }
            }

            IL_4A:
            string result;
            _buffer[num] = '\n';
            if (useBuilder) {
                _builder.Append(_buffer, _bufferPosition, num - _bufferPosition + 1);
                result = _builder.ToString();
            } else {
                result = new string(_buffer, _bufferPosition, num - _bufferPosition + 1);
            }

            _bufferPosition = num + 1;

            if (c == '\r' && (_bufferPosition < _bufferLength || ReadBuffer()) && _buffer[_bufferPosition] == '\n') {
                if (line.IsTrackingPositions)
                    line.AddOffset(_previousBufferLength + _bufferPosition - 1 + tabIncreaseCount, 1);

                _bufferPosition++;
            }

            line.Line = result;
        }

        public bool EndOfStream() {
            return _endOfStream || (_bufferPosition == _bufferLength && !ReadBuffer());
        }
    }
}