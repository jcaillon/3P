using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using BrightIdeasSoftware;
using YamuiFramework.Themes;

namespace _3PA.MainFeatures.DockableExplorer {
    /// <summary>
    ///     This class handles drawing the tree structure of the primary column.
    /// </summary>
    public class CustomTreeRenderer : TreeListView.TreeRenderer {


        /// <summary>
        ///     Return the branch that the renderer is currently drawing.
        /// </summary>
        private TreeListView.Branch Branch {
            get { return TreeListView.TreeModel.GetBranch(RowObject); }
        }

        /// <summary>
        ///     Create a TreeRenderer
        /// </summary>
        public CustomTreeRenderer() {
            LinePen = new Pen(ThemeManager.AccentColor, 1.0f);
            LinePen.DashStyle = DashStyle.Dot;
            FillBrush = new SolidBrush(ThemeManager.Current.AutoCompletionHighlightBack);
            FramePen = new Pen(ThemeManager.Current.AutoCompletionHighlightBorder);
        }


        /// <summary>
        ///     Create a TreeRenderer
        /// </summary>
        public CustomTreeRenderer(ObjectListView fastOvl, string filterStr) : this() {
            Filter = new TextMatchFilter(fastOvl, filterStr, StringComparison.OrdinalIgnoreCase);
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
                DrawLines(graphic, rect, LinePen, br, expandGlyphRectangleMidVertical);

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
            var h = 8;
            var w = 8;
            var x = r.X + 4;
            var y = r.Y + (r.Height / 2) - 4;

            using (var p = new Pen(ThemeManager.Current.ButtonColorsHoverBorderColor)) {
                g.DrawRectangle(p, new Rectangle(x, y, w, h));
            }
            using (var p = new SolidBrush(ThemeManager.Current.ButtonColorsHoverBackColor)) {
                g.FillRectangle(p, new Rectangle(x + 1, y + 1, w - 1, h - 1));
            }
            using (var p = new Pen(ThemeManager.Current.ButtonColorsHoverForeColor)) {
            g.DrawLine(p, x + 2, y + 4, x + w - 2, y + 4);
            if (!isExpanded)
                g.DrawLine(p, x + 4, y + 2, x + 4, y + h - 2);
            }
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
            //if (p.DashStyle == DashStyle.Dot && (top & 1) == 0)
            //    top += 1;

            // Draw lines for ancestors
            int midX;
            IList<TreeListView.Branch> ancestors = br.Ancestors;
            foreach (TreeListView.Branch ancestor in ancestors) {
                if (!ancestor.IsLastChild && !ancestor.IsOnlyBranch) {
                    midX = r2.Left + r2.Width / 2;
                    g.DrawLine(p, midX, top, midX, r2.Bottom);
                }
                r2.Offset(PIXELS_PER_LEVEL, 0);
            }

            // Draw lines for this branch
            midX = r2.Left + r2.Width / 2;

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

        #region IRenderer interface overrides

        /// <summary>
        /// Handle a HitTest request after all state information has been initialized
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cellBounds"></param>
        /// <param name="item"></param>
        /// <param name="subItemIndex"></param>
        /// <param name="preferredSize"> </param>
        /// <returns></returns>
        protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, int subItemIndex, Size preferredSize) {
            return StandardGetEditRectangle(g, cellBounds, preferredSize);
        }

        #endregion

        #region Rendering

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

        #endregion
    }
}