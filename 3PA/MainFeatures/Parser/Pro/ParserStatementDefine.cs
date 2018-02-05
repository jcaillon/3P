using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {

        /// <summary>
        /// Matches a new definition
        /// </summary>
        private void CreateParsedDefine(Token defineToken, bool isDynamic) {
            /*
            all DEFINE and CREATE statement
            */


            // info we will extract from the current statement :
            string name = "";
            ParseFlag flags = isDynamic ? ParseFlag.Dynamic : 0;
            ParsedAsLike asLike = ParsedAsLike.None;
            ParseDefineType type = ParseDefineType.None;
            string tempPrimitiveType = "";
            string viewAs = "";
            string bufferFor = "";
            _lastTokenWasSpace = true;
            StringBuilder left = new StringBuilder();

            // for temp tables:
            string likeTable = "";
            bool isTempTable = false;
            var fields = new List<ParsedField>();
            ParsedField currentField = new ParsedField("", "", "", 0, 0, "", "", ParsedAsLike.None);
            StringBuilder useIndex = new StringBuilder();

            // for tt indexes
            var indexList = new List<ParsedIndex>();
            string indexName = "";
            var indexFields = new List<string>();
            ParsedIndexFlag indexFlags = ParsedIndexFlag.None;
            var indexSort = "+"; // + for ascending, - for descending

            Token token;
            int state = 0;
            do {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                string lowerToken;
                bool matchedLikeTable = false;
                switch (state) {
                    case 0:
                        // matching until type of define is found
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        switch (lowerToken) {
                            case "buffer":
                            case "browse":
                            case "stream":
                            case "button":
                            case "dataset":
                            case "frame":
                            case "query":
                            case "event":
                            case "image":
                            case "menu":
                            case "rectangle":
                            case "property":
                            case "sub-menu":
                            case "parameter":
                                if (!Enum.TryParse(lowerToken, true, out type))
                                    type = ParseDefineType.None;
                                state++;
                                break;
                            case "data-source":
                                type = ParseDefineType.DataSource;
                                state++;
                                break;
                            case "var":
                            case "variable":
                                type = ParseDefineType.Variable;
                                state++;
                                break;
                            case "temp-table":
                            case "work-table":
                            case "workfile":
                                isTempTable = true;
                                state++;
                                break;
                            case "new":
                                flags |= ParseFlag.New;
                                break;
                            case "global":
                                flags |= ParseFlag.Global;
                                break;
                            case "shared":
                                flags |= ParseFlag.Shared;
                                break;
                            case "private":
                                flags |= ParseFlag.Private;
                                break;
                            case "protected":
                                flags |= ParseFlag.Protected;
                                break;
                            case "public":
                                flags |= ParseFlag.Public;
                                break;
                            case "static":
                                flags |= ParseFlag.Static;
                                break;
                            case "abstract":
                                flags |= ParseFlag.Abstract;
                                break;
                            case "override":
                                flags |= ParseFlag.Override;
                                break;
                            case "serializable":
                                flags |= ParseFlag.Serializable;
                                break;
                            case "input":
                                flags |= ParseFlag.Input;
                                break;
                            case "return":
                                flags |= ParseFlag.Return;
                                break;
                            case "output":
                                flags |= ParseFlag.Output;
                                break;
                            case "input-output":
                                flags |= ParseFlag.InputOutput;
                                break;
                            /*default:
                                ParseFlag parsedFlag;
                                if (Enum.TryParse(lowerToken, true, out parsedFlag))
                                    flags |= parsedFlag;
                                break;*/
                        }
                        break;

                    case 1:
                        // matching the name
                        if (!(token is TokenWord)) break;
                        name = token.Value;
                        if (type == ParseDefineType.Variable) 
                            state = 10;
                        if (type == ParseDefineType.Buffer) {
                            tempPrimitiveType = "buffer";
                            state = 81;
                        }
                        if (type == ParseDefineType.Parameter) {
                            lowerToken = token.Value.ToLower();
                            switch (lowerToken) {
                                case "buffer":
                                    tempPrimitiveType = lowerToken;
                                    type = ParseDefineType.Buffer;
                                    flags |= ParseFlag.Parameter;
                                    state = 80;
                                    break;
                                case "table":
                                case "table-handle":
                                case "dataset":
                                case "dataset-handle":
                                    tempPrimitiveType = lowerToken;
                                    state = 80;
                                    break;
                                default:
                                    state = 10;
                                    break;
                            }
                        }
                        if (isTempTable) 
                            state = 20;
                        if (state != 1) 
                            break;
                        state = 99;
                        break;

                    case 10:
                        // define variable : match as or like
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("as")) asLike = ParsedAsLike.As;
                        else if (lowerToken.Equals("like")) asLike = ParsedAsLike.Like;
                        if (asLike != ParsedAsLike.None) state = 11;
                        break;
                    case 11:
                        // define variable : match a primitive type or a field in db
                        if (!(token is TokenWord)) break;
                        tempPrimitiveType = token.Value;
                        state = 12;
                        break;
                    case 12:
                        // define variable : match a view-as (or extent)
                        AddTokenToStringBuilder(left, token);
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("view-as")) state = 13;
                        if (lowerToken.Equals("extent"))
                            flags |= ParseFlag.Extent;
                        break;
                    case 13:
                        // define variable : match a view-as
                        AddTokenToStringBuilder(left, token);
                        if (!(token is TokenWord)) break;
                        viewAs = token.Value;
                        state = 99;
                        break;

                    case 20:
                        // define temp-table
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        switch (lowerToken) {
                            case "field":
                                // matches FIELD
                                state = 22;
                                break;
                            case "index":
                                // matches INDEX
                                state = 25;
                                break;
                            case "use-index":
                                // matches USE-INDEX (after a like/like-sequential, we can have this keyword)
                                state = 26;
                                break;
                            case "help":
                                // a field has a help text:
                                state = 27;
                                break;
                            case "initial":
                                // a field has an initial value
                                state = 29;
                                break;
                            case "format":
                                // a field has a format
                                state = 30;
                                break;
                            case "extent":
                                // a field is extent:
                                currentField.Flags = currentField.Flags | ParseFlag.Extent;
                                break;
                            default:
                                // matches a LIKE table
                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse, resharper doesn't get this one
                                if ((lowerToken.Equals("like") || lowerToken.Equals("like-sequential")) && !matchedLikeTable)
                                    state = 21;
                                // After a USE-UNDEX and the index name, we can match a AS PRIMARY for the previously defined index
                                if (lowerToken.Equals("primary") && useIndex.Length > 0)
                                    useIndex.Append("!");
                                break;
                        }
                        break;
                    case 21:
                        // define temp-table : match a LIKE table, get the table name in asLike
                        // ReSharper disable once RedundantAssignment
                        matchedLikeTable = true;
                        if (!(token is TokenWord)) break;
                        likeTable = token.Value.ToLower();
                        state = 20;
                        break;

                    case 22:
                        // define temp-table : matches a FIELD name
                        if (!(token is TokenWord)) break;
                        currentField = new ParsedField(token.Value, "", "", 0, 0, "", "", ParsedAsLike.None);
                        state = 23;
                        break;
                    case 23:
                        // define temp-table : matches a FIELD AS or LIKE
                        if (!(token is TokenWord)) break;
                        currentField.AsLike = token.Value.EqualsCi("like") ? ParsedAsLike.Like : ParsedAsLike.As;
                        state = 24;
                        break;
                    case 24:
                        // define temp-table : match a primitive type or a field in db
                        if (!(token is TokenWord)) break;
                        currentField.TempType = token.Value;
                        currentField.Type = ConvertStringToParsedPrimitiveType(token.Value, currentField.AsLike == ParsedAsLike.Like);
                        // push the field to the fields list
                        fields.Add(currentField);
                        state = 20;
                        break;

                    case 25:
                        // define temp-table : match an index name
                        if (!(token is TokenWord)) break;
                        indexName = token.Value;
                        state = 28;
                        break;

                    case 28:
                        // define temp-table : match the definition of the index
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("unique")) {
                            indexFlags = indexFlags | ParsedIndexFlag.Unique;
                        } else if (lowerToken.Equals("primary")) {
                            indexFlags = indexFlags | ParsedIndexFlag.Primary;
                        } else if (lowerToken.Equals("word-index")) {
                            indexFlags = indexFlags | ParsedIndexFlag.WordIndex;
                        } else if (lowerToken.Equals("ascending")) {
                            // match a sort order for a field
                            indexSort = "+";
                            var lastField = indexFields.LastOrDefault();
                            if (lastField != null) {
                                indexFields.RemoveAt(indexFields.Count - 1);
                                indexFields.Add(lastField.Replace("-", "+"));
                            }
                        } else if (lowerToken.Equals("descending")) {
                            // match a sort order for a field
                            indexSort = "-";
                            var lastField = indexFields.LastOrDefault();
                            if (lastField != null) {
                                indexFields.RemoveAt(indexFields.Count - 1);
                                indexFields.Add(lastField.Replace("+", "-"));
                            }
                        } else if (lowerToken.Equals("index")) {
                            // matching a new index
                            if (!String.IsNullOrEmpty(indexName))
                                indexList.Add(new ParsedIndex(indexName, indexFlags, indexFields.ToList()));

                            indexName = "";
                            indexFields.Clear();
                            indexFlags = ParsedIndexFlag.None;
                            indexSort = "+";

                            state = 25;
                        } else {
                            // Otherwise, it's a field name
                            indexFields.Add(token.Value + indexSort);
                        }
                        break;

                    case 26:
                        // define temp-table : match a USE-INDEX name
                        if (!(token is TokenWord)) break;
                        useIndex.Append(",");
                        useIndex.Append(token.Value);
                        state = 20;
                        break;

                    case 27:
                        // define temp-table : match HELP for a field
                        if (!(token is TokenString)) break;
                        currentField.Description = GetTokenStrippedValue(token);
                        state = 20;
                        break;
                    case 29:
                        // define temp-table : match INITIAL for a field
                        if (!(token is TokenWhiteSpace)) {
                            currentField.InitialValue = GetTokenStrippedValue(token);
                            state = 20;
                        }
                        break;
                    case 30:
                        // define temp-table : match FORMAT for a field
                        if (!(token is TokenWhiteSpace)) {
                            currentField.Format = GetTokenStrippedValue(token);
                            state = 20;
                        }
                        break;

                    case 80:
                        // define parameter : match a temptable, table, dataset or buffer name
                        if (!(token is TokenWord)) break;
                        if (token.Value.ToLower().Equals("for")) break;
                        name = token.Value;
                        state++;
                        break;

                    case 81:
                        // match the table/dataset name that the buffer or handle is FOR
                        if (!(token is TokenWord)) break;
                        lowerToken = token.Value.ToLower();
                        if (lowerToken.Equals("for") || lowerToken.Equals("temp-table")) break;
                        bufferFor = lowerToken;
                        state = 99;
                        break;

                    case 99:
                        // matching the rest of the define
                        AddTokenToStringBuilder(left, token);
                        break;
                }
            } while (MoveNext());

            if (state <= 1)
                return;
            if (isTempTable) {

                if (!String.IsNullOrEmpty(indexName))
                    indexList.Add(new ParsedIndex(indexName, indexFlags, indexFields));

                ParsedTable likeTableMatch = null;
                if (!String.IsNullOrEmpty(likeTable)) {
                    likeTableMatch = FindAnyTableByName(likeTable);
                }

                var newTable = new ParsedTable(name, defineToken, "", "", name, "", likeTable, likeTableMatch, true, fields, indexList, new List<ParsedTrigger>(), useIndex.ToString()) {
                    // = end position of the EOS of the statement
                    EndPosition = token.EndPosition,
                    Flags = flags
                };

                // temp table is LIKE another table? copy fields
                if (newTable.LikeTable != null) {
                    // add the fields of the found table (minus the primary information)
                    foreach (var field in newTable.LikeTable.Fields) {
                        newTable.Fields.Add(
                            new ParsedField(field.Name, field.TempType, field.Format, field.Order, 0, field.InitialValue, field.Description, field.AsLike) {
                                Type = field.Type
                            });
                    }

                    // handles the use-index
                    if (!String.IsNullOrEmpty(newTable.UseIndex)) {
                        // add only the indexes that are used
                        foreach (var index in newTable.UseIndex.Split(',')) {
                            var foundIndex = newTable.LikeTable.Indexes.Find(index2 => index2.Name.EqualsCi(index.Replace("!", "")));
                            if (foundIndex != null) {
                                newTable.Indexes.Add(new ParsedIndex(foundIndex.Name, foundIndex.Flag, foundIndex.FieldsList.ToList()));
                                // if one of the index used is marked as primary
                                if (index.ContainsFast("!")) {
                                    newTable.Indexes.ForEach(parsedIndex => parsedIndex.Flag &= ~ParsedIndexFlag.Primary);
                                    newTable.Indexes.Last().Flag |= ParsedIndexFlag.Primary;
                                }
                            }
                        }
                    } else {
                        // if there is no "use index" and we didn't define new indexes, the tt uses the same index as the original table
                        if (newTable.Indexes == null || newTable.Indexes.Count == 0)
                            newTable.Indexes = newTable.LikeTable.Indexes.ToList();
                    }
                }

                // browse all the indexes and set the according flags to each field of the index
                foreach (var index in newTable.Indexes) {
                    foreach (var fieldName in index.FieldsList) {
                        var foundfield = newTable.Fields.Find(field => field.Name.EqualsCi(fieldName.Substring(0, fieldName.Length - 1)));
                        if (foundfield != null) {
                            if (index.Flag.HasFlag(ParsedIndexFlag.Primary))
                                foundfield.Flags |= ParseFlag.Primary;
                            foundfield.Flags |= ParseFlag.Index;
                        }
                    }
                }

                AddParsedItem(newTable, defineToken.OwnerNumber);

            } else {              
                
                var newDefine = NewParsedDefined(name, flags, defineToken, asLike, left.ToString(), type, tempPrimitiveType, viewAs, bufferFor);
                newDefine.EndPosition = token.EndPosition;
                AddParsedItem(newDefine, defineToken.OwnerNumber);

                // case of a parameters, add it to the current scope (if procedure)
                var currentScope =  GetCurrentBlock<ParsedScopeBlock>() as ParsedProcedure;
                if (type == ParseDefineType.Parameter && currentScope != null) {
                    if (currentScope.Parameters == null)
                        currentScope.Parameters = new List<ParsedDefine>();
                    currentScope.Parameters.Add(newDefine);
                }

            }
        }

    }
}