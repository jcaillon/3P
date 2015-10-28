#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (Token.cs) is part of 3P.
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
namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// Token object
    /// </summary>
    public abstract class Token {
        public string Value { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int StartPosition { get; private set; }
        public int EndPosition { get; private set; }
        protected Token(string value, int line, int column, int startPosition, int endPosition) {
            Value = value;
            Line = line;
            Column = column;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
        public abstract void Accept(ILexerVisitor visitor);
    }

    // Token types...
    // Eos is end of statement (a .)
    // Eof is end of file
    // Eol is end of line (\r\n or \n)
    // QuotedString is either a simple or double quote string (handles ~ escape char)
    // Symbol is a single char

    public class TokenComment : Token {
        public TokenComment(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenPreProcStatement : Token {
        public TokenPreProcStatement(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenInclude : Token {
        public TokenInclude(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenEos : Token {
        public TokenEos(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenUnknown : Token {
        public TokenUnknown(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenWord : Token {
        public TokenWord(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenNumber : Token {
        public TokenNumber(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenQuotedString : Token {
        public TokenQuotedString(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenWhiteSpace : Token {
        public TokenWhiteSpace(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenSymbol : Token {
        public TokenSymbol(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenEol : Token {
        public TokenEol(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class TokenEof : Token {
        public TokenEof(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) { }
        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
