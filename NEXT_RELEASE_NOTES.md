Hello folks,

This is yet another small release in the stable branch. It doesn't bring new features, but instead tries to stabilize the v1.8.x version of 3P.

In the meantime, i've been pretty busy in the development branch of 3P, working on the GUI framework and modularizing components of 3P for external usage (like the parser and the deployer/mass compiler for instance). A lot of goodies that will come with the next beta version v1.9; if you wish to preview these features and help me debug 3P, don't forget that you can switch to the beta branch of updates in the option page.

### Important notes : ###

I've slightly lowered the requirement from .net 4.6.2 to .net 4.6.1 for the simple reason that this latter version is more commonly used and that 4.6.2 doesn't bring interesting features for me.

### Improvements : ###

- #220 : Extent indicator on tooltips for variables, parameters, fields and function return types

### Fixed issues : ###

- #237: typo on disclamimer
- #235: the username/password configured for the http proxy were not sent to the proxy
- #236: you can now use an external directory for your datadigger installation
