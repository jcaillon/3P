#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CodeExplorer.cs) is part of 3P.
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
using _3PA.Images;
using _3PA.Interop;
using _3PA.MainFeatures.NppInterfaceForm;

namespace _3PA.MainFeatures.CodeExplorer {
    internal class CodeExplorer : NppDockableDialog<CodeExplorerForm> {
        #region Singleton

        private static CodeExplorer _instance;

        public static CodeExplorer Instance {
            get { return _instance ?? (_instance = new CodeExplorer()); }
            set { _instance = value; }
        }

        private CodeExplorer() {
            _dialogDescription = "Code explorer";
            _formDefaultPos = NppTbMsg.CONT_RIGHT;
            _iconImage = ImageResources.CodeExplorerLogo;
        }

        #endregion

        #region InitForm

        protected override void InitForm() {
            Form = new CodeExplorerForm(_fakeForm);
        }

        #endregion

        #region handling form

        /// <summary>
        /// Use this to redraw the docked form
        /// </summary>
        public void ApplyColorSettings() {
            if (Form == null)
                return;
            Form.YamuiList.ShowTreeBranches = Config.Instance.ShowTreeBranches;
            Form.Refresh();
        }

        /// <summary>
        /// update the "selected" scope when the user click in scintilla
        /// </summary>
        public void UpdateCurrentScope() {
            if (Form == null)
                return;
            Form.UpdateCurrentScope();
        }

        public override void UpdateMenuItemChecked() {
            base.UpdateMenuItemChecked();
            Config.Instance.CodeExplorerVisible = _fakeForm != null && _fakeForm.Visible;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Call this method to update the code explorer tree with the data from the Parser Handler
        /// </summary>
        public void OnParseEnded() {
            if (Form == null) return;
            Form.UpdateTreeData();
        }

        public void OnParseStarted() {
            if (Form == null) return;
            Form.Refreshing = true;
        }

        #endregion
    }
}