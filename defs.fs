\ Core helper word definitions.

\ TODO Remove this when not debugging.
: u.s ( -- ) base @ >R   16 base !   .s cr   R> base ! ;

: u.h ( u -- ) base @ swap 16 base !   . base ! ;

