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
        /// When Type is UserVariable, is used to know if the var is shared, local global..
        /// When Type is Keyword, is used to know if the keyword is reserved
        /// </summary>
        public ParseFlag Flag { get; set; }

        /// <summary>
        /// Used for sorting the autocompletion list
        /// </summary>
        public int Ranking { get; set; }

        /// <summary>
        /// A free to use string, can contain :
        /// - keyword = type of keyword
        /// - table = name of the owner database
        /// - field = type
        /// </summary>
        public string SubString { get; set; }

        public ParsedItem ParsedItem { get; set; }

        /// <summary>
        /// Indicates whether or not this completionData is created by the parser Visitor
        /// </summary>
        public bool FromParser { get; set; }
    }

    public enum CompletionType {
        FieldPk = 0,
        Field = 1,
        Snippet = 2,
        TempTable = 3,
        UserVariablePrimitive = 4,
        UserVariableOther = 5,
        Table = 6,
        Function = 7,
        Procedure = 8,
        Preprocessed = 9,
        Keyword = 10,
        Databases = 11,
        Widget = 12
    }
}
