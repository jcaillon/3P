#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (HtmlScrollEventArgs.cs) is part of YamuiFramework.
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
using System;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities
{
    /// <summary>
    /// Raised when Html Renderer request scroll to specific location.<br/>
    /// This can occur on document anchor click.
    /// </summary>
    public sealed class HtmlScrollEventArgs : EventArgs
    {
        /// <summary>
        /// the location to scroll to
        /// </summary>
        private readonly RPoint _location;

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="location">the location to scroll to</param>
        public HtmlScrollEventArgs(RPoint location)
        {
            _location = location;
        }

        /// <summary>
        /// the x location to scroll to
        /// </summary>
        public double X
        {
            get { return _location.X; }
        }

        /// <summary>
        /// the x location to scroll to
        /// </summary>
        public double Y
        {
            get { return _location.Y; }
        }

        public override string ToString()
        {
            return string.Format("Location: {0}", _location);
        }
    }
}