#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (OffsetStream.cs) is part of 3P.
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
using System.IO;

namespace WixToolset.Dtf.Compression {
    /// <summary>
    /// Wraps a source stream and offsets all read/write/seek calls by a given value.
    /// </summary>
    /// <remarks>
    /// This class is used to trick archive an packing or unpacking process
    /// into reading or writing at an offset into a file, primarily for
    /// self-extracting packages.
    /// </remarks>
    public class OffsetStream : Stream {
        private Stream source;
        private long sourceOffset;

        /// <summary>
        /// Creates a new OffsetStream instance from a source stream
        /// and using a specified offset.
        /// </summary>
        /// <param name="source">Underlying stream for which all calls will be offset.</param>
        /// <param name="offset">Positive or negative number of bytes to offset.</param>
        public OffsetStream(Stream source, long offset) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            this.source = source;
            sourceOffset = offset;

            this.source.Seek(sourceOffset, SeekOrigin.Current);
        }

        /// <summary>
        /// Gets the underlying stream that this OffsetStream calls into.
        /// </summary>
        public Stream Source {
            get { return source; }
        }

        /// <summary>
        /// Gets the number of bytes to offset all calls before
        /// redirecting to the underlying stream.
        /// </summary>
        public long Offset {
            get { return sourceOffset; }
        }

        /// <summary>
        /// Gets a value indicating whether the source stream supports reading.
        /// </summary>
        /// <value>true if the stream supports reading; otherwise, false.</value>
        public override bool CanRead {
            get { return source.CanRead; }
        }

        /// <summary>
        /// Gets a value indicating whether the source stream supports writing.
        /// </summary>
        /// <value>true if the stream supports writing; otherwise, false.</value>
        public override bool CanWrite {
            get { return source.CanWrite; }
        }

        /// <summary>
        /// Gets a value indicating whether the source stream supports seeking.
        /// </summary>
        /// <value>true if the stream supports seeking; otherwise, false.</value>
        public override bool CanSeek {
            get { return source.CanSeek; }
        }

        /// <summary>
        /// Gets the effective length of the stream, which is equal to
        /// the length of the source stream minus the offset.
        /// </summary>
        public override long Length {
            get { return source.Length - sourceOffset; }
        }

        /// <summary>
        /// Gets or sets the effective position of the stream, which
        /// is equal to the position of the source stream minus the offset.
        /// </summary>
        public override long Position {
            get { return source.Position - sourceOffset; }
            set { source.Position = value + sourceOffset; }
        }

        /// <summary>
        /// Reads a sequence of bytes from the source stream and advances
        /// the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer
        /// contains the specified byte array with the values between offset and
        /// (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin
        /// storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less
        /// than the number of bytes requested if that many bytes are not currently available,
        /// or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count) {
            return source.Read(buffer, offset, count);
        }

        /// <summary>
        /// Writes a sequence of bytes to the source stream and advances the
        /// current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count
        /// bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which
        /// to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the
        /// current stream.</param>
        public override void Write(byte[] buffer, int offset, int count) {
            source.Write(buffer, offset, count);
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the
        /// source stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the
        /// end of the stream.</returns>
        public override int ReadByte() {
            return source.ReadByte();
        }

        /// <summary>
        /// Writes a byte to the current position in the source stream and
        /// advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        public override void WriteByte(byte value) {
            source.WriteByte(value);
        }

        /// <summary>
        /// Flushes the source stream.
        /// </summary>
        public override void Flush() {
            source.Flush();
        }

        /// <summary>
        /// Sets the position within the current stream, which is
        /// equal to the position within the source stream minus the offset.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating
        /// the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin) {
            return source.Seek(offset + (origin == SeekOrigin.Begin ? sourceOffset : 0), origin) - sourceOffset;
        }

        /// <summary>
        /// Sets the effective length of the stream, which is equal to
        /// the length of the source stream minus the offset.
        /// </summary>
        /// <param name="value">The desired length of the
        /// current stream in bytes.</param>
        public override void SetLength(long value) {
            source.SetLength(value + sourceOffset);
        }

        /// <summary>
        /// Closes the underlying stream.
        /// </summary>
        public override void Close() {
            source.Close();
        }
    }
}