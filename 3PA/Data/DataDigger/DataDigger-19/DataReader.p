/*------------------------------------------------------------------------
    File        : DataReader.p
    Purpose     : Start an instance of DataDigger in ReadOnly mode
  ----------------------------------------------------------------------*/

define new global shared variable ReadOnlyDigger as logical no-undo.
define variable gcProgramDir as character no-undo.

/* Where are we running from? */
gcProgramDir = SUBSTRING(THIS-PROCEDURE:FILE-NAME,1,R-INDEX(THIS-PROCEDURE:FILE-NAME,'\')).
/*
if '{&uib_is_running}' <> '' then
  gcProgramDir = 'd:\data\dropbox\datadigger\'.
else do:
  file-info:file-name = this-procedure:file-name.
  gcProgramDir = substring(file-info:full-pathname,1,r-index(file-info:full-pathname,'\')).
end.
*/

/* Start the actual DataDigger program */
ReadOnlyDigger = true.
run value(gcProgramDir + "DataDigger.p").