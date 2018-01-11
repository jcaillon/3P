#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (FtpPackager.cs) is part of 3P.
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
using System.Linq;
using System.Net;
using WixToolset.Dtf.Compression;
using _3PA.MainFeatures.Pro;
using _3PA.MainFeatures.Pro.Deploy;

namespace _3PA.Lib.Ftp {

    internal class FtpPackager : IPackager {

        #region Private

        private string _uri;
        private string _host;
        private int _port;
        private string _userName;
        private string _passWord;

        #endregion

        #region Life and death

        public FtpPackager(string host, int port, string userName, string passWord, string uri) {
            _host = host;
            _port = port;
            _userName = userName;
            _passWord = passWord;
            _uri = uri;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Send files to a FTP server
        /// </summary>
        public void PackFileSet(IDictionary<string, FileToDeployInPack> files, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler) {
            var ftp = FtpsClient.Instance.Get(_uri);

            // try to connect!
            if (!ftp.Connected) {
                ConnectFtp(ftp, _userName, _passWord, _host, _port);
            }

            foreach (var file in files.Values) {
                try {
                    ftp.PutFile(file.From, file.RelativePathInPack);
                } catch (Exception) {
                    // might be disconnected??
                    try {
                        ftp.GetCurrentDirectory();
                    } catch (Exception) {
                        ConnectFtp(ftp, _userName, _passWord, _host, _port);
                    }

                    // try to create the directory and then push the file again
                    ftp.MakeDir((Path.GetDirectoryName(file.RelativePathInPack) ?? "").Replace('\\', '/'), true);
                    ftp.SetCurrentDirectory("/");
                    ftp.PutFile(file.From, file.RelativePathInPack);
                }
                if (progressHandler != null) {
                    progressHandler(this, new ArchiveProgressEventArgs(ArchiveProgressType.FinishFile, file.RelativePathInPack, null));
                }
            }
        }

        /// <summary>
        /// Connects to a FTP server trying every methods
        /// </summary>
        private void ConnectFtp(FtpsClient ftp, string userName, string passWord, string server, int port) {
            NetworkCredential credential = null;
            if (!string.IsNullOrEmpty(userName))
                credential = new NetworkCredential(userName, passWord);

            var modes = new List<EsslSupportMode>();
            typeof(EsslSupportMode).ForEach<EsslSupportMode>((s, l) => { modes.Add(l); });

            ftp.DataConnectionMode = EDataConnectionMode.Passive;
            while (!ftp.Connected && ftp.DataConnectionMode == EDataConnectionMode.Passive) {
                foreach (var mode in modes.OrderByDescending(mode => mode)) {
                    try {
                        var curPort = port > -1 ? port : ((mode & EsslSupportMode.Implicit) == EsslSupportMode.Implicit ? 990 : 21);
                        ftp.Connect(server, curPort, credential, mode, 1800);
                        ftp.Connected = true;
                        break;
                    } catch (Exception) {
                        //ignored
                    }
                }
                ftp.DataConnectionMode = EDataConnectionMode.Active;
            }

            // failed?
            if (!ftp.Connected) {
                throw new Exception("Failed to connect to a FTP server with : " + string.Format(@"Username : {0}, Password : {1}, Host : {2}, Port : {3}", userName ?? "none", passWord ?? "none", server, port == -1 ? 21 : port));
            }
            
        }

        #endregion
    }
}
