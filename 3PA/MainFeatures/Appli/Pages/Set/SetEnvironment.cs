#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (SetEnvironment.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    public partial class SetEnvironment : YamuiPage {

        #region fields

        private const string ModifyStr = "Modify";
        private const string SaveStr = "Save";
        private const string AddNewStr = "Add new";
        private const string CancelStr = "Cancel";

        private bool _isModifying;

        #endregion

        #region constructor

        public SetEnvironment() {
            InitializeComponent();

            // sets buttons behavior
            foreach (var control in mainPanel.Controls) {
                if (control is YamuiImageButton) {
                    var x = (YamuiImageButton) control;
                    if (x.Name.StartsWith("btleft")) {
                        // Left button
                        x.BackGrndImage = ImageResources.SelectFile;
                        x.ButtonPressed += BtleftOnButtonPressed;
                        toolTip.SetToolTip(x, "<b>Click</b> to select a new file");
                    } else {
                        // right button
                        x.BackGrndImage = ImageResources.OpenInExplorer;
                        x.ButtonPressed += BtrightOnButtonPressed;
                        string tag = (string)(x.Tag ?? string.Empty);
                        toolTip.SetToolTip(x, "<b>Click</b> to " + (tag.Equals("true") ? "open this folder in the explorer" : "to open the containing folder in the explorer"));
                    }
                }
            }

            // switch all to disabled
            SwitchEnabledDisabled(false);

            // control buttons
            _isModifying = true;
            Btcontrol2ButtonPressed(false);

            btcontrol2.ButtonPressed += (sender, args) => Btcontrol2ButtonPressed();
            btcontrol1.ButtonPressed += Btcontrol1ButtonPressed;

            // tooltips
            toolTip.SetToolTip(cbAppli, "<b>Select</b> the application to use");
            toolTip.SetToolTip(cbEnvLetter, "<b>Select</b> the environment letter to use");
            toolTip.SetToolTip(cbDatabase, "<b>Select</b> the database to use");

            UpdateView();
        }

        #endregion

        #region UpdateView / updateModel

        /// <summary>
        /// Updates the fields (view) of the form
        /// </summary>
        private void UpdateView() {
            var envList = ProgressEnv.GetList();

            if (envList.Count == 0) {
                // the user needs to add a new one

                return;
            }

            try {
                // Combo box appli
                var appliList = envList.Select(environnement => environnement.Appli).Distinct().ToList();
                if (appliList.Count > 0) {
                    cbAppli.DataSource = appliList;
                    var selectedIdx = appliList.FindIndex(str => str.EqualsCi(ProgressEnv.Current.Appli));
                    cbAppli.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                    // Combo box env letter
                    var envLetterList = envList.Where(environnement => environnement.Appli.EqualsCi(cbAppli.SelectedItem.ToString())).Select(environnement => environnement.EnvLetter).ToList();
                    if (envLetterList.Count > 0) {
                        cbEnvLetter.DataSource = envLetterList;
                        selectedIdx = envLetterList.FindIndex(str => str.EqualsCi(ProgressEnv.Current.EnvLetter));
                        cbEnvLetter.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                        // Combo box database
                        var databaseList = envList.First(environnement => environnement.Appli.EqualsCi(cbAppli.SelectedItem.ToString()) && environnement.EnvLetter.EqualsCi(cbEnvLetter.SelectedItem.ToString())).PfPath.Keys.ToList();
                        if (databaseList.Count > 0) {
                            cbDatabase.DataSource = databaseList;
                            selectedIdx = databaseList.FindIndex(str => str.EqualsCi(Config.Instance.EnvCurrentDatabase));
                            cbDatabase.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when filling comboboxes");
            }

            // fill details
            multitextbox1.Text = ProgressEnv.Current.DataBaseConnection;
            multibox2.Text = ProgressEnv.Current.ProPath;

            textbox1.Text = ProgressEnv.Current.GetCurrentPfPath();
            textbox2.Text = ProgressEnv.Current.IniPath;
            textbox3.Text = ProgressEnv.Current.BaseLocalPath;
            textbox4.Text = ProgressEnv.Current.BaseCompilationPath;
            textbox5.Text = ProgressEnv.Current.ProwinPath;
            textbox6.Text = ProgressEnv.Current.LogFilePath;

            envLabel.Text = ProgressEnv.Current.Label;

            // download database information
            if (DataBase.TryToLoadDatabaseInfo()) {
                btDownload.BackGrndImage = ImageResources.DownloadDbOk;
                toolTip.SetToolTip(btDownload, "<i>The database information for this environment are available and in use in the auto-completion list</i><br><br><b>Click this button</b> to force a refresh of the database information for this environment".BreakText(40, "<br>"));
            } else {
                btDownload.BackGrndImage = ImageResources.DownloadDbNok;
                toolTip.SetToolTip(btDownload, "<i>No information available for this database!</i><br><br><b>Click this button</b> to fetch the database information for this environment, they will be used in the auto-completion list to suggest database names, table names and field names.<br><br>By default, the auto-completion list uses the last environment selected where database info were available.".BreakText(40, "<br>"));
            }
            btDownload.Invalidate();
        }

        private void UpdateModel() {
            ProgressEnv.Current.DataBaseConnection = multitextbox1.Text;
            ProgressEnv.Current.ProPath = multibox2.Text;
            ProgressEnv.Current.PfPath[Config.Instance.EnvCurrentDatabase] = textbox1.Text;
            ProgressEnv.Current.IniPath = textbox2.Text;
            ProgressEnv.Current.BaseLocalPath = textbox3.Text;
            ProgressEnv.Current.BaseCompilationPath = textbox4.Text;
            ProgressEnv.Current.ProwinPath = textbox5.Text;
            ProgressEnv.Current.LogFilePath = textbox6.Text;
            ProgressEnv.Current.Label = envLabel.Text;

            ProgressEnv.Save();
        }

        #endregion

        #region Events

        /// <summary>
        ///  Click on "CANCEL" or "ADD NEW"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buttonPressedEventArgs"></param>
        private void Btcontrol1ButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            if (_isModifying) {
                // we clicked "cancel"
                UpdateView();
                Btcontrol2ButtonPressed(false);
            }
        }

        /// <summary>
        /// Click on "SAVE" of "MODIFY"
        /// </summary>
        private void Btcontrol2ButtonPressed(bool save = true) {
            if (_isModifying) {
                // we clicked "Save"
                _isModifying = false;
                SwitchEnabledDisabled(false);
                btcontrol2.Text = ModifyStr;
                toolTip.SetToolTip(btcontrol2, "Click to <b>modify</b> the information for the selected environment");
                btcontrol1.Text = AddNewStr;
                toolTip.SetToolTip(btcontrol1, "Click to <b>add a new</b> environment");

                if (save)
                    UpdateModel();

            } else {
                // we clicked "modify"
                _isModifying = true;
                SwitchEnabledDisabled(true);
                btcontrol2.Text = SaveStr;
                toolTip.SetToolTip(btcontrol2, "Click to <b>save</b> your modifications");
                btcontrol1.Text = CancelStr;
                toolTip.SetToolTip(btcontrol1, "Click to <b>cancel</b> your modifications");
            } 
        }

        /// <summary>
        /// when changing appli
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAppli_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvCurrentAppli.Equals(cbAppli.SelectedItem.ToString()))
                return;
            Config.Instance.EnvCurrentAppli = cbAppli.SelectedItem.ToString();
            ProgressEnv.SetCurrent();
            UpdateView();
        }

        /// <summary>
        /// when changing env letter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbEnvLetter_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvCurrentEnvLetter.Equals(cbEnvLetter.SelectedItem.ToString()))
                return;
            Config.Instance.EnvCurrentEnvLetter = cbEnvLetter.SelectedItem.ToString();
            ProgressEnv.SetCurrent();
            UpdateView();
        }

        /// <summary>
        /// when changing database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDatabase_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvCurrentDatabase.Equals(cbDatabase.SelectedItem.ToString()))
                return;
            Config.Instance.EnvCurrentDatabase = cbDatabase.SelectedItem.ToString();
            ProgressEnv.SetCurrent();
            UpdateView();
        }

        /// <summary>
        /// Select a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buttonPressedEventArgs"></param>
        private void BtleftOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            var associatedTextBox = GetTextBoxByName(((Control) sender).Name);
            if (associatedTextBox == null) return;

            // if the textbox is disabled, we need to click modify before we can do anything
            if (!associatedTextBox.Enabled) {
                BlinkButton(btcontrol2, ThemeManager.AccentColor);
                return;
            }

            string tag = (string) (associatedTextBox.Tag ?? string.Empty);
            var selectedStuff = tag.Equals("true") ? Utils.ShowFolderSelection(associatedTextBox.Text) : Utils.ShowFileSelection(associatedTextBox.Text, tag);
            if (!string.IsNullOrEmpty(selectedStuff)) {
                associatedTextBox.Text = selectedStuff;
                BlinkTextBox(associatedTextBox, ThemeManager.AccentColor);
            }
        }

        /// <summary>
        /// Open file in folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buttonPressedEventArgs"></param>
        private void BtrightOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            var associatedTextBox = GetTextBoxByName(((Control) sender).Name);
            if (associatedTextBox == null) return;

            string tag = (string) (associatedTextBox.Tag ?? string.Empty);
            var hasOpened = tag.Equals("true") ? Utils.OpenFolder(associatedTextBox.Text) : Utils.OpenFileInFolder(associatedTextBox.Text);
            if (!hasOpened)
                BlinkTextBox(associatedTextBox, ThemeManager.Current.GenericErrorColor);
        }

        private void btDownload_Click(object sender, EventArgs e) {
            // refresh the info after the extraction
            DataBase.OnExtractionDone = UpdateView;
            DataBase.FetchCurrentDbInfo();
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Retrieves the text box reference associated with the button (uses the button's number)
        /// </summary>
        /// <param name="buttonName"></param>
        /// <returns></returns>
        private YamuiTextBox GetTextBoxByName(string buttonName) {
            return (YamuiTextBox) Controls.Find("textbox" + buttonName.Substring(buttonName.Length - 1, 1), true).FirstOrDefault();
        }

        /// <summary>
        /// Makes the given textbox blink
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="blinkColor"></param>
        private void BlinkTextBox(YamuiTextBox textBox, Color blinkColor) {
            textBox.UseCustomBackColor = true;
            Transition.run(textBox, "BackColor", ThemeManager.Current.ButtonColorsNormalBackColor, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { textBox.UseCustomBackColor = false; });
        }
        private void BlinkButton(YamuiButton button, Color blinkColor) {
            button.UseCustomBackColor = true;
            Transition.run(button, "BackColor", ThemeManager.Current.ButtonColorsNormalBackColor, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { button.UseCustomBackColor = false; });
        }

        /// <summary>
        /// Disabled or enabled all textboxes of the form
        /// </summary>
        private void SwitchEnabledDisabled(bool newStatus) {
            foreach (var control in mainPanel.Controls) {
                if (control is YamuiTextBox)
                    ((YamuiTextBox)control).Enabled = newStatus;
            }
        }

        #endregion

    }
}
