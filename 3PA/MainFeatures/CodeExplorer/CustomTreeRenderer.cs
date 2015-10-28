#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (CustomTreeRenderer.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Themes;
using _3PA.Lib;

namespace _3PA.MainFeatures.CodeExplorer {
    /// <summary>
    ///     This class handles drawing the tree structure of the primary column.
    /// </summary>
    public class CustomTreeRenderer : TreeListView.TreeRenderer {

        /// <summary>
        ///     Create a TreeRenderer
        /// </summary>
        public CustomTreeRenderer(string filterStr) {
            _linePen = new Pen(ThemeManager.AccentColor, 1.5f) {
                DashStyle = DashStyle.Solid
            };
            _fillBrush = new SolidBrush(ThemeManager.Current.AutoCompletionHighlightBack);
            _framePen = new Pen(ThemeManager.Current.AutoCompletionHighlightBorder);
            _filterStr = filterStr;
        }

        #region Configuration properties
        private Pen _linePen;
        private string _filterStr;
        private Brush _fillBrush;
        private Pen _framePen;
        private bool _useRoundedRectangle = true;
        #endregion

        #region Tree Rendering
        /// <summary>
        ///     Return the branch that the renderer is currently drawing.
        /// </summary>
        private TreeListView.Branch Branch {
            get { return TreeListView.TreeModel.GetBranch(RowObject); }
        }

        /// <summary>
        ///     The real work of drawing the tree is done in this method
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="rect"></param>
        public override void Render(Graphics graphic, Rectangle rect) {
            DrawBackground(graphic, rect);

            var br = Branch;

            var paddedRectangle = ApplyCellPadding(rect);

            var expandGlyphRectangle = paddedRectangle;
            expandGlyphRectangle.Offset((br.Level - 1)*PIXELS_PER_LEVEL, 0);
            expandGlyphRectangle.Width = PIXELS_PER_LEVEL;
            expandGlyphRectangle.Height = PIXELS_PER_LEVEL;
            expandGlyphRectangle.Y = AlignVertically(paddedRectangle, expandGlyphRectangle);
            var expandGlyphRectangleMidVertical = expandGlyphRectangle.Y + (expandGlyphRectangle.Height/2);

            if (IsShowLines)
                DrawLines(graphic, rect, _linePen, br, expandGlyphRectangleMidVertical);

            if (br.CanExpand)
                DrawExpansionGlyph(graphic, expandGlyphRectangle, br.IsExpanded);

            var indent = br.Level*PIXELS_PER_LEVEL;
            paddedRectangle.Offset(indent, 0);
            paddedRectangle.Width -= indent;

            DrawImageAndText(graphic, paddedRectangle);
        }

        /// <summary>
        ///     Draw the expansion indicator
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="isExpanded"></param>
        protected new virtual void DrawExpansionGlyph(Graphics g, Rectangle r, bool isExpanded) {
            var h = 12;
            var w = 12;
            var x = r.X + (r.Width / 2) - w / 2;
            var y = r.Y + (r.Height / 2) - h / 2;

            using (var p = new Pen(ThemeManager.Current.ButtonColorsHoverBorderColor)) {
                g.DrawRectangle(p, new Rectangle(x, y, w, h));
            }
            using (var p = new SolidBrush(ThemeManager.Current.ButtonColorsHoverBackColor)) {
                g.FillRectangle(p, new Rectangle(x + 1, y + 1, w - 1, h - 1));
            }
            if (isExpanded)
                using (var b = new SolidBrush(ThemeManager.AccentColor)) {
                    g.FillRectangle(b, new Rectangle(x + 2, y + 2, w - 4, h - 4));
                }
            //using (var p = new Pen(ThemeManager.Current.ButtonColorsHoverForeColor)) {
            //    g.DrawLine(p, x + 2, y + 4, x + w - 2, y + 4);
            //    if (!isExpanded)
            //        g.DrawLine(p, x + 4, y + 2, x + 4, y + h - 2);
            //}
        }

        /// <summary>
        ///     Draw the lines of the tree
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="p"></param>
        /// <param name="br"></param>
        /// <param name="glyphMidVertical"> </param>
        protected new virtual void DrawLines(Graphics g, Rectangle r, Pen p, TreeListView.Branch br, int glyphMidVertical) {
            Rectangle r2 = r;
            r2.Width = PIXELS_PER_LEVEL;

            // Vertical lines have to start on even points, otherwise the dotted line looks wrong.
            // This is only needed if pen is dotted.
            int top = r2.Top;

            // Draw lines for ancestors
            int midX;
            IList<TreeListView.Branch> ancestors = br.Ancestors;
            foreach (TreeListView.Branch ancestor in ancestors) {
                if (!ancestor.IsLastChild && !ancestor.IsOnlyBranch) {
                    midX = r2.Left + r2.Width/2;
                    g.DrawLine(p, midX, top, midX, r2.Bottom);
                }
                r2.Offset(PIXELS_PER_LEVEL, 0);
            }

            // Draw lines for this branch
            midX = r2.Left + r2.Width/2;

            // Horizontal line first
            g.DrawLine(p, midX, glyphMidVertical, r2.Right, glyphMidVertical);

            // Vertical line second
            if (br.IsFirstBranch) {
                if (!br.IsLastChild && !br.IsOnlyBranch)
                    g.DrawLine(p, midX, glyphMidVertical, midX, r2.Bottom);
            } else {
                if (br.IsLastChild)
                    g.DrawLine(p, midX, top, midX, glyphMidVertical);
                else
                    g.DrawLine(p, midX, top, midX, r2.Bottom);
            }
        }
        #endregion


        #region Text Rendering
        // This class has two implement two highlighting schemes: one for GDI, another for GDI+.
        // Naturally, GDI+ makes the task easier, but we have to provide something for GDI
        // since that it is what is normally used.

        /// <summary>
        /// Draw text using GDI
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected override void DrawTextGdi(Graphics g, Rectangle r, string txt) {
            if (ShouldDrawHighlighting)
                DrawGdiTextHighlighting(g, r, txt);
            base.DrawTextGdi(g, r, txt);
        }

        /// <summary>
        /// Draw the highlighted text using GDI
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected override void DrawGdiTextHighlighting(Graphics g, Rectangle r, string txt) {
            const TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform;

            // TextRenderer puts horizontal padding around the strings, so we need to take
            // that into account when measuring strings
            const int paddingAdjustment = 6;

            // Cache the font
            Font f = Font;

            foreach (CharacterRange range in txt.ToLower().FindAllMatchedRanges(_filterStr)) {
                // Measure the text that comes before our substring
                Size precedingTextSize = Size.Empty;
                if (range.First > 0) {
                    string precedingText = txt.Substring(0, range.First);
                    precedingTextSize = TextRenderer.MeasureText(g, precedingText, f, r.Size, flags);
                    precedingTextSize.Width -= paddingAdjustment;
                }

                // Measure the length of our substring (may be different each time due to case differences)
                string highlightText = txt.Substring(range.First, range.Length);
                Size textToHighlightSize = TextRenderer.MeasureText(g, highlightText, f, r.Size, flags);
                textToHighlightSize.Width -= paddingAdjustment;

                float textToHighlightLeft = r.X + precedingTextSize.Width + 1;
                float textToHighlightTop = AlignVertically(r, textToHighlightSize.Height);

                // Draw a filled frame around our substring
                DrawSubstringFrame(g, textToHighlightLeft, textToHighlightTop, textToHighlightSize.Width, textToHighlightSize.Height);
            }
        }

        /// <summary>
        /// Draw an indication around the given frame that shows a text match
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected override void DrawSubstringFrame(Graphics g, float x, float y, float width, float height) {
            if (_useRoundedRectangle) {
                using (GraphicsPath path = GetRoundedRect(x, y, width, height, 3.0f)) {
                    if (_fillBrush != null)
                        g.FillPath(_fillBrush, path);
                    if (_framePen != null)
                        g.DrawPath(_framePen, path);
                }
            } else {
                if (_fillBrush != null)
                    g.FillRectangle(_fillBrush, x, y, width, height);
                if (_framePen != null)
                    g.DrawRectangle(_framePen, x, y, width, height);
            }
        }

        /// <summary>
        /// Draw the text using GDI+
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected override void DrawTextGdiPlus(Graphics g, Rectangle r, string txt) {
            if (ShouldDrawHighlighting)
                DrawGdiPlusTextHighlighting(g, r, txt);
            base.DrawTextGdiPlus(g, r, txt);
        }

        /// <summary>
        /// Draw the highlighted text using GDI+
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected override void DrawGdiPlusTextHighlighting(Graphics g, Rectangle r, string txt) {
            // Find the substrings we want to highlight
            var ranges = new List<CharacterRange>(txt.ToLower().FindAllMatchedRanges(_filterStr));

            if (ranges.Count == 0)
                return;

            using (StringFormat fmt = StringFormatForGdiPlus) {
                RectangleF rf = r;
                fmt.SetMeasurableCharacterRanges(ranges.ToArray());
                Region[] stringRegions = g.MeasureCharacterRanges(txt, Font, rf, fmt);

                foreach (Region region in stringRegions) {
                    RectangleF bounds = region.GetBounds(g);
                    DrawSubstringFrame(g, bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height);
                }
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Gets whether the renderer should actually draw highlighting
        /// </summary>
        protected new bool ShouldDrawHighlighting {
            get { return Column == null || (Column.Searchable); }
        }
        #endregion
    }
}