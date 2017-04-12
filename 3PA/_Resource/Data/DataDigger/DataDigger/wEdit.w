&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
/* Connected Databases 
*/
&Scoped-define WINDOW-NAME wEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS wEdit 
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
DEFINE {&invar} picMode           AS CHARACTER  NO-UNDO.
DEFINE {&invar} pihBrowse         AS HANDLE     NO-UNDO.
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

/* This table holds the actual values of the selected records */
DEFINE TEMP-TABLE ttData NO-UNDO RCODE-INFORMATION
  FIELD cFieldName AS CHARACTER
  FIELD cValue     AS CHARACTER FORMAT 'x(80)'
  INDEX iPrim IS PRIMARY cFieldName /* cValue */.

&GLOBAL-DEFINE field-lShow     1
&GLOBAL-DEFINE field-iOrder    2
&GLOBAL-DEFINE field-cFullName 3
&GLOBAL-DEFINE field-cLabel    4
&GLOBAL-DEFINE field-cNewValue 5

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME frMain
&Scoped-define BROWSE-NAME brRecord

/* Internal Tables (found by Frame, Query & Browse Queries)             */
&Scoped-define INTERNAL-TABLES ttColumn

/* Definitions for BROWSE brRecord                                      */
&Scoped-define FIELDS-IN-QUERY-brRecord ttColumn.lShow ttColumn.iOrder ttColumn.cFullName ttColumn.cLabel ttColumn.cNewValue   
&Scoped-define ENABLED-FIELDS-IN-QUERY-brRecord ttColumn.lShow  ttColumn.cNewValue   
&Scoped-define ENABLED-TABLES-IN-QUERY-brRecord ttColumn
&Scoped-define FIRST-ENABLED-TABLE-IN-QUERY-brRecord ttColumn
&Scoped-define SELF-NAME brRecord
&Scoped-define QUERY-STRING-brRecord FOR EACH ttColumn
&Scoped-define OPEN-QUERY-brRecord OPEN QUERY {&SELF-NAME} FOR EACH ttColumn.
&Scoped-define TABLES-IN-QUERY-brRecord ttColumn
&Scoped-define FIRST-TABLE-IN-QUERY-brRecord ttColumn


/* Definitions for FRAME frMain                                         */

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS brRecord tgSelAll fiNumRecords btnDecrease ~
btnOk btnClose tgWriteTrigger btnIncrease btnDatePicker btnEditor btnEncode ~
btnListEdit btnLowerCase btnUpperCase btnWordCase 
&Scoped-Define DISPLAYED-OBJECTS tgSelAll fiNumRecords tgWriteTrigger 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD increaseCharValue wEdit 
FUNCTION increaseCharValue RETURNS CHARACTER
  ( pcCharValue AS CHARACTER 
  , piDelta     AS INTEGER) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wEdit AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnClose AUTO-END-KEY 
     LABEL "&Close" 
     SIZE-PIXELS 74 BY 24.

DEFINE BUTTON btnDatePicker  NO-FOCUS FLAT-BUTTON
     LABEL "Date" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "pick a date".

DEFINE BUTTON btnDecrease  NO-FOCUS FLAT-BUTTON
     LABEL "--" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "decrease value (CTRL-CURSOR-DOWN)".

DEFINE BUTTON btnEditor  NO-FOCUS FLAT-BUTTON
     LABEL "Edit" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "view-as editor (F3)".

DEFINE BUTTON btnEncode  NO-FOCUS FLAT-BUTTON
     LABEL "Enc" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "encode the current value (F11)".

DEFINE BUTTON btnIncrease  NO-FOCUS FLAT-BUTTON
     LABEL "++" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "increase value (CTRL-CURSOR-UP)".

DEFINE BUTTON btnListEdit  NO-FOCUS FLAT-BUTTON
     LABEL "List" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "edit as list (F12)".

DEFINE BUTTON btnLowerCase  NO-FOCUS FLAT-BUTTON
     LABEL "abc" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "convert to all lower case (SHIFT-DOWN)".

DEFINE BUTTON btnOk 
     LABEL "&Ok" 
     SIZE-PIXELS 74 BY 24 TOOLTIP "confirm changes".

DEFINE BUTTON btnUpperCase  NO-FOCUS FLAT-BUTTON
     LABEL "ABC" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "convert to all upper case (SHIFT-UP)".

DEFINE BUTTON btnWordCase  NO-FOCUS FLAT-BUTTON
     LABEL "Abc" 
     SIZE-PIXELS 30 BY 23 TOOLTIP "each word begins with a capital".

DEFINE VARIABLE fiNumRecords AS CHARACTER FORMAT "X(256)":U 
     LABEL "Records" 
     VIEW-AS FILL-IN NATIVE 
     SIZE-PIXELS 50 BY 21 NO-UNDO.

DEFINE VARIABLE tgSelAll AS LOGICAL INITIAL yes 
     LABEL "" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 15 BY 15 TOOLTIP "toggle selection for all records" NO-UNDO.

DEFINE VARIABLE tgWriteTrigger AS LOGICAL INITIAL yes 
     LABEL "Use &write trigger" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 136 BY 17 TOOLTIP "Enable write triggers or not" NO-UNDO.

/* Query definitions                                                    */
&ANALYZE-SUSPEND
DEFINE QUERY brRecord FOR 
      ttColumn SCROLLING.
&ANALYZE-RESUME

/* Browse definitions                                                   */
DEFINE BROWSE brRecord
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _DISPLAY-FIELDS brRecord wEdit _FREEFORM
  QUERY brRecord DISPLAY
      ttColumn.lShow      column-label '' view-as toggle-box 
  ttColumn.iOrder
  ttColumn.cFullName 
  ttColumn.cLabel
  ttColumn.cNewValue
  enable 
  ttColumn.lShow 
  ttColumn.cNewValue
/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
    WITH NO-ROW-MARKERS SEPARATORS
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 132 BY 19
          &ELSE SIZE-PIXELS 660 BY 390 &ENDIF FIT-LAST-COLUMN TOOLTIP "fields to edit".


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frMain
     brRecord AT Y 25 X 0 WIDGET-ID 200
     tgSelAll AT Y 27 X 5 WIDGET-ID 38
     fiNumRecords AT Y 425 X 78 COLON-ALIGNED WIDGET-ID 10
     btnDecrease AT Y 0 X 210 WIDGET-ID 26
     btnOk AT Y 425 X 505 WIDGET-ID 6
     btnClose AT Y 425 X 585 WIDGET-ID 4
     tgWriteTrigger AT Y 427 X 145 WIDGET-ID 16
     btnIncrease AT Y 0 X 180 WIDGET-ID 24
     btnDatePicker AT Y 0 X 240 WIDGET-ID 34
     btnEditor AT Y 0 X 0 WIDGET-ID 36
     btnEncode AT Y 0 X 60 WIDGET-ID 12
     btnListEdit AT Y 0 X 30 WIDGET-ID 14
     btnLowerCase AT Y 0 X 120 WIDGET-ID 20
     btnUpperCase AT Y 0 X 90 WIDGET-ID 18
     btnWordCase AT Y 0 X 150 WIDGET-ID 22
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT COL 1 ROW 1 SCROLLABLE 
         CANCEL-BUTTON btnClose WIDGET-ID 100.


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
  CREATE WINDOW wEdit ASSIGN
         HIDDEN             = YES
         TITLE              = "Edit records"
         HEIGHT-P           = 455
         WIDTH-P            = 668
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
/* SETTINGS FOR WINDOW wEdit
  NOT-VISIBLE,,RUN-PERSISTENT                                           */
/* SETTINGS FOR FRAME frMain
   NOT-VISIBLE FRAME-NAME Size-to-Fit                                   */
/* BROWSE-TAB brRecord 1 frMain */
ASSIGN 
       FRAME frMain:SCROLLABLE       = FALSE
       FRAME frMain:RESIZABLE        = TRUE.

ASSIGN 
       brRecord:COLUMN-RESIZABLE IN FRAME frMain       = TRUE.

ASSIGN 
       btnEditor:HIDDEN IN FRAME frMain           = TRUE.

ASSIGN 
       btnEncode:HIDDEN IN FRAME frMain           = TRUE.

ASSIGN 
       btnListEdit:HIDDEN IN FRAME frMain           = TRUE.

ASSIGN 
       fiNumRecords:READ-ONLY IN FRAME frMain        = TRUE.

IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wEdit)
THEN wEdit:HIDDEN = yes.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME


/* Setting information for Queries and Browse Widgets fields            */

&ANALYZE-SUSPEND _QUERY-BLOCK BROWSE brRecord
/* Query rebuild information for BROWSE brRecord
     _START_FREEFORM
OPEN QUERY {&SELF-NAME} FOR EACH ttColumn.
     _END_FREEFORM
     _Query            is NOT OPENED
*/  /* BROWSE brRecord */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME wEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wEdit wEdit
ON END-ERROR OF wEdit /* Edit records */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wEdit wEdit
ON WINDOW-CLOSE OF wEdit /* Edit records */
/* OR "LEAVE" of wEdit */
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wEdit wEdit
ON WINDOW-RESIZED OF wEdit /* Edit records */
DO:
  IF FOCUS:NAME = 'cNewValue' THEN APPLY 'leave' TO FOCUS.

  RUN LockWindow (INPUT wEdit:HANDLE, INPUT YES).

  DO WITH FRAME frMain:

    /* Make 'em small so we don't get errors on resizing the window */
    btnOk:X    = 1.
    btnOk:Y    = 1.
    btnClose:X = 1.
    btnClose:Y = 1.
  
    /* Set frame width */
    FRAME frMain:WIDTH-PIXELS  = wEdit:WIDTH-PIXELS NO-ERROR.
    FRAME frMain:HEIGHT-PIXELS = wEdit:HEIGHT-PIXELS NO-ERROR.
  
    /* Adjust the browse */
    brRecord:WIDTH-PIXELS  = FRAME frMain:WIDTH-PIXELS - 3.
    brRecord:HEIGHT-PIXELS = FRAME frMain:HEIGHT-PIXELS - brRecord:Y - 35.
    
    btnClose:X     = brRecord:X + brRecord:WIDTH-PIXELS - btnClose:WIDTH-PIXELS.
    btnClose:Y     = FRAME frMain:HEIGHT-PIXELS - 27.
    btnOk:X        = btnClose:X - btnOk:WIDTH-PIXELS - 10.
    btnOk:Y        = btnClose:Y.
    fiNumRecords:Y = btnClose:Y.
    fiNumRecords:SIDE-LABEL-HANDLE:Y = fiNumRecords:y.
    tgWriteTrigger:y = fiNumRecords:Y + 2.
    tgWriteTrigger:X = fiNumRecords:X + fiNumRecords:WIDTH-PIXELS + 10.
  
    /* Save settings */
    setRegistry("DataDigger:Edit", "Window:x", STRING(wEdit:X) ).                             
    setRegistry("DataDigger:Edit", "Window:y", STRING(wEdit:Y) ).                             
    setRegistry("DataDigger:Edit", "Window:height", STRING(wEdit:HEIGHT-PIXELS) ).                             
    setRegistry("DataDigger:Edit", "Window:width", STRING(wEdit:WIDTH-PIXELS) ).                             
  END.

  RUN showScrollBars(FRAME frMain:HANDLE, NO, NO).
  RUN LockWindow (INPUT wEdit:HANDLE, INPUT NO).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME frMain
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frMain wEdit
ON GO OF FRAME frMain
DO:
  RUN btnGoChoose(OUTPUT polSuccess).
  IF NOT polSuccess THEN RETURN NO-APPLY.
  ELSE APPLY 'close' TO THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brRecord
&Scoped-define SELF-NAME brRecord
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brRecord wEdit
ON RETURN OF brRecord IN FRAME frMain
DO:
  apply 'entry' to ttColumn.cNewValue in browse brRecord.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brRecord wEdit
ON ROW-DISPLAY OF brRecord IN FRAME frMain
DO:

  /* Get field definition */
  FIND ttField WHERE ttField.cFieldName = ttColumn.cFieldName.

  /* Change color when it has been changed */
  IF ttColumn.cNewValue <> ttColumn.cOldValue THEN
    ttColumn.cNewValue:FGCOLOR IN BROWSE brRecord = 12.
  ELSE
    ttColumn.cNewValue:FGCOLOR IN BROWSE brRecord = 9.

  /* Set bgcolor of the new value field if it is mandatory */
  IF ttField.lMandatory = TRUE then
    ASSIGN
      ttColumn.lShow    :BGCOLOR IN BROWSE brRecord = 8
      ttColumn.iOrder   :BGCOLOR IN BROWSE brRecord = 8
      ttColumn.cFullName:BGCOLOR IN BROWSE brRecord = 8
      ttColumn.cLabel   :BGCOLOR IN BROWSE brRecord = 8.
  ELSE
    ASSIGN
      ttColumn.lShow    :BGCOLOR IN BROWSE brRecord = ?
      ttColumn.iOrder   :BGCOLOR IN BROWSE brRecord = ?
      ttColumn.cFullName:BGCOLOR IN BROWSE brRecord = ?
      ttColumn.cLabel   :BGCOLOR IN BROWSE brRecord = ?.

  /* Set bgcolor of the field name if it is mandatory */
/*   if ttColumn.cNewValue <> ttColumn.cOldValue then       */
/*     ttColumn.cNewValue:fgcolor in browse brRecord = 12. */
/*   else                                                 */
/*     ttColumn.cNewValue:fgcolor in browse brRecord = 9.  */

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brRecord wEdit
ON SCROLL-NOTIFY OF brRecord IN FRAME frMain
DO:
  IF FOCUS:NAME = 'cNewValue' THEN APPLY 'leave' TO FOCUS.

  RUN enableToolbar("").

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brRecord wEdit
ON START-SEARCH OF brRecord IN FRAME frMain
DO:
  apply 'leave' to ttColumn.cNewValue in browse brRecord.
  run reopenFieldBrowse(brRecord:current-column:name,?).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClose
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClose wEdit
ON CHOOSE OF btnClose IN FRAME frMain /* Close */
DO:
  define variable hToggle as handle no-undo.
  define buffer ttColumn for ttColumn. 
  define buffer ttData  for ttData. 

  /* If we are updating and we press ESC we don't want to close the 
   * window, but we want to escape out of the update mode
   */
  if focus:name = 'cNewValue' then
  do with frame {&frame-name}:
    
    find ttColumn where ttColumn.cFullName = brRecord:get-browse-column( {&field-cFullName} ):screen-value.

    /* See if there is only ONE ttData for this field.
     * The find will only succeed if there is exactly ONE record
     */
    find ttData where ttData.cFieldName = ttColumn.cFullName no-error.
    if not available ttData then ttColumn.lShow = no.

    brRecord:get-browse-column( {&field-cNewValue} ):screen-value in frame {&frame-name} = ttColumn.cOldValue.
    brRecord:get-browse-column( {&field-lShow} ):screen-value in frame {&frame-name} = string(ttColumn.lShow).

    hToggle = brRecord:get-browse-column( {&field-lShow} ) in frame {&frame-name}.
    apply 'entry' to hToggle.

    return no-apply.
  end.

  apply 'close' to this-procedure. 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDatePicker
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDatePicker wEdit
ON CHOOSE OF btnDatePicker IN FRAME frMain /* Date */
OR "F10" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DEFINE VARIABLE dDate AS DATE NO-UNDO.

  /* Check if allowed to run */
  IF NOT btnDatePicker:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  DO WITH FRAME frMain:
    dDate = DATE(FOCUS:SCREEN-VALUE).
    RUN VALUE(getProgramDir() + 'datepicker.w') (INPUT-OUTPUT dDate).
    IF dDate <> ? THEN FOCUS:SCREEN-VALUE = STRING(dDate).
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDecrease
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDecrease wEdit
ON CHOOSE OF btnDecrease IN FRAME frMain /* -- */
OR "CTRL-CURSOR-DOWN" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  /* Check if allowed to run */
  IF NOT btnDecrease:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  RUN increaseValue(-1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnEditor
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnEditor wEdit
ON CHOOSE OF btnEditor IN FRAME frMain /* Edit */
OR "F3" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  /* View-as editor */
  DEFINE VARIABLE cValue AS CHARACTER no-undo. 

  /* Check if allowed to run */
  IF NOT btnEditor:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  DO WITH FRAME frMain:
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    cValue = ttColumn.cNewValue:SCREEN-VALUE IN BROWSE brRecord.

    RUN VALUE(getProgramDir() + 'wViewAsEditor.w')
     ( INPUT-OUTPUT cValue).

    ttColumn.cNewValue:SCREEN-VALUE IN BROWSE brRecord = cValue.
    APPLY 'value-changed' to ttColumn.cNewValue IN BROWSE brRecord.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnEncode
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnEncode wEdit
ON CHOOSE OF btnEncode IN FRAME frMain /* Enc */
OR "F11" of ttColumn.cNewValue IN BROWSE brRecord
DO:
  /* Check if allowed to run */
  IF NOT btnEncode:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  DO WITH FRAME frMain:
    brRecord:GET-BROWSE-COLUMN( {&field-cNewValue} ):SCREEN-VALUE = ENCODE(brRecord:GET-BROWSE-COLUMN( {&field-cNewValue} ):SCREEN-VALUE).
    APPLY 'value-changed' TO ttColumn.cNewValue IN BROWSE brRecord.
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnIncrease
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnIncrease wEdit
ON CHOOSE OF btnIncrease IN FRAME frMain /* ++ */
OR "CTRL-CURSOR-UP" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  /* Check if allowed to run */
  IF NOT btnIncrease:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  RUN increaseValue(+1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnListEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnListEdit wEdit
ON CHOOSE OF btnListEdit IN FRAME frMain /* List */
OR "F12" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  /* List editor */
  DEFINE VARIABLE cValue AS CHARACTER no-undo. 

  /* Check if allowed to run */
  IF NOT btnListEdit:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  DO WITH FRAME frMain:
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    cValue = ttColumn.cNewValue:SCREEN-VALUE IN BROWSE brRecord.

    RUN VALUE( getProgramDir() + 'wLister.w') 
      ( INPUT picDatabase 
      , INPUT SUBSTITUTE("&1.&2", picTableName, ttColumn.cFullName)
      , INPUT-OUTPUT cValue
      ).
    ttColumn.cNewValue:SCREEN-VALUE IN BROWSE brRecord = cValue.
    APPLY 'value-changed' to ttColumn.cNewValue IN BROWSE brRecord.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnLowerCase
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnLowerCase wEdit
ON CHOOSE OF btnLowerCase IN FRAME frMain /* abc */
OR "SHIFT-CURSOR-DOWN" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  /* Check if allowed to run */
  IF NOT btnLowerCase:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  /* Make the string LOWER case */
  DO WITH FRAME frMain:
    brRecord:GET-BROWSE-COLUMN({&field-cNewValue}):SCREEN-VALUE = LOWER(brRecord:GET-BROWSE-COLUMN({&field-cNewValue}):SCREEN-VALUE).
    APPLY 'value-changed' TO ttColumn.cNewValue IN BROWSE brRecord.
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnOk
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOk wEdit
ON CHOOSE OF btnOk IN FRAME frMain /* Ok */
DO:
  RUN btnGoChoose(OUTPUT polSuccess).
  IF NOT polSuccess THEN RETURN NO-APPLY.
  ELSE APPLY 'close' TO THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnUpperCase
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnUpperCase wEdit
ON CHOOSE OF btnUpperCase IN FRAME frMain /* ABC */
OR "SHIFT-CURSOR-UP" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  /* Check if allowed to run */
  IF NOT btnUpperCase:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  RUN btnUpperCaseChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnWordCase
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnWordCase wEdit
ON CHOOSE OF btnWordCase IN FRAME frMain /* Abc */
OR "ALT-SHIFT-CURSOR-DOWN" OF ttColumn.cNewValue IN BROWSE brRecord
OR "ALT-SHIFT-CURSOR-UP" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  /* Let each word begin with a capital and let it continue in lower */
  DEFINE VARIABLE iWord AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cText AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cWord AS CHARACTER   NO-UNDO.

  /* Check if allowed to run */
  IF NOT btnWordCase:SENSITIVE 
    OR FOCUS:NAME <> 'cNewValue' THEN RETURN.

  DO WITH FRAME frMain:
    cText = brRecord:GET-BROWSE-COLUMN({&field-cNewValue}):SCREEN-VALUE.
     
    DO iWord = 1 TO NUM-ENTRIES(cText," "):
      cWord = ENTRY(iWord,cText," ").
      cWord = CAPS(SUBSTRING(cWord,1,1)) + LOWER(SUBSTRING(cWord,2)).
      ENTRY(iWord,cText," ") = cWord.
    END.

    brRecord:GET-BROWSE-COLUMN({&field-cNewValue}):SCREEN-VALUE = cText.
    APPLY 'value-changed' TO ttColumn.cNewValue IN BROWSE brRecord.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tgSelAll
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgSelAll wEdit
ON VALUE-CHANGED OF tgSelAll IN FRAME frMain
DO:
  FOR EACH ttColumn:
    ttColumn.lShow = SELF:CHECKED.
  END.

  RUN reopenFieldBrowse(?,?).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tgWriteTrigger
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgWriteTrigger wEdit
ON VALUE-CHANGED OF tgWriteTrigger IN FRAME frMain /* Use write trigger */
DO:
  setRegistry("DataDigger","EnableWriteTriggers", STRING(SELF:CHECKED) ).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK wEdit 


/* ***************************  Main Block  *************************** */

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME} 
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
DO:
  /* Save settings */
  setRegistry("DataDigger:Edit", "Window:x", STRING(wEdit:X) ).
  setRegistry("DataDigger:Edit", "Window:y", STRING(wEdit:Y) ).
  setRegistry("DataDigger:Edit", "Window:height", STRING(wEdit:HEIGHT-PIXELS) ).
  setRegistry("DataDigger:Edit", "Window:width", STRING(wEdit:WIDTH-PIXELS) ).

  RUN disable_UI.
END.

ON "RETURN" OF ttColumn.lShow IN BROWSE brRecord
DO:
  DEFINE VARIABLE hDataField AS HANDLE NO-UNDO. 
  hDataField = brRecord:GET-BROWSE-COLUMN( {&field-cNewValue} ) IN FRAME {&FRAME-NAME}.
  APPLY "ENTRY" TO hDataField.
  RETURN NO-APPLY.
END.

ON " " OF ttColumn.lShow IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
    ttColumn.lShow = NOT ttColumn.lShow.
    brRecord:GET-BROWSE-COLUMN( {&field-lShow} ):CHECKED  = ttColumn.lShow.
    APPLY "value-changed" TO ttColumn.lShow IN BROWSE brRecord.
    brRecord:SELECT-NEXT-ROW().
  END.
END.

ON VALUE-CHANGED OF ttColumn.lShow IN BROWSE brRecord
DO:
  /* If you toggle the field off, set the value to blank */
  DO WITH FRAME {&FRAME-NAME}:
    IF brRecord:GET-BROWSE-COLUMN( {&field-lShow} ):SCREEN-VALUE = "no" THEN 
    DO:
      ttColumn.cNewValue = "".
      ttColumn.lShow     = NO.
      brRecord:GET-BROWSE-COLUMN( {&field-cNewValue} ):SCREEN-VALUE = "".
    END.
  END.
END. 

ON "PAGE-DOWN" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DEFINE BUFFER bData FOR ttData.

  DO WITH FRAME frMain:
    findNext:
    FOR EACH bData
      WHERE bData.cFieldName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE
        AND bData.cValue     > SELF:SCREEN-VALUE
         BY bData.cValue:
  
      SELF:SCREEN-VALUE = bData.cValue.
      APPLY "value-changed" TO SELF.
      LEAVE findNext.
    END. 
  END. 

  RETURN NO-APPLY.
END.

ON "PAGE-UP" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DEFINE BUFFER bData FOR ttData.

  DO WITH FRAME frMain:
    findPrev:
    FOR EACH bData
      WHERE bData.cFieldName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE
        AND bData.cValue     < SELF:SCREEN-VALUE
         BY bData.cValue DESCENDING:
  
      SELF:SCREEN-VALUE = bData.cValue.
      APPLY "value-changed" TO SELF.
      LEAVE findPrev.
    END. 
  END.

  RETURN NO-APPLY.
END.

ON VALUE-CHANGED OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  ttColumn.lShow = TRUE.  
  ttColumn.cNewValue = SELF:INPUT-VALUE.
  DO WITH FRAME {&FRAME-NAME}:
    brRecord:GET-BROWSE-COLUMN( {&field-lShow} ):SCREEN-VALUE = "YES".
  END.
END.

ON "entry" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
    brRecord:TOOLTIP = "use CTRL-PGUP / CTRL-PGDN to~nbrowse through existing values".

    /* Make sure we are looking at the right field. It might have changed due to a sort */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    IF ttField.cDataType = "character" then
    DO:
      SELF:FORMAT       = ttField.cFormat.  
      SELF:SCREEN-VALUE = ttColumn.cNewValue. 
    END.

    RUN enableToolbar(ttField.cDataType).
  END.
END.

ON "leave" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
    brRecord:TOOLTIP = "fields to edit".
    ttColumn.cNewValue = ttColumn.cOldValue.
    RUN enableToolbar("").
  END.
END.

/* Set field back to original value */
ON "SHIFT-HOME" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    SELF:SCREEN-VALUE = ttColumn.cOldValue.
  END. 
END. 

/* Clean field */
ON "SHIFT-DEL" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    CASE ttField.cDataType:
      WHEN "date"      THEN SELF:SCREEN-VALUE = ?.
      WHEN "integer"   THEN SELF:SCREEN-VALUE = "0".
      WHEN "decimal"   THEN SELF:SCREEN-VALUE = "0".
      WHEN "character" THEN SELF:SCREEN-VALUE = "".
      WHEN "logical"   THEN SELF:SCREEN-VALUE = ?.
    END CASE. 
  END. 
END. 


/* Set date to today */
ON "HOME" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DEFINE VARIABLE dValue AS DATE NO-UNDO.

  DO WITH FRAME {&FRAME-NAME}:
  
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    /* If it is a date or looks like a date, treat it like a date */
    dValue = DATE(SELF:SCREEN-VALUE) NO-ERROR.
    IF dValue <> ? OR ttField.cDataType = "date"
      THEN SELF:SCREEN-VALUE = STRING(TODAY) NO-ERROR.
  END.
END.


/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  RUN enable_UI.
  RUN initializeObject.
  VIEW wEdit.

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
    WAIT-FOR CLOSE OF THIS-PROCEDURE FOCUS brRecord.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnGoChoose wEdit 
PROCEDURE btnGoChoose :
/*------------------------------------------------------------------------
  Name         : btnGoChoose
  Description  : Apply changes to all selected records
  ----------------------------------------------------------------------*/
  
  DEFINE OUTPUT PARAMETER polSuccess AS LOGICAL NO-UNDO. 

  DEFINE VARIABLE hBuffer         AS handle    NO-UNDO.
  DEFINE VARIABLE hBufferSrc      AS handle    NO-UNDO.
  DEFINE VARIABLE iNumRecs        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iRow            AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iStartTime      AS INTEGER   NO-UNDO.
  DEFINE VARIABLE lDisableTrigger AS LOGICAL   NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn. 

  /* In read-only mode, return */
  IF plReadOnlyDigger THEN 
  DO:
    polSuccess = TRUE.
    RETURN. 
  END.

  /* 2012-09-14 JEE Disable Triggers */
  lDisableTrigger = tgWriteTrigger:SCREEN-VALUE IN FRAME {&FRAME-NAME} = "no".

  /* See if any fields have been set. If not, go back. */
  IF NOT CAN-FIND(FIRST bColumn WHERE bColumn.lShow = TRUE) THEN RETURN.

  /* Prohibit editing of VST records */
  IF picTableName BEGINS "_" THEN
  DO:
    RUN showHelp("CannotEditVst", "").
    APPLY "close" TO THIS-PROCEDURE. 
    RETURN.
  END.

  /* Show that we're busy */
  iStartTime = ETIME. 
  fiNumRecords:LABEL IN FRAME frMain = "Left to save".
  PROCESS EVENTS.
  SESSION:SET-WAIT-STATE("general").

  commitLoop:
  DO TRANSACTION:

    /* Create or fetch a buffer */
    CASE picMode:
      WHEN "Add" THEN
      DO:
        iNumRecs = 1.
        CREATE BUFFER hBuffer FOR TABLE SUBSTITUTE("&1.&2",picDatabase, picTableName).
    
        IF lDisableTrigger THEN
        DO:
          hBuffer:DISABLE-LOAD-TRIGGERS(FALSE).
          hBuffer:DISABLE-DUMP-TRIGGERS( ).
        END.

        IF NOT hBuffer:BUFFER-CREATE() THEN LEAVE commitLoop.
      END.

      WHEN "Clone" THEN
      DO:
        iNumRecs = 1.
        CREATE BUFFER hBuffer FOR TABLE SUBSTITUTE("&1.&2",picDatabase, picTableName).

        IF lDisableTrigger THEN
        DO:
          hBuffer:DISABLE-LOAD-TRIGGERS(FALSE).
          hBuffer:DISABLE-DUMP-TRIGGERS( ).
        END.
        hBufferSrc = pihBrowse:QUERY:GET-BUFFER-HANDLE(1).

        IF NOT hBuffer:BUFFER-COPY(hBufferSrc,gcUniqueFields) THEN LEAVE commitLoop.
      END.

      WHEN "edit" THEN
      DO:
        ASSIGN
          hBuffer  = pihBrowse:QUERY:GET-BUFFER-HANDLE(1)
          iNumRecs = pihBrowse:NUM-SELECTED-ROWS.
    
        IF lDisableTrigger THEN
        DO:
          hBuffer:DISABLE-LOAD-TRIGGERS(FALSE).
          hBuffer:DISABLE-DUMP-TRIGGERS( ).
        END.
      END.
    END CASE. /* picMode */

    /* Process record by record */
    DO iRow = 1 TO iNumRecs:

      /* Dump the current version of the record as a backup */
      IF picMode = "Edit" THEN 
      DO:
        pihBrowse:FETCH-SELECTED-ROW(iRow).
        hBuffer:FIND-CURRENT(EXCLUSIVE-LOCK).

        RUN dumpRecord(INPUT "Update", INPUT hBuffer, OUTPUT polSuccess).
        IF NOT polSuccess THEN UNDO commitLoop, LEAVE commitLoop.
      END.

      /* Set values of all fields */
      FOR EACH bColumn WHERE bColumn.lShow = TRUE
        ON ERROR UNDO commitLoop, LEAVE commitLoop:
        /* 2016-08-08 richardk large decimal values are not correctly casted from string, 
         * last two digits of a 23 digit decimal are always zero */
        CASE hBuffer:BUFFER-FIELD(bColumn.cFieldName):DATA-TYPE: 
          WHEN "decimal" THEN hBuffer:BUFFER-FIELD(bColumn.cFieldName):BUFFER-VALUE(bColumn.iExtent) = DECIMAL(bColumn.cNewValue).
          OTHERWISE hBuffer:BUFFER-FIELD(bColumn.cFieldName):BUFFER-VALUE(bColumn.iExtent) = bColumn.cNewValue.
        END CASE.
      END. /* f/e bColumn */
      
      IF CAN-DO("Add,Clone", picMode) THEN 
      DO:
        /* Dump the newly created record as a backup */
        RUN dumpRecord(INPUT "Create", INPUT hBuffer, OUTPUT polSuccess).
        porRepositionId = hBuffer:ROWID.
        DELETE OBJECT hBuffer.
        IF NOT polSuccess THEN UNDO commitLoop, LEAVE commitLoop.
      END.
      ELSE
        hBuffer:BUFFER-RELEASE.

      /* Set nr of processed records in field "Num records" */
      IF ETIME - iStartTime > 500 THEN
      DO:
        fiNumRecords:SCREEN-VALUE IN FRAME frMain = STRING(iNumRecs - iRow).
        PROCESS EVENTS.
        iStartTime = ETIME. 
      END.

    END. /* do iRow */

    polSuccess = TRUE.
  END. /* transaction */

  /* Unfreeze the window */
  SESSION:SET-WAIT-STATE("").
  
END PROCEDURE. /* btnGoChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnLowerCaseChoose wEdit 
PROCEDURE btnLowerCaseChoose :
/*------------------------------------------------------------------------
  Name         : btnLowerCaseChoose
  Description  : Make the string LOWER case
  ----------------------------------------------------------------------*/

  IF FOCUS:NAME = 'cNewValue' THEN
  DO WITH FRAME frMain:
    brRecord:GET-BROWSE-COLUMN({&field-cNewValue}):SCREEN-VALUE = LOWER(brRecord:GET-BROWSE-COLUMN({&field-cNewValue}):SCREEN-VALUE).
    APPLY 'value-changed' TO ttColumn.cNewValue IN BROWSE brRecord.
  END.

END PROCEDURE. /* btnLowerCaseChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnUpperCaseChoose wEdit 
PROCEDURE btnUpperCaseChoose :
/*------------------------------------------------------------------------
  Name         : btnUpperCaseChoose
  Description  : Make the string UPPER case
  ----------------------------------------------------------------------*/

  IF FOCUS:NAME = 'cNewValue' THEN
  DO WITH FRAME frMain:
    brRecord:GET-BROWSE-COLUMN({&field-cNewValue}):SCREEN-VALUE = UPPER(brRecord:GET-BROWSE-COLUMN({&field-cNewValue}):SCREEN-VALUE).
    APPLY 'value-changed' TO ttColumn.cNewValue IN BROWSE brRecord.
  END.

END PROCEDURE. /* btnUpperCaseChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI wEdit  _DEFAULT-DISABLE
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
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wEdit)
  THEN DELETE WIDGET wEdit.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enableToolbar wEdit 
PROCEDURE enableToolbar :
/*------------------------------------------------------------------------
  Name         : enableToolbar
  Description  : Enable/disable buttons on the toolbar
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER pcDataType AS CHARACTER NO-UNDO.

  DO WITH FRAME frMain:
    btnEditor    :SENSITIVE = CAN-DO("CHARACTER",pcDataType).
    btnEncode    :SENSITIVE = CAN-DO("CHARACTER",pcDataType).
    btnListEdit  :SENSITIVE = CAN-DO("CHARACTER",pcDataType).
    btnUpperCase :SENSITIVE = CAN-DO("CHARACTER",pcDataType).
    btnLowerCase :SENSITIVE = CAN-DO("CHARACTER",pcDataType).
    btnWordCase  :SENSITIVE = CAN-DO("CHARACTER",pcDataType).
    btnIncrease  :SENSITIVE = CAN-DO("CHARACTER,INTEGER,DECIMAL,INT64,DATE*",pcDataType).
    btnDecrease  :SENSITIVE = CAN-DO("CHARACTER,INTEGER,DECIMAL,INT64,DATE*",pcDataType).
    btnDatePicker:SENSITIVE = CAN-DO("DATE",pcDataType).
  END.

END PROCEDURE. /* enableToolbar */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI wEdit  _DEFAULT-ENABLE
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
  DISPLAY tgSelAll fiNumRecords tgWriteTrigger 
      WITH FRAME frMain IN WINDOW wEdit.
  ENABLE brRecord tgSelAll fiNumRecords btnDecrease btnOk btnClose 
         tgWriteTrigger btnIncrease btnDatePicker btnEditor btnEncode 
         btnListEdit btnLowerCase btnUpperCase btnWordCase 
      WITH FRAME frMain IN WINDOW wEdit.
  {&OPEN-BROWSERS-IN-QUERY-frMain}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getDataValues wEdit 
PROCEDURE getDataValues :
/*------------------------------------------------------------------------
  Name         : getDataValues
  Description  : Collect all values in the selected records
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER phBrowse AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER pcColumn AS CHARACTER   NO-UNDO.
  
  DEFINE VARIABLE cRowValue AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBuffer   AS handle      NO-UNDO.
  DEFINE VARIABLE iRow      AS INTEGER     NO-UNDO.

  DEFINE BUFFER bData FOR ttData. 

  hBuffer = phBrowse:QUERY:GET-BUFFER-HANDLE(1).

  addValue:
  DO iRow = 1 TO phBrowse:NUM-SELECTED-ROWS:
    phBrowse:FETCH-SELECTED-ROW(iRow).
    cRowValue = hBuffer:BUFFER-FIELD(ttColumn.cFieldName):BUFFER-VALUE(ttColumn.iExtent).

    /* Already in the set or not? */
    FIND bData 
      WHERE bData.cFieldName = pcColumn 
        AND bData.cValue     = cRowValue
            NO-ERROR.

    IF NOT AVAILABLE bData THEN
    DO:
      CREATE bData.
      ASSIGN bData.cFieldName = pcColumn 
             bData.cValue     = cRowValue.
    END.
  END. 

END PROCEDURE. /* getDataValues */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE increaseValue wEdit 
PROCEDURE increaseValue :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER piDelta AS INTEGER NO-UNDO.

  DEFINE VARIABLE cScreenValue AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iValue  AS INTEGER     NO-UNDO.
  DEFINE VARIABLE daValue AS DATE        NO-UNDO.
  DEFINE VARIABLE deValue AS DECIMAL     NO-UNDO.
  DEFINE VARIABLE lValue  AS LOGICAL     NO-UNDO.

  IF FOCUS:NAME = 'cNewValue' THEN
  DO WITH FRAME frMain:

    /* Make sure we are looking at the right field. It might have changed due to a sort */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN({&field-cFullName}):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.
  
    /* Get current value on the screen */
    cScreenValue = ttColumn.cNewValue:SCREEN-VALUE IN BROWSE brRecord.

    CASE ttField.cDataType:
      WHEN "INTEGER" OR WHEN "INT64" THEN iValue  = INTEGER(cScreenValue) NO-ERROR.
      WHEN "DATE"                    THEN daValue = DATE(cScreenValue)    NO-ERROR.
      WHEN "DECIMAL"                 THEN deValue = DECIMAL(cScreenValue) NO-ERROR.
      WHEN "LOGICAL"                 THEN lValue  = LOGICAL(cScreenValue) NO-ERROR.
      WHEN "CHARACTER"               THEN cScreenValue = increaseCharValue(cScreenValue,piDelta) NO-ERROR.
    END CASE. 

    /* Use default value if date fails */
    IF daValue = ? THEN daValue = TODAY - piDelta. 

    IF NOT ERROR-STATUS:ERROR THEN
    DO:
      CASE ttField.cDataType:
        WHEN "INTEGER" OR WHEN "INT64" THEN cScreenValue = STRING(iValue  + piDelta).
        WHEN "DATE"                    THEN cScreenValue = STRING(daValue + piDelta).
        WHEN "DECIMAL"                 THEN cScreenValue = STRING(deValue + piDelta).
        WHEN "LOGICAL"                 THEN cScreenValue = STRING(NOT lValue) NO-ERROR.
      END CASE. 

      ttColumn.cNewValue:SCREEN-VALUE = cScreenValue.
    END.

    APPLY 'value-changed' TO ttColumn.cNewValue IN BROWSE brRecord.
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject wEdit 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------
  Name         : initializeObject
  Description  : Setup
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE cExtentFormat   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSetting        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cValueList      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBuffer         AS handle      NO-UNDO.
  DEFINE VARIABLE iFieldExtent    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMaxFieldLength AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iValue          AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lNewRecord      AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE iDefaultFont    AS INTEGER     NO-UNDO.

  DEFINE BUFFER bColumn FOR ttColumn. 

  /* Get fonts */
  iDefaultFont = getFont('Default').
  FRAME {&FRAME-NAME}:FONT = iDefaultFont.
  BROWSE brRecord:FONT = iDefaultFont.
  BROWSE brRecord:ROW-HEIGHT-PIXELS = font-table:GET-TEXT-HEIGHT-PIXELS(iDefaultFont).
  RUN setLabelPosition(fiNumRecords:HANDLE).

  /* This program is called for both ADD and EDIT */
  lNewRecord = (picMode = 'add').

  /* If we add a new record, enable all fields that are either
   * part of a unique index or are mandatory
   */
  IF CAN-DO('Add,Clone',picMode) THEN
  FOR EACH ttField 
    WHERE ttField.lMandatory = TRUE
       OR ttField.lUniqueIdx = TRUE
   , EACH ttColumn
    WHERE ttColumn.cFieldName = ttField.cFieldname:

    ttColumn.lShow = TRUE.
    gcUniqueFields = gcUniqueFields + "," + ttColumn.cFieldName.
  END.
  gcUniqueFields = TRIM(gcUniqueFields,",").

  /* Get rid of all hidden fields. Since the tt is a COPY of the tt
   * in the main window we can safely delete them. While we're at 
   * it, get rid of other trash as well
   */
  FOR EACH ttField 
    WHERE ttField.lShow      = FALSE   /* Hidden by user     */
       OR ttField.cFieldName = "RECID"
       OR ttField.cFieldName = "ROWID"
       OR ttField.cDataType  = "CLOB"
       OR ttField.cDataType  = "BLOB"
       OR ttField.cDataType  BEGINS "RAW"
    , EACH ttColumn
     WHERE ttColumn.cFieldName = ttField.cFieldname:
    
    DELETE ttColumn.
  END.

  /* Find out max fieldname length */
  FOR EACH ttColumn: 
    ttColumn.cFilterValue = ''.    /* cFilterValue is now the list of currently used values */
    ttColumn.lShow        = FALSE. /* lShow now means: "Change this field" */
    iMaxFieldLength      = MAXIMUM(iMaxFieldLength,LENGTH(ttColumn.cFullName)).
  END.

  /* Collect data for all fields 
   * And if we only have 1 value for all selected records, let's show that 
   */
  FOR EACH ttColumn: 
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    IF CAN-DO('Clone,Edit',picMode) THEN
      RUN getDataValues(pihBrowse,ttColumn.cFullName).

    FIND ttData WHERE ttData.cFieldName = ttColumn.cFullName NO-ERROR.
    IF AVAILABLE ttData THEN
    DO:
      ASSIGN 
        ttColumn.cOldValue = ttData.cValue /* so we can revert to the old value */
        ttColumn.cNewValue = ttData.cValue
        ttColumn.lShow     = TRUE. 

      /* If the data is longer than the format allows, adjust format */
      IF ttField.cDatatype = 'character'
        AND LENGTH(ttColumn.cNewValue) > LENGTH( STRING(ttColumn.cNewValue,ttField.cFormat)) THEN
        ttField.cFormat = SUBSTITUTE('x(&1)', LENGTH(ttColumn.cNewValue)).

    END.
  END.

  DO WITH FRAME {&FRAME-NAME}:

    /* Hide encode-button */
    RUN enableToolbar("").

    /* Use triggers? */
    cSetting = getRegistry("DataDigger","EnableWriteTriggers").
    IF cSetting = ? THEN cSetting = "yes".
    tgWriteTrigger:CHECKED = LOGICAL(cSetting).

    /* Adjust column width to fit precisely */
    brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):WIDTH-CHARS = iMaxFieldLength + 2.
    
    /* Window position and size */
    /* Set title of the window */
    wEdit:TITLE = SUBSTITUTE('&1 - &2.&3'
                            , picMode
                            , picDatabase 
                            , picTableName
                            ).

    /* Num records */
    fiNumRecords:SCREEN-VALUE = STRING(IF CAN-DO('Add,Clone',picMode) THEN 1 ELSE pihBrowse:NUM-SELECTED-ROWS).

    /* Set minimum size of the window */
    wEdit:MIN-WIDTH-PIXELS  = 400.
    wEdit:MIN-HEIGHT-PIXELS = 200.

    /* to avoid scrollbars on the frame */
    FRAME {&FRAME-NAME}:SCROLLABLE = FALSE.

    iValue = INTEGER(getRegistry('DataDigger:Edit', 'Window:x' )).
    IF iValue = ? THEN iValue = INTEGER(getRegistry('DataDigger', 'Window:x' )) + 50.
    ASSIGN wEdit:X = iValue NO-ERROR.

    /* Window has been parked at y=-1000 to get it out of sight */
    iValue = INTEGER(getRegistry('DataDigger:Edit', 'Window:y' )).
    IF iValue = ? THEN iValue = INTEGER(getRegistry('DataDigger', 'Window:y' )) + 50.
    IF iValue <> ? THEN ASSIGN wEdit:Y = iValue NO-ERROR.

    iValue = INTEGER(getRegistry('DataDigger:Edit', 'Window:height' )).
    IF iValue = ? OR iValue = 0 THEN iValue = INTEGER(getRegistry('DataDigger', 'Window:height' )) - 100. 
    ASSIGN wEdit:HEIGHT-PIXELS = iValue NO-ERROR.

    iValue = INTEGER(getRegistry('DataDigger:Edit', 'Window:width' )).
    IF iValue = ? OR iValue = 0 THEN iValue = INTEGER(getRegistry('DataDigger', 'Window:width' )) - 100.
    ASSIGN wEdit:WIDTH-PIXELS = iValue NO-ERROR.

    DO WITH FRAME frMain:
      btnEditor:LOAD-IMAGE(getImagePath('Editor.gif')).
      btnEncode:LOAD-IMAGE(getImagePath('Encode.gif')).
      btnListEdit:LOAD-IMAGE(getImagePath('List.gif')).
      btnDatePicker:LOAD-IMAGE(getImagePath('DatePicker.gif')).
    END. 
  END. 

  /* Force a redraw */
  APPLY 'window-resized' TO wEdit.

  /* Open the browse */
  {&OPEN-QUERY-brRecord}
   
  /* Restore sort */
  cSetting = getRegistry('DataDigger','ColumnSortRecord').
  IF cSetting <> ? THEN
    brRecord:SET-SORT-ARROW(INTEGER(ENTRY(1,cSetting)), LOGICAL(ENTRY(2,cSetting)) ).

  RUN reopenFieldBrowse(?,?).

  IF plReadOnlyDigger THEN btnOk:SENSITIVE = FALSE.

  /* Start listener to table changes in main window */
  SUBSCRIBE TO 'TableChange' ANYWHERE. 

END PROCEDURE. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reopenFieldBrowse wEdit 
PROCEDURE reopenFieldBrowse :
/*------------------------------------------------------------------------
  Name         : reopenFieldBrowse
  Description  : Open the field browse again, taking into account the 
                 filter values the user has entered. 
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER pcSortField AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER plAscending AS LOGICAL     NO-UNDO.

  DEFINE VARIABLE cNewSort       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cOldSort       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBuffer        AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hColumn        AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery         AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iColumn        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lAscending     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE rCurrentRecord AS ROWID       NO-UNDO.

  /* Protect routine against invalid input */
  IF pcSortField = "" THEN pcSortField = ?.

  /* Remember record we're on */
  brRecord:SELECT-FOCUSED-ROW() IN FRAME {&FRAME-NAME}.
  brRecord:FETCH-SELECTED-ROW(1) IN FRAME {&FRAME-NAME}.

  IF brRecord:NUM-SELECTED-ROWS IN FRAME {&FRAME-NAME} > 0 THEN 
    rCurrentRecord = brRecord:QUERY:GET-BUFFER-HANDLE(1):ROWID.

  /* Find out what the current sort is */
  RUN getColumnSort(INPUT brRecord:HANDLE, OUTPUT cOldSort, OUTPUT lAscending).

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
  RUN setSortArrow(brRecord:HANDLE, cNewSort, lAscending).

  /* Close open query */
  IF VALID-HANDLE(brRecord:QUERY) THEN brRecord:QUERY:QUERY-CLOSE().

  /* Build the query */
  CREATE QUERY hQuery.
  CREATE BUFFER hBuffer FOR TABLE "ttColumn".
  hQuery:SET-BUFFERS(hBuffer).

  cQuery = SUBSTITUTE("for each ttColumn by &1 &2", cNewSort, STRING(lAscending,"/descending")).

  hQuery:QUERY-PREPARE(cQuery).
  hQuery:QUERY-OPEN().
  hQuery:GET-FIRST.

  /* Attach query to the browse */
  brRecord:QUERY IN FRAME {&FRAME-NAME} = hQuery.

  /* Jump back to selected row */
  IF NOT hQuery:QUERY-OFF-END 
    AND CAN-FIND(ttColumn WHERE ROWID(ttColumn) = rCurrentRecord) THEN
  DO:
    hQuery:REPOSITION-TO-ROWID(rCurrentRecord) NO-ERROR.
    brRecord:SELECT-FOCUSED-ROW().
  END.

END PROCEDURE. /* reopenFieldBrowse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE tableChange wEdit 
PROCEDURE tableChange :
/* Event handler for 'TableChange' event of main window
 */
  DEFINE INPUT PARAMETER pcNewDatabase AS CHARACTER NO-UNDO.
  DEFINE INPUT PARAMETER pcNewTable    AS CHARACTER NO-UNDO.

  IF   pcNewDatabase <> picDatabase
    OR pcNewTable <> picTableName THEN APPLY 'close' TO THIS-PROCEDURE. 

END PROCEDURE. /* tableChange */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* ************************  Function Implementations ***************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION increaseCharValue wEdit 
FUNCTION increaseCharValue RETURNS CHARACTER
  ( pcCharValue AS CHARACTER 
  , piDelta     AS INTEGER):

  DEFINE VARIABLE cChar   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iChar   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cNumber AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLeft   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cRight  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iNumber AS INTEGER     NO-UNDO.
  DEFINE VARIABLE dValue  AS DATE        NO-UNDO.
  
  /* If the complete value looks like a date, 
   * then treat it like a date 
   */
  IF pcCharValue MATCHES "*/*/*" THEN
  DO:
    dValue = DATE(pcCharValue) NO-ERROR.
    IF dValue <> ? THEN
    DO:
      dValue = dValue + piDelta.
      RETURN STRING(dValue).
    END.
  END.

  /* Otherwise look for the first number in the string. 
   * Extract it and remember what is at the left and 
   * at the right of the number 
   */
  DO iChar = 1 TO LENGTH(pcCharValue):
    cChar = SUBSTRING(pcCharValue,iChar,1).
    IF LOOKUP(cChar,"0,1,2,3,4,5,6,7,8,9") > 0 THEN
      cNumber = cNumber + cChar.
    ELSE 
    DO:
      IF cNumber <> "" THEN 
      DO:
        cRight = SUBSTRING(pcCharValue,iChar).
        LEAVE.
      END.
  
      /* Collect all that is left of the nr */
      cLeft = cLeft + cChar.
    END.
  END.

  /* If we have found a number, increase it */
  IF cNumber <> "" THEN
  DO:
    iNumber = INTEGER(cNumber) NO-ERROR.
    IF NOT ERROR-STATUS:ERROR 
      AND (iNumber + piDelta) >= 0 THEN cNumber = STRING(iNumber + piDelta).
  END.

  /* Now return the left part + new number + right part */
  pcCharValue = SUBSTITUTE("&1&2&3",cLeft,cNumber,cRight).
  RETURN pcCharValue. /* Function return value. */

END FUNCTION. /* increaseCharValue */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

