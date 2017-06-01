\ Core helper word definitions.

\ TODO Remove these when not debugging.
: u.s ( -- ) base @ >R   16 base !   .s cr   R> base ! ;
: u.h ( u -- ) base @ swap 16 base !   . base ! ;


\ Defines "arrays", a block of cells. Defines a word that expects a cell index
\ and returns the address of that cell.
: ARRAY ( len "name" -- ) ( exec: i -- a-addr )
  CREATE cells allot
DOES>
  swap cells +
;

