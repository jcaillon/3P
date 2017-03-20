#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
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

namespace _3PA.MainFeatures.Parser {

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
        /// When including a file, each item parsed has a definition line that corresponds to the line in the file where the item was parsed,
        /// but we also need to need to know where, in the current file parsed, this include is, so we can know filter the items correctly... 
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
        /// Scope in which this item has been parsed
        /// </summary>
        public ParsedScopeItem Scope { get; set; }

        public ParseFlag Flags { get; set; }

        public abstract void Accept(IParserVisitor visitor);

        protected ParsedItem(string name, Token token) : base(name) {
            Line = token.Line;
            Column = token.Column;
            Position = token.StartPosition;
            EndPosition = token.EndPosition;
            IncludeLine = -1;
        }

        public virtual bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            return true;
        }

        public override ParsedScopeType GetScopeType() {
            return Scope != null ? Scope.ScopeType : ParsedScopeType.Root;
        }
    }

    /// <summary>
    /// Parent class for procedure, function and OnEvent Items
    /// </summary>
    internal abstract class ParsedScopeItem : ParsedItem {
        /// <summary>
        /// line of the "end" keyword that ends the block
        /// </summary>
        public int EndBlockLine { get; set; }

        /// <summary>
        /// end position of the EOS that closes the block, initiated to -1 by default
        /// </summary>
        public int EndBlockPosition { get; set; }

        /// <summary>
        /// If true, the block contains too much characters and will not be openable in the
        /// appbuilder
        /// </summary>
        public bool TooLongForAppbuilder { get; set; }

        /// <summary>
        /// Allows faster comparison against ParsedScopeItems
        /// </summary>
        public ParsedScopeType ScopeType { get; private set; }

        protected ParsedScopeItem(string name, Token token, ParsedScopeType scopeType)
            : base(name, token) {
            ScopeType = scopeType;
            EndBlockPosition = -1;
        }
    }

    /// <summary>
    /// Allows faster comparison against ParsedScopeItems (should have the same name as the CodeExplorerBranch Enum)
    /// </summary>
    internal enum ParsedScopeType {
        Root,
        Block,
        Procedure,
        Function,
        OnEvent,
        Class,
        Method,
        Constructor
    }

    #endregion

    #region ParseFlag

    /// <summary>
    /// Flags applicable for every ParsedItems
    /// </summary>
    [Flags]
    internal enum ParseFlag : ulong {
        // indicates that the parsed item is not coming from the originally parsed source (= from .i)
        FromInclude = 1,
        // Local/File define the scope of a defined variable...
        LocalScope = 2,
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
        IsTooLong = 131072,
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
    /// The "root" scope of a file
    /// </summary>
    internal class ParsedFile : ParsedScopeItem {
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFile(string name, Token token) : base(name, token, ParsedScopeType.Root) {}
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    internal class ParsedPreProcBlock : ParsedScopeItem {
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

        public ParsedPreProcBlock(string name, Token token) : base(name, token, ParsedScopeType.Block) {}
    }

    internal enum ParsedPreProcBlockType {
        Unknown,
        FunctionForward,
        MainBlock,
        Definitions,
        UibPreprocessorBlock,
        Xftr,
        ProcedureSettings,
        CreateWindow,
        RunTimeAttributes,

        // before that, this is an ANALYSE-SUSPEND block, below are the other pre-processed block
        IfEndIf
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    internal class ParsedProcedure : ParsedScopeItem {
        public string Left { get; private set; }
        public string ExternalDllName { get; private set; }

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
    internal abstract class ParsedFunction : ParsedScopeItem {
        public ParsedPrimitiveType ReturnType { get; set; }

        /// <summary>
        /// Parsed string for the return type, use ReturnType instead!
        /// </summary>
        public string ParsedReturnType { get; private set; }

        /// <summary>
        /// is the return-type "EXTENT [x]" (0 if not extented) / should be a string representing an integer
        /// </summary>
        public string Extend { get; set; }

        public string Parameters { get; set; }

        protected ParsedFunction(string name, Token token, string parsedReturnType) : base(name, token, ParsedScopeType.Function) {
            ParsedReturnType = parsedReturnType;
            Extend = String.Empty;
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

        public ParsedPrototype(string name, Token token, string parsedReturnType) : base(name, token, parsedReturnType) {
            Extend = String.Empty;
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

        public ParsedImplementation(string name, Token token, string parsedReturnType)
            : base(name, token, parsedReturnType) {
            Extend = String.Empty;
        }
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    internal class ParsedOnStatement : ParsedScopeItem {
        public string EventList { get; private set; }
        public string WidgetList { get; private set; }

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
        public int UndefinedLine { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedLabel(string name, Token token) : base(name, token) {}

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            // check for scope
            var output = true;
            if (currentScope != null && !(Scope is ParsedFile)) {
                output = Scope.ScopeType == currentScope.ScopeType;
                output = output && Scope.Name.Equals(currentScope.Name);
            }
            
            // check for the definition line
            if (currentLine >= 0) {
                output = output && currentLine >= (IncludeLine >= 0 ? IncludeLine : Line);

                // for labels, only dislay them in the block which they label
                output = output && currentLine <= UndefinedLine;
            }
            return output;
        }
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
        /// The dictionary contains the association between parameter name -> value
        /// passed with the include file; either 1->value or & name->value 
        /// depending on the way parameters were passed to the include
        /// The value is tokenized and we store the list of tokens here. During parsing, 
        /// we will inject those tokens in place of the {& varname}
        /// 
        /// Contains a dictionary in which each variable name known corresponds to its value tokenized
        /// It can either be parameters from an include, ex: {1}->SHARED, {& name}->_extension
        /// or & DEFINE variables from the current file
        /// </summary>
        public Dictionary<string, List<Token>> ScopedPreProcVariables { get; set; }

        /// <summary>
        /// if null, that means this ParsedIncludeFile is actually the procedure being parsed,
        /// otherwise it can be found in an include itself found on the procedure being parsed
        /// </summary>
        public ParsedIncludeFile Parent { get; private set; }

        /// <summary>
        /// Full path of the include file (when actually found in the propath
        /// null otherwise)
        /// </summary>
        public string FullFilePath { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedIncludeFile(string name, Token token, Dictionary<string, List<Token>> scopedPreProcVariables, string fullFilePath, ParsedIncludeFile parent)
            : base(name, token) {
            ScopedPreProcVariables = scopedPreProcVariables;
            FullFilePath = fullFilePath;
            Parent = parent;
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

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            var output = true;
            if (currentLine >= 0) {
                // if preproc, check line of definition and undefine
                output = currentLine >= (IncludeLine >= 0 ? IncludeLine : Line);
                if (UndefinedLine > 0)
                    output = output && currentLine <= UndefinedLine;
            }
            return output;
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

        /// <summary>
        /// In case of a buffer, contains the references table (BUFFER name FOR xxx)
        /// </summary>
        public string BufferFor { get; private set; }

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

        /// <summary>
        /// first word after "view-as"
        /// </summary>
        public string ViewAs { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedDefine(string name, Token token, ParsedAsLike asLike, string left, ParseDefineType type, string tempPrimitiveType, string viewAs, string bufferFor)
            : base(name, token) {
            AsLike = asLike;
            Left = left;
            Type = type;
            TempPrimitiveType = tempPrimitiveType;
            ViewAs = viewAs;
            BufferFor = bufferFor;
        }

        public override bool SurvivesFilter(int currentLine, ParsedScopeItem currentScope) {
            // check for scope
            var output = true;
            if (currentScope != null && !(Scope is ParsedFile)) {
                output = Scope.ScopeType == currentScope.ScopeType;
                output = output && Scope.Name.Equals(currentScope.Name);
            }
            // check for the definition line
            if (currentLine >= 0) {
                output = output && currentLine >= (IncludeLine >= 0 ? IncludeLine : Line);
            }
            return output;
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
        Class,
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
    /// data base parsed item (the Name field is the LogicalName)
    /// </summary>
    internal class ParsedDataBase : ParsedBaseItem {
        public string PhysicalName { get; private set; }
        public string ProgressVersion { get; private set; }
        public List<ParsedTable> Tables { get; private set; }

        public ParsedDataBase(string logicalName, string physicalName, string progressVersion, List<ParsedTable> tables)
            : base(logicalName) {
            PhysicalName = physicalName;
            ProgressVersion = progressVersion;
            Tables = tables;
        }
    }

    /// <summary>
    /// Table or temp table parsed item
    /// </summary>
    internal class ParsedTable : ParsedItem {
        public string Id { get; private set; }
        public string Crc { get; private set; }
        public string DumpName { get; private set; }

        /// <summary>
        /// To know if the table is a temptable
        /// </summary>
        public bool IsTempTable { get; private set; }

        /// <summary>
        /// From database, represents the description of the table
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// contains the table "LIKE TABLE" name in lowercase
        /// </summary>
        public string LcLikeTable { get; private set; }

        /// <summary>
        /// if temptable and temptable is "like" another table, contains the USE-INDEX 
        /// </summary>
        public string UseIndex { get; private set; }

        public List<ParsedField> Fields { get; set; }
        public List<ParsedIndex> Indexes { get; set; }
        public List<ParsedTrigger> Triggers { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedTable(string name, Token token, string id, string crc, string dumpName, string description, string lcLikeTable, bool isTempTable, List<ParsedField> fields, List<ParsedIndex> indexes, List<ParsedTrigger> triggers, string useIndex) : base(name, token) {
            Id = id;
            Crc = crc;
            DumpName = dumpName;
            Description = description;
            LcLikeTable = lcLikeTable;
            IsTempTable = isTempTable;
            Fields = fields;
            Indexes = indexes;
            Triggers = triggers;
            UseIndex = useIndex;
        }
    }

    /// <summary>
    /// describes a field of a table
    /// </summary>
    internal class ParsedField : ParsedBaseItem {
        /// <summary>
        /// When parsing, we store the value of the "primitive-type" in there, 
        /// with the visitor, we convert this to a ParsedPrimitiveType later
        /// </summary>
        public string TempType { get; set; }

        public ParsedPrimitiveType Type { get; set; }
        public string Format { get; set; }
        public int Order { get; set; }
        public ParseFlag Flags { get; set; }
        public string InitialValue { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// contains as or like in lowercase
        /// </summary>
        public ParsedAsLike AsLike { get; set; }

        public ParsedField(string name, string lcTempType, string format, int order, ParseFlag flags, string initialValue, string description, ParsedAsLike asLike) : base(name) {
            TempType = lcTempType;
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
    internal class ParsedClass : ParsedScopeItem {
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
    internal class ParsedMethod : ParsedScopeItem {

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
    internal class ParsedConstructor : ParsedScopeItem {

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
    internal class ParsedMethod : ParsedScopeItem {

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
    internal class ParsedMethod : ParsedScopeItem {

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