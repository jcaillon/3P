#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (HomePage.cs) is part of 3P.
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
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using _3PA.Html;
using _3PA.Lib;

namespace _3PA.MainFeatures.Appli.Pages.Home {
    internal partial class HomePage : YamuiPage {

        #region fields

        #endregion

        #region constructor
        public HomePage() {
            InitializeComponent();
            html.Text = HtmlResources.home.Replace("%version%", AssemblyInfo.Version)
                .Replace("%disclaimer%", AssemblyInfo.IsPreRelease ? HtmlResources.disclaimer : "")
                .Replace("%getting-started.md%", HtmlResources.getting_started.MdToHtml());

            html.LinkClicked += HtmlOnLinkClicked;

            yamuiScrollPage1.ContentPanel.Height = html.Height;
        }

        private void HtmlOnLinkClicked(object sender, HtmlLinkClickedEventArgs htmlLinkClickedEventArgs) {
            if (htmlLinkClickedEventArgs.Link.Equals("update")) {
                UpdateHandler.CheckForUpdates();
                htmlLinkClickedEventArgs.Handled = true;
            }
        }

        #endregion

        public override void OnShow() {
            ActiveControl = html;
            base.OnShow();
        }
    }
}
