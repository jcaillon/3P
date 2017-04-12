#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (IPackStreamContext.cs) is part of 3P.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace WixToolset.Dtf.Compression {
    /// <summary>
    /// This interface provides the methods necessary for the
    /// <see cref="CompressionEngine"/> to open and close streams for archives
    /// and files. The implementor of this interface can use any kind of logic
    /// to determine what kind of streams to open and where.
    /// </summary>
    public interface IPackStreamContext {
        /// <summary>
        /// Gets the name of the archive with a specified number.
        /// </summary>
        /// <param name="archiveNumber">The 0-based index of the archive
        /// within the chain.</param>
        /// <returns>The name of the requested archive. May be an empty string
        /// for non-chained archives, but may never be null.</returns>
        /// <remarks>The archive name is the name stored within the archive, used for
        /// identification of the archive especially among archive chains. That
        /// name is often, but not necessarily the same as the filename of the
        /// archive package.</remarks>
        string GetArchiveName(int archiveNumber);

        /// <summary>
        /// Opens a stream for writing an archive package.
        /// </summary>
        /// <param name="archiveNumber">The 0-based index of the archive within
        /// the chain.</param>
        /// <param name="archiveName">The name of the archive that was returned
        /// by <see cref="GetArchiveName"/>.</param>
        /// <param name="truncate">True if the stream should be truncated when
        /// opened (if it already exists); false if an existing stream is being
        /// re-opened for writing additional data.</param>
        /// <param name="compressionEngine">Instance of the compression engine
        /// doing the operations.</param>
        /// <returns>A writable Stream where the compressed archive bytes will be
        /// written, or null to cancel the archive creation.</returns>
        /// <remarks>
        /// If this method returns null, the archive engine will throw a
        /// FileNotFoundException.
        /// </remarks>
        Stream OpenArchiveWriteStream(
            int archiveNumber,
            string archiveName,
            bool truncate,
            CompressionEngine compressionEngine);

        /// <summary>
        /// Closes a stream where an archive package was written.
        /// </summary>
        /// <param name="archiveNumber">The 0-based index of the archive within
        /// the chain.</param>
        /// <param name="archiveName">The name of the archive that was previously
        /// returned by
        /// <see cref="GetArchiveName"/>.</param>
        /// <param name="stream">A stream that was previously returned by
        /// <see cref="OpenArchiveWriteStream"/> and is now ready to be closed.</param>
        /// <remarks>
        /// If there is another archive package in the chain, then after this stream
        /// is closed a new stream will be opened.
        /// </remarks>
        void CloseArchiveWriteStream(int archiveNumber, string archiveName, Stream stream);

        /// <summary>
        /// Opens a stream to read a file that is to be included in an archive.
        /// </summary>
        /// <param name="path">The path of the file within the archive. This is often,
        /// but not necessarily, the same as the relative path of the file outside
        /// the archive.</param>
        /// <param name="attributes">Returned attributes of the opened file, to be
        /// stored in the archive.</param>
        /// <param name="lastWriteTime">Returned last-modified time of the opened file,
        /// to be stored in the archive.</param>
        /// <returns>A readable Stream where the file bytes will be read from before
        /// they are compressed, or null to skip inclusion of the file and continue to
        /// the next file.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        Stream OpenFileReadStream(
            string path,
            out FileAttributes attributes,
            out DateTime lastWriteTime);

        /// <summary>
        /// Closes a stream that has been used to read a file.
        /// </summary>
        /// <param name="path">The path of the file within the archive; the same as
        /// the path provided
        /// when the stream was opened.</param>
        /// <param name="stream">A stream that was previously returned by
        /// <see cref="OpenFileReadStream"/> and is now ready to be closed.</param>
        void CloseFileReadStream(string path, Stream stream);

        /// <summary>
        /// Gets extended parameter information specific to the compression
        /// format being used.
        /// </summary>
        /// <param name="optionName">Name of the option being requested.</param>
        /// <param name="parameters">Parameters for the option; for per-file options,
        /// the first parameter is typically the internal file path.</param>
        /// <returns>Option value, or null to use the default behavior.</returns>
        /// <remarks>
        /// This method provides a way to set uncommon options during packaging, or a
        /// way to handle aspects of compression formats not supported by the base library.
        /// <para>For example, this may be used by the zip compression library to
        /// specify different compression methods/levels on a per-file basis.</para>
        /// <para>The available option names, parameters, and expected return values
        /// should be documented by each compression library.</para>
        /// </remarks>
        object GetOption(string optionName, object[] parameters);
    }
}