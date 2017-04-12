&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Procedure 
/*------------------------------------------------------------------------
  Name         : DataDigger.p
  Description  : Launcher for DataDigger.
  ---------------------------------------------------------------------- 
  15-10-2009 pti Created
  ----------------------------------------------------------------------*/

/* Buildnr, temp-tables and forward defs */
{ DataDigger.i }

PROCEDURE GetUserNameA EXTERNAL "ADVAPI32.DLL":
  DEFINE INPUT        PARAMETER mUserId       AS MEMPTR NO-UNDO.
  DEFINE INPUT-OUTPUT PARAMETER intBufferSize AS LONG NO-UNDO.
  DEFINE RETURN       PARAMETER intResult     AS SHORT NO-UNDO.
END PROCEDURE.

PROCEDURE GetKeyboardState EXTERNAL "user32.dll":
    DEFINE INPUT  PARAMETER KBState AS LONG. /* memptr */ 
    DEFINE RETURN PARAMETER RetVal  AS LONG. /* bool   */ 
END PROCEDURE.

/* Windows API entry point */
PROCEDURE ShowScrollBar EXTERNAL "user32.dll":
  DEFINE INPUT  PARAMETER hwnd        AS LONG.
  DEFINE INPUT  PARAMETER fnBar       AS LONG.
  DEFINE INPUT  PARAMETER fShow       AS LONG.
  DEFINE RETURN PARAMETER ReturnValue AS LONG.
END PROCEDURE.

PROCEDURE SendMessageA EXTERNAL "user32.dll":
  DEFINE INPUT  PARAMETER hwnd   AS long NO-UNDO.
  DEFINE INPUT  PARAMETER wmsg   AS long NO-UNDO.
  DEFINE INPUT  PARAMETER wparam AS long NO-UNDO.
  DEFINE INPUT  PARAMETER lparam AS long NO-UNDO.
  DEFINE RETURN PARAMETER rc     AS long NO-UNDO.
END PROCEDURE.

PROCEDURE RedrawWindow EXTERNAL "user32.dll":
  DEFINE INPUT PARAMETER v-hwnd  AS LONG NO-UNDO.
  DEFINE INPUT PARAMETER v-rect  AS LONG NO-UNDO.
  DEFINE INPUT PARAMETER v-rgn   AS LONG NO-UNDO.
  DEFINE INPUT PARAMETER v-flags AS LONG NO-UNDO.
  DEFINE RETURN PARAMETER v-ret  AS LONG NO-UNDO.
END PROCEDURE.

PROCEDURE SetWindowTextA EXTERNAL "user32.dll":
  DEFINE INPUT PARAMETER hwnd AS long.
  DEFINE INPUT PARAMETER txt AS CHARACTER.
END PROCEDURE.

PROCEDURE GetWindow EXTERNAL "user32.dll" :
  DEFINE INPUT PARAMETER hwnd AS LONG.
  DEFINE INPUT PARAMETER uCmd AS LONG.
  DEFINE RETURN PARAMETER hwndOther AS LONG.
END PROCEDURE.

PROCEDURE GetParent EXTERNAL "user32.dll" :
  DEFINE INPUT PARAMETER hwndChild AS LONG.
  DEFINE RETURN PARAMETER hwndParent AS LONG.
END PROCEDURE.

PROCEDURE GetCursorPos EXTERNAL "user32.dll" : 
  DEFINE INPUT-OUTPUT PARAMETER lRect AS MEMPTR. 
END. 

PROCEDURE GetSysColor EXTERNAL "user32.dll":
  DEFINE INPUT PARAMETER nDspElement AS LONG.
  DEFINE RETURN PARAMETER COLORREF AS LONG.
END.

PROCEDURE ScreenToClient EXTERNAL "user32.dll" :
  DEFINE INPUT  PARAMETER hWnd     AS LONG.
  DEFINE INPUT  PARAMETER lpPoint  AS MEMPTR.
END PROCEDURE.

/* Transparency */
PROCEDURE SetWindowLongA EXTERNAL "user32.dll":
  DEFINE INPUT PARAMETER HWND AS LONG.
  DEFINE INPUT PARAMETER nIndex AS LONG.
  DEFINE INPUT PARAMETER dwNewLong AS LONG.
  DEFINE RETURN PARAMETER stat AS LONG.
END.

PROCEDURE SetLayeredWindowAttributes EXTERNAL "user32.dll":
 DEFINE INPUT PARAMETER HWND AS LONG.
 DEFINE INPUT PARAMETER crKey AS LONG.
 DEFINE INPUT PARAMETER bAlpha AS SHORT.
 DEFINE INPUT PARAMETER dwFlagsas AS LONG.
 DEFINE RETURN PARAMETER stat AS SHORT.
END.


/* Find out if a file is locked */
&GLOBAL-DEFINE GENERIC_WRITE         1073741824 /* &H40000000 */
&GLOBAL-DEFINE OPEN_EXISTING         3
&GLOBAL-DEFINE FILE_SHARE_READ       1          /* = &H1 */
&GLOBAL-DEFINE FILE_ATTRIBUTE_NORMAL 128        /* = &H80 */

PROCEDURE CreateFileA EXTERNAL "kernel32":
  DEFINE INPUT PARAMETER lpFileName AS CHARACTER.
  DEFINE INPUT PARAMETER dwDesiredAccess AS LONG.
  DEFINE INPUT PARAMETER dwShareMode AS LONG.
  DEFINE INPUT PARAMETER lpSecurityAttributes AS LONG.
  DEFINE INPUT PARAMETER dwCreationDisposition AS LONG.
  DEFINE INPUT PARAMETER dwFlagsAndAttributes AS LONG.
  DEFINE INPUT PARAMETER hTemplateFile AS LONG.
  DEFINE RETURN PARAMETER ReturnValue AS LONG.
END PROCEDURE.

PROCEDURE CloseHandle EXTERNAL "kernel32" :
  DEFINE INPUT  PARAMETER hObject     AS LONG.
  DEFINE RETURN PARAMETER ReturnValue AS LONG.
END PROCEDURE.

DEFINE TEMP-TABLE ttWidget NO-UNDO RCODE-INFORMATION
  FIELD hWidget   AS HANDLE
  FIELD iPosX     AS INTEGER
  FIELD iWidth    AS INTEGER
  INDEX iPrim AS PRIMARY hWidget.

/* If you have trouble with the cache, disable it in the settings screen */
DEFINE VARIABLE glCacheSettings  AS LOGICAL NO-UNDO.
DEFINE VARIABLE glCacheTableDefs AS LOGICAL NO-UNDO.
DEFINE VARIABLE glCacheFieldDefs AS LOGICAL NO-UNDO.

/* Locking / unlocking windows */
&GLOBAL-DEFINE WM_SETREDRAW     11
&GLOBAL-DEFINE RDW_ALLCHILDREN 128
&GLOBAL-DEFINE RDW_ERASE         4
&GLOBAL-DEFINE RDW_INVALIDATE    1

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE no



/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&IF DEFINED(EXCLUDE-addConnection) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD addConnection Procedure 
FUNCTION addConnection RETURNS LOGICAL
  ( pcDatabase AS CHARACTER 
  , pcSection  AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-formatQueryString) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD formatQueryString Procedure 
FUNCTION formatQueryString RETURNS CHARACTER
  ( INPUT pcQueryString AS CHARACTER
  , INPUT plExpanded    AS LOGICAL )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColor) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getColor Procedure 
FUNCTION getColor RETURNS INTEGER
  ( pcName AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColumnLabel) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getColumnLabel Procedure 
FUNCTION getColumnLabel RETURNS CHARACTER
  ( INPUT phFieldBuffer AS HANDLE ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColumnWidthList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getColumnWidthList Procedure 
FUNCTION getColumnWidthList RETURNS CHARACTER
  ( INPUT phBrowse AS HANDLE ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getDatabaseList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getDatabaseList Procedure 
FUNCTION getDatabaseList RETURNS CHARACTER FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getEscapedData) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getEscapedData Procedure 
FUNCTION getEscapedData RETURNS CHARACTER
  ( pcTarget AS CHARACTER
  , pcString AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getFileCategory) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getFileCategory Procedure 
FUNCTION getFileCategory RETURNS CHARACTER
  ( piFileNumber AS INTEGER
  , pcFileName   AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getFont) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getFont Procedure 
FUNCTION getFont RETURNS INTEGER
  ( pcFontName AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getImagePath) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getImagePath Procedure 
FUNCTION getImagePath RETURNS CHARACTER
  ( pcImage AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getIndexFields) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getIndexFields Procedure 
FUNCTION getIndexFields RETURNS CHARACTER
  ( INPUT pcDatabaseName AS CHARACTER
  , INPUT pcTableName    AS CHARACTER  
  , INPUT pcFlags        AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getKeyList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getKeyList Procedure 
FUNCTION getKeyList RETURNS CHARACTER
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getLinkInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getLinkInfo Procedure 
FUNCTION getLinkInfo RETURNS CHARACTER
  ( INPUT pcFieldName AS CHARACTER
  ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMatchesValue) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getMatchesValue Procedure 
FUNCTION getMatchesValue RETURNS CHARACTER
  ( hFillIn AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMaxLength) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getMaxLength Procedure 
FUNCTION getMaxLength RETURNS INTEGER
  ( cFieldList AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getOsErrorDesc) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getOsErrorDesc Procedure 
FUNCTION getOsErrorDesc RETURNS CHARACTER
  (INPUT piOsError AS INTEGER) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getProgramDir) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getProgramDir Procedure 
FUNCTION getProgramDir RETURNS CHARACTER
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getQuery) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getQuery Procedure 
FUNCTION getQuery RETURNS CHARACTER
  ( INPUT pcDatabase AS CHARACTER
  , INPUT pcTable    AS CHARACTER
  , INPUT piQuery    AS INTEGER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getReadableQuery) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getReadableQuery Procedure 
FUNCTION getReadableQuery RETURNS CHARACTER
  ( INPUT pcQuery AS CHARACTER ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getRegistry Procedure 
FUNCTION getRegistry RETURNS CHARACTER
    ( pcSection AS CHARACTER
    , pcKey     AS CHARACTER 
    )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getStackSize) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getStackSize Procedure 
FUNCTION getStackSize RETURNS INTEGER() FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTableList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getTableList Procedure 
FUNCTION getTableList RETURNS CHARACTER
  ( INPUT  pcDatabaseFilter AS CHARACTER
  , INPUT  pcTableFilter    AS CHARACTER
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getUserName) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getUserName Procedure 
FUNCTION getUserName RETURNS CHARACTER
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getWidgetUnderMouse) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getWidgetUnderMouse Procedure 
FUNCTION getWidgetUnderMouse RETURNS HANDLE
  ( phFrame AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getXmlNodeName) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getXmlNodeName Procedure 
FUNCTION getXmlNodeName RETURNS CHARACTER
  ( pcFieldName AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isBrowseChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isBrowseChanged Procedure 
FUNCTION isBrowseChanged RETURNS LOGICAL
  ( INPUT phBrowse AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isDefaultFontsChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isDefaultFontsChanged Procedure 
FUNCTION isDefaultFontsChanged RETURNS LOGICAL
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isFileLocked) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isFileLocked Procedure 
FUNCTION isFileLocked RETURNS LOGICAL
  ( pcFileName AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isMouseOver) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isMouseOver Procedure 
FUNCTION isMouseOver RETURNS LOGICAL
  ( phWidget AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isTableFilterUsed) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isTableFilterUsed Procedure 
FUNCTION isTableFilterUsed RETURNS LOGICAL
  ( INPUT TABLE ttTableFilter )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isValidCodePage) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isValidCodePage Procedure 
FUNCTION isValidCodePage RETURNS LOGICAL
  (pcCodepage AS CHARACTER) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isWidgetChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isWidgetChanged Procedure 
FUNCTION isWidgetChanged RETURNS LOGICAL
  ( INPUT phWidget AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-readFile) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD readFile Procedure 
FUNCTION readFile RETURNS LONGCHAR
  (pcFilename AS CHARACTER) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-removeConnection) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD removeConnection Procedure 
FUNCTION removeConnection RETURNS LOGICAL
  ( pcDatabase AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resolveOsVars) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD resolveOsVars Procedure 
FUNCTION resolveOsVars RETURNS CHARACTER
  ( pcString AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resolveSequence) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD resolveSequence Procedure 
FUNCTION resolveSequence RETURNS CHARACTER
  ( pcString AS CHARACTER )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setColumnWidthList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setColumnWidthList Procedure 
FUNCTION setColumnWidthList RETURNS LOGICAL
  ( INPUT phBrowse    AS HANDLE 
  , INPUT pcWidthList AS CHARACTER) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setFilterFieldColor) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setFilterFieldColor Procedure 
FUNCTION setFilterFieldColor RETURNS LOGICAL
  ( phWidget AS HANDLE )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setLinkInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setLinkInfo Procedure 
FUNCTION setLinkInfo RETURNS LOGICAL
  ( INPUT pcFieldName AS CHARACTER
  , INPUT pcValue     AS CHARACTER
  ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setRegistry Procedure 
FUNCTION setRegistry RETURNS CHARACTER
    ( pcSection AS CHARACTER
    , pcKey     AS CHARACTER
    , pcValue   AS CHARACTER
    )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF


/* *********************** Procedure Settings ************************ */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
   Type: Procedure
   Allow: 
   Frames: 0
   Add Fields to: Neither
   Other Settings: CODE-ONLY COMPILE
 */
&ANALYZE-RESUME _END-PROCEDURE-SETTINGS

/* *************************  Create Window  ************************** */

&ANALYZE-SUSPEND _CREATE-WINDOW
/* DESIGN Window definition (used by the UIB) 
  CREATE WINDOW Procedure ASSIGN
         HEIGHT             = 45
         WIDTH              = 45.4.
/* END WINDOW DEFINITION */
                                                                        */
&ANALYZE-RESUME

 


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Procedure 


/* ***************************  Main Block  *************************** */

/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
  DO:
    DEFINE VARIABLE cEnvironment AS CHARACTER NO-UNDO.
    cEnvironment = SUBSTITUTE('DataDigger-&1', getUserName() ).

    UNLOAD 'DataDiggerHelp' NO-ERROR.
    UNLOAD 'DataDigger'     NO-ERROR.
    UNLOAD cEnvironment     NO-ERROR.
  END. /* CLOSE OF THIS-PROCEDURE  */

/* Subscribe to setUsage event to track user behaviour */
SUBSCRIBE TO "setUsage" ANYWHERE.


/* Caching settings must be set from within UI.
 * Since the library might be started from DataDigger.p
 * we cannot rely on the registry being loaded yet
 */
glCacheTableDefs = TRUE.
glCacheFieldDefs = TRUE.
glCacheSettings  = TRUE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&IF DEFINED(EXCLUDE-applyChoose) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE applyChoose Procedure 
PROCEDURE applyChoose :
/*------------------------------------------------------------------------
    Name         : applyChoose
    Description  : Apply the choose event to a widget. Is used in dynamic
                   creation of widgets.
    ----------------------------------------------------------------------
    16-01-2009 pti Created
    ----------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER pihWidget AS HANDLE NO-UNDO.
  
  IF VALID-HANDLE(pihWidget) THEN
  DO:
    PUBLISH "debugMessage" (3, SUBSTITUTE("Apply CHOOSE to &1 &2", pihWidget:TYPE, pihWidget:NAME)).
    APPLY 'choose' TO pihWidget.
  END.

END PROCEDURE. /* applyChoose */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-applyEvent) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE applyEvent Procedure 
PROCEDURE applyEvent :
/*------------------------------------------------------------------------
    Name         : applyEvent
    Description  : Apply an event to a widget. Is used in dynamic
                   creation of widgets.
    ----------------------------------------------------------------------
    16-01-2009 pti Created
    ----------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER pihWidget AS HANDLE NO-UNDO.
  DEFINE INPUT  PARAMETER pcEvent   AS CHARACTER   NO-UNDO.
  
  IF VALID-HANDLE(pihWidget) THEN
  DO:
    PUBLISH "debugMessage" (3, SUBSTITUTE("Apply &1 to &2 &3", CAPS(pcEvent), pihWidget:TYPE, pihWidget:NAME)).
    APPLY pcEvent TO pihWidget.
  END.

END PROCEDURE. /* applyEvent */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-checkDir) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE checkDir Procedure 
PROCEDURE checkDir :
/*------------------------------------------------------------------------------
    Purpose:     
    Parameters:  <none>
    Notes:       
  ------------------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER pcFileName AS CHARACTER   NO-UNDO.
  DEFINE OUTPUT PARAMETER pcError    AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cDumpDir     AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cDirToCreate AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iDir         AS INTEGER   NO-UNDO.

  PUBLISH "debugMessage" (3, SUBSTITUTE("Check &1", pcFileName)).

  /* If no path is given, use startup folder */
  cDumpDir = SUBSTRING(pcFileName, 1, R-INDEX(pcFileName,"\")).
  IF cDumpDir = '' THEN cDumpDir = '.'.

  /* We cannot use the program dir itself */
  FILE-INFO:FILE-NAME = cDumpDir.
  IF TRIM(FILE-INFO:FULL-PATHNAME,'\/') = TRIM(getProgramDir(),"/\") THEN 
  DO:
    pcError = getRegistry('DataDigger:help', 'ExportToProgramdir:message').
    RETURN.
  END.

  PUBLISH "debugMessage" (3, SUBSTITUTE("Dir = &1", cDumpDir)).

  /* Ask to overwrite if it already exists */
  FILE-INFO:FILE-NAME = pcFileName.
  IF FILE-INFO:FULL-PATHNAME <> ? THEN 
  DO:
    PUBLISH "debugMessage" (3, SUBSTITUTE("Already exists as &1 (&2)", FILE-INFO:FULL-PATHNAME, FILE-INFO:FILE-TYPE)).
    
    IF FILE-INFO:FILE-TYPE MATCHES '*F*' THEN
    DO:
      RUN showHelp('OverwriteDumpFile', pcFileName).
      IF getRegistry('DataDigger:help', 'OverwriteDumpFile:answer') <> '1' THEN 
      DO:
        /* Do not remember the answer "No" for this question, otherwise it will be
         * confusing the next time the user encounters this situation
         */
        setRegistry('DataDigger:help', 'OverwriteDumpFile:answer',?).
        pcError = 'Aborted by user.'.
        RETURN. 
      END.
  
      /* Write access to this file? */
      IF NOT FILE-INFO:FILE-TYPE MATCHES '*W*' THEN 
      DO:
        pcError = SUBSTITUTE('Cannot overwrite output file "&1"', pcFileName).
        RETURN. 
      END.
    END.

    /* If a dir already exists with the same name as the output file, we cannot create it */
    IF FILE-INFO:FILE-TYPE MATCHES '*D*' THEN
    DO:
      pcError = SUBSTITUTE('A directory named "&1" exists; cannot create a file with the same name.', pcFileName).
      RETURN. 
    END. 
  END.

  /* Check dir */
  FILE-INFO:FILE-NAME = cDumpDir.
  IF cDumpDir <> "" /* Don't complain about not using a dir */
    AND FILE-INFO:FULL-PATHNAME = ? THEN
  DO:
    RUN showHelp('CreateDumpDir', cDumpDir).
    IF getRegistry('DataDigger:help', 'CreateDumpDir:answer') <> '1' THEN 
    DO:
      pcError = 'Aborted by user.'.
      RETURN. 
    END.
  END.

  /* Try to create path + file. Progress will not raise an error if it already exists */
  cDirToCreate = ENTRY(1,cDumpDir,'\').
  DO iDir = 2 TO NUM-ENTRIES(cDumpDir,'\').

    /* In which dir do we want to create a subdir? */
    IF iDir = 2 THEN
      FILE-INFO:FILE-NAME = cDirToCreate + '\'.
    ELSE 
      FILE-INFO:FILE-NAME = cDirToCreate.

    /* Does it even exist? */
    IF FILE-INFO:FULL-PATHNAME = ? THEN
    DO:
      pcError = SUBSTITUTE('Directory "&1" does not exist.', cDirToCreate).
      PUBLISH "debugMessage" (3, SUBSTITUTE("Error: &1", pcError)).
      RETURN.
    END.

    /* Check if the dir is writable */
    IF FILE-INFO:FILE-TYPE MATCHES '*X*'  /* Happens on CD-ROM drives */
      OR (        FILE-INFO:FILE-TYPE MATCHES '*D*'
          AND NOT FILE-INFO:FILE-TYPE MATCHES '*W*' ) THEN 
    DO:
      pcError = SUBSTITUTE('No write-access to directory: "&1"', cDirToCreate).
      PUBLISH "debugMessage" (3, SUBSTITUTE("Error: &1", pcError)).
      RETURN.
    END.

    /* Seems to exist and to be writable. */
    cDirToCreate = cDirToCreate + '\' + ENTRY(iDir,cDumpDir,'\'). 

    /* If a file already exists with the same name, we cannot create a dir */
    FILE-INFO:FILE-NAME = cDirToCreate.
    IF FILE-INFO:FILE-TYPE MATCHES '*F*' THEN 
    DO:
      pcError = SUBSTITUTE('A file named "&1" exists; cannot create a dir with the same name.', cDirToCreate).
      PUBLISH "debugMessage" (3, SUBSTITUTE("Error: &1", pcError)).
      RETURN. 
    END.

    /* Create the dir. Creating an existing dir gives no error */
    OS-CREATE-DIR value(cDirToCreate). 
    IF OS-ERROR <> 0 THEN
    DO:
      pcError = getOsErrorDesc(OS-ERROR).
      PUBLISH "debugMessage" (3, SUBSTITUTE("Error: &1", pcError)).
      RETURN.
    END. /* error */

  END. /* iDir */
  
END PROCEDURE. /* checkDir */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-clearDiskCache) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearDiskCache Procedure 
PROCEDURE clearDiskCache :
/*------------------------------------------------------------------------
    Name         : clearDiskCache
    Description  : Clear the cache files on disk
    ---------------------------------------------------------------------- 
    15-11-2013 pti Created
    ----------------------------------------------------------------------*/
  DEFINE VARIABLE cFile AS CHARACTER NO-UNDO EXTENT 3.

  PUBLISH "debugMessage" (3, SUBSTITUTE("Clearing disk cache")).

  INPUT FROM OS-DIR(getProgramdir() + "cache").
  REPEAT:
    IMPORT cFile.
    IF cFile[1] MATCHES "*.xml" THEN OS-DELETE VALUE( cFile[2]).
  END.
  INPUT CLOSE. 

END PROCEDURE. /* clearDiskCache */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-clearMemoryCache) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearMemoryCache Procedure 
PROCEDURE clearMemoryCache :
/*------------------------------------------------------------------------
    Name         : clearMemoryCache
    Description  : Clear the memory cache 
    ---------------------------------------------------------------------- 
    15-11-2013 pti Created
    ----------------------------------------------------------------------*/

  PUBLISH "debugMessage" (3, SUBSTITUTE("Clearing memory cache")).
  EMPTY TEMP-TABLE ttFieldCache.

END PROCEDURE. /* clearMemoryCache */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-clearRegistryCache) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE clearRegistryCache Procedure 
PROCEDURE clearRegistryCache :
/*------------------------------------------------------------------------
    Name         : clearRegistryCache
    Description  : Clear the registry cache 
    ---------------------------------------------------------------------- 
    23-11-2012 pti Created
    ----------------------------------------------------------------------*/

  PUBLISH "debugMessage" (3, SUBSTITUTE("Clearing registry cache")).
  EMPTY TEMP-TABLE ttConfig.

END PROCEDURE. /* clearRegistryCache */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-collectQueryInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE collectQueryInfo Procedure 
PROCEDURE collectQueryInfo :
/*------------------------------------------------------------------------
  Name         : collectQueryInfo
  Description  : Fill the query temp-table
  ---------------------------------------------------------------------- 
  03-11-2009 pti Created
  ----------------------------------------------------------------------*/
 
 DEFINE INPUT  PARAMETER pcDatabase     AS CHARACTER   NO-UNDO.
 DEFINE INPUT  PARAMETER pcTable        AS CHARACTER   NO-UNDO.

 DEFINE VARIABLE iMaxQueryHistory AS INTEGER NO-UNDO. 
 DEFINE VARIABLE iQueryNr         AS INTEGER NO-UNDO. 
 DEFINE VARIABLE iLoop            AS INTEGER NO-UNDO. 
 DEFINE VARIABLE cSetting         AS CHARACTER NO-UNDO. 

 DEFINE BUFFER bQuery FOR ttQuery.
 {&timerStart}
 
 /* Delete all known queries in memory of this table */
 FOR EACH bQuery 
   WHERE bQuery.cDatabase = pcDatabase
     AND bQuery.cTable    = pcTable:
   DELETE bQuery.
 END. 

 iMaxQueryHistory = INTEGER(getRegistry("DataDigger", "MaxQueryHistory" )).
 IF iMaxQueryHistory = 0 THEN RETURN. /* no query history wanted */

 /* If it is not defined use default setting */
 IF iMaxQueryHistory = ? THEN iMaxQueryHistory = 10. 

 collectQueries:
 DO iLoop = 1 TO iMaxQueryHistory:
   cSetting = getRegistry( SUBSTITUTE("DB:&1", pcDatabase)
                         , SUBSTITUTE('&1:query:&2', pcTable, iLoop )).

   IF cSetting = '<Empty>' THEN NEXT.

   IF cSetting <> ? THEN
   DO:
     CREATE bQuery.
     ASSIGN iQueryNr         = iQueryNr + 1
            bQuery.cDatabase = pcDatabase 
            bQuery.cTable    = pcTable
            bQuery.iQueryNr  = iQueryNr
            bQuery.cQueryTxt = cSetting.
   END.
   ELSE 
     LEAVE collectQueries.

 END. /* 1 .. MaxQueryHistory */
 {&timerStop}

END PROCEDURE. /* collectQueryInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-correctFilterList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE correctFilterList Procedure 
PROCEDURE correctFilterList :
/*------------------------------------------------------------------------------
  Move negative entries from positive list to negative
  ------------------------------------------------------------------------------*/
  
  DEFINE INPUT-OUTPUT PARAMETER pcPositive AS CHARACTER   NO-UNDO.
  DEFINE INPUT-OUTPUT PARAMETER pcNegative AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE iWord AS INTEGER NO-UNDO.

  /* Strip entries that start with a ! */
  IF INDEX(pcPositive,"!") > 0 THEN
  DO:
    DO iWord = 1 TO NUM-ENTRIES(pcPositive):
      IF ENTRY(iWord,pcPositive) BEGINS "!" THEN
      DO:
        /* Add this word to the negative-list */
        pcNegative = TRIM(pcNegative + ',' + TRIM(ENTRY(iWord,pcPositive),'!'),',').
    
        /* And wipe it from the positive-list */
        ENTRY(iWord,pcPositive) = ''.
      END.
    END.
    
    /* Remove empty elements */
    pcPositive = REPLACE(pcPositive,',,',',').
    pcPositive = TRIM(pcPositive,',').
  END.

END PROCEDURE. /* correctFilterList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-createFolder) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE createFolder Procedure 
PROCEDURE createFolder :
/* Create a folder structure 
 */
  DEFINE INPUT PARAMETER pcFolder AS CHARACTER NO-UNDO.

  DEFINE VARIABLE iElement AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cPath    AS CHARACTER   NO-UNDO.

  /* c:\temp\somefolder\subfolder\ */
  DO iElement = 1 TO NUM-ENTRIES(pcFolder,'\'):
    cPath = SUBSTITUTE('&1\&2', cPath, ENTRY(iElement,pcFolder,'\')).
    cPath = LEFT-TRIM(cPath,'\').

    IF iElement > 1 THEN OS-CREATE-DIR VALUE(cPath).
  END.

END PROCEDURE. /* createFolder */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-dumpRecord) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dumpRecord Procedure 
PROCEDURE dumpRecord :
/*------------------------------------------------------------------------------
    Purpose:     
    Parameters:  <none>
    Notes:       
  ------------------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER pcAction   AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER phSource   AS HANDLE      NO-UNDO.
  DEFINE OUTPUT PARAMETER plContinue AS LOGICAL     NO-UNDO.

  DEFINE VARIABLE hExportTT       AS HANDLE    NO-UNDO.
  DEFINE VARIABLE hExportTtBuffer AS HANDLE    NO-UNDO.
  DEFINE VARIABLE hBuffer         AS HANDLE    NO-UNDO.
  DEFINE VARIABLE cFileName       AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cError          AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cMessage        AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iRow            AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cDumpProg       AS CHARACTER NO-UNDO.
  DEFINE VARIABLE lContinue       AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE lDefaultDump    AS LOGICAL   NO-UNDO.

  IF NOT VALID-HANDLE(phSource) THEN RETURN.

  /* Protect against wrong input */
  IF LOOKUP(pcAction,'Dump,Create,Update,Delete') = 0 THEN
  DO:
    MESSAGE 'Unknown action' pcAction VIEW-AS ALERT-BOX INFORMATION BUTTONS OK.
    RETURN. 
  END.

  /* Determine appropriate buffer and populate an intermediate tt
   * with the data to export
   */
  CASE phSource:TYPE:
    WHEN 'buffer' THEN 
    DO:
      hBuffer = phSource.

      /* Create temptable-handle... */
      CREATE TEMP-TABLE hExportTt.
      hExportTt:CREATE-LIKE(SUBSTITUTE("&1.&2", hBuffer:DBNAME, hBuffer:TABLE)).

      /* Prepare the TempTable... */
      hExportTt:TEMP-TABLE-PREPARE(SUBSTITUTE("&1", hBuffer:TABLE)).
      hExportTtBuffer = hExportTt:DEFAULT-BUFFER-HANDLE.
      hExportTtBuffer:BUFFER-CREATE().
      hExportTtBuffer:BUFFER-COPY(hBuffer).
    END.

    WHEN 'browse' THEN 
    DO:
      hBuffer = phSource:QUERY:GET-BUFFER-HANDLE(1).

      /* Create temptable-handle... */
      CREATE TEMP-TABLE hExportTt.
      hExportTt:CREATE-LIKE(SUBSTITUTE("&1.&2", hBuffer:DBNAME, hBuffer:TABLE)).

      /* Prepare the TempTable... */
      hExportTt:TEMP-TABLE-PREPARE(SUBSTITUTE("&1", hBuffer:TABLE)).
      hExportTtBuffer = hExportTt:DEFAULT-BUFFER-HANDLE.

      /* Copy the records */
      DO iRow = 1 TO phSource:NUM-SELECTED-ROWS:
        phSource:FETCH-SELECTED-ROW(iRow).
        hExportTtBuffer:BUFFER-CREATE().
        hExportTtBuffer:BUFFER-COPY(hBuffer).
      END.
    END.

    OTHERWISE RETURN. 
  END CASE.

  /* Do we need to dump at all? 
   * If the setting=NO or if no setting at all, then don't do any checks 
   */
  IF pcAction <> 'Dump' 
    AND (   getRegistry('DataDigger:Backup','BackupOn' + pcAction) = ?
         OR logical(getRegistry('DataDigger:Backup','BackupOn' + pcAction)) = NO
        ) THEN 
  DO:
    ASSIGN plContinue = YES.
    RETURN. 
  END.  

  /* Determine the default name to save to */
  RUN getDumpFileName
    ( INPUT pcAction        /* Dump | Create | Update | Delete */
    , INPUT hBuffer:DBNAME    
    , INPUT hBuffer:TABLE     
    , INPUT "XML"
    , INPUT ""
    , OUTPUT cFileName
    ).

  RUN checkDir(INPUT cFileName, OUTPUT cError).
  IF cError <> "" THEN 
  DO:
    MESSAGE cError VIEW-AS ALERT-BOX INFORMATION BUTTONS OK.
    RETURN. 
  END. 

  /* Fix XML Node Names for fields in the tt */
  RUN setXmlNodeNames(INPUT hExportTt:DEFAULT-BUFFER-HANDLE).

  /* See if the user has specified his own dump program
   */
  plContinue = ?. /* To see if it ran or not */
  PUBLISH "customDump"
      ( INPUT pcAction
      , INPUT hBuffer:DBNAME 
      , INPUT hBuffer:TABLE
      , INPUT hExportTt
      , INPUT cFileName
      , OUTPUT cMessage
      , OUTPUT lDefaultDump
      , OUTPUT plContinue
      ).
      
  IF plContinue <> ? THEN
  DO:
    IF cMessage <> "" THEN 
      MESSAGE cMessage VIEW-AS ALERT-BOX INFORMATION BUTTONS OK.

    IF NOT lDefaultDump OR NOT plContinue THEN
      RETURN. 
  END.

  plContinue = hExportTT:WRITE-XML
    ( 'file'        /* TargetType     */
    , cFileName     /* File           */
    , YES           /* Formatted      */
    , ?             /* Encoding       */
    , ?             /* SchemaLocation */
    , NO            /* WriteSchema    */
    , NO            /* MinSchema      */
    ).

  DELETE OBJECT hExportTt.
END PROCEDURE. /* dumpRecord */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-dynamicDump) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dynamicDump Procedure 
PROCEDURE dynamicDump :
/*------------------------------------------------------------------------
  Name         : dynamicDump
  Description  : Dump the data to a file that is similar to those of   
                 Progress self. Add a checksum and nr of records at the
                 end of the file. 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER pihBrowse AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER picFile   AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cDataType  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTimeStamp AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBuffer    AS HANDLE      NO-UNDO EXTENT 5.
  DEFINE VARIABLE hColumn    AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hField     AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery     AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iBack      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iBuffer    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iColumn    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iExtent    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iRecords   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iTrailer   AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lFirst     AS LOGICAL     NO-UNDO.

  hQuery = pihBrowse:QUERY.

  /* Accept max 5 buffers for a query */
  DO iBuffer = 1 TO min(5, hQuery:NUM-BUFFERS):
    hBuffer[iBuffer] = hQuery:GET-BUFFER-HANDLE(iBuffer).
  END.

  ASSIGN
    iRecords   = 0
    cTimeStamp = STRING(YEAR( TODAY),"9999":u) + "/":u
               + string(MONTH(TODAY),"99":u  ) + "/":u
               + string(DAY(  TODAY),"99":u  ) + "-":u
               + string(TIME,"HH:MM:SS":u).

  hQuery:GET-FIRST.

  /* Open outputfile */
  OUTPUT to value(picFile) no-echo no-map.
  EXPORT ?.
  iBack = seek(output) - 1.
  SEEK OUTPUT TO 0.    

  REPEAT WHILE NOT hQuery:QUERY-OFF-END
  ON STOP UNDO, LEAVE:

    ASSIGN 
      iRecords = iRecords + 1
      lFirst   = TRUE
      .

    PROCESS EVENTS.
    
    browseColumn:
    DO iColumn = 1 TO pihBrowse:NUM-COLUMNS:

      /* Grab the handle */
      hColumn = pihBrowse:GET-BROWSE-COLUMN(iColumn).

      /* Skip invisible columns */
      IF NOT hColumn:VISIBLE THEN NEXT browseColumn.

      /* Find the buffer the column belongs to */
      SearchLoop:
      DO iBuffer = 1 TO 5:
        ASSIGN hField = hBuffer[iBuffer]:BUFFER-FIELD(hColumn:NAME) no-error.
        IF ERROR-STATUS:ERROR = FALSE 
          AND hField <> ? THEN 
          LEAVE SearchLoop.
      END.

      /* If no column found, something weird happened */
      IF hField = ? THEN NEXT browseColumn.

      IF hField:DATA-TYPE = "recid":u THEN NEXT.
  
      IF lFirst THEN
        lFirst = FALSE.
      ELSE
      DO:
        SEEK OUTPUT TO seek(output) - iBack.
        PUT CONTROL ' ':u.
      END.
  
      IF hField:EXTENT > 1 THEN
      DO iExtent = 1 TO hField:EXTENT:
        IF iExtent > 1 THEN
        DO:
          SEEK OUTPUT TO seek(output) - iBack.
          PUT CONTROL ' ':u.
        END.
  
        EXPORT hField:BUFFER-VALUE(iExtent).
      END.
      ELSE
        EXPORT hField:BUFFER-VALUE.
    END. 

    hQuery:GET-NEXT().
  END.
  
  PUT UNFORMATTED ".":u SKIP.
  iTrailer = seek(output).
  
  PUT UNFORMATTED
         "PSC":u 
    SKIP "filename=":u hBuffer[1]:TABLE 
    SKIP "records=":u  STRING(iRecords,"9999999999999":u) 
    SKIP "ldbname=":u  hBuffer[1]:DBNAME 
    SKIP "timestamp=":u cTimeStamp 
    SKIP "numformat=":u ASC(SESSION:NUMERIC-SEPARATOR) ",":u ASC(SESSION:NUMERIC-DECIMAL-POINT) 
    SKIP "dateformat=":u SESSION:DATE-FORMAT "-":u SESSION:YEAR-OFFSET 
    SKIP "map=NO-MAP":u 
    SKIP "cpstream=":u SESSION:CPSTREAM 
    SKIP ".":u 
    SKIP STRING(iTrailer,"9999999999":u) 
    SKIP.
  
  OUTPUT close.

END PROCEDURE. /* dynamicDump */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-dynamicLoad) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE dynamicLoad Procedure 
PROCEDURE dynamicLoad :
/*------------------------------------------------------------------------
  Name         : dynamicLoad
  Description  : Load data from a file into a buffer.
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT  PARAMETER hTable AS HANDLE      NO-UNDO.
  DEFINE INPUT  PARAMETER cDelim AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cImport AS CHARACTER   NO-UNDO EXTENT 2800.
  DEFINE VARIABLE iImp    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iCnt    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iExtnt  AS INTEGER     NO-UNDO.
  DEFINE VARIABLE hFld    AS HANDLE      NO-UNDO.

  IF hTable:TYPE <> "BUFFER" THEN RETURN.
  
  REPEAT:
    ASSIGN cImport = ""
           iImp = 0.

    IMPORT cImport.
    hTable:BUFFER-CREATE().

    DO iCnt = 1 TO hTable:NUM-FIELDS:
      ASSIGN hFld = hTable:BUFFER-FIELD(iCnt).

      IF hFld:EXTENT = 0 THEN 
      DO:
        ASSIGN iImp = iImp + 1
               hFld:BUFFER-VALUE = cImport[iImp].
      END.
      ELSE 
      DO iExtnt = 1 TO hFld:EXTENT:
        ASSIGN iImp = iImp + 1
               hFld:BUFFER-VALUE(iExtnt) = cImport[iImp].
      END.
    END.

    hTable:BUFFER-VALIDATE().
  END.

END PROCEDURE. /* dynamicLoad */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColumnSort) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getColumnSort Procedure 
PROCEDURE getColumnSort :
/*------------------------------------------------------------------------
  Name         : getColumnSort
  Description  : Return the column nr the browse is sorted on
  ----------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER phBrowse    AS HANDLE      NO-UNDO.
  DEFINE OUTPUT PARAMETER pcColumn    AS CHARACTER   NO-UNDO.
  DEFINE OUTPUT PARAMETER plAscending AS LOGICAL     NO-UNDO.
  
  DEFINE VARIABLE hColumn AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iColumn AS INTEGER     NO-UNDO.
  
  {&timerStart}

  do iColumn = 1 to phBrowse:num-columns:
    hColumn = phBrowse:get-browse-column(iColumn).
    IF hColumn:SORT-ASCENDING <> ? THEN DO:
      ASSIGN
        pcColumn    = hColumn:NAME
        plAscending = hColumn:SORT-ASCENDING.
      LEAVE.
    END.
  END.

  IF pcColumn = '' THEN
    ASSIGN
      pcColumn    = phBrowse:get-browse-column(1):name
      plAscending = TRUE.
      
  PUBLISH "debugMessage" (3, SUBSTITUTE("Sorting &1 on &2", STRING(plAscending,"up/down"), pcColumn)).
  
  {&timerStop}

END PROCEDURE. /* getColumnSort */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getDumpFileName) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getDumpFileName Procedure 
PROCEDURE getDumpFileName :
/*------------------------------------------------------------------------
  Name         : getDumpFileName
  Description  : Return a file name based on a template
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT  PARAMETER pcAction    AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcDatabase  AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcTable     AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcExtension AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcTemplate  AS CHARACTER   NO-UNDO.
  DEFINE OUTPUT PARAMETER pcFileName  AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cLastDir      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDayOfWeek    AS CHARACTER   NO-UNDO EXTENT 7 INITIAL ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].
  DEFINE VARIABLE cDumpName     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDumpDir      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDumpFile     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cBackupDir    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cBackupFile   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBuffer       AS HANDLE      NO-UNDO.

  /* Checks */
  IF LOOKUP(pcAction, "Dump,Create,Update,Delete") = 0 THEN 
  DO:
    MESSAGE 'Unknown action' pcAction VIEW-AS ALERT-BOX.
    RETURN.
  END.

  /* If not provided, find the template from the settings,
   * depending on the action we want to perform. 
   */
  IF pcTemplate = ? OR pcTemplate = "" THEN
  DO:
    IF pcAction = 'Dump' THEN 
      pcFileName = "<DUMPDIR>" + getRegistry("DumpAndLoad", "DumpFileTemplate").
    ELSE 
      pcFileName = "<BACKUPDIR>" + getRegistry("DataDigger:Backup", "BackupFileTemplate").
  END.
  ELSE 
    pcFileName = pcTemplate.

  IF pcFileName = ? THEN pcFileName = "".

  PUBLISH "debugMessage" (3, SUBSTITUTE("Dump to: &1", pcFileName)).

  /* Dump dir / backup dir / last-used dir from settings */
  cDumpDir = RIGHT-TRIM(getRegistry("DumpAndLoad", "DumpDir"),'/\') + '\'.
  IF cDumpDir = ? OR cDumpDir = '' THEN cDumpDir = "<PROGDIR>dump\". 

  cBackupDir  = RIGHT-TRIM(getRegistry("DataDigger:Backup", "BackupDir"),'/\') + '\'.
  IF cBackupDir = ? OR cBackupDir = '' THEN cBackupDir = "<PROGDIR>backup\". 

  cLastDir = RIGHT-TRIM(getRegistry("DumpAndLoad", "DumpLastFileName"),'/\').
  cLastDir = SUBSTRING(cLastDir,1,R-INDEX(cLastDir,"\")).
  IF cLastDir = ? THEN cLastDir = "<PROGDIR>dump".
  cLastDir = RIGHT-TRIM(cLastDir,'\').

  /* Find _file for the dump-name */
  CREATE BUFFER hBuffer FOR TABLE SUBSTITUTE('&1._file', pcDatabase) NO-ERROR.
  IF VALID-HANDLE(hBuffer) THEN
  DO:
    hBuffer:FIND-UNIQUE(SUBSTITUTE('where _file-name = &1 and _File._File-Number < 32768', QUOTER(pcTable)),NO-LOCK).
    IF hBuffer:AVAILABLE THEN
      cDumpName = hBuffer::_dump-name.
    ELSE 
      cDumpName = pcTable.
  END.
  ELSE 
    cDumpName = pcTable.
  IF cDumpName = ? THEN cDumpName = pcTable.
  
  PUBLISH "debugMessage" (3, SUBSTITUTE("DumpDir  : &1", cDumpDir)).
  PUBLISH "debugMessage" (3, SUBSTITUTE("BackupDir: &1", cBackupDir)).
  PUBLISH "debugMessage" (3, SUBSTITUTE("LastDir  : &1", cLastDir)).
  PUBLISH "debugMessage" (3, SUBSTITUTE("DumpName : &1", cDumpName)).

  /* Now resolve all tags */
  pcFileName = REPLACE(pcFileName,"<DUMPDIR>"  , cDumpDir                    ).
  pcFileName = REPLACE(pcFileName,"<BACKUPDIR>", cBackupDir                  ).
  pcFileName = REPLACE(pcFileName,"<LASTDIR>"  , cLastDir                    ).
  pcFileName = REPLACE(pcFileName,"<PROGDIR>"  , getProgramDir()             ).

  pcFileName = REPLACE(pcFileName,"<ACTION>"   , pcAction                    ).
  pcFileName = REPLACE(pcFileName,"<USERID>"   , USERID(LDBNAME(1))          ).
  pcFileName = REPLACE(pcFileName,"<DB>"       , pcDatabase                  ).
  pcFileName = REPLACE(pcFileName,"<TABLE>"    , pcTable                     ).
  pcFileName = REPLACE(pcFileName,"<DUMPNAME>" , cDumpName                   ).
  pcFileName = REPLACE(pcFileName,"<EXT>"      , pcExtension                 ).

  pcFileName = REPLACE(pcFileName,"<TIMESTAMP>", "<YEAR><MONTH><DAY>.<HH><MM><SS>" ).
  pcFileName = REPLACE(pcFileName,"<DATE>"     , "<YEAR>-<MONTH>-<DAY>"      ).
  pcFileName = REPLACE(pcFileName,"<TIME>"     , "<HH>:<MM>:<SS>"            ).
  pcFileName = REPLACE(pcFileName,"<WEEKDAY>"  , STRING(WEEKDAY(TODAY))      ).
  pcFileName = REPLACE(pcFileName,"<DAYNAME>"  , cDayOfWeek[WEEKDAY(today)]  ).

  pcFileName = REPLACE(pcFileName,"<YEAR>"     , STRING(YEAR (TODAY),"9999") ).
  pcFileName = REPLACE(pcFileName,"<MONTH>"    , STRING(MONTH(TODAY),  "99") ).
  pcFileName = REPLACE(pcFileName,"<DAY>"      , STRING(DAY  (TODAY),  "99") ).
  pcFileName = REPLACE(pcFileName,"<HH>"       , ENTRY(1,STRING(TIME,"HH:MM:SS"),":" ) ).
  pcFileName = REPLACE(pcFileName,"<MM>"       , ENTRY(2,STRING(TIME,"HH:MM:SS"),":" ) ).
  pcFileName = REPLACE(pcFileName,"<SS>"       , ENTRY(3,STRING(TIME,"HH:MM:SS"),":" ) ).

  /* Get rid of annoying slashes */
  pcFileName = TRIM(pcFileName,'/\').
  
  /* Get rid of double slashes (except at the beginning for UNC paths) */
  pcFileName = SUBSTRING(pcFileName,1,1) + REPLACE(SUBSTRING(pcFileName,2),'\\','\').
  
  /* Sequences */
  pcFileName = resolveSequence(pcFileName).

  /* OS-vars */
  pcFileName = resolveOsVars(pcFileName).

  /* Make lower */
  pcFileName = LC(pcFileName).
  PUBLISH "debugMessage" (3, SUBSTITUTE("Dump to: &1", pcFileName)).

END PROCEDURE. /* getDumpFileName */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getFields) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getFields Procedure 
PROCEDURE getFields :
/*------------------------------------------------------------------------
  Name         : getFields
  Description  : Fill the fields temp-table
  ---------------------------------------------------------------------- 
  18-03-2009 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT  PARAMETER pcDatabase  AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcTableName AS CHARACTER   NO-UNDO.
  DEFINE OUTPUT PARAMETER DATASET FOR dsFields.

  DEFINE VARIABLE cCacheFile         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldOrder        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFields            AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cPrimIndexFields   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery             AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSelectedFields    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTableCacheId      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cUniqueIndexFields AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSDBName           AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hBufferField       AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hBufferFile        AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hQuery             AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iFieldExtent       AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iFieldOrder        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lDataField         AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lShowRecidField    AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lShowRowidField    AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE iDataOrder         AS INTEGER     NO-UNDO.

  DEFINE BUFFER bTable       FOR ttTable. 
  DEFINE BUFFER bField       FOR ttField. 
  DEFINE BUFFER bColumn      FOR ttColumn. 
  DEFINE BUFFER bFieldCache  FOR ttFieldCache.
  DEFINE BUFFER bColumnCache FOR ttColumnCache.
  DEFINE BUFFER bTableFilter FOR ttTableFilter.

  {&timerStart}
  
  /* Clean up first */
  EMPTY TEMP-TABLE bField.
  EMPTY TEMP-TABLE bColumn.

  /* For dataservers, use the schema name [dataserver] */
  ASSIGN cSDBName = SDBNAME(pcDatabase).

  /* Return if no db connected */
  IF NUM-DBS = 0 THEN RETURN. 

  /* caching */
  IF glCacheFieldDefs THEN
  DO:
    /* Find the table. Should exist. */
    FIND bTable WHERE bTable.cDatabase = pcDatabase AND bTable.cTableName = pcTableName.
    
    /* Verify whether the CRC is still the same. If not, kill the cache */
    CREATE BUFFER hBufferFile FOR TABLE cSDBName + "._File".

    hBufferFile:FIND-UNIQUE(SUBSTITUTE('where _file-name = &1 and _File._File-Number < 32768', QUOTER(pcTableName)),NO-LOCK).
    IF hBufferFile::_crc <> bTable.cCrc THEN
    DO:
      /* It seems that it is not possible to refresh the schema cache of the running
       * session. You just have to restart your session. 
       */
      PUBLISH "debugMessage" (1, SUBSTITUTE("File CRC changed, kill cache and build new")).
      FOR EACH bFieldCache WHERE bFieldCache.cTableCacheId = bTable.cCacheId:
        DELETE bFieldCache.
      END.
      FOR EACH bColumnCache WHERE bColumnCache.cTableCacheId = bTable.cCacheId:
        DELETE bColumnCache.
      END.
      
      /* Get a fresh list of tables */
      RUN getTables(INPUT TABLE bTableFilter, OUTPUT TABLE bTable).

      /* Find the table back. Should exist. */
      FIND bTable WHERE bTable.cDatabase = pcDatabase AND bTable.cTableName = pcTableName.
    END.

    /* First look in the memory-cache */
    IF CAN-FIND(FIRST bFieldCache WHERE bFieldCache.cTableCacheId = bTable.cCacheId) THEN
    DO:
      PUBLISH "debugMessage" (3, SUBSTITUTE("Get from memory-cache")).
      FOR EACH bFieldCache WHERE bFieldCache.cTableCacheId = bTable.cCacheId:
        CREATE bField.
        BUFFER-COPY bFieldCache TO bField.
      END.
      FOR EACH bColumnCache WHERE bColumnCache.cTableCacheId = bTable.cCacheId:
        CREATE bColumn.
        BUFFER-COPY bColumnCache TO bColumn.
      END.

      /* Update with settings from registry */
      RUN updateFields(INPUT pcDatabase, INPUT pcTableName, INPUT-OUTPUT TABLE bField).

      {&timerStop}
      RETURN. 
    END.
  
    /* See if disk cache exists */
    cCacheFile = SUBSTITUTE('&1cache\&2.xml', getProgramDir(), bTable.cCacheId).
    PUBLISH "debugMessage" (2, SUBSTITUTE("Cachefile: &1", cCacheFile)).
  
    IF SEARCH(cCacheFile) <> ? THEN
    DO:
      PUBLISH "debugMessage" (3, SUBSTITUTE("Get from disk cache")).
      DATASET dsFields:READ-XML("file", cCacheFile, "empty", ?, ?, ?, ?).
  
      /* Add to memory cache, so the next time it's even faster */
      IF TEMP-TABLE bField:HAS-RECORDS THEN
      DO:
        PUBLISH "debugMessage" (3, SUBSTITUTE("Add to first-level cache")).
        FOR EACH bField:
          CREATE bFieldCache.
          BUFFER-COPY bField TO bFieldCache.
        END.
        FOR EACH bColumn:
          CREATE bColumnCache.
          BUFFER-COPY bColumn TO bColumnCache.
        END.
      END.
  
      /* Update with settings from registry */
      RUN updateFields(INPUT pcDatabase, INPUT pcTableName, INPUT-OUTPUT TABLE bField).

      {&timerStop}
      RETURN. 
    END.
    PUBLISH "debugMessage" (3, SUBSTITUTE("Not found in any cache, build tables...")).
  END.

  /*
   * If we get here, the table either cannot be found in the cache
   * or caching is disabled. Either way, fill the tt with fields
   */
  FIND bTable WHERE bTable.cDatabase = pcDatabase AND bTable.cTableName = pcTableName.

  CREATE BUFFER hBufferFile  FOR TABLE cSDBName + "._File".                    
  CREATE BUFFER hBufferField FOR TABLE cSDBName + "._Field".

  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hBufferFile,hBufferField).

  cQuery = SUBSTITUTE("FOR EACH &1._File  WHERE &1._file._file-name = '&2' AND _File._File-Number < 32768 NO-LOCK, " +
                      "    EACH &1._Field OF &1._File NO-LOCK BY _ORDER" 
                     , cSDBName
                     , pcTableName
                     ).

  hQuery:QUERY-PREPARE(cQuery).
  hQuery:QUERY-OPEN().
  hQuery:GET-FIRST().

  /* Get list of fields in primary index. */
  cPrimIndexFields = getIndexFields(cSDBName, pcTableName, "P").

  /* Get list of fields in all unique indexes. */
  cUniqueIndexFields = getIndexFields(cSDBName, pcTableName, "U").

  /* Get list of all previously selected fields */
  cSelectedFields = getRegistry(SUBSTITUTE("DB:&1",pcDatabase), SUBSTITUTE("&1:Fields",pcTableName)).

  /* If none selected, set mask to 'all' */
  IF cSelectedFields = ? THEN cSelectedFields = '*'.

  /* Get field ordering */
  cFieldOrder = getRegistry(SUBSTITUTE('DB:&1',pcDatabase), SUBSTITUTE('&1:FieldOrder',pcTableName)).
              
  REPEAT WHILE NOT hQuery:QUERY-OFF-END:

    CREATE bField.
    ASSIGN 
      iFieldOrder          = iFieldOrder + 1
      bField.cTableCacheId = bTable.cCacheId
      bField.cDatabase     = pcDatabase
      bField.cTablename    = pcTableName
      bField.cFieldName    = hBufferField:BUFFER-FIELD('_field-name'):BUFFER-VALUE

      bField.lShow         = CAN-DO(cSelectedFields, hBufferField:BUFFER-FIELD('_field-name'):BUFFER-VALUE)
      bField.iOrder        = iFieldOrder
      bField.iOrderOrg     = iFieldOrder 

      bField.cFullName     = hBufferField:BUFFER-FIELD('_field-name'):BUFFER-VALUE 
      bField.cDataType     = hBufferField:BUFFER-FIELD('_data-type'):BUFFER-VALUE
      bField.cInitial      = hBufferField:BUFFER-FIELD('_initial'):BUFFER-VALUE   
      bField.cFormat       = hBufferField:BUFFER-FIELD('_format'):BUFFER-VALUE     
      bField.cFormatOrg    = hBufferField:BUFFER-FIELD('_format'):BUFFER-VALUE      
      bField.cLabel        = hBufferField:BUFFER-FIELD('_label'):BUFFER-VALUE
      bField.lPrimary      = CAN-DO(cPrimIndexFields, bField.cFieldName)
      bField.iExtent       = hBufferField:BUFFER-FIELD('_Extent'):BUFFER-VALUE
      bField.lMandatory    = hBufferField:BUFFER-FIELD('_mandatory'):BUFFER-VALUE
      bField.lUniqueIdx    = CAN-DO(cUniqueIndexFields,bField.cFieldName)
      
      /* New fields as per v19 */
      bField.cColLabel     = hBufferField:BUFFER-FIELD('_Col-label'):BUFFER-VALUE
      bField.iDecimals     = hBufferField:BUFFER-FIELD('_Decimals'):BUFFER-VALUE
      bField.iFieldRpos    = hBufferField:BUFFER-FIELD('_Field-rpos'):BUFFER-VALUE
      bField.cValExp       = hBufferField:BUFFER-FIELD('_ValExp'):BUFFER-VALUE
      bField.cValMsg       = hBufferField:BUFFER-FIELD('_ValMsg'):BUFFER-VALUE
      bField.cHelp         = hBufferField:BUFFER-FIELD('_Help'):BUFFER-VALUE
      bField.cDesc         = hBufferField:BUFFER-FIELD('_Desc'):BUFFER-VALUE
      bField.cViewAs       = hBufferField:BUFFER-FIELD('_View-as'):BUFFER-VALUE
      .
    ASSIGN
      bField.cXmlNodeName  = getXmlNodeName(bField.cFieldName)
      .

    /* Make a list of fields on table level */
    bTable.cFields = bTable.cFields + "," + bField.cFieldName.

    /* Some types should not be shown like CLOB BLOB and RAW */
    lDataField = (LOOKUP(bField.cDataType, 'clob,blob,raw') = 0).

    /* Create TT records for each column to show, except for CLOB / BLOB / RAW */
    IF lDataField = TRUE THEN
    DO iFieldExtent = (IF bField.iExtent = 0 THEN 0 ELSE 1) TO bField.iExtent:

      iDataOrder = iDataOrder + 1. 

      CREATE bColumn.
      ASSIGN
        bColumn.cTableCacheId = bTable.cCacheId
        bColumn.cDatabase     = bField.cDatabase
        bColumn.cTableName    = bField.cTablename
        bColumn.cFieldName    = bField.cFieldName
        bColumn.iExtent       = iFieldExtent
        bColumn.cFullName     = bField.cFieldName + (IF iFieldExtent > 0 THEN SUBSTITUTE("[&1]", iFieldExtent) ELSE "")
        bColumn.iColumnNr     = iDataOrder
        bColumn.iOrder        = bField.iOrder
        bColumn.cLabel        = bField.cLabel
        .
      PUBLISH "debugMessage"(3,SUBSTITUTE("Field &1 created", bColumn.cFullName)).
    END. /* For each extent nr */
    
    hQuery:GET-NEXT().
  END.
  hQuery:QUERY-CLOSE().

  DELETE OBJECT hQuery.
  DELETE OBJECT hBufferField.
  DELETE OBJECT hBufferFile.

  /* Fieldlist */
  bTable.cFields = SUBSTRING(bTable.cFields,2).

  /* Add a column for the recid */
  lShowRecidField = LOGICAL(getRegistry ("DataDigger", "AddDataColumnForRecid")).
  lShowRowidField = LOGICAL(getRegistry ("DataDigger", "AddDataColumnForRowid")).

  CREATE bField.
  ASSIGN 
    iFieldOrder          = iFieldOrder + 1
    bField.cTableCacheId = bTable.cCacheId
    bField.cDatabase     = pcDatabase
    bField.cTablename    = pcTableName
    bField.cFieldName    = "RECID"
                          
    bField.lShow         = (IF cSelectedFields = '*' THEN lShowRecidField ELSE LOOKUP('RECID',cSelectedFields) > 0)
    bField.iOrder        = iFieldOrder
    bField.iOrderOrg     = iFieldOrder
    bField.cFullName     = 'RECID'
    bField.cDataType     = 'character'
    bField.cInitial      = ''
    bField.cFormat       = 'X(14)'
    bField.cFormatOrg    = 'X(14)'
    bField.cLabel        = 'RECID'
    bField.lPrimary      = NO
    bField.iExtent       = 0
    .
  iDataOrder = iDataOrder + 1. 
  CREATE bColumn.
  ASSIGN
    bColumn.cTableCacheId = bField.cTableCacheId
    bColumn.cDatabase     = bField.cDatabase
    bColumn.cTableName    = bField.cTablename
    bColumn.cFieldName    = bField.cFieldName
    bColumn.iExtent       = 0
    bColumn.cFullName     = bField.cFieldName
    bColumn.iColumnNr     = iDataOrder
    bColumn.iOrder        = bField.iOrder
    bColumn.cLabel        = bField.cLabel
    .

  /* Add a column for the rowid */
  CREATE bField.
  ASSIGN 
    iFieldOrder           = iFieldOrder + 1
    bField.cTableCacheId = bTable.cCacheId
    bField.cDatabase     = pcDatabase
    bField.cTablename    = pcTableName
    bField.cFieldName    = "ROWID"
                          
    bField.lShow         = (IF cSelectedFields = '*' THEN lShowRowidField ELSE LOOKUP('ROWID',cSelectedFields) > 0)
    bField.iOrder        = iFieldOrder
    bField.iOrderOrg     = iFieldOrder
    bField.cFieldName    = 'ROWID'
    bField.cFullName     = 'ROWID'
    bField.cDataType     = 'character'
    bField.cInitial      = ''
    bField.cFormat       = 'X(30)'
    bField.cFormatOrg    = 'X(30)'
    bField.cLabel        = 'ROWID'
    bField.lPrimary      = NO
    bField.iExtent       = 0
    .
  iDataOrder = iDataOrder + 1. 
  CREATE bColumn.
  ASSIGN
    bColumn.cTableCacheId = bField.cTableCacheId
    bColumn.cDatabase     = bField.cDatabase
    bColumn.cTableName    = bField.cTablename
    bColumn.cFieldName    = bField.cFieldName
    bColumn.iExtent       = 0
    bColumn.cFullName     = bField.cFieldName
    bColumn.iColumnNr     = iDataOrder
    bColumn.iOrder        = bField.iOrder
    bColumn.cLabel        = bField.cLabel
    .
    
  /* Update the cache */
  IF glCacheFieldDefs THEN
  DO:
    /* Add to disk cache */
    PUBLISH "debugMessage" (3, SUBSTITUTE("Add to second-level cache.")).
    DATASET dsFields:WRITE-XML( "file", cCacheFile, YES, ?, ?, NO, NO). 
    
    /* Add to memory cache */
    PUBLISH "debugMessage" (3, SUBSTITUTE("Add to first-level cache.")).
    FOR EACH bField:
      CREATE bFieldCache.
      BUFFER-COPY bField TO bFieldCache.
    END.

    FOR EACH bColumn:
      CREATE bColumnCache.
      BUFFER-COPY bColumn TO bColumnCache.
    END.

    /* Update table cache */
  END.

  /* Update fields with settings from registry */
  RUN updateFields(INPUT pcDatabase, INPUT pcTableName, INPUT-OUTPUT TABLE bField).

  {&timerStop}

END PROCEDURE. /* getFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMouseXY) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getMouseXY Procedure 
PROCEDURE getMouseXY :
/*------------------------------------------------------------------------
  Name         : getMouseXY
  Description  : Get the position of the mouse relative to the frame
  ----------------------------------------------------------------------
  14-09-2010 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT  PARAMETER phFrame AS HANDLE      NO-UNDO.
  DEFINE OUTPUT PARAMETER piMouseX AS INTEGER     NO-UNDO.
  DEFINE OUTPUT PARAMETER piMouseY AS INTEGER     NO-UNDO.

  DEFINE VARIABLE lp AS MEMPTR  NO-UNDO. 
  
  set-size( lp ) = 16. 
  
  RUN GetCursorPos( INPUT-OUTPUT lp). 
  
  /* Get the location of the mouse relative to the frame */
  RUN ScreenToClient ( INPUT phFrame:HWND, INPUT lp ).
  
  piMouseX = GET-LONG( lp, 1 ). 
  piMouseY = GET-LONG( lp, 5 ). 
  
  set-size( lp ) = 0. 
  
  PUBLISH "debugMessage" (3, SUBSTITUTE("Mouse X/Y = &1 / &2", piMouseX, piMouseY)).

END PROCEDURE. /* getMouseXY */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getQueryTable) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getQueryTable Procedure 
PROCEDURE getQueryTable :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

 DEFINE OUTPUT PARAMETER table FOR ttQuery.
    
 /* This procedure just returns the table, no further logic needed. */

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTables) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getTables Procedure 
PROCEDURE getTables :
/*------------------------------------------------------------------------
  Name         : getTables
  Description  : Fill the ttTable temptable with all tables of all 
                 currently connected databases.
  ----------------------------------------------------------------------
  29-10-2009 pti Created
  06-01-2010 pti Optimized for large / a lot of databases.
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER TABLE FOR ttTableFilter.
  DEFINE OUTPUT PARAMETER TABLE FOR ttTable. 

  DEFINE VARIABLE cCacheFile      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabaseName   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cIniFile        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLine           AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSection        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hDbBuffer       AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hDbStatusBuffer AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFileBuffer     AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFieldBuffer    AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFileQuery      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFieldQuery     AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iDatabase       AS INTEGER     NO-UNDO.

  {&timerStart}

  EMPTY TEMP-TABLE ttTable.
  CREATE WIDGET-POOL "metaInfo".
  CREATE QUERY hFileQuery IN WIDGET-POOL "metaInfo".

  DO iDatabase = 1 TO NUM-DBS:
    /* NelsonAlcala added */
    IF DBTYPE(iDatabase) <> "PROGRESS" THEN NEXT.

    /* Calculate the name of the cache file */
    IF glCacheTableDefs THEN
    DO:
      CREATE BUFFER hDbStatusBuffer FOR TABLE LDBNAME(iDatabase) + "._DbStatus" IN WIDGET-POOL "metaInfo".
      hDbStatusBuffer:FIND-FIRST("",NO-LOCK).
      cCacheFile = SUBSTITUTE("&1cache\db.&2.&3.xml"
                             , getProgramDir()
                             , LDBNAME(iDatabase)
                             , REPLACE(REPLACE(hDbStatusBuffer::_dbstatus-cachestamp," ","_"),":","")
                             ). 
      DELETE OBJECT hDbStatusBuffer.
    END. 

    /* If caching enabled and there is a cache file, read it */
    IF glCacheTableDefs 
      AND SEARCH(cCacheFile) <> ? THEN
    DO:
      PUBLISH "debugMessage" (3, SUBSTITUTE("Get table list from cache file &1", cCacheFile)).
      TEMP-TABLE ttTable:READ-XML("file", cCacheFile, "APPEND", ?, ?, ?, ?).
    END.

    /* Otherwise build it */
    ELSE
    DO:
      /* To get all tables */
      CREATE BUFFER hDbBuffer    FOR TABLE LDBNAME(iDatabase) + "._Db"    IN WIDGET-POOL "metaInfo".
      CREATE BUFFER hFileBuffer  FOR TABLE LDBNAME(iDatabase) + "._file"  IN WIDGET-POOL "metaInfo".
      CREATE BUFFER hFieldBuffer FOR TABLE LDBNAME(iDatabase) + "._field" IN WIDGET-POOL "metaInfo".
      CREATE QUERY hFileQuery  IN WIDGET-POOL "metaInfo".
  
      hFileQuery:SET-BUFFERS(hDbBuffer, hFileBuffer).
      hFileQuery:QUERY-PREPARE( "FOR EACH _Db NO-LOCK "
                              + ",   EACH _File NO-LOCK"
                              + "   WHERE _File._Db-recid    = RECID(_Db)"
                              + "     AND _File._File-Number < 32768"
                              + "     AND (IF _Db._Db-slave THEN _File._For-Type = 'TABLE' ELSE TRUE)"
                              ).

      /* To get all fields */
      CREATE QUERY hFieldQuery IN WIDGET-POOL "metaInfo".
      hFieldQuery:SET-BUFFERS(hFieldBuffer).

      hFileQuery:QUERY-OPEN().
      REPEAT:
        hFileQuery:GET-NEXT().
        IF hFileQuery:QUERY-OFF-END THEN LEAVE.

        CREATE ttTable.
        ASSIGN 
          ttTable.cDatabase   = (IF hDbBuffer::_Db-slave THEN hDbBuffer::_Db-name ELSE LDBNAME(iDatabase))
          ttTable.cTableName  = hFileBuffer::_file-name
          ttTable.cTableDesc  = hFileBuffer::_desc
          ttTable.lHidden     = hFileBuffer::_hidden
          ttTable.lFrozen     = hFileBuffer::_frozen
          ttTable.cCrc        = hFileBuffer::_crc
          ttTable.cCacheId    = SUBSTITUTE('&1.&2.&3', ttTable.cDatabase, hFileBuffer::_file-name, hFileBuffer::_crc)
          ttTable.iFileNumber = hFileBuffer::_file-number
          .
        ASSIGN
          ttTable.cCategory   = getFileCategory(hFileBuffer::_file-number, hFileBuffer::_file-name)
          .

        /* Build field list */
        hFieldQuery:QUERY-PREPARE(SUBSTITUTE("FOR EACH _Field NO-LOCK WHERE _Field._File-Recid = &1", hFileBuffer:RECID) ).
        hFieldQuery:QUERY-OPEN().

        REPEAT:
          hFieldQuery:GET-NEXT().
          IF hFieldQuery:QUERY-OFF-END THEN LEAVE.
          ttTable.cFields = ttTable.cFields + "," + hFieldBuffer::_Field-name.
        END.

        ttTable.cFields = TRIM(ttTable.cFields, ",").
        hFieldQuery:QUERY-CLOSE().
      END.

      hFieldQuery:QUERY-CLOSE().
      hFileQuery:QUERY-CLOSE().
      DELETE OBJECT hFieldQuery.
      DELETE OBJECT hFileQuery.
      DELETE OBJECT hFieldBuffer.
      DELETE OBJECT hFileBuffer.
      DELETE OBJECT hDbBuffer.

      /* Save cache file for next time */
      IF glCacheTableDefs THEN
      DO:
        /* Move the tables of the current db to a separate tt so we can dump it. */
        EMPTY TEMP-TABLE ttTableXml.

        CREATE QUERY hFileQuery IN WIDGET-POOL "metaInfo".

        CREATE BUFFER hDbBuffer    FOR TABLE LDBNAME(iDatabase) + "._Db"    IN WIDGET-POOL "metaInfo".

        hFileQuery:SET-BUFFERS(hDbBuffer).
        hFileQuery:QUERY-PREPARE("FOR EACH _Db NO-LOCK ").

        hFileQuery:QUERY-OPEN().
        REPEAT:
           hFileQuery:GET-NEXT().
           IF hFileQuery:QUERY-OFF-END THEN LEAVE.

           FOR EACH ttTable 
              WHERE ttTable.cDatabase = (IF hDbBuffer::_Db-slave THEN hDbBuffer::_Db-name ELSE LDBNAME(iDatabase)):
          CREATE ttTableXml.
          BUFFER-COPY ttTable TO ttTableXml.
        END.
        END.

        hFileQuery:QUERY-CLOSE().
        DELETE OBJECT hFileQuery.
        DELETE OBJECT hDbBuffer.

        TEMP-TABLE ttTableXml:WRITE-XML("file", cCacheFile, YES, ?, ?, NO, NO).
        EMPTY TEMP-TABLE ttTableXml.
      END.
    END. /* tt empty */
  END. /* 1 to num-dbs */

  DELETE WIDGET-POOL "metaInfo".

/*   /* By default, show all tables */ */
/*   FOR EACH ttTable:                 */
/*     ttTable.lShowInList = TRUE.     */
/*   END.                              */
  RUN getTablesFiltered(INPUT TABLE ttTableFilter, OUTPUT TABLE ttTable).

  /* Get table properties from the INI file */
  RUN getTableStats(INPUT-OUTPUT TABLE ttTable).
  
  {&timerStop}

END PROCEDURE. /* getTables */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTablesFiltered) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getTablesFiltered Procedure 
PROCEDURE getTablesFiltered :
/*
 * Determine whether tables in the ttTable are visible 
 * given a user defined filter
 */
  {&timerStart}

  DEFINE INPUT PARAMETER TABLE FOR ttTableFilter.
  DEFINE OUTPUT PARAMETER TABLE FOR ttTable. 

  DEFINE VARIABLE cSearchFld  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cThisField  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iSearch     AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iField      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lRejected   AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lFieldFound AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lNormal     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lSchema     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lVst        AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lSql        AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lOther      AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lHidden     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lFrozen     AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE cNameShow   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cNameHide   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldShow  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldHide  AS CHARACTER   NO-UNDO.

  /* This table **SHOULD** exist 
   * and have exactly 1 record 
   */
  FIND ttTableFilter. 

  ASSIGN 
    lNormal    = ttTableFilter.lShowNormal
    lSchema    = ttTableFilter.lShowSchema
    lVst       = ttTableFilter.lShowVst
    lSql       = ttTableFilter.lShowSql
    lOther     = ttTableFilter.lShowOther
    lHidden    = ttTableFilter.lShowHidden
    lFrozen    = ttTableFilter.lShowFrozen
    cNameShow  = ttTableFilter.cTableNameShow
    cNameHide  = ttTableFilter.cTableNameHide 
    cFieldShow = ttTableFilter.cTableFieldShow
    cFieldHide = ttTableFilter.cTableFieldHide
    .
    
  /* Reset the filters to sane values if needed */
  IF cNameShow  = ''  OR cNameShow  = ? THEN cNameShow  = '*'.
  IF cNameHide  = '*' OR cNameHide  = ? THEN cNameHide  = '' .
  IF cFieldShow = '*' OR cFieldShow = ? THEN cFieldShow = ''.
  IF cFieldHide = '*' OR cFieldHide = ? THEN cFieldHide = ''.

  /* Move elements starting with "!" from pos-list to neg-list */
  RUN correctFilterList(INPUT-OUTPUT cNameShow, INPUT-OUTPUT cNameHide).
  RUN correctFilterList(INPUT-OUTPUT cFieldShow, INPUT-OUTPUT cFieldHide).

  tableLoop:
  FOR EACH ttTable:
    /* Init table to false until proven otherwise */
    ASSIGN ttTable.lShowInList = FALSE.
    
    /* Check against filter-to-hide */
    IF CAN-DO(cNameHide,ttTable.cTableName) THEN NEXT tableLoop.
    
    /* Check against filter-to-show */
    IF NOT CAN-DO(cNameShow,ttTable.cTableName) THEN NEXT tableLoop.
    
    /* User tables          : _file-number > 0   AND _file-number < 32000
     * Schema tables        : _file-number > -80 AND _file-number < 0
     * Virtual system tables: _file-number < -16384
     * SQL catalog tables   : _file-name BEGINS "_sys"
     */
    IF NOT lNormal AND ttTable.cCategory = 'Application' THEN NEXT tableLoop.
    IF NOT lSchema AND ttTable.cCategory = 'Schema'      THEN NEXT tableLoop.
    IF NOT lVst    AND ttTable.cCategory = 'VST'         THEN NEXT tableLoop.
    IF NOT lSql    AND ttTable.cCategory = 'SQL'         THEN NEXT tableLoop.
    IF NOT lOther  AND ttTable.cCategory = 'Other'       THEN NEXT tableLoop.

    /* Handling for Hidden and Frozen apply only to user tables otherwise it will be too confusing
     * because Schema, VST and SQL tables are all by default hidden and frozen. 
     */
    IF NOT lHidden AND ttTable.cCategory = 'Application' AND ttTable.lHidden = TRUE THEN NEXT tableLoop.
    IF NOT lFrozen AND ttTable.cCategory = 'Application' AND ttTable.lFrozen = TRUE THEN NEXT tableLoop.
    
    /* Fields that must be in the list */
    DO iSearch = 1 TO NUM-ENTRIES(cFieldShow):
      cSearchFld = ENTRY(iSearch,cFieldShow).
    
      /* If no wildcards used, we can simply CAN-DO */
      IF INDEX(cSearchFld,"*") = 0 THEN
      DO:
        IF NOT CAN-DO(ttTable.cFields, cSearchFld) THEN NEXT tableLoop.
      END.
      ELSE 
      DO:
        lFieldFound = FALSE.
        DO iField = 1 TO NUM-ENTRIES(ttTable.cFields):
          cThisField = ENTRY(iField,ttTable.cFields).
          IF CAN-DO(cSearchFld,cThisField) THEN 
          DO: 
            lFieldFound = TRUE.
            LEAVE.
          END. 
        END.
        IF NOT lFieldFound THEN NEXT tableLoop.
      END.
    END.       
    
    /* Fields that may not be in the list */
    DO iSearch = 1 TO NUM-ENTRIES(cFieldHide):
      cSearchFld = ENTRY(iSearch,cFieldHide).
    
      /* If no wildcards used, we can simply CAN-DO */
      IF INDEX(cSearchFld,"*") = 0 THEN
      DO:
        IF CAN-DO(ttTable.cFields, cSearchFld) THEN NEXT tableLoop.
      END.
      ELSE 
      DO:
        lRejected = FALSE.
        DO iField = 1 TO NUM-ENTRIES(ttTable.cFields):
          cThisField = ENTRY(iField,ttTable.cFields).
          IF CAN-DO(cSearchFld,cThisField) THEN  
          DO: 
            lRejected = TRUE.
            LEAVE.
          END. 
        END.
        IF lRejected THEN NEXT tableLoop.
      END.
    END.
    
    /* If we get here, we should add the table */
    ASSIGN ttTable.lShowInList = TRUE.
  END.
  
  {&timerStop}
END PROCEDURE. /* getTablesWithFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTableStats) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getTableStats Procedure 
PROCEDURE getTableStats :
/*------------------------------------------------------------------------
  Name         : getTableStats
  Description  : Get table statistics from the INI file
  ----------------------------------------------------------------------
  17-09-2012 pti Separated from getTables and getTablesWithField
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT-OUTPUT PARAMETER table FOR ttTable. 

  DEFINE VARIABLE cIniFile    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cLine       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSection    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cDatabase   AS CHARACTER   NO-UNDO.

  /* Read the ini file as plain text and parse the lines. 
   * 
   * The normal way would be to do a FOR-EACH on the _file table and 
   * retrieve the information needed. But if you have a large database 
   * (or a lot of databases), this becomes VERY slow. Searching the 
   * other way around by parsing the INI is a lot faster.
   */
  {&timerStart}

  cIniFile = substitute('&1DataDigger-&2.ini', getProgramDir(), getUserName() ).
  INPUT from value(cIniFile).
  REPEAT:
    /* Sometimes lines get screwed up and are waaaay too long
     * for the import statement. So just ignore those. 
     */
    IMPORT UNFORMATTED cLine NO-ERROR.
    IF ERROR-STATUS:ERROR THEN NEXT.

    /* Find DB sections */
    IF cLine MATCHES '[DB:*]' THEN 
    DO:
      cSection = TRIM(cLine,'[]').
      cDatabase = ENTRY(2,cSection,":").
    END. 

    /* Only process lines of database-sections */
    IF NOT cSection BEGINS "DB:" THEN NEXT.

    /* Only process setting lines */
    IF NOT cLine MATCHES '*:*=*' THEN NEXT.
    
    /* Filter out some settings */
    IF cLine MATCHES "*:QueriesServed=*" THEN
    DO:
      FIND FIRST ttTable 
        WHERE ttTable.cDatabase = cDatabase
          AND ttTable.cTableName = entry(1,cLine,':') NO-ERROR.
    
      IF AVAILABLE ttTable THEN
      DO:
        ttTable.iNumQueries = INTEGER(ENTRY(2,cLine,'=')) NO-ERROR.
        IF ttTable.iNumQueries = ? THEN ttTable.iNumQueries = 0.
      END.
    END. /* queriesServed */

    ELSE
    IF cLine MATCHES "*:LastUsed=*" THEN
    DO:
      FIND FIRST ttTable 
        WHERE ttTable.cDatabase = cDatabase
          AND ttTable.cTableName = entry(1,cLine,':') NO-ERROR.

      IF AVAILABLE ttTable THEN
      DO:
        ttTable.tLastUsed = DATETIME(ENTRY(2,cLine,'=')) NO-ERROR.
      END.
    END. /* lastUsed */

    ELSE
    IF cLine MATCHES "*:Favourite=*" THEN
    DO:
      FIND FIRST ttTable
        WHERE ttTable.cDatabase = cDatabase
          AND ttTable.cTableName = entry(1,cLine,':') NO-ERROR.

      IF AVAILABLE ttTable THEN
      DO:
        ttTable.lFavourite = TRUE NO-ERROR.
      END.
    END. /* favourite */

  END. /* repeat */
  INPUT close. 

  {&timerStop}

END PROCEDURE. /* getTableStats */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-initTableFilter) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initTableFilter Procedure 
PROCEDURE initTableFilter :
/*
 * Set table filter values back to their initial values
 */
  DEFINE INPUT-OUTPUT PARAMETER TABLE FOR ttTableFilter.

  EMPTY TEMP-TABLE ttTableFilter.
  CREATE ttTableFilter.

  /* Set visibility of schema tables */
  ttTableFilter.lShowSchema = LOGICAL(getRegistry('DataDigger','ShowHiddenTables')).
  IF ttTableFilter.lShowSchema = ? THEN ttTableFilter.lShowSchema = NO.

END PROCEDURE. /* initTableFilter */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-lockWindow) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE lockWindow Procedure 
PROCEDURE lockWindow :
/*------------------------------------------------------------------------
  Name         : lockWindow
  Description  : Lock / unlock updates that Windows does to windows. 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER phWindow AS HANDLE  NO-UNDO.
  DEFINE INPUT PARAMETER plLock   AS LOGICAL NO-UNDO.

  DEFINE VARIABLE iRet AS INTEGER NO-UNDO. 
  DEFINE BUFFER ttWindowLock FOR ttWindowLock. 

  PUBLISH "debugMessage" (3, SUBSTITUTE("Window &1, lock: &2", phWindow:TITLE, STRING(plLock,"ON/OFF"))).

  IF NOT VALID-HANDLE(phWindow) THEN RETURN.
  
  /* Find window in our tt of locked windows */
  FIND ttWindowLock WHERE ttWindowLock.hWindow = phWindow NO-ERROR.
  IF NOT AVAILABLE ttWindowLock THEN
  DO:
    /* If we try to unlock a window thats not in the tt, just go back */
    IF NOT plLock THEN RETURN. 

    /* Otherwise create a tt record for it */
    CREATE ttWindowLock.
    ttWindowLock.hWindow = phWindow.
  END.

  /* Because commands to lock or unlock may be nested, keep track
   * of the number of locks/unlocks using a semaphore.
   * 
   * The order of commands may be:
   * lockWindow(yes). -> actually lock the window
   * lockWindow(yes). -> do nothing
   * lockWindow(yes). -> do nothing
   * lockWindow(no).  -> do nothing
   * lockWindow(no).  -> do nothing
   * lockWindow(yes). -> do nothing
   * lockWindow(no).  -> do nothing
   * lockWindow(no).  -> actually unlock the window
   */
  IF plLock THEN 
    ttWindowLock.iLockCounter = ttWindowLock.iLockCounter + 1.
  ELSE 
    ttWindowLock.iLockCounter = ttWindowLock.iLockCounter - 1.

  PUBLISH "debugMessage" (3, SUBSTITUTE("Lock counter: &1", ttWindowLock.iLockCounter)).

  /* Now, only lock when the semaphore is increased to 1 */
  IF plLock AND ttWindowLock.iLockCounter = 1 THEN
  DO:
    RUN SendMessageA( phWindow:HWND /* {&window-name}:hwnd */
                    , {&WM_SETREDRAW}
                    , 0
                    , 0
                    , OUTPUT iRet
                    ).
  END.

  /* And only unlock after the last unlock command */
  ELSE IF ttWindowLock.iLockCounter <= 0 THEN
  DO:
    RUN SendMessageA( phWindow:HWND /* {&window-name}:hwnd */
                    , {&WM_SETREDRAW}
                    , 1
                    , 0
                    , OUTPUT iRet
                    ).
    
    RUN RedrawWindow( phWindow:HWND /* {&window-name}:hwnd */
                    , 0
                    , 0
                    , {&RDW_ALLCHILDREN} + {&RDW_ERASE} + {&RDW_INVALIDATE}
                    , OUTPUT iRet
                    ).

    /* Clean up tt */
    DELETE ttWindowLock.
  END. 

END PROCEDURE. /* lockWindow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-readConfigFile) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE readConfigFile Procedure 
PROCEDURE readConfigFile :
/*
 * Read the ini-file and create tt records for it
 */
  DEFINE INPUT PARAMETER pcConfigFile AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cFile      AS LONGCHAR    NO-UNDO.
  DEFINE VARIABLE cLine      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cChunk     AS LONGCHAR    NO-UNDO.
  DEFINE VARIABLE cSection   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTrimChars AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iLine      AS INTEGER     NO-UNDO.

  {&timerStart}
  DEFINE BUFFER bfConfig FOR ttConfig.
  
  /* Read file in 1 pass to memory */
  COPY-LOB FILE pcConfigFile TO cFile NO-CONVERT NO-ERROR.
  IF ERROR-STATUS:ERROR THEN cFile = readFile(pcConfigFile).

  cTrimChars = " " + CHR(1) + "~r". /* space / chr-1 / LF */

  /* Process line by line */
  #LineLoop:
  DO iLine = 1 TO NUM-ENTRIES(cFile,"~n"):

    cChunk = ENTRY(iLine,cFile,"~n").
    cChunk = SUBSTRING(cChunk, 1,20000). /* trim very long lines */
    cLine = TRIM(cChunk, cTrimChars).    /* remove junk */

    /* Section line */
    IF cLine MATCHES "[*]" THEN
    DO:
      cSection = TRIM(cLine,"[]").
      NEXT #LineLoop.
    END.

    /* Ignore weird settings within [DB:xxxx] sections */
    IF cSection BEGINS 'DB:' 
      AND NUM-ENTRIES( TRIM(ENTRY(1,cLine,"=")), ':') = 1 THEN NEXT #LineLoop.

    /* Config line */
    FIND bfConfig
      WHERE bfConfig.cSection = cSection
        AND bfConfig.cSetting = TRIM(ENTRY(1,cLine,"=")) NO-ERROR.

    IF NOT AVAILABLE bfConfig THEN
    DO:
      CREATE bfConfig.
      ASSIGN
        bfConfig.cSection = cSection
        bfConfig.cSetting = TRIM(ENTRY(1,cLine,"="))
        .
    END.

    /* Config line /might/ already exist. This can happen if you have
     * the same setting in multiple .ini files. 
     */
    ASSIGN bfConfig.cValue = TRIM(SUBSTRING(cLine, INDEX(cLine,"=") + 1)).
  END.

  {&timerStop}
END PROCEDURE. /* readConfigFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resizeFilterFields) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE resizeFilterFields Procedure 
PROCEDURE resizeFilterFields :
/*
 * Generic procedure to redraw the filter fields of the 
 * fields browse and of the index browse. 
 */
  DEFINE INPUT PARAMETER phLeadButton   AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER pcFilterFields AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pcButtons      AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER phBrowse       AS HANDLE      NO-UNDO.

  DEFINE VARIABLE iField        AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iButton       AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iCurrentPos   AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iRightEdge    AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iWidth        AS INTEGER NO-UNDO. 
  DEFINE VARIABLE hColumn       AS HANDLE  NO-UNDO. 
  DEFINE VARIABLE hButton       AS HANDLE  NO-UNDO. 
  DEFINE VARIABLE hFilterField  AS HANDLE  NO-UNDO. 
  DEFINE VARIABLE iFilter       AS INTEGER NO-UNDO. 
  DEFINE VARIABLE lChangeDetected AS LOGICAL NO-UNDO. 
  
  /* Find out if there has been a change in the browse or in one of
   * its columns. If no changes, save a little time by not redrawing
   */
  IF NOT isBrowseChanged(phBrowse) THEN RETURN.
  
  {&timerStart}

  PUBLISH "timerCommand" ("start", "resizeFilterFields:makeSmall").
  /* To prevent drawing error, make all fields small */
  DO iField = 1 TO NUM-ENTRIES(pcFilterFields):
    hFilterField = HANDLE(ENTRY(iField,pcFilterFields)).
    hFilterField:VISIBLE      = NO.
    hFilterField:X            = phBrowse:X. 
    hFilterField:Y            = phBrowse:Y - 23.
    hFilterField:WIDTH-PIXELS = 1.
  END.
  PUBLISH "timerCommand" ("stop", "resizeFilterFields:makeSmall").

  /* Start by setting the buttons at the proper place. Do this right to left */
  ASSIGN iRightEdge = phBrowse:X + phBrowse:WIDTH-PIXELS.
  DO iButton = NUM-ENTRIES(pcButtons) TO 1 BY -1:
    hButton = HANDLE(ENTRY(iButton,pcButtons)).
    hButton:X = iRightEdge - hButton:WIDTH-PIXELS.
    hButton:Y = phBrowse:Y - 23. /* filter buttons close to the browse */
    iRightEdge = hButton:X + 0. /* A little margin between buttons */
  END.

  /* The left side of the left button is the maximum point 
   * Fortunately, this value is already in iRightEdge.
   * Resize and reposition the fields from left to right, 
   * use the space between browse:x and iRightEdge
   */

  /* Take the left side of the first visible column as a starting point. */
  firstVisibleColumn:
  DO iField = 1 TO phBrowse:NUM-COLUMNS:
    hColumn = phBrowse:GET-BROWSE-COLUMN(iField):handle.

    IF hColumn:X > 0 AND hColumn:VISIBLE THEN
    DO:
      iCurrentPos = phBrowse:X + hColumn:X.
      LEAVE firstVisibleColumn.
    END.
  END.

  PUBLISH "timerCommand" ("start", "resizeFilterFields:fieldLoop").
  fieldLoop:
  DO iField = 1 TO phBrowse:NUM-COLUMNS:

    hColumn = phBrowse:GET-BROWSE-COLUMN(iField):handle.
    
    /* Some types cannot have a filter */
    IF hColumn:DATA-TYPE = 'raw' THEN NEXT. 

    iFilter = iFilter + 1.
    IF iFilter > num-entries(pcFilterFields) THEN LEAVE fieldLoop.

    /* Determine the handle of the filterfield */
    hFilterField = HANDLE(ENTRY(iFilter, pcFilterFields)).

    /* If the column is hidden, make the filter hidden and go to the next */
    IF NOT hColumn:VISIBLE THEN 
    DO:
      hFilterField:VISIBLE = NO.
      NEXT fieldLoop. 
    END.

    /* Where *are* we ?? */
    iCurrentPos = phBrowse:X + hColumn:X.

    /* If the columns have been resized, some columns might have fallen off the screen */
    IF hColumn:X < 1 THEN NEXT. 

    /* Does it fit on the screen? */
    IF iCurrentPos >= iRightEdge - 5 THEN LEAVE fieldLoop. /* accept some margin */

    /* Where will this field end? And does it fit? */
    iWidth = hColumn:WIDTH-PIXELS + 4.
    IF iCurrentPos + iWidth > iRightEdge THEN iWidth = iRightEdge - iCurrentPos.

    /* Ok, seems to fit */
    hFilterField:X            = iCurrentPos.
    hFilterField:WIDTH-PIXELS = iWidth.
    iCurrentPos               = iCurrentPos + iWidth.
    hFilterField:VISIBLE      = phBrowse:VISIBLE. /* take over the visibility of the browse */
  END.
  PUBLISH "timerCommand" ("stop", "resizeFilterFields:fieldLoop").
  
  /* Finally, set the lead button to the utmost left */
  IF VALID-HANDLE(phLeadButton) THEN
    ASSIGN 
      phLeadButton:X = phBrowse:X 
      phLeadButton:Y = phBrowse:Y - 23.

  {&timerStop}

END PROCEDURE. /* resizeFilterFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-restoreWindowPos) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE restoreWindowPos Procedure 
PROCEDURE restoreWindowPos :
/*------------------------------------------------------------------------------
  Name : restoreWindowPos
  Desc : Restore position / size of a window 
------------------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER phWindow     AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER pcWindowName AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE iValue AS INTEGER     NO-UNDO.

  iValue = INTEGER(getRegistry(pcWindowName, 'Window:x' )).
  IF iValue = ? THEN iValue = INTEGER(getRegistry('DataDigger', 'Window:x' )) + 50.
  ASSIGN phWindow:X = iValue NO-ERROR.

  iValue = INTEGER(getRegistry(pcWindowName, 'Window:y' )).
  IF iValue = ? THEN iValue = INTEGER(getRegistry('DataDigger', 'Window:y' )) + 50.
  IF iValue <> ? THEN ASSIGN phWindow:Y = iValue NO-ERROR.

  iValue = INTEGER(getRegistry(pcWindowName, 'Window:height' )).
  IF iValue = ? OR iValue = 0 THEN iValue = INTEGER(getRegistry('DataDigger', 'Window:height' )) - 100. 
  ASSIGN phWindow:HEIGHT-PIXELS = iValue NO-ERROR.

  iValue = INTEGER(getRegistry(pcWindowName, 'Window:width' )).
  IF iValue = ? OR iValue = 0 THEN iValue = INTEGER(getRegistry('DataDigger', 'Window:width' )) - 100.
  ASSIGN phWindow:WIDTH-PIXELS = iValue NO-ERROR.

  /* Force a redraw */
  APPLY 'window-resized' TO phWindow.

END PROCEDURE. /* restoreWindowPos */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-saveConfigFileSorted) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveConfigFileSorted Procedure 
PROCEDURE saveConfigFileSorted :
/*
 * Save settings file sorted
 */
  DEFINE VARIABLE cUserConfigFile AS CHARACTER NO-UNDO.
  DEFINE BUFFER bfConfig FOR ttConfig.

  cUserConfigFile = SUBSTITUTE("&1DataDigger-&2.ini", getProgramDir(), getUserName() ).

  /* Config table holds data from 3 .ini sources, so start fresh */
  EMPTY TEMP-TABLE bfConfig. 
  RUN readConfigFile(cUserConfigFile).

  /* Now write back, sorted */
  OUTPUT TO VALUE(cUserConfigFile).

  FOR EACH bfConfig 
    WHERE bfConfig.cSection BEGINS "DataDigger"
      AND bfConfig.cSetting <> ''
      AND bfConfig.cSetting <> ?
    BREAK BY bfConfig.cSection:
  
    IF FIRST-OF(bfConfig.cSection) THEN
      PUT UNFORMATTED SUBSTITUTE("[&1]",bfConfig.cSection) SKIP.
  
    PUT UNFORMATTED SUBSTITUTE("&1=&2",bfConfig.cSetting, bfConfig.cValue) SKIP.
  
    IF LAST-OF(bfConfig.cSection) THEN
      PUT UNFORMATTED SKIP(1).
  END.

  FOR EACH bfConfig 
    WHERE NOT bfConfig.cSection BEGINS "DataDigger"
      AND bfConfig.cSetting <> ''
      AND bfConfig.cSetting <> ?
    BREAK BY bfConfig.cSection:
  
    IF FIRST-OF(bfConfig.cSection) THEN
      PUT UNFORMATTED SUBSTITUTE("[&1]",bfConfig.cSection) SKIP.
  
    PUT UNFORMATTED SUBSTITUTE("&1=&2",bfConfig.cSetting, bfConfig.cValue) SKIP.
  
    IF LAST-OF(bfConfig.cSection) THEN
      PUT UNFORMATTED SKIP(1).
  END.

  OUTPUT CLOSE. 

END PROCEDURE. /* saveConfigFileSorted */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-saveQuery) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveQuery Procedure 
PROCEDURE saveQuery :
/*------------------------------------------------------------------------
  Name         : saveQuery
  Description  : Save a query to the INI file. Increase the nr of all 
                 other queries and place this one at the top
                 
  ----------------------------------------------------------------------
  18-11-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT  PARAMETER pcDatabase     AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcTable        AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcQuery        AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cQuery AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE iQuery AS INTEGER   NO-UNDO. 

  DEFINE BUFFER bQuery FOR ttQuery.
  
  {&timerStart}

  /* Prepare query for saving in ini-file */
  cQuery = pcQuery.
  cQuery = REPLACE(cQuery,'~n',CHR(1)).
  cQuery = REPLACE(cQuery,{&QUERYSEP},CHR(1)).
  IF cQuery = '' THEN cQuery = '<empty>'.

  /* Get the table with queries again, because they might be 
   * changed if the user has more than one window open.
   */
  RUN collectQueryInfo(pcDatabase, pcTable).

  /* Save current query in the tt. If it already is in the 
   * TT then just move it to the top
   */
  FIND bQuery 
    WHERE bQuery.cDatabase = pcDatabase
      AND bQuery.cTable    = pcTable 
      AND bQuery.cQueryTxt = cQuery NO-ERROR.

  IF AVAILABLE bQuery THEN 
  DO:
    ASSIGN bQuery.iQueryNr = 0.
  END.
  ELSE 
  DO:
    CREATE bQuery.
    ASSIGN bQuery.cDatabase = pcDatabase 
           bQuery.cTable    = pcTable   
           bQuery.iQueryNr  = 0
           bQuery.cQueryTxt = cQuery.
  END.

  /* The ttQuery temp-table is already filled, renumber it */
  iQuery = 0.
  REPEAT PRESELECT EACH bQuery 
    WHERE bQuery.cDatabase = pcDatabase
      AND bQuery.cTable    = pcTable 
       BY bQuery.iQueryNr:

    FIND NEXT bQuery.
    ASSIGN 
      iQuery          = iQuery + 1
      bQuery.iQueryNr = iQuery.
  END.

  /* And save it to the INI-file */
  RUN saveQueryTable(table bQuery, pcDatabase, pcTable).

  {&timerStop}
END PROCEDURE. /* saveQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-saveQueryTable) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveQueryTable Procedure 
PROCEDURE saveQueryTable :
/*------------------------------------------------------------------------
  Name         : saveQueryTable
  Description  : Save the queries in the TT to the INI file with a max
                 of MaxQueryHistory
                 
  ----------------------------------------------------------------------
  18-11-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT  PARAMETER table FOR ttQuery.
  DEFINE INPUT  PARAMETER pcDatabase     AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcTable        AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE iMaxQueryHistory AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iQuery           AS INTEGER NO-UNDO. 
  DEFINE VARIABLE cSetting         AS CHARACTER NO-UNDO. 

  DEFINE BUFFER bQuery FOR ttQuery.
 
  {&timerStart}
 
  iMaxQueryHistory = integer(getRegistry("DataDigger", "MaxQueryHistory" )).
  IF iMaxQueryHistory = 0 THEN RETURN. /* no query history wanted */
 
  /* If it is not defined use default setting */
  IF iMaxQueryHistory = ? THEN iMaxQueryHistory = 10. 

  iQuery = 1.

  saveQuery:
  FOR EACH bQuery 
    WHERE bQuery.cDatabase = pcDatabase
      AND bQuery.cTable    = pcTable 
       BY bQuery.iQueryNr:
    
    cSetting = bQuery.cQueryTxt.
    IF cSetting = '' THEN NEXT. /* cSetting = '<empty>' */

    setRegistry( SUBSTITUTE("DB:&1", pcDatabase)
               , SUBSTITUTE('&1:query:&2', pcTable, iQuery)
               , cSetting).
    iQuery = iQuery + 1.
    IF iQuery > iMaxQueryHistory THEN LEAVE saveQuery.
  END.

  /* Delete higher nrs than MaxQueryHistory */
  DO WHILE iQuery <= iMaxQueryHistory:
 
    setRegistry( SUBSTITUTE("DB:&1", pcDatabase)
               , SUBSTITUTE('&1:query:&2', pcTable, iQuery)
               , ?).
    iQuery = iQuery + 1.
  END. /* iQuery .. MaxQueryHistory */
  
  {&timerStop}
END PROCEDURE. /* saveQueryTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-saveWindowPos) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE saveWindowPos Procedure 
PROCEDURE saveWindowPos :
/*
 * Save position / size of a window 
 */
 
  DEFINE INPUT PARAMETER phWindow     AS HANDLE      NO-UNDO.
  DEFINE INPUT PARAMETER pcWindowName AS CHARACTER   NO-UNDO.

  setRegistry(pcWindowName, "Window:x"     , STRING(phWindow:X) ).
  setRegistry(pcWindowName, "Window:y"     , STRING(phWindow:Y) ).
  setRegistry(pcWindowName, "Window:height", STRING(phWindow:HEIGHT-PIXELS) ).
  setRegistry(pcWindowName, "Window:width" , STRING(phWindow:WIDTH-PIXELS) ).

END PROCEDURE. /* saveWindowPos */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setCaching) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setCaching Procedure 
PROCEDURE setCaching :
/*
 * Set the cache vars for the library
 */

  glCacheTableDefs = LOGICAL( getRegistry("DataDigger:Cache","TableDefs") ).
  glCacheFieldDefs = LOGICAL( getRegistry("DataDigger:Cache","FieldDefs") ).
  glCacheSettings  = LOGICAL( getRegistry("DataDigger:Cache","Settings")  ).

END PROCEDURE. /* setCaching */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setLabelPosition) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setLabelPosition Procedure 
PROCEDURE setLabelPosition :
/*
 * Correct the position of the label for larger fonts 
 */
  DEFINE INPUT PARAMETER phWidget AS HANDLE NO-UNDO.

  /* Move horizontally far enough from the widget */
  phWidget:SIDE-LABEL-HANDLE:X = phWidget:X 
    - FONT-TABLE:GET-TEXT-WIDTH-PIXELS(phWidget:SIDE-LABEL-HANDLE:SCREEN-VALUE, phWidget:FRAME:FONT)
    - (IF phWidget:TYPE = 'fill-in' THEN 5 ELSE 0)
    .

END PROCEDURE. /* setLabelPosition */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setSortArrow) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setSortArrow Procedure 
PROCEDURE setSortArrow :
/*------------------------------------------------------------------------
  Name         : setSortArrow
  Description  : Set the sorting arrow on a browse
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER phBrowse    AS HANDLE    NO-UNDO. 
  DEFINE INPUT PARAMETER pcSortField AS CHARACTER NO-UNDO. 
  DEFINE INPUT PARAMETER plAscending AS LOGICAL   NO-UNDO. 

  DEFINE VARIABLE iColumn    AS INTEGER   NO-UNDO.
  DEFINE VARIABLE hColumn    AS HANDLE    NO-UNDO.
  DEFINE VARIABLE lSortFound AS LOGICAL   NO-UNDO.
  
  {&timerStart}

  do iColumn = 1 to phBrowse:num-columns:
    hColumn = phBrowse:get-browse-column(iColumn).

    /* If you apply the sort to the same column, the order 
     * of sorting is inverted.
     */
    IF hColumn:NAME = pcSortField THEN 
    DO:
      phBrowse:set-sort-arrow(iColumn, plAscending ).
      lSortFound = TRUE.

      /* Setting is one of: ColumnSortFields | ColumnSortIndexes | ColumnSortTables */
      setRegistry( 'DataDigger'
                 , SUBSTITUTE('ColumnSort&1', SUBSTRING(phBrowse:name,3))  
                 , SUBSTITUTE('&1,&2',iColumn, plAscending)
                 ).
    END.
    ELSE 
      phBrowse:set-sort-arrow(iColumn, ? ). /* erase existing arrow */
  END.
  
  /* If no sort is found, delete setting */
  IF NOT lSortFound THEN
    setRegistry( 'DataDigger', SUBSTITUTE('ColumnSort&1', SUBSTRING(phBrowse:name,3)), ?).

  {&timerStop}

END PROCEDURE. /* setSortArrow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setTransparency) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setTransparency Procedure 
PROCEDURE setTransparency :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER phFrame AS HANDLE     NO-UNDO.
  DEFINE INPUT  PARAMETER piLevel AS INTEGER    NO-UNDO.
  
  &scop GWL_EXSTYLE         -20
  &scop WS_EX_LAYERED       524288
  &scop LWA_ALPHA           2
  &scop WS_EX_TRANSPARENT   32
  
  DEFINE VARIABLE stat AS INTEGER    NO-UNDO.

  /* Set WS_EX_LAYERED on this window  */
  RUN SetWindowLongA(phFrame:HWND, {&GWL_EXSTYLE}, {&WS_EX_LAYERED}, OUTPUT stat).

  /* Make this window transparent (0 - 255) */
  RUN SetLayeredWindowAttributes(phFrame:HWND, 0, piLevel, {&LWA_ALPHA}, OUTPUT stat).

END PROCEDURE. /* setTransparency */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setUsage) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setUsage Procedure 
PROCEDURE setUsage :
/*------------------------------------------------------------------------
  Name         : setUsage
  Description  : Save DataDigger usage in the INI file
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER pcUsageId AS CHARACTER NO-UNDO.

  DEFINE VARIABLE iNumUsed   AS INTEGER NO-UNDO.
  DEFINE VARIABLE iNumDays   AS INTEGER NO-UNDO.
  DEFINE VARIABLE cEventType AS CHARACTER   NO-UNDO.

  /* Save DataDigger usage in the INI file
   *
   * [DataDigger:Usage]
   * btnAdd:lastDate  = 2013-05-06      /* last date it is used */
   * btnAdd:numUsed   = 32              /* nr of times it is used */
   * btnAdd:numDays   = 2               /* nr of days on which it is used */
   * btnAdd:eventType = keypress        /* how was it invoked */
   *
   * Valid event types are:
   *   "KEYPRESS" - Keyboard event identified by key label, such as ESC, CTRL+A, or A.
   *   "MOUSE"    - Portable or three-button mouse event, such as MOUSE-SELECT-UP or LEFT-MOUSE-UP.
   *   "PROGRESS" - High-level ABL event. These include all events identified as direct manipulation,
   *                key function, developer, and other miscellaneous events, such as SELECTION,
   *                DELETE-LINE, U1, or CHOOSE.
   */
  cEventType = getRegistry("DataDigger:Usage", SUBSTITUTE("&1:eventType", pcUsageId)).
  IF cEventType = ? THEN cEventType = "".
  IF LOOKUP(LAST-EVENT:EVENT-TYPE, cEventType) = 0 THEN
  DO:
    cEventType = TRIM(cEventType + "," + LAST-EVENT:EVENT-TYPE,",").
    setRegistry("DataDigger:Usage", SUBSTITUTE("&1:eventType", pcUsageId), cEventType).
  END.

  iNumDays = INTEGER(getRegistry("DataDigger:Usage", SUBSTITUTE("&1:numDays", pcUsageId))).
  IF iNumDays = ? THEN iNumDays = 0.

  /* Update lastDate and numDays only first time per day */
  IF getRegistry("DataDigger:Usage", SUBSTITUTE("&1:lastDate" , pcUsageId)) <> ISO-DATE(TODAY) THEN
  DO:
    /* Num days on which the function is used */
    iNumDays = iNumDays + 1.
    setRegistry("DataDigger:Usage", SUBSTITUTE("&1:numDays" , pcUsageId), STRING(iNumDays)).
  
    /* Date last used */
    setRegistry("DataDigger:Usage", SUBSTITUTE("&1:lastDate" , pcUsageId), ISO-DATE(TODAY)).
  END.

  /* Number of times used */
  iNumUsed = INTEGER(getRegistry("DataDigger:Usage", SUBSTITUTE("&1:numUsed", pcUsageId))).
  IF iNumUsed = ? THEN iNumUsed = 0.
  iNumUsed = iNumUsed + 1.
  setRegistry("DataDigger:Usage", SUBSTITUTE("&1:numUsed", pcUsageId), STRING(iNumUsed)).

END PROCEDURE. /* setUsage */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setXmlNodeNames) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setXmlNodeNames Procedure 
PROCEDURE setXmlNodeNames :
/* 
 * Set the XML-NODE-NAMES of all fields in a buffer
 */
  DEFINE INPUT PARAMETER phTable AS HANDLE NO-UNDO.
  DEFINE VARIABLE iField AS INTEGER NO-UNDO.

  DO iField = 1 TO phTable:NUM-FIELDS:
    phTable:BUFFER-FIELD(iField):XML-NODE-NAME = getXmlNodeName(phTable:BUFFER-FIELD(iField):NAME).
  END.

END PROCEDURE. /* setXmlNodeNames */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-showHelp) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showHelp Procedure 
PROCEDURE showHelp :
DEFINE INPUT  PARAMETER pcTopic   AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcStrings AS CHARACTER   NO-UNDO.

  DEFINE VARIABLE cButtons       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cMessage       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cMsg           AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cPrg           AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTitle         AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cType          AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cUrl           AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cCanHide       AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iButtonPressed AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lAnswer        AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lDontShowAgain AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lCanHide       AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lHidden        AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE iString        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE cUserString    AS CHARACTER   NO-UNDO EXTENT 9.

  /* If no message, then just return */
  cMessage = getRegistry('DataDigger:help', pcTopic + ':message').

  /* What to start? */
  cUrl = getRegistry('DataDigger:help', pcTopic + ':url').
  cPrg = getRegistry('DataDigger:help', pcTopic + ':program').
  cCanHide = getRegistry('DataDigger:help', pcTopic + ':canHide').
  cCanHide = TRIM(cCanHide).
  lCanHide = LOGICAL(cCanHide) NO-ERROR.
  IF lCanHide = ? THEN lCanHide = TRUE.

  IF cMessage = ? THEN 
  DO:
    IF cUrl = ? AND cPrg = ? THEN RETURN.
    lHidden        = YES. /* suppress empty text window */
    iButtonPressed = 1.   /* forces to start the url or prog */
  END.

  /* If type is unknown, set to QUESTION if there is a question mark in the message */
  cType    = getRegistry('DataDigger:help', pcTopic + ':type').
  IF cType = ? THEN cType = (IF cMessage MATCHES '*?*' THEN 'Question' ELSE 'Message').

  /* If no button labels defined, set them based on message type */
  cButtons = getRegistry('DataDigger:help', pcTopic + ':buttons').
  IF cButtons = ? THEN cButtons = (IF cType = 'Question' THEN '&Yes,&No,&Cancel' ELSE '&Ok').

  /* If title is empty, set it to the type of the message */
  cTitle   = getRegistry('DataDigger:help', pcTopic + ':title').
  IF cTitle = ? THEN cTitle = cType.
  
  /* If hidden has strange value, set it to NO */
  lHidden = LOGICAL(getRegistry('DataDigger:help', pcTopic + ':hidden')) NO-ERROR.
  IF lHidden = ? THEN lHidden = NO.
  
  /* If ButtonPressed has strange value, set hidden to NO */
  iButtonPressed = INTEGER( getRegistry('DataDigger:help',pcTopic + ':answer') ) NO-ERROR.
  IF iButtonPressed = ? THEN lHidden = NO.
  
  /* if we have no message, but we do have an URL or prog, then
   * dont show an empty message box.
   */
  IF cMessage = ? THEN
    ASSIGN 
      lHidden        = YES /* suppress empty text window */
      iButtonPressed = 1.   /* forces to start the url or prog */

  /* Fill in strings in message */
  DO iString = 1 TO NUM-ENTRIES(pcStrings):
    cUserString[iString] = ENTRY(iString,pcStrings).
  END.

  cMessage = SUBSTITUTE( cMessage
                       , cUserString[1]
                       , cUserString[2]
                       , cUserString[3]
                       , cUserString[4]
                       , cUserString[5]
                       , cUserString[6]
                       , cUserString[7]
                       , cUserString[8]
                       , cUserString[9]
                       ).

  /* If not hidden, show the message and let the user choose an answer */
  IF NOT lHidden THEN
  DO:
    RUN VALUE( getProgramDir() + 'dQuestion.w')
      ( INPUT cTitle
      , INPUT cMessage
      , INPUT cButtons
      , INPUT lCanHide
      , OUTPUT iButtonPressed
      , OUTPUT lDontShowAgain
      ).
      
    IF lDontShowAgain THEN
      setRegistry('DataDigger:help', pcTopic + ':hidden', 'yes').
  END.
  
  /* Start external things if needed */                                            
  IF iButtonPressed = 1 THEN
  DO:
    IF cUrl <> ? THEN OS-COMMAND NO-WAIT START (cUrl).
    IF cPrg <> ? THEN RUN VALUE(cPrg) NO-ERROR.
  END.
  
  /* Save answer */
  setRegistry('DataDigger:help',pcTopic + ':answer', STRING(iButtonPressed)).
      
END PROCEDURE. /* showHelp */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-showScrollbars) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showScrollbars Procedure 
PROCEDURE showScrollbars :
/*------------------------------------------------------------------------
  Name         : showScrollbars
  Description  : Hide or show scrollbars the hard way
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER ip-Frame      AS HANDLE  NO-UNDO.
  DEFINE INPUT PARAMETER ip-horizontal AS LOGICAL NO-UNDO.
  DEFINE INPUT PARAMETER ip-vertical   AS LOGICAL NO-UNDO.

  DEFINE VARIABLE iv-retint AS INTEGER NO-UNDO.
  
  {&timerStart}
  
  IF NOT VALID-HANDLE(ip-Frame) OR ip-Frame:HWND = ? THEN RETURN.
  
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

  {&timerStop}
END PROCEDURE. /* ShowScrollbars */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-unlockWindow) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE unlockWindow Procedure 
PROCEDURE unlockWindow :
/*------------------------------------------------------------------------
  Name         : unlockWindow
  Description  : Force a window to unlock 
  ----------------------------------------------------------------------
  26-11-2013 pti Created
  ----------------------------------------------------------------------*/

  DEFINE INPUT PARAMETER phWindow AS HANDLE  NO-UNDO.

  DEFINE VARIABLE iRet AS INTEGER NO-UNDO. 
  DEFINE BUFFER ttWindowLock FOR ttWindowLock. 

  PUBLISH "debugMessage" (3, SUBSTITUTE("Window &1, force to unlock", phWindow:TITLE)).

  /* Find window in our tt of locked windows */
  FIND ttWindowLock WHERE ttWindowLock.hWindow = phWindow NO-ERROR.
  IF NOT AVAILABLE ttWindowLock THEN RETURN. 

  IF ttWindowLock.iLockCounter > 0 THEN
  DO:
    RUN SendMessageA( phWindow:HWND /* {&window-name}:hwnd */
                    , {&WM_SETREDRAW}
                    , 1
                    , 0
                    , OUTPUT iRet
                    ).
    
    RUN RedrawWindow( phWindow:HWND /* {&window-name}:hwnd */
                    , 0
                    , 0
                    , {&RDW_ALLCHILDREN} + {&RDW_ERASE} + {&RDW_INVALIDATE}
                    , OUTPUT iRet
                    ).

    /* Clean up tt */
    DELETE ttWindowLock.
  END. 

END PROCEDURE. /* unlockWindow */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-updateFields) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE updateFields Procedure 
PROCEDURE updateFields :
/*------------------------------------------------------------------------
  Name         : updateFields
  Description  : Update the fields temp-table with settings from registry
  ---------------------------------------------------------------------- 
  22-10-2012 pti Created
  24-10-2014 pti Place newly created fields at the bottom
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER pcDatabase    AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pcTableName   AS CHARACTER   NO-UNDO.
  DEFINE INPUT-OUTPUT PARAMETER TABLE FOR ttField.

  DEFINE VARIABLE cCustomFormat      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSelectedFields    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldOrder        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lSaveDataFilters   AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lShow              AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE iColumnOrder       AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iFieldOrder        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMaxExtent         AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lRecRowAtEnd       AS LOGICAL     NO-UNDO.

  DEFINE BUFFER bField FOR ttField.
  DEFINE BUFFER bColumn FOR ttColumn.

  {&timerStart}

  PUBLISH "debugMessage" (1, SUBSTITUTE("Update field definitions for &1.&2", pcDatabase, pcTableName)).

  /* Get list of all previously selected fields */
  cSelectedFields = getRegistry(SUBSTITUTE("DB:&1",pcDatabase), SUBSTITUTE("&1:fields",pcTableName)).
  IF cSelectedFields = ? THEN cSelectedFields = '*'.

  /* Get field ordering */
  cFieldOrder = getRegistry(SUBSTITUTE('DB:&1',pcDatabase), SUBSTITUTE('&1:fieldOrder',pcTableName)).

  /* RECID and ROWID at the end? */
  IF cFieldOrder <> ? THEN
  DO:
    lRecRowAtEnd = LOOKUP("ROWID", cFieldOrder) > NUM-ENTRIES(cFieldOrder) - 2 AND LOOKUP("RECID", cFieldOrder) > NUM-ENTRIES(cFieldOrder) - 2.
    PUBLISH "debugMessage" (2, SUBSTITUTE("Field order for table &1: &2", pcTableName, cFieldOrder)).
    PUBLISH "debugMessage" (3, SUBSTITUTE("Rowid/recid at the end for table &1: &2", pcTableName, lRecRowAtEnd)).
  END.

  /* Saved data filters? */
  lSaveDataFilters = LOGICAL(getRegistry ("DataDigger", "SaveDataFilters")).

  FOR EACH bField:

    /* Due to a bug the nr of decimals may be set on non-decimal fields
     * See PKB P185263 (article 18087) for more information
     * http://knowledgebase.progress.com/articles/Article/P185263
     */
    IF bField.cDataType <> 'DECIMAL' THEN bField.iDecimals = ?.

    /* Was this field selected? */
    bField.lShow = CAN-DO(cSelectedFields, bField.cFullName).

    /* Customization option for the user to show/hide certain fields */
    PUBLISH 'customShowField' (pcDatabase, pcTableName, bField.cFieldName, INPUT-OUTPUT bField.lShow).

    /* Customization option for the user to adjust the format */
    PUBLISH 'customFormat' (pcDatabase, pcTableName, bField.cFieldName, bField.cDatatype, INPUT-OUTPUT bField.cFormat).
    
    /* Restore changed field format. */
    cCustomFormat = getRegistry( SUBSTITUTE("DB:&1",pcDatabase)
                               , SUBSTITUTE("&1.&2:format",pcTableName,bField.cFieldName) ).
    IF cCustomFormat <> ? THEN bField.cFormat = cCustomFormat.

    /* Restore changed field order. */
    bField.iOrder = LOOKUP(bField.cFullName,cFieldOrder).
    IF bField.iOrder = ? THEN bField.iOrder = bField.iOrderOrg.

    /* Keep track of highest nr */
    iFieldOrder = MAXIMUM(iFieldOrder,bField.iOrder).

    /* RECID / ROWID field visibility might be changed */
/*     IF LOOKUP(bField.cFullName, "RECID,ROWID") > 0 THEN                                          */
/*       bField.lShow = LOGICAL(getRegistry ("DataDigger", "AddDataColumnFor" + bField.cFullName)). */

  END. /* f/e bField */

  /* Only show first X of an extent */
  iMaxExtent = INTEGER(getRegistry("DataDigger","MaxExtent")) NO-ERROR.
  IF iMaxExtent = ? THEN iMaxExtent = 100.
  IF iMaxExtent > 0 THEN
  FOR EACH bColumn WHERE bColumn.iExtent > iMaxExtent:
    DELETE bColumn.
  END.


  IF CAN-FIND(FIRST bField WHERE bField.iOrder = 0) THEN
  DO:
    /* Set new fields (no order assigned) at the end */
    FOR EACH bField WHERE bField.iOrder = 0 BY bField.iFieldRpos: 
       ASSIGN 
         iFieldOrder   = iFieldOrder + 1
         bField.iOrder = iFieldOrder.      
    END.

    /* If RECID+ROWID should be at the end then re-assign them */
    IF lRecRowAtEnd THEN
    FOR EACH bField 
      WHERE bField.cFieldName = "RECID" OR bField.cFieldName = "ROWID" BY bField.iOrder:
      ASSIGN 
        iFieldOrder   = iFieldOrder + 1
        bField.iOrder = iFieldOrder.
    END. 
  END.

  /* Reorder fields to get rid of gaps */
  iFieldOrder = 0.
  REPEAT PRESELECT EACH bField BY bField.iOrder:
    FIND NEXT bField.
    ASSIGN 
      iFieldOrder   = iFieldOrder + 1
      bField.iOrder = iFieldOrder.
  END.

  /* Assign order nrs to columns to handle extents */
  iColumnOrder = 0.
  FOR EACH bField BY bField.iOrder:
    FOR EACH bColumn WHERE bColumn.cFieldName =  bField.cFieldName BY bColumn.cFieldName:
      iColumnOrder = iColumnOrder + 1.
      bColumn.iColumnNr = iColumnOrder.
    END.
  END.

  {&timerStop}

END PROCEDURE. /* updateFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-updateMemoryCache) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE updateMemoryCache Procedure 
PROCEDURE updateMemoryCache :
/*------------------------------------------------------------------------
  Name         : updateMemoryCache
  Description  : Update the memory cache with current settings 
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER pcDatabase    AS CHARACTER NO-UNDO.
  DEFINE INPUT PARAMETER pcTableName   AS CHARACTER NO-UNDO.
  DEFINE INPUT PARAMETER TABLE FOR ttField.
  DEFINE INPUT PARAMETER TABLE FOR ttColumn.

  DEFINE BUFFER bField  FOR ttField. 
  DEFINE BUFFER bColumn FOR ttColumn.
  DEFINE BUFFER bFieldCache  FOR ttFieldCache. 
  DEFINE BUFFER bColumnCache FOR ttColumnCache.

  PUBLISH "debugMessage" (2, SUBSTITUTE("Update first-level cache for &1.&2", pcDatabase, pcTableName)).

  /* Delete old */
  FOR EACH bFieldCache 
    WHERE bFieldCache.cDatabase  = pcDatabase
      AND bFieldCache.cTableName = pcTableName:

    DELETE bFieldCache.
  END.

  FOR EACH bColumnCache 
    WHERE bColumnCache.cDatabase  = pcDatabase
      AND bColumnCache.cTableName = pcTableName:

    DELETE bColumnCache.
  END.

  /* Create new */
  FOR EACH bField:
    CREATE bFieldCache.
    BUFFER-COPY bField TO bFieldCache.
  END.
  FOR EACH bColumn:
    CREATE bColumnCache.
    BUFFER-COPY bColumn TO bColumnCache.
  END.

END PROCEDURE. /* updateMemoryCache */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

/* ************************  Function Implementations ***************** */

&IF DEFINED(EXCLUDE-addConnection) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION addConnection Procedure 
FUNCTION addConnection RETURNS LOGICAL
  ( pcDatabase AS CHARACTER 
  , pcSection  AS CHARACTER ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
    
define temp-table ttDatabase no-undo rcode-information
  field cLogicalName  as character column-label "Logical Name" format "x(20)"
  field cSection      as character column-label "Section"      format "x(20)"
  index iPrim is primary unique cLogicalName
  .
------------------------------------------------------------------------------*/

  IF NOT CAN-FIND(ttDatabase WHERE ttDatabase.cLogicalName = pcDatabase) THEN
  DO:
    CREATE ttDatabase.
    ASSIGN 
      ttDatabase.cLogicalName  = pcDatabase
      ttDatabase.cSection      = pcSection
      . 

  END.

  RETURN TRUE.

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-formatQueryString) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION formatQueryString Procedure 
FUNCTION formatQueryString RETURNS CHARACTER
  ( INPUT pcQueryString AS CHARACTER
  , INPUT plExpanded    AS LOGICAL ) :

/*------------------------------------------------------------------------------
  Purpose: formatQueryString
    Notes: return a properly formatted query string
------------------------------------------------------------------------------*/
  DEFINE VARIABLE cReturnValue AS CHARACTER   NO-UNDO.

  {&timerStart}
  
  cReturnValue = pcQueryString.

  IF cReturnValue <> '' AND cReturnValue <> ? THEN 
  DO:
    /* There might be chr(1) chars in the text (if read from ini, for example)
     * Replace these with normal CRLF, then proceed 
     */
    cReturnValue = REPLACE(cReturnValue,CHR(1),'~n').

    IF plExpanded THEN
      cReturnValue = REPLACE(cReturnValue, {&QUERYSEP}, '~n').
    ELSE
      cReturnValue = REPLACE(cReturnValue, '~n', {&QUERYSEP}).
  END.

  {&timerStop}
  return cReturnValue.

END FUNCTION. /* formatQueryString */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColor) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getColor Procedure 
FUNCTION getColor RETURNS INTEGER
  ( pcName AS CHARACTER ) :

/*------------------------------------------------------------------------
  Name         : getColor
  Description  : Return the color number for the name given
                  
                 pcName = Name of the UI element eg 'DataOddRow' 
  ---------------------------------------------------------------------- 
  25-03-2011 pti Created
  ----------------------------------------------------------------------*/
  DEFINE VARIABLE iColor AS INTEGER NO-UNDO. 

  {&timerStart}
  
  /* Get the setting for this color name */
  iColor = INTEGER(getRegistry('DataDigger:colors', pcName)) NO-ERROR.
  
  /* Default colors if it is unknown */
  IF iColor = ? THEN 
  DO:
    CASE pcName:
      WHEN 'CustomFormat:fg'           THEN iColor = 12. /* red       */
      WHEN 'CustomOrder:fg'            THEN iColor = 12. /* red       */
      WHEN 'DataRow:even:bg'           THEN iColor =  8. /* lightgray */
      WHEN 'DataRow:even:fg'           THEN iColor =  0. /* black     */
      WHEN 'DataRow:odd:bg'            THEN iColor = 15. /* white     */
      WHEN 'DataRow:odd:fg'            THEN iColor =  0. /* black     */
      WHEN 'FilterBox:bg'              THEN iColor = 12. /* red       */
      WHEN 'IndexInactive:fg'          THEN iColor = 12. /* red       */
      WHEN 'PrimIndex:bg'              THEN iColor =  8. /* lightgray */
      WHEN 'QueryCounter:fg'           THEN iColor =  7. /* darkgray  */
      WHEN 'QueryError:bg'             THEN iColor = 12. /* red       */
      WHEN 'QueryError:fg'             THEN iColor = 14. /* yellow    */
      WHEN 'QueryInfo:fg'              THEN iColor =  7. /* darkgray  */
      WHEN 'RecordCount:Complete:fg'   THEN iColor =  2. /* green     */
      WHEN 'RecordCount:Incomplete:fg' THEN iColor = 12. /* red       */
      WHEN 'RecordCount:Selected:fg'   THEN iColor =  7. /* darkgray */
      WHEN 'WarningBox:bg'             THEN iColor = 14. /* yellow    */
      WHEN 'WarningBox:fg'             THEN iColor = 12. /* red       */
      WHEN 'FieldFilter:bg'            THEN iColor = 14. /* yellow    */
      WHEN 'FieldFilter:fg'            THEN iColor =  9. /* blue      */
    END CASE.

    /* Save it, so the next time it comes from the settings */ 
    IF iColor <> ? THEN setRegistry('DataDigger:colors', pcName, STRING(iColor)).
  END.

  {&timerStop}
  RETURN iColor.   /* Function return value. */

END FUNCTION. /* getColor */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColumnLabel) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getColumnLabel Procedure 
FUNCTION getColumnLabel RETURNS CHARACTER
  ( INPUT phFieldBuffer AS HANDLE ):

  DEFINE VARIABLE cColumnLabel AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTemplate    AS CHARACTER   NO-UNDO.
  
  {&timerStart}
  
  cTemplate = getRegistry("DataDigger","ColumnLabelTemplate").
  IF cTemplate = ? OR cTemplate = "" THEN cTemplate = "&1".

  cColumnLabel = SUBSTITUTE(cTemplate
                           , phFieldBuffer::cFullName
                           , phFieldBuffer::iOrder
                           , phFieldBuffer::cLabel
                           ).
  {&timerStop}
  return cColumnLabel.

END FUNCTION. /* getColumnLabel */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColumnWidthList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getColumnWidthList Procedure 
FUNCTION getColumnWidthList RETURNS CHARACTER
  ( INPUT phBrowse AS HANDLE ):

  /* returns a list of all fields and their width like:
   * custnum:12,custname:20,city:12
   */
  DEFINE VARIABLE cWidthList AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hColumn    AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iColumn    AS INTEGER     NO-UNDO.
  
  {&timerStart}
  
  do iColumn = 1 to phBrowse:num-columns:
  
    hColumn = phBrowse:get-browse-column(iColumn).
    cWidthList = SUBSTITUTE('&1,&2:&3'
                           , cWidthList 
                           , hColumn:NAME
                           , hColumn:WIDTH-PIXELS
                           ).
  END.
  
  {&timerStop}
  return trim(cWidthList,','). 
END FUNCTION. /* getColumnWidthList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getDatabaseList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getDatabaseList Procedure 
FUNCTION getDatabaseList RETURNS CHARACTER:

/*------------------------------------------------------------------------
  Name         : getDatabaseList
  Description  : Return a comma separated list of all connected datbases
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE cDatabaseList  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSchemaHolders AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iCount         AS INTEGER     NO-UNDO.

  {&timerStart}
  
  /* Make a list of schema holders */
  DO iCount = 1 TO NUM-DBS:
    IF DBTYPE(iCount) <> 'PROGRESS' THEN cSchemaHolders = cSchemaHolders + ',' + SDBNAME(iCount).
  END.

  /* And a list of all databases. If a database is in the list of schemaholders
   * we don't want to see it here. */
  DO iCount = 1 to NUM-DBS:
    IF LOOKUP(LDBNAME(iCount),cSchemaHolders) > 0 THEN NEXT.
    cDatabaseList = cDatabaseList + ',' + LDBNAME(iCount).
  END.

  {&timerStop}
  RETURN TRIM(cDatabaseList,',').

END FUNCTION. /* getDatabaseList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getEscapedData) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getEscapedData Procedure 
FUNCTION getEscapedData RETURNS CHARACTER
  ( pcTarget AS CHARACTER
  , pcString AS CHARACTER ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/
  DEFINE VARIABLE cOutput AS CHARACTER NO-UNDO.
  DEFINE VARIABLE iTmp    AS INTEGER   NO-UNDO.

  {&timerStart}
  
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
        cOutput = REPLACE(cOutput, CHR(iTmp), "' + chr(" + string(iTmp) + ") + '").
      END.
    END.
  END CASE.

  {&timerStop}
  RETURN pcString.

END FUNCTION. /* getEscapedData */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getFileCategory) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getFileCategory Procedure 
FUNCTION getFileCategory RETURNS CHARACTER
  ( piFileNumber AS INTEGER
  , pcFileName   AS CHARACTER
  ) :
/*
 * Purpose: Based on name and number, return the category for a table
 */

  /* 
   * Application tables   : _file-number > 0   AND _file-number < 32000
   * Schema tables        : _file-number > -80 AND _file-number < 0
   * Virtual system tables: _file-number < -16384
   * SQL catalog tables   : _file-name BEGINS "_sys"
   * Other tables         : _file-number >= -16384 AND _file-number <= -80
   */
  IF piFileNumber > 0       AND piFileNumber < 32000 THEN RETURN 'Normal'.
  IF piFileNumber > -80     AND piFileNumber < 0     THEN RETURN 'Schema'.
  IF piFileNumber < -16384                           THEN RETURN 'VST'.
  IF pcFileName BEGINS '_sys'                        THEN RETURN 'SQL'.
  IF piFileNumber >= -16384 AND piFileNumber <= -80  THEN RETURN 'Other'.

  RETURN ''.   /* Function return value. */
  
END FUNCTION. /* getFileCategory */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getFont) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getFont Procedure 
FUNCTION getFont RETURNS INTEGER
  ( pcFontName AS CHARACTER ) :

/*------------------------------------------------------------------------
  Name         : getFont
  Description  : Return the fontnumber for the type given
  ---------------------------------------------------------------------- 
  25-03-2011 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE iFontNr AS INTEGER NO-UNDO. 

  {&timerStart}
  iFontNr = integer(getRegistry('DataDigger:Fonts',pcFontName)) no-error.

  IF iFontNr = ? OR iFontNr > 23 THEN 
  CASE pcFontName:
    WHEN 'Default' THEN iFontNr = 4.
    WHEN 'Fixed'   THEN iFontNr = 0.
  END CASE.

  {&timerStop}
  return iFontNr.   /* Function return value. */

END FUNCTION. /* getFont */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getImagePath) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getImagePath Procedure 
FUNCTION getImagePath RETURNS CHARACTER
  ( pcImage AS CHARACTER ) :
  
/*------------------------------------------------------------------------
  Name         : getImagePath
  Description  : Return the image path + icon set name 
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE cImagePath AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cIconSet   AS CHARACTER   NO-UNDO.
  
  {&timerStart}
  cIconSet = 'default'.

  cImagePath = SUBSTITUTE('&1Image/&2_&3'
                         , getProgramDir()
                         , cIconSet
                         , pcImage
                         ).

  /* Fall back to the default icon set when image not found */
  IF SEARCH(cImagePath) = ? THEN
    cImagePath = SUBSTITUTE('&1Image/default_&2'
                           , getProgramDir()
                           , pcImage
                           ).
  {&timerStop}
  return cImagePath.
END FUNCTION. /* getImagePath */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getIndexFields) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getIndexFields Procedure 
FUNCTION getIndexFields RETURNS CHARACTER
  ( INPUT pcDatabaseName AS CHARACTER
  , INPUT pcTableName    AS CHARACTER  
  , INPUT pcFlags        AS CHARACTER
  ) :
/*------------------------------------------------------------------------
  Name         : getIndexFields
  Description  : Return the index fields of a table.
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  18-12-2012 pti Merged with similar functions
  ----------------------------------------------------------------------*/
  
  DEFINE VARIABLE cWhere            AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hQuery            AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFieldBuffer      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hFileBuffer       AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hIndexBuffer      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE hIndexFieldBuffer AS HANDLE      NO-UNDO.
  DEFINE VARIABLE cFieldList        AS CHARACTER   NO-UNDO.
  
  {&timerStart}
  
  create buffer hFileBuffer       for table pcDatabaseName + "._File".
  CREATE BUFFER hIndexBuffer      FOR TABLE pcDatabaseName + "._Index".
  CREATE BUFFER hIndexFieldBuffer FOR TABLE pcDatabaseName + "._Index-Field".
  CREATE BUFFER hFieldBuffer      FOR TABLE pcDatabaseName + "._Field".
  
  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hFileBuffer,hIndexBuffer,hIndexFieldBuffer,hFieldBuffer).
 
  cWhere = SUBSTITUTE("FOR EACH &1._file WHERE &1._file._file-name = &2 AND _File._File-Number < 32768, ~
                           EACH &1._index       OF &1._file WHERE TRUE &3 &4,  ~
                           EACH &1._index-field OF &1._index,            ~
                           EACH &1._field       OF &1._index-field"
                     , pcDatabaseName
                     , QUOTER(pcTableName)
                     , (IF CAN-DO(pcFlags,"U") THEN "AND _index._unique = true" ELSE "")
                     , (IF CAN-DO(pcFlags,"P") THEN "AND recid(_index) = _file._prime-index" ELSE "")
                     ).
  
  IF hQuery:QUERY-PREPARE (cWhere) THEN 
  DO:
    hQuery:QUERY-OPEN().
    hQuery:GET-FIRST(NO-LOCK).
    REPEAT WHILE NOT hQuery:QUERY-OFF-END:
      cFieldList = cFieldList + "," + trim(hFieldBuffer:BUFFER-FIELD("_field-name"):string-value).
      hQuery:GET-NEXT(NO-LOCK).
    END.
  END.
  
  cFieldList = TRIM(cFieldList, ",").
  
  hQuery:QUERY-CLOSE. 
  
  DELETE OBJECT hFileBuffer.
  DELETE OBJECT hIndexBuffer.
  DELETE OBJECT hIndexFieldBuffer.
  DELETE OBJECT hFieldBuffer.
  DELETE OBJECT hQuery.
  
  {&timerStop}
  return cFieldList.   /* Function return value. */

END FUNCTION. /* getIndexFields */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getKeyList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getKeyList Procedure 
FUNCTION getKeyList RETURNS CHARACTER
  ( /* parameter-definitions */ ) :

/*------------------------------------------------------------------------
  Name         : getKeyList
  Description  : Return a list of special keys pressed 
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE L-KBSTATE AS MEMPTR NO-UNDO. 
  DEFINE VARIABLE L-RETURNVALUE AS INTEGER NO-UNDO. 
  DEFINE VARIABLE L-SHIFTLIST AS CHARACTER NO-UNDO. 
  
  SET-SIZE(L-KBSTATE) = 256.
  
  /* Get the current state of the keyboard */ 
  RUN GetKeyboardState(GET-POINTER-VALUE(L-KBSTATE), OUTPUT L-RETURNVALUE). 
  
  IF GET-BITS(GET-BYTE(L-KBSTATE, 1 + 16), 8, 1) = 1 
  THEN L-SHIFTLIST = TRIM(L-SHIFTLIST + ",SHIFT",",").
  IF GET-BITS(GET-BYTE(L-KBSTATE, 1 + 17), 8, 1) = 1 
  THEN L-SHIFTLIST = TRIM(L-SHIFTLIST + ",CTRL",",").
  IF GET-BITS(GET-BYTE(L-KBSTATE, 1 + 18), 8, 1) = 1 
  THEN L-SHIFTLIST = TRIM(L-SHIFTLIST + ",ALT",",").
  
  SET-SIZE(L-KBSTATE) = 0. 
  
  RETURN L-SHIFTLIST.   /* Function return value. */ 

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getLinkInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getLinkInfo Procedure 
FUNCTION getLinkInfo RETURNS CHARACTER
  ( INPUT pcFieldName AS CHARACTER
  ):

/*------------------------------------------------------------------------
  Name         : getLinkInfo
  Description  : Save name/value of a field.
  ----------------------------------------------------------------------
  21-10-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE BUFFER bLinkInfo FOR ttLinkInfo.
  {&timerStart}
  find bLinkInfo where bLinkInfo.cField = pcFieldName no-error.
  {&timerStop}
  return (if available bLinkInfo then bLinkInfo.cValue else "").

END FUNCTION. /* getLinkInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMatchesValue) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getMatchesValue Procedure 
FUNCTION getMatchesValue RETURNS CHARACTER
  ( hFillIn AS HANDLE ) :

/*------------------------------------------------------------------------
  Name         : getMatchesValue
  Description  : Transform the value of a fillin to something we can use
                 with the MATCHES function. 

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE cValue AS CHARACTER NO-UNDO. 

  cValue = hFillIn:SCREEN-VALUE. 
  IF cValue = hFillIn:PRIVATE-DATA THEN cValue = ''.

  IF cValue = ? OR cValue = '' THEN cValue = '*'.
  ELSE 
  IF    INDEX(cValue,'*') = 0 
    AND index(cValue,'.') = 0 THEN 
    cValue = '*' + cValue + '*'.

  RETURN cValue.   /* Function return value. */

END FUNCTION. /* getMatchesValue */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMaxLength) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getMaxLength Procedure 
FUNCTION getMaxLength RETURNS INTEGER
  ( cFieldList AS CHARACTER ) :

/*------------------------------------------------------------------------
  Name         : getMaxLength
  Description  : Return the length of the longest element in a comma 
                 separated list
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/
  DEFINE VARIABLE iField     AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iMaxLength AS INTEGER NO-UNDO. 

  {&timerStart}
  
  /* Get max field length */
  do iField = 1 to num-entries(cFieldList):
    iMaxLength = MAXIMUM(iMaxLength,LENGTH(ENTRY(iField,cFieldList))).
  END.

  {&timerStop}
  return iMaxLength.   /* Function return value. */

END FUNCTION. /* getMaxLength */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getOsErrorDesc) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getOsErrorDesc Procedure 
FUNCTION getOsErrorDesc RETURNS CHARACTER
  (INPUT piOsError AS INTEGER):

/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  CASE piOsError:
    WHEN   0 THEN RETURN "No error                 ".
    WHEN   1 THEN RETURN "Not owner                ".
    WHEN   2 THEN RETURN "No such file or directory".
    WHEN   3 THEN RETURN "Interrupted system call  ".
    WHEN   4 THEN RETURN "I/O error                ".
    WHEN   5 THEN RETURN "Bad file number          ".
    WHEN   6 THEN RETURN "No more processes        ".
    WHEN   7 THEN RETURN "Not enough core memory   ".
    WHEN   8 THEN RETURN "Permission denied        ".
    WHEN   9 THEN RETURN "Bad address              ".
    WHEN  10 THEN RETURN "File exists              ".
    WHEN  11 THEN RETURN "No such device           ".
    WHEN  12 THEN RETURN "Not a directory          ".
    WHEN  13 THEN RETURN "Is a directory           ".
    WHEN  14 THEN RETURN "File table overflow      ".
    WHEN  15 THEN RETURN "Too many open files      ".
    WHEN  16 THEN RETURN "File too large           ".
    WHEN  17 THEN RETURN "No space left on device  ".
    WHEN  18 THEN RETURN "Directory not empty      ".
    OTHERWISE RETURN "Unmapped error           ".
  END CASE.

END FUNCTION. /* getOsErrorDesc */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getProgramDir) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getProgramDir Procedure 
FUNCTION getProgramDir RETURNS CHARACTER
  ( /* parameter-definitions */ ) :

/*------------------------------------------------------------------------
  Name         : getProgramDir
  Description  : Return the DataDigger install dir, including a backslash
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE cProgDir AS CHARACTER NO-UNDO.

  /* this-procedure:file-name will return the .p name without path when the 
   * procedure us run without full path. We need to seek it in the propath.
   */
  FILE-INFO:FILE-NAME = THIS-PROCEDURE:FILE-NAME.
  IF FILE-INFO:FULL-PATHNAME = ? THEN
  DO:
    IF SUBSTRING(THIS-PROCEDURE:FILE-NAME,LENGTH(THIS-PROCEDURE:FILE-NAME) - 1, 2) = ".p" THEN 
      FILE-INFO:FILE-NAME = SUBSTRING(THIS-PROCEDURE:FILE-NAME,1,LENGTH(THIS-PROCEDURE:FILE-NAME) - 2) + ".r".
  END.
  
  cProgDir = SUBSTRING(FILE-INFO:FULL-PATHNAME,1,R-INDEX(FILE-INFO:FULL-PATHNAME,'\')).
  PUBLISH "message" 
    ( 50
    , cProgDir
    ).

  RETURN cProgDir. /* Function return value. */

END FUNCTION. /* getProgramDir */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getQuery) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getQuery Procedure 
FUNCTION getQuery RETURNS CHARACTER
  ( INPUT pcDatabase AS CHARACTER
  , INPUT pcTable    AS CHARACTER
  , INPUT piQuery    AS INTEGER
  ) :

  DEFINE BUFFER bQuery FOR ttQuery.

  FIND bQuery 
    WHERE bQuery.cDatabase = pcDatabase
      AND bQuery.cTable    = pcTable
      AND bQuery.iQueryNr  = piQuery NO-ERROR.

  IF AVAILABLE bQuery THEN 
    RETURN bQuery.cQueryTxt.
  ELSE
    RETURN ?.

END FUNCTION. /* getQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getReadableQuery) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getReadableQuery Procedure 
FUNCTION getReadableQuery RETURNS CHARACTER
  ( INPUT pcQuery AS CHARACTER ):

  /* Name: getReadableQuery
   * Desc: Return a query as a string that is readable for humans. 
   * 
   *       message getReadableQuery( hQuery:prepare-string ) view-as alert-box.
   *       
   *       message getReadableQuery( string(hQuery) ) view-as alert-box.
   */
  DEFINE VARIABLE hQuery AS HANDLE      NO-UNDO.
  
  /* Accept query or query-handle */
  hQuery = WIDGET-HANDLE(pcQuery) NO-ERROR.
  IF VALID-HANDLE( hQuery ) THEN
  DO:
    hQuery = WIDGET-HANDLE(pcQuery).
    pcQuery = hQuery:PREPARE-STRING.
  END.
  
  pcQuery = REPLACE(pcQuery,' EACH ' ,' EACH ').
  pcQuery = REPLACE(pcQuery,' FIRST ',' FIRST ').
  pcQuery = REPLACE(pcQuery,' WHERE ',  '~n  WHERE ').
  pcQuery = REPLACE(pcQuery,' AND '  ,  '~n    AND ').
  pcQuery = REPLACE(pcQuery,' BY '   ,  '~n     BY ').
  pcQuery = REPLACE(pcQuery,' FIELDS ()','').
  pcQuery = REPLACE(pcQuery,'FOR EACH ' ,'FOR EACH ').
  pcQuery = REPLACE(pcQuery,' NO-LOCK',  ' NO-LOCK').
  pcQuery = REPLACE(pcQuery,' INDEXED-REPOSITION',  '').

  .pcQuery = pcQuery + '~n'.

  RETURN pcQuery.
END FUNCTION. /* getReadableQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getRegistry Procedure 
FUNCTION getRegistry RETURNS CHARACTER
    ( pcSection AS CHARACTER
    , pcKey     AS CHARACTER 
    ) :

/*------------------------------------------------------------------------
  Name         : getRegistry 
  Description  : Get a value from the registry. 
  ---------------------------------------------------------------------- 
  15-01-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE cValue AS CHARACTER NO-UNDO.
  DEFINE VARIABLE lValue AS LOGICAL   NO-UNDO.

  {&timerStart}
  DEFINE BUFFER bfConfig FOR ttConfig.

  /* If this is a DB-specific section then replace db name if needed */
  if pcSection begins "DB:" then
  do:
    FIND ttDatabase WHERE ttDatabase.cLogicalName = entry(2,pcSection,":") NO-ERROR.
    IF AVAILABLE ttDatabase THEN pcSection = "DB:" + ttDatabase.cSection.
  END.


  IF glCacheSettings THEN
  DO:
    /* Load settings if there is nothing in the config table */
    IF NOT TEMP-TABLE ttConfig:HAS-RECORDS THEN
    DO:
      /* Help file is least important */
      RUN readConfigFile( SUBSTITUTE("&1DataDiggerHelp.ini"
                                    , getProgramDir()
                                    )).
      /* General DD settings */
      RUN readConfigFile( SUBSTITUTE("&1DataDigger.ini"
                                    , getProgramDir()
                                    )).
      /* Per-user settings */
      RUN readConfigFile( SUBSTITUTE("&1DataDigger-&2.ini"
                                    , getProgramDir()
                                    , getUserName()
                                    )).
  
      /* When all ini-files have been read, we can determine whether 
       * caching needs to be enabled
       */
      lValue = LOGICAL(getRegistry("DataDigger:Cache","Settings")) NO-ERROR.
      IF lValue <> ? THEN ASSIGN glCacheSettings = lValue.

      lValue = LOGICAL(getRegistry("DataDigger:Cache","TableDefs")) NO-ERROR.
      IF lValue <> ? THEN ASSIGN glCacheTableDefs = lValue.

      /* If we do not want to cache the registry, empty it now */
      IF NOT glCacheSettings THEN RUN clearRegistryCache.
    END.

    /* Search in settings tt */
    FIND bfConfig
      WHERE bfConfig.cSection = pcSection
        AND bfConfig.cSetting = pcKey
            NO-ERROR.
            
    {&timerStop}
    RETURN ( IF AVAILABLE bfConfig THEN bfConfig.cValue ELSE ? ).
  END.

  ELSE 
  DO:  
    USE SUBSTITUTE('DataDigger-&1', getUserName() ).
    GET-KEY-VALUE 
      SECTION pcSection
      KEY     pcKey
      VALUE   cValue.
  
    /* If setting is not in the personal INI file
     * then check the default DataDigger.ini
     */
    IF cValue = ? THEN
    DO:
      USE 'DataDigger'.
      GET-KEY-VALUE 
        SECTION pcSection
        KEY     pcKey
        VALUE   cValue.
    END.
  
    /* And if it is still not found, look in 
     * the DataDiggerHelp ini file 
     */
    IF cValue = ? THEN
    DO:
      USE 'DataDiggerHelp'.
      GET-KEY-VALUE 
        SECTION pcSection
        KEY     pcKey
        VALUE   cValue.
    END. 
  
    /* Clean up and return */
    USE "".
    {&timerStop}
    return cValue.
  END.

END FUNCTION. /* getRegistry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getStackSize) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getStackSize Procedure 
FUNCTION getStackSize RETURNS INTEGER():
  DEFINE VARIABLE cList      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cParm      AS CHARACTER   CASE-SENSITIVE NO-UNDO.
  DEFINE VARIABLE cSetting   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cValue     AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iOption    AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iParm      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iStackSize AS INTEGER     NO-UNDO.
  
  cList = SESSION:STARTUP-PARAMETERS.
  
  DO iParm = 1 TO NUM-ENTRIES(cList):
    cSetting = ENTRY(iParm,cList) + " ".
    cParm    = ENTRY(1,cSetting," ").
    cValue   = ENTRY(2,cSetting," ").
  
    IF cParm = "-s" THEN 
      iStackSize = INTEGER(cValue).
  END.

  /* If not defined, report the default */
  IF iStackSize = 0 THEN iStackSize = 40.

  RETURN iStackSize.
END FUNCTION. /* getStackSize */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTableList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getTableList Procedure 
FUNCTION getTableList RETURNS CHARACTER
  ( INPUT  pcDatabaseFilter AS CHARACTER
  , INPUT  pcTableFilter    AS CHARACTER
  ) :

/* Get a list of all tables in the current 
 * database that match a certain filter 
 */
  DEFINE VARIABLE cTableList  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cQuery      AS CHARACTER   NO-UNDO.

  DEFINE BUFFER bTable FOR ttTable.
  DEFINE QUERY qTable FOR bTable.

  {&timerStart}
  IF pcDatabaseFilter = '' OR pcDatabaseFilter = ? THEN pcDatabaseFilter = '*'.

  /* Build query */
  cQuery = SUBSTITUTE('for each bTable where cDatabase matches &1', QUOTER(pcDatabaseFilter)).
  cQuery = SUBSTITUTE("&1 and cTableName matches &2", cQuery, QUOTER(pcTableFilter )).

  QUERY qTable:QUERY-PREPARE( SUBSTITUTE('&1 by cTableName', cQuery)).
  QUERY qTable:QUERY-OPEN.
  QUERY qTable:GET-FIRST.

  /* All fields */
  REPEAT WHILE NOT QUERY qTable:QUERY-OFF-END:
    cTableList = cTableList + "," + bTable.cTableName.
    QUERY qTable:GET-NEXT.
  END.
  QUERY qTable:QUERY-CLOSE.

  cTableList = LEFT-TRIM(cTableList, ",").

  {&timerStop}
  RETURN cTableList.   /* Function return value. */
  
END FUNCTION. /* getTableList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getUserName) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getUserName Procedure 
FUNCTION getUserName RETURNS CHARACTER
  ( /* parameter-definitions */ ) :

  DEFINE VARIABLE cUserName AS LONGCHAR   NO-UNDO.
  DEFINE VARIABLE intResult AS INTEGER    NO-UNDO.
  DEFINE VARIABLE intSize   AS INTEGER    NO-UNDO.
  DEFINE VARIABLE mUserId   AS MEMPTR     NO-UNDO.

  /* Otherwise determine the value */
  SET-SIZE(mUserId) = 256.
  intSize = 255.

  RUN GetUserNameA ( INPUT mUserId
                   , INPUT-OUTPUT intSize
                   , OUTPUT intResult).

  COPY-LOB mUserId FOR (intSize - 1) TO cUserName NO-CONVERT.

  IF intResult <> 1 THEN
    cUserName = "default".
  ELSE
    cUserName = REPLACE(cUserName,".","").

  RETURN STRING(cUserName). /* Function return value. */

END FUNCTION. /* getUserName */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getWidgetUnderMouse) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getWidgetUnderMouse Procedure 
FUNCTION getWidgetUnderMouse RETURNS HANDLE
  ( phFrame AS HANDLE ) :

  DEFINE VARIABLE hWidget AS HANDLE  NO-UNDO.
  DEFINE VARIABLE iMouseX AS INTEGER NO-UNDO.
  DEFINE VARIABLE iMouseY AS INTEGER NO-UNDO.

  {&timerStart}
  hWidget = phFrame:first-child:first-child. 
  RUN getMouseXY(INPUT phFrame, OUTPUT iMouseX, OUTPUT iMouseY).

  REPEAT WHILE VALID-HANDLE(hWidget):

    IF hWidget:TYPE <> "RECTANGLE"
      AND iMouseX >= hWidget:X
      AND iMouseX <= hWidget:X + hWidget:WIDTH-PIXELS
      AND iMouseY >= hWidget:Y
      AND iMouseY <= hWidget:Y + hWidget:HEIGHT-PIXELS THEN RETURN hWidget.

    hWidget = hWidget:NEXT-SIBLING.
  END. 

  {&timerStop}
  return ?.

END FUNCTION. /* getWidgetUnderMouse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getXmlNodeName) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getXmlNodeName Procedure 
FUNCTION getXmlNodeName RETURNS CHARACTER
  ( pcFieldName AS CHARACTER ) :
  /* Return a name that is safe to use in XML output by
   * replacing forbidden characters with an underscore
   */

  pcFieldName = REPLACE(pcFieldName,'%', '_').
  pcFieldName = REPLACE(pcFieldName,'#', '_').

  RETURN pcFieldName. 

END FUNCTION. /* getXmlNodeName */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isBrowseChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isBrowseChanged Procedure 
FUNCTION isBrowseChanged RETURNS LOGICAL
  ( INPUT phBrowse AS HANDLE ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  DEFINE VARIABLE iField  AS INTEGER NO-UNDO.
  DEFINE VARIABLE hColumn AS HANDLE  NO-UNDO.

  IF NOT VALID-HANDLE(phBrowse) THEN RETURN FALSE. 
  IF phBrowse:TYPE <> "BROWSE" THEN RETURN FALSE. 

  {&TimerStart}

  /* First check the browse itself */
  if isWidgetChanged(phBrowse) then return true. 

  DO iField = 1 TO phBrowse:NUM-COLUMNS:
    hColumn = phBrowse:GET-BROWSE-COLUMN(iField):handle.
    IF isWidgetChanged(hColumn) THEN RETURN TRUE. 
  END. /* browse */

  /* apparently nothing changed, so... */
  PUBLISH "debugMessage" (2, SUBSTITUTE("Nothing changed in browse: &1", phBrowse:NAME)).

  {&TimerStop}
  return false. 

END FUNCTION. /* isBrowseChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isDefaultFontsChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isDefaultFontsChanged Procedure 
FUNCTION isDefaultFontsChanged RETURNS LOGICAL
  ( /* parameter-definitions */ ) :

/*------------------------------------------------------------------------
  Name         : isDefaultFontsChanged
  Description  : Returns whether the default fonts 0-7 were changed.

  ----------------------------------------------------------------------
  26-04-2010 pti Created
  ----------------------------------------------------------------------*/

  DEFINE VARIABLE cFontSize     AS CHARACTER   NO-UNDO EXTENT 8.
  DEFINE VARIABLE iFont         AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lFontsChanged AS LOGICAL     NO-UNDO.
  
  /* These are the expected fontsizes of the text 'DataDigger' */
  cFontSize[1] = '70/14'. /* font0 */
  cFontSize[2] = '54/13'. /* font1 */
  cFontSize[3] = '70/14'. /* font2 */
  cFontSize[4] = '70/14'. /* font3 */
  cFontSize[5] = '54/13'. /* font4 */
  cFontSize[6] = '70/16'. /* font5 */
  cFontSize[7] = '65/13'. /* font6 */
  cFontSize[8] = '54/13'. /* font7 */
  
  /* Innocent until proven guilty */
  lFontsChanged = NO.

  checkFont:
  DO iFont = 0 TO 7:
    IF cFontSize[iFont + 1] <> substitute('&1/&2'
                                         , FONT-TABLE:GET-TEXT-WIDTH-PIXELS('DataDigger',iFont) 
                                         , FONT-TABLE:GET-TEXT-HEIGHT-PIXELS(iFont)
                                         ) THEN 
    DO:
      lFontsChanged = TRUE.
      LEAVE checkFont.
    END.
  END. /* checkFont */

  RETURN lFontsChanged.

END FUNCTION. /* isDefaultFontsChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isFileLocked) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isFileLocked Procedure 
FUNCTION isFileLocked RETURNS LOGICAL
  ( pcFileName AS CHARACTER ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  DEFINE VARIABLE lpSecurityAtt AS INTEGER NO-UNDO.
  DEFINE VARIABLE iFileHandle   AS INTEGER NO-UNDO.
  DEFINE VARIABLE nReturn       AS INTEGER NO-UNDO.
  
  /* Try to lock the file agains writing */
  RUN CreateFileA ( INPUT pcFileName
                  , INPUT {&GENERIC_WRITE}
                  , {&FILE_SHARE_READ}
                  , lpSecurityAtt
                  , {&OPEN_EXISTING}
                  , {&FILE_ATTRIBUTE_NORMAL}
                  , 0
                  , OUTPUT iFileHandle
                  ).
  
  /* Release file handle */
  RUN CloseHandle ( INPUT iFileHandle
                  , OUTPUT nReturn
                  ).

  RETURN (iFileHandle = -1).

END FUNCTION. /* isFileLocked */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isMouseOver) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isMouseOver Procedure 
FUNCTION isMouseOver RETURNS LOGICAL
  ( phWidget AS HANDLE ) :

  DEFINE VARIABLE iMouseX AS INTEGER   NO-UNDO.
  DEFINE VARIABLE iMouseY AS INTEGER   NO-UNDO.

  IF NOT VALID-HANDLE(phWidget) THEN RETURN FALSE. 

  RUN getMouseXY ( INPUT phWidget:FRAME
                 , OUTPUT iMouseX
                 , OUTPUT iMouseY
                 ).

  RETURN (    iMouseX >= phWidget:X 
          AND iMouseX <= phWidget:X + phWidget:WIDTH-PIXELS 
          AND iMouseY >= phWidget:Y 
          AND iMouseY <= phWidget:Y + phWidget:HEIGHT-PIXELS ).

END FUNCTION. /* isMouseOver */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isTableFilterUsed) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isTableFilterUsed Procedure 
FUNCTION isTableFilterUsed RETURNS LOGICAL
  ( INPUT TABLE ttTableFilter ) :
  /* Returns whether any setting is used for table filtering 
   */
  FIND ttTableFilter NO-ERROR.
  IF NOT AVAILABLE ttTableFilter THEN RETURN FALSE. 

  /* Main toggles */
  IF   ttTableFilter.lShowNormal = FALSE 
    OR ttTableFilter.lShowSchema <> LOGICAL(getRegistry('DataDigger','ShowHiddenTables')) 
    OR ttTableFilter.lShowVst    = TRUE
    OR ttTableFilter.lShowSql    = TRUE
    OR ttTableFilter.lShowOther  = TRUE
    OR ttTableFilter.lShowHidden = TRUE 
    OR ttTableFilter.lShowFrozen = TRUE THEN RETURN TRUE. 

  /* Show these tables */
  IF   ttTableFilter.cTableNameShow <> ?
    AND ttTableFilter.cTableNameShow <> ''
    AND ttTableFilter.cTableNameShow <> '*' THEN RETURN TRUE. 

  /* But hide these */
  IF   ttTableFilter.cTableNameHide <> ?
    AND ttTableFilter.cTableNameHide <> '' THEN RETURN TRUE. 

  /* Show only tables that contain all of these fields */
  IF    ttTableFilter.cTableFieldShow <> ?
    AND ttTableFilter.cTableFieldShow <> ''
    AND ttTableFilter.cTableFieldShow <> '*' THEN RETURN TRUE. 

  /* But hide tables that contain any of these */
  IF    ttTableFilter.cTableFieldHide <> ?
    AND ttTableFilter.cTableFieldHide <> '' THEN RETURN TRUE. 

  /* else */
  RETURN FALSE. 

END FUNCTION. /* isTableFilterUsed */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isValidCodePage) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isValidCodePage Procedure 
FUNCTION isValidCodePage RETURNS LOGICAL
  (pcCodepage AS CHARACTER):
  /* Returns whether pcCodePage is valid
  */
  DEFINE VARIABLE cDummy AS LONGCHAR NO-UNDO.
  
  IF pcCodePage = '' THEN RETURN TRUE.

  FIX-CODEPAGE(cDummy) = pcCodepage NO-ERROR.
  RETURN NOT ERROR-STATUS:ERROR.

END FUNCTION. /* isValidCodePage */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isWidgetChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isWidgetChanged Procedure 
FUNCTION isWidgetChanged RETURNS LOGICAL
  ( INPUT phWidget AS HANDLE ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/
  DEFINE VARIABLE lChangeDetected AS LOGICAL NO-UNDO. 
  DEFINE VARIABLE iField          AS INTEGER NO-UNDO.
  DEFINE VARIABLE hColumn         AS HANDLE  NO-UNDO.
  DEFINE VARIABLE hFirstColumn    AS HANDLE  NO-UNDO.

  DEFINE BUFFER ttWidget FOR ttWidget.

  IF NOT VALID-HANDLE(phWidget) THEN RETURN FALSE. 
  {&timerStart}

  find ttWidget where ttWidget.hWidget = phWidget no-error.
  IF NOT AVAILABLE ttWidget THEN 
  DO:
    CREATE ttWidget.
    ASSIGN ttWidget.hWidget = phWidget.
  END.

  PUBLISH "debugMessage" (3, SUBSTITUTE("Widget: &1 &2", phWidget:TYPE, phWidget:NAME)).

  IF ttWidget.iPosX     <> phWidget:X
  OR ttWidget.iWidth <> phWidget:WIDTH-PIXELS THEN
  DO:
    ASSIGN
      ttWidget.iPosX     = phWidget:X
      ttWidget.iWidth    = phWidget:WIDTH-PIXELS
      lChangeDetected    = TRUE.
  END.

  PUBLISH "debugMessage" (2, SUBSTITUTE("  Widget changed: &1", lChangeDetected)).

  return lChangeDetected.

  FINALLY:
    {&TimerStop}
  END FINALLY.
END FUNCTION. /* isWidgetChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-readFile) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION readFile Procedure 
FUNCTION readFile RETURNS LONGCHAR
  (pcFilename AS CHARACTER):
  /* read contents of a file as a longchar. This is used
   * when the COPY-LOB statement fails as a result of 
   * conflicting code-pages. 
   */
  DEFINE VARIABLE cContent AS LONGCHAR  NO-UNDO.
  DEFINE VARIABLE cLine    AS CHARACTER NO-UNDO.

  INPUT FROM VALUE(pcFilename).
  REPEAT:
    IMPORT UNFORMATTED cLine.
    cContent = cContent + "~n" + cLine.
  END.
  INPUT CLOSE. 

  RETURN cContent.
END FUNCTION. /* readFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-removeConnection) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION removeConnection Procedure 
FUNCTION removeConnection RETURNS LOGICAL
  ( pcDatabase AS CHARACTER ) :
  /*------------------------------------------------------------------------------
    Purpose:  
      Notes:  
      
  define temp-table ttDatabase no-undo rcode-information
    field cLogicalName  as character column-label "Logical Name" format "x(20)"
    field cSection      as character column-label "Section"      format "x(20)"
    index iPrim is primary unique cLogicalName
    .
  ------------------------------------------------------------------------------*/
  FIND ttDatabase WHERE ttDatabase.cLogicalName = pcDatabase NO-ERROR.
  IF AVAILABLE ttDatabase THEN DELETE ttDatabase.

  RETURN TRUE.

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resolveOsVars) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION resolveOsVars Procedure 
FUNCTION resolveOsVars RETURNS CHARACTER
  ( pcString AS CHARACTER ) :

  DEFINE VARIABLE iPercStart   AS INTEGER NO-UNDO. 
  DEFINE VARIABLE iPercEnd     AS INTEGER NO-UNDO. 
  DEFINE VARIABLE cEnvVarName  AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cEnvVarValue AS CHARACTER NO-UNDO. 
  DEFINE VARIABLE cReturnValue AS CHARACTER NO-UNDO. 


  /* Support for OS-directives between % eg: %username% will expand
   * to your username, as long as you have an OS-var for that. Type 'set' on the dos prompt for 
   * a list of all environment variables
   */
  cReturnValue = pcString.
  iPercStart = INDEX(cReturnValue,'%').
  
  resolveOsVars:
  DO WHILE iPercStart > 0:
    iPercEnd = INDEX(cReturnValue,'%',iPercStart + 1).
    
    IF iPercEnd = 0 THEN LEAVE resolveOsVars. /* single % */
    cEnvVarName = TRIM( SUBSTRING(cReturnValue,iPercStart, iPercEnd - iPercStart) ,'%'). /* Grab text between % */
    
    /* Search in the registry */
    LOAD "System" BASE-KEY "HKEY_LOCAL_MACHINE".
    USE "System".
    GET-KEY-VALUE SECTION "CurrentControlSet~\Control~\Session Manager~\Environment" KEY cEnvVarName VALUE cEnvVarValue.
    UNLOAD "System".
    
    /* If not defined, try our luck in the default env */
    IF cEnvVarValue = ? THEN
      cEnvVarValue = OS-GETENV(cEnvVarName) . /* try to resolve */
    
    /* If still not found, step to next % */
    IF cEnvVarValue = ? THEN
    DO:
      iPercStart = iPercEnd.
      NEXT resolveOsVars.  
    END.
    
    cReturnValue = REPLACE(cReturnValue,'%' + cEnvVarName + '%', cEnvVarValue). /* Replace with value */
    iPercStart = INDEX(cReturnValue,'%'). /* Find next directive */
  END.

  RETURN cReturnValue.

END FUNCTION. /* resolveOsVars */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resolveSequence) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION resolveSequence Procedure 
FUNCTION resolveSequence RETURNS CHARACTER
  ( pcString AS CHARACTER ) :

  DEFINE VARIABLE iFileNr       AS INTEGER    NO-UNDO.
  DEFINE VARIABLE cSeqMask      AS CHARACTER  NO-UNDO .
  DEFINE VARIABLE cSeqFormat    AS CHARACTER  NO-UNDO .
  DEFINE VARIABLE cFileName     AS CHARACTER   NO-UNDO.

  cFileName = pcString.

  /* User can specify a sequence for the file. The length of 
   * the tag sets the format: <###> translates to a 3-digit nr
   * Special case is <#> which translates to no leading zeros
   */ 
  IF    INDEX(cFileName,'<#') > 0 
    AND index(cFileName,'#>') > 0 THEN
  DO:
    cSeqMask = SUBSTRING(cFileName,INDEX(cFileName,'<#')). /* <#####>tralalala */
    cSeqMask = SUBSTRING(cSeqMask,1,INDEX(cSeqMask,'>')). /* <#####> */
    cSeqFormat = TRIM(cSeqMask,'<>'). /* ##### */
    cSeqFormat = REPLACE(cSeqFormat,'#','9').
    IF cSeqFormat = '9' THEN cSeqFormat = '>>>>>>>>>9'.

    setFileNr:
    REPEAT:
      iFileNr = iFileNr + 1.
      IF SEARCH(REPLACE(cFileName,cSeqMask,TRIM(STRING(iFileNr,cSeqFormat)))) = ? THEN 
      DO:
        cFileName = REPLACE(cFileName,cSeqMask,TRIM(STRING(iFileNr,cSeqFormat))).
        LEAVE setFileNr.
      END.
    END.
  END.

  RETURN cFileName.

END FUNCTION. /* resolveSequence */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setColumnWidthList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setColumnWidthList Procedure 
FUNCTION setColumnWidthList RETURNS LOGICAL
  ( INPUT phBrowse    AS HANDLE 
  , INPUT pcWidthList AS CHARACTER):

  /* set all specified columns in pcWidthList to a specified width like:
   * custnum:12,custname:20,city:12
   */
  DEFINE VARIABLE cColumnName  AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cListItem    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE hColumn      AS HANDLE      NO-UNDO.
  DEFINE VARIABLE iColumn      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iColumnWidth AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iListItem    AS INTEGER     NO-UNDO.
  
  DO iListItem = 1 TO NUM-ENTRIES(pcWidthList):
    cListItem    = ENTRY(iListItem,pcWidthList).
    cColumnName  = ENTRY(1,cListItem,':') NO-ERROR.
    iColumnWidth = INTEGER(ENTRY(2,cListItem,':')) NO-ERROR.
    
    DO iColumn = 1 TO phBrowse:NUM-COLUMNS:
      hColumn = phBrowse:GET-BROWSE-COLUMN(iColumn).
      IF hColumn:NAME = cColumnName THEN
        hColumn:WIDTH-PIXELS = iColumnWidth.
    END. /* iColumn */
  END. /* iListItem */
  
  RETURN TRUE. 
END FUNCTION. /* setColumnWidthList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setFilterFieldColor) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setFilterFieldColor Procedure 
FUNCTION setFilterFieldColor RETURNS LOGICAL
  ( phWidget AS HANDLE ) :

/*------------------------------------------------------------------------
  Name         : setFilterFieldColor
  Description  : If you enter the field and you have not put in a filter
                 clear out the field so you can type something yourself. 
                 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  IF NOT VALID-HANDLE(phWidget) THEN MESSAGE "DEBUG ALARM" VIEW-AS ALERT-BOX.
  
  IF phWidget:SCREEN-VALUE = phWidget:PRIVATE-DATA THEN 
    phWidget:FGCOLOR = 7.
  ELSE 
    phWidget:FGCOLOR = ?.
    
  /* phWidget:bgcolor = ?. */

  RETURN TRUE.

END FUNCTION. /* setFilterFieldColor */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setLinkInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setLinkInfo Procedure 
FUNCTION setLinkInfo RETURNS LOGICAL
  ( INPUT pcFieldName AS CHARACTER
  , INPUT pcValue     AS CHARACTER
  ):

/*------------------------------------------------------------------------
  Name         : setLinkInfo
  Description  : Save name/value of a field.
  ----------------------------------------------------------------------
  21-10-2009 pti Created
  ----------------------------------------------------------------------*/

  DEFINE BUFFER bLinkInfo FOR ttLinkInfo. 

  {&timerStart}

  publish "debugMessage" (2, substitute("Set linkinfo for field &1 to &2", pcFieldName, pcValue)).

  FIND bLinkInfo WHERE bLinkInfo.cField = pcFieldName NO-ERROR.
  IF NOT AVAILABLE bLinkInfo THEN
  DO:
    CREATE bLinkInfo.
    ASSIGN bLinkInfo.cField = pcFieldName.
  END.

  bLinkInfo.cValue = TRIM(pcValue).
  
  {&timerStop}
  return true.   /* Function return value. */

END FUNCTION. /* setLinkInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setRegistry Procedure 
FUNCTION setRegistry RETURNS CHARACTER
    ( pcSection AS CHARACTER
    , pcKey     AS CHARACTER
    , pcValue   AS CHARACTER
    ) :
    /* Set a value in the registry. */

    {&timerStart}
    
    DEFINE BUFFER bfConfig FOR ttConfig.
    
    USE SUBSTITUTE('DataDigger-&1', getUserName() ) NO-ERROR.
    IF NOT ERROR-STATUS:ERROR THEN
    DO:
      PUT-KEY-VALUE
        SECTION pcSection
        KEY     pcKey
        VALUE   pcValue
        NO-ERROR
        .
      USE "".
    END.

    /* Update the local cache of the registry as well */
    FIND bfConfig
      WHERE bfConfig.cSection = pcSection
        AND bfConfig.cSetting = pcKey NO-ERROR.

    IF NOT AVAILABLE bfConfig THEN
    DO:
      CREATE bfConfig.
      ASSIGN 
        bfConfig.cSection = pcSection
        bfConfig.cSetting = pcKey.
    END.
    bfConfig.cValue = pcValue.

    {&timerStop}
    return "". /* Function return value. */

END FUNCTION. /* setRegistry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

