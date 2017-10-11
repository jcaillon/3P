#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (OthersPage.cs) is part of 3P.
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
using System.Linq;
using YamuiFramework.Controls;
using _3PA.Lib;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    internal partial class OthersPage : YamuiPage {
        #region constructor

        public OthersPage() {
            InitializeComponent();

            toolTip.SetToolTip(cbEncoding, "Choose the encoding to apply to the files when they are opened<br><i>The default option is 'Automatic', to let Notepad++ select the encoding</i>");
            toolTip.SetToolTip(fl_encodingfilter, "<i>Leave empty to disable this feature (default)</i><br>A comma (,) separated list of filters :<br>when a file is opened, if it matches one of the filter, the selected encoding is applied<br><br>Example of filter :<div class='ToolTipcodeSnippet'>*.p,\\*my_sub_directory\\*,*.r</div>");

            btCancel.BackGrndImage = ImageResources.UndoUserAction;
            btCancel.ButtonPressed += BtCancelOnButtonPressed;

            btSave.BackGrndImage = ImageResources.Save;
            btSave.ButtonPressed += BtSaveOnButtonPressed;

            cbEncoding.DataSource = Enum.GetNames(typeof(NppEncodingFormat)).OrderBy(s => s).Select(s => s.Replace("_", " ")).ToNonNullList();

            UpdateView();

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
        }

        #endregion

        public override void OnShow() {
            ActiveControl = btSave;
            base.OnShow();
        }

        #region private event

        private void UpdateView() {
            fl_encodingfilter.Text = Config.Instance.AutoSwitchEncodingForFilePatterns;
            cbEncoding.SelectedIndex = Enum.GetNames(typeof(NppEncodingFormat)).OrderBy(s => s).IndexOf(Config.Instance.AutoSwitchEncodingTo.ToString());
        }

        private void UpdateModel() {
            Config.Instance.AutoSwitchEncodingForFilePatterns = fl_encodingfilter.Text;
            NppEncodingFormat format;
            if (Enum.TryParse(Enum.GetNames(typeof(NppEncodingFormat)).OrderBy(s => s).ToList()[cbEncoding.SelectedIndex], true, out format))
                Config.Instance.AutoSwitchEncodingTo = format;
        }

        private void BtSaveOnButtonPressed(object sender, EventArgs eventArgs) {
            UpdateModel();
        }

        private void BtCancelOnButtonPressed(object sender, EventArgs eventArgs) {
            UpdateView();
        }

        #endregion
    }
}