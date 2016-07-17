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
/* { windows.i } */

/* Parameters Definitions ---                                           */
define {&invar} picMode           as character  no-undo.
define {&invar} pihBrowse         as handle     no-undo.
define {&invar} picDatabase       as character  no-undo.
define {&invar} picTableName      as character  no-undo.

&if defined(UIB_is_Running) = 0 &then
define {&invar} table for ttField.
define {&invar} table for ttColumn.
&endif

define {&outvar} polSuccess        as logical    no-undo initial ?.
define {&outvar} porRepositionId   as rowid      no-undo.

/* Local Variable Definitions ---                                       */
define variable gcUniqueFields as character no-undo. 
define variable glInEditMode   as logical   no-undo. 

/* This table holds the actual values of the selected records */
define temp-table ttData no-undo rcode-information
  field cFieldName    as character
  field cValue        as character format 'x(80)'
  index iPrim is primary cFieldName /* cValue */.


&global-define field-lShow     1
&global-define field-iOrder    2
&global-define field-cFullName 3
&global-define field-cLabel    4
&global-define field-cNewValue 5

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME frEdit
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


/* Definitions for FRAME frEdit                                         */

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS brRecord btnEncode fiNumRecords btnOk ~
btnClose tgWriteTrigger btnListEdit 
&Scoped-Define DISPLAYED-OBJECTS fiNumRecords tgWriteTrigger 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wEdit AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnClose AUTO-END-KEY 
     LABEL "&Close" 
     SIZE-PIXELS 74 BY 24.

DEFINE BUTTON btnEncode  NO-FOCUS FLAT-BUTTON
     LABEL "Encode" 
     SIZE-PIXELS 20 BY 14 TOOLTIP "encode the current value (F11)".

DEFINE BUTTON btnListEdit  NO-FOCUS FLAT-BUTTON
     LABEL "List Editor" 
     SIZE-PIXELS 20 BY 14 TOOLTIP "edit as list (F12)".

DEFINE BUTTON btnOk 
     LABEL "&Ok" 
     SIZE-PIXELS 74 BY 24 TOOLTIP "start the dump".

DEFINE VARIABLE fiNumRecords AS CHARACTER FORMAT "X(256)":U 
     LABEL "Records Selected" 
     VIEW-AS FILL-IN NATIVE 
     SIZE-PIXELS 50 BY 21 NO-UNDO.

DEFINE VARIABLE tgWriteTrigger AS LOGICAL INITIAL yes 
     LABEL "Write trigger" 
     VIEW-AS TOGGLE-BOX
     SIZE 18 BY .81 TOOLTIP "Enable write triggers or not" NO-UNDO.

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
          &IF '{&WINDOW-SYSTEM}' = 'TTY':U &THEN SIZE 133 BY 21
          &ELSE SIZE-PIXELS 665 BY 450 &ENDIF FIT-LAST-COLUMN TOOLTIP "fields to edit".


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frEdit
     brRecord AT Y 5 X 0 WIDGET-ID 200
     btnEncode AT Y 465 X 250 WIDGET-ID 12
     fiNumRecords AT Y 465 X 90 COLON-ALIGNED WIDGET-ID 10
     btnOk AT Y 465 X 505 WIDGET-ID 6
     btnClose AT Y 465 X 585 WIDGET-ID 4
     tgWriteTrigger AT ROW 23.24 COL 32 WIDGET-ID 16
     btnListEdit AT Y 465 X 275 WIDGET-ID 14
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
         HEIGHT-P           = 491
         WIDTH-P            = 665
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
/* SETTINGS FOR FRAME frEdit
   NOT-VISIBLE FRAME-NAME Size-to-Fit                                   */
/* BROWSE-TAB brRecord 1 frEdit */
ASSIGN 
       FRAME frEdit:SCROLLABLE       = FALSE
       FRAME frEdit:RESIZABLE        = TRUE.

ASSIGN 
       brRecord:COLUMN-RESIZABLE IN FRAME frEdit       = TRUE.

ASSIGN 
       btnEncode:HIDDEN IN FRAME frEdit           = TRUE.

ASSIGN 
       btnListEdit:HIDDEN IN FRAME frEdit           = TRUE.

ASSIGN 
       fiNumRecords:READ-ONLY IN FRAME frEdit        = TRUE.

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
or "LEAVE" of wEdit
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
  if focus:name = 'cNewValue' then apply 'leave' to focus.

  run LockWindow (input wEdit:handle, input yes).

  do with frame frEdit:

    /* Make 'em small so we don't get errors on resizing the window */
    btnOk:x    = 1.
    btnOk:y    = 1.
    btnClose:x = 1.
    btnClose:y = 1.
  
    /* Set frame width */
    frame frEdit:width-pixels  = wEdit:width-pixels no-error.
    frame frEdit:height-pixels = wEdit:height-pixels no-error.
  
    /* Adjust the browse */
    brRecord:width-pixels  = frame frEdit:width-pixels - 3.
    brRecord:height-pixels = frame frEdit:height-pixels - 40.
    
    btnOk:x        = brRecord:width-pixels  - 160.
    btnOk:y        = frame frEdit:height-pixels - 30.
    btnClose:x     = brRecord:width-pixels - 80.
    btnClose:y     = frame frEdit:height-pixels - 30.
    fiNumRecords:y = frame frEdit:height-pixels - 30.
    fiNumRecords:side-label-handle:y = fiNumRecords:y.
    tgWriteTrigger:y = fiNumRecords:y + 2.
    tgWriteTrigger:x = fiNumRecords:x + fiNumRecords:width-pixels + 10.
  
    /* Save settings */
    setRegistry("DataDigger:Edit", "Window:x", string(wEdit:x) ).                             
    setRegistry("DataDigger:Edit", "Window:y", string(wEdit:y) ).                             
    setRegistry("DataDigger:Edit", "Window:height", string(wEdit:height-pixels) ).                             
    setRegistry("DataDigger:Edit", "Window:width", string(wEdit:width-pixels) ).                             
  end.

  run showScrollBars(frame frEdit:handle, no, no).
  run LockWindow (input wEdit:handle, input no).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME frEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frEdit wEdit
ON GO OF FRAME frEdit
DO:
  run btnGoChoose(output polSuccess).
  if not polSuccess then return no-apply.
  else apply 'close' to this-procedure.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME brRecord
&Scoped-define SELF-NAME brRecord
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brRecord wEdit
ON RETURN OF brRecord IN FRAME frEdit
DO:
  apply 'entry' to ttColumn.cNewValue in browse brRecord.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brRecord wEdit
ON ROW-DISPLAY OF brRecord IN FRAME frEdit
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
ON SCROLL-NOTIFY OF brRecord IN FRAME frEdit
DO:
  if focus:name = 'cNewValue' then apply 'leave' to focus.
  btnEncode:hidden = yes.
  btnListEdit:hidden = yes.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL brRecord wEdit
ON START-SEARCH OF brRecord IN FRAME frEdit
DO:
  apply 'leave' to ttColumn.cNewValue in browse brRecord.
  run reopenFieldBrowse(brRecord:current-column:name,?).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnClose
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnClose wEdit
ON CHOOSE OF btnClose IN FRAME frEdit /* Close */
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


&Scoped-define SELF-NAME btnEncode
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnEncode wEdit
ON CHOOSE OF btnEncode IN FRAME frEdit /* Encode */
OR "F11" of ttColumn.cNewValue in browse brRecord
DO:
  if focus:name = 'cNewValue' then
  do:
    brRecord:get-browse-column( {&field-cNewValue} ):screen-value = encode(brRecord:get-browse-column( {&field-cNewValue} ):screen-value).
    apply 'value-changed' to ttColumn.cNewValue in browse brRecord.
  end.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnListEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnListEdit wEdit
ON CHOOSE OF btnListEdit IN FRAME frEdit /* List Editor */
or "F12" of ttColumn.cNewValue in browse brRecord
DO:

  /* List editor */
  define variable cValue as character no-undo. 

  do with frame {&frame-name}:
    /* Make sure we are looking at the right field. */
    find ttColumn where ttColumn.cFullName = brRecord:get-browse-column( {&field-cFullName} ):screen-value.
    cValue = ttColumn.cNewValue:screen-value in browse brRecord.

    run value( getProgramDir() + 'wLister.w') 
      ( input picDatabase 
      , input substitute("&1.&2", picTableName, ttColumn.cFullName)
      , input-output cValue
      ).
    ttColumn.cNewValue:screen-value in browse brRecord = cValue.
    apply 'value-changed' to ttColumn.cNewValue in browse brRecord.
  end.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnOk
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOk wEdit
ON CHOOSE OF btnOk IN FRAME frEdit /* Ok */
do:
  run btnGoChoose(output polSuccess).
  if not polSuccess then return no-apply.
  else apply 'close' to this-procedure.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tgWriteTrigger
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tgWriteTrigger wEdit
ON VALUE-CHANGED OF tgWriteTrigger IN FRAME frEdit /* Write trigger */
DO:
  setRegistry("DataDigger","EnableWriteTriggers", string(self:checked) ).
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

  DO WITH FRAME frEdit:
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

  DO WITH FRAME frEdit:
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
    btnEncode:X = brRecord:X + brRecord:WIDTH-PIXELS - btnEncode:WIDTH-PIXELS - 18.
    btnEncode:Y = brRecord:Y + SELF:Y + 1.
    btnEncode:HIDDEN = NO.
    btnEncode:MOVE-TO-TOP().

    btnListEdit:X = btnEncode:X - btnEncode:WIDTH-PIXELS.
    btnListEdit:Y = btnEncode:Y.
    btnListEdit:HIDDEN = NO.
    btnListEdit:MOVE-TO-TOP().

    brRecord:TOOLTIP = "use CTRL-PGUP / CTRL-PGDN to~nbrowse through existing values".

    /* Make sure we are looking at the right field. It might have changed due to a sort */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    IF ttField.cDataType = "character" then
    DO:
      SELF:FORMAT       = ttField.cFormat.  
      SELF:SCREEN-VALUE = ttColumn.cNewValue. 
    END.
  END.
END.

ON "leave" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
    brRecord:TOOLTIP = "fields to edit".

    btnEncode:X = 1.
    btnEncode:Y = 1.
    btnEncode:HIDDEN = TRUE.

    btnListEdit:X = 1.
    btnListEdit:Y = 1.
    btnListEdit:HIDDEN = TRUE.
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

/* Increase field or set to all caps */
ON "SHIFT-CURSOR-UP" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
  
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    CASE ttField.cDataType:
      WHEN "date"      THEN SELF:SCREEN-VALUE = STRING( DATE(SELF:SCREEN-VALUE) + 1 ) NO-ERROR.
      WHEN "integer"   THEN SELF:SCREEN-VALUE = STRING( INTEGER(SELF:SCREEN-VALUE) + 1 ) NO-ERROR.
      WHEN "decimal"   THEN SELF:SCREEN-VALUE = STRING( DECIMAL(SELF:SCREEN-VALUE) + 1 ) NO-ERROR.
      WHEN "character" THEN SELF:SCREEN-VALUE = CAPS(SELF:SCREEN-VALUE) NO-ERROR.
      WHEN "logical"   THEN SELF:SCREEN-VALUE = STRING( NOT LOGICAL(SELF:SCREEN-VALUE) ) NO-ERROR.
    END CASE. 
  END.
END.

/* Decrease field or set to all lower */
ON "SHIFT-CURSOR-DOWN" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
  
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    CASE ttField.cDataType:
      WHEN "date"      THEN SELF:SCREEN-VALUE = STRING( DATE(SELF:SCREEN-VALUE) - 1 ) NO-ERROR.
      WHEN "integer"   THEN SELF:SCREEN-VALUE = STRING( INTEGER(SELF:SCREEN-VALUE) - 1 ) NO-ERROR.
      WHEN "decimal"   THEN SELF:SCREEN-VALUE = STRING( DECIMAL(SELF:SCREEN-VALUE) - 1 ) NO-ERROR.
      WHEN "character" THEN SELF:SCREEN-VALUE = LOWER(SELF:SCREEN-VALUE) NO-ERROR.
      WHEN "logical"   THEN SELF:SCREEN-VALUE = STRING( NOT LOGICAL(SELF:SCREEN-VALUE) ) NO-ERROR.
    END CASE. 
  END.
END.

/* Increase date with 1 month */
ON "SHIFT-PAGE-UP" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
  
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    CASE ttField.cDataType:
      WHEN "date" THEN SELF:SCREEN-VALUE = STRING( ADD-INTERVAL( DATE(SELF:SCREEN-VALUE), 1, "MONTH" )) NO-ERROR.
    END CASE. 
  END.
END.

/* Decrease date with 1 month */
ON "SHIFT-PAGE-DOWN" OF ttColumn.cNewValue IN BROWSE brRecord
DO:
  DO WITH FRAME {&FRAME-NAME}:
    /* Make sure we are looking at the right field. */
    FIND ttColumn WHERE ttColumn.cFullName = brRecord:GET-BROWSE-COLUMN( {&field-cFullName} ):SCREEN-VALUE.
    FIND ttField WHERE ttField.cFieldname = ttColumn.cFieldName.

    CASE ttField.cDataType:
      WHEN "date" THEN SELF:SCREEN-VALUE = STRING( ADD-INTERVAL( DATE(SELF:SCREEN-VALUE), -1, "MONTH" )) NO-ERROR.
    END CASE. 
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
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
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
  IF ReadOnlyDigger THEN 
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
  fiNumRecords:LABEL IN FRAME frEdit = "Left to save".
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

        hBuffer:BUFFER-FIELD(bColumn.cFieldName):BUFFER-VALUE(bColumn.iExtent) = bColumn.cNewValue.
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
        fiNumRecords:SCREEN-VALUE IN FRAME frEdit = STRING(iNumRecs - iRow).
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
  DISPLAY fiNumRecords tgWriteTrigger 
      WITH FRAME frEdit IN WINDOW wEdit.
  ENABLE brRecord btnEncode fiNumRecords btnOk btnClose tgWriteTrigger 
         btnListEdit 
      WITH FRAME frEdit IN WINDOW wEdit.
  {&OPEN-BROWSERS-IN-QUERY-frEdit}
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

  DEFINE BUFFER bColumn FOR ttColumn. 

  /* Get fonts */
  FRAME {&FRAME-NAME}:FONT = getFont('Default').
  BROWSE brRecord:FONT = getFont('Default').

  /* This program is called for both ADD and EDIT */
  lNewRecord = picMode = 'add'.

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
   * And  if we only have 1 value for all selected records, let's show that 
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
    btnEncode:HIDDEN   = YES.
    btnListEdit:HIDDEN = YES.

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

    btnEncode:LOAD-IMAGE(getImagePath('Encode.gif')).
    btnListEdit:LOAD-IMAGE(getImagePath('List.gif')).
  END. 

  /* Force a redraw */
  APPLY 'window-resized' TO wEdit.

  /* Open the browse */
  /*
  run reopenFieldBrowse('iOrder',yes).
  */
  {&OPEN-QUERY-brRecord}
   
  /* Restore sort */
  cSetting = getRegistry('DataDigger','ColumnSortRecord').
  IF cSetting <> ? THEN
    brRecord:SET-SORT-ARROW(INTEGER(ENTRY(1,cSetting)), LOGICAL(ENTRY(2,cSetting)) ).

  RUN reopenFieldBrowse(?,?).

  IF ReadOnlyDigger THEN btnOk:SENSITIVE = FALSE.

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

