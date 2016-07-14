&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION {&name} Procedure 
FUNCTION {&name} RETURNS {&Type}
  ( /* parameter-definitions */ ) :
/*------------------------------------------------------------------------------
  Summary    :     
  Parameters : <none>
  Returns    : 
  Remarks    :       
------------------------------------------------------------------------------*/

    DEFINE VARIABLE retVal AS {&Type} NO-UNDO.

    RETURN retVal.

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME