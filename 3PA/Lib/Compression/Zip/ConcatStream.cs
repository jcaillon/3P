// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

using System;
using System.IO;

namespace WixToolset.Dtf.Compression.Zip {
    /// <summary>
    /// Used to trick a DeflateStream into reading from or writing to
    /// a series of (chunked) streams instead of a single steream.
    /// </summary>
    internal class ConcatStream : Stream {
        private Stream source;
        private long position;
        private long length;
        private Action<ConcatStream> nextStreamHandler;

        public ConcatStream(Action<ConcatStream> nextStreamHandler) {
            if (nextStreamHandler == null) {
                throw new ArgumentNullException("nextStreamHandler");
            }

            this.nextStreamHandler = nextStreamHandler;
            length = Int64.MaxValue;
        }

        public Stream Source {
            get { return source; }
            set { source = value; }
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override long Length {
            get { return length; }
        }

        public override long Position {
            get { return position; }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (source == null) {
                nextStreamHandler(this);
            }

            count = (int) Math.Min(count, length - position);

            int bytesRemaining = count;
            while (bytesRemaining > 0) {
                if (source == null) {
                    throw new InvalidOperationException();
                }

                int partialCount = (int) Math.Min(bytesRemaining,
                    source.Length - source.Position);

                if (partialCount == 0) {
                    nextStreamHandler(this);
                    continue;
                }

                partialCount = source.Read(
                    buffer, offset + count - bytesRemaining, partialCount);
                bytesRemaining -= partialCount;
                position += partialCount;
            }

            return count;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (source == null) {
                nextStreamHandler(this);
            }

            int bytesRemaining = count;
            while (bytesRemaining > 0) {
                if (source == null) {
                    throw new InvalidOperationException();
                }

                int partialCount = (int) Math.Min(bytesRemaining,
                    Math.Max(0, length - source.Position));

                if (partialCount == 0) {
                    nextStreamHandler(this);
                    continue;
                }

                source.Write(
                    buffer, offset + count - bytesRemaining, partialCount);
                bytesRemaining -= partialCount;
                position += partialCount;
            }
        }

        public override void Flush() {
            if (source != null) {
                source.Flush();
            }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            length = value;
        }

        public override void Close() {
            if (source != null) {
                source.Close();
            }
        }
    }
}