#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using YamuiFramework.Helper;
using _3PA.Lib;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Parser;

namespace _3PA.Tests {

    /// <summary>
    /// This class is only for debug/dev purposes, it will not be used in production
    /// </summary>
    internal class PlugDebug {

        #region tests and dev


        public static void DebugTest1() {

            
            UserCommunication.Message(("# What's new in this version? #\n\n" + Utils.ReadAllText(Path.Combine(Npp.GetConfigDir(), @"Tests\content.md"), Encoding.Default)).MdToHtml(),
                MessageImg.MsgUpdate,
                "A new version has been installed!",
                "Updated to version " + AssemblyInfo.Version,
                new List<string> { "ok" },
                false);

            //UserCommunication.Notify("fuck");
            //UserCommunication.Notify("fuck");

            //UserCommunication.Notify(clickedButton.ToString() + "<br>" + ((ProGenerateCode.ProNewFunction)newFunc).Name
            //    + "<br>" + ((ProGenerateCode.ProNewFunction)newFunc).InsertPosition
            //    + "<br>" + ((ProGenerateCode.ProNewFunction)newFunc).IsPrivate
            //    + "<br>" + ((ProGenerateCode.ProNewFunction)newFunc).Type);

//            RunParserTests(Utils.ReadAllText(Path.Combine(Npp.GetConfigDir(), "Tests", "in.p")));

            //var stylersXml = XDocument.Load(Config.FileNppStylersXml);
            //var firstname = (string)stylersXml.Descendants("WidgetStyle").First(x => x.Attribute("name").Value.Equals("Selected text colour")).Attribute("bgColor");
            //UserCommunication.Notify(firstname);
        }

        public static void stack1() {
            stack2();
        }

        public static void stack2() {
            var x = 0;
            var y = 1 / x;
        }

        public static string RunExternalExe(string filename, string arguments = null) {
            var process = new Process();

            process.StartInfo.FileName = filename;
            if (!string.IsNullOrEmpty(arguments)) {
                process.StartInfo.Arguments = arguments;
            }

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            var stdOutput = new StringBuilder();
            process.OutputDataReceived += (sender, args) => stdOutput.Append(args.Data);

            string stdError = null;
            try {
                process.Start();
                process.BeginOutputReadLine();
                stdError = process.StandardError.ReadToEnd();
                process.WaitForExit();
            } catch (Exception e) {
                throw new Exception("OS error while executing " + filename + (arguments ?? "") + ": " + e.Message, e);
            }

            if (process.ExitCode == 0) {
                return stdOutput.ToString();
            } else {
                var message = new StringBuilder();

                if (!string.IsNullOrEmpty(stdError)) {
                    message.AppendLine(stdError);
                }

                if (stdOutput.Length != 0) {
                    message.AppendLine("Std output:");
                    message.AppendLine(stdOutput.ToString());
                }

                throw new Exception(process.ExitCode + ": " + message);
            }
        }

        public static void DebugTest2() {
            string message;

            StringBuilder str = new StringBuilder();
            StringBuilder str2 = new StringBuilder();

            try {
                message = RunExternalExe(@"C:\Progress\client\v1160_dv\dlc\bin\prolib.exe", @"V:\appli\progress\_others\derp.pl".ProQuoter() + " -create");
            } catch (Exception e) {
                message = e.Message;
            }
            UserCommunication.Notify(message);

            MeasureIt(() => {
                try {
                    message = RunExternalExe(@"V:\appli\progress\_others\yo.bat");
                } catch (Exception e) {
                    message = e.Message;
                }
            });

            MeasureIt(() => {
                // -replace -add -create -compress?
                for (int i = 0; i < 100; i++) {
                    for (int j = 0; j < 5; j++) {
                        str2.AppendLine(@"C:\Progress\client\v1160_dv\dlc\bin\prolib.exe".ProQuoter() + " " + @"V:\appli\progress\_others\derp.pl".ProQuoter() + " -a " + (@"V:\appli\progress\_others\fuck\compil-" + j + ".r").ProQuoter());
                        try {
                            str.Append(RunExternalExe(@"C:\Progress\client\v1160_dv\dlc\bin\prolib.exe", @"V:\appli\progress\_others\derp.pl".ProQuoter() + " -a " + (@"V:\appli\progress\_others\fuck\compil-" + j + ".r").ProQuoter()));
                        } catch (Exception e) {
                            str.Append(e.Message);
                        }
                    }
                }
            });

            File.AppendAllText(@"V:\appli\progress\_others\yo.bat", str2.ToString());
            UserCommunication.Notify(str.ToString());

        }

        public static void DebugTest3() {

            RunParserTests(Utils.ReadAllText(Path.Combine(Npp.GetConfigDir(), "Tests", "Parser_in.p")));

            //RunParserTests(Npp.Text);

            /*
            UserCommunication.Message(("# What's new in this version? #\n\n" + Utils.ReadAllText(@"C:\Users\Julien\Desktop\content.md")).MdToHtml(),
                    MessageImg.MsgUpdate,
                    "A new version has been installed!",
                    "Updated to version " + AssemblyInfo.Version,
                    new List<string> { "ok", "cancel" },
                    true);
             */
            /*
            Task.Factory.StartNew(() => {

                var ftp = new FtpsClient();
                bool connected = false;
                foreach (var mode in EsslSupportMode.ClearText.GetEnumValues<EsslSupportMode>().OrderByDescending(mode => mode)) {
                    try {
                        ftp.Connect("localhost", ((mode & EsslSupportMode.Implicit) == EsslSupportMode.Implicit ? 990 : 21), new NetworkCredential("test", "superpwd"), mode, 1000);
                        connected = true;
                        UserCommunication.Notify(mode.GetDescription());
                    } catch (Exception) {
                        //ignored
                    }
                }

                if (connected) {
                    //ftp.PutFiles();
                }

                ftp.Close();
            });
             * */
        }

        public static void DisplayBugs() {
            var wb = new WebServiceJson(WebServiceJson.WebRequestMethod.Get, Config.BugsGetWebWervice);
            wb.OnRequestEnded += webServ => {
                if (webServ.StatusCodeResponse != HttpStatusCode.OK)
                    UserCommunication.Notify(webServ.ResponseException.ToString());
                else
                    UserCommunication.Notify(webServ.JsonResponse);
            };
            wb.Execute();
        }

        public static void MeasureIt(Action toMeasure) {
            var watch = Stopwatch.StartNew();
            toMeasure();
            watch.Stop();
            UserCommunication.Notify(watch.ElapsedMilliseconds + "ms");
        }

        public static void RunParserTests(string content) {

            // create unique temporary folder
            var testDir = Path.Combine(Npp.GetConfigDir(), "Tests", DateTime.Now.ToString("yy.MM.dd_HH-mm-ss-fff"));

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

            Lexer lexer = new Lexer(content);

            //--------------
            watch.Stop();
            //--------------

            OutputLexerVisitor lexerVisitor = new OutputLexerVisitor();
            lexer.Accept(lexerVisitor);
            Utils.FileWriteAllText(outLocation, lexerVisitor.Output.ToString());
            File.AppendAllText(perfFile, @"LEXER DONE in " + watch.ElapsedMilliseconds + @" ms > nb items = " + lexerVisitor.NbItems + "\r\n");

            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            // PARSER
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            outLocation = Path.Combine(testDir, "Parser_out.txt");

            //------------
            watch = Stopwatch.StartNew();
            //------------

            Parser parser = new Parser(lexer, "", null, true);

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

        #region extract color scheme from less files
        /*
        private static void ExtracFromMultipleLess() {
            var output = @"D:\Profiles\jcaillon\Downloads\bootwatch\out.txt";
            foreach (var file in Directory.GetFiles(@"D:\Profiles\jcaillon\Downloads\bootwatch", "*.less")) {
                ExtracFromLess(file, output);
            }
        }

        private static void ExtracFromLess(string lessFile, string outputFile) {

            var colorsDictionary = new Dictionary<string, string>();

            var matchArray = new Dictionary<string, string> {
                {"ThemeAccentColor", "@brand-primary"},

                {"FormBack", "@body-bg"},
                {"FormFore", "@text-color"},
                {"FormBorder", "@jumbotron-bg"},
                {"FormAltBack", "@breadcrumb-bg"},
                {"SubTextFore", "@brand-primary"},

                {"ScrollBarNormalBack", "@breadcrumb-bg"},
                {"ScrollThumbNormalBack", "@breadcrumb-color"},
                {"ScrollBarHoverBack", "@breadcrumb-bg"},
                {"ScrollThumbHoverBack", "@breadcrumb-active-color"},
                {"ScrollBarDisabledBack", "@breadcrumb-bg"},
                {"ScrollThumbDisabledBack", "@buttondisabled-bg"},

                {"ButtonNormalBack", "@pagination-bg"},
                {"ButtonNormalFore", "@pagination-color"},
                {"ButtonNormalBorder", "@input-border"},

                {"ButtonHoverBack", "@buttonhover-bg"},
                {"ButtonHoverFore", "@buttonhover-color"},
                {"ButtonHoverBorder", "@buttonhover-border"},
                
                {"ButtonDisabledBack", "@buttondisabled-bg"},
                {"ButtonDisabledFore", "@text-muted"},
                {"ButtonDisabledBorder", "@input-border"},

                {"ButtonPressedFore", "@buttondpressed-bg"},

                {"LabelNormalFore", "@text-color"},
                {"LabelPressedFore", "@brand-primary"},
                {"LabelDisabledFore", "@text-muted"},

                {"TabNormalBack", "@body-bg"},
                {"TabNormalFore", "@text-color"},
                {"TabHoverFore", "@link-color"},
                {"TabPressedFore", "@link-hover-color"},

                {"MenuHoverBack", "@buttonhover-bg"},
                {"MenuHoverFore", "@buttonhover-color"},
                {"MenuFocusBack", "@buttondpressed-bg"},
                {"MenuFocusFore", "@buttonhover-color"},

                {"AutoCompletionHighlightBack", "@brand-warning"},
                {"AutoCompletionHighlightBorder", "@brand-warning-darker"},

                {"GenericLinkColor", "@link-color"},
                {"GenericErrorColor", "@brand-danger"},
            };

            var result = new StringBuilder();

            // extract colors from less file
            var regex = new Regex(@"(^@[\w-]*)\:\s*(.*?)\;.*?$");
            foreach (var line in File.ReadAllLines(lessFile).Select(s => s.Trim())) {
                if (regex.IsMatch(line)) {
                    string colorName = null;
                    string colorValue = null;
                    foreach (Match match in regex.Matches(line)) {
                        colorName = match.Groups[1].Value;
                        colorValue = match.Groups[2].Value;
                    }
                    if (colorValue != null) {
                        if (!colorsDictionary.ContainsKey(colorName))
                            colorsDictionary.Add(colorName, colorValue);
                    }
                }
            }

            // convert stuff like lighten/darken of reference to other colors
            var colorsDictTemp = new Dictionary<string, string>();
            foreach (var color in colorsDictionary) {
                var colorValue = color.Value;
                // ref
                if (colorValue.StartsWith("@") && colorsDictionary.ContainsKey(colorValue))
                    colorValue = colorsDictionary[colorValue];

                // transparent
                else if (colorValue.EqualsCi("transparent") && colorsDictionary.ContainsKey("@body-bg"))
                    colorValue = colorsDictionary["@body-bg"];
                if (!colorsDictTemp.ContainsKey(color.Key))
                    colorsDictTemp.Add(color.Key, FindColor(colorsDictTemp, colorValue));
            }

            var isBgDark = ColorTranslator.FromHtml(colorsDictTemp["@body-bg"]).IsColorDark();
            colorsDictTemp.Add("@brand-primary-darker", @"darken(@brand-warning, " + (isBgDark ? "" : "-") + "35%)");
            colorsDictTemp.Add("@buttonhover-bg", @"darken(@pagination-bg, " + (isBgDark ? "-" : "") + "20%)");
            colorsDictTemp.Add("@buttonhover-color", @"darken(@pagination-color, " + (isBgDark ? "" : "-") + "15%)");
            colorsDictTemp.Add("@buttonhover-border", @"darken(@input-border, " + (isBgDark ? "-" : "") + "15%)");
            colorsDictTemp.Add("@buttondisabled-bg", @"darken(@pagination-bg, " + (isBgDark ? "" : "-") + "35%)");
            colorsDictTemp.Add("@buttondpressed-bg", @"darken(@pagination-bg, " + (isBgDark ? "-" : "") + "35%)");

            colorsDictionary.Clear();
            foreach (var color in colorsDictTemp) {
                if (!colorsDictionary.ContainsKey(color.Key))
                    colorsDictionary.Add(color.Key, FindColor(colorsDictTemp, color.Value));
            }

            // and now write the result...
            foreach (var kpv in matchArray) {
                // the var corresponds to one of our var?
                if (colorsDictionary.ContainsKey(kpv.Value)) {
                    result.AppendLine(kpv.Key + "\t" + colorsDictionary[kpv.Value]);
                }
            }

            File.AppendAllText(outputFile, "\r\n\r\n\r\n> " + Path.GetFileNameWithoutExtension(lessFile) + "\r\n" + result);

        }

        private static string FindColor(Dictionary<string, string> refDic, string link) {
            if (link.StartsWith("@")) {
                if (refDic.ContainsKey(link))
                    return ConvertLightenDarken(refDic, FindColor(refDic, refDic[link]));
            }
            return ConvertLightenDarken(refDic, link);
        }

        private static string ConvertLightenDarken(Dictionary<string, string> refDic, string link) {
            try {
                if (link.StartsWith("darken") || link.StartsWith("lighten")) {
                    var darken = link.StartsWith("darken");
                    link = link.Substring(link.IndexOf("(", StringComparison.CurrentCultureIgnoreCase) + 1);
                    link = link.Substring(0, link.Length - 2);
                    var split = link.Split(',');
                    link = FindColor(refDic, split[0]);
                    var percent = float.Parse((split[1].Trim().Replace("%", "").Replace(",", "."))) / 100;
                    link = ColorTranslator.ToHtml(ColorTranslator.FromHtml(link).ModifyColorLuminosity((darken ? -1 : 1) * percent));
                }
            } catch (Exception) {
            }
            return link;
        }
        */
        #endregion

    }

    #region Parser

    internal class OutputParserVisitor : IParserVisitor {

        public void PreVisit() {
            Output = new StringBuilder();
        }

        public void PostVisit() {
        }

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

        public void Visit(ParsedOnStatement pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedProcedure pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedIncludeFile pars) {
            AppendEverything(pars);
        }

        public void Visit(ParsedPreProc pars) {
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

        public void Visit(TokenPreProcStatement tok) {
            AppendEverything(tok);
            NbItems++;
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
