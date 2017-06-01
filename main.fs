\ Top-level file for the Z-machine, and the one that loads the others.

REQUIRE defs.fs
REQUIRE core.fs
REQUIRE header.fs
REQUIRE mem.fs
REQUIRE random.fs
REQUIRE strings.fs
REQUIRE objects.fs
REQUIRE ops.fs


VARIABLE (story-file)

\ Opens a story file whose name is given. Doesn't actually read it in.
: LOAD-STORY ( c-addr u -- )
  r/o bin open-file ABORT" Could not open story file"
  (story-file) !
;

\ Reloads the story file, and sets up to run it.
: (RESTART) ( -- )
  0 s>d (story-file) @ reposition-file ABORT" Could not reset to start of story"
  \ Read the entire file on top of the memory.
  \ TODO A few parts aren't supposed to be overwritten.
  (zmem) mem-size (story-file) @ read-file ABORT" Failed to read story file"
  ." Read " . ."  bytes." cr

  \ TODO More legwork here: populate flags etc., set initial PC, initial SP.
;

' (RESTART) IS restart \ Populate the deferred word.

