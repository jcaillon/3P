&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Procedure
/*------------------------------------------------------------------------

    Purpose   :
                This program is called by 3P when the user execute the "Prolint" action,
                the user has to configure/modify this file according to prolint configuration
                to make the "prolint" command work from 3P

                Basically, you need to first configure :
                    - the path to your Proparse assemblies
                    - the path to your Prolint folder
                They are defined as pre-processed variables in the definition block of this file!

                And then go to the main block and edit this program to call your Prolint entry point,
                for that you can use the preprocessed variables defined in this file, they will be replaced
                by values from 3P at run-time. Notorious variables are :
                    - {&PathFileToProlint} : this is a string representing the path to the current file opened in 3P
                    - {&PathProlintOutputFile} : this is a string representing the path to the prolint output file
                                               that will be read back in 3P to display the prolint messages
                                               (see below for more details about the format of this file)
                                               
                You then have to configure your prolint by creating/modifying a profile to use a custom
                "outputhandler" that will fill the {&PathProlintOutputFile} file with the prolint errors encountered
                (see description for more details)

    Description :
                When this program is being run, the conditions are the same as when you compile/run a file,
                i.e. the databases (if any) are connected, the propath is correctly configured according to
                your "Set environment" screen and so on....

                The format of the prolint output file expected by 3P is :
                original filepath \t currently prolinted file path \t ErrorLevel \t line \t column \t error number \t error message \t help for the message
                errorLevel can be one of the following values : Information, Warning, StrongWarning, Error, Critical
                for instance :
                c:\work\tmp.p	c:\work\tmp.p	Critical	28	1	247	The expression afer "derp" is not understandable.	(Syntax) PROGRESS only understood a part of a statement. Look carefully at your procedure and at "derp".  The problem should be in the next word or special character after "derp" ends.  Check the previous statement for a missing terminator (period or colon).  Check for misplaced keywords or constants that are missing quotes.

                You can check out an example of output handler for 3P here :
                https://github.com/jcaillon/3P/blob/master/3PA/Data/Progress/3pOutputHandler.p

    Author(s)   : Julien Caillon (julien.caillon@gmail.com)
    Created     : 19/02/2016
    Notes       : This file CAN and MUST be freely modified by the user.

  ----------------------------------------------------------------------*/
/*  This file was created with the 3P :  https://jcaillon.github.io/3P/ */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE Procedure
&Scoped-define DB-AWARE no

/* =================================== */
/* ==== USER CONFIGURATION HERE ! ==== */

&SCOPED-DEFINE PathDirectoryToProparseAssemblies ""
&SCOPED-DEFINE PathDirectoryToProlint ""

/* =================================== */


/* Do not modify the lines below, values coming from 3P are set here,
    you can use the pre-processed variables defined below if you find them useful! */
/*<inserted_3P_values>*/

&IF DEFINED(PathFileToProlint) = 0 &THEN
    /* this block is only present for debug/tests purposes, the values below are
       overwritten when this file is run from 3P,
       you can set those values manually for your tests */
    &SCOPED-DEFINE PathFileToProlint ""
    &SCOPED-DEFINE PathProlintOutputFile ""
    &SCOPED-DEFINE PathToStartProlintProgram ""
    &SCOPED-DEFINE UserName ""
    &SCOPED-DEFINE PathActualFilePath ""
&ENDIF
&IF DEFINED(FileApplicationName) = 0 &THEN
    &SCOPED-DEFINE FileApplicationName ""
    &SCOPED-DEFINE FileApplicationVersion ""
    &SCOPED-DEFINE FileWorkPackage ""
    &SCOPED-DEFINE FileBugID ""
    &SCOPED-DEFINE FileCorrectionNumber ""
    &SCOPED-DEFINE FileDate ""
    &SCOPED-DEFINE FileCorrectionDescription ""
&ENDIF


/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME


/* ************************  Function Prototypes ********************** */


/* *********************** Procedure Settings ************************ */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
   Type: Procedure
   Allow:
   Frames: 0
   Add Fields to: Neither
   Other Settings: CODE-ONLY COMPILE
 */
&ANALYZE-RESUME _END-PROCEDURE-SETTINGS

/* *************************  Create Window  ************************** */

&ANALYZE-SUSPEND _CREATE-WINDOW
/* DESIGN Window definition (used by the UIB)
  CREATE WINDOW Procedure ASSIGN
         HEIGHT             = 15
         WIDTH              = 60.
/* END WINDOW DEFINITION */
                                                                        */
&ANALYZE-RESUME




&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Procedure


/* ***************************  Main Block  *************************** */

/* Make sure to find the assemblies needed for prolint (proparse assemblies) */
IF {&PathDirectoryToProparseAssemblies} > "" THEN
    RUN pi_changeAssembliesPath (INPUT {&PathDirectoryToProparseAssemblies}).

/* add the prolint directory to the propath */
IF {&PathDirectoryToProlint} > "" THEN
    PROPATH = PROPATH + "," + {&PathDirectoryToProlint}.

/* a cheap way to make the path to the output file available in prolint : */
SUBSCRIBE "getProlintOutputFile" ANYWHERE RUN-PROCEDURE "pi_getProlintOutputFile".
    
/* ========================================== */
/* ==== MAKE YOUR CALL TO PROLINT HERE ! ==== */

/* a common call to prolint can be : */
/* RUN prolint/launch/lintfileList.p (INPUT {&PathFileToProlint}). */

/* delete this message, it's only for the demo */
IF NOT SESSION:BATCH-MODE THEN
    MESSAGE "Dear user," SKIP(2)
        "Your prolint is not yet configured, you need to modify the following program : " SKIP
        {&PathToStartProlintProgram} SKIP(2)
        "Here are the available variables you can work with : " SKIP
        "~{~&PathFileToProlint~} = " + {&PathFileToProlint} SKIP
        "~{~&PathProlintOutputFile~} = " + {&PathProlintOutputFile} SKIP
        "~{~&UserName~} = " + {&UserName} SKIP
        "~{~&FileApplicationName~} = " + {&FileApplicationName} SKIP
        "~{~&FileApplicationVersion~} = " + {&FileApplicationVersion} SKIP
        "~{~&FileWorkPackage~} = " + {&FileWorkPackage} SKIP
        "~{~&FileBugID~} = " + {&FileBugID} SKIP
        "~{~&FileCorrectionNumber~} = " + {&FileCorrectionNumber} SKIP
        "~{~&FileDate~} = " + {&FileDate} SKIP
        "~{~&FileCorrectionDescription~} = " + {&FileCorrectionDescription} SKIP
        VIEW-AS ALERT-BOX INFORMATION
        BUTTONS OK
        TITLE "Prolint not configured".

/* ========================================== */

/* use the PUBLISH below to display a notification in 3P after the end of this program */
PUBLISH "eventToPublishToNotifyTheUserAfterExecution" (
    INPUT "my message content, <b>HTML</b> format! You can also set a <a href='http://jcaillon.github.io/3P/'>link</a> or whatever you want",
    INPUT 0, /* from 0 to 4, to have an icon corresponding to : "MsgOk", "MsgError", "MsgWarning", "MsgInfo", "MsgHighImportance" */
    INPUT "My notification title",
    INPUT "My notification subtitle",
    INPUT 0, /* duration of the notification in seconds (0 for infinite time) */
    INPUT "uniquename" /* unique name for the notification, if it it set, the notif will close on a click on a link and 
                will automatically be closed if another notification with the same name pops up */
    ).

RETURN "".

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

PROCEDURE pi_getProlintOutputFile:
/*------------------------------------------------------------------------------
  Purpose: Allows to get the path of the prolint output file
  Parameters:
    opc_prolintOutputPath = path to the output file to feed
------------------------------------------------------------------------------*/

    DEFINE OUTPUT PARAMETER opc_prolintOutputPath AS CHARACTER NO-UNDO INITIAL {&PathProlintOutputFile}.

    RETURN "".
END.

PROCEDURE pi_changeAssembliesPath PRIVATE:
/*------------------------------------------------------------------------------
  Purpose: This procedure allows to modify the assemblies path dynamically, it is equivalent to
    start a prowin process with the parameter : -assemblies "my path"
  Parameters:
    ipc_newPath = new assemblies path
------------------------------------------------------------------------------*/

    DEFINE INPUT PARAMETER ipc_newPath AS CHARACTER NO-UNDO.

    DEFINE VARIABLE assemblyStore AS Progress.ClrBridge.AssemblyStore NO-UNDO.
    DEFINE VARIABLE lc_oldAssemblyDir AS CHARACTER NO-UNDO.

    assemblyStore = Progress.ClrBridge.AssemblyStore:Instance.
    lc_oldAssemblyDir = assemblyStore:AssembliesPath.

    IF LENGTH(ipc_newPath) > 0 THEN DO:
        assemblyStore:AssembliesPath = ipc_newPath.
    END.

    assemblyStore:Load() NO-ERROR.
    assemblyStore:AssembliesPath  = lc_oldAssemblyDir.

    IF VALID-OBJECT(assemblyStore) THEN
        DELETE OBJECT assemblyStore.
    IF ERROR-STATUS:ERROR THEN
        RETURN ERROR-STATUS:GET-MESSAGE(1).

    RETURN "".
END.

/* ************************  Function Implementations ***************** */

