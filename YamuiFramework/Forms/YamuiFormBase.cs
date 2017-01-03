#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiFormBase.cs) is part of YamuiFramework.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms {
    
    /// <summary>
    /// Form class that implements interesting utilities + shadow + onpaint + movable borderless
    /// </summary>
    public class YamuiFormBase : Form {

        #region Constants

        public const int BorderWidth = 2;
        
        // to allow the form to be moved
        protected const int Htclient = 0x1;
        protected const int Htcaption = 0x2;

        #endregion
        
        #region private fields

        private bool _reverseX;
        private bool _reverseY;
        private bool _isMovable = true;

        #endregion
        
        #region Properties

        [Category("Yamui")]
        public bool Movable {
            get { return _isMovable; }
            set { _isMovable = value; }
        }

        #endregion
        
        #region constructor

        public YamuiFormBase() {

            // why those styles? check here: 
            // https://sites.google.com/site/craigandera/craigs-stuff/windows-forms/flicker-free-control-drawing
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            FormBorderStyle = FormBorderStyle.None;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the best position (for the starting position of the form) given the spawn location
        /// (spawn location being the mouse location for a menu for instance)
        /// Example:
        /// If the spawn point if too far on the right of the screen, the location returned will spawn the menu
        /// on the left of the spawn point
        /// </summary>
        public Point GetBestPosition(Point spawnLocation) {
            var screen = Screen.FromPoint(spawnLocation);
            if (spawnLocation.X > screen.WorkingArea.X + screen.WorkingArea.Width/2) {
                spawnLocation.X = spawnLocation.X - Width;
                _reverseX = true;
            }
            if (spawnLocation.Y > screen.WorkingArea.Y + screen.WorkingArea.Height/2) {
                spawnLocation.Y = spawnLocation.Y - Height;
                _reverseY = true;
            }
            return spawnLocation;
        }

        /// <summary>
        /// Returns the location that should be used for a window child relative to this window
        /// the location of the childRectangle should be the "default" position of the child menu
        /// (i.e. somewhere on the right of this form and between this.XY and this.Y + this.Height
        /// </summary>
        public Point GetChildBestPosition(Rectangle childRectangle, int parentLineHeight) {
            return new Point(
                _reverseX ? (childRectangle.X - childRectangle.Width - Width) : childRectangle.X,
                _reverseY ? (childRectangle.Y - (Height - 2) + parentLineHeight) : childRectangle.Y);
        }

        #endregion

        #region WndProc
        
        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg) {
                case (int)WinApi.Messages.WM_SYSCOMMAND:
                    var sc = m.WParam.ToInt32() & 0xFFF0;
                    switch (sc) {
                        // prevent the window from moving
                        case (int)WinApi.Messages.SC_MOVE:
                            if (!Movable) 
                                return;
                            break;
                    }
                    break;
            }

            base.WndProc(ref m);

        }

        #endregion

        #region Events

        /// <summary>
        /// override to make a borderless window movable
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e) {
            if (Movable && e.Button == MouseButtons.Left) {
                if (WindowState == FormWindowState.Maximized) 
                    return;
                
                // do as if the cursor was on the title bar
                WinApi.ReleaseCapture();
                WinApi.SendMessage(Handle, (uint)WinApi.Messages.WM_NCLBUTTONDOWN, new IntPtr((int)WinApi.HitTest.HTCAPTION), new IntPtr(0));
            }
            base.OnMouseDown(e);
        }

        #region KeyDown helper

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            return e.Handled;
        }

        #endregion


        #endregion

        #region OnPaint

        protected override void OnPaint(PaintEventArgs e) {

            var backColor = YamuiThemeManager.Current.FormBack;
            var borderColor = YamuiThemeManager.Current.FormBorder;

            e.Graphics.Clear(backColor);

            // draw the border with Style color
            var rect = new Rectangle(new Point(0, 0), new Size(Width, Height));
            var pen = new Pen(borderColor, BorderWidth) { Alignment = PenAlignment.Inset };
            e.Graphics.DrawRectangle(pen, rect);

        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        #endregion
        
    }
}
