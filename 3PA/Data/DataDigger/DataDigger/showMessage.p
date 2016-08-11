/*------------------------------------------------------------------------
  Name         : showMessage.p
  Description  : Show a user defined message in a new window.
  ---------------------------------------------------------------------- 
  16-09-2012 pti Created
  ----------------------------------------------------------------------*/

define input parameter pcTitle as character.
define input parameter pcMessage as character.
define output parameter phWindow as handle.

define variable cMessage   as character   no-undo format "x(256)".
define variable iFont      as integer     no-undo.
define variable iWidth     as integer     no-undo.
define variable winMessage as handle      no-undo.

define frame infoFrame
cMessage view-as fill-in size 1 by 1 at row 1.5 col 1.5 no-label
with 1 down no-box overlay side-labels three-d at col 1 row 1 size-pixels 50 by 40.

/* *************************  Create Window  ************************** */
create window winMessage assign
     title         = pcTitle
     width-pixels  = 260
     height-pixels = 40
     status-area   = no
     message-area  = no
     min-button    = no
     max-button    = no
     sensitive     = yes.

/* Set CURRENT-WINDOW: this will parent dialog-boxes and frames.        */
assign current-window                = winMessage.
     this-procedure:current-window = winMessage.
     default-window = winMessage.

/* Find a decent font */
do iFont = 0 to font-table:num-entries - 1:
  if    font-table:get-text-width-pixels('DataDigger',iFont) = 54
    and font-table:get-text-height-pixels(iFont) = 13 then
  do:
    frame infoFrame:font = iFont.
    leave.
  end.
end. 

/* How wide should the text be? */
iWidth = font-table:get-text-width-pixels(pcMessage,iFont) + cMessage:x + 30.
iWidth = maximum(iWidth,150).

winMessage:width-pixels = iWidth .
cMessage:width-pixels = iWidth - 10.
cMessage:screen-value = pcMessage.
frame infoFrame:width-pixels = iWidth.

/* Center the window */
winMessage:x = (session:work-area-width-pixels - winMessage:width-pixels) / 2.
winMessage:y = (session:work-area-height-pixels - winMessage:height-pixels) / 2.

/* Showtime! */
view frame infoFrame in window winMessage.
view winMessage.

process events.

phWindow = winMessage:handle.
