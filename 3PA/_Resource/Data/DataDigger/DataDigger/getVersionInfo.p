/*------------------------------------------------------------------------
  File : getVersionInfo.p
  Desc : Give back latest versions from DataDigger on GitHub
    
  Notes:
    The version nr is increased when it is ready for production, the
    build nr is increaded when something is ready for beta testing.
  ----------------------------------------------------------------------*/
  
DEFINE OUTPUT PARAMETER pcVersion AS CHARACTER NO-UNDO.
DEFINE OUTPUT PARAMETER pcBuildNr AS CHARACTER NO-UNDO.

FUNCTION getRemoteFile RETURNS CHARACTER (pcRemoteFile AS CHARACTER) FORWARD.

pcVersion = getRemoteFile('https://raw.githubusercontent.com/patrickTingen/DataDigger/master/version.i').
pcBuildNr = getRemoteFile('https://raw.githubusercontent.com/patrickTingen/DataDigger/master/build.i').

pcVersion = TRIM(pcVersion).
pcBuildNr = TRIM(pcBuildNr).

/* ---------- implementation ---------- */
PROCEDURE URLDownloadToFileA EXTERNAL "URLMON.DLL" :
   DEFINE INPUT PARAMETER pCaller    AS LONG.
   DEFINE INPUT PARAMETER szURL      AS CHARACTER.
   DEFINE INPUT PARAMETER szFilename AS CHARACTER.
   DEFINE INPUT PARAMETER dwReserved AS LONG.
   DEFINE INPUT PARAMETER lpfnCB     AS LONG.
   DEFINE RETURN PARAMETER ReturnValue AS LONG.
END PROCEDURE. /* URLDownloadToFileA */


PROCEDURE DeleteUrlCacheEntry EXTERNAL "WININET.DLL" :
   DEFINE INPUT PARAMETER lbszUrlName AS CHARACTER.
END PROCEDURE. /* DeleteUrlCacheEntry */


FUNCTION getRemoteFile RETURNS CHARACTER (pcRemoteFile AS CHARACTER):
  DEFINE VARIABLE cLocalFile AS CHARACTER NO-UNDO.
  DEFINE VARIABLE cContents  AS LONGCHAR  NO-UNDO.
  DEFINE VARIABLE iResult    AS INTEGER   NO-UNDO.

  cLocalFile = SESSION:TEMP-DIR + 'VersionInfo.txt'.
  OS-DELETE cLocalFile.

  RUN DeleteURLCacheEntry (INPUT pcRemoteFile).
  RUN urlDownloadToFileA (0, pcRemoteFile, cLocalFile, 0, 0, OUTPUT iResult).

  IF SEARCH(cLocalFile) <> ? THEN
    COPY-LOB FILE cLocalFile TO cContents.
    
  RETURN STRING(cContents).
END FUNCTION. /* getRemoteFile */
