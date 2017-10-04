#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (PlugDebug.cs) is part of 3P.
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WixToolset.Dtf.Compression;
using WixToolset.Dtf.Compression.Cab;
using WixToolset.Dtf.Compression.Zip;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.Pro;
using _3PA.NppCore;

namespace _3PA.Tests {
    /// <summary>
    /// This class is only for debug/dev purposes, it will not be used in production
    /// </summary>
    internal class PlugDebug {
        #region Debug test

        public static void DebugTest1() {
            UserCommunication.Notify(string.Concat(WebUtility.HtmlDecode(null).Split(' ')));
        }
        
        public static void DebugTest2() {

            var exec = new ProExecutionTableCrc {
                NeedDatabaseConnection = true
            };
            exec.OnExecutionOk += execution => {
                var sb = new StringBuilder();
                foreach (var tab in ((ProExecutionTableCrc)execution).GetTableCrc()) {
                    sb.Append(tab.QualifiedTableName + " -> " + tab.Crc + "<br>");
                }
                UserCommunication.Notify(sb.ToString());
            };
            exec.Start();
        }

        public static void DebugTest3() {
            /*
            UserCommunication.Notify(Npp.CurrentInternalLangName.ProQuoter() + "<br>Versus : " + Npp.NppLangs.Instance.GetLangName(Path.GetExtension(Npp.CurrentFile.Path)).ProQuoter());
            MeasureIt(() => {
                var parser = new NppAutoCompParser(Utils.ReadAllText(@"C:\Users\Julien\Desktop\in.p"));
                UserCommunication.Notify(parser.GetWordsList.Count.ToString());
            });
             */
            //UserCommunication.Notify(Path.GetExtension(Npp.CurrentFile.Path) + " = " + NppLangs.Instance.GetLangName(Path.GetExtension(Npp.CurrentFile.Path)) + " > " + NppLangs.Instance.GetLangDescription(Path.GetExtension(Npp.CurrentFile.Path)).Keywords.Count);
            //RunParserTests(Npp.Text);
            /*
            UserCommunication.Message(("# What's new in this version? #\n\n" + Utils.ReadAllText(@"C:\Users\Julien\Desktop\content.md")).MdToHtml(),
                MessageImg.MsgUpdate,
                "A new version has been installed!",
                "Updated to version " + AssemblyInfo.Version,
                new List<string> {"ok", "cancel"},
                true);
             */
             
        }
        
        #endregion

        #region debug

        public static void ParseReferenceFile() {
            RunParserTests(Utils.ReadAllText(Path.Combine(Npp.ConfigDirectory, "Tests", "Parser_in.p")));
        }

        public static void ParseCurrentFile() {
            RunParserTests(Sci.Text);
        }

        public static void ParseAllFiles() {
            // create unique temporary folder
            var testDir = Path.Combine(Npp.ConfigDirectory, "Tests", "ParseAllFiles_" + DateTime.Now.ToString("yy.MM.dd_HH-mm-ss-fff"));
            string outNotif = "";
            var outFile = Path.Combine(testDir, "out.txt");
            if (!Utils.CreateDirectory(testDir))
                return;

            var parserErrors = "";
            var watch2 = Stopwatch.StartNew();

            foreach (var file in Directory.EnumerateFiles(ProEnvironment.Current.BaseLocalPath, "*", SearchOption.AllDirectories)) {
                if (file.TestAgainstListOfPatterns(Config.Instance.FilesPatternProgress)) {
                    string outStr = file + " >>> ";

                    var watch = Stopwatch.StartNew();

                    ProLexer proLexer = new ProLexer(Utils.ReadAllText(file));
                    outStr += "ProLexer (" + watch.ElapsedMilliseconds + " ms), ";

                    Parser parser = new Parser(proLexer, "", null, true);
                    outStr += "Parser (" + watch.ElapsedMilliseconds + " ms), ";

                    if (parser.ParserErrors != null && parser.ParserErrors.Count > 0) {
                        outNotif += file.ToHtmlLink() + "<br>";
                        parserErrors += file + "<br>" + parser.ParseErrorsInHtml + "<br>";
                    }
                    /*
                    var parserVisitor = new ParserVisitor(true);
                    parser.Accept(parserVisitor);
                    outStr += "Visitor (" + watch.ElapsedMilliseconds + " ms)\r\n";
                    */

                    watch.Stop();

                    Utils.FileAppendAllText(outFile, outStr + "\r\n");
                }
            }

            Utils.FileAppendAllText(outFile, "\r\n\r\n" + parserErrors);

            watch2.Stop();

            Utils.FileAppendAllText(outFile, "\r\n\r\nTotal time : " + watch2.ElapsedMilliseconds);

            UserCommunication.Notify(outNotif + "<br>Done :<br>" + outFile.ToHtmlLink(), 0);
        }
        
        #endregion

        #region tests and dev

        public static void DisplayBugs() {
            var wb = new WebServiceJson(WebServiceJson.WebRequestMethod.Get, Config.GetBugsWebWervice);
            // wb.AddToReq("UUID", "allo");
            wb.OnRequestEnded += webServ => {
                if (webServ.StatusCodeResponse != HttpStatusCode.OK)
                    UserCommunication.Notify(webServ.ResponseException.ToString());
                else
                    UserCommunication.Notify(webServ.JsonResponse);
            };
            wb.Execute();
        }

        public static void MeasureIt(Action toMeasure, string id = null) {
            var watch = Stopwatch.StartNew();
            toMeasure();
            watch.Stop();
            UserCommunication.Notify((id ?? "") + watch.ElapsedMilliseconds + "ms");
        }

        public static void RunParserTests(string content) {
            // create unique temporary folder
            var testDir = Path.Combine(Npp.ConfigDirectory, "Tests", "RunParserTests_" + DateTime.Now.ToString("yy.MM.dd_HH-mm-ss-fff"));

            var perfFile = Path.Combine(testDir, "perfs.txt");
            if (!Utils.CreateDirectory(testDir))
                return;

            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            // LEXER
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            var outLocation = Path.Combine(testDir, "Lexer_out.txt");

            //------------
            var watch = Stopwatch.StartNew();
            //------------

            ProLexer proLexer = new ProLexer(content);

            //--------------
            watch.Stop();
            //--------------

            OutputLexerVisitor lexerVisitor = new OutputLexerVisitor();
            proLexer.Accept(lexerVisitor);
            Utils.FileWriteAllText(outLocation, lexerVisitor.Output.ToString());
            File.AppendAllText(perfFile, @"LEXER DONE in " + watch.ElapsedMilliseconds + @" ms > nb items = " + lexerVisitor.NbItems + "\r\n");

            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            // PARSER
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            outLocation = Path.Combine(testDir, "Parser_out.txt");

            //------------
            watch = Stopwatch.StartNew();
            //------------

            Parser parser = new Parser(proLexer, "", null, true);

            //--------------
            watch.Stop();
            //------------

            OutputParserVisitor parserVisitor = new OutputParserVisitor();
            parser.Accept(parserVisitor);
            Utils.FileWriteAllText(outLocation, parserVisitor.Output.ToString());
            File.AppendAllText(perfFile, @"PARSER DONE in " + watch.ElapsedMilliseconds + @" ms > nb items = " + parser.ParsedItemsList.Count + "\r\n");

            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            // LINE INFO
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            outLocation = Path.Combine(testDir, "LineInfo_out.txt");

            StringBuilder lineInfo = new StringBuilder();
            var i = 0;
            var dic = parser.LineInfo;
            while (dic.ContainsKey(i)) {
                lineInfo.AppendLine(i + 1 + " > " + dic[i].BlockDepth + " , " + dic[i].Scope + " , " + dic[i].Scope.ScopeType + " , " + dic[i].Scope.Name);
                i++;
            }
            Utils.FileWriteAllText(outLocation, lineInfo.ToString());
            File.AppendAllText(perfFile, @"nb items in Line info = " + parser.LineInfo.Count + "\r\n");

            UserCommunication.Notify("Done :<br>" + testDir.ToHtmlLink());
        }

        #endregion
    }

    #region Parser

    internal class OutputParserVisitor : IParserVisitor {
        public void PreVisit(Parser parser) {
            Output = new StringBuilder();
        }

        public void PostVisit() {}

        public StringBuilder Output;

        private void AppendEverything<T>(T item) {
            // type
            Output.Append(typeof(T).Name + "\t:");

            // for each field
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(item)) {
                Output.Append("\t" + prop.Name + "=[" + prop.GetValue(item).ConvertToStr() + "]");
            }

            Output.Append("\r\n");
        }

        public void Visit(ParsedWord pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedFile pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedPreProcBlock pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedImplementation pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedPrototype pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedLabel pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedFunctionCall pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedFoundTableUse pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedEvent pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedOnStatement pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedProcedure pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedIncludeFile pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedPreProcVariable pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedDefine pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedTable pars) {
            AppendEverything(pars);
            foreach (var field in pars.Fields) {
                AppendEverything(field);
            }
            foreach (var index in pars.Indexes) {
                AppendEverything(index);
            }
        }

        public void Visit(ParsedRun pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedBuffer pars) {
            AppendEverything(pars);
        }
    }

    internal class OutputLexerVisitor : ILexerVisitor {
        public int NbItems;
        public StringBuilder Output = new StringBuilder();

        private void AppendEverything<T>(T item) {
            // type
            Output.Append(typeof(T).Name + "\t:");

            // for each field
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(item)) {
                Output.Append("\t" + prop.Name + "=[" + prop.GetValue(item).ConvertToStr() + "]");
            }

            Output.Append("\r\n");
        }

        public void Visit(TokenComment tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenEol tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenEos tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenPreProcVariable tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenInclude tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenNumber tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenString tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenStringDescriptor tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenSymbol tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenWhiteSpace tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenWord tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenEof tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenUnknown tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void Visit(TokenPreProcDirective tok) {
            AppendEverything(tok);
            NbItems++;
        }

        public void PreVisit(MainFeatures.Parser.Lexer lexer) {
        }

        public void PostVisit() {
        }
    }

    #endregion

    #region create help file

    /*
    internal class CreateHelpConfigFile {

        public static void Do() {

            var output = new StringBuilder();
            var log = new StringBuilder();
            var indexContent = Utils.ReadAllText(@"C:\Work\3PA_side\ProgressFiles\help_tooltip\extracted\index.hhk", Encoding.Default);

            foreach (var items in File.ReadAllLines(@"C:\Work\3PA_side\ProgressFiles\help_tooltip\keywords.data", Encoding.Default).Select(line => line.Split('\t')).Where(items => items.Count() == 4)) {

                string keyword = items[0];

                var regex = new Regex("<param name=\"Name\" value=\"(" + keyword + " " + items[1] + "[^\"]*)\">[^<]*(?:<param name=\"Name\" value=\"([^\"]*)\">[^<]*)?<param name=\"Local\" value=\"([^\"]*)\">", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var m = regex.Match(indexContent);
                if (m.Success) {
                    var filename = m.Groups[3].Value;
                    if (filename.Contains("#"))
                        filename = filename.Substring(0, filename.IndexOf('#'));
                    if (File.Exists(@"D:\temp\lgrfeng\" + filename))
                        output.AppendLine(keyword + " " + items[1] + "\t" + ExtractFromHtml(filename));
                    else
                        log.AppendLine("file not found for :" + keyword + " " + items[1]);
                } else {
                    if (items[1] != "Statement" && items[1] != "Appbuilder" && items[1] != "Abbreviation" && items[1] != "Option" && items[1] != "Preprocessor" && items[1] != "Type") {

                        string newmatchtext = keyword;

                        if (items[1] == "Method")
                            newmatchtext = keyword + @"[\W]+method";

                        regex = new Regex("<param name=\"Name\" value=\"(" + newmatchtext + "[^\"]*)\">[^<]*(?:<param name=\"Name\" value=\"([^\"]*)\">[^<]*)?<param name=\"Local\" value=\"([^\"]*)\">", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        m = regex.Match(indexContent);
                        if (m.Success) {
                            var filename = m.Groups[3].Value;
                            if (filename.Contains("#"))
                                filename = filename.Substring(0, filename.IndexOf('#'));
                            if (File.Exists(@"D:\temp\lgrfeng\" + filename)) {
                                output.AppendLine(keyword + " " + items[1] + "\t" + ExtractFromHtml(filename));
                            } else
                                log.AppendLine("file not found for :" + keyword + " " + items[1]);
                        } else {
                            log.AppendLine(keyword + " " + items[1]);
                        }
                    }
                    /*
                    // try to match only the keyword?
                    regex = new Regex("<param name=\"Name\" value=\"" + keyword + " [^\"]*" + items[1] + "[^\"]*\">[^<]*<param name=\"Local\" value=\"([^\"]*)\">", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    m = regex.Match(indexContent);
                    if (m.Success) {
                        //MessageBox.Show(@"degraded regex for " + keyword + " :" + m.Groups[1].Value);
                        if (File.Exists(@"D:\temp\lgrfeng\" + m.Groups[1].Value))
                            output.AppendLine(keyword + " " + items[1] + "\t" + ExtractFromHtml(m.Groups[1].Value));
                    }
                    */
    /*
}
}

Utils.FileWriteAllText(@"C:\Work\3PA_side\ProgressFiles\help_tooltip\keywords.log", log.ToString(), Encoding.Default);
Utils.FileWriteAllText(@"C:\Work\3PA_side\ProgressFiles\help_tooltip\keywordsHelp.data", output.ToString(), Encoding.Default);
}


private static string ExtractFromHtml(string htmlName) {
string paragraph = "";
var synthax = new StringBuilder();
int status = 0;
bool first = true;
foreach (var lines in File.ReadAllLines(@"D:\temp\lgrfeng\" + htmlName, Encoding.Default)) {
var line = lines.Trim();
if (status == 0) {
  if (line.StartsWith("<div class=\"paragraph\">")) {
      paragraph = ClearTags(line);
      status++;
  }
} else if (status == 1) {
  if (line.StartsWith("<div class=\"paramete"))
      status = -1;

  if (line.StartsWith("<table class=\"table_")) {
      status++;
      first = true;
  }
} else if (status == 2) {
  if (line.StartsWith("</table>")) {
      status = 1;
      synthax.Append("\t");
  }

  if (line.StartsWith("<pre class=\"cell_003acode\">")) {
      if (!first) synthax.Append("<br>");
      if (first) first = false;
      synthax.Append(ClearTags(line));

  }
}
}
return paragraph + "\t" + synthax;
}

private static string ClearTags(string input) {
return Regex.Replace(input.Replace(@"<br />", "~n"), "<.*?>", string.Empty).Replace("~n", "<br>").Replace("\t", " ");
}

}
*/

    #endregion
}