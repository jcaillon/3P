using System;
using System.Drawing;
using System.Text;

namespace _3PA.NppCore {

    internal static partial class Sci {

        /// <summary>
        /// A style definition in a Scintilla control.
        /// </summary>
        public class Style {
            #region Constants

            /// <summary>
            /// Default style index. This style is used to define properties that all styles receive when calling
            /// Scintilla.StyleClearAll.
            /// </summary>
            public const int Default = (int)SciMsg.STYLE_DEFAULT;

            /// <summary>
            /// Line number style index. This style is used for text in line number margins. The background color of this style
            /// also
            /// sets the background color for all margins that do not have any folding mask set.
            /// </summary>
            public const int LineNumber = (int)SciMsg.STYLE_LINENUMBER;

            /// <summary>
            /// Call tip style index. Only font name, size, foreground color, background color, and character set attributes
            /// can be used when displaying a call tip.
            /// </summary>
            public const int CallTip = (int)SciMsg.STYLE_CALLTIP;

            /// <summary>
            /// Indent guide style index. This style is used to specify the foreground and background colors of
            /// Scintilla.IndentationGuides.
            /// </summary>
            public const int IndentGuide = (int)SciMsg.STYLE_INDENTGUIDE;

            /// <summary>
            /// Brace highlighting style index. This style is used on a brace character when set with the Scintilla.BraceHighlight
            /// method
            /// or the indentation guide when used with the Scintilla.HighlightGuide property.
            /// </summary>
            public const int BraceLight = (int)SciMsg.STYLE_BRACELIGHT;

            /// <summary>
            /// Bad brace style index. This style is used on an unmatched brace character when set with the Scintilla.BraceBadLight
            /// method.
            /// </summary>
            public const int BraceBad = (int)SciMsg.STYLE_BRACEBAD;

            #endregion Constants

            #region Properties

            /// <summary>
            /// Gets or sets the background color of the style.
            /// </summary>
            /// <returns>A Color object representing the style background color. The default is White.</returns>
            /// <remarks>Alpha color values are ignored.</remarks>
            public Color BackColor {
                get {
                    var color = Api.Send(SciMsg.SCI_STYLEGETBACK, new IntPtr(Index), IntPtr.Zero).ToInt32();
                    return ColorTranslator.FromWin32(color);
                }
                set {
                    if (value.IsEmpty)
                        value = Color.White;

                    var color = ColorTranslator.ToWin32(value);
                    Api.Send(SciMsg.SCI_STYLESETBACK, new IntPtr(Index), new IntPtr(color));
                }
            }

            /// <summary>
            /// Gets or sets whether the style font is bold.
            /// </summary>
            /// <returns>true if bold; otherwise, false. The default is false.</returns>
            /// <remarks>Setting this property affects the Weight property.</remarks>
            public bool Bold {
                get { return Api.Send(SciMsg.SCI_STYLEGETBOLD, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero; }
                set {
                    var bold = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_STYLESETBOLD, new IntPtr(Index), bold);
                }
            }

            /// <summary>
            /// Gets or sets the casing used to display the styled text.
            /// </summary>
            /// <returns>One of the StyleCase enum values. The default is StyleCase.Mixed.</returns>
            /// <remarks>This does not affect how text is stored, only displayed.</remarks>
            public StyleCase Case {
                get {
                    var @case = Api.Send(SciMsg.SCI_STYLEGETCASE, new IntPtr(Index), IntPtr.Zero).ToInt32();
                    return (StyleCase)@case;
                }
                set {
                    // Just an excuse to use @... syntax
                    var @case = (int)value;
                    Api.Send(SciMsg.SCI_STYLESETCASE, new IntPtr(Index), new IntPtr(@case));
                }
            }

            /// <summary>
            /// Gets or sets whether the remainder of the line is filled with the BackColor
            /// when this style is used on the last character of a line.
            /// </summary>
            /// <returns>true to fill the line; otherwise, false. The default is false.</returns>
            public bool FillLine {
                get { return Api.Send(SciMsg.SCI_STYLEGETEOLFILLED, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero; }
                set {
                    var fillLine = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_STYLESETEOLFILLED, new IntPtr(Index), fillLine);
                }
            }

            /// <summary>
            /// Gets or sets the style font name.
            /// </summary>
            /// <returns>The style font name. The default is Verdana.</returns>
            /// <remarks>Scintilla caches fonts by name so font names and casing should be consistent.</remarks>
            public string Font {
                get {
                    var length = Api.Send(SciMsg.SCI_STYLEGETFONT, new IntPtr(Index), IntPtr.Zero).ToInt32();
                    var font = new byte[length];
                    unsafe {
                        fixed (byte* bp = font)
                            Api.Send(SciMsg.SCI_STYLEGETFONT, new IntPtr(Index), new IntPtr(bp));
                    }

                    var name = Encoding.UTF8.GetString(font, 0, length);
                    return name;
                }
                set {
                    if (String.IsNullOrEmpty(value))
                        value = "Verdana";

                    // Scintilla expects UTF-8
                    var font = GetBytes(value, Encoding.UTF8, true);
                    unsafe {
                        fixed (byte* bp = font)
                            Api.Send(SciMsg.SCI_STYLESETFONT, new IntPtr(Index), new IntPtr(bp));
                    }
                }
            }

            /// <summary>
            /// Gets or sets the foreground color of the style.
            /// </summary>
            /// <returns>A Color object representing the style foreground color. The default is Black.</returns>
            /// <remarks>Alpha color values are ignored.</remarks>
            public Color ForeColor {
                get {
                    var color = Api.Send(SciMsg.SCI_STYLEGETFORE, new IntPtr(Index), IntPtr.Zero).ToInt32();
                    return ColorTranslator.FromWin32(color);
                }
                set {
                    if (value.IsEmpty)
                        value = Color.Black;

                    var color = ColorTranslator.ToWin32(value);
                    Api.Send(SciMsg.SCI_STYLESETFORE, new IntPtr(Index), new IntPtr(color));
                }
            }

            /// <summary>
            /// Gets or sets whether hovering the mouse over the style text exhibits hyperlink behavior.
            /// </summary>
            /// <returns>true to use hyperlink behavior; otherwise, false. The default is false.</returns>
            public bool Hotspot {
                get { return Api.Send(SciMsg.SCI_STYLEGETHOTSPOT, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero; }
                set {
                    var hotspot = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_STYLESETHOTSPOT, new IntPtr(Index), hotspot);
                }
            }

            /// <summary>
            /// Gets the zero-based style definition index.
            /// </summary>
            /// <returns>The style definition index within the StyleCollection.</returns>
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets whether the style font is italic.
            /// </summary>
            /// <returns>true if italic; otherwise, false. The default is false.</returns>
            public bool Italic {
                get { return Api.Send(SciMsg.SCI_STYLEGETITALIC, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero; }
                set {
                    var italic = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_STYLESETITALIC, new IntPtr(Index), italic);
                }
            }

            /// <summary>
            /// Gets or sets the size of the style font in points.
            /// </summary>
            /// <returns>The size of the style font as a whole number of points. The default is 8.</returns>
            public int Size {
                get { return Api.Send(SciMsg.SCI_STYLEGETSIZE, new IntPtr(Index), IntPtr.Zero).ToInt32(); }
                set { Api.Send(SciMsg.SCI_STYLESETSIZE, new IntPtr(Index), new IntPtr(value)); }
            }

            /// <summary>
            /// Gets or sets the size of the style font in fractoinal points.
            /// </summary>
            /// <returns>The size of the style font in fractional number of points. The default is 8.</returns>
            public float SizeF {
                get {
                    var fraction = Api.Send(SciMsg.SCI_STYLEGETSIZEFRACTIONAL, new IntPtr(Index), IntPtr.Zero).ToInt32();
                    return (float)fraction / (int)SciMsg.SC_FONT_SIZE_MULTIPLIER;
                }
                set {
                    var fraction = (int)(value * (int)SciMsg.SC_FONT_SIZE_MULTIPLIER);
                    Api.Send(SciMsg.SCI_STYLESETSIZEFRACTIONAL, new IntPtr(Index), new IntPtr(fraction));
                }
            }

            /// <summary>
            /// Gets or sets whether the style is underlined.
            /// </summary>
            /// <returns>true if underlined; otherwise, false. The default is false.</returns>
            public bool Underline {
                get { return Api.Send(SciMsg.SCI_STYLEGETUNDERLINE, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero; }
                set {
                    var underline = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_STYLESETUNDERLINE, new IntPtr(Index), underline);
                }
            }

            /// <summary>
            /// Gets or sets whether the style text is visible.
            /// </summary>
            /// <returns>true to display the style text; otherwise, false. The default is true.</returns>
            public bool Visible {
                get { return Api.Send(SciMsg.SCI_STYLEGETVISIBLE, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero; }
                set {
                    var visible = value ? new IntPtr(1) : IntPtr.Zero;
                    Api.Send(SciMsg.SCI_STYLESETVISIBLE, new IntPtr(Index), visible);
                }
            }

            /// <summary>
            /// Gets or sets the style font weight.
            /// </summary>
            /// <returns>The font weight. The default is 400.</returns>
            /// <remarks>Setting this property affects the Bold property.</remarks>
            public int Weight {
                get { return Api.Send(SciMsg.SCI_STYLEGETWEIGHT, new IntPtr(Index), IntPtr.Zero).ToInt32(); }
                set { Api.Send(SciMsg.SCI_STYLESETWEIGHT, new IntPtr(Index), new IntPtr(value)); }
            }

            #endregion Properties

            #region constructor

            /// <summary>
            /// Style constructor, There are 256 lexer styles that can be set, numbered 0 to STYLE_MAX (255).
            /// There are also some predefined numbered styles starting at 32
            /// </summary>
            /// <param name="index"></param>
            public Style(byte index) {
                Index = index;
            }

            #endregion
        }
    }
}
