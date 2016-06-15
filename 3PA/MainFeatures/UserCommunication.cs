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
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Forms;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using _3PA.Html;
using _3PA.Lib;
using _3PA.MainFeatures.NppInterfaceForm;

// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures {

    internal static class UserCommunication {

        #region private fields

        private static EmptyForm _anchorForm;

        /// <summary>
        /// Allows to keep track of opened notifications (when its ID is set)
        /// </summary>
        private static Dictionary<string, YamuiNotifications> _registeredNotif = new Dictionary<string, YamuiNotifications>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region Management

        /// <summary>
        /// init an empty form, this gives us a Form to hook onto if we want to do stuff on the UI thread
        /// from a back groundthread, use : BeginInvoke()
        /// </summary>
        public static void Init() {
            _anchorForm = new EmptyForm {
                Location = new Point(-10000, -10000),
                Visible = false
            };
        }

        /// <summary>
        /// Get true if the notifications are ready to be used
        /// </summary>
        public static bool Ready {
            get { return _anchorForm != null && _anchorForm.IsHandleCreated; }
        }

        /// <summary>
        /// Close all the notifications
        /// </summary>
        public static void ForceClose() {
            if (_anchorForm != null)
                _anchorForm.Close();
            YamuiNotifications.CloseEverything();
        }

        #endregion

        #region Notify and NotifyUnique

        /// <summary>
        /// Closes the notification represented by the given id
        /// </summary>
        /// <param name="id"></param>
        public static void CloseUniqueNotif(string id) {
            if (_registeredNotif.ContainsKey(id)) {
                try {
                    if (_registeredNotif[id] != null) {
                        _registeredNotif[id].Close();
                    }
                } catch (Exception) {
                    // ignored
                }
                _registeredNotif.Remove(id);
            }
        }

        /// <summary>
        /// Displays a notification on the bottom right of the screen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="html"></param>
        /// <param name="clickHandler"></param>
        /// <param name="subTitle"></param>
        /// <param name="duration"></param>
        /// <param name="width"></param>
        /// <param name="imageType"></param>
        /// <param name="title"></param>
        public static void NotifyUnique(string id, string html, MessageImg imageType, string title, string subTitle, Action<HtmlLinkClickedEventArgs> clickHandler, int duration = 0, int width = 450) {
            Task.Factory.StartNew(() => {

                try {
                    if (Ready) {
                        _anchorForm.BeginInvoke((Action) delegate {
                            var toastNotification = new YamuiNotifications(HtmlHandler.FormatMessage(html, imageType, title, subTitle), duration, width, Npp.GetNppScreen());

                            if (id != null) {
                                // close existing notification with the same id
                                CloseUniqueNotif(id);
                                // Remember this notification
                                _registeredNotif.Add(id, toastNotification);
                            }

                            if (clickHandler != null)
                                toastNotification.LinkClicked += (sender, args) => clickHandler(args);
                            else
                                toastNotification.LinkClicked += Utils.OpenPathClickHandler;

                            toastNotification.Show();
                        });
                        return;
                    }

                    ErrorHandler.Log(html);
                } catch (Exception e) {
                    ErrorHandler.Log(e.Message);

                    // if we are here, display the error message the old way
                    MessageBox.Show("An error has occurred and we couldn't display a notification.\n\nCheck the log at the following location to learn more about this error : " + Config.FileErrorLog.ProgressQuoter() + "\n\nTry to restart Notepad++, consider opening an issue on : " + Config.IssueUrl + " if the problem persists.", AssemblyInfo.AssemblyProduct + " error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        public static void Notify(string html, MessageImg imageType, string title, string subTitle, Action<HtmlLinkClickedEventArgs> clickHandler, int duration = 0, int width = 450) {
            NotifyUnique(null, html, imageType, title, subTitle, clickHandler, duration, width);
        }

        public static void Notify(string html, MessageImg imageType, string title, string subTitle, int duration = 0, int width = 450) {
            NotifyUnique(null, html, imageType, title, subTitle, null, duration, width);
        }

        public static void Notify(string html, int duration = 10, int width = 450) {
            Notify(html, MessageImg.MsgDebug, "Debug message", "Should not appear in prod?", duration, width);
        }

        #endregion

        #region Message

        /// <summary>
        /// Displays a messagebox like window
        /// REMARK : DON'T WAIT FOR AN ANSWER IF YOU CALL IT FROM A THREAD!!!!!!!
        /// new List string  { "fu", "ok" }
        /// </summary>
        /// <returns>returns an integer (-1 if closed, or from 0 to x = buttons.count - 1)</returns>
        public static int Message(string html, MessageImg type, string title, string subTitle, List<string> buttons, bool waitResponse, EventHandler<HtmlLinkClickedEventArgs> clickHandler = null) {
            var clickedButton = -1;
            if (_anchorForm != null) {
                if (clickHandler == null) {
                    clickHandler = Utils.OpenPathClickHandler;
                }
                if (waitResponse) {
                    clickedButton = YamuiFormMessageBox.ShwDlg(Npp.GetNppScreen(), Npp.HandleNpp, title, HtmlHandler.FormatMessage(html, type, title, subTitle, true), buttons, true, clickHandler);
                } else {
                    if (_anchorForm.IsHandleCreated) {
                        _anchorForm.BeginInvoke((Action) delegate {
                            clickedButton = YamuiFormMessageBox.ShwDlg(Npp.GetNppScreen(), Npp.HandleNpp, title, HtmlHandler.FormatMessage(html, type, title, subTitle, true), buttons, false, clickHandler);
                        });
                    }
                }
            }
            return clickedButton;
        }

        #endregion
    }
}
