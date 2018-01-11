#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (FencedCodeData.cs) is part of 3P.
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
    /// Contains additional data for fenced code blocks. Used in <see cref="Block.FencedCodeData"/>/
    /// </summary>
    public sealed class FencedCodeData {
        /// <summary>
        /// Gets or sets the number of characters that were used in the opening code fence.
        /// </summary>
        public int FenceLength { get; set; }

        /// <summary>
        /// Gets or sets the number of spaces the opening fence was indented.
        /// </summary>
        public int FenceOffset { get; set; }

        /// <summary>
        /// Gets or sets the character that is used in the fences.
        /// </summary>
        public char FenceChar { get; set; }

        /// <summary>
        /// Gets or sets the additional information that was present in the same line as the opening fence.
        /// </summary>
        public string Info { get; set; }
    }
}