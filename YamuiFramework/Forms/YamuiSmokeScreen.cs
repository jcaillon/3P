#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiSmokeScreen.cs) is part of YamuiFramework.
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
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {
    public class YamuiSmokeScreen : Form {
        #region fields

        private Rectangle _pageRectangle;
        private Point _sizeDifference;

        /// <summary>
        /// Opacity, scaled on the owner's opacity
        /// </summary>
        public new double Opacity {
            get { return base.Opacity; }
            set {
                if (Owner != null)
                    base.Opacity = value*Owner.Opacity;
            }
        }

        /// <summary>
        /// Allows to hide or show the form
        /// </summary>
        public bool GoHide {
            set {
                if (value) {
                    Opacity = 0;
                    Location = new Point(-100000, -100000);
                } else {
                    Opacity = 1d;
                    if (Owner != null)
                        Location = Owner.PointToScreen(_pageRectangle.Location);
                }
            }
        }

        /// <summary>
        /// Set to true if you want to use the BackColor property, otherwise the color is the form back color
        /// </summary>
        public bool UseCustomBackColor { get; set; }

        #endregion

        #region Constructor

        public YamuiSmokeScreen(Form owner, Rectangle pageRectangle) {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            _pageRectangle = pageRectangle;
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Location = owner.PointToScreen(_pageRectangle.Location);
            _sizeDifference = new Point(owner.Width - _pageRectangle.Width, owner.Height - _pageRectangle.Height);
            ClientSize = new Size(owner.Width - _sizeDifference.X, owner.Height - _sizeDifference.Y);
            owner.LocationChanged += Cover_LocationChanged;
            owner.ClientSizeChanged += Cover_ClientSizeChanged;
            owner.VisibleChanged += Cover_OnVisibleChanged;

            // Disable Aero transitions, the plexiglass gets too visible
            if (Environment.OSVersion.Version.Major >= 6) {
                int value = 1;
                WinApi.DwmSetWindowAttribute(owner.Handle, WinApi.DwmwaTransitionsForcedisabled, ref value, 4);
            }

            base.Opacity = 0d;
            Show(owner);
            owner.Focus();
        }

        #endregion

        #region Events

        private void Cover_OnVisibleChanged(object sender, EventArgs eventArgs) {
            Visible = Owner.Visible;
        }

        private void Cover_LocationChanged(object sender, EventArgs e) {
            Location = Owner.PointToScreen(_pageRectangle.Location);
        }

        private void Cover_ClientSizeChanged(object sender, EventArgs e) {
            ClientSize = new Size(Owner.Width - _sizeDifference.X, Owner.Height - _sizeDifference.Y);
        }

        protected override void Dispose(bool disposing) {
            try {
                if (Owner != null) {
                    Owner.LocationChanged -= Cover_LocationChanged;
                    Owner.ClientSizeChanged -= Cover_ClientSizeChanged;
                    Owner.VisibleChanged -= Cover_OnVisibleChanged;
                    if (!Owner.IsDisposed && Environment.OSVersion.Version.Major >= 6) {
                        int value = 0;
                        WinApi.DwmSetWindowAttribute(Owner.Handle, WinApi.DwmwaTransitionsForcedisabled, ref value, 4);
                    }
                }
            } catch (Exception) {
                // ignored
            }
            base.Dispose(disposing);
        }

        protected override void OnActivated(EventArgs e) {
            BeginInvoke(new Action(() => Owner.Activate()));
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.Clear(UseCustomBackColor ? BackColor : YamuiThemeManager.Current.FormBack);
        }
    }
}