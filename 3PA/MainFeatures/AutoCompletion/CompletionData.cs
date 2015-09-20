using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {
    /// <summary>
    /// class used in the auto completion feature
    /// </summary>
    public class CompletionData {
        public string DisplayText { get; set; }
        public CompletionType Type { get; set; }

        /// <summary>
        /// Used for sorting the autocompletion list
        /// </summary>
        public int Ranking { get; set; }

        /// <summary>
        /// When Type is UserVariable, is used to know if the var is shared, local global..
        /// When Type is Keyword, is used to know if the keyword is reserved
        /// </summary>
        public ParseFlag Flag { get; set; }

        public string SubType { get; set; }
    }

    public enum CompletionType {
        Keyword,
        Table,
        TempTable,
        Field,
        FieldPk,
        Snippet,
        Function,
        Procedure,
        UserVariablePrimitive,
        UserVariableOther,
        Preprocessed
    }
}
