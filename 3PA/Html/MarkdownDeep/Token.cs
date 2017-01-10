#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Token.cs) is part of 3P.
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
namespace MarkdownDeep
{
	/*
	 * Token is used to mark out various special parts of a string being
	 * formatted by SpanFormatter.
	 * 
	 * Strings aren't actually stored in the token - just their offset
	 * and length in the input string.
	 * 
	 * For performance, Token's are pooled and reused.  
	 * See SpanFormatter.CreateToken()
	 */

	// TokenType - what sort of token?
	internal enum TokenType
	{
		Text,			// Plain text, should be htmlencoded
		HtmlTag,		// Valid html tag, write out directly but escape &amps;
		Html,			// Valid html, write out directly
		open_em,		// <em>
		close_em,		// </em>
		open_strong,	// <strong>
		close_strong,	// </strong>
		code_span,		// <code></code>
		br,				// <br />

		link,			// <a href>, data = LinkInfo
		img,			// <img>, data = LinkInfo
		footnote,		// Footnote reference
		abbreviation,	// An abbreviation, data is a reference to Abbrevation instance

		// These are used during construction of <em> and <strong> tokens
		opening_mark,	// opening '*' or '_'
		closing_mark,	// closing '*' or '_'
		internal_mark	// internal '*' or '_'
	}

	// Token
	internal class Token
	{
		// Constructor
		public Token(TokenType type, int startOffset, int length)
		{
			this.type = type;
			this.startOffset = startOffset;
			this.length = length;
		}

		// Constructor
		public Token(TokenType type, object data)
		{
			this.type = type;
			this.data = data;
		}

		public override string ToString() {
		    if (true || data == null)
			{
				return string.Format("{0} - {1} - {2}", type, startOffset, length);
			}
		    return string.Format("{0} - {1} - {2} -> {3}", type, startOffset, length, data);
		}

	    public TokenType type;
		public int startOffset;
		public int length;
		public object data;
	}

}
