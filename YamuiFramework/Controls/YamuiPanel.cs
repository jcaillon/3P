#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiPanel.cs) is part of YamuiFramework.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [ToolboxBitmap(typeof(Panel))]
    public class YamuiPanel : Panel {
        #region Fields

        private YamuiScrollBar _verticalScrollbar = new YamuiScrollBar(ScrollOrientation.Vertical);
        private YamuiScrollBar _horizontalScrollbar = new YamuiScrollBar(ScrollOrientation.Horizontal);

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        private bool _showHorizontalScrollbar;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool HorizontalScrollbar {
            get { return _showHorizontalScrollbar; }
            set { _showHorizontalScrollbar = value; }
        }

        [Category("Yamui")]
        public int HorizontalScrollbarSize {
            get { return _horizontalScrollbar.ScrollbarSize; }
            set { _horizontalScrollbar.ScrollbarSize = value; }
        }

        [Category("Yamui")]
        public bool HorizontalScrollbarHighlightOnWheel {
            get { return _horizontalScrollbar.HighlightOnWheel; }
            set { _horizontalScrollbar.HighlightOnWheel = value; }
        }

        private bool _showVerticalScrollbar;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool VerticalScrollbar {
            get { return _showVerticalScrollbar; }
            set { _showVerticalScrollbar = value; }
        }

        [Category("Yamui")]
        public int VerticalScrollbarSize {
            get { return _verticalScrollbar.ScrollbarSize; }
            set { _verticalScrollbar.ScrollbarSize = value; }
        }

        [Category("Yamui")]
        public bool VerticalScrollbarHighlightOnWheel {
            get { return _verticalScrollbar.HighlightOnWheel; }
            set { _verticalScrollbar.HighlightOnWheel = value; }
        }

        [Category("Yamui")]
        public new bool AutoScroll {
            get {
                return base.AutoScroll;
            }
            set {
                _showHorizontalScrollbar = value;
                _showVerticalScrollbar = value;

                base.AutoScroll = value;
            }
        }

        [Category("Yamui")]
        [DefaultValue(false)]
        public bool DontUseTransparentBackGround { get; set; }

        #endregion

        #region Constructor

        public YamuiPanel() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            Controls.Add(_verticalScrollbar);
            Controls.Add(_horizontalScrollbar);

            _verticalScrollbar.Visible = false;
            _horizontalScrollbar.Visible = false;

            _verticalScrollbar.Scroll += VerticalScrollbarScroll;
            _horizontalScrollbar.Scroll += HorizontalScrollbarScroll;
        }

        #endregion

        #region Paint
        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            graphics.Clear(Color.Transparent);
            if ((Parent != null)) {
                clipRect.Offset(Location);
                PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                GraphicsState state = graphics.Save();
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                try {
                    graphics.TranslateTransform(-Location.X, -Location.Y);
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                } finally {
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected void CustomOnPaintBackground(PaintEventArgs e) {
            if (!UseCustomBackColor && !DontUseTransparentBackGround)
                PaintTransparentBackground(e.Graphics, DisplayRectangle);
            else
                if (!UseCustomBackColor)
                    e.Graphics.Clear(ThemeManager.Current.FormColorBackColor);
                else
                    e.Graphics.Clear(BackColor);
        }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                CustomOnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {
            if (DesignMode) {
                _horizontalScrollbar.Visible = false;
                _verticalScrollbar.Visible = false;
                return;
            }

            UpdateScrollBarPositions();

            if (HorizontalScrollbar) {
                _horizontalScrollbar.Visible = HorizontalScroll.Visible;
            }
            if (HorizontalScroll.Visible) {
                _horizontalScrollbar.Minimum = HorizontalScroll.Minimum;
                _horizontalScrollbar.Maximum = HorizontalScroll.Maximum;
                _horizontalScrollbar.SmallChange = HorizontalScroll.SmallChange;
                _horizontalScrollbar.LargeChange = HorizontalScroll.LargeChange;
            }

            if (VerticalScrollbar) {
                _verticalScrollbar.Visible = VerticalScroll.Visible;
            }
            if (VerticalScroll.Visible) {
                _verticalScrollbar.Minimum = VerticalScroll.Minimum;
                _verticalScrollbar.Maximum = VerticalScroll.Maximum;
                _verticalScrollbar.SmallChange = VerticalScroll.SmallChange;
                _verticalScrollbar.LargeChange = VerticalScroll.LargeChange;
            }
        }
        #endregion

        #region Scroll Events

        private void HorizontalScrollbarScroll(object sender, ScrollEventArgs e) {
            AutoScrollPosition = new Point(e.NewValue, _verticalScrollbar.Value);
            UpdateScrollBarPositions();
        }

        private void VerticalScrollbarScroll(object sender, ScrollEventArgs e) {
            AutoScrollPosition = new Point(_horizontalScrollbar.Value, e.NewValue);
            UpdateScrollBarPositions();
        }

        #endregion

        #region Overridden Methods
        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);

            _verticalScrollbar.Value = Math.Abs(VerticalScroll.Value);
            _horizontalScrollbar.Value = Math.Abs(HorizontalScroll.Value);
        }

        [SecuritySafeCritical]
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);

            if (!DesignMode && AutoScroll) {
                WinApi.ShowScrollBar(Handle, (int)WinApi.ScrollBar.SB_BOTH, 0);
            }
        }

        #endregion

        #region Management Methods

        private void UpdateScrollBarPositions() {
            if (DesignMode) {
                return;
            }

            if (!AutoScroll) {
                _verticalScrollbar.Visible = false;
                _horizontalScrollbar.Visible = false;
                return;
            }

            _verticalScrollbar.Location = new Point(ClientRectangle.Width - _verticalScrollbar.Width, ClientRectangle.Y);
            _verticalScrollbar.Height = ClientRectangle.Height - _horizontalScrollbar.Height;

            if (!VerticalScrollbar) {
                _verticalScrollbar.Visible = false;
            }

            _horizontalScrollbar.Location = new Point(ClientRectangle.X, ClientRectangle.Height - _horizontalScrollbar.Height);
            _horizontalScrollbar.Width = ClientRectangle.Width - _verticalScrollbar.Width;

            if (!HorizontalScrollbar) {
                _horizontalScrollbar.Visible = false;
            }
        }

        #endregion
    }
}
