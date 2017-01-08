#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls.YamuiList {

    /// <summary>
    /// A control that allows you to display a list of items super efficiently,
    /// use SetItems to start
    /// </summary>
    public class YamuiScrollList : UserControl {

        #region constants

        /// <summary>
        /// Padding for scrollbar thumb
        /// </summary>
        private const int ThumbPadding = 2;

        /// <summary>
        /// Min height for scroll bar thumb 
        /// </summary>
        private const int MinThumbHeight = 15;

        /// <summary>
        /// Default height for rows
        /// </summary>
        private const int DefaultRowHeight = 18;

        private const int DefaultScrollWidth = 10;

        private const string DefaultEmptyString = @"Empty list!";

        #endregion

        #region private fields

        protected Action<ListItem, YamuiListRow, PaintEventArgs> _onRowPaint;

        protected Padding _listPadding = new Padding(0);

        private string _emptyListString = DefaultEmptyString;

        private int _scrollWidth = DefaultScrollWidth;

        private int _rowHeight = DefaultRowHeight;

        protected List<ListItem> _items;

        protected int _nbItems;

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

        private Rectangle _scrollRectangle;
        private Rectangle _listRectangle;
        private Rectangle _barRectangle;
        private Rectangle _thumbRectangle;

        private bool _isScrollPressed;
        private bool _isScrollHovered;
        private bool _isHovered;
        private bool _isFocused;

        #endregion

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
        public virtual int RowHeight {
            get { return _rowHeight; }
            set { _rowHeight = value; }
        }

        /// <summary>
        /// The text that appears when the list is empty
        /// </summary>
        public string EmptyListString {
            get { return _emptyListString; }
            set { _emptyListString = value; }
        }

        /// <summary>
        /// index of the first item currently displayed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedItemIndex {
            get { return _selectedItemIndex; }
            set {
                _selectedItemIndex = value;
                _selectedItemIndex = _selectedItemIndex.ClampMax(_nbItems - 1);
                _selectedItemIndex = _selectedItemIndex.ClampMin(0);

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
        /// Returns the currently selected item or null if it doesn't exist / disabled
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ListItem SelectedItem {
            get {
                var item = GetItem(SelectedItemIndex);
                return item != null ? (item.IsDisabled ? null : item) : null;
            }
        }

        /// <summary>
        /// Index of the selected row (can be negative or too high if the selected index isn't visible)
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public YamuiListRow SelectedRow {
            get { return 0 <= SelectedRowIndex && SelectedRowIndex < _nbRowDisplayed ? _rows[SelectedRowIndex] : null; }
        }

        /// <summary>
        /// Exposes the states of the scroll bars, true if they are displayed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasScrolls { get; private set; }

        /// <summary>
        /// The row currently hovered by the cursor
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int HotRow {
            get { return _hotRow; }
            private set {
                _hotRow = value;
                if (HotIndexChanged != null)
                    HotIndexChanged(this);
            }
        }

        /// <summary>
        /// Returns the items that currently has the cursor on it
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ListItem HotItem {
            get {
                var item = GetItem(TopIndex + HotRow);
                return item != null ? (item.IsDisabled ? null : item) : null;
            }
        }

        /// <summary>
        /// Is the control hovered by the mouse?
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHovered {
            get { return _isHovered; }
            private set {
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
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFocused {
            get { return _isFocused; }
            private set {
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
        /// The padding to apply to display the list
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Padding ListPadding {
            get { return _listPadding; }
            set {
                _listPadding = value;

                ComputeBaseRectangle();
                ResizeControl();
                Invalidate();

                // reposition buttons
                for (int i = 0; i < _nbRowDisplayed; i++) {
                    _rows[i].Location = new Point(_listRectangle.Left, _listRectangle.Top + i * RowHeight);
                }
            }
        }

        /// <summary>
        /// Action that will be called each time a row needs to be painted
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Action<ListItem, YamuiListRow, PaintEventArgs> OnRowPaint {
            get { return _onRowPaint ?? RowPaint; }
            set { _onRowPaint = value; }
        }

        /// <summary>
        /// Returns the list of items currently displayed in the list, use SetItems to define this list
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual List<ListItem> Items {
            get { return _items; }
        }

        /// <summary>
        /// handle the keys pressed in the control, returns true if you actually handled the key
        /// </summary>
        public Func<YamuiScrollList, bool> KeyPressed { get; set; }

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
        /// Triggered when the selected index changes
        /// </summary>
        public event Action<YamuiScrollList> IndexChanged;        
        
        /// <summary>
        /// Triggered when a row is clicked (no matter with which button)
        /// You can analyse the MouseEventArgs.Clicks number to know if it's a simple or
        /// double click
        /// </summary>
        public event Action<YamuiScrollList, MouseEventArgs> RowClicked;

        /// <summary>
        /// Triggered when the mouse moves on a row
        /// </summary>
        public event Action<YamuiScrollList, MouseEventArgs> RowMouseMove;     
   
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

        #region constructor

        public YamuiScrollList() {

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            // this usercontrol should not be able to get the focus, only the first button can get it
            SetStyle(ControlStyles.Selectable, false);

            ComputeBaseRectangle();
            ComputeScrollBar();
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e) {
            OnPaintBackground(e);

            if (_nbItems > 0) {
                if (HasScrolls)
                    PaintScrollBar(e);
            } else {
                PaintEmptyList(e);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            e.Graphics.Clear(!UseCustomBackColor ? YamuiThemeManager.Current.MenuNormalBack : BackColor);
        }

        /// <summary>
        /// Paint the scroll bar
        /// </summary>
        protected virtual void PaintScrollBar(PaintEventArgs e) {
            Color thumbColor = YamuiThemeManager.Current.ScrollBarsFg(false, _isScrollHovered, _isScrollPressed, Enabled);
            Color barColor = YamuiThemeManager.Current.ScrollBarsBg(false, _isScrollHovered, _isScrollPressed, Enabled);
            if (barColor != Color.Transparent) {
                using (var b = new SolidBrush(barColor)) {
                    e.Graphics.FillRectangle(b, _scrollRectangle);
                }
            }
            using (var b = new SolidBrush(thumbColor)) {
                e.Graphics.FillRectangle(b, _thumbRectangle);
            }
        }

        /// <summary>
        /// Paint empty list
        /// </summary>
        protected virtual void PaintEmptyList(PaintEventArgs e) {
            // empty list
            using (HatchBrush hBrush = new HatchBrush(HatchStyle.WideUpwardDiagonal, YamuiThemeManager.Current.MenuNormalAltBack, Color.Transparent))
                e.Graphics.FillRectangle(hBrush, _listRectangle);

            // text
            var foreColor = YamuiThemeManager.Current.MenuNormalFore;
            var textFont = FontManager.GetFont(FontStyle.Bold, 11);
            var textSize = TextRenderer.MeasureText(EmptyListString, textFont);
            var drawPoint = new Point(_listRectangle.Left + _listRectangle.Width / 2 - textSize.Width / 2 - 1, _listRectangle.Top + _listRectangle.Height / 2 - textSize.Height / 2 - 1);
            using (var b = new SolidBrush(YamuiThemeManager.Current.MenuNormalBack)) {
                e.Graphics.FillRectangle(b, drawPoint.X - 2, drawPoint.Y - 1, textSize.Width + 2, textSize.Height + 3);
            }
            TextRenderer.DrawText(e.Graphics, EmptyListString, textFont, drawPoint, foreColor);
            using (var pen = new Pen(Color.FromArgb((int)(255 * 0.8), foreColor), 1) { Alignment = PenAlignment.Left }) {
                e.Graphics.DrawRectangle(pen, drawPoint.X - 2, drawPoint.Y - 1, textSize.Width + 2, textSize.Height + 3);
            }
        }

        #endregion

        #region set

        /// <summary>
        /// Set the items that will be displayed in the list
        /// </summary>
        public virtual void SetItems(List<ListItem> listItems) {
            if (listItems == null)
                throw new ArgumentNullException();

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

            // Correct the top index if needed, in any case this will Refresh the buttons + reposition the thumb
            TopIndex = TopIndex;
        }
        
        #endregion

        #region DrawButtons

        /// <summary>
        /// This method just add the number of buttons required to display the list
        /// </summary>
        protected virtual void DrawButtons() {

            // we already display the right number of items?
            if ((_nbRowDisplayed - 1) * RowHeight <= _listRectangle.Height && _listRectangle.Height <= _nbRowDisplayed * RowHeight)
                return;

            // how many items should be displayed?
            _nbRowDisplayed = _nbItems.ClampMax(_listRectangle.Height / RowHeight + 1);

            // for each displayed item of the list
            for (int i = 0; i < _nbRowDisplayed; i++) {

                // need to add button?
                if (i >= _rows.Count) {

                    _rows.Add(new YamuiListRow {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                        Name = i.ToString(),
                        TabStop = i == 0,
                        OnRowPaint = OnRowPaint
                    });

                    _rows[i].KeyDown += (sender, args) => {
                        OnKeyDown(args);
                    };
                    _rows[i].ButtonPressed += OnRowClick;
                    _rows[i].DoubleClick += OnRowClick;
                    _rows[i].MouseEnter += (sender, args) => {
                        IsHovered = true;
                        HotRow = int.Parse(((YamuiListRow) sender).Name);
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
                    _rows[i].MouseMove += (sender, args) => {
                        if (RowMouseMove != null)
                                RowMouseMove(this, args);
                    };
                }

                _rows[i].Location = new Point(_listRectangle.Left, _listRectangle.Top + i*RowHeight);
                _rows[i].Size = new Size(_listRectangle.Width - (HasScrolls ? ScrollWidth : 0), RowHeight.ClampMax(_listRectangle.Height - i * RowHeight));
                _rows[i].IsSelected = i == SelectedRowIndex;

                // add it to the visible controls
                if (!Controls.Contains(_rows[i]))
                    Controls.Add(_rows[i]);
            }

            for (int i = _nbRowDisplayed; i < _rows.Count; i++) {
                if (Controls.Contains(_rows[i]))
                    Controls.Remove(_rows[i]);
            }
            
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

                        _rows[i].Height = RowHeight.ClampMax(_listRectangle.Height - i * RowHeight);
                    }

                    // repaint
                    _rows[i].Invalidate();
                }
            }
        }

        #endregion

        #region Draw list

        /// <summary>
        /// Called by default to paint the row if no OnRowPaint is defined
        /// </summary>
        protected virtual void RowPaint(ListItem item, YamuiListRow row, PaintEventArgs e) {
            var backColor = YamuiThemeManager.Current.MenuBg(row.IsSelected, row.IsHovered, !item.IsDisabled);
            var foreColor = YamuiThemeManager.Current.MenuFg(row.IsSelected, row.IsHovered, !item.IsDisabled);

            // background
            e.Graphics.Clear(backColor);

            // case of a separator
            if (item.IsSeparator) {
                var rect = row.ClientRectangle;
                rect.Height = RowHeight;
                RowPaintSeparator(e.Graphics, rect);
                return;
            }

            // foreground
            // left line
            if (row.IsSelected && !item.IsDisabled) {
                using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.AccentColor)) {
                    e.Graphics.FillRectangle(b, new Rectangle(0, 0, 3, row.ClientRectangle.Height));
                }
            }

            // text
            TextRenderer.DrawText(e.Graphics, item.DisplayText, FontManager.GetStandardFont(), new Rectangle(5, 0, row.ClientRectangle.Width - 5, RowHeight), foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }

        protected virtual void RowPaintSeparator(Graphics g, Rectangle drawRect) {
            using (SolidBrush b = new SolidBrush(YamuiThemeManager.Current.FormAltBack)) {
                var width = (int)(drawRect.Width * 0.45);
                g.FillRectangle(b, new Rectangle(0, drawRect.Y + drawRect.Height / 2 - 2, width, 4));
            }
        }

        #endregion

        #region Handle scroll

        protected override void OnResize(EventArgs e) {
            ResizeControl();
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
        protected void HandleWindowsProc(Message message) {
            switch (message.Msg) {
                case (int) WinApi.Messages.WM_MOUSEWHEEL:
                    // delta negative when scrolling up
                    var delta = -(short)(message.WParam.ToInt32() >> 16);
                    TopIndex += Math.Sign(delta) * _nbRowFullyDisplayed / 2;
                    break;

                case (int)WinApi.Messages.WM_LBUTTONDOWN:
                    var mousePosRelativeToThis = PointToClient(MousePosition);

                    // mouse in scrollbar
                    if (_scrollRectangle.Contains(mousePosRelativeToThis)) {

                        // mouse in thumb
                        if (_thumbRectangle.Contains(mousePosRelativeToThis)) {
                            _isScrollPressed = true;
                            Invalidate();
                            _yPosOnThumb = mousePosRelativeToThis.Y - _thumbRectangle.Y;
                        } else {
                            ThumbPosToTopIndex(mousePosRelativeToThis.Y);
                        }

                        // give focus back to the control
                        GrabFocus();
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

        protected override void OnMouseEnter(EventArgs e) {
            IsHovered = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            if (!new Rectangle(new Point(0, 0), Size).Contains(PointToClient(MousePosition)))
                IsHovered = false;
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Converts a thumb Y location to a top index
        /// </summary>
        protected void ThumbPosToTopIndex(int yPos) {
            yPos = yPos.Clamp(ThumbPadding, _barRectangle.Height - ThumbPadding - _thumbRectangle.Height);
            var percent = (yPos - ThumbPadding) / (float)(_barRectangle.Height - ThumbPadding * 2 - _thumbRectangle.Height);
            var newTopIndex = (int) Math.Round(percent * (_nbItems - _nbRowFullyDisplayed));
            if (TopIndex != newTopIndex)
                TopIndex = newTopIndex;
        }

        /// <summary>
        /// This method simply reposition the thumb rectangle in the scroll bar according to the current top index
        /// </summary>
        protected void RepositionThumb() {
            _thumbRectangle.Location = new Point(_thumbRectangle.X, _barRectangle.Top + ThumbPadding + (int)((float)TopIndex / _nbItems * (_barRectangle.Height - ThumbPadding * 2)));
            Invalidate();
        }

        /// <summary>
        /// This method computes the height of the scrollbar and the thumb inside it, if the scroll bar it reduces the width of the 
        /// buttons so that the scroll bar actually appears
        /// </summary>
        protected void ComputeScrollBar() {
            // get the new list rectangle
            _listRectangle = new Rectangle(ListPadding.Left, ListPadding.Top, Width - ListPadding.Horizontal, Height - ListPadding.Vertical);

            _barRectangle.Height = _scrollRectangle.Height = _listRectangle.Height;
            _barRectangle.X = _scrollRectangle.X = _listRectangle.Left + _listRectangle.Width - _barRectangle.Width;
            _thumbRectangle.X = _listRectangle.Left + _listRectangle.Width - _barRectangle.Width + ThumbPadding;

            _nbRowFullyDisplayed = (int)Math.Floor((decimal)_listRectangle.Height / RowHeight);

            // if the content is not too tall, no need to display the scroll bars
            if (_nbItems * RowHeight <= _listRectangle.Height) {
                foreach (var button in _rows) {
                    button.Width = _listRectangle.Width;
                }
                HasScrolls = false;
            } else {
                foreach (var button in _rows) {
                    button.Width = _listRectangle.Width - ScrollWidth;
                }
                HasScrolls = true;

                // thumb height is a ratio of displayed height and the content panel height
                _thumbRectangle.Height = (int)((_barRectangle.Height - ThumbPadding * 2) * ((float)_nbRowFullyDisplayed / _nbItems));
                if (_thumbRectangle.Height < MinThumbHeight) {
                    _thumbRectangle.Height = MinThumbHeight;
                    _barRectangle.Height = _listRectangle.Height - MinThumbHeight;
                }
            }

        }
        
        #endregion

        #region Events pushed from the button rows

        protected override void OnKeyDown(KeyEventArgs e) {
            e.Handled = HandleKeyDown(e.KeyCode);
            if (!e.Handled)
                base.OnKeyDown(e);
        }

        /// <summary>
        /// Handles the key pressed for the control
        /// </summary>
        private bool HandleKeyDown(Keys pressedKey) {
            var newIndex = SelectedItemIndex;

            switch (pressedKey) {
                case Keys.End:
                    newIndex = _nbItems;
                    break;

                case Keys.Home:
                    newIndex = 0;
                    break;

                default:
                    do {
                        switch (pressedKey) {
                            case Keys.Tab:
                                if (TabPressed != null) {
                                    TabPressed(this);
                                    return true;
                                }
                                return false;

                            case Keys.Enter:
                                if (EnterPressed != null) {
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
                                if (SelectedRowIndex == _nbRowFullyDisplayed - 1)
                                    newIndex += _nbRowFullyDisplayed;
                                else
                                    newIndex = TopIndex + _nbRowFullyDisplayed - 1;
                                pressedKey = Keys.Down;
                                break;

                            case Keys.PageUp:
                                if (SelectedRowIndex == 0)
                                    newIndex -= _nbRowFullyDisplayed;
                                else
                                    newIndex = TopIndex;
                                pressedKey = Keys.Up;
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
                        if (_nbItems == 0)
                            return false;

                    } // do this while the current button is disabled and we didn't already try every button
                    while (_items[newIndex].IsDisabled && SelectedItemIndex != newIndex);
                    break;
            }

            SelectedItemIndex = newIndex;

            return true;
        }

        /// <summary>
        /// Click on a row
        /// </summary>
        private void OnRowClick(object sender, EventArgs eventArgs) {
            var args = eventArgs as MouseEventArgs;
            if (args != null) {
                // can't select a disabled item
                var rowIndex = int.Parse(((YamuiListRow) sender).Name);
                var newItem = GetItem(rowIndex + TopIndex);
                if (newItem != null && newItem.IsDisabled)
                    return;

                // change the selected row
                SelectedRowIndex = rowIndex;

                OnItemClick(args);
            }
        }

        /// <summary>
        /// Click on an item, SelectedItem is usable at this time
        /// </summary>
        protected virtual void OnItemClick(MouseEventArgs eventArgs) {
            if (RowClicked != null)
                RowClicked(this, eventArgs);
        }

        #endregion

        #region Misc

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            return e.Handled;
        }

        /// <summary>
        /// Call this method to make the list focused
        /// </summary>
        public void GrabFocus() {
            if (_nbRowDisplayed > 0)
                ActiveControl = _rows[0];
        }

        /// <summary>
        /// Return the item at the given index (or null)
        /// </summary>
        protected ListItem GetItem(int index) {
            return 0 <= index && index < _nbItems ? _items[index] : null;
        }

        /// <summary>
        /// Computes the rectangles used to draw the control from the ListPadding and ClientRectangle
        /// </summary>
        private void ComputeBaseRectangle() {
            _listRectangle = new Rectangle(ListPadding.Left, ListPadding.Top, Width - ListPadding.Horizontal, Height - ListPadding.Vertical);
            _scrollRectangle = new Rectangle(_listRectangle.Left + _listRectangle.Width - ScrollWidth, _listRectangle.Top, ScrollWidth, _listRectangle.Height);
            _barRectangle = _scrollRectangle;
            _thumbRectangle = new Rectangle(_listRectangle.Left + _listRectangle.Width - ScrollWidth + ThumbPadding, _listRectangle.Top + ThumbPadding, ScrollWidth - ThumbPadding * 2, _listRectangle.Height - ThumbPadding * 2);
        }

        /// <summary>
        /// to be called when the padding/size of the control changes
        /// </summary>
        private void ResizeControl() {
            ComputeScrollBar();
            DrawButtons();

            // reposition top index if needed (also refresh buttons and thumb)
            TopIndex = TopIndex; 
        }

        #endregion

        #region YamuiMenuButton

        public class YamuiListRow : YamuiButton {

            /// <summary>
            /// true if the row is selected
            /// </summary>
            public bool IsSelected;

            public Action<ListItem, YamuiListRow, PaintEventArgs> OnRowPaint;

            public YamuiListRow() {
                // by default, buttons don't handle double click since a button in windows can only be simply clicked
                // here we activate the double click
                SetStyle(
                    ControlStyles.StandardClick | 
                    ControlStyles.StandardDoubleClick, 
                    true);
            }

            /// <summary>
            /// redirect all input key to keydown
            /// </summary>
            protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
                e.IsInputKey = true;
                base.OnPreviewKeyDown(e);
            }

            protected override void OnPaint(PaintEventArgs e) {
                if (Tag != null) {
                    OnRowPaint((ListItem) Tag, this, e);
                } else {
                    e.Graphics.Clear(YamuiThemeManager.Current.MenuNormalBack);
                }
            }
        }

        #endregion

    }

}
