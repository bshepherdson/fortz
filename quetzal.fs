\ Definitions for reading and writing Quetzal format save files.
\ TODO Actually support Quetzal! For now this is a hack that just dumps
\ my own interpreter-specific data format, which is detailed below.

\ Things to store:
\ Relative SP (4 bytes)
\ Relative FP (4 bytes)
\ Rel-SP bytes of stack data, stored from SP upwards.
\ PC (4 bytes, points at the branch offset or store target)
\ All of dynamic memory, up to the top noted in the header.

\ 80,000 bytes of save buffer, which is plenty.
here CONSTANT save-buffer 80000 allot
VARIABLE save-ptr

: byte>save ( byte -- ) save-ptr @ c!   1 save-ptr +! ;
: native>save ( machine-word -- )
  dup 24 rshift 255 and byte>save
  dup 16 rshift 255 and byte>save
  dup  8 rshift 255 and byte>save
                255 and byte>save
;

: save>byte ( -- byte ) save-ptr @ c@   1 save-ptr +! ;
: save>native ( -- u )
  save>byte 24 lshift
  save>byte 16 lshift or
  save>byte  8 lshift or
  save>byte           or
;

\ Fills save-buffer with the game state, ready to be written to disk.
: fill-save-buffer ( -- )
  save-buffer save-ptr !
  stack-top sp @ -   native>save
  stack-top fp @ -   native>save
  stack-top sp @ DO i c@ byte>save LOOP

  pc @ native>save

  hdr-static-mem w@ ba   0 DO i b@ byte>save LOOP
;

\ Drains the save-buffer, which has been read from disk, into the game state.
: drain-save-buffer ( -- )
  save-buffer save-ptr !
  stack-top save>native -   sp !
  stack-top save>native -   fp !
  stack-top   sp @ DO save>byte i c! LOOP

  save>native pc !
  hdr-static-mem w@ ba   0 DO save>byte i b! LOOP
;


: write-save-file ( c-addr u -- )
  fill-save-buffer
  w/o create-file ABORT" Failed to create save file" ( fileid )
  dup save-buffer   save-ptr @ save-buffer - ( fileid fileid caddr u )
  rot write-file ABORT" Failed to write save file"   ( fileid )
  close-file ABORT" Failed to close save file" ( )
;

\ Loads the save-buffer with the contents of the named save file.
: read-save-file ( c-addr u -- )
  r/o open-file ABORT" Failed to open save file" ( fileid )
  dup file-size ABORT" Failed to check file size" ( fileid ud )
  d>s save-buffer swap ( fileid c-addr u )
  rot dup >r read-file ABORT" Failed to read save file" ( size R: fileid )
  drop
  r> close-file ABORT" Failed to close save file"
  drain-save-buffer
;



here CONSTANT save-file 256 allot
: request-file-name ( -- c-addr u )
  S" Save file name: " type
  save-file 255 accept ( len )
  save-file swap
;
: save-game ( -- )    request-file-name write-save-file ;
: restore-game ( -- ) request-file-name read-save-file ;

