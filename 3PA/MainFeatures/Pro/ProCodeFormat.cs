#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (CodeBeautifier.cs) is part of 3P.
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

using System.IO;
using System.Text;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.Pro {

    internal static class ProCodeFormat {

        /// <summary>
        /// Tries to re-indent the code of the whole document
        /// </summary>
        public static void CorrectCodeIndentation() {

            var canIndent = ParserHandler.AblParser.ParsedBlockOk;
            UserCommunication.Notify(canIndent ? "This document can be reindented!" : "Oups can't reindent the code...<br>Log : <a href='" + Path.Combine(Config.FolderTemp, "lines.log") + "'>" + Path.Combine(Config.FolderTemp, "lines.log") + "</a>", canIndent ? MessageImg.MsgOk : MessageImg.MsgError, "Parser state", "Can indent?", 20);
            if (!canIndent) {
                StringBuilder x = new StringBuilder();
                var i = 0;
                var dic = ParserHandler.AblParser.LineInfo;
                while (dic.ContainsKey(i)) {
                    x.AppendLine((i + 1) + " > " + dic[i].BlockDepth + " , " + dic[i].Scope + " , " + dic[i].Scope.Name);
                    //x.AppendLine(item.Key + " > " + item.Value.BlockDepth + " , " + item.Value.Scope);
                    i++;
                }
                Utils.FileWriteAllText(Path.Combine(Config.FolderTemp, "lines.log"), x.ToString());
            }

            // Can we indent? We can't if we didn't parse the code correctly or if there are grammar errors
            if (ParserHandler.AblParser.ParsedBlockOk) {
                
            } else {
                UserCommunication.NotifyUnique("FormatDocumentFail", "This action can't be executed right now because it seems that your document contains grammatical errors.<br><br><i>If the code compiles sucessfully then i failed to parse your document correctly, please make sure to create an issue on the project's github and (if possible) include the incriminating code so i can fix this problem : <br><a href='#about'>Open the about window to get the github url</a>", MessageImg.MsgRip, "Format document", "Incorrect grammar", args => {
                    Appli.Appli.GoToPage(PageNames.Welcome); 
                    UserCommunication.CloseUniqueNotif("FormatDocumentFail");
                }, 20);
            }
        }

        /// <summary>
        /// Allows to clear the appbuilder markup from a given string
        /// </summary>
        public static string StripAppBuilderMarkup(string inputSnippet) {
            // consist in suppressing the lines starting with :
            // &ANALYZE-SUSPEND
            // &ANALYZE-RESUME
            // /* _UIB-CODE-BLOCK-END */
            // and, for this method only, also strips :
            // &IF DEFINED(EXCLUDE-&{name}) = 0 &THEN
            // &ENDIF
            var outputSnippet = new StringBuilder();
            string line;
            using (StringReader reader = new StringReader(inputSnippet)) {
                while ((line = reader.ReadLine()) != null) {
                    if (line.Length == 0 || (line[0] != '&' && !line.Equals(@"/* _UIB-CODE-BLOCK-END */")))
                        outputSnippet.AppendLine(line);
                }
            }
            return outputSnippet.ToString();
        }
    }
}
