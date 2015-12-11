/*
&SCOPED-DEFINE ExecutionType "DATABASE"
&SCOPED-DEFINE ToCompile "dumpDatabase.p"
&SCOPED-DEFINE CompilePath "D:\Work\ProgressFiles\compiler\compile\"
&SCOPED-DEFINE LogFile "D:\Work\ProgressFiles\compiler\compile\sc80lbeq.log"
&SCOPED-DEFINE LstFile "D:\Work\ProgressFiles\compiler\compile\sc80lbeq.lst"
&SCOPED-DEFINE ExtractDbOutputPath "D:\Work\ProgressFiles\compiler\compile\outdb.txt"
&SCOPED-DEFINE propathToUse "D:\Work\ProgressFiles\compiler\compile,,"
*/
DEFINE STREAM str_logout.
DEFINE VARIABLE gi_db AS INTEGER NO-UNDO.

FUNCTION fi_get_message_description RETURNS CHARACTER
    ( INPUT piMsgNum AS INTEGER )FORWARD.

PROPATH = PROPATH + "," + {&propathToUse}.

FILE-INFORMATION:FILE-NAME = "base.pf".
IF FILE-INFORMATION:FULL-PATHNAME <> ? THEN
    CONNECT -pf "base.pf" NO-ERROR.
    
FILE-INFORMATION:FILE-NAME = "extra.pf".
IF FILE-INFORMATION:FULL-PATHNAME <> ? THEN
    CONNECT -pf "extra.pf" NO-ERROR.
    
OUTPUT STREAM str_logout TO VALUE({&LogFile}) BINARY.
PUT STREAM str_logout UNFORMATTED "".

CASE {&ExecutionType} :
    WHEN "COMPILE" THEN DO:
        COMPILE VALUE({&ToCompile})
        SAVE=TRUE INTO VALUE({&CompilePath})
        DEBUG-LIST VALUE({&LstFile})
        NO-ERROR.
        RUN pi_handleCompilErrors NO-ERROR.
    END.
    WHEN "RUN" THEN DO:
        DO  ON STOP   UNDO, LEAVE
            ON ERROR  UNDO, LEAVE
            ON ENDKEY UNDO, LEAVE
            ON QUIT   UNDO, LEAVE:
            RUN VALUE({&ToCompile}).
        END.
        RUN pi_handleCompilErrors NO-ERROR.
    END.
    
    WHEN "DATABASE" THEN DO:
        /* for each connected db */
        REPEAT gi_db = 1 TO NUM-DBS:
            CREATE ALIAS "DICTDB" FOR DATABASE VALUE(LDBNAME(gi_db)).
            RUN DumpDatabase.p (INPUT {&ExtractDbOutputPath}, INPUT LDBNAME(gi_db), INPUT PDBNAME(gi_db)).
            DELETE ALIAS "DICTDB".
        END.
    END.
END CASE.

RETURN "".

PROCEDURE pi_handleCompilErrors :
/*------------------------------------------------------------------------------
  Purpose: save any compilation error into a log file (using global stream str_logout)
  Parameters:  <none>
------------------------------------------------------------------------------*/

    DEFINE VARIABLE lc_msg AS CHARACTER NO-UNDO.
    DEFINE VARIABLE li_i AS INTEGER NO-UNDO.
    DEFINE VARIABLE li_ret AS INTEGER NO-UNDO.
    DEFINE VARIABLE lc_listerreurs AS CHARACTER NO-UNDO.

    IF COMPILER:NUM-MESSAGES > 0 THEN DO:
        ASSIGN li_i = 1.
        DO WHILE li_i <= COMPILER:NUM-MESSAGES:
            /* dont display general messages that do not target a specific line more than once */
            IF COMPILER:GET-ERROR-ROW(li_i) <> ? OR LOOKUP("#" + STRING(COMPILER:GET-NUMBER(li_i)) + "#", lc_listerreurs) = 0 THEN DO:
                /* remember the message number */
                IF COMPILER:GET-ERROR-ROW(li_i) = ? THEN
                    lc_listerreurs = lc_listerreurs + "#" + STRING(COMPILER:GET-NUMBER(li_i)) + "#".
                PUT STREAM str_logout UNFORMATTED SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7",
                COMPILER:GET-FILE-NAME(li_i),
                IF COMPILER:GET-MESSAGE-TYPE(li_i) = 2 THEN "Warning" ELSE "Critical",
                COMPILER:GET-ERROR-ROW(li_i),
                COMPILER:GET-ERROR-COLUMN(li_i),
                COMPILER:GET-NUMBER(li_i),
                TRIM(REPLACE(REPLACE(COMPILER:GET-MESSAGE(li_i), "** ", ""), " (" + STRING(COMPILER:GET-NUMBER(li_i)) + ")", "")),
                fi_get_message_description(INTEGER(COMPILER:GET-NUMBER(li_i)))
                ) SKIP.
            END.
            ASSIGN li_i = li_i + 1.
        END.
    END.

    OUTPUT STREAM str_logout CLOSE.

    RETURN "".

END PROCEDURE.

FUNCTION fi_get_message_description RETURNS CHARACTER (INPUT piMsgNum AS INTEGER):
/*------------------------------------------------------------------------------
  Purpose: extract a more detailed error message from the progress help
  Parameters:  piMsgNum AS INTEGER
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
    iPosition = (piMsgNum MODULO 50) WHEN (piMsgNum MODULO 50) > 0
    cMsgFile = SEARCH("prohelp/msgdata/msg" + STRING(TRUNCATE((piMsgNum - 1) / 50, 0) + 1)).

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

    IF INTEGER(cMsgNumber) = piMsgNum AND cText <> "Reserved for Seq " THEN DO: /* Process Description */
        IF cDescription BEGINS "syserr" THEN
            ASSIGN cDescription = "An unexpected system error has occurred. Can't say much more.".
        IF cCategoryIndex <> 0 THEN
            ASSIGN cDescription = "(" + cCategoryArray[cCategoryIndex] + ") " + cDescription.
        IF (cKnowledgeBase GT "") EQ TRUE THEN
            ASSIGN cDescription = cDescription + "(" + cKnowledgeBase + ").".
    END.

    RETURN cDescription.

END FUNCTION.