#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (ImageAdapter.cs) is part of YamuiFramework.
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

namespace YamuiFramework.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Image object for core.
    /// </summary>
    internal sealed class ImageAdapter : RImage
    {
        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        private readonly Image _image;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ImageAdapter(Image image)
        {
            _image = image;
        }

        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        public Image Image
        {
            get { return _image; }
        }

        public override double Width
        {
            get { return _image.Width; }
        }

        public override double Height
        {
            get { return _image.Height; }
        }

        public override void Dispose()
        {
            _image.Dispose();
        }
    }
}