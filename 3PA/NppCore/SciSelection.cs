using System;

namespace _3PA.NppCore {
    internal static partial class Sci {

        /// <summary>
        /// Represents a selection when there are multiple active selections in a Scintilla control.
        /// </summary>
        public class Selection {
            /// <summary>
            /// Sets both anchor and caret position to the same position
            /// </summary>
            /// <param name="pos"></param>
            public void SetPosition(int pos) {
                Anchor = pos;
                Caret = pos;
            }

            /// <summary>
            /// Gets or sets the anchor position of the selection.
            /// </summary>
            /// <returns>The zero-based document position of the selection anchor.</returns>
            public int Anchor {
                get {
                    var pos = Api.Send(SciMsg.SCI_GETSELECTIONNANCHOR, new IntPtr(Index)).ToInt32();
                    if (pos <= 0)
                        return pos;

                    return Api.Lines.ByteToCharPosition(pos);
                }
                set {
                    value = Clamp(value, 0, TextLength);
                    value = Api.Lines.CharToBytePosition(value);
                    Api.Send(SciMsg.SCI_SETSELECTIONNANCHOR, new IntPtr(Index), new IntPtr(value));
                }
            }

            /// <summary>
            /// Gets or sets the amount of anchor virtual space.
            /// </summary>
            /// <returns>The amount of virtual space past the end of the line offsetting the selection anchor.</returns>
            public int AnchorVirtualSpace {
                get { return Api.Send(SciMsg.SCI_GETSELECTIONNANCHORVIRTUALSPACE, new IntPtr(Index)).ToInt32(); }
                set {
                    value = ClampMin(value, 0);
                    Api.Send(SciMsg.SCI_SETSELECTIONNANCHORVIRTUALSPACE, new IntPtr(Index), new IntPtr(value));
                }
            }

            /// <summary>
            /// Gets or sets the caret position of the selection.
            /// </summary>
            /// <returns>The zero-based document position of the selection caret.</returns>
            public int Caret {
                get {
                    var pos = Api.Send(SciMsg.SCI_GETSELECTIONNCARET, new IntPtr(Index)).ToInt32();
                    if (pos <= 0)
                        return pos;

                    return Api.Lines.ByteToCharPosition(pos);
                }
                set {
                    value = Clamp(value, 0, TextLength);
                    value = Api.Lines.CharToBytePosition(value);
                    Api.Send(SciMsg.SCI_SETSELECTIONNCARET, new IntPtr(Index), new IntPtr(value));
                }
            }

            /// <summary>
            /// Gets or sets the amount of caret virtual space.
            /// </summary>
            /// <returns>The amount of virtual space past the end of the line offsetting the selection caret.</returns>
            public int CaretVirtualSpace {
                get { return Api.Send(SciMsg.SCI_GETSELECTIONNCARETVIRTUALSPACE, new IntPtr(Index)).ToInt32(); }
                set {
                    value = ClampMin(value, 0);
                    Api.Send(SciMsg.SCI_SETSELECTIONNCARETVIRTUALSPACE, new IntPtr(Index), new IntPtr(value));
                }
            }

            /// <summary>
            /// Gets or sets the end position of the selection, regardeless of wheter it's the anchor or the caret
            /// </summary>
            /// <returns>The zero-based document position where the selection ends.</returns>
            public int End {
                get {
                    var carPos = Caret;
                    var ancPos = Anchor;
                    return carPos < ancPos ? ancPos : carPos;
                }
                set {
                    if (Caret > Anchor)
                        Caret = value;
                    else
                        Anchor = value;
                }
            }

            /// <summary>
            /// Gets the selection index.
            /// </summary>
            /// <returns>The zero-based selection index within the SelectionCollection that created it.</returns>
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the start position of the selection.
            /// </summary>
            /// <returns>The zero-based document position where the selection starts.</returns>
            public int Start {
                get {
                    var carPos = Caret;
                    var ancPos = Anchor;
                    return carPos > ancPos ? ancPos : carPos;
                }
                set {
                    if (Caret < Anchor)
                        Caret = value;
                    else
                        Anchor = value;
                }
            }

            /// <summary>
            /// Initializes a new instance of the Selection class.
            /// </summary>
            /// <param name="index">The index of this selection within the SelectionCollection that created it.</param>
            public Selection(int index) {
                Index = index;
            }

            #region static

            /// <summary>
            /// Gets the number of active selections.
            /// </summary>
            /// <returns>The number of selections in the SelectionCollection.</returns>
            public static int Count {
                get { return Api.Send(SciMsg.SCI_GETSELECTIONS).ToInt32(); }
            }

            /// <summary>
            /// Gets a value indicating whether all selection ranges are empty.
            /// </summary>
            /// <returns>true if all selection ranges are empty; otherwise, false.</returns>
            public static bool IsEmpty {
                get { return Api.Send(SciMsg.SCI_GETSELECTIONEMPTY) != IntPtr.Zero; }
            }

            #endregion
        }
    }
}
