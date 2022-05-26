-adc-utils
marker -adc-utils

: adc.lcd
  adc.init 
  clear home
  begin
    0 adc.select adc@
    0 0 setcursor s" adc0 " sendstring sendint
    1 adc.select adc@
    0 1 setcursor s" adc1 " sendstring sendint
    #500 ms
    clear home
    key?
  until
  adc.close
;

