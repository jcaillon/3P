using System.Collections.Generic;

namespace _3PA.MainFeatures.AutoCompletion {

    class ParserHandler {
        /// <summary>
        /// This dictionnary is what is used to remember the ranking of each word for the current session
        /// (otherwise this info is lost since we clear the DynamicList each time we parse!)
        /// </summary>
        public static Dictionary<string, int> DisplayTextRankingDynamic = new Dictionary<string, int>();

        /// <summary>
        /// Same as above but for static stuff (= database)
        /// </summary>
        public static Dictionary<string, int> DisplayTextRankingStatic = new Dictionary<string, int>();

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

        /// <summary>
        /// Find ranking of a dynamic item
        /// </summary>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static int FindRankingOfDynamic(string displayText) {
            return DisplayTextRankingDynamic.ContainsKey(displayText) ? DisplayTextRankingDynamic[displayText] : 0;
        }

        /// <summary>
        /// Find ranking of a static item
        /// </summary>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static int FindRankingOfStatic(string displayText) {
            return DisplayTextRankingStatic.ContainsKey(displayText) ? DisplayTextRankingStatic[displayText] : 0;
        }

        /// <summary>
        /// remember the use of a particular item in the completion list
        /// (for dynamic items = parsed items)
        /// </summary>
        /// <param name="displayText"></param>
        public static void RememberUseOfDynamic(string displayText) {
            if (!DisplayTextRankingDynamic.ContainsKey(displayText))
                DisplayTextRankingDynamic.Add(displayText, 1);
            else
                DisplayTextRankingDynamic[displayText]++;
        }

        /// <summary>
        /// remember the use of a particular item in the completion list
        /// (for database items!)
        /// </summary>
        /// <param name="displayText"></param>
        public static void RememberUseOfStatic(string displayText) {
            if (!DisplayTextRankingStatic.ContainsKey(displayText))
                DisplayTextRankingStatic.Add(displayText, 1);
            else
                DisplayTextRankingStatic[displayText]++;
        }

    }
}
