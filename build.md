## Build info : ##

**To be able to compile this project you need to**

- You might have a missing librairy/exe when compiling, install the [SDK for .net 4.0](https://www.microsoft.com/en-us/download/details.aspx?id=8279)
- If it still doesn't work, install [this](https://developer.microsoft.com/en-us/windows/downloads/windows-8-sdk)

## When releasing (Note to myself) : ##

**Manual chores**

- Change the dll version of 3P in AssemblyInfo.cs, the format should be vX.X.X.X with v(major).(minor).(revision).(lastdigit) ; *the last digit of the dll's version indicates if it's a pre-release build (1) or stable build (0)*
- Check the default Config + UserCommunication.Notify()
- Create a new tag : `git tag vX.X.X` (the last digit must be left empty for the tag name!)
- Push the tag `git push --tags` : this will trigger the build with the deployment on the appveyor
- Wait for the build to be done, edit the newly created https://github.com/jcaillon/3P/releases
  - don't forget to check/uncheck the prerelease option
  - Find a cool title
  - past the body from the `NEXT_RELEASE_NOTES.md` file
  - Publish the release!

**Done automatically by appveyor**

- Clean/Rebuild in release mode, not debug for both x86 and x64 versions
- Create 2 .zip in the release on github :
  - "3P.zip" containing the 3P.dll (32 bits) and eventually the .pdb file
  - "3P_x64.zip" containg the 3P.dll (64 bits!) and eventually the .pdb file

## To clean up old releases ##

Since I use the github releases API for the updates, i need this list to be as short as possible so it doesn't download a big json. You can clean up obsolete releases on github; however we want to keep an history on those. 2 solutions :
- create a new md in `md-versions/` on the 3P pages with the tag name of the release to store
- create a new comment on the commit corresponding to the tag of the release