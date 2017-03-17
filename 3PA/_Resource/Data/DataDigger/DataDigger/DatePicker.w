&ANALYZE-SUSPEND _VERSION-NUMBER UIB_v9r12 GUI
&ANALYZE-RESUME
&Scoped-define WINDOW-NAME CURRENT-WINDOW
&Scoped-define FRAME-NAME frCalendarDays
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS frCalendarDays 
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

{ DataDigger.i }

/* Parameters Definitions ---                                           */
&IF "{&uib_is_running}" = "" &THEN
  DEFINE INPUT-OUTPUT PARAMETER pdCalendarDate AS DATE NO-UNDO.
&ELSE
  DEFINE VARIABLE pdCalendarDate AS DATE NO-UNDO.
&ENDIF

/* Local Variable Definitions ---                                       */
DEFINE VARIABLE gtCalendarDate  AS DATE             NO-UNDO .
DEFINE VARIABLE ghDayField      AS HANDLE EXTENT 42 NO-UNDO.
DEFINE VARIABLE ghDayName       AS HANDLE EXTENT 7  NO-UNDO.
DEFINE VARIABLE ghWeekNum       AS HANDLE EXTENT 6  NO-UNDO.
DEFINE VARIABLE gcDayNames      AS CHARACTER        NO-UNDO.
DEFINE VARIABLE gcMonthNames    AS CHARACTER        NO-UNDO.
DEFINE VARIABLE ghPrevDay       AS HANDLE           NO-UNDO.


/* --------------------------------------------------------------------------------
  Procedure     : LockWindowUpdate
  Description   : Temporarily disables window painting.
  Parameters    : - The window handle (HWND; input)
                :   0 means re-enable window painting.
-------------------------------------------------------------------------------- */
PROCEDURE LockWindowUpdate EXTERNAL "user32.dll" :
  DEFINE INPUT PARAMETER hWndLock AS LONG NO-UNDO.
END PROCEDURE. /* LockWindowUpdate */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Dialog-Box
&Scoped-define DB-AWARE no

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME frCalendarDays

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS btnNextMonth reBorderIn reBorderOut ~
reSelectedDay btnHome btnNextYear btnPrevMonth btnPrevYear 
&Scoped-Define DISPLAYED-OBJECTS fiMonth fiDayName-1 fiDayName-2 ~
fiDayName-3 fiDayName-4 fiDayName-5 fiDayName-6 fiDayName-7 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getIsoWeekday frCalendarDays 
FUNCTION getIsoWeekday RETURNS INTEGER
  ( ptDate AS DATE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getMonthName frCalendarDays 
FUNCTION getMonthName RETURNS CHARACTER
  ( INPUT ptDate AS DATE)  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getWeekNum frCalendarDays 
FUNCTION getWeekNum RETURNS INTEGER
  (ptDate AS DATE) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON btnHome 
     LABEL "Today" 
     SIZE-PIXELS 125 BY 21.

DEFINE BUTTON btnNextMonth  NO-FOCUS FLAT-BUTTON
     LABEL ">" 
     SIZE-PIXELS 13 BY 20.

DEFINE BUTTON btnNextYear  NO-FOCUS FLAT-BUTTON
     LABEL ">>" 
     SIZE-PIXELS 20 BY 20.

DEFINE BUTTON btnPrevMonth  NO-FOCUS FLAT-BUTTON
     LABEL "<" 
     SIZE-PIXELS 13 BY 20.

DEFINE BUTTON btnPrevYear  NO-FOCUS FLAT-BUTTON
     LABEL "<<" 
     SIZE-PIXELS 20 BY 20.

DEFINE VARIABLE fiDayName-1 AS CHARACTER FORMAT "XX":U INITIAL "M" 
      VIEW-AS TEXT 
     SIZE-PIXELS 13 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiDayName-2 AS CHARACTER FORMAT "XX":U INITIAL "T" 
      VIEW-AS TEXT 
     SIZE-PIXELS 14 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiDayName-3 AS CHARACTER FORMAT "XX":U INITIAL "W" 
      VIEW-AS TEXT 
     SIZE-PIXELS 14 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiDayName-4 AS CHARACTER FORMAT "XX":U INITIAL "T" 
      VIEW-AS TEXT 
     SIZE-PIXELS 14 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiDayName-5 AS CHARACTER FORMAT "XX":U INITIAL "F" 
      VIEW-AS TEXT 
     SIZE-PIXELS 14 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiDayName-6 AS CHARACTER FORMAT "XX":U INITIAL "S" 
      VIEW-AS TEXT 
     SIZE-PIXELS 14 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiDayName-7 AS CHARACTER FORMAT "XX":U INITIAL "S" 
      VIEW-AS TEXT 
     SIZE-PIXELS 14 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiMonth AS CHARACTER FORMAT "X(256)":U INITIAL "January" 
     VIEW-AS FILL-IN 
     SIZE-PIXELS 85 BY 20 NO-UNDO.

DEFINE VARIABLE fiWeek-1 AS CHARACTER FORMAT "XX":U INITIAL "01" 
      VIEW-AS TEXT 
     SIZE-PIXELS 15 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiWeek-2 AS CHARACTER FORMAT "XX":U INITIAL "02" 
      VIEW-AS TEXT 
     SIZE-PIXELS 15 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiWeek-3 AS CHARACTER FORMAT "XX":U INITIAL "03" 
      VIEW-AS TEXT 
     SIZE-PIXELS 15 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiWeek-4 AS CHARACTER FORMAT "XX":U INITIAL "04" 
      VIEW-AS TEXT 
     SIZE-PIXELS 15 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiWeek-5 AS CHARACTER FORMAT "XX":U INITIAL "05" 
      VIEW-AS TEXT 
     SIZE-PIXELS 15 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE VARIABLE fiWeek-6 AS CHARACTER FORMAT "XX":U INITIAL "06" 
      VIEW-AS TEXT 
     SIZE-PIXELS 15 BY 12
     BGCOLOR 8 FGCOLOR 1  NO-UNDO.

DEFINE RECTANGLE reBorderIn
     EDGE-PIXELS 0    
     SIZE-PIXELS 142 BY 120
     BGCOLOR 15 .

DEFINE RECTANGLE reBorderOut
     EDGE-PIXELS 0    
     SIZE-PIXELS 172 BY 148
     BGCOLOR 8 FGCOLOR 14 .

DEFINE RECTANGLE reSelectedDay
     EDGE-PIXELS 0    
     SIZE-PIXELS 20 BY 20
     BGCOLOR 1 FGCOLOR 1 .


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME frCalendarDays
     btnNextMonth AT Y 1 X 139
     btnHome AT Y 178 X 30
     fiMonth AT Y 0 X 35 COLON-ALIGNED NO-LABEL WIDGET-ID 2
     btnNextYear AT Y 1 X 152
     btnPrevMonth AT Y 1 X 20
     btnPrevYear AT Y 1 X 0
     fiDayName-1 AT Y 29 X 13 COLON-ALIGNED NO-LABEL WIDGET-ID 34
     fiDayName-2 AT Y 29 X 35 COLON-ALIGNED NO-LABEL WIDGET-ID 4
     fiDayName-3 AT Y 29 X 57 COLON-ALIGNED NO-LABEL WIDGET-ID 6
     fiDayName-4 AT Y 29 X 79 COLON-ALIGNED NO-LABEL WIDGET-ID 8
     fiDayName-5 AT Y 29 X 101 COLON-ALIGNED NO-LABEL WIDGET-ID 10
     fiDayName-6 AT Y 29 X 125 COLON-ALIGNED NO-LABEL WIDGET-ID 12
     fiDayName-7 AT Y 29 X 147 COLON-ALIGNED NO-LABEL WIDGET-ID 14
     fiWeek-1 AT Y 49 X 2 NO-LABEL WIDGET-ID 16
     fiWeek-2 AT Y 69 X 2 NO-LABEL WIDGET-ID 18
     fiWeek-3 AT Y 89 X 2 NO-LABEL WIDGET-ID 20
     fiWeek-4 AT Y 110 X 2 NO-LABEL WIDGET-ID 22
     fiWeek-5 AT Y 130 X 2 NO-LABEL WIDGET-ID 24
     fiWeek-6 AT Y 150 X 3 NO-LABEL WIDGET-ID 26
     reBorderIn AT Y 45 X 18 WIDGET-ID 28
     reBorderOut AT Y 25 X 0 WIDGET-ID 30
     reSelectedDay AT Y 100 X 105 WIDGET-ID 32
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D 
         SIZE-PIXELS 186 BY 233
         TITLE "Calendar".


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
/* SETTINGS FOR DIALOG-BOX frCalendarDays
   FRAME-NAME                                                           */
ASSIGN 
       FRAME frCalendarDays:SCROLLABLE       = FALSE
       FRAME frCalendarDays:HIDDEN           = TRUE.

/* SETTINGS FOR FILL-IN fiDayName-1 IN FRAME frCalendarDays
   NO-ENABLE                                                            */
ASSIGN 
       fiDayName-1:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiDayName-2 IN FRAME frCalendarDays
   NO-ENABLE                                                            */
ASSIGN 
       fiDayName-2:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiDayName-3 IN FRAME frCalendarDays
   NO-ENABLE                                                            */
ASSIGN 
       fiDayName-3:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiDayName-4 IN FRAME frCalendarDays
   NO-ENABLE                                                            */
ASSIGN 
       fiDayName-4:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiDayName-5 IN FRAME frCalendarDays
   NO-ENABLE                                                            */
ASSIGN 
       fiDayName-5:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiDayName-6 IN FRAME frCalendarDays
   NO-ENABLE                                                            */
ASSIGN 
       fiDayName-6:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiDayName-7 IN FRAME frCalendarDays
   NO-ENABLE                                                            */
ASSIGN 
       fiDayName-7:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiMonth IN FRAME frCalendarDays
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN fiWeek-1 IN FRAME frCalendarDays
   NO-DISPLAY NO-ENABLE ALIGN-L                                         */
ASSIGN 
       fiWeek-1:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiWeek-2 IN FRAME frCalendarDays
   NO-DISPLAY NO-ENABLE ALIGN-L                                         */
ASSIGN 
       fiWeek-2:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiWeek-3 IN FRAME frCalendarDays
   NO-DISPLAY NO-ENABLE ALIGN-L                                         */
ASSIGN 
       fiWeek-3:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiWeek-4 IN FRAME frCalendarDays
   NO-DISPLAY NO-ENABLE ALIGN-L                                         */
ASSIGN 
       fiWeek-4:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiWeek-5 IN FRAME frCalendarDays
   NO-DISPLAY NO-ENABLE ALIGN-L                                         */
ASSIGN 
       fiWeek-5:READ-ONLY IN FRAME frCalendarDays        = TRUE.

/* SETTINGS FOR FILL-IN fiWeek-6 IN FRAME frCalendarDays
   NO-DISPLAY NO-ENABLE ALIGN-L                                         */
ASSIGN 
       fiWeek-6:READ-ONLY IN FRAME frCalendarDays        = TRUE.

ASSIGN 
       reSelectedDay:HIDDEN IN FRAME frCalendarDays           = TRUE.

/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME frCalendarDays
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frCalendarDays frCalendarDays
ON CURSOR-DOWN OF FRAME frCalendarDays /* Calendar */
ANYWHERE DO:
  RUN setDate(gtCalendarDate + 7).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frCalendarDays frCalendarDays
ON CURSOR-LEFT OF FRAME frCalendarDays /* Calendar */
ANYWHERE DO:
  run setDate(gtCalendarDate - 1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frCalendarDays frCalendarDays
ON CURSOR-RIGHT OF FRAME frCalendarDays /* Calendar */
ANYWHERE DO:
  run setDate(gtCalendarDate + 1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frCalendarDays frCalendarDays
ON CURSOR-UP OF FRAME frCalendarDays /* Calendar */
ANYWHERE DO:
  run setDate(gtCalendarDate - 7).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frCalendarDays frCalendarDays
ON END OF FRAME frCalendarDays /* Calendar */
ANYWHERE DO:
  run btnEndChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frCalendarDays frCalendarDays
ON HOME OF FRAME frCalendarDays /* Calendar */
ANYWHERE DO:
  run btnHomeChoose.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL frCalendarDays frCalendarDays
ON WINDOW-CLOSE OF FRAME frCalendarDays /* Calendar */
DO:
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnHome
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnHome frCalendarDays
ON CHOOSE OF btnHome IN FRAME frCalendarDays /* Today */
OR "CTRL-HOME" OF FRAME {&FRAME-NAME} ANYWHERE
DO:
  RUN setDate(TODAY).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnNextMonth
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnNextMonth frCalendarDays
ON CHOOSE OF btnNextMonth IN FRAME frCalendarDays /* > */
OR "PAGE-DOWN" OF FRAME {&FRAME-NAME} 
OR "ctrl-cursor-down" OF FRAME {&FRAME-NAME} ANYWHERE
DO:
  RUN changeMonth(gtCalendarDate, +1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnNextYear
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnNextYear frCalendarDays
ON CHOOSE OF btnNextYear IN FRAME frCalendarDays /* >> */
OR "CTRL-PAGE-DOWN" of frame {&frame-name} anywhere
DO:
  RUN changeYear(gtCalendarDate, +1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnPrevMonth
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnPrevMonth frCalendarDays
ON CHOOSE OF btnPrevMonth IN FRAME frCalendarDays /* < */
OR "PAGE-UP" OF FRAME {&FRAME-NAME} 
OR "ctrl-cursor-up" OF FRAME {&FRAME-NAME} ANYWHERE
DO:
  RUN changeMonth(gtCalendarDate, -1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btnPrevYear
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btnPrevYear frCalendarDays
ON CHOOSE OF btnPrevYear IN FRAME frCalendarDays /* << */
OR "CTRL-PAGE-UP" of frame {&frame-name} anywhere
DO:
  RUN changeYear(gtCalendarDate, -1).
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK frCalendarDays 


/* ***************************  Main Block  *************************** */

/* Parent the dialog-box to the ACTIVE-WINDOW, if there is no parent.   */
IF VALID-HANDLE(ACTIVE-WINDOW) AND FRAME {&FRAME-NAME}:PARENT eq ?
THEN FRAME {&FRAME-NAME}:PARENT = ACTIVE-WINDOW.

/* Now enable the interface and wait for the exit condition.            */
/* (NOTE: handle ERROR and END-KEY so cleanup code will always fire.    */
MAIN-BLOCK:
DO ON ERROR   UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK
   ON END-KEY UNDO MAIN-BLOCK, LEAVE MAIN-BLOCK:

  /* Freeze window */
  RUN LockWindowUpdate IN THIS-PROCEDURE (INPUT {&WINDOW-NAME}:HWND).  

  RUN enable_UI.
  RUN initializeObject.

  /* Unfreeze window */
  RUN LockWindowUpdate IN THIS-PROCEDURE (INPUT 0).  

  WAIT-FOR GO OF FRAME {&FRAME-NAME} OR RETURN OF FRAME {&FRAME-NAME}.
  IF KEYFUNCTION(LASTKEY) = "RETURN" THEN RUN btnOkChoose.

END.
RUN disable_UI.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnEndChoose frCalendarDays 
PROCEDURE btnEndChoose :
/*------------------------------------------------------------------------
  Name : btnEndChoose
  Desc : Jump to end of month
  ---------------------------------------------------------------------- */

  DEFINE VARIABLE dtNewDate AS DATE NO-UNDO.
  
  IF DAY(gtCalendarDate + 1) = 1 THEN
    RUN setDate (TODAY).
  ELSE
  DO:
    /* First jump to the first day of current month */
    dtNewDate = DATE(MONTH(gtCalendarDate),1,YEAR(gtCalendarDate)).
  
    /* Then add some days so that we end in the next month */
    dtNewDate = dtNewDate + 40.
  
    /* Then subtract just enough to end on the last day of the current month */
    dtNewDate = dtNewDate - DAY(dtNewDate).
  
    RUN setDate(dtNewDate).
  END.

END PROCEDURE. /* btnEndChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnHomeChoose frCalendarDays 
PROCEDURE btnHomeChoose :
/*------------------------------------------------------------------------
  Name : btnHomeChoose
  Desc : Jump to either today or first of month
  ---------------------------------------------------------------------- */

  IF DAY(gtCalendarDate) = 1 THEN
    RUN setDate (TODAY).
  ELSE 
    RUN setDate(INPUT DATE(MONTH(gtCalendarDate),1,YEAR(gtCalendarDate) )).

END PROCEDURE. /* btnHomeChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE btnOkChoose frCalendarDays 
PROCEDURE btnOkChoose :
/*------------------------------------------------------------------------
  Name : btnOkChoose
  Desc : Pass back date to calling program
  ---------------------------------------------------------------------- */

  pdCalendarDate = gtCalendarDate.
  APPLY 'GO' TO FRAME {&FRAME-NAME}.

END PROCEDURE. /* btnOkChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE changeMonth frCalendarDays 
PROCEDURE changeMonth :
/*------------------------------------------------------------------------
  Name : changeMonth
  Desc : Change year of the current calendar date
  ---------------------------------------------------------------------- */
    
  DEFINE INPUT PARAMETER ptBaseDate  AS DATE    NO-UNDO. 
  DEFINE INPUT PARAMETER piDeltaDays AS INTEGER NO-UNDO. 

  DEFINE VARIABLE tNewDate       AS DATE    NO-UNDO. 
  DEFINE VARIABLE iDayCorrection AS INTEGER NO-UNDO. 

  ASSIGN 
    tNewDate = DATE(MONTH(ptBaseDate),15,YEAR(ptBaseDate))+ (20 * piDeltaDays).

  /* Correct day nr, for days that have less than 31 days */
  DO iDayCorrection = 0 TO 3:
    ASSIGN 
      tNewDate = DATE(MONTH(tNewDate), DAY(ptBaseDate) - iDayCorrection, YEAR(tNewDate)) NO-ERROR.

    IF NOT ERROR-STATUS:ERROR THEN LEAVE. 
  END.

  RUN setDate(INPUT tNewDate).
  
END PROCEDURE. /* changeMonth */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE changeYear frCalendarDays 
PROCEDURE changeYear :
/*------------------------------------------------------------------------
  Name : changeYear
  Desc : Change year of the current calendar date
  ---------------------------------------------------------------------- */
  
  DEFINE INPUT PARAMETER ptBaseDate   AS DATE    NO-UNDO. 
  DEFINE INPUT PARAMETER piChangeYear AS INTEGER NO-UNDO. 

  DEFINE VARIABLE tNewDate       AS DATE    NO-UNDO. 
  DEFINE VARIABLE iDayCorrection AS INTEGER NO-UNDO. 

  /* Avoid ending up in the year 0 AD */
  IF YEAR(ptBaseDate) + piChangeYear = 0 THEN piChangeYear = piChangeYear * 2.

  /* Correct day nr in february for non-leap years */
  DO iDayCorrection = 0 TO 1:
    ASSIGN 
      tNewDate = DATE(MONTH(ptBaseDate), DAY(ptBaseDate) - iDayCorrection, YEAR(ptBaseDate) + pichangeYear) NO-ERROR.
    IF NOT ERROR-STATUS:ERROR THEN LEAVE. 
  END.

  RUN setDate(INPUT tNewDate).
  
END PROCEDURE. /* changeYear */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI frCalendarDays  _DEFAULT-DISABLE
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
  HIDE FRAME frCalendarDays.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE drawCalendar frCalendarDays 
PROCEDURE drawCalendar :
/*------------------------------------------------------------------------
  Name : drawCalendar
  Desc : Build the screen for a specific month. 
  ---------------------------------------------------------------------- */
  
  DEFINE INPUT PARAMETER ptNewDate AS DATE NO-UNDO.

  DEFINE VARIABLE hDayField       AS HANDLE    EXTENT 31 NO-UNDO.
  DEFINE VARIABLE iNumDaysInMonth AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iDayNr          AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iDayField       AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iCol            AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iRow            AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iStartWeekday   AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iWeekNum        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iWeekDay        AS INTEGER   NO-UNDO.
  DEFINE VARIABLE tTempDate       AS DATE      NO-UNDO.

  /* Init */
  ASSIGN 
    gtCalendarDate  = ptNewDate
    iStartWeekday   = getIsoWeekday(DATE(MONTH(gtCalendarDate),1,YEAR(gtCalendarDate))) 
    tTempDate       = DATE(MONTH(gtCalendarDate),28,YEAR(gtCalendarDate) ) + 10
    iNumDaysInMonth = DAY(tTempDate - DAY(tTempDate))
    iWeekDay        = iStartWeekday
    iRow            = 1
    iCol            = iWeekDay
    hDayField       = ?
    .

  /* Hide fields before the first of month */
  DO iDayNr = 1 TO iWeekDay - 1:
    ASSIGN ghDayField[iDayNr]:VISIBLE = FALSE.
  END.

  /* Hide week numbers */
  DO iWeekNum = 1 TO 6:
    ASSIGN ghWeekNum[iWeekNum]:SCREEN-VALUE = ''.
  END.

  /* Draw days of the month */
  DO iDayNr = 1 TO iNumDaysInMonth:

    /* Get nr in the array for this day */
    iDayField = (iRow - 1) * 7 + iCol.
    hDayField[iDayNr] = ghDayField[iDayField].

    /* And get date for this field */
    tTempDate = DATE(MONTH(gtCalendarDate),iDayNr,YEAR(gtCalendarDate)).

    /* Get weeknr for this row. */
    IF ghWeekNum[iRow]:SCREEN-VALUE = '' THEN 
      ASSIGN ghWeekNum[iRow]:SCREEN-VALUE = STRING(getWeekNum(tTempDate)).

    /* Settings of the day itself */
    ASSIGN
      hDayField[iDayNr]:VISIBLE      = FALSE
      hDayField[iDayNr]:LABEL        = STRING(iDayNr,'Z9')
      hDayField[iDayNr]:PRIVATE-DATA = STRING(tTempDate)
      hDayField[iDayNr]:VISIBLE      = TRUE
      hDayField[iDayNr]:SENSITIVE    = TRUE 
      .

    /* Set focus on date */
    ON 'choose':u OF hDayField[iDayNr] PERSISTENT
      RUN setDate(INPUT tTempDate ).

    /* Select this date 
     OR 'return':u OF FRAME frCalendarDays ANYWHERE
    */
    ON 'mouse-select-click' OF hDayField[iDayNr] PERSISTENT
      RUN selectDate(INPUT tTempDate ).

    /* Go to next row */
    iCol = iCol + 1.
    IF iCol = 8 THEN
    DO:
      iRow = iRow + 1.
      iCol = 1.
    END.
  END.

  /* Hide fields past end of month */
  DO iDayNr = iDayField + 1 TO 42:
    ASSIGN ghDayField[iDayNr]:VISIBLE = FALSE.
  END.

  /* Place month, date and year in screen */
  DO WITH FRAME {&FRAME-NAME}:
    fiMonth:SCREEN-VALUE = SUBSTITUTE("&1 &2", getMonthName(gtCalendarDate),YEAR(gtCalendarDate)).
    fiMonth:WIDTH-PIXELS = FONT-TABLE:GET-TEXT-WIDTH-PIXELS(fiMonth:SCREEN-VALUE,fiMonth:FONT) + 10.
    fiMonth:X = (reBorderOut:WIDTH-PIXELS - fiMonth:WIDTH-PIXELS) / 2.
  
  END.

END PROCEDURE. /* drawCalendar */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI frCalendarDays  _DEFAULT-ENABLE
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
  DISPLAY fiMonth fiDayName-1 fiDayName-2 fiDayName-3 fiDayName-4 fiDayName-5 
          fiDayName-6 fiDayName-7 
      WITH FRAME frCalendarDays.
  ENABLE btnNextMonth reBorderIn reBorderOut reSelectedDay btnHome btnNextYear 
         btnPrevMonth btnPrevYear 
      WITH FRAME frCalendarDays.
  VIEW FRAME frCalendarDays.
  {&OPEN-BROWSERS-IN-QUERY-frCalendarDays}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject frCalendarDays 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------
  Name : InitializeObject
  Desc : Initialize. What else did you expect? :)
  ---------------------------------------------------------------------- */

  DEFINE VARIABLE hWidget   AS HANDLE  NO-UNDO.
  DEFINE VARIABLE iButtonNr AS INTEGER NO-UNDO.
  DEFINE VARIABLE iRowNr    AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iColNr    AS INTEGER NO-UNDO. 
  DEFINE VARIABLE tTempDate AS DATE    NO-UNDO.

  ASSIGN 
    gcDayNames   = 'Sunday,Monday,Tueday,Wednesday,Thursday,Friday,Saturday'
    gcMonthNames = 'January,February,March,April,May,June,July,August,September,October,November,December'
    .

  DO WITH FRAME {&FRAME-NAME}:

    /* Get fonts */
    FRAME {&FRAME-NAME}:FONT = getFont('Default').
    FRAME {&FRAME-NAME}:VISIBLE = FALSE. 

    /* Get handles of week- and day labels. */
    hWidget = FRAME frCalendarDays:FIRST-CHILD:FIRST-CHILD.

    DO WHILE VALID-HANDLE(hWidget):
      IF NUM-ENTRIES(hWidget:NAME,'-') = 2 THEN
      DO:
        iButtonNr = INTEGER(ENTRY(2,hWidget:NAME,'-')).
  
        CASE ENTRY(1,hWidget:NAME,'-'):
          WHEN 'fiWeek'    THEN ghWeekNum[iButtonNr] = hWidget.
          WHEN 'fiDayName' THEN ghDayName[iButtonNr] = hWidget.
        END CASE.
      END.

      hWidget = hWidget:NEXT-SIBLING.
    END.

    ASSIGN iButtonNr = 0.

    /* Yeah, this is what we call semi-dynamic :) */
    &GLOBAL-DEFINE BUTTON-WIDTH       18
    &GLOBAL-DEFINE BUTTON-HEIGHT      18
    &GLOBAL-DEFINE BUTTON-HOR-SPACE    4
    &GLOBAL-DEFINE BUTTON-VER-SPACE    3 
    &GLOBAL-DEFINE BUTTON-HOR-OFFSET  20
    &GLOBAL-DEFINE BUTTON-VER-OFFSET  45 
    
    /* Set canvas size */
    FRAME frCalendarDays:WIDTH-PIXELS  = {&BUTTON-HOR-OFFSET} + 7 * ({&BUTTON-WIDTH}  + {&BUTTON-HOR-SPACE} ) + 10.
    FRAME frCalendarDays:HEIGHT-PIXELS = {&BUTTON-VER-OFFSET} + 6 * ({&BUTTON-HEIGHT} + {&BUTTON-VER-SPACE} ) + 60.

    reBorderOut:WIDTH-PIXELS  = {&BUTTON-HOR-OFFSET} + 7 * ({&BUTTON-WIDTH}  + {&BUTTON-HOR-SPACE} ).
    reBorderOut:HEIGHT-PIXELS = 7 * ({&BUTTON-HEIGHT} + {&BUTTON-VER-SPACE} ).

    /* Correct position of the day name */
    DO iColNr = 0 TO 6:
      ghDayName[iColNr + 1]:X = {&BUTTON-HOR-OFFSET} + 5 + iColNr * ({&BUTTON-WIDTH} + {&BUTTON-HOR-SPACE} ).
    END.

    /* Correct position of the week nr */
    DO iRowNr = 0 TO 5:
      ghWeekNum[iRowNr + 1]:Y = {&BUTTON-VER-OFFSET} + 2 + iRowNr * ({&BUTTON-HEIGHT} + {&BUTTON-VER-SPACE} ).
    END.

    /* Center home button */
    btnHome:X = (FRAME frCalendarDays:WIDTH-PIXELS - btnHome:WIDTH-PIXELS) / 2.
    btnHome:Y = reBorderOut:Y + reBorderOut:HEIGHT-PIXELS + ({&BUTTON-VER-SPACE} * 2).

    /* Center top row */
    btnNextYear:X  = (reBorderOut:X + reBorderOut:WIDTH-PIXELS) - btnNextYear:WIDTH-PIXELS.
    btnNextMonth:X = (btnNextYear:X - btnNextMonth:WIDTH-PIXELS).

    DO iRowNr = 0 TO 5:
      DO iColNr = 0 TO 6:

        ASSIGN iButtonNr = iButtonNr + 1.

        CREATE BUTTON ghDayField[iButtonNr]
          ASSIGN
          X              = {&BUTTON-HOR-OFFSET} + iColNr * ({&BUTTON-WIDTH}  + {&BUTTON-HOR-SPACE} )
          Y              = {&BUTTON-VER-OFFSET} + iRowNr * ({&BUTTON-HEIGHT} + {&BUTTON-VER-SPACE} )
          WIDTH-PIXELS   = {&BUTTON-WIDTH}
          HEIGHT-PIXELS  = {&BUTTON-HEIGHT}
          FRAME          = FRAME frCalendarDays:HANDLE
          VISIBLE        = NO
          SENSITIVE      = NO
          FONT           = getFont('Default')
          FLAT-BUTTON    = TRUE
          NO-FOCUS       = TRUE
          .
      END. /* col */
    END. /* row */

    FRAME {&FRAME-NAME}:VISIBLE = TRUE. 
  END. /* frame */

  /* Init on date from caller */
  tTempDate = pdCalendarDate.

  /* Don't accept rubbish */
  IF tTempDate = ? THEN tTempDate = TODAY.
  
  /* Set calendar to this day */
  RUN setDate(tTempDate).

END PROCEDURE. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE selectDate frCalendarDays 
PROCEDURE selectDate :
/*------------------------------------------------------------------------
  Name : selectDate
  Desc : Give back the selected date to the calling program
  ---------------------------------------------------------------------- */
  DEFINE INPUT PARAMETER ptDate AS DATE NO-UNDO.

  RUN setDate( ptDate ).
  RUN btnOkChoose.

END PROCEDURE. /* selectDate */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setDate frCalendarDays 
PROCEDURE setDate :
/*------------------------------------------------------------------------
  Name : setDate
  Desc : Point the calendar to a specific date. If needed, redraw screen
  ---------------------------------------------------------------------- */
  DEFINE INPUT PARAMETER ptNewDate AS DATE NO-UNDO.

  DEFINE VARIABLE hToday    AS HANDLE  NO-UNDO.
  DEFINE VARIABLE iDayField AS INTEGER NO-UNDO.

  /* Check if screen must be rebuilt */
  IF MONTH(ptNewDate) <> MONTH(gtCalendarDate)
    OR YEAR(ptNewDate) <> YEAR(gtCalendarDate) THEN 
    RUN drawCalendar(ptNewDate ).

  /* Set global date */
  ASSIGN gtCalendarDate = ptNewDate.

  /* Find today's button */
  DO iDayField = 1 TO 42:
    IF ghDayField[iDayField]:PRIVATE-DATA = STRING(ptNewDate) THEN
      hToday = ghDayField[iDayField].
  END.

  /* Deselect previous date */
  IF VALID-HANDLE(ghPrevDay) THEN
    ASSIGN
      ghPrevDay:BGCOLOR = 15
      ghPrevDay:FGCOLOR = ?
      .

  /* Remember current date */
  ASSIGN ghPrevDay = hToday.

  /* Move selection rectangle */
  DO WITH FRAME frCalendarDays:
    reSelectedDay:X = hToday:X - 1.
    reSelectedDay:Y = hToday:Y - 1.
  END.

  /* Set focus on this day */
  APPLY 'entry' TO FRAME frCalendarDays.

END PROCEDURE. /* setDate */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

/* ************************  Function Implementations ***************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getIsoWeekday frCalendarDays 
FUNCTION getIsoWeekday RETURNS INTEGER
  ( ptDate AS DATE ) :

  DEFINE VARIABLE iIsoWeekday AS INTEGER  EXTENT 7 NO-UNDO INITIAL [7,1,2,3,4,5,6].

  RETURN iIsoWeekday[WEEKDAY(ptDate)].

END FUNCTION. /* getIsoWeekday */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getMonthName frCalendarDays 
FUNCTION getMonthName RETURNS CHARACTER
  ( INPUT ptDate AS DATE) :

  /*------------------------------------------------------------------------
    Name : getMonthName
    Desc : Get name of month
    ---------------------------------------------------------------------- */

  RETURN ENTRY(MONTH(ptDate),gcMonthNames).

END FUNCTION. /* getMonthName */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getWeekNum frCalendarDays 
FUNCTION getWeekNum RETURNS INTEGER
  (ptDate AS DATE):
  /*
   * getWeekNum
   * 
   * Returns the ISO week number
   * Don't use the progress function WEEKDAY, as that 
   * starts on Sunday when the ISO weeks start on Monday! 
   */
  DEFINE VARIABLE v_Days         AS INTEGER   NO-UNDO.
  DEFINE VARIABLE v_WeekMonday   AS DATE      NO-UNDO.
  DEFINE VARIABLE v_WeekNumber   AS INTEGER   NO-UNDO.
  DEFINE VARIABLE v_WeekThursday AS DATE      NO-UNDO.

  v_WeekMonday    = ptDate - getIsoWeekday(ptDate) + 1.    /* Start of week */
  v_WeekThursday  = v_WeekMonday + 3.

  v_Days = v_WeekMonday - DATE(1,1,YEAR(ptDate)).   /* get number of days for monday*/
  v_WeekNumber = INTEGER(ROUND(v_Days / 7,0)) + 1.     /* calculate week number */

  IF v_WeekNumber = 53 THEN 
  DO: /* work where the overlapping week sits */
    IF YEAR(v_WeekThursday) = YEAR(ptDate)
      THEN v_WeekNumber = 53.
      ELSE v_WeekNumber = 1.
  END.
  IF v_WeekNumber = 0 THEN v_WeekNumber = 53.

  RETURN v_WeekNumber.

END FUNCTION. /* getWeekNum */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

