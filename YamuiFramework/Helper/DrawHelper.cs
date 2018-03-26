using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace YamuiFramework.Helper {
    public class DrawHelper {

        public static void HandleWmPaint(ref Message m, Control control, Action<PaintEventArgs> paint) {
            var ps = new WinApi.PAINTSTRUCT();
            bool needDisposeDc = false;
            try {
                Rectangle clip;
                IntPtr dc;
                if (m.WParam == IntPtr.Zero) {
                    dc = WinApi.BeginPaint(new HandleRef(control, control.Handle), ref ps);
                    if (dc == IntPtr.Zero) {
                        return;
                    }
                    needDisposeDc = true;
                    clip = new Rectangle(ps.rcPaint_left, ps.rcPaint_top, ps.rcPaint_right - ps.rcPaint_left, ps.rcPaint_bottom - ps.rcPaint_top);
                } else {
                    dc = m.WParam;
                    clip = control.ClientRectangle;
                }

                if (clip.Width > 0 && clip.Height > 0) {
                    try {
                        using (var bufferedGraphics = BufferedGraphicsManager.Current.Allocate(dc, control.ClientRectangle)) {
                            bufferedGraphics.Graphics.SetClip(clip);
                            using (var pevent = new PaintEventArgs(bufferedGraphics.Graphics, clip)) {
                                paint?.Invoke(pevent);
                            }
                            bufferedGraphics.Render();
                        }
                    } catch (Exception ex) {
                        // BufferContext.Allocate will throw out of memory exceptions
                        // when it fails to create a device dependent bitmap while trying to 
                        // get information about the device we are painting on.
                        // That is not the same as a system running out of memory and there is a 
                        // very good chance that we can continue to paint successfully. We cannot
                        // check whether double buffering is supported in this case, and we will disable it.
                        // We could set a specific string when throwing the exception and check for it here
                        // to distinguish between that case and real out of memory exceptions but we
                        // see no reasons justifying the additional complexity.
                        if (!(ex is OutOfMemoryException)) {
                            throw;
                        }
                    }
                }
            } finally {
                if (needDisposeDc) {
                    WinApi.EndPaint(new HandleRef(control, control.Handle), ref ps);
                }
            }
        }

        public static void DrawImage(Graphics g) {
            // improve performances
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low; // or NearestNeighbour
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
        }

        #region Resume/Suspend drawing

        /// <summary>Allows suspending and resuming redrawing a Windows Forms window via the <b>WM_SETREDRAW</b> 
        /// Windows message.</summary>
        /// <remarks>Usage: The window for which drawing will be suspended and resumed needs to instantiate this type, 
        /// passing a reference to itself to the constructor, then call either of the public methods. For each call to 
        /// <b>SuspendDrawing</b>, a corresponding <b>ResumeDrawing</b> call must be made. Calls may be nested, but
        /// should not be made from any other than the GUI thread. (This code tries to work around such an error, but 
        /// is not guaranteed to succeed.)</remarks>
        public class DrawingSuspender {
                
            private int _suspendCounter;
            
            private IWin32Window _owner;

            private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;


            public DrawingSuspender(IWin32Window owner) {
                this._owner = owner;
            }
            
            /// <summary>This overload allows you to specify whether the optimal flags for a container 
            /// or child control should be used. To specify custom flags, use the overload that accepts 
            /// a <see cref="WinApi.RedrawWindowFlags"/> parameter.</summary>
            /// <param name="isContainer">When <b>true</b>, the optimal flags for redrawing a container 
            /// control are used; otherwise the optimal flags for a child control are used.</param>
            public void ResumeDrawing(bool isContainer = false) {
                ResumeDrawing(isContainer ? WinApi.RedrawWindowFlags.Erase | WinApi.RedrawWindowFlags.Frame | WinApi.RedrawWindowFlags.Invalidate | WinApi.RedrawWindowFlags.AllChildren : WinApi.RedrawWindowFlags.NoErase | WinApi.RedrawWindowFlags.Invalidate | WinApi.RedrawWindowFlags.InternalPaint);
            }

            public void ResumeDrawing(WinApi.RedrawWindowFlags flags) {
                Interlocked.Decrement(ref _suspendCounter);

                if (_suspendCounter == 0) {
                    Action resume = new Action(() => {
                        WinApi.SendMessage(_owner.Handle, (int) WinApi.Messages.WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                        WinApi.RedrawWindow(_owner.Handle, IntPtr.Zero, IntPtr.Zero, flags);
                    });
                    try {
                        resume();
                    } catch (InvalidOperationException) {
                        _synchronizationContext.Post(s => ((Action) s)(), resume);
                    }
                }
            }

            public void SuspendDrawing() {
                try {
                    if (_suspendCounter == 0) {
                        Action suspend = new Action(() => WinApi.SendMessage(_owner.Handle, (int) WinApi.Messages.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero));
                        try {
                            suspend();
                        } catch (InvalidOperationException) {
                            _synchronizationContext.Post(s => ((Action) s)(), suspend);
                        }
                    }
                } finally {
                    Interlocked.Increment(ref _suspendCounter);
                }
            }
        } 

        #endregion
        
    }
}