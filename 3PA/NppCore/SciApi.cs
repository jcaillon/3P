using System;
using System.Runtime.InteropServices;
using _3PA.WindowsCore;

namespace _3PA.NppCore {
    /// <summary>
    /// Use this class to communicate with scintilla
    /// </summary>
    internal class SciApi {
        #region Fields

        private Win32Api.Scintilla_DirectFunction _directFunction;
        private IntPtr _directMessagePointer;

        #endregion

        #region Life and death

        public SciApi(IntPtr handle) {
            Handle = handle;
            Lines = new DocumentLines();
            UpdateScintillaDirectMessage(handle);
        }

        #endregion

        #region Public

        public DocumentLines Lines { get; private set; }

        public IntPtr Handle { get; private set; }

        public IntPtr Send(SciMsg msg, IntPtr wParam, IntPtr lParam) {
            return _directFunction(_directMessagePointer, (uint) msg, wParam, lParam);
        }

        public IntPtr Send(SciMsg msg, IntPtr wParam) {
            return _directFunction(_directMessagePointer, (uint) msg, wParam, IntPtr.Zero);
        }

        public IntPtr Send(SciMsg msg) {
            return _directFunction(_directMessagePointer, (uint) msg, IntPtr.Zero, IntPtr.Zero);
        }

        #endregion

        #region Private

        /// <summary>
        /// Instantiates the direct message function
        /// </summary>
        private void UpdateScintillaDirectMessage(IntPtr scintillaHandle) {
            Handle = scintillaHandle;
            var directFunctionPointer = Win32Api.SendMessage(Handle, (uint) SciMsg.SCI_GETDIRECTFUNCTION, IntPtr.Zero, IntPtr.Zero);
            // Create a managed callback
            _directFunction = (Win32Api.Scintilla_DirectFunction) Marshal.GetDelegateForFunctionPointer(directFunctionPointer, typeof(Win32Api.Scintilla_DirectFunction));
            _directMessagePointer = Win32Api.SendMessage(Handle, (uint) SciMsg.SCI_GETDIRECTPOINTER, IntPtr.Zero, IntPtr.Zero);
        }

        #endregion
    }
}