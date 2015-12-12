#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (UserCommunication.cs) is part of 3P.
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
using System.Web.Script.Serialization;
using YamuiFramework.Forms;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using _3PA.Html;

namespace _3PA.MainFeatures {
    public class UserCommunication {

        /// <summary>
        /// Displays a notification on the bottom right of the screen
        /// </summary>
        /// <param name="html"></param>
        /// <param name="clickHandler"></param>
        /// <param name="subTitle"></param>
        /// <param name="duration"></param>
        /// <param name="width"></param>
        /// <param name="imageType"></param>
        /// <param name="title"></param>
        public static void Notify(string html, MessageImg imageType, string title, Action<HtmlLinkClickedEventArgs> clickHandler, string subTitle = "", int duration = 0, int width = 450) {
            if (Appli.Appli.Form != null)
                Appli.Appli.Form.BeginInvoke((Action)delegate {
                    var toastNotification = new YamuiNotifications(
                        LocalHtmlHandler.FormatMessage(html, imageType, title, subTitle)
                        , duration, width);
                    if (clickHandler != null)
                        toastNotification.LinkClicked += (sender, args) => clickHandler(args);
                    toastNotification.Show();
                });
        }

        public static void Notify(string html, MessageImg imageType, string title, string subTitle = "", int duration = 0, int width = 450) {
            Notify(html, imageType, title, null, subTitle, duration, width);
        }

        public static void Notify(string html, int duration = 0, int width = 450) {
            Notify(html, MessageImg.MsgLogo, "debug", "?", duration, width);
        }

        /// <summary>
        /// 
        /// new List string  { "fu", "ok" }
        /// </summary>
        /// <param name="html"></param>
        /// <param name="type"></param>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="buttons"></param>
        /// <param name="waitResponse"></param>
        /// <param name="clickHandler"></param>
        /// <param name="dontWrapLines"></param>
        public static void Message(string html, MessageImg type, string title, string subTitle, List<string> buttons, bool waitResponse, EventHandler<HtmlLinkClickedEventArgs> clickHandler, bool dontWrapLines) {
            if (Appli.Appli.Form != null)
                Appli.Appli.Form.BeginInvoke((Action)delegate {
                    YamuiFormMessageBox.ShwDlg(Npp.HandleNpp, LocalHtmlHandler.FormatMessage(html, type, title, subTitle), buttons, waitResponse, clickHandler, dontWrapLines);
                });
        }

        /// <summary>
        /// Sends an issue for debug purposes
        /// </summary>
        /// <param name="message"></param>
        /// <param name="url"></param>
        public static void SendIssue(string message, string url) {
            try {
                HttpWebRequest req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
                if (req == null)
                    return;
                req.Method = "POST";
                req.ContentType = "application/json";
                req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                req.Headers.Add("Authorization", "Basic M3BVc2VyOnJhbmRvbXBhc3N3b3JkMTIz");
                StreamWriter writer = new StreamWriter(req.GetRequestStream());
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                writer.Write("{\"body\": " + serializer.Serialize(
                    "### " + Environment.UserName + " (" + Environment.MachineName + ") ###\r\n" +
                    message
                    ) + "}");
                writer.Close();
                string result = null;
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse) {
                    if (resp != null && resp.GetResponseStream() != null) {
                        StreamReader reader = new StreamReader(resp.GetResponseStream());
                        result = reader.ReadToEnd();
                        reader.Close();
                    }
                }
                if (result != null) {
                    File.Delete(ErrorHandler.PathErrorToSend);
                }
            } catch (Exception ex) {
                ErrorHandler.Log(ex.ToString());
            }
        }
    }
}
