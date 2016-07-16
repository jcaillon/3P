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
&Scoped-define List-1 btnSeq btnAction btnDb btnDumpName btnExt btnLastDir ~
btnProgDir btnTable btnUserid btnWeekday btnDate btnDay btnDayName btnHH ~
btnMM btnMonth btnSS btnTime btnTimestamp btnYear 

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR C-Win AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE VARIABLE tgBackupOnCreate AS LOGICAL INITIAL no 
     LABEL "Backup on &Create" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 120 BY 17 TOOLTIP "save a copy of the record as xml on creation" NO-UNDO.

DEFINE VARIABLE tgBackupOnDelete AS LOGICAL INITIAL no 
     LABEL "Backup on &Delete" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 115 BY 17 TOOLTIP "save a copy of the record as xml on deletion" NO-UNDO.

DEFINE VARIABLE tgBackupOnUpdate AS LOGICAL INITIAL no 
     LABEL "Backup on &Update" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 120 BY 17 TOOLTIP "save a copy of the record as xml on change" NO-UNDO.

DEFINE BUTTON btnAction  NO-FOCUS
     LABEL "<ACTION>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnDate  NO-FOCUS
     LABEL "<DATE>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnDay  NO-FOCUS
     LABEL "<DAY>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnDayName  NO-FOCUS
     LABEL "<DAYNAME>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnDb  NO-FOCUS
     LABEL "<DB>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnDumpName  NO-FOCUS
     LABEL "<DUMPNAME>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnExt  NO-FOCUS
     LABEL "<EXT>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnHH  NO-FOCUS
     LABEL "<HH>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnLastDir  NO-FOCUS
     LABEL "<LASTDIR>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnMM  NO-FOCUS
     LABEL "<MM>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnMonth  NO-FOCUS
     LABEL "<MONTH>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnProgDir  NO-FOCUS
     LABEL "<PROGDIR>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnSeq  NO-FOCUS
     LABEL "<#>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnSS  NO-FOCUS
     LABEL "<SS>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnTable  NO-FOCUS
     LABEL "<TABLE>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnTime  NO-FOCUS
     LABEL "<TIME>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnTimestamp  NO-FOCUS
     LABEL "<TIMESTAMP>" 
     SIZE-PIXELS 100 BY 19 TOOLTIP "show current date/time".

DEFINE BUTTON btnUserid  NO-FOCUS
     LABEL "<USERID>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnWeekday  NO-FOCUS
     LABEL "<WEEKDAY>" 
     SIZE-PIXELS 100 BY 19.

DEFINE BUTTON btnYear  NO-FOCUS
     LABEL "<YEAR>" 
     SIZE-PIXELS 100 BY 19.

DEFINE VARIABLE fiDirTemplate AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP
     SIZE-PIXELS 520 BY 21 TOOLTIP "template for the backup directory" NO-UNDO.

DEFINE VARIABLE fiDumpDirTemplate AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP
     SIZE-PIXELS 520 BY 21 TOOLTIP "template for the dump directory" NO-UNDO.

DEFINE VARIABLE fiDumpFileExample AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP
     SIZE-PIXELS 520 BY 20 TOOLTIP "example of the dumpfile name" NO-UNDO.

DEFINE VARIABLE fiDumpFileTemplate AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP
     SIZE-PIXELS 520 BY 21 TOOLTIP "template for the dump file" NO-UNDO.

DEFINE VARIABLE fiFileExample AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP
     SIZE-PIXELS 520 BY 20 TOOLTIP "example of the backup filename" NO-UNDO.

DEFINE VARIABLE fiFileTemplate AS CHARACTER 
     VIEW-AS EDITOR NO-WORD-WRAP
     SIZE-PIXELS 520 BY 21 TOOLTIP "template for the backup file" NO-UNDO.

DEFINE RECTANGLE RECT-14
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 560 BY 300.

DEFINE RECTANGLE RECT-15
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 560 BY 202.

DEFINE RECTANGLE RECT-16
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 560 BY 107.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME DEFAULT-FRAME
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 0 Y 0
         SIZE-PIXELS 586 BY 556 WIDGET-ID 100.

DEFINE FRAME FRAME-K
     tgBackupOnCreate AT Y 0 X 120 WIDGET-ID 126
     tgBackupOnUpdate AT Y 0 X 255 WIDGET-ID 130
     tgBackupOnDelete AT Y 0 X 400 WIDGET-ID 132
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 5 Y 0
         SIZE-PIXELS 565 BY 40
         TITLE "3" WIDGET-ID 1300.

DEFINE FRAME FRAME-O
     btnSeq AT Y 273 X 450 WIDGET-ID 210
     btnAction AT Y 273 X 240 WIDGET-ID 196
     btnDb AT Y 213 X 345 WIDGET-ID 200
     fiDirTemplate AT Y 18 X 30 NO-LABEL WIDGET-ID 166
     btnDumpName AT Y 253 X 450 WIDGET-ID 204
     fiFileTemplate AT Y 41 X 30 NO-LABEL WIDGET-ID 176
     fiFileExample AT Y 65 X 30 NO-LABEL WIDGET-ID 212
     fiDumpDirTemplate AT Y 116 X 30 NO-LABEL WIDGET-ID 178
     fiDumpFileTemplate AT Y 139 X 30 NO-LABEL WIDGET-ID 182
     btnExt AT Y 273 X 345 WIDGET-ID 198
     fiDumpFileExample AT Y 161 X 30 NO-LABEL WIDGET-ID 214
     btnLastDir AT Y 233 X 450 WIDGET-ID 208
     btnProgDir AT Y 213 X 450 WIDGET-ID 206
     btnTable AT Y 233 X 345 WIDGET-ID 202
     btnUserid AT Y 253 X 345 WIDGET-ID 194
     btnWeekday AT Y 273 X 30 WIDGET-ID 148
     btnDate AT Y 233 X 30 WIDGET-ID 140
     btnDay AT Y 253 X 135 WIDGET-ID 156
     btnDayName AT Y 273 X 135 WIDGET-ID 150
     btnHH AT Y 213 X 240 WIDGET-ID 158
     btnMM AT Y 233 X 240 WIDGET-ID 160
     btnMonth AT Y 233 X 135 WIDGET-ID 154
     btnSS AT Y 253 X 240 WIDGET-ID 162
     btnTime AT Y 253 X 30 WIDGET-ID 142
     btnTimestamp AT Y 213 X 30 WIDGET-ID 138
     btnYear AT Y 213 X 135 WIDGET-ID 152
     "Dir:" VIEW-AS TEXT
          SIZE-PIXELS 23 BY 13 AT Y 23 X 7 WIDGET-ID 168
     "Dump template" VIEW-AS TEXT
          SIZE-PIXELS 80 BY 13 AT Y 98 X 10 WIDGET-ID 190
     "Dir:" VIEW-AS TEXT
          SIZE-PIXELS 23 BY 13 AT Y 120 X 5 WIDGET-ID 186
     "Backup template" VIEW-AS TEXT
          SIZE-PIXELS 85 BY 13 AT Y 0 X 14 WIDGET-ID 172
     "File:" VIEW-AS TEXT
          SIZE-PIXELS 23 BY 13 AT Y 145 X 5 WIDGET-ID 184
     "File:" VIEW-AS TEXT
          SIZE-PIXELS 23 BY 13 AT Y 49 X 7 WIDGET-ID 174
     "Use these fields for templates :" VIEW-AS TEXT
          SIZE-PIXELS 140 BY 13 AT Y 193 X 10 WIDGET-ID 146
     RECT-14 AT Y 5 X 0 WIDGET-ID 170
     RECT-15 AT Y 103 X 0 WIDGET-ID 188
     RECT-16 AT Y 198 X 0 WIDGET-ID 192
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 5 Y 42
         SIZE-PIXELS 565 BY 335
         TITLE "3" WIDGET-ID 1600.


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
         HEIGHT-P           = 571
         WIDTH-P            = 590
         MAX-HEIGHT-P       = 696
         MAX-WIDTH-P        = 963
         VIRTUAL-HEIGHT-P   = 696
         VIRTUAL-WIDTH-P    = 963
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
ASSIGN FRAME FRAME-K:FRAME = FRAME DEFAULT-FRAME:HANDLE
       FRAME FRAME-O:FRAME = FRAME DEFAULT-FRAME:HANDLE.

/* SETTINGS FOR FRAME DEFAULT-FRAME
   FRAME-NAME                                                           */

DEFINE VARIABLE XXTABVALXX AS LOGICAL NO-UNDO.

ASSIGN XXTABVALXX = FRAME FRAME-K:MOVE-BEFORE-TAB-ITEM (FRAME FRAME-O:HANDLE)
/* END-ASSIGN-TABS */.

ASSIGN 
       FRAME DEFAULT-FRAME:HIDDEN           = TRUE.

/* SETTINGS FOR FRAME FRAME-K
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-K:HIDDEN           = TRUE.

ASSIGN 
       tgBackupOnCreate:PRIVATE-DATA IN FRAME FRAME-K     = 
                "DataDigger:backup,BackupOnCreate".

ASSIGN 
       tgBackupOnDelete:PRIVATE-DATA IN FRAME FRAME-K     = 
                "DataDigger:backup,BackupOnDelete".

ASSIGN 
       tgBackupOnUpdate:PRIVATE-DATA IN FRAME FRAME-K     = 
                "DataDigger:backup,BackupOnUpdate".

/* SETTINGS FOR FRAME FRAME-O
   NOT-VISIBLE                                                          */
ASSIGN 
       FRAME FRAME-O:HIDDEN           = TRUE.

/* SETTINGS FOR BUTTON btnAction IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnDate IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnDay IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnDayName IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnDb IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnDumpName IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnExt IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnHH IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnLastDir IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnMM IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnMonth IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnProgDir IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnSeq IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnSS IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnTable IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnTime IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnTimestamp IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnUserid IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnWeekday IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
/* SETTINGS FOR BUTTON btnYear IN FRAME FRAME-O
   NO-ENABLE 1                                                          */
ASSIGN 
       fiDirTemplate:PRIVATE-DATA IN FRAME FRAME-O     = 
                "DataDigger:Backup,BackupDir".

ASSIGN 
       fiDumpDirTemplate:PRIVATE-DATA IN FRAME FRAME-O     = 
                "DumpAndLoad,DumpDir".

ASSIGN 
       fiDumpFileExample:READ-ONLY IN FRAME FRAME-O        = TRUE.

ASSIGN 
       fiDumpFileTemplate:PRIVATE-DATA IN FRAME FRAME-O     = 
                "DumpAndLoad,DumpFileTemplate".

ASSIGN 
       fiFileExample:READ-ONLY IN FRAME FRAME-O        = TRUE.

ASSIGN 
       fiFileTemplate:PRIVATE-DATA IN FRAME FRAME-O     = 
                "DataDigger:Backup,BackupFileTemplate".

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


&Scoped-define FRAME-NAME FRAME-O
&Scoped-define SELF-NAME btnTimestamp
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnTimestamp C-Win
ON CHOOSE OF btnTimestamp IN FRAME FRAME-O /* <TIMESTAMP> */
, btnAction, btnDb, btnDumpName, btnExt, btnLastDir
, btnProgDir, btnTable, btnWeekday, btnDate, btnUserid
, btnDay, btnDayName, btnHH, btnMM, btnMonth, btnSS
, btnTime, btnTimestamp, btnYear, btnSeq

do:

  if   focus:name = 'fiDirTemplate' 
    or focus:name = 'fiFileTemplate' 
    or focus:name = 'fiDumpDirTemplate' 
    or focus:name = 'fiDumpFileTemplate' then 
  do:

    if focus:selection-text <> '' then
      focus:replace-selection-text( self:label ).
    else
      focus:insert-string(self:label).

    apply "value-changed" to focus.
  end.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fiFileTemplate
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiFileTemplate C-Win
ON ENTRY OF fiFileTemplate IN FRAME FRAME-O
, fiFileTemplate, fiDirTemplate 
, fiDumpFileTemplate, fiDumpDirTemplate
DO:
  enable {&list-1} with frame frame-o.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiFileTemplate C-Win
ON LEAVE OF fiFileTemplate IN FRAME FRAME-O
, fiFileTemplate, fiDirTemplate 
, fiDumpFileTemplate, fiDumpDirTemplate
DO:
  disable {&list-1} with frame frame-o.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiFileTemplate C-Win
ON VALUE-CHANGED OF fiFileTemplate IN FRAME FRAME-O
, fiFileTemplate, fiDirTemplate 
, fiDumpFileTemplate, fiDumpDirTemplate
do:
  define variable cDummyName as character no-undo. 
  define variable cTemplate   as character no-undo. 

  if   self:name = "fiDirTemplate" 
    or self:name = "fiFileTemplate" then 
    cTemplate = fiDirTemplate:screen-value + fiFileTemplate:screen-value.
  else
  if   self:name = "fiDumpDirTemplate" 
    or self:name = "fiDumpFileTemplate" then 
    cTemplate = fiDumpDirTemplate:screen-value + fiDumpFileTemplate:screen-value.

  run getDumpFileName 
    ( input "Update"
    , input "sports"
    , input "customer"
    , input "d"
    , input cTemplate
    , output cDummyName
    ).

  if   self:name = "fiDirTemplate" 
    or self:name = "fiFileTemplate" then fiFileExample:screen-value = cDummyName.

  else 
  if   self:name = "fiDumpDirTemplate" 
    or self:name = "fiDumpFileTemplate" then fiDumpFileExample:screen-value = cDummyName.
  
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME FRAME-K
&Scoped-define SELF-NAME tgBackupOnCreate
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgBackupOnCreate C-Win
ON VALUE-CHANGED OF tgBackupOnCreate IN FRAME FRAME-K /* Backup on Create */
or 'value-changed' of tgBackupOnUpdate 
or 'value-changed' of tgBackupOnDelete 
DO:

  if    not tgBackupOnCreate:checked  
    and not tgBackupOnUpdate:checked
    and not tgBackupOnDelete:checked then 
  do with frame frame-q:
    fiDirTemplate:sensitive       in frame FRAME-O = no.
    fiFileTemplate:sensitive      in frame FRAME-O = no.
  end.
  else
  do:
    fiDirTemplate:sensitive   = yes.
    fiFileTemplate:sensitive  = yes.
  end.

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
  
  run initializeObject.

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
  DISPLAY tgBackupOnCreate tgBackupOnUpdate tgBackupOnDelete 
      WITH FRAME FRAME-K IN WINDOW C-Win.
  ENABLE tgBackupOnCreate tgBackupOnUpdate tgBackupOnDelete 
      WITH FRAME FRAME-K IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-K}
  DISPLAY fiDirTemplate fiFileTemplate fiFileExample fiDumpDirTemplate 
          fiDumpFileTemplate fiDumpFileExample 
      WITH FRAME FRAME-O IN WINDOW C-Win.
  ENABLE RECT-14 RECT-15 RECT-16 fiDirTemplate fiFileTemplate fiFileExample 
         fiDumpDirTemplate fiDumpFileTemplate fiDumpFileExample 
      WITH FRAME FRAME-O IN WINDOW C-Win.
  {&OPEN-BROWSERS-IN-QUERY-FRAME-O}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject C-Win 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  define variable hWidget as handle no-undo. 
  define variable cTooltip as character no-undo. 

  /* Attach tooltips to the template buttons */
  hWidget = frame frame-o:handle:first-child:first-child. 

  do while valid-handle(hWidget):
    if hWidget:type = "BUTTON" then
    do:
      run getDumpFileName 
        ( input "Update"
        , input "sports"
        , input "customer"
        , input "d"
        , input hWidget:label
        , output cTooltip
        ).
      hWidget:tooltip = cTooltip.
    end.

    hWidget = hWidget:next-sibling.
  end.

  apply "VALUE-CHANGED" to fiFileTemplate in frame frame-o.
  apply "VALUE-CHANGED" to fiDumpFileTemplate in frame frame-o.
  apply "VALUE-CHANGED" to tgBackupOnCreate in frame frame-k.

end procedure. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE xxxxlocalInitialize C-Win 
PROCEDURE xxxxlocalInitialize :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  apply "VALUE-CHANGED" to fiFileTemplate     in frame frame-o.
  apply "VALUE-CHANGED" to fiDumpFileTemplate in frame frame-o.
  apply "VALUE-CHANGED" to tgBackupOnCreate   in frame frame-k.

end procedure. /* localInitialize */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

