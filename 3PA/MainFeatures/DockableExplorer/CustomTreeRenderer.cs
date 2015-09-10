using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Themes;

namespace _3PA.MainFeatures.DockableExplorer {
    /// <summary>
    ///     This class handles drawing the tree structure of the primary column.
    /// </summary>
    public class CustomTreeRenderer : TreeListView.TreeRenderer {
        /// <summary>
        ///     How many pixels will be reserved for each level of indentation?
        /// </summary>
        public static int PIXELS_PER_LEVEL = 16 + 1;

        private bool isShowLines = true;

        /// <summary>
        ///     Return the branch that the renderer is currently drawing.
        /// </summary>
        private TreeListView.Branch Branch {
            get { return TreeListView.TreeModel.GetBranch(RowObject); }
        }

        /// <summary>
        ///     Return the pen that will be used to draw the lines between branches
        /// </summary>
        public Pen LinePen { get; set; }

        /// <summary>
        ///     Return the TreeListView for which the renderer is being used.
        /// </summary>
        public TreeListView TreeListView {
            get { return (TreeListView) ListView; }
        }

        /// <summary>
        ///     Should the renderer draw lines connecting siblings?
        /// </summary>
        public bool IsShowLines {
            get { return isShowLines; }
            set { isShowLines = value; }
        }

        /// <summary>
        ///     Gets whether or not we should render using styles
        /// </summary>
        protected virtual bool UseStyles {
            get { return !IsPrinting; }
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

        #region Configuration properties

        /// <summary>
        /// Gets or set how rounded will be the corners of the text match frame
        /// </summary>
        [Category("Appearance"),
         DefaultValue(3.0f),
         Description("How rounded will be the corners of the text match frame?")]
        public float CornerRoundness {
            get { return _cornerRoundness; }
            set { _cornerRoundness = value; }
        }

        private float _cornerRoundness = 4.0f;

        /// <summary>
        /// Gets or set the brush will be used to paint behind the matched substrings.
        /// Set this to null to not fill the frame.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush FillBrush {
            get { return _fillBrush; }
            set { _fillBrush = value; }
        }

        private Brush _fillBrush;

        /// <summary>
        /// Gets or sets the filter that is filtering the ObjectListView and for
        /// which this renderer should highlight text
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextMatchFilter Filter {
            get { return _filter; }
            set { _filter = value; }
        }

        private TextMatchFilter _filter;

        /// <summary>
        /// Gets or set the pen will be used to frame the matched substrings.
        /// Set this to null to not draw a frame.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen FramePen {
            get { return _framePen; }
            set { _framePen = value; }
        }

        private Pen _framePen;

        /// <summary>
        /// Gets or sets whether the frame around a text match will have rounded corners
        /// </summary>
        [Category("Appearance"),
         DefaultValue(true),
         Description("Will the frame around a text match will have rounded corners?")]
        public bool UseRoundedRectangle {
            get { return _useRoundedRectangle; }
            set { _useRoundedRectangle = value; }
        }

        private bool _useRoundedRectangle = true;

        #endregion

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
        /// Draw the highlighted text using GDI
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="txt"></param>
        protected virtual void DrawGdiTextHighlighting(Graphics g, Rectangle r, string txt) {
            TextFormatFlags flags = TextFormatFlags.NoPrefix |
                                    TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform;

            // TextRenderer puts horizontal padding around the strings, so we need to take
            // that into account when measuring strings
            int paddingAdjustment = 6;

            // Cache the font
            Font f = Font;

            foreach (CharacterRange range in Filter.FindAllMatchedRanges(txt)) {
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
        protected virtual void DrawSubstringFrame(Graphics g, float x, float y, float width, float height) {
            if (UseRoundedRectangle) {
                using (GraphicsPath path = GetRoundedRect(x, y, width, height, 3.0f)) {
                    if (FillBrush != null)
                        g.FillPath(FillBrush, path);
                    if (FramePen != null)
                        g.DrawPath(FramePen, path);
                }
            } else {
                if (FillBrush != null)
                    g.FillRectangle(FillBrush, x, y, width, height);
                if (FramePen != null)
                    g.DrawRectangle(FramePen, x, y, width, height);
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
        protected virtual void DrawGdiPlusTextHighlighting(Graphics g, Rectangle r, string txt) {
            // Find the substrings we want to highlight
            List<CharacterRange> ranges = new List<CharacterRange>(Filter.FindAllMatchedRanges(txt));

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
        protected bool ShouldDrawHighlighting {
            get { return Column == null || (Column.Searchable); }
        }

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <returns>A round cornered rectagle path</returns>
        /// <remarks>If I could rely on people using C# 3.0+, this should be
        /// an extension method of GraphicsPath.</remarks>        
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="diameter"></param>
        protected GraphicsPath GetRoundedRect(float x, float y, float width, float height, float diameter) {
            return GetRoundedRect(new RectangleF(x, y, width, height), diameter);
        }

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="diameter">The diameter of the corners</param>
        /// <returns>A round cornered rectagle path</returns>
        /// <remarks>If I could rely on people using C# 3.0+, this should be
        /// an extension method of GraphicsPath.</remarks>
        protected GraphicsPath GetRoundedRect(RectangleF rect, float diameter) {
            GraphicsPath path = new GraphicsPath();

            if (diameter > 0) {
                RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
            } else {
                path.AddRectangle(rect);
            }

            return path;
        }

        #endregion
    }
}