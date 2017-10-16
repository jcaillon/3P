using System;
using System.Drawing;

namespace _3PA.NppCore {
    internal static partial class Sci {

        /// <summary>
        /// Represents a margin marker in a Scintilla control.
        /// </summary>
        public class Marker {
            #region Constants

            /// <summary>
            /// An unsigned 32-bit mask of all Margin indexes where each bit cooresponds to a margin index.
            /// </summary>
            public const uint MaskAll = unchecked((uint)-1);

            /// <summary>
            /// An unsigned 32-bit mask of folder Margin indexes (25 through 31) where each bit cooresponds to a margin index.
            /// </summary>
            public const uint MaskFolders = 0xFE000000; // SciMsg.SC_MASK_FOLDERS;

            /// <summary>
            /// Folder end marker index. This marker is typically configured to display the MarkerSymbol.BoxPlusConnected symbol.
            /// </summary>
            public const int FolderEnd = (int)SciMsg.SC_MARKNUM_FOLDEREND;

            /// <summary>
            /// Folder open marker index. This marker is typically configured to display the MarkerSymbol.BoxMinusConnected symbol.
            /// </summary>
            public const int FolderOpenMid = (int)SciMsg.SC_MARKNUM_FOLDEROPENMID;

            /// <summary>
            /// Folder mid tail marker index. This marker is typically configured to display the MarkerSymbol.TCorner symbol.
            /// </summary>
            public const int FolderMidTail = (int)SciMsg.SC_MARKNUM_FOLDERMIDTAIL;

            /// <summary>
            /// Folder tail marker index. This marker is typically configured to display the MarkerSymbol.LCorner symbol.
            /// </summary>
            public const int FolderTail = (int)SciMsg.SC_MARKNUM_FOLDERTAIL;

            /// <summary>
            /// Folder sub marker index. This marker is typically configured to display the MarkerSymbol.VLine symbol.
            /// </summary>
            public const int FolderSub = (int)SciMsg.SC_MARKNUM_FOLDERSUB;

            /// <summary>
            /// Folder marker index. This marker is typically configured to display the MarkerSymbol.BoxPlus symbol.
            /// </summary>
            public const int Folder = (int)SciMsg.SC_MARKNUM_FOLDER;

            /// <summary>
            /// Folder open marker index. This marker is typically configured to display the MarkerSymbol.BoxMinus symbol.
            /// </summary>
            public const int FolderOpen = (int)SciMsg.SC_MARKNUM_FOLDEROPEN;

            #endregion

            #region public

            /// <summary>
            /// Sets the marker symbol to a custom image.
            /// </summary>
            /// <param name="image">The Bitmap to use as a marker symbol.</param>
            /// <remarks>Calling this method will also update the Symbol property to MarkerSymbol.RgbaImage.</remarks>
            public unsafe void DefineRgbaImage(Bitmap image) {
                if (image == null)
                    return;

                Api.Send(SciMsg.SCI_RGBAIMAGESETWIDTH, new IntPtr(image.Width));
                Api.Send(SciMsg.SCI_RGBAIMAGESETHEIGHT, new IntPtr(image.Height));

                var bytes = BitmapToArgb(image);
                fixed (byte* bp = bytes)
                    Api.Send(SciMsg.SCI_MARKERDEFINERGBAIMAGE, new IntPtr(Index), new IntPtr(bp));
            }

            /// <summary>
            /// Sets the foreground alpha transparency for markers that are drawn in the content area.
            /// </summary>
            /// <param name="alpha">The alpha transparency ranging from 0 (completely transparent) to 255 (no transparency).</param>
            /// <remarks>
            /// See the remarks on the SetBackColor method for a full explanation of when a marker can be drawn in the content
            /// area.
            /// </remarks>
            public void SetAlpha(int alpha) {
                alpha = Clamp(alpha, 0, 255);
                Api.Send(SciMsg.SCI_MARKERSETALPHA, new IntPtr(Index), new IntPtr(alpha));
            }

            /// <summary>
            /// Sets the background color of the marker.
            /// </summary>
            /// <param name="color">The Marker background Color. The default is White.</param>
            /// <remarks>
            /// The background color of the whole line will be drawn in the <paramref name="color" /> specified when the marker is
            /// not visible
            /// because it is hidden by a Margin.Mask or the Margin.Width is zero.
            /// </remarks>
            public void SetBackColor(Color color) {
                var colour = ColorTranslator.ToWin32(color);
                Api.Send(SciMsg.SCI_MARKERSETBACK, new IntPtr(Index), new IntPtr(colour));
            }

            /// <summary>
            /// This message sets the highlight background colour of a marker number when its folding block is selected. The default colour is #FF0000
            /// </summary>
            public void SetBackSelectedColor(Color color) {
                var colour = ColorTranslator.ToWin32(color);
                Api.Send(SciMsg.SCI_MARKERSETBACKSELECTED, new IntPtr(Index), new IntPtr(colour));
            }

            /// <summary>
            /// Sets the foreground color of the marker.
            /// </summary>
            /// <param name="color">The Marker foreground Color. The default is Black.</param>
            public void SetForeColor(Color color) {
                var colour = ColorTranslator.ToWin32(color);
                Api.Send(SciMsg.SCI_MARKERSETFORE, new IntPtr(Index), new IntPtr(colour));
            }

            /// <summary>
            /// Gets the zero-based marker index this object represents.
            /// </summary>
            /// <returns>The marker index within the MarkerCollection.</returns>
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the marker symbol.
            /// </summary>
            /// <returns>
            /// One of the MarkerSymbol enumeration values.
            /// The default is MarkerSymbol.Circle.
            /// </returns>
            public MarkerSymbol Symbol {
                get { return (MarkerSymbol)Api.Send(SciMsg.SCI_MARKERSYMBOLDEFINED, new IntPtr(Index)); }
                set {
                    var markerSymbol = (int)value;
                    Api.Send(SciMsg.SCI_MARKERDEFINE, new IntPtr(Index), new IntPtr(markerSymbol));
                }
            }

            #endregion

            #region constructor

            /// <summary>
            /// Initializes a new instance of the Marker class
            /// There are 32 markers, numbered 0 to MARKER_MAX (31)
            /// Marker numbers 0 to 24 have no pre-defined function; you can use them to mark syntax errors and so on..
            /// </summary>
            /// <param name="index">The index of this style within the MarkerCollection that created it.</param>
            public Marker(int index) {
                Index = index;
            }

            #endregion

            #region static

            /// <summary>
            /// Removes the specified marker from all Api.Lines.
            /// </summary>
            /// <param name="marker">The zero-based Marker index to remove from all lines, or -1 to remove all markers from all Api.Lines.</param>
            public static void MarkerDeleteAll(int marker) {
                Api.Send(SciMsg.SCI_MARKERDELETEALL, new IntPtr(marker));
            }

            #endregion
        }
    }
}
