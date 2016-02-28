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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Web.Script.Serialization;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    public static class User {

        /// <summary>
        /// This method pings a webservice deployed for 3P, it simply allows to do
        /// statistics on the number of users of the software. A 'unique' id made of the 
        /// mac address + machine name allows to count a user only once
        /// </summary>
        public static void Ping() {
            try {
                DateTime lastPing;
                if (!DateTime.TryParseExact(Config.Instance.TechnicalLastPing, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out lastPing)) {
                    lastPing = DateTime.MinValue;
                }
                // ping once every hour
                if (DateTime.Now.Subtract(lastPing).TotalMinutes > 58) {
                    Config.Instance.TechnicalLastPing = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    HttpWebRequest req = WebRequest.Create(new Uri(Config.PingWebWervice)) as HttpWebRequest;
                    if (req != null) {
                        req.Proxy = Config.Instance.GetWebClientProxy();
                        req.Method = "POST";
                        req.ContentType = "application/json";
                        req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                        req.Headers.Add("Authorization", "Basic M3BVc2VyOnJhbmRvbXBhc3N3b3JkMTIz");
                        StreamWriter writer = new StreamWriter(req.GetRequestStream());
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        writer.Write("{" +
                                     "\"computerId\": " + serializer.Serialize(Environment.MachineName + GetMacAddress()) + "," +
                                     "\"userName\": " + serializer.Serialize(Environment.UserName) + "," +
                                     "\"3pVersion\": " + serializer.Serialize(AssemblyInfo.Version) + "," +
                                     "\"NppVersion\": " + serializer.Serialize(Npp.GetNppVersion()) + "," +
                                     "\"timeZone\": " + serializer.Serialize(TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now) ? TimeZone.CurrentTimeZone.DaylightName : TimeZone.CurrentTimeZone.StandardName) +
                                     "}");
                        writer.Close();
                        string result = null;
                        using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse) {
                            if (resp != null && resp.GetResponseStream() != null) {
                                var respStream = resp.GetResponseStream();
                                if (respStream != null) {
                                    StreamReader reader = new StreamReader(respStream);
                                    result = reader.ReadToEnd();
                                    reader.Close();
                                }
                            }
                        }
                    }
                }
            } catch (Exception) {
                // we don't care if it goes wrong
            }
        }

        /// <summary>
        /// Returns the mac address of the computer
        /// </summary>
        public static string GetMacAddress() {
            string mac = string.Empty;
            try {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled=true");
                IEnumerable<ManagementObject> objects = searcher.Get().Cast<ManagementObject>();
                mac = (from o in objects orderby o["IPConnectionMetric"] select o["MACAddress"].ToString()).FirstOrDefault();
            } catch (Exception e) {
                if (!(e is ArgumentNullException)) {
                    ErrorHandler.DirtyLog(e);
                }
            }
            return mac;
        }

    }

}
