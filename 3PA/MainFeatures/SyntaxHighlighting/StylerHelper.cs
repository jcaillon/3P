#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (StylerHelper.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System.Collections.Generic;
using System.Linq;

namespace _3PA.MainFeatures.SyntaxHighlighting {

    /// <summary>
    /// This class facilitates the use of StyeEx or SetStyles for annotations and syntax highlighting
    /// It creates the byte array of styles for the text it is fed with
    /// </summary>
    internal class StylerHelper {

        #region private fields

        private List<byte> _styleArray = new List<byte>();

        #endregion

        #region constructor

        #endregion

        #region public methods

        /// <summary>
        /// Style the given text with given style
        /// </summary>
        /// <param name="text"></param>
        /// <param name="styleId"></param>
        public void Style(string text, byte styleId) {
            _styleArray.AddRange(Enumerable.Repeat(styleId, text.Length));
        }

        /// <summary>
        /// Reset result byte array
        /// </summary>
        public void Clear() {
            _styleArray.Clear();
        }

        /// <summary>
        /// Get result byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetStyleArray() {
            return _styleArray.ToArray();
        }

        #endregion

    }
}
