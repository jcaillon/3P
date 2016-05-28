#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ExportPage.cs) is part of 3P.
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
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using _3PA.Html;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.FilesInfoNs;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures.Appli.Pages.Actions {

    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class CompilePage : YamuiPage {

        #region fields

        // Timer that ticks every seconds to update the progress bar
        private Timer _progressTimer;

        // Stores the current compilation info
        private ProCompilation _currentCompil;

        #endregion

        #region constructor
        public CompilePage() {

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

            btUndo.BackGrndImage = ImageResources.UndoUserAction;
            btUndo.ButtonPressed += BtUndoOnButtonPressed;
            tooltip.SetToolTip(btUndo, "Click to <b>select</b> the base local path (your source directory)<br>for the current environment");

            // default values
            fl_directory.Text = ProEnvironment.Current.BaseLocalPath;
            toggleRecurs.Checked = Config.Instance.CompileExploreDirRecursiv;

            // compilation
            tooltip.SetToolTip(btStart, "Click to <b>start</b> the compilation of all executable progress files<br>for the selected folder");
            btStart.ButtonPressed += BtStartOnButtonPressed;

            // cancel
            tooltip.SetToolTip(btCancel, "Click to <b>cancel</b> the current compilation");
            btCancel.ButtonPressed += BtCancelOnButtonPressed;
            btCancel.Visible = false;

            // progress bar
            progressBar.Style = ProgressStyle.Normal;
            progressBar.CenterText = CenterElement.Text;
            progressBar.Visible = false;

            // report
            lbl_report.Visible = false;
            lbl_report.LinkClicked += Utils.OpenPathClickHandler;

            // subscribe to env update
            ProEnvironment.OnEnvironmentChange += ProEnvironmentOnOnEnvironmentChange;
        }

        #endregion

        #region Build the report

        private void BuildReport() {
            StringBuilder currentReport = new StringBuilder();

            currentReport.Append(@"<h2>Results :</h2>");

            // compilation time
            currentReport.Append(@"<div class='ToolTipRowWithImg'><img style='padding-right: 20px; padding-left: 5px;' src='Time' height='15px'>Total elapsed time for the compilation : <b>" + _currentCompil.ExecutionTime + @"</b></div>");

            // the execution ended successfully
            if (_currentCompil.NumberOfProcesses == _currentCompil.NumberOfProcessesEndedOk) {
                currentReport.Append(@"<div class='ToolTipRowWithImg'><img style='padding-right: 20px; padding-left: 5px;' src='MsgOk' height='15px'>All the processes ended correctly</div>");

                var listLines = new List<Tuple<int, string>>();

                StringBuilder line = new StringBuilder();

                var nbFailed = 0;
                var nbWarning = 0;
                
                foreach (var fileToCompile in _currentCompil.GetListOfFileToCompile) {

                    var toCompile = fileToCompile;

                    bool moveFail = _currentCompil.MovedFiles.Exists(move => move.Origin.Equals(fileToCompile.InputPath) && !move.IsOk);
                    var errorsOfTheFile = _currentCompil.ErrorsList.Where(error => error.CompiledFilePath.Equals(toCompile.InputPath)).ToList();
                    bool hasError = errorsOfTheFile.Count > 0 && errorsOfTheFile.Exists(error => error.Level > ErrorLevel.StrongWarning);
                    bool hasWarning = errorsOfTheFile.Count > 0 && errorsOfTheFile.Exists(error => error.Level <= ErrorLevel.StrongWarning); ;

                    line.Clear();

                    line.Append("<tr><td style='width: 50px; padding-bottom: 15px;'><img src='" + (moveFail || hasError ? "MsgError" : (hasWarning ? "MsgWarning" : "MsgOk")) + "' width='30' height='30' /></td><td %ALTERNATE%style='padding-bottom: 10px;'>");

                    line.Append(ProExecution.FormatCompilationResult(fileToCompile, errorsOfTheFile, _currentCompil.MovedFiles.Where(move => move.Origin.Equals(toCompile.InputPath)).ToList()));

                    line.Append("</td></tr>");

                    listLines.Add(new Tuple<int, string>((moveFail || hasError ? 3 : (hasWarning ? 2 : 1)), line.ToString()));

                    if (moveFail || hasError) 
                        nbFailed++;
                    else if (hasWarning)
                        nbWarning++;
                }

                currentReport.Append("<br><h3 style='padding-top: 0px; margin-top: 0px'>Summary :</h3>");

                if (nbFailed > 0)
                    currentReport.Append("<div class='ToolTipRowWithImg'><img style='padding-right: 20px; padding-left: 5px;' src='MsgError' height='15px'>" + nbFailed + " files with error(s)</div>");
                if (nbWarning > 0)
                    currentReport.Append("<div class='ToolTipRowWithImg'><img style='padding-right: 20px; padding-left: 5px;' src='MsgWarning' height='15px'>" + nbWarning + " files with warning(s)</div>");
                if ((_currentCompil.NbFilesToCompile - nbFailed - nbWarning) > 0)
                    currentReport.Append("<div class='ToolTipRowWithImg'><img style='padding-right: 20px; padding-left: 5px;' src='MsgOk' height='15px'>" + (_currentCompil.NbFilesToCompile - nbFailed - nbWarning) + " files ok!</div>");

                currentReport.Append("<br><h3 style='padding-top: 0px; margin-top: 0px'>Details per program :</h3>");

                currentReport.Append("<table style='margin-bottom: 0px; width: 100%'>");
                var boolAlternate = false;
                foreach (var listLine in listLines.OrderByDescending(tuple => tuple.Item1)) {
                    currentReport.Append(listLine.Item2.Replace("%ALTERNATE%", boolAlternate ? "class='AlternatBackColor' " : ""));
                    boolAlternate = !boolAlternate;
                }
                currentReport.Append("</table>");

            } else {
                if (_currentCompil.HasBeenKilled) {
                    // the process has been cancelled
                    currentReport.Append(@"<div class='ToolTipRowWithImg'><img style='padding-right: 20px; padding-left: 5px;' src='MsgWarning' height='15px'>The compilation has been cancelled by the user</div>");
                } else {
                    // provide info on the possible error!
                    currentReport.Append(@"<div class='ToolTipRowWithImg'><img style='padding-right: 20px; padding-left: 5px;' src='MsgError' height='15px'>Only " + _currentCompil.NumberOfProcessesEndedOk + " on a total of " + _currentCompil.NumberOfProcesses + " ended correctly...</div>");
                    currentReport.Append(@"<div>A possible explanation is....................... TODO</div>");
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
                _currentCompil.KillProcesses();
                OnCompilationEnd();
            }
        }

        /// <summary>
        /// Start the compilation!
        /// </summary>
        private void BtStartOnButtonPressed(object sender, EventArgs eventArgs) {

            // remember options
            Config.Instance.CompileExploreDirRecursiv = toggleRecurs.Checked;

            // init screen
            btStart.Visible = false;
            progressBar.Visible = true;
            progressBar.Progress = 0;
            progressBar.Text = @"The compilation is starting, please wait...";
            lbl_report.Visible = false;
            Application.DoEvents();

            // start the compilation
            Task.Factory.StartNew(() => {
                if (IsHandleCreated) {
                    BeginInvoke((Action)delegate {

                        // new mass compilation
                        _currentCompil = new ProCompilation {
                            // check if we need to force the compiler to only use 1 process 
                            // (either because the user want to, or because we have a single user mode database)
                            MonoProcess = Config.Instance.CompileForceMonoProcess || ProEnvironment.Current.IsDatabaseSingleUser(),
                            RecursInDirectories = Config.Instance.CompileExploreDirRecursiv
                        };
                        _currentCompil.OnCompilationEnd += OnCompilationEnd;

                        if (_currentCompil.CompileFolders(new List<string> { fl_directory.Text })) {
                            // display the progress bar
                            btCancel.Visible = true;

                            UpdateReport("");

                            // start a recurrent event (every second) to update the progression of the compilation
                            _progressTimer = new Timer();
                            _progressTimer.Interval = 500;
                            _progressTimer.Tick += (o, args) => UpdateProgressBar();
                            _progressTimer.Start();
                        } else {
                            // nothing started
                            ResetScreen();
                        }

                    });
                }
            });
        }

        // called when the compilation ended
        private void OnCompilationEnd() {
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
            Task.Factory.StartNew(() => {
                if (!_currentCompil.HasBeenKilled)
                    UserCommunication.Notify("The requested compilation is over,<br>please check the generated report to see the result :<br><br><a href= '#'>Cick here to see the report</a>", MessageImg.MsgInfo, "Mass compiler", "Report available", args => { Appli.GoToPage(PageNames.MassCompiler); }, Appli.IsFocused() ? 10 : 0);
            });
        }

        /// <summary>
        /// The historic button shows a menu that allows the user to select a previously selected folders
        /// </summary>
        private void BtHistoricOnButtonPressed(object sender, EventArgs eventArgs) {
            List<YamuiMenuItem> itemList = new List<YamuiMenuItem>();
            foreach (var path in Config.Instance.CompileDirectoriesHistoric.Split(',')) {
                if (!string.IsNullOrEmpty(path)) {
                    itemList.Add(new YamuiMenuItem {ItemImage = ImageResources.FolderType, ItemName = path, OnClic = () => {
                        if (IsHandleCreated) {
                            BeginInvoke((Action) delegate {
                                fl_directory.Text = path;
                                SaveHistoric();
                            });
                        }
                    }});
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

        private void ProEnvironmentOnOnEnvironmentChange() {
            fl_directory.Text = ProEnvironment.Current.BaseLocalPath;
        }

        #endregion

        #region private methods

        // allows to update the progression bar
        private void UpdateProgressBar() {
            if (IsHandleCreated) {
                BeginInvoke((Action) delegate {
                    var progression = _currentCompil.GetOverallProgression();

                    // we represent the progression of the files being moved to the compilation folder in reverse
                    if (_currentCompil.CompilationDone) {
                        if (progressBar.Style != ProgressStyle.Reversed) {
                            progressBar.Style = ProgressStyle.Reversed;
                            btCancel.Visible = false;
                        }
                    } else if (progressBar.Style != ProgressStyle.Normal)
                        progressBar.Style = ProgressStyle.Normal;

                    progressBar.Text = (Math.Abs(progression) < 0.01 ? "Initialization" : (_currentCompil.CompilationDone ? "Compiling... " : "Moving files... ") + Math.Round(progression, 1) + "%") + @" (elapsed time = " + _currentCompil.GetElapsedTime() + @")";
                    progressBar.Progress = progression;
                });
            }
        }

        // update the report, activates the scroll bars when needed
        private void UpdateReport(string htmlContent) {
            if (IsHandleCreated) {
                BeginInvoke((Action) delegate {
                    // ensure it's visible 
                    lbl_report.Visible = true;

                    lbl_report.Text = @"
                        <table class='ToolTipName' style='margin-bottom: 0px; width: 100%'>
                            <tr>
                                <td rowspan='2' style='width: 95px; padding-left: 10px'><img src='Report' width='64' height='64' /></td>
                                <td class='NotificationTitle'>Compilation report</td>
                            </tr>
                            <tr>
                                <td class='NotificationSubTitle'>" + (_currentCompil.HasBeenKilled ? "<img style='padding-right: 2px;' src='MsgWarning' height='25px'>Canceled by the user" : (string.IsNullOrEmpty(_currentCompil.ExecutionTime) ? "<img style='padding-right: 2px;' src='MsgInfo' height='25px'>Compilation on going..." : (_currentCompil.NumberOfProcesses == _currentCompil.NumberOfProcessesEndedOk ? "<img style='padding-right: 2px;' src='MsgOk' height='25px'>Compilation done" : "<img style='padding-right: 2px;' src='MsgError' height='25px'>An error has occured..."))) + @"</td>
                            </tr>
                        </table>                
                        <div style='margin-left: 8px; margin-right: 8px; margin-top: 0px; padding-top: 10px;'>
                            <br><h2 style='margin-top: 0px; padding-top: 0;'>Parameters :</h2>
                            <table style='width: 100%'>
                                <tr><td style='width: 40%; padding-right: 20px'>Total number of files being compile :</td><td><b>" + _currentCompil.NbFilesToCompile + @" files</b></td></tr>
                                <tr><td style='padding-right: 20px'>Type of files compiled :</td><td><b>" + _currentCompil.GetNbFilesPerType().Aggregate("", (current, kpv) => current + (@"<img style='padding-right: 5px;' src='" + kpv.Key + "Type' height='15px'><span style='padding-right: 15px;'>x" + kpv.Value + "</span>")) + @"</b></td></tr>
                                <tr><td style='padding-right: 20px'>Number of cores detected on this computer :</td><td><b>" + Environment.ProcessorCount + @" cores</b></td></tr>
                                <tr><td style='padding-right: 20px'>Number of Prowin processes used for the compilation :</td><td><b>" + _currentCompil.NumberOfProcesses + @" processes</b></td></tr>
                            </table>
                            " + htmlContent + @"                    
                        </div>";

                    // Activate scrollbars if needed
                    var yPos = lbl_report.Location.Y + lbl_report.Height;
                    if (yPos > Height) {
                        dockedPanel.ContentPanel.Controls.Add(new YamuiLabel {
                            AutoSize = true,
                            Location = new Point(0, yPos),
                            Text = @" "
                        });
                        yPos += 10;
                        dockedPanel.ContentPanel.Height = yPos;
                    }
                    Height = yPos;
                });
            }
        }

        private void ResetScreen() {
            btStart.Visible = true;
            btCancel.Visible = false;
            progressBar.Visible = false;
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
            }
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
