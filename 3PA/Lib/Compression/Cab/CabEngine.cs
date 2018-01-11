#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (CabEngine.cs) is part of 3P.
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
using System.IO;

namespace WixToolset.Dtf.Compression.Cab {
    /// <summary>
    /// Engine capable of packing and unpacking archives in the cabinet format.
    /// </summary>
    public class CabEngine : CompressionEngine {
        private CabPacker packer;
        private CabUnpacker unpacker;

        /// <summary>
        /// Disposes of resources allocated by the cabinet engine.
        /// </summary>
        /// <param name="disposing">If true, the method has been called directly
        /// or indirectly by a user's code, so managed and unmanaged resources
        /// will be disposed. If false, the method has been called by the runtime
        /// from inside the finalizer, and only unmanaged resources will be
        /// disposed.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (packer != null) {
                    packer.Dispose();
                    packer = null;
                }
                if (unpacker != null) {
                    unpacker.Dispose();
                    unpacker = null;
                }
            }

            base.Dispose(disposing);
        }

        private CabPacker Packer {
            get {
                if (packer == null) {
                    packer = new CabPacker(this);
                }

                return packer;
            }
        }

        private CabUnpacker Unpacker {
            get {
                if (unpacker == null) {
                    unpacker = new CabUnpacker(this);
                }

                return unpacker;
            }
        }

        /// <summary>
        /// Creates a cabinet or chain of cabinets.
        /// </summary>
        /// <param name="streamContext">A context interface to handle opening
        /// and closing of cabinet and file streams.</param>
        /// <param name="files">The paths of the files in the archive (not
        /// external file paths).</param>
        /// <param name="maxArchiveSize">The maximum number of bytes for one
        /// cabinet before the contents are chained to the next cabinet, or zero
        /// for unlimited cabinet size.</param>
        /// <exception cref="ArchiveException">The cabinet could not be
        /// created.</exception>
        /// <remarks>
        /// The stream context implementation may provide a mapping from the
        /// file paths within the cabinet to the external file paths.
        /// <para>Smaller folder sizes can make it more efficient to extract
        /// individual files out of large cabinet packages.</para>
        /// </remarks>
        public override void Pack(
            IPackStreamContext streamContext,
            IEnumerable<string> files,
            long maxArchiveSize) {
            Packer.CompressionLevel = CompressionLevel;
            Packer.UseTempFiles = UseTempFiles;
            Packer.Pack(streamContext, files, maxArchiveSize);
        }

        /// <summary>
        /// Checks whether a Stream begins with a header that indicates
        /// it is a valid cabinet file.
        /// </summary>
        /// <param name="stream">Stream for reading the cabinet file.</param>
        /// <returns>True if the stream is a valid cabinet file
        /// (with no offset); false otherwise.</returns>
        public override bool IsArchive(Stream stream) {
            return Unpacker.IsArchive(stream);
        }

        /// <summary>
        /// Gets information about files in a cabinet or cabinet chain.
        /// </summary>
        /// <param name="streamContext">A context interface to handle opening
        /// and closing of cabinet and file streams.</param>
        /// <param name="fileFilter">A predicate that can determine
        /// which files to process, optional.</param>
        /// <returns>Information about files in the cabinet stream.</returns>
        /// <exception cref="ArchiveException">The cabinet provided
        /// by the stream context is not valid.</exception>
        /// <remarks>
        /// The <paramref name="fileFilter"/> predicate takes an internal file
        /// path and returns true to include the file or false to exclude it.
        /// </remarks>
        public override IList<ArchiveFileInfo> GetFileInfo(
            IUnpackStreamContext streamContext,
            Predicate<string> fileFilter) {
            return Unpacker.GetFileInfo(streamContext, fileFilter);
        }

        /// <summary>
        /// Extracts files from a cabinet or cabinet chain.
        /// </summary>
        /// <param name="streamContext">A context interface to handle opening
        /// and closing of cabinet and file streams.</param>
        /// <param name="fileFilter">An optional predicate that can determine
        /// which files to process.</param>
        /// <exception cref="ArchiveException">The cabinet provided
        /// by the stream context is not valid.</exception>
        /// <remarks>
        /// The <paramref name="fileFilter"/> predicate takes an internal file
        /// path and returns true to include the file or false to exclude it.
        /// </remarks>
        public override void Unpack(
            IUnpackStreamContext streamContext,
            Predicate<string> fileFilter) {
            Unpacker.Unpack(streamContext, fileFilter);
        }

        internal void ReportProgress(ArchiveProgressEventArgs e) {
            OnProgress(e);
        }
    }
}