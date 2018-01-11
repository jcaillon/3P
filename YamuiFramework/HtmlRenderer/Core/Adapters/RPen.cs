#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (RPen.cs) is part of YamuiFramework.
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
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;

namespace YamuiFramework.HtmlRenderer.Core.Adapters {
    /// <summary>
    /// Adapter for platform specific pen objects - used to draw graphics (lines, rectangles and paths) 
    /// </summary>
    public abstract class RPen {
        /// <summary>
        /// Gets or sets the width of this Pen, in units of the Graphics object used for drawing.
        /// </summary>
        public abstract double Width { get; set; }

        /// <summary>
        /// Gets or sets the style used for dashed lines drawn with this Pen.
        /// </summary>
        public abstract RDashStyle DashStyle { set; }
    }
}