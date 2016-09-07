#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ListItem.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System.Collections.Generic;
using System.Drawing;

namespace YamuiFramework.Controls.YamuiList {

    #region ListItem

    /// <summary>
    /// Describes a basic item of a scroll list
    /// </summary>
    public class ListItem {

        /// <summary>
        /// The piece of text displayed in the list
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// The item is disabled or not
        /// </summary>
        public bool IsDisabled { get; set; }
    }

    #endregion

    #region FilteredItem

    /// <summary>
    /// Adds attributes that allow to filter the list of items to display based on a filter string, 
    /// the method FilterApply allows to compute the attributes
    /// </summary>
    public class FilteredItem : ListItem {

        #region Filter

        /// <summary>
        /// The dispertion level with which the lowerCaseFilterString matches the DisplayText
        /// </summary>
        public int FilterDispertionLevel { get; set; }

        /// <summary>
        /// True of the lowerCaseFilterString fully matches DisplayText
        /// </summary>
        public bool FilterFullyMatch { get; set; }

        /// <summary>
        /// The way lowerCaseFilterString matches DisplayText
        /// </summary>
        public List<CharacterRange> FilterMatchedRanges { get; set; }

        /// <summary>
        /// Call this method to compute the value of
        /// FilterDispertionLevel, FilterFullyMatch, FilterMatchedRanges
        /// </summary>
        public void FilterApply(string lowerCaseFilterString) {

            FilterMatchedRanges = new List<CharacterRange>();
            FilterFullyMatch = true;
            FilterDispertionLevel = 0;

            if (string.IsNullOrEmpty(lowerCaseFilterString) || string.IsNullOrEmpty(DisplayText))
                return;

            var lcText = DisplayText.ToLower();
            var textLenght = lcText.Length;
            var filterLenght = lowerCaseFilterString.Length;

            int pos = 0;
            int posFilter = 0;
            bool matching = false;
            int startMatch = 0;

            while (pos < textLenght) {
                // remember matching state at the beginning of the loop
                bool wasMatching = matching;
                // we match the current char of the filter
                if (lcText[pos] == lowerCaseFilterString[posFilter]) {
                    if (!matching) {
                        matching = true;
                        startMatch = pos;
                    }
                    posFilter++;
                    // we matched the entire filter
                    if (posFilter >= filterLenght) {
                        FilterMatchedRanges.Add(new CharacterRange(startMatch, pos - startMatch + 1));
                        break;
                    }
                } else {
                    matching = false;

                    // gap between match mean more penalty than finding the match later in the string
                    if (posFilter > 0) {
                        FilterDispertionLevel += 900;
                    } else {
                        FilterDispertionLevel += 30;
                    }
                }
                // we stopped matching, remember matching range
                if (!matching && wasMatching)
                    FilterMatchedRanges.Add(new CharacterRange(startMatch, pos - startMatch));
                pos++;
            }

            // put the exact matches first
            if (filterLenght != textLenght)
                FilterDispertionLevel += 1;

            // we reached the end of the input, if we were matching stuff, remember matching range
            if (pos >= textLenght && matching)
                FilterMatchedRanges.Add(new CharacterRange(startMatch, pos - 1 - startMatch));

            // we didn't match the entire filter
            if (posFilter < filterLenght)
                FilterFullyMatch = false;
        }

        #endregion
    }

    #endregion

    #region FilteredTypeItem

    public class FilteredTypeItem : FilteredItem {

        /// <summary>
        /// to override, should return a unique int for each item type
        /// </summary>
        public virtual int ItemType {
            get { return 0; }
        }

    }

    #endregion

    #region FilteredTypeItemTree

    /// <summary>
    /// Each item is now view as a node of the tree, allows to view the list as a tree
    /// </summary>
    public class FilteredTypeItemTree : FilteredTypeItem {

        /// <summary>
        /// Is this item expanded? (useful only if CanExpand)
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
        /// A list of this object ancestors (PARENT) node
        /// </summary>
        public List<FilteredTypeItemTree> Ancestors { get; set; }

        /// <summary>
        /// True if the object is at root level as an item, not as a branch
        /// </summary>
        public bool IsRoot { get; set; }

        /// <summary>
        /// The level of the item defines its place in the tree, level 1 is the root, 2 is deeper and so on...
        /// </summary>
        public int Level {
            get { return (Ancestors == null ? 0 : Ancestors.Count) + 1; }
        }
    }

    #endregion

}
