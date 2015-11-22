#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (CodeExplorer.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
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
                ExplorerForm.CodeExplorerPage.UseAlternativeBackColor = Config.Instance.CodeExplorerUseAlternateColors;
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

            // set "transparent" color
            Icon dockableIcon;
            using (Bitmap newBmp = new Bitmap(16, 16)) {
                Graphics g = Graphics.FromImage(newBmp);
                ColorMap[] colorMap = new ColorMap[1];
                colorMap[0] = new ColorMap();
                colorMap[0].OldColor = Color.Transparent;
                colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                ImageAttributes attr = new ImageAttributes();
                attr.SetRemapTable(colorMap);
                g.DrawImage(ImageResources.code_explorer_logo, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                dockableIcon = Icon.FromHandle(newBmp.GetHicon());
            }

            NppTbData nppTbData = new NppTbData {
                hClient = ExplorerForm.Handle,
                pszName = AssemblyInfo.ProductTitle + " - Code explorer",
                dlgID = DockableCommandIndex,
                uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                hIconTab = (uint) dockableIcon.Handle,
                pszModuleName = AssemblyInfo.ProductTitle
            };

            IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
            Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);

            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);
        }
    }
}
