namespace _3PA.Forms {

    /*
    public partial class ShowFileTags : MetroForm {

        public FileTag LocFileTag;
        public string Filename;

        public ShowFileTags() {
            InitializeComponent();

            Filename = Npp.GetCurrentFileName();

            //metroTextBox1.DataBindings.Add("Text", LocFileTag, "NomAppli", false, DataSourceUpdateMode.OnPropertyChanged);
            //metroTextBox2.DataBindings.Add("Text", LocFileTag, "Version");
            //metroTextBox3.DataBindings.Add("Text", LocFileTag, "Chantier");
            //metroTextBox4.DataBindings.Add("Text", LocFileTag, "Jira");
            //metroTextBox5.DataBindings.Add("Text", LocFileTag, "Nb");
            //metroTextBox6.DataBindings.Add("Text", LocFileTag, "Text");
            //metroTextBox7.DataBindings.Add("Text", LocFileTag, "Date");

            // populate combobox
            metroComboBox1.Items.Clear();

            var lst = new List<ItemCombo> {
                new ItemCombo {DisplayText = "Last info", Nb = "last_tag"},
                new ItemCombo {DisplayText = "Default info", Nb = "default_tag"}
            };
            metroComboBox1.DisplayMember = "DisplayText";
            metroComboBox1.ValueMember = "Nb";

            if (FileTags.Contains(Filename)) {
                var item = FileTags.GetLastFileTag(Filename);
                lst.Add(new ItemCombo {DisplayText = Filename + " #" + item.Nb, Nb = item.Nb});
                LocFileTag = item;

                metroComboBox1.DataSource = lst;
                metroComboBox1.SelectedIndex = 2;

                //var itemsOfFile = FileTags.GetFileTagsList(Filename);
                //lst.AddRange(itemsOfFile.Select(item => new ItemCombo { DisplayText = item.Nb, Nb = item.Nb }));
            } else {
                LocFileTag = FileTags.GetFileTags(Lib.Config.Instance.UseDefaultValuesInsteadOfLastValuesInEditTags ? "default_tag" : "last_tag", "");

                metroComboBox1.DataSource = lst;
                metroComboBox1.SelectedIndex = Lib.Config.Instance.UseDefaultValuesInsteadOfLastValuesInEditTags ? 1 : 0;
            }
            UpdateView();

            // add event handlers
            metroComboBox1.SelectedIndexChanged += SelectedIndexChanged;
            KeyDown += fileTag_KeyDown;
            FormClosing += fileTag_FormClosing;

            // register to npp
            Interop.FormIntegration.RegisterToNpp(Handle);

            // result of the dialog 
            DialogResult = DialogResult.Cancel;

            ActiveControl = metroLabel1;

            metroTextBox1.Enter += (sender, args) => {
                metroTextBox1.UseStyleColors = true;
                metroTextBox1.Invalidate();
            };
            metroTextBox1.Leave += (sender, args) => {
                metroTextBox1.UseStyleColors = false;
                metroTextBox1.Invalidate();
            };
        }

        public void OnKeyDown(Keys key) {
            if (key == Keys.Escape)
                Close();
            if (key == Keys.Space) { // equivalent to click Ok
                Modifiers modifiers = KeyInterceptor.GetModifiers();
                if (modifiers.IsCtrl && !modifiers.IsAlt && !modifiers.IsShift) {
                    Save(Filename);
                    Close();
                }
            }
        }

        public void fileTag_KeyDown(object sender, KeyEventArgs e) {
            OnKeyDown(e.KeyCode);
        }

        private void fileTag_FormClosing(object sender, FormClosingEventArgs e) {
            Save("last_tag");
            FileTags.Save();
            Interop.FormIntegration.UnRegisterToNpp(Handle);
        }

        /// <summary>
        /// Save the info
        /// </summary>
        /// <param name="filename"></param>
        private void Save(string filename) {
            UpdateModel();
            FileTags.SetFileTags(filename, LocFileTag.Nb, LocFileTag.Date, LocFileTag.Text, LocFileTag.NomAppli, LocFileTag.Version, LocFileTag.Chantier, LocFileTag.Jira);
        }

        private void UpdateModel() {
            LocFileTag.NomAppli = metroTextBox1.Text;
            LocFileTag.Version = metroTextBox2.Text;
            LocFileTag.Chantier = metroTextBox3.Text;
            LocFileTag.Jira = metroTextBox4.Text;
            LocFileTag.Nb = metroTextBox5.Text;
            LocFileTag.Text = metroTextBox6.Text;
            LocFileTag.Date = metroTextBox7.Text;
        }

        private void UpdateView() {
            metroTextBox1.Text = LocFileTag.NomAppli;
            metroTextBox2.Text = LocFileTag.Version;
            metroTextBox3.Text = LocFileTag.Chantier;
            metroTextBox4.Text = LocFileTag.Jira;
            metroTextBox5.Text = LocFileTag.Nb;
            metroTextBox6.Text = LocFileTag.Text;
            metroTextBox7.Text = LocFileTag.Date;
        }

        /// <summary>
        /// called when the user changes the value of the combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedIndexChanged(object sender, EventArgs e) {
            var val = metroComboBox1.SelectedValue.ToString();
            if (val == "last_tag" || val == "default_tag")
                LocFileTag = FileTags.GetFileTags(val, "");
            else
                LocFileTag = FileTags.GetFileTags(Filename, val);
            UpdateView();
        }

        private void ShowFileTags_Load(object sender, EventArgs e) {
            //StyleManager = msmShowFileTags;
        }

        /// <summary>
        /// Click "Ok"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton1_Click(object sender, EventArgs e) {
            Save(Filename);
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Click "Close"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton2_Click(object sender, EventArgs e) {
            Close();
        }

        /// <summary>
        /// Click "reset"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton3_Click(object sender, EventArgs e) {
            LocFileTag = new FileTag();
            UpdateView();
        }

        /// <summary>
        ///  click today
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton4_Click(object sender, EventArgs e) {
            metroTextBox7.Text = DateTime.Now.ToString("dd/MM/yy");
        }

        /// <summary>
        /// click set as default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton5_Click(object sender, EventArgs e) {
            Save("default_tag");
        }
    }

    // cue provider : http://stackoverflow.com/questions/4902565/watermark-textbox-in-winforms
    /*
    public class BorderedTextBox : UserControl {
        TextBox textBox;

        public BorderedTextBox() {
            textBox = new TextBox() {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(-1, -1),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right
            };
            Control container = new ContainerControl() {
                Dock = DockStyle.Fill,
                Padding = new Padding(-1)
            };
            container.Controls.Add(textBox);
            this.Controls.Add(container);

            DefaultBorderColor = SystemColors.ControlDark;
            FocusedBorderColor = Color.Red;
            BackColor = DefaultBorderColor;
            Padding = new Padding(1);
            Size = textBox.Size;
        }

        public Color DefaultBorderColor { get; set; }
        public Color FocusedBorderColor { get; set; }

        public override string Text {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        protected override void OnEnter(EventArgs e) {
            BackColor = FocusedBorderColor;
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            BackColor = DefaultBorderColor;
            base.OnLeave(e);
        }

        protected override void SetBoundsCore(int x, int y,
            int width, int height, BoundsSpecified specified) {
            base.SetBoundsCore(x, y, width, textBox.PreferredHeight, specified);
        }
    }
    */

    public struct ItemCombo {
        public string DisplayText { get; set; }
        public string Nb { get; set; }
    }
}
