#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiColorRadioButton.cs) is part of YamuiFramework.
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
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer("YamuiFramework.Controls.YamuiColorRadioButtonDesigner")]
    [ToolboxBitmap(typeof(RadioButton))]
    public class YamuiColorRadioButton : RadioButton {
        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseBorder { get; set; }

        private bool _isHovered;
        //private bool _isPressed;
        //private bool _isFocused;
        #endregion

        #region Constructor
        public YamuiColorRadioButton() {
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

            if (!Checked && _isHovered)
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.ButtonColorsHoverBackColor))
                    e.Graphics.FillRectangle(b, new Rectangle(0, 0, Width, Height));

            Rectangle boxRect = (!Checked) ? new Rectangle(5, 5, Width - 10, Height - 10) : new Rectangle(0, 0, Width, Height);

            if (UseBorder) {
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.AccentColor))
                    e.Graphics.FillRectangle(b, boxRect);
                boxRect = (!Checked) ? new Rectangle(10, 10, Width - 20, Height - 20) : new Rectangle(5, 5, Width - 10, Height - 10);
            }

            // Paint the back + border of the checkbox
            using (SolidBrush b = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(b, boxRect);
        }
        #endregion

        #region Managing isHovered, isPressed, isFocused

        #region Mouse Methods

        protected override void OnMouseEnter(EventArgs e) {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
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
            AutoSize = false;
            return new Size(50, 50);
        }

        #endregion
    }

    internal class YamuiColorRadioButtonDesigner : ControlDesigner {
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
            properties.Remove("Text");
            properties.Remove("RightToLeft");

            base.PreFilterProperties(properties);
        }
    }
}
