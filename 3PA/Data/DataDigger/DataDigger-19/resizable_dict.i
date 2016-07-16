/*------------------------------------------------------------------------
  Name         : resizable_dict.i
  Description  : Find the dictionary window and make it resizable.
  
  Note: This code comes from AbHack, a fine tool made by
        Sebastien Lacroix. He donated this code for the DataDigger.
        Many thanks to Sebastien and you should really try AbHack!
  
  ----------------------------------------------------------------------
  08-03-2012 pti Created
  ----------------------------------------------------------------------*/

DEFINE VARIABLE hDictFrame     AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_Browse_Stat AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_btn_Create  AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_btn_Dbs     AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_btn_Delete  AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_btn_Flds    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_btn_Idxs    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_btn_Props   AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_btn_Seqs    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_btn_Tbls    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_fil_Dbs     AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_fil_Flds    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_fil_Idxs    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_fil_Seqs    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_fil_Tbls    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_lst_Dbs     AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_lst_Flds    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_lst_Idxs    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_lst_Seqs    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_lst_Tbls    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_txt_DBs     AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_txt_Flds    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_txt_Idxs    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_txt_Seqs    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hs_txt_Tbls    AS HANDLE  NO-UNDO.
DEFINE VARIABLE hwDict         AS HANDLE  NO-UNDO.


procedure resizeDictWindow:
  define variable cActiveWindowTitle as character   no-undo.

  /* Find the dictionary window */
  cActiveWindowTitle = active-window:title no-error.  

  IF VALID-HANDLE(hwDict) AND hwDict:TITLE BEGINS "Data Dictionary " THEN DO:
      /*05-DEC-2006 sla: get rid off scrollbar that may come when choosing index/field or buttons*/
      DEFINE VARIABLE iDontCare AS INTEGER    NO-UNDO.
      IF VALID-HANDLE(hDictFrame) THEN RUN ShowScrollBar (hDictFrame:HWND, 3, 0, OUTPUT iDontCare).
      RETURN.
  END.

  IF cActiveWindowTitle = "Data Dictionary" THEN DO:
      /*06-DEC-2006 sla: Error if not connected to any database => wait until we leave the dialog-box*/
      DEFINE VARIABLE hDictionaryDialog AS HANDLE NO-UNDO.
      hDictionaryDialog = FOCUS:FRAME NO-ERROR.
      IF VALID-HANDLE(hDictionaryDialog)
       AND hDictionaryDialog:TYPE = "DIALOG-BOX"
       THEN RETURN.
      hwDict = ACTIVE-WINDOW.
      RUN refineDictWidget.
      RETURN.
  END.
  IF hwDict <> ? THEN ASSIGN hwDict = ?.
end procedure. /* resizeDictWindow */


PROCEDURE DictResized :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
DEFINE VARIABLE iDiffHeight AS INTEGER  NO-UNDO.
DEFINE VARIABLE iDiffWidth  AS INTEGER  NO-UNDO.

iDiffWidth  = ( hwDict:WIDTH-PIXELS - hDictFrame:WIDTH-PIXELS ) / 3.
iDiffHeight = hwDict:HEIGHT-PIXELS - hDictFrame:HEIGHT-PIXELS.
hDictFrame:SCROLLABLE = YES.

IF iDiffHeight > 0 THEN ASSIGN
 hDictFrame:HEIGHT-PIXELS         = hwDict:HEIGHT-PIXELS
 hDictFrame:VIRTUAL-HEIGHT-PIXELS = hwDict:HEIGHT-PIXELS.

IF iDiffHeight <> 0 THEN ASSIGN
 hs_lst_Dbs:HEIGHT-PIXELS  = hs_lst_Dbs:HEIGHT-PIXELS + iDiffHeight
 hs_lst_Tbls:HEIGHT-PIXELS = hs_lst_Tbls:HEIGHT-PIXELS + iDiffHeight
 hs_lst_Seqs:HEIGHT-PIXELS = hs_lst_Seqs:HEIGHT-PIXELS + iDiffHeight
 hs_lst_Flds:HEIGHT-PIXELS = hs_lst_Flds:HEIGHT-PIXELS + iDiffHeight
 hs_lst_Idxs:HEIGHT-PIXELS = hs_lst_Idxs:HEIGHT-PIXELS + iDiffHeight

 hs_btn_Create:Y           = hs_btn_Create:Y + iDiffHeight
 hs_btn_Props:Y            = hs_btn_Props:Y + iDiffHeight
 hs_btn_Delete:Y           = hs_btn_Delete:Y + iDiffHeight
 hs_Browse_Stat:Y          = hs_Browse_Stat:Y + iDiffHeight.

IF iDiffHeight < 0 THEN ASSIGN
 hDictFrame:VIRTUAL-HEIGHT-PIXELS = hwDict:HEIGHT-PIXELS
 hDictFrame:HEIGHT-PIXELS         = hwDict:HEIGHT-PIXELS.

IF iDiffWidth > 0 THEN ASSIGN
 hDictFrame:WIDTH-PIXELS         = hwDict:WIDTH-PIXELS
 hDictFrame:VIRTUAL-WIDTH-PIXELS = hwDict:WIDTH-PIXELS.

IF iDiffWidth <> 0 THEN ASSIGN
 hs_lst_Dbs:WIDTH-PIXELS  = hs_lst_Dbs:WIDTH-PIXELS + iDiffWidth
 hs_txt_Dbs:WIDTH-PIXELS  = hs_lst_Dbs:WIDTH-PIXELS
 hs_fil_Dbs:WIDTH-PIXELS  = hs_lst_Dbs:WIDTH-PIXELS

 hs_lst_Tbls:X            = hs_lst_Tbls:X + iDiffWidth
 hs_lst_Tbls:WIDTH-PIXELS = hs_lst_Tbls:WIDTH-PIXELS + iDiffWidth
 hs_txt_Tbls:X            = hs_lst_Tbls:X
 hs_fil_Tbls:X            = hs_lst_Tbls:X
 hs_txt_Tbls:WIDTH-PIXELS = hs_lst_Tbls:WIDTH-PIXELS
 hs_fil_Tbls:WIDTH-PIXELS = hs_lst_Tbls:WIDTH-PIXELS

 hs_lst_Seqs:X            = hs_lst_Seqs:X + iDiffWidth
 hs_lst_Seqs:WIDTH-PIXELS = hs_lst_Seqs:WIDTH-PIXELS + iDiffWidth
 hs_lst_Seqs:X            = hs_lst_Tbls:X
 hs_fil_Seqs:X            = hs_lst_Tbls:X
 hs_lst_Seqs:WIDTH-PIXELS = hs_lst_Tbls:WIDTH-PIXELS
 hs_fil_Seqs:WIDTH-PIXELS = hs_lst_Tbls:WIDTH-PIXELS

 hs_lst_Flds:X            = hs_lst_Flds:X + 2 * iDiffWidth
 hs_lst_Flds:WIDTH-PIXELS = hs_lst_Tbls:WIDTH-PIXELS
 hs_txt_Flds:X            = hs_lst_Flds:X
 hs_fil_Flds:X            = hs_lst_Flds:X
 hs_txt_Flds:WIDTH-PIXELS = hs_lst_Flds:WIDTH-PIXELS
 hs_fil_Flds:WIDTH-PIXELS = hs_lst_Flds:WIDTH-PIXELS

 hs_lst_Idxs:X            = hs_lst_Flds:X
 hs_lst_Idxs:WIDTH-PIXEL  = hs_lst_Tbls:WIDTH-PIXELS
 hs_fil_Idxs:X            = hs_lst_Flds:X
 hs_fil_Idxs:WIDTH-PIXELS = hs_lst_Tbls:WIDTH-PIXELS

 hs_btn_Tbls:X            = hs_lst_Tbls:X
 hs_btn_Seqs:X            = hs_lst_Tbls:X + hs_btn_Tbls:WIDTH-PIXELS + 10
 hs_btn_Flds:X            = hs_lst_Flds:X
 hs_btn_Idxs:X            = hs_lst_Flds:X + hs_btn_Flds:WIDTH-PIXELS + 10

 hs_btn_Create:X          = hs_btn_Create:X + ( iDiffWidth * 3 ) / 2
 hs_btn_Props:X           = hs_btn_Create:X + hs_btn_Create:WIDTH-PIXELS + 5
 hs_btn_Delete:X          = hs_btn_Props:X + hs_btn_Props:WIDTH-PIXELS + 5
 hs_Browse_Stat:X         = hs_btn_Delete:X + hs_btn_Delete:WIDTH-PIXELS + 5.

IF iDiffWidth < 0 THEN ASSIGN
 hDictFrame:VIRTUAL-WIDTH-PIXELS = hwDict:WIDTH-PIXELS
 hDictFrame:WIDTH-PIXELS         = hwDict:WIDTH-PIXELS.


/* no scrollbar when sizing down plzzz */
hDictFrame:SCROLLABLE            = NO.
hDictFrame:VIRTUAL-HEIGHT-PIXELS = hDictFrame:HEIGHT-PIXELS.
hDictFrame:VIRTUAL-WIDTH-PIXELS  = hDictFrame:WIDTH-PIXELS.

END PROCEDURE.

PROCEDURE refineDictWidget :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

DEFINE VARIABLE hfg AS HANDLE     NO-UNDO.
DEFINE VARIABLE h   AS HANDLE     NO-UNDO.

hDictFrame = hwDict:FIRST-CHILD. /* 1st frame */
hfg = hDictFrame:FIRST-CHILD. /* 1st field group */

hwDict:RESIZABLE = YES.
hwDict:MAX-WIDTH-PIXELS = SESSION:WORK-AREA-WIDTH-PIXELS.
hwDict:MIN-WIDTH-PIXELS = hwDict:WIDTH-PIXELS.
hwDict:MAX-HEIGHT-PIXELS = SESSION:WORK-AREA-HEIGHT-PIXELS.
hwDict:MIN-HEIGHT-PIXELS = hwDict:HEIGHT-PIXELS. /* that should have been done at the very beginning */
ON 'WINDOW-RESIZED':U OF hwDict PERSISTENT RUN DictResized IN THIS-PROCEDURE.

h = hfg:FIRST-CHILD.
DO WHILE h <> ?:
    CASE h:NAME:
        WHEN "s_DbLbl2"         THEN  hs_txt_Dbs     = h.
        WHEN "s_DbFill"         THEN  hs_fil_Dbs     = h.
        WHEN "s_lst_Dbs"        THEN  hs_lst_Dbs     = h.

        WHEN "s_Lvl1Lbl"        THEN  hs_txt_Tbls    = h.
        WHEN "s_TblFill"        THEN  hs_fil_Tbls    = h.
        WHEN "s_lst_Tbls"       THEN  hs_lst_Tbls    = h.
        
        WHEN "s_SeqFill"        THEN  hs_fil_Seqs    = h.
        WHEN "s_lst_Seqs"       THEN  hs_lst_Seqs    = h.
        
        WHEN "s_FldFill"        THEN  hs_fil_Flds    = h.
        WHEN "s_Lvl2Lbl"        THEN  hs_txt_Flds    = h.
        WHEN "s_lst_Flds"       THEN  hs_lst_Flds    = h.

        WHEN "s_IdxFill"        THEN  hs_fil_Idxs    = h.
        WHEN "s_lst_Idxs"       THEN  hs_lst_Idxs    = h.
        
        WHEN "s_icn_Dbs"        THEN  hs_btn_Dbs     = h.
        WHEN "s_icn_Tbls"       THEN  hs_btn_Tbls    = h.
        WHEN "s_icn_Seqs"       THEN  hs_btn_Seqs    = h.
        WHEN "s_icn_Flds"       THEN  hs_btn_Flds    = h.
        WHEN "s_icn_Idxs"       THEN  hs_btn_Idxs    = h.

        WHEN "s_btn_Create"     THEN  hs_btn_Create  = h.
        WHEN "s_btn_Props"      THEN  hs_btn_Props   = h.
        WHEN "s_btn_Delete"     THEN  hs_btn_Delete  = h.
        WHEN "s_Browse_Stat"    THEN  hs_Browse_Stat = h.
    END CASE.
    h = h:NEXT-SIBLING.
END.

/* show that there is something new... */
APPLY 'MOUSE-SELECT-DOWN' TO hs_btn_Flds.
hwDict:HEIGHT-PIXELS = hwDict:HEIGHT-PIXELS + 200.
hwDict:WIDTH-PIXELS = hwDict:WIDTH-PIXELS + 50.
APPLY 'WINDOW-RESIZED' TO hwDict.

hwDict:TITLE = hwDict:TITLE + "  (made resizable by Seb's ABHack)".

END PROCEDURE.