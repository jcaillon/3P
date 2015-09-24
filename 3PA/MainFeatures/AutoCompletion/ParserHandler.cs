using System.Collections.Generic;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {

    class ParserHandler {
        /// <summary>
        /// contains the list of items that depend on the current file, that list
        /// is updated by the parser's visitor class
        /// </summary>
        public static List<CompletionData> DynamicItems;

        private static Parser.Parser _ablParser;

        /// <summary>
        /// is used to make sure that 2 different threads dont try to access
        /// the same resource (_ablParser) at the same time, which would be problematic
        /// </summary>
        private static object _thisLock = new object();

        /// <summary>
        /// Returns the owner name (currentScopeName) of the caret line
        /// </summary>
        /// <returns></returns>
        public static string GetCarretLineLcOwnerName {
            get {
                var line = Npp.GetCaretLineNumber();
                if (_ablParser == null) return "";
                lock (_thisLock) {
                    return !_ablParser.GetLineInfo.ContainsKey(line) ? "" : _ablParser.GetLineInfo[line].CurrentScopeName;
                }
            }
        }

        /// <summary>
        /// this method should be called to refresh the Items list with all the static items
        /// as well as the dynamic items found by the parser
        /// </summary>
        public static void RefreshParser() {
            if (DynamicItems == null) DynamicItems = new List<CompletionData>();

            // we launch the parser, that will fill the DynamicItems
            lock (_thisLock) {
                _ablParser = new Parser.Parser(Npp.GetDocumentText(), Npp.GetCurrentFilePath());
                DynamicItems.Clear();
                _ablParser.Accept(new AutoCompParserVisitor());
            }

        }

    }
}
