using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using _3PA.Interop;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.Colorisation {

    /// <summary>
    /// This class handles the STYLENEEDED notification of scintilla
    /// </summary>
    class Colorisation {
        public static int derp = 0;

        /// <summary>
        /// Called on STYLENEEDED notification
        /// </summary>
        /// <param name="endPos"></param>
        public static void Colorize(int endPos) {
            int startPos = Npp.GetSylingNeededStartPos();

            // redefine the styles
            SetCustomStyles();

            //------------
            var watch = Stopwatch.StartNew();
            //------------

            List<Token> TokenList = new List<Token>();
            Lexer tok = new Lexer(Npp.GetAllText());
            Token token;
            do {
                token = tok.GetNext();
                TokenList.Add(token);
            } while (token.Type != TokenType.Eof);

            //Npp.StyleText((int)SciMsg.STYLE_DEFAULT, startPos, endPos);
            foreach (var item in TokenList) {
                int thisStyle;
                switch (item.Type) {
                    case TokenType.Comment:
                        thisStyle = (int)TextStyle.Comment;
                        break;
                    case TokenType.Word:
                        thisStyle = (int)TextStyle.StrongStatements;
                        break;
                    default:
                        continue;
                        thisStyle = (int)SciMsg.STYLE_DEFAULT;
                        break;
                }
                Npp.StyleText(thisStyle, item.StartPosition, item.EndPosition);
            }

            //--------------
            watch.Stop();
            Npp.SetStatusbarLabel("derp = " + derp + "startPos = " + startPos + ", endPos = " + endPos + ", done in " + watch.ElapsedMilliseconds + " ms");
            //------------
            derp++;
        }

        public static void SetCustomStyles() {
            Npp.SetDefaultStyle(Color.White, Color.Crimson);
            Npp.SetStyle((int)TextStyle.Comment, Color.White, Color.Green);
            Npp.SetStyle((int)TextStyle.StrongStatements, Color.White, Color.Blue);
        }

    }

    public enum TextStyle {
        Comment,
        StrongStatements,
        Statements,
        PrimitiveTypes,
        Abbreviations
    }
}
