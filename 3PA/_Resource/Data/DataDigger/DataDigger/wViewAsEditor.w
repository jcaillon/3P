&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME wEditor
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS wEditor 
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
/*          This .W file was created with the Progress AppBuilder.      */
/*----------------------------------------------------------------------*/

/* Create an unnamed pool to store all the widgets created 
     by this procedure. This is a good default which assures
     that this procedure's triggers and internal procedures 
     will execute in this procedure's storage, and that proper
     cleanup will occur on deletion of the procedure. */

CREATE WIDGET-POOL.

/* ***************************  Definitions  ************************** */

{datadigger.i}

/* Parameters Definitions ---                                           */
DEFINE INPUT-OUTPUT PARAMETER pcValue   AS CHARACTER NO-UNDO.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME DEFAULT-FRAME

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS edValue btnOk btnCancel 
&Scoped-Define DISPLAYED-OBJECTS edValue 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wEditor AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnCancel AUTO-END-KEY 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON btnOk AUTO-GO 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE VARIABLE edValue AS CHARACTER 
     VIEW-AS EDITOR SCROLLBAR-VERTICAL
     SIZE-PIXELS 400 BY 130 NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME DEFAULT-FRAME
     edValue AT Y 0 X 0 NO-LABEL WIDGET-ID 36
     btnOk AT Y 140 X 245 WIDGET-ID 34
     btnCancel AT Y 140 X 325 WIDGET-ID 32
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 0 Y 0
         SIZE-PIXELS 400 BY 170 WIDGET-ID 100.


/* *********************** Procedure Settings ************************ */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
   Type: Window
   Allow: Basic,Browse,DB-Fields,Window,Query
   Other Settings: COMPILE
 */
&ANALYZE-RESUME _END-PROCEDURE-SETTINGS

/* *************************  Create Window  ************************** */

&ANALYZE-SUSPEND _CREATE-WINDOW
IF SESSION:DISPLAY-TYPE = "GUI":U THEN
  CREATE WINDOW wEditor ASSIGN
         HIDDEN             = YES
         TITLE              = "Edit Field Value"
         HEIGHT-P           = 173
         WIDTH-P            = 400
         MAX-HEIGHT-P       = 6720
         MAX-WIDTH-P        = 1600
         VIRTUAL-HEIGHT-P   = 6720
         VIRTUAL-WIDTH-P    = 1600
         RESIZE             = yes
         SCROLL-BARS        = no
         STATUS-AREA        = no
         BGCOLOR            = ?
         FGCOLOR            = ?
         KEEP-FRAME-Z-ORDER = yes
         THREE-D            = yes
         MESSAGE-AREA       = no
         SENSITIVE          = yes.
ELSE {&WINDOW-NAME} = CURRENT-WINDOW.
/* END WINDOW DEFINITION                                                */
&ANALYZE-RESUME



/* ***********  Runtime Attributes and AppBuilder Settings  *********** */

&ANALYZE-SUSPEND _RUN-TIME-ATTRIBUTES
/* SETTINGS FOR WINDOW wEditor
  VISIBLE,,RUN-PERSISTENT                                               */
/* SETTINGS FOR FRAME DEFAULT-FRAME
   FRAME-NAME                                                           */
IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wEditor)
THEN wEditor:HIDDEN = no.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME wEditor
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wEditor wEditor
ON END-ERROR OF wEditor /* Edit Field Value */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wEditor wEditor
ON LEAVE OF wEditor /* Edit Field Value */
OR "LEAVE" of wEditor
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wEditor wEditor
ON WINDOW-CLOSE OF wEditor /* Edit Field Value */
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wEditor wEditor
ON WINDOW-RESIZED OF wEditor /* Edit Field Value */
DO:

  /* Sanity checks */
  IF wEditor:WIDTH-PIXELS < 100 
    OR wEditor:HEIGHT-PIXELS < 100 THEN RETURN. 

  RUN LockWindow (INPUT wEditor:HANDLE, INPUT YES).

  DO WITH FRAME {&FRAME-NAME}:

    /* Make everything small so we don't get errors on resizing the window */
    btnOk:X     = 0.
    btnOk:Y     = 0.
    btnCancel:X = 0.
    btnCancel:Y = 0.
    edValue:X   = 0.
    edValue:Y   = 0.
    edValue:WIDTH-PIXELS = 10.
    edValue:HEIGHT-PIXELS = 10.
  
    /* Set frame width */
    FRAME {&FRAME-NAME}:WIDTH-PIXELS  = wEditor:WIDTH-PIXELS NO-ERROR.
    FRAME {&FRAME-NAME}:HEIGHT-PIXELS = wEditor:HEIGHT-PIXELS NO-ERROR.
  
    /* Adjust the browse */
    edValue:WIDTH-PIXELS  = FRAME {&FRAME-NAME}:WIDTH-PIXELS.
    edValue:HEIGHT-PIXELS = FRAME {&FRAME-NAME}:HEIGHT-PIXELS - 40.
    btnOk:X               = FRAME {&FRAME-NAME}:WIDTH-PIXELS - 155.
    btnOk:Y               = FRAME {&FRAME-NAME}:HEIGHT-PIXELS - 30.
    btnCancel:X           = FRAME {&FRAME-NAME}:WIDTH-PIXELS - 75.
    btnCancel:Y           = FRAME {&FRAME-NAME}:HEIGHT-PIXELS - 30.
  
    /* Save settings */
    setRegistry("DataDigger:ViewAsEditor", "Window:x", STRING(wEditor:X) ).                             
    setRegistry("DataDigger:ViewAsEditor", "Window:y", STRING(wEditor:Y) ).                             
    setRegistry("DataDigger:ViewAsEditor", "Window:height", STRING(wEditor:HEIGHT-PIXELS) ).                             
    setRegistry("DataDigger:ViewAsEditor", "Window:width", STRING(wEditor:WIDTH-PIXELS) ).                             
  END.

  RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO).
  RUN LockWindow (INPUT wEditor:HANDLE, INPUT NO).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnOk
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOk wEditor
ON CHOOSE OF btnOk IN FRAME DEFAULT-FRAME /* OK */
OR "F2" OF edValue
DO:
  pcValue = edValue:SCREEN-VALUE.
  APPLY "CLOSE" TO THIS-PROCEDURE. 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK wEditor 


/* ***************************  Main Block  *************************** */

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME} 
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
DO:
  /* Save settings */
  setRegistry("DataDigger:ViewAsEditor", "Window:x", STRING(wEditor:X) ).
  setRegistry("DataDigger:ViewAsEditor", "Window:y", STRING(wEditor:Y) ).
  setRegistry("DataDigger:ViewAsEditor", "Window:height", STRING(wEditor:HEIGHT-PIXELS) ).
  setRegistry("DataDigger:ViewAsEditor", "Window:width", STRING(wEditor:WIDTH-PIXELS) ).

  RUN disable_UI.
END.

/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  ASSIGN edValue = pcValue.  
  RUN enable_UI.
  RUN initializeObject.

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI wEditor  _DEFAULT-DISABLE
PROCEDURE disable_UI :
/*------------------------------------------------------------------------------
  Purpose:     DISABLE the User Interface
  Parameters:  <none>
  Notes:       Here we clean-up the user-interface by deleting
               dynamic widgets we have created and/or hide 
               frames.  This procedure is usually called when
               we are ready to "clean-up" after running.
------------------------------------------------------------------------------*/
  /* Delete the WINDOW we created */
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wEditor)
  THEN DELETE WIDGET wEditor.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI wEditor  _DEFAULT-ENABLE
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
  DISPLAY edValue 
      WITH FRAME DEFAULT-FRAME IN WINDOW wEditor.
  ENABLE edValue btnOk btnCancel 
      WITH FRAME DEFAULT-FRAME IN WINDOW wEditor.
  {&OPEN-BROWSERS-IN-QUERY-DEFAULT-FRAME}
  VIEW wEditor.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject wEditor 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE VARIABLE iValue AS INTEGER NO-UNDO.

  DO WITH FRAME {&FRAME-NAME}:

    /* Set default font */
    FRAME {&FRAME-NAME}:FONT = getFont('Default').
    edValue:FONT = getFont('Fixed').

    /* For some reasons, these #*$&# scrollbars keep coming back */
    .run showScrollBars(frame {&frame-name}:handle, no, no). /* KILL KILL KILL */
    
    /* Restore window */
    iValue = INTEGER(getRegistry('DataDigger:ViewAsEditor', 'Window:x' )).
    IF iValue > 0 THEN ASSIGN wEditor:X = iValue NO-ERROR.

    iValue = INTEGER(getRegistry('DataDigger:ViewAsEditor', 'Window:y' )).
    IF iValue > 0 THEN ASSIGN wEditor:Y = iValue NO-ERROR.

    iValue = INTEGER(getRegistry('DataDigger:ViewAsEditor', 'Window:height' )).
    IF iValue > 0 THEN ASSIGN wEditor:HEIGHT-PIXELS = iValue NO-ERROR.

    iValue = INTEGER(getRegistry('DataDigger:ViewAsEditor', 'Window:width' )).
    IF iValue > 0 THEN ASSIGN wEditor:WIDTH-PIXELS = iValue NO-ERROR.

  END.

  APPLY "window-resized" TO wEditor.

END PROCEDURE. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

