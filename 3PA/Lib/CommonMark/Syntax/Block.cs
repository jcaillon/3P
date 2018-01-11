#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (Block.cs) is part of 3P.
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
using System.Collections.Generic;

namespace _3PA.Lib.CommonMark.Syntax {
    /// <summary>
    /// Represents a block-level element of the parsed document.
    /// </summary>
    public sealed class Block {
        /// <summary>
        /// Initializes a new instance of the <see cref="Block"/> class.
        /// </summary>
        /// <param name="tag">The type of the element this instance represents.</param>
        /// <param name="sourcePosition">The position of the first character of this block in the source text.</param>
        public Block(BlockTag tag, int sourcePosition) {
            Tag = tag;
            SourcePosition = sourcePosition;
            IsOpen = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Block"/> class.
        /// </summary>
        /// <param name="tag">The type of the element this instance represents.</param>
        /// <param name="startLine">The number of the first line in the source text that contains this element.</param>
        /// <param name="startColumn">The number of the first column (within the first line) in the source text that contains this element.</param>
        /// <param name="sourcePosition">The position of the first character of this block in the source text.</param>
        [Obsolete("StartLine/StartColumn are deprecated in favor of SourcePosition/SourceLength and will be removed in future. If you have a use case where this property cannot be replaced with the new ones, please log an issue at https://github.com/Knagis/CommonMark.NET", false)]
        public Block(BlockTag tag, int startLine, int startColumn, int sourcePosition) {
            Tag = tag;
            StartLine = startLine;
            EndLine = startLine;
            StartColumn = startColumn;
            SourcePosition = sourcePosition;
            IsOpen = true;
        }

        /// <summary>
        /// Returns an enumerable that allows the iteration over all block and inline elements within this
        /// instance. Note that the enumerator should not be used if optimal performance is desired and instead
        /// a custom implementation should be written.
        /// </summary>
        public IEnumerable<EnumeratorEntry> AsEnumerable() {
            return new Enumerable(this);
        }

        /// <summary>
        /// Creates a new top-level document block.
        /// </summary>
        internal static Block CreateDocument() {
#pragma warning disable 0618
            Block e = new Block(BlockTag.Document, 1, 1, 0);
#pragma warning restore 0618
            e.Document = new DocumentData();
            e.Document.ReferenceMap = new Dictionary<string, Reference>();
            e.Top = e;
            return e;
        }

        /// <summary>
        /// Gets or sets the type of the element this instance represents.
        /// </summary>
        public BlockTag Tag { get; set; }

        /// <summary>
        /// Gets or sets the type of the HTML block. Only applies when <see cref="Tag"/> equals <see cref="BlockTag.HtmlBlock"/>.
        /// </summary>
        public HtmlBlockType HtmlBlockType { get; set; }

        /// <summary>
        /// Gets or sets the number of the first line in the source text that contains this element.
        /// </summary>
        [Obsolete("This is deprecated in favor of SourcePosition/SourceLength and will be removed in future. If you have a use case where this property cannot be replaced with the new ones, please log an issue at https://github.com/Knagis/CommonMark.NET", false)]
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public int StartLine { get; set; }

        /// <summary>
        /// Gets or sets the number of the first column (within the first line) in the source text that contains this element.
        /// </summary>
        [Obsolete("This is deprecated in favor of SourcePosition/SourceLength and will be removed in future. If you have a use case where this property cannot be replaced with the new ones, please log an issue at https://github.com/Knagis/CommonMark.NET", false)]
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public int StartColumn { get; set; }

        /// <summary>
        /// Gets or sets the number of the last line in the source text that contains this element.
        /// </summary>
        [Obsolete("This is deprecated in favor of SourcePosition/SourceLength and will be removed in future. If you have a use case where this property cannot be replaced with the new ones, please log an issue at https://github.com/Knagis/CommonMark.NET", false)]
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public int EndLine { get; set; }

        /// <summary>
        /// Gets or sets the position of the block element within the source data. This position is before 
        /// any opening characters. <see cref="CommonMarkSettings.TrackSourcePosition"/> must be enabled
        /// for this value to be defined.
        /// </summary>
        /// <seealso cref="SourceLength"/>
        public int SourcePosition { get; set; }

        internal int SourceLastPosition { get; set; }

        /// <summary>
        /// Gets or sets the length of the block element within the source data. This includes also characters that
        /// close the block element and in most cases the newline characters right after the block element.
        /// <see cref="CommonMarkSettings.TrackSourcePosition"/> must be enabled for this value to be defined.
        /// </summary>
        /// <seealso cref="SourcePosition"/>
        public int SourceLength {
            get { return SourceLastPosition - SourcePosition; }
            set { SourceLastPosition = SourcePosition + value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this block element has been completed (and thus new lines cannot be added
        /// to it) or is still open. By default all elements are created as open and are closed when the parser detects it.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the last line parsed for this block element was blank (containing only
        /// whitespaces).
        /// </summary>
        public bool IsLastLineBlank { get; set; }

        /// <summary>
        /// Gets or sets the first child element of this instance, or <see langword="null"/> if there are no children.
        /// </summary>
        public Block FirstChild { get; set; }

        /// <summary>
        /// Gets or sets the last child element (the last sibling of <see cref="FirstChild"/>) of this instance. 
        /// <see langword="null"/> if there are no children.
        /// </summary>
        public Block LastChild { get; set; }

        /// <summary>
        /// Gets or sets the parent element of this block.
        /// </summary>
        public Block Parent { get; set; }

        /// <summary>
        /// Gets or sets the root element (that represents the document itself).
        /// </summary>
        public Block Top { get; set; }

        /// <summary>
        /// Gets or sets the string content of this block. The content consists of multiple string parts to avoid string concatenation.
        /// Note that some parts of the parser (for example, <see cref="CommonMark.HtmlFormatterassume that
        /// the parts are not split within certain objects, so it is advised that the parts are split on newline.
        /// </summary>
        public StringContent StringContent { get; set; }

        /// <summary>
        /// Gets or sets the first inline element that was parsed from <see cref="StringContent"/> property.
        /// Note that once the inlines are parsed, <see cref="StringContent"/> will be set to <see langword="null"/>.
        /// </summary>
        public Inline InlineContent { get; set; }

        /// <summary>
        /// Gets or sets the additional properties that apply to list elements.
        /// </summary>
        public ListData ListData { get; set; }

        /// <summary>
        /// Gets or sets the additional properties that apply to fenced code blocks.
        /// </summary>
        public FencedCodeData FencedCodeData { get; set; }

        /// <summary>
        /// Gets or sets the additional properties that apply to heading elements.
        /// </summary>
        public HeadingData Heading { get; set; }

        /// <summary>
        /// Gets or sets the heading level (as in <c>&lt;h1&gt;</c> or <c>&lt;h2&gt;</c>).
        /// </summary>
        [Obsolete("Use Heading instead.")]
        public int HeaderLevel {
            get { return Heading.Level; }
            set { Heading = new HeadingData(value); }
        }

        /// <summary>
        /// Gets or sets the additional properties that apply to document nodes.
        /// </summary>
        public DocumentData Document { get; set; }

        /// <summary>
        /// Obsolete. Use <see cref="Document"/> instead.
        /// </summary>
        [Obsolete("Use Document instead.")]
        public Dictionary<string, Reference> ReferenceMap {
            get { return Document != null ? Document.ReferenceMap : null; }
            set {
                if (Document == null) {
                    if (value == null) {
                        return;
                    }
                    Document = new DocumentData();
                }
                Document.ReferenceMap = value;
            }
        }

        /// <summary>
        /// Gets or sets the next sibling of this block element; <see langword="null"/> if this is the last element.
        /// </summary>
        public Block NextSibling { get; set; }

        /// <summary>
        /// Gets or sets the previous sibling of this block element; <see langword="null"/> if this is the first element.
        /// </summary>
        [Obsolete("This property will be removed in future. If you have a use case where this property is required, please log an issue at https://github.com/Knagis/CommonMark.NET", false)]
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Block Previous { get; set; }
    }
}