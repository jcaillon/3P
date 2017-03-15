/*************************************************************************
  File    : preCompile.p
  Purpose : Perform actions that must be done prior to recompile

  When new features are implemented, sometimes files become obsolete.
  They should be removed to avoid compilation problems but this must
  be done BEFORE DataDigger recompiles itself.
  
  The DataDigger.p programs takes this into account and when it decides
  it should recompile, it deletes the old version of preCompile.r and
  then runs the uncompiled version. This is needed, because at the time
  the program runs, the OLD .r files are still in place, but the NEW
  source files are there.

 *************************************************************************/

/* No actions required for this version */

