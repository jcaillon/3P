#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileExplorerForm.cs) is part of 3P.
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
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace _3PA.MainFeatures.FileExplorer {
    public partial class FileExplorerForm : Form {

        #region fields

        /// <summary>
        /// Access the FileExplorerPage component
        /// </summary>
        public FileExplorerPage FileExplorerPage {
            get { return _fileExplorerPage; }
        }

        #endregion

        #region constructor

        public FileExplorerForm() {
            InitializeComponent();

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);


            StartPosition = FormStartPosition.Manual;
            Location = new Point(-1000, -1000);
        }

        #endregion


        #region Paint Methods
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e) {
            var backColor = ThemeManager.Current.FormColorBackColor;
            e.Graphics.Clear(backColor);
        }
        #endregion

        #region events

        /// <summary>
        /// Check/uncheck the menu depending on this form visibility
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e) {
            CodeExplorer.CodeExplorer.UpdateMenuItemChecked();
            base.OnVisibleChanged(e);
        }

        #endregion

    }
}
