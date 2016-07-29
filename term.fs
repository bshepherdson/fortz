\ Words for controlling and reading from the terminal.
\ Uses VT100-style escapes to control the scrolling regions and move the cursor.

here 8 cells allot CONSTANT term-buf

: read-number-to ( c )
  >R
  term-buf ( buf   R: c )
  BEGIN key dup R@ <> WHILE ( buf key )
    over c! char+
  REPEAT ( c-addr key   R: c)
  R> 2drop ( c-addr )
  0 0 rot ( ud c-addr )
  term-buf - term-buf swap  ( ud c-addr u )
  >number 2drop drop ( num )
;

: esc 27 emit ;
: esc-brack esc '[' emit ;

\ ESC[rows;colsH
: cursor-move ( row col -- ) esc-brack swap 0 .R  ';' emit   0 .R   'H' emit ;

: cursor-position ( -- row col )
  esc-brack '6' emit 'n' emit \ Request position
  \ Expect to read <ESC>[{row};{col}R
  key drop key drop
  ';' read-number-to ( row )
  'R' read-number-to ( row col )
;

\ Save/restore just the cursor position.
: cursor-save ( -- ) esc-brack 's' emit ;
: cursor-restore ( -- ) esc-brack 'u' emit ;

\ Save/restore the cursor position and attributes.
: term-save ( -- ) esc-brack '7' emit ;
: term-restore ( -- ) esc-brack '8' emit ;

\ Save the cursor position, move it to 999,999, ask for its position, restore.
: term-size ( -- rows cols )
  cursor-save
  999 999 cursor-move
  cursor-position   ( rows cols )
  cursor-restore
;

VARIABLE upper-window-size

\ Controls the scrollable area.
\ The row count here is raw - remember to leave room for the status line, if
\ any.
: resize-upper-window ( rows -- )
  \ ESC[start;endr sets the scroll region.
  dup upper-window-size !
  esc-brack    0 .R   ';' emit   999 0 .R   'r' emit
;


VARIABLE term-rows
VARIABLE term-cols
VARIABLE term-window

\ Erasing regions
\ Erases the whole current line.
\ Do so manually, with spaces, so that the background colour is honoured.
: erase-line ( -- )
  cursor-position drop ( row col )
  cursor-save 1 cursor-move ( )
  term-cols @ spaces
  cursor-restore
;

\ Erases from the cursor to the end of the line.
: erase-to-eol ( -- )
  cursor-position ( row col )
  \ 2dup .err .err cr-err
  cursor-save term-cols @ swap - ( row col-delta )
  spaces drop ( )
  cursor-restore
;

: erase-screen ( -- ) esc-brack '2' emit 'J' emit ;


here 2 cells allot CONSTANT cursor-upper
here 2 cells allot CONSTANT cursor-lower

: init-term ( -- )
  term-size term-cols ! term-rows !
  0 term-window !
  erase-screen
  999 1 cursor-lower 2!
  999 1 cursor-move
;

init-term


\ Screen attributes.
\ Both the terminal and the Z-machine can only set-one or clear-all.
: term-roman     ( -- ) esc-brack '0' emit 'm' emit ;
: term-bold      ( -- ) esc-brack '1' emit 'm' emit ;
: term-italic    ( -- ) esc-brack '4' emit 'm' emit ;
: term-reverse   ( -- ) esc-brack '7' emit 'm' emit ;

here
'9' c, \ 0 is never used; it means "current" and we skip it.
'9' c, \ 1 = default = '9'
'0' c, \ 2 = black = '0'
'1' c, \ 3 = red = '1'
'2' c, \ 4 = green = '2'
'3' c, \ 5 = yellow = '3'
'4' c, \ 6 = blue = '4'
'5' c, \ 7 = magenta = '5'
'6' c, \ 8 = cyan = '6'
'7' c, \ 9 = white = '7'
align CONSTANT colour-map

\ Z-machine colours are given as 0-9 which index into this table:
: convert-colour ( c -- char ) colour-map + c@ ;

\ Sets a single colour. The first character ('3' for foreground, '4' for
\ background) is emitted first.
: term-colour ( c char -- )
  esc-brack emit convert-colour emit 'm' emit
;

\ Colours are always set as a pair, foreground and background.
: term-colours ( bg fg -- )
  \ Z-machine colour 0 means "current", so I don't change anything.
  dup IF '3' term-colour ELSE drop THEN
  dup IF '4' term-colour ELSE drop THEN
;



\ Notes on the Z-machine screen model:
\ TODO There's a bit (5 in Flags 1) that signals whether screen splitting is
\ available, for version 3 and below.

\ By version:
\ 1-2: No cursor control or windows: status line and teletype, essentially.
\ 3: Upper and lower windows. Upper window is N+1 lines tall (status line) and
\    does not scroll. Cursor positions are independent between windows, and
\    should be saved when the program flips between them.
\ 4-5: As V3, except:
\  - More styles.
\  - Upper window cursor is reset when it's switched to.
