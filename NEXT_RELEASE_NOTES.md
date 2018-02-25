Hello everyone,

### Important notes :###

Since February 22 the 3P automatic updater is not working and displays a message saying it failed to query the github API.

The 3P updater is using the [github API](https://developer.github.com/v3/) to check for new releases and download the .zip file. Last Thursday, GitHub turned off some weak crypto standards; including TLSv1.1. [You can read about this here.](https://githubengineering.com/crypto-removal-notice/)

How is it related you ask? I've build 3P around the .NET framework v4.0... which only supports TLSv1.1. This is the default security protocol and also the latest protocol supported by this version. This means we have to upgrade the project to [.NET 4.6](https://www.microsoft.com/en-US/download/details.aspx?id=48130). That also means that 3P will not be able to run on windows versions inferior to windows vista.


### Improvements :###



### Fixed issues :###

