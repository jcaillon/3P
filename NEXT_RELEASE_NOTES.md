Hello everyone,

Quick release to fix small issues reported recently.

### Improvements ###

- \#238: added the possibility to use `<` and `>` in regexes without them being interpreted as variables by doubling them in the deployment rules configuration. This allows to have string exclusion in regexes like: `:^.*(?<<!srv)\.(p|cls)$`

### Fixed issues ###

- \#237: typo on disclaimer
- \#235: the username/password configured for the http proxy were not sent correctly to the proxy
- \#236: you can now use an external directory for your datadigger installation (`options` > `Misc.` > `External tools`)

### Sakoe ###

If you like 3P, you should take a look at my next upcoming project : [SAKOE](https://github.com/jcaillon/Oetools.Sakoe), the **S**wiss **A**rmy **K**nife for **O**pen**E**dge. 

It's a command line interface, multi-purpose tool that will *hopefully* be useful to any openedge developer:

- It will provide various utilities (like simplified database manipulation, reversed XCODE, a better prolib interface and much more!)
- It is designated to be used in a CI (continuous integration) pipeline and will automate build tasks (similarly to ant/maven/msbuild and tools alike)

There are no releases yet, but it will be available soon.

If you like it, don't forget to **STAR IT** on github as it helps a lot for visibility on the platform. Same goes for 3P by the way! :D

Enjoy!