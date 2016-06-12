#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppInterfaceForm.cs) is part of 3P.
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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using _3PA.Interop;

namespace _3PA.MainFeatures.NppInterfaceForm {

    /// <summary>
    /// This is the base class for the tooltips and the autocomplete form
    /// </summary>
    internal class NppInterfaceForm : Form {

        #region fields
        /// <summary>
        /// Should be set when you create the new form
        /// CurrentForegroundWindow = WinApi.GetForegroundWindow();
        /// </summary>
        public IntPtr CurrentForegroundWindow;

        /// <summary>
        /// Set to true if scintilla should get the focus back, false if you want
        /// to use CurrentForegroundWindow
        /// </summary>
        public bool GiveFocusBackToScintilla = true;

        /// <summary>
        /// Sets the Opacity to give to the window when it's not focused
        /// </summary>
        public double UnfocusedOpacity;

        /// <summary>
        /// Sets the Opacity to give to the window when it's focused
        /// </summary>
        public double FocusedOpacity;

        private bool _allowInitialdisplay;
        private bool _focusAllowed;

        /// <summary>
        /// Use this to know if the form is currently activated
        /// </summary>
        public bool IsActivated;
        #endregion

        #region constructor
        /// <summary>
        /// Create a new npp interface form, please set CurrentForegroundWindow
        /// </summary>
        public NppInterfaceForm() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;

            // register to Npp
            FormIntegration.RegisterToNpp(Handle);

            CurrentForegroundWindow = Npp.HandleScintilla;

            Opacity = 0;
            Visible = false;
            Tag = false;
            Closing += OnClosing;

            Plug.OnNppWindowsMove += PlugOnOnNppWindowsMove;
        }

        protected override void Dispose(bool disposing) {
            Plug.OnNppWindowsMove -= PlugOnOnNppWindowsMove;
            base.Dispose(disposing);
        }

        #endregion

        #region public

        /// <summary>
        /// hides the form
        /// </summary>
        public void Cloack() {
            GiveFocusBack();
            Visible = false;
            // move this to an invisible part of the screen, otherwise we can see this window
            // if another window with Opacity <1 is in front Oo
            Location = new Point(-10000, 0);
        }

        /// <summary>
        /// show the form
        /// </summary>
        public void UnCloack() {
            _allowInitialdisplay = true;
            Opacity = UnfocusedOpacity;
            Visible = true;
            GiveFocusBack();
        }

        /// <summary>
        /// Call this method instead of Close() to really close this form
        /// </summary>
        public void ForceClose() {
            FormIntegration.UnRegisterToNpp(Handle);
            Tag = true;
            Close();
        }

        /// <summary>
        /// Gives focus back to the owner window
        /// </summary>
        public void GiveFocusBack() {
            if (GiveFocusBackToScintilla)
                Npp.GrabFocus();
            else
                WinApi.SetForegroundWindow(CurrentForegroundWindow);
            IsActivated = !IsActivated;
            Opacity = UnfocusedOpacity;
        }

        #endregion

        #region private methods

        /// <summary>
        /// instead of closing, cloak this form (invisible)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cancelEventArgs"></param>
        private void OnClosing(object sender, CancelEventArgs cancelEventArgs) {
            if ((bool) Tag) return;
            cancelEventArgs.Cancel = true;
            Cloack();
        }


        protected override void OnActivated(EventArgs e) {
            // Activate the window that previously had focus
            if (!_focusAllowed)
                if (GiveFocusBackToScintilla)
                    Npp.GrabFocus();
                else
                    WinApi.SetForegroundWindow(CurrentForegroundWindow);
            else {
                IsActivated = true;
                Opacity = FocusedOpacity;
            }
            base.OnActivated(e);
        }

        private void PlugOnOnNppWindowsMove() {
            Close();
        }

        protected override void OnShown(EventArgs e) {
            _focusAllowed = true;
            base.OnShown(e);
        }

        /// <summary>
        /// This ensures the form is never visible at start
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value) {
            base.SetVisibleCore(_allowInitialdisplay ? value : _allowInitialdisplay);
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = ThemeManager.Current.FormBack;
            var borderColor = ThemeManager.Current.FormBorder;
            var borderWidth = 2;

            e.Graphics.Clear(backColor);

            // draw the border with Style color
            var rect = new Rectangle(new Point(0, 0), new Size(Width, Height));
            var pen = new Pen(borderColor, borderWidth) { Alignment = PenAlignment.Inset };
            e.Graphics.DrawRectangle(pen, rect);
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
                WinApi.DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1);
            }
            return false;
        }

        protected override void WndProc(ref Message m) {

            if (m.Msg == WmNcpaint && _mAeroEnabled) {
                var v = 2;
                WinApi.DwmSetWindowAttribute(Handle, 2, ref v, 4);
                var margins = new WinApi.MARGINS(1, 1, 1, 1);
                WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
            }

            base.WndProc(ref m);

            if (m.Msg == WmNchittest && (int)m.Result == Htclient) // drag the form
                m.Result = (IntPtr)Htcaption;
        }

        #endregion
    }
}
