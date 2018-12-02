using System.Text;
using System.Collections.Generic;

namespace _3PA.MainFeatures.Parser.Pro.Parse
{
    internal partial class Parser
    {

        /// <summary>
        /// Matches a procedure definition
        /// </summary>
        private ParsedClass CreateParsedClass(Token classToken)
        {
            /*            
            CLASS class-type-name [ INHERITS super-type-name ]
                [ IMPLEMENTS interface-type-name [ , interface-type-name ] ... ]
                [ USE-WIDGET-POOL ]
                [ ABSTRACT | FINAL ] : 
                [ class-body ]
            */

            // info we will extract from the current statement :
            string name = "";
            string Inherits = "";            
            ParseFlag flags = 0;            
            List<string> Implements = new List<string>();

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
                    } else
                    {
                        switch (token.Value.ToLower())
                        {
                            case "inherits":
                                state = 2;
                                break;
                            case "implements":
                                state = 3;
                                break;
                            case "abstract":
                                flags |= ParseFlag.Abstract;
                                break;
                            case "final":
                                flags |= ParseFlag.Final;
                                break;
                            default:
                                if (state == 2)
                                {
                                    Inherits = token.Value;
                                    state--;
                                }
                                else if (state == 3)
                                    Implements.Add(token.Value);
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

            var newClass = new ParsedClass(name, classToken, Inherits, Implements)
            {
                // = end position of the EOS of the statement
                Flags = flags,
                EndPosition = token.EndPosition                
            };
            AddParsedItem(newClass, classToken.OwnerNumber);

            return newClass;
        }

    }
}