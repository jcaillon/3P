### Improvements ###

### Fixed issues ###

- \#246: Wrong behavior for the auto-completion in multi-carets mode 
- \#251: Silent error parsing UserDefineLang.xml
- \#250: Crash during shutdown due to modified collection
- \#249: Admin rights required to update 3P for notepad++ version 7.6.1 to 7.6.2
- \#252: From notepad++ v7.6.2, autocompletion .xml files were moved to $installdir/autoCompletion
- \#227: Infinite loop on includes calling the same include (huge thanks to @slegian for his precious help)

### Recent changes in notepad++ ###

Little warning, if you plan to update your notepad++ installation (which you probably should do :p).

Since v7.6.x, there has been some changes regarding the plugins location:

- in v7.6.0, plugins were moved from `/plugins/3P.dll` to `/plugins/3P/3P.dll`.
- in v7.6.1, (for non portable installation) the base plugins folder was moved from `%LOCALAPPDATA%` to `%PROGRAMDATA%`.
- in v7.6.3, (for non portable installation) the base plugins folder was moved from `%PROGRAMDATA%` to `%ProgramFiles%\Notepad++\plugins\`.

Hopefully, the latest change is final.

I've updated the 3P documentation to reflect those changes. If you update to the latest version of notepad++ from an older version (<= 7.6), you will have to manually place the `3P.dll` into a `3P` sub folder of your `plugins` folder. Otherwise, 3P will simply not be loaded.

Enjoy!