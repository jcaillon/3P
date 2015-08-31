/* ***************************  Definitions  ************************** */

/* Parameters Definitions ---                                           */

/* Local Variable Definitions ---                                       */
DEFINE VARIABLE gc_program          AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_typeTrait        AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_proj             AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_env              AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_notepadPath      AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_NpptoolPath      AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_tempPath         AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_baseFileName     AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_Trig             AS CHARACTER NO-UNDO.
DEFINE VARIABLE gc_SynUser          AS CHARACTER NO-UNDO.
DEFINE VARIABLE gi_tmaprogressUp    AS INTEGER NO-UNDO. /*  */
DEFINE VARIABLE gi_prolintUp        AS INTEGER NO-UNDO. /*  */

DEFINE STREAM gs_inFile.
DEFINE STREAM gs_outFile.

RUN pi_main NO-ERROR.

QUIT.

/* **********************  Internal Procedures  *********************** */


/* API WINDOWS */
PROCEDURE WinExec EXTERNAL "kernel32.dll":u:
/*------------------------------------------------------------------------------
  Purpose:
  Parameters:  <none>
  Notes:
------------------------------------------------------------------------------*/
    DEF INPUT PARAMETER pProgName AS CHAR.
    DEF INPUT PARAMETER pStyle AS LONG.
    DEF RETURN PARAMETER pResult AS LONG.

END PROCEDURE.

PROCEDURE Sleep EXTERNAL "KERNEL32":
    DEFINE INPUT PARAMETER lMilliseconds            AS LONG         NO-UNDO.
END PROCEDURE.


FUNCTION fi_readRegValue RETURNS CHARACTER
    (PBASEKEY AS CHAR, /* I.E. "HKEY_..." */
    PKEYNAME AS CHAR, /* MAIN KEY, I.E. "SOFTWARE\ACME..." */
    PSECNAME AS CHAR, /* SECTION */
    PITEM AS CHAR /* ITEM IDENTIFIER, "" = RETURN LIST, ? = DEFAULT */
    ) :
/*------------------------------------------------------------------------------
  Purpose: Read a value from any section of the registry
  Parameters:  <none>
  Notes: Windows only
------------------------------------------------------------------------------*/

    DEF VAR IVALUE AS CHAR NO-UNDO.

    LOAD PKEYNAME
    BASE-KEY PBASEKEY NO-ERROR.

    IF NOT ERROR-STATUS:ERROR THEN
    DO:
        USE PKEYNAME.
        IF PITEM = ? THEN
            GET-KEY-VALUE SECTION PSECNAME KEY DEFAULT VALUE IVALUE.
        ELSE
            GET-KEY-VALUE SECTION PSECNAME KEY PITEM VALUE IVALUE.
        IF IVALUE = ? THEN
        IVALUE = "".
        UNLOAD PKEYNAME NO-ERROR.
    END. /* IF NO ERROR*/

    RETURN IVALUE.

END FUNCTION.


PROCEDURE pi_main :
/*------------------------------------------------------------------------------
  Purpose:
  Parameters:  <none>
  Notes:
------------------------------------------------------------------------------*/

    DEFINE VARIABLE ll_success AS LOGICAL NO-UNDO.
    DEFINE VARIABLE lc_propath AS CHARACTER NO-UNDO.
    DEFINE VARIABLE li_ret AS INTEGER NO-UNDO.
    DEFINE VARIABLE lc_commande_dos AS CHARACTER NO-UNDO.

    SESSION:SYSTEM-ALERT-BOXES = YES.
    SESSION:APPL-ALERT-BOXES = YES.

    IF NUM-ENTRIES(SESSION:PARAMETER) <> 4 THEN DO:
        MESSAGE "Oops, 4 parameters are expected!" VIEW-AS ALERT-BOX ERROR BUTTONS OK.
        RETURN "".
    END.

    /* get the environnement we are currently working on (from PA in the windows reg) */
    ASSIGN
        gc_tempPath = DYNAMIC-FUNCTION("fi_readRegValue", "HKEY_CURRENT_USER", "Software\NppTool", "", "tempPath")
        gc_notepadPath = DYNAMIC-FUNCTION("fi_readRegValue", "HKEY_CURRENT_USER", "Software\NppTool", "", "notepadPath")
        gc_NpptoolPath = DYNAMIC-FUNCTION("fi_readRegValue", "HKEY_CURRENT_USER", "Software\NppTool", "", "scriptLoc")
        gc_proj = DYNAMIC-FUNCTION("fi_readRegValue", "HKEY_CURRENT_USER", "Software\ProgressAssist", "", "LastUsedEnvironment")
        gc_env   = SUBSTRING(gc_proj, LENGTH(gc_proj), 1 )
        gc_proj  = SUBSTRING(gc_proj, 1, LENGTH(gc_proj) - 1 )
        gc_typeTrait = ENTRY(1, SESSION:PARAMETER, ",")
        gc_program = ENTRY(2, SESSION:PARAMETER, ",")
        gc_baseFileName = SUBSTRING(gc_program, R-INDEX(gc_program, "\" ) + 1)
        gc_baseFileName = SUBSTRING(gc_baseFileName, 1, R-INDEX(gc_baseFileName, "." ) - 1)
        gi_tmaprogressUp = INT(ENTRY(3, SESSION:PARAMETER, ","))
        gi_prolintUp = INT(ENTRY(4, SESSION:PARAMETER, ",")).

    /*
    ASSIGN FILE-INFORMATION:FILE-NAME = gc_appDataPath + "\notepad_tools.ini".
    IF FILE-INFORMATION:FULL-PATHNAME = ? THEN DO:
        OS-COPY VALUE("{&PROGRESSRPATH}notepad_tools.ini") VALUE(gc_appDataPath + "\notepad_tools.ini").
        MESSAGE "first time" VIEW-AS ALERT-BOX INFORMATION BUTTONS OK.
    END.
    
    ASSIGN FILE-INFORMATION:FILE-NAME = gc_notepadPath.
    IF NOT ll_success OR FILE-INFORMATION:FULL-PATHNAME = ? THEN DO:
        MESSAGE "Oops, the path to notepad++ is not valid, check your .ini file" VIEW-AS ALERT-BOX ERROR BUTTONS OK.
        RETURN "".
    END.
        */
    
    IF gc_typeTrait = "INIT" THEN DO:
        derploop:
        REPEAT:
            RUN Sleep(5000).
            ASSIGN FILE-INFORMATION:FILE-NAME = gc_NpptoolPath + "\isnotepadopened.yolo".
            IF FILE-INFORMATION:FULL-PATHNAME = ? THEN DO:
                RETURN "".
            END.
        END.
        RETURN "".
    END.

    /* Connecting tmaprogress db */
    IF NOT CONNECTED("tmaprogress") THEN DO:
        CONNECT -pf p:\base\tmaprogress\newtmap.pf NO-ERROR.
        IF NOT CONNECTED("tmaprogress") THEN DO:
            MESSAGE ERROR-STATUS:GET-MESSAGE(1) VIEW-AS ALERT-BOX INFO BUTTONS OK TITLE "Erreur à la connexion de tmaprogress".
            RETURN "".
        END.
    END.

    /* get .ini and .pf file to use for the compilation */
    FIND FIRST tma_compilation NO-LOCK
        WHERE tma_compilation.code_projet = gc_proj AND tma_compilation.env_sopra = gc_env AND tma_compilation.module = "CL".
    FIND FIRST tma_paramenv NO-LOCK
        WHERE tma_paramenv.code_projet = gc_proj AND tma_paramenv.env_sopra = gc_env AND tma_paramenv.rversion = tma_compilation.rversion.

    /* connect to the database */
    CONNECT -pf VALUE(TRIM(tma_paramenv.pf_win)) NO-ERROR.

    IF SEARCH(gc_NpptoolPath) = ? THEN
        PROPATH = PROPATH + "," + gc_NpptoolPath.
    
    /* load the propath from the ini file */
    RUN progress_getini.r (
        "Startup",
        "PROPATH",
        "",
        OUTPUT lc_propath,
        TRIM(tma_paramenv.ini_win),
        OUTPUT ll_success).

    PROPATH = TRIM(lc_propath) + "," + gc_NpptoolPath.

    IF NOT ll_success THEN DO:
        MESSAGE "Oops, i can't read the propath from : " + TRIM(tma_paramenv.ini_win) VIEW-AS ALERT-BOX ERROR BUTTONS OK.
        RETURN "".
    END.
    

    CASE gc_typeTrait :
        WHEN "COMPILE" THEN DO:
            RUN compilUnCCL.p PERSISTENT (INPUT gc_program, INPUT gc_env, INPUt gc_proj) NO-ERROR..
            IF ERROR-STATUS:ERROR THEN DO:
                MESSAGE "Oops, problem in compilUnCCL.p : " SKIP RETURN-VALUE VIEW-AS ALERT-BOX ERROR BUTTONS OK.
                RETURN "".
            END.
            IF RETURN-VALUE <> "" THEN
                RUN pi_localCompilation NO-ERROR.
        END.

        WHEN "OPENLST" THEN DO:
            COMPILE VALUE(gc_program) SAVE=TRUE INTO VALUE(gc_tempPath) DEBUG-LIST VALUE(gc_tempPath + "\" + gc_baseFileName + ".lst") NO-ERROR.
            RUN WinExec (gc_notepadPath + " " + gc_tempPath + "\" + gc_baseFileName + ".lst",  1,  OUTPUT li_ret).
        END.

        WHEN "CHECK" THEN DO:
            COMPILE VALUE(gc_program) SAVE=TRUE INTO VALUE(gc_tempPath) DEBUG-LIST VALUE(gc_tempPath + "\" + gc_baseFileName + ".lst") NO-ERROR.
            RUN pi_handleCompilErrors NO-ERROR.
        END.

        WHEN "RUN" THEN DO:
            RUN pi_run NO-ERROR.
        END.

        WHEN "PROLINT" THEN DO:
            RUN pi_runProLint NO-ERROR.
        END.

        WHEN "OPENAB" THEN DO:
            ASSIGN lc_commande_dos = "C:\Progress\client\v1110_dv\dlc\bin\prowin32.exe"
                + " -T ~"c:\temp~""
                + " -cpinternal ISO8859-1"
                + " -inp 20000 -tok 2048 -numsep 46"
                + " -ini " + TRIM(tma_paramenv.ini_win)
                + " -pf " + TRIM(tma_paramenv.pf_win)
                + " -p _ab.p"
                + " -param ~"" + gc_program + "~""
                + " " + TRIM(tma_paramenv.params).
            RUN WinExec (lc_commande_dos,  1,  OUTPUT li_ret).          
        END.

        OTHERWISE DO:
            MESSAGE "Oops, wrong type of action" VIEW-AS ALERT-BOX ERROR BUTTONS OK.
            RETURN "".
        END.
    END CASE.
    
    RETURN "".

END PROCEDURE.


PROCEDURE pi_run :
/*------------------------------------------------------------------------------
  Purpose:
  Parameters:  <none>
  Notes:
------------------------------------------------------------------------------*/

    DO  ON STOP   UNDO, LEAVE
        ON ERROR  UNDO, LEAVE
        ON ENDKEY UNDO, LEAVE
        ON QUIT   UNDO, LEAVE:
        RUN VALUE(gc_program).
    END.

    RUN pi_handleCompilErrors NO-ERROR.
    
    RETURN "".

END PROCEDURE.


PROCEDURE pi_localCompilation :
/*------------------------------------------------------------------------------
  Purpose:
  Parameters:  <none>
  Notes:
------------------------------------------------------------------------------*/

    DEFINE VARIABLE lc_compPath             AS CHARACTER NO-UNDO.
    DEFINE VARIABLE ll_resp                 AS LOGICAL NO-UNDO.

    FIND FIRST TMA_ENVIRONNEMENT NO-LOCK
        WHERE TMA_ENVIRONNEMENT.code_projet = gc_proj AND TMA_ENVIRONNEMENT.env_sopra = gc_env.

    ASSIGN lc_compPath = TMA_ENVIRONNEMENT.r_dstwin.

    tma_comp:
    FOR EACH tma_compilation NO-LOCK
        WHERE   tma_compilation.code_projet = gc_proj AND
                tma_compilation.env_sopra = gc_env AND
                tma_compilation.module = "CL" AND
                tma_compilation.rep_dest <> ".\".
        IF SUBSTRING(gc_program, 1, R-INDEX(gc_program, "\" )) MATCHES("*" + tma_compilation.rep_source + "*") THEN DO:
            ASSIGN lc_compPath = lc_compPath + tma_compilation.rep_dest.
            LEAVE tma_comp.
        END.
    END.

    /* do you want to compile it where it is or on P:/appli ? */
    MESSAGE "This program isn't located in the Synergy workarea. Choose an option :" SKIP
        "(YES) compile into " + lc_compPath SKIP
        "(NO) compile into " + SUBSTRING(gc_program, 1, R-INDEX(gc_program, "\" )) SKIP
        "(CANCEL) guess what it does"
        VIEW-AS ALERT-BOX QUESTION BUTTONS YES-NO-CANCEL UPDATE ll_resp.

    IF ll_resp = ? THEN
        RETURN "".

    IF ll_resp = FALSE THEN
        ASSIGN lc_compPath = SUBSTRING(gc_program, 1, R-INDEX(gc_program, "\" )).

    COMPILE VALUE(gc_program) SAVE=TRUE INTO VALUE(lc_compPath) DEBUG-LIST VALUE(lc_compPath + gc_baseFileName + ".lst") NO-ERROR.

    RUN pi_handleCompilErrors NO-ERROR.
    
    RETURN "".

END PROCEDURE.


PROCEDURE pi_handleCompilErrors :
/*------------------------------------------------------------------------------
  Purpose:
  Parameters:  <none>
  Notes:
------------------------------------------------------------------------------*/

    DEFINE VARIABLE lc_msg AS CHARACTER NO-UNDO.
    DEFINE VARIABLE li_i AS INTEGER NO-UNDO.
    DEFINE VARIABLE li_ret AS INTEGER NO-UNDO.

    IF COMPILER:NUM-MESSAGES > 0 THEN DO:
        ASSIGN
            lc_msg = ""
            li_i = 1.
        DO WHILE li_i <= COMPILER:NUM-MESSAGES:
            IF COMPILER:GET-NUMBER(li_i) <> 196 THEN
                ASSIGN
                    lc_msg = lc_msg + IF li_i <> 1 THEN "____________________________________________~n" ELSE ""
                    lc_msg = lc_msg + SUBSTITUTE(" >> &4 : ~n&1 (in &5 : line &2, column &3)~n",
                        REPLACE(COMPILER:GET-MESSAGE(li_i), "** ", ""),
                        COMPILER:GET-ERROR-ROW(li_i),
                        COMPILER:GET-ERROR-COLUMN(li_i),
                        IF COMPILER:GET-MESSAGE-TYPE(li_i) = 2 THEN "WARNING" ELSE "ERROR",
                        COMPILER:GET-FILE-NAME(li_i)).
            ASSIGN li_i = li_i + 1.
        END.
        IF COMPILER:WARNING = TRUE THEN
            MESSAGE lc_msg VIEW-AS ALERT-BOX WARNING TITLE "Messages du compilateur".
        ELSE DO:
            MESSAGE lc_msg VIEW-AS ALERT-BOX ERROR TITLE "Messages du compilateur".
            RUN WinExec (
                gc_notepadPath + SUBSTITUTE(" -n&1 -c&2 &3", COMPILER:GET-ERROR-ROW(1), COMPILER:GET-ERROR-COLUMN(1), COMPILER:GET-FILE-NAME(1)),
                1,
                OUTPUT li_ret).
        END.
    END.
    ELSE DO:
        IF gc_typeTrait = "CHECK" THEN
            MESSAGE "The synthax is correct, congrats =)" VIEW-AS ALERT-BOX INFORMATION.
    END.
    
    RETURN "".

END PROCEDURE.


PROCEDURE pi_runProLint :
/*------------------------------------------------------------------------------
  Purpose:
  Parameters:  <none>
  Notes:
------------------------------------------------------------------------------*/

    /* build propath */
    IF SEARCH("P:~\outils~\cnaftools") = ? THEN
        PROPATH = PROPATH + "," + "P:~\outils~\cnaftools".

    RUN prolint/launch/lintfileList.p(gc_program).

    RETURN "".
    
END PROCEDURE.