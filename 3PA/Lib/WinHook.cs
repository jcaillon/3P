#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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

        public event KeyDownHandler KeyDownByPass;

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

            // bypass the normal keydown handler
            if (KeyDownByPass != null) {
                // on key down
                if (!context.IsBitSet(31)) {
                    bool handled = false;
                    KeyDownByPass(key, GetModifiers, ref handled);
                    return handled;
                }
            } else {
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
            }

            return false;
        }

        #endregion

        #region Key modifiers

        private static bool IsPressed(Keys key) {
            const int keyPressed = 0x8000;
            return Convert.ToBoolean(WinApi.GetKeyState((int) key) & keyPressed);
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

    /// <summary>
    /// Monitors the mouse actions
    /// </summary>
    internal class MouseMonitor : WindowsHook<MouseMonitor> {

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event MessageHandler GetMouseMessage;
        public delegate void MessageHandler(WinApi.WindowsMessageMouse message, WinApi.MOUSEHOOKSTRUCT mouseStruct, out bool handled);

        private HashSet<uint> _messagesToIntercept = new HashSet<uint>();

        /// <summary>
        /// Add the keys to monitor (does not include any modifier (CTRL/ALT/SHIFT))
        /// </summary>
        public void Add(params WinApi.WindowsMessageMouse[] messages) {
            foreach (WinApi.WindowsMessageMouse key in messages.Where(key => !_messagesToIntercept.Contains((uint) key))) {
                _messagesToIntercept.Add((uint) key);
            }
        }

        /// <summary>
        /// Remove the keys to monitor (does not include any modifier (CTRL/ALT/SHIFT))
        /// </summary>
        public bool Remove(params WinApi.WindowsMessageMouse[] messages) {
            bool iDidSomething = false;
            foreach (WinApi.WindowsMessageMouse key in messages.Where(key => _messagesToIntercept.Contains((uint) key))) {
                _messagesToIntercept.Remove((uint) key);
                iDidSomething = true;
            }
            return iDidSomething;
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
            WinApi.MOUSEHOOKSTRUCT ms = (WinApi.MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinApi.MOUSEHOOKSTRUCT));
            GetMouseMessage((WinApi.WindowsMessageMouse)wParam, ms, out handled);
            return handled;
        }

        #endregion
    }

    #endregion
    
    #region GetMessage hook

    internal class CallWndProcMonitor : WindowsHook<CallWndProcMonitor> {

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event MessageHandler GetMessage;
        public delegate void MessageHandler(WinApi.MSG message, out bool handled);

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
            WinApi.MSG nc = (WinApi.MSG)Marshal.PtrToStructure(lParam, typeof(WinApi.MSG));
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

    internal class CbtMonitor : WindowsHook<CbtMonitor> {

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event MessageHandler GetCode;
        public delegate void MessageHandler(WinApi.HCBT code);

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
                    GetCode((WinApi.HCBT)code);
            }
            return WinApi.CallNextHookEx(InternalHook, code, wParam, lParam);
        }

        #endregion
    }

    #endregion

    #region WndProc notifications

    /*
    Originally i was using a hook on wndProc to know when the npp windows was moving so i could move the dialog forms accordingly

    private static WinApi.WindowProc _newWindowDeleg;
    private static IntPtr _oldWindowProc = IntPtr.Zero;

    private static void InstallHooks() {
        if (_oldWindowProc == IntPtr.Zero) {
            _newWindowDeleg = OnWndProcMessage;
            int newWndProc = Marshal.GetFunctionPointerForDelegate(_newWindowDeleg).ToInt32();
            int result = WinApi.SetWindowLong(Npp.HandleNpp, (int) WinApi.WindowLongFlags.GWL_WNDPROC, newWndProc);
            _oldWindowProc = (IntPtr) result;
            if (result == 0) {
                ErrorHandler.ShowErrors(new Exception("Failed to SetWindowLong"), "Error in OverrideWindowProc");
            }
        }
    }

    private static IntPtr OnWndProcMessage(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam) {
        switch (uMsg) {
            //case (uint)WinApi.WindowsMessage.WM_SIZE:
            case (uint)WinApi.WindowsMessage.WM_EXITSIZEMOVE:
            case (uint)WinApi.WindowsMessage.WM_MOVE:
                if (OnNppWindowsMove != null) {
                    OnNppWindowsMove();
                }
                break;
            case (uint)WinApi.WindowsMessage.WM_NCACTIVATE:
                if (OnNppWindowsActivate != null) {
                    OnNppWindowsActivate();
                }
                break;
        }
        return WinApi.CallWindowProc(_oldWindowProc, hwnd, uMsg, wParam, lParam);
    }

    private static void UninstallHooks() {
        WinApi.SetWindowLong(Npp.HandleNpp, (int) WinApi.WindowLongFlags.GWL_WNDPROC, _oldWindowProc.ToInt32());
        _oldWindowProc = IntPtr.Zero;
    }

    */

    #endregion

}