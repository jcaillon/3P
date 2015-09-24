using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    /// <summary>
    /// This class sustains the autocompletion list AND the code explorer list
    /// </summary>
    class AutoCompParserVisitor : IParserVisitor{

        public void Visit(ParsedFunction pars) {
            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Function,
                SubType = "",
                Flag = ParseFlag.IsParsedItem,
                Ranking = 0,
                ParsedItem = pars
            });
        }

        public void Visit(ParsedProcedure pars) {
            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Procedure,
                SubType = "",
                Flag = ParseFlag.IsParsedItem,
                Ranking = 0,
                ParsedItem = pars
            });
        }

        public void Visit(ParsedIncludeFile pars) {
           // To code explorer

            // Parse the include file
        }

        public void Visit(ParsedPreProc pars) {
            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.Preprocessed,
                SubType = "",
                Flag = ParseFlag.IsParsedItem,
                Ranking = 0,
                ParsedItem = pars
            });
        }

        public void Visit(ParsedDefine pars) {
            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.UserVariablePrimitive,
                SubType = "",
                Flag = ParseFlag.IsParsedItem,
                Ranking = 0,
                ParsedItem = pars
            });
        }

        public void Visit(ParsedTable pars) {
            ParserHandler.DynamicItems.Add(new CompletionData() {
                DisplayText = pars.Name,
                Type = CompletionType.TempTable,
                SubType = "",
                Flag = ParseFlag.IsParsedItem,
                Ranking = 0,
                ParsedItem = pars
            });
        }

        public void Visit(ParsedOnEvent pars) {
            // To code explorer
        }
    }
}
