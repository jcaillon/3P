&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Procedure
/*------------------------------------------------------------------------

    Purpose   :
                This program is called by 3P when after each step of a deployment,
                you can modify freely and execute whatever is needed depending on the input received
                
    Author(s)   : Julien Caillon (julien.caillon@gmail.com)
    Created     : 09/08/2016

  ----------------------------------------------------------------------*/
/*  This file was created with the 3P :  https://jcaillon.github.io/3P/ */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */

DEFINE INPUT PARAMETER ipc_applicationName AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER ipc_applicationSuffix AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER ipi_stepNumber AS INTEGER NO-UNDO.
DEFINE INPUT PARAMETER ipc_sourceDirectory AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER ipc_deploymentDirectory AS CHARACTER NO-UNDO.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE no

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


/* use the PUBLISH below to display a notification in 3P after the end of this program */
PUBLISH "eventToPublishToNotifyTheUserAfterExecution" (
    INPUT "my message content, <b>HTML</b> format! You can also set a <a href='http://jcaillon.github.io/3P/'>link</a> or whatever you want",
    INPUT 0, /* from 0 to 4, to have an icon corresponding to : "MsgOk", "MsgError", "MsgWarning", "MsgInfo", "MsgHighImportance" */
    INPUT "My notification title",
    INPUT "My notification subtitle",
    INPUT 0, /* duration of the notification in seconds (0 for infinite time) */
    INPUT "uniquename" /* unique name for the notification, if it it set, the notif will close on a click on a link and 
                will automatically be closed if another notification with the same name pops up */
    ).

RETURN "".

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */



/* ************************  Function Implementations ***************** */

