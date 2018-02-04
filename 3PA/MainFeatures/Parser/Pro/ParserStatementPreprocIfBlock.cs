using System;
using System.Text;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {
        
        /// <summary>
        /// Matches a &amp;IF expression &amp;THEN pre-processed statement (extractes the evaluated expression in BlockDescription)
        /// </summary>
        private ParsedPreProcBlock CreateParsedIfEndIfPreProc(Token ifToken) {

            var statementFirstTokenRelativePosition = _context.StatementFirstTokenPosition - _tokenPos;
            int i = 1;
            _lastTokenWasSpace = true;
            StringBuilder expression = new StringBuilder();

            while (statementFirstTokenRelativePosition + i < 0) {
                var token = PeekAt(statementFirstTokenRelativePosition + i);
                i++;
                if (token is TokenComment) continue;
                AddTokenToStringBuilder(expression, token);
            }

            var newIf = new ParsedPreProcBlock(String.Empty, ifToken) {
                Type = ParsedPreProcBlockType.IfEndIf,
                BlockDescription = expression.ToString()
            };
            AddParsedItem(newIf, ifToken.OwnerNumber);

            return newIf;
        }

    }
}