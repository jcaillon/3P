FUNCTION aazazazaz RETURNS CHARACTER 
	(  ):
/*------------------------------------------------------------------------------
 Purpose:
 Notes:
------------------------------------------------------------------------------*/	

		DEFINE VARIABLE result AS CHARACTER NO-UNDO.

		RETURN result.


		
END FUNCTION.




// proc structurée :

/* ************************  Function Implementations ***************** */

&IF DEFINED(EXCLUDE-fgergre) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION fgergre Procedure
FUNCTION fgergre RETURNS CHARACTER 
  (  ):
/*------------------------------------------------------------------------------
 Purpose:
 Notes:
------------------------------------------------------------------------------*/
		DEFINE VARIABLE result AS CHARACTER NO-UNDO.

		RETURN result.

END FUNCTION.
	
/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ENDIF


/* ************************  Function Prototypes ********************** */

&IF DEFINED(EXCLUDE-fgergre) = 0 &THEN

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _FUNCTION-FORWARD fgergre Procedure
FUNCTION fgergre RETURNS CHARACTER 
  (  ) FORWARD.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ENDIF



// surround with catch block:

CATCH  AS Progress.Lang.Error :

END CATCH.


FINALLY:

END FINALLY.
