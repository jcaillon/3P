#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System;
using System.Runtime.InteropServices;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.NppInterfaceForm;

namespace _3PA.MainFeatures.CodeExplorer {
    internal static class CodeExplorer {

        #region Fields

        public static CodeExplorerForm Form { get; private set; }

        public static bool IsVisible {
            get { return Form != null && Form.Visible; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Call this method to update the code explorer tree with the data from the Parser Handler
        /// </summary>
        public static void UpdateCodeExplorer() {
            if (Form == null) return;
            Form.UpdateTreeData();
        }

        /// <summary>
        /// Just redraw the code explorer, it is used to update the "selected" scope when
        /// the user click in scintilla
        /// </summary>
        public static void RedrawCodeExplorerList() {
            if (Form == null) return;
            Form.Redraw();
        }

        /// <summary>
        /// Use this to redraw the docked form
        /// </summary>
        public static void ApplyColorSettings() {
            if (Form == null) return;
            Form.StyleOvlTree();
            Form.Refresh();
        }

        /// <summary>
        /// Use this to change the image of the refresh button to let the user know the tree is being refreshed
        /// </summary>
        public static bool Refreshing {
            set {
                if (Form == null) return;
                Form.Refreshing = value;
            }
        }

        #endregion

        #region DockableDialog

        public static EmptyForm FakeForm { get; private set; }
        public static int DockableCommandIndex;

        public static void Toggle(bool doShow) {
            if ((doShow && !IsVisible) || (!doShow && IsVisible)) {
                Toggle();
            }
        }

        /// <summary>
        /// Toggle the docked form on and off, can be called first and will initialize the form
        /// </summary>
        public static void Toggle() {
            try {
                // initialize if not done
                if (FakeForm == null) {
                    Init();
                } else {
                    WinApi.SendMessage(Npp.HandleNpp, !FakeForm.Visible ? NppMsg.NPPM_DMMSHOW : NppMsg.NPPM_DMMHIDE, 0, FakeForm.Handle);
                }
                Form.RefreshPosAndLoc();
                if (FakeForm == null) return;
                UpdateMenuItemChecked();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in Dockable explorer");
            }
        }

        /// <summary>
        /// Either check or uncheck the menu, depending on the visibility of the form
        /// (does it both on the menu and toolbar)
        /// </summary>
        public static void UpdateMenuItemChecked() {
            if (FakeForm == null) return;
            WinApi.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETMENUITEMCHECK, UnmanagedExports.FuncItems.Items[DockableCommandIndex]._cmdID, FakeForm.Visible ? 1 : 0);
            Config.Instance.CodeExplorerVisible = FakeForm.Visible;
        }

        /// <summary>
        /// Initialize the form
        /// </summary>
        public static void Init() {
            FakeForm = new EmptyForm();
            NppTbData nppTbData = new NppTbData {
                hClient = FakeForm.Handle,
                pszName = AssemblyInfo.AssemblyProduct + " - Code explorer",
                dlgID = DockableCommandIndex,
                uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                hIconTab = (uint) Utils.GetIconFromImage(ImageResources.CodeExplorerLogo).Handle,
                pszModuleName = AssemblyInfo.AssemblyProduct
            };
            IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
            Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);
            WinApi.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);
            Form = new CodeExplorerForm(FakeForm);
        }

        public static void ForceClose() {
            if (Form != null)
                Form.Close();
        }

        #endregion

    }
}
