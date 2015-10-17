using System;
using System.Collections.Generic;
using _3PA.Lib;
using _3PA.MainFeatures.DockableExplorer;

namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// base abstract class for ParsedItem
    /// </summary>
    public abstract class ParsedItem {
        public string Name { get; private set; }
        public string FilePath { get; set; }
        /// <summary>
        /// Line of the first keyword of the statement where the item is found
        /// <remarks>THE LINE COUNT START AT 0 NOT 1!!</remarks>
        /// </summary>
        public int Line { get; private set; }
        /// <summary>
        /// Column of the first keyword of the statement where the item is found
        /// </summary>
        public int Column { get; private set; }
        public ParsedScope Scope { get; set; }
        public string OwnerName { get; set; }
        public abstract void Accept(IParserVisitor visitor);
        protected ParsedItem(string name, int line, int column) {
            Name = name;
            Line = line;
            Column = column;
        }
    }

    public enum ParsedScope {
        File,
        Procedure,
        Function,
        Trigger
    }

    /// <summary>
    /// Flags applicable for every ParsedItems
    /// </summary>
    [Flags]
    public enum ParseFlag {
        // indicates that the parsed item is not coming from the originally parsed source (= from .i)
        External = 1,
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
        // the procedure is EXTERNAL
        ExternalProc = 32768,
    }

    /// <summary>
    /// Parent class for procedure, function and OnEvent Items
    /// </summary>
    public abstract class ParsedScopeItem : ParsedItem {
        /// <summary>
        /// line of the "end" keyword that ends the block
        /// </summary>
        public int EndLine { get; set; }

        /// <summary>
        /// If true, the block contains too much characters and will not be openable in the
        /// appbuilder
        /// </summary>
        public bool TooLongForAppbuilder { get; set; }
        protected ParsedScopeItem(string name, int line, int column) : base(name, line, column) {}
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    public class ParsedBlock : ParsedScopeItem {
        public CodeExplorerIconType IconIconType { get; set; }
        public CodeExplorerBranch Branch { get; set; }
        public bool IsRoot { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }
        public ParsedBlock(string name, int line, int column, CodeExplorerBranch branch) : base(name, line, column) {
            Branch = branch;
        }
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    public class ParsedProcedure : ParsedScopeItem {
        public string Left { get; private set; }

        /// <summary>
        /// Has the external flag in its definition
        /// </summary>
        public bool IsExternal { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedProcedure(string name, int line, int column, string left, bool isExternal)
            : base(name, line, column) {
            Left = left;
            IsExternal = isExternal;
        }
    }

    /// <summary>
    /// Function parsed item
    /// Flag : private
    /// </summary>
    public class ParsedFunction : ParsedScopeItem {
        public ParsedPrimitiveType ReturnType { get; set; }
        /// <summary>
        /// Parsed string for the return type, use ReturnType instead!
        /// </summary>
        public string ParsedReturnType { get; private set; }
        /// <summary>
        /// is the return-type "EXTENT [x]"
        /// </summary>
        public bool IsExtended { get; set; }
        public string Parameters { get; set; }
        public bool IsPrivate { get; set; }
        public int PrototypeLine { get; set; }
        public int PrototypeColumn { get; set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFunction(string name, int line, int column, string parsedReturnType) : base(name, line, column) {
            ParsedReturnType = parsedReturnType;
        }
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    public class ParsedOnEvent : ParsedScopeItem {
        public string On { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedOnEvent(string name, int line, int column, string @on)
            : base(name, line, column) {
            On = @on;
        }
    }

    /// <summary>
    /// found table in program
    /// </summary>
    public class ParsedFoundTableUse : ParsedItem {
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFoundTableUse(string name, int line, int column)
            : base(name, line, column) {
        }
    }

    /// <summary>
    /// Label
    /// </summary>
    public class ParsedLabel : ParsedItem {
        public int UndefinedLine { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedLabel(string name, int line, int column) : base(name, line, column) {
        }
    }

    /// <summary>
    /// dynamic function calls parsed item
    /// </summary>
    public class ParsedFunctionCall : ParsedItem {
        /// <summary>
        /// true if the called function is not defined in the program
        /// </summary>
        public bool ExternalCall { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFunctionCall(string name, int line, int column, bool externalCall)
            : base(name, line, column) {
            ExternalCall = externalCall;
        }
    }

    /// <summary>
    /// Run parsed item
    /// </summary>
    public class ParsedRun : ParsedItem {
        /// <summary>
        /// true if the Run statement is based on a evaluating a VALUE()
        /// </summary>
        public bool IsEvaluateValue { get; private set; }
        public string Left { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedRun(string name, int line, int column, string left, bool isEvaluateValue) : base(name, line, column) {
            Left = left;
            IsEvaluateValue = isEvaluateValue;
        }
    }

    /// <summary>
    /// include file parsed item
    /// </summary>
    public class ParsedIncludeFile : ParsedItem {

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedIncludeFile(string name, int line, int column) : base(name, line, column) {}
    }

    /// <summary>
    /// Pre-processed var parsed item
    /// </summary>
    public class ParsedPreProc : ParsedItem {
        public int UndefinedLine { get; set; }
        public ParsedPreProcFlag Flag { get; set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedPreProc(string name, int line, int column, int undefinedLine, ParsedPreProcFlag flag) : base(name, line, column) {
            UndefinedLine = undefinedLine;
            Flag = flag;
        }
    }

    public enum ParsedPreProcFlag {
        None = 1,
        Scope = 2,
        Global = 4,
    }

    /// <summary>
    /// Define parsed item
    /// </summary>
    public class ParsedDefine : ParsedItem {
        /// <summary>
        /// can contains (separated by 1 space) :
        /// global, shared, private, new
        /// </summary>
        public string LcFlagString { get; private set; }
        /// <summary>
        /// contains as or like in lowercase
        /// (for buffers, it contains the table it buffs)
        /// </summary>
        public ParsedAsLike AsLike { get; private set; }
        /// <summary>
        /// In case of a buffer, contains the references table (BUFFER name FOR xxx)
        /// </summary>
        public string BufferFor { get; private set; }
        /// <summary>
        /// if the variable is "EXTENT [x]"
        /// </summary>
        public bool IsExtended { get; private set; }
        /// <summary>
        /// if the variable was CREATE'd instead of DEFINE'd
        /// </summary>
        public bool IsDynamic { get; private set; }
        public string Left { get; private set; }
        public ParseDefineType Type { get; private set; }
        /// <summary>
        /// When parsing, we store the value of the "primitive-type" in there, 
        /// with the visitor, we convert this to a ParsedPrimitiveType later
        /// </summary>
        public string TempPrimitiveType { get; private set; } 
        /// <summary>
        /// (Used for variables) contains the raw (string) primitive type, will be converted to ParseDefineType
        /// in the ParserVisitor
        /// </summary>
        public ParsedPrimitiveType PrimitiveType { get; set; }
        /// <summary>
        /// first word after "view-as"
        /// </summary>
        public string ViewAs { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedDefine(string name, int line, int column, string lcFlagString, ParsedAsLike asLike, string left, ParseDefineType type, string tempPrimitiveType, string viewAs, string bufferFor, bool isExtended, bool isDynamic)
            : base(name, line, column) {
            LcFlagString = lcFlagString;
            AsLike = asLike;
            Left = left;
            Type = type;
            TempPrimitiveType = tempPrimitiveType;
            ViewAs = viewAs;
            BufferFor = bufferFor;
            IsExtended = isExtended;
            IsDynamic = isDynamic;
        }
    }

    public enum ParsedAsLike {
        None,
        As,
        Like
    }

    public class ParseDefineTypeAttr : Extensions.EnumAttr {
        public string Value { get; set; }
    }

    /// <summary>
    /// Enumeration of DEFINE types
    /// Retrieve the description value with ((ParseDefineTypeAttr)parseDefineType.GetAttributes()).Value
    /// </summary>
    public enum ParseDefineType {
        [ParseDefineTypeAttr(Value = "PARAMETER")]
        Parameter,
        [ParseDefineTypeAttr(Value = "DATA-SOURCE")]
        DataSource,
        [ParseDefineTypeAttr(Value = "EVENT")]
        Event,
        [ParseDefineTypeAttr(Value = "BUFFER")]
        Buffer,
        [ParseDefineTypeAttr(Value = "VARIABLE")]
        Variable,
        [ParseDefineTypeAttr(Value = "BROWSE")]
        Browse,
        [ParseDefineTypeAttr(Value = "STREAM")]
        Stream,
        [ParseDefineTypeAttr(Value = "BUTTON")]
        Button,
        [ParseDefineTypeAttr(Value = "DATASET")]
        Dataset,
        [ParseDefineTypeAttr(Value = "IMAGE")]
        Image,
        [ParseDefineTypeAttr(Value = "MENU")]
        Menu,
        [ParseDefineTypeAttr(Value = "FRAME")]
        Frame,
        [ParseDefineTypeAttr(Value = "QUERY")]
        Query,
        [ParseDefineTypeAttr(Value = "RECTANGLE")]
        Rectangle,
        [ParseDefineTypeAttr(Value = "PROPERTY")]
        Property,
        [ParseDefineTypeAttr(Value = "SUB-MENU")]
        SubMenu,
        [ParseDefineTypeAttr(Value = "NONE")]
        None,
    }

    public enum ParsedPrimitiveType {
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
    /// data base parsed item
    /// </summary>
    public class ParsedDataBase {
        public string LogicalName { get; private set; }
        public string PhysicalName { get; private set; }
        public string ProgressVersion { get; private set; }
        public List<ParsedTable> Tables { get; private set; }

        public ParsedDataBase(string logicalName, string physicalName, string progressVersion, List<ParsedTable> tables) {
            LogicalName = logicalName;
            PhysicalName = physicalName;
            ProgressVersion = progressVersion;
            Tables = tables;
        }
    }

    /// <summary>
    /// Table or temp table parsed item
    /// </summary>
    public class ParsedTable : ParsedItem {
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
        /// <summary>
        /// In case of a temp table, can contains the eventuals :
        /// NEW [ GLOBAL ] ] SHARED ] | [ PRIVATE | PROTECTED ] [ STATIC ] flags
        /// </summary>
        public string LcFlagString { get; private set; }
        public List<ParsedField> Fields { get; set; }
        public List<ParsedIndex> Indexes { get; set; }
        public List<ParsedTrigger> Triggers { get; set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedTable(string name, int line, int column, string id, string crc, string dumpName, string description, string lcLikeTable, bool isTempTable, List<ParsedField> fields, List<ParsedIndex> indexes, List<ParsedTrigger> triggers, string lcFlagString, string useIndex) : base(name, line, column) {
            Id = id;
            Crc = crc;
            DumpName = dumpName;
            Description = description;
            LcLikeTable = lcLikeTable;
            IsTempTable = isTempTable;
            Fields = fields;
            Indexes = indexes;
            Triggers = triggers;
            LcFlagString = lcFlagString;
            UseIndex = useIndex;
        }
    }

    /// <summary>
    /// describes a field of a table
    /// </summary>
    public class ParsedField {
        public string Name { get; private set; }
        /// <summary>
        /// When parsing, we store the value of the "primitive-type" in there, 
        /// with the visitor, we convert this to a ParsedPrimitiveType later
        /// </summary>
        public string TempType { get; set; } 
        public ParsedPrimitiveType Type { get; set; } 
        public string Format { get;  set; }
        public int Order { get;  set; }
        public ParsedFieldFlag Flag { get;  set; }
        public string InitialValue { get;  set; }
        public string Description { get;  set; }
        /// <summary>
        /// contains as or like in lowercase
        /// </summary>
        public ParsedAsLike AsLike { get; set; }
        public ParsedField(string name, string lcTempType, string format, int order, ParsedFieldFlag flag, string initialValue, string description, ParsedAsLike asLike) {
            Name = name;
            TempType = lcTempType;
            Format = format;
            Order = order;
            Flag = flag;
            InitialValue = initialValue;
            Description = description;
            AsLike = asLike;
        }
    }

    [Flags]
    public enum ParsedFieldFlag {
        None = 1,
        Extent = 2,
        Index = 4,
        Primary = 8,
        Mandatory = 16
    }

    /// <summary>
    /// defines a index of a table
    /// </summary>
    public class ParsedIndex {
        public string Name { get; private set; }
        public ParsedIndexFlag Flag { get; private set; }
        public List<string> FieldsList { get; private set; }
        public ParsedIndex(string name, ParsedIndexFlag flag, List<string> fieldsList) {
            Name = name;
            Flag = flag;
            FieldsList = fieldsList;
        }
    }

    [Flags]
    public enum ParsedIndexFlag {
        None = 1,
        Unique = 2,
        Primary = 4,
    }

    /// <summary>
    /// defines a trigger of a table
    /// </summary>
    public class ParsedTrigger {
        public string Event { get; private set; }
        public string ProcName { get; private set; }
        public ParsedTrigger(string @event, string procName) {
            Event = @event;
            ProcName = procName;
        }
    }
}
