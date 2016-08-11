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

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME Dialog-Frame

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS btnDataDigger BtnOK btnTabAbout edChangelog ~
btnTabChanges 
&Scoped-Define DISPLAYED-OBJECTS edChangelog fiDataDigger-1 fiDataDigger-2 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnDataDigger  NO-FOCUS FLAT-BUTTON
     LABEL "" 
     SIZE 6 BY 1.43.

DEFINE BUTTON BtnOK AUTO-GO DEFAULT 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON btnTabAbout  NO-FOCUS FLAT-BUTTON
     LABEL "About" 
     SIZE 16 BY 1.14.

DEFINE BUTTON btnTabChanges  NO-FOCUS FLAT-BUTTON
     LABEL "Changes" 
     SIZE 16 BY 1.14.

DEFINE VARIABLE edChangelog AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP SCROLLBAR-VERTICAL LARGE
     SIZE-PIXELS 625 BY 335
     FONT 0 NO-UNDO.

DEFINE VARIABLE fiDataDigger-1 AS CHARACTER FORMAT "X(256)":U INITIAL "DataDigger ~{&&version} - ~{&&edition}" 
      VIEW-AS TEXT 
     SIZE-PIXELS 275 BY 13
     FONT 0 NO-UNDO.

DEFINE VARIABLE fiDataDigger-2 AS CHARACTER FORMAT "X(256)":U INITIAL "Build ~{&&build}" 
      VIEW-AS TEXT 
     SIZE-PIXELS 155 BY 13
     FONT 0 NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     btnDataDigger AT ROW 1.24 COL 2 WIDGET-ID 82
     BtnOK AT Y 5 X 545 WIDGET-ID 48
     btnTabAbout AT ROW 3.24 COL 1 WIDGET-ID 78
     edChangelog AT Y 70 X 0 NO-LABEL WIDGET-ID 72
     btnTabChanges AT ROW 3.24 COL 17 WIDGET-ID 80
     fiDataDigger-1 AT Y 5 X 35 COLON-ALIGNED NO-LABEL WIDGET-ID 74
     fiDataDigger-2 AT Y 20 X 35 COLON-ALIGNED NO-LABEL WIDGET-ID 76
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 635 BY 439
         TITLE "About DataDigger"
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

/* SETTINGS FOR FILL-IN fiDataDigger-1 IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN fiDataDigger-2 IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* About DataDigger */
DO:
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTabAbout
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabAbout Dialog-Frame
ON CHOOSE OF btnTabAbout IN FRAME Dialog-Frame /* About */
or 'ctrl-1' of frame {&frame-name} anywhere
DO:
  run setPage(1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTabChanges
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabChanges Dialog-Frame
ON CHOOSE OF btnTabChanges IN FRAME Dialog-Frame /* Changes */
or 'ctrl-2' of frame {&frame-name} anywhere
DO:
  run setPage(2).
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
  
  run fadeWindow(0,240).
  wait-for go of frame {&frame-name} focus edChangelog.
  run fadeWindow(240,0).
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
  DISPLAY edChangelog fiDataDigger-1 fiDataDigger-2 
      WITH FRAME Dialog-Frame.
  ENABLE btnDataDigger BtnOK btnTabAbout edChangelog btnTabChanges 
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
    do while etime < iStartTime + 20: end.
  end.

  else
  do iTranparency = piStartValue to piEndValue by -24:
    run setTransparency( input frame Dialog-Frame:handle, iTranparency).
    iStartTime = etime.
    do while etime < iStartTime + 20: end.
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

    frame {&frame-name}:font = getFont('Default').
    fiDataDigger-1:font = getFont('Fixed').
    fiDataDigger-2:font = getFont('Fixed').
    edChangelog:font = getFont('Fixed').

    btnDataDigger:load-image(getImagePath('DataDigger24x24.gif')).
    RUN setPage(1).

    fiDataDigger-1:screen-value = "DataDigger {&version} - {&edition}".
    fiDataDigger-2:screen-value = 'Build {&build}'.

    run setTransparency(input FRAME Dialog-Frame:handle, 1).

    /* For some reasons, these #*$&# scrollbars keep coming back */
    run showScrollBars(frame {&frame-name}:handle, no, no). /* KILL KILL KILL */

  end.

end procedure. /* initializeObject. */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setPage Dialog-Frame 
PROCEDURE setPage :
/*------------------------------------------------------------------------
  Name         : setPage
  Description  : Activate either the About or the Changes tab

  ----------------------------------------------------------------------
  7-9-2012 pti Created
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER piPage AS INTEGER     NO-UNDO.

  do with frame {&frame-name}:
    edChangelog:SCREEN-VALUE = "".

    case piPage:
      when 1 then do:
        btnTabAbout  :load-image( getImagePath('tab_about_active.gif'    )).
        btnTabChanges:load-image( getImagePath('tab_changes_inactive.gif' )).

        edChangeLog:insert-file(getProgramDir() + 'DataDiggerAbout.txt').
        edChangeLog:cursor-offset = 1.
      end.
  
      when 2 then do:
        btnTabAbout  :load-image( getImagePath('tab_about_inactive.gif'    )).
        btnTabChanges:load-image( getImagePath('tab_changes_active.gif' )).

        edChangeLog:insert-file(getProgramDir() + 'DataDigger.txt').
        edChangeLog:cursor-offset = 1.
      end.                                          
    end case. /* piPage */
  end.
  
END PROCEDURE. /* setPage */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
