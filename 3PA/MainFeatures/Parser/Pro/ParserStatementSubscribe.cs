using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {

        /// <summary>
        /// Creates a subscribe parsed item
        /// </summary>
        private void CreateParsedSubscribe(Token tokenSub) {
            /*
            SUBSCRIBE [ PROCEDURE subscriber-handle] [ TO ] event-name 
                { IN publisher-handle | ANYWHERE }
                [ RUN-PROCEDURE local-internal-procedure ] [ NO-ERROR ].
             * 
            UNSUBSCRIBE [ PROCEDURE subscriber-handle ] [ TO ] { event-name | ALL } 
                [ IN publisher-handle ].
            */

            // info we will extract from the current statement :
            string subscriberHandle = null;
            string eventName = null;
            string publisherHandler = null;
            string runProcedure = null;
            int state = 0;

            do {
                var token = PeekAt(1); // next token
                if (state == 3) break; // stop when the run procedure has been found
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        if (token is TokenWord) {
                            switch (token.Value.ToLower()) {
                                case "procedure":
                                    state = 20;
                                    break;
                                case "to":
                                    break;
                                default:
                                    // event name
                                    eventName = GetTokenStrippedValue(token);
                                    state++;
                                    break;
                            }
                        } else if (token is TokenString) {
                            // event name
                            eventName = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                    case 1:
                        if (!(token is TokenWord))
                            break;
                        switch (token.Value.ToLower()) {
                            case "in":
                                state = 30;
                                break;
                            case "anywhere":
                                publisherHandler = token.Value;
                                break;
                            case "run-procedure":
                                state++;
                                break;
                        }
                        break;
                    case 2:
                        // matching the local procedure 
                        if (token is TokenString || token is TokenWord) {
                            runProcedure = GetTokenStrippedValue(token);
                            state++;
                        }
                        break;
                    case 20:
                        // matching PROCEDURE xx
                        if (!(token is TokenWord))
                            continue;
                        subscriberHandle = token.Value;
                        state = 0;
                        break;
                    case 30:
                        // matching IN publisher-handle
                        if (!(token is TokenWord))
                            continue;
                        publisherHandler = token.Value;
                        state = 1;
                        break;
                }
            } while (MoveNext());

            if (!string.IsNullOrEmpty(eventName))
                AddParsedItem(new ParsedEvent(tokenSub.Value.EqualsCi("subscribe") ? ParsedEventType.Subscribe : ParsedEventType.Unsubscribe, eventName, tokenSub, subscriberHandle, publisherHandler, runProcedure, null), tokenSub.OwnerNumber);
        }

    }
}