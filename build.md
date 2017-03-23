## Build info : ##

**To be able to compile this project you need to**

- You might have a missing librairy/exe when compiling, install the [SDK for .net 4.0](https://www.microsoft.com/en-us/download/details.aspx?id=8279)
- Your system envVar path must be extended to include ILDasm.exe directory (e.g. "c:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools")
- (If everything else fails, install [this](https://developer.microsoft.com/en-us/windows/downloads/windows-8-sdk))

## When releasing (Note to myself) : ##

**Manual chores**

- Change the dll version of 3P in AssemblyInfo.cs, the format should be vX.X.X.X with v(major).(minor).(revision).(lastdigit) ; *the last digit of the dll's version indicates if it's a pre-release build (1) or stable build (0)*
- Check the default Config
- Recompile the updater if needed
- Create a new tag : `git tag vX.X.X` (the last digit must be left empty for the tag name!)
- Push the tag `git push --tags` : this will trigger the build with the deployment on the appveyor
- Wait for the build to be done, edit the newly created https://github.com/jcaillon/3P/releases
  - don't forget to check/uncheck the prerelease option
  - Find a cool title
  - verify the 2 .zip files
  - Publish the release!

**Done automatically by appveyor**

- Clean/Rebuild in release mode, not debug for both x86 and x64 versions
- Create 2 .zip in the release on github :
  - "3P.zip" containing the 3P.dll (32 bits) and eventually the .pdb file
  - "3P_x64.zip" containg the 3P.dll (64 bits!) and eventually the .pdb file
