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
    public class FilteredListItem : ListItem {

        #region Constructor

        public FilteredListItem() {
            FilterFullyMatch = true;
        }

        #endregion

        #region Filter

        /// <summary>
        /// The dispertion level with which the lowerCaseFilterString matches the DisplayText
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public int FilterDispertionLevel { get; private set; }

        /// <summary>
        /// True of the lowerCaseFilterString fully matches DisplayText
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public bool FilterFullyMatch { get; private set; }

        /// <summary>
        /// The way lowerCaseFilterString matches DisplayText
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public List<CharacterRange> FilterMatchedRanges { get; private set; }

        /// <summary>
        /// Call this method to compute the value of
        /// FilterDispertionLevel, FilterFullyMatch, FilterMatchedRanges
        /// </summary>
        /// <remarks>Internal use only!</remarks>
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

    /// <summary>
    /// Adds a layer "type" for each item, they are then categorized in a particular "type" which can
    /// be used to quickly filter the items through a set of "type" buttons
    /// </summary>
    public class FilteredTypeListItem : FilteredListItem {
        
        /// <summary>
        /// to override, should return this item type (a unique int for each item type)
        /// </summary>
        public virtual int ItemType {
            get { return 0; }
        }

        /// <summary>
        /// to override, should return the image to display for this item
        /// </summary>
        public virtual Image ItemImage {
            get { return null; }
        }

        /// <summary>
        /// to override, should return true if the item is to be highlighted
        /// </summary>
        public virtual bool IsRowHighlighted {
            get { return false; }
        }

        /// <summary>
        /// to override, should return a string containing the subtext to display
        /// </summary>
        public virtual string SubText {
            get { return null; }
        }

        /// <summary>
        /// to override, should return a list of images to be displayed (in reverse order) for the item
        /// </summary>
        public virtual List<Image> TagImages {
            get { return null; }
        }

    }

    #endregion

    #region FilteredTypeItemTree

    /// <summary>
    /// Each item is now view as a node of the tree, allows to view the list as a tree
    /// </summary>
    public class FilteredTypeTreeListItem : FilteredTypeListItem {

        /// <summary>
        /// to override, does this item have children?
        /// </summary>
        public virtual bool CanExpand {
            get { return false; } 
        }

        /// <summary>
        /// to override, that should return the list of the children for this item (if any) or null
        /// </summary>
        public virtual List<FilteredTypeTreeListItem> Children {
            get { return null; } 
        }
        
        /// <summary>
        /// Is this item expanded? (useful only if CanExpand), should only be used in read mode
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Returns the list of the children for this item, to be used internally by the YamuiList as it
        /// also sets properties for each child
        /// </summary>
        /// <remarks>Internal use only!</remarks>
        public List<FilteredTypeTreeListItem> GetItemChildren() {
            var list = Children;
            if (list != null && list.Count > 0) {
                var count = 0;
                foreach (var itemTree in list) {
                    itemTree.ParentNode = this;
                    itemTree.IsFirstItem = (count == 0);
                    itemTree.IsLastItem = false;
                    itemTree.ComputeItemProperties();
                    count++;
                }
                list[count - 1].IsLastItem = true;
            }
            return list;
        }

        /// <summary>
        /// Compute the ancestors/path descriptor/level for this item (not required for root items)
        /// </summary>
        private void ComputeItemProperties() {

            if (ParentNode != null) {

                // compute ancestors
                Ancestors = new List<FilteredTypeTreeListItem>();
                // compute path descriptor
                _pathDescriptor = string.Empty;
                // compute node level
                Level = 0;

                var loopParent = ParentNode;
                while (loopParent != null) {
                    Ancestors.Add(loopParent);
                    _pathDescriptor = loopParent.DisplayText + "(" + loopParent.ItemType + ")/" + _pathDescriptor;
                    Level++;
                    loopParent = loopParent.ParentNode;
                }
                _pathDescriptor = _pathDescriptor + DisplayText + "(" + ItemType + ")";
            }
        }

        /// <summary>
        /// Parent node for this item (can be null if the item is on the root of the tree)
        /// </summary>
        public FilteredTypeTreeListItem ParentNode { get; private set; }

        /// <summary>
        /// Is this the first item of the tree?
        /// </summary>
        public bool IsFirstItem { get; private set; }

        /// <summary>
        /// This is the last item of the tree or the last item of its branch
        /// </summary>
        public bool IsLastItem { get; private set; }

        /// <summary>
        /// True if the object is at root level as an item, not as a branch
        /// </summary>
        public bool IsRoot {
            get { return ParentNode == null; } 
        }

        /// <summary>
        /// A list of this object ancestors (PARENT) node (null for root items)
        /// </summary>
        public List<FilteredTypeTreeListItem> Ancestors { get; private set; }

        /// <summary>
        /// Returns a string describing the place of this item in the tree in the form of a path (using displaytext + (type)) :
        /// rootitem(10)/parent(2)/this(4)
        /// </summary>
        public string PathDescriptor {
            get { return _pathDescriptor ?? DisplayText + "(" + ItemType + ")"; }
        }
        private string _pathDescriptor;

        /// <summary>
        /// The level of the item defines its place in the tree, level 0 is the root, 1 is deeper and so on...
        /// </summary>
        public int Level { get; private set; }

    }

    #endregion

}
