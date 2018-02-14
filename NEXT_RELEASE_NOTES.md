The database information are reset in this version, you will need to fetch them again from the environment page

**Improvements :**

- Changed the way include files are treated by the parser; they are now treated the same way as {&var} which allows more complex usage of include files like {{inc.i}} (where inc.i contains an include name)
- #204 : The parser now handles &IF everywhere
- #203 : The parser now correctly reads and interprets ALL includes, even when they are within strings!
- #207 : Missing includes and missing preprocessed variables are now displayed in a new branch of the code explorer "Missing includes/variables" which allows to solve PROPATH problems faster
- #199 : 
	- Fields of tables are now sorted by names by default (instead of order number previously)
	- In tooltips fields that are part of the primary index are always shown on top
	- Moved the shortcuts help from bottom to top in tooltips (to be able to see the hit CTRL once even if the tooltip is long)
- #205 : The "check code validity" notification that is displayed on document save will now close itself if you fix the file problem and save again. It also only triggers on progress compilable files
- #208 : 3P can now indent correctly very complex code (it actually does a better job than developper studio)
- Added more information on tables (in the tooltip) : is hidden, is frozen and table type (T,S,V)
- #152 : you can now choose the table types you want to show in the autocompletion, you are also able to filter the tables extracted by names (this allows you to, for instance, show the _FILE table and its fields in the autocompletion) - the option can be found in the set environment page, you can filter the tables that will be fetched for the autocompletion by their table type (`T,S` with T : User Data Table, S : Virtual System Table, V : SQL View) and their name (`_Sequence,_FILE,_INDEX,_FIELD,!_*,*` for instance to fetch all the user tables and a few interesting system tables)
- #187 : Automatically create aliases for the database and show the aliases in the autocompletion - the option can be found in the set environment page, you have to set a list of `ALIAS,DATABASE;ALIAS2,DATABASE;...`

**Fixed issues :**

- #192 : Problem when duplicating an environment, then deleting some databases
- #194 : Bug with the autocompletion after typing a number and "." on an empty document
- Fixed a problem with the option "insert suggestion on word end" for numbers on non progress documents
- #200 : Exception raised when the userDefineLang.xml file is not found
- #201 : Indentation problems for ELSE DO: blocks
- #202 : Indentation improvement for preprocessed &IF &ENDIF blocks
- #211 : Impossible to check syntax with openedge v9.1
