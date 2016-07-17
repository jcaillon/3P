#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiMessageBox.cs) is part of YamuiFramework.
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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {
    public sealed partial class YamuiNotification : YamuiForm {

        #region Static

        private static List<YamuiNotification> _openNotifications = new List<YamuiNotification>();

        public static void CloseEverything() {
            try {
                foreach (var yamuiNotification in _openNotifications.ToList()) {
                    if (yamuiNotification != null)
                        yamuiNotification.Close();
                }
            } catch (Exception) {
                // nothing much to do if this crashes
            }
        }

        /// <summary>
        /// The user clicked on the button to close all visible notifications
        /// </summary>
        private static void OnCloseAllVisibleNotif() {
            var toClose = _openNotifications.Where(openForm => openForm.Top > 0).ToList();
            toClose.ForEach(notifications => notifications.Close());
        }

        #endregion

        #region Fields
        
        private bool _allowFocus;
        private IntPtr _currentForegroundWindow;
        private int _duration;
        private YamuiSimplePanel _progressSimplePanel;
        private Transition _closingTransition;
        private Screen _screen;

        /// <summary>
        /// This field is used for the fade in/out animation, shouldn't be used by the user
        /// </summary>
        public double AnimationOpacity {
            get { return Opacity; }
            set {
                if (value < 0) {
                    try {
                        Close();
                    } catch (Exception) {
                        // ignored
                    }
                    return;
                }
                Opacity = value;
            }
        }

        //Very important to keep it. It prevents the form from stealing the focus
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        #endregion

        #region Don't show in ATL+TAB

        protected override CreateParams CreateParams {
            get {
                var Params = base.CreateParams;
                Params.ExStyle |= 0x80;
                return Params;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new notification, to be displayed with Show() later
        /// </summary>
        public YamuiNotification(string htmlTitle, string htmlMessage, int duration, Screen screenToUse = null, int formMinWidth = 0, int formMaxWidth = 0, int formMaxHeight = 0, EventHandler<HtmlLinkClickedEventArgs> onLinkClicked = null) {

            // close all notif button
            ShowCloseAllVisibleButton = true;
            OnCloseAllVisible = OnCloseAllVisibleNotif;

            InitializeComponent();

            // correct input if needed
            _screen = screenToUse ?? Screen.PrimaryScreen;
            if (formMaxWidth == 0)
                formMaxWidth = _screen.WorkingArea.Width - 20;
            if (formMaxHeight == 0)
                formMaxHeight = _screen.WorkingArea.Height - 20;
            if (formMinWidth == 0)
                formMinWidth = 300;

            contentPanel.NoBackgroundImage = true;

            // Set title, it will define a new minimum width for the message box
            var space = FormButtonWidth + BorderWidth*2 + titleLabel.Location.X + 5;
            titleLabel.SetNeededSize(htmlTitle, formMinWidth - space, formMaxWidth - space, true);
            formMinWidth = formMinWidth.ClampMin(titleLabel.Width + space);
            var newPadding = Padding;
            newPadding.Bottom = newPadding.Bottom + (duration > 0 ? 8 : 0); 
            newPadding.Top = titleLabel.Height + 10;
            Padding = newPadding;
            titleLabel.Location = new Point(5,5);

            // set content label
            space = Padding.Left + Padding.Right;
            contentLabel.SetNeededSize(htmlMessage, formMinWidth - space, formMaxWidth - space, true);
            contentPanel.ContentPanel.Size = contentLabel.Size;
            if (onLinkClicked != null)
                contentLabel.LinkClicked += onLinkClicked;

            // set form size
            Size = new Size(contentPanel.ContentPanel.Width + space, (Padding.Top + Padding.Bottom + contentLabel.Height).ClampMax(formMaxHeight));
            if (contentPanel.HasScrolls)
                Width += 10;
            MinimumSize = Size;

            // do we need to animate a panel on the bottom to visualise time left?
            if (duration > 0) {
                _progressSimplePanel = new YamuiSimplePanel {
                    BackColor = YamuiThemeManager.Current.AccentColor,
                    AutoScroll = false,
                    Location = new Point(1, Height - 9),
                    Name = "progressPanel",
                    Size = new Size(Width - 2, 8),
                    TabStop = false,
                    UseCustomBackColor = true
                };
                Controls.Add(_progressSimplePanel);
                _duration = duration * 1000;
            } else
                _duration = 0;

            // for the outro animation
            Tag = false;

            // fade in animation
            Transition.run(this, "AnimationOpacity", 0d, 1d, new TransitionType_Acceleration(200));
        }

        #endregion

        #region override

        // get activated window
        protected override void OnShown(EventArgs e) {
            // Once the animation has completed the form can receive focus
            _allowFocus = true;
            if (_duration > 0) {
                _closingTransition = new Transition(new TransitionType_Linear(_duration));
                _closingTransition.add(_progressSimplePanel, "Width", 0);
                _closingTransition.TransitionCompletedEvent += (o, args) => { Close(); };
                _closingTransition.run();
            }
            base.OnShown(e);
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            // cancel initialise close to run an animation, after that allow it
            if ((bool)Tag)
                return;
            Tag = true;
            e.Cancel = true;
            Transition.run(this, "AnimationOpacity", 1d, -0.01d, new TransitionType_Acceleration(200), (o, args1) => {
                Dispose();
            });
        }

        protected override void OnLoad(EventArgs e) {
            // Display the form just above the system tray.
            Location = new Point(_screen.WorkingArea.X + _screen.WorkingArea.Width - Width - 5, _screen.WorkingArea.Y + _screen.WorkingArea.Height - Height - 5);

            // be sure it always stay on top of every window
            WinApi.SetWindowPos(Handle, WinApi.HWND_TOPMOST, 0, 0, 0, 0, WinApi.TOPMOST_FLAGS);

            // Move each open form upwards to make room for this one
            foreach (YamuiNotification openForm in _openNotifications) {
                openForm.Top -= Height + 5;
            }

            _openNotifications.Add(this);
            base.OnLoad(e);
        }

        protected override void OnActivated(EventArgs e) {
            // Prevent the form taking focus when it is initially shown
            if (!_allowFocus) {
                // Activate the window that previously had focus
                WinApi.SetForegroundWindow(_currentForegroundWindow);
            } else {
                if (_closingTransition != null) {
                    _closingTransition.removeProperty(_closingTransition.TransitionedProperties.FirstOrDefault());
                    _closingTransition = null;
                    Controls.Remove(_progressSimplePanel);
                    _progressSimplePanel.Dispose();
                }
            }
            base.OnActivated(e);
        }

        protected override void OnClosed(EventArgs e) {
            // Move down any open forms above this one
            foreach (YamuiNotification openForm in _openNotifications) {
                if (openForm == this) {
                    // Remaining forms are below this one
                    break;
                }
                openForm.Top += Height + 5;
            }
            _openNotifications.Remove(this);
            base.OnClosed(e);
        }

        #endregion

        #region Show

        /// <summary>
        /// Call this method to show the notification
        /// </summary>
        public new void Show() {
            // Prevent the form taking focus when it is initially shown
            _currentForegroundWindow = WinApi.GetForegroundWindow();
            base.Show();
        }

        #endregion

    }

}
