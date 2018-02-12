using System.Collections.Generic;
using System.Text;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {
        
        /// <summary>
        /// Matches a &amp;IF expression &amp;THEN pre-processed statement (extractes the evaluated expression in BlockDescription)
        /// </summary>
        private ParsedScopePreProcIfBlock CreateParsedIfEndIfPreProc(Token ifToken, bool skipFalseBlock) {
            
            StringBuilder expression = new StringBuilder();
            List<Token> expressionTokens = null;

            // do we need to extract an expression from this IF/ELSEIF? (or is it an ELSE)
            if (ifToken.Value.ToLower().EndsWith("if")) {

                expressionTokens = new List<Token>();

                int i = 0;
                do {
                    i++;
                    // need to replace in case we use for instance a {&var} in a scope-define value
                    ReplaceIncludeAndPreprocVariablesAhead(i);
                    var token = PeekAt(i);
                    if (token is TokenComment)
                        continue;
                    if (token is TokenEof)
                        break;
                    if (token is TokenPreProcDirective)
                        break;
                    if (token is TokenEol) {
                        AddLineInfo(token);
                    }
                    expressionTokens.Add(token);
                } while (true);

                // we directly set the new token position there (it will be just 1 token before the &THEN)
                _tokenPos += i - 1;

                // since we didn't use MoveNext we also manually replace the includes ahead
                ReplaceIncludeAndPreprocVariablesAhead(1);
                ReplaceIncludeAndPreprocVariablesAhead(2);

                foreach (var token in expressionTokens) {
                    expression.Append(token.Value);
                }
            }

            var newIf = new ParsedScopePreProcIfBlock(ifToken.Value, ifToken) {
                EvaluatedExpression = expression.ToString().Trim(),
                ExpressionResult = ExpressionEvaluator(expressionTokens)
            };

            AddParsedItem(newIf, ifToken.OwnerNumber);

            if (!newIf.ExpressionResult && skipFalseBlock) {
                // do not analyse this whold block (until the next else/elseif/endif)
            }

            return newIf;
        }

        private bool ExpressionEvaluator(List<Token> expressionTokens) {
            if (expressionTokens != null) {
                if (expressionTokens.Count > 2) {
                    if (expressionTokens[1].Value.ToLower().Equals("true"))
                        return true;
                }
                
            }
            return false;
        }
    }
}