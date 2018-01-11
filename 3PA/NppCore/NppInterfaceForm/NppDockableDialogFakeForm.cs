#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppDockableDialogFakeForm.cs) is part of 3P.
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using YamuiFramework.Helper;
using _3PA.MainFeatures;

namespace _3PA.NppCore.NppInterfaceForm {
    /// <summary>
    /// The form to be registered to npp
    /// </summary>
    internal class NppDockableDialogFakeForm : NppEmptyForm {
        #region Events

        public event Action OnDockableDialogClose;

        #endregion

        #region Properties

        public override Color BackColor {
            get { return ThemeManager.Current.FormBack; }
        }

        #endregion

        #region WndProc

        protected override void WndProc(ref Message m) {
            //Listen for the closing of the dockable panel to toggle the toolbar icon
            switch (m.Msg) {
                case (int) WinApi.Messages.WM_NOTIFY:
                    var notify = (WinApi.NMHDR) Marshal.PtrToStructure(m.LParam, typeof(WinApi.NMHDR));
                    if (notify.code == (int) DockMgrMsg.DMN_CLOSE) {
                        if (OnDockableDialogClose != null)
                            OnDockableDialogClose();
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        #endregion
    }
}