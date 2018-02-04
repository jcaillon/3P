namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {

        /// <summary>
        /// Creates a label parsed item
        /// </summary>
        private void CreateParsedLabel(Token labelToken) {
            AddParsedItem(new ParsedLabel(_context.StatementFirstToken.Value, _context.StatementFirstToken), labelToken.OwnerNumber);
        }

    }
}