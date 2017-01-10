#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (FilteredItemTree.cs) is part of 3P.
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

namespace _3PA.MainFeatures.FilteredLists {
    internal class FilteredItemTree : FilteredItem {

        /// <summary>
        /// Is this item expanded?
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Does it have children?
        /// </summary>
        public bool CanExpand { get; set; }

        /// <summary>
        /// Is this the first item of the tree?
        /// </summary>
        public bool IsFirstItem { get; set; }

        /// <summary>
        /// This is the last item of the tree or the last item of its branch
        /// </summary>
        public bool IsLastItem { get; set; }

        /// <summary>
        /// A list of this object ancerstors node
        /// </summary>
        public List<FilteredItemTree> Ancestors { get; set; }

        /// <summary>
        /// True if the object is at root level (=1) as an item, not as a branch
        /// </summary>
        public bool IsRoot { get; set; }

        /// <summary>
        /// The level of the item defines its place in the tree, level 1 is the root, 2 is deeper and so on...
        /// </summary>
        public int Level {
            get { return (Ancestors == null ? 0 : Ancestors.Count) + 1; }
        }
    }
}
