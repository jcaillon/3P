using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletionFeature {


    /// <summary>
    /// This class is not a singleton (the constructor is public) but we use a static instance
    /// performances and handiness
    /// </summary>
    internal class CompletionFilterClass {

        #region private

        private int _currentLineNumber = -2;
        private ParsedScopeItem _currrentScope;

        #endregion

        #region static instance (not a singleton)

        private static CompletionFilterClass _instance;

        public static CompletionFilterClass Instance {
            get { return _instance ?? (_instance = new CompletionFilterClass()); }
            set { _instance = value; }
        }

        #endregion

        #region public

        /// <summary>
        /// Returns true if the conditions have changed
        /// </summary>
        public bool UpdateConditions(int currentLineNumber, bool dontCheckLine) {

            if (currentLineNumber != _currentLineNumber) {
                _currentLineNumber = currentLineNumber;
                _currrentScope = ParserHandler.GetScopeOfLine(currentLineNumber);
                if (dontCheckLine || !Config.Instance.AutoCompleteOnlyShowDefinedVar)
                    _currentLineNumber = -1;
                return true;
            }

            return false;
        }


        /// <summary>
        /// if true, the item isn't filtered
        /// </summary>
        public bool FilterPredicate(object o) {
            var compData = (CompletionItem) o;
            if (compData == null)
                return false;

            // if the item isn't a parsed item, it is available no matter where we are in the code
            if (!compData.FromParser)
                return true;

            var output = true;

            // case of Parsed define or temp table define
            if (compData.ParsedItem is ParsedDefine || compData.ParsedItem is ParsedTable || compData.ParsedItem is ParsedLabel) {
                // check for scope
                if (_currrentScope != null && !(compData.ParsedItem.Scope is ParsedFile)) {
                    output = output && compData.ParsedItem.Scope.ScopeType == _currrentScope.ScopeType;
                    output = output && compData.ParsedItem.Scope.Name.Equals(_currrentScope.Name);
                }

                if (_currentLineNumber >= 0) {
                    // check for the definition line
                    output = output && _currentLineNumber >= (compData.ParsedItem.IncludeLine >= 0 ? compData.ParsedItem.IncludeLine : compData.ParsedItem.Line);

                    // for labels, only dislay them in the block which they label
                    if (compData.ParsedItem is ParsedLabel)
                        output = output && _currentLineNumber <= ((ParsedLabel) compData.ParsedItem).UndefinedLine;
                }

            } else if (compData.ParsedItem is ParsedPreProcVariable) {
                if (_currentLineNumber >= 0) {
                    // if preproc, check line of definition and undefine
                    var parsedItem = (ParsedPreProcVariable) compData.ParsedItem;

                    output = output && _currentLineNumber >= (parsedItem.IncludeLine >= 0 ? parsedItem.IncludeLine : parsedItem.Line);
                    if (parsedItem.UndefinedLine > 0)
                        output = output && _currentLineNumber <= parsedItem.UndefinedLine;
                }
            }

            return output;
        }

        #endregion
        
    }

}
