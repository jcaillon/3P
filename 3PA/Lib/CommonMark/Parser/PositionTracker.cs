#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (PositionTracker.cs) is part of 3P.
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

namespace _3PA.Lib.CommonMark.Parser {
    internal sealed class PositionTracker {
        public PositionTracker(int blockOffset) {
            _blockOffset = blockOffset;
        }

        private static readonly PositionOffset EmptyPositionOffset = default(PositionOffset);

        private int _blockOffset;

        public void AddBlockOffset(int offset) {
            _blockOffset += offset;
        }

        public void AddOffset(LineInfo line, int startIndex, int length) {
            if (OffsetCount + line.OffsetCount + 2 >= Offsets.Length)
                Array.Resize(ref Offsets, Offsets.Length + line.OffsetCount + 20);

            PositionOffset po1, po2;

            if (startIndex > 0)
                po1 = new PositionOffset(
                    CalculateOrigin(line.Offsets, line.OffsetCount, line.LineOffset, false, true),
                    startIndex);
            else
                po1 = EmptyPositionOffset;

            if (line.Line.Length - startIndex - length > 0)
                po2 = new PositionOffset(
                    CalculateOrigin(line.Offsets, line.OffsetCount, line.LineOffset + startIndex + length, false, true),
                    line.Line.Length - startIndex - length);
            else
                po2 = EmptyPositionOffset;

            var indexAfterLastCopied = 0;

            if (po1.Offset == 0) {
                if (po2.Offset == 0)
                    goto FINTOTAL;

                po1 = po2;
                po2 = EmptyPositionOffset;
            }

            for (var i = 0; i < line.OffsetCount; i++) {
                var pc = line.Offsets[i];
                if (pc.Position > po1.Position) {
                    if (i > indexAfterLastCopied) {
                        Array.Copy(line.Offsets, indexAfterLastCopied, Offsets, OffsetCount, i - indexAfterLastCopied);
                        OffsetCount += i - indexAfterLastCopied;
                        indexAfterLastCopied = i;
                    }

                    Offsets[OffsetCount++] = po1;

                    po1 = po2;

                    if (po1.Offset == 0)
                        goto FIN;

                    po2 = EmptyPositionOffset;
                }
            }

            FIN:
            if (po1.Offset != 0)
                Offsets[OffsetCount++] = po1;

            if (po2.Offset != 0)
                Offsets[OffsetCount++] = po2;

            FINTOTAL:
            Array.Copy(line.Offsets, indexAfterLastCopied, Offsets, OffsetCount, line.OffsetCount - indexAfterLastCopied);
            OffsetCount += line.OffsetCount - indexAfterLastCopied;
        }

        public int CalculateInlineOrigin(int position, bool isStartPosition) {
            return CalculateOrigin(Offsets, OffsetCount, _blockOffset + position, true, isStartPosition);
        }

        internal static int CalculateOrigin(PositionOffset[] offsets, int offsetCount, int position, bool includeReduce, bool isStart) {
            if (isStart)
                position++;

            var minus = 0;
            for (var i = 0; i < offsetCount; i++) {
                var po = offsets[i];
                if (po.Position < position) {
                    if (po.Offset > 0)
                        position += po.Offset;
                    else
                        minus += po.Offset;
                } else
                    break;
            }

            if (includeReduce)
                position += minus;

            if (isStart)
                position--;

            return position;
        }

        private PositionOffset[] Offsets = new PositionOffset[10];
        private int OffsetCount;
    }
}