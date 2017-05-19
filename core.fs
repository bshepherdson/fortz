\ Core of the Z-machine emulation.
\ Declares the memory, stack, PC and so on.

\ The main memory. Always allocated to 1 meg for simplicity; it's never that
\ big.
1024 1024 * CONSTANT mem-size
CREATE (zmem)   mem-size allot

VARIABLE pc
VARIABLE sp

CREATE (zstack)   1024 2 * allot
here CONSTANT (zstack-top)
