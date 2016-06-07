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
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    internal partial class SetFtp : YamuiPage {

        #region fields

        #endregion

        #region constructor

        public SetFtp() {
            InitializeComponent();

            toolTip.SetToolTip(fl_host, "Name or IP address of the remote host to connect to");
            toolTip.SetToolTip(fl_user, "User name to be used in case of non anonymous connections");
            toolTip.SetToolTip(fl_password, "Password to be used in case of non anonymous connections");
            toolTip.SetToolTip(fl_remoteDir, "The directory on the FTP server on which you want to upload the compiled code");
            toolTip.SetToolTip(cb_info, "3P will automatically set the right option for you");
            toolTip.SetToolTip(fl_port, "TCP/IP connection port, default is 21 for standard FTP or explicit FTPS and 990 for implicit FTPS<br>Leave empty to use the default values");
            
        }

        #endregion

        public override void OnShow() {
            ActiveControl = bt_test;
            base.OnShow();
        }

        #region public

        #endregion

        #region private event

        #endregion

        #region private data management


        #endregion
    }

}
