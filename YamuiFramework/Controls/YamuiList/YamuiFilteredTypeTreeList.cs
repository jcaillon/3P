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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    public class YamuiFilteredTypeTreeList : YamuiFilteredTypeList {

        #region private fields

        private Dictionary<string, bool> _savedExpandState = new Dictionary<string, bool>();

        /// <summary>
        /// Root items passed to the SetItems method
        /// </summary>
        protected List<FilteredTypeTreeListItem> _treeRootItems;

        #endregion

        #region public properties

        #endregion

        #region ExpansionState

        public enum ForceExpansion {
            Idle,
            ForceExpand,
            ForceCollapse
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

                // force expand/collapse?
                if (forceExpansion != ForceExpansion.Idle) {
                    if (_savedExpandState.ContainsKey(descriptor))
                        _savedExpandState[descriptor] = forceExpansion == ForceExpansion.ForceExpand;
                    else
                        _savedExpandState.Add(descriptor, forceExpansion == ForceExpansion.ForceExpand);
                }

                // restore the expand state of the item if needed
                if (_savedExpandState.ContainsKey(descriptor))
                    item.IsExpanded = _savedExpandState[descriptor];

                if (item.CanExpand && item.IsExpanded) {
                    var children = GetExpandedItemsList(item.GetItemChildren(), forceExpansion);
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
            _savedExpandState.Clear();
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

                List<ListItem> newList;

                // switch state
                if (forceExpansion != ForceExpansion.Idle)
                    currentItem.IsExpanded = (forceExpansion == ForceExpansion.ForceExpand);
                else
                    currentItem.IsExpanded = !currentItem.IsExpanded;
                string currentItemPathDescriptor = currentItem.PathDescriptor;

                // saves expansion state
                if (_savedExpandState.ContainsKey(currentItemPathDescriptor))
                    _savedExpandState[currentItemPathDescriptor] = currentItem.IsExpanded;
                else
                    _savedExpandState.Add(currentItemPathDescriptor, currentItem.IsExpanded);

                if (currentItem.IsExpanded) {
                    // insert children to the existing list
                    newList = _initialItems.Cast<ListItem>().ToList();
                    var children = GetExpandedItemsList(currentItem.GetItemChildren(), ForceExpansion.Idle);

                    if (children != null) {
                        if (itemIndex + 1 < _nbInitialItems)
                            newList.InsertRange(itemIndex + 1, children);
                        else
                            newList.AddRange(children);
                    }
                } else {
                    // remove all children (using the path descriptor to know which are the children)
                    var list = _initialItems.Cast<FilteredTypeTreeListItem>().ToList();
                    int idx = itemIndex + 1;
                    string currentPath = currentItemPathDescriptor + "/";

                    // while the next item (following our current item) begins with the same path descriptor
                    while (idx < _nbInitialItems && list[idx].PathDescriptor.StartsWith(currentPath)) {
                        idx++;
                    }
                    int nbToDelete = idx - (itemIndex + 1);
                    if (nbToDelete > 1)
                        list.RemoveRange(itemIndex + 1, nbToDelete);
                    newList = list.Cast<ListItem>().ToList();
                }

                // set the new list
                base.SetItems(newList);

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

        #region Events pushed from the button rows

        /// <summary>
        /// Click on an item, SelectedItem is usable at this time
        /// </summary>
        protected override void OnItemClick(MouseEventArgs eventArgs) {
            // handles node expansion
            ExpandCollapse(SelectedItemIndex, ForceExpansion.Idle);

            base.OnItemClick(eventArgs);
        }

        #endregion

        #region OnKeyDown

        public override bool OnKeyDown(Keys pressedKey) {
            switch (pressedKey) {
                case Keys.Left:
                    if (ModifierKeys.HasFlag(Keys.Control)) {
                        LeftRight(true);
                    } else {
                        // collapse the current item
                        ExpandCollapse(SelectedItemIndex, ForceExpansion.ForceCollapse);
                    }
                    return true;

                case Keys.Right:
                    if (ModifierKeys.HasFlag(Keys.Control)) {
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

    }

}
