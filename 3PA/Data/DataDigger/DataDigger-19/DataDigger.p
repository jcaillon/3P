&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Procedure 
/*------------------------------------------------------------------------
    File        : 
    Purpose     :

    Syntax      :

    Description :

    Author(s)   :
    Created     :
    Notes       :
  ----------------------------------------------------------------------*/
/*          This .W file was created with the Progress AppBuilder.      */
/*----------------------------------------------------------------------*/

/* ***************************  Constants ***************************** */

/* GetDriveType return values */
&GLOBAL-DEFINE DRIVE_UNKNOWN     0
&GLOBAL-DEFINE DRIVE_NO_ROOT_DIR 1
&GLOBAL-DEFINE DRIVE_REMOVABLE   2
&GLOBAL-DEFINE DRIVE_FIXED       3
&GLOBAL-DEFINE DRIVE_REMOTE      4
&GLOBAL-DEFINE DRIVE_CDROM       5
&GLOBAL-DEFINE DRIVE_RAMDISK     6


/* ***************************  Definitions  ************************** */

DEFINE VARIABLE giNumDiggers  AS INTEGER   NO-UNDO.
DEFINE VARIABLE gcProgramDir  AS CHARACTER NO-UNDO.
DEFINE VARIABLE glUpdated     AS LOGICAL   NO-UNDO.
DEFINE VARIABLE ghCustomSuper AS HANDLE    NO-UNDO.

DEFINE TEMP-TABLE ttOsFile NO-UNDO RCODE-INFORMATION
  FIELD cFileName     AS CHARACTER FORMAT 'x(30)'
  FIELD cFileType     AS CHARACTER FORMAT 'x(8)'
  FIELD iFileSize     AS INTEGER   FORMAT '>>,>>>,>>9 '
  FIELD dtModified    AS DATETIME  FORMAT '99-99-9999 HH:MM:SS '
  FIELD cModified     AS CHARACTER FORMAT 'x(20)'
  FIELD cFullPathname AS CHARACTER FORMAT 'x(60)'
  FIELD cBaseName     AS CHARACTER FORMAT 'x(40)'
  FIELD cStatus       AS CHARACTER FORMAT 'x(20)'
  INDEX iPrim IS PRIMARY cBaseName cFileType
  .

PROCEDURE GetDriveTypeA EXTERNAL "kernel32.dll":
  DEFINE INPUT  PARAMETER lpRootPathName AS CHARACTER.
  DEFINE RETURN PARAMETER iType          AS LONG.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE no



/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&IF DEFINED(EXCLUDE-getDriveType) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getDriveType Procedure 
FUNCTION getDriveType RETURNS CHARACTER ( pcDrive AS CHARACTER ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getProcessorArchitecture) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getProcessorArchitecture Procedure 
FUNCTION getProcessorArchitecture RETURNS INTEGER() FORWARD.

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

&IF DEFINED(EXCLUDE-getTimeStamp) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getTimeStamp Procedure 
FUNCTION getTimeStamp RETURNS CHARACTER
  ( input pDateTime as datetime )  FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isRecompileNeeded) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD isRecompileNeeded Procedure 
FUNCTION isRecompileNeeded returns logical
  ( /* parameter-definitions */ )  FORWARD.

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
         HEIGHT             = 14.91
         WIDTH              = 46.
/* END WINDOW DEFINITION */
                                                                        */
&ANALYZE-RESUME

 


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Procedure 


/* ***************************  Main Block  *************************** */
run initializeObject.

/* Notifications of starts and stops of DataDigger windows */
subscribe to 'DataDigger' anywhere. 

/* Get new version, if needed */
run value(gcProgramDir + 'getNewVersion.p')
  ( input ?
  , input false
  , output glUpdated
  ).

/* When we start, do a check whether we need to recompile */
run recompileDataDigger.

/* Start the main window */
run value(gcProgramDir + 'wDataDigger.w') persistent.

/* Sit back and relax */
if not this-procedure:persistent then
do:
  wait-for close of this-procedure. 
  if session:first-procedure:file-name matches '*DataDiggerLib.p' then quit.
end.

else 
do:
  on close of this-procedure
  do:
    define variable hDiggerLib as handle no-undo.

    delete object this-procedure no-error.
    publish 'DataDiggerClose'.
    
    /* Kill the library */
    PUBLISH 'DataDiggerLib' (OUTPUT hDiggerLib).
    apply 'close' to hDiggerLib.
    delete object hDiggerLib no-error.
    
    /* Kill custom super */
    /* delete object ghCustomSuper no-error. */
    return.
  end.
end.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&IF DEFINED(EXCLUDE-DataDigger) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE DataDigger Procedure 
PROCEDURE DataDigger :
/*------------------------------------------------------------------------
  Name         : DataDigger
  Description  : Notifications of starts and stops of DataDigger windows 
  ----------------------------------------------------------------------*/
  define input parameter piChange as integer. 

  giNumDiggers = giNumDiggers + piChange.
  if giNumDiggers = 0 then apply 'close' to this-procedure.

END PROCEDURE. /* DataDigger */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getSourceFiles) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getSourceFiles Procedure 
PROCEDURE getSourceFiles :
/*------------------------------------------------------------------------
  Name         : getSourceFiles
  Description  : Read all source files with date/time stamp
  ----------------------------------------------------------------------
  16-04-2010 pti Created
  ----------------------------------------------------------------------*/
  
  DEFINE INPUT  PARAMETER pcDirectory AS CHARACTER NO-UNDO.
  DEFINE OUTPUT PARAMETER TABLE FOR ttOsFile.

  DEFINE VARIABLE cExtension AS CHARACTER   NO-UNDO.
  DEFINE VARIABLE cFile      AS CHARACTER   NO-UNDO EXTENT 3.

  DEFINE BUFFER bOsFile FOR ttOsFile.

  /* Read contents of progdir */
  EMPTY TEMP-TABLE bOsFile.
  INPUT FROM OS-DIR (pcDirectory).

  fileLoop:
  REPEAT:
    IMPORT cFile[1 FOR 3]. /* File FullPath Attributes */
    
    /* Only files */
    IF NOT cFile[3] BEGINS 'F' THEN NEXT fileLoop. 
    cExtension = ENTRY(NUM-ENTRIES(cFile[1], '.'),cFile[1],'.').
  
    /* Check if we see image files. These do not belong here, so just 
     * move them to their own directory (and create that one if needed)
     */
    IF LOOKUP(cExtension,'gif,ico') > 0 THEN 
    DO:
      OS-CREATE-DIR "image".
      OS-COPY VALUE(cFile[2]) VALUE("image\" + cFile[1]).
      OS-DELETE VALUE(cFile[2]).
      NEXT fileLoop. 
    END.

    /* Only valid file types (src + obj) */
    IF LOOKUP(cExtension,'r,i,w,p,cls') = 0 THEN NEXT fileLoop. 

    /* get info modified */
    FILE-INFO:FILE-NAME = cFile[2].

    /* Create it */
    CREATE bOsFile.
    ASSIGN
      bOsFile.dtModified    = DATETIME(STRING(FILE-INFO:FILE-MOD-DATE) + ' ' + STRING(FILE-INFO:FILE-MOD-TIME,'hh:mm:ss'))
      bOsFile.cModified     = getTimeStamp(bOsFile.dtModified)
      bOsFile.cFileName     = cFile[1]
      bOsFile.iFileSize     = FILE-INFO:FILE-SIZE
      bOsFile.cBaseName     = SUBSTRING(cFile[1],1,R-INDEX(cFile[1],'.') - 1)
      bOsFile.cFullPathname = cFile[2]
      bOsFile.cFileType     = cExtension
      .
  END.
  INPUT CLOSE.

END PROCEDURE. /* getSourceFiles */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-initializeObject) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initializeObject Procedure 
PROCEDURE initializeObject :
/*------------------------------------------------------------------------
  Name         : initializeObject
  Description  : Initialize all kind of things. 
  ----------------------------------------------------------------------
  16-04-2010 pti Created
  ----------------------------------------------------------------------*/
  
  define variable cDebuggerStart as character no-undo. 

  /* Are we at least 10.1B ? */
  if proversion < "10.1B" then
  do:
    message "You need at least Progress 10.1B to run DataDigger" skip(1)
            "The program will now quit."
            view-as alert-box info.
    quit.
  end.

  /* Where are we running from? */
  FILE-INFO:FILE-NAME = THIS-PROCEDURE:FILE-NAME.
  gcProgramDir = SUBSTRING(FILE-INFO:FULL-PATHNAME,1,R-INDEX(FILE-INFO:FULL-PATHNAME,'\')). 

  /* Add program dir to propath (if not already in) */
  if search('framelib.i') = ? then
    propath = gcProgramDir + ',' + propath.

  /* If the general ini file does not exist, create it */
  if search(gcProgramDir + 'DataDigger.ini') = ? then
  do:
    output to value(gcProgramDir + 'DataDigger.ini').
    output close. 
  end.

  /* In any case, load it */
  load 'DataDigger' dir gcProgramDir base-key 'ini' no-error.

  /* See if we should start the DD-Debugger */
  cDebuggerStart = getRegistry('debugger', 'start').

  /* Start debugger if needed */
  if logical(cDebuggerStart) then
    run value(gcProgramDir + "wDebugger.w") persistent.
    
  publish "timerCommand" ("start", "Startup").

  /* Disable updates if we started in an environment without sources */
  if search(gcProgramDir + 'DataDigger.p') = ? then
    setRegistry("DataDigger:update", "UpdateCheck","0").

end procedure. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-recompileDataDigger) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE recompileDataDigger Procedure 
PROCEDURE recompileDataDigger :
/*------------------------------------------------------------------------
  Name         : recompileDataDigger
  Description  : recompile all files and restart if needed

  ----------------------------------------------------------------------
  28-06-2011 pti Created
  ----------------------------------------------------------------------*/
  define variable cSetting as character no-undo.
  
  /* You can specify in the settings that you do not want to compile
   * This can be useful when you use DD in multiple environments
   * with different versions or codepages
   */
  cSetting = getRegistry("DataDigger", "AutoCompile").
  if cSetting = ? then setRegistry("DataDigger", "AutoCompile","yes").
  if logical(cSetting) = false then return.

  /* Find out if a recompile is needed (change in source files / newer version / older version) */
  if isRecompileNeeded() then
  do:
    /* Startup parameter -s should be set */
    if index(session:startup-parameters, "-s ") = 0 then
    do:
      message "You have not specified the -s startup parameter. DataDigger will not compile without this." skip(1)
              "Please set it to at least 128 and then try again."
        view-as alert-box info buttons ok.
      stop.
    end.

    /* Remove ourself from memory */
    run recompileSelf.
  
    run startDiggerLib(input true).
  end.

end procedure. /* recompileDataDigger */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-recompileSelf) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE recompileSelf Procedure 
PROCEDURE recompileSelf :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DEFINE VARIABLE winCompile         AS HANDLE    NO-UNDO.
  DEFINE VARIABLE cExpectedDateTime  AS CHARACTER NO-UNDO.
  DEFINE VARIABLE lCompileError      AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE lCoreFileError     AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE iFont              AS INTEGER   NO-UNDO.
  DEFINE VARIABLE cVersionInfo       AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cSystem            AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cMemory            AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cProgressDriveType AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cDiggerDriveType   AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cBuildNr           AS CHARACTER NO-UNDO.
  
  DEFINE BUFFER bOsFile FOR ttOsFile.

  /* Get progress version info */
  IF SEARCH("version") <> ? THEN
  DO:
    INPUT FROM VALUE(SEARCH("version")).
    IMPORT UNFORMATTED cVersionInfo.
    INPUT CLOSE.
  END.
  
  /* See if there is a special .p for this particular build */
  IF SEARCH("precompile.p") <> ? THEN
  DO:
    OS-DELETE VALUE(SEARCH("precompile.r")).
    RUN VALUE(SEARCH("precompile.p")) NO-ERROR.
  END.

  /* Get Windows version info */
  RUN adecomm\_winsys.p(OUTPUT cSystem, OUTPUT cMemory).

  ETIME(YES).
  RUN showMessage.p("DataDigger", "Please wait while DataDigger is recompiled.", OUTPUT winCompile).

  /* Check whether there are any sources. If you distribute DD without
   * sources, you definitely do not want to delete object files!
   */
  IF NOT CAN-FIND(FIRST bOsFile
                  WHERE bOsFile.cFileType = "p"
                     OR bOsFile.cFileType = "cls"
                     OR bOsFile.cFileType = "w") THEN
  DO:
    MESSAGE "No source files found. Compiling aborted.".
    
    OUTPUT CLOSE.
    EMPTY TEMP-TABLE bOsFile.
    DELETE WIDGET winCompile.
    RETURN.
  END.

  /* Start the timer. We want the message to appear at least a certain time 
   * to avoid flashing of windows 
   */
  ETIME(YES).
  SESSION:SET-WAIT-STATE("general").

  /* Open log */
  OUTPUT TO VALUE(gcProgramDir + "DataDigger.log").
  PUT UNFORMATTED "DataDigger recompile as of " string(now,"99-99-9999 HH:MM:SS").

  FILE-INFO:FILE-NAME = "prowin32.exe".
  cProgressDriveType = getDriveType(ENTRY(1,FILE-INFO:FULL-PATHNAME,"\")).
  cDiggerDriveType   = getDriveType(ENTRY(1,gcProgramDir,"\")).

  PUT UNFORMATTED SKIP(1) "ENVIRONMENT".
  PUT UNFORMATTED SKIP(0) "  Progress version   : " PROVERSION " " PROGRESS " " SESSION:CLIENT-TYPE.
  PUT UNFORMATTED SKIP(0) "  Version info file  : " cVersionInfo.
  PUT UNFORMATTED SKIP(0) "  Progress path      : " FILE-INFO:FULL-PATHNAME.
  PUT UNFORMATTED SKIP(0) "  Drive type         : " cProgressDriveType.
  PUT UNFORMATTED SKIP(0) "  ProPath            : " PROPATH.
  PUT UNFORMATTED SKIP(0) "  Windows version    : " SESSION:WINDOW-SYSTEM " " cSystem + ", " getProcessorArchitecture() "bit".
  PUT UNFORMATTED SKIP(0) "  System memory      : " cMemory.
  PUT UNFORMATTED SKIP(0) "  Display size       : " SESSION:WORK-AREA-WIDTH-PIXELS " x " SESSION:WORK-AREA-HEIGHT-PIXELS.
  PUT UNFORMATTED SKIP(0) "  Logged in as       : " OS-GETENV("username").

  PUT UNFORMATTED SKIP(1) "SESSION INFO".
  PUT UNFORMATTED SKIP(0) "  Program dir        : " gcProgramDir.
  PUT UNFORMATTED SKIP(0) "  Drive type         : " cDiggerDriveType.
  PUT UNFORMATTED SKIP(0) "  Codepage           : " SESSION:CPINTERNAL.
  PUT UNFORMATTED SKIP(0) "  Character size     : " SESSION:PIXELS-PER-COLUMN " x " SESSION:PIXELS-PER-ROW.
  PUT UNFORMATTED SKIP(0) "  Startup parameters : " SESSION:STARTUP-PARAMETERS.
  PUT UNFORMATTED SKIP(0) "  Temp directory     : " SESSION:TEMP-DIRECTORY.
  PUT UNFORMATTED SKIP(0) "  System alert boxes : " SESSION:SYSTEM-ALERT-BOXES.
  PUT UNFORMATTED SKIP(0) "  Three-D            : " SESSION:THREE-D.
  PUT UNFORMATTED SKIP(0) "  V6Display          : " SESSION:V6display.

  PUT UNFORMATTED SKIP(1) "CURRENT FILES".
  FOR EACH bOsFile:
    cExpectedDateTime = getRegistry("DataDigger:files", bOsFile.cFileName).

    DISPLAY 
      bOsFile.cFileName  COLUMN-LABEL "File name" AT 3
      bOsFile.iFileSize  COLUMN-LABEL "Size "
      bOsFile.dtModified COLUMN-LABEL "File date"
      cExpectedDateTime  COLUMN-LABEL "Expected" FORMAT "x(20)"
      bOsFile.cStatus    COLUMN-LABEL "Status"
      WITH WIDTH 120 STREAM-IO NO-BOX.
  END.

  /* Delete old .r files */
  PUT UNFORMATTED SKIP(1) "Deleting old .r files".
  FOR EACH bOsFile WHERE bOsFile.cFileType = "r":
    OS-DELETE VALUE(bOsFile.cFullPathname).
  END.

  /* Recompile */
  PUT UNFORMATTED SKIP(1) "RECOMPILING".

  FOR EACH bOsFile 
    WHERE bOsFile.cFileType = "p"
       OR bOsFile.cFileType = "cls"
       OR bOsFile.cFileType = "w":

    MESSAGE "  Compiling:" bOsFile.cFullPathName.
    COMPILE VALUE(bOsFile.cFullPathName) SAVE.

    IF COMPILER:ERROR THEN 
    DO:
      ASSIGN lCompileError = TRUE.        
      IF bOsFile.cFileName <> "myDataDigger.p" THEN lCoreFileError = TRUE.
    END.
  END.

  /* Reread dir to catch new date/times of .r files */
  RUN getSourceFiles(INPUT gcProgramDir, OUTPUT TABLE bOsFile).

  /* Save date/time of all files in INI-file */
  FOR EACH bOsFile: 
    USE "DataDigger".
    PUT-KEY-VALUE SECTION "DataDigger:files" KEY bOsFile.cFileName VALUE STRING(bOsFile.cModified).
    USE "".
  END.

  IF NOT lCompileError THEN 
    PUT UNFORMATTED SKIP(1) "All files successfully compiled.".
  ELSE 
  DO:
    PUT UNFORMATTED 
      SKIP(1) "Error while recompiling (see above)" .

    IF SEARCH("myDataDigger.p") <> ? AND SEARCH("myDataDigger.r") = ? THEN
      PUT UNFORMATTED 
        SKIP(1) "Apparantly, something is broken in your custom code :)" 
        SKIP    "DataDigger will now start without your customizations"
        SKIP    "Fix the errors or rename myDataDigger.p otherwise DD will "
        SKIP    "try to compile it each time it starts.".

    IF lCoreFileError THEN
      PUT UNFORMATTED 
        SKIP(1) "There is an error in one of DD's own files." 
        SKIP    "If you have not messed with DataDigger, please send this"
        SKIP    "logfile to patrick@tingen.net".

    PUT UNFORMATTED 
      SKIP(1) "Sorry for the inconvenience ..."
      SKIP    " ".
  END.

  /* Close the log */
  OUTPUT CLOSE. 

  /* Show the window at least some time, otherwise it will flash, which is annoying */
  REPEAT WHILE ETIME < 1500:
    PROCESS EVENTS.
  END.

  SESSION:SET-WAIT-STATE("").

  IF lCompileError THEN 
  DO:
    MESSAGE "An error occurred while recompiling. ~n~nPlease check 'DataDigger.log' in the DataDigger directory."
      VIEW-AS ALERT-BOX INFO BUTTONS OK.
    OS-COMMAND NO-WAIT START "datadigger.log".
  END.

  /* Clean up */
  EMPTY TEMP-TABLE bOsFile.
  DELETE WIDGET winCompile.

END PROCEDURE. /* recompileSelf */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-startDiggerLib) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE startDiggerLib Procedure 
PROCEDURE startDiggerLib :
/*------------------------------------------------------------------------
  Name         : startDiggerLib
  Description  : Start DiggerLib if it has not already been started

  If the lib was already started, it might be a lib of a previous version if you
  install DataDigger over a running version. To avoid errors, we get the version 
  of the running instance and see if it matches the version of the window. 
  If not, close all other windows (these are old and if we restart the lib then
  in turn THEY will be out of sync with the lib) and restart the lib.

  ----------------------------------------------------------------------
  21-10-2009 pti Created
  ----------------------------------------------------------------------*/
  
  define input parameter plForcedRestart as logical no-undo.
  define variable hDiggerLib as handle no-undo.

  /* Call out to see if the lib has been started for this build nr */
  publish 'DiggerLib' (output hDiggerLib). 

  /* If we MUST restart (after recompile), or if there is a new version, kill the library */
  if plForcedRestart then
  do:
    /* Publish a close to all open digger windows. The one that issues
     * this publish will not be closed because we are not subscribed yet. 
     */
    publish 'DataDiggerClose'.
    delete procedure hDiggerLib no-error.
    hDiggerLib = ?.
  end.

  /* Now, start the lib */
  if not valid-handle(hDiggerLib) then 
  do:
    run value(gcProgramDir + 'DataDiggerLib.p') persistent set hDiggerLib.
    session:add-super-procedure(hDiggerLib, search-target).
  end.

end procedure. /* startDiggerLib */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

/* ************************  Function Implementations ***************** */

&IF DEFINED(EXCLUDE-getDriveType) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getDriveType Procedure 
FUNCTION getDriveType RETURNS CHARACTER ( pcDrive AS CHARACTER ):
  /* Return the type of drive */
  DEFINE VARIABLE iType AS INTEGER NO-UNDO.

  RUN GetDriveTypeA(INPUT pcDrive, OUTPUT iType).

  CASE iType:
    WHEN {&DRIVE_UNKNOWN}     THEN RETURN ?.
    WHEN {&DRIVE_NO_ROOT_DIR} THEN RETURN ?.
    WHEN {&DRIVE_REMOVABLE}   THEN RETURN "Removable".
    WHEN {&DRIVE_FIXED}       THEN RETURN "Fixed".
    WHEN {&DRIVE_REMOTE}      THEN RETURN "Remote".
    WHEN {&DRIVE_CDROM}       THEN RETURN "CD-Rom".
    WHEN {&DRIVE_RAMDISK}     THEN RETURN "Ramdisk".
  END CASE. 

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getProcessorArchitecture) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getProcessorArchitecture Procedure 
FUNCTION getProcessorArchitecture RETURNS INTEGER():
  DEFINE VARIABLE ival1 AS INT64 NO-UNDO. 
  DEFINE VARIABLE ival2 AS INT64 NO-UNDO. 
  DEFINE VARIABLE mdata AS MEMPTR NO-UNDO. 
  
  ival1 = 0x12345678abcdef12. 
  SET-POINTER-VALUE(mdata) = ival1. 
  ival2 = GET-POINTER-VALUE(mdata). 

  RETURN (IF ival1 = ival2 THEN 64 ELSE 32).
END FUNCTION. /* getProcessorArchitecture */

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
  Description  : Get a value from DataDigger.ini Not from personal ini! 
  ----------------------------------------------------------------------*/
  define variable cRegistryValue as character   no-undo.

  use 'DataDigger'.
  get-key-value section pcSection key pcKey value cRegistryValue.
  use "".

  return cRegistryValue.
end function. /* getRegistry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getTimeStamp) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION getTimeStamp Procedure 
FUNCTION getTimeStamp RETURNS CHARACTER
  ( input pDateTime as datetime ) :

  /* Return a timestamp in the form "YYYY-MM-DD HH:MM:SS" */

  return 
    substitute('&1-&2-&3 &4'
              , string(year(pDateTime),'9999')
              , string(month(pDateTime),'99')
              , string(day(pDateTime),'99')
              , string( integer( truncate( mtime( pDateTime ) / 1000, 0 ) ),'HH:MM:SS' )
              ).

END FUNCTION. /* getTimeStamp */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-isRecompileNeeded) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION isRecompileNeeded Procedure 
FUNCTION isRecompileNeeded returns logical
  ( /* parameter-definitions */ ) :

  DEFINE VARIABLE lRecompileNeeded AS LOGICAL   NO-UNDO.
  DEFINE VARIABLE cRegistryValue   AS CHARACTER NO-UNDO.

  DEFINE BUFFER bOsFile FOR ttOsFile.

  /* If we run a version of Progress that does not allow compilation
   * we simply return that no recompile is needed.
   */
  IF LOOKUP(PROGRESS, 'Full,Query') = 0 THEN RETURN FALSE. 

  /* Read all files from program dir. */
  RUN getSourceFiles(INPUT gcProgramDir, OUTPUT TABLE ttOsFile).

  /* Has any of the source files changed since the last run? */
  FOR EACH bOsFile WHERE CAN-DO('i,p,w,cls', bOsFile.cFileType):
    cRegistryValue = getRegistry('DataDigger:files', bOsFile.cFileName).

    IF cRegistryValue = ? THEN bOsFile.cStatus = 'Status unknown'.
    ELSE
    IF cRegistryValue <> bOsFile.cModified THEN bOsFile.cStatus = 'File modified'.
  END.

  /* Does every source has an object? */
  FOR EACH bOsFile WHERE CAN-DO('p,w,cls', bOsFile.cFileType):

    IF NOT CAN-FIND(ttOsFile WHERE ttOsFile.cBaseName = bOsFile.cBaseName
                               AND ttOsFile.cFileType = 'R') THEN 
      bOsFile.cStatus = 'File has no .r'.
  END.

  /* Need to recompile? */
  RETURN CAN-FIND(FIRST bOsFile WHERE bOsFile.cStatus <> '').
END FUNCTION. /* isRecompileNeeded */

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
  Name         : getRegistry 
  Description  : Get a value from DataDigger.ini Not from personal ini! 
  ----------------------------------------------------------------------*/
  use 'DataDigger'.
  put-key-value section pcSection key pcKey value pcValue no-error.
  use "".

  return "".
end function. /* setRegistry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF
