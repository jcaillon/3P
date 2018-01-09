**Improvements :**

- 

**Fixed issues :**

- #185 : Remove the custom syntax highlight checkbox from the options page
- #181 : open the files in the propath on "Go to definition"
- #186 : Parsing problem with preprocessed variables like `{&_proparse_ prolint-nowarn(noundo)}`
- #171 : indentation problem caused by the parser

**Notes regarding cutom syntax themes :**

Due to numerous changes of the syntax highlight feature, the custom colors configuration file has changed a bit. The conf file is in `%notepadplugins%\config\3P\Themes\_ThemesForSyntax.conf`. If you had a custom one, rename it. Then go to Options > Share Export config > click the export button for the syntax highlighting theme. You can then compare the default file to yours and put back your modifications.