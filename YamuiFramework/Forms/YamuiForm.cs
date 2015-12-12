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
using System.Linq;
using System.Security;
using System.Web.UI.Design.WebControls;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {

    public class YamuiForm : Form {

        #region Fields
        /// <summary>
        /// difference between a main form and another form : 
        /// - go back navigation button
        /// </summary>
        [Category("Yamui")]
        public bool IsMainForm { get; set; }

        private bool _isMovable = true;

        [Category("Yamui")]
        public bool Movable {
            get { return _isMovable; }
            set { _isMovable = value; }
        }

        [Category("Yamui")]
        [DefaultValue(false)]
        public bool UseCustomBackColor { get; set; }

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
            get { return new Padding(40, 40, BorderWidth + 10, BorderWidth + 10); }
        }

        private bool _isResizable = true;

        [Category("Yamui")]
        public bool Resizable {
            get { return _isResizable; }
            set { _isResizable = value; }
        }

        /// <summary>
        /// is set to true when this form is the parent of a yamuimsgbox
        /// </summary>
        [Browsable(false)]
        public bool HasModalOpened { get; set; }

        private const int BorderWidth = 1;

        private List<int[]> _formHistory = new List<int[]>();
        private HtmlToolTip _mainFormToolTip = new HtmlToolTip();
        private YamuiGoBackButton _goBackButton;
        #endregion

        #region Constructor

        public YamuiForm() {
            // why those styles? check here: https://sites.google.com/site/craigandera/craigs-stuff/windows-forms/flicker-free-control-drawing
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            TransparencyKey = Color.Fuchsia;

            Shown += OnShown;
        }
        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = UseCustomBackColor ? BackColor : ThemeManager.Current.FormColorBackColor;
            var foreColor = ThemeManager.Current.FormColorForeColor;
            var borderColor = UseCustomBorderColor ? ForeColor : ThemeManager.AccentColor;

            e.Graphics.Clear(backColor);

            /*
            if (ThemeManager.ThemePageImage != null) {
                Rectangle imgRectangle = new Rectangle(ClientRectangle.Right - ThemeManager.ThemePageImage.Width, ClientRectangle.Height - ThemeManager.ThemePageImage.Height, ThemeManager.ThemePageImage.Width, ThemeManager.ThemePageImage.Height);
                e.Graphics.DrawImage(ThemeManager.ThemePageImage, imgRectangle, 0, 0, ThemeManager.ThemePageImage.Width, ThemeManager.ThemePageImage.Height, GraphicsUnit.Pixel);
            }*/

            /*
            // draw my logo
            ColorMap[] colorMap = new ColorMap[1];
            colorMap[0] = new ColorMap();
            colorMap[0].OldColor = Color.Black;
            colorMap[0].NewColor = ThemeManager.AccentColor;
            ImageAttributes attr = new ImageAttributes();
            attr.SetRemapTable(colorMap);
            Image logoImage = Properties.Resources.bull_ant;
            rect = new Rectangle(ClientRectangle.Right - (100 + logoImage.Width), 0 + 2, logoImage.Width, logoImage.Height);
            e.Graphics.DrawImage(logoImage, rect, 0, 0, logoImage.Width, logoImage.Height, GraphicsUnit.Pixel, attr);
            //e.Graphics.DrawImage(Properties.Resources.bull_ant, ClientRectangle.Right - (100 + Properties.Resources.bull_ant.Width), 0 + 5);
            */

            // draw the border with Style color
            var rect = new Rectangle(new Point(0, 0), new Size(Width - BorderWidth, Height - BorderWidth));
            var pen = new Pen(borderColor, BorderWidth);
            e.Graphics.DrawRectangle(pen, rect);

            // draw the resize pixel stuff on the bottom right
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
        /// <param name="pageName"></param>
        public void GoToPage(string pageName) {
            try {
                YamuiTabPage page = null;
                foreach (var control in ControlHelper.GetAll(this, typeof(YamuiTabPage))) {
                    if (control.Name == pageName)
                        page = (YamuiTabPage)control;
                }
                if (page == null) return;
                YamuiTabControl secControl = (YamuiTabControl)page.Parent;
                YamuiTabControl mainControl = (YamuiTabControl)secControl.Parent.Parent;
                GoToPage(mainControl, (YamuiTabPage)secControl.Parent, secControl, page, true);
            } catch (Exception) {
                // ignored
            }
        }

        public void GoToPage(int pageMainInt, int pageSecInt) {
            YamuiTabControl mainControl = (YamuiTabControl)ControlHelper.GetFirst(this, typeof(YamuiTabControl));
            if (mainControl == null || mainControl.TabCount < pageMainInt || mainControl.TabPages[pageMainInt] == null) return;
            YamuiTabControl secControl = (YamuiTabControl)ControlHelper.GetFirst(mainControl.TabPages[pageMainInt], typeof(YamuiTabControl));
            if (secControl == null) return;
            GoToPage(mainControl, (YamuiTabPage)mainControl.TabPages[pageMainInt], secControl, (YamuiTabPage)secControl.TabPages[pageSecInt], false);
        }

        public void GoToPage(YamuiTabControl tabMain, YamuiTabPage pageMain, YamuiTabControl tabSecondary, YamuiTabPage pageSecondary, bool histoSave) {

            // if we want to display a hidden page            
            if (pageMain.HiddenPage)
                pageMain.HiddenState = false;

            if (pageSecondary.HiddenPage)
                pageSecondary.HiddenState = false;

            var pageMainInt = tabMain.GetIndexOf(pageMain);
            var pageSecInt = tabSecondary.GetIndexOf(pageSecondary);

            if (histoSave)
                SaveCurrentPathInHistory();

            // if we change both pages, we can't do the animation for both!
            if (pageMainInt != tabMain.SelectIndex && pageSecInt != tabSecondary.SelectIndex) {
                var initState = ThemeManager.TabAnimationAllowed;
                ThemeManager.TabAnimationAllowed = false;
                tabSecondary.SelectIndex = pageSecInt;
                ThemeManager.TabAnimationAllowed = initState;
            } else if (pageSecInt != tabSecondary.SelectIndex)
                tabSecondary.SelectIndex = pageSecInt;

            if (pageMainInt != tabMain.SelectIndex)
                tabMain.SelectIndex = pageMainInt;
        }

        /// <summary>
        /// Use the history list to go back to previous tabs
        /// </summary>
        public void GoBack() {
            if (_formHistory.Count == 0) return;
            var lastPage = _formHistory.Last();
            GoToPage(lastPage[0], lastPage[1]);
            _formHistory.Remove(_formHistory.Last());
            if (_formHistory.Count == 0 && _goBackButton != null) {
                _goBackButton.FakeDisabled = true;
                _goBackButton.TabStop = false;
            }
        }

        /// <summary>
        /// Keep an history of the tabs visited through a list handled here
        /// </summary>
        public void SaveCurrentPathInHistory() {
            YamuiTabControl mainControl = GetMainTabControl();
            if (mainControl == null) return;
            var pageMainInt = mainControl.SelectedIndex;
            YamuiTabControl secControl = GetSelectSecondaryTabControl(mainControl);
            if (secControl == null) return;
            var pageSecInt = secControl.SelectedIndex;
            // save only if different from the previous 
            if (_formHistory.Count > 0) {
                var lastPage = _formHistory.Last();
                if (lastPage[0] == pageMainInt && lastPage[1] == pageSecInt) return;
            }
            _formHistory.Add(new[] { pageMainInt, pageSecInt });
            if (_goBackButton.FakeDisabled) {
                _goBackButton.FakeDisabled = false;
                _goBackButton.TabStop = true;
            }
        }

        private YamuiTabControl GetMainTabControl() {
            return (YamuiTabControl)ControlHelper.GetFirst(this, typeof(YamuiTabControl));
        }

        private YamuiTabControl GetSelectSecondaryTabControl(YamuiTabControl mainControl) {
            return (YamuiTabControl)ControlHelper.GetFirst(mainControl.TabPages[mainControl.SelectedIndex], typeof(YamuiTabControl));
        }

        /// <summary>
        /// allows to automatically generates the tabs/pages
        /// </summary>
        /// <param name="menuDescriber"></param>
        public void CreateContent(List<YamuiMainMenuTab> menuDescriber) {

            var mainTabControl = new YamuiTabControl {
                Dock = DockStyle.Fill,
                Function = TabFunction.Main
            };

            foreach (var yamuiMainMenuTab in menuDescriber) {
                CreateMainTab(yamuiMainMenuTab, mainTabControl);
            }

            Controls.Add(mainTabControl);
        }

        private void CreateMainTab(YamuiMainMenuTab mainTab, YamuiTabControl mainTabControl) {
            var mainTabPage = new YamuiTabPage {
                Text = mainTab.Name,
                Dock = DockStyle.Fill,
                Function = TabFunction.Main,
                HiddenPage = mainTab.Hidden,
                Name = mainTab.PageName,
                TabStop = true
            };

            var secTabControl = new YamuiTabControl {
                Dock = DockStyle.Fill,
                Function = TabFunction.Secondary,
                TabStop = true
            };

            foreach (var yamuiSecMenuTab in mainTab.SecTabs) {
                var secTabPage = new YamuiTabPage {
                    Dock = DockStyle.Fill,
                    Function = TabFunction.Secondary,
                    Text = yamuiSecMenuTab.Name,
                    Name = yamuiSecMenuTab.PageName,
                    Padding = new Padding(30, 25, 0, 0),
                    TabStop = true
                };

                yamuiSecMenuTab.Page.TabStop = true;
                yamuiSecMenuTab.Page.Dock = DockStyle.Fill;
                secTabPage.Controls.Add(yamuiSecMenuTab.Page);
                secTabControl.Controls.Add(secTabPage);
            }

            mainTabPage.Controls.Add(secTabControl);
            mainTabControl.Controls.Add(mainTabPage);
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

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (IsMainForm) {
                _goBackButton = new YamuiGoBackButton();
                Controls.Add(_goBackButton);
                _goBackButton.Location = new Point(8, Padding.Top + 6);
                _goBackButton.Size = new Size(27, 27);
                _goBackButton.TabStop = false;
            }

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

            // add the fonts to the html renderer
            //HtmlRender.AddFontFamily(GetFontFamily("SEGOEUI"));

            // animate the current tab
            if (IsMainForm)
                GetSelectSecondaryTabControl(GetMainTabControl()).TabAnimator();
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

        #region go back button
        private class YamuiGoBackButton : YamuiCharButton {
            public YamuiGoBackButton() {
                UseWingdings = true;
                ButtonChar = "ç";
                FakeDisabled = true;
                ButtonPressed += (sender, args) => {
                    if (!FakeDisabled)
                        TryToGoBack();
                };
                Focus();
            }

            private void TryToGoBack() {
                try {
                    YamuiForm ownerForm = (YamuiForm)FindForm();
                    if (ownerForm != null) ownerForm.GoBack();
                } catch (Exception) {
                    // ignored
                }
            }
        }
        #endregion

        #region Window Buttons

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

        #region Helper Methods

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
        #endregion

        private void InitializeComponent() {
            this._mainFormToolTip = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.SuspendLayout();
            // 
            // mainFormToolTip
            // 
            this._mainFormToolTip.AllowLinksHandling = true;
            this._mainFormToolTip.BaseStylesheet = null;
            this._mainFormToolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this._mainFormToolTip.OwnerDraw = true;
            this._mainFormToolTip.TooltipCssClass = "htmltooltip";
            // 
            // YamuiForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "YamuiForm";
            this.ResumeLayout(false);

        }
    }

    #region AutoBuild tabs

    public class YamuiMainMenuTab {
        public string Name { get; private set; }
        public string PageName { get; private set; }
        public bool Hidden { get; private set; }
        public List<YamuiSecMenuTab> SecTabs { get; private set; }

        public YamuiMainMenuTab(string name, string pageName, bool hidden, List<YamuiSecMenuTab> secTabs) {
            this.Name = name;
            Hidden = hidden;
            SecTabs = secTabs;
            PageName = pageName;
        }
    }

    public class YamuiSecMenuTab {
        public string Name { get; private set; }
        public string PageName { get; private set; }
        public YamuiPage Page { get; private set; }

        public YamuiSecMenuTab(string name, string pageName, YamuiPage page) {
            this.Name = name;
            Page = page;
            PageName = pageName;
        }
    }

    #endregion

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