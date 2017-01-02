#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (BrushAdapter.cs) is part of YamuiFramework.
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
using System.Drawing;
using YamuiFramework.HtmlRenderer.Core.Adapters;

namespace YamuiFramework.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms brushes objects for core.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        private readonly Brush _brush;

        /// <summary>
        /// If to dispose the brush when <see cref="Dispose"/> is called.<br/>
        /// Ignore dispose for cached brushes.
        /// </summary>
        private readonly bool _dispose;

        /// <summary>
        /// Init.
        /// </summary>
        public BrushAdapter(Brush brush, bool dispose)
        {
            _brush = brush;
            _dispose = dispose;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public Brush Brush
        {
            get { return _brush; }
        }

        public override void Dispose()
        {
            if (_dispose)
            {
                _brush.Dispose();
            }
        }
    }
}