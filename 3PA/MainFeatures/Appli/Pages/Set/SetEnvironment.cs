#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.Pro;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    internal partial class SetEnvironment : YamuiPage {
        #region fields

        private ViewMode _currentMode;

        private bool _unsafeDelete;

        #endregion

        #region constructor

        public SetEnvironment() {
            InitializeComponent();

            // sets buttons behavior
            foreach (var control in scrollPanel.ContentPanel.Controls) {
                if (control is YamuiButtonImage) {
                    var x = (YamuiButtonImage) control;
                    if (x.Name.StartsWith("btleft")) {
                        // Left button
                        x.BackGrndImage = ImageResources.SelectFile;
                        x.ButtonPressed += BtleftOnButtonPressed;
                        toolTip.SetToolTip(x, "Click to <b>select</b> a file / folder");
                    } else if (x.Name.StartsWith("btright")) {
                        // right button
                        x.BackGrndImage = ImageResources.OpenInExplorer;
                        x.ButtonPressed += OpenFileOnButtonPressed;
                        x.MouseDown += OpenFileOnMouseDown;
                        toolTip.SetToolTip(x, (string.IsNullOrEmpty((string) (x.Tag ?? string.Empty)) ? "Left click to <b>open</b> this file in notepad++<br>Right click to <b>open</b> this file / folder in the explorer" : "Click to <b>open the directory</b> in the windows explorer"));
                    }
                }
            }

            // tooltips
            toolTip.SetToolTip(cbName, "Select the <b>environment to use</b><br><br>3P allows the user to define several work environment<br><i>(that can, for instance, correspond to several applications)</i><br>Each environment can also has several suffixes and each<br>environment/suffix couples have their own parameters (see 'details' below)");
            toolTip.SetToolTip(cbSuffix, "Select the <b>environment's suffix</b>");
            toolTip.SetToolTip(flName, "The name of this environment<br><br>3P allows the user to define several work environment<br><i>(that can, for instance, correspond to several applications)</i><br>Each environment can also has several suffixes and each<br>environment/suffix couples have their own parameters (see 'details' below)");
            toolTip.SetToolTip(flSuffix, "This field is optional, you can have several suffixes for a<br>given environment, or you can just use different environment names if it's enough for you");
            toolTip.SetToolTip(flLabel, "The label for this environment (has no use beside being more meaningful than the name)");

            var textTool = "Select <b>the database</b> to use for the current environment<br><br>For each environment, you can have several database definition<br>that consists of a database name <i>(it doesn't have to be the physical<br>or logical name of the actual db, it is just a name you are giving it in 3P!)</i><br>and the path to a .pf file that contains the connection info to the data<br><br>This .pf file is used like this in 3P :<div class='ToolTipcodeSnippet'>CONNECT -pf 'your.pf'.</div>";
            toolTip.SetToolTip(cbDatabase, textTool);
            cbDatabase.WaterMark = "No database registered for this environment";
            toolTip.SetToolTip(lbl_listdb, textTool);

            toolTip.SetToolTip(flDatabase, "Enter the name for this database definition");
            toolTip.SetToolTip(textbox1, "Path to your parameter file (.pf) containing the database<br>connection information<br><br>This .pf file is used like this in 3P :<div class='ToolTipcodeSnippet'>CONNECT -pf 'your.pf'.</div>");

            toolTip.SetToolTip(btDbEdit, "Click to <b>edit</b> this database definition (name and .pf path)");
            toolTip.SetToolTip(btDbAdd, "Click to <b>add</b> a new database definition (name and .pf path)<br>for the current environment");
            toolTip.SetToolTip(btDbDelete, "Click to <b>delete</b> this database definition");
            toolTip.SetToolTip(btDbSave, "Click to <b>save</b> your modifications");
            toolTip.SetToolTip(btDbCancel, "Click to <b>cancel</b> your modifications");
            toolTip.SetToolTip(btDbDeleteDownload, "Click here to <b>delete</b> the extracted database structure info");
            toolTip.SetToolTip(btDbView, "Click here to <b>view</b> the content of the file holding the database structure info for 3P");

            textTool = "You can set a database connection that will occur for the current<br>environment, no matter which database definition is selected.<br><br>This field is used like this in 3P :<div class='ToolTipcodeSnippet'>CONNECT VALUE(my_info).</div><i>This is a different connect statement that for the .pf above</i><br><br>Below is an example of content to connect 2 databases :<div class='ToolTipcodeSnippet'>-db base1 -ld mylogicalName1 -H 127.0.0.1 -S 1024<br>-db C:\\wrk\\sport2000.db -1</div>";
            toolTip.SetToolTip(flExtraPf, textTool);
            toolTip.SetToolTip(htmlLabel8, textTool);

            textTool = "Path to an .ini file, which as a <b>PROPATH=</b> field<br>(the section in which this field is doesn't matter)<br>that lists the directories to use for the compilation/execution of your progress files";
            toolTip.SetToolTip(textbox2, textTool);
            toolTip.SetToolTip(htmlLabel2, textTool);

            toolTip.SetToolTip(flExtraProPath, "A list of directories to be used when compiling/executing your 4GL code<br>They can be separated by a ',' or ';' or new lines '\\n'<br><br><i>You can specify relative paths, the working directory (i.e. base path)<br>is the base local directory</i>");

            textTool = "Path to your project directory<br>This should be your repository folder, where you keep the source files<br>It is used (among other things) to find the .p or .w you RUN in your code";
            toolTip.SetToolTip(textbox3, textTool);
            toolTip.SetToolTip(htmlLabel3, textTool);

            textTool = "Set the base directory to which you want to deploy your files for this environment<br>Your r-code can be automatically moved to this location after a successful compilation<br><br><i>See the deployment screen for more information</i>";
            toolTip.SetToolTip(textbox4, textTool);
            toolTip.SetToolTip(htmlLabel4, textTool);

            textTool = "The path to the prowin.exe (or prowin32.exe for 32 bits version), it is usually located in :<div class='ToolTipcodeSnippet'>%INSTALL_DIR%\\client\\vXXXX\\dlc\\bin\\</div>";
            toolTip.SetToolTip(textbox5, textTool);
            toolTip.SetToolTip(htmlLabel5, textTool);

            toolTip.SetToolTip(htmlLabel6, "Appended to the prowin.exe command line<br>you can define custom options here");
            toolTip.SetToolTip(flCmdLine, @"This field can be used if you have special needs when you compile or run a progress program<br>For instance, you can activate the logs when you run a program by setting those parameters :<br><div class='ToolTipcodeSnippet'>-clientlog ""client.log"" - logginglevel ""3"" - logentrytypes ""4GLMessages,4GLTrace,FileID""</div>");
            toolTip.SetToolTip(textbox6, "Path to your server.log file, for a quick access");

            textTool = "The path to a progress program that should be executed before any progress execution<br>(compilation, check syntax and so on...)<br>It can either be an absolute path or a relative path from your PROPATH<br><br><i>You can, for instance, use this program to dynamically connect a database</i>";
            toolTip.SetToolTip(textbox7, textTool);
            toolTip.SetToolTip(htmlLabel11, textTool);

            textTool = "The path to a progress program that should be executed after any progress execution<br>(compilation, check syntax and so on...)<br>It can either be an absolute path or a relative path from your PROPATH<br><br><i>You can, for instance, use this program to dynamically disconnect a database</i>";
            toolTip.SetToolTip(textbox8, textTool);
            toolTip.SetToolTip(htmlLabel12, textTool);

            toolTip.SetToolTip(tgCompLocally, "By default (toggle on), your files will be compiled next to the source code<br>You can also chose to automatically deploy your r-code/.lst automatically when they are compiled (toggle off)<br><br><i>Check the deployment screen to learn how to configure your deployment!</i>");

            toolTip.SetToolTip(btEdit, "Click to <b>modify</b> the information for the current environment");
            toolTip.SetToolTip(btAdd, "Click to <b>add a new</b> environment<br>");
            toolTip.SetToolTip(btDelete, "Click here to <b>delete</b> the current environment");
            toolTip.SetToolTip(btCopy, "Click to <b>copy (duplicate)</b> the current environment");
            toolTip.SetToolTip(btSave, "Click to <b>save</b> your modifications");
            toolTip.SetToolTip(btCancel, "Click to <b>cancel</b> your modifications");

            // buttons
            btDbDeleteDownload.BackGrndImage = ImageResources.Delete;
            btDbView.BackGrndImage = ImageResources.ViewFile;

            btAdd.BackGrndImage = ImageResources.Add;
            btCancel.BackGrndImage = ImageResources.Cancel;
            btSave.BackGrndImage = ImageResources.Save;
            btEdit.BackGrndImage = ImageResources.Edit;
            btCopy.BackGrndImage = ImageResources.Copy;
            btDelete.BackGrndImage = ImageResources.Del;

            btDbAdd.BackGrndImage = ImageResources.Add;
            btDbCancel.BackGrndImage = ImageResources.Cancel;
            btDbSave.BackGrndImage = ImageResources.Save;
            btDbEdit.BackGrndImage = ImageResources.Edit;
            btDbDelete.BackGrndImage = ImageResources.Del;

            btEdit.ButtonPressed += BtModifyOnButtonPressed;
            btAdd.ButtonPressed += BtAddOnButtonPressed;
            btCopy.ButtonPressed += BtduplicateOnButtonPressed;
            btDelete.ButtonPressed += BtDeleteOnButtonPressed;
            btSave.ButtonPressed += BtSaveOnButtonPressed;
            btCancel.ButtonPressed += BtCancelOnButtonPressed;

            btDbEdit.ButtonPressed += BtDbEditOnButtonPressed;
            btDbAdd.ButtonPressed += BtDbAddOnButtonPressed;
            btDbDelete.ButtonPressed += BtDbDeleteOnButtonPressed;
            btDbSave.ButtonPressed += BtDbSaveOnButtonPressed;
            btDbCancel.ButtonPressed += BtDbCancelOnButtonPressed;

            btDbDeleteDownload.ButtonPressed += BtDeleteDownloadOnButtonPressed;
            btDbDownload.ButtonPressed += BtDownloadOnButtonPressed;
            btDbView.ButtonPressed += BtDbViewOnButtonPressed;

            tgCompLocally.ButtonPressed += TgCompLocallyOnCheckedChanged;

            cbName.SelectedIndexChangedByUser += cbName_SelectedIndexChanged;
            cbSuffix.SelectedIndexChangedByUser += cbSuffix_SelectedIndexChanged;
            cbDatabase.SelectedIndexChangedByUser += cbDatabase_SelectedIndexChanged;

            _currentMode = ViewMode.Edit;
            ToggleMode(ViewMode.Select);

            linkurl.Text = @"<img src='Help'><a href='" + Config.UrlHelpSetEnv + @"'>How to set up a new environment?</a>";

            // register to change env event
            ProEnvironment.OnEnvironmentChange += () => ToggleMode(ViewMode.Select);

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
        }

        #endregion

        public override void OnShow() {
            ToggleMode(ViewMode.Select);
        }

        #region Update view / update Model

        private bool Save() {
            switch (_currentMode) {
                case ViewMode.DbDelete:
                    ProEnvironment.Current.RemoveCurrentPfPath();
                    break;

                case ViewMode.Delete:
                    ProEnvironment.DeleteCurrent();
                    break;

                case ViewMode.DbAdd:
                case ViewMode.DbEdit:
                    if (!(_currentMode == ViewMode.DbAdd ? ProEnvironment.Current.AddPfPath(flDatabase.Text, textbox1.Text) : ProEnvironment.Current.ModifyPfPath(flDatabase.Text, textbox1.Text))) {
                        BlinkTextBox(flDatabase, ThemeManager.Current.GenericErrorColor);
                        return false;
                    }
                    break;

                case ViewMode.Add:
                case ViewMode.Edit:
                case ViewMode.Copy:
                    // mandatory fields
                    foreach (var box in new List<YamuiTextBox> {flName}.Where(box => string.IsNullOrWhiteSpace(box.Text))) {
                        BlinkTextBox(box, ThemeManager.Current.GenericErrorColor);
                        return false;
                    }

                    var newEnv = new ProEnvironment.ProEnvironmentObject {
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
                        PreExecutionProgram = textbox7.Text,
                        PostExecutionProgram = textbox8.Text,
                        CmdLineParameters = flCmdLine.Text,
                        DbConnectionInfo = _currentMode == ViewMode.Add ? new Dictionary<string, string>() : ProEnvironment.Current.DbConnectionInfo,
                        CompileLocally = tgCompLocally.Checked
                    };

                    if (_currentMode != ViewMode.Edit && (ProEnvironment.GetList.Exists(env => env.Name.EqualsCi(newEnv.Name) && env.Suffix.EqualsCi(newEnv.Suffix)))) {
                        // name + suffix must be unique!
                        BlinkTextBox(flName, ThemeManager.Current.GenericErrorColor);
                        BlinkTextBox(flSuffix, ThemeManager.Current.GenericErrorColor);
                        return false;
                    }

                    ProEnvironment.Modify((_currentMode == ViewMode.Edit) ? ProEnvironment.Current : null, newEnv);
                    break;
            }

            ProEnvironment.SaveList();
            return true;
        }

        private enum ViewMode {
            Select,
            Edit,
            Add,
            Copy,
            Delete,
            DbEdit,
            DbAdd,
            DbDelete
        }

        private void ToggleMode(ViewMode mode) {
            if (IsHandleCreated) {
                BeginInvoke((Action) delegate {
                    // mode
                    var isAddOrEdit = (mode == ViewMode.Add || mode == ViewMode.Copy || mode == ViewMode.Edit);
                    var isDbAddOrEdit = (mode == ViewMode.DbAdd || mode == ViewMode.DbEdit);
                    var isSelect = (mode == ViewMode.Select);

                    // selection
                    flName.Visible = flSuffix.Visible = flLabel.Visible = isAddOrEdit;
                    cbName.Visible = cbSuffix.Visible = !isAddOrEdit;
                    cbName.Enabled = cbSuffix.Enabled = !isAddOrEdit && !isDbAddOrEdit;
                    txLabel.Text = ProEnvironment.Current.Label;

                    // Fill combo boxes
                    if (isSelect) {
                        var envList = ProEnvironment.GetList;
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
                                        selectedIdx = databaseList.FindIndex(str => str.EqualsCi(ProEnvironment.Current.GetCurrentDb()));
                                        cbDatabase.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;
                                    }
                                }
                            } else {
                                cbSuffix.Hide();
                            }
                        } else {
                            // the user needs to add a new one
                            btAdd.UseCustomBackColor = true;
                            btAdd.BackColor = ThemeManager.Current.AccentColor;
                        }
                    }

                    // handle pf dictionnary
                    flDatabase.Visible = isDbAddOrEdit;
                    cbDatabase.Visible = !flDatabase.Visible;

                    // entering or leaving DB add/edit mode
                    if (isDbAddOrEdit || _currentMode == ViewMode.DbAdd || _currentMode == ViewMode.DbEdit) {
                        flDatabase.Enabled = textbox1.Enabled = isDbAddOrEdit;
                        areaEnv.SetPropertyOnArea("Visible", !isDbAddOrEdit);
                        areaDb.SetPropertyOnArea("Visible", !isDbAddOrEdit);
                        areaLeftButtons.SetPropertyOnArea("Enabled", isDbAddOrEdit);
                    }

                    // entering or leaving add/edit mode
                    else if (isAddOrEdit || _currentMode == ViewMode.Add || _currentMode == ViewMode.Copy || _currentMode == ViewMode.Edit) {
                        EnableAllTextBoxes(isAddOrEdit);
                        areaPf.SetPropertyOnArea("Visible", !isAddOrEdit);
                        areaDb.SetPropertyOnArea("Visible", !isAddOrEdit);
                        areaLeftButtons.SetPropertyOnArea("Enabled", isAddOrEdit);
                    }

                    // update the download database button
                    UpdateDownloadButton();

                    // buttons to handle pf files
                    btDbAdd.Visible = btDbEdit.Visible = btDbDelete.Visible = isSelect;
                    btDbSave.Visible = btDbCancel.Visible = isDbAddOrEdit;
                    btDbDelete.Enabled = ProEnvironment.Current.DbConnectionInfo.Count >= 1;

                    // buttons modify/new/duplicate/delete
                    btEdit.Visible = btAdd.Visible = btDelete.Visible = btCopy.Visible = isSelect;
                    btSave.Visible = btCancel.Visible = isAddOrEdit;
                    btDelete.Enabled = ProEnvironment.GetList.Count > 1;

                    if (mode == ViewMode.Add) {
                        // reset fields when adding a new env
                        foreach (var control in scrollPanel.ContentPanel.Controls) {
                            if (control is YamuiTextBox)
                                ((YamuiTextBox) control).Text = string.Empty;
                        }
                        cbDatabase.DataSource = new List<string>();
                        var defaultEnv = new ProEnvironment.ProEnvironmentObject();
                        tgCompLocally.Checked = defaultEnv.CompileLocally;
                    } else if (mode == ViewMode.DbAdd) {
                        // reset fields when adding a new pf
                        flDatabase.Text = string.Empty;
                        textbox1.Text = string.Empty;
                    } else {
                        // fill details
                        flName.Text = ProEnvironment.Current.Name;
                        flSuffix.Text = ProEnvironment.Current.Suffix;
                        flLabel.Text = ProEnvironment.Current.Label;
                        flExtraPf.Text = ProEnvironment.Current.ExtraPf;
                        flExtraProPath.Text = ProEnvironment.Current.ExtraProPath;
                        flCmdLine.Text = ProEnvironment.Current.CmdLineParameters;
                        flDatabase.Text = ProEnvironment.Current.GetCurrentDb();
                        textbox1.Text = ProEnvironment.Current.GetPfPath();
                        textbox2.Text = ProEnvironment.Current.IniPath;
                        textbox3.Text = ProEnvironment.Current.BaseLocalPath;
                        textbox4.Text = ProEnvironment.Current.BaseCompilationPath;
                        textbox5.Text = ProEnvironment.Current.ProwinPath;
                        textbox6.Text = ProEnvironment.Current.LogFilePath;
                        textbox7.Text = ProEnvironment.Current.PreExecutionProgram;
                        textbox8.Text = ProEnvironment.Current.PostExecutionProgram;

                        tgCompLocally.Checked = ProEnvironment.Current.CompileLocally;
                    }

                    // blink when changing mode
                    if (mode != _currentMode && mode != ViewMode.Select) {
                        if (isAddOrEdit) {
                            BlinkButton(btSave, ThemeManager.Current.AccentColor);
                            BlinkButton(btCancel, ThemeManager.Current.AccentColor);
                            ActiveControl = btSave;
                        } else {
                            BlinkButton(btDbSave, ThemeManager.Current.AccentColor);
                            BlinkButton(btDbCancel, ThemeManager.Current.AccentColor);
                            ActiveControl = btDbSave;
                        }
                    } else if (mode != _currentMode && mode == ViewMode.Select)
                        ActiveControl = _currentMode == ViewMode.Edit ? btEdit : btDbEdit;

                    // save current mode
                    _currentMode = mode;
                });
            }
        }

        private void UpdateDownloadButton() {
            this.SafeInvoke(form => {
                // download database information
                if (DataBase.Instance.IsDbInfoAvailable) {
                    btDbDownload.BackGrndImage = ImageResources.DownloadDbOk;
                    btDbDeleteDownload.Enabled = true;
                    btDbView.Enabled = true;
                    toolTip.SetToolTip(btDbDownload, "<i>The database information for this environment are available and in use in the autocompletion list</i><br><br>Click this button to <b>force a refresh of the database information</b> for this environment");
                } else {
                    btDbDownload.BackGrndImage = ImageResources.DownloadDbNok;
                    btDbDeleteDownload.Enabled = false;
                    btDbView.Enabled = false;
                    toolTip.SetToolTip(btDbDownload, "<i>No information available for this database!</i><br><br>Click this button to <b>fetch the database information</b> for this environment,<br>they will be used in the autocompletion list to suggest database names,<br>table names and field names.<br><br>By default, the autocompletion list uses the last environment <br>selected where database info were available.");
                }
            });
        }

        #endregion

        #region Events

        #region misc

        private void BtDownloadOnButtonPressed(object sender, EventArgs e) {
            DataBase.Instance.FetchCurrentDbInfo(UpdateDownloadButton, DataBase.Instance.GetCurrentDumpPath);
        }

        private void BtDeleteDownloadOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            DataBase.Instance.DeleteCurrentDbInfo();
            UpdateDownloadButton();
        }

        private void BtDbViewOnButtonPressed(object sender, EventArgs eventArgs) {
            Npp.OpenFile(DataBase.Instance.GetCurrentDumpPath);
        }

        private void TgCompLocallyOnCheckedChanged(object sender, EventArgs eventArgs) {
            ProEnvironment.Current.CompileLocally = tgCompLocally.Checked;
            ProEnvironment.SaveList();
        }

        #endregion

        #region Env management

        private void BtAddOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            btAdd.UseCustomBackColor = false;
            ToggleMode(ViewMode.Add);
        }

        private void BtduplicateOnButtonPressed(object sender, EventArgs eventArgs) {
            ToggleMode(ViewMode.Copy);
        }

        private void BtModifyOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            ToggleMode(ViewMode.Edit);
        }

        private void BtCancelOnButtonPressed(object sender, EventArgs eventArgs) {
            ToggleMode(ViewMode.Select);
        }

        private void BtSaveOnButtonPressed(object sender, EventArgs eventArgs) {
            if (Save())
                ToggleMode(ViewMode.Select);
        }

        private void BtDeleteOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            var answ = _unsafeDelete ? 0 : UserCommunication.Message("Do you really want to delete the current environment?", MessageImg.MsgQuestion, "Delete", "Confirmation", new List<string> {"Yes I do", "Yes don't ask again", "No, Cancel"});
            if (answ == 0 || answ == 1) {
                if (answ == 1)
                    _unsafeDelete = true;
                _currentMode = ViewMode.Delete;
                if (Save())
                    ToggleMode(ViewMode.Select);
            }
        }

        #endregion

        #region db management

        private void BtDbCancelOnButtonPressed(object sender, EventArgs eventArgs) {
            ToggleMode(ViewMode.Select);
        }

        private void BtDbSaveOnButtonPressed(object sender, EventArgs eventArgs) {
            if (Save())
                ToggleMode(ViewMode.Select);
        }

        private void BtDbDeleteOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            var answ = _unsafeDelete ? 0 : UserCommunication.Message("Do you really want to delete the current database info?", MessageImg.MsgQuestion, "Delete", "Confirmation", new List<string> {"Yes I do", "Yes don't ask again", "No, Cancel"});
            if (answ == 0 || answ == 1) {
                if (answ == 1)
                    _unsafeDelete = true;
                _currentMode = ViewMode.DbDelete;
                if (Save())
                    ToggleMode(ViewMode.Select);
            }
        }

        private void BtDbEditOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            ToggleMode(ViewMode.DbEdit);
        }

        private void BtDbAddOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            ToggleMode(ViewMode.DbAdd);
        }

        #endregion

        #region combo-boxes

        /// <summary>
        /// when changing appli
        /// </summary>
        /// <param name="sender"></param>
        private void cbName_SelectedIndexChanged(YamuiComboBox sender) {
            if (Config.Instance.EnvName.Equals(cbName.SelectedItem.ToString()))
                return;
            ProEnvironment.SetCurrent(cbName.SelectedItem.ToString(), null, null);
            ToggleMode(ViewMode.Select);
        }

        /// <summary>
        /// when changing env letter
        /// </summary>
        /// <param name="sender"></param>
        private void cbSuffix_SelectedIndexChanged(YamuiComboBox sender) {
            if (Config.Instance.EnvSuffix.Equals(cbSuffix.SelectedItem.ToString()))
                return;
            ProEnvironment.SetCurrent(null, cbSuffix.SelectedItem.ToString(), null);
            ToggleMode(ViewMode.Select);
        }

        /// <summary>
        /// when changing database
        /// </summary>
        /// <param name="sender"></param>
        private void cbDatabase_SelectedIndexChanged(YamuiComboBox sender) {
            if (ProEnvironment.Current.GetCurrentDb().Equals(cbDatabase.SelectedItem.ToString()))
                return;
            ProEnvironment.SetCurrent(null, null, cbDatabase.SelectedItem.ToString());
            textbox1.Text = ProEnvironment.Current.GetPfPath();
            ProEnvironment.SaveList();
        }

        #endregion

        #region common buttons

        /// <summary>
        /// Select a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buttonPressedEventArgs"></param>
        private void BtleftOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            var associatedTextBox = GetTextBoxByName(((Control) sender).Name);
            if (associatedTextBox == null)
                return;
            string tag = (string) (associatedTextBox.Tag ?? string.Empty);
            var selectedStuff = tag.Equals("true") ? Utils.ShowFolderSelection(associatedTextBox.Text) : Utils.ShowFileSelection(associatedTextBox.Text, tag);
            if (!string.IsNullOrEmpty(selectedStuff)) {
                associatedTextBox.Text = selectedStuff;
                BlinkTextBox(associatedTextBox, ThemeManager.Current.AccentColor);
            }
        }

        /// <summary>
        /// Open folder or open in npp
        /// </summary>
        private void OpenFileOnButtonPressed(object sender, EventArgs eventArgs) {
            var associatedTextBox = GetTextBoxByName(((Control) sender).Name);
            if (associatedTextBox == null)
                return;
            string tag = (string) (associatedTextBox.Tag ?? string.Empty);

            var ext = Path.GetExtension(associatedTextBox.Text) ?? "";
            var hasOpened = tag.Equals("true") ? Utils.OpenFolder(associatedTextBox.Text) : (!ext.Equals(".exe") ? Npp.OpenFile(associatedTextBox.Text) : Utils.OpenFileInFolder(associatedTextBox.Text));
            if (!hasOpened)
                BlinkTextBox(associatedTextBox, ThemeManager.Current.GenericErrorColor);
        }

        /// <summary>
        /// open folder or file in folder
        /// </summary>
        private void OpenFileOnMouseDown(object sender, MouseEventArgs e) {
            var associatedTextBox = GetTextBoxByName(((Control) sender).Name);
            if (associatedTextBox == null)
                return;
            string tag = (string) (associatedTextBox.Tag ?? string.Empty);

            if (e.Button == MouseButtons.Right) {
                var hasOpened = tag.Equals("true") ? Utils.OpenFolder(associatedTextBox.Text) : Utils.OpenFileInFolder(associatedTextBox.Text);
                if (!hasOpened)
                    BlinkTextBox(associatedTextBox, ThemeManager.Current.GenericErrorColor);
            }
        }

        #endregion

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
            Transition.run(textBox, "CustomBackColor", ThemeManager.Current.ButtonNormalBack, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { textBox.UseCustomBackColor = false; });
        }

        private void BlinkButton(YamuiButton button, Color blinkColor) {
            button.UseCustomBackColor = true;
            Transition.run(button, "BackColor", ThemeManager.Current.ButtonNormalBack, blinkColor, new TransitionType_Flash(1, 300), (o, args) => { button.UseCustomBackColor = false; });
        }

        /// <summary>
        /// false to disable all textboxes of the form, true to enable
        /// </summary>
        private void EnableAllTextBoxes(bool newStatus) {
            foreach (var control in scrollPanel.ContentPanel.Controls) {
                if (control is YamuiTextBox)
                    ((YamuiTextBox) control).Enabled = newStatus;
            }
        }

        #endregion
        
    }
}