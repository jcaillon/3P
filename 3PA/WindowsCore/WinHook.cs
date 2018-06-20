#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
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
using YamuiFramework.Helper;
using _3PA.Lib;

namespace _3PA.WindowsCore {

    #region Keyboard hook

    internal struct KeyModifiers {
        public bool IsCtrl;
        public bool IsShift;
        public bool IsAlt;
    }

    internal class KeyboardMonitor : WindowsHook<KeyboardMonitor> {
        #region private

        private HashSet<Keys> _keysToIntercept = new HashSet<Keys>();

        #endregion

        #region public

        /// <summary>
        /// Register to receive on keyPressed events
        /// </summary>
        public event Func<KeyEventArgs, bool> KeyDown;

        public event Func<KeyEventArgs, bool> KeyDownByPass;

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
            var key = (Keys) (wParam.ToInt64());
            long context = lParam.ToInt64();

            // bypass the normal keydown handler, send all the messages
            if (KeyDownByPass != null) {
                // on key down
                if (!context.IsBitSet(31)) {
                    bool handled = KeyDownByPass(new KeyEventArgs(ToFullKey(key, GetModifiers)));
                    return handled;
                }
            } else {
                if (KeyDown != null && _keysToIntercept.Contains(key)) {
                    // on key down
                    if (!context.IsBitSet(31)) {
                        bool handled = KeyDown(new KeyEventArgs(ToFullKey(key, GetModifiers)));
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
            return Convert.ToBoolean(Win32Api.GetKeyState((int) key) & keyPressed);
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

        private static Keys ToFullKey(Keys rawKey, KeyModifiers modifiers) {
            if (modifiers.IsAlt)
                rawKey |= Keys.Alt;
            if (modifiers.IsCtrl)
                rawKey |= Keys.Control;
            if (modifiers.IsShift)
                rawKey |= Keys.Shift;
            return rawKey;
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
        public event Func<WinApi.Messages, Win32Api.MOUSEHOOKSTRUCT, bool> GetMouseMessage;

        private HashSet<uint> _messagesToIntercept = new HashSet<uint>();

        /// <summary>
        /// Add the keys to monitor (does not include any modifier (CTRL/ALT/SHIFT))
        /// </summary>
        public void Add(params WinApi.Messages[] messages) {
            foreach (WinApi.Messages key in messages.Where(key => !_messagesToIntercept.Contains((uint) key))) {
                _messagesToIntercept.Add((uint) key);
            }
        }

        /// <summary>
        /// Remove the keys to monitor (does not include any modifier (CTRL/ALT/SHIFT))
        /// </summary>
        public bool Remove(params WinApi.Messages[] messages) {
            bool iDidSomething = false;
            foreach (WinApi.Messages key in messages.Where(key => _messagesToIntercept.Contains((uint) key))) {
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
            if (!_messagesToIntercept.Contains((uint) wParam))
                return false;
            Win32Api.MOUSEHOOKSTRUCT ms = (Win32Api.MOUSEHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(Win32Api.MOUSEHOOKSTRUCT));
            return GetMouseMessage((WinApi.Messages) wParam, ms);
        }

        #endregion
    }

    #endregion

}