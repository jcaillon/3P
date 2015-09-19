using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _3PA.Interop;
using NppPlugin.DllExport;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.SynthaxHighlighting;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedParameter.Local

namespace _3PA
{
    class UnmanagedExports
    {
        #region Other

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static bool isUnicode() {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static void setInfo(NppData notepadPlusData) {
            Plug.NppData = notepadPlusData;
            Plug.CommandMenuInit();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static IntPtr getFuncsArray(ref int nbF) {
            nbF = Plug.FuncItems.Items.Count;
            return Plug.FuncItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam) {
            return 1;
        }

        private static IntPtr _ptrPluginName = IntPtr.Zero;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static IntPtr getName() {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(AssemblyInfo.ProductTitle);
            return _ptrPluginName;
        }

        #endregion

        #region BeNotified
        /// <summary>
        /// handles the notifications send by npp and scintilla to the plugin
        /// </summary>
        /// <param name="notifyCode"></param>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static void beNotified(IntPtr notifyCode) {
            try {
                SCNotification nc = (SCNotification) Marshal.PtrToStructure(notifyCode, typeof (SCNotification));
                #region must 
                switch (nc.nmhdr.code) {
                    case (uint) NppMsg.NPPN_TBMODIFICATION:
                        Plug.FuncItems.RefreshItems();
                        Plug.InitToolbarImages();
                        return;

                    case (uint) NppMsg.NPPN_READY:
                        // notify plugins that all the procedures of launchment of notepad++ are done
                        Plug.OnNppReady();
                        return;

                    case (uint) NppMsg.NPPN_SHUTDOWN:
                        Marshal.FreeHGlobal(_ptrPluginName);
                        Plug.CleanUp();
                        return;
                }
                #endregion

                // Only do stuff when the dll is fully loaded
                if (!Plug.PluginIsFullyLoaded) return;

                // the user changed the current document
                if (nc.nmhdr.code == (uint) NppMsg.NPPN_BUFFERACTIVATED) {
                    Plug.OnDocumentSwitched();
                    return;
                }

                // only do extra stuff if we are in a progress file
                if (!Utils.IsCurrentProgressFile()) return;

                #region extra
                switch (nc.nmhdr.code) {
                    case (uint)NppMsg.NPPN_FILEBEFOREOPEN:
                        // fire when a file is opened, can be used to clean up data on closed documents

                        return;

                    case (uint)NppMsg.NPPN_SHORTCUTREMAPPED:
                        // notify plugins that plugin command shortcut is remapped
                        Interop.Plug.ShortcutsUpdated((int)nc.nmhdr.idFrom, (ShortcutKey)Marshal.PtrToStructure(nc.nmhdr.hwndFrom, typeof(ShortcutKey)));
                        return;

                    case (uint) SciMsg.SCN_CHARADDED:
                        // called each time the user add a char in the current scintilla
                        Plug.OnCharTyped((char) nc.ch);
                        return;

                    case (uint)SciMsg.SCN_STYLENEEDED:
                        // if we use the contained lexer, we will receive this notification and we will have to style the text
                        Highlight.Colorize(Npp.GetSylingNeededStartPos(), nc.position);
                        return;

                    case (uint) SciMsg.SCN_UPDATEUI:
                        // we need to set the indentation when we received this notification, not before or it's overwritten
                        if (Plug.ActionAfterUpdateUi != null) {
                            Plug.ActionAfterUpdateUi();
                            Plug.ActionAfterUpdateUi = null;
                        }

                        if (nc.updated == (int) SciMsg.SC_UPDATE_V_SCROLL ||
                            nc.updated == (int) SciMsg.SC_UPDATE_H_SCROLL) {
                            // user scrolled
                            Plug.OnPageScrolled();
                        } else if (nc.updated == (int) SciMsg.SC_UPDATE_SELECTION) {
                            // the user changed its selection
                            Plug.OnUpdateSelection();
                        }
                        return;

                    case (uint) SciMsg.SCN_MODIFYATTEMPTRO:
                        // Code a checkout when trying to modify a read-only file

                        return;

                    case (uint) SciMsg.SCN_DWELLSTART:
                        // when the user hover at a fixed position for too long
                        Plug.OnDwellStart();
                        return;

                    case (uint) SciMsg.SCN_DWELLEND:
                        // when he moves his cursor
                        Plug.OnDwellEnd();
                        return;

                    case (uint) SciMsg.SCN_MODIFIED:
                        // if at least 1 line has been added or removed
                        if (nc.linesAdded != 0) {
                            Plug.OnLineAddedOrRemoved();
                        }
                        
                        // did the user supress 1 char?
                        if ((nc.modificationType & (int) SciMsg.SC_MOD_DELETETEXT) != 0 && nc.length == 1) {
                            AutoComplete.ActivatedAutoCompleteIfNeeded();
                        }

                        //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_USER) != 0;
                        //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_UNDO) != 0;
                        //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_REDO) != 0;
                        return;
                }
                #endregion

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in beNotified");
            }
        }

        #endregion

    }
}