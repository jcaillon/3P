#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiForm.cs) is part of YamuiFramework.
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
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;
using Timer = System.Timers.Timer;

namespace YamuiFramework.Forms {

    public class YamuiForm : Form {

        #region Fields

        private bool _isMovable = true;

        [Category("Yamui")]
        public bool Movable {
            get { return _isMovable; }
            set { _isMovable = value; }
        }

        [Category("Yamui")]
        [DefaultValue(false)]
        public bool UseCustomBorderColor { get; set; }

        /// <summary>
        /// Set this to true to show the "close all notifications button",
        /// to use with OnCloseAllVisible
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        public bool ShowCloseAllVisibleButton { get; set; }

        /// <summary>
        /// To use with ShowCloseAllVisibleButton,
        /// Action to do when the user click the button
        /// </summary>
        [Browsable(false)]
        public Action OnCloseAllVisible { get; set; }

        public new Padding Padding {
            get { return base.Padding; }
            set {
                //value.Top = Math.Max(value.Top, 40);
                base.Padding = value;
            }
        }

        protected override Padding DefaultPadding {
            get { return new Padding(8, 40, BorderWidth + 16, BorderWidth + 16); }
        }

        [Category("Yamui")]
        public bool Resizable {
            get { return _isResizable; }
            set { _isResizable = value; }
        }
        private bool _isResizable = true;

        [Category("Yamui")]
        [DefaultValue(true)]
        public bool SetMinSizeOnLoad { get; set; }

        /// <summary>
        /// is set to true when this form is the parent of a yamuimsgbox
        /// </summary>
        [Browsable(false)]
        public bool HasModalOpened { get; set; }

        private const int BorderWidth = 1;

        /// <summary>
        /// Tooltip for close buttons
        /// </summary>
        private HtmlToolTip _mainFormToolTip = new HtmlToolTip();

        private YamuiTab _contentTab;

        private YamuiTabButtons _topLinks;

        private YamuiNotifLabel _bottomNotif;

        #endregion

        #region Constructor / destructor

        public YamuiForm() {
            // why those styles? check here: 
            // https://sites.google.com/site/craigandera/craigs-stuff/windows-forms/flicker-free-control-drawing
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw
                , true);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            TransparencyKey = Color.Fuchsia;

            _mainFormToolTip.ShowAlways = true;

            Shown += OnShown;
        }

        ~YamuiForm() {
            Shown -= OnShown;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = ThemeManager.Current.FormColorBackColor;
            var foreColor = ThemeManager.Current.FormColorForeColor;
            var borderColor = UseCustomBorderColor ? ForeColor : ThemeManager.AccentColor;

            // background
            e.Graphics.Clear(backColor);

            // draw the border with Style color
            var rect = new Rectangle(new Point(0, 0), new Size(Width - BorderWidth, Height - BorderWidth));
            var pen = new Pen(borderColor, BorderWidth);
            e.Graphics.DrawRectangle(pen, rect);

            // draw the resize pixels icon on the bottom right
            if (Resizable && (SizeGripStyle == SizeGripStyle.Auto || SizeGripStyle == SizeGripStyle.Show)) {
                using (var b = new SolidBrush(foreColor)) {
                    var resizeHandleSize = new Size(2, 2);
                    e.Graphics.FillRectangles(b, new[] {
                        new Rectangle(new Point(ClientRectangle.Width - 6, ClientRectangle.Height - 6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 10, ClientRectangle.Height - 10), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 10, ClientRectangle.Height - 6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 6, ClientRectangle.Height - 10), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 14, ClientRectangle.Height - 6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width - 6, ClientRectangle.Height - 14), resizeHandleSize)
                    });
                }
            }
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
                    Size = new Size(Width - 21, 16),
                    Location = new Point(1, Height - 17),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
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
                    ForeColor = ThemeManager.Current.LabelsColorsNormalForeColor;
                    var t = new Transition(new TransitionType_Linear(500));
                    t.add(this, "Text", value);
                    t.add(this, "ForeColor", ThemeManager.AccentColor);
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
                        _durationTimer.Interval = Math.Max(Duration, 3) * 1000;
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
                    BackColor = !_linearBool ? ThemeManager.AccentColor : ThemeManager.Current.FormColorBackColor;
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
                t.add(this, "ForeColor", ThemeManager.Current.LabelsColorsNormalForeColor);
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

            ~YamuiNotifLabel() {
                _durationTimer.Dispose();
                _durationTimer = null;
            }

            #endregion

            #region paint

            protected override void OnPaint(PaintEventArgs e) {
                e.Graphics.Clear(ThemeManager.Current.FormColorBackColor);

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

        #region Management Methods

        /// <summary>
        /// allows to do stuff only when everything is fully loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnShown(object sender, EventArgs eventArgs) {
            // Processes all Windows messages currently in the message queue.
            Application.DoEvents();
        }

        /// <summary>
        /// On load of the form
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (DesignMode) return;

            switch (StartPosition) {
                case FormStartPosition.CenterParent:
                    CenterToParent();
                    break;
                case FormStartPosition.CenterScreen:
                    if (IsMdiChild) {
                        CenterToParent();
                    } else {
                        CenterToScreen();
                    }
                    break;
            }

            // display windows buttons
            RemoveCloseButton();
            if (ControlBox) {
                AddWindowButton(WindowButtons.Close);
                if (MaximizeBox)
                    AddWindowButton(WindowButtons.Maximize);
                if (MinimizeBox)
                    AddWindowButton(WindowButtons.Minimize);
                if (ShowCloseAllVisibleButton)
                    AddWindowButton(WindowButtons.CloseAllVisible);
                UpdateWindowButtonPosition();
            }

            // Focus content main menu
            if (_contentTab != null) {
                ActiveControl = _contentTab;
                _contentTab.OnFormLoad();
            }

            // set minimum size
            if (SetMinSizeOnLoad)
                MinimumSize = Size;
        }

        [SecuritySafeCritical]
        public bool FocusMe() {
            return WinApi.SetForegroundWindow(Handle);
        }

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnResizeEnd(EventArgs e) {
            base.OnResizeEnd(e);
            UpdateWindowButtonPosition();
        }

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg) {
                case (int)WinApi.Messages.WM_SYSCOMMAND:
                    var sc = m.WParam.ToInt32() & 0xFFF0;
                    switch (sc) {
                        case (int)WinApi.Messages.SC_MOVE:
                            if (!Movable) return;
                            break;
                        case (int)WinApi.Messages.SC_MAXIMIZE:
                            break;
                        case (int)WinApi.Messages.SC_RESTORE:
                            break;
                    }
                    break;

                case (int)WinApi.Messages.WM_NCLBUTTONDBLCLK:
                case (int)WinApi.Messages.WM_LBUTTONDBLCLK:
                    if (!MaximizeBox) return;
                    break;

                case (int)WinApi.Messages.WM_NCHITTEST:
                    var ht = HitTestNca(m.HWnd, m.WParam, m.LParam);
                    if (ht != WinApi.HitTest.HTCLIENT) {
                        m.Result = (IntPtr)ht;
                        return;
                    }
                    break;

                case (int)WinApi.Messages.WM_DWMCOMPOSITIONCHANGED:
                    break;

                case WmNcpaint: // box shadow
                    if (_mAeroEnabled) {
                        var v = 2;
                        DwmApi.DwmSetWindowAttribute(Handle, 2, ref v, 4);
                        var margins = new DwmApi.MARGINS(1, 1, 1, 1);
                        DwmApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
                    }
                    break;
            }

            base.WndProc(ref m);

            switch (m.Msg) {
                case (int)WinApi.Messages.WM_GETMINMAXINFO:
                    OnGetMinMaxInfo(m.HWnd, m.LParam);
                    break;
                case (int)WinApi.Messages.WM_SIZE:
                    if (_windowButtonList != null) {
                        YamuiFormButton btn;
                        _windowButtonList.TryGetValue(WindowButtons.Maximize, out btn);
                        if (WindowState == FormWindowState.Normal && btn != null) {
                            btn.Text = @"1";
                        }
                        if (WindowState == FormWindowState.Maximized && btn != null) btn.Text = @"2";
                    }
                    break;
            }
            if (m.Msg == WmNchittest && (int)m.Result == Htclient) // drag the form
                m.Result = (IntPtr)Htcaption;
        }

        [SecuritySafeCritical]
        private unsafe void OnGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            var pmmi = (WinApi.MINMAXINFO*)lParam;

            var s = Screen.FromHandle(hwnd);
            pmmi->ptMaxSize.x = s.WorkingArea.Width;
            pmmi->ptMaxSize.y = s.WorkingArea.Height;
            pmmi->ptMaxPosition.x = Math.Abs(s.WorkingArea.Left - s.Bounds.Left);
            pmmi->ptMaxPosition.y = Math.Abs(s.WorkingArea.Top - s.Bounds.Top);
        }

        private WinApi.HitTest HitTestNca(IntPtr hwnd, IntPtr wparam, IntPtr lparam) {
            var vPoint = new Point((short)lparam, (short)((int)lparam >> 16));
            var vPadding = Math.Max(Padding.Right, Padding.Bottom);
            if (Resizable) {
                if (RectangleToScreen(new Rectangle(ClientRectangle.Width - vPadding, ClientRectangle.Height - vPadding, vPadding, vPadding)).Contains(vPoint))
                    return WinApi.HitTest.HTBOTTOMRIGHT;
            }
            if (RectangleToScreen(new Rectangle(BorderWidth, BorderWidth, ClientRectangle.Width - 2 * BorderWidth, 50)).Contains(vPoint))
                return WinApi.HitTest.HTCAPTION;
            return WinApi.HitTest.HTCLIENT;
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && Movable) {
                if (WindowState == FormWindowState.Maximized) return;
                MoveControl();
            }
        }

        [SecuritySafeCritical]
        private void MoveControl() {
            WinApi.ReleaseCapture();
            WinApi.SendMessage(Handle, (int)WinApi.Messages.WM_NCLBUTTONDOWN, (int)WinApi.HitTest.HTCAPTION, 0);
        }

        #endregion

        #region Window Buttons

        [SecuritySafeCritical]
        public void RemoveCloseButton() {
            var hMenu = WinApi.GetSystemMenu(Handle, false);
            if (hMenu == IntPtr.Zero) return;

            var n = WinApi.GetMenuItemCount(hMenu);
            if (n <= 0) return;

            WinApi.RemoveMenu(hMenu, (uint)(n - 1), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.RemoveMenu(hMenu, (uint)(n - 2), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.DrawMenuBar(Handle);
        }

        private enum WindowButtons {
            Minimize,
            Maximize,
            Close,
            CloseAllVisible
        }

        private Dictionary<WindowButtons, YamuiFormButton> _windowButtonList;

        private void AddWindowButton(WindowButtons button) {
            if (_windowButtonList == null)
                _windowButtonList = new Dictionary<WindowButtons, YamuiFormButton>();

            if (_windowButtonList.ContainsKey(button))
                return;

            var newButton = new YamuiFormButton();

            switch (button) {
                case WindowButtons.Close:
                    newButton.Text = @"r";
                    _mainFormToolTip.SetToolTip(newButton, "<b>Close</b> this window");
                    break;
                case WindowButtons.Minimize:
                    newButton.Text = @"0";
                    _mainFormToolTip.SetToolTip(newButton, "<b>Minimize</b> this window");
                    break;
                case WindowButtons.Maximize:
                    newButton.Text = WindowState == FormWindowState.Normal ? @"1" : @"2";
                    _mainFormToolTip.SetToolTip(newButton, "<b>" + (WindowState == FormWindowState.Normal ? "Maximize" : "Restore") + "</b> this window");
                    break;
                case WindowButtons.CloseAllVisible:
                    newButton.Text = ((char)(126)).ToString();
                    _mainFormToolTip.SetToolTip(newButton, "<b>Close all visible</b> notification windows");
                    break;
            }

            newButton.Tag = button;
            newButton.Size = new Size(25, 20);
            newButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newButton.TabStop = false; //remove the form controls from the tab stop
            newButton.Click += WindowButton_Click;
            Controls.Add(newButton);

            _windowButtonList.Add(button, newButton);
        }

        private void WindowButton_Click(object sender, EventArgs e) {
            var btn = sender as YamuiFormButton;
            if (btn == null) return;
            if (((MouseEventArgs)e).Button != MouseButtons.Left) return;
            var btnFlag = (WindowButtons)btn.Tag;
            switch (btnFlag) {
                case WindowButtons.Close:
                    Close();
                    break;
                case WindowButtons.Minimize:
                    WindowState = FormWindowState.Minimized;
                    break;
                case WindowButtons.Maximize:
                    WindowState = WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
                    btn.Text = WindowState == FormWindowState.Normal ? @"1" : @"2";
                    _mainFormToolTip.SetToolTip(btn, "<b>" + (WindowState == FormWindowState.Normal ? "Maximize" : "Restore") + "</b> this window");
                    break;
                case WindowButtons.CloseAllVisible:
                    if (OnCloseAllVisible != null)
                        OnCloseAllVisible();
                    break;
            }
        }

        private void UpdateWindowButtonPosition() {
            if (!ControlBox) return;

            var priorityOrder = new Dictionary<int, WindowButtons>(3) { { 0, WindowButtons.Close }, { 1, WindowButtons.Maximize }, { 2, WindowButtons.Minimize } };

            var firstButtonLocation = new Point(ClientRectangle.Width - BorderWidth - 25, BorderWidth);
            var lastDrawedButtonPosition = firstButtonLocation.X - 25;

            YamuiFormButton firstButton = null;

            foreach (var button in priorityOrder) {
                var buttonExists = _windowButtonList.ContainsKey(button.Value);

                if (firstButton == null && buttonExists) {
                    firstButton = _windowButtonList[button.Value];
                    firstButton.Location = firstButtonLocation;
                    continue;
                }

                if (firstButton == null || !buttonExists) continue;

                _windowButtonList[button.Value].Location = new Point(lastDrawedButtonPosition, BorderWidth);
                lastDrawedButtonPosition = lastDrawedButtonPosition - 25;
            }

            if (_windowButtonList.ContainsKey(WindowButtons.CloseAllVisible)) {
                _windowButtonList[WindowButtons.CloseAllVisible].Location = new Point(ClientRectangle.Width - BorderWidth - 25, BorderWidth + 25);
            }

            Refresh();
        }

        public class YamuiFormButton : Label {
            #region Constructor

            public YamuiFormButton() {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);
            }

            #endregion

            #region Paint Methods
            protected override void OnPaint(PaintEventArgs e) {
                if (_isPressed)
                    e.Graphics.Clear(ThemeManager.AccentColor);
                else if (_isHovered)
                    e.Graphics.Clear(ThemeManager.Current.ButtonColorsHoverBackColor);
                else
                    e.Graphics.Clear(ThemeManager.Current.FormColorBackColor);
                //PaintTransparentBackground(e.Graphics, DisplayRectangle);

                Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, false, false, _isHovered, _isPressed, Enabled);
                TextRenderer.DrawText(e.Graphics, Text, new Font("Webdings", 9.25f), ClientRectangle, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            #endregion

            #region Fields

            private bool _isHovered;
            private bool _isPressed;

            #endregion

            #region Mouse Methods

            protected override void OnMouseDown(MouseEventArgs e) {
                if (e.Button == MouseButtons.Left) {
                    _isPressed = true;
                    Invalidate();
                }
                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e) {
                _isPressed = false;
                Invalidate();

                base.OnMouseUp(e);
            }

            protected override void OnMouseEnter(EventArgs e) {
                _isHovered = true;
                Invalidate();
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e) {
                _isHovered = false;
                Invalidate();

                base.OnMouseLeave(e);
            }

            #endregion
        }

        #endregion

        #region Shadows

        private bool _mAeroEnabled; // variables for box shadow
        private const int CsDropshadow = 0x00020000;
        private const int WmNcpaint = 0x0085;

        private const int WmNchittest = 0x84; // variables for dragging the form
        private const int Htclient = 0x1;
        private const int Htcaption = 0x2;

        protected override CreateParams CreateParams {
            get {
                _mAeroEnabled = CheckAeroEnabled();

                var cp = base.CreateParams;
                if (!_mAeroEnabled)
                    cp.ClassStyle |= CsDropshadow;

                return cp;
            }
        }

        private bool CheckAeroEnabled() {
            if (Environment.OSVersion.Version.Major >= 6) {
                var enabled = 0;
                DwmApi.DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1);
            }
            return false;
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