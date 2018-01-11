#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (DoDeployPage.cs) is part of 3P.
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
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures.Pro;
using _3PA.MainFeatures.Pro.Deploy;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Appli.Pages.Actions {
    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class DoDeployPage : YamuiPage {

        #region fields

        // Timer that ticks every seconds to update the progress bar
        private Timer _progressTimer;

        // proenv copied when clicking on the start button
        private DeploymentHandler _proDeployment;

        private string _reportExportPath;

        // true if the interface has been shown at least once
        private bool _shownOnce;

        #endregion

        #region constructor

        public DoDeployPage() {
            InitializeComponent();

            // browse
            btBrowse.BackGrndImage = ImageResources.SelectFile;
            btBrowse.ButtonPressed += BtBrowseOnButtonPressed;
            tooltip.SetToolTip(btBrowse, "Click to <b>select</b> a folder");

            // open
            btOpen.BackGrndImage = ImageResources.OpenInExplorer;
            btOpen.ButtonPressed += BtOpenOnButtonPressed;
            tooltip.SetToolTip(btOpen, "Click to <b>open</b> this folder in the explorer");

            // historic
            btHistoric.BackGrndImage = ImageResources.Historic;
            btHistoric.ButtonPressed += BtHistoricOnButtonPressed;
            tooltip.SetToolTip(btHistoric, "Click to <b>browse</b> the previous folders");
            if (string.IsNullOrEmpty(Config.Instance.CompileDirectoriesHistoric))
                btHistoric.Visible = false;

            tooltip.SetToolTip(toggleRecurs, "Toggle this option on to explore recursively the selected folder<br>Toggle off and you will only compile/deploy the files directly under the selected folder");
            tooltip.SetToolTip(toggleAutoUpdateSourceDir, "Automatically update the above directory when you switch environment<br>(it then takes the source directory of the environment)");
            tooltip.SetToolTip(toggleMono, "Toggle on to only use a single process when compiling during the deployment<br>Obviously, this will slow down the process by a lot!<br>The only reason to use this option is if you want to limit the number of connections made to your database during compilation...");
            tooltip.SetToolTip(toggleOnlyGenerateRcode, "Override the environment settings and only generated R-code during the deployement<br>(i.e. dont deploy debug-list or xref)");
            tooltip.SetToolTip(fl_nbProcess, "This parameter is used when compiling multiple files, it determines how many<br>Prowin processes can be started to handle compilation<br>The total number of processes started is actually multiplied by your number of cores<br><br>Be aware that as you increase the number or processes for the compilation, you<br>decrease the potential time of compilation but you also increase the number of connection<br>needed to your database (if you have one defined!)<br>You might have an error on certain processes that can't connect to the database<br>if you try to increase this number too much<br><br><i>This value can't be superior to 15</i>");

            // Open hook
            btOpenHook.BackGrndImage = ImageResources.Edit;
            btOpenHook.ButtonPressed += (sender, args) => {
                if (!File.Exists(Config.FileDeploymentHook))
                    Utils.FileWriteAllBytes(Config.FileDeploymentHook, DataResources.DeploymentHook);
                Npp.OpenFile(Config.FileDeploymentHook);
            };

            // diretory from env
            btUndo.BackGrndImage = ImageResources.UndoUserAction;
            btUndo.ButtonPressed += BtUndoOnButtonPressed;
            tooltip.SetToolTip(btUndo, "Click to <b>select</b> the base local path (your source directory)<br>for the current environment");

            // start
            btStart.BackGrndImage = ImageResources.Deploy;
            tooltip.SetToolTip(btStart, "Click to <b>start</b> deploying your application");
            btStart.ButtonPressed += BtStartOnButtonPressed;

            // Test
            btTest.BackGrndImage = ImageResources.Tests;
            tooltip.SetToolTip(btTest, "Click to <b>test</b> the deployment your application (this is safe)");
            btTest.ButtonPressed += BtTestOnButtonPressed;

            // cancel
            btCancel.BackGrndImage = ImageResources.Cancel;
            tooltip.SetToolTip(btCancel, "Click to <b>cancel</b> the current deployement");
            btCancel.ButtonPressed += BtCancelOnButtonPressed;
            btCancel.Visible = false;

            // progress bar
            progressBar.Style = ProgressStyle.Normal;
            progressBar.CenterText = CenterElement.Text;
            progressBar.Visible = false;

            // report
            btReport.BackGrndImage = ImageResources.Report;
            btReport.Visible = false;
            btReport.ButtonPressed += BtReportOnButtonPressed;
            lbl_report.Visible = false;
            lbl_report.LinkClicked += Utils.OpenPathClickHandler;

            // reset
            btReset.BackGrndImage = ImageResources.Default;
            tooltip.SetToolTip(btReset, "Click to reset the options to their default values");
            btReset.ButtonPressed += (sender, args) => { ResetFields(); };

            // help kink
            linkurl.Text = @"<img src='Help'><a href='" + Config.UrlHelpDeploy + @"'>Learn more about this feature?</a>";

            // switch link
            lblCurEnv.LinkClicked += (sender, args) => { AppliMenu.ShowEnvMenu(true); };

            // save
            tooltip.SetToolTip(btSave, "Save the settings for the currently selected profile");
            btSave.BackGrndImage = ImageResources.Save;
            btSave.ButtonPressed += (sender, args) => {
                if (string.IsNullOrEmpty(DeploymentProfile.Current.Name) && !ChooseName())
                    return;
                SetDataFromFields();
                SaveProfilesList();
            };

            // save as...
            tooltip.SetToolTip(btSaveAs, "Save the settings in a new profile that you will name");
            btSaveAs.BackGrndImage = ImageResources.Save;
            btSaveAs.ButtonPressed += (sender, args) => {
                var cur = DeploymentProfile.Current;
                DeploymentProfile.List.Add(new DeploymentProfile());
                DeploymentProfile.Current = DeploymentProfile.List.Last();
                if (ChooseName()) {
                    SetDataFromFields();
                    SaveProfilesList();
                } else {
                    DeploymentProfile.List.RemoveAt(DeploymentProfile.List.Count - 1);
                    DeploymentProfile.Current = cur;
                }
                btDelete.Visible = DeploymentProfile.List.Count > 1;
            };

            // delete
            tooltip.SetToolTip(btDelete, "Delete the current profile");
            btDelete.BackGrndImage = ImageResources.Delete;
            btDelete.ButtonPressed += (sender, args) => {
                if (UserCommunication.Message("Do you really want to delete this profile?", MessageImg.MsgQuestion, "Delete", "Deployment profile", new List<string> {"Yes", "Cancel"}) == 0) {
                    DeploymentProfile.List.Remove(DeploymentProfile.Current);
                    DeploymentProfile.Current = null;
                    SaveProfilesList();
                    SetFieldsFromData();
                    if (DeploymentProfile.List.Count == 1)
                        btDelete.Hide();
                }
            };

            // cb
            tooltip.SetToolTip(cbName, "Browse and select the available profiles, each profile hold deployment settings");
            cbName.SelectedIndexChangedByUser += CbNameOnSelectedIndexChanged;

            // modify rules
            tooltip.SetToolTip(btRules, "Click to modify the rules");
            btRules.BackGrndImage = ImageResources.Rules;
            btRules.ButtonPressed += (sender, args) => DeploymentRules.EditRules();

            // view rules
            tooltip.SetToolTip(btRules, "Click to view the rules filtered for the current environment<br><i>The rules are also sorted!</i>");
            btSeeRules.BackGrndImage = ImageResources.ViewFile;
            btSeeRules.ButtonPressed += (sender, args) => { UserCommunication.Message(DeploymentRules.BuildHtmlTableForRules(ProEnvironment.Current.Deployer.DeployRules), MessageImg.MsgInfo, "List of deployment rules", "Sorted and filtered for the current environment"); };

            DeploymentProfile.OnDeployProfilesUpdate += () => {
                UpdateCombo();
                SetFieldsFromData();
                btDelete.Visible = DeploymentProfile.List.Count > 1;
            };

            // subscribe
            ProEnvironment.OnEnvironmentChange += OnShow;
            DeploymentRules.OnDeployConfigurationUpdate += OnShow;

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
        }

        #endregion

        #region on show

        public override void OnShow() {
            // update combo and fields
            if (!_shownOnce) {
                UpdateCombo();
                SetFieldsFromData();
                _shownOnce = true;
            }

            // hide delete if needed
            btDelete.Visible = DeploymentProfile.List.Count > 1;

            // cur env
            lblCurEnv.Text = string.Format("{0} <a href='#'>(switch)</a>", ProEnvironment.Current.Name + (!string.IsNullOrEmpty(ProEnvironment.Current.Suffix) ? " - " + ProEnvironment.Current.Suffix : ""));
            lbl_deployDir.Text = @"The deployment directory is " + ProEnvironment.Current.BaseCompilationPath.ToHtmlLink();

            // update the rules for the current env
            var currentDeployer = ProEnvironment.Current.Deployer;
            lbl_rules.Text = string.Format("There are <b>{0}</b> rules for the compilation (step 0), <b>{1}</b> rules for step 1, <b>{2}</b> rules for step 2 and <b>{3}</b> rules beyond", currentDeployer.DeployRules.Count(rule => rule.Step == 0), currentDeployer.DeployRules.Count(rule => rule.Step == 1), currentDeployer.DeployRules.Count(rule => rule.Step == 2), currentDeployer.DeployRules.Count(rule => rule.Step >= 3));

            if (DeploymentProfile.Current.AutoUpdateSourceDir)
                fl_directory.Text = ProEnvironment.Current.BaseLocalPath;
        }

        #endregion

        #region events

        private void BtTestOnButtonPressed(object sender, EventArgs e) {
            BtStartOnButtonPressed(null, e);
        }

        /// <summary>
        /// Start the deployment!
        /// </summary>
        private void BtStartOnButtonPressed(object sender, EventArgs eventArgs) {

            bool isTest = sender == null;

            SetDataFromFields();
            SaveProfilesList();

            if (string.IsNullOrEmpty(DeploymentProfile.Current.SourceDirectory) || !Directory.Exists(DeploymentProfile.Current.SourceDirectory)) {
                BlinkTextBox(fl_directory, ThemeManager.Current.GenericErrorColor);
                return;
            }

            // init screen
            btStart.Visible = false;
            btTest.Visible = false;
            btReset.Visible = false;
            progressBar.Visible = true;
            progressBar.Progress = 0;
            progressBar.Text = @"Please wait, the deployment is starting...";
            btReport.Visible = false;
            lbl_report.Visible = false;
            _reportExportPath = null;

            // start the deployment
            Task.Factory.StartNew(() => {
                _proDeployment = new DeploymentHandler(ProEnvironment.Current, DeploymentProfile.Current) {
                    IsTestMode = isTest,
                    OnExecutionEnd = OnCompilationEnd,
                    IsAnalysisMode = false
                };

                UpdateProgressBar();
                btCancel.SafeInvoke(button => button.Visible = true);

                if (_proDeployment.Start()) {
                    this.SafeInvoke(page => {
                        // start a recurrent event (every second) to update the progression of the compilation
                        _progressTimer = new Timer();
                        _progressTimer.Interval = 500;
                        _progressTimer.Tick += (o, args) => UpdateProgressBar();
                        _progressTimer.Start();
                    });
                } else {
                    // nothing started
                    ResetScreen();
                }

            });
        }

        // called when the compilation ended
        private void OnCompilationEnd(DeploymentHandler proDeployment) {
            Task.Factory.StartNew(() => {
                this.SafeInvoke(page => {
                    // get rid of the timer
                    if (_progressTimer != null) {
                        _progressTimer.Stop();
                        _progressTimer.Dispose();
                        _progressTimer = null;
                    }
                    ResetScreen();
                    UpdateReport(_proDeployment.FormatDeploymentReport());
                    btReport.Visible = true;

                    // notify the user
                    if (!_proDeployment.HasBeenCancelled) {
                        UserCommunication.NotifyUnique("ReportAvailable", "The requested deployment is over,<br>please check the generated report to see the result :<br><br><a href= '#'>Click here to see the report</a>", MessageImg.MsgInfo, "Deploy your application", "Report available", args => {
                            Appli.GoToPage(PageNames.MassCompiler);
                            UserCommunication.CloseUniqueNotif("ReportAvailable");
                        }, Appli.IsFocused() ? 10 : 0);
                    }
                });
            });
        }

        /// <summary>
        /// Cancel the current compilation
        /// </summary>
        private void BtCancelOnButtonPressed(object sender, EventArgs eventArgs) {
            btCancel.Visible = false;
            _proDeployment.Cancel();
        }

        /// <summary>
        /// The historic button shows a menu that allows the user to select a previously selected folders
        /// </summary>
        private void BtHistoricOnButtonPressed(object sender, EventArgs eventArgs) {
            List<YamuiMenuItem> itemList = new List<YamuiMenuItem>();
            foreach (var path in Config.Instance.CompileDirectoriesHistoric.Split(',')) {
                if (!string.IsNullOrEmpty(path)) {
                    itemList.Add(new YamuiMenuItem {
                        ItemImage = ImageResources.ExtFolder, DisplayText = path, OnClic = item => {
                            if (IsHandleCreated) {
                                BeginInvoke((Action) delegate {
                                    fl_directory.Text = path;
                                    SaveHistoric();
                                });
                            }
                        }
                    });
                }
            }
            if (itemList.Count > 0) {
                var menu = new YamuiWaterfallMenu(Cursor.Position, itemList);
                menu.Show();
            }
        }

        private void BtOpenOnButtonPressed(object sender, EventArgs eventArgs) {
            var hasOpened = Utils.OpenFolder(fl_directory.Text);
            if (!hasOpened)
                BlinkTextBox(fl_directory, ThemeManager.Current.GenericErrorColor);
        }

        private void BtBrowseOnButtonPressed(object sender, EventArgs eventArgs) {
            var selectedStuff = Utils.ShowFolderSelection(fl_directory.Text);
            if (!string.IsNullOrEmpty(selectedStuff)) {
                fl_directory.Text = selectedStuff;
                BlinkTextBox(fl_directory, ThemeManager.Current.AccentColor);
                SaveHistoric();
            }
        }

        private void BtUndoOnButtonPressed(object sender, EventArgs eventArgs) {
            fl_directory.Text = ProEnvironment.Current.BaseLocalPath;
        }

        private void BtReportOnButtonPressed(object sender, EventArgs eventArgs) {

            // report already generated
            if (!string.IsNullOrEmpty(_reportExportPath)) {
                Utils.OpenAnyLink(_reportExportPath);
                return;
            }

            var reportDir = Path.Combine(Config.FolderTemp, "Export_html", DateTime.Now.ToString("yyMMdd_HHmmssfff"));
            if (!Utils.CreateDirectory(reportDir))
                return;
            _reportExportPath = Path.Combine(reportDir, "index.html");

            _proDeployment.ExportReport(_reportExportPath);

            // open it
            Utils.OpenAnyLink(_reportExportPath);
        }

        private void CbNameOnSelectedIndexChanged(YamuiComboBox sender) {
            DeploymentProfile.Current = DeploymentProfile.List[cbName.SelectedIndex];
            Config.Instance.CurrentDeployProfile = DeploymentProfile.Current.Name;
            SetFieldsFromData();
        }

        #endregion

        #region private methods
        
        // allows to update the progression bar
        private void UpdateProgressBar() {
            progressBar.SafeInvoke(bar => {
                if (_proDeployment.OverallProgressionPercentage < 0.1)
                    bar.Text = @"Initializing, please wait...";
                else
                    bar.Text = Math.Round((decimal) (_proDeployment.OverallProgressionPercentage / _proDeployment.TotalNumberOfOperations), 1) + @"% / " + _proDeployment.CurrentOperationName + @" / total time " + _proDeployment.ElapsedTime;
                bar.Progress = _proDeployment.OverallProgressionPercentage / _proDeployment.TotalNumberOfOperations;
            });
        }

        // update the report, activates the scroll bars when needed
        private void UpdateReport(string text) {
            this.SafeInvoke(page => {
                // ensure it's visible 
                lbl_report.Visible = true;

                lbl_report.Text = text;

                // Activate scrollbars if needed
                scrollPanel.ContentPanel.Height = lbl_report.Location.Y + lbl_report.Height + 20;
                Height = lbl_report.Location.Y + lbl_report.Height + 20;
            });
        }

        private void ResetScreen() {
            this.SafeInvoke(page => {
                btStart.Visible = true;
                btTest.Visible = true;
                btReset.Visible = true;
                btCancel.Visible = false;
                progressBar.Visible = false;
            });
        }

        private void SaveHistoric() {
            // we save the directory in the historic
            if (!string.IsNullOrEmpty(fl_directory.Text) && Directory.Exists(fl_directory.Text)) {
                var list = Config.Instance.CompileDirectoriesHistoric.Split(',').ToList();
                if (list.Exists(s => s.Equals(fl_directory.Text)))
                    list.Remove(fl_directory.Text);
                list.Insert(0, fl_directory.Text);
                if (list.Count > 2)
                    list.RemoveAt(list.Count - 1);
                Config.Instance.CompileDirectoriesHistoric = string.Join(",", list);
                btHistoric.Visible = true;
            }
        }

        private bool ChooseName() {
            object name = string.Empty;
            if (UserCommunication.Input(ref name, "", MessageImg.MsgQuestion, "Save profile as...", "Enter a name for this profile") == 1 ||
                string.IsNullOrEmpty((string) name))
                return false;
            DeploymentProfile.Current.Name = (string) name;
            return true;
        }

        private void SaveProfilesList() {
            DeploymentProfile.Save();
            Config.Instance.CurrentDeployProfile = DeploymentProfile.Current.Name;
            UpdateCombo();
        }

        private void SetFieldsFromData() {
            fl_directory.Text = DeploymentProfile.Current.SourceDirectory;
            toggleRecurs.Checked = DeploymentProfile.Current.ExploreRecursively;
            toggleAutoUpdateSourceDir.Checked = DeploymentProfile.Current.AutoUpdateSourceDir;
            toggleMono.Checked = DeploymentProfile.Current.ForceSingleProcess;
            toggleOnlyGenerateRcode.Checked = DeploymentProfile.Current.OnlyGenerateRcode;
            fl_nbProcess.Text = DeploymentProfile.Current.NumberProcessPerCore.ToString();
        }

        private void SetDataFromFields() {
            DeploymentProfile.Current.SourceDirectory = fl_directory.Text;
            DeploymentProfile.Current.ExploreRecursively = toggleRecurs.Checked;
            DeploymentProfile.Current.AutoUpdateSourceDir = toggleAutoUpdateSourceDir.Checked;
            DeploymentProfile.Current.ForceSingleProcess = toggleMono.Checked;
            DeploymentProfile.Current.OnlyGenerateRcode = toggleOnlyGenerateRcode.Checked;
            if (!int.TryParse(fl_nbProcess.Text, out DeploymentProfile.Current.NumberProcessPerCore))
                DeploymentProfile.Current.NumberProcessPerCore = 1;
            DeploymentProfile.Current.NumberProcessPerCore.Clamp(1, 15);
        }

        private void UpdateCombo() {
            cbName.DataSource = DeploymentProfile.List.Select(profile => profile.Name).ToList();
            if (DeploymentProfile.List.Exists(profile => profile.Name.Equals(Config.Instance.CurrentDeployProfile)))
                cbName.SelectedText = Config.Instance.CurrentDeployProfile;
            else
                cbName.SelectedIndex = 0;
        }

        private void ResetFields() {
            DeploymentProfile.Current = new DeploymentProfile();
            SetFieldsFromData();
            DeploymentProfile.Current = null;
        }

        /// <summary>
        /// Makes the given textbox blink
        /// </summary>
        private void BlinkTextBox(YamuiTextBox textBox, Color blinkColor) {
            textBox.UseCustomBackColor = true;
            Transition.run(textBox, "CustomBackColor", ThemeManager.Current.ButtonNormalBack, blinkColor, new TransitionType_Flash(3, 300), (o, args) => { textBox.UseCustomBackColor = false; });
        }

        #endregion
    }
}