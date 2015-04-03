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

