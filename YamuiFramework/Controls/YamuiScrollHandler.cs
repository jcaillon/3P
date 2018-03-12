using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    public class YamuiScrollHandler {

        public bool IsVertical { get; }

        public int ThumbPadding { get; set; } = 2;

        public bool Enabled { get; set; } = true;

        public int ScrollBarThickness { get; set; } = 15;
        
        /// <summary>
        /// Exposes the states of the scroll bars, true if they are displayed
        /// </summary>
        public bool HasScroll { get; private set; }

        private int ParentLenght => IsVertical ? _parent.Height : _parent.Width;
        
        private int ParentOpposedLenght => IsVertical ? _parent.Width : _parent.Height;
        
        /// <summary>
        /// Maximum 'height' of this panel if we wanted to show it all w/o scrolls
        /// </summary>
        public int MaximumValue { get; private set; }

        public int Value {
            get { return _value; }
            set {
                var previousValue = _value;
                _value = value.Clamp(0, MaximumValue);
                SetDisplayRectLocation(previousValue - _value);
            }
        }

        [Browsable(false)]
        public float ValuePercent {
            get { return (float) Value / MaximumValue; }
            set { Value = (int) (MaximumValue * value); }
        }

        public Rectangle BarRect => IsVertical ? 
            new Rectangle(ParentOpposedLenght - ScrollBarThickness, 0, ScrollBarThickness, ParentLenght) : 
            new Rectangle(0, ParentOpposedLenght - ScrollBarThickness, ParentLenght, ScrollBarThickness);
        
        private float BarScrollSpace => ParentLenght - ThumbLenght - ThumbPadding * 2;

        // thumb length is a ratio of displayed height and the content panel height
        public int ThumbLenght => ((int) ((ParentLenght - ThumbPadding * 2) * ((float) ParentLenght / MaximumValue))).ClampMin(ScrollBarThickness);

        public Rectangle ThumbRect => IsVertical ? 
            new Rectangle(ParentOpposedLenght - ScrollBarThickness + ThumbPadding, 0 + ThumbPadding + (int) (BarScrollSpace * ValuePercent), ScrollBarThickness - ThumbPadding * 2, ThumbLenght) : 
            new Rectangle(0 + ThumbPadding + (int) (BarScrollSpace * ValuePercent), ParentOpposedLenght - ScrollBarThickness + ThumbPadding, ThumbLenght, ScrollBarThickness - ThumbPadding * 2);

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
        /// The actual scroll magic is here
        /// </summary>
        /// <param name="deltaValue"></param>
        private void SetDisplayRectLocation(int deltaValue) {

            if (deltaValue == 0 || !HasScroll)
                return;

            InvalidateScrollBar();

            Rectangle cr = _parent.ClientRectangle;
            WinApi.RECT rcClip = WinApi.RECT.FromXYWH(cr.X, cr.Y, cr.Width - ScrollBarThickness, cr.Height);
            WinApi.RECT rcUpdate = WinApi.RECT.FromXYWH(cr.X, cr.Y, cr.Width - ScrollBarThickness, cr.Height);
            WinApi.ScrollWindowEx(
                new HandleRef(this, _parent.Handle),
                IsVertical ? 0 : deltaValue,
                IsVertical ? deltaValue : 0,
                null,
                ref rcClip,
                WinApi.NullHandleRef,
                ref rcUpdate,
                WinApi.SW_INVALIDATE
                | WinApi.SW_ERASE
                | WinApi.SW_SCROLLCHILDREN
                | WinApi.SW_SMOOTHSCROLL);

            // note : .net does an UpdateChildrenBound here but i find it is not necessary atm
            // (see SetDisplayRectLocation(0, deltaVerticalValue);)

            UpdateChildrenBound();

            _parent.Refresh(); // not critical but help reduce flickering
        }

        private void UpdateChildrenBound() {
            foreach (Control control in _parent.Controls) {
                var yamuiControl = control as IYamuiControl;
                if (yamuiControl != null && control.IsHandleCreated) {
                    yamuiControl.UpdateBoundsPublic();
                }
            }
        }

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
                ValuePercent = (newThumbPos - ThumbPadding) / BarScrollSpace;
            } else {
                ValuePercent = (newThumbPos - ThumbPadding) / BarScrollSpace;
            }
        }

        public void HandleWindowsProc(Message message) {
            if (message.Msg == (int) WinApi.Messages.WM_SHOWWINDOW) {
                UpdateChildrenBound();
            }

            if (!HasScroll)
                return;

            switch (message.Msg) {
                case (int) WinApi.Messages.WM_MOUSEWHEEL:
                    // delta negative when scrolling up
                    var delta = -(short) (message.WParam.ToInt64() >> 16);
                    Value += Math.Sign(delta) * ParentLenght / 2;
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
                    
            }
        }

        public void HandleOnLayout(LayoutEventArgs levent) {
            
            MaximumValue = 0;
            foreach (Control control in _parent.Controls) {
                int controlReach = IsVertical ? control.Top + control.Height : control.Left + control.Width;
                controlReach += Value;
                if (controlReach > MaximumValue) {
                    MaximumValue = controlReach;
                }
            }

            var prevHasScroll = HasScroll;
            MaximumValue -= ParentLenght;

            // if the content is not too tall, no need to display the scroll bars
            if (MaximumValue <= 0 || !Enabled) {
                HasScroll = false;
            } else {
                HasScroll = true;
                Value = Value;
                InvalidateScrollBar();
            }

            // add/remove padding for the scrollbar
            if (prevHasScroll != HasScroll) {
                if (IsVertical) {
                    _parent.Padding = new Padding(_parent.Padding.Left, _parent.Padding.Top, (_parent.Padding.Right + (HasScroll ? ScrollBarThickness : -ScrollBarThickness)).ClampMin(0), _parent.Padding.Bottom);
                } else {
                    _parent.Padding = new Padding(_parent.Padding.Left, _parent.Padding.Top, _parent.Padding.Right, (_parent.Padding.Right + (HasScroll ? ScrollBarThickness : -ScrollBarThickness)).ClampMin(0));
                }
            }
            
        }

        public void HandleOnKeyDown(KeyEventArgs e) {
            if (IsVertical) {
                if (e.KeyCode == Keys.Up) {
                    Value -= 70;
                } else if (e.KeyCode == Keys.Down) {
                    Value += 70;
                } else if (e.KeyCode == Keys.PageUp) {
                    Value -= 400;
                } else if (e.KeyCode == Keys.PageDown) {
                    Value += 400;
                } else if (e.KeyCode == Keys.End) {
                    Value = MaximumValue;
                } else if (e.KeyCode == Keys.Home) {
                    Value = 0;
                }
            } else {
                if (e.KeyCode == Keys.Left) {
                    Value -= 70;
                } else if (e.KeyCode == Keys.Right) {
                    Value += 70;
                }
            }
        }

        public bool HandleIsInputKey(Keys keyData) {
            switch (keyData) {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }

            return false;
        }
    }
}