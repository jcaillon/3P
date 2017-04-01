/*
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProgressRun.p) is part of 3P.
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
*/

/* When executed from 3P, the preprocessed variables below are set to real values */

/* if ExecutionType not already defined */
&IF DEFINED(LogPath) = 0 &THEN
    &SCOPED-DEFINE ExecutionType "DICTIONNARY"
    &SCOPED-DEFINE LogPath "run.log"
    &SCOPED-DEFINE PropathToUse ""
    &SCOPED-DEFINE DbConnectString ""
    &SCOPED-DEFINE DbLogPath "db.log"
    &SCOPED-DEFINE DbConnectionMandatory FALSE
    &SCOPED-DEFINE NotificationOutputPath "postExecution.notif"
    &SCOPED-DEFINE PreExecutionProgram ""

    &SCOPED-DEFINE CurrentFilePath ""

    &SCOPED-DEFINE ToCompileListFile "files.list"
    &SCOPED-DEFINE CompileProgressionFile "compile.progression"
    &SCOPED-DEFINE CompilationLogPath "compil.log"

    &SCOPED-DEFINE ExtractDbOutputPath ""
&ENDIF


/* ***************************  Definitions  ************************** */

DEFINE STREAM str_w.
DEFINE STREAM str_r.
DEFINE STREAM str_rlist.
DEFINE STREAM str_wlog.
DEFINE STREAM str_wcplog.
DEFINE STREAM str_wdblog.

DEFINE VARIABLE gi_db AS INTEGER NO-UNDO.
DEFINE VARIABLE gl_dbKo AS LOGICAL NO-UNDO.

/* Prototypes */

FUNCTION fi_get_message_description RETURNS CHARACTER PRIVATE (INPUT ipi_messNumber AS INTEGER) FORWARD.
FUNCTION fi_output_last_error RETURNS LOGICAL PRIVATE ( ) FORWARD.
FUNCTION fi_output_last_error_db RETURNS LOGICAL PRIVATE ( ) FORWARD.
FUNCTION fi_add_connec_try RETURNS CHARACTER PRIVATE ( INPUT ipc_conn AS CHARACTER) FORWARD.

/* ***************************  Main Block  *************************** */

/* Critical options!! */
SESSION:SYSTEM-ALERT-BOXES = YES.
SESSION:APPL-ALERT-BOXES = YES.

OUTPUT STREAM str_wdblog TO VALUE({&DbLogPath}) BINARY.
OUTPUT STREAM str_wlog TO VALUE({&LogPath}) BINARY.
PUT STREAM str_wlog UNFORMATTED "".

/* assign the PROPATH here */
ASSIGN PROPATH = TRIM({&PropathToUse} + "," + PROPATH, ",").

/* connect the database(s) */
&IF {&DbConnectString} > "" &THEN
    CONNECT VALUE(fi_add_connec_try({&DbConnectString})) NO-ERROR.
    IF fi_output_last_error_db() THEN
        ASSIGN gl_dbKo = TRUE.
&ENDIF

/* pre execution program */
&IF {&PreExecutionProgram} > "" &THEN
    IF SEARCH({&PreExecutionProgram}) = ? THEN
        PUT STREAM str_wlog UNFORMATTED "Couldn't find the pre-execution program : " + QUOTER({&PreExecutionProgram}) SKIP.
    ELSE DO:
        RUN VALUE({&PreExecutionProgram}) NO-ERROR.
        fi_output_last_error().
    END.
&ENDIF

SUBSCRIBE "eventToPublishToNotifyTheUserAfterExecution" ANYWHERE RUN-PROCEDURE "pi_feedNotification".

IF NOT {&DbConnectionMandatory} OR NOT gl_dbKo THEN DO:

    CASE {&ExecutionType} :
        WHEN "CHECKSYNTAX" OR
        WHEN "COMPILE" OR
        WHEN "GENERATEDEBUGFILE" OR
        WHEN "RUN" THEN DO:
            OUTPUT STREAM str_wcplog TO VALUE({&CompilationLogPath}) BINARY.
            PUT STREAM str_wcplog UNFORMATTED "".

            RUN pi_compileList NO-ERROR.
            fi_output_last_error().

            OUTPUT STREAM str_wcplog CLOSE.
        END.
        WHEN "DEPLOYMENTHOOK" OR
        WHEN "PROLINT" THEN DO:
            RUN VALUE({&CurrentFilePath}) NO-ERROR.
            fi_output_last_error().
        END.
        WHEN "DATABASE" THEN DO:
            IF NUM-DBS < 1 THEN DO:
                PUT STREAM str_wdblog UNFORMATTED "0 database(s) connected! There is nothing to be done." SKIP.
                ASSIGN gl_dbKo = TRUE.
            END.
            /* for each connected db */
            REPEAT gi_db = 1 TO NUM-DBS:
                CREATE ALIAS "DICTDB" FOR DATABASE VALUE(LDBNAME(gi_db)).
                RUN {&CurrentFilePath} (INPUT {&ExtractDbOutputPath}, INPUT LDBNAME(gi_db), INPUT PDBNAME(gi_db)) NO-ERROR.
                fi_output_last_error().
                DELETE ALIAS "DICTDB".
            END.
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
                    PUT STREAM str_wlog UNFORMATTED SKIP "The following commands both failed : RUN adeuib/_uibmain.p. RUN _ab.p" SKIP.
            END.
        END.
    END CASE.

END.

UNSUBSCRIBE TO "eventToPublishToNotifyTheUserAfterExecution".

OUTPUT STREAM str_wlog CLOSE.
OUTPUT STREAM str_wdblog CLOSE.

IF NOT gl_dbKo THEN
    OS-DELETE VALUE({&DbLogPath}).

/* Must be QUIT or prowin32.exe opens an empty editor! */
QUIT.


/* **********************  Internal Procedures  *********************** */

/* if PROVERSION >= 11 */
&IF DECIMAL(SUBSTRING(PROVERSION, 1, INDEX(PROVERSION, "."))) >= 11 &THEN
    PROCEDURE pi_handleCompilErrors PRIVATE:
    /*------------------------------------------------------------------------------
      Purpose: save any compilation error into a log file
      Parameters:  <none>
    ------------------------------------------------------------------------------*/

        DEFINE INPUT PARAMETER lc_from AS CHARACTER NO-UNDO.
        DEFINE VARIABLE li_i AS INTEGER NO-UNDO.

        IF COMPILER:NUM-MESSAGES > 0 THEN DO:
            DO li_i = 1 TO COMPILER:NUM-MESSAGES:
                PUT STREAM str_wcplog UNFORMATTED SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7~t&8",
                lc_from,
                COMPILER:GET-FILE-NAME(li_i),
                IF COMPILER:GET-MESSAGE-TYPE(li_i) = 1 THEN "Critical" ELSE "Warning",
                COMPILER:GET-ERROR-ROW(li_i),
                COMPILER:GET-ERROR-COLUMN(li_i),
                COMPILER:GET-NUMBER(li_i),
                TRIM(REPLACE(REPLACE(COMPILER:GET-MESSAGE(li_i), "** ", ""), " (" + STRING(COMPILER:GET-NUMBER(li_i)) + ")", "")),
                fi_get_message_description(INTEGER(COMPILER:GET-NUMBER(li_i)))
                ) SKIP.
            END.
        END.

        IF ERROR-STATUS:ERROR THEN DO:
            DO li_i = 1 TO ERROR-STATUS:NUM-MESSAGES:
                PUT STREAM str_wcplog UNFORMATTED SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7~t&8",
                lc_from,
                lc_from,
                "Critical",
                0,
                0,
                ERROR-STATUS:GET-NUMBER(li_i),
                TRIM(REPLACE(REPLACE(ERROR-STATUS:GET-MESSAGE(li_i), "** ", ""), " (" + STRING(ERROR-STATUS:GET-NUMBER(li_i)) + ")", "")),
                ""
                ) SKIP.
            END.
        END.

        ERROR-STATUS:ERROR = NO.

        RETURN "".

    END PROCEDURE.
&ELSE
    PROCEDURE pi_handleCompilErrors PRIVATE:
    /*------------------------------------------------------------------------------
      Purpose: save any compilation error into a log file (using global stream str_wcplog)
      Parameters:  <none>
    ------------------------------------------------------------------------------*/

        DEFINE INPUT PARAMETER lc_from AS CHARACTER NO-UNDO.
        DEFINE VARIABLE lc_msg AS CHARACTER NO-UNDO.

        IF COMPILER:ERROR OR COMPILER:WARNING OR ERROR-STATUS:ERROR THEN DO:
            IF RETURN-VALUE > "" THEN
                lc_msg = RETURN-VALUE + "~n".
            IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
                DEFINE VARIABLE li_ AS INTEGER NO-UNDO.
                DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                    lc_msg = lc_msg + "(" + STRING(li_) + "): " + ERROR-STATUS:GET-MESSAGE(li_) + "~n".
                END.
            END.
            lc_msg = SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7~t&8",
                lc_from,
                COMPILER:FILE-NAME,
                IF COMPILER:ERROR THEN "Critical" ELSE "Warning",
                COMPILER:ERROR-ROW,
                COMPILER:ERROR-COLUMN,
                ?,
                REPLACE(lc_msg, "~n", "<br>"),
                ""
                ).
            PUT STREAM str_wcplog UNFORMATTED lc_msg SKIP.
        END.

        ERROR-STATUS:ERROR = NO.

        RETURN "".

    END PROCEDURE.
&ENDIF

PROCEDURE pi_compileList PRIVATE:
/*------------------------------------------------------------------------------
  Purpose: allows to compile all the files listed in the {&ToCompileListFile}
  Parameters:  <none>
------------------------------------------------------------------------------*/

    DEFINE VARIABLE lc_from AS CHARACTER NO-UNDO.
    DEFINE VARIABLE lc_to AS CHARACTER NO-UNDO.
    DEFINE VARIABLE lc_lis AS CHARACTER NO-UNDO.
    DEFINE VARIABLE lc_xrf AS CHARACTER NO-UNDO.
    DEFINE VARIABLE lc_dgb AS CHARACTER NO-UNDO.

    ASSIGN FILE-INFO:FILE-NAME = {&ToCompileListFile}.
    IF FILE-INFO:FILE-TYPE = ? OR NOT FILE-INFO:FILE-TYPE MATCHES("*R*") OR NOT FILE-INFO:FILE-TYPE MATCHES("*F*") THEN
        RETURN ERROR "Can't find the list of files to compile".

    /* loop through all the files to compile */
    INPUT STREAM str_rlist FROM VALUE({&ToCompileListFile}) NO-ECHO.
    REPEAT:
        IMPORT STREAM str_rlist lc_from lc_to lc_lis lc_xrf lc_dgb.
        IF lc_from > "" THEN DO:

            &IF {&ExecutionType} = "RUN" &THEN
                DO  ON STOP   UNDO, LEAVE
                    ON ERROR  UNDO, LEAVE
                    ON ENDKEY UNDO, LEAVE
                    ON QUIT   UNDO, LEAVE:
                    RUN VALUE(lc_from) NO-ERROR.
                END.
            &ELSE
                &IF {&ExecutionType} = "CHECKSYNTAX" &THEN
                    COMPILE VALUE(lc_from)
                        SAVE = FALSE
                        NO-ERROR.
                &ELSE
                    /* COMPILE / GENERATEDEBUGFILE */
                    IF lc_lis = "?" THEN ASSIGN lc_lis = ?.
                    IF lc_xrf = "?" THEN ASSIGN lc_xrf = ?.
                    IF lc_dgb = "?" THEN ASSIGN lc_dgb = ?.
                    IF lc_xrf = ? OR NOT lc_xrf MATCHES "~~.xml" THEN
                        COMPILE VALUE(lc_from)
                            &IF {&ExecutionType} = "GENERATEDEBUGFILE" &THEN
                                SAVE = FALSE                            
                            &ELSE
                                SAVE INTO VALUE(lc_to)
                            &ENDIF
                            LISTING VALUE(lc_lis)
                            XREF VALUE(lc_xrf)
                            DEBUG-LIST VALUE(lc_dgb)
                            NO-ERROR.
                    ELSE
                        COMPILE VALUE(lc_from)
                            &IF {&ExecutionType} = "GENERATEDEBUGFILE" &THEN
                                SAVE = FALSE                            
                            &ELSE
                                SAVE INTO VALUE(lc_to)
                            &ENDIF
                            LISTING VALUE(lc_lis)
                            XREF-XML VALUE(lc_xrf)
                            DEBUG-LIST VALUE(lc_dgb)
                            NO-ERROR.            
                &ENDIF
            &ENDIF
            
            fi_output_last_error().
            RUN pi_handleCompilErrors (INPUT lc_from) NO-ERROR.
            fi_output_last_error().

            /* the following stream / file is used to inform the C# side of the progression */
            OUTPUT STREAM str_w TO VALUE({&CompileProgressionFile}) APPEND BINARY.
            PUT STREAM str_w UNFORMATTED "x".
            OUTPUT STREAM str_w CLOSE.

        END.
    END.
    INPUT STREAM str_rlist CLOSE.

    RETURN "".

END PROCEDURE.

PROCEDURE pi_feedNotification PRIVATE:
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

    OUTPUT STREAM str_w TO VALUE({&NotificationOutputPath}) APPEND BINARY.
    PUT STREAM str_w UNFORMATTED SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6",
        ipc_message,
        lc_messageType,
        ipc_title,
        ipc_subtitle,
        STRING(ipi_duration),
        ipc_uniqueTag
        ) SKIP.
    OUTPUT STREAM str_w CLOSE.

    RETURN "".
END.


/* ************************  Function Implementations ***************** */

FUNCTION fi_get_message_description RETURNS CHARACTER PRIVATE (INPUT ipi_messNumber AS INTEGER) :
/*------------------------------------------------------------------------------
  Purpose: extract a more detailed error message from the progress help
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

    ASSIGN
    cMsgFile = SEARCH("prohelp/msgdata/msg" + STRING(TRUNCATE((ipi_messNumber - 1) / 50, 0) + 1))
    NO-ERROR.

    IF cMsgFile = ? THEN
        RETURN "".

    INPUT STREAM str_r FROM VALUE(cMsgFile) NO-ECHO.
    DO iCount = 1 TO iPosition ON ENDKEY UNDO, LEAVE:
        IMPORT STREAM str_r cMsgNumber cText cDescription cCategory cKnowledgeBase.
    END.
    INPUT STREAM str_r CLOSE.

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
        IF (cKnowledgeBase GT "") EQ TRUE THEN
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

    IF ERROR-STATUS:ERROR THEN DO:
        IF RETURN-VALUE > "" THEN
            PUT STREAM str_wlog UNFORMATTED RETURN-VALUE SKIP.
        IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                PUT STREAM str_wlog UNFORMATTED "(" + STRING(li_) + "): " + ERROR-STATUS:GET-MESSAGE(li_) SKIP.
            END.
        END.
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

        IF RETURN-VALUE > "" THEN
            PUT STREAM str_wdblog UNFORMATTED RETURN-VALUE SKIP.

        IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                IF ERROR-STATUS:GET-NUMBER(li_) = 1432 THEN
                    ASSIGN ll_dbDown = TRUE.
            END.
            IF ll_dbDown THEN
                PUT STREAM str_wdblog UNFORMATTED "Failed to connect to the database, check your connection parameters!" SKIP "More details below : " SKIP SKIP.
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                PUT STREAM str_wdblog UNFORMATTED "(" + STRING(ERROR-STATUS:GET-NUMBER(li_)) + "): " + ERROR-STATUS:GET-MESSAGE(li_) SKIP.
            END.
        END.
        RETURN TRUE.
    END.
    ERROR-STATUS:ERROR = NO.
    RETURN FALSE.

END FUNCTION.

FUNCTION fi_add_connec_try RETURNS CHARACTER PRIVATE ( INPUT ipc_conn AS CHARACTER) :
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
