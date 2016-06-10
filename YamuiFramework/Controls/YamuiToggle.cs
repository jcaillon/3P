#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiToggle.cs) is part of YamuiFramework.
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

    [Designer("YamuiFramework.Controls.YamuiButtonDesigner")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public class YamuiToggle : YamuiButton {

        #region public field

        private bool _checked;

        public bool Checked {
            get { return _checked; }
            set {
                _checked = value;
                Invalidate();
            }
        }

        public int ToggleSize { get; set; }

        #endregion


        #region Constructor

        public YamuiToggle() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint, true);
            ButtonPressed += OnButtonPressed;
        }

        ~YamuiToggle() {
            ButtonPressed -= OnButtonPressed;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            try {
                // draw background
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.FormBack)) {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                if (ToggleSize == 0)
                    ToggleSize = string.IsNullOrEmpty(Text) ? Width : 30;

                Color textColor = YamuiThemeManager.Current.ButtonFg(ForeColor, false, IsFocused, IsHovered, IsPressed, Enabled);
                Color foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, false, IsFocused, IsHovered, Checked, Enabled);
                Color borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, IsPressed, Enabled);
                Color unfilledColor = YamuiThemeManager.Current.ButtonNormalBack;
                if (unfilledColor == YamuiThemeManager.Current.FormBack) unfilledColor = borderColor;
                Color fillColor = Checked ? YamuiThemeManager.Current.AccentColor : unfilledColor;

                Rectangle textRect = new Rectangle(ToggleSize + 3, 0, Width - 42, Height);
                Rectangle backRect = new Rectangle(0, 0, ToggleSize, Height);

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

                if (!string.IsNullOrEmpty(Text))
                    TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRect, textColor, FontManager.GetTextFormatFlags(TextAlign));
            } catch {
                // ignored
            }
        }
        #endregion

        #region private method

        private void OnButtonPressed(object sender, EventArgs eventArgs) {
            Checked = !Checked;
        }

        #endregion

    }

}
