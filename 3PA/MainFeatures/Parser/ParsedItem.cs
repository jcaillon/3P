using System;
using System.Collections.Generic;

namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// base abstract class for ParsedItem
    /// </summary>
    public abstract class ParsedItem {
        public string Name { get; private set; }
        public ParseFlag Flag { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public abstract void Accept(IParserVisitor visitor);
        protected ParsedItem(string name, ParseFlag flag, int line, int column) {
            Name = name;
            Flag = flag;
            Line = line;
            Column = column;
        }
    }

    /// <summary>
    /// Flags applicable for every ParsedItems
    /// </summary>
    [Flags]
    public enum ParseFlag {
        None = 1,
        Scope = 2,
        Global = 4,
        Parameter = 8,
        Reserved = 16,
        Abbreviation = 32,
        TempTable = 64,
        Input = 128,
        Output = 256,
        InputOutput = 512,
        Return = 1024,
        Private = 2028,
    }

    /// <summary>
    /// Mother of ParsedFunction and ParsedProcedure
    /// </summary>
    public abstract class ParsedScope : ParsedItem {
        protected ParsedScope(string name, ParseFlag flag, int line, int column) : base(name, flag, line, column) {}
    }

    /// <summary>
    /// Function parsed item
    /// Flag : private
    /// </summary>
    public class ParsedFunction : ParsedScope {
        public string ReturnType { get; private set; }
        public string Parameters { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFunction(string name, ParseFlag flag, int line, int column, string returnType, string parameters) : base(name, flag, line, column) {
            ReturnType = returnType;
            Parameters = parameters;
        }
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    public class ParsedProcedure : ParsedScope {
        public string Left { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedProcedure(string name, ParseFlag flag, int line, int column, string left) : base(name, flag, line, column) {
            Left = left;
        }
    }

    /// <summary>
    /// include file parsed item
    /// </summary>
    public class ParsedIncludeFile : ParsedItem {
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedIncludeFile(string name, ParseFlag flag, int line, int column) : base(name, flag, line, column) {}
    }

    /// <summary>
    /// Pre-processed var parsed item
    /// </summary>
    public class ParsedPreProc : ParsedItem {
        public string Definition { get; private set; }
        public int UndefinedLine { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedPreProc(string name, ParseFlag flag, int line, int column, string definition, int undefinedLine) : base(name, flag, line, column) {
            Definition = definition;
            UndefinedLine = undefinedLine;
        }
    }

    /// <summary>
    /// Define parsed item
    /// </summary>
    public class ParsedDefine : ParsedItem {
        public string AsLike { get; private set; }
        public string Left { get; private set; }
        public ParseDefineType Type { get; private set; }
        public string PrimitiveType { get; private set; }
        public ParseScope Scope { get; private set; }
        public ParsedScope OwnerIfNotGlobal { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedDefine(string name, ParseFlag flag, int line, int column, string asLike, string left, ParseDefineType type, string primitiveType, ParseScope scope, ParsedScope ownerIfNotGlobal) : base(name, flag, line, column) {
            AsLike = asLike;
            Left = left;
            Type = type;
            PrimitiveType = primitiveType;
            Scope = scope;
            OwnerIfNotGlobal = ownerIfNotGlobal;
        }
    }

    public enum ParseScope {
        Global,
        Procedure,
        Function
    }

    public enum ParseDefineType {
        Buffer,
        Variable,
        TempTable,
        Browse,
        Stream,
        Button,
        Dataset,
        Frame,
        Query,
        Rectangle,
    }

    /// <summary>
    /// Table or temp table parsed item
    /// </summary>
    public class ParsedTable : ParsedItem {
        public string Id { get; private set; }
        public string Crc { get; private set; }
        public string DumpName { get; private set; }
        public string Description { get; private set; }
        public ParseScope Scope { get; private set; }
        public ParsedScope OwnerIfNotGlobal { get; private set; }
        public List<ParsedField> Fields { get; private set; }
        public List<ParsedIndex> Indexes { get; private set; }
        public List<ParsedTrigger> Triggers { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedTable(string name, ParseFlag flag, int line, int column, string id, string crc, string dumpName, string description, ParseScope scope, ParsedScope ownerIfNotGlobal, List<ParsedField> fields, List<ParsedIndex> indexes, List<ParsedTrigger> triggers) : base(name, flag, line, column) {
            Id = id;
            Crc = crc;
            DumpName = dumpName;
            Description = description;
            Scope = scope;
            OwnerIfNotGlobal = ownerIfNotGlobal;
            Fields = fields;
            Indexes = indexes;
            Triggers = triggers;
        }
    }

    /// <summary>
    /// describes a field of a table
    /// </summary>
    public class ParsedField {
        public string Name { get; private set; }
        public string Type { get; private set; }    
        public string Format { get; private set; }
        public int Order { get; private set; }
        public ParseFieldFlag Flag { get; private set; }
        public string InitialValue { get; private set; }
        public string Description { get; private set; }
        public string Stat { get; private set; }
        public ParsedField(string name, string type, string format, int order, ParseFieldFlag flag, string initialValue, string description, string stat) {
            Name = name;
            Type = type;
            Format = format;
            Order = order;
            Flag = flag;
            InitialValue = initialValue;
            Description = description;
            Stat = stat;
        }
    }

    [Flags]
    public enum ParseFieldFlag {
        Mandatory = 1,
        Extent = 2,
        Index = 4,
        Primary = 8
    }

    /// <summary>
    /// defines a index of a table
    /// </summary>
    public class ParsedIndex {
        public string Name { get; private set; }
        public ParseIndexFlag Flag { get; private set; }
        public List<string> FieldsList { get; private set; }
        public ParsedIndex(string name, ParseIndexFlag flag, List<string> fieldsList) {
            Name = name;
            Flag = flag;
            FieldsList = fieldsList;
        }
    }

    [Flags]
    public enum ParseIndexFlag {
        Primary = 1,
        Unique = 2,
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
