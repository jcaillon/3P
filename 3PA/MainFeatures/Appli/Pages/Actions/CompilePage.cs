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
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures.Appli.Pages.Actions {

    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class CompilePage : YamuiPage {

        #region fields

        private Timer _progressTimer;
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

            // progress bar
            progressBar.Style = ProgressStyle.Normal;
            progressBar.CenterText = CenterElement.Text;
            progressBar.Visible = false;

            // report
            lbl_report.Visible = false;

            // subscribe to env update
            ProEnvironment.OnEnvironmentChange += ProEnvironmentOnOnEnvironmentChange;
        }

        #endregion

        #region events

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

        /// <summary>
        /// Start the compilation!
        /// </summary>
        private void BtStartOnButtonPressed(object sender, EventArgs eventArgs) {

            // remember options
            Config.Instance.CompileExploreDirRecursiv = toggleRecurs.Checked;

            // new mass compilation
            _currentCompil = new ProCompilation {
                // check if we need to force the compiler to only use 1 process 
                // (either because the user want to, or because we have a single user mode database)
                MonoProcess = Config.Instance.CompileForceMonoProcess || ProEnvironment.Current.IsDatabaseSingleUser(),
                RecursInDirectories = Config.Instance.CompileExploreDirRecursiv
            };
            _currentCompil.OnCompilationEnded += CurrentCompilOnOnCompilationEnded;

            // start the compilation
            if (_currentCompil.CompileFolders(new List<string> {fl_directory.Text})) {

                btStart.Visible = false;
                progressBar.Visible = true;

                // start a recurrent event (every second) to update the progression of the compilation
                _progressTimer = new Timer();
                _progressTimer.Interval = 1000;
                _progressTimer.Tick += (o, args) => UpdateProgressBar();
                _progressTimer.Start();
            }
        }

        // called when the compilation ended
        private void CurrentCompilOnOnCompilationEnded() {

            UpdateProgressBar();

            // get rid of the timer
            _progressTimer.Stop();
            _progressTimer.Dispose();
            _progressTimer = null;

            UpdateReport("Compiling X files = " + _currentCompil.NbFilesToCompile + "<br>using X process " + _currentCompil.NumberOfProcesses + "<br>Ended ok = " + _currentCompil.NumberOfProcessesEndedOk);
        }

        #endregion

        #region private methods

        // allows to update the progression bar
        private void UpdateProgressBar() {
            var progression = _currentCompil.GetOverallProgression();
            progressBar.Text = progression + @"% (in " + _currentCompil.GetElapsedTime() + @")";
            progressBar.Progress = progression;
        }

        // update the report, activates the scroll bars when needed
        private void UpdateReport(string htmlContent) {
            // ensure it's visible 
            lbl_report.Visible = true;
            lbl_report.Text = htmlContent;

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
