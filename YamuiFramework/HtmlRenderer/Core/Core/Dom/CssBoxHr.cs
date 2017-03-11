#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CssBoxHr.cs) is part of YamuiFramework.
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
using YamuiFramework.HtmlRenderer.Core.Adapters;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;
using YamuiFramework.HtmlRenderer.Core.Core.Handlers;
using YamuiFramework.HtmlRenderer.Core.Core.Parse;
using YamuiFramework.HtmlRenderer.Core.Core.Utils;

namespace YamuiFramework.HtmlRenderer.Core.Core.Dom {
    /// <summary>
    /// CSS box for hr element.
    /// </summary>
    internal sealed class CssBoxHr : CssBox {
        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parent">the parent box of this box</param>
        /// <param name="tag">the html tag data of this box</param>
        public CssBoxHr(CssBox parent, HtmlTag tag)
            : base(parent, tag) {
            Display = CssConstants.Block;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.<br/>
        /// Performs layout of the DOM structure creating lines by set bounds restrictions.
        /// </summary>
        /// <param name="g">Device context to use</param>
        protected override void PerformLayoutImp(RGraphics g) {
            if (Display == CssConstants.None)
                return;

            RectanglesReset();

            var prevSibling = DomUtils.GetPreviousSibling(this);
            double left = ContainingBlock.Location.X + ContainingBlock.ActualPaddingLeft + ActualMarginLeft + ContainingBlock.ActualBorderLeftWidth;
            double top = (prevSibling == null && ParentBox != null ? ParentBox.ClientTop : ParentBox == null ? Location.Y : 0) + MarginTopCollapse(prevSibling) + (prevSibling != null ? prevSibling.ActualBottom + prevSibling.ActualBorderBottomWidth : 0);
            Location = new RPoint(left, top);
            ActualBottom = top;

            //width at 100% (or auto)
            double minwidth = GetMinimumWidth();
            double width = ContainingBlock.Size.Width
                           - ContainingBlock.ActualPaddingLeft - ContainingBlock.ActualPaddingRight
                           - ContainingBlock.ActualBorderLeftWidth - ContainingBlock.ActualBorderRightWidth
                           - ActualMarginLeft - ActualMarginRight - ActualBorderLeftWidth - ActualBorderRightWidth;

            //Check width if not auto
            if (Width != CssConstants.Auto && !string.IsNullOrEmpty(Width)) {
                width = CssValueParser.ParseLength(Width, width, this);
            }

            if (width < minwidth || width >= 9999)
                width = minwidth;

            double height = ActualHeight;
            if (height < 1) {
                height = Size.Height + ActualBorderTopWidth + ActualBorderBottomWidth;
            }
            if (height < 1) {
                height = 2;
            }
            if (height <= 2 && ActualBorderTopWidth < 1 && ActualBorderBottomWidth < 1) {
                BorderTopStyle = BorderBottomStyle = CssConstants.Solid;
                BorderTopWidth = "1px";
                BorderBottomWidth = "1px";
            }

            Size = new RSize(width, height);

            ActualBottom = Location.Y + ActualPaddingTop + ActualPaddingBottom + height;
        }

        /// <summary>
        /// Paints the fragment
        /// </summary>
        /// <param name="g">the device to draw to</param>
        protected override void PaintImp(RGraphics g) {
            var offset = HtmlContainer != null ? HtmlContainer.ScrollOffset : RPoint.Empty;
            var rect = new RRect(Bounds.X + offset.X, Bounds.Y + offset.Y, Bounds.Width, Bounds.Height);

            if (rect.Height > 2 && RenderUtils.IsColorVisible(ActualBackgroundColor)) {
                g.DrawRectangle(g.GetSolidBrush(ActualBackgroundColor), rect.X, rect.Y, rect.Width, rect.Height);
            }

            var b1 = g.GetSolidBrush(ActualBorderTopColor);
            BordersDrawHandler.DrawBorder(Border.Top, g, this, b1, rect);

            if (rect.Height > 1) {
                var b2 = g.GetSolidBrush(ActualBorderLeftColor);
                BordersDrawHandler.DrawBorder(Border.Left, g, this, b2, rect);

                var b3 = g.GetSolidBrush(ActualBorderRightColor);
                BordersDrawHandler.DrawBorder(Border.Right, g, this, b3, rect);

                var b4 = g.GetSolidBrush(ActualBorderBottomColor);
                BordersDrawHandler.DrawBorder(Border.Bottom, g, this, b4, rect);
            }
        }
    }
}