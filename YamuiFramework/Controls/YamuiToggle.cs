#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiToggle.cs) is part of YamuiFramework.
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer("YamuiFramework.Controls.YamuiToggleDesigner")]
    [ToolboxBitmap(typeof(CheckBox))]

    public class YamuiToggle : CheckBox {
        #region Fields
        private bool _isHovered;
        private bool _isPressed;
        private bool _isFocused;
        #endregion

        #region Constructor
        public YamuiToggle() {
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
            Color textColor = YamuiThemeManager.ButtonColors.ForeGround(ForeColor, false, _isFocused, _isHovered, _isPressed, Enabled);
            Color foreColor = YamuiThemeManager.ButtonColors.ForeGround(ForeColor, false, _isFocused, _isHovered, Checked, Enabled);
            Color borderColor = YamuiThemeManager.ButtonColors.BorderColor(_isFocused, _isHovered, _isPressed, Enabled);
            Color unfilledColor = YamuiThemeManager.Current.ButtonColorsNormalBackColor;
            if (unfilledColor == YamuiThemeManager.Current.FormColorBackColor) unfilledColor = borderColor;
            Color fillColor = Checked ? YamuiThemeManager.AccentColor : unfilledColor;

            Rectangle textRect = new Rectangle(33, 0, Width - 42, Height);
            Rectangle backRect = new Rectangle(0, 0, 30, Height);

            // draw the back
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;           
            using (SolidBrush b = new SolidBrush(fillColor)) {
                e.Graphics.FillRectangle(b, new Rectangle(Height / 2, 0, backRect.Width - Height, Height));
                e.Graphics.FillEllipse(b, new Rectangle(0, 0, Height, Height));
                e.Graphics.FillEllipse(b, new Rectangle(backRect.Width - Height, 0, Height, Height));
            }
            // draw foreground ellipse
            using (SolidBrush b = new SolidBrush(foreColor)) {
                if (!Checked)
                    e.Graphics.FillEllipse(b, new Rectangle(2, 2, Height - 4, Height - 4));
                else
                    e.Graphics.FillEllipse(b, new Rectangle(backRect.Width - Height + 2, 2, Height - 4, Height - 4));
            }
            // draw checked.. or not
            if (Checked) {
                var fuRect = ClientRectangle;
                fuRect.Width = 15;
                fuRect.Offset(5, -3);
                TextRenderer.DrawText(e.Graphics, "a", new Font("Webdings", 15f, GraphicsUnit.Pixel), fuRect, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            e.Graphics.SmoothingMode = SmoothingMode.Default;

            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRect, textColor, FontManager.GetTextFormatFlags(TextAlign));
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

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnCheckedChanged(EventArgs e) {
            base.OnCheckedChanged(e);
            Invalidate();
        }

        public override Size GetPreferredSize(Size proposedSize) {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics()) {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, Text, FontManager.GetStandardFont(), proposedSize, FontManager.GetTextFormatFlags(TextAlign));
                preferredSize.Width += 42;
            }

            return preferredSize;
        }

        #endregion
    }

    internal class YamuiToggleDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");

            properties.Remove("UseCompatibleTextRendering");
            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");

            properties.Remove("UseVisualStyleBackColor");

            properties.Remove("Font");
            properties.Remove("RightToLeft");

            properties.Remove("ThreeState");

            base.PreFilterProperties(properties);
        }
    }
}
