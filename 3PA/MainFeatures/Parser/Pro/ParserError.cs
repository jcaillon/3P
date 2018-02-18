using System.Collections.Generic;
using System.ComponentModel;

namespace _3PA.MainFeatures.Parser.Pro {

    internal class ParserError {
        /// <summary>
        /// Type of the error
        /// </summary>
        public ParserErrorType Type { get; set; }

        /// <summary>
        /// Line at which the error happened
        /// </summary>
        public int TriggerLine { get; set; }

        /// <summary>
        /// Position at which the error happened
        /// </summary>
        public int TriggerPosition { get; set; }

        /// <summary>
        /// Stack count at the moment of the error (the type of stack will depend on the error)
        /// </summary>
        public int StackCount { get; set; }

        /// <summary>
        /// Can either be in the procedure parser or in an include file
        /// </summary>
        public string FullFilePath { get; set; }

        public ParserError(ParserErrorType type, Token triggerToken, int stackCount, List<ParsedIncludeFile> includeFiles) {
            Type = type;
            TriggerLine = triggerToken.Line;
            TriggerPosition = triggerToken.StartPosition;
            StackCount = stackCount;
            FullFilePath = includeFiles[triggerToken.OwnerNumber].FullFilePath;
        }
    }

    internal enum ParserErrorType {
        [Description("Unexpected block start, you can not nest functions, methods or procedures")]
        ForbiddenNestedBlockStart,

        [Description("Unexpected block end, the start of this block has not been found")]
        UnexpectedBlockEnd,

        [Description("A block end seems to be missing")]
        MissingBlockEnd,

        [Description("Unexpected Appbuilder block start, two consecutive ANALYSE-SUSPEND found (no ANALYSE-RESUME)")]
        UnexpectedUibBlockStart,

        [Description("Unexpected Appbuilder block end, can not match ANALYSE-SUSPEND for this ANALYSE-RESUME")]
        UnexpectedUibBlockEnd,

        [Description("Unexpected Appbuilder block start, ANALYSE-SUSPEND should be created at root level")]
        NotAllowedUibBlockStart,

        [Description("Unexpected Appbuilder block end, ANALYSE-RESUME should be created at root level")]
        NotAllowedUibBlockEnd,

        [Description("A preprocessed directive ANALYSE-RESUME seems to be missing")]
        MissingUibBlockEnd,

        [Description("A preprocessed directive should always be at the beggining of a new statement")]
        UibBlockStartMustBeNewStatement,

        [Description("&ENDIF pre-processed statement matched without the corresponding &IF")]
        UnexpectedPreProcEndIf,

        [Description("&THEN pre-processed statement matched without the corresponding &IF")]
        UnexpectedPreprocThen,

        [Description("&IF pre-processed statement missing an &ENDIF")]
        MissingPreprocEndIf,
    }
}