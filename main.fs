\ Main routines for loading and running the Z-machine

\ Main interpreter loop.
: interp ( -- )
;

VARIABLE file-name-addr
VARIABLE file-name-len

\ Loads the named file into the machine.
: load-file ( -- )
;

\ Runs the restart process and sets the Z-machine running.
: restart ( -- )
  true-seed
  \ ...
  load-file
;


\ Sets the file name into file-name-{addr,len} for later use.
: parse-file-name ( -- )
;

\ Main routine
parse-file-name
restart

