\ Screen-handling

\ The most fundamental print operation: renders the ASCII text to the screen.
: print ( c-addr u -- ) type ;

: print-zstr ( ra -- ) zstr-decode decoded-string print ;


