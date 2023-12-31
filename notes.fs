[IFDEF] -my-newnotes
	-my-newnotes
[ENDIF]

marker -my-newnotes
init-included-files

\ Updated: 12/24/2023 20:45
\ Next put in the date/time stamp
\ After that, serialize, write to file
\ The read from file and de-serialize.
\ BAZ

decimal
require numconv.fs
require fileio.fs



1 constant debugging

[IFDEF] debugging
	: z clearstack ;
	\ for testing
[ENDIF]
[IFDEF] debugging
	: killstack depth 0> if
			clearstack
		then ; \ for testing
[ENDIF]
decimal

\ struct
\	char% field note char% $100 *
\ end-struct n%


killstack

&10 constant NEWLINE \ 0x0A 
$100 constant tbsz   \ hex 100, decimal 256 "text buffer size"
&76 constant linesz  \ decimal 76 - size of input line

align
struct
	cell% field size
	char% field year
	char% field month
	char% field day
	char% field hour
	char% field minute
	char% field rsv1
	char% field rsv2
	char% field rsv3
	\	n% field nt
	char% field note char% $100 *	
	cell% field list-next
end-struct note-node%

struct
	cell% field numnotes
	cell% field head
	cell% field last
end-struct notes%

killstack

create notes notes% allot drop
notes notes% %size erase

variable saveref

: new-node ( -- addr )
	note-node% %allot \ dup saveref !
	dup note-node% %size chars erase
;

\ variable psize 0 ,  \ paragraph size

: timestamp-node ( a -- )
	>r
	time&date  ( sec min hr day month year )
	2000 - 
	r@ year c! ( sec min hr day mon )
	r@ month c! ( sec min hr day )
	r@ day c! ( sec min hr )
	r@ hour c! ( sec min )
	r> minute c! ( sec )
	drop    \ no room to store seconds
;

: display-timestamp ( a -- )
	dup >r
	minute c@
	r@ hour  c@
	r@ day c@
	r@ month c@
	r> year c@
	dispdt space disptm ;

: show-prompt ( -- )
	cr ." Enter note. Enter '.' on  a new line to finish."
	cr ;

: init-size-to-zero ( node -- )
	size 0 swap !
;	

: get-line-input ( a -- size )
		linesz \ a line-size 
		cr ." > "  
		accept      \ a "actual size"
;

: terminate-with-zero ( address size -- newaddress )
	+ dup          \ a+size a+size
	0 swap c!      \ store the string-terminating 0
	1+             \ a+size+1 \ update the address
;

: add-newline ( address size -- newaddress )
	dup	NEWLINE swap c!	1+
;

: increment-size ( node -- ) \ increment size field by 1
	size 1 swap +!
;	


\ pedit problem: Saturday, December 30, 2023 12:10
\ for some reason, the '.' is getting stored with the
\ rest of the input:
\ 7FC696D28C18: 54 68 69 73  20 69 73 20 - 6C 69 6E 65  20 31 20 6F  This is line 1 o
\ 7FC696D28C28: 66 20 6E 6F  74 65 20 6F - 6E 65 2C 00  61 6E 64 20  f note one,.and 
\ 7FC696D28C38: 74 68 69 73  20 69 73 20 - 74 68 65 20  73 65 63 6F  this is the seco
\ 7FC696D28C48: 6E 64 20 6C  69 6E 65 21 - 00 2E 0A 00  00 00 00 00  nd line!........
\ as can be seen, the '.' ($2E) is in there, without a terminating 0,
\ right before the newline.

: pedit ( node-address  -- )
	\ IN: address of buffer \ , size of one line.
	\ OUT: number of bytes in buffer
	\ init size to 0
	dup init-size-to-zero
	\ get the note address within node
	show-prompt
	dup note \ node note-within-node	
	begin          \ node note+currentsize
		dup get-line-input   \ node a "actual-size-of-input"
		2dup s" ." compare 0<> if \ node a actsize 
			\ update size field for this note		
			dup 3 pick size +! \ node a actsize
			terminate-with-zero
			over increment-size
		else
			drop \ because this is the '.', size 1
			add-newline
			over increment-size
			2drop
			exit
		then
	again
	\ return to caller with empty stack
;

: pshow ( a -- ) \ a is address of paragraph to show
	\ cr ." You entered:" cr
	begin
		cstring>sstring
		over c@
		NEWLINE <>			
	while
		2dup
		cr type
		+ 1+
	repeat
	2drop
;


: first-node? ( -- f )
	notes head @ 0= if
		true
	else
		false
	then
;

: store-head ( a -- )
	notes head ! ;

: store-last ( a -- )
	notes last ! ;

: update-previous-nodes-list-next-pointer ( addr -- )
	notes last @ \ addr "last"-note-ptr
	list-next    \ addr "last"-notes-next-ptr
	!
;	

: makenote ( -- )
	\ creates a new node and asks the user for note entry,
	\ then stores it in the linked list
	new-node dup pedit   \ just send pedit the node address
	\ add date/timestamp here
	dup timestamp-node
	cr ." On: "
	dup display-timestamp
	cr ." You entered: "
	dup note pshow 
	\ here, ask the user if things look OK.
	\ if not, enter "edit mode".
	\ ... ask-edit? if edit-mode then
	\ (or something like that)
	
	\ is this the first node? If so, store as "head"
	first-node? if
		dup store-head
	else
		\ otherwise, update the previous node's
		\ "list-next" pointer
		dup update-previous-nodes-list-next-pointer
	then
	\ store current node as "last" in notes struct
	dup store-last
	drop \ I guess we don't need the new node's address anymore.
;

\ This next one needs work:
: display-list-data
	notes head @
	dup 0<>
	if
		begin
			dup note pshow
			list-next @
			dup 0=
		until
		drop
	else
		cr ." No notes." cr
	then
;

: my-u. ( u -- )
  \ Simplest use of pns.. behaves like Standard u. 
  0              \ convert to unsigned double
  <<#            \ start conversion
  #s             \ convert all digits
	#>             \ complete conversion
\ type space     \ display, with trailing space
  #>> ;          \ release hold area


: nconv ( n -- addr u )
	0 <# #s #> ;

\ Working here:
: write-list ( -- )
	notes head @
	s" newnotes.txt" open-output
	begin
		
		
		
		dup 0=
	until
			\ need to convert datestamp to ascii
			\ and we need the size of the note in the size field
\			dup data @ dup 
\			fd-out write-line throw
\	repeat
\	drop
	\	close-output
;

\ cr ." After end of source file " cr .s key emit cr


