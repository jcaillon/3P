## Build info : ##

**To be able to compile this project you need to**

- Unzip `3P\3PA\Lib\3pUpdater\3pUpdater.7z` in its folder
- Open and build the solution `3P\3PA\Lib\3pUpdater\3pUpdater.sln`
- Unzip `3P\3PA\Interop\DllExport\DllExport.7z` in its folder
- Your system envVar path must be extended to include ILDasm.exe directory (e.g. "c:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools")
- You might also have a missing librairy/exe when compiling, i invite you to google it :) fyi, it is used here : `LibToolPath="$(DevEnvDir)\..\..\VC\bin"` in `3P\3PA\Interop\DllExport\NppPlugin.DllExport.targets`


## When releasing (Note to myself) : ##

- be sure to match the dll version with the tag name of the release in GitHub (btw the format should be vX.X.X where v(major).(minor).(revision), last digit must be left empty on GitHub tag name, see below)
- the last digit of the dll's version indicates if it's a pre-release build (1) or stable build (0)
- Check the default Config
- Recompile the updater if needed!
- Make to leave at least 1 empty line above bullet lists in the GitHub release description!
