namespace _3PA.MainFeatures.Parser.Pro.Parse {
    internal partial class Parser {

        /// <summary>
        /// Creates a label parsed item
        /// </summary>
        private ParsedLabel CreateParsedLabel(Token labelToken) {
            var newLabel = new ParsedLabel(_context.CurrentStatement.FirstToken.Value, _context.CurrentStatement.FirstToken);
            AddParsedItem(newLabel, labelToken.OwnerNumber);
            return newLabel;
        }

    }
}