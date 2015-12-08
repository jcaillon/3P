#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (WindowWrapper.cs) is part of 3P.
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
using System.Windows.Forms;

namespace _3PA.Interop {
    public class WindowWrapper : IWin32Window {
        public WindowWrapper(IntPtr handle) {
            _hwnd = handle;
        }

        public IntPtr Handle {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }

    public static class FormIntegration {

        public static void RegisterToNpp(IntPtr handle) {
            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_MODELESSDIALOG, (int)NppMsg.MODELESSDIALOGADD, handle);
        }

        public static void UnRegisterToNpp(IntPtr handle) {
            Win32.SendMessage(Npp.HandleNpp, NppMsg.NPPM_MODELESSDIALOG, (int)NppMsg.MODELESSDIALOGREMOVE, handle);
        }
    }
}