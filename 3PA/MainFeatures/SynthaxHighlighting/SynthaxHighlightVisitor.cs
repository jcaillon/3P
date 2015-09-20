using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.SynthaxHighlighting {
    internal class SynthaxHighlightVisitor : ILexerVisitor {
        /// <summary>
        /// Only colorize from this line!
        /// </summary>
        public int FromLine { get; set; }

        public int ToLine { get; set; }

        public void Visit(TokenComment tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            Npp.StyleText((int)TextStyle.Comment, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenEol tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenEos tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenInclude tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenNumber tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenQuotedString tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.String, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenSymbol tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenEof tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenWord tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenWhiteSpace tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenUnknown tok) {
            if (tok.Line < FromLine || tok.Line > ToLine) return;
            //Npp.StyleText((int)TextStyle.Default, tok.StartPosition, tok.EndPosition);
        }

        public void Visit(TokenPreProcessed tok) {
            
        }
    }
}