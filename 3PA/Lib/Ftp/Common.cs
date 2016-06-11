#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (Common.cs) is part of 3P.
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
using System.Security.Authentication;

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

    public enum ETransferMode { Ascii, Binary }

    public enum ETextEncoding { Ascii, Utf8 }

    public class FtpReply {
        private int _code;
        private string _message;

        public int Code {
            get { return _code; }
            set { _code = value; }
        }

        public string Message {
            get { return _message; }
            set { _message = value; }
        }

        public override string ToString() {
            return string.Format("{0} {1}", Code, Message);
        }
    }

    public class DirectoryListItem {
        private string _flags;
        private string _owner;
        private string _group;
        private bool _isDirectory;
        private bool _isSymLink;
        private string _name;
        private ulong _size;
        private DateTime _creationTime;
        private string _symLinkTargetPath;

        public ulong Size {
            get { return _size; }
            set { _size = value; }
        }

        public string SymLinkTargetPath {
            get { return _symLinkTargetPath; }
            set { _symLinkTargetPath = value; }
        }

        public string Flags {
            get { return _flags; }
            set { _flags = value; }
        }

        public string Owner {
            get { return _owner; }
            set { _owner = value; }
        }

        public string Group {
            get { return _group; }
            set { _group = value; }
        }

        public bool IsDirectory {
            get { return _isDirectory; }
            set { _isDirectory = value; }
        }

        public bool IsSymLink {
            get { return _isSymLink; }
            set { _isSymLink = value; }
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public DateTime CreationTime {
            get { return _creationTime; }
            set { _creationTime = value; }
        }
    }

    /// <summary>
    /// Encapsulates the SSL/TLS algorithms connection information.
    /// </summary>
    public class SslInfo {
        SslProtocols _sslProtocol;

        CipherAlgorithmType _cipherAlgorithm;
        int _cipherStrength;

        HashAlgorithmType _hashAlgorithm;
        int _hashStrength;

        ExchangeAlgorithmType _keyExchangeAlgorithm;
        int _keyExchangeStrength;

        public SslProtocols SslProtocol {
            get { return _sslProtocol; }
            set { _sslProtocol = value; }
        }

        public CipherAlgorithmType CipherAlgorithm {
            get { return _cipherAlgorithm; }
            set { _cipherAlgorithm = value; }
        }

        public int CipherStrength {
            get { return _cipherStrength; }
            set { _cipherStrength = value; }
        }

        public HashAlgorithmType HashAlgorithm {
            get { return _hashAlgorithm; }
            set { _hashAlgorithm = value; }
        }

        public int HashStrength {
            get { return _hashStrength; }
            set { _hashStrength = value; }
        }

        public ExchangeAlgorithmType KeyExchangeAlgorithm {
            get { return _keyExchangeAlgorithm; }
            set { _keyExchangeAlgorithm = value; }
        }

        public int KeyExchangeStrength {
            get { return _keyExchangeStrength; }
            set { _keyExchangeStrength = value; }
        }

        public override string ToString() {
            return SslProtocol + ", " +
                   CipherAlgorithm + " (" + _cipherStrength + " bit), " +
                   KeyExchangeAlgorithm + " (" + _keyExchangeStrength + " bit), " +
                   HashAlgorithm + " (" + _hashStrength + " bit)";
        }
    }

    public class LogCommandEventArgs : EventArgs {
        public LogCommandEventArgs(string commandText) {
            CommandText = commandText;
        }

        public string CommandText { get; private set; }
    }

    public class LogServerReplyEventArgs : EventArgs {
        public LogServerReplyEventArgs(FtpReply serverReply) {
            ServerReply = serverReply;
        }

        public FtpReply ServerReply { get; private set; }
    }
}
