&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME C-Win
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS C-Win 
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

/* Parameters Definitions ---                                           */

DEFINE INPUT  PARAMETER phParent    AS HANDLE      NO-UNDO.
DEFINE INPUT  PARAMETER phRectangle AS HANDLE      NO-UNDO.

/* Local Variable Definitions ---                                       */

{ datadigger.i }

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME DEFAULT-FRAME

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR C-Win AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE RECTANGLE RECT-21
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 32.

DEFINE VARIABLE tgAddDataColumnForRecid AS LOGICAL INITIAL no 
     LABEL "&Recid" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 55 BY 15 NO-UNDO.

DEFINE VARIABLE tgAddDataColumnForRowid AS LOGICAL INITIAL no 
     LABEL "R&owid" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 60 BY 17 NO-UNDO.

DEFINE VARIABLE rsTitleBarDbName AS CHARACTER 
     VIEW-AS RADIO-SET VERTICAL
     RADIO-BUTTONS 
          "No Name", "none",
"Logical Name", "ldbname",
"Physical Name", "pdbname"
     SIZE-PIXELS 175 BY 55 TOOLTIP "which database name to use in title bar" NO-UNDO.

DEFINE RECTANGLE RECT-22
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 85.

DEFINE BUTTON btnRecordCountComplete 
     LABEL "Set" 
     SIZE-PIXELS 50 BY 21.

DEFINE BUTTON btnRecordCountIncomplete 
     LABEL "Set" 
     SIZE-PIXELS 50 BY 21.

DEFINE BUTTON btnRecordCountSelected 
     LABEL "Set" 
     SIZE-PIXELS 50 BY 21.

DEFINE VARIABLE fiRecordCountComplete AS CHARACTER FORMAT "X(256)":U INITIAL "RecordCount:Complete" 
     LABEL "All records fetched" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiRecordCountIncomplete AS CHARACTER FORMAT "X(256)":U INITIAL "RecordCount:Incomplete" 
     LABEL "Not all records fetched" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiRecordCountSelected AS CHARACTER FORMAT "X(256)":U INITIAL "RecordCount:Selected" 
     LABEL "Nr of selected records" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE RECTANGLE RECT-18
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 95.

DEFINE BUTTON btnDefaultFont 
     LABEL "&Default Font" 
     SIZE-PIXELS 120 BY 24.

DEFINE BUTTON btnFixedFont 
     LABEL "&Fixed Font" 
     SIZE-PIXELS 120 BY 24.

DEFINE RECTANGLE RECT-20
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 70.

DEFINE VARIABLE tgAutoFont AS LOGICAL INITIAL no 
     LABEL "&Automatically set fonts" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 215 BY 17 NO-UNDO.

DEFINE BUTTON btnEvenBg 
     LABEL "BG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnEvenFg 
     LABEL "FG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnFieldFilterBG 
     LABEL "BG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnFieldFilterFG 
     LABEL "FG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnOddBg 
     LABEL "BG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnOddFg 
     LABEL "FG" 
     SIZE-PIXELS 25 BY 21.

DEFINE VARIABLE fiEvenRow AS CHARACTER FORMAT "X(256)":U INITIAL "Data Row: even" 
     LABEL "Even rows" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiFieldFilter AS CHARACTER FORMAT "X(256)":U INITIAL "Matched by Field Filter" 
     LABEL "Matched by Field Filter" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiOddRow AS CHARACTER FORMAT "X(256)":U INITIAL "Data Row: odd" 
     LABEL "Odd rows" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE RECTANGLE RECT-14
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 135.

DEFINE VARIABLE tgUseSystemColors AS LOGICAL INITIAL no 
     LABEL "Use &System Colors" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 150 BY 17 NO-UNDO.

DEFINE BUTTON btnfiCustomFormatBg 
     LABEL "BG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnfiCustomFormatFg 
     LABEL "FG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnfiCustomOrderBg 
     LABEL "BG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnfiCustomOrderFg 
     LABEL "FG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnFilterBox 
     LABEL "Set" 
     SIZE-PIXELS 50 BY 21.

DEFINE BUTTON btnWarningBoxBg 
     LABEL "BG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnWarningBoxFg 
     LABEL "FG" 
     SIZE-PIXELS 25 BY 21.

DEFINE VARIABLE fiCustomFormat AS CHARACTER FORMAT "X(256)":U INITIAL "CustomFormat" 
     LABEL "User-defined format" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiCustomOrder AS CHARACTER FORMAT "X(256)":U INITIAL "CustomOrder" 
     LABEL "User-defined order" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiFilterBox AS CHARACTER FORMAT "X(256)":U INITIAL "FilterBox" 
     LABEL "Box color when filter used" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiWarningBox AS CHARACTER FORMAT "X(256)":U INITIAL "WarningBox" 
     LABEL "Changed format warning" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE RECTANGLE RECT-17
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 120.

DEFINE BUTTON btnIndexInactive 
     LABEL "Set" 
     SIZE-PIXELS 50 BY 21.

DEFINE BUTTON btnPrimIndex 
     LABEL "Set" 
     SIZE-PIXELS 50 BY 21.

DEFINE VARIABLE fiIndexInactive AS CHARACTER FORMAT "X(256)":U INITIAL "IndexInactive" 
     LABEL "Index Inactive" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiPrimIndex AS CHARACTER FORMAT "X(256)":U INITIAL "PrimIndex" 
     LABEL "Primary Index" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE RECTANGLE RECT-15
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 70.

DEFINE BUTTON btnQueryCounter 
     LABEL "Set" 
     SIZE-PIXELS 50 BY 21.

DEFINE BUTTON btnQueryErrorBg 
     LABEL "BG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnQueryErrorFg 
     LABEL "FG" 
     SIZE-PIXELS 25 BY 21.

DEFINE BUTTON btnQueryInfo 
     LABEL "Set" 
     SIZE-PIXELS 50 BY 21.

DEFINE VARIABLE fiQueryCounter AS CHARACTER FORMAT "X(256)":U INITIAL "QueryCounter" 
     LABEL "Nr of queries served" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiQueryError AS CHARACTER FORMAT "X(256)":U INITIAL "QueryError" 
     LABEL "Error on opening query" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE VARIABLE fiQueryInfo AS CHARACTER FORMAT "X(256)":U INITIAL "QueryInfo" 
     LABEL "Query-information" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 160 BY 21 NO-UNDO.

DEFINE RECTANGLE RECT-16
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 95.

DEFINE VARIABLE fiExample AS CHARACTER FORMAT "X(256)":U 
     LABEL "Example" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 170 BY 21 NO-UNDO.

DEFINE VARIABLE rsColumnLabelTemplate AS CHARACTER 
     VIEW-AS RADIO-SET VERTICAL
     RADIO-BUTTONS 
          "Field name", "&1",
"Order + Field name", "&2. &1",
"Label", "&3",
"Order + Label", "&2. &3"
     SIZE-PIXELS 175 BY 75 TOOLTIP "template for column labels on the data browse" NO-UNDO.

DEFINE RECTANGLE RECT-19
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 400 BY 140.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME DEFAULT-FRAME
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT COL 1 ROW 1
         SIZE 222 BY 39.81 WIDGET-ID 100.

DEFINE FRAME FRAME-J
     tgAutoFont AT Y 13 X 90 WIDGET-ID 122
     btnDefaultFont AT Y 34 X 90 WIDGET-ID 98
     btnFixedFont AT Y 34 X 220 WIDGET-ID 100
     "Set fonts:" VIEW-AS TEXT
          SIZE-PIXELS 73 BY 13 AT Y 39 X 12 WIDGET-ID 102
     RECT-20 AT Y 5 X 5 WIDGET-ID 124
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 10 Y 70
         SIZE-PIXELS 440 BY 102
         TITLE "2" WIDGET-ID 1200.

DEFINE FRAME FRAME-A
     tgAddDataColumnForRowid AT Y 12 X 232 WIDGET-ID 60
     tgAddDataColumnForRecid AT Y 13 X 163 WIDGET-ID 58
     "Show Data Column For:" VIEW-AS TEXT
          SIZE-PIXELS 146 BY 13 AT Y 13 X 14 WIDGET-ID 62
     RECT-21 AT Y 5 X 5 WIDGET-ID 64
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 10 Y 0
         SIZE-PIXELS 440 BY 65
         TITLE "2" WIDGET-ID 300.

DEFINE FRAME FRAME-AF
     fiRecordCountComplete AT Y 16 X 160 COLON-ALIGNED WIDGET-ID 168 NO-TAB-STOP 
     btnRecordCountComplete AT Y 16 X 344 WIDGET-ID 116
     fiRecordCountIncomplete AT Y 42 X 160 COLON-ALIGNED WIDGET-ID 172 NO-TAB-STOP 
     btnRecordCountIncomplete AT Y 42 X 344 WIDGET-ID 170
     fiRecordCountSelected AT Y 68 X 160 COLON-ALIGNED WIDGET-ID 176 NO-TAB-STOP 
     btnRecordCountSelected AT Y 68 X 344 WIDGET-ID 174
     "Nr of Records color" VIEW-AS TEXT
          SIZE 32 BY .81 AT ROW 1 COL 4 WIDGET-ID 180
     RECT-18 AT Y 5 X 5 WIDGET-ID 178
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 10 Y 650
         SIZE-PIXELS 440 BY 130
         TITLE "2" WIDGET-ID 2800.

DEFINE FRAME FRAME-W
     fiIndexInactive AT Y 18 X 160 COLON-ALIGNED WIDGET-ID 168 NO-TAB-STOP 
     btnIndexInactive AT Y 18 X 344 WIDGET-ID 116
     fiPrimIndex AT Y 44 X 160 COLON-ALIGNED WIDGET-ID 172 NO-TAB-STOP 
     btnPrimIndex AT Y 44 X 344 WIDGET-ID 170
     "Index colors" VIEW-AS TEXT
          SIZE 20 BY .81 AT ROW 1 COL 4 WIDGET-ID 180
     RECT-15 AT Y 5 X 5 WIDGET-ID 176
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 10 Y 540
         SIZE-PIXELS 440 BY 100
         TITLE "2" WIDGET-ID 2200.

DEFINE FRAME FRAME-T
     tgUseSystemColors AT Y 16 X 170 WIDGET-ID 122
     fiOddRow AT Y 39 X 160 COLON-ALIGNED WIDGET-ID 110 NO-TAB-STOP 
     btnOddFg AT Y 39 X 344 WIDGET-ID 114
     btnOddBg AT Y 39 X 369 WIDGET-ID 118
     fiEvenRow AT Y 64 X 160 COLON-ALIGNED WIDGET-ID 112 NO-TAB-STOP 
     btnEvenFg AT Y 64 X 344 WIDGET-ID 170
     btnEvenBg AT Y 64 X 369 WIDGET-ID 120
     fiFieldFilter AT Y 104 X 160 COLON-ALIGNED WIDGET-ID 186 NO-TAB-STOP 
     btnFieldFilterFG AT Y 104 X 344 WIDGET-ID 184
     btnFieldFilterBG AT Y 104 X 369 WIDGET-ID 182
     "Data Row colors" VIEW-AS TEXT
          SIZE 25 BY .81 AT ROW 1 COL 4 WIDGET-ID 180
     RECT-14 AT Y 5 X 5 WIDGET-ID 176
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 10 Y 355
         SIZE-PIXELS 440 BY 165
         TITLE "2" WIDGET-ID 2000.

DEFINE FRAME FRAME-Y
     fiQueryCounter AT Y 15 X 160 COLON-ALIGNED WIDGET-ID 168 NO-TAB-STOP 
     btnQueryCounter AT Y 15 X 344 WIDGET-ID 116
     fiQueryInfo AT Y 41 X 160 COLON-ALIGNED WIDGET-ID 172 NO-TAB-STOP 
     btnQueryInfo AT Y 41 X 344 WIDGET-ID 170
     fiQueryError AT Y 67 X 160 COLON-ALIGNED WIDGET-ID 174 NO-TAB-STOP 
     btnQueryErrorFg AT Y 67 X 344 WIDGET-ID 114
     btnQueryErrorBg AT Y 67 X 369 WIDGET-ID 118
     "Query Info colors" VIEW-AS TEXT
          SIZE 25 BY .81 AT ROW 1 COL 4 WIDGET-ID 178
     RECT-16 AT Y 5 X 5 WIDGET-ID 176
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 470 Y 365
         SIZE-PIXELS 440 BY 140
         TITLE "2" WIDGET-ID 2400.

DEFINE FRAME FRAME-V
     fiFilterBox AT Y 16 X 160 COLON-ALIGNED WIDGET-ID 168 NO-TAB-STOP 
     btnFilterBox AT Y 16 X 344 WIDGET-ID 116
     fiCustomOrder AT Y 42 X 160 COLON-ALIGNED WIDGET-ID 190 NO-TAB-STOP 
     btnfiCustomOrderFg AT Y 42 X 344 WIDGET-ID 188
     btnfiCustomOrderBg AT Y 42 X 369 WIDGET-ID 186
     fiCustomFormat AT Y 68 X 160 COLON-ALIGNED WIDGET-ID 184 NO-TAB-STOP 
     btnfiCustomFormatFg AT Y 68 X 344 WIDGET-ID 182
     btnfiCustomFormatBg AT Y 68 X 369 WIDGET-ID 180
     fiWarningBox AT Y 94 X 160 COLON-ALIGNED WIDGET-ID 170 NO-TAB-STOP 
     btnWarningBoxFg AT Y 94 X 344 WIDGET-ID 114
     btnWarningBoxBg AT Y 94 X 369 WIDGET-ID 118
     "Warning colors" VIEW-AS TEXT
          SIZE 23 BY .81 AT ROW 1 COL 4 WIDGET-ID 178
     RECT-17 AT Y 5 X 5 WIDGET-ID 176
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 470 Y 510
         SIZE-PIXELS 440 BY 155
         TITLE "2" WIDGET-ID 2100.

DEFINE FRAME FRAME-Z
     rsColumnLabelTemplate AT Y 25 X 85 NO-LABEL WIDGET-ID 186
     fiExample AT Y 111 X 75 COLON-ALIGNED WIDGET-ID 192
     "Data Browse Column Label Template" VIEW-AS TEXT
          SIZE-PIXELS 240 BY 17 AT Y 0 X 15 WIDGET-ID 180
     RECT-19 AT Y 5 X 5 WIDGET-ID 176
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 10 Y 175
         SIZE-PIXELS 440 BY 170
         TITLE "2" WIDGET-ID 2900.

DEFINE FRAME FRAME-AB
     rsTitleBarDbName AT Y 25 X 85 NO-LABEL WIDGET-ID 186
     "Database Name in Title Bar" VIEW-AS TEXT
          SIZE-PIXELS 180 BY 17 AT Y 0 X 15 WIDGET-ID 180
     RECT-22 AT Y 5 X 5 WIDGET-ID 176
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 455 Y 140
         SIZE-PIXELS 440 BY 120
         TITLE "2" WIDGET-ID 3000.


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
         TITLE              = "<insert window title>"
         HEIGHT             = 40.52
         WIDTH              = 235.6
         MAX-HEIGHT         = 40.52
         MAX-WIDTH          = 235.6
         VIRTUAL-HEIGHT     = 40.52
         VIRTUAL-WIDTH      = 235.6
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
ASSIGN FRAME FRAME-A:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-AB:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-AF:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-J:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-T:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-V:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-W:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-Y:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-Z:FRAME = FRAME DEFAULT-FRAME:HANDLE.

/* SETTINGS FOR FRAME DEFAULT-FRAME
   FRAME-NAME                                                           */

DEFINE VARIABLE XXTABVALXX AS LOGICAL NO-UNDO.

ASSIGN XXTABVALXX = FRAME FRAME-W:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-AF:HANDLE)
       XXTABVALXX = FRAME FRAME-V:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-W:HANDLE)
       XXTABVALXX = FRAME FRAME-Y:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-V:HANDLE)
       XXTABVALXX = FRAME FRAME-T:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-Y:HANDLE)
       XXTABVALXX = FRAME FRAME-Z:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-T:HANDLE)
       XXTABVALXX = FRAME FRAME-AB:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-Z:HANDLE)
       XXTABVALXX = FRAME FRAME-J:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-AB:HANDLE)
       XXTABVALXX = FRAME FRAME-A:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-J:HANDLE)
/* END-ASSIGN-TABS */.

ASSIGN 
       FRAME DEFAULT-FRAME:HIDDEN           = TRUE.

/* SETTINGS FOR FRAME FRAME-A
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-A:HIDDEN           = TRUE.

ASSIGN 
       tgAddDataColumnForRecid:PRIVATE-DATA IN FRAME FRAME-A     = 
                "DataDigger,AddDataColumnForRecid".

ASSIGN 
       tgAddDataColumnForRowid:PRIVATE-DATA IN FRAME FRAME-A     = 
                "DataDigger,AddDataColumnForRowid".

/* SETTINGS FOR FRAME FRAME-AB
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-AB:HIDDEN           = TRUE.

ASSIGN 
       rsTitleBarDbName:PRIVATE-DATA IN FRAME FRAME-AB     = 
                "DataDigger,TitleBarDbName".

/* SETTINGS FOR FRAME FRAME-AF
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-AF:HIDDEN           = TRUE.

ASSIGN 
       fiRecordCountComplete:READ-ONLY IN FRAME FRAME-AF        = TRUE
       fiRecordCountComplete:PRIVATE-DATA IN FRAME FRAME-AF     = 
                "DataDigger:colors,RecordCount:Complete".

ASSIGN 
       fiRecordCountIncomplete:READ-ONLY IN FRAME FRAME-AF        = TRUE
       fiRecordCountIncomplete:PRIVATE-DATA IN FRAME FRAME-AF     = 
                "DataDigger:colors,RecordCount:Incomplete".

ASSIGN 
       fiRecordCountSelected:READ-ONLY IN FRAME FRAME-AF        = TRUE
       fiRecordCountSelected:PRIVATE-DATA IN FRAME FRAME-AF     = 
                "DataDigger:colors,RecordCount:Selected".

/* SETTINGS FOR FRAME FRAME-J
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-J:HIDDEN           = TRUE.

ASSIGN 
       btnDefaultFont:PRIVATE-DATA IN FRAME FRAME-J     = 
                "DataDigger:fonts,default".

ASSIGN 
       btnFixedFont:PRIVATE-DATA IN FRAME FRAME-J     = 
                "DataDigger:fonts,fixed".

ASSIGN 
       tgAutoFont:PRIVATE-DATA IN FRAME FRAME-J     = 
                "DataDigger:fonts,AutoSetFont".

/* SETTINGS FOR FRAME FRAME-T
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-T:HIDDEN           = TRUE.

ASSIGN 
       fiEvenRow:READ-ONLY IN FRAME FRAME-T        = TRUE
       fiEvenRow:PRIVATE-DATA IN FRAME FRAME-T     = 
                "DataDigger:colors,DataRow:even".

ASSIGN 
       fiFieldFilter:READ-ONLY IN FRAME FRAME-T        = TRUE
       fiFieldFilter:PRIVATE-DATA IN FRAME FRAME-T     = 
                "DataDigger:colors,FieldFilter".

ASSIGN 
       fiOddRow:READ-ONLY IN FRAME FRAME-T        = TRUE
       fiOddRow:PRIVATE-DATA IN FRAME FRAME-T     = 
                "DataDigger:colors,DataRow:odd".

ASSIGN 
       tgUseSystemColors:PRIVATE-DATA IN FRAME FRAME-T     = 
                "DataDigger:colors,DataRow:UseSystem".

/* SETTINGS FOR FRAME FRAME-V
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-V:HIDDEN           = TRUE.

ASSIGN 
       fiCustomFormat:READ-ONLY IN FRAME FRAME-V        = TRUE
       fiCustomFormat:PRIVATE-DATA IN FRAME FRAME-V     = 
                "DataDigger:colors,CustomFormat".

ASSIGN 
       fiCustomOrder:READ-ONLY IN FRAME FRAME-V        = TRUE
       fiCustomOrder:PRIVATE-DATA IN FRAME FRAME-V     = 
                "DataDigger:colors,CustomOrder".

ASSIGN 
       fiFilterBox:READ-ONLY IN FRAME FRAME-V        = TRUE
       fiFilterBox:PRIVATE-DATA IN FRAME FRAME-V     = 
                "DataDigger:colors,FilterBox".

ASSIGN 
       fiWarningBox:READ-ONLY IN FRAME FRAME-V        = TRUE
       fiWarningBox:PRIVATE-DATA IN FRAME FRAME-V     = 
                "DataDigger:colors,WarningBox".

/* SETTINGS FOR FRAME FRAME-W
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-W:HIDDEN           = TRUE.

ASSIGN 
       fiIndexInactive:READ-ONLY IN FRAME FRAME-W        = TRUE
       fiIndexInactive:PRIVATE-DATA IN FRAME FRAME-W     = 
                "DataDigger:colors,IndexInactive".

ASSIGN 
       fiPrimIndex:READ-ONLY IN FRAME FRAME-W        = TRUE
       fiPrimIndex:PRIVATE-DATA IN FRAME FRAME-W     = 
                "DataDigger:colors,PrimIndex".

/* SETTINGS FOR FRAME FRAME-Y
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-Y:HIDDEN           = TRUE.

ASSIGN 
       fiQueryCounter:READ-ONLY IN FRAME FRAME-Y        = TRUE
       fiQueryCounter:PRIVATE-DATA IN FRAME FRAME-Y     = 
                "DataDigger:colors,QueryCounter".

ASSIGN 
       fiQueryError:READ-ONLY IN FRAME FRAME-Y        = TRUE
       fiQueryError:PRIVATE-DATA IN FRAME FRAME-Y     = 
                "DataDigger:colors,QueryError".

ASSIGN 
       fiQueryInfo:READ-ONLY IN FRAME FRAME-Y        = TRUE
       fiQueryInfo:PRIVATE-DATA IN FRAME FRAME-Y     = 
                "DataDigger:colors,QueryInfo".

/* SETTINGS FOR FRAME FRAME-Z
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-Z:HIDDEN           = TRUE.

ASSIGN 
       fiExample:READ-ONLY IN FRAME FRAME-Z        = TRUE.

ASSIGN 
       rsColumnLabelTemplate:PRIVATE-DATA IN FRAME FRAME-Z     = 
                "DataDigger,ColumnLabelTemplate".

IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(C-Win)
THEN C-Win:HIDDEN = yes.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME C-Win
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON END-ERROR OF C-Win /* <insert window title> */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL C-Win C-Win
ON WINDOW-CLOSE OF C-Win /* <insert window title> */
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-J
&Scoped-define SELF-NAME btnDefaultFont
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDefaultFont C-Win
ON CHOOSE OF btnDefaultFont IN FRAME FRAME-J /* Default Font */
DO:
  define variable iFontNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseFont.w') 
     ( input self:font
     , output iFontNr
     ).

  if iFontNr <> -1 then self:font = iFontNr.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-T
&Scoped-define SELF-NAME btnEvenBg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnEvenBg C-Win
ON CHOOSE OF btnEvenBg IN FRAME FRAME-T /* BG */
or mouse-menu-click of fiEvenRow
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiEvenRow:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiEvenRow:bgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnEvenFg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnEvenFg C-Win
ON CHOOSE OF btnEvenFg IN FRAME FRAME-T /* FG */
or mouse-select-click of fiEvenRow
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiEvenRow:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiEvenRow:fgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-V
&Scoped-define SELF-NAME btnfiCustomFormatBg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnfiCustomFormatBg C-Win
ON CHOOSE OF btnfiCustomFormatBg IN FRAME FRAME-V /* BG */
or mouse-menu-click of fiCustomFormat
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiCustomFormat:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiCustomFormat:bgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnfiCustomFormatFg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnfiCustomFormatFg C-Win
ON CHOOSE OF btnfiCustomFormatFg IN FRAME FRAME-V /* FG */
or mouse-select-click of fiCustomFormat
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiCustomFormat:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiCustomFormat:fgcolor = iColorNr.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnfiCustomOrderBg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnfiCustomOrderBg C-Win
ON CHOOSE OF btnfiCustomOrderBg IN FRAME FRAME-V /* BG */
or mouse-menu-click of fiCustomOrder
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiCustomOrder:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiCustomOrder:bgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnfiCustomOrderFg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnfiCustomOrderFg C-Win
ON CHOOSE OF btnfiCustomOrderFg IN FRAME FRAME-V /* FG */
or mouse-select-click of fiCustomOrder
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiCustomOrder:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiCustomOrder:fgcolor = iColorNr.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-T
&Scoped-define SELF-NAME btnFieldFilterBG
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnFieldFilterBG C-Win
ON CHOOSE OF btnFieldFilterBG IN FRAME FRAME-T /* BG */
or mouse-menu-click of fiEvenRow
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiFieldFilter:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiFieldFilter:bgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnFieldFilterFG
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnFieldFilterFG C-Win
ON CHOOSE OF btnFieldFilterFG IN FRAME FRAME-T /* FG */
or mouse-select-click of fiEvenRow
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiFieldFilter:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiFieldFilter:fgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-V
&Scoped-define SELF-NAME btnFilterBox
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnFilterBox C-Win
ON CHOOSE OF btnFilterBox IN FRAME FRAME-V /* Set */
or mouse-select-click of fiFilterBox
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiFilterBox:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiFilterBox:bgcolor = iColorNr.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-J
&Scoped-define SELF-NAME btnFixedFont
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnFixedFont C-Win
ON CHOOSE OF btnFixedFont IN FRAME FRAME-J /* Fixed Font */
do:
  define variable iFontNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseFont.w') 
     ( input self:font
     , output iFontNr
     ).

  if iFontNr <> -1 then self:font = iFontNr.
    
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-W
&Scoped-define SELF-NAME btnIndexInactive
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnIndexInactive C-Win
ON CHOOSE OF btnIndexInactive IN FRAME FRAME-W /* Set */
or mouse-select-click of fiIndexInactive
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiIndexInactive:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiIndexInactive:fgcolor = iColorNr.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-T
&Scoped-define SELF-NAME btnOddBg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOddBg C-Win
ON CHOOSE OF btnOddBg IN FRAME FRAME-T /* BG */
or mouse-menu-click of fiOddRow
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiOddRow:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiOddRow:bgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnOddFg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOddFg C-Win
ON CHOOSE OF btnOddFg IN FRAME FRAME-T /* FG */
or mouse-select-click of fiOddRow
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiOddRow:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiOddRow:fgcolor = iColorNr.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-W
&Scoped-define SELF-NAME btnPrimIndex
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnPrimIndex C-Win
ON CHOOSE OF btnPrimIndex IN FRAME FRAME-W /* Set */
or mouse-select-click of fiPrimIndex
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiPrimIndex:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiPrimIndex:bgcolor = iColorNr.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-Y
&Scoped-define SELF-NAME btnQueryCounter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQueryCounter C-Win
ON CHOOSE OF btnQueryCounter IN FRAME FRAME-Y /* Set */
or mouse-select-click of fiQueryCounter
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiQueryCounter:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiQueryCounter:fgcolor = iColorNr.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnQueryErrorBg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQueryErrorBg C-Win
ON CHOOSE OF btnQueryErrorBg IN FRAME FRAME-Y /* BG */
or mouse-menu-click of fiQueryError
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiQueryError:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiQueryError:bgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnQueryErrorFg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQueryErrorFg C-Win
ON CHOOSE OF btnQueryErrorFg IN FRAME FRAME-Y /* FG */
or mouse-select-click of fiQueryError
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiQueryError:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiQueryError:fgcolor = iColorNr.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnQueryInfo
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnQueryInfo C-Win
ON CHOOSE OF btnQueryInfo IN FRAME FRAME-Y /* Set */
or mouse-select-click of fiQueryInfo
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiQueryInfo:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiQueryInfo:fgcolor = iColorNr.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-AF
&Scoped-define SELF-NAME btnRecordCountComplete
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnRecordCountComplete C-Win
ON CHOOSE OF btnRecordCountComplete IN FRAME FRAME-AF /* Set */
or mouse-select-click of fiRecordCountComplete
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiRecordCountComplete:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiRecordCountComplete:fgcolor = iColorNr.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnRecordCountIncomplete
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnRecordCountIncomplete C-Win
ON CHOOSE OF btnRecordCountIncomplete IN FRAME FRAME-AF /* Set */
or mouse-select-click of fiRecordCountIncomplete
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiRecordCountIncomplete:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiRecordCountIncomplete:fgcolor = iColorNr.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnRecordCountSelected
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnRecordCountSelected C-Win
ON CHOOSE OF btnRecordCountSelected IN FRAME FRAME-AF /* Set */
or mouse-select-click of fiRecordCountSelected
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiRecordCountSelected:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiRecordCountSelected:fgcolor = iColorNr.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-V
&Scoped-define SELF-NAME btnWarningBoxBg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnWarningBoxBg C-Win
ON CHOOSE OF btnWarningBoxBg IN FRAME FRAME-V /* BG */
or mouse-menu-click of fiWarningBox
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiWarningBox:bgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiWarningBox:bgcolor = iColorNr.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnWarningBoxFg
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnWarningBoxFg C-Win
ON CHOOSE OF btnWarningBoxFg IN FRAME FRAME-V /* FG */
or mouse-select-click of fiWarningBox
do:
  define variable iColorNr as integer no-undo. 

  run value(getProgramDir() + 'dChooseColor.w') 
     ( input fiWarningBox:fgcolor
     , output iColorNr
     ).

  if iColorNr <> -1 then 
    fiWarningBox:fgcolor = iColorNr.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-Z
&Scoped-define SELF-NAME rsColumnLabelTemplate
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL rsColumnLabelTemplate C-Win
ON VALUE-CHANGED OF rsColumnLabelTemplate IN FRAME FRAME-Z
do:

  fiExample:screen-value = 
    substitute(self:screen-value
              , "cust-num"
              , "1"
              , "Customer Number"
              ).

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-J
&Scoped-define SELF-NAME tgAutoFont
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgAutoFont C-Win
ON VALUE-CHANGED OF tgAutoFont IN FRAME FRAME-J /* Automatically set fonts */
DO:
  
  btnDefaultFont:sensitive = not self:checked.
  btnFixedFont  :sensitive = not self:checked.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-T
&Scoped-define SELF-NAME tgUseSystemColors
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgUseSystemColors C-Win
ON VALUE-CHANGED OF tgUseSystemColors IN FRAME FRAME-T /* Use System Colors */
DO:
  
  fiOddRow :sensitive = not self:checked.
  fiEvenRow:sensitive = not self:checked.
  btnOddFg :sensitive = not self:checked.
  btnOddBg :sensitive = not self:checked.
  btnEvenFg:sensitive = not self:checked.
  btnEvenBg:sensitive = not self:checked.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME DEFAULT-FRAME
&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK C-Win 


/* ***************************  Main Block  *************************** */

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
   RUN disable_UI.

/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.


/* Handle reparenting, startup etc */
{frameLib.i}


/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

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
  VIEW FRAME DEFAULT-FRAME IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-DEFAULT-FRAME}
  DISPLAY tgAddDataColumnForRowid tgAddDataColumnForRecid 
      WITH FRAME FRAME-A IN WINDOW C-Win.
  ENABLE RECT-21 tgAddDataColumnForRowid tgAddDataColumnForRecid 
      WITH FRAME FRAME-A IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-A}
  DISPLAY tgAutoFont 
      WITH FRAME FRAME-J IN WINDOW C-Win.
  ENABLE RECT-20 tgAutoFont btnDefaultFont btnFixedFont 
      WITH FRAME FRAME-J IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-J}
  DISPLAY rsTitleBarDbName 
      WITH FRAME FRAME-AB IN WINDOW C-Win.
  ENABLE RECT-22 rsTitleBarDbName 
      WITH FRAME FRAME-AB IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-AB}
  DISPLAY rsColumnLabelTemplate fiExample 
      WITH FRAME FRAME-Z IN WINDOW C-Win.
  ENABLE RECT-19 rsColumnLabelTemplate fiExample 
      WITH FRAME FRAME-Z IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-Z}
  DISPLAY tgUseSystemColors fiOddRow fiEvenRow fiFieldFilter 
      WITH FRAME FRAME-T IN WINDOW C-Win.
  ENABLE RECT-14 tgUseSystemColors fiOddRow btnOddFg btnOddBg fiEvenRow 
         btnEvenFg btnEvenBg fiFieldFilter btnFieldFilterFG btnFieldFilterBG 
      WITH FRAME FRAME-T IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-T}
  DISPLAY fiQueryCounter fiQueryInfo fiQueryError 
      WITH FRAME FRAME-Y IN WINDOW C-Win.
  ENABLE RECT-16 fiQueryCounter btnQueryCounter fiQueryInfo btnQueryInfo 
         fiQueryError btnQueryErrorFg btnQueryErrorBg 
      WITH FRAME FRAME-Y IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-Y}
  DISPLAY fiFilterBox fiCustomOrder fiCustomFormat fiWarningBox 
      WITH FRAME FRAME-V IN WINDOW C-Win.
  ENABLE RECT-17 fiFilterBox btnFilterBox fiCustomOrder btnfiCustomOrderFg 
         btnfiCustomOrderBg fiCustomFormat btnfiCustomFormatFg 
         btnfiCustomFormatBg fiWarningBox btnWarningBoxFg btnWarningBoxBg 
      WITH FRAME FRAME-V IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-V}
  DISPLAY fiIndexInactive fiPrimIndex 
      WITH FRAME FRAME-W IN WINDOW C-Win.
  ENABLE RECT-15 fiIndexInactive btnIndexInactive fiPrimIndex btnPrimIndex 
      WITH FRAME FRAME-W IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-W}
  DISPLAY fiRecordCountComplete fiRecordCountIncomplete fiRecordCountSelected 
      WITH FRAME FRAME-AF IN WINDOW C-Win.
  ENABLE RECT-18 fiRecordCountComplete btnRecordCountComplete 
         fiRecordCountIncomplete btnRecordCountIncomplete fiRecordCountSelected 
         btnRecordCountSelected 
      WITH FRAME FRAME-AF IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-AF}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
