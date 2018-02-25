## Build info : ##

### How to configure DLLExport ###

- Execute `DllExport.bat -action Configure`
- This download packages\DllExport.1.6.0
- Select your .sln solution file
- Choose a namespace for the DllExport : `RGiesecke.DllExport` and target x86 + x64
- Click apply
- Read all the info here : https://github.com/3F/DllExport
- If missing a lib/exe when building, install [MS BUILD TOOL 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159) / [SDK .net 4.6.2](https://www.microsoft.com/en-us/download/details.aspx?id=53321) / [win 10 SDK](https://developer.microsoft.com/windows/downloads/windows-10-sdk)


## Update Notepad++ plugin manager ##

- For the x86 version, it happens here : https://npppm.bruderste.in/
- For the x64 version, at the moment, it happens here : https://github.com/bruderstein/npp-plugins-x64


## When releasing (Note to myself) : ##

### Manual chores ###

- Change the dll version of 3P in AssemblyInfo.cs, the format should be vX.X.X.X with v(major).(minor).(revision).(lastdigit) ; *the last digit of the dll's version indicates if it's a pre-release build (1) or stable build (0)*
- Check the default Config + UserCommunication.Notify()
- Create a new tag : `git tag vX.X.X` (the last digit must be left empty for the tag name!)
- Push the tag `git push --tags` : this will trigger the build with the deployment on the appveyor
- Wait for the build to be done, edit the newly created https://github.com/jcaillon/3P/releases
  - don't forget to check/uncheck the prerelease option
  - Find a cool title
  - past the body from the `NEXT_RELEASE_NOTES.md` file
  - Publish the release!

### Done automatically by appveyor ###

- Clean/Rebuild in release mode, not debug for both x86 and x64 versions
- Create 2 .zip in the release on github :
  - "3P.zip" containing the 3P.dll (32 bits) and eventually the .pdb file
  - "3P_x64.zip" containg the 3P.dll (64 bits!) and eventually the .pdb file
