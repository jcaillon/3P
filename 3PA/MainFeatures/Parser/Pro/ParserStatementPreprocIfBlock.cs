using System;
using System.Text;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {
        
        /// <summary>
        /// Matches a &amp;IF expression &amp;THEN pre-processed statement (extractes the evaluated expression in BlockDescription)
        /// </summary>
        private ParsedScopePreProcIfBlock CreateParsedIfEndIfPreProc(Token ifToken) {
            
            int i = 1;
            _lastTokenWasSpace = true;
            StringBuilder expression = new StringBuilder();

            while (i < 0) {
                var token = PeekAt(i);
                i++;
                if (token is TokenComment) continue;
                AddTokenToStringBuilder(expression, token);
            }

            var newIf = new ParsedScopePreProcIfBlock(ifToken.Value, ifToken) {
                BlockDescription = expression.ToString()
            };
            AddParsedItem(newIf, ifToken.OwnerNumber);

            return newIf;
        }

    }
}