using System.Text;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {

         /// <summary>
        /// Matches a procedure definition
        /// </summary>
        private bool CreateParsedProcedure(Token procToken) {
            /*
            PROCEDURE proc-name[ PRIVATE ] :
                [procedure-body]

            PROCEDURE proc-name 
                {   EXTERNAL "dllname" [ CDECL | PASCAL | STDCALL ]
                        [ ORDINAL n ][ PERSISTENT ][ THREAD-SAFE ] | IN SUPER } :
                [ procedure-body ]
            */

            // info we will extract from the current statement :
            string name = "";
            ParseFlag flags = 0;
            string externalDllName = null;
            _lastTokenWasSpace = true;
            StringBuilder leftStr = new StringBuilder();

            Token token;
            int state = 0;
            do {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching name
                        if (token is TokenWord || token is TokenString) {
                            name = token is TokenWord ? token.Value : GetTokenStrippedValue(token);
                            state++;
                        }
                        continue;
                    case 1:
                        // matching external
                        if (!(token is TokenWord)) continue;
                        switch (token.Value.ToLower()) {
                            case "external":
                                flags |= ParseFlag.External;
                                state++;
                                break;
                            case "private":
                                flags |= ParseFlag.Private;
                                break;
                        }
                        break;
                    case 2:
                        // matching the name of the external dll
                        if (!(token is TokenString)) continue;
                        externalDllName = GetTokenStrippedValue(token);
                        state--;
                        break;
                }
                AddTokenToStringBuilder(leftStr, token);
            } while (MoveNext());

            if (state < 1) return false;
            var newProc = new ParsedProcedure(name, procToken, leftStr.ToString(), externalDllName) {
                // = end position of the EOS of the statement
                EndPosition = token.EndPosition,
                Flags = flags
            };
            AddParsedItem(newProc, procToken.OwnerNumber);
            _context.Scope = newProc;
            return true;
        }

    }
}