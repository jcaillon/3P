using System;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    
    public class YamuiScrollHandler {

        /// <summary>
        /// Action that will be triggered when the value of the scroll changes : this, oldValue, newValue
        /// </summary>
        public event Action<YamuiScrollHandler, int, int> OnValueChange;

        /// <summary>
        /// Read-only, is the scrollbar vertical
        /// </summary>
        public bool IsVertical { get; }

        /// <summary>
        /// Padding for the thumb within the bar
        /// </summary>
        public int ThumbPadding {
            get { return _thumbPadding; }
            set {
                _thumbPadding = value;
                InvalidateScrollBar();
            }
        }

        /// <summary>
        /// Is this scrollbar enabled
        /// </summary>
        public bool Enabled {
            get { return _enabled; }
            set {
                _enabled = value;
                AnalyzeScrollNeeded();
            }
        }

        /// <summary>
        /// Scrollbar thickness
        /// </summary>
        public int BarThickness {
            get { return _barThickness; }
            set {
                _barThickness = value;
                InvalidateScrollBar();
            }
        }

        /// <summary>
        /// Will be added/substracted to the Value when using directional keys
        /// </summary>
        public int SmallChange {
            get { return _smallChange == 0 ? LengthAvailable / 10 : _smallChange; }
            set { _smallChange = value; }
        }

        /// <summary>
        /// Will be added/substracted to the Value when scrolling or page up/down
        /// </summary>
        public int LargeChange {
            get { return _largeChange == 0 ? LengthAvailable / 2 : _largeChange; }
            set { _largeChange = value; }
        }

        /// <summary>
        /// Extra padding to apply on the right (for horizontal) / bottom (for vertical) to take into account
        /// the fact that the 2 scrollbars are displayed
        /// </summary>
        public int ExtraEndPadding {
            get { return _extraEndPadding; }
            set {
                if (_extraEndPadding != value) {
                    _extraEndPadding = value;
                    InvalidateScrollBar();
                } else {
                    _extraEndPadding = value;
                }
            }
        }

        /// <summary>
        /// Exposes the states of the scroll bars, true if they are displayed
        /// </summary>
        public bool HasScroll { get; private set; }

        public bool HasScrollButtons { get; private set; }

        /// <summary>
        /// Maximum length of this panel if we wanted to show it all w/o scrolls (set with <see cref="UpdateLength"/>)
        /// </summary>
        public int LengthToRepresent { get; private set; }

        /// <summary>
        /// Maximum length really available to show the content (set with <see cref="UpdateLength"/>)
        /// </summary>
        public int LengthAvailable { get; private set; }

        /// <summary>
        /// Update the length to represent as well as the length really available.
        /// This effectively defines the ratio between thumb length and bar length. 
        /// Returns whether or not the scrollbar is needed
        /// </summary>
        /// <param name="lengthToRepresent"></param>
        /// <param name="lengthAvailable"></param>
        /// <returns></returns>
        public bool UpdateLength(int lengthToRepresent, int lengthAvailable) {
            LengthToRepresent = lengthToRepresent; 
            LengthAvailable = lengthAvailable;
            AnalyzeScrollNeeded();
            return HasScroll;
        } 

        /// <summary>
        /// Represents the current scroll value, limited by <see cref="MinimumValue"/> and by <see cref="MaximumValue"/>
        /// </summary>
        public int Value {
            get { return _value; }
            set {
                var previousValue = _value;
                _value = value.Clamp(MinimumValue, MaximumValue);
                InvalidateScrollBar();
                OnValueChange?.Invoke(this, previousValue, _value);
            }
        }
        
        /// <summary>
        /// The scroll value but represented in percent
        /// </summary>
        public double ValuePercent {
            get { return (double) Value / MaximumValue; }
            set { Value = (int) (MaximumValue * value); }
        }

        /// <summary>
        /// Is the thumb pressed
        /// </summary>
        public bool IsPressed {
            get { return _isPressed; }
            private set {
                if (_isPressed != value) {
                    _isPressed = value;
                    InvalidateScrollBar();
                } else {
                    _isPressed = value;
                }
            }
        }

        /// <summary>
        /// Is the mouse flying over the thumb
        /// </summary>
        public bool IsHovered {
            get { return _isHovered; }
            private set {
                if (_isHovered != value) {
                    _isHovered = value;
                    InvalidateScrollBar();
                } else {
                    _isHovered = value;
                }
            }
        }

        /// <summary>
        /// Is the mouse flying over the bar
        /// </summary>
        public bool IsActive { get; private set; }
        
        public const int MinimumValue = 0;

        public int MaximumValue => LengthToRepresent - LengthAvailable;

        protected bool CanDisplayThumb => BarScrollSpace > 0;

        protected int ScrollButtonSize => BarThickness;
        
        private const int BarOffset = 0;

        protected int ThumbOffset => BarOffset + (HasScrollButtons ? ScrollButtonSize : 0) + ThumbPadding;

        private int BarOpposedOffset => (IsVertical ? _parent.Width : _parent.Height) - BarThickness;

        private int ThumbThickness => BarThickness - ThumbPadding * 2;

        protected int BarLength => (IsVertical ? _parent.Height : _parent.Width) - ExtraEndPadding;

        // The total space available to move the thumb in the bar
        protected int BarScrollSpace => BarLength - (HasScrollButtons ? 2 * ScrollButtonSize : 0) - ThumbLenght - ThumbPadding * 2;
        
        private int ThumbLenght => ((int) Math.Floor((BarLength - ThumbPadding * 2) * ((double) LengthAvailable / LengthToRepresent))).ClampMin(BarThickness);
        
        protected Rectangle ScrollBottomUp => IsVertical ? 
            new Rectangle(BarOpposedOffset, BarOffset, ScrollButtonSize, ScrollButtonSize) : 
            new Rectangle(BarOffset, BarOpposedOffset, ScrollButtonSize, ScrollButtonSize);

        protected Rectangle ScrollBottomDown => IsVertical ? 
            new Rectangle(BarOpposedOffset, BarLength - ScrollButtonSize, ScrollButtonSize, ScrollButtonSize) : 
            new Rectangle(BarLength - ScrollButtonSize, BarOpposedOffset, ScrollButtonSize, ScrollButtonSize);

        // represents the bar rectangle (that will be painted)
        protected Rectangle BarRect => IsVertical ? 
            new Rectangle(BarOpposedOffset, BarOffset, BarThickness, BarLength) : 
            new Rectangle(BarOffset, BarOpposedOffset, BarLength, BarThickness);
        
        // represents the thumb rectangle (that will be painted)
        protected Rectangle ThumbRect => IsVertical ? 
            new Rectangle(BarOpposedOffset + ThumbPadding, ThumbOffset + (int) (BarScrollSpace * ValuePercent), ThumbThickness, ThumbLenght) : 
            new Rectangle(ThumbOffset + (int) (BarScrollSpace * ValuePercent), BarOpposedOffset + ThumbPadding, ThumbLenght, ThumbThickness);

        private int _value;
        private Control _parent;
        private bool _isPressed;
        private bool _isHovered;
        private int _mouseMoveInThumbPosition;
        private int _smallChange;
        private int _largeChange;
        private bool _enabled = true;
        private int _thumbPadding = 2;
        private int _barThickness = 15;
        private int _extraEndPadding;

        public YamuiScrollHandler(bool isVertical, Control parent) {
            IsVertical = isVertical;
            _parent = parent;
        }

        #region Paint

        /// <summary>
        /// Paint the scroll bar in the parent client rectangle
        /// </summary>
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

            if (CanDisplayThumb) {
                using (var b = new SolidBrush(thumbColor)) {
                    e.Graphics.FillRectangle(b, ThumbRect);
                }
            }

            if (HasScrollButtons) {
                using (var b = new SolidBrush(IsHovered ? Color.Aqua : Color.Yellow)) {
                    e.Graphics.FillRectangle(b, ScrollBottomUp);
                    e.Graphics.FillRectangle(b, ScrollBottomDown);
                }
            }
        }
        
        #endregion

        #region private

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
                ValuePercent = (double) (newThumbPos - ThumbOffset) / BarScrollSpace;
            } else {
                ValuePercent = (double) (newThumbPos - ThumbOffset) / BarScrollSpace;
            }
        }
        
        /// <summary>
        /// Returns true if the bit at the given position is set to true
        /// </summary>
        private static bool IsBitSet(long b, int pos) {
            return (b & (1 << pos)) != 0;
        }

        private void AnalyzeScrollNeeded() {
            // if the content is not too tall, no need to display the scroll bars
            if (MaximumValue <= 0 || !Enabled) {
                HasScroll = false;
                HasScrollButtons = false;
                Value = 0;
            } else {
                HasScroll = true;
                HasScrollButtons = BarLength > 3 * ScrollButtonSize;
                Value = Value;
            }
        }

        #endregion

        #region public

        /// <summary>
        /// Mouse move
        /// </summary>
        public void HandleMouseMove(MouseEventArgs e) {
            if (!HasScroll)
                return;

            // hover thumb
            var mousePosRelativeToThis = _parent.PointToClient(Cursor.Position);

            if (BarRect.Contains(mousePosRelativeToThis)) {
                IsActive = true;
                if (ThumbRect.Contains(mousePosRelativeToThis)) {
                    IsHovered = true;
                } else {
                    if (IsHovered) {
                        IsHovered = false;
                    }
                }
            } else {
                IsActive = false;
            }

            // move thumb
            if (IsPressed) {
                MoveThumb(IsVertical ? mousePosRelativeToThis.Y - _mouseMoveInThumbPosition : mousePosRelativeToThis.X - _mouseMoveInThumbPosition);
            }
        }

        /// <summary>
        /// Mouse down
        /// </summary>
        public void HandleMouseDown(MouseEventArgs e) {
            if (!HasScroll)
                return;

            if (e.Button == MouseButtons.Left) {
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
            }
        }

        /// <summary>
        /// Mouse up
        /// </summary>
        public void HandleMouseUp(MouseEventArgs e) {
            if (!HasScroll)
                return;

            if (e.Button == MouseButtons.Left && IsPressed) {
                IsPressed = false;
            }
        }

        /// <summary>
        /// Handle scroll
        /// </summary>
        public void HandleScroll(MouseEventArgs e) {
            if (!HasScroll)
                return;

            // delta negative when scrolling up
            Value += -Math.Sign(e.Delta) * LengthAvailable / 2;
        }

        /// <summary>
        /// Keydown
        /// </summary>
        public bool HandleKeyDown(KeyEventArgs e) {
            if (!HasScroll)
                return false;

            bool handled = true;

            if (IsVertical) {
                if (e.KeyCode == Keys.Up) {
                    Value -= SmallChange;
                } else if (e.KeyCode == Keys.Down) {
                    Value += SmallChange;
                } else if (e.KeyCode == Keys.PageUp) {
                    Value -= LargeChange;
                } else if (e.KeyCode == Keys.PageDown) {
                    Value += LargeChange;
                } else if (e.KeyCode == Keys.End) {
                    Value = MaximumValue;
                } else if (e.KeyCode == Keys.Home) {
                    Value = MinimumValue;
                } else {
                    handled = false;
                }
            } else {
                if (e.KeyCode == Keys.Left) {
                    Value -= SmallChange;
                } else if (e.KeyCode == Keys.Right) {
                    Value += SmallChange;
                } else {
                    handled = false;
                }
            }

            return handled;
        }

        /// <summary>
        /// Either use this global handling or individual handling above
        /// </summary>
        public bool HandleWindowsProc(Message message) {
            if (!HasScroll)
                return false;
            
            switch (message.Msg) {
                case (int) WinApi.Messages.WM_MOUSEWHEEL:
                    // delta negative when scrolling up
                    var delta = (short) (message.WParam.ToInt64() >> 16);
                    HandleScroll(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, delta));
                    break;

                case (int) WinApi.Messages.WM_LBUTTONDOWN:
                    HandleMouseDown(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                    break;

                case (int) WinApi.Messages.WM_LBUTTONUP:
                    HandleMouseUp(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                    break;

                case (int) WinApi.Messages.WM_MOUSEMOVE:
                    HandleMouseMove(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                    break;
                    
                case (int) WinApi.Messages.WM_KEYDOWN:
                    // need the parent control to override OnPreviewKeyDown or IsInputKey
                    var key = (Keys) (message.WParam.ToInt64());
                    long context = message.LParam.ToInt64();

                    // on key down
                    if (!IsBitSet(context, 31)) {
                        return HandleKeyDown(new KeyEventArgs(key));
                    }
                    break;
            }

            return false;
        }

        #endregion
        
    }
    
}