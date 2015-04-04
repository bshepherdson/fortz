\ 0OP instructions
16 array 0OPS

\ rtrue
:noname 1 zreturn ; 0 0OPS !
\ rfalse
:noname 0 zreturn ; 1 0OPS !

\ print (literal)
: print-literal pc @ print-string   pc @ string-length 2 * pc+ ;
' print-literal 2 0OPS !

\ print_ret (literal)
:noname print-literal cr 1 zreturn ; 3 0OPS !

\ nop
:noname ; 4 0OPS !

\ save (V1-3: save ?(label), V4: save and store, V5 illegal)
:noname
  ." [Saving not implemented]" cr
  version
  dup 5 >= IF ." [Illegal opcode: save in v5]" cr BYE THEN
      4 >= IF 0 zstore ELSE false zbranch THEN
; 5 0OPS !

\ restore (similar to save)
:noname
  ." [Saving not implemented]" cr
  version
  dup 5 >= IF ." [Illegal opcode: restore in v5]" cr BYE THEN
      4 >= IF 0 zstore ELSE false zbranch THEN
; 6 0OPS !


\ restart - no prompting, just do it.
\ TODO Maybe empty the stacks here so that the interpreter doesn't use them
\ up across multiple restarts.
:noname restart ; 7 0OPS !


\ ret_popped
:noname pop zreturn ;

\ Throws away the top of the stack.
: 0OP_pop pop drop ;

\ Captures the current "stack frame" such that a future throw will return as
\ if from the current routine.
\ Should be simply the fp value, but that's too big to be a Z-machine value.
\ Instead, it's the distance in cells from the top of the stack to the current
\ fp.
\ TODO Needs adjustment to work with Quetzal save format.
: 0OP_catch stack-top fp @ - zstore ;

\ EITHER: pop in V4 and lower, OR catch in 5+
:noname version 5 >= IF 0OP_catch ELSE 0OP_pop THEN ; 9 0OPS !

\ quit
:noname ." Quitting. Goodbye." cr bye ; 10 0OPS !

\ new_line
:noname cr ; 11 0OPS !

\ TODO Implement v3 status lines - requires console handling
: print-v3-status ( -- ) ;

\ show_status
:noname version 3 = IF print-v3-status THEN ; 12 0OPS !

\ verify
\ TODO implement me.
:noname true zbranch ; 13 0OPS !

\ piracy
:noname true zbranch ; 15 0OPS !
