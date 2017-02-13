using System;
using System.Drawing;
using System.Runtime.InteropServices;
using YamuiFramework.Helper;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;

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
            get { return !(Form == null || !(bool)Form.SafeSyncInvoke(form => form.Visible)); }
        }
        
        public int DockableCommandIndex { get; set; }
        
        #endregion

        #region Methods

        /// <summary>
        /// Initialize the form, should set RealForm = new T()
        /// </summary>
        protected virtual void Init() {

        }

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
                        hIconTab = (uint)Utils.GetIconFromImage(_iconImage).Handle,
                        pszModuleName = AssemblyInfo.AssemblyProduct
                    };
                    IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
                    Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);
                    Win32Api.SendMessage(Npp.HandleNpp, NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);
                    Init();
                } else {
                    Win32Api.SendMessage(Npp.HandleNpp, !_fakeForm.Visible ? NppMsg.NPPM_DMMSHOW : NppMsg.NPPM_DMMHIDE, 0, _fakeForm.Handle);
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
            if (_fakeForm == null) return;
            Win32Api.SendMessage(Npp.HandleNpp, NppMsg.NPPM_SETMENUITEMCHECK, UnmanagedExports.FuncItems.Items[DockableCommandIndex]._cmdID, _fakeForm.Visible);
        }

        public void ForceClose() {
            if (Form != null)
                Form.Close();
            Form = null;
        }

        #endregion
    }
}
