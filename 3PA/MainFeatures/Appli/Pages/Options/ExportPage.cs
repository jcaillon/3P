#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (OptionPage.cs) is part of 3P.
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
using System.Threading.Tasks;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;
using _3PA.Html;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures.Appli.Pages.Options {

    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class ExportPage : YamuiPage {

        #region fields

        private List<ConfLine> _confList;

        private bool _isCheckingDistant;

        #endregion

        #region constructor
        public ExportPage() {
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
            tooltip.SetToolTip(btOpen, "Click to <b>browse</b> the previous folders");
            btHistoric.Hide();

            btRefresh.BackGrndImage = ImageResources.refresh;
            btRefresh.ButtonPressed += BtRefreshOnButtonPressed;
            tooltip.SetToolTip(btOpen, "Click to <b>update</b> the local and distant file status");

            btDownloadAll.BackGrndImage = ImageResources.DownloadAll;
            btDownloadAll.ButtonPressed += BtDownloadAllOnButtonPressed;
            tooltip.SetToolTip(btOpen, "Click to <b>fetch</b> all the distant versions newer than the local versions");

            fl_directory.Text = Config.Instance.SharedConfFolder;
            fl_directory.TextChanged += FlDirectoryOnTextChanged;

            // Configure each line
            _confList = new List<ConfLine> {
                new ConfLine {
                    Label = "List of environments",
                    HandledItem = Config.FileProEnv,
                    OnFetch = DoFetch,
                    OnPush = DoPush,
                    OnImport = line => ProEnvironment.Import()
                },
                new ConfLine {
                    Label = "List of snippets",
                    HandledItem = Config.FileSnippets
                },
                new ConfLine {
                    Label = "Compilation path rerouting",
                    HandledItem = Config.FileCompilPath
                },
                new ConfLine {
                    Label = "Start prolint procedure",
                    HandledItem = Config.FileStartProlint,
                    OnDelete = DoDelete,
                    OnFetch = DoFetch,
                    OnPush = DoPush
                },
                new ConfLine {
                    Label = "4GL keywords list",
                    HandledItem = Config.FileKeywordsList
                },
                new ConfLine {
                    Label = "4GL keywords help list",
                    HandledItem = Config.FileKeywordsHelp
                },
                new ConfLine {
                    Label = "4GL abbreviations list",
                    HandledItem = Config.FileAbbrev
                },
                new ConfLine {
                    Label = "Templates for new files",
                    HandledItem = Config.FolderTemplates,
                    IsDir = true,
                    OnFetch = DoFetch,
                    OnPush = DoPush
                }
            };

            // build the interface
            var iNbLine = 0;
            var yPos = btRefresh.Location.Y + 35;
            foreach (var confLine in _confList) {

                // label
                var label = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(30, yPos),
                    Size = new Size(190, 10),
                    IsSelectionEnabled = false,
                    Text = confLine.Label
                };
                tooltip.SetToolTip(label, "File or folder handled :<br>" + confLine.HandledItem);
                dockedPanel.ContentPanel.Controls.Add(label);

                // do we have an update available?
                var strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.OutDated,
                    Size = new Size(20, 20),
                    Location = new Point(240, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Name = "btm_" + iNbLine,
                    Visible = false
                };
                strButton.ButtonPressed += StrButtonOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "The distant version is more recent than the local one<br>Press this button to <b>fetch</b> the distant version");

                // local date
                var date = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(270, yPos),
                    Size = new Size(130, 10),
                    IsSelectionEnabled = false,
                    Text = @"???",
                    Name = "datel_" + iNbLine
                };
                dockedPanel.ContentPanel.Controls.Add(date);

                // local open
                strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.OpenInExplorer,
                    Size = new Size(20, 20),
                    Location = new Point(410, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Enabled = false,
                    Name = "bto_" + iNbLine,
                };
                strButton.ButtonPressed += OpenFileOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "Click to <b>open</b> this file / folder in the explorer");

                // local import
                strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.Import,
                    Size = new Size(20, 20),
                    Location = new Point(430, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Enabled = false,
                    Name = "bti_" + iNbLine,
                };
                strButton.ButtonPressed += StrButtonOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "Click to <b>import</b> this file<br>It reads its content and use it in this session of 3P");

                // local export
                strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.Export,
                    Size = new Size(20, 20),
                    Location = new Point(450, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Name = "bte_" + iNbLine,
                    Enabled = false
                };
                strButton.ButtonPressed += StrButtonOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "Click to <b>export</b> this file to a local version,<br>you will use the exported file instead of the embedded file in 3P");

                // local delete
                strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.Delete,
                    Size = new Size(20, 20),
                    Location = new Point(470, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Name = "btd_" + iNbLine,
                    Enabled = false
                };
                strButton.ButtonPressed += StrButtonOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "Click to <b>delete</b> the local version of your file,<br>you will use the embedded (default) file of 3P instead");

                // distant date
                date = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(510, yPos),
                    Size = new Size(130, 10),
                    IsSelectionEnabled = false,
                    Text = @"???",
                    Name = "dated_" + iNbLine
                };
                dockedPanel.ContentPanel.Controls.Add(date);

                // distant open
                strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.OpenInExplorer,
                    Size = new Size(20, 20),
                    Location = new Point(650, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Enabled = false,
                    Name = "btz_" + iNbLine,
                };
                strButton.ButtonPressed += OpenFileOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "Click to <b>open</b> this file / folder in the explorer");

                // distant fetch
                strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.Fetch,
                    Size = new Size(20, 20),
                    Location = new Point(670, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Name = "btf_" + iNbLine,
                    Enabled = false
                };
                strButton.ButtonPressed += StrButtonOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "Click to <b>fetch</b> this file from the shared directory,<br>replacing the local one");

                // distant push
                strButton = new YamuiImageButton {
                    BackGrndImage = ImageResources.Push,
                    Size = new Size(20, 20),
                    Location = new Point(690, yPos),
                    Tag = confLine,
                    TabStop = false,
                    Name = "btp_" + iNbLine,
                    Enabled = false
                };
                strButton.ButtonPressed += StrButtonOnButtonPressed;
                dockedPanel.ContentPanel.Controls.Add(strButton);
                tooltip.SetToolTip(strButton, "Click to <b>push</b> the local file to the shared directory,<br>replacing any existing file");


                yPos += label.Height + 15;
                iNbLine++;
            }
        }

        #endregion

        #region generic methods for the buttons

        private void DoDelete(ConfLine conf) {
            var answ = UserCommunication.Message("Do you really want to delete this file, or did your finger slipped?", MessageImg.MsgQuestion, "Delete", "Confirmation", new List<string> { "Yes I do", "No, Cancel" }, true);
            if (answ == 0) {
                Utils.DeleteFile(conf.LocalPath);
                RefreshList();
            }
        }

        private void DoFetch(ConfLine conf) {
            if (!string.IsNullOrEmpty(conf.DistantPath)) {
                if (conf.IsDir)
                    Utils.CopyDirectory(conf.DistantPath, conf.LocalPath);
                else {
                    Utils.CopyFile(conf.DistantPath, conf.LocalPath);
                    if (conf.OnImport != null)
                        conf.OnImport(conf);
                }
                RefreshList();
            }
        }

        private void DoPush(ConfLine conf) {
            if (!string.IsNullOrEmpty(conf.LocalPath)) {
                if (conf.IsDir)
                    Utils.CopyDirectory(conf.LocalPath, conf.DistantPath);
                else
                    Utils.CopyFile(conf.LocalPath, conf.DistantPath);
                RefreshList();
            }
        }

        #endregion

        #region events

        /// <summary>
        /// Called when the page is shown
        /// </summary>
        public override void OnShow() {
            RefreshList();
        }

        /// <summary>
        /// Called by any line button
        /// </summary>
        private void StrButtonOnButtonPressed(object sender, EventArgs eventArgs) {
            // find the corresponding control
            var button = (YamuiImageButton) sender;
            if (button == null) return;
            var confLine = (ConfLine) button.Tag;

            if (button.Name.StartsWith("btm_")) {
                if (confLine.OnFetch != null) confLine.OnFetch(confLine);
            } else if (button.Name.StartsWith("bte_")) {
                if (confLine.OnExport != null) confLine.OnExport(confLine);
            } else if (button.Name.StartsWith("btd_")) {
                if (confLine.OnDelete != null) confLine.OnDelete(confLine);
            } else if (button.Name.StartsWith("btf_")) {
                if (confLine.OnFetch != null) confLine.OnFetch(confLine);
            } else if (button.Name.StartsWith("btp_")) {
                if (confLine.OnPush != null) confLine.OnPush(confLine);
            } else if (button.Name.StartsWith("bti_")) {
                if (confLine.OnImport != null) confLine.OnImport(confLine);
            }
        }

        private void FlDirectoryOnTextChanged(object sender, EventArgs eventArgs) {
            RefreshList();
        }

        private void OpenFileOnButtonPressed(object sender, EventArgs eventArgs) {
            var conf = (ConfLine)((YamuiImageButton)sender).Tag;
            var pathToOpen = ((YamuiImageButton) sender).Name.StartsWith("bto_") ? conf.LocalPath : conf.DistantPath;
            if (conf.IsDir)
                Utils.OpenFolder(pathToOpen);
            else
                Utils.OpenFileInFolder(pathToOpen);
        }

        /// <summary>
        /// updates all the local stuff
        /// </summary>
        private void BtDownloadAllOnButtonPressed(object sender, EventArgs eventArgs) {
            var iNbLine = 0;
            foreach (var confLine in _confList) {
                if (((YamuiImageButton) dockedPanel.ContentPanel.Controls["btm_" + iNbLine]).Visible) {
                    if (confLine.OnFetch != null)
                        confLine.OnFetch(confLine);
                }
                iNbLine++;
            }
        }

        private void BtRefreshOnButtonPressed(object sender, EventArgs eventArgs) {
            RefreshList();
        }

        private void BtHistoricOnButtonPressed(object sender, EventArgs eventArgs) {
            
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
                RefreshList();
            }
        }

        #endregion

        #region private methods

        private void RefreshList() {
            if (_isCheckingDistant) return;
            _isCheckingDistant = true;
            Task.Factory.StartNew(() => {
                try {
                    RefreshListAction();
                    _isCheckingDistant = false;
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error while fetching distant files");
                    _isCheckingDistant = false;
                }
            });

        }

        public void RefreshListAction() {

            // We get the latest info for each line
            bool sharedDirOk = false;
            if (!string.IsNullOrEmpty(fl_directory.Text) && Directory.Exists(fl_directory.Text)) {
                sharedDirOk = true;
                Config.Instance.SharedConfFolder = fl_directory.Text;
            }

            foreach (var confLine in _confList) {
                confLine.LocalPath = confLine.HandledItem;
                confLine.DistantPath = sharedDirOk ? Path.Combine(fl_directory.Text, confLine.HandledItem.Replace(Npp.GetConfigDir(), "").Trim('\\')) : "";

                confLine.LocalTime = DateTime.Now;
                confLine.DistantTime = DateTime.Now;

                if (confLine.IsDir) {
                    confLine.LocalExists = true;
                    confLine.DistantExists = !string.IsNullOrEmpty(confLine.DistantPath) && Directory.Exists(confLine.DistantPath);

                    if (confLine.LocalExists) {
                        confLine.LocalNbFiles = 0;
                        foreach (var file in Directory.GetFiles(confLine.LocalPath)) {
                            if (confLine.LocalNbFiles == 0)
                                confLine.LocalTime = File.GetLastWriteTime(file);
                            else if (File.GetLastWriteTime(file).CompareTo(confLine.LocalTime) > 0)
                                confLine.LocalTime = File.GetLastWriteTime(file);
                            confLine.LocalNbFiles++;
                        }
                    }

                    if (!string.IsNullOrEmpty(confLine.DistantPath) && confLine.DistantExists) {
                        confLine.DistantNbFiles = 0;
                        foreach (var file in Directory.GetFiles(confLine.DistantPath)) {
                            if (confLine.DistantNbFiles == 0)
                                confLine.DistantTime = File.GetLastWriteTime(file);
                            else if (File.GetLastWriteTime(file).CompareTo(confLine.DistantTime) > 0)
                                confLine.DistantTime = File.GetLastWriteTime(file);
                            confLine.DistantNbFiles++;
                        }
                    }

                } else {
                    confLine.LocalExists = !string.IsNullOrEmpty(confLine.LocalPath) && File.Exists(confLine.LocalPath);
                    confLine.DistantExists = !string.IsNullOrEmpty(confLine.DistantPath) && File.Exists(confLine.DistantPath);

                    if (confLine.LocalExists) {
                        confLine.LocalTime = File.GetLastWriteTime(confLine.LocalPath);
                    }

                    if (!string.IsNullOrEmpty(confLine.DistantPath) && confLine.DistantExists) {
                        confLine.DistantTime = File.GetLastWriteTime(confLine.DistantPath);
                    }

                }
            }

            
            // invoke on ui thread
            BeginInvoke((Action) delegate {
                var nbMaj = 0;
                var iNbLine = 0;

                foreach (var confLine in _confList) {

                    // open
                    ((YamuiImageButton)dockedPanel.ContentPanel.Controls["bto_" + iNbLine]).Enabled = confLine.LocalExists;

                    // import
                    ((YamuiImageButton)dockedPanel.ContentPanel.Controls["bti_" + iNbLine]).Enabled = confLine.OnImport != null && confLine.LocalExists;

                    if (confLine.IsDir) {
                        // hide export/delete
                        ((YamuiImageButton)dockedPanel.ContentPanel.Controls["bte_" + iNbLine]).Hide();
                        ((YamuiImageButton)dockedPanel.ContentPanel.Controls["btd_" + iNbLine]).Hide();
                    } else {
                        // export
                        ((YamuiImageButton)dockedPanel.ContentPanel.Controls["bte_" + iNbLine]).Enabled = confLine.OnExport != null && !confLine.LocalExists;

                        // delete
                        ((YamuiImageButton)dockedPanel.ContentPanel.Controls["btd_" + iNbLine]).Enabled = confLine.OnDelete != null && confLine.LocalExists;
                    }

                    // distant open
                    ((YamuiImageButton)dockedPanel.ContentPanel.Controls["btz_" + iNbLine]).Enabled = confLine.DistantExists;

                    // fetch
                    ((YamuiImageButton)dockedPanel.ContentPanel.Controls["btf_" + iNbLine]).Enabled = confLine.OnFetch != null && confLine.DistantExists && (confLine.DistantTime.CompareTo(confLine.LocalTime) != 0 || confLine.LocalNbFiles != confLine.DistantNbFiles);

                    // push
                    ((YamuiImageButton)dockedPanel.ContentPanel.Controls["btp_" + iNbLine]).Enabled = confLine.OnPush != null && confLine.LocalExists && (confLine.DistantTime.CompareTo(confLine.LocalTime) != 0 || confLine.LocalNbFiles != confLine.DistantNbFiles);

                    ((HtmlLabel)dockedPanel.ContentPanel.Controls["datel_" + iNbLine]).Text = confLine.LocalExists ? confLine.LocalTime.ToString("yyyy-MM-dd HH:mm:ss") : (confLine.IsDir ? "" : @"Not exported");

                    ((HtmlLabel)dockedPanel.ContentPanel.Controls["dated_" + iNbLine]).Text = confLine.DistantExists ? confLine.DistantTime.ToString("yyyy-MM-dd HH:mm:ss") : @"Not found";

                    // maj button
                    bool majNeeded = (confLine.DistantExists && !confLine.LocalExists) || (confLine.LocalExists && confLine.DistantExists && confLine.DistantTime.CompareTo(confLine.LocalTime) > 0);
                    if (confLine.OnFetch != null && majNeeded) {
                        nbMaj++;
                        ((YamuiImageButton)dockedPanel.ContentPanel.Controls["btm_" + iNbLine]).Show();
                    } else
                        ((YamuiImageButton)dockedPanel.ContentPanel.Controls["btm_" + iNbLine]).Hide();

                    iNbLine++;
                }

                // download all button
                if (nbMaj > 0)
                    btDownloadAll.Show();
                else
                    btDownloadAll.Hide();
            });
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

        #endregion

        #region ConfLine class

        private class ConfLine {
            public string HandledItem { get; set; }
            public string Label { get; set; }
            public Action<ConfLine> OnDelete { get; set; }
            public Action<ConfLine> OnExport { get; set; }
            public Action<ConfLine> OnFetch { get; set; }
            public Action<ConfLine> OnPush { get; set; }
            public Action<ConfLine> OnImport { get; set; }
            public DateTime LocalTime { get; set; }
            public DateTime DistantTime { get; set; }
            public bool LocalExists { get; set; }
            public bool DistantExists { get; set; }
            public string LocalPath { get; set; }
            public string DistantPath { get; set; }
            public bool IsDir { get; set; }
            public int LocalNbFiles { get; set; }
            public int DistantNbFiles { get; set; }
        }

        #endregion

    }


}
