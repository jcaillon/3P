
/* if ExecutionType not already defined */
&IF DEFINED(ExecutionType) = 0 &THEN 
    &SCOPED-DEFINE ExecutionType "DATABASE"
    &SCOPED-DEFINE ToExecute "dumpDatabase.p"
    &SCOPED-DEFINE TempDir "D:\Work\ProgressFiles\compiler\compile\"
    &SCOPED-DEFINE LogFile "D:\Work\ProgressFiles\compiler\compile\sc80lbeq.log"
    &SCOPED-DEFINE LstFile "D:\Work\ProgressFiles\compiler\compile\sc80lbeq.lst"
    &SCOPED-DEFINE ExtractDbOutputPath "D:\Work\ProgressFiles\compiler\compile\outdb.txt"
    &SCOPED-DEFINE propathToUse "D:\Work\ProgressFiles\compiler\compile,,"
    &SCOPED-DEFINE ExtraPf ""
    &SCOPED-DEFINE BasePfPath ""
    &SCOPED-DEFINE BaseIniPath ""
&ENDIF

DEFINE STREAM str_logout.
DEFINE VARIABLE gi_db AS INTEGER NO-UNDO.

FUNCTION fi_get_message_description RETURNS CHARACTER ( INPUT ipi_messNumber AS INTEGER) FORWARD.
FUNCTION fi_output_last_error RETURNS LOGICAL ( ) FORWARD.

/* Critical options!! */
SESSION:SYSTEM-ALERT-BOXES = YES.
SESSION:APPL-ALERT-BOXES = YES.

OUTPUT STREAM str_logout TO VALUE({&LogFile}) BINARY.
PUT STREAM str_logout UNFORMATTED "".

/* load the .ini file */
IF {&BaseIniPath} > "" THEN DO:
    LOAD {&BaseIniPath} NO-ERROR.
    fi_output_last_error().

    USE {&BaseIniPath} NO-ERROR.
    fi_output_last_error().
END.

/* correct the PROPATH here */
ASSIGN PROPATH = {&propathToUse}.

/* connect the database(s) */
IF {&BasePfPath} > "" THEN DO:
    CONNECT -pf {&BasePfPath} -ct 2 NO-ERROR.
    fi_output_last_error().
END.

IF {&ExtraPf} > "" THEN DO:
    CONNECT VALUE({&ExtraPf}) -ct 2 NO-ERROR.
    fi_output_last_error().
END.

CASE {&ExecutionType} :
    WHEN "CHECKSYNTAX" OR
    WHEN "COMPILE" THEN DO:
        COMPILE VALUE({&ToExecute})
            SAVE=TRUE INTO VALUE({&TempDir})
            DEBUG-LIST VALUE({&LstFile})
            NO-ERROR.
        fi_output_last_error().
        RUN pi_handleCompilErrors NO-ERROR.
        fi_output_last_error().
    END.
    WHEN "RUN" THEN DO:
        DO  ON STOP   UNDO, LEAVE
            ON ERROR  UNDO, LEAVE
            ON ENDKEY UNDO, LEAVE
            ON QUIT   UNDO, LEAVE:
            RUN VALUE({&ToExecute}) NO-ERROR.
        END.
        fi_output_last_error().
        RUN pi_handleCompilErrors NO-ERROR.
        fi_output_last_error().
    END.
    WHEN "DATABASE" THEN DO:
        /* for each connected db */
        REPEAT gi_db = 1 TO NUM-DBS:
            CREATE ALIAS "DICTDB" FOR DATABASE VALUE(LDBNAME(gi_db)).
            RUN {&ToExecute} (INPUT {&ExtractDbOutputPath}, INPUT LDBNAME(gi_db), INPUT PDBNAME(gi_db)).
            DELETE ALIAS "DICTDB".
        END.
    END.
END CASE.

OUTPUT STREAM str_logout CLOSE.

/* Must be QUIT or prowin32.exe opens an empty editor! */
QUIT.

/* if PROVERSION >= 11 */
&IF DECIMAL(SUBSTRING(PROVERSION, 1, INDEX(PROVERSION, "."))) >= 11 &THEN
    PROCEDURE pi_handleCompilErrors :
    /*------------------------------------------------------------------------------
      Purpose: save any compilation error into a log file
      Parameters:  <none>
    ------------------------------------------------------------------------------*/

        DEFINE VARIABLE lc_msg AS CHARACTER NO-UNDO.
        DEFINE VARIABLE li_i AS INTEGER NO-UNDO.
        DEFINE VARIABLE li_ret AS INTEGER NO-UNDO.

        IF COMPILER:NUM-MESSAGES > 0 THEN DO:
            ASSIGN li_i = 1.
            DO WHILE li_i <= COMPILER:NUM-MESSAGES:
                PUT STREAM str_logout UNFORMATTED SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7",
                COMPILER:GET-FILE-NAME(li_i),
                IF COMPILER:GET-MESSAGE-TYPE(li_i) = 2 THEN "Warning" ELSE "Critical",
                COMPILER:GET-ERROR-ROW(li_i),
                COMPILER:GET-ERROR-COLUMN(li_i),
                COMPILER:GET-NUMBER(li_i),
                TRIM(REPLACE(REPLACE(COMPILER:GET-MESSAGE(li_i), "** ", ""), " (" + STRING(COMPILER:GET-NUMBER(li_i)) + ")", "")),
                fi_get_message_description(INTEGER(COMPILER:GET-NUMBER(li_i)))
                ) SKIP.
                ASSIGN li_i = li_i + 1.
            END.
        END.

        RETURN "".

    END PROCEDURE.
&ELSE
    PROCEDURE pi_handleCompilErrors :
    /*------------------------------------------------------------------------------
      Purpose: save any compilation error into a log file (using global stream str_logout)
      Parameters:  <none>
    ------------------------------------------------------------------------------*/

        DEFINE VARIABLE lc_msg AS CHARACTER NO-UNDO.

        IF COMPILER:ERROR OR COMPILER:WARNING THEN DO:
            IF RETURN-VALUE > "" THEN
                lc_msg = RETURN-VALUE + "~n".
            IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
                DEFINE VARIABLE li_ AS INTEGER NO-UNDO.
                DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                    lc_msg = lc_msg + "(" + STRING(li_) + "): " + ERROR-STATUS:GET-MESSAGE(li_) + "~n".
                END.
            END.
        
            lc_msg = SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7",
                COMPILER:FILE-NAME,
                IF COMPILER:ERROR THEN "Critical" ELSE "Warning",
                COMPILER:ERROR-ROW,
                COMPILER:ERROR-COLUMN,
                ?,
                REPLACE(lc_msg, "~n", "<br>"),
                ""
                ).
            PUT STREAM str_logout UNFORMATTED lc_msg SKIP.
        END.

        RETURN "".

    END PROCEDURE.
&ENDIF

FUNCTION fi_get_message_description RETURNS CHARACTER (INPUT ipi_messNumber AS INTEGER):
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
    iPosition = (ipi_messNumber MODULO 50) WHEN (ipi_messNumber MODULO 50) > 0
    cMsgFile = SEARCH("prohelp/msgdata/msg" + STRING(TRUNCATE((ipi_messNumber - 1) / 50, 0) + 1)).

    IF cMsgFile = ? THEN
        RETURN "".

    INPUT FROM VALUE(cMsgFile) NO-ECHO.
    DO iCount = 1 TO iPosition ON ENDKEY UNDO, LEAVE:
        IMPORT cMsgNumber cText cDescription cCategory cKnowledgeBase.
    END.
    INPUT CLOSE.

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

FUNCTION fi_output_last_error RETURNS LOGICAL ( ) :
/*------------------------------------------------------------------------------
  Purpose: output the last error encountered
    Notes:
------------------------------------------------------------------------------*/

    DEFINE VARIABLE li_ AS INTEGER NO-UNDO.
    DEFINE VARIABLE ll_dbDown AS LOGICAL NO-UNDO.

    IF ERROR-STATUS:ERROR THEN DO:

        IF RETURN-VALUE > "" THEN
            PUT STREAM str_logout UNFORMATTED RETURN-VALUE SKIP.

        IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                IF ERROR-STATUS:GET-NUMBER(li_) = 1432 THEN
                    ASSIGN ll_dbDown = TRUE.
            END.
            IF ll_dbDown THEN
                PUT STREAM str_logout UNFORMATTED "Failed to connect to the database, check your connection parameters, more details below : " SKIP SKIP.
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                PUT STREAM str_logout UNFORMATTED "(" + STRING(li_) + "): " + ERROR-STATUS:GET-MESSAGE(li_) SKIP.
            END.
        END.
        RETURN TRUE.
    END.
    ERROR-STATUS:ERROR = NO.
    RETURN FALSE.

END FUNCTION.