#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CommonMarkAdditionalFeatures.cs) is part of 3P.
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

using System;

namespace _3PA.Lib.CommonMark {
    /// <summary>
    /// Lists additional features that can be enabled in <see cref="CommonMarkSettings"/>.
    /// These features are not part of the standard and should not be used if interoperability with other
    /// CommonMark implementations is required.
    /// </summary>
    [Flags]
    public enum CommonMarkAdditionalFeatures {
        /// <summary>
        /// No additional features are enabled. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// The parser will recognize syntax <c>~~foo~~</c> that will be rendered as <c>&lt;del&gt;foo&lt;/del&gt;</c>.
        /// </summary>
        StrikethroughTilde = 1,

        /// <summary>
        /// The parser will recognize syntax <c>[foo]</c>, which will be encoded in a separate AST node that the host application may evaluate as desired.
        /// </summary>
        PlaceholderBracket = 2,

        /// <summary>
        /// All additional features are enabled.
        /// </summary>
        All = 0x7FFFFFFF
    }
}