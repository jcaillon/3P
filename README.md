# 3P: Progress Programmers Pal #

![State](https://img.shields.io/badge/state-work%20in%20progress-red.svg?style=plastic)
[![License](https://img.shields.io/badge/license-GPLv3-blue.svg?style=plastic)](../master/COPYING.GPLv3.txt)
[![Download](https://img.shields.io/badge/download-no%20version%20available-lightgrey.svg?style=plastic)](../master/COPYING.GPLv3.txt)

**BE WARNED** : As of today, this project is still a work in progress and a first release has not been created yet, this page is only a draft

## Content ##

+ [About this project](#about-this-project)
+ [Quick start](#quick-start)
    - [Installation](#installation)
    - [First use](#first-use)
+ [3P's showcase](#3p-showcase)
+ [A final word](#a-final-word)

***

## About this project ##

The **OpenEdge** **A**dvanced **B**usiness **L**anguage, or [OpenEdge ABL](https://www.progress.com/openedge) (formerly known as **Progress 4GL**) is a fourth-generation programming language which uses an English-like syntax. Applications developped with this langage are portable across computing systems, it uses its own integrated relational database and programming tool which is called the "appbuilder".

Progress Programmers Pal (3P), is a **[notepad++](https://notepad-plus-plus.org/ "Notepad++ home page")** plugin designed to help writing **ABL code**,  its use is governed by [GPL License](http://www.gnu.org/copyleft/gpl.html).

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

### First use ###

Once you installed the plugin you will be asked to restart notepad++ once again to activate the syntax highlighting, then you are ready to use 3P.

> wiki incoming...



***

## 3P's showcase  ##

This paragraph briefly presents the main features of 3P.

### Overview ###

Ths plugin will display two new side panels (on left and right) as shown below :

![image](https://cloud.githubusercontent.com/assets/11553075/11215041/8e342adc-8d44-11e5-9c3e-fef920076f46.png)

### Syntax highlighting ###

3P has an embedded syntax highligher, several themes are available and switchable on the fly :

![image](https://cloud.githubusercontent.com/assets/11553075/11215274/a84d092e-8d45-11e5-87c6-830d40460e14.png)

### Autocompletion ###

A key feature of 3P is the built in autocompletion window. Fully integrated with notepad++, it smoothes the developper work by suggesting the best match according to his intention :

![image](https://cloud.githubusercontent.com/assets/11553075/11215781/419a86a4-8d48-11e5-9155-c062659551dd.png)

*[Learn more about this feature here](../master/wiki/autocompletion.md)*

### ToolTips ###

Tooltips are another important part of 3P, they provide information on a word. You can activate them by simply hovering a word with your cursor : 

![image](https://cloud.githubusercontent.com/assets/11553075/11218206/3b6b3e8e-8d54-11e5-8162-297dcb0f4c5c.png)

*[Learn more about this feature here](../master/wiki/tooltips.md)*

### Code explorer ###

The code explorer is your best friend when it comes to... exploring the code. 

> *thanks captain!*

It acts as an improved `function list`, well known to the notepad++ users. It displays the structure of the program and provides a quick way to jump from a code's portion to another (left click an item to get redirected to it) :

![image](https://cloud.githubusercontent.com/assets/11553075/11218752/256db7f8-8d57-11e5-9924-93fa3d87e83e.png)

*[Learn more about this feature here](../master/wiki/code_explorer.md)*


***

## A final word ##

One thing the appbuilder has that 3P doesn't, is the [graphical interface](https://documentation.progress.com/output/ua/OpenEdge_latest/index.html#page/gsstu/overview-of-the-openedge-appbuilder.html) to modify your .w files. You can do it by code, obviously, but i admit that it's not a simple task.

Personally, i use 3P as a **complement** to the app builder, designing interfaces with the graphical tool and switching to notepad++ to modify the core behavior.
