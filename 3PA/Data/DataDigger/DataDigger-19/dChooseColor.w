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
define {&invar}  piColorOld as integer no-undo.
define {&outvar} piColorNew as integer no-undo initial -1.

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
&Scoped-Define DISPLAYED-OBJECTS btn-0 btn-1 btn-2 btn-3 btn-4 btn-5 btn-6 ~
btn-7 btn-8 btn-9 btn-10 btn-11 btn-12 btn-13 btn-14 btn-15 btn-16 btn-17 ~
btn-18 btn-19 btn-20 btn-21 btn-22 btn-23 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON BtnCancel AUTO-END-KEY DEFAULT 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON Btn_OK AUTO-GO 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE VARIABLE btn-0 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-1 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-10 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-11 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-12 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-13 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-14 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-15 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-16 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-17 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-18 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-19 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-2 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-20 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-21 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-22 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-23 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-3 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-4 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-5 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-6 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-7 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-8 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE VARIABLE btn-9 AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 60 BY 42 NO-UNDO.

DEFINE RECTANGLE rcFocus
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 30 BY 20
     BGCOLOR 9 .

DEFINE RECTANGLE RECT-6
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 525 BY 155.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     btn-0 AT Y 10 X 5 NO-LABEL WIDGET-ID 108
     btn-1 AT Y 10 X 70 NO-LABEL WIDGET-ID 70
     btn-2 AT Y 10 X 125 COLON-ALIGNED NO-LABEL WIDGET-ID 112
     btn-3 AT Y 10 X 190 COLON-ALIGNED NO-LABEL WIDGET-ID 66
     btn-4 AT Y 10 X 255 COLON-ALIGNED NO-LABEL WIDGET-ID 68
     btn-5 AT Y 10 X 320 COLON-ALIGNED NO-LABEL WIDGET-ID 72
     btn-6 AT Y 10 X 385 COLON-ALIGNED NO-LABEL WIDGET-ID 74
     btn-7 AT Y 10 X 450 COLON-ALIGNED NO-LABEL WIDGET-ID 76
     btn-8 AT Y 60 X 5 NO-LABEL WIDGET-ID 78
     btn-9 AT Y 60 X 70 NO-LABEL WIDGET-ID 80
     btn-10 AT Y 60 X 125 COLON-ALIGNED NO-LABEL WIDGET-ID 114
     btn-11 AT Y 60 X 190 COLON-ALIGNED NO-LABEL WIDGET-ID 84
     btn-12 AT Y 60 X 255 COLON-ALIGNED NO-LABEL WIDGET-ID 86
     btn-13 AT Y 60 X 320 COLON-ALIGNED NO-LABEL WIDGET-ID 88
     btn-14 AT Y 60 X 385 COLON-ALIGNED NO-LABEL WIDGET-ID 90
     btn-15 AT Y 60 X 450 COLON-ALIGNED NO-LABEL WIDGET-ID 92
     btn-16 AT Y 110 X 5 NO-LABEL WIDGET-ID 94
     btn-17 AT Y 110 X 70 NO-LABEL WIDGET-ID 110
     btn-18 AT Y 110 X 125 COLON-ALIGNED NO-LABEL WIDGET-ID 116
     btn-19 AT Y 110 X 190 COLON-ALIGNED NO-LABEL WIDGET-ID 98
     btn-20 AT Y 110 X 255 COLON-ALIGNED NO-LABEL WIDGET-ID 100
     btn-21 AT Y 110 X 320 COLON-ALIGNED NO-LABEL WIDGET-ID 102
     btn-22 AT Y 110 X 385 COLON-ALIGNED NO-LABEL WIDGET-ID 104
     btn-23 AT Y 110 X 450 COLON-ALIGNED NO-LABEL WIDGET-ID 106
     BtnCancel AT Y 170 X 365 WIDGET-ID 54
     Btn_OK AT Y 170 X 445
     "Double click color to edit" VIEW-AS TEXT
          SIZE-PIXELS 175 BY 13 AT Y 180 X 5 WIDGET-ID 56
     RECT-6 AT Y 5 X 0 WIDGET-ID 50
     rcFocus AT Y 165 X 195 WIDGET-ID 52
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 538 BY 231
         TITLE "Choose Color"
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

/* SETTINGS FOR FILL-IN btn-0 IN FRAME Dialog-Frame
   ALIGN-L                                                              */
ASSIGN 
       btn-0:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

/* SETTINGS FOR FILL-IN btn-1 IN FRAME Dialog-Frame
   ALIGN-L                                                              */
ASSIGN 
       btn-1:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-10:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-11:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-12:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-13:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-14:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-15:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

/* SETTINGS FOR FILL-IN btn-16 IN FRAME Dialog-Frame
   ALIGN-L                                                              */
ASSIGN 
       btn-16:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

/* SETTINGS FOR FILL-IN btn-17 IN FRAME Dialog-Frame
   ALIGN-L                                                              */
ASSIGN 
       btn-17:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-18:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-19:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-2:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-20:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-21:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-22:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-23:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-3:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-4:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-5:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-6:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       btn-7:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

/* SETTINGS FOR FILL-IN btn-8 IN FRAME Dialog-Frame
   ALIGN-L                                                              */
ASSIGN 
       btn-8:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

/* SETTINGS FOR FILL-IN btn-9 IN FRAME Dialog-Frame
   ALIGN-L                                                              */
ASSIGN 
       btn-9:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

ASSIGN 
       rcFocus:HIDDEN IN FRAME Dialog-Frame           = TRUE.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* Choose Color */
DO:
  piColorNew = -1.
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btn-0
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON CURSOR-DOWN OF btn-0 IN FRAME Dialog-Frame
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
ON CURSOR-LEFT OF btn-0 IN FRAME Dialog-Frame
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
ON CURSOR-RIGHT OF btn-0 IN FRAME Dialog-Frame
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
ON CURSOR-UP OF btn-0 IN FRAME Dialog-Frame
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
ON ENTRY OF btn-0 IN FRAME Dialog-Frame
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:

  rcFocus:x = self:x - 3.
  rcFocus:y = self:y - 3.
  rcFocus:width-pixels = self:width-pixels + 6.
  rcFocus:height-pixels = self:height-pixels + 6.
  rcFocus:hidden = no.

  piColorNew = integer(entry(2,self:name,'-')).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON LEAVE OF btn-0 IN FRAME Dialog-Frame
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:
  rcFocus:hidden = yes.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btn-0 Dialog-Frame
ON MOUSE-SELECT-DBLCLICK OF btn-0 IN FRAME Dialog-Frame
, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23

or 'RETURN' of btn-0, btn-1, btn-2, btn-3, btn-4, btn-5, btn-6, btn-7, btn-8, btn-9, btn-10, btn-11, btn-12
, btn-13, btn-14, btn-15, btn-16, btn-17, btn-18, btn-19, btn-20, btn-21, btn-22, btn-23
DO:
  define variable iColorNr as integer no-undo. 

  iColorNr = integer(entry(2,self:name,'-')).

  if not color-table:get-dynamic(iColorNr) 
    and not color-table:set-dynamic(iColorNr,true)
  then 
    message "Color must be DYNAMIC to edit.".

  else 
  do:
    system-dialog color iColorNr.
    self:fgcolor = iColorNr. /* Font is same as name of button */
    self:bgcolor = iColorNr. /* Font is same as name of button */
  end.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME BtnCancel
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnCancel Dialog-Frame
ON CHOOSE OF BtnCancel IN FRAME Dialog-Frame /* Cancel */
DO:
  piColorNew = -1.
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
  DISPLAY btn-0 btn-1 btn-2 btn-3 btn-4 btn-5 btn-6 btn-7 btn-8 btn-9 btn-10 
          btn-11 btn-12 btn-13 btn-14 btn-15 btn-16 btn-17 btn-18 btn-19 btn-20 
          btn-21 btn-22 btn-23 
      WITH FRAME Dialog-Frame.
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

  /* Get fonts */
  frame {&frame-name}:font = getFont('Default').

  hButton = frame {&frame-name}:first-child:first-child. /* rectangle */
  repeat:
    hButton = hButton:next-sibling.
    if not valid-handle(hButton) then leave. 
    if not hButton:name matches 'btn-*' then next.

    iButtonNr = integer(entry(2,hButton:name,'-')).

    hButton:fgcolor = iButtonNr. /* Color is same as name of button */
    hButton:bgcolor = iButtonNr. /* Color is same as name of button */
    hButton:tooltip = 'Double click color to edit'.

    /* If this is the one that is specified in the input param
     * then set focus on this one by applying ENTRY.
     */
    if hButton:name = substitute('btn-&1', piColorOld) then
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

