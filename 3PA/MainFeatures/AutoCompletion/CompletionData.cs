namespace _3PA.MainFeatures.AutoCompletion {

    public class CompletionData {
        public string DisplayText { get; set; }
        public CompletionType Type { get; set; }

        /// <summary>
        /// Used for sorting the autocompletion list
        /// </summary>
        public int Ranking { get; set; }

        /// <summary>
        /// Used only when Type is = UserVariable
        /// </summary>
        public CompletionFlag Flag { get; set; }

        public string VariableType { get; set; }
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

    public enum CompletionFlag {
        None,
        Scope,
        Global,
        Parameter,
        Reserved,
        Abbreviation
    }
}
