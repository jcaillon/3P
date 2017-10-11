#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using _3PA.Lib;
using _3PA._Resource;

namespace _3PA.MainFeatures.Appli.Pages.Home {
    internal partial class HomePage : YamuiPage {
        #region fields

        #endregion

        #region constructor

        public HomePage() {
            InitializeComponent();

            var prolintVer = Updater<ProlintUpdaterWrapper>.Instance.LocalVersion;
            var proparseVer = Updater<ProparseUpdaterWrapper>.Instance.LocalVersion;
            var datadiggerVer = Updater<DataDiggerUpdaterWrapper>.Instance.LocalVersion;
                 

            html.Text = HtmlResources.home.Replace("%version%", AssemblyInfo.Version)
                .Replace("%disclaimer%", AssemblyInfo.IsPreRelease ? HtmlResources.disclaimer : "")
                .Replace("%YamuiFrameworkVersion%", LibLoader.GetYamuiAssemblyVersion())
                .Replace("%ProlintVersion%", prolintVer.Equals("v0") ? "*not installed*" : prolintVer)
                .Replace("%ProparseVersion%", proparseVer.Equals("v0") ? "*not installed*" : proparseVer)
                .Replace("%DataDiggerVersion%", datadiggerVer.Equals("v0") ? "*not installed*" : datadiggerVer)
                .Replace("%getting-started.md%", HtmlResources.getting_started.MdToHtml());

            html.LinkClicked += (sender, args) => {
                if (args.Link.Equals("update")) {
                    Appli.GoToPage(PageNames.OptionsUpdate);
                    args.Handled = true;
                }
            };

            yamuiScrollPage1.ContentPanel.Height = html.Height;
        }

        #endregion

        public override void OnShow() {
            ActiveControl = html;
            base.OnShow();
        }
    }
}