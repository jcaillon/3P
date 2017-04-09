/*
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (DumpTableCrc.p) is part of 3P.
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
