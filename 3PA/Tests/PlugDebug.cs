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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using _3PA.Lib;
using _3PA.Lib.Ftp;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Parser;

namespace _3PA.Tests {

    /// <summary>
    /// This class is only for debug/dev purposes, it will not be used in production
    /// </summary>
    internal class PlugDebug {

        #region tests and dev

        public static void GetCurrentScrollPageAddOrder() {
        }

        public static void StartDebug() {
        }

        public static void Test() {
            //UserCommunication.Message(("# What's new in this version? #\n\n" + File.ReadAllText(@"d:\Profiles\jcaillon\Desktop\derp.md", Encoding.Default)).MdToHtml(),
            //        MessageImg.MsgUpdate,
            //        "A new version has been installed!",
            //        "Updated to version " + AssemblyInfo.Version,
            //        new List<string> { "ok", "cancel" },
            //        true);

            Task.Factory.StartNew(() => {

                var ftp = new FtpsClient();
                bool connected = false;
                foreach (var mode in Extensions.EnumUtil.GetValues<EsslSupportMode>().OrderByDescending(mode => mode)) {
                    try {
                        ftp.Connect("localhost", ((mode & EsslSupportMode.Implicit) == EsslSupportMode.Implicit ? 990 : 21), new NetworkCredential("test", "superpwd"), mode, 1000);
                        connected = true;
                        UserCommunication.Notify(((EsslSupportModeAttr)mode.GetAttributes()).Value);
                    } catch (Exception) {
                        //ignored
                    }
                }

                if (connected) {
                    //ftp.PutFiles();
                }

                ftp.Close();

                /*
                Ftp ftpClient = new Ftp {
                    Host = "localhost",
                    User = "progress",
                    Pass = "progress",
                    UseSssl = true
                };
                if (ftpClient.CanConnect) {

                    UserCommunication.Notify(ftpClient.CreateDirectory("/fuck/more/stuff").ToString());
                    UserCommunication.Notify(ftpClient.Upload(@"/fuck/more/stuff/program.r", @"C:\Users\AdminLocal\Desktop\compile\_underescore.r").ToString());
                    UserCommunication.Notify(ftpClient.Download(@"/fuck/more/stuff/program.r", @"C:\Users\AdminLocal\Desktop\program.r").ToString());

                    UserCommunication.Notify(ftpClient.ErrorLog.ToString().Replace("\n", "<br>"));
                    UserCommunication.Notify(ftpClient.Log.ToString().Replace("\n", "<br>"));
                } else {
                    // coulnd't connect
                    UserCommunication.Notify("An error has occured when connecting to the FTP server,<br><b>Please check your connection information!</b><br><div class='ToolTipcodeSnippet'>" + ftpClient.ErrorLog + "</div><br><i>" + ErrorHandler.GetHtmlLogLink + "</i>", MessageImg.MsgError, "Ftp connection", "Failed");
                }
                */
            });
        }

        public static void RunParserTests() {

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

        public void Visit(TokenEol tok) {}

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

        public void Visit(TokenWhiteSpace tok) {}

        public void Visit(TokenWord tok) {
            Output.AppendLine("W  " + tok.Value);
        }

        public void Visit(TokenEof tok) {}

        public void Visit(TokenUnknown tok) {}

        public void Visit(TokenPreProcStatement tok) {
            //Output.AppendLine(tok.Value);
        }
    }

    #endregion

    #region create help file
    /*
    internal class CreateHelpConfigFile {

        public static void Do() {

            var output = new StringBuilder();
            var log = new StringBuilder();
            var indexContent = File.ReadAllText(@"C:\Work\3PA_side\ProgressFiles\help_tooltip\extracted\index.hhk", Encoding.Default);

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
                    *//*
                }
            }

            File.WriteAllText(@"C:\Work\3PA_side\ProgressFiles\help_tooltip\keywords.log", log.ToString(), Encoding.Default);
            File.WriteAllText(@"C:\Work\3PA_side\ProgressFiles\help_tooltip\keywordsHelp.data", output.ToString(), Encoding.Default);
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
