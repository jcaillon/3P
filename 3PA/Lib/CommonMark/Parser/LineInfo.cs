#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (LineInfo.cs) is part of 3P.
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
    internal sealed class LineInfo {
        public LineInfo(bool trackPositions) {
            IsTrackingPositions = trackPositions;
        }

        public readonly bool IsTrackingPositions;

        public string Line;

        /// <summary>
        /// Gets or sets the offset in the source data at which the current line starts.
        /// </summary>
        public int LineOffset;

        public int LineNumber;

        public PositionOffset[] Offsets = new PositionOffset[20];

        public int OffsetCount;

        public void AddOffset(int position, int offset) {
            if (offset == 0)
                return;

            if (OffsetCount == Offsets.Length)
                Array.Resize(ref Offsets, OffsetCount + 20);

            Offsets[OffsetCount++] = new PositionOffset(position, offset);
        }

        public override string ToString() {
            string ln;
            if (Line == null)
                ln = string.Empty;
            else if (Line.Length < 50)
                ln = Line;
            else
                ln = Line.Substring(0, 49) + "…";

            return LineNumber.ToString(System.Globalization.CultureInfo.InvariantCulture)
                   + ": " + ln;
        }

        public int CalculateOrigin(int position, bool isStartPosition) {
            return PositionTracker.CalculateOrigin(Offsets, OffsetCount, LineOffset + position, true, isStartPosition);
        }
    }
}