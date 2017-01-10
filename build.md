## Build info : ##

**To be able to compile this project you need to**

- You might have a missing librairy/exe when compiling, install the [SDK for .net 4.0](https://www.microsoft.com/en-us/download/details.aspx?id=8279)
- Your system envVar path must be extended to include ILDasm.exe directory (e.g. "c:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools")

## When releasing (Note to myself) : ##

- be sure to match the dll version with the tag name of the release in GitHub (btw the format should be vX.X.X where v(major).(minor).(revision), last digit must be left empty on GitHub tag name, see below)
- the last digit of the dll's version indicates if it's a pre-release build (1) or stable build (0)
- Check the default Config
- Recompile the updater if needed!!
- Make sure to leave at least 1 empty line above bullet lists in the GitHub release description!
- Clean/Rebuild in release mode, not debug
- Create 2 .zip in the release on github :
  - "3P.zip" containing the 3P.dll (32 bits) and eventually the .pdb file
  - "3P_x64.zip" containg the 3P.dll (64 bits!) and eventually the .pdf file
- When creating a stable release, target the master branch and don't forget to do a pull request from beta > master
