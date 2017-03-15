DataDigger
==========
Welcome to DataDigger, a tool to manage the data in your databases.

With DataDigger you can:
  - Look at the tables in your database
  - Display the data in those tables
  - Select which fields to display
  - Easily dump your data to standard dump files, xml or excel
  - Load data from .d files and xml using an advanced wizard
  - Edit one or more records at the same time

In addition, DataDigger was designed to have an attractive and pleasant user interface.
Almost all actions can be performed by either mouse or keyboard.

DataDigger saves your personal preferences in a settings file so you need to define them once.


HOW TO INSTALL
==============
Installing DataDigger is just as simple as 1-2-3:

1. Create a directory called DataDigger
2. Extract the zipfile in this dir
3. Create a shortcut to prowin32.exe, and use -basekey "INI" -p DataDigger.p
   (set the "start in" path to the one you used in step 2)

Ready you are.

In addition you might like to add one or more of these:
  -s 1000000         Increase the amount of memory (see also MaxColumns)
  -param "My Title"  Title for your DataDigger window. Handy if you have more than one shortcut.
  -pf filename.pf    Use the pf file you use for your normal development (or production) session. DataDigger recognizes db's when they are connected at startup
  -rereadnolock      Force Progress to re-read the record from the database, even if the record is already in another active record buffer.
  -h 100             The maximum number of databases that can be connected during an OpenEdge session. Default value is 5.


FEEDBACK
========
If you have trouble installing DataDigger, want to report a bug or request a feature, feel free to contact me at:
patrick@tingen.net
