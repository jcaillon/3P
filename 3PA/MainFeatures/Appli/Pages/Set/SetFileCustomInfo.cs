#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (SetFileCustomInfo.cs) is part of 3P.
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
using System.Linq;
using YamuiFramework.Controls;
using _3PA.Lib;
using _3PA.MainFeatures.ModificationsTag;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    internal partial class SetFileCustomInfo : YamuiPage {
        #region fields

        private FileTagObject _locFileTagObject;
        private string _filename;

        #endregion

        #region constructor

        public SetFileCustomInfo() {
            InitializeComponent();

            // add event handlers
            btTemplate.ButtonPressed += BtTemplateOnButtonPressed;
            btTemplate.BackGrndImage = ImageResources.ModificationTagMenu;
            toolTip.SetToolTip(btTemplate, "Modify the template used in the modification tags feature");

            cb_info.SelectedIndexChangedByUser += SelectedIndexChanged;
            bt_ok.ButtonPressed += BtOkOnButtonPressed;
            bt_ok.BackGrndImage = ImageResources.Save;
            bt_cancel.ButtonPressed += BtCancelOnButtonPressed;
            bt_cancel.BackGrndImage = ImageResources.Cancel;
            bt_clear.ButtonPressed += BtClearOnButtonPressed;
            bt_clear.BackGrndImage = ImageResources.ClearAll;
            bt_default.ButtonPressed += BtDefaultOnButtonPressed;
            bt_default.BackGrndImage = ImageResources.UndoUserAction;
            bt_today.ButtonPressed += BtTodayOnButtonPressed;
            bt_today.BackGrndImage = ImageResources.Calendar;
            bt_today.Click += (sender, args) => { fl_correctionDate.Text = DateTime.Now.ToString("dd/MM/yy"); };
            bt_delete.ButtonPressed += BtDeleteOnButtonPressed;
            bt_delete.BackGrndImage = ImageResources.Delete;

            toolTip.SetToolTip(bt_today, "Click to automatically fill the <i>date</i> field");

            // register to the document changed event
            Plug.OnDocumentChangedEnd += OnShow;

            // changing a value set the state to not saved
            fl_appliName.TextChanged += YamuiTextBoxOnTextChanged;
            fl_appliVersion.TextChanged += YamuiTextBoxOnTextChanged;
            fl_bugId.TextChanged += YamuiTextBoxOnTextChanged;
            fl_workPackage.TextChanged += YamuiTextBoxOnTextChanged;
            fl_correctionNb.TextChanged += YamuiTextBoxOnTextChanged;
            fl_correctionDate.TextChanged += YamuiTextBoxOnTextChanged;
            fl_correctionDesc.TextChanged += YamuiTextBoxOnTextChanged;

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(this);
        }

        #endregion

        public override void OnShow() {
            // update the info displayed on the screen
            if (!DesignMode)
                UpdateInfo();

            base.OnShow();
        }

        #region public

        /// <summary>
        /// Call this method to update the content of the form according to the current document
        /// </summary>
        public void UpdateInfo() {
            _filename = Npp.CurrentFileInfo.FileName;

            // populate combobox
            var list = new List<ItemCombo> {
                new ItemCombo {DisplayText = "Last info", Nb = FileCustomInfo.LastTag},
                new ItemCombo {DisplayText = "Default info", Nb = FileCustomInfo.DefaultTag}
            };

            cb_info.DisplayMember = "DisplayText";
            cb_info.ValueMember = "Nb";

            if (FileCustomInfo.Contains(_filename)) {
                var currentList = FileCustomInfo.GetFileTagsList(_filename);
                _locFileTagObject = currentList.Last();

                var i = 2;
                var lastItemPos = 0;
                foreach (var fileTag in currentList.OrderByDescending(o => o.CorrectionNumber).ToList()) {
                    list.Add(new ItemCombo {DisplayText = _filename + " # " + fileTag.CorrectionNumber, Nb = fileTag.CorrectionNumber});
                    if (fileTag.CorrectionNumber.Equals(_locFileTagObject.CorrectionNumber))
                        lastItemPos = i;
                    i++;
                }

                cb_info.DataSource = list;
                cb_info.SelectedIndex = lastItemPos;
            } else {
                _locFileTagObject = FileCustomInfo.GetFileTags(Config.Instance.UseDefaultValuesInsteadOfLastValuesInEditTags ? FileCustomInfo.DefaultTag : FileCustomInfo.LastTag, "");

                cb_info.DataSource = list;
                cb_info.SelectedIndex = Config.Instance.UseDefaultValuesInsteadOfLastValuesInEditTags ? 1 : 0;
            }

            UpdateView();
            ActiveControl = cb_info;
        }

        #endregion

        #region private event

        private void BtTemplateOnButtonPressed(object sender1, EventArgs eventArgs) {
            ModificationTagTemplate.EditTemplate();
        }

        private void BtOkOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            Save(_filename);
            Save(FileCustomInfo.LastTag);
            UpdateInfo();
            FileCustomInfo.Export();
            Appli.ToggleView();
        }

        private void BtCancelOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            UpdateInfo();
            Appli.ToggleView();
        }

        private void BtClearOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            _locFileTagObject = new FileTagObject();
            UpdateView();
            FileHasChanged();
        }

        private void BtDefaultOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            Save(FileCustomInfo.DefaultTag);
        }

        private void BtTodayOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            fl_correctionDate.Text = DateTime.Now.ToString("dd/MM/yy");
        }

        private void BtDeleteOnButtonPressed(object sender, EventArgs eventArgs) {
            if (FileCustomInfo.DeleteFileTags(_filename, _locFileTagObject.CorrectionNumber))
                UpdateInfo();
        }

        /// <summary>
        /// called when the user changes the value of the combo box
        /// </summary>
        private void SelectedIndexChanged(YamuiComboBox sender) {
            var val = cb_info.SelectedValue.ToString();
            if (val.Equals(FileCustomInfo.LastTag) || val.Equals(FileCustomInfo.DefaultTag))
                _locFileTagObject = FileCustomInfo.GetFileTags(val, "");
            else {
                _locFileTagObject = FileCustomInfo.GetFileTags(_filename, val);
                FileCustomInfo.SetFileTags(_filename, _locFileTagObject.CorrectionNumber, _locFileTagObject.CorrectionDate, _locFileTagObject.CorrectionDecription, _locFileTagObject.ApplicationName, _locFileTagObject.ApplicationVersion, _locFileTagObject.WorkPackage, _locFileTagObject.BugId);
            }
            UpdateView();
        }

        private void YamuiTextBoxOnTextChanged(object sender, EventArgs eventArgs) {
            FileHasChanged();
        }

        #endregion

        #region private data management

        /// <summary>
        /// Save the info
        /// </summary>
        /// <param name="filename"></param>
        private void Save(string filename) {
            UpdateModel();
            FileCustomInfo.SetFileTags(filename, _locFileTagObject.CorrectionNumber, _locFileTagObject.CorrectionDate, _locFileTagObject.CorrectionDecription, _locFileTagObject.ApplicationName, _locFileTagObject.ApplicationVersion, _locFileTagObject.WorkPackage, _locFileTagObject.BugId);
        }

        private void UpdateModel() {
            _locFileTagObject.ApplicationName = fl_appliName.Text;
            _locFileTagObject.ApplicationVersion = fl_appliVersion.Text;
            _locFileTagObject.WorkPackage = fl_workPackage.Text;
            _locFileTagObject.BugId = fl_bugId.Text;
            _locFileTagObject.CorrectionNumber = fl_correctionNb.Text;
            _locFileTagObject.CorrectionDecription = fl_correctionDesc.Text.Replace("\r", "");
            _locFileTagObject.CorrectionDate = fl_correctionDate.Text;
        }

        private void UpdateView() {
            fl_appliName.Text = _locFileTagObject.ApplicationName;
            fl_appliVersion.Text = _locFileTagObject.ApplicationVersion;
            fl_workPackage.Text = _locFileTagObject.WorkPackage;
            fl_bugId.Text = _locFileTagObject.BugId;
            fl_correctionNb.Text = _locFileTagObject.CorrectionNumber;
            fl_correctionDesc.Text = (_locFileTagObject.CorrectionDecription ?? "");
            fl_correctionDate.Text = _locFileTagObject.CorrectionDate;

            lb_FileName.Text = @"<b>" + Npp.CurrentFileInfo.FileName + @"</b>";
            var val = cb_info.SelectedValue.ToString();
            if (!val.Equals(FileCustomInfo.LastTag) && !val.Equals(FileCustomInfo.DefaultTag)) {
                lb_SaveState.Text = @"<b>Info saved</b>";
                bt_SaveState.BackGrndImage = ImageResources.Ok;
                toolTip.SetToolTip(bt_SaveState, "3P has access to info on the current file");
            } else {
                FileHasChanged();
            }
        }

        private void FileHasChanged() {
            lb_SaveState.Text = @"<b>The visible info are not saved!</b>";
            bt_SaveState.BackGrndImage = ImageResources.NotSaved;
            toolTip.SetToolTip(bt_SaveState, "The info you see on this screen are not yet saved<br>and thus, not known to 3P<br>Click the <i>save and close</i> button");
        }

        #endregion
    }

    #region item combo struct

    public struct ItemCombo {
        public string DisplayText { get; set; }
        public string Nb { get; set; }
    }

    #endregion
}