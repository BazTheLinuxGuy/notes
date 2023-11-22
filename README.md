# notes
A gforth application (in progress) that takes/reads/deletes/etc notes from the command line.

This application has just been started. It is envisioned to be a series of smaller programs that are called from the command line. 

The purpose is to write down and sotre a note to yourself when you are in the middle of doing somnething else at the command prompt.

For example, you are debugging and compiling a program and are at a command prompt. You get a call from your dentist remindiing you 
that you have an appointment at 1:00 PM Thursday. You quickly dash a note to yourself by entering (as an example):

bazimmer ==> ~ $ makenote [enter]
You get a prompt
Enter note: _
where you can enter "Dentist appointment Thursday 1:00 PM" [enter]
The program gives you an "ok" and returns you to the prompt.

To read your note you simply enter: $ readnote dentist [enter]
and your note is displayed. Again, you are returned to the command prompt.

I envision being able to read all notes stored in the sequence entered, read by index number, or by keyword or regular expression.

The notes are stored to a file on every entry. Notes can be entered, read, or deleted.
This is just the begining of this project. I have a long way to go.


