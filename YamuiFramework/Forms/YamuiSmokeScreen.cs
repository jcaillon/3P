#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiSmokeScreen.cs) is part of YamuiFramework.
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
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Helper;

namespace YamuiFramework.Forms {
    public class YamuiSmokeScreen : Form {


        #region Constructor
        public YamuiSmokeScreen(Form tocover) {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            BackColor = Color.Black;
            Opacity = 0.5;
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            AutoScaleMode = AutoScaleMode.None;
            Location = tocover.PointToScreen(Point.Empty);
            ClientSize = tocover.ClientSize;
            tocover.LocationChanged += Cover_LocationChanged;
            tocover.ClientSizeChanged += Cover_ClientSizeChanged;
            tocover.VisibleChanged += Cover_OnVisibleChanged;
            Show(tocover);
            tocover.Focus();
            // Disable Aero transitions, the plexiglass gets too visible
            if (Environment.OSVersion.Version.Major >= 6) {
                int value = 1;
                DwmApi.DwmSetWindowAttribute(Owner.Handle, DwmApi.DwmwaTransitionsForcedisabled, ref value, 4);
            }
        }

        #endregion

        #region Events
        private void Cover_OnVisibleChanged(object sender, EventArgs eventArgs) {
            Visible = Owner.Visible;
        }

        private void Cover_LocationChanged(object sender, EventArgs e) {
            // Ensure the plexiglass follows the owner
            Location = Owner.PointToScreen(Point.Empty);
        }

        private void Cover_ClientSizeChanged(object sender, EventArgs e) {
            // Ensure the plexiglass keeps the owner covered
            ClientSize = Owner.ClientSize;
        }

        protected override void Dispose(bool disposing) {
            Owner.LocationChanged -= Cover_LocationChanged;
            Owner.ClientSizeChanged -= Cover_ClientSizeChanged;
            Owner.VisibleChanged -= Cover_OnVisibleChanged;
            if (!Owner.IsDisposed && Environment.OSVersion.Version.Major >= 6) {
                int value = 0;
                DwmApi.DwmSetWindowAttribute(Owner.Handle, DwmApi.DwmwaTransitionsForcedisabled, ref value, 4);
            }
            base.Dispose(disposing);
        }

        protected override void OnActivated(EventArgs e) {
            // Always keep the owner activated instead
            BeginInvoke(new Action(() => Owner.Activate()));
        }
        #endregion
    }
}
