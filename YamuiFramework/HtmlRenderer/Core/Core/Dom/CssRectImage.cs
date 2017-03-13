#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CssRectImage.cs) is part of YamuiFramework.
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

namespace YamuiFramework.HtmlRenderer.Core.Core.Dom {
    /// <summary>
    /// Represents a word inside an inline box
    /// </summary>
    internal sealed class CssRectImage : CssRect {
        #region Fields and Consts

        /// <summary>
        /// the image object if it is image word (can be null if not loaded)
        /// </summary>
        private RImage _image;

        /// <summary>
        /// the image rectangle restriction as returned from image load event
        /// </summary>
        private RRect _imageRectangle;

        #endregion

        /// <summary>
        /// Creates a new BoxWord which represents an image
        /// </summary>
        /// <param name="owner">the CSS box owner of the word</param>
        public CssRectImage(CssBox owner)
            : base(owner) {}

        /// <summary>
        /// Gets the image this words represents (if one exists)
        /// </summary>
        public override RImage Image {
            get { return _image; }
            set { _image = value; }
        }

        /// <summary>
        /// Gets if the word represents an image.
        /// </summary>
        public override bool IsImage {
            get { return true; }
        }

        /// <summary>
        /// the image rectange restriction as returned from image load event
        /// </summary>
        public RRect ImageRectangle {
            get { return _imageRectangle; }
            set { _imageRectangle = value; }
        }

        /// <summary>
        /// Represents this word for debugging purposes
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return "Image";
        }
    }
}