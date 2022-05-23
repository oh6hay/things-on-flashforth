\ Utilities to use the 16x2 LCD + buttons shield for arduino
\ which has a Hitachi 44780 controller
\ references:
\ * https://github.com/arduino-libraries/LiquidCrystal
\ * flashforth documentation

-lcd
marker -lcd

\ remember to load bit.fs, asm.fs, us.fs

\ note that all of below definitions may not be used (at this time)

$0025 constant portb
$0024 constant ddrb
$0023 constant pinb

$0028 constant portc
$0027 constant ddrc
$0026 constant pinc

$002b constant portd
$002a constant ddrd
$0029 constant pind

\ LCD COMMANDS
$01 constant cleardisplay
$02 constant returnhome
$04 constant entrymodeset
$08 constant controlset
$10 constant cursorshift
$20 constant functionset
$40 constant setcgramaddr
$80 constant setddramaddr

\ ENTRY MODE FLAGS
$00 constant entryright
$02 constant entryleft
$01 constant entryshiftinc
$00 constant entryshiftdec

\ FUNCTION SET FLAGS
$10 constant mode4bit
$08 constant mode2line
$00 constant dots58

\ DISPLAY ON/OFF FLAGS
$04 constant displayon
$00 constant displayoff
$02 constant cursoron
$00 constant cursoroff
$01 constant blinkon
$00 constant blinkoff

\ DISPLAY/CURSOR SHIFT FLAGS
$08 constant displaymove
$00 constant cursormove
$04 constant moveright
$00 constant moveleft

\ STATE
value function
value control
value mode

\ ***** pin mappings
\ D0 (rx) - pd0
\ D1 (tx) - pd1
\ D2-D7   - pd2-pd7
\ D8      - pb0
\ A0-A5   - pc0-pc5
\ d9-d13  - pb1-pb5

\ According to some tutorial, pins:
\ lcd pin - arduino pin - address
\ RS      - D8          - PB0
\ Enable  - D9          - PB1
\ D4-D7   - D4-D7       - PD4-PD7
\ r/w - gnd
\ vss - gnd
\ vcc - vcc

: millis ( n -- )
  begin 1000 us 1- dup 0= until drop
;

portb 0 bit0: rslo
portb 0 bit1: rshi
portb 1 bit0: enlo
portb 1 bit1: enhi
portd 4 bit0: d4off
portd 4 bit1: d4on
portd 5 bit0: d5off
portd 5 bit1: d5on
portd 6 bit0: d6off
portd 6 bit1: d6on
portd 7 bit0: d7off
portd 7 bit1: d7on

: initlcdpins ( -- )
  $03 ddrb c! \ set D8,D9 (pb0,pb1)/ RS,EN as outputs
  $f0 ddrd c! \ set d4-d7 (pd4-pd7) as outputs
;


: pulseenable ( -- )
  enlo
  4 us
  enhi
  4 us
  enlo
  400 us
;

: write4bits ( value -- )   \ sends lower 4 bits of value
  $0f and #4 lshift portd c!
  pulseenable
;

: sendbyte ( value mode -- ) \ sends full byte, on given mode
  0= if rslo else rshi then
  dup 4 rshift write4bits
  write4bits
;

: command ( value -- )
  0 sendbyte
;

: write ( value -- )
  1 sendbyte
;

\ high level stuff
: setdisplayon ( -- )
  control displayon or to control
  control command
;
: setdisplayoff ( -- )
  control displayon xor to control
  control command
;

: setcursoron ( -- )
  control cursoron or to control
  control command
;

: setcursoroff ( -- )
  control cursoron xor to control
  control command
;

: setcursor ( col row -- )
  0= if 0 else 63 then +
  setddramaddr or command
;

: clear ( -- )
  cleardisplay command 2000 us
;

: home ( -- )
  returnhome command 2000 us
;

: sendfunction ( value -- )
  functionset or command
;

: sendmode ( value -- )
  entrymodeset or command
;

: sendcontrol ( value -- )
  controlset or command
;

: sendstring ( c-addr n -- )
  for c@+ write next drop
;

\ TODO make it easier to send arbitrary text
  


\ ********** actually init stuff
: initlcd ( -- )
  0 to control
  0 to mode
  mode4bit mode2line or dots58 or to function
  initlcdpins
  50 millis
  rslo enlo
  function sendfunction
  4500 us
  function sendfunction
  150 us
  function sendfunction
  function sendfunction
  control displayon or cursoroff or blinkoff or to control
  control sendcontrol
  clear
  entryleft entryshiftdec or to mode
  mode sendmode
  displayon cursoron blinkon or or to control
  control sendcontrol
  clear home 0 0 setcursor
;


: startlcd ( -- )
  initlcdpins
  initlcd
;

: hello ( -- )
  0 0 setcursor s" hello" sendstring
  0 1 setcursor s" world" sendstring
;
