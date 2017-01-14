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
    /// Form class that implements interesting utilities + shadow + onpaint + movable/resizable borderless
    /// </summary>
    public class YamuiFormBase : Form {

        #region Constants

        public const int BorderWidth = 2;
        public const int ResizeHitDetectionSize = 8;
        
        #endregion
        
        #region private fields

        private bool _reverseX;
        private bool _reverseY;
        private bool _isResizable = true;
        private bool _isMovable = true;

        #endregion
        
        #region Properties

        [Category("Yamui")]
        public bool Movable {
            get { return _isMovable; }
            set { _isMovable = value; }
        }
        
        [Category("Yamui")]
        public bool Resizable {
            get { return _isResizable; }
            set { _isResizable = value; }
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

                case (int)WinApi.Messages.WM_NCHITTEST:
                    // Allows to resize the form
                    if (Resizable) {
                        var ht = HitTestNca(m.LParam);
                        if (ht != WinApi.HitTest.HTCLIENT) {
                            m.Result = (IntPtr)ht;
                            return;
                        }
                    }
                    break;
            }

            base.WndProc(ref m);

        }

        /// <summary>
        /// test in which part of the form the cursor is in, it allows to resize a borderless window
        /// </summary>
        protected virtual WinApi.HitTest HitTestNca(IntPtr lparam) {
            var cursorLocation = new Point((short)lparam, (short)((int)lparam >> 16));

            // top left
            if (RectangleToScreen(new Rectangle(0, 0, ResizeHitDetectionSize, ResizeHitDetectionSize)).Contains(cursorLocation))
                return WinApi.HitTest.HTTOPLEFT;            
            
            // top
            if (RectangleToScreen(new Rectangle(ResizeHitDetectionSize, 0, ClientRectangle.Width - 2 * ResizeHitDetectionSize, ResizeHitDetectionSize)).Contains(cursorLocation))
                return WinApi.HitTest.HTTOP;
            
            // top right
            if (RectangleToScreen(new Rectangle(ClientRectangle.Width - ResizeHitDetectionSize, 0, ResizeHitDetectionSize, ResizeHitDetectionSize)).Contains(cursorLocation))
                return WinApi.HitTest.HTTOPRIGHT;
            
            // right
            if (RectangleToScreen(new Rectangle(ClientRectangle.Width - ResizeHitDetectionSize, ResizeHitDetectionSize, ResizeHitDetectionSize, ClientRectangle.Height - 2 * ResizeHitDetectionSize)).Contains(cursorLocation))
                return WinApi.HitTest.HTRIGHT;

            // bottom right
            if (RectangleToScreen(new Rectangle(ClientRectangle.Width - ResizeHitDetectionSize, ClientRectangle.Height - ResizeHitDetectionSize, ResizeHitDetectionSize, ResizeHitDetectionSize)).Contains(cursorLocation))
                return WinApi.HitTest.HTBOTTOMRIGHT;

            // bottom
            if (RectangleToScreen(new Rectangle(ResizeHitDetectionSize, ClientRectangle.Height - ResizeHitDetectionSize, ClientRectangle.Width - 2 * ResizeHitDetectionSize, ResizeHitDetectionSize)).Contains(cursorLocation))
                return WinApi.HitTest.HTBOTTOM;
            
            // bottom left
            if (RectangleToScreen(new Rectangle(0, ClientRectangle.Height - ResizeHitDetectionSize, ResizeHitDetectionSize, ResizeHitDetectionSize)).Contains(cursorLocation))
                return WinApi.HitTest.HTBOTTOMLEFT;

            // left
            if (RectangleToScreen(new Rectangle(0, ResizeHitDetectionSize, ResizeHitDetectionSize, ClientRectangle.Height - 2 * ResizeHitDetectionSize)).Contains(cursorLocation))
                return WinApi.HitTest.HTLEFT;
            
            return WinApi.HitTest.HTCLIENT;
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

        #endregion

        #region Methods

        /// <summary>
        /// Returns the best position (for the starting position of the form) given the spawn location
        /// (spawn location being the mouse location for a menu for instance)
        /// Example:
        /// If the spawn point if too far on the right of the screen, the location returned will spawn the menu
        /// on the left of the spawn point
        /// </summary>
        public Point GetBestMenuPosition(Point spawnLocation) {
            var screen = Screen.FromPoint(spawnLocation);
            if (spawnLocation.X > screen.WorkingArea.X + screen.WorkingArea.Width/2) {
                spawnLocation.X = spawnLocation.X - Width;
                _reverseX = true;
            } else
                _reverseX = false;
            if (spawnLocation.Y > screen.WorkingArea.Y + screen.WorkingArea.Height / 2) {
                spawnLocation.Y = spawnLocation.Y - Height;
                _reverseY = true;
            } else
                _reverseY = false;
            return spawnLocation;
        }

        /// <summary>
        /// Returns the best position (for the starting position of the form) given the spawn location
        /// (spawn location being the mouse location for a menu for instance) for an autocompletion form
        /// </summary>
        public Point GetBestAutocompPosition(Point spawnLocation, int lineHeight) {
            var screen = Screen.FromPoint(spawnLocation);
            // position the window smartly
            if (spawnLocation.X + Width > screen.WorkingArea.X + screen.WorkingArea.Width) {
                spawnLocation.X -= (spawnLocation.X + Width) - (screen.WorkingArea.X + screen.WorkingArea.Width);
                _reverseX = true;
            } else
                _reverseX = false;
            if (spawnLocation.Y + Height > screen.WorkingArea.Y + screen.WorkingArea.Height) {
                spawnLocation.Y = spawnLocation.Y - Height - lineHeight;
                _reverseY = true;
            } else
                _reverseY = false;
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

        /// <summary>
        /// Returns the location that should be used for a tooltip window relative to this window
        /// </summary>
        public Point GetToolTipBestPosition(Size childSize) {
            return new Point(
                _reverseX ? Location.X - childSize.Width : Location.X + Width,
                _reverseY ? Location.Y + Height - childSize.Height : Location.Y);
        }

        /// <summary>
        /// Resizes the form so that it doesn't go out of screen
        /// </summary>
        public void ResizeFormToFitScreen() {
            var loc = Location;
            loc.Offset(Width / 2, Height / 2);
            var screen = Screen.FromPoint(loc);
            if (Location.X < screen.WorkingArea.X) {
                var rightPos = Location.X + Width;
                Location = new Point(screen.WorkingArea.X, Location.Y);
                Width = rightPos - Location.X;
            }
            if (Location.X + Width > screen.WorkingArea.X + screen.WorkingArea.Width) {
                Width -= (Location.X + Width) - (screen.WorkingArea.X + screen.WorkingArea.Width);
            }

            if (Location.Y < screen.WorkingArea.Y) {
                var bottomPos = Location.Y + Height;
                Location = new Point(Location.X, screen.WorkingArea.Y);
                Height = bottomPos - Location.Y;
            }
            if (Location.Y + Height > screen.WorkingArea.Y + screen.WorkingArea.Height) {
                Height -= (Location.Y + Height) - (screen.WorkingArea.Y + screen.WorkingArea.Height);
            }
        }

        #endregion

        #region KeyDown helper

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            return e.Handled;
        }

        #endregion
        
    }
}
