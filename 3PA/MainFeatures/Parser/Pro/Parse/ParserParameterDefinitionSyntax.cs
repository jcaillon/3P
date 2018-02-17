using System.Collections.Generic;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro.Parse {
    internal partial class Parser {

        /// <summary>
        /// Parses a parameter definition (used in function, class method, class event)
        /// Returns a list of define parsed items representing the parameters of the function
        /// </summary>
        private List<ParsedDefine> GetParsedParameters(Token functionToken, StringBuilder parameters) {

            /*
            ( parameter [ , parameter ] ... ) 

            { INPUT | OUTPUT | INPUT-OUTPUT }
            {   parameter-name AS {primitive-type-name|[ CLASS ]object-type-name}
              | { LIKE field }
              [ EXTENT [ constant ] ] 
              | TABLE temp-table-name [ APPEND ] [ BIND ] [ BY-VALUE ]
              | TABLE-HANDLE temp-table-handle [ APPEND ] [ BIND ] [ BY-VALUE ]
              | DATASET dataset-name [ APPEND ] [ BIND ] [ BY-VALUE ]
              | DATASET-HANDLE dataset-handle [ APPEND ] [ BIND ] [ BY-VALUE ]
            } 

            BUFFER buffer-name FOR table-name[ PRESELECT ] 
             */


            // info the parameters
            string paramName = "";
            ParseFlag flags = 0;
            ParsedAsLike paramAsLike = ParsedAsLike.None;
            string paramPrimitiveType = "";
            string parameterFor = "";
            var parametersList = new List<ParsedDefine>();

            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenSymbol && token.Value.Equals(")")) state = 99;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching parameters type
                        if (!(token is TokenWord)) break;
                        var lwToken = token.Value.ToLower();
                        switch (lwToken) {
                            case "buffer":
                                paramPrimitiveType = lwToken;
                                state = 10;
                                break;
                            case "table":
                            case "table-handle":
                            case "dataset":
                            case "dataset-handle":
                                paramPrimitiveType = lwToken;
                                state = 20;
                                break;
                            case "input":
                                flags |= ParseFlag.Input;
                                break;
                            case "return":
                                flags |= ParseFlag.Return;
                                break;
                            case "output":
                                flags |= ParseFlag.Output;
                                break;
                            case "input-output":
                                flags |= ParseFlag.InputOutput;
                                break;
                            default:
                                paramName = token.Value;
                                state = 2;
                                break;
                        }

                        break;
                    case 2:
                        // matching parameters as or like
                        if (!(token is TokenWord)) break;
                        var lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("as")) paramAsLike = ParsedAsLike.As;
                        else if (lowerToken.Equals("like"))
                            paramAsLike = ParsedAsLike.Like;
                        if (paramAsLike != ParsedAsLike.None) state++;
                        break;
                    case 3:
                        // matching parameters primitive type or a field in db
                        if (!(token is TokenWord)) break;
                        paramPrimitiveType = token.Value;
                        state = 99;
                        break;

                    case 10:
                        // match a buffer name
                        if (!(token is TokenWord)) break;
                        paramName = token.Value;
                        state++;
                        break;
                    case 11:
                        // match the table/dataset name that the buffer is FOR
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("for")) break;
                        parameterFor = lowerToken;
                        state = 99;
                        break;

                    case 20:
                        // match a table/dataset name
                        if (!(token is TokenWord)) break;
                        paramName = token.Value;
                        state = 99;
                        break;

                    case 99:
                        // matching parameters "," that indicates a next param
                        if (token is TokenWord && token.Value.EqualsCi("extent"))
                            flags |= ParseFlag.Extent;

                        else if (token is TokenSymbol && (token.Value.Equals(")") || token.Value.Equals(","))) {

                            // create a variable for this function scope
                            if (!string.IsNullOrEmpty(paramName)) {
                                parametersList.Add(NewParsedDefined(paramName, flags, functionToken, token, paramAsLike, "", ParseDefineType.Parameter, paramPrimitiveType, "", parameterFor));
                            }

                            paramName = "";
                            paramAsLike = ParsedAsLike.None;
                            paramPrimitiveType = "";
                            parameterFor = "";
                            flags = 0;

                            if (token.Value.Equals(","))
                                state = 0;
                            else
                                return parametersList;
                        }

                        break;
                }

                AddTokenToStringBuilder(parameters, token);
            } while (MoveNext());

            return parametersList;
        }

    }
}