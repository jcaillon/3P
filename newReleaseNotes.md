Enjoy!


**Improvements :**

- New options for each environment :
  - You can now choose a progress program that will be executed before each progress execution (like a compilation for instance); this will be useful for users that prefer to dynamically connect databases instead of using a .pf or extra connection info
  - You can also choose a progress program executer *after* any execution
- New compilation options (available through a new item in the main menu) : 
  - Generate debug-list
  - Generate xref file
  - Generate xref xml file
  - Generate listing file
  - You can also choose to immediatly generate + show one of this files in the new *Progress tools* menu
- New feature : **Correct code indentation**
- New item in the main menu **Display parser errors** : this will validate your file using the parser of 3P, it can be used to know if your file can be read by the AppBuilder (for instance). You also have the opportunity to display the parser errors each time you save the file (it will only be displayed if there are errors), see `Options` > `Code edition`. This is enabled by default. Since I introduced this feature, i will stop displaying the number of extra characters in the code explorer, which already displays a lot of information.
- New options for the auto completion : 
  - you can now choose to immediately show the auto completion after a `.` or `:` (this is true currently and by default)
  - you can now choose to not display the auto completion on each input but still correct the case of the words as you type
  - options are now splitted between : general options / progress specific options / default auto completion replacement options
- Updated [DataDigger](https://datadigger.wordpress.com/2017/02/20/20170220/) to the latest beta version 20170324, thanks [Patrick](https://github.com/patrickTingen/DataDigger)!
- Ftp connections now also work for servers with the active transfer mode activated
- Improvements to the parser, this should be its final form; it now parses include files exactly like the progress compiler, replacing the include call by the content of the file
- Replaced the old option "Npp openable extension" by "NppFilesPattern" to be in line with the other options of that type (it allows you to describe the files that should be opened with Npp from the file explorer)
- Improvements to the tree view
  - new option to display (or not) the branches of the tree for the file/code explorer
  - the click to collapse/expand a node now only works on the small arrow area (instead of all the node previously)
  - you can now expand/collapse the file explorer tree view as well as the main menu
- new options for the deployment :

```
You can define variables on a line with info separated by tabulations; the format is :
name	suffix	<var_name>	path
The <var_name> is then replaced in the transfer rules by the value of the "path"
See the example below for a better understanding

#name	suffix  	<var_name>    	path
##############################################
*   	*       	<myvar>     	C:\temp 

#step	name	suffix	type	next?	source_pattern	deploy_target 
#####################################################################
0  	*   	*   	Move	no  	*\client\*     	<myvar>\client\
```

**Fixed issues :**

- Replaced the old mechanism to hide the progress splashscreen during a prowin execution, now simply uses `-nosplash`
- 3P no longers catches errors from other .net plugins (like CSScriptNpp)
- Correction of various bugs related to document encoding
- Fixed an issue with the type buttons of the file explorer that would show the tooltip content of the autocompetion type buttons
- 3P now correctly reads the Npp configuration from the cloud folder (if any)
- small fixes : #76, #129