\ Opcode implementations and instruction decoding.

\ Runs a single opcode.
: execute-op ( -- )
  pc@+      ( opcode )
  dup 0xbe =   version 5 >= and IF extended-form EXIT THEN
  dup 6 rshift 3 and ( opcode top-two-bits )
  CASE
  3 OF variable-form ENDOF
  2 OF short-form    ENDOF
  long-form
  ENDCASE
;

