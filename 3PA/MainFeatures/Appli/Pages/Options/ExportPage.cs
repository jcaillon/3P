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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Controls;
using YamuiFramework.Forms;
using YamuiFramework.HtmlRenderer.WinForms;
using _3PA.Images;
using _3PA.Lib;

namespace _3PA.MainFeatures.Appli.Pages.Options {

    /// <summary>
    /// This page is built programatically
    /// </summary>
    internal partial class ExportPage : YamuiPage {

        #region fields

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
            tooltip.SetToolTip(btHistoric, "Click to <b>browse</b> the previous folders");

            btRefresh.BackGrndImage = ImageResources.refresh;
            btRefresh.ButtonPressed += BtRefreshOnButtonPressed;
            tooltip.SetToolTip(btRefresh, "Click to <b>refresh</b> the local and distant file status");

            btDownloadAll.BackGrndImage = ImageResources.DownloadAll;
            btDownloadAll.ButtonPressed += BtDownloadAllOnButtonPressed;
            btDownloadAll.Hide();
            tooltip.SetToolTip(btDownloadAll, "Click to <b>fetch</b> all the distant versions newer than the local versions");

            fl_directory.Text = Config.Instance.SharedConfFolder;

            // build the interface
            var iNbLine = 0;
            var yPos = btRefresh.Location.Y + 35;
            foreach (var confLine in ShareExportConf.List) {

                // label
                var label = new HtmlLabel {
                    AutoSizeHeightOnly = true,
                    BackColor = Color.Transparent,
                    Location = new Point(30, yPos + 2),
                    Size = new Size(185, 10),
                    IsSelectionEnabled = false,
                    Text = confLine.Label
                };
                tooltip.SetToolTip(label, "File or folder handled :<br>" + confLine.HandledItem);
                dockedPanel.ContentPanel.Controls.Add(label);

                // switch, auto update?
                var toggleControl = new YamuiCheckBox {
                    Location = new Point(215, yPos + 2),
                    Size = new Size(26, 15),
                    Text = @" ",
                    Checked = confLine.AutoUpdate,
                    Tag = confLine
                };
                toggleControl.CheckedChanged += ToggleControlOnCheckedChanged;
                dockedPanel.ContentPanel.Controls.Add(toggleControl);
                tooltip.SetToolTip(toggleControl, "Check this option to automatically fetch the most recent version of the file<br>This update occurs on notepad++ startup and each time you refresh the local/distant file status");

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
                    Location = new Point(270, yPos + 2),
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
                    Name = "bto_" + iNbLine
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
                    Name = "bti_" + iNbLine
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
                    Location = new Point(510, yPos + 2),
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
                    Name = "btz_" + iNbLine
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
            foreach (var confLine in ShareExportConf.List) {
                if (((YamuiImageButton) dockedPanel.ContentPanel.Controls["btm_" + iNbLine]).Visible) {
                    if (confLine.OnFetch != null)
                        confLine.OnFetch(confLine);
                }
                iNbLine++;
            }
            RefreshList();
        }

        private void ToggleControlOnCheckedChanged(object sender, EventArgs eventArgs) {
            var cb = (YamuiCheckBox)sender;
            var confLine = (ConfLine)cb.Tag;
            var list = Config.Instance.AutoUpdateConfList.Split(',').ToList();
            if (cb.Checked) {
                list.Add(confLine.Label);
            } else {
                if (list.Exists(s => s.Equals(confLine.Label)))
                    list.Remove(confLine.Label);
            }
            Config.Instance.AutoUpdateConfList = string.Join(",", list);
        }


        private void BtRefreshOnButtonPressed(object sender, EventArgs eventArgs) {
            RefreshList();
        }

        /// <summary>
        /// The historic button shows a menu that allows the user to select a previously selected folders
        /// </summary>
        private void BtHistoricOnButtonPressed(object sender, EventArgs eventArgs) {
            List<YamuiMenuItem> itemList = new List<YamuiMenuItem>();
            foreach (var path in Config.Instance.SharedConfHistoric.Split(',')) {
                if (!string.IsNullOrEmpty(path)) {
                    itemList.Add(new YamuiMenuItem {ItemImage = ImageResources.FolderType, ItemName = path, OnClic = () => {
                        BeginInvoke((Action) delegate {
                            fl_directory.Text = path;
                            RefreshList();
                        });
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
                    bool sharedDirOk = !string.IsNullOrEmpty(fl_directory.Text) && Directory.Exists(fl_directory.Text);

                    // we save the directory in the historic
                    if (sharedDirOk) {
                        var list = Config.Instance.SharedConfHistoric.Split(',').ToList();
                        if (list.Exists(s => s.Equals(fl_directory.Text)))
                            list.Remove(fl_directory.Text);
                        list.Insert(0, fl_directory.Text);
                        if (list.Count > 2)
                            list.RemoveAt(list.Count - 1);
                        Config.Instance.SharedConfHistoric = string.Join(",", list);
                    }

                    // update the info we have on each item of the list
                    ShareExportConf.UpdateList(fl_directory.Text);

                    // invoke on ui thread
                    BeginInvoke((Action)delegate {
                        var nbMaj = 0;
                        var iNbLine = 0;

                        foreach (var confLine in ShareExportConf.List) {

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
                            ((YamuiImageButton)dockedPanel.ContentPanel.Controls["btp_" + iNbLine]).Enabled = confLine.OnPush != null && confLine.LocalExists && sharedDirOk && (confLine.DistantTime.CompareTo(confLine.LocalTime) != 0 || confLine.LocalNbFiles != confLine.DistantNbFiles);

                            ((HtmlLabel)dockedPanel.ContentPanel.Controls["datel_" + iNbLine]).Text = confLine.LocalExists ? confLine.LocalTime.ToString("yyyy-MM-dd HH:mm:ss") : (confLine.IsDir ? "" : @"Not exported");

                            ((HtmlLabel)dockedPanel.ContentPanel.Controls["dated_" + iNbLine]).Text = confLine.DistantExists ? confLine.DistantTime.ToString("yyyy-MM-dd HH:mm:ss") : @"Not found";

                            // maj button
                            if (confLine.NeedUpdate) {
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

                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error while fetching distant files");
                }
            });
            _isCheckingDistant = false;
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
