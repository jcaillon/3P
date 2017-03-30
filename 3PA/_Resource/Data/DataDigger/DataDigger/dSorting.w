&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME CURRENT-WINDOW
&Scoped-define FRAME-NAME frSorting
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS frSorting 
/*------------------------------------------------------------------------

  File: 

  Description: 

  Input Parameters:
      <none>

  Output Parameters:
      <none>

  Author: 

  Created: 
------------------------------------------------------------------------*/
/*          This .W file was created with the Progress AppBuilder.       */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */
{ datadigger.i }

/* Parameters Definitions ---                                           */
DEFINE INPUT PARAMETER TABLE FOR ttColumn.
DEFINE INPUT-OUTPUT PARAMETER TABLE FOR ttQuerySort.
DEFINE OUTPUT PARAMETER plSortChanged AS LOGICAL NO-UNDO.

DEFINE VARIABLE ghSortField  AS HANDLE EXTENT 9 NO-UNDO.
DEFINE VARIABLE ghDescending AS HANDLE EXTENT 9 NO-UNDO.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME frSorting

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS btnOk Btn_Cancel btnClearAll cbSortField-1 
&Scoped-Define DISPLAYED-OBJECTS tgDescending-1 cbSortField-2 ~
tgDescending-2 cbSortField-3 tgDescending-3 cbSortField-4 tgDescending-4 ~
cbSortField-5 tgDescending-5 cbSortField-6 tgDescending-6 cbSortField-7 ~
tgDescending-7 cbSortField-8 tgDescending-8 cbSortField-9 tgDescending-9 ~
cbSortField-1 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnClearAll 
     LABEL "&Clear All" 
     SIZE-PIXELS 75 BY 24 TOOLTIP "clear all sorting".

DEFINE BUTTON btnOk 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON Btn_Cancel AUTO-END-KEY 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE VARIABLE cbSortField-1 AS CHARACTER 
     LABEL "First sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE cbSortField-2 AS CHARACTER 
     LABEL "Then sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE cbSortField-3 AS CHARACTER 
     LABEL "Then sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE cbSortField-4 AS CHARACTER 
     LABEL "Then sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE cbSortField-5 AS CHARACTER 
     LABEL "Then sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE cbSortField-6 AS CHARACTER 
     LABEL "Then sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE cbSortField-7 AS CHARACTER 
     LABEL "Then sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE cbSortField-8 AS CHARACTER 
     LABEL "Then sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE cbSortField-9 AS CHARACTER 
     LABEL "Then sort on" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN AUTO-COMPLETION UNIQUE-MATCH
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE tgDescending-1 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.

DEFINE VARIABLE tgDescending-2 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.

DEFINE VARIABLE tgDescending-3 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.

DEFINE VARIABLE tgDescending-4 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.

DEFINE VARIABLE tgDescending-5 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.

DEFINE VARIABLE tgDescending-6 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.

DEFINE VARIABLE tgDescending-7 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.

DEFINE VARIABLE tgDescending-8 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.

DEFINE VARIABLE tgDescending-9 AS LOGICAL INITIAL no 
     LABEL "&Descending" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 95 BY 17 NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frSorting
     tgDescending-1 AT Y 10 X 269 WIDGET-ID 16
     cbSortField-2 AT Y 36 X 80 COLON-ALIGNED WIDGET-ID 78
     tgDescending-2 AT Y 36 X 269 WIDGET-ID 80
     cbSortField-3 AT Y 61 X 80 COLON-ALIGNED WIDGET-ID 82
     tgDescending-3 AT Y 61 X 269 WIDGET-ID 84
     cbSortField-4 AT Y 86 X 80 COLON-ALIGNED WIDGET-ID 86
     tgDescending-4 AT Y 86 X 269 WIDGET-ID 88
     cbSortField-5 AT Y 111 X 80 COLON-ALIGNED WIDGET-ID 90
     tgDescending-5 AT Y 111 X 269 WIDGET-ID 92
     cbSortField-6 AT Y 136 X 80 COLON-ALIGNED WIDGET-ID 94
     tgDescending-6 AT Y 136 X 269 WIDGET-ID 96
     cbSortField-7 AT Y 161 X 80 COLON-ALIGNED WIDGET-ID 98
     tgDescending-7 AT Y 161 X 269 WIDGET-ID 100
     cbSortField-8 AT Y 186 X 80 COLON-ALIGNED WIDGET-ID 102
     btnOk AT Y 185 X 405
     tgDescending-8 AT Y 186 X 269 WIDGET-ID 104
     cbSortField-9 AT Y 211 X 80 COLON-ALIGNED WIDGET-ID 106
     tgDescending-9 AT Y 211 X 269 WIDGET-ID 108
     Btn_Cancel AT Y 211 X 405
     btnClearAll AT Y 100 X 405 WIDGET-ID 110
     cbSortField-1 AT Y 10 X 80 COLON-ALIGNED WIDGET-ID 2
     "ALT-S also opens this screen" VIEW-AS TEXT
          SIZE-PIXELS 205 BY 18 AT Y 247 X 155 WIDGET-ID 112
          FGCOLOR 7 
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 504 BY 300
         TITLE "Set Sorting"
         DEFAULT-BUTTON btnOk CANCEL-BUTTON Btn_Cancel WIDGET-ID 100.


/* *********************** Procedure Settings ************************ */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
   Type: Dialog-Box
   Allow: Basic,Browse,DB-Fields,Query
   Other Settings: COMPILE
 */
&ANALYZE-RESUME _END-PROCEDURE-SETTINGS



/* ***********  Runtime Attributes and AppBuilder Settings  *********** */

&ANALYZE-SUSPEND _RUN-TIME-ATTRIBUTES
/* SETTINGS FOR DIALOG-BOX frSorting
   FRAME-NAME                                                           */
ASSIGN 
       FRAME frSorting:SCROLLABLE       = FALSE
       FRAME frSorting:HIDDEN           = TRUE.

/* SETTINGS FOR COMBO-BOX cbSortField-2 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR COMBO-BOX cbSortField-3 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR COMBO-BOX cbSortField-4 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR COMBO-BOX cbSortField-5 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR COMBO-BOX cbSortField-6 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR COMBO-BOX cbSortField-7 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR COMBO-BOX cbSortField-8 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR COMBO-BOX cbSortField-9 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-1 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-2 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-3 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-4 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-5 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-6 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-7 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-8 IN FRAME frSorting
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDescending-9 IN FRAME frSorting
   NO-ENABLE                                                            */
/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME frSorting
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frSorting frSorting
ON WINDOW-CLOSE OF FRAME frSorting /* Set Sorting */
DO:
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClearAll
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClearAll frSorting
ON CHOOSE OF btnClearAll IN FRAME frSorting /* Clear All */
DO:
  cbSortField-1:SCREEN-VALUE = ?.
  APPLY 'value-changed' TO cbSortField-1.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnOk
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOk frSorting
ON CHOOSE OF btnOk IN FRAME frSorting /* OK */
OR 'go' OF FRAME {&FRAME-NAME} ANYWHERE
DO:
  DEFINE VARIABLE lContinue AS LOGICAL     NO-UNDO.
  RUN btnOkChoose(OUTPUT lContinue).
  IF lContinue THEN APPLY 'GO' TO FRAME {&FRAME-NAME}.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbSortField-1
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbSortField-1 frSorting
ON ALT-D OF cbSortField-1 IN FRAME frSorting /* First sort on */
, cbSortField-2, cbSortField-3, cbSortField-4, cbSortField-5, cbSortField-6, cbSortField-7, cbSortField-8, cbSortField-9
DO:
  DEFINE VARIABLE iSelf AS INTEGER NO-UNDO.
  iSelf = INTEGER(ENTRY(2,SELF:NAME,'-')).
  IF ghDescending[iSelf]:SENSITIVE THEN
    ghDescending[iSelf]:CHECKED = NOT ghDescending[iSelf]:CHECKED.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbSortField-1 frSorting
ON RETURN OF cbSortField-1 IN FRAME frSorting /* First sort on */
, cbSortField-2, cbSortField-3, cbSortField-4, cbSortField-5, cbSortField-6, cbSortField-7, cbSortField-8, cbSortField-9
, tgDescending-1, tgDescending-2, tgDescending-3, tgDescending-4, tgDescending-5, tgDescending-6, tgDescending-7, tgDescending-8, tgDescending-9
DO:
  DEFINE VARIABLE iSelf     AS INTEGER NO-UNDO.
  DEFINE VARIABLE lContinue AS LOGICAL NO-UNDO.

  iSelf = INTEGER(ENTRY(2,SELF:NAME,'-')).

  IF ghSortField[iSelf]:SCREEN-VALUE = ? THEN 
  DO:
    RUN btnOkChoose(OUTPUT lContinue).
    IF lContinue THEN APPLY 'GO' TO FRAME {&FRAME-NAME}.
  END.
  ELSE 
    IF iSelf < 9 THEN APPLY 'entry' TO ghSortField[iSelf + 1].
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbSortField-1 frSorting
ON VALUE-CHANGED OF cbSortField-1 IN FRAME frSorting /* First sort on */
, cbSortField-2, cbSortField-3, cbSortField-4, cbSortField-5, cbSortField-6, cbSortField-7, cbSortField-8, cbSortField-9
DO:
  DEFINE VARIABLE iSelf  AS INTEGER NO-UNDO.
  DEFINE VARIABLE iField AS INTEGER NO-UNDO.

  iSelf = INTEGER(ENTRY(2,SELF:NAME,'-')).

  IF SELF:SCREEN-VALUE = ? THEN 
  DO:
    ghDescending[iSelf]:CHECKED = FALSE.
    ghDescending[iSelf]:SENSITIVE = NO.

    DO iField = iSelf + 1 TO 9:
      ghSortField[iField]:SCREEN-VALUE = ?.
      ghSortField[iField]:SENSITIVE    = NO.
      ghDescending[iField]:CHECKED     = FALSE.
      ghDescending[iField]:SENSITIVE   = NO.
    END.
  END.

  ELSE
  DO:
    ghDescending[iSelf]:SENSITIVE    = YES.
    IF iSelf < 9 THEN ghSortField[iSelf + 1]:SENSITIVE = YES.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK frSorting 


/* ***************************  Main Block  *************************** */

/* Parent the dialog-box to the ACTIVE-WINDOW, if there is no parent.   */
IF VALID-HANDLE(ACTIVE-WINDOW) AND FRAME {&FRAME-NAME}:PARENT eq ?
THEN FRAME {&FRAME-NAME}:PARENT = ACTIVE-WINDOW.


/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  /* Set default font */
  FRAME {&FRAME-NAME}:FONT = getFont('Default').

  RUN enable_UI.
  RUN initializeObject.

  WAIT-FOR GO OF FRAME {&FRAME-NAME}.
END.
RUN disable_UI.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnOkChoose frSorting 
PROCEDURE btnOkChoose :
/* Accept sort and go back
 */  
  DEFINE OUTPUT PARAMETER plContinue AS LOGICAL NO-UNDO.

  DEFINE VARIABLE i AS INTEGER NO-UNDO.
  DEFINE BUFFER bfQuerySort FOR ttQuerySort.

  /* fill tt with new sort */
  FOR EACH bfQuerySort WHERE bfQuerySort.iGroup = 2:
    DELETE bfQuerySort.
  END.

  DO i = 1 TO 9:
    IF LOOKUP(ghSortField[i]:SCREEN-VALUE, ghSortField[i]:LIST-ITEMS) = 0 THEN
    DO:
      MESSAGE 'Sorting' i 'does not seem to be a valid field name' VIEW-AS ALERT-BOX INFO BUTTONS OK.
      APPLY 'entry' TO ghSortField[i].
      RETURN NO-APPLY.
    END.

    IF ghSortField[i]:SCREEN-VALUE <> ? THEN 
    DO:
      CREATE bfQuerySort.
      ASSIGN 
        bfQuerySort.iGroup     = 2
        bfQuerySort.iSortNr    = i
        bfQuerySort.cSortField = ghSortField[i]:SCREEN-VALUE
        bfQuerySort.lAscending = NOT ghDescending[i]:CHECKED
        .
      /* Extract extent nr from name */
      IF bfQuerySort.cSortField MATCHES '*[*]' THEN
        bfQuerySort.iExt = INTEGER( ENTRY(1,ENTRY(2,bfQuerySort.cSortField,'['),']') ).
    END.
  END.

  plSortChanged = TRUE.
  plContinue = TRUE. 

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI frSorting  _DEFAULT-DISABLE
PROCEDURE disable_UI :
/*------------------------------------------------------------------------------
  Purpose:     DISABLE the User Interface
  Parameters:  <none>
  Notes:       Here we clean-up the user-interface by deleting
               dynamic widgets we have created and/or hide 
               frames.  This procedure is usually called when
               we are ready to "clean-up" after running.
------------------------------------------------------------------------------*/
  /* Hide all frames. */
  HIDE FRAME frSorting.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI frSorting  _DEFAULT-ENABLE
PROCEDURE enable_UI :
/*------------------------------------------------------------------------------
  Purpose:     ENABLE the User Interface
  Parameters:  <none>
  Notes:       Here we display/view/enable the widgets in the
               user-interface.  In addition, OPEN all queries
               associated with each FRAME and BROWSE.
               These statements here are based on the "Other 
               Settings" section of the widget Property Sheets.
------------------------------------------------------------------------------*/
  DISPLAY tgDescending-1 cbSortField-2 tgDescending-2 cbSortField-3 
          tgDescending-3 cbSortField-4 tgDescending-4 cbSortField-5 
          tgDescending-5 cbSortField-6 tgDescending-6 cbSortField-7 
          tgDescending-7 cbSortField-8 tgDescending-8 cbSortField-9 
          tgDescending-9 cbSortField-1 
      WITH FRAME frSorting.
  ENABLE btnOk Btn_Cancel btnClearAll cbSortField-1 
      WITH FRAME frSorting.
  VIEW FRAME frSorting.
  {&OPEN-BROWSERS-IN-QUERY-frSorting}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject frSorting 
PROCEDURE initializeObject :
/*
 * Prepare widgets
 */
 /*
 DEFINE TEMP-TABLE ttQuerySort NO-UNDO RCODE-INFORMATION
  FIELD iGroup     AS INTEGER /* 1:query, 2:browse */
  FIELD iSortNr    AS INTEGER
  FIELD cSortField AS CHARACTER
  FIELD lAscending AS LOGICAL
  INDEX iPrim IS PRIMARY iGroup iSortNr
 */
 DEFINE VARIABLE cFields   AS CHARACTER NO-UNDO.
 DEFINE VARIABLE i         AS INTEGER   NO-UNDO.

 FOR EACH ttColumn BY ttColumn.cFieldName:
   cFields = cFields + ',' + ttColumn.cFullName.
 END.

 DO WITH FRAME {&FRAME-NAME}:
   /* Grab handles of the fields for easier handling */
   ghSortField[1] = cbSortField-1:HANDLE. ghDescending[1] = tgDescending-1:HANDLE.
   ghSortField[2] = cbSortField-2:HANDLE. ghDescending[2] = tgDescending-2:HANDLE.
   ghSortField[3] = cbSortField-3:HANDLE. ghDescending[3] = tgDescending-3:HANDLE.
   ghSortField[4] = cbSortField-4:HANDLE. ghDescending[4] = tgDescending-4:HANDLE.
   ghSortField[5] = cbSortField-5:HANDLE. ghDescending[5] = tgDescending-5:HANDLE.
   ghSortField[6] = cbSortField-6:HANDLE. ghDescending[6] = tgDescending-6:HANDLE.
   ghSortField[7] = cbSortField-7:HANDLE. ghDescending[7] = tgDescending-7:HANDLE.
   ghSortField[8] = cbSortField-8:HANDLE. ghDescending[8] = tgDescending-8:HANDLE.
   ghSortField[9] = cbSortField-9:HANDLE. ghDescending[9] = tgDescending-9:HANDLE.

   /* Make a list of all fields */
   DO i = 1 TO 9:
     ghSortField[i]:LIST-ITEMS = cFields.
     ghSortField[i]:SCREEN-VALUE = ?.
   END.

   /* fill combos with current sort */
   FOR EACH ttQuerySort WHERE ttQuerySort.iGroup = 2 BY ttQuerySort.iSortNr:
     ghSortField[ttQuerySort.iSortNr]:SCREEN-VALUE = ttQuerySort.cSortField.
     ghDescending[ttQuerySort.iSortNr]:CHECKED = NOT ttQuerySort.lAscending.
   END.

   /* Set correct state */
   DO i = 1 TO 9:
     APPLY 'value-changed' TO ghSortField[i].
   END.

 END.

END PROCEDURE. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

