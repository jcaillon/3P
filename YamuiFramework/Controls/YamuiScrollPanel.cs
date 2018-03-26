#region header

// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiScrollPage.cs) is part of YamuiFramework.
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer(typeof(YamuiScrollPanelDesigner))]
    public class YamuiScrollPanel : YamuiControl {
        #region fields

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool NoBackgroundImage { get; set; }

        /*
        [DefaultValue(2)]
        [Category("Yamui")]
        public int ThumbPadding { get; set; }

        [DefaultValue(10)]
        [Category("Yamui")]
        public int ScrollBarWidth { get; set; }
        */

        [Browsable(false)]
        public YamuiScrollHandler VerticalScroll { get; }

        [Browsable(false)]
        public YamuiScrollHandler HorizontalScroll { get; }

        /// <summary>
        /// Can this control have vertical scroll?
        /// </summary>
        [DefaultValue(true)]
        public bool HScroll {
            get { return _hScroll; }
            set {
                _hScroll = value;
                HorizontalScroll.Enabled = _hScroll;
                PerformLayout();
            }
        }

        /// <summary>
        /// Can this control have vertical scroll?
        /// </summary>
        [DefaultValue(true)]
        public bool VScroll {
            get { return _vScroll; }
            set {
                _vScroll = value;
                VerticalScroll.Enabled = _vScroll;
                PerformLayout();
            }
        }

        [Browsable(false)]
        public Point AutoScrollPosition {
            get { return new Point(HorizontalScroll.HasScroll ? HorizontalScroll.Value : 0, VerticalScroll.HasScroll ? VerticalScroll.Value : 0); }
            set {
                if (HorizontalScroll.HasScroll)
                    HorizontalScroll.Value = value.X;
                if (VerticalScroll.HasScroll)
                    VerticalScroll.Value = value.Y;
            }
        }

        [Browsable(false)]
        public Size AutoScrollMinSize {
            get { return new Size(HorizontalScroll.LengthToRepresentMinSize, VerticalScroll.LengthToRepresentMinSize); }
            set {
                HorizontalScroll.LengthToRepresentMinSize = value.Width;
                VerticalScroll.LengthToRepresentMinSize = value.Height;
            }
        }

        [Browsable(false)]
        public bool HasScroll => VerticalScroll.HasScroll || HorizontalScroll.HasScroll;

        private bool _vScroll = true;
        private bool _hScroll = true;
        private Size _preferedSize;
        private Rectangle _leftoverBar;
        private bool _needBothScroll;
        
        #endregion

        #region constructor

        public YamuiScrollPanel() {
            SetStyle(
                ControlStyles.UserMouse |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.Selectable |
                ControlStyles.Opaque, true);

            VerticalScroll = new YamuiScrollHandler(true, this) {
                SmallChange = 70,
                LargeChange = 400
            };
            VerticalScroll.OnValueChange += ScrollOnOnValueChange;
            VerticalScroll.OnRedrawScrollBars += OnRedrawScrollBars;
            HorizontalScroll = new YamuiScrollHandler(false, this) {
                SmallChange = 70,
                LargeChange = 400
            };
            HorizontalScroll.OnValueChange += ScrollOnOnValueChange;
            HorizontalScroll.OnRedrawScrollBars += OnRedrawScrollBars;
        }

        ~YamuiScrollPanel() {
            VerticalScroll.OnValueChange -= ScrollOnOnValueChange;
            HorizontalScroll.OnValueChange -= ScrollOnOnValueChange;
        }

        private void ScrollOnOnValueChange(YamuiScrollHandler yamuiScrollHandler, int previousValue, int newValue) {
            SetDisplayRectLocation(yamuiScrollHandler, previousValue - newValue);
        }

        private void OnRedrawScrollBars() {
            Invalidate();
            PaintScrollBars();
        }

        #endregion

        #region Paint

        /// <summary>
        /// Paint scrollpanel background
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e) {
            // paint background
            e.Graphics.Clear(YamuiThemeManager.Current.FormBack);
            if (!NoBackgroundImage && !DesignMode) {
                var img = YamuiThemeManager.CurrentThemeImage;
                if (img != null) {
                    Rectangle rect = new Rectangle(ClientRectangle.Right - img.Width, ClientRectangle.Height - img.Height, img.Width, img.Height);
                    e.Graphics.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                }
            }
        }

        private void PaintScrollBars() {
            if (!IsHandleCreated)
                return;
            var hdc = WinApi.GetWindowDC(new HandleRef(this, Handle));
            if (hdc == IntPtr.Zero) {
                return;
            }
            try {
                // paint the left over between the 2 scrolls (the small square)
                if (_needBothScroll) {
                    PaintOnRectangle(hdc, ref _leftoverBar, PaintLeftOver);
                }
                if (VerticalScroll.HasScroll) {
                    var verticalBar = VerticalScroll.BarRect;
                    PaintOnRectangle(hdc, ref verticalBar, PaintVerticalScroll);
                }
                if (HorizontalScroll.HasScroll) {
                    var horizontalBar = HorizontalScroll.BarRect;
                    PaintOnRectangle(hdc, ref horizontalBar, PaintHorizontalScroll);
                }
            } finally {
                WinApi.ReleaseDC(new HandleRef(this, Handle), new HandleRef(null, hdc));
            }
        }

        private void PaintLeftOver(PaintEventArgs e) {
            using (var b = new SolidBrush(YamuiThemeManager.Current.ScrollNormalBack)) {
                e.Graphics.FillRectangle(b, e.ClipRectangle);
            }
        }

        private void PaintHorizontalScroll(PaintEventArgs e) {
            HorizontalScroll.Paint(e);
        }

        private void PaintVerticalScroll(PaintEventArgs e) {
            VerticalScroll.Paint(e);
        }

        private void PaintOnRectangle(IntPtr winDc, ref Rectangle rect, Action<PaintEventArgs> paint) {
            using (BufferedGraphics bg = BufferedGraphicsManager.Current.Allocate(winDc, rect)) {
                Graphics g = bg.Graphics;
                g.SetClip(rect);
                using (PaintEventArgs e = new PaintEventArgs(g, rect)) {
                    paint(e);
                }
                bg.Render();
            }
        }

        #endregion

        #region handle windows events

        /// <summary>
        /// redirect all input key to keydown
        /// </summary>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// Handle keydown
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e) {
            e.Handled = HorizontalScroll.HandleKeyDown(e) || VerticalScroll.HandleKeyDown(e);
            if (!e.Handled)
                base.OnKeyDown(e);
        }
        
        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            return e.Handled;
        }

        protected override void WndProc(ref Message m) {
            if (DesignMode) {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg) {
                case (int) WinApi.Messages.WM_NCCALCSIZE:
                    //Get Window Rect
                    var formRect = new Rectangle();
                    WinApi.GetWindowRect(m.HWnd, ref formRect);

                    //Check WPARAM
                    if (m.WParam != IntPtr.Zero) {
                        // When TRUE, LPARAM Points to a NCCALCSIZE_PARAMS structure
                        var nccsp = (WinApi.NCCALCSIZE_PARAMS) Marshal.PtrToStructure(m.LParam, typeof(WinApi.NCCALCSIZE_PARAMS));
                        AdjustClientArea(ref nccsp.rectProposed);
                        Marshal.StructureToPtr(nccsp, m.LParam, true);
                    } else {
                        // When FALSE, LPARAM Points to a RECT structure
                        var clnRect = (WinApi.RECT) Marshal.PtrToStructure(m.LParam, typeof(WinApi.RECT));
                        AdjustClientArea(ref clnRect);
                        Marshal.StructureToPtr(clnRect, m.LParam, true);
                    }

                    //Return Zero
                    m.Result = IntPtr.Zero;
                    break;

                case (int) WinApi.Messages.WM_NCPAINT:
                    PaintScrollBars();
                    // we handled everything
                    m.Result = IntPtr.Zero;
                    break;

                case (int) WinApi.Messages.WM_NCHITTEST:
                    // we need to correctly handle this if we want the non client area events (WM_NC*) to fire properly!
                    var point = PointToClient(new Point(m.LParam.ToInt32()));
                    if (!ClientRectangle.Contains(point)) {
                        m.Result = (IntPtr) WinApi.HitTest.HTBORDER;
                    } else { 
                        base.WndProc(ref m);
                    }
                    break;

                case (int) WinApi.Messages.WM_MOUSEWHEEL:
                    if (HasScroll) {
                        // delta negative when scrolling up
                        var delta = (short) (m.WParam.ToInt64() >> 16);
                        var mouseEvent1 = new MouseEventArgs(MouseButtons.None, 0, 0, 0, delta);
                        if (HorizontalScroll.IsHovered) {
                            HorizontalScroll.HandleScroll(mouseEvent1);
                        } else {
                            VerticalScroll.HandleScroll(mouseEvent1);
                        }
                    } else {
                        // propagate the event
                        base.WndProc(ref m);
                    }
                    break;
                    
                case (int) WinApi.Messages.WM_NCMOUSELEAVE:
                    VerticalScroll.HandleMouseLeave();
                    HorizontalScroll.HandleMouseLeave();  
                    base.WndProc(ref m);
                    break;
                    
                case (int) WinApi.Messages.WM_MOUSEMOVE:
                    if (VerticalScroll.IsThumbPressed)
                        VerticalScroll.HandleMouseMove(null);
                    if (HorizontalScroll.IsThumbPressed)
                        HorizontalScroll.HandleMouseMove(null);
                    base.WndProc(ref m);
                    break;

                case (int) WinApi.Messages.WM_NCMOUSEMOVE:
                    // track mouse leaving
                    WinApi.TRACKMOUSEEVENT tme = new WinApi.TRACKMOUSEEVENT();
                    tme.cbSize = (uint)Marshal.SizeOf(tme);
                    tme.dwFlags = (uint) (WinApi.TMEFlags.TME_LEAVE | WinApi.TMEFlags.TME_NONCLIENT);
                    tme.hwndTrack = Handle;
                    WinApi.TrackMouseEvent(tme);

                    var point2 = PointToClient(new Point(m.LParam.ToInt32()));
                    var mouseEvent2 = new MouseEventArgs(MouseButtons.None, 0, point2.X, point2.Y, 0);
                    VerticalScroll.HandleMouseMove(mouseEvent2);  
                    HorizontalScroll.HandleMouseMove(mouseEvent2);  
                    base.WndProc(ref m);
                    break;

                case (int) WinApi.Messages.WM_NCLBUTTONDOWN:
                    var point3 = PointToClient(new Point(m.LParam.ToInt32()));
                    var mouseEvent3 = new MouseEventArgs(MouseButtons.Left, 0, point3.X, point3.Y, 0);
                    VerticalScroll.HandleMouseDown(mouseEvent3);  
                    HorizontalScroll.HandleMouseDown(mouseEvent3);
                    Focus();
                    // here we forward to base button down because it has a internal focus mecanism that we want to exploit
                    // if we don't do that, the mouse MOVE events are not fired outside the bounds of this control!
                    m.Msg = (int) WinApi.Messages.WM_LBUTTONDOWN;
                    base.WndProc(ref m);
                    break;

                case (int) WinApi.Messages.WM_NCLBUTTONUP:
                case (int) WinApi.Messages.WM_LBUTTONUP:
                    var point4 = PointToClient(new Point(m.LParam.ToInt32()));
                    var mouseEvent4 = new MouseEventArgs(MouseButtons.Left, 0, point4.X, point4.Y, 0);
                    VerticalScroll.HandleMouseUp(mouseEvent4);  
                    HorizontalScroll.HandleMouseUp(mouseEvent4);
                    // here we forward this message to base WM_LBUTTONUP to release the internal focus on this control
                    m.Msg = (int) WinApi.Messages.WM_LBUTTONUP;
                    base.WndProc(ref m);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void AdjustClientArea(ref WinApi.RECT rect) {
            if (HorizontalScroll.HasScroll) {
                rect.bottom -= HorizontalScroll.BarThickness;
            }
            if (VerticalScroll.HasScroll) {
                rect.right -= VerticalScroll.BarThickness;
            }
        }

        protected override void OnResize(EventArgs e) {
            ApplyPreferedSize(_preferedSize);
            base.OnResize(e);
        }

        /// <summary>
        /// Perform the layout of the control (call SuspendLayout/ResumeLayout to temporaly stop calling this method)
        /// </summary>
        protected override void OnLayout(LayoutEventArgs levent) {
            base.OnLayout(levent);
            if (!string.IsNullOrEmpty(levent.AffectedProperty) && levent.AffectedProperty.Equals("Bounds")) {
                if (levent.AffectedControl != null && levent.AffectedControl != this) {
                    // when a child item changes bounds
                    UpdatePreferedSizeIfNeeded(levent.AffectedControl);
                    ApplyPreferedSize(_preferedSize);
                }
            }
        }

        #endregion
        
        #region core

        /// <summary>
        /// Control the controls added
        /// </summary>
        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);
            if (!(e.Control is IScrollableControl)) {
                throw new Exception("All controls added to this panel should implement " + nameof(IScrollableControl));
            }
        }

        /// <summary>
        /// Override, called at the end of initializeComponent() in forms made with the designer
        /// </summary>
        public new void PerformLayout() {
            ApplyPreferedSize(PreferedSize());
            base.PerformLayout();
        }

        private void ApplyPreferedSize(Size size) {
            _needBothScroll = false;
            var needHorizontalScroll = HorizontalScroll.HasScroll;
            var needVerticalScroll = VerticalScroll.UpdateLength(size.Height, Height - (needHorizontalScroll ? HorizontalScroll.BarThickness : 0), Height, Width);
            HorizontalScroll.UpdateLength(size.Width, Width - (needVerticalScroll ? VerticalScroll.BarThickness : 0), Width, Height);

            if (needHorizontalScroll != HorizontalScroll.HasScroll) {
                needHorizontalScroll = HorizontalScroll.HasScroll;
                needVerticalScroll = VerticalScroll.UpdateLength(size.Height, Height - (needHorizontalScroll ? HorizontalScroll.BarThickness : 0), Height, Width);
            }

            if (needVerticalScroll && needHorizontalScroll) {
                HorizontalScroll.ExtraEndPadding = VerticalScroll.BarThickness;
                VerticalScroll.ExtraEndPadding = HorizontalScroll.BarThickness;
                _leftoverBar = new Rectangle(Width - VerticalScroll.BarThickness, Height - HorizontalScroll.BarThickness, HorizontalScroll.ExtraEndPadding, VerticalScroll.ExtraEndPadding);
            } else {
                HorizontalScroll.ExtraEndPadding = 0;
                VerticalScroll.ExtraEndPadding = 0;
            }
        }

        /// <summary>
        /// The actual scroll magic is here
        /// </summary>
        private void SetDisplayRectLocation(YamuiScrollHandler scroll, int deltaValue) {
            if (deltaValue == 0 || !scroll.HasScroll)
                return;

            // (found in ScrollablePanel.SetDisplayRectLocation(0, deltaVerticalValue);)
            Rectangle cr = ClientRectangle;
            WinApi.RECT rcClip = WinApi.RECT.FromXYWH(cr.X, cr.Y, cr.Width - scroll.BarThickness, cr.Height);
            WinApi.RECT rcUpdate = WinApi.RECT.FromXYWH(cr.X, cr.Y, cr.Width - scroll.BarThickness, cr.Height);
            WinApi.ScrollWindowEx(
                new HandleRef(this, Handle),
                scroll.IsVertical ? 0 : deltaValue,
                scroll.IsVertical ? deltaValue : 0,
                null,
                ref rcClip,
                WinApi.NullHandleRef,
                ref rcUpdate,
                WinApi.SW_INVALIDATE
                | WinApi.SW_ERASE
                | WinApi.SW_SCROLLCHILDREN
                | WinApi.SW_SMOOTHSCROLL);

            UpdateChildrenBound();

            Refresh(); // not critical but help reduce flickering
        }

        private void UpdateChildrenBound() {
            foreach (Control control in Controls) {
                var yamuiControl = control as IScrollableControl;
                if (yamuiControl != null && control.IsHandleCreated) {
                    yamuiControl.UpdateBoundsPublic();
                }
            }
        }

        /// <summary>
        /// Get prefered size, i.e. the size we would need to display all the child controls at the same time
        /// </summary>
        /// <returns></returns>
        private Size PreferedSize() {
            _preferedSize = new Size(0, 0);
            foreach (Control control in Controls) {
                UpdatePreferedSizeIfNeeded(control);
            }

            return _preferedSize;
        }

        private void UpdatePreferedSizeIfNeeded(Control control) {
            int controlReach = control.Top + control.Height + VerticalScroll.Value;
            if (controlReach > _preferedSize.Height) {
                _preferedSize.Height = controlReach;
            }

            controlReach = control.Left + control.Width + HorizontalScroll.Value;
            if (controlReach > _preferedSize.Width) {
                _preferedSize.Width = controlReach;
            }
        }
        
        /// <summary>
        /// Very important to display the correct scroll value when coming back to a scrolled panel.
        /// Try without it and watch for yourself
        /// </summary>
        public override Rectangle DisplayRectangle {
            get {
                Rectangle rect = ClientRectangle;
                if (VerticalScroll.HasScroll)
                    rect.Y = -VerticalScroll.Value;
                rect.Width -= HorizontalScroll.BarThickness;
                if (HorizontalScroll.HasScroll) {
                    rect.X = -HorizontalScroll.Value;
                    rect.Height -= VerticalScroll.BarThickness;
                }

                return rect;
            }
        }

        public void ScrollControlIntoView(Control control) { }

        #endregion
    }

    internal class YamuiScrollPanelDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("AutoScrollMargin");
            properties.Remove("AutoScrollMinSize");
            properties.Remove("Font");
            properties.Remove("ForeColor");
            properties.Remove("AllowDrop");
            properties.Remove("RightToLeft");
            properties.Remove("Cursor");
            properties.Remove("UseWaitCursor");
            base.PreFilterProperties(properties);
        }
    }
}