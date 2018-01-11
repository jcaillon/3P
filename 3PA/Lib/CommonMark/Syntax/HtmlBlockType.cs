#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlBlockType.cs) is part of 3P.
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
    /// Specifies the type of the HTML block a <see cref="Block"/> instance represents.
    /// See http://spec.commonmark.org/0.22/#html-block
    /// </summary>
    public enum HtmlBlockType {
        /// <summary>
        /// This is not a HTML block.
        /// </summary>
        None = 0,

        /// <summary>
        /// The HTML block represents <c>script</c>, <c>pre</c> or <c>style</c> element. Unline other HTML tags
        /// these are allowed to contain blank lines.
        /// </summary>
        InterruptingBlockWithEmptyLines = 1,

        /// <summary>
        /// The block represents an HTML comment.
        /// </summary>
        Comment = 2,

        /// <summary>
        /// The block represents a processing instruction <c>&lt;??&gt;</c>
        /// </summary>
        ProcessingInstruction = 3,

        /// <summary>
        /// The block represents a doctype element <c>&lt;!...&gt;</c>
        /// </summary>
        DocumentType = 4,

        /// <summary>
        /// The block represents <c>&lt;![CDATA[...]]</c> element.
        /// </summary>
        CData = 5,

        /// <summary>
        /// This HTML block can interrupt paragraphs.
        /// </summary>
        InterruptingBlock = 6,

        /// <summary>
        /// This HTML block cannot interrupt paragraphs.
        /// </summary>
        NonInterruptingBlock = 7
    }
}