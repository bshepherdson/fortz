\ Memory accessing words for Fortz.

\ NB: All of these words deal in Z-machine addresses. Real Forth-system
\ addresses into "ram" exist only briefly, inside these functions.

\ These words convert byte, word and packed addresses into "real addresses",
\ which are indexes into ram for use with these functions.
\ These are denoted as ba, wa, pa, and ra in stack comments.
: ba> ( ba -- ra ) ;
: wa> ( wa -- ra ) 1 lshift ;
defer pa \ Will be filled in after the header is defined (depends on version).

: b@ ( ra -- u ) ram c@ ;
: b! ( u ra -- ) swap 255 and swap ram c! ;
: w@ ( ra -- u )
  dup b@ 8 lshift ( ra hi )
  swap 1+ b@ or
;
: w! ( u ra -- )
  over 8 rshift 255 and over b! ( u ra )
  1+ swap 255 and swap b!
;

\ Converts an unsigned 16-bit value into a signed 32-bit one usable in Forth.
: signed ( u -- n ) dup 0x8000 and IF 0xffff0000 or THEN ;

