&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Procedure
/*------------------------------------------------------------------------

    Purpose   :
                This program is called by 3P when after each step of a deployment,
                you can modify freely and execute whatever is needed depending on the input received
                The procedure pi_main() should be the entry point for you modifications !
                
    Author(s)   : Julien Caillon (julien.caillon@gmail.com)

  ----------------------------------------------------------------------*/
/*  This file was created with the 3P :  https://jcaillon.github.io/3P/ */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */


/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE no

/* Do not modify the lines below, values coming from 3P are set here,
    you can use the pre-processed variables defined below if you find them useful! */
/*<inserted_3P_values>*/

&IF DEFINED(ApplicationName) = 0 &THEN
    /* this block is only present for debug/tests purposes, the values below are
       overwritten when this file is run from 3P,
       you can set those values manually for your tests */
    &SCOPED-DEFINE ApplicationName ""
    &SCOPED-DEFINE ApplicationSuffix ""
    &SCOPED-DEFINE StepNumber 0
    &SCOPED-DEFINE SourceDirectory ""
    &SCOPED-DEFINE DeploymentDirectory ""
&ENDIF

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */


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

RUN pi_main (
    INPUT {&ApplicationName},
    INPUT {&ApplicationSuffix},
    INPUT {&StepNumber},
    INPUT {&SourceDirectory},
    INPUT {&DeploymentDirectory}    
    ) NO-ERROR.
IF ERROR-STATUS:ERROR THEN
    RETURN ERROR (IF RETURN-VALUE > "" THEN RETURN-VALUE ELSE ERROR-STATUS:GET-MESSAGE(1)).

RETURN "".

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE pi_main Procedure
PROCEDURE pi_main:
/*------------------------------------------------------------------------------
  Summary    :  Do the treatments needed here depending on the step, application and so on...   
  Parameters : <none>
  Returns    : 
  Remarks    :     
------------------------------------------------------------------------------*/

    DEFINE INPUT PARAMETER ipc_applicationName AS CHARACTER NO-UNDO. /* name of the application as it appears in the environment page of 3P */
    DEFINE INPUT PARAMETER ipc_applicationSuffix AS CHARACTER NO-UNDO. /* suffix of the application as it appears in the environment page of 3P */
    DEFINE INPUT PARAMETER ipi_stepNumber AS INTEGER NO-UNDO. /* step number of the deployment, this procedure is called for each step! */
    DEFINE INPUT PARAMETER ipc_sourceDirectory AS CHARACTER NO-UNDO. /* full path to the source directory */
    DEFINE INPUT PARAMETER ipc_deploymentDirectory AS CHARACTER NO-UNDO. /* full path to the deployment directory */

    /* use the PUBLISH below to display a notification in 3P after the end of this program */
    PUBLISH "eventToPublishToNotifyTheUserAfterExecution" (
        INPUT "Deployment for the application : <b>" + QUOTER(ipc_applicationName) + "</b><br>" +
              "The application suffix : <b>" + QUOTER(ipc_applicationSuffix) + "</b><br>" +
              "The deployment step <b>n°" + STRING(ipi_stepNumber) + "</b><br>" +
              "Source directory : <br><b>" + QUOTER(ipc_sourceDirectory) + "</b><br>" +
              "Deployment directory : <br><b>" + QUOTER(ipc_deploymentDirectory) + "</b><br>",
        INPUT 0, /* from 0 to 4, to have an icon corresponding to : "MsgOk", "MsgError", "MsgWarning", "MsgInfo", "MsgHighImportance" */
        INPUT "Deployment title",
        INPUT "Deployment subtitle",
        INPUT 0, /* duration of the notification in seconds (0 for infinite time) */
        INPUT "" /* unique name for the notification, if it it set, the notif will close on a click on a link and 
                    will automatically be closed if another notification with the same name pops up */
        ).
    
    RETURN "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Implementations ***************** */

