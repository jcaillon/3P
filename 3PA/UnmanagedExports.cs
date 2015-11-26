#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (UnmanagedExports.cs) is part of 3P.
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
using NppPlugin.DllExport;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.SyntaxHighlighting;

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
            uint code = 0;
            try {
                SCNotification nc = (SCNotification) Marshal.PtrToStructure(notifyCode, typeof (SCNotification));
                code = nc.nmhdr.code;

                #region must 
                switch (code) {
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
                if (code == (uint) NppMsg.NPPN_BUFFERACTIVATED) {
                    Plug.OnDocumentSwitched();
                    return;
                }

                // only do extra stuff if we are in a progress file
                if (!Plug.IsCurrentFileProgress) return;

                #region extra
                switch (code) {
                    case (uint) SciMsg.SCN_CHARADDED:
                        // called each time the user add a char in the current scintilla
                        Plug.OnCharTyped((char)nc.ch);
                        return;

                    case (uint) SciMsg.SCN_UPDATEUI:
                        // we need to set the indentation when we received this notification, not before or it's overwritten
                        if (Plug.ActionAfterUpdateUi != null) {
                            Plug.ActionAfterUpdateUi();
                            Plug.ActionAfterUpdateUi = null;
                        }

                        if (nc.updated == (int)SciMsg.SC_UPDATE_V_SCROLL ||
                            nc.updated == (int)SciMsg.SC_UPDATE_H_SCROLL) {
                            // user scrolled
                            Plug.OnPageScrolled();
                        } else if (nc.updated == (int)SciMsg.SC_UPDATE_SELECTION) {
                            // the user changed its selection
                            Plug.OnUpdateSelection();
                        }
                        return;

                    case (uint) SciMsg.SCN_MODIFIED:
                        // if the text has changed, parse
                        if ((nc.modificationType & (int)SciMsg.SC_MOD_DELETETEXT) != 0 ||
                            (nc.modificationType & (int)SciMsg.SC_MOD_INSERTTEXT) != 0)
                            AutoComplete.ParseCurrentDocument();

                        // did the user supress 1 char?
                        if ((nc.modificationType & (int)SciMsg.SC_MOD_DELETETEXT) != 0 && nc.length == 1) {
                            AutoComplete.UpdateAutocompletion();
                        }

                        // if (nc.linesAdded != 0)
                        //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_USER) != 0;
                        //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_UNDO) != 0;
                        //bool x = (nc.modificationType & (int)SciMsg.SC_PERFORMED_REDO) != 0;
                        return;

                    case (uint) SciMsg.SCN_STYLENEEDED:
                        // if we use the contained lexer, we will receive this notification and we will have to style the text
                        //Highlight.Colorize(Npp.GetSylingNeededStartPos(), nc.position);
                        return;

                    case (uint) NppMsg.NPPN_FILEBEFOREOPEN:
                        // fire when a file is opened, can be used to clean up data on closed documents

                        return;

                    case (uint) NppMsg.NPPN_SHORTCUTREMAPPED:
                        // notify plugins that plugin command shortcut is remapped
                        Interop.Plug.ShortcutsUpdated((int)nc.nmhdr.idFrom, (ShortcutKey)Marshal.PtrToStructure(nc.nmhdr.hwndFrom, typeof(ShortcutKey)));
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

                    case (uint) NppMsg.NPPN_FILESAVED:
                        // on file saved
                        Plug.OnFileSaved();
                        return;
                }
                #endregion

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error in beNotified : code = " + code);
            }
        }

        #endregion

    }
}