#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (LinkElementData.cs) is part of YamuiFramework.
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
namespace YamuiFramework.HtmlRenderer.Core.Core.Entities {
    /// <summary>
    /// Holds data on link element in HTML.<br/>
    /// Used to expose data outside of HTML Renderer internal structure.
    /// </summary>
    public sealed class LinkElementData<T> {
        /// <summary>
        /// the id of the link element if present
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// the href data of the link
        /// </summary>
        private readonly string _href;

        /// <summary>
        /// the rectangle of element as calculated by html layout
        /// </summary>
        private readonly T _rectangle;

        /// <summary>
        /// Init.
        /// </summary>
        public LinkElementData(string id, string href, T rectangle) {
            _id = id;
            _href = href;
            _rectangle = rectangle;
        }

        /// <summary>
        /// the id of the link element if present
        /// </summary>
        public string Id {
            get { return _id; }
        }

        /// <summary>
        /// the href data of the link
        /// </summary>
        public string Href {
            get { return _href; }
        }

        /// <summary>
        /// the rectangle of element as calculated by html layout
        /// </summary>
        public T Rectangle {
            get { return _rectangle; }
        }

        /// <summary>
        /// Is the link is directed to another element in the html
        /// </summary>
        public bool IsAnchor {
            get { return _href.Length > 0 && _href[0] == '#'; }
        }

        /// <summary>
        /// Return the id of the element this anchor link is referencing.
        /// </summary>
        public string AnchorId {
            get { return IsAnchor && _href.Length > 1 ? _href.Substring(1) : string.Empty; }
        }

        public override string ToString() {
            return string.Format("Id: {0}, Href: {1}, Rectangle: {2}", _id, _href, _rectangle);
        }
    }
}