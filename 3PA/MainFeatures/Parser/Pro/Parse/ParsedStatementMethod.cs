using System.Text;
using System.Collections.Generic;

namespace _3PA.MainFeatures.Parser.Pro.Parse
{
    internal partial class Parser
    {

        /// <summary>
        /// Matches a procedure definition
        /// </summary>
        private ParsedMethod CreateParsedMethod(Token methodToken, ParsedScope parentScope)
        {
            /*                        
            METHOD [ PRIVATE | PROTECTED | PUBLIC ] [ STATIC | ABSTRACT ] 
                [ OVERRIDE ] [ FINAL ] 
                { VOID | return-type } method-name 
                ( [ parameter [ , parameter ] ... ] ) :
            */

            // info we will extract from the current statement :
            string name = "";
            string parsedReturnType = "CLASS";            
            ParseFlag flags = 0;
            StringBuilder parameters = new StringBuilder();
            List<ParsedDefine> parametersList = null;

            Token token;
            int state = 0;
            do
            {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state)
                {
                    case 0:
                        // default state
                        if (token is TokenWord)
                        {
                            switch (token.Value.ToLower())
                            {
                                case "private":
                                    flags |= ParseFlag.Private;
                                    break;
                                case "public":
                                    flags |= ParseFlag.Public;
                                    break;
                                case "protected":
                                    flags |= ParseFlag.Protected;
                                    break;
                                case "static":
                                    flags |= ParseFlag.Static;
                                    break;
                                case "abstract":
                                    flags |= ParseFlag.Abstract;
                                    break;
                                case "override":
                                    flags |= ParseFlag.Override;
                                    break;
                                case "final":
                                    flags |= ParseFlag.Final;
                                    break;
                                case "void":                                   
                                case "class":                                    
                                case "character":                                    
                                case "integer":
                                case "int64":
                                case "decimal":
                                case "date":
                                case "datetime":
                                case "datetime-tz":
                                case "handle":
                                case "logical":
                                case "longchar":
                                case "memptr":
                                case "recid":
                                case "rowid":
                                case "raw":
                                    parsedReturnType = token.Value.ToUpper();
                                    break;
                                default:
                                    name = token.Value;                                    
                                    break;
                            }
                        }
                        if (token is TokenSymbol && token.Value.Equals("("))
                            state = 1;
                        break;                  
                    case 1:
                        // read parameters, define a ParsedDefineItem for each
                        parametersList = GetParsedParameters(methodToken, parameters);
                        state = 2;
                        break;                  
                }
            } while (MoveNext());

            if (state < 1)
                return null;

            var newMethod = new ParsedMethod(name, methodToken, parentScope, parsedReturnType)
            {
                // = end position of the EOS of the statement
                Flags = flags,
                EndPosition = token.EndPosition
            };
            if (parametersList != null)
            {
                newMethod.Parameters = new List<ParsedDefine>();
                foreach (var parsedItem in parametersList)
                {
                    newMethod.Parameters.Add(parsedItem);
                }
            }
            AddParsedItem(newMethod, methodToken.OwnerNumber);

            return newMethod;
        }

    }
}