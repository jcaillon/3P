using System;
using System.Collections.Generic;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// base abstract class for ParsedItem
    /// </summary>
    public abstract class ParsedItem {
        public string Name { get; private set; }
        public string FilePath { get; set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public ParsedScope Scope { get; set; }
        public string LcOwnerName { get; set; }
        public abstract void Accept(IParserVisitor visitor);
        protected ParsedItem(string name, int line, int column) {
            Name = name;
            Line = line;
            Column = column;
        }
    }

    public enum ParsedScope {
        Global,
        Procedure,
        Function,
        Trigger
    }

    /// <summary>
    /// Flags applicable for every ParsedItems
    /// </summary>
    [Flags]
    public enum ParseFlag {
        None = 1,
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
        Index = 4096
    }

    /// <summary>
    /// Function parsed item
    /// Flag : private
    /// </summary>
    public class ParsedFunction : ParsedItem {
        public string ReturnType { get; private set; }
        public string Parameters { get; set; }
        public bool IsPrivate { get; set; }
        public string LcName { get; private set; }
        public int PrototypeLine { get; set; }
        public int PrototypeColumn { get; set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFunction(string name, int line, int column, string returnType, string lcName) : base(name, line, column) {
            ReturnType = returnType;
            LcName = lcName;
        }
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    public class ParsedOnEvent : ParsedItem {
        public string On { get; private set; }
        public string LcName { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedOnEvent(string name, int line, int column, string @on, string lcName)
            : base(name, line, column) {
            On = @on;
            LcName = lcName;
        }
    }


    /// <summary>
    /// Procedure parsed item
    /// </summary>
    public class ParsedProcedure : ParsedItem {
        public string Left { get; private set; }
        public string LcName { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedProcedure(string name, int line, int column, string left, string lcName) : base(name, line, column) {
            Left = left;
            LcName = lcName;
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
        public string LcFlagString { get; private set; }
        /// <summary>
        /// contains as or like in lowercase
        /// </summary>
        public string LcAsLike { get; private set; }
        public string Left { get; private set; }
        public ParseDefineType Type { get; private set; }
        /// <summary>
        /// When parsing, we store the value of the "primitive-type" in there, 
        /// with the visitor, we convert this to a ParsedPrimitiveType later
        /// </summary>
        public string TempPrimitiveType { get; private set; } 
        /// <summary>
        /// Used only for variables, contains the primitive type (for "as x") or the field name (for "like x")
        /// </summary>
        public ParsedPrimitiveType PrimitiveType { get; set; }
        /// <summary>
        /// first word after "view-as"
        /// </summary>
        public string ViewAs { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedDefine(string name, int line, int column, string lcFlagString, string lcAsLike, string left, ParseDefineType type, string lcTempPrimitivePrimitiveType, string viewAs) : base(name, line, column) {
            LcFlagString = lcFlagString;
            LcAsLike = lcAsLike;
            Left = left;
            Type = type;
            TempPrimitiveType = lcTempPrimitivePrimitiveType;
            ViewAs = viewAs;
        }
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
        // below are the types that are not considered as primitive
        Clob = 30,
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
        public string Description { get; private set; }
        /// <summary>
        /// contains the table "LIKE TABLE" name in lowercase
        /// </summary>
        public string LcLikeTable { get; private set; }
        public int Ranking { get; set; }
        /// <summary>
        /// To know if the table is a temptable
        /// </summary>
        public bool IsTempTable { get; private set; }
        /// <summary>
        /// In case of a temp table, can contains the eventuals :
        /// NEW [ GLOBAL ] ] SHARED ] | [ PRIVATE | PROTECTED ] [ STATIC ] flags
        /// </summary>
        public string LcFlagString { get; private set; }
        public List<ParsedField> Fields { get; private set; }
        public List<ParsedIndex> Indexes { get; private set; }
        public List<ParsedTrigger> Triggers { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedTable(string name, int line, int column, string id, string crc, string dumpName, string description, string lcLikeTable, int ranking, bool isTempTable, List<ParsedField> fields, List<ParsedIndex> indexes, List<ParsedTrigger> triggers, string lcFlagString) : base(name, line, column) {
            Id = id;
            Crc = crc;
            DumpName = dumpName;
            Description = description;
            LcLikeTable = lcLikeTable;
            Ranking = ranking;
            IsTempTable = isTempTable;
            Fields = fields;
            Indexes = indexes;
            Triggers = triggers;
            LcFlagString = lcFlagString;
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
        public string LcAsLike { get;  set; }
        public int Ranking { get;  set; }
        public ParsedField(string name, string lcTempType, string format, int order, ParsedFieldFlag flag, string initialValue, string description, string lcAsLike, int ranking) {
            Name = name;
            TempType = lcTempType;
            Format = format;
            Order = order;
            Flag = flag;
            InitialValue = initialValue;
            Description = description;
            LcAsLike = lcAsLike;
            Ranking = ranking;
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
