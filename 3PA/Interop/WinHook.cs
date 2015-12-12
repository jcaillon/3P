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
using System.Runtime.InteropServices;
using System.Windows.Forms;

#pragma warning disable 1591

namespace _3PA.Interop
{
    public class ClickMouse {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        public static void DoMouseClick(int _x, int _y) {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)(_x - X), (uint)(_y - Y), 0, 0);
        }
    }

    public class WinHook<T> : LocalWindowsHook, IDisposable where T : new()
    {
        static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }

        protected WinHook()
            : base(HookType.WH_DEBUG)
        {
            m_filterFunc = Proc;
        }

        ~WinHook()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (IsInstalled)
                Uninstall();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        protected void Install(HookType type)
        {
            m_hookType = type;
            base.Install();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected int Proc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code == 0) //Win32.HC_ACTION
                if (HandleHookEvent(wParam, lParam))
                    return 1;

            return CallNextHookEx(m_hhook, code, wParam, lParam);
        }

        virtual protected bool HandleHookEvent(IntPtr wParam, IntPtr lParam)
        {
            throw new NotSupportedException();
        }
    }

    public class MouseMonitor : WinHook<MouseMonitor>
    {
        public event Action MouseMove;

        override protected bool HandleHookEvent(IntPtr wParam, IntPtr lParam)
        {
            const int WM_MOUSEMOVE = 0x0200;
            const int WM_NCMOUSEMOVE = 0x00A0;

            if ((wParam.ToInt32() == WM_MOUSEMOVE || wParam.ToInt32() == WM_NCMOUSEMOVE) && MouseMove != null)
            {
                MouseMove();
            }
            return false;
        }

        public new void Install()
        {
            base.Install(HookType.WH_MOUSE);
        }
    }

    public struct Modifiers
    {
        public bool IsCtrl;
        public bool IsShift;
        public bool IsAlt;
    }

    public class KeyInterceptor : WinHook<KeyInterceptor>
    {
        [DllImport("USER32.dll")]
        static extern short GetKeyState(int nVirtKey);

        public static bool IsPressed(Keys key)
        {
            const int KEY_PRESSED = 0x8000;
            return Convert.ToBoolean(GetKeyState((int)key) & KEY_PRESSED);
        }

        public static Modifiers GetModifiers()
        {
            return new Modifiers
            {
                IsCtrl = IsPressed(Keys.ControlKey),
                IsShift = IsPressed(Keys.ShiftKey),
                IsAlt = IsPressed(Keys.Menu)
            };
        }

        public delegate void KeyDownHandler(Keys key, int repeatCount, ref bool handled);

        public List<int> KeysToIntercept = new List<int>();

        public new void Install()
        {
            base.Install(HookType.WH_KEYBOARD);
        }

        public event KeyDownHandler KeyDown;

        public void Add(params Keys[] keys)
        {
            foreach (int key in keys)
                if (!KeysToIntercept.Contains(key))
                    KeysToIntercept.Add(key);
        }

        public void Remove(params Keys[] keys)
        {
            foreach (int key in keys)
            {
                //ignore for now as anyway the extra invoke will not do any harm 
                //but eventually it needs to be ref counting based
                //KeysToIntercept.RemoveAll(k => k == key);
            }
        }

        public const int KF_UP = 0x8000;
        public const long KB_TRANSITION_FLAG = 0x80000000;

        override protected bool HandleHookEvent(IntPtr wParam, IntPtr lParam)
        {
            int key = (int)wParam;
            int context = (int)lParam;

            if (KeysToIntercept.Contains(key))
            {
                bool down = ((context & KB_TRANSITION_FLAG) != KB_TRANSITION_FLAG);
                int repeatCount = (context & 0xFF00);
                if (down && KeyDown != null)
                {
                    bool handled = false;
                    KeyDown((Keys)key, repeatCount, ref handled);
                    return handled;
                }
            }
            return false;
        }
    }

}