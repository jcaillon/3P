#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (FtpsClient.cs) is part of 3P.
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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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

    /// <summary>
    /// The SSL/TLS support requested or required for a connection.
    /// ((EsslSupportModeAttr)EsslSupportModeType.GetAttributes()).Value
    /// </summary>
    [Flags]
    public enum EsslSupportMode {
        /// <summary>
        /// No SSL/TLS support. Used for standard FTP connections.
        /// </summary>
        [Description("No SSL/TLS support. Used for standard FTP connections.")]
        ClearText = 0,

        /// <summary>
        /// Requests a SSL/TLS connection during authentication. 
        /// Authentication is performed using <see cref="ClearText"/> if SSL/TLS is not supported by the server.
        /// Reverts to <see cref="ClearText"/> after authetication if the CCC command is supported by the server.
        /// </summary>
        [Description("Requests a SSL/TLS connection during authentication.")]
        CredentialsRequested = 1,

        /// <summary>
        /// Requires a SSL/TLS connection during authentication. 
        /// Reverts to <see cref="ClearText"/> after authetication if the CCC command is supported by the server.
        /// </summary>
        [Description("Requires a SSL/TLS connection during authentication.")]
        CredentialsRequired = 2 | CredentialsRequested,

        /// <summary>
        /// Requests a SSL/TLS connection on the control channel. 
        /// </summary>
        /// <remarks>
        /// Acts like <see cref="CredentialsRequested"/> but does not revert to <see cref="ClearText"/> after authentication.
        /// </remarks>
        [Description("Requests a SSL/TLS connection on the control channel.")]
        ControlChannelRequested = 4 | CredentialsRequested,

        /// <summary>
        /// Requires a SSL/TLS connection on the control channel. 
        /// </summary>
        /// <remarks>
        /// Acts like <see cref="CredentialsRequired"/> but does not revert to <see cref="ClearText"/> after authentication.
        /// </remarks>
        [Description("Requires a SSL/TLS connection on the control channel.")]
        ControlChannelRequired = CredentialsRequired | ControlChannelRequested,

        /// <summary>
        /// Requests a SSL/TLS connection on the data channel, implies <see cref="CredentialsRequested"/>.
        /// Data transfers are not encrypted is not supported by the server.
        /// </summary>
        [Description("Requests a SSL/TLS connection on the data channel.")]
        DataChannelRequested = 8 | CredentialsRequested,

        /// <summary>
        /// Requires a SSL/TLS connection on the data channel, implies <see cref="CredentialsRequired"/>.
        /// </summary>
        [Description("Requires a SSL/TLS connection on the data channel.")]
        DataChannelRequired = 16 | DataChannelRequested | CredentialsRequired,

        /// <summary>
        /// Requests a SSL/TLS connection on both control and data channels, implies <see cref="ControlChannelRequested"/> and <see cref="DataChannelRequested"/>.
        /// Control channel commands and data transfers are not encrypted is not supported by the server.
        /// </summary>
        [Description("Requests a SSL/TLS connection on both control and data channels.")]
        ControlAndDataChannelsRequested = ControlChannelRequested | DataChannelRequested,

        /// <summary>
        /// Requires a SSL/TLS connection on both control and data channels, implies <see cref="ControlChannelRequired"/> and <see cref="DataChannelRequired"/>.
        /// </summary>
        [Description("Requires a SSL/TLS connection on both control and data channels.")]
        ControlAndDataChannelsRequired = ControlChannelRequired | DataChannelRequired,

        /// <summary>
        /// Implicit SSL/TLS, not supported by RFC 4217. Both control channel and data channel are always encrypted.
        /// </summary>
        [Description("Implicit SSL/TLS. Both control channel and data channel are always encrypted.")]
        Implicit = 32 | ControlAndDataChannelsRequired
    }

    /// <summary>
    /// Possible actions occurring during a file transfer.
    /// </summary>
    public enum ETransferActions {
        LocalDirectoryCreated,
        RemoteDirectoryCreated,
        FileUploaded,
        FileUploadingStatus,
        FileDownloaded,
        FileDownloadingStatus
    }

    /// <summary>
    /// File pattern style used in <see cref="FtpsClient.GetFiles"/> and  <see cref="FtpsClient.PutFiles"/>.
    /// </summary>
    public enum EPatternStyle {
        /// <summary>
        /// Interpret as is.
        /// </summary>
        Verbatim,

        /// <summary>
        /// Interpret as wildcard, where <c>*</c> means 0 or more chars having any value and <c>?</c> means one char having any value. 
        /// </summary>
        Wildcard,

        /// <summary>
        /// Interpret as a regular expression.
        /// </summary>
        Regex
    }

    /// <summary>
    /// Trasfer mode used in connections
    /// </summary>
    public enum EDataConnectionMode {
        Active,
        Passive
    }

    /// <summary>
    /// Callback used during file transfers to notify the caller about any command progress. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="action"></param>
    /// <param name="localObjectName"></param>
    /// <param name="remoteObjectName"></param>
    /// <param name="fileTransmittedBytes"></param>
    /// <param name="fileTransferSize"><c>null</c> if not available (e.g. the server does not support the SIZE command).</param>
    /// <param name="cancel"></param>
    public delegate void FileTransferCallback(FtpsClient sender, ETransferActions action,
        string localObjectName, string remoteObjectName,
        ulong fileTransmittedBytes, ulong? fileTransferSize,
        ref bool cancel);

    public delegate void LogCommandEventHandler(object sender, LogCommandEventArgs args);

    public delegate void LogServerReplyEventHandler(object sender, LogServerReplyEventArgs args);

    /// <summary>
    /// Class used to connect to an FTP/FTPS server. 
    /// The main goal of this class is to provide a complete and easy to use FTP client connection, implementing SSL/TLS extension 
    /// and providing tested compatibility to the main FTP server products. 
    /// 
    /// Implemented RFCs: 959, 2228, 2389, 2428, 2640, 3659, 4217. 
    /// 
    /// Not all commands described in the above RFCs are implemented at the current stage.
    /// </summary>
    /// <remarks>
    /// Requirements: MS Framework 2.0 and above or Mono 2.0 and above.
    /// </remarks>
    public sealed partial class FtpsClient : IDisposable {

        #region Private Enums

        enum EProtCode {
            C,
            S,
            E,
            P
        }

        enum EAuthMechanism {
            Tls
        }

        enum ERepType {
            A,
            E,
            I,
            L
        }

        #endregion

        #region Private Fields

        TcpClient _ctrlClient;
        StreamReader _ctrlSr;
        StreamWriter _ctrlSw;
        SslStream _ctrlSslStream;

        TcpClient _dataClient;
        SslStream _dataSslStream;

        EDataConnectionMode _dataConnectionMode = EDataConnectionMode.Passive;

        /// <summary>
        /// <c>true</c> to ignore the address returned by PASV
        /// </summary>
        bool _useCtrlEndPointAddressForData = true;

        bool _waitingCompletionReply;

        string _hostname;

        const string AnonUsername = "anonymous";
        const string AnonPassword = "randompwd"; // dummy password

        const string ClntName = "3P";

        const EsslSupportMode DefaultSslSupportMode = EsslSupportMode.CredentialsRequired | EsslSupportMode.DataChannelRequested;

        EsslSupportMode _sslSupportRequestedMode;
        EsslSupportMode _sslSupportCurrentMode;

        X509Certificate _sslServerCert;
        X509Certificate _sslClientCert;

        SslInfo _sslInfo;

        /// <summary>
        /// 0 means no check
        /// </summary>
        int _sslMinKeyExchangeAlgStrength;

        int _sslMinCipherAlgStrength;
        int _sslMinHashAlgStrength;

        bool _sslCheckCertRevocation = true;

        RemoteCertificateValidationCallback _userValidateServerCertificate;

        int _timeout = 120000; //ms

        IList<string> _features;

        ETransferMode _transferMode = ETransferMode.Ascii;
        ETextEncoding _textEncoding = ETextEncoding.Ascii;

        string _welcomeMessage;

        string _bannerMessage;

        Stack<string> _currDirStack = new Stack<string>();

        TcpListener _activeDataConnListener;

        Thread _keepAliveThread;
        volatile bool _keepAlive = true;
        int _keepAliveTimeout = 20000; // ms

        private bool _connected;

        #endregion

        #region Public Properties

        public EDataConnectionMode DataConnectionMode {
            get { return _dataConnectionMode; }
            set { _dataConnectionMode = value; }
        }

        /// <summary>
        /// The requested SSL/TLS support level.
        /// </summary>
        public EsslSupportMode SslSupportRequestedMode {
            get { return _sslSupportRequestedMode; }
        }

        /// <summary>
        /// The current SSL/TLS support level.
        /// </summary>
        public EsslSupportMode SslSupportCurrentMode {
            get { return _sslSupportCurrentMode; }
        }

        /// <summary>
        /// The current text encoding
        /// </summary>
        public ETextEncoding TextEncoding {
            get { return _textEncoding; }
        }

        /// <summary>
        /// The current transfer mode
        /// </summary>
        public ETransferMode TransferMode {
            get { return _transferMode; }
        }

        /// <summary>
        /// The welcome message returned by the server during connection.
        /// </summary>
        public string WelcomeMessage {
            get { return _welcomeMessage; }
        }

        /// <summary>
        /// The banner message returned by the server during connection.
        /// </summary>
        public string BannerMessage {
            get { return _bannerMessage; }
        }

        /// <summary>
        /// The server X.509 certificate used by the current connection
        /// </summary>
        /// <values><c>null</c> if the connection is not encrypted</value>
        public X509Certificate RemoteCertificate {
            get { return _ctrlSslStream != null ? _ctrlSslStream.RemoteCertificate : null; }
        }

        /// <summary>
        /// The key exchange, hash and cipher algorithms used by the SSL/TLS connection or <c>null</c> if encryption is not used.
        /// </summary>
        public SslInfo SslInfo {
            get { return _sslInfo; }
        }

        /// <summary>
        /// The client X.509 certificate used by the current connection
        /// </summary>
        /// <value><c>null</c> if the connection is not using a client certificate</value>
        public X509Certificate LocalCertificate {
            get { return _ctrlSslStream != null ? _ctrlSslStream.LocalCertificate : null; }
        }

        /// <summary>
        /// Returns true if the keep alive thread has been started
        /// </summary>
        public bool KeepAliveStarted {
            get { return _keepAliveThread != null; }
        }

        /// <summary>
        /// Is the ftp connected?
        /// </summary>
        public bool Connected {
            get { return _connected; }
            set { _connected = value; }
        }

        #endregion

        #region private Properties

        private bool IsControlChannelEncrypted {
            get { return _ctrlSslStream != null; }
        }

        private bool IsDataChannelOpen {
            get { return (_dataClient != null); }
        }

        #endregion

        #region Public Events

        public event LogCommandEventHandler LogCommand;
        public event LogServerReplyEventHandler LogServerReply;

        #endregion

        #region Public Methods

        /// <summary>
        /// Anonymous authentication
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns>The text of the \"welcome message\" sent by the server.</returns>
        public string Connect(string hostname) {
            return Connect(hostname, DefaultSslSupportMode);
        }

        /// <summary>
        /// Anonymous authentication
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns>The text of the \"welcome message\" sent by the server.</returns>
        public string Connect(string hostname, EsslSupportMode sslSupportMode) {
            return Connect(hostname, null, sslSupportMode);
        }

        public string Connect(string hostname, NetworkCredential credential) {
            return Connect(hostname, credential, DefaultSslSupportMode);
        }

        public string Connect(string hostname, NetworkCredential credential, EsslSupportMode sslSupportMode) {
            return Connect(hostname, credential, sslSupportMode, null);
        }

        public string Connect(string hostname, int port, NetworkCredential credential, EsslSupportMode sslSupportMode, int? timeout) {
            return Connect(hostname, port, credential, sslSupportMode, null, null, 0, 0, 0, timeout);
        }

        public string Connect(string hostname, NetworkCredential credential, EsslSupportMode sslSupportMode,
            RemoteCertificateValidationCallback userValidateServerCertificate) {
            // Default implicit FTPS port is 990, default standard and explicit FTPS port is 21
            int port = (sslSupportMode & EsslSupportMode.Implicit) == EsslSupportMode.Implicit ? 990 : 21;
            return Connect(hostname, port, credential, sslSupportMode, userValidateServerCertificate, null, 0, 0, 0, null);
        }

        /// <summary>
        /// Connects to a FTP server using the provided parameters. 
        /// The default representation tipe is set to Binary.
        /// The text encoding is set to UTF8, if supported by the server via the FEAT command.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="credential"></param>
        /// <param name="sslSupportMode"></param>
        /// <param name="userValidateServerCertificate"></param>
        /// <param name="x509ClientCert"></param>
        /// <param name="sslMinKeyExchangeAlgStrength"></param>
        /// <param name="sslMinCipherAlgStrength"></param>
        /// <param name="sslMinHashAlgStrength"></param>
        /// <param name="timeout">Connection timeout in ms. <c>null</c> can be specifiad to keep the default value of 120s.</param>
        /// <returns>The text of the \"welcome message\" sent by the server.</returns>
        public string Connect(string hostname, int port, NetworkCredential credential, EsslSupportMode sslSupportMode,
            RemoteCertificateValidationCallback userValidateServerCertificate, X509Certificate x509ClientCert,
            int sslMinKeyExchangeAlgStrength, int sslMinCipherAlgStrength, int sslMinHashAlgStrength,
            int? timeout) {
            return Connect(hostname, port, credential, sslSupportMode, userValidateServerCertificate, x509ClientCert,
                sslMinKeyExchangeAlgStrength, sslMinCipherAlgStrength, sslMinHashAlgStrength, timeout, true);
        }

        /// <summary>
        /// Connects to a FTP server using the provided parameters. 
        /// The default representation tipe is set to Binary.
        /// The text encoding is set to UTF8, if supported by the server via the FEAT command.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="credential"></param>
        /// <param name="sslSupportMode"></param>
        /// <param name="userValidateServerCertificate"></param>
        /// <param name="x509ClientCert"></param>
        /// <param name="sslMinKeyExchangeAlgStrength"></param>
        /// <param name="sslMinCipherAlgStrength"></param>
        /// <param name="sslMinHashAlgStrength"></param>
        /// <param name="timeout">Connection timeout in ms. <c>null</c> can be specifiad to keep the default value of 120s.</param>
        /// <param name="useCtrlEndPointAddressForData"><c>true</c> to use the control channel remote address for data connections instead of the address returned by PASV</param>
        /// <returns>The text of the \"welcome message\" sent by the server.</returns>
        public string Connect(string hostname, int port, NetworkCredential credential, EsslSupportMode sslSupportMode,
            RemoteCertificateValidationCallback userValidateServerCertificate, X509Certificate x509ClientCert,
            int sslMinKeyExchangeAlgStrength, int sslMinCipherAlgStrength, int sslMinHashAlgStrength,
            int? timeout, bool useCtrlEndPointAddressForData) {
            return Connect(hostname, port, credential, sslSupportMode, userValidateServerCertificate, x509ClientCert,
                sslMinKeyExchangeAlgStrength, sslMinCipherAlgStrength, sslMinHashAlgStrength, timeout, true, EDataConnectionMode.Passive);
        }

        /// <summary>
        /// Connects to a FTP server using the provided parameters. 
        /// The default representation tipe is set to Binary.
        /// The text encoding is set to UTF8, if supported by the server via the FEAT command.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="credential"></param>
        /// <param name="sslSupportMode"></param>
        /// <param name="userValidateServerCertificate"></param>
        /// <param name="x509ClientCert"></param>
        /// <param name="sslMinKeyExchangeAlgStrength"></param>
        /// <param name="sslMinCipherAlgStrength"></param>
        /// <param name="sslMinHashAlgStrength"></param>
        /// <param name="timeout">Connection timeout in ms. <c>null</c> can be specifiad to keep the default value of 120s.</param>
        /// <param name="useCtrlEndPointAddressForData"><c>true</c> to use the control channel remote address for data connections instead of the address returned by PASV</param>
        /// <param name="dataConnectionMode">Active or Passive data connection mode</param>
        /// <returns>The text of the \"welcome message\" sent by the server.</returns>
        public string Connect(string hostname, int port, NetworkCredential credential, EsslSupportMode sslSupportMode,
            RemoteCertificateValidationCallback userValidateServerCertificate, X509Certificate x509ClientCert,
            int sslMinKeyExchangeAlgStrength, int sslMinCipherAlgStrength, int sslMinHashAlgStrength,
            int? timeout, bool useCtrlEndPointAddressForData, EDataConnectionMode dataConnectionMode) {
            Close();

            // Anonymous authentication
            if (credential == null)
                credential = new NetworkCredential(AnonUsername, AnonPassword);

            if (timeout != null)
                _timeout = timeout.Value;

            _sslClientCert = x509ClientCert;

            _userValidateServerCertificate = userValidateServerCertificate;

            _sslMinKeyExchangeAlgStrength = sslMinKeyExchangeAlgStrength;
            _sslMinCipherAlgStrength = sslMinCipherAlgStrength;
            _sslMinHashAlgStrength = sslMinHashAlgStrength;

            _sslSupportRequestedMode = sslSupportMode;
            _sslSupportCurrentMode = sslSupportMode;

            _useCtrlEndPointAddressForData = useCtrlEndPointAddressForData;

            DataConnectionMode = dataConnectionMode;

            _sslInfo = null;

            _features = null;

            _transferMode = ETransferMode.Ascii;
            _textEncoding = ETextEncoding.Ascii;

            _bannerMessage = null;
            _welcomeMessage = null;

            _currDirStack.Clear();

            // Ok, member initialization is done. Start with setting up a control connection
            SetupCtrlConnection(hostname, port, Encoding.ASCII);

            // Used later for SSL/TLS auth
            _hostname = hostname;

            // Implicit SSL/TLS
            bool isImplicitSsl = (sslSupportMode & EsslSupportMode.Implicit) == EsslSupportMode.Implicit;
            if (isImplicitSsl)
                SwitchCtrlToSslMode();

            // Wait fot server message
            _bannerMessage = GetReply().Message;

            // Explicit SSL/TLS
            if (!isImplicitSsl)
                SslControlChannelCheckExplicitEncryptionRequest(sslSupportMode);

            // Login. Note that a password might not be required
            // TODO: check if the welcomeMessage is returned by the USER command in case the PASS command is not required.  
            if (UserCmd(credential.UserName))
                _welcomeMessage = PassCmd(credential.Password);

            GetFeaturesFromServer();

            if (IsControlChannelEncrypted)
                if (!isImplicitSsl) {
                    SslDataChannelCheckExplicitEncryptionRequest();

                    if ((sslSupportMode & EsslSupportMode.ControlChannelRequested) != EsslSupportMode.ControlChannelRequested)
                        SSlCtrlChannelCheckRevertToClearText();
                } else
                    SslDataChannelImplicitEncryptionRequest();

            try {
                // This is required by some FTP servers and must precede any OPTS command
                if (CheckFeature("CLNT"))
                    ClntCmd(ClntName);

                // Set UTF8 as character encoding, but only if listed among the FEAT features
                if (CheckFeature("UTF8"))
                    SetTextEncoding(ETextEncoding.Utf8);
            } catch (Exception) {
                //TODO: add warning info
            }

            // Default binary transfers
            SetTransferMode(ETransferMode.Binary);

            return _welcomeMessage;
        }

        private void KeepAliveThreadFunc() {
            while (_keepAlive) {
                try {
                    NoopCmd();
                    Thread.Sleep(_keepAliveTimeout);
                } catch {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Issues NOOP commands periodically in a separated thread
        /// </summary>
        public void StartKeepAlive() {
            CheckConnection();

            if (_keepAliveThread != null)
                throw new FtpException("KeepAlive already started");

            _keepAliveThread = new Thread(KeepAliveThreadFunc);
            _keepAliveThread.Start();
        }

        /// <summary>
        /// Stops the keep alive thread 
        /// </summary>
        public void StopKeepAlive() {
            if (_keepAliveThread != null) {
                _keepAlive = false;
                // Interrupt any sleep/wait operation 
                _keepAliveThread.Interrupt();
                _keepAliveThread.Join();
                _keepAliveThread = null;
            }
        }

        private void SslDataChannelImplicitEncryptionRequest() {
            try {
                // FileZilla server requires this
                if (CheckFeature("AUTH SSL") ||
                    CheckFeature("AUTH TLS") ||
                    (CheckFeature("PBSZ") && CheckFeature("PROT"))) {
                    PbszCmd(0);
                    ProtCmd(EProtCode.P);
                }
            } catch (Exception) {
                // Just ignore
            }
        }

        /// <summary>
        /// Set the representation type according to the given parameter
        /// </summary>
        /// <param name="transferMode"></param>
        public void SetTransferMode(ETransferMode transferMode) {
            TypeCmd(transferMode == ETransferMode.Ascii ? ERepType.A : ERepType.I, null);
            _transferMode = transferMode;
        }

        /// <summary>
        /// Features returned from the FEAT command
        /// </summary>
        /// <returns>null if the FEAT command is not supported by the server</returns>
        public IList<string> GetFeatures() {
            return _features != null ? new List<string>(_features) : null;
        }

        /// <summary>
        /// Remote transfer file size returned by the SIZE command.
        /// </summary>
        /// <param name="remoteFileName"></param>
        /// <returns>The file transmission size in bytes, based on the current transfer mode or <c>null</c> if the SIZE command is not supported.</returns>
        public ulong? GetFileTransferSize(string remoteFileName) {
            // RFC 3659 4.3. SIZE must be included in the FEAT reply
            return CheckFeature("SIZE") ? (ulong?) SizeCmd(remoteFileName) : null;
        }

        /// <summary>
        /// Retrieves the given file from the server. A <see cref="FtpStream"/> is returned, to be read until the end of file.
        /// See the <see cref="GetFile(string, string)"/> overload to easily save the stream to a local file. 
        /// </summary>
        /// <param name="remoteFileName"></param>
        /// <returns></returns>
        public FtpStream GetFile(string remoteFileName) {
            SetupDataConnection();
            RetrCmd(remoteFileName);
            return EndStreamCommand(FtpStream.EAllowedOperation.Read);
        }

        /// <summary>
        /// GetFile overload to easily transfer a file from remote to local
        /// </summary>
        /// <param name="remoteFileName"></param>
        /// <param name="localFileName"></param>
        /// <returns>Transferred bytes count.</returns>
        public ulong GetFile(string remoteFileName, string localFileName) {
            return GetFile(remoteFileName, localFileName, null);
        }

        /// <summary>
        /// GetFile overload to easily transfer a file from remote to local
        /// </summary>
        /// <param name="remoteFileName"></param>
        /// <param name="localFileName"></param>
        /// <param name="transferCallback"></param>
        /// <returns>Transferred bytes count.</returns>
        public ulong GetFile(string remoteFileName, string localFileName, FileTransferCallback transferCallback) {
            ulong totalBytes = 0;
            ulong? fileTransferSize = null;

            if (transferCallback != null)
                try {
                    fileTransferSize = GetFileTransferSize(remoteFileName);
                } catch (FtpCommandException ex) {
                    // Give a more detailed description, insteand of, e.g.: "Could not get file size".
                    if (ex.ErrorCode == 550)
                        throw new FtpException("Could not get the requested remote file", ex);
                    throw ex;
                }

            using (Stream s = GetFile(remoteFileName)) {
                using (FileStream fs = new FileStream(localFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    byte[] buf = new byte[1024];
                    int n = 0;
                    do {
                        CallTransferCallback(transferCallback, ETransferActions.FileDownloadingStatus, localFileName, remoteFileName, totalBytes, fileTransferSize);

                        n = s.Read(buf, 0, buf.Length);
                        if (n > 0) {
                            fs.Write(buf, 0, n);
                            totalBytes += (ulong) n;
                        }
                    } while (n > 0);

                    fs.Close();
                }
                s.Close();
            }

            CallTransferCallback(transferCallback, ETransferActions.FileDownloaded, localFileName, remoteFileName, totalBytes, fileTransferSize);

            return totalBytes;
        }

        /// <summary>
        /// Transfers all files, matching the given pattern, from the given remote directory. Optionally recursive
        /// </summary>
        /// <param name="remoteDirectoryName">Absolute or relative remote path, null indicates the current directory</param>
        /// <param name="localDirectoryName">Absolute local path</param>
        /// <param name="filePattern">May be null to transfer all files</param>
        /// <param name="patternStyle"></param>
        /// <param name="recursive"></param>
        /// <param name="transferCallback"></param>
        /// <remarks>
        /// </remarks>
        public void GetFiles(string remoteDirectoryName, string localDirectoryName,
            string filePattern, EPatternStyle patternStyle, bool recursive,
            FileTransferCallback transferCallback) {
            GetFiles(remoteDirectoryName, localDirectoryName, filePattern, patternStyle, recursive, transferCallback, new List<string>());
        }

        private void GetFiles(string remoteDirectoryName, string localDirectoryName, string filePattern, EPatternStyle patternStyle, bool recursive, FileTransferCallback transferCallback, IList<string> paths) {
            Regex regex = null;
            if (filePattern != null) {
                string fileRegexPattern = GetRegexPattern(filePattern, patternStyle);
                regex = new Regex(fileRegexPattern);
            }

            string currLocalDirectoryName = localDirectoryName;
            if (currLocalDirectoryName == null || currLocalDirectoryName.Length == 0)
                currLocalDirectoryName = Directory.GetCurrentDirectory();
            else if (!Directory.Exists(currLocalDirectoryName)) {
                // Create local directory if needed
                Directory.CreateDirectory(currLocalDirectoryName);
                CallTransferCallback(transferCallback, ETransferActions.LocalDirectoryCreated, currLocalDirectoryName, null, 0, null);
            }

            string currRemoteDirectoryName = remoteDirectoryName;
            if (currRemoteDirectoryName == null || currRemoteDirectoryName.Length == 0)
                currRemoteDirectoryName = GetCurrentDirectory();

            IList<DirectoryListItem> dirList = GetDirectoryList(currRemoteDirectoryName);

            // TODO: add SymLink dir recursion check
            CheckSymLinks(currRemoteDirectoryName, dirList);

            foreach (DirectoryListItem item in dirList)
                if (!item.IsDirectory && (regex == null || regex.IsMatch(item.Name))) {
                    // Transfer file
                    string localFilePath = GetUniquePath(paths, Path.Combine(currLocalDirectoryName, PathCheck.GetValidLocalFileName(item.Name)));
                    string remoteFilePath = CombineRemotePath(currRemoteDirectoryName, item.Name);
                    GetFile(remoteFilePath, localFilePath, transferCallback);
                }

            if (recursive)
                foreach (DirectoryListItem item in dirList)
                    if (item.IsDirectory) {
                        // Recursion
                        string localNextDirPath = GetUniquePath(paths, Path.Combine(currLocalDirectoryName, PathCheck.GetValidLocalFileName(item.Name)));
                        string remoteNextDirPath = CombineRemotePath(currRemoteDirectoryName, item.Name);
                        GetFiles(remoteNextDirPath, localNextDirPath, filePattern, patternStyle, recursive, transferCallback, paths);
                    }
        }

        private static string GetUniquePath(IList<string> paths, string localFilePath) {
            string checkPath = localFilePath;
            int i = 1;

            // TODO: check if FS is case sensitive (here we assume it's not)
            while (paths.Contains(checkPath.ToLower()))
                checkPath = localFilePath + "_" + i++;
            paths.Add(checkPath.ToLower());

            return checkPath;
        }

        /// <summary>
        /// Transfers all files, matching the given pattern, from the current remote directory. Optionally recursive
        /// </summary>
        /// <param name="localDirectoryName"></param>
        /// <param name="filePattern"></param>
        /// <param name="patternStyle"></param>
        /// <param name="recursive"></param>
        public void GetFiles(string localDirectoryName, string filePattern, EPatternStyle patternStyle, bool recursive) {
            GetFiles(null, localDirectoryName, filePattern, patternStyle, recursive, null);
        }

        /// <summary>
        /// Transfers all files from the current remote directory. Optionally recursive
        /// </summary>
        /// <param name="localDirectoryName"></param>
        /// <param name="recursive"></param>
        public void GetFiles(string localDirectoryName, bool recursive) {
            GetFiles(null, localDirectoryName, null, EPatternStyle.Verbatim, recursive, null);
        }

        /// <summary>
        /// Transfers all files from the current remote directory without recursion
        /// </summary>
        /// <param name="localDirectoryName"></param>
        public void GetFiles(string localDirectoryName) {
            GetFiles(null, localDirectoryName, null, EPatternStyle.Verbatim, false, null);
        }

        /// <summary>
        /// Stores a remote file, returning a <see cref="FtpStream"/> to be used for writing the file contents. Call <see cref="FtpStream.Close"/> on the stream once done.
        /// See the <see cref="PutFile(string, string)"/> overload to easily transfer the contents of a local file. 
        /// </summary>
        /// <param name="remoteFileName"></param>
        /// <returns></returns>
        public FtpStream PutFile(string remoteFileName) {
            SetupDataConnection();
            StorCmd(remoteFileName);
            return EndStreamCommand(FtpStream.EAllowedOperation.Write);
        }

        /// <summary>
        /// PutFile overload to easily transfer a file from local to remote
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="remoteFileName"></param>
        /// <returns>Transferred bytes count.</returns>
        public ulong PutFile(string localFileName, string remoteFileName) {
            return PutFile(localFileName, remoteFileName, null);
        }

        /// <summary>
        /// PutFile overload to easily transfer a file from local to remote
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="remoteFileName"></param>
        /// <param name="transferCallback"></param>
        /// <returns>Transferred bytes count.</returns>
        public ulong PutFile(string localFileName, string remoteFileName, FileTransferCallback transferCallback) {
            using (Stream s = PutFile(remoteFileName))
                return SendFile(localFileName, remoteFileName, s, transferCallback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localDirectoryName"></param>
        /// <param name="remoteDirectoryName"></param>
        /// <param name="filePattern"></param>
        /// <param name="patternStyle"></param>
        /// <param name="recursive"></param>
        /// <param name="transferCallback"></param>
        public void PutFiles(string localDirectoryName, string remoteDirectoryName,
            string filePattern, EPatternStyle patternStyle, bool recursive,
            FileTransferCallback transferCallback) {
            Regex regex = null;
            if (filePattern != null) {
                string fileRegexPattern = GetRegexPattern(filePattern, patternStyle);
                regex = new Regex(fileRegexPattern);
            }

            string previuosDirName = null;
            string currRemoteDirectoryName = remoteDirectoryName;
            if (currRemoteDirectoryName != null) {
                previuosDirName = GetCurrentDirectory();
                EnsureDir(currRemoteDirectoryName, transferCallback);
            } else
                currRemoteDirectoryName = GetCurrentDirectory();

            string currLocalDirectoryName = localDirectoryName;
            if (string.IsNullOrEmpty(currLocalDirectoryName))
                currLocalDirectoryName = Directory.GetCurrentDirectory();

            string[] dirItems = Directory.GetFiles(currLocalDirectoryName);

            foreach (string itemName in dirItems) {
                String fileName = Path.GetFileName(itemName);
                if (regex == null || regex.IsMatch(fileName)) {
                    string remoteFileName = CombineRemotePath(currRemoteDirectoryName, fileName);
                    PutFile(itemName, remoteFileName, transferCallback);
                }
            }

            if (recursive) {
                dirItems = Directory.GetDirectories(currLocalDirectoryName);
                foreach (string itemName in dirItems) {
                    string remoteNextDirName = CombineRemotePath(currRemoteDirectoryName, Path.GetFileName(itemName));
                    PutFiles(itemName, remoteNextDirName, filePattern, patternStyle, true, transferCallback);
                }
            }

            if (previuosDirName != null)
                SetCurrentDirectory(previuosDirName);
        }

        /// <summary>
        /// Transfers all files, matching the given pattern, to the current remote directory. Optionally recursive
        /// </summary>
        /// <param name="localDirectoryName"></param>
        /// <param name="filePattern"></param>
        /// <param name="patternStyle"></param>
        /// <param name="recursive"></param>
        public void PutFiles(string localDirectoryName, string filePattern, EPatternStyle patternStyle, bool recursive) {
            PutFiles(localDirectoryName, null, filePattern, patternStyle, recursive, null);
        }

        /// <summary>
        /// Transfers all files to the current remote directory. Optionally recursive
        /// </summary>
        /// <param name="localDirectoryName"></param>
        /// <param name="recursive"></param>
        public void PutFiles(string localDirectoryName, bool recursive) {
            PutFiles(localDirectoryName, null, null, EPatternStyle.Verbatim, recursive, null);
        }

        /// <summary>
        /// Transfers all files to the current remote directory without recursion
        /// </summary>
        /// <param name="localDirectoryName"></param>
        public void PutFiles(string localDirectoryName) {
            PutFiles(localDirectoryName, null, null, EPatternStyle.Verbatim, false, null);
        }

        public FtpStream AppendFile(string remoteFileName) {
            SetupDataConnection();
            AppeCmd(remoteFileName);
            return EndStreamCommand(FtpStream.EAllowedOperation.Write);
        }

        /// <summary>
        /// AppendFile overload to easily transfer a file from local to remote
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="remoteFileName"></param>
        public ulong AppendFile(string localFileName, string remoteFileName) {
            return AppendFile(localFileName, remoteFileName, null);
        }

        /// <summary>
        /// AppendFile overload to easily transfer a file from local to remote
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="remoteFileName"></param>
        /// <param name="transferCallback"></param>
        public ulong AppendFile(string localFileName, string remoteFileName, FileTransferCallback transferCallback) {
            using (Stream s = AppendFile(remoteFileName))
                return SendFile(localFileName, remoteFileName, s, transferCallback);
        }

        public FtpStream PutUniqueFile(out string remoteFileName) {
            SetupDataConnection();
            StouCmd(out remoteFileName);
            return EndStreamCommand(FtpStream.EAllowedOperation.Write);
        }

        /// <summary>
        /// PutUniqueFile overload to easily transfer a file from local to remote
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="remoteFileName"></param>
        public ulong PutUniqueFile(string localFileName, out string remoteFileName) {
            return PutUniqueFile(localFileName, out remoteFileName, null);
        }

        /// <summary>
        /// PutUniqueFile overload to easily transfer a file from local to remote.
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="remoteFileName"></param>
        /// <param name="transferCallback"></param>
        public ulong PutUniqueFile(string localFileName, out string remoteFileName, FileTransferCallback transferCallback) {
            using (Stream s = PutUniqueFile(out remoteFileName))
                return SendFile(localFileName, remoteFileName, s, transferCallback);
        }

        /// <summary>
        /// Deletes the given remote file.
        /// </summary>
        public void DeleteFile(string remoteFileName) {
            DeleCmd(remoteFileName);
        }

        /// <summary>
        /// Renames the given remote file.
        /// </summary>
        public void RenameFile(string remoteFileNameFrom, string remoteFileNameTo) {
            RnfrCmd(remoteFileNameFrom);
            RntoCmd(remoteFileNameTo);
        }

        /// <summary>
        /// Creates the given remote directory.
        /// </summary>
        public void MakeDir(string remoteDirName, bool recursive = false) {
            if (!recursive)
                MkdCmd(remoteDirName);
            else {
                remoteDirName = remoteDirName.TrimEnd('/');
                var endPos = remoteDirName.Length;
                string curDir;
                do {
                    curDir = remoteDirName.Substring(0, endPos);
                    try {
                        CwdCmd(curDir);
                        break;
                    } catch (FtpCommandException) {
                        // ignored, it means the folder doesn't exist
                    }
                    endPos = curDir.LastIndexOf("/", StringComparison.CurrentCultureIgnoreCase);
                } while (endPos > -1);
                if (!curDir.Equals(remoteDirName)) {
                    do {
                        endPos = remoteDirName.IndexOf("/", endPos + 1, StringComparison.CurrentCultureIgnoreCase);
                        if (endPos > -1) {
                            curDir = remoteDirName.Substring(0, endPos);
                            MkdCmd(curDir);
                        }
                    } while (endPos > -1);
                    MkdCmd(remoteDirName);
                }
            }
        }

        /// <summary>
        /// Removes the given remote directory.
        /// </summary>
        /// <param name="remoteDirName"></param>
        public void RemoveDir(string remoteDirName) {
            RmdCmd(remoteDirName);
        }

        /// <summary>
        /// Changes the remote directory to the parent directory.
        /// </summary>
        public void ChangeToUpperDir() {
            CdupCmd();
        }

        public IList<string> GetShortDirectoryList() {
            return GetShortDirectoryList(null);
        }

        /// <summary>
        /// Returns an array of file names and directories contained in the given directory. 
        /// Please use <see cref="GetShortDirectoryList(string)"/> or <see cref="GetDirectoryListUnparsed(string)"/> for more detailed directory information.
        /// </summary>
        /// <param name="remoteDirName"></param>
        /// <returns></returns>
        public IList<string> GetShortDirectoryList(string remoteDirName) {
            SetupDataConnection();
            NlstCmd(remoteDirName);
            string listData = GetDataString();
            GetReply();

            return new List<string>(listData.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries));
        }

        public IList<DirectoryListItem> GetDirectoryList() {
            return GetDirectoryList(null);
        }

        /// <summary>
        /// Returns a list of the contents form the given directory. A parsing is performed on the data returned fronm the LIST command.
        /// </summary>
        /// <param name="remoteDirName"></param>
        /// <returns></returns>
        /// <remarks>
        /// Please use <see cref="GetShortDirectoryList(string)"/> or <see cref="GetDirectoryListUnparsed(string)"/> in case of parsing errors, 
        /// as the contents returned from FTP servers may differ from the common DOS and UNIX formats adopted here.
        /// </remarks>
        public IList<DirectoryListItem> GetDirectoryList(string remoteDirName) {
            string listData = GetDirectoryListUnparsed(remoteDirName);
            return DirectoryListParser.GetDirectoryList(listData);
        }

        public string GetDirectoryListUnparsed() {
            return GetDirectoryListUnparsed(null);
        }

        /// <summary>
        /// returns the given directory list data as returned from the server, without parsing its contents.
        /// </summary>
        /// <returns></returns>
        public string GetDirectoryListUnparsed(string remoteDirName) {
            SetupDataConnection();
            ListCmd(remoteDirName);
            string listData = GetDataString();
            GetReply();

            if (listData.Length == 0) {
                // On some systems (vsftpd) there is no error in case of non existent directories
                // Check the directory existence by explicitly CWD into it
                PushCurrentDirectory();
                SetCurrentDirectory(remoteDirName);
                PopCurrentDirectory();
            }

            return listData;
        }

        public string GetCurrentDirectory() {
            return PwdCmd();
        }

        /// <summary>
        /// Pushes the current remote directory on a stack, in order to easily restore it later calling <see cref="PopCurrentDirectory"/>.
        /// </summary>
        /// <returns>The current remote directory.</returns>
        public string PushCurrentDirectory() {
            string curDir = GetCurrentDirectory();
            _currDirStack.Push(curDir);
            return curDir;
        }

        /// <summary>
        /// Restores the current directory. For details please see <see cref="PushCurrentDirectory"/>.   
        /// Throws an exception if the stack is empty.
        /// </summary>
        public string PopCurrentDirectory() {
            string dir = _currDirStack.Pop();
            SetCurrentDirectory(dir);
            return dir;
        }

        /// <summary>
        /// Changes the remote current directory.
        /// </summary>
        /// <param name="remoteDirName"></param>
        public void SetCurrentDirectory(string remoteDirName) {
            CwdCmd(remoteDirName);
        }

        /// <summary>
        /// Returns some remote system information, as returned from the SYST command.
        /// </summary>
        /// <returns></returns>
        public string GetSystem() {
            return SystCmd();
        }

        /// <summary>
        /// Returns the modification time of the given remote file or <c>null</c> if the MDTM feature is not supported by the server.
        /// </summary>
        /// <param name="remoteFileName"></param>
        /// <returns></returns>
        public DateTime? GetFileModificationTime(string remoteFileName) {
            // RFC 3659 3.3. MDTM must be included in the FEAT reply
            return CheckFeature("MDTM") ? (DateTime?) MdtmCmd(remoteFileName) : null;
        }

        /// <summary>
        /// Set the language used by the server during the current connection. Please check the features returned by GetFeatues for a list of 
        /// available languages supported by the server.
        /// </summary>
        /// <param name="ietfLanguageTag">RFC 1766 language tag.</param>
        public void SetLanguage(string ietfLanguageTag) {
            LangCmd(ietfLanguageTag);
        }

        /// <summary>
        /// Set the given text encoding.
        /// </summary>
        /// <param name="textEncoding"></param>
        public void SetTextEncoding(ETextEncoding textEncoding) {
            OptsCmd("UTF8 " + (textEncoding == ETextEncoding.Utf8 ? "ON" : "OFF"));
            _textEncoding = textEncoding;
        }

        /// <summary>
        /// Sends the given FTP command text to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns the parsed server reply.</returns>
        /// <remarks>In case of return codes >= 400 an exception is thrown.</remarks>
        public FtpReply SendCustomCommand(string command) {
            return HandleCmd(command);
        }

        /// <summary>
        /// Closes the current connection, freeing resources. 
        /// </summary>
        public void Close() {
            StopKeepAlive();

            CloseDataConnection();
            CloseCtrlConnection();

            _sslServerCert = null;
            _sslClientCert = null;
        }

        #region IDisposable Members

        public void Dispose() {
            Close();
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Copies the protocol information form the given stream.
        /// </summary>
        /// <param name="sslStream"></param>
        private void SetSslInfo(SslStream sslStream) {
            _sslInfo = new SslInfo {
                SslProtocol = sslStream.SslProtocol,
                CipherAlgorithm = sslStream.CipherAlgorithm,
                CipherStrength = sslStream.CipherStrength,
                HashAlgorithm = sslStream.HashAlgorithm,
                HashStrength = sslStream.HashStrength,
                KeyExchangeAlgorithm = sslStream.KeyExchangeAlgorithm,
                KeyExchangeStrength = sslStream.KeyExchangeStrength
            };
        }

        /// <summary>
        /// Workaround needed because it is not possible to determine if a symlink is a directory or a file based on the UNIX style directory listing.
        /// </summary>
        /// <param name="remoteDirectoryName"></param>
        /// <param name="dirList"></param>
        private void CheckSymLinks(string remoteDirectoryName, IList<DirectoryListItem> dirList) {
            string currDir = null;

            foreach (DirectoryListItem item in dirList)
                if (item.IsSymLink)
                    try {
                        if (currDir == null)
                            currDir = GetCurrentDirectory();

                        // If we cannot set this item as the current directory then it is a file
                        String itemPath = CombineRemotePath(remoteDirectoryName, item.Name);
                        SetCurrentDirectory(itemPath);
                        item.IsDirectory = true;

                        if (item.SymLinkTargetPath == null)
                            item.SymLinkTargetPath = GetCurrentDirectory();
                    } catch (FtpCommandException ex) {
                        // Ok, it's (probably) a file
                        if (ex.ErrorCode == 550)
                            item.IsDirectory = false;
                        else
                            throw;
                    }

            if (currDir != null)
                SetCurrentDirectory(currDir);
        }

        private void SSlCtrlChannelCheckRevertToClearText() {
            // Back to clear text mode, but only if supported by the server
            if (CheckFeature("CCC"))
                CccCmd();
            else
                _sslSupportCurrentMode |= EsslSupportMode.ControlChannelRequested;
        }

        private bool CheckFeature(string feature) {
            return _features != null && _features.Contains(feature);
        }

        private void GetFeaturesFromServer() {
            try {
                _features = FeatCmd();
            } catch (FtpCommandException) {
                // FEAT is not supported by the server
                _features = null;
            }
        }

        private void SslDataChannelCheckExplicitEncryptionRequest() {
            if ((_sslSupportCurrentMode & EsslSupportMode.DataChannelRequested) == EsslSupportMode.DataChannelRequested) {
                PbszCmd(0);

                try {
                    ProtCmd(EProtCode.P);
                } catch (FtpCommandException ex) {
                    // Note: MS FTP 7.0 returns 536, but RFC 2228 requires 534
                    if ((_sslSupportCurrentMode & EsslSupportMode.DataChannelRequired) == EsslSupportMode.DataChannelRequired)
                        if (ex.ErrorCode == 534 || ex.ErrorCode == 536)
                            throw new FtpSslException("The server policy denies SSL/TLS", ex);
                        else
                            throw;

                    _sslSupportCurrentMode ^= EsslSupportMode.DataChannelRequired;

                    // Data channel transfers will be done in clear text
                    ProtCmd(EProtCode.C);
                }
            }
        }

        private void SslControlChannelCheckExplicitEncryptionRequest(EsslSupportMode sslSupportMode) {
            if ((sslSupportMode & EsslSupportMode.CredentialsRequested) == EsslSupportMode.CredentialsRequested)
                try {
                    AuthCmd(EAuthMechanism.Tls);
                } catch (FtpCommandException ex) {
                    if ((sslSupportMode & EsslSupportMode.CredentialsRequired) == EsslSupportMode.CredentialsRequired)
                        if (ex.ErrorCode == 530 || ex.ErrorCode == 534)
                            throw new FtpSslException("SSL/TLS connection not supported on server", ex);
                        else
                            throw;

                    _sslSupportCurrentMode = EsslSupportMode.ClearText;
                }
        }

        private static string GetRegexPattern(string filePattern, EPatternStyle patternStyle) {
            string fileRegexPattern = filePattern;

            switch (patternStyle) {
                case EPatternStyle.Wildcard:
                case EPatternStyle.Verbatim:
                    fileRegexPattern = "^" + Regex.Escape(filePattern) + "$";
                    if (patternStyle == EPatternStyle.Wildcard)
                        fileRegexPattern = fileRegexPattern.Replace(@"\*", ".*").Replace(@"\?", ".{1}");
                    break;
            }
            return fileRegexPattern;
        }

        private void CallTransferCallback(FileTransferCallback transferCallback, ETransferActions transferAction,
            string localObjectName, string remoteObjectName,
            ulong fileTransmittedBytes, ulong? fileTransferSize) {
            if (transferCallback != null) {
                bool cancel = false;
                transferCallback(this, transferAction, localObjectName, remoteObjectName, fileTransmittedBytes, fileTransferSize, ref cancel);
                if (cancel)
                    throw new FtpOperationCancelledException("Operation cancelled by the user");
            }
        }

        private ulong SendFile(string localFileName, string remoteFileName, Stream s, FileTransferCallback transferCallback) {
            ulong totalBytes = 0;

            ulong? fileTransferSize = null;
            if (transferCallback != null)
                fileTransferSize = (ulong) (new FileInfo(localFileName).Length);

            using (FileStream fs = File.OpenRead(localFileName)) {
                byte[] buf = new byte[1024];
                int n;
                do {
                    CallTransferCallback(transferCallback, ETransferActions.FileUploadingStatus, localFileName, remoteFileName, totalBytes, fileTransferSize);

                    n = fs.Read(buf, 0, buf.Length);
                    if (n > 0) {
                        s.Write(buf, 0, n);
                        totalBytes += (ulong) n;
                    }
                } while (n > 0);

                fs.Close();
            }
            s.Close();

            CallTransferCallback(transferCallback, ETransferActions.FileUploaded, localFileName, remoteFileName, totalBytes, fileTransferSize);

            return totalBytes;
        }

        /// <summary>
        /// Check if the given directory exists and create it if it doesn't.
        /// </summary>
        /// <param name="remoteDirectoryName"></param>
        /// <param name="transferCallback"></param>        
        private void EnsureDir(string remoteDirectoryName, FileTransferCallback transferCallback) {
            try {
                string currDir = GetCurrentDirectory();
                SetCurrentDirectory(remoteDirectoryName);
                // Ok, the directory exists, set the previous current directory
                SetCurrentDirectory(currDir);
            } catch (FtpCommandException ex) {
                if (ex.ErrorCode == 550) {
                    MakeDir(remoteDirectoryName);
                    CallTransferCallback(transferCallback, ETransferActions.RemoteDirectoryCreated, null, remoteDirectoryName, 0, null);
                } else
                    throw;
            }
        }

        private FtpStream EndStreamCommand(FtpStream.EAllowedOperation allowedOp) {
            return new FtpStream(GetDataStream(), allowedOp,
                delegate {
                    CloseDataConnection();
                    if (_waitingCompletionReply)
                        GetReply();
                });
        }

        private Stream GetDataStream() {
            Stream s;

            if (DataConnectionMode == EDataConnectionMode.Active)
                SetupActiveDataConnectionStep2();

            if ((_sslSupportCurrentMode & EsslSupportMode.DataChannelRequested) == EsslSupportMode.DataChannelRequested) {
                if (_dataSslStream == null)
                    _dataSslStream = CreateSSlStream(_dataClient.GetStream(), false);
                s = _dataSslStream;
            } else
                s = _dataClient.GetStream();

            return s;
        }

        private string GetDataString() {
            try {
                Stream s = GetDataStream();

                StringBuilder data = new StringBuilder();

                byte[] buf = new byte[1024];
                int n;
                do {
                    n = s.Read(buf, 0, buf.Length);
                    data.Append(Encoding.UTF8.GetString(buf, 0, n));
                } while (n != 0);

                return data.ToString();
            } finally {
                CloseDataConnection();
            }
        }

        private void SetupCtrlConnection(string hostname, int port, Encoding textEncoding) {
            CloseCtrlConnection();

            _ctrlClient = new TcpClient(hostname, port);

            Stream s = _ctrlClient.GetStream();
            s.ReadTimeout = _timeout;
            s.WriteTimeout = _timeout;

            SetupCtrlStreamReaderAndWriter(s);
        }

        private void SetupCtrlStreamReaderAndWriter(Stream s) {
            if (_ctrlSw != null)
                _ctrlSw.Flush();

            // SreamWriter's doc states that the default encoding is UTF8 without BOM.
            // Mono 2.0 seems to have a BOM anyway. Create it explicitly as a workaround
            Encoding encoding = new UTF8Encoding(false);

            _ctrlSr = new StreamReader(s, encoding);
            _ctrlSw = new StreamWriter(s, encoding);
            _ctrlSw.NewLine = "\r\n";
        }

        private int SetupActiveDataConnectionStep1() {
            CloseDataConnection();

            IPEndPoint localAddr = new IPEndPoint(IPAddress.Any, 0);
            _activeDataConnListener = new TcpListener(localAddr);
            _activeDataConnListener.Start();

            return (_activeDataConnListener.LocalEndpoint as IPEndPoint).Port;
        }

        private void SetupActiveDataConnectionStep2() {
            try {
                _dataClient = _activeDataConnListener.AcceptTcpClient();
            } finally {
                StopActiveDataConnListener();
            }
        }

        private void StopActiveDataConnListener() {
            _activeDataConnListener.Stop();
            _activeDataConnListener = null;
        }

        private void SetupPassiveDataConnection(IPEndPoint dataEndPoint) {
            CloseDataConnection();

            IPAddress addr;
            if (_useCtrlEndPointAddressForData)
                addr = (_ctrlClient.Client.RemoteEndPoint as IPEndPoint).Address;
            else
                addr = dataEndPoint.Address;

            // Enter passive mode                
            _dataClient = new TcpClient(addr.ToString(), dataEndPoint.Port);

            SetDataClientTimeout();
        }

        private void SetDataClientTimeout() {
            Stream s = _dataClient.GetStream();
            s.ReadTimeout = _timeout;
            s.WriteTimeout = _timeout;
        }

        private void SwitchCtrlToSslMode() {
            _ctrlSslStream = CreateSSlStream(_ctrlClient.GetStream(), true);

            SetupCtrlStreamReaderAndWriter(_ctrlSslStream);

            SetSslInfo(_ctrlSslStream);
        }

        private SslStream CreateSSlStream(Stream s, bool leaveInnerStreamOpen) {
            SslStream sslStream = new SslStream(s, leaveInnerStreamOpen,
                ValidateServerCertificate,
                null //new LocalCertificateSelectionCallback(ValidateClientCertificate)
            );

            sslStream.ReadTimeout = _timeout;
            sslStream.WriteTimeout = _timeout;

            X509CertificateCollection clientCertColl = new X509CertificateCollection();
            if (_sslClientCert != null)
                clientCertColl.Add(_sslClientCert);

            //sslStream.AuthenticateAsClient(hostname);
            sslStream.AuthenticateAsClient(_hostname, clientCertColl, SslProtocols.Default, _sslCheckCertRevocation);

            CheckSslAlgorithmsStrength(sslStream);

            return sslStream;
        }

        private void CheckSslAlgorithmsStrength(SslStream sslStream) {
            // Check algorithms length
            if (_sslMinKeyExchangeAlgStrength > 0 && sslStream.KeyExchangeStrength < _sslMinKeyExchangeAlgStrength)
                throw new FtpSslException("The SSL/TSL key exchange algorithm strength does not fulfill the requirements: " + sslStream.KeyExchangeStrength);

            if (_sslMinCipherAlgStrength > 0 && sslStream.CipherStrength < _sslMinCipherAlgStrength)
                throw new FtpSslException("The SSL/TSL cipher algorithm strength does not fulfill the requirements: " + sslStream.CipherStrength);

            if (_sslMinHashAlgStrength > 0 && sslStream.HashStrength < _sslMinHashAlgStrength)
                throw new FtpSslException("The SSL/TSL hash algorithm strength does not fulfill the requirements: " + sslStream.HashStrength);
        }

        private void SwitchCtrlToClearMode() {
            _ctrlSslStream.Close();
            _ctrlSslStream = null;

            SetupCtrlStreamReaderAndWriter(_ctrlClient.GetStream());
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            bool certOk = true;

            // Validate only the first time or if the certificate changes
            if (_sslServerCert == null || !_sslServerCert.Equals(certificate)) {
                if (_userValidateServerCertificate != null)
                    certOk = _userValidateServerCertificate(this, certificate, chain, sslPolicyErrors);
                else if (sslPolicyErrors != SslPolicyErrors.None)
                    certOk = false;

                if (certOk)
                    _sslServerCert = new X509Certificate(certificate.Export(X509ContentType.Cert));
            }

            return certOk;
        }

        private static string ParsePwdReply(FtpReply reply) {
            int i = reply.Message.IndexOf('\"');
            if (i < 0)
                throw new FtpProtocolException(reply);

            int j = reply.Message.IndexOf('\"', i + 1);
            if (j < 0)
                throw new FtpProtocolException(reply);

            string dirName = reply.Message.Substring(i + 1, j - i - 1);
            return dirName;
        }

        private static IPEndPoint ParsePasvReply(FtpReply reply) {
            IPEndPoint dataEndPoint;
            int i = reply.Message.IndexOf('(');
            if (i < 0)
                throw new FtpProtocolException(reply);

            int j = reply.Message.IndexOf(')', i + 1);
            if (j < 0)
                throw new FtpProtocolException(reply);

            string[] parts = reply.Message.Substring(i + 1, j - i - 1).Split(',');
            if (parts.Length != 6)
                throw new FtpProtocolException(reply);

            byte[] addr = new byte[4];
            for (i = 0; i < addr.Length; i++)
                addr[i] = byte.Parse(parts[i]);

            int port = byte.Parse(parts[4]) * 256 + byte.Parse(parts[5]);

            dataEndPoint = new IPEndPoint(new IPAddress(addr), port);
            return dataEndPoint;
        }

        private IPEndPoint ParseEpsvReply(FtpReply reply) {
            string[] parts = reply.Message.Split('|');
            if (parts.Length != 5)
                throw new FtpProtocolException(reply);

            int port = int.Parse(parts[3]);
            return new IPEndPoint(((IPEndPoint) _ctrlClient.Client.LocalEndPoint).Address, port);
        }

        private FtpReply HandleCmd(string command) {
            return HandleCmd(command, true);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private FtpReply HandleCmd(string command, bool waitForAnswer) {
            CheckConnection();
            CheckCommandInjection(command);

            // TODO: perfor proper synchronization
            //if (waitingCompletionReply)
            //    throw new FTPException("Cannot issue a new command while waiting for a previous one to complete");

            _ctrlSw.WriteLine(command);
            _ctrlSw.Flush();

            if (LogCommand != null)
                LogCommand(this, new LogCommandEventArgs(command));

            return waitForAnswer ? GetReply() : null;
        }

        private void CheckConnection() {
            if (_ctrlClient == null)
                throw new FtpException("Not connected");
        }

        /// <summary>
        /// Basic injection check
        /// </summary>
        /// <param name="command"></param>
        private static void CheckCommandInjection(string command) {
            if (command.Contains("\r\n"))
                throw new FtpException("Newlines not allowed in command text");
        }

        /// <summary>
        /// Works like Path.Combine(...), but without replacing the "/" separator with Path.DirectorySeparatorChar
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        private static string CombineRemotePath(string path1, string path2) {
            return (path1.EndsWith("/") ? path1 : path1 + "/") + path2;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private FtpReply GetReply() {
            try {
                FtpReply reply = new FtpReply();
                bool replyDone = false;

                do {
                    string replyLine = _ctrlSr.ReadLine();

                    Match m = Regex.Match(replyLine, @"^([0-9]{3})([\s\-])(.*)$");

                    if (m.Success) {
                        int code = int.Parse(m.Groups[1].Value);
                        string messageLine = m.Groups[3].Value;
                        replyDone = (m.Groups[2].Value == " ");

                        if (reply.Code == 0) {
                            reply.Code = code;
                            reply.Message = messageLine;
                        } else // Multiline message
                        {
                            if (reply.Code != code)
                                throw new FtpReplyParseException(replyLine);

                            reply.Message += "\r\n" + messageLine;
                        }
                    } else // Multiline message
                    {
                        if (reply.Code == 0)
                            throw new FtpReplyParseException(replyLine);

                        reply.Message += "\r\n" + replyLine.TrimStart();
                    }
                } while (!replyDone);

                _waitingCompletionReply = (reply.Code < 200);

                if (LogServerReply != null)
                    LogServerReply(this, new LogServerReplyEventArgs(reply));

                if (reply.Code >= 400)
                    throw new FtpCommandException(reply);

                return reply;
            } catch (Exception) {
                _waitingCompletionReply = false;
                throw;
            }
        }

        private void CloseCtrlConnection() {
            if (_ctrlClient != null) {
                // Be polite
                try {
                    QuitCmd(false);
                } catch (Exception) {}

                if (_ctrlSslStream != null) {
                    _ctrlSslStream.Close();
                    _ctrlSslStream = null;
                }

                _ctrlSr.Close();
                _ctrlSr = null;

                _ctrlSw.Close();
                _ctrlSw = null;

                _ctrlClient.Close();
                _ctrlClient = null;

                _waitingCompletionReply = false;
            }
        }

        private void CloseDataConnection() {
            if (_dataClient != null) {
                if (_dataSslStream != null) {
                    _dataSslStream.Close();
                    _dataSslStream = null;
                }

                _dataClient.Close();
                _dataClient = null;
            }

            if (_activeDataConnListener != null)
                StopActiveDataConnListener();
        }

        #region RFC 959

        private void StorCmd(string fileName) {
            HandleCmd("STOR " + fileName);
        }

        private void StouCmd(out string fileName) {
            FtpReply reply = HandleCmd("STOU");
            fileName = ParseStouReply(reply);
        }

        private static string ParseStouReply(FtpReply reply) {
            string fileName;
            int i = reply.Message.LastIndexOf(' ');
            if (i < 0)
                throw new FtpProtocolException(reply);
            fileName = reply.Message.Substring(i + 1, reply.Message.Length - i - 2);
            return fileName;
        }

        private void AppeCmd(string fileName) {
            HandleCmd("APPE " + fileName);
        }

        private void RetrCmd(string fileName) {
            HandleCmd("RETR " + fileName);
        }

        private void DeleCmd(string fileName) {
            HandleCmd("DELE " + fileName);
        }

        private void MkdCmd(string dirName) {
            HandleCmd("MKD " + dirName);
        }

        private void RmdCmd(string dirName) {
            HandleCmd("RMD " + dirName);
        }

        private void CdupCmd() {
            HandleCmd("CDUP");
        }

        private string SystCmd() {
            return HandleCmd("SYST").Message;
        }

        private void TypeCmd(ERepType repType, string param2) {
            HandleCmd("TYPE " + repType + (param2 != null ? (" " + param2) : ""));
        }

        private string PwdCmd() {
            FtpReply reply = HandleCmd("PWD");
            return ParsePwdReply(reply);
        }

        private void CwdCmd(string dirName) {
            HandleCmd("CWD " + dirName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns><c>true</c> if the PASS command is required, false <c>otherwise</c>.</returns>
        private bool UserCmd(string userName) {
            return HandleCmd("USER " + userName).Code != 232;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <returns>Returns the server \"welcome message\" in case of successful login.</returns>
        private string PassCmd(string password) {
            return HandleCmd("PASS " + password).Message;
        }

        private void PortCmd() {
            int port = SetupActiveDataConnectionStep1();

            byte[] addr = (_ctrlClient.Client.LocalEndPoint as IPEndPoint).Address.GetAddressBytes();
            string portStr = string.Format("{0},{1},{2},{3},{4},{5}", addr[0], addr[1], addr[2], addr[3], port / 256, port % 256);

            FtpReply reply = HandleCmd("PORT " + portStr);
        }

        private void PasvCmd() {
            FtpReply reply = HandleCmd("PASV");
            IPEndPoint dataEndPoint = ParsePasvReply(reply);

            // The caller has to close the data connection
            SetupPassiveDataConnection(dataEndPoint);
        }

        private AddressFamily GetCtrlConnAddressFamily() {
            return ((IPEndPoint) _ctrlClient.Client.LocalEndPoint).AddressFamily;
        }

        private void SetupDataConnection() {
            if (DataConnectionMode == EDataConnectionMode.Active)
                PortCmd();
            else
                switch (GetCtrlConnAddressFamily()) {
                    case AddressFamily.InterNetwork:
                        PasvCmd();
                        break;
                    default:
                        EpsvCmd();
                        break;
                }
        }

        private void ListCmd(string dirName) {
            HandleCmd("LIST" + (dirName != null ? (" " + dirName) : ""));
        }

        private void NlstCmd(string dirName) {
            HandleCmd("NLST" + (dirName != null ? (" " + dirName) : ""));
        }

        private void RnfrCmd(string fileOldName) {
            HandleCmd("RNFR " + fileOldName);
        }

        private void RntoCmd(string fileNewName) {
            HandleCmd("RNTO " + fileNewName);
        }

        private void QuitCmd(bool waitForAnswer) {
            HandleCmd("QUIT", waitForAnswer);
        }

        private void NoopCmd() {
            HandleCmd("NOOP");
        }

        #endregion

        #region RFC 2228

        private void AuthCmd(EAuthMechanism authMech) {
            HandleCmd("AUTH " + authMech);
            SwitchCtrlToSslMode();
        }

        private void CccCmd() {
            HandleCmd("CCC");
            SwitchCtrlToClearMode();
        }

        private void ProtCmd(EProtCode protCode) {
            HandleCmd("PROT " + protCode);
        }

        private void PbszCmd(uint maxSize) {
            HandleCmd("PBSZ " + maxSize);
        }

        #endregion

        #region RFC 2389

        private IList<string> FeatCmd() {
            FtpReply reply = HandleCmd("FEAT");
            IList<string> features = new List<string>(reply.Message.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries));
            features.RemoveAt(0);
            features.RemoveAt(features.Count - 1);

            return features;
        }

        private void OptsCmd(string command) {
            HandleCmd("OPTS " + command);
        }

        #endregion

        #region RFC 2428

        private void EpsvCmd() {
            FtpReply reply = HandleCmd("EPSV");
            IPEndPoint dataEndPoint = ParseEpsvReply(reply);

            // The caller has to close the data connection
            SetupPassiveDataConnection(dataEndPoint);
        }

        #endregion

        #region RFC 2640

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ietfLanguageTag">RFC 1766 language tag</param>
        private void LangCmd(string ietfLanguageTag) {
            HandleCmd("LANG" + (ietfLanguageTag != null ? (" " + ietfLanguageTag) : ""));
        }

        #endregion

        #region RFC 3659

        private DateTime MdtmCmd(string fileName) {
            FtpReply reply = HandleCmd("MDTM " + fileName);
            return ParseFtpDateTime(reply.Message);
        }

        private ulong SizeCmd(string fileName) {
            FtpReply reply = HandleCmd("SIZE " + fileName);
            return ulong.Parse(reply.Message);
        }

        private static DateTime ParseFtpDateTime(string message) {
            return DateTime.ParseExact(message, "yyyyMMddHHmmss.FFF", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AssumeUniversal);
        }

        #endregion

        #region Other FTP Commands

        private void ClntCmd(string name) {
            HandleCmd("CLNT " + name);
        }

        #endregion

        #endregion
    }
}