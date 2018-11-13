# Deploy your application #

*<small>Last update of this page : 13/08/2016</small>*

## Goal ##

The goal of this feature is pretty straightforward : define a set of [rules](#/deployment-rules) and in one click on a button 3P will automatically :

- Compile **all your progress files** at lightning speed : the compilation is multi-process based and is **A LOT** faster than the built-in application compiler (x16 on my computer),
- Copy/move any file (including the generated *.r code) at a chosen location,
- Add them to progress libraries (.pl) or .zip files,
- Send them on a distant server over FTP

The rules are easy to set up but powerful enough to let you create the perfect package for your application.

## Get the general idea ##

For several reasons, I've splitted the deployment in different **steps** :

- **Step 0** :
	- *COMPILE* : 3P list the files in a chosen directory (usually your source directory) then compiles all the progress files (*.p, *.w, *.cls, *.t) that match the filters that you set for this step (we will see that later on)
    - *DEPLOY R-CODE* : following a list of transfer rules that you define (to be discussed later), 3P moves/copies/zip... your r-code where you want them
    
- **Step 1** : r-codes are ready, time to deploy the other types of files! It can be any file that pass the filters you define, 3P then applies the transfer rules of step 1 to know what do to with each file. *The step 1 is made to transfer files from your source directory to your deployment directory*

- **Step 2** : During this step, the files will be transfered from the deployment directory to the deployment (yes you read correctly), you will see the point of this step if you continue reading this documentation :)

- **Step X** : you can potentially define as many steps as you want (that would be the same as step 2), however I think step 2 should be enough in most cases

## Graphics speak louder than words ##

The deployment presented below is made to illustrate all the possibilities offered by 3P :

![image](documents/deployment.png)

Here are the **filters rules** used for this deployment :

![image](documents/filter_rules.png)

And below are the **transfer rules** that drive this deployment :

![image](documents/transfer_rules.png)

## How to ##

First of all, go to the deployment interface via the main window : `ACTIONS` > `DEPLOY YOUR APPLICATION` :

![image](content_images/2016-08-09_183805.png)

In this interface, you will be able to define several options for the deployment and save them as profiles.

You can also modify and view the rules that will apply to your deployment, see the [rules](#/deployment-rules) page for more information on this regard.

## Deployment report ##

When the deployment ends, you get a report that you can export to html and that summarizes the actions done.

Any errors are also reported here!

![image](content_images/deploy_report.png)


## Notes about the compilation ##

In order to be **that** fast, 3P starts several processes of the Progress application (Prowin.exe / Prowin32.exe). It starts *X* processes for each core on your computer (an option that you can set in the page).

This implies that if you are using a connected database to compile your programs, the database must be able to accept as many connections as the number of processes started!

It also implies that if you are connected to said database in single-user mono, 3P will start the `MASS COMPILATION` in a single process and you will lose all the benefits of this feature!

*Hint : by default, a database is started to handle 20 users, you can increase this number by adding the following options to your `proserve` command : `-n 80` where 80 is the max number of users*

## A complete example ##

Here is the deployment profile used :

![image](content_images/deploy/deploy_application_screen.png)

The goal is to deploy the application in a distant folder and put some files on an FTP server like illustrated below :

![image](content_images/deploy/folders.png)

In the `DEPLOY YOUR APPLICATION` interface, I clicked on the button `View rules for this environment`

![image](content_images/deploy/rules_screen.png)

For the record, here is the configuration file used :

![image](content_images/deploy/deployment_rules.png)

And [here is a link to the report generated for this deployment](content_images/deploy/report/index.html)
