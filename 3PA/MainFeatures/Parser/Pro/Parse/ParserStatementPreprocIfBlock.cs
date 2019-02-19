using System;
using System.Collections.Generic;
using System.Text;

namespace _3PA.MainFeatures.Parser.Pro.Parse {
    internal partial class Parser {
                
        /// <summary>
        /// Matches a &amp;IF expression &amp;THEN pre-processed statement (extractes the evaluated expression in BlockDescription)
        /// </summary>
        private ParsedScopePreProcIfBlock CreateParsedIfEndIfPreProc(Token ifToken, bool lastPreProcIfBlockWasTrue) {
            
            List<Token> expressionTokens;
            StringBuilder expression = new StringBuilder();
            bool expressionResult;
            
            // do we need to extract an expression from this IF/ELSEIF? (or is it an ELSE)
            if (ifToken.Value.ToLower().EndsWith("if")) {

                expressionTokens = new List<Token>();

                int i = 0;
                do {
                    i++;
                    // need to replace in case we use for instance a {&var} in a scope-define value
                    ReplaceIncludeAndPreprocVariablesAhead(i + 1);
                    var token = PeekAt(i);
                    if (token is TokenComment)
                        continue;
                    if (token is TokenEof)
                        break;
                    if (token is TokenPreProcDirective)
                        break;
                    expressionTokens.Add(token);
                } while (true);

                // we directly set the new token position there (it will be just 1 token before the &THEN)
                _tokenPos += i - 1;

                AddLineInfo(PeekAt(0));

                // since we didn't use MoveNext we also manually replace the includes ahead
                ReplaceIncludeAndPreprocVariablesAhead(2);

                foreach (var token in expressionTokens) {
                    expression.Append(token.Value);
                }
                
                try { 
                    expressionResult = !lastPreProcIfBlockWasTrue && UoePreprocessedExpressionEvaluator.IsExpressionTrue(expression.ToString(), s => {
                        return _parsedIncludes[ifToken.OwnerNumber].GetScopedPreProcVariableDefinedLevel(s);
                    });
                } catch (Exception e) {
                    ErrorHandler.LogError(e);
                    expressionResult = false;
                }
            } else {
                // it's an &ELSE
                expressionResult = !lastPreProcIfBlockWasTrue;
            }
            
            var newIf = new ParsedScopePreProcIfBlock(ifToken.Value, ifToken) {
                EvaluatedExpression = expression.ToString().Trim(),
                ExpressionResult = expressionResult
            };

            AddParsedItem(newIf, ifToken.OwnerNumber);

            return newIf;
        }
    }
}