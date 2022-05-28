-ssd-commands
marker -ssd-commands

\ see SSD1306 datasheet https://cdn-shop.adafruit.com/datasheets/SSD1306.pdf

: makecmd ( command -- ) \ makes a one-byte command
  create , does> @ dcmd ;
: makedcmd ( command1 command2 -- ) \ makes a two-byte command
  create , , does> @+ swap @ swap command-stream i2c.w2 ;

$a4 makecmd display-enable
$a5 makecmd display-on

$a7 makecmd invert-on
$a6 makecmd invert-off

$26 $2f makedcmd scroll-on
$2e makecmd scroll-off

$ae makecmd oled-off
$af makecmd oled-on

\ **
\ ** ADDRESSING **

$20 $00 makedcmd hori-addr-mode
$20 $01 makedcmd vert-addr-mode
$20 $10 makedcmd page-addr-mode

\ Sets column start address. Only for page addressing mode
: set-col-addr ( column-address -- )
  dup $0f and
  swap $f0 and 4 rshift $10 or
  2 dcmds
;

\ sets column start and end address. Only for horizontal
\ or vertical addressing mode. start, end 0..127
: set-col-bounds ( start end -- )
  i2c.startcmds
  $21 i2c.wb          \ $21 setup column start and end address
  swap $7f and i2c.wb \ start
  $7f and i2c.wb      \ end
  i2c.stop
;

\ sets page start and end address. only for horizontal
\ or vertical addressing mode. start, end 0..7
: set-page-bounds ( start end -- ) \ 0..7
  i2c.startcmds
  $22 i2c.wb          \ $22 setup page start and end address
  swap $07 and i2c.wb \ start address
  $07 and i2c.wb      \ end address
  i2c.stop
;

\ sets GDDRAM page start address PAGE0..PAGE7
\ for page addressing mode
: set-page-start ( start -- ) \ 0..7
  $07 and $b0 or dcmd
;

\ tests
: foo s" foobar" dtxt ;
