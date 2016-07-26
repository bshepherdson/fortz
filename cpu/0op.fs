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

: dump-pointers ( -- )
  ." PC: " pc @ hex. cr
  ." SP: " sp @ hex.   stack-top sp @ - . cr
  ." FP: " fp @ hex. cr
;

\ save (V1-3: save ?(label), V4: save and store, V5 illegal)
:noname
  version
  dup 5 >= IF ." [Illegal opcode: save in v5]" cr BYE THEN
  \ quit
  save-game
  4 >= IF 1 zstore ELSE true zbranch THEN
; 5 0OPS !

\ restore (similar to save)
:noname
  version 5 >= IF ." [Illegal opcode: restore in v5]" cr BYE THEN

  \ Preserve the transcription and fixed-width-font bits (Flags 2, bits 0 and 1)
  hdr-flags2 w@ 3 and ( bits )

  restore-game \ State is now loaded, including PC.
  \ Reset those bits to be the same as they were before the restore.
  hdr-flags2 w@ 3 invert and or   hdr-flags2 w!
  cr
  version 4 >= IF 2 zstore ELSE true zbranch THEN
; 6 0OPS !


\ restart - no prompting, just do it.
:noname restart ; 7 0OPS !


\ ret_popped
:noname pop zreturn ; 8 0OPS !

\ Throws away the top of the stack.
: 0OP_pop pop drop ;

\ Captures the current "stack frame" such that a future throw will return as
\ if from the current routine.
\ Should be simply the fp value, but that's too big to be a Z-machine value.
\ Instead, it's the distance in cells from the top of the stack to the current
\ fp.
\ TODO Needs adjustment to work with Quetzal save format. SAVE
: 0OP_catch stack-top fp @ - zstore ;

\ EITHER: pop in V4 and lower, OR catch in 5+
:noname version 5 >= IF 0OP_catch ELSE 0OP_pop THEN ; 9 0OPS !

\ quit
:noname ." Quitting. Goodbye." cr bye ; 10 0OPS !

\ new_line
:noname cr ; 11 0OPS !


\ In v1 and v2, all games are "score games". V3 uses Flags 1 bit 1 = 1 to mean
\ time.
: print-v3-status ( -- )
  version 3 =   hdr-flags1 b@ 2 and and ( time-game? )
  \ The current "room" is the object whose number is in G00, variable 16.
  16 var@ ( time-game? obj-num )
  zobject short-name ( time-game? ra-string )
  \ Save the cursor position and the colour setting.
  term-save
  \ Move the cursor to 0, 0, erase the line, and print there.
  0 0 cursor-move
  1 1 term-colours \ That's "default, default".
  term-roman term-reverse
  erase-line
  \ Now emit our line: Write out the object short name
  print-string   ( time-game? )
  \ Move to near the end, erase to EOL in case the object name is long.
  IF \ time game
    \ Time: HH:MM = 12 chars
    0   term-cols @ 14 -  cursor-move
    erase-to-eol
    2 spaces
    ." Time: "
    17 var@ 2 .R
    ':' emit
    18 var@ 2 .R
  ELSE \ score game
    \ Score: xxx  Turn: xxxx = 22 chars
    0   term-cols @ 24 -  cursor-move
    erase-to-eol
    2 spaces
    ." Score: "
    17 var@ 3 .R
    2 spaces
    ." Turn: "
    18 var@ 4 .R
  THEN
  term-restore
;

\ show_status
:noname version 3 = IF print-v3-status THEN ; 12 0OPS !

\ verify
\ TODO implement me.
:noname true zbranch ; 13 0OPS !

\ piracy
:noname true zbranch ; 15 0OPS !
