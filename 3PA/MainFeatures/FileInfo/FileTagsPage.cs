using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls;
using _3PA.Lib;

namespace _3PA.MainFeatures.FileInfo {
    public partial class FileTagsPage : YamuiPage {

        #region fields

        public FileTag LocFileTag;
        public string Filename;

        #endregion

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public FileTagsPage() {
            InitializeComponent();

            UpdateInfo();

            // add event handlers
            yamuiComboBox1.SelectedIndexChanged += SelectedIndexChanged;
            KeyDown += OnKeyDown;
            btok.ButtonPressed += BtokOnButtonPressed;
            btcancel.ButtonPressed += BtcancelOnButtonPressed;
            btclear.ButtonPressed += BtclearOnButtonPressed;
            btdefault.ButtonPressed += BtdefaultOnButtonPressed;
            bttoday.ButtonPressed += BttodayOnButtonPressed;
            bttoday.Click += (sender, args) => { yamuiTextBox6.Text = DateTime.Now.ToString("dd/MM/yy"); };
        }

        #endregion

        #region public

        /// <summary>
        /// Call this method to update the content of the form according to the current document
        /// </summary>
        public void UpdateInfo() {
            Filename = Npp.GetCurrentFileName();

            // populate combobox
            var lst = new List<ItemCombo> {
                new ItemCombo {DisplayText = "Last info", Nb = "last_tag"},
                new ItemCombo {DisplayText = "Default info", Nb = "default_tag"}
            };
            yamuiComboBox1.DisplayMember = "DisplayText";
            yamuiComboBox1.ValueMember = "Nb";

            if (FileTags.Contains(Filename)) {

                var lastItem = FileTags.GetLastFileTag(Filename);
                LocFileTag = lastItem;

                var i = 2;
                var lastItemPos = 0;
                foreach (var fileTag in FileTags.GetFileTagsList(Filename)) {
                    lst.Add(new ItemCombo { DisplayText = Filename + " #" + fileTag.Nb, Nb = fileTag.Nb });
                    if (fileTag.Nb == lastItem.Nb) lastItemPos = i;
                    i++;
                }

                yamuiComboBox1.DataSource = lst;
                yamuiComboBox1.SelectedIndex = lastItemPos;

                var itemsOfFile = FileTags.GetFileTagsList(Filename);
                lst.AddRange(itemsOfFile.Select(item => new ItemCombo { DisplayText = item.Nb, Nb = item.Nb }));
            } else {
                LocFileTag = FileTags.GetFileTags(Config.Instance.UseDefaultValuesInsteadOfLastValuesInEditTags ? "default_tag" : "last_tag", "");

                yamuiComboBox1.DataSource = lst;
                yamuiComboBox1.SelectedIndex = Config.Instance.UseDefaultValuesInsteadOfLastValuesInEditTags ? 1 : 0;
            }
            UpdateView();

            ActiveControl = yamuiComboBox1;
        }

        #endregion

        #region private event

        private void BtokOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            Save(Filename);
            Save("last_tag");
            FileTags.Save();
            FileTags.Cloak();
        }

        private void BtcancelOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            UpdateView();
            FileTags.Cloak();
        }

        private void BtclearOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            LocFileTag = new FileTag();
            UpdateView();
        }

        private void BtdefaultOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            Save("default_tag");
        }

        private void BttodayOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            yamuiTextBox6.Text = DateTime.Now.ToString("dd/MM/yy");
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.KeyCode == Keys.Escape)
                FileTags.Cloak();
        }

        #endregion

        #region private data management

        /// <summary>
        /// Save the info
        /// </summary>
        /// <param name="filename"></param>
        private void Save(string filename) {
            UpdateModel();
            FileTags.SetFileTags(filename, LocFileTag.Nb, LocFileTag.Date, LocFileTag.Text, LocFileTag.NomAppli, LocFileTag.Version, LocFileTag.Chantier, LocFileTag.Jira);
        }

        private void UpdateModel() {
            LocFileTag.NomAppli = yamuiTextBox1.Text;
            LocFileTag.Version = yamuiTextBox2.Text;
            LocFileTag.Chantier = yamuiTextBox4.Text;
            LocFileTag.Jira = yamuiTextBox3.Text;
            LocFileTag.Nb = yamuiTextBox5.Text;
            LocFileTag.Text = yamuiTextBox7.Text;
            LocFileTag.Date = yamuiTextBox6.Text;
        }

        private void UpdateView() {
            yamuiTextBox1.Text = LocFileTag.NomAppli;
            yamuiTextBox2.Text = LocFileTag.Version;
            yamuiTextBox4.Text = LocFileTag.Chantier;
            yamuiTextBox3.Text = LocFileTag.Jira;
            yamuiTextBox5.Text = LocFileTag.Nb;
            yamuiTextBox7.Text = LocFileTag.Text;
            yamuiTextBox6.Text = LocFileTag.Date;
        }

        /// <summary>
        /// called when the user changes the value of the combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedIndexChanged(object sender, EventArgs e) {
            var val = yamuiComboBox1.SelectedValue.ToString();
            if (val == "last_tag" || val == "default_tag")
                LocFileTag = FileTags.GetFileTags(val, "");
            else
                LocFileTag = FileTags.GetFileTags(Filename, val);
            UpdateView();
        }

        #endregion

    }

    #region item combo struct

    public struct ItemCombo {
        public string DisplayText { get; set; }
        public string Nb { get; set; }
    }

    #endregion

}
