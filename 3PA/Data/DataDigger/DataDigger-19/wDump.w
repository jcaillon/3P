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
define input parameter pihBrowse         as handle     no-undo.
define input parameter picSelectedFields as character  no-undo.
define input parameter table for ttField.


/* Global Variable Definitions ---                                      */
define variable gcDB                   as character  no-undo.
define variable gcTable                as character  no-undo.
define variable gcFileName             as character  no-undo.
define variable gcLastFile             as character  no-undo.
define variable gcFileViewCmd          as character  no-undo.
define variable glNoRecordsWarning     as logical     no-undo.
define variable gcSessionNumericFormat as character   no-undo.
define variable gcSessionDateFormat    as character   no-undo.
define variable glAborted              as logical     no-undo.

define stream strDump.

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
btnChooseDumpFile cbDumpType cbCodePage cbiRecordSelection cbNumericFormat ~
tbUseCustomizedFormats cbiFieldSelection cbDateFormat btnDump btnClose ~
tbDumpReadyClose tbDumpReadyExplore tbDumpReadyView tbDumpReadyClipboard 
&Scoped-Define DISPLAYED-OBJECTS ficFileName cbDumpType cbCodePage ~
cbiRecordSelection cbNumericFormat tbUseCustomizedFormats tbExportSchema ~
cbiFieldSelection cbDateFormat tbMinimalSchema ficMessageNow ficMessage ~
tbDumpReadyClose tbDumpReadyExplore tbDumpReadyView tbDumpReadyClipboard 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getEscapedData wDump 
FUNCTION getEscapedData RETURNS CHARACTER
  ( pcTarget as character
  , pcString as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getExcelCol wDump 
FUNCTION getExcelCol RETURNS CHARACTER
  ( input iColumnNr as integer )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getFieldListFromIndexInformation wDump 
FUNCTION getFieldListFromIndexInformation RETURNS CHARACTER
  ( input picIndexInformation as character
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
  ( input picExpressions as character
  , input picList        as character
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
     LABEL "&Code page" 
     VIEW-AS COMBO-BOX INNER-LINES 5
     SIMPLE
     SIZE-PIXELS 130 BY 23 TOOLTIP "the code page used for dumping" NO-UNDO.

DEFINE VARIABLE cbDateFormat AS CHARACTER FORMAT "X(256)":U 
     LABEL "D&ate Format" 
     VIEW-AS COMBO-BOX INNER-LINES 7
     DROP-DOWN-LIST
     SIZE-PIXELS 130 BY 21 TOOLTIP "the date format used for the dump" NO-UNDO.

DEFINE VARIABLE cbDumpType AS CHARACTER FORMAT "X(32)":U 
     LABEL "&Export as" 
     VIEW-AS COMBO-BOX INNER-LINES 6
     LIST-ITEM-PAIRS "Excel","XLS",
                     "HTML","HTML",
                     "Progress dumpfile (*.d)","D",
                     "Text file","TXT",
                     "XML","XML",
                     "4GL code","P"
     DROP-DOWN-LIST
     SIZE-PIXELS 155 BY 21 TOOLTIP "type of format of the file" NO-UNDO.

DEFINE VARIABLE cbiFieldSelection AS INTEGER FORMAT "9":U INITIAL 0 
     LABEL "&Fields" 
     VIEW-AS COMBO-BOX INNER-LINES 2
     LIST-ITEM-PAIRS "All",1,
                     "Selected",2
     DROP-DOWN-LIST
     SIZE-PIXELS 155 BY 21 TOOLTIP "the fieldset that will be exported" NO-UNDO.

DEFINE VARIABLE cbiRecordSelection AS INTEGER FORMAT "9":U INITIAL 0 
     LABEL "&Records" 
     VIEW-AS COMBO-BOX INNER-LINES 3
     LIST-ITEM-PAIRS "Table",1,
                     "Browse",2,
                     "Selection",3
     DROP-DOWN-LIST
     SIZE-PIXELS 155 BY 21 TOOLTIP "the recordset that will be exported" NO-UNDO.

DEFINE VARIABLE cbNumericFormat AS CHARACTER FORMAT "X(256)":U INITIAL "0" 
     LABEL "&Numeric Format" 
     VIEW-AS COMBO-BOX INNER-LINES 5
     DROP-DOWN-LIST
     SIZE-PIXELS 130 BY 21 TOOLTIP "the numeric format used for dumping" NO-UNDO.

DEFINE VARIABLE ficFileName AS CHARACTER FORMAT "X(256)":U 
     LABEL "&Dumpfile" 
     VIEW-AS FILL-IN NATIVE 
     SIZE-PIXELS 590 BY 21 TOOLTIP "the name and path of the resulting dumpfile" NO-UNDO.

DEFINE VARIABLE ficMessage AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 640 BY 22 NO-UNDO.

DEFINE VARIABLE ficMessageNow AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 130 BY 21 NO-UNDO.

DEFINE RECTANGLE RECT-2
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 710 BY 125.

DEFINE RECTANGLE RECT-3
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 710 BY 65.

DEFINE RECTANGLE RECT-4
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 493 BY 40.

DEFINE VARIABLE tbDumpReadyClipboard AS LOGICAL INITIAL no 
     LABEL "Data to Clip&board" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 112 BY 17 NO-UNDO.

DEFINE VARIABLE tbDumpReadyClose AS LOGICAL INITIAL no 
     LABEL "&Close this window" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 123 BY 17 NO-UNDO.

DEFINE VARIABLE tbDumpReadyExplore AS LOGICAL INITIAL no 
     LABEL "E&xplore the dump dir" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 127 BY 17 NO-UNDO.

DEFINE VARIABLE tbDumpReadyView AS LOGICAL INITIAL no 
     LABEL "&View the dump file" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 113 BY 17 NO-UNDO.

DEFINE VARIABLE tbExportSchema AS LOGICAL INITIAL no 
     LABEL "Export &XML schema" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 185 BY 17 TOOLTIP "also export the schema to the XML file" NO-UNDO.

DEFINE VARIABLE tbMinimalSchema AS LOGICAL INITIAL no 
     LABEL "&Minimal Schema" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 180 BY 17 TOOLTIP "use a minimal schema export" NO-UNDO.

DEFINE VARIABLE tbUseCustomizedFormats AS LOGICAL INITIAL no 
     LABEL "&Use Customized Field Formats" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 180 BY 17 TOOLTIP "export with the customized field formats instead of dictionary formats" NO-UNDO.

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
     ficFileName AT Y 15 X 75 COLON-ALIGNED WIDGET-ID 2
     btnChooseDumpFile AT Y 15 X 680 WIDGET-ID 4
     cbDumpType AT Y 45 X 75 COLON-ALIGNED WIDGET-ID 6
     cbCodePage AT Y 45 X 355 COLON-ALIGNED WIDGET-ID 54
     cbiRecordSelection AT Y 70 X 75 COLON-ALIGNED WIDGET-ID 8
     cbNumericFormat AT Y 70 X 355 COLON-ALIGNED WIDGET-ID 48
     tbUseCustomizedFormats AT Y 70 X 510 WIDGET-ID 52
     tbExportSchema AT Y 85 X 510 WIDGET-ID 20
     cbiFieldSelection AT Y 95 X 75 COLON-ALIGNED WIDGET-ID 30
     cbDateFormat AT Y 95 X 355 COLON-ALIGNED WIDGET-ID 50
     tbMinimalSchema AT Y 100 X 510 WIDGET-ID 22
     ficMessageNow AT Y 156 X 10 NO-LABEL WIDGET-ID 18
     ficMessage AT Y 175 X 10 NO-LABEL WIDGET-ID 16
     btnViewLastDump AT Y 175 X 655 WIDGET-ID 28
     btnOpenLastDumpDir AT Y 175 X 678 WIDGET-ID 32
     btnDump AT Y 225 X 560 WIDGET-ID 62
     btnClose AT Y 225 X 640 WIDGET-ID 60
     tbDumpReadyClose AT Y 230 X 15 WIDGET-ID 36
     tbDumpReadyExplore AT Y 230 X 139 WIDGET-ID 38
     tbDumpReadyView AT Y 230 X 269 WIDGET-ID 40
     tbDumpReadyClipboard AT Y 230 X 383 WIDGET-ID 56
     "Last dump" VIEW-AS TEXT
          SIZE-PIXELS 75 BY 13 AT Y 135 X 10 WIDGET-ID 26
     "After the dump ..." VIEW-AS TEXT
          SIZE-PIXELS 95 BY 13 AT Y 208 X 10 WIDGET-ID 44
     RECT-2 AT Y 5 X 5 WIDGET-ID 14
     RECT-3 AT Y 141 X 5 WIDGET-ID 24
     RECT-4 AT Y 214 X 5 WIDGET-ID 34
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 0 Y 0
         SIZE-PIXELS 720 BY 257
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
         TITLE              = "Dump to..."
         HEIGHT-P           = 261
         WIDTH-P            = 724
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
ON END-ERROR OF wDump /* Dump to... */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wDump wDump
ON LEAVE OF wDump /* Dump to... */
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wDump wDump
ON WINDOW-CLOSE OF wDump /* Dump to... */
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
  glAborted = true.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define FRAME-NAME DEFAULT-FRAME
&Scoped-define SELF-NAME btnChooseDumpFile
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnChooseDumpFile wDump
ON CHOOSE OF btnChooseDumpFile IN FRAME DEFAULT-FRAME /* ... */
do:

  define variable     lOkay           as logical    no-undo.
  define variable     cFileName       as character  no-undo.

  cFileName = ficFileName:screen-value.

  system-dialog get-file cFilename
    filters "XML Dumpfile (*.xml)" "*.xml",
            ".d Dumpfile (*.d)" "*.d",
            "Any File (*.*)" "*.*"
    initial-filter 1
    ask-overwrite
    use-filename
    create-test-file
    default-extension ".xml"
    save-as
    update lOkay.
  
  if not lOkay then 
    return.

  do with frame {&frame-name}:
    ficFileName = lc(cFileName).
    display ficFileName.
  end.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDump
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDump wDump
ON CHOOSE OF btnDump IN FRAME DEFAULT-FRAME /* Dump */
or go of frame {&frame-name} 
do:
  run btnDumpChoose.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnOpenLastDumpDir
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnOpenLastDumpDir wDump
ON CHOOSE OF btnOpenLastDumpDir IN FRAME DEFAULT-FRAME /* Open */
DO:
  run btnOpenLastDumpDirChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnViewLastDump
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnViewLastDump wDump
ON CHOOSE OF btnViewLastDump IN FRAME DEFAULT-FRAME /* View */
DO:
  run btnViewLastDumpChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbCodePage
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbCodePage wDump
ON VALUE-CHANGED OF cbCodePage IN FRAME DEFAULT-FRAME /* Code page */
DO:
  run cbCodePageValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbDateFormat
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbDateFormat wDump
ON VALUE-CHANGED OF cbDateFormat IN FRAME DEFAULT-FRAME /* Date Format */
DO:
  run cbDateFormatValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbDumpType
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbDumpType wDump
ON VALUE-CHANGED OF cbDumpType IN FRAME DEFAULT-FRAME /* Export as */
DO:
  run cbDumpTypeValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbiFieldSelection
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbiFieldSelection wDump
ON VALUE-CHANGED OF cbiFieldSelection IN FRAME DEFAULT-FRAME /* Fields */
DO:
  
  run cbiFieldSelectionValueChanged.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbiRecordSelection
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbiRecordSelection wDump
ON VALUE-CHANGED OF cbiRecordSelection IN FRAME DEFAULT-FRAME /* Records */
DO:
  
  run cbiRecordSelectionValueChanged.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME cbNumericFormat
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL cbNumericFormat wDump
ON VALUE-CHANGED OF cbNumericFormat IN FRAME DEFAULT-FRAME /* Numeric Format */
DO:
  run cbNumericFormatValueChanged.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME ficFileName
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL ficFileName wDump
ON LEAVE OF ficFileName IN FRAME DEFAULT-FRAME /* Dumpfile */
DO:
  assign ficFileName.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME tbDumpReadyClose
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL tbDumpReadyClose wDump
ON VALUE-CHANGED OF tbDumpReadyClose IN FRAME DEFAULT-FRAME /* Close this window */
, tbDumpReadyExplore, tbDumpReadyView, tbDumpReadyClipboard
DO:
  define variable cDumpReadyAction as character   no-undo.

  if tbDumpReadyClose    :checked then cDumpReadyAction = cDumpReadyAction + ',Close'.
  if tbDumpReadyExplore  :checked then cDumpReadyAction = cDumpReadyAction + ',Explore'.
  if tbDumpReadyView     :checked then cDumpReadyAction = cDumpReadyAction + ',View'.
  if tbDumpReadyClipboard:checked then cDumpReadyAction = cDumpReadyAction + ',Clipboard'.

  cDumpReadyAction = replace(cDumpReadyAction,",,",","). 
  cDumpReadyAction = trim(cDumpReadyAction,",").
  if cDumpReadyAction = "" then cDumpReadyAction = "Nothing".

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
  run tbMinimalSchemaValueChanged.
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

  RUN enable_UI.
  run initializeObject.

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
END.

/* Restore settings */
session:date-format = gcSessionDateFormat.
session:numeric-format = gcSessionNumericFormat.

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

  define variable cAction  as character   no-undo.
  define variable iAction  as integer     no-undo. 
  define variable cError   as character   no-undo.

  do with frame {&frame-name}:

    /* If no slashes used, check if we are writing to DD dir */
    IF NUM-ENTRIES(ficFileName:SCREEN-VALUE,"\") < 2
      AND NUM-ENTRIES(ficFileName:SCREEN-VALUE,"/") < 2 THEN
    DO:
      FILE-INFO:FILE-NAME = ".".
      IF TRIM(FILE-INFO:FULL-PATHNAME,"/\") = TRIM(getProgramDir(),"/\") THEN 
      DO:
        /* If so, let user confirm */
        RUN showHelp("ExportToProgramdir", "").
        IF getRegistry("DataDigger:help", "ExportToProgramdir:answer") <> "1" THEN RETURN.
      END.
    END.

    run checkDir(input ficFileName:screen-value, output cError).
    if cError <> '' then 
    do:
      message cError view-as alert-box info buttons ok.
      return. 
    end.

    run dumpData
      ( input pihBrowse                       /*   input  pihDdBrowse         as handle    */ 
      , input cbDumpType:screen-value         /*   input  picFormat           as character */ 
      , input cbiRecordSelection:screen-value /*   input  piiRecordSelection  as integer   */ 
      , input cbiFieldSelection:screen-value  /*   input  piiRecordSelection  as integer   */ 
      , input ficFileName:screen-value        /*   input  picFile             as character */ 
      ).

    cAction = getRegistry('DumpAndLoad','DumpReadyAction').
    do iAction = 1 to num-entries(cAction):
      case entry(iAction,cAction):
        when 'view'      then apply 'choose' to btnViewLastDump.
        when 'explore'   then apply 'choose' to btnOpenLastDumpDir.
        when 'clipboard' then run tbDumpReadyClipboard.
      end case.
    end.

    /* Do the close as last action */
    if can-do(cAction,"Close") then APPLY "CLOSE" TO this-procedure.
  end.

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

  define variable cDumpDir as character   no-undo.

  do with frame {&frame-name}:

    /* Strip filename, only keep path */
    cDumpDir = getRegistry("DumpAndLoad", "DumpLastFileName").
    if cDumpDir = ? then return. 

    entry(num-entries(cDumpDir,'\'),cDumpDir,'\') = ''.
    IF cDumpDir = "" THEN cDumpDir = ".". /* if blank go to DD dir */
    file-info:file-name = cDumpDir.

    if file-info:full-pathname <> ? then
      os-command no-wait explorer /n, /e, value(file-info:full-pathname).
    else
      message substitute("Last used dir '&1' not found.", cDumpDir)
        view-as alert-box info buttons ok title "Invalid Dir" .
  end.

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
  define variable     cCmd           as character  no-undo.
  define variable     cDumpType      as character  no-undo.

  cDumpType = getRegistry("DumpAndLoad", "DumpExportType" ).
  cCmd = getRegistry("DumpAndLoad", substitute("DumpFileViewCmd_&1",cDumpType) ).

  assign
    cCmd = substitute(cCmd, gcLastFile)
    cCmd = substitute('"&1"',cCmd)
    .

  publish "message" ( 50, substitute("DD FileViewCommand: &1",cCmd) ).

  os-command no-wait value(cCmd).

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
  
  do with frame {&frame-name}:

    gcLastFile = getRegistry("DumpAndLoad", "DumpLastFileName").
    
    if gcLastFile <> ? then
    do:
      file-info:filename = gcLastFile.
      gcLastFile = file-info:full-pathname.
    end.

    if gcLastFile <> ? then
      enable btnViewLastDump btnOpenLastDumpDir.
    else
      disable btnViewLastDump btnOpenLastDumpDir.

  end.

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
  
  do with frame {&frame-name}:
    
    assign cbCodePage.
    setRegistry("DumpAndLoad", "DumpCodePage", cbCodePage ).

  end.
  
end procedure. /* cbCodePageValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbDateFormatValueChanged wDump 
PROCEDURE cbDateFormatValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  do with frame {&frame-name}:
    
    assign cbDateFormat.
    setRegistry("DumpAndLoad", "DumpDateFormat", cbDateFormat ).

  end.
  
end procedure. /* cbDateFormatValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbDumpTypeValueChanged wDump 
PROCEDURE cbDumpTypeValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  do with frame {&frame-name}:
    
    assign cbDumpType.

    /* Save dump type */
    gcFileViewCmd = getRegistry("DumpAndLoad", substitute("DumpFileViewCmd_&1",cbDumpType) ).
    if gcFileViewCmd = ? then
    do:
      gcFileViewCmd = getRegistry("DumpAndLoad", "DumpFileViewCmd").
      setRegistry("DumpAndLoad", substitute("DumpFileViewCmd_&1",cbDumpType), gcFileViewCmd).
    end. /* if gcFileViewCmd = ? then */

    case cbDumpType:
      when "XML" then
      do:
        enable tbExportSchema.
        disable cbNumericFormat cbDateFormat.
        cbNumericFormat = 'American'.
        cbDateFormat = 'YMD'.

        if tbUseCustomizedFormats:sensitive then
          tbUseCustomizedFormats:checked = true.
      end.

      when "P" then
      do:
        disable cbNumericFormat cbDateFormat tbExportSchema.
        assign
          cbNumericFormat = 'American'
          cbDateFormat = 'MDY'.
      end.

      otherwise
      do:
        disable tbExportSchema.
        enable cbNumericFormat cbDateFormat.
      end.
    end case.

    /* For progress dump file and XML, use dictionary format by default
     * For others, use customized
     */
    if   cbDumpType = "XML" 
      or cbDumpType = "D" then
    do:
      /* But only if there has been changed something by the 
       * user, otherwise this is not relevant 
       */
      tbUseCustomizedFormats:sensitive = false. 
      tbUseCustomizedFormats:checked   = false.
    end.
    else 
    do:
      /* Find out whether the user has some customized fields */
      if can-find(first ttField where ttField.cFormat <> ttField.cFormatOrg) then
        assign tbUseCustomizedFormats:sensitive = true
               tbUseCustomizedFormats:checked   = true. 
      else 
        assign tbUseCustomizedFormats:sensitive = false
               tbUseCustomizedFormats:checked   = false. 
    end.


    /* replace extension of filename */
    if ficFileName = '' then
      run setDumpFileName.

    if num-entries(ficFileName,'.') > 1 then
      entry(num-entries(ficFileName,'.'),ficFileName,'.') = lower(cbDumpType).
    else 
      ficFileName = ficFileName + '.' + lower(cbDumpType).

    display ficFileName cbNumericFormat cbDateFormat.

  end.
  
  run tbExportSchemaValueChanged.
  run tbUseCustomizedFormatsValueChanged.


end procedure. /* cbDumpTypeValueChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE cbiFieldSelectionValueChanged wDump 
PROCEDURE cbiFieldSelectionValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  do with frame {&frame-name}:
    
    assign
      cbiFieldSelection
      .

  end.
  
  .run setDumpFileName. /* debug */

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
  
  do with frame {&frame-name}:
    
    assign cbiRecordSelection.

    if (   ( cbiRecordSelection = 2 and pihBrowse:query:num-results = 0) 
        or ( cbiRecordSelection = 3 and pihBrowse:num-selected-rows = 0) )
      and not glNoRecordsWarning then
    do:
      run showHelp('NoSelection','').
      glNoRecordsWarning = true.
    end.

  end.

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
  
  do with frame {&frame-name}:
    
    assign cbNumericFormat.
    setRegistry("DumpAndLoad", "DumpNumericFormat", cbNumericFormat ).

  end.
  
end procedure. /* cbNumericFormatValueChanged */

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

  define input parameter    pihDdBrowse         as handle      no-undo.
  define input parameter    picFormat           as character   no-undo.
  define input parameter    piiRecordSelection  as integer     no-undo.
  define input parameter    piiFieldSelection   as integer     no-undo.
  define input parameter    picFile             as character   no-undo.
  
  define variable cDumpDir            as character   no-undo.
  define variable iNumRecs            as integer     no-undo.
  define variable iCurField           as integer     no-undo.
  define variable cTtField            as character   no-undo.
  define variable cDbField            as character   no-undo.

  define variable hExportTT           as handle      no-undo.
  define variable hExportTtBuffer     as handle      no-undo.
  define variable hExportQuery        as handle      no-undo.
  define variable hExportQueryBuffer  as handle      no-undo.
  define variable cExportQueryString  as character   no-undo.

  define variable iCurSelectedRow     as integer     no-undo.
  define variable cStatus             as character   no-undo.
  define variable iCurIndex           as integer     no-undo.
  define variable cIndexInfo          as character   no-undo.
  define variable cIndexName          as character   no-undo.
  define variable cIndexFields        as character   no-undo.
  define variable iTimeStarted        as integer     no-undo.

  glAborted = false. 

  run setStatusMessage
    ( input now
    , input substitute
              ( "Dumping records from table &1.&2 in progres..."
              , gcDb
              , gcTable
              )
    ).

  /* Construct the Query-string... */
  case piiRecordSelection:
    when 1 then cExportQueryString = substitute("for each &1.&2 no-lock", gcDb, gcTable ).
    when 2 then cExportQueryString = pihDdBrowse:query:prepare-string.
  end case. /* case piiRecordSelection: */
  
  /* Create temptable-handle... */
  create temp-table hExportTt.
  
  /* Add fields & indexes to TempTable... */
  case piiFieldSelection:
    
    /* Add all fields & indexes from the db-table... */
    when 1 then 
    do:
      hExportTt:create-like(substitute("&1.&2",gcDb,gcTable)).
    end. /* when 1 then  */


    /* Add selected fields & some indexes from the db-table... */
    when 2 then
    do:
      /* add selected fields to the temp-table layout */
      do iCurField = 1 to num-entries(picSelectedFields):

        assign
          cTtField = entry(iCurField,picSelectedFields)
          cDbField = substitute("&1.&2.&3", gcDB, gcTable, cTtField)
          .

        /* skip ROWID and RECID fields as they don't exist in the table */
        IF LOOKUP(cTtField,"ROWID,RECID") > 0 THEN NEXT.
        
        hExportTt:add-like-field(cTtField,cDbField).

      end. /* do iCurField = 1 to num-entries(picSelectedFields): */
      
      /* add all indexes to the temp-table layout which consists of selected fields  */
      create buffer hExportQueryBuffer for table substitute("&1.&2",gcDb,gcTable).

      iCurIndex = 0.
      do while true:

        assign
          iCurIndex  = iCurIndex + 1
          cIndexInfo = hExportQueryBuffer:index-information(iCurIndex)
          .

        if cIndexInfo = ? then
          leave.

        assign
          cIndexName   = entry(1,cIndexInfo)
          cIndexFields = getFieldListFromIndexInformation(input cIndexInfo) 
          .

        if multipleLookUpGreaterThanZero
            ( input cIndexFields
            , input picSelectedFields
            ) 
        then
          hExportTt:add-like-index(cIndexName,cIndexName,substitute("&1.&2", gcDB, gcTable)).

      end. /* do while true: */

      delete object hExportQueryBuffer.
    end. /* when 2 then */
  end. /* case piiFieldSelection: */

  /* Prepare the TempTable... */
  hExportTt:temp-table-prepare(substitute("&1",gcTable)).
  hExportTtBuffer = hExportTt:default-buffer-handle.

  /* Populate the TempTable... */
  case piiRecordSelection:

    /* All records from table(1) or browse(2) */
    when 1 or when 2 then
    do:
      create buffer hExportQueryBuffer for table substitute("&1.&2",gcDb,gcTable).
      create query hExportQuery.
      hExportQuery:set-buffers(hExportQueryBuffer).
      hExportQuery:query-prepare(cExportQueryString).

      hExportQuery:query-open(). 
      repeat:    
        hExportQuery:get-next().    
        if hExportQuery:query-off-end then 
          leave.

        assign iNumRecs = iNumRecs + 1.
        if (etime - iTimeStarted) > 1000 then
        do:
          iTimeStarted = etime.
          run showProgressBar(substitute('Collected &1 records',iNumRecs), 0).
          PROCESS EVENTS.
          if glAborted then leave. 
        end.

        hExportTtBuffer:buffer-create.
        hExportTtBuffer:buffer-copy(hExportQuery:get-buffer-handle(1)).
      end.
      hExportQuery:query-close().

    end. /* when 1 or when 2 then */
    
    /* Records from the selection */
    when 3 then
    do:
      do iCurSelectedRow = 1 to pihDdBrowse:num-selected-rows:
        pihDdBrowse:fetch-selected-row(iCurSelectedRow).
        hExportTtBuffer:buffer-create.
        hExportTtBuffer:buffer-copy(pihDdBrowse:query:get-buffer-handle()).

        assign iNumRecs = iNumRecs + 1.
        if (etime - iTimeStarted) > 1000 then
        do:
          iTimeStarted = etime.
          run showProgressBar(substitute('Collected &1 records',iNumRecs), 0).
          PROCESS EVENTS.
          if glAborted then leave. 
        end.

      end. /* when 3 then */
    end.
  end. /* case piiRecordSelection: */

  /* Dump the TempTable... */
  session:numeric-format = cbNumericFormat.
  session:date-format = cbDateFormat.

  case picFormat:

    when "D" then 
      run DumpDataProgressD
            ( input  picFile
            , input  hExportTt
            , input  iNumRecs
            , input  cbCodePage
            ).

    when "HTML" then 
      run DumpDataHtml
            ( input  picFile
            , input  hExportTt
            , input  iNumRecs
            , input  cbCodePage
            ).

    when "TXT" then 
      run DumpDataTxt
            ( input  picFile
            , input  hExportTt
            , input  iNumRecs
            , input  cbCodePage
            ).

    when "XLS" then 
      run DumpDataExcel
            ( input  picFile
            , input  hExportTt
            , input  iNumRecs
            , input  cbCodePage
            ).

    when "XML" then 
      run dumpDataXml
            ( input  picFile
            , input  hExportTt
            , input  iNumRecs
            , input  cbCodePage
            ).

    when "P" then 
      run dumpData4GL
            ( input  picFile
            , input  hExportTt
            , input  iNumRecs
            , input  cbCodePage
            ).

  end case. /* case picFormat: */
  
  session:numeric-format = gcSessionNumericFormat.
  session:date-format = gcSessionDateFormat.

  /* Clean up */
  if valid-handle(hExportQueryBuffer) then delete object hExportQueryBuffer.
  if valid-handle(hExportQuery      ) then delete object hExportQuery.
  if valid-handle(hExportTt         ) then delete object hExportTt.

  do with frame {&frame-name}:
    
    if glAborted then 
    do:
      cStatus = substitute( "Dumping table &1 aborted", gcTable ).
      run showHelp('DumpAborted', gcTable).
    end.
    else 
      cStatus = substitute( "&1 records dumped from table &2.&3 to &4"
                          , iNumRecs
                          , gcDb
                          , gcTable
                          , picFile
                          ).

    run setStatusMessage( input now, input cStatus ).

    assign
      cDumpDir = substring(picFile, 1, r-index(picFile,"~\"))
      .

    setRegistry( "DumpAndLoad", "DumpExportType"      , cbDumpType).
    setRegistry( "DumpAndLoad", "DumpFilter"          , string(cbiRecordSelection) ).
    setRegistry( "DumpAndLoad", "DumpFilterFields"    , string(cbiFieldSelection) ).
    setRegistry( "DumpAndLoad", "DumpXmlSchema"       , string(tbExportSchema) ).
    setRegistry( "DumpAndLoad", "DumpMinimalXmlSchema", string(tbMinimalSchema) ).
    setRegistry( "DumpAndLoad", "DumpLastFileName"    , picFile ).
    setRegistry( "DumpAndLoad", "DumpActionTimeStamp" , ficMessageNow ).
    setRegistry( "DumpAndLoad", "DumpActionResult"    , ficMessage ).

    gcLastFile = picFile.
  end. /* do with frame {&frame-name}: */

  run btnViewLastDumpEnable.

end procedure. /* dumpData */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpData4GL wDump 
PROCEDURE DumpData4GL :
/*------------------------------------------------------------------------------
  Purpose:     Dump Data as 4GL code
  2012-09-14 JEE Created
------------------------------------------------------------------------------*/

  define input parameter picFileName  as character   no-undo.
  define input parameter pihTempTable as handle      no-undo.
  define input parameter piNumRecords as integer     no-undo.
  define input parameter pcCodePage   as character   no-undo.

  define variable hField              as handle      no-undo.
  define variable hTTBuffer           as handle      no-undo.
  define variable hQuery              as handle      no-undo.
  define variable iCurField           as integer     no-undo.
  define variable iNumRecords         as integer     no-undo.
  define variable iMaxLength          as integer     no-undo.
  define variable iNumFields          as integer     no-undo.
  define variable iTimeStarted        as integer     no-undo.
  define variable iLength             as integer     no-undo.
  define variable iExtent             as integer     no-undo.
  define variable cCodePage           as character   no-undo.
  define variable cKeyFields          as character   no-undo.
  define variable cLine               as character   no-undo.
  define variable iExtBegin           as integer     no-undo.
  define variable iExtEnd             as integer     no-undo.
  define variable cFieldName          as character   no-undo.
  define variable cFieldNameFormat    as character   no-undo.
  define variable cFieldValue         as character   no-undo.
  
  ASSIGN
    cCodePage    = (if pcCodePage <> "" then pcCodePage else session:cpstream)
    iTimeStarted = etime
    hTTBuffer    = pihTempTable:default-buffer-handle
    iNumRecords = 0
    cKeyFields   = getIndexFields(gcDB, gcTable, "P,U")
    .

  /* Open outputfile */
  output stream strDump to value(picFileName) convert target cCodePage.
  put stream strDump unformatted 
         substitute("/* Generated procedure for &1.&2 ", gcDB, gcTable)
    skip substitute(" * Date: &1 ", string(now,"99-99-9999 HH:MM"))
    skip substitute(" * By  : &1 ", getUsername() )
    skip substitute(" */" ) 
    skip substitute("define buffer bData for &1.&2.", gcDB, gcTable)
    skip substitute(" ")
    .

  /* Build query */
  create query hQuery.
  hQuery:set-buffers(hTTBuffer).
  hQuery:query-prepare(substitute("for each &1 no-lock", hTTBuffer:name)).
  hQuery:query-open().


  /* Pump the table data into the table */
  pumpDataLoop:
  repeat:
    hQuery:get-next().
    if hQuery:query-off-end then leave pumpDataLoop.

    if (etime - iTimeStarted) > 1000 then
    do:
      iTimeStarted = etime.
      run showProgressBar("Dumping records, please wait", iNumRecords / piNumRecords * 100).
      PROCESS EVENTS.
      if glAborted then leave pumpDataLoop. 
    end.

    iNumRecords = iNumRecords + 1.

    put stream strDump unformatted 
      skip "find bData".

    /* Keyfields and calculation of name length */
    iNumFields = 0.
    do iCurField = 1 to hTTBuffer:num-fields:
      hField = hTTBuffer:buffer-field(iCurField).
      iExtBegin = (if hField:extent = 0 then 0 else 1).
      iExtEnd   = maximum(hField:extent, 0).

      do iExtent = iExtBegin to iExtEnd:
        /* Calculate the length of this field name */
        cFieldName = substitute("&1&2",hField:name, if iExtent > 0 then substitute("[&1]",iExtent) else "").
        iMaxLength = maximum(iMaxLength,length(cFieldName)).

        if lookup(hField:name, cKeyfields) > 0 or cKeyfields = "" then
        do:
            iNumFields = iNumFields + 1.
            cFieldValue = getFieldValue(hField,iExtent).

            put stream strDump unformatted skip 
              substitute("  &1 bData.&2 = &3"
                          , (if iNumFields = 1 then "where" else "  and")
                          , cFieldName
                          , cFieldValue
                          ).
        end.
      end.
    end.

    /* Calculate the format for the fields to allign then nicely */
    cFieldNameFormat = substitute("X(&1)",iMaxLength).

    put stream strDump unformatted 
      skip "        exclusive-lock no-error."
      skip " "
      skip "if not available bData then"
      skip "  create bData."
      skip " ".

    /* data */
    iNumFields = 0.
    do iCurField = 1 to hTTBuffer:num-fields:
      hField    = hTTBuffer:buffer-field(iCurField).
      iExtBegin = (if hField:extent = 0 then 0 else 1).
      iExtEnd   = maximum(hField:extent, 0).
      if lookup(hField:data-type, "raw,clob,blob") > 0 then next.

      do iExtent = iExtBegin to iExtEnd:
        /* Place an assign statement every 100 fields */
        if iNumFields modulo 100 = 0 then
        do:
          /* Closing dot for previouse assign */
          if iNumFields > 0 then put stream strDump unformatted skip substitute("  ."). 
          /* New assign */
          put stream strDump unformatted skip substitute("assign").
        end.

        cFieldName = substitute("&1&2",hField:name, if iExtent > 0 then substitute("[&1]",iExtent) else "").
        iNumFields = iNumFields + 1.
        cFieldValue = getFieldValue(hField,iExtent).

        put stream strDump unformatted 
          skip substitute("  bData.&1 = &2"
                          , string(cFieldName,cFieldNameFormat)
                          , cFieldValue
                          ).
      end.
    end.
    put stream strDump unformatted skip "  ." skip(2).

  end. /* pumpDataLoop */

  output stream strDump close.

  /* Hide progress bar frame */
  run showProgressBar("", ?).

  delete object hQuery.

end procedure. /* DumpData4GL */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataExcel wDump 
PROCEDURE DumpDataExcel :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  define input parameter picFileName  as character   no-undo.
  define input parameter pihTempTable as handle      no-undo.
  define input parameter piNumRecords as integer     no-undo.
  define input parameter pcCodePage   as character   no-undo.

  define variable cColumnRange        as character   no-undo.
  define variable hExcel              as com-handle  no-undo.
  define variable hWorkbook           as com-handle  no-undo.
  define variable hWorksheet          as com-handle  no-undo.

  /* First, dump the file as HTML */
  run DumpDataHtml
        ( input picFileName 
        , input pihTempTable
        , input piNumRecords
        , input pcCodePage
        ).

  if glAborted then return. 

  file-info:file-name = picFileName.

  /* Open Excel and initialize variables */
  create "Excel.Application" hExcel.
  assign
    hExcel:visible = false
    hWorkbook      = hExcel:Workbooks:open(file-info:full-pathname)
    hWorkSheet     = hExcel:Sheets:item(1)
    .
  
  /* Adjust column sizes */
  hExcel:columns("A:ZZ"):select.
  hExcel:selection:columns:Autofit.

  /* Set first row as title row with autofilter */
  hWorksheet:Range("A1:A1"):Select.
  hWorkbook:Windows(1):SplitColumn = 0.
  hWorkbook:Windows(1):SplitRow    = 1.
  hWorkbook:Windows(1):FreezePanes = True.
  hWorksheet:Range("A1:A1"):AutoFilter(1,?,?). 

  /* Perform housekeeping and cleanup steps */
  hExcel:DisplayAlerts = False.  /* don't show confirmation dialog from excel */
  hWorkbook:SaveAs(picFileName,1,?,?,?,?,?).
  hExcel:application:Workbooks:close() no-error.
  hExcel:application:quit no-error.
  
  release object hWorksheet.
  release object hWorkbook.
  release object hExcel.

end procedure. /* DumpDataExcel */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataHtml wDump 
PROCEDURE DumpDataHtml :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  define input parameter picFileName  as character   no-undo.
  define input parameter pihTempTable as handle      no-undo.
  define input parameter piNumRecords as integer     no-undo.
  define input parameter pcCodePage   as character   no-undo.

  define variable hField              as handle      no-undo.
  define variable hTTBuffer           as handle      no-undo.
  define variable hQuery              as handle      no-undo.
  define variable iCurField           as integer     no-undo.
  define variable iNrOfRecords        as integer     no-undo.
  define variable iExtent             as integer     no-undo.
  define variable iTimeStarted        as integer     no-undo.
  define variable cCodePage           as character   no-undo. 
  define variable cData               as character   no-undo. 

  assign
    iTimeStarted = etime
    hTTBuffer    = pihTempTable:default-buffer-handle
    iNrOfRecords = 0
    cCodePage    = (if pcCodePage <> "" then pcCodePage else session:cpstream)
    .

  /* Open outputfile */
  output stream strDump to value(picFileName) convert target cCodePage.
  put stream strDump unformatted 
    '<html><body><table border="0"><tr bgcolor="KHAKI">'.

  /* Pump field names as column headers*/
  do iCurField = 1 TO hTTBuffer:num-fields:
    hField = hTTBuffer:buffer-field(iCurField).
    if lookup(hField:data-type,'clob,blob,raw') > 0 then next. 

    if hField:extent > 1 then
    do iExtent = 1 to hField:extent:
      put stream strDump unformatted
        skip substitute('<th>&1[&2]</th>', hField:label, iExtent).
    end.
    else
    do:
      put stream strDump unformatted
        skip substitute('<th>&1</th>', hField:label).
    end.
  end.

  put stream strDump unformatted '</tr>'.
  
  /* Build query */
  create query hQuery.
  hQuery:set-buffers(hTTBuffer).
  hQuery:query-prepare( substitute( "for each &1 no-lock", hTTBuffer:name)).
  hQuery:query-open().

  /* Pump the table data into the table */
  pumpDataLoop:
  repeat:
    hQuery:get-next().
    if hQuery:query-off-end then leave pumpDataLoop.

    if (etime - iTimeStarted) > 1000 then
    do:
      iTimeStarted = etime.
      run showProgressBar( 'Dumping records, please wait', iNrOfRecords / piNumRecords * 100 ).
      PROCESS EVENTS.
      if glAborted then leave pumpDataLoop. 
    end.

    put stream strDump unformatted skip 
      substitute('<tr bgcolor="&1">', trim(string(iNrOfRecords mod 2 = 1,'WHITE/LIGHTYELLOW')) ).

    iNrOfRecords = iNrOfRecords + 1.
    do iCurField = 1 to hTTBuffer:num-fields:
      hField = hTTBuffer:buffer-field(iCurField).
      if lookup(hField:data-type,'clob,blob,raw') > 0 then next. 

      /* Find out format of field */
      find ttField where ttField.cFullName = hField:name.

      if hField:extent > 1 then
      do iExtent = 1 to hField:extent:

        cData = if tbUseCustomizedFormats 
                  then trim(string(hField:buffer-value(iExtent),ttField.cFormat))
                  else hField:buffer-value(iExtent).

        cData = getEscapedData("HTML", cData).

        put stream strDump unformatted 
          skip substitute('<td>&1</td>', cData).
      end.
      else
      do:
        cData = if tbUseCustomizedFormats 
                  then trim(string(hField:buffer-value,ttField.cFormat))
                  else hField:buffer-value.

        cData = getEscapedData("HTML", cData).

        put stream strDump unformatted 
          skip substitute('<td>&1</td>', cData).
      end.
    end. 
    put stream strDump unformatted '</tr>'.

  end. /* pumpDataLoop */
   
  put stream strDump unformatted '</table></body></html>'.
  output stream strDump close.
  
  /* Hide progress bar frame */
  run showProgressBar('',?).
  
  delete object hQuery.

end procedure. /* DumpDataExcel */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataProgressD wDump 
PROCEDURE DumpDataProgressD :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  define input parameter picFileName  as character   no-undo.
  define input parameter pihTempTable as handle      no-undo.
  define input parameter piNumRecords as integer     no-undo.
  define input parameter pcCodePage   as character   no-undo.

  define variable cField              as character   no-undo.
  define variable cTimeStamp          as character   no-undo.
  define variable hQuery              as handle      no-undo.
  define variable hTTBuffer           as handle      no-undo.
  define variable iBack               as integer     no-undo.
  define variable iCurField           as integer     no-undo.
  define variable iExtent             as integer     no-undo.
  define variable iNrOfRecords        as integer     no-undo.
  define variable iTimeStarted        as integer     no-undo.
  define variable iTrailer            as integer     no-undo.
  define variable lFirstFieldOfRecord as logical     no-undo.
  define variable cCodePage           as character   no-undo. 

  assign
    cCodePage    = (if pcCodePage <> "" then pcCodePage else session:cpstream)
    iTimeStarted = etime
    hTTBuffer    = pihTempTable:default-buffer-handle
    iNrOfRecords = 0
    cTimeStamp   = string(year( today),"9999":u) + "/":u
                 + string(month(today),"99":u  ) + "/":u
                 + string(day(  today),"99":u  ) + "-":u
                 + string(time,"HH:MM:SS":u).

  create query hQuery.
  hQuery:set-buffers(hTTBuffer).
  hQuery:query-prepare( substitute( "for each &1 no-lock", hTTBuffer:name)).
  hQuery:query-open().
  
  /* Open outputfile */
  output stream strDump to value(picFileName) no-echo no-map convert target cCodePage.
  export stream strDump ?.
  iBack = seek(strDump) - 1.
  seek stream strDump to 0.    
  
  pumpDataLoop:
  repeat:
    hQuery:get-next().
    if hQuery:query-off-end then leave pumpDataLoop.

    assign 
      iNrOfRecords        = iNrOfRecords + 1
      lFirstFieldOfRecord = true
      .

    if (etime - iTimeStarted) > 1000 then
    do:
      iTimeStarted = etime.
      run showProgressBar( 'Dumping records, please wait', iNrOfRecords / piNumRecords * 100 ).
      PROCESS EVENTS.
      if glAborted then leave pumpDataLoop. 
    end.

    do iCurField = 1 to hTTBuffer:num-fields:

      if cbiFieldSelection = 2 then
      do:
        cField = hTTBuffer:buffer-field(iCurField):name.
        if lookup(cField,picSelectedFields) = 0 then next.
      end.

      if lFirstFieldOfRecord then
        lFirstFieldOfRecord = false.
      else
      do:
        seek stream strDump to seek(strDump) - iBack.
        put stream strDump control ' ':u.
      end.

      if hTTBuffer:buffer-field(iCurField):extent > 1 then
      do iExtent = 1 to hTTBuffer:buffer-field(iCurField):extent:
        
        if iExtent > 1 then
        do:
          seek stream strDump to seek(strDump) - iBack.
          put stream strDump control ' ':u.
        end.

        export stream strDump hTTBuffer:buffer-field(iCurField):buffer-value(iExtent).
      end.
      else
        export stream strDump hTTBuffer:buffer-field(iCurField):buffer-value.

    end. /* Fields */
  end. /* while hTTBuffer:available */
  
  hQuery:query-close().
  delete object hQuery.
  hQuery = ?.
  
  if not glAborted then
  do:
    put stream strDump unformatted ".":u skip.
    iTrailer = seek(strDump).
    
    put stream strDump unformatted
           "PSC":u 
      skip "filename=":u   substring(hTTBuffer:table,index(hTTBuffer:table,"_") + 1) 
      skip "records=":u    string(iNrOfRecords,"9999999999999":u) 
      skip "ldbname=":u    entry(1,hTTBuffer:table,"_")
      skip "timestamp=":u  cTimeStamp 
      skip "numformat=":u  asc(session:numeric-separator) ",":u asc(session:numeric-decimal-point) 
      skip "dateformat=":u session:date-format "-":u session:year-offset 
      skip "map=NO-MAP":u 
      skip "cpstream=":u   cCodePage 
      skip ".":u 
      skip string(iTrailer,"9999999999":u) 
      skip.
  end.
  
  output stream strDump close.
  
  /* Hide progress bar frame */
  run showProgressBar('',?).

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

  define input parameter picFileName  as character   no-undo.
  define input parameter pihTempTable as handle      no-undo.
  define input parameter piNumRecords as integer     no-undo.
  define input parameter pcCodePage   as character   no-undo.

  define variable cFieldFormat        as character   no-undo. 
  define variable cDumpFormat         as character   no-undo. 
  define variable cName               as character   no-undo. 
  define variable hField              as handle      no-undo.
  define variable hTTBuffer           as handle      no-undo.
  define variable hQuery              as handle      no-undo.
  define variable iCurField           as integer     no-undo.
  define variable iField              as integer     no-undo.
  define variable iNrOfRecords        as integer     no-undo.
  define variable iExtent             as integer     no-undo.
  define variable iTimeStarted        as integer     no-undo.
  define variable iLength             as integer     no-undo.
  define variable cDumpFormatList     as character   no-undo.
  define variable cFieldFormatList    as character   no-undo.
  define variable cCodePage           as character   no-undo. 

  assign
    cCodePage    = (if pcCodePage <> "" then pcCodePage else session:cpstream)
    iTimeStarted = etime
    hTTBuffer    = pihTempTable:default-buffer-handle
    iNrOfRecords = 0
    .

  /* Open outputfile */
  output stream strDump to value(picFileName) convert target cCodePage.

  /* Pump field names as column headers*/
  iField = 0.
  do iCurField = 1 TO hTTBuffer:num-fields:
    hField = hTTBuffer:buffer-field(iCurField).
    if lookup(hField:data-type,'raw,clob,blob') > 0 then next.
    iField = iField + 1.
    
    /* Take the larger one of name and actual format */
    cName = (if hField:extent > 1 
               then substitute('&1[&2]',hField:name, hField:extent) 
               else hField:label ).

    /* Find out format of field */
    find ttField where ttField.cFullName = hField:name.
    if tbUseCustomizedFormats then cFieldFormat = ttField.cFormat.
                              else cFieldFormat = ttField.cFormatOrg.
    
    /* What is the largest? Take at least length 10 for date fields */
    iLength = maximum( length( string(hField:initial, cFieldFormat) )
                     , length( cName )).
    if hField:data-type = 'date' then iLength = maximum(iLength,10).
    cDumpFormat = substitute('X(&1)', iLength).

    /* Save for future use */
    cDumpFormatList  = cDumpFormatList  + cDumpFormat  + chr(1).
    cFieldFormatList = cFieldFormatList + cFieldFormat + chr(1).


    if hField:extent > 1 then
    do iExtent = 1 to hField:extent:
      put stream strDump unformatted 
        string(substitute('&1[&2]',hField:label, iExtent),cDumpFormat) ' '.
    end.
    else
    do:
      put stream strDump unformatted
        string(hField:label,cDumpFormat) ' '.
    end.
  end.
  put stream strDump unformatted skip.

  
  /* And nice lines below them please */
  iField = 0.
  do iCurField = 1 TO hTTBuffer:num-fields:
    hField = hTTBuffer:buffer-field(iCurField).
    if lookup(hField:data-type,'raw,clob,blob') > 0 then next.
    iField = iField + 1.

    cFieldFormat = entry(iField,cFieldFormatList,chr(1)).
    cDumpFormat  = entry(iField,cDumpFormatList, chr(1)).

    if hField:extent > 1 then
    do iExtent = 1 to hField:extent:
      put stream strDump unformatted 
        string(fill('-', 1000),cDumpFormat) ' '.
    end.
    else
    do:
      put stream strDump unformatted
        string(fill('-', 1000),cDumpFormat) ' '.
    end.
  end. 
  put stream strDump unformatted skip.

  /* Build query */
  create query hQuery.
  hQuery:set-buffers(hTTBuffer).
  hQuery:query-prepare( substitute( "for each &1 no-lock", hTTBuffer:name)).
  hQuery:query-open().

  /* Pump the table data into the table */
  pumpDataLoop:
  repeat:
    hQuery:get-next().
    if hQuery:query-off-end then leave pumpDataLoop.

    if (etime - iTimeStarted) > 1000 then
    do:
      iTimeStarted = etime.
      run showProgressBar( 'Dumping records, please wait', iNrOfRecords / piNumRecords * 100 ).
      PROCESS EVENTS.
      if glAborted then leave pumpDataLoop. 
    end.

    iNrOfRecords = iNrOfRecords + 1.
    iField = 0.
    do iCurField = 1 to hTTBuffer:num-fields:
      hField = hTTBuffer:buffer-field(iCurField).
      if lookup(hField:data-type,'raw,clob,blob') > 0 then next.
      iField = iField + 1.

      cFieldFormat = entry(iField,cFieldFormatList,chr(1)).
      cDumpFormat  = entry(iField,cDumpFormatList, chr(1)).

      if hField:extent > 1 then
      do iExtent = 1 to hField:extent:
        put stream strDump unformatted 
          string( substitute('&1', string(hField:buffer-value(iExtent), cFieldFormat)), cDumpFormat ) ' '.
      end.
      else
      do:
        put stream strDump unformatted 
          string( substitute('&1', string(hField:buffer-value, cFieldFormat)), cDumpFormat ) ' '.
      end.
    end. 

    put stream strDump unformatted skip.

  end. /* pumpDataLoop */
   
  output stream strDump close.
  
  /* Hide progress bar frame */
  run showProgressBar('',?).
  
  delete object hQuery.

end procedure. /* DumpDataText */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DumpDataXml wDump 
PROCEDURE DumpDataXml :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  define input parameter picFileName  as character   no-undo.
  define input parameter pihTempTable as handle      no-undo.
  define input parameter piNumRecords as integer     no-undo.
  define input parameter pcCodePage   as character   no-undo.

  define variable cTargetType      as character   no-undo.
  define variable cFile            as character   no-undo.
  define variable lFormatted       as logical     no-undo.
  define variable cEncoding        as character   no-undo.
  define variable cSchemaLocation  as character   no-undo.
  define variable lWriteSchema     as logical     no-undo.
  define variable lMinSchema       as logical     no-undo.
  
  do with frame {&frame-name}:

    run showProgressBar( substitute('Dumping &1 records, please wait', piNumRecords) , 0).

    assign     
      cTargetType     = "file"     
      cFile           = picFileName
      lFormatted      = yes     
      cEncoding       = ?     
      cSchemaLocation = ?     
      lWriteSchema    = tbExportSchema:checked
      lMinSchema      = tbMinimalSchema:checked
      .
  
    pihTempTable:write-xml
      ( cTargetType
      , cFile
      , lFormatted
      , cEncoding
      , cSchemaLocation
      , lWriteSchema
      , lMinSchema
      ).
  
    run showProgressBar( '', ? ).
  end. /* do with frame {&frame-name}: */

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
  DISPLAY ficFileName cbDumpType cbCodePage cbiRecordSelection cbNumericFormat 
          tbUseCustomizedFormats tbExportSchema cbiFieldSelection cbDateFormat 
          tbMinimalSchema ficMessageNow ficMessage tbDumpReadyClose 
          tbDumpReadyExplore tbDumpReadyView tbDumpReadyClipboard 
      WITH FRAME DEFAULT-FRAME IN WINDOW wDump.
  ENABLE RECT-2 RECT-3 RECT-4 ficFileName btnChooseDumpFile cbDumpType 
         cbCodePage cbiRecordSelection cbNumericFormat tbUseCustomizedFormats 
         cbiFieldSelection cbDateFormat btnDump btnClose tbDumpReadyClose 
         tbDumpReadyExplore tbDumpReadyView tbDumpReadyClipboard 
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
  define variable cFileViewCmd     as character  no-undo.
  define variable cDumpReadyAction as character  no-undo.

  hide frame infoFrame.

  do with frame {&frame-name}:

    /* Get fonts */
    frame {&frame-name}:font = getFont('Default').
    FRAME infoFrame:font = getFont('Default').

    btnViewLastDump:load-image(getImagePath('View.gif')).
    btnOpenLastDumpDir:load-image(getImagePath('OpenFolder.gif')).

    assign
      gcDb    = pihBrowse:query:get-buffer-handle(1):dbname
      gcTable = pihBrowse:query:get-buffer-handle(1):table
      .

    run setStatusMessage
      ( input datetime(getRegistry("DumpAndLoad", "DumpActionTimeStamp"))
      , input getRegistry("DumpAndLoad", "DumpActionResult")
      ).

    cFileViewCmd = getRegistry("DumpAndLoad", "DumpFileViewCmd").
    if cFileViewCmd = ? then
    do:
      assign cFileViewCmd = "Start &1".
      setRegistry("DumpAndLoad", "DumpFileViewCmd", cFileViewCmd ).
    end.
    
    cbDumpType = getRegistry("DumpAndLoad", "DumpExportType" ).
    if cbDumpType = ? then cbDumpType = "XML".

    cbiRecordSelection = integer( getRegistry( "DumpAndLoad", "DumpFilter")).
    if cbiRecordSelection = ? then cbiRecordSelection = 2.

    cbiFieldSelection = integer( getRegistry( "DumpAndLoad", "DumpFilterFields")).
    if cbiFieldSelection = ? then cbiFieldSelection = 1.

    tbExportSchema = logical( getRegistry( "DumpAndLoad", "DumpXmlSchema")).
    if tbExportSchema = ? then tbExportSchema = true.

    tbMinimalSchema = logical( getRegistry( "DumpAndLoad", "DumpMinimalXmlSchema")).
    if tbMinimalSchema = ? then tbMinimalSchema = false.
    
    /* Set current setting for codepage */
    cbCodePage = session:cpstream.

    /* Add current setting to combo for numeric format */
    gcSessionNumericFormat = session:numeric-format.
    cbNumericFormat:delimiter = '|'.
    cbNumericFormat:list-item-pairs = substitute('Session (&1)|&1|American    (1,003.14)|American|European   (1.003,14)|European', session:numeric-format).
    cbNumericFormat = getRegistry("DumpAndLoad", "DumpNumericFormat" ).
    if cbNumericFormat = ? then cbNumericFormat = entry(2,cbNumericFormat:list-item-pairs,'|').

    /* Add current setting to combo for date format */
    gcSessionDateFormat = session:date-format.
    cbDateFormat:list-item-pairs = substitute('Session (&1),&1,DMY (18-03-2010),DMY,MDY (03-18-2010),MDY,YMD (2010-03-18),YMD,YDM (2010-18-03),YDM,MYD (03-2010-18),MYD,DYM (18-2010-03),DYM', session:date-format).
    cbDateFormat = getRegistry("DumpAndLoad", "DumpDateFormat" ).
    if cbDateFormat = ? then cbDateFormat = entry(2,cbDateFormat:list-item-pairs).

    /* DumpReadyActions */
    cDumpReadyAction = getRegistry('DumpAndLoad','DumpReadyAction').
    if cDumpReadyAction <> ? then
      assign
        tbDumpReadyClose    :checked = lookup('Close'    , cDumpReadyAction ) > 0
        tbDumpReadyExplore  :checked = lookup('Explore'  , cDumpReadyAction ) > 0
        tbDumpReadyView     :checked = lookup('View'     , cDumpReadyAction ) > 0
        tbDumpReadyClipboard:checked = lookup('Clipboard', cDumpReadyAction ) > 0
        .

    display 
      cbDumpType 
      cbiRecordSelection 
      cbiFieldSelection 
      tbExportSchema 
      tbMinimalSchema 
      cbCodePage
      cbNumericFormat
      cbDateFormat
      ficMessageNow 
      ficMessage.

  end.
  
  run setDumpFileName.
  run cbDumpTypeValueChanged.
  run cbiRecordSelectionValueChanged.

  /* For some reasons, these #*$&# scrollbars keep coming back */
  run showScrollBars(frame {&frame-name}:handle, no, no). /* KILL KILL KILL */

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
  
  define variable cError    as character   no-undo.
  define variable cFileName as character   no-undo.

  do with frame {&frame-name}:

    assign cbDumpType.

    run getDumpFileName
      ( input 'Dump'
      , input gcDB       
      , input gcTable    
      , input cbDumpType
      , input ""
      , output cFileName
      ).
  
    ficFileName = cFileName.
    display ficFileName.
  end.

  run btnViewLastDumpEnable.

end procedure. /* setDumpFileName */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setStatusMessage wDump 
PROCEDURE setStatusMessage :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  define input parameter pidtTimeStamp as datetime   no-undo.
  define input parameter picMessage    as character  no-undo.

  do with frame {&frame-name}:

    if pidtTimeStamp = ? then
      ficMessageNow = ''.
    else 
      ficMessageNow = string(pidtTimeStamp,"99-99-9999 HH:MM:SS").

    if picMessage = ? then
      ficMessage = "".
    else 
      ficMessage = picMessage.

    display ficMessageNow ficMessage.

  end. /* do with frame {&frame-name}: */

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
  
  define input  parameter pcInfoText as character   no-undo.
  define input  parameter piPrcDone  as integer     no-undo.

  define variable iNewWidth as integer     no-undo.
  
  process events.
  
  if piPrcDone = ? then
  do:
    frame infoFrame:visible = no.
  end.
  else 
  do:
    view frame infoFrame.
    enable btAbort with frame infoFrame. 
    fcInfoLine:screen-value = pcInfoText.

    iNewWidth = (minimum(100,piPrcDone) / 100) * rcBorder:width-pixels.
    rcBody:visible = (iNewWidth > 0).
    if iNewWidth > 0 then 
      rcBody:width-pixels = iNewWidth.

    process events.
  end.

end procedure. /* showProgressBar */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE tbDumpReadyClipboard wDump 
PROCEDURE tbDumpReadyClipboard :
/*------------------------------------------------------------------------------
  Purpose:     Put last dumpfile in clipboard
  2012-09-14 JEE Created
------------------------------------------------------------------------------*/

  define variable cLine               as character   no-undo.
  define variable iLength             as integer     no-undo.
  define variable lTooMuch            as logical     no-undo.

  output to "clipboard".
  input stream strDump from value(gcLastFile).
  repeat:
    import stream strDump unformatted cLine.
    if cLine <> ? then
    do:
      put unformatted cLine skip.
      iLength = iLength + length(cLine).
    end.
    if iLength > 65535 then
    do:
      lTooMuch = true.
      leave.
    end.
  end.
  input stream strDump close.
  output close.

  if lTooMuch then
    message "Too much data for clipboard. Data has been put on clipboard only partially!" view-as alert-box warning.

end procedure.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE tbExportSchemaValueChanged wDump 
PROCEDURE tbExportSchemaValueChanged :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  
  do with frame {&frame-name}:
    
    assign tbExportSchema.

    if cbDumpType = "XML" and tbExportSchema:checked then
    do:
      enable tbMinimalSchema.
    end.
    else
    do:
      disable tbMinimalSchema.
    end.
  end.

  run tbMinimalSchemaValueChanged.

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
  do with frame {&frame-name}:
    
    assign tbMinimalSchema.

  end.

  .run setDumpFileName. /* debug */

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
  do with frame {&frame-name}:
    
    assign tbUseCustomizedFormats.

  end.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* ************************  Function Implementations ***************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getEscapedData wDump 
FUNCTION getEscapedData RETURNS CHARACTER
  ( pcTarget as character
  , pcString as character ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/
  define variable cOutput as character no-undo.
  define variable iTmp    as integer   no-undo.

  /* Garbage in, garbage out  */
  cOutput = pcString. 

  case pcTarget:
    when "HTML" then
    do:
      cOutput = replace(cOutput,"<","&lt;").
      cOutput = replace(cOutput,">","&gt;").
    end.

    when "4GL" then
    do:
      /* Replace single quotes because we are using them for 4GL separating too */
      cOutput = replace(cOutput, "'", "~~'"). 

      /* Replace CHR's 1 till 13  */
      do iTmp = 1 to 13:
        cOutput = replace(cOutput, chr(iTmp), "' + chr(" + string(iTmp) + ") + '").
      end.
    end.
  end case.

  RETURN pcString.

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getExcelCol wDump 
FUNCTION getExcelCol RETURNS CHARACTER
  ( input iColumnNr as integer ) :
/*------------------------------------------------------------------------------
  Purpose: Transform a column nr to Excel Column name (27 -> AA)
    Notes:  
------------------------------------------------------------------------------*/

  define variable ifirst  as integer   no-undo.
  define variable isecond as integer   no-undo.
  define variable cCols   as character no-undo.

  iFirst  = integer(truncate((iColumnNr - 1) / 26, 0)).
  iSecond = iColumnNr - (26 * iFirst).
  cCols   = chr(64 + iSecond).
  if iFirst > 0 then
    cCols = chr(64 + iFirst) + cCols.

  return cCols.

end function. /* getExcelCol */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getFieldListFromIndexInformation wDump 
FUNCTION getFieldListFromIndexInformation RETURNS CHARACTER
  ( input picIndexInformation as character
  ) :

/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  define variable     cFieldList           as character  no-undo.
  define variable     cIndexName           as character  no-undo.
  define variable     iCurPair             as integer    no-undo.

  /*   The returned comma-separated list consists of the following in the specified order:  */
  /*   - The index name.                                                                    */
  /*   - Three integer values of value 0 (FALSE) or 1 (TRUE) depending on whether           */
  /*     1 the index is unique                                                              */
  /*     2 the index primary                                                                */
  /*     3 the index is a word index.                                                       */
  /*   - The names of the index fields, each followed by a 0 (ascending) or 1 (descending). */

  assign
    cFieldList = ""
    .

  if picIndexInformation = ? then
    return cFieldList.

  cIndexName = entry(1,picIndexInformation).
  
  entry(1,picIndexInformation) = "".
  
  picIndexInformation = substring(picIndexInformation,8).
  
  do iCurPair = 1 to num-entries(picIndexInformation) - 1 by 2:

    cFieldList = substitute
      ( "&1&2&3"
      , cFieldList 
      , (if cFieldList  = "" then "" else ",")
      , entry(iCurPair,picIndexInformation)
      ).
  end. /* do iCurPair = 1 to num-entries(picIndexInformation) - 1 by 2: */
  
  return cFieldList.

end function. /* getFieldListFromIndexInformation */

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
      WHEN "date"        THEN cFieldValue = SUBSTITUTE("date(&1,&2,&3)"
                                                      , MONTH(phField:BUFFER-VALUE(piExtent))
                                                      , DAY(phField:BUFFER-VALUE(piExtent))
                                                      , YEAR(phField:BUFFER-VALUE(piExtent))
                                                      ).
      WHEN "datetime"    THEN cFieldValue = SUBSTITUTE("datetime(date(&1,&2,&3),&4)"
                                                      , MONTH(phField:BUFFER-VALUE(piExtent))
                                                      , DAY(phField:BUFFER-VALUE(piExtent))
                                                      , YEAR(phField:BUFFER-VALUE(piExtent))
                                                      , MTIME(phField:BUFFER-VALUE(piExtent))
                                                      ).
      WHEN "datetime-tz" THEN cFieldValue = SUBSTITUTE("datetime-tz(date(&1,&2,&3),&4,&5)"
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
  ( input picExpressions as character
  , input picList        as character
  ) :

/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  define variable     lGreaterThanZero           as logical    no-undo.
  define variable     iTel                       as integer    no-undo.
  
  assign
    lGreaterThanZero = true
    .

  
  do iTel = 1 to num-entries(picExpressions):

    assign
      lGreaterThanZero = lGreaterThanZero and lookup(entry(iTel,picExpressions), picList) > 0 
      .

    if not lGreaterThanZero then
      return lGreaterThanZero.

  end.

  return lGreaterThanZero.

end function. /* function multipleLookUpGreaterThanZero returns logical */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
