#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParserLexerTests.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System.Diagnostics;
using System.IO;
using System.Text;
using _3PA.MainFeatures.Parser;

namespace _3PA.Tests {
    public class ParserLexerTests {

        public static void Run() {

            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            // PARSER
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            /*
            //------------
            var watch = Stopwatch.StartNew();
            //------------
            var inputFile = @"C:\Temp\in.p";
            Parser tok = new Parser(File.ReadAllText(inputFile), inputFile, null, true);

            OutputVis vis = new OutputVis();
            tok.Accept(vis);

            //--------------
            watch.Stop();
            //------------

            // OUPUT OF VISITOR
            File.WriteAllText(@"C:\Temp\out.p", vis.Output.AppendLine("\n\nDONE in " + watch.ElapsedMilliseconds + " ms").ToString());
            */

            // OUTPUT INFO ON EACH LINE
            /*
                StringBuilder x = new StringBuilder();
                var i = 0;
                var dic = tok.GetLineInfo;
                while (dic.ContainsKey(i)) {
                    x.AppendLine((i+1) + " > " + dic[i].BlockDepth + " , " + dic[i].Scope + " , " + dic[i].CurrentScopeName);
                    //x.AppendLine(item.Key + " > " + item.Value.BlockDepth + " , " + item.Value.Scope);
                    i++;
                }
                File.WriteAllText(@"C:\Temp\out.p", x.AppendLine("DONE in " + watch.ElapsedMilliseconds + " ms").ToString());
            */

            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            // LEXER
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

            
            //------------
            var watch2 = Stopwatch.StartNew();
            //------------

            Lexer tok2 = new Lexer(File.ReadAllText(@"C:\Temp\in.p"));
            tok2.Tokenize();
            OutputLexer vis2 = new OutputLexer();
            tok2.Accept(vis2);

            //--------------
            watch2.Stop();

            File.WriteAllText(@"C:\Temp\out.p", vis2.Output.AppendLine("DONE in " + watch2.ElapsedMilliseconds + " ms").ToString());
            
        }
    }

    internal class OutputVis : IParserVisitor {
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
            Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.Name);
        }

        public StringBuilder Output = new StringBuilder();
        public void Visit(ParsedOnEvent pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.On);
        }

        public void Visit(ParsedFunction pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > FUNCTION," + pars.Name + "," + pars.ReturnType + "," + pars.Scope + "," + pars.OwnerName + "," + pars.Parameters + "," + pars.IsPrivate + "," + pars.PrototypeLine + "," + pars.PrototypeColumn + "," + pars.IsExtended + "," + pars.EndLine);
        }
        
        public void Visit(ParsedProcedure pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.EndLine + "," + pars.Left);
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
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + ((ParseDefineTypeAttr)pars.Type.GetAttributes()).Value + "," + pars.LcFlagString + "," + pars.Name + "," + pars.AsLike + "," + pars.TempPrimitiveType + "," + pars.Scope + "," + pars.IsDynamic + "," + pars.ViewAs + "," + pars.BufferFor + "," + pars.Left + "," + pars.IsExtended + "," + pars.OwnerName);
        }

        public void Visit(ParsedTable pars) {
            //Output.Append(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.LcLikeTable + "," + pars.OwnerName + "," + pars.UseIndex + ">");
            //foreach (var field in pars.Fields) {
            //    Output.Append(field.Name + "|" + field.AsLike + "|" + field.Type + ",");
            //}
            //Output.AppendLine("");
        }

        public void Visit(ParsedRun pars) {
            //Output.AppendLine(pars.Line + "," + pars.Column + " > " + pars.Name + "," + pars.Left + "," + pars.HasPersistent);
        }
    }

    internal class OutputLexer : ILexerVisitor {

        public StringBuilder Output = new StringBuilder();

        public void Visit(TokenComment tok) {
            Output.AppendLine("C" + (tok.IsSingleLine ? "S" : "M") + " " + tok.Value);
        }

        public void Visit(TokenEol tok) {
           
        }

        public void Visit(TokenEos tok) {
            Output.AppendLine("EOS " + tok.Value);
        }

        public void Visit(TokenInclude tok) {
            //Output.AppendLine(tok.Value);
        }

        public void Visit(TokenNumber tok) {
            Output.AppendLine("N  " + tok.Value);
        }

        public void Visit(TokenString tok) {
            Output.AppendLine("S  " + tok.Value);
        }

        public void Visit(TokenStringDescriptor tok) {
            Output.AppendLine("D  " + tok.Value);
        }

        public void Visit(TokenSymbol tok) {
            Output.AppendLine("S  " + tok.Value);
        }

        public void Visit(TokenWhiteSpace tok) {
            
        }

        public void Visit(TokenWord tok) {
            Output.AppendLine("W  " + tok.Value);
        }

        public void Visit(TokenEof tok) {
            
        }

        public void Visit(TokenUnknown tok) {
            
        }

        public void Visit(TokenPreProcStatement tok) {
            //Output.AppendLine(tok.Value);
        }
    }

}
