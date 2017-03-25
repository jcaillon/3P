#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (StringContentPart.cs) is part of 3P.
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
namespace _3PA.Lib.CommonMark.Syntax {
    /// <summary>
    /// Represents a part of <see cref="StringContent"/>.
    /// </summary>
    internal struct StringPart {
        public StringPart(string source, int startIndex, int length) {
            Source = source;
            StartIndex = startIndex;
            Length = length;
        }

        /// <summary>
        /// Gets or sets the string object this part is created from.
        /// </summary>
        public string Source;

        /// <summary>
        /// Gets or sets the index at which this part starts.
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// Gets or sets the length of the part.
        /// </summary>
        public int Length;

        public override string ToString() {
            if (Source == null)
                return null;

            return Source.Substring(StartIndex, Length);
        }
    }
}