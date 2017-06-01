\ 2OP instructions

32 ARRAY 2ops   ( b a -- )

\ 0 is not defined - it's a different format?

\ je a b ?(label)
\ Branch if the operands are equal. This is the straight 2op version.
:noname ( b a -- )   = zbranch ; 1 2ops !

\ jl a b ?(label)
\ Branch if a < b, treating them as signed.
:noname ( b a -- ) 2signed swap < zbranch ; 2 2ops !

\ jg a b ?(label)
\ Branch if a > b, treating them as signed.
:noname ( b a -- ) 2signed swap > zbranch ; 3 2ops !


\ dec_chk (var) value ?(label)
\ Decrements the value in (var), and branches if it is now less than my value.
:noname ( val var -- ) -1 incdec ( limit value ) 2signed > zbranch ; 4 2ops !

\ inc_chk (var) value ?(label)
\ Increments the value in (var), and branches if it is now greater than value.
:noname ( val var -- )  1 incdec ( limit value ) 2signed < zbranch ; 5 2ops !


\ jin a b ?(label)
\ Branch if a is a direct child of b, ie. if parent of a is b.
:noname ( b a -- ) zobject >parent rel@ = zbranch ; 6 2ops !


\ test map flags ?(label)
\ Branch if ALL the flags in the bitmap are set.
:noname ( flags map -- ) over and = zbranch ; 7 2ops !

\ or  a b -> (result)
:noname ( b a -- ) or  zstore ; 8 2ops !
\ and a b -> (result)
:noname ( b a -- ) and zstore ; 9 2ops !


\ test_attr obj attr ?(label)
\ Branch if the given attribute is set on the object.
:noname ( attr obj -- ) zobject attr? zbranch ; $a 2ops !

\ set_attr obj attr
\ Set the given attribute on this object.
:noname ( attr obj -- ) zobject attr+ ; $b 2ops !

\ clear_attr obj attr
\ Clear the given attribute on this object.
:noname ( attr obj -- ) zobject attr- ; $c 2ops !



\ store (var) value
\ Stores the given value into the variable whose number is provided.
\ This is a peek-write if (var) == sp == 0!
:noname ( value var -- ) ?dup IF var! ELSE sp @ ! THEN ; $d 2ops !


\ insert_obj obj dest
\ Removes obj from the tree, and makes it the new first child of destination.
:noname ( dest obj -- )
  dup >r
  obj-remove
  zobject
  dup >child rel@   r@ zobject >sibling rel! \ Make obj's sibling the old child.
      >child r@ swap rel! \ And make obj the new child of dest.
; $e 2ops !


\ loadw array word-index -> (result)
\ Loads the word at that index from the array, and stores it.
:noname ( wi arr -- ) swap 2 * +   w@   zstore ; $f 2ops !

\ loadb array byte-index -> (result)
:noname ( bi arr -- ) + b@ zstore ; $10 2ops !


\ get_prop obj prop -> (result)
:noname ( prop obj -- ) zobject swap prop-read zstore ; $11 2ops !

\ get_prop_addr obj prop -> (result)
\ Returns the DATA address for this property, or 0 if not found.
:noname ( prop obj -- )
  zobject swap prop-find   ?dup IF prop-data THEN zstore ; $12 2ops !

\ get_next_prop obj prop -> (result)
\ Returns the NUMBER of the next property.
:noname ( prop obj -- )
  zobject swap prop-find   prop-next   prop-num zstore ; $13 2ops !



\ Arithmetic

\ add a b -> (result)
:noname ( b a -- ) 2signed + clip zstore ; $14 2ops !

\ sub a b -> (result)
:noname ( b a -- ) 2signed swap - clip zstore ; $15 2ops !

\ mul a b -> (result)
:noname ( b a -- ) 2signed * clip zstore ; $16 2ops !

\ div a b -> (result)
\ NB: The SM/REM does the correct calculation.
:noname ( b a -- ) 2signed s>d rot sm/rem zstore drop ; $17 2ops !

\ mod a b -> (result)
\ NB: The SM/REM does the correct calculation.
:noname ( b a -- ) 2signed s>d rot sm/rem drop zstore ; $18 2ops !


\ call_2s routine arg1 -> (result)
:noname ( arg routine -- )
  >r
  0 argv !   0 argv   1  r> pa-r   true ( argv argc routine ret? )
  zcall
; $19 2ops !


\ call_2n routine arg1
:noname ( arg routine -- )
  >r
  0 argv !   0 argv   1  r> pa-r   false ( argv argc routine ret? )
  zcall
; $1a 2ops !


\ set_colour fg bg
\ TODO PRINTING
:noname ( bg fg -- ) 2drop ; $1b 2ops !

\ throw value stack-frame
\ TODO Adjust for save format. Needs adjustment for Quetzal to work. SAVE
\ The throw/catch value now is the OFFSET of the FP relative to stack-top.
:noname ( stack-frame value -- )
  swap (zstack-top) swap - fp !   ( val ) zreturn ; $1c 2ops !


