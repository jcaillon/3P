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
using _3PA.Data;
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

        // keep track of the current step of the deployment
        private int _currentStep;

        private int _totalSteps;

        private float _deploymentPercentage;

        // proenv copied when clicking on the start button
        private ProEnvironment.ProEnvironmentObject _proEnv;

        private DeployProfile _deployProfile;

        private Dictionary<int, List<FileToDeploy>> _filesToDeployPerStep = new Dictionary<int, List<FileToDeploy>>();

        // Stores the current compilation info
        private ProCompilation _currentCompil;

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
            linkurl.Text = @"<img src='Help'><a href='" + Config.UrlHelpMassCompiler + @"'>Learn more about this feature?</a>";

            // switch link
            lblCurEnv.LinkClicked += (sender, args) => {
                AppliMenu.ShowEnvMenuAtCursor();
            };

            // save
            tooltip.SetToolTip(btSave, "Save the settings for the currently selected profile");
            btSave.BackGrndImage = ImageResources.Save;
            btSave.ButtonPressed += (sender, args) => {
                if (string.IsNullOrEmpty(DeployProfile.Current.Name) && !ChooseName())
                    return;
                SetDataFromFields();
                SaveProfilesList();
            };

            // save as...
            tooltip.SetToolTip(btSaveAs, "Save the settings in a new profile that you will name");
            btSaveAs.BackGrndImage = ImageResources.Save;
            btSaveAs.ButtonPressed += (sender, args) => {
                var _cur = DeployProfile.Current;
                DeployProfile.List.Add(new DeployProfile());
                DeployProfile.Current = DeployProfile.List.Last();
                if (ChooseName()) {
                    SetDataFromFields();
                    SaveProfilesList();
                } else {
                    DeployProfile.List.RemoveAt(DeployProfile.List.Count - 1);
                    DeployProfile.Current = _cur;
                }
                btDelete.Visible = DeployProfile.List.Count > 1;
            };

            // delete
            tooltip.SetToolTip(btDelete, "Delete the current profile");
            btDelete.BackGrndImage = ImageResources.Delete;
            btDelete.ButtonPressed += (sender, args) => {
                if (UserCommunication.Message("Do you really want to delete this profile?", MessageImg.MsgQuestion, "Delete", "Deployment profile", new List<string> {"Yes", "Cancel"}) == 0) {
                    DeployProfile.List.Remove(DeployProfile.Current);
                    DeployProfile.Current = null;
                    SaveProfilesList();
                    SetFieldsFromData();
                    if (DeployProfile.List.Count == 1)
                        btDelete.Hide();
                }
            };

            // cb
            tooltip.SetToolTip(cbName, "Browse and select the available profiles, each profile hold deployment settings");
            cbName.SelectedIndexChanged += CbNameOnSelectedIndexChanged;

            // modify rules
            tooltip.SetToolTip(btRules, "Click to modify the rules");
            btRules.BackGrndImage = ImageResources.Rules;
            btRules.ButtonPressed += (sender, args) => {
                Deployer.Export();
                Npp.OpenFile(Config.FileDeploymentRules);
            };
            
            // view rules
            tooltip.SetToolTip(btRules, "Click to view the rules filtered for the current environment<br><i>The rules are also sorted!</i>");
            btSeeRules.BackGrndImage = ImageResources.ViewFile;
            btSeeRules.ButtonPressed += (sender, args) => {
                UserCommunication.Message(Deployer.BuildHtmlTableForRules(ProEnvironment.Current.Deployer.DeployRules), MessageImg.MsgInfo, "List of deployment rules", "Sorted and filtered for the current environment");
            };

            DeployProfile.OnDeployProfilesUpdate += () => {
                UpdateCombo();
                SetFieldsFromData();
                btDelete.Visible = DeployProfile.List.Count > 1;
            };

            // subscribe
            ProEnvironment.OnEnvironmentChange += OnShow;
            Deployer.OnDeployConfigurationUpdate += () => ProEnvironment.Current.Deployer.DeployRules = null;
            Deployer.OnDeployConfigurationUpdate += OnShow;
            
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
            btDelete.Visible = DeployProfile.List.Count > 1;

            // cur env
            lblCurEnv.Text = string.Format("{0} <a href='#'>(switch)</a>", ProEnvironment.Current.Name + (!string.IsNullOrEmpty(ProEnvironment.Current.Suffix) ? " - " + ProEnvironment.Current.Suffix : ""));
            lbl_deployDir.Text = string.Format("The deployment directory is <a href='{0}'>{0}</a>", ProEnvironment.Current.BaseCompilationPath);

            // update the rules for the current env
            lbl_rules.Text = string.Format("There are <b>{0}</b> rules for the compilation (step 0), <b>{1}</b> rules for step 1, <b>{2}</b> rules for step 2 and <b>{3}</b> rules beyond", ProEnvironment.Current.Deployer.DeployRules.Count(rule => rule.Step == 0), ProEnvironment.Current.Deployer.DeployRules.Count(rule => rule.Step == 1), ProEnvironment.Current.Deployer.DeployRules.Count(rule => rule.Step == 2), ProEnvironment.Current.Deployer.DeployRules.Count(rule => rule.Step >= 3));
            
            if (DeployProfile.Current.AutoUpdateSourceDir)
                fl_directory.Text = ProEnvironment.Current.BaseLocalPath;

        }

        #endregion
        
        #region Build the report

        private void BuildReport() {

            StringBuilder currentReport = new StringBuilder();

            currentReport.Append(@"<h2 style='margin-top: 8px; margin-bottom: 8px;'>Results :</h2>");

            // the execution ended successfully
            if (_currentCompil.NumberOfProcesses == _currentCompil.NumberOfProcessesEndedOk) {

                var listLinesByStep = new Dictionary<int, List<Tuple<int, string>>> {
                    {0, new List<Tuple<int, string>>()}
                };
                var listLinesCompilation = new List<Tuple<int, string>>();
                StringBuilder line = new StringBuilder();

                var totalDeployedFiles = 0;
                var nbDeploymentError = 0;
                var nbCompilationError = 0;
                var nbCompilationWarning = 0;

                // compiled files
                foreach (var fileToCompile in _currentCompil.GetListOfFileToCompile.OrderBy(compile => Path.GetFileName(compile.InputPath))) {

                    var toCompile = fileToCompile;
                    var errorsOfTheFile = _currentCompil.ErrorsList.Where(error => error.CompiledFilePath.Equals(toCompile.InputPath)).ToList();
                    bool hasError = errorsOfTheFile.Count > 0 && errorsOfTheFile.Exists(error => error.Level > ErrorLevel.StrongWarning);
                    bool hasWarning = errorsOfTheFile.Count > 0 && errorsOfTheFile.Exists(error => error.Level <= ErrorLevel.StrongWarning);

                    if (hasError || hasWarning) {
                        // only add compilation errors
                        line.Clear();
                        line.Append("<div %ALTERNATE%style=\"background-repeat: no-repeat; background-image: url('" + (hasError ? "Error30x30" : "Warning30x30") + "'); padding-left: 40px; padding-top: 6px; padding-bottom: 6px;\">");
                        line.Append(ProCompilation.FormatCompilationResult(fileToCompile.InputPath, errorsOfTheFile, null));
                        line.Append("</div>");
                        listLinesCompilation.Add(new Tuple<int, string>(hasError ? 3 : 2, line.ToString()));
                    }

                    if (hasError) {
                        nbCompilationError++;
                        // if compilation errors, delete all transfer records for this file since they obviously didn't happen
                        _filesToDeployPerStep[0].RemoveAll(move => move.Origin.Equals(toCompile.InputPath));
                    } else if (hasWarning)
                        nbCompilationWarning++;
                }

                // for each deploy step
                foreach (var kpv in _filesToDeployPerStep) {

                    // group by transfer type
                    foreach (var groupType in kpv.Value.GroupBy(deploy => deploy.DeployType).Select(deploys => deploys.ToList()).ToList().OrderBy(list => list.First().DeployType)) {

                        // group either by directory name or by archive name
                        var groupDirectory = groupType.First().DeployType <= DeployType.Zip ? 
                            groupType.GroupBy(deploy => deploy.ArchivePath).Select(deploys => deploys.ToList()).ToList().OrderBy(list => list.First().ArchivePath) : 
                            groupType.GroupBy(deploy => Path.GetDirectoryName(deploy.To)).Select(deploys => deploys.ToList()).ToList().OrderBy(list => Path.GetDirectoryName(list.First().To));

                        foreach (var group in groupDirectory) {

                            var deployFailed = group.Exists(deploy => !deploy.IsOk);
                            var first = group.First();

                            line.Clear();
                            line.Append("<div %ALTERNATE%style=\"background-repeat: no-repeat; background-image: url('" + (deployFailed ? "Error30x30" : "Ok30x30") + "'); padding-left: 40px; padding-top: 6px; padding-bottom: 6px;\">");
                
                            if (first.DeployType <= DeployType.Zip) {
                                line.Append("<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage(first.DeployType == DeployType.Prolib ? "Pl": "Zip", true) + "' height='15px'><b>" + string.Format("<a class='SubTextColor' href='{0}'>{1}</a>", first.ArchivePath, Path.GetFileName(first.ArchivePath)) + "</b> in " + Path.GetDirectoryName(first.ArchivePath).ToHtmlLink() + "</div>");
                            } else {
                                line.Append("<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage("Folder", true) + "' height='15px'><b>" + Path.GetDirectoryName(first.To).ToHtmlLink() + "</div>");
                            }

                            foreach (var file in group.OrderBy(deploy => deploy.To)) {
                                var ext = (Path.GetExtension(file.To) ?? "").Replace(".", "");
                                var transferMsg = file.DeployType == DeployType.Move ? "" : "(" + file.DeployType + ") ";
                                if (file.IsOk) {
                                    line.Append("<div style='padding-left: 10px'>" + "<img src='" + Utils.GetExtensionImage(ext) + "' height='15px'>" + transferMsg + (ext.EqualsCi("lst") ? file.To.ToHtmlLink() : Path.GetDirectoryName(file.To).ToHtmlLink(file.To)) + "</div>");
                                } else {
                                    line.Append("<div style='padding-left: 10px'>" + "<img src='Error30x30' height='15px'>Transfer error " + transferMsg + file.To + "</div>");
                                }
                            }

                            line.Append("</div>");

                            if (!listLinesByStep.ContainsKey(kpv.Key))
                                listLinesByStep.Add(kpv.Key, new List<Tuple<int, string>>());

                            listLinesByStep[kpv.Key].Add(new Tuple<int, string>(deployFailed ? 3 : 1, line.ToString()));

                            if (deployFailed)
                                nbDeploymentError += group.Count(deploy => !deploy.IsOk);
                            else
                                totalDeployedFiles += group.Count;

                        }
                    }
                }

                // compilation
                currentReport.Append(@"<div style='padding-top: 7px; padding-bottom: 7px;'>Compiling <b>" + _currentCompil.NbFilesToCompile + "</b> files : <b>" + Utils.GetNbFilesPerType(_currentCompil.GetListOfFileToCompile.Select(compile => compile.InputPath).ToList()).Aggregate("", (current, kpv) => current + (@"<img style='padding-right: 5px;' src='" + Utils.GetExtensionImage(kpv.Key.ToString(), true) + "' height='15px'><span style='padding-right: 12px;'>x" + kpv.Value + "</span>")) + "</b></div>");

                // compilation time
                currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Time' height='15px'>Total elapsed time for the compilation : <b>" + _currentCompil.ExecutionTime + @"</b></div>");

                if (nbCompilationError > 0)
                    currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Error30x30' height='15px'>" + nbCompilationError + " files with compilation error(s)</div>");
                if (nbCompilationWarning > 0)
                    currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Warning30x30' height='15px'>" + nbCompilationWarning + " files with compilation warning(s)</div>");
                if (_currentCompil.NbFilesToCompile - nbCompilationError - nbCompilationWarning > 0)
                    currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Ok30x30' height='15px'>" + (_currentCompil.NbFilesToCompile - nbCompilationError - nbCompilationWarning) + " files compiled correctly</div>");

                // deploy
                currentReport.Append(@"<div style='padding-top: 7px; padding-bottom: 7px;'>Deploying <b>" + totalDeployedFiles + "</b> files : <b>" + Utils.GetNbFilesPerType(_filesToDeployPerStep.SelectMany(pair => pair.Value).Select(deploy => deploy.To).ToList()).Aggregate("", (current, kpv) => current + (@"<img style='padding-right: 5px;' src='" + Utils.GetExtensionImage(kpv.Key.ToString(), true) + "' height='15px'><span style='padding-right: 12px;'>x" + kpv.Value + "</span>")) + "</b></div>");

                // deployment time
                currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Time' height='15px'>Total elapsed time for the deployment : <b>" + _currentCompil.GetElapsedTime() + @"</b></div>");

                if (nbDeploymentError > 0)
                    currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Error30x30' height='15px'>" + nbDeploymentError + " files not deployed</div>");
                if (totalDeployedFiles - nbDeploymentError > 0)
                    currentReport.Append("<div><img style='padding-right: 20px; padding-left: 5px;' src='Ok30x30' height='15px'>" + (totalDeployedFiles - nbDeploymentError) + " files deployed correctly</div>");

                // compilation
                if (listLinesCompilation.Count > 0) {
                    currentReport.Append("<h3 style='margin-top: 7px; margin-bottom: 7px;'>Compilation error details :</h3>");
                    var boolAlternate = false;
                    foreach (var listLine in listLinesCompilation.OrderByDescending(tuple => tuple.Item1)) {
                        currentReport.Append(listLine.Item2.Replace("%ALTERNATE%", boolAlternate ? "class='AlternatBackColor' " : "class='NormalBackColor' "));
                        boolAlternate = !boolAlternate;
                    }
                }

                // deployment steps
                foreach (var listLinesKpv in listLinesByStep) {
                    currentReport.Append("<h3 style='margin-top: 7px; margin-bottom: 7px;'>Deployment step " + listLinesKpv.Key + " :</h3>");

                    var boolAlternate2 = false;
                    foreach (var listLine in listLinesKpv.Value.OrderByDescending(tuple => tuple.Item1)) {
                        currentReport.Append(listLine.Item2.Replace("%ALTERNATE%", boolAlternate2 ? "class='AlternatBackColor' " : "class='NormalBackColor' "));
                        boolAlternate2 = !boolAlternate2;
                    }
                }

            } else {

                if (_currentCompil.HasBeenCancelled) {
                    // the process has been cancelled
                    currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Warning30x30' height='15px'>The compilation has been cancelled by the user</div>");
                } else {
                    // provide info on the possible error!
                    currentReport.Append(@"<div><img style='padding-right: 20px; padding-left: 5px;' src='Error30x30' height='15px'>At least one process has ended in error, the compilation has been cancelled</div>");

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
        /// Start the deployment!
        /// </summary>
        private void BtStartOnButtonPressed(object sender, EventArgs eventArgs) {

            SetDataFromFields();
            SaveProfilesList();

            if (string.IsNullOrEmpty(DeployProfile.Current.SourceDirectory) || !Directory.Exists(DeployProfile.Current.SourceDirectory)) {
                BlinkTextBox(fl_directory, ThemeManager.Current.GenericErrorColor);
                return;
            }

            // init screen
            btStart.Visible = false;
            btReset.Visible = false;
            progressBar.Visible = true;
            progressBar.Progress = 0;
            progressBar.Text = @"Please wait, the deployment is starting...";
            btReport.Visible = false;
            lbl_report.Visible = false;
            _reportExportPath = null;
            Application.DoEvents();

            // start the deployment
            Task.Factory.StartNew(() => {

                _proEnv = new ProEnvironment.ProEnvironmentObject(ProEnvironment.Current);
                _deployProfile = new DeployProfile(DeployProfile.Current);

                // new mass compilation
                _currentCompil = new ProCompilation {
                    // check if we need to force the compiler to only use 1 process 
                    // (either because the user want to, or because we have a single user mode database)
                    MonoProcess = _deployProfile.ForceSingleProcess || _proEnv.IsDatabaseSingleUser(),
                    NumberOfProcessesPerCore = _deployProfile.NumberProcessPerCore,
                    RFilesOnly = _deployProfile.OnlyGenerateRcode
                };
                _currentCompil.OnCompilationEnd += OnCompilationEnd;

                var filesToCompile = _proEnv.Deployer.GetFilesList(new List<string> { _deployProfile.SourceDirectory }, _deployProfile.ExploreRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, 0);
                
                _deploymentPercentage = 0;
                _currentStep = 0;
                _totalSteps = _proEnv.Deployer.DeployTransferRules.Count > 0 ? _proEnv.Deployer.DeployTransferRules.Max(rule => rule.Step) : 0;
                _filesToDeployPerStep.Clear();

                if (filesToCompile.Count > 0 && _currentCompil.CompileFiles(filesToCompile)) {

                    UpdateReport("");
                    UpdateProgressBar();

                    btCancel.SafeInvoke(button => button.Visible = true);

                    this.SafeInvoke(page => {
                        // start a recurrent event (every second) to update the progression of the compilation
                        _progressTimer = new Timer();
                        _progressTimer.Interval = 500;
                        _progressTimer.Tick += (o, args) => UpdateProgressBar();
                        _progressTimer.Start();
                    });

                } else {
                    if (filesToCompile.Count == 0) {
                        UserCommunication.Notify("No compilable files found in the input directories,<br>the valid extensions for compilable Progress files are : " + Config.Instance.CompileKnownExtension, MessageImg.MsgInfo, "Multiple compilation", "No files found", 10);
                    }

                    // nothing started
                    ResetScreen();
                }
            });
        }

        // called when the compilation ended
        private void OnCompilationEnd() {
            Task.Factory.StartNew(() => {

                _filesToDeployPerStep.Add(0, _currentCompil.TransferedFiles);

                // if it went ok, move on to deploying files
                if (_currentCompil.DeploymentDone) {

                    // hook
                    ExecuteDeploymentHook();

                    _currentStep++; // move on to step 1

                    // Update the progress bar
                    UpdateProgressBar();

                    // transfer rules found for this step?
                    while (_proEnv.Deployer.DeployTransferRules.Exists(rule => rule.Step == _currentStep)) {

                        _filesToDeployPerStep.Add(_currentStep,
                            _proEnv.Deployer.DeployFilesForStep(_currentStep, new List<string> { _currentStep == 1 ? _deployProfile.SourceDirectory : _proEnv.BaseCompilationPath }, _deployProfile.ExploreRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, f => _deploymentPercentage = f));

                        // hook
                        ExecuteDeploymentHook();

                        _currentStep++;
                    }
                }

                this.SafeInvoke(page => {

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

                    btReport.Visible = true;
                });
            });
        }
        
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
            DeployProfile.Current = DeployProfile.List[cbName.SelectedIndex];
            Config.Instance.CurrentDeployProfile = DeployProfile.Current.Name;
            SetFieldsFromData();
        }

        #endregion

        #region private methods

        private void ExecuteDeploymentHook() {
            // launch the compile process for the current file
            if (File.Exists(Config.FileDeploymentHook)) {
                var hookExec = new ProExecution {
                    DeploymentStep = _currentStep,
                    DeploymentSourcePath = _deployProfile.SourceDirectory
                };
                if (hookExec.Do(ExecutionType.DeploymentHook)) {
                    hookExec.Process.WaitForExit();

                    var fileInfo = new FileInfo(hookExec.LogPath);
                    if (fileInfo.Length > 0) {
                        // the .log is not empty, maybe something went wrong in the runner, display errors
                        UserCommunication.Notify(
                            "Something went wrong while executing the deployment hook procedure:<br>" + Config.FileDeploymentHook.ToHtmlLink() + "<br>The following problems were logged :" +
                            Utils.ReadAndFormatLogToHtml(hookExec.LogPath), MessageImg.MsgError,
                            "Deployment hook procedure", "Execution failed");
                    }
                }
            }
        }

        // allows to update the progression bar
        private void UpdateProgressBar() {
            this.SafeInvoke(page => {

                if (_currentStep == 0) {

                    var progression = _currentCompil.GetOverallProgression();

                    // we represent the progression of the files being moved to the compilation folder in reverse
                    if (_currentCompil.CompilationDone) {
                        if (progressBar.Style != ProgressStyle.Reversed) {
                            progressBar.Style = ProgressStyle.Reversed;
                            btCancel.Visible = false;
                        }
                    } else if (progressBar.Style != ProgressStyle.Normal)
                        progressBar.Style = ProgressStyle.Normal;

                    progressBar.Text = @"Step 0 / " + _totalSteps + @" ~ " + (Math.Abs(progression) < 0.01 ? (!_currentCompil.CompilationDone ? "Initialization" : "Creating deployment folder... ") : (!_currentCompil.CompilationDone ? "Compiling... " : "Deploying files... ") + Math.Round(progression, 1) + "%") + @" (elapsed time = " + _currentCompil.GetElapsedTime() + @")";
                    progressBar.Progress = progression;

                } else {

                    var neededStyle = _currentStep%2 == 0 ? ProgressStyle.Reversed : ProgressStyle.Normal;

                    if (progressBar.Style != neededStyle) {
                        progressBar.Style = neededStyle;
                    }

                    progressBar.Text = @"Step " + _currentStep + @" / " + _totalSteps + @" ~ " + (_deploymentPercentage > 0.01 ? @"Deploying files... " + Math.Round(_deploymentPercentage, 1) + @"%" : "Enumerating files to deploy...") + @" (elapsed time = " + _currentCompil.GetElapsedTime() + @")";
                    progressBar.Progress = _deploymentPercentage;
                    
                }

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
                                <td rowspan='2' style='width: 95px; padding-left: 10px'><img src='Report_64x64' width='64' height='64' /></td>
                                <td class='NotificationTitle'>Deployment report</td>
                            </tr>
                            <tr>
                                <td class='NotificationSubTitle'>" + (_currentCompil.HasBeenCancelled ? "<img style='padding-right: 2px;' src='Warning30x30' height='25px'>Canceled by the user" : (string.IsNullOrEmpty(_currentCompil.ExecutionTime) ? "<img style='padding-right: 2px;' src='MsgInfo' height='25px'>Compilation on going..." : (_currentCompil.NumberOfProcesses == _currentCompil.NumberOfProcessesEndedOk ? "<img style='padding-right: 2px;' src='Ok30x30' height='25px'>Done!" : "<img style='padding-right: 2px;' src='Error30x30' height='25px'>An error has occured..."))) + @"</td>
                            </tr>
                        </table>         
                        <h2 style='margin-top: 8px; margin-bottom: 8px;'>Parameters :</h2>       
                        <div style='margin-left: 8px; margin-right: 8px;'>
                            <table style='width: 100%' class='NormalBackColor'>
                                <tr><td style='width: 40%; padding-right: 20px'>Compilation starting time :</td><td><b>" + _currentCompil.StartingTime + @"</b></td></tr>
                                <tr><td style='padding-right: 20px'>Number of cores detected on this computer :</td><td><b>" + Environment.ProcessorCount + @" cores</b></td></tr>
                                <tr><td style='padding-right: 20px'>Number of Prowin processes used for the compilation :</td><td><b>" + _currentCompil.NumberOfProcesses + @" processes</b></td></tr>
                                <tr><td style='padding-right: 20px'>Forced to mono process? :</td><td><b>" + _currentCompil.MonoProcess + (_proEnv.IsDatabaseSingleUser() ? " (connected to database in single user mode!)" : "") + @"</b></td></tr>
                                <tr><td style='width: 40%; padding-right: 20px'>Total number of files being compile :</td><td><b>" + _currentCompil.NbFilesToCompile + @" files</b></td></tr>
                                <tr><td style='width: 40%; padding-right: 20px'>Source directory :</td><td><b>" + _deployProfile.SourceDirectory.ToHtmlLink() + @"</b></td></tr>
                                <tr><td style='width: 40%; padding-right: 20px'>Target deployment directory :</td><td><b>" + _proEnv.BaseCompilationPath.ToHtmlLink() + @"</b></td></tr>
                            </table>           
                        </div>
                        " + htmlContent + @"
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
            DeployProfile.Current.Name = (string)name;
            return true;
        }

        private void SaveProfilesList() {
            try {
                Object2Xml<DeployProfile>.SaveToFile(DeployProfile.List, Config.FileDeployProfiles);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while saving the deployment profiles");
            }
            Config.Instance.CurrentDeployProfile = DeployProfile.Current.Name;
            UpdateCombo();
        }

        private void SetFieldsFromData() {
            fl_directory.Text = DeployProfile.Current.SourceDirectory;
            toggleRecurs.Checked = DeployProfile.Current.ExploreRecursively;
            toggleAutoUpdateSourceDir.Checked = DeployProfile.Current.AutoUpdateSourceDir;
            toggleMono.Checked = DeployProfile.Current.ForceSingleProcess;
            toggleOnlyGenerateRcode.Checked = DeployProfile.Current.OnlyGenerateRcode;
            fl_nbProcess.Text = DeployProfile.Current.NumberProcessPerCore.ToString();
        }

        private void SetDataFromFields() {
            DeployProfile.Current.SourceDirectory = fl_directory.Text;
            DeployProfile.Current.ExploreRecursively = toggleRecurs.Checked;
            DeployProfile.Current.AutoUpdateSourceDir = toggleAutoUpdateSourceDir.Checked;
            DeployProfile.Current.ForceSingleProcess = toggleMono.Checked;
            DeployProfile.Current.OnlyGenerateRcode = toggleOnlyGenerateRcode.Checked;
            if (!int.TryParse(fl_nbProcess.Text, out DeployProfile.Current.NumberProcessPerCore))
                DeployProfile.Current.NumberProcessPerCore = 1;
            DeployProfile.Current.NumberProcessPerCore.Clamp(1, 15);
        }

        private void UpdateCombo() {
            cbName.SelectedIndexChanged -= CbNameOnSelectedIndexChanged;
            cbName.DataSource = DeployProfile.List.Select(profile => profile.Name).ToList();
            if (DeployProfile.List.Exists(profile => profile.Name.Equals(Config.Instance.CurrentDeployProfile)))
                cbName.SelectedItem = Config.Instance.CurrentDeployProfile;
            else
                cbName.SelectedIndex = 0;
            cbName.SelectedIndexChanged += CbNameOnSelectedIndexChanged;
        }

        private void ResetFields() {
            DeployProfile.Current = new DeployProfile();
            SetFieldsFromData();
            DeployProfile.Current = null;
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
