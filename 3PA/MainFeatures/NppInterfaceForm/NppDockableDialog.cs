#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppDockableDialog.cs) is part of 3P.
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
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.NppInterfaceForm {
    internal class NppDockableDialog<T> where T : NppDockableDialogForm {
        #region private

        protected string _dialogDescription = "?";

        protected NppTbMsg _formDefaultPos = NppTbMsg.CONT_LEFT;

        protected Image _iconImage = ImageResources.FileExplorerLogo;

        protected T Form { get; set; }

        protected NppDockableDialogEmptyForm _fakeForm;

        #endregion

        #region Fields

        /// <summary>
        /// Does the form exists and is visible?
        /// </summary>
        public bool IsVisible {
            get { return !(Form == null || !(bool) Form.SafeSyncInvoke(form => form.Visible)); }
        }

        public int DockableCommandIndex { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the form, should set RealForm = new T()
        /// </summary>
        protected virtual void InitForm() {}

        public void Toggle(bool doShow) {
            if ((doShow && !IsVisible) || (!doShow && IsVisible)) {
                Toggle();
            }
        }

        /// <summary>
        /// Toggle the docked form on and off, can be called first and will initialize the form
        /// </summary>
        public void Toggle() {
            try {
                // initialize if not done
                if (_fakeForm == null) {
                    // register fake form to Npp
                    _fakeForm = new NppDockableDialogEmptyForm();
                    NppTbData nppTbData = new NppTbData {
                        hClient = _fakeForm.Handle,
                        pszName = AssemblyInfo.AssemblyProduct + " - " + _dialogDescription,
                        dlgID = DockableCommandIndex,
                        uMask = _formDefaultPos | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                        hIconTab = (uint) Utils.GetIconFromImage(_iconImage).Handle,
                        pszModuleName = AssemblyInfo.AssemblyProduct
                    };
                    Npp.RegisterDockableDialog(nppTbData);
                    InitForm();
                } else {
                    if (_fakeForm.Visible)
                        Npp.HideDockableDialog(_fakeForm.Handle);
                    else
                        Npp.ShowDockableDialog(_fakeForm.Handle);
                }
                Form.RefreshPosAndLoc();
                if (_fakeForm == null) return;
                UpdateMenuItemChecked();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error loading " + _dialogDescription);
            }
        }

        /// <summary>
        /// Either check or uncheck the menu, depending on the visibility of the form
        /// (does it both on the menu and toolbar)
        /// </summary>
        public virtual void UpdateMenuItemChecked() {
            if (_fakeForm == null)
                return;
            Npp.SetMenuItemCheck(UnmanagedExports.NppFuncItems.Items[DockableCommandIndex]._cmdID, _fakeForm.Visible);
        }

        public void ForceClose() {
            if (Form != null)
                Form.Close();
            Form = null;
        }

        #endregion
    }
}