\ Borrowed from https://github.com/TG9541/forth-oled-display
\ also: see https://cdn-shop.adafruit.com/datasheets/SSD1306.pdf

\ datasheet says, writing works like this
\ 1. master sets start condition
\ 2. then, slave address, here it's $3c
\ 3. write mode established, r/w bit to 0
\ 4. ack this
\ 5. send either control byte or data byte.
\      control byte: Co, D/C#, 0, 0, 0, 0, 0, 0
\           Co: continuation bit
\           D/C#: data/command selection bit
\      Co == 0: following bytes are data
\      Co == 1: following bytes are command
\         D/C# == 0: following byte is command
\         D/C# == 1: following byte is stored at GDDRAM
\                    and in this case GDDRAM address pointer
\                    is automatically increased
\ 6. acknowledge bit will be generated
\ 7. write mode is finished by applying stop condition
\
\ Co=1, D/C#=0: %10000000 = $80     Next one is command
\ Co=1, D/C#=1: %11000000 = $C0     Next one goes to GDDRAM
\ Co=0, D/C#=1: %01000000 = $40     Not data, goes to GDDRAM
\                                   ^ draw 8bits and incr addr?
\ The above doesn't make much sense but datasheet says so
\ however, https://github.com/Matiasus/SSD1306/blob/master/lib/ssd1306.h
\ suggests the Co and D/C# bits mean:
\ Co   = 1: single thing
\ Co   = 0: stream of things
\ D/C# = 1: command
\ D/C# = 0: data
\ therefore:
\ $80 -> single command
\ $00 -> stream of commands
\ $40 -> single byte of data
\ $00 -> stream of data bytes

-ssd1306
marker -ssd1306

$80 constant command-byte
$00 constant command-stream
$c0 constant data-byte
$40 constant data-stream

\ i2c write one byte, discard whether it succeeded
: i2c.wb ( n -- )
  \ dup ." write: " . cr \ uncomment if you mess up.
  i2c.c! drop
;

\ i2c set write address ($3c)
: i2c.wsa ( -- )
  \ ." WSA" cr \ uncomment if you mess up
  $3c 2* i2c.wb
;

\ i2c write one byte with control byte first
: i2c.w ( b control-byte -- )
  i2c.start i2c.wsa i2c.wb i2c.wb i2c.stop
;

\ i2c write two bytes with control byte first
: i2c.w2 ( b b control-byte -- )
  i2c.start i2c.wsa i2c.wb swap i2c.wb i2c.wb i2c.stop
;

\ start i2c, send the start-command-stream byte
: i2c.startcmds
  i2c.start i2c.wsa command-stream i2c.wb
;

\ display command:
: dcmd ( b --) command-stream i2c.w ;

\ multiple display commands:
: dcmds ( b b .. b n --)
  0 do dcmd loop
;

\ i2c write n bytes from pointer, with control byte first
: i2c.wsp ( pointer n control-byte -- )
  i2c.start i2c.wsa i2c.wb
  0 do
    dup i + c@ i2c.wb
  loop
  i2c.stop drop
;


create ssd-init
\ * = vccstate dependant
 $ae c,  \ ssd1306_displayoff
 $d5 c,  \ ssd1306_setdisplayclockdiv
 $80 c,  \ 
 $a8 c,  \ ssd1306_setmultiplex
 $3f c,  \ ssd1306_lcdheight - 1
 $d3 c,  \ ssd1306_setdisplayoffset
 $0  c,  \ no offset
 $40 c,  \ ssd1306_setstartline \ line #0
 $8d c,  \ ssd1306_chargepump
 $14 c,  \ *
 $20 c,  \ ssd1306_memorymode
 $0  c,  \ 0x0 act like ks0108
 $a0 c,  \ ssd1306_segremap
 $c0 c,  \ ssd1306_comscandec
 $da c,  \ ssd1306_setcompins
 $12 c,  \ 
 $81 c,  \ ssd1306_setcontrast
 $cf c,  \ *
 $d9 c,  \ ssd1306_setprecharge
 $f1 c,  \ *
 $db c,  \ ssd1306_setvcomdetect
 $40 c,  \ 
 $a4 c,  \ ssd1306_displayallon_resume
 $a6 c,  \ ssd1306_normaldisplay
 $2e c,  \ ssd1306_deactivate_scroll
 $af c,  \ ssd1306_displayon
 
\ Initialise display
: ssdi ( --)
  i2c.init
  ssd-init #27 command-stream i2c.wsp
;

 
\ write byte in display memory:
: wram ( b --) 
   data-byte i2c.w ;

variable page $7f allot 

\ Write page:
: wpage ( --)
   0 $10 2 dcmds page $80 data-stream i2c.wsp ; 

\ Write screen = 8 pages: 
: wsc ( --)
  8 0 do
    i $b0 + dcmd wpage
  loop
;

\ Fill screen with character
: fscr ( b --)
  page $80 rot fill wsc
;

\ Clear screen
: cls  ( --)
  0 fscr
;

\ Write pattern:
: test ( --)
  $b0 0 $10 3 dcmds
  8 0 do
    i $b0 + dcmd
    $10 0 do
      $ff wram
    loop
  loop
;


hex
create font   \ 5x8    
  00 c, 00 c, 00 c, 00 c, 00 c, \
  00 c, 00 c, 4f c, 00 c, 00 c, \ !
  00 c, 03 c, 00 c, 03 c, 00 c, \ "
  14 c, 3e c, 14 c, 3e c, 14 c, \ #
  24 c, 2a c, 7f c, 2a c, 12 c, \ $
  63 c, 13 c, 08 c, 64 c, 63 c, \ %
  36 c, 49 c, 55 c, 22 c, 50 c, \ &
  00 c, 00 c, 07 c, 00 c, 00 c, \ '
  00 c, 1c c, 22 c, 41 c, 00 c, \ (
  00 c, 41 c, 22 c, 1c c, 00 c, \ )
  0a c, 04 c, 1f c, 04 c, 0a c, \ *
  04 c, 04 c, 1f c, 04 c, 04 c, \ +
  50 c, 30 c, 00 c, 00 c, 00 c, \ ,
  08 c, 08 c, 08 c, 08 c, 08 c, \ -
  60 c, 60 c, 00 c, 00 c, 00 c, \ .
  00 c, 60 c, 1c c, 03 c, 00 c, \ /
  3e c, 41 c, 49 c, 41 c, 3e c, \ 0
  00 c, 02 c, 7f c, 00 c, 00 c, \ 1
  46 c, 61 c, 51 c, 49 c, 46 c, \ 2
  21 c, 49 c, 4d c, 4b c, 31 c, \ 3
  18 c, 14 c, 12 c, 7f c, 10 c, \ 4
  4f c, 49 c, 49 c, 49 c, 31 c, \ 5
  3e c, 51 c, 49 c, 49 c, 32 c, \ 6
  01 c, 01 c, 71 c, 0d c, 03 c, \ 7
  36 c, 49 c, 49 c, 49 c, 36 c, \ 8
  26 c, 49 c, 49 c, 49 c, 3e c, \ 9
  00 c, 33 c, 33 c, 00 c, 00 c, \ :
  00 c, 53 c, 33 c, 00 c, 00 c, \ ;
  00 c, 08 c, 14 c, 22 c, 41 c, \ <
  14 c, 14 c, 14 c, 14 c, 14 c, \ =
  41 c, 22 c, 14 c, 08 c, 00 c, \ >
  06 c, 01 c, 51 c, 09 c, 06 c, \ ?
  3e c, 41 c, 49 c, 15 c, 1e c, \ @
  78 c, 16 c, 11 c, 16 c, 78 c, \ a
  7f c, 49 c, 49 c, 49 c, 36 c, \ b
  3e c, 41 c, 41 c, 41 c, 22 c, \ c
  7f c, 41 c, 41 c, 41 c, 3e c, \ d
  7f c, 49 c, 49 c, 49 c, 49 c, \ e
  7f c, 09 c, 09 c, 09 c, 09 c, \ f
  3e c, 41 c, 41 c, 49 c, 7b c, \ g
  7f c, 08 c, 08 c, 08 c, 7f c, \ h
  00 c, 41 c, 7f c, 41 c, 00 c, \ i
  38 c, 40 c, 40 c, 41 c, 3f c, \ j
  7f c, 08 c, 08 c, 14 c, 63 c, \ k
  7f c, 40 c, 40 c, 40 c, 40 c, \ l
  7f c, 06 c, 18 c, 06 c, 7f c, \ m
  7f c, 06 c, 18 c, 60 c, 7f c, \ n
  3e c, 41 c, 41 c, 41 c, 3e c, \ o
  7f c, 09 c, 09 c, 09 c, 06 c, \ p
  3e c, 41 c, 51 c, 21 c, 5e c, \ q
  7f c, 09 c, 19 c, 29 c, 46 c, \ r
  26 c, 49 c, 49 c, 49 c, 32 c, \ s
  01 c, 01 c, 7f c, 01 c, 01 c, \ t
  3f c, 40 c, 40 c, 40 c, 7f c, \ u
  0f c, 30 c, 40 c, 30 c, 0f c, \ v
  1f c, 60 c, 1c c, 60 c, 1f c, \ w
  63 c, 14 c, 08 c, 14 c, 63 c, \ x
  03 c, 04 c, 78 c, 04 c, 03 c, \ y
  61 c, 51 c, 49 c, 45 c, 43 c, \ z
  00 c, 7f c, 41 c, 00 c, 00 c, \ [
  00 c, 03 c, 1c c, 60 c, 00 c, \ \
  00 c, 41 c, 7f c, 00 c, 00 c, \ ]
  0c c, 02 c, 01 c, 02 c, 0c c, \ ^
  40 c, 40 c, 40 c, 40 c, 40 c, \ _
  00 c, 01 c, 02 c, 04 c, 00 c, \ `
  20 c, 54 c, 54 c, 54 c, 78 c, \ a
  7f c, 48 c, 44 c, 44 c, 38 c, \ b
  38 c, 44 c, 44 c, 44 c, 44 c, \ c
  38 c, 44 c, 44 c, 48 c, 7f c, \ d
  38 c, 54 c, 54 c, 54 c, 18 c, \ e
  08 c, 7e c, 09 c, 09 c, 00 c, \ f
  0c c, 52 c, 52 c, 54 c, 3e c, \ g
  7f c, 08 c, 04 c, 04 c, 78 c, \ h
  00 c, 00 c, 7d c, 00 c, 00 c, \ i
  00 c, 40 c, 3d c, 00 c, 00 c, \ j
  7f c, 10 c, 28 c, 44 c, 00 c, \ k
  00 c, 00 c, 3f c, 40 c, 00 c, \ l
  7c c, 04 c, 18 c, 04 c, 78 c, \ m
  7c c, 08 c, 04 c, 04 c, 78 c, \ n
  38 c, 44 c, 44 c, 44 c, 38 c, \ o
  7f c, 12 c, 11 c, 11 c, 0e c, \ p
  0e c, 11 c, 11 c, 12 c, 7f c, \ q
  00 c, 7c c, 08 c, 04 c, 04 c, \ r
  48 c, 54 c, 54 c, 54 c, 24 c, \ s
  04 c, 3e c, 44 c, 44 c, 00 c, \ t
  3c c, 40 c, 40 c, 20 c, 7c c, \ u
  1c c, 20 c, 40 c, 20 c, 1c c, \ v
  1c c, 60 c, 18 c, 60 c, 1c c, \ w
  44 c, 28 c, 10 c, 28 c, 44 c, \ x
  46 c, 28 c, 10 c, 08 c, 06 c, \ y
  44 c, 64 c, 54 c, 4c c, 44 c, \ z
  00 c, 08 c, 77 c, 41 c, 00 c, \ {
  00 c, 00 c, 7f c, 00 c, 00 c, \ |
  00 c, 41 c, 77 c, 08 c, 00 c, \ }
  10 c, 08 c, 18 c, 10 c, 08 c, \ ~
decimal

\ Translates ASCII to address of bitpatterns:
: a>bp ( c -- c-adr ) 
  $20 max $7f min  $20 - 5 * font + ;

\ Draw character:
: drc ( c --)
  a>bp 5 0
  do
    dup c@ wram 1 +
  loop drop ;

\ spaces
: spc ( u --) 0 do 0 wram loop ;

\ display text put on stack with s"
: dtxt ( adr len --)
  \ count
  0 do dup c@ dup $20 =
       if 3 spc drop 
       else drc 1 spc then 1+
    loop drop
;
   
\ display number
: d# ( n --) s>d dup abs <# #s swap sign #> 0 
	do dup c@ drc 1 spc 1+ loop drop ;

: ssdhello ssdi cls s" hello" dtxt ;
