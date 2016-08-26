#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    public class YamuiScrollList : UserControl {

        #region fields

        /// <summary>
        /// true if you want to use the BackColor property
        /// </summary>
        public bool UseCustomBackColor { get; set; }

        /// <summary>
        /// Width of the scroll bar
        /// </summary>
        public int ScrollWidth {
            get { return _scrollWidth; }
            set { _scrollWidth = value; }
        }
        private int _scrollWidth = 10;

        /// <summary>
        /// Height of each row
        /// </summary>
        public int RowHeight {
            get { return _rowHeight; }
            set { _rowHeight = value; }
        }
        private int _rowHeight = 22;

        /// <summary>
        /// Action that will be called each time a row needs to be painted
        /// </summary>
        public Action<ScrollListItem, YamuiListRow, PaintEventArgs> OnRowPaint;

        private List<ScrollListItem> _items;

        private int _nbItems;
        public int TopIndex {
            get { return _topIndex; }
        }

        private int _topIndex;

        public int SelectedIndex {
            get { return _selectedIndex; }
            private set {
                _selectedIndex = value;
            }
        }

        private int _selectedIndex;

        private int _hotRow;
        
        private int _nbRowFullyDisplayed;

        private List<YamuiListRow> _rows = new List<YamuiListRow>();
        
        private int _nbRowDisplayed;
        public int SelectedRow {
            get { return _selectedRow; }
            private set {
                _rows[_selectedRow].IsSelected = false;
                _rows[_selectedRow].Invalidate();
                _selectedRow = value;
                _rows[_selectedRow].IsSelected = true;
                _rows[_selectedRow].Invalidate();
                _selectedIndex = _selectedRow + _topIndex;
            }
        }

        private int _selectedRow;

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

        public YamuiScrollList() {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer, true);

            _barRectangle = new Rectangle(Width - ScrollWidth, 0, ScrollWidth, Height);
            _thumbRectangle = new Rectangle(Width - ScrollWidth + _thumbPadding, _thumbPadding, ScrollWidth - _thumbPadding * 2, Height - _thumbPadding * 2);

            ComputeScrollBar();
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.Clear(!UseCustomBackColor ? YamuiThemeManager.Current.FormBack : BackColor);
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
                    //var delta = -((short)(message.WParam.ToInt32() >> 16));
                    MessageBox.Show("kk");
                    break;
                case (int)WinApi.Messages.WM_LBUTTONDOWN:
                    if (!_isPressed) {
                        _isPressed = true;
                        _lastMouseMove = PointToScreen(MousePosition);
                        Invalidate();
                        var mousePosRelativeToThis = PointToClient(MousePosition);
                        if (_barRectangle.Contains(mousePosRelativeToThis) && !_thumbRectangle.Contains(mousePosRelativeToThis)) {
                            //DoScroll(mousePosRelativeToThis.Y - _thumbRectangle.Y);
                        }
                    }
                    break;
                case (int)WinApi.Messages.WM_LBUTTONUP:
                    if (_isPressed) {
                        _isPressed = false;
                        Invalidate();
                    }
                    break;
                case (int)WinApi.Messages.WM_MOUSEMOVE:
                    if (_isPressed) {
                        Point currentlMouse = PointToScreen(MousePosition);
                        if (_lastMouseMove != currentlMouse) {
                            //DoScroll(currentlMouse.Y - _lastMouseMove.Y);
                        }
                        _lastMouseMove = PointToScreen(MousePosition);
                    }
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
                    break;
            }
        }
        
        #endregion

        #region handle item selection / view

        /// <summary>
        /// A key has been pressed on the menu
        /// </summary>
        public bool OnKeyDown(Keys pressedKey) {
            var initialIndex = SelectedIndex;
            do {
                switch (pressedKey) {
                    case Keys.Tab:
                    case Keys.Enter:
                        //OnItemPressed();
                        break;
                    case Keys.Up:
                        SelectedIndex--;
                        break;
                    case Keys.Down:
                        SelectedIndex++;
                        break;
                    case Keys.PageDown:
                        SelectedIndex = _nbItems - 1;
                        break;
                    case Keys.PageUp:
                        SelectedIndex = 0;
                        break;
                    default:
                        return false;
                }
                if (SelectedIndex > _nbItems - 1)
                    SelectedIndex = 0;
                if (SelectedIndex < 0)
                    SelectedIndex = _nbItems - 1;

            } // do this while the current button is disabled and we didn't already try every button
            while (_items[SelectedIndex].IsDisabled && initialIndex != SelectedIndex && _items.Count(item => !item.IsDisabled) > 1);

            bool _topIndexChanged = false;
            if (SelectedIndex < _topIndex) {
                _topIndex = SelectedIndex;
                _topIndexChanged = true;
            } else if (SelectedIndex > _topIndex + _nbRowFullyDisplayed - 1) {
                _topIndex = SelectedIndex - (_nbRowFullyDisplayed - 1);
                _topIndexChanged = true;
            }

            // select button
            SelectedRow = _selectedIndex - _topIndex;
            if (_rows.Count > SelectedRow)
                ActiveControl = _rows[SelectedRow];

            if (_topIndexChanged) {
                RefreshButtons();
                RepositionThumb();
            }

            return true;
        }

        private void OnItemKeyDown(KeyEventArgs keyEventArgs) {
            keyEventArgs.Handled = OnKeyDown(keyEventArgs.KeyCode);
        }

        private void OnItemClick(object sender, EventArgs eventArgs) {
            var args = eventArgs as MouseEventArgs;
            if (args != null) {
                SelectedRow = int.Parse(((YamuiListRow)sender).Name);
                MessageBox.Show(args.Button + " " + SelectedRow);
            }
        }

        private void OnEnter(object sender, EventArgs eventArgs) {
            _hotRow = int.Parse(((YamuiListRow) sender).Name);
        }

        private void OnLeave(object sender, EventArgs eventArgs) {
            if (!new Rectangle(Location, Size).Contains(PointToClient(MousePosition))) {
                MessageBox.Show("out!");
            }
        }

        #endregion

        #region Draw list

        public void SetItems(List<ScrollListItem> listItems) {
            _items = listItems;
            _nbItems = _items.Count;

            DrawButtons();
            RefreshButtons();
            RepositionThumb();
        }

        /// <summary>
        /// This method just add the number of buttons required to display the list
        /// </summary>
        private bool DrawButtons() {

            // we already display the right number of items?
            if ((_nbRowDisplayed - 1) * RowHeight <= Height && Height <= _nbRowDisplayed * RowHeight)
                return false;

            Controls.Clear();

            // how many items should be displayed?
            _nbRowDisplayed = _nbItems.ClampMax(Height / RowHeight + 1);

            // for each displayed item of the list
            for (int i = 0; i < _nbRowDisplayed; i++) {
  
                // need to add button?
                if (_rows.Count <= i + 1) {
                        
                    _rows.Add(new YamuiListRow {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    Location = new Point(0, i * RowHeight),
                    Size = new Size(Width, RowHeight),
                    Name = i.ToString(),
                    TabStop = true,
                    OnPaintAction = OnRowPaint,
                    });
                }

                _rows[i].SuperKeyDown += OnItemKeyDown;
                _rows[i].ButtonPressed += OnItemClick;
               //_buttons[i].Enter += OnEnter;
               //_buttons[i].Leave += OnLeave;

                // add it to the visible controls
                Controls.Add(_rows[i]);
            }

            return true;

        }

        private void RefreshButtons() {

            if (_items != null) {

                // for each displayed item of the list
                for (int i = 0; i < _nbRowDisplayed; i++) {

                    if (_topIndex + i > _nbItems - 1) {
                        _rows[i].Visible = false;
                    } else {
                        // associate with the item
                        var itemBeingDisplayed = _items[_topIndex + i];
                        _rows[i].Tag = itemBeingDisplayed;

                        if (i == _nbRowDisplayed - 1 && !_rows[i].Visible)
                            _rows[i].Visible = true;
                    }

                    // repaint
                    _rows[i].Invalidate();
                }
            }

        }

        #endregion
        
        #region Handle scroll

        protected override void OnResize(EventArgs e) {
            if(DrawButtons())
                RefreshButtons();
            ComputeScrollBar();
            RepositionThumb();
            base.OnResize(e);
        }

        /// <summary>
        /// This method simply reposition the thumb rectangle in the scroll bar according to the current top index
        /// </summary>
        private void RepositionThumb() {
            _thumbRectangle.Location = new Point(_thumbRectangle.X, _thumbPadding + (int) ((float)_topIndex / _nbItems * (_barRectangle.Height - _thumbPadding * 2)));
            Invalidate();
        }

        /// <summary>
        /// This method computes the height of the scrollbar and the thumb inside it, if the scroll bar it reduces the width of the 
        /// buttons so that the scroll bar actually appears
        /// </summary>
        private void ComputeScrollBar() {
            
            _barRectangle.Height = Height;
            _barRectangle.X = Width - _barRectangle.Width;
            _thumbRectangle.X = Width - _barRectangle.Width + _thumbPadding;

            _nbRowFullyDisplayed = (int) Math.Floor((decimal)Height / RowHeight);

            // if the content is not too tall, no need to display the scroll bars
            if (_nbItems * RowHeight <= Height) {
                if (HasScrolls) {
                    foreach (var button in _rows) {
                        button.Width = Width;
                    }
                }
                HasScrolls = false;
            } else {
                if (!HasScrolls) {
                    foreach (var button in _rows) {
                        button.Width = Width - ScrollWidth;
                    }
                }
                HasScrolls = true;

                // thumb height is a ratio of displayed height and the content panel height
                _thumbRectangle.Height = ((int)((_barRectangle.Height - _thumbPadding*2)*((float)_nbRowFullyDisplayed/_nbItems))).ClampMin(1);
            }

        }


        #endregion

        #region YamuiMenuButton

        public class YamuiListRow : YamuiButton {

            /// <summary>
            /// true if the row is selected
            /// </summary>
            public bool IsSelected;

            public Action<ScrollListItem, YamuiListRow, PaintEventArgs> OnPaintAction;

            protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
                e.IsInputKey = true;
                base.OnPreviewKeyDown(e);
            }

            protected override void OnMouseDown(MouseEventArgs e) {
                IsPressed = true;
                Invalidate();
                base.OnMouseDown(e);
            }

            protected override void OnPaint(PaintEventArgs e) {
                if (Tag != null && OnPaintAction != null) {
                    OnPaintAction((ScrollListItem) Tag, this, e);
                } else {
                    e.Graphics.Clear(YamuiThemeManager.Current.MenuNormalBack);
                }
            }
        }

        #endregion

    }

    public class ScrollListItem {

        /// <summary>
        /// The piece of text displayed in the list
        /// </summary>
        public string DisplayText { get; set; }

        public bool IsDisabled { get; set; }
    }

}
