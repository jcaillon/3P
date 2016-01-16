#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (WinHook.cs) is part of 3P.
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _3PA.Interop;

namespace _3PA.Lib {

    #region Keyboard hook

    internal struct KeyModifiers {
        public bool IsCtrl;
        public bool IsShift;
        public bool IsAlt;
    }

    internal class KeyboardMonitor : WindowsHook<KeyboardMonitor> {

        #region private

        private HashSet<Keys> _keysToIntercept = new HashSet<Keys>();
        public delegate void KeyDownHandler(Keys key, KeyModifiers modifiers, ref bool handled);

        #endregion

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event KeyDownHandler KeyDown;

        /// <summary>
        /// Add the keys to monitor (does not include any modifier (CTRL/ALT/SHIFT))
        /// </summary>
        public void Add(params Keys[] keys) {
            foreach (Keys key in keys.Where(key => !_keysToIntercept.Contains(key)))
                _keysToIntercept.Add(key);
        }

        public void Clear() {
            _keysToIntercept.Clear();
        }

        /// <summary>
        /// Call this method to start listening to events
        /// </summary>
        public new void Install() {
            base.Install(HookType.WH_KEYBOARD);
        }

        #endregion


        #region Override HandleHookEvent

        protected override bool HandleHookEvent(IntPtr wParam, IntPtr lParam) {
            var key = (Keys)((int)wParam);
            int context = (int) lParam;

            if (KeyDown == null)
                return false;

            if (_keysToIntercept.Contains(key)) {
                // on key down
                if (!context.IsBitSet(31)) {
                    bool handled = false;
                    KeyDown(key, GetModifiers, ref handled);
                    return handled;
                } 
            }
            return false;
        }

        #endregion

        #region Key modifiers

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private static bool IsPressed(Keys key) {
            const int keyPressed = 0x8000;
            return Convert.ToBoolean(GetKeyState((int) key) & keyPressed);
        }

        public static KeyModifiers GetModifiers {
            get {
                return new KeyModifiers {
                    IsCtrl = IsPressed(Keys.ControlKey),
                    IsShift = IsPressed(Keys.ShiftKey),
                    IsAlt = IsPressed(Keys.Menu)
                };
            }
        }

        #endregion

    }

    #endregion

    #region Mouse hook

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEHOOKSTRUCT {
        public POINT pt;
        public IntPtr hwnd;
        public uint wHitTestCode;
        public IntPtr dwExtraInfo;
    }

    /// <summary>
    /// Monitors the mouse actions
    /// </summary>
    internal class MouseMonitor : WindowsHook<MouseMonitor> {

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event MessageHandler GetMouseMessage;
        public delegate void MessageHandler(WinApi.WindowsMessage message, MOUSEHOOKSTRUCT mouseStruct, out bool handled);

        private HashSet<uint> _messagesToIntercept = new HashSet<uint>();

        /// <summary>
        /// Add the keys to monitor (does not include any modifier (CTRL/ALT/SHIFT))
        /// WM_MOUSEMOVE = 0x200,
        /// WM_LBUTTONDOWN = 0x201,
        /// WM_LBUTTONUP = 0x202,
        /// WM_LBUTTONDBLCLK = 0x203,
        /// WM_RBUTTONDOWN = 0x204,
        /// WM_RBUTTONUP = 0x205,
        /// WM_RBUTTONDBLCLK = 0x206,
        /// WM_MBUTTONDOWN = 0x207,
        /// WM_MBUTTONUP = 0x208,
        /// WM_MBUTTONDBLCLK = 0x209,
        /// WM_MOUSEWHEEL = 0x20A,
        /// WM_XBUTTONDOWN = 0x20B,
        /// WM_XBUTTONUP = 0x20C,
        /// WM_XBUTTONDBLCLK = 0x20D,
        /// WM_MOUSEHWHEEL = 0x20E
        /// </summary>
        public void Add(params WinApi.WindowsMessage[] messages) {
            foreach (WinApi.WindowsMessage key in messages.Where(key => !_messagesToIntercept.Contains((uint)key)))
                _messagesToIntercept.Add((uint)key);
        }

        public void Clear() {
            _messagesToIntercept.Clear();
        }

        /// <summary>
        /// Call this method to start listening to events
        /// </summary>
        public new void Install() {
            base.Install(HookType.WH_MOUSE);
        }

        #endregion


        #region Override HandleHookEvent

        protected override bool HandleHookEvent(IntPtr wParam, IntPtr lParam) {
            if (GetMouseMessage == null)
                return false;
            if (!_messagesToIntercept.Contains((uint)wParam))
                return false;
            bool handled;
            MOUSEHOOKSTRUCT ms = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));
            GetMouseMessage((WinApi.WindowsMessage)wParam, ms, out handled);
            return handled;
        }

        #endregion
    }

    #endregion
    
    #region GetMessage hook

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSG {
        public IntPtr hwnd;
        public UInt32 message;
        public IntPtr wParam;
        public IntPtr lParam;
        public UInt32 time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT {
        public Int32 x;
        public Int32 y;

        public POINT(Int32 x, Int32 y) { this.x = x; this.y = y; }
    }

    internal class CallWndProcMonitor : WindowsHook<CallWndProcMonitor> {

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event MessageHandler GetMessage;
        public delegate void MessageHandler(MSG message, out bool handled);

        private HashSet<uint> _messagesToIntercept = new HashSet<uint>();

        /// <summary>
        /// Add the keys to monitor (does not include any modifier (CTRL/ALT/SHIFT))
        /// </summary>
        public void Add(params WinApi.WindowsMessage[] messages) {
            foreach (WinApi.WindowsMessage key in messages.Where(key => !_messagesToIntercept.Contains((uint)key)))
                _messagesToIntercept.Add((uint)key);
        }

        public void Clear() {
            _messagesToIntercept.Clear();
        }

        /// <summary>
        /// Call this method to start listening to events
        /// </summary>
        public new void Install() {
            base.Install(HookType.WH_GETMESSAGE);
        }

        #endregion


        #region Override HandleHookEvent

        protected override bool HandleHookEvent(IntPtr wParam, IntPtr lParam) {
            MSG nc = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
            if (GetMessage == null)
                return false;
            if (_messagesToIntercept.Contains(nc.message)) {
                bool handled;
                GetMessage(nc, out handled);
                return handled;
            }
            return false;
        }

        #endregion
    }

    #endregion

    #region Cbt hook
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

    internal class CbtMonitor : WindowsHook<CbtMonitor> {

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event MessageHandler GetCode;
        public delegate void MessageHandler(HCBT code);

        /// <summary>
        /// Call this method to start listening to events
        /// </summary>
        public new void Install() {
            CallBackFunction = ThisCallBackFunction;
            base.Install(HookType.WH_CBT);
        }

        #endregion

        #region Override HandleHookEvent

        /// <summary>
        /// Override the callback function handling the events so we can return wether or not the event has been handled
        /// </summary>
        private int ThisCallBackFunction(int code, IntPtr wParam, IntPtr lParam) {
            if (code >= 0) {
                if (GetCode != null)
                    GetCode((HCBT)code);
            }
            return CallNextHookEx(InternalHook, code, wParam, lParam);
        }

        #endregion
    }

    #endregion

}