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

procedure GetKeyboardState external "user32.dll":
    define input  parameter KBState as long. /* memptr */ 
    define return parameter RetVal  as long. /* bool   */ 
end procedure.

/* Windows API entry point */
procedure ShowScrollBar external "user32.dll":
    define input  parameter hwnd        as long.
    define input  parameter fnBar       as long.
    define input  parameter fShow       as long.
    define return parameter ReturnValue as long.
end procedure.

procedure SendMessageA external "user32.dll":
    define input  parameter hwnd   as long no-undo.
    define input  parameter wmsg   as long no-undo.
    define input  parameter wparam as long no-undo.
    define input  parameter lparam as long no-undo.
    define return parameter rc     as long no-undo.
end procedure.

procedure RedrawWindow external "user32.dll":
    def input parameter v-hwnd  as long no-undo.
    def input parameter v-rect  as long no-undo.
    def input parameter v-rgn   as long no-undo.
    def input parameter v-flags as long no-undo.
    def return parameter v-ret  as long no-undo.
end procedure.

procedure SetWindowTextA external "user32.dll":
  define input parameter hwnd as long.
  define input parameter txt as character.
end procedure.

procedure GetWindow external "user32.dll" :
  define input parameter hwnd as long.
  define input parameter uCmd as long.
  define return parameter hwndOther as long.
end procedure.

procedure GetParent external "user32.dll" :
  define input parameter hwndChild as long.
  define return parameter hwndParent as long.
end procedure.

procedure GetCursorPos external "user32.dll" : 
  define input-output parameter lRect as memptr. 
end. 

procedure GetSysColor external "user32.dll":
  define input parameter nDspElement as long.
  define return parameter COLORREF as long.
end.

procedure ScreenToClient external "user32.dll" :
  define input  parameter hWnd     as long.
  define input  parameter lpPoint  as memptr.
end procedure.

/* Transparency */
PROCEDURE SetWindowLongA EXTERNAL "user32.dll":
 def INPUT PARAM HWND AS LONG.
 def INPUT PARAM nIndex AS LONG.
 def INPUT PARAM dwNewLong AS LONG.
 DEF RETURN PARAM stat AS LONG.
END.

PROCEDURE SetLayeredWindowAttributes EXTERNAL "user32.dll":
 def INPUT PARAM HWND AS LONG.
 def INPUT PARAM crKey AS LONG.
 def INPUT PARAM bAlpha AS SHORT.
 def INPUT PARAM dwFlagsas AS LONG.
 DEF RETURN PARAM stat AS SHORT.
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

define temp-table ttWidget no-undo rcode-information
  field hWidget   as handle
  field iPosX     as integer
  field iPosY     as integer
  field iWidth    as integer
  field iHeight   as integer
  field lVisible  as logical 
  index iPrim as primary hWidget.

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
  ( pcDatabase as character 
  , pcSection  as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-formatQueryString) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD formatQueryString Procedure 
FUNCTION formatQueryString RETURNS CHARACTER
  ( input pcQueryString as character
  , input plExpanded    as logical )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColor) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getColor Procedure 
FUNCTION getColor returns integer
  ( pcName as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColumnLabel) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getColumnLabel Procedure 
FUNCTION getColumnLabel returns character
  ( input phFieldBuffer as handle ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColumnWidthList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getColumnWidthList Procedure 
FUNCTION getColumnWidthList returns character
  ( input phBrowse as handle ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getDatabaseList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getDatabaseList Procedure 
FUNCTION getDatabaseList returns character FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getEscapedData) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getEscapedData Procedure 
FUNCTION getEscapedData RETURNS CHARACTER
  ( pcTarget as character
  , pcString as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getFont) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getFont Procedure 
FUNCTION getFont returns integer
  ( pcFontName as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getImagePath) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getImagePath Procedure 
FUNCTION getImagePath returns character
  ( pcImage as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getIndexFields) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getIndexFields Procedure 
FUNCTION getIndexFields returns character
  ( input pcDatabaseName as character
  , input pcTableName    as character  
  , input pcFlags        as character
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
FUNCTION getLinkInfo returns character
  ( input pcFieldName as character
  ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMatchesValue) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getMatchesValue Procedure 
FUNCTION getMatchesValue RETURNS CHARACTER
  ( hFillIn as handle )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMaxLength) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getMaxLength Procedure 
FUNCTION getMaxLength RETURNS integer
  ( cFieldList as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getOsErrorDesc) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getOsErrorDesc Procedure 
FUNCTION getOsErrorDesc returns character
  (input piOsError as integer) FORWARD.

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
FUNCTION getQuery returns character
  ( input pcDatabase as character
  , input pcTable    as character
  , input piQuery    as integer
  )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getReadableQuery) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getReadableQuery Procedure 
FUNCTION getReadableQuery returns character
  ( input pcQuery as character ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getRegistry Procedure 
FUNCTION getRegistry returns character
    ( pcSection as character
    , pcKey     as character 
    )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getStackSize) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getStackSize Procedure 
FUNCTION getStackSize returns integer() FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTableList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getTableList Procedure 
FUNCTION getTableList returns character
  ( input  pcDatabaseFilter   as character
  , input  pcTableFilter      as character
  , input  plShowHiddenTables as logical  
  , input  pcSortField        as character
  , input  plAscending        as logical  
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
  ( phFrame as handle )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isBrowseChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isBrowseChanged Procedure 
FUNCTION isBrowseChanged RETURNS LOGICAL
  ( input phBrowse as handle )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isDefaultFontsChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isDefaultFontsChanged Procedure 
FUNCTION isDefaultFontsChanged returns logical
  ( /* parameter-definitions */ )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isFileLocked) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isFileLocked Procedure 
FUNCTION isFileLocked RETURNS LOGICAL
  ( pcFileName as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isMouseOver) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isMouseOver Procedure 
FUNCTION isMouseOver returns logical
  ( phWidget as handle )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isWidgetChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isWidgetChanged Procedure 
FUNCTION isWidgetChanged RETURNS LOGICAL
  ( input phWidget as handle )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-removeConnection) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD removeConnection Procedure 
FUNCTION removeConnection RETURNS LOGICAL
  ( pcDatabase as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resolveOsVars) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD resolveOsVars Procedure 
FUNCTION resolveOsVars RETURNS CHARACTER
  ( pcString as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resolveSequence) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD resolveSequence Procedure 
FUNCTION resolveSequence returns character
  ( pcString as character )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setColumnWidthList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setColumnWidthList Procedure 
FUNCTION setColumnWidthList returns logical
  ( input phBrowse    as handle 
  , input pcWidthList as character) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setFilterFieldColor) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setFilterFieldColor Procedure 
FUNCTION setFilterFieldColor returns logical
  ( phWidget as handle )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setLinkInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setLinkInfo Procedure 
FUNCTION setLinkInfo returns logical
  ( input pcFieldName as character
  , input pcValue     as character
  ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD setRegistry Procedure 
FUNCTION setRegistry returns character
    ( pcSection as character
    , pcKey     as character
    , pcValue   as character
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
         HEIGHT             = 37.52
         WIDTH              = 56.6.
/* END WINDOW DEFINITION */
                                                                        */
&ANALYZE-RESUME

 


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Procedure 


/* ***************************  Main Block  *************************** */

/* terminate it.                                                        */
ON CLOSE OF THIS-PROCEDURE 
do:
  define variable cEnvironment as character   no-undo.
  cEnvironment = substitute('DataDigger-&1', getUserName() ).

  unload 'DataDiggerHelp' no-error.
  unload 'DataDigger'     no-error.
  unload cEnvironment     no-error.
end. /* CLOSE OF THIS-PROCEDURE  */

/* Subscribe to setUsage event to track user behaviour */
subscribe to "setUsage" anywhere.


/* Caching settings must be set from within UI.
 * Since the library might be started from DataDigger.p
 * we cannot rely on the registry being loaded yet
 */
glCacheTableDefs = true.
glCacheFieldDefs = true.
glCacheSettings  = true.

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
  define input  parameter pihWidget as handle no-undo.
  
  if valid-handle(pihWidget) then
  do:
    publish "debugMessage" (3, substitute("Apply CHOOSE to &1 &2", pihWidget:type, pihWidget:name)).
    apply 'choose' to pihWidget.
  end.

end procedure. /* applyChoose */

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
  define input  parameter pihWidget as handle no-undo.
  define input  parameter pcEvent   as character   no-undo.
  
  if valid-handle(pihWidget) then
  do:
    publish "debugMessage" (3, substitute("Apply &1 to &2 &3", CAPS(pcEvent), pihWidget:type, pihWidget:name)).
    apply pcEvent to pihWidget.
  end.

end procedure. /* applyEvent */

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
  define input  parameter pcFileName as character   no-undo.
  define output parameter pcError    as character   no-undo.

  define variable cDumpDir     as character   no-undo.
  define variable cDirToCreate as character   no-undo.
  define variable iDir         as integer     no-undo.

  publish "debugMessage" (3, substitute("Check &1", pcFileName)).

  /* Already exist. Overwrite? */
  file-info:file-name = pcFileName.

  if file-info:full-pathname <> ? then 
  do:
    publish "debugMessage" (3, substitute("Already exists as &1 (&2)", file-info:full-pathname, file-info:file-type)).
    
    if file-info:file-type matches '*F*' then
    do:
      run showHelp('OverwriteDumpFile', pcFileName).
      if getRegistry('DataDigger:help', 'OverwriteDumpFile:answer') <> '1' then 
      do:
        /* Do not remember the answer "No" for this question, otherwise it will be
         * confusing the next time the user encounters this situation
         */
        setRegistry('DataDigger:help', 'OverwriteDumpFile:answer',?).
        pcError = 'Aborted by user.'.
        return. 
      end.
  
      /* Write access to this file? */
      if not file-info:file-type matches '*W*' then 
      do:
        pcError = substitute('Cannot overwrite output file "&1"', pcFileName).
        return. 
      end.
    end.

    /* If a dir already exists with the same name as the output file, we cannot create it */
    if file-info:file-type matches '*D*' then
    do:
      pcError = substitute('A directory named "&1" exists; cannot create a file with the same name.', pcFileName).
      return. 
    end. 
  end.

  /* Check dir */
  cDumpDir = substring(pcFileName, 1, r-index(pcFileName,"\")).
  publish "debugMessage" (3, substitute("Dir = &1", cDumpDir)).
  
  file-info:file-name = cDumpDir.
  if cDumpDir <> "" /* Don't complain about not using a dir */
    and file-info:full-pathname = ? then
  do:
    run showHelp('CreateDumpDir', cDumpDir).
    if getRegistry('DataDigger:help', 'CreateDumpDir:answer') <> '1' then 
    do:
      pcError = 'Aborted by user.'.
      return. 
    end.
  end.

  /* Try to create path + file. Progress will not raise an error if it already exists */
  cDirToCreate = entry(1,cDumpDir,'\').
  do iDir = 2 to num-entries(cDumpDir,'\').

    /* In which dir do we want to create a subdir? */
    if iDir = 2 then
      file-info:file-name = cDirToCreate + '\'.
    else 
      file-info:file-name = cDirToCreate.

    /* Does it even exist? */
    if file-info:full-pathname = ? then
    do:
      pcError = substitute('Directory "&1" does not exist.', cDirToCreate).
      publish "debugMessage" (3, substitute("Error: &1", pcError)).
      return.
    end.

    /* Check if the dir is writable */
    if file-info:file-type matches '*X*'  /* Happens on CD-ROM drives */
      or (        file-info:file-type matches '*D*'
          and not file-info:file-type matches '*W*' ) then 
    do:
      pcError = substitute('No write-access to directory: "&1"', cDirToCreate).
      publish "debugMessage" (3, substitute("Error: &1", pcError)).
      return.
    end.

    /* Seems to exist and to be writable. */
    cDirToCreate = cDirToCreate + '\' + entry(iDir,cDumpDir,'\'). 

    /* If a file already exists with the same name, we cannot create a dir */
    file-info:file-name = cDirToCreate.
    if file-info:file-type matches '*F*' then 
    do:
      pcError = substitute('A file named "&1" exists; cannot create a dir with the same name.', cDirToCreate).
      publish "debugMessage" (3, substitute("Error: &1", pcError)).
      return. 
    end.

    /* Create the dir. Creating an existing dir gives no error */
    os-create-dir value(cDirToCreate). 
    if os-error <> 0 then
    do:
      pcError = getOsErrorDesc(os-error).
      publish "debugMessage" (3, substitute("Error: &1", pcError)).
      return.
    end. /* error */

  end. /* iDir */
  
end procedure. /* checkDir */

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
 
 define input  parameter pcDatabase     as character   no-undo.
 define input  parameter pcTable        as character   no-undo.

 define variable iMaxQueryHistory as integer no-undo. 
 define variable iQueryNr         as integer no-undo. 
 define variable iLoop            as integer no-undo. 
 define variable cSetting         as character no-undo. 

 define buffer bQuery for ttQuery.
 {&timerStart}
 
 /* Delete all known queries in memory of this table */
 for each bQuery 
   where bQuery.cDatabase = pcDatabase
     and bQuery.cTable    = pcTable:
   delete bQuery.
 end. 

 iMaxQueryHistory = integer(getRegistry("DataDigger", "MaxQueryHistory" )).
 if iMaxQueryHistory = 0 then return. /* no query history wanted */

 /* If it is not defined use default setting */
 if iMaxQueryHistory = ? then iMaxQueryHistory = 10. 

 collectQueries:
 do iLoop = 1 to iMaxQueryHistory:
   cSetting = getRegistry( substitute("DB:&1", pcDatabase)
                         , substitute('&1:query:&2', pcTable, iLoop )).

   if cSetting = '<Empty>' then next.

   if cSetting <> ? then
   do:
     create bQuery.
     assign iQueryNr         = iQueryNr + 1
            bQuery.cDatabase = pcDatabase 
            bQuery.cTable    = pcTable
            bQuery.iQueryNr  = iQueryNr
            bQuery.cQueryTxt = cSetting.
   end.
   else 
     leave collectQueries.

 end. /* 1 .. MaxQueryHistory */
 {&timerStop}

end procedure. /* collectQueryInfo */

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
  define input  parameter pcAction   as character   no-undo.
  define input  parameter phSource   as handle      no-undo.
  define output parameter plContinue as logical     no-undo.

  define variable hExportTT          as handle      no-undo.
  define variable hExportTtBuffer    as handle      no-undo.
  define variable hBuffer            as handle      no-undo.
  define variable cFileName          as character   no-undo.
  define variable cError             as character   no-undo.
  define variable cMessage           as character   no-undo.
  define variable iRow               as integer     no-undo.
  define variable cDumpProg          as character   no-undo.
  define variable lContinue          as logical     no-undo.
  define variable lDefaultDump       as logical     no-undo.

  if not valid-handle(phSource) then return.

  /* Protect against wrong input */
  if lookup(pcAction,'Dump,Create,Update,Delete') = 0 then
  do:
    message 'Unknown action' pcAction view-as alert-box info buttons ok.
    return. 
  end.

  /* Determine appropriate buffer and populate an intermediate tt
   * with the data to export
   */
  case phSource:type:
    when 'buffer' then 
    do:
      hBuffer = phSource.

      /* Create temptable-handle... */
      create temp-table hExportTt.
      hExportTt:create-like(substitute("&1.&2", hBuffer:dbname, hBuffer:table)).

      /* Prepare the TempTable... */
      hExportTt:temp-table-prepare(substitute("&1", hBuffer:table)).
      hExportTtBuffer = hExportTt:default-buffer-handle.
      hExportTtBuffer:buffer-create().
      hExportTtBuffer:buffer-copy(hBuffer).
    end.

    when 'browse' then 
    do:
      hBuffer = phSource:query:get-buffer-handle(1).

      /* Create temptable-handle... */
      create temp-table hExportTt.
      hExportTt:create-like(substitute("&1.&2", hBuffer:dbname, hBuffer:table)).

      /* Prepare the TempTable... */
      hExportTt:temp-table-prepare(substitute("&1", hBuffer:table)).
      hExportTtBuffer = hExportTt:default-buffer-handle.

      /* Copy the records */
      do iRow = 1 to phSource:num-selected-rows:
        phSource:fetch-selected-row(iRow).
        hExportTtBuffer:buffer-create().
        hExportTtBuffer:buffer-copy(hBuffer).
      end.
    end.

    otherwise return. 
  end case.

  /* Do we need to dump at all? 
   * If the setting=NO or if no setting at all, then don't do any checks 
   */
  if pcAction <> 'Dump' 
    and (   getRegistry('DataDigger:Backup','BackupOn' + pcAction) = ?
         or logical(getRegistry('DataDigger:Backup','BackupOn' + pcAction)) = no
        ) then 
  do:
    assign plContinue = yes.
    return. 
  end.  

  /* Determine the default name to save to */
  run getDumpFileName
    ( input pcAction        /* Dump | Create | Update | Delete */
    , input hBuffer:dbname    
    , input hBuffer:table     
    , input "XML"
    , input ""
    , output cFileName
    ).

  run checkDir(input cFileName, output cError).
  if cError <> "" then 
  do:
    message cError view-as alert-box info buttons ok.
    return. 
  end. 

  /* See if the user has specified his own dump program
   */
  plContinue = ?. /* To see if it ran or not */
  publish "customDump"
      ( input pcAction
      , input hBuffer:dbname 
      , input hBuffer:table
      , input hExportTt
      , input cFileName
      , output cMessage
      , output lDefaultDump
      , output plContinue
      ).
      
  if plContinue <> ? then
  do:
    if cMessage <> "" then 
      message cMessage view-as alert-box info buttons ok.

    if not lDefaultDump or not plContinue then
      return. 
  end.

  plContinue = hExportTT:write-xml
    ( 'file'        /* TargetType     */
    , cFileName     /* File           */
    , yes           /* Formatted      */
    , ?             /* Encoding       */
    , ?             /* SchemaLocation */
    , no            /* WriteSchema    */
    , no            /* MinSchema      */
    ).

  delete object hExportTt.

end procedure. /* dumpRecord */

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

  define input parameter pihBrowse as handle      no-undo.
  define input parameter picFile   as character   no-undo.

  define variable cDataType  as character   no-undo.
  define variable cTimeStamp as character   no-undo.
  define variable hBuffer    as handle      no-undo extent 5.
  define variable hColumn    as handle      no-undo.
  define variable hField     as handle      no-undo.
  define variable hQuery     as handle      no-undo.
  define variable iBack      as integer     no-undo.
  define variable iBuffer    as integer     no-undo.
  define variable iColumn    as integer     no-undo.
  define variable iExtent    as integer     no-undo.
  define variable iRecords   as integer     no-undo.
  define variable iTrailer   as integer     no-undo.
  define variable lFirst     as logical     no-undo.

  hQuery = pihBrowse:query.

  /* Accept max 5 buffers for a query */
  do iBuffer = 1 to min(5, hQuery:num-buffers):
    hBuffer[iBuffer] = hQuery:get-buffer-handle(iBuffer).
  end.

  assign
    iRecords   = 0
    cTimeStamp = string(year( today),"9999":u) + "/":u
               + string(month(today),"99":u  ) + "/":u
               + string(day(  today),"99":u  ) + "-":u
               + string(time,"HH:MM:SS":u).

  hQuery:get-first.

  /* Open outputfile */
  output to value(picFile) no-echo no-map.
  export ?.
  iBack = seek(output) - 1.
  seek output to 0.    

  repeat while not hQuery:query-off-end
  on stop undo, leave:

    assign 
      iRecords = iRecords + 1
      lFirst   = true
      .

    process events.
    
    browseColumn:
    do iColumn = 1 to pihBrowse:num-columns:

      /* Grab the handle */
      hColumn = pihBrowse:get-browse-column(iColumn).

      /* Skip invisible columns */
      if not hColumn:visible then next browseColumn.

      /* Find the buffer the column belongs to */
      SearchLoop:
      do iBuffer = 1 to 5:
        assign hField = hBuffer[iBuffer]:buffer-field(hColumn:name) no-error.
        if error-status:error = false 
          and hField <> ? then 
          leave SearchLoop.
      end.

      /* If no column found, something weird happened */
      if hField = ? then next browseColumn.

      if hField:data-type = "recid":u then next.
  
      if lFirst then
        lFirst = false.
      else
      do:
        seek output to seek(output) - iBack.
        put control ' ':u.
      end.
  
      if hField:extent > 1 then
      do iExtent = 1 to hField:extent:
        if iExtent > 1 then
        do:
          seek output to seek(output) - iBack.
          put control ' ':u.
        end.
  
        export hField:buffer-value(iExtent).
      end.
      else
        export hField:buffer-value.
    end. 

    hQuery:get-next().
  end.
  
  put unformatted ".":u skip.
  iTrailer = seek(output).
  
  put unformatted
         "PSC":u 
    skip "filename=":u hBuffer[1]:table 
    skip "records=":u  string(iRecords,"9999999999999":u) 
    skip "ldbname=":u  hBuffer[1]:dbname 
    skip "timestamp=":u cTimeStamp 
    skip "numformat=":u asc(session:numeric-separator) ",":u asc(session:numeric-decimal-point) 
    skip "dateformat=":u session:date-format "-":u session:year-offset 
    skip "map=NO-MAP":u 
    skip "cpstream=":u session:cpstream 
    skip ".":u 
    skip string(iTrailer,"9999999999":u) 
    skip.
  
  output close.

end procedure. /* dynamicDump */

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
    if hColumn:sort-ascending <> ? then do:
      assign
        pcColumn    = hColumn:name
        plAscending = hColumn:sort-ascending.
      leave.
    end.
  end.

  if pcColumn = '' then
    assign
      pcColumn    = phBrowse:get-browse-column(1):name
      plAscending = true.
      
  publish "debugMessage" (3, substitute("Sorting &1 on &2", string(plAscending,"up/down"), pcColumn)).
  
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
  
  define input  parameter pcAction    as character   no-undo.
  define input  parameter pcDatabase  as character   no-undo.
  define input  parameter pcTable     as character   no-undo.
  define input  parameter pcExtension as character   no-undo.
  define input  parameter pcTemplate  as character   no-undo.
  define output parameter pcFileName  as character   no-undo.

  define variable cLastDir      as character   no-undo.
  define variable cDayOfWeek    as character   no-undo extent 7 initial ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].
  define variable cDumpName     as character   no-undo.
  define variable cDumpDir      as character   no-undo.
  define variable cDumpFile     as character   no-undo.
  define variable cBackupDir    as character   no-undo.
  define variable cBackupFile   as character   no-undo.
  define variable hBuffer       as handle      no-undo.

  /* Checks */
  if lookup(pcAction, "Dump,Create,Update,Delete") = 0 then 
  do:
    message 'Unknown action' pcAction view-as alert-box.
    return.
  end.

  /* If not provided, find the template from the settings,
   * depending on the action we want to perform. 
   */
  if pcTemplate = ? or pcTemplate = "" then
  do:
    if pcAction = 'Dump' then 
      pcFileName = "<DUMPDIR>" + getRegistry("DumpAndLoad", "DumpFileTemplate").
    else 
      pcFileName = "<BACKUPDIR>" + getRegistry("DataDigger:Backup", "BackupFileTemplate").
  end.
  else 
    pcFileName = pcTemplate.

  if pcFileName = ? then pcFileName = "".

  publish "debugMessage" (3, substitute("Dump to: &1", pcFileName)).

  /* Dump dir / backup dir / last-used dir from settings */
  cDumpDir = right-trim(getRegistry("DumpAndLoad", "DumpDir"),'/\') + '\'.
  if cDumpDir = ? then cDumpDir = "". 

  cBackupDir  = right-trim(getRegistry("DataDigger:Backup", "BackupDir"),'/\') + '\'.
  if cBackupDir = ? then cBackupDir = "". 

  cLastDir = right-trim(getRegistry("DumpAndLoad", "DumpLastFileName"),'/\').
  cLastDir = substring(cLastDir,1,r-index(cLastDir,"\")).
  if cLastDir = ? then cLastDir = "".
  cLastDir = right-trim(cLastDir,'\').

  /* Find _file for the dump-name */
  create buffer hBuffer for table substitute('&1._file', pcDatabase) no-error.
  if valid-handle(hBuffer) then
  do:
    hBuffer:find-unique(substitute('where _file-name = &1 and _File._File-Number < 32768', quoter(pcTable)),NO-LOCK).
    if hBuffer:available then 
      cDumpName = hBuffer::_dump-name.
    else 
      cDumpName = pcTable.
  end.
  else 
    cDumpName = pcTable.

  publish "debugMessage" (3, substitute("DumpDir  : &1", cDumpDir)).
  publish "debugMessage" (3, substitute("BackupDir: &1", cBackupDir)).
  publish "debugMessage" (3, substitute("LastDir  : &1", cLastDir)).
  publish "debugMessage" (3, substitute("DumpName : &1", cDumpName)).

  /* Now resolve all tags */
  pcFileName = replace(pcFileName,"<DUMPDIR>"  , cDumpDir                    ).
  pcFileName = replace(pcFileName,"<BACKUPDIR>", cBackupDir                  ).
  pcFileName = replace(pcFileName,"<LASTDIR>"  , cLastDir                    ).
  pcFileName = replace(pcFileName,"<PROGDIR>"  , getProgramDir()             ).

  pcFileName = replace(pcFileName,"<ACTION>"   , pcAction                    ).
  pcFileName = replace(pcFileName,"<USERID>"   , userid(ldbname(1))          ).
  pcFileName = replace(pcFileName,"<DB>"       , pcDatabase                  ).
  pcFileName = replace(pcFileName,"<TABLE>"    , pcTable                     ).
  pcFileName = replace(pcFileName,"<DUMPNAME>" , cDumpName                   ).
  pcFileName = replace(pcFileName,"<EXT>"      , pcExtension                 ).

  pcFileName = replace(pcFileName,"<TIMESTAMP>", "<YEAR><MONTH><DAY>.<HH><MM><SS>" ).
  pcFileName = replace(pcFileName,"<DATE>"     , "<YEAR>-<MONTH>-<DAY>"      ).
  pcFileName = replace(pcFileName,"<TIME>"     , "<HH>:<MM>:<SS>"            ).
  pcFileName = replace(pcFileName,"<WEEKDAY>"  , string(weekday(today))      ).
  pcFileName = replace(pcFileName,"<DAYNAME>"  , cDayOfWeek[weekday(today)]  ).

  pcFileName = replace(pcFileName,"<YEAR>"     , string(year (today),"9999") ).
  pcFileName = replace(pcFileName,"<MONTH>"    , string(month(today),  "99") ).
  pcFileName = replace(pcFileName,"<DAY>"      , string(day  (today),  "99") ).
  pcFileName = replace(pcFileName,"<HH>"       , entry(1,string(time,"HH:MM:SS"),":" ) ).
  pcFileName = replace(pcFileName,"<MM>"       , entry(2,string(time,"HH:MM:SS"),":" ) ).
  pcFileName = replace(pcFileName,"<SS>"       , entry(3,string(time,"HH:MM:SS"),":" ) ).

  /* Get rid of annoying slashes */
  pcFileName = trim(pcFileName,'/\').
  
  /* Sequences */
  pcFileName = resolveSequence(pcFileName).

  /* OS-vars */
  pcFileName = resolveOsVars(pcFileName).

  /* Make lower */
  pcFileName = lc(pcFileName).
  publish "debugMessage" (3, substitute("Dump to: &1", pcFileName)).

end procedure. /* getDumpFileName */

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

  {&timerStart}
  
  /* For dataservers, use the schema name */
  pcDatabase = sdbname(pcDatabase).

  /* Clean up first */
  EMPTY TEMP-TABLE bField.
  EMPTY TEMP-TABLE bColumn.

  /* Return if no db connected */
  IF NUM-DBS = 0 THEN RETURN. 

  /* caching */
  IF glCacheFieldDefs THEN
  DO:
    /* Find the table. Should exist. */
    FIND bTable WHERE bTable.cDatabase = pcDatabase AND bTable.cTableName = pcTableName.
    
    /* Verify whether the CRC is still the same. If not, kill the cache */
    CREATE BUFFER hBufferFile FOR TABLE pcDatabase + "._File".
    hBufferFile:FIND-UNIQUE(SUBSTITUTE('where _file-name = &1 and _File._File-Number < 32768', QUOTER(pcTableName)),NO-LOCK).
    IF hBufferFile::_crc <> bTable.cCrc THEN
    DO:
      PUBLISH "debugMessage" (3, SUBSTITUTE("File CRC changed, kill cache and build new")).
      FOR EACH bFieldCache WHERE bFieldCache.cTableCacheId = bTable.cCacheId:
        DELETE bFieldCache.
      END.
      FOR EACH bColumnCache WHERE bColumnCache.cTableCacheId = bTable.cCacheId:
        DELETE bColumnCache.
      END.
      
      /* Get a fresh list of tables */
      RUN getTables(OUTPUT TABLE bTable).
    END.

    /* First look in the memory-cache */
    IF CAN-FIND(FIRST bFieldCache WHERE bFieldCache.cTableCacheId = bTable.cCacheId) THEN
    DO:
      PUBLISH "debugMessage" (3, SUBSTITUTE("Get from memory-cache")).
      FOR EACH bFieldCache WHERE bFieldCache.cTableCacheId = bTable.cCacheId:
        CREATE bField.
        BUFFER-COPY bFieldCache to bField.
      END.
      FOR EACH bColumnCache WHERE bColumnCache.cTableCacheId = bTable.cCacheId:
        CREATE bColumn.
        BUFFER-COPY bColumnCache to bColumn.
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
    PUBLISH "debugMessage" (2, SUBSTITUTE("Not found in any cache, build tables...")).
  END.

  /*
   * If we get here, the table either cannot be found in the cache
   * or caching is disabled. Either way, fill the tt with fields
   */
  FIND bTable WHERE bTable.cDatabase = pcDatabase AND bTable.cTableName = pcTableName.

  CREATE BUFFER hBufferFile  FOR TABLE pcDatabase + "._File".                    
  CREATE BUFFER hBufferField FOR TABLE pcDatabase + "._Field".

  CREATE QUERY hQuery.
  hQuery:SET-BUFFERS(hBufferFile,hBufferField).

  cQuery = SUBSTITUTE("FOR EACH &1._File  WHERE &1._file._file-name = '&2' AND _File._File-Number < 32768 NO-LOCK, " +
                      "    EACH &1._Field OF &1._File NO-LOCK BY _ORDER" 
                     , pcDatabase
                     , pcTableName
                     ).

  hQuery:QUERY-PREPARE(cQuery).
  hQuery:QUERY-OPEN().
  hQuery:GET-FIRST().

  /* Get list of fields in primary index. */
  cPrimIndexFields = getIndexFields(pcDatabase, pcTableName, "P").

  /* Get list of fields in all unique indexes. */
  cUniqueIndexFields = getIndexFields(pcDatabase, pcTableName, "U").

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
      bField.lPrimary      = can-do(cPrimIndexFields, bField.cFieldName)
      bField.iExtent       = hBufferField:BUFFER-FIELD('_Extent'):BUFFER-VALUE
      bField.lMandatory    = hBufferField:BUFFER-FIELD('_mandatory'):BUFFER-VALUE
      bField.lUniqueIdx    = can-do(cUniqueIndexFields,bField.cFieldName)
      
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
                          
    bField.lShow         = lShowRecidField
    bField.iOrder        = iFieldOrder
    bField.iOrderOrg     = iFieldOrder
    bField.cFullName     = 'RECID'
    bField.cDataType     = 'character'
    bField.cInitial      = ''
    bField.cFormat       = 'X(14)'
    bField.cFormatOrg    = 'X(14)'
    bField.cLabel        = 'RECID'
    bField.lPrimary      = no
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
  create bField.
  assign 
    iFieldOrder           = iFieldOrder + 1
    bField.cTableCacheId = bTable.cCacheId
    bField.cDatabase     = pcDatabase
    bField.cTablename    = pcTableName
    bField.cFieldName    = "ROWID"
                          
    bField.lShow         = lShowRowidField
    bField.iOrder        = iFieldOrder
    bField.iOrderOrg     = iFieldOrder
    bField.cFieldName    = 'ROWID'
    bField.cFullName     = 'ROWID'
    bField.cDataType     = 'character'
    bField.cInitial      = ''
    bField.cFormat       = 'X(30)'
    bField.cFormatOrg    = 'X(30)'
    bField.cLabel        = 'ROWID'
    bField.lPrimary      = no
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
    publish "debugMessage" (3, substitute("Add to second-level cache.")).
    DATASET dsFields:WRITE-XML( "file", cCacheFile, yes, ?, ?, no, no). 
    
    /* Add to memory cache */
    publish "debugMessage" (3, substitute("Add to first-level cache.")).
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

end procedure. /* getFields */

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
  
  define input  parameter phFrame as handle      no-undo.
  define output parameter piMouseX as integer     no-undo.
  define output parameter piMouseY as integer     no-undo.

  define variable lp as memptr  no-undo. 
  
  set-size( lp ) = 16. 
  
  run GetCursorPos( input-output lp). 
  
  /* Get the location of the mouse relative to the frame */
  run ScreenToClient ( input phFrame:hwnd, input lp ).
  
  piMouseX = get-long( lp, 1 ). 
  piMouseY = get-long( lp, 5 ). 
  
  set-size( lp ) = 0. 
  
  publish "debugMessage" (2, substitute("Mouse X/Y = &1 / &2", piMouseX, piMouseY)).

end procedure. /* getMouseXY */

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

 define output parameter table for ttQuery.
    
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
      hFileQuery:QUERY-PREPARE("FOR EACH _Db NO-LOCK " +
                               ", EACH _File NO-LOCK" +
                               "  WHERE _File._Db-recid    = RECID(_Db)" +
                               "    AND _File._File-Number < 32768").

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
          ttTable.cCrc        = hFileBuffer::_crc
          ttTable.cCacheId    = SUBSTITUTE('&1.&2.&3', ttTable.cDatabase, hFileBuffer::_file-name, hFileBuffer::_crc)
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
        FOR EACH ttTable WHERE ttTable.cDatabase = LDBNAME(iDatabase):
          CREATE ttTableXml.
          BUFFER-COPY ttTable TO ttTableXml.
        END.
        TEMP-TABLE ttTableXml:WRITE-XML("file", cCacheFile, YES, ?, ?, NO, NO).
        EMPTY TEMP-TABLE ttTableXml.
      END.
    END. /* tt empty */
  END. /* 1 to num-dbs */

  DELETE WIDGET-POOL "metaInfo".

  /* By default, show all tables */
  FOR EACH ttTable:
    ttTable.lShowInList = TRUE.
  END.

  /* Get table properties from the INI file */
  RUN getTableStats(INPUT-OUTPUT TABLE ttTable).
  
  {&timerStop}

END PROCEDURE. /* getTables */

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
  
  define input-output parameter table for ttTable. 

  define variable cIniFile    as character   no-undo.
  define variable cLine       as character   no-undo.
  define variable cSection    as character   no-undo.
  define variable cDatabase   as character   no-undo.

  /* Read the ini file as plain text and parse the lines. 
   * 
   * The normal way would be to do a FOR-EACH on the _file table and 
   * retrieve the information needed. But if you have a large database 
   * (or a lot of databases), this becomes VERY slow. Searching the 
   * other way around by parsing the INI is a lot faster.
   */
  {&timerStart}

  cIniFile = substitute('&1DataDigger-&2.ini', getProgramDir(), getUserName() ).
  input from value(cIniFile).
  repeat:
    /* Sometimes lines get screwed up and are waaaay too long
     * for the import statement. So just ignore those. 
     */
    IMPORT UNFORMATTED cLine NO-ERROR.
    IF ERROR-STATUS:ERROR THEN NEXT.

    /* Find DB sections */
    if cLine matches '[DB:*]' then 
    do:
      cSection = trim(cLine,'[]').
      cDatabase = entry(2,cSection,":").
    end. 

    /* Only process lines of database-sections */
    if not cSection begins "DB:" then next.

    /* Only process setting lines */
    if not cLine matches '*:*=*' then next.
    
    /* Filter out some settings */
    if cLine matches "*:QueriesServed=*" then
    do:
      find first ttTable 
        where ttTable.cDatabase = cDatabase
          and ttTable.cTableName = entry(1,cLine,':') no-error.
    
      if available ttTable then
      do:
        ttTable.iNumQueries = integer(entry(2,cLine,'=')) no-error.
        if ttTable.iNumQueries = ? then ttTable.iNumQueries = 0.
      end.
    end. /* queriesServed */

    else
    if cLine matches "*:LastUsed=*" then
    do:
      find first ttTable 
        where ttTable.cDatabase = cDatabase
          and ttTable.cTableName = entry(1,cLine,':') no-error.

      if available ttTable then
      do:
        ttTable.tLastUsed = datetime(entry(2,cLine,'=')) no-error.
      end.
    end. /* lastUsed */

    else
    if cLine matches "*:Favourite=*" then
    do:
      find first ttTable
        where ttTable.cDatabase = cDatabase
          and ttTable.cTableName = entry(1,cLine,':') no-error.

      if available ttTable then
      do:
        ttTable.lFavourite = true no-error.
      end.
    end. /* favourite */

  end. /* repeat */
  input close. 

  {&timerStop}

end procedure. /* getTableStats */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTablesWithField) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getTablesWithField Procedure 
PROCEDURE getTablesWithField :
/*------------------------------------------------------------------------
  Name         : getTablesWithField
  Description  : Fill the ttTable temptable with all tables of all 
                 currently connected databases.
  ----------------------------------------------------------------------
  29-10-2009 pti Created
  06-01-2010 pti Optimized for large / a lot of databases.
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER pcFieldList AS CHARACTER NO-UNDO.
  DEFINE OUTPUT PARAMETER TABLE FOR ttTable. 

  DEFINE VARIABLE cSearchFld   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSearchNeg   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSearchPos   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTableList   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cThisField   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iField       AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iSearch      AS INTEGER     NO-UNDO.
  DEFINE VARIABLE lFieldFound  AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lRejected    AS LOGICAL     NO-UNDO.

  {&timerStart}
  
  cSearchPos = pcFieldList.
  cSearchNeg = "".
  
  /* Strip entries that start with a ! */
  IF INDEX(cSearchPos,"!") > 0 THEN
  DO:
    DO iSearch = 1 TO NUM-ENTRIES(cSearchPos):
      IF ENTRY(iSearch,cSearchPos) BEGINS "!" THEN
      DO:
        /* Add this word to the negative-list */
        cSearchNeg = TRIM(cSearchNeg + "," + SUBSTRING(ENTRY(iSearch,cSearchPos),2),",").
    
        /* And wipe it from the positive-list */
        ENTRY(iSearch,cSearchPos) = "".
      END.
    END.
    
    /* Remove empty elements */
    cSearchPos = REPLACE(cSearchPos,",,",",").
    cSearchPos = TRIM(cSearchPos,",").
  END.
  
  /* Sanity check */
  IF cSearchNeg = "*" THEN cSearchNeg = "".


  tableLoop:
  FOR EACH ttTable:

    ASSIGN ttTable.lShowInList = FALSE.
  
    /* Required field */
    DO iSearch = 1 TO NUM-ENTRIES(cSearchPos):
      cSearchFld = ENTRY(iSearch,cSearchPos).
  
      /* If no wildcards used, we can simply CAN-DO */
      IF INDEX(cSearchFld,"*") = 0 THEN
      DO:
        IF NOT CAN-DO(ttTable.cFields, cSearchFld) THEN NEXT tableLoop.
      END.
  
      ELSE
      DO:
        lFieldFound = FALSE.
  
        /* Examine field by field */
        DO iField = 1 TO NUM-ENTRIES(ttTable.cFields):
          cThisField = ENTRY(iField,ttTable.cFields).
    
          /* We require this one. Jump out the loop when found */
          IF CAN-DO(cSearchFld,cThisField) THEN lFieldFound = TRUE.
          IF lFieldFound THEN LEAVE.
        END.
  
        IF NOT lFieldFound THEN NEXT tableLoop.
      END.
    END.
  
    /* Rejected field */
    DO iSearch = 1 TO NUM-ENTRIES(cSearchNeg):
      cSearchFld = ENTRY(iSearch,cSearchNeg).
    
      /* If no wildcards used, we can simply CAN-DO */
      IF INDEX(cSearchFld,"*") = 0 THEN
      DO:
        IF CAN-DO(ttTable.cFields, cSearchFld) THEN NEXT tableLoop.
      END.
  
      ELSE 
      DO:
        lRejected = FALSE.
  
        /* Examine field by field */
        DO iField = 1 TO NUM-ENTRIES(ttTable.cFields):
          cThisField = ENTRY(iField,ttTable.cFields).
    
          /* If the field is present, this table should be rejected */
          IF CAN-DO(cSearchFld,cThisField) THEN lRejected = TRUE.
          IF lRejected THEN LEAVE.
        END.
  
        /* We require this one. Skip if not present */
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

  PUBLISH "debugMessage" (1, SUBSTITUTE("Window &1, lock: &2", phWindow:TITLE, STRING(plLock,"ON/OFF"))).

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

&IF DEFINED(EXCLUDE-openUrl) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE openUrl Procedure 
PROCEDURE openUrl :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
  define input  parameter pcUrl as character   no-undo.
  define variable cLocation as character   no-undo.

  /* We want to open a HTML page that is on the local system. 
   * That is not a problem. But.... The page we want to open 
   * contains a named link (with a '#' in the url) to point
   * to a specific tiddler in tiddlywiki. When you open a URL
   * like this on the local filesystem, the named links are
   * not supported, so we have to create a workaround using
   * a redirect from within an HTML page using javascript....
   */
  cLocation = 'file://' + getProgramDir() + pcUrl.
  cLocation = replace(cLocation,'\','/').

  output to value(session:temp-dir + 'datadigger.html').
  put unformatted 
         '<html>'
    skip '<head>'
    skip '<script>'
    skip 'document.location="' + cLocation + '";'
    skip '</script>'
    skip '</head>'
    skip '</html>'.
  output close. 

  os-command no-wait start value(session:temp-dir + 'datadigger.html').
  pause 1 no-message. /* otherwise error 'file not found' */
  os-delete value(session:temp-dir + 'datadigger.html').

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-readConfigFile) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE readConfigFile Procedure 
PROCEDURE readConfigFile :
/*------------------------------------------------------------------------
  Name         : readConfigFile
  Description  : Read the ini-file and create tt records for it
----------------------------------------------------------------------
  01-10-2012 pti Created
  ----------------------------------------------------------------------*/
  DEFINE INPUT PARAMETER pcConfigFile AS CHARACTER NO-UNDO.

  DEFINE VARIABLE cFile      AS LONGCHAR    NO-UNDO.
  DEFINE VARIABLE cLine      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cChunk     AS LONGCHAR    NO-UNDO.
  DEFINE VARIABLE cSection   AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cTrimChars AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE iLine      AS INTEGER     NO-UNDO.

  {&timerStart}

  /* Read file in 1 pass to memory */
  COPY-LOB FILE pcConfigFile TO cFile.

  cTrimChars = " " + CHR(1) + "~r". /* space / chr-1 / LF */

  /* Process line by line */
  DO iLine = 1 TO NUM-ENTRIES(cFile,"~n"):

    cChunk = ENTRY(iLine,cFile,"~n").
    cChunk = SUBSTRING(cChunk, 1,20000). /* trim very long lines */
    cLine = TRIM(cChunk, cTrimChars).    /* remove junk */

    /* Remember section */
    IF cLine MATCHES "[*]" THEN
      cSection = TRIM(cLine,"[]").

    FIND ttConfig
      WHERE ttConfig.cSection = cSection
        AND ttConfig.cSetting = ENTRY(1,cLine,"=") NO-ERROR.

    IF NOT AVAILABLE ttConfig THEN
    DO:
      CREATE ttConfig.
      ASSIGN
        ttConfig.cSection = cSection
        ttConfig.cSetting = ENTRY(1,cLine,"=")
        .
    END.

    ASSIGN ttConfig.cValue = SUBSTRING(cLine, INDEX(cLine,"=") + 1).
  END.

  {&timerStop}
END PROCEDURE. /* readConfigFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resizeFilterFields) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE resizeFilterFields Procedure 
PROCEDURE resizeFilterFields :
/*------------------------------------------------------------------------
  Name         : resizeFilterFields
  Description  : Generic procedure to redraw the filter fields of the 
                 fields browse and of the index browse. 

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  define input  parameter pcFilterFields as character   no-undo.
  define input  parameter pcButtons      as character   no-undo.
  define input  parameter phBrowse       as handle      no-undo.

  define variable iField        as integer no-undo. 
  define variable iButton       as integer no-undo. 
  define variable iCurrentPos   as integer no-undo. 
  define variable iRightEdge    as integer no-undo. 
  define variable iWidth        as integer no-undo. 
  define variable hColumn       as handle  no-undo. 
  define variable hButton       as handle  no-undo. 
  define variable hFilterField  as handle  no-undo. 
  define variable iFilter       as integer no-undo. 
  define variable lChangeDetected as logical no-undo. 
  
  /* Find out if there has been a change in the browse or in one of
   * its columns. If no changes, save a little time by not redrawing
   */
  if not isBrowseChanged(phBrowse) then return.
  
  {&timerStart}

  PUBLISH "timerCommand" ("start", "resizeFilterFields:makeSmall").
  /* To prevent drawing error, make all fields small */
  do iField = 1 to num-entries(pcFilterFields):
    hFilterField = handle(entry(iField,pcFilterFields)).
    hFilterField:visible      = no.
    hFilterField:x            = phBrowse:x. 
    hFilterField:y            = phBrowse:y - 23.
    hFilterField:width-pixels = 1.
  end.
  PUBLISH "timerCommand" ("stop", "resizeFilterFields:makeSmall").

  /* Start by setting the buttons at the proper place. Do this right to left */
  assign iRightEdge = phBrowse:x + phBrowse:width-pixels.
  do iButton = num-entries(pcButtons) to 1 by -1:
    hButton = handle(entry(iButton,pcButtons)).
    hButton:x = iRightEdge - hButton:width-pixels.
    hButton:y = phBrowse:y - 23. /* filter buttons close to the browse */
    iRightEdge = hButton:x + 0. /* A little margin between buttons */
  end.

  /* The left side of the left button is the maximum point 
   * Fortunately, this value is already in iRightEdge.
   * Resize and reposition the fields from left to right, 
   * use the space between browse:x and iRightEdge
   */

  /* Take the left side of the first visible column as a starting point. */
  firstVisibleColumn:
  do iField = 1 to phBrowse:num-columns:
    hColumn = phBrowse:get-browse-column(iField):handle.

    if hColumn:x > 0 and hColumn:visible then
    do:
      iCurrentPos = phBrowse:x + hColumn:x.
      leave firstVisibleColumn.
    end.
  end.

  PUBLISH "timerCommand" ("start", "resizeFilterFields:fieldLoop").
  fieldLoop:
  do iField = 1 to phBrowse:num-columns:

    hColumn = phBrowse:get-browse-column(iField):handle.
    
    /* Some types cannot have a filter */
    if hColumn:data-type = 'raw' then next. 

    iFilter = iFilter + 1.
    if iFilter > num-entries(pcFilterFields) then leave fieldLoop.

    /* Determine the handle of the filterfield */
    hFilterField = handle(entry(iFilter, pcFilterFields)).

    /* If the column is hidden, make the filter hidden and go to the next */
    if not hColumn:visible then 
    do:
      hFilterField:visible = no.
      next fieldLoop. 
    end.

    /* Where *are* we ?? */
    iCurrentPos = phBrowse:x + hColumn:x.

    /* If the columns have been resized, some columns might have fallen off the screen */
    if hColumn:x < 1 then next. 

    /* Does it fit on the screen? */
    if iCurrentPos >= iRightEdge - 5 then leave fieldLoop. /* accept some margin */

    /* Where will this field end? And does it fit? */
    iWidth = hColumn:width-pixels + 4.
    if iCurrentPos + iWidth > iRightEdge then iWidth = iRightEdge - iCurrentPos.

    /* Ok, seems to fit */
    hFilterField:x            = iCurrentPos.
    hFilterField:width-pixels = iWidth.
    iCurrentPos               = iCurrentPos + iWidth.
    hFilterField:visible      = phBrowse:visible. /* take over the visibility of the browse */
  end.
  PUBLISH "timerCommand" ("stop", "resizeFilterFields:fieldLoop").
  
  {&timerStop}

end procedure. /* resizeFilterFields */

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

  define input  parameter pcDatabase     as character   no-undo.
  define input  parameter pcTable        as character   no-undo.
  define input  parameter pcQuery        as character   no-undo.

  define variable cQuery as character no-undo. 
  define variable iQuery as integer   no-undo. 

  define buffer bQuery for ttQuery.
  
  {&timerStart}

  /* Prepare query for saving in ini-file */
  cQuery = pcQuery.
  cQuery = replace(cQuery,'~n',chr(1)).
  cQuery = replace(cQuery,{&QUERYSEP},chr(1)).
  if cQuery = '' then cQuery = '<empty>'.

  /* Get the table with queries again, because they might be 
   * changed if the user has more than one window open.
   */
  run collectQueryInfo(pcDatabase, pcTable).

  /* Save current query in the tt. If it already is in the 
   * TT then just move it to the top
   */
  find bQuery 
    where bQuery.cDatabase = pcDatabase
      and bQuery.cTable    = pcTable 
      and bQuery.cQueryTxt = cQuery no-error.

  if available bQuery then 
  do:
    assign bQuery.iQueryNr = 0.
  end.
  else 
  do:
    create bQuery.
    assign bQuery.cDatabase = pcDatabase 
           bQuery.cTable    = pcTable   
           bQuery.iQueryNr  = 0
           bQuery.cQueryTxt = cQuery.
  end.

  /* The ttQuery temp-table is already filled, renumber it */
  iQuery = 0.
  repeat preselect each bQuery 
    where bQuery.cDatabase = pcDatabase
      and bQuery.cTable    = pcTable 
       by bQuery.iQueryNr:

    find next bQuery.
    assign 
      iQuery          = iQuery + 1
      bQuery.iQueryNr = iQuery.
  end.

  /* And save it to the INI-file */
  run saveQueryTable(table bQuery, pcDatabase, pcTable).

  {&timerStop}
end procedure. /* saveQuery */

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

  define input  parameter table for ttQuery.
  define input  parameter pcDatabase     as character   no-undo.
  define input  parameter pcTable        as character   no-undo.

  define variable iMaxQueryHistory as integer no-undo. 
  define variable iQuery           as integer no-undo. 
  define variable cSetting         as character no-undo. 

  define buffer bQuery for ttQuery.
 
  {&timerStart}
 
  iMaxQueryHistory = integer(getRegistry("DataDigger", "MaxQueryHistory" )).
  if iMaxQueryHistory = 0 then return. /* no query history wanted */
 
  /* If it is not defined use default setting */
  if iMaxQueryHistory = ? then iMaxQueryHistory = 10. 

  iQuery = 1.

  saveQuery:
  for each bQuery 
    where bQuery.cDatabase = pcDatabase
      and bQuery.cTable    = pcTable 
       by bQuery.iQueryNr:
    
    cSetting = bQuery.cQueryTxt.
    if cSetting = '' then next. /* cSetting = '<empty>' */

    setRegistry( substitute("DB:&1", pcDatabase)
               , substitute('&1:query:&2', pcTable, iQuery)
               , cSetting).
    iQuery = iQuery + 1.
    if iQuery > iMaxQueryHistory then leave saveQuery.
  end.

  /* Delete higher nrs than MaxQueryHistory */
  do while iQuery <= iMaxQueryHistory:
 
    setRegistry( substitute("DB:&1", pcDatabase)
               , substitute('&1:query:&2', pcTable, iQuery)
               , ?).
    iQuery = iQuery + 1.
  end. /* iQuery .. MaxQueryHistory */
  
  {&timerStop}
end procedure. /* saveQueryTable */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setCaching) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setCaching Procedure 
PROCEDURE setCaching :
/*------------------------------------------------------------------------
  Name : setCaching
  Desc : Set the cache vars for the library
  ----------------------------------------------------------------------
  13-11-2013 pti Created
  ----------------------------------------------------------------------*/

  /* Set global vars */
  glCacheTableDefs = LOGICAL( getRegistry("DataDigger:Cache","FieldDefs") ).
  glCacheFieldDefs = LOGICAL( getRegistry("DataDigger:Cache","TableDefs") ).
  glCacheSettings  = LOGICAL( getRegistry("DataDigger:Cache","Settings")  ).

END PROCEDURE. /* setCaching */

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

  define input parameter phBrowse    as handle    no-undo. 
  define input parameter pcSortField as character no-undo. 
  define input parameter plAscending as logical   no-undo. 

  define variable iColumn    as integer   no-undo.
  define variable hColumn    as handle    no-undo.
  define variable lSortFound as logical   no-undo.
  
  {&timerStart}

  do iColumn = 1 to phBrowse:num-columns:
    hColumn = phBrowse:get-browse-column(iColumn).

    /* If you apply the sort to the same column, the order 
     * of sorting is inverted.
     */
    if hColumn:name = pcSortField then 
    do:
      phBrowse:set-sort-arrow(iColumn, plAscending ).
      lSortFound = true.

      /* Setting is one of: ColumnSortFields | ColumnSortIndexes | ColumnSortTables */
      setRegistry( 'DataDigger'
                 , substitute('ColumnSort&1', substring(phBrowse:name,3))  
                 , substitute('&1,&2',iColumn, plAscending)
                 ).
    end.
    else 
      phBrowse:set-sort-arrow(iColumn, ? ). /* erase existing arrow */
  end.
  
  /* If no sort is found, delete setting */
  if not lSortFound then
    setRegistry( 'DataDigger', substitute('ColumnSort&1', substring(phBrowse:name,3)), ?).

  {&timerStop}

end procedure. /* setSortArrow */

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
  define input  parameter phFrame as handle     no-undo.
  define input  parameter piLevel as integer    no-undo.
  
  &scop GWL_EXSTYLE         -20
  &scop WS_EX_LAYERED       524288
  &scop LWA_ALPHA           2
  &scop WS_EX_TRANSPARENT   32
  
  DEFINE VARIABLE stat AS INTEGER    NO-UNDO.

  /* Set WS_EX_LAYERED on this window  */
  RUN SetWindowLongA(phFrame:HWND, {&GWL_EXSTYLE}, {&WS_EX_LAYERED}, output stat).

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

  iNumDays = integer(getRegistry("DataDigger:Usage", SUBSTITUTE("&1:numDays", pcUsageId))).
  IF iNumDays = ? THEN iNumDays = 0.

  /* Update lastDate and numDays only first time per day */
  IF getRegistry("DataDigger:Usage", SUBSTITUTE("&1:lastDate" , pcUsageId)) <> ISO-DATE(TODAY) THEN
  DO:
    /* Num days on which the function is used */
    iNumDays = iNumDays + 1.
    setRegistry("DataDigger:Usage", SUBSTITUTE("&1:numDays" , pcUsageId), string(iNumDays)).
  
    /* Date last used */
    setRegistry("DataDigger:Usage", SUBSTITUTE("&1:lastDate" , pcUsageId), ISO-DATE(TODAY)).
  END.

  /* Number of times used */
  iNumUsed = integer(getRegistry("DataDigger:Usage", SUBSTITUTE("&1:numUsed", pcUsageId))).
  IF iNumUsed = ? THEN iNumUsed = 0.
  iNumUsed = iNumUsed + 1.
  setRegistry("DataDigger:Usage", SUBSTITUTE("&1:numUsed", pcUsageId), STRING(iNumUsed)).

END PROCEDURE. /* setUsage */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-showHelp) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showHelp Procedure 
PROCEDURE showHelp :
define input  parameter pcTopic   as character   no-undo.
  define input  parameter pcStrings as character   no-undo.

  define variable cButtons       as character   no-undo.
  define variable cMessage       as character   no-undo.
  define variable cMsg           as character   no-undo.
  define variable cPrg           as character   no-undo.
  define variable cTitle         as character   no-undo.
  define variable cType          as character   no-undo.
  define variable cUrl           as character   no-undo.
  define variable cHlp           as character   no-undo.
  define variable cCanHide       as character   no-undo.
  define variable iButtonPressed as integer     no-undo.
  define variable lAnswer        as logical     no-undo.
  define variable lDontShowAgain as logical     no-undo.
  define variable lCanHide       as logical     no-undo.
  define variable lHidden        as logical     no-undo.
  define variable iString        as integer     no-undo.
  define variable cUserString    as character   no-undo extent 9.
  define variable cHelpfile    as character   no-undo.

  /* If no message, then just return */
  cMessage = getRegistry('DataDigger:help', pcTopic + ':message').
  cHelpfile = getProgramDir() + 'DataDigger.chm'.

  /* What to start? */
  cUrl = getRegistry('DataDigger:help', pcTopic + ':url').
  cHlp = getRegistry('DataDigger:help', pcTopic + ':hlp').
  cPrg = getRegistry('DataDigger:help', pcTopic + ':program').
  cCanHide = getRegistry('DataDigger:help', pcTopic + ':canHide').
  cCanHide = TRIM(cCanHide).
  lCanHide = logical(cCanHide) no-error.
  if lCanHide = ? then lCanHide = true.

  if cMessage = ? then 
  do:
    if cUrl = ? and cPrg = ? and cHlp = ? then return. 
    lHidden        = yes. /* suppress empty text window */
    iButtonPressed = 1.   /* forces to start the url or prog */
  end.

  /* If type is unknown, set to QUESTION if there is a question mark in the message */
  cType    = getRegistry('DataDigger:help', pcTopic + ':type').
  if cType = ? then cType = (if cMessage matches '*?*' then 'Question' else 'Message').

  /* If no button labels defined, set them based on message type */
  cButtons = getRegistry('DataDigger:help', pcTopic + ':buttons').
  if cButtons = ? then cButtons = (if cType = 'Question' then '&Yes,&No,&Cancel' else '&Ok').

  /* If title is empty, set it to the type of the message */
  cTitle   = getRegistry('DataDigger:help', pcTopic + ':title').
  if cTitle = ? then cTitle = cType.
  
  /* If hidden has strange value, set it to NO */
  lHidden = logical(getRegistry('DataDigger:help', pcTopic + ':hidden')) no-error.
  if lHidden = ? then lHidden = no.
  
  /* If ButtonPressed has strange value, set hidden to NO */
  iButtonPressed = integer( getRegistry('DataDigger:help',pcTopic + ':answer') ) no-error.
  if iButtonPressed = ? then lHidden = no.
  
  /* if we have no message, but we do have an URL or prog, then
   * dont show an empty message box.
   */
  if cMessage = ? then
    assign 
      lHidden        = yes /* suppress empty text window */
      iButtonPressed = 1.   /* forces to start the url or prog */

  /* Fill in strings in message */
  do iString = 1 to num-entries(pcStrings):
    cUserString[iString] = entry(iString,pcStrings).
  end.

  cMessage = substitute( cMessage
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
  if not lHidden then 
  do:
    /* If the window is locked, unlock it for the message window */


    run value( getProgramDir() + 'dQuestion.w')
      ( input cTitle
      , input cMessage
      , input cButtons
      , input lCanHide
      , output iButtonPressed
      , output lDontShowAgain
      ).
      
    if lDontShowAgain then 
      setRegistry('DataDigger:help', pcTopic + ':hidden', 'yes').
  end. 
  
  /* Start external things if needed */                                            
  if iButtonPressed = 1 then
  do:
    if cHlp <> ? then system-help cHelpfile context integer(cHlp).
    if cUrl <> ? then run openUrl( cUrl ).
    if cPrg <> ? then run value( cPrg ) no-error.
  end.
  
  /* Save answer */
  setRegistry('DataDigger:help',pcTopic + ':answer', string(iButtonPressed)).
      
end procedure. /* showHelp */

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

&IF DEFINED(EXCLUDE-startWinHelp) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE startWinHelp Procedure 
PROCEDURE startWinHelp :
/*------------------------------------------------------------------------
  Name         : startWinHelp
  Description  : Invoke help
  ----------------------------------------------------------------------
  16-03-2011 pti Created
  ----------------------------------------------------------------------*/

  define input  parameter phFocus as handle no-undo.

  define variable cHelpfile    as character   no-undo.
  define variable iHelpContext as integer     no-undo. 
  define variable cStartedFrom as character   no-undo.

  cHelpfile = getProgramDir() + 'DataDigger.chm'.
  
  IF SEARCH(cHelpfile) = ? THEN 
    RUN fetchChm.p (INPUT getProgramDir()) NO-ERROR.
  IF ERROR-STATUS:ERROR OR SEARCH(cHelpfile) = ? THEN
    RETURN "". 

  if can-query(phFocus,'context-help-id') then
    iHelpContext = phFocus:context-help-id.

  /* If no help available, find the help-id of the current window */
  if iHelpContext = 0 then
  do:
    cStartedFrom = entry(num-entries(program-name(3),'\'), program-name(3),'\').
    cStartedFrom = entry(1,cStartedFrom,'.').

    case cStartedFrom:
      when 'dAbout'       then iHelpContext = 260.
      when 'dDump'        then iHelpContext = 160.
      when 'dFilter'      then iHelpContext = 260.
      when 'dQueries'     then iHelpContext = 140.
      when 'dQuestion'    then iHelpContext = 260.
      when 'dSettings'    then iHelpContext = 120.
      when 'wConnections' then iHelpContext = 130.
      when 'wEdit'        then iHelpContext = 170.
      when 'wDataDigger'  then iHelpContext = 260.
      when 'wLoadData'    then iHelpContext = 150.
    end case.
  end.

  /* If still nothing found, show help about main window */
  if iHelpContext = 0 then 
    iHelpContext = 260. /* page about main window */ 

  system-help cHelpfile context iHelpContext.

end procedure. /* startWinHelp */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-unblockFile) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE unblockFile Procedure 
PROCEDURE unblockFile :
/*------------------------------------------------------------------------
  Name         : unblockFile
  Description  : Unblock a file. 
  
  Starting with Windows XP-SP2, Windows preserves zone information in 
  downloaded files to NTFS partitions, such that it blocks certain files 
  in certain applications until you "unblock" the files.
  
  So for example if you download a zip file of source code to try 
  something out, every file will display this in the security settings of 
  the file properties:
  
        "This file came from another computer and might 
         be blocked to help protect this computer"
  
  You have to manually unblock the file by pressing the "Unblock" 
  button in the properties window. And that is a real PITA.
  
  Fortunately, there is a workaround by using the TYPE command. The zone 
  information of the original file is kept in an alternate data stream 
  in the file. The TYPE command just copies the default stream and just
  ignores all others. So if you TYPE a file and pipe it to another file, 
  the zone information is lost:
  
        TYPE originalfile.ext > copiedfile.ext
  
  ----------------------------------------------------------------------
  16-03-2011 pti Created
  ----------------------------------------------------------------------*/

  define input parameter pcFileName as character no-undo. 

  define variable cNewFileName as character no-undo. 

  /* Protect against nasty errors */
  if search(pcFileName) = ? then return.

  /* The filename will remain the same, but we will
   * remove the last extension. Files that need to be unblocked
   * therefore need to have an extra extension. 
   */
  cNewFileName = pcFileName.
  entry(num-entries(cNewFileName,'.'), cNewFileName,'.') = ''.
  cNewFileName = right-trim(cNewFileName,'.').

  /* First, delete the file if it already exists */ 
  os-delete value(cNewFileName).

  /* Do a TYPE of the original file. This will copy the 
   * default stream and skip all other streams. In one
   * of the alternate streams the information is stored
   * that this file came from outside the computer. 
   */
  os-command silent type value(pcFileName) > value(cNewFileName).
  
  /* Then, if we have a new file, delete the original one. */
  if search(cNewFileName) <> ? then
    os-delete value(pcFileName).

end procedure. /* unblockFile */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-uncacheTable) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE uncacheTable Procedure 
PROCEDURE uncacheTable :
/*------------------------------------------------------------------------
  Name         : uncacheTable
  Description  : Remove table from cache
  ----------------------------------------------------------------------*/

  DEFINE INPUT  PARAMETER pcDatabase  AS CHARACTER   NO-UNDO.
  DEFINE INPUT  PARAMETER pcTableName AS CHARACTER   NO-UNDO.

/*   DEFINE VARIABLE cCacheFile AS CHARACTER NO-UNDO.                                         */
/*                                                                                            */
/*   DEFINE BUFFER bTable       FOR ttTable.                                                  */
/*   DEFINE BUFFER bFieldCache  FOR ttFieldCache.                                             */
/*   DEFINE BUFFER bColumnCache FOR ttColumnCache.                                            */
/*                                                                                            */
/*   PUBLISH "debugMessage" (3, SUBSTITUTE("Kill cache for &1.&2", pcDatabase, pcTableName)). */
/*                                                                                            */
/*   /* Find the table. Should exist. */                                                      */
/*   FIND bTable WHERE bTable.cDatabase = pcDatabase AND bTable.cTableName = pcTableName.     */
/*                                                                                            */
/*   FOR EACH bFieldCache WHERE bFieldCache.cTableCacheId = bTable.cCacheId:                  */
/*     DELETE bFieldCache.                                                                    */
/*   END.                                                                                     */
/*                                                                                            */
/*   FOR EACH bColumnCache WHERE bColumnCache.cTableCacheId = bTable.cCacheId:                */
/*     DELETE bColumnCache.                                                                   */
/*   END.                                                                                     */
/*                                                                                            */
/*   cCacheFile = SUBSTITUTE('&1cache\&2.xml', getProgramDir(), bTable.cCacheId).             */
/*   OS-DELETE VALUE(cCacheFile).                                                             */

END PROCEDURE. /* uncacheTable */

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

  PUBLISH "debugMessage" (1, SUBSTITUTE("Window &1, force to unlock", phWindow:TITLE)).

  /* Find window in our tt of locked windows */
  FIND ttWindowLock WHERE ttWindowLock.hWindow = phWindow NO-ERROR.
  IF NOT AVAILABLE ttWindowLock THEN RETURN. 

  IF ttWindowLock.iLockCounter > 0 THEN
  do:
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
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT PARAMETER pcDatabase    AS CHARACTER   NO-UNDO.
  DEFINE INPUT PARAMETER pcTableName   AS CHARACTER   NO-UNDO.
  DEFINE INPUT-OUTPUT PARAMETER TABLE FOR ttField.

  DEFINE VARIABLE cCustomFormat      AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cSelectedFields    AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFieldOrder        AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE lSaveDataFilters   AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE lShow              AS LOGICAL     NO-UNDO.
  DEFINE VARIABLE iFieldOrder        AS INTEGER     NO-UNDO.
  DEFINE VARIABLE iMaxExtent         AS INTEGER     NO-UNDO.

  DEFINE BUFFER bField FOR ttField.
  DEFINE BUFFER bColumn FOR ttColumn.

  {&timerStart}

  PUBLISH "debugMessage" (3, SUBSTITUTE("Update field definitions for &1.&2", pcDatabase, pcTableName)).

  /* Get list of all previously selected fields */
  cSelectedFields = getRegistry(substitute("DB:&1",pcDatabase), substitute("&1:fields",pcTableName)).
  if cSelectedFields = ? then cSelectedFields = '*'.

  /* Get field ordering */
  cFieldOrder = getRegistry(substitute('DB:&1',pcDatabase), substitute('&1:fieldOrder',pcTableName)).

  /* Saved data filters? */
  lSaveDataFilters = logical(getRegistry ("DataDigger", "SaveDataFilters")).

  for each bField:
    /* Was this field selected? */
    bField.lShow = can-do(cSelectedFields, bField.cFullName).

    /* Restore changed field format. */
    cCustomFormat = getRegistry( substitute("DB:&1",pcDatabase)
                               , substitute("&1.&2:format",pcTableName,bField.cFieldName) ).
    if cCustomFormat <> ? then bField.cFormat = cCustomFormat.

    /* Restore changed field order. */
    bField.iOrder = lookup(bField.cFullName,cFieldOrder).
    if bField.iOrder = ? then bField.iOrder = bField.iOrderOrg.

    /* Retrieve a formerly saved filter value */
    bField.cFilterValue = "".

    if lSaveDataFilters then
    do:
      bField.cFilterValue = getRegistry( substitute("DB:&1", pcDatabase)
                                       , substitute("&1.&2:filter",pcTableName,bField.cFullName)
                                       ).
      if bField.cFilterValue = ? then bField.cFilterValue = "".
    end.

    /* Formerly saved filters take precedence over Custom
     * filters, so only retrieve Custom Filter when there
     * was no previously saved user filter
     */
    if bField.cFilterValue = "" then
    do:
      publish "customFilter"
        ( input pcDataBase
        , input pcTableName
        , input bField.cFullName
        , output bField.cFilterValue
        ).
    end.

    /* RECID / ROWID field visibility might be changed */
    if lookup(bField.cFullName, "RECID,ROWID") > 0 then
      bField.lShow = logical(getRegistry ("DataDigger", "AddDataColumnFor" + bField.cFullName)).

  end. /* f/e bField */

  /* Only show first X of an extent */
  iMaxExtent = INTEGER(getRegistry("DataDigger","MaxExtent")) NO-ERROR.
  IF iMaxExtent = ? THEN iMaxExtent = 100.
  IF iMaxExtent > 0 THEN
  FOR EACH bColumn WHERE bColumn.iExtent > iMaxExtent:
    DELETE bColumn.
  END.

  /* Reorder fields. This is especially needed for extents */
  iFieldOrder = 0.
  FOR EACH bField BY bField.iOrder:
    FOR EACH bColumn WHERE bColumn.cFieldName =  bField.cFieldName BY bColumn.cFieldName:
      iFieldOrder = iFieldOrder + 1.
      bColumn.iColumnNr = iFieldOrder.
    END.
  END.

  {&timerStop}

end procedure. /* updateFields */

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

  PUBLISH "debugMessage" (3, SUBSTITUTE("Update first-level cache for &1.&2", pcDatabase, pcTableName)).

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
  ( pcDatabase as character 
  , pcSection  as character ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
    
define temp-table ttDatabase no-undo rcode-information
  field cLogicalName  as character column-label "Logical Name" format "x(20)"
  field cSection      as character column-label "Section"      format "x(20)"
  index iPrim is primary unique cLogicalName
  .
------------------------------------------------------------------------------*/

  if not can-find(ttDatabase where ttDatabase.cLogicalName = pcDatabase) then
  do:
    create ttDatabase.
    assign 
      ttDatabase.cLogicalName  = pcDatabase
      ttDatabase.cSection      = pcSection
      . 

  end.

  return true.

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-formatQueryString) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION formatQueryString Procedure 
FUNCTION formatQueryString RETURNS CHARACTER
  ( input pcQueryString as character
  , input plExpanded    as logical ) :

/*------------------------------------------------------------------------------
  Purpose: formatQueryString
    Notes: return a properly formatted query string
------------------------------------------------------------------------------*/
  define variable cReturnValue as character   no-undo.

  {&timerStart}
  
  cReturnValue = pcQueryString.

  if cReturnValue <> '' and cReturnValue <> ? then 
  do:
    /* There might be chr(1) chars in the text (if read from ini, for example)
     * Replace these with normal CRLF, then proceed 
     */
    cReturnValue = replace(cReturnValue,chr(1),'~n').

    if plExpanded then
      cReturnValue = replace(cReturnValue, {&QUERYSEP}, '~n').
    else
      cReturnValue = replace(cReturnValue, '~n', {&QUERYSEP}).
  end.

  {&timerStop}
  return cReturnValue.

end function. /* formatQueryString */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColor) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getColor Procedure 
FUNCTION getColor returns integer
  ( pcName as character ) :

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
FUNCTION getColumnLabel returns character
  ( input phFieldBuffer as handle ):

  define variable cColumnLabel as character   no-undo.
  define variable cTemplate    as character   no-undo.
  
  {&timerStart}
  
  cTemplate = getRegistry("DataDigger","ColumnLabelTemplate").
  if cTemplate = ? or cTemplate = "" then cTemplate = "&1".

  cColumnLabel = substitute(cTemplate
                           , phFieldBuffer::cFullName
                           , phFieldBuffer::iOrder
                           , phFieldBuffer::cLabel
                           ).
  {&timerStop}
  return cColumnLabel.

end function. /* getColumnLabel */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getColumnWidthList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getColumnWidthList Procedure 
FUNCTION getColumnWidthList returns character
  ( input phBrowse as handle ):

  /* returns a list of all fields and their width like:
   * custnum:12,custname:20,city:12
   */
  define variable cWidthList as character   no-undo.
  define variable hColumn    as handle      no-undo.
  define variable iColumn    as integer     no-undo.
  
  {&timerStart}
  
  do iColumn = 1 to phBrowse:num-columns:
  
    hColumn = phBrowse:get-browse-column(iColumn).
    cWidthList = substitute('&1,&2:&3'
                           , cWidthList 
                           , hColumn:name
                           , hColumn:width-pixels
                           ).
  end.
  
  {&timerStop}
  return trim(cWidthList,','). 
end function. /* getColumnWidthList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getDatabaseList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getDatabaseList Procedure 
FUNCTION getDatabaseList returns character:

/*------------------------------------------------------------------------
  Name         : getDatabaseList
  Description  : Return a comma separated list of all connected datbases
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define variable cDatabaseList as character   no-undo.

  define variable iCount as integer     no-undo.

  {&timerStart}
  
  /* Special options */
  do iCount = 1 to num-dbs:
    cDatabaseList = cDatabaseList + ',' + ldbname(iCount).
  end.

  {&timerStop}
  return trim(cDatabaseList,',').

end function. /* getDatabaseList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getEscapedData) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getEscapedData Procedure 
FUNCTION getEscapedData RETURNS CHARACTER
  ( pcTarget as character
  , pcString as character ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/
  define variable cOutput as character no-undo.
  define variable iTmp    as integer   no-undo.

  {&timerStart}
  
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

  {&timerStop}
  RETURN pcString.

END FUNCTION. /* getEscapedData */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getFont) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getFont Procedure 
FUNCTION getFont returns integer
  ( pcFontName as character ) :

/*------------------------------------------------------------------------
  Name         : getFont
  Description  : Return the fontnumber for the type given
  ---------------------------------------------------------------------- 
  25-03-2011 pti Created
  ----------------------------------------------------------------------*/

  define variable iFontNr as integer no-undo. 

  {&timerStart}
  iFontNr = integer(getRegistry('DataDigger:Fonts',pcFontName)) no-error.

  if iFontNr = ? or iFontNr > 23 then 
  case pcFontName:
    when 'Default' then iFontNr = 4.
    when 'Fixed'   then iFontNr = 0.
  end case.

  {&timerStop}
  return iFontNr.   /* Function return value. */

end function. /* getFont */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getImagePath) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getImagePath Procedure 
FUNCTION getImagePath returns character
  ( pcImage as character ) :
  
/*------------------------------------------------------------------------
  Name         : getImagePath
  Description  : Return the image path + icon set name 
  ---------------------------------------------------------------------- 
  22-01-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define variable cImagePath as character   no-undo.
  define variable cIconSet   as character   no-undo.
  
  {&timerStart}
  cIconSet = 'default'.

  cImagePath = substitute('&1Image/&2_&3'
                         , getProgramDir()
                         , cIconSet
                         , pcImage
                         ).

  /* Fall back to the default icon set when image not found */
  if search(cImagePath) = ? then
    cImagePath = substitute('&1Image/default_&2'
                           , getProgramDir()
                           , pcImage
                           ).
  {&timerStop}
  return cImagePath.
end function. /* getImagePath */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getIndexFields) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getIndexFields Procedure 
FUNCTION getIndexFields returns character
  ( input pcDatabaseName as character
  , input pcTableName    as character  
  , input pcFlags        as character
  ) :
/*------------------------------------------------------------------------
  Name         : getIndexFields
  Description  : Return the index fields of a table.
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  18-12-2012 pti Merged with similar functions
  ----------------------------------------------------------------------*/
  
  define variable cWhere            as character   no-undo.
  define variable hQuery            as handle      no-undo.
  define variable hFieldBuffer      as handle      no-undo.
  define variable hFileBuffer       as handle      no-undo.
  define variable hIndexBuffer      as handle      no-undo.
  define variable hIndexFieldBuffer as handle      no-undo.
  define variable cFieldList        as character   no-undo.
  
  {&timerStart}
  
  create buffer hFileBuffer       for table pcDatabaseName + "._File".
  create buffer hIndexBuffer      for table pcDatabaseName + "._Index".
  create buffer hIndexFieldBuffer for table pcDatabaseName + "._Index-Field".
  create buffer hFieldBuffer      for table pcDatabaseName + "._Field".
  
  create query hQuery.
  hQuery:set-buffers(hFileBuffer,hIndexBuffer,hIndexFieldBuffer,hFieldBuffer).
 
  cWhere = substitute("FOR EACH &1._file WHERE &1._file._file-name = &2 AND _File._File-Number < 32768, ~
                           EACH &1._index       OF &1._file WHERE TRUE &3 &4,  ~
                           EACH &1._index-field OF &1._index,            ~
                           EACH &1._field       OF &1._index-field"
                     , pcDatabaseName
                     , quoter(pcTableName)
                     , (if can-do(pcFlags,"U") then "AND _index._unique = true" else "")
                     , (if can-do(pcFlags,"P") then "AND recid(_index) = _file._prime-index" else "")
                     ).
  
  if hQuery:query-prepare (cWhere) then 
  do:
    hQuery:query-open().
    hQuery:get-first(no-lock).
    repeat while not hQuery:query-off-end:
      cFieldList = cFieldList + "," + trim(hFieldBuffer:buffer-field("_field-name"):string-value).
      hQuery:get-next(no-lock).
    end.
  end.
  
  cFieldList = trim(cFieldList, ",").
  
  hQuery:query-close. 
  
  delete object hFileBuffer.
  delete object hIndexBuffer.
  delete object hIndexFieldBuffer.
  delete object hFieldBuffer.
  delete object hQuery.
  
  {&timerStop}
  return cFieldList.   /* Function return value. */

end function. /* getIndexFields */

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

  def var L-KBSTATE as memptr no-undo. 
  def var L-RETURNVALUE as integer no-undo. 
  def var L-SHIFTLIST as char no-undo. 
  
  set-size(L-KBSTATE) = 256. 
  
  /* Get the current state of the keyboard */ 
  run GetKeyboardState(get-pointer-value(L-KBSTATE), output L-RETURNVALUE). 
  
  if get-bits(get-byte(L-KBSTATE, 1 + 16), 8, 1) = 1 
  then L-SHIFTLIST = L-SHIFTLIST + ",SHIFT". 
  if get-bits(get-byte(L-KBSTATE, 1 + 17), 8, 1) = 1 
  then L-SHIFTLIST = L-SHIFTLIST + ",CTRL". 
  if get-bits(get-byte(L-KBSTATE, 1 + 18), 8, 1) = 1 
  then L-SHIFTLIST = L-SHIFTLIST + ",ALT". 
  
  SET-SIZE(L-KBSTATE) = 0. 
  
  return L-SHIFTLIST.   /* Function return value. */ 

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getLinkInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getLinkInfo Procedure 
FUNCTION getLinkInfo returns character
  ( input pcFieldName as character
  ):

/*------------------------------------------------------------------------
  Name         : getLinkInfo
  Description  : Save name/value of a field.
  ----------------------------------------------------------------------
  21-10-2009 pti Created
  ----------------------------------------------------------------------*/

  define buffer bLinkInfo for ttLinkInfo.
  {&timerStart}
  find bLinkInfo where bLinkInfo.cField = pcFieldName no-error.
  {&timerStop}
  return (if available bLinkInfo then bLinkInfo.cValue else "").

end function. /* getLinkInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMatchesValue) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getMatchesValue Procedure 
FUNCTION getMatchesValue RETURNS CHARACTER
  ( hFillIn as handle ) :

/*------------------------------------------------------------------------
  Name         : getMatchesValue
  Description  : Transform the value of a fillin to something we can use
                 with the MATCHES function. 

  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable cValue as character no-undo. 

  cValue = hFillIn:screen-value. 
  if cValue = hFillIn:private-data then cValue = ''.

  if cValue = ? or cValue = '' then cValue = '*'.
  else 
  if    index(cValue,'*') = 0 
    and index(cValue,'.') = 0 then 
    cValue = '*' + cValue + '*'.

  return cValue.   /* Function return value. */

END FUNCTION. /* getMatchesValue */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getMaxLength) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getMaxLength Procedure 
FUNCTION getMaxLength RETURNS integer
  ( cFieldList as character ) :

/*------------------------------------------------------------------------
  Name         : getMaxLength
  Description  : Return the length of the longest element in a comma 
                 separated list
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/
  define variable iField     as integer no-undo. 
  define variable iMaxLength as integer no-undo. 

  {&timerStart}
  
  /* Get max field length */
  do iField = 1 to num-entries(cFieldList):
    iMaxLength = maximum(iMaxLength,length(entry(iField,cFieldList))).
  end.

  {&timerStop}
  return iMaxLength.   /* Function return value. */

end function. /* getMaxLength */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getOsErrorDesc) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getOsErrorDesc Procedure 
FUNCTION getOsErrorDesc returns character
  (input piOsError as integer):

/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  case piOsError:
    when   0 then return "No error                 ".
    when   1 then return "Not owner                ".
    when   2 then return "No such file or directory".
    when   3 then return "Interrupted system call  ".
    when   4 then return "I/O error                ".
    when   5 then return "Bad file number          ".
    when   6 then return "No more processes        ".
    when   7 then return "Not enough core memory   ".
    when   8 then return "Permission denied        ".
    when   9 then return "Bad address              ".
    when  10 then return "File exists              ".
    when  11 then return "No such device           ".
    when  12 then return "Not a directory          ".
    when  13 then return "Is a directory           ".
    when  14 then return "File table overflow      ".
    when  15 then return "Too many open files      ".
    when  16 then return "File too large           ".
    when  17 then return "No space left on device  ".
    when  18 then return "Directory not empty      ".
    otherwise return "Unmapped error           ".
  end case.

end function. /* getOsErrorDesc */

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
  cProgDir = SUBSTRING(FILE-INFO:FULL-PATHNAME,1,R-INDEX(FILE-INFO:FULL-PATHNAME,'\')).

  RETURN cProgDir. /* Function return value. */

END FUNCTION. /* getProgramDir */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getQuery) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getQuery Procedure 
FUNCTION getQuery returns character
  ( input pcDatabase as character
  , input pcTable    as character
  , input piQuery    as integer
  ) :

  define buffer bQuery for ttQuery.

  find bQuery 
    where bQuery.cDatabase = pcDatabase
      and bQuery.cTable    = pcTable
      and bQuery.iQueryNr  = piQuery no-error.

  if available bQuery then 
    return bQuery.cQueryTxt.
  else
    return ?.

end function. /* getQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getReadableQuery) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getReadableQuery Procedure 
FUNCTION getReadableQuery returns character
  ( input pcQuery as character ):

  /* Name: getReadableQuery
   * Desc: Return a query as a string that is readable for humans. 
   * 
   *       message getReadableQuery( hQuery:prepare-string ) view-as alert-box.
   *       
   *       message getReadableQuery( string(hQuery) ) view-as alert-box.
   */
  define variable hQuery as handle      no-undo.
  
  /* Accept query or query-handle */
  hQuery = widget-handle(pcQuery) no-error.
  if valid-handle( hQuery ) then
  do:
    hQuery = widget-handle(pcQuery).
    pcQuery = hQuery:prepare-string.
  end.
  
  pcQuery = replace(pcQuery,' EACH ' ,' EACH ').
  pcQuery = replace(pcQuery,' FIRST ',' FIRST ').
  pcQuery = replace(pcQuery,' WHERE ',  '~n  WHERE ').
  pcQuery = replace(pcQuery,' AND '  ,  '~n    AND ').
  pcQuery = replace(pcQuery,' BY '   ,  '~n     BY ').
  pcQuery = replace(pcQuery,' FIELDS ()','').
  pcQuery = replace(pcQuery,'FOR EACH ' ,'FOR EACH ').
  pcQuery = replace(pcQuery,' NO-LOCK',  ' NO-LOCK').
  pcQuery = replace(pcQuery,' INDEXED-REPOSITION',  '').

  .pcQuery = pcQuery + '~n'.

  return pcQuery.
end function. /* getReadableQuery */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getRegistry Procedure 
FUNCTION getRegistry returns character
    ( pcSection as character
    , pcKey     as character 
    ) :

/*------------------------------------------------------------------------
  Name         : getRegistry 
  Description  : Get a value from the registry. 
  ---------------------------------------------------------------------- 
  15-01-2009 pti Created
  ----------------------------------------------------------------------*/

  define variable cValue as character no-undo.
  define variable lValue as logical   no-undo.

  {&timerStart}
  
  /* If this is a DB-specific section then replace db name if needed */
  if pcSection begins "DB:" then
  do:
    find ttDatabase where ttDatabase.cLogicalName = entry(2,pcSection,":") no-error.
    if available ttDatabase then pcSection = "DB:" + ttDatabase.cSection.
  end.


  if glCacheSettings then
  do:
    /* Load settings if there is nothing in the config table */
    if not temp-table ttConfig:has-records then
    do:
      /* Help file is least important */
      run readConfigFile( substitute("&1DataDiggerHelp.ini"
                                    , getProgramDir()
                                    )).
      /* General DD settings */
      run readConfigFile( substitute("&1DataDigger.ini"
                                    , getProgramDir()
                                    )).
      /* Per-user settings */
      run readConfigFile( substitute("&1DataDigger-&2.ini"
                                    , getProgramDir()
                                    , getUserName()
                                    )).
  
      /* When all ini-files have been read, we can determine whether 
       * caching needs to be enabled
       */
      lValue = logical(getRegistry("DataDigger:Cache","Settings")) no-error.
      if lValue <> ? then assign glCacheSettings = lValue.

      lValue = logical(getRegistry("DataDigger:Cache","TableDefs")) no-error.
      if lValue <> ? then assign glCacheTableDefs = lValue.

      /* If we do not want to cache the registry, empty it now */
      if not glCacheSettings then run clearRegistryCache.
    end.

    /* Search in settings tt */
    find ttConfig
      where ttConfig.cSection = pcSection
        and ttConfig.cSetting = pcKey 
            no-error.
            
    {&timerStop}
    return ( if available ttConfig then ttConfig.cValue else ? ).
  end.

  else 
  do:  
    use substitute('DataDigger-&1', getUserName() ).
    get-key-value 
      section pcSection
      key     pcKey
      value   cValue.
  
    /* If setting is not in the personal INI file
     * then check the default DataDigger.ini
     */
    if cValue = ? then
    do:
      use 'DataDigger'.
      get-key-value 
        section pcSection
        key     pcKey
        value   cValue.
    end.
  
    /* And if it is still not found, look in 
     * the DataDiggerHelp ini file 
     */
    if cValue = ? then
    do:
      use 'DataDiggerHelp'.
      get-key-value 
        section pcSection
        key     pcKey
        value   cValue.
    end. 
  
    /* Clean up and return */
    use "".
    {&timerStop}
    return cValue.
  end.

end function. /* getRegistry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getStackSize) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getStackSize Procedure 
FUNCTION getStackSize returns integer():
  define variable cList      as character   no-undo.
  define variable cParm      as character   case-sensitive no-undo.
  define variable cSetting   as character   no-undo.
  define variable cValue     as character   no-undo.
  define variable iOption    as integer     no-undo.
  define variable iParm      as integer     no-undo.
  define variable iStackSize as integer     no-undo.
  
  cList = session:startup-parameters.
  
  do iParm = 1 to num-entries(cList):
    cSetting = entry(iParm,cList) + " ".
    cParm    = entry(1,cSetting," ").
    cValue   = entry(2,cSetting," ").
  
    if cParm = "-s" then 
      iStackSize = integer(cValue).
  end.

  /* If not defined, report the default */
  if iStackSize = 0 then iStackSize = 40.

  return iStackSize.
end function. /* getStackSize */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTableList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getTableList Procedure 
FUNCTION getTableList returns character
  ( input  pcDatabaseFilter   as character
  , input  pcTableFilter      as character
  , input  plShowHiddenTables as logical  
  , input  pcSortField        as character
  , input  plAscending        as logical  
  ) :

/*------------------------------------------------------------------------
  Name         : getTableList
  Description  : Get a list of all tables in the current database that 
                 match a certain filter. 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  23-01-2009 pti added filter
  08-10-2009 pti added input parm plShowHiddenTables
  17-12-2009 pti added input parm pcSortField / plAscending
  ----------------------------------------------------------------------*/
  
  define variable cTableList  as character   no-undo.
  define variable cQuery      as character   no-undo.

  define buffer bTable for ttTable.
  define query qTable for bTable.

  {&timerStart}
  if pcDatabaseFilter = '' or pcDatabaseFilter = ? then pcDatabaseFilter = '*'.
  if pcSortField = '' or pcSortField = ? then pcSortField = 'cTableName'.
  if plAscending = ? then plAscending = yes.

  /* Build query */
  cQuery = substitute('for each bTable where cDatabase matches &1', quoter(pcDatabaseFilter)).
  cQuery = substitute("&1 and cTableName matches &2", cQuery, quoter(pcTableFilter )).
  if plShowHiddenTables = false then 
    cQuery = substitute('&1 and lHidden = no', cQuery).
  query qTable:query-prepare( substitute('&1 by &2 &3', cQuery, pcSortField, string(plAscending,'/descending')) ).

  query qTable:query-open.
  query qTable:get-first.

  /* All fields */
  repeat while not query qTable:query-off-end:
    cTableList = cTableList + "," + bTable.cTableName.
    query qTable:get-next.
  end.
  query qTable:query-close.

  cTableList = left-trim(cTableList, ",").

  {&timerStop}
  return cTableList.   /* Function return value. */
  
end function. /* getTableList */

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

  COPY-LOB mUserId FOR (intSize - 1) TO cUserName.

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
  ( phFrame as handle ) :

  define variable hWidget as handle  no-undo.
  define variable iMouseX as integer no-undo.
  define variable iMouseY as integer no-undo.

  {&timerStart}
  hWidget = phFrame:first-child:first-child. 
  run getMouseXY(input phFrame, output iMouseX, output iMouseY).

  repeat while valid-handle(hWidget):

    if hWidget:type <> "RECTANGLE"
      and iMouseX >= hWidget:x
      and iMouseX <= hWidget:x + hWidget:width-pixels
      and iMouseY >= hWidget:y
      and iMouseY <= hWidget:y + hWidget:height-pixels then return hWidget.

    hWidget = hWidget:next-sibling.
  end. 

  {&timerStop}
  return ?.

end function. /* getWidgetUnderMouse */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isBrowseChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isBrowseChanged Procedure 
FUNCTION isBrowseChanged RETURNS LOGICAL
  ( input phBrowse as handle ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/

  define variable iField  as integer no-undo.
  define variable hColumn as handle  no-undo.

  if not valid-handle(phBrowse) then return false. 
  if phBrowse:type <> "BROWSE" then return false. 

  {&TimerStart}

  /* First check the browse itself */
  if isWidgetChanged(phBrowse) then return true. 

  do iField = 1 to phBrowse:num-columns:
    hColumn = phBrowse:get-browse-column(iField):handle.
    if isWidgetChanged(hColumn) then return true. 
  end. /* browse */

  /* apparently nothing changed, so... */
  publish "debugMessage" (2, substitute("Nothing changed in browse: &1", phBrowse:name)).

  {&TimerStop}
  return false. 

end function. /* isBrowseChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isDefaultFontsChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isDefaultFontsChanged Procedure 
FUNCTION isDefaultFontsChanged returns logical
  ( /* parameter-definitions */ ) :


/*------------------------------------------------------------------------
  Name         : isDefaultFontsChanged
  Description  : Returns whether the default fonts 0-7 were changed.

  ----------------------------------------------------------------------
  26-04-2010 pti Created
  ----------------------------------------------------------------------*/

  define variable cFontSize     as character   no-undo extent 8.
  define variable iFont         as integer     no-undo.
  define variable lFontsChanged as logical     no-undo.
  
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
  lFontsChanged = no.

  checkFont:
  do iFont = 0 to 7:
    if cFontSize[iFont + 1] <> substitute('&1/&2'
                                         , font-table:get-text-width-pixels('DataDigger',iFont) 
                                         , font-table:get-text-height-pixels(iFont)
                                         ) then 
    do:
      lFontsChanged = true.
      leave checkFont.
    end.
  end. /* checkFont */

  return lFontsChanged.

end function. /* isDefaultFontsChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isFileLocked) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isFileLocked Procedure 
FUNCTION isFileLocked RETURNS LOGICAL
  ( pcFileName as character ) :
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
FUNCTION isMouseOver returns logical
  ( phWidget as handle ) :

  define variable iMouseX as integer   no-undo.
  define variable iMouseY as integer   no-undo.

  if not valid-handle(phWidget) then return false. 

  run getMouseXY ( input phWidget:frame
                 , output iMouseX
                 , output iMouseY
                 ).

  return (    iMouseX >= phWidget:x 
          and iMouseX <= phWidget:x + phWidget:width-pixels 
          and iMouseY >= phWidget:y 
          and iMouseY <= phWidget:y + phWidget:height-pixels ).

end function. /* isMouseOver */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isWidgetChanged) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isWidgetChanged Procedure 
FUNCTION isWidgetChanged RETURNS LOGICAL
  ( input phWidget as handle ) :
/*------------------------------------------------------------------------------
  Purpose:  
    Notes:  
------------------------------------------------------------------------------*/
  define variable lChangeDetected as logical no-undo. 
  define variable iField          as integer no-undo.
  define variable hColumn         as handle  no-undo.
  define variable hFirstColumn    as handle  no-undo.

  define buffer ttWidget for ttWidget.

  if not valid-handle(phWidget) then return false. 
  {&timerStart}

  find ttWidget where ttWidget.hWidget = phWidget no-error.
  if not available ttWidget then 
  do:
    publish "debugMessage" (2, substitute("New widget: &1 &2", phWidget:type, phWidget:name)).
    create ttWidget.
    assign ttWidget.hWidget = phWidget.
  end.
  else
    publish "debugMessage" (3, substitute("Widget: &1 &2", phWidget:type, phWidget:name)).

  publish "debugMessage" (3, substitute("  iPosX     &1 - &2", ttWidget.iPosX    , phWidget:x           )).
  publish "debugMessage" (3, substitute("  iPosY     &1 - &2", ttWidget.iPosY    , phWidget:y           )).
  publish "debugMessage" (3, substitute("  iWidth    &1 - &2", ttWidget.iWidth   , phWidget:width-pixels)).
  publish "debugMessage" (3, substitute("  iHeight   &1 - &2", ttWidget.iHeight  , phWidget:height-pixels)).
  publish "debugMessage" (3, substitute("  lVisible  &1 - &2", ttWidget.lVisible , phWidget:visible)).

  if ttWidget.iPosX     <> phWidget:x
/*  or ttWidget.iPosY     <> phWidget:y */
  or ttWidget.iWidth    <> phWidget:width-pixels
/*  or ttWidget.iHeight   <> phWidget:height-pixels 
  or ttWidget.lVisible  <> phWidget:visible */
    then
  do:
    assign
      ttWidget.iPosX     = phWidget:x
      ttWidget.iPosY     = phWidget:y
      ttWidget.iWidth    = phWidget:width-pixels
      ttWidget.iHeight   = phWidget:height-pixels
/*       ttWidget.hFirstCol = hFirstColumn */
      ttWidget.lVisible  = phWIdget:visible
      lChangeDetected    = true.
  end.

  publish "debugMessage" (2, substitute("  Widget changed: &1", lChangeDetected)).

  {&TimerStop}
  return lChangeDetected.

END FUNCTION. /* isWidgetChanged */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-removeConnection) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION removeConnection Procedure 
FUNCTION removeConnection RETURNS LOGICAL
  ( pcDatabase as character ) :
  /*------------------------------------------------------------------------------
    Purpose:  
      Notes:  
      
  define temp-table ttDatabase no-undo rcode-information
    field cLogicalName  as character column-label "Logical Name" format "x(20)"
    field cSection      as character column-label "Section"      format "x(20)"
    index iPrim is primary unique cLogicalName
    .
  ------------------------------------------------------------------------------*/
  find ttDatabase where ttDatabase.cLogicalName = pcDatabase no-error.
  if available ttDatabase then delete ttDatabase.

  return true.

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resolveOsVars) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION resolveOsVars Procedure 
FUNCTION resolveOsVars RETURNS CHARACTER
  ( pcString as character ) :

  define variable iPercStart   as integer no-undo. 
  define variable iPercEnd     as integer no-undo. 
  define variable cEnvVarName  as character no-undo. 
  define variable cEnvVarValue as character no-undo. 
  define variable cReturnValue as character no-undo. 


  /* Support for OS-directives between % eg: %username% will expand
   * to your username, as long as you have an OS-var for that. Type 'set' on the dos prompt for 
   * a list of all environment variables
   */
  cReturnValue = pcString.
  iPercStart = index(cReturnValue,'%').
  
  resolveOsVars:
  do while iPercStart > 0:
    iPercEnd = index(cReturnValue,'%',iPercStart + 1).
    
    if iPercEnd = 0 then leave resolveOsVars. /* single % */
    cEnvVarName = trim( substring(cReturnValue,iPercStart, iPercEnd - iPercStart) ,'%'). /* Grab text between % */
    
    /* Search in the registry */
    load "System" base-key "HKEY_LOCAL_MACHINE".
    use "System".
    get-key-value section "CurrentControlSet~\Control~\Session Manager~\Environment" key cEnvVarName value cEnvVarValue.
    unload "System".
    
    /* If not defined, try our luck in the default env */
    if cEnvVarValue = ? then
      cEnvVarValue = os-getenv(cEnvVarName) . /* try to resolve */
    
    /* If still not found, step to next % */
    if cEnvVarValue = ? then
    do:
      iPercStart = iPercEnd.
      next resolveOsVars.  
    end.
    
    cReturnValue = replace(cReturnValue,'%' + cEnvVarName + '%', cEnvVarValue). /* Replace with value */
    iPercStart = index(cReturnValue,'%'). /* Find next directive */
  end.

  RETURN cReturnValue.

end function. /* resolveOsVars */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-resolveSequence) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION resolveSequence Procedure 
FUNCTION resolveSequence returns character
  ( pcString as character ) :

  define variable iFileNr       as integer    no-undo.
  define variable cSeqMask      as character  no-undo .
  define variable cSeqFormat    as character  no-undo .
  define variable cFileName     as character   no-undo.

  cFileName = pcString.

  /* User can specify a sequence for the file. The length of 
   * the tag sets the format: <###> translates to a 3-digit nr
   * Special case is <#> which translates to no leading zeros
   */ 
  if    index(cFileName,'<#') > 0 
    and index(cFileName,'#>') > 0 then
  do:
    cSeqMask = substring(cFileName,index(cFileName,'<#')). /* <#####>tralalala */
    cSeqMask = substring(cSeqMask,1,index(cSeqMask,'>')). /* <#####> */
    cSeqFormat = trim(cSeqMask,'<>'). /* ##### */
    cSeqFormat = replace(cSeqFormat,'#','9').
    if cSeqFormat = '9' then cSeqFormat = '>>>>>>>>>9'.

    setFileNr:
    repeat:
      iFileNr = iFileNr + 1.
      if search(replace(cFileName,cSeqMask,trim(string(iFileNr,cSeqFormat)))) = ? then 
      do:
        cFileName = replace(cFileName,cSeqMask,trim(string(iFileNr,cSeqFormat))).
        leave setFileNr.
      end.
    end.
  end.

  return cFileName.

end function. /* resolveSequence */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setColumnWidthList) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setColumnWidthList Procedure 
FUNCTION setColumnWidthList returns logical
  ( input phBrowse    as handle 
  , input pcWidthList as character):

  /* set all specified columns in pcWidthList to a specified width like:
   * custnum:12,custname:20,city:12
   */
  define variable cColumnName  as character   no-undo.
  define variable cListItem    as character   no-undo.
  define variable hColumn      as handle      no-undo.
  define variable iColumn      as integer     no-undo.
  define variable iColumnWidth as integer     no-undo.
  define variable iListItem    as integer     no-undo.
  
  do iListItem = 1 to num-entries(pcWidthList):
    cListItem    = entry(iListItem,pcWidthList).
    cColumnName  = entry(1,cListItem,':') no-error.
    iColumnWidth = integer(entry(2,cListItem,':')) no-error.
    
    do iColumn = 1 to phBrowse:num-columns:
      hColumn = phBrowse:get-browse-column(iColumn).
      if hColumn:name = cColumnName then
        hColumn:width-pixels = iColumnWidth.
    end. /* iColumn */
  end. /* iListItem */
  
  return true. 
end function. /* setColumnWidthList */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setFilterFieldColor) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setFilterFieldColor Procedure 
FUNCTION setFilterFieldColor returns logical
  ( phWidget as handle ) :

/*------------------------------------------------------------------------
  Name         : setFilterFieldColor
  Description  : If you enter the field and you have not put in a filter
                 clear out the field so you can type something yourself. 
                 
  ----------------------------------------------------------------------
  16-01-2009 pti Created
  ----------------------------------------------------------------------*/

  if not valid-handle(phWidget) then message "DEBUG ALARM" view-as alert-box.
  
  if phWidget:screen-value = phWidget:private-data then 
    phWidget:fgcolor = 7.
  else 
    phWidget:fgcolor = ?.
    
  /* phWidget:bgcolor = ?. */

  return true.

END FUNCTION. /* setFilterFieldColor */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setLinkInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setLinkInfo Procedure 
FUNCTION setLinkInfo returns logical
  ( input pcFieldName as character
  , input pcValue     as character
  ):

/*------------------------------------------------------------------------
  Name         : setLinkInfo
  Description  : Save name/value of a field.
  ----------------------------------------------------------------------
  21-10-2009 pti Created
  ----------------------------------------------------------------------*/

  define buffer bLinkInfo for ttLinkInfo. 

  {&timerStart}

  publish "debugMessage" (2, substitute("Set linkinfo for field &1 to &2", pcFieldName, pcValue)).

  find bLinkInfo where bLinkInfo.cField = pcFieldName no-error.
  if not available bLinkInfo then
  do:
    create bLinkInfo.
    assign bLinkInfo.cField = pcFieldName.
  end.

  bLinkInfo.cValue = trim(pcValue).
  
  {&timerStop}
  return true.   /* Function return value. */

end function. /* setLinkInfo */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-setRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setRegistry Procedure 
FUNCTION setRegistry returns character
    ( pcSection as character
    , pcKey     as character
    , pcValue   as character
    ) :

  /*------------------------------------------------------------------------
    Name         : setRegistry 
    Description  : Set a value in the registry. 
    ---------------------------------------------------------------------- 
    15-01-2009 pti Created
    02-05-2012 pti Some settings to general ini
    ----------------------------------------------------------------------*/

    {&timerStart}
    
    /* Some settings go to DataDigger.ini and not
     * to the personalized one. 
     */
    if   pcSection = "DataDigger:Update" then
      use 'DataDigger.ini'.
    else 
      use substitute('DataDigger-&1', getUserName() ).

    put-key-value 
      section pcSection
      key     pcKey
      value   pcValue
      no-error
      .
    use "".

    /* Update the local cache of the registry as well */
    find ttConfig 
      where ttConfig.cSection = pcSection
        and ttConfig.cSetting = pcKey no-error.

    if not available ttConfig then
    do:
      create ttConfig.
      assign 
        ttConfig.cSection = pcSection
        ttConfig.cSetting = pcKey.
    end.
    ttConfig.cValue = pcValue.

    {&timerStop}
    return "". /* Function return value. */

end function. /* setRegistry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

