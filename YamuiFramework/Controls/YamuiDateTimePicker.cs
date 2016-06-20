#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiDateTime.cs) is part of YamuiFramework.
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
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [ToolboxBitmap(typeof(DateTimePicker))]
    public class YamuiDateTimePicker : DateTimePicker {

        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        private bool _isHovered;
        private bool _isPressed;
        private bool _isFocused;

        #endregion

        #region Constructor
        public YamuiDateTimePicker() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint, true);
        }
        #endregion

        #region Paint Methods
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
            try {
                Color backColor = YamuiThemeManager.Current.ButtonBg(BackColor, UseCustomBackColor, _isFocused, _isHovered, _isPressed, Enabled);
                if (backColor != Color.Transparent)
                    e.Graphics.Clear(backColor);
                else
                    PaintTransparentBackground(e.Graphics, DisplayRectangle);
            } catch {
                Invalidate();
            }
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
            Color borderColor = YamuiThemeManager.Current.ButtonBorder(_isFocused, _isHovered, _isPressed, Enabled);
            Color foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, UseCustomForeColor, _isFocused, _isHovered, _isPressed, Enabled);

            if (borderColor != Color.Transparent)
                using (Pen p = new Pen(borderColor)) {
                    Rectangle borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
                    e.Graphics.DrawRectangle(p, borderRect);
                }

            using (SolidBrush b = new SolidBrush(foreColor)) {
                e.Graphics.FillPolygon(b, new[] { new Point(Width - 20, (Height / 2) - 2), new Point(Width - 9, (Height / 2) - 2), new Point(Width - 15, (Height / 2) + 4) });
                //e.Graphics.FillPolygon(b, new Point[] { new Point(Width - 15, (Height / 2) - 5), new Point(Width - 21, (Height / 2) + 2), new Point(Width - 9, (Height / 2) + 2) });
            }

            int check = 0;

            if (ShowCheckBox) {
                check = 15;
                using (Pen p = new Pen(borderColor)) {
                    Rectangle boxRect = new Rectangle(3, Height / 2 - 6, 12, 12);
                    e.Graphics.DrawRectangle(p, boxRect);
                }
                if (Checked) {
                    Color fillColor = YamuiThemeManager.Current.AccentColor;
                    using (SolidBrush b = new SolidBrush(fillColor)) {
                        Rectangle boxRect = new Rectangle(4, Height / 2 - 2, 5, 5);
                        e.Graphics.FillRectangle(b, boxRect);
                    }
                } else {
                    foreColor = YamuiThemeManager.Current.ButtonWatermarkFore;
                }
            }

            Rectangle textRect = new Rectangle(2 + check, 2, Width - 20, Height - 4);

            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRect, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        protected override void OnValueChanged(EventArgs eventargs) {
            base.OnValueChanged(eventargs);
            Invalidate();
        }

        #endregion

        #region Managing isHovered, isPressed, isFocused

        #region Focus Methods

        protected override void OnGotFocus(EventArgs e) {
            _isFocused = true;
            Invalidate();

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e) {
            _isFocused = false;
            Invalidate();

            base.OnLostFocus(e);
        }

        protected override void OnEnter(EventArgs e) {
            _isFocused = true;
            Invalidate();

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            _isFocused = false;
            Invalidate();

            base.OnLeave(e);
        }

        #endregion

        #region Keyboard Methods

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyCode == Keys.Space) {
                _isPressed = true;
                Invalidate();
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            //Remove this code cause this prevents the focus color
            _isPressed = false;
            Invalidate();
            base.OnKeyUp(e);
        }

        #endregion

        #region Mouse Methods

        protected override void OnMouseEnter(EventArgs e) {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

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

        protected override void OnMouseLeave(EventArgs e) {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        #endregion

        #endregion

        #region Overridden Methods

        public override Size GetPreferredSize(Size proposedSize) {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics()) {
                string measureText = Text.Length > 0 ? Text : "MeasureText";
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, measureText, FontManager.GetStandardFont(), proposedSize, TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.VerticalCenter);
                preferredSize.Height += 10;
            }

            return preferredSize;
        }
        #endregion
    }
}
