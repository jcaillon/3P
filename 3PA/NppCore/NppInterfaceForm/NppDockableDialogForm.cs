#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppDockableDialogForm.cs) is part of 3P.
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Helper;
using _3PA.MainFeatures;

namespace _3PA.NppCore.NppInterfaceForm {
    /// <summary>
    /// Okay so... what is the point of this class?
    /// Basically, if you directly feed a form (that you want to display as a dockable panel) to Npp, you will get screwed
    /// It handles form so poorly, it can cause npp to freeze when you play with it (docking it/undocking it, moving it...)
    /// 
    /// So, what we do is we tell Npp to use a dummy form with nothing on it and we let it manipulate this meanless form.
    /// Meanwhile, we hook to this empty form and we track its position/size to cover it with our own borderless window
    /// 
    /// Unfortunatly, we can't just subscribe to ClientSizeChanged and LocationChanged events of the master form
    /// because when we move the Npp window, the LocationChange event of the master form isn't triggered... Idk why...
    /// So instead, use a hook onto npp and we update the position/size each time we receive a message that npp has 
    /// moved
    /// </summary>
    internal class NppDockableDialogForm : Form {
        #region fields

        private Rectangle _masterRectangle;
        private Form _masterForm;
        private bool _forceClose;

        #endregion

        #region ShowWithoutActivation & Don't show in ATL+TAB

        /// <summary>
        /// This indicates that the form should not take focus when shown
        /// specify it through the CreateParams
        /// </summary>
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        /// <summary>
        /// Don't show in ATL+TAB
        /// </summary>
        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (int) WinApi.WindowStylesEx.WS_EX_TOOLWINDOW;
                return createParams;
            }
        }

        #endregion

        #region constructor

        public NppDockableDialogForm() {
            // why those styles? check here: 
            // https://sites.google.com/site/craigandera/craigs-stuff/windows-forms/flicker-free-control-drawing
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            AutoScaleMode = AutoScaleMode.None;
        }

        public NppDockableDialogForm(Form formToCover) : this() {
            _masterForm = formToCover;
            _masterForm.VisibleChanged += Cover_OnVisibleChanged;
            _masterForm.Closed += MasterFormOnClosed;
            _masterForm.ClientSizeChanged += RefreshPosAndLoc;
            _masterForm.LocationChanged += RefreshPosAndLoc;
            _masterForm.LostFocus += RefreshPosAndLoc;
            _masterForm.GotFocus += RefreshPosAndLoc;

            Location = _masterForm.PointToScreen(Point.Empty);
            ClientSize = _masterForm.ClientSize;

            Show(_masterForm);

            // Disable Aero transitions, the plexiglass gets too visible
            if (Environment.OSVersion.Version.Major >= 6) {
                int value = 1;
                WinApi.DwmSetWindowAttribute(Owner.Handle, WinApi.DwmwaTransitionsForcedisabled, ref value, 4);
            }

            // register to Npp
            Npp.RegisterToNpp(Handle);

            Plug.OnNppWindowsMove += RefreshPosAndLoc;
        }

        protected override void Dispose(bool disposing) {
            try {
                Plug.OnNppWindowsMove -= RefreshPosAndLoc;

                if (!Owner.IsDisposed && Environment.OSVersion.Version.Major >= 6) {
                    int value = 0;
                    WinApi.DwmSetWindowAttribute(Owner.Handle, WinApi.DwmwaTransitionsForcedisabled, ref value, 4);
                }
            } catch (Exception) {
                // ignored
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.Clear(ThemeManager.Current.FormBack);
        }

        #endregion

        #region RefreshPosAndLoc

        public void RefreshPosAndLoc() {
            if (Owner == null)
                return;

            var rect = WinApi.GetWindowRect(_masterForm.Handle);

            // update location
            if (_masterRectangle.Location != rect.Location) {
                Location = Owner.PointToScreen(Point.Empty);
            }

            // update size
            if (ClientSize != Owner.ClientSize) {
                ClientSize = Owner.ClientSize;
            }

            if (Visible != Owner.Visible) {
                Visible = Owner.Visible;
            }

            _masterRectangle = rect;
        }

        #endregion

        #region On event handlers

        private void RefreshPosAndLoc(object sender, EventArgs e) {
            RefreshPosAndLoc();
        }

        private void Cover_OnVisibleChanged(object sender, EventArgs eventArgs) {
            if (Owner == null)
                return;

            Visible = Owner.Visible;
            if (!Visible) {
                // get it out of the screen or it might be visible through low opacity... trust me
                Location = new Point(-10000, -10000);
            } else {
                Location = Owner.PointToScreen(Point.Empty);
                Refresh();
            }
        }

        private void MasterFormOnClosed(object sender, EventArgs eventArgs) {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e) {
            if (!_forceClose) {
                e.Cancel = true;
                return;
            }
            // register to Npp
            Npp.UnRegisterToNpp(Handle);
            base.OnClosing(e);
        }

        public void ForceClose() {
            _forceClose = true;
            Close();
        }

        #endregion
    }
}