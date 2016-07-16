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
         HEIGHT             = 13.05
         WIDTH              = 60.
/* END WINDOW DEFINITION */
                                                                        */
&ANALYZE-RESUME

 


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK Include 


/* ***************************  Main Block  *************************** */

run initFrames.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE initFrames Include 
PROCEDURE initFrames :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

  DELETE WIDGET {&WINDOW-NAME}.
  {&WINDOW-NAME} = CURRENT-WINDOW.

  run reparentFrames(input frame DEFAULT-FRAME:handle, input phParent).

  run enable_UI.
  
  /* Adjust the size of the frame to the rectange (if provided) */
  if valid-handle(phRectangle) then
    run setFrame ( input phRectangle:x + 2
                 , input phRectangle:y + 2             
                 , input phRectangle:width-pixels - 4
                 , input phRectangle:height-pixels - 4 
                 ).  

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE reparentFrames Include 
PROCEDURE reparentFrames :
/*------------------------------------------------------------------------------
  Name : reparentFrames
  Desc : Reparent all frames
------------------------------------------------------------------------------*/
  DEFINE INPUT  PARAMETER phOldParent AS HANDLE NO-UNDO.
  DEFINE INPUT  PARAMETER phNewParent AS HANDLE NO-UNDO.

  /* Attach all frames on the main frame to the parent */
  define variable hWidget as handle no-undo. 

  repeat:
    hWidget = phOldParent:first-child:first-child.
    if not valid-handle(hWidget) then leave.
    if hWidget:type = 'FRAME' then hWidget:frame = phNewParent.
  end.

end procedure. /* reparentFrames */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE setFrame Include 
PROCEDURE setFrame :
/*------------------------------------------------------------------------------
  Name : setFrame    
  Desc : Position the frame to a specified location with a specified size
------------------------------------------------------------------------------*/

  define input parameter piFrame-x as integer no-undo.
  define input parameter piFrame-y as integer no-undo.
  define input parameter piFrame-w as integer no-undo.
  define input parameter piFrame-h as integer no-undo.

  if piFrame-w <> ? then
  do:
    frame {&frame-name}:width-pixels = piFrame-w.
    frame {&frame-name}:virtual-width-pixels  = piFrame-w.
  end.

  if piFrame-h <> ? then 
  do:
    frame {&frame-name}:height-pixels = piFrame-h.
    frame {&frame-name}:virtual-height-pixels = piFrame-h.
  end.
  
  if piFrame-x <> ? then frame {&frame-name}:x = piFrame-x.

  if piFrame-y <> ? then frame {&frame-name}:y = piFrame-y.


end procedure. /* setFrame */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE viewFrame Include 
PROCEDURE viewFrame :
/*------------------------------------------------------------------------------
  Name : viewFrame
  Desc : Show or hide the frame 
------------------------------------------------------------------------------*/
  define input parameter plView as logical no-undo.

  frame {&frame-name}:hidden = not plView.
  if plView then apply 'entry' to frame {&frame-name}.

end procedure. /* viewFrame */

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME