using System;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {

        /// <summary>
        /// Creates a publish parsed item
        /// </summary>
        private void CreateParsedPublish(Token tokenPub) {
            /*
            PUBLISH event-name
              [ FROM publisher-handle ]
              [ ( parameter[ , parameter ]... ) ].
            */

            // info we will extract from the current statement :
            string eventName = null;
            string publisherHandler = null;
            StringBuilder left = new StringBuilder();
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        if (token is TokenString || token is TokenWord) {
                            // event name
                            eventName = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                    case 1:
                        if (token is TokenWord) {
                            if (token.Value.EqualsCi("from"))
                                state = 10;
                        } else if (token is TokenSymbol && token.Value.Equals("(")) {
                            AddTokenToStringBuilder(left, token);
                            state++;
                        }
                        break;
                    case 2:
                        AddTokenToStringBuilder(left, token);
                        break;
                    case 10:
                        // match publisher handler
                        if (!(token is TokenWord))
                            break;
                        publisherHandler = token.Value;
                        state = 1;
                        break;
                }
            } while (MoveNext());
            if (!string.IsNullOrEmpty(eventName))
                AddParsedItem(new ParsedEvent(ParsedEventType.Publish, eventName, tokenPub, null, publisherHandler, null, left.ToString()), tokenPub.OwnerNumber);

        }
    }
}