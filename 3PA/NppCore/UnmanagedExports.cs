#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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

namespace _3PA.NppCore {
    /// <summary>
    /// Main entry point for Npp,
    /// Allows Npp to manipulate our plugin, ask for info, execute functions that we declared in the menu...
    /// </summary>
    public static class UnmanagedExports {
        #region fields

        /// <summary>
        /// Info on the 3 handles : Npp's window, and the 2 scintilla instances
        /// </summary>
        public static NppData NppData { get; set; }

        /// <summary>
        /// Info on the functions of our plugin
        /// </summary>
        public static NppFuncItems NppFuncItems = new NppFuncItems();

        private static IntPtr _ptrPluginName = IntPtr.Zero;

        #endregion

        #region Exported methods (to be used by npp)

        /// <summary>
        /// A plugin is designed to either work with an ANSI or Unicode build of Notepad++. ANSI plugins must not define this function. 
        /// Unicode plugins must define it, and it must return true
        /// </summary>
        /// <returns></returns>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static bool isUnicode() {
            return true;
        }

        /// <summary>
        /// This routine is called when the plugin is loaded, providing it with information on the current instance of Notepad++ – namely, an array of three handles for: the main Notepad++ window, the primary Scintilla control, the secondary Scintilla control
        /// </summary>
        /// <param name="notepadPlusData"></param>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static void setInfo(NppData notepadPlusData) {
            NppData = notepadPlusData;
            Plug.DoPlugLoad();
        }

        /// <summary>
        /// Retrieves a pointer to an array of structures that describe the exposed functions. The expected length of the array is the value pointed by the argument. 
        /// There must be at least one such routine. 
        /// Provide one that displays some sort of About dialog box if there is otherwise no need for a menu entry - a typical case for external lexers
        /// </summary>
        /// <param name="nbF"></param>
        /// <returns></returns>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static IntPtr getFuncsArray(ref int nbF) {
            Plug.DoFuncItemsNeeded();
            nbF = NppFuncItems.Items.Count;
            return NppFuncItems.NativePointer;
        }

        /// <summary>
        /// This is a message processor handling any message Notepad++ has to pass on
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam) {
            return 1;
        }

        /// <summary>
        /// Returns name of the plugin, to appear in the Plugin menu
        /// </summary>
        /// <returns></returns>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static IntPtr getName() {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(AssemblyInfo.AssemblyProduct);
            return _ptrPluginName;
        }

        /// <summary>
        /// This procedure will be called by Notepad++ for a variety of reasons. The complete list of codes is to be found on the Messages And Notifications. 
        /// It should handle these tasks using information passed in the notification header
        /// </summary>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        public static void beNotified(IntPtr notifyCode) {
            SCNotification nc = (SCNotification) Marshal.PtrToStructure(notifyCode, typeof(SCNotification));
            NotificationsPublisher.OnNppNotification(nc);
        }

        #endregion
    }
}