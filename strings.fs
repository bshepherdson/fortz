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

here @ CONSTANT input-buffer
2048 allot align
here @ CONSTANT output-buffer
1024 allot align

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

here @ CONSTANT a2-table
char 0 c,   char 1 c,   char 2 c,   char 3 c,   char 4 c,
char 5 c,   char 6 c,   char 7 c,   char 8 c,   char 9 c,
char . c,   char , c,   char ! c,   char ? c,   char _ c,
char # c,   char ' c,   34     c,   char / c,   char \ c,
char - c,   char : c,   char ( c,   char ) c,
align

: alphabet-2 ( zc -- c )
  CASE
  6 OF char-literal ENDOF
  7 OF 10 ENDOF
  8 - a2-table + c@
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
  1- 32 * ( index )
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
  dup 255 = IF true EXIT THEN \ Bail with TRUE if end-of-string reached.
  dup CASE
  0   OF drop 32 string-output-char   reset-shift ENDOF
  1   OF abbreviation reset-shift ENDOF
  2   OF abbreviation reset-shift ENDOF
  3   OF abbreviation reset-shift ENDOF
  4   OF 1 shift ! ENDOF
  5   OF 2 shift ! ENDOF
  \ default - alphabet output
  shift @ alphabets execute ( c )
  string-output-char ( )
  ENDCASE
  false \ We haven't run out of string yet.
;

: expand-word ( u -- end? )
  dup           31 and string-expand
  dup  5 rshift 31 and string-expand
  dup 10 rshift 31 and string-expand
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

