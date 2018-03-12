#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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

    // https://gist.github.com/Ciantic/471698 : keyboard event listener

    [Designer("YamuiFramework.Controls.YamuiRegularTextBox2Designer")]
    public sealed class YamuiTextBox : TextBox, IYamuiControl {


        #region private

        private bool _selectAllTextOnActivate = true;
        private bool _appliedPadding;

        #endregion

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
            set {
                _customBackColor = value;
                Invalidate();
            }
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
            set {
                _customForeColor = value;
                Invalidate();
            }
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
        [Browsable(true)]
        public new bool Multiline {
            get { return _multiline; }
            set {
                _multiline = value;
                WordWrap = Multiline;
                AcceptsReturn = Multiline;
            }
        }

        private bool _multiline;

        /// <summary>
        /// If true, when the textbox is activated, the whole text is selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SelectAllTextOnActivate {
            get { return _selectAllTextOnActivate; }
            set { _selectAllTextOnActivate = value; }
        }
        
        public override bool AllowDrop { get; set; } = true;

        #endregion

        #region Constructor

        public YamuiTextBox() {
            BorderStyle = BorderStyle.None;
            Font = FontManager.GetStandardFont();
            BackColor = YamuiThemeManager.Current.ButtonBg(CustomBackColor, UseCustomBackColor, _isFocused, _isHovered, false, Enabled);
            ForeColor = YamuiThemeManager.Current.ButtonFg(CustomForeColor, UseCustomForeColor, _isFocused, _isHovered, false, Enabled);
            WordWrap = false;
            base.Multiline = true;
            MinimumSize = new Size(20, 20);
        }

        #endregion

        #region Custom paint

        private const int OcmCommand = 0x2111;

        protected override void WndProc(ref Message m) {
            
            // Send WM_MOUSEWHEEL messages to the parent
            if (!Multiline && m.Msg == (int) WinApi.Messages.WM_MOUSEWHEEL) WinApi.SendMessage(Parent.Handle, (uint) m.Msg, m.WParam, m.LParam);
            else base.WndProc(ref m);

            if ((m.Msg == (int) WinApi.Messages.WM_PAINT) || (m.Msg == OcmCommand)) {
                // Apply a padding INSIDE the textbox (so we can draw the border!)
                if (!_appliedPadding) {
                    ApplyInternalPadding();
                    _appliedPadding = true;
                } else {
                    using (Graphics graphics = CreateGraphics()) {
                        CustomPaint(graphics);
                    }
                }
            }
        }

        public void ApplyInternalPadding() {
            WinApi.RECT rc = new WinApi.RECT(2, 2, ClientSize.Width - 4, ClientSize.Height - 3);
            WinApi.SendMessageRefRect(Handle, WinApi.EM_SETRECT, 0, ref rc);
        }

        private void CustomPaint(Graphics g) {
            BackColor = YamuiThemeManager.Current.ButtonBg(CustomBackColor, UseCustomBackColor, _isFocused, _isHovered, false, Enabled);
            ForeColor = YamuiThemeManager.Current.ButtonFg(CustomForeColor, UseCustomForeColor, _isFocused, _isHovered, false, Enabled);
            Color borderColor = YamuiThemeManager.Current.ButtonBorder(_isFocused, _isHovered, false, Enabled);

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
                TextRenderer.DrawText(g, WaterMark, FontManager.GetFont(FontFunction.WaterMark), clientRectangle, YamuiThemeManager.Current.ButtonWatermarkFore, flags);
            }

            // draw border
            using (Pen p = new Pen(borderColor))
                g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        #endregion

        #region Managing isHovered, isFocused

        #region Focus Methods

        protected override void OnEnter(EventArgs e) {
            _isFocused = true;
            if (SelectAllTextOnActivate)
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

        #endregion

        #endregion

        #region Dragdrop

        protected override void OnDragEnter(DragEventArgs drgevent) {
            base.OnDragEnter(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))  
                drgevent.Effect = DragDropEffects.Link;  
            else if (drgevent.Data.GetDataPresent(DataFormats.StringFormat))  
                drgevent.Effect = DragDropEffects.Copy;  
            else 
                drgevent.Effect = DragDropEffects.None;  
        }
        
        protected override void OnDragDrop(DragEventArgs drgevent) {
            base.OnDragDrop(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null) {
                    Text = string.Join(Multiline ? "\r\n" : ",", files);
                }
            } else if (drgevent.Data.GetDataPresent(DataFormats.StringFormat)) {
                Text = drgevent.Data.GetData(DataFormats.FileDrop) as string ?? "";
            }
        }

        #endregion

        #region PerformKeyDown

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            if (!e.Handled && e.KeyCode == Keys.Return) {
                if (Multiline) {
                    var initialPos = SelectionStart;
                    Text = Text.Substring(0, initialPos) + Environment.NewLine + (initialPos < TextLength ? Text.Substring(initialPos, TextLength - initialPos) : "");
                    SelectionStart = initialPos + 2;
                    SelectionLength = 0;
                    ScrollToCaret();
                    return true;
                }
            }
            return e.Handled;
        }

        #endregion

        public void UpdateBoundsPublic() {
            UpdateBounds();
        }
    }

    internal class YamuiRegularTextBox2Designer : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("WordWrap");
            properties.Remove("MinimumSize");
            base.PreFilterProperties(properties);
        }
    }
}