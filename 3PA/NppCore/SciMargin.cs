#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (SciMargin.cs) is part of 3P.
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
using _3PA.WindowsCore;

namespace _3PA.NppCore {

    internal static partial class Sci {
        /// <summary>
        /// Represents a margin displayed on the left edge of a Scintilla control.
        /// </summary>
        public class Margin {
            /// <summary>
            /// Removes all text displayed in every MarginType.Text and MarginType.RightText margins.
            /// </summary>
            public void ClearAllText() {
                Api.Send(SciMsg.SCI_MARGINTEXTCLEARALL);
            }

            /// <summary>
            /// Gets or sets the mouse cursor style when over the margin.
            /// </summary>
            /// <returns>One of the MarginCursor enumeration values. The default is MarginCursor.Arrow.</returns>
            public MarginCursor Cursor {
                get { return (MarginCursor)Api.Send(SciMsg.SCI_GETMARGINCURSORN, new IntPtr(Index)); }
                set {
                    var cursor = (int)value;
                    Api.Send(SciMsg.SCI_SETMARGINCURSORN, new IntPtr(Index), new IntPtr(cursor));
                }
            }

            /// <summary>
            /// Gets the zero-based margin index this object represents.
            /// </summary>
            /// <returns>The margin index within the MarginCollection.</returns>
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets whether the margin is sensitive to mouse clicks.
            /// </summary>
            /// <returns>true if the margin is sensitive to mouse clicks; otherwise, false. The default is false.</returns>
            public bool Sensitive {
                get { return Api.Send(SciMsg.SCI_GETMARGINSENSITIVEN, new IntPtr(Index)) != IntPtr.Zero; }
                set {
                    var sensitive = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_SETMARGINSENSITIVEN, new IntPtr(Index), sensitive);
                }
            }

            /// <summary>
            /// Gets or sets the margin type.
            /// </summary>
            /// <returns>One of the MarginType enumeration values. The default is MarginType.Symbol.</returns>
            public MarginType Type {
                get { return (MarginType)Api.Send(SciMsg.SCI_GETMARGINTYPEN, new IntPtr(Index)); }
                set {
                    var type = (int)value;
                    Api.Send(SciMsg.SCI_SETMARGINTYPEN, new IntPtr(Index), new IntPtr(type));
                }
            }

            /// <summary>
            /// Gets or sets the width in pixels of the margin.
            /// </summary>
            /// <returns>The width of the margin measured in pixels.</returns>
            /// <remarks>Scintilla assigns various default widths.</remarks>
            public int Width {
                get { return Api.Send(SciMsg.SCI_GETMARGINWIDTHN, new IntPtr(Index)).ToInt32(); }
                set {
                    value = ClampMin(value, 0);
                    Win32Api.SendMessage(Api.Handle, SciMsg.SCI_SETMARGINWIDTHN, Index, value);
                }
            }

            /// <summary>
            /// Gets or sets a mask indicating which markers this margin can display.
            /// </summary>
            /// <returns>
            /// An unsigned 32-bit value with each bit cooresponding to one of the 32 zero-based Margin indexes.
            /// The default is 0x1FFFFFF, which is every marker except folder markers (i.e. 0 through 24).
            /// </returns>
            /// <remarks>
            /// For example, the mask for marker index 10 is 1 shifted left 10 times (1 &lt;&lt; 10).
            /// Marker.MaskFolders is a useful constant for working with just folder margin indexes.
            /// </remarks>
            public uint Mask {
                get { return unchecked((uint)Api.Send(SciMsg.SCI_GETMARGINMASKN, new IntPtr(Index)).ToInt32()); }
                set {
                    var mask = unchecked((int)value);
                    Api.Send(SciMsg.SCI_SETMARGINMASKN, new IntPtr(Index), new IntPtr(mask));
                }
            }

            /// <summary>
            /// Initializes a new instance of the Margin class.
            /// </summary>
            /// <param name="index">The index of this margin within the MarginCollection that created it.</param>
            public Margin(int index) {
                Index = index;
            }
        }
    }
}
