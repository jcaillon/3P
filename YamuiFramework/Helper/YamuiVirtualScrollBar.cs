using System;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace YamuiFramework.Helper {
        public class YamuiVirtualScrollBar {

            #region Private

            private int _value;
            private int _maximum = 100;
            private int _thumbSize = 10;
            private ScrollOrientation _orientation;
            private int _thumbPadding = 2;
            private int _minThumbSize = 8;
            private bool _isHovered;
            private bool _isPressed;

            #endregion

            #region Public fields
            public bool Disabled { get; set; }

            public Point Location { get; set; }

            public Size Size { get; set; }

            public Rectangle ClientRectangle {
                get { return new Rectangle(Location, Size); }
            }

            public bool Visible { get; set; }

            public int Value {
                get { return _value; }
                set { _value = Math.Max(0, Math.Min(Maximum, value)); }
            }

            public int Maximum {
                get { return _maximum; }
                set {
                    _maximum = value;
                    Visible = _maximum > 0;
                }
            }

            public int ThumbSize {
                get { return Math.Max(_thumbSize, MinThumbSize); }
                set { _thumbSize = value; }
            }

            public ScrollOrientation Orientation {
                get { return _orientation; }
                set { _orientation = value; }
            }

            public int ThumbPadding {
                get { return _thumbPadding; }
                set { _thumbPadding = value; }
            }

            public int MinThumbSize {
                get { return _minThumbSize; }
                set { _minThumbSize = value; }
            }

            public Rectangle ThumbRectangle {
                get {
                    Rectangle thumbRect = Rectangle.Empty;
                    switch (Orientation) {
                        case ScrollOrientation.HorizontalScroll:
                            thumbRect = new Rectangle(
                                Location.X + ThumbPadding + (int) ((float) Value/Maximum*(Size.Width - ThumbSize - 2*ThumbPadding)),
                                Location.Y + ThumbPadding,
                                ThumbSize,
                                Size.Height - ThumbPadding*2);
                            break;
                        case ScrollOrientation.VerticalScroll:
                            thumbRect = new Rectangle(
                                Location.X + ThumbPadding,
                                Location.Y + ThumbPadding + (int) ((float) Value/Maximum*(Size.Height - ThumbSize - 2*ThumbPadding)),
                                Size.Width - ThumbPadding*2,
                                ThumbSize);
                            break;
                    }
                    return thumbRect;
                }
            }

            public bool IsHovered {
                get { return _isHovered; }
                set {
                    _isHovered = value;
                    if (OnInvalidate != null)
                        OnInvalidate(this, new ScrollEventArgs(ScrollEventType.First, 1));
                }
            }

            public bool IsPressed {
                get { return _isPressed; }
                set {
                    _isPressed = value;
                    if (OnInvalidate != null)
                        OnInvalidate(this, new ScrollEventArgs(ScrollEventType.First, 1));
                }
            }

            public int PosOnThumb { get; set; }

            #endregion

            #region Public events

            public event ScrollEventHandler Scroll;

            public event ScrollEventHandler OnInvalidate;

            #endregion

            #region Public methods

            public void Paint(Graphics g, bool enabled) {
                if (Disabled)
                    return;

                Color thumbColor = YamuiThemeManager.Current.ScrollBarsFg(false, IsHovered, IsPressed, enabled);
                Color barColor = YamuiThemeManager.Current.ScrollBarsBg(false, IsHovered, IsPressed, enabled);

                if (Visible) {
                    // paint both scroll rectangles
                    using (var brush = new SolidBrush(barColor)) {
                        g.FillRectangle(brush, ClientRectangle);
                    }

                    // paint both thumb rectangles
                    using (var brush = new SolidBrush(thumbColor)) {
                        g.FillRectangle(brush, ThumbRectangle);
                    }
                }
            }

            public void DoScroll(int delta) {
                switch (Orientation) {
                    case ScrollOrientation.VerticalScroll:
                        Value += -Math.Sign(delta)*(int) ((float) Maximum*ThumbSize/(Size.Height - 2*ThumbPadding))/2;
                        break;
                    case ScrollOrientation.HorizontalScroll:
                        Value += -Math.Sign(delta)*(int) ((float) Maximum*ThumbSize/(Size.Width - 2*ThumbPadding))/2;
                        break;
                }
                if (Scroll != null)
                    Scroll(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, Value, Orientation));
            }

            public void ChangeThumbPos(int thumbPos) {
                switch (Orientation) {
                    case ScrollOrientation.VerticalScroll:
                        Value = (int) ((float) Maximum*(thumbPos - ThumbPadding)/(Size.Height - ThumbSize - 2*ThumbPadding));
                        break;
                    case ScrollOrientation.HorizontalScroll:
                        Value = (int) ((float) Maximum*(thumbPos - ThumbPadding)/(Size.Width - ThumbSize - 2*ThumbPadding));
                        break;
                }
                if (Scroll != null)
                    Scroll(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, Value, Orientation));
            }

            public void UpdateThumbSize(int displayedSize, int realSize) {

                // thumb height is a ratio of displayed height and the content panel height
                switch (Orientation) {
                    case ScrollOrientation.VerticalScroll:
                        ThumbSize = (int) ((Size.Height - 2*ThumbPadding)*((float) displayedSize/realSize));
                        break;
                    case ScrollOrientation.HorizontalScroll:
                        ThumbSize = (int) ((Size.Width - 2*ThumbPadding)*((float) displayedSize/realSize));
                        break;
                }

                ThumbSize = Math.Max(ThumbSize, MinThumbSize);
            }

            public bool OnMouseWheel(Point pt, int delta) {
                if (!Disabled && ClientRectangle.Contains(pt)) {
                    DoScroll(delta);
                    return true;
                }
                return false;
            }

            public bool OnMouseDown(Point pt) {
                // mouse in bar
                if (!Disabled && ClientRectangle.Contains(pt)) {
                    // mouse in thumb
                    var thumbRectangle = ThumbRectangle;
                    if (thumbRectangle.Contains(pt)) {
                        IsPressed = true;
                        if (Orientation == ScrollOrientation.VerticalScroll)
                            PosOnThumb = pt.Y - thumbRectangle.Y;
                        else
                            PosOnThumb = pt.X - thumbRectangle.X;
                    } else {
                        ChangeThumbPos(Orientation == ScrollOrientation.VerticalScroll ? pt.Y : pt.X);
                    }
                    return true;
                }
                return false;
            }

            public void OnMouseMove(Point pt) {
                if (Disabled)
                    return;

                // hover thumb
                if (ThumbRectangle.Contains(pt)) {
                    IsHovered = true;
                } else {
                    if (IsHovered)
                        IsHovered = false;
                }

                // moving thumb?
                if (IsPressed) {
                    if (Orientation == ScrollOrientation.VerticalScroll)
                        ChangeThumbPos(pt.Y - PosOnThumb);
                    else
                        ChangeThumbPos(pt.X - PosOnThumb);
                }
            }

            public void OnMouseUp() {
                if (Disabled)
                    return;

                if (IsPressed)
                    IsPressed = false;
            }

            #endregion

        }
}
