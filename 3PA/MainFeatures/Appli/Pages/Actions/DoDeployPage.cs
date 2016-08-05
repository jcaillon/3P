#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompilePage.cs) is part of 3P.
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using YamuiFramework.Helper;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.Pro;

namespace _3PA.MainFeatures.Appli.Pages.Actions {

    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class DoDeployPage : YamuiPage {

        #region fields

        // Timer that ticks every seconds to update the progress bar
        private Timer _progressTimer;

        // Stores the current compilation info
        private ProCompilation _currentCompil;

        private string _reportExportPath;

        private List<DeployProfile> ListProfiles {
            get {
                if (_listConfig == null) {
                    if (File.Exists(Config.FileDeployProfiles)) {
                        _listConfig = new List<DeployProfile>();
                        try {
                            Object2Xml<DeployProfile>.LoadFromFile(_listConfig, Config.FileDeployProfiles);
                        } catch (Exception e) {
                            ErrorHandler.ShowErrors(e, "Error when loading settings", Config.FileDeployProfiles);
                        }
                    }
                    if (_listConfig == null || _listConfig.Count == 0)
                        _listConfig = new List<DeployProfile> { new DeployProfile() };
                }
                return _listConfig;
            }
            set { _listConfig = value; }
        }

        public DeployProfile CurrentProfile {
            get {
                if (_currentProfile == null) {
                    _currentProfile = ListProfiles.FirstOrDefault(profile => profile.Name.Equals(Config.Instance.CurrentDeployProfile));
                    if (_currentProfile == null)
                        _currentProfile = ListProfiles.First();
                }
                return _currentProfile;
            }
            set { _currentProfile = value; }
        }

        private List<DeployProfile> _listConfig;

        private DeployProfile _currentProfile;

        #endregion

        #region constructor
        public DoDeployPage() {

            InitializeComponent();

            // browse
            btBrowse.BackGrndImage = ImageResources.SelectFile;
            btBrowse.ButtonPressed += BtBrowseOnButtonPressed;
            tooltip.SetToolTip(btBrowse, "Click to <b>select</b> a folder");

            btOpen.BackGrndImage = ImageResources.OpenInExplorer;
            btOpen.ButtonPressed += BtOpenOnButtonPressed;
            tooltip.SetToolTip(btOpen, "Click to <b>open</b> this folder in the explorer");

            btHistoric.BackGrndImage = ImageResources.Historic;
            btHistoric.ButtonPressed += BtHistoricOnButtonPressed;
            tooltip.SetToolTip(btHistoric, "Click to <b>browse</b> the previous folders");
            if (string.IsNullOrEmpty(Config.Instance.CompileDirectoriesHistoric))
                btHistoric.Visible = false;

            btUndo.BackGrndImage = ImageResources.UndoUserAction;
            btUndo.ButtonPressed += BtUndoOnButtonPressed;
            tooltip.SetToolTip(btUndo, "Click to <b>select</b> the base local path (your source directory)<br>for the current environment");

            // compilation
            tooltip.SetToolTip(btStart, "Click to <b>start</b> deploying your application :<br>First step, compile all the progress files<br>Second step, deploy r-code following the deployment rules for compilation<br>Third step, deploy any file following the deployment rules for files");
            btStart.ButtonPressed += BtStartOnButtonPressed;

            // cancel
            tooltip.SetToolTip(btCancel, "Click to <b>cancel</b> the current deployement");
            btCancel.ButtonPressed += BtCancelOnButtonPressed;
            btCancel.Visible = false;

            // progress bar
            progressBar.Style = ProgressStyle.Normal;
            progressBar.CenterText = CenterElement.Text;
            progressBar.Visible = false;

            // report
            bt_export.Visible = false;
            bt_export.ButtonPressed += BtExportOnButtonPressed;
            lbl_report.Visible = false;
            lbl_report.LinkClicked += Utils.OpenPathClickHandler;


            tooltip.SetToolTip(toggleRecurs, "Toggle this option on to explore recursively the selected folder<br>Toggle off and you will only compile/deploy the files directly under the selected folder");
            tooltip.SetToolTip(title, "Automatically update the source directory for the deployement from the environment source directory");
            tooltip.SetToolTip(toggleMono, "Toggle on to only use a single process when compiling multiple files through the mass compiler page<br>Obviously, this will slow down the process by a lot!<br>The only reason to use this option is if you want to limit the number of connections made to your database during compilation...");
            tooltip.SetToolTip(fl_nbProcess, "This parameter is used when compiling multiple files, it determines how many<br>Prowin processes can be started to handle compilation<br>The total number of processes started is actually multiplied by your number of cores<br><br>Be aware that as you increase the number or processes for the compilation, you<br>decrease the potential time of compilation but you also increase the number of connection<br>needed to your database (if you have one defined!)<br>You might have an error on certain processes that can't connect to the database<br>if you try to increase this number too much<br><br><i>This value can't be superior to 15</i>");
            tooltip.SetToolTip(fl_include, "<i>Leave empty to not apply this filter</i><br>A comma (,) separated list of filters to apply on each <u>full path</u> of the<br>files found in the selected folder<br>If the path matches one of the filter, the file is <b>kept</b> for the compilation, otherwise it is not<br><br>You can use the wildcards * and ? for your filters!<br>* matches any character 0 or more times<br>? matches any character 1 time exactly<br><br>Example of filter :<div class='ToolTipcodeSnippet'>*foo*.cls,*\\my_sub_directory\\*,*proc_???.p</div>");
            tooltip.SetToolTip(fl_exclude, "<i>Leave empty to not apply this filter</i><br>A comma (,) separated list of filters to apply on each <u>full path</u> of the<br>files found in the selected folder<br>If the path matches one of the filter, the file is <b>excluded</b> for the compilation, otherwise it is not<br><br>You can use the wildcards * and ? for your filters!<br>* matches any character 0 or more times<br>? matches any character 1 time exactly<br><br>Example of filter :<div class='ToolTipcodeSnippet'>*foo*.cls,\\*my_sub_directory\\*,*proc_???.p</div>");

            // reset
            tooltip.SetToolTip(btReset, "Click to reset the options to their default values");
            btReset.ButtonPressed += (sender, args) => { ResetFields(); };

            // help kink
            linkurl.Text = @"<img src='Help'><a href='" + Config.UrlHelpMassCompiler + @"'>Learn more about this feature?</a>";

            // switch link
            lblCurEnv.LinkClicked += (sender, args) => {
                AppliMenu.ShowEnvMenuAtCursor();
            };

            // save
            btSave.BackGrndImage = ImageResources.Save;
            btSave.ButtonPressed += (sender, args) => {
                if (string.IsNullOrEmpty(CurrentProfile.Name) && !ChooseName())
                    return;
                SetDataFromFields();
                SaveProfilesList();
            };

            // save as...
            btSaveAs.BackGrndImage = ImageResources.Save;
            btSaveAs.ButtonPressed += (sender, args) => {
                var _cur = CurrentProfile;
                ListProfiles.Add(new DeployProfile());
                CurrentProfile = ListProfiles.Last();
                if (ChooseName()) {
                    SetDataFromFields();
                    SaveProfilesList();
                } else {
                    ListProfiles.RemoveAt(ListProfiles.Count - 1);
                    CurrentProfile = _cur;
                }
            };

            // delete
            btDelete.BackGrndImage = ImageResources.Delete;
            btDelete.ButtonPressed += (sender, args) => {
                if (UserCommunication.Message("Do you really want to delete this profile?", MessageImg.MsgQuestion, "Delete", "Deployment profile", new List<string> {"Yes", "Cancel"}) == 0) {
                    if (ListProfiles.Count == 1)
                        ResetFields();
                    else {
                        if (ListProfiles.Count > 1)
                            ListProfiles.Remove(CurrentProfile);
                        else
                            ListProfiles = new List<DeployProfile> { new DeployProfile() };
                        CurrentProfile = null;
                        SaveProfilesList();
                        SetFieldsFromData();
                    }
                }
            };

            // cb
            cbName.SelectedIndexChanged += CbNameOnSelectedIndexChanged;

            // modify rules
            btRules.BackGrndImage = ImageResources.Rules;
            btRules.ButtonPressed += (sender, args) => Appli.GoToPage(PageNames.DeploymentRules);
            
            // view rules
            btSeeRules.BackGrndImage = ImageResources.ViewFile;

            // subscribe to env update
            ProEnvironment.OnEnvironmentChange += UpdateMassCompilerBaseDirectory;

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
        }

        #endregion

        #region on show

        public override void OnShow() {

            // cur env
            lblCurEnv.Text = string.Format("{0} <a href='#'>(switch)</a>", ProEnvironment.Current.Name + (!string.IsNullOrEmpty(ProEnvironment.Current.Suffix) ? " - " + ProEnvironment.Current.Suffix : ""));

            // update the rules for the current env
            lbl_rules.Text = string.Format("<b>{0}</b> rules for step 1, <b>{1}</b> rules for step, <b>{2}</b> rules for further steps", ProEnvironment.Current.GetDeployRulesList.Count(rule => rule.Step == 0), ProEnvironment.Current.GetDeployRulesList.Count(rule => rule.Step == 1), ProEnvironment.Current.GetDeployRulesList.Count(rule => rule.Step > 1));

            // update combo and fields
            if (_currentProfile == null) {
                UpdateCombo();
                SetFieldsFromData();
            }
        }

        public override void OnHide() {
        }

        #endregion
        
        #region Build the report

        private void BuildReport() {

            StringBuilder currentReport = new StringBuilder();

            currentReport.Append(@"<h2>Results :</h2>");

            // the execution ended successfully
            if (_currentCompil.NumberOfProcesses == _currentCompil.NumberOfProcessesEndedOk) {
                currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='MsgOk' height='15px'>All the processes ended correctly</div>");

                // compilation time
                currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Time' height='15px'>Total elapsed time for the compilation : <b>" + _currentCompil.ExecutionTime + @"</b></div>");

                var listLines = new List<Tuple<int, string>>();

                StringBuilder line = new StringBuilder();

                var nbFailed = 0;
                var nbWarning = 0;

                foreach (var fileToCompile in _currentCompil.GetListOfFileToCompile.OrderBy(compile => Path.GetFileName(compile.InputPath))) {

                    var toCompile = fileToCompile;

                    bool moveFail = _currentCompil.TransferedFiles.Exists(move => move.Origin.Equals(fileToCompile.InputPath) && !move.IsOk);
                    var errorsOfTheFile = _currentCompil.ErrorsList.Where(error => error.CompiledFilePath.Equals(toCompile.InputPath)).ToList();
                    bool hasError = errorsOfTheFile.Count > 0 && errorsOfTheFile.Exists(error => error.Level > ErrorLevel.StrongWarning);
                    bool hasWarning = errorsOfTheFile.Count > 0 && errorsOfTheFile.Exists(error => error.Level <= ErrorLevel.StrongWarning);

                    line.Clear();

                    line.Append("<tr><td style='width: 50px; padding-bottom: 15px;'><img src='" + (moveFail || hasError ? "MsgError" : (hasWarning ? "MsgWarning" : "MsgOk")) + "' width='30' height='30' /></td><td %ALTERNATE%style='padding-bottom: 10px;'>");

                    line.Append(ProCompilation.FormatCompilationResult(fileToCompile, errorsOfTheFile, _currentCompil.TransferedFiles.Where(move => move.Origin.Equals(toCompile.InputPath)).ToList()));

                    line.Append("</td></tr>");

                    listLines.Add(new Tuple<int, string>((moveFail || hasError ? 3 : (hasWarning ? 2 : 1)), line.ToString()));

                    if (moveFail || hasError)
                        nbFailed++;
                    else if (hasWarning)
                        nbWarning++;
                }

                currentReport.Append("<br><h3 style='padding-top: 0px; margin-top: 0px'>Summary :</h3>");

                if (nbFailed > 0)
                    currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='MsgError' height='15px'>" + nbFailed + " files with error(s)</div>");
                if (nbWarning > 0)
                    currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='MsgWarning' height='15px'>" + nbWarning + " files with warning(s)</div>");
                if ((_currentCompil.NbFilesToCompile - nbFailed - nbWarning) > 0)
                    currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='MsgOk' height='15px'>" + (_currentCompil.NbFilesToCompile - nbFailed - nbWarning) + " files ok!</div>");

                currentReport.Append("<br><h3 style='padding-top: 0px; margin-top: 0px'>Details per program :</h3>");

                currentReport.Append("<table style='margin-bottom: 0px; width: 100%'>");
                var boolAlternate = false;
                foreach (var listLine in listLines.OrderByDescending(tuple => tuple.Item1)) {
                    currentReport.Append(listLine.Item2.Replace("%ALTERNATE%", boolAlternate ? "class='AlternatBackColor' " : "class='NormalBackColor' "));
                    boolAlternate = !boolAlternate;
                }
                currentReport.Append("</table>");

            } else {
                if (_currentCompil.HasBeenCancelled) {
                    // the process has been cancelled
                    currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='MsgWarning' height='15px'>The compilation has been cancelled by the user</div>");
                } else {
                    // provide info on the possible error!
                    currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='MsgError' height='15px'>At least one process has ended in error, the compilation has been cancelled</div>");

                    if (_currentCompil.CompilationFailedOnMaxUser()) {
                        currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Help' height='15px'>One or more processes started for this compilation tried to connect to the database and failed because the maximum number of connection has been reached (error 748). To correct this problem, you can either :<br><li>reduce the number of processes to use for each core of your computer</li><li>or increase the maximum of connections for your database (-n parameter in the proserve command)</li></div>");
                    }
                    currentReport.Append(@"<div></div>");
                }
            }

            UpdateReport(currentReport.ToString());
        }

        #endregion

        #region events

        /// <summary>
        /// Cancel the current compilation
        /// </summary>
        private void BtCancelOnButtonPressed(object sender, EventArgs eventArgs) {
            // we can only cancel the compilation part, when the file start to be moved to their destination it's done...
            if (!_currentCompil.CompilationDone) {
                btCancel.Visible = false;
                _currentCompil.CancelCompilation();
            }
        }

        /// <summary>
        /// Start the deployment!
        /// </summary>
        private void BtStartOnButtonPressed(object sender, EventArgs eventArgs) {

            SetDataFromFields();

            if (string.IsNullOrEmpty(CurrentProfile.SourceDirectory) || !Directory.Exists(CurrentProfile.SourceDirectory)) {
                BlinkTextBox(fl_directory, ThemeManager.Current.GenericErrorColor);
                return;
            }

            // init screen
            btStart.Visible = false;
            btReset.Visible = false;
            progressBar.Visible = true;
            progressBar.Progress = 0;
            progressBar.Text = @"Please wait, the compilation is starting...";
            bt_export.Visible = false;
            lbl_report.Visible = false;
            _reportExportPath = null;
            Application.DoEvents();

            // start the deployment
            Task.Factory.StartNew(() => {

                // new mass compilation
                _currentCompil = new ProCompilation {
                    // check if we need to force the compiler to only use 1 process 
                    // (either because the user want to, or because we have a single user mode database)
                    MonoProcess = CurrentProfile.ForceSingleProcess || ProEnvironment.Current.IsDatabaseSingleUser(),
                    RecursInDirectories = CurrentProfile.ExploreRecursively,
                    NumberOfProcessesPerCore = CurrentProfile.NumberProcessPerCore,
                    CompileIncludeList = CurrentProfile.FilterInclude,
                    CompileExcludeList = CurrentProfile.FilterExclude,
                };
                _currentCompil.OnCompilationEnd += OnCompilationEnd;

                if (_currentCompil.CompileFolders(new List<string> { fl_directory.Text })) {

                    UpdateReport("");
                    UpdateProgressBar();

                    btCancel.SafeInvoke(button => button.Visible = true);

                    this.SafeInvoke(page => {
                        // start a recurrent event (every second) to update the progression of the compilation
                        _progressTimer = new Timer();
                        _progressTimer.Interval = 1000;
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
        private void OnCompilationEnd() {
            this.SafeInvoke(page => {

                // TODO: if it went ok, move on to deploying files
                if (_currentCompil.DeploymentDone) {

                }

                // Update the progress bar
                progressBar.Progress = 100;
                progressBar.Text = @"Generating the report, please wait...";

                // get rid of the timer
                if (_progressTimer != null) {
                    _progressTimer.Stop();
                    _progressTimer.Dispose();
                    _progressTimer = null;
                }

                // create the report and display it
                BuildReport();

                ResetScreen();

                // notify the user
                if (!_currentCompil.HasBeenCancelled)
                    UserCommunication.NotifyUnique("ReportAvailable", "The requested deployment is over,<br>please check the generated report to see the result :<br><br><a href= '#'>Cick here to see the report</a>", MessageImg.MsgInfo, "Deploy your application", "Report available", args => {
                        Appli.GoToPage(PageNames.MassCompiler);
                        UserCommunication.CloseUniqueNotif("ReportAvailable");
                    }, Appli.IsFocused() ? 10 : 0);

                bt_export.Visible = true;
            });
        }

        /// <summary>
        /// The historic button shows a menu that allows the user to select a previously selected folders
        /// </summary>
        private void BtHistoricOnButtonPressed(object sender, EventArgs eventArgs) {
            List<YamuiMenuItem> itemList = new List<YamuiMenuItem>();
            foreach (var path in Config.Instance.CompileDirectoriesHistoric.Split(',')) {
                if (!string.IsNullOrEmpty(path)) {
                    itemList.Add(new YamuiMenuItem {
                        ItemImage = ImageResources.FolderType, ItemName = path, OnClic = () => {
                            if (IsHandleCreated) {
                                BeginInvoke((Action)delegate {
                                    fl_directory.Text = path;
                                    SaveHistoric();
                                });
                            }
                        }
                    });
                }
            }
            if (itemList.Count > 0) {
                var menu = new YamuiMenu(Cursor.Position, itemList);
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

        private void UpdateMassCompilerBaseDirectory() {
            if (CurrentProfile.AutoUpdateSourceDir)
                fl_directory.Text = ProEnvironment.Current.BaseLocalPath;
        }

        private void BtExportOnButtonPressed(object sender, EventArgs eventArgs) {

            // report already generated
            if (!string.IsNullOrEmpty(_reportExportPath)) {
                Utils.OpenAnyLink(_reportExportPath);
                return;
            }

            var reportDir = Path.Combine(Config.FolderTemp, "Export_html", DateTime.Now.ToString("yyMMdd_HHmmssfff"));
            if (!Utils.CreateDirectory(reportDir))
                return;
            _reportExportPath = Path.Combine(reportDir, "index.html");

            var html = lbl_report.GetHtml();

            var regex1 = new Regex("src=\"(.*?)\"", RegexOptions.Compiled);
            foreach (Match match in regex1.Matches(html)) {
                if (match.Groups.Count >= 2) {
                    var imgFile = Path.Combine(reportDir, match.Groups[1].Value);
                    if (!File.Exists(imgFile)) {
                        var tryImg = (Image)ImageResources.ResourceManager.GetObject(match.Groups[1].Value);
                        if (tryImg != null) {
                            tryImg.Save(imgFile);
                        }
                    }
                }
            }

            regex1 = new Regex("<a href=\"(.*?)[|\"]", RegexOptions.Compiled);
            html = regex1.Replace(html, "<a href=\"file:///$1\"");

            Utils.FileWriteAllText(_reportExportPath, html, Encoding.Default);

            // open it
            Utils.OpenAnyLink(_reportExportPath);

        }

        private void CbNameOnSelectedIndexChanged(object sender, EventArgs eventArgs) {
            CurrentProfile = ListProfiles[cbName.SelectedIndex];
            Config.Instance.CurrentDeployProfile = CurrentProfile.Name;
            SetFieldsFromData();
        }

        #endregion

        #region private methods

        // allows to update the progression bar
        private void UpdateProgressBar() {
            this.SafeInvoke(page => {
                var progression = _currentCompil.GetOverallProgression();

                // we represent the progression of the files being moved to the compilation folder in reverse
                if (_currentCompil.CompilationDone) {
                    if (progressBar.Style != ProgressStyle.Reversed) {
                        progressBar.Style = ProgressStyle.Reversed;
                        btCancel.Visible = false;
                    }
                } else if (progressBar.Style != ProgressStyle.Normal)
                    progressBar.Style = ProgressStyle.Normal;

                progressBar.Text = (Math.Abs(progression) < 0.01 ? (!_currentCompil.CompilationDone ? "Initialization" : "Creating deployment folder... ") : (!_currentCompil.CompilationDone ? "Compiling... " : "Deploying files... ") + Math.Round(progression, 1) + "%") + @" (elapsed time = " + _currentCompil.GetElapsedTime() + @")";
                progressBar.Progress = progression;
            });
        }

        // update the report, activates the scroll bars when needed
        private void UpdateReport(string htmlContent) {
            this.SafeInvoke(page => {
                // ensure it's visible 
                lbl_report.Visible = true;

                lbl_report.Text = @"
                    <div class='NormalBackColor'>
                        <table class='ToolTipName' style='margin-bottom: 0px; width: 100%'>
                            <tr>
                                <td rowspan='2' style='width: 95px; padding-left: 10px'><img src='Report' width='64' height='64' /></td>
                                <td class='NotificationTitle'>Compilation report</td>
                            </tr>
                            <tr>
                                <td class='NotificationSubTitle'>" + (_currentCompil.HasBeenCancelled ? "<img style='padding-right: 2px;' src='MsgWarning' height='25px'>Canceled by the user" : (string.IsNullOrEmpty(_currentCompil.ExecutionTime) ? "<img style='padding-right: 2px;' src='MsgInfo' height='25px'>Compilation on going..." : (_currentCompil.NumberOfProcesses == _currentCompil.NumberOfProcessesEndedOk ? "<img style='padding-right: 2px;' src='MsgOk' height='25px'>Compilation done" : "<img style='padding-right: 2px;' src='MsgError' height='25px'>An error has occured..."))) + @"</td>
                            </tr>
                        </table>                
                        <div style='margin-left: 8px; margin-right: 8px; margin-top: 0px; padding-top: 10px;'>
                            <br><h2 style='margin-top: 0px; padding-top: 0;'>Parameters :</h2>
                            <table style='width: 100%' class='NormalBackColor'>
                                <tr><td style='width: 40%; padding-right: 20px'>Compilation starting time :</td><td><b>" + _currentCompil.StartingTime + @"</b></td></tr>
                                <tr><td style='width: 40%; padding-right: 20px'>Total number of files being compile :</td><td><b>" + _currentCompil.NbFilesToCompile + @" files</b></td></tr>
                                <tr><td style='padding-right: 20px'>Type of files compiled :</td><td><b>" + _currentCompil.GetNbFilesPerType().Aggregate("", (current, kpv) => current + (@"<img style='padding-right: 5px;' src='" + kpv.Key + "Type' height='15px'><span style='padding-right: 15px;'>x" + kpv.Value + "</span>")) + @"</b></td></tr>
                                <tr><td style='padding-right: 20px'>Number of cores detected on this computer :</td><td><b>" + Environment.ProcessorCount + @" cores</b></td></tr>
                                <tr><td style='padding-right: 20px'>Number of Prowin processes used for the compilation :</td><td><b>" + _currentCompil.NumberOfProcesses + @" processes</b></td></tr>
                                <tr><td style='padding-right: 20px'>Forced to mono process? :</td><td><b>" + _currentCompil.MonoProcess + (ProEnvironment.Current.IsDatabaseSingleUser() ? " (connected to database in single user mode!)" : "") + @"</b></td></tr>
                            </table>
                            " + htmlContent + @"                    
                        </div>
                    </div>";

                // Activate scrollbars if needed
                scrollPanel.ContentPanel.Height = lbl_report.Location.Y + lbl_report.Height + 20;
                Height = lbl_report.Location.Y + lbl_report.Height + 20;
            });
        }

        private void ResetScreen() {
            this.SafeInvoke(page => {
                btStart.Visible = true;
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
                string.IsNullOrEmpty((string)name))
                return false;
            CurrentProfile.Name = (string)name;
            return true;
        }

        private void SaveProfilesList() {
            try {
                Object2Xml<DeployProfile>.SaveToFile(ListProfiles, Config.FileDeployProfiles);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while saving the deployment profiles");
            }
            Config.Instance.CurrentDeployProfile = CurrentProfile.Name;
            UpdateCombo();
        }

        private void SetFieldsFromData() {
            fl_directory.Text = CurrentProfile.SourceDirectory;
            toggleRecurs.Checked = CurrentProfile.ExploreRecursively;
            toggleAutoUpdateSourceDir.Checked = CurrentProfile.AutoUpdateSourceDir;
            toggleMono.Checked = CurrentProfile.ForceSingleProcess;
            toggleOnlyGenerateRcode.Checked = CurrentProfile.OnlyGenerateRcode;
            fl_nbProcess.Text = CurrentProfile.NumberProcessPerCore.ToString();
            fl_include.Text = CurrentProfile.FilterInclude;
            fl_exclude.Text = CurrentProfile.FilterExclude;
        }

        private void SetDataFromFields() {
            CurrentProfile.SourceDirectory = fl_directory.Text;
            CurrentProfile.ExploreRecursively = toggleRecurs.Checked;
            CurrentProfile.AutoUpdateSourceDir = toggleAutoUpdateSourceDir.Checked;
            CurrentProfile.ForceSingleProcess = toggleMono.Checked;
            CurrentProfile.OnlyGenerateRcode = toggleOnlyGenerateRcode.Checked;
            if (!int.TryParse(fl_nbProcess.Text, out CurrentProfile.NumberProcessPerCore))
                CurrentProfile.NumberProcessPerCore = 1;
            CurrentProfile.NumberProcessPerCore.Clamp(1, 15);
            CurrentProfile.FilterInclude = fl_include.Text;
            CurrentProfile.FilterExclude = fl_exclude.Text;
        }

        private void UpdateCombo() {
            cbName.SelectedIndexChanged -= CbNameOnSelectedIndexChanged;
            cbName.DataSource = ListProfiles.Select(profile => profile.Name).ToList();
            if (ListProfiles.Exists(profile => profile.Name.Equals(Config.Instance.CurrentDeployProfile)))
                cbName.SelectedItem = Config.Instance.CurrentDeployProfile;
            else
                cbName.SelectedIndex = 0;
            cbName.SelectedIndexChanged += CbNameOnSelectedIndexChanged;
        }

        private void ResetFields() {
            CurrentProfile = new DeployProfile();
            SetFieldsFromData();
            CurrentProfile = null;
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

    internal class DeployProfile {
        public string Name = "";
        public string SourceDirectory = "";
        public bool ExploreRecursively = true;
        public bool AutoUpdateSourceDir = true;
        public bool ForceSingleProcess = false;
        public bool OnlyGenerateRcode = true;
        public int NumberProcessPerCore = 3;
        public string FilterInclude = "";
        public string FilterExclude = "";
    }

}
