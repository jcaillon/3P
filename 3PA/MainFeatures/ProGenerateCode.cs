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
using System.Collections.Generic;
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
using _3PA.MainFeatures.Parser;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures {
    internal class ProGenerateCode {

        #region Update/delete/add function prototype

        /// <summary>
        /// This method checks if the current document contains function prototypes that are not updated
        /// and correct them if needed
        /// </summary>
        public static void UpdateFunctionPrototypesIfNeeded(bool silent = false) {

            List<ParsedFunction> listOfOutDatedProto;
            List<ParsedFunction> listOfSoloImplementation;
            List<ParsedFunction> listOfUselessProto;

            StringBuilder outputMessage = new StringBuilder();
            bool nothingDone = true;

            // if there is at least 1 thing to do
            if (GetPrototypesLists(out listOfOutDatedProto, out listOfSoloImplementation, out listOfUselessProto)) {

                Npp.BeginUndoAction();

                // Add proto
                if (listOfSoloImplementation.Count > 0) {
                    var tempMes = new StringBuilder("The following function prototypes have been created :");
                    var nbNotCreated = 0;
                    while (listOfSoloImplementation.Count > nbNotCreated) {
                        var ok = AddPrototypes(ref tempMes, listOfSoloImplementation[0]);
                        if (!ok) nbNotCreated++;
                        nothingDone = !ok && nothingDone;
                        GetPrototypesLists(out listOfOutDatedProto, out listOfSoloImplementation, out listOfUselessProto);
                    }
                    tempMes.Append("<br><br>");
                    if (!nothingDone)
                        outputMessage.Append(tempMes);
                }

                // delete proto
                if (listOfUselessProto.Count > 0) {
                    outputMessage.Append("The following prototypes have been deleted :");
                    while (listOfUselessProto.Count > 0) {
                        nothingDone = !DeletePrototypes(ref outputMessage, listOfUselessProto[0]) && nothingDone;
                        GetPrototypesLists(out listOfOutDatedProto, out listOfSoloImplementation, out listOfUselessProto);
                    }
                    outputMessage.Append("<br><br>");
                }

                // update proto
                if (listOfOutDatedProto.Count > 0) {
                    outputMessage.Append("The following functions have had their prototype synchronized :");
                    while (listOfOutDatedProto.Count > 0) {
                        nothingDone = !UpdatePrototypes(ref outputMessage, listOfOutDatedProto[0]) && nothingDone;
                        GetPrototypesLists(out listOfOutDatedProto, out listOfSoloImplementation, out listOfUselessProto);
                    }
                    outputMessage.Append("<br><br>");
                }

                Npp.EndUndoAction();

            }

            if (nothingDone) {
                if (!silent)
                    UserCommunication.Notify("There was nothing to be done :<br>All the prototypes match their implementation", MessageImg.MsgInfo, "Function prototypes", "Everything is synchronized", 5);
            } else {
                outputMessage.Append("<div><i>CTRL + Z will cancel the above-mentionned modifications</i></div>");
                UserCommunication.NotifyUnique("Prototype_synchro", outputMessage.ToString(), MessageImg.MsgOk, "Function prototypes", "Synchronization done", args => {
                    var split = args.Link.Split('#');
                    if (split.Length == 2) {
                        Npp.GotoPos(split[0], Int32.Parse(split[1]));
                        args.Handled = true;
                    }
                }, 5);
            }
        }

        /// <summary>
        /// Gets the list of functions/proto of interest
        /// </summary>
        private static bool GetPrototypesLists(out List<ParsedFunction> listOfOutDatedProto, out List<ParsedFunction> listOfSoloImplementation, out List<ParsedFunction> listOfUselessProto) {

            // make sure to parse the current document before checking anything
            ParserHandler.ParseCurrentDocument(true, true);

            // list the outdated proto
            listOfOutDatedProto = ParserHandler.AblParser.ParsedItemsList.Where(item => {
                var funcItem = item as ParsedFunction;
                return funcItem != null && funcItem.HasPrototype && !funcItem.PrototypeUpdated;
            }).Select(item => (ParsedFunction)item).ToList();

            // list the implementation w/o prototypes
            listOfSoloImplementation = ParserHandler.AblParser.ParsedItemsList.Where(item => {
                var funcItem = item as ParsedFunction;
                return funcItem != null && !funcItem.HasPrototype;
            }).Select(item => (ParsedFunction)item).ToList();

            // list the prototypes w/o implementation
            listOfUselessProto = ParserHandler.AblParser.ParsedPrototypes.Where(item => {
                // it's a prototype with no implementation
                if (item.Value.Type == ParsedFunctionType.ForwardSimple && !ParserHandler.AblParser.ParsedItemsList.Exists(func => func is ParsedFunction && func.Name.EqualsCi(item.Value.Name) && ((ParsedFunction)func).Type == ParsedFunctionType.Implementation))
                    return true;
                return false;
            }).Select(item => item.Value).ToList();

            return listOfOutDatedProto.Count > 0 || listOfSoloImplementation.Count > 0 || listOfUselessProto.Count > 0;
        }

        /// <summary>
        /// This method checks if the current document contains function prototypes that are not updated
        /// and correct them if needed
        /// </summary>
        private static bool UpdatePrototypes(ref StringBuilder outputMessage, ParsedFunction function) {

            var protoStr = Npp.GetTextByRange(function.Position, function.EndPosition);

            // we take caution here... ensure that the implementation syntax is correct
            if (protoStr.EndsWith(":") && protoStr.CountOccurences(":") == 1) {

                // replace the end ":" by a " FOWARD."
                protoStr = protoStr.Substring(0, protoStr.Length - 1).TrimEnd(' ') + " FORWARD.";
                Npp.SetTextByRange(function.PrototypePosition, function.PrototypeEndPosition, protoStr);

                outputMessage.Append("<br> - <a href='" + function.FilePath + "#" + (function.PrototypePosition) + "'>" + function.Name + "</a>");
            }

            return true;
        }

        private static bool AddPrototypes(ref StringBuilder outputMessage, ParsedFunction function) {

            var protoStr = Npp.GetTextByRange(function.Position, function.EndPosition);

            // we take caution here... ensure that the implementation syntax is correct
            if (protoStr.EndsWith(":") && protoStr.CountOccurences(":") == 1) {

                // get the best position to insert the prototype
                bool insertBefore;
                int insertPos = GetCaretPositionForInsertion<ParsedFunction>(new ProNewPrototype { Name = function.Name }, out insertBefore);

                // if we didn't find a good position, then let's assume the user doesn't need one
                if (insertPos > 0) {

                    // replace the end ":" by a " FOWARD."
                    protoStr = FormatInsertion(protoStr.Substring(0, protoStr.Length - 1).TrimEnd(' ') + " FORWARD.", "_FUNCTION-FORWARD " + function.Name + " Procedure", insertBefore);

                    Npp.SetTextByRange(insertPos, insertPos, protoStr);

                    //outputMessage.Append("<br> - <a href='" + function.FilePath + "#" + insertPos + "'>" + function.Name + "</a>");
                    outputMessage.Append("<br> - " + function.Name);

                    return true;
                }
            }

            return false;
        }

        private static bool DeletePrototypes(ref StringBuilder outputMessage, ParsedFunction function) {

            var protoPreProcBlock = GetPreProcBlock(function.Name, function.Position, "_FUNCTION-FORWARD");

            // we also want to delete the trailing new lines
            int endPosition = (protoPreProcBlock != null ? protoPreProcBlock.EndBlockPosition : function.EndPosition);
            while (Npp.GetTextByRange(endPosition, endPosition + 2).Equals(Npp.GetEolString)) {
                endPosition += 2;
            }

            if (protoPreProcBlock != null) {
                Npp.DeleteTextByRange(protoPreProcBlock.Position, endPosition);
            } else {
                // if not found, we just delete the proto statement
                Npp.DeleteTextByRange(function.Position, endPosition);
            }

            outputMessage.Append("<br> - " + function.Name);

            return true;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// returns the best caret position for inserting a new IProNew
        /// </summary>
        private static int GetCaretPositionForInsertion<T>(IProNew proNew, out bool insertBefore) where T : ParsedScopeItem {
            insertBefore = false;

            // at caret position
            if (proNew.InsertPosition == ProInsertPosition.CaretPosition && !(proNew is ProNewPrototype))
                return Npp.GetPosFromLineColumn(Npp.Line.CurrentLine, 0);


            T refItem = null;

            #region set insertBefore and refItem

            // the following is a little annoying to code and understand...
            // the idea is to get (or dont get if it doesn't exist) the previous or the next item
            // of type T in the existing list of said types so we can "anchor" on it to insert
            // our new stuff...
            if (proNew is ProNewPrototype) {
                // find the previous/next function implementation with a prototype
                bool found = false;
                foreach (var item2 in ParserHandler.AblParser.ParsedItemsList.Where(item => item.GetType() == typeof(T)).OrderBy(item => item.Line)) {
                    var item = item2 as T;
                    if (item != null) {
                        // we didn't match our current function implementation yet
                        if (!found) {
                            // we just did
                            if (item.Name.Equals(proNew.Name)) {
                                found = true;
                                continue;
                            }
                            // set previous item
                            if (((ParsedFunction)item2).HasPrototype && ((ParsedFunction)item2).Type == ParsedFunctionType.Implementation)
                                refItem = item;
                        } else {
                            // match first item after we found our implementation
                            insertBefore = true;
                            if (((ParsedFunction)item2).HasPrototype && ((ParsedFunction)item2).Type == ParsedFunctionType.Implementation) {
                                refItem = item;
                                break;
                            }
                        }
                    }
                }
                if (!found)
                    refItem = null;

                // now we need its proto
                if (refItem != null) {
                    refItem = ParserHandler.AblParser.ParsedPrototypes.Select(pair => pair.Value).FirstOrDefault(fun => fun.Type == ParsedFunctionType.ForwardSimple && fun.Name.Equals(refItem.Name)) as T;
                }

            } else {
                var existingList = ParserHandler.AblParser.ParsedItemsList.Where(item => item.GetType() == typeof(T)).Select(item => (T)item).ToList();
                if (existingList.Count > 0) {

                    // alphabetical order
                    if (proNew.InsertPosition == ProInsertPosition.AlphabeticalOrder) {
                        // find the position that would take our new code
                        int index = existingList.Select(item => item.Name).ToList().BinarySearch(proNew.Name);
                        if (index < 0) {
                            index = ~index - 1; // we get the index in which it should be inserted - 1
                            if (index == -1) {
                                insertBefore = true;
                                refItem = existingList[0];
                            } else {
                                refItem = existingList[index];
                            }
                        }

                        // first of its kind
                    } else if (proNew.InsertPosition == ProInsertPosition.First) {
                        refItem = existingList.FirstOrDefault();
                        insertBefore = true;
                    } else if (proNew.InsertPosition == ProInsertPosition.Last) {
                        refItem = existingList.LastOrDefault();
                    }
                }

            }

            #endregion

            // is there already an item existing?
            if (refItem != null) {
                // try to find a proc block, otherwise do from the proc itself

                // try to find a &IF DEFINED(EXCLUDE- block that surrounds the prototype
                var preProcBlock = GetPreProcBlock(refItem.Name, refItem.Position);
                if (preProcBlock != null)
                    return (insertBefore ? preProcBlock.Position : preProcBlock.EndBlockPosition);

                // otherwise return the position of the function itself
                return (insertBefore ? refItem.Position : refItem.EndBlockPosition);
            }

            // can we find a comment indicating where the proc should be inserted?
            string typeComment = null;
            if (proNew is ProNewFunction) {
                typeComment = @"Function\s+Implementations";
            } else if (proNew is ProNewPrototype) {
                typeComment = @"Function\s+Prototypes";
            } else if (proNew is ProNewProcedure) {
                typeComment = @"Internal\s+Procedures";
            }
            if (typeComment != null) {
                Npp.TargetWholeDocument();
                var previousFlags = Npp.SearchFlags;
                Npp.SearchFlags = SearchFlags.Regex;
                var streg = @"\/\*\s+[\*]+\s+" + typeComment + @"\s+[\*]+";
                var foundPos = Npp.SearchInTarget(streg);
                Npp.SearchFlags = previousFlags;
                if (foundPos == -1) {
                    foundPos = new Regex(@"\/\*\s+[\*]+\s+" + typeComment + @"\s+[\*]+").Match(Npp.Text).Index;
                    if (foundPos == 0) foundPos = -1;
                }
                if (foundPos > -1) {
                    return Npp.GetPosFromLineColumn(Npp.LineFromPosition(foundPos) + 1, 0);
                }
            }

            // At last, we find the best position considering the appbuilder blocks
            if (proNew is ProNewFunction) {
                // function implementation goes all the way bottom
                return Npp.TextLength;

            }
            if (proNew is ProNewProcedure) {
                // new procedure goes before the first function implementation of last
                var firstFunc = ParserHandler.AblParser.ParsedItemsList.FirstOrDefault(item => item is ParsedFunction);
                if (firstFunc != null) {
                    insertBefore = true;

                    // try to find a &IF DEFINED(EXCLUDE- block that surrounds the func
                    var preProcBlock = GetPreProcBlock(firstFunc.Name, firstFunc.Position);
                    if (preProcBlock != null)
                        return preProcBlock.Position;

                    return firstFunc.Position;
                }
            }
            if (proNew is ProNewPrototype) {
                // prototypes go after &ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 
                var preprocessorBlock = ParserHandler.AblParser.ParsedItemsList.FirstOrDefault(item => item is ParsedPreProcBlock && ((ParsedPreProcBlock)item).Type == ParsedPreProcBlockType.UibPreprocessorBlock);
                if (preprocessorBlock != null) {
                    return ((ParsedPreProcBlock)preprocessorBlock).EndBlockPosition;
                }
            }

            return -1;
        }

        /// <summary>
        /// returns the surrounding IF DEFINED or _UIB-CODE-BLOCK of a function, procedure.. if it exists
        /// otherwise returns null
        /// </summary>
        private static ParsedPreProcBlock GetPreProcBlock(string functionName, int surroundedPosition, string typeStr = @"[\w-]+") {

            // if we parsed the UIB (appbuilder) blocks correctly
            if (ParserHandler.AblParser.ParsedUibBlockOk) {

                // try to find a &IF DEFINED(EXCLUDE- block that surrounds the prototype
                var protoPreProcBlock = ParserHandler.AblParser.ParsedItemsList.Where(item => {
                    var blockItem = item as ParsedPreProcBlock;
                    if (blockItem != null && blockItem.Type == ParsedPreProcBlockType.IfEndIf &&
                        blockItem.BlockDescription.ContainsFast(@"DEFINED(EXCLUDE-" + functionName + @")"))
                        return true;
                    return false;
                }).ToList();
                if (protoPreProcBlock.Count == 0) {

                    // try to find a _FUNCTION-FORWARD block with the name, as it surrounds the prototype if it exists
                    var protoRegex = new Regex(@"\s*_UIB-CODE-BLOCK\s+" + typeStr + @"\s+" + functionName + @"\s", RegexOptions.IgnoreCase);
                    protoPreProcBlock = ParserHandler.AblParser.ParsedItemsList.Where(item => {
                        var blockItem = item as ParsedPreProcBlock;
                        if (blockItem != null && protoRegex.Match(blockItem.BlockDescription).Success)
                            return true;
                        return false;
                    }).ToList();
                }

                foreach (var item in protoPreProcBlock.Select(item => (ParsedPreProcBlock)item)) {
                    if (item.Position < surroundedPosition && surroundedPosition < item.EndBlockPosition)
                        return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Surround the text to insert with the appbuilder directives if needed
        /// </summary>
        private static string FormatInsertion(string insertText, string blockDescription, bool insertBefore) {
            var eol = Npp.GetEolString;
            if (!String.IsNullOrEmpty(blockDescription) && Abl.IsCurrentFileFromAppBuilder) {
                insertText = @"&ANALYZE-SUSPEND _UIB-CODE-BLOCK " + blockDescription + eol + insertText;
                insertText += eol + eol + @"/* _UIB-CODE-BLOCK-END */" + eol + @"&ANALYZE-RESUME";
            }
            if (insertBefore) insertText += eol + eol;
            else insertText = eol + eol + insertText;
            return insertText;
        }

        #endregion

        #region Insert new

        /// <summary>
        /// Call this method to insert a new piece of code
        /// </summary>
        public static void InsertNew<T>() where T : ParsedScopeItem {
            IProNew newCode;
            string insertText;
            string blockDescription;

            if (typeof(ParsedFunction) == typeof(T)) {
                object input = new ProNewFunction();
                if (UserCommunication.Input(ref input, "Please provide information about the procedure that will be created", MessageImg.MsgQuestion, "Generate code", "Insert a new function") != 0)
                    return;
                newCode = (IProNew) input;

                blockDescription = @"_FUNCTION " + newCode.Name + " Procedure";
                insertText = Encoding.Default.GetString(DataResources.FunctionImplementation).Trim();
                insertText = insertText.Replace("{&type}", ((ProNewFunction)newCode).Type.GetDescription());
                insertText = insertText.Replace("{&private}", ((ProNewFunction)newCode).IsPrivate ? " PRIVATE" : "");

            } else if (typeof(ParsedProcedure) == typeof(T)) {
                object input = new ProNewProcedure();
                if (UserCommunication.Input(ref input, "Please provide information about the procedure that will be created", MessageImg.MsgQuestion, "Generate code", "Insert a new procedure") != 0)
                    return;
                newCode = (IProNew)input;

                blockDescription = @"_PROCEDURE " + newCode.Name + " Procedure";
                insertText = Encoding.Default.GetString(DataResources.InternalProcedure).Trim();
                insertText = insertText.Replace("{&private}", ((ProNewProcedure)newCode).IsPrivate ? " PRIVATE" : "");

            } else {
                return;
            }

            if (string.IsNullOrEmpty(newCode.Name))
                return;

            // make sure to parse the current document before checking anything
            ParserHandler.ParseCurrentDocument(true, true);

            insertText = insertText.Replace("{&name}", newCode.Name);

            // reposition caret and insert
            bool insertBefore;
            int insertPos = GetCaretPositionForInsertion<T>(newCode, out insertBefore);
            if (insertPos < 0) insertPos = Npp.GetPosFromLineColumn(Npp.Line.CurrentLine, 0);

            insertText = FormatInsertion(insertText, blockDescription, insertBefore);
            int internalCaretPos = insertText.IndexOf("|||", StringComparison.Ordinal);
            insertText = insertText.Replace("|||", "");

            Npp.SetSelection(insertPos);
            Npp.ModifyTextAroundCaret(0, 0, insertText);

            Npp.GoToLine(Npp.LineFromPosition(insertPos));
            Npp.GotoPosition(insertPos + (internalCaretPos > 0 ? internalCaretPos : 0));

            // in the case of a new function, create the prototype if needed
            if (typeof(ParsedFunction) == typeof(T)) {
                UpdateFunctionPrototypesIfNeeded(true);
            }
        }

        internal enum ProInsertNewType {
            Procedure,
            Function
        }


        internal interface IProNew {
            string Name { get; set; }
            ProInsertPosition InsertPosition { get; set; }
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

        internal class ProNewPrototype : IProNew {
            public string Name { get; set; }
            public ProInsertPosition InsertPosition { get; set; }
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
