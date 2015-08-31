/*
 2012-12-xx : Améliorations CCL
              - Ajout d'un message d'erreur si le moteur d'exécution Progress client à lancer n'existe pas (pas installé)
 2013-03-13 : Améliorations CCL
              - Amélioration des messages d'erreur en cas d'échec d'exécution sur AppServer avec version en question
              - Recherche dynamique de port socket libre
              - Mise en forme du code pour une meilleur lisibilité
              - Ajout de constantes de précompilation pour la plage de ports TCP
              - Amélioration des descriptions de messages d'erreur en général
              - Ajout d'un titre précisant la release de l'outil de compil pour tous les messages
              - Amélioration des points de sortie du programme
              - Détection du mode DEBUG
              - Suppression des fichiers temporaires sauf en mode DEBUG
              - Le mode DEBUG force le démarrage des moteurs Progress enfants (dans la version cible) avec des traces
 2013-12-13 : Améliorations CCL
              - Ajout de détection d'erreur de connexion de la partie serveur
              - Correction de la compilation unitaire serveur qui ne fonctionnait pas
*/


/* -------------------------------------------------------------------------- */
/* PARAMTERES                                                                 */
/* -------------------------------------------------------------------------- */
DEFINE INPUT PARAMETER icFile AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER cEnv AS CHARACTER NO-UNDO.
DEFINE INPUT PARAMETER cProj AS CHARACTER NO-UNDO.

/* -------------------------------------------------------------------------- */
/* CONSTANTES                                                                 */
/* -------------------------------------------------------------------------- */
&SCOPED-DEFINE DLLPATH                p:\appli\tmaprogress\new_tmaprogress\SharedMem.dll
&SCOPED-DEFINE COMPILERPATH           p:\appli\tmaprogress\new_tmaprogress\compileur.p
&SCOPED-DEFINE COMPILERPATHUNIX       /appli2/progress/appli/tmaprogress/new_tmaprogress/compilSocket.p

/* The following Visual Style parameters define the initial state of the application's main window. */
&GLOBAL-DEFINE SW-SHOWNORMAL          1     /* Start the application in a normal size window. */
&GLOBAL-DEFINE SW-SHOWMINIMIZED       2     /* Start the application minimized. Show an icon at the bottom of the screen. */
&GLOBAL-DEFINE SW-SHOWMAXIMIZED       3     /* Start the application in a maximized window. */
&GLOBAL-DEFINE SW-SHOWNOACTIVATE      4     /* Start the application but set the focus back to the calling program. */
&GLOBAL-DEFINE SW-SHOWMINNOACTIVE     7     /* Start the application minimized and set the focus back to the calling program. */

&GLOBAL-DEFINE SOCKET_FIRST_PORT      10001
&GLOBAL-DEFINE SOCKET_LAST_PORT       10500

&GLOBAL-DEFINE APP_TITLE              "Outil de compilation unitaire"
&SCOPED-DEFINE DEBUGFILE              "d:\temp\compil.debug"
&GLOBAL-DEFINE SRV_CONNECT_STRING     "-AppService compiltool -S NSV10 -H rs28.lyon.fr.sopra"



/* -------------------------------------------------------------------------- */
/* API                                                                        */
/* -------------------------------------------------------------------------- */
PROCEDURE Sleep EXTERNAL "KERNEL32.DLL":
    DEFINE INPUT  PARAMETER dwMilliSeconds          AS LONG.
    DEFINE RETURN PARAMETER StatusCode              AS LONG.
END PROCEDURE.

PROCEDURE WinExec EXTERNAL "KERNEL32.DLL":
    DEFINE INPUT  PARAMETER ProgramName             AS CHARACTER.
    DEFINE INPUT  PARAMETER VisualStyle             AS LONG.
    DEFINE RETURN PARAMETER StatusCode              AS LONG.
END PROCEDURE.

PROCEDURE CreateSharedMemA EXTERNAL "{&DLLPATH}" CDECL PERSISTENT:
    DEFINE INPUT  PARAMETER dwIdx                   AS LONG        NO-UNDO.
    DEFINE INPUT  PARAMETER cchSize                 AS LONG        NO-UNDO.
    DEFINE INPUT  PARAMETER hwndFnd                 AS LONG        NO-UNDO.
    DEFINE RETURN PARAMETER lOK                     AS LONG        NO-UNDO.
END PROCEDURE.

PROCEDURE GetSharedMemA EXTERNAL "{&DLLPATH}" CDECL PERSISTENT:
    DEFINE INPUT  PARAMETER bWaitForEvent           AS LONG        NO-UNDO.
    DEFINE INPUT  PARAMETER dwIdx                   AS LONG        NO-UNDO.
    DEFINE RETURN PARAMETER ocSet                   AS MEMPTR      NO-UNDO.
END PROCEDURE.

PROCEDURE SetSharedMemA EXTERNAL "{&DLLPATH}" CDECL PERSISTENT:
    DEFINE INPUT  PARAMETER lpszBuf                 AS CHARACTER   NO-UNDO.
    DEFINE INPUT  PARAMETER dwIdx                   AS LONG        NO-UNDO.
    DEFINE INPUT  PARAMETER bEventInsteadOfMessage  AS LONG        NO-UNDO.
    DEFINE RETURN PARAMETER lOK                     AS LONG        NO-UNDO.
END PROCEDURE.

DEFINE VARIABLE  AS INTEGER NO-UNDO.


 

/* -------------------------------------------------------------------------- */
/* VARIABLES                                                                  */
/* -------------------------------------------------------------------------- */
DEFINE VARIABLE dwMemSize               AS INTEGER     NO-UNDO      INITIAL 4096.
DEFINE VARIABLE mMem                    AS MEMPTR      NO-UNDO.
DEFINE VARIABLE iReturn                 AS INTEGER     NO-UNDO.
DEFINE VARIABLE iIdx                    AS INTEGER     NO-UNDO.
DEFINE VARIABLE vcommande_dos           AS CHARACTER   NO-UNDO.


DEFINE VARIABLE vcpt_fic                AS INTEGER      NO-UNDO.
DEFINE VARIABLE vflag_tache             AS LOGICAL      NO-UNDO.
DEFINE VARIABLE vflag_rep               AS LOGICAL      NO-UNDO.
DEFINE VARIABLE vflag_local             AS LOGICAL      NO-UNDO.
DEFINE VARIABLE vflag_distantpart       AS LOGICAL      NO-UNDO.
DEFINE VARIABLE crepsrc                 AS CHARACTER    NO-UNDO.
DEFINE VARIABLE rversion                AS CHARACTER    NO-UNDO.
DEFINE VARIABLE cResultCompil           AS CHARACTER    NO-UNDO.
DEFINE VARIABLE gcsynuser               AS CHARACTER    NO-UNDO.
DEFINE VARIABLE gcTrig                  AS CHARACTER    NO-UNDO.

DEFINE VARIABLE cDir                    AS CHARACTER    NO-UNDO.
DEFINE VARIABLE lsinc                   AS CHARACTER    NO-UNDO.

DEFINE VARIABLE iCompileFileCount       AS INTEGER      NO-UNDO.
DEFINE VARIABLE iConnexionCount         AS INTEGER      NO-UNDO.
DEFINE VARIABLE iDummy                  AS INTEGER      NO-UNDO.

DEFINE VARIABLE ghAppSrv                AS HANDLE       NO-UNDO.
DEFINE VARIABLE mHeader                 AS MEMPTR       NO-UNDO.
DEFINE VARIABLE mData                   AS MEMPTR       NO-UNDO.
DEFINE VARIABLE iDataSize               AS INTEGER      NO-UNDO.

DEFINE VARIABLE gKillCompil             AS LOGICAL      NO-UNDO.
DEFINE VARIABLE glError                 AS LOGICAL      NO-UNDO.

DEFINE VARIABLE lhSocketServer          AS HANDLE       NO-UNDO.



/* -------------------------------------------------------------------------- */
/* TABLES TEMPO                                                               */
/* -------------------------------------------------------------------------- */
DEFINE NEW SHARED TEMP-TABLE temp_fic NO-UNDO
    FIELD nomfic        AS CHARACTER FORMAT "x(50)"
    FIELD src           AS CHARACTER FORMAT "x(150)"
    FIELD dest          AS CHARACTER FORMAT "x(150)"
    FIELD repdest       AS CHARACTER FORMAT "x(150)"
    FIELD module        AS CHARACTER FORMAT "x(150)"
    FIELD rversion      AS CHARACTER
    INDEX idx1 IS PRIMARY UNIQUE src rversion module
    INDEX idx2 rversion module.

DEFINE TEMP-TABLE ttMess NO-UNDO
    FIELD iIdx          AS INTEGER
    FIELD cVersion      AS CHARACTER
    FIELD rRowid        AS ROWID
    FIELD hSocketServer AS HANDLE
    INDEX idx1st IS PRIMARY UNIQUE iIdx
    INDEX idx2nd cVersion.  
    

/* -------------------------------------------------------------------------- */
/* INCLUDES                                                                   */
/* -------------------------------------------------------------------------- */
/* Chemin complet nécessaire, au moins pour la partie AppBuilder */
{p:\appli\tmaprogress\new_tmaprogress\createCompilFic.i}



/* -------------------------------------------------------------------------- */
/* STREAMS                                                                    */
/* -------------------------------------------------------------------------- */
DEFINE STREAM sLog.

IF SEARCH("d:~\temp~\" + OS-GETENV("USERNAME") + ".properties" ) <> ? THEN DO:
    INPUT FROM VALUE("D:~\temp~\" + OS-GETENV("USERNAME") + ".properties").
    IMPORT gcTrig gcSynUser NO-ERROR.
    INPUT CLOSE.
END.

/* Petite verrue pour la façade */
IF cProj = "TIE" THEN
    ASSIGN cProj = "FTE".

FIND FIRST tma_environnement WHERE tma_environnement.code_projet    = CAPS(cProj)
                               AND tma_environnement.env_sopra      = CAPS(cEnv)
                               NO-LOCK NO-ERROR.

/* Vérif. si paramétrage trouvé */
/********************************/
IF NOT AVAILABLE tma_environnement THEN DO:
    MESSAGE "Paramétrage <Environnement> absent." SKIP
            "- code projet = " + QUOTER(CAPS(cProj)) SKIP
            "- code environnement = " + QUOTER(CAPS(cEnv)) SKIP
            "- répertoire = " + QUOTER(cDir) SKIP
            VIEW-AS ALERT-BOX WARNING BUTTONS OK TITLE {&APP_TITLE}.
    RETURN ERROR "".
END.


/* write delete local files */
OUTPUT TO VALUE("d:~\temp~\supprloc.bat").
OUTPUT CLOSE.


/* Compile */
EMPTY TEMP-TABLE temp_fic.
ASSIGN  vcpt_fic = 0.
        iConnexionCount = ?.

RUN createCompilFic
            (INPUT ROWID(tma_environnement),
             INPUT icFile,
             INPUT FALSE,
             INPUT TRUE,
             INPUT FALSE,
             INPUT TRUE, /* Distant + local delete */
             INPUT cProj,
             INPUT "",
             INPUT ?,
             INPUT 0,
             INPUT 0,
             INPUT FALSE,
             INPUT FALSE,
             INPUT FALSE,
             INPUT-OUTPUT lsinc,
             INPUT-OUTPUT vcpt_fic,
             INPUT-OUTPUT TABLE temp_fic).

 /* execute delete local files */
OS-COMMAND SILENT VALUE("d:~\temp~\supprloc.bat").


IF vcpt_fic = 0 THEN DO:
    /*
    MESSAGE "Aucun fichier à compiler." SKIP
            "Les programmes à compiler doivent se situer dans la workarea Synergy."
            VIEW-AS ALERT-BOX WARNING BUTTONS OK TITLE {&APP_TITLE}.
    */
    RETURN "0".
END.
ELSE DO:
    /* Create Shared Mem **********************************/
    ASSIGN iIdx = 0.
    FOR EACH temp_fic NO-LOCK WHERE temp_fic.module = "CL",
        FIRST tma_paramenv NO-LOCK  WHERE tma_paramenv.CODE_projet = tma_environnement.CODE_projet
                                      AND tma_paramenv.env_sopra   = tma_environnement.env_sopra
                                      AND tma_paramenv.rversion    = temp_fic.rversion,
        FIRST tma_version NO-LOCK WHERE tma_version.version     = tma_paramenv.rversion:

        IF SEARCH(tma_version.dir_win) <> ? THEN DO:
            /* Initialisation de la mémoire partagée */
            RUN CreateSharedMemA(iIdx, dwMemSize, 0, OUTPUT iReturn).
            IF iReturn = 0 THEN
                MESSAGE "Erreur à l'intialisation de la mémoire partagée (OCX)." VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE {&APP_TITLE}.
            ELSE DO:
                ASSIGN vcommande_dos = " -basekey ini -ininame "
                                     + TRIM(tma_paramenv.ini_win)
                                     + " -pf "
                                     + TRIM(tma_paramenv.pf_win)
                                     + " -p {&COMPILERPATH}"
                                     + " -inp 20000"        /* Nbre maximum de caractères par instruction */
                                     + " -tok 2048"         /* Nbre maximum de "token" par instruction    */
                                     + " " + TRIM(tma_paramenv.params)
                                     + " -param " + STRING(iIdx) + "@0"
                                     + " -T d:\temp"
                                     + " -b " /*+ "> D:\temp\" + tma_paramenv.rversion + ".log"*/
                       vcommande_dos = tma_version.dir_win + vcommande_dos.

                RUN WinExec (INPUT vcommande_dos, INPUT {&SW-SHOWMINIMIZED}, OUTPUT iReturn).
                IF iReturn < 32 THEN DO:
                        MESSAGE "** Erreur au démarrage d'une session Progress batch client " + tma_paramenv.rversion + "." SKIP
                                vcommande_dos
                                VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE {&APP_TITLE}.
                    ASSIGN glError = TRUE.
                    LEAVE.
                END.

                ASSIGN  iIdx = iIdx + 1
                        iCompileFileCount = iCompileFileCount + 1.
            END.
        END.
        ELSE DO:
            ASSIGN glError = TRUE.
            MESSAGE "** Moteur d'exécution Progress non trouvé dans le répertoire :" SKIP
                    QUOTER(tma_version.dir_win)
                    VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE {&APP_TITLE}.
            LEAVE.
        END.
    END. /* FOR EACH temp_fic, FIRST tma_paramenv */


    IF NOT glError THEN DO:
        iIdx = 0.           /* Init importante */
        FOR EACH tma_paramenv NO-LOCK
                WHERE tma_paramenv.CODE_projet = tma_environnement.CODE_projet
                  AND tma_paramenv.env_sopra   = tma_environnement.env_sopra:
            IF CAN-FIND( FIRST temp_fic WHERE temp_fic.rversion = tma_paramenv.rversion AND temp_fic.module = "SRV" ) THEN DO:
                FIND FIRST tma_version WHERE tma_version.VERSION = tma_paramenv.rversion NO-LOCK.

                /* Create socket server once (and add protection because OpenEdge allows listenning only ton ONE socket) */
                IF iIdx = 0 THEN DO:
                    ASSIGN iIdx = {&SOCKET_FIRST_PORT}
                           lhSocketServer = ?.
                    RUN createSocketServer ( INPUT-OUTPUT iIdx, OUTPUT lhSocketServer ) NO-ERROR.
                    IF ERROR-STATUS:ERROR THEN DO:
                        MESSAGE "** Erreur à la création du serveur de sockets." VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE {&APP_TITLE}.
                        ASSIGN glError = TRUE.
                        LEAVE.
                    END.
                END.

                /* iIdx contient le numéro de port du serveur */
                CREATE  ttMess.
                ASSIGN  ttMess.cVersion       = tma_paramenv.rversion
                        ttMess.hSocketServer  = lhSocketServer
                        ttMess.iIdx           = iIdx.


                /* Initialisation de la mémoire partagée */
                ASSIGN vcommande_dos = " -pf "
                                         + TRIM(tma_paramenv.pf_aix)
                                         + " -p {&COMPILERPATHUNIX}"
                                         + " -inp 20000"    /* Nbre maximum de caractères par instruction */
                                         + " -tok 2048"     /* Nbre maximum de "token" par instruction    */
                                         + " " + TRIM(tma_paramenv.params)
                                         + " -param " + STRING(iIdx) + "@" + OS-GETENV("COMPUTERNAME") + "@" + gcSynUser
                                         + " -T tmp"
                                         + " -b > /appli2/progress/tmp/" + gcSynUser + "-compiltool2" + tma_paramenv.rversion + ".log".

                /* connexion unique à l'appserver */
                IF NOT VALID-HANDLE(ghAppSrv) THEN DO:
                    CREATE SERVER ghAppSrv.
                    ghAppSrv:CONNECT({&SRV_CONNECT_STRING}).
                END.

                /* launch compil session through appserver procedure */
                IF VALID-HANDLE(ghAppSrv) AND ghAppSrv:CONNECTED() THEN DO:
                    ASSIGN iConnexionCount = 0.           /* On le fait passer de ? à 0 */
                    RUN launchCompilSrv.p ON SERVER ghAppSrv ( INPUT SUBSTRING( tma_version.dir_aix, 1, INDEX( tma_version.dir_aix, "_dv/" ) ), INPUT tma_version.dir_aix, INPUT TRIM(tma_paramenv.ini_aix), INPUT vcommande_dos ) NO-ERROR.
                    IF ERROR-STATUS:ERROR THEN DO:
                        ASSIGN glError = TRUE.
                        MESSAGE "** Erreur au lancement du traitement batch de compilation pour la version " + tma_version.dir_aix SKIP
                                RETURN-VALUE
                                VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE {&APP_TITLE}.
                        LEAVE.
                    END.
                    ASSIGN iCompileFileCount = iCompileFileCount + 1.
                END.
                ELSE DO:
                MESSAGE "** Problème de connexion à l'AppServer." SKIP
                        {&SRV_CONNECT_STRING}
                        VIEW-AS ALERT-BOX ERROR TITLE {&APP_TITLE}.
                    ASSIGN glError = TRUE.
                    LEAVE.
                END.
                ASSIGN iIdx = iIdx + 1.
            END.

        END.
    END.

    IF iCompileFileCount = 0 THEN DO:
        ASSIGN glError = TRUE.
        MESSAGE "Aucun fichier n'est compilable en fonction du paramétrage environnement." SKIP
                "Vérifiez le paramétrage environnement (Menu Environnement > Param session)."
                VIEW-AS ALERT-BOX ERROR TITLE {&APP_TITLE}.
    END.


    IF NOT glError THEN DO:
        /* Wait For IDENT to send file to compile **********************************/
        ASSIGN iIdx = 0.
        FOR EACH temp_fic
            WHERE temp_fic.module = "CL",
            FIRST tma_paramenv NO-LOCK
            WHERE tma_paramenv.CODE_projet = tma_environnement.CODE_projet
              AND tma_paramenv.env_sopra   = tma_environnement.env_sopra
              AND tma_paramenv.rversion    = temp_fic.rversion:

            /* Read and Write Memory */
            RUN ShMemRead(iIdx, tma_paramenv.rversion).
            iIdx = iIdx + 1.
        END. /* FOR EACH temp_fic, FIRST tma_paramenv */

        /* Wait For Response of compil **********************************/
        ASSIGN iIdx = 0.
        FOR EACH temp_fic
            WHERE temp_fic.module = "CL",
            FIRST tma_paramenv NO-LOCK
            WHERE tma_paramenv.CODE_projet = tma_environnement.CODE_projet
              AND tma_paramenv.env_sopra   = tma_environnement.env_sopra
              AND tma_paramenv.rversion    = temp_fic.rversion:

            /* Read Memory and display Message if necessary */
            RUN ShMemRead(iIdx, tma_paramenv.rversion).
            iIdx = iIdx + 1.
        END. /* FOR EACH temp_fic, FIRST tma_paramenv */

        /* Kill Compilers **********************************/
        ASSIGN iIdx = 0.
        FOR EACH temp_fic
            WHERE temp_fic.module = "CL",
            FIRST tma_paramenv NO-LOCK
            WHERE tma_paramenv.CODE_projet = tma_environnement.CODE_projet
              AND tma_paramenv.env_sopra   = tma_environnement.env_sopra
              AND tma_paramenv.rversion    = temp_fic.rversion:

            RUN SetSharedMemA("DIE", iIdx, 1, OUTPUT iReturn).
            IF iReturn = 0 THEN DO:
                ASSIGN glError = TRUE.
                MESSAGE "** Erreur lors de l'écriture de la commande " + QUOTER("DIE") + " (Main-Block)." VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE {&APP_TITLE}.
                LEAVE.
            END.
            ASSIGN iIdx = iIdx + 1.
        END. /* FOR EACH temp_fic, FIRST tma_paramenv */


        /* Try to detect server compilation errors */

        IF iConnexionCount = 0 THEN DO:       /* iConnexionCount = 0 <==> server compilation was started */
            WAIT-FOR CLOSE OF THIS-PROCEDURE PAUSE 2.
            IF iConnexionCount = 0 THEN DO:
                ASSIGN glError = TRUE.
                MESSAGE "Aucun fichier n'a été compilé depuis le serveur." SKIP
                        "Vérifiez le paramétrage environnement et les logs serveur de compilation."
                        VIEW-AS ALERT-BOX ERROR TITLE {&APP_TITLE}.
            END.
        END.

    END.
END.

RETURN "".


/* -------------------------------------------------------------------------- */
/* ShMemRead                                                                  */
/* -------------------------------------------------------------------------- */
PROCEDURE ShMemRead:
    DEFINE INPUT PARAMETER pIdx AS INTEGER NO-UNDO.
    DEFINE INPUT  PARAMETER pVersion AS CHARACTER   NO-UNDO.

    DEFINE VARIABLE cLigneRecue  AS CHARACTER   NO-UNDO.
    DEFINE VARIABLE i            AS INTEGER     NO-UNDO.
    DEFINE VARIABLE mMem         AS MEMPTR      NO-UNDO.
    DEFINE VARIABLE cMessageBox  AS CHARACTER   NO-UNDO.


    /* Lecture des données en retour */
    /* Récupération des données dans une variable */
    /* Format : KOouOK|[FileName|ERROR-ROW|ERROR-COL|NUM-MESSAGES|ERROR-STATUS:GET-MESSAGE(1)|... */
    /* ou Identification Ident */
    RUN GetSharedMemA(1, pIdx, OUTPUT mMem).

    ASSIGN cLigneRecue = GET-STRING(mMem, 1, INTEGER(dwMemSize / 2)) + "#######".      /* Pour éviter une erreur plus tard dans l'analyse des résultats */

    /* Demande d'identification -> Envoi du premier élément */
    /********************************************************/
    IF cLigneRecue = "Ident" THEN DO:
        FIND FIRST temp_fic WHERE temp_fic.rversion = pVersion NO-ERROR.
        IF AVAILABLE temp_fic THEN
            RUN ShMemWrite ( temp_fic.src, temp_fic.repdest, pIdx ).            /* Ecriture du fichier */
    END.
    /* Retour de compilation -> Récupération des erreurs et envoi de l'élément suivant */
    /***********************************************************************************/
    ELSE DO:
        /* Ecriture de la log */
        IF NUM-ENTRIES(cLigneRecue, "#") > 1 THEN DO:
            IF ENTRY(1, cLigneRecue, "#") = "KO" AND NUM-ENTRIES(cLigneRecue, "#") >= 4 THEN
                ASSIGN cMessageBox = "** ERREUR de compilation dans " + ENTRY(2,cLigneRecue, "#") + " ligne " + ENTRY(3,cLigneRecue, "#") + " colonne " + ENTRY(4,cLigneRecue, "#").
            ELSE
                ASSIGN cMessageBox = "** WARNING de compilation".

            IF ENTRY(5, cLigneRecue, "#") <> "" THEN DO:
                DO i = 1 TO /*INTEGER(ENTRY(5, cLigneRecue, "#"))*/ NUM-ENTRIES(cLigneRecue, "#") - 5:
                    ASSIGN  cMessageBox = cMessageBox +
                                          "~n    Message: " + ENTRY(5 + i, cLigneRecue, "#") +
                                          FILL(" ", 92 - LENGTH(ENTRY(5 + i, cLigneRecue, "#"))) +
                                          (IF i = INTEGER(ENTRY(5, cLigneRecue, "#")) THEN "**" ELSE "").
                END.
            END.

            IF ENTRY(1, cLigneRecue, "#") = "KO" THEN
                MESSAGE cMessageBox  VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE pVersion + " - " + {&APP_TITLE}.
            ELSE
                MESSAGE cMessageBox  VIEW-AS ALERT-BOX WARNING BUTTONS OK TITLE pVersion + " - " + {&APP_TITLE}.
        END.
    END.
    RETURN "".
END PROCEDURE.



/* -------------------------------------------------------------------------- */
/* ShMemWrite                                                                 */
/* -------------------------------------------------------------------------- */
PROCEDURE ShMemWrite:
    DEFINE INPUT  PARAMETER icSrc    AS CHARACTER   NO-UNDO.
    DEFINE INPUT  PARAMETER icDst    AS CHARACTER   NO-UNDO.
    DEFINE INPUT  PARAMETER iIdx     AS INTEGER     NO-UNDO.

    DEFINE VARIABLE cLigneATrans    AS CHARACTER   NO-UNDO.
    DEFINE VARIABLE iRet            AS INTEGER     NO-UNDO.


    ASSIGN cLigneATrans    = icSrc + "|" + icDst.

    /* Ecriture du fichier dans la Shared Mem */
    RUN SetSharedMemA(cLigneATrans, iIdx, 1, OUTPUT iRet).
    IF iRet = 0 THEN
        MESSAGE "Erreur lors de l'écriture de " + QUOTER(cLigneATrans) + " dans la Shared Memory (ShMemWrite)." VIEW-AS ALERT-BOX ERROR BUTTONS OK.

    RETURN "".
END PROCEDURE.



/* -------------------------------------------------------------------------- */
/* createSocketServer                                                         */
/* -------------------------------------------------------------------------- */
PROCEDURE createSocketServer:
    DEFINE INPUT-OUTPUT PARAMETER   ioiLocalPort  AS INTEGER     NO-UNDO.
    DEFINE OUTPUT PARAMETER         ohServer      AS HANDLE      NO-UNDO.

    DEFINE VARIABLE llOK    AS LOGICAL  NO-UNDO.


    CREATE SERVER-SOCKET ohServer.

    /* Boucle pour utiliser le premier port libre */
    ohServer:SET-CONNECT-PROCEDURE("ProcessClientConnect").
    DO WHILE ioiLocalPort <= {&SOCKET_LAST_PORT} :
        ASSIGN llOK = ohServer:ENABLE-CONNECTIONS("-S " + STRING(ioiLocalPort)) NO-ERROR.
        IF ERROR-STATUS:ERROR OR ERROR-STATUS:GET-NUMBER(1) = 9185 THEN DO:
            ASSIGN llOK = FALSE.
            LEAVE.
        END.
        IF llOK THEN
            LEAVE.

        ASSIGN ioiLocalPort = ioiLocalPort + 1.
    END.

    /* Cas tous les ports sont occupés */
    IF ioiLocalPort > {&SOCKET_LAST_PORT} THEN DO:
        ASSIGN llOK = FALSE.
        MESSAGE "** Tous les ports de la plage autorisée sont occupés [" + STRING({&SOCKET_FIRST_PORT}) + " - " + STRING({&SOCKET_LAST_PORT}) + "]." SKIP
                "L'application est dans un état instable." SKIP
                "Fermez toutes les instances de Progress OpenEdge, priez et recommencez."
                VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE {&APP_TITLE}.
    END.

    /* Cas il y a eu un problème */
    IF NOT llOK THEN DO:
        IF VALID-HANDLE(ohServer) THEN DO:
            /* Pas besoin de ohServer:DISABLE-CONNECTIONS() ici car il n'y a pas encore eu de ENABLE-CONNECTIONS() */
            DELETE OBJECT ohServer.
            ASSIGN ohServer = ?.
        END.
        RETURN ERROR "".
    END.

    ASSIGN  SET-SIZE(mHeader)       = 4
            SET-BYTE-ORDER(mHeader) = BIG-ENDIAN.

    RETURN "".
END PROCEDURE.



/* -------------------------------------------------------------------------- */
/* processClientConnect                                                       */
/* -------------------------------------------------------------------------- */
PROCEDURE processClientConnect:
    DEFINE INPUT PARAMETER hSocket AS HANDLE NO-UNDO.

    hSocket:SET-READ-RESPONSE-PROCEDURE("SocketIO").
    RETURN "".
END PROCEDURE.



/* -------------------------------------------------------------------------- */
/* SocketIO                                                                   */
/* -------------------------------------------------------------------------- */
PROCEDURE SocketIO:
    DEFINE VARIABLE iPortNumber     AS INTEGER     NO-UNDO.
    DEFINE VARIABLE cLigneRecue     AS CHARACTER   NO-UNDO.
    DEFINE VARIABLE i               AS INTEGER     NO-UNDO.
    DEFINE VARIABLE cMessage        AS CHARACTER   NO-UNDO.
    DEFINE VARIABLE iMessageSize    AS INTEGER     NO-UNDO.
    DEFINE VARIABLE cMessageBox     AS CHARACTER   NO-UNDO.

    DEFINE BUFFER bttMess        FOR ttMess.


    ASSIGN iPortNumber = SELF:LOCAL-PORT.

    FIND FIRST bttMess WHERE bttMess.iIdx = iPortNumber NO-ERROR.
    IF NOT AVAILABLE bttMess THEN RETURN "".

    /* On quit asked then kill compilers */
    IF gKillCompil THEN DO:
        ASSIGN  cMessage        = "DIE"
                iMessageSize    = LENGTH(cMessage) + 1.
        IF GET-SIZE(mHeader) > 0 THEN
            PUT-LONG(mHeader, 1)    = iMessageSize.
        SELF:WRITE(mHeader,1,4) NO-ERROR.

        IF iDataSize < iMessageSize THEN DO:
            IF GET-SIZE(mData) > 0 THEN
                PUT-STRING(mData,1)     = "".
            ASSIGN
                SET-SIZE(mData)         = 0
                SET-SIZE(mData)         = iMessageSize
                iDataSize               = iMessageSize
                SET-BYTE-ORDER(mData)   = BIG-ENDIAN.
        END.

        IF GET-SIZE(mData) > 0 THEN
            PUT-STRING(mData,1)                 = cMessage.
        SELF:WRITE(mData,1, iMessageSize) NO-ERROR.
        RETURN "".
    END.

    /* Lecture des données en retour */
    /* Récupération des données dans une variable */
    /* Format : KOouOK|[FileName|ERROR-ROW|ERROR-COL|NUM-MESSAGES|ERROR-STATUS:GET-MESSAGE(1)|... */
    /* ou Identification Ident */
    SELF:READ(mHeader,1,4,2) NO-ERROR.
    ASSIGN iMessageSize = GET-LONG(mHeader,1).
    IF iDataSize < iMessageSize THEN DO:
        IF GET-SIZE(mData) > 0 THEN
            PUT-STRING(mData,1)     = "".
        ASSIGN  SET-SIZE(mData)         = 0
                SET-SIZE(mData)         = iMessageSize
                iDataSize               = iMessageSize
                SET-BYTE-ORDER(mData)   = BIG-ENDIAN.
    END.

    SELF:READ(mData,1,iMessageSize,2) NO-ERROR.
    IF GET-SIZE(mData) > 0 THEN
        ASSIGN cLigneRecue = GET-STRING(mData,1).

    /* Demande d'identification -> Envoi du premier élément */
    /********************************************************/
    IF cLigneRecue = "Ident" THEN DO:
        FIND FIRST temp_fic WHERE temp_fic.rversion = bttMess.cversion AND temp_fic.module = "SRV" NO-ERROR.
        IF AVAILABLE temp_fic THEN
            ASSIGN  bttMess.rRowid  = ROWID(temp_fic).
    END.
    /* Retour de compilation -> Récupération des erreurs et envoi de l'élément suivant */
    /***********************************************************************************/
    ELSE IF cLigneRecue > "" THEN DO:
        /* Sélection du fichier courant */
        FIND FIRST temp_fic WHERE ROWID(temp_fic) = bttMess.rRowid NO-ERROR.

        IF AVAILABLE(temp_fic) THEN DO:  /* DEBUG */
            ASSIGN iConnexionCount = iConnexionCount + 1.

            /* Ecriture de la log */
            IF NUM-ENTRIES(cLigneRecue, "#") > 1 THEN DO:
                IF ENTRY(1,cLigneRecue, "#") = "KO" THEN DO:
                    ASSIGN cMessageBox =    "** ERREUR de compilation dans " + ENTRY(2,cLigneRecue, "#") + " ligne " +
                                            ENTRY(3,cLigneRecue, "#") + " colonne " + ENTRY(4,cLigneRecue, "#").
                END.
                ELSE
                    ASSIGN cMessageBox = "** WARNING de compilation".

                IF ENTRY(5,cLigneRecue, "#") <> "" THEN DO:
                    DO i = 1 TO /*INTEGER(ENTRY(5,cLigneRecue, "#"))*/ NUM-ENTRIES(cLigneRecue, "#") - 5 :
                        ASSIGN cMessageBox =    cMessageBox +
                                                "~n    Message: " + ENTRY(5 + i,cLigneRecue, "#") +
                                                FILL(" ", 92 - LENGTH(ENTRY(5 + i,cLigneRecue, "#"))) +
                                                (IF i = INTEGER(ENTRY(5,cLigneRecue, "#")) THEN "**" ELSE "").
                    END.
                END.

                IF ENTRY(1,cLigneRecue, "#") = "KO" THEN
                    MESSAGE cMessageBox VIEW-AS ALERT-BOX ERROR BUTTONS OK TITLE temp_fic.rversion + " - " + {&APP_TITLE}.
                ELSE
                    MESSAGE cMessageBox VIEW-AS ALERT-BOX INFO BUTTONS OK TITLE temp_fic.rversion + " - " + {&APP_TITLE}.
            END.

            /* Sélection du prochain fichier à compiler */
            FIND NEXT temp_fic WHERE temp_fic.rversion = bttMess.cVersion AND temp_fic.module = "SRV" NO-ERROR.
            IF AVAILABLE temp_fic THEN
                bttMess.rRowid = ROWID(temp_fic).
        END.
        ELSE DO:
            TEMP-TABLE bttMess:WRITE-XML("FILE", "d:~\temp~\compil_tool_debug_ttMess.xml", TRUE) NO-ERROR.
            TEMP-TABLE temp_fic:WRITE-XML("FILE", "d:~\temp~\compil_tool_debug_temp_fic.xml", TRUE) NO-ERROR.

            MESSAGE "** Erreur interne : ce cas ne devrait jamis se produire." SKIP(1)
                    "Si vous le rencontrez, c'est une preuve selon laquelle l'outil est mal codé." SKIP
                    "Alors, à vos fouets !" SKIP(1)
                    "Dans la pratique, on ne trouve pas l'enregistrement temp_fic (ou oublié de le remplir)." SKIP
                    "La rcherche se faisait avec le ROWID " + QUOTER(bttMess.rRowid) SKIP(1)
                    "Les TT ont été déchargées en XML dans le répertoire d:~\temp~\"
                    VIEW-AS ALERT-BOX WARNING.
        END.
    END.
    ELSE
        MESSAGE "** Erreur interne : rien n'a été reçu sur le canal socket." VIEW-AS ALERT-BOX WARNING.

    IF AVAILABLE temp_fic THEN DO:
        ASSIGN cMessage  = temp_fic.src + "|" + temp_fic.repdest.

        /* Ecriture du fichier dans le socket */
        iMessageSize    = LENGTH(cMessage) + 1.
        IF GET-SIZE(mHeader) > 0 THEN
            ASSIGN PUT-LONG(mHeader, 1) = iMessageSize.
        SELF:WRITE(mHeader,1,4) NO-ERROR.

        IF iDataSize < iMessageSize THEN DO:
            IF GET-SIZE(mData) > 0 THEN
                ASSIGN PUT-STRING(mData,1)     = "".
            ASSIGN  SET-SIZE(mData)         = 0
                    SET-SIZE(mData)         = iMessageSize
                    iDataSize               = iMessageSize
                    SET-BYTE-ORDER(mData)   = BIG-ENDIAN.
        END.

        IF GET-SIZE(mData) > 0 THEN
            ASSIGN PUT-STRING(mData,1)      = cMessage.
        SELF:WRITE(mData,1, iMessageSize) NO-ERROR.
    END.
    ELSE DO:
        SELF:DISCONNECT().
        bttMess.hSocketServer:DISABLE-CONNECTIONS().
        DELETE OBJECT bttMess.hSocketServer.

        DELETE bttMess.

        /* delete server */
        ASSIGN  cMessage        = "DIE"
                iMessageSize    = LENGTH(cMessage) + 1.
        IF GET-SIZE(mHeader) > 0 THEN
            ASSIGN PUT-LONG(mHeader, 1)    = iMessageSize.
        SELF:WRITE(mHeader,1,4) NO-ERROR.

        IF iDataSize < iMessageSize THEN DO:
            IF GET-SIZE(mData) > 0 THEN
                ASSIGN PUT-STRING(mData,1)     = "".
            ASSIGN  SET-SIZE(mData)         = 0
                    SET-SIZE(mData)         = iMessageSize
                    iDataSize               = iMessageSize
                    SET-BYTE-ORDER(mData)   = BIG-ENDIAN.
        END.

        IF GET-SIZE(mData) > 0 THEN
            ASSIGN PUT-STRING(mData,1)      = cMessage.
        SELF:WRITE(mData,1, iMessageSize) NO-ERROR.

        /* if another connection is active then return  */
        IF CAN-FIND(FIRST bttMess ) THEN
            RETURN "".
    END.

    RETURN "".
END PROCEDURE.
