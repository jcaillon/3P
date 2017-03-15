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

/* Parameters Definitions ---                                           */
{ datadigger.i }

/* Local Variable Definitions ---                                       */
define {&invar}  piFontOld as integer no-undo.
define {&outvar} piFontNew as integer no-undo initial -1.

define temp-table ttButton no-undo
  field x as integer
  field y as integer
  field h as handle
  .

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME Dialog-Frame

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS btn-0 btn-1 btn-2 btn-3 btn-4 btn-5 btn-6 ~
btn-7 btn-8 btn-9 btn-10 btn-11 btn-12 btn-13 btn-14 btn-15 btn-16 btn-17 ~
btn-18 btn-19 btn-20 btn-21 btn-22 btn-23 BtnCancel Btn_OK RECT-6 rcFocus 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON btn-0 
     LABEL "Font 0" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-1 
     LABEL "Font 1" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-10 
     LABEL "Font 10" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-11 
     LABEL "Font 11" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-12 
     LABEL "Font 12" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-13 
     LABEL "Font 13" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-14 
     LABEL "Font 14" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-15 
     LABEL "Font 15" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-16 
     LABEL "Font 16" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-17 
     LABEL "Font 17" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-18 
     LABEL "Font 18" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit"
     FONT 18.

DEFINE BUTTON btn-19 
     LABEL "Font 19" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-2 
     LABEL "Font 2" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-20 
     LABEL "Font 20" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-21 
     LABEL "Font 21" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-22 
     LABEL "Font 22" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-23 
     LABEL "Font 23" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-3 
     LABEL "Font 3" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-4 
     LABEL "Font 4" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-5 
     LABEL "Font 5" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-6 
     LABEL "Font 6" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-7 
     LABEL "Font 7" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-8 
     LABEL "Font 8" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON btn-9 
     LABEL "Font 9" 
     SIZE-PIXELS 200 BY 42 TOOLTIP "Double click font to edit".

DEFINE BUTTON BtnCancel AUTO-END-KEY DEFAULT 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON Btn_OK AUTO-GO 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE RECTANGLE rcFocus
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 30 BY 20
     BGCOLOR 9 .

DEFINE RECTANGLE RECT-6
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 830 BY 305.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     btn-0 AT Y 10 X 5 WIDGET-ID 10
     btn-1 AT Y 10 X 210 WIDGET-ID 2
     btn-2 AT Y 10 X 415 WIDGET-ID 4
     btn-3 AT Y 10 X 620 WIDGET-ID 8
     btn-4 AT Y 60 X 5 WIDGET-ID 6
     btn-5 AT Y 60 X 210 WIDGET-ID 12
     btn-6 AT Y 60 X 415 WIDGET-ID 14
     btn-7 AT Y 60 X 620 WIDGET-ID 16
     btn-8 AT Y 110 X 5 WIDGET-ID 18
     btn-9 AT Y 110 X 210 WIDGET-ID 20
     btn-10 AT Y 110 X 415 WIDGET-ID 22
     btn-11 AT Y 110 X 620 WIDGET-ID 24
     btn-12 AT Y 160 X 5 WIDGET-ID 26
     btn-13 AT Y 160 X 210 WIDGET-ID 28
     btn-14 AT Y 160 X 415 WIDGET-ID 30
     btn-15 AT Y 160 X 620 WIDGET-ID 32
     btn-16 AT Y 210 X 5 WIDGET-ID 34
     btn-17 AT Y 210 X 210 WIDGET-ID 36
     btn-18 AT Y 210 X 415 WIDGET-ID 38
     btn-19 AT Y 210 X 620 WIDGET-ID 40
     btn-20 AT Y 260 X 5 WIDGET-ID 42
     btn-21 AT Y 260 X 210 WIDGET-ID 44
     btn-22 AT Y 260 X 415 WIDGET-ID 46
     btn-23 AT Y 260 X 620 WIDGET-ID 48
     BtnCancel AT Y 325 X 660 WIDGET-ID 54
     Btn_OK AT Y 325 X 740
     "Double click font to edit" VIEW-AS TEXT
          SIZE-PIXELS 125 BY 13 AT Y 320 X 10 WIDGET-ID 56
     RECT-6 AT Y 5 X 0 WIDGET-ID 50
     rcFocus AT Y 315 X 150 WIDGET-ID 52
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 847 BY 388
         TITLE "Choose Font"
         DEFAULT-BUTTON Btn_OK CANCEL-BUTTON BtnCancel WIDGET-ID 100.


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
   FRAME-NAME Custom                                                    */
ASSIGN 
       FRAME Dialog-Frame:SCROLLABLE       = FALSE
       FRAME Dialog-Frame:HIDDEN           = TRUE.

ASSIGN 
       rcFocus:HIDDEN IN FRAME Dialog-Frame           = TRUE.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* Choose Font */
DO:
  piFontNew = -1.
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btn-0
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON CURSOR-DOWN OF btn-0 IN FRAME Dialog-Frame /* Font 0 */
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:

  define variable hTarget as handle no-undo. 
  define buffer bButton for ttButton.

  for each bButton 
    where bButton.x = self:x 
      and bButton.y > self:y 
       by bButton.y:
    hTarget = bButton.h.
    leave.
  end. 

  if valid-handle(hTarget) then
    apply 'entry' to hTarget.

  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON CURSOR-LEFT OF btn-0 IN FRAME Dialog-Frame /* Font 0 */
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:

  define variable hTarget as handle no-undo. 
  define buffer bButton for ttButton.

  for each bButton 
    where bButton.y = self:y 
      and bButton.x < self:x 
       by bButton.x descending:
    hTarget = bButton.h.
    leave.
  end. 

  if valid-handle(hTarget) then
    apply 'entry' to hTarget.

  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON CURSOR-RIGHT OF btn-0 IN FRAME Dialog-Frame /* Font 0 */
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:

  define variable hTarget as handle no-undo. 
  define buffer bButton for ttButton.

  for each bButton 
    where bButton.y = self:y 
      and bButton.x > self:x 
       by bButton.x:
    hTarget = bButton.h.
    leave.
  end. 

  if valid-handle(hTarget) then
    apply 'entry' to hTarget.

  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON CURSOR-UP OF btn-0 IN FRAME Dialog-Frame /* Font 0 */
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:

  define variable hTarget as handle no-undo. 
  define buffer bButton for ttButton.

  for each bButton 
    where bButton.x = self:x 
      and bButton.y < self:y 
       by bButton.y descending:
    hTarget = bButton.h.
    leave.
  end. 

  if valid-handle(hTarget) then
    apply 'entry' to hTarget.

  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON ENTRY OF btn-0 IN FRAME Dialog-Frame /* Font 0 */
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:

  rcFocus:x = self:x - 3.
  rcFocus:y = self:y - 3.
  rcFocus:width-pixels = self:width-pixels + 6.
  rcFocus:height-pixels = self:height-pixels + 6.
  rcFocus:hidden = no.

  piFontNew = integer(entry(2,self:name,'-')).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON LEAVE OF btn-0 IN FRAME Dialog-Frame /* Font 0 */
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:
  rcFocus:hidden = yes.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON MOUSE-SELECT-DBLCLICK OF btn-0 IN FRAME Dialog-Frame /* Font 0 */
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23

or 'RETURN' of btn-0, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23

DO:
  define variable iFontNr as integer no-undo. 

  iFontNr = integer(entry(2,self:name,'-')).

  system-dialog font iFontNr.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME BtnCancel
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnCancel Dialog-Frame
ON CHOOSE OF BtnCancel IN FRAME Dialog-Frame /* Cancel */
DO:
  piFontNew = -1.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Dialog-Frame 


/* ***************************  Main Block  *************************** */

/* Parent the dialog-box to the ACTIVE-WINDOW, if there is no parent.   */
IF VALID-HANDLE(ACTIVE-WINDOW) AND FRAME {&FRAME-NAME}:PARENT eq ?
THEN FRAME {&FRAME-NAME}:PARENT = ACTIVE-WINDOW.


/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  run enable_UI.
  run initializeObjects.

  WAIT-FOR GO OF FRAME {&FRAME-NAME}.
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
  ENABLE btn-0 btn-1 btn-2 btn-3 btn-4 btn-5 btn-6 btn-7 btn-8 btn-9 btn-10 
         btn-11 btn-12 btn-13 btn-14 btn-15 btn-16 btn-17 btn-18 btn-19 btn-20 
         btn-21 btn-22 btn-23 BtnCancel Btn_OK RECT-6 rcFocus 
      WITH FRAME Dialog-Frame.
  VIEW FRAME Dialog-Frame.
  {&OPEN-BROWSERS-IN-QUERY-Dialog-Frame}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObjects Dialog-Frame 
PROCEDURE initializeObjects :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  define variable iButtonNr as integer no-undo. 
  define variable hButton   as handle  no-undo.
  define variable cFontName as character no-undo. 

  /* Get fonts */
  frame {&frame-name}:font = getFont('Default').

  hButton = frame {&frame-name}:first-child:first-child. /* rectangle */
  repeat:
    hButton = hButton:next-sibling.
    if not valid-handle(hButton) then leave. 
    if not valid-handle(hButton) then next.
    if not hButton:name matches 'btn-*' then next. 

    iButtonNr = integer(entry(2,hButton:name,'-')).
    hButton:font = iButtonNr. /* Font is same as name of button */

    /* Get the name of the font from the progress.ini */
    get-key-value section 'fonts'
      key 'font' + string(iButtonNr)
      value cFontName.

    if cFontName <> ? then 
      hButton:label = substitute('&1: &2', iButtonNr, cFontName).

    /* If this is the one that is specified in the input param
     * then set focus on this one by applying ENTRY.
     */
    if hButton:name = substitute('btn-&1', piFontOld) then
      apply 'entry' to hButton.

    /* Save button props */
    create ttButton.
    assign ttButton.x = hButton:x
           ttButton.y = hButton:y
           ttButton.h = hButton
           .
  end.
  
  /* For some reasons, these #*$&# scrollbars keep coming back */
  run showScrollBars(frame {&frame-name}:handle, no, no). /* KILL KILL KILL */

end procedure. /* initializeObjects */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

