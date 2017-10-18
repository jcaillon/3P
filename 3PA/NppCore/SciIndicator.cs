#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (SciIndicator.cs) is part of 3P.
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
using System.Drawing;
using _3PA.Lib;

namespace _3PA.NppCore {
    internal static partial class Sci {

        /// <summary>
        /// Represents an indicator in a Scintilla control.
        /// </summary>
        public class Indicator {
            #region Constants

            /// <summary>
            /// An OR mask to use with Scintilla.IndicatorValue and IndicatorFlags.ValueFore to indicate
            /// that the user-defined indicator value should be treated as a RGB color.
            /// </summary>
            public const int ValueBit = (int)SciMsg.SC_INDICVALUEBIT;

            /// <summary>
            /// An AND mask to use with Indicator.ValueAt to retrieve the user-defined value as a RGB color when being treated as
            /// such.
            /// </summary>
            public const int ValueMask = (int)SciMsg.SC_INDICVALUEMASK;

            #endregion Constants

            #region Methods

            /// <summary>
            /// Find the end of an indicator range from a position
            /// Can be used to iterate through the document to discover all the indicator positions.
            /// </summary>
            /// <param name="position">A zero-based document position using this indicator.</param>
            /// <returns>The zero-based document position where the use of this indicator ends.</returns>
            /// <remarks>
            /// Specifying a <paramref name="position" /> which is not filled with this indicator will cause this method
            /// to return the end position of the range where this indicator is not in use (the negative space). If this
            /// indicator is not in use anywhere within the document the return value will be 0.
            /// </remarks>
            public int End(int position) {
                position = Clamp(position, 0, TextLength);
                position = Api.Lines.CharToBytePosition(position);
                position = Api.Send(SciMsg.SCI_INDICATOREND, new IntPtr(Index), new IntPtr(position)).ToInt32();
                return Api.Lines.ByteToCharPosition(position);
            }

            /// <summary>
            /// Find the start of an indicator range from a position
            /// Can be used to iterate through the document to discover all the indicator positions.
            /// </summary>
            /// <param name="position">A zero-based document position using this indicator.</param>
            /// <returns>The zero-based document position where the use of this indicator starts.</returns>
            /// <remarks>
            /// Specifying a <paramref name="position" /> which is not filled with this indicator will cause this method
            /// to return the start position of the range where this indicator is not in use (the negative space). If this
            /// indicator is not in use anywhere within the document the return value will be 0.
            /// </remarks>
            public int Start(int position) {
                position = Clamp(position, 0, TextLength);
                position = Api.Lines.CharToBytePosition(position);
                position = Api.Send(SciMsg.SCI_INDICATORSTART, new IntPtr(Index), new IntPtr(position)).ToInt32();
                return Api.Lines.ByteToCharPosition(position);
            }

            /// <summary>
            /// Returns the user-defined value for the indicator at the specified position.
            /// </summary>
            /// <param name="position">The zero-based document position to get the indicator value for.</param>
            /// <returns>The user-defined value at the specified <paramref name="position" />.</returns>
            public int ValueAt(int position) {
                position = Clamp(position, 0, TextLength);
                position = Api.Lines.CharToBytePosition(position);
                return Api.Send(SciMsg.SCI_INDICATORVALUEAT, new IntPtr(Index), new IntPtr(position)).ToInt32();
            }

            /// <summary>
            /// Adds the indicator and IndicatorValue value to the specified range of text.
            /// </summary>
            /// <param name="start"></param>
            /// <param name="end"></param>
            public void Add(int start, int end) {
                IndicatorCurrent = Index;
                IndicatorFillRange(start, end - start);
            }

            /// <summary>
            /// Clears the indicator and IndicatorValue value from the specified range of text.
            /// </summary>
            /// <param name="start"></param>
            /// <param name="end"></param>
            public void Clear(int start, int end) {
                IndicatorCurrent = Index;
                IndicatorClearRange(start, end - start);
            }

            /// <summary>
            /// List of points(start, end) that represents the range were the given indicator has been found
            /// </summary>
            /// <returns></returns>
            public List<Point> FindRanges() {
                var ranges = new List<Point>();
                var testPosition = 0;
                while (true) {
                    var rangeEnd = End(testPosition);
                    ranges.Add(new Point(Start(testPosition), rangeEnd));
                    if (testPosition == rangeEnd) break;
                    testPosition = rangeEnd;
                }
                return ranges;
            }

            /// <summary>
            /// Returns true if the indicator is present at the given position
            /// </summary>
            /// <param name="pos"></param>
            /// <returns></returns>
            public bool DefinedAt(int pos) {
                return ((int)IndicatorAllOnFor(pos)).IsBitSet(Index);
            }

            #endregion Methods

            #region Properties

            /// <summary>
            /// Gets or sets the alpha transparency of the indicator used for drawing the fill colour of the INDIC_ROUNDBOX and
            /// INDIC_STRAIGHTBOX rectangle
            /// </summary>
            /// <returns>
            /// The alpha transparency ranging from 0 (completely transparent)
            /// to 255 (no transparency). The default is 30.
            /// </returns>
            public int Alpha {
                get { return Api.Send(SciMsg.SCI_INDICGETALPHA, new IntPtr(Index)).ToInt32(); }
                set {
                    value = Clamp(value, 0, 255);
                    Api.Send(SciMsg.SCI_INDICSETALPHA, new IntPtr(Index), new IntPtr(value));
                }
            }

            /// <summary>
            /// Gets or sets the indicator flags.
            /// </summary>
            /// <returns>
            /// A bitwise combination of the IndicatorFlags enumeration.
            /// The default is IndicatorFlags.None.
            /// </returns>
            public IndicatorFlags Flags {
                get { return (IndicatorFlags)Api.Send(SciMsg.SCI_INDICGETFLAGS, new IntPtr(Index)); }
                set {
                    var flags = (int)value;
                    Api.Send(SciMsg.SCI_INDICSETFLAGS, new IntPtr(Index), new IntPtr(flags));
                }
            }

            /// <summary>
            /// Gets or sets the color used to draw an indicator.
            /// </summary>
            /// <returns>The Color used to draw an indicator. The default varies.</returns>
            /// <remarks>Changing the ForeColor property will reset the HoverForeColor.</remarks>
            public Color ForeColor {
                get {
                    var color = Api.Send(SciMsg.SCI_INDICGETFORE, new IntPtr(Index)).ToInt32();
                    return ColorTranslator.FromWin32(color);
                }
                set {
                    var color = ColorTranslator.ToWin32(value);
                    Api.Send(SciMsg.SCI_INDICSETFORE, new IntPtr(Index), new IntPtr(color));
                }
            }

            /// <summary>
            /// Gets or sets the color used to draw an indicator when the mouse or caret is over an indicator.
            /// </summary>
            /// <returns>
            /// The Color used to draw an indicator.
            /// By default, the hover style is equal to the regular ForeColor.
            /// </returns>
            /// <remarks>Changing the ForeColor property will reset the HoverForeColor.</remarks>
            public Color HoverForeColor {
                get {
                    var color = Api.Send(SciMsg.SCI_INDICGETHOVERFORE, new IntPtr(Index)).ToInt32();
                    return ColorTranslator.FromWin32(color);
                }
                set {
                    var color = ColorTranslator.ToWin32(value);
                    Api.Send(SciMsg.SCI_INDICSETHOVERFORE, new IntPtr(Index), new IntPtr(color));
                }
            }

            /// <summary>
            /// Gets or sets the indicator style used when the mouse or caret is over an indicator.
            /// </summary>
            /// <returns>
            /// One of the IndicatorStyle enumeration values.
            /// By default, the hover style is equal to the regular Style.
            /// </returns>
            /// <remarks>Changing the Style property will reset the HoverStyle.</remarks>
            public IndicatorStyle HoverStyle {
                get { return (IndicatorStyle)Api.Send(SciMsg.SCI_INDICGETHOVERSTYLE, new IntPtr(Index)); }
                set {
                    var style = (int)value;
                    Api.Send(SciMsg.SCI_INDICSETHOVERSTYLE, new IntPtr(Index), new IntPtr(style));
                }
            }

            /// <summary>
            /// Gets the zero-based indicator index this object represents.
            /// </summary>
            /// <returns>The indicator definition index within the IndicatorCollection.</returns>
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the alpha transparency of the indicator outline used for drawing the outline colour
            /// of the INDIC_ROUNDBOX and INDIC_STRAIGHTBOX rectangle
            /// </summary>
            /// <returns>
            /// The alpha transparency ranging from 0 (completely transparent)
            /// to 255 (no transparency). The default is 50.
            /// </returns>
            public int OutlineAlpha {
                get { return Api.Send(SciMsg.SCI_INDICGETOUTLINEALPHA, new IntPtr(Index)).ToInt32(); }
                set {
                    value = Clamp(value, 0, 255);
                    Api.Send(SciMsg.SCI_INDICSETOUTLINEALPHA, new IntPtr(Index), new IntPtr(value));
                }
            }

            /// <summary>
            /// Gets or sets the indicator style.
            /// </summary>
            /// <returns>One of the IndicatorStyle enumeration values. The default varies.</returns>
            /// <remarks>Changing the Style property will reset the HoverStyle.</remarks>
            public IndicatorStyle IndicatorStyle {
                get { return (IndicatorStyle)Api.Send(SciMsg.SCI_INDICGETSTYLE, new IntPtr(Index)); }
                set {
                    var style = (int)value;
                    Api.Send(SciMsg.SCI_INDICSETSTYLE, new IntPtr(Index), new IntPtr(style));
                }
            }

            /// <summary>
            /// Gets or sets whether indicators are drawn under or over text.
            /// </summary>
            /// <returns>true to draw the indicator under text; otherwise, false. The default is false.</returns>
            /// <remarks>Drawing indicators under text requires Phases.One or Phases.Multiple drawing.</remarks>
            public bool Under {
                get { return Api.Send(SciMsg.SCI_INDICGETUNDER, new IntPtr(Index)) != IntPtr.Zero; }
                set {
                    var under = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_INDICSETUNDER, new IntPtr(Index), under);
                }
            }

            #endregion Properties

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the Indicator class.
            /// </summary>
            /// <param name="index">The index of this style within the IndicatorCollection that created it.</param>
            public Indicator(int index) {
                Index = index;
            }

            #endregion Constructors

            #region Static

            /// <summary>
            /// Gets or sets the indicator used in a subsequent call to IndicatorFillRange or IndicatorClearRange.
            /// </summary>
            /// <returns>
            /// The zero-based indicator index to apply when calling IndicatorFillRange or remove when calling
            /// IndicatorClearRange.
            /// </returns>
            public static int IndicatorCurrent {
                get { return Api.Send(SciMsg.SCI_GETINDICATORCURRENT).ToInt32(); }
                set {
                    value = Clamp(value, 0, 31);
                    Api.Send(SciMsg.SCI_SETINDICATORCURRENT, new IntPtr(value));
                }
            }

            /// <summary>
            /// Gets or sets the user-defined value used in a subsequent call to IndicatorFillRange.
            /// </summary>
            /// <returns>The indicator value to apply when calling IndicatorFillRange.</returns>
            public static int IndicatorValue {
                get { return Api.Send(SciMsg.SCI_GETINDICATORVALUE).ToInt32(); }
                set { Api.Send(SciMsg.SCI_SETINDICATORVALUE, new IntPtr(value)); }
            }

            /// <summary>
            /// Returns a bitmap representing the 32 indicators in use at the specified position.
            /// </summary>
            /// <param name="position">The zero-based character position within the document to test.</param>
            /// <returns>A bitmap indicating which of the 32 indicators are in use at the specified <paramref name="position" />.</returns>
            public static uint IndicatorAllOnFor(int position) {
                position = Clamp(position, 0, TextLength);
                position = Api.Lines.CharToBytePosition(position);

                var bitmap = Api.Send(SciMsg.SCI_INDICATORALLONFOR, new IntPtr(position)).ToInt32();
                return unchecked((uint)bitmap);
            }

            /// <summary>
            /// Removes the IndicatorCurrent indicator (and user-defined value) from the specified range of text.
            /// </summary>
            /// <param name="position">The zero-based character position within the document to start clearing.</param>
            /// <param name="length">The number of characters to clear.</param>
            public static void IndicatorClearRange(int position, int length) {
                var textLength = TextLength;
                position = Clamp(position, 0, textLength);
                length = Clamp(length, 0, textLength - position);

                var startPos = Api.Lines.CharToBytePosition(position);
                var endPos = Api.Lines.CharToBytePosition(position + length);

                Api.Send(SciMsg.SCI_INDICATORCLEARRANGE, new IntPtr(startPos), new IntPtr(endPos - startPos));
            }

            /// <summary>
            /// Adds the IndicatorCurrent indicator and IndicatorValue value to the specified range of text.
            /// </summary>
            /// <param name="position">The zero-based character position within the document to start filling.</param>
            /// <param name="length">The number of characters to fill.</param>
            public static void IndicatorFillRange(int position, int length) {
                var textLength = TextLength;
                position = Clamp(position, 0, textLength);
                length = Clamp(length, 0, textLength - position);

                var startPos = Api.Lines.CharToBytePosition(position);
                var endPos = Api.Lines.CharToBytePosition(position + length);

                Api.Send(SciMsg.SCI_INDICATORFILLRANGE, new IntPtr(startPos), new IntPtr(endPos - startPos));
            }

            #endregion
        }
    }
}
