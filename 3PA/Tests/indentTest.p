PROCEDURE PI_NUMCPTE_PS:
    
    label1:
    CASE I_Imput:
        
        WHEN 1 OR 
        WHEN 3 OR
        WHEN 2 THEN
            MESSAGE "ok".
        WHEN 3 OR WHEN 4 THEN
        CASE i_type_cpte_FA :
            WHEN 0 THEN
                MESSAGE "ok".
            
            /* plus de N-2 */
            OTHERWISE IF TRUE THEN
                MESSAGE "ok".
        END CASE.
        WHEN 5 OR WHEN 6 THEN
            IF i_type_cpte_FA > 1 THEN
                MESSAGE "ok".
            ELSE
                MESSAGE "ok".
            
    END CASE.
    
    IF TRUE THEN
        IF FALSE THEN
            IF TRUE THEN
                MESSAGE "ok".
            ELSE
            DO:
                IF TRUE THEN 
                    MESSAGE "ok".
                ELSE MESSAGE "derp".
            END.
    MESSAGE "cool".
    
    IF TRUE THEN IF TRUE THEN
        ASSIGN fff = "".
    
    IF FALSE THEN ASSIGN 
        lcuuu = "zezfe"
        lcuuu = "zezfe".
    
    
    IF TRUE THEN 
        IF FALSE THEN 
            run 
                lcuuu = "zezfe"
                lcuuu = "zezfe"
                .
        ELSE IF TRUE THEN FOR i = 1 TO 2:
            MESSAGE i.
        END.                
    
    DEFINE
        VARIABLE
        lc_fuck 
        AS CHARACTER
        NO-UNDO.
    
    RETURN "".      
    
END PROCEDURE. /* PI_NUMCPTE_PS: */

ON CHOOSE OF bt_profileRename IN FRAME DEFAULT-FRAME /* Renommer */
DO:
    RUN pi_profileRename NO-ERROR.
END.

&SCOPED-DEFINE test zefzef~
    zefzeafza~
    zafzeafzeaf

&IF FALSE OR
    FALSE OR
    TRUE &THEN
    DEFINE VARIABLE lx AS CHARACTER.
&ELSEIF FALSE &THEN
    DEFINE VARIABLE lx AS CHARACTER.
&ELSE
    DEFINE VARIABLE lx AS CHARACTER.
&ENDIF

/* ******************************************************************** */
{common_preproc.i}

