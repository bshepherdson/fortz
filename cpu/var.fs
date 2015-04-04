\ VAR instructions

\ Stack effect: ( argN ... arg2 arg1 n-args -- )

32 array VAROPS

\ call_vs routine args... -> (result)
:noname true zcall ; 0 VAROPS !

\ storew array word-index value
:noname ( value index array 3 -- ) drop ba swap 2 * + w! ; 1 VAROPS !

\ storeb array byte-index value
:noname ( value index array 3 -- ) drop ba          + b! ; 2 VAROPS !

\ put_prop obj prop value
:noname ( value prop obj 3 -- )
  drop prop-find ( value ra-prop )
  dup prop-size  ( value ra-prop size )
  1 = IF prop-data b! ELSE prop-data w! THEN
; 3 VAROPS !



\ Converts the given string to lowercase (ASCII only).
: lower-case ( c-addr len -- )
  over + swap DO
    i c@ dup 65 91 within IF
      32 + i c!
    ELSE drop THEN
  LOOP
;

\ v1-3: sread text parse
\ v4:   sread text parse time routine
\ v5:   aread text parse time routine -> (result)
: v3read ( parse text 2 -- )
  drop ( parse text )
  print-status-line
  \ Slightly hacky, since I'm asking Forth to write into the Z-machine's buffer.
  ba dup 1+ ram over b@ ( parse text c-addr maxlen )
  2dup accept           ( parse text c-addr maxlen len )
  nip 2dup lower-case   ( parse text c-addr len )
  2dup + 1+ 0 swap b!   ( parse text len ) \ Write 0 terminator

  \ Only parse if the parse buffer is nonzero.
  >r over IF 1+ r> parse-line ELSE r> drop 2drop THEN
;

: v4read ( routine time parse text n-args -- )
  >r \ Set aside the number of args for later
  2 v3read ( routine time ) \ maybe
  r> 2 -
  BEGIN dup WHILE nip REPEAT
;


\ Calls into v4read for now.
\ TODO Needs support for other terminating characters.
: v5read ( routine time parse text n-args -- )
  >r \ Set aside the arg count.
  \ Slightly hacky, since I'm asking Forth to write into the Z-machine's buffer.
  ba dup 1+ ram over b@ ( parse text c-addr maxlen )
  2dup accept           ( parse text c-addr maxlen len )
  nip 2dup lower-case   ( parse text c-addr len )
  nip                   ( parse text len )
  2dup swap 1+ b!       ( parse text len ) \ Write read legth into text buffer

  \ Only parse if the parse buffer is nonzero.
  >r over IF 2 + r> parse-line ELSE r> drop 2drop THEN
  r> 2 - ( n-args' )
  BEGIN dup WHILE nip REPEAT

  \ Always returning 13 for now.
  13 zstore
;





