using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletion {
    /// <summary>
    /// class used in the auto completion feature
    /// </summary>
    public class CompletionData {
        /// <summary>
        /// The piece of text displayed in the completion list
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// Type of completion
        /// </summary>
        public CompletionType Type { get; set; }

        /// <summary>
        /// A free to use string, can contain :
        /// - keyword = type of keyword
        /// </summary>
        public string SubType { get; set; }

        /// <summary>
        /// When Type is UserVariable, is used to know if the var is shared, local global..
        /// When Type is Keyword, is used to know if the keyword is reserved
        /// </summary>
        public ParseFlag Flag { get; set; }

        /// <summary>
        /// Used for sorting the autocompletion list
        /// </summary>
        public int Ranking { get; set; }

        public ParsedItem ParsedItem { get; set; }
    }

    public enum CompletionType {
        FieldPk,
        Field,
        Snippet,
        TempTable,
        UserVariablePrimitive,
        UserVariableOther,
        Table,
        Function,
        Procedure,
        Preprocessed,
        Keyword
    }
}
