Hello everyone,

If you are reading this update message from 3P, thank you for understanding my need to update to .net 4.6.2! Read more about this below...

This release is mainly to fix the updater and to target .net 4.6.2, new features are coming in the next release, stay tuned!


### Important notes :###

Since February 22 the 3P automatic updater is not working and displays a message saying it failed to query the github API.

The 3P updater is using the [github API](https://developer.github.com/v3/) to check for new releases and download the .zip file. Last Thursday, GitHub turned off some weak crypto standards; including TLSv1.1. [You can read about this here.](https://githubengineering.com/crypto-removal-notice/)

How is it related you ask? I've build 3P around the .NET framework v4.0 which only supports TLSv1.1. This is the default security protocol and also the latest protocol supported by this version. This means I had to upgrade the project to a newer version of the .net framework : [.NET 4.6.2](https://www.microsoft.com/en-us/download/details.aspx?id=53344). That also means that 3P will not be able to run on windows versions inferior to windows 7. Learn more in [issue 217](https://github.com/jcaillon/3P/issues/217) and in [issue 214](https://github.com/jcaillon/3P/issues/214).


### Improvements :###

- #215 : Remove multiple database configuration per environment

### Fixed issues :###

- #214 : Updater error : Couldn't query GITHUB API


Thank you for reading,

Enjoy