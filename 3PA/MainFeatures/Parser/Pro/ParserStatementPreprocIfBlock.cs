using System;
using System.Collections.Generic;
using System.Text;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {
        
        /// <summary>
        /// Matches a &amp;IF expression &amp;THEN pre-processed statement (extractes the evaluated expression in BlockDescription)
        /// </summary>
        private ParsedScopePreProcIfBlock CreateParsedIfEndIfPreProc(Token ifToken) {
            
            StringBuilder expression = new StringBuilder();

            // do we need to extract an expression from this IF/ELSEIF? (or is it an ELSE)
            if (ifToken.Value.ToLower().EndsWith("if")) {

                int i = 1;
                List<Token> expressiontokens = new List<Token>();

                do {
                    var token = PeekAt(i);
                    if (token is TokenComment)
                        continue;
                    if (token is TokenEof)
                        break;
                    if (token is TokenPreProcDirective)
                        break;
                    expressiontokens.Add(token);
                    i++;
                } while (true);

                foreach (var token in expressiontokens) {
                    expression.Append(token.Value);
                }
            }

            var newIf = new ParsedScopePreProcIfBlock(ifToken.Value, ifToken) {
                BlockDescription = expression.ToString()
            };

            AddParsedItem(newIf, ifToken.OwnerNumber);

            return newIf;
        }

    }
}