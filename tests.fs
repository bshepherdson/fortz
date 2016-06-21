\ Tests for various parts of the Z-machine, especially objects.

: test-obj-reading-relatives ( -- )
  88 zobject parent  relative@   82 <> ABORT" Misread object's parent"
  88 zobject sibling relative@   75 <> ABORT" Misread object's sibling"
  88 zobject child   relative@   89 <> ABORT" Misread object's child"
;

: test-obj-prop-table ( -- )
  88 zobject prop-table-addr   $1371 <> ABORT" Wrong property table address"
  88 zobject prop-table        $137a <> ABORT" Wrong first property address"
  88 zobject short-name   parse-string   S" Up a Tree" str= not
      ABORT" Wrong short name"
;

: test-obj-remove ( -- )
  82 zobject  88  find-child   not ABORT" 88 is not a child of 82"
  88 object-remove
  88 zobject parent  relative@ 0 <> ABORT" Removed object still has a parent"
  88 zobject sibling relative@ 0 <> ABORT" Removed object still has a sibling"
  82 zobject  88  find-child   ABORT" 88 is still a child of 82 after removal"
;


: test-dump-children ( obj -- )
  ." Children of object " dup . ." : "
  zobject child relative@
  BEGIN ( child# )
    dup 0<>
  WHILE
    dup .
    zobject sibling relative@
  REPEAT
  cr
;

: test-obj-insert ( -- )
  ." This test moves object 89 from its original parent 88 to 94." cr
  89 test-dump-children
  88 test-dump-children
  94 test-dump-children

  88 zobject  89  find-child not ABORT" 89 starts as a child of 88"
  94 zobject child relative@ ( child-94-pre )
  94 89   14 2OPS @ execute

  94 zobject child relative@  89 <> ABORT" Post-insert child of 94 should be 89"
  89 zobject sibling relative@   <>
      ABORT" Post-insert sibling of 89 should match pre-insert child of 94"
  89 zobject parent relative@ 94 <> ABORT" Post-insert parent of 89 must be 94"

  88 zobject 89 find-child
      ABORT" Post-insert, 89 should be gone from the children of 88"
  ." Move complete. 89 should be gone from 88 and attached to 94." cr
  89 test-dump-children
  88 test-dump-children
  94 test-dump-children
;

: test-objects ( -- )
  restart test-obj-reading-relatives
  restart test-obj-prop-table
  restart test-obj-remove
  restart test-obj-insert
  ." Object tests are passing." cr
;
