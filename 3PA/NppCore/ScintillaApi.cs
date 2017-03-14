using System;
using System.Runtime.InteropServices;
using _3PA.WindowsCore;

namespace _3PA.NppCore {

    /// <summary>
    /// Use this class to communicate with scintilla
    /// </summary>
    internal class ScintillaApi {

        private static Win32Api.Scintilla_DirectFunction _directFunction;
        private static IntPtr _directMessagePointer;
        private static IntPtr _scintillaHandle;

        public ScintillaApi(IntPtr scintillaHandle) {
            UpdateScintillaDirectMessage(scintillaHandle);
        }

        /// <summary>
        /// Instantiates the direct message function
        /// </summary>
        public void UpdateScintillaDirectMessage(IntPtr scintillaHandle) {
            _scintillaHandle = scintillaHandle;
            var directFunctionPointer = Win32Api.SendMessage(_scintillaHandle, (uint)SciMsg.SCI_GETDIRECTFUNCTION, IntPtr.Zero, IntPtr.Zero);
            // Create a managed callback
            _directFunction = (Win32Api.Scintilla_DirectFunction)Marshal.GetDelegateForFunctionPointer(directFunctionPointer, typeof(Win32Api.Scintilla_DirectFunction));
            _directMessagePointer = Win32Api.SendMessage(_scintillaHandle, (uint)SciMsg.SCI_GETDIRECTPOINTER, IntPtr.Zero, IntPtr.Zero);
        }

        public IntPtr Send(SciMsg msg, IntPtr wParam, IntPtr lParam) {
            return _directFunction(_directMessagePointer, (uint)msg, wParam, lParam);
        }

        public IntPtr Send(SciMsg msg, IntPtr wParam) {
            return _directFunction(_directMessagePointer, (uint)msg, wParam, IntPtr.Zero);
        }

        public IntPtr Send(SciMsg msg) {
            return _directFunction(_directMessagePointer, (uint)msg, IntPtr.Zero, IntPtr.Zero);
        }
    }

}
