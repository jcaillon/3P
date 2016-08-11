&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Procedure
/*------------------------------------------------------------------------

    Purpose     : A special output handler for prolint, creates a .log file readable by 3P
    Description :
    Author(s)   : Julien Caillon (julien.caillon@gmail.com)
    Created     : 19/02/2016
    Notes       : 

  ----------------------------------------------------------------------*/
/*  This file was created with the 3P :  https://jcaillon.github.io/3P/ */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */
DEFINE VARIABLE gc_outputFilePath AS CHARACTER NO-UNDO.
DEFINE STREAM str_logout.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE no



/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */
FUNCTION fi_severity_label RETURNS CHARACTER ( INPUT ipi_sev AS INTEGER) FORWARD.


/* *********************** Procedure Settings ************************ */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
   Type: Procedure
   Allow:
   Frames: 0
   Add Fields to: Neither
   Other Settings: CODE-ONLY COMPILE
 */
&ANALYZE-RESUME _END-PROCEDURE-SETTINGS

/* *************************  Create Window  ************************** */

&ANALYZE-SUSPEND _CREATE-WINDOW
/* DESIGN Window definition (used by the UIB)
  CREATE WINDOW Procedure ASSIGN
         HEIGHT             = 15
         WIDTH              = 60.
/* END WINDOW DEFINITION */
                                                                        */
&ANALYZE-RESUME




&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Procedure


/* ***************************  Main Block  *************************** */

/*SUBSCRIBE TO "Prolint_InitializeResults" ANYWHERE.*/
SUBSCRIBE TO "Prolint_AddResult" ANYWHERE.
SUBSCRIBE TO "Prolint_FinalizeResults" ANYWHERE.

PUBLISH "getProlintOutputFile" (OUTPUT gc_outputFilePath).

/* make sure the file doesn't exist */
OS-DELETE VALUE(gc_outputFilePath).
IF OS-ERROR > 0 AND OS-ERROR <> 2 THEN
    MESSAGE "3P prolint, can't delete : ~n" + gc_outputFilePath.

RETURN "".

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */
           
PROCEDURE Prolint_AddResult :              
/*------------------------------------------------------------------------------
  Purpose: add one result from a 'rule' to the logfile
  Parameters:
------------------------------------------------------------------------------*/

    DEFINE INPUT PARAMETER pCompilationUnit  AS CHARACTER   NO-UNDO.  /* the sourcefile we're parsing          */
    DEFINE INPUT PARAMETER pSourcefile       AS CHARACTER   NO-UNDO.  /* may be an includefile                 */
    DEFINE INPUT PARAMETER pLineNumber       AS INTEGER     NO-UNDO.  /* line number in pSourceFile            */
    DEFINE INPUT PARAMETER pDescription      AS CHARACTER   NO-UNDO.  /* human-readable hint                   */
    DEFINE INPUT PARAMETER pRuleID           AS CHARACTER   NO-UNDO.  /* defines rule-program and maps to help */
    DEFINE INPUT PARAMETER pSeverity         AS INTEGER     NO-UNDO.  /* importance of this rule, scale 0-9    */



    OUTPUT STREAM str_logout TO VALUE(gc_outputFilePath) APPEND BINARY.
    PUT STREAM str_logout UNFORMATTED SUBSTITUTE("&1~t&2~t&3~t&4~t&5~t&6~t&7~t&8",
        pCompilationUnit,
        pSourcefile,
        fi_severity_label(pSeverity),
        pLineNumber,
        0,
        pSeverity,
        "rule " + pRuleID + ", " + pDescription,
        ""
        ) SKIP.
    OUTPUT STREAM str_logout CLOSE.
   
END PROCEDURE.
   
PROCEDURE Prolint_FinalizeResults :                                    
/*------------------------------------------------------------------------------
  Purpose: close the logfile and/or show it. Free resources
  Parameters:
------------------------------------------------------------------------------*/

    /* In this case there are no open resources, so we're done. */
    /* This procedure will not be invoked again, so it can exit */
    DELETE PROCEDURE THIS-PROCEDURE.                          

END PROCEDURE.

/* ************************  Function Implementations ***************** */

FUNCTION fi_severity_label RETURNS CHARACTER (INPUT ipi_sev AS INTEGER):
    
    CASE ipi_sev:
        WHEN 10 THEN
            RETURN "Critical".
        WHEN 9 THEN
            RETURN "Error".
        WHEN 8 OR
        WHEN 7 THEN
            RETURN "StrongWarning".
        WHEN 6 OR
        WHEN 5 THEN
            RETURN "Warning".
        OTHERWISE
            RETURN "Information".
    END.
    RETURN "NoErrors".
    
END FUNCTION.