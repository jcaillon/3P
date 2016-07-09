#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiInputDialog.cs) is part of YamuiFramework.
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
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Forms {

    #region YamuiInternalInputDialog

    /// <summary>
    /// Get an input dialog based on an object fields
    /// </summary>
    public class YamuiInternalInputDialog : YamuiForm {

        #region private fields

        private object _dataObj;
        private ErrorProvider _errorProvider;
        private List<MemberInfo> _items = new List<MemberInfo>();
        private YamuiTableLayoutPanel _table;

        #endregion

        #region life and death

        internal YamuiInternalInputDialog(string prompt) {
            IContainer components = new Container();
            var buttonPanel = new YamuiTableLayoutPanel();
            var okBtn = new YamuiButton();
            var cancelBtn = new YamuiButton();
            _table = new YamuiTableLayoutPanel();
            _errorProvider = new ErrorProvider(components);
            var lbl = new HtmlLabel {
                BackColor = Color.Transparent,
                AutoSize = true,
                Text = prompt,
                Dock = DockStyle.Top,
                Margin = new Padding(0),
                Padding = new Padding(0),
                IsSelectionEnabled = false,
                Enabled = false
            };
            buttonPanel.SuspendLayout();
            ((ISupportInitialize) (_errorProvider)).BeginInit();
            SuspendLayout();
            //
            // buttonPanel
            //
            buttonPanel.AutoSize = true;
            buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonPanel.BackColor = SystemColors.Control;
            buttonPanel.ColumnCount = 3;
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle());
            buttonPanel.ColumnStyles.Add(new ColumnStyle());
            buttonPanel.Controls.Add(okBtn, 1, 0);
            buttonPanel.Controls.Add(cancelBtn, 2, 0);
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Location = new Point(0, 25);
            buttonPanel.Margin = new Padding(0);
            buttonPanel.Name = "_buttonPanel";
            buttonPanel.Padding = new Padding(10, 0, 10, 0);
            buttonPanel.RowCount = 1;
            buttonPanel.RowStyles.Add(new RowStyle());
            buttonPanel.Size = new Size(177, 22);
            buttonPanel.TabIndex = 1;
            buttonPanel.DontUseTransparentBackGround = true;
            //
            // okBtn
            //
            okBtn.DialogResult = DialogResult.OK;
            okBtn.Location = new Point(10, 8);
            okBtn.Margin = new Padding(0, 0, 7, 0);
            okBtn.MinimumSize = new Size(75, 23);
            okBtn.Name = "_okBtn";
            okBtn.Size = new Size(75, 23);
            okBtn.TabIndex = 0;
            okBtn.Text = @"OK";
            okBtn.ButtonPressed += okBtn_Click;
            //
            // cancelBtn
            //
            cancelBtn.DialogResult = DialogResult.Cancel;
            cancelBtn.Location = new Point(92, 8);
            cancelBtn.Margin = new Padding(0);
            cancelBtn.MinimumSize = new Size(75, 23);
            cancelBtn.Name = "_cancelBtn";
            cancelBtn.Size = new Size(75, 23);
            cancelBtn.TabIndex = 1;
            cancelBtn.Text = @"&Cancel";
            cancelBtn.ButtonPressed += cancelBtn_Click;
            //
            // table
            //
            _table.AutoSize = true;
            _table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _table.ColumnCount = 3;
            _table.ColumnStyles.Add(new ColumnStyle());
            _table.ColumnStyles.Add(new ColumnStyle());
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _table.Dock = DockStyle.Bottom;
            _table.Location = new Point(0, 0);
            _table.Margin = new Padding(0);
            _table.Name = "_table";
            _table.Padding = new Padding(10);
            _table.RowCount = 1;
            _table.RowStyles.Add(new RowStyle());
            _table.TabIndex = 3;
            _table.DontUseTransparentBackGround = true;
            //
            // errorProvider
            //
            _errorProvider.ContainerControl = this;
            //
            // InternalInputDialog
            //
            AcceptButton = okBtn;
            CancelButton = cancelBtn;
            //ClientSize = new Size(50, 50);
            Controls.Add(lbl);
            Controls.Add(_table);
            Controls.Add(buttonPanel);
            Name = "YamuiInternalInputDialog";
            StartPosition = FormStartPosition.CenterParent;
            ControlBox = false;
            Padding = new Padding(Padding.Left, 8, Padding.Right, Padding.Bottom);

            buttonPanel.ResumeLayout(false);
            ((ISupportInitialize) (_errorProvider)).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        #region public

        /// <summary>
        /// Gets or sets the data
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [DefaultValue(null), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Data {
            get { return _dataObj; }
            set {
                if (value == null)
                    throw new ArgumentNullException();
                _items.Clear();
                if (value.GetType().IsSimpleType())
                    _items.Add(null);
                else {
                    foreach (var mi in value.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public)) {
                        if (GetAttr(mi) != null && GetAttr(mi).Hidden)
                            continue;
                        var fi = mi as FieldInfo;
                        var pi = mi as PropertyInfo;
                        if (fi != null && Utilities.IsSupportedType(fi.FieldType)) {
                            _items.Add(fi);
                        } else if (pi != null && Utilities.IsSupportedType(pi.PropertyType) && pi.GetIndexParameters().Length == 0 && pi.CanWrite) {
                            _items.Add(pi);
                        }
                    }

                    _items.Sort((x, y) => (GetAttr(x) != null ? GetAttr(x).Order : int.MaxValue) - (GetAttr(y) != null ? GetAttr(y).Order : int.MaxValue));
                }
                _dataObj = value;
                BuildTable();
            }
        }

        #endregion

        #region private

        private void BuildTable() {
            _table.SuspendLayout();

            // Clear out last layout
            _table.Controls.Clear();
            while (_table.RowStyles.Count > 1)
                _table.RowStyles.RemoveAt(1);

            _table.RowCount = _items.Count;

            int hrow = 0;

            // Build rows for each item
            for (int i = 0; i < _items.Count; i++) {
                if (i + hrow > 0)
                    _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                _table.Controls.Add(BuildLabelForItem(i), 1, i + hrow);
                _table.Controls.Add(BuildInputForItem(i), 2, i + hrow);
            }

            _table.ResumeLayout();
        }

        private Label BuildLabelForItem(int i) {
            var item = _items[i];
            var lbl = new YamuiLabel {AutoSize = true, Dock = DockStyle.Left, Margin = new Padding(0, 0, 1, 0)};
            if (item != null) {
                lbl.Text = (GetAttr(item) != null ? GetAttr(item).Label : item.Name) + @":";
                lbl.Margin = new Padding(0, 10, 4, 0);
            }
            return lbl;
        }

        private Control BuildInputForItem(int i) {
            var item = _items[i];
            var itemType = GetItemType(item);

            // Get default text value
            object val;
            if (item == null)
                val = _dataObj;
            else if (item is PropertyInfo)
                val = ((PropertyInfo) item).GetValue(_dataObj, null);
            else
                val = ((FieldInfo) item).GetValue(_dataObj);
            string t = val.ConvertToStr();

            // Build control type
            Control retVal;
            if (itemType == typeof (bool)) {
                retVal = new YamuiButtonToggle {AutoSize = false, Checked = (bool) val, Margin = new Padding(0, 10, 0, 0), Size = new Size(40, 20)};
            } else if (itemType.IsEnum) {
                var cb = new YamuiComboBox {Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList};
                cb.Items.AddRange(Enum.GetNames(itemType));
                cb.Text = t;
                retVal = cb;
            } else {
                var tb = new YamuiTextBox {CausesValidation = true, Dock = DockStyle.Fill, Text = t};
                tb.Enter += (s, e) => tb.SelectAll();
                if (itemType == typeof (char))
                    tb.KeyPress += (s, e) => e.Handled = !char.IsControl(e.KeyChar) && tb.TextLength > 0;
                else
                    tb.KeyPress += (s, e) => e.Handled = Utilities.IsInvalidKey(e.KeyChar, itemType);
                tb.Validating += (s, e) => {
                    bool invalid = IsTextInvalid(tb, itemType);
                    e.Cancel = invalid;
                    _errorProvider.SetError(tb, invalid ? "Text must be in a valid format for " + itemType.Name + "." : "");
                };
                tb.Validated += (s, e) => _errorProvider.SetError(tb, "");
                _errorProvider.SetIconPadding(tb, -18);
                _errorProvider.Icon = Resources.Resources.IcoError;
                retVal = tb;
            }

            // Set standard props
            retVal.Margin = new Padding(0, 7, 0, 0);
            retVal.Name = "input" + i;

            return retVal;
        }

        /// <summary>
        /// Binds input text values back to the Data object.
        /// </summary>
        private void BindToData() {
            for (int i = 0; i < _items.Count; i++) {
                var item = _items[i];
                var itemType = GetItemType(item);

                // Get value from control
                Control c = _table.Controls["input" + i];
                object val;
                if (c is YamuiButtonToggle)
                    val = ((YamuiButtonToggle) c).Checked;
                else
                    val = c.Text.ConvertFromStr(itemType);

                // Apply value to dataObj
                if (item == null)
                    _dataObj = val;
                else if (item is PropertyInfo)
                    ((PropertyInfo) item).SetValue(_dataObj, val, null);
                else
                    ((FieldInfo) item).SetValue(_dataObj, val);
            }
        }

        private bool IsTextInvalid(YamuiTextBox tb, Type itemType) {
            if (string.IsNullOrEmpty(tb.Text))
                return false;
            Predicate<string> p;
            Utilities.Validations.TryGetValue(itemType, out p);
            if (p != null)
                return !p(tb.Text);
            return false;
        }

        private void okBtn_Click(object sender, EventArgs e) {
            File.AppendAllText(@"C:\Work\3P_notepad++\plugins\Config\3P\Nouveau document texte.txt", "yo\n");
            if (ValidateChildren()) {
                BindToData();
                Close();
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e) {
            Close();
        }

        private YamuiInputDialogItemAttribute GetAttr(MemberInfo mi) {
            return (YamuiInputDialogItemAttribute) Attribute.GetCustomAttribute(mi, typeof (YamuiInputDialogItemAttribute), true);
        }

        private Type GetItemType(MemberInfo mi) {
            return mi == null ? _dataObj.GetType() : (mi is PropertyInfo ? ((PropertyInfo) mi).PropertyType : ((FieldInfo) mi).FieldType);
        }

        #endregion

        #region static

        /// <summary>
        /// Displays an input dialog that will automatically update the values of the given object according
        /// to the user's answers
        /// </summary>
        public static DialogResult Show(IWin32Window owner, string prompt, string caption, ref object data, int width = 500) {
            using (var dialog = new YamuiInternalInputDialog(prompt)) {
                dialog.Width = width;
                dialog.Text = string.IsNullOrEmpty(caption) ? "Question" : caption;
                dialog.AutoSize = true;
                dialog.MinimumSize = new Size(width, 50);
                dialog.ClientSize = new Size(width, 50);
                dialog.Data = data;
                var ret = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
                if (ret == DialogResult.OK)
                    data = dialog.Data;
                return ret;
            }
        }

        #endregion

    }

    #endregion


    #region YamuiInputDialogItemAttribute

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YamuiInputDialogItemAttribute : Attribute {

        public YamuiInputDialogItemAttribute() { }

        public YamuiInputDialogItemAttribute(string label) {
            Label = label;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item is hidden and not displayed
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the label to use as the label for this field or property
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the order in which to display the input for this field
        /// </summary>
        public int Order { get; set; }
    }

    #endregion

}