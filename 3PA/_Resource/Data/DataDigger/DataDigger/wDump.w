&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME wDump
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS wDump 
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
DEFINE INPUT PARAMETER pihBrowse         AS HANDLE     NO-UNDO.
DEFINE INPUT PARAMETER picSelectedFields AS CHARACTER  NO-UNDO.
DEFINE INPUT PARAMETER table FOR ttField.


/* Global Variable Definitions ---                                      */
DEFINE VARIABLE gcDB                   AS CHARACTER  NO-UNDO.
DEFINE VARIABLE gcTable                AS CHARACTER  NO-UNDO.
DEFINE VARIABLE gcFileName             AS CHARACTER  NO-UNDO.
DEFINE VARIABLE gcLastFile             AS CHARACTER  NO-UNDO.
DEFINE VARIABLE gcFileViewCmd          AS CHARACTER  NO-UNDO.
DEFINE VARIABLE glNoRecordsWarning     AS LOGICAL    NO-UNDO.
DEFINE VARIABLE gcSessionNumericFormat AS CHARACTER  NO-UNDO.
DEFINE VARIABLE gcSessionDateFormat    AS CHARACTER  NO-UNDO.
DEFINE VARIABLE glAborted              AS LOGICAL    NO-UNDO.

DEFINE STREAM strDump.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME DEFAULT-FRAME

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS RECT-2 RECT-3 RECT-4 ficFileName ~
btnChooseDumpFile cbDumpType cbCodePage tbUseCustomizedFormats cbSeparator ~
cbNumericFormat cbiRecordSelection cbDateFormat cbiFieldSelection btnDump ~
btnClose tbDumpReadyClose tbDumpReadyExplore tbDumpReadyView ~
tbDumpReadyClipboard 
&Scoped-Define DISPLAYED-OBJECTS ficFileName cbDumpType cbCodePage ~
tbUseCustomizedFormats cbSeparator cbNumericFormat tbExportSchema ~
cbiRecordSelection cbDateFormat tbMinimalSchema cbiFieldSelection ~
ficMessageNow ficMessage tbDumpReadyClose tbDumpReadyExplore ~
tbDumpReadyView tbDumpReadyClipboard 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getEscapedData wDump 
FUNCTION getEscapedData RETURNS CHARACTER
  ( pcTarget AS CHARACTER
  , pcString AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getExcelCol wDump 
FUNCTION getExcelCol RETURNS CHARACTER
  ( INPUT iColumnNr AS INTEGER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getFieldListFromIndexInformation wDump 
FUNCTION getFieldListFromIndexInformation RETURNS CHARACTER
  ( INPUT picIndexInformation AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getFieldValue wDump 
FUNCTION getFieldValue RETURNS CHARACTER
  ( phField AS HANDLE
  , piExtent AS INTEGER 
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD multipleLookUpGreaterThanZero wDump 
FUNCTION multipleLookUpGreaterThanZero RETURNS LOGICAL
  ( INPUT picExpressions AS CHARACTER
  , INPUT picList        AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wDump AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnChooseDumpFile 
     LABEL "..." 
     SIZE-PIXELS 20 BY 21.

DEFINE BUTTON btnClose AUTO-END-KEY 
     LABEL "&Close" 
     SIZE-PIXELS 74 BY 24.

DEFINE BUTTON btnDump 
     LABEL "&Dump" 
     SIZE-PIXELS 74 BY 24 TOOLTIP "start the dump".

DEFINE BUTTON btnOpenLastDumpDir 
     LABEL "&Open" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "open directory of last dump".

DEFINE BUTTON btnViewLastDump 
     LABEL "&View" 
     SIZE-PIXELS 20 BY 23 TOOLTIP "view the last exported file".

DEFINE VARIABLE cbCodePage AS CHARACTER INITIAL "0" 
     LABEL "Code &page" 
     VIEW-AS COMBO-BOX INNER-LINES 5
     SIMPLE
     SIZE-PIXELS 150 BY 23 TOOLTIP "the code page used for dumping" NO-UNDO.

DEFINE VARIABLE cbDateFormat AS CHARACTER FORMAT "X(256)":U 
     LABEL "D&ate Format" 
     VIEW-AS COMBO-BOX INNER-LINES 10
     DROP-DOWN-LIST
     SIZE-PIXELS 150 BY 21 TOOLTIP "the date format used for the dump" NO-UNDO.

DEFINE VARIABLE cbDumpType AS CHARACTER FORMAT "X(32)":U 
     LABEL "&Export as" 
     VIEW-AS COMBO-BOX INNER-LINES 8
     LIST-ITEM-PAIRS "Comma Separated","CSV",
                     "Excel","XLS",
                     "HTML","HTML",
                     "Progress dumpfile (*.d)","D",
                     "Text file","TXT",
                     "XML","XML",
                     "4GL code","P"
     DROP-DOWN-LIST
     SIZE-PIXELS 150 BY 21 TOOLTIP "type of format of the file" NO-UNDO.

DEFINE VARIABLE cbiFieldSelection AS INTEGER FORMAT "9":U INITIAL 0 
     LABEL "&Fields" 
     VIEW-AS COMBO-BOX INNER-LINES 3
     LIST-ITEM-PAIRS "All",1,
                     "Selected",2
     DROP-DOWN-LIST
     SIZE-PIXELS 150 BY 21 TOOLTIP "the fieldset that will be exported" NO-UNDO.

DEFINE VARIABLE cbiRecordSelection AS INTEGER FORMAT "9":U INITIAL 0 
     LABEL "&Records" 
     VIEW-AS COMBO-BOX INNER-LINES 4
     LIST-ITEM-PAIRS "Table",1,
                     "Browse",2,
                     "Selection",3
     DROP-DOWN-LIST
     SIZE-PIXELS 150 BY 21 TOOLTIP "the recordset that will be exported" NO-UNDO.

DEFINE VARIABLE cbNumericFormat AS CHARACTER FORMAT "X(256)":U INITIAL "0" 
     LABEL "&Numeric Format" 
     VIEW-AS COMBO-BOX INNER-LINES 6
     DROP-DOWN-LIST
     SIZE-PIXELS 150 BY 21 TOOLTIP "the numeric format used for dumping" NO-UNDO.

DEFINE VARIABLE cbSeparator AS INTEGER FORMAT "9":U INITIAL 1 
     LABEL "&Separator" 
     VIEW-AS COMBO-BOX INNER-LINES 4
     LIST-ITEM-PAIRS "Comma",1,
                     "Pipe",2,
                     "Semicolon",3
     DROP-DOWN-LIST
     SIZE-PIXELS 150 BY 21 TOOLTIP "character used to separate fields in a record" NO-UNDO.

DEFINE VARIABLE ficFileName AS CHARACTER FORMAT "X(256)":U 
     LABEL "&Dumpfile" 
     VIEW-AS FILL-IN NATIVE 
     SIZE-PIXELS 570 BY 21 TOOLTIP "the name and path of the resulting dumpfile" NO-UNDO.

DEFINE VARIABLE ficMessage AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 735 BY 22 NO-UNDO.

DEFINE VARIABLE ficMessageNow AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 130 BY 21 NO-UNDO.

DEFINE RECTANGLE RECT-2
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 795 BY 145.

DEFINE RECTANGLE RECT-3
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 800 BY 65.

DEFINE RECTANGLE RECT-4
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 580 BY 40.

DEFINE VARIABLE tbDumpReadyClipboard AS LOGICAL INITIAL no 
     LABEL "Data to Clip&board" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 137 BY 17 NO-UNDO.

DEFINE VARIABLE tbDumpReadyClose AS LOGICAL INITIAL no 
     LABEL "C&lose window" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 115 BY 17 NO-UNDO.

DEFINE VARIABLE tbDumpReadyExplore AS LOGICAL INITIAL no 
     LABEL "E&xplore dump dir" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 121 BY 17 NO-UNDO.

DEFINE VARIABLE tbDumpReadyView AS LOGICAL INITIAL no 
     LABEL "&View dump file" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 113 BY 17 NO-UNDO.

DEFINE VARIABLE tbExportSchema AS LOGICAL INITIAL no 
     LABEL "Export &XML schema" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 205 BY 17 TOOLTIP "also export the schema to the XML file" NO-UNDO.

DEFINE VARIABLE tbMinimalSchema AS LOGICAL INITIAL no 
     LABEL "&Minimal Schema" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 205 BY 17 TOOLTIP "use a minimal schema export" NO-UNDO.

DEFINE VARIABLE tbUseCustomizedFormats AS LOGICAL INITIAL no 
     LABEL "&Use Customized Field Formats" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 205 BY 17 TOOLTIP "export with the customized field formats instead of dictionary formats" NO-UNDO.

DEFINE BUTTON btAbort 
     LABEL "Abort" 
     SIZE-PIXELS 75 BY 24 TOOLTIP "abort the dumping process".

DEFINE VARIABLE fcInfoLine AS CHARACTER FORMAT "X(256)":U 
      VIEW-AS TEXT 
     SIZE-PIXELS 165 BY 13 NO-UNDO.

DEFINE RECTANGLE rcBody
     EDGE-PIXELS 2 GRAPHIC-EDGE    
     SIZE-PIXELS 50 BY 20
     BGCOLOR 9 .

DEFINE RECTANGLE rcBorder
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 165 BY 20.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME DEFAULT-FRAME
     ficFileName AT Y 15 X 97 COLON-ALIGNED WIDGET-ID 2
     btnChooseDumpFile AT Y 15 X 682 WIDGET-ID 4
     cbDumpType AT Y 45 X 97 COLON-ALIGNED WIDGET-ID 6
     cbCodePage AT Y 45 X 384 COLON-ALIGNED WIDGET-ID 54
     tbUseCustomizedFormats AT Y 66 X 576 WIDGET-ID 52
     cbSeparator AT Y 70 X 97 COLON-ALIGNED WIDGET-ID 64
     cbNumericFormat AT Y 70 X 384 COLON-ALIGNED WIDGET-ID 48
     tbExportSchema AT Y 85 X 576 WIDGET-ID 20
     cbiRecordSelection AT Y 95 X 97 COLON-ALIGNED WIDGET-ID 8
     cbDateFormat AT Y 95 X 384 COLON-ALIGNED WIDGET-ID 50
     tbMinimalSchema AT Y 104 X 576 WIDGET-ID 22
     cbiFieldSelection AT Y 120 X 97 COLON-ALIGNED WIDGET-ID 30
     ficMessageNow AT Y 184 X 10 NO-LABEL WIDGET-ID 18
     btnViewLastDump AT Y 200 X 750 WIDGET-ID 28
     btnOpenLastDumpDir AT Y 200 X 773 WIDGET-ID 32
     ficMessage AT Y 203 X 10 NO-LABEL WIDGET-ID 16
     btnDump AT Y 265 X 649 WIDGET-ID 62
     btnClose AT Y 265 X 729 WIDGET-ID 60
     tbDumpReadyClose AT Y 270 X 15 WIDGET-ID 36
     tbDumpReadyExplore AT Y 270 X 145 WIDGET-ID 38
     tbDumpReadyView AT Y 270 X 282 WIDGET-ID 40
     tbDumpReadyClipboard AT Y 270 X 406 WIDGET-ID 56
     "Last dump" VIEW-AS TEXT
          SIZE-PIXELS 87 BY 13 AT Y 163 X 13 WIDGET-ID 26
     "After the dump ..." VIEW-AS TEXT
          SIZE-PIXELS 103 BY 13 AT Y 248 X 12 WIDGET-ID 44
     RECT-2 AT Y 5 X 10 WIDGET-ID 14
     RECT-3 AT Y 169 X 5 WIDGET-ID 24
     RECT-4 AT Y 254 X 5 WIDGET-ID 34
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 0 Y 0
         SIZE-PIXELS 810 BY 301
         DEFAULT-BUTTON btnDump WIDGET-ID 100.

DEFINE FRAME infoFrame
     btAbort AT Y 58 X 60 WIDGET-ID 54
     fcInfoLine AT Y 10 X 0 COLON-ALIGNED NO-LABEL WIDGET-ID 46
     rcBorder AT Y 30 X 10 WIDGET-ID 2
     rcBody AT Y 29 X 10 WIDGET-ID 6
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 280 Y 84
         SIZE-PIXELS 188 BY 103 WIDGET-ID 200.


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
  CREATE WINDOW wDump ASSIGN
         HIDDEN             = YES
         TITLE              = "Save as"
         HEIGHT-P           = 299
         WIDTH-P            = 810
         MAX-HEIGHT-P       = 608
         MAX-WIDTH-P        = 958
         VIRTUAL-HEIGHT-P   = 608
         VIRTUAL-WIDTH-P    = 958
         MIN-BUTTON         = no
         MAX-BUTTON         = no
         RESIZE             = no
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
/* SETTINGS FOR WINDOW wDump
  VISIBLE,,RUN-PERSISTENT                                               */
/* REPARENT FRAME */
ASSIGN FRAME infoFrame:FRAME = FRAME DEFAULT-FRAME:HANDLE.

/* SETTINGS FOR FRAME DEFAULT-FRAME
   FRAME-NAME                                                           */
/* SETTINGS FOR BUTTON btnOpenLastDumpDir IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
/* SETTINGS FOR BUTTON btnViewLastDump IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN ficMessage IN FRAME DEFAULT-FRAME
   NO-ENABLE ALIGN-L                                                    */
/* SETTINGS FOR FILL-IN ficMessageNow IN FRAME DEFAULT-FRAME
   NO-ENABLE ALIGN-L                                                    */
/* SETTINGS FOR TOGGLE-BOX tbExportSchema IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tbMinimalSchema IN FRAME DEFAULT-FRAME
   NO-ENABLE                                                            */
/* SETTINGS FOR FRAME infoFrame
                                                                        */
IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wDump)
THEN wDump:HIDDEN = no.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME wDump
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wDump wDump
ON END-ERROR OF wDump /* Save as */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wDump wDump
ON LEAVE OF wDump /* Save as */
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wDump wDump
ON WINDOW-CLOSE OF wDump /* Save as */
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME infoFrame
&Scoped-define SELF-NAME btAbort
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btAbort wDump
ON CHOOSE OF btAbort IN FRAME infoFrame /* Abort */
DO:
  glAborted = TRUE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME DEFAULT-FRAME
&Scoped-define SELF-NAME btnChooseDumpFile
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnChooseDumpFile wDump
ON CHOOSE OF btnChooseDumpFile IN FRAME DEFAULT-FRAME /* ... */
DO:

  DEFINE VARIABLE     lOkay           AS LOGICAL    NO-UNDO.
  DEFINE VARIABLE     cFileName       AS CHARACTER  NO-UNDO.

  cFileName = ficFileName:screen-value.

  SYSTEM-DIALOG GET-FILE cFilename
    FILTERS "XML Dumpfile (*.xml)" "*.xml",
            ".d Dumpfile (*.d)" "*.d",
            "Any File (*.*)" "*.*"
    INITIAL-FILTER 1
    ASK-OVERWRITE
    USE-FILENAME
    CREATE-TEST-FILE
    DEFAULT-EXTENSION ".xml"
    SAVE-AS
    UPDATE lOkay.
  
  IF NOT lOkay THEN 
    RETURN.

  DO WITH FRAME {&frame-name}:
    ficFileName = LC(cFileName).
    DISPLAY ficFileName.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDump
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDump wDump
ON CHOOSE OF btnDump IN FRAME DEFAULT-FRAME /* Dump */
OR GO OF FRAME {&frame-name} 
DO:
  DEFINE VARIABLE lContinue AS LOGICAL NO-UNDO.
  RUN btnDumpChoose(OUTPUT lContinue).
  IF NOT lContinue THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnOpenLastDumpDir
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOpenLastDumpDir wDump
ON CHOOSE OF btnOpenLastDumpDir IN FRAME DEFAULT-FRAME /* Open */
DO:
  RUN btnOpenLastDumpDirChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnViewLastDump
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnViewLastDump wDump
ON CHOOSE OF btnViewLastDump IN FRAME DEFAULT-FRAME /* View */
DO:
  RUN btnViewLastDumpChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbCodePage
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbCodePage wDump
ON VALUE-CHANGED OF cbCodePage IN FRAME DEFAULT-FRAME /* Code page */
DO:
  RUN cbCodePageValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbDateFormat
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbDateFormat wDump
ON VALUE-CHANGED OF cbDateFormat IN FRAME DEFAULT-FRAME /* Date Format */
DO:
  RUN cbDateFormatValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbDumpType
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbDumpType wDump
ON VALUE-CHANGED OF cbDumpType IN FRAME DEFAULT-FRAME /* Export as */
DO:
  RUN cbDumpTypeValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbiFieldSelection
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbiFieldSelection wDump
ON VALUE-CHANGED OF cbiFieldSelection IN FRAME DEFAULT-FRAME /* Fields */
DO:  
  RUN cbiFieldSelectionValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbiRecordSelection
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbiRecordSelection wDump
ON VALUE-CHANGED OF cbiRecordSelection IN FRAME DEFAULT-FRAME /* Records */
DO:  
  RUN cbiRecordSelectionValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbNumericFormat
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbNumericFormat wDump
ON VALUE-CHANGED OF cbNumericFormat IN FRAME DEFAULT-FRAME /* Numeric Format */
DO:
  RUN cbNumericFormatValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbSeparator
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbSeparator wDump
ON VALUE-CHANGED OF cbSeparator IN FRAME DEFAULT-FRAME /* Separator */
DO:
  DO WITH FRAME {&FRAME-NAME}:
    ASSIGN cbSeparator.
    setRegistry("DumpAndLoad", "FieldSeparator", STRING(cbSeparator) ).
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME ficFileName
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficFileName wDump
ON LEAVE OF ficFileName IN FRAME DEFAULT-FRAME /* Dumpfile */
DO:
  ASSIGN ficFileName.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tbDumpReadyClose
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tbDumpReadyClose wDump
ON VALUE-CHANGED OF tbDumpReadyClose IN FRAME DEFAULT-FRAME /* Close window */
, tbDumpReadyExplore, tbDumpReadyView, tbDumpReadyClipboard
DO:
  DEFINE VARIABLE cDumpReadyAction AS CHARACTER   NO-UNDO.

  IF tbDumpReadyClose    :CHECKED THEN cDumpReadyAction = cDumpReadyAction + ',Close'.
  IF tbDumpReadyExplore  :CHECKED THEN cDumpReadyAction = cDumpReadyAction + ',Explore'.
  IF tbDumpReadyView     :CHECKED THEN cDumpReadyAction = cDumpReadyAction + ',View'.
  IF tbDumpReadyClipboard:CHECKED THEN cDumpReadyAction = cDumpReadyAction + ',Clipboard'.

  cDumpReadyAction = REPLACE(cDumpReadyAction,",,",","). 
  cDumpReadyAction = TRIM(cDumpReadyAction,",").
  IF cDumpReadyAction = "" THEN cDumpReadyAction = "Nothing".

  setRegistry('DumpAndLoad','DumpReadyAction',cDumpReadyAction).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tbExportSchema
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tbExportSchema wDump
ON VALUE-CHANGED OF tbExportSchema IN FRAME DEFAULT-FRAME /* Export XML schema */
DO:
  RUN tbExportSchemaValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tbMinimalSchema
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tbMinimalSchema wDump
ON VALUE-CHANGED OF tbMinimalSchema IN FRAME DEFAULT-FRAME /* Minimal Schema */
DO:
  RUN tbMinimalSchemaValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tbUseCustomizedFormats
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tbUseCustomizedFormats wDump
ON VALUE-CHANGED OF tbUseCustomizedFormats IN FRAME DEFAULT-FRAME /* Use Customized Field Formats */
DO:
  RUN tbUseCustomizedFormatsValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK wDump 


/* ***************************  Main Block  *************************** */

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
ASSIGN CURRENT-WINDOW                = {&WINDOW-NAME} 
       THIS-PROCEDURE:CURRENT-WINDOW = {&WINDOW-NAME}.

/* The CLOSE event can be used from inside or outside the procedure to  */
/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
   RUN disable_UI.

/* Best default for GUI applications is...                              */
PAUSE 0 BEFORE-HIDE.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  RUN initializeObject.

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
END.

/* Restore settings */
SESSION:DATE-FORMAT = gcSessionDateFormat.
SESSION:NUMERIC-FORMAT = gcSessionNumericFormat.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnDumpChoose wDump 
PROCEDURE btnDumpChoose :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE OUTPUT PARAMETER plContinue AS LOGICAL NO-UNDO.

  DEFINE VARIABLE cAction  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iAction  AS INTEGER     NO-UNDO. 
  DEFINE VARIABLE cError   AS CHARACTER   NO-UNDO.

  DO WITH FRAME {&frame-name}:

    IF NOT isValidCodePage(cbCodePage:SCREEN-VALUE) THEN
    DO:
      MESSAGE cbCodePage:SCREEN-VALUE 'is not a valid code page' VIEW-AS ALERT-BOX INFO BUTTONS OK.
      RETURN. 
    END.
    
    RUN checkDir(INPUT ficFileName:SCREEN-VALUE, OUTPUT cError).
    IF cError <> '' THEN 
    DO:
      MESSAGE cError VIEW-AS ALERT-BOX INFO BUTTONS OK.
      RETURN. 
    END.

    RUN dumpData
      ( INPUT pihBrowse                       /*   input  pihDdBrowse         as handle    */ 
      , INPUT cbDumpType:SCREEN-VALUE         /*   input  picFormat           as character */ 
      , INPUT cbiRecordSelection:SCREEN-VALUE /*   input  piiRecordSelection  as integer   */ 
      , INPUT cbiFieldSelection:SCREEN-VALUE  /*   input  piiRecordSelection  as integer   */ 
      , INPUT ficFileName:SCREEN-VALUE        /*   input  picFile             as character */ 
      ).

    cAction = getRegistry('DumpAndLoad','DumpReadyAction').
    DO iAction = 1 TO NUM-ENTRIES(cAction):
      CASE ENTRY(iAction,cAction):
        WHEN 'view'      THEN APPLY 'choose' TO btnViewLastDump.
        WHEN 'explore'   THEN APPLY 'choose' TO btnOpenLastDumpDir.
        WHEN 'clipboard' THEN RUN tbDumpReadyClipboard.
      END CASE.
    END.

    /* Do the close as last action */
    plContinue = TRUE. 
    IF CAN-DO(cAction,"Close") THEN APPLY "CLOSE" TO THIS-PROCEDURE.
  END.

END PROCEDURE. /* btnDumpChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnOpenLastDumpDirChoose wDump 
PROCEDURE btnOpenLastDumpDirChoose :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE VARIABLE cDumpDir AS CHARACTER   NO-UNDO.

  DO WITH FRAME {&frame-name}:

    /* Strip filename, only keep path */
    cDumpDir = getRegistry("DumpAndLoad", "DumpLastFileName").
    IF cDumpDir = ? THEN RETURN. 

    ENTRY(NUM-ENTRIES(cDumpDir,'\'),cDumpDir,'\') = ''.
    IF cDumpDir = "" THEN cDumpDir = ".". /* if blank go to DD dir */
    FILE-INFO:FILE-NAME = cDumpDir.

    IF FILE-INFO:FULL-PATHNAME <> ? THEN
      OS-COMMAND NO-WAIT explorer /n, /e, VALUE(FILE-INFO:FULL-PATHNAME).
    ELSE
      MESSAGE SUBSTITUTE("Last used dir '&1' not found.", cDumpDir)
        VIEW-AS ALERT-BOX INFO BUTTONS OK TITLE "Invalid Dir" .
  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnViewLastDumpChoose wDump 
PROCEDURE btnViewLastDumpChoose :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE VARIABLE     cCmd           AS CHARACTER  NO-UNDO.
  DEFINE VARIABLE     cDumpType      AS CHARACTER  NO-UNDO.

  cDumpType = getRegistry("DumpAndLoad", "DumpExportType" ).
  cCmd = getRegistry("DumpAndLoad", SUBSTITUTE("DumpFileViewCmd_&1",cDumpType) ).

  ASSIGN
    cCmd = SUBSTITUTE(cCmd, gcLastFile)
    cCmd = SUBSTITUTE('"&1"',cCmd)
    .

  PUBLISH "message" ( 50, SUBSTITUTE("DD FileViewCommand: &1",cCmd) ).

  OS-COMMAND NO-WAIT VALUE(cCmd).

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnViewLastDumpEnable wDump 
PROCEDURE btnViewLastDumpEnable :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  DO WITH FRAME {&frame-name}:

    gcLastFile = getRegistry("DumpAndLoad", "DumpLastFileName").
    
    IF gcLastFile <> ? THEN
    DO:
      FILE-INFO:FILENAME = gcLastFile.
      gcLastFile = FILE-INFO:FULL-PATHNAME.
    END.

    IF gcLastFile <> ? THEN
      ENABLE btnViewLastDump btnOpenLastDumpDir.
    ELSE
      DISABLE btnViewLastDump btnOpenLastDumpDir.

  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbCodePageValueChanged wDump 
PROCEDURE cbCodePageValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  DO WITH FRAME {&frame-name}:
    
    ASSIGN cbCodePage.
    setRegistry("DumpAndLoad", "DumpCodePage", cbCodePage ).

  END.
  
END PROCEDURE. /* cbCodePageValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbDateFormatValueChanged wDump 
PROCEDURE cbDateFormatValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  DO WITH FRAME {&frame-name}:
    
    ASSIGN cbDateFormat.
    setRegistry("DumpAndLoad", "DumpDateFormat", cbDateFormat ).

  END.
  
END PROCEDURE. /* cbDateFormatValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbDumpTypeValueChanged wDump 
PROCEDURE cbDumpTypeValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DO WITH FRAME {&frame-name}:
    
    ASSIGN cbDumpType.

    /* Save dump type */
    gcFileViewCmd = getRegistry("DumpAndLoad", SUBSTITUTE("DumpFileViewCmd_&1",cbDumpType) ).
    IF gcFileViewCmd = ? THEN
    DO:
      gcFileViewCmd = getRegistry("DumpAndLoad", "DumpFileViewCmd").
      setRegistry("DumpAndLoad", SUBSTITUTE("DumpFileViewCmd_&1",cbDumpType), gcFileViewCmd).
    END. /* if gcFileViewCmd = ? then */

    CASE cbDumpType:
      WHEN "XML" THEN
      DO:
        ENABLE tbExportSchema.
        DISABLE cbNumericFormat cbDateFormat.
        cbNumericFormat = 'American'.
        cbDateFormat = 'YMD'.

        IF tbUseCustomizedFormats:SENSITIVE THEN
          tbUseCustomizedFormats:CHECKED = TRUE.
      END.

      WHEN "P" THEN
      DO:
        DISABLE cbNumericFormat cbDateFormat tbExportSchema.
        ASSIGN
          cbNumericFormat = 'American'
          cbDateFormat = 'MDY'.
      END.

      OTHERWISE
      DO:
        DISABLE tbExportSchema.
        ENABLE cbNumericFormat cbDateFormat.
      END.
    END CASE.

    /* For progress dump file and XML, use dictionary format by default
     * For others, use customized
     */
    IF   cbDumpType = "XML" 
      OR cbDumpType = "D" THEN
    DO:
      /* But only if there has been changed something by the 
       * user, otherwise this is not relevant 
       */
      tbUseCustomizedFormats:SENSITIVE = FALSE. 
      tbUseCustomizedFormats:CHECKED   = FALSE.
    END.
    ELSE 
    DO:
      /* Find out whether the user has some customized fields */
      IF CAN-FIND(FIRST ttField WHERE ttField.cFormat <> ttField.cFormatOrg) THEN
        ASSIGN tbUseCustomizedFormats:SENSITIVE = TRUE
               tbUseCustomizedFormats:CHECKED   = TRUE. 
      ELSE 
        ASSIGN tbUseCustomizedFormats:SENSITIVE = FALSE
               tbUseCustomizedFormats:CHECKED   = FALSE. 
    END.

    /* Separator only for csv */
    cbSeparator:SENSITIVE = (cbDumpType = "CSV").

    /* replace extension of filename */
    IF ficFileName = '' THEN
      RUN setDumpFileName.

    IF NUM-ENTRIES(ficFileName,'.') > 1 THEN
      ENTRY(NUM-ENTRIES(ficFileName,'.'),ficFileName,'.') = LOWER(cbDumpType).
    ELSE 
      ficFileName = ficFileName + '.' + lower(cbDumpType).

    DISPLAY ficFileName cbNumericFormat cbDateFormat.

  END.
  
  RUN tbExportSchemaValueChanged.
  RUN tbUseCustomizedFormatsValueChanged.


END PROCEDURE. /* cbDumpTypeValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbiFieldSelectionValueChanged wDump 
PROCEDURE cbiFieldSelectionValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  DO WITH FRAME {&frame-name}:    
    ASSIGN cbiFieldSelection.
  END.
  
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbiRecordSelectionValueChanged wDump 
PROCEDURE cbiRecordSelectionValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       cbiRecordSelectionValueChanged
------------------------------------------------------------------------------*/
  
  DO WITH FRAME {&frame-name}:
    
    ASSIGN cbiRecordSelection.

    IF (   ( cbiRecordSelection = 2 AND pihBrowse:QUERY:num-results = 0) 
        OR ( cbiRecordSelection = 3 AND pihBrowse:NUM-SELECTED-ROWS = 0) )
      AND NOT glNoRecordsWarning THEN
    DO:
      RUN showHelp('NoSelection','').
      glNoRecordsWarning = TRUE.
    END.

  END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbNumericFormatValueChanged wDump 
PROCEDURE cbNumericFormatValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  DO WITH FRAME {&frame-name}:
    
    ASSIGN cbNumericFormat.
    setRegistry("DumpAndLoad", "DumpNumericFormat", cbNumericFormat ).

  END.
  
END PROCEDURE. /* cbNumericFormatValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI wDump  _DEFAULT-DISABLE
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
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wDump)
  THEN DELETE WIDGET wDump.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dumpData wDump 
PROCEDURE dumpData :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER    pihDdBrowse         AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER    picFormat           AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER    piiRecordSelection  AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER    piiFieldSelection   AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER    picFile             AS CHARACTER   NO-UNDO.
  
  DEFINE VARIABLE cDumpDir            AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iNumRecs            AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iCurField           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cTtField            AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDbField            AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE hExportTT           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hExportTtBuffer     AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hExportQuery        AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hExportQueryBuffer  AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cExportQueryString  AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE iCurSelectedRow     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cStatus             AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iCurIndex           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cIndexInfo          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cIndexName          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cIndexFields        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iTimeStarted        AS INTEGER     NO-UNDO.

  glAborted = FALSE. 

  RUN setStatusMessage
    ( INPUT NOW
    , INPUT SUBSTITUTE
              ( "Dumping records from table &1.&2 in progres..."
              , gcDb
              , gcTable
              )
    ).

  /* Construct the Query-string... */
  CASE piiRecordSelection:
    WHEN 1 THEN cExportQueryString = SUBSTITUTE("for each &1.&2 no-lock", gcDb, gcTable ).
    WHEN 2 THEN cExportQueryString = pihDdBrowse:QUERY:prepare-string.
  END CASE. /* case piiRecordSelection: */
  
  /* Create temptable-handle... */
  CREATE TEMP-TABLE hExportTt.
  
  /* Add fields & indexes to TempTable... */
  CASE piiFieldSelection:
    
    /* Add all fields & indexes from the db-table... */
    WHEN 1 THEN 
    DO:
      hExportTt:CREATE-LIKE(SUBSTITUTE("&1.&2",gcDb,gcTable)).
    END. /* when 1 then  */


    /* Add selected fields & some indexes from the db-table... */
    WHEN 2 THEN
    DO:
      /* add selected fields to the temp-table layout */
      DO iCurField = 1 TO NUM-ENTRIES(picSelectedFields):

        ASSIGN
          cTtField = ENTRY(iCurField,picSelectedFields)
          cDbField = SUBSTITUTE("&1.&2.&3", gcDB, gcTable, cTtField)
          .

        /* skip ROWID and RECID fields as they don't exist in the table */
        IF LOOKUP(cTtField,"ROWID,RECID") > 0 THEN NEXT.
        
        hExportTt:ADD-LIKE-FIELD(cTtField,cDbField).

      END. /* do iCurField = 1 to num-entries(picSelectedFields): */
      
      /* add all indexes to the temp-table layout which consists of selected fields  */
      CREATE BUFFER hExportQueryBuffer FOR TABLE SUBSTITUTE("&1.&2",gcDb,gcTable).

      iCurIndex = 0.
      DO WHILE TRUE:

        ASSIGN
          iCurIndex  = iCurIndex + 1
          cIndexInfo = hExportQueryBuffer:INDEX-INFORMATION(iCurIndex)
          .

        IF cIndexInfo = ? THEN
          LEAVE.

        ASSIGN
          cIndexName   = ENTRY(1,cIndexInfo)
          cIndexFields = getFieldListFromIndexInformation(INPUT cIndexInfo) 
          .

        IF multipleLookUpGreaterThanZero(cIndexFields,picSelectedFields) THEN
          hExportTt:ADD-LIKE-INDEX(cIndexName,cIndexName,SUBSTITUTE("&1.&2", gcDB, gcTable)).

      END. /* do while true: */

      DELETE OBJECT hExportQueryBuffer.
    END. /* when 2 then */
  END. /* case piiFieldSelection: */

  /* Prepare the TempTable... */
  hExportTt:TEMP-TABLE-PREPARE(SUBSTITUTE("&1",gcTable)).
  hExportTtBuffer = hExportTt:DEFAULT-BUFFER-HANDLE.

  /* Populate the TempTable... */
  CASE piiRecordSelection:

    /* All records from table(1) or browse(2) */
    WHEN 1 OR WHEN 2 THEN
    DO:
      CREATE BUFFER hExportQueryBuffer FOR TABLE SUBSTITUTE("&1.&2",gcDb,gcTable).
      CREATE QUERY hExportQuery.
      hExportQuery:SET-BUFFERS(hExportQueryBuffer).
      hExportQuery:QUERY-PREPARE(cExportQueryString).

      hExportQuery:QUERY-OPEN(). 
      REPEAT:    
        hExportQuery:GET-NEXT().    
        IF hExportQuery:QUERY-OFF-END THEN 
          LEAVE.

        ASSIGN iNumRecs = iNumRecs + 1.
        IF (ETIME - iTimeStarted) > 1000 THEN
        DO:
          iTimeStarted = ETIME.
          RUN showProgressBar(SUBSTITUTE('Collected &1 records',iNumRecs), 0).
          PROCESS EVENTS.
          IF glAborted THEN LEAVE. 
        END.

        hExportTtBuffer:BUFFER-CREATE.
        hExportTtBuffer:BUFFER-COPY(hExportQuery:GET-BUFFER-HANDLE(1)).
      END.
      hExportQuery:QUERY-CLOSE().

    END. /* when 1 or when 2 then */
    
    /* Records from the selection */
    WHEN 3 THEN
    DO:
      DO iCurSelectedRow = 1 TO pihDdBrowse:NUM-SELECTED-ROWS:
        pihDdBrowse:FETCH-SELECTED-ROW(iCurSelectedRow).
        hExportTtBuffer:BUFFER-CREATE.
        hExportTtBuffer:BUFFER-COPY(pihDdBrowse:QUERY:get-buffer-handle()).

        ASSIGN iNumRecs = iNumRecs + 1.
        IF (ETIME - iTimeStarted) > 1000 THEN
        DO:
          iTimeStarted = ETIME.
          RUN showProgressBar(SUBSTITUTE('Collected &1 records',iNumRecs), 0).
          PROCESS EVENTS.
          IF glAborted THEN LEAVE. 
        END.

      END. /* when 3 then */
    END.
  END. /* case piiRecordSelection: */

  /* Dump the TempTable... */
  SESSION:NUMERIC-FORMAT = cbNumericFormat.
  SESSION:DATE-FORMAT = cbDateFormat.

  CASE picFormat:

    WHEN "D" THEN 
      RUN DumpDataProgressD
            ( INPUT  picFile
            , INPUT  hExportTt
            , INPUT  iNumRecs
            , INPUT  cbCodePage
            ).

    WHEN "HTML" THEN 
      RUN DumpDataHtml
            ( INPUT  picFile
            , INPUT  hExportTt
            , INPUT  iNumRecs
            , INPUT  cbCodePage
            ).

    WHEN "TXT" THEN 
      RUN DumpDataTxt
            ( INPUT  picFile
            , INPUT  hExportTt
            , INPUT  iNumRecs
            , INPUT  cbCodePage
            ).

    WHEN "XLS" THEN 
      RUN DumpDataExcel
            ( INPUT  picFile
            , INPUT  hExportTt
            , INPUT  iNumRecs
            , INPUT  cbCodePage
            ).

    WHEN "XML" THEN 
      RUN dumpDataXml
            ( INPUT  picFile
            , INPUT  hExportTt
            , INPUT  iNumRecs
            , INPUT  cbCodePage
            ).

    WHEN "P" THEN 
      RUN dumpData4GL
            ( INPUT  picFile
            , INPUT  hExportTt
            , INPUT  iNumRecs
            , INPUT  cbCodePage
            ).

    WHEN "CSV" THEN 
      RUN dumpDataCSV
            ( INPUT  picFile
            , INPUT  hExportTt
            , INPUT  iNumRecs
            , INPUT  cbCodePage
            ).

  END CASE. /* case picFormat: */
  
  SESSION:NUMERIC-FORMAT = gcSessionNumericFormat.
  SESSION:DATE-FORMAT = gcSessionDateFormat.

  /* Clean up */
  IF VALID-HANDLE(hExportQueryBuffer) THEN DELETE OBJECT hExportQueryBuffer.
  IF VALID-HANDLE(hExportQuery      ) THEN DELETE OBJECT hExportQuery.
  IF VALID-HANDLE(hExportTt         ) THEN DELETE OBJECT hExportTt.

  DO WITH FRAME {&frame-name}:
    
    IF glAborted THEN 
    DO:
      cStatus = SUBSTITUTE( "Dumping table &1 aborted", gcTable ).
      RUN showHelp('DumpAborted', gcTable).
    END.
    ELSE 
      cStatus = SUBSTITUTE( "&1 records dumped from table &2.&3 to &4"
                          , iNumRecs
                          , gcDb
                          , gcTable
                          , picFile
                          ).

    RUN setStatusMessage( INPUT NOW, INPUT cStatus ).

    ASSIGN
      cDumpDir = SUBSTRING(picFile, 1, R-INDEX(picFile,"~\"))
      .

    setRegistry( "DumpAndLoad", "DumpExportType"      , cbDumpType).
    setRegistry( "DumpAndLoad", "DumpFilter"          , STRING(cbiRecordSelection) ).
    setRegistry( "DumpAndLoad", "DumpFilterFields"    , STRING(cbiFieldSelection) ).
    setRegistry( "DumpAndLoad", "DumpXmlSchema"       , STRING(tbExportSchema) ).
    setRegistry( "DumpAndLoad", "DumpMinimalXmlSchema", STRING(tbMinimalSchema) ).
    setRegistry( "DumpAndLoad", "DumpLastFileName"    , picFile ).
    setRegistry( "DumpAndLoad", "DumpActionTimeStamp" , ficMessageNow ).
    setRegistry( "DumpAndLoad", "DumpActionResult"    , ficMessage ).

    gcLastFile = picFile.
  END. /* do with frame {&frame-name}: */

  RUN btnViewLastDumpEnable.

END PROCEDURE. /* dumpData */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpData4GL wDump 
PROCEDURE DumpData4GL :
/*------------------------------------------------------------------------------
  Purpose:     Dump Data as 4GL code
  2012-09-14 JEE Created
------------------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER picFileName  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pihTempTable AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER piNumRecords AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER pcCodePage   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE hField              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hTTBuffer           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iCurField           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNumRecords         AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMaxLength          AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNumFields          AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iTimeStarted        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iLength             AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iExtent             AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cCodePage           AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cKeyFields          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLine               AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iExtBegin           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iExtEnd             AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cFieldName          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldNameFormat    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldValue         AS CHARACTER   NO-UNDO.
  
  ASSIGN
    cCodePage    = (IF pcCodePage <> "" THEN pcCodePage ELSE SESSION:CPSTREAM)
    iTimeStarted = ETIME
    hTTBuffer    = pihTempTable:DEFAULT-BUFFER-HANDLE
    iNumRecords = 0
    cKeyFields   = getIndexFields(gcDB, gcTable, "P,U")
    .

  /* Open outputfile */
  OUTPUT stream strDump to value(picFileName) convert target cCodePage.
  PUT STREAM strDump UNFORMATTED 
         SUBSTITUTE("/* Generated procedure for &1.&2 ", gcDB, gcTable)
    SKIP SUBSTITUTE(" * Date: &1 ", STRING(NOW,"99-99-9999 HH:MM"))
    SKIP SUBSTITUTE(" * By  : &1 ", getUsername() )
    SKIP SUBSTITUTE(" */" ) 
    SKIP SUBSTITUTE("DEFINE BUFFER bData FOR &1.&2.", gcDB, gcTable)
    SKIP SUBSTITUTE(" ")
    .

  /* Build query */
  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hTTBuffer).
  hQuery:QUERY-PREPARE(SUBSTITUTE("FOR EACH &1 NO-LOCK", hTTBuffer:NAME)).
  hQuery:QUERY-OPEN().


  /* Pump the table data into the table */
  pumpDataLoop:
  REPEAT:
    hQuery:GET-NEXT().
    IF hQuery:QUERY-OFF-END THEN LEAVE pumpDataLoop.

    IF (ETIME - iTimeStarted) > 1000 THEN
    DO:
      iTimeStarted = ETIME.
      RUN showProgressBar("Dumping, please wait", iNumRecords / piNumRecords * 100).
      PROCESS EVENTS.
      IF glAborted THEN LEAVE pumpDataLoop. 
    END.

    iNumRecords = iNumRecords + 1.

    PUT STREAM strDump UNFORMATTED 
      SKIP "FIND bData".

    /* Keyfields and calculation of name length */
    iNumFields = 0.
    DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
      hField = hTTBuffer:BUFFER-FIELD(iCurField).
      iExtBegin = (IF hField:EXTENT = 0 THEN 0 ELSE 1).
      iExtEnd   = MAXIMUM(hField:EXTENT, 0).

      DO iExtent = iExtBegin TO iExtEnd:
        /* Calculate the length of this field name */
        cFieldName = SUBSTITUTE("&1&2",hField:NAME, IF iExtent > 0 THEN SUBSTITUTE("[&1]",iExtent) ELSE "").
        iMaxLength = MAXIMUM(iMaxLength,LENGTH(cFieldName)).

        IF LOOKUP(hField:NAME, cKeyfields) > 0 OR cKeyfields = "" THEN
        DO:
            iNumFields = iNumFields + 1.
            cFieldValue = getFieldValue(hField,iExtent).

            PUT STREAM strDump UNFORMATTED SKIP 
              SUBSTITUTE("  &1 bData.&2 = &3"
                          , (IF iNumFields = 1 THEN "WHERE" ELSE "  AND")
                          , cFieldName
                          , cFieldValue
                          ).
        END.
      END.
    END.

    /* Calculate the format for the fields to allign then nicely */
    cFieldNameFormat = SUBSTITUTE("X(&1)",iMaxLength).

    PUT STREAM strDump UNFORMATTED 
      SKIP "        EXCLUSIVE-LOCK NO-ERROR."
      SKIP " "
      SKIP "IF NOT AVAILABLE bData THEN"
      SKIP "  CREATE bData."
      SKIP " ".

    /* data */
    iNumFields = 0.
    DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
      hField    = hTTBuffer:BUFFER-FIELD(iCurField).
      iExtBegin = (IF hField:EXTENT = 0 THEN 0 ELSE 1).
      iExtEnd   = MAXIMUM(hField:EXTENT, 0).
      IF LOOKUP(hField:DATA-TYPE, "raw,clob,blob") > 0 THEN NEXT.

      DO iExtent = iExtBegin TO iExtEnd:
        /* Place an assign statement every 100 fields */
        IF iNumFields MODULO 100 = 0 THEN
        DO:
          /* Closing dot for previouse assign */
          IF iNumFields > 0 THEN PUT STREAM strDump UNFORMATTED SKIP SUBSTITUTE("  ."). 
          /* New assign */
          PUT STREAM strDump UNFORMATTED SKIP SUBSTITUTE("ASSIGN").
        END.

        cFieldName = SUBSTITUTE("&1&2",hField:NAME, IF iExtent > 0 THEN SUBSTITUTE("[&1]",iExtent) ELSE "").
        iNumFields = iNumFields + 1.
        cFieldValue = getFieldValue(hField,iExtent).

        PUT STREAM strDump UNFORMATTED 
          SKIP SUBSTITUTE("  bData.&1 = &2"
                          , STRING(cFieldName,cFieldNameFormat)
                          , cFieldValue
                          ).
      END.
    END.
    PUT STREAM strDump UNFORMATTED SKIP "  ." SKIP(2).

  END. /* pumpDataLoop */

  OUTPUT stream strDump close.

  /* Hide progress bar frame */
  RUN showProgressBar("", ?).

  DELETE OBJECT hQuery.

END PROCEDURE. /* DumpData4GL */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataCSV wDump 
PROCEDURE DumpDataCSV :
/*------------------------------------------------------------------------------
  Notes: Copied the TXT dump procedure and updated to make CSV - mbd 2015.03.20
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER picFileName  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pihTempTable AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER piNumRecords AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER pcCodePage   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cFieldFormat        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cName               AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hField              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hTTBuffer           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iCurField           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iField              AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNrOfRecords        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iExtent             AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iTimeStarted        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cCodePage           AS CHARACTER   NO-UNDO. 
  DEFINE VARIABLE cSeparator          AS CHARACTER   NO-UNDO.

  DO WITH FRAME {&FRAME-NAME}:
    ASSIGN
      cCodePage    = (IF pcCodePage <> "" THEN pcCodePage ELSE SESSION:CPSTREAM)
      iTimeStarted = ETIME
      hTTBuffer    = pihTempTable:DEFAULT-BUFFER-HANDLE
      iNrOfRecords = 0
      .
    CASE cbSeparator:SCREEN-VALUE:
      WHEN "1" THEN cSeparator = ",".
      WHEN "2" THEN cSeparator = "|".
      WHEN "3" THEN cSeparator = ";".
    END CASE.
  END.

  /* Open outputfile */
  OUTPUT stream strDump to value(picFileName) convert target cCodePage.

  /* Pump field names as column headers*/
  iField = 0.
  DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
    hField = hTTBuffer:BUFFER-FIELD(iCurField).
    IF LOOKUP(hField:DATA-TYPE,'raw,clob,blob') > 0 THEN NEXT.
    iField = iField + 1.
    
    /* Take regular field name or including extent */
    cName = (IF hField:EXTENT > 1 
               THEN SUBSTITUTE('&1[&2]',hField:NAME, hField:EXTENT) 
               ELSE hField:LABEL ).

    IF hField:EXTENT > 1 THEN
    DO iExtent = 1 TO hField:EXTENT:
      PUT STREAM strDump UNFORMATTED 
        (IF iCurField = 1 AND iExtent = 1 THEN "" ELSE cSeparator)
        SUBSTITUTE('&1[&2]',hField:LABEL, iExtent).
    END.
    ELSE
    DO:
      PUT STREAM strDump UNFORMATTED
       (IF iCurField = 1 THEN "" ELSE cSeparator)
       hField:LABEL.
    END.
  END.
  PUT STREAM strDump UNFORMATTED SKIP.

  /* Build query */
  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hTTBuffer).
  hQuery:QUERY-PREPARE( SUBSTITUTE( "for each &1 no-lock", hTTBuffer:NAME)).
  hQuery:QUERY-OPEN().

  /* Pump the table data into the table */
  pumpDataLoop:
  REPEAT:
    hQuery:GET-NEXT().
    IF hQuery:QUERY-OFF-END THEN LEAVE pumpDataLoop.

    IF (ETIME - iTimeStarted) > 1000 THEN
    DO:
      iTimeStarted = ETIME.
      RUN showProgressBar( 'Dumping records, please wait', iNrOfRecords / piNumRecords * 100 ).
      PROCESS EVENTS.
      IF glAborted THEN LEAVE pumpDataLoop. 
    END.

    iNrOfRecords = iNrOfRecords + 1.
    iField = 0.
    DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
      hField = hTTBuffer:BUFFER-FIELD(iCurField).
      
      IF LOOKUP(hField:DATA-TYPE,'raw,clob,blob') > 0 THEN NEXT.
      iField = iField + 1.

      /* Find out format of field */
      FIND ttField WHERE ttField.cFullName = hField:NAME.
      IF tbUseCustomizedFormats THEN cFieldFormat = ttField.cFormat.
                                ELSE cFieldFormat = ttField.cFormatOrg.

      IF hField:EXTENT > 1 THEN
      DO iExtent = 1 TO hField:EXTENT:
        PUT STREAM strDump UNFORMATTED 
          (IF iCurField = 1 AND iExtent = 1 THEN "" ELSE cSeparator)
          QUOTER(TRIM(SUBSTITUTE('&1', STRING(hField:BUFFER-VALUE(iExtent), cFieldFormat)))).
      END.
      ELSE
      DO:
        PUT STREAM strDump UNFORMATTED 
          (IF iCurField = 1 THEN "" ELSE cSeparator)
          QUOTER(TRIM(SUBSTITUTE('&1',STRING(hField:BUFFER-VALUE,cFieldFormat)))).
      END.
    END. 

    PUT STREAM strDump UNFORMATTED SKIP.

  END. /* pumpDataLoop */
   
  OUTPUT stream strDump close.
  
  /* Hide progress bar frame */
  RUN showProgressBar('',?).
  
  DELETE OBJECT hQuery.

END PROCEDURE. /* DumpDataCSV */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataExcel wDump 
PROCEDURE DumpDataExcel :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER picFileName  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pihTempTable AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER piNumRecords AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER pcCodePage   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cColumnRange        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hExcel              AS COM-HANDLE  NO-UNDO.
  DEFINE VARIABLE hWorkbook           AS COM-HANDLE  NO-UNDO.
  DEFINE VARIABLE hWorksheet          AS COM-HANDLE  NO-UNDO.

  /* First, dump the file as HTML */
  RUN DumpDataHtml
        ( INPUT picFileName 
        , INPUT pihTempTable
        , INPUT piNumRecords
        , INPUT pcCodePage
        ).

  IF glAborted THEN RETURN. 

  FILE-INFO:FILE-NAME = picFileName.

  /* Open Excel and initialize variables */
  CREATE "Excel.Application" hExcel.
  ASSIGN
    hExcel:visible = FALSE
    hWorkbook      = hExcel:Workbooks:open(FILE-INFO:FULL-PATHNAME)
    hWorkSheet     = hExcel:Sheets:item(1)
    .
  
  /* Adjust column sizes */
  hExcel:columns("A:ZZ"):select.
  hExcel:selection:columns:Autofit.

  /* Set first row as title row with autofilter */
  hWorksheet:Range("A1:A1"):Select.
  hWorkbook:Windows(1):SplitColumn = 0.
  hWorkbook:Windows(1):SplitRow    = 1.
  hWorkbook:Windows(1):FreezePanes = TRUE.
  hWorksheet:Range("A1:A1"):AutoFilter(1,?,?). 

  /* Perform housekeeping and cleanup steps */
  hExcel:DisplayAlerts = FALSE.  /* don't show confirmation dialog from excel */
  hWorkbook:SaveAs(picFileName,1,?,?,?,?,?).
  hExcel:application:Workbooks:close() NO-ERROR.
  hExcel:application:quit NO-ERROR.
  
  RELEASE OBJECT hWorksheet.
  RELEASE OBJECT hWorkbook.
  RELEASE OBJECT hExcel.

END PROCEDURE. /* DumpDataExcel */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataHtml wDump 
PROCEDURE DumpDataHtml :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER picFileName  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pihTempTable AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER piNumRecords AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER pcCodePage   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE hField              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hTTBuffer           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iCurField           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNrOfRecords        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iExtent             AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iTimeStarted        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cCodePage           AS CHARACTER   NO-UNDO. 
  DEFINE VARIABLE cData               AS CHARACTER   NO-UNDO. 

  ASSIGN
    iTimeStarted = ETIME
    hTTBuffer    = pihTempTable:DEFAULT-BUFFER-HANDLE
    iNrOfRecords = 0
    cCodePage    = (IF pcCodePage <> "" THEN pcCodePage ELSE SESSION:CPSTREAM)
    .

  /* Open outputfile */
  OUTPUT stream strDump to value(picFileName) convert target cCodePage.
  PUT STREAM strDump UNFORMATTED 
    '<html><body><table border="0"><tr bgcolor="KHAKI">'.

  /* Pump field names as column headers*/
  DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
    hField = hTTBuffer:BUFFER-FIELD(iCurField).
    IF LOOKUP(hField:DATA-TYPE,'clob,blob,raw') > 0 THEN NEXT. 

    IF hField:EXTENT > 1 THEN
    DO iExtent = 1 TO hField:EXTENT:
      PUT STREAM strDump UNFORMATTED
        SKIP SUBSTITUTE('<th>&1[&2]</th>', hField:NAME, iExtent).
    END.
    ELSE
    DO:
      PUT STREAM strDump UNFORMATTED
        SKIP SUBSTITUTE('<th>&1</th>', hField:NAME).
    END.
  END.

  PUT STREAM strDump UNFORMATTED '</tr>'.
  
  /* Build query */
  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hTTBuffer).
  hQuery:QUERY-PREPARE( SUBSTITUTE( "for each &1 no-lock", hTTBuffer:NAME)).
  hQuery:QUERY-OPEN().

  /* Pump the table data into the table */
  pumpDataLoop:
  REPEAT:
    hQuery:GET-NEXT().
    IF hQuery:QUERY-OFF-END THEN LEAVE pumpDataLoop.

    IF (ETIME - iTimeStarted) > 1000 THEN
    DO:
      iTimeStarted = ETIME.
      RUN showProgressBar( 'Dumping records, please wait', iNrOfRecords / piNumRecords * 100 ).
      PROCESS EVENTS.
      IF glAborted THEN LEAVE pumpDataLoop. 
    END.

    PUT STREAM strDump UNFORMATTED SKIP 
      SUBSTITUTE('<tr bgcolor="&1">', TRIM(STRING(iNrOfRecords MOD 2 = 1,'WHITE/LIGHTYELLOW')) ).

    iNrOfRecords = iNrOfRecords + 1.
    DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
      hField = hTTBuffer:BUFFER-FIELD(iCurField).
      IF LOOKUP(hField:DATA-TYPE,'clob,blob,raw') > 0 THEN NEXT. 

      /* Find out format of field */
      FIND ttField WHERE ttField.cFullName = hField:NAME.

      IF hField:EXTENT > 1 THEN
      DO iExtent = 1 TO hField:EXTENT:

        cData = IF tbUseCustomizedFormats 
                  THEN TRIM(STRING(hField:BUFFER-VALUE(iExtent),ttField.cFormat))
                  ELSE hField:BUFFER-VALUE(iExtent).

        cData = getEscapedData("HTML", cData).

        PUT STREAM strDump UNFORMATTED 
          SKIP SUBSTITUTE('<td>&1</td>', cData).
      END.
      ELSE
      DO:
        cData = IF tbUseCustomizedFormats 
                  THEN TRIM(STRING(hField:BUFFER-VALUE,ttField.cFormat))
                  ELSE hField:BUFFER-VALUE.

        cData = getEscapedData("HTML", cData).

        PUT STREAM strDump UNFORMATTED 
          SKIP SUBSTITUTE('<td>&1</td>', cData).
      END.
    END. 
    PUT STREAM strDump UNFORMATTED '</tr>'.

  END. /* pumpDataLoop */
   
  PUT STREAM strDump UNFORMATTED '</table></body></html>'.
  OUTPUT stream strDump close.
  
  /* Hide progress bar frame */
  RUN showProgressBar('',?).
  
  DELETE OBJECT hQuery.

END PROCEDURE. /* DumpDataExcel */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataProgressD wDump 
PROCEDURE DumpDataProgressD :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER picFileName  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pihTempTable AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER piNumRecords AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER pcCodePage   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cField              AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTimeStamp          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hQuery              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hTTBuffer           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iBack               AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iCurField           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iExtent             AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNrOfRecords        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iTimeStarted        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iTrailer            AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lFirstFieldOfRecord AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE cCodePage           AS CHARACTER   NO-UNDO. 

  ASSIGN
    cCodePage    = (IF pcCodePage <> "" THEN pcCodePage ELSE SESSION:CPSTREAM)
    iTimeStarted = ETIME
    hTTBuffer    = pihTempTable:DEFAULT-BUFFER-HANDLE
    iNrOfRecords = 0
    cTimeStamp   = STRING(YEAR( TODAY),"9999":u) + "/":u
                 + string(MONTH(TODAY),"99":u  ) + "/":u
                 + string(DAY(  TODAY),"99":u  ) + "-":u
                 + string(TIME,"HH:MM:SS":u).

  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hTTBuffer).
  hQuery:QUERY-PREPARE( SUBSTITUTE( "for each &1 no-lock", hTTBuffer:NAME)).
  hQuery:QUERY-OPEN().
  
  /* Open outputfile */
  OUTPUT stream strDump to value(picFileName) no-echo no-map convert target cCodePage.
  EXPORT STREAM strDump ?.
  iBack = SEEK(strDump) - 1.
  SEEK STREAM strDump TO 0.    
  
  pumpDataLoop:
  REPEAT:
    hQuery:GET-NEXT().
    IF hQuery:QUERY-OFF-END THEN LEAVE pumpDataLoop.

    ASSIGN 
      iNrOfRecords        = iNrOfRecords + 1
      lFirstFieldOfRecord = TRUE
      .

    IF (ETIME - iTimeStarted) > 1000 THEN
    DO:
      iTimeStarted = ETIME.
      RUN showProgressBar( 'Dumping records, please wait', iNrOfRecords / piNumRecords * 100 ).
      PROCESS EVENTS.
      IF glAborted THEN LEAVE pumpDataLoop. 
    END.

    DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:

      IF cbiFieldSelection = 2 THEN
      DO:
        cField = hTTBuffer:BUFFER-FIELD(iCurField):name.
        IF LOOKUP(cField,picSelectedFields) = 0 THEN NEXT.
      END.

      IF lFirstFieldOfRecord THEN
        lFirstFieldOfRecord = FALSE.
      ELSE
      DO:
        SEEK STREAM strDump TO SEEK(strDump) - iBack.
        PUT STREAM strDump CONTROL ' ':u.
      END.

      IF hTTBuffer:BUFFER-FIELD(iCurField):extent > 1 THEN
      DO iExtent = 1 TO hTTBuffer:BUFFER-FIELD(iCurField):extent:
        
        IF iExtent > 1 THEN
        DO:
          SEEK STREAM strDump TO SEEK(strDump) - iBack.
          PUT STREAM strDump CONTROL ' ':u.
        END.

        EXPORT STREAM strDump hTTBuffer:BUFFER-FIELD(iCurField):buffer-value(iExtent).
      END.
      ELSE
        EXPORT STREAM strDump hTTBuffer:BUFFER-FIELD(iCurField):buffer-value.

    END. /* Fields */
  END. /* while hTTBuffer:available */
  
  hQuery:QUERY-CLOSE().
  DELETE OBJECT hQuery.
  hQuery = ?.
  
  IF NOT glAborted THEN
  DO:
    PUT STREAM strDump UNFORMATTED ".":u SKIP.
    iTrailer = SEEK(strDump).
    
    PUT STREAM strDump UNFORMATTED
           "PSC":u 
      SKIP "filename=":u   SUBSTRING(hTTBuffer:TABLE,INDEX(hTTBuffer:TABLE,"_") + 1) 
      SKIP "records=":u    STRING(iNrOfRecords,"9999999999999":u) 
      SKIP "ldbname=":u    ENTRY(1,hTTBuffer:TABLE,"_")
      SKIP "timestamp=":u  cTimeStamp 
      SKIP "numformat=":u  ASC(SESSION:NUMERIC-SEPARATOR) ",":u ASC(SESSION:NUMERIC-DECIMAL-POINT) 
      SKIP "dateformat=":u SESSION:DATE-FORMAT "-":u SESSION:YEAR-OFFSET 
      SKIP "map=NO-MAP":u 
      SKIP "cpstream=":u   cCodePage 
      SKIP ".":u 
      SKIP STRING(iTrailer,"9999999999":u) 
      SKIP.
  END.
  
  OUTPUT stream strDump close.
  
  /* Hide progress bar frame */
  RUN showProgressBar('',?).

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataTxt wDump 
PROCEDURE DumpDataTxt :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER picFileName  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pihTempTable AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER piNumRecords AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER pcCodePage   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cFieldFormat        AS CHARACTER   NO-UNDO. 
  DEFINE VARIABLE cDumpFormat         AS CHARACTER   NO-UNDO. 
  DEFINE VARIABLE cName               AS CHARACTER   NO-UNDO. 
  DEFINE VARIABLE hField              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hTTBuffer           AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery              AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iCurField           AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iField              AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iNrOfRecords        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iExtent             AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iTimeStarted        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iLength             AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cDumpFormatList     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldFormatList    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cCodePage           AS CHARACTER   NO-UNDO. 

  ASSIGN
    cCodePage    = (IF pcCodePage <> "" THEN pcCodePage ELSE SESSION:CPSTREAM)
    iTimeStarted = ETIME
    hTTBuffer    = pihTempTable:DEFAULT-BUFFER-HANDLE
    iNrOfRecords = 0
    .

  /* Open outputfile */
  OUTPUT stream strDump to value(picFileName) convert target cCodePage.

  /* Pump field names as column headers*/
  iField = 0.
  DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
    hField = hTTBuffer:BUFFER-FIELD(iCurField).
    IF LOOKUP(hField:DATA-TYPE,'raw,clob,blob') > 0 THEN NEXT.
    iField = iField + 1.
    
    /* Take the larger one of name and actual format */
    cName = (IF hField:EXTENT > 1 
               THEN SUBSTITUTE('&1[&2]',hField:NAME, hField:EXTENT) 
               ELSE hField:LABEL ).

    /* Find out format of field */
    FIND ttField WHERE ttField.cFullName = hField:NAME.
    IF tbUseCustomizedFormats THEN cFieldFormat = ttField.cFormat.
                              ELSE cFieldFormat = ttField.cFormatOrg.
    
    /* What is the largest? Take at least length 10 for date fields */
    iLength = MAXIMUM( LENGTH( STRING(hField:INITIAL, cFieldFormat) )
                     , LENGTH( cName )).
    IF hField:DATA-TYPE = 'date' THEN iLength = MAXIMUM(iLength,10).
    cDumpFormat = SUBSTITUTE('X(&1)', iLength).

    /* Save for future use */
    cDumpFormatList  = cDumpFormatList  + cDumpFormat  + chr(1).
    cFieldFormatList = cFieldFormatList + cFieldFormat + chr(1).


    IF hField:EXTENT > 1 THEN
    DO iExtent = 1 TO hField:EXTENT:
      PUT STREAM strDump UNFORMATTED 
        STRING(SUBSTITUTE('&1[&2]',hField:LABEL, iExtent),cDumpFormat) ' '.
    END.
    ELSE
    DO:
      PUT STREAM strDump UNFORMATTED
        STRING(hField:LABEL,cDumpFormat) ' '.
    END.
  END.
  PUT STREAM strDump UNFORMATTED SKIP.

  
  /* And nice lines below them please */
  iField = 0.
  DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
    hField = hTTBuffer:BUFFER-FIELD(iCurField).
    IF LOOKUP(hField:DATA-TYPE,'raw,clob,blob') > 0 THEN NEXT.
    iField = iField + 1.

    cFieldFormat = ENTRY(iField,cFieldFormatList,CHR(1)).
    cDumpFormat  = ENTRY(iField,cDumpFormatList, CHR(1)).

    IF hField:EXTENT > 1 THEN
    DO iExtent = 1 TO hField:EXTENT:
      PUT STREAM strDump UNFORMATTED 
        STRING(FILL('-', 1000),cDumpFormat) ' '.
    END.
    ELSE
    DO:
      PUT STREAM strDump UNFORMATTED
        STRING(FILL('-', 1000),cDumpFormat) ' '.
    END.
  END. 
  PUT STREAM strDump UNFORMATTED SKIP.

  /* Build query */
  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hTTBuffer).
  hQuery:QUERY-PREPARE( SUBSTITUTE( "for each &1 no-lock", hTTBuffer:NAME)).
  hQuery:QUERY-OPEN().

  /* Pump the table data into the table */
  pumpDataLoop:
  REPEAT:
    hQuery:GET-NEXT().
    IF hQuery:QUERY-OFF-END THEN LEAVE pumpDataLoop.

    IF (ETIME - iTimeStarted) > 1000 THEN
    DO:
      iTimeStarted = ETIME.
      RUN showProgressBar( 'Dumping records, please wait', iNrOfRecords / piNumRecords * 100 ).
      PROCESS EVENTS.
      IF glAborted THEN LEAVE pumpDataLoop. 
    END.

    iNrOfRecords = iNrOfRecords + 1.
    iField = 0.
    DO iCurField = 1 TO hTTBuffer:NUM-FIELDS:
      hField = hTTBuffer:BUFFER-FIELD(iCurField).
      IF LOOKUP(hField:DATA-TYPE,'raw,clob,blob') > 0 THEN NEXT.
      iField = iField + 1.

      cFieldFormat = ENTRY(iField,cFieldFormatList,CHR(1)).
      cDumpFormat  = ENTRY(iField,cDumpFormatList, CHR(1)).

      IF hField:EXTENT > 1 THEN
      DO iExtent = 1 TO hField:EXTENT:
        PUT STREAM strDump UNFORMATTED 
          STRING( SUBSTITUTE('&1', STRING(hField:BUFFER-VALUE(iExtent), cFieldFormat)), cDumpFormat ) ' '.
      END.
      ELSE
      DO:
        PUT STREAM strDump UNFORMATTED 
          STRING( SUBSTITUTE('&1', STRING(hField:BUFFER-VALUE, cFieldFormat)), cDumpFormat ) ' '.
      END.
    END. 

    PUT STREAM strDump UNFORMATTED SKIP.

  END. /* pumpDataLoop */
   
  OUTPUT stream strDump close.
  
  /* Hide progress bar frame */
  RUN showProgressBar('',?).
  
  DELETE OBJECT hQuery.

END PROCEDURE. /* DumpDataText */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataXml wDump 
PROCEDURE DumpDataXml :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER picFileName  AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pihTempTable AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER piNumRecords AS INTEGER     NO-UNDO.
  DEFINE INPUT PARAMETER pcCodePage   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cTargetType      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFile            AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lFormatted       AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE cEncoding        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSchemaLocation  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lWriteSchema     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lMinSchema       AS LOGICAL     NO-UNDO.
  
  DO WITH FRAME {&frame-name}:

    RUN showProgressBar( SUBSTITUTE('Dumping &1 records, please wait', piNumRecords) , 0).

    ASSIGN     
      cTargetType     = "file"     
      cFile           = picFileName
      lFormatted      = YES     
      cEncoding       = ?     
      cSchemaLocation = ?     
      lWriteSchema    = tbExportSchema:checked
      lMinSchema      = tbMinimalSchema:checked
      .
  
    /* Fix XML Node Names for fields in the tt */
    RUN setXmlNodeNames(INPUT pihTempTable:DEFAULT-BUFFER-HANDLE).

    pihTempTable:WRITE-XML
      ( cTargetType
      , cFile
      , lFormatted
      , cEncoding
      , cSchemaLocation
      , lWriteSchema
      , lMinSchema
      ).
  
    RUN showProgressBar( '', ? ).
  END. /* do with frame {&frame-name}: */

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI wDump  _DEFAULT-ENABLE
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
  DISPLAY ficFileName cbDumpType cbCodePage tbUseCustomizedFormats cbSeparator 
          cbNumericFormat tbExportSchema cbiRecordSelection cbDateFormat 
          tbMinimalSchema cbiFieldSelection ficMessageNow ficMessage 
          tbDumpReadyClose tbDumpReadyExplore tbDumpReadyView 
          tbDumpReadyClipboard 
      WITH FRAME DEFAULT-FRAME IN WINDOW wDump.
  ENABLE RECT-2 RECT-3 RECT-4 ficFileName btnChooseDumpFile cbDumpType 
         cbCodePage tbUseCustomizedFormats cbSeparator cbNumericFormat 
         cbiRecordSelection cbDateFormat cbiFieldSelection btnDump btnClose 
         tbDumpReadyClose tbDumpReadyExplore tbDumpReadyView 
         tbDumpReadyClipboard 
      WITH FRAME DEFAULT-FRAME IN WINDOW wDump.
  {&OPEN-BROWSERS-IN-QUERY-DEFAULT-FRAME}
  DISPLAY fcInfoLine 
      WITH FRAME infoFrame IN WINDOW wDump.
  ENABLE rcBorder rcBody btAbort fcInfoLine 
      WITH FRAME infoFrame IN WINDOW wDump.
  {&OPEN-BROWSERS-IN-QUERY-infoFrame}
  VIEW wDump.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject wDump 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE VARIABLE cFileViewCmd     AS CHARACTER  NO-UNDO.
  DEFINE VARIABLE cDumpReadyAction AS CHARACTER  NO-UNDO.

  RUN LockWindow (INPUT wDump:HANDLE, INPUT YES).
  RUN enable_UI.

  HIDE FRAME infoFrame.

  DO WITH FRAME {&FRAME-NAME}:

    /* Get fonts */
    FRAME {&FRAME-NAME}:FONT = getFont('Default').
    FRAME infoFrame:FONT = getFont('Default').

    RUN setLabelPosition(ficFileName:HANDLE).
    RUN setLabelPosition(cbDumpType:HANDLE).
    RUN setLabelPosition(cbSeparator:HANDLE).
    RUN setLabelPosition(cbiRecordSelection:HANDLE).
    RUN setLabelPosition(cbiFieldSelection:HANDLE).
    RUN setLabelPosition(cbCodePage:HANDLE).
    RUN setLabelPosition(cbNumericFormat:HANDLE).
    RUN setLabelPosition(cbDateFormat:HANDLE).
                                          
    btnViewLastDump:LOAD-IMAGE(getImagePath('View.gif')).
    btnOpenLastDumpDir:LOAD-IMAGE(getImagePath('OpenFolder.gif')).

    ASSIGN
      gcDb    = pihBrowse:QUERY:get-buffer-handle(1):dbname
      gcTable = pihBrowse:QUERY:get-buffer-handle(1):table
      .

    RUN setStatusMessage
      ( INPUT DATETIME(getRegistry("DumpAndLoad", "DumpActionTimeStamp"))
      , INPUT getRegistry("DumpAndLoad", "DumpActionResult")
      ).

    cFileViewCmd = getRegistry("DumpAndLoad", "DumpFileViewCmd").
    IF cFileViewCmd = ? THEN
    DO:
      ASSIGN cFileViewCmd = "Start &1".
      setRegistry("DumpAndLoad", "DumpFileViewCmd", cFileViewCmd ).
    END.
    
    cbDumpType = getRegistry("DumpAndLoad", "DumpExportType" ).
    IF cbDumpType = ? THEN cbDumpType = "XML".

    cbiRecordSelection = INTEGER( getRegistry("DumpAndLoad", "DumpFilter")).
    IF cbiRecordSelection = ? THEN cbiRecordSelection = 2.

    cbiFieldSelection = INTEGER( getRegistry("DumpAndLoad", "DumpFilterFields")).
    IF cbiFieldSelection = ? THEN cbiFieldSelection = 1.

    tbExportSchema = LOGICAL( getRegistry("DumpAndLoad", "DumpXmlSchema")).
    IF tbExportSchema = ? THEN tbExportSchema = TRUE.

    tbMinimalSchema = LOGICAL( getRegistry("DumpAndLoad", "DumpMinimalXmlSchema")).
    IF tbMinimalSchema = ? THEN tbMinimalSchema = FALSE.
    
    cbSeparator = INTEGER(getRegistry("DumpAndLoad", "FieldSeparator")).
    IF cbSeparator = ? THEN cbSeparator = 1. /* comma */

    /* Set current setting for codepage */
    cbCodePage = SESSION:CPSTREAM.

    /* Add current setting to combo for numeric format */
    gcSessionNumericFormat = SESSION:NUMERIC-FORMAT.
    cbNumericFormat:DELIMITER = '|'.
    cbNumericFormat:LIST-ITEM-PAIRS = SUBSTITUTE('Session (&1)|&1|American    (1,003.14)|American|European   (1.003,14)|European', SESSION:NUMERIC-FORMAT).
    cbNumericFormat = getRegistry("DumpAndLoad", "DumpNumericFormat" ).
    IF cbNumericFormat = ? THEN cbNumericFormat = ENTRY(2,cbNumericFormat:LIST-ITEM-PAIRS,'|').

    /* Add current setting to combo for date format */
    gcSessionDateFormat = SESSION:DATE-FORMAT.
    cbDateFormat:LIST-ITEM-PAIRS = SUBSTITUTE('Session (&1),&1,DMY (18-03-2010),DMY,MDY (03-18-2010),MDY,YMD (2010-03-18),YMD,YDM (2010-18-03),YDM,MYD (03-2010-18),MYD,DYM (18-2010-03),DYM', SESSION:DATE-FORMAT).
    cbDateFormat = getRegistry("DumpAndLoad", "DumpDateFormat" ).
    IF cbDateFormat = ? THEN cbDateFormat = ENTRY(2,cbDateFormat:LIST-ITEM-PAIRS).

    /* DumpReadyActions */
    cDumpReadyAction = getRegistry('DumpAndLoad','DumpReadyAction').
    IF cDumpReadyAction <> ? THEN
      ASSIGN
        tbDumpReadyClose    :CHECKED = LOOKUP('Close'    , cDumpReadyAction ) > 0
        tbDumpReadyExplore  :CHECKED = LOOKUP('Explore'  , cDumpReadyAction ) > 0
        tbDumpReadyView     :CHECKED = LOOKUP('View'     , cDumpReadyAction ) > 0
        tbDumpReadyClipboard:CHECKED = LOOKUP('Clipboard', cDumpReadyAction ) > 0
        .

    DISPLAY 
      cbDumpType 
      cbiRecordSelection 
      cbiFieldSelection 
      cbSeparator
      tbExportSchema 
      tbMinimalSchema 
      cbCodePage
      cbNumericFormat
      cbDateFormat
      ficMessageNow 
      ficMessage.

  END.
  
  RUN setDumpFileName.
  RUN cbDumpTypeValueChanged.
  RUN cbiRecordSelectionValueChanged.

  /* For some reasons, these #*$&# scrollbars keep coming back */
  RUN showScrollBars(FRAME {&FRAME-NAME}:HANDLE, NO, NO). /* KILL KILL KILL */
  RUN LockWindow (INPUT wDump:HANDLE, INPUT NO).

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setDumpFileName wDump 
PROCEDURE setDumpFileName :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  DEFINE VARIABLE cError    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFileName AS CHARACTER   NO-UNDO.

  DO WITH FRAME {&frame-name}:

    ASSIGN cbDumpType.

    RUN getDumpFileName
      ( INPUT 'Dump'
      , INPUT gcDB       
      , INPUT gcTable    
      , INPUT cbDumpType
      , INPUT ""
      , OUTPUT cFileName
      ).
  
    ficFileName = cFileName.
    DISPLAY ficFileName.
  END.

  RUN btnViewLastDumpEnable.

END PROCEDURE. /* setDumpFileName */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setStatusMessage wDump 
PROCEDURE setStatusMessage :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER pidtTimeStamp AS DATETIME   NO-UNDO.
  DEFINE INPUT PARAMETER picMessage    AS CHARACTER  NO-UNDO.

  DO WITH FRAME {&frame-name}:

    IF pidtTimeStamp = ? THEN
      ficMessageNow = ''.
    ELSE 
      ficMessageNow = STRING(pidtTimeStamp,"99-99-9999 HH:MM:SS").

    IF picMessage = ? THEN
      ficMessage = "".
    ELSE 
      ficMessage = picMessage.

    DISPLAY ficMessageNow ficMessage.

  END. /* do with frame {&frame-name}: */

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showProgressBar wDump 
PROCEDURE showProgressBar :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  DEFINE INPUT  PARAMETER pcInfoText AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER piPrcDone  AS INTEGER     NO-UNDO.

  DEFINE VARIABLE iNewWidth AS INTEGER     NO-UNDO.
  
  PROCESS EVENTS.
  
  IF piPrcDone = ? THEN
  DO:
    FRAME infoFrame:visible = NO.
  END.
  ELSE 
  DO:
    VIEW FRAME infoFrame.
    ENABLE btAbort WITH FRAME infoFrame. 
    fcInfoLine:SCREEN-VALUE = pcInfoText.

    iNewWidth = (MINIMUM(100,piPrcDone) / 100) * rcBorder:width-pixels.
    rcBody:VISIBLE = (iNewWidth > 0).
    IF iNewWidth > 0 THEN 
      rcBody:WIDTH-PIXELS = iNewWidth.

    PROCESS EVENTS.
  END.

END PROCEDURE. /* showProgressBar */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE tbDumpReadyClipboard wDump 
PROCEDURE tbDumpReadyClipboard :
/*------------------------------------------------------------------------------
  Purpose:     Put last dumpfile in clipboard
  2012-09-14 JEE Created
------------------------------------------------------------------------------*/

  DEFINE VARIABLE cLine    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iLength  AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lTooMuch AS LOGICAL     NO-UNDO.

  OUTPUT TO "clipboard".
  INPUT STREAM strDump FROM VALUE(gcLastFile).
  REPEAT:
    IMPORT STREAM strDump UNFORMATTED cLine.
    IF cLine <> ? THEN
    DO:
      PUT UNFORMATTED cLine SKIP.
      iLength = iLength + length(cLine).
    END.
    IF iLength > 65535 THEN
    DO:
      lTooMuch = TRUE.
      LEAVE.
    END.
  END.
  INPUT STREAM strDump CLOSE.
  OUTPUT CLOSE.

  IF lTooMuch THEN
    MESSAGE "Too much data for clipboard. Data has been put on clipboard only partially!" VIEW-AS ALERT-BOX WARNING.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE tbExportSchemaValueChanged wDump 
PROCEDURE tbExportSchemaValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  DO WITH FRAME {&frame-name}:
    
    ASSIGN tbExportSchema.

    IF cbDumpType = "XML" AND tbExportSchema:CHECKED THEN
      ENABLE tbMinimalSchema.
    ELSE
      DISABLE tbMinimalSchema.
  END.

  RUN tbMinimalSchemaValueChanged.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE tbMinimalSchemaValueChanged wDump 
PROCEDURE tbMinimalSchemaValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DO WITH FRAME {&frame-name}:    
    ASSIGN tbMinimalSchema.
  END.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE tbUseCustomizedFormatsValueChanged wDump 
PROCEDURE tbUseCustomizedFormatsValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DO WITH FRAME {&frame-name}:    
    ASSIGN tbUseCustomizedFormats.
  END.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* ************************  Function Implementations ***************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getEscapedData wDump 
FUNCTION getEscapedData RETURNS CHARACTER
  ( pcTarget AS CHARACTER
  , pcString AS CHARACTER ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/
  DEFINE VARIABLE cOutput AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iTmp    AS INTEGER   NO-UNDO.

  /* Garbage in, garbage out  */
  cOutput = pcString. 

  CASE pcTarget:
    WHEN "HTML" THEN
    DO:
      cOutput = REPLACE(cOutput,"<","&lt;").
      cOutput = REPLACE(cOutput,">","&gt;").
    END.

    WHEN "4GL" THEN
    DO:
      /* Replace single quotes because we are using them for 4GL separating too */
      cOutput = REPLACE(cOutput, "'", "~~'"). 

      /* Replace CHR's 1 till 13  */
      DO iTmp = 1 TO 13:
        cOutput = REPLACE(cOutput, CHR(iTmp), "' + CHR(" + STRING(iTmp) + ") + '").
      END.
    END.
  END CASE.

  RETURN pcString.

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getExcelCol wDump 
FUNCTION getExcelCol RETURNS CHARACTER
  ( INPUT iColumnNr AS INTEGER ) :
/*------------------------------------------------------------------------------
  Purpose: Transform a column nr to Excel Column name (27 -> AA)
    Notes:  
------------------------------------------------------------------------------*/

  DEFINE VARIABLE ifirst  AS INTEGER   NO-UNDO.
  DEFINE VARIABLE isecond AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cCols   AS CHARACTER NO-UNDO.

  iFirst  = INTEGER(TRUNCATE((iColumnNr - 1) / 26, 0)).
  iSecond = iColumnNr - (26 * iFirst).
  cCols   = CHR(64 + iSecond).
  IF iFirst > 0 THEN
    cCols = CHR(64 + iFirst) + cCols.

  RETURN cCols.

END FUNCTION. /* getExcelCol */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getFieldListFromIndexInformation wDump 
FUNCTION getFieldListFromIndexInformation RETURNS CHARACTER
  ( INPUT picIndexInformation AS CHARACTER
  ) :

/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  DEFINE VARIABLE cFieldList AS CHARACTER  NO-UNDO.
  DEFINE VARIABLE cIndexName AS CHARACTER  NO-UNDO.
  DEFINE VARIABLE iCurPair   AS INTEGER    NO-UNDO.

  /*   The returned comma-separated list consists of the following in the specified order:  */
  /*   - The index name.                                                                    */
  /*   - Three integer values of value 0 (FALSE) or 1 (TRUE) depending on whether           */
  /*     1 the index is unique                                                              */
  /*     2 the index primary                                                                */
  /*     3 the index is a word index.                                                       */
  /*   - The names of the index fields, each followed by a 0 (ascending) or 1 (descending). */

  ASSIGN cFieldList = "".

  IF picIndexInformation = ? THEN
    RETURN cFieldList.

  cIndexName = ENTRY(1,picIndexInformation).  
  ENTRY(1,picIndexInformation) = "".  
  picIndexInformation = SUBSTRING(picIndexInformation,8).
  
  DO iCurPair = 1 TO NUM-ENTRIES(picIndexInformation) - 1 BY 2:

    cFieldList = SUBSTITUTE( "&1&2&3"
                           , cFieldList 
                           , (IF cFieldList = "" THEN "" ELSE ",")
                           , ENTRY(iCurPair,picIndexInformation)
                           ).
  END. /* do iCurPair = 1 to num-entries(picIndexInformation) - 1 by 2: */
  
  RETURN cFieldList.
END FUNCTION. /* getFieldListFromIndexInformation */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getFieldValue wDump 
FUNCTION getFieldValue RETURNS CHARACTER
  ( phField AS HANDLE
  , piExtent AS INTEGER 
  ) :

/*------------------------------------------------------------------------------
  Purpose: Give back the value of a field in such a way that it can be 
           used to write an assign statement. 
  ------------------------------------------------------------------------------*/

  DEFINE VARIABLE cFieldValue AS CHARACTER   NO-UNDO.

  IF phField:BUFFER-VALUE(piExtent) <> ? THEN
  DO:
    CASE phField:DATA-TYPE:
      WHEN "character"   THEN cFieldValue = SUBSTITUTE("'&1'", getEscapedData("4GL", STRING(phField:BUFFER-VALUE(piExtent)) )).
      WHEN "date"        THEN cFieldValue = SUBSTITUTE("DATE(&1,&2,&3)"
                                                      , MONTH(phField:BUFFER-VALUE(piExtent))
                                                      , DAY(phField:BUFFER-VALUE(piExtent))
                                                      , YEAR(phField:BUFFER-VALUE(piExtent))
                                                      ).
      WHEN "datetime"    THEN cFieldValue = SUBSTITUTE("DATETIME(DATE(&1,&2,&3),&4)"
                                                      , MONTH(phField:BUFFER-VALUE(piExtent))
                                                      , DAY(phField:BUFFER-VALUE(piExtent))
                                                      , YEAR(phField:BUFFER-VALUE(piExtent))
                                                      , MTIME(phField:BUFFER-VALUE(piExtent))
                                                      ).
      WHEN "datetime-tz" THEN cFieldValue = SUBSTITUTE("DATETIME-TZ(DATE(&1,&2,&3),&4,&5)"
                                                    , MONTH(phField:BUFFER-VALUE(piExtent))
                                                    , DAY(phField:BUFFER-VALUE(piExtent))
                                                    , YEAR(phField:BUFFER-VALUE(piExtent))
                                                    , MTIME(phField:BUFFER-VALUE(piExtent))
                                                    , TIMEZONE(phField:BUFFER-VALUE(piExtent))
                                                    ).
      OTHERWISE cFieldValue = STRING(phField:BUFFER-VALUE(piExtent)).
    END CASE.
  END.
  ELSE 
    cFieldValue = "?".

  RETURN cFieldValue.
              
END FUNCTION. /* getFieldValue */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION multipleLookUpGreaterThanZero wDump 
FUNCTION multipleLookUpGreaterThanZero RETURNS LOGICAL
  ( INPUT picExpressions AS CHARACTER
  , INPUT picList        AS CHARACTER
  ) :

/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  DEFINE VARIABLE lGreaterThanZero AS LOGICAL    NO-UNDO.
  DEFINE VARIABLE iTel             AS INTEGER    NO-UNDO.
  
  ASSIGN lGreaterThanZero = TRUE.
  
  DO iTel = 1 TO NUM-ENTRIES(picExpressions):

    ASSIGN
      lGreaterThanZero = lGreaterThanZero AND LOOKUP(ENTRY(iTel,picExpressions), picList) > 0      .

    IF NOT lGreaterThanZero THEN
      RETURN lGreaterThanZero.
  END.

  RETURN lGreaterThanZero.

END FUNCTION. /* function multipleLookUpGreaterThanZero returns logical */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

