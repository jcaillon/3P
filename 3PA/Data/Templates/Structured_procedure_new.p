&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Procedure
/* ************************************************************************************************** */
/*                                                                                                    */
/* APPLICATION       : Boite à Outils                                                                 */
/* MODULE            : .....................................                                          */
/* PROGRAMME         : interv_api.p                                                                   */
/* DATE              : 19/01/2016                                                                     */
/* AUTEUR            : CS SOPRA - XXX                                                                 */
/* DESCRIPTION       :                                                                                */
/* -------------------------------------------------------------------------------------------------- */
/* HISTORIQUE DES MODIFICATIONS :                                                                     */
/*  ______ ____________ ___________ ________________________________________________________________  */
/* |      |            |           |                                                                | */
/* |  N°  |    DATE    |   AUTEUR  | DESCRIPTION                                                    | */
/* |______|____________|___________|________________________________________________________________| */
/*                                                                                                    */

/* ***************************  Definitions  ************************** */

/* ******************************************************************** */
/* *** Temp-Table Definitions                                       *** */
/* ******************************************************************** */

/* ******************************************************************** */
/* *** Buffers Definitions                                          *** */
/* ******************************************************************** */

/* ******************************************************************** */
/* *** Parameters Definitions                                       *** */
/* ******************************************************************** */

/* ******************************************************************** */
/* *** Local Variable Definitions                                   *** */
/* ******************************************************************** */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE no



/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */



/* *********************** Procedure Settings ************************* */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
   Type: Procedure
   Allow:
   Frames: 0
   Add Fields to: Neither
   Other Settings: CODE-ONLY
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



/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */


/* ************************  Function Implementations ***************** */

