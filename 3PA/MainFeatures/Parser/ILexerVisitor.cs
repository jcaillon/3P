namespace _3PA.MainFeatures.Parser {
     
    public interface ILexerVisitor {
        void Visit(TokenComment tok);
        void Visit(TokenEol tok);
        void Visit(TokenEos tok);
        void Visit(TokenInclude tok);
        void Visit(TokenNumber tok);
        void Visit(TokenQuotedString tok);
        void Visit(TokenSymbol tok);
        void Visit(TokenWhiteSpace tok);
        void Visit(TokenWord tok);
        void Visit(TokenEof tok);
        void Visit(TokenUnknown tok);
        void Visit(TokenPreProcStatement tok);
    }
}
