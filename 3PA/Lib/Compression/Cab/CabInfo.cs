#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CabInfo.cs) is part of 3P.
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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WixToolset.Dtf.Compression.Cab {
    /// <summary>
    /// Object representing a cabinet file on disk; provides access to
    /// file-based operations on the cabinet file.
    /// </summary>
    /// <remarks>
    /// Generally, the methods on this class are much easier to use than the
    /// stream-based interfaces provided by the <see cref="CabEngine"/> class.
    /// </remarks>
    [Serializable]
    public class CabInfo : ArchiveInfo {
        /// <summary>
        /// Creates a new CabinetInfo object representing a cabinet file in a specified path.
        /// </summary>
        /// <param name="path">The path to the cabinet file. When creating a cabinet file, this file does not
        /// necessarily exist yet.</param>
        public CabInfo(string path)
            : base(path) {}

        /// <summary>
        /// Initializes a new instance of the CabinetInfo class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected CabInfo(SerializationInfo info, StreamingContext context)
            : base(info, context) {}

        /// <summary>
        /// Creates a compression engine that does the low-level work for
        /// this object.
        /// </summary>
        /// <returns>A new <see cref="CabEngine"/> instance.</returns>
        /// <remarks>
        /// Each instance will be <see cref="CompressionEngine.Dispose()"/>d
        /// immediately after use.
        /// </remarks>
        protected override CompressionEngine CreateCompressionEngine() {
            return new CabEngine();
        }

        /// <summary>
        /// Gets information about the files contained in the archive.
        /// </summary>
        /// <returns>A list of <see cref="CabFileInfo"/> objects, each
        /// containing information about a file in the archive.</returns>
        public new IList<CabFileInfo> GetFiles() {
            IList<ArchiveFileInfo> files = base.GetFiles();
            List<CabFileInfo> cabFiles = new List<CabFileInfo>(files.Count);
            foreach (CabFileInfo cabFile in files) cabFiles.Add(cabFile);
            return cabFiles.AsReadOnly();
        }

        /// <summary>
        /// Gets information about the certain files contained in the archive file.
        /// </summary>
        /// <param name="searchPattern">The search string, such as
        /// &quot;*.txt&quot;.</param>
        /// <returns>A list of <see cref="CabFileInfo"/> objects, each containing
        /// information about a file in the archive.</returns>
        public new IList<CabFileInfo> GetFiles(string searchPattern) {
            IList<ArchiveFileInfo> files = base.GetFiles(searchPattern);
            List<CabFileInfo> cabFiles = new List<CabFileInfo>(files.Count);
            foreach (CabFileInfo cabFile in files) cabFiles.Add(cabFile);
            return cabFiles.AsReadOnly();
        }
    }
}