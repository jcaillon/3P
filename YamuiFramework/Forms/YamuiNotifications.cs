using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {

    public partial class YamuiNotifications : YamuiForm {

        #region fields
        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        private static List<YamuiNotifications> _openNotifications = new List<YamuiNotifications>();
        private bool _allowFocus;
        private IntPtr _currentForegroundWindow;
        private int _duration;
        private YamuiPanel _progressPanel;
        #endregion

        /// <summary>
        /// Create a new notification, to be displayed with Show() later
        /// </summary>
        /// <param name="body">content of the notification</param>
        /// <param name="duration">life time in seconds, if 0 then it's a sticky notif</param>
        /// <param name="defaultWidth"></param>
        public YamuiNotifications(string body, int duration, int defaultWidth = 300) {
            InitializeComponent();

            Load += YamuiNotificationsLoad;
            Activated += YamuiNotificationsActivated;
            Shown += YamuiNotificationsShown;
            FormClosed += YamuiNotificationsFormClosed;

            // register to the panel onclicked event and propagate it as a public field of this class
            contentLabel.LinkClicked += OnLinkClicked;


            // resize form and panel
            int j = 0;
            int compWidth = defaultWidth;
            do {
                Width = compWidth;
                contentLabel.Text = body;
                var compHeight = contentLabel.Height + 10 + ((duration > 0) ? 10 : 0);
                compHeight = Math.Min(compHeight, Screen.PrimaryScreen.WorkingArea.Height);
                Height = compHeight;
                compWidth = compWidth * (compHeight / compWidth);
                j++;
            } while (j < 2 && Height > Width);

            // do we need to animate a panel on the bottom to visualise time left
            if (duration > 0) {
                _progressPanel = new YamuiPanel {
                    BackColor = ThemeManager.AccentColor,
                    AutoScroll = false,
                    Location = new Point(1, Height - 11),
                    Name = "progressPanel",
                    Size = new Size(Width - 2, 10),
                    TabStop = false,
                    UseCustomBackColor = true
                };
                Controls.Add(_progressPanel);
                _duration = duration*1000;
            } else
                _duration = 0;

            // fade out animation
            Opacity = 0d;
            Tag = false;
            Closing += (sender, args) => {
                if ((bool)Tag) return;
                args.Cancel = true;
                Tag = true;
                var t = new Transition(new TransitionType_Acceleration(200));
                t.add(this, "Opacity", 0d);
                t.TransitionCompletedEvent += (o, args1) => { Close(); };
                t.run();
            };
            // fade in animation
            Transition.run(this, "Opacity", 1d, new TransitionType_Acceleration(200));
        }

        #region Methods
        public new void Show() {
            // Prevent the form taking focus when it is initially shown
            _currentForegroundWindow = WinApi.GetForegroundWindow();
            base.Show();
        }

        #endregion // Methods

        #region Event Handlers
        private void YamuiNotificationsActivated(object sender, EventArgs e) {
            // Prevent the form taking focus when it is initially shown
            if (!_allowFocus) {
                // Activate the window that previously had focus
                WinApi.SetForegroundWindow(_currentForegroundWindow);
            }
        }

        private void YamuiNotificationsLoad(object sender, EventArgs e) {
            // Display the form just above the system tray.
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width,
                                      Screen.PrimaryScreen.WorkingArea.Height - Height - 5);

            // be sure it always stay on top of every window
            WinApi.SetWindowPos(Handle, WinApi.HWND_TOPMOST, 0, 0, 0, 0, WinApi.TOPMOST_FLAGS);

            // Move each open form upwards to make room for this one
            foreach (YamuiNotifications openForm in _openNotifications) {
                openForm.Top -= Height + 5;
            }

            _openNotifications.Add(this);
        }


        private void YamuiNotificationsShown(object sender, EventArgs e) {
            // Once the animation has completed the form can receive focus
            _allowFocus = true;
            if (_duration > 0) {
                var t = new Transition(new TransitionType_Linear(_duration));
                t.add(_progressPanel, "Width", 0);
                t.TransitionCompletedEvent += (o, args) => { Close(); };
                t.run();
            }
        }

        private void YamuiNotificationsFormClosed(object sender, FormClosedEventArgs e) {
            // Move down any open forms above this one
            foreach (YamuiNotifications openForm in _openNotifications) {
                if (openForm == this) {
                    // Remaining forms are below this one
                    break;
                }
                openForm.Top += Height + 5;
            }
            _openNotifications.Remove(this);
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

        #endregion // Event Handlers
    }
}
