# things-on-flashforth

A collection of Forth code to make random things work on an Arduino
board with [FlashForth](https://flashforth.com/) on it

### Arduino 16x2 LCD keypad shield aka. Hitachi 44780 (`lcd.fs`, `lcd-keypad.fs`)

Controller for Hitachi 44780 controller aka. making the 16x2 LCD
keypad shield to work. 

### `adc.fs` and `adc-utils.fs`

Access the built-in AD converter, pins 0 and 1. Code is from the Flashforth tutorial -- couldn't find the code files in the Flashforth repo, therefore including here

### SSD1306

Driver for an SSD1306 based OLED display on i2c bus. These are widely available, search the internet for '.96" OLED i2c display for Arduino'.

### MH-Z19b

Words to read CO2 concentration using the MH-Z19b sensor.
