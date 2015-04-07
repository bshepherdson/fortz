\ Memory accessing words for Fortz.

\ NB: All of these words deal in Z-machine addresses. Real Forth-system
\ addresses into "ram" exist only briefly, inside these functions.

\ These words convert byte, word and packed addresses into "real addresses",
\ which are indexes into ram for use with these functions.
\ These are denoted as ba, wa, pa, and ra in stack comments.
: ba ( ba -- ra ) ;
: wa ( wa -- ra ) 1 lshift ;
defer pa \ Will be filled in after the header is defined (depends on version).

\ One of these will be set into pa above, based on the version at load time.
\ v1-3 PAs are 2P
: pa-early 1 lshift ;
\ v4-5 PAs are 4P
: pa-mid   2 lshift ;
\ v8 PAs are 8P
: pa-late  3 lshift ;

\ TODO versions 6 and 7 have more complex PAs, different for routines and
\ strings. Implement them if I ever start caring about those versions.


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


\ Reads the byte under PC. DOES NOT advance it.
: pc@ ( -- u ) pc @ b@ ;

\ Adjusts PC by the given amount.
: pc+ ( n -- ) pc @ +   0x7ffff and   pc ! ;

\ Bumps PC by one.
: pc++ ( -- ) 1 pc+ ;

\ Reads the byte at PC and advances it.
: pc@+ ( -- u ) pc@ pc++ ;

\ Reads a word at PC and advances it.
: pc@w+ ( -- u ) pc@+ 8 lshift   pc@+   or ;

