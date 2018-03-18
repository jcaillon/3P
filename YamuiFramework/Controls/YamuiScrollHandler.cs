using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    
    public class YamuiScrollHandler {

        public event Action<YamuiScrollHandler, int, int> OnValueChange;

        public bool IsVertical { get; }

        public int ThumbPadding { get; set; } = 2;

        public bool Enabled { get; set; } = true;

        public int BarThickness { get; set; } = 15;

        public int SmallChange {
            get {
                return _smallChange == 0 ? LengthAvailable / 10 : _smallChange;
            }
            set { _smallChange = value; }
        }

        public int LargeChange {
            get {
                return _largeChange == 0 ? LengthAvailable / 2 : _largeChange;
            }
            set { _largeChange = value; }
        }

        public Padding Padding { get; set; } = new Padding(0);

        /// <summary>
        /// Exposes the states of the scroll bars, true if they are displayed
        /// </summary>
        public bool HasScroll { get; private set; }

        /// <summary>
        /// Maximum length of this panel if we wanted to show it all w/o scrolls
        /// </summary>
        public int LengthToRepresent { get; private set; }

        /// <summary>
        /// Maximum length really available to show the content
        /// </summary>
        public int LengthAvailable { get; private set; }

        public bool UpdateLength(int lengthToRepresent, int lengthAvailable) {

            LengthToRepresent = lengthToRepresent; 
            LengthAvailable = lengthAvailable; 

            // if the content is not too tall, no need to display the scroll bars
            if (MaximumValue <= 0 || !Enabled) {
                HasScroll = false;
                Value = 0;
            } else {
                HasScroll = true;
                Value = Value;
            }

            return HasScroll;
        } 

        public int Value {
            get { return _value; }
            set {
                var previousValue = _value;
                _value = value.Clamp(MinimumValue, MaximumValue);
                InvalidateScrollBar();
                OnValueChange?.Invoke(this, previousValue, _value);
            }
        }

        [Browsable(false)]
        public double ValuePercent {
            get { return (double) Value / MaximumValue; }
            set { Value = (int) (MaximumValue * value); }
        }
        
        public const int MinimumValue = 0;

        public int MaximumValue => LengthToRepresent - LengthAvailable;
        
        private int ParentLenght => IsVertical ? _parent.Height : _parent.Width;
        
        public int BarOffset => IsVertical ? Padding.Top : Padding.Left;

        public int BarOpposedOffset => (IsVertical ? _parent.Width - Padding.Right : _parent.Height - Padding.Bottom) - BarThickness;

        public int BarLength => ParentLenght - (IsVertical ? Padding.Vertical : Padding.Horizontal);

        public Rectangle BarRect => IsVertical ? 
            new Rectangle(BarOpposedOffset, BarOffset, BarThickness, BarLength) : 
            new Rectangle(BarOffset, BarOpposedOffset, BarLength, BarThickness);
        
        private int BarScrollSpace => BarLength - ThumbLenght - ThumbPadding * 2;

        public int ThumbLenght => ((int) Math.Floor((BarLength - ThumbPadding * 2) * ((double) LengthAvailable / LengthToRepresent))).ClampMin(BarThickness);

        public int ThumbThickness => BarThickness - ThumbPadding * 2;

        public Rectangle ThumbRect => IsVertical ? 
            new Rectangle(BarOpposedOffset + ThumbPadding, BarOffset + ThumbPadding + (int) (BarScrollSpace * ValuePercent), ThumbThickness, ThumbLenght) : 
            new Rectangle(BarOffset + ThumbPadding + (int) (BarScrollSpace * ValuePercent), BarOpposedOffset + ThumbPadding, ThumbLenght, ThumbThickness);

        public bool IsPressed {
            get { return _isPressed; }
            set {
                if (_isPressed != value)
                    InvalidateScrollBar();
                _isPressed = value;
            }
        }

        public bool IsHovered {
            get { return _isHovered; }
            set {
                if (_isHovered != value)
                    InvalidateScrollBar();
                _isHovered = value;
            }
        }

        private int _value;

        private Control _parent;
        
        private bool _isPressed;
        private bool _isHovered;

        private int _mouseMoveInThumbPosition;
        private int _smallChange;
        private int _largeChange;

        public YamuiScrollHandler(bool isVertical, Control parent) {
            IsVertical = isVertical;
            _parent = parent;
        }

        #region Paint

        public void Paint(PaintEventArgs e) {
            if (!HasScroll)
                return;
            
            Color thumbColor = YamuiThemeManager.Current.ScrollBarsFg(false, IsHovered, IsPressed, _parent.Enabled);
            Color barColor = YamuiThemeManager.Current.ScrollBarsBg(false, IsHovered, IsPressed, _parent.Enabled);

            if (barColor != Color.Transparent) {
                using (var b = new SolidBrush(barColor)) {
                    e.Graphics.FillRectangle(b, BarRect);
                }
            }

            using (var b = new SolidBrush(thumbColor)) {
                e.Graphics.FillRectangle(b, ThumbRect);
            }
        }
        
        #endregion
        
        /// <summary>
        /// Redraw the scrollbar
        /// </summary>
        private void InvalidateScrollBar() {
            _parent.Invalidate();
        }

        /// <summary>
        /// move the thumb
        /// </summary>
        private void MoveThumb(int newThumbPos) {
            if (IsVertical) {
                ValuePercent = (double) (newThumbPos - ThumbPadding) / BarScrollSpace;
            } else {
                ValuePercent = (double) (newThumbPos - ThumbPadding) / BarScrollSpace;
            }
        }

        public void HandleWindowsProc(Message message) {
            if (!HasScroll)
                return;

            switch (message.Msg) {
                case (int) WinApi.Messages.WM_MOUSEWHEEL:
                    // delta negative when scrolling up
                    var delta = -(short) (message.WParam.ToInt64() >> 16);
                    Value += Math.Sign(delta) * LengthAvailable / 2;
                    break;

                case (int) WinApi.Messages.WM_LBUTTONDOWN:
                    var mousePosRelativeToThis = _parent.PointToClient(Cursor.Position);

                    // mouse in scrollbar
                    if (BarRect.Contains(mousePosRelativeToThis)) {
                        var thumbRect = ThumbRect;
                        if (IsVertical) {
                            thumbRect.X -= ThumbPadding;
                            thumbRect.Width += ThumbPadding * 2;
                        } else {
                            thumbRect.Y -= ThumbPadding;
                            thumbRect.Height += ThumbPadding * 2;
                        }

                        // mouse in thumb
                        if (thumbRect.Contains(mousePosRelativeToThis)) {
                            IsPressed = true;
                            _mouseMoveInThumbPosition = IsVertical ? mousePosRelativeToThis.Y - thumbRect.Y : mousePosRelativeToThis.X - thumbRect.X;
                        } else {
                            MoveThumb(IsVertical ? mousePosRelativeToThis.Y : mousePosRelativeToThis.X);
                        }
                    }

                    break;

                case (int) WinApi.Messages.WM_LBUTTONUP:
                    if (IsPressed) {
                        IsPressed = false;
                    }

                    break;

                case (int) WinApi.Messages.WM_MOUSEMOVE:
                    // hover thumb
                    var mousePosRelativeToThis2 = _parent.PointToClient(Cursor.Position);
                    if (ThumbRect.Contains(mousePosRelativeToThis2)) {
                        IsHovered = true;
                    } else {
                        if (IsHovered) {
                            IsHovered = false;
                        }
                    }

                    // move thumb
                    if (IsPressed) {
                        MoveThumb(IsVertical ? mousePosRelativeToThis2.Y - _mouseMoveInThumbPosition : mousePosRelativeToThis2.X - _mouseMoveInThumbPosition);
                    }

                    break;

                case (int) WinApi.Messages.WM_KEYDOWN:
                case (int) WinApi.Messages.WM_IME_KEYDOWN:
                    // need the parent control to override OnPreviewKeyDown or IsInputKey

                    var key = (Keys) (message.WParam.ToInt64());
                    long context = message.LParam.ToInt64();

                    // on key down
                    if (!IsBitSet(context, 31)) {
                        if (IsVertical) {
                            if (key == Keys.Up) {
                                Value -= SmallChange;
                            } else if (key == Keys.Down) {
                                Value += SmallChange;
                            } else if (key == Keys.PageUp) {
                                Value -= LargeChange;
                            } else if (key == Keys.PageDown) {
                                Value += LargeChange;
                            } else if (key == Keys.End) {
                                Value = MaximumValue;
                            } else if (key == Keys.Home) {
                                Value = MinimumValue;
                            }
                        } else {
                            if (key == Keys.Left) {
                                Value -= SmallChange;
                            } else if (key == Keys.Right) {
                                Value += SmallChange;
                            }
                        }

                    }

                    break;
            }
        }

        /// <summary>
        /// Returns true if the bit at the given position is set to true
        /// </summary>
        private static bool IsBitSet(long b, int pos) {
            return (b & (1 << pos)) != 0;
        }
        
    }
    
}