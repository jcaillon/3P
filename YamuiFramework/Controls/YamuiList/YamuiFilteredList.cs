using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    public class YamuiFilteredList : YamuiScrollList {

        #region private fields

        protected new List<FilteredItem> _items;

        protected new int _rowHeight = 18;

        #endregion

        #region Draw list

        /// <summary>
        /// Set the items that will be displayed in the list
        /// </summary>
        protected void SetItems(List<FilteredItem> listItems) {
            _items = listItems;
            _nbItems = _items.Count;

            ComputeScrollBar();
            DrawButtons();

            // make sure to select an index that exists
            SelectedItemIndex = SelectedItemIndex;

            // and an enabled item!
            if (_nbItems > SelectedItemIndex) {
                if (_items[SelectedItemIndex].IsDisabled) {
                    var newIndex = SelectedItemIndex;
                    do {
                        newIndex++;
                        if (newIndex > _nbItems - 1)
                            newIndex = 0;

                    } // do this while the current button is disabled and we didn't already try every button
                    while (_items[newIndex].IsDisabled && SelectedItemIndex != newIndex);
                    SelectedItemIndex = newIndex;
                }
            }

            RefreshButtons();
            RepositionThumb();
        }

        protected new void RowPaint(ListItem item, YamuiListRow row, PaintEventArgs e) {
            if (OnRowPaint != null)
                OnRowPaint(item, row, e);
            else {
                var backColor = YamuiThemeManager.Current.MenuBg(row.IsSelected, row.IsHovered, !item.IsDisabled);
                var foreColor = YamuiThemeManager.Current.MenuFg(row.IsSelected, row.IsHovered, !item.IsDisabled);

                // background
                e.Graphics.Clear(backColor);

                // foreground
                // left line
                if (row.IsSelected && !item.IsDisabled) {
                    using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.AccentColor)) {
                        e.Graphics.FillRectangle(b, new Rectangle(0, 0, 3, row.ClientRectangle.Height));
                    }
                }

                // text
                TextRenderer.DrawText(e.Graphics, item.DisplayText, FontManager.GetStandardFont(), new Rectangle(7, 0, row.ClientRectangle.Width - 7, row.ClientRectangle.Height), foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
            }
        }

        #endregion
    }
}
