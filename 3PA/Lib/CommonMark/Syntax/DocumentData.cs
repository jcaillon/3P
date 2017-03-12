#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (DocumentData.cs) is part of 3P.
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
using System.Collections.Generic;

namespace _3PA.Lib.CommonMark.Syntax {
    /// <summary>
    /// Contains additional data for document elements. Used in the <see cref="Block.Document"/> property.
    /// </summary>
    public class DocumentData {
        /// <summary>
        /// Gets or sets the dictionary containing resolved link references. Only set on the document node, <see langword="null"/>
        /// and not used for all other elements.
        /// </summary>
        public Dictionary<string, Reference> ReferenceMap { get; set; }
    }
}