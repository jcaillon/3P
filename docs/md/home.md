# About this project #

*<small>Last update of this page : 03/02/2019</small>*

The **OpenEdge** **A**dvanced **B**usiness **L**anguage, or [OpenEdge ABL](https://www.progress.com/openedge) (formerly known as **Progress 4GL**) is a fourth-generation programming language which uses an English-like syntax. Applications developed with this language are portable across computing systems, it uses its own integrated relational database and programming tool which is called the "appbuilder".

Progress Programmers Pal (3P), is a **[notepad++](https://notepad-plus-plus.org/ "Notepad++ home page")** plug-in designed to help **Openedge ABL** developpers, 3P is a **free and open-source** software release under [GPL License](http://www.gnu.org/copyleft/gpl.html).

3P transforms notepad++ into an ABL code editor, providing :

* a powerful auto-completion
* tool-tips on every words
* a code explorer to quickly navigate through your code
* a file explorer to easily access all your sources
* more than 50 options to better suit your needs
* the ability to run/compile and even **[PROLINT](http://www.oehive.org/book/export/html/223)** your source file with an in-line visualization of errors
* and so much more!

If you are not fond of the appbuilder and looking for a fast and efficient **ABL editor**, look no further!


# Newest features (beta version) #

## An alternative to notepad++ autocompletion ##

In addition to the features targeting the openedge users, 3P now also include a tool for a wider public : It can completely replace the default autocompletion of notepad++ (more precisely scintilla) by the one used in 3P!

![image](content_images/gif/npp-autocompletion-ex2.gif)

*[Learn more about this feature here](#/alternative-autocompletion)*

## Prolint made easy ##

Prolint is a tool for automated source code review of Progress 4GL code. It reads one or more sourcefiles and examines it for bad programming practices or violations against coding standards.

3P will now automatically download and install an instance of prolint for you : just press F12 and get your file prolint'ed; it is that simple!

# Key features  #

This paragraph briefly presents the main features of 3P (the list is not exhaustive).


## Overview ##

The screenshot below gives you a quick overview of 3P's features :

![image](content_images/home/overview.png)

## Main menu ##

Access the main menu of 3P through the top bar icon or with the shortcut `CTRL+Right click` :

![image](content_images/gif/main-menu.gif)

## Auto-completion ##

A key feature of 3P is the built in auto-completion window. Fully integrated with notepad++, it smooths the developer work by suggesting the best match according to his intention :

![image](content_images/gif/auto-comp_demo.gif)

*[Learn more about this feature here](#/autocompletion)*


## Tool-Tips ##

Tool-tips are another important part of 3P, they provide information on a word. You can activate them by simply hovering a word with your cursor :

![image](content_images/home/tooltips.png)

*[Learn more about this feature here](#/tooltips)*


## Code explorer ##

The code explorer is your best friend when it comes to... exploring the code.

> *thanks captain!*

It acts as an improved `function list`, well known to the notepad++ users. It displays the structure of the program and provides a quick way to jump from a code's portion to another (left click an item to get redirected to it) :

![image](content_images/gif/code-explorer.gif)

*[Learn more about this feature here](#/code-explorer)*


## File explorer ##

You can easily browse the files of your local directory and/or propath with the file explorer. A powerful search tool has been added to make sure you can quickly find the file you are looking for :

![image](content_images/home/file_explorer.png)

*[Learn more about this feature here](#/file-explorer)*


## Check syntax and visualize errors ##

With 3P, you can visualize the **compilation errors** (or PROLINT errors like in the screenshot below) directly into notepad++ :

![image](content_images/home/compilation_errors.png)


## Adaptability ##

3P has tons of options to help you in your work and to better suit your project needs. Beside all the *classic* options that could be expected from a code editor, 3P also has *4GL progress* oriented options ; like the ability to define several environments, to choose the way prowin is launched, the way database are connected and so on... Below is one of the option page of the application :

![image](content_images/home/set_environmment.png)

*[Learn how to set an environment here](#/set-environment)*


## Deploy your application ##


### Lightning fast compilation ###


Do you have a large application with a lot of programs that you often need to recompile? 3P uses a multi-process compilation that makes it **A LOT** faster than the built-in application compiler (like x16 faster, or more!). You can compile an entire directory recursively (with filter options!) in one click and get a nice and interactive report to quickly fix the errors.


### Powerful deployment tools ###

Do you need to send your r-code to a distant ftp server after compiling them? Do you need to .zip all your configuration files or package r-code in progress libraries (.pl)? Set your deployment rules and 3P can automatically do all of this for you!

![image](content_images/home/deploy_application.png)

*[Learn more about deploying your application here](#/deployment)*


## Syntax highlighting ##

3P has an embedded syntax highlighter, several themes are available and switchable at will :

![image](content_images/home/syntax_themes.png)


## Data digger ##

3P offers to set up data digger for you just by clicking on a buton; you will then be able to browse through your database data easily and efficiently!

[More info on datadigger here](https://datadigger.wordpress.com/)


## And more!? ##

See what 3P can bring you by *[installing](#/installation)* it! It only takes a few seconds ;)

You can also browse this website through the navigation menu on your right to check out more features of 3P.

***


# A final word #

One thing the AppBuilder has that 3P doesn't, is the [graphical interface](https://documentation.progress.com/output/ua/OpenEdge_latest/index.html#page/gsstu/overview-of-the-openedge-appbuilder.html) to modify your .w files. You can do it by code, obviously, but i admit that it's not a simple task.

Personally, i use 3P as a **complement** to the AppBuilder, designing interfaces with the graphical tool and switching to notepad++ to modify the core behavior.

Also, at the moment, 3P does not parse .cls files. But don't worry, this feature will come in a (near) future release.
