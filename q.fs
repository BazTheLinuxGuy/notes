[IFDEF] -mine
	-mine
[ENDIF]



marker -mine
init-included-files

decimal

\ new queue data structure, Friday, November 10, 2023
\ queue header:
\ offset:
\ cell 0           count byte
\ cell 1           head ptr
\ cell 2           tail ptr
\ cell 3           reserved
\ queue data:
\ chars 0..qnelems-1


s" Queue overflow" exception constant q-overflow
s" Queue underflow" exception constant q-underflow

hex
F4 constant EMPTY
decimal

0 constant qc
1 constant qh
2 constant qt
\ 3 constant qr
16 constant qhdrsz
16 constant qdata
40 constant qnelems 

\ create a queue to hold the "available" (empty) note slots
qhdrsz qnelems + constant qsize
create q qsize allot 

\ q qc + constant qcount
\ q qh + constant qhead
\ q qt + constant qtail
\ q qr + constant qresv
\ q qhdrsz + constant qdata
qnelems 1- constant qlastelem
0 constant qfirstelem

: get-qcount ( -- )
	q qc + c@ ;

: get-qhead ( -- )
	q qh + c@ ;

: get-qtail ( -- )
	q qt + c@ ;

: store-qcount ( n -- )
	q qc + c! ;

: store-qhead ( n -- )
	q qh + c! ;

: store-qtail ( n -- )
	q qt + c! ;

: init-qcount ( -- )
	qnelems store-qcount ;

: init-qhead qfirstelem q qh + c! ;
	
: init-qtail qlastelem q qt + c! ;

: init-qdata ( -- )
	q qdata +        \ address of queue data ==> TOS
	qnelems 0   \ stack: qdata qnelems 0
	u+do        \ stack: qdata
		dup i + \ stack: qdata qdata+(0..qnelems-1)
		i       \ stack: qdata qdata+(0..qelems-1) (0..qnelems-1)
		swap    \ stack: qdata (0..qnelems-1) qdata+(0..qelems-1)
		c!      \ stack: qdata ("i" is stored at qdata+i)
	loop
	drop ;      \ stack: empty

	
: initq ( -- )       \ initialize the queue
	init-qcount      \ store "qnelems" as the number of items in the queue
	init-qhead init-qtail
	init-qdata ;     \ put the numbers 0..qnelems-1 into the queue,

initq

: q-full?
	get-qcount   \ get the count
	qnelems >= ; \ is the queue full?

: q-empty?
	get-qcount  \ get the count byte
	0= ;  \ is it 0 (empty)?

: inc-qcount ( -- )
	q-full? if
		q-overflow throw else
		get-qcount 1+ store-qcount then ;

: dec-qcount ( -- )
	q-empty? if
		q-underflow throw else
		get-qcount 1- store-qcount then ;

: update-q ( qhead-or-qtail --  )
	dup c@  \ get qhead or qtail current value
	dup qlastelem >= if  \ is it at the end of the queue?
		drop qfirstelem else \ drop the value and return to the head of the queue
		1+ then         \ otherwise, add 1
	swap c! ;           \ and store it

: enqueue-elem ( n -- ) \ the mechanics of enqueueing an element at the q tail
	inc-qcount \ throws an error if queue is full
	q qt + update-q \ update the q tail. First it will be 0, then 1, etc.
	q qt + c@   \ get the new value it will be n mod qnelems (0 - 39)
	q qdata +   \ get the starting point...the queue data
	+           \ add the offset
	c! ;     \ store the element (enqueue it)

: dequeue-elem ( -- n ) \ dequeue the first element
	dec-qcount \ throws an error if queue is empty
	q qh + c@  \ get the head value, first time it is 0, then 1, etc.
	           \ this value is an offset into qdata ( q + qdata )
	q qdata +  \ find the starting point, "q + qdata"
	+          \ add the head value (first 0, then 1, etc.)
	dup
	c@         \ retrieve the value stored in "q + qdata + offset"
	swap       \ ( value addr )
	EMPTY swap c! \ and store an "EMPTY" value in the queue data
	q qh + update-q ;      \ where we just retrieved a value
                        \ and update the q pointer for the q head



