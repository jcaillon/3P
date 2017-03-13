#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (GraphicsPathAdapter.cs) is part of YamuiFramework.
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
using System.Drawing.Drawing2D;
using YamuiFramework.HtmlRenderer.Core.Adapters;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;

namespace YamuiFramework.HtmlRenderer.WinForms.Adapters {
    /// <summary>
    /// Adapter for WinForms graphics path object for core.
    /// </summary>
    internal sealed class GraphicsPathAdapter : RGraphicsPath {
        /// <summary>
        /// The actual WinForms graphics path instance.
        /// </summary>
        private readonly GraphicsPath _graphicsPath = new GraphicsPath();

        /// <summary>
        /// the last point added to the path to begin next segment from
        /// </summary>
        private RPoint _lastPoint;

        /// <summary>
        /// The actual WinForms graphics path instance.
        /// </summary>
        public GraphicsPath GraphicsPath {
            get { return _graphicsPath; }
        }

        public override void Start(double x, double y) {
            _lastPoint = new RPoint(x, y);
        }

        public override void LineTo(double x, double y) {
            _graphicsPath.AddLine((float) _lastPoint.X, (float) _lastPoint.Y, (float) x, (float) y);
            _lastPoint = new RPoint(x, y);
        }

        public override void ArcTo(double x, double y, double size, Corner corner) {
            float left = (float) (Math.Min(x, _lastPoint.X) - (corner == Corner.TopRight || corner == Corner.BottomRight ? size : 0));
            float top = (float) (Math.Min(y, _lastPoint.Y) - (corner == Corner.BottomLeft || corner == Corner.BottomRight ? size : 0));
            _graphicsPath.AddArc(left, top, (float) size*2, (float) size*2, GetStartAngle(corner), 90);
            _lastPoint = new RPoint(x, y);
        }

        public override void Dispose() {
            _graphicsPath.Dispose();
        }

        /// <summary>
        /// Get arc start angle for the given corner.
        /// </summary>
        private static int GetStartAngle(Corner corner) {
            int startAngle;
            switch (corner) {
                case Corner.TopLeft:
                    startAngle = 180;
                    break;
                case Corner.TopRight:
                    startAngle = 270;
                    break;
                case Corner.BottomLeft:
                    startAngle = 90;
                    break;
                case Corner.BottomRight:
                    startAngle = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("corner");
            }
            return startAngle;
        }
    }
}