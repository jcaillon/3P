#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (FontFamilyAdapter.cs) is part of YamuiFramework.
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
    /// Adapter for WinForms Font family object for core.
    /// </summary>
    internal sealed class FontFamilyAdapter : RFontFamily
    {
        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        private readonly FontFamily _fontFamily;

        /// <summary>
        /// Init.
        /// </summary>
        public FontFamilyAdapter(FontFamily fontFamily)
        {
            _fontFamily = fontFamily;
        }

        /// <summary>
        /// the underline win-forms font family.
        /// </summary>
        public FontFamily FontFamily
        {
            get { return _fontFamily; }
        }

        public override string Name
        {
            get { return _fontFamily.Name; }
        }
    }
}