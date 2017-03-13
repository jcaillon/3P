#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlStylesheetLoadEventArgs.cs) is part of YamuiFramework.
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

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities {
    /// <summary>
    /// Invoked when a stylesheet is about to be loaded by file path or URL in 'link' element.<br/>
    /// Allows to overwrite the loaded stylesheet by providing the stylesheet data manually, or different source (file or URL) to load from.<br/>
    /// Example: The stylesheet 'href' can be non-valid URI string that is interpreted in the overwrite delegate by custom logic to pre-loaded stylesheet object<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </summary>
    public sealed class HtmlStylesheetLoadEventArgs : EventArgs {
        #region Fields and Consts

        /// <summary>
        /// the source of the stylesheet as found in the HTML (file path or URL)
        /// </summary>
        private readonly string _src;

        /// <summary>
        /// collection of all the attributes that are defined on the link element
        /// </summary>
        private readonly Dictionary<string, string> _attributes;

        /// <summary>
        /// provide the new source (file path or URL) to load stylesheet from
        /// </summary>
        private string _setSrc;

        /// <summary>
        /// provide the stylesheet to load
        /// </summary>
        private string _setStyleSheet;

        /// <summary>
        /// provide the stylesheet data to load
        /// </summary>
        private CssData _setStyleSheetData;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="src">the source of the image (file path or URL)</param>
        /// <param name="attributes">collection of all the attributes that are defined on the image element</param>
        internal HtmlStylesheetLoadEventArgs(string src, Dictionary<string, string> attributes) {
            _src = src;
            _attributes = attributes;
        }

        /// <summary>
        /// the source of the stylesheet as found in the HTML (file path or URL)
        /// </summary>
        public string Src {
            get { return _src; }
        }

        /// <summary>
        /// collection of all the attributes that are defined on the link element
        /// </summary>
        public Dictionary<string, string> Attributes {
            get { return _attributes; }
        }

        /// <summary>
        /// provide the new source (file path or URL) to load stylesheet from
        /// </summary>
        public string SetSrc {
            get { return _setSrc; }
            set { _setSrc = value; }
        }

        /// <summary>
        /// provide the stylesheet to load
        /// </summary>
        public string SetStyleSheet {
            get { return _setStyleSheet; }
            set { _setStyleSheet = value; }
        }

        /// <summary>
        /// provide the stylesheet data to load
        /// </summary>
        public CssData SetStyleSheetData {
            get { return _setStyleSheetData; }
            set { _setStyleSheetData = value; }
        }
    }
}