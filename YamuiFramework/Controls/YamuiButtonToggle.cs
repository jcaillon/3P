#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiButtonToggle.cs) is part of YamuiFramework.
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
    [DefaultEvent("ButtonPressed")]
    public class YamuiButtonToggle : YamuiButton {
        #region public field

        private bool _checked;

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool Checked {
            get { return _checked; }
            set {
                _checked = value;
                Invalidate();
            }
        }

        [DefaultValue(35)]
        [Category("Yamui")]
        public int ToggleSize { get; set; }

        #endregion

        #region Constructor

        public YamuiButtonToggle() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.Selectable |
                     ControlStyles.AllPaintingInWmPaint, true);
            ButtonPressed += OnButtonPressed;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            try {
                var backColor = YamuiThemeManager.Current.ButtonBg(BackColor, UseCustomBackColor, IsFocused, IsHovered, false, Enabled, Checked);
                var borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, false, Enabled, Checked);
                var foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, UseCustomForeColor, IsFocused, IsHovered, false, Enabled, Checked);

                if (ToggleSize == 0)
                    ToggleSize = 30;

                Rectangle textRect = new Rectangle(ToggleSize + 3, 0, Width - 42, Height);
                Rectangle backRect = new Rectangle(0, 0, string.IsNullOrEmpty(Text) ? Width : ToggleSize, Height);

                // background
                if (!string.IsNullOrEmpty(Text)) {
                    PaintTransparentBackground(e.Graphics, DisplayRectangle);
                    if (backColor != Color.Transparent)
                        using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.FormBack)) {
                            e.Graphics.FillRectangle(b, backRect);
                        }
                } else {
                    if (backColor != Color.Transparent)
                        e.Graphics.Clear(YamuiThemeManager.Current.FormBack);
                    else
                        PaintTransparentBackground(e.Graphics, DisplayRectangle);
                }

                // draw the back
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                using (SolidBrush b = new SolidBrush(backColor)) {
                    e.Graphics.FillRectangle(b, new Rectangle(Height/2, 0, backRect.Width - Height - 1, Height - 1));
                    e.Graphics.FillEllipse(b, new Rectangle(0, 0, Height - 1, Height - 1));
                    e.Graphics.FillEllipse(b, new Rectangle(backRect.Width - Height - 1, 0, Height - 1, Height - 1));
                }

                // draw foreground ellipse
                using (SolidBrush b = new SolidBrush(foreColor)) {
                    if (!Checked)
                        e.Graphics.FillEllipse(b, new Rectangle(2, 2, Height - 5, Height - 5));
                    else
                        e.Graphics.FillEllipse(b, new Rectangle(backRect.Width - Height + 2, 2, Height - 5, Height - 5));
                }

                // draw checked.. or not
                if (Checked) {
                    var fuRect = ClientRectangle;
                    fuRect.Width = 15;
                    fuRect.Offset(5, Height/2 - 11);
                    TextRenderer.DrawText(e.Graphics, "a", FontManager.GetOtherFont("Webdings", FontStyle.Regular, (float) (Height*0.9)), fuRect, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                // draw border
                if (borderColor != Color.Transparent)
                    using (Pen b = new Pen(borderColor, 1)) {
                        var path = new GraphicsPath();
                        path.AddLine(Height/2, 0, backRect.Width - Height - 1, 0);
                        path.AddArc(backRect.Width - Height - 1, 0, Height - 1, Height - 1, -90, 180);

                        path.AddLine(backRect.Width - Height - 1, Height - 1, Height/2, Height - 1);
                        path.AddArc(0, 0, Height - 1, Height - 1, 90, 180);
                        e.Graphics.DrawPath(b, path);
                    }

                e.Graphics.SmoothingMode = SmoothingMode.Default;

                // text?
                if (!string.IsNullOrEmpty(Text))
                    TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRect, YamuiThemeManager.Current.FormFore, FontManager.GetTextFormatFlags(TextAlign));
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