#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ftp.cs) is part of 3P.
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
using System.Net;
using System.Text;
using _3PA.MainFeatures;

namespace _3PA.Lib.Ftp {

    /// <summary>
    /// Limitations : can do simple ftp, ftps over TLS explicit only
    /// </summary>
    class Ftp {

        /// <summary>
        /// host to which we want to connect (localhost)
        /// </summary>
        public string Host;

        /// <summary>
        /// User name if any
        /// </summary>
        public string User;

        /// <summary>
        /// User password if any
        /// </summary>
        public string Pass;

        /// <summary>
        /// Set to true if you want to use secure ftp transfer (ftps)
        /// </summary>
        public bool UseSssl;

        /// <summary>
        /// Stores the log of all the errors that occured for this object
        /// </summary>
        public StringBuilder ErrorLog = new StringBuilder();

        public StringBuilder Log = new StringBuilder();

        private int _bufferSize = 16 * 1024;

        /// <summary>
        /// The wrapper for any ftp command
        /// </summary>
        private bool IssueFtpCommand(string path, string method, Action<FtpWebRequest> commandRequest, Action<FtpWebResponse> commandResponse) {
            FtpWebResponse ftpResponse = null;
            var executionOk = true;
            try {
                var ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + Host  + "/" + path);

                ftpRequest.UseBinary = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Headers.Add("user-agent", Config.GetUserAgent);
                ftpRequest.Timeout = 2000;

                //_ftpRequest.Proxy = Config.Instance.GetWebClientProxy();

                // user
                if (!string.IsNullOrEmpty(User))
                    ftpRequest.Credentials = new NetworkCredential(User, Pass);

                // SSL
                ftpRequest.EnableSsl = UseSssl;

                // false if we use a different port?
                ftpRequest.UsePassive = true;
               
                // accept all the certificates...
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
                
                ftpRequest.Method = (method.Equals("ping") ? WebRequestMethods.Ftp.ListDirectory : method);

                if (commandRequest != null)
                    commandRequest(ftpRequest);

                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                if (commandResponse != null)
                    commandResponse(ftpResponse);

                Log.AppendLine(ftpResponse.StatusDescription);

            } catch (Exception e) {
                ErrorHandler.Log(e.ToString(), true);
                if (e is WebException) {
                    ErrorLog.AppendLine(method + ": " + e.Message);      
                } else
                    ErrorHandler.ShowErrors(e, "Ftp command");
                executionOk = false;
            }
            if (ftpResponse != null)
                ftpResponse.Close();
            return executionOk;
        }

        /// <summary>
        /// Test if the ftp client can connect, returns true if so
        /// It can test different combinations of login to try and connect
        /// </summary>
        public bool CanConnect {
            get {
                if (Ping)
                    return true;

                // Try to switch SSL status
                var logOfFirstError = ErrorLog.ToString();
                UseSssl = !UseSssl;
                if (Ping)
                    return true;

                // Try to turn of SSL and use an anonymous user?
                User = "";
                UseSssl = false;
                if (Ping)
                    return true;

                ErrorLog.Clear();
                ErrorLog.Append(logOfFirstError);

                return false;
            }
        }

        private bool Ping {
            get { return IssueFtpCommand("/", WebRequestMethods.Ftp.PrintWorkingDirectory, null, null); }
        }

        /// <summary>
        /// Get a simple/detailed list of files/folders in the given folder
        /// </summary>
        public List<string> DirectoryList(string directory, bool detailed = false) {
            var output = new List<string>();
            IssueFtpCommand(directory, detailed ? WebRequestMethods.Ftp.ListDirectoryDetails : WebRequestMethods.Ftp.ListDirectory, null, response => {
                var ftpStream = response.GetResponseStream();
                if (ftpStream != null) {
                    using (StreamReader ftpReader = new StreamReader(ftpStream)) {
                        while (ftpReader.Peek() >= 0) {
                            output.Add(ftpReader.ReadLine());
                        }
                    }
                    ftpStream.Close();
                }
            });
            return output;
        }

        /// <summary>
        /// Get the size of a file (doesn't work on all servers)
        /// </summary>
        public long GetFileSize(string fileName) {
            long output = 0;
            IssueFtpCommand(fileName, WebRequestMethods.Ftp.GetFileSize, null, response => {
                output = response.ContentLength;
            });
            return output;
        }

        /// <summary>
        /// Delete a distant file
        /// </summary>
        public bool Delete(string fileName) {
            return IssueFtpCommand(fileName, WebRequestMethods.Ftp.DeleteFile, null, null);
        }

        /// <summary>
        /// Creates a distant directory, also returns false if the dir exists... care
        /// </summary>
        public bool CreateDirectory(string directory) {
            return IssueFtpCommand(directory, WebRequestMethods.Ftp.MakeDirectory, null, null);
        }

        /// <summary>
        /// Download the distant file to a local one
        /// </summary>
        public bool Download(string remoteFile, string localFile) {
            return IssueFtpCommand(remoteFile, WebRequestMethods.Ftp.DownloadFile, null, response => {
                var ftpStream = response.GetResponseStream();
                if (ftpStream != null) {
                    FileStream localFileStream = new FileStream(localFile, FileMode.Create);
                    byte[] byteBuffer = new byte[_bufferSize];
                    int bytesRead = ftpStream.Read(byteBuffer, 0, _bufferSize);
                    while (bytesRead > 0) {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, _bufferSize);
                    }
                    localFileStream.Close();
                    ftpStream.Close();
                }
            });
        }

        /// <summary>
        /// Upload a file to the server
        /// </summary>
        public bool Upload(string remoteFile, string localFile) {
            if (!File.Exists(localFile))
                return false;
            return IssueFtpCommand(remoteFile, WebRequestMethods.Ftp.UploadFile,
                request => {
                    FileInfo objFile = new FileInfo(localFile);
                    request.ContentLength = objFile.Length;
                    byte[] objBuffer = new byte[_bufferSize];
                    FileStream objFileStream = objFile.OpenRead();
                    using (Stream objStream = request.GetRequestStream()) {
                        int len;
                        while ((len = objFileStream.Read(objBuffer, 0, _bufferSize)) != 0) {
                            objStream.Write(objBuffer, 0, len);
                        }
                    }
                    objFileStream.Close();
                }, null);
        }

    } 
}
