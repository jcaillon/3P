/*
	Author(s) : Julien Caillon (julien.caillon@gmail.com)
	This file was created with the 3P :  https://jcaillon.github.io/3P/
*/

DEFINE INPUT PARAMETER gc_FileName AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER ipc_baseName AS CHARACTER NO-UNDO. /* */
DEFINE INPUT PARAMETER ipc_physicalName AS CHARACTER NO-UNDO. /* */

DEFINE VARIABLE gc_sep AS CHARACTER NO-UNDO INITIAL "~t".

DEFINE STREAM str_out.
OUTPUT STREAM str_out TO VALUE(gc_FileName) APPEND BINARY.

FUNCTION fi_subst RETURNS CHARACTER(gc_text AS CHARACTER) FORWARD.
    
/* Report meta-information */
PUT STREAM str_out UNFORMATTED "#<Table name>|<Table CRC>" SKIP.

/* Write table information */
/* Format is: <Table name>|<Table CRC> */
FOR EACH DICTDB._FILE NO-LOCK WHERE NOT DICTDB._FILE._HIDDEN AND DICTDB._FILE._Tbl-Type = "T":
    PUT STREAM str_out UNFORMATTED
        ipc_baseName + "." + TRIM(fi_subst(DICTDB._FILE._FILE-NAME)) + gc_sep +
        fi_subst(STRING(DICTDB._FILE._CRC))
        SKIP.
END.

OUTPUT STREAM str_out CLOSE.

RETURN "".

FUNCTION fi_subst RETURNS CHARACTER(gc_text AS CHARACTER):
    RETURN (IF gc_text <> ? THEN REPLACE(REPLACE(REPLACE(gc_text, CHR(9), ""), CHR(10), ""), CHR(13), "") ELSE "?").
END FUNCTION.
