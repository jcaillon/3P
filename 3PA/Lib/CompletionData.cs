namespace _3PA.Lib {

    public class CompletionData {
        public string DisplayText { get; set; }
        public CompletionType Type { get; set; }
    }

    public enum CompletionType {
        Keyword = 1,
        Table = 2,
        Field = 3,
        Snippet = 4,
        Buffer = 5,
        Temptable = 6,
        Function = 7,
        Procedure = 8,
        Special = 9,
        Empty = 10,
        Abbreviation = 11
    }
}
