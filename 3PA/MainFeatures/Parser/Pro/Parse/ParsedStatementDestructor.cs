using System.Text;
using System.Collections.Generic;

namespace _3PA.MainFeatures.Parser.Pro.Parse
{
    internal partial class Parser
    {

        /// <summary>
        /// Matches a procedure definition
        /// </summary>
        private ParsedDestructor CreateParsedDestructor(Token destructorToken, ParsedScope parentScope)
        {
            /*                        
            DESTRUCTOR [ PUBLIC ] class-name ( ) :            
            Destructor-body            
            */

            // info we will extract from the current statement :
            string name = "";
            ParseFlag flags = 0;            

            Token token;            
            do
            {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                if (token is TokenWord)
                {
                    switch (token.Value.ToLower())
                    {
                        case "public":
                            flags |= ParseFlag.Public;
                            break;
                        default:
                            name = token.Value;
                            break;
                    }
                }               
            } while (MoveNext());

            if (name=="")
                return null;

            var newDestructor = new ParsedDestructor(name, destructorToken, parentScope)
            {
                // = end position of the EOS of the statement
                Flags = flags,
                EndPosition = token.EndPosition
            };
            
            AddParsedItem(newDestructor, destructorToken.OwnerNumber);

            return newDestructor;
        }

    }
}