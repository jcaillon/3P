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