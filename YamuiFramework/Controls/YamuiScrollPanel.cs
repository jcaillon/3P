#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiScrollPanel.cs) is part of YamuiFramework.
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
using System.Windows.Forms.Design;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer(typeof(ScrollPageDesigner))]
    public class YamuiScrollPanel : UserControl {
        #region fields

        /// <summary>
        /// The base UserControl should be used to add controls, add them to this panel!
        /// This internal panel has a fixed size, the outer UserControl which is YamuiScrollPage
        /// adapts to the size you want, and then displays portion of the internal panel corresponding
        /// to what has been scrolled
        /// </summary>
        [Category("Yamui")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public YamuiInternalPanel ContentPanel {
            get { return _contentPanel; }
        }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool NoBackgroundImage {
            get { return _noBackgroundImage; }
            set {
                _contentPanel.DontUseTransparentBackGround = value;
                _noBackgroundImage = value;
            }
        }

        private bool _noBackgroundImage;

        private YamuiInternalPanel _contentPanel;
        private Point _lastMouseMove;
        private int _thumbPadding = 2;

        /// <summary>
        /// Exposes the states of the scroll bars, true if they are displayed
        /// </summary>
        public bool HasScrolls { get; private set; }

        private Rectangle _barRectangle;
        private Rectangle _thumbRectangle;

        private bool _isPressed;
        private bool _isHovered;

        #endregion

        #region constructor

        public YamuiScrollPanel() {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            _contentPanel = new YamuiInternalPanel {
                Location = new Point(0, 0),
                Width = Width,
                Height = Height,
                OwnerPanel = this
            };
            Controls.Add(_contentPanel);

            _barRectangle = new Rectangle(Width - 10, 0, 10, Height);
            _thumbRectangle = new Rectangle(Width - 10 + 2, 2, 6, Height - 4);
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e) {
            // paint background
            e.Graphics.Clear(YamuiThemeManager.Current.FormBack);
            if (!HasScrolls && !NoBackgroundImage && !DesignMode) {
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
                    DoScroll(Math.Sign(delta)*_thumbRectangle.Height/2);
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

        #region core

        private void DoScroll(int delta) {
            // minimum Y position
            if (_thumbRectangle.Y + delta < (_barRectangle.Y + _thumbPadding)) {
                _thumbRectangle.Location = new Point(_thumbRectangle.X, _barRectangle.Y + _thumbPadding);
            } else {
                // maximum Y position
                if (_thumbRectangle.Y + delta > _barRectangle.Height + _barRectangle.Y - _thumbRectangle.Height - _thumbPadding) {
                    _thumbRectangle.Location = new Point(_thumbRectangle.X, _barRectangle.Height + _barRectangle.Y - _thumbRectangle.Height - _thumbPadding);
                } else {
                    // apply delta
                    _thumbRectangle.Location = new Point(_thumbRectangle.X, _thumbRectangle.Y + delta);
                }
            }
            // Set panel positon from thumb position
            SetPanelPosition();
            Invalidate();
        }

        /// <summary>
        /// Sets the position of the content panel in function of the thumb position in the scroll bar
        /// </summary>
        private void SetPanelPosition() {
            // maximum free space to scroll in the bar
            float barScrollSpace = (_barRectangle.Height - _thumbPadding*2) - _thumbRectangle.Height;
            if (barScrollSpace <= 0) {
                _contentPanel.Top = 0;
            } else {
                float percentScrolled = ((_thumbRectangle.Y - (_barRectangle.Y + _thumbPadding))/barScrollSpace)*100;
                // maximum free space to scroll in the panel
                float scrollSpace = _contentPanel.Height - Height;
                // % in the scroll bar to % in the panel
                _contentPanel.Top = (int) (scrollSpace/100*percentScrolled)*-1;
            }
        }

        protected override void OnResize(EventArgs e) {
            // in designer mode, we need the internal panel to fit the page so the user don't get confused
            if (DesignMode) {
                _contentPanel.Location = new Point(0, 0);
                _contentPanel.Width = Width;
                _contentPanel.Height = Height;
            }

            _barRectangle.Height = Height;
            _barRectangle.X = Width - _barRectangle.Width;
            _thumbRectangle.X = Width - _barRectangle.Width + _thumbPadding;

            OnResizedContentPanel();

            base.OnResize(e);
        }

        public void OnResizedContentPanel() {
            // if the content is not too tall, no need to display the scroll bars
            if (_contentPanel.Height <= Height) {
                _contentPanel.Width = Width;
                HasScrolls = false;
            } else {
                // thumb heigh is a ratio of displayed height and the content panel height
                _thumbRectangle.Height = Math.Max((int) (_barRectangle.Height*((float) Height/_contentPanel.Height)) - _thumbPadding*2, 10);
                _contentPanel.Width = Width - 10;
                HasScrolls = true;
            }

            if (!NoBackgroundImage)
                _contentPanel.DontUseTransparentBackGround = HasScrolls;

            DoScroll(0);
        }

        #endregion

        #region internal content panel

        public class YamuiInternalPanel : YamuiSimplePanel {
            public YamuiScrollPanel OwnerPanel { get; set; }

            public new DockStyle Dock {
                get { return base.Dock; }
                set {
                    OwnerPanel.Dock = value;
                    base.Dock = DockStyle.None;
                }
            }
        }

        #endregion
    }

    internal class ScrollPageDesigner : ParentControlDesigner {
        public override void Initialize(IComponent component) {
            base.Initialize(component);
            EnableDesignMode(((YamuiScrollPanel) Control).ContentPanel, "ContentPanel");
        }
    }
}