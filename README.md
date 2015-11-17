# 3P: Progress Programmers Pal #

![State](https://img.shields.io/badge/state-work%20in%20progress-red.svg?style=plastic)
[![License](https://img.shields.io/badge/license-GPLv3-blue.svg?style=plastic)](../master/COPYING.GPLv3.txt)

**BE WARNED** : As of today, this project is still a work in progress and a first release has not been created yet, this page is only a draft



***

## About this project ##

The **OpenEdge** **A**dvanced **B**usiness **L**anguage, or [OpenEdge ABL](https://www.progress.com/openedge) (formerly known as **Progress 4GL**) is a fourth-generation programming language which uses an English-like syntax. Applications developped with this langage are portable across computing systems, it uses its own integrated relational database and programming tool which is called the "appbuilder".

Progress Programmers Pal (3P), is a **notepad++** plugin designed to help writing **ABL code**,  its use is governed by [GPL License](http://www.gnu.org/copyleft/gpl.html).

3P transforms notepad++ into an ABL code editor, providing :

* synthax checking
* compilation
* autocompletion
* tooltips 
* and much more!

If you are not fond of the appbuilder and looking for a fast and efficient **ABL editor**, look no further!



***

## Quick start ##

### Installation ###

There are two **very easy** ways of installing 3P :

1. The first way is through the plugin manager, go to : 
    * `PLUGINS > PLUGIN MANAGER > SHOW PLUGIN MANAGER`
    * Look for **3P** in the available plugins list and click on the checkbox
    * Press install
    * restart notepad++

2. The second way is to install the plugin manually, which is also easy since it comes as a single .dll file
    * Download the latest version of the file
    * `SETTINGS > IMPORT > IMPORT PLUGIN(S)`
    * Select the downloaded .dll file
    * restart notepad++

### How to ###

Once you installed the plugin you will be asked to restart notepad++ once again to activate the syntax highlighting, then you are ready to use 3P.

> wiki incoming...



***

## Key features  ##

The paragraph below is only a 'showcase' of key features brought by 3P and does not represent a complete list of its benefits.

### Overview ###

Ths plugin will display two new side panels (on left and right) as shown below :

![image](https://cloud.githubusercontent.com/assets/11553075/11215041/8e342adc-8d44-11e5-9c3e-fef920076f46.png)

### Syntax highlighting ###

3P has an embedded syntax highligher, several themes are available and switchable on the fly :

![image](https://cloud.githubusercontent.com/assets/11553075/11215274/a84d092e-8d45-11e5-87c6-830d40460e14.png)

### Autocompletion ###

* A key feature of 3P is the built in autocompletion window. Fully integrated with notepad++, it smoothes the developper work by suggesting the best match according to his intention :

![image](https://cloud.githubusercontent.com/assets/11553075/11215781/419a86a4-8d48-11e5-9155-c062659551dd.png)

* The embedded dictionnary of ABL keywords contains more than 2800 words which should cover your entire needs (you can even add new words if some are missings) :

![image](https://cloud.githubusercontent.com/assets/11553075/11216157/136dc9d8-8d4a-11e5-9775-2abdc5b77d33.png)

* The more you use a keyword, the higher it will appear in the list

* The list doesn't exclusivly include *static* words ; a built in ABL parser analyses the code on the fly and pushes words like : variables, procedures, functions, temp-tables and so on, into the autocompletion list. Moreover, if a program contains an include call `{myinclude.i}` it will also analyse the included file.

* It only displays elements that are relevant at the cursor position. For instance, a variable defined in a local procedure will not show up in another procedure ; or a variable will not show up in a line where it is not yet known by the compiler :

![image](https://cloud.githubusercontent.com/assets/11553075/11218038/8c49e586-8d53-11e5-884e-736cac8892a7.png)

* it sorts the keywords into categories (more than 15) to easily find your keyword. You can activate/deactivate a category with a simple click :

![image](https://cloud.githubusercontent.com/assets/11553075/11217991/5ca40abe-8d53-11e5-99ec-66ea7a06187d.png)

* if you configure a connection to a progress database, it provides the autocompletion on `TABLE` names as well as `FIELD` names. Enter `mydatabasename` followed by a dot `.` and a list of tables of this database will show up, a dot `.` after a table name will show up its fields (it also differentiates fields that are part of the primary key) :

![image](https://cloud.githubusercontent.com/assets/11553075/11216639/85bdaccc-8d4c-11e5-9caa-6ff3a24d5b72.png)

> This also works with buffers so... use them!

* A keyword can have one or several flags (and eventually a small text) that quickly provides extra information on said word. In the example below, `g_codeset` is a `SHARED` variable, defined in the `PROGRAM SCOPE` (i.e. available to all procedures and functions) and was found in an `INCLUDE` file:

![image](https://cloud.githubusercontent.com/assets/11553075/11216736/208108f8-8d4d-11e5-944f-a2267c7c0c34.png)

* When using the `UP` or `DOWN` arrow keys, you can navigate through the list. It also shows a tooltip next to the autocompletion that provides more information on the keyword selected, this tooltip can be used to understand the flags described in the above section!

![image](https://cloud.githubusercontent.com/assets/11553075/11216436/70493fba-8d4b-11e5-9822-3089c62be2de.png)

* Extra tips :
    - Thanks to the way keywords are sorted, you can find quick ways to type a word. For instance, the fastest way to type `ERROR-STATUS` can be typing `errsta` and pressing tab to accept the suggestion
    - By holding the `ALT` key in addition to `LEFT` or `RIGHT` arrow key you can quickly filter the keywords by categories while keeping your two hands on the keyboard
    - Through the option window, you can set a mode that automatically inserts the best match when you end a word (a space, a dot or any operator can end a word). This is the fastest way to write ABL code!
    - A tons of other options are available, go check them out!
  
### ToolTips ###

* Tooltips are another important part of 3P, they quickly provide information on a word (as long as it is known by the software). You can activate them by simply hovering a word with your cursor : 

![image](https://cloud.githubusercontent.com/assets/11553075/11218206/3b6b3e8e-8d54-11e5-8162-297dcb0f4c5c.png)

* Each type of word has its own way of displaying information. For instance, hovering a statement keyword will show you its syntax :

![image](https://cloud.githubusercontent.com/assets/11553075/11218361/efc50de2-8d54-11e5-8181-f63a60eff0b8.png)

* Sometimes, several definitions are available for a word. For instance, the word `ASSIGN` is an ABL *statement*, but is also a *method*. You can loop through the various definitions by holding the `CTRL` key and pressing the `DOWN` arrow key (also works with `UP`)

* In another case, you might want to keep the tooltip open when you move your mouse ; in order, for example, to click the `Go to definition` link of a procedure's tooltip. You might want to press the `CTRL` key to do just that

### Code explorer ###

* The code explorer is your best friend when it comes to... exploring the code. 

> *thanks captain!*

* Jokes aside, it acts as an improved `function list`, well known to the notepad++ users. It displays the structure of the program and provides a quick way to jump from a code's portion to another (left click an item to get redirected to it) :

![image](https://cloud.githubusercontent.com/assets/11553075/11218752/256db7f8-8d57-11e5-9924-93fa3d87e83e.png)

* The code explorer provides way more information than your usual function list ; and it can sometimes be overwhelming. You can reduce the amount of information at any time by expanding/collapsing one or all categories :

![image](https://cloud.githubusercontent.com/assets/11553075/11219013/5771f858-8d58-11e5-85f8-1b4e343da4b4.png)

* You can also filter the items by typing a word in the textbox available for this end :

![image](https://cloud.githubusercontent.com/assets/11553075/11219059/8e34ca6e-8d58-11e5-9ffb-e954cb291fcc.png)


***

## Drawbacks of using 3P over the appbuilder ##

One thing the appbuilder has that 3P doesn't, is the [graphical interface](https://documentation.progress.com/output/ua/OpenEdge_latest/index.html#page/gsstu/overview-of-the-openedge-appbuilder.html) to modify your .w files. You can do it by code, obviously, but i admit that it's not a simple task.

Personally, i use 3P as a **complement** to the app builder, designing interfaces with the graphical tool and switching to notepad++ to modify the core behavior.
