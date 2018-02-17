namespace _3PA.MainFeatures.Parser.Pro.Parse {
    internal partial class Parser {

        /// <summary>
        /// Creates a dynamic function parsed item
        /// </summary>
        private void CreateParsedDynamicFunction(Token tokenFun) {
            /*
            DYNAMIC-FUNCTION
              ( function-name[ IN proc-handle]
                [ , param1[ , param2]...]
              )
            */

            // info we will extract from the current statement :
            string name = "";
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (state == 2) break; // stop after finding the name
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        if (token is TokenSymbol && token.Value.Equals("("))
                            state++;
                        break;
                    case 1:
                        // matching proc name (or VALUE)
                        if (token is TokenString) {
                            name = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                }
            } while (MoveNext());

            if (state == 0) return;

            AddParsedItem(new ParsedFunctionCall(name, tokenFun, !_functionPrototype.ContainsKey(name), false), tokenFun.OwnerNumber);
        }

    }
}