#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiTextBox.cs) is part of YamuiFramework.
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiTextBox2Designer")]
    public sealed class YamuiTextBox : TextBox {

        #region Fields

        /// <summary>
        /// Designed to be used with CustomBackColor
        /// </summary>
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        /// <summary>
        /// set UseCustomBackColor to true or it won't do anything
        /// </summary>
        [DefaultValue(false)]
        [Category("Yamui")]
        public Color CustomBackColor {
            get { return _customBackColor; }
            set { _customBackColor = value; Invalidate(); }
        }
        private Color _customBackColor;

        /// <summary>
        /// Designed to be used with CustomForeColor
        /// </summary>
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        /// <summary>
        /// Set UseCustomForeColor to true or it won't do anything
        /// </summary>
        [DefaultValue(false)]
        [Category("Yamui")]
        public Color CustomForeColor {
            get { return _customForeColor; }
            set { _customForeColor = value; Invalidate(); }
        }
        private Color _customForeColor;

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("")]
        [Category("Yamui")]
        public string WaterMark { get; set; }

        private bool _isHovered;
        private bool _isFocused;

        /// <summary>
        /// Use this propety instead of the multiLine property!
        /// </summary>
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool MultiLines { get; set; }

        #endregion

        #region Constructor

        public YamuiTextBox() {
            BorderStyle = BorderStyle.None;
            Font = FontManager.GetStandardFont();
            BackColor = YamuiThemeManager.Current.ButtonBg(CustomBackColor, UseCustomBackColor, _isFocused, _isHovered, false, Enabled);
            ForeColor = YamuiThemeManager.Current.ButtonFg(CustomForeColor, UseCustomForeColor, _isFocused, _isHovered, false, Enabled);
            Multiline = true;
            Size = new Size(100, 20);
            MinimumSize = new Size(20, 20);
        }

        #endregion

        #region Handle MultiLines

        protected override void OnTextChanged(EventArgs e) {
            if (!MultiLines && Text.Contains("\n")) {
                Text = Text.Replace("\n", "").Replace("\r", "");
                SelectionStart = TextLength;
            }
            base.OnTextChanged(e);
        }

        #endregion

        #region Custom paint

        private const int OcmCommand = 0x2111;
        private const int WmPaint = 15;
        private bool _appliedPadding;

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if ((m.Msg == WmPaint) || (m.Msg == OcmCommand)) {
                // Apply a padding INSIDE the textbox (so we can draw the border!)
                if (!_appliedPadding) {
                    WinApi.RECT rc = new WinApi.RECT(4, 2, ClientSize.Width - 8, ClientSize.Height - 3);
                    WinApi.SendMessageRefRect(Handle, WinApi.EM_SETRECT, 0, ref rc);

                    _appliedPadding = true;
                } else {
                    using (Graphics graphics = CreateGraphics()) {
                        CustomPaint(graphics);
                    }
                }

            }
        }

        public void ApplyInternalPadding() {
            WinApi.RECT rc = new WinApi.RECT(4, 1, ClientSize.Width - 8, ClientSize.Height - 2);
            WinApi.SendMessageRefRect(Handle, WinApi.EM_SETRECT, 0, ref rc);
        }

        private void CustomPaint(Graphics g) {
            if (!_isFocused && string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(WaterMark) && Enabled) {
                TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
                Rectangle clientRectangle = ClientRectangle;
                switch (TextAlign) {
                    case HorizontalAlignment.Left:
                        clientRectangle.Offset(4, 2);
                        break;

                    case HorizontalAlignment.Right:
                        flags |= TextFormatFlags.Right;
                        clientRectangle.Offset(0, 2);
                        clientRectangle.Inflate(-4, 0);
                        break;

                    case HorizontalAlignment.Center:
                        flags |= TextFormatFlags.HorizontalCenter;
                        clientRectangle.Offset(0, 2);
                        break;
                }
                TextRenderer.DrawText(g, WaterMark, FontManager.GetFont(FontFunction.WaterMark), clientRectangle, YamuiThemeManager.Current.ButtonDisabledFore, flags);
            }

            // Modify colors
            BackColor = YamuiThemeManager.Current.ButtonBg(CustomBackColor, UseCustomBackColor, _isFocused, _isHovered, false, Enabled);
            ForeColor = YamuiThemeManager.Current.ButtonFg(CustomForeColor, UseCustomForeColor, _isFocused, _isHovered, false, Enabled);

            // draw border
            Color borderColor = YamuiThemeManager.Current.ButtonBorder(_isFocused, _isHovered, false, Enabled);
            using (Pen p = new Pen(borderColor)) {
                g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }

        #endregion

        #region Managing isHovered, isPressed, isFocused

        #region Focus Methods

        protected override void OnGotFocus(EventArgs e) {
            _isFocused = true;
            SelectAll();
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
            SelectAll();
            Invalidate();

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            _isFocused = false;
            Invalidate();

            base.OnLeave(e);
        }

        #endregion

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

        protected override void OnClick(EventArgs e) {
            if (!_isFocused) {
                SelectAll();
            }
            base.OnClick(e);
        }

        #endregion

        #endregion
    }

    internal class YamuiTextBox2Designer : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("Multiline");
            properties.Remove("MinimumSize");
            base.PreFilterProperties(properties);
        }
    }
}