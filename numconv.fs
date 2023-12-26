\ This file attempts to define words that convert a number to the
\ associated string. e.g. the number 2023 to the ASCII representation "2023",
\ so the number can be represented with a series of emit or a type statement.
\ The problem with just using "." is that it prints the number followed by a
\ space. but we want to possibly build a buffer that will print "11/30/2023".

\ Problem solved 11/30/2023 by using ".r"
 
warnings off
require caseext.fs
warnings on

: testlen ( n -- len )
case
	dup 1000 >= ?of drop 4 endof 
	dup 100 >= ?of drop 3 endof
	dup 10 >= ?of drop 2 endof
	dup 1 >= ?of drop 1 endof
0 endcase	;
		
: how-many-zeros ( n width -- number-of-zeros )
	swap dup testlen 2 pick swap - -rot 2drop ;

: 0fill ( n width )
	2dup how-many-zeros 0 u+do 0 1 .r loop drop dup testlen .r ;
decimal
: dispdt ( DD MM YY -- )
	-rot dup testlen .r 47 emit dup testlen .r 47 emit
	2000 + dup testlen .r ;

0 value AMPM \ 0 = "AM", 1 = "PM"
decimal
: disptm ( min hr -- ) \ convert 24-hour time to 12-hour + AM/PM
	case
		dup 13 >= ?of 1 to AMPM 12 - endof
	    dup 12 = ?of 1 to AMPM endof
	    dup 0= ?of 0 to AMPM 12 + endof
        dup 1 >= ?of 0 to AMPM endof
	0 endcase

	dup testlen .r [char] : emit dup testlen dup 1 = if
		0 1 .r then
	.r
	AMPM 0= if
		s"  AM" type
	else
		s"  PM" type
	then ;
	
