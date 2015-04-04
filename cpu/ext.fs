\ EXT instructions
\ Stack effect is ( argN ... arg2 arg1 n-args -- ), same as VAR.

30 array EXTOPS

\ save table bytes name prompt -> (result)
\ TODO Implement save properly.
\ This version simply fails.
:noname discard-args 0 zstore ; 0 EXTOPS !

\ restore table bytes name prompt -> (result)
\ TODO Implement restoer properly.
\ This version simply fails.
:noname discard-args 0 zstore ; 1 EXTOPS !


\ log_shift number places -> (result)
:noname ( places number 2 -- )
  drop swap  ( number places )
  dup 0< IF negate rshift ELSE lshift THEN ( number' )
  zstore
; 2 EXTOPS !

\ art_shift numer places -> (result)
\ Simply signs the number first. Then the unused high two bytes become FFFF and
\ can be shifted in.
:noname ( places number 2 -- )
  drop signed swap  ( number places )
  dup 0< IF negate rshift ELSE lshift THEN ( number' )
  zstore
; 3 EXTOPS !

\ set_font font -> (result)
\ TODO Implement font changing.
:noname ( font 1 -- ) 2drop 0 zstore ; 4 EXTOPS !


\ save_undo -> (result)
\ Returns -1 for "unable to provide this feature".
:noname ( 0 -- ) drop -1 zstore ; 5 EXTOPS !

\ restore-undo -> (result)
\ Also returns -1. Behavior is undefined if no previous save_undo.
:noname ( 0 -- ) drop -1 zstore ; 6 EXTOPS !





