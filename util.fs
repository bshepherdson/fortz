\ Utility functions for Fortz.

: array ( n "name" -- ) ( name: n -- a-addr )
  create cells allot DOES> swap cells + ;

: carray ( n "name" -- ) ( name: n -- c-addr )
  create allot align DOES> + ;

: not ( ? -- ? ) 0= ;

: hex. base @ >R hex . R> base ! ;

: d>s drop ;

: cmove move ;

: dump-bytes ( c-addr u -- ) 0 DO dup i + c@ hex. LOOP cr drop ;

VARIABLE log-file

\ File handle for the stderr stream, usable with the file-access word set.
c-call fdopen
2 S" w" >cstring   fdopen ccall2   CONSTANT stderr

\ Helpers for debugging to stderr.
: type-err ( addr u -- ) stderr write-file abort" Failed to write to stderr" ;
: cr-err ( -- ) 10 pad c!   pad 1 type-err ;
: .err ( x -- ) 0 <# 32 hold #s #> type-err ;
: hex.err ( x -- ) base @ >r hex   .err   r> base ! ;

