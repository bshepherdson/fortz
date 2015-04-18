\ String handling functions for Fortz.

\ Z-machine strings are a mess.
\ This lexicon defines a wide variety of words to read, print and manipulate
\ them.

\ General string-reading process:
\ parse-string ( ra -- c-addr u ) string-reset (parse-string) ;
  \ (Fresh strings only, not abbreviations. Those want to keep writing in-line.)
\ There are two pointers into input-buffer and one into output-buffer.
\ - ptr-expansion indicates where new Z-characters should be expanded to.
\ - ptr-input indicates the next Z-char to read.
\ - ptr-output indicates where the next output character should go.
\ These pointers are always in this order: ptr-expansion is at the highest
\ address, then ptr-input, and then ptr-output.

\ (parse-string) works in two steps.
\ - First it unpacks the whole string into Z-characters starting at
\   ptr-expansion. This run is terminated by an $FF to mark the end of each
\   string.
\ - Second, it processes the string from ptr-input to the $FF, outputting to
\   ptr-output.

here 2048 allot align CONSTANT input-buffer
here 1024 allot align CONSTANT output-buffer


VARIABLE ptr-expansion
VARIABLE ptr-input
VARIABLE ptr-output

VARIABLE shift

: string-reset ( -- )
  input-buffer  ptr-expansion !
  input-buffer  ptr-input     !
  output-buffer ptr-output    !
  0             shift         !
;


: string-expand ( zc -- )
  ptr-expansion @ c! ( )
  1 ptr-expansion +!
;

\ Read the next Z-character
: string-next-char ( -- zc )
  ptr-input @ c@
  1 ptr-input +!
;

: string-output-char ( c -- )
  ptr-output @ c!
  1 ptr-output +!
;

\ Deferred so it can be recursed into for abbreviations.
defer (parse-string)

: char-literal ( -- c )
  string-next-char 5 lshift
  string-next-char or
;

\ Lower case. a = 97
: alphabet-0 ( zc -- c ) 91 + ;
\ Upper case. A = 65
: alphabet-1 ( zc -- c ) 59 + ;

here
char 0 c,   char 1 c,   char 2 c,   char 3 c,   char 4 c,
char 5 c,   char 6 c,   char 7 c,   char 8 c,   char 9 c,
char . c,   char , c,   char ! c,   char ? c,   char _ c,
char # c,   char ' c,   34     c,   char / c,   char \ c,
char - c,   char : c,   char ( c,   char ) c,
align
CONSTANT a2-table

: alphabet-2 ( zc -- c )
  CASE
  6 OF char-literal ENDOF
  7 OF 10 ENDOF
  8 - a2-table + c@ 0 ( c 0-dummy )
  ENDCASE
;

3 array alphabets
' alphabet-0   0 alphabets  !
' alphabet-1   1 alphabets  !
' alphabet-2   2 alphabets  !

\ TODO Handle header-specified custom alphabet tables. Cf. S 3.5.5

: reset-shift 0 shift ! ;


\ Identifies and outputs (to the buffer) an abbreviation.
: abbreviation ( u -- )
  1- 32 * ( block )
  string-next-char + ( index )
  2 *     ( offset )
  hdr-abbreviations w@ ba ( offset ra )
  +     ( ra-entry )
  w@ wa ( ra-string )

  \ Need to save the input pointer, since (parse-string) will move it.
  ptr-input @ >r
  (parse-string)
  r> ptr-input !
;



\ Transforms the next chunk of input into output.
\ TODO Doesn't handle versions 1 or 2.
: process-char ( -- ? )
  string-next-char
  dup 255 = IF drop true EXIT THEN \ Bail with TRUE if end-of-string reached.
  CASE
  0   OF 32 string-output-char   reset-shift ENDOF
  1   OF 1 abbreviation reset-shift ENDOF
  2   OF 2 abbreviation reset-shift ENDOF
  3   OF 3 abbreviation reset-shift ENDOF
  4   OF 1 shift ! ENDOF
  5   OF 2 shift ! ENDOF
  \ default - alphabet output
  shift @ alphabets @ execute ( c )
  string-output-char ( )
  reset-shift
  0 \ dummy for endcase
  ENDCASE
  false \ We haven't run out of string yet.
;

: expand-word ( u -- end? )
  dup 10 rshift 31 and string-expand
  dup  5 rshift 31 and string-expand
  dup           31 and string-expand
  0x8000 and ( done? )
  dup IF 255 string-expand THEN
;

\ (parse-string)
:noname ( ra -- )
  \ Move the input pointer to match the expansion pointer.
  ptr-expansion @ ptr-input !
  \ Then work through each 2-byte word of the string.
  BEGIN dup w@   expand-word not WHILE 2 + REPEAT
  drop

  BEGIN process-char not WHILE REPEAT
; IS (parse-string)


\ Note that the returned string is only valid until another string is parsed
\ or any interpreting is done.
: parse-string ( ra -- c-addr u )
  string-reset
  (parse-string)
  \ When this is done, the string is ready in the output buffer and ptr-output
  \ is positioned just after it.
  output-buffer
  ptr-output @  output-buffer - ( buffer length )
;

: print-string ( ra -- )
  parse-string type
;

\ Returns the length of the given string IN Z-MACHINE WORDS.
\ Doesn't affect any of the buffers.
: string-length ( ra -- u )
  0 BEGIN ( ra count )
    1+ over w@ 0x8000 and not ( ra count' ? )
  WHILE
    swap 2 + swap
  REPEAT
  nip
;

\ TODO Accented and unicode characters.

: dict-header ( -- ra-dict-header ) hdr-dictionary w@ ba ;

: dict-separator? ( char -- sep? )
  dict-header dup b@ ( char ra-hdr n-seps )
  swap 1+ swap
  0 DO
    2dup I + b@ = ( char ra-hdr match? )
    IF drop UNLOOP EXIT THEN
  LOOP
  2drop false
;

: sep-or-space? ( char -- sep? )
  dup 32 = IF drop true EXIT THEN
  dict-separator?
;


: dict-n-chars ( -- length ) 6 9 3or5 ;

\ Fills the input-buffer with 9 5s for Z-char padding.
: fill-5s ( -- ) input-buffer dup 9 + swap DO 5 i c! LOOP ;

: build-z-word ( buf -- buf' word )
  dup  c@ 10 lshift    swap 1+ swap ( buf hi )
  over c@ 5  lshift or swap 1+ swap ( buf hi+mid )
  over c@           or swap 1+ swap ( buf word )
;


\ Encodes a single ASCII char into possibly multiple Z-chars.
\ These land in the input-buffer, using ptr-expansion.
\ No uppercase letters; they've been lowercased.
: encode-char ( ascii-char -- )
  dup 97 123 within IF 91 - string-expand EXIT THEN
  a2-table 24 + a2-table DO ( char )
    dup i c@ = IF 5 string-expand string-expand UNLOOP EXIT THEN
  LOOP
  \ If we're still here, it's not in A0 or A2, so output the multi-byte
  \ sequence for it.
  5 string-expand
  6 string-expand
  dup 5 rshift 31 and string-expand
               31 and string-expand
;



: dict-after-terms ( -- ra ) dict-header dup  b@ + 1+ ;
: dict-entry-size  ( -- u )  dict-after-terms b@    ;
: dict-entry-count ( -- u )  dict-after-terms 1+ w@ ;
: dict-entry-0     ( -- ra ) dict-after-terms 3  +  ;


\ Tries to look up the word whose encoded self is in output-buffer.
\ Returns 0 for non-found words.
: dict-lookup ( -- ra-dict )
  dict-entry-size 4 6 3or5 ( entry-size bytes )
  dict-entry-0  ram
  dict-entry-size dict-entry-count * ( entry-size bytes c-addr total-size )
  over + swap ( entry-size bytes c-addr-end c-addr-start )
  DO ( entry-size bytes )
    i over             ( size bytes dict-word bytes )
    output-buffer over ( size bytes dict-word bytes input-word bytes )
    compare            ( size bytes cmp )
    dup 0>= IF \ Early exit
      -rot 2drop ( cmp )
      0= IF i 0 ram - ELSE 0 THEN
      UNLOOP EXIT
    THEN
    drop ( size bytes )
  over +LOOP

  2drop 0
;

: zscii>zstring ( -- )
  input-buffer
  2 3 3or5 0 DO ( buf )
    build-z-word ( buf' word )
    dup 8 rshift
    string-output-char \ high byte
    string-output-char \ low byte
  LOOP ( buf )
  drop

  \ Update the constructed input to have the appropriate end bit.
  2 4 3or5 output-buffer + dup c@ 128 or swap c!
;

VARIABLE zm-parse   \ Current position in the user-provided parse buffer.
VARIABLE zm-input   \ Start of the word currently under consideration.
VARIABLE zm-input-0 \ Start of the input buffer, for computing offsets.

: parse-record-dict     ( -- ra-dict-field )     zm-parse @ ;
: parse-record-position ( -- ra-position-field ) zm-parse @ 3 + ;
: parse-record-length   ( -- ra-length-field )   zm-parse @ 2 + ;

: zm-input-pos ( -- offset ) zm-input @   zm-input-0 @   - ;

: ascii>zscii ( -- )
  \ Moves an ASCII character from zm-input to ZSCII characters in input-buffer.
  zm-input @ b@   encode-char
  1 zm-input +!
;

: find-non-whitespace ( text-len -- text-len' )
  BEGIN dup 0>   zm-input @ b@   32 =   and
  WHILE 1-   1 zm-input +!  REPEAT
;

: parse-word ( text-len -- text-len' )
  \ Go until we run out of string or hit a separator, copying to input-buffer.
  string-reset fill-5s
  \ Write the starting position into its parse buffer.
  zm-input-pos  1+  parse-record-position b!
  dup ( start-len current-len )
  BEGIN
    dup 0>
    zm-input @ b@   sep-or-space? not
    and
  WHILE
    1-   ascii>zscii
  REPEAT
  swap over - ( len' delta )
  parse-record-length b! ( len' )
  zscii>zstring \ output buffer is now loaded
  dict-lookup parse-record-dict w! \ and looked up in the dictionary
;

: parse-single-delimiter ( text-len -- text-len' )
  string-reset fill-5s
  zm-input-pos  parse-record-position b!
  1 parse-record-length b!
  ascii>zscii
  1-
;

: locate-word ( text-len -- text-len' )
  find-non-whitespace
  dup not IF EXIT THEN \ Bail when no string left.
  zm-input @ b@   dict-separator? IF
    \ Special case: when this word is a separator, parse just it.
    parse-single-delimiter
  ELSE
    \ Normal case: parse a word, up to space, a separator, or EOL.
    parse-word
  THEN
  4 zm-parse +!
;

\ Read the words and write parse data for them.
: parse-line ( parse text len -- )
  >r
  dup zm-input !   zm-input-0 ! ( parse )
  \ Store the first parse result's address, but save the start for later.
  dup 2 + zm-parse !
  r> ( parse0 len )
  BEGIN dup WHILE locate-word REPEAT
  drop ( parse0 )

  zm-parse @ ( parse0 parse-end )
  over 2 + -   2 rshift   ( parse0 parse-end )
  over 1+ b! \ Write number of words parsed.
  drop ( )
;
