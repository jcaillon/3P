#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiTabAnimation.cs) is part of YamuiFramework.
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
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {
    /// <summary>
    /// Basically, this handles the animation of a tabpage by drawing the same background of the tabpage in
    /// custom form, we display it at max opacity and then reduce opacity with the transition class
    /// to effectively create a fade in animation
    /// </summary>
    public class YamuiTabAnimation : Form {

        private static YamuiTabPage _pageToFadeIn;

        // So.. when we set the opacity to 0, this resizes to 0,0 to be "invisible" for the user
        // this is done to avoid having to create this form each time we want to display the animation
        public new double Opacity {
            get { return base.Opacity; }
            set {
                base.Opacity = value;
                if (_pageToFadeIn != null)
                ClientSize = value <= 0d ? new Size(0, 0) : _pageToFadeIn.ClientSize;
            }
        }

        #region Constructor
        public YamuiTabAnimation(Form tocover, YamuiTabPage pageToFadeIn) {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            _pageToFadeIn = pageToFadeIn;
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            AutoScaleMode = AutoScaleMode.None;
            Location = _pageToFadeIn.PointToScreen(Point.Empty);
            ClientSize = _pageToFadeIn.ClientSize;
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

        #region Paint
        /// <summary>
        /// This is the same paint method as a yamuitabpage!!
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintBackground(PaintEventArgs e) {}

        protected override void OnPaint(PaintEventArgs e) {
            Color backColor = ThemeManager.Current.TabsColorsNormalBackColor;
            e.Graphics.Clear(backColor);
            var img = ThemeManager.ThemePageImage;
            if (img != null) {
                Rectangle rect = new Rectangle(ClientRectangle.Right - img.Width, ClientRectangle.Height - img.Height, img.Width, img.Height);
                e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            }
        }
        #endregion

        #region Events
        private void Cover_OnVisibleChanged(object sender, EventArgs eventArgs) {
            Visible = Owner.Visible;
        }

        private void Cover_LocationChanged(object sender, EventArgs e) {
            // Ensure the plexiglass follows the owner
            Location = _pageToFadeIn.PointToScreen(Point.Empty);
        }

        private void Cover_ClientSizeChanged(object sender, EventArgs e) {
            // Ensure the plexiglass keeps the owner covered
            ClientSize = _pageToFadeIn.ClientSize;
        }

        protected override void Dispose(bool disposing) {
            if (Owner != null) {
                Owner.LocationChanged -= Cover_LocationChanged;
                Owner.ClientSizeChanged -= Cover_ClientSizeChanged;
                Owner.VisibleChanged -= Cover_OnVisibleChanged;
                if (!Owner.IsDisposed && Environment.OSVersion.Version.Major >= 6) {
                    int value = 0;
                    DwmApi.DwmSetWindowAttribute(Owner.Handle, DwmApi.DwmwaTransitionsForcedisabled, ref value, 4);
                }
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
