#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFilteredTypeTreeList.cs) is part of YamuiFramework.
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    /// <summary>
    /// This class is the most complicated, obviously
    /// The difficulty being handling the filter string correctly;
    /// basically, we have two separate modes (see SearchMode)
    /// This is not the conventionnal tree searching but this makes more sense to me
    /// </summary>
    public class YamuiFilteredTypeTreeList : YamuiFilteredTypeList {

        #region private fields

        /// <summary>
        /// Allows to save the expansion state for each node of the tree
        /// </summary>
        private Dictionary<string, bool> _savedState = new Dictionary<string, bool>();

        /// <summary>
        /// Root items passed to the SetItems method
        /// </summary>
        protected List<FilteredTypeTreeListItem> _treeRootItems;

        private SearchModeOption _searchMode;

        #endregion

        #region public properties

        /// <summary>
        /// Two modes can be active when the filter string isn't empty :
        /// - either we filter the item + we include their parent and still display the list as a tree
        /// - or we filter + sort and display the list as a simple filtered list with types
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SearchModeOption SearchMode {
            get { return _searchMode; }
            set {
                if (value != _searchMode) {
                    if (value == SearchModeOption.FilterSortWithNoParent)
                        StartSearching();
                    else
                        StopSearching();
                }
                _searchMode = value;
            }
        }

        #endregion

        #region private properties

        /// <summary>
        /// True if the user is currently searching the list through the filter string
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private bool IsSearching {
            get { return !string.IsNullOrEmpty(FilterString) && SearchMode == SearchModeOption.FilterSortWithNoParent; }
        }

        #endregion

        #region Enum

        public enum ForceExpansion {
            Idle,
            ForceExpand,
            ForceCollapse
        }

        /// <summary>
        /// Two modes can be active when the filter string isn't empty :
        /// - either we filter the item + we include their parent and still display the list as a tree
        /// - or we filter + sort and display the list as a simple filtered list with types
        /// </summary>
        public enum SearchModeOption {
            FilterSortWithNoParent,
            FilterOnlyAndIncludeParent,
        }

        #endregion

        #region Set

        /// <summary>
        /// Set the items that will be displayed in the list
        /// </summary>
        public override void SetItems(List<ListItem> listItems) {

            var firstItem = listItems.FirstOrDefault();
            if (firstItem != null && !(firstItem is FilteredTypeTreeListItem))
                throw new Exception("listItems shoud contain objects of type FilteredTypeItem");

            _treeRootItems = listItems.Cast<FilteredTypeTreeListItem>().ToList();


            base.SetItems(GetExpandedItemsList(_treeRootItems, ForceExpansion.Idle));
        }

        /// <summary>
        /// Returns the full list of items to be displayed, taking into account expanded items
        /// </summary>
        private List<ListItem> GetExpandedItemsList(List<FilteredTypeTreeListItem> list, ForceExpansion forceExpansion) {

            if (list == null)
                return null;

            var outList = new List<ListItem>();

            foreach (var item in list) {
                outList.Add(item);
                var descriptor = item.PathDescriptor;

                if (item.CanExpand) {

                    // force expand/collapse?
                    if (forceExpansion != ForceExpansion.Idle) {
                        if (_savedState.ContainsKey(descriptor))
                            _savedState[descriptor] = forceExpansion == ForceExpansion.ForceExpand;
                        else
                            _savedState.Add(descriptor, forceExpansion == ForceExpansion.ForceExpand);
                    }

                    // restore the expand state of the item if needed
                    if (_savedState.ContainsKey(descriptor))
                        item.IsExpanded = _savedState[descriptor];

                    if (item.IsExpanded) {
                        var children = GetExpandedItemsList(item.GetItemChildren(), forceExpansion);
                        if (children != null)
                            outList.AddRange(children);
                    }
                }
            }

            return outList;
        }

        /// <summary>
        /// Returns the full list of items in the tree
        /// </summary>
        private List<ListItem> GetFullItemsList(List<FilteredTypeTreeListItem> list) {
            if (list == null)
                return null;
            var outList = new List<ListItem>();
            foreach (var item in list) {
                outList.Add(item);
                if (item.CanExpand) {
                    var children = GetFullItemsList(item.GetItemChildren());
                    if (children != null)
                        outList.AddRange(children);
                }
            }
            return outList;
        }

        #endregion

        #region Expand/Retract

        /// <summary>
        /// Call this method to expand the whole tree
        /// </summary>
        public void ForceAllToExpand() {
            base.SetItems(GetExpandedItemsList(_treeRootItems, ForceExpansion.ForceExpand));
        }

        /// <summary>
        /// Call this method to collapse the whole tree
        /// </summary>
        public void ForceAllToCollapse() {
            base.SetItems(GetExpandedItemsList(_treeRootItems, ForceExpansion.ForceCollapse));
        }

        /// <summary>
        /// ReCompute the whole list while taking into account expanded items, to be called when you change the 
        /// IsExpanded property of items on the list
        /// </summary>
        public void ApplyExpansionState() {
            base.SetItems(GetExpandedItemsList(_treeRootItems, ForceExpansion.Idle));
        }

        /// <summary>
        /// The list automatocally saves the expansion state of a node when it is changed,
        /// if you want the states to be forgotten, call this method
        /// </summary>
        public void ClearSavedExpansionState() {
            _savedState.Clear();
        }

        /// <summary>
        /// Toggle expand/collapse for the an item at the given index
        /// </summary>
        public bool ExpandCollapse(int itemIndex, ForceExpansion forceExpansion) {

            var selectedItem = GetItem(itemIndex);
            if (selectedItem == null)
                return false;

            // handles a node expansion
            var currentItem = selectedItem as FilteredTypeTreeListItem;
            if (currentItem != null && currentItem.CanExpand) {
                string currentItemPathDescriptor = currentItem.PathDescriptor;

                // switch state
                if (forceExpansion != ForceExpansion.Idle)
                    currentItem.IsExpanded = (forceExpansion == ForceExpansion.ForceExpand);
                else
                    currentItem.IsExpanded = !currentItem.IsExpanded;

                // saves expansion state
                if (_savedState.ContainsKey(currentItemPathDescriptor))
                    _savedState[currentItemPathDescriptor] = currentItem.IsExpanded;
                else
                    _savedState.Add(currentItemPathDescriptor, currentItem.IsExpanded);

                ApplyExpansionState();

                return true;
            }

            return false;
        }

        #endregion

        #region Draw list

        /// <summary>
        /// Called by default to paint the row if no OnRowPaint is defined
        /// </summary>
        protected override void RowPaint(ListItem item, YamuiListRow row, PaintEventArgs e) {

            // background
            var backColor = YamuiThemeManager.Current.MenuBg(row.IsSelected, row.IsHovered, !item.IsDisabled);
            e.Graphics.Clear(backColor);

            var curItem = item as FilteredTypeTreeListItem;
            if (curItem != null) {
                var drawRect = row.ClientRectangle;
                drawRect.Height = RowHeight;

                for (int i = 0; i <= curItem.Level; i++) {
                    drawRect.X += 8;
                    drawRect.Width -= 8;
                }

                // Draw the arrow icon indicating if the node is expanded or not
                if (curItem.CanExpand) {
                    var foreColor = YamuiThemeManager.Current.MenuFg(row.IsSelected, row.IsHovered, !item.IsDisabled);
                    TextRenderer.DrawText(e.Graphics, curItem.IsExpanded ? "y" : "u", FontManager.GetOtherFont("Wingdings 3", FontStyle.Regular, (float) (drawRect.Height*0.40)), new Rectangle(drawRect.X - 8, drawRect.Y, 8, drawRect.Height), curItem.IsExpanded ? YamuiThemeManager.Current.AccentColor : foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }

                DrawFilteredTypeRow(curItem, drawRect, row, e);
            }
        }

        #endregion

        #region ApplyFilter

        /// <summary>
        /// Returns a list of items that meet the FilterPredicate requirement as well as the filter string requirement,
        /// it also sorts the list of items
        /// We override this for trees so that if an item survives the filter, its parent should be displayed as well
        /// </summary>
        protected override List<ListItem> GetFilteredAndSortedList(List<FilteredListItem> listItems) {

            var outList = new List<FilteredTypeTreeListItem>();
            var parentsToInclude = new HashSet<string>();

            var nbItems = listItems.Count;
            for (int i = nbItems - 1; i >= 0; i--) {
                var item = listItems[i] as FilteredTypeTreeListItem;
                if (item == null) continue;
                
                // the item must be included
                if (parentsToInclude.Contains(item.PathDescriptor) || (item.FilterFullyMatch && (FilterPredicate == null || FilterPredicate(item)))) {
                    outList.Add(item);

                    // we register its parent to be included as well
                    if (!IsSearching) {
                        var lastIdx = item.PathDescriptor.LastIndexOf(FilteredTypeTreeListItem.TreePathSeparator, StringComparison.CurrentCultureIgnoreCase);
                        if (lastIdx > -1) {
                            var parentPath = item.PathDescriptor.Substring(0, lastIdx);
                            if (!parentsToInclude.Contains(parentPath))
                                parentsToInclude.Add(parentPath);
                        }
                    }
                }
            }

            // we have our list completed, but we need to reverse it before delivering it
            outList.Reverse();

            return outList.Cast<ListItem>().ToList();
        }

        #endregion

        #region Events pushed from the button rows

        /// <summary>
        /// Click on an item, SelectedItem is usable at this time
        /// </summary>
        protected override void OnItemClick(MouseEventArgs eventArgs) {
            // handles node expansion
            if (!IsSearching)
                ExpandCollapse(SelectedItemIndex, ForceExpansion.Idle);

            base.OnItemClick(eventArgs);
        }

        #endregion

        #region OnKeyDown

        public override bool OnKeyDown(Keys pressedKey) {
            switch (pressedKey) {
                case Keys.Left:
                    if (!IsSearching || ModifierKeys.HasFlag(Keys.Control)) {
                        LeftRight(true);
                    } else {
                        // collapse the current item
                        ExpandCollapse(SelectedItemIndex, ForceExpansion.ForceCollapse);
                    }
                    return true;

                case Keys.Right:
                    if (!IsSearching || ModifierKeys.HasFlag(Keys.Control)) {
                        LeftRight(false);
                    } else {
                        // expand the current item
                        ExpandCollapse(SelectedItemIndex, ForceExpansion.ForceExpand);
                    }
                    return true;
            }
            return base.OnKeyDown(pressedKey);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Associate the TextChanged event of a text box to this method to filter this list with the input text of the textbox
        /// </summary>
        public override void OnTextChangedEvent(object sender, EventArgs eventArgs) {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            var oldValue = _filterString;
            var newValue = textBox.Text.ToLower().Trim();

            // this is the classic filter where we don't search in the whole tree, just what's displayed
            if (SearchMode == SearchModeOption.FilterOnlyAndIncludeParent) {
                FilterString = newValue;
                return;
            }

            _filterString = newValue;
            if (string.IsNullOrEmpty(oldValue) && !string.IsNullOrEmpty(newValue)) {
                StartSearching();

            } else if (!string.IsNullOrEmpty(oldValue) && string.IsNullOrEmpty(newValue)) {
                StopSearching();

            } else {
                FilterString = newValue;
            }

        }

        private void StartSearching() {
            // we started searching
            base.SetItems(GetFullItemsList(_treeRootItems));
        }

        private void StopSearching() {
            // we stopped seaching
            base.SetItems(GetExpandedItemsList(_treeRootItems, ForceExpansion.Idle));
        }

        #endregion

    }

}
