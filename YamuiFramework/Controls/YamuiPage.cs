#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiPage.cs) is part of YamuiFramework.
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
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer(typeof(UserControl))]
    public class YamuiPage : UserControl {

        #region Virtual methods

        /// <summary>
        /// Method called by YamuiTab when the page changes to this page
        /// </summary>
        public virtual void OnShow() {}

        /// <summary>
        /// Method called by YamuiTab when the page changes from this one and when the form closes
        /// </summary>
        public virtual void OnHide() {}

        #endregion
     
        #region fields
                
        [Browsable(false)]
        public override bool AutoScroll { get; set; } = false;

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool NoBackgroundImage { get; set; }

        private Point _lastMouseMove;
        private int _thumbPadding = 2;

        /// <summary>
        /// Exposes the states of the scroll bars, true if they are displayed
        /// </summary>
        public bool HasScrolls { get; private set; }

        /// <summary>
        /// Maximum 'height' of this panel if we wanted to show it all w/o scrolls
        /// </summary>
        private int VirtualPanelHeight { get; set; }

        private RectangleF _barRectangle;
        private RectangleF _thumbRectangle;
        private float _thumbRectangleRealHeight;

        private bool _isPressed;
        private bool _isHovered;

        #endregion

        #region constructor

        public YamuiPage() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);
            _barRectangle = new Rectangle(Width - 10, 0, 10, Height);
            _thumbRectangle = new Rectangle(Width - 10 + 2, 2, 6, Height - 4);

            VerticalScroll.Enabled = true;
            VerticalScroll.Visible = false;
            VerticalScroll.Minimum = 0;

            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
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

            if (HasScrolls)
                OnPaintForeground(e);
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {
            Color thumbColor = YamuiThemeManager.Current.ScrollBarsFg(false, _isHovered, _isPressed, Enabled);
            Color barColor = YamuiThemeManager.Current.ScrollBarsBg(false, _isHovered, _isPressed, Enabled);
            DrawScrollBar(e.Graphics, thumbColor, barColor);
        }

        private void DrawScrollBar(Graphics g, Color thumbColor, Color barColor) {
            if (barColor != Color.Transparent) {
                using (var b = new SolidBrush(barColor)) {
                    g.FillRectangle(b, _barRectangle);
                }
            }
            using (var b = new SolidBrush(thumbColor)) {
                g.FillRectangle(b, _thumbRectangle);
            }
        }

        #endregion

        #region Handle windows messages

        [SecuritySafeCritical]
        protected override void WndProc(ref Message message) {
            if (HasScrolls)
                HandleWindowsProc(message);
            base.WndProc(ref message);
        }

        private void HandleWindowsProc(Message message) {
            switch (message.Msg) {
                case (int) WinApi.Messages.WM_MOUSEWHEEL:
                    // delta negative when scrolling up
                    var delta = -((short) (message.WParam.ToInt64() >> 16));
                    DoScroll(Math.Sign(delta)*_thumbRectangleRealHeight/2);
                    break;

                case (int) WinApi.Messages.WM_LBUTTONDOWN:
                    var mousePosRelativeToThis = PointToClient(MousePosition);

                    // mouse in scrollbar
                    if (_barRectangle.Contains(mousePosRelativeToThis)) {
                        var thumbRect = _thumbRectangle;
                        thumbRect.X -= _thumbPadding;
                        thumbRect.Width += _thumbPadding*2;

                        // mouse in thumb
                        if (thumbRect.Contains(mousePosRelativeToThis)) {
                            _isPressed = true;
                            _lastMouseMove = PointToScreen(MousePosition);
                            Invalidate();
                        } else {
                            DoScroll(mousePosRelativeToThis.Y - _thumbRectangle.Y);
                        }
                    }
                    break;

                case (int) WinApi.Messages.WM_LBUTTONUP:
                    if (_isPressed) {
                        _isPressed = false;
                        Invalidate();
                    }
                    break;

                case (int) WinApi.Messages.WM_MOUSEMOVE:
                    // hover thumb
                    var controlPos = PointToScreen(Location);
                    var mousePosInControl = new Point(MousePosition.X - controlPos.X, MousePosition.Y - controlPos.Y);
                    if (_thumbRectangle.Contains(mousePosInControl)) {
                        _isHovered = true;
                        Invalidate();
                    } else {
                        if (_isHovered) {
                            _isHovered = false;
                            Invalidate();
                        }
                    }
                    // move thumb
                    if (_isPressed) {
                        Point currentlMouse = PointToScreen(MousePosition);
                        if (_lastMouseMove != currentlMouse) {
                            DoScroll(currentlMouse.Y - _lastMouseMove.Y);
                        }
                        _lastMouseMove = currentlMouse;
                    }

                    break;
            }
        }

        #endregion

        #region Keep maximum scroll updated

        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);
            if (e.Control != null && e.Control.Top + e.Control.Height > VirtualPanelHeight) {
                VirtualPanelHeight = e.Control.Top + e.Control.Height;
                OnResizedVirtualPanel();
            }
        }

        protected override void OnControlRemoved(ControlEventArgs e) {
            base.OnControlRemoved(e);
            if (e.Control != null && e.Control.Top + e.Control.Height >= VirtualPanelHeight) {
                RefreshVirtualPanelHeight();
            }
        }

        /// <summary>
        /// Compute the virtual panel height
        /// </summary>
        public void RefreshVirtualPanelHeight() {
            foreach (Control control in Controls) {
                if (control.Top + control.Height > VirtualPanelHeight) {
                    VirtualPanelHeight = control.Top +control.Height;
                }
            }
            OnResizedVirtualPanel();
        }

        #endregion

        #region core

        private void DoScroll(float delta) {
            // minimum Y position
            if (_thumbRectangle.Y + delta < (_barRectangle.Y + _thumbPadding)) {
                _thumbRectangle.Location = new PointF(_thumbRectangle.X, _barRectangle.Y + _thumbPadding);
            } else {
                // maximum Y position
                if (_thumbRectangle.Y + delta > _barRectangle.Height + _barRectangle.Y - _thumbRectangle.Height - _thumbPadding) {
                    _thumbRectangle.Location = new PointF(_thumbRectangle.X, _barRectangle.Height + _barRectangle.Y - _thumbRectangle.Height - _thumbPadding);
                } else {
                    // apply delta
                    _thumbRectangle.Location = new PointF(_thumbRectangle.X, _thumbRectangle.Y + delta);
                }
            }
            Invalidate();
            // Set panel positon from thumb position
            SetPanelPosition();
        }

        /// <summary>
        /// Sets the position of the content panel in function of the thumb position in the scroll bar
        /// </summary>
        private void SetPanelPosition() {
            if (VerticalScroll.Maximum != VirtualPanelHeight) {
                // okay, this is yet another weird thing but to be able to use 
                // AutoScrollPosition setter as expected, we have to do the thing below 
                VerticalScroll.Maximum = VirtualPanelHeight;
                VerticalScroll.Visible = true;
                var fuckingHack = AutoScrollPosition;
                VerticalScroll.Visible = false;
            }
            
            // maximum free space to scroll in the bar
            float barScrollSpace = (_barRectangle.Height - _thumbPadding*2) - _thumbRectangle.Height;
            if (barScrollSpace <= 0) {
                AutoScrollPosition = new Point(0, 0);
            } else {
                float percentScrolled = ((_thumbRectangle.Y - (_barRectangle.Y + _thumbPadding))/barScrollSpace)*100;
                // maximum free space to scroll in the panel
                float scrollSpace = VirtualPanelHeight - Height;
                // % in the scroll bar to % in the panel
                AutoScrollPosition = new Point(0, (int) (scrollSpace/100*percentScrolled));
            }
        }
        
        
        protected override void OnResize(EventArgs e) {
            _barRectangle.Height = Height;
            _barRectangle.X = Width - _barRectangle.Width;
            _thumbRectangle.X = Width - _barRectangle.Width + _thumbPadding;

            RefreshVirtualPanelHeight();

            base.OnResize(e);
        }

        private void OnResizedVirtualPanel() {
            
            // if the content is not too tall, no need to display the scroll bars
            if (VirtualPanelHeight <= Height) {
                if (HasScrolls)
                    Padding = new Padding(Padding.Left, Padding.Top, Math.Max(Padding.Right - 10, 0), Padding.Bottom);
                HasScrolls = false;
            } else {
                // thumb heigh is a ratio of displayed height and the content panel height
                _thumbRectangleRealHeight = _barRectangle.Height * ((float) Height / VirtualPanelHeight);
                _thumbRectangle.Height = Math.Max(_thumbRectangleRealHeight - _thumbPadding * 2, 10);
                if (!HasScrolls)
                    Padding = new Padding(Padding.Left, Padding.Top, Math.Min(Padding.Right + 10, Width), Padding.Bottom);
                HasScrolls = true;
            }

            DoScroll(0);
        }

        #endregion
        

    }
}