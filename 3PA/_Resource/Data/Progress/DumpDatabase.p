/*
	Author(s) : Julien Caillon (julien.caillon@gmail.com)
	This file was created with the 3P :  https://jcaillon.github.io/3P/
*/

DEFINE INPUT PARAMETER gc_FileName AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER ipc_baseName AS CHARACTER NO-UNDO. /* */
DEFINE INPUT PARAMETER ipc_physicalName AS CHARACTER NO-UNDO. /* */
DEFINE INPUT PARAMETER ipc_candoTableType AS CHARACTER NO-UNDO. /* */
DEFINE INPUT PARAMETER ipc_candoFileName AS CHARACTER NO-UNDO. /* */

DEFINE VARIABLE gc_sep AS CHARACTER NO-UNDO INITIAL "~t".
DEFINE VARIABLE gc_fieldList AS CHARACTER NO-UNDO INITIAL "".
DEFINE VARIABLE gc_champIndex AS CHARACTER NO-UNDO INITIAL "".
DEFINE VARIABLE gc_champPK AS CHARACTER NO-UNDO INITIAL "".

DEFINE STREAM str_out.
OUTPUT STREAM str_out TO VALUE(gc_FileName) APPEND BINARY.

FUNCTION fi_subst RETURNS CHARACTER(gc_text AS CHARACTER) FORWARD.
    
/* Report meta-information */
/* Format is: H|<Dump date ISO 8601>|<Dump time>|<Logical DB name>|<Physical DB name>|<Progress version> */
PUT STREAM str_out UNFORMATTED "#H|<Dump date ISO 8601>|<Dump time>|<Logical DB name>|<Physical DB name>|<Progress version>" SKIP.
PUT STREAM str_out UNFORMATTED "#S|<Sequence name>|<Sequence num>" SKIP.
PUT STREAM str_out UNFORMATTED "#T|<Table name>|<Table ID>|<Table CRC>|<Dump name>|<Description>|<Hidden? 0/1>|<Frozen? 0/1>|<Table type>" SKIP.
PUT STREAM str_out UNFORMATTED "#X|<Parent table>|<Event>|<Proc name>|<Trigger CRC>" SKIP.
PUT STREAM str_out UNFORMATTED "#I|<Parent table>|<Index name>|<Primary? 0/1>|<Unique? 0/1>|<Index CRC>|<Fileds separated with %>" SKIP.
PUT STREAM str_out UNFORMATTED "#F|<Parent table>|<Field name>|<Type>|<Format>|<Order #>|<Mandatory? 0/1>|<Extent? 0/1>|<Part of index? 0/1>|<Part of PK? 0/1>|<Initial value>|<Desription>" SKIP.

/* Write database info */
PUT STREAM str_out UNFORMATTED "H" + gc_sep +
    STRING(YEAR(TODAY), "9999") + "." + STRING(MONTH(TODAY), "99") + "." + STRING(DAY(TODAY), "99") + gc_sep +
    STRING(TIME, "HH:MM:SS") + gc_sep +
    TRIM(ipc_baseName) + gc_sep +
    TRIM(ipc_physicalName) + gc_sep +
    PROVERSION
    SKIP.

/* write sequences info */
FOR EACH TPALDB._Sequence NO-LOCK:
    PUT STREAM str_out UNFORMATTED    
        "S" + gc_sep +
        TRIM(fi_subst(TPALDB._Sequence._Seq-Name)) + gc_sep +
        fi_subst(STRING(TPALDB._Sequence._Seq-Num))
        SKIP.
END.

/* Write table information */
/* Format is: T|<Table name>|<Table ID>|<Table CRC>|<Dump name>|<Description> */
FOR EACH TPALDB._FILE NO-LOCK WHERE CAN-DO(ipc_candoTableType, TPALDB._FILE._Tbl-Type) AND CAN-DO(ipc_candoFileName, TPALDB._FILE._FILE-NAME):
    PUT STREAM str_out UNFORMATTED "# ______________________________________________________________" SKIP.
    PUT STREAM str_out UNFORMATTED    
        "T" + gc_sep +
        TRIM(fi_subst(TPALDB._FILE._FILE-NAME)) + gc_sep +
        fi_subst(STRING(TPALDB._FILE._FILE-NUMBER)) + gc_sep +
        fi_subst(STRING(TPALDB._FILE._CRC)) + gc_sep +
        TRIM(fi_subst(TPALDB._FILE._DUMP-NAME)) + gc_sep +
        TRIM(fi_subst(TPALDB._FILE._DESC)) + gc_sep +
        (IF TPALDB._FILE._HIDDEN THEN "1" ELSE "0") + gc_sep +
        (IF TPALDB._FILE._Frozen THEN "1" ELSE "0") + gc_sep +
        TRIM(fi_subst(TPALDB._FILE._Tbl-Type))
        SKIP.

    /* Write triggers information */
    /* Format is: X|<Parent table>|<Event>|<Proc name>|<Trigger CRC> */
    FOR EACH TPALDB._FILE-TRIG OF TPALDB._FILE NO-LOCK:
        PUT STREAM str_out UNFORMATTED 
            "X" + gc_sep +
            TRIM(fi_subst(TPALDB._FILE._FILE-NAME)) + gc_sep +
            TRIM(fi_subst(TPALDB._FILE-TRIG._EVENT)) + gc_sep +
            TRIM(fi_subst(TPALDB._FILE-TRIG._PROC-NAME)) + gc_sep +
            fi_subst(STRING(TPALDB._FILE-TRIG._TRIG-CRC))
            SKIP.
    END.

    ASSIGN 
        gc_champIndex = ""
        gc_champPK = "".
    
    /* Write index information */
    /* Format is: I|<Parent table>|<Index name>|<Primary? 0/1>|<Unique? 0/1>|<Index CRC>|<Fields separated with %> */
    FOR EACH TPALDB._INDEX OF TPALDB._FILE NO-LOCK WHERE TPALDB._INDEX._ACTIVE:
        ASSIGN gc_fieldList = "".
        FOR EACH TPALDB._INDEX-FIELD OF TPALDB._INDEX NO-LOCK,
            FIRST TPALDB._FIELD OF TPALDB._INDEX-FIELD NO-LOCK:
            ASSIGN gc_fieldList = gc_fieldList + TRIM(fi_subst(TPALDB._FIELD._FIELD-NAME)) + (IF TPALDB._INDEX-FIELD._ASCENDING THEN "+" ELSE "-") + "%".
        END.
        
        IF RECID(TPALDB._INDEX) = TPALDB._FILE._PRIME-INDEX THEN
            gc_champPK = gc_champPK + gc_fieldList.
        gc_champIndex = gc_champIndex + gc_fieldList.

        PUT STREAM str_out UNFORMATTED    
            "I" + gc_sep +
            TRIM(fi_subst(TPALDB._FILE._FILE-NAME)) + gc_sep +
            TRIM(fi_subst(TPALDB._INDEX._INDEX-NAME)) + gc_sep +
            (IF RECID(TPALDB._INDEX) = TPALDB._FILE._PRIME-INDEX THEN "1" ELSE "0") + gc_sep +
            (IF TPALDB._INDEX._UNIQUE THEN "1" ELSE "0") + gc_sep +
            fi_subst(STRING(TPALDB._INDEX._IDX-CRC)) + gc_sep +
            TRIM(gc_fieldList, " %")
            SKIP.
    END.

    /* Write fields information */
    /* Format is: F|<Parent table>|<Field name>|<Type>|<Format>|<Order #>|<Mandatory? 0/1>|<Extent? 0/1>|<Part of index? 0/1>|<Part of PK? 0/1>|<Initial value>|<Desription> */
    FOR EACH TPALDB._FIELD OF TPALDB._FILE BY _Order:
        PUT STREAM str_out UNFORMATTED 
            "F" + gc_sep +
            TRIM(fi_subst(TPALDB._FILE._FILE-NAME)) + gc_sep +
            TRIM(fi_subst(TPALDB._FIELD._FIELD-NAME)) + gc_sep +
            TRIM(fi_subst(TPALDB._FIELD._DATA-TYPE)) + gc_sep +
            TRIM(fi_subst(TPALDB._FIELD._FORMAT)) + gc_sep +
            fi_subst(STRING(TPALDB._FIELD._ORDER)) + gc_sep +
            (IF TPALDB._FIELD._MANDATORY THEN "1" ELSE "0") + gc_sep +
            (IF TPALDB._FIELD._EXTENT > 0 THEN "1" ELSE "0") + gc_sep +
            (IF INDEX(gc_champIndex, TRIM(fi_subst(TPALDB._FIELD._FIELD-NAME))) > 0 THEN "1" ELSE "0") + gc_sep +
            (IF INDEX(gc_champPK, TRIM(fi_subst(TPALDB._FIELD._FIELD-NAME))) > 0 THEN "1" ELSE "0") + gc_sep +
            TRIM(fi_subst(TPALDB._FIELD._INITIAL)) + gc_sep +
            (IF TRIM(fi_subst(TPALDB._FIELD._DESC)) <> "" THEN TRIM(fi_subst(TPALDB._FIELD._DESC))
            ELSE (IF TRIM(fi_subst(TPALDB._FIELD._LABEL)) <> "" THEN    TRIM(fi_subst(TPALDB._FIELD._LABEL))
                ELSE (IF TRIM(fi_subst(TPALDB._FIELD._HELP)) <> "" THEN TRIM(fi_subst(TPALDB._FIELD._HELP))
                    ELSE TRIM(fi_subst(TPALDB._FIELD._COL-LABEL))
                    )
                )
            )
            SKIP.
    END.
END.

OUTPUT STREAM str_out CLOSE.

RETURN "".

FUNCTION fi_subst RETURNS CHARACTER(gc_text AS CHARACTER):
    RETURN (IF gc_text <> ? THEN REPLACE(REPLACE(REPLACE(gc_text, CHR(9), ""), CHR(10), ""), CHR(13), "") ELSE "?").
END FUNCTION.
