\ Helpers for handling the Z-machines tree of objects.

\ TODO Many of these version-dependent values could be stored as VALUEs that are
\ set once instead of expensively looked up. The version and object table don't
\ change once the game is running.

\ Returns the address of the object table (which starts with property defaults)
: object-table-raw ( -- ra-table ) hdr-objtable w@ ba ;
: property-defaults ( -- ra ) object-table-raw ;
: object-table ( -- ra-table ) object-table-raw 62 126 3or5 + ;

: object-size ( -- size-in-bytes ) 9 14 3or5 ;

\ Field addresses for the relatives.
: parent  ( ra-obj -- ra-relative ) 4 6  3or5 + ;
: sibling ( ra-obj -- ra-relative ) 5 8  3or5 + ;
: child   ( ra-obj -- ra-relative ) 6 10 3or5 + ;

: relative@ ( ra-relative -- value ) version 3 <= IF b@ ELSE w@ THEN ;
: relative! ( value ra-relative -- ) version 3 <= IF b! ELSE w! THEN ;

\ Turns an object from its address to the property table address.
\ Note that this points at the slot in the object record holding the pointer.
\ You probably want prop-table (address of the first property) or short-name.
: prop-table-addr ( ra-obj -- ra-prop-table-ptr ) 7 12 3or5 + w@ ba ;

\ Address of the first property in the object's property table.
: prop-table ( ra-obj -- ra-properties )
  prop-table-addr dup b@  ( base len )
  2 * + 1+
;

\ Address of the Z-machine string for the object's short name.
: short-name ( ra-obj -- ra-string ) prop-table-addr 1+ ;

\ Turns an object number into its RA.
: zobject ( num -- ra-obj ) 1- object-size *   object-table + ;


\ Given the address of a property, returns its number.
: prop-number ( ra-prop -- num )
  b@   31 63 3or5  and
;

\ Given the address of a property, returns its "inner" size. That is, its
\ data length, not including the one or two size/number bytes.
: prop-size ( ra-prop -- size-in-bytes )
  version 3 <= IF
    b@ 5 rshift 1+
  ELSE
    dup b@ dup 128 and ( ra-prop first-byte first-bit? )
    IF \ Two size and number bytes.
      drop 1+ b@ 63 and ( size )
      dup 0= IF drop 64 THEN \ Special case: 0 is treated as 64.
    ELSE \ Only one. bit 6 is set for 2 bytes, clear for 1.
      64 and IF 2 ELSE 1 THEN ( ra-prop size )
      nip
    THEN
  THEN
;

\ Given a property address, returns the address of that property's data area.
: prop-data ( ra-prop -- ra-data )
  version 3 <= IF 1+
  ELSE dup b@ 128 and IF 2 + ELSE 1+ THEN
  THEN
;

\ Given a property address, returns the address of the next property.
: prop-next ( ra-prop -- ra-prop )
  dup prop-size swap prop-data +
;

\ Works backwards from a data address to return the size.
\ Relies on the fact that bit 7 is set in both bytes of a two-byte size.
: prop-data>prop ( ra-data -- ra-prop )
  version 3 <=
  IF 1- ELSE
    1- dup b@ 128 and IF 1- THEN
  THEN
;

\ Returns the address of the (first) size byte for the given prop.
\ Returns 0 if this object does not have the property.
: prop-find ( prop obj-num -- ra-prop )
  zobject prop-table ( prop ra-table )
  swap               ( ra-table prop )
  BEGIN
    over b@ 0= IF 2drop 0 EXIT THEN
    over prop-number    ( ra-table prop num )
    over = IF drop EXIT THEN
    swap prop-next swap
  AGAIN
;


\ Fetches the property value (must be one or two bytes).
\ Returns the default if the object is missing the property.
: prop-read ( prop obj-num -- value )
  over >r   \ Set aside the prop number in case of default
  prop-find ( ra-prop )
  r>
  over 0= IF ( ra-prop prop )
    \ Properties are numbered from 1, but the defaults table is 0-based.
    nip  1- 2 * property-defaults + w@
  ELSE
    drop dup prop-data swap prop-size ( ra-data size )
    1 = IF b@ ELSE w@ THEN ( value )
  THEN
;


\ Walks the child list of the given object and returns the
\ ADDRESS of the RELATIVE FIELD wherein the second named object is found.
\ Returns 0 if the child is not found.
: find-child ( ra-this target -- ra-relative )
  over child ( this target ra-child )
  dup >r     ( this target ra-child   R: ra-child )
  relative@ ( this target child   R: ra-child )
  dup 0= IF r> 2drop 2drop ( ) 0 EXIT THEN
  over = IF ( this target ) 2drop r> EXIT THEN
  ( this target   R: ra-child )
  swap drop r>  ( target new-this )
  \ Follow the sibling chain until we find it.
  swap ( this target )
  BEGIN
    over sibling relative@ ( this target sibling )
    dup 0= IF 2drop drop 0 EXIT THEN
    over = ( this target match? )
    not
  WHILE
    swap sibling relative@ zobject swap
  REPEAT
  ( this target )
  drop sibling
;

\ Removes the given object (by number) from the tree, so it is parentless.
\ Find the parent, walk the children.
: object-remove ( num -- )
  dup zobject dup parent relative@ zobject ( num this ra-parent )
  rot swap ( this num ra-parent )
  swap find-child ( this ra-relative )
  dup 0= IF 2drop EXIT THEN
  \ Now store my sibling into that field.
  over sibling relative@ swap relative! ( this )
  \ And remove my parent.
  0 swap   parent relative!
;


\ Index of the byte in question.
: attr-index ( attr -- offset ) 3 rshift ;
: attr-mask  ( attr -- mask )   7 and 7 swap -  1 swap lshift ;
: attr@ ( attr obj -- ? )
  zobject over attr-index + ( attr ra )
  b@   swap attr-mask   and 0<>
;
: attr! ( ? attr obj -- )
  rot >r                    ( attr ra    R: ? )
  zobject over attr-index + ( attr ra )
  swap attr-mask            ( ra mask )
  over b@                   ( ra mask old )
  over invert 255 and and   ( ra mask masked )
  swap r>                   ( ra masked mask ? )
  and or                    ( ra updated )
  swap b!
;


