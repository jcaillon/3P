#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamuiFramework.Forms;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Pro {
    internal class ProGenerateCode {
        #region Factory

        /// <summary>
        /// Get a new instance of ProGenerateCode (parses the current document in the constructor
        /// </summary>
        public static ProGenerateCode Factory {
            get { return new ProGenerateCode(); }
        }

        #endregion

        #region Private

        private HashSet<string> _ignoredFiles = new HashSet<string>();

        private Parser.Parser _parser;

        private List<ParsedItem> _parsedItems;

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public ProGenerateCode() {
            ParseNow();
        }

        #endregion

        #region Public

        /// <summary>
        /// Call this method to insert a new piece of code
        /// </summary>
        public void InsertCode<T>() where T : ParsedScopeItem {
            IProCode codeCode;
            string insertText;
            string blockDescription;

            // in case of an incorrect document, warn the user
            var parserErrors = _parser.ParseErrorsInHtml;
            if (!string.IsNullOrEmpty(parserErrors)) {
                if (UserCommunication.Message("The internal parser of 3P has found inconsistencies in your document :<br>" + parserErrors + "<br>You can still insert a new piece of code but the insertion position might not be calculated correctly; take caution of what is generated if you decide to go through with it.", MessageImg.MsgQuestion, "Generate code", "Problems spotted", new List<string> {"Continue", "Abort"}) != 0)
                    return;
            }

            if (typeof(ParsedImplementation) == typeof(T)) {
                object input = new ProCodeFunction();
                if (UserCommunication.Input(ref input, "Please provide information about the procedure that will be created", MessageImg.MsgQuestion, "Generate code", "Insert a new function") != 0)
                    return;
                codeCode = (IProCode) input;

                codeCode.Name = codeCode.Name.MakeValidVariableName();

                blockDescription = @"_FUNCTION " + codeCode.Name + " Procedure";
                insertText = Encoding.Default.GetString(DataResources.FunctionImplementation).Trim();
                insertText = insertText.Replace("{&type}", ((ProCodeFunction) codeCode).Type);
                insertText = insertText.Replace("{&private}", ((ProCodeFunction) codeCode).IsPrivate ? " PRIVATE" : "");
            } else if (typeof(ParsedProcedure) == typeof(T)) {
                object input = new ProCodeProcedure();
                if (UserCommunication.Input(ref input, "Please provide information about the procedure that will be created", MessageImg.MsgQuestion, "Generate code", "Insert a new procedure") != 0)
                    return;
                codeCode = (IProCode) input;

                blockDescription = @"_PROCEDURE " + codeCode.Name + " Procedure";
                insertText = Encoding.Default.GetString(DataResources.InternalProcedure).Trim();
                insertText = insertText.Replace("{&private}", ((ProCodeProcedure) codeCode).IsPrivate ? " PRIVATE" : "");
            } else {
                return;
            }

            if (string.IsNullOrEmpty(codeCode.Name))
                return;

            // check if the code already exists
            if (_parsedItems.Exists(item => item.GetType() == typeof(T) && item.Name.EqualsCi(codeCode.Name))) {
                UserCommunication.Notify("Sorry, this name is already taken by another existing instance", MessageImg.MsgHighImportance, "Invalid name", "Existing name", 5);
                return;
            }

            insertText = insertText.Replace("{&name}", codeCode.Name);

            // reposition caret and insert
            bool insertBefore;
            int insertPos = GetCaretPositionForInsertion<T>(codeCode.Name, codeCode.InsertPosition, out insertBefore);
            if (insertPos < 0) insertPos = Sci.GetPosFromLineColumn(Sci.Line.CurrentLine, 0);

            insertText = FormatInsertion(insertText, blockDescription, insertBefore);
            int internalCaretPos = insertText.IndexOf("|||", StringComparison.Ordinal);
            insertText = insertText.Replace("|||", "");

            Sci.SetSelection(insertPos);
            Sci.ModifyTextAroundCaret(0, 0, insertText);

            Sci.GoToLine(Sci.LineFromPosition(insertPos));
            Sci.GotoPosition(insertPos + (internalCaretPos > 0 ? internalCaretPos : 0));

            // in the case of a new function, update the prototype if needed
            if (typeof(ParsedImplementation) == typeof(T)) {
                ParseNow();
                UpdateFunctionPrototypes(true);
            }
        }

        public void DeleteCode<T>() where T : ParsedScopeItem {
            // make a list of existing items for this type
            var existingList = _parsedItems.Where(item => item.GetType() == typeof(T)).Cast<T>().ToList();

            object nameToDelete = new ProCodeDelete {Value = string.Join("|", existingList.Select(arg => arg.Name))};

            if (string.IsNullOrEmpty(((ProCodeDelete) nameToDelete).Value)) {
                UserCommunication.Notify("Sorry, there was nothing to do!", MessageImg.MsgInfo, "Delete code", "Nothing to delete!", 5);
                return;
            }

            if (UserCommunication.Input(ref nameToDelete, "Please select which piece of code should be deleted", MessageImg.MsgQuestion, "Delete code", "Select the item to delete") != 0)
                return;

            var delete = (ProCodeDelete) nameToDelete;

            if (string.IsNullOrEmpty(delete.Value))
                return;

            var toDelete = existingList.FirstOrDefault(item => item.Name.Equals(delete.Value));
            if (toDelete != null)
                DeleteCode(toDelete);

            // in the case of a new function, update the prototype if needed
            if (typeof(ParsedImplementation) == typeof(T)) {
                ParseNow();
                UpdateFunctionPrototypes(true);
            }
        }

        /// <summary>
        /// This method checks if the current document contains function prototypes that are not updated
        /// and correct them if needed
        /// </summary>
        /// <remarks>This method is costly because we parse everything potentially X times, but it's much simpler this way...</remarks>
        public void UpdateFunctionPrototypesIfNeeded(bool silent = false) {
            if (_ignoredFiles.Contains(Npp.CurrentFile.Path) || Config.Instance.DisablePrototypeAutoUpdate) {
                if (silent)
                    return;
                _ignoredFiles.Remove(Npp.CurrentFile.Path);
            }

            UpdateFunctionPrototypes(silent);
        }

        #endregion

        #region Update/delete/add function prototype

        /// <summary>
        /// This method checks if the current document contains function prototypes that are not updated
        /// and correct them if needed
        /// </summary>
        /// <remarks>This method is costly because we parse everything potentially X times, but it's much simpler this way...</remarks>
        private void UpdateFunctionPrototypes(bool silent) {

            try {
                List<ParsedImplementation> listOfOutDatedProto;
                List<ParsedImplementation> listOfSoloImplementation;
                List<ParsedPrototype> listOfUselessProto;

                StringBuilder outputMessage = new StringBuilder();

                var nbLoop = 0;
                var nbNotCreated = 0;
                var nbThingsDone = 0;
                var nbToDo = GetPrototypesLists(out listOfOutDatedProto, out listOfSoloImplementation, out listOfUselessProto);

                // if there is at least 1 thing to do
                if (nbToDo > 0) {
                    Sci.BeginUndoAction();

                    // Add proto
                    if (listOfSoloImplementation.Count > 0 && string.IsNullOrEmpty(_parser.ParseErrorsInHtml)) {
                        var tempMes = new StringBuilder("The following function prototypes have been created :");

                        while (listOfSoloImplementation.Count > nbNotCreated && nbLoop < nbToDo) {
                            if (AddPrototypes(ref tempMes, listOfSoloImplementation[nbNotCreated]))
                                nbThingsDone++;
                            else
                                nbNotCreated++;

                            ParseNow();
                            GetPrototypesLists(out listOfOutDatedProto, out listOfSoloImplementation, out listOfUselessProto);
                            nbLoop++;
                        }
                        tempMes.Append("<br><br>");
                        if (nbThingsDone > 0)
                            outputMessage.Append(tempMes);
                    }

                    // delete proto
                    if (listOfUselessProto.Count > 0) {
                        outputMessage.Append("The following prototypes have been deleted :");
                        while (listOfUselessProto.Count > 0 && nbLoop < nbToDo) {
                            if (DeletePrototypes(ref outputMessage, listOfUselessProto[0]))
                                nbThingsDone++;

                            ParseNow();
                            GetPrototypesLists(out listOfOutDatedProto, out listOfSoloImplementation, out listOfUselessProto);
                            nbLoop++;
                        }
                        outputMessage.Append("<br><br>");
                    }

                    // update proto
                    if (listOfOutDatedProto.Count > 0) {
                        outputMessage.Append("The following functions have had their prototype synchronized :");
                        while (listOfOutDatedProto.Count > 0 && nbLoop < nbToDo) {
                            if (UpdatePrototypes(ref outputMessage, listOfOutDatedProto[0]))
                                nbThingsDone++;

                            ParseNow();
                            GetPrototypesLists(out listOfOutDatedProto, out listOfSoloImplementation, out listOfUselessProto);
                            nbLoop++;
                        }
                        outputMessage.Append("<br><br>");
                    }

                    Sci.EndUndoAction();
                }

                if (nbThingsDone == 0) {
                    if (!silent) {
                        if (nbToDo == 0)
                            UserCommunication.Notify("There was nothing to be done :<br>All the prototypes match their implementation", MessageImg.MsgInfo, "Function prototypes", "Everything is synchronized", 5);
                        else
                            UserCommunication.Notify("Failed to find the prototype for " + nbNotCreated + " function implementations<br>Your document is not correctly formatted for 3P to automatically create them :<br><i>The block _UIB-PREPROCESSOR-BLOCK is missing or the procedure can't be opened in the appbuilder!</i><br><br>Please correct your document manually, then they will all be updated correctly" + _parser.ParseErrorsInHtml, MessageImg.MsgHighImportance, "Function prototypes", "Failed to create prototypes");
                    }
                } else {
                    outputMessage.Append("<i>");
                    outputMessage.Append("CTRL + Z will cancel the above-mentioned modifications<br>");
                    outputMessage.Append(Npp.CurrentFile.Path.ToHtmlLink("Click here to stop auto-updating the prototypes for this file"));
                    outputMessage.Append("</i>");
                    UserCommunication.NotifyUnique("Prototype_synchro", outputMessage.ToString(), MessageImg.MsgOk, "Function prototypes", "Synchronization done", args => {
                        var split = args.Link.Split('#');
                        if (split.Length == 2) {
                            Npp.GotoPos(split[0], int.Parse(split[1]));
                            args.Handled = true;
                        } else {
                            if (!_ignoredFiles.Contains(args.Link)) {
                                _ignoredFiles.Add(args.Link);
                                UserCommunication.NotifyUnique("Prototype_synchro", "Automatic prototype updates stopped for the file :<br>" + Npp.CurrentFile.Path + "<br><br><i>This is effective until you restart Notepad++<br>You can also trigger an update manually to restart the auto-update</i>", MessageImg.MsgInfo, "Function prototypes", "Synchronization stopped", null, 5);
                                args.Handled = true;
                            }
                        }
                    }, 5);
                }

            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error updating prototypes");
            }
        }

        /// <summary>
        /// Gets the list of functions/proto of interest
        /// </summary>
        private int GetPrototypesLists(out List<ParsedImplementation> listOfOutDatedProto, out List<ParsedImplementation> listOfSoloImplementation, out List<ParsedPrototype> listOfUselessProto) {

            // list the outdated proto
            listOfOutDatedProto = _parsedItems.Where(item => {
                var funcItem = item as ParsedImplementation;
                return funcItem != null && funcItem.HasPrototype && !funcItem.PrototypeUpdated;
            }).Select(item => (ParsedImplementation) item).ToList();

            // list the implementation w/o prototypes
            listOfSoloImplementation = _parsedItems.Where(item => {
                var funcItem = item as ParsedImplementation;
                return funcItem != null && !funcItem.HasPrototype;
            }).Select(item => (ParsedImplementation) item).ToList();

            // list the prototypes w/o implementation
            listOfUselessProto = _parsedItems.Where(item => {
                // it's a prototype with no implementation
                var proto = item as ParsedPrototype;
                return proto != null && proto.SimpleForward && !_parsedItems.Exists(func => func is ParsedImplementation && func.Name.EqualsCi(item.Name));
            }).Select(item => (ParsedPrototype) item).ToList();

            return listOfOutDatedProto.Count + listOfSoloImplementation.Count + listOfUselessProto.Count;
        }

        /// <summary>
        /// This method checks if the current document contains function prototypes that are not updated
        /// and correct them if needed
        /// </summary>
        private bool UpdatePrototypes(ref StringBuilder outputMessage, ParsedImplementation function) {
            var protoStr = Sci.GetTextByRange(function.Position, function.EndPosition);

            // replace the end ":" or "." by a " FOWARD."
            protoStr = protoStr.Substring(0, protoStr.Length - 1).TrimEnd(' ') + " FORWARD.";
            Sci.SetTextByRange(function.PrototypePosition, function.PrototypeEndPosition, protoStr);

            outputMessage.Append("<br> - <a href='" + function.FilePath + "#" + (function.PrototypePosition) + "'>" + function.Name + "</a>");

            return true;
        }

        private bool AddPrototypes(ref StringBuilder outputMessage, ParsedImplementation function) {
            var protoStr = Sci.GetTextByRange(function.Position, function.EndPosition);

            // get the best position to insert the prototype
            bool insertBefore;
            int insertPos = GetCaretPositionForInsertion<ParsedPrototype>(function.Name, ProInsertPosition.Last, out insertBefore);

            // if we didn't find a good position, then let's assume the user doesn't need one
            if (insertPos > 0 && protoStr.Length > 1) {
                // replace the end ":" or "." by a " FOWARD."
                protoStr = FormatInsertion(protoStr.Substring(0, protoStr.Length - 1).TrimEnd(' ') + " FORWARD.", "_FUNCTION-FORWARD " + function.Name + " Procedure", insertBefore);

                Sci.SetTextByRange(insertPos, insertPos, protoStr);

                //outputMessage.Append("<br> - <a href='" + function.FilePath + "#" + insertPos + "'>" + function.Name + "</a>");
                outputMessage.Append("<br> - " + function.Name);

                return true;
            }

            return false;
        }

        private bool DeletePrototypes(ref StringBuilder outputMessage, ParsedPrototype function) {
            DeleteCode(function);

            outputMessage.Append("<br> - " + function.Name);

            return true;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// returns the surrounding IF DEFINED or _UIB-CODE-BLOCK of a function, procedure.. if it exists
        /// otherwise returns null
        /// </summary>
        private ParsedPreProcBlock GetPreProcBlock<T>(T parsedScopeItem, string typeStr) where T : ParsedScopeItem {
            // try to find a &IF DEFINED(EXCLUDE- block that surrounds the prototype
            var protoPreProcBlock = _parsedItems.Where(item => {
                var blockItem = item as ParsedPreProcBlock;
                if (blockItem != null && blockItem.Type == ParsedPreProcBlockType.IfEndIf &&
                    blockItem.BlockDescription.ContainsFast(@"DEFINED(EXCLUDE-" + parsedScopeItem.Name + @")"))
                    return true;
                return false;
            }).ToList();

            // if we found a block that actually surrounds our parsedScopeItem then that's it
            foreach (var item in protoPreProcBlock.Select(item => (ParsedPreProcBlock) item)) {
                if (item.Position < parsedScopeItem.Position && parsedScopeItem.Position < item.EndBlockPosition)
                    return item;
            }

            // try to find a _FUNCTION-FORWARD block with the name, as it surrounds the prototype if it exists
            var protoRegex = new Regex(@"\s*_UIB-CODE-BLOCK\s+" + typeStr + @"\s+" + parsedScopeItem.Name + @"\s", RegexOptions.IgnoreCase);
            protoPreProcBlock = _parsedItems.Where(item => {
                var blockItem = item as ParsedPreProcBlock;
                if (blockItem != null && protoRegex.Match(blockItem.BlockDescription).Success)
                    return true;
                return false;
            }).ToList();

            // if we found a block that actually surrounds our parsedScopeItem then that's it
            foreach (var item in protoPreProcBlock.Select(item => (ParsedPreProcBlock) item)) {
                if (item.Position < parsedScopeItem.Position && parsedScopeItem.Position < item.EndBlockPosition)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Surround the text to insert with the appbuilder directives if needed
        /// </summary>
        private string FormatInsertion(string insertText, string blockDescription, bool insertBefore) {
            var eol = Sci.GetEolString;
            if (!String.IsNullOrEmpty(blockDescription) && ProCodeFormat.IsCurrentFileFromAppBuilder) {
                insertText = @"&ANALYZE-SUSPEND _UIB-CODE-BLOCK " + blockDescription + eol + insertText;
                insertText += eol + eol + @"/* _UIB-CODE-BLOCK-END */" + eol + @"&ANALYZE-RESUME";
            }
            if (insertBefore) insertText += eol + eol;
            else insertText = eol + eol + insertText;
            return insertText;
        }

        /// <summary>
        /// Delete the given ParsedScopeItem whose name is qualified through proCode.Name
        /// </summary>
        private void DeleteCode<T>(T toDelete) where T : ParsedScopeItem {
            string preProcBlockType = null;
            if (typeof(ParsedImplementation) == typeof(T)) {
                preProcBlockType = @"_FUNCTION";
            } else if (typeof(ParsedPrototype) == typeof(T)) {
                preProcBlockType = @"_FUNCTION-FORWARD";
            } else if (typeof(ParsedProcedure) == typeof(T)) {
                preProcBlockType = @"_PROCEDURE";
            }

            // find a pre proc block that surrounds it
            var protoPreProcBlock = GetPreProcBlock(toDelete, preProcBlockType);

            // we also want to delete the trailing new lines
            int endPosition = (protoPreProcBlock != null ? protoPreProcBlock.EndBlockPosition : toDelete.EndBlockPosition);
            while (Sci.GetTextByRange(endPosition, endPosition + 2).Equals(Sci.GetEolString)) {
                endPosition += 2;
            }

            if (protoPreProcBlock != null) {
                Sci.DeleteTextByRange(protoPreProcBlock.Position, endPosition);
            } else {
                // if not found, we just delete the proto statement
                Sci.DeleteTextByRange(toDelete.Position, endPosition);
            }
        }

        /// <summary>
        /// returns the best caret position for inserting a new IProNew
        /// </summary>
        private int GetCaretPositionForInsertion<T>(string codeName, ProInsertPosition insertPos, out bool insertBefore) where T : ParsedScopeItem {
            insertBefore = false;

            // at caret position
            if (insertPos == ProInsertPosition.CaretPosition)
                return Sci.GetPosFromLineColumn(Sci.Line.CurrentLine, 0);

            T refItem = null;

            #region set insertBefore and refItem

            // the following is a little annoying to code and understand...
            // the idea is to get (or dont get if it doesn't exist) the previous or the next item
            // of type T in the existing list of said types so we can "anchor" on it to insert
            // our new stuff...
            if (typeof(ParsedPrototype) == typeof(T)) {
                // find the previous/next function implementation with a prototype
                bool found = false;
                ParsedImplementation foundImplement = null;
                foreach (var impl in _parsedItems.Where(item => item is ParsedImplementation).Cast<ParsedImplementation>()) {
                    if (impl != null) {
                        // we didn't match our current function implementation yet
                        if (!found) {
                            // we just did
                            if (impl.Name.Equals(codeName)) {
                                found = true;
                                continue;
                            }
                            // set previous item
                            if (impl.HasPrototype)
                                foundImplement = impl;
                        } else {
                            // match first item after we found our implementation
                            if (impl.HasPrototype) {
                                insertBefore = true;
                                foundImplement = impl;
                                break;
                            }
                        }
                    }
                }

                // now we need its proto
                if (foundImplement != null) {
                    refItem = _parsedItems.FirstOrDefault(fun => {
                        var proto = fun as ParsedPrototype;
                        return proto != null && proto.Name.Equals(foundImplement.Name) && proto.SimpleForward;
                    }) as T;
                }
            } else {
                // list of existing items of the same type
                var existingList = _parsedItems.Where(item => item.GetType() == typeof(T)).Select(item => (T) item).ToList();
                if (existingList.Count > 0) {
                    // alphabetical order
                    if (insertPos == ProInsertPosition.AlphabeticalOrder) {
                        // find the position that would take our new code
                        int index = existingList.Select(item => item.Name).ToList().BinarySearch(codeName);
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
                    } else if (insertPos == ProInsertPosition.First) {
                        refItem = existingList.FirstOrDefault();
                        insertBefore = true;
                    } else if (insertPos == ProInsertPosition.Last) {
                        refItem = existingList.LastOrDefault();
                    }
                }
            }

            #endregion

            string preProcBlockType = null;
            string typeComment = null;
            if (typeof(ParsedImplementation) == typeof(T)) {
                preProcBlockType = @"_FUNCTION";
                typeComment = @"Function\s+Implementations";
            } else if (typeof(ParsedPrototype) == typeof(T)) {
                preProcBlockType = @"_FUNCTION-FORWARD";
                typeComment = @"Function\s+Prototypes";
            } else if (typeof(ParsedProcedure) == typeof(T)) {
                preProcBlockType = @"_PROCEDURE";
                typeComment = @"Internal\s+Procedures";
            }

            // is there already an item existing?
            if (refItem != null && preProcBlockType != null) {
                // try to find a &IF DEFINED(EXCLUDE- block or a _UIB_BLOCK that surrounds the prototype
                var preProcBlock = GetPreProcBlock(refItem, preProcBlockType);
                if (preProcBlock != null)
                    return (insertBefore ? preProcBlock.Position : preProcBlock.EndBlockPosition);

                // otherwise return the position of the function itself
                return insertBefore ? refItem.Position : refItem.EndBlockPosition;
            }

            // can we find a comment indicating where the proc should be inserted?
            if (typeComment != null) {
                Sci.TargetWholeDocument();
                var previousFlags = Sci.SearchFlags;
                Sci.SearchFlags = SearchFlags.Regex;
                var streg = @"\/\*\s+[\*]+\s+" + typeComment + @"\s+[\*]+";
                var foundPos = Sci.SearchInTarget(streg);
                Sci.SearchFlags = previousFlags;
                if (foundPos == -1) {
                    foundPos = new Regex(@"\/\*\s+[\*]+\s+" + typeComment + @"\s+[\*]+").Match(Sci.Text).Index;
                    if (foundPos == 0) foundPos = -1;
                }
                if (foundPos > -1) {
                    return Sci.GetPosFromLineColumn(Sci.LineFromPosition(foundPos) + 1, 0);
                }
            }

            // At last, we find the best position considering the appbuilder blocks

            if (typeof(ParsedImplementation) == typeof(T)) {
                // function implementation goes all the way bottom
                return Sci.TextLength;
            }
            if (typeof(ParsedPrototype) == typeof(T)) {
                // prototypes go after &ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 
                var preprocessorBlock = _parsedItems.FirstOrDefault(item => item is ParsedPreProcBlock && ((ParsedPreProcBlock) item).Type == ParsedPreProcBlockType.UibPreprocessorBlock);
                if (preprocessorBlock != null) {
                    insertBefore = false;
                    return ((ParsedPreProcBlock) preprocessorBlock).EndBlockPosition;
                }
            }
            if (typeof(ParsedProcedure) == typeof(T)) {
                // new procedure goes before the first function implementation of last
                var firstFunc = _parsedItems.FirstOrDefault(item => item is ParsedImplementation) as ParsedImplementation;
                if (firstFunc != null) {
                    insertBefore = true;

                    // try to find a &IF DEFINED(EXCLUDE- block that surrounds the func
                    var preProcBlock = GetPreProcBlock(firstFunc, @"_FUNCTION");
                    if (preProcBlock != null)
                        return preProcBlock.Position;

                    return firstFunc.Position;
                }
                // otherwise it goes at the end
                return Sci.TextLength;
            }

            return -1;
        }

        /// <summary>
        /// Parse the current document
        /// </summary>
        private void ParseNow() {
            _parser = new Parser.Parser(Sci.Text, Npp.CurrentFile.Path, null, false);
            _parsedItems = _parser.ParsedItemsList.Where(item => !item.Flags.HasFlag(ParseFlag.FromInclude)).ToNonNullList();
        }

        #endregion

        #region Pro code class

        internal class ProCodeDelete {
            [YamuiInput("Selection", AllowListedValuesOnly = true)]
            public string Value = "";
        }

        internal interface IProCode {
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

        internal class ProCodePrototype : IProCode {
            public string Name { get; set; }
            public ProInsertPosition InsertPosition { get; set; }
        }

        internal class ProCodeProcedure : IProCode {
            [YamuiInput("Name", Order = 0)]
            public string Name { get; set; }

            [YamuiInput("Private procedure", Order = 1)]
            public bool IsPrivate { get; set; }

            [YamuiInput("Insertion position", Order = 2)]
            public ProInsertPosition InsertPosition { get; set; }
        }

        internal class ProCodeFunction : IProCode {
            [YamuiInput("Name", Order = 0)]
            public string Name { get; set; }

            [YamuiInput("Return type", Order = 1, AllowListedValuesOnly = true)]
            public string Type = "CHARACTER|HANDLE|INTEGER|LOGICAL|COM-HANDLE|DECIMAL|DATE|DATETIME|DATETIME-TZ|INT64|LONGCHAR|MEMPTR|RAW|RECID|ROWID|WIDGET-HANDLE|CLASS XXX";

            [YamuiInput("Private function", Order = 2)]
            public bool IsPrivate { get; set; }

            [YamuiInput("Insertion position", Order = 3)]
            public ProInsertPosition InsertPosition { get; set; }
        }

        #endregion
    }
}