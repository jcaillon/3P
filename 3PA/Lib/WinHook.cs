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

    public struct KeyModifiers {
        public bool IsCtrl;
        public bool IsShift;
        public bool IsAlt;
    }

    public class KeyboardMonitor : WindowsHook<KeyboardMonitor> {

        #region private

        private HashSet<Keys> _keysToIntercept = new HashSet<Keys>();
        public delegate void KeyDownHandler(Keys key, KeyModifiers modifiers, ref bool handled);

        #endregion

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event KeyDownHandler KeyPressed;

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

            if (KeyPressed == null)
                return false;

            if (_keysToIntercept.Contains(key)) {
                // on key down
                if (!context.IsBitSet(31)) {
                    bool handled = false;
                    KeyPressed(key, GetModifiers, ref handled);
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

    #region GetMessage hook

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG {
        public IntPtr hwnd;
        public UInt32 message;
        public IntPtr wParam;
        public IntPtr lParam;
        public UInt32 time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public Int32 x;
        public Int32 y;

        public POINT(Int32 x, Int32 y) { this.x = x; this.y = y; }
    }

    public class CallWndProcMonitor : WindowsHook<CallWndProcMonitor> {

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event MessageHandler GetMessage;
        public delegate void MessageHandler(MSG message);

        private HashSet<uint> _messagesToIntercept = new HashSet<uint>();

        /// <summary>
        /// Add the keys to monitor (does not include any modifier (CTRL/ALT/SHIFT))
        /// </summary>
        public void Add(params WindowsMessage[] messages) {
            foreach (WindowsMessage key in messages.Where(key => !_messagesToIntercept.Contains((uint)key)))
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
                bool handled = false;
                GetMessage(nc);
                return handled;
            }
            return false;
        }

        #endregion
    }

    #endregion

    #region Mouse hook

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEHOOKSTRUCT {
        public POINT pt;
        public IntPtr hwnd;
        public uint wHitTestCode;
        public IntPtr dwExtraInfo;
    }

    /// <summary>
    /// To do when i need it, for now it's just an empty shell
    /// </summary>
    public class MouseMonitor : WindowsHook<MouseMonitor> {

        public event Action MouseMove;

        protected override bool HandleHookEvent(IntPtr wParam, IntPtr lParam) {
            MOUSEHOOKSTRUCT ms = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));
            const int wmMousemove = 0x0200;
            const int wmNcmousemove = 0x00A0;
            if ((wParam.ToInt32() == wmMousemove || wParam.ToInt32() == wmNcmousemove) && MouseMove != null) {
                MouseMove();
            }
            return false;
        }

        public new void Install() {
            base.Install(HookType.WH_MOUSE);
        }
    }

    #endregion

}