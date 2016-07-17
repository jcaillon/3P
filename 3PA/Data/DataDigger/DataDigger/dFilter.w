&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME CURRENT-WINDOW
&Scoped-define FRAME-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Dialog-Frame 
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
{datadigger.i}

/* Parameters Definitions ---                                           */
define input-output parameter pcFilter  as character  no-undo.
define input        parameter pcOptions as character  no-undo.

/* Local Variable Definitions ---                                       */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME Dialog-Frame

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS RECT-1 Btn_OK Btn_Cancel cbFieldFilter 
&Scoped-Define DISPLAYED-OBJECTS cbFieldFilter 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON Btn_Cancel AUTO-END-KEY 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON Btn_OK AUTO-GO 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE VARIABLE cbFieldFilter AS CHARACTER 
     VIEW-AS COMBO-BOX INNER-LINES 10
     DROP-DOWN
     SIZE-PIXELS 385 BY 21 TOOLTIP "provide a comma separated list of (partial) field names" NO-UNDO.

DEFINE RECTANGLE RECT-1
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL  GROUP-BOX  
     SIZE-PIXELS 410 BY 93.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     Btn_OK AT Y 61 X 243
     Btn_Cancel AT Y 61 X 323
     cbFieldFilter AT Y 25 X 5 COLON-ALIGNED NO-LABEL WIDGET-ID 8
     "Show only tables that contain these fields (comma-sep):" VIEW-AS TEXT
          SIZE-PIXELS 377 BY 13 AT Y 11 X 13 WIDGET-ID 6
     RECT-1 AT Y 0 X 0 WIDGET-ID 4
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 424 BY 129
         TITLE "Edit table field filter"
         DEFAULT-BUTTON Btn_OK CANCEL-BUTTON Btn_Cancel WIDGET-ID 100.


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
/* SETTINGS FOR DIALOG-BOX Dialog-Frame
   FRAME-NAME                                                           */
ASSIGN 
       FRAME Dialog-Frame:SCROLLABLE       = FALSE
       FRAME Dialog-Frame:HIDDEN           = TRUE.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* Edit table field filter */
DO:
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME Btn_OK
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Btn_OK Dialog-Frame
ON CHOOSE OF Btn_OK IN FRAME Dialog-Frame /* OK */
DO:
  pcFilter = cbFieldFilter:screen-value.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Dialog-Frame 


/* ***************************  Main Block  *************************** */

/* Parent the dialog-box to the ACTIVE-WINDOW, if there is no parent.   */
IF VALID-HANDLE(ACTIVE-WINDOW) AND FRAME {&FRAME-NAME}:PARENT eq ?
THEN FRAME {&FRAME-NAME}:PARENT = ACTIVE-WINDOW.

assign cbFieldFilter = pcFilter.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  RUN initializeObject.
  RUN enable_UI.
  WAIT-FOR GO OF FRAME {&FRAME-NAME} FOCUS cbFieldFilter.
END.
RUN disable_UI.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI Dialog-Frame  _DEFAULT-DISABLE
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
  HIDE FRAME Dialog-Frame.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI Dialog-Frame  _DEFAULT-ENABLE
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
  DISPLAY cbFieldFilter 
      WITH FRAME Dialog-Frame.
  ENABLE RECT-1 Btn_OK Btn_Cancel cbFieldFilter 
      WITH FRAME Dialog-Frame.
  VIEW FRAME Dialog-Frame.
  {&OPEN-BROWSERS-IN-QUERY-Dialog-Frame}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject Dialog-Frame 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE VARIABLE iOption           AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cOption           AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cSetting          AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cValue            AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cFilterList       AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cNewList          AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iPos              AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cThisValue        AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iMaxFilterHistory AS INTEGER   NO-UNDO.

  do with frame {&FRAME-NAME}:

    /* Set default font */
    frame {&frame-name}:font = getFont('Default').
    
    /* For some reasons, these #*$&# scrollbars keep coming back */
    .run showScrollBars(frame {&frame-name}:handle, no, no). /* KILL KILL KILL */
    
    do iOption = 1 to num-entries(pcOptions):
      cOption  = entry(iOption,pcOptions).
      cSetting = entry(1,cOption,"=").
      cValue   = entry(2,cOption,"=").
      
      case cSetting:
        when "x" then frame {&FRAME-NAME}:x = integer(cValue).
        when "y" then frame {&FRAME-NAME}:y = integer(cValue).
      end case.
    end.


    /* Place search history in the combo */
    cbFieldFilter:DELIMITER = CHR(1).

    /* How many to remember? */
    iMaxFilterHistory = INTEGER(getRegistry("DataDigger", "MaxFilterHistory")).
    IF iMaxFilterHistory = ? THEN iMaxFilterHistory = 10.

    /* Populate combo */
    cFilterList = getRegistry("DataDigger","TableFieldFilter").
    IF cFilterList = ? THEN cFilterList = "".
  
    /* Start the new list with the current filter value */
    cNewList = pcFilter.
  
    /* Add old entries to the list */
    DO iPos = 1 TO NUM-ENTRIES(cFilterList,CHR(1)).
      cThisValue = ENTRY(iPos,cFilterList,CHR(1)).
  
      /* Skip empty */
      IF cThisValue = "" THEN NEXT. 
  
      /* If it is already in the list, ignore */
      IF LOOKUP(cThisValue,cNewList,CHR(1)) > 0 THEN NEXT. 
  
      /* Add to list */
      cNewList = cNewList + CHR(1) + cThisValue.
  
      /* Stop if there are too much in the list */
      IF NUM-ENTRIES(cNewList,CHR(1)) >= iMaxFilterHistory THEN LEAVE. 
    END.
  
    setRegistry("DataDigger","TableFieldFilter", cNewList).
    cNewList = TRIM(cNewList,CHR(1)).
    IF NUM-ENTRIES(cNewList,CHR(1)) > 0 THEN cbFieldFilter:LIST-ITEMS = cNewList.

  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

