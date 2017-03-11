#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CoreWindowsHook.cs) is part of 3P.
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

// ReSharper disable InconsistentNaming

namespace _3PA.Interop {

    #region WindowsHook wrapper

    /// <summary>
    /// A nice wrapper around the CoreWindowsHook class, 
    /// doesn't use the EventHandler but replace the CallBackFunction
    /// read this: 
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644959(v=vs.85).aspx
    /// </summary>
    internal abstract class WindowsHook<T> : CoreWindowsHook, IDisposable where T : new() {
        #region Singleton

        /// <summary>
        /// Singleton mechanism
        /// </summary>
        public static T Instance {
            get {
                if (_instance == null)
                    _instance = new T();
                return _instance;
            }
        }

        private static T _instance;

        #endregion

        #region Constructor / destructor

        protected WindowsHook() : base(HookType.WH_DEBUG) {
            CallBackFunction = OverrideCallBackFunction;
        }

        ~WindowsHook() {
            Dispose(false);
        }

        #endregion

        #region Core

        /// <summary>
        /// Override this method to handle the hook events
        /// </summary>
        protected virtual bool HandleHookEvent(IntPtr wParam, IntPtr lParam) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Override the callback function handling the events so we can return wether or not the event has been handled
        /// </summary>
        protected int OverrideCallBackFunction(int code, IntPtr wParam, IntPtr lParam) {
            if (code == 0) // Win32.HC_ACTION
                if (HandleHookEvent(wParam, lParam))
                    return 1; // The event has been handled
            return Win32Api.CallNextHookEx(InternalHook, code, wParam, lParam);
        }

        /// <summary>
        /// Call this method to monitor for certain types of events
        /// </summary>
        protected void Install(HookType type) {
            InternalHookType = type;
            base.Install();
        }

        #endregion

        #region dispose

        protected void Dispose(bool disposing) {
            if (IsInstalled)
                Uninstall();
            if (disposing)
                GC.SuppressFinalize(this);
        }

        public void Dispose() {
            Dispose(true);
        }

        #endregion
    }

    #endregion

    #region Enum HookType

    // Hook Types
    internal enum HookType {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    #endregion

    #region CoreWindowsHook

    internal class CoreWindowsHook {
        // Internal properties
        protected IntPtr InternalHook = IntPtr.Zero;
        protected Win32Api.HookProc CallBackFunction;
        protected HookType InternalHookType;

        // Event delegate
        public delegate void HookEventHandler(object sender, HookEventArgs e);

        // Event: HookInvoked 
        public event HookEventHandler HookInvoked;

        protected void OnHookInvoked(HookEventArgs e) {
            if (HookInvoked != null)
                HookInvoked(this, e);
        }

        /// <summary>
        /// Register to the HookInvoked event
        /// </summary>
        public CoreWindowsHook(HookType hookType) {
            InternalHookType = hookType;
            CallBackFunction = CoreHookProc;
        }

        /// <summary>
        /// Use your own HookProc ?
        /// </summary>
        public CoreWindowsHook(HookType hookType, Win32Api.HookProc callBackFunction) {
            InternalHookType = hookType;
            CallBackFunction = callBackFunction;
        }

        // Default filter function
        protected int CoreHookProc(int code, IntPtr wParam, IntPtr lParam) {
            if (code < 0)
                return Win32Api.CallNextHookEx(InternalHook, code, wParam, lParam);

            // Let clients determine what to do
            OnHookInvoked(new HookEventArgs {
                HookCode = code,
                WParam = wParam,
                LParam = lParam
            });

            // Yield to the next hook in the chain
            return Win32Api.CallNextHookEx(InternalHook, code, wParam, lParam);
        }

        // Install the hook
        public void Install() {
            InternalHook = Win32Api.SetWindowsHookEx(InternalHookType, CallBackFunction, IntPtr.Zero, Win32Api.GetCurrentThreadId());
        }

        // Uninstall the hook
        public void Uninstall() {
            // delete all subscribers
            /*
            foreach (Delegate d in HookInvoked.GetInvocationList()) {
                HookInvoked -= (HookEventHandler)d;
            }
            */
            Win32Api.UnhookWindowsHookEx(InternalHook);
            InternalHook = IntPtr.Zero;
        }

        public bool IsInstalled {
            get { return InternalHook != IntPtr.Zero; }
        }
    }

    #endregion

    #region Class HookEventArgs

    internal class HookEventArgs : EventArgs {
        public int HookCode; // Hook code
        public IntPtr WParam; // WPARAM argument
        public IntPtr LParam; // LPARAM argument
    }

    #endregion
}