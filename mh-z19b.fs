\ Winson MH-Z19B co2 sensor
\ pwm mode
\ sensor hooked to digital pin 2 = PORTD2

-mhz19b
marker -mhz19b

$002b constant portd
$002a constant ddrd
$0029 constant pind

: mhz-init
  ddrd c@
  $fb and  \ set D2 as input
  ddrd c!
;

: readpin2
  pind c@ $04 and
;

: waitlow ( -- ) \ blocks until D2 is low
  begin
    readpin2 0=
  until
;

: waithigh
  begin
    readpin2
  until
;


\ this should be computed as below
\ : get1pulse
\   waitlow
\   waithigh ticks
\   waitlow ticks
\   waithigh ticks
\   over -
\   rot rot swap -  \ now we have TL TH
\   over over . . cr
\   dup 2 - 2000 * \ TL TH 2000*TH-2
\   rot rot \ TH-2 TL TH
\   + 4 -
\   /
\ ;


\ a good enough approximation... the hi+low pulses
\ are about 1002 ms if all is ok so assume all is ok
: get1pulse
  waitlow waithigh ticks
  waitlow ticks swap - 2 - 2 *
;


: dnum s>d <# #s #> 0 do dup c@ drc 1 spc 1+ loop drop ;

: co2loop
  ssdi clshome
  begin
    get1pulse
    clshome
    s" CO2:  " dtxt dnum
    key?
  until
;
