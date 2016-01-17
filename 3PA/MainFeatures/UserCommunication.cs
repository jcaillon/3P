#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Drawing;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using YamuiFramework.Forms;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.NppInterfaceForm;

namespace _3PA.MainFeatures {

    internal static class UserCommunication {

        private static EmptyForm _anchorForm;

        /// <summary>
        /// init an empty form, this gives us a Form to hook onto if we want to do stuff on the UI thread
        /// from a back groundthread, use : BeginInvoke()
        /// </summary>
        public static void Init() {
            _anchorForm = new EmptyForm() {
                Location = new Point(-10000, -10000),
                Visible = false
            };
        }

        public static void Close() {
            if (_anchorForm != null)
                _anchorForm.Close();
            YamuiNotifications.CloseEverything();
        }

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
        public static void Notify(string html, MessageImg imageType, string title, string subTitle,  Action<HtmlLinkClickedEventArgs> clickHandler,int duration = 0, int width = 450) {
            if (_anchorForm != null) {
                // get npp's screen
                _anchorForm.BeginInvoke((Action) delegate {
                    var toastNotification = new YamuiNotifications(
                        HtmlHandler.FormatMessage(html, imageType, title, subTitle)
                        , duration, width, Npp.GetNppScreen());
                    if (clickHandler != null)
                        toastNotification.LinkClicked += (sender, args) => clickHandler(args);
                    else
                        toastNotification.LinkClicked += Utils.OpenPathClickHandler;
                    toastNotification.Show();
                });
            }
        }

        public static void Notify(string html, MessageImg imageType, string title, string subTitle, int duration = 0, int width = 450) {
            Notify(html, imageType, title, subTitle, null, duration, width);
        }

        public static void Notify(string html, int duration = 10, int width = 450) {
            Notify(html, MessageImg.MsgDebug, "Debug message", "Should not appear in prod?", duration, width);
        }

        /// <summary>
        /// Displays a messagebox like window
        /// REMARK : DON'T WAIT FOR AN ANSWER IF YOU CALL IT FROM A THREAD!!!!!!!
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
        /// <returns>returns an integer (-1 if closed, or from 0 to x = buttons.count - 1)</returns>
        public static int Message(string html, MessageImg type, string title, string subTitle, List<string> buttons, bool waitResponse, EventHandler<HtmlLinkClickedEventArgs> clickHandler = null, bool dontWrapLines = false) {
            var clickedButton = -1;
            if (_anchorForm != null) {
                if (clickHandler == null) {
                    clickHandler = Utils.OpenPathClickHandler;
                }
                if (waitResponse) {
                    clickedButton = YamuiFormMessageBox.ShwDlg(Npp.HandleNpp, HtmlHandler.FormatMessage(html, type, title, subTitle), buttons, true, clickHandler, dontWrapLines);
                } else {
                    _anchorForm.BeginInvoke((Action) delegate {
                        clickedButton = YamuiFormMessageBox.ShwDlg(Npp.HandleNpp, HtmlHandler.FormatMessage(html, type, title, subTitle), buttons, false, clickHandler, dontWrapLines);
                    });
                }
            }
            return clickedButton;
        }

        /// <summary>
        /// Sends an comment to a given GITHUB issue url
        /// </summary>
        /// <param name="message"></param>
        /// <param name="url"></param>
        public static bool SendIssue(string message, string url) {
            try {
                // handle spam (50s min between 2 posts)
                if (Utils.IsSpamming("SendIssue", 50000))
                    return false;

                HttpWebRequest req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
                if (req == null)
                    return false;
                req.Proxy = Config.Instance.GetWebClientProxy();
                req.Method = "POST";
                req.ContentType = "application/json";
                req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                req.Headers.Add("Authorization", "Basic M3BVc2VyOnJhbmRvbXBhc3N3b3JkMTIz");
                StreamWriter writer = new StreamWriter(req.GetRequestStream());
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                writer.Write("{\"body\": " + serializer.Serialize(
                    "### " + Environment.UserName + " (" + Environment.MachineName + ") ###\r\n" +
                    "#### 3P version : " + AssemblyInfo.Version + ", Notepad++ version : " + Npp.GetNppVersion() + " ####\r\n" +
                    message
                    ) + "}");
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
                if (result != null) {
                    return true;
                }
            } catch (Exception ex) {
                ErrorHandler.Log(ex.ToString());
            }
            return false;
        }
    }
}
