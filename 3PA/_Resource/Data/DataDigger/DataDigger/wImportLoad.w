&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME wImportLoad
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS wImportLoad 
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
DEFINE INPUT PARAMETER plReadOnlyDigger   AS LOGICAL   NO-UNDO.
DEFINE INPUT PARAMETER TABLE-HANDLE pihXmlTable.
DEFINE INPUT PARAMETER picDatabase        AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER picTableName       AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER TABLE FOR ttField.
DEFINE INPUT PARAMETER TABLE FOR ttColumn.
DEFINE OUTPUT PARAMETER polSuccess        AS LOGICAL   NO-UNDO INITIAL ?.
DEFINE OUTPUT PARAMETER porRepositionId   AS ROWID     NO-UNDO.

/* Local Variable Definitions ---                                       */
DEFINE VARIABLE giCurrentRecord    AS INTEGER NO-UNDO.
DEFINE VARIABLE giNumRecords       AS INTEGER NO-UNDO.
DEFINE VARIABLE ghXmlBuffer        AS HANDLE  NO-UNDO.
DEFINE VARIABLE ghXmlQuery         AS HANDLE  NO-UNDO.
DEFINE VARIABLE ghDataBrowse       AS HANDLE  NO-UNDO.
DEFINE VARIABLE giDataOddRowColor  AS INTEGER NO-UNDO EXTENT 2.
DEFINE VARIABLE giDataEvenRowColor AS INTEGER NO-UNDO EXTENT 2.
DEFINE VARIABLE giDataErrorColor   AS INTEGER NO-UNDO EXTENT 2.
DEFINE VARIABLE giMaxFilterHistory AS INTEGER NO-UNDO.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME frMain

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS btnBack btnFinish tgWriteTrigger 
&Scoped-Define DISPLAYED-OBJECTS tgWriteTrigger 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wImportLoad AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnBack 
     LABEL "&Back" 
     SIZE-PIXELS 74 BY 24 TOOLTIP "cancel load data".

DEFINE BUTTON btnFinish 
     LABEL "&Finish" 
     SIZE-PIXELS 74 BY 24 TOOLTIP "load the data to the database".

DEFINE RECTANGLE rctData
     EDGE-PIXELS 1 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 699 BY 455.

DEFINE VARIABLE tgWriteTrigger AS LOGICAL INITIAL yes 
     LABEL "Use &write trigger" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 120 BY 17 TOOLTIP "Enable write triggers or not" NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frMain
     btnBack AT Y 460 X 535 WIDGET-ID 6
     btnFinish AT Y 460 X 615 WIDGET-ID 4
     tgWriteTrigger AT Y 463 X 15 WIDGET-ID 16
     rctData AT Y 0 X 1 WIDGET-ID 52
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT COL 1 ROW 1 SCROLLABLE 
         CANCEL-BUTTON btnBack WIDGET-ID 100.


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
  CREATE WINDOW wImportLoad ASSIGN
         HIDDEN             = YES
         TITLE              = "Import Finalize"
         HEIGHT-P           = 494
         WIDTH-P            = 700
         MAX-HEIGHT-P       = 2079
         MAX-WIDTH-P        = 1600
         VIRTUAL-HEIGHT-P   = 2079
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
/* SETTINGS FOR WINDOW wImportLoad
  NOT-VISIBLE,,RUN-PERSISTENT                                           */
/* SETTINGS FOR FRAME frMain
   NOT-VISIBLE FRAME-NAME Size-to-Fit                                   */
ASSIGN 
       FRAME frMain:SCROLLABLE       = FALSE
       FRAME frMain:RESIZABLE        = TRUE.

/* SETTINGS FOR RECTANGLE rctData IN FRAME frMain
   NO-ENABLE                                                            */
ASSIGN 
       rctData:HIDDEN IN FRAME frMain           = TRUE.

IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wImportLoad)
THEN wImportLoad:HIDDEN = yes.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME wImportLoad
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImportLoad wImportLoad
ON END-ERROR OF wImportLoad /* Import Finalize */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImportLoad wImportLoad
ON WINDOW-CLOSE OF wImportLoad /* Import Finalize */
OR "LEAVE" OF wImportLoad
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImportLoad wImportLoad
ON WINDOW-RESIZED OF wImportLoad /* Import Finalize */
DO:

  RUN LockWindow (INPUT wImportLoad:HANDLE, INPUT YES).

  DO WITH FRAME frMain:

    /* Make 'em small so we don't get errors on resizing the window */
    btnBack:X   = 1.
    btnBack:Y   = 1.
    btnFinish:X = 1.
    btnFinish:Y = 1.
    ghDataBrowse:WIDTH-PIXELS  = 50. 
    ghDataBrowse:HEIGHT-PIXELS = 100.
    tgWriteTrigger:Y = 1.
    tgWriteTrigger:X = 1.

    /* Set frame width */
    FRAME frMain:WIDTH-PIXELS  = wImportLoad:WIDTH-PIXELS NO-ERROR.
    FRAME frMain:HEIGHT-PIXELS = wImportLoad:HEIGHT-PIXELS NO-ERROR.

    /* Adjust the browse */
    ghDataBrowse:WIDTH-PIXELS  = FRAME frMain:WIDTH-PIXELS - 6.
    ghDataBrowse:HEIGHT-PIXELS = FRAME frMain:HEIGHT-PIXELS - 30 - 10.

    /* Set buttons + toggle for use of triggers */
    tgWriteTrigger:Y = FRAME frMain:HEIGHT-PIXELS - 30.
    tgWriteTrigger:X = 15.
    btnFinish:X      = FRAME frMain:WIDTH-PIXELS - 80.
    btnFinish:Y      = tgWriteTrigger:Y.
    btnBack:X        = btnFinish:X - btnBack:WIDTH-PIXELS - 10.
    btnBack:Y        = tgWriteTrigger:Y.
    
    /* Save settings */
    RUN saveWindowPos(wImportLoad:HANDLE,"DataDigger:ImportLoad").
  END.

  RUN showScrollBars(FRAME frMain:HANDLE, NO, NO).
  RUN LockWindow (INPUT wImportLoad:HANDLE, INPUT NO).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnBack
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnBack wImportLoad
ON CHOOSE OF btnBack IN FRAME frMain /* Back */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE 
DO:
  polSuccess = FALSE.
  APPLY "CLOSE" TO THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnFinish
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnFinish wImportLoad
ON CHOOSE OF btnFinish IN FRAME frMain /* Finish */
OR 'GO' OF FRAME frMain
DO:
  RUN btnFinishChoose (OUTPUT polSuccess).

  IF NOT polSuccess THEN 
    RETURN NO-APPLY.
  ELSE 
    APPLY 'close' TO THIS-PROCEDURE.  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tgWriteTrigger
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgWriteTrigger wImportLoad
ON VALUE-CHANGED OF tgWriteTrigger IN FRAME frMain /* Use write trigger */
DO:
  setRegistry("DataDigger","EnableWriteTriggers", STRING(SELF:CHECKED) ).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK wImportLoad 


/* ***************************  Main Block  *************************** */

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME} 
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
DO:
  /* Save settings */
  RUN saveWindowPos(wImportLoad:HANDLE,"DataDigger:ImportLoad").

  IF VALID-HANDLE(ghXmlQuery) THEN
  DO:
    ghXmlQuery:QUERY-CLOSE. 
    DELETE OBJECT ghXmlQuery.
  END.

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

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnFinishChoose wImportLoad 
PROCEDURE btnFinishChoose :
/*------------------------------------------------------------------------
  Name         : btnFinishChoose
  Description  : Proceed to procedure that actually loads the data
  ----------------------------------------------------------------------*/
  DEFINE OUTPUT PARAMETER plSuccess AS LOGICAL NO-UNDO.

  DEFINE VARIABLE lErrorsFound AS LOGICAL NO-UNDO.
  RUN loadData(INPUT TRUE, OUTPUT lErrorsFound).

  IF lErrorsFound THEN 
  DO:
    MESSAGE "Your data was loaded, but with errors" VIEW-AS ALERT-BOX INFO BUTTONS OK.
    ASSIGN plSuccess = NO.
  END. 

  ELSE
  DO:
    ASSIGN plSuccess = YES.
  END.

END PROCEDURE. /* btnFinishChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE createBrowse wImportLoad 
PROCEDURE createBrowse :
/*------------------------------------------------------------------------
  Name         : initializeObject
  Description  : Setup
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE iColumnWidth AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMinWidth    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cMyFormat    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iPos         AS INTEGER     NO-UNDO.
  DEFINE VARIABLE hField       AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cColumnName  AS CHARACTER   NO-UNDO.
  
  DEFINE BUFFER bColumn FOR ttColumn. 
  DEFINE BUFFER bField FOR ttField.
  
  /* Start building */
  CREATE BROWSE ghDataBrowse
    ASSIGN
    NAME              = "brData"
    FRAME             = FRAME frMain:HANDLE
    FONT              = getFont('Default')
    QUERY             = ghXmlQuery
    MULTIPLE          = TRUE
    X                 = 3
    Y                 = 3
    WIDTH-PIXELS      = 50 /* will be handled by resize routine */
    HEIGHT-PIXELS     = 100 /* will be handled by resize routine */
    ROW-MARKER        = TRUE
    SEPARATORS        = TRUE
    READ-ONLY         = FALSE
    SENSITIVE         = TRUE
    VISIBLE           = FALSE
    NO-VALIDATE       = TRUE
    COLUMN-RESIZABLE  = TRUE
    COLUMN-SCROLLING  = TRUE /* scroll with whole columns at a time */
    TRIGGERS:
/*       ON "CTRL-A"           PERSISTENT RUN dataSelectAll           IN THIS-PROCEDURE (ghDataBrowse). */
/*       ON "CTRL-D"           PERSISTENT RUN dataSelectNone          IN THIS-PROCEDURE (ghDataBrowse). */
      ON "ROW-DISPLAY"      PERSISTENT RUN dataRowDisplay          IN THIS-PROCEDURE (ghXmlBuffer).
/*       ON "START-SEARCH"     PERSISTENT RUN dataColumnSort          IN THIS-PROCEDURE.                */
/*       ON "VALUE-CHANGED"    PERSISTENT RUN dataRowValueChanged     IN THIS-PROCEDURE (ghXmlBuffer).  */
/*       ON "END"              PERSISTENT RUN dataRowJumpToEnd        IN THIS-PROCEDURE (ghXmlBuffer).  */
/*       ON "DEFAULT-ACTION"   PERSISTENT RUN dataDoubleClick         IN THIS-PROCEDURE (ghDataBrowse). */
/*       ON "OFF-HOME"         PERSISTENT RUN dataOffHome             IN THIS-PROCEDURE.                */
    END TRIGGERS.


  /* Add columns to the browse */
  addColumnLoop:
  FOR EACH bField BY bField.iOrder:

    FOR EACH bColumn
      WHERE bColumn.cTableCacheId = bField.cTableCacheId 
        AND bColumn.cFieldName    = bField.cFieldName
        AND bColumn.lShow         = TRUE 
         BY bColumn.iColumnNr:
  
      cMyFormat = getRegistry( SUBSTITUTE("DB:&1",bColumn.cDatabase)
                             , SUBSTITUTE("&1.&2:format",bColumn.cTableName, bColumn.cFieldName)
                             ).
      IF cMyFormat = ? THEN cMyFormat = bField.cFormat. 
  
      /* Autocorrect 2-digit years in date fields */
      IF bField.cDataType = "DATE"
        AND cMyFormat MATCHES "99.99.99" THEN cMyFormat = cMyFormat + "99".
  
      /* Protect against "value could not be displayed using..." errors. */
      IF (   bField.cDataType = "DECIMAL"
          OR bField.cDataType = "RECID" 
          OR bField.cDataType BEGINS "INT") /* Use BEGINS to cover integer / int64 and extents of both */ 
         AND NOT cMyFormat BEGINS "HH:MM"   /* Skip time fields */ THEN 
      DO:
        /* Add minus sign if needed */
        IF INDEX(cMyFormat,"-") = 0 AND INDEX(cMyFormat,"+") = 0 THEN 
          cMyFormat = "-" + cMyFormat.
  
        /* Add extra digit placeholders */
        addDigits:
        DO iPos = 1 TO LENGTH(cMyFormat):
          IF LOOKUP(SUBSTRING(cMyFormat,iPos,1),">,Z,9") > 0 THEN
          DO:
            IF iPos = 1 THEN
              cMyFormat = ">>>>>>>>>>>>>>>" + cMyFormat.
            ELSE 
              cMyFormat = SUBSTRING(cMyFormat,1,iPos - 1) + ">>>>>>>>>>>>>>>" + SUBSTRING(cMyFormat,iPos).
            LEAVE addDigits.
          END.
        END.
      END.  
  
      /* Apply the format */           
      IF NOT cMyFormat BEGINS "HH:MM" THEN
      DO:
        hField:FORMAT = cMyFormat NO-ERROR. 
        IF ERROR-STATUS:ERROR THEN 
        DO:
          bField.cFormat = bField.cFormatOrg.
          hField:FORMAT = bField.cFormat NO-ERROR.
        END.
      END.
  
      /* Add a calculated column for integers with time format */
      cColumnName = SUBSTITUTE("&1.&2", bColumn.cTableName, bColumn.cFullName).
  
      IF (   bField.cDataType = "DECIMAL"
          OR bField.cDataType BEGINS "INT") /* use BEGINS to cover integer / int64 and extents of both */
        AND bField.cFormat BEGINS "HH:MM" THEN 
      DO:
        bColumn.hColumn = ghDataBrowse:ADD-CALC-COLUMN("character","x(8)","", cColumnName ) NO-ERROR.
      END.
      ELSE
      DO:
        bColumn.hColumn = ghDataBrowse:ADD-LIKE-COLUMN(cColumnName).
      END.
  
      bColumn.hColumn:LABEL = bColumn.cFullName.
      bColumn.hColumn:RESIZABLE = TRUE.
    END.
  END.

  /* Show the browse */
  ghDataBrowse:VISIBLE = TRUE.

  adjustColumnWidth:
  FOR EACH bColumn WHERE VALID-HANDLE(bColumn.hColumn):

    /* Get last defined width from registry. Might have been set by user */
    iColumnWidth = INTEGER(getRegistry(SUBSTITUTE("DB:&1",bColumn.cDatabase), SUBSTITUTE("&1.&2:width", bColumn.cTableName, bColumn.cFullname)) ) NO-ERROR.

    .message bColumn.cFullname iColumnWidth view-as alert-box.

    /* If it's not set, calculate a width. Make sure it is not wider than 300px */
    IF iColumnWidth = ? THEN iColumnWidth = MINIMUM(300, bColumn.hColumn:WIDTH-PIXELS).

    /* Make sure the column is at least as wide as its name */
    iMinWidth = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(bColumn.cFullname,getFont("default")).
    .message bColumn.cFullname iMinWidth view-as alert-box.

    /* For the combo-filters, reserve some extra space for the arrow down */
    /* And if the filter is of type COMBO, reserve some extra space for the arrow down */
    IF giMaxFilterHistory > 0 THEN iMinWidth = iMinWidth + 21.
    IF iColumnWidth < iMinWidth THEN iColumnWidth = iMinWidth.

    bColumn.hColumn:WIDTH-PIXELS = iColumnWidth.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataColumnSort wImportLoad 
PROCEDURE dataColumnSort :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  /*
    run reopenDataBrowse(self:current-column:name,?).
  */

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataRowDisplay wImportLoad 
PROCEDURE dataRowDisplay :
/*------------------------------------------------------------------------
  Name         : dataRowDisplay
  Description  : Set the background color to another color to get 
                 an odd/even coloring of the rows.
                 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER phBrowseBuffer AS HANDLE NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.
  DEFINE BUFFER bField  FOR ttField.
  
  FOR EACH bColumn, bField WHERE bField.cFieldName = bColumn.cFieldName:
    IF NOT VALID-HANDLE(bColumn.hColumn) THEN NEXT.

    /* Alternate FG and BGcolor */
    IF phBrowseBuffer:QUERY:CURRENT-RESULT-ROW MODULO 2 = 1 THEN
      ASSIGN 
        bColumn.hColumn:FGCOLOR = giDataOddRowColor[1]
        bColumn.hColumn:BGCOLOR = giDataOddRowColor[2].
    ELSE                 
      ASSIGN 
        bColumn.hColumn:FGCOLOR = giDataEvenRowColor[1]
        bColumn.hColumn:BGCOLOR = giDataEvenRowColor[2].

    /* Color may be overruled in case of error */
    /*                                            */
/*     IF RANDOM(1,10) > 8 THEN                                                 */
/*       ASSIGN                                                                 */
/*         bColumn.hColumn:FGCOLOR = giDataErrorColor[1]  /* red or similar */  */
/*         bColumn.hColumn:BGCOLOR = giDataErrorColor[2].                       */
    /* */

    IF bField.cFormat BEGINS "HH:MM" THEN
    DO:
      /* Try to format in time format */
      bColumn.hColumn:SCREEN-VALUE = STRING(INTEGER(phBrowseBuffer:BUFFER-FIELD(bColumn.cFieldName):BUFFER-VALUE(bColumn.iExtent)), bField.cFormat) NO-ERROR.

      /* If you type a crappy time format like HH:MAM:SS just ignore it */
      IF ERROR-STATUS:ERROR THEN
        bColumn.hColumn:SCREEN-VALUE = STRING(phBrowseBuffer:BUFFER-FIELD(bColumn.cFieldName):BUFFER-VALUE(bColumn.iExtent)).
    END.
  END.

END PROCEDURE. /* dataRowDisplay */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI wImportLoad  _DEFAULT-DISABLE
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
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wImportLoad)
  THEN DELETE WIDGET wImportLoad.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI wImportLoad  _DEFAULT-ENABLE
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
  DISPLAY tgWriteTrigger 
      WITH FRAME frMain IN WINDOW wImportLoad.
  ENABLE btnBack btnFinish tgWriteTrigger 
      WITH FRAME frMain IN WINDOW wImportLoad.
  {&OPEN-BROWSERS-IN-QUERY-frMain}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE hideNonExistingFields wImportLoad 
PROCEDURE hideNonExistingFields :
/*------------------------------------------------------------------------
  Name         : hideNonExistingFields
  Description  : Hide non-existing fields.
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER phBuffer AS HANDLE NO-UNDO.

  DEFINE BUFFER bfField  FOR ttField.
  DEFINE BUFFER bfColumn FOR ttColumn.

  /* Go thru the tt one by one and check whether the field
   * is in the XML buffer. If not, hide the field from the user
   */
  FOR EACH bfField, EACH bfColumn WHERE bfColumn.cFieldName = bfField.cFieldName:

    IF LOOKUP(bfField.cDataType, 'clob,blob,raw') > 0 
      OR LOOKUP(bfField.cFieldName, 'RECID,ROWID') > 0 THEN
    DO:
      bfColumn.lShow = FALSE.
      bfColumn.hFilter = ?.
      NEXT. 
    END.

    /* Save handle to the buffer in the tt */
    bfColumn.hFilter = phBuffer:BUFFER-FIELD(bfColumn.cFieldName):HANDLE NO-ERROR.
    bfColumn.lShow   = VALID-HANDLE(bfColumn.hFilter).

  END.

/*   OUTPUT TO c:\temp\dd.txt.                */
/*   FOR EACH bfColumn:                       */
/*     EXPORT                                 */
/*       cFieldName                           */
/*       lShow                                */
/*       INTEGER(hFilter).                    */
/*   END.                                     */
/*   OUTPUT CLOSE.                            */
/*   OS-COMMAND NO-WAIT START c:\temp\dd.txt. */

END PROCEDURE. /* hideNonExistingFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject wImportLoad 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------
  Name         : initializeObject
  Description  : Setup
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE cExtentFormat   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSetting        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cValueList      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iFieldExtent    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMaxFieldLength AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iValue          AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lNewRecord      AS LOGICAL     NO-UNDO.

  VIEW wImportLoad.

  /* Get fonts */
  FRAME {&FRAME-NAME}:FONT = getFont('Default').

  /* Colors for odd/even data rows */
  giDataOddRowColor[1]  = getColor("DataRow:odd:fg" ).
  giDataOddRowColor[2]  = getColor("DataRow:odd:bg" ).
  giDataEvenRowColor[1] = getColor("DataRow:even:fg").
  giDataEvenRowColor[2] = getColor("DataRow:even:bg").
  giDataErrorColor[1]   = getColor("QueryError:even:fg"). /* default: yellow */
  giDataErrorColor[2]   = getColor("QueryError:even:fg"). /* default: red */

  IF giDataErrorColor[1] = ? THEN giDataErrorColor[1] = 14.  
  IF giDataErrorColor[2] = ? THEN giDataErrorColor[2] = 12.  

  IF getRegistry("DataDigger:colors","DataRow:UseSystem") = "YES" THEN
  ASSIGN
    giDataOddRowColor[1]  = 1
    giDataOddRowColor[2]  = 24
    giDataEvenRowColor[1] = 1
    giDataEvenRowColor[2] = 15.
  
  /* Maximum number OF history ON data filters */
  giMaxFilterHistory = INTEGER(getRegistry("DataDigger", "MaxFilterHistory")).
  IF giMaxFilterHistory = ? THEN giMaxFilterHistory = 10.
  
  /* Find out max fieldname length */
  FOR EACH ttColumn: 
    iMaxFieldLength = MAXIMUM(iMaxFieldLength,LENGTH(ttColumn.cFullName)).
  END.

  DO WITH FRAME {&FRAME-NAME}:

    /* Use triggers or not? */
    cSetting = getRegistry("DataDigger","EnableWriteTriggers").
    IF cSetting = ? THEN cSetting = "yes".
    tgWriteTrigger:CHECKED = LOGICAL(cSetting).

    /* Window position and size */
    /* Set title of the window */
    wImportLoad:TITLE = SUBSTITUTE('Import &1.&2', picDatabase, picTableName).

    /* Set minimum size of the window */
    wImportLoad:MIN-WIDTH-PIXELS  = 400.
    wImportLoad:MIN-HEIGHT-PIXELS = 200.

    /* to avoid scrollbars on the frame */
    FRAME {&FRAME-NAME}:SCROLLABLE = FALSE.

    /* Open query on provided xml data */
    CREATE QUERY ghXmlQuery.
    ghXmlBuffer = pihXmlTable:DEFAULT-BUFFER-HANDLE.

    /* Set all fields to 'hidden' in the tt that are not in the buffer */
    RUN hideNonExistingFields(ghXmlBuffer).

    ghXmlQuery:ADD-BUFFER(ghXmlBuffer).
    ghXmlQuery:QUERY-PREPARE(SUBSTITUTE("PRESELECT EACH &1",ghXmlBuffer:NAME)).
    ghXmlQuery:QUERY-OPEN().
    ghXmlQuery:GET-FIRST().
    giNumRecords = ghXmlQuery:NUM-RESULTS.

  END. 

  /* Create the browse */
  RUN createBrowse.
   
  /* Set window back to last known pos */
  RUN restoreWindowPos(wImportLoad:HANDLE, "DataDigger:ImportLoad").

  /* Restore sort */
/*   cSetting = getRegistry('DataDigger','ColumnSortRecord').                                */
/*   IF cSetting <> ? THEN                                                                   */
/*     ghDataBrowse:SET-SORT-ARROW(INTEGER(ENTRY(1,cSetting)), LOGICAL(ENTRY(2,cSetting)) ). */

  /* In read-only mode, disable FINISH button */
  IF plReadOnlyDigger THEN btnFinish:SENSITIVE = FALSE.

END PROCEDURE. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE loadData wImportLoad 
PROCEDURE loadData :
/*------------------------------------------------------------------------
  Name         : loadData
  Description  : Write changes to database, optionally roll back
  ----------------------------------------------------------------------*/

  DEFINE INPUT  PARAMETER plKeepData    AS LOGICAL     NO-UNDO.
  DEFINE OUTPUT PARAMETER plErrorsFound AS LOGICAL     NO-UNDO.

  DEFINE VARIABLE hXmlQuery       AS HANDLE    NO-UNDO.
  DEFINE VARIABLE hXmlBuffer      AS HANDLE    NO-UNDO.
  DEFINE VARIABLE hDbBuffer       AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iRow            AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iField          AS INTEGER   NO-UNDO.
  DEFINE VARIABLE lDisableTrigger AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE cKeyFields      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cQuery          AS CHARACTER NO-UNDO.
  DEFINE VARIABLE lOk          AS LOGICAL     NO-UNDO.

  /* Disable Triggers? */
  lDisableTrigger = tgWriteTrigger:SCREEN-VALUE IN FRAME {&FRAME-NAME} = "no".

  /* Prohibit editing of VST records */
  IF picTableName BEGINS "_" THEN
  DO:
    RUN showHelp("CannotEditVst", "").
    APPLY "close" TO THIS-PROCEDURE. 
    RETURN.
  END.

  /* Show that we're busy */
  PROCESS EVENTS.
  SESSION:SET-WAIT-STATE("general").

  TransBlock:
  DO TRANSACTION:

    /* Get list of all Primary Unique index fields 
     * If the primary index is not unique, just give a list of unique fields 
     * If there are no unique fields, we need a list of all fields 
     */
    cKeyFields = getIndexFields(picDatabase, picTableName, "P,U").
    IF cKeyFields = "" THEN cKeyFields = getIndexFields(picDatabase, picTableName, "U").
    IF cKeyFields = "" THEN cKeyFields = getIndexFields(picDatabase, picTableName, "").

    /* Create buffer on db table */
    CREATE BUFFER hDbBuffer FOR TABLE SUBSTITUTE('&1.&2', picDatabase, picTableName) NO-ERROR.

    /* Disable triggers if needed */
    IF lDisableTrigger THEN
    DO:
      hDbBuffer:DISABLE-LOAD-TRIGGERS(FALSE).
      hDbBuffer:DISABLE-DUMP-TRIGGERS( ).
    END.

    /* Open query on provided xml data */
    CREATE QUERY hXmlQuery.
    hXmlBuffer = pihXmlTable:DEFAULT-BUFFER-HANDLE.
    hXmlQuery:ADD-BUFFER(hXmlBuffer).
    hXmlQuery:QUERY-PREPARE(SUBSTITUTE("PRESELECT EACH &1",hXmlBuffer:NAME)).
    hXmlQuery:QUERY-OPEN().

    /* Walk through all records of the query */
    XmlLoop:
    REPEAT WHILE hXmlQuery:GET-NEXT():

      /* See if there is a record in the database with the same unique key */
      cQuery = "".

      DO iField = 1 TO NUM-ENTRIES(cKeyFields):
        cQuery = SUBSTITUTE("&1 &2 &3.&4.&5 = &6"
                           , cQuery
                           , (IF iField = 1 THEN "WHERE" ELSE "AND")
                           , picDatabase
                           , picTableName
                           , ENTRY(iField,cKeyFields)
                           , QUOTER(hXmlBuffer:BUFFER-FIELD(ENTRY(iField,cKeyFields)):BUFFER-VALUE)
                           ).
      END.

      /* Find in database */
      hDbBuffer:FIND-UNIQUE(cQuery, EXCLUSIVE-LOCK) NO-ERROR.
      
      .message "In db:" hDbBuffer:AVAILABLE view-as alert-box.
      
      /* If nothing found, then the record from the tt is a new one */
      IF NOT hDbBuffer:AVAILABLE THEN
      DO:
        hDbBuffer:BUFFER-CREATE NO-ERROR.

        /* If this fails, log it */
        IF ERROR-STATUS:ERROR THEN
        DO:
          /* Create failed */
          NEXT XmlLoop.
        END.

        /* Save backup file when not a testrun */
        IF plKeepData THEN
        DO:
          RUN dumpRecord(INPUT "Create", INPUT hDbBuffer, OUTPUT lOk).
          IF NOT lOk THEN UNDO XmlLoop, NEXT XmlLoop.
        END. 
      END.

      .message hDbBuffer:BUFFER-COMPARE(hXmlBuffer) view-as alert-box.

      /* Save backup file when this is not a testrun */
      IF plKeepData AND NOT hDbBuffer:NEW THEN
      DO:
        RUN dumpRecord(INPUT "Update", INPUT hDbBuffer, OUTPUT lOk).
        IF NOT lOk THEN UNDO XmlLoop, NEXT XmlLoop.
      END.

      /* Try to copy the data from the TT to the DB */
      hDbBuffer:BUFFER-COPY(hXmlBuffer) NO-ERROR.
      IF ERROR-STATUS:ERROR THEN
      DO:
        MESSAGE "Copy to database failed :(" VIEW-AS ALERT-BOX INFO.
        /* Copy failed */
        NEXT XmlLoop.
      END.

      /* Let it go ... */
      hXmlBuffer:BUFFER-RELEASE.
    END. /* XmlLoop */

    hXmlQuery:QUERY-CLOSE. 
    DELETE OBJECT hXmlQuery.
    DELETE OBJECT hDbBuffer.

    IF NOT plKeepData THEN UNDO TransBlock.
    lOk = TRUE.
  END. /* transaction */

  /* Unfreeze the window */
  SESSION:SET-WAIT-STATE("").

END PROCEDURE. /* loadData */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

