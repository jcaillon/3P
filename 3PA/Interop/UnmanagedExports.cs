#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using RGiesecke.DllExport;
using _3PA.Lib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedParameter.Local

namespace _3PA.Interop {

    /// <summary>
    /// Main entry point for Npp,
    /// Allows Npp to manipulate our plugin, ask for info, execute functions that we declared in the menu...
    /// </summary>
    internal static class UnmanagedExports {

        #region fields

        /// <summary>
        /// Info on the 3 handles : Npp's window, and the 2 scintilla instances
        /// </summary>
        public static NppData NppData { get; set; }

        /// <summary>
        /// Info on the functions of our plugin
        /// </summary>
        public static FuncItems FuncItems = new FuncItems();

        private static IntPtr _ptrPluginName = IntPtr.Zero;

        #endregion

        #region Exported methods (to be used by npp)

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static bool isUnicode() {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static void setInfo(NppData notepadPlusData) {
            NppData = notepadPlusData;
            Plug.OnCommandMenuInit();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static IntPtr getFuncsArray(ref int nbF) {
            nbF = FuncItems.Items.Count;
            return FuncItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam) {
            return 1;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static IntPtr getName() {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(AssemblyInfo.AssemblyProduct);
            return _ptrPluginName;
        }

        /// <summary>
        /// handles the notifications send by npp and scintilla to the plugin
        /// </summary>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static void beNotified(IntPtr notifyCode) {
            SCNotification nc = (SCNotification)Marshal.PtrToStructure(notifyCode, typeof(SCNotification));
            Plug.OnNppNotification(nc);
        }

        #endregion

        #region public methods

        public static void FreeUpMem() {
            Marshal.FreeHGlobal(_ptrPluginName);
        }

        #endregion
    }
}