#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiComboBox.cs) is part of YamuiFramework.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Fonts;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    [Designer("YamuiFramework.Controls.YamuiComboBoxDesigner")]
    [ToolboxBitmap(typeof(ComboBox))]
    public sealed class YamuiComboBox : YamuiButton {

        #region events

        public event EventHandler<EventArgs> SelectedIndexChanged;

        public event Action<YamuiComboBox> SelectedIndexChangedByUser;

        #endregion

        #region Fields

        [DefaultValue(ContentAlignment.MiddleLeft)]
        public override ContentAlignment TextAlign {
            get { return _textAlign; }
            set { _textAlign = value; }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("")]
        [Category("Yamui")]
        public string WaterMark {
            get { return _waterMark; }
            set {
                _waterMark = value.Trim();
                Invalidate();
            }
        }

        /// <summary>
        /// Set the datasource for this combo box
        /// </summary>
        public object DataSource {
            get {
                return _originalDataSource;
            }
            set {
                _originalDataSource = value;
                var enumerable = _originalDataSource as IEnumerable;
                if (enumerable == null)
                    throw new Exception("Datasource expects an IEnumerable object");
                _datasource = enumerable.Cast<object>().ToList();
                _listItems.Clear();

                InitList();
            }
        }

        public String DisplayMember { get; set; }

        public String ValueMember { get; set; }

        public int SelectedIndex {
            get {
                return _selectedIndex;
            }
            set {
                if (value >= 0 && value < _listItems.Count) {
                    _selectedIndex = value;
                    if (SelectedIndexChanged != null)
                        SelectedIndexChanged(this, new EventArgs());
                    RefreshText();
                }
            }
        }

        public object SelectedItem {
            get {
                return SelectedIndex >= 0 && SelectedIndex < _listItems.Count ? _listItems[SelectedIndex].BaseValue : null;
            }
        }

        public String SelectedText {
            get {
                return SelectedIndex >= 0 && SelectedIndex < _listItems.Count ? _listItems[SelectedIndex].DisplayText : null;
            }
            set {
                var newIdx = _listItems.FindIndex(item => item.DisplayText.Equals(value));
                if (newIdx >= 0 && newIdx < _listItems.Count)
                    SelectedIndex = newIdx;
            }
        }
        public object SelectedValue {
            get {
                var item = SelectedItem;
                if (item == null)
                    return null;
                try {
                    var field = item.GetType().GetField(ValueMember);
                    if (field.FieldType == typeof(string)) {
                        return field.GetValue(item);
                    }
                } catch (Exception) {
                    try {
                        var prop = item.GetType().GetProperty(ValueMember);
                        if (prop.PropertyType == typeof(string)) {
                            return prop.GetValue(item, null);
                        }
                    } catch (Exception) {
                        if (item is string)
                            return item;
                        throw new Exception("Unknow property or field : " + ValueMember);
                    }
                }
                return null;
            }
            set {
                var newIdx = -1;
                var i = 0;
                foreach (var item in _listItems.Select(item => item.BaseValue)) {
                    try {
                        var field = item.GetType().GetField(ValueMember);
                        if (field.FieldType == typeof(string)) {
                            if (((string) field.GetValue(item)).Equals(value)) {
                                newIdx = i;
                                break;
                            }
                        }
                    } catch (Exception) {
                        try {
                            var prop = item.GetType().GetProperty(ValueMember);
                            if (prop.PropertyType == typeof(string)) {
                                if (((string)prop.GetValue(item, null)).Equals(value)) {
                                    newIdx = i;
                                    break;
                                }
                            }
                        } catch (Exception) {
                            var s = item as string;
                            if (s != null) {
                                if (s.Equals(value)) {
                                    newIdx = i;
                                    break;
                                }
                            } else
                                throw new Exception("Unknow property or field : " + ValueMember);
                        }
                    }
                    i++;
                }
                if (newIdx >= 0 && newIdx < _listItems.Count)
                    SelectedIndex = newIdx;
            }
        }
        
        #endregion

        #region private

        private string _waterMark = "";
        private object _originalDataSource;
        private List<object> _datasource;
        private List<YamuiComboItem> _listItems = new List<YamuiComboItem>();
        private int _selectedIndex;
        private ContentAlignment _textAlign = ContentAlignment.MiddleLeft;
        private YamuiMenu _listPopup;

        #endregion

        #region private methods

        private void RefreshText() {
            if (SelectedIndex < _listItems.Count) {
                Text = _listItems[SelectedIndex].DisplayText;
                Invalidate();
            }
        }
        private void InitList() {

            // simple list of strings?
            if (_datasource != null) {
                if (_datasource.Exists(o => o is string)) {
                    var i = 0;
                    _listItems = _datasource.Select(o => new YamuiComboItem {DisplayText = (string) o, BaseValue = o, Index = i++}).ToList();
                } else {
                    var i = 0;
                    foreach (var obj in _datasource) {
                        string displayText = null;
                        try {
                            var field = obj.GetType().GetField(DisplayMember);
                            if (field.FieldType == typeof(string)) {
                                displayText = (string) field.GetValue(obj);
                            }
                        } catch (Exception) {
                            try {
                                var prop = obj.GetType().GetProperty(DisplayMember);
                                if (prop.PropertyType == typeof(string)) {
                                    displayText = (string) prop.GetValue(obj, null);
                                }
                            } catch (Exception) {
                                if (obj is string)
                                    displayText = (string) obj;
                                else
                                // ReSharper disable once ConstantNullCoalescingCondition
                                    throw new Exception("Unknow property or field : " + DisplayMember ?? "null");
                            }
                        }
                        _listItems.Add(new YamuiComboItem {DisplayText = displayText, BaseValue = obj, Index = i});
                        i++;
                    }
                }
            }

            RefreshText();
        }
        
        #endregion

        #region Override

        protected override void OnButtonPressed(EventArgs eventArgs) {
            if (_listItems != null && _listItems.Count > 0) {

                // correct default selected
                foreach (var item in _listItems) {
                    item.IsSelectedByDefault = item.Index == SelectedIndex;
                }

                _listPopup = new YamuiMenu {
                    SpawnLocation = Cursor.Position,
                    MenuList = _listItems.Cast<YamuiMenuItem>().ToList(),
                    DisplayFilterBox = true
                };
                _listPopup.ClicItemWrapper = item => {
                    SelectedIndex = ((YamuiComboItem) item).Index;
                    if (SelectedIndexChangedByUser != null)
                        SelectedIndexChangedByUser(this);
                    _listPopup.Close();
                    _listPopup.Dispose();
                };
                var owner = FindForm();
                if (owner != null) {
                    _listPopup.Show(new WindowWrapper(owner.Handle));
                } else {
                    _listPopup.Show();
                }
            }

            base.OnButtonPressed(eventArgs);
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {

            var backColor = YamuiThemeManager.Current.ButtonBg(BackColor, UseCustomBackColor, IsFocused, IsHovered, IsPressed, Enabled);
            var borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, IsPressed, Enabled);
            var foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, UseCustomForeColor, IsFocused, IsHovered, IsPressed, Enabled);

            // background
            if (backColor != Color.Transparent)
                e.Graphics.Clear(backColor);
            else
                PaintTransparentBackground(e.Graphics, DisplayRectangle);

            // border?
            if (borderColor != Color.Transparent)
                using (var p = new Pen(borderColor)) {
                    var borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
                    e.Graphics.DrawRectangle(p, borderRect);
                }

            // highlight is a border with more width
            if (Highlight && !IsHovered && !IsPressed && Enabled) {
                using (var p = new Pen(YamuiThemeManager.Current.AccentColor, 4)) {
                    var borderRect = new Rectangle(2, 2, Width - 4, Height - 4);
                    e.Graphics.DrawRectangle(p, borderRect);
                }
            }
            
            // draw the down arrow
            using (SolidBrush b = new SolidBrush(foreColor)) {
                e.Graphics.FillPolygon(b, new[] { new Point(ClientRectangle.Width - 20, ClientRectangle.Height / 2 - 2), new Point(ClientRectangle.Width - 9, ClientRectangle.Height / 2 - 2), new Point(ClientRectangle.Width - 15, ClientRectangle.Height / 2 + 4) });
            }
            
            // text
            if (!IsFocused && string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(WaterMark) && Enabled) {
                TextRenderer.DrawText(e.Graphics, WaterMark, FontManager.GetFont(FontFunction.WaterMark), new Rectangle(0, 0, ClientRectangle.Width - 20, ClientRectangle.Height), YamuiThemeManager.Current.ButtonWatermarkFore, FontManager.GetTextFormatFlags(TextAlign));
            } else {
                TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), new Rectangle(0, 0, ClientRectangle.Width - 20, ClientRectangle.Height), foreColor, FontManager.GetTextFormatFlags(TextAlign));
            }

        }
        
        #endregion

        #region YamuiComboItem

        private class YamuiComboItem : YamuiMenuItem {

            public object BaseValue { get; set; }

            public int Index { get; set; }

        }

        #endregion


    }

}