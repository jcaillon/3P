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

/* Parameters Definitions ---                                           */
DEFINE INPUT PARAMETER pcDatabase AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER pcField    AS CHARACTER NO-UNDO.
DEFINE INPUT-OUTPUT PARAMETER pcList AS CHARACTER NO-UNDO. 

/* Local Variable Definitions ---                                       */
{ DataDigger.i }

DEFINE VARIABLE glEditMode AS LOGICAL NO-UNDO.

DEFINE TEMP-TABLE ttSort NO-UNDO
  FIELD cItem AS CHARACTER.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME Dialog-Frame

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS sList fiNewItem btnUp btnDelete btnDown ~
btnSort rsDelimiter fcDelimiter fiDelimiter Btn_OK Btn_Cancel RECT-1 
&Scoped-Define DISPLAYED-OBJECTS sList fiNewItem rsDelimiter fcDelimiter ~
fiDelimiter 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnAdd 
     LABEL "+" 
     SIZE-PIXELS 25 BY 25 TOOLTIP "add an item to the list".

DEFINE BUTTON btnDelete 
     LABEL "del" 
     SIZE-PIXELS 25 BY 25 TOOLTIP "delete the selected item".

DEFINE BUTTON btnDown 
     LABEL "dn" 
     SIZE-PIXELS 25 BY 25 TOOLTIP "move the selected item down".

DEFINE BUTTON btnSort 
     LABEL "srt" 
     SIZE-PIXELS 25 BY 25 TOOLTIP "sort items".

DEFINE BUTTON btnUp 
     LABEL "up" 
     SIZE-PIXELS 25 BY 25 TOOLTIP "move the selected item up".

DEFINE BUTTON Btn_Cancel AUTO-END-KEY 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON Btn_OK AUTO-GO 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE VARIABLE fcDelimiter AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 16 BY 21 TOOLTIP "type the delimiter you would like to use" NO-UNDO.

DEFINE VARIABLE fiDelimiter AS INTEGER FORMAT ">>9":U INITIAL 0 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 28 BY 21 TOOLTIP "the ASCII code of the delimiter you would like to use" NO-UNDO.

DEFINE VARIABLE fiNewItem AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 245 BY 20 NO-UNDO.

DEFINE VARIABLE rsDelimiter AS INTEGER INITIAL 44 
     VIEW-AS RADIO-SET VERTICAL
     RADIO-BUTTONS 
          "&Comma", 44,
"Se&mi colon", 59,
"Co&lon", 58,
"&Slash", 47,
"&Backslash", 92,
"&Pipe", 124,
"CHR-&1", 1,
"&Other:", 0
     SIZE-PIXELS 75 BY 154 TOOLTIP "select the separator for the list" NO-UNDO.

DEFINE RECTANGLE RECT-1
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 125 BY 184.

DEFINE VARIABLE sList AS CHARACTER 
     VIEW-AS SELECTION-LIST SINGLE SCROLLBAR-VERTICAL 
     LIST-ITEMS "alpha","bravo","charlie" 
     SIZE-PIXELS 245 BY 305 NO-UNDO.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME Dialog-Frame
     sList AT Y 5 X 5 NO-LABEL WIDGET-ID 2
     fiNewItem AT Y 310 X 5 NO-LABEL WIDGET-ID 4
     btnAdd AT Y 308 X 250 WIDGET-ID 6
     btnUp AT Y 85 X 250 WIDGET-ID 8
     btnDelete AT Y 110 X 250 WIDGET-ID 10
     btnDown AT Y 135 X 250 WIDGET-ID 12
     btnSort AT Y 185 X 250 WIDGET-ID 32
     rsDelimiter AT Y 60 X 295 NO-LABEL WIDGET-ID 14
     fcDelimiter AT Y 195 X 350 COLON-ALIGNED NO-LABEL WIDGET-ID 30
     fiDelimiter AT Y 195 X 367 COLON-ALIGNED NO-LABEL WIDGET-ID 22
     Btn_OK AT Y 280 X 335
     Btn_Cancel AT Y 308 X 335
     "Delimiter:" VIEW-AS TEXT
          SIZE-PIXELS 60 BY 13 AT Y 40 X 315 WIDGET-ID 26
     RECT-1 AT Y 46 X 290 WIDGET-ID 28
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 429 BY 366
         TITLE "List Editor"
         DEFAULT-BUTTON Btn_OK WIDGET-ID 100.


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
   FRAME-NAME Custom                                                    */
ASSIGN 
       FRAME Dialog-Frame:SCROLLABLE       = FALSE
       FRAME Dialog-Frame:HIDDEN           = TRUE.

/* SETTINGS FOR BUTTON btnAdd IN FRAME Dialog-Frame
   NO-ENABLE                                                            */
ASSIGN 
       fcDelimiter:HIDDEN IN FRAME Dialog-Frame           = TRUE.

ASSIGN 
       fiDelimiter:HIDDEN IN FRAME Dialog-Frame           = TRUE.

/* SETTINGS FOR FILL-IN fiNewItem IN FRAME Dialog-Frame
   ALIGN-L                                                              */
/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME Dialog-Frame
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON GO OF FRAME Dialog-Frame /* List Editor */
DO:
  pcList = sList:list-items.  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL Dialog-Frame Dialog-Frame
ON WINDOW-CLOSE OF FRAME Dialog-Frame /* List Editor */
DO:
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnAdd
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnAdd Dialog-Frame
ON CHOOSE OF btnAdd IN FRAME Dialog-Frame /* + */
OR 'RETURN' OF fiNewItem
DO:

  DEFINE VARIABLE iThis  AS INTEGER   NO-UNDO. 
  DEFINE VARIABLE cThis  AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cList  AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cSep   AS CHARACTER NO-UNDO. 
  
  cList = sList:list-items.

  cSep  = sList:delimiter. 
  cThis  = sList:screen-value.
  IF cThis = ? THEN cThis = "".
  iThis  = LOOKUP(cThis, cList, cSep).

  IF fiNewItem:screen-value <> "" THEN
  DO:
    IF glEditMode THEN
    DO:
      ENTRY(iThis,cList,cSep) = fiNewItem:screen-value.
      sList:list-items = cList.
    END.
    ELSE
    DO:
      IF cList = ? THEN 
        sList:list-items = fiNewItem:screen-value.
      ELSE
        sList:insert(fiNewItem:screen-value, iThis + 1).
    END.

    sList:screen-value = fiNewItem:screen-value.
    fiNewItem:screen-value = "".
    APPLY "VALUE-CHANGED" TO fiNewItem.
    
    IF glEditMode THEN APPLY 'ENTRY' TO sList. 
    glEditMode = FALSE.
    RETURN NO-APPLY.
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDelete
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDelete Dialog-Frame
ON CHOOSE OF btnDelete IN FRAME Dialog-Frame /* del */
OR 'DELETE-CHARACTER' OF sList
DO:

  DEFINE VARIABLE iThis  AS INTEGER   NO-UNDO. 
  DEFINE VARIABLE iOther AS INTEGER   NO-UNDO. 
  DEFINE VARIABLE cThis  AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cOther AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cList  AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cSep   AS CHARACTER NO-UNDO. 
  
  cList = sList:list-items.
  cSep  = sList:delimiter. 
  cThis  = sList:screen-value.
  IF cThis = ? THEN RETURN.

  iThis  = LOOKUP(cThis, cList, cSep).
  ENTRY(iThis, cList, cSep) = "".
  cList = TRIM(REPLACE(cList, cSep + cSep, cSep), cSep).

  sList:list-items = cList.
  IF cList > "" THEN
    sList:screen-value = ENTRY( MINIMUM(iThis,NUM-ENTRIES(cList,cSep)),cList,cSep).


END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDown
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDown Dialog-Frame
ON CHOOSE OF btnDown IN FRAME Dialog-Frame /* dn */
OR 'CTRL-CURSOR-DOWN' OF sList
DO:
  RUN moveItem(+1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnSort
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSort Dialog-Frame
ON CHOOSE OF btnSort IN FRAME Dialog-Frame /* srt */
OR 'CTRL-S' OF sList
DO:
  RUN sortList.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnUp
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnUp Dialog-Frame
ON CHOOSE OF btnUp IN FRAME Dialog-Frame /* up */
OR 'CTRL-CURSOR-UP' OF sList
DO:
  RUN moveItem(-1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fcDelimiter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fcDelimiter Dialog-Frame
ON VALUE-CHANGED OF fcDelimiter IN FRAME Dialog-Frame
DO:
  IF SELF:screen-value > "" THEN
  DO:
    fiDelimiter:screen-value = STRING(ASC(SELF:screen-value)).
  END.

  APPLY "VALUE-CHANGED" TO fiDelimiter.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fiNewItem
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiNewItem Dialog-Frame
ON END-ERROR OF fiNewItem IN FRAME Dialog-Frame
DO:
  fiNewItem:screen-value = "".
  glEditMode = FALSE. 
  APPLY "VALUE-CHANGED" TO fiNewItem. 
  APPLY "ENTRY" TO sList.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiNewItem Dialog-Frame
ON VALUE-CHANGED OF fiNewItem IN FRAME Dialog-Frame
DO:
  btnAdd:sensitive = (SELF:screen-value <> "").
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME rsDelimiter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL rsDelimiter Dialog-Frame
ON VALUE-CHANGED OF rsDelimiter IN FRAME Dialog-Frame
OR "ENTER" OF fiDelimiter
OR "ENTER" OF fcDelimiter
DO:
  DEFINE VARIABLE cSep  AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cList AS CHARACTER NO-UNDO. 

  fiDelimiter:sensitive = (rsDelimiter:screen-value = "0").
  fiDelimiter:visible   = (rsDelimiter:screen-value = "0").
  fcDelimiter:sensitive = (rsDelimiter:screen-value = "0").
  fcDelimiter:visible   = (rsDelimiter:screen-value = "0").

  /* If we set the radioset to "other" set focus to fill in */
  IF SELF:name = "rsDelimiter" 
    AND rsDelimiter:screen-value = "0" THEN 
  DO:
    APPLY 'ENTRY' TO fcDelimiter.
    IF fcDelimiter:screen-value = ? THEN
      RETURN NO-APPLY.
  END.

  /* Otherwise reflect the changes in the list */
  cList = sList:list-items.

  IF rsDelimiter:screen-value = "0" THEN
    cSep = CHR(INTEGER(fiDelimiter:screen-value)) NO-ERROR.
  ELSE
    cSep = CHR(INTEGER(rsDelimiter:screen-value)) NO-ERROR.

  IF NOT ERROR-STATUS:ERROR AND cSep > "" THEN 
  DO:
    sList:delimiter = cSep.
    sList:list-items = cList.
    IF cList > ""  THEN sList:screen-value = ENTRY(1,cList,cSep).

    fiDelimiter:screen-value = STRING(ASC(cSep)).
    fcDelimiter:screen-value = cSep.

    /* Save this separator in the settings */
    setRegistry( SUBSTITUTE('DB:&1',pcDatabase)
               , SUBSTITUTE('&1:delimiter', pcField) 
               , fiDelimiter:screen-value
               ).
  END.

  /* Don't set focus to list when we are typing a number in 
   * fiDelimiter. Otherwise typing is weird.
   */
  DO:
    APPLY 'ENTRY' TO sList.
    RETURN NO-APPLY.
  END.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME sList
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL sList Dialog-Frame
ON ANY-PRINTABLE OF sList IN FRAME Dialog-Frame
DO:
  DEFINE VARIABLE iPos AS INTEGER NO-UNDO. 

  iPos = LOOKUP(STRING(LASTKEY), rsDelimiter:radio-buttons).
  IF iPos > 0 AND iPos MODULO 2  = 0 THEN 
  DO:
    rsDelimiter:screen-value = ENTRY(iPos, rsDelimiter:radio-buttons).
    APPLY 'value-changed' TO rsDelimiter.
  END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL sList Dialog-Frame
ON DEFAULT-ACTION OF sList IN FRAME Dialog-Frame
OR "RETURN" OF sList
DO:
  glEditMode = TRUE.
  fiNewItem:screen-value = sList:screen-value.

  APPLY "VALUE-CHANGED" TO fiNewItem. 
  APPLY "entry" TO fiNewItem.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL sList Dialog-Frame
ON INSERT-MODE OF sList IN FRAME Dialog-Frame
DO:
  APPLY 'ENTRY' TO fiNewItem.

  APPLY "VALUE-CHANGED" TO fiNewItem. 
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL sList Dialog-Frame
ON VALUE-CHANGED OF sList IN FRAME Dialog-Frame
DO:
  SELF:TOOLTIP = SELF:SCREEN-VALUE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Dialog-Frame 


/* ***************************  Main Block  *************************** */

/* Parent the dialog-box to the ACTIVE-WINDOW, if there is no parent.   */
IF VALID-HANDLE(ACTIVE-WINDOW) AND FRAME {&FRAME-NAME}:PARENT EQ ?
THEN FRAME {&FRAME-NAME}:PARENT = ACTIVE-WINDOW.


/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK 
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK :

  /* Get fonts */
  FRAME {&frame-name}:font = getFont('Default').
  RUN enable_UI.
  RUN initializeObject.
  APPLY "value-changed" TO rsDelimiter.
  IF sList:list-items <> "" THEN sList:screen-value = ENTRY(1,sList:list-items,sList:delimiter).

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
  DISPLAY sList fiNewItem rsDelimiter fcDelimiter fiDelimiter 
      WITH FRAME Dialog-Frame.
  ENABLE sList fiNewItem btnUp btnDelete btnDown btnSort rsDelimiter 
         fcDelimiter fiDelimiter Btn_OK Btn_Cancel RECT-1 
      WITH FRAME Dialog-Frame.
  VIEW FRAME Dialog-Frame.
  {&OPEN-BROWSERS-IN-QUERY-Dialog-Frame}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject Dialog-Frame 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------------
  Name : initializeObject
  Desc : Prepare frame etc etc
------------------------------------------------------------------------------*/
  
  DEFINE VARIABLE iPos AS INTEGER NO-UNDO. 
  DEFINE VARIABLE cSep AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE iSep AS INTEGER NO-UNDO. 

  DO WITH FRAME {&FRAME-NAME}:

    btnAdd   :LOAD-IMAGE(getImagePath('Add.gif')).
    btnUp    :LOAD-IMAGE(getImagePath('Up.gif')).
    btnDown  :LOAD-IMAGE(getImagePath('Down.gif')).
    btnDelete:LOAD-IMAGE(getImagePath('Clear.gif')).
    btnSort  :LOAD-IMAGE(getImagePath('Sort.gif')).

    /* Populate list */
    sList:list-items = pcList. 

    /* Is a separator defined in the settings? */
    /* Else try to find the separator in the field value */
    cSep = getRegistry( SUBSTITUTE('DB:&1',pcDatabase)
                      , SUBSTITUTE('&1:delimiter', pcField) 
                      ).

    /* avoid strange errors in INI file */
    iSep = INTEGER(cSep) NO-ERROR. 
    IF iSep = 0 THEN cSep = ?.

    IF cSep <> ? THEN 
    DO:
      /* Is it a value in the radioset? */
      iPos = LOOKUP(cSep, rsDelimiter:radio-buttons).
      IF iPos > 0 AND iPos MODULO 2 = 0 THEN
        rsDelimiter:screen-value = ENTRY(iPos, rsDelimiter:radio-buttons).
      ELSE 
      DO:
        rsDelimiter:SCREEN-VALUE = "0".
        fiDelimiter:SCREEN-VALUE = cSep.
        fcDelimiter:SCREEN-VALUE = CHR(INTEGER(cSep)).

      END.
      APPLY 'value-changed' TO rsDelimiter.
    END.

    ELSE
    findSep:
    DO iPos = 2 TO NUM-ENTRIES(rsDelimiter:radio-buttons) BY 2:
      cSep = CHR(INTEGER(ENTRY(iPos, rsDelimiter:radio-buttons))).
      IF NUM-ENTRIES(pcList,cSep) > 1 THEN
      DO:
        rsDelimiter:screen-value = ENTRY(iPos, rsDelimiter:radio-buttons).
        APPLY 'value-changed' TO rsDelimiter.
        LEAVE findSep.
      END.
    END.
  END.

END PROCEDURE. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE moveItem Dialog-Frame 
PROCEDURE moveItem :
/*------------------------------------------------------------------------------
  Name : moveItem
  Desc : Move an item up or down in the list
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER piDirection AS INTEGER NO-UNDO. 

  DEFINE VARIABLE iThis  AS INTEGER   NO-UNDO. 
  DEFINE VARIABLE iOther AS INTEGER   NO-UNDO. 
  DEFINE VARIABLE cThis  AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cOther AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cList  AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cSep   AS CHARACTER NO-UNDO. 
  
  DO WITH FRAME {&FRAME-NAME}:
    cList = sList:LIST-ITEMS.
    cSep  = sList:DELIMITER. 
    cThis  = sList:SCREEN-VALUE.
    IF cThis = ? THEN RETURN.
  
    iThis  = LOOKUP(cThis, cList, cSep).
    iOther = iThis + piDirection.
    IF iOther = 0 OR iOther > NUM-ENTRIES(cList, cSep) THEN RETURN. 
  
    cOther = ENTRY(iOther, cList, cSep).
    
    ENTRY(iThis, cList, cSep) = cOther.
    ENTRY(iOther, cList, cSep) = cThis.
    sList:LIST-ITEMS = cList.
    sList:SCREEN-VALUE = cThis.
  END.

END PROCEDURE. /* moveItem */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE sortList Dialog-Frame 
PROCEDURE sortList :
/*------------------------------------------------------------------------------
  Name : sortList
  Desc : Sort the complete list
------------------------------------------------------------------------------*/

  DEFINE VARIABLE cList     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cSep      AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cListDesc AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cListAsc  AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iElement  AS INTEGER   NO-UNDO.
  
  DO WITH FRAME {&FRAME-NAME}:
    cList = sList:LIST-ITEMS.
    cSep  = sList:DELIMITER. 

    /* Save them to tt */
    EMPTY TEMP-TABLE ttSort.
    DO iElement = 1 TO NUM-ENTRIES(cList,cSep):
      CREATE ttSort.
      ASSIGN ttSort.cItem = ENTRY(iElement,cList,cSep).
    END.
    
    /* Get ascending list */
    FOR EACH ttSort BY ttSort.cItem: 
      cListAsc = cListAsc + cSep + ttSort.cItem.
    END.
    cListAsc = SUBSTRING(cListAsc,2).
  
    /* Get ascending list */
    FOR EACH ttSort BY ttSort.cItem DESCENDING: 
      cListDesc = cListDesc + cSep + ttSort.cItem.
    END.
    cListDesc = SUBSTRING(cListDesc,2).

    EMPTY TEMP-TABLE ttSort.
  
    /* Find out if the list is currently asc / desc */
    IF sList:LIST-ITEMS = cListAsc THEN
      sList:LIST-ITEMS = cListDesc.
    ELSE 
      sList:LIST-ITEMS = cListAsc.

  END.
                               
END PROCEDURE. /* sortList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

