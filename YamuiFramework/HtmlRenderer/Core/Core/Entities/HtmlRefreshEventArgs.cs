#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlRefreshEventArgs.cs) is part of YamuiFramework.
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

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities {
    /// <summary>
    /// Raised when html renderer requires refresh of the control hosting (invalidation and re-layout).<br/>
    /// It can happen if some async event has occurred that requires re-paint and re-layout of the html.<br/>
    /// Example: async download of image is complete.
    /// </summary>
    public sealed class HtmlRefreshEventArgs : EventArgs {
        /// <summary>
        /// is re-layout is required for the refresh
        /// </summary>
        private readonly bool _layout;

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="layout">is re-layout is required for the refresh</param>
        public HtmlRefreshEventArgs(bool layout) {
            _layout = layout;
        }

        /// <summary>
        /// is re-layout is required for the refresh
        /// </summary>
        public bool Layout {
            get { return _layout; }
        }

        public override string ToString() {
            return string.Format("Layout: {0}", _layout);
        }
    }
}