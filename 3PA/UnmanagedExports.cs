using System;
using System.Runtime.InteropServices;
using _3PA.Interop;
using NppPlugin.DllExport;
using _3PA.Lib;
using _3PA.MainFeatures;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedParameter.Local

namespace _3PA
{
    class UnmanagedExports
    {
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            Plug.NppData = notepadPlusData;
            Plug.CommandMenuInit();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            nbF = Plug.FuncItems.Items.Count;
            return Plug.FuncItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        static IntPtr _ptrPluginName = IntPtr.Zero;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(AssemblyInfo.ProductTitle);
            return _ptrPluginName;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            try {
                SCNotification nc = (SCNotification) Marshal.PtrToStructure(notifyCode, typeof (SCNotification));

                switch (nc.nmhdr.code) {

                    case (uint)NppMsg.NPPN_TBMODIFICATION:
                        Plug.FuncItems.RefreshItems();
                        Plug.InitToolbarImages();
                        break;

                    case (uint)NppMsg.NPPN_READY:
                        // notify plugins that all the procedures of launchment of notepad++ are done
                        Plug.OnNppReady();
                        Win32.SendMessage(Npp.HandleNpp, SciMsg.SCI_SETMODEVENTMASK,
                            SciMsg.SC_MOD_INSERTTEXT | SciMsg.SC_MOD_DELETETEXT | SciMsg.SC_PERFORMED_USER, 0);
                        // set the timer of dwell time, if the user let the mouse inactive for this period of time, npp fires the dwellstart notif
                        Win32.SendMessage(Npp.HandleNpp, SciMsg.SCI_SETMOUSEDWELLTIME, 500, 0);
                        break;

                    case (uint)NppMsg.NPPN_SHUTDOWN:
                        Marshal.FreeHGlobal(_ptrPluginName);
                        Plug.CleanUp();
                        break;

                    case (uint)SciMsg.SCN_CHARADDED:
                        // called each time the user add a char in the current scintilla
                        Plug.OnCharTyped((char) nc.ch);
                        break;

                    case (uint)SciMsg.SCN_UPDATEUI:
                        // check if the user switched tab, we need to apply certain options
                        Plug.ApplyPluginSpecificOptions();

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
                        break;

                    case (uint)SciMsg.SCN_MODIFYATTEMPTRO:
                        // Code a checkout when trying to modify a read-only file

                        break;

                    case (uint)SciMsg.SCN_DWELLSTART:
                        // when the user hover at a fixed position for too long
                        
                        break;

                    case (uint)SciMsg.SCN_DWELLEND:
                        // when he moves his cursor
                        
                        break;

                    case (uint)SciMsg.SCN_MODIFIED:
                        if (Plug.PluginIsFullyLoaded) {
                            // check if the user switched tab, we need to apply certain options
                            Plug.ApplyPluginSpecificOptions();

                            // if at least 1 line has been added or removed
                            if (nc.linesAdded != 0) {
                                Plug.OnLineAddedOrRemoved();
                            }
                            // did the user supress 1 char?
                            if ((nc.modificationType & (int) SciMsg.SC_MOD_DELETETEXT) != 0 && nc.length == 1) {
                                AutoComplete.ActivatedAutoCompleteIfNeeded();
                            }
                        }
                        break;

                    case (uint)NppMsg.NPPN_FILEBEFOREOPEN:
                        // fire when a file is opened, can be used to clean up data on closed documents

                        break;

                    case (uint)NppMsg.NPPN_SHORTCUTREMAPPED:
                        // notify plugins that plugin command shortcut is remapped
                        Interop.Plug.ShortcutsUpdated((int)nc.nmhdr.idFrom, (ShortcutKey)Marshal.PtrToStructure(nc.nmhdr.hwndFrom, typeof(ShortcutKey)));
                        break;
                }
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error in beNotified");
            }
        }
    }
}