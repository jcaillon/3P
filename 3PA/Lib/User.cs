#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (User.cs) is part of 3P.
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
using System.Globalization;
using System.IO;
using System.Net;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    public static class User {

        #region Ping

        /// <summary>
        /// This method pings a webservice deployed for 3P, it simply allows to do
        /// statistics on the number of users of the software
        /// </summary>
        public static void Ping() {
            try {
                DateTime lastPing;
                if (!DateTime.TryParseExact(Config.Instance.TechnicalLastPing, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastPing)) {
                    lastPing = DateTime.MinValue;
                }
                // ping once every hour
                if (DateTime.Now.Subtract(lastPing).TotalMinutes > 59) {
                    var webServiceJson = new WebServiceJson(WebServiceJson.WebRequestMethod.Post, Config.PingPostWebWervice);
                    webServiceJson.AddToReq("UUID", UniqueId);
                    webServiceJson.AddToReq("userName", Name);
                    webServiceJson.AddToReq("version", AssemblyInfo.Version);
                    webServiceJson.OnRequestEnded += req => {
                        if (req.StatusCodeResponse == HttpStatusCode.OK) {
                            Config.Instance.TechnicalLastPing = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    };
                    webServiceJson.Execute();
                }
            } catch (Exception e) {
                if (Config.IsDevelopper)
                    ErrorHandler.ShowErrors(e);
            }
        }
        
        #endregion

        #region User info

        /// <summary>
        /// Returns a formatted name for the user
        /// </summary>
        public static string Name {
            get {
                return Environment.UserName +
                   (!string.IsNullOrEmpty(Config.Instance.UserName) ? " aka " + Config.Instance.UserName : string.Empty) +
                   " (" + Environment.MachineName + ")";
            }
            
        }
        
        /// <summary>
        /// Returns a unique identifier for the current user, this ID is exactly 36 char long
        /// </summary>
        public static string UniqueId {
            get {
                try {
                    string checkIdPath = Path.Combine(Path.GetTempPath(), "x" + Environment.Version + ".tmp");
                    if (File.Exists(checkIdPath)) {
                        var content = File.ReadAllText(checkIdPath);
                        if (content.Length == 36 && !Config.Instance.MyUuid.Equals(content))
                            Config.Instance.MyUuid = content;
                    } else
                        Utils.FileWriteAllText(checkIdPath, Config.Instance.MyUuid);
                } catch (Exception e) {
                    if (Config.IsDevelopper)
                        ErrorHandler.ShowErrors(e);
                }
                return Config.Instance.MyUuid;
            }
        }

        #endregion

        #region SendIssue

        /// <summary>
        /// Sends an comment to a given GITHUB issue url
        /// </summary>
        /// <param name="message"></param>
        /// <param name="url"></param>
        public static bool SendComment(string message, string url) {

            // https://api.github.com/repos/jcaillon/3p/issues/1/comments

            // handle spam (10s min between 2 posts)
            if (Utils.IsSpamming("SendComment", 10000))
                return false;

            var wb = new WebServiceJson(WebServiceJson.WebRequestMethod.Post, url);

            // Convert.ToBase64String(Encoding.ASCII.GetBytes("user:mdp"));
            wb.OnInitHttpWebRequest += request => request.Headers.Add("Authorization", "Basic " + Config._3PUserCredentials);
            wb.AddToReq("body", "### " + Environment.UserName + " (" + Environment.MachineName + ") ###\r\n" +
                "#### 3P version : " + AssemblyInfo.Version + ", Notepad++ version : " + Npp.GetNppVersion + " ####\r\n" +
                message
                );
            wb.Execute();

            return false;
        }

        #endregion
    }

}
