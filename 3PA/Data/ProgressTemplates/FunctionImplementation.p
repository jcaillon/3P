&IF DEFINED(EXCLUDE-{&name}) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION {&name} Procedure 
FUNCTION {&name} RETURNS {&Type}
  ( /* parameter-definitions */ ) :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

    DEFINE VARIABLE retVal AS {&Type} NO-UNDO.

    RETURN retVal.

END FUNCTION.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ENDIF