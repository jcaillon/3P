#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (YamuiComboBox.cs) is part of YamuiFramework.
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiComboBoxDesigner")]
    [ToolboxBitmap(typeof(ComboBox))]
    public class YamuiComboBox : ComboBox {
        #region Fields

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        [DefaultValue(DrawMode.OwnerDrawFixed)]
        [Browsable(false)]
        public new DrawMode DrawMode {
            get { return DrawMode.OwnerDrawFixed; }
            set { base.DrawMode = DrawMode.OwnerDrawFixed; }
        }

        [DefaultValue(ComboBoxStyle.DropDownList)]
        [Browsable(false)]
        public new ComboBoxStyle DropDownStyle {
            get { return ComboBoxStyle.DropDownList; }
            set { base.DropDownStyle = ComboBoxStyle.DropDownList; }
        }

        private string _waterMark = "";
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("")]
        [Category("Yamui")]
        public string WaterMark {
            get { return _waterMark; }
            set {
                _waterMark = value.Trim();
                Invalidate();
            }
        }

        private bool _isHovered;
        private bool _isPressed;
        private bool _isFocused;

        #endregion

        #region Constructor

        public YamuiComboBox() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint, true);

            base.DrawMode = DrawMode.OwnerDrawFixed;
            base.DropDownStyle = ComboBoxStyle.DropDownList;

            // Stuff for the border color
            _dropDownCheck.Interval = 100;
            _dropDownCheck.Tick += dropDownCheck_Tick;
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

        protected override void OnPaintBackground(PaintEventArgs e) {
            try {
                Color backColor = ThemeManager.ButtonColors.BackGround(BackColor, UseCustomBackColor, _isFocused, _isHovered, _isPressed, Enabled);
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
                OnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {
            ItemHeight = GetPreferredSize(Size.Empty).Height;

            Color borderColor = ThemeManager.ButtonColors.BorderColor(_isFocused, _isHovered, _isPressed, Enabled);
            Color foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, UseCustomForeColor, _isFocused, _isHovered, _isPressed, Enabled);

            // draw border
            if (borderColor != Color.Transparent)
                using (Pen p = new Pen(borderColor)) {
                    Rectangle borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
                    e.Graphics.DrawRectangle(p, borderRect);
                }

            // draw the down arrow
            using (SolidBrush b = new SolidBrush(foreColor)) {
                e.Graphics.FillPolygon(b, new[] { new Point(Width - 20, (Height / 2) - 2), new Point(Width - 9, (Height / 2) - 2), new Point(Width - 15, (Height / 2) + 4) });
            }

            Rectangle textRect = new Rectangle(2, 2, Width - 20, Height - 4);
            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), textRect, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            DrawTextPrompt(e.Graphics);
        }

        protected override void OnDrawItem(DrawItemEventArgs e) {
            if (e.Index >= 0) {
                Color foreColor;
                Color backColor = ThemeManager.Current.ButtonColorsNormalBackColor;

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
                    backColor = ThemeManager.AccentColor;
                    foreColor = ThemeManager.Current.ButtonColorsPressForeColor;
                } else {
                    foreColor = ThemeManager.Current.ButtonColorsNormalForeColor;
                }

                using (SolidBrush b = new SolidBrush(backColor)) {
                    e.Graphics.FillRectangle(b, new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height));
                }

                Rectangle textRect = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), FontManager.GetStandardFont(), textRect, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            } else {
                base.OnDrawItem(e);
            }
        }

        private void DrawTextPrompt() {
            using (Graphics graphics = CreateGraphics()) {
                DrawTextPrompt(graphics);
            }
        }

        private void DrawTextPrompt(Graphics g) {
            if (_isPressed || SelectedIndex != -1) return;
            Rectangle textRect = new Rectangle(2, 2, Width - 20, Height - 4);
            TextRenderer.DrawText(g, _waterMark, FontManager.GetStandardWaterMarkFont(), textRect, SystemColors.GrayText, Color.Transparent, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
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
        protected override void OnSelectedIndexChanged(EventArgs e) {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }

        public override Size GetPreferredSize(Size proposedSize) {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics()) {
                string measureText = Text.Length > 0 ? Text : "MeasureText";
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, measureText, FontManager.GetStandardFont(), proposedSize, TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.VerticalCenter);
                preferredSize.Height += 4;
            }

            return preferredSize;
        }

        private const int OCM_COMMAND = 0x2111;
        private const int WM_PAINT = 15;
        private const int WM_CTLCOLORLISTBOX = 0x0134;

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);

            if (((m.Msg == WM_PAINT) || (m.Msg == OCM_COMMAND))) {
                DrawTextPrompt();
            }

            if (m.Msg == WM_CTLCOLORLISTBOX) {
                base.WndProc(ref m);
                DrawNativeBorder(m.LParam);
            }
        }

        #endregion

        #region " color border "

        /// <summary>
        /// Non client area border drawing
        /// </summary>
        /// <param name="handle">The handle to the control</param>
        public void DrawNativeBorder(IntPtr handle) {
            // Define the windows frame rectangle of the control
            RECT controlRect;
            GetWindowRect(handle, out controlRect);
            controlRect.Right -= controlRect.Left; controlRect.Bottom -= controlRect.Top;
            controlRect.Top = controlRect.Left = 0;

            // GetFont the device context of the control
            IntPtr dc = GetWindowDC(handle);

            // Define the client area inside the control rect so it won't be filled when drawing the border
            RECT clientRect = controlRect;
            clientRect.Left += 1;
            clientRect.Top += 1;
            clientRect.Right -= 1;
            clientRect.Bottom -= 1;
            ExcludeClipRect(dc, clientRect.Left, clientRect.Top, clientRect.Right, clientRect.Bottom);

            // Create a pen and select it
            Color borderColor = ThemeManager.AccentColor;
            IntPtr border = CreatePen(PenStyles.PS_SOLID, 1, RGB(borderColor.R,
                borderColor.G, borderColor.B));

            // Draw the border rectangle
            IntPtr borderPen = SelectObject(dc, border);
            Rectangle(dc, controlRect.Left, controlRect.Top, controlRect.Right, controlRect.Bottom);
            SelectObject(dc, borderPen);
            DeleteObject(border);

            // Release the device context
            ReleaseDC(handle, dc);
            SetFocus(handle);
        }

        private Timer _dropDownCheck = new Timer();

        /// <summary>
        /// On drop down
        /// </summary>
        protected override void OnDropDown(EventArgs e) {
            base.OnDropDown(e);

            // Start checking for the dropdown visibility
            _dropDownCheck.Start();
        }

        /// <summary>
        /// Checks when the drop down is fully visible
        /// </summary>
        private void dropDownCheck_Tick(object sender, EventArgs e) {
            _isHovered = false;
            Invalidate();

            // If the drop down has been fully dropped
            if (DroppedDown) {
                // Stop the time, send a listbox update
                _dropDownCheck.Stop();
                Message m = GetControlListBoxMessage(Handle);
                WndProc(ref m);
            }
        }

        /// <summary>
        /// Creates a default WM_CTLCOLORLISTBOX message
        /// </summary>
        /// <param name="handle">The drop down handle</param>
        /// <returns>A WM_CTLCOLORLISTBOX message</returns>
        public Message GetControlListBoxMessage(IntPtr handle) {
            // Force non-client redraw for focus border
            Message m = new Message();
            m.HWnd = handle;
            m.LParam = GetListHandle(handle);
            m.WParam = IntPtr.Zero;
            m.Msg = WM_CTLCOLORLISTBOX;
            m.Result = IntPtr.Zero;
            return m;
        }

        /// <summary>
        /// Gets the list control of a combo box
        /// </summary>
        /// <param name="handle">Handle of the combo box itself</param>
        /// <returns>A handle to the list</returns>
        public static IntPtr GetListHandle(IntPtr handle) {
            COMBOBOXINFO info;
            info = new COMBOBOXINFO();
            info.cbSize = Marshal.SizeOf(info);
            return GetComboBoxInfo(handle, ref info) ? info.hwndList : IntPtr.Zero;
        }

        public enum PenStyles {
            PS_SOLID = 0,
            PS_DASH = 1,
            PS_DOT = 2,
            PS_DASHDOT = 3,
            PS_DASHDOTDOT = 4
        }

        public enum ComboBoxButtonState {
            STATE_SYSTEM_NONE = 0,
            STATE_SYSTEM_INVISIBLE = 0x00008000,
            STATE_SYSTEM_PRESSED = 0x00000008
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COMBOBOXINFO {
            public Int32 cbSize;
            public RECT rcItem;
            public RECT rcButton;
            public ComboBoxButtonState buttonState;
            public IntPtr hwndCombo;
            public IntPtr hwndEdit;
            public IntPtr hwndList;
        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetComboBoxInfo(IntPtr hWnd, ref COMBOBOXINFO pcbi);

        [DllImport("gdi32.dll")]
        public static extern int ExcludeClipRect(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(PenStyles enPenStyle, int nWidth, int crColor);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern void Rectangle(IntPtr hdc, int X1, int Y1, int X2, int Y2);

        public static int RGB(int R, int G, int B) {
            return (R | (G << 8) | (B << 16));
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left_, int top_, int right_, int bottom_) {
                Left = left_;
                Top = top_;
                Right = right_;
                Bottom = bottom_;
            }

            public override bool Equals(object obj) {
                if (obj == null || !(obj is RECT)) {
                    return false;
                }
                return Equals((RECT)obj);
            }

            public bool Equals(RECT value) {
                return Left == value.Left &&
                Top == value.Top &&
                Right == value.Right &&
                Bottom == value.Bottom;
            }

            public int Height {
                get {
                    return Bottom - Top + 1;
                }
            }

            public int Width {
                get {
                    return Right - Left + 1;
                }
            }

            public Size Size { get { return new Size(Width, Height); } }
            public Point Location { get { return new Point(Left, Top); } }
            // Handy method for converting to a System.Drawing.Rectangle
            public Rectangle ToRectangle() {
                return System.Drawing.Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }

            public static RECT FromRectangle(Rectangle rectangle) {
                return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            public void Inflate(int width, int height) {
                Left -= width;
                Top -= height;
                Right += width;
                Bottom += height;
            }

            public override int GetHashCode() {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                                  ^ ((Width << 0x1a) | (Width >> 6))
                                  ^ ((Height << 7) | (Height >> 0x19));
            }

            public static implicit operator Rectangle(RECT rect) {
                return System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }

            public static implicit operator RECT(Rectangle rect) {
                return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
        }

        #endregion
    }

    internal class YamuiComboBoxDesigner : ControlDesigner {
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