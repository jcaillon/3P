using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using _3PA;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.Parser;

namespace YamuiDemoApp {
    class ParserLexerTests {

        public static void Run() {

            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            // PARSER
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

            //------------
            var watch = Stopwatch.StartNew();
            //------------

            Parser tok = new Parser(File.ReadAllText(@"C:\Users\Julien\Desktop\tt.p"), @"C:\Users\Julien\Desktop\tt.p", null);
            //Lexer tok = new Lexer(File.ReadAllText(@"E:\temp\sac-dev\sac\sac\src\proc_uib\sc42lsdd.w"));

            OutputVis vis = new OutputVis();
            tok.Accept(vis);

            //--------------
            watch.Stop();
            //------------

            // OUPUT OF VISITOR
            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", vis.Output.AppendLine("DONE in " + watch.ElapsedMilliseconds + " ms").ToString());


            // OUTPUT INFO ON EACH LINE
            if (true) {
                StringBuilder x = new StringBuilder();
                var i = 0;
                var dic = tok.GetLineInfo;
                while (dic.ContainsKey(i)) {
                    x.AppendLine((i+1) + " > " + dic[i].BlockDepth + " , " + dic[i].Scope + " , " + dic[i].CurrentScopeName);
                    //x.AppendLine(item.Key + " > " + item.Value.BlockDepth + " , " + item.Value.Scope);
                    i++;
                }
                File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", x.AppendLine("DONE in " + watch.ElapsedMilliseconds + " ms").ToString());
            }

            return;

            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            // LEXER
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

            //------------
            var watch2 = Stopwatch.StartNew();
            //------------

            Lexer tok2 = new Lexer(File.ReadAllText(@"C:\Users\Julien\Desktop\in.p"));
            tok2.Tokenize();
            OutputLexer vis2 = new OutputLexer();
            tok2.Accept(vis2);

            //--------------
            watch2.Stop();

            File.WriteAllText(@"C:\Users\Julien\Desktop\test.p", vis2.Output.AppendLine("DONE in " + watch2.ElapsedMilliseconds + " ms").ToString());
            
        }
    }

    public class OutputVis : IParserVisitor {
        public void Visit(ParsedBlock pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > BLOCK," + pars.Name + "," + pars.BranchType);
        }

        public void Visit(ParsedLabel pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name);
        }

        public void Visit(ParsedFunctionCall pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.ExternalCall);
        }

        public void Visit(ParsedFoundTableUse pars) {
            
        }

        public StringBuilder Output = new StringBuilder();
        public void Visit(ParsedOnEvent pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.On);
        }

        public void Visit(ParsedFunction pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > FUNCTION," + pars.Name + "," + pars.ReturnType + "," + pars.Scope + "," + pars.OwnerName + "," + pars.Parameters + "," + pars.IsPrivate + "," + pars.PrototypeLine + "," + pars.PrototypeColumn + "," + pars.IsExtended);
        }

        public void Visit(ParsedProcedure pars) {
            Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.EndLine + "," + pars.Left);
        }

        public void Visit(ParsedIncludeFile pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name);
        }

        public void Visit(ParsedPreProc pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.Flag + "," + pars.UndefinedLine);
        }

        public void Visit(ParsedDefine pars) {
            //if (pars.PrimitiveType == ParsedPrimitiveType.Buffer || pars.Type == ParseDefineType.Buffer)
            //if (pars.Type == ParseDefineType.Parameter)
            //if (string.IsNullOrEmpty(pars.ViewAs))
                //Output.AppendLine(pars.Line + "," + pars.Column + " > " + ((ParseDefineTypeAttr)pars.Type.GetAttributes()).Value + "," + pars.LcFlagString + "," + pars.Name + "," + pars.AsLike + "," + pars.TempPrimitiveType + "," + pars.Scope + "," + pars.IsDynamic + "," + pars.ViewAs + "," + pars.BufferFor + "," + pars.Left + "," + pars.IsExtended);
        }

        public void Visit(ParsedTable pars) {
            return;
            Output.Append("\r\n" + pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.LcLikeTable + "," + pars.OwnerName + "," + pars.UseIndex + ">");
            foreach (var field in pars.Fields) {
                Output.Append(field.Name + "|" + field.AsLike + "|" + field.Type + ",");
            }
        }

        public void Visit(ParsedRun pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.Left);
        }
    }

    public class OutputLexer : ILexerVisitor {

        public StringBuilder Output = new StringBuilder();

        public void Visit(TokenComment tok) {
            //output.AppendLine(tok.Value);
        }

        public void Visit(TokenEol tok) {
            
        }

        public void Visit(TokenEos tok) {
            Output.AppendLine("EOS");
        }

        public void Visit(TokenInclude tok) {
            Output.AppendLine(tok.Value);
        }

        public void Visit(TokenNumber tok) {
            
        }

        public void Visit(TokenQuotedString tok) {
            //output.AppendLine(tok.Value);
        }

        public void Visit(TokenSymbol tok) {
            
        }

        public void Visit(TokenWhiteSpace tok) {
            
        }

        public void Visit(TokenWord tok) {
            Output.AppendLine(tok.Value);
        }

        public void Visit(TokenEof tok) {
            
        }

        public void Visit(TokenUnknown tok) {
            
        }

        public void Visit(TokenPreProcStatement tok) {
            Output.AppendLine(tok.Value);
        }
    }
}
