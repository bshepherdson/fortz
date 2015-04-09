\ 2OP instructions
\ Stack effect: ( b a -- )

31 array 2OPS

\ je a b ?(label) - jump when equal
\ This is the proper 2OP form of je, called like a 2OP.
:noname = zbranch ; 1 2OPS !

\ jl a b ?(label) - jump when a < b
:noname signed swap signed < zbranch ; 2 2OPS !

\ jg a b ?(label) - jump when a > b
:noname signed swap signed > zbranch ; 3 2OPS !

\ dec_chk (var) value ?(label) - decrement, then jump if < value
:noname ( val var -- )
  dup var@ ( val var x )
  1- 0xffff and
  2dup swap var! ( val var x )
  nip ( val x )
  signed swap <
  zbranch
; 4 2OPS !

\ inc_chk (var) value ?(label) - increment, then jump if > value
:noname ( val var -- )
  dup var@ ( val var x )
  1+ 0xffff and
  2dup swap var! ( val var x )
  nip ( val x )
  signed swap signed   >
  zbranch
; 5 2OPS !

\ jin a b ?(label) - branch if parent of a is b.
:noname ( b a ) zobject parent relative@ = zbranch ; 6 2OPS !

\ test map flags ?(label) - branch if bitmap & flags == flags
:noname ( flags map ) over and = zbranch ; 7 2OPS !

\ or a b -> (result) - bitwise OR
:noname or zstore ; 8 2OPS !
\ and a b -> (result) - bitwise AND
:noname and zstore ; 9 2OPS !


\ test_attr obj attr ?(label) - Branch if the attribute is set.
:noname ( attr obj ) attr@ zbranch ; 10 2OPS !
\ set_attr obj attr
:noname ( attr obj ) true -rot attr! ; 11 2OPS !
\ clear_attr obj attr
:noname ( attr obj ) false -rot attr! ; 12 2OPS !

\ store (var) value
:noname ( value var -- ) var! ; 13 2OPS !

\ insert_obj obj destination
:noname ( dest obj -- )
  over swap dup ( dest dest obj obj )
  object-remove ( dest dest obj ) \ obj is now parentless.
  swap zobject dup child relative@ ( dest obj ra-dest sib )
  rot ( dest ra-dest sib obj )
  dup >r
  zobject sibling relative! ( dest ra-dest   R: obj )
  r> dup >r ( dest ra-dest obj   R: obj )
  swap
  child relative! ( dest   R: obj )
  r> zobject parent relative! ( )
; 14 2OPS !


\ loadw array word-index -> (result)
:noname ( word-index array -- ) ba swap 2 * + w@ zstore ; 15 2OPS !

\ loadb array byte-index -> (result)
:noname ( byte-index array -- ) ba + b@ zstore ; 16 2OPS !

\ get_prop obj prop -> (result)
:noname ( prop obj -- ) prop-read zstore ; 17 2OPS !

\ get_prop_addr obj prop -> (result) - Returns address of the data area, or 0.
:noname ( prop obj -- ) prop-find dup 0<> IF prop-data THEN zstore ; 18 2OPS !

\ get_next_prop obj prop -> (result) - Returns number of the next property.
\ When given a property number of 0, finds the first property.
\ Returns 0 for no next property.
:noname ( prop obj -- )
  over 0= IF zobject prop-table prop-number zstore 2drop EXIT THEN
  prop-find prop-next prop-number zstore
; 19 2OPS !


: math( ( ub ua -- a b ) signed swap signed ;
: math) ( res -- ) 0xffff and zstore ;

\ add a b -> (result), sub, mul, div and mod.
:noname ( b a -- ) math( + math) ; 20 2OPS !
:noname ( b a -- ) math( - math) ; 21 2OPS !
:noname ( b a -- ) math( * math) ; 22 2OPS !

\ div a b -> (result) - sm/rem does the right kind of symmetric division.
:noname ( b a -- ) signed s>d rot signed sm/rem zstore drop ; 23 2OPS !
\ mod a b -> (result)
:noname ( b a -- ) signed s>d rot signed sm/rem drop zstore ; 24 2OPS !


\ call_2s routine arg
:noname ( arg routine -- ) pa 2 true  zcall ; 25 2OPS !
\ call_2n routine arg
:noname ( arg routine -- ) pa 2 false zcall ; 26 2OPS !

\ TODO Implement me COLOR
\ set_colour fg bg
:noname 2drop ." [Unimplemented: set_colour]" cr ; 27 2OPS !

\ TODO Adjust for save format. SAVE
\ throw value stack-frame
\ The throw/catch token here is the OFFSET of the FP relative to stack-top.
:noname ( stack-frame value -- )
  swap stack-top swap - fp ! zreturn
; 28 2OPS !

