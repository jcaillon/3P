/*------------------------------------------------------------------------
  File : checkVersion.p
  Desc : Check if there is a new version on GitHub
    
  Notes:
    The version nr is increased when it is ready for production, the
    build nr is increaded when something is ready for beta testing.
    
  Parameters:
    piChannel     : 0=no check, 1=check stable, 2=check beta
    plManualCheck : TRUE when user presses 'Check Now' button
  ----------------------------------------------------------------------*/

DEFINE INPUT PARAMETER piChannel     AS INTEGER NO-UNDO.
DEFINE INPUT PARAMETER plManualCheck AS LOGICAL NO-UNDO.

{ DataDigger.i }

DEFINE VARIABLE cLocalVersion  AS CHARACTER   NO-UNDO INITIAL '{version.i}'.
DEFINE VARIABLE cLocalBuildNr  AS CHARACTER   NO-UNDO INITIAL '{build.i}'.
DEFINE VARIABLE cRemoteVersion AS CHARACTER   NO-UNDO.
DEFINE VARIABLE cRemoteBuildNr AS CHARACTER   NO-UNDO.
DEFINE VARIABLE lAutoCheck     AS LOGICAL     NO-UNDO.

RUN getVersionInfo.p(OUTPUT cRemoteVersion, OUTPUT cRemoteBuildNr).


IF (cRemoteVersion > cLocalVersion)
  AND (   plManualCheck = TRUE
       OR piChannel = {&CHECK-STABLE} 
       OR piChannel = {&CHECK-BETA}) THEN
DO:
  OS-COMMAND NO-WAIT START VALUE('https://datadigger.wordpress.com/category/status').
  MESSAGE 'A new version is available on the DataDigger website' VIEW-AS ALERT-BOX INFO BUTTONS OK.
END.
    
ELSE
IF    (cRemoteVersion = cLocalVersion)
  AND (cRemoteBuildNr > cLocalBuildNr)
  AND (   plManualCheck = TRUE 
       OR piChannel = {&CHECK-BETA}) THEN
DO:
  OS-COMMAND NO-WAIT START VALUE('https://datadigger.wordpress.com/category/beta').
  MESSAGE 'A new BETA version is available on the DataDigger website' VIEW-AS ALERT-BOX INFO BUTTONS OK.
END.
  
/* In case of a manual check, report what is found */
ELSE
IF plManualCheck
  AND cRemoteVersion <= cLocalVersion
  AND cRemoteBuildNr <= cLocalBuildNr THEN
DO:
  MESSAGE 'No new version available, you are up to date.' VIEW-AS ALERT-BOX INFO BUTTONS OK.
END.

