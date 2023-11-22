[IFDEF] this-code
	this-code
[ENDIF]

marker this-code
init-included-files

\ Messages struct
\ Messages 1. Number of messages
\          2. First message ptr
\          3. Next msg ptr
\          4. Total messages

\ Struct for each messages: 1. Message number 2. Active? 3. Next message
\ 4. Previous msg  5.len
\ 6-8. Date/time stamp of when it was written. 4. Author

\ : getnextptr \ They have to give a message number to start with
\	;

decimal

20 constant maxlines
128 constant maxline
40 constant  maxmsgs
16 constant  msgbufsz
\ create line-buffer maxline 2 + allot


0 Value fd-in
1 Value fd-out
: open-input ( addr u -- )  r/o open-file throw to fd-in ;
: open-output ( addr u -- )  w/o create-file throw to fd-out ;
: close-input ( -- )  fd-in close-file throw ;
: close-output ( -- )  fd-out close-file throw ;

create line-buffer maxline 2 + chars allot


: prfile-pg ( -- )
	cr
	maxlines >r
	begin
		r> 1 - >r
		line-buffer maxline fd-in read-line throw
	while
		line-buffer swap stdout write-line throw
		r> dup 0= if
			cr ." Press any key to continue: " key drop cr
			drop maxlines
		then
		>r
    repeat
	r>  2drop drop ;

: pgfile ( -- )
	s" messages2.fs" open-input prfile-pg close-input ;


Variable messages maxline maxmsgs * msgbufsz + chars allot
   \ 16 for the header

: get-first-msg messages 1+ c@ ; \ which message starts the linked list?
: put-first-message messages 1+ c! ;
: get-total-msgs messages 3 + c@ ; \ total number of messages saved.
: put-total-msgs messages 3 + c! ;


: messages+len messages maxline maxmsgs * ;
: msgptr messages maxline maxmsgs * + ;
: INPUT$ ( nchars -- n n )
	pad swap accept pad swap ;

: Message-array
    \ len msgs -- ; [child] n -– addr
	\ generate an array for msgs messages of size len
Create
\ create data word
Over , \ save the size of the array
 * \ calculate space needed
Here over erase \ clear memory
	allot \ reserve space for it
   Does> \ run time gives address of data
	Dup @ \ get length
	Rot * \ calculate offset
  	+ \ add to base
	cell + \ skip length
 ;


: get-content
	cr ." Enter your nickname : " 20 INPUT$
   	fd-out write-file cr ;

: getfn-out \ get filename
	cr ." Enter file name : " 20 INPUT$
	w/o create-file throw to fd-out ;

: getfn-in cr ." Enter file name: " 20 INPUT$
	r/o open-file throw to fd-in ;

: addlf \ Inject a carriage return at end of file
	s\" \n" fd-out write-file ;

: runit getfn-out get-content addlf close-output ;

: MsgArray ( len msgs -- ) create over , * here over erase chars allot
  does> ( n -- addr ) maxline * + ;

: msgstore ( addr u n -- ) maxline * messages +  swap cmove ;
s" Here is another test." 0 msgstore
: clear ( msgN -- ) maxline * messages + maxline 0 fill ;
: clearall messages maxmsgs 0 do i clear loop ;
clearall

s" THIRD damn message occupies this space!" 2 msgstore
clearall
s" This should be the first message." 1 msgstore
s" This is the second full message, there is too much space in the buffer." 2 msgstore


\ messages maxline chars dump


\ 0 clear
\ 1 clear
\ 2 clear
\ 3 clear


: $len ( nth -- len ) maxline * messages + 127 0 do dup i + c@ 0=
	if drop i unloop exit then
    loop nip ;

: message-ptr maxline * messages + ; \ ?

: getmsg ( nth -- addr u ) dup message-ptr >r $len r> swap ;
\ 2 getmsg
\ .s
\ type
\ 1 getmsg type cr
\ 0 getmsg type cr
\ words
\
s" This will be the ULTIMATE last one I have time for" 3 msgstore

3 getmsg type cr

: put-messages-to-file
    messages+len fd-out write-file cr ;

: get-messages-from-file messages+len fd-in read-file ;

: write-msgs-to-file getfn-out put-messages-to-file close-output ;

: read-msgs-from-file getfn-in get-messages-from-file 0<>
	if
		." error reading input file."
	else
		." Sucessfully read file into messages buffer."
	then
	drop close-input ;



: findmsg ( u -- msgptr len ) dup maxline * messages msgbufsz + +
	dup $len drop ;

: putmsg ( addr u -- 1|0 ) messages 2 + c@ messages msgbufsz + swap
	maxline * + ;
: firstmsg messages msgbufsz + ;
: init-messages ( -- )
	messages 0 + 0 swap cr .s c! cr .s dup 1 + 1 swap cr .s c! cr .s dup 2 + 1 swap cr .s c! cr .s dup 3 + 0 swap cr .s c! cr .s 2drop cr .s ;

: nextmsg maxline + ;

: init-each-msg messages cr .s msgbufsz + cr .s maxmsgs 0 cr .s u+do cr .s dup i swap cr .s c! cr .s  nextmsg loop ;
: getnextmsg messages 2 + c@ ;
: storemsg ( [0-39] -- 1|0 ) dup firstmsg swap maxline * ;
