\ Header constants

\ Real Z-machine addresses for each part of the header.
0x00 CONSTANT hdr-version
0x01 CONSTANT hdr-flags1
0x04 CONSTANT hdr-himem
0x06 CONSTANT hdr-init-pc
0x08 CONSTANT hdr-dictionary
0x0a CONSTANT hdr-objtable
0x0c CONSTANT hdr-globals
0x0e CONSTANT hdr-static-mem
0x10 CONSTANT hdr-flags2
0x18 CONSTANT hdr-abbreviations
0x1a CONSTANT hdr-file-size
0x1c CONSTANT hdr-checksum
0x1e CONSTANT hdr-interpreter-number
0x1f CONSTANT hdr-interpreter-version

0x20 CONSTANT hdr-screen-height-chars
0x21 CONSTANT hdr-screen-width-chars
0x22 CONSTANT hdr-screen-height-units
0x24 CONSTANT hdr-screen-width-units
0x26 CONSTANT hdr-font-width-units
0x27 CONSTANT hdr-font-height-units
0x2c CONSTANT hdr-default-background
0x2d CONSTANT hdr-default-foreground
0x2e CONSTANT hdr-terminators

0x32 CONSTANT hdr-standard-revision
0x34 CONSTANT hdr-alphabet-table
0x36 CONSTANT hdr-extension-table

\ Returns the Z-machine version in play.
: version ( -- u ) hdr-version b@ ;

\ Convenience function for choosing between two values based on the version.
\ Returns x when the version is 3 or less, y for 4 or higher.
\ The word's name is a misnomer, but basically all games are 3, 5 or 8, and
\ almost all the differences are on the 3/4 boundary.
: 3or5 ( x y -- x-or-y ) version 3 > IF nip ELSE drop THEN ;

