#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CrcStream.cs) is part of 3P.
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

namespace WixToolset.Dtf.Compression.Zip {
    /// <summary>
    /// Wraps a source stream and calcaluates a CRC over all bytes that are read or written.
    /// </summary>
    /// <remarks>
    /// The CRC algorithm matches that used in the standard ZIP file format.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Crc")]
    public class CrcStream : Stream {
        private Stream source;
        private uint crc;

        /// <summary>
        /// Creates a new CrcStream instance from a source stream.
        /// </summary>
        /// <param name="source">Underlying stream where bytes will be read from or written to.</param>
        public CrcStream(Stream source) {
            this.source = source;
        }

        /// <summary>
        /// Gets the current CRC over all bytes that have been read or written
        /// since this instance was created.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Crc")]
        public uint Crc {
            get { return crc; }
        }

        /// <summary>
        /// Gets the underlying stream that this stream reads from or writes to.
        /// </summary>
        public Stream Source {
            get { return source; }
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
        /// Gets the length of the source stream.
        /// </summary>
        public override long Length {
            get { return source.Length; }
        }

        /// <summary>
        /// Gets or sets the position of the source stream.
        /// </summary>
        public override long Position {
            get { return source.Position; }

            set { source.Position = value; }
        }

        /// <summary>
        /// Sets the position within the source stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating
        /// the reference point used to obtain the new position.</param>
        /// <returns>The new position within the source stream.</returns>
        /// <remarks>
        /// Note the CRC is only calculated over bytes that are actually read or
        /// written, so any bytes skipped by seeking will not contribute to the CRC.
        /// </remarks>
        public override long Seek(long offset, SeekOrigin origin) {
            return source.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the source stream.
        /// </summary>
        /// <param name="value">The desired length of the
        /// stream in bytes.</param>
        public override void SetLength(long value) {
            source.SetLength(value);
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
            count = source.Read(buffer, offset, count);
            UpdateCrc(buffer, offset, count);
            return count;
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
            UpdateCrc(buffer, offset, count);
        }

        /// <summary>
        /// Flushes the source stream.
        /// </summary>
        public override void Flush() {
            source.Flush();
        }

        /// <summary>
        /// Closes the underlying stream.
        /// </summary>
        public override void Close() {
            source.Close();
            base.Close();
        }

        /// <summary>
        /// Updates the CRC with a range of bytes that were read or written.
        /// </summary>
        private void UpdateCrc(byte[] buffer, int offset, int count) {
            crc = ~crc;
            for (; count > 0; count--, offset++) {
                crc = (crc >> 8) ^
                      crcTable[(crc & 0xFF) ^ buffer[offset]];
            }
            crc = ~crc;
        }

        private static uint[] crcTable = MakeCrcTable();

        /// <summary>
        /// Computes a table that speeds up calculation of the CRC.
        /// </summary>
        private static uint[] MakeCrcTable() {
            const uint poly = 0x04C11DB7u;
            uint[] crcTable = new uint[256];
            for (uint n = 0; n < 256; n++) {
                uint c = Reflect(n, 8);
                c = c << 24;
                for (uint k = 0; k < 8; k++) {
                    c = (c << 1) ^ ((c & 0x80000000u) != 0 ? poly : 0);
                }
                crcTable[n] = Reflect(c, 32);
            }
            return crcTable;
        }

        /// <summary>
        /// Reflects the ordering of certain number of bits. For exmample when reflecting
        /// one byte, bit one is swapped with bit eight, bit two with bit seven, etc.
        /// </summary>
        private static uint Reflect(uint value, int bits) {
            for (int i = 0; i < bits / 2; i++) {
                uint leftBit = 1u << (bits - 1 - i);
                uint rightBit = 1u << i;
                if (((value & leftBit) != 0) != ((value & rightBit) != 0)) {
                    value ^= leftBit | rightBit;
                }
            }
            return value;
        }
    }
}