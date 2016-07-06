#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProGenerateCode.cs) is part of 3P.
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamuiFramework.Forms;
using _3PA.Data;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures {
    internal class ProGenerateCode {

        /// <summary>
        /// This method checks if the current document contains function prototypes that are not updated
        /// and correct them if needed
        /// </summary>
        public static void UpdateFunctionPrototypesIfNeeded(bool silent = false) {
            // make sure to parse the current document before checking anything
            ParserHandler.ParseCurrentDocument(true, true);

            // list the outdated proto
            var listOfOutDatedProto = ParserHandler.ParsedItemsList.Where(item => {
                var funcItem = item as ParsedFunction;
                if (funcItem != null) {
                    return funcItem.HasPrototype && !funcItem.PrototypeUpdated;
                }
                return false;
            }).Select(item => (ParsedFunction)item).ToList();

            // if everything is up to date
            if (listOfOutDatedProto.Count == 0) {
                if (!silent)
                    UserCommunication.Notify("There was nothing to be done :<br>All the prototypes match their implementation", MessageImg.MsgInfo, "Function prototypes", "Everything is synchronized", 5);
                return;
            }

            // we update the prototypes
            StringBuilder outputMessage = new StringBuilder("The following functions have had their prototype synchronized :<br>");
            Npp.BeginUndoAction();
            foreach (var function in listOfOutDatedProto) {
                // start of the prototype statement
                var startProtoPos = Npp.GetPosFromLineColumn(function.PrototypeLine, function.PrototypeColumn);
                // start of the function statement
                var startImplemPos = Npp.GetPosFromLineColumn(function.Line, function.Column);
                var protoStr = Npp.GetTextByRange(startImplemPos, function.EndPosition);

                // we take caution here...
                if (protoStr.EndsWith(":") && protoStr.CountOccurences(":") == 1) { 

                    protoStr = protoStr.Substring(0, protoStr.Length - 1).TrimEnd(' ') + " FORWARD.";
                    Npp.SetTextByRange(startProtoPos, function.PrototypeEndPosition, protoStr);

                    outputMessage.Append("<br> - <a href='" + function.FilePath + "#" + function.PrototypeLine + "#" + function.PrototypeColumn + "'>" + function.Name + "</a>");
                }
            }
            Npp.EndUndoAction();

            UserCommunication.NotifyUnique("Prototype_synchro", outputMessage.ToString(), MessageImg.MsgOk, "Function prototypes", "Synchronization done", args => {
                var split = args.Link.Split('#');
                if (split.Length == 3) {
                    Npp.Goto(split[0], int.Parse(split[1]), int.Parse(split[2]));
                    args.Handled = true;
                }
            }, 5);
        }

        /// <summary>
        /// Call this method to insert a new piece of code
        /// </summary>
        /// <param name="type"></param>
        public static void InsertNew(ProInsertNewType type) {

            string insertText = null;

            switch (type) {

                case ProInsertNewType.Function:
                    object newFunc = new ProNewFunction();
                    if (UserCommunication.Input("Insert function", "Define a new function", ref newFunc) != DialogResult.OK)
                        return;
                    break;

                case ProInsertNewType.Procedure:
                    var eol = Npp.GetEolString;
                    var appbuilderBefore = @"&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE {&name} Procedure" + eol;
                    var appbuilderAfter = @"&ANALYZE-RESUME" + eol;

                    object newProc = new ProNewProcedure();
                    if (UserCommunication.Input("Insert procedure", "Define a new internal procedure", ref newProc) != DialogResult.OK)
                        return;

                    var proNew = newProc as IProNew;
                    if (proNew == null)
                        return;

                    // reposition caret
                    RepositionCaretForInsertion(proNew, CompletionType.Procedure);
                    Npp.ModifyTextAroundCaret(0, 0, StripAppBuilderMarkup(Encoding.Default.GetString(DataResources.InternalProcedure)).Trim());

                    break;
                default:
                    return;
            }

            // stip appbuilder markup from the piece of code?
            if (!Abl.IsCurrentFileFromAppBuilder)
                insertText = StripAppBuilderMarkup(insertText);
        }

        /// <summary>
        /// Reposition the cursor to the best position for inserting a new IProNew
        /// </summary>
        private static void RepositionCaretForInsertion(IProNew proNew, CompletionType completionType) {
            // at caret position
            if (proNew.InsertPosition == ProInsertPosition.CaretPosition) {
                Npp.SetSelection(Npp.GetPosFromLineColumn(Npp.Line.CurrentLine, 0));
            } else {
                var findExisting = ParserHandler.CompletionItemsList.FirstOrDefault(data => data.Type == completionType);

                // is there already a proc existing?
                if (findExisting != null) {
                    // try to find a proc block, otherwise do from the proc itself
                    UserCommunication.Notify("existing proc");
                    Npp.SetSelection(Npp.CurrentPosition);

                } else {
                    Npp.TargetWholeDocument();
                    var previousFlags = Npp.SearchFlags;
                    Npp.SearchFlags = SearchFlags.Regex;
                    var foundPos = Npp.SearchInTarget(@"/\*\s+[\*]+\s+Internal Procedures");
                    Npp.SearchFlags = previousFlags;

                    // we found a comment indicating where the proc should be inserted?
                    if (foundPos > -1) {
                        Npp.SetSelection(Npp.GetPosFromLineColumn(Npp.LineFromPosition(foundPos), 0));

                    } else {
                        // we find the ideal pos considering the blocks
                        //var findBlock = ParserHandler.ParsedItemsList.FirstOrDefault(data => data.Type == )

                        // function proto : après &ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 
                        // proc : avant function implementation or last
                        // function implem : last

                        Npp.SetSelection(Npp.CurrentPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Allows to clear the appbuilder markup from a given string
        /// </summary>
        private static string StripAppBuilderMarkup(string inputSnippet) {
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

        internal interface IProNew {
            string Name { get; set; }
            ProInsertPosition InsertPosition { get; set; }
        }

        internal class ProNewProcedure : IProNew {

            [YamuiInputDialogItem("Name", Order = 0)]
            public string Name { get; set; }

            [YamuiInputDialogItem("Private procedure", Order = 1)]
            public bool IsPrivate { get; set; }

            [YamuiInputDialogItem("Insertion position", Order = 2)]
            public ProInsertPosition InsertPosition { get; set; }
        }

        internal class ProNewFunction : IProNew {

            [YamuiInputDialogItem("Name", Order = 0)]
            public string Name { get; set; }

            [YamuiInputDialogItem("Return type", Order = 1)]
            public ProFunctionType Type { get; set; }

            [YamuiInputDialogItem("Private function", Order = 2)]
            public bool IsPrivate { get; set; }

            [YamuiInputDialogItem("Insertion position", Order = 3)]
            public ProInsertPosition InsertPosition { get; set; }
        }

        internal enum ProInsertNewType {
            Procedure,
            Function
        }

        internal enum ProInsertPosition {
            [Description("Alphabetical order")]
            AlphabeticalOrder,
            [Description("First")]
            First,
            [Description("Last")]
            Last,
            [Description("At caret position")]
            CaretPosition
        }

        internal enum ProFunctionType {
            [Description("CHARACTER")]
            Character,
            [Description("HANDLE")]
            Handle,
            [Description("INTEGER")]
            Integer,
            [Description("LOGICAL")]
            Logical,
            [Description("COM-HANDLE")]
            ComHandle,
            [Description("DECIMAL")]
            Decimal,
            [Description("DATE")]
            Date,
            [Description("DATETIME")]
            Datetime,
            [Description("DATETIME-TZ")]
            DatetimeTz,
            [Description("INT64")]
            Int64,
            [Description("LONGCHAR")]
            Longchar,
            [Description("MEMPTR")]
            Memptr,
            [Description("RAW")]
            Raw,
            [Description("RECID")]
            Recid,
            [Description("ROWID")]
            Rowid,
            [Description("WIDGET-HANDLE")]
            WidgetHandle,
            [Description("CLASS XXX")]
            Class
        }

    }
}
