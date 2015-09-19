using System.Diagnostics;
using System.Drawing;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.SynthaxHighlighting {

    /// <summary>
    /// This class handles the STYLENEEDED notification of scintilla
    /// </summary>
    class Highlight {
        public static int derp = 0;

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

            //Npp.StyleText((int)TextStyle.Default, startPos, endPos);
            //Npp.StyleText(thisStyle, item.StartPosition, item.EndPosition);

            Lexer tok = new Lexer(Npp.GetDocumentText());
            tok.Tokenize();
            SynthaxHighlightVisitor vis = new SynthaxHighlightVisitor {
                FromLine = Npp.LineFromPosition(startPos),
                ToLine = Npp.LineFromPosition(endPos)
            };
            tok.Accept(vis);

            //--------------
            watch.Stop();
            Npp.SetStatusbarLabel("derp = " + derp + "startPos = " + startPos + ", endPos = " + endPos + ", done in " + watch.ElapsedMilliseconds + " ms");
            //------------
            derp++;
        }

        public static void SetCustomStyles() {
            //Npp.SetDefaultStyle(Color.White, Color.Crimson);
            Npp.SetStyle((int)Npp.UdlStyles.Default, Color.AntiqueWhite, Color.MidnightBlue);
            Npp.SetStyle((int)Npp.UdlStyles.Comment, Color.GreenYellow, Color.Green);
            Npp.SetStyle((int)Npp.UdlStyles.CommentLine, Color.Black, Color.Aquamarine);
            Npp.SetStyle((int)Npp.UdlStyles.Delimiter1, Color.White, Color.Crimson);
            Npp.SetStyle((int)Npp.UdlStyles.Delimiter2, Color.White, Color.Brown);
            Npp.SetStyle((int)Npp.UdlStyles.KeyWordsList1, Color.White, Color.DarkViolet);
        }

    }

    public enum TextStyle {
        Default = 220,
        Comment,
        String,
        StrongStatements,
        Statements,
        PrimitiveTypes,
        Abbreviations
    }
}
