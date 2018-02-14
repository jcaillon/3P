/*
Author(s) : Julien Caillon (julien.caillon@gmail.com)
This file was created with the 3P :  https://jcaillon.github.io/3P/
*/

/* When executed from 3P, the preprocessed variables below are set to real values */

/* if ExecutionType not already defined */
&IF DEFINED(LogPath) = 0 &THEN
    &SCOPED-DEFINE ExecutionType "DICTIONNARY"
    &SCOPED-DEFINE LogPath "run.log"
    &SCOPED-DEFINE PropathFilePath ""
    &SCOPED-DEFINE DbConnectString ""
    &SCOPED-DEFINE DbLogPath "db.log"
    &SCOPED-DEFINE DbConnectionMandatory FALSE
    &SCOPED-DEFINE NotificationOutputPath "postExecution.notif"
    &SCOPED-DEFINE PreExecutionProgram ""
    &SCOPED-DEFINE PostExecutionProgram ""
    
    &SCOPED-DEFINE CurrentFilePath ""
    &SCOPED-DEFINE OutputPath ""
    
    &SCOPED-DEFINE AnalysisMode FALSE
    &SCOPED-DEFINE ToCompileListFile "files.list"
    &SCOPED-DEFINE CompileProgressionFile "compile.progression"
    
    &SCOPED-DEFINE DatabaseExtractCandoTblType "T,S"
    &SCOPED-DEFINE DatabaseExtractCandoTblName "*"
    &SCOPED-DEFINE DatabaseAliasList ""
&ENDIF
&SCOPED-DEFINE verHigherThan11 INTEGER(ENTRY(1, PROVERSION, '.')) >= 11
&SCOPED-DEFINE CanAnalyse {&AnalysisMode} AND {&verHigherThan11}


/* ***************************  Definitions  ************************** */

DEFINE STREAM str_rw.

DEFINE TEMP-TABLE tt_list NO-UNDO
    FIELD order AS INTEGER
    FIELD source AS CHARACTER
    FIELD outdir AS CHARACTER
    FIELD lis AS CHARACTER
    FIELD xrf AS CHARACTER
    FIELD dbg AS CHARACTER
    FIELD fileid AS CHARACTER
    FIELD reftables AS CHARACTER
    INDEX rtb_idxfld order ASCENDING
    .

DEFINE VARIABLE gi_db AS INTEGER NO-UNDO.
DEFINE VARIABLE gl_dbKo AS LOGICAL NO-UNDO.


/* Used for the unexpected errors in this program */
DEFINE VARIABLE gc_lastError AS CHARACTER NO-UNDO INITIAL "".

/* ***************************  Prototypes  *************************** */

FUNCTION fi_get_message_description RETURNS CHARACTER PRIVATE (INPUT ipi_messNumber AS INTEGER ) FORWARD.
FUNCTION fi_output_last_error RETURNS LOGICAL PRIVATE ( ) FORWARD.
FUNCTION fi_output_last_error_db RETURNS LOGICAL PRIVATE ( ) FORWARD.
FUNCTION fi_add_connec_try RETURNS CHARACTER PRIVATE ( INPUT ipc_conn AS CHARACTER ) FORWARD.
FUNCTION fi_write RETURNS LOGICAL PRIVATE ( INPUT ipc_path AS CHARACTER, INPUT ipc_content AS CHARACTER ) FORWARD.

/* ***************************  Main Block  *************************** */

/* Session options */
SESSION:SYSTEM-ALERT-BOXES = YES.
SESSION:APPL-ALERT-BOXES = YES.

/* Make sure to create this log, otherwise the C# side would think something went wrong */
fi_write(INPUT {&LogPath}, INPUT "").


/* Assign the PROPATH here */
&IF {&verHigherThan11} &THEN
    DEFINE VARIABLE llg_propath AS LONGCHAR NO-UNDO.
    COPY-LOB FROM FILE {&PropathFilePath} TO llg_propath.
    IF LENGTH(llg_propath) > 31190 THEN
        ASSIGN llg_propath = SUBSTRING(llg_propath, 1, 31190 - LENGTH(PROPATH)).
    ASSIGN PROPATH = TRIM(TRIM(STRING(llg_propath)), ",") + "," + PROPATH.
&ELSE
    /* COPY-LOB and LONGCHAR do not exist */
    DEFINE VARIABLE lc_propath AS CHARACTER NO-UNDO.
    INPUT STREAM str_rw FROM VALUE({&PropathFilePath}) NO-ECHO.
    REPEAT:
        IMPORT STREAM str_rw UNFORMATTED lc_propath.
    END.
    INPUT STREAM str_rw CLOSE.
    ASSIGN PROPATH = TRIM(TRIM(lc_propath), ",") + "," + PROPATH.
&ENDIF


/* Connect the database(s) */
&IF {&DbConnectString} > "" &THEN
    CONNECT VALUE(fi_add_connec_try({&DbConnectString})) NO-ERROR.
    IF fi_output_last_error_db() THEN
        ASSIGN gl_dbKo = TRUE.
&ENDIF

/* Create aliases */
&IF {&DatabaseAliasList} > "" &THEN
    REPEAT gi_db = 1 TO NUM-ENTRIES({&DatabaseAliasList}, ";"):
        IF NUM-ENTRIES(ENTRY(gi_db, {&DatabaseAliasList}, ";")) = 2 THEN DO:
            CREATE ALIAS VALUE(ENTRY(1, ENTRY(gi_db, {&DatabaseAliasList}, ";"))) FOR DATABASE VALUE(ENTRY(2, ENTRY(gi_db, {&DatabaseAliasList}, ";"))) NO-ERROR.
            IF fi_output_last_error_db() THEN
                ASSIGN gl_dbKo = TRUE.
        END.
        ELSE
            ASSIGN gc_lastError = gc_lastError + "Invalid ALIAS format, please correct it : " + QUOTER(ENTRY(gi_db, {&DatabaseAliasList}, ";")) + "~n".
    END.
&ENDIF


SUBSCRIBE "eventToPublishToNotifyTheUserAfterExecution" ANYWHERE RUN-PROCEDURE "pi_feedNotification".

/* Pre-execution program */
&IF {&PreExecutionProgram} > "" &THEN
    IF SEARCH({&PreExecutionProgram}) = ? THEN
        ASSIGN gc_lastError = gc_lastError + "Couldn't find the pre-execution program : " + QUOTER({&PreExecutionProgram}) + "~n".
    ELSE DO:
        DO ON STOP UNDO, LEAVE
            ON ERROR UNDO, LEAVE
                ON ENDKEY UNDO, LEAVE
                ON QUIT UNDO, LEAVE:
            RUN VALUE({&PreExecutionProgram}) NO-ERROR.
        END.
        fi_output_last_error().
    END.
&ENDIF

IF NOT {&DbConnectionMandatory} OR NOT gl_dbKo THEN DO:
    
    CASE {&ExecutionType} :
        WHEN "CHECKSYNTAX" OR
        WHEN "COMPILE" OR
        WHEN "GENERATEDEBUGFILE" OR
        WHEN "RUN" THEN DO:
            RUN pi_compileList NO-ERROR.
            fi_output_last_error().
        END.
        WHEN "DEPLOYMENTHOOK" OR
        WHEN "PROLINT" THEN DO:
            RUN VALUE({&CurrentFilePath}) NO-ERROR.
            fi_output_last_error().
        END.
        WHEN "TABLECRC" OR
        WHEN "DATABASE" THEN DO:
            IF NUM-DBS < 1 THEN DO:
                fi_write(INPUT {&DbLogPath}, INPUT "Zero database connected, there is nothing to be done.~n").
                ASSIGN gl_dbKo = TRUE.
            END.
            /* for each connected db */
            REPEAT gi_db = 1 TO NUM-DBS:
                CREATE ALIAS "TPALDB" FOR DATABASE VALUE(LDBNAME(gi_db)) NO-ERROR.
                IF NOT fi_output_last_error() THEN DO:
                    RUN {&CurrentFilePath} (INPUT {&OutputPath}, INPUT LDBNAME(gi_db), INPUT PDBNAME(gi_db), INPUT {&DatabaseExtractCandoTblType}, INPUT {&DatabaseExtractCandoTblName}) NO-ERROR.
                    fi_output_last_error().
                    DELETE ALIAS "TPALDB".
                END.
            END.
        END.
        WHEN "PROVERSION" THEN DO:
            fi_write(INPUT {&OutputPath}, INPUT PROVERSION).
        END.
        WHEN "DATADIGGER" THEN DO:
            RUN DataDigger.p NO-ERROR.
            fi_output_last_error().
        END.
        WHEN "DATAREADER" THEN DO:
            RUN DataReader.p NO-ERROR.
            fi_output_last_error().
        END.
        WHEN "DICTIONARY" THEN DO:
            RUN _dict.p NO-ERROR.
            fi_output_last_error().
        END.
        WHEN "DBADMIN" THEN DO:
            RUN _admin.p NO-ERROR.
            fi_output_last_error().
        END.
        WHEN "PRODESKTOP" THEN DO:
            RUN _desk.p NO-ERROR.
            fi_output_last_error().
        END.
        WHEN "APPBUILDER" THEN DO:
            RUN adeuib/_uibmain.p (INPUT {&CurrentFilePath}) NO-ERROR.
            IF fi_output_last_error() THEN DO:
                RUN _ab.p NO-ERROR.
                IF fi_output_last_error() THEN
                    ASSIGN gc_lastError = gc_lastError + "The following commands both failed : RUN adeuib/_uibmain.p. RUN _ab.p : ~n".
            END.
        END.
    END CASE.
    
END.

IF NOT gl_dbKo THEN
    OS-DELETE VALUE({&DbLogPath}) NO-ERROR.

/* Post-execution program */
&IF {&PostExecutionProgram} > "" &THEN
    IF SEARCH({&PostExecutionProgram}) = ? THEN
        ASSIGN gc_lastError = gc_lastError + "Couldn't find the post-execution program : ~n".
    ELSE DO:
        DO  ON STOP   UNDO, LEAVE
            ON ERROR  UNDO, LEAVE
                ON ENDKEY UNDO, LEAVE
                ON QUIT   UNDO, LEAVE:
            RUN VALUE({&PostExecutionProgram}) NO-ERROR.
        END.
        fi_output_last_error().
    END.
&ENDIF

UNSUBSCRIBE TO "eventToPublishToNotifyTheUserAfterExecution".

/* Delete all aliases */
REPEAT gi_db = 1 TO NUM-ALIASES:
    DELETE ALIAS VALUE(ALIAS(gi_db)).
END.

/* Disconnect all db */
REPEAT gi_db = 1 TO NUM-DBS:
    DISCONNECT VALUE(LDBNAME(gi_db)) NO-ERROR.
END.


IF gc_lastError > "" THEN
    fi_write(INPUT {&LogPath}, INPUT gc_lastError).

/* Must be QUIT or prowin32.exe opens an empty editor! */
QUIT.


/* **********************  Internal Procedures  *********************** */

PROCEDURE pi_handleCompilErrors PRIVATE:
    /*------------------------------------------------------------------------------
    Purpose: save any compilation error into a log file
    Parameters:  <none>
    ------------------------------------------------------------------------------*/
    
    DEFINE INPUT PARAMETER lc_from AS CHARACTER NO-UNDO.
    DEFINE VARIABLE lc_msg AS CHARACTER NO-UNDO INITIAL "".
    
    /* if PROVERSION >= 11 */
    &IF {&verHigherThan11} &THEN
        DEFINE VARIABLE li_i AS INTEGER NO-UNDO.
        IF COMPILER:NUM-MESSAGES > 0 THEN DO:
            DO li_i = 1 TO COMPILER:NUM-MESSAGES:
                ASSIGN lc_msg = lc_msg + SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7~t&8&9",
                    lc_from,
                    COMPILER:GET-FILE-NAME(li_i),
                    IF COMPILER:GET-MESSAGE-TYPE(li_i) = 1 THEN "Critical" ELSE "Warning",
                    COMPILER:GET-ERROR-ROW(li_i),
                    COMPILER:GET-ERROR-COLUMN(li_i),
                    COMPILER:GET-NUMBER(li_i),
                    TRIM(REPLACE(REPLACE(COMPILER:GET-MESSAGE(li_i), "** ", ""), " (" + STRING(COMPILER:GET-NUMBER(li_i)) + ")", "")),
                    fi_get_message_description(INTEGER(COMPILER:GET-NUMBER(li_i))),
                    "~r~n"
                    ).
            END.
        END.
        
        IF ERROR-STATUS:ERROR THEN DO:
            DO li_i = 1 TO ERROR-STATUS:NUM-MESSAGES:
                ASSIGN lc_msg = lc_msg + SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7~t&8&9",
                    lc_from,
                    lc_from,
                    "Critical",
                    0,
                    0,
                    ERROR-STATUS:GET-NUMBER(li_i),
                    TRIM(REPLACE(REPLACE(ERROR-STATUS:GET-MESSAGE(li_i), "** ", ""), " (" + STRING(ERROR-STATUS:GET-NUMBER(li_i)) + ")", "")),
                    "",
                    "~r~n"
                    ).
            END.
        END.
    &ELSE
        IF COMPILER:ERROR OR COMPILER:WARNING OR ERROR-STATUS:ERROR THEN DO:
            IF RETURN-VALUE > "" THEN
                ASSIGN lc_msg = RETURN-VALUE + "~n".
            IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
                DEFINE VARIABLE li_ AS INTEGER NO-UNDO.
                DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                    ASSIGN lc_msg = lc_msg + "(" + STRING(li_) + "): " + ERROR-STATUS:GET-MESSAGE(li_) + "~n".
                END.
            END.
            ASSIGN lc_msg = SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7~t&8&9",
                lc_from,
                COMPILER:FILE-NAME,
                IF COMPILER:ERROR THEN "Critical" ELSE "Warning",
                COMPILER:ERROR-ROW,
                COMPILER:ERROR-COLUMN,
                ?,
                REPLACE(lc_msg, "~n", "<br>"),
                "",
                "~r~n"
                ).
        END.
    &ENDIF
    
    IF lc_msg > "" THEN
        fi_write(INPUT {&OutputPath}, INPUT lc_msg).
    
    ERROR-STATUS:ERROR = NO.
    
    RETURN "".
    
END PROCEDURE.


PROCEDURE pi_compileList PRIVATE:
    /*------------------------------------------------------------------------------
    Purpose: allows to compile all the files listed in the {&ToCompileListFile}
    Parameters:  <none>
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE li_order AS INTEGER NO-UNDO INITIAL 0.
    
    /* loop through all the files to compile */
    INPUT STREAM str_rw FROM VALUE({&ToCompileListFile}) NO-ECHO.
    REPEAT:
        CREATE tt_list.
        ASSIGN
            tt_list.order = li_order
            li_order = li_order + 1.
        IMPORT STREAM str_rw tt_list EXCEPT tt_list.order.
        RELEASE tt_list.
    END.
    INPUT STREAM str_rw CLOSE.
    IF AVAILABLE(tt_list) THEN
        DELETE tt_list.
    
    /* for each file to compile */
    FOR EACH tt_list:
        &IF {&CanAnalyse} &THEN
            /* we don't bother saving/restoring the log-manager state since we are only compiling, there
            should be no *useful* log activated at this moment */
            ASSIGN
                LOG-MANAGER:LOGFILE-NAME = tt_list.fileid
                LOG-MANAGER:LOGGING-LEVEL = 3
                LOG-MANAGER:LOG-ENTRY-TYPES = "FileID"
                .
        &ENDIF
        
        &IF {&ExecutionType} = "RUN" &THEN
            DO  ON STOP   UNDO, LEAVE
                ON ERROR  UNDO, LEAVE
                    ON ENDKEY UNDO, LEAVE
                    ON QUIT   UNDO, LEAVE:
                RUN VALUE(tt_list.source) NO-ERROR.
            END.
        &ELSE
            &IF {&ExecutionType} = "CHECKSYNTAX" &THEN
                COMPILE VALUE(tt_list.source)
                    /* we still save into because if we compile a file and a .r exists next to said file, it
                    doesn't compile... *Sad* */
                    SAVE INTO VALUE(tt_list.outdir)
                    NO-ERROR.
            &ELSE
                /* COMPILE / GENERATEDEBUGFILE */
                IF tt_list.lis = "?" THEN ASSIGN tt_list.lis = ?.
                IF tt_list.xrf = "?" THEN DO:
                    ASSIGN tt_list.xrf = IF NOT {&CanAnalyse} THEN ? ELSE RIGHT-TRIM(ENTRY(1, PROPATH, ","), "~\") + "~\compil.xref".
                END.
                IF tt_list.dbg = "?" THEN ASSIGN tt_list.dbg = ?.
                IF tt_list.xrf = ? OR NOT tt_list.xrf MATCHES "*~~.xml" THEN
                    COMPILE VALUE(tt_list.source)
                        SAVE INTO VALUE(tt_list.outdir)
                        LISTING VALUE(tt_list.lis)
                        XREF VALUE(tt_list.xrf)
                        DEBUG-LIST VALUE(tt_list.dbg)
                        NO-ERROR.
                ELSE
                    COMPILE VALUE(tt_list.source)
                        SAVE INTO VALUE(tt_list.outdir)
                        LISTING VALUE(tt_list.lis)
                        &IF {&verHigherThan11} AND NOT {&CanAnalyse} &THEN
                            XREF-XML VALUE(tt_list.xrf)
                        &ELSE
                            XREF VALUE(tt_list.xrf)
                        &ENDIF
                        DEBUG-LIST VALUE(tt_list.dbg)
                        NO-ERROR.
            &ENDIF
        &ENDIF
        
        &IF {&CanAnalyse} &THEN
            LOG-MANAGER:CLOSE-LOG().
        &ENDIF
        
        fi_output_last_error().
        RUN pi_handleCompilErrors (INPUT tt_list.source) NO-ERROR.
        fi_output_last_error().
        
        &IF {&CanAnalyse} &THEN
            /* Here we generate a file that lists all db.tables + CRC referenced in the .r code produced */
            RUN pi_generateTableRef (INPUT tt_list.source, INPUT tt_list.outdir, INPUT tt_list.xrf, INPUT tt_list.reftables) NO-ERROR.
            fi_output_last_error().
        &ENDIF
        
        /* the following stream / file is used to inform the C# side of the progression */
        fi_write(INPUT {&CompileProgressionFile}, INPUT "x").
    END.
    
    RETURN "".
    
END PROCEDURE.

&IF {&CanAnalyse} &THEN
    PROCEDURE pi_generateTableRef PRIVATE:
        /*------------------------------------------------------------------------------
        Summary    : generate a file that lists all db.tables + CRC referenced in the .r code produced
        Parameters : <none>
        ------------------------------------------------------------------------------*/
        
        DEFINE INPUT PARAMETER ipc_compiledSource AS CHARACTER NO-UNDO.
        DEFINE INPUT PARAMETER ipc_compilationDir AS CHARACTER NO-UNDO.
        DEFINE INPUT PARAMETER ipc_xrefPath AS CHARACTER NO-UNDO.
        DEFINE INPUT PARAMETER ipc_outTableRefPath AS CHARACTER NO-UNDO.
        
        DEFINE VARIABLE li_i AS INTEGER NO-UNDO.
        DEFINE VARIABLE lc_tableList AS CHARACTER NO-UNDO INITIAL "".
        DEFINE VARIABLE lc_crcList AS CHARACTER NO-UNDO INITIAL "".
        DEFINE VARIABLE lc_rcode AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_rcodePath AS CHARACTER NO-UNDO.
        
        ASSIGN
            lc_rcode = ipc_compiledSource
            lc_rcode = SUBSTRING(lc_rcode, R-INDEX(lc_rcode, "~\") + 1)
            lc_rcode = SUBSTRING(lc_rcode, 1, R-INDEX(lc_rcode, ".") - 1)
            lc_rcode = lc_rcode + ".r"
            lc_rcodePath = RIGHT-TRIM(ipc_compilationDir, "~\") + "~\" + lc_rcode
            .
        
        /* The only difficulty is to find the .r code for classes */
        ASSIGN FILE-INFO:FILE-NAME = lc_rcodePath.
        IF FILE-INFO:FILE-TYPE = ? THEN DO:
            
            /* need to find the right .r code in the directories created during compilation */
            RUN pi_findInFolders (INPUT lc_rcode, INPUT ipc_compilationDir) NO-ERROR.
            ASSIGN
                lc_rcodePath = RETURN-VALUE
                FILE-INFO:FILE-NAME = lc_rcodePath.
        END.
        /* Retrieve table list as well as their CRC values */
        IF FILE-INFO:FILE-TYPE <> ? THEN
            ASSIGN
                RCODE-INFO:FILE-NAME = lc_rcodePath
                lc_tableList = TRIM(RCODE-INFO:TABLE-LIST)
                lc_crcList = TRIM(RCODE-INFO:TABLE-CRC-LIST)
                .
        
        DEFINE VARIABLE lc_sourcePath AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_filePath AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_lineNumber AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_xrefType AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_info AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_field AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lh_buff AS HANDLE.
        
        /* okay, since RCODE-INFO:TABLE-LIST doesn't list table references in LIKE TABLE sentences, we HAVE */
        INPUT STREAM str_rw FROM VALUE(ipc_xrefPath) NO-ECHO.
        REPEAT:
            IMPORT STREAM str_rw lc_sourcePath lc_filePath lc_lineNumber lc_xrefType lc_info lc_field NO-ERROR.
            IF lc_xrefType = "REFERENCE" AND lc_info MATCHES "*~~.*" AND LOOKUP(lc_info, lc_tableList) = 0 THEN DO:
                ASSIGN lc_tableList = lc_tableList + "," + lc_info.
                CREATE BUFFER lh_buff FOR TABLE lc_info NO-ERROR.
                IF NOT VALID-HANDLE(lh_buff) THEN
                    ASSIGN lc_crcList = lc_crcList + ",?".
                ELSE DO:
                    ASSIGN lc_crcList = lc_crcList + "," + STRING(lh_buff:CRC-VALUE).
                    DELETE OBJECT lh_buff NO-ERROR.
                END.
            END.
        END.
        INPUT STREAM str_rw CLOSE.
        
        ASSIGN
            lc_crcList = LEFT-TRIM(lc_crcList, ",")
            lc_tableList = LEFT-TRIM(lc_tableList, ",")
            .
        
        /* Store tables referenced in the .R file */
        OUTPUT STREAM str_rw TO VALUE(ipc_outTableRefPath) APPEND BINARY.
        PUT STREAM str_rw UNFORMATTED "".
        REPEAT li_i = 1 TO NUM-ENTRIES(lc_tableList):
            PUT STREAM str_rw UNFORMATTED ENTRY(li_i, lc_tableList) + "~t" + ENTRY(li_i, lc_crcList) SKIP.
        END.
        OUTPUT STREAM str_rw CLOSE.
        
        RETURN "".
        
    END PROCEDURE.
    
    PROCEDURE pi_findInFolders PRIVATE:
        /*------------------------------------------------------------------------------
        Summary    : Allows to find the fullpath of the given file in a given folder (recursively)
        Parameters : <none>
        ------------------------------------------------------------------------------*/
        
        DEFINE INPUT PARAMETER ipc_fileToFind AS CHARACTER NO-UNDO.
        DEFINE INPUT PARAMETER ipc_dir AS CHARACTER NO-UNDO.
        
        DEFINE VARIABLE lc_listSubdir AS CHARACTER NO-UNDO INITIAL "".
        DEFINE VARIABLE li_subDir AS INTEGER NO-UNDO.
        DEFINE VARIABLE lc_listFilesSubDir AS CHARACTER NO-UNDO INITIAL "".
        DEFINE VARIABLE lc_filename AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_fullPath AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_fileType AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_outputFullPath AS CHARACTER NO-UNDO INITIAL "".
        
        INPUT STREAM str_rw FROM OS-DIR(ipc_dir).
        dirRepeat:
        REPEAT:
            IMPORT STREAM str_rw lc_filename lc_fullPath lc_fileType.
            IF lc_filename = "." OR lc_filename = ".." THEN
                NEXT dirRepeat.
            IF lc_filename = ipc_fileToFind THEN DO:
                ASSIGN lc_outputFullPath = lc_fullPath.
                LEAVE dirRepeat.
            END.
            ELSE IF lc_fileType MATCHES "*D*" THEN
                ASSIGN lc_listSubdir = lc_listSubdir + lc_fullPath + ",".
        END.
        INPUT STREAM str_rw CLOSE.
        
        IF lc_outputFullPath > "" THEN
            RETURN lc_outputFullPath.
        
        ASSIGN lc_listSubdir = TRIM(lc_listSubdir, ",").
        DO li_subDir = 1 TO NUM-ENTRIES(lc_listSubdir):
            RUN pi_findInFolders (INPUT ipc_fileToFind, INPUT ENTRY(li_subDir, lc_listSubdir)) NO-ERROR.
            ASSIGN lc_outputFullPath = RETURN-VALUE.
            IF NOT fi_output_last_error() AND lc_outputFullPath > "" THEN
                RETURN lc_outputFullPath.
        END.
        
        RETURN "".
        
    END PROCEDURE.
    
&ENDIF

PROCEDURE pi_feedNotification:
    /*------------------------------------------------------------------------------
    Purpose: called when the associated event is published, allows to display a
    custom notification to the user after executing this program
    Parameters:
    ipc_message = my message content, <b>HTML</b> format! You can also set a <a href='location'>link</a> or whatever you want
    ipi_type = from 0 to 4, to have an icon corresponding to : "MsgOk", "MsgError", "MsgWarning", "MsgInfo", "MsgHighImportance"
    ipc_title = My notification title
    ipc_subtitle = My notification subtitle
    ipi_duration = duration of the notification in seconds (0 for infinite time)
    ipc_uniqueTag = unique name for the notification, if it it set, the notif will close on a click on a link and
    will automatically be closed if another notification with the same name pops up
    ------------------------------------------------------------------------------*/
    
    DEFINE INPUT PARAMETER ipc_message AS CHARACTER NO-UNDO.
    DEFINE INPUT PARAMETER ipi_type AS INTEGER NO-UNDO.
    DEFINE INPUT PARAMETER ipc_title AS CHARACTER NO-UNDO.
    DEFINE INPUT PARAMETER ipc_subtitle AS CHARACTER NO-UNDO.
    DEFINE INPUT PARAMETER ipi_duration AS INTEGER NO-UNDO.
    DEFINE INPUT PARAMETER ipc_uniqueTag AS CHARACTER NO-UNDO.
    
    DEFINE VARIABLE lc_messageType AS CHARACTER NO-UNDO.
    
    CASE ipi_type :
        WHEN 0 THEN lc_messageType = "MsgOk".
        WHEN 1 THEN lc_messageType = "MsgError".
        WHEN 2 THEN lc_messageType = "MsgWarning".
        WHEN 3 THEN lc_messageType = "MsgInfo".
        WHEN 4 THEN lc_messageType = "MsgHighImportance".
    END CASE.
    
    fi_write(INPUT {&NotificationOutputPath}, INPUT SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6&7",
        ipc_message,
        lc_messageType,
        ipc_title,
        ipc_subtitle,
        STRING(ipi_duration),
        ipc_uniqueTag,
        "~r~n"
        )).
    
    RETURN "".
END.


/* ************************  Function Implementations ***************** */

FUNCTION fi_get_message_description RETURNS CHARACTER PRIVATE (INPUT ipi_messNumber AS INTEGER ) :
    /*------------------------------------------------------------------------------
    Purpose: extracts a more detailed error message from the progress help
    Parameters:  ipi_messNumber AS INTEGER
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE cDescription   AS CHARACTER NO-UNDO INITIAL "".
    DEFINE VARIABLE cMsgFile       AS CHARACTER NO-UNDO.
    DEFINE VARIABLE cMsgNumber     AS CHARACTER NO-UNDO FORMAT "x(6)".
    DEFINE VARIABLE cText          AS CHARACTER NO-UNDO FORMAT "x(78)".
    DEFINE VARIABLE cCategory      AS CHARACTER NO-UNDO FORMAT "xx".
    DEFINE VARIABLE cCategoryArray AS CHARACTER NO-UNDO FORMAT "x(30)" EXTENT 7.
    DEFINE VARIABLE cKnowledgeBase AS CHARACTER NO-UNDO FORMAT "X(78)".
    DEFINE VARIABLE iCount         AS INTEGER   NO-UNDO.
    DEFINE VARIABLE iPosition      AS INTEGER   NO-UNDO.
    DEFINE VARIABLE cCategoryIndex AS INTEGER   NO-UNDO.
    
    ASSIGN
        cCategoryArray[1] = "Compiler"
        cCategoryArray[2] = "Database"
        cCategoryArray[3] = "Index"
        cCategoryArray[4] = "Miscellaneous"
        cCategoryArray[5] = "Operating System"
        cCategoryArray[6] = "Program/Execution"
        cCategoryArray[7] = "Syntax"
        iPosition = (ipi_messNumber MODULO 50) WHEN (ipi_messNumber MODULO 50) > 0.
    
    ASSIGN cMsgFile = SEARCH("prohelp/msgdata/msg" + STRING(TRUNCATE((ipi_messNumber - 1) / 50, 0) + 1)) NO-ERROR.
    IF cMsgFile = ? THEN
        RETURN "".
    
    INPUT STREAM str_rw FROM VALUE(cMsgFile) NO-ECHO.
    DO iCount = 1 TO iPosition ON ENDKEY UNDO, LEAVE:
        IMPORT STREAM str_rw cMsgNumber cText cDescription cCategory cKnowledgeBase.
    END.
    INPUT STREAM str_rw CLOSE.
    
    ASSIGN
        cCategoryIndex = LOOKUP(cCategory, "C,D,I,M,O,P,S")
        cDescription = REPLACE(cDescription, "~n", "<br>")
        cDescription = REPLACE(cDescription, "~r", "")
        cDescription = REPLACE(cDescription, "~t", "").
    
    IF INTEGER(cMsgNumber) = ipi_messNumber AND cText <> "Reserved for Seq " THEN DO: /* Process Description */
        IF cDescription BEGINS "syserr" THEN
            ASSIGN cDescription = "An unexpected system error has occurred. Can't say much more.".
        IF cCategoryIndex <> 0 THEN
            ASSIGN cDescription = "(" + cCategoryArray[cCategoryIndex] + ") " + cDescription.
        IF cKnowledgeBase > "" THEN
            ASSIGN cDescription = cDescription + "(" + cKnowledgeBase + ").".
    END.
    
    RETURN cDescription.
    
END FUNCTION.

FUNCTION fi_output_last_error RETURNS LOGICAL PRIVATE ( ) :
    /*------------------------------------------------------------------------------
    Purpose: output the last error encountered
    Notes:
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE li_ AS INTEGER NO-UNDO.
    DEFINE VARIABLE lc_out AS CHARACTER NO-UNDO.
    
    IF ERROR-STATUS:ERROR THEN DO:
        IF RETURN-VALUE > "" THEN
            ASSIGN lc_out = RETURN-VALUE.
        IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                ASSIGN lc_out = "(" + STRING(li_) + "): " + ERROR-STATUS:GET-MESSAGE(li_) + "~n".
            END.
        END.
        fi_write(INPUT {&LogPath}, INPUT lc_out).
        RETURN TRUE.
    END.
    
    ERROR-STATUS:ERROR = NO.
    
    RETURN FALSE.
    
END FUNCTION.

FUNCTION fi_output_last_error_db RETURNS LOGICAL PRIVATE ( ) :
    /*------------------------------------------------------------------------------
    Purpose: output the last error encountered
    Notes:
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE li_ AS INTEGER NO-UNDO.
    DEFINE VARIABLE ll_dbDown AS LOGICAL NO-UNDO.
    
    IF ERROR-STATUS:ERROR THEN DO:
        
        OUTPUT STREAM str_rw TO VALUE({&DbLogPath}) APPEND BINARY.
        
        IF RETURN-VALUE > "" THEN
            PUT STREAM str_rw UNFORMATTED RETURN-VALUE SKIP.
        
        IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                IF ERROR-STATUS:GET-NUMBER(li_) = 1432 THEN
                    ASSIGN ll_dbDown = TRUE.
            END.
            IF ll_dbDown THEN
                PUT STREAM str_rw UNFORMATTED "Failed to connect to the database, check your connection parameters!" SKIP "More details below : " SKIP SKIP.
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                PUT STREAM str_rw UNFORMATTED "(" + STRING(ERROR-STATUS:GET-NUMBER(li_)) + "): " + ERROR-STATUS:GET-MESSAGE(li_) SKIP.
            END.
        END.
        
        OUTPUT STREAM str_rw CLOSE.
        
        RETURN TRUE.
    END.
    
    ERROR-STATUS:ERROR = NO.
    
    RETURN FALSE.
    
END FUNCTION.

FUNCTION fi_add_connec_try RETURNS CHARACTER PRIVATE ( INPUT ipc_conn AS CHARACTER ) :
    /*------------------------------------------------------------------------------
    Purpose: adds a -ct 1 option for each connection
    Notes:
    ------------------------------------------------------------------------------*/
    
    DEFINE VARIABLE lc_conn AS CHARACTER NO-UNDO.
    DEFINE VARIABLE lc_toAdd AS CHARACTER NO-UNDO INITIAL "-ct 1".
    
    ASSIGN
        lc_conn = REPLACE(ipc_conn, "  ", " ")
        lc_conn = REPLACE(lc_conn, "-db", lc_toAdd + " -db")
        lc_conn = IF lc_conn BEGINS (" " + lc_toAdd) THEN SUBSTRING(lc_conn, 7) ELSE lc_conn
        lc_conn = lc_conn + " " + lc_toAdd.
    
    RETURN lc_conn.
    
END FUNCTION.

FUNCTION fi_write RETURNS LOGICAL PRIVATE ( INPUT ipc_path AS CHARACTER, INPUT ipc_content AS CHARACTER ) :
    /*------------------------------------------------------------------------------
    Purpose: adds a -ct 1 option for each connection
    Notes:
    ------------------------------------------------------------------------------*/
    
    OUTPUT STREAM str_rw TO VALUE(ipc_path) APPEND BINARY.
    PUT STREAM str_rw UNFORMATTED ipc_content.
    OUTPUT STREAM str_rw CLOSE.
    
    RETURN TRUE.
    
END FUNCTION.
