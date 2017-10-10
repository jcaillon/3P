Hello, long time no see! I had less time to spend on 3P lately but that doesn't mean the project is dead, far from it! I'm still planning to release a stable in a near future (that will include OOABL), please be patient :)
And without further ado, let's move on to the news...

**Prolint automatically installed!**

Remark : The location of the StartProlint.p procedure has changed, it is now in a folder called `prolint`. This means your previous procedure is not lost and you can reuse parts of it for the new procedure. When you are done, it is advised to delete the old and obsolete file.

**Improvements :**

- New option in the update page, you can now configuration a proxy to use to check for updates
- New options for the deployment :
  - You can now send files to a .cab file (works like zip rules)
  - Changed the behavior of the `display parser errors on save`, the notification now allows you to disable the feature for the current file only
  - Extended the possibilities of the deployer :
    - CopyFolder
	- DeleteFolder
  - You can now use the special variable `<ROOT>` which is always available and is replaced by the path to your source directory (without the ending '')
  - For the transfer rules and in particular, for the transfert target : it is now possible to use the | character to separate several target path (the file will then be copied to the different locations). This is equivalent to writing several rules with the "continue" option set to "yes", it is just a simpler way to write it
  - Improved updater and added a setup dialog box after an install to modify notepad++ options such as : automatic backup, multi-selection and default auto-completion replacement
  - Added keywords : `PROCESS-ARCHITECTURE` `PROCEDURE-COMPLETE` `Write`

**Fixed issues :**

- #160, #161 : problems with prototype synchronization in include files
- #162 : no fields available on the autocompletion for buffers
- Problem with progress v9x : issue with the use of too many streams in the 3P progress program, v9 limits the max stream to 5 -> corrected to only use 1 stream...
- Fields display all the flags
- Change the behavior of the prototype update, it now occurs AFTER the file is saved, which is safer in case of a problem
- #163 : The code explorer now has again the button "from include" that allows to display/hide the items parsed from include files
- No longer automatically autocompletes simple numbers
- fixed compilation errors display issue that would occur if the error was located in an include file for instance
- #164 : Search window inactive when CRTL+F
- #165 : Incomplete procedure name when using preprocessed variables
- Fixed the critical bug with progress version inferior to 11.6 in the latest version of 3P
- Fix a problem occuring when the PROPATH used was too long

And as always, ENJOY!