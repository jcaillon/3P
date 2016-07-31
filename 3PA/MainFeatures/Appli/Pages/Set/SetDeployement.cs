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
using System.Linq;
using System.Text;
using YamuiFramework.Controls;
using _3PA.Data;
using _3PA.Lib;
using _3PA.MainFeatures.Pro;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    internal partial class SetDeployement : YamuiPage {

        #region fields

        #endregion

        #region constructor

        public SetDeployement() {
            InitializeComponent();

            // tooltips
            toolTip.SetToolTip(bt_import, "Click this button to <b>import</b> the last changes made to the<br>compilation path file");
            toolTip.SetToolTip(bt_modify, "Click this button to <b>open</b> the compilation path file<br>you will be able to modify at your will through Notepad++");

            bt_import.ButtonPressed += BtImportOnButtonPressed;
            bt_modify.ButtonPressed += BtModifyOnButtonPressed;

            // dynamically reorder the controls for a correct tab order on notepad++
            SetTabOrder.RemoveAndAddForTabOrder(scrollPanel);
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
            html_list.Text = BuildHtmlTable();

            scrollPanel.ContentPanel.Height = html_list.Location.Y + html_list.Height;
            scrollPanel.OnResizedContentPanel();
        }

        /// <summary>
        /// returns a string containing an html representation of the compilation path table
        /// </summary>
        private string BuildHtmlTable() {
            var strBuilder = new StringBuilder();

            if (Deployer.GetDeployRulesList.Any()) {

                strBuilder.Append("<table width='100%;'>");
                strBuilder.Append("<tr><td class='CompPathHead' align='center' style='padding-right: 15px; padding-right: 15px;'>Application</td><td class='CompPathHead' align='center' style='padding-right: 15px; padding-right: 15px;'>Suffix</td><td class='CompPathHead' width='40%'>Source path pattern</td><td class='CompPathHead' align='center'>Transfer type</td><td class='CompPathHead' width='40%'>Deployement target</td></tr>");
                foreach (var compLine in Deployer.GetDeployRulesList) {
                    strBuilder.Append("<tr><td align='center'>" + (string.IsNullOrEmpty(compLine.ApplicationFilter) ? "*" : compLine.ApplicationFilter) + "</td><td align='center'>" + (string.IsNullOrEmpty(compLine.EnvLetterFilter) ? "*" : compLine.EnvLetterFilter) + "</td><td>" + (compLine.SourcePattern.Length > 40 ? "..." + compLine.SourcePattern.Substring(compLine.SourcePattern.Length - 40) : compLine.SourcePattern) + "</td><td align='center'>" + compLine.Type + "</td><td>" + (compLine.DeployTarget.Length > 40 ? "..." + compLine.SourcePattern.Substring(compLine.DeployTarget.Length - 40) : compLine.DeployTarget) + "</td></tr>");
                }
                strBuilder.Append("</table>");

            } else {
                strBuilder.Append("<b>Start by clicking the <i>modify</i> button</b><br>When you are done modying the file, save it and click the <i>read changes</i> button to import it into 3P");
            }

            return strBuilder.ToString();
        }

        #endregion

        #region private event

        private void BtModifyOnButtonPressed(object sender, EventArgs eventArgs) {
            if (!File.Exists(Config.FileDeployement))
                Utils.FileWriteAllBytes(Config.FileDeployement, DataResources.DeployementRules);

            Npp.OpenFile(Config.FileDeployement);
        }

        private void BtImportOnButtonPressed(object sender, EventArgs eventArgs) {
            Deployer.Import();
            UpdateList();
        }

        #endregion
    }
}
