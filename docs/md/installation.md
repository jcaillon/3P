# Installation #

*<small>Last update of this page : 06/02/2019</small>*

## Install Notepad++ ##

**3P** is a notepad++ plug-in, hence the first step is to download and install the latest version notepad++ at :
[notepad-plus-plus.org/download/](https://notepad-plus-plus.org/download/).

## Install the required .net framework version ##

3P is developped in C# with the [.net framework](https://docs.microsoft.com/en-us/dotnet/framework/get-started/overview), you will need the [4.6.1 version](http://go.microsoft.com/fwlink/p/?LinkId=671744) or superior otherwise it will simply not work!

If you don't know which version you currently have, no worries, just follow the next step. I have included a small program (NetFrameworkChecker.exe [opensource here](https://github.com/jcaillon/NetFrameworkChecker)) that can check if you fullfill the requirement.

## Installation of 3P ##

### Automatic with the plugin manager ###

Automatically install 3P through the plugin manager of notepad++:

* Menu `Plugins` then click on `Plugin Admin...`.

![image](content_images/installation/plugin_admins.png)

* Look for **3P** in the available list of plugins, check it and press install.

![image](content_images/installation/install_3P.png)

* A confirmation is asked to allow to exit notepad++.
* The program `NetFrameworkChecker.exe` will be started to check that you have the required .net version (it will not even show if you have the required version).

### Manual installation of 3P ###

* Stop notepad++.
* Download the [latest version](https://github.com/jcaillon/3P/releases/latest) of 3P (direct download button available on the menu on your right).
* Go to your notepad++ installation folder (usually %programfiles%\Notepad++), you should see a `/plugins/` folder.
* Create a new subfolder `3P` and unzip the content of the downloaded package into it: `/plugins/3P/3P.dll` should now exist. *Note: for older notepad++ version <= v7.6, unzip the content directly to the plugins folder*.
* Optionally, you can execute `NetFrameworkChecker.exe` to check if you have the required .net version to run 3P.
* Start notepad++.

## <font color='#CD7918'>Troubleshooting</font> ##

When starting notepad++, you have this kind of message:

> Failed to load.
>
> 3P.dll is not valid notepad++ plugin.

or

> Cannot load plugin.
>
> 3P.dll is not compatible with the current version of notepad++.

Solutions:

1. First, check that you have the right version of 3P depending on your version of notepad++ (i.e. 3P 64bits if you have notepad++ 64bits or x86 version otherwise).
2. Secondly, make sure you have the minimal required version of .net framework installed. For that, execute `NetFrameworkChecker.exe` which is provided in the 3P.zip file.
