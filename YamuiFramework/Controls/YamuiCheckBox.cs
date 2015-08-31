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
    [Designer("YamuiFramework.Controls.YamuiCheckBoxDesigner")]
    [ToolboxBitmap(typeof(CheckBox))]

    public class YamuiCheckBox : CheckBox {
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

        public YamuiCheckBox() {
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
            Color borderColor = ThemeManager.ButtonColors.BorderColor(_isFocused, _isHovered, _isPressed, Enabled);
            Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, UseCustomForeColor, _isFocused, _isHovered, _isPressed, Enabled);
            Color backColor = ThemeManager.ButtonColors.BackGround(BackColor, UseCustomBackColor, _isFocused, _isHovered, _isPressed, Enabled);

            var backRect = new Rectangle(0, Height / 2 - 6, 12, 12);

            // Paint the back + border of the checkbox
            using (SolidBrush b = new SolidBrush(backColor)) {
                e.Graphics.FillRectangle(b, backRect);
            }

            if (borderColor != Color.Transparent)
                using (Pen p = new Pen(borderColor)) {
                    e.Graphics.DrawRectangle(p, backRect);
                }

            // paint the form inside
            if (Checked) {
                if (CheckState != CheckState.Indeterminate) {
                    //using (Pen p = new Pen(ThemeManager.AccentColor, 2)) {
                    //    e.Graphics.DrawLines(p, new[] { new Point(2, Height / 2 - 1), new Point(6, Height / 2 + 3), new Point(10, Height / 2 - 4) });
                    //}
                    var fuRect = ClientRectangle;
                    fuRect.Width = 15;
                    fuRect.Offset(0, -3);
                    TextRenderer.DrawText(e.Graphics, "a", new Font("Webdings", 15f, GraphicsUnit.Pixel), fuRect, ThemeManager.AccentColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                } else {
                    using (SolidBrush b = new SolidBrush(ThemeManager.AccentColor)) {
                        Rectangle boxRect = new Rectangle(4, Height / 2 - 2, 5, 5);
                        e.Graphics.FillRectangle(b, boxRect);
                    }
                }

            }

            Rectangle textRect = new Rectangle(16, 0, Width - 16, Height);
            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRect, foreColor, FontManager.GetTextFormatFlags(TextAlign));
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
                preferredSize.Width += 16;
            }

            return preferredSize;
        }

        #endregion
    }

    internal class YamuiCheckBoxDesigner : ControlDesigner {
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

            base.PreFilterProperties(properties);
        }
    }
}
