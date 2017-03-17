&ANALYZE-SUSPEND _VERSION-NUMBER UIB_v9r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME CURRENT-WINDOW
&Scoped-define FRAME-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Dialog-Frame 
/*************************************************************/
/* Copyright (c) 1984-2005 by Progress Software Corporation  */
/*                                                           */
/* All rights reserved.  No part of this program or document */
/* may be  reproduced in  any form  or by  any means without */
/* permission in writing from PROGRESS Software Corporation. */
/*************************************************************/
/*------------------------------------------------------------------------

  File      : adm2/askquestion.w
  Purpose   : Question Message dialog with one prompt value
  Parameters: pcMessage  - The message (question) 
              pcButtons  - Comma delimited string of button labels
                          - 3 entries  Yes,No,Cancel 
                          - 2 entries  Yes,No
                          - 1 entry    Yes 
             pcFieldInfo - Comma separated list of field info
                          - 1st entry = label             
                          - 2nd entry = datatype (optional; default 'char')
                            Supports char, int and dec. 
                          - 3rd entry = alignment, L,R,C (optional; default L)
                          - 4th entry = format commas allowed (optional)
             pcTitle     - Message Title
        I-O  pcValue     - Character Value of field  
        OUT  plOk        - Chosen action (Yes no cancel) button
------------------------------------------------------------------------*/
/*          This .W file was created with the Progress AppBuilder.       */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */

&IF DEFINED(UIB_is_Running) NE 0  &THEN
  DEFINE variable pcTitle         AS CHARACTER  no-undo initial 'Disconnect user'.
  DEFINE variable pcMessage       AS CHARACTER  no-undo initial 'Do you want to disconnect user "&1" from database "&2"?'.
  DEFINE variable pcButtons       AS CHARACTER  no-undo initial '&Yes,&No'.
  define variable plCanHide       as logical    no-undo initial true.
  DEFINE variable piButton        AS integer    NO-UNDO INIT ?.
  DEFINE variable plDontShowAgain AS logical    NO-UNDO INIT ?.
&ELSE
  DEFINE INPUT PARAMETER  pcTitle         AS CHARACTER  no-undo initial 'Disconnect user'.
  DEFINE INPUT PARAMETER  pcMessage       AS CHARACTER  no-undo initial 'Do you want to disconnect user "&1" from database "&2"?'.
  DEFINE INPUT PARAMETER  pcButtons       AS CHARACTER  no-undo initial '&Yes,&No'.
  DEFINE INPUT PARAMETER  plCanHide       as logical    no-undo initial true.
  DEFINE OUTPUT PARAMETER piButton        AS integer    NO-UNDO INIT ?.
  DEFINE OUTPUT PARAMETER plDontShowAgain AS logical    NO-UNDO INIT ?.
&ENDIF

{ datadigger.i } 

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

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS EdMessage BtnYes 
&Scoped-Define DISPLAYED-OBJECTS tgDontShowAgain 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON BtnCancel AUTO-END-KEY 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON BtnNo AUTO-GO 
     LABEL "&No" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON BtnYes AUTO-GO 
     LABEL "&Yes" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE VARIABLE EdMessage AS CHARACTER 
     CONTEXT-HELP-ID 0
     VIEW-AS EDITOR NO-BOX
     SIZE-PIXELS 325 BY 74 NO-UNDO.

DEFINE IMAGE imgQuestion TRANSPARENT
     SIZE-PIXELS 32 BY 36.

DEFINE VARIABLE tgDontShowAgain AS LOGICAL INITIAL no 
     LABEL "&Don't show this message again" 
     VIEW-AS TOGGLE-BOX
     SIZE-PIXELS 215 BY 17 NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     EdMessage AT Y 11 X 65 NO-LABEL NO-TAB-STOP 
     BtnYes AT Y 115 X 145
     BtnNo AT Y 115 X 227
     BtnCancel AT Y 115 X 309
     tgDontShowAgain AT Y 145 X 5
     imgQuestion AT Y 11 X 10
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 402 BY 204
         TITLE "Question"
         DEFAULT-BUTTON BtnYes CANCEL-BUTTON BtnCancel.


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

/* SETTINGS FOR BUTTON BtnCancel IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* SETTINGS FOR BUTTON BtnNo IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* SETTINGS FOR EDITOR EdMessage IN FRAME Dialog-Frame
   NO-DISPLAY                                                           */
ASSIGN 
       EdMessage:AUTO-RESIZE IN FRAME Dialog-Frame      = TRUE
       EdMessage:READ-ONLY IN FRAME Dialog-Frame        = TRUE.

/* SETTINGS FOR IMAGE imgQuestion IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* SETTINGS FOR TOGGLE-BOX tgDontShowAgain IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME


/* Setting information for Queries and Browse Widgets fields            */

&ANALYZE-SUSPEND _QUERY-BLOCK DIALOG-BOX Dialog-Frame
/* Query rebuild information for DIALOG-BOX Dialog-Frame
     _Options          = "SHARE-LOCK"
     _Query            is NOT OPENED
*/  /* DIALOG-BOX Dialog-Frame */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* Question */
DO:
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME BtnCancel
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnCancel Dialog-Frame
ON CHOOSE OF BtnCancel IN FRAME Dialog-Frame /* Cancel */
DO:
  piButton = 3.
  plDontShowAgain = tgDontShowAgain:checked.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnCancel Dialog-Frame
ON CURSOR-LEFT OF BtnCancel IN FRAME Dialog-Frame /* Cancel */
DO:
  APPLY "ENTRY" TO btnNo.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME BtnNo
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnNo Dialog-Frame
ON CHOOSE OF BtnNo IN FRAME Dialog-Frame /* No */
DO:
  piButton = 2.
  plDontShowAgain = tgDontShowAgain:checked.  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnNo Dialog-Frame
ON CURSOR-LEFT OF BtnNo IN FRAME Dialog-Frame /* No */
DO:
  APPLY "ENTRY" TO btnYes.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnNo Dialog-Frame
ON CURSOR-RIGHT OF BtnNo IN FRAME Dialog-Frame /* No */
DO:
  APPLY "ENTRY" TO btnCancel.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME BtnYes
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnYes Dialog-Frame
ON CHOOSE OF BtnYes IN FRAME Dialog-Frame /* Yes */
DO:
  piButton = 1.
  plDontShowAgain = tgDontShowAgain:checked.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnYes Dialog-Frame
ON CURSOR-RIGHT OF BtnYes IN FRAME Dialog-Frame /* Yes */
DO:
  APPLY "ENTRY" TO btnNo.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Dialog-Frame 


/* ***************************  Main Block  *************************** */
/* Parent the dialog-box to the ACTIVE-WINDOW, if there is no parent.   */
IF VALID-HANDLE(ACTIVE-WINDOW) AND FRAME {&FRAME-NAME}:PARENT eq ? THEN 
  FRAME {&FRAME-NAME}:PARENT = ACTIVE-WINDOW.
 

DO ON ERROR   UNDO, LEAVE
   ON END-KEY UNDO, LEAVE:

  RUN initializeObject.
  RUN enable_UI.

  /* Toggle for Don't Show Again is optional */
  tgDontShowAgain:visible = plCanHide.
  tgDontShowAgain:sensitive = plCanHide. 

  WAIT-FOR GO OF FRAME {&FRAME-NAME}.
  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

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
  DISPLAY tgDontShowAgain 
      WITH FRAME Dialog-Frame.
  ENABLE EdMessage BtnYes 
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
   DEFINE VARIABLE lYesNoCancel  AS LOGICAL    NO-UNDO.
   DEFINE VARIABLE lOkCancel     AS LOGICAL    NO-UNDO.
   DEFINE VARIABLE lOk           AS LOGICAL    NO-UNDO.
   DEFINE VARIABLE iNumButtons   AS INTEGER    NO-UNDO.
   DEFINE VARIABLE dMargin       AS DECIMAL    NO-UNDO.
   DEFINE VARIABLE iVertMargin   AS INTEGER    NO-UNDO.
   DEFINE VARIABLE cYesLabel     AS CHARACTER  NO-UNDO.
   DEFINE VARIABLE cNoLabel      AS CHARACTER  NO-UNDO.
   DEFINE VARIABLE cCancelLabel  AS CHARACTER  NO-UNDO.
   define variable iFrameHeight  as integer    no-undo. 

   DO WITH FRAME {&FRAME-NAME}:

     /* Get fonts */
     frame {&frame-name}:font = getFont('Default').

     iNumButtons  = NUM-ENTRIES(pcButtons).   
     IF pcButtons = '' THEN pcButtons = 'OK'.

     /* Show Question-image or DD-logo */
     if pcMessage matches "*?" then
       imgQuestion:load-image( getImagePath('Question.gif')) no-error.
     else 
       imgQuestion:load-image( getImagePath("DataDigger24x24.gif")) no-error.
     
     /* Replace fake NEWLINES with chr(10) */
     pcMessage = replace(pcMessage,'~~n',chr(10)).

     /* Strip leading spaces */
     pcMessage = TRIM(pcMessage).

     PUBLISH "debugMessage" (1, pcMessage).

     /* Make some room in the frame for moving around with widgets */
     FRAME {&FRAME-NAME}:HEIGHT-PIXELS = FRAME {&FRAME-NAME}:HEIGHT-PIXELS * 2.

     ASSIGN            
       FRAME {&FRAME-NAME}:TITLE  = IF pcTitle > '' THEN pcTitle ELSE FRAME {&FRAME-NAME}:TITLE  
       edMessage:SCREEN-VALUE     = RIGHT-TRIM(pcMessage,CHR(10))
       edMessage:INNER-LINES      = edMessage:NUM-LINES
       dMargin                    = imgQuestion:COL /* Use the editor Y as margin template */
       iVertMargin                = edMessage:Y 
       btnYes:Y                   = edMessage:Y + edMessage:HEIGHT-PIXELS + iVertMargin
       btnNo:Y                    = btnYes:Y 
       btnCancel:y                = btnYes:Y 
       .
                 
     /* Toggle for Don't Show Again is optional */
     if plCanHide then
     do:
       tgDontShowAgain:visible = plCanHide.
       tgDontShowAgain:sensitive = plCanHide. 
       tgDontShowAgain:y = btnYes:Y + btnYes:height-pixels + iVertMargin.
      
       /* Add border top and bottom to ensure min heigth  */
       FRAME {&FRAME-NAME}:HEIGHT-PIXELS = tgDontShowAgain:Y + tgDontShowAgain:HEIGHT-PIXELS
                                         + FRAME {&FRAME-NAME}:BORDER-TOP-PIXELS 
                                         + FRAME {&FRAME-NAME}:BORDER-BOTTOM-PIXELS
                                         + INTEGER(iVertMargin / 2) NO-ERROR.
     end.
     else 
     do:
       tgDontShowAgain:visible = plCanHide.
       tgDontShowAgain:sensitive = plCanHide. 
       tgDontShowAgain:y = 1.

       FRAME {&FRAME-NAME}:HEIGHT-P = btnYes:Y + btnYes:height-pixels
                                    + FRAME {&FRAME-NAME}:BORDER-TOP-P 
                                    + FRAME {&FRAME-NAME}:BORDER-BOTTOM-P
                                    + INT(iVertMargin / 2) NO-ERROR.

     end.
    
     ASSIGN 
       btnNo:HIDDEN           = iNumButtons < 2
       btnCancel:HIDDEN       = iNumButtons < 3
       btnNo:SENSITIVE        = NOT btnNo:HIDDEN
       btnCancel:SENSITIVE    = NOT btnCancel:HIDDEN
      
       cYesLabel              = ENTRY(1,pcButtons)
       cNoLabel               = ENTRY(2,pcButtons) WHEN iNumButtons >= 2
       cCancelLabel           = ENTRY(3,pcButtons) WHEN iNumButtons >= 3
      
       btnYes:LABEL           = IF cYesLabel > '':U THEN cYesLabel 
                                  ELSE IF iNumButtons = 1 THEN 'OK' ELSE btnYes:LABEL
       btnNo:LABEL            = IF cNoLabel > '':U THEN cNoLabel ELSE btnNo:LABEL
       btnCancel:LABEL        = IF cCancelLabel > '':U THEN cCancelLabel ELSE btnCancel:LABEL
       btnYes:width           = MAX(btnYes:width,FONT-TABLE:GET-TEXT-width(btnYes:LABEL) + 1.5) 
       btnNo:width            = MAX(btnNo:width,FONT-TABLE:GET-TEXT-width(btnNo:LABEL) + 1.5) 
       btnCancel:width        = MAX(btnCancel:width,FONT-TABLE:get-text-width(btnCancel:LABEL) + 1.5) 
       btnCancel:COL          = FRAME {&FRAME-NAME}:width - (btnCancel:width + dMargin)
       btnNo:COL              = IF btnCancel:HIDDEN  
                                  THEN FRAME {&FRAME-NAME}:width - (btnNo:width + dMargin) 
                                  ELSE btnCancel:COL - (btnNo:width + (dMargin / 2))
       btnYes:COL             = MAX(1, IF btnNo:HIDDEN  
                                      THEN FRAME {&FRAME-NAME}:width - (btnYes:width + dMargin) 
                                      ELSE btnNo:COL - (btnYes:width + (dMargin / 2)) ).
      
     /* For some reasons, these #*$&# scrollbars keep coming back */
     run showScrollBars(frame {&frame-name}:handle, no, no). /* KILL KILL KILL */
   END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME