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

{ datadigger.i }

/* Parameters Definitions ---                                           */

/* Local Variable Definitions ---                                       */

/* Transparency */
PROCEDURE SetWindowLongA EXTERNAL "user32.dll":
 def INPUT PARAM HWND AS LONG.
 def INPUT PARAM nIndex AS LONG.
 def INPUT PARAM dwNewLong AS LONG.
 DEF RETURN PARAM stat AS LONG.
END.

PROCEDURE SetLayeredWindowAttributes EXTERNAL "user32.dll":
 def INPUT PARAM HWND AS LONG.
 def INPUT PARAM crKey AS LONG.
 def INPUT PARAM bAlpha AS SHORT.
 def INPUT PARAM dwFlagsas AS LONG.
 DEF RETURN PARAM stat AS SHORT.
END.

PROCEDURE setTransparency :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  define input  parameter phFrame as handle     no-undo.
  define input  parameter piLevel as integer    no-undo.
  
  &scop GWL_EXSTYLE         -20
  &scop WS_EX_LAYERED       524288
  &scop LWA_ALPHA           2
  &scop WS_EX_TRANSPARENT   32
  
  DEFINE VARIABLE stat AS INTEGER    NO-UNDO.

  /* Set WS_EX_LAYERED on this window  */
  RUN SetWindowLongA(phFrame:HWND, {&GWL_EXSTYLE}, {&WS_EX_LAYERED}, output stat).

  /* Make this window transparent (0 - 255) */
  RUN SetLayeredWindowAttributes(phFrame:HWND, 0, piLevel, {&LWA_ALPHA}, OUTPUT stat).

END PROCEDURE. /* setTransparency */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME Dialog-Frame

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS btnDataDigger BtnOK edChangelog 
&Scoped-Define DISPLAYED-OBJECTS edChangelog 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnDataDigger  NO-FOCUS FLAT-BUTTON
     LABEL "" 
     SIZE-PIXELS 30 BY 30.

DEFINE BUTTON BtnOK AUTO-GO DEFAULT 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE VARIABLE edChangelog AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP SCROLLBAR-VERTICAL LARGE
     SIZE-PIXELS 275 BY 90
     BGCOLOR 15 FGCOLOR 9 FONT 0 NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     btnDataDigger AT Y 5 X 5 WIDGET-ID 82
     BtnOK AT Y 105 X 140 WIDGET-ID 48
     edChangelog AT Y 5 X 45 NO-LABEL WIDGET-ID 72
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 355 BY 174
         BGCOLOR 14 
         TITLE "Hint"
         DEFAULT-BUTTON BtnOK WIDGET-ID 100.


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
       FRAME Dialog-Frame:SCROLLABLE       = FALSE.

ASSIGN 
       edChangelog:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* Hint */
DO:
  APPLY "END-ERROR":U TO SELF.
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

  frame {&frame-name}:hidden = yes.
  run enable_UI.
  run initializeObject.
  frame {&frame-name}:hidden = no.
  
  run fadeWindow(0,220).
  wait-for go of frame {&frame-name} focus edChangelog.
  run fadeWindow(220,0).
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
  DISPLAY edChangelog 
      WITH FRAME Dialog-Frame.
  ENABLE btnDataDigger BtnOK edChangelog 
      WITH FRAME Dialog-Frame.
  {&OPEN-BROWSERS-IN-QUERY-Dialog-Frame}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE fadeWindow Dialog-Frame 
PROCEDURE fadeWindow :
define input parameter piStartValue as integer no-undo.
  define input parameter piEndValue   as integer no-undo.

  define variable iStartTime   as integer    no-undo.
  define variable iTranparency as integer    no-undo.

  if piEndValue > piStartValue then 
  do iTranparency = piStartValue to piEndValue by 24:
    run setTransparency( input frame Dialog-Frame:handle, iTranparency).
    iStartTime = etime.
    do while etime < iStartTime + 40: end.
  end.

  else
  do iTranparency = piStartValue to piEndValue by -24:
    run setTransparency( input frame Dialog-Frame:handle, iTranparency).
    iStartTime = etime.
    do while etime < iStartTime + 40: end.
  end.

END PROCEDURE. /* fadeWindow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject Dialog-Frame 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  define buffer bQuery for ttQuery.

  do with frame {&frame-name}:

    if "{&uib_is_running}" = "" then 
    do:
      frame {&frame-name}:font = getFont('Default').
      edChangelog:font = getFont('Fixed').
    end.

    if "{&uib_is_running}" = "" then 
      btnDataDigger:load-image(getImagePath('DataDigger24x24.gif')).

    run setTransparency(input FRAME Dialog-Frame:handle, 1).

    /* For some reasons, these #*$&# scrollbars keep coming back */
    if "{&uib_is_running}" = "" then 
      run showScrollBars(frame {&frame-name}:handle, no, no). /* KILL KILL KILL */

  end.

end procedure. /* initializeObject. */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

