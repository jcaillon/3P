using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.Parser;

namespace YamuiDemoApp {
    class CreateHelpConfigFile {

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
                    */
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
}
