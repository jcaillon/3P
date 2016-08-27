#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiScrollList.cs) is part of YamuiFramework.
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
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    /// <summary>
    /// A control that allows you to display a list of items super efficiently
    /// </summary>
    public class YamuiScrollList : UserControl {

        #region public properties

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

        /// <summary>
        /// Height of each row
        /// </summary>
        public int RowHeight {
            get { return _rowHeight; }
            set { _rowHeight = value; }
        }

        /// <summary>
        /// index of the first item currently displayed
        /// </summary>
        public int TopIndex {
            get { return _topIndex; }
            set {
                _topIndex = value;
                _topIndex = _topIndex.ClampMax(_nbItems - _nbRowFullyDisplayed);
                _topIndex = _topIndex.ClampMin(0);
                
                RefreshButtons();
                RepositionThumb();

                // activate/select the correct button button
                SelectedRowIndex = _selectedItemIndex - TopIndex;
            }
        }

        /// <summary>
        /// Index of the item currently selected
        /// </summary>
        public int SelectedItemIndex {
            get { return _selectedItemIndex; }
            set {
                _selectedItemIndex = value;
                _selectedItemIndex = _selectedItemIndex.ClampMax(_nbItems - 1);
                _selectedItemIndex = _selectedItemIndex.ClampMin(0);

                // make sure to select a 

                // do we need to change the top index?
                if (_selectedItemIndex < TopIndex) {
                    TopIndex = _selectedItemIndex;
                } else if (_selectedItemIndex > TopIndex + _nbRowFullyDisplayed - 1) {
                    TopIndex = _selectedItemIndex - (_nbRowFullyDisplayed - 1);
                }

                // activate/select the correct button button
                SelectedRowIndex = _selectedItemIndex - TopIndex;

                if (IndexChanged != null)
                    IndexChanged(this);
            }
        }

        /// <summary>
        /// Returns the currently selected item or null if it doesn't exist
        /// </summary>
        public ListItem SelectedItem {
            get {
                var item = GetItem(SelectedItemIndex);
                return item != null ? (item.IsDisabled ? null : item) : null;
            }
        }

        /// <summary>
        /// Index of the selected row (can be negative or too high if the selected index isn't visible)
        /// </summary>
        private int SelectedRowIndex {
            get { return _selectedRowIndex; }
            set {
                // select the row
                if (0 <= _selectedRowIndex && _selectedRowIndex < _nbRowDisplayed) {
                    _rows[_selectedRowIndex].IsSelected = false;
                    _rows[_selectedRowIndex].Invalidate();
                }
                _selectedRowIndex = value;
                if (0 <= _selectedRowIndex && _selectedRowIndex <_nbRowDisplayed) {
                    _rows[_selectedRowIndex].IsSelected = true;
                    _rows[_selectedRowIndex].Invalidate();
                    _selectedItemIndex = _selectedRowIndex + TopIndex;
                }
            }
        }

        /// <summary>
        /// Returns the currently selected row or null if it doesn't exist
        /// </summary>
        public YamuiListRow SelectedRow {
            get { return 0 <= SelectedRowIndex && SelectedRowIndex < _nbRowDisplayed ? _rows[SelectedRowIndex] : null; }
        }

        /// <summary>
        /// Exposes the states of the scroll bars, true if they are displayed
        /// </summary>
        public bool HasScrolls { get; private set; }

        /// <summary>
        /// The row currently hovered by the cursor
        /// </summary>
        public int HotRow {
            get { return _hotRow; }
            set {
                _hotRow = value;
                if (HotIndexChanged != null)
                    HotIndexChanged(this);
            }
        }

        /// <summary>
        /// Is the control hovered by the mouse?
        /// </summary>
        public bool IsHovered {
            get { return _isHovered; }
            set {
                if (_isHovered != value) {
                    _isHovered = value;
                    if (_isHovered) {
                        if (MouseEntered != null)
                            MouseEntered(this);
                    } else {
                        if (MouseLeft != null)
                            MouseLeft(this);
                    }
                }
            }
        }

        /// <summary>
        /// The control has focus?
        /// </summary>
        public bool IsFocused {
            get { return _isFocused; }
            set {
                if (_isFocused != value) {
                    if (value) {
                        _isFocused = true;
                        if (FocusGained != null)
                            FocusGained(this);
                    } else {
                        if (!Controls.Contains(ActiveControl)) {
                            if (FocusLost != null)
                                FocusLost(this);
                            _isFocused = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The text that appears when the list is empty
        /// </summary>
        public string EmptyListString {
            get { return _emptyListString; }
            set { _emptyListString = value; }
        }

        /// <summary>
        /// Action that will be called each time a row needs to be painted
        /// </summary>
        public Action<ListItem, YamuiListRow, PaintEventArgs> OnRowPaint;

        #endregion

        #region public Events

        /// <summary>
        /// Triggered when the TAB key is pressed
        /// </summary>
        public event Action<YamuiScrollList> TabPressed;

        /// <summary>
        /// Triggered when the ENTER key is pressed
        /// </summary>
        public event Action<YamuiScrollList> EnterPressed;

        /// <summary>
        /// handle the keys pressed in the control, returns true if you actually handled the key
        /// </summary>
        public Func<YamuiScrollList, bool> KeyPressed;

        /// <summary>
        /// Triggered when the selected index changes
        /// </summary>
        public event Action<YamuiScrollList> IndexChanged;        
        
        /// <summary>
        /// Triggered when a row is clicked (no matter with which button)
        /// </summary>
        public event Action<YamuiScrollList, MouseEventArgs> RowClicked;     
   
        /// <summary>
        /// Triggered when the row hovered changes
        /// </summary>
        public event Action<YamuiScrollList> HotIndexChanged;

        /// <summary>
        /// Triggered when the mouse enters a row
        /// </summary>
        public event Action<YamuiScrollList> MouseEnteredRow;

        /// <summary>
        /// Triggered when the mouse leaves a row
        /// </summary>
        public event Action<YamuiScrollList> MouseLeftRow;

        /// <summary>
        /// Triggered when the mouse enters the control
        /// </summary>
        public event Action<YamuiScrollList> MouseEntered;

        /// <summary>
        /// Triggered when the mouse leaves the control
        /// </summary>
        public event Action<YamuiScrollList> MouseLeft;

        /// <summary>
        /// Triggered when the control has the focus
        /// </summary>
        public event Action<YamuiScrollList> FocusGained;

        /// <summary>
        /// Triggered when the control loses the focus
        /// </summary>
        public event Action<YamuiScrollList> FocusLost;

        #endregion

        #region private fields

        private string _emptyListString = @"Empty list!";

        private int _scrollWidth = 10;
        
        private int _rowHeight = 22;
        
        private List<ListItem> _items;

        private int _nbItems;
        
        private int _topIndex;
        
        private int _selectedItemIndex;

        private int _hotRow;
        
        private int _nbRowFullyDisplayed;

        /// <summary>
        /// Collection of rows
        /// </summary>
        private List<YamuiListRow> _rows = new List<YamuiListRow>();
        
        private int _nbRowDisplayed;

        private int _selectedRowIndex;

        private int _yPosOnThumb;

        private int _thumbPadding = 2;

        private int _minThumbHeight = 5;

        private Rectangle _barRectangle;
        private Rectangle _thumbRectangle;

        private bool _isScrollPressed;
        private bool _isScrollHovered;
        private bool _isHovered;
        private bool _isFocused;

        #endregion

        #region constructor

        public YamuiScrollList() {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer, true);

            // this usercontrol should not be able to get the focus, only the first button can get it
            SetStyle(ControlStyles.Selectable, false);

            _barRectangle = new Rectangle(Width - ScrollWidth, 0, ScrollWidth, Height);
            _thumbRectangle = new Rectangle(Width - ScrollWidth + _thumbPadding, _thumbPadding, ScrollWidth - _thumbPadding * 2, Height - _thumbPadding * 2);

            ComputeScrollBar();

            MouseEnter += (sender, args) => IsHovered = true;
            MouseLeave += (sender, args) => {
                if (!new Rectangle(new Point(0, 0), Size).Contains(PointToClient(MousePosition)))
                    IsHovered = false;
            };
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.Clear(!UseCustomBackColor ? YamuiThemeManager.Current.MenuNormalBack : BackColor);
            if (_nbItems > 0) {
                if (HasScrolls)
                    OnPaintForeground(e);
            } else {
                // empty list
                using (HatchBrush hBrush = new HatchBrush(HatchStyle.WideUpwardDiagonal, YamuiThemeManager.Current.MenuNormalAltBack, Color.Transparent))
                    e.Graphics.FillRectangle(hBrush, ClientRectangle);
                
                // text
                var foreColor = YamuiThemeManager.Current.MenuNormalFore;
                var textFont = FontManager.GetFont(FontStyle.Bold, 11);
                var textSize = TextRenderer.MeasureText(_emptyListString, textFont);
                var drawPoint = new PointF(ClientRectangle.Width / 2 - textSize.Width / 2 - 1, ClientRectangle.Height / 2 - textSize.Height / 2 - 1);
                using (var b = new SolidBrush(YamuiThemeManager.Current.MenuNormalBack)) {
                    e.Graphics.FillRectangle(b, drawPoint.X - 2, drawPoint.Y - 1, textSize.Width + 2, textSize.Height + 3);
                }
                e.Graphics.DrawString("Empty list!", textFont, new SolidBrush(Color.FromArgb(255, foreColor)), drawPoint);
                using (var pen = new Pen(Color.FromArgb((int)(255 * 0.8), foreColor), 1) { Alignment = PenAlignment.Left }) {
                    e.Graphics.DrawRectangle(pen, drawPoint.X - 2, drawPoint.Y - 1, textSize.Width + 2, textSize.Height + 3);
                }
            }
        }

        /// <summary>
        /// Paint the scroll bar
        /// </summary>
        protected virtual void OnPaintForeground(PaintEventArgs e) {
            Color thumbColor = YamuiThemeManager.Current.ScrollBarsFg(false, _isScrollHovered, _isScrollPressed, Enabled);
            Color barColor = YamuiThemeManager.Current.ScrollBarsBg(false, _isScrollHovered, _isScrollPressed, Enabled);
            if (barColor != Color.Transparent) {
                using (var b = new SolidBrush(barColor)) {
                    e.Graphics.FillRectangle(b, new Rectangle(Width - ScrollWidth, 0, ScrollWidth, Height));
                }
            }
            using (var b = new SolidBrush(thumbColor)) {
                e.Graphics.FillRectangle(b, _thumbRectangle);
            }
        }

        #endregion
        
        #region Draw list

        /// <summary>
        /// Set the items that will be displayed in the list
        /// </summary>
        public void SetItems(List<ListItem> listItems) {
            _items = listItems;
            _nbItems = _items.Count;

            ComputeScrollBar();
            DrawButtons();

            // make sure to select an index that exists
            SelectedItemIndex = SelectedItemIndex;

            // and an enabled item!
            if (_nbItems > SelectedItemIndex) {
                if (_items[SelectedItemIndex].IsDisabled) {
                    var newIndex = SelectedItemIndex;
                    do {
                        newIndex++;
                        if (newIndex > _nbItems - 1)
                            newIndex = 0;

                    } // do this while the current button is disabled and we didn't already try every button
                    while (_items[newIndex].IsDisabled && SelectedItemIndex != newIndex);
                    SelectedItemIndex = newIndex;
                }
            }

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
                        TabStop = i == 0,
                        IsSelected = i == SelectedRowIndex,
                        OnRowPaint = OnRowPaint,
                        Visible = true
                    });

                    _rows[i].SuperKeyDown += args => args.Handled = OnKeyDown(args.KeyCode);
                    _rows[i].ButtonPressed += OnItemClick;

                    _rows[i].MouseEnter += (sender, args) => {
                        IsHovered = true;
                        HotRow = int.Parse(((YamuiListRow)sender).Name);
                        if (MouseEnteredRow != null)
                            MouseEnteredRow(this);
                    };
                    _rows[i].MouseLeave += (sender, args) => {
                        if (!new Rectangle(new Point(0, 0), Size).Contains(PointToClient(MousePosition)))
                            IsHovered = false;
                        if (MouseLeftRow != null)
                            MouseLeftRow(this);
                    };

                    _rows[i].Enter += (sender, args) => IsFocused = true;
                    _rows[i].Leave += (sender, args) => IsFocused = false;
                }

                // add it to the visible controls
                Controls.Add(_rows[i]);
            }
            
            return true;
        }

        /// <summary>
        /// Refresh all the buttons to display the right items
        /// </summary>
        private void RefreshButtons() {

            if (_items != null) {

                // for each displayed item of the list
                for (int i = 0; i < _nbRowDisplayed; i++) {

                    if (TopIndex + i > _nbItems - 1) {
                        _rows[i].Visible = false;
                    } else {
                        // associate with the item
                        var itemBeingDisplayed = _items[TopIndex + i];
                        _rows[i].Tag = itemBeingDisplayed;

                        if (!_rows[i].Visible)
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
            ComputeScrollBar();
            if (DrawButtons())
                RefreshButtons();
            RepositionThumb();
            base.OnResize(e);
        }

        [SecuritySafeCritical]
        protected override void WndProc(ref Message message) {
            if (HasScrolls)
                HandleWindowsProc(message);
            base.WndProc(ref message);
        }

        /// <summary>
        /// when the scroll bar is visible we listen to messages to handle the scroll
        /// </summary>
        private void HandleWindowsProc(Message message) {
            switch (message.Msg) {
                case (int) WinApi.Messages.WM_MOUSEWHEEL:
                    // delta negative when scrolling up
                    var delta = -(short)(message.WParam.ToInt32() >> 16);
                    TopIndex += Math.Sign(delta) * _nbRowFullyDisplayed / 2;
                    break;

                case (int)WinApi.Messages.WM_LBUTTONDOWN:
                    var mousePosRelativeToThis = PointToClient(MousePosition);

                    // mouse in scrollbar
                    if (_barRectangle.Contains(mousePosRelativeToThis)) {

                        // mouse in thumb
                        if (_thumbRectangle.Contains(mousePosRelativeToThis)) {
                            _isScrollPressed = true;
                            Invalidate();
                            _yPosOnThumb = mousePosRelativeToThis.Y - _thumbRectangle.Y;
                        } else {
                            ThumbPosToTopIndex(mousePosRelativeToThis.Y);
                        }
                    }
                    break;

                case (int)WinApi.Messages.WM_LBUTTONUP:
                    if (_isScrollPressed) {
                        _isScrollPressed = false;
                        Invalidate();
                    }
                    break;

                case (int)WinApi.Messages.WM_MOUSEMOVE:
                    // hover thumb
                    var mousePosRelativeToThis2 = PointToClient(MousePosition);
                    if (_thumbRectangle.Contains(mousePosRelativeToThis2)) {
                        _isScrollHovered = true;
                        Invalidate();
                    } else {
                        if (_isScrollHovered) {
                            _isScrollHovered = false;
                            Invalidate();
                        }
                    }

                    // moving thumb
                    if (_isScrollPressed) {
                        ThumbPosToTopIndex(mousePosRelativeToThis2.Y - _yPosOnThumb);
                    }

                    break;
            }
        }

        /// <summary>
        /// Converts a thumb Y location to a top index
        /// </summary>
        private void ThumbPosToTopIndex(int yPos) {
            yPos = yPos.Clamp(_thumbPadding, _barRectangle.Height - _thumbPadding - _thumbRectangle.Height);
            var percent = (yPos - _thumbPadding) / (float)(_barRectangle.Height - _thumbPadding * 2 - _thumbRectangle.Height);
            var newTopIndex = (int) Math.Round(percent * (_nbItems - _nbRowFullyDisplayed));
            if (TopIndex != newTopIndex)
                TopIndex = newTopIndex;
        }

        /// <summary>
        /// This method simply reposition the thumb rectangle in the scroll bar according to the current top index
        /// </summary>
        private void RepositionThumb() {
            _thumbRectangle.Location = new Point(_thumbRectangle.X, _thumbPadding + (int)((float)TopIndex / _nbItems * (_barRectangle.Height - _thumbPadding * 2)));
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

            _nbRowFullyDisplayed = (int)Math.Floor((decimal)Height / RowHeight);

            // if the content is not too tall, no need to display the scroll bars
            if (_nbItems * RowHeight <= Height) {
                foreach (var button in _rows) {
                    button.Width = Width;
                }
                HasScrolls = false;
            } else {
                foreach (var button in _rows) {
                    button.Width = Width - ScrollWidth;
                }
                HasScrolls = true;

                // thumb height is a ratio of displayed height and the content panel height
                _thumbRectangle.Height = (int)((_barRectangle.Height - _thumbPadding * 2) * ((float)_nbRowFullyDisplayed / _nbItems));
                if (_thumbRectangle.Height < _minThumbHeight) {
                    _thumbRectangle.Height = _minThumbHeight;
                    _barRectangle.Height = Height - _minThumbHeight;
                }
            }

        }
        
        #endregion

        #region Events pushed from the button rows

        /// <summary>
        /// A key has been pressed on the menu
        /// </summary>
        public bool OnKeyDown(Keys pressedKey) {
            var newIndex = SelectedItemIndex;
            do {
                switch (pressedKey) {
                    case Keys.Tab:
                        if (TabPressed != null) {
                            TabPressed(this);
                            return true;
                        }
                        return false;
                    case Keys.Enter:
                        if (EnterPressed != null){
                            EnterPressed(this);
                            return true;
                        }
                        return false;
                    case Keys.Up:
                        newIndex--;
                        break;
                    case Keys.Down:
                        newIndex++;
                        break;
                    case Keys.PageDown:
                        newIndex = _nbItems - 1;
                        break;
                    case Keys.PageUp:
                        newIndex = 0;
                        break;
                    default:
                        if (KeyPressed != null)
                            return KeyPressed(this);
                        return false;
                }
                if (newIndex > _nbItems - 1)
                    newIndex = 0;
                if (newIndex < 0) 
                    newIndex = _nbItems - 1;

            } // do this while the current button is disabled and we didn't already try every button
            while (_items[newIndex].IsDisabled && SelectedItemIndex != newIndex);

            SelectedItemIndex = newIndex;

            return true;
        }

        /// <summary>
        /// Click on a row
        /// </summary>
        private void OnItemClick(object sender, EventArgs eventArgs) {
            var args = eventArgs as MouseEventArgs;
            if (args != null) {
                // can't select a disabled item
                var rowIndex = int.Parse(((YamuiListRow) sender).Name);
                var newItem = GetItem(rowIndex + TopIndex);
                if (newItem != null && newItem.IsDisabled)
                    return;

                // change the selected row
                SelectedRowIndex = rowIndex;

                if (RowClicked != null)
                    RowClicked(this, args);                
            }
        }

        #endregion

        #region Misc

        /// <summary>
        /// Call this method to make the list focused
        /// </summary>
        public void GrabFocus() {
            if (_nbRowDisplayed > 0)
                ActiveControl = _rows[0];
        }

        /// <summary>
        /// Return the item at the given index
        /// </summary>
        public ListItem GetItem(int index) {
            return 0 <= index && index < _nbItems ? _items[index] : null;
        }

        #endregion

        
        #region YamuiMenuButton

        public class YamuiListRow : YamuiButton {

            /// <summary>
            /// true if the row is selected
            /// </summary>
            public bool IsSelected;

            public Action<ListItem, YamuiListRow, PaintEventArgs> OnRowPaint;

            /// <summary>
            /// redirect all input key to keydown
            /// </summary>
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
                if (Tag != null && OnRowPaint != null) {
                    OnRowPaint((ListItem) Tag, this, e);
                } else {
                    e.Graphics.Clear(YamuiThemeManager.Current.MenuNormalBack);
                }
            }
        }

        #endregion

    }

}
