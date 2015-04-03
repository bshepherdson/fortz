\ 1OP instructions
\ The stack effect for 1OPs is always ( value -- )
16 array 1OPS

\ jz a - jump if a is 0.
:noname 0= zbranch ; 0 1OPS !

\ get_sibling obj, get_child and get_parent. NB: get_parent doesn't branch.
:noname zobject sibling relative@ dup zstore zbranch ; 1 1OPS !
:noname zobject child   relative@ dup zstore zbranch ; 2 1OPS !
:noname zobject parent  relative@     zstore         ; 3 1OPS !

\ get_prop_len ( ba-data ) Given the byte-address of an object's property's data
\ this returns the length of the data field. Requires working backward.
\ Returns 0 when passed 0, as a special case.
:noname dup 0= IF zstore ELSE prop-data>prop prop-size zstore THEN ;


