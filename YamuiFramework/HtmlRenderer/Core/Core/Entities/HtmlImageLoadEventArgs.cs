#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlImageLoadEventArgs.cs) is part of YamuiFramework.
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
using System.Collections.Generic;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;
using YamuiFramework.HtmlRenderer.Core.Core.Utils;

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities {
    /// <summary>
    /// Callback used in <see cref="HtmlImageLoadEventArgs"/> to allow setting image externally and async.<br/>
    /// The callback can provide path to image file path, URL or the actual image to use.<br/>
    /// If <paramref name="imageRectangle"/> is given (not <see cref="RRect.Empty"/>) then only the specified rectangle will
    /// be used from the loaded image and not all of it, also the rectangle will be used for size and not the actual image size.<br/> 
    /// </summary>
    /// <param name="path">the path to the image to load (file path or URL)</param>
    /// <param name="image">the image to use</param>
    /// <param name="imageRectangle">optional: limit to specific rectangle in the loaded image</param>
    public delegate void HtmlImageLoadCallback(string path, Object image, RRect imageRectangle);

    /// <summary>
    /// Invoked when an image is about to be loaded by file path, URL or inline data in 'img' element or background-image CSS style.<br/>
    /// Allows to overwrite the loaded image by providing the image object manually, or different source (file or URL) to load from.<br/>
    /// Example: image 'src' can be non-valid string that is interpreted in the overwrite delegate by custom logic to resource image object<br/>
    /// Example: image 'src' in the html is relative - the overwrite intercepts the load and provide full source URL to load the image from<br/>
    /// Example: image download requires authentication - the overwrite intercepts the load, downloads the image to disk using custom code and 
    /// provide file path to load the image from. Can also use the asynchronous image overwrite not to block HTML rendering is applicable.<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </summary>
    public sealed class HtmlImageLoadEventArgs : EventArgs {
        #region Fields and Consts

        /// <summary>
        /// use to cancel the image loading by html renderer, the provided image will be used.
        /// </summary>
        private bool _handled;

        /// <summary>
        /// the source of the image (file path or uri)
        /// </summary>
        private readonly string _src;

        /// <summary>
        /// collection of all the attributes that are defined on the image element
        /// </summary>
        private readonly Dictionary<string, string> _attributes;

        /// <summary>
        /// Callback used to allow setting image externally and async.
        /// </summary>
        private readonly HtmlImageLoadCallback _callback;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="src">the source of the image (file path or Uri)</param>
        /// <param name="attributes">collection of all the attributes that are defined on the image element</param>
        /// <param name="callback">Callback used to allow setting image externally and async.</param>
        internal HtmlImageLoadEventArgs(string src, Dictionary<string, string> attributes, HtmlImageLoadCallback callback) {
            _src = src;
            _attributes = attributes;
            _callback = callback;
        }

        /// <summary>
        /// the source of the image (file path, URL or inline data)
        /// </summary>
        public string Src {
            get { return _src; }
        }

        /// <summary>
        /// collection of all the attributes that are defined on the image element or CSS style
        /// </summary>
        public Dictionary<string, string> Attributes {
            get { return _attributes; }
        }

        /// <summary>
        /// Indicate the image load is handled asynchronously.
        /// Cancel this image loading and overwrite the image asynchronously using callback method.<br/>
        /// </summary>
        public bool Handled {
            get { return _handled; }
            set { _handled = value; }
        }

        /// <summary>
        /// Callback to overwrite the loaded image with error image.<br/>
        /// Can be called directly from delegate handler or asynchronously after setting <see cref="Handled"/> to True.<br/>
        /// </summary>
        public void Callback() {
            _handled = true;
            _callback(null, null, new RRect());
        }

        /// <summary>
        /// Callback to overwrite the loaded image with image to load from given URI.<br/>
        /// Can be called directly from delegate handler or asynchronously after setting <see cref="Handled"/> to True.<br/>
        /// </summary>
        /// <param name="path">the path to the image to load (file path or URL)</param>
        public void Callback(string path) {
            ArgChecker.AssertArgNotNullOrEmpty(path, "path");

            _handled = true;
            _callback(path, null, RRect.Empty);
        }

        /// <summary>
        /// Callback to overwrite the loaded image with image to load from given URI.<br/>
        /// Can be called directly from delegate handler or asynchronously after setting <see cref="Handled"/> to True.<br/>
        /// Only the specified rectangle (x,y,width,height) will be used from the loaded image and not all of it, also 
        /// the rectangle will be used for size and not the actual image size.<br/> 
        /// </summary>
        /// <param name="path">the path to the image to load (file path or URL)</param>
        /// <param name="imageRectangle">optional: limit to specific rectangle of the image and not all of it</param>
        public void Callback(string path, double x, double y, double width, double height) {
            ArgChecker.AssertArgNotNullOrEmpty(path, "path");

            _handled = true;
            _callback(path, null, new RRect(x, y, width, height));
        }

        /// <summary>
        /// Callback to overwrite the loaded image with given image object.<br/>
        /// Can be called directly from delegate handler or asynchronously after setting <see cref="Handled"/> to True.<br/>
        /// If <paramref name="imageRectangle"/> is given (not <see cref="RRect.Empty"/>) then only the specified rectangle will
        /// be used from the loaded image and not all of it, also the rectangle will be used for size and not the actual image size.<br/> 
        /// </summary>
        /// <param name="image">the image to load</param>
        public void Callback(Object image) {
            ArgChecker.AssertArgNotNull(image, "image");

            _handled = true;
            _callback(null, image, RRect.Empty);
        }

        /// <summary>
        /// Callback to overwrite the loaded image with given image object.<br/>
        /// Can be called directly from delegate handler or asynchronously after setting <see cref="Handled"/> to True.<br/>
        /// Only the specified rectangle (x,y,width,height) will be used from the loaded image and not all of it, also 
        /// the rectangle will be used for size and not the actual image size.<br/> 
        /// </summary>
        /// <param name="image">the image to load</param>
        /// <param name="imageRectangle">optional: limit to specific rectangle of the image and not all of it</param>
        public void Callback(Object image, double x, double y, double width, double height) {
            ArgChecker.AssertArgNotNull(image, "image");

            _handled = true;
            _callback(null, image, new RRect(x, y, width, height));
        }
    }
}