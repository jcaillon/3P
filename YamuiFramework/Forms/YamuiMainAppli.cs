#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiMainAppli.cs) is part of YamuiFramework.
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Timers;
using System.Web.UI.Design.WebControls;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;
using Timer = System.Timers.Timer;

namespace YamuiFramework.Forms {
    public class YamuiMainAppli : YamuiFormButtons {
        #region Fields

        protected override Padding DefaultPadding {
            get { return new Padding(8, 40, BorderWidth + 16, BorderWidth + 16); }
        }

        /// <summary>
        /// is set to true when this form is the parent of a yamuimsgbox
        /// </summary>
        [Browsable(false)]
        public bool HasModalOpened { get; set; }

        private YamuiTab _contentTab;

        private YamuiTabButtons _topLinks;

        private YamuiNotifLabel _bottomNotif;

        #endregion

        #region Constructor / destructor

        public YamuiMainAppli() {
            StartPosition = FormStartPosition.CenterScreen;
            TransparencyKey = Color.Fuchsia;
        }

        #endregion

        #region For the user

        /// <summary>
        /// Go to page pagename
        /// </summary>
        public void ShowPage(string pageName) {
            if (_contentTab != null)
                _contentTab.ShowPage(pageName);
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            if (!e.Cancel) {
                if (_contentTab != null)
                    _contentTab.ExecuteOnClose();
            }
        }

        /// <summary>
        /// Automatically generates the tabs/pages
        /// </summary>
        public void CreateContent(List<YamuiMainMenu> menuDescriber) {
            _contentTab = new YamuiTab(menuDescriber, this) {
                Dock = DockStyle.Fill
            };
            Controls.Add(_contentTab);
            _contentTab.Init();
        }

        /// <summary>
        /// Automatically generates top links
        /// </summary>
        public void CreateTopLinks(List<string> links, EventHandler<TabPressedEventArgs> onTabPressed, int xPosFromRight = 120, int yPosFromTop = 10) {
            _topLinks = new YamuiTabButtons(links, -1) {
                Font = FontManager.GetFont(FontFunction.TopLink),
                Height = 15,
                SpaceBetweenText = 14,
                DrawSeparator = true,
                WriteFromRight = true,
                UseLinksColors = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            _topLinks.TabPressed += onTabPressed;
            _topLinks.Width = _topLinks.GetWidth() + 10;
            _topLinks.Location = new Point(Width - xPosFromRight - _topLinks.Width, yPosFromTop);
            Controls.Add(_topLinks);
        }

        /// <summary>
        /// Displays an animated notification on the bottom of the form
        /// you can choose how much time the notif will last (in seconds)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stickDurationSecs"></param>
        public void Notify(string message, int stickDurationSecs) {
            if (_bottomNotif == null) {
                _bottomNotif = new YamuiNotifLabel {
                    Font = FontManager.GetFont(FontFunction.Normal),
                    Text = "",
                    Size = new Size(Width - 20 - BorderWidth, 16),
                    Location = new Point(BorderWidth, Height - 17),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                };
                Controls.Add(_bottomNotif);
            }
            _bottomNotif.Duration = stickDurationSecs;
            _bottomNotif.AnimText = message;
        }

        public Control FindFocusedControl() {
            if (_contentTab == null)
                return ActiveControl;
            Control control = _contentTab;
            var container = control as IContainerControl;
            while (container != null) {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }

        #endregion

        #region Bottom notif

        /// <summary>
        /// Small class to animate a text display
        /// </summary>
        internal class YamuiNotifLabel : UserControl {
            #region public fields

            /// <summary>
            /// Min 3s, the duration the text stays
            /// </summary>
            public int Duration { get; set; }

            /// <summary>
            /// The final text you want to display
            /// </summary>
            public string AnimText {
                set {
                    LinearBlink = 0;
                    ForeColor = YamuiThemeManager.Current.LabelNormalFore;
                    var t = new Transition(new TransitionType_Linear(500));
                    t.add(this, "Text", value);
                    t.add(this, "ForeColor", YamuiThemeManager.Current.AccentColor);
                    if (!string.IsNullOrEmpty(value))
                        t.add(this, "LinearBlink", 100);
                    else
                        LinearBlink = 21;
                    t.TransitionCompletedEvent += OnTransitionCompletedEvent;
                    t.run();

                    // end of duration event
                    if (!string.IsNullOrEmpty(value)) {
                        if (_durationTimer == null) {
                            _durationTimer = new Timer {
                                AutoReset = false
                            };
                            _durationTimer.Elapsed += DurationTimerOnElapsed;
                        }
                        _durationTimer.Stop();
                        _durationTimer.Interval = Math.Max(Duration, 3)*1000;
                        _durationTimer.Start();
                    }
                }
            }

            /// <summary>
            /// For animation purposes, don't use
            /// </summary>
            public int LinearBlink {
                get { return _linearBlink; }
                set {
                    _linearBlink = value;
                    if (_linearBlink == 0) {
                        _linearCount = 20;
                        _linearBool = false;
                    }
                    if (_linearBlink >= _linearCount) {
                        _linearBool = !_linearBool;
                        _linearCount += 20;
                    }
                    BackColor = !_linearBool ? YamuiThemeManager.Current.AccentColor : YamuiThemeManager.Current.FormBack;
                }
            }

            #endregion

            #region private

            private int _linearBlink;
            private bool _linearBool;
            private int _linearCount;

            private Timer _durationTimer;

            private void OnTransitionCompletedEvent(object sender, Transition.Args args) {
                if (string.IsNullOrEmpty(Text))
                    return;
                var t = new Transition(new TransitionType_Linear(500));
                t.add(this, "ForeColor", YamuiThemeManager.Current.LabelNormalFore);
                t.run();
                LinearBlink = 0;
                var t2 = new Transition(new TransitionType_Linear(2000));
                t2.add(this, "LinearBlink", 401);
                t2.run();
            }

            private void DurationTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
                AnimText = "";
            }

            #endregion

            #region constructor / destructor

            public YamuiNotifLabel() {
                SetStyle(
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.ResizeRedraw
                    , true);
            }

            protected override void Dispose(bool disposing) {
                if (_durationTimer != null)
                    _durationTimer.Dispose();
                base.Dispose(disposing);
            }

            #endregion

            #region paint

            protected override void OnPaint(PaintEventArgs e) {
                e.Graphics.Clear(YamuiThemeManager.Current.FormBack);

                // blinking square
                using (SolidBrush b = new SolidBrush(BackColor)) {
                    Rectangle boxRect = new Rectangle(0, 0, 10, Height);
                    e.Graphics.FillRectangle(b, boxRect);
                }

                // text
                TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(12, 0, ClientSize.Width - 12, ClientSize.Height), ForeColor, TextFormatFlags.Top | TextFormatFlags.Left | TextFormatFlags.NoPadding);
            }

            #endregion
        }

        #endregion

        #region Events

        protected override void OnShown(EventArgs e) {
            // Processes all Windows messages currently in the message queue.
            Application.DoEvents();
            base.OnShown(e);
        }

        #endregion

        #region Methods

        [SecuritySafeCritical]
        public bool FocusMe() {
            return WinApi.SetForegroundWindow(Handle);
        }

        #endregion
    }

    #region Designer

    internal class YamuiFormDesigner : FormViewDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("Font");
            properties.Remove("Text");
            base.PreFilterProperties(properties);
        }
    }

    #endregion
}