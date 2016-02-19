/*
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (StartProlint.p.cs) is part of 3P.
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

Credits to SMarmotte Labs for the original dump program 
*/

DEFINE INPUT PARAMETER gc_FileName      AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER ipc_baseName     AS CHARACTER NO-UNDO. /* */
DEFINE INPUT PARAMETER ipc_physicalName AS CHARACTER NO-UNDO. /* */

DEFINE VARIABLE gc_sep            AS CHARACTER    NO-UNDO     INITIAL "~t".
DEFINE VARIABLE gc_fieldList      AS CHARACTER    NO-UNDO     INITIAL "".
DEFINE VARIABLE gc_champIndex   AS CHARACTER    NO-UNDO     INITIAL "".
DEFINE VARIABLE gc_champPK      AS CHARACTER    NO-UNDO     INITIAL "".

DEFINE STREAM str_out.
OUTPUT STREAM str_out TO VALUE(gc_FileName) APPEND BINARY.

FUNCTION fi_subst RETURNS CHARACTER(gc_text AS CHARACTER) FORWARD.
    
/* Report meta-information */
/* Format is: H|<Dump date ISO 8601>|<Dump time>|<Logical DB name>|<Physical DB name>|<Progress version> */
PUT STREAM str_out UNFORMATTED "#H|<Dump date ISO 8601>|<Dump time>|<Logical DB name>|<Physical DB name>|<Progress version>" SKIP.
PUT STREAM str_out UNFORMATTED "#S|<Sequence name>|<Sequence num>" SKIP.
PUT STREAM str_out UNFORMATTED "#T|<Table name>|<Table ID>|<Table CRC>|<Dump name>|<Description>" SKIP.
PUT STREAM str_out UNFORMATTED "#X|<Parent table>|<Event>|<Proc name>|<Trigger CRC>" SKIP.
PUT STREAM str_out UNFORMATTED "#I|<Parent table>|<Index name>|<Primary? 0/1>|<Unique? 0/1>|<Index CRC>|<Fileds separated with %>" SKIP.
PUT STREAM str_out UNFORMATTED "#F|<Parent table>|<Field name>|<Type>|<Format>|<Order #>|<Mandatory? 0/1>|<Extent? 0/1>|<Part of index? 0/1>|<Part of PK? 0/1>|<Initial value>|<Desription>" SKIP.

/* Write database info */
PUT STREAM str_out UNFORMATTED    "H" + gc_sep +
    STRING(YEAR(TODAY), "9999") + "." + STRING(MONTH(TODAY), "99") + "." + STRING(DAY(TODAY), "99") + gc_sep +
    STRING(TIME, "HH:MM:SS") + gc_sep +
    TRIM(ipc_baseName) + gc_sep +
    TRIM(ipc_physicalName) + gc_sep +
    PROVERSION
    SKIP.

/* write sequences info */
FOR EACH DICTDB._Sequence NO-LOCK:
    PUT STREAM str_out UNFORMATTED    
        "S" + gc_sep +
        TRIM(fi_subst(DICTDB._Sequence._Seq-Name)) + gc_sep +
        fi_subst(STRING(DICTDB._Sequence._Seq-Num))
        SKIP.
END.

/* Write table information */
/* Format is: T|<Table name>|<Table ID>|<Table CRC>|<Dump name>|<Description> */
FOR EACH DICTDB._FILE NO-LOCK WHERE NOT DICTDB._FILE._HIDDEN AND DICTDB._FILE._Tbl-Type = "T":
    PUT STREAM str_out UNFORMATTED "# ______________________________________________________________" SKIP.
    PUT STREAM str_out UNFORMATTED    
        "T" + gc_sep +
        TRIM(fi_subst(DICTDB._FILE._FILE-NAME)) + gc_sep +
        fi_subst(STRING(DICTDB._FILE._FILE-NUMBER)) + gc_sep +
        fi_subst(STRING(DICTDB._FILE._CRC)) + gc_sep +
        TRIM(fi_subst(DICTDB._FILE._DUMP-NAME)) + gc_sep +
        TRIM(fi_subst(DICTDB._FILE._DESC))
        SKIP.

    /* Write triggers information */
    /* Format is: X|<Parent table>|<Event>|<Proc name>|<Trigger CRC> */
    FOR EACH DICTDB._FILE-TRIG OF DICTDB._FILE NO-LOCK:
        PUT STREAM str_out UNFORMATTED 
            "X" + gc_sep +
            TRIM(fi_subst(DICTDB._FILE._FILE-NAME)) + gc_sep +
            TRIM(fi_subst(DICTDB._FILE-TRIG._EVENT)) + gc_sep +
            TRIM(fi_subst(DICTDB._FILE-TRIG._PROC-NAME)) + gc_sep +
            fi_subst(STRING(DICTDB._FILE-TRIG._TRIG-CRC))
            SKIP.
    END.

    ASSIGN 
        gc_champIndex = ""
        gc_champPK = "".
    
    /* Write index information */
    /* Format is: I|<Parent table>|<Index name>|<Primary? 0/1>|<Unique? 0/1>|<Index CRC>|<Fileds separated with %> */
    FOR EACH DICTDB._INDEX OF DICTDB._FILE NO-LOCK WHERE DICTDB._INDEX._ACTIVE:
        ASSIGN gc_fieldList = "".
        FOR EACH DICTDB._INDEX-FIELD OF DICTDB._INDEX NO-LOCK,
            FIRST DICTDB._FIELD OF DICTDB._INDEX-FIELD NO-LOCK:
            ASSIGN gc_fieldList = gc_fieldList + TRIM(fi_subst(DICTDB._FIELD._FIELD-NAME)) + (IF DICTDB._INDEX-FIELD._ASCENDING THEN "+" ELSE "-") + "%".
        END.
        
        IF RECID(DICTDB._INDEX) = DICTDB._FILE._PRIME-INDEX THEN
            gc_champPK = gc_champPK + gc_fieldList.
        gc_champIndex = gc_champIndex + gc_fieldList.

        PUT STREAM str_out UNFORMATTED    
            "I" + gc_sep +
            TRIM(fi_subst(DICTDB._FILE._FILE-NAME)) + gc_sep +
            TRIM(fi_subst(DICTDB._INDEX._INDEX-NAME)) + gc_sep +
            (IF RECID(DICTDB._INDEX) = DICTDB._FILE._PRIME-INDEX THEN "1" ELSE "0") + gc_sep +
            (IF DICTDB._INDEX._UNIQUE THEN "1" ELSE "0") + gc_sep +
            fi_subst(STRING(DICTDB._INDEX._IDX-CRC)) + gc_sep +
            TRIM(gc_fieldList, " %")
            SKIP.
    END.

    /* Write fields information */
    /* Format is: F|<Parent table>|<Field name>|<Type>|<Format>|<Order #>|<Mandatory? 0/1>|<Extent? 0/1>|<Part of index? 0/1>|<Part of PK? 0/1>|<Initial value>|<Desription> */
    FOR EACH DICTDB._FIELD OF DICTDB._FILE BY _Order:
        PUT STREAM str_out UNFORMATTED 
            "F" + gc_sep +
            TRIM(fi_subst(DICTDB._FILE._FILE-NAME)) + gc_sep +
            TRIM(fi_subst(DICTDB._FIELD._FIELD-NAME)) + gc_sep +
            TRIM(fi_subst(DICTDB._FIELD._DATA-TYPE)) + gc_sep +
            TRIM(fi_subst(DICTDB._FIELD._FORMAT)) + gc_sep +
            fi_subst(STRING(DICTDB._FIELD._ORDER)) + gc_sep +
            (IF DICTDB._FIELD._MANDATORY THEN "1" ELSE "0") + gc_sep +
            (IF DICTDB._FIELD._EXTENT > 0 THEN "1" ELSE "0") + gc_sep +
            (IF INDEX(gc_champIndex, TRIM(fi_subst(DICTDB._FIELD._FIELD-NAME))) > 0 THEN "1" ELSE "0") + gc_sep +
            (IF INDEX(gc_champPK, TRIM(fi_subst(DICTDB._FIELD._FIELD-NAME))) > 0 THEN "1" ELSE "0") + gc_sep +
            TRIM(fi_subst(DICTDB._FIELD._INITIAL)) + gc_sep +
            (IF TRIM(fi_subst(DICTDB._FIELD._DESC)) <> "" THEN TRIM(fi_subst(DICTDB._FIELD._DESC))
            ELSE (IF TRIM(fi_subst(DICTDB._FIELD._LABEL)) <> "" THEN    TRIM(fi_subst(DICTDB._FIELD._LABEL))
                ELSE (IF TRIM(fi_subst(DICTDB._FIELD._HELP)) <> "" THEN TRIM(fi_subst(DICTDB._FIELD._HELP))
                    ELSE TRIM(fi_subst(DICTDB._FIELD._COL-LABEL))
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
