\ VAR format opcodes

32 ARRAY varops ( -- ) \ Args are in argv and argc.

\ call_vs routine args... -> (result)
:noname ( -- )
  1 argv   argc @ 1-   0 argv @ pa-r    true ( argv argc routine ret? )
  zcall
; 0 varops !

\ storew array word-index value
:noname ( -- ) 3 args   2 argv @   0 argv @   1 argv @ 2 * + w! ; 1 varops !

\ storeb array byte-index value
:noname ( -- ) 3 args   2 argv @   0 argv @   1 argv @ +   b! ; 2 varops !

\ put_prop obj prop value
:noname ( -- ) 3 args
  0 argv @ zobject   1 argv @   prop-find
  dup 0= ABORT" Attempted to put non-existent property."
  2 argv @   swap dup prop-data swap prop-size ( val ra-data size )
  CASE
    1 OF b! ENDOF
    2 OF w! ENDOF
    ABORT" Attempted to put to property whose size is not 1 or 2."
    2drop
  ENDCASE
; 3 varops !



\ v1-3 read: text and parse buffers
\ In these versions, the status line is redisplayed first.
: v3-read ( -- ) ( A: text parse )
  0 argv @   dup b@   >r 1+ r>   ( addr len ) accept ( len )
  \ v3 doesn't record the length parsed in the text buffer, but it's needed for
  \ parsing, so we retain it.
  \ We're supposed to write a 0 terminator, too.
  dup 0 argv @ + 1+   0 swap c! ( len )
  >r 1 argv @   0 argv @   r> ( parse text len )
  zparse ( )
;

: v4-read ( -- ) ( A: text parse [time routine] )
  v3-read \ These are identical, until I support the time/routine thing.
  \ TODO Support the time and routine arguments.
;

: v5-read ( -- ) ( A: text parse [time routine] )
  \ Byte 0 of the buffer contains the max length, byte 1 contains the total so
  \ far. We use these to compute the maximum for ACCEPT, and its start position.
  0 argv @   dup b@ over 1+ b@ ( text max i )
  dup >r - r> ( text max' i )
  swap >r 2 + + r> ( start max' )
  accept ( len )
  0 argv @ 1+   dup b@ rot +   dup >r   swap b!  \ Update the length
  1 argv @   0 argv @   r> ( parse text total-len )
  zparse ( )
  13 zstore \ TODO Other terminating characters.
;


\ v1+ sread text parse
\ v4+ sread text parse time routine
\ v5+ aread text parse time routine -> (result)
:noname
  version 3 <= IF v3-read EXIT THEN
  version 4  = IF v4-read EXIT THEN
  v5-read
;

