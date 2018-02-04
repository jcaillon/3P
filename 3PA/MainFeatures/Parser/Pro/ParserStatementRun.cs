using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {

    internal partial class Parser {

        /// <summary>
        /// Creates a parsed item for RUN statements
        /// </summary>
        /// <param name="runToken"></param>
        private void CreateParsedRun(Token runToken) {
            /*
            RUN
              {   extern-proc-name
                 | VALUE ( extern-expression )
                 | path-name<<member-name>>
              }
              [ PERSISTENT | SINGLE-RUN | SINGLETON [ SET proc-handle]]
              [ ON [ SERVER ] {server-handle | session-handle }
                     [ TRANSACTION DISTINCT ]
                     [ ASYNCHRONOUS 
                        [ SET async-request-handle]
                        [ EVENT-PROCEDURE event-internal-procedure
                            [ IN procedure-context]]
                     ]
              ]
              [ ( parameter[ , parameter]... ) ]
              [ argument ]...
              [ NO-ERROR ]

            RUN
              { intern-proc-name | VALUE ( intern-expression) }
              [ IN proc-handle]
              [ ASYNCHRONOUS
                   [ SET async-request-handle]
                   [ EVENT-PROCEDURE event-internal-procedure
                       [ IN procedure-context]]
              ]
              [ ( parameter[ , parameter]... ) ]
              [ NO-ERROR ] 

            RUN portTypeName[ SET hPortType ] ON SERVER hWebService[ NO-ERROR ] .

            RUN operationName IN hPortType 
              [ ASYNCHRONOUS 
                [ SET async-request-handle] 
                [ EVENT-PROCEDURE event-internal-procedure 
                   [ IN procedure-context]]
              [ ( parameter[ , parameter]... ) ]
              [ NO-ERROR ]. 


            RUN SUPER [ ( parameter[ , parameter]... ) ][ NO-ERROR ] 

            TODO :
            RUN STORED-PROCEDURE procedure
              [integer-field = PROC-HANDLE ]
              [ NO-ERROR ]
              [ ( parameter[ , parameter]... ) ] 
              
            */

            // info we will extract from the current statement :
            string name = "";
            ParseFlag flag = 0;
            _lastTokenWasSpace = true;
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (state == 2) break; // stop after finding the RUN name to be able to match other words in the statement
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching proc name (or VALUE)
                        if (token is TokenSymbol && token.Value.Equals(")")) {
                            state++;
                        } else if (flag.HasFlag(ParseFlag.Uncertain) && !(token is TokenWhiteSpace || token is TokenSymbol)) {
                            name += GetTokenStrippedValue(token);
                        } else if (token is TokenWord) {
                            if (token.Value.ToLower().Equals("value"))
                                flag |= ParseFlag.Uncertain;
                            else {
                                name += token.Value;
                                state++;
                            }
                        } else if (token is TokenString) {
                            name = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                    case 1:
                        // matching PERSISTENT (or a path instead of a file)
                        if (token is TokenSymbol && (token.Value.Equals("/") || token.Value.Equals("\\"))) {
                            // if it's a path, append it to the name of the run
                            name += token.Value;
                            state = 0;
                            break;
                        }
                        if (!(token is TokenWord))
                            break;
                        if (token.Value.EqualsCi("persistent"))
                            flag |= ParseFlag.Persistent;
                        state++;
                        break;
                }
            } while (MoveNext());

            if (state == 0) return;
            AddParsedItem(new ParsedRun(name, runToken, null) {Flags = flag}, runToken.OwnerNumber);
        }

    }
}