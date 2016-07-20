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
using _3PA.Lib;
using _3PA.MainFeatures.NppInterfaceForm;

// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures {

    internal static class UserCommunication {

        #region private fields

        private static EmptyForm _anchorForm;

        private static List<YamuiInput> _openedMessage = new List<YamuiInput>();

        /// <summary>
        /// Allows to keep track of opened notifications (when its ID is set)
        /// </summary>
        private static Dictionary<string, YamuiNotification> _registeredNotif = new Dictionary<string, YamuiNotification>(StringComparer.CurrentCultureIgnoreCase);

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
            YamuiNotification.CloseEverything();
            foreach (var yamuiInput in _openedMessage) {
                if (yamuiInput != null) {
                    yamuiInput.Tag = true;
                    yamuiInput.Close();
                    yamuiInput.Dispose();
                }
            }
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
        /// <param name="htmlContent"></param>
        /// <param name="clickHandler"></param>
        /// <param name="subTitle"></param>
        /// <param name="duration"></param>
        /// <param name="width"></param>
        /// <param name="imageType"></param>
        /// <param name="title"></param>
        public static void NotifyUnique(string id, string htmlContent, MessageImg imageType, string title, string subTitle, Action<HtmlLinkClickedEventArgs> clickHandler, int duration = 0, int width = 450) {
            Task.Factory.StartNew(() => {

                try {
                    if (Ready) {
                        _anchorForm.BeginInvoke((Action)delegate {
                            var nppScreen = Npp.NppScreen;
                            var toastNotification = new YamuiNotification(
                                ThemeManager.FormatTitle(imageType, title, subTitle),
                                ThemeManager.FormatContent(htmlContent),
                                duration,
                                nppScreen,
                                Math.Min(width, nppScreen.WorkingArea.Width / 3),
                                nppScreen.WorkingArea.Width / 3,
                                nppScreen.WorkingArea.Height / 3,
                                (sender, args) => {
                                    if (clickHandler != null) clickHandler(args);
                                    else Utils.OpenPathClickHandler(sender, args);
                                });

                            if (id != null) {
                                // close existing notification with the same id
                                CloseUniqueNotif(id);
                                // Remember this notification
                                _registeredNotif.Add(id, toastNotification);
                            }

                            toastNotification.Show();
                        });
                        return;
                    }

                    ErrorHandler.Log(htmlContent);
                } catch (Exception e) {
                    ErrorHandler.Log(e.Message);

                    // if we are here, display the error message the old way
                    MessageBox.Show("An error has occurred and we couldn't display a notification.\n\nCheck the log at the following location to learn more about this error : " + Config.FileErrorLog.ProQuoter() + "\n\nTry to restart Notepad++, consider opening an issue on : " + Config.IssueUrl + " if the problem persists.", AssemblyInfo.AssemblyProduct + " error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// Displays a messagebox like window, 
        /// REMARK : DON'T WAIT FOR AN ANSWER IF YOU CALL IT FROM A THREAD!!!!!!!, 
        /// new List string  { "Ok", "Cancel" }, 
        /// returns an integer (-1 if closed, or from 0 to x = buttons.count - 1),
        /// buttonsList default to { "Ok", "Cancel" }
        /// </summary>
        public static int Message(string htmlContent, MessageImg imageType, string title, string subTitle, List<string> buttonsList = null, bool waitResponse = true, Action<HtmlLinkClickedEventArgs> clickHandler = null) {
            object nullObject = null;
            return Message(ref nullObject, htmlContent, imageType, title, subTitle, buttonsList, waitResponse, clickHandler);
        }

        public static int Message(ref object data, string htmlContent, MessageImg imageType, string title, string subTitle, List<string> buttonsList = null, bool waitResponse = true, Action<HtmlLinkClickedEventArgs> clickHandler = null, int minWidth = 450) {
            var clickedButton = -1;

            if (buttonsList == null)
                buttonsList = new List<string> { "Ok", "Cancel" };

            if (waitResponse) {
                clickedButton = YamuiInput.ShowDialog(
                    Npp.HandleNpp,
                    "3P: " + title,
                    ThemeManager.FormatTitle(imageType, title, subTitle),
                    ThemeManager.FormatContent(htmlContent),
                    buttonsList,
                    ref data,
                    Npp.NppScreen.WorkingArea.Width * 3 / 5,
                    Npp.NppScreen.WorkingArea.Height * 9 / 10,
                    minWidth,
                    (sender, args) => {
                        if (clickHandler != null) clickHandler(args);
                        else Utils.OpenPathClickHandler(sender, args);
                    });
            } else {
                if (_anchorForm != null && _anchorForm.IsHandleCreated) {
                    _anchorForm.BeginInvoke((Action)delegate {
                        YamuiInput form;
                        object nullObject = null;
                        clickedButton = YamuiInput.Show(
                            Npp.HandleNpp,
                            "3P: " + title,
                            ThemeManager.FormatTitle(imageType, title, subTitle),
                            ThemeManager.FormatContent(htmlContent),
                            buttonsList,
                            ref nullObject,
                            out form,
                            Npp.NppScreen.WorkingArea.Width * 3 / 5,
                            Npp.NppScreen.WorkingArea.Height * 9 / 10,
                            minWidth,
                            (sender, args) => {
                                if (clickHandler != null) clickHandler(args);
                                else Utils.OpenPathClickHandler(sender, args);
                            });
                        _openedMessage.Add(form);
                    });
                }
            }
            return clickedButton;
        }

        #endregion

        #region Input

        /// <summary>
        /// Allows to ask information to the user,
        /// returns an integer (-1 if closed, or from 0 to x = buttons.count - 1), 
        /// buttonsList default to { "Ok", "Cancel" }
        /// </summary>
        public static int Input(ref object data, string htmlContent, MessageImg imageType, string title, string subTitle, List<string> buttonsList = null, Action<HtmlLinkClickedEventArgs> clickHandler = null) {
            return Message(ref data, htmlContent, imageType, title, subTitle, buttonsList, true, clickHandler);
        }

        #endregion

    }
}
