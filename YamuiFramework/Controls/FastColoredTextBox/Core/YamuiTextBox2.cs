using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Web.UI.Design;
using System.Windows.Forms;
using YamuiFramework.Controls.FastColoredTextBox.Core;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    public class YamuiTextBox2 : UserControl {

        #region constant

        private const int BorderWidth = 1;

        #endregion

        #region private

        private YamuiVirtualScrollBar _verticScrollBar;
        private YamuiVirtualScrollBar _horizScrollBar;
        private FastColoredTextBox.Core.FastColoredTextBox _fctb;
        private int _scrollBarWidth = 10;
        private bool _multiLine;
        private Color _borderColor;
        private Color _backColor;
        private Color _foreColor;
        private bool _waterMarkOn;
        private bool _horizontalScrollBarDisabled;
        private bool _verticalScrollBarDisabled;
        private Padding _basePadding = new Padding(BorderWidth);

        #endregion

        #region Public fields

        /// <summary>
        /// The padding prop can no longer be set, it is used internally
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected new Padding Padding { get; set; }

        /// <summary>
        /// Width of the scrollbars
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollBarWidth {
            get { return _scrollBarWidth; }
            set { _scrollBarWidth = value; }
        }

        /// <summary>
        /// Designed to be used with CustomBackColor
        /// </summary>
        [DefaultValue(false)]
        [Category("Yamui")]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseCustomBackColor { get; set; }


        /// <summary>
        /// Designed to be used with CustomForeColor
        /// </summary>
        [DefaultValue(false)]
        [Category("Yamui")]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseCustomForeColor { get; set; }

        /// <summary>
        /// Display a text when the textbox is empty
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("")]
        [Category("Yamui")]
        public string WaterMark { get; set; }

        /// <summary>
        /// Use this propety instead of the multiLine property!
        /// </summary>
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool MultiLine {
            get { return _multiLine; }
            set {
                _multiLine = value;
                _fctb.Multiline = _multiLine;
                _fctb.AcceptsReturn = _multiLine;
                _fctb.AcceptsTab = _multiLine;
                _verticScrollBar.Disabled = VerticalScrollBarDisabled || !_multiLine;
                _horizScrollBar.Disabled = HorizontalScrollBarDisabled || !_multiLine;
            }
        }

        /// <summary>
        /// The control has focus?
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFocused { get; set; }

        /// <summary>
        /// The control is hovered by the cursor?
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHovered { get; set; }

        /// <summary>
        /// Set to true to disable the horizontal scrollbar
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HorizontalScrollBarDisabled {
            get { return _horizontalScrollBarDisabled; }
            set {
                _horizontalScrollBarDisabled = value;
                _horizScrollBar.Disabled = value;
            }
        }

        /// <summary>
        /// Set to true to disable the vertical scrollbar
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool VerticalScrollBarDisabled {
            get { return _verticalScrollBarDisabled; }
            set {
                _verticalScrollBarDisabled = value;
                _verticScrollBar.Disabled = value;
            }
        }

        /// <summary>
        /// Use this property to access the real text box
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FastColoredTextBox.Core.FastColoredTextBox Core {
            get { return _fctb; }
            set { _fctb = value; }
        }
        
        #endregion

        #region Relayed properties

        public override string Text {
            get { return _fctb.Text; }
            set { _fctb.Text = value; }
        }

        #endregion

        #region Relayed events

        public new event EventHandler<TextChangedEventArgs> TextChanged {
            add { _fctb.TextChanged += value; }
            remove { _fctb.TextChanged -= value; }
        }

        public new event KeyEventHandler KeyDown {
            add { _fctb.KeyDown += value; }
            remove { _fctb.KeyDown -= value; }
        }

        #endregion
        
        #region Constructor

        public YamuiTextBox2() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Selectable |
                ControlStyles.Opaque, true);

            SuspendLayout();

            // Fast colored text box
            _fctb = new FastColoredTextBox.Core.FastColoredTextBox {
                Dock = DockStyle.Fill,
                AutoIndent = false,
                BackBrush = null,
                Cursor = Cursors.IBeam,
                IsReplaceMode = false,
                Location = new Point(0, 0),
                Paddings = new Padding(0),
                ShowLineNumbers = false,
                ShowScrollBars = false,
                Zoom = 100,
                CaretBlinking = true,
                TabStop =  true
            };
            ApplyColorScheme();
            UpdateColors();

            _fctb.Enter += FctbOnEnter;
            _fctb.Leave += FctbOnLeave;
            _fctb.MouseEnter += FctbOnMouseEnter;
            _fctb.MouseLeave += FctbOnMouseLeave;
            _fctb.ScrollbarsUpdated += OnInternalTextBoxScrollbarUpdate;

            Controls.Add(_fctb);

            // vertical scroll
            _verticScrollBar = new YamuiVirtualScrollBar {
                Orientation = ScrollOrientation.VerticalScroll
            };
            _verticScrollBar.Scroll += OnScrollBarUpdate;
            _verticScrollBar.OnInvalidate += OnScrollBarInvalidate;

            // horizontal scroll
            _horizScrollBar = new YamuiVirtualScrollBar {
                Orientation = ScrollOrientation.HorizontalScroll
            };
            _horizScrollBar.Scroll += OnScrollBarUpdate;
            _horizScrollBar.OnInvalidate += OnScrollBarInvalidate;

            ResumeLayout();

            // apply multiline
            MultiLine = MultiLine;

            TabStop = true;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _fctb.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region On events
        
        #region Focus/hover

        protected override void OnMouseLeave(EventArgs e) {
            if (!ClientRectangle.Contains(PointToClient(MousePosition))) {
                IsHovered = false;
                if (_waterMarkOn)
                    Cursor = Cursors.Default;
            }
            Invalidate();
            base.OnMouseLeave(e);
        }

        private void FctbOnMouseLeave(object sender, EventArgs eventArgs) {
            if (!ClientRectangle.Contains(PointToClient(MousePosition)))
                IsHovered = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) {
            IsHovered = true;
            if (_waterMarkOn)
                Cursor = Cursors.IBeam;
            Invalidate();
            base.OnMouseEnter(e);
        }

        private void FctbOnMouseEnter(object sender, EventArgs eventArgs) {
            IsHovered = true;
            Invalidate();
        }

        private void FctbOnLeave(object sender, EventArgs eventArgs) {
            IsFocused = false;
            Invalidate();
        }

        private void FctbOnEnter(object sender, EventArgs eventArgs) {
            IsFocused = true;
            Invalidate();
        }

        #endregion

        protected override void OnMouseDown(MouseEventArgs e) {
            ActivateChild();
            base.OnMouseDown(e);
        }

        private void ActivateChild() {
            if (_waterMarkOn) {
                Cursor = Cursors.Default;
                _fctb.Visible = true;
            }
            ActiveControl = _fctb;
        }

        private void OnScrollBarInvalidate(object sender, ScrollEventArgs scrollEventArgs) {
            Invalidate();
        }

        private void OnScrollBarUpdate(object sender, ScrollEventArgs scrollEventArgs) {
            _fctb.OnScroll(scrollEventArgs, true);
            Invalidate();
        }
        private void OnInternalTextBoxScrollbarUpdate(object sender, EventArgs eventArgs) {
            // update max/value
            _verticScrollBar.Maximum = _fctb.VerticalScroll.Maximum;
            _verticScrollBar.Value = _fctb.VerticalScroll.Value;
            _horizScrollBar.Maximum = _fctb.HorizontalScroll.Maximum;
            _horizScrollBar.Value = _fctb.HorizontalScroll.Value;

            // UpdateScrollBars
            UpdateScrollBars();

            Invalidate();
        }

        #endregion

        #region Resize scrollbars

        protected override void OnResize(EventArgs e) {
            _fctb.NeedRecalc(); // recalc the values for the internal scroll of the control

            // update max/value
            _verticScrollBar.Maximum = _fctb.VerticalScroll.Maximum;
            _verticScrollBar.Value = _fctb.VerticalScroll.Value;
            _horizScrollBar.Maximum = _fctb.HorizontalScroll.Maximum;
            _horizScrollBar.Value = _fctb.HorizontalScroll.Value;

            // update size/location
            _verticScrollBar.Size = new Size(ScrollBarWidth, Height - (_horizScrollBar.Visible && !_horizScrollBar.Disabled ? ScrollBarWidth : 0) - 2 * BorderWidth);
            _horizScrollBar.Size = new Size(Width - (_verticScrollBar.Visible && !_verticScrollBar.Disabled ? ScrollBarWidth : 0) - 2 * BorderWidth, ScrollBarWidth);
            _verticScrollBar.Location = new Point(Width - ScrollBarWidth - BorderWidth, BorderWidth);
            _horizScrollBar.Location = new Point(BorderWidth, Height - ScrollBarWidth - BorderWidth);

            if (!_multiLine) {
                _basePadding.Top = BorderWidth + (Height - 2 * BorderWidth - _fctb.CharHeight) / 2;
            } else {
                _basePadding.Top = BorderWidth;
            }

            UpdateScrollBars();
            
            base.OnResize(e);
        }

        private void UpdateScrollBars() {
            // thumbsize
            _verticScrollBar.UpdateThumbSize(_fctb.ClientRectangle.Height, _fctb.TextAreaRect.Height);
            _horizScrollBar.UpdateThumbSize(_fctb.ClientRectangle.Width, _fctb.TextAreaRect.Width);

            // Padding to display the bars
            base.Padding = new Padding(_basePadding.Left, _basePadding.Top, _basePadding.Right + (_verticScrollBar.Visible && !_verticScrollBar.Disabled ? ScrollBarWidth : 0), _basePadding.Bottom + (_horizScrollBar.Visible && !_horizScrollBar.Disabled ? ScrollBarWidth : 0));
        }

        #endregion

        #region Paint

        public override void Refresh() {
            ApplyColorScheme();
            UpdateColors();
            _fctb.Invalidate();
            base.Refresh();
        }

        private void ApplyColorScheme() {
            _fctb.Font = FontManager.GetFont(FontFunction.TextBox);
            _fctb.CaretColor = YamuiThemeManager.Current.ButtonNormalFore;
            _fctb.DisabledColor = YamuiThemeManager.Current.ButtonDisabledBack;
            _fctb.SelectionColor = YamuiThemeManager.Current.AccentColor;
            if (!_multiLine) {
                _basePadding.Top = BorderWidth + (Height - 2*BorderWidth - _fctb.CharHeight)/2;
            } else {
                _basePadding.Top = BorderWidth;
            }
        }

        private void UpdateColors() {
            _borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, false, Enabled);
            _backColor = YamuiThemeManager.Current.ButtonBg(BackColor, UseCustomBackColor, IsFocused, IsHovered, false, Enabled);
            _foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, UseCustomForeColor, IsFocused, IsHovered, false, Enabled);
            _fctb.BackColor = _backColor;
            _fctb.ForeColor = _foreColor;
        }

        protected override void OnPaint(PaintEventArgs e) {

            UpdateColors();
            
            // paint background
            e.Graphics.Clear(_backColor);

            // Watermark
            if (!IsFocused && string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(WaterMark) && Enabled) {
                Rectangle clientRectangle = ClientRectangle;
                clientRectangle.Offset(4, _multiLine ? 2 : 0);
                TextRenderer.DrawText(e.Graphics, WaterMark, FontManager.GetFont(FontFunction.WaterMark), clientRectangle, YamuiThemeManager.Current.ButtonWatermarkFore, FontManager.GetTextFormatFlags(_multiLine ? ContentAlignment.TopLeft : ContentAlignment.MiddleLeft));
                _fctb.Visible = false;
                _waterMarkOn = true;
            } else {
                _fctb.Visible = true;
                _waterMarkOn = false;
            }

            // border?
            if (_borderColor != Color.Transparent)
                using (var p = new Pen(_borderColor)) {
                    var borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
                    e.Graphics.DrawRectangle(p, borderRect);
                }

            // paint both scroll
            _verticScrollBar.Paint(e.Graphics, Enabled);
            _horizScrollBar.Paint(e.Graphics, Enabled);

        }

        #endregion

        #region Handle user interaactions with scrolls

        [SecuritySafeCritical]
        protected override void WndProc(ref Message message) {
            HandleWindowsProc(message);
            base.WndProc(ref message);
        }

        /// <summary>
        /// when the scroll bar is visible we listen to messages to handle the scroll
        /// </summary>
        protected void HandleWindowsProc(Message message) {
            switch (message.Msg) {
                case (int) WinApi.Messages.WM_MOUSEWHEEL:
                    var delta = (short) (message.WParam.ToInt64() >> 16);
                    var pt = PointToClient(MousePosition);

                    if (!_verticScrollBar.OnMouseWheel(pt, delta))
                        _horizScrollBar.OnMouseWheel(pt, delta);
                    break;

                case (int) WinApi.Messages.WM_LBUTTONDOWN:
                    var pt2 = PointToClient(MousePosition);

                    if (!_verticScrollBar.OnMouseDown(pt2))
                        _horizScrollBar.OnMouseDown(pt2);
                    break;

                case (int) WinApi.Messages.WM_LBUTTONUP:
                    _verticScrollBar.OnMouseUp();
                    _horizScrollBar.OnMouseUp();
                    break;

                case (int) WinApi.Messages.WM_MOUSEMOVE:
                    var pt3 = PointToClient(MousePosition);

                    _verticScrollBar.OnMouseMove(pt3);
                    _horizScrollBar.OnMouseMove(pt3);
                    break;
            }
        }

        #endregion

    }

    internal class YamuiTextBoxDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("BorderStyle");
            properties.Remove("ForeColor");
            properties.Remove("BackColor");
            properties.Remove("Font");
            base.PreFilterProperties(properties);
        }
    }
}
