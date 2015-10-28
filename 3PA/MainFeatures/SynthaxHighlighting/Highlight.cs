#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (Highlight.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Diagnostics;
using System.Drawing;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.SynthaxHighlighting {

    /// <summary>
    /// This class handles the STYLENEEDED notification of scintilla
    /// </summary>
    public class Highlight {
        public static int derp = 0;

        //TODO: the class doesn't correctly handle text that is not encoded on 8 bits because we just do pos++, need to fix this 
        // GetByteCount();
        /*
         * Encoding.UTF8.GetByteCount(text);
         * 
         * byte[] bytes = Encoding.Default.GetBytes(myString);
            myString = Encoding.UTF8.GetString(bytes);
         * 
         * Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(Message);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
         * 
         * File.ReadAllText(file, Encoding.GetEncoding(codePage));
         */

        /// <summary>
        /// Called on STYLENEEDED notification
        /// </summary>
        /// <param name="endPos"></param>
        public static void Colorize(int startPos, int endPos) {
            //------------
            var watch = Stopwatch.StartNew();
            //------------

            // redefine the styles
            SetCustomStyles();

            Lexer tok = new Lexer(Npp.GetDocumentText());
            tok.Tokenize();
            SynthaxHighlightVisitor vis = new SynthaxHighlightVisitor {
                FromLine = Npp.GetLineFromPosition(startPos),
                ToLine = Npp.GetLineFromPosition(endPos)
            };
            tok.Accept(vis);

            //--------------
            watch.Stop();
            Npp.SetStatusbarLabel("derp = " + derp + "startPos = " + startPos + ", endPos = " + endPos + ", done in " + watch.ElapsedMilliseconds + " ms");
            //------------
            derp++;
        }

        public static void SetCustomStyles() {
            Npp.SetDefaultStyle(Color.White, Color.Crimson);
            Npp.SetStyle((int)UdlStyles.Default, Color.AntiqueWhite, Color.MidnightBlue);
            Npp.SetStyle((int)UdlStyles.Comment, Color.GreenYellow, Color.Green);
            Npp.SetStyle((int)UdlStyles.CommentLine, Color.Black, Color.Aquamarine);
            Npp.SetStyle((int)UdlStyles.Delimiter1, Color.White, Color.Crimson);
            Npp.SetStyle((int)UdlStyles.Delimiter2, Color.White, Color.Brown);
            Npp.SetStyle((int)UdlStyles.KeyWordsList1, Color.White, Color.DarkViolet);
        }

        /// <summary>
        /// Is the caret not in : an include, a string, a comment
        /// </summary>
        /// <returns></returns>
        public static bool IsCarretInNormalContext(int curPos) {
            try {
                var curContext = (UdlStyles)Npp.GetStyleAt(curPos);
                if (curPos <= 0) return true;
                if (IsNormalContext(curContext)) return true;
                var prevContext = (UdlStyles) Npp.GetStyleAt(curPos - 1);
                return IsNormalContext(prevContext);
            } catch (Exception) {
                // we can be here if the style ID isn't in the UdlStyles enum
                return true;
            }
        }

        /// <summary>
        /// Is the caret not in : an include, a string, a comment
        /// </summary>
        /// <returns></returns>
        public static bool IsNormalContext(UdlStyles context) {
            return (context != UdlStyles.Comment
                    && context != UdlStyles.Delimiter1
                    && context != UdlStyles.Delimiter2
                    && context != UdlStyles.Delimiter3
                    && context != UdlStyles.CommentLine
                    && context != UdlStyles.Delimiter8);
        }
    }

    /// <summary>
    /// Enumeration of the style id used by the UDL
    /// </summary>
    public enum UdlStyles {
        Default = 0,
        Comment = 1,
        CommentLine = 2,
        Number = 3,
        KeyWordsList1 = 4,
        KeyWordsList2 = 5,
        KeyWordsList3 = 6,
        KeyWordsList4 = 7,
        KeyWordsList5 = 8,
        KeyWordsList6 = 9,
        KeyWordsList7 = 10,
        KeyWordsList8 = 11,
        FolderInCode1 = 13,
        FolderInCode2 = 14,
        FolderInComment = 15,
        Delimiter1 = 16,
        Delimiter2 = 17,
        Delimiter3 = 18,
        Delimiter4 = 19,
        Delimiter5 = 20,
        Delimiter6 = 21,
        Delimiter7 = 22,
        Delimiter8 = 23,
        Operators = 24,
    }
}
