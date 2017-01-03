#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiTabButtons.cs) is part of YamuiFramework.
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
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    /// <summary>
    /// This class displays items on a list o strings, next to each other
    /// each item is clickable and triggers the TabPressed event that provides
    /// the clicked index
    /// </summary>
    public class YamuiTabButtons : Button {

        #region Fields

        /// <summary>
        /// Spacing between items
        /// </summary>
        public int SpaceBetweenText;

        /// <summary>
        /// Writes the elements from the right to the left instead
        /// </summary>
        public bool WriteFromRight { get; set; }

        /// <summary>
        /// Draw a | separator between the items
        /// </summary>
        public bool DrawSeparator { get; set; }

        public bool UseLinksColors { get; set; }

        public event EventHandler<TabPressedEventArgs> TabPressed {
            add { OnTabPressed += value; }
            remove { OnTabPressed -= value; }
        }
        private event EventHandler<TabPressedEventArgs> OnTabPressed;

        private bool _isHovered;
        private bool _isPressed;
#pragma warning disable 414
        private bool _isFocused;
#pragma warning restore 414

        private List<string> _listOfButtons;

        // used to remember the position of each tab
        private Dictionary<int, Rectangle> _buttonsRect = new Dictionary<int, Rectangle>();

        private int _selectedIndex;
        private int _hotIndex;

        #endregion

        #region Constructor

        public YamuiTabButtons(List<string> listOfButtons, int selectedIndex) {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);

            _selectedIndex = selectedIndex;
            _listOfButtons = listOfButtons;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            // background
            e.Graphics.Clear(YamuiThemeManager.Current.FormBack);

            // foreground
            var startingIndex = WriteFromRight ? _listOfButtons.Count - 1 : 0;
            var index = startingIndex;
            for (int i = 0; i < _listOfButtons.Count; i++) {
                var button = _listOfButtons[WriteFromRight ? _listOfButtons.Count - 1 - i : i];

                Color foreColor = UseLinksColors ?
                    YamuiThemeManager.Current.LabelsFg(ForeColor, false, false, (index == _hotIndex && _isHovered), _isPressed, Enabled) :
                    YamuiThemeManager.Current.TabsFg((index == _hotIndex && _isHovered), index == _selectedIndex);

                // get the rectangle in which will fit this item
                Rectangle thisTabRekt;
                if (!_buttonsRect.ContainsKey(index)) {
                    var textWidth = TextRenderer.MeasureText(e.Graphics, button, Font, ClientSize, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding).Width;
                    if (index == startingIndex) {
                        thisTabRekt = WriteFromRight ? new Rectangle(Width - textWidth - SpaceBetweenText, 0, textWidth + SpaceBetweenText, Height) : new Rectangle(0, 0, textWidth + SpaceBetweenText, Height);
                    } else {
                        var lastRect = _buttonsRect.Last().Value;
                        thisTabRekt = WriteFromRight ? new Rectangle(lastRect.X - textWidth - SpaceBetweenText, 0, textWidth + SpaceBetweenText, Height) : new Rectangle(lastRect.X + lastRect.Width, 0, textWidth + SpaceBetweenText, Height);
                    }
                    _buttonsRect.Add(index, thisTabRekt);
                } else {
                    thisTabRekt = GetRect(index);
                }

                // draw the text
                Rectangle textRect = thisTabRekt;
                if (WriteFromRight)
                    textRect.Offset(SpaceBetweenText, 0);
                TextRenderer.DrawText(e.Graphics, button, Font, textRect, foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);

                // draw a | separator?
                if (DrawSeparator && i != (_listOfButtons.Count - 1)) {
                    using (var pen = new Pen(YamuiThemeManager.Current.TabNormalFore, 1)) {
                        var xPos = thisTabRekt.X + (WriteFromRight ? SpaceBetweenText / 2 : thisTabRekt.Width - SpaceBetweenText / 2);
                        e.Graphics.DrawLine(pen, new Point(xPos, (int)(Height * 0.8)), new Point(xPos, Height - (int)(Height * 0.7)));
                    }
                }

                index += WriteFromRight ? -1 : 1;
            }
        }

        #endregion

        #region core

        /// <summary>
        /// Returns the width that will take the control
        /// </summary>
        /// <returns></returns>
        public int GetWidth() {
            return _listOfButtons.Sum(button => TextRenderer.MeasureText(button, Font, ClientSize, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding).Width + SpaceBetweenText);
        }

        public void UpdateList(List<string> listOfButtons, int selectedIndex) {
            _selectedIndex = selectedIndex;
            if (listOfButtons != null) {
                _buttonsRect.Clear();
                _listOfButtons = listOfButtons;
            }
            Refresh();
        }

        private Rectangle GetRect(int index) {
            return _buttonsRect.ContainsKey(index) ? _buttonsRect[index] : new Rectangle();
        }

        private int GetIndexFromLocation(Point loc) {
            // get the index of the button hovered
            for (int i = 0; i < _listOfButtons.Count; i++) {
                if (GetRect(i).Contains(loc))
                    return i;
            }
            return -1;
        }

        private void HandlePressedButton() {
            if (OnTabPressed == null || _selectedIndex == -1) return;
            OnTabPressed(this, new TabPressedEventArgs(_selectedIndex));
        }

        #endregion

        #region Managing isHovered, isPressed, isFocused

        protected override void OnMouseMove(MouseEventArgs e) {
            // get the index of the button hovered
            int hot = GetIndexFromLocation(e.Location);
            if (hot == -1) {
                _hotIndex = hot;
                return;
            }
            if (hot != _hotIndex) {
                _hotIndex = hot;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        #region Focus Methods

        protected override void OnEnter(EventArgs e) {
            _isFocused = true;
            Invalidate();

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            _isFocused = false;
            Invalidate();

            base.OnLeave(e);
        }

        #endregion

        #region Keyboard Methods

        // This is mandatory to be able to handle the ENTER key in key events!!
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Right || e.KeyCode == Keys.Left) e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) {
                _selectedIndex = _hotIndex;
                _isPressed = true;
                Invalidate();
                HandlePressedButton();
                e.Handled = true;
            } else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left) {
                _selectedIndex += e.KeyCode == Keys.Right ? +1 : -1;
                if (_selectedIndex < 0) _selectedIndex = _listOfButtons.Count - 1;
                if (_selectedIndex > _listOfButtons.Count - 1) _selectedIndex = 0;
                _isPressed = true;
                Invalidate();
                HandlePressedButton();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            _isPressed = false;
            Invalidate();
            base.OnKeyUp(e);
        }

        #endregion

        #region Mouse Methods

        protected override void OnMouseEnter(EventArgs e) {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                _selectedIndex = GetIndexFromLocation(e.Location);
                _isPressed = true;
                Invalidate();
                HandlePressedButton();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            _isPressed = false;
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Programatically triggers the OnKeyDown event
        /// </summary>
        public bool PerformKeyDown(KeyEventArgs e) {
            OnKeyDown(e);
            return e.Handled;
        }

        #endregion

    }

    public sealed class TabPressedEventArgs : EventArgs {
        public int SelectedIndex;
        public TabPressedEventArgs(int selectedIndex) {
            SelectedIndex = selectedIndex;
        }
    }
}
