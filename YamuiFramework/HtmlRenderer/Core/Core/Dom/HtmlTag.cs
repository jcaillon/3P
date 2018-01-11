#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlTag.cs) is part of YamuiFramework.
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
using System.Collections.Generic;
using YamuiFramework.HtmlRenderer.Core.Core.Utils;

namespace YamuiFramework.HtmlRenderer.Core.Core.Dom {
    internal sealed class HtmlTag {
        #region Fields and Consts

        /// <summary>
        /// the name of the html tag
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// if the tag is single placed; in other words it doesn't have a separate closing tag;
        /// </summary>
        private readonly bool _isSingle;

        /// <summary>
        /// collection of attributes and their value the html tag has
        /// </summary>
        private readonly Dictionary<string, string> _attributes;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="name">the name of the html tag</param>
        /// <param name="isSingle">if the tag is single placed; in other words it doesn't have a separate closing tag;</param>
        /// <param name="attributes">collection of attributes and their value the html tag has</param>
        public HtmlTag(string name, bool isSingle, Dictionary<string, string> attributes = null) {
            ArgChecker.AssertArgNotNullOrEmpty(name, "name");

            _name = name;
            _isSingle = isSingle;
            _attributes = attributes;
        }

        /// <summary>
        /// Gets the name of this tag
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// Gets collection of attributes and their value the html tag has
        /// </summary>
        public Dictionary<string, string> Attributes {
            get { return _attributes; }
        }

        /// <summary>
        /// Gets if the tag is single placed; in other words it doesn't have a separate closing tag; <br/>
        /// e.g. &lt;br&gt;
        /// </summary>
        public bool IsSingle {
            get { return _isSingle; }
        }

        /// <summary>
        /// is the html tag has attributes.
        /// </summary>
        /// <returns>true - has attributes, false - otherwise</returns>
        public bool HasAttributes() {
            return _attributes != null && _attributes.Count > 0;
        }

        /// <summary>
        /// Gets a boolean indicating if the attribute list has the specified attribute
        /// </summary>
        /// <param name="attribute">attribute name to check if exists</param>
        /// <returns>true - attribute exists, false - otherwise</returns>
        public bool HasAttribute(string attribute) {
            return _attributes != null && _attributes.ContainsKey(attribute);
        }

        /// <summary>
        /// Get attribute value for given attribute name or null if not exists.
        /// </summary>
        /// <param name="attribute">attribute name to get by</param>
        /// <param name="defaultValue">optional: value to return if attribute is not specified</param>
        /// <returns>attribute value or null if not found</returns>
        public string TryGetAttribute(string attribute, string defaultValue = null) {
            return _attributes != null && _attributes.ContainsKey(attribute) ? _attributes[attribute] : defaultValue;
        }

        public override string ToString() {
            return string.Format("<{0}>", _name);
        }
    }
}