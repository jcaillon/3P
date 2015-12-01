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
using System.Windows.Forms;
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
        public static void Notify(string html, MessageImage imageType, string title, Action<HtmlLinkClickedEventArgs> clickHandler, string subTitle = "", int duration = 0, int width = 450) {
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

        public static void Notify(string html, MessageImage imageType, string title, string subTitle = "", int duration = 0, int width = 450) {
            Notify(html, imageType, title, null, subTitle, duration, width);
        }

        public static void Notify(string html, int duration = 0, int width = 450) {
            Notify(html, MessageImage.Logo, "debug", "?", duration, width);
        }

        public static void NotifyUserAboutNppDefaultAutoComp() {
            
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
        public static void Message(string html, MessageImage type, string title, string subTitle, List<string> buttons, bool waitResponse, EventHandler<HtmlLinkClickedEventArgs> clickHandler, bool dontWrapLines) {
            if (Appli.Appli.Form != null)
                Appli.Appli.Form.BeginInvoke((Action)delegate {
                    YamuiFormMessageBox.ShwDlg(Npp.HandleNpp, LocalHtmlHandler.FormatMessage(html, type, title, subTitle), buttons, waitResponse, clickHandler, dontWrapLines);
                });
        }
    }
}
