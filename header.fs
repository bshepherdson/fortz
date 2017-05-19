\ Definitions of locations in the story header.
\ These are byte/real addresses of parts of the story's header.

$00 CONSTANT hdr-version
$01 CONSTANT hdr-flags1
$04 CONSTANT hdr-high-mem
$06 CONSTANT hdr-initial-pc
$08 CONSTANT hdr-dictionary
$0a CONSTANT hdr-object-table
$0c CONSTANT hdr-globals
$0e CONSTANT hdr-static
$10 CONSTANT hdr-flags2
$18 CONSTANT hdr-abbreviations
$1a CONSTANT hdr-length
$1c CONSTANT hdr-checksum
$1e CONSTANT hdr-interpreter-number
$1f CONSTANT hdr-interpreter-version
$20 CONSTANT hdr-screen-height-chars
$21 CONSTANT hdr-screen-width-chars
$22 CONSTANT hdr-screen-height-units
$24 CONSTANT hdr-screen-width-units
$26 CONSTANT hdr-font-width-units
$27 CONSTANT hdr-font-height-units
$28 CONSTANT hdr-routines-offset
$2a CONSTANT hdr-strings-offset
$2c CONSTANT hdr-default-background
$2d CONSTANT hdr-default-foreground
$2e CONSTANT hdr-terminator-table
$32 CONSTANT hdr-standard-revision
$34 CONSTANT hdr-alphabet-table
$36 CONSTANT hdr-header-extension
