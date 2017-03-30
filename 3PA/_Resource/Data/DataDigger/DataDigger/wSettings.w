&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME wSettings
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS wSettings 
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
&if '{&file-name}' matches '*.ab' &then 
  define variable pcSettingsFile as character   no-undo.
  define variable plSuccess      as logical     no-undo.
  pcSettingsFile = 'd:\Data\DropBox\DataDigger\DataDigger-nljrpti.ini'.
&else 
  define input  parameter pcSettingsFile as character   no-undo.
  define output parameter plSuccess      as logical     no-undo.
&endif.

/* Local Variable Definitions ---                                       */

define variable gcPageButtons    as character no-undo. 
define variable giLastActivePage as integer   no-undo. 
define variable giWinX           as integer   no-undo. 
define variable giWinY           as integer   no-undo. 

define temp-table ttFrame no-undo rcode-info
  field cFrame   as character
  field hFrame   as handle 
  field iOrder   as integer
  field cTags    as character
.

/* Windows API entry point */
procedure ShowScrollBar external "user32.dll":
    define input  parameter hwnd        as long.
    define input  parameter fnBar       as long.
    define input  parameter fShow       as long.
    define return parameter ReturnValue as long.
end procedure.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Window
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME DEFAULT-FRAME

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS rcSettings btnSettings ficSettingsFile ~
btnRawEdit fiSearch btPage1 btPage2 btPage3 BtnCancel-2 BtnOK 
&Scoped-Define DISPLAYED-OBJECTS ficSettingsFile fiSearch 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define the widget handle for the window                              */
DEFINE VAR wSettings AS WIDGET-HANDLE NO-UNDO.

/* Definitions of the field level widgets                               */
DEFINE BUTTON BtnCancel-2 AUTO-END-KEY DEFAULT 
     LABEL "Cancel" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON BtnOK AUTO-GO DEFAULT 
     LABEL "OK" 
     SIZE-PIXELS 75 BY 24
     BGCOLOR 8 .

DEFINE BUTTON btnRawEdit 
     LABEL "&Raw Edit" 
     SIZE-PIXELS 60 BY 21 TOOLTIP "direct editing of the settings file".

DEFINE BUTTON btnSettings  NO-FOCUS FLAT-BUTTON
     LABEL "" 
     SIZE-PIXELS 125 BY 35.

DEFINE BUTTON btPage1 
     LABEL "Behavior" 
     SIZE-PIXELS 125 BY 35.

DEFINE BUTTON btPage2 
     LABEL "Appearance" 
     SIZE-PIXELS 125 BY 35.

DEFINE BUTTON btPage3 
     LABEL "Backup" 
     SIZE-PIXELS 125 BY 35.

DEFINE VARIABLE ficSettingsFile AS CHARACTER FORMAT "X(256)":U 
     LABEL "Settings file" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 460 BY 21 NO-UNDO.

DEFINE VARIABLE fiSearch AS CHARACTER FORMAT "X(256)":U 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 125 BY 23
     FONT 4 NO-UNDO.

DEFINE RECTANGLE rcSettings
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE-PIXELS 610 BY 402.


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME DEFAULT-FRAME
     btnSettings AT Y 10 X 20 WIDGET-ID 24
     ficSettingsFile AT Y 15 X 215 COLON-ALIGNED WIDGET-ID 54 NO-TAB-STOP 
     btnRawEdit AT Y 15 X 690 WIDGET-ID 90
     fiSearch AT Y 64 X 10 COLON-ALIGNED NO-LABEL WIDGET-ID 16
     btPage1 AT Y 104 X 20 WIDGET-ID 8
     btPage2 AT Y 139 X 20 WIDGET-ID 14
     btPage3 AT Y 174 X 20 WIDGET-ID 10
     BtnCancel-2 AT Y 470 X 575 WIDGET-ID 98
     BtnOK AT Y 470 X 660 WIDGET-ID 94
     "CTRL-ALT-S also opens this window" VIEW-AS TEXT
          SIZE-PIXELS 240 BY 20 AT Y 475 X 15 WIDGET-ID 100
          FGCOLOR 7 
     rcSettings AT Y 60 X 150 WIDGET-ID 92
    WITH 1 DOWN NO-BOX KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 0 Y 0
         SIZE-PIXELS 761 BY 508
         DEFAULT-BUTTON BtnOK CANCEL-BUTTON BtnCancel-2 WIDGET-ID 100.

DEFINE FRAME frSettings
    WITH 1 DOWN KEEP-TAB-ORDER OVERLAY 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         AT X 154 Y 65
         SCROLLABLE SIZE-PIXELS 1600 BY 3900
         TITLE "" WIDGET-ID 200.


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
  CREATE WINDOW wSettings ASSIGN
         HIDDEN             = YES
         TITLE              = "DataDigger Settings"
         HEIGHT-P           = 510
         WIDTH-P            = 769
         MAX-HEIGHT-P       = 562
         MAX-WIDTH-P        = 769
         VIRTUAL-HEIGHT-P   = 562
         VIRTUAL-WIDTH-P    = 769
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
/* SETTINGS FOR WINDOW wSettings
  NOT-VISIBLE,,RUN-PERSISTENT                                           */
/* REPARENT FRAME */
ASSIGN FRAME frSettings:FRAME = FRAME DEFAULT-FRAME:HANDLE.

/* SETTINGS FOR FRAME DEFAULT-FRAME
   NOT-VISIBLE FRAME-NAME                                               */
ASSIGN 
       ficSettingsFile:READ-ONLY IN FRAME DEFAULT-FRAME        = TRUE.

/* SETTINGS FOR FRAME frSettings
                                                                        */
ASSIGN 
       FRAME frSettings:HEIGHT           = 18.57
       FRAME frSettings:WIDTH            = 120.

IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wSettings)
THEN wSettings:HIDDEN = yes.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME wSettings
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wSettings wSettings
ON END-ERROR OF wSettings /* DataDigger Settings */
OR ENDKEY OF {&WINDOW-NAME} ANYWHERE DO:
  /* This case occurs when the user presses the "Esc" key.
     In a persistently run window, just ignore this.  If we did not, the
     application would exit. */
  IF THIS-PROCEDURE:PERSISTENT THEN RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL wSettings wSettings
ON WINDOW-CLOSE OF wSettings /* DataDigger Settings */
or "LEAVE" of wSettings
DO:
  /* This event will close the window and terminate the procedure.  */
  APPLY "CLOSE":U TO THIS-PROCEDURE.
  RETURN NO-APPLY.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME BtnOK
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL BtnOK wSettings
ON CHOOSE OF BtnOK IN FRAME DEFAULT-FRAME /* OK */
OR GO OF wSettings ANYWHERE
DO:
  SESSION:SET-WAIT-STATE("general").
  RUN saveSettings.
  SESSION:SET-WAIT-STATE("").

  RUN saveConfigFileSorted.

  plSuccess = TRUE.
  APPLY "CLOSE" TO THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnRawEdit
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnRawEdit wSettings
ON CHOOSE OF btnRawEdit IN FRAME DEFAULT-FRAME /* Raw Edit */
DO:
  /* Start default editor for ini file */
  os-command no-wait start value( pcSettingsFile ).

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnSettings
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnSettings wSettings
ON CHOOSE OF btnSettings IN FRAME DEFAULT-FRAME
DO:
  fiSearch:screen-value = ''.
  apply 'entry' to btPage1.
  apply 'choose' to btPage1.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btPage1
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btPage1 wSettings
ON CURSOR-DOWN OF btPage1 IN FRAME DEFAULT-FRAME /* Behavior */
, fiSearch ,btPage2
DO:

  case self:name:
    when 'fiSearch' then apply 'entry' to btPage1.
    when 'btPage1'  then apply 'entry' to btPage2.
    when 'btPage2'  then apply 'entry' to btPage3.
  end case.

  return no-apply.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btPage1 wSettings
ON CURSOR-UP OF btPage1 IN FRAME DEFAULT-FRAME /* Behavior */
,btPage2, btPage3
DO:

  case self:name:
    when 'btPage1' then apply 'entry' to fiSearch.
    when 'btPage2' then apply 'entry' to btPage1.
    when 'btPage3' then apply 'entry' to btPage2.
  end case.

  return no-apply.

END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btPage1 wSettings
ON ENTRY OF btPage1 IN FRAME DEFAULT-FRAME /* Behavior */
,btPage2, btPage3
DO:

  RUN setPage( INTEGER(SUBSTRING(SELF:NAME,7,1)) ).

  RUN SetScrollPos ( INPUT FRAME frSettings:HWND
                   , INPUT 1 /* Indicates this function should operate on the vertical scrollbar attached to the frame */
                   , INPUT 1 /* Scrollbar row position */
                   , INPUT 1 /* Causes the scrollbar to be re-drawn to reflect the changed position */ 
                   ). 
  RUN PostMessageA( INPUT FRAME frSettings:HWND
                  , INPUT 277
                  , INPUT 4 + 65536 * 1
                  , INPUT 0
                  ).
END.

PROCEDURE SetScrollPos EXTERNAL "USER32.DLL": 
  DEFINE INPUT PARAMETER pHwnd   AS LONG  NO-UNDO. 
  DEFINE INPUT PARAMETER pNBar   AS SHORT NO-UNDO. 
  DEFINE INPUT PARAMETER pNPos   AS SHORT NO-UNDO. 
  DEFINE INPUT PARAMETER pRedraw AS SHORT NO-UNDO. 
END PROCEDURE.

PROCEDURE PostMessageA EXTERNAL "USER32.DLL": 
  DEFINE INPUT  PARAMETER pHwnd    AS LONG NO-UNDO.
  DEFINE INPUT  PARAMETER pMsg     AS LONG NO-UNDO.
  DEFINE INPUT  PARAMETER pWparam  AS LONG NO-UNDO.
  DEFINE INPUT  PARAMETER pLparam  AS LONG NO-UNDO.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btPage1 wSettings
ON RETURN OF btPage1 IN FRAME DEFAULT-FRAME /* Behavior */
,btPage2, btPage3
DO:

  .apply 'entry' to frame frSettings.
  
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME fiSearch
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiSearch wSettings
ON ENTRY OF fiSearch IN FRAME DEFAULT-FRAME
DO:
  if self:screen-value <> '' then
  do:
    run setPage(0).
    run showFrames(self:screen-value).
  end.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL fiSearch wSettings
ON VALUE-CHANGED OF fiSearch IN FRAME DEFAULT-FRAME
or 'return' of fiSearch
DO:
  if self:screen-value <> '' then
  do:
    run setPage(0).
    run showFrames(self:screen-value).
  end.
  else
  do:
    run setPage(giLastActivePage).
  end.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK wSettings 


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

  giWinX = active-window:x.
  giWinY = active-window:y.

  run initializeObject. /* Collect frames and set values from ini */
  apply 'entry' to btPage1.
  view wSettings.

  run showScrollBars(frame {&frame-name}:handle, no, no).
  
  wSettings:x = giWinX + 50.
  wSettings:y = giWinY + 50.

  IF NOT THIS-PROCEDURE:PERSISTENT THEN
    WAIT-FOR CLOSE OF THIS-PROCEDURE.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE collectFrames wSettings 
PROCEDURE collectFrames :
/*------------------------------------------------------------------------
  Name         : collectFrames
  Description  : Collect all frames that have been instantiated
  ----------------------------------------------------------------------*/
  
  define input parameter phParent as handle no-undo. 

  define variable hWidget as handle no-undo. 
  define buffer ttFrame for ttFrame. 
  
  if not can-query(phParent,'first-child') then return. 

  hWidget = phParent:first-child.

  do while valid-handle(hWidget):

    /* Collect frames at a lower level */
    run collectFrames(hWidget).

    if hWidget:type = 'FRAME' then
    do:

      create ttFrame.
      assign ttFrame.hFrame = hWidget
             ttFrame.cFrame = hWidget:name
             ttFrame.cTags  = 'page' + hWidget:title
             ttFrame.iOrder = integer(hWidget:title) * 1000 + hWidget:y 
             .

      hWidget:title = ?.
      hWidget:box = no.
    end.

    hWidget = hWidget:next-sibling.
  end.

end procedure. /* collectFrames */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI wSettings  _DEFAULT-DISABLE
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
  IF SESSION:DISPLAY-TYPE = "GUI":U AND VALID-HANDLE(wSettings)
  THEN DELETE WIDGET wSettings.
  IF THIS-PROCEDURE:PERSISTENT THEN DELETE PROCEDURE THIS-PROCEDURE.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI wSettings  _DEFAULT-ENABLE
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
  DISPLAY ficSettingsFile fiSearch 
      WITH FRAME DEFAULT-FRAME IN WINDOW wSettings.
  ENABLE rcSettings btnSettings ficSettingsFile btnRawEdit fiSearch btPage1 
         btPage2 btPage3 BtnCancel-2 BtnOK 
      WITH FRAME DEFAULT-FRAME IN WINDOW wSettings.
  {&OPEN-BROWSERS-IN-QUERY-DEFAULT-FRAME}
  VIEW FRAME frSettings IN WINDOW wSettings.
  {&OPEN-BROWSERS-IN-QUERY-frSettings}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject wSettings 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------
  Name         : initializeObject
  Description  : Do initialisation
  ----------------------------------------------------------------------*/
  
  define variable hWidget    as handle      no-undo.
  define variable cValue     as character   no-undo.
  define variable iMaxHeight as integer     no-undo.
  define variable iScreen    as integer     no-undo. 
  define variable hProg      as handle      no-undo extent 3.

  /* Load decoration stuff */
  do with frame {&frame-name}:
    btnSettings:load-image(getImagePath('Settings_txt.gif')).
  end.

  /* Hide all signs of the existence of the frame that holds the actual setting frames */
  frame frSettings:title = ?.
  frame frSettings:box = no.
  frame {&frame-name}:font = getFont("Default").

  /* Collect the page buttons at the left of the screen */
  hWidget = frame {&frame-name}:handle:first-child:first-child.
  do while valid-handle(hWidget):
    if hWidget:name begins 'btPage' then do:
      gcPageButtons = trim(gcPageButtons + ',' + string(hWidget),',').
      hWidget:private-data = hWidget:label. /* save original label */
    end.
    hWidget = hWidget:next-sibling.
  end.

  do iScreen = 1 to 3:
    run value(substitute('&1\wSettingsTab&2.w', getProgramDir(), iScreen)) persistent set hProg[iScreen]
      ( input frame frSettings:handle 
      , input rcSettings:handle
      ).
  end.

  /* Collect all frames in the window */
  run collectFrames(input frame frSettings:handle).

  /* process the content on the frames */
  for each ttFrame:

    assign
      iMaxHeight = 0
      hWidget    = ttFrame.hFrame:first-child:first-child.

    /* Collect all labels on the frame */
    do while valid-handle(hWidget):
      iMaxHeight = maximum(iMaxHeight, hWidget:y + hWidget:height-pixels).
      if can-set(hWidget,'font') then hWidget:font = getFont('DEFAULT').
      hWidget = hWidget:next-sibling.
    end. 

    /* Adjust height of frame */
    ttFrame.hFrame:height-pixels         = iMaxHeight + 4.
    ttFrame.hFrame:virtual-height-pixels = ttFrame.hFrame:height-pixels.
    ttFrame.hFrame:virtual-width-pixels  = ttFrame.hFrame:width-pixels.
  end.

  run enable_UI.
  run loadSettings.

  do with frame {&frame-name}:
    ficSettingsFile:screen-value = pcSettingsFile.
  end.

  /* Run local inits */
  do iScreen = 1 to 3:
    run localInitialize in hProg[iScreen] no-error.
  end.

end procedure. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE loadSettings wSettings 
PROCEDURE loadSettings :
/*------------------------------------------------------------------------
  Name         : loadSettings
  Description  : Walk the frames and load settings for all widgets
  ----------------------------------------------------------------------*/

  define variable hWidget    as handle      no-undo.
  define variable cValue     as character   no-undo.
  define variable lValue     as logical     no-undo. 
  define variable iColor     as integer     no-undo.
  define variable cSection   as character   no-undo. 
  define variable cSetting   as character   no-undo. 

  for each ttFrame:

    hWidget = ttFrame.hFrame:first-child:first-child.

    do while valid-handle(hWidget):

      /* Collect tags */
      if can-query(hWidget,'label') then
        ttFrame.cTags = substitute('&1 &2', ttFrame.cTags, hWidget:label).

      if can-query(hWidget,'tooltip') then
        ttFrame.cTags = substitute('&1 &2', ttFrame.cTags, hWidget:tooltip).

      if hWidget:type = 'literal' then
        ttFrame.cTags = substitute('&1 &2', ttFrame.cTags, hWidget:screen-value).

      if can-query(hWidget,'private-data') then
        ttFrame.cTags = substitute('&1 &2', ttFrame.cTags, entry(num-entries(hWidget:private-data),hWidget:private-data)).

      /* Get value from INI file and set it in the widget */
      if hWidget:private-data <> ? 
        and num-entries(hWidget:private-data) = 2 then
      do:
        cSection = entry(1,hWidget:private-data).
        cSetting = entry(2,hWidget:private-data).
        cValue   = getRegistry(cSection, cSetting).
        if cValue = ? then cValue = "".

        if hWidget:type = 'BUTTON' then 
        do:
          if cSection = 'DataDigger:fonts' then 
            hWidget:font = integer(cValue) no-error.
        end.

        else
        if hWidget:type = 'TOGGLE-BOX' then 
        do:
          lValue = logical(getRegistry(cSection, cSetting)) no-error.
          if lValue = ? then lValue = false.
          hWidget:checked = lValue.
        end.

        else 
        if hWidget:type = 'FILL-IN'
          and cSection = 'DataDigger:colors' then 
        do:
          /* Try to get :FG */
          iColor = getColor(cSetting + ':FG' ).
          if iColor <> ? then hWidget:fgcolor = iColor no-error.

          /* Try to get :BG */
          iColor = getColor(cSetting + ':BG' ).
          if iColor <> ? then hWidget:bgcolor = iColor no-error.

          hWidget:screen-value = cSetting.
        end.

        else
          hWidget:screen-value = cValue.

        /* For some reason, applying "VALUE-CHANGED" toggles
         * the value of the checkbox, so do it twice :)
         */
        apply "VALUE-CHANGED" to hWidget.
        apply "VALUE-CHANGED" to hWidget.
      end.

      hWidget = hWidget:next-sibling.
    end. /* f/e ttFrame */

    /* Correct tags, remove strange characters */
    ttFrame.cTags = replace(ttFrame.cTags,'&','').
    ttFrame.cTags = replace(ttFrame.cTags,'?','').
  end.

end procedure. /* loadSettings */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveSettings wSettings 
PROCEDURE saveSettings :
/*------------------------------------------------------------------------
  Name         : saveSettings
  Description  : Write settings back to the ini file
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE hWidget  AS HANDLE    NO-UNDO.
  DEFINE VARIABLE iColor   AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cSection AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cSetting AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cValue   AS CHARACTER NO-UNDO.

  FOR EACH ttFrame:

    hWidget = ttFrame.hFrame:FIRST-CHILD:FIRST-CHILD.

    DO WHILE VALID-HANDLE(hWidget):

      /* Get value from INI file and set it in the widget */
      IF hWidget:PRIVATE-DATA <> ? 
        AND NUM-ENTRIES(hWidget:PRIVATE-DATA) = 2 THEN
      DO:
        cSection = ENTRY(1,hWidget:PRIVATE-DATA).
        cSetting = ENTRY(2,hWidget:PRIVATE-DATA).

        IF hWidget:TYPE = 'BUTTON' THEN 
        DO:
          IF cSection = 'DataDigger:fonts' THEN 
            setRegistry(cSection, cSetting, STRING(hWidget:FONT)).
        END.

        ELSE 
        IF hWidget:TYPE = 'TOGGLE-BOX' THEN 
        DO:
          setRegistry(cSection, cSetting, STRING(hWidget:CHECKED)).
        END.

        ELSE 
        IF hWidget:TYPE = 'FILL-IN'
          AND cSection = 'DataDigger:colors' THEN 
        DO:
          setRegistry(cSection, cSetting + ':FG', STRING(hWidget:FGCOLOR)).
          setRegistry(cSection, cSetting + ':BG', STRING(hWidget:BGCOLOR)).
        END.

        ELSE
          setRegistry(cSection, cSetting, hWidget:SCREEN-VALUE).
      END.

      hWidget = hWidget:NEXT-SIBLING.
    END. /* f/e ttFrame */
  END. /* while valid-handle */

END PROCEDURE. /* saveSettings */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setPage wSettings 
PROCEDURE setPage :
/*------------------------------------------------------------------------
  Name         : setPage
  Description  : Show a page 
  ----------------------------------------------------------------------*/
  
  define input parameter piPageNr as integer no-undo.

  define variable hButton as handle  no-undo. 
  define variable iPage   as integer no-undo. 

  /* Remember the last active page */
  if piPageNr <> 0 then giLastActivePage = piPageNr.

  do iPage = 1 to num-entries(gcPageButtons):
    hButton = widget-handle( entry(iPage,gcPageButtons) ).

    /* Normal sizes */
    assign 
      hButton:x = 20
      hButton:y = 60 + (iPage * 35)
      hButton:width-pixels = 125
      hButton:height-pixels = 35
      hButton:label = hButton:private-data.
      .

    /* Selected button */
    if iPage = piPageNr then
    do:
      assign 
        hButton:x = hButton:x - 10
        hButton:y = hButton:y - 5
        hButton:width-pixels = hButton:width-pixels + 10
        hButton:height-pixels = hButton:height-pixels + 10
        hButton:label = caps(hButton:private-data)
        .
      hButton:move-to-top().

      run showFrames('Page' + string(piPageNr)).
    end.
  end.

end procedure. /* setPage */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showFrames wSettings 
PROCEDURE showFrames :
/*------------------------------------------------------------------------
  Name         : showFrames
  Description  : Show all subframes containing a certain tag
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER pcTag AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE iRow AS INTEGER     NO-UNDO.

  run LockWindow (input wSettings:handle, input yes).

  /* Show the first setting on y=42 */
  iRow = 15.

  /* Make the frame large enough to hold all settings */
  frame frSettings:virtual-height-pixels = 6720.
  frame frSettings:height-pixels = 390.

  /* Make frames visible based on whether the tags match */
  for each ttFrame by ttFrame.iOrder:

    ttFrame.hFrame:visible = ( ttFrame.cTags matches '*' + pcTag + '*' ).

    if ttFrame.hFrame:visible then
      assign
        ttFrame.hFrame:x = 1
        ttFrame.hFrame:y = iRow
        iRow = iRow + ttFrame.hFrame:height-pixels + 2.
  end.

  frame frSettings:width-pixels = rcSettings:width-pixels in frame {&frame-name} - 10.
  frame frSettings:virtual-height-pixels = maximum(iRow,390).

  run showScrollBars( frame frSettings:handle
                    , no
                    , (frame frSettings:virtual-height > frame frSettings:height) 
                    ).

  run LockWindow (input wSettings:handle, input no).

END PROCEDURE. /* showFrames */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE ShowScrollbars wSettings 
PROCEDURE ShowScrollbars :
/*------------------------------------------------------------------------
  Name         : showScrollbars
  Description  : Hide or show scrollbars the hard way
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER ip-Frame      AS HANDLE  NO-UNDO.
  DEFINE INPUT PARAMETER ip-horizontal AS LOGICAL NO-UNDO.
  DEFINE INPUT PARAMETER ip-vertical   AS LOGICAL NO-UNDO.

  DEFINE VARIABLE iv-retint AS INTEGER NO-UNDO.

  &scoped-define SB_HORZ 0
  &scoped-define SB_VERT 1
  &scoped-define SB_BOTH 3
  &scoped-define SB_THUMBPOSITION 4

  RUN ShowScrollBar ( ip-Frame:HWND,
                      {&SB_HORZ},
                      IF ip-horizontal THEN -1 ELSE 0,
                      OUTPUT iv-retint ).
       
  RUN ShowScrollBar ( ip-Frame:HWND, 
                      {&SB_VERT},
                      IF ip-vertical  THEN -1 ELSE 0,
                      OUTPUT iv-retint ).
  &undefine SB_HORZ
  &undefine SB_VERT
  &undefine SB_BOTH
  &undefine SB_THUMBPOSITION
    
END PROCEDURE. /* ShowScrollbars */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

