#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (PositionOffset.cs) is part of 3P.
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

namespace _3PA.Lib.CommonMark.Parser {
    internal struct PositionOffset {
        public PositionOffset(int position, int offset) {
            Position = position;
            Offset = offset;
        }

        public int Position;
        public int Offset;

        public override string ToString() {
            if (Offset == 0)
                return "empty";

            return (Offset < 0 ? string.Empty : "+")
                   + Offset.ToString(System.Globalization.CultureInfo.InvariantCulture)
                   + " chars @ " + Position.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}