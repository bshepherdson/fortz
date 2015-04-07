\ Utilities and helper functions for the CPU.

\ Expects the local number from 1 to 15. Returns the Forth address to use.
: local ( n -- a-addr ) cells fp @ + ;
: local@ ( n -- value ) local @ ;
: local! ( value n -- ) >r 0xffff and r>  local ! ;

\ Expects the global number from 16 to 255. Returns the Z-machine ra of it.
: global ( n -- ra ) 16 - 2 * ( offset ) hdr-globals w@ + ( ra ) ;
: global@ ( n -- value ) global w@ ;
: global! ( value n -- ) global w! ;

\ Reads from a given var number.
: var@ ( n -- value )
  dup 0= IF drop pop  EXIT THEN
  dup 16 < IF local@ ELSE global@ THEN
;
: var! ( value n -- )
  dup 0= IF drop push EXIT THEN
  dup 16 < IF local! ELSE global! THEN
;

\ Reads the one-byte store location and puts the value there.
: zstore ( value -- ) pc@+ var! ;

\ Routines use a frame pointer in C fashion.
\ When we start a new routine, the stack looks like this:
\ old stack ...
\ ------------
\ local N
\ ....
\ local 1
\ old-fp        <--- fp
\ old-sp
\ old-pc
\ return-expected <--- sp

\ Address where the old value is stored.
: old-FP ( -- a-addr ) fp @ ;
: old-SP ( -- a-addr ) fp @ 1 cells - ;
: old-PC ( -- a-addr ) fp @ 2 cells - ;
: expected-return ( -- a-addr ) fp @ 3 cells - ;

\ Note that the ret-value is the actual value, not a variable reference.
\ Therefore if it came from a local or the stack, we're free to mangle those.
: zreturn ( ret-value -- )
  \ First, grab the return-expected flag.
  expected-return @ ( ret-value ret? )
  \ Then restore the old PC, SP and FP, in that order (FP has to be last).
  old-PC @ pc !
  old-SP @ sp !
  old-FP @ fp !
  ( ret-value ret? )
  IF zstore ELSE drop THEN
;


\ Given the base condition, adjusts for possible negation, then branches if
\ the branch should be taken.
: zbranch ( ? -- )
  pc@+ ( ? branch-hi )
  \ Invert flag if bit 7 is 0.
  dup 128 and not IF >r not r> THEN ( branch? branch-hi )
  \ If bit 6 is 1, single byte 0-63. If 0, two-byte signed 14-bit offset.
  dup 64 and IF
    63 and
  ELSE
    63 and 8 lshift pc@+ or ( branch? unsigned-offset )
    dup 0x2000 and IF 0x4000 swap - negate THEN \ Adjust for signed offset.
  THEN
  ( branch? branch-offset )
  swap not IF drop EXIT THEN \ Bail if we're not branching.
  \ An offset of 0 or 1 means to return that value.
  dup 1 invert and 0= IF
    zreturn
  ELSE
    \ Otherwise adjust PC by the offset - 2.
    2 - pc+
  THEN
;



\ Helper for zcall. Expects the routine address and number of locals.
\ Copies the default local values at routine+1, routine+3, etc. into the locals.
: copy-locals ( ra-routine local-count -- )
  swap 1+ swap ( ra-local-1 local-count -- )
  0 ?DO ( ra-local-1 )
    I ( ra-local-1 0-based-local-number )
    2dup 2 * + w@ ( ra-local-1 0-based-local-number value )
    swap 1+ local! ( ra-local-1 )
  LOOP
  drop
;

\ Given a count on top, deletes the count and count more values.
: discard-args ( ... n -- ) BEGIN dup WHILE nip 1- REPEAT drop ;


\ Expects the arguments (first one on top) with the routine address above it,
\ and the counter (#args + 1) above that. Topmost is a flag for whether a return
\ is expected.
\ Since the args and routine have already been read, I'm free to mangle the
\ PC, FP and SP.
\ NB: The routine address is an RA! Any conversion of it, eg. from a packed
\ address, needs to be done before calling zcall.
: zcall ( argN ... arg2 arg1 ra-routine n return? -- )
  rot ( args... n ret? routine )
  dup 0= IF drop IF 0 zstore THEN 1- discard-args EXIT THEN
  dup b@ ( ... routine local-count )
  \ The new FP will be the old SP minus 2*(locals+1)
  sp @ over 1+ cells - ( ... routine local-count new-fp )
  fp @ over ! \ Store the old fp into the new fp.
  \ And the new fp value into the variable.
  fp !    ( ... routine local-count )
  \ Store the old PC too, because I'm about to mangle it.
  pc @ old-PC !
  \ And SP too because it's fine whenever.
  sp @ old-SP !
  version 4 <= IF \ Copy the default locals into their new home.
    \ Compute the new PC and store it now.
    2dup 2 * 1 + + pc ! ( ... routine local-count )
    copy-locals ( ... )
  ELSE ( ... routine local-count )
    drop 1+ pc ! ( ... )
  THEN

  ( args... n ret? )
  \ Now FP and PC are set, the old FP, SP and PC are saved.
  \ Save the return-expectation too.
  expected-return ! ( args... n )

  \ The new SP is four cells lower than the new FP.
  fp @ 4 cells - sp !

  \ Note that N is one greater than the number of arguments. Perfect, since
  \ locals are numbersed from 1.
  1 ?DO \ Don't want to loop if they're equal. No args to copy.
    I local!
  LOOP
  ( )
  \ Now all the pointers should be set up properly.
;


