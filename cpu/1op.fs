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
:noname dup 0= IF zstore ELSE prop-data>prop prop-size zstore THEN ; 4 1OPS !

\ inc var-ref
:noname dup var@ 1+ swap var! ; 5 1OPS !

\ dec var-ref
:noname dup var@ 1- swap var! ; 6 1OPS !


\ print_addr
:noname ba print-string ; 7 1OPS !

\ call_1s routine -> (result)
:noname pa 1 true zcall ; 8 1OPS !

\ remove_obj object
:noname object-remove ; 9 1OPS !

\ print_obj object
:noname zobject short-name print-string ; 10 1OPS !

\ ret value
:noname zreturn ; 11 1OPS !

\ jump ?(label) - not a branch! 2-byte signed offset.
:noname signed 2 - pc+ ; 12 1OPS !

\ print_paddr addr
:noname pa print-string ; 13 1OPS !

\ load (var) -> (result)
:noname dup 0= IF drop sp @ @ ELSE var@ THEN zstore ; 14 1OPS !

\ v1-4: not value -> (result)
\ v5:   call_1n routine
:noname
  version 4 <= IF \ not
    invert $ffff and zstore
  ELSE \ call_1n
    pa 1 false zcall
  THEN
; 15 1OPS !

