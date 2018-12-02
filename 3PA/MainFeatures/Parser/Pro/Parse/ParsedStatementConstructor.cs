using System.Text;
using System.Collections.Generic;

namespace _3PA.MainFeatures.Parser.Pro.Parse
{
    internal partial class Parser
    {

        /// <summary>
        /// Matches a procedure definition
        /// </summary>
        private ParsedConstructor CreateParsedConstructor(Token constructorToken, ParsedScope parentScope)
        {
            /*                        
            CONSTRUCTOR [ PRIVATE | PROTECTED | PUBLIC | STATIC ] class-name 
            ( [ parameter [ , parameter ] ... ] ) : 
            constructor-body            
            */

            // info we will extract from the current statement :
            string name = "";            
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
                        parametersList = GetParsedParameters(constructorToken, parameters);
                        state = 2;
                        break;
                }
            } while (MoveNext());

            if (state < 1)
                return null;

            var newConstructor = new ParsedConstructor(name, constructorToken, parentScope)
            {
                // = end position of the EOS of the statement
                Flags = flags,
                EndPosition = token.EndPosition
            };
            if (parametersList != null)
            {
                newConstructor.Parameters = new List<ParsedDefine>();
                foreach (var parsedItem in parametersList)
                {
                    newConstructor.Parameters.Add(parsedItem);
                }
            }
            AddParsedItem(newConstructor, constructorToken.OwnerNumber);

            return newConstructor;
        }

    }
}