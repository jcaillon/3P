#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (CssRectWord.cs) is part of YamuiFramework.
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
namespace YamuiFramework.HtmlRenderer.Core.Core.Dom
{
    /// <summary>
    /// Represents a word inside an inline box
    /// </summary>
    internal sealed class CssRectWord : CssRect
    {
        #region Fields and Consts

        /// <summary>
        /// The word text
        /// </summary>
        private readonly string _text;

        /// <summary>
        /// was there a whitespace before the word chars (before trim)
        /// </summary>
        private readonly bool _hasSpaceBefore;

        /// <summary>
        /// was there a whitespace after the word chars (before trim)
        /// </summary>
        private readonly bool _hasSpaceAfter;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="owner">the CSS box owner of the word</param>
        /// <param name="text">the word chars </param>
        /// <param name="hasSpaceBefore">was there a whitespace before the word chars (before trim)</param>
        /// <param name="hasSpaceAfter">was there a whitespace after the word chars (before trim)</param>
        public CssRectWord(CssBox owner, string text, bool hasSpaceBefore, bool hasSpaceAfter)
            : base(owner)
        {
            _text = text;
            _hasSpaceBefore = hasSpaceBefore;
            _hasSpaceAfter = hasSpaceAfter;
        }

        /// <summary>
        /// was there a whitespace before the word chars (before trim)
        /// </summary>
        public override bool HasSpaceBefore
        {
            get { return _hasSpaceBefore; }
        }

        /// <summary>
        /// was there a whitespace after the word chars (before trim)
        /// </summary>
        public override bool HasSpaceAfter
        {
            get { return _hasSpaceAfter; }
        }

        /// <summary>
        /// Gets a bool indicating if this word is composed only by spaces.
        /// Spaces include tabs and line breaks
        /// </summary>
        public override bool IsSpaces
        {
            get
            {
                foreach (var c in Text)
                {
                    if (!char.IsWhiteSpace(c))
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets if the word is composed by only a line break
        /// </summary>
        public override bool IsLineBreak
        {
            get { return Text == "\n"; }
        }

        /// <summary>
        /// Gets the text of the word
        /// </summary>
        public override string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Represents this word for debugging purposes
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} ({1} char{2})", Text.Replace(' ', '-').Replace("\n", "\\n"), Text.Length, Text.Length != 1 ? "s" : string.Empty);
        }
    }
}