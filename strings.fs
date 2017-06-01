\ String handling routines for the Z-machine.
\ These are quite involved, but fortunately also quite testable.

\ Decoding text is a stateful process, and it can be nested. Therefore, care is
\ required when nesting that the state is preserved and initialized correctly.

\ We proceed bottom-up: handling the decoding of a single Z-character, then of
\ a word containing 3 such characters, then of entire strings.

\ Output is into a buffer, which is usually not cleared when nesting strings
\ (since the nested strings are abbreviations).

CREATE zs-out 1024 allot align
VARIABLE (zs-out*)
VARIABLE (zs-scratch)

: (z>str) ( -- c-addr u ) zs-out (zs-out*) @ ;
\ Debugging helper: shorthand for "print so far".
: psf ( -- ) ." So far: " (z>str) type cr ;

\ Writes a ZSCII value into the output buffer.
: (zemit) ( zscii -- ) (zs-out*) @ zs-out + c!   1 (zs-out*) +! ;

: (populate-alphabet) ( base -- ) 26 0 DO dup i + c, LOOP drop ;

\ 78 bytes giving the three standard alphabets. A2's first two entries are
\ dummies, being treated as special cases (2-char literal and newline, resp.)
CREATE (alphabet-table)
'a' (populate-alphabet)
'A' (populate-alphabet)
  bl  c, '^' c, \ Dummy values.
  '0' c, '1' c, '2' c, '3' c, '4' c, '5' c, '6' c, '7' c, '8' c, '9' c,
  '.' c, ',' c, '!' c, '?' c, '_' c, '#' c, $27 c, '"' c,
  '/' c, '\' c, '-' c, ':' c, '(' c, ')' c,
align


\ This is handled as a state machine with three pieces of state:
\ - The (zstr-state) is a deferred word that gets adjusted accordingly.
\   It handles (1) regular characters, (2) abbreviations, (3+4) long literals.
\ - The (zstr-scratch) holds scratch data.
\   - Holds 0, 26, or 52 for regular characters, giving the alphabet.
\   - Used by long literals to hold the upper 5 bits.
\   - Used by abbreviations to hold the abbreviation block number.

\ Top-level DEFERed word for decoding a string given its real address.
\ Needs to be DEFERed so that abbreviations can use it.
\ Considered internal, since this one doesn't reset the output buffer.
DEFER (zstr-decode)

\ Rebound xt for the base state.
DEFER (zstr-regular)
\ Top-level deferred word for the state machine.
DEFER (zstr-decode-char)
VARIABLE (zstr-scratch)

\ Resets the state to regular character, and the scratch to 0.
: (reg) ( -- )
  0 (zstr-scratch) !
  ['] (zstr-regular) ['] (zstr-decode-char) DEFER!
;


\ Handles an abbreviation. Table number (1-3) is in the (zstr-scratch).
: (zstr-state-abbrev) ( zc -- )
  (zstr-scratch) @   1- 32 *   + ( number )
  2 *   hdr-abbreviations ba w@ +
  w@ wa (reg) (zstr-decode) drop
  (reg) \ Needed to make sure the state is normal and the scratch is clear.
;

\ Handles part 2 of a long literal (the lower 5 bits).
: (zstr-state-lit-2) ( zc -- )
  (zstr-scratch) @ or (zemit)
  (reg)
;

\ Handles part 1 of a long literal (upper 5 bits).
: (zstr-state-lit-1) ( zc -- )
  5 lshift   (zstr-scratch) !
  ['] (zstr-state-lit-2) ['] (zstr-decode-char) defer!
;


\ Helper that gets the alphabet table to use. Either the standard one, or if the
\ version is at least 5 and the slot in the header is nonzero, the user-supplied
\ one.
: (get-alphabet-table) ( -- ra )
  version 5 < IF (alphabet-table) EXIT THEN
  hdr-alphabet-table w@ ?dup IF ba (zmem) + ELSE (alphabet-table) THEN
;

\ Handles the basic state.
\ Special cases:
\ 0:   Space
\ 1-3: Abbreviations block 0-2, number to follow.
\ 4-5: Shift to A1 or A2.
\ 6 in A2: long literal
\ 7 in A2: newline
: (zstr-state-regular) ( zc -- )
  dup 0= IF drop $20 (zemit)   0 (zstr-scratch) ! EXIT THEN
  dup 4 < IF
    (zstr-scratch) !
    ['] (zstr-state-abbrev) ['] (zstr-decode-char) defer!
    EXIT THEN
  dup 6 < IF
    3 - 26 * (zstr-scratch) !
    EXIT THEN
  dup 6 =   (zstr-scratch) @ 52 = and IF \ 6 in A2: Long literal
    drop ['] (zstr-state-lit-1) ['] (zstr-decode-char) defer!
    EXIT THEN
  dup 7 =   (zstr-scratch) @ 52 = and IF \ 7 in A2: Newline
    drop $0a (zemit)   0 (zstr-scratch) !
    EXIT THEN

  \ Finally: regular character output.
  \ In v5+, check for a custom alphabet table.
  \ ." Emitting a real character: " u.s
  6 - (get-alphabet-table) (zstr-scratch) @ + + c@ (zemit)
  0 (zstr-scratch) !
  \ u.s
;
' (zstr-state-regular) IS (zstr-regular)


: dcdbg ( zc -- zc ) ; \ ." Decoding: " dup u.h cr ;

: (zstr-decode-word) ( w -- end? )
  \ ." Word: " dup u.h cr
  dup 10 rshift $1f and dcdbg (zstr-decode-char)
  dup  5 rshift $1f and dcdbg (zstr-decode-char)
  dup           $1f and dcdbg (zstr-decode-char)
  15 rshift 0<> \ end? bit
;

\ Decodes a string found at the given real address, into the emit buffer.
\ DOES NOT reset the output buffer, so it's usable for abbreviations.
\ Returns the length in words.
: (zstr-decode-int) ( ra -- len-words )
  dup BEGIN dup w@ (zstr-decode-word) 0= WHILE 2 + REPEAT swap - 2 / ;
' (zstr-decode-int) IS (zstr-decode)

: decoded-string ( -- c-addr u ) zs-out   (zs-out*) @ ;

\ Top-level string decoder. Resets the output buffer, and returns the result as
\ a Forth-style string.
\ NB: The returned string is volatile, and will only survive until more text is
\ decoded.
: zstr-decode ( ra -- c-addr u )
  0 (zs-out*) !   (reg)
  (zstr-decode) drop
  decoded-string
;

\ Convenience that decodes a string at PC, and adjusts PC accordingly.
: zstr-pc ( -- ) pc @   (zstr-decode)   2 * pc +! ;

