#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FastObjectListViewWoScrolls.cs) is part of 3P.
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
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace _3PA.MainFeatures.AutoCompletion {
    class FastObjectListViewWoScrolls : FastObjectListView {

        const UInt32 WmNccalcsize = 0x83;
        const int GwlStyle = -16;
        const int WsVscroll = 0x00200000;
        const int WsHscroll = 0x00100000;

        protected override void WndProc(ref Message m) {

            if (m.Msg == WmNccalcsize) // WM_NCCALCSIZE
            {
                int style = GetWindowLong(Handle, GwlStyle);
                if (Config.Instance.AutoCompleteHideScrollBar) {
                    // deactivate vertical scroll bar
                    if ((style & WsVscroll) == WsVscroll)
                        SetWindowLong(Handle, GwlStyle, style & ~WsVscroll);
                }
                // deactivate horizontal scroll bar
                if ((style & WsHscroll) == WsHscroll)
                    SetWindowLong(Handle, GwlStyle, style & ~WsHscroll);
            }

            base.WndProc(ref m);
        }

        public static int GetWindowLong(IntPtr hWnd, int nIndex) {
            if (IntPtr.Size == 4)
                return (int)GetWindowLong32(hWnd, nIndex);
            return (int)(long)GetWindowLongPtr64(hWnd, nIndex);
        }

        public static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong) {
            if (IntPtr.Size == 4)
                return (int)SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            return (int)(long)SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
