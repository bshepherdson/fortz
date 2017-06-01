\ 0OP instructions

16 ARRAY 0ops



\ rtrue
:noname 1 zreturn ;  0 0ops !

\ rfalse
:noname 0 zreturn ;  1 0ops !

\ print (lit-string)
\ TODO Printing
:noname zstr-pc   decoded-string type ;  2 0ops !

\ print_ret (lit-string)
\ TODO Printing
:noname zstr-pc   decoded-string type  cr   1 zreturn ;  3 0ops !


\ nop
:noname ; 4 0ops !

\ save
\ In v3 and below, branches.
\ In v4, stores.
\ v5+, illegal.
\ TODO Support save/restore.
:noname
  version 3 <= IF 0 zbranch EXIT THEN
  version 4  = IF 0 zstore  EXIT THEN
  S" save (pre-v5 0OP)" illegal-opcode
; 5 0ops !


\ restore
\ In v3 and below, branches.
\ In v4, stores.
\ v5+, illegal.
:noname
  version 3 <= IF 0 zbranch EXIT THEN
  version 4  = IF 0 zstore  EXIT THEN
  S" restore (pre-v5 0OP)" illegal-opcode
; 6 0ops !


\ restart
:noname restart ; 7 0ops !

\ ret_popped
\ Pops the top of stack and returns it. Cheaper then ret sp.
:noname pop zreturn ; 8 0ops !


\ v5 handles catch.
\ Should be simply the FP value, but that's too big. Instead, the distance in
\ cells from the top of the stack to the current FP.
\ TODO Needs adjustment to work with the Quetzal save format. SAVE
: zcatch ( -- ) (zstack-top) fp @ -   zstore ;

\ EITHER: pop in v4 and lower, OR catch in 5+
\ pop simply pops and throws away the value.
:noname version 5 >= IF zcatch ELSE pop drop THEN ; 9 0ops !

\ quit
:noname ." Quitting. Goodbye." cr bye ; $a 0ops !

\ new_line
\ TODO Printing
:noname cr ; $b 0ops !

\ show_status
\ Shows the status line in v3 and lower; illegal later.
:noname
  version 3 <= IF ." (status line)" cr
  ELSE S" show_status" illegal-opcode THEN
; $c 0ops !


\ verify
\ Verifies the checksum.
\ TODO Implement properly. Always succeeds right now.
:noname 1 zbranch ; $d 0ops !


\ $e is the first byte of extended opcodes, and undefined earlier.

\ piracy
\ Supposedly ensures the game disk is genuine. Interpreters always pass.
:noname 1 zbranch ; $f 0ops !

