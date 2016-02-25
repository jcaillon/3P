#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (SetCompilationPath.cs) is part of 3P.
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
using System.IO;
using YamuiFramework.Controls;
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    internal partial class SetCompilationPath : YamuiPage {

        #region fields

        #endregion

        #region constructor

        public SetCompilationPath() {
            InitializeComponent();

            // tooltips
            toolTip.SetToolTip(bt_import, "Click this button to <b>import</b> the last changes made to the<br>compilation path file");
            toolTip.SetToolTip(bt_modify, "Click this button to <b>open</b> the compilation path file<br>you will be able to modify at your will through Notepad++");

            bt_import.ButtonPressed += BtImportOnButtonPressed;
            bt_modify.ButtonPressed += BtModifyOnButtonPressed;
        }

        #endregion

        #region On show

        public override void OnShow() {
            UpdateList();
            base.OnShow();
        }

        #endregion


        #region private methods

        private void UpdateList() {
            // build the html
            html_list.Text = ProCompilePath.BuildHtmlTable();

            dockedPanel.ContentPanel.Height = html_list.Location.Y + html_list.Height;
            dockedPanel.OnResizedContentPanel();
        }

        #endregion


        #region private event

        private void BtModifyOnButtonPressed(object sender, EventArgs eventArgs) {
            if (!File.Exists(Config.FileCompilPath))
                Utils.FileWriteAllBytes(Config.FileCompilPath, DataResources.CompilationPath);

            Npp.OpenFile(Config.FileCompilPath);
        }

        private void BtImportOnButtonPressed(object sender, EventArgs eventArgs) {
            ProCompilePath.Import();
            UpdateList();
        }

        #endregion
    }
}
