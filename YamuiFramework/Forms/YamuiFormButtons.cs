#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormButtons.cs) is part of YamuiFramework.
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
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {
    /// <summary>
    /// Form class that adds the top right buttons + resize
    /// </summary>
    public class YamuiFormButtons : YamuiFormBaseFadeIn {
        #region constants

        protected const int FormButtonWidth = 25;
        protected const int ResizeIconSize = 14;

        #endregion

        #region Private

        /// <summary>
        /// Tooltip for close buttons
        /// </summary>
        private HtmlToolTip _mainFormToolTip = new HtmlToolTip();

        private Dictionary<WindowButtons, YamuiFormButton> _windowButtonList;
        private int _captionBarHeight = FormButtonWidth + BorderWidth;

        #endregion

        #region Properties

        /// <summary>
        /// Set this to true to show the "close all notifications button",
        /// to use with OnCloseAllVisible
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        public bool CloseAllBox { get; set; }

        /// <summary>
        /// To use with ShowCloseAllVisibleButton,
        /// Action to do when the user click the button
        /// </summary>
        [Browsable(false)]
        public Action OnCloseAllNotif { get; set; }

        /// <summary>
        /// Height of the caption bar (what should be the title bar in a normal window)
        /// the caption bar is sensitive to double click which maximize/reduce the window
        /// </summary>
        public int CaptionBarHeight {
            get { return _captionBarHeight; }
            set { _captionBarHeight = value; }
        }

        protected override Padding DefaultPadding {
            get { return new Padding(BorderWidth, 20, BorderWidth + ResizeIconSize, BorderWidth + ResizeIconSize); }
        }

        #endregion

        #region Enum

        private enum WindowButtons {
            Minimize,
            Maximize,
            Close,
            CloseAllVisible
        }

        #endregion

        #region Constructor

        public YamuiFormButtons() {
            _mainFormToolTip.ShowAlways = true;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            // draw the resize pixels icon on the bottom right
            var foreColor = YamuiThemeManager.Current.FormFore;
            if (Resizable) {
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

        #region WndProc

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            base.WndProc(ref m);

            switch (m.Msg) {
                case (int) WinApi.Messages.WM_GETMINMAXINFO:
                    // allows the window to be maximized at teh size of the working area instead of the whole screen size
                    OnGetMinMaxInfo(m.HWnd, m.LParam);
                    break;

                case (int) WinApi.Messages.WM_SIZE:
                    if (_windowButtonList != null) {
                        YamuiFormButton btn;
                        _windowButtonList.TryGetValue(WindowButtons.Maximize, out btn);
                        if (WindowState == FormWindowState.Normal && btn != null)
                            btn.Text = @"1";
                        if (WindowState == FormWindowState.Maximized && btn != null)
                            btn.Text = @"2";
                    }
                    break;
            }
        }

        [SecuritySafeCritical]
        private unsafe void OnGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            var pmmi = (WinApi.MINMAXINFO*) lParam;
            var s = Screen.FromHandle(hwnd);
            pmmi->ptMaxSize.x = s.WorkingArea.Width;
            pmmi->ptMaxSize.y = s.WorkingArea.Height;
            pmmi->ptMaxPosition.x = Math.Abs(s.WorkingArea.Left - s.Bounds.Left);
            pmmi->ptMaxPosition.y = Math.Abs(s.WorkingArea.Top - s.Bounds.Top);
        }

        // test in which part of the form the cursor is in (we return the caption bar if
        // it's on the top of the window or the resizebottomright when it's on the bottom right), 
        // allow to be able to maximize the window by double clicking the "title bar" for instance
        protected override WinApi.HitTest HitTestNca(IntPtr lparam) {
            var output = base.HitTestNca(lparam);
            if (output != WinApi.HitTest.HTCLIENT)
                return output;

            var vPoint = new Point((short) lparam, (short) ((int) lparam >> 16));
            if (RectangleToScreen(new Rectangle(0, 0, ClientRectangle.Width, CaptionBarHeight)).Contains(vPoint))
                return WinApi.HitTest.HTCAPTION;

            return WinApi.HitTest.HTCLIENT;
        }

        #endregion

        #region Events

        /// <summary>
        /// On load of the form
        /// </summary>
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
                if (CloseAllBox)
                    AddWindowButton(WindowButtons.CloseAllVisible);
                UpdateWindowButtonPosition();
            }
        }

        protected override void OnResizeEnd(EventArgs e) {
            base.OnResizeEnd(e);
            UpdateWindowButtonPosition();
        }

        #endregion

        #region Window Buttons

        /// <summary>
        /// Allows to remove the default caption bar
        /// </summary>
        [SecuritySafeCritical]
        public void RemoveCloseButton() {
            var hMenu = WinApi.GetSystemMenu(Handle, false);
            if (hMenu == IntPtr.Zero)
                return;

            var n = WinApi.GetMenuItemCount(hMenu);
            if (n <= 0)
                return;

            WinApi.RemoveMenu(hMenu, (uint) (n - 1), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.RemoveMenu(hMenu, (uint) (n - 2), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.DrawMenuBar(Handle);
        }

        /// <summary>
        /// Add a particular button on the right top of the form
        /// </summary>
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
                    newButton.Text = ((char) (126)).ToString();
                    _mainFormToolTip.SetToolTip(newButton, "<b>Close all</b> notification windows");
                    break;
            }

            newButton.Tag = button;
            newButton.Size = new Size(25, 20);
            newButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newButton.TabStop = false; //remove the form controls from the tab stop
            newButton.Click += OnWindowButtonClick;
            Controls.Add(newButton);

            _windowButtonList.Add(button, newButton);
        }

        /// <summary>
        /// Triggered when a button is clicked
        /// </summary>
        private void OnWindowButtonClick(object sender, EventArgs e) {
            var btn = sender as YamuiFormButton;
            if (btn == null) return;
            if (((MouseEventArgs) e).Button != MouseButtons.Left) return;
            var btnFlag = (WindowButtons) btn.Tag;
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
                    if (OnCloseAllNotif != null)
                        OnCloseAllNotif();
                    break;
            }
        }

        /// <summary>
        /// Update buttons position
        /// </summary>
        private void UpdateWindowButtonPosition() {
            if (!ControlBox) return;

            var priorityOrder = new Dictionary<int, WindowButtons>(3) {{0, WindowButtons.Close}, {1, WindowButtons.Maximize}, {2, WindowButtons.Minimize}};

            var buttonsWidth = 0;

            foreach (var button in priorityOrder.Where(button => _windowButtonList.ContainsKey(button.Value))) {
                buttonsWidth += _windowButtonList[button.Value].Width;
                _windowButtonList[button.Value].Location = new Point(ClientRectangle.Width - BorderWidth - buttonsWidth, BorderWidth);
            }

            if (_windowButtonList.ContainsKey(WindowButtons.CloseAllVisible)) {
                _windowButtonList[WindowButtons.CloseAllVisible].Location = new Point(ClientRectangle.Width - BorderWidth - 25, BorderWidth + 25);
            }

            Refresh();
        }

        public class YamuiFormButton : Label {
            #region Constructor

            public YamuiFormButton() {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);
            }

            #endregion

            #region Paint Methods

            protected override void OnPaint(PaintEventArgs e) {
                if (_isPressed)
                    e.Graphics.Clear(YamuiThemeManager.Current.AccentColor);
                else if (_isHovered)
                    e.Graphics.Clear(YamuiThemeManager.Current.ButtonHoverBack);
                else
                    e.Graphics.Clear(YamuiThemeManager.Current.FormBack);

                Color foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, false, false, _isHovered, _isPressed, Enabled);
                using (var font = new Font("Webdings", 9.25f)) {
                    TextRenderer.DrawText(e.Graphics, Text, font, ClientRectangle, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
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
    }
}