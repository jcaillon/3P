namespace _3PA.MainFeatures.Parser {
     
    public interface IParserVisitor {
        void Visit(ParsedFunction pars);
        void Visit(ParsedProcedure pars);
        void Visit(ParsedIncludeFile pars);
        void Visit(ParsedPreProc pars);
        void Visit(ParsedDefine pars);
        void Visit(ParsedTable pars);
        void Visit(ParsedOnEvent pars);
        void Visit(ParsedRun pars);
        void Visit(ParsedBlock pars);
    }
}
