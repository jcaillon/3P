&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME CURRENT-WINDOW
&Scoped-define FRAME-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Dialog-Frame 
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
/*          This .W file was created with the Progress AppBuilder.       */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */
{ datadigger.i }

/* Parameters Definitions ---                                           */
define input-output parameter pcTable   as character  no-undo.
define input        parameter pcOptions as character  no-undo.

/* Local Variable Definitions ---                                       */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME Dialog-Frame

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS RECT-1 fiDir Btn_OK Btn_Cancel tgOpenFile ~
btnChooseDumpFile 
&Scoped-Define DISPLAYED-OBJECTS fiDir tgOpenFile 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnChooseDumpFile 
     LABEL "..." 
     SIZE-PIXELS 20 BY 21.

DEFINE BUTTON Btn_Cancel AUTO-END-KEY 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON Btn_OK AUTO-GO 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE VARIABLE fiDir AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 362 BY 21 TOOLTIP "the dir where you want to dump the .df file to" NO-UNDO.

DEFINE RECTANGLE RECT-1
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL  GROUP-BOX  
     SIZE-PIXELS 410 BY 110.

DEFINE VARIABLE tgOpenFile AS LOGICAL INITIAL no 
     LABEL "&Open DF after dump" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 130 BY 17 TOOLTIP "open the DF file right after dumping" NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     fiDir AT Y 26 X 2 COLON-ALIGNED NO-LABEL WIDGET-ID 2
     Btn_OK AT Y 77 X 245
     Btn_Cancel AT Y 77 X 325
     tgOpenFile AT Y 50 X 12 WIDGET-ID 10
     btnChooseDumpFile AT Y 26 X 380 WIDGET-ID 8
     "Dump definitions to" VIEW-AS TEXT
          SIZE-PIXELS 295 BY 13 AT Y 11 X 12 WIDGET-ID 6
     RECT-1 AT Y 0 X 0 WIDGET-ID 4
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 423 BY 146
         TITLE "Dump Definitions"
         DEFAULT-BUTTON Btn_OK CANCEL-BUTTON Btn_Cancel WIDGET-ID 100.


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
   FRAME-NAME                                                           */
ASSIGN 
       FRAME Dialog-Frame:SCROLLABLE       = FALSE
       FRAME Dialog-Frame:HIDDEN           = TRUE.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* Dump Definitions */
DO:
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnChooseDumpFile
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnChooseDumpFile Dialog-Frame
ON CHOOSE OF btnChooseDumpFile IN FRAME Dialog-Frame /* ... */
do:

  define variable lOkay as logical    no-undo.
  define variable cDir  as character  no-undo.

  cDir = fiDir:screen-value.

  system-dialog get-dir cDir
    initial-dir cDir
    return-to-start-dir.
  
  do with frame {&frame-name}:
    fiDir:screen-value = cDir.
  end.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME Btn_OK
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Btn_OK Dialog-Frame
ON CHOOSE OF Btn_OK IN FRAME Dialog-Frame /* OK */
DO:
  DEFINE VARIABLE cDumpFile AS CHARACTER   NO-UNDO.

  cDumpFile = right-trim(fiDir:screen-value,"\") + "\" + pcTable + ".df".

  run prodict/dump_df.p
    ( pcTable
    , cDumpFile
    , ""
    ).
  
  /* Save settings */
  setRegistry("DataDigger","DumpDF:dir" ,fiDir:screen-value).
  setRegistry("DataDigger","DumpDF:open",string(tgOpenFile:checked)).

  if tgOpenFile:checked then 
    os-command no-wait start value(cDumpFile).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Dialog-Frame 


/* ***************************  Main Block  *************************** */

/* Parent the dialog-box to the ACTIVE-WINDOW, if there is no parent.   */
IF VALID-HANDLE(ACTIVE-WINDOW) AND FRAME {&FRAME-NAME}:PARENT eq ?
THEN FRAME {&FRAME-NAME}:PARENT = ACTIVE-WINDOW.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  RUN initializeObject.
  RUN enable_UI.

  WAIT-FOR GO OF FRAME {&FRAME-NAME}.
END.
RUN disable_UI.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

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
  DISPLAY fiDir tgOpenFile 
      WITH FRAME Dialog-Frame.
  ENABLE RECT-1 fiDir Btn_OK Btn_Cancel tgOpenFile btnChooseDumpFile 
      WITH FRAME Dialog-Frame.
  VIEW FRAME Dialog-Frame.
  {&OPEN-BROWSERS-IN-QUERY-Dialog-Frame}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject Dialog-Frame 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  define variable iOption  as integer   no-undo.
  define variable cOption  as character no-undo.
  define variable cSetting as character no-undo.
  define variable cValue   as character no-undo.

  do with frame {&FRAME-NAME}:
    
    /* Set default font */
    frame {&frame-name}:font = getFont('Default').

    do iOption = 1 to num-entries(pcOptions):
      cOption  = entry(iOption,pcOptions).
      cSetting = entry(1,cOption,"=").
      cValue   = entry(2,cOption,"=").
      
      case cSetting:
        when "x" then frame {&FRAME-NAME}:x = integer(cValue).
        when "y" then frame {&FRAME-NAME}:y = integer(cValue).
      end case.
    end.

    fiDir = getRegistry("DataDigger","DumpDF:dir").
    if fiDir = ? then fiDir = session:temp-dir.

    cSetting = getRegistry("DataDigger","DumpDF:open").
    if cSetting = ? then cSetting = "yes".
    tgOpenFile = logical(cSetting).
  end.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

