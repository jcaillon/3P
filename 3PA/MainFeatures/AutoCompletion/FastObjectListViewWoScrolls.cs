using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using _3PA.Lib;

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
