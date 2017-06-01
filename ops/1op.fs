\ 1OP opcode implementations

16 ARRAY 1ops ( a -- )

\ jz a ?(label)
\ Branches if a is 0.
:noname ( a -- ) 0 = zbranch ; 0 1ops !

\ get_sibling object -> (result) ?(label)
\ Gets the object number of the sibling of this object, and stores it.
\ Branches when it exists (ie. is nonzero).
:noname ( a -- ) zobject >sibling rel@   dup zstore zbranch ; 1 1ops !

\ get_child object -> (result) ?(label)
\ Gets the object number of the child of this object, and stores it.
\ Branches when it exists (ie. is nonzero).
:noname ( a -- ) zobject >child rel@   dup zstore zbranch ; 2 1ops !

\ get_parent object -> (result)   NB get_parent doesn't branch
\ Gets the object number of the parent of this object, and stores it.
\ Note that this one doesn't branch like the other two.
:noname ( a -- ) zobject >parent rel@   zstore ; 3 1ops !



\ get_prop_len prop-address -> (result)
\ Given a property address, store its length in bytes.
\ NB: This gets a data address, converts to entry, and uses that.
:noname ( ra-data -- ) prop>entry prop-size zstore ; 4 1ops !


\ General helper for inc/dec. Takes the variable number and delta, and returns
\ the new value.
: incdec ( var delta -- val' ) over var@ +   dup >r   swap var!   r> ;

\ inc (var)
:noname ( var -- )  1 incdec drop ; 5 1ops !
\ dec (var)
:noname ( var -- ) -1 incdec drop ; 6 1ops !


\ print_addr ba-string
\ Note that this prints from a byte address, which is not so useful.
\ TODO PRINTING
:noname ( ba-str -- ) ba zstr-decode   decoded-string type ; 7 1ops !


\ call_1s routine -> (result)
\ To set up for the call, I need: argv argc ra-routine ret?
:noname ( pa -- )   >r  0 0  r> pa-r true zcall ; 8 1ops !


\ remove_obj object
\ Removes the given object from the tree, so it has no parent or sibling, but
\ keeps its children.
:noname ( obj -- ) obj-remove ; 9 1ops !


\ print_obj object
\ Prints the short name of the given object.
:noname ( obj -- ) zobject obj-short-name print-zstr ; $a 1ops !


\ ret value
\ Returns the value indicated from this routine.
:noname ( val -- ) zreturn ; $b 1ops !

\ jump ?(label)
\ This is NOT a Branch instruction.
\ Instead, the argument is a 2-byte signed offset to apply to the PC.
\ It has the same -2 effect seen elsewhere, though.
:noname ( offset -- ) signed 2 -   pc +! ; $c 1ops !

\ print_paddr pa
:noname ( pa-s -- ) pa-s print-zstr ; $d 1ops !


\ load (var) -> (result)
\ The value of the variable whose number is given is stored.
\ NB: This is effectively a PEEK, if the variable is 0 == sp.
:noname ( var -- ) ?dup IF var@ ELSE peek THEN zstore ; $e 1ops !


\ not value -> (result)       v1-4
\ call_1n routine             v5+
:noname ( x -- )
  version 5 >= IF \ call_1n
    >r 0 0 r> pa-r false zcall
  ELSE \ not
    invert $ffff and   zstore
  THEN
; $f 1ops !

