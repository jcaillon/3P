### Autocompletion ###

* A key feature of 3P is the built in autocompletion window. Fully integrated with notepad++, it smoothes the developper work by suggesting the best match according to his intention :

![image](https://cloud.githubusercontent.com/assets/11553075/11215781/419a86a4-8d48-11e5-9155-c062659551dd.png)

* The embedded dictionnary of ABL keywords contains more than 2800 words which should cover your entire needs (you can even add new words if some are missings) :

![image](https://cloud.githubusercontent.com/assets/11553075/11216157/136dc9d8-8d4a-11e5-9775-2abdc5b77d33.png)

* The more you use a keyword, the higher it will appear in the list

* The list doesn't exclusivly include *static* words ; a built in ABL parser analyses the code on the fly and pushes words like : variables, procedures, functions, temp-tables and so on, into the autocompletion list. Moreover, if a program contains an include call `{myinclude.i}` it will also analyse the included file.

* It only displays elements that are relevant at the cursor position. For instance, a variable defined in a local procedure will not show up in another procedure ; or a variable will not show up in a line where it is not yet known by the compiler :

![image](https://cloud.githubusercontent.com/assets/11553075/11218038/8c49e586-8d53-11e5-884e-736cac8892a7.png)

* it sorts the keywords into categories (more than 15) to easily find your keyword. You can activate/deactivate a category with a simple click :

![image](https://cloud.githubusercontent.com/assets/11553075/11217991/5ca40abe-8d53-11e5-99ec-66ea7a06187d.png)

* if you configure a connection to a progress database, it provides the autocompletion on `TABLE` names as well as `FIELD` names. Enter `mydatabasename` followed by a dot `.` and a list of tables of this database will show up, a dot `.` after a table name will show up its fields (it also differentiates fields that are part of the primary key) :

![image](https://cloud.githubusercontent.com/assets/11553075/11216639/85bdaccc-8d4c-11e5-9caa-6ff3a24d5b72.png)

> This also works with buffers so... use them!

* A keyword can have one or several flags (and eventually a small text) that quickly provides extra information on said word. In the example below, `g_codeset` is a `SHARED` variable, defined in the `PROGRAM SCOPE` (i.e. available to all procedures and functions) and was found in an `INCLUDE` file:

![image](https://cloud.githubusercontent.com/assets/11553075/11216736/208108f8-8d4d-11e5-944f-a2267c7c0c34.png)

* When using the `UP` or `DOWN` arrow keys, you can navigate through the list. It also shows a tooltip next to the autocompletion that provides more information on the keyword selected, this tooltip can be used to understand the flags described in the above section!

![image](https://cloud.githubusercontent.com/assets/11553075/11216436/70493fba-8d4b-11e5-9822-3089c62be2de.png)

* Extra tips :
    - Thanks to the way keywords are sorted, you can find quick ways to type a word. For instance, the fastest way to type `ERROR-STATUS` can be typing `errsta` and pressing tab to accept the suggestion
    - By holding the `ALT` key in addition to `LEFT` or `RIGHT` arrow key you can quickly filter the keywords by categories while keeping your two hands on the keyboard
    - Through the option window, you can set a mode that automatically inserts the best match when you end a word (a space, a dot or any operator can end a word). This is the fastest way to write ABL code!
    - A tons of other options are available, go check them out!
