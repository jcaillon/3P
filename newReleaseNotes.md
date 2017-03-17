Enjoy!

Config.IsDevelopper in DocumentLines!

**Improvements :**

- 3P now correctly reads the Npp configuration from the cloud folder (if any)
- Updated [DataDigger](https://datadigger.wordpress.com/2017/02/20/20170220/) to version 22, thanks [Patrick](https://github.com/patrickTingen/DataDigger)!
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

- Fixed an issue with the type buttons of the file explorer that would the tooltips of the autocompetion type buttons
- Replaced the old mechanism to hide the progress splashscreen during a prowin execution, now uses `-nosplash`!
- 3P no longers catches errors from other .net plugins (like CSScriptNpp)
- Correction of various bugs related to document encoding

