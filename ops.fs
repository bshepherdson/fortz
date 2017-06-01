\ Top-level CPU operations file.

\ Includes the different types of ops, which each have their own file.
\ Also defines some helpers, and the top-level decoding functions.

\ Helpers go here.

\ Reading and writing variables.
\ Locals are on the stack.
: local ( var -- a-addr ) cells fp @ + ;
: local@ ( var -- w ) local @ ;
: local! ( w var -- ) >R $ffff and R>   local ! ;

\ Globals are in a table in dynamic memory.
: global ( var -- ra ) 16 -  2 *  hdr-globals w@ + ( ra ) ;
: global@ ( var -- w ) global w@ ;
: global! ( w var -- ) global w! ;

\ General variable handlers.
: var@ ( var -- w )
  dup 0= IF drop pop EXIT THEN
  dup 16 < IF local@ ELSE global@ THEN
;
: var! ( w var -- )
  dup 0= IF drop push EXIT THEN
  dup 16 < IF local! ELSE global! THEN
;


\ For variable and extended form, here are the argument slots.
\ Keeping them on the stack is unwieldy.
\ They're also passed to function calls in this fashion.
\ These are cell-sized.
\ NB: 0-, 1- and 2-op instructions expect their arguments on the stack.
8 array argv
VARIABLE argc

3 ARRAY arg-types

: arg-large ( -- val ) pc@w+ ;
: arg-small ( -- val ) pc@+  ;
: arg-var   ( -- val ) pc@+ var@ ;

' arg-large   0 arg-types !
' arg-small   1 arg-types !
' arg-var     2 arg-types !

\ Reads an argument, given its type.
\ Error to call this with type $$11 = 3 = omitted.
: arg ( arg-type -- val ) arg-types @ execute ;


: push-arg ( w -- ) argc @   argv !   1 argc +! ;

\ Parses one variable-form arg type byte, and consumes those arguments.
: var-args ( -- )
  pc@+ 6 0 DO
    dup i rshift   3 and
    dup 3 = IF 2drop unloop EXIT THEN
    arg push-arg
  -2 +LOOP
  drop
;


\ Asserts that there are exactly N arguments.
\ Consumes the arg count.
\ Enables variable-form words with fixed arg requirements to say eg. 3 args
: args ( n -- ) argc @ <> ABORT" Bad argument count" ;



: signed ( w -- n ) dup 15 rshift IF -1 16 lshift or THEN ;
: 2signed ( w w -- n n ) signed >r signed r> ;
: clip ( x -- w ) $ffff and ;


\ Some important helpers: store, branch, call and return.
: zstore ( w -- ) pc@+ var! ;

\ Calls and returns
\ Routines use a frame point (FP) in C fashion.
\ When we start a new routine, the stack looks like this:

\ old stack...
\ --------------
\ local N
\ ...
\ local 1
\ old-fp          <--- fp
\ old-sp
\ old-pc
\ arg-count
\ return-expected <--- sp

\ NB: In order to make save files portable between runs of the emulator, the
\ real pointers (old SP and FP) need to be relative to stack-top, not raw.

\ Addresses where old values are stored.
: old-FP ( -- a-addr ) fp @ ;
: old-SP ( -- a-addr ) fp @ 1 cells - ;
: old-PC ( -- a-addr ) fp @ 2 cells - ;
: arg-count ( -- a-addr ) fp @ 3 cells - ;
: expected-return ( -- a-addr ) fp @ 4 cells - ;


\ Note that the returned value is an actual value, not a variable reference.
\ So we're free to mangle the stack.
: zreturn ( val -- )
  \ First, check if a return is expected.
  expected-return @
  \ Then restore the old PC, SP and FP in that order (FP must be last).
  old-PC @ pc !
  (zstack-top)   old-SP @  -  sp !
  (zstack-top)   old-FP @  -  fp !
  ( ret-val ret? )
  IF zstore ELSE drop THEN
;


\ Helper for zcall. Expects the routine address and number of locals.
\ Copies the default locals at routine+1, routine+3 etc. into the locals.
\ This is only used prior to v5.
\ NB: FP must already be set, so that local! can be used.
: copy-locals ( ra-routine count -- )
  swap 1+ swap 0 ( ra-local-1 count 0 ) ?DO
    i 2dup 2 * + w@
    swap 1+ local!
  LOOP
  drop
;

\ On v5+, there are no default locals. This 0s them out.
: zero-locals ( count -- ) 0 ?DO 0 i 1+ local! LOOP ;


\ Expects the argument array, argument count, and routine address on the stack.
\ Argument array is usually (part of) argv, but not necessarily.
: zcall ( argv argc ra-routine return? -- )
  over 0= IF \ Special case: A routine address of 0 is an instant rfalse.
    >R 2drop drop R> IF 0 zstore THEN \ Store if return expected.
    EXIT
  THEN

  >r ( argv argc ra )
  dup b@ ( ... local-count )
  \ FP is old-SP minus locals+1 cells.
  sp @   over 1+ cells - ( ra local-count new-fp )
  stack-top fp @ -   over !   \ Store old fp (relativized) at the new fp.
  \ And the new FP into the variable.
  fp !

  \ Store the old PC too, before we mangle it.
  pc @ old-PC !
  stack-top sp @ -   old-SP ! \ SP too.

  ( argv argc ra locals )
  dup >R   ( argv argc ra locals    R: ret? locals )
  version 4 <= IF \ Copy the default locals.
    \ Compute the new PC and store it.
    2dup  2 * 1+ + pc !
    copy-locals ( argv argc )
  ELSE \ Zero the locals.
    zero-locals ( argv argc routine )
    1+ pc ! ( argv argc )
  THEN

  ( argv argc    R: ret? )
  R>
  R> expected-return !
  over arg-count !
  ( argv argc locals )

  fp @ 4 cells - sp ! \ New SP is four cells below the new FP.

  \ Copy the minimum of locals and arguments.
  min ( argv count )
  0 ?DO dup i cells @   i 1+ local! LOOP
  ( )
;


\ Branches are either a single byte, 0-63, or two bytes, signed 14-bit.

\ Sign-extends 14-bit 2's complement to native size.
: branch-sign-extend ( w -- n )
  dup 13 rshift   1 and   IF -1 14 lshift   or THEN
;

\ Actually does a branch.
: (do-branch) ( offset -- )
  dup 0 = IF drop 0 zreturn EXIT THEN
  dup 1 = IF drop 1 zreturn EXIT THEN
  2 -   pc +!
;

\ First, when bit 7 is clear, we negate the flag.
\ Second, compute the offset, and then branch if needed.
: zbranch ( ? -- )
  pc@+
  dup 7 rshift 1 and 0= IF >r 0= r> THEN \ Invert the condition
  dup 6 rshift 1 and IF \ single byte
    $3f and
  ELSE \ two bytes
    $3f and   8 lshift
    pc@+ or
    branch-sign-extend
  THEN
  swap IF (do-branch) ELSE drop THEN
;



: illegal-opcode ( c-addr u -- ) cr cr ." Illegal opcode: " type cr cr bye ;


\ Requiring the actual operand definitions.
\ All of these define an ARRAY of xts, whose name is given.
\ The stack arguments are given as well.
\ All of these expect PC to point to just after the last argument.
\ That will either be (cascading priority):
\ - Store byte
\ - Branch byte or word
\ - Inline string
\ - Next instruction.
REQUIRE ops/0op.fs  \ 0ops, format: ( -- )
REQUIRE ops/1op.fs  \ 1ops, format: ( a -- )
REQUIRE ops/2op.fs  \ 2ops, format: ( b a -- ) (ie. first argument on top)
REQUIRE ops/var.fs  \ varops, format: ( -- ) (argv+argc holds the args)
REQUIRE ops/ext.fs  \ extops, format: ( -- ) (argv+argc holds the args)


\ Top-level decoding.

\ Decoding the four forms of instructions.
\ All of these call through to the implementation function.


\ Variable form.
\ If bit 5 is 0, this is a 2OP instruction encoded in var form.
\ Read the args in variable form, but then call into the 2OP table.
: decode-variable ( opcode -- )
  var-args \ Consumes the arg byte.
  dup $20 and 0= IF \ 2OP style
    2 args   \ Wanted in ( b a -- ) form
    >r   1 argv @   0 argv @   r>
    $1f and   2ops @ execute
  ELSE \ VAR form
    \ Special case instructions call_vs2 and call_vn2 know to call var-args
    \ again, if argc==4.
    $1f and   varops @ execute
  THEN
;

\ In extended form, it's VAR argument count.
\ The opcode is the entire next byte.
: decode-extended ( -- )
  pc@+ ( opcode )
  var-args
  ext-ops @ execute
;

\ Decoding short form.
\ Bits 4 and 5 give an operand type, maybe 3 (0OP) or not (1OP).
\ Bottom 4 bits give the opcode number.
: decode-short ( opcode -- )
  \ Special case: $BE on v5+ is "extended form".
  dup $be =   version 5 >= and IF drop decode-extended EXIT THEN

  \ Otherwise, continue with short form.
  dup rshift 4   3 and
  dup 3 = IF
    drop     $f and 0ops @ execute \ 0OP, no arg to fetch
  ELSE
    arg swap $f and 1ops @ execute \ 1OP
  THEN
;


\ In long form, it's always 2OP.
\ Bit 6 gives the first type, bit 5 the second. 0 means small constant, 1 means
\ variable.
\ Since those are types 1 and 2 respectively, adding 1 gives the standard type.
\ We have to read the first arg first, then swap it onto the top.
: decode-long ( opcode -- )
  dup >r
     6 rshift   1 and   1+ arg ( a )
  r@ 5 rshift   1 and   1+ arg ( a b )
  swap r> ( b a opcode )
  $1f and 2ops @ execute
;


\ Top-level instruction decoder.
: decode-instruction ( opcode -- )
  dup 6 rshift   3 and
  dup 3 = IF drop decode-variable EXIT THEN
  2 = IF decode-short EXIT THEN
  decode-long
;

