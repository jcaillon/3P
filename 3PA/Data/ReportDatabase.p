/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
/*                                                                                     */
/* This file is part of the ProgressAssist project.                                    */
/* Copyrights(C) 2010, SMarmotte Labs.                                                 */
/*                                                                                     */
/* Description: This program is design to create a readable database report.           */
/* Author:  SMarmotte                                                                  */
/* Created: 2010.07.10                                                                 */
/* Changed: 2010.07.10                                                                 */
/* Version: ---> see variable cVersion <---                                            */
/*                                                                                     */
/* Prerequisites:                                                                      */
/* - The progresssession should have only one database connected.                      */
/* - The output file name is given by the -param "xxxxx" parameter of command line.    */
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

DEFINE VARIABLE cVersion        AS CHARACTER    NO-UNDO     INITIAL "0.10".
DEFINE VARIABLE cSep            AS CHARACTER    NO-UNDO     INITIAL "|".
DEFINE VARIABLE cFileName       AS CHARACTER    NO-UNDO     INITIAL "".
DEFINE VARIABLE cFieldList      AS CHARACTER    NO-UNDO     INITIAL "".
DEFINE STREAM s.


FUNCTION fi_subst RETURNS CHARACTER(p_cText AS CHARACTER) FORWARD.


/* Step 1 - Get output file name */
ASSIGN cFileName = TRIM(SESSION:PARAMETER).
/*
IF cFileName = "" THEN
    MESSAGE "** ERROR ** - Invalid target file name: <" + cFileName + ">." VIEW-AS ALERT-BOX ERROR.
ELSE DO:
    /* Step 2 - Remove previously generated file */
    OS-DELETE VALUE(cFileName).

    /* Step 3 - Ensure there is only one database connected. */
    IF NUM-DBS <> 1 THEN
        MESSAGE "** ERROR ** - There must be only one database connected. Current database count: " + STRING(NUM-DBS) + "." VIEW-AS ALERT-BOX ERROR.
    ELSE
        RUN pi_dumpdatabase.
END.
*/
OS-DELETE VALUE(cFileName).
RUN pi_dumpdatabase.
QUIT.



PROCEDURE pi_dumpdatabase.
    OUTPUT STREAM s TO VALUE(cFileName) BINARY.


    /* File header */
    PUT STREAM s UNFORMATTED "# SMarmotte Labs ProgressAssist. Copyrights (C) 2010-" + STRING(YEAR(TODAY)) + ", SMarmotte Labs." SKIP.
    PUT STREAM s UNFORMATTED "# This file has been generated from ProgressAssist Progress scripts v" + cVersion + "." SKIP.
    PUT STREAM s UNFORMATTED "#" SKIP.


    /* Report meta-information */
    /* Format is: H|<Dump date ISO 8601>|<Dump time>|<Logical DB name>|<Physical DB name>|<Progress version> */
    PUT STREAM s UNFORMATTED    "H" + cSep +
                                STRING(YEAR(TODAY), "9999") + "." + STRING(MONTH(TODAY), "99") + "." + STRING(DAY(TODAY), "99") + cSep +
                                STRING(TIME, "HH:MM:SS") + cSep +
                                TRIM(LDBNAME(1)) + cSep +
                                TRIM(PDBNAME(1)) + cSep +
                                PROVERSION
                                SKIP.


    /* Write table information */
    /* Format is: T|<Table name>|<Table ID>|<Table CRC>|<Dump name>|<Description> */
    FOR EACH _FILE NO-LOCK WHERE NOT _FILE._HIDDEN.
        PUT STREAM s UNFORMATTED    "T" + cSep +
                                    TRIM(fi_subst(_FILE._FILE-NAME)) + cSep +
                                    fi_subst(STRING(_FILE._FILE-NUMBER)) + cSep +
                                    fi_subst(STRING(_FILE._CRC)) + cSep +
                                    TRIM(fi_subst(_FILE._DUMP-NAME)) + cSep +
                                    TRIM(fi_subst(_FILE._DESC))
                                    SKIP.


        /* Write fields information */
        /* Format is: F|<Parent table>|<Field name>|<Type>|<Format>|<Order #>|<Mandatory? 0/1>|<Extent? 0/1>|<Initial value>|<Desription> */
        FOR EACH _FIELD OF _FILE BY _Order:
            PUT STREAM s UNFORMATTED    "F" + cSep +
                                        TRIM(fi_subst(_FILE._FILE-NAME)) + cSep +
                                        TRIM(fi_subst(_FIELD._FIELD-NAME)) + cSep +
                                        TRIM(fi_subst(_FIELD._DATA-TYPE)) + cSep +
                                        TRIM(fi_subst(_FIELD._FORMAT)) + cSep +
                                        fi_subst(STRING(_FIELD._ORDER)) + cSep +
                                        (IF _FIELD._MANDATORY THEN "1" ELSE "0") + cSep +
                                        (IF _FIELD._EXTENT > 0  THEN "1" ELSE "0") + cSep +
                                        (IF _FIELD._INITIAL <> ? THEN TRIM(fi_subst(_FIELD._INITIAL)) ELSE "?") + cSep +
                                        (IF TRIM(fi_subst(_FIELD._DESC)) <> "" THEN TRIM(fi_subst(_FIELD._DESC))
                                        ELSE                                        (IF TRIM(fi_subst(_FIELD._LABEL)) <> "" THEN    TRIM(fi_subst(_FIELD._LABEL))
                                                                                    ELSE                                            (IF TRIM(fi_subst(_FIELD._HELP)) <> "" THEN TRIM(fi_subst(_FIELD._HELP))
                                                                                                                                    ELSE                                        TRIM(fi_subst(_FIELD._COL-LABEL))
                                                                                                                                    )
                                                                                    )
                                        )
                                        SKIP.
        END.


        /* Write triggers information */
        /* Format is: X|<Parent table>|<Event>|<Proc name>|<Trigger CRC> */
        FOR EACH _FILE-TRIG OF _FILE NO-LOCK:
            PUT STREAM s UNFORMATTED    "X" + cSep +
                                        TRIM(fi_subst(_FILE._FILE-NAME)) + cSep +
                                        TRIM(fi_subst(_FILE-TRIG._EVENT)) + cSep +
                                        TRIM(fi_subst(_FILE-TRIG._PROC-NAME)) + cSep +
                                        fi_subst(STRING(_FILE-TRIG._TRIG-CRC))
                                        SKIP.
        END.

        /* Write index information */
        /* Format is: I|<Parent table>|<Index name>|<Primary? 0/1>|<Unique? 0/1>|<Index CRC>|<Fileds separated with %> */
        FOR EACH _INDEX OF _FILE NO-LOCK WHERE _INDEX._ACTIVE:
            ASSIGN cFieldList = "".
            FOR EACH _INDEX-FIELD OF _INDEX NO-LOCK,
                FIRST _FIELD OF _INDEX-FIELD NO-LOCK:
                ASSIGN cFieldList = cFieldList + TRIM(fi_subst(_FIELD._FIELD-NAME)) + (IF _INDEX-FIELD._ASCENDING THEN "+" ELSE "-") + "%".
            END.

            PUT STREAM s UNFORMATTED    "I" + cSep +
                                        TRIM(fi_subst(_FILE._FILE-NAME)) + cSep +
                                        TRIM(fi_subst(_INDEX._INDEX-NAME)) + cSep +
                                        (IF RECID(_INDEX) = _FILE._PRIME-INDEX THEN "1" ELSE "0") + cSep +
                                        (IF _INDEX._UNIQUE THEN "1" ELSE "0") + cSep +
                                        fi_subst(STRING(_INDEX._IDX-CRC)) + cSep +
                                        TRIM(cFieldList, " %")
                                        SKIP.
        END.
    END.


    /* End of file */
    PUT STREAM s UNFORMATTED "<<EOF>>" SKIP.
    OUTPUT STREAM s CLOSE.
    RETURN "".
END PROCEDURE.


FUNCTION fi_subst RETURNS CHARACTER(p_cText AS CHARACTER):
    RETURN (IF p_cText <> ? THEN REPLACE(REPLACE(REPLACE(p_cText, CHR(9), ""), CHR(10), ""), CHR(13), "") ELSE "").
END FUNCTION.
