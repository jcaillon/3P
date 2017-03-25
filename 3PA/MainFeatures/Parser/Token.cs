#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Token.cs) is part of 3P.
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
namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// Token object
    /// </summary>
    internal abstract class Token {
        public string Value { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int StartPosition { get; private set; }
        public int EndPosition { get; private set; }

        /// <summary>
        /// Used to differentiate the source of the tokens in a procedure containing include files
        /// 0 will correspond to the procedure itself, 1 to the first include and so on...
        /// </summary>
        public ushort OwnerNumber { get; set; }

        protected Token(string value, int line, int column, int startPosition, int endPosition) {
            Value = value;
            Line = line;
            Column = column;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        public abstract void Accept(ILexerVisitor visitor);
    }

    /// <summary>
    /// Can either be a single (//) or multiline comment (/* */)
    /// </summary>
    internal class TokenComment : Token {
        public bool IsSingleLine { get; private set; }

        public TokenComment(string value, int line, int column, int startPosition, int endPosition, bool isSingleLine) : base(value, line, column, startPosition, endPosition) {
            IsSingleLine = isSingleLine;
        }

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Ex: & if, & analyse, & define...
    /// </summary>
    internal class TokenPreProcDirective : Token {
        public TokenPreProcDirective(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Its value is '{', indicates the beggining of an include
    /// </summary>
    internal class TokenInclude : Token {
        public TokenInclude(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Use of a pre-processed variable i.e. {& name}, its value is '{&'
    /// </summary>
    internal class TokenPreProcVariable : Token {
        public TokenPreProcVariable(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Eos is end of statement (a .)
    /// </summary>
    internal class TokenEos : Token {
        public TokenEos(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    internal class TokenUnknown : Token {
        public TokenUnknown(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    internal class TokenWord : Token {
        public TokenWord(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    internal class TokenNumber : Token {
        public TokenNumber(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Either a simple or double quote string (handles ~ escape char)
    /// </summary>
    internal class TokenString : Token {
        public TokenString(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// A character-string in progress can be described with different properties :
    /// "characters" [ : [ R | L | C | T ] [ U ] [ max-length ] ]
    /// This matches the properties of the string
    /// </summary>
    internal class TokenStringDescriptor : Token {
        public TokenStringDescriptor(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    internal class TokenWhiteSpace : Token {
        public TokenWhiteSpace(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    internal class TokenSymbol : Token {
        public TokenSymbol(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Eol is end of line (\r\n or \n)
    /// </summary>
    internal class TokenEol : Token {
        public TokenEol(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Eof is end of file
    /// </summary>
    internal class TokenEof : Token {
        public TokenEof(string value, int line, int column, int startPosition, int endPosition) : base(value, line, column, startPosition, endPosition) {}

        public override void Accept(ILexerVisitor visitor) {
            visitor.Visit(this);
        }
    }
}