#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (InlineContentLinkable.cs) is part of 3P.
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
    /// An obsolete class. Used to contain properties specific to link inline elements.
    /// </summary>
    [Obsolete("These properties have been moved directly into the Inline element.")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class InlineContentLinkable {
        /// <summary>
        /// Gets or sets the URL of a link. Moved to <see cref="Inline.TargetUrl"/>.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the title of a link. Moved to <see cref="Inline.LiteralContent"/>.
        /// </summary>
        public string Title { get; set; }
    }
}