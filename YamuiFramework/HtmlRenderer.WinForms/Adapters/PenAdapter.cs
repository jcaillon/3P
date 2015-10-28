#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (PenAdapter.cs) is part of YamuiFramework.
// 
// // YamuiFramework is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // YamuiFramework is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System.Drawing;
using YamuiFramework.HtmlRenderer.Core.Adapters;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;

namespace YamuiFramework.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms pens objects for core.
    /// </summary>
    internal sealed class PenAdapter : RPen
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        private readonly Pen _pen;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(Pen pen)
        {
            _pen = pen;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public Pen Pen
        {
            get { return _pen; }
        }

        public override double Width
        {
            get { return _pen.Width; }
            set { _pen.Width = (float)value; }
        }

        public override RDashStyle DashStyle
        {
            set
            {
                switch (value)
                {
                    case RDashStyle.Solid:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        break;
                    case RDashStyle.Dash:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        if (Width < 2)
                            _pen.DashPattern = new[] { 4, 4f }; // better looking
                        break;
                    case RDashStyle.Dot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        break;
                    case RDashStyle.DashDot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
                        break;
                    case RDashStyle.Custom:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                        break;
                    default:
                        _pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        break;
                }
            }
        }
    }
}