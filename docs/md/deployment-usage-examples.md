# Usage examples of the deployment feature #

*<small>Last update of this page : 13/08/2016</small>*

## Send a file to a distant FTP server in one click ##

**The situation**

Let's say that you are working on several ABL programs (.p) and several configuration files (.xml for instance).

When you modify one of those files, you want to automatically send them to an FTP server (for instance an UNIX server hosting your OpenEdge application).

**The solution**

You will need to create 2 rules in 3P to each this goal. Go to `SET` > `DEPLOYMENT RULES` and click on the modify button. The configuration file is opened in Notepad++, add those two lines :

![image](content_images/deploy/example1.png)

You can now modify your file and select `DEPLOY CURRENT FILE` in the main menu to mirror any local changes to the server!

![image](content_images/deploy/deploy_current_file.png)

**Explanation**

*Rule 1*

The first rule (line 94) is for the step 0 (the compilation step) and concerns every application/suffix (the asterisk is like the joker character on windows). It's an FTP rule, we stipulate that we don't want to apply another rule after this one (no) and that the rule should be applied for everyfile (asterisk on source pattern).

This rule will transfer every progress file that we compile (our .p) onto the FTP server 127.0.0.1 under the tree /myfolder/deploy/ (creating the folders if needed).

*Rule 2*

The second rule (line 95) is exactly the same, except that it concerns the step 1 of the deployment. It will be applied when you click `DEPLOY CURRENT FILE` if you are not on a compilable file. This will allo you to push your .xml files to the server (for instance).