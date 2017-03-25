#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Windows.Forms;
using YamuiFramework.Forms;
using YamuiFramework.Helper;

namespace _3PA.NppCore.NppInterfaceForm {
    /// <summary>
    /// This is the base class for the tooltips and the autocomplete form
    /// </summary>
    internal class NppInterfaceForm : YamuiFormBase {
        #region constant

        private static readonly Point CloakedPosition = new Point(-10000, -10000);

        #endregion

        #region Private

        private bool _allowInitialdisplay;

        private Point _lastPosition = CloakedPosition;

        #endregion

        #region fields

        /// <summary>
        /// Should be set when you create the new form
        /// CurrentForegroundWindow = WinApi.GetForegroundWindow();
        /// </summary>
        public IntPtr CurrentForegroundWindow = IntPtr.Zero;

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

        /// <summary>
        /// Use this to know if the form is currently activated
        /// </summary>
        public bool IsActivated;

        public bool IsVisible { get; set; }

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

        /// <summary>
        /// Create a new npp interface form, please set CurrentForegroundWindow
        /// </summary>
        public NppInterfaceForm() {
            ShowInTaskbar = false;
            Movable = true;
            Opacity = 0;
            Visible = false;
            Tag = false;

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
        public void Cloak() {
            _lastPosition = Location;
            Visible = false;
            // move this to an invisible part of the screen, otherwise we can see this window
            // if another window with Opacity <1 is in front Oo
            Location = new Point(-10000, -10000);
            GiveFocusBack();
            IsVisible = false;
        }

        /// <summary>
        /// show the form
        /// </summary>
        public void UnCloak() {
            if (Location == CloakedPosition)
                Location = _lastPosition;
            _allowInitialdisplay = true;
            Opacity = UnfocusedOpacity;
            Visible = true;
            IsVisible = true;
        }

        /// <summary>
        /// Call this method instead of Close() to really close this form
        /// </summary>
        public void ForceClose() {
            Tag = true;
            Close();
        }

        /// <summary>
        /// Gives focus back to the owner window
        /// </summary>
        protected void GiveFocusBack() {
            if (GiveFocusBackToScintilla || CurrentForegroundWindow == IntPtr.Zero)
                Sci.GrabFocus();
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
        protected override void OnClosing(CancelEventArgs e) {
            if ((bool) Tag)
                return;
            e.Cancel = true;
            Cloak();
            base.OnClosing(e);
        }

        protected override void OnActivated(EventArgs e) {
            IsActivated = true;
            Opacity = FocusedOpacity;
            base.OnActivated(e);
        }

        private void PlugOnOnNppWindowsMove() {
            Cloak();
        }

        /// <summary>
        /// This ensures the form is never visible at start
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value) {
            base.SetVisibleCore(_allowInitialdisplay ? value : _allowInitialdisplay);
        }

        #endregion
    }
}