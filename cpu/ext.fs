\ EXT instructions
\ Stack effect is ( argN ... arg2 arg1 n-args -- ), same as VAR.

30 array EXTOPS

\ save table bytes name prompt -> (result)
:noname discard-args save-game 1 zstore ; 0 EXTOPS !

\ restore table bytes name prompt -> (result)
:noname discard-args restore-game 2 zstore ; 1 EXTOPS !


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
:noname ( 0 -- ) drop -1 zstore ; 9 EXTOPS !

\ restore-undo -> (result)
\ Also returns -1. Behavior is undefined if no previous save_undo.
:noname ( 0 -- ) drop -1 zstore ; 10 EXTOPS !


\ TODO Implement Unicode support
\ print_unicode char-number
:noname discard-args ." [Unimplemented: print_unicode]" cr ; 11 EXTOPS !
\ check_unicode char-number -> (result)
:noname discard-args ." [Unimplemented: check_unicode]" cr ; 12 EXTOPS !

\ TODO Implement true color support
\ set_true_color fg bg
:noname discard-args ." [Unimplemented: set_true_color]" cr ; 13 EXTOPS !

