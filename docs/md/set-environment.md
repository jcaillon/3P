# How to : set an environment #

*<small>Last update of this page : 27/04/2016</small>*

This page will hopefully guide you through the process of setting an environment in 3P :

![2016-04-27_104309](content_images/home/set_environmment.png)

## General idea and goal of this interface ##

### The goal ###

If configured, 3P can do the following things :

- Check the syntax, compile or execute your code
- Browse your source files, automatically open an include file or a procedure
- auto-complete info from the database (tables, fields, sequences...)

For that, you need to provide :

- The location of the progress executable (prowin32.exe / prowin.exe for windows)
- The path to the folder of your sources
- A way to connect to your database

### General idea ###

3P let's you define several environment so you can quickly switch through them.

You can define *one or more* **application(s)** and for each of them, you can define *one or more* **suffixes** (a sub-level of an application).

![2016-04-27_105412](content_images/67c1c01a-0c67-11e6-9a5c-17fd9a2dada6.png)

For instance :
I have several clients, I develop a different application for each of them.
For each application, I have a version in development, one being tested by my client and a last one that is an exact copy of the app in production for debug purposes.

This is the idea behind those two lists

![2016-04-27_110118](content_images/76a2c854-0c67-11e6-9226-07565977a2f4.png)

## Configure a new environment ##

### 1st step : create a new env ###

Use the following buttons to modify or create a new environment : ![2016-04-27_110547](content_images/03e2fbee-0c68-11e6-85bd-933b7fa03b35.png) ![2016-04-27_110544](content_images/03dea62a-0c68-11e6-9441-fedf09950b85.png)

*Hint : you can right click the ![2016-04-27_110547](content_images/03e2fbee-0c68-11e6-85bd-933b7fa03b35.png) button to copy the current environment!*

### 2nd step : name it ###

First things first, you have to define the name of your *application*. If you need it / want it, you can also create sub items (suffixes) for this application, and give it a label to quickly identify it.

![2016-04-27_111103](content_images/bdb0fd3c-0c68-11e6-93a7-063468ca39d1.png)

### 3rd step : set the prowin32 path ###

Provide the windows path to your prowin32.exe executable. It is usually located in : `%progress_install_dir%\dlc\bin\prowin32.exe`

![2016-04-27_111239](content_images/f89ca374-0c68-11e6-9014-ff62f5f75fef.png)

### 4th step : set your local directory ###

Provide the path to the folder where your application sources are located :

![2016-04-27_111616](content_images/7904c7c6-0c69-11e6-8133-fc6ce853a459.png)

### 5th step : set your propath ###

There are two ways of setting your PROPATH :

![2016-04-27_111801](content_images/b9716f62-0c69-11e6-8977-c3b1854fae6b.png)

*No matter which of the following methods is used, the propath can be specified as relative path. In that case, the root directory is your local directory!*

#### First method ####

You can provide the path to the progress `.ini` file, that usually contains a section `[Startup]` with the key `PROPATH` :

![2016-04-27_112004](content_images/0145f7fe-0c6a-11e6-9e01-8fe15cc2d2bf.png)

#### Second method ####

Or you can directly provide of list of folders / .pl to use in your PROPATH :

![2016-04-27_112717](content_images/066fd730-0c6b-11e6-8b9d-d8592527aefd.png)

In this example, my PROPATH will be :

- D:\repo\folder1
- D:\repo\folder2
- D:\work

### 6th step : set the connection(s) to your database(s) ###

Again, two ways or configuring your database(s) connection(s) :

![2016-04-27_113446](content_images/114a4a54-0c6c-11e6-86c8-3e7eaa73047b.png)

#### First method ####

You can choose to use a parameter file `*.pf` that contains the connection parameters.
In that case, 3P lets you have a *list* of files so you can quickly switch the current .pf to use (once again, you can only have 1 item in your list!). Use the following buttons to handle this list : ![2016-04-27_113619](content_images/47624092-0c6c-11e6-8619-44262232c874.png) 

Add a new item ![2016-04-27_113800](content_images/823c5694-0c6c-11e6-813a-83f9f296ab96.png), set a name and the path to this `*.pf` file as shown below : 
![2016-04-27_113943](content_images/c19d5626-0c6c-11e6-8716-94c0866f28f9.png)

#### Second method ####

You can also choose to define the connection parameters for *one or more* databases directly in the interface :

![2016-04-27_114159](content_images/2fe8d6dc-0c6d-11e6-9302-baf211ef1efe.png)

This example will connect to two different databases.

#### Once the connection is set! ####

When your database connection is set properly, you can click this button ![2016-04-27_114415](content_images/61b7fdd2-0c6d-11e6-904f-f0bed498be02.png) to download the database structure and get the autocompletion on tables/fields/sequences!

If everything went well, the button will turn green ![2016-04-27_114529](content_images/99998860-0c6d-11e6-9e8d-c9ff02151c3a.png), meaning that the autocompletion is activated. You can click it again to update the autocompletion if you database structure is changed.


### 7th step : set your compilation folder ###

You can choose to compile your programs and move the .r file next to your source file, or move the .r to a particular location using the following toggle :
![2016-04-27_113135](content_images/a28390ee-0c6b-11e6-811e-4654f48b906d.png) ![2016-04-27_113143](content_images/a2856108-0c6b-11e6-99b8-dfba15082b98.png)

![2016-04-27_113004](content_images/690c9ef0-0c6b-11e6-9a9f-07697b2cb304.png)

*Hint : Use the compilation path plus the page `SET > COMPILATION PATH` to fully customize the way your files are compilated and choose a different compilation folder for each particular file!*

