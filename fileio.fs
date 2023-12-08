[IFDEF] -my-fileio
	-my-fileio
[ENDIF]

marker -my-fileio
init-included-files

\ File operators

0 Value fd-in
1 Value fd-out

16 constant notes-filename-size
create notes-filename notes-filename-size chars allot
notes-filename notes-filename-size chars erase
s" NOTES" notes-filename swap cmove
cr ." Notes filename: "
notes-filename cstring>sstring type

16 constant q-filename-size
create q-filename q-filename-size chars allot
q-filename q-filename-size chars erase
s" Q" q-filename swap cmove
cr ." Queue filename: "
q-filename cstring>sstring type cr


: open-input ( addr u -- )  r/o open-file throw to fd-in ;
: open-output ( addr u -- )  w/o create-file throw to fd-out ;
: close-input ( -- )  fd-in close-file throw ;
: close-output ( -- )  fd-out close-file throw ;

: INPUT$ ( nchars -- n n )
	pad swap accept pad swap ;

: getfn-out \ get filename
	cr ." Enter file name : " 20 INPUT$
	w/o create-file throw to fd-out ;

: getfn-in cr ." Enter file name: " 20 INPUT$
	r/o open-file throw to fd-in ;


: put-notes-to-file ( addr u -- )
    fd-out write-file cr ;

: get-notes-from-file ( addr u -- )
	fd-in read-file ;

: write-msgs-to-file getfn-out put-notes-to-file close-output ;

: read-msgs-from-file getfn-in get-notes-from-file 0<>
	if
		." error reading input file."
	else
		." Sucessfully read file into notes buffer."
	then
	drop close-input ;

