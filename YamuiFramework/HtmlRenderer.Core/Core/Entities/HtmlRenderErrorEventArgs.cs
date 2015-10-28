#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (HtmlRenderErrorEventArgs.cs) is part of YamuiFramework.
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
using System;

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities
{
    /// <summary>
    /// Raised when an error occurred during html rendering.
    /// </summary>
    public sealed class HtmlRenderErrorEventArgs : EventArgs
    {
        /// <summary>
        /// error type that is reported
        /// </summary>
        private readonly HtmlRenderErrorType _type;

        /// <summary>
        /// the error message
        /// </summary>
        private readonly string _message;

        /// <summary>
        /// the exception that occurred (can be null)
        /// </summary>
        private readonly Exception _exception;

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="type">the type of error to report</param>
        /// <param name="message">the error message</param>
        /// <param name="exception">optional: the exception that occurred</param>
        public HtmlRenderErrorEventArgs(HtmlRenderErrorType type, string message, Exception exception = null)
        {
            _type = type;
            _message = message;
            _exception = exception;
        }

        /// <summary>
        /// error type that is reported
        /// </summary>
        public HtmlRenderErrorType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// the error message
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

        /// <summary>
        /// the exception that occurred (can be null)
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        public override string ToString()
        {
            return string.Format("Type: {0}", _type);
        }
    }
}