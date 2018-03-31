#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ZipException.cs) is part of 3P.
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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace _3PA.Lib.Compression.Zip {
    /// <summary>
    /// Exception class for zip operations.
    /// </summary>
    [Serializable]
    public class ZipException : ArchiveException {
        /// <summary>
        /// Creates a new ZipException with a specified error message and a reference to the
        /// inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the
        /// innerException parameter is not a null reference (Nothing in Visual Basic), the current exception
        /// is raised in a catch block that handles the inner exception.</param>
        public ZipException(string message, Exception innerException)
            : base(message, innerException) {}

        /// <summary>
        /// Creates a new ZipException with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ZipException(string message)
            : this(message, null) {}

        /// <summary>
        /// Creates a new ZipException.
        /// </summary>
        public ZipException()
            : this(null, null) {}

        /// <summary>
        /// Initializes a new instance of the ZipException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected ZipException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        /// <summary>
        /// Sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
        }
    }
}