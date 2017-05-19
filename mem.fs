\ Memory access words.
\ The memory buffer is defined in core.fs, and words to access it are defined
\ here.

\ First, basic reading and writing functions. These consume "real addresses";
\ see below on addressing.
: b@ ( ra -- b ) (zmem) + c@ ;
: b! ( b ra -- ) (zmem) + c! ;

\ The Z-machine story files are stored big-endian.
: w@ ( ra -- w ) dup b@ 8 lshift   swap 1+ b@ or ;
: w! ( w ra -- ) over 8 rshift over b!    >R $ff and R> 1+ b! ;

\ Returns the version of the story file being read (1-5, 7 or 8).
: version ( -- v ) hdr-version b@ ;



\ Addressing:
\ There are five flavours of address on the Z-machine:
\ ba: Byte addresses - literal 16-bit addresses, limited to the lower 64KB.
\ wa: Word addresses - 16-bit addresses, shifted right by one. Used only in
\     abbreviations tables. Limited to low 128KB.
\ pa or pa-s: Packed addresses (strings) - 16-bit address of a string in high
\    memory.  Multiplied by 2, 4, or 8. A base address is added on v7 only.
\ pa or pa-r: Packed addresses (routines) - 16-bit address of a routine in high
\    memory. Multiplied by 2, 4, or 8. A base address is added on v7 only.
\    NB: Only v7 makes a distinction between strings and routines.
\ ra: Real addresses - Cell-sized addresses to the exact byte. Effectively the
\     index into (zmem).

\ Here are provided conversion functions for these types.
\ We ignore the packed types for now until we have the basics ready.

: ba ( ba -- ra ) ;
: wa ( ba -- ra ) 2 * ;

\ In versions 1-3, packed addresses are shifted by 1.
: (pa-3) ( pa -- ra ) 1 lshift ;
\ In 4 and 5, shifted by 2.
: (pa-5) ( pa -- ra ) 2 lshift ;
\ In version 7, use the string and routine offsets to compute a packed address.
: (pa-s-7) ( pa-s -- ra ) 2 lshift   hdr-strings-offset  w@ 3 lshift + ;
: (pa-r-7) ( pa-r -- ra ) 2 lshift   hdr-routines-offset w@ 3 lshift + ;
\ In version 8, shifted by 3.
: (pa-8) ( pa -- ra ) 3 lshift ;

\ Handles the non-7 cases.
: (pa) ( pa -- ra ) version dup 8 = IF drop (pa-8) ELSE
  4 < IF (pa-3) ELSE (pa-5) THEN THEN ;

: pa-s ( pa-s -- ra ) version 7 = IF (pa-s-7) ELSE (pa) THEN ;
: pa-r ( pa-r -- ra ) version 7 = IF (pa-r-7) ELSE (pa) THEN ;


\ Now some helpers for handling PC and the stack.
\ Reads a byte from PC without advancing it.
: pc@ ( -- b ) pc @ b@ ;
: pc++ ( -- ) 1 pc +! ;
: pc@+ ( -- b) pc@ pc++ ;

\ Reading a word from PC.
: pc@w  ( -- w ) pc @ w@ ;
: pc@w+ ( -- w ) pc@w   2 pc +! ;

\ Handling the stack, which is treated as full-descending.
: peek ( -- w ) sp @ @ $ffff and ;
: push ( w -- ) sp @ 1 cells -   dup sp !    ! ;
: pop  ( -- w ) peek   1 cells sp +! ;

