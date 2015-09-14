using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace _3PA.Interop {
    public class Win32 {

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, NppMenuCmd lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, int lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, out int lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, IntPtr wParam, int lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, ref LangType lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, SciMsg wParam, string lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, SciMsg wParam, int lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, int wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, int wParam, string lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, int wParam, int lParam);

        public static IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, string text) {
            byte[] bites = Encoding.UTF8.GetBytes(text);
            IntPtr ip = ToUnmanagedArray(bites);
            var result = Win32.SendMessage(hWnd, Msg, bites.Length, ip);
            Marshal.FreeHGlobal(ip);
            return result;
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, out string lParam) {
            var text = new StringBuilder(Win32.MAX_PATH);
            IntPtr retval = Win32.SendMessage(hWnd, Msg, 0, text);
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

        public const int MAX_PATH = 260;
        public const int MF_BYCOMMAND = 0;
        public const int MF_CHECKED = 8;
        public const int MF_UNCHECKED = 0;

        [DllImport("user32")]
        public static extern IntPtr GetMenu(IntPtr hWnd);
        [DllImport("user32")]
        public static extern int CheckMenuItem(IntPtr hmenu, int uIDCheckItem, int uCheck);

        public const int WM_CREATE = 1;

        [DllImport("user32")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("kernel32")]
        public static extern void OutputDebugString(string lpOutputString);

        [DllImport("user32", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(
            int hWnd, // Window handle
            int hWndInsertAfter, // Placement-order handle
            int x, // Horizontal position
            int y, // Vertical position
            int cx, // Width
            int cy, // Height
            uint uFlags); // Window positioning flags

        [DllImport("user32")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32")]
        public static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport("user32", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
            IntPtr windowTitle);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);
    }

    public class ClikeStringArray : IDisposable {
        IntPtr _nativeArray;
        List<IntPtr> _nativeItems;
        bool _disposed = false;

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
            List<string> _managedItems = new List<string>();
            for (int i = 0; i < _nativeItems.Count; i++) {
                if (unicode) _managedItems.Add(Marshal.PtrToStringUni(_nativeItems[i]));
                else _managedItems.Add(Marshal.PtrToStringAnsi(_nativeItems[i]));
            }
            return _managedItems;
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
    public struct COLORREF {
        public uint ColorDWORD;

        public COLORREF(System.Drawing.Color color) {
            ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
        }

        public System.Drawing.Color GetColor() {
            return System.Drawing.Color.FromArgb((int)(0x000000FFU & ColorDWORD),
           (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
        }

        public void SetColor(System.Drawing.Color color) {
            ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
        }
    }
}
