#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (CssBlockSelectorItem.cs) is part of YamuiFramework.
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
using YamuiFramework.HtmlRenderer.Core.Core.Utils;

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities
{
    /// <summary>
    /// Holds single class selector in css block hierarchical selection (p class1 > div.class2)
    /// </summary>
    public struct CssBlockSelectorItem
    {
        #region Fields and Consts

        /// <summary>
        /// the name of the css class of the block
        /// </summary>
        private readonly string _class;

        /// <summary>
        /// is the selector item has to be direct parent
        /// </summary>
        private readonly bool _directParent;

        #endregion


        /// <summary>
        /// Creates a new block from the block's source
        /// </summary>
        /// <param name="class">the name of the css class of the block</param>
        /// <param name="directParent"> </param>
        public CssBlockSelectorItem(string @class, bool directParent)
        {
            ArgChecker.AssertArgNotNullOrEmpty(@class, "@class");

            _class = @class;
            _directParent = directParent;
        }

        /// <summary>
        /// the name of the css class of the block
        /// </summary>
        public string Class
        {
            get { return _class; }
        }

        /// <summary>
        /// is the selector item has to be direct parent
        /// </summary>
        public bool DirectParent
        {
            get { return _directParent; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        public override string ToString()
        {
            return _class + (_directParent ? " > " : string.Empty);
        }
    }
}