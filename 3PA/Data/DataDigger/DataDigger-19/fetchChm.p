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

/* Buildnr, temp-tables and forward defs */
{ DataDigger.i }

define variable cRemoteFile as character no-undo.
define variable gcError as character no-undo.
define variable gcProgramDir as character no-undo.
define variable winCompile as handle no-undo.

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

  RUN showMessage.p("DataDigger", "Please wait while the help file is being downloaded.", OUTPUT winCompile).
  ETIME(YES).
  SESSION:SET-WAIT-STATE("general").

cRemoteFile = getRegistry("DataDigger:Update","ChmDownloadUrl").
if cRemoteFile = ? then
    gcError = "Download-URL not defined in settings.".
ELSE
    run upgradeDataDigger(input cRemoteFile, INPUT gcProgramDir + "DataDigger.chm", output gcError).
    
  /* Show the window at least some time, otherwise it will flash, which is annoying */
  REPEAT WHILE ETIME < 1500:
    PROCESS EVENTS.
  END.

  SESSION:SET-WAIT-STATE("").
  DELETE WIDGET winCompile.
    
if gcError <> '' then 
  message gcError view-as alert-box info buttons ok.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

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

&IF DEFINED(EXCLUDE-upgradeDataDigger) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE upgradeDataDigger Procedure 
PROCEDURE upgradeDataDigger :
/*------------------------------------------------------------------------
  Name         : upgradeDataDigger
  Description  : Download and install latest version.
  ----------------------------------------------------------------------*/
  
  define input  parameter pcUrlDistant as character no-undo. 
  define input  parameter pcPathLocal as character no-undo. 
  define output parameter pcError as character NO-UNDO INITIAL "". 

  define variable iResult    as integer   no-undo.

  /* If it is not already downloaded */
  if search(pcPathLocal) = ? then
  do: 
    /* Download the new file from oehive */
    run DeleteURLCacheEntry (input pcUrlDistant). /* remove old version from IE cache */
    run urlDownloadToFileA (0, pcUrlDistant, pcPathLocal, 0, 0, output iResult).

    /* Check this action */
    if iResult <> 0 or search(pcPathLocal) = ? then 
    do:
      pcError = substitute("Download of '&1' failed. Please download manually.", pcUrlDistant).
      RETURN "". 
    end.
  end.
  
  RETURN "".

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
