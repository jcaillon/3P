using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    #region Enums

    public enum ScrollOrientation {
        Horizontal,
        Vertical
    }

    #endregion

    [Designer("YamuiFramework.Controls.YamuiScrollBarDesigner")]
    [DefaultEvent("Scroll")]
    [DefaultProperty("Value")]
    public class YamuiScrollBar : Control {
        #region Events

        public event ScrollEventHandler Scroll;

        private void OnScroll(ScrollEventType type, int oldValue, int newValue, System.Windows.Forms.ScrollOrientation orientation) {
            if (Scroll == null) return;

            if (orientation == System.Windows.Forms.ScrollOrientation.HorizontalScroll) {
                if (type != ScrollEventType.EndScroll && _isFirstScrollEventHorizontal) {
                    type = ScrollEventType.First;
                } else if (!_isFirstScrollEventHorizontal && type == ScrollEventType.EndScroll) {
                    _isFirstScrollEventHorizontal = true;
                }
            } else {
                if (type != ScrollEventType.EndScroll && _isFirstScrollEventVertical) {
                    type = ScrollEventType.First;
                } else if (!_isFirstScrollEventHorizontal && type == ScrollEventType.EndScroll) {
                    _isFirstScrollEventVertical = true;
                }
            }

            Scroll(this, new ScrollEventArgs(type, oldValue, newValue, orientation));
        }

        #endregion

        #region Fields

        private bool _isFirstScrollEventVertical = true;
        private bool _isFirstScrollEventHorizontal = true;

        private bool _inUpdate;

        private Rectangle _clickedBarRectangle;
        private Rectangle _thumbRectangle;

        private bool _topBarClicked;
        private bool _bottomBarClicked;
        private bool _thumbClicked;

        private int _thumbWidth = 6;
        private int _thumbHeight;

        private int _thumbBottomLimitBottom;
        private int _thumbBottomLimitTop;
        private int _thumbTopLimit;
        private int _thumbPosition;

        private int _trackPosition;

        private readonly Timer _progressTimer = new Timer();

        private int _mouseWheelBarPartitions = 10;

        public int MouseWheelBarPartitions {
            get { return _mouseWheelBarPartitions; }
            set {
                if (value > 0) {
                    _mouseWheelBarPartitions = value;
                } else {
                    throw new ArgumentOutOfRangeException("value", "MouseWheelBarPartitions has to be greather than zero");
                }
            }
        }

        private bool _isHovered;
        private bool _isPressed;

        [Category("Yamui")]
        public int ScrollbarSize {
            get { return Orientation == ScrollOrientation.Vertical ? Width : Height; }
            set {
                if (Orientation == ScrollOrientation.Vertical)
                    Width = value;
                else
                    Height = value;
            }
        }

        private bool _highlightOnWheel;
        [DefaultValue(false)]
        [Category("Yamui")]
        public bool HighlightOnWheel {
            get { return _highlightOnWheel; }
            set { _highlightOnWheel = value; }
        }

        private ScrollOrientation _orientation = ScrollOrientation.Vertical;
        private System.Windows.Forms.ScrollOrientation _scrollOrientation = System.Windows.Forms.ScrollOrientation.VerticalScroll;

        public ScrollOrientation Orientation {
            get { return _orientation; }

            set {
                if (value == _orientation) {
                    return;
                }

                _orientation = value;

                if (value == ScrollOrientation.Vertical) {
                    _scrollOrientation = System.Windows.Forms.ScrollOrientation.VerticalScroll;
                } else {
                    _scrollOrientation = System.Windows.Forms.ScrollOrientation.HorizontalScroll;
                }

                Size = new Size(Height, Width);
                SetupScrollBar();
            }
        }

        private int _minimum;
        private int _maximum = 100;
        private int _smallChange = 1;
        private int _largeChange = 10;
        private int _curValue;

        public int Minimum {
            get { return _minimum; }
            set {
                if (_minimum == value || value < 0 || value >= _maximum) {
                    return;
                }

                _minimum = value;
                if (_curValue < value) {
                    _curValue = value;
                }

                if (_largeChange > (_maximum - _minimum)) {
                    _largeChange = _maximum - _minimum;
                }

                SetupScrollBar();

                if (_curValue < value) {
                    _dontUpdateColor = true;
                    Value = value;
                } else {
                    ChangeThumbPosition(GetThumbPosition());
                    Refresh();
                }
            }
        }

        public int Maximum {
            get { return _maximum; }
            set {
                if (value == _maximum || value < 1 || value <= _minimum) {
                    return;
                }

                _maximum = value;
                if (_largeChange > (_maximum - _minimum)) {
                    _largeChange = _maximum - _minimum;
                }

                SetupScrollBar();

                if (_curValue > value) {
                    _dontUpdateColor = true;
                    Value = _maximum;
                } else {
                    ChangeThumbPosition(GetThumbPosition());
                    Refresh();
                }
            }
        }

        [DefaultValue(1)]
        public int SmallChange {
            get { return _smallChange; }
            set {
                if (value == _smallChange || value < 1 || value >= _largeChange) {
                    return;
                }

                _smallChange = value;
                SetupScrollBar();
            }
        }

        [DefaultValue(5)]
        public int LargeChange {
            get { return _largeChange; }
            set {
                if (value == _largeChange || value < _smallChange || value < 2) {
                    return;
                }

                if (value > (_maximum - _minimum)) {
                    _largeChange = _maximum - _minimum;
                } else {
                    _largeChange = value;
                }

                SetupScrollBar();
            }
        }

        private bool _dontUpdateColor;

        [DefaultValue(0)]
        [Browsable(false)]
        public int Value {
            get { return _curValue; }

            set {
                if (_curValue == value || value < _minimum || value > _maximum) {
                    return;
                }

                _curValue = value;

                ChangeThumbPosition(GetThumbPosition());

                OnScroll(ScrollEventType.ThumbPosition, -1, value, _scrollOrientation);

                if (!_dontUpdateColor && _highlightOnWheel) {
                    if (!_isHovered)
                        _isHovered = true;

                    if (_autoHoverTimer == null) {
                        _autoHoverTimer = new Timer();
                        _autoHoverTimer.Interval = 1000;
                        _autoHoverTimer.Tick += autoHoverTimer_Tick;
                        _autoHoverTimer.Start();
                    } else {
                        _autoHoverTimer.Stop();
                        _autoHoverTimer.Start();
                    }
                } else {
                    _dontUpdateColor = false;
                }

                Refresh();
            }
        }

        private void autoHoverTimer_Tick(object sender, EventArgs e) {
            _isHovered = false;
            Invalidate();
            _autoHoverTimer.Stop();
        }

        private Timer _autoHoverTimer;

        #endregion

        #region Constructor

        public YamuiScrollBar() {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint, true);

            Width = 10;
            Height = 200;

            SetupScrollBar();

            _progressTimer.Interval = 20;
            _progressTimer.Tick += ProgressTimerTick;
        }

        public YamuiScrollBar(ScrollOrientation orientation)
            : this() {
            Orientation = orientation;
        }

        public YamuiScrollBar(ScrollOrientation orientation, int width)
            : this(orientation) {
            Width = width;
        }

        public bool HitTest(Point point) {
            return _thumbRectangle.Contains(point);
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
            Color thumbColor = ThemeManager.ScrollBarsColors.ForeGround(false, _isHovered, _isPressed, Enabled);
            Color barColor = ThemeManager.ScrollBarsColors.BackGround(false, _isHovered, _isPressed, Enabled);
            DrawScrollBar(e.Graphics, thumbColor, barColor);
        }

        private void DrawScrollBar(Graphics g, Color thumbColor, Color barColor) {
            if (barColor != Color.Transparent) {
                using (var b = new SolidBrush(barColor)) {
                    g.FillRectangle(b, ClientRectangle);
                }
            }

            using (var b = new SolidBrush(thumbColor)) {
                var thumbRect = new Rectangle(_thumbRectangle.X + 2, _thumbRectangle.Y + 2, _thumbRectangle.Width - 4, _thumbRectangle.Height - 4);
                g.FillRectangle(b, thumbRect);
            }
        }

        #endregion

        #region Update Methods

        [SecuritySafeCritical]
        public void BeginUpdate() {
            WinApi.SendMessage(Handle, (int)WinApi.Messages.WM_SETREDRAW, false, 0);
            _inUpdate = true;
        }

        [SecuritySafeCritical]
        public void EndUpdate() {
            WinApi.SendMessage(Handle, (int)WinApi.Messages.WM_SETREDRAW, true, 0);
            _inUpdate = false;
            SetupScrollBar();
            Refresh();
        }

        #endregion

        #region Managing pressed, hover, focus

        #region Focus Methods

        protected override void OnGotFocus(EventArgs e) {
            Invalidate();

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e) {
            _isHovered = false;
            _isPressed = false;
            Invalidate();

            base.OnLostFocus(e);
        }

        protected override void OnEnter(EventArgs e) {
            Invalidate();

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            _isHovered = false;
            _isPressed = false;
            Invalidate();

            base.OnLeave(e);
        }

        #endregion

        #region Mouse Methods

        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);

            int v = e.Delta/120*(_maximum - _minimum)/_mouseWheelBarPartitions;

            if (Orientation == ScrollOrientation.Vertical) {
                Value -= v;
            } else {
                Value += v;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                _isPressed = true;
                Invalidate();
            }

            base.OnMouseDown(e);

            Focus();

            if (e.Button == MouseButtons.Left) {

                var mouseLocation = e.Location;

                if (_thumbRectangle.Contains(mouseLocation)) {
                    _thumbClicked = true;
                    _thumbPosition = _orientation == ScrollOrientation.Vertical ? mouseLocation.Y - _thumbRectangle.Y : mouseLocation.X - _thumbRectangle.X;

                    Invalidate(_thumbRectangle);
                } else {
                    _trackPosition = _orientation == ScrollOrientation.Vertical ? mouseLocation.Y : mouseLocation.X;

                    if (_trackPosition < (_orientation == ScrollOrientation.Vertical ? _thumbRectangle.Y : _thumbRectangle.X)) {
                        _topBarClicked = true;
                    } else {
                        _bottomBarClicked = true;
                    }

                    ProgressThumb(true);
                }
            } else if (e.Button == MouseButtons.Right) {
                _trackPosition = _orientation == ScrollOrientation.Vertical ? e.Y : e.X;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            _isPressed = false;

            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left) {
                if (_thumbClicked) {
                    _thumbClicked = false;
                    OnScroll(ScrollEventType.EndScroll, -1, _curValue, _scrollOrientation);
                } else if (_topBarClicked) {
                    _topBarClicked = false;
                    StopTimer();
                } else if (_bottomBarClicked) {
                    _bottomBarClicked = false;
                    StopTimer();
                }

                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e) {
            _isHovered = true;
            Invalidate();

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            _isHovered = false;
            Invalidate();

            base.OnMouseLeave(e);

            ResetScrollStatus();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left) {
                if (_thumbClicked) {
                    int oldScrollValue = _curValue;

                    int pos = _orientation == ScrollOrientation.Vertical ? e.Location.Y : e.Location.X;
                    int thumbSize = _orientation == ScrollOrientation.Vertical ? (pos/Height)/_thumbHeight : (pos/Width)/_thumbWidth;

                    if (pos <= (_thumbTopLimit + _thumbPosition)) {
                        ChangeThumbPosition(_thumbTopLimit);
                        _curValue = _minimum;
                        Invalidate();
                    } else if (pos >= (_thumbBottomLimitTop + _thumbPosition)) {
                        ChangeThumbPosition(_thumbBottomLimitTop);
                        _curValue = _maximum;
                        Invalidate();
                    } else {
                        ChangeThumbPosition(pos - _thumbPosition);

                        int pixelRange, thumbPos;

                        if (Orientation == ScrollOrientation.Vertical) {
                            pixelRange = Height - thumbSize;
                            thumbPos = _thumbRectangle.Y;
                        } else {
                            pixelRange = Width - thumbSize;
                            thumbPos = _thumbRectangle.X;
                        }

                        float perc = 0f;

                        if (pixelRange != 0) {
                            perc = (thumbPos)/(float) pixelRange;
                        }

                        _curValue = Convert.ToInt32((perc*(_maximum - _minimum)) + _minimum);
                    }

                    if (oldScrollValue != _curValue) {
                        OnScroll(ScrollEventType.ThumbTrack, oldScrollValue, _curValue, _scrollOrientation);
                        Refresh();
                    }
                }
            } else if (!ClientRectangle.Contains(e.Location)) {
                ResetScrollStatus();
            } else if (e.Button == MouseButtons.None) {
                if (_thumbRectangle.Contains(e.Location)) {
                    Invalidate(_thumbRectangle);
                } else if (ClientRectangle.Contains(e.Location)) {
                    Invalidate();
                }
            }
        }

        #endregion

        #region Keyboard Methods

        protected override void OnKeyDown(KeyEventArgs e) {
            _isHovered = true;
            _isPressed = true;
            Invalidate();

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            _isHovered = false;
            _isPressed = false;
            Invalidate();

            base.OnKeyUp(e);
        }

        #endregion

        #endregion

        #region Management Methods

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
            base.SetBoundsCore(x, y, width, height, specified);

            if (DesignMode) {
                SetupScrollBar();
            }
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            SetupScrollBar();
        }

        protected override bool ProcessDialogKey(Keys keyData) {
            var keyUp = Keys.Up;
            var keyDown = Keys.Down;

            if (Orientation == ScrollOrientation.Horizontal) {
                keyUp = Keys.Left;
                keyDown = Keys.Right;
            }

            if (keyData == keyUp) {
                Value -= _smallChange;

                return true;
            }

            if (keyData == keyDown) {
                Value += _smallChange;

                return true;
            }

            if (keyData == Keys.PageUp) {
                Value = GetValue(false, true);

                return true;
            }

            if (keyData == Keys.PageDown) {
                if (_curValue + _largeChange > _maximum) {
                    Value = _maximum;
                } else {
                    Value += _largeChange;
                }

                return true;
            }

            if (keyData == Keys.Home) {
                Value = _minimum;

                return true;
            }

            if (keyData == Keys.End) {
                Value = _maximum;

                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        private void SetupScrollBar() {
            if (_inUpdate) return;

            if (Orientation == ScrollOrientation.Vertical) {
                _thumbWidth = Width > 0 ? Width : 10;
                _thumbHeight = GetThumbSize();

                _clickedBarRectangle = ClientRectangle;
                _clickedBarRectangle.Inflate(-1, -1);

                _thumbRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y, _thumbWidth, _thumbHeight);

                _thumbPosition = _thumbRectangle.Height / 2;
                _thumbBottomLimitBottom = ClientRectangle.Bottom;
                _thumbBottomLimitTop = _thumbBottomLimitBottom - _thumbRectangle.Height;
                _thumbTopLimit = ClientRectangle.Y;
            } else {
                _thumbHeight = Height > 0 ? Height : 10;
                _thumbWidth = GetThumbSize();

                _clickedBarRectangle = ClientRectangle;
                _clickedBarRectangle.Inflate(-1, -1);

                _thumbRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y, _thumbWidth, _thumbHeight);

                _thumbPosition = _thumbRectangle.Width / 2;
                _thumbBottomLimitBottom = ClientRectangle.Right;
                _thumbBottomLimitTop = _thumbBottomLimitBottom - _thumbRectangle.Width;
                _thumbTopLimit = ClientRectangle.X;
            }

            ChangeThumbPosition(GetThumbPosition());

            Refresh();
        }

        private void ResetScrollStatus() {
            _bottomBarClicked = _topBarClicked = false;

            StopTimer();
            Refresh();
        }

        private void ProgressTimerTick(object sender, EventArgs e) {
            ProgressThumb(true);
        }

        private int GetValue(bool smallIncrement, bool up) {
            int newValue;

            if (up) {
                newValue = _curValue - (smallIncrement ? _smallChange : _largeChange);

                if (newValue < _minimum) {
                    newValue = _minimum;
                }
            } else {
                newValue = _curValue + (smallIncrement ? _smallChange : _largeChange);

                if (newValue > _maximum) {
                    newValue = _maximum;
                }
            }

            return newValue;
        }

        private int GetThumbPosition() {
            int pixelRange;

            if (_thumbHeight == 0 || _thumbWidth == 0) {
                return 0;
            }

            int thumbSize = _orientation == ScrollOrientation.Vertical ? (_thumbPosition / Height) / _thumbHeight : (_thumbPosition / Width) / _thumbWidth;

            if (Orientation == ScrollOrientation.Vertical) {
                pixelRange = Height - thumbSize;
            } else {
                pixelRange = Width - thumbSize;
            }

            int realRange = _maximum - _minimum;
            float perc = 0f;

            if (realRange != 0) {
                perc = (_curValue - (float)_minimum) / realRange;
            }

            return Math.Max(_thumbTopLimit, Math.Min(_thumbBottomLimitTop, Convert.ToInt32((perc * pixelRange))));
        }

        private int GetThumbSize() {
            int trackSize =
                _orientation == ScrollOrientation.Vertical ?
                    Height : Width;

            if (_maximum == 0 || _largeChange == 0) {
                return trackSize;
            }

            float newThumbSize = (_largeChange * (float)trackSize) / _maximum;

            return Convert.ToInt32(Math.Min(trackSize, Math.Max(newThumbSize, 10f)));
        }

        private void EnableTimer() {
            if (!_progressTimer.Enabled) {
                _progressTimer.Interval = 600;
                _progressTimer.Start();
            } else {
                _progressTimer.Interval = 10;
            }
        }

        private void StopTimer() {
            _progressTimer.Stop();
        }

        private void ChangeThumbPosition(int position) {
            if (Orientation == ScrollOrientation.Vertical) {
                _thumbRectangle.Y = position;
            } else {
                _thumbRectangle.X = position;
            }
        }

        private void ProgressThumb(bool enableTimer) {
            int scrollOldValue = _curValue;
            var type = ScrollEventType.First;
            int thumbSize, thumbPos;

            if (Orientation == ScrollOrientation.Vertical) {
                thumbPos = _thumbRectangle.Y;
                thumbSize = _thumbRectangle.Height;
            } else {
                thumbPos = _thumbRectangle.X;
                thumbSize = _thumbRectangle.Width;
            }

            if ((_bottomBarClicked && (thumbPos + thumbSize) < _trackPosition)) {
                type = ScrollEventType.LargeIncrement;

                _curValue = GetValue(false, false);

                if (_curValue == _maximum) {
                    ChangeThumbPosition(_thumbBottomLimitTop);

                    type = ScrollEventType.Last;
                } else {
                    ChangeThumbPosition(Math.Min(_thumbBottomLimitTop, GetThumbPosition()));
                }
            } else if ((_topBarClicked && thumbPos > _trackPosition)) {
                type = ScrollEventType.LargeDecrement;

                _curValue = GetValue(false, true);

                if (_curValue == _minimum) {
                    ChangeThumbPosition(_thumbTopLimit);

                    type = ScrollEventType.First;
                } else {
                    ChangeThumbPosition(Math.Max(_thumbTopLimit, GetThumbPosition()));
                }
            }

            if (scrollOldValue != _curValue) {
                OnScroll(type, scrollOldValue, _curValue, _scrollOrientation);

                Invalidate();

                if (enableTimer) {
                    EnableTimer();
                }
            }
        }

        #endregion
    }


    [Designer(typeof(ScrollableControlDesigner), typeof(ParentControlDesigner))]
    internal class YamuiScrollBarDesigner : ControlDesigner {
        public override SelectionRules SelectionRules {
            get {
                PropertyDescriptor propDescriptor = TypeDescriptor.GetProperties(Component)["Orientation"];

                if (propDescriptor != null) {
                    var value = propDescriptor.GetValue(Component);
                    if (value != null) {
                        ScrollOrientation orientation = (ScrollOrientation)value;

                        if (orientation == ScrollOrientation.Vertical) {
                            return SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.BottomSizeable | SelectionRules.TopSizeable;
                        }
                    }

                    return SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.LeftSizeable | SelectionRules.RightSizeable;
                }

                return base.SelectionRules;
            }
        }

        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("Text");
            properties.Remove("BackgroundImage");
            properties.Remove("ForeColor");
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("BackColor");
            properties.Remove("Font");
            properties.Remove("RightToLeft");

            base.PreFilterProperties(properties);
        }
    }
}