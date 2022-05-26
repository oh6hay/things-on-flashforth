\ needs adc.fs

-lcd-keypad
marker -lcd-keypad

$0 constant nokey
$1 constant key-select
$2 constant key-left
$3 constant key-up
$4 constant key-down
$5 constant key-right

\ keypress is readable from A0
\ limits:
\ (no key)   > 1000
\ SELECT 715 .. 735
\ LEFT   470 .. 490
\ UP     125 .. 145
\ DOWN   300 .. 320
\ RIGHT  0   .. 70

: poll-key ( -- keynum )
  adc.init 0 adc.select adc@ adc.close
  dup 1000 > if drop nokey exit then
  dup dup 715 > swap 735 < and if drop key-select exit then
  dup dup 470 > swap 490 < and if drop key-left   exit then
  dup dup 125 > swap 145 < and if drop key-up     exit then
  dup dup 300 > swap 320 < and if drop key-down   exit then
  dup dup 0 > swap 70 <    and if drop key-right  exit then
  drop nokey
;

: keytest ( -- )
  clear home
  begin
    poll-key
    dup nokey = if s" --" sendstring then
    dup key-select = if s" SELECT" sendstring then
    dup key-left = if s" LEFT" sendstring then
    dup key-up = if s" UP" sendstring then
    dup key-down = if s" DOWN" sendstring then
    key-right = if s" RIGHT" sendstring then
    0 15 setcursor
    #50 ms key?
    clear home
  until
;
  
