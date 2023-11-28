[IFDEF] -my-notes
	-my-notes
[ENDIF]

marker -my-notes
init-included-files

require q.fs
require fileio.fs

decimal
1 constant debugging
[IFDEF] debugging
: z clearstack ; \ for testing
[ENDIF]


\ testing
2Variable String1Buffer
s" This is a test string!" String1Buffer 2!

\ Note parameters

160 constant ntsz
40  constant numnotes

\ basic note buffer
\ create ntbuf ntsz chars allot
\ ntbuf ntsz 0 fill


\ offsets for header of each note
0 constant note
0 constant ntidx      \ index number of this note
1 constant ntnext     \ index number of next note 
2 constant ntprev     \ index number of previous note
\ date stamp:
3 constant ntyear     \ year, for date stamp, e.g. 2023
4 constant ntmonth    \ month (1-12)
5 constant ntday      \ day (1-31)
6 constant nthour     \ hour (0-23)
7 constant ntmin      \ minute (0-59)
\ the note itself
16 constant ntdata     \ note data
16 constant nthdrsz


hex F4 constant nullptr  \ 244 decimal, used as a "null pointer"
decimal

\ NOTES Struct
\ queue first, then notes struct ?

16 constant ntshdrsz
ntsz numnotes * ntshdrsz + constant ntssz
create nts ntssz chars allot
nts ntssz 0 fill
nts 0 + constant ntshdr

\ offsets for NOTES struct header
0 constant ntstotal       \ total number of current active notes (not deleted)
1 constant ntsfirst       \ index number of first note
2 constant ntsnext        \ index number of next available empty note
3 constant ntslastwritten \ index number of last note entered
\ 4 - 15 reserved

: store-ntstotal ( n -- )
	ntshdr ntstotal + c! ;

: update-ntstotal ( n -- )
	ntshdr ntstotal + dup c@ ( n "ntstotal" ntstotal.value )
	rot +                    ( "ntstotal" ntstotal.value+n )
	swap c! ;                ( stack empty )

: store-ntsfirst ( n -- )
	ntshdr ntsfirst + c! ;

: get-ntsfirst ( -- n )
	ntshdr ntsfirst + c@ ;

: store-ntsnext ( n -- )
	ntshdr ntsnext + c! ;

: store-next-avail-nt-idx ( -- )
	dequeue-elem store-ntsnext ;

: store-lastwritten ( n -- )
	ntshdr ntslastwritten + c! ;

: get-lastwritten ( -- n )
	ntshdr ntslastwritten + c@ ;

: init-nts ( -- )
	\ header
	0 store-ntstotal
	nullptr store-ntsfirst
	store-next-avail-nt-idx \ read from queue, first = 0
	nullptr store-lastwritten ; \ initialize last written to dummy data

: get-ntsnext ( -- n )
	ntshdr ntsnext + c@ ;

: get-nt-addr-from-idx  ( idx -- addr-of-note-within-notes )
	ntsz *
	ntshdrsz + 
	nts +
;


: get-next-nt-offset ( -- addr )
	get-ntsnext  ( n )
	dup              ( n n )
	get-nt-addr-from-idx ( n addr )
	dup               ( n addr addr )
	-rot              ( addr n addr )
	\ store index in note header	
	ntidx + c!   ( addr )
;

: this-is-last-note ( addr -- )
	nullptr swap ntnext + c! ;

: get-ntstotal ( -- n )
	ntshdr ntstotal + c@ ;

\ : update-next-nt ( -- next-avail-nt )
\	store-next-avail-nt-idx ;
	
: get-nt-content
	cr ." Enter note: " ntsz INPUT$ ;

: store-content-to-nts-body ( addr-1 len -- addr-of-nt-within-nts )
	get-next-nt-offset ( addr-1 len addr-of-nt-within-nts )
	dup >r             ( R: addr-of-nt-within-nts )	
	nthdrsz +          ( addr-1 len addr-of-nt-data )
	swap               ( addr-1 addr-of-nt-data len )
	cmove              ( stack empty )
	r>                 ( addr-of-nt-within-nts )
;	
	
: get-idx-of-nt-from-offset ( addr-of-current-note - idx )
	ntidx + c@ ;

	
: update-nt-and-nts-headers ( addr-of-current-note -- )
	dup get-idx-of-nt-from-offset ( addr-of-curr-note idx )
	store-lastwritten            ( addr-of-curr-note )
	this-is-last-note            ( stack empty )
	1 update-ntstotal            ( stack empty ) \ increment ntstotal
	store-next-avail-nt-idx      ( stack empty )
;

: maybe-update-ntsfirst-field ( -- )
	get-ntsfirst ( first-idx )
	nullptr = if 
		get-ntsnext ( n )
		store-ntsfirst ( stack empty )
	then ;

: update-next-nt-field ( -- ) 
	get-lastwritten ( n )
	get-nt-addr-from-idx ( nt-addr )
	get-ntsnext ( nt-addr next-nt-idx )
	swap            ( next-nt-idx nt-addr )
	ntnext + c!     ( stack empty )
;

: write-nt  ( -- )
	\ 0. locate current (i.e. "next avail") note offset within notes struct
	\ 1. get content
	\ 2. store in "note" structure within "notes" structure
	\ 3. update note struct header
	\ 4. update header in "notes" structure
	\ 5. write out file
	\ 6. exit
    maybe-update-ntsfirst-field ( stack empty )
	update-next-nt-field ( stack empty )
	get-nt-content ( c-addr u )
	store-content-to-nts-body
	update-nt-and-nts-headers
;

 
: test ( -- )
	init-nts \ initialize nts ("notes") structure
	write-nt \ enter the first note
	nts ntsz 1 * ntshdrsz + dump \ display it
;

\ read notes:
\ 1. get idx of first note
\ 2. display note with index 
\ 3. get note's "next note" index
\ 4. last note? yes? end. no? go to step 2.

: read-nt ( n -- )
	get-nt-addr-from-idx ( nt-addr )
	dup ntidx + c@       ( nt-addr idx-field-of-nt )
	nullptr =            ( nt-addr f )
	if                   ( nt-addr ) 
		cr ." That note was deleted." cr
		drop exit
	else  \ otherwise, display the note
		nthdrsz + cstring>sstring cr type cr
	then ; 
	

: get-ntnext ( idx -- ntnext-of-idx )
	get-nt-addr-from-idx  ( nt-addr-of-idx )
	ntnext + c@           ( ntnext-of-nt )
;

: last-note? ( idx -- f )
	nullptr = ;

: read-nts ( -- ) \ displays stored notes from first to last.
	get-ntsfirst ( n )
	dup ( n n )
	last-note? ( n f )
	if ( n )
		cr ." No notes. " cr
		drop ( stack empty )
		exit
	else
		begin
			dup ( n n )
			dup cr ." Note: " . ( n n )
			read-nt ( n )
			get-ntnext ( idx-of-next-nt )
			dup ( idx-of-next-note idx-of-next-nt )
			last-note? ( idx-of-next-note f )
		until
		drop
	then ;

\ delete note: delete by number, text, or regexp, with prompt 1. find
\ idx of note to delete 2. compare with first note is it first?
\ 3. yes: change first note field in nts to first note's "next" field
\ no: proceed 4. change the "next" pointer of the previous note (how
\ to determine which is the "previous" note?) to point to the deleted
\ note's "next" pointer. If there is going to be a "previous" pointer,
\ adjust those too.  4a. Could determine the "previous" node by
\ cycling throungh all the notes until the note's "next" pointer
\ points to the note to be deleted, to avoid the complications of a
\ doubly-linked list.

: first-nt? ( idx -- f )
	get-ntsfirst = ;

: erase-nt-by-idx ( idx -- )
	get-nt-addr-from-idx ( ntaddr )
	ntsz erase  \ 0-fills the note
;	

: find-idx-of-nt-that-points-to-n ( n -- idx-of-nt-that-points-to-n )
	dup >r ( n ) ( R: n )
	get-ntsfirst ( n nt[0] )
	begin            ( n nt[0] )
		nip        ( nt[i] )   \ i := i + 1, starting from 0
		dup        ( nt[i] nt[i] )
		get-ntnext ( nt[i] nt[i+1] )
		r@         ( nt[i] nt[i+1] n )
		over       ( nt[i] nt[i+1] n nt[i+1] )
		=          ( nt[i] nt[i+1] f )
	until  	( idx-of-nt-that-points-to-n that-idxs-next-ptr )
	drop    ( idx-of-nt-that-points-to-n )
	r> drop ;


: point-ntnext-ptr-to-next-of-n ( idx-of-nt-that-points-to-n n -- )
	\ What is supposed to happen:
	\ n's "next" pointer is retrieved
	\ and stored in the "next" pointer
	\ of the nt whose "next" points to n
	\ meaning that the next ptr of the nt that points to n
	\ now points to the nt that was pointed to by n's "next" ptr
	get-ntnext ( nt-that-pts-to-n nt-next-of-n )
	swap       ( value-of-next-of-n idx-of-nt-that-points-to-n )
	get-nt-addr-from-idx ( value-of-next-of-n addr-of-nt-that-points-to-n )
	ntnext + ( value-of-next-of-n addr-of-next-ptr-of-nt-that-points-to-n )
	c!    ( stack empty )
;

: store-nullptr-as-idx ( n -- )
	get-nt-addr-from-idx ( nt-addr )
	ntidx + ( nt-idx-addr )
	nullptr swap ( nullptr nt-idx-addr )
	c! ;

: finish-up-del ( n -- )
	dup erase-nt-by-idx ( n ) \ erase note to be deleted
	dup store-nullptr-as-idx ( n ) \ store "null ptr" so we know
	\ there's no note there, rather than index 0
	enqueue-elem        ( stack empty ) \ enqueue n's index back onto the queue
	-1 update-ntstotal	\ decrement the total number of notes
;	

\ This needs to be tested Saturday, November 25, 2023 15:30
: del-nt ( n -- ) \ first, delete the note by index ("idx")
	\ in this word, "n" is the nt to be deleted
	\ are we deleting the first note?
	dup ( n n )
	first-nt? ( n f )
	if        ( first-idx )
		\ if so, change the "first note" pointer to the "next" pointer
		\ of the first note
		dup   ( first-idx first-idx )
		get-ntnext ( first-idx ntnext-of-first )
		\ and store it as the new "first" note
		store-ntsfirst ( old-first-idx ) \ same as first-idx of
		\ previous word \ it might be the last note pointer ("nullptr") !
		finish-up-del \ erase the note and update total # of notes		
	else  \ it's not the first note
		\ cycle through the notes, starting from first
		\ until the "hext" pointer of the note points to "n"
		dup                              ( idx-of-n idx-of-n )
		find-idx-of-nt-that-points-to-n ( idx-n idx-that-points-to-n )
		\ point that node's "next" pointer to the value of
		\ n's "next" pointer
		over ( n ptr-to-n n )
		point-ntnext-ptr-to-next-of-n ( n ) \ could be "nullptr"
		finish-up-del \ erase the note and update total # of notes
	then
;

: write-notes-to-file ( -- )
	notes-filename cstring>sstring open-output
	nts ntssz fd-out write-file
	0= if
		cr ." Notes file written successfully."
	else
		cr ." Error writing notes file."
	then
	close-output ;

: read-notes-from-file ( -- )
	notes-filename cstring>sstring open-input
	nts ntssz
	fd-in read-file ( ntssz sz wior )
	0= if
		cr ." Notes file read successfully."
	else
		cr ." Error reading notes file."
	then
	drop
	close-input ;

[IFDEF] debugging
: r ( n -- ) \ dumps n notes from notes structure
	nts over ntsz * ntshdrsz + dump drop ;
[ENDIF]


: write-q-to-file ( -- )
	q-filename cstring>sstring open-output
	q qsize fd-out write-file
	0= if
		cr ." Queue file written successfully."	else
		cr ." Error writing queue file." then
	close-output ;

: read-q-from-file ( -- )
	q-filename cstring>sstring open-input
	q qsize
	fd-in read-file
	0= if
		cr ." Queue file read successfully."
	else
		cr ." Error reading queue file. "
	then
	drop \ drop length of file read
	close-input ;

: save-state ( -- )
	write-notes-to-file
	write-q-to-file ;

: restore-state ( -- )
	read-notes-from-file
	read-q-from-file ;
