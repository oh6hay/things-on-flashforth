\ this is from the flashforth tutorial

-read-adc
marker -read-adc
\ Registers of interest on the ATmega328P
$78 constant adcl
$79 constant adch
$7a constant adcsra
$7b constant adcsrb
$7c constant admux
$7e constant didr0
\ Bit masks
%10000000 constant mADEN
%01000000 constant mADSC
%00010000 constant mADIF


: adc.clear.iflag ( -- )
  mADIF adcsra mset \ clear by writing 1
;

: adc.init ( -- )
  $3f didr0 c! \ Disable digital inputs 5 through 0
  $40 admux c! \ AVcc as ref , right - adjust result , channel 0
  $06 adcsra c! \ single conversion mode , prescaler 64
  mADEN adcsra mset \ enable ADC
  adc.clear.iflag
;

: adc.close ( -- )
  mADEN adcsra mclr
  adc.clear.iflag
;
: adc.wait ( -- )
  begin mADSC adcsra mtst 0= until
;
: adc.select ( u -- )
  adc.wait
  $0f and \ channel selected by lower nibble
  admux c@ $f0 and \ fetch upper nibble
  or admux c!
;

: adc@ ( -- u )
  adc.wait
  mADSC adcsra mset
  adc.wait
  adcl c@ adch c@ #8 lshift or
  adc.clear.iflag
;

: adc.test ( -- ) \ Exercise the analog converter
  adc.init
  begin
    0 adc.select adc@ ." adc0 " u.
    1 adc.select adc@ ." adc1 " u.
    cr
    #500 ms
    key? until
  adc.close
;
