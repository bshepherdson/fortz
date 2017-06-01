\ Implementation of the object tree and its attached systems.

\ Takes a v3-or-lower value and a v4+ value and returns the right one.
: 3-5 ( v3 v5 -- correct ) version 3 > IF nip ELSE drop THEN ;

: obj-size   9 14 3-5 ;
: max-props 31 63 3-5 ;


\ Returns the address of the 0th object in the table (actually object 1, since
\ 0 does not exist).
: obj0 ( -- ra ) hdr-object-table w@ ba   max-props 2 * + ;

\ Looks up an object number, returning its real address.
: zobject ( obj -- ra ) 1-   obj-size *   obj0 + ;


\ Given an object address, returns the address where its parent, sibling and
\ child can be found.
: >parent  4  6 3-5 + ;
: >sibling 5  8 3-5 + ;
: >child   6 10 3-5 + ;

: rel@ ( ra -- b/w )  version 3 <= IF b@ ELSE w@ THEN ;
: rel! ( b/w ra -- )  version 3 <= IF b! ELSE w! THEN ;




\ Properties

\ Requirements for properties, coming from the opcodes, are:
\ - Get the byte address of a property by its number.
\ - Get the value of a property given its address (may be byte- or word-sized)
\ - Get the address of the next property, given one property.
\ - Get the length of a property, given its address.
\ - Write a new byte or word value into the property, given its number.

\ To that end, we use the following words:
\ prop-0 - gets the first (highest-numbered) prop's address in an object's chain
\ prop-data - returns the real address of the prop's actual data.
\ prop-size - returns the size in bytes of the prop's actual value.
\ prop-next - advances the pointer to the next one; returns 0 at end.
\ prop-num - returns the number of this property
\ prop-find - looks for the property with the given number; returns either
\   0 or its address.

\ These work together neatly, enabling these words and the opcodes to be
\ implemented cleanly.

\ Advances an object pointer to its property table address.
: >prop-table ( ra-obj -- ra ) 7 12 3-5 + ;

\ In v3 the property number is the lower 5 bits, in v5 the lower 6, of the first
\ byte of the property entry.
: prop-num ( ra-prop -- prop-num ) b@   $1f $3f 3-5 and ;

\ Returns the size of the actual data portion.
: prop-size ( ra-prop -- size )
  dup b@   version 3 <= IF nip 5 rshift 1+ EXIT THEN
  dup $80 and IF \ long form, size in second byte
    drop 1+ b@   $3f and
    dup 0= IF drop 64 THEN \ 0 is treated as 64.
  ELSE \ short form, size is 1 (bit 6 clear) or 2 (bit 6 set).
    nip $40 and IF 2 ELSE 1 THEN
  THEN
;


\ Points at the prop's data.
\ This is 1+, unless it's v5 and the top bit is set.
: prop-data ( ra-prop -- ra-data )
  version 3 >    dup b@ $80 and   and IF 1+ THEN
  1+
;

\ Finds the total length of this propert
: prop-next ( ra-prop -- ra-prop' ) dup prop-data swap prop-size + ;

: prop-0 ( ra-obj -- ra-prop )
  >prop-table w@
  dup b@ 2 * + 1+
;

\ Given an object and property number, finds that property here, or returns 0.
: prop-find ( ra-obj prop-num -- ra-prop|0 )
  swap prop-0   ( num ra-prop )
  BEGIN
    dup b@
  WHILE
    2dup prop-num = IF ( num ra-prop ) nip EXIT THEN
    prop-next
  REPEAT
  \ If we got down here, not found.
  2drop 0
;

\ Reads a 1 or 2 byte value, as appropriate, from a property address.
: (prop-read) ( ra-prop -- val )
  \ Check the size, which must be 1 or 2 bytes.
  dup prop-data swap prop-size
  dup 2 > IF ." Error: Can't read property longer than 2 bytes" cr bye THEN
  1 > IF w@ ELSE b@ THEN
;

\ Returns the address of the property defaults.
: prop-default ( prop -- val )   1- 2 *   hdr-object-table w@   + w@ ;

\ Reads a property by object address and property number. Reads from the
\ defaults table if the property is not found on this object.
: prop-read ( ra-obj prop -- val )
  dup >R
  prop-find dup IF
    (prop-read) r> drop
  ELSE
    drop r> prop-default
  THEN
;


\ Works backward from a property data address to its entry address.
\ Opcodes get the former, but the words above consume the latter.
\ This is 1- in v3, and in v5 it's 2- when the bit 7 of that byte is set.
: prop>entry  ( ra-data -- ra-prop )
  1-    version 3 > IF
    dup b@ $80 and IF 1- THEN \ Extra 1- when top bit set, two byte header.
  THEN
;



\ Dealing with object trees.
\ The instructions demand the following requirements:
\ - Remove an object from its parent.
\   - Search a sibling linked list for a given object number, stopping when the
\     sibling is a target value.
\ - Print an object's short name.
\ - Jump if an object is a child of another.
\ - Move an object to be a child of another
\   - Requires removing it first!


\ Searches through the parent's linked list of children.
\ Returns the address of the relative slot containing the target object.
\ Note that in the special case where the target object is the first child of
\ the parent, that is the slot returned.
\ This is useful as a helper for removal.
\ Returns 0 if not found. (Can't happen?)
: prev-relative ( sibling ra-parent -- ra-relative-slot )
  swap >r
  >child BEGIN   ( rel-slot    R: target )
    dup rel@   dup r@ = IF r> 2drop EXIT THEN \ Found
    0= IF r> 2drop 0 EXIT THEN \ Found 0, so exit with it.
    nip zobject >sibling ( rel-slot   R: target )
  REPEAT
;



\ Removes a given object (by number) from its parent, if any.
: obj-remove ( target -- )
  dup zobject >parent rel@ 0= IF drop EXIT THEN \ No parent, already done.
  \ Need to actually do the work.
  dup zobject dup R> \ Save the target object for later.
  >parent rel@ zobject ( target ra-parent    R: ra-target )
  prev-relative  ( ra-relative-slot )
  ?dup IF \ Actually defined, so use it.
    r@ >sibling rel@   swap rel! \ Copy the target's sibling into the prev slot.
  THEN
  0 r@ >sibling rel! \ And set my sibling and parent to 0.
  0 r> >parent  rel!
;

: obj-short-name ( ra-obj -- ra-zstr ) >prop-table w@ 1+ ;



\ Attributes

: (attr-byte) ( attr ra-obj -- ra-byte ) swap 3 rshift   4 6 3-5 swap -   + ;
: (attr-mask) ( attr -- mask ) 7 and   $80 swap rshift ;

: attr? ( attr ra-obj -- ? ) 2dup (attr-byte) b@   swap (attr-mask) and 0<> ;
: attr+ ( attr ra-obj -- )
  over (attr-mask) >r
  (attr-byte) dup b@ r> or
  swap b!
;

: attr- ( attr ra-obj -- )
  over (attr-mask) invert >r
  (attr-byte) dup b@ r> and
  swap b!
;

