using System.Diagnostics;
using System.IO;
using System.Text;
using _3PA.MainFeatures.Parser;

namespace YamuiDemoApp {
    class Tests {

        public static void Run() {

            //------------
            var watch = Stopwatch.StartNew();
            //------------

            Parser tok = new Parser(File.ReadAllText(@"C:\Users\Julien\Desktop\in.p"));
            //Lexer tok = new Lexer(File.ReadAllText(@"E:\temp\sac-dev\sac\sac\src\proc_uib\sc42lsdd.w"));

            OutputVis vis = new OutputVis();
            tok.Accept(vis);

            //--------------
            watch.Stop();
            //------------

            StringBuilder x = new StringBuilder();
            foreach (var item in tok.LineIndent) {
                x.AppendLine(item.Key.ToString() + " > " + item.Value.ToString());
            }
            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", x.AppendLine("DONE in " + watch.ElapsedMilliseconds + " ms").ToString());

            //File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", vis.output.AppendLine("DONE in " + watch.ElapsedMilliseconds + " ms").ToString());


            return;


            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", vis.output.AppendLine("DONE in " + watch.ElapsedMilliseconds + " ms").ToString());


            //------------
            var watch2 = Stopwatch.StartNew();
            //------------

            Lexer tok2 = new Lexer(File.ReadAllText(@"C:\Users\Julien\Desktop\in.p"));
            tok2.Tokenize();
            OutputLexer vis2 = new OutputLexer();
            tok2.Accept(vis2);

            //--------------
            watch2.Stop();

            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", vis2.output.AppendLine("DONE in " + watch2.ElapsedMilliseconds + " ms").ToString());
            
        }
    }

    public class OutputVis : IParserVisitor {
        public StringBuilder output = new StringBuilder();
        public void Visit(ParsedFunction pars) {
            output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.ReturnType + "," + pars.Parameters + "," + (pars.Flag.HasFlag(ParseFlag.Private)));
        }

        public void Visit(ParsedProcedure pars) {
            output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.Left);
        }

        public void Visit(ParsedIncludeFile pars) {
            output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name);
        }

        public void Visit(ParsedPreProc pars) {

        }

        public void Visit(ParsedDefine pars) {

        }

        public void Visit(ParsedTable pars) {

        }
    }

    public class OutputLexer : ILexerVisitor {

        public StringBuilder output = new StringBuilder();

        public void Visit(TokenComment tok) {
            //output.AppendLine(tok.Value);
        }

        public void Visit(TokenEol tok) {
            
        }

        public void Visit(TokenEos tok) {
            
        }

        public void Visit(TokenInclude tok) {
            //output.AppendLine(tok.Value);
        }

        public void Visit(TokenNumber tok) {
            
        }

        public void Visit(TokenQuotedString tok) {
            output.AppendLine(tok.Value);
        }

        public void Visit(TokenSymbol tok) {
            
        }

        public void Visit(TokenWhiteSpace tok) {
            
        }

        public void Visit(TokenWord tok) {
            //output.AppendLine(tok.Value);
        }

        public void Visit(TokenEof tok) {
            
        }

        public void Visit(TokenUnknown tok) {
            
        }

        public void Visit(TokenPreProcessed tok) {
            
        }
    }
}
