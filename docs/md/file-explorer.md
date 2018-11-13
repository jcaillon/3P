# The file explorer #

*<small>Last update of this page : 29/02/2016</small>*

* The file explorer has several purposes, the first one is to show the user the errors on a specific file after a compilation, a check syntax or a prolint :

![image](https://raw.githubusercontent.com/jcaillon/3P/gh-pages/content_images/file_explorer_filestatus.png)

* The user can quickly navigate to the next/previous error, it can also clear all the error (this only has a visual effect obviously; 3P doesn't code for you... yet)

* The second purpose is to ease the search of files through your local repository or your propath. The search feature uses the same algorithm than the auto-completion : no need for * or ., just type something close to the file name your are looking for and voilà (hopefully!)

* As always, use the tool-tips on each button to learn more about the file explorer. For example, you can filter the type of files you are searching in the same way you would filter the keywords in the auto-completion list!

* A final note : the list of files will stay empty if you don't set your propath or your project local directory in the `SET` > `ENVIRONMENT` page
