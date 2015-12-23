PROCEDURE pi_handleCompilErrors :
/*------------------------------------------------------------------------------
  Purpose: save any compilation error into a log file (using global stream str_logout)
  Parameters:  <none>
------------------------------------------------------------------------------*/

    DEFINE VARIABLE lc_msg AS CHARACTER NO-UNDO.

    IF COMPILER:ERROR OR COMPILER:WARNING THEN DO:
        IF RETURN-VALUE > "" THEN
            lc_msg = "RETURN-VALUE: " + RETURN-VALUE + "~n".
        IF ERROR-STATUS:NUM-MESSAGES > 0 THEN DO:
            DEFINE VARIABLE li_ AS INTEGER NO-UNDO.
            DO li_ = 1 TO ERROR-STATUS:NUM-MESSAGES:
                lc_msg = lc_msg + "GET-MESSAGE(" + STRING(li_) + "): " + ERROR-STATUS:GET-MESSAGE(li_) + "~n".
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