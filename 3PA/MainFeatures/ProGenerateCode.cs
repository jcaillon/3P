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

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamuiFramework.Forms;
using _3PA.Data;
using _3PA.Interop;
using _3PA.Lib;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures {
    internal class ProGenerateCode {

        #region Update function prototype

        /// <summary>
        /// This method checks if the current document contains function prototypes that are not updated
        /// and correct them if needed
        /// </summary>
        public static void UpdateFunctionPrototypesIfNeeded(bool silent = false) {
            // make sure to parse the current document before checking anything
            ParserHandler.ParseCurrentDocument(true, true);

            // list the outdated proto
            var listOfOutDatedProto = ParserHandler.AblParser.ParsedItemsList.Where(item => {
                var funcItem = item as ParsedFunction;
                return funcItem != null && funcItem.HasPrototype && !funcItem.PrototypeUpdated;
            }).Select(item => (ParsedFunction) item).ToList();

            // list the prototypes w/o implementation
            var listOfUselessProto = ParserHandler.AblParser.ParsedPrototypes.Where(item => {
                // it's a prototype with no implementation
                if (item.Value.IsPrototype && !ParserHandler.AblParser.ParsedItemsList.Exists(func => (func is ParsedFunction && func.Name.EqualsCi(item.Value.Name) && !((ParsedFunction)func).IsPrototype)))
                    return true;
                return false;
            }).Select(item => item.Value).ToList();

            // list the implementation w/o prototypes
            var listOfSoloImplementation = ParserHandler.AblParser.ParsedItemsList.Where(item => {
                var funcItem = item as ParsedFunction;
                return funcItem != null && !funcItem.HasPrototype;
            }).Select(item => (ParsedFunction) item).ToList();


            // if everything is up to date, dont go further
            if ((listOfOutDatedProto.Count + listOfUselessProto.Count + listOfSoloImplementation.Count) == 0) {
                if (!silent)
                    UserCommunication.Notify("There was nothing to be done :<br>All the prototypes match their implementation", MessageImg.MsgInfo, "Function prototypes", "Everything is synchronized", 5);
                return;
            }


            Npp.BeginUndoAction();

            // we update the prototypes
            StringBuilder outputMessage = new StringBuilder("The following functions have had their prototype synchronized :<br>");

            // update proto
            foreach (var function in listOfOutDatedProto) {
                // start of the prototype statement
                var startProtoPos = Npp.GetPosFromLineColumn(function.PrototypeLine, function.PrototypeColumn);

                // start of the function statement
                var startImplemPos = Npp.GetPosFromLineColumn(function.Line, function.Column);
                var protoStr = Npp.GetTextByRange(startImplemPos, function.EndPosition);

                // we take caution here... ensure that the prototype's syntax is correct
                if (protoStr.EndsWith(":") && protoStr.CountOccurences(":") == 1) {

                    // replace the end ":" by a " FOWARD."
                    protoStr = protoStr.Substring(0, protoStr.Length - 1).TrimEnd(' ') + " FORWARD.";
                    Npp.SetTextByRange(startProtoPos, function.PrototypeEndPosition, protoStr);

                    outputMessage.Append("<br> - <a href='" + function.FilePath + "#" + function.PrototypeLine + "#" + function.PrototypeColumn + "'>" + function.Name + "</a>");
                }
            }

            // delete proto
            foreach (var function in listOfUselessProto) {

                ParsedBlock protoBlock = null;

                // if we parsed the UIB (appbuilder) blocks correctly
                if (ParserHandler.AblParser.ParsedUibBlockOk) {

                    // try to find a _FUNCTION-FORWARD block with the name, as it surrounds the prototype if it exists
                    var protoRegex = new Regex(@"\s*_UIB-CODE-BLOCK\s+_FUNCTION-FORWARD\s+" + function.Name, RegexOptions.IgnoreCase);
                    protoBlock = ParserHandler.AblParser.ParsedItemsList.FirstOrDefault(item => {
                        var blockItem = item as ParsedBlock;
                        if (blockItem != null && blockItem.BlockType == ParsedBlockType.FunctionForward &&
                            protoRegex.Match(blockItem.BlockDescription).Success)
                            return true;
                        return false;
                    }) as ParsedBlock;
                }

                if (protoBlock != null) {
                    UserCommunication.Notify("Delete bloock : " + function.Name);
                    Npp.DeleteTextByRange(protoBlock.Position, protoBlock.EndBlockPosition);
                } else {
                    // if not found, we just delete the proto statement
                    UserCommunication.Notify("Delete : " + function.Name);
                    Npp.DeleteTextByRange(function.Position, function.EndPosition);
                }

                
            }

            // add proto
            foreach (var function in listOfSoloImplementation) {
                UserCommunication.Notify("Add : " + function.Name);
            }

            Npp.EndUndoAction();

            UserCommunication.NotifyUnique("Prototype_synchro", outputMessage.ToString(), MessageImg.MsgOk, "Function prototypes", "Synchronization done", args => {
                var split = args.Link.Split('#');
                if (split.Length == 3) {
                    Npp.Goto(split[0], Int32.Parse(split[1]), Int32.Parse(split[2]));
                    args.Handled = true;
                }
            }, 5);
        }

        #endregion

        #region Insert new

        /// <summary>
        /// Call this method to insert a new piece of code
        /// </summary>
        /// <param name="type"></param>
        public static void InsertNew(ProInsertNewType type) {
            object newCode;
            switch (type) {

                case ProInsertNewType.Function:
                    newCode = new ProNewFunction();
                    if (UserCommunication.Input(ref newCode, "Please provide information about the procedure that will be created", MessageImg.MsgQuestion, "Generate code", "Insert a new function") != 0)
                        return;
                    break;

                case ProInsertNewType.Procedure:
                    newCode = new ProNewProcedure();
                    if (UserCommunication.Input(ref newCode, "Please provide information about the procedure that will be created", MessageImg.MsgQuestion, "Generate code", "Insert a new procedure") != 0)
                        return;
                    break;

                default:
                    return;
            }
            InsertNew(type, newCode as IProNew);
        }

        /// <summary>
        /// Call this method to insert a new piece of code
        /// </summary>
        public static void InsertNew(ProInsertNewType type, IProNew newCode) {
            string insertText = null;
            string blockDescription = null;
            switch (type) {

                case ProInsertNewType.Function:
                    break;

                case ProInsertNewType.Procedure:

                    object newProc = new ProNewProcedure();
                    if (UserCommunication.Input(ref newProc, "Please provide information about the procedure that will be created", MessageImg.MsgQuestion, "Generate code", "Insert a new procedure") != 0)
                        return;

                    var proNew = newProc as IProNew;
                    if (proNew == null)
                        return;

                    // reposition caret
                    RepositionCaretForInsertion(proNew, CompletionType.Procedure);
                    Npp.ModifyTextAroundCaret(0, 0, Encoding.Default.GetString(DataResources.InternalProcedure).Trim());

                    break;
                default:
                    return;
            }

            var eol = Npp.GetEolString;
            if (!String.IsNullOrEmpty(blockDescription) && Abl.IsCurrentFileFromAppBuilder) {
                insertText = @"&ANALYZE-SUSPEND _UIB-CODE-BLOCK " + blockDescription + eol + insertText;
                insertText = insertText + eol + eol + @"/* _UIB-CODE-BLOCK-END */" + eol + @"&ANALYZE-RESUME" + eol;
            }


        }

        /// <summary>
        /// Reposition the cursor to the best position for inserting a new IProNew
        /// </summary>
        private static void RepositionCaretForInsertion(IProNew proNew, CompletionType completionType) {
            // at caret position
            if (proNew.InsertPosition == ProInsertPosition.CaretPosition) {
                Npp.SetSelection(Npp.GetPosFromLineColumn(Npp.Line.CurrentLine, 0));
            } else {
                var findExisting = ParserHandler.ParserVisitor.ParsedCompletionItemsList.FirstOrDefault(data => data.Type == completionType);

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

        internal interface IProNew {
            string Name { get; set; }
            ProInsertPosition InsertPosition { get; set; }
        }

        internal class ProNewProcedure : IProNew {

            [YamuiInput("Name", Order = 0)]
            public string Name { get; set; }

            [YamuiInput("Private procedure", Order = 1)]
            public bool IsPrivate { get; set; }

            [YamuiInput("Insertion position", Order = 2)]
            public ProInsertPosition InsertPosition { get; set; }
        }

        internal class ProNewFunction : IProNew {

            [YamuiInput("Name", Order = 0)]
            public string Name { get; set; }

            [YamuiInput("Return type", Order = 1)]
            public ProFunctionType Type { get; set; }

            [YamuiInput("Private function", Order = 2)]
            public bool IsPrivate { get; set; }

            [YamuiInput("Insertion position", Order = 3)]
            public ProInsertPosition InsertPosition { get; set; }

        }

        internal enum ProInsertNewType {
            Procedure,
            Prototype,
            Function
        }

        internal enum ProInsertPosition {
            [Description("Alphabetical order")] AlphabeticalOrder,
            [Description("First")] First,
            [Description("Last")] Last,
            [Description("At caret position")] CaretPosition
        }

        internal enum ProFunctionType {
            [Description("CHARACTER")] Character,
            [Description("HANDLE")] Handle,
            [Description("INTEGER")] Integer,
            [Description("LOGICAL")] Logical,
            [Description("COM-HANDLE")] ComHandle,
            [Description("DECIMAL")] Decimal,
            [Description("DATE")] Date,
            [Description("DATETIME")] Datetime,
            [Description("DATETIME-TZ")] DatetimeTz,
            [Description("INT64")] Int64,
            [Description("LONGCHAR")] Longchar,
            [Description("MEMPTR")] Memptr,
            [Description("RAW")] Raw,
            [Description("RECID")] Recid,
            [Description("ROWID")] Rowid,
            [Description("WIDGET-HANDLE")] WidgetHandle,
            [Description("CLASS XXX")] Class
        }

        #endregion

        #region Modification tags

        /// <summary>
        /// Allows the user to surround its selection with custom modification tags
        /// </summary>
        public static void SurroundSelectionWithTag() {
            CommonTagAction(fileInfo => {
                var output = new StringBuilder();

                Npp.TargetFromSelection();
                var indent = new String(' ', Npp.GetLine(Npp.LineFromPosition(Npp.TargetStart)).Indentation);

                var opener = FileTag.ReplaceTokens(fileInfo, Config.Instance.TagModifOpener);
                var eol = Npp.GetEolString;
                output.Append(opener);
                output.Append(eol);
                output.Append(indent);
                output.Append(Npp.SelectedText);
                output.Append(eol);
                output.Append(indent);
                output.Append(FileTag.ReplaceTokens(fileInfo, Config.Instance.TagModifCloser));

                Npp.TargetFromSelection();
                Npp.ReplaceTarget(output.ToString());

                Npp.SetSel(Npp.TargetStart + opener.Length + eol.Length);
            });
        }

        /// <summary>
        /// Allows the user to generate a title block at the caret location, using the current file info
        /// </summary>
        public static void AddTitleBlockAtCaret() {
            CommonTagAction(fileInfo => {
                var output = new StringBuilder();
                var eol = Npp.GetEolString;
                output.Append(FileTag.ReplaceTokens(fileInfo, Config.Instance.TagTitleBlock1));
                output.Append(eol);

                // description
                var regex = new Regex(@"({&de\s*})");
                var match = regex.Match(Config.Instance.TagTitleBlock2);
                if (match.Success && !String.IsNullOrEmpty(fileInfo.CorrectionDecription)) {
                    var matchedStr = match.Groups[1].Value;
                    foreach (var line in fileInfo.CorrectionDecription.BreakText(matchedStr.Length).Split('\n')) {
                        output.Append(Config.Instance.TagTitleBlock2.Replace(matchedStr, String.Format("{0,-" + matchedStr.Length + @"}", line)));
                        output.Append(eol);
                    }
                }

                output.Append(FileTag.ReplaceTokens(fileInfo, Config.Instance.TagTitleBlock3));
                output.Append(eol);

                Npp.SetTextByRange(Npp.CurrentPosition, Npp.CurrentPosition, output.ToString());
                Npp.SetSel(Npp.CurrentPosition + output.Length);
            });
        }

        private static void CommonTagAction(Action<FileTagObject> performAction) {
            var filename = Path.GetFileName(Plug.CurrentFilePath);
            if (FileTag.Contains(filename)) {
                var fileInfo = FileTag.GetLastFileTag(filename);
                Npp.BeginUndoAction();
                performAction(fileInfo);
                Npp.EndUndoAction();
            } else {
                UserCommunication.Notify("No info available for this file, please fill the file info form first!", MessageImg.MsgToolTip, "Insert modification tags", "No info available", 4);
                Appli.Appli.GoToPage(PageNames.FileInfo);
            }
        }

        #endregion

        #region Toggle comment

        /// <summary>
        /// If no selection, comment the line of the caret
        /// If selection, comment the selection as a block
        /// </summary>
        public static void ToggleComment() {
            Npp.BeginUndoAction();

            // for each selection (limit selection number)
            for (var i = 0; i < Npp.Selection.Count; i++) {
                var selection = Npp.GetSelection(i);

                int startPos;
                int endPos;
                bool singleLineComm = false;
                if (selection.Caret == selection.Anchor) {
                    // comment line
                    var thisLine = new Npp.Line(Npp.LineFromPosition(selection.Caret));
                    startPos = thisLine.IndentationPosition;
                    endPos = thisLine.EndPosition;
                    singleLineComm = true;
                } else {
                    startPos = selection.Start;
                    endPos = selection.End;
                }

                var toggleMode = ToggleCommentOnRange(startPos, endPos);
                if (toggleMode == 3)
                    selection.SetPosition(startPos + 3);

                // correct selection...
                if (!singleLineComm && toggleMode == 2) {
                    selection.End += 2;
                }
            }

            Npp.EndUndoAction();
        }

        /// <summary>
        /// Toggle comment on the specified range, returns a value indicating what has been done
        /// 0: null, 1: toggle off; 2: toggle on, 3: added
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        private static int ToggleCommentOnRange(int startPos, int endPos) {
            // the line is essentially empty
            if ((endPos - startPos) == 0) {
                Npp.SetTextByRange(startPos, startPos, "/*  */");
                return 3;
            }

            // line is surrounded by /* */
            if (Npp.GetTextOnRightOfPos(startPos, 2).Equals("/*") && Npp.GetTextOnLeftOfPos(endPos, 2).Equals("*/")) {
                if (Npp.GetTextByRange(startPos, endPos).Equals("/*  */")) {
                    // delete an empty comment
                    Npp.SetTextByRange(startPos, endPos, String.Empty);
                } else {
                    // delete /* */
                    Npp.SetTextByRange(endPos - 2, endPos, String.Empty);
                    Npp.SetTextByRange(startPos, startPos + 2, String.Empty);
                }
                return 1;
            }

            Npp.SetTextByRange(endPos, endPos, "*/");
            Npp.SetTextByRange(startPos, startPos, "/*");
            return 2;
        }

        #endregion
    }
}
