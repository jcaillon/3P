#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (CustomHighlightTextRenderer.cs) is part of 3P.
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Themes;
using _3PA.Lib;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This renderer highlights substrings that match a given text filter. 
    /// </summary>
    public class CustomHighlightTextRenderer : HighlightTextRenderer {

        /// <summary>
        /// Create a HighlightTextRenderer
        /// </summary>
        public CustomHighlightTextRenderer(string filterStr) {
            _fillBrush = new SolidBrush(ThemeManager.Current.AutoCompletionHighlightBack);
            _framePen = new Pen(ThemeManager.Current.AutoCompletionHighlightBorder);
            _filterStr = filterStr;
        }

        #region Configuration properties
        private string _filterStr;
        private Brush _fillBrush;
        private Pen _framePen;
        private bool _useRoundedRectangle = true;
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
