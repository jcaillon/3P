#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormMessageBox.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;

namespace YamuiFramework.Forms {
    public sealed partial class YamuiFormMessageBox : YamuiForm {

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
                    try {
                        Close();
                    } catch (Exception) {
                        // ignored
                    }
                    return;
                }
                base.Opacity = value;
            }
        }

        private static int _dialogResult = -1;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor, you should the method ShwDlg instead
        /// </summary>
        private YamuiFormMessageBox(string htmlContent, List<string> buttonsList, int maxHeight, int maxWidth, int minWidth = 300) {
            InitializeComponent();

            // register to the panel onclicked event and propagate it as a public field of this class
            contentLabel.LinkClicked += LinkClicked;

            // Set buttons
            int i = 0;
            int cumButtonWidth = 0;
            foreach (var buttonText in buttonsList) {
                var size = TextRenderer.MeasureText(buttonText, FontManager.GetStandardFont());

                var yamuiButton1 = new YamuiButton {
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Size = new Size(Math.Max(size.Width + 10, 80), 25),
                    Name = "yamuiButton" + i,
                    TabIndex = buttonsList.Count - i,
                    Tag = i,
                    Text = buttonText
                };
                yamuiButton1.Location = new Point(Width - 12 - yamuiButton1.Width - cumButtonWidth, Height - 12 - yamuiButton1.Height);
                cumButtonWidth += yamuiButton1.Width + 5;
                yamuiButton1.ButtonPressed += (sender, args) => {
                    _dialogResult = (int) ((YamuiButton) sender).Tag;
                    Close();
                };
                Controls.Add(yamuiButton1);
                i++;
            }

            // set label size
            contentLabel.SetNeededSize(htmlContent, Math.Max(cumButtonWidth + 12 + 30, minWidth), maxWidth);
            contentPanel.ContentPanel.Size = contentLabel.Size;

            // set form size
            Size = new Size(contentLabel.Width + 10, Math.Min(maxHeight, contentLabel.Height + 85));
            if (contentLabel.Height + 85 > maxHeight) {
                Width += 10;
            }
            MinimumSize = Size;

            // add outro animation
            Tag = false;
            Closing += (sender, args) => {
                // cancel initialise close to run an animation, after that allow it
                if ((bool) Tag) return;
                Tag = true;
                args.Cancel = true;
                Transition.run(this, "Opacity", 1d, -0.01d, new TransitionType_Linear(400));
            };

            Shown += (sender, args) => {
                Focus();
                WinApi.SetForegroundWindow(Handle);
            };
        }

        #endregion

        #region override

        // allow the user to use the scroll directly when the messagebox shows, instead of having to click on the scroller
        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            ActiveControl = contentLabel;
            contentLabel.Focus();
        }

        #endregion


        /// <summary>
        /// Show a message box dialog
        /// </summary>
        public static int ShwDlg(Screen screen, IntPtr ownerHandle, MessageImage type, string heading, string text, List<string> buttonsList, bool waitResponse, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null) {

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

            return ShwDlg(screen, ownerHandle, text, buttonsList, waitResponse, onLinkClicked);
        }

        public static int ShwDlg(Screen screen, IntPtr ownerHandle, string text, List<string> buttonsList, bool waitResponse, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null) {

            // new message box
            var msgbox = new YamuiFormMessageBox(text, buttonsList, screen.WorkingArea.Height * 8/10, screen.WorkingArea.Width * 8 / 10) { ShowInTaskbar = !waitResponse, TopMost = true };

            if (onLinkClicked != null)
                msgbox.LinkClicked += onLinkClicked;

            var ownerRect = WinApi.GetWindowRect(ownerHandle);

            // center parent
            msgbox.Location = new Point((ownerRect.Width - msgbox.Width) / 2 + ownerRect.X, (ownerRect.Height - msgbox.Height) / 2 + ownerRect.Y);

            // get yamui form
            var curForm = FromHandle(ownerHandle);
            var yamuiForm = curForm as YamuiForm;

            // we either display a modal or a normal messagebox
            Transition.run(msgbox, "Opacity", 0d, 1d, new TransitionType_Linear(400));
            if (waitResponse) {
                if (yamuiForm != null) {
                    yamuiForm.HasModalOpened = true;
                }
                msgbox.ShowDialog(new WindowWrapper(ownerHandle));
                if (yamuiForm != null) {
                    yamuiForm.HasModalOpened = false;
                }
                // get focus back to owner
                WinApi.SetForegroundWindow(ownerHandle);
            } else {
                msgbox.Show(new WindowWrapper(ownerHandle));
            }
            return _dialogResult;
        }
    }

    public enum MessageImage {
        Free,
        Error,
        HighImportance,
        Info,
        Ok,
        Question,
        QuestionShield,
        Services,
        Warning,
        WarningShield,
    }
}
