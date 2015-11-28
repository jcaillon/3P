#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiFormMessageBox.cs) is part of YamuiFramework.
// 
// // YamuiFramework is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // YamuiFramework is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.Resources;

namespace YamuiFramework.Forms {
    public partial class YamuiFormMessageBox : YamuiForm {

        #region Fields
        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// This field is used for the fade in/out animation, shouldn't be used by the user
        /// </summary>
        public new double Opacity {
            get { return base.Opacity; }
            set {
                if (value < 0) {
                    try { Close(); } catch (Exception) {
                        // ignored
                    }
                    return;
                }
                base.Opacity = value;
            }
        }

        private static int _dialogResult = -1;
        private const int ButtonWidth = 110;
        public static YamuiSmokeScreen OwnerSmokeScreen;
        #endregion

        /// <summary>
        /// Constructor, you should the method ShwDlg instead
        /// </summary>
        /// <param name="type"></param>
        /// <param name="htmlContent"></param>
        /// <param name="buttonsList"></param>
        /// <param name="dontWrapLines"></param>
        private YamuiFormMessageBox(string htmlContent, List<string> buttonsList, bool dontWrapLines) {
            InitializeComponent();

            // register to the panel onclicked event and propagate it as a public field of this class
            contentLabel.LinkClicked += OnLinkClicked;

            // Set buttons
            int i = 0;
            foreach (var buttonText in buttonsList) {
                var yamuiButton1 = new YamuiButton {
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Size = new Size(ButtonWidth, 25),
                    Name = "yamuiButton" + i,
                    TabIndex = buttonsList.Count - i,
                    Tag = i,
                    Text = buttonText
                };
                yamuiButton1.Location = new Point(Width - 12 - ButtonWidth - (ButtonWidth + 5) * i, Height - 12 - yamuiButton1.Height);
                yamuiButton1.ButtonPressed += (sender, args) => {
                    _dialogResult = (int) args.OriginalEventArgs;
                    Close();
                };
                Controls.Add(yamuiButton1);
                i++;
            }

            var minButtonsWidth = (ButtonWidth + 5) * buttonsList.Count + 12 + 30;

            // small loop to compute a good width that is enough to display the entire lines of the input text
            if (dontWrapLines) {
                // find max height taken by the html
                Width = Screen.PrimaryScreen.WorkingArea.Width;
                contentLabel.Text = htmlContent;
                Height = Math.Min(contentPanel.Location.Y + contentLabel.Location.Y + contentLabel.Height + 55, Screen.PrimaryScreen.WorkingArea.Height);

                // now we got the final height, resize width until height changes
                int j = 0;
                int detla = 300;
                int curWidth = Width;
                do {
                    curWidth -= detla;
                    Width = Math.Max(minButtonsWidth, curWidth);
                    contentLabel.Text = htmlContent;
                    var compHeight = Math.Min(contentPanel.Location.Y + contentLabel.Location.Y + contentLabel.Height + 55, Screen.PrimaryScreen.WorkingArea.Height);
                    if (compHeight > Height) {
                        curWidth += detla;
                        detla /= 2;
                    }
                    j++;
                } while (j < 10);
            } else {
                // resize form and panel (basically, we dont allow the form to be more tall than fat and we prefere it squared)
                int j = 0;
                do {
                    Width = minButtonsWidth;
                    contentLabel.Text = htmlContent;
                    var compHeight = contentPanel.Location.Y + contentLabel.Location.Y + contentLabel.Height + 55;
                    compHeight = Math.Min(compHeight, Screen.PrimaryScreen.WorkingArea.Height);
                    Height = compHeight;
                    minButtonsWidth = minButtonsWidth * (compHeight / minButtonsWidth);
                    j++;
                } while (j < 2 && Height > Width);
            }
            MinimumSize = new Size(Width, Height);

            // add outro animation
            Tag = false;
            Closing += (sender, args) => {
                // cancel initialise close to run an animation, after that allow it
                if ((bool)Tag) return;
                Tag = true;
                args.Cancel = true;
                FadeOut(this);
            };
            
            Shown += (sender, args) => {
                Focus();
                WinApi.SetForegroundWindow(Handle);
            };
        }

        /// <summary>
        /// Show a message box dialog
        /// </summary>
        /// <param name="ownerHandle"></param>
        /// <param name="type">this is used to know which image to put on message box</param>
        /// <param name="heading">Title of the msgbox</param>
        /// <param name="text"></param>
        /// <param name="buttonsList">new List of string { "button1", "buttton2" }</param>
        /// <param name="waitResponse">do we need to wait for an answer or not?</param>
        /// <param name="onLinkClicked">to execute on a click on a link (can be null)</param>
        /// <param name="dontWrapLines">set to true if you want to display you whole lines without going on a new line</param>
        /// <returns>returns an integer (-1 if closed, or from 0 to x = buttons.count - 1)</returns>
        /// <remarks>As of today, you can only show 1 msgbox at a given time!!!</remarks>
        public static int ShwDlg(IntPtr ownerHandle, MessageImage type, string heading, string text, List<string> buttonsList, bool waitResponse, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null, bool dontWrapLines = false) {

            text = @"
                <table style='margin-bottom: 7px;>
                    <tr>
                        <td rowspan='2'><img style='padding-right: 10px;' src='" + type + @"' width='64' height='64' /></td>
                        <td class='NotificationTitle'>" + heading + @"</td>
                    </tr>
                    <tr>
                        <td class='NotificationSubTitle'></td>
                    </tr>
                </table><br>" + text;

            return ShwDlg(ownerHandle, text, buttonsList, waitResponse, onLinkClicked, dontWrapLines);
        }

        public static int ShwDlg(IntPtr ownerHandle, string text, List<string> buttonsList, bool waitResponse, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null, bool dontWrapLines = false) {
            // try to get the owner as a yamui form, if we can then we will animate both the msgbox and the main form
            YamuiForm ownerForm = null;
            try {
                ownerForm = FromHandle(ownerHandle) as YamuiForm;
            } catch (Exception) {
                // ignored
            }

            // new message box
            var msgbox = new YamuiFormMessageBox(text, buttonsList, dontWrapLines) { ShowInTaskbar = !waitResponse, TopMost = true };

            if (onLinkClicked != null)
                msgbox.LinkClicked += onLinkClicked;

            if (ownerForm != null && ownerForm.Width > msgbox.Width && ownerForm.Height > msgbox.Height) {
                // center parent
                msgbox.Location = new Point((ownerForm.Width - msgbox.Width) / 2 + ownerForm.Location.X, (ownerForm.Height - msgbox.Height) / 2 + ownerForm.Location.Y);
            } else {
                // center screen
                msgbox.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - msgbox.Width) / 2 + Screen.PrimaryScreen.WorkingArea.Location.X, (Screen.PrimaryScreen.WorkingArea.Height - msgbox.Height) / 2 + Screen.PrimaryScreen.WorkingArea.Location.Y);
            }

            // we either display a modal or a normal messagebox
            FadeIn(msgbox, ownerForm);
            if (waitResponse) {
                if (ownerForm != null) ownerForm.HasModalOpened = true;
                msgbox.ShowDialog(new WindowWrapper(ownerHandle));
                if (ownerForm != null) ownerForm.HasModalOpened = false;
                if (OwnerSmokeScreen != null) {
                    OwnerSmokeScreen.Close();
                    OwnerSmokeScreen = null;
                }
                // get focus back to owner
                try {
                    WinApi.SetForegroundWindow(ownerHandle);
                } catch (Exception) {
                    //ignored
                }
            } else {
                msgbox.Show(new WindowWrapper(ownerHandle));
            }
            return _dialogResult;
        }

        private static void FadeIn(YamuiFormMessageBox msgBox, YamuiForm ownerForm) {
            // transition on msgbox
            Transition t = new Transition(new TransitionType_CriticalDamping(400));
            t.add(msgBox, "Opacity", 1d);
            msgBox.Opacity = 0d;

            // if owner isn't a yamuiform then run anim on msgbox only
            if (OwnerSmokeScreen != null) { t.run(); return; }
            if (ownerForm == null) { t.run(); return; }

            // otherwise had fadein of smokescreen for yamuiform
            OwnerSmokeScreen = new YamuiSmokeScreen(ownerForm);
            t.add(OwnerSmokeScreen, "Opacity", OwnerSmokeScreen.Opacity);
            OwnerSmokeScreen.Opacity = 0d;
            t.run();
        }

        private static void FadeOut(YamuiFormMessageBox msgBox) {
            // transition on msgbox
            Transition t = new Transition(new TransitionType_CriticalDamping(400));
            t.add(msgBox, "Opacity", -0.01d);
            if (OwnerSmokeScreen == null) { t.run(); return; }
            t.add(OwnerSmokeScreen, "Opacity", 0d);
            t.run();
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e) {
            var handler = LinkClicked;
            if (handler != null)
                handler(this, e);
        }

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e) {
            OnLinkClicked(e);
        }
    }

    public enum MessageImage {
        Free,
        Logo,
        Ant,
        Error,
        HighImportance,
        Info,
        Ok,
        Pin,
        Poison,
        Question,
        QuestionShield,
        RadioActive,
        Services,
        Skull,
        Warning,
        WarningShield,
        Update
    }
}
