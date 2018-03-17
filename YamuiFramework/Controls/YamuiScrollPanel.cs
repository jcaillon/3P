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
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer(typeof(YamuiScrollPanelDesigner))]
    public class YamuiScrollPanel : ScrollableControl, IYamuiControl {

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
        public YamuiScrollHandler VertScroll { get; }
        
        [Browsable(false)]
        public YamuiScrollHandler HoriScroll { get; }

        /// <summary>
        /// Can this control have vertical scroll?
        /// </summary>
        [DefaultValue(true)]
        public bool CanHasHScroll {
            get { return _canHasHScroll; }
            set {
                _canHasHScroll = value;
                VertScroll.Enabled = _canHasHScroll;
                PerformLayout();
            }
        }
        
        /// <summary>
        /// Can this control have vertical scroll?
        /// </summary>
        [DefaultValue(true)]
        public bool CanHasVScroll {
            get { return _canHasVScroll; }
            set {
                _canHasVScroll = value;
                VertScroll.Enabled = _canHasVScroll;
                PerformLayout();
            }
        }

        private bool _canHasVScroll = true;
        private bool _canHasHScroll = true;

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

            VertScroll = new YamuiScrollHandler(true, this);
            HoriScroll = new YamuiScrollHandler(false, this);

            base.VScroll = false;
            base.VerticalScroll.Enabled = false;
            base.VerticalScroll.Visible = false;
            
            base.HScroll = false;
            base.HorizontalScroll.Enabled = false;
            base.HorizontalScroll.Visible = false;
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

            VertScroll.Paint(e);
            HoriScroll.Paint(e);
        }

        #endregion

        #region Handle windows messages

        //[DebuggerStepThrough]
        [SecuritySafeCritical]
        protected override void WndProc(ref Message message) {
            VertScroll.HandleWindowsProc(message);
            HoriScroll.HandleWindowsProc(message);
            base.WndProc(ref message);
        }
        
        #endregion

        #region core
        
        /// <summary>
        /// Perform the layout of the control
        /// </summary>
        protected override void OnLayout(LayoutEventArgs levent) {
            base.OnLayout(levent);
            if (!string.IsNullOrEmpty(levent.AffectedProperty) && levent.AffectedProperty.Equals("Bounds")) {
                VertScroll.HandleOnLayout(levent);
                HoriScroll.HandleOnLayout(levent);
            }
        }
        
        /// <summary>
        /// Handle key down event for selection, copy and scrollbars handling.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            VertScroll.HandleOnKeyDown(e);
            HoriScroll.HandleOnKeyDown(e);
        }

        /// <summary>
        /// Used to add arrow keys to the handled keys in <see cref="OnKeyDown"/>.
        /// </summary>
        protected override bool IsInputKey(Keys keyData) {
            if (VertScroll.HandleIsInputKey(keyData))
                return true;
            if (HoriScroll.HandleIsInputKey(keyData))
                return true;
            return base.IsInputKey(keyData);
        }
        
        /// <summary>
        /// Set focus on the control for keyboard scrollbars handling
        /// </summary>
        protected override void OnClick(EventArgs e) {
            base.OnClick(e);
            Focus();
        }
        
        public void UpdateBoundsPublic() {
            UpdateBounds();
        }

        /// <summary>
        /// Correct original padding as we need extra space for the scrollbars
        /// </summary>
        public new Padding Padding {
            get {
                var basePadding = base.Padding;
                if (!DesignMode) {
                    if (HoriScroll.HasScroll) {
                        basePadding.Bottom = basePadding.Bottom + HoriScroll.ScrollBarThickness;
                    }

                    if (VertScroll.HasScroll) {
                        basePadding.Right = basePadding.Right + VertScroll.ScrollBarThickness;
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
                if (VertScroll.HasScroll)
                    rect.Y = -VertScroll.Value;
                    rect.Width -= HoriScroll.ScrollBarThickness;
                if (HoriScroll.HasScroll) {
                    rect.X = -HoriScroll.Value;
                    rect.Height -= VertScroll.ScrollBarThickness;
                }
                return rect;
            }
        }

        [Browsable(false)]
        public new Point AutoScrollPosition {
            get { return new Point(HoriScroll.HasScroll ? HoriScroll.Value : 0, VertScroll.HasScroll ? VertScroll.Value : 0); }
            set {
                if (HoriScroll.HasScroll)
                    HoriScroll.Value = value.X;
                if (VertScroll.HasScroll)
                    VertScroll.Value = value.Y;
            }
        }

        [Browsable(false)]
        public new Size AutoScrollMinSize {
            get {
                return new Size(HoriScroll.MaximumValue, VertScroll.MaximumValue);
            }
            set {
                // TODO : 
            }
        }

        public new void ScrollControlIntoView(Control control) {
            
        }

        #endregion

        #region Hide not relevant properties from designer

        [Browsable(false)]
        public override bool AutoScroll => false;

        [Browsable(false)]
        public new bool VScroll => false;

        [Browsable(false)]
        public new bool HScroll => false;

        [Browsable(false)]
        public new Size AutoScrollMargin => new Size();

        [Browsable(false)]
        public new DockPaddingEdges DockPadding => null;

        [Browsable(false)]
        public new HScrollProperties HorizontalScroll => null;

        [Browsable(false)]
        public new HScrollProperties VerticalScroll => null;

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Font Font {
            get { return base.Font; }
            set { base.Font = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Color ForeColor {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override bool AllowDrop {
            get { return base.AllowDrop; }
            set { base.AllowDrop = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override RightToLeft RightToLeft {
            get { return base.RightToLeft; }
            set { base.RightToLeft = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Cursor Cursor {
            get { return base.Cursor; }
            set { base.Cursor = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public new bool UseWaitCursor {
            get { return base.UseWaitCursor; }
            set { base.UseWaitCursor = value; }
        }

        #endregion

    }

    internal class YamuiScrollPanelDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("AutoScrollMargin");
            properties.Remove("AutoScrollMinSize");
            base.PreFilterProperties(properties);
        }
    }
}