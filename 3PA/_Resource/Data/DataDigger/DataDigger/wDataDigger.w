&ANALYZE-SUSPEND _VERSION-NUMBER UIB_v9r12 GUI
&ANALYZE-RESUME
/* Connected Databases 
*/
&Scoped-define WINDOW-NAME C-Win
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS C-Win 
/*
 * Main program for DataDigger
 */

/* Parameter tells if we are in read-only mode */
&IF "{&uib_is_running}" = "" &THEN
  DEFINE INPUT PARAMETER plReadOnlyDigger AS LOGICAL NO-UNDO.
&ELSE
  DEFINE VARIABLE plReadOnlyDigger AS LOGICAL INITIAL FALSE NO-UNDO.
&ENDIF

CREATE WIDGET-POOL.

/* ***************************  Definitions  ************************** */

/* Buildnr, temp-tables and forward defs */
{ DataDigger.i }
{ resizable_dict.i } /* thanks to Sebastien Lacroix */

/* Constants for page numbers */
&GLOBAL-DEFINE PAGE-TABLES     1
&GLOBAL-DEFINE PAGE-FAVOURITES 2
&GLOBAL-DEFINE PAGE-FIELDS     3
&GLOBAL-DEFINE PAGE-INDEXES    4

/* Constants for arrows in hint frame */
&GLOBAL-DEFINE ARROW-NONE       0
&GLOBAL-DEFINE ARROW-LEFT-UP    1
&GLOBAL-DEFINE ARROW-RIGHT-UP   2
&GLOBAL-DEFINE ARROW-RIGHT-DOWN 3
&GLOBAL-DEFINE ARROW-LEFT-DOWN  4

/* TT for the generic timer OCX */
DEFINE TEMP-TABLE ttTimer NO-UNDO RCODE-INFORMATION
  FIELD cProc  AS CHARACTER
  FIELD iTime  AS INTEGER
  FIELD tNext  AS DATETIME
  INDEX idxNext IS PRIMARY tNext
  INDEX idxProc cProc.

/* TT for showing record in a new window */
DEFINE TEMP-TABLE ttView NO-UNDO RCODE-INFORMATION
  FIELD iHor   AS INTEGER
  FIELD iVer   AS INTEGER
  FIELD cValue AS CHARACTER FORMAT 'x(20)'
  .

/* TT to save widths of columns for ttView */
DEFINE TEMP-TABLE ttColumnWidth NO-UNDO RCODE-INFORMATION
  FIELD iHor   AS INTEGER
  FIELD iWidth AS INTEGER
  .

/* TT for fonts, used in checkFonts */
DEFINE TEMP-TABLE ttFont NO-UNDO RCODE-INFORMATION
  FIELD iFontNr   AS INTEGER
  FIELD cFontName AS CHARACTER
  .

/* TT for sorting combo box */
DEFINE TEMP-TABLE ttItem NO-UNDO RCODE-INFORMATION
  FIELD cItem AS CHARACTER
  INDEX iPrim IS PRIMARY cItem
  .

/* Local Variable Definitions --- */
DEFINE VARIABLE ghFirstColumn              AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghFieldMenu                AS HANDLE      NO-UNDO. /* Popup menu on brFields */
DEFINE VARIABLE gcCurrentTable             AS CHARACTER   NO-UNDO.
DEFINE VARIABLE gcCurrentDatabase          AS CHARACTER   NO-UNDO.
DEFINE VARIABLE gcFieldFilterHandles       AS CHARACTER   NO-UNDO. /* To save handles to the filter widgets */
DEFINE VARIABLE gcFieldFilterList          AS CHARACTER   NO-UNDO.
DEFINE VARIABLE gcDataBrowseColumnNames    AS CHARACTER   NO-UNDO.
DEFINE VARIABLE gcDataBrowseColumns        AS CHARACTER   NO-UNDO.
DEFINE VARIABLE gcFieldBrowseColumnHandles AS CHARACTER   NO-UNDO.
DEFINE VARIABLE gcFieldBrowseColumnNames   AS CHARACTER   NO-UNDO.
DEFINE VARIABLE gcIndexBrowseColumnHandles AS CHARACTER   NO-UNDO.
DEFINE VARIABLE gcQueryEditorState         AS CHARACTER   NO-UNDO.
DEFINE VARIABLE ghDataBrowse               AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghDataBuffer               AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghDataQuery                AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghTableQuery               AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghTableBuffer              AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghLockTable                AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghFieldBrowse              AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghLastFilterField          AS HANDLE      NO-UNDO.
DEFINE VARIABLE ghLastIndexFilter          AS HANDLE      NO-UNDO.
DEFINE VARIABLE gcLastDataField            AS CHARACTER   NO-UNDO.
DEFINE VARIABLE ghNameColumn               AS HANDLE      NO-UNDO.
DEFINE VARIABLE giCurrentPage              AS INTEGER     NO-UNDO. /* 1=fields 2=indexes */.
DEFINE VARIABLE giQueryPointer             AS INTEGER     NO-UNDO.
DEFINE VARIABLE giWindowLock               AS INTEGER     NO-UNDO.
DEFINE VARIABLE glRowEditActive            AS LOGICAL     NO-UNDO.
DEFINE VARIABLE glFormatChanged            AS LOGICAL     NO-UNDO. /* When user changes a format */
DEFINE VARIABLE glHintCancelled            AS LOGICAL     NO-UNDO. /* When user presses ESC during hint */
DEFINE VARIABLE giMaxQueryTime             AS INTEGER     NO-UNDO.
DEFINE VARIABLE ghOverlayField             AS HANDLE      NO-UNDO EXTENT 500.
DEFINE VARIABLE gcRecordMode               AS CHARACTER   NO-UNDO.
DEFINE VARIABLE giDataOddRowColor          AS INTEGER     NO-UNDO EXTENT 2.
DEFINE VARIABLE giDataEvenRowColor         AS INTEGER     NO-UNDO EXTENT 2.
DEFINE VARIABLE giDefaultFont              AS INTEGER     NO-UNDO.
DEFINE VARIABLE giFixedFont                AS INTEGER     NO-UNDO.
DEFINE VARIABLE giMaxColumns               AS INTEGER     NO-UNDO.
DEFINE VARIABLE giMaxExtent                AS INTEGER     NO-UNDO.
DEFINE VARIABLE giMaxFilterHistory         AS INTEGER     NO-UNDO.
DEFINE VARIABLE glDebugMode                AS LOGICAL     NO-UNDO INITIAL FALSE.
DEFINE VARIABLE giLastDataColumnX          AS INTEGER     NO-UNDO.
DEFINE VARIABLE glShowFavourites           AS LOGICAL     NO-UNDO. /* show table list of Favourite tables */
DEFINE VARIABLE glUseTimer                 AS LOGICAL     NO-UNDO. /* use PSTimer? */

DEFINE VARIABLE gcPreviousValues           AS CHARACTER   NO-UNDO. /* used in DataRowDisplay for row coloring */
DEFINE VARIABLE glUseEvenRowColorSet       AS LOGICAL     NO-UNDO. /* used in DataRowDisplay for row coloring */

/* Procedure used in update check */
PROCEDURE URLDownloadToFileA EXTERNAL "URLMON.DLL" :
  DEFINE INPUT PARAMETER pCaller    AS LONG.
  DEFINE INPUT PARAMETER szURL      AS CHARACTER.
  DEFINE INPUT PARAMETER szFilename AS CHARACTER.
  DEFINE INPUT PARAMETER dwReserved AS LONG.
  DEFINE INPUT PARAMETER lpfnCB     AS LONG.
END PROCEDURE. /* URLDownloadToFileA */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME frMain
&Scoped-define BROWSE-NAME brFields

/* Internal Tables (found by Frame, Query & Browse Queries)             */
&Scoped-define INTERNAL-TABLES ttField ttIndex ttTable

/* Definitions for BROWSE brFields                                      */
&Scoped-define FIELDS-IN-QUERY-brFields ttField.lShow ttField.iOrder ttField.cFieldName (IF ttField.iExtent > 0 THEN SUBSTITUTE('&1[&2]', ttField.cDataType, ttField.iExtent) ELSE ttField.cDataType ) @ ttField.cDataType ttField.cFormat ttField.cLabel /* Extra fields as per v19 */ ttField.cInitial ttField.cColLabel ttField.lMandatory ttField.iExtent ttField.iDecimals ttField.iFieldRpos ttField.cValExp ttField.cValMsg ttField.cHelp ttField.cDesc ttField.cViewAs   
&Scoped-define ENABLED-FIELDS-IN-QUERY-brFields ttField.lShow  ttField.cFormat   
&Scoped-define ENABLED-TABLES-IN-QUERY-brFields ttField
&Scoped-define FIRST-ENABLED-TABLE-IN-QUERY-brFields ttField
&Scoped-define SELF-NAME brFields
&Scoped-define QUERY-STRING-brFields FOR EACH ttField
&Scoped-define OPEN-QUERY-brFields OPEN QUERY {&SELF-NAME} FOR EACH ttField.
&Scoped-define TABLES-IN-QUERY-brFields ttField
&Scoped-define FIRST-TABLE-IN-QUERY-brFields ttField


/* Definitions for BROWSE brIndexes                                     */
&Scoped-define FIELDS-IN-QUERY-brIndexes cIndexName cIndexFlags cIndexFields   
&Scoped-define ENABLED-FIELDS-IN-QUERY-brIndexes   
&Scoped-define SELF-NAME brIndexes
&Scoped-define QUERY-STRING-brIndexes FOR EACH ttIndex
&Scoped-define OPEN-QUERY-brIndexes OPEN QUERY {&SELF-NAME} FOR EACH ttIndex.
&Scoped-define TABLES-IN-QUERY-brIndexes ttIndex
&Scoped-define FIRST-TABLE-IN-QUERY-brIndexes ttIndex


/* Definitions for BROWSE brTables                                      */
&Scoped-define FIELDS-IN-QUERY-brTables ttTable.cTableName ttTable.cDatabase ttTable.iNumQueries   
&Scoped-define ENABLED-FIELDS-IN-QUERY-brTables   
&Scoped-define SELF-NAME brTables
&Scoped-define QUERY-STRING-brTables FOR EACH ttTable
&Scoped-define OPEN-QUERY-brTables OPEN QUERY {&SELF-NAME} FOR EACH ttTable.
&Scoped-define TABLES-IN-QUERY-brTables ttTable
&Scoped-define FIRST-TABLE-IN-QUERY-brTables ttTable


/* Definitions for FRAME frMain                                         */
&Scoped-define OPEN-BROWSERS-IN-QUERY-frMain ~
    ~{&OPEN-QUERY-brFields}~
    ~{&OPEN-QUERY-brIndexes}

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS rctQuery btnTools rctEdit fiTableFilter ~
cbDatabaseFilter btnClearTableFilter btnTableFilter tgSelAll ~
btnClearFieldFilter fiIndexNameFilter fiFlagsFilter fiFieldsFilter ~
btnClearIndexFilter tgDebugMode brTables brFields btnMoveTop brIndexes ~
btnMoveUp btnReset btnMoveDown btnMoveBottom fiTableDesc btnWhere btnClear ~
btnQueries btnFavourite btnClipboard ficWhere btnNextQuery btnPrevQuery ~
btnDump btnLoad btnTabFavourites btnTabFields btnTabIndexes btnTabTables ~
btnDelete btnResizeVer btnClone btnView btnAdd btnEdit fiFeedback 
&Scoped-Define DISPLAYED-OBJECTS fiTableFilter cbDatabaseFilter tgSelAll ~
fiIndexNameFilter fiFlagsFilter fiFieldsFilter fiTableDesc ficWhere ~
fiFeedback 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */
&Scoped-define List-1 btnBegins rctQueryButtons cbAndOr cbFields cbOperator ~
ficValue btnInsert btnOr btnAnd btnBracket btnContains btnEq btnGT btnLT ~
btnMatches btnNE btnQt btnToday 
&Scoped-define List-2 rcFieldFilter tgSelAll btnClearFieldFilter brFields ~
btnMoveTop btnMoveUp btnReset btnMoveDown btnMoveBottom 
&Scoped-define List-3 rcIndexFilter fiIndexNameFilter fiFlagsFilter ~
fiFieldsFilter btnClearIndexFilter brIndexes 

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD createMenu C-Win 
FUNCTION createMenu RETURNS HANDLE
  ( phParent AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD createMenuItem C-Win 
FUNCTION createMenuItem RETURNS HANDLE
  ( phMenu    AS HANDLE
  , pcType    AS CHARACTER
  , pcLabel   AS CHARACTER
  , pcName    AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getActiveQueryEditor C-Win 
FUNCTION getActiveQueryEditor RETURNS HANDLE
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getDroppedFiles C-Win 
FUNCTION getDroppedFiles RETURNS CHARACTER
  ( phDropTarget AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getFieldList C-Win 
FUNCTION getFieldList RETURNS CHARACTER
  ( pcSortBy AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getQueryFromFields C-Win 
FUNCTION getQueryFromFields RETURNS CHARACTER
  ( INPUT pcFieldList AS CHARACTER ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getSelectedFields C-Win 
FUNCTION getSelectedFields RETURNS CHARACTER
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getSelectedText C-Win 
FUNCTION getSelectedText RETURNS CHARACTER
  ( INPUT hWidget AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD killMenu C-Win 
FUNCTION killMenu RETURNS LOGICAL
  ( phMenu AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD saveSelectedFields C-Win 
FUNCTION saveSelectedFields RETURNS CHARACTER
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setCurrentTable C-Win 
FUNCTION setCurrentTable RETURNS LOGICAL
  ( pcTableName AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setDebugMode C-Win 
FUNCTION setDebugMode RETURNS LOGICAL
  ( plDebugMode AS LOGICAL )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setQuery C-Win 
FUNCTION setQuery RETURNS LOGICAL
  ( piPointerChange AS INTEGER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setQueryEditor C-Win 
FUNCTION setQueryEditor RETURNS LOGICAL
  ( pcQueryEditorState AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setUpdatePanel C-Win 
FUNCTION setUpdatePanel RETURNS LOGICAL
  ( INPUT pcMode AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setWindowFreeze C-Win 
FUNCTION setWindowFreeze RETURNS LOGICAL
  ( plWindowsLocked AS LOGICAL )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD trimList C-Win 
FUNCTION trimList RETURNS CHARACTER
  ( pcList  AS CHARACTER
  , pcSep   AS CHARACTER
  , piItems AS INTEGER
  ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR C-Win AS WIDGET-HANDLE NO-UNDO.

/* Menu Definitions                                                     */
DEFINE MENU POPUP-MENU-brTables 
       MENU-ITEM m_Quick_Connect LABEL "Quick Connect" 
       MENU-ITEM m_Disconnect   LABEL "&Disconnect"   
       MENU-ITEM m_Manage_Connections LABEL "Manage Connections"
       MENU-ITEM m_Toggle_as_favourite LABEL "Set / Unset as &Favourite"
       MENU-ITEM m_Dump_table_DF LABEL "&Dump definitions"
       MENU-ITEM m_Clone_this_Database LABEL "&Clone this Database"
       RULE.

DEFINE MENU POPUP-MENU-btnView 
       MENU-ITEM m_View_as_text LABEL "View as TEXT"  
       MENU-ITEM m_View_as_HTML LABEL "View as HTML"  
       MENU-ITEM m_View_as_Excel LABEL "View as Excel" .


/* Definitions of handles for OCX Containers                            */
DEFINE VARIABLE CtrlFrame AS WIDGET-HANDLE NO-UNDO.
DEFINE VARIABLE chCtrlFrame AS COMPONENT-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnClearDataFilter 
     LABEL "C" 
     SIZE-PIXELS 20 BY 21 TOOLTIP "clear all filters #(SHIFT-DEL)".

DEFINE BUTTON btnDataSort 
     LABEL "S" 
     SIZE-PIXELS 15 BY 21 TOOLTIP "set sorting".

DEFINE VARIABLE fiNumRecords AS CHARACTER FORMAT "X(256)":U 
      VIEW-AS TEXT 
     SIZE-PIXELS 90 BY 13 TOOLTIP "nr of results of the query, double click to fetch all records" NO-UNDO.

DEFINE VARIABLE fiNumSelected AS CHARACTER FORMAT "X(256)":U 
      VIEW-AS TEXT 
     SIZE-PIXELS 29 BY 13 TOOLTIP "nr of selected rows" NO-UNDO.

DEFINE RECTANGLE rctData
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 785 BY 205.

DEFINE RECTANGLE rctDataFilter
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 765 BY 28
     BGCOLOR 12 .

DEFINE BUTTON btGotIt 
     LABEL "I &Got it" 
     SIZE-PIXELS 75 BY 24.

DEFINE VARIABLE edHint AS CHARACTER 
     VIEW-AS EDITOR NO-BOX
     SIZE-PIXELS 160 BY 75
     BGCOLOR 14 FGCOLOR 9  NO-UNDO.

DEFINE IMAGE imgArrow
     FILENAME "adeicon/blank":U TRANSPARENT
     SIZE-PIXELS 32 BY 32.

DEFINE BUTTON btnAdd  NO-FOCUS FLAT-BUTTON
     LABEL "&Add" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "add a record #(INS)".

DEFINE BUTTON btnClear 
     LABEL "&C" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "clear the where field #(SHIFT-DEL)".

DEFINE BUTTON btnClearFieldFilter 
     LABEL "C" 
     CONTEXT-HELP-ID 280
     SIZE-PIXELS 20 BY 21 TOOLTIP "clear all filters #(SHIFT-DEL)".

DEFINE BUTTON btnClearIndexFilter 
     LABEL "C" 
     CONTEXT-HELP-ID 960
     SIZE-PIXELS 20 BY 21 TOOLTIP "clear all filters #(SHIFT-DEL)".

DEFINE BUTTON btnClearTableFilter 
     LABEL "C" 
     CONTEXT-HELP-ID 950
     SIZE-PIXELS 20 BY 21 TOOLTIP "clear all filters #(SHIFT-DEL)".

DEFINE BUTTON btnClipboard 
     LABEL "Cp" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "copy the expression to the clipboard #(CTRL-C)".

DEFINE BUTTON btnClone  NO-FOCUS FLAT-BUTTON
     LABEL "Cl&one" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "clone focused record and edit #(ALT-O)".

DEFINE BUTTON btnDelete  NO-FOCUS FLAT-BUTTON
     LABEL "Del" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "delete the selected records #(DEL)".

DEFINE BUTTON btnDump  NO-FOCUS FLAT-BUTTON
     LABEL "&Save" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "save data to disk #(CTRL-S)".

DEFINE BUTTON btnEdit  NO-FOCUS FLAT-BUTTON
     LABEL "&Edit" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "edit the selected records #(ALT-E)".

DEFINE BUTTON btnFavourite  NO-FOCUS FLAT-BUTTON
     LABEL "F" 
     SIZE-PIXELS 20 BY 20 TOOLTIP "toggle as favourite".

DEFINE BUTTON btnLoad  NO-FOCUS FLAT-BUTTON
     LABEL "&Load" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "load data #(CTRL-L)".

DEFINE BUTTON btnMoveBottom 
     LABEL "Btm" 
     SIZE-PIXELS 23 BY 23 TOOLTIP "move selected field to bottom #(CTRL-SHIFT-DOWN)".

DEFINE BUTTON btnMoveDown 
     LABEL "Dn" 
     SIZE-PIXELS 23 BY 23 TOOLTIP "move selected field down #(CTRL-DOWN)".

DEFINE BUTTON btnMoveTop 
     LABEL "Top" 
     SIZE-PIXELS 23 BY 23 TOOLTIP "move selected field to top #(CTRL-SHIFT-UP)".

DEFINE BUTTON btnMoveUp 
     LABEL "Up" 
     SIZE-PIXELS 23 BY 23 TOOLTIP "move selected field up #(CTRL-UP)".

DEFINE BUTTON btnNextQuery  NO-FOCUS FLAT-BUTTON
     LABEL "->" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "next query".

DEFINE BUTTON btnPrevQuery  NO-FOCUS FLAT-BUTTON
     LABEL "<-" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "previous query".

DEFINE BUTTON btnQueries 
     LABEL "&Q" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "show previous queries on this table #(ALT-Q)".

DEFINE BUTTON btnReset 
     LABEL "R" 
     SIZE-PIXELS 23 BY 23 TOOLTIP "reset default ordering #(CTRL-SHIFT-HOME)".

DEFINE BUTTON btnResizeVer  NO-FOCUS FLAT-BUTTON
     LABEL "||||||||||||||||||||||||||" 
     SIZE 156 BY .24 TOOLTIP "drag me up and down".

DEFINE BUTTON btnTabFavourites  NO-FOCUS FLAT-BUTTON
     LABEL "Fav" 
     CONTEXT-HELP-ID 270
     SIZE-PIXELS 25 BY 77 TOOLTIP "show favorites #(CTRL-2)".

DEFINE BUTTON btnTabFields  NO-FOCUS FLAT-BUTTON
     LABEL "Fld" 
     CONTEXT-HELP-ID 270
     SIZE-PIXELS 25 BY 77 TOOLTIP "show fields #(CTRL-3)".

DEFINE BUTTON btnTabIndexes  NO-FOCUS FLAT-BUTTON
     LABEL "Idx" 
     CONTEXT-HELP-ID 270
     SIZE-PIXELS 25 BY 77 TOOLTIP "show indexes #(CTRL-4)".

DEFINE BUTTON btnTableFilter 
     LABEL "Y" 
     CONTEXT-HELP-ID 950
     SIZE-PIXELS 20 BY 21 TOOLTIP "press arrow-down for extra filter options #(CTRL-DOWN)".

DEFINE BUTTON btnTabTables  NO-FOCUS FLAT-BUTTON
     LABEL "Tbl" 
     CONTEXT-HELP-ID 270
     SIZE-PIXELS 25 BY 77 TOOLTIP "show tables #(CTRL-1)".

DEFINE BUTTON btnTools  NO-FOCUS FLAT-BUTTON
     LABEL "Tools" 
     SIZE-PIXELS 21 BY 21 TOOLTIP "tools and settings #(CTRL-T)".

DEFINE BUTTON btnView  NO-FOCUS FLAT-BUTTON
     LABEL "&View" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "view selected records  #(ENTER) #right click to set type of view".

DEFINE BUTTON btnViewData 
     LABEL "->" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "execute the query #(CTRL-ENTER)".

DEFINE BUTTON btnWhere 
     LABEL "&Where" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "show expanded query editor  #(CTRL-ALT-W)".

DEFINE VARIABLE cbDatabaseFilter AS CHARACTER FORMAT "X(256)":U 
     CONTEXT-HELP-ID 950
     VIEW-AS COMBO-BOX SORT INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN-LIST
     SIZE-PIXELS 59 BY 21 TOOLTIP "filter on database" NO-UNDO.

DEFINE VARIABLE ficWhere AS CHARACTER 
     CONTEXT-HELP-ID 110
     VIEW-AS EDITOR NO-WORD-WRAP
     SIZE-PIXELS 595 BY 21 TOOLTIP "query on your table  #(ALT-W)"
     FONT 2 NO-UNDO.

DEFINE VARIABLE fiFeedback AS CHARACTER FORMAT "X(256)":U INITIAL "Got a question or feedback?" 
      VIEW-AS TEXT 
     SIZE-PIXELS 145 BY 13 TOOLTIP "click me!"
     FGCOLOR 7  NO-UNDO.

DEFINE VARIABLE fiFieldsFilter AS CHARACTER FORMAT "X(256)":U INITIAL "Fields" 
     CONTEXT-HELP-ID 960
     VIEW-AS FILL-IN 
     SIZE-PIXELS 140 BY 21 TOOLTIP "filter indexes on used fields"
     FGCOLOR 7  NO-UNDO.

DEFINE VARIABLE fiFlagsFilter AS CHARACTER FORMAT "X(256)":U INITIAL "Flags" 
     CONTEXT-HELP-ID 960
     VIEW-AS FILL-IN 
     SIZE-PIXELS 55 BY 21 TOOLTIP "filter indexes on index flags"
     FGCOLOR 7  NO-UNDO.

DEFINE VARIABLE fiIndexNameFilter AS CHARACTER FORMAT "X(256)":U INITIAL "Index Name" 
     CONTEXT-HELP-ID 960
     VIEW-AS FILL-IN 
     SIZE-PIXELS 75 BY 21 TOOLTIP "filter indexes on name"
     FGCOLOR 7  NO-UNDO.

DEFINE VARIABLE fiTableDesc AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 200 BY 20 NO-UNDO.

DEFINE VARIABLE fiTableFilter AS CHARACTER FORMAT "X(256)":U INITIAL "Table filter" 
     CONTEXT-HELP-ID 950
     VIEW-AS FILL-IN 
     SIZE-PIXELS 70 BY 21 TOOLTIP "filter on table names  #(ALT-T)"
     FGCOLOR 7  NO-UNDO.

DEFINE VARIABLE fiWarning AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 45 BY 21
     BGCOLOR 14 FGCOLOR 12  NO-UNDO.

DEFINE RECTANGLE rcFieldFilter
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 466 BY 237
     BGCOLOR 12 FGCOLOR 12 .

DEFINE RECTANGLE rcIndexFilter
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 315 BY 141
     BGCOLOR 12 FGCOLOR 12 .

DEFINE RECTANGLE rcTableFilter
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 226 BY 236
     BGCOLOR 12 .

DEFINE RECTANGLE rctEdit
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 265 BY 35.

DEFINE RECTANGLE rctQuery
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 789 BY 290
     BGCOLOR 18 .

DEFINE VARIABLE tgDebugMode AS LOGICAL INITIAL yes 
     LABEL "" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 15 BY 13 TOOLTIP "debugging mode".

DEFINE VARIABLE tgSelAll AS LOGICAL INITIAL yes 
     LABEL "" 
     CONTEXT-HELP-ID 280
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 14 BY 15 TOOLTIP "toggle to (de)select all fields" NO-UNDO.

DEFINE BUTTON btnAbout 
     LABEL "Que" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "about the DataDigger #(CTRL-SHIFT-A)".

DEFINE BUTTON btnAbout-txt  NO-FOCUS FLAT-BUTTON
     LABEL "About DataDigger" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "about the DataDigger #(CTRL-SHIFT-A)".

DEFINE BUTTON btnConnections 
     LABEL "Co&n" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "connections #(CTRL-SHIFT-N)".

DEFINE BUTTON btnConnections-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Co&nnections" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "connections #(CTRL-SHIFT-N)".

DEFINE BUTTON btnDataAdmin 
     LABEL "&ADM" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "Data Administration".

DEFINE BUTTON btnDataAdmin-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Data Administration" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "Data Administration".

DEFINE BUTTON btnDataDigger 
     LABEL "DD" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "start a new DataDigger window #(ALT-D)".

DEFINE BUTTON btnDataDigger-txt  NO-FOCUS FLAT-BUTTON
     LABEL "New &Window" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "start a new DataDigger window #(ALT-D)".

DEFINE BUTTON btnDict 
     LABEL "&DD" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "start the Data Dictionary #(CTRL-SHIFT-D)".

DEFINE BUTTON btnDict-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Data &Dictionary" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "start the Data Dictionary #(CTRL-SHIFT-D)".

DEFINE BUTTON btnHelp 
     LABEL "Help" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "get help about the DataDigger window".

DEFINE BUTTON btnHelp-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Help" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "show quick intro".

DEFINE BUTTON btnProcEdit 
     LABEL "&Ed" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "start the Procedure Editor #(SHIFT-F3)".

DEFINE BUTTON btnProcEdit-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Procedure &Editor" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "start the Procedure Editor #(SHIFT-F3)".

DEFINE BUTTON btnQueries-3 
     LABEL "&Q" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "show previous queries #(CTRL-SHIFT-Q)".

DEFINE BUTTON btnQueries-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Manage &Queries" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "show previous queries on this table #(CTRL-SHIFT-Q)".

DEFINE BUTTON btnQueryTester 
     LABEL "Q&T" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "start the query tester #(CTRL-Q)".

DEFINE BUTTON btnQueryTester-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Query &Tester" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "start the query tester #(CTRL-SHIFT-T)".

DEFINE BUTTON btnSettings 
     LABEL "&Set" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "edit your settings file #(CTRL-ALT-S)".

DEFINE BUTTON btnSettings-txt  NO-FOCUS FLAT-BUTTON
     LABEL "&Settings" 
     SIZE-PIXELS 155 BY 30 TOOLTIP "edit your settings file #(CTRL-ALT-S)".

DEFINE BUTTON btnAnd  NO-FOCUS
     LABEL "and" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 40 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnBegins  NO-FOCUS
     LABEL "begins" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 80 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnBracket  NO-FOCUS
     LABEL "()" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 40 BY 21 TOOLTIP "insert this text into the where field"
     FONT 0.

DEFINE BUTTON btnCancel-2 DEFAULT 
     LABEL "Cancel" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON btnClear-2 
     LABEL "&C" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 20 BY 23 TOOLTIP "clear the where field".

DEFINE BUTTON btnClipboard-2 
     LABEL "Cp" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 20 BY 23 TOOLTIP "copy the expression to the clipboard".

DEFINE BUTTON btnContains  NO-FOCUS
     LABEL "contains" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 80 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnEq  NO-FOCUS
     LABEL "=" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 40 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnGT  NO-FOCUS
     LABEL ">" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 40 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnInsert 
     LABEL "+" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 20 BY 23 TOOLTIP "insert the expression into the where field".

DEFINE BUTTON btnLT  NO-FOCUS
     LABEL "<" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 40 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnMatches  NO-FOCUS
     LABEL "matches" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 80 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnNE  NO-FOCUS
     LABEL "<>" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 40 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnOK AUTO-GO DEFAULT 
     LABEL "OK" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON btnOr  NO-FOCUS
     LABEL "or" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 40 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnQt  NO-FOCUS
     LABEL "~"~"" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 40 BY 21 TOOLTIP "insert this text into the where field"
     FONT 0.

DEFINE BUTTON btnQueries-2 
     LABEL "&Q" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 20 BY 23 TOOLTIP "show previous queries on this table".

DEFINE BUTTON btnToday  NO-FOCUS
     LABEL "today" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 80 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnViewData-2 
     LABEL "->" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 20 BY 23 TOOLTIP "execute the query".

DEFINE VARIABLE cbAndOr AS CHARACTER FORMAT "X(256)":U 
     LABEL "&Where" 
     CONTEXT-HELP-ID 1050
     VIEW-AS COMBO-BOX INNER-LINES 5
     LIST-ITEMS "","AND","OR" 
     DROP-DOWN-LIST
     SIZE-PIXELS 50 BY 21 TOOLTIP "preceding AND or OR for the expression" NO-UNDO.

DEFINE VARIABLE cbFields AS CHARACTER FORMAT "X(256)":U 
     CONTEXT-HELP-ID 1050
     VIEW-AS COMBO-BOX INNER-LINES 10
     DROP-DOWN-LIST
     SIZE-PIXELS 186 BY 21 TOOLTIP "field used in the expression"
     FONT 2 NO-UNDO.

DEFINE VARIABLE cbOperator AS CHARACTER FORMAT "X(256)":U 
     CONTEXT-HELP-ID 1050
     VIEW-AS COMBO-BOX INNER-LINES 10
     LIST-ITEMS "","=","<>",">",">=","<","<=","begins","matches","contains" 
     DROP-DOWN-LIST
     SIZE-PIXELS 85 BY 21 TOOLTIP "operator used in the expression"
     FONT 2 NO-UNDO.

DEFINE VARIABLE ficWhere2 AS CHARACTER 
     CONTEXT-HELP-ID 1050
     VIEW-AS EDITOR SCROLLBAR-VERTICAL
     SIZE-PIXELS 501 BY 175 TOOLTIP "alt-cursor-up / down to view/hide query editor"
     FONT 2 NO-UNDO.

DEFINE VARIABLE ficValue AS CHARACTER FORMAT "X(256)":U 
     CONTEXT-HELP-ID 1050
     VIEW-AS FILL-IN 
     SIZE-PIXELS 210 BY 23 TOOLTIP "the literal value for the expression"
     FONT 2 NO-UNDO.

DEFINE RECTANGLE rctQueryButtons
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 610 BY 190.

/* Query definitions                                                    */
&ANALYZE-SUSPEND
DEFINE QUERY brFields FOR 
      ttField SCROLLING.

DEFINE QUERY brIndexes FOR 
      ttIndex SCROLLING.

DEFINE QUERY brTables FOR 
      ttTable SCROLLING.
&ANALYZE-RESUME

/* Browse definitions                                                   */
DEFINE BROWSE brFields
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _DISPLAY-FIELDS brFields C-Win _FREEFORM
  QUERY brFields DISPLAY
      ttField.lShow VIEW-AS TOGGLE-BOX
  ttField.iOrder
  ttField.cFieldName
  (IF ttField.iExtent > 0
    THEN SUBSTITUTE('&1[&2]', ttField.cDataType, ttField.iExtent)
    ELSE ttField.cDataType ) @ ttField.cDataType
  ttField.cFormat
  ttField.cLabel

  /* Extra fields as per v19 */
  ttField.cInitial
  ttField.cColLabel
  ttField.lMandatory
  ttField.iExtent
  ttField.iDecimals
  ttField.iFieldRpos
  ttField.cValExp
  ttField.cValMsg
  ttField.cHelp
  ttField.cDesc
  ttField.cViewAs

  ENABLE
  ttField.lShow
  ttField.cFormat
/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
    WITH NO-ROW-MARKERS SEPARATORS NO-VALIDATE
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 92 BY 11
          &ELSE SIZE-PIXELS 460 BY 231 &ENDIF FIT-LAST-COLUMN TOOLTIP "fields of selected table"
         CONTEXT-HELP-ID 80.

DEFINE BROWSE brIndexes
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _DISPLAY-FIELDS brIndexes C-Win _FREEFORM
  QUERY brIndexes DISPLAY
      cIndexName
cIndexFlags
cIndexFields
/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
    WITH NO-ROW-MARKERS SEPARATORS NO-VALIDATE
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 62 BY 6
          &ELSE SIZE-PIXELS 308 BY 132 &ENDIF FIT-LAST-COLUMN TOOLTIP "indexes of the table"
         CONTEXT-HELP-ID 90.

DEFINE BROWSE brTables
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _DISPLAY-FIELDS brTables C-Win _FREEFORM
  QUERY brTables DISPLAY
      ttTable.cTableName
ttTable.cDatabase
ttTable.iNumQueries
/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
    WITH NO-ROW-MARKERS
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 44 BY 10
          &ELSE SIZE-PIXELS 220 BY 211 &ENDIF FIT-LAST-COLUMN TOOLTIP "(F) to set/unset as favourite"
         CONTEXT-HELP-ID 70.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frMain
     btnTools AT Y 4 X 2 WIDGET-ID 264
     fiTableFilter AT Y 4 X 38 NO-LABEL
     cbDatabaseFilter AT Y 4 X 99 COLON-ALIGNED NO-LABEL
     btnClearTableFilter AT Y 4 X 219 WIDGET-ID 222
     btnTableFilter AT Y 4 X 239 WIDGET-ID 38
     tgSelAll AT Y 5 X 315 WIDGET-ID 6
     btnClearFieldFilter AT Y 5 X 735 WIDGET-ID 232
     fiIndexNameFilter AT Y 5 X 785 COLON-ALIGNED NO-LABEL WIDGET-ID 168
     fiFlagsFilter AT Y 5 X 860 COLON-ALIGNED NO-LABEL WIDGET-ID 164
     fiFieldsFilter AT Y 5 X 915 COLON-ALIGNED NO-LABEL WIDGET-ID 166
     btnClearIndexFilter AT Y 5 X 1065 WIDGET-ID 160
     tgDebugMode AT Y 25 X 5 WIDGET-ID 238
     brTables AT Y 27 X 38 WIDGET-ID 300
     brFields AT Y 27 X 295 WIDGET-ID 100
     btnMoveTop AT Y 28 X 760 WIDGET-ID 198
     brIndexes AT Y 28 X 799 WIDGET-ID 200
     btnMoveUp AT Y 50 X 760 WIDGET-ID 192
     btnReset AT Y 72 X 760 WIDGET-ID 196
     btnMoveDown AT Y 94 X 760 WIDGET-ID 194
     btnMoveBottom AT Y 116 X 760 WIDGET-ID 200
     fiTableDesc AT Y 238 X 38 NO-LABEL WIDGET-ID 90
     btnWhere AT Y 265 X 653 WIDGET-ID 236
     btnViewData AT Y 265 X 675
     btnClear AT Y 265 X 695 WIDGET-ID 30
     btnQueries AT Y 265 X 715 WIDGET-ID 190
     btnFavourite AT Y 239 X 238 WIDGET-ID 310
     btnClipboard AT Y 265 X 735 WIDGET-ID 178
     ficWhere AT Y 266 X 50 NO-LABEL
     btnNextQuery AT Y 265 X 27 WIDGET-ID 314
     btnPrevQuery AT Y 265 X 6 WIDGET-ID 312
     fiWarning AT Y 520 X 450 COLON-ALIGNED NO-LABEL WIDGET-ID 172
     btnDump AT Y 520 X 145
     btnLoad AT Y 520 X 195 WIDGET-ID 224
     btnTabFavourites AT Y 122 X 13 WIDGET-ID 302
     btnTabFields AT Y 45 X 271 WIDGET-ID 156
     btnTabIndexes AT Y 122 X 271 WIDGET-ID 158
     btnTabTables AT Y 45 X 13 WIDGET-ID 300
     btnDelete AT Y 520 X 250
     btnResizeVer AT ROW 13.38 COL 1.6 WIDGET-ID 274
     btnClone AT Y 520 X 50 WIDGET-ID 276
     btnView AT Y 520 X 170 WIDGET-ID 4
     btnAdd AT Y 520 X 25
     btnEdit AT Y 520 X 75
     fiFeedback AT Y 520 X 634 COLON-ALIGNED NO-LABEL WIDGET-ID 308
     rctQuery AT Y 2 X 2
     rctEdit AT Y 515 X 20
     rcTableFilter AT Y 24 X 35 WIDGET-ID 254
     rcFieldFilter AT Y 24 X 292 WIDGET-ID 256
     rcIndexFilter AT Y 24 X 795 WIDGET-ID 258
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 0 Y 0
         SIZE-PIXELS 1494 BY 675 DROP-TARGET.

DEFINE FRAME frWhere
     btnBegins AT Y 123 X 17 WIDGET-ID 74
     cbAndOr AT Y 5 X 46 COLON-ALIGNED WIDGET-ID 10
     cbFields AT Y 5 X 100 COLON-ALIGNED NO-LABEL WIDGET-ID 12
     cbOperator AT Y 5 X 286 COLON-ALIGNED NO-LABEL WIDGET-ID 14
     ficValue AT Y 5 X 371 COLON-ALIGNED NO-LABEL WIDGET-ID 16
     btnInsert AT Y 5 X 595 WIDGET-ID 18
     ficWhere2 AT Y 35 X 110 NO-LABEL WIDGET-ID 130
     btnViewData-2 AT Y 70 X 623 WIDGET-ID 216
     btnClear-2 AT Y 95 X 623 WIDGET-ID 30
     btnQueries-2 AT Y 120 X 623 WIDGET-ID 190
     btnClipboard-2 AT Y 145 X 623 WIDGET-ID 178
     btnOK AT Y 230 X 460 WIDGET-ID 132
     btnCancel-2 AT Y 230 X 540 WIDGET-ID 134
     btnOr AT Y 101 X 57 WIDGET-ID 24
     btnAnd AT Y 101 X 17 WIDGET-ID 22
     btnBracket AT Y 79 X 17 WIDGET-ID 28
     btnContains AT Y 145 X 17 WIDGET-ID 116
     btnEq AT Y 35 X 17 WIDGET-ID 62
     btnGT AT Y 57 X 57 WIDGET-ID 66
     btnLT AT Y 57 X 17 WIDGET-ID 64
     btnMatches AT Y 167 X 17 WIDGET-ID 114
     btnNE AT Y 35 X 57 WIDGET-ID 68
     btnQt AT Y 79 X 57 WIDGET-ID 72
     btnToday AT Y 189 X 17 WIDGET-ID 122
     "CTRL-ALT-W also opens this window" VIEW-AS TEXT
          SIZE-PIXELS 250 BY 20 AT Y 235 X 10 WIDGET-ID 218
          FGCOLOR 7 
     rctQueryButtons AT Y 30 X 5 WIDGET-ID 128
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS TOP-ONLY NO-UNDERLINE THREE-D 
         AT X 799 Y 370
         SIZE-PIXELS 656 BY 285
         TITLE "Query Editor"
         DEFAULT-BUTTON btnOK WIDGET-ID 400.

DEFINE FRAME frHint
     edHint AT Y 0 X 35 NO-LABEL WIDGET-ID 2
     btGotIt AT Y 80 X 70 WIDGET-ID 4
     imgArrow AT Y 0 X 0 WIDGET-ID 10
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS TOP-ONLY NO-UNDERLINE THREE-D 
         AT X 1194 Y 242
         SIZE-PIXELS 205 BY 110
         BGCOLOR 14  WIDGET-ID 600.

DEFINE FRAME frData
     btnDataSort AT Y 5 X 15 WIDGET-ID 300
     btnClearDataFilter AT Y 5 X 755 WIDGET-ID 76
     fiNumSelected AT Y 198 X 636 COLON-ALIGNED NO-LABEL WIDGET-ID 298
     fiNumRecords AT Y 198 X 665 COLON-ALIGNED NO-LABEL WIDGET-ID 210
     rctData AT Y 0 X 0 WIDGET-ID 272
     rctDataFilter AT Y 1 X 12 WIDGET-ID 296
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT COL 1 ROW 15.05
         SIZE 158 BY 10.24 WIDGET-ID 700.

DEFINE FRAME frSettings
     btnSettings-txt AT Y 32 X 30 WIDGET-ID 200
     btnDataDigger AT Y 0 X 0 WIDGET-ID 126
     btnConnections AT Y 0 X 186 WIDGET-ID 212
     btnSettings AT Y 32 X 0 WIDGET-ID 210
     btnProcEdit AT Y 32 X 186 WIDGET-ID 228
     btnDict AT Y 64 X 0 WIDGET-ID 224
     btnDataAdmin AT Y 64 X 186 WIDGET-ID 214
     btnQueries-3 AT Y 96 X 0 WIDGET-ID 190
     btnQueryTester AT Y 96 X 186 WIDGET-ID 232
     btnHelp AT Y 128 X 0 WIDGET-ID 260
     btnAbout AT Y 128 X 186 WIDGET-ID 196
     btnAbout-txt AT Y 128 X 216 WIDGET-ID 208
     btnConnections-txt AT Y 0 X 216 WIDGET-ID 202
     btnDataAdmin-txt AT Y 64 X 216 WIDGET-ID 206
     btnDataDigger-txt AT Y 0 X 30 WIDGET-ID 236
     btnDict-txt AT Y 64 X 30 WIDGET-ID 226
     btnHelp-txt AT Y 128 X 30 WIDGET-ID 262
     btnProcEdit-txt AT Y 32 X 216 WIDGET-ID 230
     btnQueries-txt AT Y 96 X 30 WIDGET-ID 204
     btnQueryTester-txt AT Y 96 X 216 WIDGET-ID 234
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT COL 160.6 ROW 9.48 SCROLLABLE 
         BGCOLOR 15  WIDGET-ID 500.


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
  CREATE WINDOW C-Win ASSIGN
         HIDDEN             = YES
         TITLE              = "DataDigger"
         HEIGHT-P           = 700
         WIDTH-P            = 1523
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
/* SETTINGS FOR WINDOW C-Win
  NOT-VISIBLE,,RUN-PERSISTENT                                           */
/* REPARENT FRAME */
ASSIGN FRAME frData:FRAME = FRAME frMain:HANDLE
       FRAME frHint:FRAME = FRAME frMain:HANDLE
       FRAME frSettings:FRAME = FRAME frMain:HANDLE
       FRAME frWhere:FRAME = FRAME frMain:HANDLE.

/* SETTINGS FOR FRAME frData
                                                                        */
ASSIGN 
       btnClearDataFilter:HIDDEN IN FRAME frData           = TRUE.

ASSIGN 
       btnDataSort:HIDDEN IN FRAME frData           = TRUE.

/* SETTINGS FOR RECTANGLE rctDataFilter IN FRAME frData
   NO-ENABLE                                                            */
ASSIGN 
       rctDataFilter:HIDDEN IN FRAME frData           = TRUE.

/* SETTINGS FOR FRAME frHint
   NOT-VISIBLE                                                          */
ASSIGN 
       edHint:READ-ONLY IN FRAME frHint        = TRUE.

/* SETTINGS FOR IMAGE imgArrow IN FRAME frHint
   NO-ENABLE                                                            */
/* SETTINGS FOR FRAME frMain
   FRAME-NAME                                                           */
/* BROWSE-TAB brTables tgDebugMode frMain */
/* BROWSE-TAB brFields brTables frMain */
/* BROWSE-TAB brIndexes btnMoveTop frMain */
/* SETTINGS FOR BROWSE brFields IN FRAME frMain
   2                                                                    */
ASSIGN 
       brFields:ALLOW-COLUMN-SEARCHING IN FRAME frMain = TRUE
       brFields:COLUMN-RESIZABLE IN FRAME frMain       = TRUE.

/* SETTINGS FOR BROWSE brIndexes IN FRAME frMain
   3                                                                    */
ASSIGN 
       brIndexes:ALLOW-COLUMN-SEARCHING IN FRAME frMain = TRUE
       brIndexes:COLUMN-RESIZABLE IN FRAME frMain       = TRUE.

ASSIGN 
       brTables:POPUP-MENU IN FRAME frMain             = MENU POPUP-MENU-brTables:HANDLE
       brTables:ALLOW-COLUMN-SEARCHING IN FRAME frMain = TRUE
       brTables:COLUMN-RESIZABLE IN FRAME frMain       = TRUE.

/* SETTINGS FOR BUTTON btnClearFieldFilter IN FRAME frMain
   2                                                                    */
/* SETTINGS FOR BUTTON btnClearIndexFilter IN FRAME frMain
   3                                                                    */
/* SETTINGS FOR BUTTON btnMoveBottom IN FRAME frMain
   2                                                                    */
/* SETTINGS FOR BUTTON btnMoveDown IN FRAME frMain
   2                                                                    */
/* SETTINGS FOR BUTTON btnMoveTop IN FRAME frMain
   2                                                                    */
/* SETTINGS FOR BUTTON btnMoveUp IN FRAME frMain
   2                                                                    */
/* SETTINGS FOR BUTTON btnReset IN FRAME frMain
   2                                                                    */
ASSIGN 
       btnResizeVer:MOVABLE IN FRAME frMain          = TRUE.

ASSIGN 
       btnView:POPUP-MENU IN FRAME frMain       = MENU POPUP-MENU-btnView:HANDLE.

/* SETTINGS FOR BUTTON btnViewData IN FRAME frMain
   NO-ENABLE                                                            */
ASSIGN 
       fiFeedback:READ-ONLY IN FRAME frMain        = TRUE.

/* SETTINGS FOR FILL-IN fiFieldsFilter IN FRAME frMain
   3                                                                    */
ASSIGN 
       fiFieldsFilter:PRIVATE-DATA IN FRAME frMain     = 
                "Fields".

/* SETTINGS FOR FILL-IN fiFlagsFilter IN FRAME frMain
   3                                                                    */
ASSIGN 
       fiFlagsFilter:PRIVATE-DATA IN FRAME frMain     = 
                "Flags".

/* SETTINGS FOR FILL-IN fiIndexNameFilter IN FRAME frMain
   3                                                                    */
ASSIGN 
       fiIndexNameFilter:PRIVATE-DATA IN FRAME frMain     = 
                "Index Name".

/* SETTINGS FOR FILL-IN fiTableDesc IN FRAME frMain
   ALIGN-L                                                              */
ASSIGN 
       fiTableDesc:READ-ONLY IN FRAME frMain        = TRUE.

/* SETTINGS FOR FILL-IN fiTableFilter IN FRAME frMain
   ALIGN-L                                                              */
ASSIGN 
       fiTableFilter:PRIVATE-DATA IN FRAME frMain     = 
                "Table filter".

/* SETTINGS FOR FILL-IN fiWarning IN FRAME frMain
   NO-DISPLAY NO-ENABLE                                                 */
ASSIGN 
       fiWarning:HIDDEN IN FRAME frMain           = TRUE.

/* SETTINGS FOR RECTANGLE rcFieldFilter IN FRAME frMain
   NO-ENABLE 2                                                          */
ASSIGN 
       rcFieldFilter:HIDDEN IN FRAME frMain           = TRUE.

/* SETTINGS FOR RECTANGLE rcIndexFilter IN FRAME frMain
   NO-ENABLE 3                                                          */
ASSIGN 
       rcIndexFilter:HIDDEN IN FRAME frMain           = TRUE.

/* SETTINGS FOR RECTANGLE rcTableFilter IN FRAME frMain
   NO-ENABLE                                                            */
ASSIGN 
       rcTableFilter:HIDDEN IN FRAME frMain           = TRUE.

/* SETTINGS FOR TOGGLE-BOX tgDebugMode IN FRAME frMain
   NO-DISPLAY                                                           */
ASSIGN 
       tgDebugMode:HIDDEN IN FRAME frMain           = TRUE.

/* SETTINGS FOR TOGGLE-BOX tgSelAll IN FRAME frMain
   2                                                                    */
/* SETTINGS FOR FRAME frSettings
   NOT-VISIBLE Size-to-Fit                                              */
ASSIGN 
       FRAME frSettings:SCROLLABLE       = FALSE
       FRAME frSettings:HIDDEN           = TRUE.

/* SETTINGS FOR FRAME frWhere
                                                                        */
ASSIGN 
       FRAME frWhere:HIDDEN           = TRUE
       FRAME frWhere:MOVABLE          = TRUE.

/* SETTINGS FOR BUTTON btnAnd IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnBegins IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnBracket IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnContains IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnEq IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnGT IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnInsert IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnLT IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnMatches IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnNE IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnOr IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnQt IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnToday IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR BUTTON btnViewData-2 IN FRAME frWhere
   NO-ENABLE                                                            */
/* SETTINGS FOR COMBO-BOX cbAndOr IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR COMBO-BOX cbFields IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR COMBO-BOX cbOperator IN FRAME frWhere
   1                                                                    */
/* SETTINGS FOR FILL-IN ficValue IN FRAME frWhere
   1                                                                    */
ASSIGN 
       ficWhere2:RETURN-INSERTED IN FRAME frWhere  = TRUE.

/* SETTINGS FOR RECTANGLE rctQueryButtons IN FRAME frWhere
   1                                                                    */
IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(C-Win)
THEN C-Win:HIDDEN = yes.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME


/* Setting information for Queries and Browse Widgets fields            */

&ANALYZE-SUSPEND _QUERY-BLOCK BROWSE brFields
/* Query rebuild information for BROWSE brFields
     _START_FREEFORM
OPEN QUERY {&SELF-NAME} FOR EACH ttField.
     _END_FREEFORM
     _Query            is OPENED
*/  /* BROWSE brFields */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _QUERY-BLOCK BROWSE brIndexes
/* Query rebuild information for BROWSE brIndexes
     _START_FREEFORM
OPEN QUERY {&SELF-NAME} FOR EACH ttIndex.
     _END_FREEFORM
     _Query            is OPENED
*/  /* BROWSE brIndexes */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _QUERY-BLOCK BROWSE brTables
/* Query rebuild information for BROWSE brTables
     _START_FREEFORM
OPEN QUERY {&SELF-NAME} FOR EACH ttTable.
     _END_FREEFORM
     _Query            is NOT OPENED
*/  /* BROWSE brTables */
&ANALYZE-RESUME

 


/* **********************  Create OCX Containers  ********************** */

&ANALYZE-SUSPEND _CREATE-DYNAMIC

&IF "{&OPSYS}" = "WIN32":U AND "{&WINDOW-SYSTEM}" NE "TTY":U &THEN

CREATE CONTROL-FRAME CtrlFrame ASSIGN
       FRAME           = FRAME frMain:HANDLE
       ROW             = 25.52
       COLUMN          = 85
       HEIGHT          = 1.43
       WIDTH           = 6
       WIDGET-ID       = 292
       HIDDEN          = yes
       SENSITIVE       = yes.
/* CtrlFrame OCXINFO:CREATE-CONTROL from: {F0B88A90-F5DA-11CF-B545-0020AF6ED35A} type: PSTimer */
      CtrlFrame:MOVE-AFTER(FRAME frWhere:HANDLE).

&ENDIF

&ANALYZE-RESUME /* End of _CREATE-DYNAMIC */


/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME C-Win
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-CTRL-D OF C-Win /* DataDigger */
DO:
  DEFINE VARIABLE cDebuggerPath AS CHARACTER   NO-UNDO.

  /* Start debugger if found */
  cDebuggerPath = getRegistry('debugger', 'path').
  IF SEARCH(cDebuggerPath) = ? THEN RUN VALUE(cDebuggerPath).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-F OF C-Win /* DataDigger */
ANYWHERE DO:
  DEFINE BUFFER bFilter FOR ttFilter.

  RUN setPage({&PAGE-FIELDS}).

  FIND bFilter WHERE bFilter.cFieldName = "cFieldName" NO-ERROR.
  IF AVAILABLE bFilter THEN APPLY 'entry' TO bFilter.hFilter.

  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-I OF C-Win /* DataDigger */
ANYWHERE DO:
  RUN setPage({&PAGE-INDEXES}).
  APPLY 'entry' TO fiIndexNameFilter IN FRAME {&FRAME-NAME}.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-T OF C-Win /* DataDigger */
ANYWHERE DO:
  APPLY 'entry' TO fiTableFilter IN FRAME {&frame-name}.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-W OF C-Win /* DataDigger */
ANYWHERE DO:
  APPLY 'entry' TO ficWhere IN FRAME {&frame-name}.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON END-ERROR OF C-Win /* DataDigger */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE
DO:
  IF FRAME frHint:VISIBLE THEN
  DO:
    APPLY 'leave' TO FRAME frHint.
    glHintCancelled = TRUE.
    RETURN NO-APPLY.
  END.

  IF FRAME frSettings:VISIBLE THEN
  DO:
    APPLY 'leave' TO FRAME frSettings.
    RETURN NO-APPLY.
  END.

  IF glRowEditActive
    AND (   FOCUS:PARENT = ghDataBrowse
         OR FOCUS:PARENT = brFields:HANDLE IN FRAME {&FRAME-NAME} ) THEN
  DO:
    glRowEditActive = NO.
    APPLY 'leave' TO FOCUS.
    FOCUS:SCREEN-VALUE = FOCUS:PRIVATE-DATA.
    FOCUS:PARENT:REFRESH().
    RETURN NO-APPLY.
  END.

  IF gcQueryEditorState = 'visible' THEN
  DO:
    setQueryEditor('Hidden').
    RETURN NO-APPLY.
  END.

  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  {&WINDOW-NAME}:WINDOW-STATE = 2.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON F11 OF C-Win /* DataDigger */
ANYWHERE DO:

  IF FRAME frMain:BGCOLOR = ? THEN
  DO:
    FRAME frMain:BGCOLOR = 0.
    FRAME frData:BGCOLOR = 0.

    setRegistry('DataDigger:colors', 'DataRow:odd:fg','6').
    setRegistry('DataDigger:colors', 'DataRow:odd:bg','0').
    setRegistry('DataDigger:colors', 'DataRow:even:fg','7').
    setRegistry('DataDigger:colors', 'DataRow:even:bg','0').

  END.
  ELSE
  DO:
    FRAME frMain:BGCOLOR = ?.
    FRAME frData:BGCOLOR = ?.

/*       WHEN 'CustomFormat:fg'           THEN iColor = 12. /* red       */ */
/*       WHEN 'CustomOrder:fg'            THEN iColor = 12. /* red       */ */
/*       WHEN 'DataRow:even:bg'           THEN iColor =  8. /* lightgray */ */
/*       WHEN 'DataRow:even:fg'           THEN iColor =  0. /* black     */ */
/*       WHEN 'DataRow:odd:bg'            THEN iColor = 15. /* white     */ */
/*       WHEN 'DataRow:odd:fg'            THEN iColor =  0. /* black     */ */
/*       WHEN 'FilterBox:bg'              THEN iColor = 12. /* red       */ */
/*       WHEN 'IndexInactive:fg'          THEN iColor = 12. /* red       */ */
/*       WHEN 'PrimIndex:bg'              THEN iColor =  8. /* lightgray */ */
/*       WHEN 'QueryCounter:fg'           THEN iColor =  7. /* darkgray  */ */
/*       WHEN 'QueryError:bg'             THEN iColor = 12. /* red       */ */
/*       WHEN 'QueryError:fg'             THEN iColor = 14. /* yellow    */ */
/*       WHEN 'QueryInfo:fg'              THEN iColor =  7. /* darkgray  */ */
/*       WHEN 'RecordCount:Complete:fg'   THEN iColor =  2. /* green     */ */
/*       WHEN 'RecordCount:Incomplete:fg' THEN iColor = 12. /* red       */ */
/*       WHEN 'RecordCount:Selected:fg'   THEN iColor =  7. /* darkgray */  */
/*       WHEN 'WarningBox:bg'             THEN iColor = 14. /* yellow    */ */
/*       WHEN 'WarningBox:fg'             THEN iColor = 12. /* red       */ */
/*       WHEN 'FieldFilter:bg'            THEN iColor = 14. /* yellow    */ */
/*       WHEN 'FieldFilter:fg'            THEN iColor =  9. /* blue      */ */

  END.

  /* Colors for odd/even data rows */
/*   giDataOddRowColor[1]  = getColor("DataRow:odd:fg" ). */
/*   giDataOddRowColor[2]  = getColor("DataRow:odd:bg" ). */
/*   giDataEvenRowColor[1] = getColor("DataRow:even:fg"). */
/*   giDataEvenRowColor[2] = getColor("DataRow:even:bg"). */

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON F12 OF C-Win /* DataDigger */
ANYWHERE DO:

  DEFINE VARIABLE hWidget  AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iTargetX AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iTargetY AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cWidgets AS CHARACTER NO-UNDO.

  &IF DEFINED (UIB_is_running) &THEN

  hWidget = FOCUS.
  REPEAT:
    IF NOT VALID-HANDLE(hWidget) OR hWidget:TYPE = "WINDOW" THEN LEAVE.

    IF hWidget:X <> ? THEN iTargetX = iTargetX + hWidget:X.
    IF hWidget:Y <> ? THEN iTargetY = iTargetY + hWidget:Y.

    cWidgets = SUBSTITUTE("&1 &2: pos: &3,&4 (&5 x &6) hlp:&7 ~n&8"
                         , hWidget:TYPE
                         , hWidget:NAME
                         , hWidget:X
                         , hWidget:Y
                         , hWidget:WIDTH-PIXELS
                         , hWidget:HEIGHT-PIXELS
                         , (IF CAN-QUERY(hWidget,'CONTEXT-HELP-ID') THEN STRING(hWidget:CONTEXT-HELP-ID) ELSE '')
                         , cWidgets
                         ).

    hWidget = hWidget:PARENT.
  END.

  MESSAGE
    cWidgets SKIP(0) 'Final pos:' iTargetX ',' iTargetY
    VIEW-AS ALERT-BOX INFO BUTTONS OK TITLE ' Debug info '.

  &ENDIF

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON SHIFT-F12 OF C-Win /* DataDigger */
ANYWHERE DO:
  RUN showNewFeatures.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON WINDOW-CLOSE OF C-Win /* DataDigger */
DO:
  /* This event will close the window and terminate the procedure. */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON WINDOW-RESIZED OF C-Win /* DataDigger */
OR "END-MOVE" OF btnResizeVer
DO:
  RUN endResize.
END. /* window-resized */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON WINDOW-RESTORED OF C-Win /* DataDigger */
DO:
  APPLY 'entry' TO c-Win.
  APPLY 'entry' TO FRAME {&frame-name}.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME frMain
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frMain C-Win
ON DROP-FILE-NOTIFY OF FRAME frMain
DO:

  DEFINE VARIABLE lSuccess      AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE rNewRecord    AS ROWID       NO-UNDO.
  DEFINE VARIABLE cDroppedFiles AS CHARACTER   NO-UNDO.

  /* Get first dropped file */
  cDroppedFiles = getDroppedFiles(INPUT FRAME frMain:HANDLE).
  IF NUM-ENTRIES(cDroppedFiles,'~n') = 0 THEN RETURN.

  /* If it is a database, connect it */
  IF NUM-ENTRIES(cDroppedFiles,'~n') = 1
    AND cDroppedFiles MATCHES '*.db' THEN
  DO:
    RUN connectDroppedDatabase(cDroppedFiles).
    RETURN.
  END.

  /* If it is a param file, check the ldbnames in it */
  IF NUM-ENTRIES(cDroppedFiles,'~n') = 1
    AND cDroppedFiles MATCHES '*.pf' THEN
  DO:
    RUN connectParamFile(cDroppedFiles).
    RETURN.
  END.

  /* Otherwise it is probably an .xml file */
  RUN VALUE(getProgramDir() + 'wImportCheck.w')
    ( INPUT plReadOnlyDigger
    , INPUT cDroppedFiles
    , INPUT gcCurrentDatabase
    , INPUT gcCurrentTable
    , INPUT TABLE ttField  /* do not use by-reference */
    , INPUT TABLE ttColumn /* do not use by-reference */
    , OUTPUT lSuccess
    , OUTPUT rNewRecord
    ).

  IF lSuccess = TRUE THEN
  DO:
    RUN showHelp('DataLoaded', '').
    RUN reopenDataBrowse.

    IF rNewRecord <> ? THEN
      ghDataBrowse:QUERY:REPOSITION-TO-ROWID(rNewRecord).
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME frWhere
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frWhere C-Win
ON ALT-W OF FRAME frWhere /* Query Editor */
ANYWHERE DO:
  APPLY 'entry' TO cbAndOr.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frWhere C-Win
ON LEAVE OF FRAME frWhere /* Query Editor */
DO:
  setQueryEditor('Hidden').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brFields
&Scoped-define SELF-NAME brFields
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brFields C-Win
ON DEFAULT-ACTION OF brFields IN FRAME frMain
DO:
  DEFINE VARIABLE iRow      AS INTEGER NO-UNDO.
  DEFINE VARIABLE lSelected AS LOGICAL NO-UNDO.
  DEFINE VARIABLE hOldFocus AS HANDLE  NO-UNDO.
  DEFINE VARIABLE iBlink    AS INTEGER NO-UNDO.

  DEFINE BUFFER bField  FOR ttField.
  DEFINE BUFFER bColumn FOR ttColumn.

  PUBLISH "setUsage" ("flashField"). /* user behaviour */

  DO WITH FRAME {&FRAME-NAME}:
    FIND bField WHERE bField.cFullName = brFields:GET-BROWSE-COLUMN(3):SCREEN-VALUE NO-ERROR.
    FIND FIRST bColumn WHERE bColumn.cFieldName = bField.cFieldName NO-ERROR.

    /* If you double-click on a raw (or similar) field, the column is not there */
    IF NOT AVAILABLE bColumn THEN RETURN.

    iRow = ghDatabrowse:FOCUSED-ROW.
    IF iRow <> ? THEN
      lSelected = ghDatabrowse:IS-ROW-SELECTED(iRow).

    hOldFocus = FOCUS:HANDLE.

    /* make the column temporarily updatable and set focus to
     * the column. This will make the browse shift to the left
     * or the right if needed. Then apply focus back to where
     * it was, make the column readonly again.
     * Setting focus back is needed, otherwise the browse row
     * cannot be selected using "select-focused-row"
     */
    setWindowFreeze(YES).
    bColumn.hColumn:READ-ONLY = FALSE.
    APPLY "entry" TO bColumn.hColumn.
    APPLY "entry" TO hOldFocus.
    bColumn.hColumn:READ-ONLY = TRUE.
    RUN dataScrollNotify(ghDataBrowse).

    IF lSelected THEN
    DO:
      ghDatabrowse:SELECT-FOCUSED-ROW().
      brFields:SELECT-FOCUSED-ROW().
    END.
    setWindowFreeze(NO).

    /* Blink the filter field */
    DO iBlink = 1 TO 3:
      IF iBlink > 1 THEN RUN doNothing(400).
      bColumn.hFilter:BGCOLOR = 14.
      RUN doNothing(400).
      bColumn.hFilter:BGCOLOR = ?.
    END.

    gcLastDataField = bColumn.cFullName.

    APPLY "entry" TO bColumn.hColumn.
    .run dataOffHome.
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brFields C-Win
ON MOUSE-MENU-CLICK OF brFields IN FRAME frMain
DO:
  RUN dropFieldMenu.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brFields C-Win
ON OFF-HOME OF brFields IN FRAME frMain
DO:
  DEFINE BUFFER bFilter FOR ttFilter.

  PUBLISH "setUsage" ("offHomeFields"). /* user behaviour */

  IF NOT VALID-HANDLE(ghLastFilterField) THEN
  DO:
    FIND bFilter WHERE bFilter.cFieldName = "cFieldName" NO-ERROR.
    IF AVAILABLE bFilter THEN ghLastFilterField = bFilter.hFilter.
  END.

  setFilterFieldColor(ghLastFilterField).
  APPLY 'entry' TO ghLastFilterField.

  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brFields C-Win
ON RETURN OF brFields IN FRAME frMain
OR " "           OF ttField.lShow IN BROWSE brFields
OR VALUE-CHANGED OF ttField.lShow IN BROWSE brFields
DO:
  DEFINE BUFFER ttField FOR ttField.
  DEFINE VARIABLE cField AS CHARACTER NO-UNDO.

  PUBLISH "setUsage" ("hideField"). /* user behaviour */

  DO WITH FRAME {&frame-name}:

    FIND ttField WHERE ttField.cFullName = brFields:get-browse-column(3):screen-value NO-ERROR.
    ttField.lShow = NOT ttField.lShow.
    cField = ttField.cFieldName.

    /* This will include all extents */
    IF ttField.iExtent > 0 THEN cField = cField + '*'.

    brFields:get-browse-column(1):checked = ttField.lShow.

    RUN showField( INPUT cField, INPUT ttField.lShow).
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brFields C-Win
ON ROW-DISPLAY OF brFields IN FRAME frMain
DO:
  DEFINE VARIABLE hField AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iField AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cField AS CHARACTER NO-UNDO.

  PUBLISH "debugMessage" (3, SUBSTITUTE("Columns: &1", gcFieldBrowseColumnNames)).
  PUBLISH "debugMessage" (3, SUBSTITUTE("Filter : &1", gcFieldFilterList)).

  DO iField = 1 TO NUM-ENTRIES(gcFieldBrowseColumnHandles):
    cField = ENTRY(iField, gcFieldBrowseColumnNames).
    hField = HANDLE(ENTRY(iField,gcFieldBrowseColumnHandles)).

    /* Set colors if field is matched on FieldFilter */
    IF CAN-DO(gcFieldFilterList,ttField.cFieldName) THEN
    DO:
      hField:FGCOLOR = getColor("FieldFilter:fg").
      hField:BGCOLOR = getColor("FieldFilter:bg").
    END.

    ELSE
    DO:
      /* Set background color if field is part of primary index */
      hField:BGCOLOR = (IF ttField.lPrimary = TRUE THEN getColor('PrimIndex:bg') ELSE ?). /* gray */

      /* Set color if format is non-default */
      CASE cField:
        WHEN "cFormat" THEN hField:FGCOLOR = (IF ttField.cFormat <> ttField.cFormatOrg THEN getColor("CustomFormat:fg") ELSE ?).
        WHEN "iOrder"  THEN hField:FGCOLOR = (IF ttField.iOrder  <> ttField.iOrderOrg  THEN getColor("CustomOrder:fg") ELSE ?).
      END CASE.
    END.

  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brFields C-Win
ON SCROLL-NOTIFY OF brFields IN FRAME frMain
, brIndexes, brTables
DO:
  DEFINE VARIABLE lp AS MEMPTR  NO-UNDO.
  DEFINE VARIABLE X  AS INTEGER NO-UNDO.
  DEFINE VARIABLE Y  AS INTEGER NO-UNDO.

  PUBLISH "debugMessage" (1, "scroll-notify of brFields").

  set-size( lp ) = 16.

  RUN GetCursorPos(INPUT-OUTPUT lp).

  /* Show the location of the mouse relative to the frame */
  RUN ScreenToClient ( INPUT FRAME {&frame-name}:hwnd
                     , INPUT lp
                     ).

  x = GET-LONG( lp, 1 ).
  y = GET-LONG( lp, 5 ).

  /* Ignore when we clicked on the vertical scrollbar or
   * above the horizontal to avoid flashing
   */
  IF   SELF:name = 'brFields'
    OR SELF:name = 'brIndexes' THEN
  DO:
    IF   x > (brFields:x + brFields:width-pixels - 15)
      OR y < (brFields:y + brFields:height-pixels - 15)
      OR y > (brFields:y + brFields:height-pixels) THEN RETURN.
  END.

  set-size( lp ) = 0.

  /* scroll-notify detects a mouse action in the scrollbar area of a browse. */
  RUN resizeFilters(INPUT {&PAGE-TABLES}).
  RUN resizeFilters(INPUT {&PAGE-FAVOURITES}).
  RUN resizeFilters(INPUT {&PAGE-FIELDS}).
  RUN resizeFilters(INPUT {&PAGE-INDEXES}).

END. /* scroll-notify */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brFields C-Win
ON START-SEARCH OF brFields IN FRAME frMain
DO:
  PUBLISH "setUsage" ("sortFields"). /* user behaviour */
  RUN reopenFieldBrowse(brFields:current-column:name,?).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brIndexes
&Scoped-define SELF-NAME brIndexes
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON DEFAULT-ACTION OF brIndexes IN FRAME frMain
DO:
  DEFINE VARIABLE cFieldList     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cQuery         AS CHARACTER NO-UNDO.
  DEFINE VARIABLE hEditor        AS HANDLE    NO-UNDO.
  DEFINE VARIABLE lOk            AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE cColumnClicked AS CHARACTER NO-UNDO.

  IF NOT brIndexes:query:get-buffer-handle(1):available THEN RETURN.

  PUBLISH "setUsage" ("useIndexAsWhere"). /* user behaviour */

  /* Select the row we clicked on */
  RUN selectClickedRow(brIndexes:handle, OUTPUT lOk, OUTPUT cColumnClicked).
  IF NOT lOk THEN RETURN.

  /* Create a query expression from all the fields in the index */
  cFieldList = brIndexes:query:get-buffer-handle(1):buffer-field('cFieldList'):buffer-value.
  cQuery = getQueryFromFields(cFieldList).

  /* Give custom code a chance to alter the query */
  PUBLISH "customQuery" (INPUT gcCurrentDatabase, INPUT gcCurrentTable, INPUT-OUTPUT cQuery).

  /* If needed, expand the query editor */
  IF LOGICAL(getRegistry ("DataDigger", "AutoExpandQueryEditor")) <> NO THEN
    setQueryEditor('visible').

  /* Fill in the query on the screen */
  hEditor = getActiveQueryEditor().
  hEditor:SCREEN-VALUE = formatQueryString(cQuery, gcQueryEditorState = 'visible').

  APPLY "entry" TO hEditor.
  hEditor:CURSOR-OFFSET = LENGTH(ENTRY(1,cQuery,'~n')) + 2.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON MOUSE-MENU-CLICK OF brIndexes IN FRAME frMain
DO:
  DEFINE VARIABLE hEditor    AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hIndexName AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFieldType AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cIndex     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cColumn    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lOk        AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE iLength    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cUseIndex  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iUseIndex  AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lUseIndex  AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE cQuery     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cWord      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iWord      AS INTEGER     NO-UNDO.

  IF NOT brIndexes:query:get-buffer-handle(1):available THEN RETURN.

  PUBLISH "setUsage" ("use-index"). /* user behaviour */

  /* Select the row we clicked on */
  RUN selectClickedRow(brIndexes:handle, OUTPUT lOk, OUTPUT cColumn).
  IF NOT lOk THEN RETURN.

  hIndexName = brIndexes:query:get-buffer-handle(1):buffer-field('cIndexName'):handle.

  IF VALID-HANDLE(hIndexName) THEN
  DO:
    /* If this is a "default" index, ignore it since this is no real index
     * and we cannot add "USE-INDEX default" to a query
     */
    IF hIndexName:BUFFER-VALUE = "default" THEN RETURN.

    /* If the query editor is expanded, do actions to that field */
    hEditor = getActiveQueryEditor().

    /* If there already is an existing "USE-INDEX bladibla" then remove it */
    cQuery = "".

    WhereLoop:
    DO iWord = 1 TO NUM-ENTRIES(hEditor:SCREEN-VALUE," "):
      cWord = ENTRY(iWord,hEditor:SCREEN-VALUE," ").

      /* Remember we have found the USE-INDEX keyword */
      IF cWord = "USE-INDEX" THEN
      DO:
        lUseIndex = TRUE.
        NEXT WhereLoop.
      END.

      /* Skip index name after USE-INDEX */
      IF lUseIndex AND CAN-FIND(ttIndex WHERE ttIndex.cIndexName = cWord) THEN
      DO:
        lUseIndex = FALSE.
        NEXT WhereLoop.
      END.

      cQuery = cQuery + " " + cWord.
    END.

    hEditor:SCREEN-VALUE = cQuery.
    cIndex = SUBSTITUTE("USE-INDEX &1", hIndexName:BUFFER-VALUE).
    iLength = LENGTH(cIndex).

    /* No text selected */
    IF hEditor:SELECTION-TEXT = "" THEN
    DO:
      /* If ficQuery only holds the text <empty> then delete that */
      IF hEditor:SCREEN-VALUE = '<empty>' THEN hEditor:SCREEN-VALUE = ''.
      hEditor:SCREEN-VALUE = TRIM(SUBSTITUTE("&1 &2", hEditor:SCREEN-VALUE, cIndex)).
    END.
    ELSE
    DO:
      hEditor:REPLACE-SELECTION-TEXT(cIndex).
    END.

    /* Give custom code a chance to alter the query */
    cQuery = hEditor:SCREEN-VALUE.
    PUBLISH "customQuery" (INPUT gcCurrentDatabase, INPUT gcCurrentTable, INPUT-OUTPUT cQuery).
    hEditor:SCREEN-VALUE = cQuery.

    APPLY "entry" TO hEditor.
    hEditor:CURSOR-OFFSET = LENGTH(hEditor:SCREEN-VALUE) + 1.
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON OFF-HOME OF brIndexes IN FRAME frMain
DO:
  PUBLISH "setUsage" ("offHomeIndexes"). /* user behaviour */

  IF NOT VALID-HANDLE(ghLastIndexFilter) THEN
    ghLastIndexFilter = fiIndexNameFilter:handle.

  setFilterFieldColor(ghLastIndexFilter).
  APPLY 'entry' TO ghLastIndexFilter.

  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON ROW-DISPLAY OF brIndexes IN FRAME frMain
DO:
  DEFINE VARIABLE hField AS HANDLE.
  DEFINE VARIABLE iField AS INTEGER.

  DO iField = 1 TO NUM-ENTRIES(gcIndexBrowseColumnHandles):
    hField = HANDLE(ENTRY(iField,gcIndexBrowseColumnHandles)).

    /* Set color if index is not active */
    hField:FGCOLOR = (IF ttIndex.lIndexActive = FALSE THEN getColor('IndexInactive:fg') ELSE ?). /* red */
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON START-SEARCH OF brIndexes IN FRAME frMain
DO:
  PUBLISH "setUsage" ("sortIndexes"). /* user behaviour */
  RUN reopenIndexBrowse(brIndexes:current-column:name,?).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brTables
&Scoped-define SELF-NAME brTables
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON MOUSE-SELECT-CLICK OF brTables IN FRAME frMain
DO:
  /* When we click on a table in the browse, we don't want
   * to wait until the timer ocx refreshes; do it instantly.
   */
  DEFINE VARIABLE cOldTable AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cOldDatabase AS CHARACTER NO-UNDO.

  cOldTable    = gcCurrentTable.
  cOldDatabase = gcCurrentDatabase.

  APPLY "value-changed" TO SELF.

  /* Cancel the timer */
  RUN setTimer("timedTableChange", 0).

  /* Apply the change immediately */
  IF cOldTable <> gcCurrentTable
    OR cOldDatabase <> gcCurrentDatabase THEN
  DO:
    setWindowFreeze(YES).
    RUN setTableContext(INPUT gcCurrentTable ).
    setWindowFreeze(NO).
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON OFF-HOME OF brTables IN FRAME frMain
DO:
  PUBLISH "setUsage" ("offHomeTables"). /* user behaviour */
  setFilterFieldColor(fiTableFilter:handle).
  APPLY 'entry' TO fiTableFilter.

  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON START-SEARCH OF brTables IN FRAME frMain
DO:
  PUBLISH "setUsage" ("sortTables"). /* user behaviour */
  RUN reopenTableBrowse(brTables:current-column:name).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON VALUE-CHANGED OF brTables IN FRAME frMain
DO:
  DEFINE VARIABLE hBuffer      AS HANDLE    NO-UNDO.
  DEFINE VARIABLE cOldTable    AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cOldDatabase AS CHARACTER NO-UNDO.
  DEFINE VARIABLE lTableFound  AS LOGICAL   NO-UNDO.

  setWindowFreeze(YES).

  hBuffer      = brTables:QUERY:GET-BUFFER-HANDLE(1).
  cOldTable    = gcCurrentTable.
  cOldDatabase = gcCurrentDatabase.
  lTableFound = hBuffer:AVAILABLE.

  IF lTableFound THEN
  DO:
    gcCurrentTable           = hBuffer::cTableName.
    gcCurrentDatabase        = hBuffer::cDatabase.
    fiTableDesc:SCREEN-VALUE = hBuffer::cTableDesc.
    fiTableDesc:TOOLTIP      = hBuffer::cTableDesc.

    RUN showFavourite(hBuffer::lFavourite).

    PUBLISH "debugMessage" (2,SUBSTITUTE("Select table &1.&2", gcCurrentDatabase, gcCurrentTable)).
  END.
  ELSE
  DO:
    /* Make sure the data browse is empty. The easies way is redrawing it */
    IF NUM-DBS > 0 AND gcCurrentTable <> "" THEN
    DO:
      RUN reopenDataBrowse-create(INPUT gcCurrentDatabase, INPUT gcCurrentTable).
      ghDataBrowse:SENSITIVE = FALSE.
    END.

    gcCurrentTable           = ''.
    gcCurrentDatabase        = ENTRY(1, getDatabaseList() ).
    fiTableDesc:SCREEN-VALUE = "".
    fiTableDesc:TOOLTIP      = ''.

    RUN showFavourite(FALSE).
  END.

  /* Switch on/off UI */
  btnFavourite:SENSITIVE = lTableFound.
  btnClearDataFilter:SENSITIVE IN FRAME frData = lTableFound.
  btnDataSort:SENSITIVE IN FRAME frData = lTableFound.
  btnNextQuery:SENSITIVE = lTableFound.
  btnPrevQuery:SENSITIVE = lTableFound.
  btnViewData-2:SENSITIVE IN FRAME frWhere = lTableFound.
  btnViewData:SENSITIVE  = lTableFound.
  btnWhere:SENSITIVE     = lTableFound.
  ficWhere:SENSITIVE     = lTableFound.
  btnWhere:SENSITIVE     = lTableFound.
  btnViewData:SENSITIVE  = lTableFound.
  btnClear:SENSITIVE     = lTableFound.
  btnQueries:SENSITIVE   = lTableFound.
  btnClipboard:SENSITIVE = lTableFound.
  btnAdd:SENSITIVE       = lTableFound.
  btnLoad:SENSITIVE      = lTableFound.

  IF cOldTable <> gcCurrentTable
    OR cOldDatabase <> gcCurrentDatabase THEN
  DO:
    /* Report new table to listeners */
    PUBLISH 'TableChange' (gcCurrentDatabase,gcCurrentTable).

    EMPTY TEMP-TABLE ttField.
    EMPTY TEMP-TABLE ttIndex.

    /* Clear user query */
    ficWhere:SCREEN-VALUE = ''.

    /* Clear the filters */
    RUN clearFieldFilter.
    RUN filterFieldsBrowse.

    RUN clearIndexFilter.
    RUN filterIndexBrowse.

    RUN setTimer("timedTableChange", 300).
  END.

  setWindowFreeze(NO).
END. /* value-changed of brTables */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frHint
&Scoped-define SELF-NAME btGotIt
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btGotIt C-Win
ON 1 OF btGotIt IN FRAME frHint /* I Got it */
OR "2" OF btGotIt
OR "3" OF btGotIt
OR "4" OF btGotIt
DO:

  DO WITH FRAME frHint:
    APPLY "choose" TO btGotIt.

    RUN showHint( INPUT WIDGET-HANDLE(FRAME frHint:PRIVATE-DATA)
                , INPUT INTEGER(KEYLABEL(LASTKEY))
                , INPUT edHint:SCREEN-VALUE
                ).
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btGotIt C-Win
ON CHOOSE OF btGotIt IN FRAME frHint /* I Got it */
DO:
  FRAME frHint:VISIBLE = FALSE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnAbout
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAbout C-Win
ON CHOOSE OF btnAbout IN FRAME frSettings /* Que */
, btnAbout-txt
OR "CTRL-SHIFT-A" OF c-win ANYWHERE
DO:
  PUBLISH "setUsage" ("About"). /* user behaviour */
  HIDE FRAME frSettings.
  RUN VALUE(getProgramDir() + 'wAbout.w').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAbout C-Win
ON MOUSE-MENU-CLICK OF btnAbout IN FRAME frSettings /* Que */
, btnAbout-txt
DO:
  PUBLISH "setUsage" ("showChangelog"). /* user behaviour */
  HIDE FRAME frSettings.
  OS-COMMAND NO-WAIT START VALUE(getProgramDir() + '\DataDigger.txt').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnAdd
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAdd C-Win
ON CHOOSE OF btnAdd IN FRAME frMain /* Add */
OR 'insert-mode' OF brTables
DO:
  PUBLISH "setUsage" ("addRecord"). /* user behaviour */
  RUN btnAddChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME btnAnd
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAnd C-Win
ON CHOOSE OF btnAnd IN FRAME frWhere /* and */
, btnOr, btnEq, btnNe, btnGt, btnLt, btnToday, btnMatches, btnContains, btnBegins
DO:
  PUBLISH "setUsage" ("useWherePanel"). /* user behaviour */
  /* No text selected */
  IF ficWhere2:selection-text = "" THEN
    ficWhere2:insert-string(SUBSTITUTE(' &1 ', SELF:label)).
  ELSE
    ficWhere2:replace-selection-text(SUBSTITUTE(' &1 ', SELF:label)).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnBracket
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnBracket C-Win
ON CHOOSE OF btnBracket IN FRAME frWhere /* () */
DO:
  PUBLISH "setUsage" ("useWherePanel"). /* user behaviour */
  /* No text selected */
  IF ficWhere2:selection-text = "" THEN
  DO:
    ficWhere2:insert-string(SUBSTITUTE(' &1 ', SELF:label)).
    ficWhere2:cursor-offset = ficWhere2:cursor-offset - 2.
  END.
  ELSE
    ficWhere2:replace-selection-text(SUBSTITUTE(' ( &1 ) ', ficWhere2:selection-text)).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnCancel-2
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnCancel-2 C-Win
ON CHOOSE OF btnCancel-2 IN FRAME frWhere /* Cancel */
DO:
  PUBLISH "setUsage" ("useWherePanel"). /* user behaviour */
  ficWhere2:screen-value IN FRAME frWhere = ficWhere:screen-value IN FRAME frMain.
  setQueryEditor('Hidden').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnClear
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClear C-Win
ON CHOOSE OF btnClear IN FRAME frMain /* C */
OR SHIFT-DEL OF ficWhere  IN FRAME frMain
OR SHIFT-DEL OF ficWhere2 IN FRAME frWhere
OR 'CHOOSE' OF btnClear-2 IN FRAME frWhere
DO:
  DEFINE VARIABLE hEditor AS HANDLE      NO-UNDO.
  PUBLISH "setUsage" ("clearQuery"). /* user behaviour */

  hEditor = getActiveQueryEditor().

  hEditor:SCREEN-VALUE = ''.
  hEditor:BGCOLOR      = ?. /* default */
  hEditor:FGCOLOR      = ?. /* default */
  hEditor:TOOLTIP      = ''.

  /* Clear query in ini file */
  setRegistry ( SUBSTITUTE('DB:&1'   , gcCurrentDatabase )
              , SUBSTITUTE('&1:query', gcCurrentTable )
              , ''
              ).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frData
&Scoped-define SELF-NAME btnClearDataFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClearDataFilter C-Win
ON CHOOSE OF btnClearDataFilter IN FRAME frData /* C */
DO:
  PUBLISH "setUsage" ("clearDataFilter"). /* user behaviour */
  RUN btnClearDataFilterChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnClearFieldFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClearFieldFilter C-Win
ON CHOOSE OF btnClearFieldFilter IN FRAME frMain /* C */
DO:
  RUN btnClearFieldFilterChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClearIndexFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClearIndexFilter C-Win
ON CHOOSE OF btnClearIndexFilter IN FRAME frMain /* C */
DO:
  RUN btnClearIndexFilterChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClearTableFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClearTableFilter C-Win
ON CHOOSE OF btnClearTableFilter IN FRAME frMain /* C */
DO:
  PUBLISH "setUsage" ("clearTableFilter"). /* user behaviour */

  RUN initTableFilter(INPUT-OUTPUT TABLE ttTableFilter).

  fiTableFilter     :SCREEN-VALUE = fiTableFilter:PRIVATE-DATA.
  cbDatabaseFilter  :SCREEN-VALUE = ' '.

  fiTableFilter     :MODIFIED = NO.
  cbDatabaseFilter  :MODIFIED = NO.

  setFilterFieldColor(fiTableFilter   :HANDLE).
  setFilterFieldColor(cbDatabaseFilter:HANDLE).

  RUN getTablesFiltered(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
  RUN setWindowTitle. 

  /* Get table properties from the INI file */
  RUN getTableStats(INPUT-OUTPUT TABLE ttTable).

  RUN filterTables.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClipboard
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClipboard C-Win
ON CHOOSE OF btnClipboard IN FRAME frMain /* Cp */
/* or 'ctrl-c' of ficWhere  in frame frMain  */
/* or 'ctrl-c' of ficWhere2 in frame frWhere */
OR 'CHOOSE' OF btnClipboard-2 IN FRAME frWhere
DO:
  DEFINE VARIABLE cQuery  AS CHARACTER NO-UNDO.
  DEFINE VARIABLE hEditor AS HANDLE    NO-UNDO.

  PUBLISH "setUsage" ("copyQuery"). /* user behaviour */

  hEditor = getActiveQueryEditor().

  IF LENGTH(hEditor:SELECTION-TEXT) > 0 THEN
    cQuery = hEditor:SELECTION-TEXT.
  ELSE
  IF VALID-HANDLE(ghDataBrowse) THEN
    cQuery = getReadableQuery(ghDataBrowse:QUERY:prepare-string).
  ELSE
  IF hEditor:SCREEN-VALUE = "" THEN
      cQuery = SUBSTITUTE('for each &1.&2 no-lock'
                         , gcCurrentDatabase
                         , gcCurrentTable
                         ).
  ELSE
    cQuery = SUBSTITUTE('for each &1.&2 no-lock &3 &4'
                       , gcCurrentDatabase
                       , gcCurrentTable
                       , (IF NOT hEditor:SCREEN-VALUE BEGINS 'where' THEN 'where' ELSE '')
                       , TRIM(hEditor:SCREEN-VALUE)
                       ).

  /* Dont take the tooltip because that is not set until the query is executed */
  cQuery = formatQueryString(cQuery, YES).
  CLIPBOARD:VALUE = cQuery.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClone
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClone C-Win
ON CHOOSE OF btnClone IN FRAME frMain /* Clone */
DO:
  PUBLISH "setUsage" ("cloneRecord"). /* user behaviour */
  RUN btnCloneChoose.
END. /* choose of btnDelete */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnConnections
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnConnections C-Win
ON CHOOSE OF btnConnections IN FRAME frSettings /* Con */
, MENU-ITEM m_Manage_Connections IN MENU POPUP-MENU-brTables
, btnConnections-txt
OR 'CTRL-SHIFT-N' OF c-win ANYWHERE
DO:
  PUBLISH "setUsage" ("manageFavourites"). /* user behaviour */
  HIDE FRAME frSettings.
  RUN btnFavouritesChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDataAdmin
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDataAdmin C-Win
ON CHOOSE OF btnDataAdmin IN FRAME frSettings /* ADM */
, btnDataAdmin-txt
OR "ALT-H" OF c-win ANYWHERE
DO:
  HIDE FRAME frSettings.
  RUN startTool("Admin").
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDataDigger
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDataDigger C-Win
ON CHOOSE OF btnDataDigger IN FRAME frSettings /* DD */
, btnDataDigger-txt
OR "ALT-D" OF FRAME frMain ANYWHERE
DO:
  /* If we're in the middle of the tour, ignore this event */
  IF FRAME frHint:VISIBLE THEN RETURN NO-APPLY.

  PUBLISH "setUsage" ("newInstance"). /* user behaviour */

  /* Set the X and Y a little higher so the new window appears cascaded */
  setRegistry("DataDigger", "Window:x", STRING(c-win:X + 20) ).
  setRegistry("DataDigger", "Window:y", STRING(c-win:Y + 20) ).

  RUN VALUE(getProgramDir() + 'wDataDigger.w') PERSISTENT (INPUT plReadOnlyDigger) .
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frData
&Scoped-define SELF-NAME btnDataSort
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDataSort C-Win
ON CHOOSE OF btnDataSort IN FRAME frData /* S */
OR 'ALT-S' OF c-win ANYWHERE
DO:
  PUBLISH "setUsage" ("SortData"). /* user behaviour */
  RUN btnDataSortChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnDelete
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDelete C-Win
ON CHOOSE OF btnDelete IN FRAME frMain /* Del */
DO:
  PUBLISH "setUsage" ("deleteRecord"). /* user behaviour */
  RUN btnDeleteChoose.
END. /* choose of btnDelete */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnDict
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDict C-Win
ON CHOOSE OF btnDict IN FRAME frSettings /* DD */
, btnDict-txt
OR 'CTRL-SHIFT-D' OF c-win ANYWHERE
DO:

  RUN startTool("Dict").

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnDump
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDump C-Win
ON CHOOSE OF btnDump IN FRAME frMain /* Save */
OR "CTRL-S" OF c-win ANYWHERE
DO:

  IF btnDump:SENSITIVE IN FRAME frMain THEN RUN btnDumpChoose.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnEdit C-Win
ON CHOOSE OF btnEdit IN FRAME frMain /* Edit */
DO:
  PUBLISH "setUsage" ("editRecord"). /* user behaviour */
  RUN btnEditChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnFavourite
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnFavourite C-Win
ON CHOOSE OF btnFavourite IN FRAME frMain /* F */
OR "f" OF brTables
OR "*" OF brTables
OR "CHOOSE" OF MENU-ITEM m_Toggle_as_favourite IN MENU POPUP-MENU-brTables
OR CHOOSE OF btnFavourite
DO:
  DEFINE BUFFER bTable FOR ttTable.
  PUBLISH "setUsage" ("addToFavourites"). /* user behaviour */

  /* Find table and set/unset as fav */
  FIND bTable
    WHERE bTable.cDatabase  = gcCurrentDatabase
      AND bTable.cTableName = gcCurrentTable.

  /* Toggle fav-status and save */
  bTable.lFavourite = NOT bTable.lFavourite.
  setRegistry( SUBSTITUTE("DB:&1",gcCurrentDatabase)
             , SUBSTITUTE("&1:Favourite",gcCurrentTable)
             , (IF bTable.lFavourite THEN "TRUE" ELSE ?)
             ).

  /* If we are in the favo-view then refresh the browse to get rid of this table */
  IF glShowFavourites THEN RUN reopenTableBrowse(?).

  IF giCurrentPage <> {&PAGE-FAVOURITES} THEN
    RUN showFavourite(bTable.lFavourite).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnHelp
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnHelp C-Win
ON CHOOSE OF btnHelp IN FRAME frSettings /* Help */
, btnHelp-txt
OR "HELP" OF c-win ANYWHERE
DO:
  HIDE FRAME frSettings.
  RUN showTour.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME btnInsert
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnInsert C-Win
ON CHOOSE OF btnInsert IN FRAME frWhere /* + */
OR "return" OF /* cbAndOr, cbFields, cbOperator, */ ficValue
DO:
  DEFINE BUFFER ttField FOR ttField.
  DEFINE VARIABLE cField AS CHARACTER NO-UNDO.

  PUBLISH "setUsage" ("useWherePanel"). /* user behaviour */

  FIND ttField WHERE ttField.cFullName = cbFields:screen-value NO-ERROR.
  IF NOT AVAILABLE ttField THEN RETURN.
  cField = ttField.cFullName.

  IF cField = 'RECID' OR cField = 'ROWID'
    THEN cField = SUBSTITUTE('&1(&2)', cField, gcCurrentTable ).

  ficWhere2:insert-string(LEFT-TRIM(SUBSTITUTE('&1 &2 &3 &4&5'
                                        , (IF cbAndOr:screen-value = ? THEN '' ELSE cbAndOr:screen-value)
                                        , cField
                                        , cbOperator:screen-value
                                        , IF ttField.cDataType = 'character' THEN QUOTER(ficValue:screen-value) ELSE ficValue:screen-value
                                        , CHR(13)
                                        )
                              )
                         ).

  APPLY "entry" TO cbAndOr.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnLoad
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnLoad C-Win
ON CHOOSE OF btnLoad IN FRAME frMain /* Load */
OR "CTRL-L" OF c-win ANYWHERE
DO:

  IF btnLoad:SENSITIVE IN FRAME frMain THEN RUN btnLoadChoose.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnMoveBottom
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnMoveBottom C-Win
ON CHOOSE OF btnMoveBottom IN FRAME frMain /* Btm */
OR 'ctrl-shift-cursor-down' OF brFields
DO:
  PUBLISH "setUsage" ("changeFieldOrder"). /* user behaviour */
  RUN moveField('bottom').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnMoveDown
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnMoveDown C-Win
ON CHOOSE OF btnMoveDown IN FRAME frMain /* Dn */
OR 'ctrl-cursor-down' OF brFields
DO:
  /* Show a hint when user uses this for the first time */
  IF getRegistry("DataDigger:Usage", "changeFieldOrder:numUsed") = ? THEN
    RUN showHint(brFields:HANDLE,2,"~nYou can also use CTRL-UP / CTRL-DOWN on the field browse to move fields around").

  PUBLISH "setUsage" ("changeFieldOrder"). /* user behaviour */
  RUN moveField('down').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnMoveTop
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnMoveTop C-Win
ON CHOOSE OF btnMoveTop IN FRAME frMain /* Top */
OR 'ctrl-shift-cursor-up' OF brFields
DO:
  PUBLISH "setUsage" ("changeFieldOrder"). /* user behaviour */
  RUN moveField('top').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnMoveUp
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnMoveUp C-Win
ON CHOOSE OF btnMoveUp IN FRAME frMain /* Up */
OR 'ctrl-cursor-up' OF brFields
DO:
  /* Show a hint when user uses this for the first time */
  IF getRegistry("DataDigger:Usage", "changeFieldOrder:numUsed") = ? THEN
    RUN showHint(brFields:HANDLE,2,"~nYou can also use CTRL-UP / CTRL-DOWN on the field browse to move fields around").

  PUBLISH "setUsage" ("changeFieldOrder"). /* user behaviour */
  RUN moveField('up').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME btnOK
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOK C-Win
ON CHOOSE OF btnOK IN FRAME frWhere /* OK */
DO:
  PUBLISH "setUsage" ("useWherePanel"). /* user behaviour */
  setQueryEditor('Hidden').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnProcEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnProcEdit C-Win
ON CHOOSE OF btnProcEdit IN FRAME frSettings /* Ed */
, btnProcEdit-txt
OR "SHIFT-F3" OF c-win ANYWHERE
DO:
  PUBLISH "setUsage" ("Editor"). /* user behaviour */

  /* Return if progress version is runtime */
  IF PROGRESS = "Run-time" THEN RETURN.

  /* In read-only mode, return */
  IF plReadOnlyDigger THEN RETURN.

  RUN _edit.p.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME btnQt
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQt C-Win
ON CHOOSE OF btnQt IN FRAME frWhere /* "" */
DO:
  PUBLISH "setUsage" ("useWherePanel"). /* user behaviour */

  /* No text selected */
  IF ficWhere2:selection-text = "" THEN
  DO:
    ficWhere2:insert-string(SUBSTITUTE(' &1 ', SELF:label)).
    ficWhere2:cursor-offset = ficWhere2:cursor-offset - 2.
  END.
  ELSE
    ficWhere2:replace-selection-text(SUBSTITUTE('"&1"', ficWhere2:selection-text)).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnQueries
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQueries C-Win
ON CHOOSE OF btnQueries IN FRAME frMain /* Q */
OR 'ALT-Q'        OF c-win
OR 'CTRL-SHIFT-Q' OF c-win
OR 'CTRL-INS'     OF ficWhere       IN FRAME frMain
OR 'CTRL-INS'     OF ficWhere2      IN FRAME frWhere
OR 'CHOOSE'       OF btnQueries-2   IN FRAME frWhere
OR 'CHOOSE'       OF btnQueries-txt IN FRAME frSettings
DO:
  PUBLISH "setUsage" ("selectQuery"). /* user behaviour */
  HIDE FRAME frSettings.
  RUN btnQueriesChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnQueryTester
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQueryTester C-Win
ON CHOOSE OF btnQueryTester IN FRAME frSettings /* QT */
, btnQueryTester-txt
OR "CTRL-Q" OF c-win ANYWHERE
DO:
  PUBLISH "setUsage" ("QueryTester"). /* user behaviour */
  HIDE FRAME frSettings.
  RUN VALUE(getProgramDir() + 'query-tester.w') (INPUT-OUTPUT TABLE ttTestQuery BY-REFERENCE).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnReset
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnReset C-Win
ON CHOOSE OF btnReset IN FRAME frMain /* R */
OR "CTRL-SHIFT-HOME" OF brFields
DO:
  PUBLISH "setUsage" ("changeFieldOrder"). /* user behaviour */
  RUN resetFields.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnSettings
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON CHOOSE OF btnSettings IN FRAME frSettings /* Set */
, btnSettings-txt
OR 'CTRL-ALT-S' OF FRAME frMain ANYWHERE 
DO:
  PUBLISH "setUsage" ("Settings"). /* user behaviour */
  HIDE FRAME frSettings.
  RUN btnSettingsChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON CURSOR-DOWN OF btnSettings IN FRAME frSettings /* Set */
, btnDataDigger, btnConnections
, btnSettings, btnProcEdit
, btnDict, btnDataAdmin
, btnQueries-3, btnQueryTester
, btnHelp, btnAbout
DO:
  RUN navigateSettings(SELF:HANDLE,LAST-EVENT:LABEL).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON CURSOR-LEFT OF btnSettings IN FRAME frSettings /* Set */
, btnDataDigger, btnConnections
, btnSettings, btnProcEdit
, btnDict, btnDataAdmin
, btnQueries-3, btnQueryTester
, btnHelp, btnAbout
DO:
  RUN navigateSettings(SELF:HANDLE,LAST-EVENT:LABEL).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON CURSOR-RIGHT OF btnSettings IN FRAME frSettings /* Set */
, btnDataDigger, btnConnections
, btnSettings, btnProcEdit
, btnDict, btnDataAdmin
, btnQueries-3, btnQueryTester
, btnHelp, btnAbout
DO:
  RUN navigateSettings(SELF:HANDLE,LAST-EVENT:LABEL).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON CURSOR-UP OF btnSettings IN FRAME frSettings /* Set */
, btnDataDigger, btnConnections
, btnSettings, btnProcEdit
, btnDict, btnDataAdmin
, btnQueries-3, btnQueryTester
, btnHelp, btnAbout
DO:
  RUN navigateSettings(SELF:HANDLE,LAST-EVENT:LABEL).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON MOUSE-MENU-CLICK OF btnSettings IN FRAME frSettings /* Set */
DO:
  DEFINE VARIABLE cEnvironment AS CHARACTER   NO-UNDO.

  PUBLISH "setUsage" ("EditSettings"). /* user behaviour */
  HIDE FRAME frSettings.

  /* Load or create personalized ini file */
  cEnvironment = SUBSTITUTE('&1DataDigger-&2.ini', getProgramDir(), getUserName()).

  /* Start default editor for ini file */
  OS-COMMAND NO-WAIT START VALUE( cEnvironment ).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnTabFavourites
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabFavourites C-Win
ON CHOOSE OF btnTabFavourites IN FRAME frMain /* Fav */
OR 'ctrl-2' OF FRAME {&frame-name} ANYWHERE
DO:
  /* Keep track of user behaviour */
  PUBLISH "setUsage" ("setPage").
  RUN setPage({&PAGE-FAVOURITES}).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabFavourites C-Win
ON MOUSE-MENU-CLICK OF btnTabFavourites IN FRAME frMain /* Fav */
DO:
  DEFINE VARIABLE dRow           AS DECIMAL     NO-UNDO.
  DEFINE VARIABLE iRow           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cFieldList     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cQuery         AS CHARACTER NO-UNDO.

  /* Find the primary index */
  FIND FIRST ttIndex WHERE ttIndex.cIndexFlags MATCHES '*P*' NO-ERROR.
  IF NOT AVAILABLE ttIndex THEN RETURN.

  /* Create a query expression from all the fields in the index */
  cQuery = getQueryFromFields(ttIndex.cFieldList).

  /* If needed, expand the query editor */
  IF LOGICAL(getRegistry ("DataDigger", "AutoExpandQueryEditor")) <> NO THEN
    setQueryEditor('visible').

  /* Fill in the query on the screen */
  ficWhere:screen-value = formatQueryString(cQuery, gcQueryEditorState = 'visible').

  APPLY "entry" TO ficWhere.
  ficWhere:cursor-offset = LENGTH(ENTRY(1,cQuery,'~n')) + 2.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTabFields
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabFields C-Win
ON CHOOSE OF btnTabFields IN FRAME frMain /* Fld */
OR 'ctrl-3' OF FRAME {&frame-name} ANYWHERE
DO:
  /* Keep track of user behaviour */
  PUBLISH "setUsage" ("setPage").
  RUN setPage({&PAGE-FIELDS}).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTabIndexes
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabIndexes C-Win
ON CHOOSE OF btnTabIndexes IN FRAME frMain /* Idx */
OR 'ctrl-4' OF FRAME {&FRAME-NAME} ANYWHERE
DO:
  /* Keep track of user behaviour */
  PUBLISH "setUsage" ("setPage").
  RUN setPage({&PAGE-INDEXES}).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabIndexes C-Win
ON MOUSE-MENU-CLICK OF btnTabIndexes IN FRAME frMain /* Idx */
DO:
  DEFINE VARIABLE dRow           AS DECIMAL     NO-UNDO.
  DEFINE VARIABLE iRow           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cFieldList     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cQuery         AS CHARACTER NO-UNDO.

  /* Find the primary index */
  FIND FIRST ttIndex WHERE ttIndex.cIndexFlags MATCHES '*P*' NO-ERROR.
  IF NOT AVAILABLE ttIndex THEN RETURN.

  /* Create a query expression from all the fields in the index */
  cQuery = getQueryFromFields(ttIndex.cFieldList).

  /* If needed, expand the query editor */
  IF LOGICAL(getRegistry ("DataDigger", "AutoExpandQueryEditor")) <> NO THEN
    setQueryEditor('visible').

  /* Fill in the query on the screen */
  ficWhere:screen-value = formatQueryString(cQuery, gcQueryEditorState = 'visible').

  APPLY "entry" TO ficWhere.
  ficWhere:cursor-offset = LENGTH(ENTRY(1,cQuery,'~n')) + 2.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTableFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTableFilter C-Win
ON CHOOSE OF btnTableFilter IN FRAME frMain /* Y */
OR "CTRL-CURSOR-DOWN" OF fiTableFilter
  OR "CTRL-CURSOR-DOWN" OF cbDatabaseFilter
  OR "CTRL-CURSOR-DOWN" OF brTables
  OR "CTRL-CURSOR-DOWN" OF btnClearTableFilter
  OR "CTRL-CURSOR-DOWN" OF btnTableFilter
  OR "ALT-CURSOR-DOWN" OF fiTableFilter
  OR "ALT-CURSOR-DOWN" OF brTables
  OR "ALT-CURSOR-DOWN" OF btnClearTableFilter
  OR "ALT-CURSOR-DOWN" OF btnTableFilter
DO:

  /* Filter options only available on normal table page */
  IF giCurrentPage <> {&PAGE-FAVOURITES}
    AND btnTableFilter:SENSITIVE THEN
  DO:
    RUN setTableFilterOptions.
    APPLY 'VALUE-CHANGED' TO brTables IN FRAME frMain.
    APPLY 'ENTRY' TO brTables IN FRAME frMain.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTabTables
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabTables C-Win
ON CHOOSE OF btnTabTables IN FRAME frMain /* Tbl */
OR 'ctrl-1' OF FRAME {&frame-name} ANYWHERE
DO:
  /* Keep track of user behaviour */
  PUBLISH "setUsage" ("setPage").
  RUN setPage({&PAGE-TABLES}).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTools
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTools C-Win
ON CHOOSE OF btnTools IN FRAME frMain /* Tools */
OR "CTRL-T" OF FRAME frMain ANYWHERE
DO:
  PUBLISH "setUsage" ("showTools"). /* user behaviour */
  
  IF FRAME frSettings:VISIBLE THEN
  DO:
    RUN hideSettingsFrame.
    RETURN NO-APPLY.
  END.

  /* Show settings frame */
  FRAME frSettings:X = btnTools:X.
  FRAME frSettings:Y = btnTools:Y + btnTools:HEIGHT-PIXELS.
  VIEW FRAME frSettings.

  /* Try to hide it every 2 seconds */
  IF KEYLABEL(LASTKEY) <> "CTRL-T" AND glUseTimer THEN
    RUN setTimer("hideSettingsFrame",3000).

  APPLY 'entry' TO btnDataDigger IN FRAME frSettings.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnView
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnView C-Win
ON CHOOSE OF btnView IN FRAME frMain /* View */
DO:
  PUBLISH "setUsage" ("viewData"). /* user behaviour */
  RUN btnViewChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnViewData
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnViewData C-Win
ON CHOOSE OF btnViewData IN FRAME frMain /* -> */
OR 'ctrl-j' OF cbAndOr, cbFields, cbOperator, ficValue, ficWhere, fiTableFilter, brTables, brFields
OR MOUSE-SELECT-DBLCLICK, RETURN OF brTables
OR 'ctrl-j' OF ficWhere2 IN FRAME frWhere
OR 'F2' OF ficWhere
OR 'F2' OF ficWhere2 IN FRAME frWhere
OR 'CTRL-J' OF ttField.cFormat IN BROWSE brFields
OR 'CHOOSE' OF btnViewData-2 IN FRAME frWhere
DO:
  DEFINE BUFFER bTimer FOR ttTimer.

  PUBLISH "setUsage" ("viewData"). /* user behaviour */

  /* Only proceed if the button is sensitive */
  IF NOT btnViewData:SENSITIVE THEN RETURN NO-APPLY.

  /* Make sure the table browse is up to date. Because we use a slight delay
   * to show the fields of the table, there is a theoretical chance the user
   * points to a new table and starts the query within 200 msec.
   * This is a realistic scenario when you use the keyboard:
   * - select table A
   * - press cursor down
   * - IMMEDIATELY (within 200 msec) press ENTER
   */
  APPLY "VALUE-CHANGED" TO brTables IN FRAME frMain.

  /* Check whether a table change event is pending
   * And speed it up if needed.
   */
  FIND bTimer WHERE bTimer.cProc = 'timedTableChange' NO-ERROR.
  IF AVAILABLE bTimer THEN RUN setTimer('timedTableChange',1).

  /* Open the query */
  RUN reopenDataBrowse.

  IF VALID-HANDLE(ghDataBrowse) THEN
    APPLY 'entry' TO ghDataBrowse.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnWhere
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnWhere C-Win
ON CHOOSE OF btnWhere IN FRAME frMain /* Where */
OR 'CTRL-ALT-W' OF c-win ANYWHERE
DO:
  PUBLISH "setUsage" ("switchQueryEditor"). /* user behaviour */

  CASE gcQueryEditorState:
    WHEN 'visible' THEN
    DO:
      setQueryEditor('hidden').
      APPLY 'entry' TO ficWhere IN FRAME frMain.
    END.

    WHEN 'hidden'  THEN
    DO:
      setQueryEditor('visible').
      APPLY 'entry' TO ficWhere2 IN FRAME frWhere.
    END.

  END CASE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME cbAndOr
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbAndOr C-Win
ON RETURN OF cbAndOr IN FRAME frWhere /* Where */
DO:
  APPLY 'entry' TO cbFields.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbFields
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbFields C-Win
ON RETURN OF cbFields IN FRAME frWhere
DO:
  APPLY 'entry' TO cbOperator.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbOperator
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbOperator C-Win
ON RETURN OF cbOperator IN FRAME frWhere
DO:
  APPLY 'entry' TO ficValue.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME CtrlFrame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL CtrlFrame C-Win OCX.Tick
PROCEDURE CtrlFrame.PSTimer.Tick .
/*------------------------------------------------------------------------------
    Name : pstimer.ocx.tick
    Desc : Execute timed procedure and schedule the next one
  ------------------------------------------------------------------------------*/

  /* Find the timer that caused the event */
  DEFINE BUFFER bTimer FOR ttTimer.

  /* Turn off events when we're running */
  IF NOT glUseTimer THEN RETURN.

  chCtrlFrame:pstimer:ENABLED = FALSE.

  /* No timer stuff in debug mode */
  IF glDebugMode THEN RETURN.

  FIND FIRST bTimer NO-ERROR.
  IF AVAILABLE bTimer THEN
  DO:
    /* Run the proc */
    RUN VALUE(bTimer.cProc).

    /* When should it run again */
    IF AVAILABLE bTimer THEN
      bTimer.tNext = ADD-INTERVAL(NOW, bTimer.iTime,"milliseconds").
  END.

  /* Schedule the next event to run */
  RUN SetTimerInterval.

END PROCEDURE. /* OCX.psTimer.Tick */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME ficValue
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficValue C-Win
ON ENTRY OF ficValue IN FRAME frWhere
DO:
  IF SELF:screen-value = "" THEN
    SELF:screen-value = getLinkInfo(cbFields:screen-value).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME ficWhere
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficWhere C-Win
ON ALT-CURSOR-DOWN OF ficWhere IN FRAME frMain
DO:
  setQueryEditor('visible').
  APPLY 'entry' TO ficWhere2 IN FRAME frWhere.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficWhere C-Win
ON PAGE-DOWN OF ficWhere IN FRAME frMain
OR "CHOOSE" OF btnPrevQuery
OR "ALT-CURSOR-LEFT" OF ficWhere
OR "CURSOR-DOWN" OF ficWhere
DO:
  PUBLISH "setUsage" ("navigateQuery"). /* user behaviour */
  setQuery(+1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficWhere C-Win
ON PAGE-UP OF ficWhere IN FRAME frMain
OR "CHOOSE" OF btnNextQuery
OR "ALT-CURSOR-RIGHT" OF ficWhere
OR "CURSOR-UP" OF ficWhere
DO:
  PUBLISH "setUsage" ("navigateQuery"). /* user behaviour */
  setQuery(-1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficWhere C-Win
ON RETURN OF ficWhere IN FRAME frMain
DO:
  /* If the editor is small, interpret an ENTER as CTRL-ENTER */
  IF gcQueryEditorState = 'hidden' THEN
  DO:
    APPLY 'choose' TO btnViewData.
    RETURN NO-APPLY.
  END.
  ELSE
    SELF:insert-string ( '~n' ) .
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME ficWhere2
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficWhere2 C-Win
ON ALT-CURSOR-UP OF ficWhere2 IN FRAME frWhere
DO:
  setQueryEditor('hidden').
  APPLY 'entry' TO ficWhere IN FRAME frMain.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME fiFeedback
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiFeedback C-Win
ON MOUSE-SELECT-CLICK OF fiFeedback IN FRAME frMain
DO:
  OS-COMMAND NO-WAIT START VALUE("https://datadigger.wordpress.com/contact/").
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fiIndexNameFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON ANY-PRINTABLE OF fiIndexNameFilter IN FRAME frMain
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
DO:
  setFilterFieldColor(SELF:handle).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON CURSOR-DOWN OF fiIndexNameFilter IN FRAME frMain
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
DO:
  setFilterFieldColor(SELF:HANDLE).
  ghLastIndexFilter = SELF.
  APPLY 'entry' TO brIndexes.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON ENTRY OF fiIndexNameFilter IN FRAME frMain
, fiTableFilter
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
DO:
  /* If you enter the field and you have not put in a filter,
   * clear out the field so you can type something yourself
   */
  IF SELF:screen-value = SELF:private-data THEN
    SELF:screen-value = ''.

  setFilterFieldColor(SELF:handle).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON LEAVE OF fiIndexNameFilter IN FRAME frMain
, fiTableFilter
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
DO:
  IF SELF:SCREEN-VALUE = '' THEN SELF:SCREEN-VALUE = SELF:PRIVATE-DATA.
  setFilterFieldColor(SELF:handle).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON RETURN OF fiIndexNameFilter IN FRAME frMain
, fiFlagsFilter, fiFieldsFilter
DO:
  RUN reopenIndexBrowse(?,?). /* reopen, while maintaining original sort */
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON SHIFT-DEL OF fiIndexNameFilter IN FRAME frMain
, fiFlagsFilter, fiFieldsFilter
DO:
  APPLY 'choose' TO btnClearIndexFilter.
  SELF:screen-value = ''.
  APPLY 'value-changed' TO SELF.
  APPLY 'entry' TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frData
&Scoped-define SELF-NAME fiNumRecords
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiNumRecords C-Win
ON MOUSE-SELECT-DBLCLICK OF fiNumRecords IN FRAME frData
DO:
  DEFINE VARIABLE hQuery  AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hBuffer AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cQuery  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTable  AS CHARACTER   NO-UNDO.

  IF NOT VALID-HANDLE(ghDataBrowse) THEN RETURN.
  SESSION:SET-WAIT-STATE('general').

  /* Change query to a PRESELECT query to get number of rows */
  cQuery = ghDataBrowse:QUERY:prepare-string.
  ENTRY(1,cQuery,' ') = 'preselect'.

  /* Create buffer for the table we selected in the data browse */
  cTable = ghDataBrowse:QUERY:get-buffer-handle(1):dbname + '.' + ghDataBrowse:QUERY:get-buffer-handle(1):name.

  CREATE QUERY hQuery.
  CREATE BUFFER hBuffer FOR TABLE ghDataBrowse:QUERY:get-buffer-handle(1):dbname + '.' + ghDataBrowse:QUERY:get-buffer-handle(1):name NO-ERROR.

  hQuery:SET-BUFFERS(hBuffer).
  hQuery:QUERY-PREPARE(cQuery).
  hQuery:QUERY-OPEN.

  SESSION:SET-WAIT-STATE('').

  /* Num results of query */
  RUN showNumRecords(hQuery:NUM-RESULTS, YES).
  RUN showNumSelected.

  hQuery:QUERY-CLOSE.

  DELETE OBJECT hQuery.
  DELETE OBJECT hBuffer.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fiNumSelected
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiNumSelected C-Win
ON MOUSE-SELECT-DBLCLICK OF fiNumSelected IN FRAME frData
DO:

  IF ghDataBrowse:NUM-SELECTED-ROWS = 0 THEN
    RUN dataSelectAll.
  ELSE
    RUN dataSelectNone.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME fiTableFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiTableFilter C-Win
ON CURSOR-DOWN OF fiTableFilter IN FRAME frMain
DO:
  setFilterFieldColor(SELF:handle).
  APPLY 'entry' TO brTables.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiTableFilter C-Win
ON RETURN OF fiTableFilter IN FRAME frMain
, cbDatabaseFilter
DO:
  RUN filterTables.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiTableFilter C-Win
ON SHIFT-DEL OF fiTableFilter IN FRAME frMain
, cbDatabaseFilter, cbDatabaseFilter
DO:
  APPLY 'choose' TO btnClearTableFilter.
  SELF:SCREEN-VALUE = ''.
  APPLY 'value-changed' TO SELF.
  APPLY 'entry' TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiTableFilter C-Win
ON VALUE-CHANGED OF fiTableFilter IN FRAME frMain
, cbDatabaseFilter
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
DO:
  DEFINE VARIABLE cSetting AS CHARACTER NO-UNDO.

  /* Save last used database */
  IF SELF:NAME = "cbDatabaseFilter" THEN
  DO:
    cSetting = cbDatabaseFilter:SCREEN-VALUE IN FRAME frMain.
    IF cSetting = ? THEN cSetting = "<empty>".
    setRegistry("DataDigger", "Database", cSetting ).
  END.

  /* Schedule the correct timer */
  IF LOOKUP(SELF:NAME,"fiTableFilter,cbDatabaseFilter") > 0 THEN
    RUN setTimer("timedTableFilter", 300).
  ELSE
  IF LOOKUP(SELF:NAME,"fiIndexNameFilter,fiFlagsFilter,fiFieldsFilter") > 0 THEN
    RUN setTimer("timedIndexFilter", 300).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_Clone_this_Database
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_Clone_this_Database C-Win
ON CHOOSE OF MENU-ITEM m_Clone_this_Database /* Clone this Database */
DO:
  DEFINE VARIABLE cNewDatabaseList AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldDatabaseList AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLogicalName     AS CHARACTER   NO-UNDO.

  DO WITH FRAME frMain:
    CREATE ALIAS dictdb FOR DATABASE VALUE( gcCurrentDatabase ).

    /* Get list of connected databases */
    cOldDatabaseList = getDatabaseList().

    RUN VALUE(getProgramDir() + 'dCloneDatabase.w')
     ( INPUT-OUTPUT gcCurrentDatabase
     , INPUT SUBSTITUTE("x=&1,y=&2", brTables:x + 10, brTables:y + 50)
     , OUTPUT cLogicalName
     ).

    /* Get all connected databases */
    cNewDatabaseList = getDatabaseList().

    IF cNewDatabaseList <> cOldDatabaseList THEN
    /* Get list of all tables of all databases */
    RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
    ASSIGN cbDatabaseFilter:LIST-ITEMS IN FRAME frMain = ',' + cNewDatabaseList.

    IF LOOKUP(cLogicalName,cNewDatabaseList) > 0 THEN
    DO:
      cbDatabaseFilter:SCREEN-VALUE = cLogicalName.
      APPLY 'value-changed' TO cbDatabaseFilter.
    END.

  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_Disconnect
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_Disconnect C-Win
ON CHOOSE OF MENU-ITEM m_Disconnect /* Disconnect */
OR "-",DELETE-CHARACTER OF cbDatabaseFilter
DO:
  RUN disconnectDatabase.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_Dump_table_DF
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_Dump_table_DF C-Win
ON CHOOSE OF MENU-ITEM m_Dump_table_DF /* Dump definitions */
DO:

  DO WITH FRAME frMain:

    CREATE ALIAS dictdb FOR DATABASE VALUE( gcCurrentDatabase ).

    RUN VALUE(getProgramDir() + 'dDumpDf.w')
     ( INPUT gcCurrentDatabase
     , INPUT gcCurrentTable
     , INPUT SUBSTITUTE("x=&1,y=&2", brTables:x + 10, brTables:y + 50)
     ).
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_Quick_Connect
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_Quick_Connect C-Win
ON CHOOSE OF MENU-ITEM m_Quick_Connect /* Quick Connect */
OR '+', INSERT-MODE OF cbDatabaseFilter
DO:
  DEFINE VARIABLE cPhysicalName AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLogicalName  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTypes        AS CHARACTER   NO-UNDO INITIAL 'PROGRESS'.
  DEFINE VARIABLE cDatabases    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iNumDbs       AS INTEGER     NO-UNDO.

  iNumDbs = NUM-DBS.

  RUN adecomm\_dbconn.p ( INPUT-OUTPUT cPhysicalName
                        , INPUT-OUTPUT cLogicalName
                        , INPUT-OUTPUT cTypes
                        ).

  IF NUM-DBS = iNumDbs THEN RETURN. /* nothing connected */

  /* Get list of all tables of all databases */
  RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).

  /* Get all connected databases */
  cDatabases = getDatabaseList().
  cbDatabaseFilter:LIST-ITEMS = ',' + cDatabases.
  cbDatabaseFilter:SCREEN-VALUE = cLogicalName.
  APPLY 'value-changed' TO cbDatabaseFilter.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_View_as_Excel
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_View_as_Excel C-Win
ON CHOOSE OF MENU-ITEM m_View_as_Excel /* View as Excel */
DO:
  RUN setViewType('XLS').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_View_as_HTML
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_View_as_HTML C-Win
ON CHOOSE OF MENU-ITEM m_View_as_HTML /* View as HTML */
DO:
  RUN setViewType('HTML').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_View_as_text
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_View_as_text C-Win
ON CHOOSE OF MENU-ITEM m_View_as_text /* View as TEXT */
DO:
  RUN setViewType('TXT').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tgDebugMode
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgDebugMode C-Win
ON VALUE-CHANGED OF tgDebugMode IN FRAME frMain
DO:

  setDebugMode(SELF:checked).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tgSelAll
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgSelAll C-Win
ON VALUE-CHANGED OF tgSelAll IN FRAME frMain
DO:

  RUN tgSelAllChoose(SELF:CHECKED).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brFields
&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK C-Win 


SESSION:DEBUG-ALERT = YES.

/* ***************************  Main Block  *************************** */
RUN startDiggerLib.

/* More than one DataDigger window can be open. The
 * startup procedure can demand that all windows are
 * closed. For example, when an update is done
 */
SUBSCRIBE TO 'DataDiggerClose' ANYWHERE.

/* If we started the DataDigger from within DWP and we stop
 * DWP, then exit the DataDigger as well.
 * Requested by Jeroen Stam from NetSetup 30-3-2012
 */
SUBSCRIBE TO "dwp_stop" ANYWHERE RUN-PROCEDURE 'DataDiggerClose'.

/* Save queries in a temp-table for the query tester */
SUBSCRIBE TO "query" ANYWHERE RUN-PROCEDURE "processQuery".

/* Avoid drawing */
/* {&WINDOW-NAME}:Y              = -1000. /* out of sight */ */
{&WINDOW-NAME}:VISIBLE           = YES. /* otherwise lockwindow complains */
{&WINDOW-NAME}:HIDDEN            = YES. /* otherwise lockwindow complains */
{&WINDOW-NAME}:MAX-WIDTH-PIXELS  = ?.
{&WINDOW-NAME}:MAX-HEIGHT-PIXELS = ?.

{&WINDOW-NAME}:WIDTH-PIXELS  = 200.
{&WINDOW-NAME}:HEIGHT-PIXELS = 100.

/* For initializing, center the main window */
{&WINDOW-NAME}:X = (SESSION:WORK-AREA-WIDTH-PIXELS - {&WINDOW-NAME}:WIDTH-PIXELS) / 2.
{&WINDOW-NAME}:Y = (SESSION:WORK-AREA-HEIGHT-PIXELS - {&WINDOW-NAME}:HEIGHT-PIXELS) / 2.

/* Show a message that we're busy setting stuff up */
DEFINE VARIABLE winWait AS HANDLE NO-UNDO.
RUN showMessage.p("DataDigger", "Digging the schema, please wait", OUTPUT winWait).

setWindowFreeze(YES).

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME}
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE
DO:
  DEFINE VARIABLE cSetting AS CHARACTER NO-UNDO.

  /* If we click this in the middle of the tour, ignore it */
  IF FRAME frHint:visible THEN RETURN NO-APPLY.

  /* Cancel all running timer events */
  IF glUseTimer THEN
    chCtrlFrame:pstimer:ENABLED = FALSE.

  /* Save size and position of the window */
  RUN saveWindow.

  cSetting = cbDatabaseFilter:screen-value IN FRAME {&frame-name}.
  IF cSetting = ? THEN cSetting = '<empty>'.
  setRegistry("DataDigger", "Database", cSetting ).

  /* Notify launcher that the window closes */
  PUBLISH 'DataDigger'(-1).

  RUN disable_UI.
END. /* CLOSE OF THIS-PROCEDURE  */


ON 'menu-drop' OF MENU POPUP-MENU-brTables
DO:
  PUBLISH "setUsage" ("TableMenu"). /* user behaviour */
  RUN setConnectionMenu.
END.


ON ENTRY OF ttField.cFormat IN BROWSE brFields
DO:
  DO WITH FRAME {&FRAME-NAME}:
    DEFINE VARIABLE cOrgValue AS CHARACTER NO-UNDO.

    PUBLISH "setUsage" ("setFormat"). /* user behaviour */

    APPLY "ENTRY" TO SELF. /* to get focus */
    cOrgValue = brFields:QUERY:GET-BUFFER-HANDLE(1):BUFFER-FIELD('cFormatOrg'):BUFFER-VALUE.
    glRowEditActive = YES.
    SELF:PRIVATE-DATA = SELF:SCREEN-VALUE.

    IF cOrgValue <> SELF:SCREEN-VALUE THEN
    DO:
      fiWarning:x            = 300.
      fiWarning:y            = SELF:y + brFields:y - 2.
      fiWarning:visible      = YES.
      fiWarning:screen-value = SUBSTITUTE('Original format: &1', cOrgValue).
      fiWarning:width        = LENGTH(fiWarning:screen-value) + 1.
      fiWarning:x            = SELF:x - fiWarning:width-pixels + brFields:x - 10.
    END.

    /* Set a flag for reopenDataBrowse to indicate that the browse must be rebuilt */
    glFormatChanged = TRUE.

    RETURN NO-APPLY.
  END.
END. /* on entry of ttField.cFormat */

ON LEAVE OF ttField.cFormat IN BROWSE brFields
DO:
  DO WITH FRAME {&FRAME-NAME}:
    DEFINE VARIABLE cOrgValue AS CHARACTER NO-UNDO.
    DEFINE VARIABLE cTable    AS CHARACTER NO-UNDO.
    DEFINE VARIABLE cField    AS CHARACTER NO-UNDO.

    fiWarning:VISIBLE = NO.
    fiWarning:X = 1.

    cTable    = gcCurrentTable.
    cField    = brFields:QUERY:GET-BUFFER-HANDLE(1):BUFFER-FIELD('cFieldName'):BUFFER-VALUE.
    cOrgValue = brFields:QUERY:GET-BUFFER-HANDLE(1):BUFFER-FIELD('cFormatOrg'):BUFFER-VALUE.
    glRowEditActive = NO.

    IF SELF:SCREEN-VALUE = '' THEN SELF:SCREEN-VALUE = cOrgValue.
    SELF:FGCOLOR = (IF SELF:SCREEN-VALUE <> cOrgValue THEN getColor('CustomFormat:FG') ELSE ?).

    /* Save changed format. If it is blank, it will be deleted from registry */
    setRegistry( SUBSTITUTE("DB:&1",gcCurrentDatabase)
               , SUBSTITUTE("&1.&2:format",cTable,cField)
               , IF SELF:SCREEN-VALUE <> cOrgValue THEN SELF:SCREEN-VALUE ELSE ?
               ).

    /* Track if format is blanked to set it back to default */
    IF SELF:SCREEN-VALUE = "" THEN
      PUBLISH "setUsage" ("restoreFormat"). /* user behaviour */

    /* Set a flag for reopenDataBrowse to indicate that the browse must be rebuilt */
    /* glFormatChanged = true. */
    /*RUN uncacheTable(gcCurrentDatabase,gcCurrentTable). */
  END.
END. /* on leave of ttField.cFormat */


ON CTRL-TAB OF C-Win ANYWHERE /* DataDigger */
DO:
  PUBLISH "setUsage" ("setPage"). /* user behaviour */
  CASE giCurrentPage:
    WHEN {&PAGE-TABLES} THEN RUN setPage({&PAGE-FAVOURITES}).
    WHEN {&PAGE-FAVOURITES} THEN RUN setPage({&PAGE-TABLES}).
  END CASE.
END. /* CTRL-TAB OF C-Win anywhere */


ON "CTRL-ALT-D" OF C-Win ANYWHERE /* Debugger */
DO:
  PUBLISH "setUsage" ("debugger"). /* user behaviour */
  RUN value(getProgramDir() + "wDebugger.w") PERSISTENT.
END.


/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  /* Notify launcher that the window started */
  PUBLISH 'DataDigger'(+1).

  RUN initializeUi.
  RUN initializeObjects.

  setWindowFreeze(NO).

  /* Clear wait-message */
  DELETE WIDGET winWait.
  {&WINDOW-NAME}:HIDDEN = NO.

  PUBLISH "timerCommand" ("stop", "Startup").
  RUN startSession.

  APPLY 'entry' TO fiTableFilter.

  /* Auto-start DD on selected text */
  RUN setTable(?).

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
  DO:
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
    QUIT. /* does this work??? */
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnAddChoose C-Win 
PROCEDURE btnAddChoose :
/* Add new record
 */
  DEFINE VARIABLE lRecordsUpdated AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE rNewRecord      AS ROWID     NO-UNDO.

  DEFINE BUFFER ttField FOR ttField.

  /* Keep track of user behaviour */
  PUBLISH "setUsage" ("AddRecord").

  /* In read-only mode, return */
  IF plReadOnlyDigger THEN RETURN.

  RUN VALUE(getProgramDir() + 'wEdit.w')
    ( INPUT plReadOnlyDigger
    , INPUT 'Add'
    , INPUT ghDataBrowse
    , INPUT gcCurrentDatabase
    , INPUT gcCurrentTable
    , INPUT TABLE ttField  /* do not use by-reference */
    , INPUT TABLE ttColumn /* do not use by-reference */
    , OUTPUT lRecordsUpdated
    , OUTPUT rNewRecord
    ).

  IF lRecordsUpdated = TRUE THEN
  DO:
    RUN reopenDataBrowse.
    ghDataBrowse:QUERY:REPOSITION-TO-ROWID(rNewRecord).
  END.

END PROCEDURE. /* btnAddChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnClearDataFilterChoose C-Win 
PROCEDURE btnClearDataFilterChoose :
/* Clear filters and reopen data browse
 */
  DEFINE BUFFER bColumn FOR ttColumn.

  FOR EACH bColumn:
    IF VALID-HANDLE(bColumn.hFilter) THEN
    DO:
      ASSIGN
        bColumn.hFilter:MODIFIED     = NO
        bColumn.hFilter:SCREEN-VALUE = bColumn.hFilter:PRIVATE-DATA.

      RUN filterFieldValueChanged(bColumn.hFilter,NO).
      setFilterFieldColor(bColumn.hFilter).
    END.
  END.

  RUN reopenDataBrowse.

END PROCEDURE. /* btnClearDataFilterChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnClearFieldFilterChoose C-Win 
PROCEDURE btnClearFieldFilterChoose :
/*
 * Clear field filters and set focus to field browse
 */
  RUN clearFieldFilter.
  RUN filterFieldsBrowse.
  APPLY "entry" TO ttField.cFieldName IN BROWSE brFields.

END PROCEDURE. /* btnClearFieldFilterChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnClearIndexFilterChoose C-Win 
PROCEDURE btnClearIndexFilterChoose :
/*
 * Clear index filters
 */
  PUBLISH "setUsage" ("clearIndexFilter"). /* user behaviour */

  RUN clearIndexFilter.
  RUN filterIndexBrowse.

  APPLY 'entry' TO brIndexes IN FRAME {&FRAME-NAME}.

END PROCEDURE. /* btnClearIndexFilterChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnCloneChoose C-Win 
PROCEDURE btnCloneChoose :
/* Copy the current record and edit it.
 */
  DEFINE VARIABLE lRecordsUpdated AS LOGICAL NO-UNDO.
  DEFINE VARIABLE rNewRecord      AS ROWID   NO-UNDO.
  DEFINE BUFFER ttField FOR ttField.

  /* In read-only mode, return */
  IF plReadOnlyDigger THEN RETURN.

  /* If no data then go back */
  IF ghDataBrowse:QUERY:NUM-RESULTS = 0
    OR ghDataBrowse:QUERY:NUM-RESULTS = ? 
    OR NOT CAN-FIND(FIRST ttField WHERE ttField.lShow = TRUE) THEN RETURN.

  /* If there is no record selected, select the focused one */
  IF ghDataBrowse:NUM-SELECTED-ROWS = 0 THEN
    ghDataBrowse:SELECT-FOCUSED-ROW().

  IF NOT ghDataBrowse:QUERY:get-buffer-handle(1):available THEN
  DO:
    RUN showHelp('RecordGone', '').
    ghDataBrowse:REFRESH().
    RETURN.
  END.

  RUN value(getProgramDir() + 'wEdit.w')
    ( INPUT plReadOnlyDigger
    , INPUT 'Clone'
    , INPUT ghDataBrowse
    , INPUT gcCurrentDatabase
    , INPUT gcCurrentTable
    , INPUT table ttField  /* do not use by-reference ! */
    , INPUT table ttColumn /* do not use by-reference ! */
    , OUTPUT lRecordsUpdated
    , OUTPUT rNewRecord
    ).

  IF lRecordsUpdated = TRUE THEN
  DO:
    ghDataBrowse:QUERY:reposition-to-rowid(rNewRecord) NO-ERROR.
    ghDataBrowse:SELECT-FOCUSED-ROW().
    RUN reopenDataBrowse.
  END.

END PROCEDURE. /* btnCloneChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnDataSortChoose C-Win 
PROCEDURE btnDataSortChoose :
/* Set sorting for data browse
 */
  DEFINE VARIABLE lSortChanged AS LOGICAL NO-UNDO.

  RUN VALUE(getProgramDir() + 'dSorting.w')
     ( INPUT TABLE ttColumn
     , INPUT-OUTPUT TABLE ttQuerySort
     , OUTPUT lSortChanged
     ).

  IF lSortChanged THEN RUN reopenDataBrowse.

END PROCEDURE. /* btnDataSortChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnDeleteChoose C-Win 
PROCEDURE btnDeleteChoose :
/* Delete selected records
 */
  DEFINE VARIABLE iCount          AS INTEGER NO-UNDO.
  DEFINE VARIABLE hBuffer         AS HANDLE  NO-UNDO.
  DEFINE VARIABLE hFileBuffer     AS HANDLE  NO-UNDO.
  DEFINE VARIABLE lContinue       AS LOGICAL NO-UNDO.
  DEFINE VARIABLE lDeleted        AS LOGICAL NO-UNDO.
  DEFINE VARIABLE lError          AS LOGICAL NO-UNDO.
  DEFINE VARIABLE lEnableTriggers AS LOGICAL NO-UNDO.

  /* In read-only mode, return */
  IF plReadOnlyDigger THEN RETURN.

  /* If nothing selected, go back */
  IF ghDataBrowse:NUM-SELECTED-ROWS = 0 
    OR NOT CAN-FIND(FIRST ttField WHERE ttField.lShow = TRUE) THEN RETURN.

  /* Prohibit editing of VST records */
  IF gcCurrentTable BEGINS '_' THEN
  DO:
    RUN showHelp('CannotEditVst', '').
    RETURN.
  END.

  RUN showHelp('ConfirmDelete', STRING(ghDataBrowse:NUM-SELECTED-ROWS)).
  IF getRegistry('DataDigger:help', 'ConfirmDelete:answer') <> '1' THEN RETURN.

  /* Dump the record as a backup */
  RUN dumpRecord( INPUT 'Delete'
                , INPUT ghDataBrowse
                , OUTPUT lContinue
                ).
  IF NOT lContinue THEN RETURN.

  setWindowFreeze(YES).
  SESSION:SET-WAIT-STATE("general").

  lEnableTriggers = LOGICAL(getRegistry("DataDigger","EnableDeleteTriggers")).
  IF lEnableTriggers = ? THEN lEnableTriggers = NO.

  /* Do the delete */
  DO iCount = 1 TO ghDataBrowse:NUM-SELECTED-ROWS:
    ghDataBrowse:FETCH-SELECTED-ROW(iCount).

    DO TRANSACTION:
      ghDataBrowse:QUERY:GET-CURRENT(EXCLUSIVE-LOCK).
      hBuffer = ghDataBrowse:QUERY:GET-BUFFER-HANDLE().

      /* 2012-09-14 JEE Disable triggers depending on toggle */
      IF NOT lEnableTriggers THEN
      DO:
        hBuffer:DISABLE-LOAD-TRIGGERS(FALSE).
        hBuffer:DISABLE-DUMP-TRIGGERS( ).
      END.

      hBuffer:BUFFER-DELETE() NO-ERROR.
      /* Records with dictionary validations cannot be dynamically deleted.
       * In that case, build a delete procedure and compile it on the fly
       */
      IF hBuffer:AVAILABLE THEN
      DO:
        /* That is, if you have a full version of Progress */
        IF PROGRESS = "FULL" THEN
          RUN deleteRecord( gcCurrentDatabase
                          , gcCurrentTable
                          , hBuffer:ROWID
                          , lEnableTriggers
                          , OUTPUT lDeleted
                          ).
        ELSE lDeleted = FALSE.
      END.
      ELSE lDeleted = TRUE.

    END.

    /* 2012-09-14 JEE User interaction outside of transaction; only to next record if record is deleted */
    IF NOT lDeleted THEN lError = TRUE.
    ghDataBrowse:QUERY:GET-NEXT(NO-LOCK).
  END.

  setWindowFreeze(NO).
  SESSION:SET-WAIT-STATE("").

  /* 20141119: Not deleted because of validations + runtime version */
  IF lError THEN
    IF PROGRESS <> "FULL" THEN
    DO:
      CREATE BUFFER hFileBuffer FOR TABLE gcCurrentDatabase + "._file".
      hFileBuffer:FIND-UNIQUE(SUBSTITUTE("WHERE _file-name = &1", QUOTER(gcCurrentTable))).

      MESSAGE
        SUBSTITUTE("Your table &1.&2 contains a validation expression:", gcCurrentDatabase, gcCurrentTable)
        SKIP(1) hFileBuffer::_valexp
        SKIP(1) "Therefore, it cannot be dynamically deleted. Normally,"
        SKIP    "DataDigger would generate a small program and compile "
        SKIP    "it but your Progress version does not allow this.     "
        SKIP(1) "So I'm sorry, but the" STRING(ghDataBrowse:NUM-SELECTED-ROWS = 1,"record/records") "could not be deleted :("
        VIEW-AS ALERT-BOX INFO BUTTONS OK.

      DELETE OBJECT hFileBuffer.
    END.
    ELSE
      MESSAGE 'Sorry, could not delete record.' VIEW-AS ALERT-BOX INFO BUTTONS OK.

  RUN reopenDataBrowse.

END PROCEDURE. /* btnDeleteChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnDumpChoose C-Win 
PROCEDURE btnDumpChoose :
/* Dump selected records
 */
  DEFINE VARIABLE cSetting       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldDateFormat AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldNumFormat  AS CHARACTER   NO-UNDO.

  DEFINE BUFFER ttField FOR ttField.

  /* Prevent illegal calls */
  IF NOT VALID-HANDLE(ghDataBrowse) THEN RETURN.

  /* If no data then go back */
  IF ghDataBrowse:QUERY:num-results = 0
    OR ghDataBrowse:QUERY:num-results = ? THEN RETURN.

  /* If there is no record selected, select the focused one */
  IF VALID-HANDLE(ghDataBrowse)
    AND ghDataBrowse:NUM-SELECTED-ROWS = 0 THEN
    ghDataBrowse:SELECT-FOCUSED-ROW().

  /* When you start DataDigger from more than 1 environment, chances are
   * that you might start with different date-format settings. The dump
   * window saves the date of the last dump in the session:date-format
   * so this needs to be consistent throughout all runs of DataDigger.
   */
  /* 2011-09-14 JEE Same goes for numeric format */
  ASSIGN
    cOldDateFormat = SESSION:DATE-FORMAT
    cOldNumFormat  = SESSION:NUMERIC-FORMAT
    .

  /* Check Date-format in ini file */
  cSetting = getRegistry('DataDigger', 'DateFormat').

  CASE cSetting:
    WHEN ? THEN setRegistry('DataDigger', 'DateFormat', SESSION:DATE-FORMAT).
    WHEN SESSION:DATE-FORMAT THEN .
    OTHERWISE SESSION:DATE-FORMAT = cSetting.
  END CASE.

  RUN value(getProgramDir() + 'wDump.w')
    ( INPUT ghDataBrowse
    , INPUT getSelectedFields()
    , INPUT table ttField BY-REFERENCE
    ).

  /* Restore date format */
  ASSIGN
    SESSION:DATE-FORMAT     = cOldDateFormat
    SESSION:NUMERIC-FORMAT  = cOldNumFormat
    .

END PROCEDURE. /* btnDumpChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnEditChoose C-Win 
PROCEDURE btnEditChoose :
/* Edit one or more records in a separate window
 */
  DEFINE VARIABLE lRecordsUpdated AS LOGICAL NO-UNDO.
  DEFINE VARIABLE rNewRecord      AS ROWID   NO-UNDO.
  DEFINE BUFFER ttField FOR ttField.

  /* If no data then go back */
  IF ghDataBrowse:QUERY:NUM-RESULTS = 0
    OR ghDataBrowse:QUERY:NUM-RESULTS = ? 
    OR NOT CAN-FIND(FIRST ttField WHERE ttField.lShow = TRUE) THEN RETURN.

  /* If there is no record selected, select the focused one */
  IF ghDataBrowse:NUM-SELECTED-ROWS = 0 THEN
    ghDataBrowse:SELECT-FOCUSED-ROW().

  IF NOT ghDataBrowse:QUERY:get-buffer-handle(1):available THEN
  DO:
    RUN showHelp('RecordGone', '').
    ghDataBrowse:REFRESH().
    RETURN.
  END.

  RUN VALUE(getProgramDir() + 'wEdit.w')
    ( INPUT plReadOnlyDigger
    , INPUT 'Edit'
    , INPUT ghDataBrowse
    , INPUT gcCurrentDatabase
    , INPUT gcCurrentTable
    , INPUT table ttField  /* do not use by-reference ! */
    , INPUT table ttColumn /* do not use by-reference ! */
    , OUTPUT lRecordsUpdated
    , OUTPUT rNewRecord /* not handled here */
    ).

  IF lRecordsUpdated THEN ghDataBrowse:REFRESH().

END PROCEDURE. /* btnEditChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnFavouritesChoose C-Win 
PROCEDURE btnFavouritesChoose :
/* Maintenance of database connection settings
 */
  DEFINE VARIABLE cDummy        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cProgDir      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabases    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabasesOld AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cCurrentDb    AS CHARACTER   NO-UNDO.

  cProgDir   = getProgramDir().
  cCurrentDb = gcCurrentDatabase.

  cDatabasesOld = getDatabaseList().
  RUN VALUE(cProgDir + 'wConnections.w') (INPUT 'UI', INPUT '', OUTPUT cDummy).

  /* Get all connected databases */
  cDatabases = getDatabaseList().

  /* If needed, repopulate db combo */
  IF cDatabases <> cDatabasesOld THEN
  DO:
    /* Get list of all tables of all databases */
    RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
    ASSIGN cbDatabaseFilter:LIST-ITEMS IN FRAME frMain = ',' + cDatabases.

    RUN filterTables.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnLoadChoose C-Win 
PROCEDURE btnLoadChoose :
/* Load data into table
 */
  DEFINE VARIABLE lRecordsUpdated AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE rNewRecord      AS ROWID     NO-UNDO.

  DEFINE BUFFER ttField FOR ttField.

  /* Keep track of user behaviour */
  PUBLISH "setUsage" ("LoadData").

  RUN VALUE(getProgramDir() + 'wImportSel.w')
    ( INPUT plReadOnlyDigger
    , INPUT gcCurrentDatabase
    , INPUT gcCurrentTable
    , INPUT TABLE ttField  /* do not use by-reference */
    , INPUT TABLE ttColumn /* do not use by-reference */
    , OUTPUT lRecordsUpdated
    , OUTPUT rNewRecord
    ).

  IF lRecordsUpdated = TRUE THEN
  DO:
    RUN reopenDataBrowse.
    ghDataBrowse:QUERY:REPOSITION-TO-ROWID(rNewRecord) NO-ERROR.
  END.

END PROCEDURE. /* btnLoadChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnQueriesChoose C-Win 
PROCEDURE btnQueriesChoose :
/* Maintenance of database connection settings
 */
  DEFINE VARIABLE iQuery  AS INTEGER NO-UNDO.
  DEFINE VARIABLE hEditor AS HANDLE  NO-UNDO.

  hEditor = getActiveQueryEditor().

  RUN value(getProgramDir() + 'dQueries.w')
    ( INPUT gcCurrentDatabase
    , INPUT gcCurrentTable
    , INPUT hEditor:SCREEN-VALUE
    , OUTPUT iQuery
    ).

  IF iQuery = ? THEN
    RETURN.
  ELSE
  DO:
    /* Queries might be changed, so reload them */
    RUN collectQueryInfo
      ( INPUT gcCurrentDatabase
      , INPUT gcCurrentTable
      ).
    giQueryPointer = iQuery.
    setQuery(0).
  END.

END PROCEDURE. /* btnQueriesChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnSettingsChoose C-Win 
PROCEDURE btnSettingsChoose :
/* Show DataDigger settings window
 */
  DEFINE VARIABLE cEnvironment AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lOkClicked   AS LOGICAL     NO-UNDO.

  /* Load or create personalized ini file */
  cEnvironment = SUBSTITUTE('&1DataDigger-&2.ini'
                           , getProgramDir()
                           , getUserName()
                           ).

  /* Save window pos & size because the initializeObject will reset it to its last known
   * position and size. That might be differentfrom the actual position of the window.
   * The window would flash and move on the screen.
   */
  RUN saveWindow.

  RUN VALUE(getProgramDir() + '\wSettings.w')
     ( INPUT cEnvironment
     , OUTPUT lOkClicked
     ).

  IF lOkClicked THEN
  DO:
    setWindowFreeze(YES).
    RUN clearRegistryCache.
    RUN initializeObjects.

    gcCurrentTable = ?.
    APPLY "value-changed" TO brTables IN FRAME frMain.
    setWindowFreeze(NO).
  END.

END PROCEDURE. /* btnSettingsChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnViewChoose C-Win 
PROCEDURE btnViewChoose :
/* Show a record in a more readable format in a new window.
 */
  DEFINE VARIABLE cDataEnd     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDataFormat  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDataStart   AS CHARACTER   NO-UNDO EXTENT 2.
  DEFINE VARIABLE cDocEnd      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDocStart    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFilename    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFileType    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLabelEnd    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLabelStart  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLineEnd     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLineStart   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hDataBuffer  AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iFieldExtent AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iLineNr      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMaxWidth    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iRecord      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iRowNr       AS INTEGER     NO-UNDO.

  DEFINE BUFFER bView  FOR ttView.
  DEFINE BUFFER bField FOR ttField.

  /* If there is no record selected, select the focused one */
  IF ghDataBrowse:NUM-SELECTED-ROWS = 0 THEN
    ghDataBrowse:SELECT-FOCUSED-ROW().

  IF NOT ghDataBrowse:QUERY:GET-BUFFER-HANDLE(1):AVAILABLE THEN
  DO:
    RUN showHelp('RecordGone', '').
    ghDataBrowse:REFRESH().
    RETURN.
  END.

  /* What type do we want? */
  cFileType = getRegistry('DataDigger', 'ViewType').

  /* Cleanup */
  EMPTY TEMP-TABLE bView.
  EMPTY TEMP-TABLE ttColumnWidth.

  /* Get data */
  IF NOT VALID-HANDLE(ghDataBrowse) THEN RETURN.
  hDataBuffer = ghDataBrowse:QUERY:GET-BUFFER-HANDLE(1).
  IF NOT hDataBuffer:AVAILABLE THEN RETURN.

  collectLoop:
  FOR EACH bField
    WHERE bField.lShow      = TRUE
    BREAK BY bField.iOrder
          BY bField.iExtent:

    /* Move it one down */
    iRowNr = iRowNr + 1.

    /* Label is the first column, so HOR=0 */
    CREATE bView.
    ASSIGN bView.iHor   = 0
           bView.iVer   = iRowNr
           bView.cValue = bField.cFullName
           .

    /* Walk thru all selected records */
    DO iRecord = 1 TO ghDataBrowse:NUM-SELECTED-ROWS:
      ghDataBrowse:FETCH-SELECTED-ROW(iRecord).

      CREATE bView.
      ASSIGN bView.iHor   = iRecord
             bView.iVer   = iRowNr
             bView.cValue = TRIM(STRING(hDataBuffer:BUFFER-FIELD(bField.cFieldName):BUFFER-VALUE(bField.iExtent), bField.cFormat )) NO-ERROR.
             .

      /* Time-formatted fields */
      IF bField.cFormat BEGINS "HH:MM" THEN
      DO:
        /* Try to format in time format */
        bView.cValue = TRIM(STRING(INTEGER(hDataBuffer:BUFFER-FIELD(bField.cFieldName):BUFFER-VALUE(bField.iExtent)), bField.cFormat )) NO-ERROR.

        /* If you type a crappy time format like HH:MAM:SS just ignore it */
        IF ERROR-STATUS:ERROR THEN
          bView.cValue = STRING(hDataBuffer:BUFFER-FIELD(bField.cFieldName):BUFFER-VALUE(bField.iExtent)).
      END.

      IF bField.cFieldName = 'RECID' THEN
        bView.cValue = STRING(hDataBuffer:RECID).

      IF bField.cFieldName = 'ROWID' THEN
        bView.cValue = STRING(hDataBuffer:ROWID).

    END. /* iRecord */
  END. /* for each bField */

  /* Calculate maximum width per column */
  DO iRecord = 0 TO ghDataBrowse:NUM-SELECTED-ROWS:

    /* Find out maximum width of all elements in this col */
    iMaxWidth = 1.
    FOR EACH bView WHERE bView.iHor = iRecord:
      /* Correct cValue for unknown values */
      bView.cValue = SUBSTITUTE('&1',bView.cValue).
      iMaxWidth = MAXIMUM(iMaxWidth, LENGTH(bView.cValue)).
    END.

    CREATE ttColumnWidth.
    ASSIGN ttColumnWidth.iHor   = iRecord
           ttColumnWidth.iWidth = iMaxWidth.
  END.

  /* Determine a unique filename
   * Something like: datadigger-view-customer.txt
   */
  cFilename = SUBSTITUTE('&1datadigger-view.&2', SESSION:TEMP-DIR, cFileType ).

  /* Showtime! */
  IF SEARCH(cFileName) <> ?
    AND isFileLocked(cFileName) THEN
  DO:
    MESSAGE 'Error opening temporary file.~nDo you have it open for editing?~n~n' cFilename VIEW-AS ALERT-BOX INFO BUTTONS OK.
    RETURN.
  END.

  OUTPUT TO VALUE( cFilename ).

  CASE cFileType:
    WHEN 'txt'  THEN ASSIGN cDocStart     = ''
                            cLineStart    = '~n'
                            cLabelStart   = ''      cLabelEnd  = ' = '
                            cDataStart[1] = ''      cDataEnd   = ' | '
                            cDataStart[2] = ''
                            cLineEnd      = ''
                            cDocEnd       = ''
                            .
    WHEN 'xls' OR
    WHEN 'html' THEN ASSIGN cDocStart     = '<html><body><table border=1>'
                            cLineStart    = '~n<tr>'
                            cLabelStart   = '<td bgcolor="KHAKI"><b>'         cLabelEnd   = '</b></td>'
                            cDataStart[1] = '<td bgcolor="LIGHTYELLOW">'      cDataEnd    = '&nbsp;</td>'
                            cDataStart[2] = '<td bgcolor="WHITE">'
                            cLineEnd      = '</td> </tr>'
                            cDocEnd       = '~n</table></body></html>'
                            .
  END CASE.

  PUT UNFORMATTED cDocStart.
  FOR EACH bView
    BREAK BY bView.iVer BY bView.iHor:

    /* Determine format for data to get names aligned */
    FIND ttColumnWidth WHERE ttColumnWidth.iHor = bView.iHor.
    cDataFormat = FILL('x', ttColumnWidth.iWidth).

    IF FIRST-OF(bView.iVer) THEN
    DO:
      iLineNr = iLineNr MOD 2 + 1.
      PUT UNFORMATTED cLineStart cLabelStart STRING(bView.cValue,cDataFormat) cLabelEnd.
    END.
    ELSE
      PUT UNFORMATTED cDataStart[iLineNr] STRING(bView.cValue,cDataFormat) cDataEnd.

    IF LAST-OF(bView.iVer) THEN
      PUT UNFORMATTED cLineEnd.
  END.
  PUT UNFORMATTED cDocEnd.
  OUTPUT CLOSE.

  /* Start associated program for the txt file */
  OS-COMMAND NO-WAIT START VALUE(cFilename).

  /* Cleanup */
  EMPTY TEMP-TABLE bView.
  EMPTY TEMP-TABLE ttColumnWidth.

END PROCEDURE. /* btnViewChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE checkFonts C-Win 
PROCEDURE checkFonts :
/* If the default fonts have been messed up, try to set the fonts to reasonable settings.
 */
  {&timerStart}
  DEFINE VARIABLE iFont     AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cFontName AS CHARACTER NO-UNDO.

  /*
   * If we want DD to manage the fonts itself
   * -- OR --
   * If the default fonts have been changed and the fonts
   * are not specified, try to set them for the user.
   */
  IF LOGICAL(getRegistry("DataDigger:fonts","AutoSetFont")) = TRUE
    OR ( isDefaultFontsChanged()
        AND ( getRegistry("DataDigger:fonts","default") = ? OR getRegistry("DataDigger:fonts","fixed") = ? )
       ) THEN
  DO:
    /*
     * Try to find fonts:
     *
     * Proportional: "MS Sans Serif, size=8"
     * Fixed       : "Courier New, size=8"
     *
     * Mind that "size=8" might also be "size ", so
     * search with and without "="
     *
     * Alternatively, if font not found:
     *
     * Proportional: first font that starts with "MS Sans Serif"
     * Fixed       : first font that starts with "Courier New"
     *
     * If still nothing found, give a warning as last resort.
     */
    EMPTY TEMP-TABLE ttFont.

    checkFont:
    DO iFont = 0 TO FONT-TABLE:NUM-ENTRIES - 1:
      CREATE ttFont.
      ASSIGN ttFont.iFontNr = iFont.

      GET-KEY-VALUE SECTION "fonts" KEY "font" + STRING(iFont) VALUE ttFont.cFontName.
    END. /* checkFont */

    /* Set default font */
    FIND FIRST ttFont WHERE ttFont.cFontName MATCHES "MS Sans Serif, size*8" NO-ERROR.
    IF NOT AVAILABLE ttFont THEN
      FIND FIRST ttFont WHERE ttFont.cFontName BEGINS "MS Sans Serif" NO-ERROR.
    IF AVAILABLE ttFont THEN setRegistry("DataDigger:fonts","default",STRING(ttFont.iFontNr)).

    /* Set fixed font */
    FIND FIRST ttFont WHERE ttFont.cFontName MATCHES "Courier New, size*8" NO-ERROR.
    IF NOT AVAILABLE ttFont THEN
      FIND FIRST ttFont WHERE ttFont.cFontName BEGINS "Courier New" NO-ERROR.
    IF AVAILABLE ttFont THEN setRegistry("DataDigger:fonts","fixed",STRING(ttFont.iFontNr)).

    /* Clean up; records are no longer needed */
    EMPTY TEMP-TABLE ttFont.

    /* Now, check again to see if we found both fonts and have been able to set them */
    IF   getRegistry("DataDigger:fonts","default") = ?
      OR getRegistry("DataDigger:fonts","fixed") = ?  THEN
    DO:
      RUN showHelp("FontsChanged","").

      /* Don't accept a choice "YES" in combination with HIDDEN=YES because
       * then the help will pop up everytime automatically. Kinda annoying.
       */
      setRegistry( "DataDigger:help", "FontsChanged:answer","2").
    END.
  END.

  /* Get user defined Default font and fixed-size font */
  giDefaultFont = getFont("Default").
  giFixedFont   = getFont("Fixed").

  /* Make the font table large enough to hold at least 24 fonts */
  IF FONT-TABLE:NUM-ENTRIES < 24 THEN FONT-TABLE:NUM-ENTRIES = 24.

  {&timerStop}
END PROCEDURE. /* checkFonts */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearDataFilter C-Win 
PROCEDURE clearDataFilter :
/* clear the Data Filters
 */
  DEFINE INPUT PARAMETER phFilterField AS HANDLE NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.

  FIND bColumn WHERE bColumn.hFilter = phFilterField NO-ERROR.
  IF AVAILABLE bColumn THEN
    setRegistry( SUBSTITUTE("DB:&1",gcCurrentDatabase )
               , SUBSTITUTE("&1.&2:FilterHistory", gcCurrentTable, bColumn.cFullName)
               , ?
               ).

  phFilterField:LIST-ITEMS = "".

END PROCEDURE. /* clearDataFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearDataSort C-Win 
PROCEDURE clearDataSort :
/*
 * Remove data column sorting and reopen browse
 */
 FOR EACH ttQuerySort WHERE ttQuerySort.iGroup = 2:
   DELETE ttQuerySort.
 END.

 RUN reopenDataBrowse.

END PROCEDURE. /* clearDataSort */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearField C-Win 
PROCEDURE clearField :
/* Clear a field
 */
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.
  phWidget:SCREEN-VALUE = "".
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearFieldFilter C-Win 
PROCEDURE clearFieldFilter :
/* Reset the field filters to the blank values
 */
 DEFINE VARIABLE hFilter AS HANDLE  NO-UNDO.
 DEFINE VARIABLE iFilter AS INTEGER NO-UNDO.

 PUBLISH "setUsage" ("clearFieldFilter"). /* user behaviour */

 DO iFilter = 1 TO NUM-ENTRIES(gcFieldFilterHandles):
   hFilter = WIDGET-HANDLE(ENTRY(iFilter, gcFieldFilterHandles)) NO-ERROR.
   IF VALID-HANDLE(hFilter) AND hFilter:TYPE <> "Toggle-Box" THEN
   DO:
     ASSIGN
       hFilter:MODIFIED     = NO
       hFilter:SCREEN-VALUE = hFilter:PRIVATE-DATA.

     setFilterFieldColor(hFilter).
   END.
 END.

END PROCEDURE. /* clearFieldFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearIndexFilter C-Win 
PROCEDURE clearIndexFilter :
/* Reset the index filters to the blank values
 */
  DO WITH FRAME frMain:
    fiIndexNameFilter:screen-value = fiIndexNameFilter:private-data.
    fiFlagsFilter    :screen-value = fiFlagsFilter    :private-data.
    fiFieldsFilter   :screen-value = fiFieldsFilter   :private-data.

    setFilterFieldColor(fiIndexNameFilter:handle).
    setFilterFieldColor(fiFlagsFilter    :handle).
    setFilterFieldColor(fiFieldsFilter   :handle).
  END.

END PROCEDURE. /* clearIndexFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE collectFieldInfo C-Win 
PROCEDURE collectFieldInfo PRIVATE :
/* Fill the fields temp-table
 */
  DEFINE INPUT PARAMETER pcTableName AS CHARACTER NO-UNDO.

  DEFINE BUFFER ttField FOR ttField.
  DEFINE BUFFER ttTable FOR ttTable.

  /* Collect fields from target table */
  RUN getFields( INPUT gcCurrentDatabase
               , INPUT pcTableName
               , OUTPUT DATASET dsFields
               ).

  FIND ttTable
    WHERE ttTable.cDatabase  = gcCurrentDatabase
      AND ttTable.cTableName = pcTableName.

  ASSIGN ttTable.lCached = TRUE.

END PROCEDURE. /* collectFieldInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE collectIndexInfo C-Win 
PROCEDURE collectIndexInfo :
/* Fill the index temp-table
 */
  {&timerStart}
  DEFINE INPUT PARAMETER pcTableName   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE hBufferFile       AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hBufferIndex      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hBufferIndexField AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hBufferField      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cQuery            AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hQuery            AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cCurrentDatabase  AS CHARACTER   NO-UNDO. /* NelsonAlcala */

  DEFINE BUFFER bIndex FOR ttIndex.

  /* Return if no db connected */
  IF NUM-DBS = 0 THEN RETURN.

  /* NelsonAlcala */
  cCurrentDatabase = SDBNAME(gcCurrentDatabase).  /*use DB schemaholder name*/
  /* Fill the tt with _Fields */
                                           /* NelsonAlcala */
  CREATE BUFFER hBufferFile       FOR TABLE /*g*/ cCurrentDatabase + "._File".
  CREATE BUFFER hBufferIndex      FOR TABLE /*g*/ cCurrentDatabase + "._Index".
  CREATE BUFFER hBufferIndexField FOR TABLE /*g*/ cCurrentDatabase + "._Index-Field".
  CREATE BUFFER hBufferField      FOR TABLE /*g*/ cCurrentDatabase + "._Field".

  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hBufferFile,hBufferIndex,hBufferIndexField,hBufferField).

  cQuery = SUBSTITUTE("for each &1._File  where &1._file._file-name = '&2' no-lock " +
                      "  , each &1._Index       of &1._File        no-lock " +
                      "  , each &1._Index-field of &1._Index       no-lock " +
                      "  , each &1._Field       of &1._Index-field no-lock where true "
                     , cCurrentDatabase /* NelsonAlcala */
                     , pcTableName
                     ).

  hQuery:QUERY-PREPARE(cQuery).
  EMPTY TEMP-TABLE bIndex.
  hQuery:QUERY-OPEN().
  hQuery:GET-FIRST().

  REPEAT WHILE NOT hQuery:QUERY-OFF-END:

    FIND bIndex WHERE bIndex.cIndexName = hBufferIndex:BUFFER-FIELD('_index-name'):buffer-value NO-ERROR.
    IF NOT AVAILABLE bIndex THEN
    DO:
      CREATE bIndex.

      bIndex.cIndexName  = hBufferIndex:BUFFER-FIELD('_index-name'):buffer-value.
      bIndex.cIndexFlags = STRING( hBufferFile:BUFFER-FIELD('_prime-index'):buffer-value = hBufferIndex:RECID, 'P/')
                          + string( hBufferIndex:BUFFER-FIELD('_unique'):buffer-value, ' U/')
                          + string( hBufferIndex:BUFFER-FIELD('_WordIdx'):buffer-value <> ?, ' W/')
                          + string( hBufferIndex:BUFFER-FIELD('_Active'):buffer-value , ' /INACTIVE')
                          .
      bIndex.cIndexFlags  = TRIM(bIndex.cIndexFlags).
      bIndex.lIndexActive = hBufferIndex:BUFFER-FIELD('_Active'):buffer-value.
    END.

    /* Add field */
    bIndex.cIndexFields = SUBSTITUTE('&1  &2&3'
                                     , bIndex.cIndexFields
                                     , hBufferField:BUFFER-FIELD('_field-name'):buffer-value
                                     , STRING( hBufferIndexField:BUFFER-FIELD('_Ascending'):buffer-value, '+/-')
                                     ).
    bIndex.cIndexFields = TRIM(bIndex.cIndexFields,' ').

    /* Naked list of just fieldnames */
    bIndex.cFieldList   = SUBSTITUTE('&1,&2'
                                     , bIndex.cFieldList
                                     , hBufferField:BUFFER-FIELD('_field-name'):buffer-value
                                     ).
    bIndex.cFieldList   = TRIM(bIndex.cFieldList,', ').

    hQuery:GET-NEXT().
  END.
  hQuery:QUERY-CLOSE().

  DELETE OBJECT hQuery.
  DELETE OBJECT hBufferFile.
  DELETE OBJECT hBufferIndex.
  DELETE OBJECT hBufferIndexField.
  DELETE OBJECT hBufferField.

  {&timerStop}
END PROCEDURE. /* collectIndexInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE connectDatabase C-Win 
PROCEDURE connectDatabase :
/* Quick-Connect to a database via the context menu
 */
  DEFINE INPUT PARAMETER pcDatabase AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cError        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cProgDir      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabases    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabasesOld AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cCurrentDb    AS CHARACTER   NO-UNDO.

  DO WITH FRAME {&frame-name}:
    cProgDir   = getProgramDir().
    cCurrentDb = gcCurrentDatabase.

    cDatabasesOld = getDatabaseList().
    RUN value(cProgDir + 'wConnections.w') (INPUT 'CONNECT', INPUT pcDatabase, OUTPUT cError).
    IF cError <> '' THEN
      MESSAGE cError VIEW-AS ALERT-BOX INFO BUTTONS OK.

    /* Get all connected databases */
    cDatabases = getDatabaseList().

    /* If needed, repopulate db combo */
    IF cDatabases <> cDatabasesOld THEN
    DO:
      ASSIGN
        cbDatabaseFilter:list-items   = ',' + cDatabases
        cbDatabaseFilter:screen-value = cCurrentDb
        .

      /* Get list of all tables of all databases */
      RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
    END.

    /* If the chosen DB is connected, switch to that one */
    IF LOOKUP(pcDatabase, cDatabases) > 0 THEN
      cbDatabaseFilter:screen-value = pcDatabase.

    APPLY 'value-changed' TO cbDatabaseFilter.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE connectDroppedDatabase C-Win 
PROCEDURE connectDroppedDatabase :
/* Quick-Connect to a drag-and-dropped database
 */
  DEFINE INPUT PARAMETER pcDatabase AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cDatabaseList AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iDatabase     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cLdbName      AS CHARACTER   NO-UNDO.

  /* Accept one database at a time */
  pcDatabase = ENTRY(1,pcDatabase,'~n').

  /* Check if the name is already in use */
  cLdbName = ENTRY(NUM-ENTRIES(pcDatabase,'\'),pcDatabase,'\').
  cLdbName = REPLACE(cLdbName,'.db','').

  /* Add a nr if the ldbname is already in use, we draw a line at 10 */
  IF CONNECTED(cLdbName) THEN
  REPEAT iDatabase = 2 TO 10:
    IF NOT CONNECTED(cLdbName + STRING(iDatabase)) THEN
    DO:
      cLdbName = cLdbName + STRING(iDatabase).
      LEAVE.
    END.
  END.

  /* Try to connect to the database single user */
  CONNECT VALUE(pcDatabase) VALUE(SUBSTITUTE('-ld &1 -1', cLdbName)) NO-ERROR.
  IF ERROR-STATUS:ERROR THEN
  DO:
    MESSAGE ERROR-STATUS:GET-MESSAGE(1) VIEW-AS ALERT-BOX INFO BUTTONS OK.
    RETURN.
  END.

  /* Get all connected databases */
  DO WITH FRAME frMain:
    cDatabaseList = getDatabaseList().
    ASSIGN
      cbDatabaseFilter:LIST-ITEMS   = ',' + cDatabaseList
      cbDatabaseFilter:SCREEN-VALUE = LDBNAME(NUM-DBS).

    /* Get list of all tables of all databases */
    SESSION:SET-WAIT-STATE('general').
    RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
    SESSION:SET-WAIT-STATE('').
  END.

  /* If the chosen DB is connected, switch to that one */
  IF LOOKUP(pcDatabase, cDatabaseList) > 0 THEN
    cbDatabaseFilter:SCREEN-VALUE = LDBNAME(NUM-DBS).

  APPLY 'value-changed' TO cbDatabaseFilter.

END PROCEDURE. /* connectDroppedDatabase */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE connectParamFile C-Win 
PROCEDURE connectParamFile :
/* Quick-Connect to a drag-and-dropped database
 */
  DEFINE INPUT PARAMETER pcParamfile AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cDatabaseList AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iDb           AS INTEGER     NO-UNDO.

  RUN getLdbsFromParamFile(INPUT pcParamfile, OUTPUT cDatabaseList).

  IF cDatabaseList <> '' THEN
  DO:
    RUN showHelp("DisconnectGroup", cDatabaseList).
    IF getRegistry("DataDigger:help", "DisconnectGroup:answer") <> "1" THEN RETURN.

    DO iDb = 1 TO NUM-ENTRIES(cDatabaseList):
      DISCONNECT VALUE(ENTRY(iDb,cDatabaseList)).
    END.
  END.

  CONNECT -pf VALUE(pcParamFile).

  /* Get all connected databases */
  DO WITH FRAME frMain:
    cDatabaseList = getDatabaseList().
    ASSIGN
      cbDatabaseFilter:LIST-ITEMS   = ',' + cDatabaseList
      cbDatabaseFilter:SCREEN-VALUE = ''.

    /* Get list of all tables of all databases */
    SESSION:SET-WAIT-STATE('general').
    RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
    SESSION:SET-WAIT-STATE('').
  END.

  APPLY 'value-changed' TO cbDatabaseFilter.

END PROCEDURE. /* connectParamFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE control_load C-Win  _CONTROL-LOAD
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

OCXFile = SEARCH( "wDataDigger.wrx":U ).
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
ELSE MESSAGE "wDataDigger.wrx":U SKIP(1)
             "The binary control file could not be found. The controls cannot be loaded."
             VIEW-AS ALERT-BOX TITLE "Controls Not Loaded".

&ENDIF

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE convertSettings C-Win 
PROCEDURE convertSettings :
/* Do one-time conversions for new versions
 */
  DEFINE INPUT PARAMETER piOldVersion AS INTEGER   NO-UNDO.
  DEFINE INPUT PARAMETER pcOldBuild   AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cFile      AS LONGCHAR  NO-UNDO.
  DEFINE VARIABLE cIniFile   AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cCustomLib AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cLine      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cSection   AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iLine      AS INTEGER   NO-UNDO.
  DEFINE BUFFER bfConfig FOR ttConfig.

  SESSION:SET-WAIT-STATE("general").
  CASE piOldVersion:

    WHEN 19 THEN
    DO:
      /* Obsolete files */
      OS-DELETE getNewVersion.p.
      OS-DELETE getNewVersion.r.
      OS-DELETE frLoadMapping.w.
      OS-DELETE frLoadMapping.r.
      OS-DELETE DataDigger.chm.
      OS-DELETE images/default_ReleaseNotes.gif.
      OS-DELETE images/default_FilterCombo.gif.
      OS-DELETE images/default_FilterComboRed.gif.
      OS-DELETE images/default_Star.gif.
      OS-DELETE images/default_Tables.gif.
      OS-DELETE images/default_PrevQuery.gif.
      OS-DELETE images/default_NextQuery.gif.
      OS-DELETE images/default_ViewAsList.gif.
      OS-DELETE images/default_Pinned.gif.
      OS-DELETE images/default_Unpinned.gif.
      OS-DELETE images/default_Undock.gif.
      OS-DELETE images/default_Dock.gif.
      OS-DELETE images/default_Tab_Favorites_Active.gif.
      OS-DELETE images/default_Tab_Favorites_Inactive.gif.

      /* Erase widths of table browse */
      setRegistry('DataDigger','ColumnWidth:cTableName',?).
      setRegistry('DataDigger','ColumnWidth:cDatabase',?).
      setRegistry('DataDigger','ColumnWidth:iNumQueries',?).

      /* Setting for last active page not used anymore */
      setRegistry("DataDigger", "ActivePage",?).

      /* Remove last used table from settings */
      FOR EACH bfConfig WHERE bfConfig.cSection BEGINS 'DB:' BREAK BY bfConfig.cSection:
        IF FIRST-OF(bfConfig.cSection) THEN setRegistry(bfConfig.cSection,'table',?).
      END.

      /* Updates (are in the general .ini file */
      USE 'DataDigger.ini' NO-ERROR.
      IF NOT ERROR-STATUS:ERROR THEN
      DO:
        PUT-KEY-VALUE SECTION "DataDigger:Update" KEY "UpdateCheck"     VALUE ? NO-ERROR.
        PUT-KEY-VALUE SECTION "DataDigger:Update" KEY "UpdateUrl"       VALUE ? NO-ERROR.
        PUT-KEY-VALUE SECTION "DataDigger:Update" KEY "UpdateChannel"   VALUE ? NO-ERROR.
        PUT-KEY-VALUE SECTION "DataDigger:Update" KEY "UpdateLastCheck" VALUE ? NO-ERROR.
        PUT-KEY-VALUE SECTION "DataDigger:Update" KEY ""                VALUE ? NO-ERROR.
      END.
      USE "".
    END. /* 19 */

    WHEN 20 THEN
    DO:
      OS-DELETE wEdit.wrx.
    END. /* 20 */
    
    WHEN 21 THEN
    DO:
      /* Nothing for version 21 */
    END. /* 21 */
    
    WHEN 22 THEN
    DO:
      OS-DELETE dAbout.w.
      OS-DELETE dAbout.r.
      OS-DELETE image/default_Tab_About_Active.gif.
      OS-DELETE image/default_Tab_About_Inactive.gif.
      OS-DELETE image/default_Tab_Changes_Active.gif.
      OS-DELETE image/default_Tab_Changes_Inactive.gif.
      
      /* A typo in DD19 prevented these from deletion */
      OS-DELETE image/default_ReleaseNotes.gif.
      OS-DELETE image/default_FilterCombo.gif.
      OS-DELETE image/default_FilterComboRed.gif.
      OS-DELETE image/default_Star.gif.
      OS-DELETE image/default_Tables.gif.
      OS-DELETE image/default_PrevQuery.gif.
      OS-DELETE image/default_NextQuery.gif.
      OS-DELETE image/default_ViewAsList.gif.
      OS-DELETE image/default_Pinned.gif.
      OS-DELETE image/default_Unpinned.gif.
      OS-DELETE image/default_Undock.gif.
      OS-DELETE image/default_Dock.gif.
      OS-DELETE image/default_Tab_Favorites_Active.gif.
      OS-DELETE image/default_Tab_Favorites_Inactive.gif.

    END. /* 22 */
    
  END CASE. /* old version */

  SESSION:SET-WAIT-STATE("").

END PROCEDURE. /* convertSettings */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE copyDataToClipboard C-Win 
PROCEDURE copyDataToClipboard :
/* Copy the value of the column to the clipboard
 */
  DEFINE VARIABLE cColumnName  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cColumnValue AS CHARACTER   NO-UNDO.

  IF NUM-ENTRIES(ghDataBrowse:PRIVATE-DATA,CHR(1)) <> 3 THEN RETURN.

  cColumnName  = ENTRY(1, ghDataBrowse:PRIVATE-DATA,CHR(1)).
  cColumnValue = ENTRY(2, ghDataBrowse:PRIVATE-DATA,CHR(1)).

  IF cColumnValue <> '' AND cColumnValue <> ? THEN CLIPBOARD:VALUE = TRIM(cColumnValue).

END PROCEDURE. /* copyDataToClipboard */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE copyToClipboard C-Win 
PROCEDURE copyToClipboard :
/* Copy value to clipboard
 */
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.
  CLIPBOARD:VALUE = phWidget:SCREEN-VALUE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE createMenuDataBrowse C-Win 
PROCEDURE createMenuDataBrowse :
/* Rebuild the connection submenu of the 'add' button
 */
  DEFINE VARIABLE hMenu     AS HANDLE NO-UNDO.
  DEFINE VARIABLE hMenuItem AS HANDLE NO-UNDO.

  hMenu = createMenu(ghDataBrowse).

  /* Copy to clipboard */
  hMenuItem = createMenuItem(hMenu,"Item","Copy to clipboard","copyDataToClipboard").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN copyDataToClipboard IN THIS-PROCEDURE.

  /* Show value of field */
  hMenuItem = createMenuItem(hMenu,"Item","Show Value","showValue").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN showValue IN THIS-PROCEDURE.

  /* Add to filter */
  hMenuItem = createMenuItem(hMenu,"Item","Add to filter","addFilter").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN setDataFilter IN THIS-PROCEDURE (NO).

  /* Filter on this field only */
  hMenuItem = createMenuItem(hMenu,"Item","Set as only filter","setFilter").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN setDataFilter IN THIS-PROCEDURE (YES).

  /* Filter on this field only */
  hMenuItem = createMenuItem(hMenu,"Item","Clear Filters","clearFilter").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnClearDataFilterChoose IN THIS-PROCEDURE.

  /* Set data sorting */
  hMenuItem = createMenuItem(hMenu,"Item","Set Sorting","SortData").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnDataSortChoose IN THIS-PROCEDURE.

  /* Clear sorting */
  hMenuItem = createMenuItem(hMenu,"Item","Clear Sorting","ClearSorting").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN clearDataSort IN THIS-PROCEDURE.

  /* Rule */
  hMenuItem = createMenuItem(hMenu,"Rule","","").

  /* Shortcut to viewing records */
  hMenuItem = createMenuItem(hMenu,"Item","View selected","view").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnViewChoose IN THIS-PROCEDURE.

  /* Shortcut to adding records */
  hMenuItem = createMenuItem(hMenu,"Item","Add record","add").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnAddChoose IN THIS-PROCEDURE.

  /* Shortcut to cloning records */
  hMenuItem = createMenuItem(hMenu,"Item","Clone record","clone").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnCloneChoose IN THIS-PROCEDURE.

  /* Shortcut to editing records */
  hMenuItem = createMenuItem(hMenu,"Item","Edit selected","edit").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnEditChoose IN THIS-PROCEDURE.

  /* Shortcut to dumping records */
  hMenuItem = createMenuItem(hMenu,"Item","Dump selected","dump").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnDumpChoose IN THIS-PROCEDURE.

  /* Shortcut to loading records */
  hMenuItem = createMenuItem(hMenu,"Item","Load data","load").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnLoadChoose IN THIS-PROCEDURE.

  /* Rule */
  hMenuItem = createMenuItem(hMenu,"Rule","","").

  /* Shortcut to hiding the column */
  hMenuItem = createMenuItem(hMenu,"Item","Hide this column","hideColumn").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN hideColumn IN THIS-PROCEDURE.

  /* Shortcut to unhiding the column */
  hMenuItem = createMenuItem(hMenu,"Item","Unhide all columns","unhideColumn").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN showField IN THIS-PROCEDURE('*',TRUE).

  /* Rule */
  hMenuItem = createMenuItem(hMenu,"Rule","","").

  /* Shortcut to deleting records */
  hMenuItem = createMenuItem(hMenu,"Item","Delete selected","delete").
  ON "CHOOSE" OF hMenuItem PERSISTENT RUN btnDeleteChoose IN THIS-PROCEDURE.

END PROCEDURE. /* createMenuDataBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE createSortTable C-Win 
PROCEDURE createSortTable :
/* Create a newly arranged sort table for all sort options
   * from both query and user selected sorts */
  DEFINE BUFFER bfQuerySort FOR ttQuerySort.
  DEFINE BUFFER bfNewSort   FOR ttQuerySort.
  DEFINE VARIABLE iNumSorts AS INTEGER NO-UNDO.

  FOR EACH bfQuerySort WHERE bfQuerySort.iGroup = 0:
    DELETE bfQuerySort.
  END.

  FOR EACH bfQuerySort
    WHERE bfQuerySort.iGroup > 0
       BY bfQuerySort.iGroup
       BY bfQuerySort.iSortNr:

    /* See if the sort already exists, if not create it.
     * This can happen if the sort is in the query and
     * later is manually changed by the user by clicking
     * on the column to reverse the sort.
     */
    FIND bfNewSort
      WHERE bfNewSort.iGroup = 0
        AND bfNewSort.cSortField = bfQuerySort.cSortField
            NO-ERROR.

    IF NOT AVAILABLE bfNewSort THEN
    DO:
      iNumSorts = iNumSorts + 1.

      CREATE bfNewSort.
      ASSIGN
        bfNewSort.iGroup     = 0
        bfNewSort.iSortNr    = iNumSorts
        bfNewSort.cSortField = bfQuerySort.cSortField
        bfNewSort.iExt       = bfQuerySort.iExt
        .
    END.

    ASSIGN bfNewSort.lAscending = bfQuerySort.lAscending.
  END.

END PROCEDURE. /* createSortTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cutToClipboard C-Win 
PROCEDURE cutToClipboard :
/* Copy value to clipboard and delete current value
 */
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.
  CLIPBOARD:VALUE = phWidget:SCREEN-VALUE.
  phWidget:SCREEN-VALUE = "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataColumnResize C-Win 
PROCEDURE dataColumnResize :
/* Resize a data column
 */
  DEFINE INPUT  PARAMETER phColumn AS HANDLE NO-UNDO.

  setRegistry( SUBSTITUTE('DB:&1',gcCurrentDatabase)
             , SUBSTITUTE('&1.&2:width', gcCurrentTable, phColumn:NAME)
             , STRING(phColumn:WIDTH-PIXELS)
             ).

  RUN dataScrollNotify IN this-procedure (ghDataBrowse).

END PROCEDURE. /* dataColumnResize */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataColumnSort C-Win 
PROCEDURE dataColumnSort PRIVATE :
/* Sort on a datacolumn
 */
  DEFINE VARIABLE lAscending AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE cFieldName AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iNumSorts  AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cKeyList   AS CHARACTER   NO-UNDO.

  DEFINE BUFFER bfQuerySort FOR ttQuerySort.
  DEFINE BUFFER bfCurrentSort FOR ttQuerySort.

  cFieldName = SELF:CURRENT-COLUMN:NAME.
  cKeyList = GetKeyList().

  /* If CTRL key is pressed, sorting is extended to include another field.
   * If not, all current user sorting should be cleared
   */
  IF LOOKUP("CTRL", cKeyList) = 0 THEN
  DO:
    /* Delete all user-clicked sorts */
    FOR EACH bfQuerySort WHERE bfQuerySort.iGroup = 2:
      DELETE bfQuerySort.
    END.
  END.

  /* See if there is any sort on this field */
  FIND bfCurrentSort
    WHERE bfCurrentSort.iGroup = 0
      AND bfCurrentSort.cSortField = cFieldName NO-ERROR.

  /* Now find or create the sort on level 2 */
  FIND bfQuerySort
    WHERE bfQuerySort.iGroup = 2
      AND bfQuerySort.cSortField = cFieldName NO-ERROR.

  IF NOT AVAILABLE bfQuerySort THEN
  DO:
    /* Determine nr of sorts */
    FIND LAST bfQuerySort WHERE bfQuerySort.iGroup = 2 NO-ERROR.
    IF AVAILABLE bfQuerySort THEN iNumSorts = bfQuerySort.iSortNr.

    CREATE bfQuerySort.
    ASSIGN
      bfQuerySort.iGroup     = 2
      bfQuerySort.iSortNr    = iNumSorts + 1
      bfQuerySort.cSortField = cFieldName.

    /* Extract extent nr from name */
    IF bfQuerySort.cSortField MATCHES '*[*]' THEN
      bfQuerySort.iExt = INTEGER( ENTRY(1,ENTRY(2,bfQuerySort.cSortField,'['),']') ).
  END.

  IF AVAILABLE bfCurrentSort THEN
    bfQuerySort.lAscending = NOT bfCurrentSort.lAscending.
  ELSE
    bfQuerySort.lAscending = TRUE.

  RUN reopenDataBrowse.

END PROCEDURE. /* dataColumnSort */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DataDiggerClose C-Win 
PROCEDURE DataDiggerClose :
/* Close DataDigger after event 'DataDiggerClose'
 */
  APPLY 'close' TO THIS-PROCEDURE.

END PROCEDURE. /* DataDiggerClose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataDoubleClick C-Win 
PROCEDURE dataDoubleClick :
/* Double click on databrowse might result in EDIT / VIEW / DUMP
 */
  DEFINE INPUT PARAMETER hBrowseBuffer AS HANDLE NO-UNDO.

  /* What to do on double click? */
  CASE getRegistry('DataDigger','DataDoubleClick'):
    WHEN 'VIEW' THEN RUN btnViewChoose.
    WHEN 'EDIT' THEN RUN btnEditChoose.
    WHEN 'DUMP' THEN RUN btnDumpChoose.
  END CASE.

END PROCEDURE. /* dataDoubleClick */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataGotoFilter C-Win 
PROCEDURE dataGotoFilter :
/* Jump from browse straight to the filter fields
 */
        DEFINE BUFFER bColumn FOR ttColumn.

        /* If we have been in the filters before, the name of the last visited
         * filter field is in gcLastDataField. Try to jump back to that field.
         * If it fails (eg made hidden) then jump to the first visible filter
         */
        FIND bColumn WHERE bColumn.cFullName = gcLastDataField NO-ERROR.

        IF NOT AVAILABLE bColumn OR NOT bColumn.hColumn:VISIBLE THEN
        DO:
                FindField:
                FOR EACH bColumn BY bColumn.iColumnNr:
                        IF NOT bColumn.hColumn:VISIBLE THEN NEXT.
                        LEAVE FindField.
                END.
        END.

        IF AVAILABLE bColumn THEN
        DO:
                setFilterFieldColor(bColumn.hFilter).
                APPLY 'entry' TO bColumn.hFilter.
                RETURN NO-APPLY.
        END.

END PROCEDURE. /* dataGotoFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataOffHome C-Win 
PROCEDURE dataOffHome :
/* Show message that this is changed as from DataDigger 21.
         */

        /* Use CTRL-CURSOR-UP / DOWN to jump from filter fields to browse and back */
        RUN showHelp('JumpToFilter', '').

END PROCEDURE. /* dataOffHome */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataRowDisplay C-Win 
PROCEDURE dataRowDisplay :
/* Set the background color to another color to get an odd/even coloring of the rows.
 */
  DEFINE INPUT PARAMETER phBrowseBuffer AS HANDLE NO-UNDO.

  DEFINE BUFFER bColumn     FOR ttColumn.
  DEFINE BUFFER bField      FOR ttField.
  DEFINE BUFFER bfQuerySort FOR ttQuerySort.

  DEFINE VARIABLE cFieldValue    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cCurrentValues AS CHARACTER   NO-UNDO.

  /* Set row coloring based on sorts */
  IF TEMP-TABLE bfQuerySort:HAS-RECORDS THEN
  DO:
    cCurrentValues = ''.
    FOR EACH bfQuerySort WHERE bfQuerySort.iGroup = 0:

      CASE bfQuerySort.cSortField:
        WHEN 'RECID' THEN cFieldValue = STRING(phBrowseBuffer:RECID).
        WHEN 'ROWID' THEN cFieldValue = STRING(phBrowseBuffer:ROWID).
        OTHERWISE cFieldValue = phBrowseBuffer:BUFFER-FIELD(ENTRY(1,bfQuerySort.cSortField,'[')):STRING-VALUE(bfQuerySort.iExt).
      END.

      cCurrentValues = cCurrentValues + '|' + cFieldValue.
    END.
  END.
  ELSE
    cCurrentValues = STRING(phBrowseBuffer:QUERY:CURRENT-RESULT-ROW MODULO 2).

  IF cCurrentValues <> gcPreviousValues THEN
    ASSIGN
      glUseEvenRowColorSet = NOT glUseEvenRowColorSet
      gcPreviousValues = cCurrentValues.

  FOR EACH bColumn, bField WHERE bField.cFieldName = bColumn.cFieldName:
    IF NOT VALID-HANDLE(bColumn.hColumn) THEN NEXT.

    /* Set colors */
    bColumn.hColumn:FGCOLOR = (IF glUseEvenRowColorSet THEN giDataEvenRowColor[1] ELSE giDataOddRowColor[1]).
    bColumn.hColumn:BGCOLOR = (IF glUseEvenRowColorSet THEN giDataEvenRowColor[2] ELSE giDataOddRowColor[2]).

    /* Set font/value for RECID field */
    IF bColumn.cFieldName = "RECID" THEN
    DO:
      bColumn.hColumn:FONT = giFixedFont.
      bColumn.hColumn:SCREEN-VALUE = STRING( phBrowseBuffer:RECID, "zzzzzzzzz9" ).
    END.

    /* Set font/value for ROWID field */
    IF bColumn.cFieldName = "ROWID" THEN
    DO:
      bColumn.hColumn:FONT = giFixedFont.
      bColumn.hColumn:SCREEN-VALUE = STRING(phBrowseBuffer:ROWID, "x(30)").
    END.

    /* Set value for TIME field */
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

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataRowJumpToEnd C-Win 
PROCEDURE dataRowJumpToEnd :
/* End on datarow
 */
  DEFINE INPUT  PARAMETER hBrowseBuffer AS HANDLE      NO-UNDO.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataRowValueChanged C-Win 
PROCEDURE dataRowValueChanged :
/* Save the content of the fields in linkinfo
 */
  DEFINE INPUT  PARAMETER hBrowseBuffer AS HANDLE      NO-UNDO.

  DEFINE VARIABLE iColumn    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE hColumn    AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cFieldName AS CHARACTER   NO-UNDO.

  PUBLISH "debugMessage" (3, SUBSTITUTE("Browse columns: &1", gcDataBrowseColumns)).
  PUBLISH "debugMessage" (3, SUBSTITUTE("Column names  : &1", gcDataBrowseColumnNames)).

  DO iColumn = 1 TO NUM-ENTRIES(gcDataBrowseColumns):

    hColumn    = WIDGET-HANDLE( ENTRY(iColumn,gcDataBrowseColumns) ).
    cFieldName = ENTRY(iColumn,gcDataBrowseColumnNames).

    RUN showNumSelected.

    IF hColumn:SCREEN-VALUE <> "" AND hColumn:SCREEN-VALUE <> ? THEN
      setLinkInfo(cFieldName, hColumn:SCREEN-VALUE).
  END.

  setUpdatePanel(?). /* Refresh sensitivity of buttons if needed */

END PROCEDURE. /* dataRowValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataScrollNotify C-Win 
PROCEDURE dataScrollNotify :
/* Adjust size and position of the filterfields to browse
 */
  DEFINE INPUT PARAMETER phBrowse AS HANDLE NO-UNDO.

  DEFINE VARIABLE cFilterFields AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iColumn       AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cButtons      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iField        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hColumn       AS HANDLE    NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.

  {&timerStart}
  PUBLISH "debugMessage" (1, "scroll-notify of dataBrowse").

  /* Might get called when browse is not yet realized, so: */
  IF NOT VALID-HANDLE(phBrowse) THEN RETURN.

  /* Freeze all */
  setWindowFreeze(YES).

  getFilterLoop:
  FOR EACH bColumn BY bColumn.iColumnNr:
    IF VALID-HANDLE(bColumn.hFilter) THEN
      cFilterFields = TRIM(SUBSTITUTE('&1,&2', cFilterFields, bColumn.hFilter),',').
  END.

  DO WITH FRAME frData:
    cButtons = SUBSTITUTE('&1', btnClearDataFilter:HANDLE).
  END.

  /* Resize them */
  RUN resizeFilterFields
    ( INPUT btnDataSort:HANDLE
    , INPUT cFilterFields
    , INPUT cButtons
    , INPUT phBrowse
    ).

  RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO).
  setWindowFreeze(NO).

  {&timerStop}
  RETURN NO-APPLY.

END PROCEDURE.  /* dataScrollNotify */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataSelectAll C-Win 
PROCEDURE dataSelectAll :
/* Select all records in the browse
 */
  DEFINE INPUT PARAMETER phBrowse AS HANDLE     NO-UNDO.

  setWindowFreeze(YES).
  SESSION:SET-WAIT-STATE('general').
  phBrowse:SELECT-ALL().
  RUN showNumSelected.
  setUpdatePanel('display'). /* Activate buttons */
  setWindowFreeze(NO).
  SESSION:SET-WAIT-STATE('').

END PROCEDURE. /* dataSelectAll */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataSelectNone C-Win 
PROCEDURE dataSelectNone :
/* Deselect all records in the browse
 */
  DEFINE INPUT  PARAMETER phBrowse AS HANDLE     NO-UNDO.

  setWindowFreeze(YES).
  phBrowse:DESELECT-ROWS().
  RUN showNumSelected.
  setUpdatePanel('display'). /* Activate buttons */
  setWindowFreeze(NO).

END PROCEDURE. /* dataSelectNone */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE deleteDataFilters C-Win 
PROCEDURE deleteDataFilters :
/* Kill the data filters and its menu
 */
  {&timerStart}
  DEFINE INPUT PARAMETER phParentBrowse AS HANDLE NO-UNDO.
  DEFINE BUFFER bFilter FOR ttFilter.

  FOR EACH bFilter WHERE bFilter.hBrowse = phParentBrowse:
    IF VALID-HANDLE(bFilter.hFilter:POPUP-MENU) THEN killMenu(bFilter.hFilter:POPUP-MENU).
    DELETE OBJECT bFilter.hFilter NO-ERROR.
    DELETE bFilter.
  END.

  {&timerStop}
END PROCEDURE. /* deleteDataFilters */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE deleteRecord C-Win 
PROCEDURE deleteRecord :
/* Generate a program to delete a record with dictionary validations
 */
  DEFINE INPUT PARAMETER pcDatabase      AS CHARACTER NO-UNDO.
  DEFINE INPUT PARAMETER pcTable         AS CHARACTER NO-UNDO.
  DEFINE INPUT PARAMETER prRowid         AS ROWID     NO-UNDO.
  DEFINE INPUT PARAMETER plEnableTrigger AS LOGICAL   NO-UNDO.
  DEFINE OUTPUT PARAMETER plDeleted      AS LOGICAL   NO-UNDO.

  DEFINE VARIABLE cTempFile AS CHARACTER NO-UNDO.

  cTempFile = SUBSTITUTE('&1delrecord.p', SESSION:TEMP-DIRECTORY).

  OUTPUT to value(cTempFile).
  PUT UNFORMATTED
         SUBSTITUTE('/* ' )
    SKIP SUBSTITUTE(' * Name: delrecord.p ')
    SKIP SUBSTITUTE(' * Desc: generated by DataDigger to delete &1.&2 ', pcDataBase, pcTable)
    SKIP SUBSTITUTE(' * Date: &1 ', NOW )
    SKIP SUBSTITUTE(' */  ' )
    SKIP SUBSTITUTE('     ' )
    SKIP SUBSTITUTE('DEFINE INPUT  PARAMETER prRowid   AS ROWID   NO-UNDO. ' )
    SKIP SUBSTITUTE('DEFINE OUTPUT PARAMETER plDeleted AS LOGICAL NO-UNDO. ' )
    SKIP SUBSTITUTE('  ' ).

  IF NOT plEnableTrigger THEN
    PUT UNFORMATTED
    SKIP SUBSTITUTE('DISABLE TRIGGERS FOR DUMP OF &1.&2.', pcDataBase, pcTable).

  PUT UNFORMATTED
    SKIP SUBSTITUTE('  ' )
    SKIP SUBSTITUTE('/* Find the record to delete */' )
    SKIP SUBSTITUTE('FIND &1.&2 WHERE ROWID(&1.&2) = prRowid EXCLUSIVE-LOCK NO-ERROR NO-WAIT.', pcDataBase, pcTable)
    SKIP SUBSTITUTE('IF NOT AVAILABLE &1.&2 THEN RETURN.                                     ', pcDataBase, pcTable).

  /* If we don't want triggers, we also skip potential static validations */
  IF NOT plEnableTrigger THEN
    PUT UNFORMATTED
    SKIP SUBSTITUTE('DISABLE TRIGGERS FOR DUMP OF &1.&2.     ', pcDataBase, pcTable)
    SKIP SUBSTITUTE('DELETE &1.&2 VALIDATE(YES,"") NO-ERROR. ', pcDataBase, pcTable).
  ELSE
    PUT UNFORMATTED
    SKIP SUBSTITUTE('DELETE &1.&2 NO-ERROR. ', pcDataBase, pcTable).

  PUT UNFORMATTED
    SKIP SUBSTITUTE('  ' )
    SKIP SUBSTITUTE('/* See if its really gone */' )
    SKIP SUBSTITUTE('plDeleted = NOT CAN-FIND(&1.&2 WHERE ROWID(&1.&2) = prRowid).           ', pcDataBase, pcTable)
    SKIP .
  OUTPUT close.

  /* Run generated prog and cleanup */
  RUN VALUE(cTempFile) (INPUT prRowid, OUTPUT plDeleted).
  OS-DELETE value(cTempFile).

END PROCEDURE. /* deleteRecord */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI C-Win  _DEFAULT-DISABLE
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
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(C-Win)
  THEN DELETE WIDGET C-Win.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disconnectDatabase C-Win 
PROCEDURE disconnectDatabase :
/*
 * Disconnect the current database and rebuild table table
 */
 DEFINE VARIABLE cDatabases  AS CHARACTER NO-UNDO.
 DEFINE VARIABLE cCurrentDb  AS CHARACTER NO-UNDO.
 DEFINE VARIABLE lDisconnect AS LOGICAL   NO-UNDO.
 DEFINE VARIABLE hBuffer     AS HANDLE    NO-UNDO.

 DO WITH FRAME {&FRAME-NAME}:
   hBuffer = brTables:QUERY:GET-BUFFER-HANDLE(1).
   IF hBuffer:AVAILABLE THEN
     cCurrentDb = hBuffer::cDatabase.
   ELSE
     RETURN.
 END.

 /* Cannot disconnect "all" */
 IF cCurrentDb = "" OR cCurrentDb = ? THEN RETURN.

 /* Confirm by user */
 RUN showHelp("Disconnect", cCurrentDb).
 IF getRegistry("DataDigger:help", "Disconnect:answer") <> "1" THEN RETURN.

 DISCONNECT VALUE(cCurrentDb).

 ASSIGN
   gcCurrentDatabase = ""
   gcCurrentTable    = "".

 /* Get list of all tables of all databases */
 RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
/*  /* Remove all tables of this db from the "tables" table */ */
/*  FOR EACH ttTable WHERE ttTable.cDatabase = cCurrentDb:     */
/*    DELETE ttTable.                                          */
/*  END.                                                       */
/*  RUN reopenTableBrowse(?). */

 /* Get all connected databases */
 cDatabases = getDatabaseList().
 cbDatabaseFilter:LIST-ITEMS = "," + cDatabases.

 /* Wipe database filter when it's the one that was just disconnected */
 IF cbDatabaseFilter:SCREEN-VALUE = cCurrentDb THEN
   cbDatabaseFilter:SCREEN-VALUE = "".

 /* If we have no db connected, kill the fields tt */
 IF NUM-DBS = 0 THEN
 DO:
   RUN deleteDataFilters(ghDataBrowse).
   IF VALID-HANDLE(ghDataBrowse) AND VALID-HANDLE(ghDataBrowse:QUERY) THEN DELETE OBJECT ghDataBrowse:QUERY NO-ERROR.
   IF VALID-HANDLE(ghDataBrowse) THEN DELETE OBJECT ghDataBrowse NO-ERROR.
   IF VALID-HANDLE(ghLockTable)  THEN DELETE OBJECT ghLockTable  NO-ERROR.
   IF VALID-HANDLE(ghDataBuffer) THEN DELETE OBJECT ghDataBuffer NO-ERROR.

   EMPTY TEMP-TABLE ttField.
   EMPTY TEMP-TABLE ttIndex.

   /* Reopen the queries on Fields and Indexes */
   RUN reopenFieldBrowse(?,?).
   RUN reopenIndexBrowse(?,?).
   setUpdatePanel(?). /* Refresh sensitivity of buttons if needed */
 END.

 APPLY "value-changed" TO brTables.  /* this sets the gcCurrentDatabase */
 RUN filterTables.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE doNothing C-Win 
PROCEDURE doNothing :
/* Wait for an amount of msec
 */
  DEFINE INPUT  PARAMETER piMilliSeconds AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartTime AS INTEGER     NO-UNDO.

  iStartTime = ETIME.
  DO WHILE ETIME < iStartTime + piMilliSeconds:
    PROCESS EVENTS.
  END.

END PROCEDURE. /* doNothing */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dropFieldMenu C-Win 
PROCEDURE dropFieldMenu :
/* Event for opening the field popup-menu on brFields
 */
  DEFINE VARIABLE hEditor    AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFieldName AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFieldType AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cField     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cColumn    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lOk        AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE iOldPos    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iLength    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMouseX    AS INTEGER NO-UNDO.
  DEFINE VARIABLE iMouseY    AS INTEGER NO-UNDO.
  DEFINE VARIABLE iRet       AS INTEGER NO-UNDO.

  /* See if we clicked on the browse column */
  RUN getMouseXY(INPUT brFields:handle IN FRAME frMain, OUTPUT iMouseX, OUTPUT iMouseY).
  IF iMouseY < 18 THEN
  DO:
    RUN SendMessageA (tgSelAll:HWND, 517, 0, 0, OUTPUT iRet).
    RETURN.
  END.

  ELSE
  DO:
    IF NOT brFields:query:get-buffer-handle(1):available THEN RETURN.

    PUBLISH "setUsage" ("showFieldMenu"). /* user behaviour */

    /* Select the row we clicked on */
    RUN selectClickedRow(brFields:handle, OUTPUT lOk, OUTPUT cColumn).
    IF NOT lOk THEN RETURN NO-APPLY.

    hFieldName = brFields:query:get-buffer-handle(1):buffer-field('cFieldName'):handle.
    hFieldType = brFields:query:get-buffer-handle(1):buffer-field('cDataType'):handle.

    IF VALID-HANDLE(hFieldName) THEN
    DO:
      /* If CTRL is pressed, do not insert the linked value */
      cField  = hFieldName:BUFFER-VALUE.

      IF LOOKUP("CTRL", GetKeyList() ) <> 0 OR getLinkInfo(cField) = "" THEN
      DO:
        CASE cField:
          WHEN "RECID" THEN cField = SUBSTITUTE('RECID(&1)', gcCurrentTable).
          WHEN "ROWID" THEN cField = SUBSTITUTE('ROWID(&1)', gcCurrentTable).
          OTHERWISE cField  = hFieldName:BUFFER-VALUE.
        END CASE.
      END.

      ELSE
      DO:
        /* In case of RECID / ROWID insert proper syntax */
        CASE cField:
          WHEN "RECID" THEN cField = SUBSTITUTE('RECID(&1) = &2', gcCurrentTable, QUOTER(getLinkInfo(cField))).
          WHEN "ROWID" THEN cField = SUBSTITUTE('ROWID(&1) = TO-ROWID(&2)', gcCurrentTable, QUOTER(getLinkInfo(cField))).
          OTHERWISE cField = SUBSTITUTE('&1 = &2', cField, QUOTER(getLinkInfo(cField))).
        END.
      END.

      iLength = LENGTH(cField).

      /* If the query editor is expanded, do actions to that field */
      hEditor = getActiveQueryEditor().

      /* Remember old position for positioning cursor */
      iOldPos = hEditor:CURSOR-OFFSET.

      /* No text selected */
      IF hEditor:SELECTION-TEXT = "" THEN
      DO:
        /* If ficWhere only holds the text <empty> then delete that */
        IF hEditor:SCREEN-VALUE = '<empty>' THEN hEditor:SCREEN-VALUE = ''.
        hEditor:INSERT-STRING(cField).
      END.
      ELSE
      DO:
        hEditor:REPLACE-SELECTION-TEXT(cField).
      END.

      APPLY "entry" TO hEditor.
      hEditor:CURSOR-OFFSET = iOldPos + iLength.
    END.

    RETURN NO-APPLY.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI C-Win  _DEFAULT-ENABLE
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
  RUN control_load.
  DISPLAY fiTableFilter cbDatabaseFilter tgSelAll fiIndexNameFilter 
          fiFlagsFilter fiFieldsFilter fiTableDesc ficWhere fiFeedback 
      WITH FRAME frMain IN WINDOW C-Win.
  ENABLE rctQuery btnTools rctEdit fiTableFilter cbDatabaseFilter 
         btnClearTableFilter btnTableFilter tgSelAll btnClearFieldFilter 
         fiIndexNameFilter fiFlagsFilter fiFieldsFilter btnClearIndexFilter 
         tgDebugMode brTables brFields btnMoveTop brIndexes btnMoveUp btnReset 
         btnMoveDown btnMoveBottom fiTableDesc btnWhere btnClear btnQueries 
         btnFavourite btnClipboard ficWhere btnNextQuery btnPrevQuery btnDump 
         btnLoad btnTabFavourites btnTabFields btnTabIndexes btnTabTables 
         btnDelete btnResizeVer btnClone btnView btnAdd btnEdit fiFeedback 
      WITH FRAME frMain IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frMain}
  ENABLE btnSettings-txt btnDataDigger btnConnections btnSettings btnProcEdit 
         btnDict btnDataAdmin btnQueries-3 btnQueryTester btnHelp btnAbout 
         btnAbout-txt btnConnections-txt btnDataAdmin-txt btnDataDigger-txt 
         btnDict-txt btnHelp-txt btnProcEdit-txt btnQueries-txt 
         btnQueryTester-txt 
      WITH FRAME frSettings IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frSettings}
  DISPLAY edHint 
      WITH FRAME frHint IN WINDOW C-Win.
  ENABLE edHint btGotIt 
      WITH FRAME frHint IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frHint}
  DISPLAY fiNumSelected fiNumRecords 
      WITH FRAME frData IN WINDOW C-Win.
  ENABLE rctData btnDataSort btnClearDataFilter fiNumSelected fiNumRecords 
      WITH FRAME frData IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frData}
  DISPLAY cbAndOr cbFields cbOperator ficValue ficWhere2 
      WITH FRAME frWhere IN WINDOW C-Win.
  ENABLE btnBegins rctQueryButtons cbAndOr cbFields cbOperator ficValue 
         btnInsert ficWhere2 btnClear-2 btnQueries-2 btnClipboard-2 btnOK 
         btnCancel-2 btnOr btnAnd btnBracket btnContains btnEq btnGT btnLT 
         btnMatches btnNE btnQt btnToday 
      WITH FRAME frWhere IN WINDOW C-Win.
  VIEW FRAME frWhere IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frWhere}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE endResize C-Win 
PROCEDURE endResize :
/* Event handler for resize of window
 */
  {&timerStart}
  DEFINE VARIABLE iButtonSpacingX AS INTEGER    NO-UNDO.
  DEFINE VARIABLE iButtonSpacingY AS INTEGER    NO-UNDO.
  DEFINE VARIABLE hWidget         AS HANDLE     NO-UNDO.

  DEFINE BUFFER ttField FOR ttField.

  /* To catch resize errors */
  DO ON ERROR UNDO, LEAVE:
    setWindowFreeze(YES).

    /* Set max width */
    IF C-Win:WIDTH > 384 THEN C-Win:WIDTH = 384.

    /* Set frame width */
    FRAME {&FRAME-NAME}:WIDTH-PIXELS = C-Win:FULL-WIDTH-PIXELS NO-ERROR.
    FRAME {&FRAME-NAME}:HEIGHT-PIXELS = C-Win:FULL-HEIGHT-PIXELS NO-ERROR.

    /* Sanity checks */
    IF btnResizeVer:Y < 150 THEN btnResizeVer:Y = 150.
    IF btnResizeVer:Y > (C-Win:HEIGHT-PIXELS - 200) THEN btnResizeVer:Y = C-Win:HEIGHT-PIXELS - 200.

    /* Park widgets at a safe place, so making the window smaller will
     * not results in errors for widgets not being placeable
     */
    IF VALID-HANDLE(ghDataBrowse) THEN
    ASSIGN
      ghDataBrowse:WIDTH-PIXELS  = 100
      ghDataBrowse:HEIGHT-PIXELS = 100
      ghDataBrowse:Y = 1
      ghDataBrowse:X = 1 NO-ERROR.

    fiFeedback:X = 1.
    fiFeedback:Y = 1.

    /* Set width of main rectangles */
    ASSIGN
      rctQuery:WIDTH-PIXELS  = C-Win:WIDTH-PIXELS - 4
      rctQuery:X             = 0
      rctQuery:Y             = 0
      rctQuery:HEIGHT-PIXELS = btnResizeVer:Y + 32
      NO-ERROR.

    /* Buttons DD / Tools / Help */
    ASSIGN
      btnResizeVer:WIDTH-PIXELS = rctQuery:WIDTH-PIXELS
      btnResizeVer:X = 0
      NO-ERROR.

    /* Table browse */
    ASSIGN
      rcTableFilter:X = rctQuery:X + 20
      rcTableFilter:Y = rctQuery:Y + 24
      rcTableFilter:WIDTH-PIXELS = 226
      rcTableFilter:HEIGHT-PIXELS = btnResizeVer:Y - rcTableFilter:Y

      brTables:X = rcTableFilter:X + 3
      brTables:Y = rcTableFilter:Y + 3
      brTables:WIDTH-PIXELS = rcTableFilter:WIDTH-PIXELS - 6
      brTables:HEIGHT-PIXELS = rcTableFilter:HEIGHT-PIXELS - 6 - fiTableDesc:HEIGHT-PIXELS
      btnTabTables:X = brTables:X - 23
      btnTabFavourites:X = brTables:X - 23

      fiTableDesc:X = brTables:X
      fiTableDesc:Y = brTables:Y + brTables:HEIGHT-PIXELS
      fiTableDesc:WIDTH-PIXELS = brTables:WIDTH-PIXELS - btnFavourite:WIDTH-PIXELS

      btnFavourite:X = fiTableDesc:X + fiTableDesc:WIDTH-PIXELS
      btnFavourite:Y = fiTableDesc:Y
      NO-ERROR.

    /* Data */
    DO WITH FRAME frData:

      /* Prepare embedding frame. First make small to avoid errors */
      ASSIGN
        FRAME frData:WIDTH-PIXELS  = 100
        FRAME frData:HEIGHT-PIXELS = 100
        FRAME frData:X = 0
        FRAME frData:Y = rctQuery:Y + rctQuery:HEIGHT-PIXELS + 2
        FRAME frData:WIDTH-PIXELS  = rctQuery:WIDTH-PIXELS + 4
        FRAME frData:HEIGHT-PIXELS = C-Win:HEIGHT-PIXELS - rctQuery:HEIGHT-PIXELS - 34
        NO-ERROR.

      /* Make small to prevent errors */
      ASSIGN
        rctData:WIDTH-PIXELS        = 1
        rctData:HEIGHT-PIXELS       = 1
        rctDataFilter:WIDTH-PIXELS  = 1
        rctDataFilter:HEIGHT-PIXELS = 27
        rctData:X                   = 0
        rctData:Y                   = 1
        rctData:WIDTH-PIXELS        = FRAME frData:WIDTH-PIXELS - 0
        rctData:HEIGHT-PIXELS       = FRAME frData:HEIGHT-PIXELS - 10
        rctDataFilter:WIDTH-PIXELS  = FRAME frData:WIDTH-PIXELS - rctDataFilter:X - 4
        NO-ERROR.
    END.

    /* Edit buttons */
    ASSIGN
      rctEdit:X = FRAME frData:X
      rctEdit:Y = FRAME frData:Y + FRAME frData:HEIGHT-PIXELS + 0
      NO-ERROR.

    /* Positioning of buttons "Add" "Save" etc */
    ASSIGN
      iButtonSpacingX = 5
      iButtonSpacingY = 0
      btnAdd:X        = rctEdit:X + iButtonSpacingX
      btnClone:X      = btnAdd:X  + btnAdd:WIDTH-PIXELS
      btnEdit:X       = btnClone:X + btnClone:WIDTH-PIXELS
      btnDump:X       = btnEdit:X + (2 * btnEdit:WIDTH-PIXELS)
      btnView:X       = btnDump:X + btnDump:WIDTH-PIXELS
      btnLoad:X       = btnView:X + btnView:WIDTH-PIXELS
      btnDelete:X     = btnLoad:X + (2 * btnLoad:WIDTH-PIXELS)

      btnAdd:Y        = rctEdit:Y + iButtonSpacingY
      btnClone:Y      = rctEdit:Y + iButtonSpacingY
      btnEdit:Y       = rctEdit:Y + iButtonSpacingY
      btnDump:Y       = rctEdit:Y + iButtonSpacingY
      btnView:Y       = rctEdit:Y + iButtonSpacingY
      btnLoad:Y       = rctEdit:Y + iButtonSpacingY
      btnDelete:Y     = rctEdit:Y + iButtonSpacingY
      NO-ERROR.

    /* Num results of query */
    RUN showNumRecords(?,?).
    RUN showNumSelected.

    /* Feedback txt */
    ASSIGN
      fiFeedback:WIDTH-PIXELS = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(fiFeedback:SCREEN-VALUE,getFont("default")) + 10
      fiFeedback:HEIGHT-PIXELS = FONT-TABLE:GET-TEXT-HEIGHT-PIXELS(getFont("default")) + 10
      fiFeedback:X = FRAME frData:X + FRAME frData:WIDTH-PIXELS - fiFeedback:WIDTH-PIXELS
      fiFeedback:Y = rctEdit:Y + rctEdit:HEIGHT-PIXELS - fiFeedback:HEIGHT-PIXELS - 6
      NO-ERROR.

    DO:
      /* Positioning of browse with fields */
      ASSIGN
        ghFieldBrowse:WIDTH-PIXELS  = rctQuery:WIDTH-PIXELS - 322
        ghFieldBrowse:HEIGHT-PIXELS = btnResizeVer:Y - ghFieldBrowse:Y - 3
        NO-ERROR.

      /* Index browse has same dimensions as field browse
       * Due to errors on resizing, first 'park' the browse in the upper
       * left with width 1, then set the proper size attributes.
       */
      ASSIGN
        brIndexes:X             = 1
        brIndexes:WIDTH-PIXELS  = 1
        brIndexes:X             = ghFieldBrowse:X
        brIndexes:Y             = ghFieldBrowse:Y
        brIndexes:WIDTH-PIXELS  = ghFieldBrowse:WIDTH-PIXELS
        brIndexes:HEIGHT-PIXELS = ghFieldBrowse:HEIGHT-PIXELS

        /* resize rectangles around the browse */
        rcFieldFilter:X             = ghFieldBrowse:X - 3
        rcFieldFilter:Y             = ghFieldBrowse:Y - 3
        rcFieldFilter:WIDTH-PIXELS  = ghFieldBrowse:WIDTH-PIXELS + 6
        rcFieldFilter:HEIGHT-PIXELS = ghFieldBrowse:HEIGHT-PIXELS + 6
        rcIndexFilter:X             = brIndexes:X - 3
        rcIndexFilter:Y             = brIndexes:Y - 3
        rcIndexFilter:WIDTH-PIXELS  = brIndexes:WIDTH-PIXELS + 6
        rcIndexFilter:HEIGHT-PIXELS = brIndexes:HEIGHT-PIXELS + 6

        /* right-align buttons with field browse */
        btnClipboard:X = (ghFieldBrowse:X + ghFieldBrowse:WIDTH-PIXELS) - btnClipboard:WIDTH-PIXELS
        btnQueries:X   = btnClipboard:X - btnQueries:WIDTH-PIXELS
        btnClear:X     = btnQueries:X - btnClear:WIDTH-PIXELS
        btnViewData:X  = btnClear:X - btnViewData:WIDTH-PIXELS
        btnWhere:X     = btnViewData:X - btnWhere:WIDTH-PIXELS

        btnClipboard:Y = btnResizeVer:Y + btnResizeVer:HEIGHT-PIXELS
        btnQueries:Y   = btnClipboard:Y
        btnClear:Y     = btnClipboard:Y
        btnViewData:Y  = btnClipboard:Y
        btnWhere:Y     = btnClipboard:Y

        /* Query buttons */
        btnPrevQuery:X = brTables:X + 1
        btnNextQuery:X = btnPrevQuery:X + btnPrevQuery:WIDTH-PIXELS
        btnPrevQuery:Y = btnClipboard:Y
        btnNextQuery:Y = btnClipboard:Y

        /* And align editor to the left of button btnViewData */
        ficWhere:X = btnNextQuery:X + btnNextQuery:WIDTH-PIXELS + 2
        ficWhere:WIDTH-PIXELS = btnWhere:X - ficWhere:X - 2
        ficWhere:Y = btnClipboard:Y + 1

        /* Buttons for field moving */
        btnMoveUp:X       = rctQuery:WIDTH-PIXELS - 25
        btnMoveDown:X     = rctQuery:WIDTH-PIXELS - 25
        btnReset:X        = rctQuery:WIDTH-PIXELS - 25
        btnMoveTop:X      = rctQuery:WIDTH-PIXELS - 25
        btnMoveBottom:X   = rctQuery:WIDTH-PIXELS - 25
        NO-ERROR.

    END.

    /* Positioning of browse with data */
    IF VALID-HANDLE(ghDataBrowse) THEN
    DO:
      ASSIGN
        ghDataBrowse:Y = 1 /* to safely adjust size */
        ghDataBrowse:WIDTH-PIXELS = rctData:WIDTH-PIXELS - 10
        ghDataBrowse:HEIGHT-PIXELS = rctData:HEIGHT-PIXELS - 10 - 23 /* Extra space for filters */
        ghDataBrowse:X = rctData:X + 3
        ghDataBrowse:Y = rctData:Y + 5 + 21 /* Extra space for filters */
        NO-ERROR.

      RUN dataScrollNotify(INPUT ghDataBrowse).
    END.

    RUN resizeFilters(INPUT {&PAGE-TABLES}).
    RUN resizeFilters(INPUT {&PAGE-FAVOURITES}).
    RUN resizeFilters(INPUT {&PAGE-FIELDS}).
    RUN resizeFilters(INPUT {&PAGE-INDEXES}).

    RUN fixTooltips(c-win:HANDLE).

    RUN saveWindow.
    RUN showScrollBars(FRAME frData:HANDLE, NO, NO).
    RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO).
    setWindowFreeze(NO).

  END.

  /* If something goes wrong with resizing we end up here */
  RUN unlockWindow(C-Win:HANDLE).

  {&timerStop}

  /* Hide rectangles */
  rctEdit:VISIBLE = FALSE.
  rctQuery:VISIBLE = FALSE.
  rctData:VISIBLE = FALSE.

  APPLY "entry" TO c-win. /* dkn */

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterDataBrowse C-Win 
PROCEDURE filterDataBrowse :
/*
 * Apply the filter to the data browse
 */
  PUBLISH "setUsage" ("filterData"). /* user behaviour */
  RUN reopenDataBrowse.

END PROCEDURE. /* filterDataBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldClearAll C-Win 
PROCEDURE filterFieldClearAll :
/* Wipe contents of all filter fields in the same group
 */
  DEFINE INPUT PARAMETER phFilterField AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER phClearButton AS HANDLE NO-UNDO.

  setWindowFreeze(YES).

  APPLY "choose" TO phClearButton.
  phFilterField:SCREEN-VALUE = "".
  APPLY "value-changed" TO phFilterField.
  APPLY "entry" TO phFilterField.

  setWindowFreeze(NO).

END PROCEDURE. /* filterFieldClearAll */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldCursorDown C-Win 
PROCEDURE filterFieldCursorDown :
/* Jump from filter field to browse on cursor down
 */
  DEFINE INPUT PARAMETER phFilterField  AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER phBrowseField  AS HANDLE      NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.

  /* Remember the field we escaped from */
  FIND bColumn WHERE bColumn.hFilter = phFilterField NO-ERROR.
  IF AVAILABLE bColumn THEN gcLastDataField = bColumn.cFullName.

  APPLY 'leave' TO phFilterField.
  APPLY 'entry' TO phBrowseField.

  RETURN NO-APPLY.

END PROCEDURE. /* filterFieldCursorDown */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldEntry C-Win 
PROCEDURE filterFieldEntry :
/* Set the color for the text in the filter to black
 */
  DEFINE INPUT  PARAMETER phFilterField AS HANDLE      NO-UNDO.

  /* If you enter the field and you have not put in a filter,
   * clear out the field so you can type something yourself
   */
  IF phFilterField:SCREEN-VALUE = phFilterField:PRIVATE-DATA THEN
    phFilterField:SCREEN-VALUE = ''.

  setFilterFieldColor(phFilterField).

  /* Remember that we were in this filterfield,
   * aka "Killroy was here"
   */
  ghLastFilterField = phFilterField.

END PROCEDURE. /* filterFieldEntry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldLeave C-Win 
PROCEDURE filterFieldLeave :
/* Set the color for the text in the filter to gray
 */
  DEFINE INPUT PARAMETER phFilterField AS HANDLE      NO-UNDO.

  /* If nothing in the filter, restore the shadow text */
  IF   phFilterField:SCREEN-VALUE = ''
    OR phFilterField:SCREEN-VALUE = ? THEN phFilterField:SCREEN-VALUE = phFilterField:PRIVATE-DATA.

  setFilterFieldColor(phFilterField).

END PROCEDURE. /* filterFieldLeave */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldsBrowse C-Win 
PROCEDURE filterFieldsBrowse :
/* Apply the filter to the fields browse
 */
  PUBLISH "setUsage" ("filterFields"). /* user behaviour */
  RUN reopenFieldBrowse(?,?). /* reopen, while maintaining original sort */

END PROCEDURE. /* filterFieldsBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldScrollNotify C-Win 
PROCEDURE filterFieldScrollNotify :
/* Catch CURSOR-LEFT and CURSOR-RIGHT actions on the browse
 */
  DEFINE INPUT PARAMETER phBrowse AS HANDLE   NO-UNDO.

  DEFINE VARIABLE cFilterFields AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iColumn       AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cButtons      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iField        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE hColumn       AS HANDLE      NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.

  PUBLISH "debugMessage" (1, "scroll-notify of fieldBrowse").

  /* Might get called when browse is not yet realized, so: */
  IF NOT VALID-HANDLE(phBrowse) THEN RETURN.

  /* Freeze all */
  setWindowFreeze(YES).

  getFilterLoop:
  FOR EACH bColumn BY bColumn.iColumnNr:
    IF VALID-HANDLE(bColumn.hFilter) THEN
      cFilterFields = TRIM(SUBSTITUTE('&1,&2', cFilterFields, bColumn.hFilter),',').
  END.

  DO WITH FRAME frData:
    cButtons = SUBSTITUTE('&1', btnClearDataFilter:HANDLE).
  END.

  /* Resize them */
  RUN resizeFilterFields
    ( INPUT ?
    , INPUT cFilterFields
    , INPUT cButtons
    , INPUT phBrowse
    ).

  RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO).
  setWindowFreeze(NO).

END PROCEDURE.  /* filterScrollNotify */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldShow C-Win 
PROCEDURE filterFieldShow :
/* Show or hide a filter field
 */
  DEFINE INPUT PARAMETER phColumn AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER phFilter AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER phMenu   AS HANDLE NO-UNDO.

  phColumn:VISIBLE = phMenu:CHECKED.
  setRegistry("DataDigger:Fields", SUBSTITUTE("&1:Visible", phColumn:NAME), STRING(phMenu:CHECKED) ).

  RUN resizeFilters(INPUT {&PAGE-TABLES}).
  RUN resizeFilters(INPUT {&PAGE-FAVOURITES}).

END PROCEDURE. /* filterFieldShow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldValueChanged C-Win 
PROCEDURE filterFieldValueChanged :
/* Save current filter value
 */
  DEFINE INPUT PARAMETER phFilterField   AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER plRefreshBrowse AS LOGICAL     NO-UNDO.

  setFilterFieldColor(phFilterField).

  IF plRefreshBrowse THEN
    RUN setTimer("timedFieldFilter", 300).

END PROCEDURE. /* filterFieldValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterIndexBrowse C-Win 
PROCEDURE filterIndexBrowse :
/* Apply the filter to the index browse
 */
  PUBLISH "setUsage" ("filterFields"). /* user behaviour */
  RUN reopenIndexBrowse(?,?). /* reopen, while maintaining original sort */

END PROCEDURE. /* filterIndexBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterTables C-Win 
PROCEDURE filterTables :
/*
 * Apply the filter to the table browse
 */
  DO WITH FRAME {&FRAME-NAME}:
    RUN reopenTableBrowse(?).
    APPLY 'value-changed' TO brTables.
    APPLY 'entry' TO brTables.
  END.

END PROCEDURE. /* filterTables */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE fixTooltips C-Win 
PROCEDURE fixTooltips :
/* Replace # in tooltips with a CHR(10)
 */
  DEFINE INPUT PARAMETER phParent AS HANDLE NO-UNDO.
  DEFINE VARIABLE hWidget AS HANDLE NO-UNDO.

  hWidget = phParent:FIRST-CHILD.

  DO WHILE VALID-HANDLE(hWidget):
    IF hWidget:TYPE = "FRAME" OR hWidget:TYPE = "FIELD-GROUP" THEN RUN fixTooltips(hWidget).
    IF CAN-SET(hWidget,"TOOLTIP") THEN hWidget:TOOLTIP = REPLACE(hWidget:TOOLTIP,"#","~n").
    hWidget = hWidget:NEXT-SIBLING.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getDataQuery C-Win 
PROCEDURE getDataQuery :
/* Return the query that belongs to the currently shown data
 */
  DEFINE OUTPUT PARAMETER pcQuery AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cAndWhere AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabase AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFilter   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNewWhere AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSort     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTable    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cUseIndex AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cWhere    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cWord     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iUseIndex AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iWord     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lUseIndex AS LOGICAL     NO-UNDO.

  cDatabase = gcCurrentDatabase.
  cTable    = gcCurrentTable.
  RUN getFilterQuery(OUTPUT cFilter).

  /* Get query from editor */
  cWhere = TRIM(ficWhere:SCREEN-VALUE IN FRAME {&FRAME-NAME}).
  cWhere = REPLACE(cWhere, {&QUERYSEP}, '~n').

  /* If a query starts with 'AND' or 'OR' or 'WHERE', strip it */
  IF LOOKUP(ENTRY(1,cWhere,' '),'AND,OR,WHERE') > 0 THEN
    ENTRY(1,cWhere,' ') = ''.

  /* Extract USE-INDEX */
  WhereLoop:
  DO iWord = 1 TO NUM-ENTRIES(cWhere," "):
    cWord = ENTRY(iWord,cWhere," ").

    /* Remember we have found the USE-INDEX keyword */
    IF cWord = "USE-INDEX" THEN
    DO:
      lUseIndex = TRUE.
      NEXT WhereLoop.
    END.

    /* Skip index name after USE-INDEX */
    IF lUseIndex AND CAN-FIND(ttIndex WHERE ttIndex.cIndexName = cWord) THEN
    DO:
      cUseIndex = cWord.
      NEXT WhereLoop.
    END.

    cNewWhere = cNewWhere + " " + cWord.
  END.
  cWhere = cNewWhere.

  /* Extract the sort-by part */
  IF cWhere BEGINS 'BY ' THEN cWhere = ' ' + cWhere.
  IF INDEX(cWhere, ' BY ') > 0 THEN
    ASSIGN cSort  = SUBSTRING(cWhere,INDEX(cWhere, ' BY '))
           cWhere = REPLACE(cWhere, cSort, '').

  /* Now, lets build it up. Start with the basics */
  pcQuery = SUBSTITUTE("for each &1.&2 no-lock", cDatabase, cTable).

  /* Add query filter */
  IF cFilter <> '' THEN
    pcQuery = SUBSTITUTE("&1 WHERE (&2)", pcQuery, cFilter).

  /* Add the where  */
  IF cFilter =  '' AND cWhere <> '' AND NOT cWhere BEGINS 'BY ' THEN cAndWhere = 'WHERE'.
  IF cFilter <> '' AND cWhere <> '' AND NOT cWhere BEGINS 'BY ' THEN cAndWhere = 'AND'.
  IF cWhere <> '' THEN
    pcQuery = SUBSTITUTE("&1 &2 (&3)", pcQuery, cAndWhere, cWhere).

  /* Add sort */
  IF cSort <> '' THEN
    pcQuery = SUBSTITUTE("&1 &2", pcQuery, cSort).

  /* Add USE-INDEX */
  IF cUseIndex <> '' THEN
    pcQuery = SUBSTITUTE("&1 USE-INDEX &2", pcQuery, cUseIndex).

  /* For speed of repositioning... */
  pcQuery = SUBSTITUTE("&1 INDEXED-REPOSITION", pcQuery).

  /* Add tilde chr(126) to curly opening brace chr(123) if not already there */
  IF INDEX(pcQuery,CHR(123)) > 0
    AND INDEX(pcQuery,CHR(126)) <> INDEX(pcQuery,CHR(123)) - 1 THEN
    pcQuery = REPLACE(pcQuery, '~{', '~~~{').

END PROCEDURE. /* getDataQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getFilterQuery C-Win 
PROCEDURE getFilterQuery :
/* Return a query built from fields in the filter fields
 */
  DEFINE OUTPUT PARAMETER pcFilterQuery AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cOperator  AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cValue     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cValueList AS CHARACTER NO-UNDO.

  DEFINE BUFFER bField  FOR ttField.
  DEFINE BUFFER bColumn FOR ttColumn.

  /* Collect all filters */
  FOR EACH bField WHERE bField.lShow = TRUE
    , EACH bColumn
     WHERE bColumn.cFieldName = bField.cFieldName
       AND VALID-HANDLE(bColumn.hColumn):

    /* Skip fields with shadowtext or empty value */
    IF   bColumn.hFilter:SCREEN-VALUE = bColumn.cFullName
      OR bColumn.hFilter:SCREEN-VALUE = ""
      OR bColumn.hFilter:SCREEN-VALUE = ? THEN NEXT.

    ASSIGN cValue = bColumn.hFilter:SCREEN-VALUE.

    /* Save last x values used for a filter */
    RUN saveFilterValue
      ( INPUT gcCurrentDatabase
      , INPUT gcCurrentTable
      , INPUT bColumn.cFullName
      , INPUT cValue
      ).

    /* Save the new list since the order of items might have changed */
    IF giMaxFilterHistory > 0 THEN
    DO:
      cValueList = getRegistry( SUBSTITUTE("DB:&1",gcCurrentDatabase)
                              , SUBSTITUTE("&1.&2:FilterHistory",gcCurrentTable,bColumn.cFullName)
                              ).
      bColumn.hFilter:LIST-ITEMS = cValueList.
      bColumn.hFilter:SCREEN-VALUE = cValue.
      RUN filterFieldLeave(bColumn.hFilter).
    END.

    cOperator = SUBSTRING(cValue, 1, 2).
    DO WHILE LOOKUP(cOperator, "=,<,>,<=,>=,<>,!,!=") = 0 AND LENGTH(cOperator) > 0:
      cOperator = SUBSTRING(cOperator, 1, LENGTH(cOperator) - 1).
    END.

    /* Don't trim spaces in value, use RIGHT-TRIM on value, not TRIM */
    ASSIGN
      cValue    = IF cOperator <> "" THEN RIGHT-TRIM(SUBSTRING(cValue, LENGTH(cOperator) + 1)) ELSE cValue
      cValue    = TRIM(cValue,"'~"") /* Remove surrounding quotes */
      cOperator = REPLACE(cOperator, "!=", "<>")
      cOperator = REPLACE(cOperator, "!", "<>")
      .

    IF bField.cDataType = "CHARACTER" THEN
    DO:
      /* If user wants to search with matches, then ignore
       * this if the asterisk is at the end. In that case
       * a BEGINS is better because it might use an index.
       */
      IF INDEX( RIGHT-TRIM(cValue,"*") ,"*") > 0 THEN
        ASSIGN cOperator = "MATCHES".
      ELSE
        ASSIGN cValue = RIGHT-TRIM(cValue,"*").

      IF cOperator = "" THEN cOperator = "BEGINS".
    END.
    ELSE
      IF cOperator = "" THEN cOperator = "=".

    /* Overrule for RECID and ROWID */
    IF bColumn.cFullName = "RECID" THEN
      pcFilterQuery = SUBSTITUTE("&1 &2 &3(&4) = (&5)"
                        , pcFilterQuery
                        , IF pcFilterQuery = "" THEN "" ELSE "AND"
                        , bColumn.cFullName
                        , gcCurrentTable
                        , QUOTER(cValue)
                        ).
    ELSE
    IF bColumn.cFullName = "ROWID" THEN
      pcFilterQuery = SUBSTITUTE("&1 &2 &3(&4) = TO-ROWID(&5)"
                        , pcFilterQuery
                        , IF pcFilterQuery = "" THEN "" ELSE "AND"
                        , bColumn.cFullName
                        , gcCurrentTable
                        , QUOTER(cValue)
                        ).
    ELSE
      pcFilterQuery = SUBSTITUTE("&1 &2 &3 &4 &5"
                        , pcFilterQuery
                        , IF pcFilterQuery = "" THEN "" ELSE "AND"
                        , bColumn.cFullName
                        , cOperator
                        , QUOTER(cValue)
                        ).
  END.

  PUBLISH "debugMessage" (1, SUBSTITUTE("Query From Filter: &1", pcFilterQuery)).

END PROCEDURE. /* getFilterQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getLdbsFromParamFile C-Win 
PROCEDURE getLdbsFromParamFile :
/* Analyze a param file and return a list of all logical
 * names from it that are currently connected
 */
  DEFINE INPUT  PARAMETER pcFileName AS CHARACTER NO-UNDO.
  DEFINE OUTPUT PARAMETER pcLdbList  AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cConfig   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLine     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iItem     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cThisItem AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNextItem AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLdbName  AS CHARACTER   NO-UNDO EXTENT 100.
  DEFINE VARIABLE cPdbName  AS CHARACTER   NO-UNDO EXTENT 100.
  DEFINE VARIABLE iNumDb    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iDb       AS INTEGER     NO-UNDO.

  /* Read in the file, make it one long string of config options */
  INPUT FROM VALUE(pcFileName).
  REPEAT:
    IMPORT UNFORMATTED cLine.
    cConfig = cConfig + ' ' + cLine.
  END.
  INPUT CLOSE.

  /* Remove double spaces */
  REPEAT WHILE INDEX(cConfig, '  ') > 0:
    cConfig = REPLACE(cConfig,'  ',' ').
  END.

  /* Examine string, find -ld or -db option and preserve a list
   * of logical database names that are currently already connected
   */
  DO iItem = 1 TO NUM-ENTRIES(cConfig,' ') - 1:

    cThisItem = TRIM(ENTRY(iItem,cConfig,' ')).
    cNextItem = TRIM(ENTRY(iItem + 1,cConfig,' ')).

    IF cThisItem = '-db' THEN
    DO:
      iNumDb = iNumDb + 1.

      /* Actual db name is the next entry in the string */
      cPdbName[iNumDb] = cNextItem.

      /* Get just the base name without path */
      cPdbName[iNumDb] = ENTRY(NUM-ENTRIES(cPdbName[iNumDb],'\'),cPdbName[iNumDb],'\').
      cPdbName[iNumDb] = REPLACE(cPdbName[iNumDb],'.db','').

      /* Logical name is same as physical name, as long as there is no
       * -ld parameter found in the string
       */
      cLdbName[iNumDb] = cPdbName[iNumDb].
    END.

    /* -ld parameter overrules default logical name */
    IF cThisItem = '-ld' THEN cLdbName[iNumDb] = cNextItem.
  END.

  /* Make a list of all ldbnames that are already in use */
  DO iDb = 1 TO iNumDb:
    IF CONNECTED(cLdbName[iDb]) THEN pcLdbList = TRIM(SUBSTITUTE('&1,&2',pcLdbList,cLdbName[iDb]),',').
  END.

END PROCEDURE. /* getLdbsFromParamFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getSortedQuery C-Win 
PROCEDURE getSortedQuery :
/* Process the query and insert the BY-phrases at the proper place
 */
  DEFINE INPUT-OUTPUT PARAMETER pcQuery AS CHARACTER NO-UNDO.
  DEFINE BUFFER bfQuerySort FOR ttQuerySort.
  DEFINE VARIABLE cField AS CHARACTER   NO-UNDO.

  /* Remove indexed-reposition keyword. Will be added back later */
  IF LOOKUP('INDEXED-REPOSITION',pcQuery,' ') > 0 THEN
    pcQuery = REPLACE(pcQuery,'INDEXED-REPOSITION','').

  /* Take the part until the first 'BY'. Note that if there is
   * no ' BY ', the substring function will take the whole string
   */
  pcQuery = SUBSTRING(pcQuery, 1, INDEX(pcQuery,' BY ') - 1).

  /* Add all query sort fields */
  SortItem:
  FOR EACH bfQuerySort
    WHERE bfQuerySort.iGroup = 0
    BREAK BY bfQuerySort.iSortNr:

    CASE bfQuerySort.cSortField:
      WHEN 'RECID' THEN cField = SUBSTITUTE('RECID(&1)', gcCurrentTable).
      WHEN 'ROWID' THEN cField = SUBSTITUTE('ROWID(&1)', gcCurrentTable).
      OTHERWISE cField = bfQuerySort.cSortField.
    END.

    pcQuery = SUBSTITUTE('&1 BY &2 &3'
                        , pcQuery
                        , cField
                        , TRIM(STRING(bfQuerySort.lAscending,'/DESCENDING'))
                        ).
  END.

  /* add back 'INDEXED-REPOSITION' */
  pcQuery = SUBSTITUTE('&1 INDEXED-REPOSITION', pcQuery).

END PROCEDURE. /* getSortedQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getSortFromQuery C-Win 
PROCEDURE getSortFromQuery :
/* Extract sorting from user query
 */
  DEFINE INPUT PARAMETER pcQuery AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cPart AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iPart AS INTEGER     NO-UNDO.
  DEFINE BUFFER bfQuerySort FOR ttQuerySort.

  /* Delete all query sorts */
  FOR EACH bfQuerySort WHERE bfQuerySort.iGroup = 1:
    DELETE bfQuerySort.
  END.

  /* Split query on the word ' BY ' */
  pcQuery = REPLACE(pcQuery,' BY ', '|').

  IndexLoop:
  DO iPart = 2 TO NUM-ENTRIES(pcQuery,'|'):
    cPart = TRIM(ENTRY(iPart,pcQuery,'|')).

    CREATE bfQuerySort.
    ASSIGN
      bfQuerySort.iGroup     = 1
      bfQuerySort.iSortNr    = iPart - 1
      bfQuerySort.cSortField = ENTRY(1,cPart,' ')
      bfQuerySort.lAscending = NOT (cPart MATCHES '* DESC*')
      .
    /* Extract extent nr from name */
    IF bfQuerySort.cSortField MATCHES '*[*]' THEN
      bfQuerySort.iExt = INTEGER( ENTRY(1,ENTRY(2,bfQuerySort.cSortField,'['),']') ).
  END.

END PROCEDURE. /* getSortFromQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE hideColumn C-Win 
PROCEDURE hideColumn :
/* Hide the current column
 */
  DEFINE VARIABLE cColumnClicked AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cColumnValue   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iExtentNr      AS INTEGER     NO-UNDO.

  IF NUM-ENTRIES(ghDataBrowse:PRIVATE-DATA,CHR(1)) <> 3 THEN RETURN.
  ASSIGN
    cColumnClicked = ENTRY(1, ghDataBrowse:PRIVATE-DATA,CHR(1))
    cColumnValue   = ENTRY(2, ghDataBrowse:PRIVATE-DATA,CHR(1))
    iExtentNr      = INTEGER(ENTRY(3, ghDataBrowse:PRIVATE-DATA,CHR(1)))
    .

  RUN showField(cColumnClicked,FALSE).

END PROCEDURE. /* hideColumn */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE hideSettingsFrame C-Win 
PROCEDURE hideSettingsFrame :
/* Auto-hide the settings frame after 2 seconds
 */

  IF FRAME frSettings:VISIBLE THEN
  DO:
    IF NOT isMouseOver(FRAME frSettings:HANDLE) THEN
    DO:
      FRAME frSettings:VISIBLE = FALSE.
      RUN setTimer("hideSettingsFrame",0).
    END.

    ELSE
      RUN setTimer("hideSettingsFrame", 2000).
  END.

END PROCEDURE. /* hideSettingsFrame */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE incQueriesOfTable C-Win 
PROCEDURE incQueriesOfTable :
/* Increment the number of queries served for a table.
 * NOTE: This must be done in one move by fetching the nr
 *       from the ini file, adding one and saving it back since
 *       the user could have more than one window open.
 */
  DEFINE INPUT PARAMETER pcDatabase     AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pcTable        AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER piNumIncrement AS INTEGER NO-UNDO.

  DEFINE VARIABLE iQueriesServed AS INTEGER   NO-UNDO.
  DEFINE BUFFER bTable FOR ttTable.

  /* Which table? */
  FIND bTable
    WHERE bTable.cDatabase  = pcDatabase
      AND bTable.cTableName = pcTable
          NO-ERROR.
  IF NOT AVAILABLE bTable THEN RETURN.

  /* Current number of queries served */
  iQueriesServed = INTEGER( getRegistry( SUBSTITUTE('DB:&1', pcDatabase)
                                       , SUBSTITUTE('&1:QueriesServed', pcTable)
                                       )
                          ).
  IF iQueriesServed = ? THEN iQueriesServed = 0.
  iQueriesServed = iQueriesServed + piNumIncrement.

  /* Save */
  ASSIGN
    bTable.iNumQueries = iQueriesServed
    bTable.tLastUsed   = NOW.

  /* Save in registry */
  setRegistry ( SUBSTITUTE('DB:&1', pcDatabase )
              , SUBSTITUTE('&1:QueriesServed', pcTable )
              , STRING(bTable.iNumQueries)
              ).
  setRegistry ( SUBSTITUTE('DB:&1', pcDatabase )
              , SUBSTITUTE('&1:LastUsed', pcTable )
              , STRING(bTable.tLastUsed, '99/99/9999 HH:MM:SS')
              ).

  BROWSE brTables:refresh().

END PROCEDURE. /* incQueriesOfTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE incQueriesServed C-Win 
PROCEDURE incQueriesServed :
/*
 * Increment the number of queries served. We need to do
 * this in one move by fetching the nr of queries served
 * from the ini file, adding one and saving it back since
 * the user could have more than one window open.
 */
  DEFINE INPUT PARAMETER piNumIncrement AS INTEGER NO-UNDO.
  DEFINE VARIABLE iQueriesServed AS INTEGER NO-UNDO.

  {&timerStart}

  /* Number of queries served */
  iQueriesServed = INTEGER(getRegistry("DataDigger", "QueriesServed" )).
  IF iQueriesServed = ? THEN iQueriesServed = 0.
  iQueriesServed = iQueriesServed + piNumIncrement.
  setRegistry("DataDigger", "QueriesServed", STRING(iQueriesServed) ).

  {&timerStop}

END PROCEDURE. /* incQueriesServed */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeFilters C-Win 
PROCEDURE initializeFilters :
/* Create filter widgets
 */
  DEFINE INPUT PARAMETER phParentBrowse AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER phClearButton  AS HANDLE NO-UNDO.

  DEFINE VARIABLE iField        AS INTEGER NO-UNDO.
  DEFINE VARIABLE hColumn       AS HANDLE  NO-UNDO.
  DEFINE VARIABLE hFilterField  AS HANDLE  NO-UNDO.
  DEFINE VARIABLE hFilterButton AS HANDLE  NO-UNDO.
  DEFINE VARIABLE hClearButton  AS HANDLE  NO-UNDO.
  DEFINE VARIABLE hMenuItem     AS HANDLE  NO-UNDO.
  DEFINE VARIABLE lVisible      AS LOGICAL NO-UNDO.

  DEFINE BUFFER bFilter FOR ttFilter.

  /* Start with the "Is-Selected" toggle and then add all other columns */
  gcFieldFilterHandles = STRING(tgSelAll:HANDLE IN FRAME frMain).

  /* Create a menu */
  ghFieldMenu = createMenu(tgSelAll:HANDLE).

  /* Clean up old filters */
  RUN deleteDataFilters(phParentBrowse).

  /* Create a filter fill-in for each column in the browse
   * Except for the first toggle box.
   */
  DO iField = 2 TO phParentBrowse:NUM-COLUMNS:
    hColumn = phParentBrowse:GET-BROWSE-COLUMN(iField):HANDLE.

    /* Force column to be visible, or else the X attribute is ?
     * we will correct this after the loop, if needed.
     */
    hColumn:VISIBLE = TRUE.

    CREATE FILL-IN hFilterField
      ASSIGN
        FRAME         = hColumn:PARENT:FRAME
        NAME          = "filter_" + hColumn:NAME
        X             = hColumn:PARENT:X + hColumn:X
        Y             = hColumn:PARENT:Y - 21 - 1
        WIDTH-PIXELS  = 10
        HEIGHT-PIXELS = 21
        SENSITIVE     = TRUE
        VISIBLE       = FALSE
        FORMAT        = "x(40)"
        PRIVATE-DATA  = hColumn:LABEL
        SCREEN-VALUE  = hColumn:LABEL
    TRIGGERS:
      ON "entry"         PERSISTENT RUN filterFieldEntry        IN THIS-PROCEDURE (hFilterField).
      ON "leave"         PERSISTENT RUN filterFieldLeave        IN THIS-PROCEDURE (hFilterField).
      ON "value-changed" PERSISTENT RUN filterFieldValueChanged IN THIS-PROCEDURE (hFilterField,YES).
      ON "shift-del"     PERSISTENT RUN filterFieldClearAll     IN THIS-PROCEDURE (hFilterField, phClearButton:HANDLE).
      ON "return"        PERSISTENT RUN reopenFieldBrowse       IN THIS-PROCEDURE (?,?).
      ON "F2"            PERSISTENT RUN reopenFieldBrowse       IN THIS-PROCEDURE (?,?).
      ON "cursor-down"   PERSISTENT RUN filterFieldCursorDown   IN THIS-PROCEDURE (hFilterField, hColumn).
    END TRIGGERS.

    gcFieldFilterHandles = TRIM(SUBSTITUTE("&1,&2", gcFieldFilterHandles, hFilterField),",").

    /* Keep track of filters */
    CREATE bFilter.
    ASSIGN
      bFilter.cFieldName = hColumn:NAME
      bFilter.hFilter    = hFilterField
      bFilter.hColumn    = hColumn
      bFilter.hBrowse    = phParentBrowse
      .

    /* Create menu item for context menu */
    hMenuItem = createMenuItem(ghFieldMenu,"TOGGLE-BOX",bFilter.hColumn:LABEL,"").
    ON "VALUE-CHANGED" OF hMenuItem PERSISTENT
      RUN filterFieldShow IN THIS-PROCEDURE(bFilter.hColumn, hFilterField, hMenuItem).

    /* Column visible? */
    lVisible = LOGICAL(getRegistry("DataDigger:Fields", SUBSTITUTE("&1:Visible", hColumn:NAME))) NO-ERROR.
    IF lVisible = ? THEN lVisible = TRUE.
    hMenuItem:CHECKED = lVisible.
    hColumn:VISIBLE = lVisible.
  END.

END PROCEDURE. /* initializeFilters */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObjects C-Win 
PROCEDURE initializeObjects :
/* General setup of the window
 */
  DEFINE VARIABLE cDatabases    AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cSetting      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iSetting      AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iColor        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iValue        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iField        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hColumn       AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iRgbValue     AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iStackSize    AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hFilterField  AS HANDLE    NO-UNDO.
  DEFINE VARIABLE IFilter       AS INTEGER   NO-UNDO.

  {&timerStart}

  /* Open the settings file */
  RUN initializeSettingsFile.

  /* Set FONTS and load Images */
  RUN initializeVisuals.

  /* Table filter */
  RUN initTableFilter(INPUT-OUTPUT TABLE ttTableFilter).

  /* Maximum time for a query */
  giMaxQueryTime = INTEGER(getRegistry("DataDigger","MaxQueryTime")) NO-ERROR.
  IF giMaxQueryTime = ? THEN giMaxQueryTime = 500.

  /* Maximum number OF columns */
  giMaxColumns = INTEGER(getRegistry("DataDigger", "MaxColumns" )).
  IF giMaxColumns = ? THEN giMaxColumns = 500.

  /* Maximum nr of extent fields */
  giMaxExtent = INTEGER(getRegistry("DataDigger", "MaxExtent" )).
  IF giMaxExtent = ? THEN giMaxExtent = 100.

  /* Maximum number OF history ON data filters */
  giMaxFilterHistory = INTEGER(getRegistry("DataDigger", "MaxFilterHistory")).
  IF giMaxFilterHistory = ? THEN giMaxFilterHistory = 10.

  /* IF the stack space is 128 or less, limit nr OF columns
   * TO prevent the sessiON from crashing. As a rough guide
   * just use 3 x stacksize as maximum nr OF columns.
   */
  iStackSize = getStackSize().
  IF iStackSize <= 128 THEN
    giMaxColumns = MINIMUM(3 * iStackSize, giMaxColumns).

  /* Main FRAME */
  DO WITH FRAME {&FRAME-NAME}:

    /* > Hint frame */
    FRAME frHint:X = 1.
    FRAME frHint:Y = 1.
    FRAME frHint:HIDDEN = TRUE.

    /* Show or hide Toggle box for Debug mode */
    tgDebugMode:HIDDEN  = &IF DEFINED (UIB_is_RUNning) &THEN NO. &ELSE YES. &ENDIF
    .setDebugMode(YES). /* debug-patrick */

    /* Colors for odd/even data rows */
    IF getRegistry("DataDigger:colors","DataRow:UseSystem") = "YES" THEN
      ASSIGN
        giDataOddRowColor[1]  = 1
        giDataOddRowColor[2]  = 24 /* ButtonFace, often defined as off-white */
        giDataEvenRowColor[1] = 1
        giDataEvenRowColor[2] = 15 /* white */
        .
    ELSE 
      ASSIGN 
        giDataOddRowColor[1]  = getColor("DataRow:odd:fg" )
        giDataOddRowColor[2]  = getColor("DataRow:odd:bg" )
        giDataEvenRowColor[1] = getColor("DataRow:even:fg")
        giDataEvenRowColor[2] = getColor("DataRow:even:bg")
        .

    DO WITH FRAME frData:
      ASSIGN
        btnClearDataFilter:VISIBLE = (NUM-DBS > 0)
        btnDataSort       :VISIBLE = (NUM-DBS > 0).
    END.

    /* Load images for buttONs */
    DO WITH FRAME frSettings:

      /* Disable these WHEN plReadOnlyDigger */
      IF plReadOnlyDigger THEN
        ASSIGN
          btnDict         :SENSITIVE = FALSE
          btnDict-txt     :SENSITIVE = FALSE
          btnDataAdmin    :SENSITIVE = FALSE
          btnDataAdmin-txt:SENSITIVE = FALSE
          btnProcEdit     :SENSITIVE = FALSE
          btnProcEdit-txt :SENSITIVE = FALSE
        .
    END.

    /* Handle to the browse with fields of a file */
    ghFieldBrowse = brFields:HANDLE IN FRAME {&FRAME-NAME}.

    /* restore width of table browser columns */
    DO iField = 1 TO brTables:NUM-COLUMNS:
      hColumn = brTables:GET-BROWSE-COLUMN(iField):HANDLE.

      /* Get the width from registry */
      iValue = INTEGER(getRegistry("DataDigger", SUBSTITUTE("ColumnWidth:&1", hColumn:NAME))) NO-ERROR.
      IF iValue = ? THEN
      DO:
        CASE hColumn:NAME:
          WHEN "cTableName"  THEN iValue = 110.
          WHEN "cDatabase"   THEN iValue =  50.
          WHEN "iNumQueries" THEN iValue =  28.
          WHEN "tLastUsed"   THEN iValue = 103.
        END CASE.
      END.
      IF iValue <> ? THEN hColumn:WIDTH-PIXELS = iValue.

      ON "end-resize" OF hColumn PERSISTENT RUN resizeFilters IN THIS-PROCEDURE (INPUT {&PAGE-TABLES}).
    END.

    /* restore width of index browser columns */
    DO iField = 1 TO brIndexes:NUM-COLUMNS:
      hColumn = brIndexes:GET-BROWSE-COLUMN(iField):HANDLE.

      /* Get the width from registry */
      iValue = INTEGER(getRegistry("DataDigger", SUBSTITUTE("ColumnWidth:&1", hColumn:NAME))) NO-ERROR.
      IF iValue = ? THEN
      DO:
        CASE hColumn:NAME:
          WHEN "cIndexName"   THEN iValue = 100.
          WHEN "cIndexFlags"  THEN iValue =  70.
          WHEN "cIndexFields" THEN iValue = 314.
        END CASE.
      END.
      IF iValue <> ? THEN hColumn:WIDTH-PIXELS = iValue.

      ON "end-resize" OF hColumn PERSISTENT RUN resizeFilters IN THIS-PROCEDURE (INPUT {&PAGE-INDEXES}).
    END.

    /* Build a list OF all columns in the fieldbrowse.
     * We use this TO hi-light fields that are in the prim index.
     */
    gcFieldBrowseColumnHandles = "".
    gcFieldBrowseColumnNames   = "".

    DO iField = 1 TO brFields:NUM-COLUMNS:
      hColumn = brFields:GET-BROWSE-COLUMN(iField):HANDLE.
      gcFieldBrowseColumnHANDLEs = gcFieldBrowseColumnHANDLEs + "," + STRING(hColumn).
      gcFieldBrowseColumnNames = gcFieldBrowseColumnNames + "," + hColumn:NAME.

      /* Hide the cFormatOrg column */
      IF hColumn:NAME = "cFormatOrg" THEN hColumn:VISIBLE = FALSE.

      /* Get the width from registry */
      iValue = INTEGER(getRegistry("DataDigger", SUBSTITUTE("ColumnWidth:&1", hColumn:NAME))) NO-ERROR.
      IF iValue = ? THEN
      DO:
        CASE hColumn:NAME:
          WHEN "lShow"      THEN iValue =  15.
          WHEN "iOrder"     THEN iValue =  35.
          WHEN "cFieldName" THEN iValue = 150.
          WHEN "cDataType"  THEN iValue =  80.
          WHEN "cFormat"    THEN iValue =  75.
          WHEN "cLabel"     THEN iValue = 117.
        END CASE.
      END.
      IF iValue <> ? THEN hColumn:WIDTH-PIXELS = iValue.

      ON "end-resize" OF hColumn PERSISTENT RUN resizeFilters IN THIS-PROCEDURE (INPUT {&PAGE-FIELDS}).
    END.
    gcFieldBrowseColumnHANDLEs = TRIM(gcFieldBrowseColumnHANDLEs,",").
    gcFieldBrowseColumnNames = TRIM(gcFieldBrowseColumnNames,",").

    /* Build a list OF all columns in the index browse.
     * We use this TO hi-light indexes that are inactive.
     */
    gcIndexBrowseColumnHANDLEs = "".

    DO iField = 1 TO brIndexes:NUM-COLUMNS:
      hColumn = brIndexes:GET-BROWSE-COLUMN(iField):HANDLE.
      gcIndexBrowseColumnHANDLEs = gcIndexBrowseColumnHANDLEs + "," + STRING(hColumn).
    END.
    gcIndexBrowseColumnHANDLEs = TRIM(gcIndexBrowseColumnHANDLEs,",").

    /* Init on Fields-page and table-page */
    RUN setPage({&PAGE-FIELDS}).
    RUN setPage({&PAGE-TABLES}).

    /* Move index browse and associated filter fields TO the left.
     * Just throw "em ON a stack, the resize event will take care OF it.
     */
    fiIndexNameFilter  :X = tgSelAll:X.
    fiFlagsFilter      :X = tgSelAll:X.
    fiFieldsFilter     :X = tgSelAll:X.
    btnClearIndexFilter:X = tgSelAll:X.

    /* Initialize the buttON panels TO OFF */
    setUpdatePanel("no-record").

    /* Set the view type */
    cSetting = getRegistry("DataDigger", "ViewType").
    IF cSetting <> ? THEN RUN setViewType(cSetting).

    /* Create filter fill-ins for the fields browse */
    RUN initializeFilters
      ( INPUT brFields:HANDLE
      , INPUT btnClearFieldFilter:HANDLE
      ).

    /* Set filters for table browse */
    RUN resizeFilters (INPUT 0).

    RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
    /* < UI Stuff */

    /*
     * > Restore
     */

    /* Window position and size */
    iValue = INTEGER(getRegistry("DataDigger", "Window:X" )).
    IF iValue = ? THEN iValue = 200.

    /* Keep DD ON primary monitor ? (Rob Willoughby) */
    IF LOGICAL(getRegistry("DataDigger","StartOnPrimaryMonitor")) = YES
      AND (iValue < 0 OR iValue > SESSION:WORK-AREA-WIDTH-PIXELS) THEN iValue = 200.

    ASSIGN c-win:X = iValue NO-ERROR.

    /* Window has been parked at y=-1000 TO get it out OF sight */
    iValue = INTEGER(getRegistry("DataDigger", "Window:Y" )).
    PUBLISH "debugMessage" (1, SUBSTITUTE("window:y from reg = &1", iValue)).
    IF iValue < 0 OR iValue = ? OR iValue > SESSION:WORK-AREA-HEIGHT-PIXELS THEN iValue = 200.
    ASSIGN c-win:Y = iValue NO-ERROR.
    PUBLISH "debugMessage" (1, SUBSTITUTE("Reset window to y = &1", iValue)).

    iValue = INTEGER(getRegistry("DataDigger", "Window:height" )).
    IF iValue = ? OR iValue = 0 THEN iValue = 600.
    ASSIGN c-win:HEIGHT-PIXELS = iValue NO-ERROR.

    iValue = INTEGER(getRegistry("DataDigger", "Window:width" )).
    IF iValue = ? OR iValue = 0 THEN iValue = 800.
    ASSIGN c-win:WIDTH-PIXELS = iValue NO-ERROR.

    /* Resize bar */
    iValue = INTEGER(getRegistry("DataDigger", "ResizeBar:Y" )).
    IF iValue = ? OR iValue < 150 THEN iValue = 150.
    IF iValue > (C-Win:HEIGHT-PIXELS - 200) THEN iValue = C-Win:HEIGHT-PIXELS - 200.
    ASSIGN btnResizeVer:Y = iValue NO-ERROR.

    /* Get all connected databases */
    cDatabases = getDatabaseList().
    cbDatabaseFilter:LIST-ITEMS = "," + cDatabases.

    /* Get sort for fields */
    cSetting = getRegistry("DataDigger","ColumnSortFields").
    IF cSetting <> ? THEN
      brFields:SET-SORT-ARROW(INTEGER(ENTRY(1,cSetting)), LOGICAL(ENTRY(2,cSetting)) ).

    /* Get sort for indexes */
    cSetting = getRegistry("DataDigger","ColumnSortIndexes").
    IF cSetting <> ? THEN
      brIndexes:SET-SORT-ARROW(INTEGER(ENTRY(1,cSetting)), LOGICAL(ENTRY(2,cSetting)) ).

    /* Get last used database from registry */
    cSetting = getRegistry("DataDigger","Database").
    IF cSetting = "<empty>" OR cSetting = ? THEN cSetting = "".

    /* Restore last used database, IF possible */
    IF LOOKUP(cSetting,cbDatabaseFilter:LIST-ITEMS) > 0 THEN
      cbDatabaseFilter:SCREEN-VALUE = cSetting.
    ELSE
      cbDatabaseFilter:SCREEN-VALUE = cbDatabaseFilter:ENTRY(1).

    /* Set Table or Favourites view */
    cSetting = getRegistry("DataDigger","TableView").
    glShowFavourites = (cSetting BEGINS "F" AND CAN-FIND(FIRST ttTable WHERE ttTable.lFavourite = TRUE)).
    RUN setTableView(glShowFavourites,YES).

    /* Hide or view the query editor */
    cSetting = getRegistry("DataDigger", "QueryEditorState").
    IF cSetting = ? THEN cSetting = "Hidden".
    setQueryEditor(cSetting).

    /* Take whatever is now selected in the db dropDOwn */
    cSetting = gcCurrentDatabase.
    /* < Restore  */

    /* timedScrollNotify */
    /* RUN setTimer("timedScrollNotIFy", 100). */

    /* KeepAlive timer */
    IF LOGICAL(getRegistry("DataDigger", "KeepAlive")) THEN
      RUN setTimer("KeepAlive", 5000). /* every 5 seconds */
    ELSE
      RUN setTimer("KeepAlive", 0).

    /* preCache timer */
    IF LOGICAL(getRegistry("DataDigger:Cache","preCache")) THEN
    DO:
      iSetting = INTEGER(getRegistry("DataDigger:Cache","preCacheInterval")) * 1000.
      IF iSetting > 0 THEN RUN setTimer("PreCache", iSetting).
    END.

    /* Set caching in library */
    RUN setCaching.
  END. /* DO WITH FRAME */

  RUN endResize.
  APPLY "value-changed" TO brTables.

  {&timerStop}
END PROCEDURE. /* initializeObjects */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeSettingsFile C-Win 
PROCEDURE initializeSettingsFile :
/* Initialize the settings file
 */
  {&timerStart}
  DEFINE VARIABLE cProgDir     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cEnvironment AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iColumn      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE hColumn      AS HANDLE      NO-UNDO.

  /* Find out where DataDigger is installed and how we"re logged on */
  cProgDir = getProgramDir().

  /* If the general ini file does not exist, create it */
  IF SEARCH(cProgDir + "DataDigger.ini") = ? THEN
  DO:
    OUTPUT TO VALUE(cProgDir + "DataDigger.ini").
    OUTPUT CLOSE.
  END.
  LOAD "DataDigger" DIR cProgDir BASE-KEY "ini" NO-ERROR.
  IF ERROR-STATUS:ERROR THEN
    LOAD "DataDigger" DIR cProgDir NEW BASE-KEY "ini" NO-ERROR.


  /* Same for the helpfile (though it SHOULD exist!) */
  IF SEARCH(cProgDir + "DataDiggerHelp.ini") = ? THEN
  DO:
    OUTPUT TO VALUE(cProgDir + "DataDiggerHelp.ini").
    OUTPUT CLOSE.
  END.
  LOAD "DataDiggerHelp" DIR cProgDir BASE-KEY "ini" NO-ERROR.
  IF ERROR-STATUS:ERROR THEN
    LOAD "DataDiggerHelp" DIR cProgDir NEW BASE-KEY "ini" NO-ERROR.


  /* Load or create personalized ini file */
  cEnvironment = SUBSTITUTE("DataDigger-&1", getUserName() ).

  /* If not exist, create it */
  IF SEARCH(cProgDir + cEnvironment + ".ini") = ? THEN
  DO:
    OUTPUT TO VALUE(cProgDir + cEnvironment + ".ini").
    OUTPUT CLOSE.
  END.
  LOAD cEnvironment DIR cProgDir BASE-KEY "ini" NO-ERROR.
  IF ERROR-STATUS:ERROR THEN
    LOAD cEnvironment DIR cProgDir NEW BASE-KEY "ini" NO-ERROR.

  /*
   * Set some settings to default values
   */

  /* Automatically compile */
  IF getRegistry("DataDigger", "AutoCompile") = ? THEN setRegistry("DataDigger", "AutoCompile","yes").

  /* Visibility of columns in brFields */
  DO iColumn = 1 TO brFields:NUM-COLUMNS IN FRAME frMain:
    hColumn = brFields:GET-BROWSE-COLUMN(iColumn).
    IF getRegistry("DataDigger:Fields", SUBSTITUTE("&1:Visible", hColumn:NAME)) = ? THEN
      setRegistry("DataDigger:Fields", SUBSTITUTE("&1:Visible", hColumn:NAME), "yes").
  END.

  /* Position of resize bar */
  IF getRegistry("DataDigger", "ResizeBar:Y" ) = ? THEN setRegistry("DataDigger", "ResizeBar:Y", "260" ).

  /* Add column for recid / rowid */
  IF getRegistry("DataDigger","AddDataColumnForRecid") = ? THEN setRegistry("DataDigger","AddDataColumnForRecid","yes").
  IF getRegistry("DataDigger","AddDataColumnForRowid") = ? THEN setRegistry("DataDigger","AddDataColumnForRowid","no").

  /* Expand the query editor when we do a right click on index */
  IF getRegistry("DataDigger","AutoExpandQueryEditor") = ? THEN setRegistry("DataDigger","AutoExpandQueryEditor","yes").

  /* Max time for a query in msec */
  IF getRegistry("DataDigger","MaxQueryTime") = ? THEN setRegistry("DataDigger","MaxQueryTime","500").

  /* Max nr of columns in the browse */
  IF getRegistry("DataDigger","MaxColumns") = ? THEN setRegistry("DataDigger","MaxColumns", "500" ).

  /* Max nr of extents in the browse */
  IF getRegistry("DataDigger","MaxExtent") = ? THEN setRegistry("DataDigger","MaxExtent", "100" ).

  /* Max nr of queries to remember */
  IF getRegistry("DataDigger","MaxQueryHistory") = ? THEN setRegistry("DataDigger","MaxQueryHistory", "10" ).

  /* Max nr of filters on data */
  IF getRegistry("DataDigger", "MaxFilterHistory") = ? THEN setRegistry("DataDigger", "MaxFilterHistory","10").

  /* Database filter */
  IF getRegistry("DataDigger","Database") = ? THEN setRegistry("DataDigger","Database","<empty>").

  /* What to do on double click? */
  IF getRegistry("DataDigger","DataDoubleClick") = ? THEN setRegistry("DataDigger","DataDoubleClick", "EDIT").

  /* What is the default view type? */
  IF getRegistry("DataDigger", "ViewType") = ? THEN RUN setViewType("txt").

  /* Column label template */
  IF getRegistry("DataDigger", "ColumnLabelTemplate") = ? THEN setRegistry("DataDigger", "ColumnLabelTemplate","&1").

  /* Show hidden tables */
  IF getRegistry("DataDigger", "ShowHiddenTables") = ? THEN setRegistry("DataDigger", "ShowHiddenTables","FALSE").

  /* Enable WRITE and DELETE triggers by default */
  IF getRegistry("DataDigger","EnableWriteTriggers")  = ? THEN setRegistry("DataDigger","EnableWriteTriggers", "true").
  IF getRegistry("DataDigger","EnableDeleteTriggers") = ? THEN setRegistry("DataDigger","EnableDeleteTriggers", "true").

  /* Keep-alive function on databases to avoid connection drop */
  IF getRegistry("DataDigger","KeepAlive") = ? THEN setRegistry("DataDigger","KeepAlive", "true").

  /* Create a dir for the cache */
  OS-CREATE-DIR VALUE(SUBSTITUTE("&1Cache",getProgramDir())).

  /* Cache */
  IF getRegistry("DataDigger:Cache","Settings")         = ? THEN setRegistry("DataDigger:Cache","Settings","true").
  IF getRegistry("DataDigger:Cache","TableDefs")        = ? THEN setRegistry("DataDigger:Cache","TableDefs","true").
  IF getRegistry("DataDigger:Cache","FieldDefs")        = ? THEN setRegistry("DataDigger:Cache","FieldDefs","true").
  IF getRegistry("DataDigger:Cache","preCache")         = ? THEN setRegistry("DataDigger:Cache","preCache", "true").
  IF getRegistry("DataDigger:Cache","preCacheInterval") = ? THEN setRegistry("DataDigger:Cache","preCacheInterval", "2"). /* sec  */

  /* Check whether font settings are OK */
  RUN checkFonts.

  /* If still no fonts defined, set default font to 4 and fixed to 0 */
  IF getRegistry("DataDigger:fonts","default") = ? THEN setRegistry("DataDigger:fonts","default", STRING(getFont("Default"))).
  IF getRegistry("DataDigger:fonts","fixed")   = ? THEN setRegistry("DataDigger:fonts","fixed"  , STRING(getFont("Fixed"))).

  /* Autoset fonts */
  IF getRegistry("DataDigger:fonts","AutoSetFont") = ? THEN setRegistry("DataDigger:fonts","AutoSetFont", "YES").

  /* If no colors defined for data rows or useSystemColors not defined, set "useSystemColors" to TRUE */
  IF    getRegistry("DataDigger:colors","DataRow:UseSystem") = ?
    OR (getRegistry("DataDigger:colors","DataRow:odd:fg") = ?
    AND getRegistry("DataDigger:colors","DataRow:odd:bg") = ?
    AND getRegistry("DataDigger:colors","DataRow:even:fg") = ?
    AND getRegistry("DataDigger:colors","DataRow:even:bg") = ?) THEN setRegistry("DataDigger:colors","DataRow:UseSystem","YES").

  /* How to deal with filtering */
  IF getRegistry("DataDigger","FilterWithMatches") = ? THEN setRegistry("DataDigger","FilterWithMatches", "YES").

  /* Dump & Load settings */
  IF    getRegistry("DumpAndLoad", "DumpFileTemplate") = ?
    AND getRegistry("DumpAndLoad", "DumpDir") = ? THEN
  DO:
    setRegistry("DumpAndLoad", "DumpFileDir"     , "<LASTDIR>").
    setRegistry("DumpAndLoad", "DumpFileTemplate", "<TABLE>.<EXT>").
  END.

  /* Backup:
   * If user has not set a name for the backup file, we will turn it on.
   * If backup folder is empty, we set it
   */
  IF getRegistry("DataDigger:Backup","BackupFileTemplate") = ? THEN
  DO:
    setRegistry("DataDigger:Backup","BackupFileTemplate", "<DB>.<TABLE>.<TIMESTAMP>.<#>.XML").
    setRegistry("DataDigger:Backup","BackupDir"         , "<PROGDIR>\Backup\").
  END.

  /* Turn backups on by default */
  setRegistry("DataDigger:Backup","BackupOnUpdate", "YES").
  setRegistry("DataDigger:Backup","BackupOnDelete", "YES").

  IF   getRegistry("DumpAndLoad", "DumpDir") = ?
    OR getRegistry("DumpAndLoad", "DumpDir") = '' THEN setRegistry("DumpAndLoad", "DumpDir", "<PROGDIR>\Dump\").

  IF   getRegistry("DataDigger:Backup", "BackupDir") = ?
    OR getRegistry("DataDigger:Backup", "BackupDir") = '' THEN setRegistry("DataDigger:Backup", "BackupDir", "<PROGDIR>\Backup\").

  /* Update check, set to check on STABLE */
  IF getRegistry("DataDigger:Update","UpdateChannel") = ? THEN setRegistry("DataDigger:Update","UpdateChannel", "{&CHECK-STABLE}").
  IF getRegistry('DataDigger:Update','PingBack') = ? THEN setRegistry('DataDigger:Update','PingBack','YES').

  /* DB Name to use in title bar */
  IF getRegistry('DataDigger','TitleBarDbName') = ? THEN setRegistry('DataDigger','TitleBarDbName','ldbname').

  {&timerStop}
END PROCEDURE. /* initializeSettingsFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeUi C-Win 
PROCEDURE initializeUi :
/* Enable the user interface
 */
  DEFINE VARIABLE UIB_S    AS LOGICAL    NO-UNDO.
  DEFINE VARIABLE OCXFile  AS CHARACTER  NO-UNDO.

  /* Load wrx file if possible */
  OCXFile = SEARCH( "wDataDigger.wrx":U ).
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

  /* FRAME frMain */
  DISPLAY
    fiIndexNameFilter fiFlagsFilter fiFieldsFilter tgSelAll fiTableFilter
    cbDatabaseFilter fiTableDesc ficWhere fiFeedback
    WITH FRAME frMain IN WINDOW C-Win.

  ENABLE
    rctQuery btnDelete rctEdit
    btnClearFieldFilter fiIndexNameFilter fiFlagsFilter
    fiFieldsFilter btnClearIndexFilter tgSelAll tgDebugMode
    brFields brIndexes
    btnMoveTop btnMoveUp btnReset btnMoveDown btnMoveBottom /* field move */
    fiTableFilter cbDatabaseFilter btnClearTableFilter btnTableFilter /* top of table browse */
    btnTabTables btnTabFavourites brTables fiTableDesc btnFavourite /* table browse + tabs */
    btnPrevQuery btnNextQuery ficWhere btnWhere btnClear btnQueries btnClipboard /* query */
    btnTools btnTabFields btnTabIndexes btnResizeVer btnClone
    btnDump btnView btnAdd btnEdit fiFeedback

    WITH FRAME frMain IN WINDOW C-Win.

  /* FRAME frHint */
  DISPLAY
    edHint
    WITH FRAME frHint IN WINDOW C-Win.

  ENABLE
    edHint btGotIt
    WITH FRAME frHint IN WINDOW C-Win.

  /* FRAME frWhere */
  FRAME frWhere:FONT = giDefaultFont.
  DISPLAY
    cbAndOr cbFields cbOperator ficValue ficWhere2
    WITH FRAME frWhere IN WINDOW C-Win.

  ENABLE
    btnAnd rctQueryButtons cbAndOr cbFields cbOperator ficValue btnInsert
    ficWhere2 btnClear-2 btnQueries-2
    btnClipboard-2 btnOK btnCancel-2 btnBegins btnBracket btnContains
    btnEq btnGT btnLT btnMatches btnNE btnOr btnQt btnToday
    WITH FRAME frWhere IN WINDOW C-Win.

  /* FRAME frWhere */
  VIEW FRAME frWhere IN WINDOW C-Win.
  DISPLAY fiNumRecords WITH FRAME frData IN WINDOW C-Win.

  ENABLE
    rctData btnClearDataFilter btnDataSort fiNumRecords
    WITH FRAME frData IN WINDOW C-Win.

  /* FRAME frSettings */
  ENABLE
    btnDataDigger  btnDataDigger-txt
    btnSettings    btnSettings-txt
    btnDict        btnDict-txt
    btnQueries-3   btnQueries-txt
    btnDataAdmin btnDataAdmin-txt
    btnConnections btnConnections-txt
    btnProcEdit    btnProcEdit-txt
    btnQueryTester btnQueryTester-txt
    btnHelp        btnHelp-txt
    btnAbout       btnAbout-txt
    WITH FRAME frSettings IN WINDOW C-Win.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeVisuals C-Win 
PROCEDURE initializeVisuals :
/* Initialize all kind of visual things
 */
  {&timerStart}
  DEFINE VARIABLE cDatabases    AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cSetting      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iSetting      AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iColor        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iValue        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iField        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hColumn       AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iRgbValue     AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iStackSize    AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hFilterField  AS HANDLE    NO-UNDO.
  DEFINE VARIABLE IFilter       AS INTEGER   NO-UNDO.

  /* Make the color table large enough TO hold at least 25 fonts
   * Entries 0-23 are for the user and entry 24 will be used for
   * the color defined by "ButtonFace" ON the local system. We will
   * use this color if the user sets "use system colors"
   */
  IF COLOR-TABLE:NUM-ENTRIES < 25 THEN COLOR-TABLE:NUM-ENTRIES = 25.
  DO iColor = 0 TO COLOR-TABLE:NUM-ENTRIES - 1:
    /* Make all colors dynamic so the user can change them */
    COLOR-TABLE:SET-DYNAMIC(iColor, TRUE).

    /* And get the last saved value from the INI file */
    cSetting = getRegistry( "DataDigger:colors", SUBSTITUTE("color&1",iColor)).
    IF NUM-ENTRIES(cSetting) = 3 THEN
    DO:
      COLOR-TABLE:SET-RED-VALUE  (iColor, INTEGER(ENTRY(1,cSetting))).
      COLOR-TABLE:SET-GREEN-VALUE(iColor, INTEGER(ENTRY(2,cSetting))).
      COLOR-TABLE:SET-BLUE-VALUE (iColor, INTEGER(ENTRY(3,cSetting))).
    END.
  END.

  /* Get the RGB value for "ButtonFace" and set color 24 */
  RUN GetSysColor(15, OUTPUT iRgbValue).
  COLOR-TABLE:SET-RGB-VALUE(24, iRgbValue).

  /* Set icon */
  C-Win:LOAD-ICON(getImagePath("DataDigger.ico")).

  /* Where-FRAME */
  DO WITH FRAME frWhere:

    FRAME frWhere:FONT = giDefaultFont.

    BROWSE brTables:ROW-HEIGHT-PIXELS = FONT-TABLE:GET-TEXT-HEIGHT-PIXELS(giDefaultFont).
    BROWSE brFields:ROW-HEIGHT-PIXELS = FONT-TABLE:GET-TEXT-HEIGHT-PIXELS(giDefaultFont).
    BROWSE brIndexes:ROW-HEIGHT-PIXELS = FONT-TABLE:GET-TEXT-HEIGHT-PIXELS(giDefaultFont).

    cbAnDOr:FONT     = giFixedFont.
    cbFields:FONT    = giFixedFont.
    cbOperator:FONT  = giFixedFont.
    ficValue:FONT    = giFixedFont.
    ficWhere2:FONT   = giFixedFont.
    btnEq:FONT       = giFixedFont.
    btnNe:FONT       = giFixedFont.
    btnGt:FONT       = giFixedFont.
    btnLt:FONT       = giFixedFont.
    btnBracket:FONT  = giFixedFont.
    btnQt:FONT       = giFixedFont.
    btnAnd:FONT      = giFixedFont.
    btnOr:FONT       = giFixedFont.
    btnBegins:FONT   = giFixedFont.
    btnCONtains:FONT = giFixedFont.
    btnMatches:FONT  = giFixedFont.
    btnToday:FONT    = giFixedFont.

    btnInsert:LOAD-IMAGE(getImagePath("Add.gif")).
  END.

  /* Main FRAME */
  DO WITH FRAME {&FRAME-NAME}:

    /* > UI Stuff */
    FRAME frHint:FONT = giDefaultFont.
    FRAME frData:FONT = giDefaultFont.

    /* Fonts */
    c-win:FONT = giDefaultFont.
    FRAME frMain:FONT = giDefaultFont.
    ficWhere:FONT = giFixedFont.
    fiTableFilter:TOOLTIP = fiTableFilter:TOOLTIP + "~n~n(CTRL-ENTER) execute".

    /* Additional tooltips */
    ficValue     :TOOLTIP = ficValue:TOOLTIP + "~n~n(CTRL-ENTER) execute".
    ficWhere     :TOOLTIP = ficWhere:TOOLTIP + "~n~n(CTRL-ENTER) execute".
    brFields     :TOOLTIP = "fields of selected table" + "~n~n(RIGHT-CLICK) insert field+value".
    brFields     :TOOLTIP = brFields:TOOLTIP + "~n(CTRL-RIGHT-CLICK) insert field".
    brFields     :TOOLTIP = brFields:TOOLTIP + "~n(CTRL-ENTER) execute".
    brIndexes    :TOOLTIP = "indexes of the table" + "~n~n(RIGHT CLICK) use-index" + "~n(DOUBLE CLICK) use index in WHERE".

    /* Filter box around tables, fields and indexes */
    rcTableFilter:BGCOLOR = getColor("FilterBox:bg").
    rcFieldFilter:BGCOLOR = getColor("FilterBox:bg").
    rcIndexFilter:BGCOLOR = getColor("FilterBox:bg").

    /* Num selected records */
    DO WITH FRAME frData:
      fiNumRecords:FGCOLOR = getColor("RecordCount:Selected:fg").
      fiWarning:BGCOLOR    = getColor("WarningBox:bg").
      fiWarning:FGCOLOR    = getColor("WarningBox:fg").
    END.

    /* Load images for buttons */
    DO WITH FRAME frSettings:

      /* Set properties for settings FRAME */
      FRAME frSettings:FONT          = giDefaultFont.
      FRAME frSettings:WIDTH-PIXELS  = btnAbout-txt:X + btnAbout-txt:WIDTH-PIXELS + 4.
      FRAME frSettings:HEIGHT-PIXELS = btnAbout-txt:Y + btnAbout-txt:HEIGHT-PIXELS + 4.

      btnSettings:LOAD-IMAGE      (getImagePath("Settings.gif")).
      btnConnections:LOAD-IMAGE   (getImagePath("Connections.gif")).
      btnDict:LOAD-IMAGE          (getImagePath("Dictionary.gif")).
      btnProcEdit:LOAD-IMAGE      (getImagePath("Editor.gif")).
      btnQueries-3:LOAD-IMAGE     (getImagePath("SavedQueries.gif")).
      btnQueryTester:LOAD-IMAGE   (getImagePath("QTester.gif")).
      btnDataAdmin:LOAD-IMAGE(getImagePath("Administration.gif")).
      btnAbout:LOAD-IMAGE         (getImagePath("About.gif")).
      btnResizeVer:LOAD-IMAGE     (getImagePath("ResizeVer.gif")).
      btnDataDigger:LOAD-IMAGE    (getImagePath("DataDigger24x24.gif")).
      btnHelp:LOAD-IMAGE          (getImagePath("Help.gif")).

      /* Text buttons for tools menu */
      btnSettings-txt:FONT    = giDefaultFont.
      btnConnections-txt:FONT = giDefaultFont.
      btnDict-txt:FONT        = giDefaultFont.
      btnProcEdit-txt:FONT    = giDefaultFont.
      btnQueries-txt:FONT     = giDefaultFont.
      btnQueryTester-txt:FONT = giDefaultFont.
      btnDataAdmin-txt:FONT   = giDefaultFont.
      btnAbout-txt:FONT       = giDefaultFont.
      btnDataDigger-txt:FONT  = giDefaultFont.
      btnHelp-txt:FONT        = giDefaultFont.
    END.

    btnTools:LOAD-IMAGE           (getImagePath("Tools.gif")).

    btnTableFilter:LOAD-IMAGE     (getImagePath("Filter.gif")).
    btnClearTableFilter:LOAD-IMAGE(getImagePath("Clear.gif")).
    btnClearFieldFilter:LOAD-IMAGE(getImagePath("Clear.gif")).
    btnClearIndexFilter:LOAD-IMAGE(getImagePath("Clear.gif")).
    btnClearDataFilter:LOAD-IMAGE (getImagePath("Clear.gif")).
    btnDataSort:LOAD-IMAGE        (getImagePath("SortGroups.gif")).

    btnPrevQuery:LOAD-IMAGE       (getImagePath("Prev.gif")).
    btnNextQuery:LOAD-IMAGE       (getImagePath("Next.gif")).
    btnWhere:LOAD-IMAGE           (getImagePath("PopOut.gif")).
    btnViewData:LOAD-IMAGE        (getImagePath("Execute.gif")).
    btnClear:LOAD-IMAGE           (getImagePath("Clear.gif")).
    btnQueries:LOAD-IMAGE         (getImagePath("SavedQueries_small.gif")).
    btnClipboard:LOAD-IMAGE       (getImagePath("Clipboard.gif")).

    /* Same buttons ON editor FRAME */
    btnViewData-2:LOAD-IMAGE      (getImagePath("Execute.gif")).
    btnClear-2:LOAD-IMAGE         (getImagePath("Clear.gif")).
    btnQueries-2:LOAD-IMAGE       (getImagePath("SavedQueries_small.gif")).
    btnClipboard-2:LOAD-IMAGE     (getImagePath("Clipboard.gif")).

    /* Buttons for field ordering */
    btnMoveTop:LOAD-IMAGE         (getImagePath("First.gif")).
    btnMoveUp:LOAD-IMAGE          (getImagePath("Up.gif")).
    btnReset:LOAD-IMAGE           (getImagePath("Reset.gif")).
    btnMoveDown:LOAD-IMAGE        (getImagePath("Down.gif")).
    btnMoveBottom:LOAD-IMAGE      (getImagePath("Last.gif")).

    /* Add/clone etc */
    btnAdd:LOAD-IMAGE             (getImagePath("Add.gif")).
    btnClone:LOAD-IMAGE           (getImagePath("Clone.gif")).
    btnEdit:LOAD-IMAGE            (getImagePath("Edit.gif")).
    btnDelete:LOAD-IMAGE          (getImagePath("Delete.gif")).

    btnDump:LOAD-IMAGE            (getImagePath("Dump.gif")).
    btnLoad:LOAD-IMAGE            (getImagePath("Load.gif")).

    btnMoveUp:MOVE-TO-TOP().
    btnMoveDown:MOVE-TO-TOP().
    btnReset:MOVE-TO-TOP().
    btnMoveTop:MOVE-TO-TOP().
    btnMoveBottom:MOVE-TO-TOP().

    /* Set minimum size of the window */
    C-Win:MIN-WIDTH-PIXELS  = 650.
    C-Win:MIN-HEIGHT-PIXELS = 460.

    /* To avoid scrollbars on the frame */
    FRAME {&FRAME-NAME}:SCROLLABLE = FALSE.

  END. /* DO WITH FRAME */

  {&timerStop}
END PROCEDURE. /* initializeVisuals */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE keepAlive C-Win 
PROCEDURE keepAlive :
/* Query all databases to keep connection alive
 */
  {&timerStart}
  DEFINE VARIABLE hBuffer       AS HANDLE    NO-UNDO.
  DEFINE VARIABLE hTable        AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iDatabase     AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cTimeStamp    AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cChangedTable AS CHARACTER NO-UNDO.

  DEFINE BUFFER bfTable FOR ttTable.

  /* hBuffer::_dbstatus-cachestamp */
  DO iDatabase = 1 TO NUM-DBS:
    /* Skip non Progress databases [dataserver] */
    IF DBTYPE(iDatabase) NE "PROGRESS" THEN
      NEXT.

    CREATE BUFFER hBuffer FOR TABLE LDBNAME(iDatabase) + "._DbStatus".
    hBuffer:FIND-FIRST("",NO-LOCK).
    cTimeStamp = hBuffer::_dbstatus-cachestamp.

    FIND ttDatabase WHERE ttDatabase.cLogicalName = LDBNAME(iDatabase) NO-ERROR.
    IF NOT AVAILABLE ttDatabase THEN
    DO:
      CREATE ttDatabase.
      ASSIGN
        ttDatabase.cLogicalName = LDBNAME(iDatabase)
        ttDatabase.cCacheStamp  = cTimeStamp
        .
    END.
    DELETE OBJECT hBuffer.

    IF ttDatabase.cCacheStamp <> cTimeStamp THEN
    DO:
      /* Find table that has been changed */
      CREATE BUFFER hTable FOR TABLE LDBNAME(iDatabase) + "._File".

      findChangedTable:
      FOR EACH bfTable WHERE bfTable.cDatabase = LDBNAME(iDatabase):
        hTable:FIND-UNIQUE(SUBSTITUTE("WHERE _file-name = &1 AND _crc = &2"
                                     , QUOTER(bfTable.cTableName)
                                     , QUOTER(bfTable.cCrc)
                                     ), NO-LOCK) NO-ERROR.
        IF NOT hTable:AVAILABLE THEN
        DO:
          cChangedTable = SUBSTITUTE("&1.&2", bfTable.cDatabase, bfTable.cTableName).
          LEAVE findChangedTable.
        END.
      END. /* f/e table */
      DELETE OBJECT hTable.

      /* If we cannot find the table name, simply use database name */
      /* IF cChangedTable = "" THEN cChangedTable = ttDatabase.cLogicalName. */
      IF cChangedTable <> "" THEN
      DO:
        RUN unlockWindow(C-Win:HANDLE).
        RUN showHelp("SchemaRestart", cChangedTable ).
        IF getRegistry("DataDigger:help", "SchemaRestart:answer") = "1" THEN QUIT.
      END.

      /* We assign the value here again. This way, the user /CAN/ choose to
       * ignore the message and continue working without being either forced
       * to quit or repeatedly seeing this message.
       */
      ttDatabase.cCacheStamp = cTimeStamp.
    END.

  END.

  {&timerStop}

END PROCEDURE. /* keepAlive */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE menuDropDataBrowse C-Win 
PROCEDURE menuDropDataBrowse :
/* Enable / disable items in the context menu
 */
  DEFINE VARIABLE hMenuItem      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cColumnName    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cColumnValue   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cColumnClicked AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lOk            AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lColumnsHidden AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE iColumn        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE hColumn        AS HANDLE      NO-UNDO.

  /* Select the row we clicked on */
  RUN selectClickedRow(ghDataBrowse, OUTPUT lOk, OUTPUT cColumnClicked).
  IF NOT lOk THEN RETURN.
  setUpdatePanel('display'). /* Activate buttons */

  IF NUM-ENTRIES( ghDataBrowse:PRIVATE-DATA,CHR(1) ) = 3 THEN
    ASSIGN
      cColumnName  = ENTRY(1, ghDataBrowse:PRIVATE-DATA,CHR(1))
      cColumnValue = ENTRY(2, ghDataBrowse:PRIVATE-DATA,CHR(1))
      .

  /* If there are hidden columns, enable the menu-item 'unhide' */
  lColumnsHidden = CAN-FIND(FIRST ttField WHERE /* ttField.lDataField = true and */ ttField.lShow = FALSE).

  /* Enable/disable all current items */
  hMenuItem = ghDataBrowse:POPUP-MENU:FIRST-CHILD.

  DO WHILE VALID-HANDLE(hMenuItem):
    IF hMenuItem:SUBTYPE = 'normal' THEN
    DO:
      hMenuItem:LABEL = hMenuItem:PRIVATE-DATA.

      /* If we did not use right mouse click but shift-f10 then
       * we do not know the column name. In that case disable all
       * menu items that do something with the column value
       */
      IF cColumnClicked = '' AND LOOKUP(hMenuItem:NAME,'add,clone,edit,view,dump,load,delete') = 0 THEN
        hMenuItem:SENSITIVE = FALSE.
      ELSE
      DO WITH FRAME {&FRAME-NAME}:
        CASE hMenuItem:NAME:
          WHEN "add"    THEN hMenuItem:SENSITIVE = btnAdd:SENSITIVE    .
          WHEN "clone"  THEN hMenuItem:SENSITIVE = btnClone:SENSITIVE  .
          WHEN "edit"   THEN hMenuItem:SENSITIVE = btnEdit:SENSITIVE   .
          WHEN "view"   THEN hMenuItem:SENSITIVE = btnView:SENSITIVE   .
          WHEN "delete" THEN hMenuItem:SENSITIVE = btnDelete:SENSITIVE .
          WHEN "dump"   THEN hMenuItem:SENSITIVE = btnDump:SENSITIVE .
          WHEN "load"   THEN hMenuItem:SENSITIVE = btnLoad:SENSITIVE .
          OTHERWISE hMenuItem:SENSITIVE = TRUE.
        END CASE.
      END.

      /* Entry 'Unhide Columns' is only enabled when there is at least 1 hidden column */
      IF hMenuItem:NAME = 'unhideColumn' THEN
        hMenuItem:SENSITIVE = lColumnsHidden.
    END.

    /* if ghDataBrowse:query:num-results = 0 then hMenuItem:sensitive = no. */

    hMenuItem = hMenuItem:NEXT-SIBLING.
  END.

END PROCEDURE. /* menuDropDataBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE moveField C-Win 
PROCEDURE moveField :
/* Move a field up or down in the field browse.
 */
  DEFINE INPUT PARAMETER pcDirection AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cFieldOrder   AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iCounter      AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iCurrentRow   AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iColumnNr     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iOldOrder     AS INTEGER   NO-UNDO.
  DEFINE VARIABLE rCurrentField AS ROWID     NO-UNDO.

  DEFINE BUFFER bColumnOrg  FOR ttColumn.
  DEFINE BUFFER bColumn     FOR ttColumn.
  DEFINE BUFFER bFieldSwap  FOR ttField.
  DEFINE BUFFER bField      FOR ttField.

  /* Remember where we are in the field browse */
  rCurrentField = BROWSE brFields:QUERY:GET-BUFFER-HANDLE(1):ROWID.
  iCurrentRow   = BROWSE brFields:FOCUSED-ROW.

  CASE pcDirection:
    WHEN 'top'    THEN iCurrentRow = 1.
    WHEN 'up'     THEN IF iCurrentRow > 1  THEN iCurrentRow = iCurrentRow - 1.
    WHEN 'down'   THEN IF iCurrentRow < 10 THEN iCurrentRow = iCurrentRow + 1.
    WHEN 'bottom' THEN iCurrentRow = 10.
  END CASE.

  setWindowFreeze(YES).

  /* Find the active record */
  FIND bField WHERE ROWID(bField) = rCurrentField NO-ERROR.
  IF NOT AVAILABLE bField THEN RETURN.
  FIND FIRST bColumnOrg WHERE bColumnOrg.cFieldName = bField.cFieldName.
  iOldOrder = bField.iOrder.

  /* Change the order of the fields by 1.5
   * This sets the field exactly where we want it

    when 'top'    then bColumnOrg.iColumnNr = -1.
    when 'up'     then bColumnOrg.iColumnNr = bColumnOrg.iColumnNr - 1.5.
    when 'down'   then bColumnOrg.iColumnNr = bColumnOrg.iColumnNr + 1.5.
    when 'bottom' then bColumnOrg.iColumnNr = 999999999.

   */
  CASE pcDirection:
    WHEN 'top' THEN bField.iOrder = -1.

    WHEN 'up' THEN DO:
      FOR EACH bFieldSwap
        WHERE bFieldSwap.iOrder < bField.iOrder BY bFieldSwap.iOrder DESCENDING:

        ASSIGN
          bField.iOrder     = bFieldSwap.iOrder
          bFieldSwap.iOrder = iOldOrder.

        LEAVE.
      END.
    END.

    WHEN 'down' THEN DO:
      FOR EACH bFieldSwap
        WHERE bFieldSwap.iOrder  > bField.iOrder BY bFieldSwap.iOrder:

        ASSIGN
          bField.iOrder     = bFieldSwap.iOrder
          bFieldSwap.iOrder = iOldOrder.

        LEAVE.
      END.
    END.

    WHEN 'bottom' THEN bField.iOrder = 999999999.
  END CASE.

  /* Now apply 'normal' numbers to the Columns */
  iCounter = 0.
  REPEAT PRESELECT EACH bField BY bField.iOrder:
    FIND NEXT bField.
    ASSIGN
      iCounter      = iCounter + 1
      bField.iOrder = iCounter.
  END.

  /* Column follows field */
  iCounter = 0.
  FOR EACH bField BY bField.iOrder:
    cFieldOrder = TRIM(SUBSTITUTE("&1,&2",cFieldOrder, bField.cFullName),",").
    FOR EACH bColumn WHERE bColumn.cFieldName = bField.cFieldName BY bColumn.iExtent:
      ASSIGN
        iCounter          = iCounter + 1
        bColumn.iColumnNr = iCounter
        .
    END.
  END.

  /* Save changed order of the field. If it is blank, it will be deleted from registry */
  setRegistry( SUBSTITUTE('DB:&1',gcCurrentDatabase)
             , SUBSTITUTE('&1:fieldOrder', gcCurrentTable )
             , IF cFieldOrder <> getFieldList('iOrderOrg') THEN cFieldOrder ELSE ?
             ).

  /* Update field cache */
  RUN updateMemoryCache
    ( INPUT gcCurrentDatabase
    , INPUT gcCurrentTable
    , INPUT TABLE bField
    , INPUT TABLE bColumn
    ).

  RUN setDataBrowseColumns.

  /* And resort it */
  RUN reopenFieldBrowse('iOrder', YES).

  /* Reopen browse */
  BROWSE brFields:SET-REPOSITIONED-ROW(iCurrentRow,"ALWAYS").
  BROWSE brFields:QUERY:REPOSITION-TO-ROWID(rCurrentField).

  setWindowFreeze(NO).

END PROCEDURE. /* moveField */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE navigateSettings C-Win 
PROCEDURE navigateSettings :
/*
 * Navigate between the buttons in the settings frame
 */
  DEFINE INPUT  PARAMETER phButton AS HANDLE      NO-UNDO.
  DEFINE INPUT  PARAMETER pcAction AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE hButton    AS HANDLE EXTENT 10 NO-UNDO.
  DEFINE VARIABLE iButton    AS INTEGER NO-UNDO.
  DEFINE VARIABLE iNewButton AS INTEGER NO-UNDO.

  DO WITH FRAME frSettings:
    hButton[01] = btnDataDigger:HANDLE.
    hButton[02] = btnConnections:HANDLE.
    hButton[03] = btnSettings:HANDLE.
    hButton[04] = btnProcEdit:HANDLE.
    hButton[05] = btnDict:HANDLE.
    hButton[06] = btnDataAdmin:HANDLE.
    hButton[07] = btnQueries-3:HANDLE.
    hButton[08] = btnQueryTester:HANDLE.
    hButton[09] = btnHelp:HANDLE.
    hButton[10] = btnAbout:HANDLE.

    DO iButton = 1 TO 10:
      IF hButton[iButton] = phButton THEN
      DO:
        CASE pcAction:
          WHEN 'cursor-down'  THEN iNewButton = iButton + 2.
          WHEN 'cursor-up'    THEN iNewButton = iButton - 2.
          WHEN 'cursor-right' THEN iNewButton = iButton + 1.
          WHEN 'cursor-left'  THEN iNewButton = iButton - 1.
          OTHERWISE RETURN.
        END CASE.

        /* Special cases */
        IF iButton = 2 AND iNewButton =  0 THEN iNewButton = 2.
        IF iButton = 9 AND iNewButton = 11 THEN iNewButton = 9.

        IF iNewButton < 1  THEN iNewButton = 1.
        IF iNewButton > 10 THEN iNewButton = 10.
        APPLY "ENTRY" TO hButton[iNewButton].
      END.
    END.

  END.

END PROCEDURE. /* navigateSettings */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE pasteFromClipboard C-Win 
PROCEDURE pasteFromClipboard :
/* Paste value from clipboard to a widget
 */
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.
  phWidget:SCREEN-VALUE = CLIPBOARD:VALUE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE preCache C-Win 
PROCEDURE preCache :
/* Pre-cache tables that have been queried at least once in the last month
 */
  {&timerStart}
  DEFINE VARIABLE lDoneSomething AS LOGICAL NO-UNDO.
  DEFINE BUFFER bTable FOR ttTable.

  FOR EACH bTable
    WHERE bTable.iNumQueries > 0
      AND bTable.lCached = FALSE
      AND bTable.tLastUsed > DATETIME(TODAY - 31)
       BY bTable.tLastUsed DESCENDING:

    PUBLISH "debugMessage" (1, SUBSTITUTE("Pre-Cache &1.&2. Last used &3"
                                         , bTable.cDatabase
                                         , bTable.cTableName
                                         , bTable.tLastUsed
                                         )).
    lDoneSomething = TRUE.

    /* Get fields. This will create the cache if needed */
    RUN getFields(bTable.cDatabase, bTable.cTableName, OUTPUT DATASET dsFieldCache).
    bTable.lCached = TRUE.

    /* Thanks, but not needed anymore */
    DATASET dsFieldCache:EMPTY-DATASET.

    /* One table at a time */
    LEAVE.
  END.

  /* If we have not done anything, it means we cached all
   * tables we want to cache, so stop caching for this session.
   */
  IF NOT lDoneSomething THEN
  DO:
    RUN setTimer("PreCache",0).
    PUBLISH "debugMessage" (1, SUBSTITUTE("Pre-Caching complete")).
  END.

  {&timerStop}
END PROCEDURE. /* preCache */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE processQuery C-Win 
PROCEDURE processQuery :
/* Move a field up or down in the field browse.
 */
  DEFINE INPUT PARAMETER ipcQueryString AS CHARACTER NO-UNDO.

  DEFINE VARIABLE iLastQuery AS INTEGER NO-UNDO.
  DEFINE VARIABLE iWord      AS INTEGER NO-UNDO.

  DEFINE BUFFER ttTestQuery FOR ttTestQuery.

  /* <BEU> */
  /* FORWARD-ONLY attribute:                                                             */
  /* Lets you avoid building result-lists for static and dynamic queries. Set to TRUE to */
  /* avoid building result-lists for queries. Set to FALSE to build result-lists for     */
  /* queries. The default is FALSE. When TRUE, you cannot use the GET PREV, GET LAST,    */
  /* REPOSITION, or BROWSE methods or statements with these queries. If you do, the AVM  */
  /* generates an error.                                                                 */
  ipcQueryString = TRIM(ipcQueryString).
  ipcQueryString = REPLACE(ipcQueryString,"INDEXED-REPOSITION","").
  /* </BEU> */

  /* The highest query nr is the first record (index is descending) */
  FIND FIRST ttTestQuery NO-ERROR.
  iLastQuery = (IF AVAILABLE ttTestQuery THEN ttTestQuery.iId ELSE 0) + 1.

  /* Don't save this one if its already in the tt
   * Just move it up the stack.
   */
  FIND FIRST ttTestQuery WHERE ttTestQuery.cQuery = ipcQueryString NO-ERROR.
  IF AVAILABLE ttTestQuery THEN
  DO:
    ttTestQuery.iId = iLastQuery.
    RETURN.
  END.

  /* Save this query */
  CREATE ttTestQuery.
  ASSIGN
    ttTestQuery.iId       = iLastQuery
    ttTestQuery.cProgName = ""
    ttTestQuery.cQuery    = ipcQueryString
    .

  /* Find table name in the query */
  findTable:
  DO iWord = 1 TO NUM-ENTRIES(ttTestQuery.cQuery," "):
    IF CAN-DO("EACH,LAST,FIRST", ENTRY(iWord,ttTestQuery.cQuery," ")) THEN
    DO:
      ttTestQuery.cProgName = ENTRY(iWord + 1,ttTestQuery.cQuery," ").
      LEAVE findTable.
    END.
  END.

END PROCEDURE. /* processQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenDataBrowse C-Win 
PROCEDURE reopenDataBrowse :
/* Build the query, based on where-box and filter fields
 */
  DEFINE VARIABLE cFullTable     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cUserQuery     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBufferDB      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery         AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iNumRecords    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartTime     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lPrepare       AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE rCurrentRecord AS ROWID       NO-UNDO.
  DEFINE VARIABLE lQueryComplete AS LOGICAL     NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.
  {&timerStart}

  /* Freeze! */
  SESSION:SET-WAIT-STATE('general').
  setWindowFreeze(YES).

  /* In case we come from a filter field, that field needs
   * to have the leave event to restore the shadow text.
   */
  IF VALID-HANDLE(FOCUS) THEN APPLY "LEAVE" TO FOCUS.

  cFullTable = gcCurrentDatabase + '.' + gcCurrentTable.

  /* Increase query counter */
  RUN incQueriesOfTable(gcCurrentDatabase, gcCurrentTable, +1).
  RUN incQueriesServed(+1).

  /* If the user has changed a format in the field browse, then rebuild the data browse */
  IF glFormatChanged THEN
  DO:
    RUN reopenDataBrowse-create(INPUT gcCurrentDatabase, INPUT gcCurrentTable).
    glFormatChanged = FALSE.
  END.

  /* Remember currently selected record */
  IF VALID-HANDLE(ghDataBrowse) AND ghDataBrowse:NUM-SELECTED-ROWS > 0 THEN
    rCurrentRecord = ghDataBrowse:QUERY:GET-BUFFER-HANDLE(1):ROWID.

  /* If we do a query on the _lock table then create and fill a temp-table */
  IF gcCurrentTable = '_lock' THEN
  DO:
    cFullTable = '_Lock'.

    /* Empty the Lock TT */
    ghDataBuffer:EMPTY-TEMP-TABLE().

    CREATE BUFFER hBufferDB FOR TABLE gcCurrentDatabase + '._lock'.

    CREATE QUERY hQuery.
    hQuery:ADD-BUFFER(hBufferDB).
    hQuery:QUERY-PREPARE(SUBSTITUTE('for each &1._lock no-lock', gcCurrentDatabase)).

    hQuery:QUERY-OPEN().
    REPEAT:
      hQuery:GET-NEXT().
      IF hQuery:QUERY-OFF-END THEN LEAVE.
      IF NOT hBufferDB:AVAILABLE THEN LEAVE.
      IF hBufferDB::_Lock-Usr = ? THEN LEAVE.
      ghDataBuffer:BUFFER-CREATE().
      ghDataBuffer:BUFFER-COPY(hBufferDB).
    END.
    hQuery:QUERY-CLOSE().

    DELETE OBJECT hQuery.
    DELETE OBJECT hBufferDB.
  END. /* table = _Lock */

  /* Reset query pointer */
  giQueryPointer = 1.
  RUN getDataQuery(OUTPUT cQuery).
  cUserQuery = ficWhere:SCREEN-VALUE IN FRAME {&FRAME-NAME}. /* this one will be saved if it has no errors */
  cQuery = REPLACE(cQuery, SUBSTITUTE("&1._lock", gcCurrentDatabase), cFullTable).

  /* Extract sorting from query */
  RUN getSortFromQuery(cQuery).

  /* Create a new sort table */
  RUN createSortTable.

  /* Rewrite query to include sorting */
  RUN getSortedQuery(INPUT-OUTPUT cQuery).

  /* Show sorts in the browse */
  RUN setSortArrows(ghDataBrowse).

  /* for DWP query tester */
  PUBLISH "debugMessage" (INPUT 1, "cQuery = " + cQuery ).
  PUBLISH 'query' (INPUT cQuery).

  /* Try to open it */
  lPrepare = ghDataQuery:QUERY-PREPARE(cQuery) NO-ERROR.

  /* if the QUERY-PREPARE failed because of the where-clause, don't open it */
  IF NOT lPrepare THEN
  DO WITH FRAME {&FRAME-NAME}:

    ficWhere:TOOLTIP = TRIM(ERROR-STATUS:GET-MESSAGE(1)).
    ficWhere:BGCOLOR = getColor('QueryError:fg'). /* red */
    ficWhere:FGCOLOR = getColor('QueryError:bg'). /* yellow */

    /* Activate buttons */
    setUpdatePanel('no-data').

    APPLY "entry" TO ficWhere.
  END.
  ELSE
  DO WITH FRAME {&FRAME-NAME}:
    ficWhere:BGCOLOR = ?. /* default */
    ficWhere:FGCOLOR = ?. /* default */
    ficWhere:TOOLTIP = getReadableQuery(cQuery).

    /* Save the user-query and set the pointer to 1 */
    RUN saveQuery(gcCurrentDatabase, gcCurrentTable, cUserQuery).

    /* Try to grab as many records as we can in a limited time.
     * This will give an indication of the amount of records.
     */
    ghDataQuery:QUERY-OPEN().
    iStartTime = ETIME.
    DO WHILE (ETIME - iStartTime) < giMaxQueryTime AND NOT ghDataQuery:QUERY-OFF-END:
      ghDataQuery:GET-NEXT.
      iNumRecords = iNumRecords + 1.
    END.
    lQueryComplete = ghDataQuery:QUERY-OFF-END.

    /* query might have gotten off end, so: */
    IF lQueryComplete THEN ghDataQuery:QUERY-OPEN().

    /* Show nr of records
     * Sometimes opening of the query takes some time so no records can be counted in
     * the query-time-out period. So then DD shows "> 0 records" while the actual browse
     * shows some records. This is odd, so we then set the nr of records to that of the browse.
     */
    IF ghDataBrowse:QUERY:NUM-RESULTS > iNumRecords THEN
      iNumRecords = ghDataBrowse:QUERY:NUM-RESULTS.

    /* Jump back to selected row */
    IF NOT ghDataBrowse:QUERY:QUERY-OFF-END
      AND rCurrentRecord <> ? THEN
    DO:
      ghDataBrowse:QUERY:REPOSITION-TO-ROWID(rCurrentRecord) NO-ERROR.
      ghDataBrowse:SELECT-FOCUSED-ROW().
    END.

    /* Activate buttons */
    setUpdatePanel('display').
  END.

  RUN showNumRecords(iNumRecords, lQueryComplete).
  RUN showNumSelected.

  /* Show or hide red line around filters */
  rctDataFilter:VISIBLE IN FRAME frData = FALSE.
  FOR EACH bColumn WHERE VALID-HANDLE(bColumn.hColumn):
    IF bColumn.hFilter:SCREEN-VALUE <> bColumn.cFullName
      AND bColumn.hFilter:SCREEN-VALUE <> ""
      AND bColumn.hFilter:SCREEN-VALUE <> ? THEN
    DO:
      rctDataFilter:VISIBLE IN FRAME frData = TRUE.
      LEAVE.
    END.
  END.

  /* For some reasons, these #*$&# scrollbars keep coming back */
  RUN showScrollBars(FRAME {&frame-name}:handle, NO, NO). /* KILL KILL KILL */

  /* Unfreeze it */
  SESSION:SET-WAIT-STATE('').
  setWindowFreeze(NO).
  APPLY 'entry' TO ghDataBrowse.

  {&timerStop}
END PROCEDURE. /* reopenDataBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenDataBrowse-create C-Win 
PROCEDURE reopenDataBrowse-create :
/* Create the browse and open the query
 */
  {&timerStart}
  DEFINE INPUT PARAMETER pcDatabase AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pcTable    AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cCustomValue   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cColumnName    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFilterHistory AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFullTable     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cMyFormat      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hField         AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFilterField   AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hMenu          AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hMenuItem      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iColumn        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iColumnWidth   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMinWidth      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iPos           AS INTEGER     NO-UNDO.

  DEFINE BUFFER bField  FOR ttField.
  DEFINE BUFFER bColumn FOR ttColumn.
  DEFINE BUFFER bFilter FOR ttFilter.

  /* Protect against rubbish */
  IF pcTable = "" THEN RETURN.

  PUBLISH "debugMessage" (1, SUBSTITUTE("Create browse for &1.&2", pcDatabase, pcTable )).

  setWindowFreeze(YES).

  /* Clean up old stuff */
  IF VALID-HANDLE(ghDataBuffer) THEN
    PUBLISH "debugMessage" (1, SUBSTITUTE("Old DataBuffer:&1 &2",ghDataBuffer, ghDataBuffer:NAME )).

  RUN deleteDataFilters(ghDataBrowse).
  EMPTY TEMP-TABLE ttQuerySort.

  IF VALID-HANDLE(ghDataBrowse) AND VALID-HANDLE(ghDataBrowse:QUERY) THEN DELETE OBJECT ghDataBrowse:QUERY NO-ERROR.
  IF VALID-HANDLE(ghDataBrowse) THEN DELETE OBJECT ghDataBrowse NO-ERROR.
  IF VALID-HANDLE(ghLockTable)  THEN DELETE OBJECT ghLockTable  NO-ERROR.
  IF VALID-HANDLE(ghDataBuffer) THEN DELETE OBJECT ghDataBuffer NO-ERROR.
  rctDataFilter:VISIBLE IN FRAME frData = FALSE.

  cFullTable = pcDatabase + "." + pcTable.

  /* For _LOCK, create a dynamic TT to prevent choking */
  IF pcTable = "_Lock" THEN
  DO:
    cFullTable = pcTable.
    CREATE TEMP-TABLE ghLockTable.
    ghLockTable:CREATE-LIKE(pcDatabase + "._lock").
    ghLockTable:TEMP-TABLE-PREPARE("_Lock").
    ghDataBuffer = ghLockTable:DEFAULT-BUFFER-HANDLE.
  END.
  ELSE
  DO:
    CREATE BUFFER ghDataBuffer FOR TABLE cFullTable.
  END.

  /* Create a base query on the table. */
  CREATE QUERY ghDataQuery.
  ghDataQuery:SET-BUFFERS(ghDataBuffer).
  ghDataQuery:QUERY-PREPARE(SUBSTITUTE("FOR EACH &1 NO-LOCK WHERE FALSE", cFullTable)).

  /* Start building */
  CREATE BROWSE ghDataBrowse
    ASSIGN
    NAME              = "brData"
    FRAME             = FRAME frData:HANDLE
    QUERY             = ghDataQuery
    MULTIPLE          = TRUE
    X                 = rctData:X + 3
    Y                 = rctData:Y + 5 + 21 /* extra space for filters */
    WIDTH-PIXELS      = rctData:WIDTH-PIXELS - 10
    HEIGHT-PIXELS     = rctData:HEIGHT-PIXELS - 10 - 23 /* extra space for filters */
    ROW-MARKER        = TRUE
    SEPARATORS        = TRUE
    READ-ONLY         = FALSE
    SENSITIVE         = TRUE
    VISIBLE           = FALSE
    NO-VALIDATE       = TRUE
    COLUMN-RESIZABLE  = TRUE
    COLUMN-SCROLLING  = TRUE /* scroll with whole columns at a time */
    CONTEXT-HELP-ID   = 100
    TRIGGERS:
      ON "CTRL-A"           PERSISTENT RUN dataSelectAll           IN THIS-PROCEDURE (ghDataBrowse).
      ON "CTRL-D"           PERSISTENT RUN dataSelectNone          IN THIS-PROCEDURE (ghDataBrowse).
      ON "CTRL-J"           PERSISTENT RUN reopenDataBrowse        IN THIS-PROCEDURE.
      ON "ROW-DISPLAY"      PERSISTENT RUN dataRowDisplay          IN THIS-PROCEDURE (ghDataBuffer).
      ON "START-SEARCH"     PERSISTENT RUN dataColumnSort          IN THIS-PROCEDURE.
      ON "INSERT-MODE"      PERSISTENT RUN btnAddChoose            IN THIS-PROCEDURE.
      ON "ALT-A"            PERSISTENT RUN btnAddChoose            IN THIS-PROCEDURE.
      ON "SHIFT-INS"        PERSISTENT RUN btnCloneChoose          IN THIS-PROCEDURE.
      ON "ALT-O"            PERSISTENT RUN btnCloneChoose          IN THIS-PROCEDURE.
      ON "ALT-E"            PERSISTENT RUN btnEditChoose           IN THIS-PROCEDURE.
      ON "DELETE-CHARACTER" PERSISTENT RUN btnDeleteChoose         IN THIS-PROCEDURE.
      ON "VALUE-CHANGED"    PERSISTENT RUN dataRowValueChanged     IN THIS-PROCEDURE (ghDataBuffer).
      ON "END"              PERSISTENT RUN dataRowJumpToEnd        IN THIS-PROCEDURE (ghDataBuffer).
      ON "SCROLL-NOTIFY"    PERSISTENT RUN dataScrollNotify        IN THIS-PROCEDURE (ghDataBrowse).
      ON "DEFAULT-ACTION"   PERSISTENT RUN dataDoubleClick         IN THIS-PROCEDURE (ghDataBrowse).
      ON "OFF-HOME"         PERSISTENT RUN dataOffHome             IN THIS-PROCEDURE.
      ON "CTRL-CURSOR-UP"   PERSISTENT RUN dataGotoFilter          IN THIS-PROCEDURE.
      ON "F5"               PERSISTENT RUN filterDataBrowse        IN THIS-PROCEDURE.
      ON "ENTRY"            PERSISTENT RUN setTimer                IN THIS-PROCEDURE ("timedScrollNotify", 100).
      ON "LEAVE"            PERSISTENT RUN setTimer                IN THIS-PROCEDURE ("timedScrollNotify", 0).
    END TRIGGERS.

  /* Add the columns to the browse */
  gcDataBrowseColumns = "".
  gcDataBrowseColumnNames = "".
  iColumn = 0.

  addColumnLoop:
  FOR EACH bField BY bField.iOrder:

    /* Some VSTs have fields with strange data-types. DD will give errors
     * when it tries to create columns for these, so we will skip them
     */
    IF pcTable BEGINS "_"
      AND LOOKUP(ENTRY(1,bField.cDataType,"["),"date,decimal,integer,int64,logical,datetime,datetime-tz,character,blob,clob,raw,recid,rowid") = 0 THEN NEXT.

    /* Walk thru all extents of this field. May be only one! */
    FOR EACH bColumn
      WHERE bColumn.cTableCacheId = bField.cTableCacheId
        AND bColumn.cFieldName    = bField.cFieldName
         BY bColumn.iColumnNr:

      /* Protect against too much columns. This gives error:
       * SYSTEM ERROR: stkpush: stack overflow. Increase the -s parameter. (279)
       */
      iColumn = iColumn + 1.
      IF iColumn > giMaxColumns THEN LEAVE addColumnLoop.

      /* Recid and Rowid column */
      IF CAN-DO("RECID,ROWID", bColumn.cFieldName) THEN
      DO:
        bColumn.hColumn = ghDataBrowse:ADD-CALC-COLUMN(bField.cDataType, bField.cFormat, "", bColumn.cFullName).

        bColumn.hColumn:NAME      = bColumn.cFieldName.
        bColumn.hColumn:VISIBLE   = bField.lShow.
        bColumn.hColumn:READ-ONLY = TRUE.
        bColumn.hColumn:LABEL     = getColumnLabel(BUFFER bColumn:HANDLE).
      END.

      ELSE
      DO:
        /* Get handle to this field in the buffer */
        hField = ghDataBuffer:BUFFER-FIELD(bField.cFieldName):HANDLE NO-ERROR.

        /* It seems that it is not possible to refresh the schema cache of the running
         * session. You just have to restart your session when the schema is changed.
         */
        IF ERROR-STATUS:ERROR THEN
        DO:
          RUN unlockWindow(C-Win:HANDLE).
          RUN showHelp("SchemaRestart", SUBSTITUTE("&1.&2", bColumn.cDatabase, bColumn.cTableName)).
          IF getRegistry("DataDigger:help", "SchemaRestart:answer") = "1" THEN QUIT.
        END.

        /* For some strange reason the format is not saved when you change the format in the field browse
         * and press CTRL-ENTER while still in the format field. Possibly due to order of triggers or so.
         * Anyway, by getting the most recent format from registry (which is cached anyway) all is ok.
         */
        cMyFormat = getRegistry( SUBSTITUTE("DB:&1",pcDatabase)
                               , SUBSTITUTE("&1.&2:format",pcTable, bField.cFieldName)
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
          /* Add minus sign if needed. Because a format can contain extra characters like "$"
           * we need to find out what the right place might be to add it. If the format begins
           * with extra chars, we add it at the back. But if there are extra chars too, we just
           * cannot add the extra minus char. So be it :(
           */
          IF INDEX(cMyFormat,"-") = 0
            AND INDEX(cMyFormat,"+") = 0 THEN
          DO:
            IF cMyFormat BEGINS '9' OR cMyFormat BEGINS '>' THEN cMyFormat = "-" + cMyFormat.
            ELSE
            IF cMyFormat MATCHES '*9' THEN cMyFormat = cMyFormat + "-".
          END.

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
            /* If there is something wrong the the format, just reset it to the original format */
            RUN unlockWindow(C-Win:HANDLE).

            /* Delete wrong format from ini file */
            setRegistry(SUBSTITUTE("DB:&1",gcCurrentDatabase), SUBSTITUTE("&1.&2:format",pcTable,bField.cFieldName), ?).
            bField.cFormat = bField.cFormatOrg.
            BROWSE brFields:REFRESH().

            hField:FORMAT = bField.cFormat NO-ERROR.
            RUN showHelp("FormatError", cMyFormat + "," + bField.cFormatOrg).
          END.
        END.

        /* Add a calculated column for integers with time format */
        cColumnName = SUBSTITUTE("&1.&2", cFullTable, bColumn.cFullName).

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

        /* Set label and name */
        bColumn.hColumn:LABEL     = getColumnLabel(BUFFER bColumn:HANDLE).
        bColumn.hColumn:NAME      = bColumn.cFullName.
        bColumn.hColumn:READ-ONLY = TRUE.
        bColumn.hColumn:VISIBLE   = bField.lShow.
      END. /* not recid/rowid */

      ON "end-resize" OF bColumn.hColumn PERSISTENT RUN dataColumnResize IN THIS-PROCEDURE(bColumn.hColumn).

      /* Build a list of column handles for the rowDisplay trigger */
      ASSIGN
        gcDataBrowseColumns     = gcDataBrowseColumns     + "," + STRING(bColumn.hColumn)
        gcDataBrowseColumnNames = gcDataBrowseColumnNames + "," + bColumn.cFullName
        .

      /* Create a filterfield for this column
       * If the user doesn't want history, we will show a normal fill-in
       */
      IF giMaxFilterHistory = 0 THEN
        CREATE FILL-IN hFilterField
          ASSIGN
            FRAME         = ghDataBrowse:FRAME
            NAME          = "filter_" + bColumn.cFullName
            X             = ghDataBrowse:X
            Y             = rctData:Y + 5 + 21 - 23 /* Extra space for filters */
            WIDTH-PIXELS  = 10
            HEIGHT-PIXELS = 21
            SENSITIVE     = TRUE
            VISIBLE       = FALSE
            FORMAT        = "x(40)"
            PRIVATE-DATA  = bColumn.cFullName
            SCREEN-VALUE  = bColumn.cFullName
          .
      ELSE
      DO:
        CREATE COMBO-BOX hFilterField
          ASSIGN
            SUBTYPE       = "DROP-DOWN"
            FRAME         = ghDataBrowse:FRAME
            NAME          = "filter_" + bColumn.cFullName
            X             = ghDataBrowse:X
            Y             = rctData:Y + 5 + 21 - 23 /* Extra space for filters */
            WIDTH-PIXELS  = 10
            SENSITIVE     = TRUE
            VISIBLE       = FALSE
            FORMAT        = "x(40)"
            PRIVATE-DATA  = bColumn.cFullName
            SCREEN-VALUE  = bColumn.cFullName
            INNER-LINES   = MINIMUM(10,giMaxFilterHistory)
            DELIMITER     = CHR(1)
            .

        /* Place search history in the combo */
        cFilterHistory = getRegistry( SUBSTITUTE("DB:&1",pcDatabase), SUBSTITUTE("&1.&2:FilterHistory",pcTable,bColumn.cFullName)).
        IF cFilterHistory = ? THEN cFilterHistory = "".
        cFilterHistory = TRIM(cFilterHistory,CHR(1)).
        IF NUM-ENTRIES(cFilterHistory,CHR(1)) > 0 THEN hFilterField:LIST-ITEMS = cFilterHistory.

        /* Add context menu to combo */
        hMenu = createMenu(hFilterField).
        hFilterField:POPUP-MENU = hMenu.

        /* Clear all filters */
        hMenuItem = createMenuItem(hMenu,"Item","Clear All &Filters","clearAllFilters").
        ON "CHOOSE" OF hMenuItem PERSISTENT RUN applyEvent IN THIS-PROCEDURE (btnClearDataFilter:handle,"choose").

        /* Clear history */
        hMenuItem = createMenuItem(hMenu,"Item","Clear &History","clearDataFilter").
        ON "CHOOSE" OF hMenuItem PERSISTENT RUN clearDataFilter IN THIS-PROCEDURE (hFilterField).

        /* Sort list */
        hMenuItem = createMenuItem(hMenu,"Item","&Sort List","sortDataFilter").
        ON "CHOOSE" OF hMenuItem PERSISTENT RUN sortComboBox IN THIS-PROCEDURE (hFilterField).

        /* RULE / Cut / Copy / Paste / Delete */
        hMenuItem = createMenuItem(hMenu,"RULE","","rule").

        /* Cut */
        hMenuItem = createMenuItem(hMenu,"ITEM","Cut","cut").
        ON "CHOOSE" OF hMenuItem PERSISTENT RUN cutToClipboard IN THIS-PROCEDURE (hFilterField).

        /* Copy */
        hMenuItem = createMenuItem(hMenu,"ITEM","C&opy","copy").
        ON "CHOOSE" OF hMenuItem PERSISTENT RUN copyToClipboard IN THIS-PROCEDURE (hFilterField).

        /* Paste */
        hMenuItem = createMenuItem(hMenu,"ITEM","Paste","paste").
        ON "CHOOSE" OF hMenuItem PERSISTENT RUN pasteFromClipboard IN THIS-PROCEDURE (hFilterField).

        /* Delete */
        hMenuItem = createMenuItem(hMenu,"ITEM","Delete","delete").
        ON "CHOOSE" OF hMenuItem PERSISTENT RUN clearField IN THIS-PROCEDURE (hFilterField).

      END. /* combo */

      /* triggers */
      ON "CTRL-A"           OF hFilterField PERSISTENT RUN dataSelectAll           IN THIS-PROCEDURE (ghDataBrowse).
      ON "CTRL-D"           OF hFilterField PERSISTENT RUN dataSelectNone          IN THIS-PROCEDURE (ghDataBrowse).
      ON "ENTRY"            OF hFilterField PERSISTENT RUN filterFieldEntry        IN THIS-PROCEDURE (hFilterField).
      ON "LEAVE"            OF hFilterField PERSISTENT RUN filterFieldLeave        IN THIS-PROCEDURE (hFilterField).
      ON "VALUE-CHANGED"    OF hFilterField PERSISTENT RUN filterFieldValueChanged IN THIS-PROCEDURE (hFilterField,NO).
      ON "SHIFT-DEL"        OF hFilterField PERSISTENT RUN filterFieldClearAll     IN THIS-PROCEDURE (hFilterField, btnClearDataFilter:HANDLE).
      ON "RETURN"           OF hFilterField PERSISTENT RUN filterDataBrowse        IN THIS-PROCEDURE.
      ON "F2"               OF hFilterField PERSISTENT RUN filterDataBrowse        IN THIS-PROCEDURE.
      ON "F5"               OF hFilterField PERSISTENT RUN filterDataBrowse        IN THIS-PROCEDURE.
      ON "CTRL-CURSOR-DOWN" OF hFilterField PERSISTENT RUN filterFieldCursorDown   IN THIS-PROCEDURE (hFilterField, bColumn.hColumn).

      /* Keep track of filters */
      CREATE bFilter.
      ASSIGN
        bFilter.cFieldName = bColumn.cFullName
        bFilter.hFilter    = hFilterField
        bFilter.hColumn    = bColumn.hColumn
        bFilter.hBrowse    = ghDataBrowse
        .

      cCustomValue = hFilterField:SCREEN-VALUE.
      PUBLISH "CustomGetFilterValue" (pcDatabase, pcTable, bField.cFieldName, INPUT-OUTPUT cCustomValue).
      IF cCustomValue <> ? THEN hFilterField:SCREEN-VALUE = cCustomValue.

      /* Connect filter to field and set color */
      bColumn.hFilter = hFilterField.
      setFilterFieldColor(hFilterField).
    END. /* f/e bColumn */

  END. /* addColumnLoop */

  gcDataBrowseColumns     = TRIM(gcDataBrowseColumns,",").
  gcDataBrowseColumnNames = TRIM(gcDataBrowseColumnNames,",").

  /* Create the context menu for the databrowse if that has not been done yet */
  RUN createMenuDataBrowse.

  /* Show the browse */
  ghDataBrowse:VISIBLE = TRUE.

  /* Limit fields to a max of 300px wide
   * This must be done after the browse is realized
   */
  adjustFilterLoop:
  FOR EACH bColumn WHERE VALID-HANDLE(bColumn.hColumn):

    /* Get last defined width from registry. Might have been set by user */
    iColumnWidth = INTEGER(getRegistry( SUBSTITUTE("DB:&1",pcDatabase)
                                      , SUBSTITUTE("&1.&2:width", pcTable, bColumn.cFullname)) ) NO-ERROR.

    /* Make sure it is not wider than 300px */
    IF iColumnWidth = ? THEN iColumnWidth = MINIMUM(300, bColumn.hColumn:WIDTH-PIXELS).

    /* Make sure the column is at least as wide as its name */
    /* And if the filter is of type COMBO, reserve some extra space for the arrow down */
    iMinWidth = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(bColumn.hColumn:NAME,getFont("default")).
    IF giMaxFilterHistory > 0 THEN iMinWidth = iMinWidth + 21.
    IF iColumnWidth < iMinWidth THEN iColumnWidth = iMinWidth.

    bColumn.hColumn:WIDTH-PIXELS = iColumnWidth.
  END.

  /* Activate buttons */
  setUpdatePanel("no-record").
  RUN showNumSelected.

  /* Adjust all filters */
  RUN dataScrollNotify(ghDataBrowse).

  /* Reset the TAB order of the filter fields */
  RUN setFilterFieldTabOrder.

  setWindowFreeze(NO).

  /* Show warning when too much columns */
  IF ghDataBrowse:NUM-COLUMNS >= giMaxColumns THEN
  DO:
    RUN unlockWindow(C-Win:HANDLE).
    RUN showHelp("TooManyColumns", giMaxColumns).
  END.
  ELSE
    fiWarning:VISIBLE IN FRAME {&FRAME-NAME} = NO.

  /* Show warning when extent fields have been suppressed */
  IF giMaxExtent > 0
    AND CAN-FIND(FIRST ttField WHERE ttField.iExtent > giMaxExtent) THEN
  DO:
    RUN unlockWindow(C-Win:HANDLE).
    RUN showHelp("TooManyExtents", giMaxExtent).
  END.

  {&timerStop}
END PROCEDURE. /* reopenDataBrowse-create */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenFieldBrowse C-Win 
PROCEDURE reopenFieldBrowse :
/* Open the field browse again, taking into account the filter values the user has entered.
 */
  DEFINE INPUT PARAMETER pcSortField  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER plAscending  AS LOGICAL     NO-UNDO.

  DEFINE VARIABLE cFilterValue     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFormatFilter    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cInitialFilter   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLabelFilter     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNameFilter      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNewSort         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldSort         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOperator        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOrderFilter     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery           AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTypeFilter      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBuffer          AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hColumn          AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iColumn          AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lAscending       AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lFieldsFound     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lAllVisible      AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE rCurrentRecord   AS ROWID       NO-UNDO.

  DEFINE BUFFER bFilter FOR ttFilter.

  DO WITH FRAME frMain:

    /* Protect routine against invalid input */
    IF pcSortField = '' THEN pcSortField = ?.

    /* Remember record we're on */
    IF brFields:num-selected-rows > 0 THEN
      rCurrentRecord = brFields:query:get-buffer-handle(1):rowid.

    /* Find out what the current sort is */
    RUN getColumnSort(INPUT brFields:handle, OUTPUT cOldSort, OUTPUT lAscending).

    /* If no new sortfield is provided, we don't want to change the sort.
     * This happens when we press the filter button.
     */
    IF pcSortField = ? THEN
      ASSIGN
        cNewSort   = cOldSort
        lAscending = lAscending. /* dont change order */
    ELSE
    IF pcSortField = cOldSort THEN
      ASSIGN
        cNewSort   = cOldSort
        lAscending = NOT lAscending. /* invert order */
    ELSE
      /* New field */
      ASSIGN
        cNewSort   = pcSortField
        lAscending = TRUE.

    /* Sort direction might be overruled */
    IF plAscending <> ? THEN lAscending = plAscending.

    /* Wich column should have what arrow? */
    RUN setSortArrow(brFields:handle, cNewSort, lAscending).

    /* If - and only if - the sort is on 'Order', the buttons for moving are enabled */
    btnMoveUp:sensitive     = (cNewSort = "iOrder").
    btnMoveDown:sensitive   = (cNewSort = "iOrder").
    btnMoveTop:sensitive    = (cNewSort = "iOrder").
    btnMoveBottom:sensitive = (cNewSort = "iOrder").

    /* Close open query */
    IF VALID-HANDLE(brFields:query) THEN brFields:query:query-close().

    /* Build the query */
    CREATE QUERY hQuery.
    CREATE BUFFER hBuffer FOR TABLE 'ttField'.
    hQuery:SET-BUFFERS(hBuffer).

    /* Initially hide red line around browse */
    rcFieldFilter:visible = FALSE.

    cQuery = 'for each ttField where true'.
    FOR EACH bFilter
      WHERE bFilter.hBrowse = brFields:handle:

      IF bFilter.hColumn:data-type = "CHARACTER" THEN
        ASSIGN
          cFilterValue = getMatchesValue(bFilter.hFilter)
          cOperator    = "MATCHES".
      ELSE
        ASSIGN
          cFilterValue = SUBSTITUTE("&1", bFilter.hFilter:screen-value)
          cOperator    = "=".

      /* Only add to the query if it has a real value */
      IF    cFilterValue <> ""
        AND cFilterValue <> "*"
        AND cFilterValue <> ?
        AND cFilterValue <> bFilter.hFilter:private-data THEN
      DO:
        cQuery = SUBSTITUTE("&1 and substitute('&6',ttField.&2) &3 &4 /* &5 */"
                           , cQuery
                           , bFilter.cFieldName
                           , cOperator
                           , QUOTER(cFilterValue)
                           , IF VALID-HANDLE(bFilter.hColumn) THEN bFilter.hColumn:data-type ELSE "?"
                           , "&1"
                           ).
        /* Show red line */
        rcFieldFilter:visible = TRUE.
      END.
    END.

    cQuery = SUBSTITUTE("&1 by &2 &3", cQuery, cNewSort, STRING(lAscending,'/descending')).

    hQuery:QUERY-PREPARE(cQuery).
    hQuery:QUERY-OPEN().

    /* Find out if all visible fields have the same 'visibility flags'. If they are all
     * the same, set the "toggle all" toggle-box to this same value.
     */
    lAllVisible = TRUE.

    hQuery:GET-FIRST.
    REPEAT WHILE NOT hQuery:QUERY-OFF-END:
      lFieldsFound = TRUE.
      IF NOT hBuffer::lShow THEN lAllVisible = FALSE.
      hQuery:GET-NEXT.
    END.

    .tgSelAll:CHECKED = lAllVisible.

    /* Attach query to the browse */
    hQuery:GET-FIRST.
    brFields:QUERY = hQuery.

    /* Jump back to selected row */
    IF NOT hQuery:QUERY-OFF-END
      AND CAN-FIND(ttField WHERE ROWID(ttField) = rCurrentRecord) THEN
    DO:
      hQuery:REPOSITION-TO-ROWID(rCurrentRecord) NO-ERROR.
      brFields:SELECT-FOCUSED-ROW().
    END.

    /* If we have fields, set VIEW button on */
    tgSelAll:SENSITIVE      = lFieldsFound.
    btnMoveUp:SENSITIVE     = lFieldsFound.
    btnMoveDown:SENSITIVE   = lFieldsFound.
    btnReset:SENSITIVE      = lFieldsFound.
    btnMoveTop:SENSITIVE    = lFieldsFound.
    btnMoveBottom:SENSITIVE = lFieldsFound.
  END.

END PROCEDURE. /* reopenFieldBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenIndexBrowse C-Win 
PROCEDURE reopenIndexBrowse :
/* Reopen the browse with indexes.
 */
  DEFINE INPUT PARAMETER pcSortField  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER plAscending  AS LOGICAL     NO-UNDO.

  DEFINE VARIABLE hColumn          AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iColumn          AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lAscending       AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE hBuffer          AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cOldSort         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNewSort         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNameFilter      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFlagFilter      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldsFilter    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery           AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE rCurrentRecord   AS ROWID       NO-UNDO.
  DEFINE VARIABLE lFieldsFound     AS LOGICAL     NO-UNDO.

  PUBLISH "setUsage" ("filterIndexes"). /* user behaviour */

  /* Set filters */
  DO WITH FRAME {&frame-name}:
    cNameFilter   = getMatchesValue(fiIndexNameFilter:handle).
    cFlagFilter   = getMatchesValue(fiFlagsFilter    :handle).
    cFieldsFilter = getMatchesValue(fiFieldsFilter   :handle).
  END.

  RUN setRedLines.

  /* Protect routine against invalid input */
  IF pcSortField = '' THEN pcSortField = ?.

  /* Find out what the current sort is */
  RUN getColumnSort(INPUT brIndexes:handle, OUTPUT cOldSort, OUTPUT lAscending).

  /* If no new sortfield is provided, we don't want to change the sort.
   * This happens when we press the filter button.
   */
  IF pcSortField = ? THEN
    ASSIGN
      cNewSort   = cOldSort
      lAscending = lAscending. /* dont change order */
  ELSE
  IF pcSortField = cOldSort THEN
    ASSIGN
      cNewSort   = cOldSort
      lAscending = NOT lAscending. /* invert order */
  ELSE
    /* New field */
    ASSIGN
      cNewSort   = pcSortField
      lAscending = TRUE.

  /* Protection against first-time usage (in that case
   * sort is not set).
   */
  IF cNewSort = "" THEN cNewSort = brIndexes:get-browse-column(1):name.

  /* Sort direction might be overruled */
  IF plAscending <> ? THEN lAscending = plAscending.

  /* Wich column should have what arrow? */
  RUN setSortArrow(brIndexes:handle, cNewSort, lAscending).

  /* Remember record */
  IF brIndexes:num-selected-rows > 0 THEN
    rCurrentRecord = brIndexes:query:get-buffer-handle(1):rowid.

  /* Build the query */
  CREATE QUERY hQuery.
  CREATE BUFFER hBuffer FOR TABLE 'ttIndex'.
  hQuery:SET-BUFFERS(hBuffer).

  cQuery = 'for each ttIndex where true'.
  IF cNameFilter   <> "" AND cNameFilter   <> "*" THEN cQuery = SUBSTITUTE("&1 and ttIndex.cIndexName   matches &2", cQuery, QUOTER(cNameFilter  )).
  IF cFlagFilter   <> "" AND cFlagFilter   <> "*" THEN cQuery = SUBSTITUTE("&1 and ttIndex.cIndexFlags  matches &2", cQuery, QUOTER(cFlagFilter  )).
  IF cFieldsFilter <> "" AND cFieldsFilter <> "*" THEN cQuery = SUBSTITUTE("&1 and ttIndex.cIndexFields matches &2", cQuery, QUOTER(cFieldsFilter)).
  cQuery = SUBSTITUTE("&1 by &2 &3", cQuery, cNewSort, STRING(lAscending,'/descending')).

  hQuery:QUERY-PREPARE(cQuery).
  hQuery:QUERY-OPEN().
  hQuery:GET-FIRST().

  lFieldsFound = NOT hQuery:QUERY-OFF-END.

  /* Attach query to the browse */
  brIndexes:query IN FRAME {&frame-name} = hQuery.

  /* Jump back to selected row */
  IF NOT hQuery:QUERY-OFF-END
    AND rCurrentRecord <> ? THEN
  DO:
    hQuery:REPOSITION-TO-ROWID(rCurrentRecord) NO-ERROR.
    brIndexes:select-focused-row().
  END.

END PROCEDURE. /* reopenIndexBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenTableBrowse C-Win 
PROCEDURE reopenTableBrowse :
/* Open the table browse again, taking into account the filter values the user has entered.
 */
  DEFINE INPUT PARAMETER pcSortField  AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE hColumn           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iColumn           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lAscending        AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE cOldSort          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNewSort          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTableFilter      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery            AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE rCurrentRecord    AS ROWID       NO-UNDO.
  DEFINE VARIABLE cTableSet         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabaseFilter   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSetting          AS CHARACTER   NO-UNDO.

  DEFINE BUFFER bTable FOR ttTable.

  DO WITH FRAME {&FRAME-NAME}:

    PUBLISH "setUsage" ("filterTables"). /* user behaviour */
    setWindowFreeze(YES).

    /* Get filters. If you type more than one table, you get
     * exactly what you type. Otherwise, DD treats it cleverly
     */
    IF NUM-ENTRIES(fiTableFilter:SCREEN-VALUE) > 1 THEN
      cTableFilter = fiTableFilter:SCREEN-VALUE.
    ELSE
      cTableFilter = getMatchesValue(fiTableFilter:HANDLE).

    cDatabaseFilter = cbDatabaseFilter:SCREEN-VALUE.

    RUN setRedLines.

    /* Protect routine against invalid input */
    IF pcSortField = '' THEN pcSortField = ?.
    IF cDatabaseFilter = ? THEN cDatabaseFilter = '*'.

    /* Remember currently selected record */
    FIND bTable
      WHERE bTable.cDatabase   = gcCurrentDatabase
        AND bTable.cTableName  = gcCurrentTable NO-ERROR.
    IF AVAILABLE bTable THEN rCurrentRecord = ROWID(bTable).

    /* If we have entered the name of a table that exactly matches the name of a table
     * in the database (say "order" in sports) then focus on that table, even if another
     * table (say "order-line" might have been selected.
     */
    FIND bTable
      WHERE bTable.cTableName  = fiTableFilter:SCREEN-VALUE
        AND bTable.lShowInList = TRUE
        AND (NOT glShowFavourites OR bTable.lFavourite = TRUE)
            NO-ERROR.
    IF AVAILABLE bTable THEN rCurrentRecord = ROWID(bTable).

    /* Find out what the current sort is */
    cSetting = getRegistry('DataDigger','ColumnSortTables').
    IF cSetting <> ? THEN
      ASSIGN
        cOldSort   = brTables:GET-BROWSE-COLUMN(INTEGER(ENTRY(1,cSetting))):NAME
        lAscending = LOGICAL(ENTRY(2,cSetting)) no-error.

    /* If no new sortfield is provided, we don't want to change the sort.
     * This happens when we press the filter button.
     */
    IF pcSortField = ? THEN
      cNewSort = cOldSort. /* sorting stays the same */
    ELSE
    IF pcSortField = cOldSort THEN
    DO:
      cNewSort = cOldSort.
      CASE lAscending:
        WHEN TRUE  THEN ASSIGN lAscending = FALSE.           /* asc  -> desc */
        WHEN FALSE THEN ASSIGN lAscending = ? cNewSort = "". /* desc -> none */
        WHEN ?     THEN ASSIGN lAscending = TRUE.            /* none -> asc  */
      END CASE.
    END.
    ELSE
      ASSIGN
        cNewSort   = pcSortField
        lAscending = TRUE.

    /* Protection against wrong parameters (in case of first-time usage sort is not set). */
    IF cNewSort = ? THEN cNewSort = "".
    IF lAscending = ? THEN lAscending = TRUE.

    /* Wich column should have what arrow? */
    RUN setSortArrow(brTables:handle, cNewSort, lAscending).

    /* Close query, which may be open */
    IF VALID-HANDLE(brTables:query) THEN brTables:query:query-close().

    /* Build the query */
    IF NOT VALID-HANDLE(ghTableQuery) THEN
    DO:
      CREATE QUERY ghTableQuery.
      CREATE BUFFER ghTableBuffer FOR TABLE 'ttTable'.
      ghTableQuery:SET-BUFFERS(ghTableBuffer).
    END.

    /* If Favourites-view then limited restrictions. Just show the list*/
    IF glShowFavourites THEN
      cQuery = 'FOR EACH ttTable WHERE ttTable.lFavourite = TRUE'.
    ELSE
    DO:
      /* Base query */
      cQuery = 'FOR EACH ttTable WHERE ttTable.lShowInList = TRUE'.

      /* Show only tables of selected database (if set) */
      IF cDatabaseFilter <> '*' THEN
        cQuery = SUBSTITUTE('&1 AND cDatabase MATCHES &2', cQuery, QUOTER(cDatabaseFilter)).
    END.

    /* Show only the tables that match the table name filter (if set) */
    IF cTableFilter <> "*" THEN
      cQuery = SUBSTITUTE("&1 AND CAN-DO(&2,cTableName)", cQuery, QUOTER(cTableFilter)).

    /* Then proceed with the user's sort */
    IF cNewSort <> "" THEN
      cQuery = SUBSTITUTE("&1 BY &2 &3", cQuery, cNewSort, STRING(lAscending,'/descending') ).

    /* Additional sort: tables that have been accessed at least once rank higher
     * then followed by date/time of last access
     */
    cQuery = SUBSTITUTE("&1 BY tLastUsed <> ? DESCENDING ", cQuery).
    cQuery = SUBSTITUTE("&1 BY tLastUsed DESCENDING", cQuery).

    ghTableQuery:QUERY-PREPARE(cQuery).
    ghTableQuery:QUERY-OPEN().

    /* Attach query to the browse */
    brTables:QUERY IN FRAME {&FRAME-NAME} = ghTableQuery.

    /* Jump back to selected row */
    IF NOT ghTableQuery:QUERY-OFF-END
      AND rCurrentRecord <> ? THEN
    DO:
      ghTableQuery:REPOSITION-TO-ROWID(rCurrentRecord) NO-ERROR.
      brTables:SELECT-FOCUSED-ROW().
      APPLY 'value-changed' TO brTables.
    END.
  END. /* do with frame */

  setWindowFreeze(NO).

END PROCEDURE. /* reopenTableBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE resetFields C-Win 
PROCEDURE resetFields :
/* Reset the field order of all fields and reset databrowse
 */
  DEFINE BUFFER bField  FOR ttField.
  DEFINE BUFFER bColumn FOR ttColumn.

  DEFINE VARIABLE iNewColNr AS INTEGER NO-UNDO.

  setWindowFreeze(TRUE).

  colLoop:
  FOR EACH bField, EACH bColumn WHERE bColumn.cFieldName = bField.cFieldName
    BY bField.iOrderOrg BY bColumn.iExtent:

    /* Reset field and column nr */
    bField.iOrder     = bField.iOrderOrg.
    iNewColNr         = iNewColNr + 1.
    bColumn.iColumnNr = iNewColNr.
  END.

  /* Arrange columns according to settings in tt */
  RUN setDataBrowseColumns.

  /* Remove field order from settings */
  setRegistry( SUBSTITUTE('DB:&1',gcCurrentDatabase)
             , SUBSTITUTE('&1:fieldOrder', gcCurrentTable )
             , ?
             ).

  /* Update field cache */
  RUN updateMemoryCache
    ( INPUT gcCurrentDatabase
    , INPUT gcCurrentTable
    , INPUT TABLE bField
    , INPUT TABLE bColumn
    ).

  RUN reopenFieldBrowse(?,?).

  setWindowFreeze(FALSE).

END PROCEDURE. /* resetFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE resizeFilters C-Win 
PROCEDURE resizeFilters :
/* Redraw the filters. This is needed when the window resizes, one of
 * the columns resizes or the user scrolls in the browse.
 */
  {&timerStart}
  DEFINE INPUT PARAMETER piPageNr AS INTEGER NO-UNDO.

  DEFINE VARIABLE cFilterFields AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cButtons      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE hBrowse       AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iField        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hField        AS HANDLE    NO-UNDO.

  IF piPageNr = ? THEN piPageNr = giCurrentPage.

  DO WITH FRAME {&frame-name}:

    /* Make one string of all handles */
    CASE piPageNr:

      WHEN {&PAGE-TABLES} OR
      WHEN {&PAGE-FAVOURITES} THEN DO: /* Tables */
        ASSIGN
          cFilterFields = SUBSTITUTE('&1,&2'
                                    , fiTableFilter:HANDLE
                                    , cbDatabaseFilter:HANDLE
                                    )
          cButtons      = SUBSTITUTE('&1,&2'
                                    , btnClearTableFilter:HANDLE
                                    , btnTableFilter:HANDLE
                                    )
          hBrowse       = brTables:HANDLE
          .

      END. /* 0 */

      WHEN {&PAGE-FIELDS} THEN DO: /* Fields */
        ASSIGN
          cFilterFields = gcFieldFilterHandles
          cButtons      = SUBSTITUTE('&1', btnClearFieldFilter:HANDLE)
          hBrowse       = brFields:HANDLE
          .
      END. /* 1 */

      WHEN {&PAGE-INDEXES} THEN DO: /* Indexes */
        ASSIGN
          cFilterFields = SUBSTITUTE('&1,&2,&3'
                                    , fiIndexNameFilter:HANDLE
                                    , fiFlagsFilter:HANDLE
                                    , fiFieldsFilter:HANDLE
                                    )
          cButtons      = SUBSTITUTE('&1', btnClearIndexFilter:HANDLE)
          hBrowse       = brIndexes:HANDLE
          .
      END. /* 2 */
    END CASE. /* giCurrentPage */

    setWindowFreeze(YES).

    /* Save current widths to registry */
    fieldLoop:
    DO iField = 1 TO NUM-ENTRIES(cFilterFields):
      hField = hBrowse:GET-BROWSE-COLUMN(iField).

      setRegistry('DataDigger'
                 , SUBSTITUTE('ColumnWidth:&1', hField:NAME)
                 , SUBSTITUTE('&1', hField:WIDTH-PIXELS)
                 ).
    END.

    /* Resize them */
    RUN resizeFilterFields
      ( INPUT ?
      , INPUT cFilterFields
      , INPUT cButtons
      , INPUT hBrowse
      ).
  END.

  RUN showScrollBars(FRAME {&frame-name}:HANDLE, NO, NO).
  setWindowFreeze(NO).

  {&timerStop}
END PROCEDURE. /* resizeFilters */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveFilterValue C-Win 
PROCEDURE saveFilterValue :
/* Save the last x filter values to registry
 */
  DEFINE INPUT  PARAMETER pcDatabase AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcTable    AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcField    AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcNewValue AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cNewList   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldList   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cThisValue AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iPos       AS INTEGER     NO-UNDO.

  IF pcNewValue = "" OR pcNewValue = ? THEN RETURN.

  /* PAT 2016-10-27 */
  PUBLISH "customSaveFilterValue" (pcDatabase, pcTable, pcField, pcNewValue).

  cOldList = getRegistry( SUBSTITUTE("DB:&1",pcDatabase)
                        , SUBSTITUTE("&1.&2:FilterHistory",pcTable,pcField)
                        ).
  IF cOldList = ? THEN cOldList = "".

  /* Start the new list with the current filter value */
  cNewList = pcNewValue.

  /* Add old entries to the list */
  DO iPos = 1 TO NUM-ENTRIES(cOldList,CHR(1)).
    cThisValue = ENTRY(iPos,cOldList,CHR(1)).

    /* Skip empty */
    IF cThisValue = "" THEN NEXT.

    /* If it is already in the list, ignore */
    IF LOOKUP(cThisValue,cNewList,CHR(1)) > 0 THEN NEXT.

    /* Add to list */
    cNewList = cNewList + CHR(1) + cThisValue.

    /* Stop if there are too much in the list */
    IF NUM-ENTRIES(cNewList,CHR(1)) >= giMaxFilterHistory THEN LEAVE.
  END.

  setRegistry( SUBSTITUTE("DB:&1",pcDatabase)
             , SUBSTITUTE("&1.&2:FilterHistory",pcTable,pcField)
             , cNewList
             ).

END PROCEDURE. /* saveFilterValue */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveWindow C-Win 
PROCEDURE saveWindow :
/* Save size and position of the window.
 */
  /* Upper left corner of window */
  setRegistry("DataDigger", "Window:x", STRING(c-win:X) ).
  setRegistry("DataDigger", "Window:y", STRING(c-win:Y) ).

  /* Width and height */
  setRegistry("DataDigger", "Window:height", STRING(c-win:HEIGHT-PIXELS) ).
  setRegistry("DataDigger", "Window:width", STRING(c-win:WIDTH-PIXELS) ).

  /* Position of the resize bar */
  DO WITH FRAME frMain:
    setRegistry("DataDigger", "ResizeBar:Y", STRING(btnResizeVer:Y) ).
  END.

END PROCEDURE. /* saveWindow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE selectClickedRow C-Win 
PROCEDURE selectClickedRow :
/* Select the row the user last clicked on
 */
  DEFINE INPUT  PARAMETER phBrowse        AS HANDLE    NO-UNDO.
  DEFINE OUTPUT PARAMETER plOk            AS LOGICAL   NO-UNDO.
  DEFINE OUTPUT PARAMETER pcColumnName    AS CHARACTER NO-UNDO.

  DEFINE VARIABLE dRow             AS DECIMAL   NO-UNDO.
  DEFINE VARIABLE iMouseX          AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iMouseY          AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iColumn          AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iExtentNr        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hColumn          AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iRow             AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cColumnValue     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE hBuffer          AS HANDLE    NO-UNDO.
  DEFINE VARIABLE hBrowseColumn    AS HANDLE    NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.

  PUBLISH "debugMessage" (1, SUBSTITUTE("Select Clicked Row.")).

  /* Get mouse position (but not if we used SHIFT-F10 for the context menu */
  IF LAST-EVENT:LABEL = 'SHIFT-F10' THEN
  DO:
    iRow = phBrowse:FOCUSED-ROW.
    PUBLISH "debugMessage" (2, SUBSTITUTE("Pressed SHIFT-F10 on row &1", iRow)).
  END.

  ELSE /* used mouse right click */
  DO:
    RUN getMouseXY(INPUT phBrowse:FRAME, OUTPUT iMouseX, OUTPUT iMouseY).

    /* Find out what row number we clicked on */
    dRow = (iMouseY - phBrowse:Y - 18) / (phBrowse:ROW-HEIGHT-PIXELS + 4).
    iRow = (IF dRow = integer(dRow) THEN INTEGER(dRow) ELSE TRUNCATE(dRow,0) + 1). /* ceiling of dRow */

    /* Is it a valid row nr? (could be invalid if we clicked below last record) */
    PUBLISH "debugMessage" (2, SUBSTITUTE(" - Clicked row &1", iRow)).
    IF phBrowse:NUM-ITERATIONS > 0 AND iRow > phBrowse:NUM-ITERATIONS THEN RETURN.
    IF iRow < 1 THEN RETURN.

    /* Get the record in the buffer */
    IF phBrowse:QUERY:NUM-RESULTS > 0 THEN
    DO:
      phBrowse:SELECT-ROW(iRow).
      phBrowse:FETCH-SELECTED-ROW(phBrowse:NUM-SELECTED-ROWS).
    END.

    plOk = TRUE.


/*     /* Should be something like this in a perfect world */        */
/*     hBrowseColumn = getClickedColumn(phBrowse).                   */
/*     IF VALID-HANDLE(hBrowseColumn)                                */
/*       AND phBrowse:QUERY:NUM-RESULTS > 0 THEN                     */
/*     DO:                                                           */
/*       hBuffer = phBrowse:QUERY:GET-BUFFER-HANDLE(1).              */
/*       CASE pcColumnName:                                          */
/*         WHEN 'RECID' THEN cColumnValue = STRING( hBuffer:RECID ). */
/*         WHEN 'ROWID' THEN cColumnValue = STRING( hBuffer:ROWID ). */
/*         OTHERWISE cColumnValue = hBrowseColumn:SCREEN-VALUE.      */
/*       END.                                                        */
/*     END.                                                          */

    PUBLISH "debugMessage" (2, SUBSTITUTE(" - Browse:&1  Cols:&2", phBrowse:NAME, phBrowse:NUM-COLUMNS)).

    /* Find out which column we clicked on */
    findColumn:
    DO iColumn = 1 TO phBrowse:NUM-COLUMNS:
      hBrowseColumn = phBrowse:GET-BROWSE-COLUMN(iColumn).

      PUBLISH "debugMessage" (2, SUBSTITUTE(" - Column:&1  XY:&2/&3 W:&4", hBrowseColumn:NAME, hBrowseColumn:X, hBrowseColumn:Y,hBrowseColumn:WIDTH-PIXELS)).

      IF hBrowseColumn:X > -1
        AND (iMouseX - phBrowse:X) > hBrowseColumn:X
        AND (iMouseX - phBrowse:X) < (hBrowseColumn:X + hBrowseColumn:WIDTH-PIXELS) THEN
      DO:
        pcColumnName = hBrowseColumn:NAME.

        /* This is the record the user clicked on in the buffer
         * Only proceed when the browse holds some data */
        IF phBrowse:QUERY:NUM-RESULTS > 0 THEN
        DO:
          hBuffer = phBrowse:QUERY:GET-BUFFER-HANDLE(1).
          CASE pcColumnName:
            WHEN 'RECID' THEN cColumnValue = STRING( hBuffer:RECID ).
            WHEN 'ROWID' THEN cColumnValue = STRING( hBuffer:ROWID ).
            OTHERWISE cColumnValue = hBrowseColumn:SCREEN-VALUE.
          END.
        END.

        LEAVE findColumn.
      END.
    END.

    /* Save the column value to be able to add it to filters */
    phBrowse:PRIVATE-DATA = pcColumnName + CHR(1) + cColumnValue + CHR(1) + STRING(iExtentNr).

    PUBLISH "debugMessage" (2, SUBSTITUTE("Column &1 has value &2", pcColumnName, cColumnValue)).
  END. /* used the mouse */

END PROCEDURE. /* selectClickedRow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setConnectionMenu C-Win 
PROCEDURE setConnectionMenu :
/* Rebuild the connection submenu of the 'add' button
 */
  DEFINE VARIABLE hMenuItem       AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cProgDir        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cConnectionList AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabase       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iConn           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE hItemToDelete   AS HANDLE      NO-UNDO.

  hMenuItem = brTables:popup-menu:first-child IN FRAME {&frame-name}.
  cProgDir = getProgramDir().

  /* Remove all current items except first 3 */
  DO WHILE VALID-HANDLE(hMenuItem):
    IF hMenuItem:DYNAMIC THEN hItemToDelete = hMenuItem.
    hMenuItem = hMenuItem:NEXT-SIBLING.
    IF VALID-HANDLE(hItemToDelete) THEN
      DELETE OBJECT hItemToDelete.
  END.

  /* Get list of connections */
  RUN value(cProgDir + 'wConnections.w')
    ( INPUT 'getConnections'
    , INPUT ''
    , OUTPUT cConnectionList
    ).

  /* And add them to the menu */
  DO iConn = 1 TO NUM-ENTRIES(cConnectionList):
    cDatabase = ENTRY(iConn,cConnectionList).

    /* Skip if already connected */
    IF NOT CONNECTED(cDatabase) THEN
      CREATE MENU-ITEM hMenuItem
        ASSIGN
          LABEL  = cDatabase
          NAME   = cDatabase
          PARENT = brTables:popup-menu
        TRIGGERS:
          ON 'CHOOSE':U PERSISTENT RUN connectDatabase IN this-procedure (cDatabase).
        END TRIGGERS.
  END. /* do iConn */

END PROCEDURE. /* setConnectionMenu */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setDataBrowseColumns C-Win 
PROCEDURE setDataBrowseColumns :
/* Set all columns according to their iColumnNr
 */
  DEFINE VARIABLE iOldPos     AS INTEGER NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.
  DEFINE BUFFER bField  FOR ttField.

  IF NOT VALID-HANDLE(ghDataBrowse) THEN RETURN.

  setWindowFreeze(TRUE).

  colLoop:
  FOR EACH bColumn BY bColumn.iColumnNr:

    /* Find the current position of this column */
    DO iOldPos = bColumn.iColumnNr + 1 TO ghDataBrowse:NUM-COLUMNS:

      IF bColumn.hColumn = ghDataBrowse:GET-BROWSE-COLUMN(iOldPos) THEN
      DO:
        /* Since position might be part of label, set it again */
        bColumn.hColumn:LABEL = getColumnLabel(BUFFER bColumn:HANDLE).

        /* Move the column to its new position */
        ghDataBrowse:MOVE-COLUMN(iOldPos,bColumn.iColumnNr).

        /* Done, go to next column */
        NEXT colLoop.
      END. /* column found */
    END. /* f/e bColumn */
  END. /* valid-handle ghDataBrowse */

  /* Redraw filters etc */
  RUN dataScrollNotify(ghDataBrowse).

  /* Reset the TAB order of the filter fields */
  RUN setFilterFieldTabOrder.

  setWindowFreeze(FALSE).

END PROCEDURE. /* setDataBrowseColumns */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setDataFilter C-Win 
PROCEDURE setDataFilter :
/* Optionally clear the filters and set a filter value
 */
  DEFINE INPUT PARAMETER plClearOtherFilters AS LOGICAL NO-UNDO.

  DEFINE VARIABLE cColumnName  AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cColumnValue AS CHARACTER NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.

  /* Freeze updates */
  setWindowFreeze(YES).

  IF NUM-ENTRIES(ghDataBrowse:PRIVATE-DATA,CHR(1)) = 3 THEN
  DO:
    cColumnName  = ENTRY(1, ghDataBrowse:PRIVATE-DATA,CHR(1)).
    cColumnValue = ENTRY(2, ghDataBrowse:PRIVATE-DATA,CHR(1)).

    FOR EACH bColumn:

      /* If this is the field we're looking for, set the
       * value. Otherwise see if we need to blank other fields.
       */
      IF bColumn.cFullName = cColumnName THEN
        bColumn.hFilter:SCREEN-VALUE = cColumnValue.
      ELSE
      IF plClearOtherFilters THEN
        bColumn.hFilter:SCREEN-VALUE = bColumn.hFilter:PRIVATE-DATA.

      setFilterFieldColor(bColumn.hFilter).
      RUN filterFieldValueChanged(bColumn.hFilter,NO).
    END.

    RUN reopenDataBrowse.
  END.

  setWindowFreeze(NO).

END PROCEDURE. /* setDataFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setFilterFieldTabOrder C-Win 
PROCEDURE setFilterFieldTabOrder :
/* Reset the TAB order of the filter fields
   */
  DEFINE VARIABLE hPrevFilter AS HANDLE  NO-UNDO.
  DEFINE BUFFER bColumn FOR ttColumn.

  /* Set the TAB order of the filter to after the previous filter field */
  FOR EACH bColumn BY bColumn.iColumnNr:
    IF NOT bColumn.hColumn:VISIBLE THEN NEXT.

    PUBLISH 'debugMessage' (1, SUBSTITUTE('&1 &2 &3 (&4)'
      , bColumn.iColumnNr
      , bColumn.cFullName
      , bColumn.hColumn:VISIBLE
      , (IF VALID-HANDLE(hPrevFilter) THEN hPrevFilter:NAME ELSE '')
      )).

    IF VALID-HANDLE(hPrevFilter) THEN bColumn.hFilter:MOVE-AFTER-TAB-ITEM(hPrevFilter).
    hPrevFilter = bColumn.hFilter.
  END.

END PROCEDURE. /* setFilterFieldTabOrder */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setPage C-Win 
PROCEDURE setPage :
/* Set active page: Tables/Pinned/Fields/Indexes
 */
  {&timerStart}
  DEFINE INPUT PARAMETER piPage AS INTEGER NO-UNDO.

  /* If we are already on this page, then we're ready */
  IF giCurrentPage = piPage THEN RETURN.

  /* remember page */
  giCurrentPage = piPage.
  setWindowFreeze(YES).

  DO WITH FRAME {&FRAME-NAME}:
    CASE piPage:

      /* FIELDS */
      WHEN {&PAGE-FIELDS} THEN
      DO:
        btnTabFields :LOAD-IMAGE( getImagePath('tab_fields_active.gif'    )).
        btnTabIndexes:LOAD-IMAGE( getImagePath('tab_indexes_inactive.gif' )).

        RUN showDataFilters(INPUT brFields:HANDLE, TRUE).
        tgSelAll:VISIBLE = TRUE.
        VIEW {&list-2}.
        HIDE {&list-3}.

        RUN setRedLines.

        IF NOT VALID-HANDLE(ghDataBrowse) THEN
        DO WITH FRAME frData:
          HIDE btnClearDataFilter btnDataSort.
        END.
      END.

      /* INDEXES */
      WHEN {&PAGE-INDEXES} THEN
      DO:
        btnTabFields :LOAD-IMAGE( getImagePath('tab_fields_inactive.gif' )).
        btnTabIndexes:LOAD-IMAGE( getImagePath('tab_indexes_active.gif'  )).

        RUN showDataFilters(INPUT brFields:HANDLE, FALSE).
        tgSelAll:VISIBLE = FALSE.
        HIDE {&list-2}.
        VIEW {&list-3}.

        RUN setRedLines.
      END.

      WHEN {&PAGE-TABLES} THEN
      DO:
        btnTabTables   :LOAD-IMAGE( getImagePath('tab_tables_active.gif'    )).
        btnTabFavourites:LOAD-IMAGE( getImagePath('tab_Favourites_inactive.gif' )).
        btnTableFilter:SENSITIVE = TRUE.
        RUN setTableView(NO,NO).
      END.

      WHEN {&PAGE-FAVOURITES} THEN
      DO:
        btnTabTables   :LOAD-IMAGE( getImagePath('tab_tables_inactive.gif'    )).
        btnTabFavourites:LOAD-IMAGE( getImagePath('tab_Favourites_active.gif' )).
        btnTableFilter:SENSITIVE = FALSE.
        RUN setTableView(YES,NO).
      END.
    END CASE. /* piPage */

    RUN resizeFilters(INPUT piPage).
  END.

  RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO).
  setWindowFreeze(NO).

  {&timerStop}

END PROCEDURE. /* setPage */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setRedLines C-Win 
PROCEDURE setRedLines :
/* Show red lines around browse when filtered
 */
  {&timerStart}
  DEFINE BUFFER bFilter FOR ttFilter.
  DEFINE VARIABLE cFilterValue AS CHARACTER NO-UNDO.

  DO WITH FRAME {&FRAME-NAME}:

    /* Table Filter above browse */
    IF getMatchesValue(fiTableFilter:HANDLE) <> "*" THEN
      rcTableFilter:VISIBLE = TRUE.
    ELSE
      rcTableFilter:VISIBLE = FALSE.

    /* Table filter options */
    IF isTableFilterUsed(INPUT TABLE ttTableFilter) THEN
      btnTableFilter:LOAD-IMAGE(getImagePath("FilterRed.gif")).
    ELSE
      btnTableFilter:LOAD-IMAGE(getImagePath("Filter.gif")).


    CASE giCurrentPage:

      /* Fields */
      WHEN {&PAGE-FIELDS} THEN
      DO:
        rcFieldFilter:VISIBLE = FALSE.

        checkFieldFilters:
        FOR EACH bFilter WHERE bFilter.hBrowse = brFields:HANDLE:

          IF bFilter.hColumn:DATA-TYPE = "CHARACTER" THEN
            ASSIGN
              cFilterValue = getMatchesValue(bFilter.hFilter).
          ELSE
            ASSIGN
              cFilterValue = SUBSTITUTE("&1", bFilter.hFilter:screen-value).

          /* Show red line when filter value was entered */
          IF    cFilterValue <> ""
            AND cFilterValue <> "*"
            AND cFilterValue <> ?
            AND cFilterValue <> bFilter.hFilter:PRIVATE-DATA THEN
          DO:
            /* Show red line */
            rcFieldFilter:VISIBLE = TRUE.
            LEAVE checkFieldFilters.
          END.
        END.
      END.

      WHEN {&PAGE-INDEXES} THEN
      DO:
        /* Indexes */
        IF   getMatchesValue(fiIndexNameFilter:HANDLE) <> "*"
          OR getMatchesValue(fiFlagsFilter    :HANDLE) <> "*"
          OR getMatchesValue(fiFieldsFilter   :HANDLE) <> "*" THEN
          rcIndexFilter:VISIBLE = TRUE.
        ELSE
          rcIndexFilter:VISIBLE = FALSE.
      END.
    END.
  END.

  {&timerStop}
END PROCEDURE. /* setRedLines */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setSortArrows C-Win 
PROCEDURE setSortArrows :
/* Set the sorting arrows on a browse
 */
  DEFINE INPUT PARAMETER phBrowse    AS HANDLE    NO-UNDO.

  DEFINE BUFFER bfQuerySort FOR ttQuerySort.
  DEFINE BUFFER bfColumn    FOR ttColumn.

  DEFINE VARIABLE iSortOrder  AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lMultiSort  AS LOGICAL NO-UNDO.

  {&timerStart}

  /* Clear existing sorts */
  phBrowse:CLEAR-SORT-ARROWS().

  /* If there is only one sort, don't use numbered arrows */
  FIND bfQuerySort NO-ERROR.
  IF AMBIGUOUS bfQuerySort THEN lMultiSort = TRUE.

  /* Process all sorts, first those that come from the query,
   * then the ones that came from clicking on the columns
   */
  SortItem:
  FOR EACH bfQuerySort
    WHERE bfQuerySort.iGroup = 0
       BY bfQuerySort.iSortNr:

    /* Find column name */
    FIND FIRST bfColumn WHERE bfColumn.cFullName = bfQuerySort.cSortField NO-ERROR.
    IF NOT AVAILABLE bfColumn THEN
      FIND FIRST bfColumn WHERE bfQuerySort.cSortField MATCHES '*' + bfColumn.cFieldName + '*' NO-ERROR.
    IF NOT AVAILABLE bfColumn THEN NEXT SortItem.

    /* Increase counter for sort arrow. Max value in Progress is 9 */
    iSortOrder = iSortOrder + 1.
    IF iSortOrder > 9 THEN NEXT SortItem.

    IF lMultiSort THEN
      phBrowse:SET-SORT-ARROW(bfColumn.iColumnNr, bfQuerySort.lAscending, iSortOrder).
    ELSE
      phBrowse:SET-SORT-ARROW(bfColumn.iColumnNr, bfQuerySort.lAscending).

  END. /* SortItem */

  {&timerStop}

END PROCEDURE. /* setSortArrow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTable C-Win 
PROCEDURE setTable :
/* If some text is selected in the session or a text is on the clipboard, select the table with that name.
 */
  DEFINE INPUT PARAMETER pcSelectedText AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cTable AS CHARACTER NO-UNDO.

  PUBLISH "timerCommand" ("start", "SetTable").

  IF pcSelectedText = ? THEN
  DO:
    /* Look in all windows if there is any text selected.
     * If not, look on the clipboard.
     */
    cTable = getSelectedText(SESSION:FIRST-CHILD).
    IF cTable = "" THEN cTable = TRIM(CLIPBOARD:VALUE) NO-ERROR.

    /* protect against garbage */
    IF cTable = ? THEN cTable = "".
    cTable = ENTRY(1,cTable," ").
    IF LENGTH(cTable) > 20 THEN cTable = "".

  END.
  ELSE
    cTable = pcSelectedText.

  /* If it contains multiple words, forget it, it's not gonna be a table */
  IF NUM-ENTRIES(cTable,' ') > 1 THEN cTable = ''.
  IF LENGTH(cTable) < 3 THEN cTable = ''.

  /* Now see if we can do anything with the text */
  IF cTable <> "" THEN
  DO:
    IF CAN-FIND(FIRST ttTable WHERE ttTable.cTableName MATCHES '*' + cTable + '*') THEN
    DO WITH FRAME frMain:

      SESSION:SET-WAIT-STATE("general").
      setWindowFreeze(TRUE).

      /* If we have a full match on table name, for example when text "ORDER"
       * is selected, make sure table is set to "ORDER" and not "ORDERLINE"
       */
      FIND FIRST ttTable WHERE ttTable.cTableName MATCHES '*' + cTable + '*' AND ttTable.lShowInList NO-ERROR.
      IF AVAILABLE ttTable THEN 
      DO: 
        /* Set db and file name */
        cbDatabaseFilter:SCREEN-VALUE = ''.
        fiTableFilter:SCREEN-VALUE = cTable.
        RUN reopenTableBrowse(?).

        brTables:QUERY:REPOSITION-TO-ROWID( ROWID(ttTable)).
        brTables:REFRESH().
      END.
                 
      APPLY 'value-changed' TO brTables.
                       
      IF gcCurrentTable <> "" THEN
      DO:
        RUN setTableContext(INPUT gcCurrentTable ).
        RUN reopenDataBrowse.
      END.
      ELSE
      DO:
        fiTableFilter:SCREEN-VALUE = "".
        APPLY 'value-changed' TO fiTableFilter.
        RUN reopenTableBrowse(?).
      END.

      APPLY 'entry' TO brTables.
                  
      setWindowFreeze(NO).
      SESSION:SET-WAIT-STATE("").
    END.
  END. /* has value */

  PUBLISH "timerCommand" ("stop", "SetTable").

END PROCEDURE. /* setTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTableContext C-Win 
PROCEDURE setTableContext :
/* Perform actions when a change of table has occurred.
 */
  DEFINE INPUT PARAMETER pcTable AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cFieldList AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cQuery     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE hEditor    AS HANDLE    NO-UNDO.

  DO WITH FRAME {&frame-name}:

    IF pcTable = "" THEN RETURN.
    setWindowFreeze(YES).

    /* If table has changed adjust the screen */
    IF pcTable <> gcCurrentTable THEN
      setCurrentTable( pcTable ).

    /* Delete filters */
    RUN deleteDataFilters(ghDataBrowse).

    /* Disable edit panel */
    setUpdatePanel('no-record').

    /* Refill the tt with fields of this table */
    RUN collectFieldInfo(INPUT pcTable).

    /* Refill the index tt */
    RUN collectIndexInfo(INPUT pcTable).

    /* Get all saved queries of this table */
    RUN collectQueryInfo( INPUT gcCurrentDatabase, INPUT pcTable ).
    ASSIGN giQueryPointer = 1.

    /* If the query editor is expanded, do actions to that field */
    hEditor = getActiveQueryEditor().

    /* Give custom code a chance to alter the query */
    cQuery = hEditor:SCREEN-VALUE.
    PUBLISH "customQuery" (INPUT gcCurrentDatabase, INPUT gcCurrentTable, INPUT-OUTPUT cQuery).
    hEditor:SCREEN-VALUE = cQuery.

    /* Reopen the queries on Fields and Indexes */
    RUN reopenFieldBrowse(?,?).
    RUN reopenIndexBrowse(?,?).

    /* Set toggle to de/select all fields */
    tgSelAll:checked = TRUE.

    /* Unless no field is selected */
    IF getSelectedFields() = '' THEN tgSelAll:checked = FALSE.

    /* Get a list of all fields (extents NOT expanded) */
    FOR EACH ttField BY ttField.cFieldName BY ttField.iExtent:
      cFieldList = cFieldList + ',' + ttField.cFullname.
    END.

    DO WITH FRAME frWhere:
      /* Set list of fields in field combo */
      cbFields:list-items     = cFieldList.
      cbAndOr:screen-value    = ENTRY(1,cbAndOr:list-items).
      cbFields:screen-value   = ENTRY(1,cbFields:list-items).
      cbOperator:screen-value = ENTRY(1,cbOperator:list-items).
    END.

    /* Reset query-pointer */
    ASSIGN giQueryPointer = 0.

    fiWarning:visible = NO.
    ficWhere:bgcolor = ?. /* default */
    ficWhere:fgcolor = ?. /* default */
    ficWhere:tooltip = ''.

    /* Save last used table and position in browse in registry */
    setRegistry ("DB:" + gcCurrentDatabase, "table", pcTable ).

    RUN setWindowTitle.

    /* Create a browse for this table */
    RUN reopenDataBrowse-create(INPUT gcCurrentDatabase, INPUT pcTable).

    setWindowFreeze(NO).
  END.
END PROCEDURE. /* setTableContext */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTableFilterOptions C-Win 
PROCEDURE setTableFilterOptions :
/* Filter tables based on whether they hold certain fields
 */
  DEFINE VARIABLE cOldTable        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lFirstUsage      AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lAnythingChanged AS LOGICAL     NO-UNDO.

  /* Check if this is the first time we are using it */
  lFirstUsage = getRegistry("DataDigger:Usage", "setTableFilter:numUsed") = ?.
  PUBLISH "setUsage" ("setTableFilter"). /* user behaviour */

  DO WITH FRAME frMain:
    RUN VALUE(getProgramDir() + 'dFilter.w') (INPUT-OUTPUT TABLE ttTableFilter).
    FIND ttTableFilter.
    gcFieldFilterList = ttTableFilter.cTableFieldShow.
  END.

  RUN setRedLines.

  IF lFirstUsage THEN
    RUN showHint(btnTableFilter:HANDLE, 1, "~nThis arrow is red to indicate you are using a filter").

  SESSION:SET-WAIT-STATE("general").
  setWindowFreeze(YES).

  cOldTable = gcCurrentTable.

  RUN getTablesFiltered(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
  RUN reopenTableBrowse(?).

  IF cOldTable <> gcCurrentTable THEN
    APPLY 'value-changed' TO brTables IN FRAME frMain.

  RUN reopenFieldBrowse(?,?).
  RUN setWindowTitle.

  setWindowFreeze(NO).
  SESSION:SET-WAIT-STATE("").

  IF lFirstUsage THEN
    RUN showHint(brFields:HANDLE, 2, "~nAnd matching fields in the tables are now highlighted").

END PROCEDURE. /* setTableFilterOptions */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTableView C-Win 
PROCEDURE setTableView :
/* Set tables view to either 'tables' or 'favourites'
 */
  DEFINE INPUT PARAMETER plFavouritesView AS LOGICAL NO-UNDO.
  DEFINE INPUT PARAMETER plFiredBySystem  AS LOGICAL NO-UNDO.

  DEFINE VARIABLE iNumFav   AS INTEGER NO-UNDO.
  DEFINE VARIABLE lFirstRun AS LOGICAL NO-UNDO.

  /* What view are we in? */
  glShowFavourites = plFavouritesView.

  /* If we switch manually to Fav-view for the first time... */
  IF NOT plFiredBySystem
    AND glShowFavourites = TRUE
    AND getRegistry("DataDigger:Usage", "switchTableView:numUsed") = ? THEN
  DO:
    lFirstRun = TRUE.

    FOR EACH ttTable
      WHERE ttTable.lHidden     = FALSE
        AND ttTable.iNumQueries > 0
      BY ttTable.iNumQueries DESCENDING
      BY ttTable.tLastUsed DESCENDING:

      ttTable.lFavourite = TRUE.
      setRegistry( SUBSTITUTE("DB:&1",ttTable.cDatabase)
                 , SUBSTITUTE("&1:Favourite",ttTable.cTableName)
                 , "TRUE"
                 ).
      iNumFav = iNumFav + 1.
      IF iNumFav >= 4 THEN LEAVE.
    END.
  END.

  PUBLISH "setUsage" ("switchTableView"). /* user behaviour */

  DO WITH FRAME frMain:
    setRegistry("DataDigger","TableView", STRING(glShowFavourites,"F/T")).
    RUN reopenTableBrowse(?).

    IF lFirstRun AND CAN-FIND(FIRST ttTable WHERE ttTable.lFavourite = TRUE) THEN
      RUN showHint(brTables:HANDLE,4,"To give you a start, I added your most used tables to the favourites. Add or remove them by hitting F on the browse.").
  END.

END PROCEDURE. /* setTableView */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTimer C-Win 
PROCEDURE setTimer :
/*
 * Enable or disable a named timer.
 * Disable timer by setting piInterval to 0
 */
  DEFINE INPUT PARAMETER pcTimerProc AS CHARACTER NO-UNDO. /* name of timer */
  DEFINE INPUT PARAMETER piInterval  AS INTEGER   NO-UNDO. /* time in msec  */

  DEFINE BUFFER bTimer FOR ttTimer.

  /* If the timer is not running, execute
   * the procedure immediately and exit */
  IF NOT glUseTimer THEN
  DO:
    IF piInterval > 0 THEN RUN VALUE(pcTimerProc).
    RETURN.
  END.

  /* Find it */
  FIND bTimer WHERE bTimer.cProc = pcTimerProc NO-ERROR.

  /* Create it if needed */
  IF NOT AVAILABLE bTimer THEN
  DO:
    CREATE bTimer.
    ASSIGN bTimer.cProc = pcTimerProc.
  END.

  /* When it is disabled, delete it */
  IF piInterval = 0 THEN
    DELETE bTimer.
  ELSE
  DO:
    ASSIGN
      bTimer.iTime = piInterval
      bTimer.tNext = ADD-INTERVAL(NOW, piInterval,"milliseconds")
      .
  END.

  /* Schedule the next event to run */
  RUN SetTimerInterval.

END PROCEDURE. /* setTimer */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTimerInterval C-Win 
PROCEDURE setTimerInterval :
/*
 * Set the interval of the timer so that it will tick exactly when the next timed event is due.
 */
  DEFINE BUFFER bTimer FOR ttTimer.

  /* Ignore this when the timer is not running */
  IF NOT glUseTimer THEN RETURN.

  IF NOT VALID-HANDLE(chCtrlFrame) THEN RETURN.

  /* Find the next timer to fire */
  FOR FIRST bTimer BY bTimer.tNext:

    /* How long until it should run? */
    chCtrlFrame:pstimer:INTERVAL = MAXIMUM(1,MTIME(bTimer.tNext) - MTIME(NOW)).

    /* Turn on events */
    chCtrlFrame:pstimer:ENABLED = TRUE.
  END.

END PROCEDURE. /* setTimerInterval */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setViewType C-Win 
PROCEDURE setViewType :
/* Set the type of view to view records (TXT HTML XLS)
 */
  DEFINE INPUT  PARAMETER pcViewType AS CHARACTER   NO-UNDO.

  DO WITH FRAME frMain:
    btnView:label = SUBSTITUTE('View:&1',pcViewType).

    CASE pcViewType:
      WHEN "txt"  THEN btnView:load-image(getImagePath("Text.gif")).
      WHEN "html" THEN btnView:load-image(getImagePath("Html.gif")).
      WHEN "xls"  THEN btnView:load-image(getImagePath("Excel.gif")).
    END CASE.

    setRegistry('DataDigger', 'ViewType', pcViewType).
  END.

END PROCEDURE. /* setViewType */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setWindowTitle C-Win 
PROCEDURE setWindowTitle :
/* Set the title of the DataDigger window
 */
  DEFINE VARIABLE cTitle          AS CHARACTER NO-UNDO.
  DEFINE VARIABLE hParent         AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hOwner          AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cFilter         AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cDatabase       AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cNameShow       AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cNameHide       AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cFieldShow      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cFieldHide      AS CHARACTER NO-UNDO.

  FIND ttTableFilter.
  ASSIGN
    cNameShow  = ttTableFilter.cTableNameShow
    cNameHide  = ttTableFilter.cTableNameHide 
    cFieldShow = ttTableFilter.cTableFieldShow
    cFieldHide = ttTableFilter.cTableFieldHide.

  /* Reset the filters to sane values if needed */
  IF cNameShow  = '*' OR cNameShow  = ? THEN cNameShow  = ''.
  IF cNameHide  = '*' OR cNameHide  = ? THEN cNameHide  = '' .
  IF cFieldShow = '*' OR cFieldShow = ? THEN cFieldShow = ''.
  IF cFieldHide = '*' OR cFieldHide = ? THEN cFieldHide = ''.

  /* Move elements starting with "!" from pos-list to neg-list */
  RUN correctFilterList(INPUT-OUTPUT cNameShow, INPUT-OUTPUT cNameHide).
  RUN correctFilterList(INPUT-OUTPUT cFieldShow, INPUT-OUTPUT cFieldHide).

  IF cNameShow  <> '' THEN cFilter = SUBSTITUTE('Show tables: &1', cNameShow).
  IF cNameHide  <> '' THEN cFilter = TRIM(SUBSTITUTE('&1 |  Hide tables: &2', cFilter, cNameHide),' |').
  IF cFieldShow <> '' THEN cFilter = TRIM(SUBSTITUTE('&1 |  Has fields: &2', cFilter, cFieldShow),' |').
  IF cFieldHide <> '' THEN cFilter = TRIM(SUBSTITUTE('&1 |  Has not fields: &2', cFilter, cFieldHide),' |').

  /* Set db name to use */
  CASE getRegistry('DataDigger','TitleBarDbName'):
    WHEN 'none'    THEN cDatabase = ''.
    WHEN 'ldbname' THEN cDatabase = LDBNAME(gcCurrentDatabase) + '.'.
    WHEN 'pdbname' THEN DO:
      /* Ignore pathnames */
      cDatabase = PDBNAME(gcCurrentDatabase) + '.'.
      cDatabase = ENTRY(NUM-ENTRIES(cDatabase,'/'),cDatabase,'/').
      cDatabase = ENTRY(NUM-ENTRIES(cDatabase,'\'),cDatabase,'\').
    END.
    OTHERWISE cDatabase = gcCurrentDatabase + '.'.
  END. 

  /*
  ** Display the current database and table name in the windowtitle
  **
  ** DataDigger 17 - DEVELOP - sports.customer
  */
  cTitle = SUBSTITUTE( "&1 &2 &3 - &4&5 &6"
                     , "DataDigger"
                     , "{&version}"
                     , (IF SESSION:PARAMETER <> '' THEN '- ' + SESSION:PARAMETER ELSE '')
                     , cDatabase
                     , gcCurrentTable
                     , (IF cFilter <> '' THEN '(' + cFilter + ')'  ELSE '')
                     ).

  /* Add warning for read-only mode */
  IF plReadOnlyDigger THEN cTitle = cTitle + " ** READ-ONLY **".

  /* Add warning for debug-mode */
  IF glDebugMode THEN cTitle = cTitle + " ** DEBUG MODE **".

  C-Win:TITLE = cTitle.

  RUN GetParent (c-win:HWND, OUTPUT hParent).
  RUN GetWindow (hParent, 4, OUTPUT hOwner).
  RUN SetWindowTextA ( hOwner, cTitle ).

END PROCEDURE. /* setWindowTitle */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showDataFilters C-Win 
PROCEDURE showDataFilters :
/* Show/hide the data filters
 */
  {&timerStart}
  DEFINE INPUT PARAMETER phParentBrowse AS HANDLE  NO-UNDO.
  DEFINE INPUT PARAMETER plShow         AS LOGICAL NO-UNDO.
  DEFINE BUFFER bFilter FOR ttFilter.

  FOR EACH bFilter WHERE bFilter.hBrowse = phParentBrowse:
    IF VALID-HANDLE(bFilter.hFilter) THEN
      bFilter.hFilter:VISIBLE = plShow.
  END.

  {&timerStop}
END PROCEDURE. /* showDataFilters */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showFavourite C-Win 
PROCEDURE showFavourite :
/* Show Favourite icon
 */
  DEFINE INPUT  PARAMETER plFavourite AS LOGICAL     NO-UNDO.

  DO WITH FRAME frMain:

    IF plFavourite THEN
      btnFavourite:LOAD-IMAGE(getImagePath("StarBlack.gif")).
    ELSE
      btnFavourite:LOAD-IMAGE(getImagePath("StarWhite.gif")).

  END.

END PROCEDURE. /* showFavourite */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showField C-Win 
PROCEDURE showField :
/* Toggle the selected status of a field.
 */
  DEFINE INPUT PARAMETER pcFieldList AS CHARACTER NO-UNDO.
  DEFINE INPUT PARAMETER plSelected  AS LOGICAL   NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.
  DEFINE BUFFER bField  FOR ttField.

  setWindowFreeze(YES).

  DO WITH FRAME {&FRAME-NAME}:

    FOR EACH bColumn WHERE CAN-DO(pcFieldList,bColumn.cFullName)
      , EACH bField WHERE bField.cFieldName = bColumn.cFieldName:

      bField.lShow = (IF plSelected = ? THEN NOT bField.lShow ELSE plSelected).

      /* Customization option for the user to show/hide certain fields */
      IF NUM-ENTRIES(pcFieldList) > 1 THEN
        PUBLISH 'customShowField' (gcCurrentDatabase, gcCurrentTable, bField.cFieldName, INPUT-OUTPUT bField.lShow).

      /* Hide data columns */
      IF VALID-HANDLE(bColumn.hColumn) THEN
      DO:
        bColumn.hColumn:VISIBLE = bField.lShow.
        /* run dataScrollNotify(input ghDataBrowse). DBG: is this needed? */
      END.

      /* This solves a strange error:
       * Uncheck a field in the field browse, leave focus on the checkbox
       * Right click on data browse, choose 'Unhide all'
       * Now all fields unhide, except the one with focus.
       */
      IF bColumn.cFieldName = brFields:GET-BROWSE-COLUMN(3):SCREEN-VALUE THEN
        brFields:GET-BROWSE-COLUMN(1):CHECKED = bField.lShow.

    END. /* f/e bColumn */

    /* If we (de)selected using ENTER/SPACE, go to the next row */
    IF LAST-EVENT:EVENT-TYPE = "KEYPRESS"
      AND (LAST-EVENT:CODE = 32 OR LAST-EVENT:CODE = 13) THEN
      brFields:SELECT-NEXT-ROW().

    /* Reset the TAB order of the filter fields */
    RUN setFilterFieldTabOrder.

    saveSelectedFields().
    brFields:REFRESH().
    RUN dataScrollNotify(ghDataBrowse).
  END.

  setWindowFreeze(NO).

END PROCEDURE. /* showField */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showHint C-Win 
PROCEDURE showHint :
/* Show a small window with a hint
 */
  DEFINE INPUT PARAMETER phWidget AS HANDLE    NO-UNDO.
  DEFINE INPUT PARAMETER piLayout AS INTEGER   NO-UNDO.
  DEFINE INPUT PARAMETER pcText   AS CHARACTER NO-UNDO.

  DEFINE VARIABLE iStep     AS INTEGER NO-UNDO.
  DEFINE VARIABLE iOffsetX  AS INTEGER NO-UNDO.
  DEFINE VARIABLE iOffsetY  AS INTEGER NO-UNDO.
  DEFINE VARIABLE iTargetX  AS INTEGER NO-UNDO.
  DEFINE VARIABLE iTargetY  AS INTEGER NO-UNDO.
  DEFINE VARIABLE hMyWidget AS HANDLE  NO-UNDO.

  /* If user pressed ESC during show of hint, this is TRUE */
  IF glHintCancelled THEN RETURN.

  PUBLISH "debugMessage" (3, SUBSTITUTE("Show hint for &1 &2 (pos &3,&4)", phWidget:TYPE, phWidget:NAME, phWidget:X, phWidget:Y)).

  DO WITH FRAME frHint:
    /* Kill scrollbars */
    RUN showScrollBars(FRAME frHint:HANDLE, NO, NO).

    FRAME frHint:PRIVATE-DATA = STRING(phWidget).
    FRAME frHint:VISIBLE = FALSE.
    FRAME frHint:MOVE-TO-TOP().

    CASE piLayout:
      /* point nowhere */
      WHEN {&ARROW-NONE} THEN ASSIGN
                    iOffsetX = (phWidget:WIDTH-PIXELS - FRAME frHint:WIDTH-PIXELS) / 2
                    iOffsetY = (phWidget:HEIGHT-PIXELS - FRAME frHint:HEIGHT-PIXELS) / 2.

      /* point left up */
      WHEN {&ARROW-LEFT-UP} THEN ASSIGN
                    iOffsetX = phWidget:WIDTH-PIXELS / 3 * 2
                    iOffsetY = phWidget:HEIGHT-PIXELS / 3 * 2.

      /* point right up */
      WHEN {&ARROW-RIGHT-UP} THEN ASSIGN
                    iOffsetX = phWidget:WIDTH-PIXELS / 3 - FRAME frHint:WIDTH-PIXELS
                    iOffsetY = phWidget:HEIGHT-PIXELS / 3 * 2.

      /* point right down */
      WHEN {&ARROW-RIGHT-DOWN} THEN ASSIGN
                    iOffsetX = phWidget:WIDTH-PIXELS / 3 - FRAME frHint:WIDTH-PIXELS
                    iOffsetY = phWidget:HEIGHT-PIXELS / 3 - FRAME frHint:HEIGHT-PIXELS.

      /* point left down */
      WHEN {&ARROW-LEFT-DOWN} THEN ASSIGN
                    iOffsetX = phWidget:WIDTH-PIXELS / 3 * 2
                    iOffsetY = phWidget:HEIGHT-PIXELS / 3 - FRAME frHint:HEIGHT-PIXELS.
    END CASE.

    /* Calculate the end position. The start is the position
     * of the widget itself. Except if it is a window because
     * we want to have the relative position within the window.
     */
    hMyWidget = phWidget.
    REPEAT:
      IF NOT VALID-HANDLE(hMyWidget) OR hMyWidget:TYPE = "WINDOW" THEN LEAVE.

      PUBLISH "debugMessage" (3, SUBSTITUTE("  - Widget &1 &2 at &3,&4", hMyWidget:TYPE, hMyWidget:NAME, hMyWidget:X, hMyWidget:Y )).

      IF hMyWidget:X <> ? THEN iTargetX = iTargetX + hMyWidget:X.
      IF hMyWidget:Y <> ? THEN iTargetY = iTargetY + hMyWidget:Y.

      hMyWidget = hMyWidget:PARENT.
    END.

    ASSIGN iTargetX = iTargetX + iOffsetX
           iTargetY = iTargetY + iOffsetY.

    PUBLISH "debugMessage" (3, SUBSTITUTE("  - Offset: &1,&2", iOffsetX, iOffsetY )).
    PUBLISH "debugMessage" (3, SUBSTITUTE("  - Target: &1,&2", iTargetX, iTargetY )).

    /* Let the arrow point in the right direction and place it at the
     * correct position. Then, relocate the editor if needed.
     */
    CASE piLayout:
      WHEN {&ARROW-NONE} THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("DataDigger24x24.gif")).
        ASSIGN
          imgArrow:X = 10
          imgArrow:Y = 10
          edHint:X   = imgArrow:X + imgArrow:WIDTH-PIXELS + 1
          .
      END.

      WHEN {&ARROW-LEFT-UP} THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("LeftUp.gif")).
        ASSIGN
          imgArrow:X = 1
          imgArrow:Y = 1
          edHint:X   = imgArrow:X + imgArrow:WIDTH-PIXELS + 5
          .
      END.

      WHEN {&ARROW-RIGHT-UP} THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("RightUp.gif")).
        ASSIGN
          imgArrow:X = FRAME frHint:WIDTH-PIXELS - imgArrow:WIDTH-PIXELS - 2
          imgArrow:Y = 1
          edHint:X   = 5
          .
      END.

      WHEN {&ARROW-RIGHT-DOWN} THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("RightDown.gif")).
        ASSIGN
          imgArrow:X = FRAME frHint:WIDTH-PIXELS - imgArrow:WIDTH-PIXELS - 2
          imgArrow:Y = FRAME frHint:HEIGHT-PIXELS - imgArrow:HEIGHT-PIXELS - 2
          edHint:X   = 20
          .
      END.

      WHEN {&ARROW-LEFT-DOWN} THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("LeftDown.gif")).
        ASSIGN
          imgArrow:X = 1
          imgArrow:Y = FRAME frHint:HEIGHT-PIXELS - imgArrow:HEIGHT-PIXELS - 2
          edHint:X   = 20
          .
      END.
    END CASE.

    /* Button label */
    IF piLayout = {&ARROW-NONE} THEN
      btGotIt:LABEL = "Ok".
    ELSE
      btGotIt:LABEL = "I Got it".

    btGotIt:X = 1.
    btGotIt:WIDTH = LENGTH(btGotIt:LABEL) + 4.
    btGotIt:X = (FRAME frHint:WIDTH-PIXELS / 2 - btGotIt:WIDTH-PIXELS / 2).

    edHint:SCREEN-VALUE IN FRAME frHint = pcText.
    FRAME frHint:VISIBLE = TRUE. 

    /* Animation. Needless, but fun to program :) */
    DO iStep = 1 TO 25:
      RUN doNothing(10).
      FRAME frHint:X = FRAME frHint:X + ((iTargetX - FRAME frHint:X) / 25 * iStep).
      FRAME frHint:Y = FRAME frHint:Y + ((iTargetY - FRAME frHint:Y) / 25 * iStep).
    END.

    WAIT-FOR "choose" OF btGotIt IN FRAME frHint
      OR CLOSE OF THIS-PROCEDURE
      OR LEAVE OF FRAME frHint
      FOCUS btGotIt /* PAUSE 2 */.

    FRAME frHint:VISIBLE = FALSE.
  END.

END PROCEDURE. /* showHint */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showNewFeatures C-Win 
PROCEDURE showNewFeatures :
/* Highlight some new features
 */
  DEFINE VARIABLE iBarPos   AS INTEGER NO-UNDO.
  DEFINE VARIABLE iColumnNr AS INTEGER NO-UNDO.

  DEFINE BUFFER bFilter FOR ttFilter.
  DEFINE BUFFER bColumn FOR ttColumn.

  demoLoop:
  DO WITH FRAME frMain:

    /* This will be checked within showHint */
    glHintCancelled = FALSE.

    /* New in 23 
    
    - New : Support for SQL Server
    - New : Multi-sort on data columns now possible
    - New : Row coloring now depending on chosen sort
    - New : Right-click on data browse now shows total, min, max, avg of selected rows

    &GLOBAL-DEFINE ARROW-NONE       0
    &GLOBAL-DEFINE ARROW-LEFT-UP    1
    &GLOBAL-DEFINE ARROW-RIGHT-UP   2
    &GLOBAL-DEFINE ARROW-RIGHT-DOWN 3
    &GLOBAL-DEFINE ARROW-LEFT-DOWN  4
    */

    /* Wait until databrowse is ready (due to timer) */
    DO WHILE NOT VALID-HANDLE(ghDataBrowse): PROCESS EVENTS. END.

    /* Multi-sort on data columns now possible */
    FOR EACH bColumn:
      IF NOT bColumn.hColumn:VISIBLE THEN NEXT.
      iColumnNr = iColumnNr + 1.
      IF iColumnNr > 1 THEN
      DO:
        RUN showHint(bColumn.hColumn, {&ARROW-LEFT-UP}, "~nUse CONTROL + CLICK to add up to 9 sort levels").
        IF glHintCancelled THEN LEAVE demoLoop.
        LEAVE.
      END.
    END.

    /* Sort button */
    RUN showHint(btnDataSort:HANDLE IN FRAME frData, {&ARROW-LEFT-DOWN}, "~nOr maintain them here").
    IF glHintCancelled THEN LEAVE demoLoop.

    /* Right-click on data browse now shows total, min, max, avg of selected rows */
    RUN showHint(ghDataBrowse, {&ARROW-RIGHT-DOWN}, "~nShow Total/Min/Max/Avg of selected rows on right-click").
    IF glHintCancelled THEN LEAVE demoLoop.
    
    /* feedback */
    RUN showHint(fiFeedback:HANDLE, 3, "~nGot some feedback? ~nClick here to mail me").
    IF glHintCancelled THEN LEAVE demoLoop.

    /* Done! */
    RUN showHint(C-Win:HANDLE, {&ARROW-NONE}, "~n That's it.~n~nHappy Digging!").
  END.

  /* back to normal */
  DO WITH FRAME frMain:
    RUN setTableView(NO,YES).
    btnResizeVer:Y = 260.
    RUN endResize.
  END.

  /* Since showHint might be called from outside this
   * proc as well, we need to reset it.
   */
  glHintCancelled = FALSE.

END PROCEDURE. /* showNewFeatures */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showNumRecords C-Win 
PROCEDURE showNumRecords :
/*
 * Show nr of total and selected records
 */
  DEFINE INPUT PARAMETER piNumRecords    AS INTEGER NO-UNDO.
  DEFINE INPUT PARAMETER plQueryComplete AS LOGICAL NO-UNDO.

  DO WITH FRAME frData:

    IF plQueryComplete <> ? AND piNumRecords <> ? THEN
    DO:
      IF plQueryComplete = TRUE THEN
      DO:
        fiNumRecords:SCREEN-VALUE = SUBSTITUTE('&1 records', piNumRecords).
        fiNumRecords:FGCOLOR = getColor('RecordCount:Complete:fg'). /* green */
      END.
      ELSE
      DO:
        fiNumRecords:SCREEN-VALUE = SUBSTITUTE('> &1 records', piNumRecords).
        fiNumRecords:FGCOLOR = getColor('RecordCount:Incomplete:fg'). /* red */
      END.

      fiNumRecords:VISIBLE = (piNumRecords > 0).
    END.

    /* Set proper position. */
    fiNumRecords:X = 1. /* park it to the left so we can expand it */
    fiNumRecords:WIDTH-PIXELS = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(fiNumRecords:SCREEN-VALUE,FRAME frMain:FONT) + 5.
    fiNumRecords:X = rctData:X + rctData:WIDTH-PIXELS - fiNumRecords:WIDTH-PIXELS - 40.
    fiNumRecords:Y = rctData:Y + rctData:HEIGHT-PIXELS - 6.

  END.

END PROCEDURE. /* showNumRecords */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showNumSelected C-Win 
PROCEDURE showNumSelected :
/*
 * Show nr of selected records
 */
  IF NOT VALID-HANDLE(ghDataBrowse) THEN RETURN.

  DO WITH FRAME frData:

    fiNumSelected:SCREEN-VALUE = SUBSTITUTE('&1 /',ghDataBrowse:NUM-SELECTED-ROWS).

    /* Set proper position. */
    fiNumSelected:X = 1. /* park it to the left so we can expand it */
    fiNumSelected:WIDTH-PIXELS = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(fiNumSelected:SCREEN-VALUE,FRAME frMain:FONT) + 10.
    fiNumSelected:X = fiNumRecords:X - fiNumSelected:WIDTH-PIXELS.
    fiNumSelected:Y = rctData:Y + rctData:HEIGHT-PIXELS - 6.
    fiNumSelected:VISIBLE = (ghDataBrowse:NUM-SELECTED-ROWS > 0).

  END.

END PROCEDURE. /* showNumSelected */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showTour C-Win 
PROCEDURE showTour :
/* Highlight some of the main features of DD
 */
  DEFINE VARIABLE hWidget AS HANDLE  NO-UNDO.
  DEFINE VARIABLE iColumn AS INTEGER NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn.
  DEFINE BUFFER bFilter FOR ttFilter.

  /* This will be checked within showHint */
  glHintCancelled = FALSE.

  /*
   * 0 = nowhere
   * 1 = left up
   * 2 = right up
   * 3 = right down
   * 4 = left down
   */
  hintBlock:
  DO WITH FRAME frMain:
    RUN showHint(C-Win:HANDLE, {&ARROW-NONE}, "~nWelcome to the DataDigger 1 minute tour").
    IF glHintCancelled THEN LEAVE hintBlock.

    /* Select a table and show data */
    RUN setPage({&PAGE-TABLES}).

    RUN showHint(brTables:HANDLE     , {&ARROW-LEFT-UP}   , "~nHere are all tables of the currently connected databases").
    RUN showHint(fiTableFilter:HANDLE, {&ARROW-LEFT-UP}   , "~nType in (a part of) the table name to filter the browse").
    RUN showHint(btnFavourite:HANDLE , {&ARROW-LEFT-DOWN} , "~nMark tables as favourite").
    RUN showHint(ficWhere:HANDLE,      {&ARROW-RIGHT-UP}  , "~nYour custom query goes here ...").
    RUN showHint(btnViewData:HANDLE  , {&ARROW-RIGHT-DOWN}, "~nPress enter or click this button to show the data").

    /* Filter fields */
    RUN showHint(BROWSE brFields:GET-BROWSE-COLUMN(1), {&ARROW-LEFT-UP}, "~nHide fields by deselecting them here").
    RUN showHint(BROWSE brFields:GET-BROWSE-COLUMN(5), {&ARROW-RIGHT-UP}, "~nClick on the format to change it on the fly").
    RUN showHint(btnReset:HANDLE                     , {&ARROW-RIGHT-UP}, "~nRe-arrange fields with the buttons in this panel").

    /* Let the hint frame point to the 2nd visible filter instead of the 1st */
    FOR EACH bColumn:
      IF NOT bColumn.hColumn:VISIBLE THEN NEXT.
      iColumn = iColumn + 1.
      IF iColumn > 1 THEN
      DO:
        RUN showHint(bColumn.hFilter, {&ARROW-LEFT-UP}, "~nFilter data by filling in the filter boxes. Your filters are saved for re-use").
        LEAVE.
      END.
    END.

    /* Confess the lie */
    RUN showHint(C-Win:HANDLE, {&ARROW-NONE}, "~nOk, I lied :)~nIt's more than 1 minute, but you're almost done!").

    /* Resize bar */
    RUN showHint(btnResizeVer:HANDLE, {&ARROW-RIGHT-UP}, "~nDrag this bar up and down to change the size of the upper browse.").

    iColumn = 0.
    FOR EACH bColumn:
      IF NOT bColumn.hColumn:VISIBLE THEN NEXT.
      iColumn = iColumn + 1.
      IF iColumn = 1 THEN DO:
        RUN showHint(bColumn.hColumn,   {&ARROW-LEFT-DOWN}, "~nClick on a column header to sort on that column. Again to change the sort").
        RUN showHint(btnDataSort:HANDLE IN FRAME frData,{&ARROW-LEFT-UP}, "~nUse CONTROL + CLICK or this button to add up to 9 sort levels").
      END.
      ELSE
      IF iColumn = 2 THEN
        RUN showHint(bColumn.hColumn, {&ARROW-LEFT-DOWN}, "~nGrab the side of a column to resize it.").
      ELSE
        LEAVE.
    END.

    RUN showHint(btnQueries:HANDLE, {&ARROW-RIGHT-UP}, "~nQueries are saved here for re-use (hint: try PGUP / PGDN in the query box)").

    /* Manipulate data */
    RUN showHint(btnEdit:HANDLE   , {&ARROW-LEFT-DOWN}, "~nAdd, update, clone, delete, dump records easily using these buttons").

    /* Extra options */
    RUN showHint(btnTools:HANDLE  , {&ARROW-LEFT-UP}, "~nOk, last tip. Use this for settings and extra tools").

    /* Done! */
    RUN showHint(C-Win:HANDLE, {&ARROW-NONE}, "~n That's it.~n~nHappy Digging!").

    FRAME frHint:VISIBLE = FALSE.
  END.

  /* Since showHint is called outside this proc as well, we need to reset it */
  glHintCancelled = FALSE.

END PROCEDURE. /* showTour */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showValue C-Win 
PROCEDURE showValue :
/* Show the sum of the fields of the selected rows
 */
  DEFINE VARIABLE hDataBuffer AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iRecord     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cColumnName  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cColumnValue AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE dColumnTotal AS DECIMAL     NO-UNDO.
  DEFINE VARIABLE dColumnValue AS DECIMAL     NO-UNDO.
  DEFINE VARIABLE dMinValue    AS DECIMAL     NO-UNDO.
  DEFINE VARIABLE dMaxValue    AS DECIMAL     NO-UNDO.
  DEFINE VARIABLE dAvgValue    AS DECIMAL     NO-UNDO.
  DEFINE VARIABLE iExtentNr    AS INTEGER     NO-UNDO.

  /* Get data */
  IF NOT VALID-HANDLE(ghDataBrowse) THEN RETURN.
  IF NUM-ENTRIES(ghDataBrowse:PRIVATE-DATA,CHR(1)) <> 3 THEN RETURN.
  hDataBuffer = ghDataBrowse:QUERY:GET-BUFFER-HANDLE(1).
  IF NOT hDataBuffer:AVAILABLE THEN RETURN.

  /* Walk thru all selected records */
  cColumnName  = ENTRY(1, ghDataBrowse:PRIVATE-DATA,CHR(1)).
  cColumnValue = ENTRY(2, ghDataBrowse:PRIVATE-DATA,CHR(1)).

  /* If we have clicked on an extent field, extract the extent nr */
  IF cColumnName MATCHES '*[*]' THEN
    ASSIGN
      iExtentNr = INTEGER(ENTRY(1, ENTRY(2,cColumnName,'['), ']'))
      cColumnName = ENTRY(1,cColumnName,'[').

  SESSION:SET-WAIT-STATE('general').

  DO iRecord = 1 TO ghDataBrowse:NUM-SELECTED-ROWS:
    ghDataBrowse:FETCH-SELECTED-ROW(iRecord).

    cColumnValue = hDataBuffer:BUFFER-FIELD(cColumnName):BUFFER-VALUE(iExtentNr).
    dColumnValue = DECIMAL(cColumnValue) NO-ERROR.

    /* Min/Max */
    IF iRecord = 1 OR dColumnValue < dMinValue THEN dMinValue = dColumnValue.
    IF iRecord = 1 OR dColumnValue > dMaxValue THEN dMaxValue = dColumnValue.

    /* Total */
    IF NOT ERROR-STATUS:ERROR AND dColumnValue <> ? THEN
      dColumnTotal = dColumnTotal + dColumnValue.
  END.

  dAvgValue = dColumnTotal / ghDataBrowse:NUM-SELECTED-ROWS.
  SESSION:SET-WAIT-STATE('').

  IF ghDataBrowse:NUM-SELECTED-ROWS > 1 THEN
    MESSAGE
      'Total of' ghDataBrowse:NUM-SELECTED-ROWS 'rows~t:' dColumnTotal SKIP
      'Min / Max / Avg~t:' dMinValue ' / ' dMaxValue ' / ' dAvgValue
      VIEW-AS ALERT-BOX INFO BUTTONS OK.
  ELSE
  IF cColumnValue <> '' AND cColumnValue <> ? THEN
    MESSAGE TRIM(cColumnValue) VIEW-AS ALERT-BOX INFO BUTTONS OK.

END PROCEDURE. /* showValue */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE sortComboBox C-Win 
PROCEDURE sortComboBox :
/* Sort the entries of a ComboBox
 */
  DEFINE INPUT  PARAMETER phCombo AS HANDLE NO-UNDO.

  DEFINE VARIABLE iItem  AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cList  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDelim AS CHARACTER   NO-UNDO.

  EMPTY TEMP-TABLE ttItem.
  cList = phCombo:LIST-ITEMS.
  cDelim = phCombo:DELIMITER.

  DO iItem = 1 TO NUM-ENTRIES(cList,cDelim).
    CREATE ttItem.
    ASSIGN ttItem.cItem = ENTRY(iItem,cList,cDelim).
  END.

  cList = "".
  FOR EACH ttItem WHERE ttItem.cItem <> "" BY ttItem.cItem:
    cList = cList + cDelim + ttItem.cItem.
  END.
  EMPTY TEMP-TABLE ttItem.

  phCombo:LIST-ITEMS = SUBSTRING(cList,2).
END. /* sortComboBox */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE startDiggerLib C-Win 
PROCEDURE startDiggerLib :
/* Start DiggerLib if it has not already been started
 */
  DEFINE VARIABLE cProgDir   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hDiggerLib AS HANDLE      NO-UNDO.

  FILE-INFO:FILE-NAME = REPLACE(THIS-PROCEDURE:FILE-NAME,"wdatadigger.w","wdatadigger.r").
  IF FILE-INFO:FULL-PATHNAME = ? THEN
    FILE-INFO:FILE-NAME = REPLACE(THIS-PROCEDURE:FILE-NAME,"wdatadigger.r","wdatadigger.w").

  cProgDir = SUBSTRING(FILE-INFO:FULL-PATHNAME,1,R-INDEX(FILE-INFO:FULL-PATHNAME,'\')).

  /* If we run in the UIB we know where we are running from */
  IF "{&UIB_is_Running}" <> "" THEN cProgDir = '.\'.

  /* Call out to see if the lib has been started for this build nr */
  PUBLISH 'DataDiggerLib' (OUTPUT hDiggerLib).

  /* If it is not, then start it */
  IF NOT VALID-HANDLE(hDiggerLib) THEN
  DO:
    RUN VALUE(cProgDir + 'DataDiggerLib.p') PERSISTENT SET hDiggerLib.
    SESSION:ADD-SUPER-PROCEDURE(hDiggerLib,SEARCH-TARGET).
  END.

  /* Start customizations in myDataDigger.p */
  IF SEARCH(cProgDir + 'myDataDigger.p') <> ? THEN
  DO:
    RUN VALUE(cProgDir + 'myDataDigger.p') PERSISTENT SET hDiggerLib.
    SESSION:ADD-SUPER-PROCEDURE(hDiggerLib, SEARCH-TARGET).

    SUBSCRIBE PROCEDURE hDiggerLib TO "customDump"   ANYWHERE.
    SUBSCRIBE PROCEDURE hDiggerLib TO "customFormat" ANYWHERE.
    SUBSCRIBE PROCEDURE hDiggerLib TO "customQuery"  ANYWHERE.
    SUBSCRIBE PROCEDURE hDiggerLib TO "customShowField" ANYWHERE.
    SUBSCRIBE PROCEDURE hDiggerLib TO "CustomGetFilterValue" ANYWHERE.
    SUBSCRIBE PROCEDURE hDiggerLib TO "customSaveFilterValue" ANYWHERE.

  END.
END PROCEDURE. /* startDiggerLib */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE startSession C-Win 
PROCEDURE startSession :
/* Show a welcome message to the user.
 */
  DEFINE VARIABLE cBuild     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cError     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE hWindow    AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iStackSize AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iVersion   AS INTEGER   NO-UNDO.
  DEFINE VARIABLE lNewBuild  AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE lNewUser   AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE lUpgraded  AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE lOpenBlog  AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE iChannel   AS INTEGER   NO-UNDO.

  /* Set debug flag */
  setDebugMode(LOGICAL(getRegistry('DataDigger:debugger','DebugMode'))).

  /* Check if this is the first run with a new version */
  iVersion = INTEGER(getRegistry('DataDigger', 'Version')) NO-ERROR.
  IF iVersion = ? THEN lNewUser = TRUE.

  cBuild = getRegistry('DataDigger', 'Build').
  IF cBuild = ? THEN cBuild = "".

  /* obsolete files, only for beta-users, only for first build after 20140314 */
  OS-DELETE VALUE( "dDump.w").
  OS-DELETE VALUE( "dDump.r").

  IF iVersion <> {&VERSION} OR cBuild <> '{&build}' THEN
  DO:
    lUpgraded = TRUE.

    /* If we come from an older version, do some conversions */
    IF iVersion <> ? THEN
    DO:
      RUN showMessage.p(INPUT "Conversion", INPUT "Please wait while your settings are converted.", OUTPUT hWindow).

      /* Do one-time conversions if needed */
      SESSION:SET-WAIT-STATE("general").
      convLoop:
      REPEAT:
        RUN convertSettings(iVersion,cBuild).
        iVersion = iVersion + 1.
        IF iVersion >= {&VERSION} THEN LEAVE convLoop.
      END.
      DELETE OBJECT hWindow.
      SESSION:SET-WAIT-STATE("").
    END.

    /* Save this version nr */
    setRegistry('DataDigger', 'Version', '{&version}').
    setRegistry('DataDigger', 'Build', '{&build}').
  END.

  /* New build nr? Then wipe disk cache */
  IF cBuild <> '{&build}' THEN
  DO:
    lNewBuild = TRUE.
    RUN clearDiskCache.
    RUN convertSettings(iVersion, cBuild).
    setRegistry('DataDigger', 'Build', '{&build}').
  END.

  /* Check on the use of -rereadnolock */
  IF LOOKUP('-rereadnolock', SESSION:STARTUP-PARAMETERS) = 0 THEN
    RUN showHelp('RereadNoLock', '').

  /* Check on the value for -s, should preferrably be > 128 */
  iStackSize = getStackSize().
  IF iStackSize <= 128 THEN
    RUN showHelp('StackSize', STRING(iStackSize)).

  /* If we are a READ-ONLY digger, show a warning */
  IF plReadOnlyDigger THEN
    RUN showHelp("ReadOnlyDigger", "").

  /* The user could be:
   * 1) a new user
   * 2) existing user on upgraded DD
   * 3) existing user on non-upgraded DD
   */
  IF lNewUser THEN RUN showTour.
  ELSE IF lUpgraded THEN RUN showNewFeatures.

  /* Start up the page on WordPress.com for this build */
  IF lNewBuild THEN
  DO:
    lOpenBlog = LOGICAL(getRegistry('DataDigger','OpenBlogOnNewVersion')).
    IF lOpenBlog = ? OR lOpenBlog = TRUE THEN
      OS-COMMAND NO-WAIT START VALUE("http://datadigger.wordpress.com/{&build}").
  END.

  /* PhoneHome / Check for new version only once a day */
  IF getRegistry('DataDigger:Update','LastUpdateCheck') <> ISO-DATE(TODAY) THEN 
  DO:
    /* Phone Home 
     * Don't be alarmed, this link refers to the build.i version on GitHub. 
     * This to track the use of DataDigger. 
     * Interested yourself? Check https://goo.gl/24deK3+ to see statistics
    */
    IF getRegistry('DataDigger:Update','PingBack') <> ? THEN
      RUN urlDownloadToFileA (0, '{&PINGBACKURL}', '', 0, 0).

    /* Check for new versions on GitHub */
    iChannel = INTEGER(getRegistry('DataDigger:Update','UpdateChannel')).
    RUN checkVersion.p(INPUT iChannel, INPUT FALSE). /* no manual check */

    setRegistry('DataDigger:Update','LastUpdateCheck',ISO-DATE(TODAY)).
  END.

END PROCEDURE. /* startSession */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE startTool C-Win 
PROCEDURE startTool :
/* Start Dictionary or Data Adminstration
 */
  DEFINE INPUT PARAMETER pcTool AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cDatabases    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabasesOld AS CHARACTER   NO-UNDO.

  /* Return if no db connected */
  IF NUM-DBS = 0 THEN RETURN.
  
  /* Don't allow in read only mode */
  IF plReadOnlyDigger THEN RETURN.

  HIDE FRAME frSettings.
  CREATE ALIAS dictdb FOR DATABASE VALUE(gcCurrentDatabase).

  /* Turn off KeepAlive timer to avoid "DB has changed warnings" */
  RUN setTimer("KeepAlive", 0).

  /* Remember all connected db's */
  cDatabasesOld = getDatabaseList().

  CASE pcTool:
    WHEN "Dict" THEN DO:
      PUBLISH "setUsage" ("Dictionary"). /* user behaviour */

      RUN setTimer("resizeDictWindow", 500).
      RUN DICT.p.
      RUN setTimer("resizeDictWindow", 0).
    END.

    WHEN "Admin" THEN
    DO:
      PUBLISH "setUsage" ("DataAdmin"). /* user behaviour */
      RUN _admin.p.
    END.
  END CASE.

  /* re-enable KeepAlive timer */
  IF LOGICAL(getRegistry("DataDigger", "KeepAlive")) THEN
    RUN setTimer("KeepAlive", 5000). /* every 5 seconds */

  /* Get all connected databases */
  cDatabases = getDatabaseList().

  /* If needed, repopulate db combo */
  IF cDatabases <> cDatabasesOld THEN
  DO:
    /* Get list of all tables of all databases */
    RUN getTables(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).
    ASSIGN cbDatabaseFilter:LIST-ITEMS IN FRAME frMain = ',' + cDatabases.
    RUN filterTables.
  END.
  ELSE
    CREATE ALIAS dictdb FOR DATABASE VALUE(gcCurrentDatabase).

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE tgSelAllChoose C-Win 
PROCEDURE tgSelAllChoose :
/*------------------------------------------------------------------------------
  Purpose:
  Parameters:  <none>
  Notes:
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER plSelectAll AS LOGICAL NO-UNDO.

  DEFINE VARIABLE cFieldList AS CHARACTER NO-UNDO.
  DEFINE VARIABLE hQuery     AS HANDLE    NO-UNDO.
  DEFINE VARIABLE hBuffer    AS HANDLE    NO-UNDO.

  SESSION:SET-WAIT-STATE('general').
  setWindowFreeze(YES).

  DEFINE BUFFER bField  FOR ttField.
  DEFINE BUFFER bColumn FOR ttColumn.

  DO WITH FRAME {&FRAME-NAME}:

    CREATE QUERY hQuery.
    CREATE BUFFER hBuffer FOR TABLE "ttField".
    hQuery:ADD-BUFFER(hBuffer).
    hQuery:QUERY-PREPARE(BROWSE brFields:QUERY:PREPARE-STRING).
    hQuery:QUERY-OPEN.

    /* Walk thru all fields that are currently visible */
    REPEAT:
      hQuery:GET-NEXT().
      IF hQuery:QUERY-OFF-END THEN LEAVE.

      /* Update the buffer with checked/unchecked value */
      hBuffer::lShow = plSelectAll.

      FOR EACH bColumn WHERE bColumn.cFieldName = hBuffer::cFieldName:
        cFieldList = cFieldList + "," + bColumn.cFullName.
      END.
    END.

    RUN showField(INPUT cFieldList, INPUT plSelectAll).

    hQuery:QUERY-CLOSE.
    DELETE OBJECT hQuery.
    DELETE OBJECT hBuffer.

    saveSelectedFields().
    brFields:REFRESH().

    RUN reopenFieldBrowse(?,?).

    setWindowFreeze(NO).
    SESSION:SET-WAIT-STATE('').

    APPLY "entry" TO ttField.cFieldName IN BROWSE brFields.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE timedFieldFilter C-Win 
PROCEDURE timedFieldFilter :
/* Activated by the timer to apply the filter
 */
  setWindowFreeze(YES).

  RUN reopenFieldBrowse(?,?).
  APPLY "value-changed" TO brFields IN FRAME frMain.
  RUN setTimer("timedFieldFilter", 0). /* turn off the timer */

  setWindowFreeze(NO).
END PROCEDURE. /* timedFieldFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE timedIndexFilter C-Win 
PROCEDURE timedIndexFilter :
/* Activated by the timer to apply the filter
 */
  setWindowFreeze(YES).

  RUN reopenIndexBrowse(?,?).
  APPLY "value-changed" TO brIndexes IN FRAME frMain.
  RUN setTimer("timedIndexFilter", 0). /* turn off the timer */

  setWindowFreeze(NO).
END PROCEDURE. /* timedIndexFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE timedScrollNotify C-Win 
PROCEDURE timedScrollNotify :
/* Run scrollnotify when user scrolls using cursor keys
 */
  DEFINE BUFFER bColumn FOR ttColumn.

  /* Might get called when browse is not yet realized, so: */
  IF NOT VALID-HANDLE(ghDataBrowse) THEN RETURN.

  /* Find most right column in the browse */
  FOR EACH bColumn BY bColumn.iColumnNr DESCENDING:

    IF NOT VALID-HANDLE(bColumn.hColumn)
      OR bColumn.hColumn:VISIBLE = FALSE THEN NEXT.

    IF bColumn.hColumn:X <> giLastDataColumnX THEN
    DO:
      RUN dataScrollNotify(INPUT ghDataBrowse).
      giLastDataColumnX = bColumn.hColumn:X.
    END.

    LEAVE.
  END.
END PROCEDURE. /* timedScrollNotify */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE timedTableChange C-Win 
PROCEDURE timedTableChange :
/* Activated by the timer to change the browse
 */
  setWindowFreeze(YES).
  RUN setTimer("timedTableChange", 0).
  RUN setTableContext(INPUT gcCurrentTable ).
  setWindowFreeze(NO).

END PROCEDURE. /* timedTableChange */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE timedTableFilter C-Win 
PROCEDURE timedTableFilter :
/* Activated by the timer to apply the filter
 */
  setWindowFreeze(YES).

  RUN reopenTableBrowse(?).
  APPLY "value-changed" TO brTables IN FRAME frMain.
  RUN setTimer("timedTableFilter", 0). /* turn off the timer */

  setWindowFreeze(NO).
END PROCEDURE. /* timedTableChange */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* ************************  Function Implementations ***************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION createMenu C-Win 
FUNCTION createMenu RETURNS HANDLE
  ( phParent AS HANDLE ) :

  DEFINE VARIABLE hMenu AS HANDLE NO-UNDO.

  IF VALID-HANDLE(phParent) THEN
    hMenu = phParent:POPUP-MENU.

  /* Kill the current menu */
  IF VALID-HANDLE(hMenu) THEN killMenu(hMenu).

  /* Create the menu itself */
  CREATE MENU hMenu
    ASSIGN
      POPUP-ONLY = TRUE
      SENSITIVE  = TRUE
    TRIGGERS:
      ON "menu-drop" PERSISTENT RUN menuDropDataBrowse IN THIS-PROCEDURE. /* enable/disable menu-items */
    END TRIGGERS.

  IF VALID-HANDLE(phParent) THEN
    phParent:POPUP-MENU = hMenu.

  RETURN hMenu.

END FUNCTION. /* createMenu */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION createMenuItem C-Win 
FUNCTION createMenuItem RETURNS HANDLE
  ( phMenu    AS HANDLE
  , pcType    AS CHARACTER
  , pcLabel   AS CHARACTER
  , pcName    AS CHARACTER
  ) :

  DEFINE VARIABLE hMenuItem AS HANDLE NO-UNDO.

  CASE pcType:
    WHEN "SUBMENU" THEN
      CREATE SUB-MENU hMenuItem
        ASSIGN
          LABEL        = pcLabel
          PRIVATE-DATA = pcLabel
          NAME         = pcName
          PARENT       = phMenu.

    WHEN "TOGGLE-BOX" THEN
      CREATE MENU-ITEM hMenuItem
        ASSIGN
          LABEL        = pcLabel
          PRIVATE-DATA = pcLabel
          NAME         = pcName
          TOGGLE-BOX   = TRUE
          CHECKED      = TRUE
          PARENT       = phMenu.

    WHEN "RULE" THEN
      CREATE MENU-ITEM hMenuItem
        ASSIGN
          PARENT       = phMenu
          SUBTYPE      = "rule".

    OTHERWISE
      CREATE MENU-ITEM hMenuItem
        ASSIGN
          LABEL        = pcLabel
          PRIVATE-DATA = pcLabel
          NAME         = pcName
          PARENT       = phMenu.
  END CASE.

  RETURN hMenuItem.

END FUNCTION. /* createMenuItem */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getActiveQueryEditor C-Win 
FUNCTION getActiveQueryEditor RETURNS HANDLE
  ( /* parameter-definitions */ ) :

/* Return the handle of the active query editor
 */
  IF gcQueryEditorState = 'hidden' THEN
    RETURN ficWhere:HANDLE IN FRAME frMain.
  ELSE
    RETURN ficWhere2:HANDLE IN FRAME frWhere.

END FUNCTION. /* getActiveQueryEditor */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getDroppedFiles C-Win 
FUNCTION getDroppedFiles RETURNS CHARACTER
  ( phDropTarget AS HANDLE ) :
  /* Return a list of dropped files onto a target.
   *
   * Note: list is returned as separated list, separated with ~n (newline)
   * instead of comma's since a comma can be part of the file name
   */
  DEFINE VARIABLE iFile     AS INTEGER NO-UNDO.
  DEFINE VARIABLE cFileList AS CHARACTER   NO-UNDO.

  DO iFile = 1 TO phDropTarget:NUM-DROPPED-FILES:
    cFileList = cFileList + "~n" + phDropTarget:GET-DROPPED-FILE(iFile).
  END.

  RETURN TRIM(cFileList,"~n").

END FUNCTION. /* getDroppedFiles */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getFieldList C-Win 
FUNCTION getFieldList RETURNS CHARACTER
  ( pcSortBy AS CHARACTER ) :
/* Return a comma separated list of all fields.
 */
  DEFINE VARIABLE cFieldList AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iMaxFields AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNumFields AS INTEGER     NO-UNDO.

  DEFINE BUFFER ttField FOR ttField.
  DEFINE QUERY qField FOR ttField.

  iMaxFields = INTEGER(getRegistry('DataDigger','MaxColumns')) NO-ERROR.
  IF iMaxFields = ? THEN iMaxFields = 500.

  QUERY qField:QUERY-PREPARE(SUBSTITUTE('for each ttField by &1', pcSortBy)).
  QUERY qField:QUERY-OPEN.
  QUERY qField:GET-FIRST.

  /* All fields */
  REPEAT WHILE NOT QUERY qField:QUERY-OFF-END:
    cFieldList = cFieldList + ',' + ttField.cFieldName.
    QUERY qField:GET-NEXT.
    iNumFields = iNumFields + 1.
    IF iNumFields > iMaxFields THEN LEAVE.
  END.
  QUERY qField:QUERY-CLOSE.

  cFieldList = LEFT-TRIM(cFieldList, ",").

  RETURN cFieldList.
END FUNCTION. /* getFieldList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getQueryFromFields C-Win 
FUNCTION getQueryFromFields RETURNS CHARACTER
  ( INPUT pcFieldList AS CHARACTER ):

/* Return a query built from fields in a list
 */
  DEFINE VARIABLE cField      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldList  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNameFormat AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iField      AS INTEGER     NO-UNDO.

  /* Determine format for names */
  cNameFormat = FILL('x', getMaxLength(pcFieldList) ).

  /* Build query */
  cQuery = ''.
  DO iField = 1 TO NUM-ENTRIES(pcFieldList):
    cField = ENTRY(iField,pcFieldList).
    FIND ttField WHERE ttField.cFieldName = cField.
    cQuery = SUBSTITUTE('&1&2 &3 = &4'
                       , cQuery
                       , (IF iField = 1 THEN 'WHERE' ELSE '~n  AND')
                       , STRING(cField,cNameFormat)
                       , QUOTER(getLinkInfo(cField))
                       ).
  END.

  PUBLISH "debugMessage" (1,SUBSTITUTE('Query From Fields: &1', cQuery)).

  RETURN cQuery.
END FUNCTION. /* getQueryFromFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getSelectedFields C-Win 
FUNCTION getSelectedFields RETURNS CHARACTER
  ( /* parameter-definitions */ ) :

/* Return all selected fields.
 */
  DEFINE VARIABLE cSelectedFields AS CHARACTER  NO-UNDO.
  DEFINE BUFFER ttField FOR ttField.

  /* All selected fields */
  FOR EACH ttField WHERE ttField.lShow = TRUE BY ttField.iOrder:
    cSelectedFields = cSelectedFields + ',' + ttField.cFullName.
    IF LENGTH(cSelectedFields) > 20000 THEN LEAVE.
  END.

  cSelectedFields = LEFT-TRIM(cSelectedFields, ",").

  RETURN cSelectedFields.
END FUNCTION. /* getSelectedFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getSelectedText C-Win 
FUNCTION getSelectedText RETURNS CHARACTER
  ( INPUT hWidget AS HANDLE ) :
/* Return the currently selected text in a widget
 */
  DEFINE VARIABLE cSelectedText AS CHARACTER   NO-UNDO.

  REPEAT WHILE VALID-HANDLE(hWidget):

    IF CAN-QUERY(hWidget,'SELECTION-TEXT') AND hWidget:SELECTION-TEXT <> '' THEN
      RETURN TRIM(hWidget:SELECTION-TEXT).

    IF CAN-QUERY(hWidget,'first-child') AND hWidget:FIRST-CHILD <> ? THEN
    DO:
      cSelectedText = getSelectedText(hWidget:FIRST-CHILD).
      IF cSelectedText <> "" THEN RETURN cSelectedText.
    END.

    hWidget = hWidget:NEXT-SIBLING.
  END.

  RETURN "".
END FUNCTION. /* getSelectedText */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION killMenu C-Win 
FUNCTION killMenu RETURNS LOGICAL
  ( phMenu AS HANDLE ) :

  /* Delete a menu and all of its siblings
   */
  IF VALID-HANDLE(phMenu) THEN
  DO:
    /* Kill subitems */
    DO WHILE VALID-HANDLE(phMenu:FIRST-CHILD):
      DELETE OBJECT phMenu:FIRST-CHILD.
    END.

    /* Kill the menu itself */
    DELETE OBJECT phMenu.
  END.

  RETURN TRUE.   /* Function return value. */

END FUNCTION. /* killMenu */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION saveSelectedFields C-Win 
FUNCTION saveSelectedFields RETURNS CHARACTER
  ( /* parameter-definitions */ ) :

/* Write the selected fields to the INI
 */
  DEFINE VARIABLE cTable          AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cSelectedFields AS CHARACTER NO-UNDO.

  DO WITH FRAME {&FRAME-NAME}:

    /* Get the selected fields to display in the browse */
    cTable          = gcCurrentTable.
    cSelectedFields = getSelectedFields().

    /* If no fields are selected, use a special marker */
    IF cSelectedFields = '' THEN cSelectedFields = '<none>'.

    /* If all fields are selected, we don't save the setting */
    IF NUM-ENTRIES(cSelectedFields) = NUM-ENTRIES(getFieldList('cFieldName')) THEN
      cSelectedFields = ?.

    /* Save selected fields */
    setRegistry(SUBSTITUTE("DB:&1",gcCurrentDatabase), SUBSTITUTE("&1:Fields", cTable), cSelectedFields).
  END.

  RETURN "".
END FUNCTION. /* saveSelectedFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setCurrentTable C-Win 
FUNCTION setCurrentTable RETURNS LOGICAL
  ( pcTableName AS CHARACTER ) :
/* Save last used table to ini
 */
  DEFINE BUFFER bTable FOR ttTable.

  DO WITH FRAME {&FRAME-NAME}:
    FIND bTable
      WHERE bTable.cDatabase  = gcCurrentDatabase
        AND bTable.cTableName = pcTableName
            NO-ERROR.
    IF NOT AVAILABLE bTable THEN RETURN NO.

    brTables:QUERY:REPOSITION-TO-ROWID( ROWID(bTable) ) NO-ERROR.
  END.
END FUNCTION. /* setCurrentTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setDebugMode C-Win 
FUNCTION setDebugMode RETURNS LOGICAL
  ( plDebugMode AS LOGICAL ) :

/* Turn debug mode on of off. Affects timers and LockWindow
 */
  IF plDebugMode = ? THEN RETURN NO.
  glDebugMode = plDebugMode.
  IF glUseTimer THEN chCtrlFrame:pstimer:ENABLED = NOT glDebugMode.

  RETURN TRUE.
END FUNCTION. /* setDebugMode */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setQuery C-Win 
FUNCTION setQuery RETURNS LOGICAL
  ( piPointerChange AS INTEGER ) :

/* Fetches the previous or next query from the settings and fills it in in the query editor.
 */
  DEFINE VARIABLE cQuery  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hEditor AS HANDLE      NO-UNDO.

  hEditor = getActiveQueryEditor().

  /* See if the requested query exists */
  cQuery = getQuery(gcCurrentDatabase, gcCurrentTable, giQueryPointer + piPointerChange).

  IF cQuery <> ? THEN
  DO:
    giQueryPointer = giQueryPointer + piPointerChange.
    hEditor:SCREEN-VALUE = formatQueryString(cQuery, gcQueryEditorState = 'visible').
  END.

  RETURN cQuery <> ?.
END FUNCTION. /* setQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setQueryEditor C-Win 
FUNCTION setQueryEditor RETURNS LOGICAL
  ( pcQueryEditorState AS CHARACTER ) :
/* Show or hide the query editor and associated fields.
 */
  /* If we try to set it to its current value, nothing will happen so: */
  IF pcQueryEditorState = gcQueryEditorState THEN RETURN FALSE.

  CASE pcQueryEditorState:
    WHEN 'visible' THEN
    DO:
      IF (ficWhere:X IN FRAME frMain + FRAME frWhere:WIDTH-PIXELS) > c-win:WIDTH-PIXELS THEN
        FRAME frWhere:X = (c-win:WIDTH-PIXELS - FRAME frWhere:WIDTH-PIXELS) / 2.
      ELSE
        FRAME frWhere:X = ficWhere:X.

      IF (ficWhere:Y IN FRAME frMain + FRAME frWhere:HEIGHT-PIXELS) > rctEdit:Y IN FRAME frMain THEN
        FRAME frWhere:Y = rctEdit:Y IN FRAME frMain - FRAME frWhere:HEIGHT-PIXELS - 20.
      ELSE
        FRAME frWhere:Y = ficWhere:Y.

      VIEW FRAME frWhere.

      gcQueryEditorState = pcQueryEditorState.
      IF ficWhere:SCREEN-VALUE IN FRAME frMain <> '' THEN
        ficWhere2:SCREEN-VALUE IN FRAME frWhere = formatQueryString(ficWhere:SCREEN-VALUE IN FRAME frMain, YES).
    END.

    WHEN 'hidden'  THEN
    DO:
      HIDE FRAME frWhere.

      gcQueryEditorState = pcQueryEditorState.
      IF ficWhere2:SCREEN-VALUE IN FRAME frWhere <> '' THEN
        ficWhere:SCREEN-VALUE IN FRAME frMain = formatQueryString(ficWhere2:SCREEN-VALUE IN FRAME frWhere, NO).
    END.

    /* All other settings will be ignored */
    OTHERWISE RETURN FALSE.
  END CASE.

  /* Save setting for query editor state */
  setRegistry("DataDigger", "QueryEditorState", gcQueryEditorState).

  RETURN TRUE.
END FUNCTION. /* setQueryEditor */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setUpdatePanel C-Win 
FUNCTION setUpdatePanel RETURNS LOGICAL
  ( INPUT pcMode AS CHARACTER ) :

/*------------------------------------------------------------------------------
  Purpose: setUpdatePanel
    Notes: enable / disable update panel buttons

    Mode       Sensitive buttons
    -----      --------------------
    display    add,delete,view,dump
    no-record  add
    update     save,cancel
------------------------------------------------------------------------------*/
  DEFINE VARIABLE lHasRecords AS LOGICAL NO-UNDO.

  {&timerStart}

  IF pcMode <> ? THEN gcRecordMode = pcMode.

  DO WITH FRAME frMain:

    lHasRecords = (    VALID-HANDLE(ghDataBrowse)
                   AND VALID-HANDLE(ghDataBrowse:QUERY)
                   AND ghDataBrowse:QUERY:NUM-RESULTS <> ?
                   AND ghDataBrowse:QUERY:NUM-RESULTS > 0).

    ASSIGN
      btnAdd:SENSITIVE         = LOOKUP( gcRecordMode, 'display,no-record') > 0 AND NOT plReadOnlyDigger
      btnClone:SENSITIVE       = LOOKUP( gcRecordMode, 'display') > 0 AND lHasRecords AND NOT plReadOnlyDigger
      btnEdit:SENSITIVE        = LOOKUP( gcRecordMode, 'display') > 0 AND lHasRecords AND ghDataBrowse:NUM-SELECTED-ROWS > 0
      btnDelete:SENSITIVE      = LOOKUP( gcRecordMode, 'display') > 0 AND lHasRecords AND ghDataBrowse:NUM-SELECTED-ROWS > 0 AND NOT plReadOnlyDigger
      btnView:SENSITIVE        = LOOKUP( gcRecordMode, 'display') > 0 AND lHasRecords AND ghDataBrowse:NUM-SELECTED-ROWS > 0
      btnDump:SENSITIVE        = LOOKUP( gcRecordMode, 'display') > 0 AND lHasRecords AND ghDataBrowse:NUM-ITERATIONS > 0
      btnLoad:SENSITIVE        = LOOKUP( gcRecordMode, 'display,no-record') > 0 AND NOT plReadOnlyDigger
      .

    /* Hide these when no data browse */
    DO WITH FRAME frData:
      ASSIGN
        btnClearDataFilter:VISIBLE = VALID-HANDLE(ghDataBrowse)
        btnDataSort:VISIBLE        = VALID-HANDLE(ghDataBrowse)
        fiNumRecords:VISIBLE       = (gcRecordMode <> 'no-record')
        .
    END.
  END.

  /* Kill scrollbars */
  RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO).

  {&timerStop}
END FUNCTION. /* setUpdatePanel */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setWindowFreeze C-Win 
FUNCTION setWindowFreeze RETURNS LOGICAL
  ( plWindowsLocked AS LOGICAL ) :
  /* Freeze updates to the screen
   */
  IF glDebugMode THEN RETURN NO.
  RUN LockWindow (INPUT C-Win:HANDLE, INPUT plWindowsLocked).

  RETURN TRUE.
END FUNCTION. /* setWindowFreeze */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION trimList C-Win 
FUNCTION trimList RETURNS CHARACTER
  ( pcList  AS CHARACTER
  , pcSep   AS CHARACTER
  , piItems AS INTEGER
  ):

  /*
   * Strip elements from a list if there are too much
   */
  DO WHILE NUM-ENTRIES(pcList,pcSep) > piItems:
    ENTRY(NUM-ENTRIES(pcList,pcSep),pcList,pcSep) = "".
    pcList = RIGHT-TRIM(pcList,pcSep).
  END.

  RETURN pcList.

END FUNCTION. /* trimList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
