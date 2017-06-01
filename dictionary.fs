\ Dictionary support. Encodes user-input strings for dictionary analysis.


\ User input is converted to lowercase, and encoded into 6 or 9 Z-characters.
\ Then the dictionary is searched, and the parse buffer populated.
\ Global variables are used to hold the raw text position and word length,
\ the parse buffer pointer, and the parseable words remaining.

VARIABLE raw-text  \ Start of the text buffer.
VARIABLE raw-start \ Start index, from the beginning of the buffer.
VARIABLE raw-len   \ Length of the current raw word.
VARIABLE parse-record \ Address of the current parse record.

: >dict      ;
: >len   2 + ;
: >start 3 + ;




