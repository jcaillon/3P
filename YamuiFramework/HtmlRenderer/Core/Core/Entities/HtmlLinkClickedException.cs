#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlLinkClickedException.cs) is part of YamuiFramework.
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

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities {
    /// <summary>
    /// Exception thrown when client code subscribed to LinkClicked event thrown exception.
    /// </summary>
    public sealed class HtmlLinkClickedException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class.
        /// </summary>
        public HtmlLinkClickedException() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public HtmlLinkClickedException(string message)
            : base(message) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param><param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public HtmlLinkClickedException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}