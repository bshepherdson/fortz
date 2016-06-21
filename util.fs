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

