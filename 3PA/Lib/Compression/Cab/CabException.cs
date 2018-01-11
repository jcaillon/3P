#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CabException.cs) is part of 3P.
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
using System.Globalization;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace WixToolset.Dtf.Compression.Cab {
    /// <summary>
    /// Exception class for cabinet operations.
    /// </summary>
    [Serializable]
    public class CabException : ArchiveException {
        private static ResourceManager errorResources;
        private int error;
        private int errorCode;

        /// <summary>
        /// Creates a new CabException with a specified error message and a reference to the
        /// inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the
        /// innerException parameter is not a null reference (Nothing in Visual Basic), the current exception
        /// is raised in a catch block that handles the inner exception.</param>
        public CabException(string message, Exception innerException)
            : this(0, 0, message, innerException) {}

        /// <summary>
        /// Creates a new CabException with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CabException(string message)
            : this(0, 0, message, null) {}

        /// <summary>
        /// Creates a new CabException.
        /// </summary>
        public CabException()
            : this(0, 0, null, null) {}

        internal CabException(int error, int errorCode, string message, Exception innerException)
            : base(message, innerException) {
            this.error = error;
            this.errorCode = errorCode;
        }

        internal CabException(int error, int errorCode, string message)
            : this(error, errorCode, message, null) {}

        /// <summary>
        /// Initializes a new instance of the CabException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected CabException(SerializationInfo info, StreamingContext context) : base(info, context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            error = info.GetInt32("cabError");
            errorCode = info.GetInt32("cabErrorCode");
        }

        /// <summary>
        /// Gets the FCI or FDI cabinet engine error number.
        /// </summary>
        /// <value>A cabinet engine error number, or 0 if the exception was
        /// not related to a cabinet engine error number.</value>
        public int Error {
            get { return error; }
        }

        /// <summary>
        /// Gets the Win32 error code.
        /// </summary>
        /// <value>A Win32 error code, or 0 if the exception was
        /// not related to a Win32 error.</value>
        public int ErrorCode {
            get { return errorCode; }
        }

        internal static ResourceManager ErrorResources {
            get {
                if (errorResources == null) {
                    errorResources = new ResourceManager(
                        typeof(CabException).Namespace + ".Errors",
                        typeof(CabException).Assembly);
                }
                return errorResources;
            }
        }

        /// <summary>
        /// Sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            info.AddValue("cabError", error);
            info.AddValue("cabErrorCode", errorCode);
            base.GetObjectData(info, context);
        }

        internal static string GetErrorMessage(int error, int errorCode, bool extracting) {
            const int FCI_ERROR_RESOURCE_OFFSET = 1000;
            const int FDI_ERROR_RESOURCE_OFFSET = 2000;
            int resourceOffset = (extracting ? FDI_ERROR_RESOURCE_OFFSET : FCI_ERROR_RESOURCE_OFFSET);

            string msg = ErrorResources.GetString(
                (resourceOffset + error).ToString(CultureInfo.InvariantCulture.NumberFormat),
                CultureInfo.CurrentCulture);

            if (msg == null) {
                msg = ErrorResources.GetString(
                    resourceOffset.ToString(CultureInfo.InvariantCulture.NumberFormat),
                    CultureInfo.CurrentCulture);
            }

            if (errorCode != 0) {
                const string GENERIC_ERROR_RESOURCE = "1";
                string msg2 = ErrorResources.GetString(GENERIC_ERROR_RESOURCE, CultureInfo.CurrentCulture);
                msg = String.Format(CultureInfo.InvariantCulture, "{0} " + msg2, msg, errorCode);
            }
            return msg;
        }
    }
}