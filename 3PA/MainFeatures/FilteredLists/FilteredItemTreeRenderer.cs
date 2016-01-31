#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (FilteredItemTreeRenderer.cs) is part of 3P.
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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace _3PA.MainFeatures.FilteredLists {

    /// <summary>
    /// This renderer highlights substrings that match a given text filter,
    /// It only works for OVL with objects that inherit from FilteredItem and it uses
    /// FilteredItem.FilterFullyMatch
    /// </summary>
    public class FilteredItemTreeRenderer : FilteredItemTextRenderer {

        public bool DoNotDrawTree { get; set; }

        /// <summary>
        /// Create a HighlightTextRenderer
        /// </summary>
        public FilteredItemTreeRenderer() {
            _linePen = new Pen(ThemeManager.Current.FormAltBack, 1.5f) {
                DashStyle = DashStyle.Solid
            };
        }

        #region Configuration properties
        private Pen _linePen;

        /// <summary>
        /// How many pixels will be reserved for each level of indentation?
        /// </summary>
        public static int PixelsPerLevel = 16 + 1;
        #endregion

        #region Tree Rendering

        /// <summary>
        /// We override the Render method of the object list view to draw our tree
        /// </summary>
        public override void Render(Graphics graphic, Rectangle rect) {

            DrawBackground(graphic, rect);

            var paddedRectangle = ApplyCellPadding(rect);

            var rowObj = RowObject as FilteredItemTree;
            if (rowObj != null && !DoNotDrawTree) {

                // Draw the arbo of the tree
                Rectangle r2 = rect;
                r2.Width = PixelsPerLevel;

                // Vertical lines have to start on even points, otherwise the dotted line looks wrong
                // This is only needed if pen is dotted
                int top = r2.Top;

                // Draw lines for ancestors
                int midX;
                if (rowObj.Ancestors != null) {
                    foreach (var ancestor in rowObj.Ancestors) {
                        if (!ancestor.IsLastItem) {
                            midX = r2.Left + r2.Width/2;
                            graphic.DrawLine(_linePen, midX, top, midX, r2.Bottom);
                        }
                        r2.Offset(PixelsPerLevel, 0);
                    }
                }

                // Draw lines for this branch
                midX = r2.Left + r2.Width / 2;

                var expandGlyphRectangle = paddedRectangle;
                expandGlyphRectangle.Offset((rowObj.Level - 1) * PixelsPerLevel, 0);
                expandGlyphRectangle.Width = PixelsPerLevel;
                expandGlyphRectangle.Height = PixelsPerLevel;
                expandGlyphRectangle.Y = AlignVertically(paddedRectangle, expandGlyphRectangle);
                var expandGlyphRectangleMidVertical = expandGlyphRectangle.Y + (expandGlyphRectangle.Height / 2);

                // Horizontal line first
                graphic.DrawLine(_linePen, midX, expandGlyphRectangleMidVertical, r2.Right, expandGlyphRectangleMidVertical);

                // Vertical line second
                if (rowObj.IsFirstItem) {
                    if (!rowObj.IsLastItem)
                        graphic.DrawLine(_linePen, midX, expandGlyphRectangleMidVertical, midX, r2.Bottom);
                } else {
                    if (rowObj.IsLastItem)
                        graphic.DrawLine(_linePen, midX, top, midX, expandGlyphRectangleMidVertical);
                    else
                        graphic.DrawLine(_linePen, midX, top, midX, r2.Bottom);
                }

                // draw the expansion symbol
                if (rowObj.CanExpand) {
                    var h = 12;
                    var w = 12;
                    var x = expandGlyphRectangle.X + (expandGlyphRectangle.Width / 2) - w / 2;
                    var y = expandGlyphRectangle.Y + (expandGlyphRectangle.Height / 2) - h / 2;

                    using (var p = new Pen(ThemeManager.Current.ButtonHoverBorder)) {
                        graphic.DrawRectangle(p, new Rectangle(x, y, w, h));
                    }
                    using (var p = new SolidBrush(ThemeManager.Current.ButtonHoverBack)) {
                        graphic.FillRectangle(p, new Rectangle(x + 1, y + 1, w - 1, h - 1));
                    }
                    if (rowObj.IsExpanded)
                        using (var b = new SolidBrush(ThemeManager.Current.AccentColor)) {
                            graphic.FillRectangle(b, new Rectangle(x + 2, y + 2, w - 4, h - 4));
                        }
                }

                var indent = rowObj.Level * PixelsPerLevel;
                paddedRectangle.Offset(indent, 0);
                paddedRectangle.Width -= indent;

            }

            DrawImageAndText(graphic, paddedRectangle);
        }

        #endregion

    }
}
