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

/* ***************************  Definitions  ************************** */
define input  parameter piChannel     as integer no-undo.
define input  parameter plInteractive as logical no-undo.
define output parameter plNewVersion  as logical no-undo.

/* Buildnr, temp-tables and forward defs */
{ DataDigger.i }

define variable gcProgramDir as character no-undo.
define variable gcError      as character no-undo. 

/* Needed for upgrade of DataDigger */
procedure URLDownloadToFileA external "URLMON.DLL" :
   define input parameter pCaller    as long.
   define input parameter szURL      as character.
   define input parameter szFilename as character.
   define input parameter dwReserved as long.
   define input parameter lpfnCB     as long.
   define return parameter ReturnValue as long.
end procedure.

procedure DeleteUrlCacheEntry external "WININET.DLL" :
   define input parameter lbszUrlName as character.
end procedure.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE no



/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */

&IF DEFINED(EXCLUDE-getRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD getRegistry Procedure 
FUNCTION getRegistry returns character
    ( pcSection as character
    , pcKey     as character 
    )  FORWARD.

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
         HEIGHT             = 18.48
         WIDTH              = 46.
/* END WINDOW DEFINITION */
                                                                        */
&ANALYZE-RESUME

 


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Procedure 


/* ***************************  Main Block  *************************** */

run initializeObject.

if plInteractive = yes then 
  run showLatestVersion(output gcError).
else 
  run getNewVersion(output gcError).

if gcError <> '' then 
  message gcError view-as alert-box info buttons ok.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&IF DEFINED(EXCLUDE-getNewVersion) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getNewVersion Procedure 
PROCEDURE getNewVersion :
/*------------------------------------------------------------------------
  Name         : getNewVersion
  Description  : Download a new version of DD, if needed
  ----------------------------------------------------------------------
  19-04-2012 pti Created
  ----------------------------------------------------------------------*/
  
  define output parameter pcError as character no-undo. 

  define variable iInterval         as integer   no-undo.
  define variable iChannel          as integer   no-undo. 
  define variable iResult           as integer   no-undo.
  define variable tLastCheck        as datetime  no-undo. 
  define variable cInterval         as character no-undo extent 4 initial ["days","weeks","months","seconds"].
  define variable cLatestVersion    as character no-undo. 
  define variable cDownloadLocation as character no-undo. 

  /* How often should we check? 
   * 0 = Never | 1 = Daily | 2 = Weekly | 3 = Monthly | 4 = every run (debugging)
   */
  iInterval = integer(dynamic-function("getRegistry", "DataDigger:update", "UpdateCheck")) no-error.
  if iInterval = ? or iInterval = 0 then return. /* rubbish */

  /* When was the last update check? */
  tLastCheck = datetime(getRegistry("DataDigger:update", "UpdateLastCheck")) no-error.

  /* Check the date we should be doing a check. If it's in the future, go back */
  if tLastCheck <> ?
    and add-interval(tLastCheck, 1, cInterval[iInterval]) > now then return.

  /* Remember we did the check */
  setRegistry("DataDigger:update", "UpdateLastCheck", string(now)).

  /* Which update channel? 0 = stable | 1 = beta */
  iChannel = integer(dynamic-function("getRegistry", "DataDigger:update", "UpdateChannel")) no-error.
  if iChannel = ? then iChannel = 0. /* rubbish -> stable */

  downloadBlock:
  do:
    /* What is the latest version? */
    run getNewVersionInfo
      ( input iChannel 
      , output cLatestVersion
      , output cDownloadLocation
      , output pcError
      ).

    if pcError <> "" then
      leave downloadBlock.

    /* Do we need to upgrade? */
    if cLatestVersion > '{&build}' then
    do:
      run upgradeDataDigger(input cDownloadLocation, output pcError).
      if pcError <> '' then return.
    end.

  end. /* latest version > this one */

end procedure. /* getNewVersion */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-getNewVersionInfo) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getNewVersionInfo Procedure 
PROCEDURE getNewVersionInfo :
/*------------------------------------------------------------------------
  Name         : getNewVersionInfo
  Description  : Get info about latest version of DataDigger
  ----------------------------------------------------------------------
  17-03-2012 pti Created
  ----------------------------------------------------------------------*/

  define input  parameter piChannel          as integer   no-undo. 
  define output parameter pcLatestVersion    as character no-undo. 
  define output parameter pcDownloadLocation as character no-undo. 
  define output parameter pcError            as character no-undo. 

  define variable cIniFile       as character no-undo.
  define variable cRemoteDir     as character no-undo.
  define variable cRemoteFile    as character no-undo.
  define variable winUpdateCheck as handle    no-undo.
  define variable iResult        as integer   no-undo.
  define variable iFont          as integer   no-undo.

  run showMessage.p("DataDigger", "Checking for new version, please wait.", output winUpdateCheck).
  session:set-wait-state("general").

  getInfo:
  do:
    /* Get location of remote ini file */
    cRemoteFile = getRegistry("DataDigger:update","UpdateUrl").
    if cRemoteFile = ? then do:
      pcError = "Update-URL not defined in settings.".
      leave getInfo.
    end.
    
    cIniFile   = entry(num-entries(cRemoteFile,"/"),cRemoteFile,"/").
    cRemoteDir = substring(cRemoteFile,1,r-index(cRemoteFile,"/")).
  
    /* Download the project page for DataDigger from oehive */
    run DeleteURLCacheEntry (input cRemoteFile). /* remove old version from IE cache */
    run urlDownloadToFileA (0, cRemoteFile, session:temp-dir + cIniFile, 0, 0, output iResult).
    
    /* Error? */
    if iResult <> 0 or search(session:temp-dir + cIniFile) = ? then do:
      pcError = substitute("Cannot retrieve information from '&1'", cRemoteDir).
      leave getInfo.
    end.
    
    /* Parse the file */
    load cIniFile dir session:temp-dir base-key 'ini' no-error.
    use cIniFile.
    case piChannel:
      /* Normal channel */
      when 0 then do:
        get-key-value section "DataDigger" key "latest"   value pcLatestVersion.
        get-key-value section "DataDigger" key "download" value pcDownloadLocation.
      end.
  
      /* Beta channel */
      when 1 then do:
        get-key-value section "BetaDigger" key "latest"   value pcLatestVersion.
        get-key-value section "BetaDigger" key "download" value pcDownloadLocation.
      end.
    end case.
    unload 'DataDiggerVersion'.
  end. /* getInfo */

  delete widget winUpdateCheck.
  session:set-wait-state("").

end procedure. /* getNewVersionInfo */

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
  
  /* Where are we running from? */
  /*
  if '{&uib_is_running}' <> '' then 
    gcProgramDir = 'd:\data\dropbox\datadigger\'.
  else do:
    file-info:file-name = this-procedure:file-name.
    gcProgramDir = substring(file-info:full-pathname,1,r-index(file-info:full-pathname,'\')).
  end.
  */
  gcProgramDir = SUBSTRING(THIS-PROCEDURE:FILE-NAME,1,R-INDEX(THIS-PROCEDURE:FILE-NAME,'\')).
  
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

end procedure. /* initializeObject */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-showLatestVersion) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE showLatestVersion Procedure 
PROCEDURE showLatestVersion :
/*------------------------------------------------------------------------
  Name         : showLatestVersion
  Description  : Get info about the latest version and show it
  ----------------------------------------------------------------------*/
  define output parameter pcError as character no-undo. 
  
  define variable cLatestVersion    as character no-undo. 
  define variable cDownloadLocation as character no-undo. 
  define variable lUpgrade          as logical   no-undo.

  /* We should do an update check */
  run getNewVersionInfo
    ( input piChannel 
    , output cLatestVersion
    , output cDownloadLocation
    , output pcError
    ).
  if pcError <> "" then return. 

  /* Latest is newer! */
  if cLatestVersion > '{&build}' then
  do:
    message 'Your version  : {&build} ' skip
            'Latest version:' cLatestVersion skip(1)
            'Would you like to upgrade?'
      view-as alert-box info buttons yes-no-cancel update lUpgrade.

    if lUpgrade = true then
    do:
      run upgradeDataDigger(input cDownloadLocation, output pcError).
      if pcError <> '' then return.
    end.
  end.
  
  else
  if cLatestVersion = '{&build}' then
  do: 
    message 'Your version  : {&build} ' skip
            'Latest version:' cLatestVersion skip(1)
            'Your are running the latest version of DataDigger'
      view-as alert-box info buttons ok.
  end.

  else
  if cLatestVersion < '{&build}' then
  do: 
    message 'Your version  : {&build} ' skip
            'Latest version:' cLatestVersion skip(1)
            'Your version is newer than the official one, ' skip
            'you are probably in the beta program.'
      view-as alert-box info buttons ok.
  end.

end procedure. /* btnCheckUpgrade */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

&IF DEFINED(EXCLUDE-upgradeDataDigger) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE upgradeDataDigger Procedure 
PROCEDURE upgradeDataDigger :
/*------------------------------------------------------------------------
  Name         : upgradeDataDigger
  Description  : Download and install latest version.
  ----------------------------------------------------------------------*/
  
  define input  parameter pcDownloadLocation as character no-undo. 
  define output parameter pcError            as character no-undo. 

  define variable cZipFile   as character no-undo. 
  define variable cUnzipCmd  as character no-undo.
  define variable iResult    as integer   no-undo.
  
  /* Strip all, except for the zipfile name */
  cZipFile = entry(num-entries(pcDownloadLocation,"/"),pcDownloadLocation,"/").
  
  /* If it is not already downloaded */
  if search(gcProgramDir + cZipFile) = ? then
  do:
    /* Download the new file from oehive */
    run DeleteURLCacheEntry (input pcDownloadLocation). /* remove old version from IE cache */
    run urlDownloadToFileA (0, pcDownloadLocation, gcProgramDir + cZipFile, 0, 0, output iResult).

    /* Check this action */
    if iResult <> 0 or search(gcProgramDir + cZipFile) = ? then 
    do:
      pcError = substitute("Download of '&1' failed. Please download manually.", cZipFile).
      return. 
    end.
  end.

  /* Otherwise, unzip it */
  /* %DLC%\jdk\bin\jar.exe xf zipfile.zip */
  /* c:\dlc101b\bin\prowin32.exe */
  file-info:file-name = "jdk\bin\jar.exe".
  cUnzipCmd = substitute("cd /d &2 && &1 xf &2&3", file-info:full-pathname, gcProgramDir, cZipFile).
  os-command silent value(cUnzipCmd).

  /* New version has been installed! */
  plNewVersion = true.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF

/* ************************  Function Implementations ***************** */

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

&IF DEFINED(EXCLUDE-setRegistry) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION setRegistry Procedure 
FUNCTION setRegistry returns character
    ( pcSection as character
    , pcKey     as character 
    , pcValue   as character 
    ) :
/*------------------------------------------------------------------------
  Name         : setRegistry 
  Description  : Set a value in DataDigger.ini 
  ----------------------------------------------------------------------*/
  use 'DataDigger'.
  put-key-value section pcSection key pcKey value pcValue no-error.
  use "".

  return "".
end function. /* setRegistry */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF