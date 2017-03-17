&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
/* Connected Databases 
*/
&Scoped-define WINDOW-NAME CURRENT-WINDOW
&Scoped-define FRAME-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Dialog-Frame 
/*------------------------------------------------------------------------
  Name         : wConnections.w
  Description  : Maintain connections for DataDigger
  ---------------------------------------------------------------------- 
  15-10-2009 pti Created
  ----------------------------------------------------------------------*/

/* Parameters Definitions ---                                           */

&IF DEFINED(UIB_is_running)=0 &THEN
  define input parameter pcCommand   as character no-undo.
  define input parameter pcAttribute as character no-undo. 
  define output parameter pcResult   as character no-undo. 
&ELSE
  define variable pcCommand   as character no-undo initial "UI".
  define variable pcAttribute as character no-undo initial "". 
  define variable pcResult    as character no-undo initial "". 
&ENDIF   

{ datadigger.i } 

define variable gcRecordState as character no-undo initial 'nodata'.

/* Allow testing */
&IF DEFINED(UIB_is_running) <> 0 &THEN

  define variable hDiggerLib as handle no-undo.
  run DataDiggerLib.p persistent set hDiggerLib.
  session:add-super-procedure(hDiggerLib, search-target).

  /* Load or create personalized ini files */
  define variable cEnvironment as character no-undo.
  define variable cProgDir     as character no-undo.

  cEnvironment = substitute('DataDigger-&1', getUserName()).
  output to value(cProgDir + cEnvironment + '.ini') append.
  output close. 
  load cEnvironment dir cProgDir base-key 'ini' no-error.

  cEnvironment = 'DataDigger'.
  output to value(cProgDir + cEnvironment + '.ini') append.
  output close. 
  load cEnvironment dir cProgDir base-key 'ini' no-error.

&ENDIF

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME Dialog-Frame
&Scoped-define BROWSE-NAME brConnections

/* Internal Tables (found by Frame, Query & Browse Queries)             */
&Scoped-define INTERNAL-TABLES ttConnection

/* Definitions for BROWSE brConnections                                 */
&Scoped-define FIELDS-IN-QUERY-brConnections ttConnection.cLogicalName ttConnection.cDescription ttConnection.lConnected   
&Scoped-define ENABLED-FIELDS-IN-QUERY-brConnections   
&Scoped-define SELF-NAME brConnections
&Scoped-define QUERY-STRING-brConnections FOR EACH ttConnection
&Scoped-define OPEN-QUERY-brConnections OPEN QUERY {&SELF-NAME} FOR EACH ttConnection.
&Scoped-define TABLES-IN-QUERY-brConnections ttConnection
&Scoped-define FIRST-TABLE-IN-QUERY-brConnections ttConnection


/* Definitions for DIALOG-BOX Dialog-Frame                              */
&Scoped-define OPEN-BROWSERS-IN-QUERY-Dialog-Frame ~
    ~{&OPEN-QUERY-brConnections}

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS btnBrowse btnDelete btnTest btnConnect ~
btnDisconnect cbSection btnAdd brConnections btnClone btnEdit btnSave ~
btnUndo 
&Scoped-Define DISPLAYED-OBJECTS fiLogicalName cbSection fiDescription ~
fiDatabaseName edParameters 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getNewConnectionNr Dialog-Frame 
FUNCTION getNewConnectionNr RETURNS INTEGER
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnAdd  NO-FOCUS FLAT-BUTTON
     LABEL "&Add" 
     SIZE-PIXELS 25 BY 24 TOOLTIP "add a connection".

DEFINE BUTTON btnBrowse 
     LABEL "..." 
     SIZE-PIXELS 25 BY 24 TOOLTIP "find database".

DEFINE BUTTON btnClone  NO-FOCUS FLAT-BUTTON
     LABEL "&Clone" 
     SIZE-PIXELS 25 BY 24 TOOLTIP "clone connection".

DEFINE BUTTON btnConnect DEFAULT  NO-FOCUS FLAT-BUTTON
     LABEL "&Con" 
     SIZE-PIXELS 25 BY 24 TOOLTIP "connect selected database"
     BGCOLOR 8 .

DEFINE BUTTON btnDelete  NO-FOCUS FLAT-BUTTON
     LABEL "&Delete" 
     SIZE-PIXELS 25 BY 24 TOOLTIP "delete the currently selected connection".

DEFINE BUTTON btnDisconnect  NO-FOCUS FLAT-BUTTON
     LABEL "&Dis" 
     SIZE-PIXELS 25 BY 24 TOOLTIP "disconnect selected database".

DEFINE BUTTON btnEdit  NO-FOCUS FLAT-BUTTON
     LABEL "&Edit" 
     SIZE-PIXELS 25 BY 24 TOOLTIP "change settings of currently selected connection".

DEFINE BUTTON btnSave DEFAULT  NO-FOCUS FLAT-BUTTON
     LABEL "&Save" 
     SIZE-PIXELS 25 BY 24 TOOLTIP "save changes"
     BGCOLOR 8 .

DEFINE BUTTON btnTest DEFAULT 
     LABEL "&Test" 
     SIZE-PIXELS 60 BY 24 TOOLTIP "test the currently selected connection"
     BGCOLOR 8 .

DEFINE BUTTON btnUndo DEFAULT  NO-FOCUS FLAT-BUTTON
     LABEL "&Undo" 
     SIZE-PIXELS 25 BY 24 TOOLTIP "cancel changes"
     BGCOLOR 8 .

DEFINE VARIABLE cbSection AS CHARACTER 
     LABEL "Section in INI" 
     VIEW-AS COMBO-BOX SORT INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN
     SIZE-PIXELS 160 BY 21 TOOLTIP "the section in the INI file to save the settings to"
     FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE edParameters AS CHARACTER 
     VIEW-AS EDITOR SCROLLBAR-VERTICAL
     SIZE-PIXELS 470 BY 180 TOOLTIP "the connection parameters for this database"
     FGCOLOR 1 FONT 2 NO-UNDO.

DEFINE VARIABLE fiDatabaseName AS CHARACTER FORMAT "X(256)":U 
     LABEL "DB/PF name" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 375 BY 21 TOOLTIP "the physical database name or PF file name to connect"
     FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiDescription AS CHARACTER FORMAT "X(256)":U 
     LABEL "Description" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 400 BY 21 TOOLTIP "the description of this connection"
     FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiLogicalName AS CHARACTER FORMAT "X(256)":U 
     LABEL "Logical Name" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 TOOLTIP "the logical name for this connection (no spaces)"
     FGCOLOR 1  NO-UNDO.

/* Query definitions                                                    */
&ANALYZE-SUSPEND
DEFINE QUERY brConnections FOR 
      ttConnection SCROLLING.
&ANALYZE-RESUME

/* Browse definitions                                                   */
DEFINE BROWSE brConnections
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _DISPLAY-FIELDS brConnections Dialog-Frame _FREEFORM
  QUERY brConnections DISPLAY
      ttConnection.cLogicalName
ttConnection.cDescription
ttConnection.lConnected
/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
    WITH NO-ROW-MARKERS SEPARATORS
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 60 BY 15
          &ELSE SIZE-PIXELS 299 BY 324 &ENDIF FIT-LAST-COLUMN.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     btnBrowse AT Y 90 X 760 WIDGET-ID 56
     btnDelete AT Y 5 X 565 WIDGET-ID 14
     btnTest AT Y 325 X 725 WIDGET-ID 42
     btnConnect AT Y 5 X 315 WIDGET-ID 48
     fiLogicalName AT Y 40 X 375 COLON-ALIGNED WIDGET-ID 4
     btnDisconnect AT Y 5 X 340 WIDGET-ID 52
     cbSection AT Y 40 X 615 COLON-ALIGNED WIDGET-ID 54
     btnAdd AT Y 5 X 390 WIDGET-ID 2
     fiDescription AT Y 65 X 375 COLON-ALIGNED WIDGET-ID 38
     fiDatabaseName AT Y 92 X 375 COLON-ALIGNED WIDGET-ID 6
     brConnections AT Y 1 X 1 WIDGET-ID 200
     btnClone AT Y 5 X 415 WIDGET-ID 46
     edParameters AT Y 145 X 315 NO-LABEL WIDGET-ID 10
     btnEdit AT Y 5 X 440 WIDGET-ID 12
     btnSave AT Y 5 X 490 WIDGET-ID 22
     btnUndo AT Y 5 X 515 WIDGET-ID 24
     "Parameters:" VIEW-AS TEXT
          SIZE-PIXELS 120 BY 13 AT Y 130 X 315 WIDGET-ID 18
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 798 BY 382
         TITLE "Database Connections" DROP-TARGET WIDGET-ID 100.


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
/* BROWSE-TAB brConnections fiDatabaseName Dialog-Frame */
ASSIGN 
       FRAME Dialog-Frame:SCROLLABLE       = FALSE
       FRAME Dialog-Frame:HIDDEN           = TRUE.

ASSIGN 
       brConnections:ALLOW-COLUMN-SEARCHING IN FRAME Dialog-Frame = TRUE
       brConnections:COLUMN-RESIZABLE IN FRAME Dialog-Frame       = TRUE.

/* SETTINGS FOR EDITOR edParameters IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN fiDatabaseName IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN fiDescription IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN fiLogicalName IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME


/* Setting information for Queries and Browse Widgets fields            */

&ANALYZE-SUSPEND _QUERY-BLOCK BROWSE brConnections
/* Query rebuild information for BROWSE brConnections
     _START_FREEFORM
OPEN QUERY {&SELF-NAME} FOR EACH ttConnection.
     _END_FREEFORM
     _Query            is OPENED
*/  /* BROWSE brConnections */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON DROP-FILE-NOTIFY OF FRAME Dialog-Frame /* Database Connections */
do:
  run addConnections.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON MOUSE-SELECT-CLICK OF FRAME Dialog-Frame /* Database Connections */
DO:
  define variable hWidget as handle no-undo.

  hWidget = getWidgetUnderMouse(input frame dialog-frame:handle).

  if not valid-handle(hWidget) then return. 

  if lookup(hWidget:name, "fiLogicalName,fiDescription,cbSection,fiDatabaseName,edParameters") > 0 then 
    run btnEditChoose. 

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* Database Connections */
DO:
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brConnections
&Scoped-define SELF-NAME brConnections
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brConnections Dialog-Frame
ON START-SEARCH OF brConnections IN FRAME Dialog-Frame
DO:
  run openConnectionQuery(input self:current-column:name,?).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brConnections Dialog-Frame
ON VALUE-CHANGED OF brConnections IN FRAME Dialog-Frame
DO:
  run viewConnection.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnAdd
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAdd Dialog-Frame
ON CHOOSE OF btnAdd IN FRAME Dialog-Frame /* Add */
or "insert-mode" of brConnections
DO:

  run btnAddChoose.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnBrowse
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnBrowse Dialog-Frame
ON CHOOSE OF btnBrowse IN FRAME Dialog-Frame /* ... */
do:

  define variable lOkay     as logical    no-undo.
  define variable cFileName as character  no-undo.

  cFileName = fiDatabaseName:screen-value.

  system-dialog get-file cFilename
    filters "Databases (*.db)" "*.db",
            "All Files (*.*)" "*.*"
    initial-filter 1
    must-exist
    use-filename
    default-extension ".db"
    update lOkay.
  
  if not lOkay then 
    return.

  do with frame {&frame-name}:
    fiDatabaseName:screen-value = cFileName.
  end.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClone
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClone Dialog-Frame
ON CHOOSE OF btnClone IN FRAME Dialog-Frame /* Clone */
or "shift-ins" of brConnections
or "alt-o" of brConnections
DO:

  run btnCloneChoose.
  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnConnect
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnConnect Dialog-Frame
ON CHOOSE OF btnConnect IN FRAME Dialog-Frame /* Con */
do:

  run btnConnectChoose. 

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDelete
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDelete Dialog-Frame
ON CHOOSE OF btnDelete IN FRAME Dialog-Frame /* Delete */
or "delete-character" of brConnections
do:

  run btnDeleteChoose.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDisconnect
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDisconnect Dialog-Frame
ON CHOOSE OF btnDisconnect IN FRAME Dialog-Frame /* Dis */
DO:

  run btnDisconnectChoose.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnEdit Dialog-Frame
ON CHOOSE OF btnEdit IN FRAME Dialog-Frame /* Edit */
or "DEFAULT-ACTION" of brConnections 
DO:
  run btnEditChoose. 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnSave
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSave Dialog-Frame
ON CHOOSE OF btnSave IN FRAME Dialog-Frame /* Save */
or "RETURN" of fiLogicalName, fiDescription, fiDatabaseName, cbSection 
DO:

  run btnSaveChoose.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTest
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTest Dialog-Frame
ON CHOOSE OF btnTest IN FRAME Dialog-Frame /* Test */
DO:

  run btnTestChoose. 

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnUndo
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnUndo Dialog-Frame
ON CHOOSE OF btnUndo IN FRAME Dialog-Frame /* Undo */
DO:

  run btnUndoChoose.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fiLogicalName
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiLogicalName Dialog-Frame
ON ANY-PRINTABLE OF fiLogicalName IN FRAME Dialog-Frame /* Logical Name */
DO:
  if lastkey = 32 then 
  do:
    apply '_'.
    return no-apply.
  end.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Dialog-Frame 


/* ***************************  Main Block  *************************** */

/* Parent the dialog-box to the ACTIVE-WINDOW, if there is no parent.   */
IF VALID-HANDLE(ACTIVE-WINDOW) AND FRAME {&FRAME-NAME}:PARENT eq ?
THEN FRAME {&FRAME-NAME}:PARENT = ACTIVE-WINDOW.

ON END-ERROR OF frame Dialog-Frame
OR ENDKEY OF frame Dialog-Frame anywhere 
do:
  if fiLogicalName:sensitive then 
  do:
    run btnUndoChoose.
    return no-apply.
  end. 
END.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  run initializeObject.

  case pcCommand:
    when 'connect'        then run connectDatabase(input pcAttribute, output pcResult).
    when 'getConnections' then run getConnections(output pcResult).
    when 'UI'             then do:
      RUN enable_UI.
      run openConnectionQuery(?,?).
      apply "value-changed" to brConnections in frame {&frame-name}.
      apply "ENTRY" to brConnections in frame {&frame-name}.         
      WAIT-FOR GO OF FRAME {&FRAME-NAME}.
    end.
  end case.

END.

RUN disable_UI.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE addConnections Dialog-Frame 
PROCEDURE addConnections :
/*------------------------------------------------------------------------
  Name         : addConnections
  Description  : Add dropped files as connections 
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/
  
  define variable iFile as integer   no-undo. 
  define variable cFile as character no-undo. 
  define variable cName as character no-undo. 
  define variable rReposition as rowid no-undo. 
  define variable cContents as longchar no-undo. 

  DO WITH FRAME dialog-frame:

    /* If we are in edit-mode, accept only the first dropped file */
    if gcRecordState = "edit" 
      and self:get-dropped-file(1) matches "*~.pf" then 
      fiDatabaseName:screen-value = self:get-dropped-file(1).
  
    /* Otherwise, add all files as new connections */
    else
    do iFile = 1 to self:num-dropped-files:
      cFile = self:get-dropped-file(iFile).
      if not cFile matches "*~.pf" and not cFile matches "*~.db" then next. 
  
      cName = entry(num-entries(cFile,"\"),cFile,"\").
  
      if not can-find(first ttConnection where ttConnection.cDatabaseName = cName) then
      do:
        create ttConnection.
        assign 
          ttConnection.iConnectionNr = getNewConnectionNr()
          ttConnection.cLogicalName  = entry(1,cName,".")
          ttConnection.cLogicalName  = replace(ttConnection.cLogicalName,' ','_')
          ttConnection.cDescription  = entry(1,cName,".")
          ttConnection.cDatabaseName = cFile
          ttConnection.cSection      = ttConnection.cLogicalName
          ttConnection.lConnected    = false
          .
  
        /* Different settings for PF and DB files */
        if cName matches "*~.db" then
          ttConnection.cParameters = "-1".
        else 
        do:
          copy-lob from file cFile to cContents NO-CONVERT.
          ttConnection.cParameters = string(cContents).
        end.

        /* Save to registry */
        run saveConnection(buffer ttConnection).
  
        if rReposition = ? then rReposition = rowid(ttConnection).
      end. 
    end.
  
    if rReposition <> ? then
    do:
      run openConnectionQuery(?,?).
      brConnections:query:reposition-to-rowid(rReposition) no-error.
      apply "VALUE-CHANGED" to brConnections.
    end.
  END.
  
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnAddChoose Dialog-Frame 
PROCEDURE btnAddChoose :
/*------------------------------------------------------------------------
  Name         : btnAddChoose
  Description  : Set screen to add-mode
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/
  
  gcRecordState = 'new'.

  create ttConnection.
  assign ttConnection.iConnectionNr = getNewConnectionNr().

  run viewConnection. 
  run setToolbar.

  apply "ENTRY" to fiLogicalName in frame dialog-frame. 

END PROCEDURE. /* btnAddChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnCloneChoose Dialog-Frame 
PROCEDURE btnCloneChoose :
/*------------------------------------------------------------------------
  Name         : btnCloneChoose
  Description  : Duplicate a connection and go to update-mode
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  define buffer bfOriginalConnection for ttConnection.

  gcRecordState = 'new'.

  find bfOriginalConnection 
    where rowid(bfOriginalConnection) = rowid(ttConnection).

  create ttConnection.
  buffer-copy bfOriginalConnection to ttConnection
    assign ttConnection.iConnectionNr = getNewConnectionNr()
           ttConnection.lConnected    = false.

  run viewConnection. 
  run setToolbar.
  apply "ENTRY" to fiLogicalName in frame dialog-frame. 

END PROCEDURE. /* btnCloneChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnConnectChoose Dialog-Frame 
PROCEDURE btnConnectChoose :
/*------------------------------------------------------------------------
  Name         : btnConnectChoose
  Description  : Try to connect to the current connection
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  define variable iError as integer     no-undo.
  define variable cError as character   no-undo initial 'Errors:'.
  define variable lAlreadyConnected as logical no-undo. 

  DO WITH FRAME dialog-frame:
    /* Warn if already connected */
    lAlreadyConnected = (lookup( fiLogicalName:screen-value, getDatabaseList()) > 0).
  
    /* Try to establish a connection */
    session:set-wait-state('general').
    run connectDatabase(input fiLogicalName:screen-value, output cError).
    session:set-wait-state('').
  
    /* Refresh connection status */
    ttConnection.lConnected = (lookup( ttConnection.cLogicalName, getDatabaseList() ) > 0). 
    brConnections:refresh().
    run viewConnection.
  
    /* If no success, show why */
    if error-status:error then
    do:
      do iError = 1 TO error-status:num-messages:
        cError = substitute('&1~n&2 (&3)'
                           , cError
                           , error-status:get-message(iError)
                           , error-status:get-number(iError)
                           ).
      end.
      message cError view-as alert-box info buttons ok.
      apply "ENTRY" to brConnections.
      return no-apply.
    end.

    else 
    /* Success, but report if db was already connected */
    if lAlreadyConnected then 
    do:
      message 'Database already connected' view-as alert-box info buttons ok.
      apply "ENTRY" to brConnections.
      return no-apply.
    end.

    apply "ENTRY" to brConnections.
  END.

END PROCEDURE. /* btnConnectChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnDeleteChoose Dialog-Frame 
PROCEDURE btnDeleteChoose :
/*------------------------------------------------------------------------
  Name         : btnDeleteChoose
  Description  : Delete a connection
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  define variable lOk         as logical no-undo initial true.
  define variable iConn       as integer no-undo. 
  define variable rConnection as rowid no-undo. 
  define variable rDelete     as rowid no-undo. 

  DO WITH FRAME dialog-frame:
    message 'Delete this connection?' view-as alert-box info buttons yes-no-cancel update lOk.
  
    if lOk = true then
    do:
      /* Delete from registry */
      iConn = ttConnection.iConnectionNr.
      setRegistry('Connections', substitute('&1-ldbname'    , string(iConn,'999')), ? ).
      setRegistry('Connections', substitute('&1-description', string(iConn,'999')), ? ).
      setRegistry('Connections', substitute('&1-pdbname'    , string(iConn,'999')), ? ).
      setRegistry('Connections', substitute('&1-parameters' , string(iConn,'999')), ? ).
  
      /* Remember record to delete */
      rDelete = brConnections:query:get-buffer-handle(1):rowid.

      /* And try to find the "next" connection, from the query's point of view */
      if brConnections:query:get-next then 
        rConnection = brConnections:query:get-buffer-handle(1):rowid.

      /* Find back record to delete */
      brConnections:query:reposition-to-rowid(rDelete) no-error.
      delete ttConnection.

      /* Point browse to next connection */
      brConnections:query:reposition-to-rowid(rConnection) no-error.

      /* And reopen */
      run openConnectionQuery(?,?).
      run viewConnection.
    end.

    apply "ENTRY" to brConnections.
  END.

END PROCEDURE. /* btnDeleteChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnDisconnectChoose Dialog-Frame 
PROCEDURE btnDisconnectChoose :
/*------------------------------------------------------------------------
  Name         : btnDisconnectChoose
  Description  : Disconnect db
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE cCurrentDb  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lDisconnect AS LOGICAL     NO-UNDO.

  cCurrentDb = fiLogicalName:screen-value in frame dialog-frame.

  message substitute('Are you sure you want to disconnect database "&1"?', cCurrentDb)
    view-as alert-box info buttons yes-no-cancel update lDisconnect.
  if lDisconnect <> yes then return. 

  disconnect value(cCurrentDb).
  removeConnection(cCurrentDb).

  ttConnection.lConnected = (lookup( ttConnection.cLogicalName, getDatabaseList() ) > 0). 
  brConnections:refresh().
  run viewConnection.

  apply "ENTRY" to brConnections.

END PROCEDURE. /* btnDisconnectChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnEditChoose Dialog-Frame 
PROCEDURE btnEditChoose :
/*------------------------------------------------------------------------
  Name         : btnEditChoose
  Description  : Set screen to edit-mode
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  DO WITH FRAME dialog-frame:
    gcRecordState = 'edit'.
    run setToolbar.
    apply "ENTRY" to fiLogicalName in frame dialog-frame. 
  END.

END PROCEDURE. /* btnEditChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnSaveChoose Dialog-Frame 
PROCEDURE btnSaveChoose :
/*------------------------------------------------------------------------
  Name         : btnSaveChoose
  Description  : Save the current connection
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  DO WITH FRAME dialog-frame:

    /* No spaces in logical name */
    fiLogicalName:screen-value = replace(fiLogicalName:screen-value,' ','_').
  
    assign 
      ttConnection.cLogicalName  = fiLogicalName:screen-value 
      ttConnection.cDescription  = fiDescription:screen-value 
      ttConnection.cDatabaseName = fiDatabaseName:screen-value
      ttConnection.cSection      = cbSection:screen-value
      ttConnection.cParameters   = edParameters:screen-value   
      .
    if ttConnection.cSection = ? then ttConnection.cSection = ttConnection.cLogicalName.

    /* Save to registry */
    run saveConnection(buffer ttConnection).
  
    run openConnectionQuery(?,?).
    run viewConnection. 
    apply "ENTRY" to brConnections.

  END.

END PROCEDURE. /* btnSaveChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnTestChoose Dialog-Frame 
PROCEDURE btnTestChoose :
/*------------------------------------------------------------------------
  Name         : btnTestChoose
  Description  : Test the current connection
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  define variable iError as integer     no-undo.
  define variable cError as character   no-undo initial 'Errors:'.
  define variable lAlreadyConnected as logical no-undo. 

  DO WITH FRAME dialog-frame:
    lAlreadyConnected = (lookup( fiLogicalName:screen-value, getDatabaseList()) > 0).
      
    /* Try to establish a connection */
    session:set-wait-state('general').
    connect value(fiDatabaseName:screen-value)
            value(edParameters:screen-value)
            value(substitute(' -ld &1', fiLogicalName:screen-value))
      no-error.
    session:set-wait-state('').
  
    /* If no success, show why */
    if error-status:error then
    do:
      do iError = 1 TO error-status:num-messages:
        cError = substitute('&1~n&2 (&3)'
                           , cError
                           , error-status:get-message(iError)
                           , error-status:get-number(iError)
                           ).
      end.
      message cError view-as alert-box info buttons ok.
    end.
    else 
    do:
      /* Otherwise disconnect the db since it's only a test */
      if not lAlreadyConnected then
        disconnect value(ldbname(num-dbs)).
      message 'Connection successful' view-as alert-box info buttons ok.
    end.
  END.

END PROCEDURE. /* btnTestConnection */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnUndoChoose Dialog-Frame 
PROCEDURE btnUndoChoose :
/*------------------------------------------------------------------------
  Name         : btnUndoChoose
  Description  : Undo changes and go back to display-mode
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  DO WITH FRAME dialog-frame:

    if gcRecordState = 'new' then delete ttConnection.
  
    run openConnectionQuery(?,?).
    run viewConnection.
  
    if can-find(first ttConnection) then
      gcRecordState = 'display'.
    else 
      gcRecordState = 'nodata'.
  
    run setToolbar.
  
    apply "ENTRY" to brConnections.
  end.

END PROCEDURE. /* btnUndoChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE connectDatabase Dialog-Frame 
PROCEDURE connectDatabase :
/*------------------------------------------------------------------------
  Name         : connectDatabase
  Description  : Try to connect to a given database. 
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define input parameter pcConnection as character no-undo. 
  define output parameter pcError as character no-undo. 

  define variable iError as integer     no-undo.
  define buffer bConnection for ttConnection. 

  /* Find the connection */
  find bConnection where bConnection.cLogicalName = pcConnection no-lock no-error. 
  if not available bConnection then 
  do:
    assign pcError = 'No such connection known'.
    return. 
  end.

  /* Try to establish a connection */
  session:set-wait-state('general').
  
  if bConnection.cDatabaseName matches "*~.pf" then
    connect value("-pf " + bConnection.cDatabaseName) no-error.
  else
  do:
    connect value(bConnection.cDatabaseName)
            value(bConnection.cParameters)
            value(substitute(' -ld &1', bConnection.cLogicalName)) no-error.

    if not error-status:error then
      addConnection(bConnection.cLogicalName, bConnection.cSection).
  end.

  session:set-wait-state('').

  /* If no success, show why */
  if error-status:error then
  do:
    pcError = 'Error connecting database:'.

    do iError = 1 TO error-status:num-messages:
      pcError = substitute('&1~n&2 (&3)'
                         , pcError
                         , error-status:get-message(iError)
                         , error-status:get-number(iError)
                         ).
    end.
    return.
  end.

END PROCEDURE. /* connectDatabase */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

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
  DISPLAY fiLogicalName cbSection fiDescription fiDatabaseName edParameters 
      WITH FRAME Dialog-Frame.
  ENABLE btnBrowse btnDelete btnTest btnConnect btnDisconnect cbSection btnAdd 
         brConnections btnClone btnEdit btnSave btnUndo 
      WITH FRAME Dialog-Frame.
  VIEW FRAME Dialog-Frame.
  {&OPEN-BROWSERS-IN-QUERY-Dialog-Frame}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getConnections Dialog-Frame 
PROCEDURE getConnections :
/*------------------------------------------------------------------------
  Name         : getConnections
  Description  : Return a comma separated list of all connections
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/

  define output parameter pcConnectionList as character no-undo. 

  define buffer bConnection for ttConnection. 

  for each bConnection by bConnection.cLogicalName:
    pcConnectionList = pcConnectionList + bConnection.cLogicalName + ','.
  end.
  
  pcConnectionList = trim(pcConnectionList,',').

end procedure. /* getConnections */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject Dialog-Frame 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------
  Name         : initializeObject
  Description  : Prepare the program. 
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/

  empty temp-table ttConnection.

  define variable iConnection as integer   no-undo. 
  define variable iFixedFont  as integer   no-undo. 
  define variable cSetting    as character no-undo. 
  
  if pcCommand = 'UI' then 
  do with frame {&frame-name}:
    btnAdd       :load-image(getImagePath('Add.gif')).
    btnClone     :load-image(getImagePath('Clone.gif')).
    btnEdit      :load-image(getImagePath('Edit.gif')).
    btnDelete    :load-image(getImagePath('Delete.gif')).
    btnSave      :load-image(getImagePath('Save.gif')).
    btnUndo      :load-image(getImagePath('Clear.gif')).
    btnConnect   :load-image(getImagePath('Execute.gif')).
    btnDisconnect:load-image(getImagePath('Stop.gif')).

    iFixedFont = getFont('Fixed').
    fiLogicalName :font = iFixedFont.
    fiDescription :font = iFixedFont.
    fiDatabaseName:font = iFixedFont.
    edParameters  :font = iFixedFont.
    cbSection     :font = iFixedFont.
  end. 

  /* Arbitrarily test for max 999 connections 
   * connection numbers need to be sequential
   */
  connectionLoop:
  do iConnection = 1 to 999:

    /* Find the ID of the connection. */
    cSetting = getRegistry('Connections', substitute('&1-ldbname',string(iConnection,'999'))).
    if cSetting = ? then next connectionLoop.

    create ttConnection.
    ttConnection.iConnectionNr = iConnection.
    ttConnection.cLogicalName  = cSetting.
    ttConnection.cDescription  = getRegistry('Connections', substitute('&1-description',string(iConnection,'999'))).
    ttConnection.cDatabaseName = getRegistry('Connections', substitute('&1-pdbname'    ,string(iConnection,'999'))).
    ttConnection.cParameters   = getRegistry('Connections', substitute('&1-parameters' ,string(iConnection,'999'))).
    ttConnection.cParameters   = replace(ttConnection.cParameters,chr(1),'~n').
    ttConnection.cSection      = getRegistry('Connections', substitute('&1-section'    ,string(iConnection,'999'))).
    ttConnection.lConnected    = (lookup( ttConnection.cLogicalName, getDatabaseList() ) > 0).

    /* Protect against blank value */
    if ttConnection.cDescription  = ? then ttConnection.cDescription  = "".
    if ttConnection.cDatabaseName = ? then ttConnection.cDatabaseName = "".
    if ttConnection.cParameters   = ? then ttConnection.cParameters   = "".
    if ttConnection.cSection      = ? then ttConnection.cSection      = ttConnection.cLogicalName.
  end.

  /* Get sort for Connections */
  do with frame {&frame-name}:

    /* Get fonts */
    frame {&frame-name}:font = getFont('Default').
    edParameters:font = getFont('Fixed').

    cSetting = getRegistry('DataDigger','ColumnSortConnections').
    if cSetting <> ? then
      brConnections:set-sort-arrow(integer(entry(1,cSetting)), logical(entry(2,cSetting)) ).
  end.

  run openConnectionQuery(?,?).

end procedure. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE openConnectionQuery Dialog-Frame 
PROCEDURE openConnectionQuery :
/*------------------------------------------------------------------------
  Name         : btnUndoChoose
  Description  : Undo changes and go back to display-mode
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  define input parameter pcSortField  as character   no-undo.
  define input parameter plAscending  as logical     no-undo.

  define variable rCurrentRecord   as rowid       no-undo. 
  define variable lAscending       as logical     no-undo.
  define variable cOldSort         as character   no-undo. 
  define variable cNewSort         as character   no-undo. 
  define variable cQuery           as character   no-undo.
  define variable hQuery           as handle      no-undo.
  define variable cConnectionList  as character   no-undo.
  define variable cSectionName     as character   no-undo. 

  do with frame {&frame-name}:

    /* Protect routine against invalid input */
    if pcSortField = '' then pcSortField = ?.

    /* Remember record we're on */
    if brConnections:num-selected-rows > 0 then 
      rCurrentRecord = brConnections:query:get-buffer-handle(1):rowid.

    /* Find out what the current sort is */
    run getColumnSort(input brConnections:handle, output cOldSort, output lAscending).

    /* If no new sortfield is provided, we don't want to change the sort.
     * This happens when we press the filter button.
     */
    if pcSortField = ? then
      assign 
        cNewSort   = cOldSort
        lAscending = lAscending. /* dont change order */
    else
    if pcSortField = cOldSort then
      assign 
        cNewSort   = cOldSort
        lAscending = not lAscending. /* invert order */
    else
      /* New field */
      assign 
        cNewSort   = pcSortField
        lAscending = true.

    /* Sort direction might be overruled */
    if plAscending <> ? then lAscending = plAscending.

    /* Wich column should have what arrow? */
    run setSortArrow(brConnections:handle, cNewSort, lAscending).

    /* Rebuild the query */
    if valid-handle(brConnections:query) then do:
      brConnections:query:query-close().
      delete object brConnections:query.
    end.

    create query hQuery.
    hQuery:set-buffers(buffer ttConnection:handle).

    /* Build the query */
    cQuery = 'for each ttConnection where true'.
    cQuery = substitute("&1 by &2 &3", cQuery, cNewSort, string(lAscending,'/descending')).
    hQuery:query-prepare(cQuery).
    hQuery:query-open().
    hQuery:get-first.

    /* Attach query to the browse */
    brConnections:query in frame {&frame-name} = hQuery.

    /* Jump back to selected row */
    if not hQuery:query-off-end 
      and can-find(ttConnection where rowid(ttConnection) = rCurrentRecord) then
    do:
      hQuery:reposition-to-rowid(rCurrentRecord) no-error.
      brConnections:select-focused-row().
    end.

    if available ttConnection then
      gcRecordState = 'display'.
    else
      gcRecordState = 'nodata'.

    /* Collect all db names for "share settings" combo */
    run getConnections(output cConnectionList).    
    cbSection:list-items = " ," + cConnectionList. 
  end.
  
  run setToolbar.

END PROCEDURE. /* openConnectionQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveConnection Dialog-Frame 
PROCEDURE saveConnection :
/*------------------------------------------------------------------------
  Name         : saveConnection
  Description  : Save a connection to the INI file
  ---------------------------------------------------------------------- 
  17-12-2012 pti Created
  ----------------------------------------------------------------------*/

  define parameter buffer ttConnection for ttConnection. 

  define variable iConn as integer no-undo. 

  iConn = ttConnection.iConnectionNr.

  setRegistry('Connections', substitute('&1-ldbname'    , string(iConn,'999')), ttConnection.cLogicalName  ).
  setRegistry('Connections', substitute('&1-description', string(iConn,'999')), ttConnection.cDescription  ).
  setRegistry('Connections', substitute('&1-pdbname'    , string(iConn,'999')), ttConnection.cDatabaseName ).
  setRegistry('Connections', substitute('&1-parameters' , string(iConn,'999')), replace(ttConnection.cParameters,'~n',chr(1)) ).
  setRegistry('Connections', substitute('&1-section'    , string(iConn,'999')), ttConnection.cSection  ).

END PROCEDURE. /* saveConnection */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setToolbar Dialog-Frame 
PROCEDURE setToolbar :
/*------------------------------------------------------------------------
  Name         : setToolbar
  Description  : Set the state of the icons on the toolbar
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/

  do with frame {&frame-name}:
    disable btnAdd btnClone btnEdit btnDelete btnTest btnSave btnUndo 
            fiLogicalName fiDescription fiDatabaseName cbSection edParameters btnBrowse
            .
  
    case gcRecordState:
      when 'nodata'  then enable btnAdd  .
      when 'display' then enable btnAdd  btnClone btnEdit btnDelete btnTest.
      when 'edit'    then enable btnTest btnSave btnUndo btnDelete fiLogicalName fiDescription fiDatabaseName cbSection edParameters btnBrowse.
      when 'new'     then enable btnTest btnSave btnUndo fiLogicalName fiDescription fiDatabaseName cbSection edParameters btnBrowse.
    end case.

    brConnections:sensitive = (gcRecordState = 'display').
  end.

END PROCEDURE. /* setToolbar */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE viewConnection Dialog-Frame 
PROCEDURE viewConnection :
/*------------------------------------------------------------------------
  Name         : viewConnection
  Description  : Show the details of the connection on the screen. 
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/
  
  do with frame {&frame-name}:

    if available ttConnection then
    do:
      assign 
        fiLogicalName:screen-value   = ttConnection.cLogicalName
        fiDescription:screen-value   = ttConnection.cDescription
        fiDatabaseName:screen-value  = ttConnection.cDatabaseName
        edParameters:screen-value    = ttConnection.cParameters
        cbSection:screen-value       = ttConnection.cSection 
        btnDisconnect:sensitive      = ttConnection.lConnected
        btnConnect:sensitive         = not ttConnection.lConnected
        btnBrowse:sensitive          = false
        .  
    end.
    else
      assign 
        fiLogicalName:screen-value   = ""
        fiDescription:screen-value   = ""
        fiDatabaseName:screen-value  = ""
        edParameters:screen-value    = ""
        cbSection:screen-value       = ?
        btnDisconnect:sensitive      = false
        btnConnect:sensitive         = false
        btnBrowse:sensitive          = false
        .
  end.

END PROCEDURE. /* viewConnection */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* ************************  Function Implementations ***************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getNewConnectionNr Dialog-Frame 
FUNCTION getNewConnectionNr RETURNS INTEGER
  ( /* parameter-definitions */ ) :

/*------------------------------------------------------------------------
  Name         : getNewConnectionNr
  Description  : Return a nr for the new connection.
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable iNewNr as integer no-undo.

  do iNewNr = 1 to 999:
    if not can-find(ttConnection where ttConnection.iConnectionNr = iNewNr) then leave.
  end.

  /* No nrs avail */
  if iNewNr = 999 then 
    message "Out of connection numbers! ~nPlease contact patrick@tingen.net" view-as alert-box info buttons ok.
  else
  if iNewNr > 900 then 
    message "Almost out of connection numbers! ~nPlease contact patrick@tingen.net" view-as alert-box info buttons ok.

  return iNewNr.   /* Function return value. */

end function. /* getNewConnectionNr */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

