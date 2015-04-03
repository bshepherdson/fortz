\ Utility functions for Fortz.

: array ( n "name" -- ) ( name: n -- a-addr )
  create cells allot DOES> swap cells + ;

: carray ( n "name" -- ) ( name: n -- c-addr )
  create allot align DOES> + ;
