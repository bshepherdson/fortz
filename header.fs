\ Header constants

\ Real Z-machine addresses for each part of the header.
$00 CONSTANT hdr-version
$01 CONSTANT hdr-flags1
$04 CONSTANT hdr-himem
$06 CONSTANT hdr-init-pc
$08 CONSTANT hdr-dictionary
$0a CONSTANT hdr-objtable
$0c CONSTANT hdr-globals
$0e CONSTANT hdr-static-mem
$10 CONSTANT hdr-flags2
$18 CONSTANT hdr-abbreviations
$1a CONSTANT hdr-file-size
$1c CONSTANT hdr-checksum
$1e CONSTANT hdr-interpreter-number
$1f CONSTANT hdr-interpreter-version

$20 CONSTANT hdr-screen-height-chars
$21 CONSTANT hdr-screen-width-chars
$22 CONSTANT hdr-screen-height-units
$24 CONSTANT hdr-screen-width-units
$26 CONSTANT hdr-font-width-units
$27 CONSTANT hdr-font-height-units
$2c CONSTANT hdr-default-background
$2d CONSTANT hdr-default-foreground
$2e CONSTANT hdr-terminators

$32 CONSTANT hdr-standard-revision
$34 CONSTANT hdr-alphabet-table
$36 CONSTANT hdr-extension-table

\ Returns the Z-machine version in play.
: version ( -- u ) hdr-version b@ ;

\ Convenience function for choosing between two values based on the version.
\ Returns x when the version is 3 or less, y for 4 or higher.
\ The word's name is a misnomer, but basically all games are 3, 5 or 8, and
\ almost all the differences are on the 3/4 boundary.
: 3or5 ( x y -- x-or-y ) version 3 > IF nip ELSE drop THEN ;


create story-size-multipliers 2 c, 2 c, 2 c, 4 c, 4 c, 8 c, 8 c, 8 c, ALIGN

\ Size of the file in bytes. The header word multiplied by a constant.
: story-file-size ( -- u )
  version 1- story-size-multipliers + c@ ( multiplier )
  hdr-file-size w@ * ( size )
;

\ Sets the various flag bits for this interpreter.
: init-header-bits ( -- )
  version 3 <= IF \ Set up flags 1 for early versions.
    %00010000 \ Indicate no status line, no screen split, fixed-pitch font.
    %01110000 \ Mask for the interpreter-set bits.
  ELSE
    0   \ None of these special features are available.
    255 \ Mask: all settable by interpreter
  THEN ( set mask )
  hdr-flags1 b@ and or ( flags1' )
  hdr-flags2 b! ( )

  \ Flags 2 is set to 0; those features are unavailable.
  0 hdr-flags2 w!

  \ Set interpreter number 6, pretending to be an IBM PC.
  6  hdr-interpreter-number b!
  \ Set interpreter version A.
  65 hdr-interpreter-version b!

  term-size ( rows cols )
  dup hdr-screen-height-chars b!
      hdr-screen-height-units w!
  dup hdr-screen-width-chars b!
      hdr-screen-width-units w!

  1  hdr-font-width-units  b!
  1  hdr-font-height-units b!

  \ TODO set default colors.
;
