#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (BlockTag.cs) is part of 3P.
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

namespace _3PA.Lib.CommonMark.Syntax {
    /// <summary>
    /// Specifies the element type of a <see cref="Block"/> instance.
    /// </summary>
    public enum BlockTag : byte {
        /// <summary>
        /// The root element that represents the document itself. There should only be one in the tree.
        /// </summary>
        Document,

        /// <summary>
        /// A block-quote element.
        /// </summary>
        BlockQuote,

        /// <summary>
        /// A list element. Will contain nested blocks with type of <see cref="BlockTag.ListItem"/>.
        /// </summary>
        List,

        /// <summary>
        /// An item in a block element of type <see cref="BlockTag.List"/>.
        /// </summary>
        ListItem,

        /// <summary>
        /// A code block element that was formatted with fences (for example, <c>~~~\nfoo\n~~~</c>).
        /// </summary>
        FencedCode,

        /// <summary>
        /// A code block element that was formatted by indenting the lines with at least 4 spaces.
        /// </summary>
        IndentedCode,

        /// <summary>
        /// A raw HTML code block element.
        /// </summary>
        HtmlBlock,

        /// <summary>
        /// A paragraph block element.
        /// </summary>
        Paragraph,

        /// <summary>
        /// A heading element that was parsed from an ATX style markup (<c>## heading 2</c>).
        /// </summary>
        AtxHeading,

        /// <summary>
        /// Obsolete. Use <see cref="AtxHeading"/> instead.
        /// </summary>
        [Obsolete("Use AtxHeading instead.")]
        AtxHeader = AtxHeading,

        /// <summary>
        /// A heading element that was parsed from a Setext style markup (<c>heading\n========</c>).
        /// </summary>
        SetextHeading,

        /// <summary>
        /// Obsolete. Use <see cref="SetextHeading"/> instead.
        /// </summary>
        [Obsolete("Use SetextHeading instead.")]
        SETextHeader = SetextHeading,

        /// <summary>
        /// A thematic break element.
        /// </summary>
        ThematicBreak,

        /// <summary>
        /// Obsolete. Use <see cref="ThematicBreak"/> instead.
        /// </summary>
        [Obsolete("Use ThematicBreak instead.")]
        HorizontalRuler = ThematicBreak,

        /// <summary>
        /// A text block that contains only link reference definitions.
        /// </summary>
        ReferenceDefinition
    }
}