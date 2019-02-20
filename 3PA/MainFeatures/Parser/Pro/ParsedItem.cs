#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParsedItem.cs) is part of 3P.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using _3PA.MainFeatures.Parser.Pro.Visit;

namespace _3PA.MainFeatures.Parser.Pro {

    #region base classes

    internal abstract class ParsedBaseItem {
        /// <summary>
        /// Name of the parsed item
        /// </summary>
        public string Name { get; private set; }

        protected ParsedBaseItem(string name) {
            Name = name;
        }

        public virtual ParsedScopeType GetScopeType() {
            return ParsedScopeType.Root;
        }
    }

    /// <summary>
    /// base abstract class for ParsedItem
    /// </summary>
    internal abstract class ParsedItem : ParsedBaseItem {
        /// <summary>
        /// full file path in which this item has been parsed
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// In an include file, each item parsed has this value which corresponds to the line at which the include was done in the base program.
        /// This is used to filter the autocompletion to only variables that should be shown.
        /// You can also test if this value is inferior to 0 to test if the parsed item belongs to the base program or an include
        /// The default value is -1 
        /// </summary>
        public int IncludeLine { get; set; }

        /// <summary>
        /// The starting position of the first keyword of the statement where the item is found
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Line of the first keyword of the statement where the item is found
        /// <remarks>THE LINE COUNT START AT 0 NOT 1!!</remarks>
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Column of the first keyword of the statement where the item is found
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// The ending position of the first keyword of the statement where the item is found
        /// OR the ending position of the EOS of the statement (be careful to verify this 
        /// before using this property...)
        /// </summary>
        public int EndPosition { get; set; }

        /// <summary>
        /// Scope in which this item has been parsed; will never be of type PreProcBlock / SimpleBlock because this field
        /// is made to find the parent object of this item (file/function/procedure...)
        /// </summary>
        public ParsedScopeBlock Scope { get; set; }

        public ParseFlag Flags { get; set; }

        public abstract void Accept(IParserVisitor visitor);

        protected ParsedItem(string name, Token token) : base(name) {
            Line = token.Line;
            Column = token.Column;
            Position = token.StartPosition;
            EndPosition = token.EndPosition;
            IncludeLine = -1;
        }

        public override ParsedScopeType GetScopeType() {
            return Scope != null ? Scope.ScopeType : ParsedScopeType.Root;
        }
    }

    /// <summary>
    /// Parent class for procedure, function and OnEvent Items
    /// </summary>
    internal abstract class ParsedScope : ParsedItem {
        /// <summary>
        /// line of the "end" keyword that ends the block
        /// </summary>
        public int EndBlockLine { get; set; }

        /// <summary>
        /// end position of the EOS that closes the block, initiated to -1 by default
        /// </summary>
        public int EndBlockPosition { get; set; }

        /// <summary>
        /// Allows faster comparison against ParsedScopeItems
        /// </summary>
        public ParsedScopeType ScopeType { get; private set; }

        protected ParsedScope(string name, Token token, ParsedScopeType scopeType)
            : base(name, token) {
            ScopeType = scopeType;
            EndBlockPosition = -1;
        }
    }

    /// <summary>
    /// Allows faster comparison against ParsedScopeItems (should have the same name as the CodeExplorerBranch Enum)
    /// </summary>
    internal enum ParsedScopeType {
        Root = 1,
        Procedure,
        Function,
        OnEvent,
        Class,
        Method,
        Constructor,
        
        PreProcAnalyzeBlock,
        PreProcIfBlock,
        SimpleBlock,
    }

    #endregion

    #region ParseFlag

    /// <summary>
    /// Flags applicable for every ParsedItems
    /// </summary>
    [Flags]
    internal enum ParseFlag : ulong {
        // indicates that the parsed item is not coming from the originally parsed source (= from .i)
        FromInclude = 1 << 0,
        // Local/File define the scope of a defined variable...
        LocalScope = 1 << 1,
        FileScope = 4,
        Parameter = 8,
        // is used for keywords
        Reserved = 16,
        Abbreviation = 32,
        New = 64,
        // Special flag for DEFINE
        Global = 128,
        Shared = 256,
        Private = 512,
        // flags for fields
        Mandatory = 1024,
        Extent = 2048,
        Index = 4096,
        // is a buffer
        Buffer = 8192,
        // the variable was defined with a CREATE and not a DEFINE
        Dynamic = 16384,
        // class serializable
        Serializable = 32768,
        // a proc or func was loaded in persistent
        Persistent = 65536,

        // the block has too much characters and the program will not be open-able in the appbuilder
        //IsTooLong = 131072, <- USE THIS IT'S FREE
        // applies for Run statement, the program/proc to run is VALUE(something) so we only guess which one it is
        Uncertain = 262144,
        // if a table found w/o the database name before it
        MissingDbName = 524288,
        // if the .i file is not found in the propath
        NotFound = 1048576,
        // a procedure is an EXTERNAL procedure
        External = 2097152,

        // Method
        Protected = 4194304,
        Public = 8388608,
        Static = 16777216,
        Abstract = 33554432,
        Override = 67108864,
        Final = 134217728,

        // parameters
        Input = 268435456,
        InputOutput = 536870912,
        Output = 1073741824,
        Return = 2147483648,

        Primary = 4294967296
    }

    #endregion

    #region procedural classes

    /// <summary>
    /// A scope that does not have a representation in the code explorer
    /// </summary>
    internal class ParsedScopeNoSection : ParsedScope {

        public override void Accept(IParserVisitor visitor) {
            // no visits
        }

        public ParsedScopeNoSection(string name, Token token, ParsedScopeType scopeType) : base(name, token, scopeType) {
        }
    }

    /// <summary>
    /// A scope that has a representation in the code explorer
    /// </summary>
    internal class ParsedScopeSection : ParsedScope {

        public override void Accept(IParserVisitor visitor) {
            // no visits
        }

        public ParsedScopeSection(string name, Token token, ParsedScopeType scopeType) : base(name, token, scopeType) {
        }
    }

    /// <summary>
    /// A simple do, triggers... block. This type of block only exists to compute a correct indentation
    /// </summary>
    internal class ParsedScopeSimpleBlock : ParsedScopeNoSection {

        public override void Accept(IParserVisitor visitor) {
            // no visits
        }

        public ParsedScopeSimpleBlock(string name, Token token) : base(name, token, ParsedScopeType.SimpleBlock) {
        }
    }

    /// <summary>
    /// Corresponds to a single time indent block after a THEN or OTHERWISE...
    /// </summary>
    internal class ParsedScopeOneStatementIndentBlock : ParsedScopeNoSection {
        
        public int StatementNumber { get; private set; }

        public int LineOfNextWord { get; set; }

        public override void Accept(IParserVisitor visitor) {
            // no visits
        }

        public ParsedScopeOneStatementIndentBlock(string name, Token token, int statementNumber) : base(name, token, ParsedScopeType.SimpleBlock) {
            StatementNumber = statementNumber;
        }
    }

    /// <summary>
    /// A &amp;IF expression &amp;THEN kind of block. This type of block only exists to compute a correct indentation
    /// </summary>
    internal class ParsedScopePreProcIfBlock : ParsedScopeNoSection {

        /// <summary>
        /// The if "expression" then
        /// </summary>
        public string EvaluatedExpression { get; set; }

        /// <summary>
        /// The result of the if "expression" then
        /// </summary>
        public bool ExpressionResult { get; set; }

        public override void Accept(IParserVisitor visitor) {
            // no visits
        }

        public ParsedScopePreProcIfBlock(string name, Token token) : base(name, token, ParsedScopeType.PreProcIfBlock) {
        }
    }
    
    /// <summary>
    /// Represents a pre-processed directive block (&amp;ANALYZE-SUSPEND and closed by &amp;ANALYZE-RESUME)
    /// </summary>
    internal class ParsedScopePreProcBlock : ParsedScopeSection {

        /// <summary>
        /// type of this block
        /// </summary>
        public ParsedPreProcBlockType Type { get; set; }

        /// <summary>
        /// Everything after ANALYZE-SUSPEND
        /// </summary>
        public string BlockDescription { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedScopePreProcBlock(string name, Token token) : base(name, token, ParsedScopeType.PreProcAnalyzeBlock) {}
    }


    internal enum ParsedPreProcBlockType {
        Block, // an unknown block
        Prototype,
        MainBlock,
        DefinitionBlock,
        UibPreprocessorBlock,
        XtfrBlock,
        SettingsBlock,
        CreateWindowBlock,
        RuntimeBlock
    }

    /// <summary>
    /// This class represents a scope that can have variables defined in it. It will be used to filter the variables to show in the autocompletion
    /// depending on where the cursor is. In openedge, a def var done in a do: end. block (for instance), is not limited to the DO but to the parent
    /// scope that is a procedure or the file (for instance)
    /// </summary>
    internal class ParsedScopeBlock : ParsedScopeSection {
        
        /// <summary>
        /// If true, the block contains too much characters and will not be openable in the
        /// appbuilder
        /// </summary>
        public bool TooLongForAppbuilder { get; set; }

        public override void Accept(IParserVisitor visitor) {
            // no visits
        }

        public ParsedScopeBlock(string name, Token token, ParsedScopeType scopeType) : base(name, token, scopeType) {
        }
    }


    /// <summary>
    /// The "root" scope of a file
    /// </summary>
    internal class ParsedFile : ParsedScopeBlock {
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFile(string name, Token token) : base(name, token, ParsedScopeType.Root) {}
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    internal class ParsedProcedure : ParsedScopeBlock {
        public string Left { get; private set; }

        public string ExternalDllName { get; private set; }

        public List<ParsedDefine> Parameters { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedProcedure(string name, Token token, string left, string externalDllName)
            : base(name, token, ParsedScopeType.Procedure) {
            Left = left;
            ExternalDllName = externalDllName;
        }
    }

    /// <summary>
    /// Function parsed item
    /// Flag : private
    /// </summary>
    internal abstract class ParsedFunction : ParsedScopeBlock {

        public string TempReturnType { get; private set; }

        public ParsedPrimitiveType ReturnType { get; set; }

        /// <summary>
        /// is the return-type "EXTENT [x]" (0 if not extented)
        /// </summary>
        public int Extent { get; set; }

        /// <summary>
        /// We keep the string formed with the parameters, it allows us to quickly identify if a prototype is up 
        /// to date or not
        /// </summary>
        public string ParametersString { get; set; }

        public List<ParsedDefine> Parameters { get; set; }

        protected ParsedFunction(string name, Token token, string tempReturnType) : base(name, token, ParsedScopeType.Function) {
            TempReturnType = tempReturnType;
        }
    }

    /// <summary>
    /// Function parsed item
    /// </summary>
    internal class ParsedPrototype : ParsedFunction {
        /// <summary>
        /// true if it's a simple FORWARD and the implementation is in the same proc,
        /// false otherwise (meaning we matched a IN)
        /// </summary>
        public bool SimpleForward { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedPrototype(string name, Token token, string tempReturnType) : base(name, token, tempReturnType) {
        }
    }

    /// <summary>
    /// Function parsed item
    /// </summary>
    internal class ParsedImplementation : ParsedFunction {
        /// <summary>
        /// true if this function is an implementation AND has a prototype
        /// </summary>
        public bool HasPrototype { get; set; }

        public int PrototypeLine { get; set; }
        public int PrototypeColumn { get; set; }
        public int PrototypePosition { get; set; }
        public int PrototypeEndPosition { get; set; }

        /// <summary>
        /// Boolean to know if the prototype is correct compared to the implementation
        /// </summary>
        public bool PrototypeUpdated { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedImplementation(string name, Token token, string tempReturnType) : base(name, token, tempReturnType) {
        }
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    internal class ParsedOnStatement : ParsedScopeBlock {
        public string EventList { get; private set; }
        public string WidgetList { get; private set; }

        /// <summary>
        /// not null if this ON statement has a trigger block
        /// </summary>
        public ParsedScopeSimpleBlock TriggerBlock { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedOnStatement(string name, Token token, string eventList, string widgetList)
            : base(name, token, ParsedScopeType.OnEvent) {
            EventList = eventList;
            WidgetList = widgetList;
        }
    }

    /// <summary>
    /// A simple word parsed in the file
    /// </summary>
    internal class ParsedWord : ParsedItem {
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedWord(string name, Token token) : base(name, token) { }
    }


    /// <summary>
    /// found table in program
    /// </summary>
    internal class ParsedFoundTableUse : ParsedItem {
        public bool IsTempTable { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFoundTableUse(string name, Token token, bool isTempTable)
            : base(name, token) {
            IsTempTable = isTempTable;
        }
    }

    /// <summary>
    /// Label
    /// </summary>
    internal class ParsedLabel : ParsedItem {

        /// <summary>
        /// The block that this label describes
        /// </summary>
        public ParsedScopeSimpleBlock Block { get; set; }

        public int UndefinedLine { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedLabel(string name, Token token) : base(name, token) {}
    }

    /// <summary>
    /// dynamic function calls parsed item
    /// </summary>
    internal class ParsedFunctionCall : ParsedItem {
        /// <summary>
        /// true if the called function is not defined in the program
        /// </summary>
        public bool ExternalCall { get; private set; }

        public bool StaticCall { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFunctionCall(string name, Token token, bool externalCall, bool staticCall)
            : base(name, token) {
            ExternalCall = externalCall;
            StaticCall = staticCall;
        }
    }

    /// <summary>
    /// Run parsed item
    /// </summary>
    internal class ParsedRun : ParsedItem {
        public string Left { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedRun(string name, Token token, string left) : base(name, token) {
            Left = left;
        }
    }

    /// <summary>
    /// Subscribe parsed item (name is the event name)
    /// </summary>
    internal class ParsedEvent : ParsedItem {
        public ParsedEventType Type { get; private set; }
        public string SubscriberHandle { get; private set; }

        /// <summary>
        /// can be anywhere or the handle of a proc
        /// </summary>
        public string PublisherHandler { get; private set; }

        public string RunProcedure { get; private set; }
        public string Left { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedEvent(ParsedEventType type, string name, Token token, string subscriberHandle, string publisherHandler, string runProcedure, string left) : base(name, token) {
            SubscriberHandle = subscriberHandle;
            PublisherHandler = publisherHandler;
            RunProcedure = runProcedure;
            Left = left;
            Type = type;
        }
    }

    internal enum ParsedEventType {
        Subscribe,
        Unsubscribe,
        Publish
    }

    /// <summary>
    /// include file parsed item
    /// </summary>
    internal class ParsedIncludeFile : ParsedItem {

        /// <summary>
        /// Contains a dictionary in which each preproc variable name known corresponds to its value.
        /// Contains only the preproc values that were defined with & SCOPED-DEFINE.
        /// Values defined with & GLOBAL-DEFINE are stored in the "base" include file (i.e. the file being compiled).
        /// </summary>
        public Dictionary<string, string> DefinedPreProcVariables { get; private set; }

        /// <summary>
        /// Contains a dictionary in which each preproc variable name known corresponds to its value.
        /// Contains only the preproc values passed to the include file at call.
        /// - either position based: {1}->SHARED
        /// - or name based: {& name}->_extension
        /// depending on the way parameters were passed to the include.
        /// </summary>
        public Dictionary<string, string> ParametersPreProcVariables { get; }

        /// <summary>
        /// if null, that means this ParsedIncludeFile is actually the procedure being parsed,
        /// otherwise it can be found in an include itself found on the procedure being parsed
        /// </summary>
        public ParsedIncludeFile Parent { get; private set; }

        /// <summary>
        /// File path of the include file (when actually found in the propath, name of the include otherwise)
        /// </summary>
        public string IncludeFilePath { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedIncludeFile(string name, Token token, Dictionary<string, string> parametersPreProcVariables, string includeFilePath, ParsedIncludeFile parent)
            : base(name, token) {
            ParametersPreProcVariables = parametersPreProcVariables;
            IncludeFilePath = includeFilePath;
            Parent = parent;
        }

        /// <summary>
        /// Returns the value of a given preproc variable.
        /// </summary>
        /// <param name="variableName">Can be a position e.g. "1" or a name e.g. "& name"</param>
        /// <returns></returns>
        public string GetScopedPreProcVariableValue(string variableName) {
            if (string.IsNullOrEmpty(variableName)) {
                return null;
            }
            if (variableName.Equals("0")) {
                return string.IsNullOrEmpty(IncludeFilePath) ? null : Path.GetFileName(IncludeFilePath);
            }
            // &scoped-define variables prevail over preproc parameters passed to the include
            if (DefinedPreProcVariables != null && DefinedPreProcVariables.ContainsKey(variableName)) {
                return DefinedPreProcVariables[variableName];
            }
            if (ParametersPreProcVariables != null && ParametersPreProcVariables.ContainsKey(variableName)) {
                return ParametersPreProcVariables[variableName];
            }
            // search for the value in parent scope
            // the global preproc, available to the whole include stack, are defined in the "root" include (which is the compiled program).
            var parentInclude = Parent;
            while (parentInclude != null) {
                if (parentInclude.DefinedPreProcVariables != null && parentInclude.DefinedPreProcVariables.ContainsKey(variableName)) {
                    return parentInclude.DefinedPreProcVariables[variableName];
                }
                parentInclude = parentInclude.Parent;
            }
            return null;
        }

        /// <summary>
        /// This function returns a value of 1 if the argument was a name defined with the & GLOBAL-DEFINE directive;
        /// a value of 2 if the argument was passed as an include file parameter;
        /// and a value of 3 if the argument was a name defined with the & SCOPED-DEFINE directive.
        /// If the argument was not defined and was not an include file parameter, then this function returns a value of 0.
        /// </summary>
        /// <param name="variableName">Can be a position e.g. "1" or a name e.g. "& name"</param>
        /// <returns></returns>
        public int GetScopedPreProcVariableDefinedLevel(string variableName) {
            // TODO: this does not work correctly because we can't distinguish between a scope variable on root file and a global variable.
            if (string.IsNullOrEmpty(variableName)) {
                return 0;
            }
            if (variableName.Equals("0")) {
                return 2;
            }
            // &scoped-define variables prevail over preproc parameters passed to the include
            if (DefinedPreProcVariables != null && DefinedPreProcVariables.ContainsKey(variableName)) {
                return 3;
            }
            if (ParametersPreProcVariables != null && ParametersPreProcVariables.ContainsKey(variableName)) {
                return 2;
            }
            // search for the value in parent scope
            // the global preproc, available to the whole include stack, are defined in the "root" include (which is the compiled program).
            var parentInclude = Parent;
            while (parentInclude != null) {
                if (parentInclude.DefinedPreProcVariables != null && parentInclude.DefinedPreProcVariables.ContainsKey(variableName)) {
                    return 1;
                }
                parentInclude = parentInclude.Parent;
            }
            return 0;
        }

        /// <summary>
        /// Set a new defined preproc variable belonging to that include (the base parsed program is also an include!)
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="variableValue"></param>
        public void SetDefinedPreProcVariable(string variableName, string variableValue) {
            if (DefinedPreProcVariables == null) {
                DefinedPreProcVariables = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            }
            if (DefinedPreProcVariables.ContainsKey( variableName))
                DefinedPreProcVariables[variableName] = variableValue;
            else
                DefinedPreProcVariables.Add(variableName, variableValue);
        }
        
        /// <summary>
        /// Returns the line in the base file were this include was included
        /// (if you have base.p -> inc1.i -> inc2.i and you are currently in inc2.i, will return the line were inc1.i is defined in base.p)
        /// </summary>
        /// <returns></returns>
        public int GetLineDefinitionInCompiledFile() {
            // search for the value in parent scope
            // the global preproc, available to the whole include stack, are defined in the "root" include (which is the compiled program).
            var firstInclude = this;
            while (firstInclude.Parent?.Parent != null) {
                firstInclude = firstInclude.Parent;
            }
            return firstInclude.Line;
        }
    }

    /// <summary>
    /// Pre-processed var parsed item
    /// </summary>
    internal class ParsedPreProcVariable : ParsedItem {
        public string Value { get; private set; }
        public int UndefinedLine { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedPreProcVariable(string name, Token token, int undefinedLine, string value) : base(name, token) {
            UndefinedLine = undefinedLine;
            Value = value;
        }
    }

    /// <summary>
    /// Pre-processed var parsed item
    /// </summary>
    internal class ParsedUsedPreProcVariable : ParsedItem {
        
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedUsedPreProcVariable(string name, Token token, bool notFound) : base(name, token) {
            Flags = notFound ? ParseFlag.NotFound : 0;
        }
    }

    /// <summary>
    /// Define parsed item
    /// </summary>
    internal class ParsedDefine : ParsedItem {

        /// <summary>
        /// contains as or like
        /// </summary>
        public ParsedAsLike AsLike { get; private set; }

        public string Left { get; private set; }

        /// <summary>
        /// The "Type" is what succeeds the DEFINE word of the statement (VARIABLE, BUFFER....)
        /// </summary>
        public ParseDefineType Type { get; private set; }

        /// <summary>
        /// When parsing, we store the value of the "primitive-type" in there, 
        /// with the visitor, we convert this to a ParsedPrimitiveType later
        /// </summary>
        public string TempPrimitiveType { get; private set; }

        /// <summary>
        /// (Used for variables) contains the primitive type of the variable
        /// </summary>
        public ParsedPrimitiveType PrimitiveType { get; set; }

        public int Extent { get; }

        /// <summary>
        /// first word after "view-as"
        /// </summary>
        public string ViewAs { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedDefine(string name, Token token, ParsedAsLike asLike, string left, ParseDefineType type, string tempPrimitiveType, string viewAs, int extent)
            : base(name, token) {
            AsLike = asLike;
            Left = left;
            Type = type;
            TempPrimitiveType = tempPrimitiveType;
            ViewAs = viewAs;
            Extent = extent;
        }
    }

    internal enum ParsedAsLike {
        None,
        As,
        Like
    }

    /// <summary>
    /// Enumeration of DEFINE types
    /// </summary>
    internal enum ParseDefineType {
        [Description("PARAMETER")]
        Parameter,

        [Description("DATA-SOURCE")]
        DataSource,

        [Description("EVENT")]
        Event,

        [Description("BUFFER")]
        Buffer,

        [Description("VARIABLE")]
        Variable,

        [Description("BROWSE")]
        Browse,

        [Description("STREAM")]
        Stream,

        [Description("BUTTON")]
        Button,

        [Description("DATASET")]
        Dataset,

        [Description("IMAGE")]
        Image,

        [Description("MENU")]
        Menu,

        [Description("FRAME")]
        Frame,

        [Description("QUERY")]
        Query,

        [Description("RECTANGLE")]
        Rectangle,

        [Description("PROPERTY")]
        Property,

        [Description("SUB-MENU")]
        SubMenu,

        [Description("NONE")]
        None
    }

    internal enum ParsedPrimitiveType {
        Character = 0,
        Comhandle,
        Date,
        Datetime,
        Datetimetz,
        Decimal,
        Handle,
        Int64,
        Integer,
        Logical,
        Longchar,
        Memptr,
        Raw,
        Recid,
        Rowid,

        // Below are the types allowed for the parameters
        Buffer = 20,
        Table,
        TableHandle,
        Dataset,
        DatasetHandle,

        // below are the types that are not considered as primitive (they will appear in the VariableComplex category)
        Clob = 30,
        WidgetHandle,
        Blob,
        Widget,
        Unknow,
        Class, // in that case the syntax is [CLASS] class-name

        // below, are the types for the .dll
        Long = 50,
        Short,
        Byte,
        Float,
        Double,
        UnsignedShort,
        UnsignedLong
    }

    /// <summary>
    /// Define parsed item
    /// </summary>
    internal class ParsedBuffer : ParsedDefine {

        /// <summary>
        /// In case of a buffer, contains the references table (BUFFER name FOR xxx)
        /// </summary>
        public string BufferFor { get; private set; }

        /// <summary>
        /// The table for which this buffer is defined
        /// </summary>
        public ParsedTable TargetTable { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedBuffer(string name, Token token, ParsedAsLike asLike, string left, ParseDefineType type, string tempPrimitiveType, string viewAs, string bufferFor) : base(name, token, asLike, left, type, tempPrimitiveType, viewAs, 0) {
            BufferFor = bufferFor;
        }
    }

    /// <summary>
    /// data base parsed item (the Name field is the LogicalName)
    /// </summary>
    internal class ParsedDataBase : ParsedBaseItem {
        public string PhysicalName { get; private set; }
        public string ProgressVersion { get; private set; }
        public List<ParsedTable> Tables { get; private set; }

        /// <summary>
        /// if this database is actualy an alias, this is the name of the referenced database (otherwise null)
        /// </summary>
        public string AliasOf { get; private set; }

        public bool IsAlias {
            get { return !string.IsNullOrEmpty(AliasOf); }
        }

        public ParsedDataBase(string logicalName, string physicalName, string progressVersion, List<ParsedTable> tables, string aliasOf)
            : base(logicalName) {
            PhysicalName = physicalName;
            ProgressVersion = progressVersion;
            Tables = tables;
            AliasOf = aliasOf;
        }
    }
    
    /// <summary>
    /// Table or temp table parsed item
    /// </summary>
    internal class ParsedTable : ParsedItem {

        public string Id { get; private set; }
        public string Crc { get; private set; }
        public string DumpName { get; private set; }

        public bool Hidden { get; private set; }
        public bool Frozen { get; private set; }
        public ParsedTableType TableType { get; private set; }

        /// <summary>
        /// From database, represents the description of the table
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// If this table is like another, link to said other
        /// </summary>
        public ParsedTable LikeTable { get; set; }

        /// <summary>
        /// If this table is like another, link to said other
        /// </summary>
        public string TempLikeTable { get; private set; }

        /// <summary>
        /// if temptable and temptable is "like" another table, contains the USE-INDEX 
        /// </summary>
        public string UseIndex { get; private set; }

        public List<ParsedField> Fields { get; set; }
        public List<ParsedIndex> Indexes { get; set; }
        public List<ParsedTrigger> Triggers { get; set; }

        public bool IsTempTable {
            get { return TableType == ParsedTableType.TT; }
        }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedTable(string name, Token token, ParsedTableType tableType, string id, string crc, string dumpName, string description, string strLikeTable, List<ParsedField> fields, List<ParsedIndex> indexes, List<ParsedTrigger> triggers, string useIndex, bool hidden, bool frozen) : base(name, token) {
            Id = id;
            Crc = crc;
            DumpName = dumpName;
            Description = description;
            Fields = fields;
            Indexes = indexes;
            Triggers = triggers;
            UseIndex = useIndex;
            Hidden = hidden;
            Frozen = frozen;
            TableType = tableType;
            TempLikeTable = strLikeTable;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal enum ParsedTableType {
        [Description("User Data Table")]
        T,
        [Description("Virtual System Table")]
        S,
        [Description("SQL View")]
        V,
        [Description("Temp Table")]
        TT                              
    }

    /// <summary>
    /// describes a field of a table
    /// </summary>
    internal class ParsedField : ParsedBaseItem {
        /// <summary>
        /// When parsing, we store the value of the "primitive-type" in there, 
        /// with the visitor, we convert this to a ParsedPrimitiveType later
        /// </summary>
        public string TempPrimitiveType { get; set; }
        public ParsedPrimitiveType PrimitiveType { get; set; }
        public string Format { get; set; }
        public int Order { get; set; }
        public ParseFlag Flags { get; set; }
        public string InitialValue { get; set; }
        public string Description { get; set; }
        public int Extent { get; set; }

        /// <summary>
        /// contains as or like in lowercase
        /// </summary>
        public ParsedAsLike AsLike { get; set; }


        public ParsedField(string name, string lcTempPrimitiveType, string format, int order, ParseFlag flags, string initialValue, string description, ParsedAsLike asLike) : base(name) {
            TempPrimitiveType = lcTempPrimitiveType;
            Format = format;
            Order = order;
            Flags = flags;
            InitialValue = initialValue;
            Description = description;
            AsLike = asLike;
        }
    }

    /// <summary>
    /// defines a index of a table
    /// </summary>
    internal class ParsedIndex {
        public string Name { get; private set; }
        public ParsedIndexFlag Flag { get; set; }
        public List<string> FieldsList { get; private set; }

        public ParsedIndex(string name, ParsedIndexFlag flag, List<string> fieldsList) {
            Name = name;
            Flag = flag;
            FieldsList = fieldsList;
        }
    }

    [Flags]
    internal enum ParsedIndexFlag {
        None = 1,
        Unique = 2,
        Primary = 4,
        WordIndex = 8
    }

    /// <summary>
    /// defines a trigger of a table
    /// </summary>
    internal class ParsedTrigger {
        public string Event { get; private set; }
        public string ProcName { get; private set; }

        public ParsedTrigger(string @event, string procName) {
            Event = @event;
            ProcName = procName;
        }
    }

    internal class ParsedSequence {
        public string SeqName { get; set; }
        public string DbName { get; set; }
    }

    #endregion

    #region OOP classes

    /*
    /// <summary>
    /// Class
    /// </summary>
    internal class ParsedClass : ParsedScopeBlock {
        /// <summary>
        /// Super type name
        /// </summary>
        public string Inherits { get; private set; }
        /// <summary>
        /// List of interfaces name
        /// </summary>
        public List<string> Implements { get; private set; }

        public ParseFlag Flags { get; private set; }

        public List<ParsedMethod> Methods { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedClass(string name, Token token)
            : base(name, token, ParsedScopeType.Class) {
        }
    }

    /// <summary>
    /// Method/Constructor
    /// Constructor are very identical to method so they use the same parse object,
    /// but we differentiate them with ParsedScopeType
    /// Flags : [ PRIVATE | PROTECTED | PUBLIC ][ STATIC | ABSTRACT ] [OVERRIDE] [FINAL]
    /// </summary>
    internal class ParsedMethod : ParsedScopeBlock {

        /// <summary>
        /// Return type or VOID
        /// </summary>
        public string ReturnType { get; private set; }

        /// <summary>
        /// </summary>
        

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedMethod(string name, Token token, ParsedScopeType type)
            : base(name, token, type) {
        }
    }

    /// <summary>
    /// Constructor
    /// Constructor are very identical to method so they use the same parse object,
    /// but we differentiate them with ParsedScopeType
    /// </summary>
    internal class ParsedConstructor : ParsedScopeBlock {

        /// <summary>
        /// Return type or VOID
        /// </summary>
        public string ReturnType { get; private set; }

        /// <summary>
        /// Flags : [ PRIVATE | PROTECTED | PUBLIC ][ STATIC | ABSTRACT ] [OVERRIDE] [FINAL]
        /// </summary>
        public ParseFlag Flags { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedConstructor(string name, Token token)
            : base(name, token, ParsedScopeType.Constructor) {
        }
    }

    /// <summary>
    /// Method
    /// </summary>
    internal class ParsedMethod : ParsedScopeBlock {

        /// <summary>
        /// Return type or VOID
        /// </summary>
        public string ReturnType { get; private set; }

        /// <summary>
        /// Flags : [ PRIVATE | PROTECTED | PUBLIC ][ STATIC | ABSTRACT ] [OVERRIDE] [FINAL]
        /// </summary>
        public ParseFlag Flags { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedMethod(string name, Token token)
            : base(name, token, ParsedScopeType.Method) {
        }
    }

    /// <summary>
    /// Method/Constructor
    /// Constructor are very identical to method so they use the same parse object,
    /// but we differentiate them with ParsedScopeType
    /// </summary>
    internal class ParsedMethod : ParsedScopeBlock {

        /// <summary>
        /// Return type or VOID
        /// </summary>
        public string ReturnType { get; private set; }

        /// <summary>
        /// Flags : [ PRIVATE | PROTECTED | PUBLIC ][ STATIC | ABSTRACT ] [OVERRIDE] [FINAL]
        /// </summary>
        public ParseFlag Flags { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedMethod(string name, Token token, ParsedScopeType type)
            : base(name, token, type) {
        }
    }
    */

    #endregion
}