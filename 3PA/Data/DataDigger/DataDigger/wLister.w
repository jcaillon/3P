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
define input parameter pcDatabase as character no-undo.
define input parameter pcField    as character no-undo.
define input-output parameter pcList as character no-undo. 

/* Local Variable Definitions ---                                       */
{ DataDigger.i }

define variable glEditMode as logical no-undo.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME Dialog-Frame

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS RECT-1 fiNewItem rsDelimiter btnUp ~
fcDelimiter btnDelete sList fiDelimiter btnDown Btn_Cancel Btn_OK 
&Scoped-Define DISPLAYED-OBJECTS fiNewItem rsDelimiter fcDelimiter sList ~
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
     btnAdd AT Y 308 X 250 WIDGET-ID 6
     fiNewItem AT Y 310 X 5 NO-LABEL WIDGET-ID 4
     rsDelimiter AT Y 60 X 295 NO-LABEL WIDGET-ID 14
     btnUp AT Y 85 X 250 WIDGET-ID 8
     fcDelimiter AT Y 195 X 350 COLON-ALIGNED NO-LABEL WIDGET-ID 30
     btnDelete AT Y 110 X 250 WIDGET-ID 10
     sList AT Y 5 X 5 NO-LABEL WIDGET-ID 2
     fiDelimiter AT Y 195 X 367 COLON-ALIGNED NO-LABEL WIDGET-ID 22
     btnDown AT Y 135 X 250 WIDGET-ID 12
     Btn_Cancel AT Y 308 X 335
     Btn_OK AT Y 280 X 335
     "Delimiter:" VIEW-AS TEXT
          SIZE-PIXELS 50 BY 13 AT Y 40 X 315 WIDGET-ID 26
     RECT-1 AT Y 46 X 290 WIDGET-ID 28
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 429 BY 365
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
   FRAME-NAME                                                           */
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
or 'RETURN' of fiNewItem
DO:

  define variable iThis  as integer   no-undo. 
  define variable cThis  as character no-undo. 
  define variable cList  as character no-undo. 
  define variable cSep   as character no-undo. 
  
  cList = sList:list-items.

  cSep  = sList:delimiter. 
  cThis  = sList:screen-value.
  if cThis = ? then cThis = "".
  iThis  = lookup(cThis, cList, cSep).

  if fiNewItem:screen-value <> "" then
  do:
    if glEditMode then
    do:
      entry(iThis,cList,cSep) = fiNewItem:screen-value.
      sList:list-items = cList.
    end.
    else
    do:
      if cList = ? then 
        sList:list-items = fiNewItem:screen-value.
      else
        sList:insert(fiNewItem:screen-value, iThis + 1).
    end.

    sList:screen-value = fiNewItem:screen-value.
    fiNewItem:screen-value = "".
    apply "VALUE-CHANGED" to fiNewItem.
    
    if glEditMode then apply 'ENTRY' to sList. 
    glEditMode = false.
    return no-apply.
  end.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDelete
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDelete Dialog-Frame
ON CHOOSE OF btnDelete IN FRAME Dialog-Frame /* del */
or 'DELETE-CHARACTER' of sList
do:

  define variable iThis  as integer   no-undo. 
  define variable iOther as integer   no-undo. 
  define variable cThis  as character no-undo. 
  define variable cOther as character no-undo. 
  define variable cList  as character no-undo. 
  define variable cSep   as character no-undo. 
  
  cList = sList:list-items.
  cSep  = sList:delimiter. 
  cThis  = sList:screen-value.
  if cThis = ? then return.

  iThis  = lookup(cThis, cList, cSep).
  entry(iThis, cList, cSep) = "".
  cList = trim(replace(cList, cSep + cSep, cSep), cSep).

  sList:list-items = cList.
  if cList > "" then
    sList:screen-value = entry( minimum(iThis,num-entries(cList,cSep)),cList,cSep).


end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnDown
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnDown Dialog-Frame
ON CHOOSE OF btnDown IN FRAME Dialog-Frame /* dn */
or 'CTRL-CURSOR-DOWN' of sList
DO:
  run moveItem(+1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnUp
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnUp Dialog-Frame
ON CHOOSE OF btnUp IN FRAME Dialog-Frame /* up */
or 'CTRL-CURSOR-UP' of sList
DO:
  run moveItem(-1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fcDelimiter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fcDelimiter Dialog-Frame
ON VALUE-CHANGED OF fcDelimiter IN FRAME Dialog-Frame
DO:
  if self:screen-value > "" then
  do:
    fiDelimiter:screen-value = string(asc(self:screen-value)).
  end.

  apply "VALUE-CHANGED" to fiDelimiter.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fiNewItem
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiNewItem Dialog-Frame
ON END-ERROR OF fiNewItem IN FRAME Dialog-Frame
DO:
  fiNewItem:screen-value = "".
  glEditMode = false. 
  apply "VALUE-CHANGED" to fiNewItem. 
  apply "ENTRY" to sList.
  return no-apply.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiNewItem Dialog-Frame
ON VALUE-CHANGED OF fiNewItem IN FRAME Dialog-Frame
DO:
  btnAdd:sensitive = (self:screen-value <> "").
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME rsDelimiter
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL rsDelimiter Dialog-Frame
ON VALUE-CHANGED OF rsDelimiter IN FRAME Dialog-Frame
or "ENTER" of fiDelimiter
or "ENTER" of fcDelimiter
DO:
  define variable cSep  as character no-undo. 
  define variable cList as character no-undo. 

  fiDelimiter:sensitive = (rsDelimiter:screen-value = "0").
  fiDelimiter:visible   = (rsDelimiter:screen-value = "0").
  fcDelimiter:sensitive = (rsDelimiter:screen-value = "0").
  fcDelimiter:visible   = (rsDelimiter:screen-value = "0").

  /* If we set the radioset to "other" set focus to fill in */
  if self:name = "rsDelimiter" 
    and rsDelimiter:screen-value = "0" then 
  do:
    apply 'ENTRY' to fcDelimiter.
    if fcDelimiter:screen-value = ? then
      return no-apply.
  end.

  /* Otherwise reflect the changes in the list */
  cList = sList:list-items.

  if rsDelimiter:screen-value = "0" then
    cSep = chr(integer(fiDelimiter:screen-value)) no-error.
  else
    cSep = chr(integer(rsDelimiter:screen-value)) no-error.

  if not error-status:error and cSep > "" then 
  do:
    sList:delimiter = cSep.
    sList:list-items = cList.
    if cList > ""  then sList:screen-value = entry(1,cList,cSep).

    fiDelimiter:screen-value = string(asc(cSep)).
    fcDelimiter:screen-value = cSep.

    /* Save this separator in the settings */
    setRegistry( substitute('DB:&1',pcDatabase)
               , substitute('&1:delimiter', pcField) 
               , fiDelimiter:screen-value
               ).
  end.

  /* Don't set focus to list when we are typing a number in 
   * fiDelimiter. Otherwise typing is weird.
   */
  do:
    apply 'ENTRY' to sList.
    return no-apply.
  end.

end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME sList
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL sList Dialog-Frame
ON ANY-PRINTABLE OF sList IN FRAME Dialog-Frame
DO:
  define variable iPos as integer no-undo. 

  iPos = lookup(string(lastkey), rsDelimiter:radio-buttons).
  if iPos > 0 and iPos modulo 2  = 0 then 
  do:
    rsDelimiter:screen-value = entry(iPos, rsDelimiter:radio-buttons).
    apply 'value-changed' to rsDelimiter.
  end.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL sList Dialog-Frame
ON DEFAULT-ACTION OF sList IN FRAME Dialog-Frame
or "RETURN" of sList
DO:
  glEditMode = true.
  fiNewItem:screen-value = sList:screen-value.

  apply "VALUE-CHANGED" to fiNewItem. 
  apply "entry" to fiNewItem.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL sList Dialog-Frame
ON INSERT-MODE OF sList IN FRAME Dialog-Frame
DO:
  apply 'ENTRY' to fiNewItem.

  apply "VALUE-CHANGED" to fiNewItem. 
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
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK :

  /* Get fonts */
  frame {&frame-name}:font = getFont('Default').
  run enable_UI.
  run initializeObject.
  apply "value-changed" to rsDelimiter.
  if sList:list-items <> "" then sList:screen-value = entry(1,sList:list-items,sList:delimiter).

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
  DISPLAY fiNewItem rsDelimiter fcDelimiter sList fiDelimiter 
      WITH FRAME Dialog-Frame.
  ENABLE RECT-1 fiNewItem rsDelimiter btnUp fcDelimiter btnDelete sList 
         fiDelimiter btnDown Btn_Cancel Btn_OK 
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
  
  define variable iPos as integer no-undo. 
  define variable cSep as character no-undo. 
  define variable iSep as integer no-undo. 

  do with frame {&frame-name}:

    btnAdd   :load-image(getImagePath('Add.gif')).
    btnUp    :load-image(getImagePath('Up.gif')).
    btnDown  :load-image(getImagePath('Down.gif')).
    btnDelete:load-image(getImagePath('Clear.gif')).

    /* Populate list */
    sList:list-items = pcList. 

    /* Is a separator defined in the settings? */
    /* Else try to find the separator in the field value */
    cSep = getRegistry( substitute('DB:&1',pcDatabase)
                      , substitute('&1:delimiter', pcField) 
                      ).

    /* avoid strange errors in INI file */
    iSep = integer(cSep) no-error. 
    if iSep = 0 then cSep = ?.

    if cSep <> ? then 
    do:
      /* Is it a value in the radioset? */
      iPos = lookup(cSep, rsDelimiter:radio-buttons).
      if iPos > 0 and iPos modulo 2 = 0 then
        rsDelimiter:screen-value = entry(iPos, rsDelimiter:radio-buttons).
      else 
      do:
        rsDelimiter:screen-value = "0".
        fiDelimiter:screen-value = cSep.
        fcDelimiter:screen-value = chr(integer(cSep)).

      end.
      apply 'value-changed' to rsDelimiter.
    end.

    else
    findSep:
    do iPos = 2 to num-entries(rsDelimiter:radio-buttons) by 2:
      cSep = chr(integer(entry(iPos, rsDelimiter:radio-buttons))).
      if num-entries(pcList,cSep) > 1 then
      do:
        rsDelimiter:screen-value = entry(iPos, rsDelimiter:radio-buttons).
        apply 'value-changed' to rsDelimiter.
        leave findSep.
      end.
    end.
  end.

end procedure. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE moveItem Dialog-Frame 
PROCEDURE moveItem :
/*------------------------------------------------------------------------------
  Name : moveItem
  Desc : Move an item up or down in the list
------------------------------------------------------------------------------*/
  define input parameter piDirection as integer no-undo. 

  define variable iThis  as integer   no-undo. 
  define variable iOther as integer   no-undo. 
  define variable cThis  as character no-undo. 
  define variable cOther as character no-undo. 
  define variable cList  as character no-undo. 
  define variable cSep   as character no-undo. 
  
  do with frame {&frame-name}:
    cList = sList:list-items.
    cSep  = sList:delimiter. 
    cThis  = sList:screen-value.
    if cThis = ? then return.
  
    iThis  = lookup(cThis, cList, cSep).
    iOther = iThis + piDirection.
    if iOther = 0 or iOther > num-entries(cList, cSep) then return. 
  
    cOther = entry(iOther, cList, cSep).
    
    entry(iThis, cList, cSep) = cOther.
    entry(iOther, cList, cSep) = cThis.
    sList:list-items = cList.
    sList:screen-value = cThis.
  end.

end procedure. /* moveItem */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

