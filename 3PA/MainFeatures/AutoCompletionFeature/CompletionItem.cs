#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompletionItem.cs) is part of 3P.
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
using System.Drawing;
using System.Linq;
using System.Text;
using YamuiFramework.Controls.YamuiList;
using _3PA.Lib;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.AutoCompletionFeature {
    /// <summary>
    /// class used in the auto completion feature
    /// </summary>
    internal abstract class CompletionItem : FilteredTypeListItem {
        /// <summary>
        /// Type of completion
        /// </summary>
        public virtual CompletionType Type {
            get { return 0; }
        }

        /// <summary>
        /// Allows to display small "tag" picture on the left of a completionData in the auto comp list,
        /// see the ParseFlag enumeration for all the possibilities
        /// It works as a Flag, call HasFlag() method to if a certain flag is set and use
        /// Flag = Flag | ParseFlag.Reserved to set a flag!
        /// </summary>
        public virtual ParseFlag Flags { get; set; }

        /// <summary>
        /// Used for sorting the auto completion list, the higher the ranking, the higher in the list
        /// the item is
        /// </summary>
        public virtual int Ranking { get; set; }

        /// <summary>
        /// Indicates whether or not this completionData is created by the parser Visitor
        /// </summary>
        public virtual bool FromParser { get; set; }

        /// <summary>
        /// When the FromParser is true, contains the ParsedItem extracted by the parser
        /// </summary>
        public virtual ParsedBaseItem ParsedBaseItem { get; set; }

        /// <summary>
        /// Use this method to do an action for each flag of the item...
        /// </summary>
        /// <param name="toApplyOnFlag"></param>
        public virtual void DoForEachFlag(Action<string, ParseFlag> toApplyOnFlag) {
            typeof(ParseFlag).ForEach<ParseFlag>((s, l) => {
                if (l == 0 || !Flags.HasFlag((ParseFlag) l))
                    return;
                toApplyOnFlag(s, (ParseFlag) l);
            });
        }

        /// <summary>
        /// The piece of text displayed in the list
        /// </summary>
        public override string DisplayText { get; set; }

        /// <summary>
        /// return the image to display for this item
        /// If null, the image corresponding to ItemTypeImage will be used instead
        /// </summary>
        public override Image ItemImage {
            get { return null; }
        }

        /// <summary>
        /// return this item type (a unique int for each item type)
        /// if the value is strictly inferior to 0, the button for this type will not appear
        /// on the bottom of list
        /// </summary>
        public override int ItemType {
            get { return (int) Type; }
        }

        /// <summary>
        /// return the image that will be used to identify this item
        /// type, it will be used for the bottom buttons of the list
        /// All items of a given type should return the same image! The image used for the 
        /// bottom buttons will be that of the first item found for the given type
        /// </summary>
        public override Image ItemTypeImage {
            get { return Utils.GetImageFromStr(Type.ToString()); }
        }

        /// <summary>
        /// The text that describes this item type
        /// </summary>
        public override string ItemTypeText {
            get { return "Category : <span class='SubTextColor'><b>" + Type + "</b></span><br><br>"; }
        }

        /// <summary>
        /// return true if the item is to be highlighted
        /// </summary>
        public override bool IsRowHighlighted {
            get { return false; }
        }

        /// <summary>
        /// return a string containing the subtext to display
        /// </summary>
        public override string SubText { get; set; }

        /// <summary>
        /// return a list of images to be displayed (in reverse order) for the item
        /// </summary>
        public override List<Image> TagImages {
            get {
                var outList = new List<Image>();
                typeof(ParseFlag).ForEach<ParseFlag>((s, l) => {
                    if (l == 0 || !Flags.HasFlag((ParseFlag) l))
                        return;
                    Image tryImg = (Image) ImageResources.ResourceManager.GetObject(s);
                    if (tryImg != null)
                        outList.Add(tryImg);
                });
                return outList;
            }
        }

        /// <summary>
        /// Html tip for this object
        /// </summary>
        public override string ToString() {
            return Type.ToString();
        }

        /// <summary>
        /// Should return true when the completion item survives the filter
        /// </summary>
        public virtual bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            // check for scope
            if (ParsedBaseItem != null) {
                var parsedItem = ParsedBaseItem as ParsedItem;
                if (parsedItem != null && parsedItem.Scope != null && currentScope != null && !(parsedItem.Scope is ParsedFile)) {
                    // must be in the right scope!
                    return parsedItem.Scope.ScopeType == currentScope.ScopeType && parsedItem.Scope.Name.Equals(currentScope.Name);
                }
            }
            return true;
        }

        #region Children and parent

        /// <summary>
        /// return the list of the children for this item (if any) or null
        /// </summary>
        public List<CompletionItem> Children { get; set; }

        /// <summary>
        /// The char that separate this item's word with each child item's word
        /// The chars that, once entered after THIS word, should trigger the list of children
        /// If you add a new child separator, you should register it in the AutoCompletion class
        /// </summary>
        public char? ChildSeparator { get; set; }

        /// <summary>
        /// Parent completionItem of this item (the parent has this item in its children)
        /// </summary>
        public CompletionItem ParentItem { get; set; }

        #endregion

        #region Factory

        public static class Factory {
            public static CompletionItem New(CompletionType type) {
                switch (type) {
                    case CompletionType.VariablePrimitive:
                        return new VariablePrimitiveCompletionItem();
                    case CompletionType.VariableComplex:
                        return new VariableComplexCompletionItem();
                    case CompletionType.Widget:
                        return new WidgetCompletionItem();
                    case CompletionType.TempTable:
                        return new TempTableCompletionItem();
                    case CompletionType.Table:
                        return new TableCompletionItem();
                    case CompletionType.Keyword:
                        return new KeywordCompletionItem();
                    case CompletionType.KeywordObject:
                        return new KeywordObjectCompletionItem();
                    case CompletionType.Field:
                        return new FieldCompletionItem();
                    case CompletionType.FieldPk:
                        return new FieldPkCompletionItem();
                    case CompletionType.Procedure:
                        return new ProcedureCompletionItem();
                    case CompletionType.ExternalProcedure:
                        return new ExternalProcedureCompletionItem();

                    case CompletionType.LangWord:
                        return new LangWordCompletionItem();
                    case CompletionType.LangFunction:
                        return new LangFunctionCompletionItem();
                    /*
                    case CompletionType.Snippet:
                        return new SnippetCompletionItem();
                    case CompletionType.Function:
                        return new FunctionCompletionItem();
                    case CompletionType.Database:
                        return new DatabaseCompletionItem();
                    case CompletionType.Sequence:
                        return new SequenceCompletionItem();
                    case CompletionType.Preprocessed:
                        return new PreprocessedCompletionItem();
                    case CompletionType.Label:
                        return new LabelCompletionItem();
                    case CompletionType.Word:
                        return new WordCompletionItem();
                    case CompletionType.LangWord:
                        return new LangWordCompletionItem();
                    */
                    default:
                        throw new Exception("You forgot to add the type" + type + " to the factory! Noob!");
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// This enum order defines the default order for the auto completion
    /// </summary>
    internal enum CompletionType {
        Snippet,
        VariablePrimitive,
        VariableComplex,
        Widget,
        Function,
        Procedure,
        ExternalProcedure,
        Database,
        TempTable,
        Table,
        Sequence,
        Preprocessed,
        Label,
        Keyword,
        KeywordObject,
        FieldPk,
        Field,

        LangWord,
        LangFunction,
        Word,
        Number,
    }

    /// <summary>
    /// Snippets
    /// </summary>
    internal class SnippetCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Snippet; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Snippet; }
        }
    }

    /// <summary>
    /// Variables (primitive, complex and widgets)
    /// </summary>
    internal abstract class VariableCompletionItem : CompletionItem {
        public ParsedDefine ParsedDefine {
            get { return ParsedBaseItem as ParsedDefine; }
        }

        public override string ToString() {
            var toDisplay = new StringBuilder();
            toDisplay.Append(HtmlHelper.FormatRow("Define type", HtmlHelper.FormatSubString(ParsedDefine.Type.ToString())));
            if (!string.IsNullOrEmpty(ParsedDefine.TempPrimitiveType))
                toDisplay.Append(HtmlHelper.FormatRow("Variable type", HtmlHelper.FormatSubString(ParsedDefine.PrimitiveType.ToString())));
            if (ParsedDefine.AsLike == ParsedAsLike.Like)
                toDisplay.Append(HtmlHelper.FormatRow("Is LIKE", ParsedDefine.TempPrimitiveType));
            if (!string.IsNullOrEmpty(ParsedDefine.ViewAs))
                toDisplay.Append(HtmlHelper.FormatRow("Screen representation", ParsedDefine.ViewAs));
            if (!string.IsNullOrEmpty(ParsedDefine.Left)) {
                toDisplay.Append(HtmlHelper.FormatSubtitle("END OF DECLARATION"));
                toDisplay.Append(@"<div class='ToolTipcodeSnippet'>");
                toDisplay.Append(ParsedDefine.Left);
                toDisplay.Append(@"</div>");
            }
            return toDisplay.ToString();
        }

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            // check for scope
            if (!base.SurvivesFilter(currentLine, currentScope))
                return false;

            // check for the definition line
            if (currentLine >= 0) {
                return currentLine >= (ParsedDefine.IncludeLine >= 0 ? ParsedDefine.IncludeLine : ParsedDefine.Line);
            }
            return true;
        }
    }

    internal class VariablePrimitiveCompletionItem : VariableCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.VariablePrimitive; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.VariablePrimitive; }
        }
    }

    internal class VariableComplexCompletionItem : VariableCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.VariableComplex; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.VariableComplex; }
        }
    }

    internal class WidgetCompletionItem : VariableCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Widget; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Widget; }
        }
    }

    /// <summary>
    /// Function
    /// </summary>
    internal class FunctionCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Function; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Function; }
        }

        public ParsedFunction ParsedFunction {
            get { return ParsedBaseItem as ParsedFunction; }
        }

        public override string ToString() {
            var toDisplay = new StringBuilder();
            toDisplay.Append(HtmlHelper.FormatSubtitle("RETURN TYPE"));
            toDisplay.Append(HtmlHelper.HtmlFormatRowParam(ParseFlag.Output, "Returns " + HtmlHelper.FormatSubString(ParsedFunction.ReturnType.ToString())));

            toDisplay.Append(HtmlHelper.FormatSubtitle("PARAMETERS"));
            if (ParsedFunction.Parameters != null && ParsedFunction.Parameters.Count > 0) {
                foreach (var parameter in ParsedFunction.Parameters) {
                    toDisplay.Append(HtmlHelper.HtmlFormatRowParam(parameter.Flags, parameter.Name + " as " + HtmlHelper.FormatSubString(parameter.PrimitiveType.ToString())));
                }
            } else {
                toDisplay.Append("None");
            }

            var funcImplem = ParsedBaseItem as ParsedImplementation;
            if (funcImplem != null) {
                toDisplay.Append(HtmlHelper.FormatSubtitle("PROTOTYPE"));
                if (funcImplem.HasPrototype) {
                    toDisplay.Append(HtmlHelper.FormatRowWithImg("Prototype", "<a class='ToolGotoDefinition' href='proto#" + ParsedFunction.FilePath + "#" + funcImplem.PrototypeLine + "#" + funcImplem.PrototypeColumn + "'>Go to prototype</a>"));
                } else {
                    toDisplay.Append("Has none");
                }
            } else {
                toDisplay.Append(HtmlHelper.FormatSubtitle("DEFINED IN"));
                toDisplay.Append("Function defined in an external procedure or is a web service operation");
            }

            return toDisplay.ToString();
        }
    }

    /// <summary>
    /// Procedure
    /// </summary>
    internal class ProcedureCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Procedure; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Procedure; }
        }

        public ParsedProcedure ParsedProcedure {
            get { return ParsedBaseItem as ParsedProcedure; }
        }

        public override string ToString() {
            var toDisplay = new StringBuilder();
            // find its parameters
            toDisplay.Append(HtmlHelper.FormatSubtitle("PARAMETERS"));
            if (ParsedProcedure.Parameters != null && ParsedProcedure.Parameters.Count > 0) {
                foreach (var parameter in ParsedProcedure.Parameters) {
                    toDisplay.Append(HtmlHelper.HtmlFormatRowParam(parameter.Flags, parameter.Name + " as " + HtmlHelper.FormatSubString(parameter.PrimitiveType.ToString())));
                }
            } else
                toDisplay.Append("None");
            return toDisplay.ToString();
        }
    }

    internal class ExternalProcedureCompletionItem : ProcedureCompletionItem {
        public override Image ItemTypeImage {
            get { return ImageResources.ExternalProcedure; }
        }
    }

    /// <summary>
    /// Database
    /// </summary>
    internal class DatabaseCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Database; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Database; }
        }

        public ParsedDataBase ParsedDataBase {
            get { return ParsedBaseItem as ParsedDataBase; }
        }

        public override string ToString() {
            var toDisplay = new StringBuilder();
            toDisplay.Append(HtmlHelper.FormatRow("Logical name", ParsedDataBase.Name));
            toDisplay.Append(HtmlHelper.FormatRow("Physical name", ParsedDataBase.PhysicalName));
            toDisplay.Append(HtmlHelper.FormatRow("Progress version", ParsedDataBase.ProgressVersion));
            toDisplay.Append(HtmlHelper.FormatRow("Number of Tables", ParsedDataBase.Tables.Count.ToString()));
            return toDisplay.ToString();
        }
    }

    /// <summary>
    /// Table
    /// </summary>
    internal class TableCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Table; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Table; }
        }

        public override string ToString() {
            var toDisplay = new StringBuilder();

            // buffer
            if (FromParser) {
                if (ParsedBaseItem is ParsedDefine) {
                    toDisplay.Append(HtmlHelper.FormatRowWithImg(ParseFlag.Buffer.ToString(), "BUFFER FOR " + HtmlHelper.FormatSubString(SubText)));
                }
                if (ParsedBaseItem is ParsedTable && !string.IsNullOrEmpty(SubText)) {
                    toDisplay.Append(HtmlHelper.FormatRow("Is like", (SubText.Contains("?")) ? "Unknown table [" + ((ParsedTable) ParsedBaseItem).LcLikeTable + "]" : SubText.Replace("Like ", "")));
                }
            }

            var tbItem = ParsedBaseItem as ParsedTable;
            if (tbItem != null) {
                if (!string.IsNullOrEmpty(tbItem.Description)) {
                    toDisplay.Append(HtmlHelper.FormatRow("Description", tbItem.Description));
                }

                if (tbItem.Fields.Count > 0) {
                    toDisplay.Append(HtmlHelper.FormatSubtitle("FIELDS [x" + tbItem.Fields.Count + "]"));
                    toDisplay.Append("<table width='100%;'>");
                    foreach (var parsedField in tbItem.Fields) {
                        toDisplay.Append("<tr><td><img src='" + (parsedField.Flags.HasFlag(ParseFlag.Primary) ? CompletionType.FieldPk.ToString() : CompletionType.Field.ToString()) + "'></td><td style='padding-right: 4px'>" + (parsedField.Flags.HasFlag(ParseFlag.Mandatory) ? "<img src='Mandatory'>" : "") + "</td><td style='padding-right: 8px'>" + parsedField.Name + "</td><td style='padding-right: 8px'>" + parsedField.Type + "</td><td style='padding-right: 8px'> = " + (string.IsNullOrEmpty(parsedField.InitialValue) ? "DEFAULT" : parsedField.Type == ParsedPrimitiveType.Character ? parsedField.InitialValue.ProQuoter() : parsedField.InitialValue) + "</td><td style='padding-right: 8px'>" + parsedField.Description + "</td></tr>");
                    }
                    toDisplay.Append("</table>");
                }

                if (tbItem.Triggers.Count > 0) {
                    toDisplay.Append(HtmlHelper.FormatSubtitle("TRIGGERS [x" + tbItem.Triggers.Count + "]"));
                    foreach (var parsedTrigger in tbItem.Triggers) {
                        toDisplay.Append(HtmlHelper.FormatRow(parsedTrigger.Event, "<a class='ToolGotoDefinition' href='trigger#" + parsedTrigger.ProcName + "'>" + parsedTrigger.ProcName + "</a>"));
                    }
                }

                if (tbItem.Indexes.Count > 0) {
                    toDisplay.Append(HtmlHelper.FormatSubtitle("INDEXES [x" + tbItem.Indexes.Count + "]"));
                    foreach (var parsedIndex in tbItem.Indexes) {
                        toDisplay.Append(HtmlHelper.FormatRow(parsedIndex.Name, (parsedIndex.Flag != ParsedIndexFlag.None ? parsedIndex.Flag + " - " : "") + parsedIndex.FieldsList.Aggregate((i, j) => i + ", " + j)));
                    }
                }
            }

            return toDisplay.ToString();
        }
    }

    internal class TempTableCompletionItem : TableCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.TempTable; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.TempTable; }
        }
    }

    /// <summary>
    /// Sequence
    /// </summary>
    internal class SequenceCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Sequence; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Sequence; }
        }

        public override string ToString() {
            return HtmlHelper.FormatRow("Database logical name", SubText);
        }
    }

    /// <summary>
    /// Pre processed
    /// </summary>
    internal class PreprocessedCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Preprocessed; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Preprocessed; }
        }

        public ParsedPreProcVariable ParsedPreProcVariable {
            get { return ParsedBaseItem as ParsedPreProcVariable; }
        }

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            var output = true;
            if (currentLine >= 0) {
                // if preproc, check line of definition and undefine
                output = currentLine >= (ParsedPreProcVariable.IncludeLine >= 0 ? ParsedPreProcVariable.IncludeLine : ParsedPreProcVariable.Line);
                if (ParsedPreProcVariable.UndefinedLine > 0)
                    output = output && currentLine <= ParsedPreProcVariable.UndefinedLine;
            }
            return output;
        }

        public override string ToString() {
            var toDisplay = new StringBuilder();
            if (ParsedPreProcVariable.UndefinedLine > 0)
                toDisplay.Append(HtmlHelper.FormatRow("Undefined line", ParsedPreProcVariable.UndefinedLine.ToString()));
            toDisplay.Append(HtmlHelper.FormatSubtitle("VALUE"));
            toDisplay.Append(@"<div class='ToolTipcodeSnippet'>");
            toDisplay.Append(ParsedPreProcVariable.Value);
            toDisplay.Append(@"</div>");
            return toDisplay.ToString();
        }
    }

    internal class LabelCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Label; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Label; }
        }

        public ParsedLabel ParsedLabel {
            get { return ParsedBaseItem as ParsedLabel; }
        }

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            // check for scope
            if (!base.SurvivesFilter(currentLine, currentScope))
                return false;

            // check for the definition line
            var output = true;
            if (currentLine >= 0) {
                output = currentLine >= (ParsedLabel.IncludeLine >= 0 ? ParsedLabel.IncludeLine : ParsedLabel.Line);

                // for labels, only display them in the block which they label
                output = output && currentLine <= ParsedLabel.UndefinedLine;
            }
            return output;
        }
    }

    /// <summary>
    /// Keyword
    /// </summary>
    internal class KeywordCompletionItem : CompletionItem {
        public KeywordType KeywordType { get; set; }

        public override CompletionType Type {
            get { return CompletionType.Keyword; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Keyword; }
        }

        public override string SubText {
            get { return KeywordType.ToString(); }
        }

        public override string ToString() {
            var toDisplay = new StringBuilder();

            toDisplay.Append(HtmlHelper.FormatRow("Type of keyword", HtmlHelper.FormatSubString(SubText)));

            // for abbreviations, find the complete keyword first
            string keyword = DisplayText;
            if (KeywordType == KeywordType.Abbreviation) {
                keyword = Keywords.Instance.GetFullKeyword(keyword);
                toDisplay.Append(HtmlHelper.FormatRow("Abbreviation of", HtmlHelper.FormatSubString(keyword)));
            }
            string keyToFind = string.Join(" ", DisplayText, KeywordType);

            // for the keywords define and create, we try to match the second keyword that goes with it
            if (KeywordType == KeywordType.Statement && (keyword.EqualsCi("define") || keyword.EqualsCi("create"))) {
                var lineStr = Sci.GetLine(Sci.LineFromPosition(Sci.GetPositionFromMouseLocation())).LineText;
                var listOfSecWords = new List<string> {"ALIAS", "BROWSE", "BUFFER", "BUTTON", "CALL", "CLIENT-PRINCIPAL", "DATA-SOURCE", "DATABASE", "DATASET", "EVENT", "FRAME", "IMAGE", "MENU", "PARAMETER", "PROPERTY", "QUERY", "RECTANGLE", "SAX-ATTRIBUTES", "SAX-READER", "SAX-WRITER", "SERVER", "SERVER-SOCKET", "SOAP-HEADER", "SOAP-HEADER-ENTRYREF", "SOCKET", "STREAM", "SUB-MENU", "TEMP-TABLE", "VARIABLE", "WIDGET-POOL", "WORK-TABLE", "WORKFILE", "X-DOCUMENT", "X-NODEREF"};
                foreach (var word in listOfSecWords) {
                    if (lineStr.ContainsFast(word)) {
                        keyToFind = string.Join(" ", keyword, word, KeywordType);
                        break;
                    }
                }
            }

            var dataHelp = Keywords.Instance.GetKeywordHelp(keyToFind);
            if (dataHelp != null) {
                toDisplay.Append(HtmlHelper.FormatSubtitle("DESCRIPTION"));
                toDisplay.Append(dataHelp.Description);

                // synthax
                if (dataHelp.Synthax.Count >= 1 && !string.IsNullOrEmpty(dataHelp.Synthax[0])) {
                    toDisplay.Append(HtmlHelper.FormatSubtitle("SYNTAX"));
                    toDisplay.Append(@"<div class='ToolTipcodeSnippet'>");
                    var i = 0;
                    foreach (var synthax in dataHelp.Synthax) {
                        if (i > 0) toDisplay.Append(@"<br>");
                        toDisplay.Append(synthax);
                        i++;
                    }
                    toDisplay.Append(@"</div>");
                }
            } else {
                toDisplay.Append(HtmlHelper.FormatSubtitle("404 NOT FOUND"));
                if (KeywordType == KeywordType.Option)
                    toDisplay.Append("<i><b>Sorry, this keyword doesn't have any help associated</b><br>Since this keyword is an option, try to hover the first keyword of the statement or refer to the 4GL help</i>");
                else
                    toDisplay.Append("<i><b>Sorry, this keyword doesn't have any help associated</b><br>Please refer to the 4GL help</i>");
            }

            return toDisplay.ToString();
        }

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            return true;
        }
    }

    /// <summary>
    /// Keyword types enumeration
    /// </summary>
    public enum KeywordType {
        // below are the types that go to the Keyword category
        Statement,
        Function,
        Operator,
        Option,
        Type,
        Widget,
        Preprocessor,
        Handle,
        Event,
        Keyboard,
        Abbreviation,
        Appbuilder,
        Unknow,

        // below are the types that go into the KeywordObject category
        Attribute = 30,
        Property,
        Method
    }

    internal class KeywordObjectCompletionItem : KeywordCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.KeywordObject; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.KeywordObject; }
        }

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            return true;
        }
    }

    /// <summary>
    /// Fields
    /// </summary>
    internal class FieldCompletionItem : CompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Field; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Field; }
        }

        public ParsedField ParsedField {
            get { return ParsedBaseItem as ParsedField; }
        }

        public override string ToString() {
            var toDisplay = new StringBuilder();
            if (ParsedField.AsLike == ParsedAsLike.Like) {
                toDisplay.Append(HtmlHelper.FormatRow("Is LIKE", ParsedField.TempType));
            }
            toDisplay.Append(HtmlHelper.FormatRow("Type", HtmlHelper.FormatSubString(SubText)));
            toDisplay.Append(HtmlHelper.FormatRow("Owner table", ((ParsedTable) ParentItem.ParsedBaseItem).Name));
            if (!string.IsNullOrEmpty(ParsedField.Description))
                toDisplay.Append(HtmlHelper.FormatRow("Description", ParsedField.Description));
            if (!string.IsNullOrEmpty(ParsedField.Format))
                toDisplay.Append(HtmlHelper.FormatRow("Format", ParsedField.Format));
            if (!string.IsNullOrEmpty(ParsedField.InitialValue))
                toDisplay.Append(HtmlHelper.FormatRow("Initial value", ParsedField.InitialValue));
            toDisplay.Append(HtmlHelper.FormatRow("Order", ParsedField.Order.ToString()));
            return toDisplay.ToString();
        }
    }

    internal class FieldPkCompletionItem : FieldCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.FieldPk; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.FieldPk; }
        }
    }

    /// <summary>
    /// Completion items that were extracted from a text (non progress document)
    /// </summary>
    internal abstract class TextCompletionItem : CompletionItem {
        
        public Token OriginToken { get; set; }

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            return true;
        }
    }

    /// <summary>
    /// Word (parsed from the npp document)
    /// </summary>
    internal class WordCompletionItem : TextCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Word; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Word; }
        }

    }

    /// <summary>
    /// Number (parsed from the npp document)
    /// </summary>
    internal class NumberCompletionItem : TextCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.Number; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.Numbers; }
        }
    }

    /// <summary>
    /// Class for Completion items that were read from a .xml configuration file
    /// </summary>
    internal abstract class LangCompletionItem : CompletionItem {

        public NppLangs.NppKeyword NppKeyword { get; set; }

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            return true;
        }
    }

    /// <summary>
    /// Lang fucntion (read from xml conf files)
    /// </summary>
    internal class LangFunctionCompletionItem : LangCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.LangFunction; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.LangFunction; }
        }
    }

    /// <summary>
    /// Lang word (read from xml conf files)
    /// </summary>
    internal class LangWordCompletionItem : LangCompletionItem {
        public override CompletionType Type {
            get { return CompletionType.LangWord; }
        }

        public override Image ItemTypeImage {
            get { return ImageResources.LangWord; }
        }
    }
}