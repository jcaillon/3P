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
        /// Allows to display small "tag" picture on the left of a completionData in the autocomp list,
        /// see the ParseFlag enumeration for all the possibilities
        /// It works as a Flag, call HasFlag() method to if a certain flag is set and use
        /// Flag = Flag | ParseFlag.Reserved to set a flag!
        /// </summary>
        public ParseFlag Flag { get; set; }

        /// <summary>
        /// Used for sorting the autocompletion list, the higher the ranking, the higher in the list
        /// the item is
        /// </summary>
        public int Ranking { get; set; }

        /// <summary>
        /// A free to use string, can contain :
        /// - keyword = type of keyword
        /// - table = name of the owner database
        /// - field = type
        /// </summary>
        public string SubString { get; set; }

        /// <summary>
        /// Indicates whether or not this completionData is created by the parser Visitor
        /// </summary>
        public bool FromParser { get; set; }

        /// <summary>
        /// When the FromParser is true, contains the ParsedItem extracted by the parser
        /// </summary>
        public ParsedItem ParsedItem { get; set; }

        /// <summary>
        /// This field is only used when Type == CompletionType.Keyword, it contains the keyword type...
        /// </summary>
        public KeywordType KeywordType { get; set; }
    }

    public enum CompletionType {
        FieldPk = 0,
        Field = 1,
        Snippet = 2,
        TempTable = 3,
        VariablePrimitive = 4,
        VariableComplex = 5,
        Table = 6,
        Function = 7,
        Procedure = 8,
        Preprocessed = 9,
        Keyword = 10,
        Databases = 11,
        Widget = 12,
        KeywordObject = 13
    }
}
