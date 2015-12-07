#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileTagsForm.cs) is part of 3P.
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
using _3PA.Html;
using _3PA.MainFeatures.NppInterfaceForm;

namespace _3PA.MainFeatures.FilesInfo {
    public partial class FileTagsForm : NppInterfaceYamuiForm {
        public FileTagsForm() {
            InitializeComponent();
            lblTitle.Text = @"<img src='" + LocalHtmlHandler.GetLogo() + @"' style='padding-right: 10px'><span class='AppliTitle'>Update file information</span>";
        }

        public void UpdateForm() {
            fileTagsPage1.UpdateInfo();
        }
    }
}
