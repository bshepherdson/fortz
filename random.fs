\ Random number generator for the Z-machine.
\ Simple linear congruence generator.
\ X_n+1 = (a X_n + c) mod m
\ Use glibc's parameters: a = 1103515245, c = 12345, m = 2^31
\ When seeded, we store the seed into the previous-value slot.
\ Exception: When the seed is 0, we use a system clock to seed it.
\ That is also the initial state.

VARIABLE multiplier
1103515245 CONSTANT real-multiplier
real-multiplier multiplier !

VARIABLE increment
12345 CONSTANT real-increment
real-increment increment !

$7fffffff CONSTANT modulus-mask

VARIABLE previous-value

: seed ( new-seed -- ) modulus-mask and   previous-value ! ;
  \ TODO Add predictable mode as suggested by the Standard section 2.4.
  \ dup 1000 < IF 1 multiplier ! 1 increment ! 0 previous-value !

: true-seed ( -- ) utime ( dtime ) drop seed ;
true-seed

\ Produces the next number in the series.
: genrandom ( -- u )
  previous-value @  multiplier @ *  increment @ +
  modulus-mask and
  dup previous-value !
;

\ Produces a random number between 1 and n.
: random ( n -- ) genrandom swap mod  1+ ;

