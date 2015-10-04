using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;

namespace _3PA.MainFeatures.DockableExplorer {
    public class DockableExplorer {

        public static int DockableCommandIndex;

        public static DockableExplorerForm ExplorerForm { get; private set; }

        public static bool IsVisible {
            get { return ExplorerForm != null && ExplorerForm.Visible; }
        }

        /// <summary>
        /// Call this method to update the code explorer tree with the data from the Parser Handler
        /// </summary>
        public static void UpdateCodeExplorer() {
            if (ExplorerForm == null) return;
            ExplorerForm.CodeExplorer.RememberExpandedItems();
            CodeExplorer.UpdateTreeData();
            ExplorerForm.CodeExplorer.InitSetObjects();
            ExplorerForm.CodeExplorer.SetRememberedExpandedItems();
            ExplorerForm.CodeExplorer.ReapplyFilter();
        }

        /// <summary>
        /// Just redraw the code explorer, it is used to update the "selected" scope when
        /// the user click in scintilla
        /// </summary>
        public static void RedrawCodeExplorer() {
            if (ExplorerForm == null) return;
            ExplorerForm.CodeExplorer.Redraw();
        }

        /// <summary>
        /// Toggle the docked form on and off, can be called first and will initialize the form
        /// </summary>
        public static void Toggle() {
            // initialize if not done
            if (ExplorerForm == null)
                Init();
            else {
                Win32.SendMessage(Npp.HandleNpp, !ExplorerForm.Visible ? NppMsg.NPPM_DMMSHOW : NppMsg.NPPM_DMMHIDE, 0, ExplorerForm.Handle);
            }
            if (ExplorerForm == null) return;
            ExplorerForm.CodeExplorer.UseAlternativeBackColor = Config.Instance.ExplorerUseAlternateColors;
            UpdateMenuItemChecked();
        }

        /// <summary>
        /// Use this to redraw the docked form
        /// </summary>
        public static void Redraw() {
            if (IsVisible) {
                Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMUPDATEDISPINFO, 0, ExplorerForm.Handle);
                ExplorerForm.CodeExplorer.StyleOvlTree();
                ExplorerForm.Invalidate();
            }
        }

        /// <summary>
        /// Either check or uncheck the menu, depending on the visibility of the form
        /// (does it both on the menu and toolbar)
        /// </summary>
        public static void UpdateMenuItemChecked() {
            if (ExplorerForm == null) return;
            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETMENUITEMCHECK, Plug.FuncItems.Items[DockableCommandIndex]._cmdID, ExplorerForm.Visible ? 1 : 0);
            Config.Instance.ExplorerVisible = ExplorerForm.Visible;
        }

        /// <summary>
        /// Initialize the form
        /// </summary>
        public static void Init() {

            ExplorerForm = new DockableExplorerForm();

            // set "transparent" color
            Icon dockableIcon;
            using (Bitmap newBmp = new Bitmap(16, 16)) {
                Graphics g = Graphics.FromImage(newBmp);
                ColorMap[] colorMap = new ColorMap[1];
                colorMap[0] = new ColorMap();
                colorMap[0].OldColor = Color.White;
                colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                ImageAttributes attr = new ImageAttributes();
                attr.SetRemapTable(colorMap);
                g.DrawImage(ImageResources._3PA, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                dockableIcon = Icon.FromHandle(newBmp.GetHicon());
            }

            NppTbData nppTbData = new NppTbData {
                hClient = ExplorerForm.Handle,
                pszName = "Code explorer",
                dlgID = DockableCommandIndex,
                uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                hIconTab = (uint) dockableIcon.Handle,
                pszModuleName = Assembly.GetExecutingAssembly().GetName().Name
            };

            IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
            Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);

            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);

            UpdateCodeExplorer();
        }
    }
}
