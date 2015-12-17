#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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

namespace _3PA.MainFeatures.CodeExplorer {
    public class CodeExplorer {

        public static int DockableCommandIndex;

        public static CodeExplorerForm ExplorerForm { get; private set; }

        public static bool IsVisible {
            get { return ExplorerForm != null && ExplorerForm.Visible; }
        }

        /// <summary>
        /// Call this method to update the code explorer tree with the data from the Parser Handler
        /// </summary>
        public static void UpdateCodeExplorer() {
            if (ExplorerForm == null) return;
            ExplorerForm.CodeExplorerPage.UpdateTreeData();
        }

        /// <summary>
        /// Just redraw the code explorer, it is used to update the "selected" scope when
        /// the user click in scintilla
        /// </summary>
        public static void RedrawCodeExplorer() {
            if (ExplorerForm == null) return;
            ExplorerForm.CodeExplorerPage.Redraw();
        }

        /// <summary>
        /// Clear tree data
        /// </summary>
        public static void ClearTree() {
            if (ExplorerForm == null) return;
            ExplorerForm.CodeExplorerPage.ClearTree();
        }

        /// <summary>
        /// Apply the onDocumentChange
        /// </summary>
        public static void RefreshParserAndCodeExplorer() {
            if (ExplorerForm == null) return;
            ExplorerForm.CodeExplorerPage.RefreshParserAndCodeExplorer();
        }

        /// <summary>
        /// Toggle the docked form on and off, can be called first and will initialize the form
        /// </summary>
        public static void Toggle() {
            try {
                // initialize if not done
                if (ExplorerForm == null) {
                    Init();
                    UpdateCodeExplorer();
                } else {
                    Win32.SendMessage(Npp.HandleNpp, !ExplorerForm.Visible ? NppMsg.NPPM_DMMSHOW : NppMsg.NPPM_DMMHIDE, 0, ExplorerForm.Handle);
                }
                if (ExplorerForm == null) return;
                ExplorerForm.CodeExplorerPage.UseAlternativeBackColor = Config.Instance.GlobalUseAlternateBackColorOnGrid;
                UpdateMenuItemChecked();
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in Dockable explorer");
            }
        }

        /// <summary>
        /// Use this to redraw the docked form
        /// </summary>
        public static void Redraw() {
            if (IsVisible) {
                //Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMUPDATEDISPINFO, 0, ExplorerForm.Handle);
                ExplorerForm.CodeExplorerPage.StyleOvlTree();
                ExplorerForm.Invalidate();
                ExplorerForm.Refresh();
            }
        }

        /// <summary>
        /// Either check or uncheck the menu, depending on the visibility of the form
        /// (does it both on the menu and toolbar)
        /// </summary>
        public static void UpdateMenuItemChecked() {
            if (ExplorerForm == null) return;
            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETMENUITEMCHECK, Plug.FuncItems.Items[DockableCommandIndex]._cmdID, ExplorerForm.Visible ? 1 : 0);
            Config.Instance.CodeExplorerVisible = ExplorerForm.Visible;
        }

        /// <summary>
        /// Initialize the form
        /// </summary>
        public static void Init() {
            ExplorerForm = new CodeExplorerForm();
            NppTbData nppTbData = new NppTbData {
                hClient = ExplorerForm.Handle,
                pszName = AssemblyInfo.ProductTitle + " - Code explorer",
                dlgID = DockableCommandIndex,
                uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                hIconTab = (uint)Utils.GetIconFromImage(ImageResources.code_explorer_logo).Handle,
                pszModuleName = AssemblyInfo.ProductTitle
            };
            IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
            Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);
            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);
        }
    }
}
