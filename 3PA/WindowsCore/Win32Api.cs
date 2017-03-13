#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Win32Api.cs) is part of 3P.
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Helper;
using _3PA.NppCore;

namespace _3PA.WindowsCore {

    #region Win32API

    [SuppressUnmanagedCodeSecurity]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Win32Api {
        #region SendMessage

        /// <summary>
        /// On Windows, the message-passing scheme used to communicate between the container and Scintilla is mediated by the 
        /// operating system SendMessage function and can lead to bad performance when calling intensively. To avoid this overhead, 
        /// Scintilla provides messages that allow you to call the Scintilla message function directly.
        /// This is the delegate!
        /// </summary>
        /// <remarks>
        /// The interface defined in notepad++ scintilla.h is :
        /// public delegate long Scintilla_DirectFunction(long ptr, uint iMessage, ulong wParam, long lParam);
        /// </remarks>
        public delegate IntPtr Scintilla_DirectFunction(IntPtr ptr, uint iMessage, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, out IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);

        public static IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam) {
            return SendMessage(hWnd, (uint) msg, wParam, lParam);
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, NppMenuCmd lParam) {
            return SendMessage(hWnd, (uint) msg, new IntPtr(wParam), new IntPtr((uint) lParam));
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, IntPtr lParam) {
            return SendMessage(hWnd, (uint) msg, new IntPtr(wParam), lParam);
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, int lParam) {
            return SendMessage(hWnd, (uint) msg, new IntPtr(wParam), new IntPtr(lParam));
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, bool lParam) {
            return SendMessage(hWnd, (uint) msg, new IntPtr(wParam), lParam.ToPointer());
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, out long lParam) {
            IntPtr outVal;
            IntPtr retval = SendMessage(hWnd, (uint) msg, new IntPtr(wParam), out outVal);
            lParam = outVal.ToInt64();
            return retval;
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, IntPtr wParam, int lParam) {
            return SendMessage(hWnd, (uint) msg, wParam, new IntPtr(lParam));
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, StringBuilder lParam) {
            return SendMessage(hWnd, (uint) msg, new IntPtr(wParam), lParam);
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, string lParam) {
            return SendMessage(hWnd, (uint) msg, new IntPtr(wParam), lParam);
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, IntPtr wParam, string lParam) {
            return SendMessage(hWnd, (uint) msg, wParam, lParam);
        }

        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, string lParam) {
            return SendMessage(hWnd, (uint) msg, new IntPtr(wParam), lParam);
        }

        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, int lParam) {
            return SendMessage(hWnd, (uint) msg, new IntPtr(wParam), new IntPtr(lParam));
        }

        public static IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam) {
            return SendMessage(hWnd, msg, new IntPtr(wParam), lParam);
        }

        public static IntPtr SendMessage(IntPtr hWnd, NppMsg msg, int wParam, out string lParam) {
            var text = new StringBuilder(MaxPath);
            IntPtr retval = SendMessage(hWnd, msg, 0, text);
            lParam = text.ToString();
            return retval;
        }

        #endregion

        #region Api

        public const int MaxPath = 260;

        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        public static Control GetFocusedControl() {
            Control focusedControl = null;
            // To get hold of the focused control: 
            IntPtr focusedHandle = GetFocus();
            if (focusedHandle != IntPtr.Zero) {
                // Note that if the focused Control is not a .Net control, then this will return null. 
                focusedControl = Control.FromHandle(focusedHandle);
            }
            return focusedControl;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        /// <summary>
        /// Test if the cursor is in the window rectangle
        /// </summary>
        public static bool IsCursorIn(IntPtr hWnd) {
            return WinApi.GetWindowRect(hWnd).Contains(GetCursorPosition());
        }

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out WinApi.POINT lpPoint);

        public static Point GetCursorPosition() {
            WinApi.POINT lpPoint;
            GetCursorPos(out lpPoint);
            return new Point(lpPoint.x, lpPoint.y);
        }

        #endregion

        #region SetHook

        // Filter function delegate
        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        // Win32: SetWindowsHookEx()
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, uint threadId);

        // Win32: UnhookWindowsHookEx()
        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hhook);

        // Win32: CallNextHookEx()
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEHOOKSTRUCT {
            public WinApi.POINT pt;
            public IntPtr hwnd;
            public uint wHitTestCode;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public WinApi.POINT pt;
        }

        internal enum HCBT {
            MoveSize = 0,
            MinMax = 1,
            QueueSync = 2,
            CreateWnd = 3,
            DestroyWnd = 4,
            Activate = 5,
            ClickSkipped = 6,
            KeySkipped = 7,
            SysCommand = 8,
            SetFocus = 9
        }

        #endregion

        #region Enums

        public enum MessagesMouse : uint {
            WM_MOUSEMOVE = WinApi.Messages.WM_MOUSEMOVE,
            WM_LBUTTONDOWN = WinApi.Messages.WM_LBUTTONDOWN,
            WM_LBUTTONUP = WinApi.Messages.WM_LBUTTONUP,
            WM_LBUTTONDBLCLK = WinApi.Messages.WM_LBUTTONDBLCLK,
            WM_RBUTTONDOWN = WinApi.Messages.WM_RBUTTONDOWN,
            WM_RBUTTONUP = WinApi.Messages.WM_RBUTTONUP,
            WM_RBUTTONDBLCLK = WinApi.Messages.WM_RBUTTONDBLCLK,
            WM_MBUTTONDOWN = WinApi.Messages.WM_MBUTTONDOWN,
            WM_MBUTTONUP = WinApi.Messages.WM_MBUTTONUP,
            WM_MBUTTONDBLCLK = WinApi.Messages.WM_MBUTTONDBLCLK,
            WM_MOUSEWHEEL = WinApi.Messages.WM_MOUSEWHEEL,
            WM_XBUTTONDOWN = WinApi.Messages.WM_XBUTTONDOWN,
            WM_XBUTTONUP = WinApi.Messages.WM_XBUTTONUP,
            WM_XBUTTONDBLCLK = WinApi.Messages.WM_XBUTTONDBLCLK,
            WM_MOUSEHWHEEL = WinApi.Messages.WM_MOUSEHWHEEL,

            WM_NCMOUSEMOVE = WinApi.Messages.WM_NCMOUSEMOVE,
            WM_NCLBUTTONDOWN = WinApi.Messages.WM_NCLBUTTONDOWN,
            WM_NCLBUTTONUP = WinApi.Messages.WM_NCLBUTTONUP,
            WM_NCLBUTTONDBLCLK = WinApi.Messages.WM_NCLBUTTONDBLCLK,
            WM_NCRBUTTONDOWN = WinApi.Messages.WM_NCRBUTTONDOWN,
            WM_NCRBUTTONUP = WinApi.Messages.WM_NCRBUTTONUP,
            WM_NCRBUTTONDBLCLK = WinApi.Messages.WM_NCRBUTTONDBLCLK,
            WM_NCMBUTTONDOWN = WinApi.Messages.WM_NCMBUTTONDOWN,
            WM_NCMBUTTONUP = WinApi.Messages.WM_NCMBUTTONUP,
            WM_NCMBUTTONDBLCLK = WinApi.Messages.WM_NCMBUTTONDBLCLK,
            WM_NCXBUTTONDOWN = WinApi.Messages.WM_NCXBUTTONDOWN,
            WM_NCXBUTTONUP = WinApi.Messages.WM_NCXBUTTONUP,
            WM_NCXBUTTONDBLCLK = WinApi.Messages.WM_NCXBUTTONDBLCLK
        }

        #endregion

        #region UnmanagedStringArray

        /// <summary>
        /// Represent an array of string, useful to communicate with notepad++
        /// </summary>
        public class UnmanagedStringArray : IDisposable {
            private IntPtr _nativeArray;
            private List<IntPtr> _nativeItems;
            private bool _disposed;

            public UnmanagedStringArray(int num, int stringCapacity) {
                _nativeArray = Marshal.AllocHGlobal((num + 1)*IntPtr.Size);
                _nativeItems = new List<IntPtr>();
                for (int i = 0; i < num; i++) {
                    IntPtr item = Marshal.AllocHGlobal(stringCapacity);
                    Marshal.WriteIntPtr((IntPtr) ((int) _nativeArray + (i*IntPtr.Size)), item);
                    _nativeItems.Add(item);
                }
                Marshal.WriteIntPtr((IntPtr) ((int) _nativeArray + (num*IntPtr.Size)), IntPtr.Zero);
            }

            public UnmanagedStringArray(List<string> lstStrings) {
                _nativeArray = Marshal.AllocHGlobal((lstStrings.Count + 1)*IntPtr.Size);
                _nativeItems = new List<IntPtr>();
                for (int i = 0; i < lstStrings.Count; i++) {
                    IntPtr item = Marshal.StringToHGlobalUni(lstStrings[i]);
                    Marshal.WriteIntPtr((IntPtr) ((int) _nativeArray + (i*IntPtr.Size)), item);
                    _nativeItems.Add(item);
                }
                Marshal.WriteIntPtr((IntPtr) ((int) _nativeArray + (lstStrings.Count*IntPtr.Size)), IntPtr.Zero);
            }

            public IntPtr NativePointer {
                get { return _nativeArray; }
            }

            public List<string> ManagedStringsAnsi {
                get { return _getManagedItems(false); }
            }

            public List<string> ManagedStringsUnicode {
                get { return _getManagedItems(true); }
            }

            private List<string> _getManagedItems(bool unicode) {
                List<string> managedItems = new List<string>();
                for (int i = 0; i < _nativeItems.Count; i++) {
                    if (unicode) managedItems.Add(Marshal.PtrToStringUni(_nativeItems[i]));
                    else managedItems.Add(Marshal.PtrToStringAnsi(_nativeItems[i]));
                }
                return managedItems;
            }

            public void Dispose() {
                try {
                    if (!_disposed) {
                        for (int i = 0; i < _nativeItems.Count; i++)
                            if (_nativeItems[i] != IntPtr.Zero) Marshal.FreeHGlobal(_nativeItems[i]);
                        if (_nativeArray != IntPtr.Zero) Marshal.FreeHGlobal(_nativeArray);
                        _disposed = true;
                    }
                } catch (Exception) {
                    // ignored
                }
            }

            ~UnmanagedStringArray() {
                Dispose();
            }
        }

        #endregion
    }

    #endregion

    #region WindowWrapper

    internal class WindowWrapper : IWin32Window {
        public WindowWrapper(IntPtr handle) {
            _hwnd = handle;
        }

        public IntPtr Handle {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }

    #endregion
}