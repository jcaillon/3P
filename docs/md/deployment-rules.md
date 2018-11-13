# Deployment rules #

*<small>Last update of this page : 13/08/2016</small>*

## Pre-requisite ##

To understand the poinf of the **deployment** rules, you must first read the page on the [the deployment here](#/deployment).

## When are those rules applied? ##

The **deployment rules** are applied each time you compile a progress file and during a deployment. 

*Yes, the rules are applied when you compile a single file with `CTRL+F1`! See [the deployment usage examples page](#/deployment-usage-examples) for more information*

## How to define a rule ##

Go to the `SET` > `DEPLOYMENT` interface and follow the instructions. At the moment, a text file holds the rules (one rule per line), a user-friendly interface might come later.

### Two types of rule ### 

Two type of rules exist :

- Transfer rules : they define when / where / how a file should be deployed
- Filter rules : they define which files are eligible to the deployment

### Composition of a filter rule ### 

Each transfer rule as 5 components :

- The deployment step : integer (a rule is always defined for a particular step)
- The application name filter : If the application name of your current environment matches this filter (you can use wildcards), the rule can apply 
- The application suffix filter : If the application suffix of your current environment matches this filter (you can use wildcards), the rule can apply 
- Rule type : `+` / `-` (or `Include` / `Exclude`) decide if the files matching the *source path pattern* below are included or excluded from the deployment
- The source path pattern : when deploying, if a file matches this pattern (you can use wildcards), the rule can apply

### Composition of a transfer rule ### 

Each transfer rule as 7 components :

- The deployment step : integer (a rule is always defined for a particular step)
- The application name filter : If the application name of your current environment matches this filter (you can use wildcards), the rule can apply 
- The application suffix filter : If the application suffix of your current environment matches this filter (you can use wildcards), the rule can apply 
- The deployment type : `Move` / `Copy` / `Prolib` (the file will be added to a progress library .pl) / `Ftp` (the file will be sent to an ftp server) / `Ftp` (see next § for more details)
- Execute further rules : `yes` / `no` : yes if more rules can be applied after this one, no to stop at this rule
- The source path pattern : when deploying, if a file matches this pattern (you can use wildcards), the rule can apply
- The deployment target : It can either be an absolute path or a relative one; If relative, it will be relative to the deployment base directory set for your current environment

### Type of transfer rules ### 

- Move
- Copy
- Prolib : the deployment target must then contain a .pl file, you can adopt the syntax `file.pl\mysubfolder\` to put the file into a special path inside the .pl
- Zip : the deployment target must then contain a .zip file, you can adopt the syntax `file.zip\mysubfolder\` to put the file into a special path inside the .pl
- Ftp : the deployment target must follow the syntax `ftp://username:password@server:port/distant/path/` with username, password and port being optionnal; `/distant/path/` represents the path on the ftp server on which to put the deployed file


### Rules of the rules ###

The following rules are applied during a deployment, work around them to get exactly what you need :

**Rules sorting (from most important to less important) :**

- exact application name first
- longer application name filter first
- exact application suffix first
- longer application suffix filter first
- rules with *execute further rules* = `yes` first
- `Prolib` before `Zip` before `Ftp` before `Copy` before `Move`
- rules defined first, first (line number in the file)

**Other rules :**

- A file can have several rules applied to it; however, the first `Move` rule encountered will be the last rule applied
- When no filter rules are defined then all the files are considered (this is only true if there are NO filter rules AT ALL for a given step)
- For step 0, if no transfer rules can be applied to a file, then the file will be `Moved` to the deployment base directory by default
- For other steps, if no transfer rules apply then the file is not transfered at all
- For step 0, if the environment is set to `compile next to source` then the *.r will be moved next to the source and no transfer rules will apply