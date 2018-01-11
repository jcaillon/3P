#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    /// <summary>
    /// Implements some of the methods/fields of a combo box
    /// </summary>
    [Designer("YamuiFramework.Controls.YamuiComboBoxDesigner")]
    [ToolboxBitmap(typeof(ComboBox))]
    [DefaultEvent("SelectedIndexChangedByUser")]
    public sealed class YamuiComboBox : YamuiButton {
        #region private

        private string _waterMark = "";
        private object _originalDataSource;
        private List<object> _datasource;
        private List<YamuiComboItem> _listItems = new List<YamuiComboItem>();
        private int _selectedIndex;
        private ContentAlignment _textAlign = ContentAlignment.MiddleLeft;
        private YamuiMenu _listPopup;
        private bool _justFocused;

        #endregion

        #region Public events

        /// <summary>
        /// published when the index is changed, wether by the user or programatically
        /// </summary>
        public event EventHandler<EventArgs> SelectedIndexChanged;

        /// <summary>
        /// published when the index is changed by the action of the user
        /// </summary>
        public event Action<YamuiComboBox> SelectedIndexChangedByUser;

        #endregion

        #region Public fields

        [DefaultValue(ContentAlignment.MiddleLeft)]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ContentAlignment TextAlign {
            get { return _textAlign; }
            set { _textAlign = value; }
        }

        /// <summary>
        /// Text to show on the combobox when no value is selected
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("")]
        [Category("Yamui")]
        public string WaterMark {
            get { return _waterMark; }
            set {
                _waterMark = value.Trim();
                RefreshText();
            }
        }

        /// <summary>
        /// Set the datasource for this combo box
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource {
            get { return _originalDataSource; }
            set {
                _originalDataSource = value;
                if (value == null) {
                    _listItems.Clear();
                    return;
                }
                var enumerable = _originalDataSource as IEnumerable;
                if (enumerable == null)
                    throw new Exception("Datasource expects an IEnumerable object");
                _datasource = enumerable.Cast<object>().ToList();
                _listItems.Clear();
                InitList();
            }
        }

        /// <summary>
        /// Field of property of the object from the Datasource List of objects that should be used
        /// as the display text
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public String DisplayMember { get; set; }

        /// <summary>
        /// Field of property of the object from the Datasource List of objects that should be used
        /// as the value (use SelectedValue)
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public String ValueMember { get; set; }

        /// <summary>
        /// get/set the currently selected index
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex {
            get { return _selectedIndex; }
            set {
                if (value >= 0 && value < _listItems.Count) {
                    _selectedIndex = value;
                    if (SelectedIndexChanged != null)
                        SelectedIndexChanged(this, new EventArgs());
                    RefreshText();
                }
            }
        }

        /// <summary>
        /// get the currently selected object
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem {
            get { return SelectedIndex >= 0 && SelectedIndex < _listItems.Count ? _listItems[SelectedIndex].BaseObject : null; }
        }

        /// <summary>
        /// Set/get the currently selected text
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public String SelectedText {
            get { return SelectedIndex >= 0 && SelectedIndex < _listItems.Count ? _listItems[SelectedIndex].DisplayText : null; }
            set {
                if (value == null)
                    return;
                var newIdx = _listItems.FindIndex(item => item.DisplayText.Equals(value));
                if (newIdx >= 0 && newIdx < _listItems.Count)
                    SelectedIndex = newIdx;
            }
        }

        /// <summary>
        /// Set/get the value of currently selected object (designated by ValueMember)
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
                if (value == null)
                    return;
                var newIdx = -1;
                var i = 0;
                foreach (var item in _listItems.Select(item => item.BaseObject)) {
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
                                if (((string) prop.GetValue(item, null)).Equals(value)) {
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

        #region private methods

        /// <summary>
        /// Refresh the text displayed on the combo
        /// </summary>
        private void RefreshText() {
            this.SafeInvoke(box => {
                if (SelectedIndex >= 0 && SelectedIndex < _listItems.Count) {
                    Text = _listItems[SelectedIndex].DisplayText;
                } else {
                    Text = null;
                }
                Invalidate();
            });
        }

        /// <summary>
        /// Initializes the internal list with either the Datasource only or Datasource + Display text
        /// </summary>
        private void InitList() {
            // simple list of strings?
            if (_datasource != null) {
                if (_datasource.Exists(o => o is string)) {
                    var i = 0;
                    _listItems = _datasource.Select(o => new YamuiComboItem {DisplayText = (string) o, BaseObject = o, Index = i++}).ToList();
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
                        _listItems.Add(new YamuiComboItem {DisplayText = displayText, BaseObject = obj, Index = i});
                        i++;
                    }
                }
            }

            RefreshText();
        }

        /// <summary>
        /// displays the popup form
        /// </summary>
        private void DisplayPopup(string initialString) {
            if (_listItems != null && _listItems.Count > 0) {
                var displayFilterBox = !string.IsNullOrEmpty(initialString) || _listItems.Count > 15;

                // correct default selected
                foreach (var item in _listItems) {
                    item.IsSelectedByDefault = item.Index == SelectedIndex;
                }

                var spawnPt = PointToScreen(new Point());
                spawnPt.Offset(0, displayFilterBox ? 0 : Height);
                _listPopup = new YamuiMenu {
                    SpawnLocation = spawnPt,
                    MenuList = _listItems.Cast<YamuiMenuItem>().ToList(),
                    DisplayFilterBox = displayFilterBox,
                    Resizable = false,
                    Movable = false,
                    FormMinSize = new Size(Width, 0),
                    AutocompletionLineHeight = (displayFilterBox ? -1 : 1)*Height,
                    InitialFilterString = initialString
                };

                // on popup clic
                _listPopup.ClicItemWrapper = item => {
                    if (item != null) {
                        SelectedIndex = ((YamuiComboItem) item).Index;
                        if (SelectedIndexChangedByUser != null)
                            SelectedIndexChangedByUser(this);
                    }
                    _listPopup.Close();
                    _listPopup.Dispose();
                };

                // show
                var owner = FindForm();
                if (owner != null) {
                    _listPopup.Show(new WindowWrapper(owner.Handle));
                } else {
                    _listPopup.Show();
                }

                _listPopup.YamuiList.IndexChanged += list => {
                    var item = list.SelectedItem;
                    if (item != null) {
                        SelectedIndex = ((YamuiComboItem) item).Index;
                        if (SelectedIndexChangedByUser != null)
                            SelectedIndexChangedByUser(this);
                    }
                };
            }
        }

        #endregion

        #region Override

        /// <summary>
        /// on key down
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e) {
            // pressing a letter opens the popup in filter mode
            try {
                var c = WinApi.GetCharFromKey(e.KeyValue);
                if (c != null && char.IsLetterOrDigit((char) c)) {
                    DisplayPopup(c.ToString());
                    e.Handled = true;
                }
            } catch (Exception) {
                //ignored
            }

            // pressing down/up changes the current index
            if (!e.Handled) {
                switch (e.KeyCode) {
                    case Keys.Up:
                        SelectedIndex--;
                        if (SelectedIndexChangedByUser != null)
                            SelectedIndexChangedByUser(this);
                        e.Handled = true;
                        break;
                    case Keys.Down:
                        SelectedIndex++;
                        if (SelectedIndexChangedByUser != null)
                            SelectedIndexChangedByUser(this);
                        e.Handled = true;
                        break;
                }
            }

            if (!e.Handled)
                base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            // Enter / space opens the popup
            if (!e.Handled && IsPressed) {
                DisplayPopup(null);
                e.Handled = true;
            }
            base.OnKeyUp(e);
        }

        /// <summary>
        /// On clic
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e) {
            if (!_justFocused) {
                DisplayPopup(null);
            } else
                _justFocused = false;
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Correctly propagate a refresh for theme changes for example
        /// </summary>
        public override void Refresh() {
            if (_listPopup != null)
                _listPopup.Refresh();
            base.Refresh();
        }

        #region Focus first, then popup

        //protected override void OnEnter(EventArgs e) {
        //    _justFocused = new Rectangle(PointToScreen(new Point()), Size).Contains(Cursor.Position);
        //    base.OnEnter(e);
        //}

        #endregion

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            var hasItems = _listItems.Count > 0;
            var backColor = YamuiThemeManager.Current.ButtonBg(BackColor, UseCustomBackColor, IsFocused, IsHovered, IsPressed, Enabled && hasItems);
            var borderColor = YamuiThemeManager.Current.ButtonBorder(IsFocused, IsHovered, IsPressed, Enabled && hasItems);
            var foreColor = YamuiThemeManager.Current.ButtonFg(ForeColor, UseCustomForeColor, IsFocused, IsHovered, IsPressed, Enabled && hasItems);

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
                e.Graphics.FillPolygon(b, new[] {new Point(ClientRectangle.Width - 20, ClientRectangle.Height/2 - 2), new Point(ClientRectangle.Width - 9, ClientRectangle.Height/2 - 2), new Point(ClientRectangle.Width - 15, ClientRectangle.Height/2 + 4)});
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
            /// <summary>
            /// Stores the base object for this item
            /// </summary>
            public object BaseObject { get; set; }

            /// <summary>
            /// Index of this item
            /// </summary>
            public int Index { get; set; }
        }

        #endregion
    }

    #region Designer

    internal class YamuiComboBoxDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");
            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");
            properties.Remove("UseVisualStyleBackColor");
            properties.Remove("Font");
            properties.Remove("RightToLeft");
            base.PreFilterProperties(properties);
        }
    }

    #endregion
}