#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Win32.cs) is part of 3P.
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
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace _3PA.Interop {
    public class Win32 {
        private const string DllNameKernel32 = "kernel32.dll";
        private const string DllNameUser32 = "user32.dll";

        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, NppMenuCmd lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, IntPtr lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, int lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, out int lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, IntPtr wParam, int lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, ref LangType lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg msg, SciMsg wParam, string lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg msg, SciMsg wParam, int lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, IntPtr lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, string lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, int lParam);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, byte[] infos);
        [DllImport(DllNameUser32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam);

        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, string text) {
            byte[] bites = Encoding.UTF8.GetBytes(text);
            IntPtr ip = ToUnmanagedArray(bites);
            var result = SendMessage(hWnd, msg, bites.Length, ip);
            Marshal.FreeHGlobal(ip);
            return result;
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, out string lParam) {
            var text = new StringBuilder(MaxPath);
            IntPtr retval = SendMessage(hWnd, msg, 0, text);
            lParam = text.ToString();
            return retval;
        }

        static IntPtr ToUnmanagedArray(byte[] data) {
            unsafe {
                int newSizeInBytes = Marshal.SizeOf(typeof(byte)) * data.Length + 2;
                byte* newArrayPointer = (byte*)Marshal.AllocHGlobal(newSizeInBytes).ToPointer();

                for (int i = 0; i < newSizeInBytes; i++)
                    *(newArrayPointer + i) = (i < data.Length ? data[i] : (byte)0);

                return (IntPtr)newArrayPointer;
            }
        }

        public const int MaxPath = 260;
        public const int MfBycommand = 0;
        public const int MfChecked = 8;
        public const int MfUnchecked = 0;

        [DllImport(DllNameUser32)]
        public static extern IntPtr GetMenu(IntPtr hWnd);
        [DllImport(DllNameUser32)]
        public static extern int CheckMenuItem(IntPtr hmenu, int uIdCheckItem, int uCheck);

        public const int WmCreate = 1;

        [DllImport(DllNameUser32)]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);
        [DllImport(DllNameUser32, EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(
            int hWnd, // Window handle
            int hWndInsertAfter, // Placement-order handle
            int x, // Horizontal position
            int y, // Vertical position
            int cx, // Width
            int cy, // Height
            uint uFlags); // Window positioning flags

        [DllImport(DllNameUser32)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport(DllNameUser32)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport(DllNameUser32)]
        public static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport(DllNameUser32, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
            IntPtr windowTitle);

        [DllImport(DllNameUser32, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);

        [DllImport(DllNameKernel32, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(HandleRef hModule, string lpProcName);
    }

    public class ClikeStringArray : IDisposable {
        IntPtr _nativeArray;
        List<IntPtr> _nativeItems;
        bool _disposed;

        public ClikeStringArray(int num, int stringCapacity) {
            _nativeArray = Marshal.AllocHGlobal((num + 1) * IntPtr.Size);
            _nativeItems = new List<IntPtr>();
            for (int i = 0; i < num; i++) {
                IntPtr item = Marshal.AllocHGlobal(stringCapacity);
                Marshal.WriteIntPtr((IntPtr)((int)_nativeArray + (i * IntPtr.Size)), item);
                _nativeItems.Add(item);
            }
            Marshal.WriteIntPtr((IntPtr)((int)_nativeArray + (num * IntPtr.Size)), IntPtr.Zero);
        }
        public ClikeStringArray(List<string> lstStrings) {
            _nativeArray = Marshal.AllocHGlobal((lstStrings.Count + 1) * IntPtr.Size);
            _nativeItems = new List<IntPtr>();
            for (int i = 0; i < lstStrings.Count; i++) {
                IntPtr item = Marshal.StringToHGlobalUni(lstStrings[i]);
                Marshal.WriteIntPtr((IntPtr)((int)_nativeArray + (i * IntPtr.Size)), item);
                _nativeItems.Add(item);
            }
            Marshal.WriteIntPtr((IntPtr)((int)_nativeArray + (lstStrings.Count * IntPtr.Size)), IntPtr.Zero);
        }

        public IntPtr NativePointer { get { return _nativeArray; } }
        public List<string> ManagedStringsAnsi { get { return _getManagedItems(false); } }
        public List<string> ManagedStringsUnicode { get { return _getManagedItems(true); } }
        List<string> _getManagedItems(bool unicode) {
            List<string> managedItems = new List<string>();
            for (int i = 0; i < _nativeItems.Count; i++) {
                if (unicode) managedItems.Add(Marshal.PtrToStringUni(_nativeItems[i]));
                else managedItems.Add(Marshal.PtrToStringAnsi(_nativeItems[i]));
            }
            return managedItems;
        }

        public void Dispose() {
            if (!_disposed) {
                for (int i = 0; i < _nativeItems.Count; i++)
                    if (_nativeItems[i] != IntPtr.Zero) Marshal.FreeHGlobal(_nativeItems[i]);
                if (_nativeArray != IntPtr.Zero) Marshal.FreeHGlobal(_nativeArray);
                _disposed = true;
            }
        }
        ~ClikeStringArray() {
            Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Colorref {
        public uint ColorDWORD;

        public Colorref(Color color) {
            ColorDWORD = color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
        }

        public Color GetColor() {
            return Color.FromArgb((int)(0x000000FFU & ColorDWORD),
           (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
        }

        public void SetColor(Color color) {
            ColorDWORD = color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
        }
    }
}
