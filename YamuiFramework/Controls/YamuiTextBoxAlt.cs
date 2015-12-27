#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiTextBox.cs) is part of YamuiFramework.
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
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer("YamuiFramework.Controls.YamuiTextBoxDesigner")]
    public class YamuiTextBoxAlt : Control {
        #region Fields
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool Highlight { get; set; }

        private PromptedTextBox _baseTextBox;

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("")]
        [Category("Yamui")]
        public string WaterMark {
            get { return _baseTextBox.WaterMark; }
            set { _baseTextBox.WaterMark = value; }
        }

        private Image _textBoxIcon;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(null)]
        [Category("Yamui")]
        public Image Icon {
            get { return _textBoxIcon; }
            set {
                _textBoxIcon = value;
                Refresh();
            }
        }

        private bool _textBoxIconRight;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool IconRight {
            get { return _textBoxIconRight; }
            set {
                _textBoxIconRight = value;
                Refresh();
            }
        }

        private bool _displayIcon = true;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(true)]
        [Category("Yamui")]
        public bool DisplayIcon {
            get { return _displayIcon; }
            set {
                _displayIcon = value;
                Refresh();
            }
        }

        protected Size IconSize {
            get {
                if (_displayIcon && _textBoxIcon != null) {
                    Size originalSize = _textBoxIcon.Size;
                    double resizeFactor = (ClientRectangle.Height - 2) / (double)originalSize.Height;

                    Point iconLocation = new Point(1, 1);
                    return new Size((int)(originalSize.Width * resizeFactor), (int)(originalSize.Height * resizeFactor));
                }

                return new Size(-1, -1);
            }
        }

        private bool _isHovered;

        private bool _isFocused;

        private bool IsFocused {
            get { return _isFocused; }
            set { 
                _baseTextBox.IsFocused = value;
                _isFocused = value;
            }
        }

        #endregion

        #region Routing Fields

        public override ContextMenu ContextMenu {
            get { return _baseTextBox.ContextMenu; }
            set {
                ContextMenu = value;
                _baseTextBox.ContextMenu = value;
            }
        }

        public override ContextMenuStrip ContextMenuStrip {
            get { return _baseTextBox.ContextMenuStrip; }
            set {
                ContextMenuStrip = value;
                _baseTextBox.ContextMenuStrip = value;
            }
        }

        [DefaultValue(false)]
        public bool Multiline {
            get { return _baseTextBox.Multiline; }
            set { _baseTextBox.Multiline = value; }
        }

        [DefaultValue(true)]
        public bool WordWrap {
            get { return _baseTextBox.WordWrap; }
            set { _baseTextBox.WordWrap = value; }
        }

        public override string Text {
            get { return _baseTextBox.Text; }
            set { _baseTextBox.Text = value; }
        }

        public string[] Lines {
            get { return _baseTextBox.Lines; }
            set { _baseTextBox.Lines = value; }
        }

        [Browsable(false)]
        public string SelectedText {
            get { return _baseTextBox.SelectedText; }
            set { _baseTextBox.Text = value; }
        }

        [DefaultValue(false)]
        public bool ReadOnly {
            get { return _baseTextBox.ReadOnly; }
            set { _baseTextBox.ReadOnly = value; }
        }

        public char PasswordChar {
            get { return _baseTextBox.PasswordChar; }
            set { _baseTextBox.PasswordChar = value; }
        }

        [DefaultValue(false)]
        public bool UseSystemPasswordChar {
            get { return _baseTextBox.UseSystemPasswordChar; }
            set { _baseTextBox.UseSystemPasswordChar = value; }
        }

        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign {
            get { return _baseTextBox.TextAlign; }
            set { _baseTextBox.TextAlign = value; }
        }

        [DefaultValue(true)]
        public new bool TabStop {
            get { return _baseTextBox.TabStop; }
            set { _baseTextBox.TabStop = value; }
        }

        public int MaxLength {
            get { return _baseTextBox.MaxLength; }
            set { _baseTextBox.MaxLength = value; }
        }

        public ScrollBars ScrollBars {
            get { return _baseTextBox.ScrollBars; }
            set { _baseTextBox.ScrollBars = value; }
        }

        #endregion

        #region Constructor

        public YamuiTextBoxAlt() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint, true);

            base.TabStop = false;
            GotFocus += YamuiTextBox_GotFocus;

            CreateBaseTextBox();
            UpdateBaseTextBox();
            AddEventHandler();
        }

        void YamuiTextBox_GotFocus(object sender, EventArgs e) {
            _baseTextBox.Focus();
        }

        #endregion

        #region Routing Methods

        public event EventHandler AcceptsTabChanged;
        private void BaseTextBoxAcceptsTabChanged(object sender, EventArgs e) {
            if (AcceptsTabChanged != null)
                AcceptsTabChanged(this, e);
        }

        private void BaseTextBoxSizeChanged(object sender, EventArgs e) {
            OnSizeChanged(e);
        }

        private void BaseTextBoxCursorChanged(object sender, EventArgs e) {
            OnCursorChanged(e);
        }

        private void BaseTextBoxContextMenuStripChanged(object sender, EventArgs e) {
            OnContextMenuStripChanged(e);
        }

        private void BaseTextBoxContextMenuChanged(object sender, EventArgs e) {
            OnContextMenuChanged(e);
        }

        private void BaseTextBoxClientSizeChanged(object sender, EventArgs e) {
            OnClientSizeChanged(e);
        }

        private void BaseTextBoxClick(object sender, EventArgs e) {
            OnClick(e);
        }

        private void BaseTextBoxChangeUiCues(object sender, UICuesEventArgs e) {
            OnChangeUICues(e);
        }

        private void BaseTextBoxCausesValidationChanged(object sender, EventArgs e) {
            OnCausesValidationChanged(e);
        }

        private void BaseTextBoxKeyUp(object sender, KeyEventArgs e) {
            OnKeyUp(e);
        }

        private void BaseTextBoxKeyPress(object sender, KeyPressEventArgs e) {
            OnKeyPress(e);
        }

        private void BaseTextBoxKeyDown(object sender, KeyEventArgs e) {
            OnKeyDown(e);
        }

        private void BaseTextBoxTextChanged(object sender, EventArgs e) {
            OnTextChanged(e);
        }

        public void Select(int start, int length) {
            _baseTextBox.Select(start, length);
        }

        public void SelectAll() {
            _baseTextBox.SelectAll();
        }

        public void Clear() {
            _baseTextBox.Clear();
        }

        public void AppendText(string text) {
            _baseTextBox.AppendText(text);
        }

        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected void CustomOnPaintBackground(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.ButtonColors.BackGround(BackColor, UseCustomBackColor, IsFocused, _isHovered, false, Enabled);
                _baseTextBox.BackColor = backColor;
                e.Graphics.Clear(backColor);
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
            _baseTextBox.ForeColor = ThemeManager.ButtonColors.ForeGround(ForeColor, UseCustomForeColor, IsFocused, _isHovered, false, Enabled);
            Color borderColor = ThemeManager.ButtonColors.BorderColor(IsFocused, _isHovered, false, Enabled);

            using (Pen p = new Pen(borderColor)) {
                e.Graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
            }

            DrawIcon(e.Graphics);
        }

        private void DrawIcon(Graphics g) {
            if (_displayIcon && _textBoxIcon != null) {
                Point iconLocation = new Point(1, 1);
                if (_textBoxIconRight) {
                    iconLocation = new Point(ClientRectangle.Width - IconSize.Width - 1, 1);
                }

                g.DrawImage(_textBoxIcon, new Rectangle(iconLocation, IconSize));

                UpdateBaseTextBox();
            }
        }
        #endregion

        #region Overridden Methods

        public override void Refresh() {
            base.Refresh();
            UpdateBaseTextBox();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            UpdateBaseTextBox();
        }

        #endregion

        #region Private Methods

        private void CreateBaseTextBox() {
            if (_baseTextBox != null) return;

            _baseTextBox = new PromptedTextBox();

            _baseTextBox.BorderStyle = BorderStyle.None;
            _baseTextBox.Font = FontManager.GetStandardFont();
            _baseTextBox.Location = new Point(1, 1);
            _baseTextBox.Size = new Size(Width - 2, Height - 2);

            Size = new Size(_baseTextBox.Width + 2, _baseTextBox.Height + 2);

            _baseTextBox.TabStop = true;
            //baseTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            Controls.Add(_baseTextBox);
        }

        private void AddEventHandler() {
            _baseTextBox.AcceptsTabChanged += BaseTextBoxAcceptsTabChanged;

            _baseTextBox.CausesValidationChanged += BaseTextBoxCausesValidationChanged;
            _baseTextBox.ChangeUICues += BaseTextBoxChangeUiCues;
            _baseTextBox.Click += BaseTextBoxClick;
            _baseTextBox.ClientSizeChanged += BaseTextBoxClientSizeChanged;
            _baseTextBox.ContextMenuChanged += BaseTextBoxContextMenuChanged;
            _baseTextBox.ContextMenuStripChanged += BaseTextBoxContextMenuStripChanged;
            _baseTextBox.CursorChanged += BaseTextBoxCursorChanged;

            _baseTextBox.KeyDown += BaseTextBoxKeyDown;
            _baseTextBox.KeyPress += BaseTextBoxKeyPress;
            _baseTextBox.KeyUp += BaseTextBoxKeyUp;

            _baseTextBox.SizeChanged += BaseTextBoxSizeChanged;

            _baseTextBox.TextChanged += BaseTextBoxTextChanged;

            _baseTextBox.Enter += (sender, args) => {
                IsFocused = true;
                Invalidate();
            };
            _baseTextBox.Leave += (sender, args) => {
                IsFocused = false;
                Invalidate();
            };
            _baseTextBox.GotFocus += (sender, args) => {
                IsFocused = true;
                Invalidate();
            };
            _baseTextBox.LostFocus += (sender, args) => {
                IsFocused = false;
                Invalidate();
            };
            _baseTextBox.MouseEnter += (sender, args) => {
                _isHovered = true;
                Invalidate();
            };
            _baseTextBox.MouseLeave += (sender, args) => {
                _isHovered = false;
                Invalidate();
            };
        }

        private void UpdateBaseTextBox() {
            if (_baseTextBox == null) return;

            _baseTextBox.Font = FontManager.GetStandardFont();

            if (_displayIcon) {
                Point textBoxLocation = new Point(IconSize.Width + 2, 1);
                if (_textBoxIconRight) {
                    textBoxLocation = new Point(1, 1);
                }

                _baseTextBox.Location = textBoxLocation;
                _baseTextBox.Size = new Size(Width - 3 - IconSize.Width, Height - 2);
            } else {
                _baseTextBox.Location = new Point(1, 1);
                _baseTextBox.Size = new Size(Width - 2, Height - 2);
            }
        }

        #endregion

        #region PromptedTextBox

        private class PromptedTextBox : TextBox {

            #region fields
            private const int OcmCommand = 0x2111;
            private const int WM_PAINT = 15;
            private string _waterMark = "";

            public bool IsFocused;

            public string WaterMark {
                get { return _waterMark; }
                set {
                    _waterMark = value.Trim();
                    Invalidate();
                }
            }
            #endregion

            #region Paint

            private void DrawTextPrompt(Graphics g) {
                TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
                Rectangle clientRectangle = ClientRectangle;
                switch (TextAlign) {
                    case HorizontalAlignment.Left:
                        clientRectangle.Offset(1, 0);
                        break;

                    case HorizontalAlignment.Right:
                        flags |= TextFormatFlags.Right;
                        break;

                    case HorizontalAlignment.Center:
                        flags |= TextFormatFlags.HorizontalCenter;
                        break;
                }
                TextRenderer.DrawText(g, _waterMark, FontManager.GetStandardWaterMarkFont(), clientRectangle, ThemeManager.Current.ButtonColorsDisabledForeColor, flags);
            }

            protected override void OnTextAlignChanged(EventArgs e) {
                base.OnTextAlignChanged(e);
                Invalidate();
            }

            #endregion


            protected override void WndProc(ref Message m) {
                base.WndProc(ref m);
                if ((m.Msg == WM_PAINT) || (m.Msg == OcmCommand)) {
                    if (!IsFocused && string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(WaterMark)) {
                        using (Graphics graphics = CreateGraphics()) {
                            DrawTextPrompt(graphics);
                        }
                    }
                }
            }

        }

        #endregion
    }

    #region Designer

    internal class YamuiTextBoxDesigner : ControlDesigner {
        public override SelectionRules SelectionRules {
            get {
                PropertyDescriptor propDescriptor = TypeDescriptor.GetProperties(Component)["Multiline"];

                if (propDescriptor != null) {
                    bool isMultiline = (bool) propDescriptor.GetValue(Component);

                    if (isMultiline) {
                        return SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.AllSizeable;
                    }

                    return SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.LeftSizeable | SelectionRules.RightSizeable;
                }

                return base.SelectionRules;
            }
        }

        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("BackgroundImage");
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("Font");

            base.PreFilterProperties(properties);
        }
    }

    #endregion
}