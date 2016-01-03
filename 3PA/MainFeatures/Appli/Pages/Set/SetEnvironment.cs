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
using System.Collections.Generic;
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
        private const string AddNewStr = "Add new";

        private const string SaveStr = "Save";
        private const string CancelStr = "Cancel";

        private const string CompileLocallyTrue = "Compile to source directory";
        private const string CompileLocallyFalse = "Compile to distant directory";

        private ViewMode _currentMode = ViewMode.Select;

        #endregion

        #region constructor

        public SetEnvironment() {
            InitializeComponent();

            // sets buttons behavior
            foreach (var control in mainPanel.Controls) {
                if (control is YamuiImageButton) {
                    var x = (YamuiImageButton)control;
                    if (x.Name.StartsWith("btleft")) {
                        // Left button
                        x.BackGrndImage = ImageResources.SelectFile;
                        x.ButtonPressed += BtleftOnButtonPressed;
                        toolTip.SetToolTip(x, "<b>Click</b> to select a new file");
                    } else if (x.Name.StartsWith("btright")) {
                        // right button
                        x.BackGrndImage = ImageResources.OpenInExplorer;
                        x.ButtonPressed += BtrightOnButtonPressed;
                        string tag = (string)(x.Tag ?? string.Empty);
                        toolTip.SetToolTip(x, "<b>Click</b> to " + (tag.Equals("true") ? "open this folder in the explorer" : "to open the containing folder in the explorer"));
                    }
                }
            }

            // tooltips
            toolTip.SetToolTip(cbName, "<b>Select</b> the environment to use<br><br>3P allows the user to define several work environment<br><i>(that can, for instance, correspond to several applications)</i><br>Each environment can also has several suffixes and each<br>environment/suffix couples have their own parameters (see 'details' below)");
            toolTip.SetToolTip(cbSuffix, "<b>Select</b> the environment's suffix");
            toolTip.SetToolTip(flLabel, "The label for this environment (has no use beside being more meaningful than the name)");
            toolTip.SetToolTip(cbDatabase, "<b>Select</b> the database to use for the current environment<br><br>For each environment, you can have several database definition<br>that consists of a database name <i>(it doesn't have to be the physical<br>or logical name of the actual db, it is just a name you are giving it in 3P!)</i><br>and the path to a .pf file that contains the connection info to the data<br><br>This .pf file is used like this in 3P :<div class='ToolTipcodeSnippet'>CONNECT -pf 'your.pf'.</div>");

            toolTip.SetToolTip(flName, "The name of this environment<br><br>3P allows the user to define several work environment<br><i>(that can, for instance, correspond to several applications)</i><br>Each environment can also has several suffixes and each<br>environment/suffix couples have their own parameters (see 'details' below)");
            toolTip.SetToolTip(flSuffix, "This field is optional, you can have several suffixes for a<br>given environment, or you can just use different environment names if it's enough for you");

            toolTip.SetToolTip(btDbEdit, "Click to <b>edit</b> this database definition (name and .pf path)");
            toolTip.SetToolTip(btDbAdd, "Click to <b>add</b> a new database definition (name and .pf path)<br>for the current environment");
            toolTip.SetToolTip(btDbDelete, "Click to <b>delete</b> this database definition");
            toolTip.SetToolTip(btDeleteDownload, "Click here to <b>delete</b> the extracted database structure info");

            toolTip.SetToolTip(flExtraPf, "You can set a database connection that will occur for the current<br>environment, no matter which database definition is selected<br><br>This field is saved as a .pf file and is used like this in 3P :<div class='ToolTipcodeSnippet'>CONNECT -pf 'extra.pf'.</div><br><i>This is a different connect statement that for the .pf above</i>");

            toolTip.SetToolTip(textbox2, "Path to an .ini file, which as a <b>PROPATH=</b> field<br>(the section in which this field is doesn't matter)<br>that lists the directories to use for the compilation/execution of your progress files");
            toolTip.SetToolTip(flExtraProPath, "A list of directories to be used when compiling/executing your 4GL code<br>They can be separated by a ',' or ';' or new lines '\\n'");

            toolTip.SetToolTip(textbox3, "Path to your project directory<br>It is used to find the .p or .w you RUN in your code");
            toolTip.SetToolTip(textbox4, "Path to the directory where you want your .r and .lst files to be<br>moved after a successful compilation");
            toolTip.SetToolTip(textbox5, "The path to the prowin.exe (or prowin32.exe), it is usually located in :<div class='ToolTipcodeSnippet'>%INSTALL_DIR%\\client\\vXXXX\\dlc\\bin\\</div>");
            toolTip.SetToolTip(textbox6, "Path to your server.log file, for a quick access");

            toolTip.SetToolTip(tgCompilLocl, "<b>TOGGLE ON</b> to move .r and .lst files to the source folder after the compilation<br>Or <b>TOGGLE OFF</b> to move .r and .lst files to the above distant folder after the compilation");

            toolTip.SetToolTip(btDelete, "Click here to <b>delete</b> the current environment");

            // buttons
            btDbAdd.BackGrndImage = ImageResources.PlusDb;
            btDbDelete.BackGrndImage = ImageResources.MinusDb;
            btDbEdit.BackGrndImage = ImageResources.EditDb;
            btDeleteDownload.BackGrndImage = ImageResources.Delete;

            btcontrol2.ButtonPressed += Btcontrol2OnButtonPressed;
            btcontrol1.ButtonPressed += Btcontrol1ButtonPressed;
            btcontrol1.MouseDown += Btcontrol1OnMouseDown;
            btDbAdd.ButtonPressed += BtDbAddOnButtonPressed;
            btDbEdit.ButtonPressed += BtDbEditOnButtonPressed;
            btDbDelete.ButtonPressed += BtDbDeleteOnButtonPressed;
            tgCompilLocl.CheckedChanged += TgCompilLoclOnCheckedChanged;
            btDeleteDownload.ButtonPressed += BtDeleteDownloadOnButtonPressed;
            btDelete.ButtonPressed += BtDeleteOnButtonPressed;

            ToggleMode(ViewMode.Select);
        }

        #endregion

        #region Update view / update Model

        private bool Save() {
            bool output = true;
            switch (_currentMode) {
                case ViewMode.DbDelete:
                    ProEnvironment.Current.RemoveCurrentPfPath();
                    break;

                case ViewMode.Delete:
                    ProEnvironment.DeleteCurrentEnv();
                    ProEnvironment.SetCurrent();
                    break;

                case ViewMode.DbAddNew:
                case ViewMode.DbEdit:
                    if (string.IsNullOrWhiteSpace(flDatabase.Text)) {
                        BlinkTextBox(flDatabase, ThemeManager.Current.GenericErrorColor);
                        return false;
                    }

                    if (_currentMode == ViewMode.DbAddNew) {
                        output = ProEnvironment.Current.AddPfPath(flDatabase.Text, textbox1.Text);
                    } else {
                        ProEnvironment.Current.RemoveCurrentPfPath();
                        output = ProEnvironment.Current.AddPfPath(flDatabase.Text, textbox1.Text);
                    }

                    if (!output) {
                        BlinkTextBox(flDatabase, ThemeManager.Current.GenericErrorColor);
                    } else {
                        Config.Instance.EnvDatabase = flDatabase.Text;
                        ProEnvironment.SaveList();
                    }
                    break;

                case ViewMode.AddNew:
                case ViewMode.Edit:
                    foreach (var box in new List<YamuiTextBox> { flName, flLabel}.Where(box => string.IsNullOrWhiteSpace(box.Text))) {
                        BlinkTextBox(box, ThemeManager.Current.GenericErrorColor);
                        return false;
                    }

                    var newEnv = new ProEnvironmentObject {
                        Name = flName.Text,
                        Suffix = flSuffix.Text,
                        Label = flLabel.Text,
                        ExtraPf = flExtraPf.Text,
                        IniPath = textbox2.Text,
                        ExtraProPath = flExtraProPath.Text,
                        BaseLocalPath = textbox3.Text,
                        BaseCompilationPath = textbox4.Text,
                        ProwinPath = textbox5.Text,
                        LogFilePath = textbox6.Text,
                        CmdLineParameters = flCmdLine.Text,
                    };

                    ProEnvironment.SaveEnv((_currentMode == ViewMode.Edit) ? ProEnvironment.Current : null, newEnv);
                    Config.Instance.EnvName = newEnv.Name;
                    Config.Instance.EnvSuffix = newEnv.Suffix;
                    ProEnvironment.SetCurrent();
                    break;
            }

            if (output) {
                ProEnvironment.SaveList();
            }

            return output;
        }

        private enum ViewMode {
            Select,
            Edit,
            AddNew,
            Delete,
            DbEdit,
            DbAddNew,
            DbDelete,
        }

        private void ToggleMode(ViewMode mode) {

            cbName.SelectedIndexChanged -= cbName_SelectedIndexChanged;
            cbSuffix.SelectedIndexChanged -= cbSuffix_SelectedIndexChanged;
            cbDatabase.SelectedIndexChanged -= cbDatabase_SelectedIndexChanged;

            // show cb
            ShowComboBoxes((mode == ViewMode.Select || mode == ViewMode.DbAddNew || mode == ViewMode.DbEdit));

            // enable text boxes
            EnableAllTextBoxes(!(mode == ViewMode.Select || mode == ViewMode.DbAddNew || mode == ViewMode.DbEdit));

            // Fill combo boxes
            if (mode == ViewMode.Select) {
                var envList = ProEnvironment.GetList;
                try {
                    // Fill combo box appli
                    var appliList = envList.Select(environnement => environnement.Name).Distinct().ToList();
                    if (appliList.Count > 0) {
                        cbName.DataSource = appliList;
                        var selectedIdx = appliList.FindIndex(str => str.EqualsCi(ProEnvironment.Current.Name));
                        cbName.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                        // Combo box env letter
                        var envLetterList = envList.Where(environnement => environnement.Name.EqualsCi(ProEnvironment.Current.Name)).Select(environnement => environnement.Suffix).ToList();

                        // empty database cb
                        cbDatabase.DataSource = new List<string>();

                        if (envLetterList.Count > 0) {

                            // hide the combo if there is only one item
                            if (envLetterList.Count == 1) {
                                cbSuffix.Hide();
                            } else {
                                cbSuffix.Show();
                                cbSuffix.DataSource = envLetterList;
                                selectedIdx = envLetterList.FindIndex(str => str.EqualsCi(ProEnvironment.Current.Suffix));
                                cbSuffix.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;
                            }

                            // Combo box database
                            var dic = envList.FirstOrDefault(environnement => environnement.Name.EqualsCi(ProEnvironment.Current.Name) && environnement.Suffix.EqualsCi(ProEnvironment.Current.Suffix));
                            if (dic != null) {
                                var databaseList = dic.DbConnectionInfo.Keys.ToList().OrderBy(s => s).ToList();
                                if (databaseList.Count > 0) {
                                    cbDatabase.DataSource = databaseList;
                                    selectedIdx = databaseList.FindIndex(str => str.EqualsCi(Config.Instance.EnvDatabase));
                                    cbDatabase.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;
                                }
                            }
                        } else {
                            cbSuffix.Hide();
                        }
                    } else {
                        // the user needs to add a new one
                        btcontrol1.UseCustomBackColor = true;
                        btcontrol1.BackColor = ThemeManager.AccentColor;
                    }
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error when filling comboboxes");
                }
            }

            // btleft
            foreach (var control in mainPanel.Controls) {
                if (control is YamuiImageButton) {
                    var x = (YamuiImageButton)control;
                    if (x.Name.StartsWith("btleft")) {
                        if (mode == ViewMode.Select || mode == ViewMode.DbAddNew || mode == ViewMode.DbEdit) {
                            x.Hide();
                        } else {
                            x.Show();
                        }
                    }
                }
            }

            // all database buttons
            if (mode == ViewMode.Select) {
                btDbAdd.Show();
                btDbDelete.Show();
                btDbEdit.Show();
                btDownload.Show();
                btDeleteDownload.Show();
            } else {
                btDbAdd.Hide();
                btDbDelete.Hide();
                btDbEdit.Hide();
                btDownload.Hide();
                btDeleteDownload.Hide();
            }
            
            // editing pf?
            if (mode == ViewMode.DbAddNew || mode == ViewMode.DbEdit) {
                flDatabase.Show();
                cbDatabase.Hide();
                flDatabase.Enabled = true;
                cbDatabase.Enabled = false;
                textbox1.Enabled = true;
                btleft1.Show();
                cbName.Enabled = false;
                cbSuffix.Enabled = false;
            } else {
                flDatabase.Hide();
                cbDatabase.Show();
                flDatabase.Enabled = false;
                cbDatabase.Enabled = (mode == ViewMode.Select);
                textbox1.Enabled = false;
                btleft1.Hide();
                cbName.Enabled = true;
                cbSuffix.Enabled = true;
            }

            // bottom buttons
            if (mode == ViewMode.Select) {
                btcontrol2.Text = ModifyStr;
                toolTip.SetToolTip(btcontrol2, "Click to <b>modify</b> the information for the current environment");
                btcontrol1.Text = AddNewStr;
                toolTip.SetToolTip(btcontrol1, "Click to <b>add a new</b> environment<br><i>Right click to duplicate the current environment</i>");
            } else {
                btcontrol2.Text = SaveStr;
                toolTip.SetToolTip(btcontrol2, "Click to <b>save</b> your modifications");
                btcontrol1.Text = CancelStr;
                toolTip.SetToolTip(btcontrol1, "Click to <b>cancel</b> your modifications");
            }
            
            // fill details
            flName.Text = ProEnvironment.Current.Name;
            flSuffix.Text = ProEnvironment.Current.Suffix;
            flLabel.Text = ProEnvironment.Current.Label;

            txLabel.Text = ProEnvironment.Current.Label;

            flExtraPf.Text = ProEnvironment.Current.ExtraPf;
            flExtraProPath.Text = ProEnvironment.Current.ExtraProPath;
            flCmdLine.Text = ProEnvironment.Current.CmdLineParameters;

            flDatabase.Text = (mode == ViewMode.DbAddNew ? string.Empty : Config.Instance.EnvDatabase);

            textbox1.Text = (mode == ViewMode.DbAddNew ? string.Empty : ProEnvironment.Current.GetPfPath());
            textbox2.Text = ProEnvironment.Current.IniPath;
            textbox3.Text = ProEnvironment.Current.BaseLocalPath;
            textbox4.Text = ProEnvironment.Current.BaseCompilationPath;
            textbox5.Text = ProEnvironment.Current.ProwinPath;
            textbox6.Text = ProEnvironment.Current.LogFilePath;

            tgCompilLocl.Checked = Config.Instance.GlobalCompileFilesLocally;
            lblLocally.Text = tgCompilLocl.Checked ? CompileLocallyTrue : CompileLocallyFalse;

            // download database information
            UpdateDownloadButton();

            if (mode == ViewMode.AddNew) {
                foreach (var control in mainPanel.Controls) {
                    if (control is YamuiTextBox)
                        ((YamuiTextBox)control).Text = string.Empty;
                }
                cbDatabase.DataSource = new List<string>();
            }

            btDelete.Enabled = ProEnvironment.GetList.Count > 1;

            if (mode != _currentMode) {
                BlinkButton(btcontrol1, ThemeManager.AccentColor);
                BlinkButton(btcontrol2, ThemeManager.AccentColor);
            }

            cbName.SelectedIndexChanged += cbName_SelectedIndexChanged;
            cbSuffix.SelectedIndexChanged += cbSuffix_SelectedIndexChanged;
            cbDatabase.SelectedIndexChanged += cbDatabase_SelectedIndexChanged;

            // save current mode
            _currentMode = mode;
        }

        private void UpdateDownloadButton() {
            // download database information
            if (DataBase.TryToLoadDatabaseInfo()) {
                btDownload.BackGrndImage = ImageResources.DownloadDbOk;
                btDeleteDownload.Enabled = true;
                btDeleteDownload.FakeDisabled = false;
                toolTip.SetToolTip(btDownload, "<i>The database information for this environment are available and in use in the auto-completion list</i><br><br><b>Click this button</b> to force a refresh of the database information for this environment");
            } else {
                btDownload.BackGrndImage = ImageResources.DownloadDbNok;
                btDeleteDownload.Enabled = false;
                btDeleteDownload.FakeDisabled = true;
                toolTip.SetToolTip(btDownload, "<i>No information available for this database!</i><br><br><b>Click this button</b> to fetch the database information for this environment,<br>they will be used in the auto-completion list to suggest database names,<br>table names and field names.<br><br>By default, the auto-completion list uses the last environment <br>selected where database info were available.");
            }
            btDownload.Invalidate();
        }

        #endregion

        #region Events

        private void btDownload_Click(object sender, EventArgs e) {
            // refresh the info after the extraction
            DataBase.FetchCurrentDbInfo(UpdateDownloadButton);
        }
        private void BtDeleteDownloadOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            DataBase.DeleteCurrentDbInfo();
            UpdateDownloadButton();
        }

        /// <summary>
        /// On change of compile locally
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void TgCompilLoclOnCheckedChanged(object sender, EventArgs eventArgs) {
            Config.Instance.GlobalCompileFilesLocally = tgCompilLocl.Checked;
            lblLocally.Text = tgCompilLocl.Checked ? CompileLocallyTrue : CompileLocallyFalse;
        }


        /// <summary>
        ///  Click on "CANCEL" or "ADD NEW"
        /// </summary>
        private void Btcontrol1ButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            btcontrol1.UseCustomBackColor = false;
            if (_currentMode == ViewMode.Select) {
                // Add new
                ToggleMode(ViewMode.AddNew);
            } else {
                // cancel
                ToggleMode(ViewMode.Select);
            }
        }

        private void Btcontrol1OnMouseDown(object sender, MouseEventArgs mouseEventArgs) {
            btcontrol1.UseCustomBackColor = false;
            if (mouseEventArgs.Button == MouseButtons.Right && _currentMode == ViewMode.Select) {
                // duplicate, add new
                ToggleMode(ViewMode.AddNew);
            }
        }


        /// <summary>
        /// Click on "SAVE" or "MODIFY"
        /// </summary>
        private void Btcontrol2OnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            if (_currentMode == ViewMode.Select) {
                // modify
                ToggleMode(ViewMode.Edit);
            } else {
                // save
                if(Save())
                    ToggleMode(ViewMode.Select);
            }
        }

        private void BtDeleteOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            _currentMode = ViewMode.Delete;
            if (Save())
                ToggleMode(ViewMode.Select);
        }

        private void BtDbDeleteOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            _currentMode = ViewMode.DbDelete;
            if (Save())
                ToggleMode(ViewMode.Select);
        }

        private void BtDbEditOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            ToggleMode(ViewMode.DbEdit);
        }

        private void BtDbAddOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            ToggleMode(ViewMode.DbAddNew);
        }


        /// <summary>
        /// when changing appli
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbName_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvName.Equals(cbName.SelectedItem.ToString()))
                return;
            Config.Instance.EnvName = cbName.SelectedItem.ToString();
            ProEnvironment.SetCurrent();
            ToggleMode(ViewMode.Select);
        }

        /// <summary>
        /// when changing env letter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSuffix_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvSuffix.Equals(cbSuffix.SelectedItem.ToString()))
                return;
            Config.Instance.EnvSuffix = cbSuffix.SelectedItem.ToString();
            ProEnvironment.SetCurrent();
            ToggleMode(ViewMode.Select);
        }

        /// <summary>
        /// when changing database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDatabase_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvDatabase.Equals(cbDatabase.SelectedItem.ToString()))
                return;
            Config.Instance.EnvDatabase = cbDatabase.SelectedItem.ToString();
            ProEnvironment.SetCurrent();
            ToggleMode(ViewMode.Select);
        }

        /// <summary>
        /// Select a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buttonPressedEventArgs"></param>
        private void BtleftOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            var associatedTextBox = GetTextBoxByName(((Control)sender).Name);
            if (associatedTextBox == null) return;
            string tag = (string)(associatedTextBox.Tag ?? string.Empty);
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
        private void BtrightOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            var associatedTextBox = GetTextBoxByName(((Control)sender).Name);
            if (associatedTextBox == null) return;

            string tag = (string)(associatedTextBox.Tag ?? string.Empty);
            var hasOpened = tag.Equals("true") ? Utils.OpenFolder(associatedTextBox.Text) : Utils.OpenFileInFolder(associatedTextBox.Text);
            if (!hasOpened)
                BlinkTextBox(associatedTextBox, ThemeManager.Current.GenericErrorColor);
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Retrieves the text box reference associated with the button (uses the button's number)
        /// </summary>
        /// <param name="buttonName"></param>
        /// <returns></returns>
        private YamuiTextBox GetTextBoxByName(string buttonName) {
            return (YamuiTextBox)Controls.Find("textbox" + buttonName.Substring(buttonName.Length - 1, 1), true).FirstOrDefault();
        }

        /// <summary>
        /// Makes the given textbox blink
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="blinkColor"></param>
        private void BlinkTextBox(YamuiTextBox textBox, Color blinkColor) {
            textBox.UseCustomBackColor = true;
            Transition.run(textBox, "CustomBackColor", ThemeManager.Current.ButtonColorsNormalBackColor, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { textBox.UseCustomBackColor = false; });
        }
        private void BlinkButton(YamuiButton button, Color blinkColor) {
            button.UseCustomBackColor = true;
            Transition.run(button, "BackColor", ThemeManager.Current.ButtonColorsNormalBackColor, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { button.UseCustomBackColor = false; });
        }

        /// <summary>
        /// false to disable all textboxes of the form, true to enable
        /// </summary>
        private void EnableAllTextBoxes(bool newStatus) {
            foreach (var control in mainPanel.Controls) {
                if (control is YamuiTextBox)
                    ((YamuiTextBox)control).Enabled = newStatus;
            }
        }

        private void ShowComboBoxes(bool state) {
            if (state) {
                flName.Hide();
                flSuffix.Hide();
                flLabel.Hide();
                cbName.Show();
                cbSuffix.Show();
                txLabel.Show();
            } else {
                flName.Show();
                flSuffix.Show();
                flLabel.Show();
                cbName.Hide();
                cbSuffix.Hide();
                txLabel.Hide();
            }
        }

        #endregion


    }
}
