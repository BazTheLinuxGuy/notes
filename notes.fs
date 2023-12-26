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


10 constant NEWLINE
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

struct
	char% field ntidx
	char% field year
	char% field month
	char% field day
	char% field hour
	char% field minute
	char% field rsv1
	char% field rsv2
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
\ variable head

: new-node ( -- addr )
	note-node% %allot \ dup saveref !
	dup note-node% %size chars erase
;


$100 constant tbsz \ hex 100, decimal 256
&76 constant linesz \ decimal 76
\ $FE constant NULL \ hex FE, decimal 254

variable psize 0 ,  \ paragraph size

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
decimal
: display-timestamp ( a -- )
	dup >r
	minute c@
	r@ hour  c@
	r@ day c@
	r@ month c@
	r> year c@
	dispdt space disptm ;

: pedit ( a n -- ) \ address of buffer, size of one line.
	>r \ save line size
	0 psize !	
	cr ." Enter note. Enter '.' on  a new line to finish."
	cr
	begin    \ a (a = address of buffer)
		r@   \ a n (n = line size)
		2dup \ a n a n 
		cr ." > "
		accept \ a n "actual size"
		dup psize +! \ keep track of actual size...use this later
		\ a n "actual size"
		nip \ a size
		2dup   \ a size a size
		s" ." compare  \ a size f
		0<>
	while
			+ dup \ a+size	a+size
			0 swap c! \ a+size
			1+       \ a+size+1...next slot
\			r@
	repeat
	drop NEWLINE swap c! \ end of input=NEWLINE (decimal 10)
	r> drop
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
	new-node
	dup note linesz pedit
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
	drop
	\ in case we need to add another instruction
	\ after "store-last"
;

\ hex

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

\ : write-list ( -- )
\	s" newnotes.txt" open-output
\	head @
\	begin
\		dup list-next @ 0<> 
\	while
\			dup data @ dup 
\			fd-out write-line throw
\	repeat
\	drop
\	close-output ;




