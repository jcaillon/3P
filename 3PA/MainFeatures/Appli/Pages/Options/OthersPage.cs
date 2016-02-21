#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (SetFileInfo.cs) is part of 3P.
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
using YamuiFramework.Controls;
using _3PA.MainFeatures.FilesInfoNs;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    internal partial class OthersPage : YamuiPage {

        #region fields

        public FileTagObject LocFileTagObject;
        public string Filename;

        #endregion

        #region constructor

        public OthersPage() {
            InitializeComponent();

            var usableVar = "<br><br>You can use the following values (taken from the file info form) :<br>{&a} : application<br>{&v} version of the application<br>{&w} : workpackage<br>{&b} : bug ID<br>{&n} : correction number<br>{&da} : correction date<br>{&u} : username<br>{&de} : description of the correction<br><br>If you leave spaces within the brackets (e.g. {&u  }) you can force the maximum space a text can take<br>(in this example, it can take 6 characters since there are 2 white spaces)<br>If there are no spaces, it can take the space needed<br><br><i>I highly recommand you to copy this in notepad++, edit it there<br>and paste it back here once your are done...</i>";

            toolTip.SetToolTip(fl_tagopen, "You can set your custom modification tag here,<br>this part will be added before your selection" + usableVar);
            toolTip.SetToolTip(fl_tagclose, "You can set your custom modification tag here,<br>this part will be appended to your selection" + usableVar);
            toolTip.SetToolTip(fl_tagtitle1, "This block is repeated only once at the beggining of the title block." + usableVar);
            toolTip.SetToolTip(fl_tagtitle2, "This block should contain the {&de} (description) variable and will be repeated as many times it is needed to display the complete description.");
            toolTip.SetToolTip(fl_tagtitle3, "This block is repeated only once at the end of the title block." + usableVar);

            bttagcancel.ButtonPressed += BttagcancelOnButtonPressed;
            bttagsave.ButtonPressed += BttagsaveOnButtonPressed;

            UpdateView();
        }

        #endregion

        #region private event

        private void UpdateView() {
            fl_tagopen.Text = Config.Instance.TagModifOpener;
            fl_tagclose.Text = Config.Instance.TagModifCloser;
            fl_tagtitle1.Lines = Config.Instance.TagTitleBlock1.Split('\n');
            fl_tagtitle2.Lines = Config.Instance.TagTitleBlock2.Split('\n');
            fl_tagtitle3.Lines = Config.Instance.TagTitleBlock3.Split('\n');
        }

        private void UpdateModel() {
            Config.Instance.TagModifOpener = fl_tagopen.Text;
            Config.Instance.TagModifCloser = fl_tagclose.Text;
            Config.Instance.TagTitleBlock1 = fl_tagtitle1.Text;
            Config.Instance.TagTitleBlock2 = fl_tagtitle2.Text;
            Config.Instance.TagTitleBlock3 = fl_tagtitle3.Text;
        }

        private void BttagsaveOnButtonPressed(object sender, EventArgs eventArgs) {
            UpdateModel();
        }

        private void BttagcancelOnButtonPressed(object sender, EventArgs eventArgs) {
            UpdateView();
        }

        #endregion
    }
}
