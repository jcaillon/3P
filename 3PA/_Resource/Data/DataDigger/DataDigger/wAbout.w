&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME wAbout
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS wAbout 
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

{ datadigger.i }

DEFINE TEMP-TABLE ttBrick NO-UNDO
  FIELD cBlockId AS CHARACTER 
  FIELD hBrick   AS HANDLE 
  FIELD iLine    AS INTEGER
  FIELD x1       AS INTEGER
  FIELD x2       AS INTEGER
  FIELD y1       AS INTEGER
  FIELD y2       AS INTEGER
  INDEX iPrim IS PRIMARY cBlockId
  INDEX iPos x1 x2 y1 y2
  .

DEFINE TEMP-TABLE ttScores NO-UNDO
  FIELD iRank AS INTEGER
  FIELD cName AS CHARACTER
  FIELD cTime AS CHARACTER
  INDEX iPrim IS PRIMARY iRank
  .

/* For debugging in the UIB */
/*&IF DEFINED(UIB_is_Running) <> 0 &THEN*/

RUN startDiggerLib.

PROCEDURE startDiggerLib :
/* Start DiggerLib if it has not already been started
 */
  DEFINE VARIABLE hDiggerLib AS HANDLE    NO-UNDO.
  DEFINE VARIABLE cDiggerLib AS CHARACTER NO-UNDO.

  /* Call out to see if the lib has been started */
  PUBLISH 'DataDiggerLib' (OUTPUT hDiggerLib).

  IF NOT VALID-HANDLE(hDiggerLib) THEN
  DO:
    /* gcProgramDir = SUBSTRING(THIS-PROCEDURE:FILE-NAME,1,R-INDEX(THIS-PROCEDURE:FILE-NAME,'\')). */
    cDiggerLib = THIS-PROCEDURE:FILE-NAME.
    cDiggerLib = REPLACE(cDiggerLib,"\","/").
    cDiggerLib = SUBSTRING(cDiggerLib,1,R-INDEX(cDiggerLib,'/')) + 'DataDiggerLib.p'.
    IF SEARCH(cDiggerLib) = ? THEN cDiggerLib = 'd:\data\progress\DataDigger\DataDiggerLib.p'.
    IF SEARCH(cDiggerLib) = ? THEN cDiggerLib = 'd:\data\dropbox\DataDigger\src\DataDiggerLib.p'.
    IF SEARCH(cDiggerLib) = ? THEN cDiggerLib = 'c:\data\dropbox\DataDigger\src\DataDiggerLib.p'.
    RUN VALUE(cDiggerLib) PERSISTENT SET hDiggerLib.
    SESSION:ADD-SUPER-PROCEDURE(hDiggerLib,SEARCH-TARGET).
  END.

END PROCEDURE. /* startDiggerLib */

/*&ENDIF*/

DEFINE VARIABLE giBallX       AS INTEGER   NO-UNDO INITIAL -5.
DEFINE VARIABLE giBallY       AS INTEGER   NO-UNDO INITIAL -5.
DEFINE VARIABLE giGameStarted AS INTEGER   NO-UNDO INITIAL ?.
DEFINE VARIABLE giOldMouseX   AS INTEGER   NO-UNDO.
DEFINE VARIABLE gcGameStatus  AS CHARACTER NO-UNDO.
DEFINE VARIABLE glDebugRun    AS LOGICAL   NO-UNDO INITIAL NO.
DEFINE VARIABLE giNumLives    AS INTEGER   NO-UNDO INITIAL 3.
DEFINE VARIABLE giNumTicks    AS INTEGER   NO-UNDO.
DEFINE VARIABLE glUseTimer    AS LOGICAL   NO-UNDO.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME DEFAULT-FRAME

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS btnDataDigger BtnOK edChangelog fiWebsite 
&Scoped-Define DISPLAYED-OBJECTS edChangelog fiDataDigger-1 fiDataDigger-2 ~
fiWebsite 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wAbout AS WIDGET-HANDLE NO-UNDO.

/* Definitions of handles for OCX Containers                            */
DEFINE VARIABLE CtrlFrame AS WIDGET-HANDLE NO-UNDO.
DEFINE VARIABLE chCtrlFrame AS COMPONENT-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnDataDigger  NO-FOCUS
     LABEL "D" 
     SIZE-PIXELS 30 BY 30.

DEFINE BUTTON BtnOK AUTO-GO DEFAULT 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

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

DEFINE VARIABLE fiMsg AS CHARACTER FORMAT "X(256)":U INITIAL "(Click to close window)" 
      VIEW-AS TEXT 
     SIZE-PIXELS 170 BY 20
     BGCOLOR 14 FGCOLOR 9  NO-UNDO.

DEFINE VARIABLE fiWebsite AS CHARACTER FORMAT "X(256)":U INITIAL "https://datadigger.wordpress.com/" 
      VIEW-AS TEXT 
     SIZE-PIXELS 210 BY 20
     FGCOLOR 9  NO-UNDO.

DEFINE IMAGE imgBall
     FILENAME "adeicon/blank":U TRANSPARENT
     SIZE-PIXELS 15 BY 15.

DEFINE IMAGE imgPaddle
     FILENAME "adeicon/blank":U TRANSPARENT
     SIZE-PIXELS 70 BY 15.

DEFINE IMAGE imgPaddle-2
     FILENAME "adeicon/blank":U
     STRETCH-TO-FIT TRANSPARENT
     SIZE-PIXELS 20 BY 10.

DEFINE IMAGE imgPaddle-3
     FILENAME "adeicon/blank":U
     STRETCH-TO-FIT TRANSPARENT
     SIZE-PIXELS 20 BY 10.

DEFINE IMAGE imgPaddle-4
     FILENAME "adeicon/blank":U
     STRETCH-TO-FIT TRANSPARENT
     SIZE-PIXELS 20 BY 10.

DEFINE IMAGE imgTitle
     FILENAME "adeicon/blank":U
     STRETCH-TO-FIT TRANSPARENT
     SIZE-PIXELS 64 BY 64.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME DEFAULT-FRAME
     btnDataDigger AT Y 5 X 5 WIDGET-ID 82
     BtnOK AT Y 5 X 545 WIDGET-ID 48
     edChangelog AT Y 70 X 0 NO-LABEL WIDGET-ID 72
     fiDataDigger-1 AT Y 5 X 35 COLON-ALIGNED NO-LABEL WIDGET-ID 74
     fiDataDigger-2 AT Y 20 X 35 COLON-ALIGNED NO-LABEL WIDGET-ID 76
     fiMsg AT Y 190 X 190 COLON-ALIGNED NO-LABEL WIDGET-ID 320
     fiWebsite AT Y 415 X 190 COLON-ALIGNED NO-LABEL WIDGET-ID 298
     imgPaddle AT Y 15 X 325 WIDGET-ID 300
     imgBall AT Y 15 X 305 WIDGET-ID 302
     imgPaddle-2 AT Y 35 X 325 WIDGET-ID 308
     imgPaddle-3 AT Y 35 X 350 WIDGET-ID 310
     imgTitle AT Y 45 X 101 WIDGET-ID 314
     imgPaddle-4 AT Y 35 X 375 WIDGET-ID 312
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT COL 1 ROW 1
         SIZE 126.6 BY 21.71 WIDGET-ID 100.


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
  CREATE WINDOW wAbout ASSIGN
         HIDDEN             = YES
         TITLE              = "About the DataDigger"
         HEIGHT             = 21.71
         WIDTH              = 126.6
         MAX-HEIGHT         = 54
         MAX-WIDTH          = 384
         VIRTUAL-HEIGHT     = 54
         VIRTUAL-WIDTH      = 384
         MIN-BUTTON         = no
         MAX-BUTTON         = no
         RESIZE             = no
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
/* SETTINGS FOR WINDOW wAbout
  NOT-VISIBLE,,RUN-PERSISTENT                                           */
/* SETTINGS FOR FRAME DEFAULT-FRAME
   FRAME-NAME                                                           */
ASSIGN 
       FRAME DEFAULT-FRAME:HIDDEN           = TRUE.

ASSIGN 
       edChangelog:READ-ONLY IN FRAME DEFAULT-FRAME        = TRUE.

/* SETTINGS FOR FILL-IN fiDataDigger-1 IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN fiDataDigger-2 IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN fiMsg IN FRAME DEFAULT-FRAME
   NO-DISPLAY NO-ENABLE                                                 */
ASSIGN 
       fiMsg:HIDDEN IN FRAME DEFAULT-FRAME           = TRUE
       fiMsg:READ-ONLY IN FRAME DEFAULT-FRAME        = TRUE.

/* SETTINGS FOR IMAGE imgBall IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
ASSIGN 
       imgBall:HIDDEN IN FRAME DEFAULT-FRAME           = TRUE.

/* SETTINGS FOR IMAGE imgPaddle IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
ASSIGN 
       imgPaddle:HIDDEN IN FRAME DEFAULT-FRAME           = TRUE.

/* SETTINGS FOR IMAGE imgPaddle-2 IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
ASSIGN 
       imgPaddle-2:HIDDEN IN FRAME DEFAULT-FRAME           = TRUE.

/* SETTINGS FOR IMAGE imgPaddle-3 IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
ASSIGN 
       imgPaddle-3:HIDDEN IN FRAME DEFAULT-FRAME           = TRUE.

/* SETTINGS FOR IMAGE imgPaddle-4 IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
ASSIGN 
       imgPaddle-4:HIDDEN IN FRAME DEFAULT-FRAME           = TRUE.

/* SETTINGS FOR IMAGE imgTitle IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
ASSIGN 
       imgTitle:HIDDEN IN FRAME DEFAULT-FRAME           = TRUE.

IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wAbout)
THEN wAbout:HIDDEN = yes.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 


/* **********************  Create OCX Containers  ********************** */

&ANALYZE-SUSPEND _CREATE-DYNAMIC

&IF "{&OPSYS}" = "WIN32":U AND "{&WINDOW-SYSTEM}" NE "TTY":U &THEN

CREATE CONTROL-FRAME CtrlFrame ASSIGN
       FRAME           = FRAME DEFAULT-FRAME:HANDLE
       ROW             = 1.95
       COLUMN          = 82.8
       HEIGHT          = 1.43
       WIDTH           = 6
       WIDGET-ID       = 292
       HIDDEN          = yes
       SENSITIVE       = yes.
/* CtrlFrame OCXINFO:CREATE-CONTROL from: {F0B88A90-F5DA-11CF-B545-0020AF6ED35A} type: BallTimer */
      CtrlFrame:MOVE-AFTER(BtnOK:HANDLE IN FRAME DEFAULT-FRAME).

&ENDIF

&ANALYZE-RESUME /* End of _CREATE-DYNAMIC */


/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME wAbout
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wAbout wAbout
ON END-ERROR OF wAbout /* About the DataDigger */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:

  APPLY 'CLOSE' TO THIS-PROCEDURE.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wAbout wAbout
ON WINDOW-CLOSE OF wAbout /* About the DataDigger */
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME DEFAULT-FRAME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL DEFAULT-FRAME wAbout
ON F12 OF FRAME DEFAULT-FRAME
ANYWHERE DO:

  RUN gameOver(YES).
  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDataDigger
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDataDigger wAbout
ON CHOOSE OF btnDataDigger IN FRAME DEFAULT-FRAME /* D */
DO:
  IF glUseTimer THEN RUN showLog.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME BtnOK
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnOK wAbout
ON CHOOSE OF BtnOK IN FRAME DEFAULT-FRAME /* OK */
DO:
  APPLY 'CLOSE' TO THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME CtrlFrame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL CtrlFrame wAbout OCX.Tick
PROCEDURE CtrlFrame.BallTimer.Tick .
/*------------------------------------------------------------------------------
    Name : BallTimer.ocx.tick
    Desc : Move the ball
  ------------------------------------------------------------------------------*/

  /* set paddle only once every few ticks */
  IF giNumTicks MODULO 2 = 0 THEN RUN setPaddle.
  giNumTicks = giNumTicks + 1.

  RUN moveBall.
  
END PROCEDURE. /* OCX.Tick */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fiWebsite
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiWebsite wAbout
ON MOUSE-SELECT-CLICK OF fiWebsite IN FRAME DEFAULT-FRAME
DO:
  
  OS-COMMAND NO-WAIT START VALUE(SELF:SCREEN-VALUE).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK wAbout 


/* ***************************  Main Block  *************************** */

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME} 
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
   RUN disable_UI.

/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  FRAME {&FRAME-NAME}:HIDDEN = YES.
  RUN initializeUi.

  RUN initializeObject.
  FRAME {&FRAME-NAME}:HIDDEN = NO.
  RUN blinkLogo. 
  
  WAIT-FOR CLOSE OF THIS-PROCEDURE FOCUS edChangelog.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE blinkLogo wAbout 
PROCEDURE blinkLogo :
/* Blink the DD logo
*/
  DEFINE VARIABLE ii   AS INTEGER NO-UNDO.
  DEFINE VARIABLE xx   AS DECIMAL NO-UNDO.
  DEFINE VARIABLE yy   AS DECIMAL NO-UNDO.
  DEFINE VARIABLE dx   AS DECIMAL NO-UNDO INIT -5. /* hor speed */
  DEFINE VARIABLE dy   AS DECIMAL NO-UNDO INIT 0.  /* ver speed */
  DEFINE VARIABLE grav AS DECIMAL NO-UNDO INIT .2. /* gravity acceleration */

  /* debug */
  IF glDebugRun OR NOT glUseTimer THEN
  DO WITH FRAME {&FRAME-NAME}:
    btnDataDigger:X = 5.
    btnDataDigger:Y = 5.
    RETURN. 
  END. 

  DO WITH FRAME {&FRAME-NAME}:

    btnDataDigger:MOVE-TO-TOP().
    btnDataDigger:X = 600.
    btnDataDigger:Y = 0.
    btnDataDigger:VISIBLE = FALSE.

    yy = btnDataDigger:Y.
    xx = btnDataDigger:X.

    RUN justWait(1000).
    btnDataDigger:VISIBLE = TRUE.
  END.

  REPEAT:
    /* Normal flow */
    dy = dy + grav.
    xx = xx + dx.
    yy = yy + dy.
    
    /* Bounce at bottom of frame */
    IF yy > (FRAME {&FRAME-NAME}:HEIGHT-PIXELS - btnDataDigger:HEIGHT-PIXELS) THEN
    DO:
      yy = FRAME {&FRAME-NAME}:HEIGHT-PIXELS - btnDataDigger:HEIGHT-PIXELS.
      dy = -1 * dy.
    END.
    IF xx <= 5 THEN LEAVE. 

    btnDataDigger:X = xx.
    btnDataDigger:Y = yy.

    RUN justWait(10). 
  END. 

  btnDataDigger:X = 5.
  btnDataDigger:Y = 5.

  DO ii = 1 TO 3:
    btnDataDigger:SENSITIVE = NO.
    RUN justWait(300). 
    btnDataDigger:SENSITIVE = YES.
    RUN justWait(300). 
  END.


END PROCEDURE. /* blinkLogo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE control_load wAbout  _CONTROL-LOAD
PROCEDURE control_load :
/*------------------------------------------------------------------------------
  Purpose:     Load the OCXs    
  Parameters:  <none>
  Notes:       Here we load, initialize and make visible the 
               OCXs in the interface.                        
------------------------------------------------------------------------------*/

&IF "{&OPSYS}" = "WIN32":U AND "{&WINDOW-SYSTEM}" NE "TTY":U &THEN
DEFINE VARIABLE UIB_S    AS LOGICAL    NO-UNDO.
DEFINE VARIABLE OCXFile  AS CHARACTER  NO-UNDO.

OCXFile = SEARCH( "wAbout.wrx":U ).
IF OCXFile = ? THEN
  OCXFile = SEARCH(SUBSTRING(THIS-PROCEDURE:FILE-NAME, 1,
                     R-INDEX(THIS-PROCEDURE:FILE-NAME, ".":U), "CHARACTER":U) + "wrx":U).

IF OCXFile <> ? THEN
DO:
  ASSIGN
    chCtrlFrame = CtrlFrame:COM-HANDLE
    UIB_S = chCtrlFrame:LoadControls( OCXFile, "CtrlFrame":U)
    CtrlFrame:NAME = "CtrlFrame":U
  .
  RUN initialize-controls IN THIS-PROCEDURE NO-ERROR.
END.
ELSE MESSAGE "wAbout.wrx":U SKIP(1)
             "The binary control file could not be found. The controls cannot be loaded."
             VIEW-AS ALERT-BOX TITLE "Controls Not Loaded".

&ENDIF

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE createBricks wAbout 
PROCEDURE createBricks :
/* Build blocks on the screen via button widgets
 */
 DEFINE BUFFER bfBrick FOR ttBrick.
 
 FOR EACH bfBrick:

   CREATE FILL-IN bfBrick.hBrick
     ASSIGN
       X             = 1
       Y             = 1
       FONT          = getFont('fixed')
       FRAME         = FRAME {&FRAME-NAME}:HANDLE
       SENSITIVE     = TRUE
       VISIBLE       = FALSE
       WIDTH-PIXELS  = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(bfBrick.cBlockId, getFont('fixed')) + 15
       HEIGHT-PIXELS = FONT-TABLE:GET-TEXT-HEIGHT-PIXELS(getFont('fixed')) + 8
       FORMAT        = 'X(100)'
       .

 END.

END PROCEDURE. /* createBricks */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI wAbout  _DEFAULT-DISABLE
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
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wAbout)
  THEN DELETE WIDGET wAbout.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE gameOver wAbout 
PROCEDURE gameOver :
/* Game over
 **/
 DEFINE INPUT PARAMETER plPlayerLost AS LOGICAL NO-UNDO.

 DO WITH FRAME {&FRAME-NAME}:
   gcGameStatus = 'Game Over'.
   
   /* Disable ball mover */
   chCtrlFrame:BallTimer:ENABLED = FALSE.  

   /* Load proper image: 'Game Over' or 'Good Job' */
   IF plPlayerLost THEN
     imgTitle:LOAD-IMAGE(getImagePath('AboutTitle2.gif')).
   ELSE 
     imgTitle:LOAD-IMAGE(getImagePath('AboutTitle3.gif')).

   imgTitle:WIDTH-PIXELS = 600.
   imgTitle:HEIGHT-PIXELS = 100.
   imgTitle:X = 250.
   imgTitle:Y = 300.
   imgTitle:VISIBLE = TRUE.

   /* Show 'Click to close' message */
   RUN justWait(1000).
   ASSIGN 
     fiMsg:FONT          = getFont('fixed')
     fiMsg:SCREEN-VALUE  = '(Click to close window)'
     fiMsg:WIDTH-PIXELS  = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(fiMsg:SCREEN-VALUE, getFont('fixed')) + 10
     fiMsg:HEIGHT-PIXELS = FONT-TABLE:GET-TEXT-HEIGHT-PIXELS(getFont('fixed'))
     fiMsg:X             = (FRAME {&FRAME-NAME}:WIDTH-PIXELS - fiMsg:WIDTH-PIXELS) / 2
     fiMsg:Y             = 450
     fiMsg:VISIBLE       = YES.
     fiMsg:SENSITIVE     = YES
     .

 END.

END PROCEDURE. /* gameOver */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE hitBottom wAbout 
PROCEDURE hitBottom :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE VARIABLE ii AS INTEGER     NO-UNDO.

  DO ii = 1 TO 3:
    RUN justWait(100).
    FRAME {&FRAME-NAME}:BGCOLOR = 14.
    RUN justWait(100).
    FRAME {&FRAME-NAME}:BGCOLOR = ?.
  END.

  giNumLives = giNumLives - 1.
  IF giNumLives = 0 THEN 
  DO:
    RUN gameOver(YES).
  END.
  ELSE 
  DO:
    RUN setBall(NO).
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject wAbout 
PROCEDURE initializeObject :
/* Init frame
*/
  DEFINE BUFFER bQuery FOR ttQuery.

  DO WITH FRAME {&FRAME-NAME}:

    wAbout:VISIBLE = FALSE. 

    /* Position frame relative to main window */
    wAbout:X = MAXIMUM(0, INTEGER(getRegistry('DataDigger', 'Window:x' )) - 50).
    wAbout:Y = MAXIMUM(0, INTEGER(getRegistry('DataDigger', 'Window:y' )) - 20).

    /* Prepare frame */
    FRAME {&FRAME-NAME}:FONT = getFont('Default').
    fiDataDigger-1:FONT      = getFont('Fixed').
    fiDataDigger-2:FONT      = getFont('Fixed').
    edChangelog:FONT         = getFont('Fixed').

    btnDataDigger:LOAD-IMAGE(getImagePath('DataDigger24x24.gif')).
    imgBall:LOAD-IMAGE(getImagePath('Ball.gif')).
    imgPaddle:LOAD-IMAGE(getImagePath('Paddle.gif')).

    imgPaddle-2:LOAD-IMAGE(getImagePath('Paddle.gif')).
    imgPaddle-3:LOAD-IMAGE(getImagePath('Paddle.gif')).
    imgPaddle-4:LOAD-IMAGE(getImagePath('Paddle.gif')).

    /* DiggerNoid title */
    imgTitle:LOAD-IMAGE(getImagePath('AboutTitle.gif')).
    imgTitle:VISIBLE = FALSE.
    
    /* Disable ball mover */
    IF glUseTimer THEN
      chCtrlFrame:BallTimer:ENABLED = TRUE.  

    /* Set version name */
    fiDataDigger-1:SCREEN-VALUE = "DataDigger {&version} - {&edition}".
    fiDataDigger-2:SCREEN-VALUE = 'Build {&build}'.

    /* Load changelog */
    edChangeLog:INSERT-FILE(getProgramDir() + 'DataDigger.txt').
    edChangeLog:CURSOR-OFFSET = 1.

    RUN setTransparency(INPUT FRAME {&FRAME-NAME}:HANDLE, 1).
    
    /* For some reasons, these #*$&# scrollbars keep coming back */
    RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO). /* KILL KILL KILL */

    wAbout:VISIBLE = TRUE. 
  END.

END PROCEDURE. /* initializeObject. */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeUi wAbout 
PROCEDURE initializeUi :
/* Enable the user interface
 */
  DEFINE VARIABLE UIB_S    AS LOGICAL    NO-UNDO.
  DEFINE VARIABLE OCXFile  AS CHARACTER  NO-UNDO.

  /* Load wrx file if possible */
  OCXFile = SEARCH( "wAbout.wrx":U ).
  IF OCXFile = ? THEN
    OCXFile = SEARCH(SUBSTRING(THIS-PROCEDURE:FILE-NAME, 1,
                       R-INDEX(THIS-PROCEDURE:FILE-NAME, ".":U), "CHARACTER":U) + "wrx":U).

  IF OCXFile <> ? THEN
  DO:
    ASSIGN
      chCtrlFrame    = CtrlFrame:COM-HANDLE
      CtrlFrame:NAME = "CtrlFrame":U
      UIB_S          = chCtrlFrame:LoadControls( OCXFile, "CtrlFrame":U) NO-ERROR
    .

    /* Check for message 6087:
     * Specified ActiveX control is not registered or the .ocx file was moved from where it was registered.
     * Error occurred in procedure: <procedure name> (6087)
     * This error occurred while trying to load an ActiveX control.  
     * It is possible that the control was not properly installed or that the .ocx file was moved or deleted.
     */
    IF ERROR-STATUS:GET-NUMBER(1) = 6087 THEN 
      glUseTimer = NO.
    ELSE
      glUseTimer = YES.
  END.

  /* From enable_ui */
  DISPLAY edChangelog fiDataDigger-1 fiDataDigger-2 fiWebsite 
      WITH FRAME DEFAULT-FRAME IN WINDOW wAbout.

  ENABLE btnDataDigger BtnOK edChangelog fiWebsite 
      WITH FRAME DEFAULT-FRAME IN WINDOW wAbout.

  VIEW FRAME DEFAULT-FRAME IN WINDOW wAbout.
  {&OPEN-BROWSERS-IN-QUERY-DEFAULT-FRAME}

END PROCEDURE. /* initializeUi */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE justWait wAbout 
PROCEDURE justWait :
/* Wait a few miliseconds 
 */
  DEFINE INPUT  PARAMETER piWait AS INTEGER NO-UNDO.
  DEFINE VARIABLE iStart    AS INTEGER NO-UNDO.
  DEFINE VARIABLE lUseTimer AS LOGICAL NO-UNDO.
   
  iStart = ETIME.
  
  /* For the duration of the wait, switch off 
   * the timer to avoid deep loops */
  lUseTimer = chCtrlFrame:BallTimer:ENABLED.
  chCtrlFrame:BallTimer:ENABLED = FALSE.

  DO WHILE ETIME < iStart + piWait: 
    PROCESS EVENTS.
  END. 

  chCtrlFrame:BallTimer:ENABLED = lUseTimer.

END PROCEDURE. /* justWait */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE moveBall wAbout 
PROCEDURE moveBall :
/* Move the ball and detect collisions 
  */
  DEFINE VARIABLE iNewX    AS INTEGER NO-UNDO.
  DEFINE VARIABLE iNewY    AS INTEGER NO-UNDO.
  DEFINE VARIABLE iMinY    AS INTEGER NO-UNDO INITIAL 7.
  DEFINE VARIABLE iMaxY    AS INTEGER NO-UNDO.
  DEFINE VARIABLE iMinX    AS INTEGER NO-UNDO INITIAL 7.
  DEFINE VARIABLE iMaxX    AS INTEGER NO-UNDO.
  DEFINE VARIABLE iPaddleX AS INTEGER NO-UNDO.
  DEFINE VARIABLE iPaddleY AS INTEGER NO-UNDO.

  DEFINE BUFFER bfBrick FOR ttBrick.

  /* Turn off events when we're running */
  IF gcGameStatus <> 'running' THEN RETURN.

  DO WITH FRAME {&FRAME-NAME}:

    iNewX = imgBall:X + 7.
    iNewY = imgBall:Y + 7.
    iMaxY = FRAME {&FRAME-NAME}:HEIGHT-PIXELS - 7.
    iMaxX = FRAME {&FRAME-NAME}:WIDTH-PIXELS - 7.

    /* New X-pos for the ball */
    iNewX = iNewX + giBallX.

    /* Gonna hit the wall? */
    IF iNewX <= iMinX OR iNewX >= iMaxX THEN 
    DO:
      giBallX = giBallX * -1.
      RETURN.
    END.
    ELSE 
    DO:
      FIND FIRST bfBrick
        WHERE iNewX > bfBrick.x1 AND iNewX < bfBrick.x2
          AND iNewY > bfBrick.y1 AND iNewY < bfBrick.y2 NO-ERROR.

      IF AVAILABLE bfBrick THEN
      DO:
        giBallX = giBallX * -1.
        DELETE OBJECT bfBrick.hBrick.
        DELETE bfBrick.
        IF NOT CAN-FIND(FIRST bfBrick) THEN RUN gameOver(NO).
        RETURN.
      END.
    END.

    /* new Y-pos for the ball */
    iNewY = iNewY + giBallY.

    /* Hit top or bottom? */
    IF iNewY <= iMinY OR iNewY >= iMaxY THEN 
    DO:
      /* flash when bottom is hit */
      IF iNewY >= iMaxY THEN RUN hitBottom.
      giBallY = giBallY * -1.
      RETURN.
    END.
    ELSE 
    DO:
      FIND FIRST bfBrick
        WHERE iNewX > bfBrick.x1 AND iNewX < bfBrick.x2 
          AND iNewY > bfBrick.y1 AND iNewY < bfBrick.y2 NO-ERROR.
  
      IF AVAILABLE bfBrick THEN
      DO:
        giBallY = giBallY * -1.
        DELETE OBJECT bfBrick.hBrick.
        DELETE bfBrick.
        IF NOT CAN-FIND(FIRST bfBrick) THEN RUN gameOver(NO).
        RETURN.
      END.
    END.

    /* hit the paddle? */
    ASSIGN 
      iPaddleX = imgPaddle:X 
      iPaddleY = imgPaddle:Y.

    IF    iNewY >= iPaddleY - 10
      AND iNewY <= iPaddleY + imgBall:HEIGHT-PIXELS 
      AND iNewX >= iPaddleX 
      AND iNewX <= iPaddleX + 70 THEN
    DO:

      /* Right side ball hits left side of paddle */
      IF    iNewX >= iPaddleX 
        AND iNewX <= iPaddleX + 20 THEN 
        ASSIGN 
          giBallY = giBallY * -1
          giBallX = -3 - (RANDOM(1,3) * 2).

      ELSE
      /* Ball hits center of paddle */
      IF    iNewX >= iPaddleX + 20
        AND iNewX <= iPaddleX + 50 THEN 
        ASSIGN 
          giBallY = giBallY * -1.

      ELSE
      /* Left side of ball hits right side of paddle */
      IF    iNewX >= iPaddleX + 50
        AND iNewX <= iPaddleX + 70 THEN 
        ASSIGN 
          giBallY = giBallY * -1
          giBallX = 3 + (RANDOM(1,3) * 2).

    END.

    imgBall:X = iNewX - 7.
    imgBall:Y = iNewY - 7.
  END.

END PROCEDURE. /* moveBall */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE movePaddle wAbout 
PROCEDURE movePaddle :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER piMove AS INTEGER NO-UNDO.

  DO WITH FRAME {&FRAME-NAME}:
    IF    imgPaddle:X + piMove > 0 
      AND imgPaddle:X + piMove < (FRAME {&FRAME-NAME}:WIDTH-PIXELS - imgPaddle:WIDTH-PIXELS - 10) THEN
      imgPaddle:X = imgPaddle:X + piMove.
  END.

END PROCEDURE. /* movePaddle */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE prepareWindow wAbout 
PROCEDURE prepareWindow :
/* Grow window to desired size and position
  */
  DEFINE VARIABLE iStep     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartH   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartW   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartX   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartY   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartEdH AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartEdW AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iEndH     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iEndW     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iEndY     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iEndX     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iEndEdH   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iEndEdW   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNumSteps AS INTEGER     NO-UNDO INITIAL 50.

  /* debug */
  IF glDebugRun THEN iNumSteps = 1. 

  DO WITH FRAME {&FRAME-NAME}:
    BtnOK:VISIBLE     = NO.
    fiWebsite:VISIBLE = FALSE.

    ASSIGN 
      iStartH = wAbout:HEIGHT-PIXELS
      iStartW = wAbout:WIDTH-PIXELS
      iStartX = wAbout:X
      iStartY = wAbout:Y
      iEndH   = 800
      iEndW   = 1100
      iEndY   = (SESSION:HEIGHT-PIXELS - iEndH) / 4
      iEndX   = (SESSION:WIDTH-PIXELS - iEndW) / 2

      /* editor box */
      iStartEdH = edChangelog:HEIGHT-PIXELS
      iEndEdH   = 10
      iStartEdW = edChangelog:WIDTH-PIXELS - 40
      iEndEdW   = 80
      .

    DO iStep = 1 TO iNumSteps:
      /* Move vertically */
      wAbout:X             = iStartX + ((iEndX - iStartX)) / iNumSteps * iStep.
      wAbout:Y             = iStartY + ((iEndY - iStartY)) / iNumSteps * iStep.
      wAbout:HEIGHT-PIXELS = iStartH + ((iEndH - iStartH)) / iNumSteps * iStep.
      wAbout:WIDTH-PIXELS  = iStartW + ((iEndW - iStartW)) / iNumSteps * iStep.
      FRAME {&FRAME-NAME}:HEIGHT-PIXELS = wAbout:HEIGHT-PIXELS.
      FRAME {&FRAME-NAME}:WIDTH-PIXELS = wAbout:WIDTH-PIXELS.

      edChangelog:HEIGHT-PIXELS = iStartEdH + ((iEndEdH - iStartEdH)) / iNumSteps * iStep.
      edChangelog:Y             = wAbout:HEIGHT-PIXELS - edChangelog:HEIGHT-PIXELS - 40.

      edChangelog:WIDTH-PIXELS = iStartEdW + ((iEndEdW - iStartEdW)) / iNumSteps * iStep.
      edChangelog:X            = (wAbout:WIDTH-PIXELS - edChangelog:WIDTH-PIXELS) / 2.

      RUN justWait(5).
    END.

    edChangelog:VISIBLE = FALSE.
  END.

END PROCEDURE. /* prepareWindow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE readAboutFile wAbout 
PROCEDURE readAboutFile :
/* Build blocks with names of all contributors 
 **/
  DEFINE VARIABLE cLine  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cName  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE ii     AS INTEGER     NO-UNDO.
  DEFINE BUFFER bfBrick FOR ttBrick.

  INPUT FROM 'DataDigger.txt'.
  REPEAT:
    IMPORT UNFORMATTED cLine.
    IF cLine BEGINS '====' THEN LEAVE. 
  END.
  
  REPEAT:
    IMPORT UNFORMATTED cLine.
    IF cLine BEGINS 'DataDigger' THEN NEXT. /* lines with version name */
    IF NOT cLine MATCHES '*(*)' THEN NEXT. /* does not end with brackets */
    cName = TRIM( ENTRY(NUM-ENTRIES(cLine,'('),cLine,'(' ), ')').
    IF cName = '' THEN NEXT.  /* blank name */

    DO ii = 1 TO NUM-ENTRIES(cName):
      FIND bfBrick WHERE bfBrick.cBlockId = ENTRY(ii,cName) NO-ERROR.
      IF NOT AVAILABLE bfBrick THEN
      DO:
        CREATE bfBrick.
        ASSIGN bfBrick.cBlockId = ENTRY(ii,cName). 
      END.
    END.
  END.
  
  INPUT CLOSE. 

END PROCEDURE. /* readAboutFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE resumeGame wAbout 
PROCEDURE resumeGame :
/* Game is waiting for the user to click so it can resume */                                                
  IF gcGameStatus = 'waiting' THEN gcGameStatus = 'running'.
  imgTitle:VISIBLE IN FRAME DEFAULT-FRAME = FALSE.

  /* After the game has ended, close the screen */
  IF gcGameStatus = "Game Over" THEN APPLY "close" TO THIS-PROCEDURE. 
  
END PROCEDURE. /* resumeGame */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setBall wAbout 
PROCEDURE setBall :
/* Set the ball to its place with a nice bounce 
*/  
  DEFINE INPUT PARAMETER plBounceBall AS LOGICAL NO-UNDO.

  DEFINE VARIABLE xx   AS DECIMAL NO-UNDO.
  DEFINE VARIABLE yy   AS DECIMAL NO-UNDO.
  DEFINE VARIABLE dx   AS DECIMAL NO-UNDO INIT 1. /* hor speed */
  DEFINE VARIABLE dy   AS DECIMAL NO-UNDO INIT 0. /* ver speed */
  DEFINE VARIABLE elas AS DECIMAL NO-UNDO INIT .85. /* perc speed left after bounce */
  DEFINE VARIABLE grav AS DECIMAL NO-UNDO INIT .2. /* gravity acceleration */

  DO WITH FRAME {&FRAME-NAME}:
    imgPaddle:X = 280.
    imgPaddle:Y = 766.
    imgPaddle:VISIBLE = TRUE.

    imgBall:X = 1.
    imgBall:Y = 575.
    imgBall:VISIBLE = TRUE.

    yy = imgBall:Y.
    xx = imgBall:X.
  END.

  IF plBounceBall THEN
  REPEAT:
    gcGameStatus = 'intro'.

    /* Normal flow */
    dy = dy + grav.
    xx = xx + dx.
    yy = yy + dy.
    
    /* Bounce at bottom of frame */
    IF xx < 280 AND yy > (FRAME {&FRAME-NAME}:HEIGHT-PIXELS - imgBall:HEIGHT-PIXELS) THEN
    DO:
      yy = FRAME {&FRAME-NAME}:HEIGHT-PIXELS - imgBall:HEIGHT-PIXELS.
      dy = -1 * dy.
      dy = dy * elas.
      IF xx > 305 THEN LEAVE. 
    END.

    /* Bounce at the bat */
    IF xx > 280 AND yy > (imgPaddle:Y - imgBall:HEIGHT-PIXELS) THEN
    DO:
      yy = imgPaddle:Y - imgBall:HEIGHT-PIXELS.
      dy = -1 * dy.
      dy = dy * elas * elas * elas.
      dx = dx * elas.
      IF xx > 305 THEN LEAVE. 
    END.

    imgBall:X = xx.
    imgBall:Y = yy.

    /* debug */
    IF NOT glDebugRun THEN RUN justWait(9).
  END. /* if plBounceBall */

  imgBall:X = 305.
  imgBall:Y = imgPaddle:Y - imgBall:HEIGHT-PIXELS.
  RUN justWait(500).

  /* Move ball and paddle to center */
  REPEAT WHILE imgBall:X < (FRAME {&FRAME-NAME}:WIDTH-PIXELS / 2):
    imgBall:X = imgBall:X + 5.
    imgPaddle:X = imgPaddle:X + 5.
    IF NOT glDebugRun THEN RUN justWait(12).
  END.

  /* Show spare paddles */
  imgPaddle-2:X = 10.
  imgPaddle-2:Y = FRAME {&FRAME-NAME}:HEIGHT-PIXELS - 15.
  imgPaddle-2:VISIBLE = (giNumLives >= 1).

  imgPaddle-3:X = imgPaddle-2:X + 30.
  imgPaddle-3:Y = imgPaddle-2:Y.
  imgPaddle-3:VISIBLE = (giNumLives >= 2).

  imgPaddle-4:X = imgPaddle-3:X + 30.
  imgPaddle-4:Y = imgPaddle-3:Y.
  imgPaddle-4:VISIBLE = (giNumLives >= 3).

  /* Enable cursor movement of paddle */
  gcGameStatus = 'waiting'.
  ON 'cursor-right' OF FRAME {&FRAME-NAME} ANYWHERE PERSISTENT RUN movePaddle(+20).
  ON 'cursor-left' OF FRAME {&FRAME-NAME} ANYWHERE PERSISTENT RUN movePaddle(-20).
  ON 'MOUSE-SELECT-CLICK' OF FRAME {&FRAME-NAME} ANYWHERE PERSISTENT RUN resumeGame.
  
  /* Start timer */
  IF plBounceBall THEN giGameStarted = MTIME.
  
END PROCEDURE. /* setBall */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setBricks wAbout 
PROCEDURE setBricks :
/* Build blocks on the screen via button widgets
 */
 DEFINE BUFFER bfBrick FOR ttBrick.
 
 &GLOBAL-DEFINE Border      30
 &GLOBAL-DEFINE RowMargin   10
 &GLOBAL-DEFINE BlockMargin 20
 
 DEFINE VARIABLE xx AS INTEGER NO-UNDO.
 DEFINE VARIABLE yy AS INTEGER NO-UNDO.
 DEFINE VARIABLE ii AS INTEGER NO-UNDO.
 DEFINE VARIABLE iBgColor AS INTEGER NO-UNDO EXTENT 7 INITIAL [10,11,12,13,14,2,9].
 DEFINE VARIABLE iFgColor AS INTEGER NO-UNDO EXTENT 7 INITIAL [0,0,0,0,0,15,15].
 DEFINE VARIABLE iColor   AS INTEGER NO-UNDO.

 DEFINE VARIABLE iBlockLine  AS INTEGER NO-UNDO.
 DEFINE VARIABLE iNumLines   AS INTEGER NO-UNDO.
 DEFINE VARIABLE iNumBricks  AS INTEGER NO-UNDO.
 DEFINE VARIABLE iTotalWidth AS INTEGER NO-UNDO.
 DEFINE VARIABLE iFreeSpace  AS INTEGER NO-UNDO.
 DEFINE VARIABLE iRestWidth  AS INTEGER NO-UNDO.
 DEFINE VARIABLE iSpaces     AS INTEGER NO-UNDO.
 DEFINE VARIABLE iExtraWidth AS INTEGER NO-UNDO.

 xx = {&border}.
 yy = 90.
 iBlockLine = 1.

 FOR EACH bfBrick:

   /* Set brick to a safe position so we can resize it */
   bfBrick.hBrick:X = 1.
   bfBrick.hBrick:Y = 1.

   bfBrick.hBrick:WIDTH-PIXELS = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(bfBrick.cBlockId, getFont('fixed')) + 10.
   bfBrick.hBrick:SCREEN-VALUE = bfBrick.cBlockId.

   /* See where it fits */
   IF xx + bfBrick.hBrick:WIDTH-PIXELS > (FRAME {&FRAME-NAME}:WIDTH-PIXELS - {&border}) THEN
   DO:
     xx = {&border}.
     yy = yy + bfBrick.hBrick:HEIGHT-PIXELS + {&RowMargin}.
     iBlockLine = iBlockLine + 1.
     iNumLines = iBlockLine.
   END.

   iColor = (iBlockLine MOD 7) + 1.
   bfBrick.hBrick:BGCOLOR = iBgColor[iColor].
   bfBrick.hBrick:FGCOLOR = iFgColor[iColor].

   bfBrick.hBrick:X = xx.
   bfBrick.hBrick:Y = yy.
   bfBrick.iLine = iBlockLine.
   xx = xx + bfBrick.hBrick:WIDTH-PIXELS + {&BlockMargin}.
 END.


 /* Justify blocks */
 DO ii = 1 TO iNumLines:

   /* How much bricks per row */
   iTotalWidth = 0.
   iNumBricks = 0.
   FOR EACH bfBrick WHERE bfBrick.iLine = ii:
     iTotalWidth = iTotalWidth + bfBrick.hBrick:WIDTH-PIXELS.
     iNumBricks = iNumBricks + 1.
   END.
   
   /* Extra space */
   iFreeSpace = FRAME {&FRAME-NAME}:WIDTH-PIXELS    /* max available space */
              - (2 * {&border})                     /* left and right border */
              - iTotalWidth                         /* width of all bricks */
              - ((iNumBricks - 1) * {&BlockMargin}) /* margin between bricks */
              .
   iExtraWidth = TRUNCATE(iFreeSpace / iNumBricks,0). 
   iRestWidth  = FRAME {&FRAME-NAME}:WIDTH-PIXELS    /* max available space */
              - (2 * {&border})                     /* left and right border */
              - iTotalWidth                         /* width of all bricks */
              - ((iNumBricks - 1) * {&BlockMargin}) /* margin between bricks */
              - (iNumBricks * iExtraWidth)          /* extra width per brick */
              .
      
   /* Redraw buttons */
   xx = {&border}.
   FOR EACH bfBrick WHERE bfBrick.iLine = ii:

     bfBrick.hBrick:X = 1. /* to avoid errors while resizing */
     bfBrick.hBrick:WIDTH-PIXELS = bfBrick.hBrick:WIDTH-PIXELS + iExtraWidth + iRestWidth.
     iRestWidth = 0.
     
     bfBrick.hBrick:X = xx.
     xx = xx + bfBrick.hBrick:WIDTH-PIXELS + {&BlockMargin}.
     bfBrick.hBrick:SENSITIVE = NO.

     /* Justify text */
     iSpaces = ((bfBrick.hBrick:WIDTH-PIXELS - FONT-TABLE:GET-TEXT-WIDTH-PIXELS(bfBrick.cBlockId, getFont('fixed'))) / 2)
               / FONT-TABLE:GET-TEXT-WIDTH-PIXELS(' ', getFont('fixed')).

     bfBrick.hBrick:SCREEN-VALUE = FILL(' ', iSpaces) + bfBrick.cBlockId.

     /* Register exact position */
     ASSIGN 
       bfBrick.x1 = bfBrick.hBrick:X
       bfBrick.y1 = bfBrick.hBrick:Y 
       bfBrick.x2 = bfBrick.hBrick:X + bfBrick.hBrick:WIDTH-PIXELS
       bfBrick.y2 = bfBrick.hBrick:Y + bfBrick.hBrick:HEIGHT-PIXELS
       .
   END.
 END.

END PROCEDURE. /* setBricks */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setPaddle wAbout 
PROCEDURE setPaddle :
/* Adjust paddle position to mouse
  */
  DEFINE VARIABLE iMouseX    AS INTEGER NO-UNDO.
  DEFINE VARIABLE iMouseY    AS INTEGER NO-UNDO.

  /* Don't move paddle while bouncing */
  IF gcGameStatus = 'intro' THEN RETURN. 

  DO WITH FRAME {&FRAME-NAME}:
    
    RUN getMouseXY(INPUT FRAME {&FRAME-NAME}:HANDLE, OUTPUT iMouseX, OUTPUT iMouseY).

    IF giOldMouseX <> iMouseX
      AND iMouseX > (imgPaddle:WIDTH-PIXELS / 2) 
      AND iMouseX < (FRAME {&FRAME-NAME}:WIDTH-PIXELS - (imgPaddle:WIDTH-PIXELS / 2)) THEN 
    DO:
      imgPaddle:X = iMouseX - (imgPaddle:WIDTH-PIXELS / 2).
      giOldMouseX = iMouseX.
      
      /* During wait phase, let ball stick to paddle */
      IF gcGameStatus = 'waiting' THEN 
        ASSIGN imgBall:X = imgPaddle:X + 25.
    END.
  END.

END PROCEDURE. /* setPaddle */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showBricks wAbout 
PROCEDURE showBricks :
/* Make bricks visible */
  DEFINE BUFFER bfBrick FOR ttBrick.

  FOR EACH bfBrick:
    bfBrick.hBrick:VISIBLE = TRUE.
  END.

END PROCEDURE. /* showBricks */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showLog wAbout 
PROCEDURE showLog :
/* Play arkanoid-like game 
 */ 
 
  gcGameStatus = 'waiting'.

  RUN prepareWindow.
  RUN readAboutFile.
  RUN createBricks.
  RUN setBricks.
  RUN showBricks.
  RUN showTitle.
  RUN setBall(YES).

  OS-COMMAND NO-WAIT START 'https://datadigger.wordpress.com/easter-egg'.
  
END PROCEDURE. /* showLog */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showTitle wAbout 
PROCEDURE showTitle :
/* Show DiggerNoid title */
  DEFINE VARIABLE ii AS INTEGER NO-UNDO.
  &GLOBAL-DEFINE step 5
  
  DO WITH FRAME {&FRAME-NAME}:
    imgTitle:LOAD-IMAGE(getImagePath('AboutTitle.gif')).
    imgTitle:WIDTH-PIXELS = 600.
    imgTitle:HEIGHT-PIXELS = 1.
    imgTitle:X = 250.
    imgTitle:Y = 1.
    imgTitle:VISIBLE = FALSE.
    
    DO ii = 1 TO 300 BY {&step}:
      imgTitle:Y = ii.
      imgTitle:VISIBLE = TRUE.
      IF ii <= 250 THEN imgTitle:HEIGHT-PIXELS = ii.
      RUN justWait(2).
    END. 

    DO ii = 1 TO 100 BY {&step}:
      imgTitle:Y = imgTitle:Y + {&step}.
      imgTitle:VISIBLE = TRUE.
      imgTitle:HEIGHT-PIXELS = imgTitle:HEIGHT-PIXELS - {&step}.
      RUN justWait(2).
    END. 
  END. 
  
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

