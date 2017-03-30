&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME wImportSel
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS wImportSel 
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

{ DataDigger.i }

/* Parameters Definitions ---                                           */
DEFINE {&invar} plReadOnlyDigger  AS LOGICAL    NO-UNDO.
DEFINE {&invar} picDatabase       AS CHARACTER  NO-UNDO.
DEFINE {&invar} picTableName      AS CHARACTER  NO-UNDO.

&IF DEFINED(UIB_is_Running) = 0 &THEN
DEFINE {&invar} TABLE FOR ttField.
DEFINE {&invar} TABLE FOR ttColumn.
&ENDIF

DEFINE {&outvar} polSuccess        AS LOGICAL   NO-UNDO INITIAL ?.
DEFINE {&outvar} porRepositionId   AS ROWID     NO-UNDO.


/* Local Variable Definitions ---                                       */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME frMain

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS fcFilename btnGetFile btnAddFile edFileList ~
btnBack BtnNext fiText 
&Scoped-Define DISPLAYED-OBJECTS fcFilename edFileList fiText 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wImportSel AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnAddFile 
     LABEL "+" 
     SIZE-PIXELS 25 BY 21 TOOLTIP "add file".

DEFINE BUTTON btnBack DEFAULT 
     LABEL "&Back" 
     SIZE-PIXELS 75 BY 24 TOOLTIP "cancel load data"
     BGCOLOR 8 .

DEFINE BUTTON btnGetFile 
     LABEL "..." 
     SIZE-PIXELS 25 BY 21 TOOLTIP "add one or more files".

DEFINE BUTTON BtnNext 
     LABEL "&Next" 
     SIZE-PIXELS 75 BY 24 TOOLTIP "analyze files"
     BGCOLOR 8 .

DEFINE VARIABLE edFileList AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP SCROLLBAR-HORIZONTAL SCROLLBAR-VERTICAL
     SIZE 100 BY 7.14 TOOLTIP "the files to load" NO-UNDO.

DEFINE VARIABLE fcFilename AS CHARACTER FORMAT "X(256)":U 
     LABEL "File" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 415 BY 21 TOOLTIP "the name of the file to load" NO-UNDO.

DEFINE VARIABLE fiText AS CHARACTER FORMAT "X(256)":U INITIAL "Select the files you want to load or drag them onto this window" 
      VIEW-AS TEXT 
     SIZE-PIXELS 380 BY 19 NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frMain
     fcFilename AT Y 30 X 25 COLON-ALIGNED WIDGET-ID 12
     btnGetFile AT Y 30 X 450 WIDGET-ID 10
     btnAddFile AT Y 30 X 475 WIDGET-ID 14
     edFileList AT ROW 3.62 COL 1 NO-LABEL WIDGET-ID 2
     btnBack AT Y 213 X 340 WIDGET-ID 8
     BtnNext AT Y 213 X 423 WIDGET-ID 6
     fiText AT Y 6 X 9 NO-LABEL WIDGET-ID 4
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 0 Y 0
         SIZE-PIXELS 500 BY 239
         CANCEL-BUTTON btnBack DROP-TARGET WIDGET-ID 100.


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
  CREATE WINDOW wImportSel ASSIGN
         HIDDEN             = YES
         TITLE              = "Load Data - Select Files"
         HEIGHT-P           = 240
         WIDTH-P            = 500
         MAX-HEIGHT-P       = 1000
         MAX-WIDTH-P        = 1400
         VIRTUAL-HEIGHT-P   = 1000
         VIRTUAL-WIDTH-P    = 1400
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
/* SETTINGS FOR WINDOW wImportSel
  NOT-VISIBLE,,RUN-PERSISTENT                                           */
/* SETTINGS FOR FRAME frMain
   NOT-VISIBLE FRAME-NAME                                               */
/* SETTINGS FOR FILL-IN fiText IN FRAME frMain
   ALIGN-L                                                              */
IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wImportSel)
THEN wImportSel:HIDDEN = no.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME wImportSel
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImportSel wImportSel
ON END-ERROR OF wImportSel /* Load Data - Select Files */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImportSel wImportSel
ON WINDOW-CLOSE OF wImportSel /* Load Data - Select Files */
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImportSel wImportSel
ON WINDOW-RESIZED OF wImportSel /* Load Data - Select Files */
DO:
  RUN LockWindow (INPUT wImportSel:HANDLE, INPUT YES).

  DO WITH FRAME frMain:

    /* Make 'em small so we don't get errors on resizing the window */
    btnNext:X   = 1.
    btnNext:Y   = 1.
    btnBack:X = 1.
    btnBack:Y = 1.
    btnAddFile:X = 1.
    edFileList:WIDTH-PIXELS = 10.
    edFileList:HEIGHT-PIXELS = 10.

    /* Set frame width */
    FRAME frMain:WIDTH-PIXELS  = wImportSel:WIDTH-PIXELS NO-ERROR.
    FRAME frMain:HEIGHT-PIXELS = wImportSel:HEIGHT-PIXELS NO-ERROR.
  
    /* Adjust the editor */
    edFileList:WIDTH-PIXELS  = FRAME frMain:WIDTH-PIXELS - 3.
    edFileList:HEIGHT-PIXELS = FRAME frMain:HEIGHT-PIXELS - 90.
    btnAddFile:X   = edFileList:X + edFileList:WIDTH-PIXELS - btnAddFile:WIDTH-PIXELS.
    btnGetFile:X   = btnAddFile:X - btnGetFile:WIDTH-PIXELS.
    fcFileName:WIDTH-PIXELS = FRAME frMain:WIDTH-PIXELS - 90.
    
    btnNext:X      = edFileList:X + edFileList:WIDTH-PIXELS - btnNext:WIDTH-PIXELS.
    btnNext:Y      = FRAME frMain:HEIGHT-PIXELS - 27.
    btnBack:X    = btnNext:X - btnBack:WIDTH-PIXELS - 10.
    btnBack:Y    = btnNext:Y.

    /* Save settings */
    RUN saveWindowPos(wImportSel:HANDLE,"DataDigger:ImportSel").
  END.

  RUN showScrollBars(FRAME frMain:HANDLE, NO, NO).
  RUN LockWindow (INPUT wImportSel:HANDLE, INPUT NO).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME frMain
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frMain wImportSel
ON DROP-FILE-NOTIFY OF FRAME frMain
DO:
  DEFINE VARIABLE iFile  AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lAdded AS LOGICAL     NO-UNDO.

  DO iFile = 1 TO SELF:NUM-DROPPED-FILES:
    RUN addFile(SELF:GET-DROPPED-FILE(iFile),OUTPUT lAdded).
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frMain wImportSel
ON GO OF FRAME frMain
DO:
  APPLY "CHOOSE" TO btnNext. 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnAddFile
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAddFile wImportSel
ON CHOOSE OF btnAddFile IN FRAME frMain /* + */
OR "RETURN" OF fcFileName
DO:
  DEFINE VARIABLE lAdded AS LOGICAL     NO-UNDO.
  RUN addFile(fcFileName:SCREEN-VALUE,OUTPUT lAdded).
  IF lAdded THEN fcFileName:SCREEN-VALUE = "".
  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnBack
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnBack wImportSel
ON CHOOSE OF btnBack IN FRAME frMain /* Back */
OR ENDKEY OF {&WINDOW-NAME} anywhere 
DO:
  polSuccess = FALSE.
  APPLY "CLOSE" TO THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnGetFile
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnGetFile wImportSel
ON CHOOSE OF btnGetFile IN FRAME frMain /* ... */
DO:

  DEFINE VARIABLE lOKpressed AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lAdded     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE cDataFile  AS CHARACTER   NO-UNDO.
  
  SYSTEM-DIALOG GET-FILE cDataFile
    TITLE   "Choose File to load ..."    
    FILTERS "XML files (*.xml)" "*.xml",
            "All files (*.*)"   "*.*"    
    MUST-EXIST    
    USE-FILENAME    
    UPDATE lOKpressed.

  IF lOKpressed THEN
    RUN addFile(cDataFile,OUTPUT lAdded).
  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME BtnNext
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnNext wImportSel
ON CHOOSE OF BtnNext IN FRAME frMain /* Next */
DO:
  DEFINE VARIABLE cFileList      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE lOldVisibility AS LOGICAL   NO-UNDO.
  
  cFileList = edFileList:SCREEN-VALUE.

  IF cFileList <> "" THEN
  DO:
    lOldVisibility = wImportSel:VISIBLE.
    wImportSel:VISIBLE = FALSE. 

    RUN VALUE(getProgramDir() + 'wImportCheck.w')
      ( INPUT plReadOnlyDigger
      , INPUT cFileList
      , INPUT picDatabase
      , INPUT picTableName
      , INPUT TABLE ttField  /* do not use by-reference */
      , INPUT TABLE ttColumn /* do not use by-reference */
      , OUTPUT polSuccess     
      , OUTPUT porRepositionId
      ).

    wImportSel:VISIBLE = lOldVisibility.

    IF polSuccess THEN 
      APPLY 'close' TO THIS-PROCEDURE.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK wImportSel 


/* ***************************  Main Block  *************************** */

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME} 
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
DO:
  /* Save settings */
  RUN saveWindowPos(wImportSel:HANDLE,"DataDigger:ImportSel").
  RUN disable_UI.
END.

/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  /* Get fonts */
  FRAME {&FRAME-NAME}:FONT = getFont('Default').

  RUN enable_UI.
  RUN initializeObject.
  VIEW wImportSel.

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE addFile wImportSel 
PROCEDURE addFile :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER pcFileName AS CHARACTER NO-UNDO.
  DEFINE OUTPUT PARAMETER plAdded   AS LOGICAL     NO-UNDO.

  DO WITH FRAME frMain:
    /* Only accept valid file names */
    FILE-INFO:FILE-NAME = pcFileName.
    IF FILE-INFO:FULL-PATHNAME = ? THEN
    DO:
      MESSAGE SUBSTITUTE("Cannot find file '&1', please retry.",pcFilename)
        VIEW-AS ALERT-BOX INFO BUTTONS OK.
      RETURN. 
    END.

    /* Only accept regular files */
    IF NOT FILE-INFO:FILE-TYPE BEGINS "F" THEN 
    DO:
      MESSAGE "This is not a regular file, please retry." VIEW-AS ALERT-BOX INFO BUTTONS OK.
      RETURN. 
    END.
  
    IF LOOKUP(FILE-INFO:FULL-PATHNAME,edFileList:SCREEN-VALUE) = 0 THEN
    DO:
      edFileList:SCREEN-VALUE = TRIM(edFileList:SCREEN-VALUE,"~n").
      edFileList:SCREEN-VALUE = edFileList:SCREEN-VALUE + "~n" + FILE-INFO:FULL-PATHNAME.
      edFileList:SCREEN-VALUE = TRIM(edFileList:SCREEN-VALUE,"~n").
    END.
    plAdded = TRUE. 
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI wImportSel  _DEFAULT-DISABLE
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
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wImportSel)
  THEN DELETE WIDGET wImportSel.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI wImportSel  _DEFAULT-ENABLE
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
  DISPLAY fcFilename edFileList fiText 
      WITH FRAME frMain IN WINDOW wImportSel.
  ENABLE fcFilename btnGetFile btnAddFile edFileList btnBack BtnNext fiText 
      WITH FRAME frMain IN WINDOW wImportSel.
  {&OPEN-BROWSERS-IN-QUERY-frMain}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject wImportSel 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------
  Name         : initializeObject
  Description  : Setup
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE cExtentFormat   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSetting        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cValueList      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBuffer         AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iFieldExtent    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMaxFieldLength AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lNewRecord      AS LOGICAL     NO-UNDO.
  
  DO WITH FRAME {&FRAME-NAME}:

    /* Get fonts */
    FRAME {&FRAME-NAME}:FONT = getFont('Default').
    edFileList:FONT = getFont('Fixed').

    /* Window position and size */
    /* Set title of the window */
    wImportSel:TITLE = SUBSTITUTE('Load Data For &1.&2 - Select Files'
                            , picDatabase 
                            , picTableName
                            ).

    /* Set minimum size of the window */
    wImportSel:MIN-WIDTH-PIXELS  = 400.
    wImportSel:MIN-HEIGHT-PIXELS = 200.

    /* to avoid scrollbars on the frame */
    FRAME {&FRAME-NAME}:SCROLLABLE = FALSE.

    /* Set window back to last known pos */
    RUN restoreWindowPos(wImportSel:HANDLE, "DataDigger:ImportSel").
  END. 

END PROCEDURE. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

