&ANALYZE-SUSPEND _VERSION-NUMBER UIB_v9r12 GUI
&ANALYZE-RESUME
/* Connected Databases 
*/
&Scoped-define WINDOW-NAME C-Win
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS C-Win 
/*------------------------------------------------------------------------
  Name         : wDataDigger.w
  Description  : Main program for DataDigger
  ---------------------------------------------------------------------- 
  15-10-2009 pti Created
  ----------------------------------------------------------------------*/

create widget-pool.

/* ***************************  Definitions  ************************** */

/* Buildnr, temp-tables and forward defs */
{ DataDigger.i }
{ resizable_dict.i } /* thanks to Sebastien Lacroix */

/* Constants for the query counter */
&GLOBAL-DEFINE numDigits   6
&GLOBAL-DEFINE digitWidth  6
&GLOBAL-DEFINE digitHeight 15
&GLOBAL-DEFINE marginHor   3
&GLOBAL-DEFINE marginVer   3

/* TT for the generic timer OCX */
DEFINE TEMP-TABLE ttTimer NO-UNDO RCODE-INFORMATION
  FIELD cProc  AS CHARACTER
  FIELD iTime  AS INTEGER
  FIELD tNext  AS DATETIME
  INDEX idxNext IS PRIMARY tNext
  INDEX idxProc cProc.

/* TT for showing record in a new window */
DEFINE TEMP-TABLE ttView NO-UNDO RCODE-INFORMATION
  field iHor   as integer  
  field iVer   as integer  
  field cValue as character format 'x(20)'
  .

/* TT to save widths of columns for ttView */
DEFINE TEMP-TABLE ttColumnWidth NO-UNDO RCODE-INFORMATION
  field iHor   as integer
  field iWidth as integer
  . 

/* TT for fonts, used in checkFonts */
DEFINE TEMP-TABLE ttFont NO-UNDO RCODE-INFORMATION
  field iFontNr   as integer
  field cFontName as character
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
DEFINE VARIABLE giDataOddRowColor          AS INTEGER NO-UNDO EXTENT 2.
DEFINE VARIABLE giDataEvenRowColor         AS INTEGER NO-UNDO EXTENT 2.
DEFINE VARIABLE giDefaultFont              AS INTEGER     NO-UNDO. 
DEFINE VARIABLE giFixedFont                AS INTEGER     NO-UNDO. 
DEFINE VARIABLE giMaxColumns               AS INTEGER     NO-UNDO. 
DEFINE VARIABLE giMaxExtent                AS INTEGER     NO-UNDO. 
DEFINE VARIABLE giMaxFilterHistory         AS INTEGER     NO-UNDO. 
DEFINE VARIABLE ghNewDigit                 AS HANDLE      NO-UNDO EXTENT {&numDigits}.
DEFINE VARIABLE ghOldDigit                 AS HANDLE      NO-UNDO EXTENT {&numDigits}.
DEFINE VARIABLE glDebugMode                AS LOGICAL     NO-UNDO INITIAL FALSE.
DEFINE VARIABLE giLastDataColumnX          AS INTEGER     NO-UNDO.
DEFINE VARIABLE glShowFavourites           AS LOGICAL     NO-UNDO. /* show table list of favourite tables */

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
&Scoped-define FIELDS-IN-QUERY-brFields ttField.lShow ttField.iOrder ttField.cFieldName (if ttField.iExtent > 0 then substitute('&1[&2]', ttField.cDataType, ttField.iExtent) else ttField.cDataType ) @ ttField.cDataType ttField.cFormat ttField.cLabel /* Extra fields as per v19 */ ttField.cInitial ttField.cColLabel ttField.lMandatory ttField.iExtent ttField.iDecimals ttField.iFieldRpos ttField.cValExp ttField.cValMsg ttField.cHelp ttField.cDesc ttField.cViewAs   
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
&Scoped-Define ENABLED-OBJECTS rctQuery btnHelp rctEdit rcCounter ~
btnClearFieldFilter btnFieldFilter fiIndexNameFilter fiFlagsFilter ~
fiFieldsFilter btnClearIndexFilter btnIndexFilter tgSelAll tgDebugMode ~
brFields btnMoveTop brIndexes fiTableFilter cbDatabaseFilter ~
btnClearTableFilter btnTableFilter btnMoveUp brTables btnReset btnMoveDown ~
btnMoveBottom fiTableDesc btnWhere btnClear btnPrevQuery btnQueries ~
btnNextQuery btnClipboard ficWhere btnDataDigger btnTools btnTabFields ~
btnTabIndexes btnTableView btnResizeVer btnClone btnDump btnView btnAdd ~
btnDelete btnEdit 
&Scoped-Define DISPLAYED-OBJECTS fiIndexNameFilter fiFlagsFilter ~
fiFieldsFilter tgSelAll fiTableFilter cbDatabaseFilter fiTableDesc ficWhere 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */
&Scoped-define List-1 btnAnd rctQueryButtons cbAndOr cbFields cbOperator ~
ficValue btnInsert btnBegins btnBracket btnContains btnEq btnGT btnLT ~
btnMatches btnNE btnOr btnQt btnToday 
&Scoped-define List-2 rcFieldFilter btnClearFieldFilter btnFieldFilter ~
tgSelAll brFields btnMoveTop btnMoveUp btnReset btnMoveDown btnMoveBottom 
&Scoped-define List-3 rcIndexFilter fiIndexNameFilter fiFlagsFilter ~
fiFieldsFilter btnClearIndexFilter btnIndexFilter brIndexes 

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
  ( phMenu    AS handle   
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

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getFieldList C-Win 
FUNCTION getFieldList returns character
  ( pcSortBy as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getQueryFromFields C-Win 
FUNCTION getQueryFromFields returns character
  ( input pcFieldList as character ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getSelectedFields C-Win 
FUNCTION getSelectedFields returns character
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getSelectedText C-Win 
FUNCTION getSelectedText RETURNS CHARACTER
  ( INPUT hWidget AS handle )  FORWARD.

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
FUNCTION setCurrentTable returns logical
  ( pcTableName as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setDebugMode C-Win 
FUNCTION setDebugMode returns logical
  ( plDebugMode as logical )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setNumRecords C-Win 
FUNCTION setNumRecords returns logical
  ( input piNumRecords    as integer
  , input plCountComplete as logical 
  , input piQueryMSec     as integer)  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setQuery C-Win 
FUNCTION setQuery RETURNS LOGICAL
  ( piPointerChange as integer )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setQueryEditor C-Win 
FUNCTION setQueryEditor RETURNS LOGICAL
  ( pcQueryEditorState as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setUpdatePanel C-Win 
FUNCTION setUpdatePanel RETURNS LOGICAL
  ( input pcMode as character )  FORWARD.

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
       MENU-ITEM m_Toggle_as_favourite LABEL "Add to &Favourites"
       MENU-ITEM m_Show_hidden_tables LABEL "Show &hidden tables"
              TOGGLE-BOX
       MENU-ITEM m_Dump_table_DF LABEL "&Dump table DF"
       RULE.

DEFINE MENU POPUP-MENU-btnHelp 
       MENU-ITEM mShowHelp      LABEL "Show &Help"    
       MENU-ITEM mShowIntroduction LABEL "Show &Introduction"
       MENU-ITEM mShowWhatsNew  LABEL "Show What's &New"
       MENU-ITEM mDataDiggerWebsite LABEL "DataDigger &Website".

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

DEFINE BUTTON btnDataFilter 
     LABEL "Y" 
     SIZE-PIXELS 20 BY 21 TOOLTIP "filter the data #(RETURN)".

DEFINE VARIABLE fiNumResults AS CHARACTER FORMAT "X(256)":U 
      VIEW-AS TEXT 
     SIZE-PIXELS 70 BY 13 TOOLTIP "nr of results of the query, double click to fetch all records" NO-UNDO.

DEFINE VARIABLE fiNumSelected AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 35 BY 13 TOOLTIP "nr of selected records"
     FGCOLOR 7  NO-UNDO.

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

DEFINE BUTTON btnDataDigger  NO-FOCUS
     LABEL "DD" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "start a new instance of DataDigger".

DEFINE BUTTON btnDelete  NO-FOCUS FLAT-BUTTON
     LABEL "Delete" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "delete the selected records #(DEL)".

DEFINE BUTTON btnDump  NO-FOCUS FLAT-BUTTON
     LABEL "Du&mp" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "dump all data #(ALT-M)".

DEFINE BUTTON btnEdit  NO-FOCUS FLAT-BUTTON
     LABEL "&Edit" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "edit the selected records #(ALT-E)".

DEFINE BUTTON btnFieldFilter 
     LABEL "Y" 
     CONTEXT-HELP-ID 280
     SIZE-PIXELS 20 BY 21 TOOLTIP "filter the fields #(RETURN)".

DEFINE BUTTON btnHelp  NO-FOCUS
     LABEL "Help" 
     SIZE-PIXELS 44 BY 30 TOOLTIP "get help about the DataDigger window".

DEFINE BUTTON btnIndexFilter 
     LABEL "Y" 
     CONTEXT-HELP-ID 960
     SIZE-PIXELS 20 BY 21 TOOLTIP "filter the indexes #(RETURN)".

DEFINE BUTTON btnLoad  NO-FOCUS FLAT-BUTTON
     LABEL "&Load" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "load data #(ALT-L)".

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

DEFINE BUTTON btnNextQuery 
     LABEL ">" 
     SIZE-PIXELS 12 BY 23 TOOLTIP "next query #(PAGE-UP)".

DEFINE BUTTON btnPrevQuery 
     LABEL "<" 
     SIZE-PIXELS 12 BY 23 TOOLTIP "previous query #(PAGE-DOWN)".

DEFINE BUTTON btnQueries 
     LABEL "&Q" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "show previous queries on this table #(ALT-Q)".

DEFINE BUTTON btnReset 
     LABEL "R" 
     SIZE-PIXELS 23 BY 23 TOOLTIP "reset default ordering #(CTRL-SHIFT-HOME)".

DEFINE BUTTON btnResizeVer  NO-FOCUS FLAT-BUTTON
     LABEL "||||||||||||||||||||||||||" 
     SIZE 156 BY .24 TOOLTIP "drag me up and down".

DEFINE BUTTON btnTabFields  NO-FOCUS FLAT-BUTTON
     LABEL "Fld" 
     CONTEXT-HELP-ID 270
     SIZE-PIXELS 25 BY 62 TOOLTIP "show fields #(CTRL-1) = jump #(CTRL-TAB) = switch tabs".

DEFINE BUTTON btnTabIndexes  NO-FOCUS FLAT-BUTTON
     LABEL "Idx" 
     CONTEXT-HELP-ID 270
     SIZE-PIXELS 25 BY 62 TOOLTIP "show indexes #(CTRL-2) = jump #(CTRL-TAB) = switch tabs".

DEFINE BUTTON btnTableFilter 
     LABEL "Y" 
     CONTEXT-HELP-ID 950
     SIZE-PIXELS 34 BY 21 TOOLTIP "press arrow-down for extra filter options #(CTRL-DOWN)".

DEFINE BUTTON btnTableView  NO-FOCUS FLAT-BUTTON
     LABEL "T" 
     CONTEXT-HELP-ID 0
     SIZE-PIXELS 20 BY 21.

DEFINE BUTTON btnTools  NO-FOCUS
     LABEL "Tools" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "tools and settings".

DEFINE BUTTON btnView  NO-FOCUS FLAT-BUTTON
     LABEL "&View" 
     SIZE-PIXELS 25 BY 23 TOOLTIP "view selected records  #(ENTER) #right click to set type of view".

DEFINE BUTTON btnViewData 
     LABEL "->" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "execute the query #(CTRL-ENTER)".

DEFINE BUTTON btnWhere 
     LABEL "&Where" 
     SIZE-PIXELS 50 BY 23 TOOLTIP "show expanded query editor  #(CTRL-ALT-W)".

DEFINE VARIABLE cbDatabaseFilter AS CHARACTER FORMAT "X(256)":U 
     CONTEXT-HELP-ID 950
     VIEW-AS COMBO-BOX SORT INNER-LINES 10
     LIST-ITEMS "Item 1" 
     DROP-DOWN-LIST
     SIZE-PIXELS 59 BY 21 TOOLTIP "filter on database" NO-UNDO.

DEFINE VARIABLE ficWhere AS CHARACTER 
     CONTEXT-HELP-ID 110
     VIEW-AS EDITOR NO-WORD-WRAP
     SIZE-PIXELS 585 BY 21 TOOLTIP "query on your table  #(ALT-W)"
     FONT 2 NO-UNDO.

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
     SIZE-PIXELS 200 BY 21 NO-UNDO.

DEFINE VARIABLE fiTableFilter AS CHARACTER FORMAT "X(256)":U INITIAL "Table filter" 
     CONTEXT-HELP-ID 950
     VIEW-AS FILL-IN 
     SIZE-PIXELS 70 BY 21 TOOLTIP "filter on table names  #(ALT-T)"
     FGCOLOR 7  NO-UNDO.

DEFINE VARIABLE fiWarning AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 45 BY 21
     BGCOLOR 14 FGCOLOR 12  NO-UNDO.

DEFINE RECTANGLE rcCounter
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 55 BY 20.

DEFINE RECTANGLE rcFieldFilter
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 506 BY 237
     BGCOLOR 12 FGCOLOR 12 .

DEFINE RECTANGLE rcIndexFilter
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 315 BY 201
     BGCOLOR 12 FGCOLOR 12 .

DEFINE RECTANGLE rcTableFilter
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 226 BY 205
     BGCOLOR 12 .

DEFINE RECTANGLE rctEdit
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 265 BY 35.

DEFINE RECTANGLE rctQuery
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 789 BY 290
     BGCOLOR 18 .

DEFINE VARIABLE tgDebugMode AS LOGICAL INITIAL yes 
     LABEL "dbg" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 40 BY 13 TOOLTIP "debugging mode".

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
     SIZE-PIXELS 144 BY 30 TOOLTIP "about the DataDigger #(CTRL-SHIFT-A)"
     FONT 0.

DEFINE BUTTON btnChangeLog 
     LABEL "&Hlp" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "watskeburt #(ALT-H)".

DEFINE BUTTON btnChangeLog-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Changelog" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "watskeburt #(ALT-H)"
     FONT 0.

DEFINE BUTTON btnConnections 
     LABEL "Co&n" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "connections #(CTRL-SHIFT-N)".

DEFINE BUTTON btnConnections-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Co&nnections" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "connections #(CTRL-SHIFT-N)"
     FONT 0.

DEFINE BUTTON btnDict 
     LABEL "&DD" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "start the Data Dictionary #(CTRL-SHIFT-D)".

DEFINE BUTTON btnDict-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Data &Dictionary" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "start the Data Dictionary #(CTRL-SHIFT-D)"
     FONT 0.

DEFINE BUTTON btnDump-2 
     LABEL "D&mp" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "dump all data #(ALT-M)".

DEFINE BUTTON btnDump-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Du&mp Data" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "dump all data #(ALT-M)"
     FONT 0.

DEFINE BUTTON btnLoad-2 
     LABEL "&Load" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "load data #(CTRL-SHIFT-L)".

DEFINE BUTTON btnLoad-txt  NO-FOCUS FLAT-BUTTON
     LABEL "&Load Data" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "load data #(CTRL-SHIFT-L)"
     FONT 0.

DEFINE BUTTON btnProcEdit 
     LABEL "&Ed" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "start the Procedure Editor #(CTRL-SHIFT-E)".

DEFINE BUTTON btnProcEdit-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Procedure &Editor" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "start the Procedure Editor #(CTRL-SHIFT-E)"
     FONT 0.

DEFINE BUTTON btnQueries-3 
     LABEL "&Q" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "show previous queries on this table #(CTRL-SHIFT-Q)".

DEFINE BUTTON btnQueries-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Manage &Queries" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "show previous queries on this table #(CTRL-SHIFT-Q)"
     FONT 0.

DEFINE BUTTON btnQueryTester 
     LABEL "Q&T" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "start the query tester #(CTRL-Q)".

DEFINE BUTTON btnQueryTester-txt  NO-FOCUS FLAT-BUTTON
     LABEL "Query &Tester" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "start the query tester #(CTRL-SHIFT-T)"
     FONT 0.

DEFINE BUTTON btnSettings 
     LABEL "&Set" 
     SIZE-PIXELS 30 BY 30 TOOLTIP "edit your settings file #(ALT-S)".

DEFINE BUTTON btnSettings-txt  NO-FOCUS FLAT-BUTTON
     LABEL "&Settings" 
     SIZE-PIXELS 144 BY 30 TOOLTIP "edit your settings file #(ALT-S)"
     FONT 0.

DEFINE BUTTON btnAnd  NO-FOCUS
     LABEL "and" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 30 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnBegins  NO-FOCUS
     LABEL "begins" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 60 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnBracket  NO-FOCUS
     LABEL "()" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 30 BY 21 TOOLTIP "insert this text into the where field"
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
     SIZE-PIXELS 60 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnEq  NO-FOCUS
     LABEL "=" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 30 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnGT  NO-FOCUS
     LABEL ">" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 30 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnInsert 
     LABEL "+" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 20 BY 23 TOOLTIP "insert the expression into the where field".

DEFINE BUTTON btnLT  NO-FOCUS
     LABEL "<" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 30 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnMatches  NO-FOCUS
     LABEL "matches" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 60 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnNE  NO-FOCUS
     LABEL "<>" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 30 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnNextQuery-2 
     LABEL ">" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 12 BY 23 TOOLTIP "next query".

DEFINE BUTTON btnOK AUTO-GO DEFAULT 
     LABEL "OK" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON btnOr  NO-FOCUS
     LABEL "or" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 30 BY 21 TOOLTIP "insert this text into the where field".

DEFINE BUTTON btnPrevQuery-2 
     LABEL "<" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 12 BY 23 TOOLTIP "previous query".

DEFINE BUTTON btnQt  NO-FOCUS
     LABEL "~"~"" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 30 BY 21 TOOLTIP "insert this text into the where field"
     FONT 0.

DEFINE BUTTON btnQueries-2 
     LABEL "&Q" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 20 BY 23 TOOLTIP "show previous queries on this table".

DEFINE BUTTON btnToday  NO-FOCUS
     LABEL "today" 
     CONTEXT-HELP-ID 1050
     SIZE-PIXELS 60 BY 21 TOOLTIP "insert this text into the where field".

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
     SIZE-PIXELS 40 BY 21 TOOLTIP "preceding AND or OR for the expression"
     FONT 2 NO-UNDO.

DEFINE VARIABLE cbFields AS CHARACTER FORMAT "X(256)":U 
     CONTEXT-HELP-ID 1050
     VIEW-AS COMBO-BOX INNER-LINES 10
     DROP-DOWN-LIST
     SIZE-PIXELS 210 BY 21 TOOLTIP "field used in the expression"
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
     SIZE-PIXELS 525 BY 170 TOOLTIP "alt-cursor-up / down to view/hide query editor"
     FONT 2 NO-UNDO.

DEFINE VARIABLE ficValue AS CHARACTER FORMAT "X(256)":U 
     CONTEXT-HELP-ID 1050
     VIEW-AS FILL-IN 
     SIZE-PIXELS 210 BY 23 TOOLTIP "the literal value for the expression"
     FONT 2 NO-UNDO.

DEFINE RECTANGLE rctQueryButtons
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 610 BY 180.

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
      ttField.lShow view-as toggle-box 
  ttField.iOrder     
  ttField.cFieldName 
  (if ttField.iExtent > 0 
    then substitute('&1[&2]', ttField.cDataType, ttField.iExtent) 
    else ttField.cDataType ) @ ttField.cDataType  
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

  enable 
  ttField.lShow 
  ttField.cFormat
/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
    WITH NO-ROW-MARKERS SEPARATORS NO-VALIDATE
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 100 BY 11
          &ELSE SIZE-PIXELS 500 BY 231 &ENDIF FIT-LAST-COLUMN TOOLTIP "fields of selected table"
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
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 62 BY 9
          &ELSE SIZE-PIXELS 308 BY 193 &ENDIF FIT-LAST-COLUMN TOOLTIP "indexes of the table"
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
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 44 BY 8
          &ELSE SIZE-PIXELS 220 BY 178 &ENDIF FIT-LAST-COLUMN TOOLTIP "(F) to set/unset as favourite#(CTRL-T) or (CTRL-F) to switch view"
         CONTEXT-HELP-ID 70.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frMain
     btnHelp AT Y 5 X 65 WIDGET-ID 260
     btnClearFieldFilter AT Y 5 X 710 WIDGET-ID 232
     btnFieldFilter AT Y 5 X 731 WIDGET-ID 234
     fiIndexNameFilter AT Y 5 X 785 COLON-ALIGNED NO-LABEL WIDGET-ID 168
     fiFlagsFilter AT Y 5 X 860 COLON-ALIGNED NO-LABEL WIDGET-ID 164
     fiFieldsFilter AT Y 5 X 915 COLON-ALIGNED NO-LABEL WIDGET-ID 166
     btnClearIndexFilter AT Y 5 X 1065 WIDGET-ID 160
     btnIndexFilter AT Y 5 X 1086 WIDGET-ID 162
     tgSelAll AT Y 7 X 259 WIDGET-ID 6
     tgDebugMode AT Y 15 X 120 WIDGET-ID 238
     brFields AT Y 27 X 255 WIDGET-ID 100
     btnMoveTop AT Y 28 X 760 WIDGET-ID 198
     brIndexes AT Y 28 X 799 WIDGET-ID 200
     fiTableFilter AT Y 37 X 5 NO-LABEL
     cbDatabaseFilter AT Y 37 X 66 COLON-ALIGNED NO-LABEL
     btnClearTableFilter AT Y 37 X 172 WIDGET-ID 222
     btnTableFilter AT Y 37 X 192 WIDGET-ID 38
     btnMoveUp AT Y 50 X 760 WIDGET-ID 192
     brTables AT Y 59 X 5 WIDGET-ID 300
     btnReset AT Y 72 X 760 WIDGET-ID 196
     btnMoveDown AT Y 94 X 760 WIDGET-ID 194
     btnMoveBottom AT Y 116 X 760 WIDGET-ID 200
     fiTableDesc AT Y 237 X 5 NO-LABEL WIDGET-ID 90
     btnWhere AT Y 265 X 5 WIDGET-ID 236
     btnViewData AT Y 265 X 651
     btnClear AT Y 265 X 671 WIDGET-ID 30
     btnPrevQuery AT Y 265 X 691 WIDGET-ID 212
     btnQueries AT Y 265 X 703 WIDGET-ID 190
     btnNextQuery AT Y 265 X 723 WIDGET-ID 214
     btnClipboard AT Y 265 X 735 WIDGET-ID 178
     ficWhere AT Y 266 X 60 NO-LABEL
     btnDataDigger AT Y 5 X 5 WIDGET-ID 126
     fiWarning AT Y 620 X 80 COLON-ALIGNED NO-LABEL WIDGET-ID 172
     btnTools AT Y 5 X 35 WIDGET-ID 264
     btnTabFields AT Y 45 X 230 WIDGET-ID 156
     btnTabIndexes AT Y 105 X 230 WIDGET-ID 158
     btnTableView AT Y 238 X 205 WIDGET-ID 288
     btnResizeVer AT ROW 13.38 COL 1.6 WIDGET-ID 274
     btnClone AT Y 565 X 50 WIDGET-ID 276
     btnDump AT Y 565 X 145
     btnLoad AT Y 565 X 195 WIDGET-ID 224
     btnView AT Y 565 X 170 WIDGET-ID 4
     btnAdd AT Y 565 X 25
     btnDelete AT Y 565 X 250
     btnEdit AT Y 565 X 75
     rctQuery AT Y 2 X 2
     rctEdit AT Y 560 X 20
     rcTableFilter AT Y 56 X 2 WIDGET-ID 254
     rcFieldFilter AT Y 24 X 252 WIDGET-ID 256
     rcIndexFilter AT Y 24 X 795 WIDGET-ID 258
     rcCounter AT Y 10 X 165 WIDGET-ID 84
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 0 Y 0
         SIZE-PIXELS 1600 BY 815.

DEFINE FRAME frData
     btnClearDataFilter AT Y 5 X 740 WIDGET-ID 76
     btnDataFilter AT Y 5 X 760 WIDGET-ID 58
     fiNumSelected AT Y 198 X 660 NO-LABEL WIDGET-ID 270
     fiNumResults AT Y 198 X 685 COLON-ALIGNED NO-LABEL WIDGET-ID 210
     rctData AT Y 0 X 0 WIDGET-ID 272
     rctDataFilter AT Y 1 X 12 WIDGET-ID 296
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT COL 1 ROW 15.05
         SIZE 158 BY 10.24 WIDGET-ID 700.

DEFINE FRAME frHint
     edHint AT Y 0 X 35 NO-LABEL WIDGET-ID 2
     btGotIt AT Y 80 X 70 WIDGET-ID 4
     imgArrow AT Y 0 X 0 WIDGET-ID 10
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS TOP-ONLY NO-UNDERLINE THREE-D 
         AT X 1116 Y 80
         SIZE-PIXELS 205 BY 110
         BGCOLOR 14  WIDGET-ID 600.

DEFINE FRAME frWhere
     btnAnd AT Y 98 X 15 WIDGET-ID 22
     cbAndOr AT Y 5 X 35 COLON-ALIGNED WIDGET-ID 10
     cbFields AT Y 5 X 76 COLON-ALIGNED NO-LABEL WIDGET-ID 12
     cbOperator AT Y 5 X 286 COLON-ALIGNED NO-LABEL WIDGET-ID 14
     ficValue AT Y 5 X 371 COLON-ALIGNED NO-LABEL WIDGET-ID 16
     btnInsert AT Y 5 X 595 WIDGET-ID 18
     ficWhere2 AT Y 35 X 86 NO-LABEL WIDGET-ID 130
     btnViewData-2 AT Y 210 X 90 WIDGET-ID 216
     btnClear-2 AT Y 210 X 110 WIDGET-ID 30
     btnPrevQuery-2 AT Y 210 X 130 WIDGET-ID 212
     btnQueries-2 AT Y 210 X 140 WIDGET-ID 190
     btnNextQuery-2 AT Y 210 X 160 WIDGET-ID 214
     btnClipboard-2 AT Y 210 X 170 WIDGET-ID 178
     btnOK AT Y 210 X 460 WIDGET-ID 132
     btnCancel-2 AT Y 210 X 540 WIDGET-ID 134
     btnBegins AT Y 120 X 15 WIDGET-ID 74
     btnBracket AT Y 77 X 15 WIDGET-ID 28
     btnContains AT Y 140 X 15 WIDGET-ID 116
     btnEq AT Y 35 X 15 WIDGET-ID 62
     btnGT AT Y 56 X 45 WIDGET-ID 66
     btnLT AT Y 56 X 15 WIDGET-ID 64
     btnMatches AT Y 161 X 15 WIDGET-ID 114
     btnNE AT Y 35 X 45 WIDGET-ID 68
     btnOr AT Y 98 X 45 WIDGET-ID 24
     btnQt AT Y 77 X 45 WIDGET-ID 72
     btnToday AT Y 182 X 15 WIDGET-ID 122
     rctQueryButtons AT Y 30 X 5 WIDGET-ID 128
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS TOP-ONLY NO-UNDERLINE THREE-D 
         AT X 800 Y 230
         SIZE-PIXELS 625 BY 260
         TITLE "Query Editor"
         DEFAULT-BUTTON btnOK WIDGET-ID 400.

DEFINE FRAME frSettings
     btnSettings AT Y 0 X 0 WIDGET-ID 210
     btnChangeLog-txt AT Y 128 X 30 WIDGET-ID 206
     btnConnections AT Y 0 X 177 WIDGET-ID 212
     btnDict AT Y 32 X 0 WIDGET-ID 224
     btnProcEdit AT Y 32 X 177 WIDGET-ID 228
     btnDump-2 AT Y 64 X 0 WIDGET-ID 216
     btnLoad-2 AT Y 64 X 177 WIDGET-ID 220
     btnQueries-3 AT Y 96 X 0 WIDGET-ID 190
     btnQueryTester AT Y 96 X 177 WIDGET-ID 232
     btnChangeLog AT Y 128 X 0 WIDGET-ID 214
     btnAbout AT Y 128 X 177 WIDGET-ID 196
     btnDump-txt AT Y 64 X 30 WIDGET-ID 218
     btnAbout-txt AT Y 128 X 207 WIDGET-ID 208
     btnDict-txt AT Y 32 X 30 WIDGET-ID 226
     btnConnections-txt AT Y 0 X 207 WIDGET-ID 202
     btnLoad-txt AT Y 64 X 207 WIDGET-ID 222
     btnProcEdit-txt AT Y 32 X 207 WIDGET-ID 230
     btnQueries-txt AT Y 96 X 30 WIDGET-ID 204
     btnQueryTester-txt AT Y 96 X 207 WIDGET-ID 234
     btnSettings-txt AT Y 0 X 30 WIDGET-ID 200
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT COL 62.8 ROW 27.67 SCROLLABLE 
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
         HEIGHT-P           = 705
         WIDTH-P            = 1467
         MAX-HEIGHT-P       = 2079
         MAX-WIDTH-P        = 1920
         VIRTUAL-HEIGHT-P   = 2079
         VIRTUAL-WIDTH-P    = 1920
         RESIZE             = yes
         SCROLL-BARS        = no
         STATUS-AREA        = no
         BGCOLOR            = ?
         FGCOLOR            = ?
         KEEP-FRAME-Z-ORDER = yes
         THREE-D            = yes
         CONTEXT-HELP-FILE  = "datadigger.chm":U
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
       btnDataFilter:HIDDEN IN FRAME frData           = TRUE.

/* SETTINGS FOR FILL-IN fiNumSelected IN FRAME frData
   NO-ENABLE ALIGN-L                                                    */
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
/* BROWSE-TAB brFields tgDebugMode frMain */
/* BROWSE-TAB brIndexes btnMoveTop frMain */
/* BROWSE-TAB brTables btnMoveUp frMain */
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
/* SETTINGS FOR BUTTON btnFieldFilter IN FRAME frMain
   2                                                                    */
ASSIGN 
       btnHelp:POPUP-MENU IN FRAME frMain       = MENU POPUP-MENU-btnHelp:HANDLE.

/* SETTINGS FOR BUTTON btnIndexFilter IN FRAME frMain
   3                                                                    */
/* SETTINGS FOR BUTTON btnLoad IN FRAME frMain
   NO-ENABLE                                                            */
ASSIGN 
       btnLoad:HIDDEN IN FRAME frMain           = TRUE.

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
       ROW             = 30.29
       COLUMN          = 11
       HEIGHT          = 1.43
       WIDTH           = 6
       WIDGET-ID       = 292
       HIDDEN          = yes
       SENSITIVE       = yes.
/* CtrlFrame OCXINFO:CREATE-CONTROL from: {F0B88A90-F5DA-11CF-B545-0020AF6ED35A} type: PSTimer */
      CtrlFrame:MOVE-AFTER(FRAME frSettings:HANDLE).

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
  if search(cDebuggerPath) = ? then run value(cDebuggerPath).  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-F OF C-Win /* DataDigger */
ANYWHERE DO:
  DEFINE BUFFER bFilter FOR ttFilter. 

  RUN setPage(1).

  FIND bFilter WHERE bFilter.cFieldName = "cFieldName" NO-ERROR.
  IF AVAILABLE bFilter THEN APPLY 'entry' TO bFilter.hFilter.

  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-I OF C-Win /* DataDigger */
anywhere DO:
  run setPage(2).
  apply 'entry' to fiIndexNameFilter in frame {&frame-name}.

  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-T OF C-Win /* DataDigger */
anywhere DO:
  apply 'entry' to fiTableFilter in frame {&frame-name}.
  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON ALT-W OF C-Win /* DataDigger */
anywhere DO:
  apply 'entry' to ficWhere in frame {&frame-name}.
  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON CTRL-F OF C-Win /* DataDigger */
ANYWHERE
DO:
  RUN setTableView(TRUE,NO).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON CTRL-T OF C-Win /* DataDigger */
ANYWHERE
DO:
  RUN setTableView(FALSE,NO).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON END-ERROR OF C-Win /* DataDigger */
OR ENDKEY OF {&WINDOW-NAME} anywhere 
DO:
  if frame frHint:visible then
  do:
    apply 'leave' to frame frHint.
    glHintCancelled = TRUE.
    return no-apply.
  end.

  if frame frSettings:visible then
  do:
    apply 'leave' to frame frSettings.
    return no-apply.
  end.

  if glRowEditActive 
    and (   focus:parent = ghDataBrowse 
         or focus:parent = brFields:handle in frame {&frame-name} ) then 
  do:
    glRowEditActive = no.
    apply 'leave' to focus.
    focus:screen-value = focus:private-data.
    focus:parent:refresh().
    return no-apply.
  end.
    
  if gcQueryEditorState = 'visible' then
  do:
    setQueryEditor('Hidden').
    return no-apply.
  end.

  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  {&window-name}:window-state = 2.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON F11 OF C-Win /* DataDigger */
ANYWHERE DO:
  DEFINE VARIABLE cDatabases AS CHARACTER   NO-UNDO.

  DO WITH FRAME frMain:
    
    /* Get all connected databases */
    cDatabases = getDatabaseList().
  
    ASSIGN 
      cbDatabaseFilter:LIST-ITEMS   = ',' + cDatabases
      cbDatabaseFilter:SCREEN-VALUE = gcCurrentDatabase.
  
    /* Get list of all tables of all databases */
    RUN getTables(OUTPUT TABLE ttTable).
    ASSIGN cbDatabaseFilter:list-items = ',' + cDatabases.
  
    APPLY 'choose' to btnTableFilter. 
  END.
  
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

    cWidgets = SUBSTITUTE("&1 &2 : &3,&4 ~n&5"
                         , hWidget:TYPE
                         , hWidget:NAME
                         , hWidget:X
                         , hWidget:Y
                         , cWidgets
                         ).

    hWidget = hWidget:PARENT.
  END. 
  
  MESSAGE 
    cWidgets SKIP(1) iTargetX '/' iTargetY
    VIEW-AS ALERT-BOX INFO BUTTONS OK TITLE ' Debug info '.

  &ENDIF
  
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
or "END-MOVE" of btnResizeVer
DO:
  run endResize.
end. /* window-resized */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON WINDOW-RESTORED OF C-Win /* DataDigger */
DO:
  apply 'entry' to c-Win.
  apply 'entry' to frame {&frame-name}.  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME frWhere
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
  run dropFieldMenu.
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
or " "           of ttField.lShow in browse brFields
or value-changed of ttField.lShow in browse brFields
do:
  define buffer ttField for ttField. 
  define variable cField as character no-undo.

  publish "setUsage" ("hideField"). /* user behaviour */

  do with frame {&frame-name}:

    find ttField where ttField.cFullName = brFields:get-browse-column(3):screen-value no-error.
    ttField.lShow = not ttField.lShow.
    cField = ttField.cFieldName.

    /* This will include all extents */
    if ttField.iExtent > 0 then cField = cField + '*'. 

    brFields:get-browse-column(1):checked = ttField.lShow.

    run showField( input cField, input ttField.lShow).
  end.
end.

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
do:
  define variable lp as memptr  no-undo. 
  define variable X  as integer no-undo.
  define variable Y  as integer no-undo.

  publish "debugMessage" (1, "scroll-notify of brFields").

  set-size( lp ) = 16. 

  run GetCursorPos(input-output lp). 

  /* Show the location of the mouse relative to the frame */
  run ScreenToClient ( input frame {&frame-name}:hwnd
                     , input lp 
                     ).

  x = get-long( lp, 1 ). 
  y = get-long( lp, 5 ). 

  /* Ignore when we clicked on the vertical scrollbar or 
   * above the horizontal to avoid flashing 
   */
  if   self:name = 'brFields' 
    or self:name = 'brIndexes' then
  do:
    if   x > (brFields:x + brFields:width-pixels - 15) 
      or y < (brFields:y + brFields:height-pixels - 15) 
      or y > (brFields:y + brFields:height-pixels) then return.
  end.

  set-size( lp ) = 0. 

  /* scroll-notify detects a mouse action in the scrollbar area of a browse. */
  run resizeFilters(input 0). /* tables  */
  run resizeFilters(input 1). /* fields  */
  run resizeFilters(input 2). /* indexes */

end. /* scroll-notify */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brFields C-Win
ON START-SEARCH OF brFields IN FRAME frMain
DO:
  publish "setUsage" ("sortFields"). /* user behaviour */
  run reopenFieldBrowse(brFields:current-column:name,?).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brIndexes
&Scoped-define SELF-NAME brIndexes
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON DEFAULT-ACTION OF brIndexes IN FRAME frMain
DO:
  define variable cFieldList     as character no-undo. 
  define variable cQuery         as character no-undo. 
  define variable hEditor        as handle    no-undo.
  define variable lOk            as logical   no-undo. 
  define variable cColumnClicked as character no-undo. 

  if not brIndexes:query:get-buffer-handle(1):available then return. 

  publish "setUsage" ("useIndexAsWhere"). /* user behaviour */

  /* Select the row we clicked on */
  run selectClickedRow(brIndexes:handle, output lOk, output cColumnClicked).
  if not lOk then return. 

  /* Create a query expression from all the fields in the index */
  cFieldList = brIndexes:query:get-buffer-handle(1):buffer-field('cFieldList'):buffer-value.
  cQuery = getQueryFromFields(cFieldList).

  /* Give custom code a chance to alter the query */
  publish "customQuery" (input gcCurrentDatabase, input gcCurrentTable, input-output cQuery).

  /* If needed, expand the query editor */
  if logical(getRegistry ("DataDigger", "AutoExpandQueryEditor")) <> no then
    setQueryEditor('visible').

  /* Fill in the query on the screen */
  hEditor = getActiveQueryEditor().
  hEditor:screen-value = formatQueryString(cQuery, gcQueryEditorState = 'visible').

  apply "entry" to hEditor.
  hEditor:cursor-offset = length(entry(1,cQuery,'~n')) + 2.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON MOUSE-MENU-CLICK OF brIndexes IN FRAME frMain
DO:
  define variable hEditor    as handle      no-undo.
  define variable hIndexName as handle      no-undo.
  define variable hFieldType as handle      no-undo.
  define variable cIndex     as character   no-undo. 
  define variable cColumn    as character   no-undo. 
  define variable lOk        as logical     no-undo. 
  define variable iLength    as integer     no-undo. 
  define variable cUseIndex  as character   no-undo. 
  define variable iUseIndex  as integer     no-undo. 
  define variable lUseIndex  as logical     no-undo. 
  define variable cQuery     as character   no-undo. 
  define variable cWord      as character   no-undo. 
  define variable iWord      as integer     no-undo. 

  if not brIndexes:query:get-buffer-handle(1):available then return. 

  publish "setUsage" ("use-index"). /* user behaviour */

  /* Select the row we clicked on */
  run selectClickedRow(brIndexes:handle, output lOk, output cColumn).
  if not lOk then return. 

  hIndexName = brIndexes:query:get-buffer-handle(1):buffer-field('cIndexName'):handle.

  if valid-handle(hIndexName) then 
  do:
    /* If this is a "default" index, ignore it since this is no real index
     * and we cannot add "USE-INDEX default" to a query
     */
    if hIndexName:buffer-value = "default" then return. 

    /* If the query editor is expanded, do actions to that field */
    hEditor = getActiveQueryEditor().

    /* If there already is an existing "USE-INDEX bladibla" then remove it */
    cQuery = "".

    WhereLoop:
    do iWord = 1 to num-entries(hEditor:screen-value," "):
      cWord = entry(iWord,hEditor:screen-value," ").

      /* Remember we have found the USE-INDEX keyword */
      if cWord = "USE-INDEX" then 
      do:
        lUseIndex = true.
        next WhereLoop.
      end.

      /* Skip index name after USE-INDEX */
      if lUseIndex and can-find(ttIndex where ttIndex.cIndexName = cWord) then 
      do: 
        lUseIndex = false. 
        next WhereLoop. 
      end.

      cQuery = cQuery + " " + cWord.
    end.
    
    hEditor:screen-value = cQuery.
    cIndex = substitute("USE-INDEX &1", hIndexName:buffer-value).
    iLength = length(cIndex).

    /* No text selected */
    if hEditor:selection-text = "" then
    do:
      /* If ficQuery only holds the text <empty> then delete that */
      if hEditor:screen-value = '<empty>' then hEditor:screen-value = ''.
      hEditor:screen-value = trim(substitute("&1 &2", hEditor:screen-value, cIndex)).
    end.
    else 
    do:
      hEditor:replace-selection-text(cIndex).
    end.

    /* Give custom code a chance to alter the query */
    cQuery = hEditor:screen-value.
    publish "customQuery" (input gcCurrentDatabase, input gcCurrentTable, input-output cQuery).
    hEditor:screen-value = cQuery.

    apply "entry" to hEditor.
    hEditor:cursor-offset = length(hEditor:screen-value) + 1.
  end.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON OFF-HOME OF brIndexes IN FRAME frMain
DO:
  publish "setUsage" ("offHomeIndexes"). /* user behaviour */

  if not valid-handle(ghLastIndexFilter) then
    ghLastIndexFilter = fiIndexNameFilter:handle.

  setFilterFieldColor(ghLastIndexFilter).
  apply 'entry' to ghLastIndexFilter. 

  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON ROW-DISPLAY OF brIndexes IN FRAME frMain
DO:
  define variable hField as handle.
  define variable iField as integer. 

  do iField = 1 to num-entries(gcIndexBrowseColumnHandles):
    hField = handle(entry(iField,gcIndexBrowseColumnHandles)).

    /* Set color if index is not active */
    hField:fgcolor = (if ttIndex.lIndexActive = false then getColor('IndexInactive:fg') else ?). /* red */
  end.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brIndexes C-Win
ON START-SEARCH OF brIndexes IN FRAME frMain
DO:
  publish "setUsage" ("sortIndexes"). /* user behaviour */
  run reopenIndexBrowse(brIndexes:current-column:name,?).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brTables
&Scoped-define SELF-NAME brTables
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON F OF brTables IN FRAME frMain
OR "f" OF brTables
OR "*" OF brTables
OR "CHOOSE" OF MENU-ITEM m_Toggle_as_favourite IN MENU POPUP-MENU-brTables
DO:
  DEFINE BUFFER bTable FOR ttTable. 
  PUBLISH "setUsage" ("addToFavourites"). /* user behaviour */

  /* Find table and set/unset as fav */
  FIND bTable WHERE bTable.cTableName = gcCurrentTable NO-ERROR.
  bTable.lFavourite = NOT glShowFavourites.

  /* Save setting */
  setRegistry( SUBSTITUTE("DB:&1",gcCurrentDatabase)
             , SUBSTITUTE("&1:Favourite",gcCurrentTable)
             , (IF bTable.lFavourite THEN "TRUE" ELSE ?)
             ).

  /* Show the change in the descripton box */
  fiTableDesc:SCREEN-VALUE = TRIM(STRING(bTable.lFavourite,"Added to/Removed from")) + " favourites".

  /* If we are in the favo-view then refresh the browse to get rid of this table */
  IF glShowFavourites THEN
    RUN reopenTableBrowse(?).
  ELSE 
    APPLY "CURSOR-DOWN" TO BROWSE brTables.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON MOUSE-SELECT-CLICK OF brTables IN FRAME frMain
DO:
  /* When we click on a table in the browse, we don't want 
   * to wait until the timer ocx refreshes; do it instantly.
   */
  DEFINE VARIABLE cOldTable AS CHARACTER NO-UNDO.
  cOldTable = gcCurrentTable.
  APPLY "value-changed" TO SELF.

  /* Cancel the timer */
  RUN setTimer("timedTableChange", 0).

  /* Apply the change immediately */
  IF cOldTable <> gcCurrentTable THEN
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
  publish "setUsage" ("offHomeTables"). /* user behaviour */
  setFilterFieldColor(fiTableFilter:handle).
  apply 'entry' to fiTableFilter.

  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON ROW-DISPLAY OF brTables IN FRAME frMain
do:
  /* Wait for v19 :) 
  if ghTableQuery:current-result-row modulo 2 = 0 then 
  do:
    ttTable.cTableName  :fgcolor in browse brTables = giDataOddRowColor[1]. 
    ttTable.cTableName  :bgcolor in browse brTables = giDataOddRowColor[2].
    ttTable.cDatabase   :fgcolor in browse brTables = giDataOddRowColor[1]. 
    ttTable.cDatabase   :bgcolor in browse brTables = giDataOddRowColor[2].
    ttTable.iNumQueries :fgcolor in browse brTables = giDataOddRowColor[1]. 
    ttTable.iNumQueries :bgcolor in browse brTables = giDataOddRowColor[2].
  end.

  else
  do:
    ttTable.cTableName  :fgcolor in browse brTables = giDataEvenRowColor[1]. 
    ttTable.cTableName  :bgcolor in browse brTables = giDataEvenRowColor[2].
    ttTable.cDatabase   :fgcolor in browse brTables = giDataEvenRowColor[1]. 
    ttTable.cDatabase   :bgcolor in browse brTables = giDataEvenRowColor[2].
    ttTable.iNumQueries :fgcolor in browse brTables = giDataEvenRowColor[1]. 
    ttTable.iNumQueries :bgcolor in browse brTables = giDataEvenRowColor[2].
  end.
  */
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON START-SEARCH OF brTables IN FRAME frMain
DO:
  publish "setUsage" ("sortTables"). /* user behaviour */
  run reopenTableBrowse(brTables:current-column:name).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brTables C-Win
ON VALUE-CHANGED OF brTables IN FRAME frMain
do:
  define variable hBuffer      as handle no-undo. 
  define variable cOldTable    as character no-undo. 
  define variable cOldDatabase as character no-undo. 

  hBuffer      = brTables:query:get-buffer-handle(1).
  cOldTable    = gcCurrentTable.
  cOldDatabase = gcCurrentDatabase.

  if hBuffer:available then
  do:
    gcCurrentTable    = hBuffer::cTableName.
    gcCurrentDatabase = hBuffer::cDatabase.
    fiTableDesc:screen-value = hBuffer::cTableDesc.
    fiTableDesc:tooltip      = hBuffer::cTableDesc.

    btnViewData:sensitive  = true.
    btnClear:sensitive     = true.
    btnPrevQuery:sensitive = true.
    btnNextQuery:sensitive = true.
    btnQueries:sensitive   = true.
    btnClipboard:sensitive = true.
    btnAdd:sensitive       = true.
  end.
  else 
  do:
    /* Make sure the data browse is empty. The easies way is redrawing it */
    IF NUM-DBS > 0 THEN
    DO:
      RUN reopenDataBrowse-create(INPUT gcCurrentDatabase, INPUT gcCurrentTable).
      ghDataBrowse:SENSITIVE = FALSE. 
    END.

    gcCurrentTable    = ''.
    gcCurrentDatabase = ENTRY(1, getDatabaseList() ).
    fiTableDesc:SCREEN-VALUE = "".
    fiTableDesc:TOOLTIP = ''.
    /*SELF:TOOLTIP = ''.*/

    btnViewData:SENSITIVE  = FALSE.
    btnClear:SENSITIVE     = FALSE.
    btnPrevQuery:SENSITIVE = FALSE.
    btnNextQuery:SENSITIVE = FALSE.
    btnQueries:SENSITIVE   = FALSE.
    btnClipboard:SENSITIVE = FALSE.
    btnAdd:SENSITIVE       = FALSE.
  END.

  IF cOldTable <> gcCurrentTable 
    OR cOldDatabase <> gcCurrentDatabase THEN
  DO:
    /* Clear the filters */
    RUN btnClearFieldFilterChoose. 
    RUN btnClearIndexFilterChoose.
    RUN setTimer("timedTableChange", 300).
  END.

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
or "CTRL-SHIFT-A" of c-win anywhere
DO:
  publish "setUsage" ("About"). /* user behaviour */
  hide frame frSettings.
  run value(getProgramDir() + 'dAbout.w') persistent.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAbout C-Win
ON MOUSE-MENU-CLICK OF btnAbout IN FRAME frSettings /* Que */
, btnAbout-txt
DO:
  publish "setUsage" ("showChangelog"). /* user behaviour */
  hide frame frSettings.
  os-command no-wait start value(getProgramDir() + '\DataDigger.txt').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnAdd
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAdd C-Win
ON CHOOSE OF btnAdd IN FRAME frMain /* Add */
or 'insert-mode' of brTables
do:
  publish "setUsage" ("addRecord"). /* user behaviour */
  run btnAddChoose.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME btnAnd
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAnd C-Win
ON CHOOSE OF btnAnd IN FRAME frWhere /* and */
, btnOr, btnEq, btnNe, btnGt, btnLt, btnToday, btnMatches, btnContains, btnBegins
DO:
  publish "setUsage" ("useWherePanel"). /* user behaviour */
  /* No text selected */
  if ficWhere2:selection-text = "" then
    ficWhere2:insert-string(substitute(' &1 ', self:label)).
  else 
    ficWhere2:replace-selection-text(substitute(' &1 ', self:label)).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnBracket
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnBracket C-Win
ON CHOOSE OF btnBracket IN FRAME frWhere /* () */
DO:
  publish "setUsage" ("useWherePanel"). /* user behaviour */
  /* No text selected */
  if ficWhere2:selection-text = "" then
  do:
    ficWhere2:insert-string(substitute(' &1 ', self:label)).
    ficWhere2:cursor-offset = ficWhere2:cursor-offset - 2.
  end.
  else 
    ficWhere2:replace-selection-text(substitute(' ( &1 ) ', ficWhere2:selection-text)).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnCancel-2
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnCancel-2 C-Win
ON CHOOSE OF btnCancel-2 IN FRAME frWhere /* Cancel */
DO:
  publish "setUsage" ("useWherePanel"). /* user behaviour */
  ficWhere2:screen-value in frame frWhere = ficWhere:screen-value in frame frMain. 
  setQueryEditor('Hidden').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnChangeLog
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnChangeLog C-Win
ON CHOOSE OF btnChangeLog IN FRAME frSettings /* Hlp */
, btnChangeLog-txt
or "ALT-H" of c-win anywhere
DO:
  publish "setUsage" ("showChangelog"). /* user behaviour */
  hide frame frSettings.
  os-command no-wait start value(getProgramDir() + '\DataDigger.txt').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnClear
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClear C-Win
ON CHOOSE OF btnClear IN FRAME frMain /* C */
or SHIFT-DEL of ficWhere  in frame frMain
or SHIFT-DEL of ficWhere2 in frame frWhere
or 'CHOOSE' of btnClear-2 in frame frWhere
DO:
  define variable hEditor as handle      no-undo.
  publish "setUsage" ("clearQuery"). /* user behaviour */

  hEditor = getActiveQueryEditor().

  hEditor:screen-value = ''.
  hEditor:bgcolor      = ?. /* default */
  hEditor:fgcolor      = ?. /* default */
  hEditor:tooltip      = ''.

  /* Clear query in ini file */
  setRegistry ( substitute('DB:&1'   , gcCurrentDatabase )
              , substitute('&1:query', gcCurrentTable )
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
  publish "setUsage" ("clearDataFilter"). /* user behaviour */
  run btnClearDataFilterChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnClearFieldFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClearFieldFilter C-Win
ON CHOOSE OF btnClearFieldFilter IN FRAME frMain /* C */
DO:
  publish "setUsage" ("clearFieldFilter"). /* user behaviour */
  run btnClearFieldFilterChoose.
  apply 'choose' to btnFieldFilter.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClearIndexFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClearIndexFilter C-Win
ON CHOOSE OF btnClearIndexFilter IN FRAME frMain /* C */
DO:
  publish "setUsage" ("clearIndexFilter"). /* user behaviour */
  run btnClearIndexFilterChoose.
  apply 'choose' to btnIndexFilter.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClearTableFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClearTableFilter C-Win
ON CHOOSE OF btnClearTableFilter IN FRAME frMain /* C */
DO:
  publish "setUsage" ("clearTableFilter"). /* user behaviour */

  fiTableFilter     :screen-value = fiTableFilter     :private-data.
  cbDatabaseFilter  :screen-value = ' '.
  
  fiTableFilter     :modified = no.
  cbDatabaseFilter  :modified = no.

  setFilterFieldColor(fiTableFilter     :handle).
  setFilterFieldColor(cbDatabaseFilter  :handle).

  apply 'choose' to btnTableFilter.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClipboard
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClipboard C-Win
ON CHOOSE OF btnClipboard IN FRAME frMain /* Cp */
/* or 'ctrl-c' of ficWhere  in frame frMain  */
/* or 'ctrl-c' of ficWhere2 in frame frWhere */
or 'CHOOSE' of btnClipboard-2 in frame frWhere
do:
  define variable cQuery  as character no-undo. 
  define variable hEditor as handle    no-undo.

  publish "setUsage" ("copyQuery"). /* user behaviour */

  hEditor = getActiveQueryEditor().

  if length(hEditor:selection-text) > 0 then 
    cQuery = hEditor:selection-text.
  else
  IF VALID-HANDLE(ghDataBrowse) THEN
    cQuery = getReadableQuery(ghDataBrowse:QUERY:prepare-string).
  ELSE
  IF hEditor:SCREEN-VALUE = "" THEN
      cQuery = substitute('for each &1.&2 no-lock'
                         , gcCurrentDatabase
                         , gcCurrentTable
                         ).
  ELSE
    cQuery = substitute('for each &1.&2 no-lock &3 &4'
                       , gcCurrentDatabase
                       , gcCurrentTable
                       , (if not hEditor:screen-value begins 'where' then 'where' else '')
                       , trim(hEditor:screen-value)
                       ).

  /* Dont take the tooltip because that is not set until the query is executed */
  cQuery = formatQueryString(cQuery, yes).
  clipboard:value = cQuery.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClone
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClone C-Win
ON CHOOSE OF btnClone IN FRAME frMain /* Clone */
DO:
  publish "setUsage" ("cloneRecord"). /* user behaviour */
  run btnCloneChoose.
END. /* choose of btnDelete */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnConnections
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnConnections C-Win
ON CHOOSE OF btnConnections IN FRAME frSettings /* Con */
, menu-item m_Manage_Connections in menu POPUP-MENU-brTables
, btnConnections-txt
or 'CTRL-SHIFT-N' of c-win anywhere
DO:
  publish "setUsage" ("manageFavourites"). /* user behaviour */
  hide frame frSettings.
  run btnFavouritesChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnDataDigger
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDataDigger C-Win
ON CHOOSE OF btnDataDigger IN FRAME frMain /* DD */
do:
  /* If we're in the middle of the tour, ignore this event */
  if frame frHint:visible then return no-apply. 

  publish "setUsage" ("newInstance"). /* user behaviour */

  /* Set the X and Y a little higher so the new window appears cascaded */
  setRegistry("DataDigger", "Window:x", string(c-win:x + 20) ).                             
  setRegistry("DataDigger", "Window:y", string(c-win:y + 20) ).                             

  run value(getProgramDir() + 'wDataDigger.w') persistent.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frData
&Scoped-define SELF-NAME btnDataFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDataFilter C-Win
ON CHOOSE OF btnDataFilter IN FRAME frData /* Y */
do:
  publish "setUsage" ("filterData"). /* user behaviour */
  run reopenDataBrowse('',?).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnDelete
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDelete C-Win
ON CHOOSE OF btnDelete IN FRAME frMain /* Delete */
DO:
  publish "setUsage" ("deleteRecord"). /* user behaviour */
  run btnDeleteChoose.
END. /* choose of btnDelete */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnDict
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDict C-Win
ON CHOOSE OF btnDict IN FRAME frSettings /* DD */
, btnDict-txt
or 'CTRL-SHIFT-D' of c-win anywhere
DO:

  run startTool("Dict").

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDict C-Win
ON MOUSE-MENU-CLICK OF btnDict IN FRAME frSettings /* DD */
DO:
  
  run startTool("Admin").

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDump-2
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDump-2 C-Win
ON CHOOSE OF btnDump-2 IN FRAME frSettings /* Dmp */
, btnDump, btnDump-txt
or 'ALT-M' of c-win anywhere
DO:
  publish "setUsage" ("dumpData"). /* user behaviour */
  hide frame frSettings.
  run btnDumpChoose. 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnEdit C-Win
ON CHOOSE OF btnEdit IN FRAME frMain /* Edit */
do:
  publish "setUsage" ("editRecord"). /* user behaviour */
  run btnEditChoose.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnFieldFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnFieldFilter C-Win
ON CHOOSE OF btnFieldFilter IN FRAME frMain /* Y */
DO:
  publish "setUsage" ("filterFields"). /* user behaviour */
  run reopenFieldBrowse(?,?). /* reopen, while maintaining original sort */
  APPLY "entry" TO ttField.cFieldName IN BROWSE brFields.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnHelp
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnHelp C-Win
ON CHOOSE OF btnHelp IN FRAME frMain /* Help */
OR "CHOOSE" OF MENU-ITEM mShowHelp 
OR "HELP" OF c-win ANYWHERE 
DO:
  DEFINE VARIABLE iMouseX AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iMouseY AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iReturn AS INTEGER NO-UNDO.

  publish "setUsage" ("filterTables"). /* user behaviour */

  IF SELF = btnHelp:HANDLE THEN
  DO:
    RUN getMouseXY ( INPUT btnHelp:HANDLE
                   , OUTPUT iMouseX
                   , OUTPUT iMouseY
                   ).

    /* Make sure we only activate the help when we
     * have really clicked on the left part of the button.
     * When we do APPLY "CHOOSE" to this button, ignore the 
     * tableFilterOptions
     */
    IF isMouseOver(btnHelp:HANDLE) 
      AND iMouseX > 28 THEN 
    DO:
      RUN SendMessageA (btnHelp:HWND, 517, 0, 0, OUTPUT iReturn).
      RETURN NO-APPLY.
    END.
  END. 

  PUBLISH "setUsage" ("help").
  RUN startWinHelp(FOCUS).
  RETURN NO-APPLY.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnIndexFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnIndexFilter C-Win
ON CHOOSE OF btnIndexFilter IN FRAME frMain /* Y */
or 'return' of fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
DO:
  publish "setUsage" ("filterIndexes"). /* user behaviour */
  run reopenIndexBrowse(?,?). /* reopen, while maintaining original sort */
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME btnInsert
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnInsert C-Win
ON CHOOSE OF btnInsert IN FRAME frWhere /* + */
or "return" of /* cbAndOr, cbFields, cbOperator, */ ficValue
DO:
  define buffer ttField for ttField. 
  define variable cField as character no-undo.

  publish "setUsage" ("useWherePanel"). /* user behaviour */

  find ttField where ttField.cFullName = cbFields:screen-value no-error.
  if not available ttField then return. 
  cField = ttField.cFullName.

  if cField = 'RECID' or cField = 'ROWID' 
    then cField = substitute('&1(&2)', cField, gcCurrentTable ).

  ficWhere2:insert-string(left-trim(substitute('&1 &2 &3 &4&5'
                                        , (if cbAndOr:screen-value = ? then '' else cbAndOr:screen-value)
                                        , cField
                                        , cbOperator:screen-value
                                        , if ttField.cDataType = 'character' then quoter(ficValue:screen-value) else ficValue:screen-value
                                        , chr(13)
                                        )
                              )
                         ).

  apply "entry" to cbAndOr.
  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnLoad-2
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnLoad-2 C-Win
ON CHOOSE OF btnLoad-2 IN FRAME frSettings /* Load */
, btnLoad, btnLoad-txt
or 'CTRL-SHIFT-L' of c-win anywhere
DO:
  return.
  /* publish "setUsage" ("loadData"). /* user behaviour */ */
  /* hide frame frSettings.                                */
  /* run loadData.                                         */
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnMoveBottom
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnMoveBottom C-Win
ON CHOOSE OF btnMoveBottom IN FRAME frMain /* Btm */
or 'ctrl-shift-cursor-down' of brFields
DO:
  publish "setUsage" ("changeFieldOrder"). /* user behaviour */
  run moveField('bottom').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnMoveDown
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnMoveDown C-Win
ON CHOOSE OF btnMoveDown IN FRAME frMain /* Dn */
or 'ctrl-cursor-down' of brFields
DO:
  /* Show a hint when user uses this for the first time */
  IF getRegistry("DataDigger:Usage", "changeFieldOrder:numUsed") = ? THEN 
    RUN showHint(brFields:HANDLE,2,"~nYou can also use CTRL-UP / CTRL-DOWN on the field browse to move fields around").

  publish "setUsage" ("changeFieldOrder"). /* user behaviour */
  run moveField('down').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnMoveTop
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnMoveTop C-Win
ON CHOOSE OF btnMoveTop IN FRAME frMain /* Top */
or 'ctrl-shift-cursor-up' of brFields
DO:
  publish "setUsage" ("changeFieldOrder"). /* user behaviour */
  run moveField('top').
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


&Scoped-define SELF-NAME btnNextQuery
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnNextQuery C-Win
ON CHOOSE OF btnNextQuery IN FRAME frMain /* > */
or 'page-up' of ficWhere       in frame frMain 
or 'choose'  of btnNextQuery-2 in frame frWhere
do:
  publish "setUsage" ("navigateQuery"). /* user behaviour */
  setQuery(-1).
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME btnOK
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOK C-Win
ON CHOOSE OF btnOK IN FRAME frWhere /* OK */
DO:
  publish "setUsage" ("useWherePanel"). /* user behaviour */
  setQueryEditor('Hidden').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnPrevQuery
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnPrevQuery C-Win
ON CHOOSE OF btnPrevQuery IN FRAME frMain /* < */
OR 'page-down' of ficWhere       in frame frMain 
or 'choose'    of btnPrevQuery-2 in frame frWhere
do:
  publish "setUsage" ("navigateQuery"). /* user behaviour */
  setQuery(+1).
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnProcEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnProcEdit C-Win
ON CHOOSE OF btnProcEdit IN FRAME frSettings /* Ed */
, btnProcEdit-txt
or "CTRL-SHIFT-E" of c-win anywhere
DO:
  publish "setUsage" ("Editor"). /* user behaviour */

  /* Return if progress version is runtime */
  if progress = "Run-time" then return. 

  /* In read-only mode, return */
  if ReadOnlyDigger then return. 

  run _edit.p.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME btnQt
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQt C-Win
ON CHOOSE OF btnQt IN FRAME frWhere /* "" */
DO:
  publish "setUsage" ("useWherePanel"). /* user behaviour */

  /* No text selected */
  if ficWhere2:selection-text = "" then
  do:
    ficWhere2:insert-string(substitute(' &1 ', self:label)).
    ficWhere2:cursor-offset = ficWhere2:cursor-offset - 2.
  end.
  else 
    ficWhere2:replace-selection-text(substitute('"&1"', ficWhere2:selection-text)).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnQueries
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQueries C-Win
ON CHOOSE OF btnQueries IN FRAME frMain /* Q */
or 'ALT-Q'        of c-win 
or 'CTRL-SHIFT-Q' of c-win 
or 'CTRL-INS'     of ficWhere       in frame frMain
or 'CTRL-INS'     of ficWhere2      in frame frWhere 
or 'CHOOSE'       of btnQueries-2   in frame frWhere 
or 'CHOOSE'       of btnQueries-txt in frame frSettings
DO:
  publish "setUsage" ("selectQuery"). /* user behaviour */
  hide frame frSettings.
  run btnQueriesChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frSettings
&Scoped-define SELF-NAME btnQueryTester
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQueryTester C-Win
ON CHOOSE OF btnQueryTester IN FRAME frSettings /* QT */
, btnQueryTester-txt
or "CTRL-Q" of c-win anywhere
DO:
  publish "setUsage" ("QueryTester"). /* user behaviour */
  hide frame frSettings.
  run value(getProgramDir() + 'query-tester.w') (input-output table ttTestQuery by-reference).
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
or 'ALT-S' of c-win anywhere
DO:
  publish "setUsage" ("Settings"). /* user behaviour */
  hide frame frSettings.
  run btnSettingsChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON CURSOR-DOWN OF btnSettings IN FRAME frSettings /* Set */
, btnSettings, btnConnections, btnDict, btnProcEdit, btnDump-2
, btnLoad-2, btnQueries-3, btnQueryTester, btnChangeLog, btnAbout
DO:
  apply "TAB" to self.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON CURSOR-UP OF btnSettings IN FRAME frSettings /* Set */
, btnConnections, btnDict, btnProcEdit, btnDump-2
, btnLoad-2, btnQueries-3, btnChangeLog, btnAbout
DO:
  apply "SHIFT-TAB" to self.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings C-Win
ON MOUSE-MENU-CLICK OF btnSettings IN FRAME frSettings /* Set */
DO:
  define variable cEnvironment as character   no-undo.

  publish "setUsage" ("EditSettings"). /* user behaviour */
  hide frame frSettings.

  /* Load or create personalized ini file */
  cEnvironment = substitute('&1DataDigger-&2.ini'
                           , getProgramDir()
                           , getUserName() 
                           ).

  /* Start default editor for ini file */
  os-command no-wait start value( cEnvironment ).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME btnTabFields
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabFields C-Win
ON CHOOSE OF btnTabFields IN FRAME frMain /* Fld */
or 'ctrl-1' of frame {&frame-name} anywhere
DO:
  /* Keep track of user behaviour */
  publish "setUsage" ("setPage").

  run setPage(1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTabIndexes
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabIndexes C-Win
ON CHOOSE OF btnTabIndexes IN FRAME frMain /* Idx */
or 'ctrl-2' of frame {&frame-name} anywhere
DO:
  /* Keep track of user behaviour */
  publish "setUsage" ("setPage").

  run setPage(2).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTabIndexes C-Win
ON MOUSE-MENU-CLICK OF btnTabIndexes IN FRAME frMain /* Idx */
DO:
  define variable dRow           as decimal     no-undo.
  define variable iRow           as integer     no-undo.
  define variable cFieldList     as character no-undo. 
  define variable cQuery         as character no-undo. 

  /* Find the primary index */
  find first ttIndex where ttIndex.cIndexFlags matches '*P*' no-error.
  if not available ttIndex then return. 

  /* Create a query expression from all the fields in the index */
  cQuery = getQueryFromFields(ttIndex.cFieldList).

  /* If needed, expand the query editor */
  if logical(getRegistry ("DataDigger", "AutoExpandQueryEditor")) <> no then
    setQueryEditor('visible').

  /* Fill in the query on the screen */
  ficWhere:screen-value = formatQueryString(cQuery, gcQueryEditorState = 'visible').

  apply "entry" to ficWhere.
  ficWhere:cursor-offset = length(entry(1,cQuery,'~n')) + 2.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTableFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTableFilter C-Win
ON CHOOSE OF btnTableFilter IN FRAME frMain /* Y */
or 'return' of fiTableFilter, cbDatabaseFilter
or 'value-changed' of menu-item m_Show_hidden_tables in menu POPUP-MENU-brTables
DO:
  define variable iMouseX as integer no-undo. 
  define variable iMouseY as integer no-undo. 

  publish "setUsage" ("filterTables"). /* user behaviour */

  if self = btnTableFilter:handle then
  do:
    run getMouseXY ( input btnTableFilter:handle
                   , output iMouseX
                   , output iMouseY
                   ).
    /* Make sure we only activate the tableFilterOptions when we
     * have really clicked on the right part of the button.
     * When we do APPLY "CHOOSE" to this button, ignore the 
     * tableFilterOptions
     */
    IF isMouseOver(btnTableFilter:HANDLE) 
      AND iMouseX >= 20 THEN 
    DO:
      RUN setTableFilterOptions.
      RETURN NO-APPLY.
    END.
  END. 

  RUN reopenTableBrowse(?).
  APPLY 'value-changed' TO brTables.
  APPLY 'entry' TO brTables.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTableView
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTableView C-Win
ON CHOOSE OF btnTableView IN FRAME frMain /* T */
DO:
  RUN setTableView(NOT glShowFavourites,NO).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnTools
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTools C-Win
ON CHOOSE OF btnTools IN FRAME frMain /* Tools */
DO:
  PUBLISH "setUsage" ("showTools"). /* user behaviour */

  IF FRAME frSettings:VISIBLE THEN
  DO:
    RUN hideSettingsFrame.
    RETURN NO-APPLY.
  END.

  /* Show settings frame */
  FRAME frSettings:X = 35.
  FRAME frSettings:Y = 35.
  VIEW FRAME frSettings.

  /* Try to hide it every 2 seconds */
  RUN setTimer("hideSettingsFrame",3000).

  APPLY 'entry' TO btnSettings IN FRAME frSettings.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnView
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnView C-Win
ON CHOOSE OF btnView IN FRAME frMain /* View */
DO:
  publish "setUsage" ("viewData"). /* user behaviour */
  run btnViewChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnViewData
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnViewData C-Win
ON CHOOSE OF btnViewData IN FRAME frMain /* -> */
OR 'ctrl-j' OF cbAndOr, cbFields, cbOperator, ficValue, ficWhere, fiTableFilter, brTables, brFields
OR mouse-select-dblclick, RETURN OF brTables
OR 'ctrl-j' OF ficWhere2 IN FRAME frWhere
OR 'F2' OF ficWhere 
OR 'F2' OF ficWhere2 IN FRAME frWhere
OR 'CTRL-J' OF ttField.cFormat IN BROWSE brFields
OR 'CHOOSE' OF btnViewData-2 IN FRAME frWhere
DO:
  PUBLISH "setUsage" ("viewData"). /* user behaviour */

  /* Only proceed if the button is sensitive */
  IF NOT btnViewData:SENSITIVE THEN RETURN NO-APPLY.

  /* question is: how realistic it the below? 
   * Some tests show that its next to impossible to achieve this. So leave it out first 
   */
/*   /* Make sure the table browse is up to date. Because we use a slight delay */
/*    * to show the fields of the table, there is a theoretical chance the user */
/*    * points to a new table and starts the query within 200 msec.             */
/*    */                                                                        */
/*   APPLY "VALUE-CHANGED" TO brTables IN FRAME frMain.                         */
/*   RUN setTableContext(input gcCurrentTable).                                 */

  /* Open the query */
  RUN reopenDataBrowse('',?).

  IF VALID-HANDLE(ghDataBrowse) THEN
    APPLY 'entry' TO ghDataBrowse. 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnWhere
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnWhere C-Win
ON CHOOSE OF btnWhere IN FRAME frMain /* Where */
or 'CTRL-ALT-W' of c-win anywhere
DO:
  publish "setUsage" ("switchQueryEditor"). /* user behaviour */

  case gcQueryEditorState:
    when 'visible' then 
    do: 
      setQueryEditor('hidden').
      apply 'entry' to ficWhere in frame frMain.
    end.

    when 'hidden'  then 
    do:
      setQueryEditor('visible').
      apply 'entry' to ficWhere2 in frame frWhere.
    end.

  end case.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME cbAndOr
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbAndOr C-Win
ON RETURN OF cbAndOr IN FRAME frWhere /* Where */
DO:
  apply 'entry' to cbFields.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbFields
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbFields C-Win
ON RETURN OF cbFields IN FRAME frWhere
DO:
  apply 'entry' to cbOperator.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbOperator
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbOperator C-Win
ON RETURN OF cbOperator IN FRAME frWhere
DO:
  apply 'entry' to ficValue.
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

  RUN SetTimerInterval.

END PROCEDURE. /* OCX.psTimer.Tick */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME ficValue
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficValue C-Win
ON ENTRY OF ficValue IN FRAME frWhere
DO:
  if self:screen-value = "" then 
    self:screen-value = getLinkInfo(cbFields:screen-value). 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME ficWhere
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficWhere C-Win
ON ALT-CURSOR-DOWN OF ficWhere IN FRAME frMain
DO:
  setQueryEditor('visible').
  apply 'entry' to ficWhere2 in frame frWhere.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficWhere C-Win
ON RETURN OF ficWhere IN FRAME frMain
DO:
  /* If the editor is small, interpret an ENTER as CTRL-ENTER */
  if gcQueryEditorState = 'hidden' then 
  do:
    apply 'choose' to btnViewData.
    return no-apply.
  end. 
  else
    self:insert-string ( '~n' ) .
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frWhere
&Scoped-define SELF-NAME ficWhere2
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficWhere2 C-Win
ON ALT-CURSOR-UP OF ficWhere2 IN FRAME frWhere
DO:
  setQueryEditor('hidden').
  apply 'entry' to ficWhere in frame frMain.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME fiIndexNameFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON ANY-PRINTABLE OF fiIndexNameFilter IN FRAME frMain
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
do:
  setFilterFieldColor(self:handle).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON CURSOR-DOWN OF fiIndexNameFilter IN FRAME frMain
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
DO:
  setFilterFieldColor(SELF:HANDLE).
  ghLastIndexFilter = SELF.
  APPLY 'entry' to brIndexes.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON ENTRY OF fiIndexNameFilter IN FRAME frMain
, fiTableFilter
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
do:
  /* If you enter the field and you have not put in a filter, 
   * clear out the field so you can type something yourself
   */
  if self:screen-value = self:private-data then
    self:screen-value = ''.

  setFilterFieldColor(self:handle).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON LEAVE OF fiIndexNameFilter IN FRAME frMain
, fiTableFilter
, fiIndexNameFilter, fiFlagsFilter, fiFieldsFilter
do:
  if self:screen-value = '' then self:screen-value = self:private-data.
  setFilterFieldColor(self:handle).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiIndexNameFilter C-Win
ON SHIFT-DEL OF fiIndexNameFilter IN FRAME frMain
, fiFlagsFilter, fiFieldsFilter
DO:
  apply 'choose' to btnClearIndexFilter.
  self:screen-value = ''.
  apply 'value-changed' to self.
  apply 'entry' to self.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frData
&Scoped-define SELF-NAME fiNumResults
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiNumResults C-Win
ON MOUSE-SELECT-DBLCLICK OF fiNumResults IN FRAME frData
, fiNumSelected 
DO:
  define variable hQuery  as handle      no-undo.
  define variable hBuffer as handle      no-undo.
  define variable cQuery  as character   no-undo.
  define variable cTable  as character   no-undo.
  define variable iStartTime as integer     no-undo.

  if not valid-handle(ghDataBrowse) then return.

  session:set-wait-state('general'). 
  iStartTime = etime.

  /* Change query to a PRESELECT query to get number of rows */
  cQuery = ghDataBrowse:query:prepare-string.
  entry(1,cQuery,' ') = 'preselect'.

  /* Create buffer for the table we selected in the data browse */
  cTable = ghDataBrowse:query:get-buffer-handle(1):dbname + '.' + ghDataBrowse:query:get-buffer-handle(1):name.

  create query hQuery.
  create buffer hBuffer for table ghDataBrowse:query:get-buffer-handle(1):dbname + '.' + ghDataBrowse:query:get-buffer-handle(1):name no-error.

  hQuery:set-buffers(hBuffer).
  hQuery:query-prepare(cQuery).
  hQuery:query-open.

  session:set-wait-state('').

  /* Show nr of records */
  setNumRecords(hQuery:num-results, yes, etime - iStartTime).

  hQuery:query-close.

  delete object hQuery.
  delete object hBuffer.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME frMain
&Scoped-define SELF-NAME fiTableFilter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiTableFilter C-Win
ON CTRL-CURSOR-DOWN OF fiTableFilter IN FRAME frMain
or "CTRL-CURSOR-DOWN" of fiTableFilter
or "CTRL-CURSOR-DOWN" of cbDatabaseFilter
or "CTRL-CURSOR-DOWN" of brTables
DO:
  run setTableFilterOptions.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiTableFilter C-Win
ON CURSOR-DOWN OF fiTableFilter IN FRAME frMain
DO:
  setFilterFieldColor(self:handle).
  apply 'entry' to brTables.
  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiTableFilter C-Win
ON SHIFT-DEL OF fiTableFilter IN FRAME frMain
, cbDatabaseFilter, cbDatabaseFilter
DO:
  apply 'choose' to btnClearIndexFilter.
  self:screen-value = ''.
  apply 'value-changed' to self.
  apply 'entry' to self.
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


&Scoped-define SELF-NAME mDataDiggerWebsite
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL mDataDiggerWebsite C-Win
ON CHOOSE OF MENU-ITEM mDataDiggerWebsite /* DataDigger Website */
DO:
  OS-COMMAND NO-WAIT START VALUE("http://datadigger.wordpress.com").
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME mShowIntroduction
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL mShowIntroduction C-Win
ON CHOOSE OF MENU-ITEM mShowIntroduction /* Show Introduction */
DO:
  RUN showTour.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME mShowWhatsNew
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL mShowWhatsNew C-Win
ON CHOOSE OF MENU-ITEM mShowWhatsNew /* Show What's New */
DO:
  RUN showNewFeatures.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_Disconnect
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_Disconnect C-Win
ON CHOOSE OF MENU-ITEM m_Disconnect /* Disconnect */
OR "-",DELETE-CHARACTER OF cbDatabaseFilter
DO:
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

  /* Remove all tables of this db from the "tables" table */
  FOR EACH ttTable WHERE ttTable.cDatabase = cCurrentDb:
    DELETE ttTable. 
  END.
  RUN reopenTableBrowse(?).

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
  APPLY "choose" TO btnTableFilter. 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_Dump_table_DF
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_Dump_table_DF C-Win
ON CHOOSE OF MENU-ITEM m_Dump_table_DF /* Dump table DF */
DO:

  do with frame frMain:
  
    create alias dictdb for database value( gcCurrentDatabase ).

    run value(getProgramDir() + 'dDumpDf.w')
     ( input-output gcCurrentTable
     , input substitute("x=&1,y=&2", brTables:x + 10, brTables:y + 50)
     ).
  end.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_Quick_Connect
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_Quick_Connect C-Win
ON CHOOSE OF MENU-ITEM m_Quick_Connect /* Quick Connect */
or '+', insert-mode of cbDatabaseFilter 
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
  RUN getTables(OUTPUT TABLE ttTable).

  /* Get all connected databases */
  cDatabases = getDatabaseList().
  cbDatabaseFilter:LIST-ITEMS = ',' + cDatabases.
  cbDatabaseFilter:SCREEN-VALUE = cLogicalName.
  APPLY 'value-changed' TO cbDatabaseFilter.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_Show_hidden_tables
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_Show_hidden_tables C-Win
ON VALUE-CHANGED OF MENU-ITEM m_Show_hidden_tables /* Show hidden tables */
DO:
  define variable cSetting as character   no-undo.

  cSetting = string(menu-item m_Show_hidden_tables:checked in menu POPUP-MENU-brTables).
  setRegistry('DataDigger', 'ShowHiddenTables', cSetting ).
  
  RUN setTimer("timedTableFilter", 300).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_View_as_Excel
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_View_as_Excel C-Win
ON CHOOSE OF MENU-ITEM m_View_as_Excel /* View as Excel */
DO:
  run setViewType('XLS').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_View_as_HTML
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_View_as_HTML C-Win
ON CHOOSE OF MENU-ITEM m_View_as_HTML /* View as HTML */
DO:
  run setViewType('HTML').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME m_View_as_text
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL m_View_as_text C-Win
ON CHOOSE OF MENU-ITEM m_View_as_text /* View as TEXT */
DO:
  run setViewType('TXT').
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tgDebugMode
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgDebugMode C-Win
ON VALUE-CHANGED OF tgDebugMode IN FRAME frMain /* dbg */
DO:

  setDebugMode(self:checked).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tgSelAll
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgSelAll C-Win
ON VALUE-CHANGED OF tgSelAll IN FRAME frMain
DO:
  DEFINE VARIABLE cFieldList AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE hQuery AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hBuffer AS HANDLE NO-UNDO. 

  SESSION:SET-WAIT-STATE('general').
  setWindowFreeze(YES).

  DEFINE BUFFER bField FOR ttField.
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

      hBuffer::lShow = SELF:CHECKED.
      /*brFields:GET-BROWSE-COLUMN(1):CHECKED = bField.lShow.*/

      FOR EACH bColumn WHERE bColumn.cFieldName = hBuffer::cFieldName:
        cFieldList = cFieldList + "," + bColumn.cFullName.
      END.
    END.

    RUN showField(INPUT cFieldList, INPUT SELF:CHECKED). 
    RUN reopenFieldBrowse(?,?).

    hQuery:QUERY-CLOSE.
    DELETE OBJECT hQuery.
    DELETE OBJECT hBuffer. 

    setWindowFreeze(no).
    SESSION:SET-WAIT-STATE('').

    APPLY "entry" TO ttField.cFieldName IN BROWSE brFields.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brFields
&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK C-Win 


SESSION:DEBUG-ALERT = YES.

/* ***************************  Main Block  *************************** */
PUBLISH "timerCommand" ("start", "startDiggerLib").
RUN startDiggerLib.
PUBLISH "timerCommand" ("stop", "startDiggerLib").

PUBLISH "timerCommand" ("start", "PostDiggerLib").

/* More than one DataDigger window can be open. The 
 * startup procedure can demand that all windows are 
 * closed. For example, when an update is done 
 */
SUBSCRIBE TO 'DataDiggerClose' ANYWHERE.

/* If we started the DataDigger from within DWP and we stop 
 * DWP, then exit the DataDigger as well. 
 * Requested by Jeroen Stam from NetSetup 30-3-2012
 */
SUBSCRIBE to "dwp_stop" ANYWHERE RUN-PROCEDURE 'DataDiggerClose'.
  
/* Save queries in a temp-table for the query tester */
SUBSCRIBE TO "query" ANYWHERE RUN-PROCEDURE "processQuery".

/* Avoid drawing */
/* {&WINDOW-NAME}:Y              = -1000. /* out of sight */ */
{&WINDOW-NAME}:VISIBLE           = YES. /* otherwise lockwindow complains */
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

setWindowFreeze(yes).

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME} 
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
do:
  define variable cSetting as character no-undo. 

  /* If we click this in the middle of the tour, ignore it */
  if frame frHint:visible then return no-apply. 

  /* Cancel all running timer events */
  chCtrlFrame:pstimer:ENABLED = FALSE.
  
  /* Save size and position of the window */
  run saveWindow.

  cSetting = cbDatabaseFilter:screen-value in frame {&frame-name}.
  if cSetting = ? then cSetting = '<empty>'.
  setRegistry("DataDigger", "Database", cSetting ).

  /* Notify launcher that the window closes */
  publish 'DataDigger'(-1).
  
  run disable_UI.
end. /* CLOSE OF THIS-PROCEDURE  */


on 'menu-drop' of menu POPUP-MENU-brTables
do:
  publish "setUsage" ("TableMenu"). /* user behaviour */
  run setConnectionMenu.
end.


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
      fiWarning:y            = self:y + brFields:y - 2.
      fiWarning:visible      = yes.
      fiWarning:screen-value = substitute('Original format: &1', cOrgValue).
      fiWarning:width        = length(fiWarning:screen-value) + 1.
      fiWarning:x            = self:x - fiWarning:width-pixels + brFields:x - 10.
    end.

    /* Set a flag for reopenDataBrowse to indicate that the browse must be rebuilt */
    glFormatChanged = true.

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


ON CTRL-TAB OF C-Win anywhere /* DataDigger */
DO:
  publish "setUsage" ("setPage"). /* user behaviour */
  case giCurrentPage:
    when 1 then run setPage(2).
    when 2 then run setPage(1).
  end case.
END. /* CTRL-TAB OF C-Win anywhere */


ON "CTRL-ALT-D" OF C-Win anywhere /* Debugger */ 
DO:
  publish "setUsage" ("debugger"). /* user behaviour */
  run value(getProgramDir() + "wDebugger.w") persistent.
END.


/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  /* Notify launcher that the window started */
  publish 'DataDigger'(+1).

  run enable_UI.
  publish "timerCommand" ("stop", "PostDiggerLib").

  publish "timerCommand" ("start", "Initialize").
  run initializeObjects.
  publish "timerCommand" ("stop", "Initialize").

  setWindowFreeze(no).

  /* Clear wait-message */
  DELETE WIDGET winWait.

  publish "timerCommand" ("stop", "Startup").
  run startSession.

/*  run showTour. */
/*  run showNewFeatures. */

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
/*------------------------------------------------------------------------
  Name         : btnAddChoose
  Description  : Add new record
  ---------------------------------------------------------------------- 
  05-04-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable lRecordsUpdated as logical no-undo. 
  define variable rNewRecord as rowid no-undo. 

  define buffer ttField for ttField. 

  /* Keep track of user behaviour */
  publish "setUsage" ("AddRecord").

  /* In read-only mode, return */
  if ReadOnlyDigger then return. 

  run value(getProgramDir() + 'wEdit.w')
    ( input 'Add'
    , input ghDataBrowse
    , input gcCurrentDatabase
    , input gcCurrentTable
    , input table ttField  /* do not use by-reference */
    , input table ttColumn /* do not use by-reference */
    , output lRecordsUpdated
    , output rNewRecord
    ).

  if lRecordsUpdated = true then 
  do:
    run reopenDataBrowse('',?).
    ghDataBrowse:query:reposition-to-rowid(rNewRecord).
  end.

end procedure. /* btnAddChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnClearDataFilterChoose C-Win 
PROCEDURE btnClearDataFilterChoose :
/*------------------------------------------------------------------------
  Name         : btnClearDataFilterChoose
  Description  : Clear filters and reopen data browse
  ---------------------------------------------------------------------- 
  04-02-2011 pti Created
  ----------------------------------------------------------------------*/

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

  RUN reopenDataBrowse('',?).

END PROCEDURE. /* btnClearDataFilterChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnClearFieldFilterChoose C-Win 
PROCEDURE btnClearFieldFilterChoose :
/*------------------------------------------------------------------------
  Name         : btnClearFieldFilterChoose
  Description  : Clear field filters 
  ---------------------------------------------------------------------- 
  26-08-2011 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE hFilter AS HANDLE  NO-UNDO.
  DEFINE VARIABLE iFilter AS INTEGER NO-UNDO.

  do iFilter = 1 to num-entries(gcFieldFilterHandles):
    hFilter = widget-handle(entry(iFilter, gcFieldFilterHandles)) no-error.
    if valid-handle(hFilter) and hFilter:type <> "Toggle-Box" then
    do:
      assign
        hFilter:modified     = no
        hFilter:screen-value = hFilter:private-data.

      setFilterFieldColor(hFilter).
    end.
  end.

END PROCEDURE. /* btnClearFieldFilterChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnClearIndexFilterChoose C-Win 
PROCEDURE btnClearIndexFilterChoose :
/*------------------------------------------------------------------------
  Name         : btnClearIndexFilterChoose
  Description  : Clear index filters 
  ---------------------------------------------------------------------- 
  26-08-2011 pti Created
  ----------------------------------------------------------------------*/

  do with frame frMain:
    fiIndexNameFilter:screen-value = fiIndexNameFilter:private-data. 
    fiFlagsFilter    :screen-value = fiFlagsFilter    :private-data. 
    fiFieldsFilter   :screen-value = fiFieldsFilter   :private-data. 
  
    setFilterFieldColor(fiIndexNameFilter:handle).
    setFilterFieldColor(fiFlagsFilter    :handle).
    setFilterFieldColor(fiFieldsFilter   :handle).
  end. 

END PROCEDURE. /* btnClearIndexFilterChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnCloneChoose C-Win 
PROCEDURE btnCloneChoose :
/*------------------------------------------------------------------------
  Name         : btnCloneChoose
  Description  : Copy the current record and edit it.

  ----------------------------------------------------------------------
  28-02-2012 pti Created
  ----------------------------------------------------------------------*/

  define variable lRecordsUpdated as logical no-undo. 
  define variable rNewRecord      as rowid   no-undo. 
  define buffer ttField for ttField. 

  /* In read-only mode, return */
  if ReadOnlyDigger then return. 

  /* If no data then go back */
  if ghDataBrowse:query:num-results = 0 
    or ghDataBrowse:query:num-results = ? then return. 

  /* If there is no record selected, select the focused one */
  if ghDataBrowse:num-selected-rows = 0 then
    ghDataBrowse:select-focused-row().

  if not ghDataBrowse:query:get-buffer-handle(1):available then
  do:
    run showHelp('RecordGone', '').
    ghDataBrowse:refresh().
    return.
  end.

  run value(getProgramDir() + 'wEdit.w')
    ( input 'Clone'
    , input ghDataBrowse
    , input gcCurrentDatabase
    , input gcCurrentTable
    , input table ttField  /* do not use by-reference ! */
    , input table ttColumn /* do not use by-reference ! */
    , output lRecordsUpdated
    , output rNewRecord
    ).

  if lRecordsUpdated = true then 
  do:
    ghDataBrowse:query:reposition-to-rowid(rNewRecord) no-error.
    ghDataBrowse:select-focused-row().
    run reopenDataBrowse('',?).
  end.

end procedure. /* btnCloneChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnDeleteChoose C-Win 
PROCEDURE btnDeleteChoose :
/*------------------------------------------------------------------------
  Name         : btnDeleteChoose 
  Description  : Delete selected records
  ---------------------------------------------------------------------- 
  18-03-2009 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE iCount          AS INTEGER NO-UNDO.
  DEFINE VARIABLE hBuffer         AS HANDLE  NO-UNDO.
  DEFINE VARIABLE lContinue       AS LOGICAL NO-UNDO.
  DEFINE VARIABLE lDeleted        AS LOGICAL NO-UNDO. 
  DEFINE VARIABLE lError          AS LOGICAL NO-UNDO. 
  DEFINE VARIABLE lEnableTriggers AS LOGICAL NO-UNDO. 

  /* In read-only mode, return */
  IF ReadOnlyDigger THEN RETURN. 

  /* If nothing selected, go back */
  IF ghDataBrowse:NUM-SELECTED-ROWS = 0 THEN RETURN. 

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
      IF hBuffer:AVAILABLE THEN 
      DO:
        RUN deleteRecord( gcCurrentDatabase
                        , gcCurrentTable 
                        , hBuffer:ROWID
                        , lEnableTriggers
                        , OUTPUT lDeleted
                        ).
      END.
      ELSE lDeleted = TRUE.

    END.

    /* 2012-09-14 JEE User interaction outside of transaction; only to next record if record is deleted */
    IF NOT lDeleted THEN lError = TRUE.
    ghDataBrowse:QUERY:GET-NEXT(NO-LOCK).
  END.

  setWindowFreeze(NO).
  SESSION:SET-WAIT-STATE("").

  IF lError THEN
    MESSAGE 'Sorry, could not delete record.' VIEW-AS ALERT-BOX INFO BUTTONS OK.

  RUN reopenDataBrowse(?,?).

END PROCEDURE. /* btnDeleteChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnDumpChoose C-Win 
PROCEDURE btnDumpChoose :
/*------------------------------------------------------------------------
  Name         : btnDumpChoose 
  Description  : Dump selected records
  ---------------------------------------------------------------------- 
  18-03-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define variable cSetting       as character   no-undo.
  define variable cOldDateFormat as character   no-undo.
  define variable cOldNumFormat  as character   no-undo.

  define buffer ttField for ttField. 

  /* Prevent illegal calls */
  if not valid-handle(ghDataBrowse) then return. 

  /* If no data then go back */
  if ghDataBrowse:query:num-results = 0 
    or ghDataBrowse:query:num-results = ? then return. 

  /* If there is no record selected, select the focused one */
  if valid-handle(ghDataBrowse) 
    and ghDataBrowse:num-selected-rows = 0 then
    ghDataBrowse:select-focused-row().

  /* When you start DataDigger from more than 1 environment, chances are
   * that you might start with different date-format settings. The dump 
   * window saves the date of the last dump in the session:date-format 
   * so this needs to be consistent throughout all runs of DataDigger.
   */
  /* 2011-09-14 JEE Same goes for numeric format */
  assign
    cOldDateFormat = session:date-format
    cOldNumFormat  = session:numeric-format
    .

  /* Check Date-format in ini file */
  cSetting = getRegistry('DataDigger', 'DateFormat').

  case cSetting:
    when ? then setRegistry('DataDigger', 'DateFormat', session:date-format).
    when session:date-format then .
    otherwise session:date-format = cSetting.
  end case. 

  run value(getProgramDir() + 'wDump.w')
    ( input ghDataBrowse
    , input getSelectedFields()
    , input table ttField by-reference
    ).

  /* Restore date format */
  assign
    session:date-format     = cOldDateFormat
    session:numeric-format  = cOldNumFormat
    .

END PROCEDURE. /* btnDumpChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnEditChoose C-Win 
PROCEDURE btnEditChoose :
/*------------------------------------------------------------------------
  Name         : btnEditChoose
  Description  : Edit one or more records in a separate window

  ----------------------------------------------------------------------
  14-01-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable lRecordsUpdated as logical no-undo. 
  define variable rNewRecord      as rowid   no-undo. 
  define buffer ttField for ttField. 

  /* If no data then go back */
  if ghDataBrowse:query:num-results = 0 
    or ghDataBrowse:query:num-results = ? then return. 

  /* If there is no record selected, select the focused one */
  if ghDataBrowse:num-selected-rows = 0 then
    ghDataBrowse:select-focused-row().

  if not ghDataBrowse:query:get-buffer-handle(1):available then
  do:
    run showHelp('RecordGone', '').
    ghDataBrowse:refresh().
    return.
  end.

  run value(getProgramDir() + 'wEdit.w')
    ( input 'Edit'
    , input ghDataBrowse
    , input gcCurrentDatabase
    , input gcCurrentTable
    , input table ttField  /* do not use by-reference ! */
    , input table ttColumn /* do not use by-reference ! */
    , output lRecordsUpdated
    , output rNewRecord /* not handled here */
    ).

  if lRecordsUpdated then ghDataBrowse:refresh().

end procedure. /* btnEditChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnFavouritesChoose C-Win 
PROCEDURE btnFavouritesChoose :
/*------------------------------------------------------------------------
  Name         : btnFavouritesChoose
  Description  : Maintenance of database connection settings

  ----------------------------------------------------------------------
  14-01-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable cDummy        as character   no-undo.
  define variable cProgDir      as character   no-undo.
  define variable cDatabases    as character   no-undo.
  define variable cDatabasesOld as character   no-undo.
  define variable cCurrentDb    as character   no-undo. 

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
    RUN getTables(OUTPUT TABLE ttTable).
    ASSIGN cbDatabaseFilter:LIST-ITEMS IN FRAME frMain = ',' + cDatabases.

    APPLY 'choose' TO btnTableFilter.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnQueriesChoose C-Win 
PROCEDURE btnQueriesChoose :
/*------------------------------------------------------------------------
  Name         : btnQueriesChoose
  Description  : Maintenance of database connection settings

  ----------------------------------------------------------------------
  14-01-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable iQuery  as integer no-undo. 
  define variable hEditor as handle  no-undo.

  hEditor = getActiveQueryEditor().

  run value(getProgramDir() + 'dQueries.w')
    ( input gcCurrentDatabase
    , input gcCurrentTable
    , input hEditor:screen-value
    , output iQuery
    ).

  if iQuery = ? then 
    return.
  else 
  do:
    /* Queries might be changed, so reload them */
    run collectQueryInfo
      ( input gcCurrentDatabase
      , input gcCurrentTable 
      ).
    giQueryPointer = iQuery.
    setQuery(0).
  end.

END PROCEDURE. /* btnQueriesChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnSettingsChoose C-Win 
PROCEDURE btnSettingsChoose :
/*------------------------------------------------------------------------
  Name         : btnSettingsChoose
  Description  : Show DataDigger settings window

  ----------------------------------------------------------------------
  14-01-2011 pti Created
  ----------------------------------------------------------------------*/
  
  define variable cEnvironment as character   no-undo.
  define variable lOkClicked   as logical     no-undo.

  /* Load or create personalized ini file */
  cEnvironment = substitute('&1DataDigger-&2.ini'
                           , getProgramDir()
                           , getUserName() 
                           ).

  /* Save window pos & size because the initializeObject will reset 
   * it to its last known position and size. That might be different 
   * from the actual position of the window. The window would flash
   * and move on the screen.
   */
  run saveWindow.

  run value(getProgramDir() + '\wSettings.w') 
     ( input cEnvironment
     , output lOkClicked
     ).

  if lOkClicked then 
  do:
    setWindowFreeze(yes).
    run initializeObjects. 
    run clearRegistryCache.

    gcCurrentTable = ?.
    apply "value-changed" to brTables in frame frMain.
    setWindowFreeze(no).
  end.

END PROCEDURE. /* btnSettingsChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnViewChoose C-Win 
PROCEDURE btnViewChoose :
/*------------------------------------------------------------------------
  Name         : btnViewChoose
  Description  : Show a record in a more readable format in a new window. 

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  27-08-2010 pti Rewritten to show multiple records.
  ----------------------------------------------------------------------*/
  
  define variable cFileType   as character   no-undo.
  define variable hDataBuffer as handle      no-undo.
  define variable iMaxWidth   as integer     no-undo. 
  define variable cDataFormat as character   no-undo.
  define variable iFieldExtent as integer     no-undo.
  define variable iRecord     as integer     no-undo.
  define variable cLineStart  as character   no-undo.
  define variable cLabelEnd   as character   no-undo.
  define variable iRowNr      as integer     no-undo. 
  define variable cColumnSep  as character   no-undo.
  define variable cLineEnd    as character   no-undo.
  define variable cDocStart   as character   no-undo.
  define variable cDocEnd     as character   no-undo.
  define variable cFilename   as character   no-undo.
  define variable cLabelStart as character   no-undo.
  define variable cDataStart  as character   no-undo extent 2.
  define variable cDataEnd    as character   no-undo.
  define variable iLineNr     as integer     no-undo.
  
  define buffer bView for ttView. 
  define buffer ttField for ttField. 

  /* If there is no record selected, select the focused one */
  if ghDataBrowse:num-selected-rows = 0 then
    ghDataBrowse:select-focused-row().

  if not ghDataBrowse:query:get-buffer-handle(1):available then
  do:
    run showHelp('RecordGone', '').
    ghDataBrowse:refresh().
    return.
  end.
  
  /* What type do we want? */
  cFileType = getRegistry('DataDigger', 'ViewType').

  /* Cleanup */
  empty temp-table ttView. 
  empty temp-table ttColumnWidth.

  /* Get data */
  if not valid-handle(ghDataBrowse) then return.
  hDataBuffer = ghDataBrowse:query:get-buffer-handle(1).
  if not hDataBuffer:available then return.

  collectLoop:
  for each ttField 
    where ttField.lShow      = true
/*      and ttField.lDataField = true */
      and ttField.cFieldName <> 'RECID'
      and ttField.cFieldName <> 'ROWID' 
    break by ttField.iOrder
          by ttField.iExtent:

    /* Move it one down */
    iRowNr = iRowNr + 1.

    /* Label is the first column, so HOR=0 */
    create ttView.
    assign ttView.iHor   = 0
           ttView.iVer   = iRowNr
           ttView.cValue = ttField.cFullName
           .
  
    /* Walk thru all selected records */
    do iRecord = 1 to ghDataBrowse:num-selected-rows:
      ghDataBrowse:fetch-selected-row(iRecord).

      create ttView.
      assign ttView.iHor   = iRecord
             ttView.iVer   = iRowNr 
             ttView.cValue = trim(string(hDataBuffer:buffer-field(ttField.cFieldName):buffer-value(ttField.iExtent), ttField.cFormat ))
             .
    end. /* iRecord */
  end. /* for each ttField */

  /* Calculate maximum width per column */
  do iRecord = 0 to ghDataBrowse:num-selected-rows:

    /* Find out maximum width of all elements in this col */
    iMaxWidth = 1.
    for each ttView where ttView.iHor = iRecord:
      /* Correct cValue for unknown values */
      ttView.cValue = substitute('&1',ttView.cValue).
      iMaxWidth = maximum(iMaxWidth, length(ttView.cValue)).
    end.

    create ttColumnWidth.
    assign ttColumnWidth.iHor   = iRecord
           ttColumnWidth.iWidth = iMaxWidth.
  end.

  /* Determine a unique filename 
   * Something like: datadigger-view-customer.txt
   */
  cFilename = substitute('&1datadigger-view.&2', session:temp-dir, cFileType ).

  /* Showtime! */
  if search(cFileName) <> ?
    and isFileLocked(cFileName) then
  do:
    message 'Error opening temporary file.~nDo you have it open for editing?~n~n' cFilename view-as alert-box info buttons ok.
    return.
  end.

  output to value( cFilename ).

  case cFileType:
    when 'txt'  then assign cDocStart     = ''
                            cLineStart    = '~n'
                            cLabelStart   = ''      cLabelEnd  = ' = '
                            cDataStart[1] = ''      cDataEnd   = ' | '
                            cDataStart[2] = ''      
                            cLineEnd      = ''
                            cDocEnd       = ''
                            .
    when 'xls' or 
    when 'html' then assign cDocStart     = '<html><body><table border=1>'
                            cLineStart    = '~n<tr>'
                            cLabelStart   = '<td bgcolor="KHAKI"><b>'         cLabelEnd   = '</b></td>'
                            cDataStart[1] = '<td bgcolor="LIGHTYELLOW">'      cDataEnd    = '&nbsp;</td>'
                            cDataStart[2] = '<td bgcolor="WHITE">'      
                            cLineEnd      = '</td> </tr>'
                            cDocEnd       = '~n</table></body></html>'
                            .
  end case. 

  put unformatted cDocStart.
  for each ttView 
    break by ttView.iVer by ttView.iHor:

    /* Determine format for data to get names aligned */
    find ttColumnWidth where ttColumnWidth.iHor = ttView.iHor.
    cDataFormat = fill('x', ttColumnWidth.iWidth).

    if first-of(ttView.iVer) then
    do:
      iLineNr = iLineNr mod 2 + 1.
      put unformatted cLineStart cLabelStart string(ttView.cValue,cDataFormat) cLabelEnd.
    end.
    else
      put unformatted cDataStart[iLineNr] string(ttView.cValue,cDataFormat) cDataEnd.
    
    if last-of(ttView.iVer) then 
      put unformatted cLineEnd.
  end.
  put unformatted cDocEnd.
  output close. 

  /* Start associated program for the txt file */
  os-command no-wait start value(cFilename).

  /* Cleanup */
  empty temp-table ttView. 
  empty temp-table ttColumnWidth.

END PROCEDURE. /* btnViewChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE checkFonts C-Win 
PROCEDURE checkFonts :
/*------------------------------------------------------------------------
  Name         : checkFonts
  Description  : If the default fonts have been messed up, try to set the 
                 fonts to reasonable settings.
  ----------------------------------------------------------------------
  19-09-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable iFont     as integer     no-undo.
  define variable cFontName as character   no-undo.

  {&timerStart} 

  /* 
   * If no fonts are set and the default fonts have
   * been changed, try to set them for the user.
   */
  if isDefaultFontsChanged() 
    and (   logical(getRegistry("DataDigger:fonts","AutoSetFont")) = true
         or getRegistry("DataDigger:fonts","default") = ? 
         or getRegistry("DataDigger:fonts","fixed") = ? ) then 
  do:
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
    empty temp-table ttFont. 

    checkFont:
    do iFont = 0 to font-table:num-entries - 1:
      create ttFont.
      assign ttFont.iFontNr = iFont.
      
      get-key-value section "fonts" key "font" + string(iFont) value ttFont.cFontName.
    end. /* checkFont */

    /* Set default font */
    find first ttFont where ttFont.cFontName matches "MS Sans Serif, size*8" no-error.
    if not available ttFont then 
      find first ttFont where ttFont.cFontName begins "MS Sans Serif" no-error.
    if available ttFont then setRegistry("DataDigger:fonts","default",string(ttFont.iFontNr)).

    /* Set fixed font */
    find first ttFont where ttFont.cFontName matches "Courier New, size*8" no-error.
    if not available ttFont then 
      find first ttFont where ttFont.cFontName begins "Courier New" no-error.
    if available ttFont then setRegistry("DataDigger:fonts","fixed",string(ttFont.iFontNr)).

    /* Clean up; records are no longer needed */
    empty temp-table ttFont. 

    /* Now, check again to see if we found both fonts and have been able to set them */
    if   getRegistry("DataDigger:fonts","default") = ?
      or getRegistry("DataDigger:fonts","fixed") = ?  then
    do:
      run showHelp("FontsChanged","").
  
      /* Don't accept a choice "YES" in combination with HIDDEN=YES because 
       * then the help will pop up everytime automatically. Kinda annoying.
       */
      setRegistry( "DataDigger:help", "FontsChanged:answer","2").
    end.
  end.

  /* Get user defined Default font and fixed-size font */
  giDefaultFont = getFont("Default").
  giFixedFont   = getFont("Fixed").

  /* Make the font table large enough to hold at least 24 fonts */
  if font-table:num-entries < 24 then font-table:num-entries = 24.

  {&timerStop}

end procedure. /* checkFonts */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearDataFilter C-Win 
PROCEDURE clearDataFilter :
/*------------------------------------------------------------------------
  Name : clearDataFilter
  ----------------------------------------------------------------------
  13-11-2013 pti Created
  ----------------------------------------------------------------------*/
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

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearField C-Win 
PROCEDURE clearField :
/*------------------------------------------------------------------------
  Name : clearField
  Desc : Clear a field
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.
  phWidget:SCREEN-VALUE = "".
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE collectFieldInfo C-Win 
PROCEDURE collectFieldInfo PRIVATE :
/*------------------------------------------------------------------------
  Name         : collectFieldInfo 
  Description  : Fill the fields temp-table
  ---------------------------------------------------------------------- 
  18-03-2009 pti Created
  ----------------------------------------------------------------------*/
  
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
/*------------------------------------------------------------------------
  Name         : collectIndexInfo
  Description  : Fill the index temp-table
  ---------------------------------------------------------------------- 
  01-09-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define input parameter pcTableName   as character   no-undo.
  
  define variable hBufferFile       as handle      no-undo.
  define variable hBufferIndex      as handle      no-undo.
  define variable hBufferIndexField as handle      no-undo.
  define variable hBufferField      as handle      no-undo.
  define variable cQuery            as character   no-undo. 
  define variable hQuery            as handle      no-undo.
  define variable cCurrentDatabase  as character   no-undo. /* NelsonAlcala */
  
  DEFINE BUFFER bIndex FOR ttIndex. 
  {&timerStart}

  /* Return if no db connected */
  if num-dbs = 0 then return. 

  /* NelsonAlcala */
  cCurrentDatabase = sdbname(gcCurrentDatabase).  /*use DB schemaholder name*/
  /*
  ** Fill the tt with _Fields
  */
                                           /* NelsonAlcala */
  create buffer hBufferFile       for table /*g*/ cCurrentDatabase + "._File".
  create buffer hBufferIndex      for table /*g*/ cCurrentDatabase + "._Index".
  create buffer hBufferIndexField for table /*g*/ cCurrentDatabase + "._Index-Field".
  create buffer hBufferField      for table /*g*/ cCurrentDatabase + "._Field".

  create query hQuery.
  hQuery:set-buffers(hBufferFile,hBufferIndex,hBufferIndexField,hBufferField).

  cQuery = substitute("for each &1._File  where &1._file._file-name = '&2' no-lock " +
                      "  , each &1._Index       of &1._File        no-lock " + 
                      "  , each &1._Index-field of &1._Index       no-lock " + 
                      "  , each &1._Field       of &1._Index-field no-lock where true " 
                     , cCurrentDatabase /* NelsonAlcala */
                     , pcTableName
                     ).

  hQuery:query-prepare(cQuery).
  empty temp-table bIndex.
  hQuery:query-open().
  hQuery:get-first().

  repeat while not hQuery:query-off-end:

    find bIndex where bIndex.cIndexName = hBufferIndex:buffer-field('_index-name'):buffer-value no-error.
    if not available bIndex then 
    do:
      create bIndex.

      bIndex.cIndexName  = hBufferIndex:buffer-field('_index-name'):buffer-value.
      bIndex.cIndexFlags = string( hBufferFile:buffer-field('_prime-index'):buffer-value = hBufferIndex:recid, 'P/') 
                          + string( hBufferIndex:buffer-field('_unique'):buffer-value, ' U/') 
                          + string( hBufferIndex:buffer-field('_WordIdx'):buffer-value <> ?, ' W/') 
                          + string( hBufferIndex:buffer-field('_Active'):buffer-value , ' /INACTIVE') 
                          .
      bIndex.cIndexFlags  = trim(bIndex.cIndexFlags).
      bIndex.lIndexActive = hBufferIndex:buffer-field('_Active'):buffer-value.
    end.

    /* Add field */
    bIndex.cIndexFields = substitute('&1  &2&3'
                                     , bIndex.cIndexFields 
                                     , hBufferField:buffer-field('_field-name'):buffer-value 
                                     , string( hBufferIndexField:buffer-field('_Ascending'):buffer-value, '+/-')
                                     ).
    bIndex.cIndexFields = trim(bIndex.cIndexFields,' ').

    /* Naked list of just fieldnames */
    bIndex.cFieldList   = substitute('&1,&2'
                                     , bIndex.cFieldList 
                                     , hBufferField:buffer-field('_field-name'):buffer-value 
                                     ).
    bIndex.cFieldList   = trim(bIndex.cFieldList,', ').

    hQuery:get-next().
  end.
  hQuery:query-close().

  delete object hQuery.
  delete object hBufferFile.      
  delete object hBufferIndex.     
  delete object hBufferIndexField.
  delete object hBufferField.     

  {&timerStop}
  
end procedure. /* collectIndexInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE connectDatabase C-Win 
PROCEDURE connectDatabase :
/*------------------------------------------------------------------------
  Name         : connectDatabase
  Description  : Quick-Connect to a database via the context menu
  ---------------------------------------------------------------------- 
  18-09-2009 pti Created
  ----------------------------------------------------------------------*/

  define input parameter pcDatabase as character no-undo.
  
  define variable cError        as character   no-undo.
  define variable cProgDir      as character   no-undo.
  define variable cDatabases    as character   no-undo.
  define variable cDatabasesOld as character   no-undo.
  define variable cCurrentDb    as character   no-undo. 

  do with frame {&frame-name}:
    cProgDir   = getProgramDir().
    cCurrentDb = gcCurrentDatabase. 
    
    cDatabasesOld = getDatabaseList().
    run value(cProgDir + 'wConnections.w') (input 'CONNECT', input pcDatabase, output cError).
    if cError <> '' then
      message cError view-as alert-box info buttons ok.
  
    /* Get all connected databases */
    cDatabases = getDatabaseList().
  
    /* If needed, repopulate db combo */
    if cDatabases <> cDatabasesOld then
    do:
      assign 
        cbDatabaseFilter:list-items   = ',' + cDatabases
        cbDatabaseFilter:screen-value = cCurrentDb
        .

      /* Get list of all tables of all databases */
      run getTables(output table ttTable).
    end.
  
    /* If the chosen DB is connected, switch to that one */
    if lookup(pcDatabase, cDatabases) > 0 then
      cbDatabaseFilter:screen-value = pcDatabase.
  
    apply 'value-changed' to cbDatabaseFilter. 
  end.

END PROCEDURE.

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
/*------------------------------------------------------------------------
  Name         : convertSettings
  Description  : Do one-time conversions for new versions
  ---------------------------------------------------------------------- 
  12-09-2012 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER piOldVersion AS INTEGER   NO-UNDO.
  DEFINE INPUT PARAMETER pcOldBuild   AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cFile      AS LONGCHAR  NO-UNDO.
  DEFINE VARIABLE cIniFile   AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cCustomLib AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cLine      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cSection   AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iLine      AS INTEGER   NO-UNDO.

  SESSION:SET-WAIT-STATE("general").
  CASE piOldVersion:

    WHEN 18 THEN
    DO:
      /* Obsolete code */
      OS-DELETE VALUE( "frLoadChooseFile.w").
      OS-DELETE VALUE( "frLoadChooseFile.r").
      OS-DELETE VALUE( "frLoadImport.w").
      OS-DELETE VALUE( "frLoadImport.r").
      OS-DELETE VALUE( "frLoadMapping.w").
      OS-DELETE VALUE( "frLoadMapping.r").
      OS-DELETE VALUE( "frLoadSettings.w").
      OS-DELETE VALUE( "frLoadSettings.r").
      OS-DELETE VALUE( "wLoadData.w").
      OS-DELETE VALUE( "wLoadData.wrx").
      OS-DELETE VALUE( "wLoadData.r").
      OS-DELETE VALUE( "myDump.p").
      OS-DELETE VALUE( "myDump.r").
      OS-DELETE VALUE( "dDump.w").
      OS-DELETE VALUE( "dDump.r").

      /* Custom filter settings not needed. CustomFilter event
       * has little to no impact on performance, so this is obsolete
       */
      setRegistry("DataDigger","CustomFilter:Fields",?).
      setRegistry("DataDigger","CustomFilter:Enabled",?).

      /* Save data filters replaced with combo box of last used values */
      setRegistry("DataDigger","SaveDataFilters",?).

      /* Load complete INI file in a longchar */
      cIniFile = SUBSTITUTE('&1DataDigger-&2.ini', getProgramDir(), getUserName() ). 
      COPY-LOB FILE cIniFile TO cFile.

      /* Replace old separator char CHR(160) to new CHR(1) for saved queries */
/*      cFile = REPLACE(cFile,CHR(1),CHR(2)). */
      cFile = REPLACE(cFile,CHR(160),CHR(1)).
      COPY-LOB cFile TO FILE cIniFile.

      DO iLine = 1 TO NUM-ENTRIES(cFile,"~n"):
        cLine = TRIM(ENTRY(iLine,cFile,"~n")).

        /* Remember section */
        IF cLine MATCHES "[*]" THEN cSection = TRIM(cLine,"[]").

        /* Only proceed if we are in a DB-section */
        IF NOT cSection MATCHES "DB:*" THEN NEXT.

        /* Remove field list when individual extents are specified. No longer per extent */
        IF cLine MATCHES "*:Fields=*[*]*" then
          setRegistry(cSection, ENTRY(1,cLine,"="), ? ).

        /* Remove filter value */
        IF cLine MATCHES "*:Filter=*" THEN
          setRegistry(cSection, ENTRY(1,cLine,"="), ? ).

      END. /* do iLine */
    END. /* 18 */

  END CASE. /* old version */
  
  SESSION:SET-WAIT-STATE("").

END PROCEDURE. /* convertSettings */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE copyDataToClipboard C-Win 
PROCEDURE copyDataToClipboard :
/*------------------------------------------------------------------------
  Name         : copyDataToClipboard 
  Description  : Copy the value of the column to the clipboard
  ---------------------------------------------------------------------- 
  22-09-2010 pti Created
  ----------------------------------------------------------------------*/

  define variable cColumnName  as character   no-undo.
  define variable cColumnValue as character   no-undo.

  if num-entries(ghDataBrowse:private-data,chr(1)) <> 3 then return. 

  cColumnName  = entry(1, ghDataBrowse:private-data,chr(1)).
  cColumnValue = entry(2, ghDataBrowse:private-data,chr(1)).

  if cColumnValue <> '' and cColumnValue <> ? then clipboard:value = trim(cColumnValue).

end procedure. /* copyDataToClipboard */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE copyToClipboard C-Win 
PROCEDURE copyToClipboard :
/*------------------------------------------------------------------------
  Name : copyToClipboard
  Desc : Copy value to clipboard 
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.
  CLIPBOARD:VALUE = phWidget:SCREEN-VALUE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE createCounter C-Win 
PROCEDURE createCounter :
/*------------------------------------------------------------------------
  Name         : createCounter
  Description  : Create the digits for the counter
  ---------------------------------------------------------------------- 
  25-01-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable iDigit     as integer no-undo. 
  define variable iNumDigits as integer no-undo initial {&numDigits}. 
  define variable hText      as handle  no-undo. 
  define variable iRow       as integer no-undo. 
  
  {&timerStart}

  do with frame frCounter:

    do iRow = 1 to 2:
      do iDigit = 1 to iNumDigits:
        create text hText
          assign 
            frame         = frame frMain:handle
            screen-value  = "0"
            font          = giDefaultFont
            fgcolor       = getColor('QueryCounter:FG')
            x             = rcCounter:x + {&marginHor} + (iDigit - 1) * {&digitWidth}
            y             = rcCounter:y + {&marginVer} - 1
            width-pixels  = {&digitWidth}
            height-pixels = {&digitHeight}
            visible       = yes
            sensitive     = no
            .
        case iRow:
          when 1 then ghNewDigit[iDigit] = hText.
          when 2 then ghOldDigit[iDigit] = hText.
        end case. 

      end.
    end.  

    rcCounter:width-pixels  = iNumDigits * {&digitWidth} + 2 * {&marginHor}.
    rcCounter:height-pixels = {&digitHeight} + 2 * {&marginVer} no-error.
    rcCounter:visible = no.
  end.

  {&timerStop}

END PROCEDURE. /* createCounter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE createMenuDataBrowse C-Win 
PROCEDURE createMenuDataBrowse :
/*------------------------------------------------------------------------
  Name         : createMenuDataBrowse2
  Description  : Rebuild the connection submenu of the 'add' button
  ----------------------------------------------------------------------*/
  
  /* Attach connections to btnConnect */
  define variable hMenu     as handle no-undo.
  define variable hMenuItem as handle no-undo.

  hMenu = createMenu(ghDataBrowse).

  /* Copy to clipboard */
  hMenuItem = createMenuItem(hMenu,"Item","Copy to clipboard","copyDataToClipboard").
  ON "CHOOSE" of hMenuItem persistent run copyDataToClipboard in this-procedure.

  /* Show value of field */
  hMenuItem = createMenuItem(hMenu,"Item","Show Value","showValue").
  ON "CHOOSE" of hMenuItem persistent run showValue in this-procedure.

  /* Add to filter */
  hMenuItem = createMenuItem(hMenu,"Item","Add to filter","addFilter").
  on "CHOOSE" of hMenuItem persistent run setDataFilter in this-procedure (no).

  /* Filter on this field only */
  hMenuItem = createMenuItem(hMenu,"Item","Set as only filter","setFilter").
  on "CHOOSE" of hMenuItem persistent run setDataFilter in this-procedure (yes).

  /* Filter on this field only */
  hMenuItem = createMenuItem(hMenu,"Item","Clear Filters","clearFilter").
  on "CHOOSE" of hMenuItem persistent run btnClearDataFilterChoose in this-procedure (yes).

  /* Rule */
  hMenuItem = createMenuItem(hMenu,"Rule","","").

  /* Shortcut to viewing records */
  hMenuItem = createMenuItem(hMenu,"Item","View selected","view").
  on "CHOOSE" of hMenuItem persistent run btnViewChoose in this-procedure.

  /* Shortcut to adding records */
  hMenuItem = createMenuItem(hMenu,"Item","Add record","add").
  on "CHOOSE" of hMenuItem persistent run btnAddChoose in this-procedure.

  /* Shortcut to cloning records */
  hMenuItem = createMenuItem(hMenu,"Item","Clone record","clone").
  on "CHOOSE" of hMenuItem persistent run btnCloneChoose in this-procedure.

  /* Shortcut to editing records */
  hMenuItem = createMenuItem(hMenu,"Item","Edit selected","edit").
  on "CHOOSE" of hMenuItem persistent run btnEditChoose in this-procedure.

  /* Shortcut to dumping records */
  hMenuItem = createMenuItem(hMenu,"Item","Dump selected","dump").
  on "CHOOSE" of hMenuItem persistent run btnDumpChoose in this-procedure.

  /* Rule */
  hMenuItem = createMenuItem(hMenu,"Rule","","").

  /* Shortcut to hiding the column */
  hMenuItem = createMenuItem(hMenu,"Item","Hide this column","hideColumn").
  on "CHOOSE" of hMenuItem persistent run hideColumn in this-procedure.

  /* Shortcut to unhiding the column */
  hMenuItem = createMenuItem(hMenu,"Item","Unhide all columns","unhideColumn").
  on "CHOOSE" of hMenuItem persistent run showField in this-procedure('*',true).
    
  /* Rule */
  hMenuItem = createMenuItem(hMenu,"Rule","","").

  /* Shortcut to deleting records */
  hMenuItem = createMenuItem(hMenu,"Item","Delete selected","delete").
  on "CHOOSE" of hMenuItem persistent run btnDeleteChoose in this-procedure.

end procedure. /* createMenuDataBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cutToClipboard C-Win 
PROCEDURE cutToClipboard :
/*------------------------------------------------------------------------
  Name : cutToClipboard
  Desc : Copy value to clipboard and delete current value
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.
  CLIPBOARD:VALUE = phWidget:SCREEN-VALUE.
  phWidget:SCREEN-VALUE = "".

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataColumnResize C-Win 
PROCEDURE dataColumnResize :
/*------------------------------------------------------------------------
  Name         : dataColumnResize
  Description  : Resize a data column
  ---------------------------------------------------------------------- 
  18-09-2009 pti Created
  ----------------------------------------------------------------------*/
  define input  parameter phColumn as handle no-undo.

  setRegistry( substitute('DB:&1',gcCurrentDatabase)
             , substitute('&1.&2:width', gcCurrentTable, phColumn:name)
             , string(phColumn:width-pixels)
             ).

  run dataScrollNotify in this-procedure (ghDataBrowse).

END PROCEDURE. /* dataColumnResize */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataColumnSort C-Win 
PROCEDURE dataColumnSort PRIVATE :
/*------------------------------------------------------------------------
  Name         : dataColumnSort
  Description  : Sort on a datacolumn
  ---------------------------------------------------------------------- 
  18-09-2009 pti Created
  07-02-2011 pti Rewritten
  ----------------------------------------------------------------------*/

  run reopenDataBrowse(self:current-column:name,?).

END PROCEDURE. /* dataColumnSort */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DataDiggerClose C-Win 
PROCEDURE DataDiggerClose :
/*------------------------------------------------------------------------
  Name         : DataDiggerClose
  Description  : Close DataDigger after event 'DataDiggerClose'
  ---------------------------------------------------------------------- 
  14-12-2009 pti Created
  ----------------------------------------------------------------------*/

  apply 'close' to this-procedure. 

end procedure. /* DataDiggerClose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataDoubleClick C-Win 
PROCEDURE dataDoubleClick :
/*------------------------------------------------------------------------
  Name         : dataDoubleClick
  Description  : Double click on databrowse might result in 
                 EDIT / VIEW / DUMP
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  28-10-2011 pti Simpler: default action is set in startSession proc
  ----------------------------------------------------------------------*/
  define input parameter hBrowseBuffer as handle no-undo.

  /* What to do on double click? */
  case getRegistry('DataDigger','DataDoubleClick'):
    when 'VIEW' then run btnViewChoose. 
    when 'EDIT' then run btnEditChoose. 
    when 'DUMP' then run btnDumpChoose. 
  end case.

end procedure. /* dataDoubleClick */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataOffHome C-Win 
PROCEDURE dataOffHome :
define buffer bColumn for ttColumn. 

  find bColumn where bColumn.cFullName = gcLastDataField no-error.
  if not available bColumn then find first bColumn. 
  if not available bColumn then return. 

  setFilterFieldColor(bColumn.hFilter).
  apply 'entry' to bColumn.hFilter. 

  return no-apply.

end procedure. /* dataOffHome */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataRowDisplay C-Win 
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

    /* Add field for RECID */
    IF bColumn.cFieldName = "RECID" THEN
    DO:
      bColumn.hColumn:FONT = giFixedFont.
      bColumn.hColumn:SCREEN-VALUE = STRING( phBrowseBuffer:RECID, "zzzzzzzzz9" ).
    END.

    /* Add field for ROWID */
    IF bColumn.cFieldName = "ROWID" then
    DO:
      bColumn.hColumn:FONT = giFixedFont.
      bColumn.hColumn:SCREEN-VALUE = STRING(phBrowseBuffer:ROWID, "x(30)").
    END.

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
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER hBrowseBuffer AS HANDLE      NO-UNDO.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataRowValueChanged C-Win 
PROCEDURE dataRowValueChanged :
/*------------------------------------------------------------------------
  Name         : dataRowValueChanged
  Description  : Save the content of the fields in linkinfo
                 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER hBrowseBuffer AS HANDLE      NO-UNDO.

  define variable iColumn    as integer     no-undo.
  define variable hColumn    as handle      no-undo.
  define variable cFieldName as character   no-undo. 

  run setNumSelectedRecords.

  publish "debugMessage" (3, substitute("Browse columns: &1", gcDataBrowseColumns)).
  publish "debugMessage" (3, substitute("Column names  : &1", gcDataBrowseColumnNames)).

  do iColumn = 1 to num-entries(gcDataBrowseColumns):

    hColumn    = widget-handle( entry(iColumn,gcDataBrowseColumns) ).
    cFieldName = entry(iColumn,gcDataBrowseColumnNames).

    /*
    if cFieldName = 'RECID' or cFieldName = 'ROWID' then
      cFieldName = lower(substitute('&1 - &2', gcCurrentTable, cFieldName)).
    */
    if hColumn:screen-value <> "" and hColumn:screen-value <> ? then
      setLinkInfo(cFieldName, hColumn:screen-value).
  end.

  setUpdatePanel(?). /* Refresh sensitivity of buttons if needed */

end procedure. /* dataRowValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataScrollNotify C-Win 
PROCEDURE dataScrollNotify :
/*------------------------------------------------------------------------
  Name         : dataScrollNotify
  Description  : Adjust size and position of the filterfields to browse
  ------------------------------------------------------------------------- */
  DEFINE INPUT PARAMETER phBrowse AS HANDLE NO-UNDO.

  DEFINE VARIABLE cFilterFields AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iColumn       AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cButtons      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iField        AS INTEGER   NO-UNDO. 
  DEFINE VARIABLE hColumn       AS HANDLE    NO-UNDO. 

  DEFINE BUFFER bColumn FOR ttColumn.

  {&timerStart}
  publish "debugMessage" (1, "scroll-notify of dataBrowse").

  /* Might get called when browse is not yet realized, so: */
  IF NOT VALID-HANDLE(phBrowse) THEN RETURN.

  /* Freeze all */
  setWindowFreeze(yes).

  getFilterLoop:
  FOR EACH bColumn BY bColumn.iColumnNr: 
    IF VALID-HANDLE(bColumn.hFilter) THEN
      cFilterFields = TRIM(SUBSTITUTE('&1,&2', cFilterFields, bColumn.hFilter),',').
  END.

  DO WITH FRAME frData:
    cButtons = SUBSTITUTE('&1,&2', btnClearDataFilter:HANDLE, btnDataFilter:HANDLE).
  END.

  /* Resize them */
  RUN resizeFilterFields
    ( INPUT cFilterFields
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
/*----------------------------------------------------------------------------
  Name: dataSelectAll
  Desc: Select all records in the browse
  ------------------------------------------------------------------------- */
  define input  parameter phBrowse as handle     no-undo.

  setWindowFreeze(yes).
  phBrowse:select-all().
  run setNumSelectedRecords.
  setUpdatePanel('display'). /* Activate buttons */
  setWindowFreeze(no).

end procedure. /* dataSelectAll */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dataSelectNone C-Win 
PROCEDURE dataSelectNone :
/*----------------------------------------------------------------------------
  Name: dataSelectNone
  Desc: Deselect all records in the browse
  ------------------------------------------------------------------------- */
  define input  parameter phBrowse as handle     no-undo.

  setWindowFreeze(yes).
  phBrowse:deselect-rows().
  run setNumSelectedRecords.
  setUpdatePanel('display'). /* Activate buttons */
  setWindowFreeze(no).

end procedure. /* dataSelectNone */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE deleteDataFilters C-Win 
PROCEDURE deleteDataFilters :
/*----------------------------------------------------------------------------
  Name : deleteDataFilters
  Desc : Kill the data filters and its menu
  ------------------------------------------------------------------------- */
  DEFINE INPUT PARAMETER phParentBrowse AS HANDLE NO-UNDO.

  DEFINE BUFFER bFilter FOR ttFilter. 

  {&timerStart}

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
/*----------------------------------------------------------------------------
  Name : deleteRecord
  Desc : Generate a program to delete a record with dictionary validations
  ------------------------------------------------------------------------- */
  
  define input parameter pcDatabase      as character no-undo. 
  define input parameter pcTable         as character no-undo. 
  define input parameter prRowid         as rowid     no-undo. 
  define input parameter plEnableTrigger as logical   no-undo.
  define output parameter plDeleted      as logical   no-undo. 

  define variable cTempFile as character no-undo. 

  cTempFile = substitute('&1delrecord.p', session:temp-directory).

  output to value(cTempFile).
  put unformatted
         substitute('/* ' ) 
    skip substitute(' * Name: delrecord.p ')
    skip substitute(' * Desc: generated by DataDigger to delete &1.&2                        ', pcDataBase, pcTable)  
    skip substitute(' * Date: &1 ', now ) 
    skip substitute(' */  ' ) 
    skip substitute('     ' ) 
    skip substitute('define input parameter prRowid    as rowid   no-undo.                   ' ) 
    skip substitute('define output parameter plDeleted as logical no-undo.                   ' ) 
    skip substitute('  ' ). 

  if not plEnableTrigger then
    put unformatted 
    skip substitute('disable triggers for dump of &1.&2.                                     ', pcDataBase, pcTable).

  put unformatted
    skip substitute('  ' )
    skip substitute('/* Find the record to delete */' ) 
    skip substitute('find &1.&2 where rowid(&1.&2) = prRowid exclusive-lock no-error no-wait.', pcDataBase, pcTable) 
    skip substitute('if available &1.&2 then delete &1.&2 no-error.                          ', pcDataBase, pcTable) 
    skip substitute('  ' ) 
    skip substitute('/* See if its really gone */' ) 
    skip substitute('plDeleted = not can-find(&1.&2 where rowid(&1.&2) = prRowid).           ', pcDataBase, pcTable) 
    skip .
  output close. 

  /* Run generated prog and cleanup */
  run value(cTempFile) (input prRowid, output plDeleted).
  os-delete value(cTempFile).

end procedure. /* deleteRecord */

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

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE doNothing C-Win 
PROCEDURE doNothing :
/*----------------------------------------------------------------------------
  Name : doNothing
  Desc : Wait for an amount of msec
  ------------------------------------------------------------------------- */
  
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
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  /* Field popup-menu on brFields */
  define variable hEditor    as handle      no-undo.
  define variable hFieldName as handle      no-undo.
  define variable hFieldType as handle      no-undo.
  define variable cField     as character   no-undo. 
  define variable cColumn    as character   no-undo. 
  define variable lOk        as logical     no-undo. 
  define variable iOldPos    as integer     no-undo. 
  define variable iLength    as integer     no-undo. 
  define variable iMouseX    as integer no-undo. 
  define variable iMouseY    as integer no-undo. 
  define variable iRet       as integer no-undo. 

  /* See if we clicked on the browse column */
  run getMouseXY(input brFields:handle in frame frMain, output iMouseX, output iMouseY).
  if iMouseY < 18 then 
  do:
    RUN SendMessageA (tgSelAll:HWND, 517, 0, 0, output iRet).
    return. 
  end.

  else
  do:
    if not brFields:query:get-buffer-handle(1):available then return. 
  
    publish "setUsage" ("showFieldMenu"). /* user behaviour */
  
    /* Select the row we clicked on */
    run selectClickedRow(brFields:handle, output lOk, output cColumn).
    if not lOk then return no-apply. 
  
    hFieldName = brFields:query:get-buffer-handle(1):buffer-field('cFieldName'):handle.
    hFieldType = brFields:query:get-buffer-handle(1):buffer-field('cDataType'):handle.
  
    if valid-handle(hFieldName) then 
    do:
      /* If CTRL is pressed, do not insert the linked value */
      cField  = hFieldName:buffer-value.
  
      if lookup("CTRL", GetKeyList() ) <> 0 or getLinkInfo(cField) = "" then 
      do:
        case cField:
          when "RECID" then cField = substitute('RECID(&1)', gcCurrentTable).
          when "ROWID" then cField = substitute('ROWID(&1)', gcCurrentTable).
          otherwise cField  = hFieldName:buffer-value.
        end case. 
      end.

      else 
      do:
        /* In case of RECID / ROWID insert proper syntax */
        case cField:
          when "RECID" then cField = substitute('RECID(&1) = &2', gcCurrentTable, quoter(getLinkInfo(cField))).
          when "ROWID" then cField = substitute('ROWID(&1) = TO-ROWID(&2)', gcCurrentTable, quoter(getLinkInfo(cField))).
          otherwise cField = substitute('&1 = &2', cField, quoter(getLinkInfo(cField))).
        end.
      end.
  
      iLength = length(cField).
  
      /* If the query editor is expanded, do actions to that field */
      hEditor = getActiveQueryEditor().
  
      /* Remember old position for positioning cursor */
      iOldPos = hEditor:cursor-offset.
  
      /* No text selected */
      if hEditor:selection-text = "" then
      do:
        /* If ficWhere only holds the text <empty> then delete that */
        if hEditor:screen-value = '<empty>' then hEditor:screen-value = ''.
        hEditor:insert-string(cField).
      end.
      else 
      do:
        hEditor:replace-selection-text(cField).
      end.
  
      apply "entry" to hEditor.
      hEditor:cursor-offset = iOldPos + iLength.
    end.

    return no-apply.
  end. 

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
  DISPLAY fiIndexNameFilter fiFlagsFilter fiFieldsFilter tgSelAll fiTableFilter 
          cbDatabaseFilter fiTableDesc ficWhere 
      WITH FRAME frMain IN WINDOW C-Win.
  ENABLE rctQuery btnHelp rctEdit rcCounter btnClearFieldFilter btnFieldFilter 
         fiIndexNameFilter fiFlagsFilter fiFieldsFilter btnClearIndexFilter 
         btnIndexFilter tgSelAll tgDebugMode brFields btnMoveTop brIndexes 
         fiTableFilter cbDatabaseFilter btnClearTableFilter btnTableFilter 
         btnMoveUp brTables btnReset btnMoveDown btnMoveBottom fiTableDesc 
         btnWhere btnClear btnPrevQuery btnQueries btnNextQuery btnClipboard 
         ficWhere btnDataDigger btnTools btnTabFields btnTabIndexes 
         btnTableView btnResizeVer btnClone btnDump btnView btnAdd btnDelete 
         btnEdit 
      WITH FRAME frMain IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frMain}
  DISPLAY edHint 
      WITH FRAME frHint IN WINDOW C-Win.
  ENABLE edHint btGotIt 
      WITH FRAME frHint IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frHint}
  DISPLAY cbAndOr cbFields cbOperator ficValue ficWhere2 
      WITH FRAME frWhere IN WINDOW C-Win.
  ENABLE btnAnd rctQueryButtons cbAndOr cbFields cbOperator ficValue btnInsert 
         ficWhere2 btnClear-2 btnPrevQuery-2 btnQueries-2 btnNextQuery-2 
         btnClipboard-2 btnOK btnCancel-2 btnBegins btnBracket btnContains 
         btnEq btnGT btnLT btnMatches btnNE btnOr btnQt btnToday 
      WITH FRAME frWhere IN WINDOW C-Win.
  VIEW FRAME frWhere IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frWhere}
  DISPLAY fiNumSelected fiNumResults 
      WITH FRAME frData IN WINDOW C-Win.
  ENABLE rctData btnClearDataFilter btnDataFilter fiNumResults 
      WITH FRAME frData IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frData}
  ENABLE btnSettings btnChangeLog-txt btnConnections btnDict btnProcEdit 
         btnDump-2 btnLoad-2 btnQueries-3 btnQueryTester btnChangeLog btnAbout 
         btnDump-txt btnAbout-txt btnDict-txt btnConnections-txt btnLoad-txt 
         btnProcEdit-txt btnQueries-txt btnQueryTester-txt btnSettings-txt 
      WITH FRAME frSettings IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-frSettings}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE endResize C-Win 
PROCEDURE endResize :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE VARIABLE iButtonSpacingX AS INTEGER    NO-UNDO.
  DEFINE VARIABLE iButtonSpacingY AS INTEGER    NO-UNDO.
  DEFINE VARIABLE hWidget         AS HANDLE     NO-UNDO.

  DEFINE BUFFER ttField FOR ttField. 
  {&timerStart}

  .MESSAGE 'endResize' VIEW-AS ALERT-BOX INFO BUTTONS OK.

  setWindowFreeze(YES).

  /* Set max width */
  IF C-Win:WIDTH > 384 THEN C-Win:WIDTH = 384.

  /* Set frame width */
  FRAME {&FRAME-NAME}:WIDTH-PIXELS = C-Win:FULL-WIDTH-PIXELS NO-ERROR.
  FRAME {&FRAME-NAME}:HEIGHT-PIXELS = C-Win:FULL-HEIGHT-PIXELS NO-ERROR.

  /* Sanity checks */
  IF btnResizeVer:Y < 150 THEN btnResizeVer:Y = 150.
  IF btnResizeVer:Y > (C-Win:HEIGHT-PIXELS - 180) THEN btnResizeVer:Y = C-Win:HEIGHT-PIXELS - 180.

  IF VALID-HANDLE(ghDataBrowse) THEN
  ASSIGN 
    ghDataBrowse:WIDTH-PIXELS  = 100
    ghDataBrowse:HEIGHT-PIXELS = 100
    ghDataBrowse:Y = 1
    ghDataBrowse:X = 1.

  /* Set width of main rectangles */
  rctQuery:WIDTH-PIXELS  = C-Win:WIDTH-PIXELS - 4 NO-ERROR. /* 4 */
  rctQuery:X             = 0 NO-ERROR.
  rctQuery:Y             = 0 NO-ERROR.
  rctQuery:HEIGHT-PIXELS = btnResizeVer:Y + 32.

  /* Buttons DD / Tools / Help */
  btnResizeVer:WIDTH-PIXELS = rctQuery:WIDTH-PIXELS.
  btnResizeVer:X = 0.
  btnDataDigger:X = rctQuery:X + 3.
  btnDataDigger:Y = rctQuery:Y + 2.
  btnTools:X = btnDataDigger:X + btnDataDigger:WIDTH-PIXELS.
  btnTools:Y = rctQuery:Y + 2.
  btnHelp:X = btnTools:X + btnTools:WIDTH-PIXELS.
  btnHelp:Y = rctQuery:Y + 2.
  tgDebugMode:X = btnHelp:X + btnHelp:WIDTH-PIXELS + 10.
  tgDebugMode:Y = rctQuery:Y + 10.

  /* Table browse */
  rcTableFilter:X = rctQuery:X.
  rcTableFilter:Y = rctQuery:Y + 54.
  rcTableFilter:WIDTH-PIXELS = 226.
  rcTableFilter:HEIGHT-PIXELS = btnResizeVer:Y - rcTableFilter:Y.

  brTables:X = rcTableFilter:X + 3.  
  brTables:Y = rcTableFilter:Y + 3.  
  brTables:WIDTH-PIXELS = rcTableFilter:WIDTH-PIXELS - 6.
  brTables:HEIGHT-PIXELS = rcTableFilter:HEIGHT-PIXELS - 6 - fiTableDesc:HEIGHT-PIXELS.

  fiTableDesc:X = brTables:X.
  fiTableDesc:Y = brTables:Y + brTables:HEIGHT-PIXELS.
  fiTableDesc:WIDTH-PIXELS = brTables:WIDTH-PIXELS - btnTableView:WIDTH-PIXELS.
  btnTableView:X = fiTableDesc:X + fiTableDesc:WIDTH-PIXELS.
  btnTableView:Y = fiTableDesc:Y.

  /* Data */
  DO WITH FRAME frData:

    /* Prepare embedding frame. First make small to avoid errors */
    FRAME frData:WIDTH-PIXELS  = 100.
    FRAME frData:HEIGHT-PIXELS = 100.
    FRAME frData:X = 0.
    FRAME frData:Y = rctQuery:Y + rctQuery:HEIGHT-PIXELS + 2.   
    FRAME frData:WIDTH-PIXELS  = rctQuery:WIDTH-PIXELS + 4 NO-ERROR.
    FRAME frData:HEIGHT-PIXELS = C-Win:HEIGHT-PIXELS - rctQuery:HEIGHT-PIXELS - 34 NO-ERROR.

    /* Make small to prevent errors */
    rctData:WIDTH-PIXELS  = 1.
    rctData:HEIGHT-PIXELS = 1.
    rctDataFilter:WIDTH-PIXELS  = 1. 
    rctDataFilter:HEIGHT-PIXELS = 27. 

    rctData:X                 = 0.
    rctData:Y                 = 1.
    rctData:WIDTH-PIXELS      = FRAME frData:WIDTH-PIXELS - 0 NO-ERROR.
    rctData:HEIGHT-PIXELS     = FRAME frData:HEIGHT-PIXELS - 10 NO-ERROR.

    rctDataFilter:WIDTH-PIXELS = FRAME frData:WIDTH-PIXELS - rctDataFilter:X - 4 NO-ERROR.
  END.
  
  /* Edit buttons */
  rctEdit:X = FRAME frData:X NO-ERROR.
  rctEdit:Y = FRAME frData:Y + FRAME frData:HEIGHT-PIXELS + 0 NO-ERROR.
  
  /* Positioning of buttons "Add" "Save" etc */
  iButtonSpacingX = 5.
  iButtonSpacingY = 0.
  btnAdd:X      = rctEdit:X + iButtonSpacingX.
  btnClone:X    = btnAdd:X  + btnAdd:WIDTH-PIXELS .
  btnEdit:X     = btnClone:X + btnClone:WIDTH-PIXELS.
  btnDump:X     = btnEdit:X + (2 * btnEdit:WIDTH-PIXELS).
  btnView:X     = btnDump:X + btnDump:WIDTH-PIXELS.
/*   btnLoad:X     = btnView:X + btnView:WIDTH-PIXELS. */
  btnDelete:X   = btnView:X + (2 * btnView:WIDTH-PIXELS).

  btnAdd:Y      = rctEdit:Y + iButtonSpacingY.
  btnClone:Y    = rctEdit:Y + iButtonSpacingY.
  btnEdit:Y     = rctEdit:Y + iButtonSpacingY.
  btnDump:Y     = rctEdit:Y + iButtonSpacingY.
  btnView:Y     = rctEdit:Y + iButtonSpacingY.
/*   btnLoad:Y     = rctEdit:Y + iButtonSpacingY. */
  btnDelete:Y   = rctEdit:Y + iButtonSpacingY.

  /* Num results of query */
  fiNumResults:X = rctData:X + rctData:WIDTH-PIXELS - fiNumResults:WIDTH-PIXELS - 40.
  fiNumResults:Y = rctData:Y + rctData:HEIGHT-PIXELS - 8.
  
  /* Num selected records */
  fiNumSelected:Y = fiNumResults:Y.
  fiNumSelected:X = fiNumResults:X - FONT-TABLE:GET-TEXT-WIDTH-PIXELS( fiNumSelected:SCREEN-VALUE, fiNumSelected:FONT) - 5.

  DO:
    /* Positioning of browse with fields */
    ghFieldBrowse:WIDTH-PIXELS  = rctQuery:WIDTH-PIXELS - 282.
    ghFieldBrowse:HEIGHT-PIXELS = btnResizeVer:Y - ghFieldBrowse:Y - 3.

    /* Index browse has same dimensions as field browse 
     * Due to errors on resizing, first 'park' the browse in the upper 
     * left with width 1, then set the proper size attributes.
     */
    brIndexes:X             = 1.
    brIndexes:WIDTH-PIXELS  = 1.
    brIndexes:X             = ghFieldBrowse:X.            
    brIndexes:Y             = ghFieldBrowse:Y.            
    brIndexes:WIDTH-PIXELS  = ghFieldBrowse:WIDTH-PIXELS. 
    brIndexes:HEIGHT-PIXELS = ghFieldBrowse:HEIGHT-PIXELS.

    /* resize rectangles around the browse */
    rcFieldFilter:X             = ghFieldBrowse:X - 3.
    rcFieldFilter:Y             = ghFieldBrowse:Y - 3.
    rcFieldFilter:WIDTH-PIXELS  = ghFieldBrowse:WIDTH-PIXELS + 6.
    rcFieldFilter:HEIGHT-PIXELS = ghFieldBrowse:HEIGHT-PIXELS + 6.
    rcIndexFilter:X             = brIndexes:X - 3.
    rcIndexFilter:Y             = brIndexes:Y - 3.
    rcIndexFilter:WIDTH-PIXELS  = brIndexes:WIDTH-PIXELS + 6.
    rcIndexFilter:HEIGHT-PIXELS = brIndexes:HEIGHT-PIXELS + 6.

    /* right-align buttons with field browse */
    btnClipboard:X = (ghFieldBrowse:X + ghFieldBrowse:WIDTH-PIXELS) - btnClipboard:WIDTH-PIXELS.
    btnNextQuery:X = btnClipboard:X - btnNextQuery:WIDTH-PIXELS + 1.
    btnQueries:X   = btnNextQuery:X - btnQueries:WIDTH-PIXELS + 1.
    btnPrevQuery:X = btnQueries:X   - btnPrevQuery:WIDTH-PIXELS + 1.
    btnClear:X     = btnPrevQuery:X - btnClear:WIDTH-PIXELS + 1.
    btnViewData:X  = btnClear:X     - btnViewData:WIDTH-PIXELS + 1.

    btnClipboard:Y = btnResizeVer:Y + btnResizeVer:HEIGHT-PIXELS.
    btnNextQuery:Y = btnClipboard:Y.
    btnQueries:Y   = btnClipboard:Y.
    btnPrevQuery:Y = btnClipboard:Y.
    btnClear:Y     = btnClipboard:Y.
    btnViewData:Y  = btnClipboard:Y.

    /* And align editor to the left of button btnViewData */
    btnWhere:X = brTables:X.
    btnWhere:Y = btnClipboard:Y.
    ficWhere:WIDTH-PIXELS = btnViewData:X - ficWhere:X - 1.
    ficWhere:Y = btnClipboard:Y.

    /* Buttons for field moving */
    btnMoveUp:X       = rctQuery:WIDTH-PIXELS - 25.
    btnMoveDown:X     = rctQuery:WIDTH-PIXELS - 25.
    btnReset:X        = rctQuery:WIDTH-PIXELS - 25.
    btnMoveTop:X      = rctQuery:WIDTH-PIXELS - 25.
    btnMoveBottom:X   = rctQuery:WIDTH-PIXELS - 25.
  END.

  /* Positioning of browse with data */
  IF VALID-HANDLE(ghDataBrowse) THEN
  DO:
    ghDataBrowse:Y = 1. /* to safely adjust size */
    ghDataBrowse:WIDTH-PIXELS = rctData:WIDTH-PIXELS - 10.
    ghDataBrowse:HEIGHT-PIXELS = rctData:HEIGHT-PIXELS - 10 - 23. /* Extra space for filters */
    ghDataBrowse:X = rctData:X + 3.
    ghDataBrowse:Y = rctData:Y + 5 + 21. /* Extra space for filters */
  
    RUN dataScrollNotify(INPUT ghDataBrowse).
  END.

  RUN resizeFilters(0). /* Resize filterfields on table browse */
  RUN resizeFilters(1).
  RUN resizeFilters(2).

  RUN fixTooltips(c-win:HANDLE).

  RUN saveWindow.
  RUN showScrollBars(FRAME frData:HANDLE, NO, NO).
  RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO).
  setWindowFreeze(NO).

  {&timerStop}

  /* Hide rectangles */
  rctEdit:VISIBLE = FALSE. 
  rctQuery:VISIBLE = FALSE. 
  rctData:VISIBLE = FALSE. 

  APPLY "entry" TO c-win. /* dkn */

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldClearAll C-Win 
PROCEDURE filterFieldClearAll :
/*------------------------------------------------------------------------
  Name : filterFieldClearAll
  Desc : Wipe contents of all filter fields in the same group
  ----------------------------------------------------------------------*/
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
/*------------------------------------------------------------------------
  Name : filterFieldCursorDown
  Desc : Jump from filter field to browse on cursor down
  ----------------------------------------------------------------------*/
  define input parameter phFilterField  as handle      no-undo.
  define input parameter phBrowseField  as handle      no-undo.

  define buffer bColumn for ttColumn. 

  /* Remember the field we escaped from */
  find bColumn where bColumn.hFilter = phFilterField no-error.
  if available bColumn then gcLastDataField = bColumn.cFullName.

  apply 'leave' to phFilterField.
  apply 'entry' to phBrowseField.

  return no-apply.

end procedure. /* filterFieldCursorDown */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldEntry C-Win 
PROCEDURE filterFieldEntry :
/*------------------------------------------------------------------------
  Name : filterFieldEntry
  Desc : Set the color for the text in the filter to black
  ----------------------------------------------------------------------*/
  define input  parameter phFilterField as handle      no-undo.

  /* If you enter the field and you have not put in a filter, 
   * clear out the field so you can type something yourself
   */
  if phFilterField:screen-value = phFilterField:private-data then
    phFilterField:screen-value = ''.

  setFilterFieldColor(phFilterField).

  /* Remember that we were in this filterfield, 
   * aka "Killroy was here"
   */
  ghLastFilterField = phFilterField.

end procedure. /* filterFieldEntry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldLeave C-Win 
PROCEDURE filterFieldLeave :
/*------------------------------------------------------------------------
  Name : filterFieldLeave
  Desc : Set the color for the text in the filter to gray 
  ----------------------------------------------------------------------*/
  define input parameter phFilterField as handle      no-undo.

  /* If nothing in the filter, restore the shadow text */
  if   phFilterField:screen-value = '' 
    or phFilterField:screen-value = ? then phFilterField:screen-value = phFilterField:private-data.

  setFilterFieldColor(phFilterField).

end procedure. /* filterFieldLeave */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldScrollNotify C-Win 
PROCEDURE filterFieldScrollNotify :
/*------------------------------------------------------------------------
  Name         : filterFieldScrollNotify
  Description  : Catch CURSOR-LEFT and CURSOR-RIGHT actions on the browse
  ------------------------------------------------------------------------- */
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
    cButtons = SUBSTITUTE('&1,&2', btnClearDataFilter:HANDLE, btnDataFilter:HANDLE).
  END.

  /* Resize them */
  RUN resizeFilterFields
    ( INPUT cFilterFields
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
/*------------------------------------------------------------------------
  Name         : filterFieldShow
  Description  : Show or hide a filter field
  ------------------------------------------------------------------------- */
  
  DEFINE INPUT PARAMETER phColumn AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER phFilter AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER phMenu   AS HANDLE NO-UNDO.

  phColumn:VISIBLE = phMenu:CHECKED.
  setRegistry("DataDigger:Fields", SUBSTITUTE("&1:Visible", phColumn:NAME), STRING(phMenu:CHECKED) ).
  RUN resizeFilters(1). /* tables  */

END PROCEDURE. /* filterFieldShow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE filterFieldValueChanged C-Win 
PROCEDURE filterFieldValueChanged :
/*------------------------------------------------------------------------
  Name : filterFieldValueChanged
  Desc : Save current filter value
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER phFilterField   AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER plRefreshBrowse AS LOGICAL     NO-UNDO.

  setFilterFieldColor(phFilterField).

  IF plRefreshBrowse THEN
    RUN setTimer("timedFieldFilter", 300). 

END PROCEDURE. /* filterFieldValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE fixTooltips C-Win 
PROCEDURE fixTooltips :
/*------------------------------------------------------------------------------
  Name : fixTooltips
  Desc : Replace # in tooltips with a CHR(10)
------------------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER phParent AS HANDLE NO-UNDO.
  DEFINE VARIABLE hWidget AS HANDLE NO-UNDO.

  hWidget = phParent:FIRST-CHILD.

  DO WHILE VALID-HANDLE(hWidget):

    IF hWidget:TYPE = "FRAME" 
      OR hWidget:TYPE = "FIELD-GROUP" THEN RUN fixTooltips(hWidget).
    
    IF CAN-SET(hWidget,"TOOLTIP") THEN
      hWidget:TOOLTIP = REPLACE(hWidget:TOOLTIP,"#","~n").

    hWidget = hWidget:NEXT-SIBLING.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getDataQuery C-Win 
PROCEDURE getDataQuery :
/*------------------------------------------------------------------------
  Name         : getDataQuery
  Description  : Return the query that belongs to the currently shown data
  ---------------------------------------------------------------------- 
  04-02-2011 pti Created
  ----------------------------------------------------------------------*/

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
  IF INDEX(cWhere, 'BY ') > 0 THEN
    ASSIGN cSort  = SUBSTRING(cWhere,INDEX(cWhere, 'BY '))
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

END PROCEDURE. /* getDataQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getFilterQuery C-Win 
PROCEDURE getFilterQuery :
/*------------------------------------------------------------------------
  Name         : getFilterQuery
  Description  : Return a query built from fields in the filter fields
  ---------------------------------------------------------------------- 
  27-10-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE OUTPUT PARAMETER pcFilterQuery AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cOperator  AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cValue     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cValueList AS CHARACTER NO-UNDO.

  DEFINE BUFFER bField  FOR ttField.
  DEFINE BUFFER bColumn FOR ttColumn. 

  /* Collect all filters */
  FOR EACH bField WHERE bField.lShow
    , EACH bColumn WHERE bColumn.cFieldName = bField.cFieldName: 
    
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
    
    ASSIGN 
      cValue    = IF cOperator <> "" THEN TRIM(SUBSTRING(cValue, LENGTH(cOperator) + 1)) ELSE cValue
      cValue    = TRIM(cValue," '~"") /* Remove surrounding quotes */
      cOperator = REPLACE(cOperator, "!=", "<>")
      cOperator = REPLACE(cOperator, "!", "<>")
      .      

    IF bField.cDataType = "CHARACTER" then
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
      pcFilterQuery = SUBSTITUTE("&1 &2 &3(&4) = to-rowid(&5)"
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

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE hideColumn C-Win 
PROCEDURE hideColumn :
/*------------------------------------------------------------------------
  Name         : hideColumn
  Description  : Hide the current column
  ---------------------------------------------------------------------- 
  18-01-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable cColumnClicked as character   no-undo.
  define variable cColumnValue   as character   no-undo.
  define variable iExtentNr      as integer     no-undo.

  if num-entries(ghDataBrowse:private-data,chr(1)) <> 3 then return. 
  assign
    cColumnClicked = entry(1, ghDataBrowse:private-data,chr(1))
    cColumnValue   = entry(2, ghDataBrowse:private-data,chr(1))
    iExtentNr      = integer(entry(3, ghDataBrowse:private-data,chr(1)))
    .

  run showField(cColumnClicked,false).

end procedure. /* hideColumn */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE hideSettingsFrame C-Win 
PROCEDURE hideSettingsFrame :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  None required for OCX.
  Notes:       
------------------------------------------------------------------------------*/
  
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
/*------------------------------------------------------------------------
  Name         : incQueriesOfTable 
  Description  : Increment the number of queries served for a table.
                 This must be done in one move by fetching the nr 
                 from the ini file, adding one and saving it back since
                 the user could have more than one window open. 
  ----------------------------------------------------------------------
  17-11-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define input parameter pcDatabase     as character   no-undo.
  define input parameter pcTable        as character   no-undo.
  define input parameter piNumIncrement as integer no-undo. 

  define variable iQueriesServed as integer   no-undo.
  define buffer bTable for ttTable.

  /* Which table? */
  find bTable 
    where bTable.cDatabase  = pcDatabase
      and bTable.cTableName = pcTable
          no-error.
  if not available bTable then return.

  /* Current number of queries served */
  iQueriesServed = integer( getRegistry( substitute('DB:&1', pcDatabase)
                                       , substitute('&1:QueriesServed', pcTable)
                                       )
                          ).
  if iQueriesServed = ? then iQueriesServed = 0.
  iQueriesServed = iQueriesServed + piNumIncrement.

  /* Save */
  assign 
    bTable.iNumQueries = iQueriesServed
    bTable.tLastUsed   = now.

  /* Save in registry */
  setRegistry ( substitute('DB:&1', pcDatabase )
              , substitute('&1:QueriesServed', pcTable )
              , string(bTable.iNumQueries) 
              ).
  setRegistry ( substitute('DB:&1', pcDatabase )
              , substitute('&1:LastUsed', pcTable )
              , string(bTable.tLastUsed, '99/99/9999 HH:MM:SS') 
              ).

  browse brTables:refresh().

end procedure. /* incQueriesOfTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE incQueriesServed C-Win 
PROCEDURE incQueriesServed :
/*------------------------------------------------------------------------
  Name         : incQueriesServed 
  Description  : Increment the number of queries served. We need to do 
                 this in one move by fetching the nr of queries served 
                 from the ini file, adding one and saving it back since
                 the user could have more than one window open. 
  ----------------------------------------------------------------------
  02-09-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define input parameter piNumIncrement as integer no-undo. 

  define variable iQueriesServed as integer no-undo.

  {&timerStart}

  /* Number of queries served */
  iQueriesServed = integer(getRegistry("DataDigger", "QueriesServed" )).
  if iQueriesServed = ? then iQueriesServed = 0.
  iQueriesServed = iQueriesServed + piNumIncrement.
  setRegistry("DataDigger", "QueriesServed", string(iQueriesServed) ).

  run setCounter(iQueriesServed,true).

  {&timerStop}

END PROCEDURE. /* incQueriesServed */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeFilters C-Win 
PROCEDURE initializeFilters :
/*------------------------------------------------------------------------
  Name : initializeFilters
  Desc : Create filter widgets
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER phParentBrowse AS HANDLE NO-UNDO.
  DEFINE INPUT PARAMETER phFilterButton AS HANDLE NO-UNDO.
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
      ON "return"        PERSISTENT RUN applyEvent              IN THIS-PROCEDURE (phFilterButton:HANDLE,"choose").
      ON "F2"            PERSISTENT RUN applyEvent              IN THIS-PROCEDURE (phFilterButton:HANDLE,"choose").
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
/*------------------------------------------------------------------------
  Name         : initializeObject
  DescriptiON  : Initialize all kind OF things
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

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

  /* Make the color table large enough TO hold at least 25 fONts
   * Entries 0-23 are for the user and entry 24 will be used for 
   * the color defined by "ButtONFace" ON the local system. We will
   * use this color IF the user sets "use system colors"
   */
  IF COLOR-TABLE:NUM-ENTRIES < 25 THEN COLOR-TABLE:NUM-ENTRIES = 25.
  DO iColor = 0 TO COLOR-TABLE:NUM-ENTRIES - 1:
    /* Make all colors dynamic so the user can change them */
    COLOR-TABLE:SET-DYNAMIC(iColor, true).

    /* And get the last saved value from the INI file */
    cSetting = getRegistry( "DataDigger:colors", SUBSTITUTE("color&1",iColor)).
    IF NUM-ENTRIES(cSetting) = 3 THEN
    DO:
      COLOR-TABLE:SET-RED-VALUE  (iColor, INTEGER(ENTRY(1,cSetting))).
      COLOR-TABLE:SET-GREEN-VALUE(iColor, INTEGER(ENTRY(2,cSetting))).
      COLOR-TABLE:SET-BLUE-VALUE (iColor, INTEGER(ENTRY(3,cSetting))).
    END.
  END.

  /* Get the RGB value for "ButtONFace" and set color 24 */
  RUN GetSysColor(15, OUTPUT iRgbValue).
  COLOR-TABLE:SET-RGB-VALUE(24, iRgbValue).

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

  /* Set icON */
  C-Win:LOAD-ICON(getImagePath("DataDigger.ico")). 
  
  /* Where-FRAME */
  DO WITH FRAME frWhere:

    FRAME frWhere:FONT = giDefaultFONt.

    cbAnDOr:FONT = gIFixedFONt.
    cbFields:FONT = gIFixedFONt.
    cbOperator:FONT = gIFixedFONt.
    ficValue:FONT = gIFixedFONt.

    ficWhere2:FONT = gIFixedFONt.

    btnEq:FONT       = gIFixedFONt.
    btnNe:FONT       = gIFixedFONt.
    btnGt:FONT       = gIFixedFONt.
    btnLt:FONT       = gIFixedFONt.
    btnBracket:FONT  = gIFixedFONt.
    btnQt:FONT       = gIFixedFONt.
    btnAnd:FONT      = gIFixedFONt.
    btnOr:FONT       = gIFixedFONt.
    btnBegins:FONT   = gIFixedFONt.
    btnCONtains:FONT = gIFixedFONt.
    btnMatches:FONT  = gIFixedFONt.
    btnToday:FONT    = gIFixedFONt.

    btnInsert:LOAD-IMAGE(getImagePath("Add.gif")).
  END.

  /* Counter FRAME */
  DO WITH FRAME frMain:
    FRAME frMain:FONT = giDefaultFONt.
  END.


  /* Main FRAME */
  DO WITH FRAME {&FRAME-NAME}:

    /* > UI Stuff */
    FRAME frHint:FONT = giDefaultFONt.
    FRAME frHint:X = 1.
    FRAME frHint:Y = 1.
    FRAME frHint:VISIBLE = FALSE.

    /* Data FRAME */
    FRAME frData:FONT = giDefaultFONt.

    /* Show or hide TOggle box for Debug mode */
    tgDebugMode:HIDDEN  = &IF DEFINED (UIB_is_RUNning) &THEN NO. &ELSE YES. &ENDIF
    .setDebugMode(YES). /* debug-patrick */

    /* FONts */
    c-win:FONT = giDefaultFONt.
    FRAME {&FRAME-NAME}:FONT = giDefaultFONt.
    ficWhere:FONT = gIFixedFONt.

    /* Colors for odd/even data rows */
    giDataOddRowColor[1]  = getColor("DataRow:odd:fg" ).
    giDataOddRowColor[2]  = getColor("DataRow:odd:bg" ).
    giDataEvenRowColor[1] = getColor("DataRow:even:fg").
    giDataEvenRowColor[2] = getColor("DataRow:even:bg").
  
    IF getRegistry("DataDigger:colors","DataRow:UseSystem") = "YES" THEN
    ASSIGN
      giDataOddRowColor[1]  = 1
      giDataOddRowColor[2]  = 24
      giDataEvenRowColor[1] = 1
      giDataEvenRowColor[2] = 15.
    
    /* Filter box around tables, fields and indexes */
    rcTableFilter:BGCOLOR = getColor("FilterBox:bg").
    rcFieldFilter:BGCOLOR = getColor("FilterBox:bg").
    rcIndexFilter:BGCOLOR = getColor("FilterBox:bg").

    /* Num selected records */
    DO WITH FRAME frData:
      fiNumSelected:FGCOLOR = getColor("RecordCount:Selected:fg").
      fiWarning:BGCOLOR     = getColor("WarningBox:bg").
      fiWarning:FGCOLOR     = getColor("WarningBox:fg").
      btnClearDataFilter:VISIBLE = (NUM-DBS > 0).
    END.

    /* Load images for buttONs */
    DO WITH FRAME frSettings:
      FRAME frSettings:FONT = giDefaultFONt.

      btnSettings:LOAD-IMAGE      (getImagePath("Settings.gif")).
      btnCONnectiONs:LOAD-IMAGE   (getImagePath("CONnectiONs.gif")).
      btnDict:LOAD-IMAGE          (getImagePath("DictiONary.gif")).
      btnProcEdit:LOAD-IMAGE      (getImagePath("Editor.gif")).
      btnDump-2:LOAD-IMAGE        (getImagePath("DOwnload.gif")).
      btnDump-2:LOAD-IMAGE-inSENSITIVE (getImagePath("DOwnload_Ins.gif")).
      btnLoad-2:LOAD-IMAGE        (getImagePath("Upload.gif")).
      btnLoad-2:LOAD-IMAGE-inSENSITIVE (getImagePath("Upload_Ins.gif")).
      btnQueries-3:LOAD-IMAGE     (getImagePath("SavedQueries.gif")).
      btnQueryTester:LOAD-IMAGE   (getImagePath("QTester.gif")).
      btnChangeLog:LOAD-IMAGE     (getImagePath("ReleaseNotes.gif")).
      btnAbout:LOAD-IMAGE         (getImagePath("About.gif")).
      btnResizeVer:LOAD-IMAGE     (getImagePath("ResizeVer.gif")).

      /* Give the txt buttONs a fixed fONt */
      btnSettings-txt:FONT    = gIFixedFONt.
      btnCONnectiONs-txt:FONT = gIFixedFONt.
      btnDict-txt:FONT        = gIFixedFONt.
      btnProcEdit-txt:FONT    = gIFixedFONt.
      btnDump-txt:FONT        = gIFixedFONt.
      btnLoad-txt:FONT        = gIFixedFONt.
      btnQueries-txt:FONT     = gIFixedFONt.
      btnQueryTester-txt:FONT = gIFixedFONt.
      btnChangeLog-txt:FONT   = gIFixedFONt.
      btnAbout-txt:FONT       = gIFixedFONt.

      /* Disable these WHEN ReaDONlyDigger */
      IF ReaDONlyDigger THEN
        ASSIGN
          btnDict        :SENSITIVE = FALSE
          btnDict-txt    :SENSITIVE = FALSE
          btnProcEdit    :SENSITIVE = FALSE
          btnProcEdit-txt:SENSITIVE = FALSE
        .

      /* Set sizes for this FRAME */
      FRAME frSettings:WIDTH-PIXELS  = btnAbout-txt:X + btnAbout-txt:WIDTH-PIXELS + 4.
      FRAME frSettings:HEIGHT-PIXELS = btnAbout-txt:Y + btnAbout-txt:HEIGHT-PIXELS + 4.

    END.

    btnDataDigger:LOAD-IMAGE      (getImagePath("DataDigger24x24.gif")).
    btnTools:LOAD-IMAGE           (getImagePath("Tools.gif")).
    btnHelp:LOAD-IMAGE            (getImagePath("Help.gif")).
    
    btnTableFilter:LOAD-IMAGE     (getImagePath("FilterCombo.gif")).
    btnClearTableFilter:LOAD-IMAGE(getImagePath("Clear.gif")).
    btnTableView:LOAD-IMAGE       (getImagePath("Tables.gif")).

    btnFieldFilter:LOAD-IMAGE     (getImagePath("Filter.gif")).
    btnClearFieldFilter:LOAD-IMAGE(getImagePath("Clear.gif")).

    btnIndexFilter:LOAD-IMAGE     (getImagePath("Filter.gif")).
    btnClearIndexFilter:LOAD-IMAGE(getImagePath("Clear.gif")).

    btnDataFilter:LOAD-IMAGE      (getImagePath("Filter.gif")).
    btnClearDataFilter:LOAD-IMAGE (getImagePath("Clear.gif")).

    btnViewData:LOAD-IMAGE        (getImagePath("Execute.gif")).
    btnClear:LOAD-IMAGE           (getImagePath("Clear.gif")).
    btnPrevQuery:LOAD-IMAGE       (getImagePath("PrevQuery.gif")).
    btnQueries:LOAD-IMAGE         (getImagePath("SavedQueries_small.gif")).
    btnNextQuery:LOAD-IMAGE       (getImagePath("NextQuery.gif")).
    btnClipboard:LOAD-IMAGE       (getImagePath("Clipboard.gif")).

    /* Same buttONs ON editor FRAME */
    btnViewData-2:LOAD-IMAGE      (getImagePath("Execute.gif")).
    btnClear-2:LOAD-IMAGE         (getImagePath("Clear.gif")).
    btnPrevQuery-2:LOAD-IMAGE     (getImagePath("PrevQuery.gif")).
    btnQueries-2:LOAD-IMAGE       (getImagePath("SavedQueries_small.gif")).
    btnNextQuery-2:LOAD-IMAGE     (getImagePath("NextQuery.gif")).
    btnClipboard-2:LOAD-IMAGE     (getImagePath("Clipboard.gif")).

    btnMoveTop:LOAD-IMAGE         (getImagePath("First.gif")).
    btnMoveUp:LOAD-IMAGE          (getImagePath("Up.gif")).
    btnReset:LOAD-IMAGE           (getImagePath("Reset.gif")).
    btnMoveDOwn:LOAD-IMAGE        (getImagePath("DOwn.gif")).
    btnMoveBottom:LOAD-IMAGE      (getImagePath("Last.gif")).

    /* Add/clONe etc */
    btnAdd:LOAD-IMAGE             (getImagePath("Add.gif")).
    btnClONe:LOAD-IMAGE           (getImagePath("ClONe.gif")).
    btnEdit:LOAD-IMAGE            (getImagePath("Edit.gif")).
    btnDelete:LOAD-IMAGE          (getImagePath("Delete.gif")).

    btnDump:LOAD-IMAGE            (getImagePath("Dump.gif")).

    btnMoveUp:MOVE-TO-TOP().
    btnMoveDOwn:MOVE-TO-TOP().
    btnReset:MOVE-TO-TOP().
    btnMoveTop:MOVE-TO-TOP().
    btnMoveBottom:MOVE-TO-TOP().

    /* Handle to the browse with fields of a file */
    ghFieldBrowse = brFields:HANDLE IN FRAME {&FRAME-NAME}.
    
    /* Set minimum size of the window */
    C-Win:MIN-WIDTH-PIXELS  = 650.
    C-Win:MIN-HEIGHT-PIXELS = 460. 

    /* To avoid scrollbars on the frame */
    FRAME {&FRAME-NAME}:SCROLLABLE = FALSE.

    /* Additional TOOLTIPs */
    ficValue     :TOOLTIP = ficValue:TOOLTIP + "~n~n(CTRL-ENTER) execute".
    ficWhere     :TOOLTIP = ficWhere:TOOLTIP + "~n~n(CTRL-ENTER) execute".
    brFields     :TOOLTIP = brFields:TOOLTIP + "~n~n(RIGHT-CLICK) insert field+value".
    brFields     :TOOLTIP = brFields:TOOLTIP + "~n(CTRL-RIGHT-CLICK) insert field".
    brFields     :TOOLTIP = brFields:TOOLTIP + "~n(CTRL-ENTER) execute".

    fiTableFilter:TOOLTIP = fiTableFilter:TOOLTIP + "~n~n(CTRL-ENTER) execute".

    /* Create digits for the counter */
    RUN createCounter.

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

      ON "end-resize" OF hColumn PERSISTENT RUN resizeFilters IN THIS-PROCEDURE (INPUT 0). 
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

      ON "end-resize" OF hColumn PERSISTENT RUN resizeFilters IN THIS-PROCEDURE (INPUT 2). 
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

      ON "end-resize" OF hColumn PERSISTENT RUN resizeFilters IN THIS-PROCEDURE (INPUT 1). 
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

    /* Restore active page */
    iValue = INTEGER(getRegistry("DataDigger", "ActivePage" )).
    IF iValue = ? THEN iValue = 1.
    RUN setPage(iValue).

    /* Move index browse and associated filter fields TO the left.
     * Just throw "em ON a stack, the resize event will take care OF it.
     */
    fiIndexNameFilter  :X = tgSelAll:X.
    fiFlagsFilter      :X = tgSelAll:X.
    fiFieldsFilter     :X = tgSelAll:X.
    btnClearIndexFilter:X = tgSelAll:X.
    btnIndexFilter     :X = tgSelAll:X.

    /* Initialize the buttON panels TO OFF */
    setUpdatePanel("no-record").

    /* Set the view type */
    cSetting = getRegistry("DataDigger", "ViewType").
    IF cSetting <> ? THEN RUN setViewType(cSetting).

    /* Create filter fill-ins for the fields browse */
    RUN initializeFilters
      ( INPUT brFields:HANDLE
      , INPUT btnFieldFilter:HANDLE
      , INPUT btnClearFieldFilter:HANDLE
      ). 

    /* Set filters for table browse */
    RUN resizeFilters (INPUT 0). 

    RUN getTables(OUTPUT TABLE ttTable).
    /* < UI Stuff */

    /* 
     * > Restore 
     */

    /* Window position and size */
    iValue = INTEGER(getRegistry("DataDigger", "Window:X" )).
    IF iValue = ? THEN iValue = 200.

    /* Keep DD ON primary mONitor ? (Rob Willoughby) */
    IF LOGICAL(getRegistry("DataDigger","StartONPrimaryMONitor")) = YES
      AND (iValue < 0 or iValue > SESSION:WORK-AREA-WIDTH-PIXELS) THEN iValue = 200.

    ASSIGN c-win:X = iValue NO-ERROR.

    /* Window has been parked at y=-1000 TO get it out OF sight */
    iValue = INTEGER(getRegistry("DataDigger", "Window:Y" )).
    PUBLISH "debugMessage" (1, SUBSTITUTE("window:y from reg = &1", iValue)).
    IF iValue < 0 or iValue = ? or iValue > SESSION:WORK-AREA-HEIGHT-PIXELS THEN iValue = 200.
    ASSIGN c-win:Y = iValue NO-ERROR.
    PUBLISH "debugMessage" (1, SUBSTITUTE("Reset window to y = &1", iValue)).

    iValue = INTEGER(getRegistry("DataDigger", "Window:height" )).
    IF iValue = ? or iValue = 0 THEN iValue = 600.
    ASSIGN c-win:HEIGHT-PIXELS = iValue NO-ERROR.

    iValue = INTEGER(getRegistry("DataDigger", "Window:width" )).
    IF iValue = ? or iValue = 0 THEN iValue = 800.
    ASSIGN c-win:WIDTH-PIXELS = iValue NO-ERROR.

    /* Resize bar */
    iValue = INTEGER(getRegistry("DataDigger", "ResizeBar:Y" )).
    IF iValue = ? OR iValue < 150 THEN iValue = 150.
    IF iValue > (C-Win:HEIGHT-PIXELS - 180) THEN iValue = C-Win:HEIGHT-PIXELS - 180.
    ASSIGN btnResizeVer:Y = iValue.

    /* Number OF queries served */
    iValue = INTEGER(getRegistry("DataDigger", "QueriesServed" )).
    IF iValue = ? THEN iValue = 0.
    RUN setCounter(iValue,FALSE).
    
    /* Get all cONnected databases */
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

    /* Show or hide hidden tables */
    cSetting = getRegistry("DataDigger", "ShowHiddenTables").
    IF cSetting <> ? THEN MENU-ITEM m_Show_hidden_tables:CHECKED IN MENU POPUP-MENU-brTables = LOGICAL(cSetting).
    
    /* Get last used database from registry */        
    cSetting = getRegistry("DataDigger","Database").    
    IF cSetting = "<empty>" or cSetting = ? THEN cSetting = "".

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
    RUN setDbCONtext(INPUT (IF cbDatabaseFilter:SCREEN-VALUE = ? THEN "<empty>" ELSE cbDatabaseFilter:SCREEN-VALUE)).
    /* < Restore  */

    /* timedScrollNotIFy */
    /* RUN setTimer("timedScrollNotIFy", 100). */

    /* KeepAlive timer */
    IF LOGICAL(getRegistry("DataDigger", "KeepAlive")) THEN
      RUN setTimer("KeepAlive", 300000).
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
/*------------------------------------------------------------------------
  Name         : initializeSettingsFile
  Description  : Initialize the settings file
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE cProgDir     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cEnvironment AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iColumn      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE hColumn      AS HANDLE      NO-UNDO.

  {&timerStart}

  /* Find out where DataDigger is installed and how we"re logged on */
  cProgDir = getProgramDir().

  /* If the general ini file does not exist, create it */
  if search(cProgDir + "DataDigger.ini") = ? THEN
  do:
    output to value(cProgDir + "DataDigger.ini").
    output close. 
  end.
  load "DataDigger" dir cProgDir base-key "ini" no-error.
  if error-status:error THEN
    load "DataDigger" dir cProgDir new base-key "ini" no-error.


  /* Same for the helpfile (though it SHOULD exist!) */
  if search(cProgDir + "DataDiggerHelp.ini") = ? THEN
  do:
    output to value(cProgDir + "DataDiggerHelp.ini").
    output close. 
  end.
  load "DataDiggerHelp" dir cProgDir base-key "ini" no-error.
  if error-status:error THEN
    load "DataDiggerHelp" dir cProgDir new base-key "ini" no-error.


  /* Load or create personalized ini file */
  cEnvironment = substitute("DataDigger-&1", getUserName() ).

  /* If not exist, create it */
  if search(cProgDir + cEnvironment + ".ini") = ? THEN
  do:
    output to value(cProgDir + cEnvironment + ".ini").
    output close. 
  end.
  load cEnvironment dir cProgDir base-key "ini" no-error.
  if error-status:error THEN
    load cEnvironment dir cProgDir new base-key "ini" no-error.

  /*
   * Set some settings to default values 
   */
  /* Visible page is fields by default */
  IF getRegistry("DataDigger","ActivePage") = ? THEN setRegistry("DataDigger","ActivePage","1").

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
  IF getRegistry("DataDigger", "ViewType") = ? THEN run setViewType("txt"). 

  /* Column label template */
  IF getRegistry("DataDigger", "ColumnLabelTemplate") = ? THEN setRegistry("DataDigger", "ColumnLabelTemplate","&1").

  /* Option to enable / disable triggers. */
  IF getRegistry("DataDigger","EnableWriteTriggers") = ? THEN setRegistry("DataDigger","EnableWriteTriggers", "true").

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
   */
  IF getRegistry("DataDigger:Backup","BackupFileTemplate") = ? THEN 
  DO:
    setRegistry("DataDigger:Backup","BackupFileTemplate", "<DB>.<TABLE>.<TIMESTAMP>.<#>.XML").
    setRegistry("DataDigger:Backup","BackupDir"         , "<PROGDIR>\Backup\").
    setRegistry("DataDigger:Backup","BackupOnDelete"    , "YES").
  END.

  /* Updates */
  IF getRegistry("DataDigger:Update", "UpdateCheck") = ? THEN 
  DO:
    setRegistry("DataDigger:Update", "UpdateCheck"  , "0"). /* weekly check */
    setRegistry("DataDigger:Update", "UpdateUrl"    , "http://www.oehive.org/files/DataDiggerVersion.txt").
    setRegistry("DataDigger:Update", "UpdateChannel", "0"). /* default channel */
    setRegistry("DataDigger:Update", "ChmDownloadUrl"  , "https://github.com/jcaillon/3P/blob/beta/3PA/Data/DataDigger/DataDigger.chm?raw=true").
    setRegistry("DataDigger:Update", "ChmDownloadUrl2"  , "https://github.com/jcaillon/3P/blob/master/3PA/Data/DataDigger/DataDigger.chm?raw=true").
  END.

  {&timerStop}

END PROCEDURE. /* initializeSettingsFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE keepAlive C-Win 
PROCEDURE keepAlive :
/*------------------------------------------------------------------------
  Name         : keepAlive
  Description  : Do a query on the database to ensure the connection stays alive
  ----------------------------------------------------------------------
  12-09-2012 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE hBuffer   AS HANDLE  NO-UNDO.
  DEFINE VARIABLE iDatabase AS INTEGER NO-UNDO.
  
  DO iDatabase = 1 TO NUM-DBS:
    CREATE BUFFER hBuffer FOR TABLE SUBSTITUTE("&1._file",SDBNAME(iDatabase)).
    hBuffer:FIND-FIRST("",no-lock).
    DELETE OBJECT hBuffer.
  END.

END PROCEDURE. /* keepAlive */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE loadData C-Win 
PROCEDURE loadData :
/*------------------------------------------------------------------------
  Name         : loadData 
  Description  : Load records from a dumpfile 
  ---------------------------------------------------------------------- 
  18-03-2009 pti Created
  ----------------------------------------------------------------------*/
  
  run value(getProgramDir() + 'wLoadData.w')
    ( input gcCurrentDatabase
    , input gcCurrentTable
    , input ''
    ).
end procedure. /* loadData */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE menuDropDataBrowse C-Win 
PROCEDURE menuDropDataBrowse :
/*------------------------------------------------------------------------
  Name         : menuDropDataBrowse 
  Description  : Enable / disable items in the context menu
  ---------------------------------------------------------------------- 
  18-03-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable hMenuItem      as handle      no-undo.
  define variable cColumnName    as character   no-undo.
  define variable cColumnValue   as character   no-undo.
  define variable cColumnClicked as character   no-undo. 
  define variable lOk            as logical     no-undo. 
  define variable lColumnsHidden as logical     no-undo. 
  define variable iColumn        as integer     no-undo. 
  define variable hColumn        as handle      no-undo. 

  /* Select the row we clicked on */
  run selectClickedRow(ghDataBrowse, output lOk, output cColumnClicked).
  if not lOk then return. 
  setUpdatePanel('display'). /* Activate buttons */

  if num-entries( ghDataBrowse:private-data,chr(1) ) = 3 then
    assign
      cColumnName  = entry(1, ghDataBrowse:private-data,chr(1))
      cColumnValue = entry(2, ghDataBrowse:private-data,chr(1))
      .

  /* If there are hidden columns, enable the menu-item 'unhide' */
  lColumnsHidden = can-find(first ttField where /* ttField.lDataField = true and */ ttField.lShow = false).

  /* Enable/disable all current items */
  hMenuItem = ghDataBrowse:popup-menu:first-child.

  do while valid-handle(hMenuItem):
    if hMenuItem:subtype = 'normal' then 
    do:
      hMenuItem:label = hMenuItem:private-data.
      
      /* If we did not use right mouse click but shift-f10 then
       * we do not know the column name. In that case disable all 
       * menu items that do something with the column value
       */
      if cColumnClicked = '' and lookup(hMenuItem:name,'add,clone,edit,view,dump,delete') = 0 then
        hMenuItem:sensitive = false.
      else
      do with frame {&frame-name}:
        case hMenuItem:name:
          when "add"    then hMenuItem:sensitive = btnAdd:sensitive    .
          when "clone"  then hMenuItem:sensitive = btnClone:sensitive  .
          when "edit"   then hMenuItem:sensitive = btnEdit:sensitive   .
          when "view"   then hMenuItem:sensitive = btnView:sensitive   .
          when "delete" then hMenuItem:sensitive = btnDelete:sensitive .
          when "dump"   then hMenuItem:sensitive = btnDump:sensitive .
          otherwise hMenuItem:sensitive = true. 
        end case. 
      end.

      /* Entry 'Unhide Columns' is only enabled when there is at least 1 hidden column */
      if hMenuItem:name = 'unhideColumn' then 
        hMenuItem:sensitive = lColumnsHidden.
    end.

    /* if ghDataBrowse:query:num-results = 0 then hMenuItem:sensitive = no. */

    hMenuItem = hMenuItem:next-sibling.
  end.

end procedure. /* menuDropDataBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE moveField C-Win 
PROCEDURE moveField :
/*------------------------------------------------------------------------
  Name         : moveField
  Description  : Move a field up or down in the field browse. 

  ----------------------------------------------------------------------
  02-12-2009 pti Created
  ----------------------------------------------------------------------*/

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

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE pasteFromClipboard C-Win 
PROCEDURE pasteFromClipboard :
/*------------------------------------------------------------------------
  Name : pasteFromClipboard
  Desc : Paste value from clipboard to a widget
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.
  phWidget:SCREEN-VALUE = CLIPBOARD:VALUE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE preCache C-Win 
PROCEDURE preCache :
/*------------------------------------------------------------------------
  Name : preCache
  Desc : Just grab any table and create a cache for it. 
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE lDoneSomething AS LOGICAL NO-UNDO.
  DEFINE BUFFER bTable FOR ttTable.

  {&timerStart}

  /* Pre-cache tables that have been queried  
   * at least once in the last month
   */
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
/*------------------------------------------------------------------------
  Name         : moveField
  Description  : Move a field up or down in the field browse. 

  ----------------------------------------------------------------------
  02-12-2009 pti Created
  ----------------------------------------------------------------------*/

  define input parameter ipcQueryString as character no-undo. 

  define variable iLastQuery as integer no-undo. 
  define variable iWord      as integer no-undo.

  define buffer ttTestQuery for ttTestQuery. 

  /* <BEU> */
  /* FORWARD-ONLY attribute:                                                             */    
  /* Lets you avoid building result-lists for static and dynamic queries. Set to TRUE to */
  /* avoid building result-lists for queries. Set to FALSE to build result-lists for     */
  /* queries. The default is FALSE. When TRUE, you cannot use the GET PREV, GET LAST,    */
  /* REPOSITION, or BROWSE methods or statements with these queries. If you do, the AVM  */
  /* generates an error.                                                                 */
  ipcQueryString = trim(ipcQueryString).
  ipcQueryString = replace(ipcQueryString,"INDEXED-REPOSITION","").
  /* </BEU> */

  /* The highest query nr is the first record (index is descending) */
  find first ttTestQuery no-error.
  iLastQuery = (if available ttTestQuery then ttTestQuery.iId else 0) + 1.

  /* Don't save this one if its already in the tt 
   * Just move it up the stack.
   */
  find first ttTestQuery where ttTestQuery.cQuery = ipcQueryString no-error.
  if available ttTestQuery then 
  do:
    ttTestQuery.iId = iLastQuery.
    return. 
  end.

  /* Save this query */
  create ttTestQuery.
  assign 
    ttTestQuery.iId       = iLastQuery
    ttTestQuery.cProgName = "" 
    ttTestQuery.cQuery    = ipcQueryString
    .

  /* Find table name in the query */
  findTable:
  do iWord = 1 to num-entries(ttTestQuery.cQuery," "):
    if can-do("EACH,LAST,FIRST", entry(iWord,ttTestQuery.cQuery," ")) then 
    do:
      ttTestQuery.cProgName = entry(iWord + 1,ttTestQuery.cQuery," ").
      leave findTable.
    end.
  end. 

end procedure. /* processQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenDataBrowse C-Win 
PROCEDURE reopenDataBrowse :
/*------------------------------------------------------------------------
  Name         : reopenDataBrowse
  Description  : Build the query, based on where-box and filter fields

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  04-02-2011 pti Restructured, merged with some other procedures.
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER pcSortField AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER plAscending AS LOGICAL     NO-UNDO.

  DEFINE VARIABLE cBaseQuery     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabase      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFullTable     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldSort       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTable         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cUserQuery     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBufferDB      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery         AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iNumRecords    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iQueryTime     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartTime     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iWord          AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lAscending     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lCountComplete AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lPrepare       AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE rCurrentRecord AS ROWID       NO-UNDO.
  
  DEFINE BUFFER bField  FOR ttField. 
  DEFINE BUFFER bColumn FOR ttColumn.

  /* In case we come from a filter field, that field needs
   * to have the leave event to restore the shadow text.
   */
  if valid-handle(focus) then
    APPLY "LEAVE" TO FOCUS.

  {&timerStart}

  /* Increase query counter */
  cDatabase  = gcCurrentDatabase.
  cTable     = gcCurrentTable.
  cFullTable = cDatabase + '.' + cTable.

  /* Do this before the window freeze */
  RUN incQueriesOfTable(cDatabase, cTable, +1).
  RUN incQueriesServed(+1).
  PROCESS EVENTS. /* to prevent a visual hick-up in the counter */

  /* Freeze! */
  SESSION:SET-WAIT-STATE('general'). 
  setWindowFreeze(YES). 

  /* If the user has changed a format in the field browse, then rebuild the data browse */
  IF glFormatChanged THEN
  DO:
    RUN reopenDataBrowse-create(INPUT cDatabase, INPUT cTable).
    glFormatChanged = FALSE. 
  END.

  /* Clean up existing dynamic stuff */
  if valid-handle(ghDataBrowse) then
  do:
    /* Remember record we're on */
    if ghDataBrowse:num-selected-rows > 0 then 
      rCurrentRecord = ghDataBrowse:query:get-buffer-handle(1):rowid.

    /* Find out what the current sort is */
    cBaseQuery = ghDataBrowse:query:prepare-string.
    lAscending = (lookup('DESCENDING',cBaseQuery,' ') = 0).
    iWord = lookup("BY",cBaseQuery," ").
    if iWord > 0 then cOldSort = entry(iWord + 1,cBaseQuery," ").

    if pcSortField = cOldSort then
      case lAscending:
        when true  then assign lAscending = false.
        when false then assign lAscending = ? pcSortField = ?.
        when ?     then assign lAscending = true.
      end case. 
    else
      lAscending = true.

    /* Sort direction might be overruled */
    if plAscending <> ? then lAscending = plAscending.
  end.

  /* If we do a query on the _lock table then create and fill a temp-table */
  if cTable = '_lock' then
  do:
    cFullTable = '_Lock'. 

    /* Empty the Lock TT */
    ghDataBuffer:empty-temp-table().

    create buffer hBufferDB for table cDatabase + '._lock'.
    create query hQuery.
    hQuery:add-buffer(hBufferDB).
    hQuery:query-prepare(substitute('for each &1._lock no-lock', cDatabase)).

    hQuery:query-open().
    repeat:
      hQuery:get-next().
      if hQuery:query-off-end then leave.
      if not hBufferDB:available then leave. 
      if hBufferDB::_Lock-Usr = ? then leave.
      ghDataBuffer:buffer-create().
      ghDataBuffer:buffer-copy(hBufferDB).
    end.
    hQuery:query-close().

    delete object hQuery.
    delete object hBufferDB.
  end.

  /* Reset query pointer */
  giQueryPointer = 1.
  RUN getDataQuery(OUTPUT cQuery).
  cUserQuery = ficWhere:screen-value in frame {&frame-name}. /* this one will be saved if it has no errors */

  cQuery = replace(cQuery, substitute("&1._lock", cDatabase), cFullTable).

  /* Sort field might be overruled when user clicks on data column header 
   * When query is: FOR EACH CUSTOMER BY CUSTNUM but we click on the ADDRESS 
   * column, the query should be rewritten to FOR EACH CUSTOMER BY ADDRESS
   */
  if pcSortField <> ? and pcSortField <> "" then
  do:
    if lookup('BY',cQuery,' ') > 0 then
      cQuery = substitute('&1 BY &2 &3 INDEXED-REPOSITION'
                         , trim(substring(cQuery,1,index(cQuery,' BY ')))
                         , pcSortField 
                         , string(lAscending,'/DESCENDING')
                         ).
    else
      cQuery = substitute('&1 BY &2 &3 INDEXED-REPOSITION'
                         , trim(substring(cQuery,1,index(cQuery,' INDEXED-REPOSITION')))
                         , pcSortField 
                         , string(lAscending,'/DESCENDING')
                         ).
  end.

  /* If the user has set a sort field, use that to set the sort arrow */
  iWord = lookup('BY',cQuery,' ').
  if iWord > 0 then 
    assign pcSortField = entry(iWord + 1,cQuery,' ')
           lAscending  = (lookup('DESCENDING',cQuery,' ') = 0).

  /* Set the sort arrow to the right column */
  run setSortArrow(ghDataBrowse, pcSortField, lAscending).

  /* for DWP query tester */
  publish "debugMessage" (input 1, "cQuery = " + cQuery ).
  publish 'query' (input cQuery).

  /* Try to open it */
  lPrepare = ghDataQuery:query-prepare(cQuery) no-error.

  /* if the QUERY-PREPARE failed because of the where-clause, reopen it without it */ 
  if not lPrepare then 
  do with frame {&frame-name}:
    ficWhere:bgcolor = getColor('QueryError:fg'). /* red */
    ficWhere:fgcolor = getColor('QueryError:bg'). /* yellow */

    ficWhere:tooltip = substitute('Open query failed due to this error:~n~n&1~n~nYour WHERE-clause will be ignored.'
                                 , trim(error-status:get-message(1))
                                 ).
    cQuery = substitute("for each &1 no-lock", cFullTable).
    lPrepare = ghDataQuery:query-prepare(cQuery).

    apply "entry" to ficWhere. 
  end.
  else
  do with frame {&frame-name}:
    ficWhere:bgcolor = ?. /* default */
    ficWhere:fgcolor = ?. /* default */
    ficWhere:tooltip = getReadableQuery(cQuery).

    /* Save the user-query and set the pointer to 1 */
    run saveQuery(cDatabase, cTable, cUserQuery).
  end.

  /* Try to grab as many records as we can in a limited time. 
   * This will give an indication of the amount of records. 
   */
  ghDataQuery:query-open().
  iStartTime = etime.
  do while (etime - iStartTime) < giMaxQueryTime and not ghDataQuery:query-off-end:
    ghDataQuery:get-next.
    iNumRecords = iNumRecords + 1.
  end.
  lCountComplete = ghDataQuery:query-off-end.
  iQueryTime = etime - iStartTime.

  /* query might have gotten off end, so: */
  if ghDataQuery:query-off-end then 
    ghDataQuery:query-open().

  /* Show nr of records 
   * Sometimes opening of the query takes some time so no records can be counted in 
   * the query-time-out period. So then DD shows "> 0 records" while the actual browse 
   * shows some records. This is odd, so we then set the nr of records to that of the browse. 
   */
  if ghDataBrowse:query:num-results > iNumRecords then
    iNumRecords = ghDataBrowse:query:num-results.

  setNumRecords(iNumRecords, lCountComplete, iQueryTime).
  run setNumSelectedRecords.

  /* Jump back to selected row */
  if not ghDataBrowse:query:query-off-end 
    and rCurrentRecord <> ? then
  do:
    ghDataBrowse:query:reposition-to-rowid(rCurrentRecord) no-error.
    ghDataBrowse:select-focused-row().
  end.

  rctDataFilter:VISIBLE IN FRAME frData = FALSE.
  FOR EACH bColumn:
    IF bColumn.hFilter:SCREEN-VALUE <> bColumn.cFullName
      AND bColumn.hFilter:SCREEN-VALUE <> ""
      AND bColumn.hFilter:SCREEN-VALUE <> ? THEN 
    DO:
      rctDataFilter:VISIBLE IN FRAME frData = TRUE.
      LEAVE.
    END. 
  END. 

  /* Activate buttons */
  setUpdatePanel('display').

  /* For some reasons, these #*$&# scrollbars keep coming back */
  run showScrollBars(frame {&frame-name}:handle, no, no). /* KILL KILL KILL */

  /* Unfreeze it */
  session:set-wait-state(''). 
  setWindowFreeze(no).

  {&timerStop}

  apply 'entry' to ghDataBrowse. 

end procedure. /* reopenDataBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenDataBrowse-create C-Win 
PROCEDURE reopenDataBrowse-create :
/*------------------------------------------------------------------------------
  Name : reopenDataBrowse-create
  Desc : Create the browse and open the query
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER pcDatabase AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pcTable    AS CHARACTER   NO-UNDO.

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

  PUBLISH "timerCommand" ("start", "CreateBrowse:Procedure").
  setWindowFreeze(YES).

  /* Clean up old stuff */
  PUBLISH "timerCommand" ("start", "CreateBrowse:Cleanup").
  IF VALID-HANDLE(ghDataBrowse) AND VALID-HANDLE(ghDataBrowse:QUERY) THEN DELETE OBJECT ghDataBrowse:QUERY NO-ERROR.
  IF VALID-HANDLE(ghDataBrowse) THEN DELETE OBJECT ghDataBrowse NO-ERROR.
  IF VALID-HANDLE(ghLockTable)  THEN DELETE OBJECT ghLockTable  NO-ERROR.
  IF VALID-HANDLE(ghDataBuffer) THEN DELETE OBJECT ghDataBuffer NO-ERROR.
  RUN deleteDataFilters(ghDataBrowse).
  rctDataFilter:VISIBLE IN FRAME frData = FALSE.
  PUBLISH "timerCommand" ("stop", "CreateBrowse:Cleanup").

  cFullTable = pcDatabase + "." + pcTable.

  /* For _LOCK, create a dynamic TT to prevent choking */
  PUBLISH "timerCommand" ("start", "CreateBrowse:Create").

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
      ON "CTRL-J"           PERSISTENT RUN reopenDataBrowse        IN THIS-PROCEDURE ("",?).
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
      ON "F5"               PERSISTENT RUN applyEvent              IN THIS-PROCEDURE (btnDataFilter:HANDLE,"choose").
      ON "ENTRY"            PERSISTENT RUN setTimer                IN THIS-PROCEDURE ("timedScrollNotify", 100).
      ON "LEAVE"            PERSISTENT RUN setTimer                IN THIS-PROCEDURE ("timedScrollNotify", 0).
    END TRIGGERS.

  PUBLISH "timerCommand" ("stop", "CreateBrowse:Create").

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
    PUBLISH "timerCommand" ("start", "CreateBrowse:Column").

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
        PUBLISH "timerCommand" ("start", "CreateBrowse:Column:Add").
        bColumn.hColumn = ghDataBrowse:ADD-CALC-COLUMN(bField.cDataType, bField.cFormat, "", bColumn.cFullName).
        PUBLISH "timerCommand" ("stop", "CreateBrowse:Column:Add").

        bColumn.hColumn:VISIBLE   = bField.lShow.
        bColumn.hColumn:READ-ONLY = TRUE.
        bColumn.hColumn:LABEL     = getColumnLabel(BUFFER bColumn:HANDLE).
      END.
  
      ELSE
      DO:
        /* Get handle to this field in the buffer */
        hField = ghDataBuffer:BUFFER-FIELD(bField.cFieldName):HANDLE.

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
        PUBLISH "timerCommand" ("start", "CreateBrowse:Column:Add").

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
        PUBLISH "timerCommand" ("stop", "CreateBrowse:Column:Add").

        /* Set label and name */
        bColumn.hColumn:LABEL     = getColumnLabel(BUFFER bColumn:HANDLE).
        bColumn.hColumn:NAME      = bField.cFullName.
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
      PUBLISH "timerCommand" ("start", "CreateBrowse:Column:Filter").
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
        PUBLISH "timerCommand" ("start", "CreateBrowse:Column:Filter:Menu").
  
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
  
        PUBLISH "timerCommand" ("stop", "CreateBrowse:Column:Filter:Menu").
      END. /* combo */
      PUBLISH "timerCommand" ("stop", "CreateBrowse:Column:Filter").
  
      /* triggers */
      PUBLISH "timerCommand" ("start", "CreateBrowse:Column:Triggers").
      ON "CTRL-A"           OF hFilterField PERSISTENT RUN dataSelectAll           IN THIS-PROCEDURE (ghDataBrowse).
      ON "CTRL-D"           OF hFilterField PERSISTENT RUN dataSelectNone          IN THIS-PROCEDURE (ghDataBrowse).
      ON "ENTRY"            OF hFilterField PERSISTENT RUN filterFieldEntry        IN THIS-PROCEDURE (hFilterField).
      ON "LEAVE"            OF hFilterField PERSISTENT RUN filterFieldLeave        IN THIS-PROCEDURE (hFilterField).
      ON "VALUE-CHANGED"    OF hFilterField PERSISTENT RUN filterFieldValueChanged IN THIS-PROCEDURE (hFilterField,NO).
      ON "SHIFT-DEL"        OF hFilterField PERSISTENT RUN filterFieldClearAll     IN THIS-PROCEDURE (hFilterField, btnClearDataFilter:HANDLE).
      ON "RETURN"           OF hFilterField PERSISTENT RUN applyEvent              IN THIS-PROCEDURE (btnDataFilter:HANDLE,"choose").
      ON "F2"               OF hFilterField PERSISTENT RUN applyEvent              IN THIS-PROCEDURE (btnDataFilter:HANDLE,"choose").
      ON "F5"               OF hFilterField PERSISTENT RUN applyEvent              IN THIS-PROCEDURE (btnDataFilter:HANDLE,"choose").
      ON "CTRL-CURSOR-DOWN" OF hFilterField PERSISTENT RUN filterFieldCursorDown   IN THIS-PROCEDURE (hFilterField, bColumn.hColumn).
      PUBLISH "timerCommand" ("stop", "CreateBrowse:Column:Triggers").
  
      /* Keep track of filters */
      CREATE bFilter. 
      ASSIGN 
        bFilter.cFieldName = bColumn.cFullName
        bFilter.hFilter    = hFilterField
        bFilter.hColumn    = bColumn.hColumn
        bFilter.hBrowse    = ghDataBrowse
        .
  
      /* Connect filter to field and set color */
      bColumn.hFilter = hFilterField.
      setFilterFieldColor(hFilterField).
    END. /* f/e bColumn */

    PUBLISH "timerCommand" ("stop", "CreateBrowse:Column").
  END. /* addColumnLoop */

  gcDataBrowseColumns     = trim(gcDataBrowseColumns,",").
  gcDataBrowseColumnNames = trim(gcDataBrowseColumnNames,",").

  /* Create the context menu for the databrowse if that has not been done yet */
  PUBLISH "timerCommand" ("start", "CreateBrowse:Menu").
  RUN createMenuDataBrowse.
  PUBLISH "timerCommand" ("stop", "CreateBrowse:Menu").

  /* Show the browse */
  ghDataBrowse:VISIBLE = TRUE.

  /* Limit fields to a max of 300px wide 
   * This must be done after the browse is realized
   */
  adjustFilterLoop:
  FOR EACH bColumn WHERE VALID-HANDLE(bColumn.hColumn):
    PUBLISH "timerCommand" ("start", "CreateBrowse:AdjustFilter").

    /* Get last defined width from registry. Might have been set by user */
    iColumnWidth = INTEGER( getRegistry( SUBSTITUTE("DB:&1",pcDatabase), SUBSTITUTE("&1.&2:width", pcTable, bColumn.cFullname)) ) NO-ERROR.

    /* Make sure it is not wider than 300px */
    IF iColumnWidth = ? THEN iColumnWidth = MINIMUM(300, bColumn.hColumn:WIDTH-PIXELS).

    /* Make sure the column is at least as wide as its name */
    /* And if the filter is of type COMBO, reserve some extra space for the arrow down */
    iMinWidth = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(bColumn.hColumn:LABEL,getFont("default")). 
    IF giMaxFilterHistory > 0 THEN iMinWidth = iMinWidth + 21.
    IF iColumnWidth < iMinWidth THEN iColumnWidth = iMinWidth.

    bColumn.hColumn:WIDTH-PIXELS = iColumnWidth.
    PUBLISH "timerCommand" ("stop", "CreateBrowse:AdjustFilter").
  END.
  /* Activate buttons */
  setUpdatePanel("no-record").

  /* Adjust all filters */
  PUBLISH "timerCommand" ("start", "CreateBrowse:ScrollNotify").
  RUN dataScrollNotify(ghDataBrowse).
  PUBLISH "timerCommand" ("stop", "CreateBrowse:ScrollNotify").

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

  PUBLISH "timerCommand" ("stop", "CreateBrowse:Procedure").
END PROCEDURE. /* reopenDataBrowse-create */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenFieldBrowse C-Win 
PROCEDURE reopenFieldBrowse :
/*------------------------------------------------------------------------
  Name         : reopenFieldBrowse
  Description  : Open the field browse again, taking into account the 
                 filter values the user has entered. 
                 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

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
    if pcSortField = '' then pcSortField = ?.
  
    /* Remember record we're on */
    if brFields:num-selected-rows > 0 then 
      rCurrentRecord = brFields:query:get-buffer-handle(1):rowid.
  
    /* Find out what the current sort is */
    run getColumnSort(input brFields:handle, output cOldSort, output lAscending).
  
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
    run setSortArrow(brFields:handle, cNewSort, lAscending).
  
    /* If - and only if - the sort is on 'Order', the buttons for moving are enabled */
    btnMoveUp:sensitive     = (cNewSort = "iOrder").
    btnMoveDown:sensitive   = (cNewSort = "iOrder").
    btnMoveTop:sensitive    = (cNewSort = "iOrder").
    btnMoveBottom:sensitive = (cNewSort = "iOrder").
  
    /* Close open query */
    if valid-handle(brFields:query) then brFields:query:query-close().
  
    /* Build the query */
    create query hQuery.
    create buffer hBuffer for table 'ttField'.
    hQuery:set-buffers(hBuffer).
  
    /* Initially hide red line around browse */
    rcFieldFilter:visible = false.

    cQuery = 'for each ttField where true'.
    for each bFilter 
      where bFilter.hBrowse = brFields:handle:
  
      if bFilter.hColumn:data-type = "CHARACTER" then
        assign 
          cFilterValue = getMatchesValue(bFilter.hFilter)
          cOperator    = "MATCHES".
      else 
        assign 
          cFilterValue = substitute("&1", bFilter.hFilter:screen-value)
          cOperator    = "=".
  
      /* Only add to the query if it has a real value */
      if    cFilterValue <> "" 
        and cFilterValue <> "*" 
        and cFilterValue <> ?
        and cFilterValue <> bFilter.hFilter:private-data then
      do:
        cQuery = substitute("&1 and substitute('&6',ttField.&2) &3 &4 /* &5 */"
                           , cQuery
                           , bFilter.cFieldName
                           , cOperator
                           , quoter(cFilterValue)
                           , if valid-handle(bFilter.hColumn) then bFilter.hColumn:data-type else "?"
                           , "&1"
                           ).
        /* Show red line */
        rcFieldFilter:visible = true.
      end.
    end.

    cQuery = substitute("&1 by &2 &3", cQuery, cNewSort, string(lAscending,'/descending')).

    hQuery:query-prepare(cQuery).
    hQuery:query-open().
  
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
  
    tgSelAll:CHECKED = lAllVisible.

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
    btnViewData:sensitive     = lFieldsFound. 
    btnViewData-2:sensitive in frame frWhere = lFieldsFound. 
    btnWhere:sensitive        = lFieldsFound. 
    tgSelAll:sensitive        = lFieldsFound. 
  
    btnMoveUp:sensitive       = lFieldsFound.
    btnMoveDown:sensitive     = lFieldsFound.
    btnReset:sensitive        = lFieldsFound.
    btnMoveTop:sensitive      = lFieldsFound.
    btnMoveBottom:sensitive   = lFieldsFound.
  END.

END PROCEDURE. /* reopenFieldBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenIndexBrowse C-Win 
PROCEDURE reopenIndexBrowse :
/*------------------------------------------------------------------------
  Name         : reopenIndexBrowse
  Description  : Reopen the browse with indexes. 
                 
  ----------------------------------------------------------------------
  01-09-2009 pti Created
  ----------------------------------------------------------------------*/

  define input parameter pcSortField  as character   no-undo.
  define input parameter plAscending  as logical     no-undo.

  define variable hColumn          as handle      no-undo.
  define variable hQuery           as handle      no-undo.
  define variable iColumn          as integer     no-undo.
  define variable lAscending       as logical     no-undo.
  define variable hBuffer          as handle      no-undo.
  define variable cOldSort         as character   no-undo. 
  define variable cNewSort         as character   no-undo. 
  define variable cNameFilter      as character   no-undo. 
  define variable cFlagFilter      as character   no-undo. 
  define variable cFieldsFilter    as character   no-undo. 
  define variable cQuery           as character   no-undo.
  define variable rCurrentRecord   as rowid       no-undo. 
  define variable lFieldsFound     as logical     no-undo.

  /* Set filters */
  do with frame {&frame-name}:
    cNameFilter   = getMatchesValue(fiIndexNameFilter:handle).
    cFlagFilter   = getMatchesValue(fiFlagsFilter    :handle).
    cFieldsFilter = getMatchesValue(fiFieldsFilter   :handle).
  end.

  run setRedLines.

  /* Protect routine against invalid input */
  if pcSortField = '' then pcSortField = ?.

  /* Find out what the current sort is */
  run getColumnSort(input brIndexes:handle, output cOldSort, output lAscending).

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

  /* Protection against first-time usage (in that case
   * sort is not set).
   */
  if cNewSort = "" then cNewSort = brIndexes:get-browse-column(1):name.

  /* Sort direction might be overruled */
  if plAscending <> ? then lAscending = plAscending.

  /* Wich column should have what arrow? */
  run setSortArrow(brIndexes:handle, cNewSort, lAscending).
  
  /* Remember record */
  if brIndexes:num-selected-rows > 0 then
    rCurrentRecord = brIndexes:query:get-buffer-handle(1):rowid.

  /* Build the query */
  create query hQuery.
  create buffer hBuffer for table 'ttIndex'.
  hQuery:set-buffers(hBuffer).

  cQuery = 'for each ttIndex where true'.
  if cNameFilter   <> "" and cNameFilter   <> "*" then cQuery = substitute("&1 and ttIndex.cIndexName   matches &2", cQuery, quoter(cNameFilter  )).
  if cFlagFilter   <> "" and cFlagFilter   <> "*" then cQuery = substitute("&1 and ttIndex.cIndexFlags  matches &2", cQuery, quoter(cFlagFilter  )).
  if cFieldsFilter <> "" and cFieldsFilter <> "*" then cQuery = substitute("&1 and ttIndex.cIndexFields matches &2", cQuery, quoter(cFieldsFilter)).
  cQuery = substitute("&1 by &2 &3", cQuery, cNewSort, string(lAscending,'/descending')).

  hQuery:query-prepare(cQuery).
  hQuery:query-open().
  hQuery:get-first().

  lFieldsFound = not hQuery:query-off-end.

  /* Attach query to the browse */
  brIndexes:query in frame {&frame-name} = hQuery.

  /* Jump back to selected row */
  if not hQuery:query-off-end 
    and rCurrentRecord <> ? then
  do:
    hQuery:reposition-to-rowid(rCurrentRecord) no-error.
    brIndexes:select-focused-row().
  end.

END PROCEDURE. /* reopenIndexBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenTableBrowse C-Win 
PROCEDURE reopenTableBrowse :
/*------------------------------------------------------------------------
  Name         : reopenTableBrowse
  Description  : Open the table browse again, taking into account the 
                 filter values the user has entered. 
                 
  ----------------------------------------------------------------------
  29-10-2009 pti Created
  ----------------------------------------------------------------------*/

  define input parameter pcSortField  as character   no-undo.

  define variable hColumn           as handle      no-undo.
  define variable iColumn           as integer     no-undo.
  define variable lAscending        as logical     no-undo.
  define variable lShowHiddenTables as logical     no-undo.
  define variable cOldSort          as character   no-undo. 
  define variable cNewSort          as character   no-undo. 
  define variable cTableFilter      as character   no-undo. 
  define variable cQuery            as character   no-undo.
  define variable rCurrentRecord    as rowid       no-undo. 
  define variable cTableSet         as character   no-undo.
  define variable cDatabaseFilter   as character   no-undo.
  define variable cSetting          as character   no-undo.

  define buffer bTable for ttTable. 

  do with frame {&frame-name}:

    setWindowFreeze(yes).

    /* TODO: check if the value for _dbstatus._dbstatus-cachestamp
     * has changed for one of the connected databases.
     * If so, then reread tables.
     */

    /* Get filters */
    cTableFilter      = getMatchesValue(fiTableFilter:handle).
    cDatabaseFilter   = cbDatabaseFilter:screen-value.
    lShowHiddenTables = menu-item m_Show_hidden_tables:checked in menu POPUP-MENU-brTables. /* tHiddenTables:checked. */
    
    run setRedLines.

    /* Protect routine against invalid input */
    if pcSortField = '' then pcSortField = ?.
    if cDatabaseFilter = ? then cDatabaseFilter = '*'.

    /* Remember currently selected record */
    FIND bTable 
      WHERE bTable.cDatabase   = gcCurrentDatabase
        AND bTable.cTableName  = gcCurrentTable NO-ERROR.
    IF AVAILABLE bTable THEN rCurrentRecord = ROWID(bTable).
/*     if brTables:num-selected-rows > 0 then                        */
/*       rCurrentRecord = brTables:query:get-buffer-handle(1):rowid. */

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
    if cSetting <> ? then 
      assign
        cOldSort   = brTables:get-browse-column(integer(entry(1,cSetting))):name
        lAscending = logical(entry(2,cSetting)) no-error.

    /* If no new sortfield is provided, we don't want to change the sort.
     * This happens when we press the filter button.
     */
    if pcSortField = ? then
      cNewSort = cOldSort. /* sorting stays the same */
    else
    if pcSortField = cOldSort then
    do:
      cNewSort = cOldSort.
      case lAscending:
        when true  then assign lAscending = false.           /* asc  -> desc */
        when false then assign lAscending = ? cNewSort = "". /* desc -> none */
        when ?     then assign lAscending = true.            /* none -> asc  */
      end case.
    end.
    else
      assign 
        cNewSort   = pcSortField
        lAscending = true.

    /* Protection against wrong parameters (in case of first-time usage sort is not set). */
    if cNewSort = ? then cNewSort = "".
    if lAscending = ? then lAscending = true.

    /* Wich column should have what arrow? */
    run setSortArrow(brTables:handle, cNewSort, lAscending).

    /* Close query, which may be open */
    if valid-handle(brTables:query) then brTables:query:query-close().
  
    /* Build the query */
    if not valid-handle(ghTableQuery) then
    do:
      create query ghTableQuery.
      create buffer ghTableBuffer for table 'ttTable'.
      ghTableQuery:set-buffers(ghTableBuffer).
    end.

    /* Base query */
    cQuery = substitute('FOR EACH ttTable WHERE ttTable.lShowInList = TRUE').

    /* Start with tables of the selected database (if set) */
    if cDatabaseFilter <> "*" then
      cQuery = substitute('&1 AND cDatabase MATCHES &2', cQuery, quoter(cDatabaseFilter)).

    /* Then only the tables that match the table name filter (if set) */
    if cTableFilter <> "*" then
      cQuery = substitute("&1 AND cTableName MATCHES &2", cQuery, quoter(cTableFilter)).

    /* But don't show the hidden tables (unless set) */
    if not lShowHiddenTables then 
      cQuery = substitute("&1 AND lHidden = FALSE", cQuery ).

    /* Favourites or normal table list? */
    IF glShowFavourites THEN
      cQuery = substitute("&1 AND lFavourite = TRUE", cQuery ).

    /* Then proceed with the user's sort */
    if cNewSort <> "" then
      cQuery = substitute("&1 BY &2 &3", cQuery, cNewSort, string(lAscending,'/descending') ).

    /* Additional sort: tables that have been accessed at least once rank higher
     * then followed by date/time of last access
     */
    cQuery = substitute("&1 BY tLastUsed <> ? DESCENDING ", cQuery).
    cQuery = substitute("&1 BY tLastUsed DESCENDING", cQuery).

    ghTableQuery:query-prepare(cQuery).
    ghTableQuery:query-open().
  
    /* Attach query to the browse */
    brTables:query in frame {&frame-name} = ghTableQuery.

    /* Jump back to selected row */
    if not ghTableQuery:query-off-end 
      and rCurrentRecord <> ? then
    do:
      ghTableQuery:reposition-to-rowid(rCurrentRecord) no-error.
      brTables:select-focused-row().
      apply 'value-changed' to brTables. 
    end.
  end. /* do with frame */

  setWindowFreeze(no).

END PROCEDURE. /* reopenTableBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE resetFields C-Win 
PROCEDURE resetFields :
/*------------------------------------------------------------------------
  Name         : resetFields
  Description  : Reset the field order of all fields and reset databrowse
                 
  ----------------------------------------------------------------------
  29-10-2012 pti Created
  ----------------------------------------------------------------------*/

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
/*------------------------------------------------------------------------
  Name         : resizeFilters
  Description  : Redraw the filters. This is needed when the window 
                 resizes, one of the columns resizes or the user scrolls
                 in the browse.
                 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  define input parameter piPageNr as integer no-undo.

  define variable cFilterFields as character no-undo. 
  define variable cButtons      as character no-undo. 
  define variable hBrowse       as handle    no-undo.
  define variable iField        as integer   no-undo.
  define variable hField        as handle    no-undo.

  {&timerStart}

  if piPageNr = ? then piPageNr = giCurrentPage.

  do with frame {&frame-name}:

    /* Make one string of all handles */
    case piPageNr:

      when 0 then do: /* Tables */
        assign 
          cFilterFields = substitute('&1,&2'
                                    , fiTableFilter:handle
                                    , cbDatabaseFilter:handle 
                                    )
          cButtons      = substitute('&1,&2'
                                    , btnClearTableFilter:handle
                                    , btnTableFilter:handle
                                    )
          hBrowse       = brTables:handle
          .

      end. /* 0 */

      when 1 then do: /* Fields */
        assign 
          cFilterFields = gcFieldFilterHandles
          cButtons      = substitute('&1,&2'
                                    , btnClearFieldFilter:handle
                                    , btnFieldFilter:handle
                                    )
          hBrowse       = brFields:handle
          .
      end. /* 1 */

      when 2 then do: /* Indexes */
        assign 
          cFilterFields = substitute('&1,&2,&3'
                                    , fiIndexNameFilter:handle
                                    , fiFlagsFilter:handle 
                                    , fiFieldsFilter:handle  
                                    )
          cButtons      = substitute('&1,&2'
                                    , btnClearIndexFilter:handle
                                    , btnIndexFilter:handle
                                    )
          hBrowse       = brIndexes:handle
          .
      end. /* 2 */
    end case. /* giCurrentPage */

    setWindowFreeze(yes).

    /* Save current widths to registry */
    fieldLoop:
    do iField = 1 to num-entries(cFilterFields):
      hField = hBrowse:get-browse-column(iField).
  
      setRegistry('DataDigger'
                 , substitute('ColumnWidth:&1', hField:name)
                 , substitute('&1', hField:width-pixels)
                 ).
    end.

    /* Resize them */
    run resizeFilterFields
      ( input cFilterFields
      , input cButtons
      , input hBrowse
      ).
  end.

  run showScrollBars(frame {&frame-name}:handle, no, no).
  setWindowFreeze(no).

  {&timerStop}

END PROCEDURE. /* resizeFilters */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveFilterValue C-Win 
PROCEDURE saveFilterValue :
/*------------------------------------------------------------------------
  Name         : saveFilterValue
  Description  : Save the last x filter values to registry
  ----------------------------------------------------------------------
  12-11-2013 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT  PARAMETER pcDatabase AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcTable    AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcField    AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcNewValue AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cNewList   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldList   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cThisValue AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iPos       AS INTEGER     NO-UNDO.

  IF pcNewValue = "" OR pcNewValue = ? THEN RETURN. 

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
/*------------------------------------------------------------------------
  Name         : saveWindow
  Description  : Save size and position of the window.
                 
  ----------------------------------------------------------------------
  25-03-2011 pti Created
  ----------------------------------------------------------------------*/

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
/*------------------------------------------------------------------------
  Name         : selectClickedRow
  Description  : Select the row the user last clicked on

  ----------------------------------------------------------------------
  20-05-2010 pti Created
  ----------------------------------------------------------------------*/
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

  DEFINE buffer bColumn for ttColumn. 

  PUBLISH "debugMessage" (1, "Select Clicked Row").

  /* Get mouse position (but not if we used SHIFT-F10 for the context menu */
  IF LAST-EVENT:LABEL = 'SHIFT-F10' then
  DO:
    iRow = phBrowse:FOCUSED-ROW.
    PUBLISH "debugMessage" (2, SUBSTITUTE("Pressed SHIFT-F10 on row &1", iRow)).
  END.

  ELSE /* used mouse right click */
  DO:
    RUN getMouseXY(INPUT phBrowse:FRAME, OUTPUT iMouseX, OUTPUT iMouseY).

    /* Find out what row number we clicked on */
    dRow = (iMouseY - phBrowse:y - 18) / (phBrowse:row-height-pixels + 4).
    iRow = (if dRow = integer(dRow) then integer(dRow) else truncate(dRow,0) + 1). /* ceiling of dRow */

    /* Is it a valid row nr? (could be invalid if we clicked below last record) */
    if phBrowse:num-iterations > 0
      and iRow > phBrowse:num-iterations then return. 
    if iRow < 1 then return.

    /* Get the record in the buffer */
    if phBrowse:query:num-results > 0 then 
    do:
      phBrowse:select-row(iRow).
      phBrowse:fetch-selected-row(phBrowse:num-selected-rows).
    end.

    plOk = true.

    /* Find out which column we clicked on */
    findColumn:
    DO iColumn = 1 TO phBrowse:NUM-COLUMNS:
      hBrowseColumn = phBrowse:GET-BROWSE-COLUMN(iColumn).

      IF    (iMouseX - phBrowse:X) > hBrowseColumn:X
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

    PUBLISH "debugMessage" (2, substitute("Column &1 has value &2", pcColumnName, cColumnValue)).
  END. /* used the mouse */

END PROCEDURE. /* selectClickedRow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setConnectionMenu C-Win 
PROCEDURE setConnectionMenu :
/*------------------------------------------------------------------------
  Name         : setConnectionMenu
  Description  : Rebuild the connection submenu of the 'add' button
                 
  ----------------------------------------------------------------------
  18-09-2009 pti Created
  ----------------------------------------------------------------------*/
  
  /* Attach connections to btnConnect */
  define variable hMenuItem       as handle      no-undo.
  define variable cProgDir        as character   no-undo.
  define variable cConnectionList as character   no-undo.
  define variable cDatabase       as character   no-undo.
  define variable iConn           as integer     no-undo.
  define variable hItemToDelete   as handle      no-undo. 

  hMenuItem = brTables:popup-menu:first-child in frame {&frame-name}.
  cProgDir = getProgramDir().

  /* Remove all current items except first 3 */
  do while valid-handle(hMenuItem):
    if hMenuItem:dynamic then hItemToDelete = hMenuItem.
    hMenuItem = hMenuItem:next-sibling.
    if valid-handle(hItemToDelete) then 
      delete object hItemToDelete.
  end.
  
  /* Get list of connections */
  run value(cProgDir + 'wConnections.w') 
    ( input 'getConnections'
    , input ''
    , output cConnectionList
    ).

  /* And add them to the menu */
  do iConn = 1 to num-entries(cConnectionList):
    cDatabase = entry(iConn,cConnectionList).

    /* Skip if already connected */
    if not connected(cDatabase) then 
      create menu-item hMenuItem
        assign
          label  = cDatabase
          name   = cDatabase
          parent = brTables:popup-menu
        triggers:
          on 'CHOOSE':U persistent run connectDatabase in this-procedure (cDatabase).
        end triggers.
  end. /* do iConn */

end procedure. /* setConnectionMenu */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setCounter C-Win 
PROCEDURE setCounter :
/*------------------------------------------------------------------------
  Name         : setCounter
  Description  : Set the counter to a certain number with small animation
  ----------------------------------------------------------------------
  25-01-2011 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE input PARAMETER piCounter  AS integer NO-UNDO.
  define input parameter plAnimated as logical no-undo. 

  DEFINE VARIABLE cNewDigit AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldDigit AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE iMove      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iPos       AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartingY AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStartTime AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNumDigits AS INTEGER     no-undo initial {&numDigits}.
  
  {&timerStart}

  if piCounter < 0 or piCounter = ? then piCounter = 0.
  piCounter = piCounter modulo 1000000.

  do with frame frMain:
    iStartingY = ghNewDigit[iNumDigits]:y.
    cNewDigit = string(piCounter,fill('9',iNumDigits)).
    cOldDigit = string(maximum(0,piCounter - 1), fill('9',iNumDigits)).
  
    /* Prepare the screen */
    do iPos = 1 to iNumDigits:
      /* Set digits on proper line with proper height and label */
      ghOldDigit[iPos]:y = iStartingY.
      ghNewDigit[iPos]:y = iStartingY.
      ghOldDigit[iPos]:height-pixels = {&digitHeight}.
      ghNewDigit[iPos]:height-pixels = {&digitHeight}.
      ghOldDigit[iPos]:screen-value = substring(cOldDigit,iPos,1).
      ghNewDigit[iPos]:screen-value = substring(cNewDigit,iPos,1).
      
      /* Hide leading zeros of old value */
      ghOldDigit[iPos]:visible = integer(substring(cOldDigit,1,iPos)) > 0.
      ghNewDigit[iPos]:visible = no.
    end.

    if plAnimated then
    do iMove = 1 to {&digitHeight} by 1:
      do iPos = 1 to iNumDigits :

        /* Only move the digit when it's changed */
        if substring(cOldDigit,iPos,1) = substring(cNewDigit,iPos,1) then next.

        /* Move the old number down, making it smaller */
        ghOldDigit[iPos]:y             = iStartingY + iMove.
        ghOldDigit[iPos]:height-pixels = maximum(1,{&digitHeight} - iMove).
        ghOldDigit[iPos]:visible       = (iMove < {&digitHeight}).

        /* And insert the new number, making it larger */
        ghNewDigit[iPos]:height-pixels = iMove.
        ghNewDigit[iPos]:visible       = yes.
      end.

      /* Enforce a small pause */
      RUN doNothing(15).
    end.

    /* Reset all images to their proper place */
    do iPos = 1 to iNumDigits:

      /* Show old value at the location of the digits */
      ghOldDigit[iPos]:y = iStartingY.
      ghNewDigit[iPos]:y = iStartingY.

      ghOldDigit[iPos]:height-pixels = {&digitHeight}.
      ghNewDigit[iPos]:height-pixels = {&digitHeight}.

      /* Hide leading zeros of old value */
      ghOldDigit[iPos]:visible      = no.
      ghOldDigit[iPos]:screen-value = ghNewDigit[iPos]:label.
      ghNewDigit[iPos]:visible      = integer(substring(cNewDigit,1,iPos)) > 0.
      ghNewDigit[iPos]:tooltip      = substitute('served &1 queries~n... and counting', piCounter).
    end.
  end.

  /* For some reasons, these #*$&# scrollbars keep coming back */
  run showScrollBars(frame frMain:handle, no, no). /* KILL KILL KILL */
  {&timerStop}

END PROCEDURE. /* setCounter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setDataBrowseColumns C-Win 
PROCEDURE setDataBrowseColumns :
/*------------------------------------------------------------------------------
  Name : setDataBrowseColumns
  Desc : Set all columns according to their iColumnNr
------------------------------------------------------------------------------*/

  DEFINE VARIABLE iOldPos  AS INTEGER NO-UNDO.

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

  setWindowFreeze(FALSE).

END PROCEDURE. /* setDataBrowseColumns */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setDataFilter C-Win 
PROCEDURE setDataFilter :
/*------------------------------------------------------------------------
  Name         : setDataFilter
  Description  : Optionally clear the filters and set a filter value
  ----------------------------------------------------------------------
  18-09-2009 pti Created
  ----------------------------------------------------------------------*/
  
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

    RUN reopenDataBrowse('',?).
  END. 

  setWindowFreeze(NO).

END PROCEDURE. /* setDataFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setDbContext C-Win 
PROCEDURE setDbContext :
/*------------------------------------------------------------------------
  Name         : setDbContext 
  Description  : Fill the list of tables and get last used table name 
                 and user query from registry.
  ---------------------------------------------------------------------- 
  18-03-2009 pti Created
  ----------------------------------------------------------------------*/

  define input parameter pcDatabase as character   no-undo.

  define variable cTableList as character   no-undo.
  define variable cQuery     as character   no-undo.
  define variable cTable     as character   no-undo.

  {&timerStart}

  do with frame {&frame-name}:
    
    /* Save last used db in registry */
    if pcDatabase = '' then
      setRegistry('DataDigger', 'Database',  '<empty>').
    else
      setRegistry('DataDigger', 'Database',  pcDatabase).
    
    /* Collect list of tables in this db */
    cTableList = getTableList( pcDatabase
                             , getMatchesValue(fiTableFilter:handle)
                             , menu-item m_Show_hidden_tables:checked in menu POPUP-MENU-brTables /* tHiddenTables:checked */
                             , ?
                             , ?
                             ).

    run reopenTableBrowse(?).

    btnViewData:sensitive = false. /* Until we know there is a table, VIEW is off */
    btnViewData-2:sensitive in frame frWhere = false.
    btnWhere:sensitive    = false. 
    if valid-handle (ghDataBrowse) then ghDataBrowse:hidden = true.

    /* Restore last used table for this db. If you switch back to a database, 
     * you want to see the last used table for that database. 
     */
    if pcDatabase <> ? then
      cTable = getRegistry ('DB:' + cbDatabaseFilter:screen-value, 'table').
    else 
      assign cTable = ''.

    /* Unknown table or 'All databases' selected, then just pick the first */
    if cTable = ? or lookup(cTable,cTableList) = 0 then 
      assign cTable = entry(1,cTableList).

    /* Make sure globals are set */
    apply 'value-changed' to brTables.

    run setTableContext(input cTable).
  end.  

  {&timerStop}

end procedure. /* setDbContext */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setNumSelectedRecords C-Win 
PROCEDURE setNumSelectedRecords :
/*------------------------------------------------------------------------
  Name         : setNumSelectedRecords 
  Description  : Set the nr of selected records and set the fill-in
  ---------------------------------------------------------------------- 
  13-04-2011 pti Created
  ----------------------------------------------------------------------*/

  /* Num selected records */
  do with frame frData:
    fiNumSelected:screen-value = substitute('&1  /', string(ghDataBrowse:num-selected-rows)).
    fiNumSelected:width-pixels = font-table:get-text-width-pixels( fiNumSelected:screen-value, fiNumSelected:font) + 6.
    fiNumSelected:x = fiNumResults:x - fiNumSelected:width-pixels .
  end.
  
end procedure. /* setNumSelectedRecords */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setPage C-Win 
PROCEDURE setPage :
/*------------------------------------------------------------------------
  Name         : setPage
  Description  : Activate either the fields browse or the indexes browse. 

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER piPage AS INTEGER     NO-UNDO.

  /* If we are already on this page, then we're ready */
  IF giCurrentPage = piPage THEN RETURN.

  {&timerStart}

  /* remember page */
  giCurrentPage = piPage.
  setRegistry("DataDigger", "ActivePage", STRING(giCurrentPage)).

  setWindowFreeze(YES).

  DO WITH FRAME {&FRAME-NAME}:
    CASE piPage:

      /* FIELDS */
      WHEN 1 THEN DO:
        btnTabFields :LOAD-IMAGE( getImagePath('tab_fields_active.gif'    )).
        btnTabIndexes:LOAD-IMAGE( getImagePath('tab_indexes_inactive.gif' )).
        
        RUN showDataFilters(INPUT brFields:HANDLE, TRUE).
        tgSelAll:VISIBLE = TRUE.
        VIEW {&list-2}.
        HIDE {&list-3}.

        RUN setRedLines.

        IF NOT VALID-HANDLE(ghDataBrowse) THEN
        DO WITH FRAME frData:
          HIDE btnClearDataFilter btnDataFilter.
        END.
      END.
  
      /* INDEXES */
      WHEN 2 THEN DO:
        btnTabFields :LOAD-IMAGE( getImagePath('tab_fields_inactive.gif' )).
        btnTabIndexes:LOAD-IMAGE( getImagePath('tab_indexes_active.gif'  )).

        RUN showDataFilters(INPUT brFields:HANDLE, FALSE).
        tgSelAll:VISIBLE = FALSE.
        HIDE {&list-2}.
        VIEW {&list-3}.

        RUN setRedLines.
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
/*------------------------------------------------------------------------
  Name         : setRedLines
  Description  : Show red lines around browse when filtered

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE BUFFER bFilter FOR ttFilter. 
  DEFINE VARIABLE cFilterValue AS CHARACTER NO-UNDO.

  {&timerStart}

  DO WITH FRAME {&FRAME-NAME}:

    /* Tables */
    IF getMatchesValue(fiTableFilter:HANDLE) <> "*" OR gcFieldFilterList <> "" THEN
      rcTableFilter:VISIBLE = TRUE.
    ELSE 
      rcTableFilter:VISIBLE = FALSE.


    CASE giCurrentPage:

      /* Fields */
      WHEN 1 THEN 
      DO:
        rcFieldFilter:VISIBLE = FALSE. 

        checkFieldFilters:
        FOR EACH bFilter WHERE bFilter.hBrowse = brFields:HANDLE:

          IF bFilter.hColumn:DATA-TYPE = "CHARACTER" THEN
            ASSIGN 
              cFilterValue = getMatchesValue(bFilter.hFilter).
          ELSE 
            ASSIGN 
              cFilterValue = substitute("&1", bFilter.hFilter:screen-value).

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

      WHEN 2 THEN 
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

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTable C-Win 
PROCEDURE setTable :
/*------------------------------------------------------------------------
  Name         : setTable
  Description  : If some text is selected in the session or a text is on 
                 the clipboard, select the table with that name.
  ----------------------------------------------------------------------
  7-3-2012 pti Created
  ----------------------------------------------------------------------*/
  define input parameter pcSelectedText as character no-undo. 

  define variable cTable as character no-undo.

  publish "timerCommand" ("start", "SetTable").
     
  if pcSelectedText = ? then
  do:
    /* Look in all windows if there is any text selected. 
     * If not, look on the clipboard.
     */
    cTable = getSelectedText(session:first-child).
    if cTable = "" then cTable = trim(clipboard:value) no-error.

    /* protect against garbage */
    if cTable = ? then cTable = "".
    cTable = entry(1,cTable," ").
    if length(cTable) > 20 then cTable = "".

  end.
  else 
    cTable = pcSelectedText.

  /* Now see if we can do anything with the text */
  if cTable <> "" then
  do:
    if can-find(first ttTable where ttTable.cTableName matches '*' + cTable + '*') then
    do with frame frMain:

      session:set-wait-state("general").
      setWindowFreeze(true).
  
      fiTableFilter:screen-value = cTable.
      apply 'value-changed' to fiTableFilter.
  
      run reopenTableBrowse(?).
  
      /* If we have a full match on table name, for example when text "ORDER"
       * is selected, make sure table is set to "ORDER" and not "ORDERLINE" 
       */
      find ttTable where ttTable.cTableName = cTable no-error.
      if available ttTable then
      do:
        brTables:query:reposition-to-rowid( rowid(ttTable)).
        brTables:refresh().
      end.
      apply 'value-changed' to brTables.

      if gcCurrentTable <> "" then
      do:
        run setTableContext(input gcCurrentTable ).
        run reopenDataBrowse('',?).
      end. 
      else 
      do:
        fiTableFilter:screen-value = "".
        apply 'value-changed' to fiTableFilter.
        run reopenTableBrowse(?).
      end.                             
      
      apply 'value-changed' to brTables.
      
      setWindowFreeze(no).
      session:set-wait-state("").
    end.
  end. /* has value */

  publish "timerCommand" ("stop", "SetTable").

end procedure. /* setTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTableContext C-Win 
PROCEDURE setTableContext :
/*------------------------------------------------------------------------
  Name         : setTableContext
  Description  : Perform actions when a change of table has occurred. 
                 Reread the fields, indexes, selected fields. 
                 Change title of window
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define input parameter pcTable as character no-undo. 

  define variable cFieldList as character no-undo.
  define variable cQuery     as character no-undo. 
  define variable hEditor    as handle    no-undo. 

  do with frame {&frame-name}:
      
    if pcTable = "" then return. 
    setWindowFreeze(yes).

    /* If table has changed adjust the screen */
    if pcTable <> gcCurrentTable then
      setCurrentTable( pcTable ).

    /* Delete filters */
    run deleteDataFilters(ghDataBrowse).

    /* Disable edit panel */
    setUpdatePanel('no-record').

    /* Refill the tt with fields of this table */
    run collectFieldInfo(input pcTable).

    /* Refill the index tt */
    run collectIndexInfo(input pcTable).

    /* Get all saved queries of this table */
    run collectQueryInfo( input gcCurrentDatabase, input pcTable ).
    assign giQueryPointer = 1.

    /* If the query editor is expanded, do actions to that field */
    hEditor = getActiveQueryEditor().

    /* Give custom code a chance to alter the query */
    cQuery = "".
    publish "customQuery" (input gcCurrentDatabase, input gcCurrentTable, input-output cQuery).
    hEditor:screen-value = cQuery.

    /* Reopen the queries on Fields and Indexes */
    run reopenFieldBrowse(?,?).
    run reopenIndexBrowse(?,?).

    /* Set toggle to de/select all fields */
    tgSelAll:checked = true.

    /* Unless no field is selected */
    if getSelectedFields() = '' then tgSelAll:checked = false.

    /* Get a list of all fields (extents NOT expanded) */
    for each ttField by ttField.cFieldName by ttField.iExtent:
      cFieldList = cFieldList + ',' + ttField.cFullname.
    end.

    do with frame frWhere:
      /* Set list of fields in field combo */
      cbFields:list-items     = cFieldList.
      cbAndOr:screen-value    = entry(1,cbAndOr:list-items).
      cbFields:screen-value   = entry(1,cbFields:list-items).
      cbOperator:screen-value = entry(1,cbOperator:list-items).
    end.
    
    /* Reset query-pointer */
    assign giQueryPointer = 0.

    fiWarning:visible = no.
    ficWhere:bgcolor = ?. /* default */
    ficWhere:fgcolor = ?. /* default */
    ficWhere:tooltip = ''.

    /* Save last used table and position in browse in registry */
    setRegistry ("DB:" + gcCurrentDatabase, "table", pcTable ).
    
    run setWindowTitle.

    /* Create a browse for this table */
    run reopenDataBrowse-create(input gcCurrentDatabase, input pcTable).

    setWindowFreeze(no).
  end.
end procedure. /* setTableContext */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTableFilterOptions C-Win 
PROCEDURE setTableFilterOptions :
/*------------------------------------------------------------------------
  Name         : setTableFilterOptions
  Description  : Filter tables based on whether they hold certain fields
  ----------------------------------------------------------------------
  22-10-2012 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE cFilter     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldTable   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lFirstUsage AS LOGICAL     NO-UNDO.

  /* Check if this is the first time we are using it */
  lFirstUsage = getRegistry("DataDigger:Usage", "setTableFilter:numUsed") = ?.
  PUBLISH "setUsage" ("setTableFilter"). /* user behaviour */

  cFilter = gcFieldFilterList.
  DO WITH FRAME frMain:
    RUN VALUE(getProgramDir() + 'dFilter.w')
     ( INPUT-OUTPUT cFilter
     , INPUT SUBSTITUTE("x=&1,y=&2", brTables:x + 10, brTables:y + 50)
     ).
  END.

  /* Sanity check */
  IF cFilter = ? OR cFilter = "*" THEN cFilter = "".

  /* If nothing changed, then don't reset anything on the screen */
  IF cFilter = gcFieldFilterList THEN RETURN NO-APPLY.
  
  /* Set color for down-arrow */
  IF cFilter = "" THEN 
    btnTableFilter:LOAD-IMAGE(getImagePath("FilterCombo.gif")).
  ELSE 
  DO:
    btnTableFilter:LOAD-IMAGE(getImagePath("FilterComboRed.gif")).
    IF lFirstUsage THEN 
      RUN showHint(btnTableFilter:HANDLE, 1, "~nThis arrow is red to indicate you are using a filter").
  END.

  SESSION:SET-WAIT-STATE("general").
  setWindowFreeze(YES).

  gcFieldFilterList = TRIM(cFilter, ',').
  cOldTable = gcCurrentTable.

  RUN getTablesWithField(INPUT gcFieldFilterList, OUTPUT TABLE ttTable).
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
/*------------------------------------------------------------------------------
  Name : setTableView
  Desc : Set tables or favourites view
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER plFavouritesView AS LOGICAL NO-UNDO.
  DEFINE INPUT PARAMETER plFiredBySystem  AS LOGICAL     NO-UNDO.

  DEFINE VARIABLE iNumFav   AS INTEGER NO-UNDO.
  DEFINE VARIABLE lFirstRun AS LOGICAL     NO-UNDO.

  /* What view are we in? */
  glShowFavourites = plFavouritesView.

  /* Set label and tooltips according to view */
  IF glShowFavourites THEN
  DO:
    MENU-ITEM m_Toggle_as_favourite:LABEL IN MENU POPUP-MENU-brTables = "Remove from &Favourites".
    brTables:TOOLTIP IN FRAME frMain = "(F) to remove from favourites~n(CTRL-T) or (CTRL-F) to switch view".
  END. 
  ELSE 
  DO:
    MENU-ITEM m_Toggle_as_favourite:LABEL IN MENU POPUP-MENU-brTables = "Add to &Favourites".
    brTables:TOOLTIP IN FRAME frMain = "(F) to add to favourites~n(CTRL-T) or (CTRL-F) to switch view".
  END.

  /* If we swithc manually to Fav-view for the first time... */
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
    IF glShowFavourites THEN
    DO:
      btnTableView:LABEL = "F".
      btnTableView:LOAD-IMAGE(getImagePath('Star.gif')).
    END.
    ELSE 
    DO:
      btnTableView:LABEL = "T".
      btnTableView:LOAD-IMAGE(getImagePath('Tables.gif')).
    END.
    
    btnTableView:TOOLTIP = SUBSTITUTE("Currently showing &1~n(CTRL-F) = favourites ~n(CTRL-T) = tables"
                                     , TRIM(STRING(glShowFavourites,"FAVOURITES/TABLES"))
                                     ).
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
/*------------------------------------------------------------------------------
    Name : setTimer
    Desc : enable / disable a timer
  ------------------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER pcTimerProc AS CHARACTER NO-UNDO. /* name of timer */
  DEFINE INPUT PARAMETER piInterval  AS INTEGER   NO-UNDO. /* time in msec  */

  DEFINE BUFFER bTimer FOR ttTimer. 

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

  RUN setTimerInterval.

END PROCEDURE. /* setTimer */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTimerInterval C-Win 
PROCEDURE setTimerInterval :
/*------------------------------------------------------------------------------
    Name : setTimerInterval
    Desc : Set the interval of the timer so that it will tick exactly when 
           the next timed event is due.
  ------------------------------------------------------------------------------*/
  
  DEFINE BUFFER bTimer FOR ttTimer. 

  /* Find the next timer to fire */
  FOR FIRST bTimer BY bTimer.tNext:

    IF VALID-HANDLE(chCtrlFrame) THEN
    DO:
      /* How long until it should run? */
      chCtrlFrame:pstimer:INTERVAL = MAXIMUM(1,MTIME(bTimer.tNext) - MTIME(NOW)).

      /* Turn on events */
      chCtrlFrame:pstimer:ENABLED = TRUE.
    END.
  END.

END PROCEDURE. /* setTimerInterval */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setViewType C-Win 
PROCEDURE setViewType :
/*------------------------------------------------------------------------
  Name         : setViewType
  Description  : Set the type of view to view records (TXT HTML XLS)
  ----------------------------------------------------------------------
  10-09-2010 pti Created
  ----------------------------------------------------------------------*/
  
  define input  parameter pcViewType as character   no-undo.

  do with frame frMain:
    btnView:label = substitute('View:&1',pcViewType).

    case pcViewType:
      when "txt"  then btnView:load-image(getImagePath("Text.gif")).
      when "html" then btnView:load-image(getImagePath("Html.gif")).
      when "xls"  then btnView:load-image(getImagePath("Excel.gif")).
    end case.
    
    setRegistry('DataDigger', 'ViewType', pcViewType).
  end.

END PROCEDURE. /* setViewType */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setWindowTitle C-Win 
PROCEDURE setWindowTitle :
/*------------------------------------------------------------------------
  Name         : setWindowTitle
  Description  : Set the title of the DataDigger window
  ----------------------------------------------------------------------
  17-02-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable cTitle       as character no-undo.
  define variable hParent      as integer   no-undo.
  define variable hOwner       as integer   no-undo.

  /*
  ** Display the current database and table name in the windowtitle
  **
  ** DataDigger 17 - DEVELOP - sports.customer 
  */
  cTitle = substitute( "&1 &2 &3 - &4.&5 &6" 
                     , "DataDigger"
                     , "{&version}"
                     , (if session:parameter <> '' then '- ' + session:parameter else '')
                     , gcCurrentDatabase
                     , gcCurrentTable
                     , (if gcFieldFilterList <> '' then '(' + gcFieldFilterList + ')'  else '')
                     ).

  /* Add warning for read-only mode */
  if ReadOnlyDigger then cTitle = cTitle + " ** READ-ONLY **".

  /* Add warning for debug-mode */
  if glDebugMode then cTitle = cTitle + " ** DEBUG MODE **".

  C-Win:title = cTitle.

  run GetParent (c-win:hwnd, output hParent).
  run GetWindow (hParent, 4, output hOwner).
  run SetWindowTextA ( hOwner, cTitle ).

end procedure. /* setWindowTitle */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showDataFilters C-Win 
PROCEDURE showDataFilters :
/*----------------------------------------------------------------------------
  Name : showDataFilters
  Desc : Show/hide the data filters 
  ------------------------------------------------------------------------- */
  DEFINE INPUT PARAMETER phParentBrowse AS HANDLE  NO-UNDO.
  DEFINE INPUT PARAMETER plShow         AS LOGICAL NO-UNDO.

  DEFINE BUFFER bFilter FOR ttFilter. 

  {&timerStart}

  FOR EACH bFilter WHERE bFilter.hBrowse = phParentBrowse:

    IF VALID-HANDLE(bFilter.hFilter) THEN 
      bFilter.hFilter:VISIBLE = plShow.

  END.

  {&timerStop}

END PROCEDURE. /* showDataFilters */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showField C-Win 
PROCEDURE showField :
/*------------------------------------------------------------------------
  Name         : showField
  Description  : Toggle the selected status of a field. 

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER pcFieldList AS CHARACTER NO-UNDO. 
  DEFINE INPUT PARAMETER plSelected  AS LOGICAL   NO-UNDO. 
  
  DEFINE BUFFER bColumn FOR ttColumn. 
  DEFINE BUFFER bField  FOR ttField. 
  
  setWindowFreeze(yes).

  DO WITH FRAME {&FRAME-NAME}:

    FOR EACH bColumn WHERE CAN-DO(pcFieldList,bColumn.cFullName)
      , EACH bField WHERE bField.cFieldName = bColumn.cFieldName:
    
      bField.lShow = (IF plSelected = ? THEN NOT bField.lShow ELSE plSelected).
    
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
/*------------------------------------------------------------------------
  Name         : showHint
  Description  : Show a small window with a hint
  ----------------------------------------------------------------------*/
  
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
    FRAME frHint:VISIBLE = TRUE.
    FRAME frHint:MOVE-TO-TOP().
  
    btGotIt:X = 1.
    btGotIt:WIDTH = LENGTH(btGotIt:LABEL) + 4.
    btGotIt:X = (FRAME frHint:WIDTH-PIXELS / 2 - btGotIt:WIDTH-PIXELS / 2).

    CASE piLayout:
      /* point nowhere */
      WHEN 0 THEN ASSIGN 
                    iOffsetX = phWidget:WIDTH-PIXELS / 2
                    iOffsetY = phWidget:HEIGHT-PIXELS / 2.

      /* point left up */
      WHEN 1 THEN ASSIGN 
                    iOffsetX = phWidget:WIDTH-PIXELS / 3 * 2 
                    iOffsetY = phWidget:HEIGHT-PIXELS / 3 * 2.

      /* point right up */
      WHEN 2 THEN ASSIGN 
                    iOffsetX = phWidget:WIDTH-PIXELS / 3 - FRAME frHint:WIDTH-PIXELS
                    iOffsetY = phWidget:HEIGHT-PIXELS / 3 * 2.

      /* point right down */
      WHEN 3 THEN ASSIGN 
                    iOffsetX = phWidget:WIDTH-PIXELS / 3 - FRAME frHint:WIDTH-PIXELS
                    iOffsetY = phWidget:HEIGHT-PIXELS / 3 - FRAME frHint:HEIGHT-PIXELS.

      /* point left down */
      WHEN 4 THEN ASSIGN 
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
      WHEN 0 THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("DataDigger24x24.gif")).
        ASSIGN 
          imgArrow:X = 10
          imgArrow:Y = 10
          edHint:X   = imgArrow:X + imgArrow:WIDTH-PIXELS + 1
          .
      END.

      WHEN 1 THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("LeftUp.gif")).
        ASSIGN 
          imgArrow:X = 1
          imgArrow:Y = 1
          edHint:X   = imgArrow:X + imgArrow:WIDTH-PIXELS + 5
          .
      END.

      WHEN 2 THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("RightUp.gif")).
        ASSIGN 
          imgArrow:X = FRAME frHint:WIDTH-PIXELS - imgArrow:WIDTH-PIXELS - 2
          imgArrow:Y = 1
          edHint:X   = 5
          .
      END.

      WHEN 3 THEN
      DO:
        imgArrow:LOAD-IMAGE(getImagePath("RightDown.gif")).
        ASSIGN 
          imgArrow:X = FRAME frHint:WIDTH-PIXELS - imgArrow:WIDTH-PIXELS - 2
          imgArrow:Y = FRAME frHint:HEIGHT-PIXELS - imgArrow:HEIGHT-PIXELS - 2
          edHint:X   = 20
          .
      END.

      WHEN 4 THEN
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
    IF piLayout = 0 THEN
      btGotIt:LABEL = "Okidoki !".
    ELSE 
      btGotIt:LABEL = "I Got it".

    /* Animation. Needless, but fun to program :) */
    DO iStep = 1 TO 25:
      RUN doNothing(10).
      FRAME frHint:X = FRAME frHint:X + ((iTargetX - FRAME frHint:X) / 25 * iStep).
      FRAME frHint:Y = FRAME frHint:Y + ((iTargetY - FRAME frHint:Y) / 25 * iStep).
    END.
  
    edHint:SCREEN-VALUE IN FRAME frHint = pcText.

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
/*------------------------------------------------------------------------
  Name         : showNewFeatures
  Description  : Highlight some new features
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE iBarPos   AS INTEGER NO-UNDO.
  DEFINE VARIABLE iColumnNr AS INTEGER NO-UNDO.

  DEFINE BUFFER bFilter FOR ttFilter. 
  DEFINE BUFFER bColumn FOR ttColumn.
  
  demoLoop:
  DO WITH FRAME frMain:

    /* This will be checked within showHint */
    glHintCancelled = FALSE. 
  
    /* Select field columns */
    RUN showHint(brFields:GET-BROWSE-COLUMN(3), 1, "~nRight-click the column header to select which columns you want to see.").

    /* Drag bar. Set window to default sizes first */
    c-win:WIDTH-PIXELS = 800.
    c-win:HEIGHT-PIXELS = 600.
    btnResizeVer:Y = 260. 
    RUN endResize.
    RUN showHint(btnResizeVer:HANDLE, 2, "~nDrag this bar up and down to change the size of the upper browse.").
    IF glHintCancelled THEN LEAVE demoLoop.

    DO iBarPos = 260 TO 160 BY -25:
      setWindowFreeze(YES).
      btnResizeVer:Y = iBarPos.
      RUN endResize.
      setWindowFreeze(NO).
      PROCESS EVENTS. 
    END.

    /* If started without database, the data browse is not there */
    IF VALID-HANDLE(ghDataBrowse) THEN
      RUN showHint(ghDataBrowse:HANDLE, 3, "~nSo you can see more data.").
    ELSE 
      RUN showHint(btnResizeVer:HANDLE, 3, "~nSo you can see more data.").

    IF glHintCancelled THEN LEAVE demoLoop.
    
    DO iBarPos = 160 TO 410 BY 25:
      setWindowFreeze(YES).
      btnResizeVer:Y = iBarPos.
      RUN endResize.
      setWindowFreeze(NO).
      PROCESS EVENTS. 
    END.
    RUN showHint(brFields:HANDLE, 3, "~n... or more fields").
    IF glHintCancelled THEN LEAVE demoLoop.

    /* back to normal */
    btnResizeVer:Y = 260. 
    RUN endResize.

    /* Favourite tables */
    IF NOT glHintCancelled THEN 
    DO:
      RUN showHint(btnTableView:HANDLE, 1, "~nSet your favourite tables and switch between all tables and favourites view. Let's do that.").
      RUN setTableView(YES,YES).
      RUN showHint(brTables:HANDLE, 4, "~nPress right mouse button on the table browse to add tables to your list of favourites").
      RUN setTableView(NO,YES).
    END. 

    /* Filter history is saved */
    iColumnNr = 0.
    FOR EACH bColumn:
      IF NOT bColumn.hColumn:VISIBLE THEN NEXT. 
      iColumnNr = iColumnNr + 1.
      IF iColumnNr > 1 THEN 
      DO:
        RUN showHint(bColumn.hFilter, 1, "~nYour 10 last used values are automatically saved for easy re-use on the data filters").
        LEAVE.
      END.
    END.

    /* Done! */
    RUN showHint(C-Win:HANDLE, 0, "~n That's it, happy Digging!").
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

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showTour C-Win 
PROCEDURE showTour :
/*------------------------------------------------------------------------
  Name         : showTour
  Description  : Highlight some of the main features of DD
  ----------------------------------------------------------------------*/
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
    RUN showHint(C-Win:HANDLE, 0, "~nWelcome to the Digger!~n~nGet ready for the 1-minute-tour").
    IF glHintCancelled THEN LEAVE hintBlock.

    /* Select a table and show data */
    RUN setPage(1).

    RUN showHint(brTables:HANDLE     , 4, "~nThis browse shows all the tables in your connected databases").
    RUN showHint(fiTableFilter:HANDLE, 1, "~nType in (a part of) the table name to filter the browse").
    RUN showHint(btnViewData:HANDLE  , 3, "~nThen press enter or click this button to show the data in this table").

    /* Filter fields */
    RUN showHint(BROWSE brFields:GET-BROWSE-COLUMN(1), 1, "~nYou can hide fields from the data browse by simply deselecting them here").

    RUN showHint(BROWSE brFields:GET-BROWSE-COLUMN(5), 2, "~nDon't like a field format? Click on the format to change it on the fly").
    RUN showHint(btnReset:HANDLE                     , 2, "~nDon't like the field order? Change it with the buttons in this panel").

    /* Filter data */
    RUN showHint(ficWhere:handle, 4 , "~nYour custom query goes here ...").

    /* Let the hint frame point to the 2nd visible filter instead of the 1st */
    FOR EACH bColumn:
      IF NOT bColumn.hColumn:VISIBLE THEN NEXT. 
      iColumn = iColumn + 1.
      IF iColumn > 1 THEN 
      DO:
        RUN showHint(bColumn.hFilter, 1, "~nOr simply filter data by filling in one of the filter boxes. Your filters are saved for easy re-use").
        LEAVE.
      END.
    END.

    /* Confess the lie */
    RUN showHint(C-Win:HANDLE, 0, "~nOk, I lied~n~nIt takes more than 1 minute, but you're almost done now!").

    /* Resize bar */
    RUN showHint(btnResizeVer:HANDLE, 2, "~nDrag this bar up and down to change the size of the upper browse.").

    iColumn = 0.
    FOR EACH bColumn:
      IF NOT bColumn.hColumn:VISIBLE THEN NEXT. 
      iColumn = iColumn + 1.
      IF iColumn = 1 THEN 
        RUN showHint(bColumn.hColumn, 4, "~nClick on a column header to sort on that column. Again to change the sort").
      ELSE 
      IF iColumn = 2 THEN 
        RUN showHint(bColumn.hColumn, 4, "~nOr grab the side of a column to resize it. DD will remember its width on the next run").
      ELSE 
        LEAVE.
    END.

    RUN showHint(btnQueries:HANDLE, 2, "~nYour queries are saved here for re-use (hint: try PGUP/PGDN in the query box)").

    /* Manipulate data */
    RUN showHint(btnEdit:HANDLE   , 4, "~nAdd, update, clone, delete, dump records easily using these buttons").

    /* Extra options */
    RUN showHint(btnTools:HANDLE  , 1, "~nOk, last tip. Use this one for settings and extra tools").

    /* Done! */
    RUN showHint(C-Win:HANDLE, 0, "~n That's it, happy Digging!").

    FRAME frHint:VISIBLE = FALSE.
  END.

  /* Since showHint is called outside this proc as well, we need to reset it */
  glHintCancelled = FALSE. 

END PROCEDURE. /* showTour */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showValue C-Win 
PROCEDURE showValue :
/*------------------------------------------------------------------------
  Name         : showValue
  Description  : Show the value of the current cell
  ---------------------------------------------------------------------- 
  18-01-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable cColumnName  as character   no-undo.
  define variable cColumnValue as character   no-undo.

  if num-entries(ghDataBrowse:private-data,chr(1)) <> 3 then return. 

  cColumnName  = entry(1, ghDataBrowse:private-data,chr(1)).
  cColumnValue = entry(2, ghDataBrowse:private-data,chr(1)).

  if cColumnValue <> '' and cColumnValue <> ? then 
    message trim(cColumnValue)
      view-as alert-box info buttons ok.

end procedure. /* showValue */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE sortComboBox C-Win 
PROCEDURE sortComboBox :
/*------------------------------------------------------------------------
  Name : sortComboBox
  ----------------------------------------------------------------------
  13-11-2013 pti Created
  ----------------------------------------------------------------------*/
  
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
/*------------------------------------------------------------------------
  Name         : startDiggerLib
  Description  : Start DiggerLib if it has not already been started

  ----------------------------------------------------------------------
  21-10-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable hDiggerLib as handle    no-undo.
  define variable cProgDir   as character no-undo.

  file-info:file-name = this-procedure:file-name.
  cProgDir = substring(file-info:full-pathname,1,r-index(file-info:full-pathname,'\')).

  /* Call out to see if the lib has been started for this build nr */
  PUBLISH 'DataDiggerLib' (OUTPUT hDiggerLib).
  
  /* If it is not, then start it */
  if not valid-handle(hDiggerLib) then
  do:
    run value(cProgDir + 'DataDiggerLib.p') persistent set hDiggerLib.
    session:add-super-procedure(hDiggerLib,search-target).
  end.

  /* Start customizations in myDataDigger.p */
  if search(cProgDir + 'myDataDigger.p') <> ? then
  do:
    run value(cProgDir + 'myDataDigger.p') persistent set hDiggerLib.
    session:add-super-procedure(hDiggerLib, search-target).

    subscribe procedure hDiggerLib to "customFilter" anywhere.
    subscribe procedure hDiggerLib to "customDump"   anywhere.
    subscribe procedure hDiggerLib to "customQuery"  anywhere.
  end.

end procedure. /* startDiggerLib */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE startSession C-Win 
PROCEDURE startSession :
/*------------------------------------------------------------------------
  Name         : startSession
  Description  : Show a welcome message to the user.
  ----------------------------------------------------------------------
  07-09-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable cError     as character no-undo.
  define variable cBuild     as character no-undo.
  define variable iStackSize as integer   no-undo.
  define variable iVersion   as integer   no-undo.
  define variable hWindow    as handle    no-undo. 
  define variable lNewUser   as logical   no-undo. 
  define variable lNewBuild  as logical   no-undo. 
  define variable lUpgraded  as logical   no-undo. 

  /* Set debug flag */
  setDebugMode(logical(getRegistry('DataDigger:debugger','DebugMode'))).

  /* DEBUG */
/*   &IF DEFINED (UIB_is_running) &THEN */
/*     RUN clearDiskCache.              */
/*   &ENDIF                             */

  /* Check if this is the first run with a new version */
  iVersion = integer(getRegistry('DataDigger', 'Version')) no-error.
  if iVersion = ? then lNewUser = true.

  cBuild = getRegistry('DataDigger', 'Build').
  IF cBuild = ? THEN cBuild = "".
  
  /* obsolete files, only for beta-users, only for first build after 20140314 */
  OS-DELETE VALUE( "dDump.w").
  OS-DELETE VALUE( "dDump.r").
  
  IF iVersion <> {&version} THEN
  DO:
    lUpgraded = TRUE.

    /* If we come from an older version, do some conversions */
    IF iVersion <> ? THEN
    DO:
      RUN showMessage.p( INPUT "Conversion"
                       , INPUT "Please wait while your settings are converted."
                       , OUTPUT hWindow).
      
      /* Do one-time conversions if needed */
      session:set-wait-state("general").
  
      convLoop:
      repeat:
        run convertSettings(iVersion,cBuild).
        iVersion = iVersion + 1.
        if iVersion >= {&version} then leave convLoop.
      end.
      delete object hWindow. 
      session:set-wait-state("").
    end.

    /* Save this version nr */
    setRegistry('DataDigger', 'Version', '{&version}').
    setRegistry('DataDigger', 'Build', '{&build}').

    /* Assume that unblocking files is only needed on version change */
    RUN unblockFile(getProgramDir() + 'DataDigger.chm.dat').
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
  if lookup('-rereadnolock', session:startup-parameters) = 0 then 
    run showHelp('RereadNoLock', '').

  /* Check on the value for -s, should preferrably be > 128 */
  iStackSize = getStackSize().
  if iStackSize <= 128 then
    run showHelp('StackSize', string(iStackSize)).

  /* If we are a READ-ONLY digger, show a warning */
  if ReadOnlyDigger then
    run showHelp("ReadOnlyDigger", "").

  /* The user could be:
   * 1) a new user 
   * 2) existing user on upgraded DD
   * 3) existing user on non-upgraded DD
   */
  if lNewUser then run showTour.
  else if lUpgraded then run showNewFeatures.

end procedure. /* startSession */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE startTool C-Win 
PROCEDURE startTool :
/*------------------------------------------------------------------------
  Name         : startTool   
  Description  : Start Dictionary or Data Adminstration
  ----------------------------------------------------------------------
  07-05-2013 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER pcTool AS CHARACTER   NO-UNDO.

  define variable cDatabasesOld as character no-undo. 
  define variable cDatabases    as character no-undo. 

  /* Return if no db connected */
  if num-dbs = 0 then return. 

  hide frame frSettings.
  create alias dictdb for database value( gcCurrentDatabase ).

  /* Remember all connected db's */
  cDatabasesOld = getDatabaseList().

  case pcTool:
    when "Dict" then do:
      publish "setUsage" ("Dictionary"). /* user behaviour */

      RUN setTimer("resizeDictWindow", 500).
      RUN dict.p.
      RUN setTimer("resizeDictWindow", 0).
    end.

    when "Admin" then
    do:
      publish "setUsage" ("DataAdmin"). /* user behaviour */
      run _admin.p.
    end.
  end case.

  /* Get all connected databases */
  cDatabases = getDatabaseList().

  /* If needed, repopulate db combo */
  if cDatabases <> cDatabasesOld then 
  do:
    /* Get list of all tables of all databases */
    run getTables(output table ttTable).
    assign cbDatabaseFilter:list-items in frame frMain = ',' + cDatabases.

    apply 'choose' to btnTableFilter. 
  end.
  else 
    create alias dictdb for database value( gcCurrentDatabase ).

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE timedFieldFilter C-Win 
PROCEDURE timedFieldFilter :
/*------------------------------------------------------------------------------
  Name : timedFieldFilter
  Desc : Activated by the timer to apply the filter
------------------------------------------------------------------------------*/

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
/*------------------------------------------------------------------------------
  Name : timedIndexFilter
  Desc : Activated by the timer to apply the filter
------------------------------------------------------------------------------*/

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
/*------------------------------------------------------------------------------
  Name : timedScrollNotify
  Desc : Run scrollnotify when user scrolls using cursor keys
------------------------------------------------------------------------------*/
  
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
/*------------------------------------------------------------------------------
  Name : timedTableChange
  Desc : Activated by the timer to change the browse 
------------------------------------------------------------------------------*/

  setWindowFreeze(YES).
  RUN setTimer("timedTableChange", 0).
  RUN setTableContext(INPUT gcCurrentTable ).
  setWindowFreeze(NO).

END PROCEDURE. /* timedTableChange */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE timedTableFilter C-Win 
PROCEDURE timedTableFilter :
/*------------------------------------------------------------------------------
  Name : timedTableFilter
  Desc : Activated by the timer to apply the filter
------------------------------------------------------------------------------*/

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
  ( phMenu    AS handle   
  , pcType    AS CHARACTER  
  , pcLabel   AS CHARACTER 
  , pcName    AS CHARACTER 
  ) :

  define variable hMenuItem as handle no-undo.

  case pcType:
    when "SUBMENU" then 
      create sub-menu hMenuItem
        assign 
          label        = pcLabel
          private-data = pcLabel
          name         = pcName
          parent       = phMenu.

    when "TOGGLE-BOX" then 
      create menu-item hMenuItem
        assign 
          label        = pcLabel
          private-data = pcLabel
          name         = pcName
          toggle-box   = true
          checked      = true
          parent       = phMenu.

    when "RULE" then 
      create menu-item hMenuItem
        assign
          parent       = phMenu
          subtype      = "rule". 

    otherwise
      create menu-item hMenuItem
        assign
          label        = pcLabel
          private-data = pcLabel
          name         = pcName
          parent       = phMenu.

  end case.

  return hMenuItem.

END FUNCTION. /* createMenuItem */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getActiveQueryEditor C-Win 
FUNCTION getActiveQueryEditor RETURNS HANDLE
  ( /* parameter-definitions */ ) :

/*------------------------------------------------------------------------
  Name         : getActiveQueryEditor
  Description  : Return the handle of the active query editor
  ----------------------------------------------------------------------
  07-09-2009 pti Created
  ----------------------------------------------------------------------*/

  /* If the query editor is expanded, do actions to that field */
  if gcQueryEditorState = 'hidden' then 
    return ficWhere:handle in frame frMain.
  else 
    return ficWhere2:handle in frame frWhere.

end function. /* getActiveQueryEditor */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getFieldList C-Win 
FUNCTION getFieldList returns character
  ( pcSortBy as character ) :

/*------------------------------------------------------------------------
  Name         : getFieldList
  Description  : Return a comma separated list of all fields.
  ---------------------------------------------------------------------- 
  09-07-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define variable cFieldList as character  no-undo.
  define variable iMaxFields as integer    no-undo.
  define variable iNumFields as integer    no-undo.
  
  DEFINE BUFFER ttField FOR ttField.
  DEFINE QUERY qField FOR ttField.
  
  iMaxFields = INTEGER(getRegistry('DataDigger','MaxColumns')) NO-ERROR.
  IF iMaxFields = ? THEN iMaxFields = 500.

  QUERY qField:QUERY-PREPARE(SUBSTITUTE('for each ttField by &1', pcSortBy)).
  QUERY qField:QUERY-OPEN.
  QUERY qField:GET-FIRST.

  /* All fields */
  repeat while not query qField:query-off-end:
    cFieldList = cFieldList + ',' + ttField.cFieldName.
    query qField:get-next.
    iNumFields = iNumFields + 1.
    if iNumFields > iMaxFields then leave.
  end.
  query qField:query-close.

  cFieldList = left-trim(cFieldList, ",").

  return cFieldList.
end function. /* getFieldList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getQueryFromFields C-Win 
FUNCTION getQueryFromFields returns character
  ( input pcFieldList as character ):

/*------------------------------------------------------------------------
  Name         : getQueryFromFields
  Description  : Return a query built from fields in a list
  ---------------------------------------------------------------------- 
  20-10-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable cField         as character   no-undo.
  define variable iField         as integer   no-undo. 
  define variable cNameFormat    as character no-undo. 
  define variable cFieldList     as character no-undo. 
  define variable cQuery         as character no-undo. 

  /* Determine format for names */
  cNameFormat = fill('x', getMaxLength(pcFieldList) ).

  /* Build query */
  cQuery = ''.
  do iField = 1 to num-entries(pcFieldList):
    cField = entry(iField,pcFieldList).
    find ttField where ttField.cFieldName = cField.
    cQuery = substitute('&1&2 &3 = &4'
                       , cQuery 
                       , (if iField = 1 then 'where' else '~n  and') 
                       , string(cField,cNameFormat) 
                       , quoter(getLinkInfo(cField))
                       ).
  end.

  publish "debugMessage" (1,substitute('Query From Fields: &1', cQuery)).

  return cQuery.
end function. /* getQueryFromFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getSelectedFields C-Win 
FUNCTION getSelectedFields returns character
  ( /* parameter-definitions */ ) :

/*------------------------------------------------------------------------
  Name         : getSelectedFields
  Description  : Return all selected fields. 

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable cSelectedFields as character  no-undo.
  define buffer ttField for ttField.

  /* All selected fields */
  for each ttField where ttField.lShow = true 
    by ttField.iOrder:

    cSelectedFields = cSelectedFields + ',' + ttField.cFullName.
         
    if length(cSelectedFields) > 20000 then leave.
  end.

  cSelectedFields = left-trim(cSelectedFields, ",").

  return cSelectedFields.
end function. /* getSelectedFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getSelectedText C-Win 
FUNCTION getSelectedText RETURNS CHARACTER
  ( INPUT hWidget AS handle ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/
  define variable cSelectedText as character no-undo. 

  repeat while valid-handle(hWidget):

    /* SalesRep */
    if can-query(hWidget,'SELECTION-TEXT')
      and hWidget:selection-text <> '' then
    do:
      return trim(hWidget:selection-text).
    end.

    if can-query(hWidget,'first-child')
      and hWidget:first-child <> ? then 
    do:
      cSelectedText = getSelectedText(hWidget:first-child).
      if cSelectedText <> "" then return cSelectedText.
    end.

    hWidget = hWidget:next-sibling.
  end.
  
  return "".
END FUNCTION. /* getSelectedText */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION killMenu C-Win 
FUNCTION killMenu RETURNS LOGICAL
  ( phMenu AS HANDLE ) :

  /*------------------------------------------------------------------------
    Name         : killMenu
    Description  : Kill a menu and all of its siblings
    ----------------------------------------------------------------------
    26-11-2013 pti Created
    ----------------------------------------------------------------------*/

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

/*------------------------------------------------------------------------
  Name         : saveSelectedFields
  Description  : Write the selected fields to the INI
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

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
FUNCTION setCurrentTable returns logical
  ( pcTableName as character ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  define buffer bTable for ttTable.

  do with frame {&frame-name}:
    find bTable 
      where bTable.cDatabase  = gcCurrentDatabase
        and bTable.cTableName = pcTableName
            no-error.
    if not available bTable then return no.

    brTables:query:reposition-to-rowid( rowid(bTable) ) no-error.
  end.
end function. /* setCurrentTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setDebugMode C-Win 
FUNCTION setDebugMode returns logical
  ( plDebugMode as logical ) :

/*------------------------------------------------------------------------
  Name         : setDebugMode
  Description  : Turn debug mode on of off. Affects timers and LockWindow
  ----------------------------------------------------------------------*/

  if plDebugMode = ? then return no.

  glDebugMode                 = plDebugMode.
  chCtrlFrame:pstimer:ENABLED = NOT glDebugMode.

  return true.

end function. /* setDebugMode */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setNumRecords C-Win 
FUNCTION setNumRecords returns logical
  ( input piNumRecords    as integer
  , input plCountComplete as logical 
  , input piQueryMSec     as integer) :

  define variable cNumResults as character   no-undo.

  do with frame frData:
    fiNumresults:visible = true.

    if plCountComplete then 
      fiNumResults:fgcolor = getColor('RecordCount:Complete:fg'). /* green */
    else 
      fiNumResults:fgcolor = getColor('RecordCount:Incomplete:fg'). /* red */
  
    if plCountComplete then
      cNumResults = substitute(' &1 records in &2 msec', piNumRecords, piQueryMSec).
    else 
      cNumResults = substitute(' > &1 records', piNumRecords).

    fiNumResults:x = 100. /* park it to the left so we can expand it */
    fiNumResults:width-pixels = font-table:get-text-width-pixels(cNumResults) + 5.
    fiNumResults:x = rctData:x + rctData:width-pixels - fiNumResults:width-pixels - 40.
    fiNumResults:screen-value = cNumResults.
  end.

  return yes.
end function. /* setNumRecords */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setQuery C-Win 
FUNCTION setQuery RETURNS LOGICAL
  ( piPointerChange as integer ) :

/*------------------------------------------------------------------------
  Name         : setQuery
  Description  : Fetches the previous or next query from the settings 
                 and fills it in in the query editor.
                 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable cQuery  as character no-undo. 
  define variable hEditor as handle    no-undo.

  hEditor = getActiveQueryEditor().

  /* See if the requested query exists */
  cQuery = getQuery(gcCurrentDatabase, gcCurrentTable, giQueryPointer + piPointerChange).

  if cQuery <> ? then 
  do:
    giQueryPointer = giQueryPointer + piPointerChange.
    hEditor:screen-value = formatQueryString(cQuery, gcQueryEditorState = 'visible').
  end.

  return cQuery <> ?.
end function. /* setQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setQueryEditor C-Win 
FUNCTION setQueryEditor RETURNS LOGICAL
  ( pcQueryEditorState as character ) :
/*------------------------------------------------------------------------
  Name         : setQueryEditor
  Description  : Show or hide the query editor and associated fields.
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  /* If we try to set it to its current value, nothing will happen so: */
  if pcQueryEditorState = gcQueryEditorState then return false.

  case pcQueryEditorState:
    when 'visible' then 
    do:
      if (ficWhere:x in frame frMain + frame frWhere:width-pixels) > c-win:width-pixels then
        frame frWhere:x = (c-win:width-pixels - frame frWhere:width-pixels) / 2.
      else
        frame frWhere:x = ficWhere:x.

      if (ficWhere:y in frame frMain + frame frWhere:height-pixels) > rctEdit:y in frame frMain then
        frame frWhere:y = rctEdit:y in frame frMain - frame frWhere:height-pixels - 20.
      else 
        frame frWhere:y = ficWhere:y.

      view frame frWhere.
      
      gcQueryEditorState = pcQueryEditorState.
      if ficWhere:screen-value in frame frMain <> '' then
        ficWhere2:screen-value in frame frWhere = formatQueryString(ficWhere:screen-value in frame frMain, yes). 
    end.

    when 'hidden'  then 
    do:
      hide frame frWhere.
      
      gcQueryEditorState = pcQueryEditorState.
      if ficWhere2:screen-value in frame frWhere <> '' then
        ficWhere:screen-value in frame frMain = formatQueryString(ficWhere2:screen-value in frame frWhere, no). 
    end.

    /* All other settings will be ignored */
    otherwise return false.
  end case.

  /* Save setting for query editor state */
  setRegistry("DataDigger", "QueryEditorState", gcQueryEditorState).

  return true.
END FUNCTION. /* setQueryEditor */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setUpdatePanel C-Win 
FUNCTION setUpdatePanel RETURNS LOGICAL
  ( input pcMode as character ) :

/*------------------------------------------------------------------------------
  Purpose: setUpdatePanel
    Notes: enable / disable update panel buttons
    
    Mode       Sensitive buttons
    -----      --------------------
    display    add,delete,view,dump
    no-record  add
    update     save,cancel
------------------------------------------------------------------------------*/
  define variable lHasRecords as logical no-undo. 

  {&timerStart} 

  if pcMode <> ? then gcRecordMode = pcMode.

  do with frame frMain:

    lHasRecords = (    valid-handle(ghDataBrowse)
                   and valid-handle(ghDataBrowse:query) 
                   and ghDataBrowse:query:num-results <> ?
                   and ghDataBrowse:query:num-results > 0).
    
    assign 
      btnAdd:sensitive      = lookup( gcRecordMode, 'display,no-record') > 0 and not ReadOnlyDigger
      btnClone:sensitive    = lookup( gcRecordMode, 'display') > 0 and lHasRecords and not ReadOnlyDigger
      btnEdit:sensitive     = lookup( gcRecordMode, 'display') > 0 and lHasRecords and ghDataBrowse:num-selected-rows > 0 
      btnDelete:sensitive   = lookup( gcRecordMode, 'display') > 0 and lHasRecords and ghDataBrowse:num-selected-rows > 0 and not ReadOnlyDigger
      btnView:sensitive     = lookup( gcRecordMode, 'display') > 0 and lHasRecords and ghDataBrowse:num-selected-rows > 0
      
      btnDump:sensitive     = lookup( gcRecordMode, 'display') > 0 and lHasRecords AND ghDataBrowse:num-iterations > 0
      btnDump-2:sensitive   in frame frSettings = btnDump:sensitive
      btnDump-txt:sensitive in frame frSettings = btnDump:sensitive
      .

    /* Load buttons noersion 16 */
    if getRegistry('DataDigger','LoadWindow') = 'yes' then
    assign
      btnLoad:sensitive          = lookup( gcRecordMode, 'display') > 0 and not ReadOnlyDigger 
      btnLoad-2:sensitive   in frame frSettings = btnLoad:sensitive and not ReadOnlyDigger 
      btnLoad-txt:sensitive in frame frSettings = btnLoad:sensitive and not ReadOnlyDigger
      .
    else 
    assign
      btnLoad:sensitive                         = false             and not ReadOnlyDigger
      btnLoad-2:sensitive   in frame frSettings = btnLoad:sensitive and not ReadOnlyDigger
      btnLoad-txt:sensitive in frame frSettings = btnLoad:sensitive and not ReadOnlyDigger
      .
                                 
    /* Hide these when no data browse */
    DO WITH FRAME frData:
      assign
        btnDataFilter:visible      = VALID-HANDLE(ghDataBrowse)
        btnClearDataFilter:visible = VALID-HANDLE(ghDataBrowse)
        fiNumresults:visible       = (gcRecordMode <> 'no-record')
        fiNumSelected:visible      = (gcRecordMode <> 'no-record')
        .
    END.
  end.

  /* Kill scrollbars */
  run showScrollBars(frame {&frame-name}:handle, no, no).

  {&timerStop}
end function. /* setUpdatePanel */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setWindowFreeze C-Win 
FUNCTION setWindowFreeze RETURNS LOGICAL
  ( plWindowsLocked AS LOGICAL ) :

/*------------------------------------------------------------------------------
  Purpose: setWindowFreeze
    Notes: enable / disable debug mode
------------------------------------------------------------------------------*/

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
