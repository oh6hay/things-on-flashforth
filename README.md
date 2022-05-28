# things-on-flashforth

A collection of Forth code to make random things work on an Arduino
board with [FlashForth](https://flashforth.com/) on it

### `lcd.fs` and `lcd-utils.fs`

Controller for Hitachi 44780 controller aka. making the 16x2 LCD
keypad shield to work. 

### `adc.fs` and `adc-utils.fs`

Access the built-in AD converter, pins 0 and 1, code is from the Flashforth tutorial

### SSD1306

Driver for an SSD1306 based OLED on i2c bus.
