#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (EnumeratorEntry.cs) is part of 3P.
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
    /// Represents a single element in the document tree when traversing through it with the enumerator.
    /// </summary>
    /// <seealso cref="Syntax.Block.AsEnumerable"/>
    public sealed class EnumeratorEntry {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumeratorEntry"/> class.
        /// </summary>
        /// <param name="opening">Specifies if this instance represents the opening of the block element.</param>
        /// <param name="closing">Specifies if this instance represents the closing of the block element (returned by the
        /// enumerator after the children have been enumerated). Both <paramref name="closing"/> and <paramref name="opening"/>
        /// can be specified at the same time if there are no children for the <paramref name="block"/> element.</param>
        /// <param name="block">The block element being returned from the enumerator.</param>
        public EnumeratorEntry(bool opening, bool closing, Block block) {
            IsOpening = opening;
            IsClosing = closing;
            Block = block;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumeratorEntry"/> class.
        /// </summary>
        /// <param name="opening">Specifies if this instance represents the opening of the inline element.</param>
        /// <param name="closing">Specifies if this instance represents the closing of the inline element (returned by the
        /// enumerator after the children have been enumerated). Both <paramref name="closing"/> and <paramref name="opening"/>
        /// can be specified at the same time if there are no children for the <paramref name="inline"/> element.</param>
        /// <param name="inline">The inlien element being returned from the enumerator.</param>
        public EnumeratorEntry(bool opening, bool closing, Inline inline) {
            IsOpening = opening;
            IsClosing = closing;
            Inline = inline;
        }

        /// <summary>
        /// Gets the value indicating whether this instance represents the opening of the element (returned before enumerating
        /// over the children of the element).
        /// </summary>
        public bool IsOpening { get; private set; }

        /// <summary>
        /// Gets the value indicating whether this instance represents the closing of the element (returned by the
        /// enumerator after the children have been enumerated). Both <see name="IsOpening"/> and <see name="IsClosing"/>
        /// can be <see langword="true"/> at the same time if there are no children for the given element.
        /// </summary>
        public bool IsClosing { get; private set; }

        /// <summary>
        /// Gets the inline element. Can be <see langword="null"/> if <see cref="Block"/> is set.
        /// </summary>
        public Inline Inline { get; private set; }

        /// <summary>
        /// Gets the block element. Can be <see langword="null"/> if <see cref="Inline"/> is set.
        /// </summary>
        public Block Block { get; private set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() {
            string r;

            if (IsOpening && IsClosing)
                r = "Complete ";
            else if (IsOpening)
                r = "Opening ";
            else if (IsClosing)
                r = "Closing ";
            else
                r = "Invalid ";

            if (Block != null)
                r += "block " + Block.Tag;
            else if (Inline != null) {
                r += "inline " + Inline.Tag;

                if (Inline.Tag == InlineTag.String) {
                    if (Inline.LiteralContent == null)
                        r += ": <null>";
                    else if (Inline.LiteralContent.Length < 20)
                        r += ": " + Inline.LiteralContent;
                    else
                        r += ": " + Inline.LiteralContent.Substring(0, 19) + "…";
                }
            } else
                r += "empty";

            return r;
        }
    }
}