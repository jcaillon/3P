&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Procedure
/*------------------------------------------------------------------------
Purpose   :
This program is called by 3P when the user execute the "Prolint" action,
the user can freely modify this file to use his own prolint version/settings

The user can use the preprocessed variables defined in this file, they will be replaced
by values from 3P at run-time. Notorious variables are :
- {&PathFileToProlint} : this is a string representing the path to the current file opened in 3P
- {&PathProlintOutputFile} : this is a string representing the path to the prolint output file
that will be read back in 3P to display the prolint messages
(see below for more details about the format of this file)

Description :
When this program is being run, the conditions are the same as when you compile/run a file,
i.e. the databases (if any) are connected, the propath is correctly configured according to
your "Set environment" screen and so on....

The format of the prolint output file expected by 3P is :
filepath \t ErrorLevel \t line \t column \t error number \t error message \t help for the message
errorLevel can be one of the following values : Information, Warning, StrongWarning, Error, Critical
This file is created/written in the Prolint_AddResult procedure

Author(s)   : Julien Caillon (julien.caillon@gmail.com)
Created     : 19/02/2016
Notes       : This file CAN and MUST be freely modified by the user.
----------------------------------------------------------------------*/
/*  This file was created with the 3P :  https://jcaillon.github.io/3P/ */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */

DEFINE VARIABLE gi_currentFile AS INTEGER NO-UNDO.

DEFINE STREAM str_logout.
DEFINE STREAM str_source.
DEFINE STREAM str_dir.

DEFINE TEMP-TABLE tt_rangesToProlint NO-UNDO
    FIELD cFileName     AS CHARACTER
    FIELD iLineBegin    AS INTEGER
    FIELD iLineEnd      AS INTEGER
    .

DEFINE TEMP-TABLE tt_tagBlock NO-UNDO
    FIELD iLineBegin    AS INTEGER
    FIELD iLineEnd      AS INTEGER
    .

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE NO

/* =================================== */

/* Do not modify the lines below, values coming from 3P are set here,
you can use the pre-processed variables defined below if you find them useful! */
/*<inserted_3P_values>*/

&IF DEFINED(PathFileToProlint) = 0 &THEN
    /* this block is only present for debug/tests purposes, the values below are
    overwritten when this file is run from 3P,
    you can set those values manually for your tests */
    &SCOPED-DEFINE PathFileToProlint ""
    &SCOPED-DEFINE PathProlintOutputFile ""
    &SCOPED-DEFINE PathToStartProlintProgram ""
    &SCOPED-DEFINE UserName ""
    &SCOPED-DEFINE PathActualFilePath ""
&ENDIF

&IF DEFINED(FileApplicationName) = 0 &THEN
    &SCOPED-DEFINE FileApplicationName ""
    &SCOPED-DEFINE FileApplicationVersion ""
    &SCOPED-DEFINE FileWorkPackage ""
    &SCOPED-DEFINE FileBugID ""
    /* can either be 0 (or inferior) to prolint the whole file, or a number (or comma separated list of numbers) to prolint between tags */
    &SCOPED-DEFINE FileCorrectionNumber ""
    &SCOPED-DEFINE FileDate ""
    &SCOPED-DEFINE FileCorrectionDescription ""
&ENDIF

&IF DEFINED(ModificationTagOpening) = 0 &THEN
    &SCOPED-DEFINE ModificationTagOpening ""
    &SCOPED-DEFINE ModificationTagEnding ""
&ENDIF

&IF DEFINED(PathDirectoryToProparseAssemblies) = 0 &THEN
    &SCOPED-DEFINE PathDirectoryToProlint ""
    &SCOPED-DEFINE PathDirectoryToProparseAssemblies ""
&ENDIF

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD fi_get_profile Procedure
FUNCTION fi_get_profile RETURNS CHARACTER
    ( INPUT ipc_appliName AS CHARACTER ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD GetSeverityLabel Procedure
FUNCTION GetSeverityLabel RETURNS CHARACTER PRIVATE
    (INPUT ipi_prolintSeverity AS INTEGER) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD btPathGetDirectoryName Procedure
FUNCTION btPathGetDirectoryName RETURNS CHARACTER
    ( INPUT ipc_path AS CHARACTER ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD btPathGetFileName Procedure
FUNCTION btPathGetFileName RETURNS CHARACTER
    ( INPUT ipc_path AS CHARACTER ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* *********************** Procedure Settings ************************ */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
Type: Procedure
Allow:
Frames: 0
Add Fields to: Neither
Other Settings: CODE-ONLY COMPILE
*/
&ANALYZE-RESUME _END-PROCEDURE-SETTINGS

/* *************************  Create Window  ************************** */

&ANALYZE-SUSPEND _CREATE-WINDOW
/* DESIGN Window definition (used by the UIB)
CREATE WINDOW Procedure ASSIGN
HEIGHT             = 15
WIDTH              = 60.
/* END WINDOW DEFINITION */
*/
&ANALYZE-RESUME




&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Procedure


/* ***************************  Main Block  *************************** */

RUN MainProc NO-ERROR.
IF ERROR-STATUS:ERROR THEN
    RETURN ERROR IF (ERROR-STATUS:NUM-MESSAGES > 0) THEN ERROR-STATUS:GET-MESSAGE(1) ELSE RETURN-VALUE.

RETURN "".

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE MainProc Procedure
PROCEDURE MainProc:
    /*------------------------------------------------------------------------------
    Summary    : Main procedure
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE li_tag AS INTEGER NO-UNDO.
    DEFINE VARIABLE li_messageType AS INTEGER NO-UNDO INITIAL 3.
    
    /* add the prolint directory to the propath */
    PROPATH = {&PathDirectoryToProlint} + "," + PROPATH.
    
    /* Make sure to find the assemblies needed for prolint (proparse assemblies) */
    RUN ChangeAssembliesPath (INPUT {&PathDirectoryToProparseAssemblies}) NO-ERROR.
    IF ERROR-STATUS:ERROR THEN
        RETURN ERROR "Error when loading the prolint assemblies : " + RETURN-VALUE.
    
    /* handles the results published by PROLINT */
    SUBSCRIBE TO "Prolint_AddResult" ANYWHERE.
    
    /* start the prolint analysis */
    RUN prolint/core/prolint.p (
        INPUT "",
        INPUT THIS-PROCEDURE:HANDLE,
        INPUT fi_get_profile(INPUT {&FileApplicationName}), /* profile */
        INPUT TRUE
        ).
    
    ASSIGN li_tag = INTEGER({&FileCorrectionNumber}) NO-ERROR.
    PUBLISH "eventToPublishToNotifyTheUserAfterExecution" (
        INPUT (IF li_tag <= 0 THEN "The whole file has been prolinted" ELSE "The modifications between the tags n°" + STRING(li_tag) + " have been prolinted"),
        INPUT li_messageType,
        INPUT "Prolint output",
        INPUT "Auto generated file",
        INPUT 5,
        INPUT "prolintOutputFile"
        ).
    
    RETURN "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE GetFirstLintSource Procedure
PROCEDURE GetFirstLintSource:
    /*------------------------------------------------------------------------------
    Summary    : returns the path of the first file to prolint
    ------------------------------------------------------------------------------*/
    
    DEFINE OUTPUT PARAMETER opc_filePath AS CHARACTER NO-UNDO.
    
    RUN GetNextLintSource(OUTPUT opc_filePath).
    
    RETURN "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE GetNextLintSource Procedure
PROCEDURE GetNextLintSource:
    /*------------------------------------------------------------------------------
    Summary    : returns the path of the next file to prolint (return ? to end the process)
    ------------------------------------------------------------------------------*/
    
    DEFINE OUTPUT PARAMETER opc_filePath AS CHARACTER NO-UNDO INITIAL ?.
    
    ASSIGN
        gi_currentFile = gi_currentFile + 1
        opc_filePath = IF gi_currentFile <= NUM-ENTRIES({&PathFileToProlint}) THEN ENTRY(gi_currentFile, {&PathFileToProlint}) ELSE ?
        .
    
    RETURN "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE Prolint_AddResult Procedure
PROCEDURE Prolint_AddResult:
    /*------------------------------------------------------------------------------
    Summary    : This procedure is called by the outputhandler and aims to store the results of PROLINT
    (it filters the results to keep only the one we want)
    ------------------------------------------------------------------------------*/
    
    DEFINE INPUT PARAMETER pCompilationUnit  AS CHARACTER   NO-UNDO.  /* the sourcefile we're parsing (fullpath) */
    DEFINE INPUT PARAMETER pSourcefile       AS CHARACTER   NO-UNDO.  /* may be an includefile (fullpath)        */
    DEFINE INPUT PARAMETER pLineNumber       AS INTEGER     NO-UNDO.  /* line number in pSourceFile              */
    DEFINE INPUT PARAMETER pDescription      AS CHARACTER   NO-UNDO.  /* human-readable hint                     */
    DEFINE INPUT PARAMETER pRuleID           AS CHARACTER   NO-UNDO.  /* defines rule-program and maps to help   */
    DEFINE INPUT PARAMETER pSeverity         AS INTEGER     NO-UNDO.  /* importance of this rule, scale 0-9      */
    
    DEFINE VARIABLE ll_toUse AS LOGICAL NO-UNDO INITIAL FALSE.
    
    /* Here we can filter the result to actually take it into account... or not  */
    RUN Prolint_FilterResult (
        INPUT pCompilationUnit,
        INPUT pSourcefile,
        INPUT pLineNumber,
        INPUT pDescription,
        INPUT pRuleID,
        INPUT pSeverity,
        OUTPUT ll_toUse
        ).
    
    IF NOT ll_toUse THEN
        RETURN "".
    
    /* 3P log */
    OUTPUT STREAM str_logout TO VALUE({&PathProlintOutputFile}) APPEND BINARY.
    PUT STREAM str_logout UNFORMATTED SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7~t&8",
        pCompilationUnit,
        pSourcefile,
        GetSeverityLabel(pSeverity),
        pLineNumber,
        0,
        pSeverity,
        "rule " + pRuleID + ", " + pDescription,
        ""
        ) SKIP.
    OUTPUT STREAM str_logout CLOSE.
    
    RETURN "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE Prolint_FilterResult Procedure
PROCEDURE Prolint_FilterResult PRIVATE:
    /*------------------------------------------------------------------------------
    Summary    :
    Parameters : <none>
    Returns    :
    Remarks    :
    ------------------------------------------------------------------------------*/
    
    DEFINE INPUT PARAMETER pCompilationUnit  AS CHARACTER NO-UNDO.  /* the sourcefile we're parsing (fullpath) */
    DEFINE INPUT PARAMETER pSourcefile       AS CHARACTER NO-UNDO.  /* may be an includefile (fullpath)        */
    DEFINE INPUT PARAMETER pLineNumber       AS INTEGER   NO-UNDO.  /* line number in pSourceFile            */
    DEFINE INPUT PARAMETER pDescription      AS CHARACTER NO-UNDO.  /* human-readable hint                   */
    DEFINE INPUT PARAMETER pRuleID           AS CHARACTER NO-UNDO.  /* defines rule-program and maps to help */
    DEFINE INPUT PARAMETER pSeverity         AS INTEGER   NO-UNDO.  /* importance of this rule, scale 0-9    */
    DEFINE OUTPUT PARAMETER opl_resultUsed     AS LOGICAL   NO-UNDO INITIAL FALSE.  /* Should return true to use this result, false otherwise */
    
    IF pRuleID = "modificationtag" THEN DO:
        ASSIGN opl_resultUsed = TRUE.
        RETURN "".
    END.
    
    IF NOT CAN-FIND(FIRST tt_rangesToProlint WHERE tt_rangesToProlint.cFileName = pSourcefile) THEN
        RUN Prolint_Differential_SetFilteredRanges (INPUT pCompilationUnit, INPUT pSourcefile).
    
    IF pLineNumber <= 0 OR
        CAN-FIND(FIRST tt_rangesToProlint WHERE tt_rangesToProlint.cFileName = pSourcefile AND
        pLineNumber >= tt_rangesToProlint.iLineBegin AND
        pLineNumber <= tt_rangesToProlint.iLineEnd)
        THEN
        ASSIGN opl_resultUsed = TRUE.
    
    RETURN "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE Prolint_Differential_SetFilteredRanges C-Win
PROCEDURE Prolint_Differential_SetFilteredRanges PRIVATE:
    /*------------------------------------------------------------------------------
    Summary    : This procedure will open the file given as parameter and set
    each line ranges that shoul be prolinted
    Parameters : <none>
    Returns    :
    Remarks    :
    ------------------------------------------------------------------------------*/
    
    DEFINE INPUT PARAMETER pCompilationUnit AS CHARACTER NO-UNDO.  /* the sourcefile we're parsing (fullpath) */
    DEFINE INPUT PARAMETER pSourcefile AS CHARACTER NO-UNDO.  /* may be an includefile (fullpath)        */
    
    DEFINE VARIABLE li_tag AS INTEGER NO-UNDO.
    
    DEFINE VARIABLE lc_line AS CHARACTER NO-UNDO.
    DEFINE VARIABLE li_lineNumber AS INTEGER NO-UNDO INITIAL 0.
    DEFINE VARIABLE li_tagColumn AS INTEGER NO-UNDO.
    
    ASSIGN li_tag = INTEGER({&FileCorrectionNumber}) NO-ERROR.
    IF li_tag > 0 THEN DO:
        
        EMPTY TEMP-TABLE tt_tagBlock.
        
        INPUT STREAM str_source FROM VALUE(pSourcefile).
        REPEAT:
            /* Read next line */
            IMPORT STREAM str_source UNFORMATTED lc_line.
            ASSIGN
                lc_line = TRIM(lc_line, "~r~n ")
                li_lineNumber = li_lineNumber + 1
                .
            
            /* - Deal with non empty lines                          */
            /* - Exclude AppBuilder browse automatic definitions    */
            IF lc_line > "" AND NOT CAPS(lc_line) BEGINS "&SCOPED-DEFINE FIELDS-IN-QUERY-" THEN DO:
                
                /* Does the line owns a begining tag? */
                ASSIGN li_tagColumn = INDEX(lc_line, {&ModificationTagOpening}).
                
                /* Because lc_line has been trimmed, li_tagColumn gives the tag type: */
                /* - "block" if li_tagColumn = 1 */
                /* - "inline" if li_tagColumn > 1 */
                IF li_tagColumn > 1 THEN DO:
                    
                    /* Warn if inline tag found inside block tag */
                    FIND FIRST tt_tagBlock NO-ERROR.
                    IF AVAILABLE(tt_tagBlock) THEN
                        RUN Prolint_AddResult (INPUT pCompilationUnit, INPUT pSourcefile, INPUT li_lineNumber, INPUT "Useless tag (line tag) detected : found inside another block tag starting at line " + STRING(tt_tagBlock.iLineBegin), INPUT "modificationtag", INPUT "-1").
                    
                    RUN Prolint_Differential_AddFileRange (INPUT pSourcefile, INPUT li_lineNumber, INPUT li_lineNumber).
                END.
                ELSE IF li_tagColumn = 1 THEN DO:
                    
                    /* Warn if tag found inside block tag */
                    FIND FIRST tt_tagBlock NO-ERROR.
                    IF AVAILABLE(tt_tagBlock) THEN
                        RUN Prolint_AddResult (INPUT pCompilationUnit, INPUT pSourcefile, INPUT li_lineNumber, INPUT "Invalid opening tag detected : found inside another block tag starting at line " + STRING(tt_tagBlock.iLineBegin), INPUT "modificationtag", INPUT "-1").
                    
                    CREATE tt_tagBlock.
                    ASSIGN
                        tt_tagBlock.iLineBegin = li_lineNumber
                        tt_tagBlock.iLineEnd = ?
                        .
                    RELEASE tt_tagBlock.
                END.
                
                /* Does the line owns an ending tag? */
                ASSIGN li_tagColumn  = INDEX(lc_line, {&ModificationTagEnding}).
                IF li_tagColumn >= 1 THEN DO:
                    FIND LAST tt_tagBlock NO-ERROR.
                    IF NOT AVAILABLE(tt_tagBlock) THEN
                        RUN Prolint_AddResult (INPUT pCompilationUnit, INPUT pSourcefile, INPUT li_lineNumber, INPUT "Invalid ending tag detected : the corresponding opening tag was not found", INPUT "modificationtag", INPUT "-1").
                    ELSE DO:
                        RUN Prolint_Differential_AddFileRange (INPUT pSourcefile, INPUT tt_tagBlock.iLineBegin, INPUT li_lineNumber).
                        DELETE tt_tagBlock.
                    END.
                END.
            END.
        END.
        INPUT STREAM str_source CLOSE.
        
        /* Warn for unclosed tags */
        FOR EACH tt_tagBlock:
            RUN Prolint_AddResult (INPUT pCompilationUnit, INPUT pSourcefile, INPUT tt_tagBlock.iLineBegin, INPUT "Invalid block : the ending tag was not found", INPUT "modificationtag", INPUT "-1").
        END.
    
    END.
    ELSE
        /* we lint the whole file but we warn about this */
        RUN Prolint_Differential_AddFileRange (INPUT pSourcefile, INPUT 1, INPUT 999999999).
    
    RETURN "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE Prolint_Differential_AddFileRange Procedure
PROCEDURE Prolint_Differential_AddFileRange PRIVATE:
    /*------------------------------------------------------------------------------
    Summary    : Flags a portion of a file has elligible to be prolinted
    Parameters : <none>
    Returns    :
    Remarks    :
    ------------------------------------------------------------------------------*/
    
    DEFINE INPUT PARAMETER ipc_filePath AS CHARACTER NO-UNDO.
    DEFINE INPUT PARAMETER ipi_lineStart AS INTEGER NO-UNDO.
    DEFINE INPUT PARAMETER ipi_lineEnd AS INTEGER NO-UNDO.
    
    CREATE tt_rangesToProlint.
    ASSIGN
        tt_rangesToProlint.cFileName = ipc_filePath
        tt_rangesToProlint.iLineBegin = ipi_lineStart
        tt_rangesToProlint.iLineEnd = ipi_lineEnd
        .
    RELEASE tt_rangesToProlint.
    
    RETURN "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE ChangeAssembliesPath Procedure
PROCEDURE ChangeAssembliesPath PRIVATE:
    /*------------------------------------------------------------------------------
    Purpose: This procedure allows to modify the assemblies path dynamically, it is equivalent to
    start a prowin process with the parameter : -assemblies "my path"
    Parameters:
    ipc_newPath = new assemblies path
    ------------------------------------------------------------------------------*/
    
    DEFINE INPUT PARAMETER ipc_newPath AS CHARACTER NO-UNDO.
    
    DEFINE VARIABLE assemblyStore AS Progress.ClrBridge.AssemblyStore NO-UNDO.
    
    assemblyStore = Progress.ClrBridge.AssemblyStore:Instance.
    
    IF LENGTH(ipc_newPath) > 0 THEN DO:
        assemblyStore:AssembliesPath = ipc_newPath NO-ERROR.
        IF ERROR-STATUS:ERROR THEN
            RETURN ERROR-STATUS:GET-MESSAGE(1).
    END.
    
    assemblyStore:Load() NO-ERROR.
    IF ERROR-STATUS:ERROR THEN
        RETURN ERROR-STATUS:GET-MESSAGE(1).
    
    IF VALID-OBJECT(assemblyStore) THEN
        DELETE OBJECT assemblyStore NO-ERROR.
    
    RETURN "".
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* ************************  Function Implementations ***************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getProlintProfileName Procedure
FUNCTION fi_get_profile RETURNS CHARACTER
    ( INPUT ipc_appliName AS CHARACTER ) :
    /*------------------------------------------------------------------------------
    Summary    : returns the prolint profile name in function of the application name,
    it reads from prolint_profiles.d
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE lc_filename AS CHARACTER NO-UNDO.
    DEFINE VARIABLE lc_fullPath AS CHARACTER NO-UNDO.
    DEFINE VARIABLE lc_fileType AS CHARACTER NO-UNDO.
    
    IF ipc_appliName = ? OR ipc_appliName = "" THEN
        RETURN "3P".
    
    INPUT STREAM str_dir FROM OS-DIR({&PathDirectoryToProlint} + "\prolint\settings") .
    REPEAT:
        IMPORT STREAM str_dir lc_filename lc_fullPath lc_fileType.
        IF lc_filename = "." OR lc_filename = ".." THEN
            NEXT.
        
        IF lc_fileType MATCHES "*D*" AND ipc_appliName MATCHES lc_filename THEN
            RETURN lc_filename.
    END.
    INPUT STREAM str_dir CLOSE.
    
    RETURN "3P".

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION GetSeverityLabel Procedure
FUNCTION GetSeverityLabel RETURNS CHARACTER PRIVATE
    (INPUT ipi_prolintSeverity AS INTEGER) :
    /*------------------------------------------------------------------------------
    Summary    : Returns the valid severity label for 3P depending on the severity
    number returned by prolint
    Parameters : <none>
    Returns    :
    Remarks    :
    ------------------------------------------------------------------------------*/
    
    CASE ipi_prolintSeverity:
        WHEN 10 THEN
            RETURN "Critical".
        WHEN 9 THEN
            RETURN "Error".
        WHEN 8 OR
            WHEN 7 THEN
            RETURN "StrongWarning".
        WHEN 6 OR
            WHEN 5 THEN
            RETURN "Warning".
        OTHERWISE
            RETURN "Information".
    END CASE.
    
    RETURN "NoErrors".

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION btPathGetDirectoryName Procedure
FUNCTION btPathGetDirectoryName RETURNS CHARACTER
    ( INPUT ipc_path AS CHARACTER ) :
    /*------------------------------------------------------------------------------
    Summary    :
    Parameters : <none>
    Returns    :
    Remarks    :
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE li_i AS INTEGER NO-UNDO.
    
    ASSIGN
        ipc_path = REPLACE(ipc_path, "~/", "~\")
        li_i = R-INDEX(ipc_path, "~\").
    
    IF li_i = 0 THEN
        RETURN "".
    
    RETURN SUBSTRING(ipc_path, 1, li_i).

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION btPathGetFileName Procedure
FUNCTION btPathGetFileName RETURNS CHARACTER
    ( INPUT ipc_path AS CHARACTER ) :
    /*------------------------------------------------------------------------------
    Summary    :
    Parameters : <none>
    Returns    :
    Remarks    :
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE li_i AS INTEGER NO-UNDO.
    
    ASSIGN
        ipc_path = REPLACE(ipc_path, "~/", "~\")
        li_i = R-INDEX(ipc_path, "~\").
    
    IF li_i = 0 THEN
        RETURN ipc_path.
    
    RETURN SUBSTRING(ipc_path, li_i + 1).

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
