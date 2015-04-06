\ Main routines for loading and running the Z-machine

\ Main interpreter loop.
: interp ( -- )
;

VARIABLE story-file

\ Loads the named file into the machine.
: load-file ( c-addr u -- )
  R/O BIN OPEN-FILE ABORT" Could not open story file" ( fileid )
  story-file !
;

\ Loads the story file from disk into memory.
\ Illegal to call when the story-file is not a valid, open fileid.
: reload-file ( -- )
  0 ram ( c-addr )
  story-file @ FILE-SIZE ABORT" Failed to read file size" ( c-addr dsize )
  d>s ( c-addr size ) \ We can assume the files are not > 4GB!

  0 0 story-file @ ( c-addr size   0 0 fileid )
  REPOSITION-FILE ABORT" Failed to reposition to 0" ( c-addr size )
  story-file @ READ-FILE ( u ior )
  ABORT" Failed to read the story file into memory." ( bytes-read )
  ." Read " . ." bytes of story file." cr ( )
;


\ Sets the deferred word pa to the appropriate calculation, based on version.
: init-packed-addrs ( -- )
  version
  dup 8  = IF drop ['] pa-late  IS pa EXIT THEN
  dup 6 >= IF ABORT" Versions 6 and 7 are not supported. Use v3-5 or v8." THEN
      4 >= IF drop ['] pa-mid   IS pa EXIT THEN
  ['] pa-early IS pa
;


\ Runs the restart process and sets the Z-machine running.
\ restart
:noname ( -- )
  \ Reset some internal state.
  init-packed-addrs
  true-seed
  stack-top sp !

  reload-file
  init-header-bits
; IS restart


\ Main routine
\ parse-file-name
\ restart

\ For testing
S" Zork1.z3" load-file reload-file

