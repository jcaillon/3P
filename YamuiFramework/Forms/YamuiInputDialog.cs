#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (InputDialog.cs) is part of YamuiFramework.
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
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Forms {
    /// <summary>
    /// Get input based on automatic interpretation of Data object.
    /// </summary>
    internal class YamuiInternalInputDialog : YamuiForm {

        #region private fields

        private const int PrefWidth = 340;

        private static readonly Size MinSize = new Size(193, 104);

        private static readonly Dictionary<Type, char[]> KeyPressValidChars = new Dictionary<Type, char[]> {
            {typeof (byte), GetCultureChars(true, false, true)},
            {typeof (sbyte), GetCultureChars(true, true, true)},
            {typeof (short), GetCultureChars(true, true, true)},
            {typeof (ushort), GetCultureChars(true, false, true)},
            {typeof (int), GetCultureChars(true, true, true)},
            {typeof (uint), GetCultureChars(true, false, true)},
            {typeof (long), GetCultureChars(true, true, true)},
            {typeof (ulong), GetCultureChars(true, false, true)},
            {typeof (double), GetCultureChars(true, true, true, true, true, true)},
            {typeof (float), GetCultureChars(true, true, true, true, true, true)},
            {typeof (decimal), GetCultureChars(true, true, true, true, true)},
            {typeof (TimeSpan), GetCultureChars(true, true, false, new[] {'-'})},
            {typeof (Guid), GetCultureChars(true, false, false, "-{}()".ToCharArray())}
        };

        private static readonly Type[] SimpleTypes = {
            typeof (Enum), typeof (Decimal), typeof (DateTime),
            typeof (DateTimeOffset), typeof (String), typeof (TimeSpan), typeof (Guid)
        };

        private static readonly Dictionary<Type, Predicate<string>> Validations = new Dictionary<Type, Predicate<string>> {
            {typeof (byte), s => {
                byte n;
                return byte.TryParse(s, out n);
            }
            }, {typeof (sbyte), s => {
                sbyte n;
                return sbyte.TryParse(s, out n);
            }
            }, {typeof (short), s => {
                short n;
                return short.TryParse(s, out n);
            }
            }, {typeof (ushort), s => {
                ushort n;
                return ushort.TryParse(s, out n);
            }
            }, {typeof (int), s => {
                int n;
                return int.TryParse(s, out n);
            }
            }, {typeof (uint), s => {
                uint n;
                return uint.TryParse(s, out n);
            }
            }, {typeof (long), s => {
                long n;
                return long.TryParse(s, out n);
            }
            }, {typeof (ulong), s => {
                ulong n;
                return ulong.TryParse(s, out n);
            }
            }, {typeof (char), s => {
                char n;
                return char.TryParse(s, out n);
            }
            }, {typeof (double), s => {
                double n;
                return double.TryParse(s, out n);
            }
            }, {typeof (float), s => {
                float n;
                return float.TryParse(s, out n);
            }
            }, {typeof (decimal), s => {
                decimal n;
                return decimal.TryParse(s, out n);
            }
            }, {typeof (DateTime), s => {
                DateTime n;
                return DateTime.TryParse(s, out n);
            }
            }, {typeof (TimeSpan), s => {
                TimeSpan n;
                return TimeSpan.TryParse(s, out n);
            }
            }, {typeof (Guid), s => {
                try {
                    Guid n = new Guid(s);
                    return true;
                } catch {
                    return false;
                }
            }
            }
        };

        private YamuiTableLayoutPanel _buttonPanel;
        private YamuiButton _cancelBtn;
        private IContainer components;
        private object _dataObj;
        private ErrorProvider _errorProvider;
        private Image _image;
        private List<MemberInfo> _items = new List<MemberInfo>();
        private YamuiButton _okBtn;
        private YamuiTableLayoutPanel _table;
        private HtmlLabel _lbl;
        private string _header;
        private string _caption;

        #endregion

        #region life and death

        /// <summary>
        /// Initializes a new instance of the <see cref="YamuiInternalInputDialog"/> class.
        /// </summary>
        public YamuiInternalInputDialog(string header) {
            _header = header;
            InitializeComponent();
        }

        internal YamuiInternalInputDialog(string header, string caption, object data, int width)
            : this(header) {
            Width = width;
            _caption = string.IsNullOrEmpty(caption) ? "Question" : caption;
            Data = data;
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

                if (IsSimpleType(value.GetType()))
                    _items.Add(null);
                else {
                    foreach (var mi in value.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public)) {
                        if (GetAttr(mi) != null && GetAttr(mi).Hidden)
                            continue;
                        var fi = mi as FieldInfo;
                        var pi = mi as PropertyInfo;
                        if (fi != null && IsSupportedType(fi.FieldType)) {
                            _items.Add(fi);
                        } else if (pi != null && IsSupportedType(pi.PropertyType) && pi.GetIndexParameters().Length == 0 && pi.CanWrite) {
                            _items.Add(pi);
                        }
                    }

                    _items.Sort((x, y) => (GetAttr(x) != null ? GetAttr(x).Order : int.MaxValue) - (GetAttr(y) != null ? GetAttr(y).Order : int.MaxValue));
                }

                _dataObj = value;

                BuildTable();
            }
        }

        public new int Width {
            get { return base.Width; }
            set {
                if (value == 0) value = PrefWidth;
                value = Math.Max(MinSize.Width, value);
                MinimumSize = new Size(value, MinSize.Height);
                MaximumSize = new Size(value, int.MaxValue);
            }
        }

        #endregion

        #region private

        private static object ConvertFromStr(string value, Type destType) {
            if (destType == typeof (string))
                return value;
            if (value.Trim() == string.Empty)
                return destType.IsValueType ? Activator.CreateInstance(destType) : null;
            if (typeof (IConvertible).IsAssignableFrom(destType))
                try {
                    return Convert.ChangeType(value, destType);
                } catch {
                    // ignored
                }
            return TypeDescriptor.GetConverter(destType).ConvertFrom(value);
        }

        private static string ConvertToStr(object value) {
            if (value == null)
                return string.Empty;
            IConvertible conv = value as IConvertible;
            if (conv != null)
                return value.ToString();
            return (string) TypeDescriptor.GetConverter(value).ConvertTo(value, typeof (string));
        }

        private static int GetBestHeight(Control c) {
            using (Graphics g = c.CreateGraphics())
                return TextRenderer.MeasureText(g, c.Text, c.Font, new Size(c.Width, 0), TextFormatFlags.WordBreak).Height;
        }

        private static char[] GetCultureChars(bool digits, bool neg, bool pos, bool dec = false, bool grp = false, bool e = false) {
            var c = CultureInfo.CurrentCulture.NumberFormat;
            var l = new List<string>();
            if (digits) l.AddRange(c.NativeDigits);
            if (neg) l.Add(c.NegativeSign);
            if (pos) l.Add(c.PositiveSign);
            if (dec) l.Add(c.NumberDecimalSeparator);
            if (grp) l.Add(c.NumberGroupSeparator);
            if (e) l.Add("Ee");
            var sb = new StringBuilder();
            foreach (var s in l)
                sb.Append(s);
            char[] ca = sb.ToString().ToCharArray();
            Array.Sort(ca);
            return ca;
        }

        private static char[] GetCultureChars(bool timeChars, bool timeSep, bool dateSep, char[] other) {
            var c = CultureInfo.CurrentCulture;
            var l = new List<string>();
            if (timeChars) l.AddRange(c.NumberFormat.NativeDigits);
            if (timeSep) {
                l.Add(c.DateTimeFormat.TimeSeparator);
                l.Add(c.NumberFormat.NumberDecimalSeparator);
            }
            if (dateSep) l.Add(c.DateTimeFormat.DateSeparator);
            if (other != null && other.Length > 0) l.Add(new string(other));
            var sb = new StringBuilder();
            foreach (var s in l)
                sb.Append(s);
            char[] ca = sb.ToString().ToCharArray();
            Array.Sort(ca);
            return ca;
        }

        private static bool IsSimpleType(Type type) {
            return type.IsPrimitive || type.IsEnum || Array.Exists(SimpleTypes, t => t == type) || Convert.GetTypeCode(type) != TypeCode.Object ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
        }

        private static bool IsSupportedType(Type type) {
            if (typeof (IConvertible).IsAssignableFrom(type))
                return true;
            var cvtr = TypeDescriptor.GetConverter(type);
            if (cvtr.CanConvertFrom(typeof (string)) && cvtr.CanConvertTo(typeof (string)))
                return true;
            return false;
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
                    val = ConvertFromStr(c.Text, itemType);

                // Apply value to dataObj
                if (item == null)
                    _dataObj = val;
                else if (item is PropertyInfo)
                    ((PropertyInfo) item).SetValue(_dataObj, val, null);
                else
                    ((FieldInfo) item).SetValue(_dataObj, val);
            }
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
            string t = ConvertToStr(val);

            // Build control type
            Control retVal;
            if (itemType == typeof (bool)) {
                retVal = new YamuiButtonToggle { AutoSize = false, Checked = (bool)val, Margin = new Padding(0, 10, 0, 0), Size = new Size(40, 20)};
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
                    tb.KeyPress += (s, e) => e.Handled = IsInvalidKey(e.KeyChar, itemType);
                tb.Validating += (s, e) => {
                    bool invalid = TextIsInvalid(tb, itemType);
                    e.Cancel = invalid;
                    _errorProvider.SetError(tb, invalid ? "Text must be in a valid format for " + itemType.Name + "." : "");
                };
                tb.Validated += (s, e) => _errorProvider.SetError(tb, "");
                _errorProvider.SetIconPadding(tb, -18);
                retVal = tb;
            }

            // Set standard props
            retVal.Margin = new Padding(0, 7, 0, 0);
            retVal.Name = "input" + i;
            return retVal;
        }

        private Label BuildLabelForItem(int i) {
            var item = _items[i];
            var lbl = new YamuiLabel {AutoSize = true, Dock = DockStyle.Left, Margin = new Padding(0, 0, 1, 0)};
            if (item != null) {
                lbl.Text = (GetAttr(item) != null ? GetAttr(item).Label : item.Name) + ":";
                lbl.Margin = new Padding(0, 10, 4, 0);
            }
            return lbl;
        }

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

        private void cancelBtn_Click(object sender, EventArgs e) {
            Close();
        }

        private YamuiInputDialogItemAttribute GetAttr(MemberInfo mi) {
            return (YamuiInputDialogItemAttribute) Attribute.GetCustomAttribute(mi, typeof (YamuiInputDialogItemAttribute), true);
        }

        private Type GetItemType(MemberInfo mi) {
            return mi == null ? _dataObj.GetType() : (mi is PropertyInfo ? ((PropertyInfo) mi).PropertyType : ((FieldInfo) mi).FieldType);
        }

        private void InitializeComponent() {
            components = new Container();
            _buttonPanel = new YamuiTableLayoutPanel();
            _okBtn = new YamuiButton();
            _cancelBtn = new YamuiButton();
            _table = new YamuiTableLayoutPanel();
            _errorProvider = new ErrorProvider(components);
            _lbl = new HtmlLabel {
                BackColor = Color.Transparent,
                AutoSize = true,
                Text = _header,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 0),
                Padding = new Padding(0, 0, 28, 10),
                IsSelectionEnabled = false,
                Enabled = false
            };
            _buttonPanel.SuspendLayout();
            ((ISupportInitialize) (_errorProvider)).BeginInit();
            SuspendLayout();
            //
            // buttonPanel
            //
            _buttonPanel.AutoSize = true;
            _buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _buttonPanel.BackColor = SystemColors.Control;
            _buttonPanel.ColumnCount = 3;
            _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _buttonPanel.ColumnStyles.Add(new ColumnStyle());
            _buttonPanel.ColumnStyles.Add(new ColumnStyle());
            _buttonPanel.Controls.Add(_okBtn, 1, 0);
            _buttonPanel.Controls.Add(_cancelBtn, 2, 0);
            _buttonPanel.Dock = DockStyle.Bottom;
            _buttonPanel.Location = new Point(0, 25);
            _buttonPanel.Margin = new Padding(0);
            _buttonPanel.Name = "_buttonPanel";
            _buttonPanel.Padding = new Padding(10, 0, 10, 0);
            _buttonPanel.RowCount = 1;
            _buttonPanel.RowStyles.Add(new RowStyle());
            _buttonPanel.Size = new Size(177, 22);
            _buttonPanel.TabIndex = 1;
            _buttonPanel.DontUseTransparentBackGround = true;
            //
            // okBtn
            //
            _okBtn.Location = new Point(10, 8);
            _okBtn.Margin = new Padding(0, 0, 7, 0);
            _okBtn.MinimumSize = new Size(75, 23);
            _okBtn.Name = "_okBtn";
            _okBtn.Size = new Size(75, 23);
            _okBtn.TabIndex = 0;
            _okBtn.Text = @"OK";
            _okBtn.UseVisualStyleBackColor = true;
            _okBtn.ButtonPressed += okBtn_Click;
            //
            // cancelBtn
            //
            _cancelBtn.DialogResult = DialogResult.Cancel;
            _cancelBtn.Location = new Point(92, 8);
            _cancelBtn.Margin = new Padding(0);
            _cancelBtn.MinimumSize = new Size(75, 23);
            _cancelBtn.Name = "_cancelBtn";
            _cancelBtn.Size = new Size(75, 23);
            _cancelBtn.TabIndex = 1;
            _cancelBtn.Text = @"&Cancel";
            _cancelBtn.UseVisualStyleBackColor = true;
            _cancelBtn.ButtonPressed += cancelBtn_Click;
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
            _table.Size = new Size(177, 24);
            _table.TabIndex = 3;
            _table.DontUseTransparentBackGround = true;
            //
            // errorProvider
            //
            _errorProvider.ContainerControl = this;
            //
            // InternalInputDialog
            //
            AcceptButton = _okBtn;
            AutoSize = true;
            //AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CancelButton = _cancelBtn;
            ClientSize = new Size(PrefWidth, 65);
            Controls.Add(_lbl);
            Controls.Add(_table);
            Controls.Add(_buttonPanel);
            MinimumSize = new Size(PrefWidth, MinSize.Height);
            //MaximumSize = new Size(PrefWidth, int.MaxValue);
            Name = "YamuiInternalInputDialog";
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Padding = new Padding(Padding.Left, 8, Padding.Right, Padding.Bottom);
            Text = _caption;

            _buttonPanel.ResumeLayout(false);
            ((ISupportInitialize) (_errorProvider)).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private bool IsInvalidKey(char keyChar, Type itemType) {
            if (char.IsControl(keyChar))
                return false;
            char[] chars;
            KeyPressValidChars.TryGetValue(itemType, out chars);
            if (chars != null) {
                int si = Array.BinarySearch(chars, keyChar);
                if (si < 0)
                    return true;
            }
            return false;
        }

        private void okBtn_Click(object sender, EventArgs e) {
            BindToData();
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool TextIsInvalid(YamuiTextBox tb, Type itemType) {
            if (string.IsNullOrEmpty(tb.Text))
                return false;
            Predicate<string> p;
            Validations.TryGetValue(itemType, out p);
            if (p != null)
                return !p(tb.Text);
            return false;
        }

        #endregion

    }

    #region YamuiInputDialog

    /// <summary>
    /// An input dialog that automatically creates controls to collect the values of the object supplied via the <see cref="Data"/> property.
    /// Credits goes to : http://www.codeproject.com/Articles/1057961/Content-driven-Input-Dialog
    /// </summary>
    public class YamuiInputDialog : CommonDialog {
        private object _data;

        /// <summary>
        /// Gets or sets the data for the input dialog box. The data type will determine the type of input mechanism displayed. For simple
        /// types, a <see cref="YamuiTextBox"/> with validation, or a <see cref="YamuiButtonToggle"/> or a <see cref="YamuiComboBox"/> will
        /// be displayed. For classes and structures, all of the public, top-level, fields and properties will have
        /// input mechanisms shown for each. See Remarks for more detail.
        /// </summary>
        /// <value>
        /// The data for the input dialog box.
        /// </value>
        /// <remarks>TBD</remarks>
        [DefaultValue(null), Category("Data"), Description("The data for the input dialog box.")]
        public object Data {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Gets or sets the image to display on the top left corner of the dialog. This value can be <c>null</c> to display no image.
        /// </summary>
        /// <value>
        /// The image to display on the top left corner of the dialog.
        /// </value>
        [DefaultValue(null), Category("Appearance"), Description("The image to display on the top left corner of the dialog.")]
        public Image Image { get; set; }

        /// <summary>
        /// Gets or sets the text prompt to display above all input options. This value can be <c>null</c>.
        /// </summary>
        /// <value>
        /// The text prompt to display above all input options.
        /// </value>
        [DefaultValue(null), Category("Appearance"), Description("The text prompt to display above all input options.")]
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the input dialog box title.
        /// </summary>
        /// <value>
        /// The input dialog box title.
        /// </value>
        [DefaultValue(""), Category("Window"), Description("The input dialog box title.")]
        public string Title { get; set; }

        /// <summary>
        /// Displays an input dialog in front of the specified object and with the specified prompt, caption, data, and image.
        /// </summary>
        /// <param name="owner">An implementation of <see cref="IWin32Window"/> that will own the modal dialog box.</param>
        /// <param name="prompt">The text prompt to display above all input options. This value can be <c>null</c>.</param>
        /// <param name="caption">The caption for the dialog.</param>
        /// <param name="data">
        /// The data for the input. The data type will determine the type of input mechanism displayed. For simple
        /// types, a <see cref="YamuiTextBox"/> with validation, or a <see cref="YamuiButtonToggle"/> or a <see cref="YamuiComboBox"/> will
        /// be displayed. For classes and structures, all of the public, top-level, fields and properties will have
        /// input mechanisms shown for each. See Remarks for more detail.
        /// </param>
        /// <param name="width">
        /// The desired width of the <see cref="YamuiInternalInputDialog"/>. A value of <c>0</c> indicates a default width.
        /// </param>
        /// <returns>
        /// Either <see cref="DialogResult.OK"/> or <see cref="DialogResult.Cancel"/>. On OK, the
        /// <paramref name="data"/> parameter will include the updated values from the <see cref="YamuiInternalInputDialog"/>.
        /// </returns>
        /// <remarks></remarks>
        public static DialogResult Show(IWin32Window owner, string prompt, string caption, ref object data, int width = 0) {
            using (var dlg = new YamuiInternalInputDialog(prompt, caption, data, width)) {
                var ret = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
                if (ret == DialogResult.OK)
                    data = dlg.Data;
                return ret;
            }
        }

        /// <summary>
        /// Displays an input dialog with the specified prompt, caption, data, and image.
        /// </summary>
        /// <param name="prompt">The text prompt to display above all input options. This value can be <c>null</c>.</param>
        /// <param name="caption">The caption for the dialog.</param>
        /// <param name="data">
        /// The data for the input. The data type will determine the type of input mechanism displayed. For simple
        /// types, a <see cref="YamuiTextBox"/> with validation, or a <see cref="YamuiButtonToggle"/> or a <see cref="YamuiComboBox"/> will
        /// be displayed. For classes and structures, all of the public, top-level, fields and properties will have
        /// input mechanisms shown for each. See Remarks for more detail.
        /// </param>
        /// <param name="width">
        /// The desired width of the <see cref="YamuiInternalInputDialog"/>. A value of <c>0</c> indicates a default width.
        /// </param>
        /// <returns>
        /// Either <see cref="DialogResult.OK"/> or <see cref="DialogResult.Cancel"/>. On OK, the
        /// <paramref name="data"/> parameter will include the updated values from the <see cref="YamuiInternalInputDialog"/>.
        /// </returns>
        /// <remarks></remarks>
        public static DialogResult Show(string prompt, string caption, ref object data, int width = 0) {
            return Show(null, prompt, caption, ref data, width);
        }

        /// <summary>
        /// Resets all properties to their default values.
        /// </summary>
        public override void Reset() {}

        /// <summary>
        /// <para>This API supports the.NET Framework infrastructure and is not intended to be used directly from your code.</para>
        /// <para>Specifies a common dialog box.</para>
        /// </summary>
        /// <param name="hwndOwner">A value that represents the window handle of the owner window for the common dialog box.</param>
        /// <returns><c>true</c> if the data was collected; otherwise, <c>false</c>.</returns>
        protected override bool RunDialog(IntPtr hwndOwner) {
            return Show(NativeWindow.FromHandle(hwndOwner), Prompt, Title, ref _data) == DialogResult.OK;
        }
    }

    #endregion

    #region YamuiInputDialogItemAttribute

    /// <summary>
    /// Allows a developer to attribute a property or field with text that gets shown instead of the field or property name in an <see cref="YamuiInputDialog"/>.
    /// Credits goes to : http://www.codeproject.com/Articles/1057961/Content-driven-Input-Dialog
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class YamuiInputDialogItemAttribute : Attribute {

        /// <summary>
        /// Initializes a new instance of the <see cref="YamuiInputDialogItemAttribute" /> class.
        /// </summary>
        public YamuiInputDialogItemAttribute() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="YamuiInputDialogItemAttribute" /> class.
        /// </summary>
        /// <param name="label">The label to use in the <see cref="YamuiInputDialog"/> as the label for this field or property.</param>
        public YamuiInputDialogItemAttribute(string label) {
            Label = label;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item is hidden and not displayed by the <see cref="YamuiInputDialog"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if hidden; otherwise, <c>false</c>.
        /// </value>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the label to use in the <see cref="YamuiInputDialog"/> as the label for this field or property.
        /// </summary>
        /// <value>
        /// The label for this item.
        /// </value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the order in which to display the input for this field or property within the <see cref="YamuiInputDialog"/>.
        /// </summary>
        /// <value>
        /// The display order for this item.
        /// </value>
        public int Order { get; set; }
    }

    #endregion

}