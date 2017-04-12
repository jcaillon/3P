&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12
&ANALYZE-RESUME
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS Include 
/*------------------------------------------------------------------------
    File        : 
    Purpose     :

    Syntax      :

    Description :

    Author(s)   :
    Created     :
    Notes       :
  ----------------------------------------------------------------------*/
/*          This .W file was created with the Progress AppBuilder.      */
/*----------------------------------------------------------------------*/

/* ***************************  Definitions  ************************** */

&GLOBAL-DEFINE version {version.i}
&GLOBAL-DEFINE edition Easter Egg
&GLOBAL-DEFINE build {build.i}

&GLOBAL-DEFINE QUERYSEP CHR(1, SESSION:CPINTERNAL, "UTF-8")
&GLOBAL-DEFINE timerStart PUBLISH "timerCommand" ("start", ENTRY(1,PROGRAM-NAME(1)," ")).
&GLOBAL-DEFINE timerStop  PUBLISH "timerCommand" ("stop" , ENTRY(1,PROGRAM-NAME(1)," ")).

/* Constant values for update channels */
&GLOBAL-DEFINE CHECK-MANUAL 0
&GLOBAL-DEFINE CHECK-STABLE 1
&GLOBAL-DEFINE CHECK-BETA   2

/* Constant for collecting statistics */
&GLOBAL-DEFINE PINGBACKURL https://goo.gl/24deK3

DEFINE VARIABLE gcThisProcedure AS CHARACTER   NO-UNDO.

/* TT for field data to link DataDiggers to each other */
DEFINE TEMP-TABLE ttLinkInfo NO-UNDO RCODE-INFORMATION
  FIELD cField AS CHARACTER
  FIELD cValue AS CHARACTER
  INDEX idxPrim IS PRIMARY cField
  .

/* TT for the tables of a db */
DEFINE TEMP-TABLE ttTable NO-UNDO RCODE-INFORMATION
  FIELD cDatabase     AS CHARACTER LABEL "DB"        FORMAT "X(12)"
  FIELD cTableName    AS CHARACTER LABEL "Table"     FORMAT "X(32)"
  FIELD cCrc          AS CHARACTER LABEL "CRC"
  FIELD cCacheId      AS CHARACTER LABEL "CacheId"
  FIELD lShowInList   AS LOGICAL   LABEL "" /* for getTablesWithField */
  FIELD cTableDesc    AS CHARACTER LABEL "Desc"      
  FIELD cFields       AS CHARACTER LABEL "Fields"
  FIELD lHidden       AS LOGICAL   LABEL ""
  FIELD lFrozen       AS LOGICAL   LABEL ""
  FIELD iNumQueries   AS INTEGER   LABEL "#"         FORMAT "zzzzz"
  FIELD tLastUsed     AS DATETIME  LABEL "Last Used" FORMAT "99/99/9999 HH:MM:SS"
  FIELD lFavourite    AS LOGICAL   LABEL ""
  FIELD lCached       AS LOGICAL   LABEL "" /* for preCaching */
  FIELD iFileNumber   AS INTEGER   LABEL "_File-Number"
  FIELD cCategory     AS CHARACTER LABEL "Category"
  INDEX idxPrim IS PRIMARY cDatabase cTableName
  INDEX idxSec cTableName
  .
DEFINE TEMP-TABLE ttTableXml NO-UNDO XML-NODE-NAME "ttTable" LIKE ttTable RCODE-INFORMATION .

/* TT for the saved queries of a table */
DEFINE TEMP-TABLE ttQuery NO-UNDO RCODE-INFORMATION
  FIELD cDatabase AS CHARACTER
  FIELD cTable    AS CHARACTER
  FIELD iQueryNr  AS INTEGER
  FIELD cQueryTxt AS CHARACTER
  INDEX idxQueryPrim IS PRIMARY iQueryNr
  .

/* TT for the fields of a table */
DEFINE TEMP-TABLE ttField NO-UNDO RCODE-INFORMATION
  FIELD cTableCacheId AS CHARACTER /* unique name for db / table / table-crc */
  FIELD cDatabase     AS CHARACTER
  FIELD cTableName    AS CHARACTER
  FIELD cFieldName    AS CHARACTER                   LABEL "Name"      FORMAT "X(40)"
                     
  FIELD cFullName     AS CHARACTER                   LABEL "Name"      FORMAT "X(40)"    /* fieldname incl extent     */
  FIELD cXmlNodeName  AS CHARACTER                   LABEL "Xml Name"  FORMAT "X(40)"    /* name for usage in XML     */
  FIELD iOrder        AS DECIMAL                     LABEL "Order"     FORMAT ">>>>>9"   /* user defined order        */
  FIELD lShow         AS LOGICAL                     LABEL ""                            /* toggle box                */
  FIELD cDataType     AS CHARACTER                   LABEL "Type"      FORMAT "X(16)"
  FIELD cInitial      AS CHARACTER                   LABEL "Initial"                     /* initial value from dict   */
  FIELD cFormat       AS CHARACTER                   LABEL "Format"    FORMAT "X(80)"    /* user defined format       */
  FIELD cFormatOrg    AS CHARACTER                   LABEL "Format"                      /* original format           */
  FIELD cLabel        AS CHARACTER                   LABEL "Label"     FORMAT "X(24)"
  FIELD iOrderOrg     AS DECIMAL                                                         /* original order            */
  FIELD iExtent       AS INTEGER                     LABEL "Extent"    FORMAT ">>>>9"
  FIELD lPrimary      AS LOGICAL                     LABEL "Prim"                        /* part of prim index?       */
  FIELD lMandatory    AS LOGICAL                     LABEL "Man"                         /* mandatory?                */
  FIELD lUniqueIdx    AS LOGICAL                     LABEL "Uni"                         /* part of unique index?     */
  
  /* New fields as per v19 */
  FIELD cColLabel     AS CHARACTER                   LABEL "Column Label" FORMAT "x(24)"
  FIELD iDecimals     AS INTEGER                     LABEL "Decimals"     FORMAT ">>9"
  FIELD iFieldRpos    AS INTEGER                     LABEL "R-pos"        FORMAT ">>>>9"
  FIELD cValExp       AS CHARACTER                   LABEL "Val Expr"     FORMAT "x(80)"
  FIELD cValMsg       AS CHARACTER                   LABEL "Val Message"  FORMAT "x(80)"
  FIELD cHelp         AS CHARACTER                   LABEL "Help msg"     FORMAT "x(80)"
  FIELD cDesc         AS CHARACTER                   LABEL "Description"  FORMAT "x(80)"
  FIELD cViewAs       AS CHARACTER                   LABEL "View-As"      FORMAT "x(40)"

  /* These fields must be moved to ttColumn */
/*  FIELD lDataField    AS LOGICAL                                                         /* show in data browse */ */
  FIELD cFilterValue  AS CHARACTER
  FIELD cNewValue     AS CHARACTER                   LABEL "New value" FORMAT "x(256)"
  FIELD cOldValue     AS CHARACTER                   LABEL "Old value" FORMAT "x(256)"
  FIELD hColumn       AS HANDLE

  INDEX idxPrim IS PRIMARY cTableCacheId
  INDEX idxName cFieldName
  INDEX idxOrder iOrder /* for fields browse */
  INDEX idxSec   cTableCacheId cFieldName iOrder
  .

/* TT for the fields of a table with extent fields extracted
 *
 * Relations:  [ttTable] -< [ttField] -< [ttColumn]
 * For non-extents the relation between ttField and ttColumn
 * will be 1:1. For extent fields it will be 1:n
 */
DEFINE TEMP-TABLE ttColumn NO-UNDO RCODE-INFORMATION
  FIELD cTableCacheId AS CHARACTER /* unique name for db / table / table-crc */
  FIELD cDatabase     AS CHARACTER
  FIELD cTableName    AS CHARACTER
  FIELD cFieldName    AS CHARACTER          LABEL "Name"      FORMAT "X(40)"
  FIELD iExtent       AS INTEGER            LABEL "Extent"    FORMAT ">>>>9"

  FIELD cFullName     AS CHARACTER          LABEL "Name"      FORMAT "X(40)"    /* fieldname incl extent     */
/*  FIELD lDataField    AS LOGICAL            /* show in data browse */ */
  FIELD cFilterValue  AS CHARACTER          /* for setting shadow color */
  FIELD cNewValue     AS CHARACTER          LABEL "New value" FORMAT "X(256)" /* for wEdit */
  FIELD cOldValue     AS CHARACTER          LABEL "Old value" FORMAT "X(256)" /* for wEdit */
  FIELD lShow         AS LOGICAL                                              /* for wEdit */
  FIELD iOrder        AS DECIMAL            LABEL "Order"     FORMAT ">>>>>9" /* user defined order        */
  FIELD cLabel        AS CHARACTER          LABEL "Label"     FORMAT "X(24)"
  FIELD iColumnNr     AS INTEGER            /* order in the databrowse */
  FIELD hColumn       AS HANDLE             /* handle to the column in the databrowse */
  FIELD hFilter       AS HANDLE             /* handle to the filter on top of the databrowse */
  INDEX idxPrim IS PRIMARY cTableCacheId
  INDEX idxField cFieldName
  INDEX idxColNr iColumnNr
  INDEX idxSort  cTableCacheId cFieldName iColumnNr
  .

/* TTs Used for preCaching */
DEFINE TEMP-TABLE ttFieldCache NO-UNDO LIKE ttField RCODE-INFORMATION
  INDEX idxTable IS PRIMARY cTableName
  .
DEFINE TEMP-TABLE ttColumnCache NO-UNDO LIKE ttColumn RCODE-INFORMATION
  .
  
DEFINE DATASET dsFields FOR ttField, ttColumn.
DEFINE DATASET dsFieldCache FOR ttFieldCache, ttColumnCache.

/* TT for the indexfields of a table */
DEFINE TEMP-TABLE ttIndex NO-UNDO RCODE-INFORMATION
  FIELD cIndexName   AS CHARACTER          LABEL "Name"        FORMAT "x(20)"
  FIELD cIndexFlags  AS CHARACTER          LABEL "Flags"       FORMAT "x(14)"
  FIELD cIndexFields AS CHARACTER          LABEL "Fields"      FORMAT "x(160)"
  FIELD cFieldList   AS CHARACTER          LABEL "Field List"  FORMAT "x(80)"
  FIELD lIndexActive AS LOGICAL            
  .

/* TT for counting windowLocks  (WindowsUpdateLock) */
DEFINE TEMP-TABLE ttWindowLock NO-UNDO RCODE-INFORMATION
  FIELD hWindow      AS HANDLE 
  FIELD iLockCounter AS INTEGER 
  INDEX idxPrim IS PRIMARY hWindow
  .

/* TT for filters on top of data browse */
DEFINE TEMP-TABLE ttFilter NO-UNDO RCODE-INFORMATION
  FIELD cFieldName as character
  FIELD hFilter    AS HANDLE
  FIELD hColumn    AS HANDLE
  FIELD hBrowse    AS HANDLE
  FIELD lVisible   AS LOGICAL
  INDEX idxBrowse hBrowse
  INDEX idxField  cFieldName
/*  FIELD hColumnHandle AS HANDLE */
/*  INDEX idxPrim IS PRIMARY cFieldName */
/*  INDEX idxFieldHandle  hFieldHandle  */
/*  INDEX idxColumnHandle hColumnHandle */
  .

/* TT for filter on database tables */
DEFINE TEMP-TABLE ttTableFilter NO-UNDO RCODE-INFORMATION
  FIELD cTableNameShow  AS CHARACTER
  FIELD cTableNameHide  AS CHARACTER
  FIELD cTableFieldShow AS CHARACTER
  FIELD cTableFieldHide AS CHARACTER
  FIELD lShowNormal     AS LOGICAL INITIAL TRUE
  FIELD lShowSchema     AS LOGICAL
  FIELD lShowVst        AS LOGICAL
  FIELD lShowSql        AS LOGICAL
  FIELD lShowOther      AS LOGICAL
  FIELD lShowHidden     AS LOGICAL
  FIELD lShowFrozen     AS LOGICAL
  .

/* TT For currently connected databases */
DEFINE TEMP-TABLE ttDatabase NO-UNDO RCODE-INFORMATION
  FIELD cLogicalName  AS CHARACTER column-label "Logical Name" FORMAT "x(20)"
  FIELD cSection      AS CHARACTER column-label "Section"      FORMAT "x(20)"
  FIELD cCacheStamp   AS CHARACTER column-label "CacheStamp"   FORMAT "x(24)"
  INDEX idxPrim IS PRIMARY unique cLogicalName
  .

/* TT for favourites */
DEFINE TEMP-TABLE ttConnection NO-UNDO RCODE-INFORMATION
  FIELD iConnectionNr as integer
  FIELD cLogicalName  AS CHARACTER column-label "Logical Name" FORMAT "x(20)"
  FIELD cDescription  AS CHARACTER column-label "Description"  FORMAT "x(28)"
  FIELD cDatabaseName AS CHARACTER column-label "Database"     FORMAT "x(20)"
  FIELD cParameters   as character
  FIELD lConnected    AS LOGICAL   column-label "Con"
  FIELD cSection      AS CHARACTER column-label "Section"      FORMAT "x(20)"
  INDEX idxPrim IS PRIMARY unique iConnectionNr
  .

/* TT for Query Tester */
DEFINE TEMP-TABLE ttTestQuery NO-UNDO RCODE-INFORMATION
  FIELD iId        AS INTEGER LABEL "Seq" COLUMN-LABEL "Seq" FORMAT ">,>>9"
  FIELD cProgName  AS character
  FIELD cQuery     AS CHARACTER
  FIELD cIndexInfo AS CHARACTER
  INDEX idxId IS PRIMARY UNIQUE iId DESCENDING
  .

/* TT for ini-file settings */
DEFINE TEMP-TABLE ttConfig NO-UNDO RCODE-INFORMATION
  FIELD cSection as character
  FIELD cSetting as character
  FIELD cValue   as character
  INDEX idxPrim IS PRIMARY cSection cSetting.


/* TT for sorting options in user query */
DEFINE TEMP-TABLE ttQuerySort NO-UNDO RCODE-INFORMATION
  FIELD iGroup     AS INTEGER /* 1:query, 2:browse */
  FIELD iSortNr    AS INTEGER
  FIELD cSortField AS CHARACTER
  FIELD lAscending AS LOGICAL
  FIELD iExt       AS INTEGER
  INDEX iPrim IS PRIMARY iGroup iSortNr
  .

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */



/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* *********************** Procedure Settings ************************ */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
   Type: Include
   Allow: 
   Frames: 0
   Add Fields to: Neither
   Other Settings: INCLUDE-ONLY
 */
&ANALYZE-RESUME _END-PROCEDURE-SETTINGS

/* *************************  Create Window  ************************** */

&ANALYZE-SUSPEND _CREATE-WINDOW
/* DESIGN Window definition (used by the UIB) 
  CREATE WINDOW Include ASSIGN
         HEIGHT             = 6
         WIDTH              = 35.8.
/* END WINDOW DEFINITION */
                                                                        */
&ANALYZE-RESUME

 


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Include 


/* ***************************  Main Block  *************************** */

/* Global-defines for testing purposes */
&if defined(invar) = 0 &then
   &if defined(UIB_is_Running) ne 0  &then
      &global-define invar  variable
      &global-define iovar  variable
      &global-define outvar variable
   &else
      &global-define invar   input parameter
      &global-define iovar   input-output parameter
      &global-define outvar  output parameter
   &endif
&endif

/* Forward defs */
function addConnection returns logical
  ( pcDatabase as character
  , pcSection  AS CHARACTER ) in super.

function formatQueryString returns character
  ( input pcQueryString as character
  , input plExpanded    AS LOGICAL ) in super.

function getDatabaseList returns character in super.

function getEscapedData returns character
  ( input pcTarget as character
  , input pcString AS CHARACTER ) in super.

function getColor returns integer
  ( input pcName AS CHARACTER ) in super.

function getColumnLabel returns character
  ( input phFieldBuffer AS HANDLE ) in super.
  
function getFont returns integer
  ( pcFontName AS CHARACTER ) in super.

function getImagePath returns character 
  ( pcImage AS CHARACTER ) in super.

function getIndexFields returns character
  ( input pcDatabaseName as character
  , input pcTableName    as character
  , input pcFlags        as character
  ) in super.

function getKeyList      returns character in super.

function getLinkInfo          returns character
  ( input pcFieldName as character
  ) in super.

function getMatchesValue returns character
  ( hFillIn AS HANDLE ) in super. 

function getMaxLength    returns integer   
  ( pcSection as character
  ) in super.

function getOsErrorDesc returns character
  ( input piOsError as integer
  ) in super.

function getProgramDir returns character in super.

function getQuery returns character
  ( input pcDatabase as character
  , input pcTable    as character
  , input piQuery    as integer
  ) in super.

function getReadableQuery returns character
  ( input pcQuery AS CHARACTER 
  ) in super. 

function getRegistry returns character 
  ( pcSection as character
  , pcKey     AS CHARACTER 
  ) in super.

function getStackSize returns intege 
  () in super.

function getTableList returns character
  ( input  pcDatabaseFilter   as character
  , input  pcTableFilter      as character
  ) in super.

function getUsername returns character in super.

function getWidgetUnderMouse returns handle
  ( input phWidget AS HANDLE ) in super.
  
function isDefaultFontsChanged returns logical in super. 

function isFileLocked returns logical 
  ( pcFileName AS CHARACTER ) in super. 

function isBrowseChanged returns logical
  ( input phWidget AS HANDLE ) in super.

function isMouseOver returns logical
  ( input phWidget AS HANDLE ) in super.

FUNCTION isTableFilterUsed RETURNS LOGICAL
  ( INPUT TABLE ttTableFilter )  in super.
  
function isWidgetChanged returns logical
  ( input phWidget AS HANDLE ) in super.
  
FUNCTION readFile RETURNS LONGCHAR
  ( INPUT pcFilename AS CHARACTER) IN SUPER.
  
function removeConnection returns logical
  ( pcDatabase AS CHARACTER ) in super.

function resolveOsVars returns character
  ( pcString AS CHARACTER ) in super. 

function setFilterFieldColor returns logical
  ( phWidget AS HANDLE ) in super.

function setLinkInfo returns logical 
  ( input pcFieldName as character
  , input pcValue     as character
  ) in super. 

function setRegistry returns character 
    ( pcSection as character
    , pcKey     as character
    , pcValue   as character
    ) in super.

FUNCTION isValidCodePage RETURNS LOGICAL
  (pcCodepage AS CHARACTER) in super.

/* Initialize */
gcThisProcedure = THIS-PROCEDURE:FILE-NAME.
gcThisProcedure = ENTRY(NUM-ENTRIES(gcThisProcedure,"\"),gcThisProcedure,"\").
gcThisProcedure = ENTRY(1,gcThisProcedure,".").

SUBSCRIBE TO gcThisProcedure ANYWHERE RUN-PROCEDURE "getProcHandle".

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE getProcHandle Include 
PROCEDURE getProcHandle :
/*
 * Name : getProcHandle
 * Desc : Return the handle of the procedure this include is in
 */
 DEFINE OUTPUT PARAMETER phHandle AS HANDLE NO-UNDO.
 phHandle = THIS-PROCEDURE:HANDLE.

END PROCEDURE. /* getProcHandle */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME