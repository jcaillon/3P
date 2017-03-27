#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (FtpStream.cs) is part of 3P.
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

using System.IO;

namespace _3PA.Lib.Ftp {
    /*
     *  Copyright 2008 Alessandro Pilotti
     *
     *  This program is free software; you can redistribute it and/or modify
     *  it under the terms of the GNU Lesser General Public License as published by
     *  the Free Software Foundation; either version 2.1 of the License, or
     *  (at your option) any later version.
     *
     *  This program is distributed in the hope that it will be useful,
     *  but WITHOUT ANY WARRANTY; without even the implied warranty of
     *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     *  GNU Lesser General Public License for more details.
     *
     *  You should have received a copy of the GNU Lesser General Public License
     *  along with this program; if not, write to the Free Software
     *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA 
     */

    internal delegate void FtpStreamCallback();

    /// <summary>
    /// Incapsulates a Stream used during FTP get and put commands.
    /// </summary>
    public class FtpStream : Stream {
        public enum EAllowedOperation {
            Read = 1,
            Write = 2
        }

        Stream _innerStream;
        FtpStreamCallback _streamClosedCallback;
        EAllowedOperation _allowedOp;

        internal FtpStream(Stream innerStream, EAllowedOperation allowedOp, FtpStreamCallback streamClosedCallback) {
            _innerStream = innerStream;
            _streamClosedCallback = streamClosedCallback;
            _allowedOp = allowedOp;
        }

        public override bool CanRead {
            get { return _innerStream.CanRead && (_allowedOp & EAllowedOperation.Read) == EAllowedOperation.Read; }
        }

        public override bool CanSeek {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite {
            get { return _innerStream.CanWrite && (_allowedOp & EAllowedOperation.Write) == EAllowedOperation.Write; }
        }

        public override void Flush() {
            _innerStream.Flush();
        }

        public override long Length {
            get { return _innerStream.Length; }
        }

        public override long Position {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (!CanRead)
                throw new FtpException("Operation not allowed");

            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (!CanWrite)
                throw new FtpException("Operation not allowed");

            _innerStream.Write(buffer, offset, count);
        }

        public override void Close() {
            base.Close();
            _streamClosedCallback();
        }
    }
}