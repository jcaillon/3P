#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (OutputFormat.cs) is part of 3P.
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
namespace _3PA.Lib.CommonMark {
    /// <summary>
    /// Specifies different formatters supported by the converter.
    /// </summary>
    public enum OutputFormat {
        /// <summary>
        /// The output is standard HTML format according to the CommonMark specification.
        /// </summary>
        Html,

        /// <summary>
        /// The output is a debug view of the syntax tree. Usable for debugging.
        /// </summary>
        SyntaxTree,

        /// <summary>
        /// The output is written using a delegate function specified in <see cref="CommonMarkSettings.OutputDelegate"/>.
        /// </summary>
        CustomDelegate
    }
}