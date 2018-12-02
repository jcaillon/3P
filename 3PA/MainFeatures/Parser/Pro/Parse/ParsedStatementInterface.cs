using System.Text;
using System.Collections.Generic;

namespace _3PA.MainFeatures.Parser.Pro.Parse
{
    internal partial class Parser
    {

        /// <summary>
        /// Matches a procedure definition
        /// </summary>
        private ParsedInterface CreateParsedInterface(Token interfaceToken)
        {
            /*            
            INTERFACE interface-type-name 
            [ INHERITS super-interface-name [ , super-interface-name ] ... ] :
            interface-body            
            */

            // info we will extract from the current statement :
            string name = "";
            string Inherits = "";

            Token token;
            int state = 0;
            do
            {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                if (token is TokenWord)
                {
                    if (state == 0)
                    {
                        name = token.Value;
                        state++;
                    }
                    else
                    {
                        switch (token.Value.ToLower())
                        {
                            case "inherits":
                                state = 2;
                                break;
                            default:
                                if (state == 2)
                                {
                                    Inherits = token.Value;
                                    state--;
                                }                                
                                break;
                        }
                    }
                    continue;
                }
                if (token is TokenString)
                {
                    if (state == 0)
                    {
                        name = GetTokenStrippedValue(token);
                        state++;
                    }
                    continue;
                }
            } while (MoveNext());

            if (state < 1)
                return null;

            var newInterface = new ParsedInterface(name, interfaceToken, Inherits)
            {
                // = end position of the EOS of the statement               
                EndPosition = token.EndPosition
            };
            AddParsedItem(newInterface, interfaceToken.OwnerNumber);

            return newInterface;
        }

    }
}