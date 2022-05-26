-lcd-utils
marker -lcd-utils

: to-string ( n -- addr c )  s>d <# #s #> ;

: sendint
  to-string sendstring
;
