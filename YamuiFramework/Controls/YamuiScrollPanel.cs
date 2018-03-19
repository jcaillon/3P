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
using System.Security;
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

        private bool _vScroll = true;
        private bool _hScroll = true;

        #endregion

        #region constructor

        public YamuiScrollPanel() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.Selectable |
                ControlStyles.Opaque, true);

            VerticalScroll = new YamuiScrollHandler(true, this);
            VerticalScroll.OnValueChange += VerticalScrollOnOnValueChange;
            HorizontalScroll = new YamuiScrollHandler(false, this);
        }

        private void VerticalScrollOnOnValueChange(YamuiScrollHandler yamuiScrollHandler, int previousValue, int newValue) {
            SetDisplayRectLocation(yamuiScrollHandler, previousValue - newValue);
        }

        #endregion

        #region Paint

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

            VerticalScroll.Paint(e);
            HorizontalScroll.Paint(e);
        }

        #endregion

        #region core
        
        /// <summary>
        /// redirect all input key to keydown
        /// </summary>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }
        
        /// <summary>
        /// Set focus on the control for keyboard scrollbars handling
        /// </summary>
        protected override void OnClick(EventArgs e) {
            base.OnClick(e);
            Focus();
        }

        //[DebuggerStepThrough]
        [SecuritySafeCritical]
        protected override void WndProc(ref Message message) {
            bool wasHandled;
            if (HorizontalScroll.IsActive) {
                wasHandled = HorizontalScroll.HandleWindowsProc(message) || VerticalScroll.HandleWindowsProc(message);
            } else {
                wasHandled = VerticalScroll.HandleWindowsProc(message) || HorizontalScroll.HandleWindowsProc(message);
            }

            if (!wasHandled)
                base.WndProc(ref message);
        }



        
        /// <summary>
        /// Perform the layout of the control
        /// </summary>
        protected override void OnLayout(LayoutEventArgs levent) {
            base.OnLayout(levent);
            if (!string.IsNullOrEmpty(levent.AffectedProperty) && levent.AffectedProperty.Equals("Bounds")) {
                OnLayoutChanged();
            }
        }

        public new void PerformLayout() {
            OnLayoutChanged();
            base.PerformLayout();
        }

        private void OnLayoutChanged() {
            var size = PreferedSize;

            bool needBothScroll = VerticalScroll.UpdateLength(size.Height, Height) &&
                HorizontalScroll.UpdateLength(size.Width, Width);
            
            if (needBothScroll) {
                HorizontalScroll.ExtraEndPadding = VerticalScroll.BarThickness;
                VerticalScroll.ExtraEndPadding = HorizontalScroll.BarThickness;
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
                var yamuiControl = control as IYamuiControl;
                if (yamuiControl != null && control.IsHandleCreated) {
                    yamuiControl.UpdateBoundsPublic();
                }
            }
        }

        private Size PreferedSize {
            get {
                int heigth = 0;
                int width = 0;
                foreach (Control control in Controls) {
                    int controlReach = control.Top + control.Height + VerticalScroll.Value;
                    if (controlReach > heigth) {
                        heigth = controlReach;
                    }
                    controlReach = control.Left + control.Width + HorizontalScroll.Value;
                    if (controlReach > width) {
                        width = controlReach;
                    }
                }
                return new Size(width, heigth);
            }
        }

        /// <summary>
        /// Correct original padding as we need extra space for the scrollbars
        /// </summary>
        public new Padding Padding {
            get {
                var basePadding = base.Padding;
                if (!DesignMode) {
                    if (HorizontalScroll.HasScroll) {
                        basePadding.Bottom = basePadding.Bottom + HorizontalScroll.BarThickness;
                    }
                    if (VerticalScroll.HasScroll) {
                        basePadding.Right = basePadding.Right + VerticalScroll.BarThickness;
                    }
                }
                return basePadding;
            }
            set {
                base.Padding = value;
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
            get {
                return new Size(HorizontalScroll.MaximumValue, VerticalScroll.MaximumValue);
            }
            set {
                // TODO : 
            }
        }

        public void ScrollControlIntoView(Control control) {
            
        }

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