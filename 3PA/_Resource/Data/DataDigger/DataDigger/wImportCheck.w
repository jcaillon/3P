&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME wImport
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS wImport 
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
DEFINE {&invar} picFileList       AS CHARACTER  NO-UNDO.
DEFINE {&invar} picDatabase       AS CHARACTER  NO-UNDO.
DEFINE {&invar} picTableName      AS CHARACTER  NO-UNDO.

&IF DEFINED(UIB_is_Running) = 0 &THEN
DEFINE {&invar} TABLE FOR ttField.
DEFINE {&invar} TABLE FOR ttColumn.
&ENDIF

DEFINE {&outvar} polSuccess        AS LOGICAL   NO-UNDO INITIAL ?.
DEFINE {&outvar} porRepositionId   AS ROWID     NO-UNDO.

/* Local Variable Definitions ---                                       */
DEFINE VARIABLE gcUniqueFields AS CHARACTER NO-UNDO. 
DEFINE VARIABLE glInEditMode   AS LOGICAL   NO-UNDO. 
DEFINE VARIABLE ghXmlTable     AS HANDLE    NO-UNDO.

/* Table to hold all xml file names */
DEFINE TEMP-TABLE ttXmlFile NO-UNDO RCODE-INFORMATION
  FIELD iFileNr     AS INTEGER 
  FIELD cFileName   AS CHARACTER
  FIELD lValidFile  AS LOGICAL 
  FIELD cBufferName AS CHARACTER
  FIELD iNumRecords AS INTEGER 
  FIELD iNumFields  AS INTEGER
  FIELD cFields     AS CHARACTER FORMAT "X(140)"
  .

DEFINE TEMP-TABLE ttMessage NO-UNDO RCODE-INFORMATION
  FIELD iFileNr     AS INTEGER 
  FIELD iType       AS INTEGER /* 1=info 2=warning 3=error */
  FIELD cMessage    AS CHARACTER
  .

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME frMain

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS edSummary btnBack btnNext 
&Scoped-Define DISPLAYED-OBJECTS edSummary 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD addError wImport 
FUNCTION addError RETURNS LOGICAL
  ( piFileNr  AS INTEGER 
  , pcMessage AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD addInfo wImport 
FUNCTION addInfo RETURNS LOGICAL
  ( piFileNr  AS INTEGER 
  , pcMessage AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD addMessage wImport 
FUNCTION addMessage RETURNS LOGICAL
  ( piFileNr  AS INTEGER 
  , piType    AS INTEGER
  , pcMessage AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD addWarning wImport 
FUNCTION addWarning RETURNS LOGICAL
  ( piFileNr  AS INTEGER 
  , pcMessage AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD hasWarnings wImport 
FUNCTION hasWarnings RETURNS LOGICAL
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wImport AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnBack 
     LABEL "&Back" 
     SIZE-PIXELS 74 BY 24 TOOLTIP "cancel load data".

DEFINE BUTTON btnNext AUTO-GO 
     LABEL "&Next" 
     SIZE-PIXELS 74 BY 24.

DEFINE VARIABLE edSummary AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP SCROLLBAR-HORIZONTAL SCROLLBAR-VERTICAL
     SIZE-PIXELS 500 BY 365
     FONT 0 NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frMain
     edSummary AT Y 0 X 0 NO-LABEL WIDGET-ID 22
     btnBack AT Y 370 X 340 WIDGET-ID 6
     btnNext AT Y 370 X 420 WIDGET-ID 4
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
  CREATE WINDOW wImport ASSIGN
         HIDDEN             = YES
         TITLE              = "Load Data - Summary"
         HEIGHT-P           = 400
         WIDTH-P            = 500
         MAX-HEIGHT-P       = 1134
         MAX-WIDTH-P        = 1920
         VIRTUAL-HEIGHT-P   = 1134
         VIRTUAL-WIDTH-P    = 1920
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
/* SETTINGS FOR WINDOW wImport
  NOT-VISIBLE,,RUN-PERSISTENT                                           */
/* SETTINGS FOR FRAME frMain
   NOT-VISIBLE FRAME-NAME Size-to-Fit                                   */
ASSIGN 
       FRAME frMain:SCROLLABLE       = FALSE
       FRAME frMain:RESIZABLE        = TRUE.

IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wImport)
THEN wImport:HIDDEN = yes.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME wImport
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImport wImport
ON END-ERROR OF wImport /* Load Data - Summary */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE 
DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImport wImport
ON WINDOW-CLOSE OF wImport /* Load Data - Summary */
OR "LEAVE" OF wImport
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wImport wImport
ON WINDOW-RESIZED OF wImport /* Load Data - Summary */
DO:
  IF FOCUS:NAME = 'cNewValue' THEN APPLY 'leave' TO FOCUS.

  RUN LockWindow (INPUT wImport:HANDLE, INPUT YES).

  DO WITH FRAME frMain:

    /* Make 'em small so we don't get errors on resizing the window */
    btnBack:X    = 1.
    btnBack:Y    = 1.
    btnNext:X = 1.
    btnNext:Y = 1.
    edSummary:WIDTH-PIXELS = 10.
    edSummary:HEIGHT-PIXELS = 10.

    /* Set frame width */
    FRAME frMain:WIDTH-PIXELS  = wImport:WIDTH-PIXELS NO-ERROR.
    FRAME frMain:HEIGHT-PIXELS = wImport:HEIGHT-PIXELS NO-ERROR.
  
    edSummary:WIDTH-PIXELS = FRAME frMain:WIDTH-PIXELS.
    edSummary:HEIGHT-PIXELS = FRAME frMain:HEIGHT-PIXELS - 32.
    btnNext:X     = FRAME frMain:WIDTH-PIXELS - btnNext:WIDTH-PIXELS.
    btnNext:Y     = FRAME frMain:HEIGHT-PIXELS - 27.
    btnBack:X        = btnNext:X - btnBack:WIDTH-PIXELS - 10.
    btnBack:Y        = btnNext:Y.
  
    /* Save settings */
    RUN saveWindowPos(wImport:HANDLE,"DataDigger:ImportCheck").
  END.

  RUN showScrollBars(FRAME frMain:HANDLE, NO, NO).
  RUN LockWindow (INPUT wImport:HANDLE, INPUT NO).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnBack
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnBack wImport
ON CHOOSE OF btnBack IN FRAME frMain /* Back */
OR "ESC" OF edSummary
DO:
  polSuccess = FALSE. 
  APPLY "CLOSE" TO THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnNext
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnNext wImport
ON CHOOSE OF btnNext IN FRAME frMain /* Next */
OR "GO" OF edSummary
DO:
  RUN btnNextChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK wImport 


/* ***************************  Main Block  *************************** */

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME} 
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
DO:
  /* Save settings */
  RUN saveWindowPos(wImport:HANDLE,"DataDigger:ImportCheck").
  RUN disable_UI.
END.

/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.


/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  /* First load the xml files and check them */
  RUN loadFiles(picFileList).
  RUN checkFiles.

  /* Only if there are warnings and/or errors, we 
   * want to see the intermediate screen. In other
   * cases, just proceed to showing the records 
   */
  IF hasWarnings() THEN
  DO:
    RUN initializeObject.
    RUN enable_UI.
    RUN showSummary.
    APPLY "ENTRY" TO edSummary.
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
  END.

  ELSE
  DO:
    RUN btnNextChoose.
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE addFile wImport 
PROCEDURE addFile :
/*------------------------------------------------------------------------
  Name         : addFile
  Description  : Add one file to the tt, do some basic checking
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER pcFileName AS CHARACTER NO-UNDO.

  DEFINE VARIABLE hTable   AS HANDLE  NO-UNDO.
  DEFINE VARIABLE hBuffer  AS HANDLE  NO-UNDO.
  DEFINE VARIABLE hQuery   AS HANDLE  NO-UNDO.
  DEFINE VARIABLE iField   AS INTEGER NO-UNDO.
  DEFINE VARIABLE iFileNum AS INTEGER NO-UNDO.

  DEFINE BUFFER bfXmlFile FOR ttXmlFile.

  /* Sanity check */
  IF pcFilename = "" OR pcFilename = ? THEN RETURN. 

  /* File nr */
  FIND LAST bfXmlFile NO-ERROR.
  iFileNum = (IF AVAILABLE bfXmlFile THEN bfXmlFile.iFileNr + 1 ELSE 1).

  /* Check if we haven't already loaded this one */
  IF CAN-FIND(FIRST bfXmlFile WHERE bfXmlFile.cFileName = pcFileName) THEN RETURN. 

  CREATE bfXmlFile.
  ASSIGN 
    bfXmlFile.iFilenr    = iFileNum
    bfXmlFile.cFileName  = pcFileName
    .

  /* See if file exists */
  FILE-INFO:FILE-NAME = pcFileName.
  IF FILE-INFO:FULL-PATHNAME = ? THEN
  DO:
    bfXmlFile.lValidFile = FALSE.
    addError(bfXmlFile.iFileNr,SUBSTITUTE("File not found: &1", pcFileName)).
    RETURN. 
  END. 

  /* Create TT and read XML */
  CREATE TEMP-TABLE hTable.
  hTable:READ-XML("FILE",pcFileName,"EMPTY",?,?) NO-ERROR.

  IF ERROR-STATUS:ERROR OR ERROR-STATUS:NUM-MESSAGES > 0 THEN 
  DO:
    bfXmlFile.lValidFile = FALSE.
    addError(bfXmlFile.iFileNr,SUBSTITUTE("Cannot read file &1", pcFileName)).
  END. 
  ELSE 
  DO:
    hBuffer = hTable:DEFAULT-BUFFER-HANDLE.
    
    /* Create query to find nr of records */
    CREATE QUERY hQuery.
    hQuery:ADD-BUFFER(hBuffer).
    hQuery:QUERY-PREPARE(SUBSTITUTE("PRESELECT EACH &1",hBuffer:NAME)).
    hQuery:QUERY-OPEN().
    hQuery:GET-FIRST().
    
    ASSIGN  
      bfXmlFile.lValidFile  = TRUE
      bfXmlFile.cBufferName = hBuffer:NAME
      bfXmlFile.iNumFields  = hBuffer:NUM-FIELDS
      bfXmlFile.iNumRecords = hQuery:NUM-RESULTS
      .
  
    /* Find all fields in this file */
    DO iField = 1 TO hBuffer:NUM-FIELDS:
      bfXmlFile.cFields = TRIM(bfXmlFile.cFields + "," + hBuffer:BUFFER-FIELD(iField):NAME,",").
    END.
    
    hQuery:QUERY-CLOSE. 
    DELETE OBJECT hQuery.
  END.

  DELETE OBJECT hTable.

END PROCEDURE. /* addFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnNextChoose wImport 
PROCEDURE btnNextChoose :
/*------------------------------------------------------------------------
  Name         : btnNextChoose
  Description  : Proceed to procedure that shows a preview of the load
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE lContinue      AS LOGICAL NO-UNDO.
  DEFINE VARIABLE lOldVisibility AS LOGICAL     NO-UNDO.

  /* Check for warnings */
  IF CAN-FIND(FIRST ttMessage WHERE ttMessage.iType = 2) THEN 
  DO:
    MESSAGE "Some warnings were found. Are you sure you want to continue?"
      VIEW-AS ALERT-BOX INFO BUTTONS YES-NO UPDATE lContinue.
    IF lContinue <> TRUE THEN RETURN NO-APPLY.
  END.

  lOldVisibility = wImport:VISIBLE.
  wImport:VISIBLE = FALSE. 

  RUN VALUE(getProgramDir() + 'wImportLoad.w')
    ( INPUT plReadOnlyDigger
    , INPUT TABLE-HANDLE ghXmlTable
    , INPUT picDatabase
    , INPUT picTableName
    , INPUT TABLE ttField  /* do not use by-reference */
    , INPUT TABLE ttColumn /* do not use by-reference */
    , OUTPUT polSuccess     
    , OUTPUT porRepositionId
    ).

  wImport:VISIBLE = lOldVisibility.

  IF polSuccess THEN 
    APPLY 'close' TO THIS-PROCEDURE.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE checkFiles wImport 
PROCEDURE checkFiles :
/*------------------------------------------------------------------------
    Name         : checkFiles
    Description  : Check contents of the files
    ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE hBuffer          AS HANDLE    NO-UNDO.
  DEFINE VARIABLE hQuery           AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iField           AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iNumFields       AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iExpectedRecords AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cFieldList       AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cIgnoreFields    AS CHARACTER NO-UNDO INITIAL "RECID,ROWID".
  DEFINE VARIABLE hTableBuffer     AS HANDLE    NO-UNDO.

  DEFINE BUFFER bfXmlFile FOR ttXmlFile.
  DEFINE BUFFER bfField   FOR ttField.

  /* Check required fields:
   * - fields that are part of the primary index
   * - mandatory fields
   * - fields that are part of a unique index
   */
  FOR EACH bfXmlFile WHERE bfXmlFile.lValidFile:

    /* Basic info */
    addInfo(bfXmlFile.iFileNr,SUBSTITUTE("Nr of records in this file: &1", bfXmlFile.iNumRecords)).

    /* Check if buffer name matches table name */
    IF bfXmlFile.cBuffername <> picTableName THEN
      addWarning(bfXmlFile.iFileNr,SUBSTITUTE("Buffer name in XML file is '&1', db name is '&2'", bfXmlFile.cBuffername, picTableName)).

    /* Check all prim index fields */
    cFieldList = "".
    FOR EACH bfField 
      WHERE bfField.lPrimary = TRUE
        AND LOOKUP(bfField.cFieldName,bfXmlFile.cFields) = 0
        AND LOOKUP(bfField.cFieldName,cIgnoreFields) = 0:
      cFieldList = TRIM(cFieldList + "," + bfField.cFieldName,",").
    END.
    IF cFieldList <> "" THEN
      addError(bfXmlFile.iFileNr,SUBSTITUTE("Primary index fields missing: &1", cFieldList)).

    /* Check all unique index fields */
    cFieldList = "".
    FOR EACH bfField 
      WHERE bfField.lPrimary = FALSE
        AND bfField.lUnique  = TRUE
        AND LOOKUP(bfField.cFieldName,bfXmlFile.cFields) = 0
        AND LOOKUP(bfField.cFieldName,cIgnoreFields) = 0:
      cFieldList = TRIM(cFieldList + "," + bfField.cFieldName,",").
    END.
    IF cFieldList <> "" THEN
      addError(bfXmlFile.iFileNr,SUBSTITUTE("Unique index fields missing : &1", cFieldList)).

    /* Check all mandatory fields */
    cFieldList = "".
    FOR EACH bfField 
      WHERE bfField.lPrimary   = FALSE
        AND bfField.lUnique    = FALSE 
        AND bfField.lMandatory = TRUE
        AND LOOKUP(bfField.cFieldName,bfXmlFile.cFields) = 0
        AND LOOKUP(bfField.cFieldName,cIgnoreFields) = 0:
      cFieldList = TRIM(cFieldList + "," + bfField.cFieldName,",").
    END.
    IF cFieldList <> "" THEN
      addError(bfXmlFile.iFileNr,  SUBSTITUTE("Mandatory fields missing    : &1", cFieldList)).

    /* Check all other fields */
    cFieldList = "".
    FOR EACH bfField 
      WHERE bfField.lPrimary   = FALSE
        AND bfField.lUnique    = FALSE 
        AND bfField.lMandatory = FALSE
        AND LOOKUP(bfField.cFieldName,bfXmlFile.cFields) = 0
        AND LOOKUP(bfField.cFieldName,cIgnoreFields) = 0:
      cFieldList = TRIM(cFieldList + "," + bfField.cFieldName,",").
    END.
    IF cFieldList <> "" THEN
      addWarning(bfXmlFile.iFileNr,SUBSTITUTE("Missing fields in XML       : &1", cFieldList)).

    /* Check for extra fields */
    cFieldList = "".
    DO iField = 1 TO NUM-ENTRIES(bfXmlFile.cFields):
      IF NOT CAN-FIND(bfField WHERE bfField.cFieldName = ENTRY(iField,bfXmlFile.cFields)) THEN
        cFieldList = TRIM(cFieldList + "," + ENTRY(iField,bfXmlFile.cFields),",").
    END.
    IF cFieldList <> "" THEN 
      addWarning(bfXmlFile.iFileNr,SUBSTITUTE("Non-db fields found         : &1", cFieldList)).
  END.


  /* Merge all the XML files into one temp-table */
  IF CAN-FIND(FIRST bfXmlFile WHERE bfXmlFile.lValidFile) THEN
  DO:
    CREATE BUFFER hTableBuffer FOR TABLE picDatabase + "." + picTableName.
    CREATE TEMP-TABLE ghXmlTable.
    
    /* To keep it running on 10.1B */
    &IF PROVERSION < '10.1C' &THEN
    ghXmlTable:CREATE-LIKE(hTableBuffer).
    &ELSE
    ghXmlTable:CREATE-LIKE-SEQUENTIAL(hTableBuffer).
    &ENDIF

    ghXmlTable:TEMP-TABLE-PREPARE(picTableName).
    DELETE OBJECT hTableBuffer.
  
    /* Fix XML Node Names for fields in the tt */
    RUN setXmlNodeNames(INPUT ghXmlTable:DEFAULT-BUFFER-HANDLE).

    fileLoop:
    FOR EACH bfXmlFile WHERE bfXmlFile.lValidFile:
      iExpectedRecords = iExpectedRecords + bfXmlFile.iNumRecords. 
  
      ghXmlTable:READ-XML("FILE",bfXmlFile.cFileName,"MERGE",?,?) NO-ERROR.
      IF ERROR-STATUS:ERROR OR ERROR-STATUS:NUM-MESSAGES > 0 THEN 
      DO:
        addError(bfXmlFile.iFileNr,"Cannot read file contents").
        NEXT fileLoop.
      END.
  
      /* Check on nr of fields in the xml file */
      IF iNumFields = 0 THEN iNumFields = bfXmlFile.iNumFields.
      IF iNumFields <> bfXmlFile.iNumFields THEN 
        addWarning(bfXmlFile.iFileNr,"Number of fields are different in the files").
    END.
  
    addInfo(0,SUBSTITUTE("Total number of records  : &1",iExpectedRecords)).
  
    /* Now check the nr of records in the merged XML */
    CREATE QUERY hQuery.
    hBuffer = ghXmlTable:DEFAULT-BUFFER-HANDLE.

    /* Try to set format similar to db
    DO iField = 1 TO hBuffer:NUM-FIELDS:
      FIND bfField WHERE bfField.cFieldName = hBuffer:BUFFER-FIELD(iField):NAME NO-ERROR.
      IF AVAILABLE bfField THEN
        hBuffer:BUFFER-FIELD(iField):FORMAT = bfField.cFormat.
      ELSE 
        hBuffer:BUFFER-FIELD(iField):FORMAT = "X(100)".
    END.
    */

    hQuery:ADD-BUFFER(hBuffer).
    hQuery:QUERY-PREPARE(SUBSTITUTE("PRESELECT EACH &1",hBuffer:NAME)).
    hQuery:QUERY-OPEN().
    hQuery:GET-FIRST().
  
    IF hQuery:NUM-RESULTS <> iExpectedRecords THEN 
      addWarning(0,SUBSTITUTE("Number of unique records : &1.",hQuery:NUM-RESULTS)).
  
    hQuery:QUERY-CLOSE. 
    DELETE OBJECT hQuery. 
  END.
  ELSE 
  DO:
    addError(0,"No files to load").
  END.

END PROCEDURE. /* checkFiles */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI wImport  _DEFAULT-DISABLE
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
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wImport)
  THEN DELETE WIDGET wImport.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI wImport  _DEFAULT-ENABLE
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
  DISPLAY edSummary 
      WITH FRAME frMain IN WINDOW wImport.
  ENABLE edSummary btnBack btnNext 
      WITH FRAME frMain IN WINDOW wImport.
  {&OPEN-BROWSERS-IN-QUERY-frMain}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject wImport 
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
  DEFINE VARIABLE iValue          AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lNewRecord      AS LOGICAL     NO-UNDO.

  VIEW wImport.

  /* Set fonts */
  DO WITH FRAME {&FRAME-NAME}:

    FRAME {&FRAME-NAME}:FONT = getFont('Default').
    edSummary:FONT = getFont('Fixed').

    /* Window position and size */
    /* Set title of the window */
    wImport:TITLE = SUBSTITUTE('Import &1.&2', picDatabase, picTableName).

    /* Set minimum size of the window */
    wImport:MIN-WIDTH-PIXELS  = 400.
    wImport:MIN-HEIGHT-PIXELS = 200.

    /* to avoid scrollbars on the frame */
    FRAME {&FRAME-NAME}:SCROLLABLE = FALSE.

    /* Set window back to last known pos */
    RUN restoreWindowPos(wImport:HANDLE, "DataDigger:ImportCheck").
  END. 

  IF plReadOnlyDigger THEN btnBack:SENSITIVE = FALSE.

END PROCEDURE. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE loadFiles wImport 
PROCEDURE loadFiles :
/*------------------------------------------------------------------------
  Name         : loadFile
  Description  : Add files, check and show summary
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER pcFileList AS CHARACTER NO-UNDO.

  DEFINE VARIABLE iFile AS INTEGER NO-UNDO.

  DO iFile = 1 TO NUM-ENTRIES(pcFileList,"~n"):
    RUN addFile(ENTRY(iFile,pcFileList,"~n")).
  END.

END PROCEDURE. /* loadFiles */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showSummary wImport 
PROCEDURE showSummary :
/*------------------------------------------------------------------------
  Name         : showSummary
  Description  : Show summary before loading the data
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE cSummary AS CHARACTER   NO-UNDO.

  cSummary = "Summary of files to be loaded:".

  FOR EACH ttXmlFile BY ttXmlFile.iFileNr:
    cSummary = cSummary + "~n~n" + SUBSTITUTE("File: &1", ttXmlFile.cFilename).

    FOR EACH ttMessage WHERE ttMessage.iFileNr = ttXmlFile.iFileNr:
      cSummary = SUBSTITUTE("&1~n - &2&3"
                           , cSummary
                           , TRIM(ENTRY(ttMessage.iType,",Warning: ,**ERROR: "))
                           , ttMessage.cMessage
                           ).
    END.
  END.

  /* General messages */
  cSummary = cSummary + "~n".
  FOR EACH ttMessage WHERE ttMessage.iFileNr = 0:
    cSummary = SUBSTITUTE("&1~n&2", cSummary, ttMessage.cMessage).
  END.

  DO WITH FRAME frMain:
    edSummary:SCREEN-VALUE = cSummary.
  END.

  /* Disable / enable NEXT button only when no errors found */
  btnNext:SENSITIVE = NOT CAN-FIND(FIRST ttMessage WHERE ttMessage.iType = 3).

END PROCEDURE. /* showSummary */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* ************************  Function Implementations ***************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION addError wImport 
FUNCTION addError RETURNS LOGICAL
  ( piFileNr  AS INTEGER 
  , pcMessage AS CHARACTER
  ) :

 RETURN addMessage(piFileNr,3,pcMessage).

END FUNCTION. /* addError */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION addInfo wImport 
FUNCTION addInfo RETURNS LOGICAL
  ( piFileNr  AS INTEGER 
  , pcMessage AS CHARACTER
  ) :

 RETURN addMessage(piFileNr,1,pcMessage).

END FUNCTION. /* addInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION addMessage wImport 
FUNCTION addMessage RETURNS LOGICAL
  ( piFileNr  AS INTEGER 
  , piType    AS INTEGER
  , pcMessage AS CHARACTER
  ) :

  DEFINE BUFFER bfMessage FOR ttMessage. 

  CREATE bfMessage.
  ASSIGN
    bfMessage.iFileNr  = piFileNr
    bfMessage.iType    = piType
    bfMessage.cMessage = pcMessage
    .

 RETURN FALSE.   /* Function return value. */

END FUNCTION. /* addMessage */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION addWarning wImport 
FUNCTION addWarning RETURNS LOGICAL
  ( piFileNr  AS INTEGER 
  , pcMessage AS CHARACTER
  ) :

 RETURN addMessage(piFileNr,2,pcMessage).

END FUNCTION. /* addWarning */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION hasWarnings wImport 
FUNCTION hasWarnings RETURNS LOGICAL
  ( /* parameter-definitions */ ) :

  RETURN CAN-FIND(FIRST ttMessage WHERE ttMessage.iType >= 2).

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
