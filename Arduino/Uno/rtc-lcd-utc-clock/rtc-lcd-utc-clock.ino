/* UTC Real Time Clock
 * 
 * Required Hardware:  
 * - Arduino UNO / ATMega 328P U
 * - DS3231 Real Time Clock module (with optional CR2032 battery)
 * - 20x4 Character LCD display with I2C backpack
 * - (2) normally-open momentary buttons 
 * 
 * DS3231 Real-time Clock Module
 *    https://tronixstuff.com/2014/12/01/tutorial-using-ds1307-and-ds3231-real-time-clock-modules-with-arduino/
 * 
 * 20x4 LCD w/I2C backpack
 *    Library:    https://www.arduinolibraries.info/libraries/hd44780
 *    Large Font: http://woodsgood.ca/projects/2015/01/16/large-numbers-on-small-displays/
 *    
 * Copyright (c) 2018 Paul Pagel
 * This is free software; see the license.txt file for more information.
 * There is no warranty; not even for merchantability or fitness for a particular purpose.
 */

#include "Wire.h"
#include <hd44780.h>                       // main hd44780 header
#include <hd44780ioClass/hd44780_I2Cexp.h> // i2c expander i/o class header

#define DS3231_I2C_ADDRESS 0x68
#define BTN_SET_PIN 7  // physical pin 13 on ATMega328P-U
#define BTN_VAL_PIN 8  // physical pin 14 on ATMega328P-U

enum AppStates {
  STD_DISPLAY,
  SET_DAY_NAME,
  SET_YEAR,
  SET_MONTH, 
  SET_DAY,
  SET_HOUR,
  SET_MIN,
  SET_SEC
};
enum AppStates appState = STD_DISPLAY;

hd44780_I2Cexp lcd; // declare lcd object: auto locate & auto config expander chip
const int LCD_COLS = 20;
const int LCD_ROWS = 4;

// OPTION 1 - for text on line 1
byte x10[8] = {0x00,0x00,0x00,0x00,0x00,0x07,0x07,0x07};   
byte x11[8] = {0x00,0x00,0x00,0x00,0x00,0x1C,0x1C,0x1C};
byte x12[8] = {0x00,0x00,0x00,0x00,0x00,0x1F,0x1F,0x1F};
byte x13[8] = {0x07,0x07,0x07,0x07,0x07,0x1F,0x1F,0x1F};
byte x14[8] = {0x1C,0x1C,0x1C,0x1C,0x1C,0x1F,0x1F,0x1F};
byte x15[8] = {0x1C,0x1C,0x1C,0x1C,0x1C,0x1C,0x1C,0x1C};
byte x16[8] = {0x07,0x07,0x07,0x07,0x07,0x07,0x07,0x07};
byte x17[8] = {0x00,0x00,0x0E,0x0E,0x0E,0x00,0x00,0x00};

byte row = 0,col = 0;

int blinkCounter = 0;
const int blinkOn = 5;
const int blinkMax = 10;

byte second, minute, hour, dayOfWeek, day, month, year;

// Convert normal decimal numbers to binary coded decimal
byte decToBcd(byte val)
{
  return( (val/10*16) + (val%10) );
}

// Convert binary coded decimal to normal decimal numbers
byte bcdToDec(byte val)
{
  return( (val/16*10) + (val%16) );
}

// doNumber: routine to position number 'num' starting top left at row 'r', column 'c'
void doNumber(byte num, byte r, byte c) {
    lcd.setCursor(c,r);
    switch(num) {
      case 0: lcd.write(byte(2)); lcd.write(byte(2)); 
              lcd.setCursor(c,r+1); lcd.write(byte(5)); lcd.write(byte(6)); 
              lcd.setCursor(c,r+2); lcd.write(byte(4)); lcd.write(byte(3)); break;
            
      case 1: lcd.write(byte(0)); lcd.write(byte(1)); 
              lcd.setCursor(c,r+1); lcd.print(" "); lcd.write(byte(5));
              lcd.setCursor(c,r+2); lcd.write(byte(0)); lcd.write(byte(4)); break;
            
      case 2: lcd.write(byte(2)); lcd.write(byte(2)); 
              lcd.setCursor(c,r+1); lcd.write(byte(2)); lcd.write(byte(3)); 
              lcd.setCursor(c,r+2); lcd.write(byte(4)); lcd.write(byte(2)); break;  
            
      case 3: lcd.write(byte(2)); lcd.write(byte(2)); 
              lcd.setCursor(c,r+1); lcd.write(byte(0)); lcd.write(byte(3)); 
              lcd.setCursor(c,r+2); lcd.write(byte(2)); lcd.write(byte(3)); break;  
            
      case 4: lcd.write(byte(1)); lcd.write(byte(0)); 
              lcd.setCursor(c,r+1); lcd.write(byte(4)); lcd.write(byte(3)); 
              lcd.setCursor(c,r+2); lcd.print(" "); lcd.write(byte(6)); break;  
            
      case 5: lcd.write(byte(2)); lcd.write(byte(2)); 
              lcd.setCursor(c,r+1); lcd.write(byte(4)); lcd.write(byte(2)); 
              lcd.setCursor(c,r+2); lcd.write(byte(2)); lcd.write(byte(3)); break;  
      case 6: lcd.write(byte(1)); lcd.print(" ");     
              lcd.setCursor(c,r+1); lcd.write(byte(4)); lcd.write(byte(2)); 
              lcd.setCursor(c,r+2); lcd.write(byte(4)); lcd.write(byte(3)); break;  

      case 7: lcd.write(byte(2)); lcd.write(byte(2));
              lcd.setCursor(c,r+1); lcd.print(" "); lcd.write(byte(6)); 
              lcd.setCursor(c,r+2); lcd.print(" "); lcd.write(byte(6)); break;  

      case 8: lcd.write(byte(2)); lcd.write(byte(2)); 
              lcd.setCursor(c,r+1); lcd.write(byte(4)); lcd.write(byte(3)); 
              lcd.setCursor(c,r+2); lcd.write(byte(4)); lcd.write(byte(3)); break;  
   
      case 9: lcd.write(byte(2)); lcd.write(byte(2)); 
              lcd.setCursor(c,r+1); lcd.write(byte(4)); lcd.write(byte(3)); 
              lcd.setCursor(c,r+2); lcd.print(" "); lcd.write(byte(6)); break;  

       case 11: // colon
              lcd.setCursor(c,r+1); lcd.write(byte(7)); lcd.setCursor(c,r+2); lcd.write(byte(7)); break; 

       case 12: // space
              lcd.write(" "); lcd.write(" "); 
              lcd.setCursor(c,r+1); lcd.write(" "); lcd.write(" "); 
              lcd.setCursor(c,r+2); lcd.print(" "); lcd.write(" "); 
              break;  
    } 
}

void setup()
{
  Wire.begin();
  Serial.begin(9600);
  // set the initial time here:
  // DS3231 seconds, minutes, hours, day, date, month, year
  // Sun=1, Sat = 7
  //setDS3231time(30,02,16,7,24,11,18);

  pinMode(BTN_SET_PIN, INPUT_PULLUP);
  pinMode(BTN_VAL_PIN, INPUT_PULLUP);

  int status;
  status = lcd.begin(LCD_COLS, LCD_ROWS);
  if(status) // non zero status means it was unsuccesful
  {
    status = -status; // convert negative status value to positive number

    // begin() failed so blink error code using the onboard LED if possible
    hd44780::fatalError(status); // does not return
  }

  lcd.createChar(0, x10);                      // digit piece
  lcd.createChar(1, x11);                      // digit piece
  lcd.createChar(2, x12);                      // digit piece
  lcd.createChar(3, x13);                      // digit piece
  lcd.createChar(4, x14);                      // digit piece
  lcd.createChar(5, x15);                      // digit piece
  lcd.createChar(6, x16);                      // digit piece
  lcd.createChar(7, x17);                      // digit piece (colon)
}


void setDS3231time(byte second, byte minute, byte hour, byte dayOfWeek, byte
dayOfMonth, byte month, byte year)
{
  // sets time and date data to DS3231
  Wire.beginTransmission(DS3231_I2C_ADDRESS);
  Wire.write(0); // set next input to start at the seconds register
  Wire.write(decToBcd(second)); // set seconds
  Wire.write(decToBcd(minute)); // set minutes
  Wire.write(decToBcd(hour)); // set hours
  Wire.write(decToBcd(dayOfWeek)); // set day of week (1=Sunday, 7=Saturday)
  Wire.write(decToBcd(dayOfMonth)); // set date (1 to 31)
  Wire.write(decToBcd(month)); // set month
  Wire.write(decToBcd(year)); // set year (0 to 99)
  Wire.endTransmission();
}

void readDS3231time()
{
  Wire.beginTransmission(DS3231_I2C_ADDRESS);
  Wire.write(0); // set DS3231 register pointer to 00h
  Wire.endTransmission();
  Wire.requestFrom(DS3231_I2C_ADDRESS, 7);
  // request seven bytes of data from DS3231 starting from register 00h
  second = bcdToDec(Wire.read() & 0x7f);
  minute = bcdToDec(Wire.read());
  hour = bcdToDec(Wire.read() & 0x3f);
  dayOfWeek = bcdToDec(Wire.read());
  day = bcdToDec(Wire.read());
  month = bcdToDec(Wire.read());
  year = bcdToDec(Wire.read());
}

void changeTime()
{
  if (digitalRead(BTN_VAL_PIN) == LOW)
  {
    switch(appState)
    {
      case SET_DAY_NAME :
        dayOfWeek = (byte)(dayOfWeek % 7 + 1);
        break;
      case SET_YEAR :
        year = (byte)(year % 30 + 1);
        break;
      case SET_MONTH :
        month = (byte)(month % 12 + 1);
        break;  
      case SET_DAY :
        day = (byte)(day % 31 + 1);
        break; 
      case SET_HOUR :
        hour = (byte)((hour + 1) % 24);
        break; 
      case SET_MIN :
        minute = (byte)((minute + 1) % 60);
        break; 
      case SET_SEC :
        second = (byte)((second + 1) % 60);
        break;
      default :
        break;
    }
    waitForBtnRelease(BTN_VAL_PIN);
  }
}


void waitForBtnRelease(int button)
{
  while(digitalRead(button) == LOW)
    delay(2);
}

void loop()
{
  blinkCounter = (blinkCounter + 1) % blinkMax;
  
  if (digitalRead(BTN_SET_PIN) == LOW)
  {
    appState = (appState + 1) % 8;
    waitForBtnRelease(BTN_SET_PIN);
    blinkCounter = 0;
    
    if (appState == STD_DISPLAY)
    {
      // just finished setting time, so write it to the DS3231 RTC
      setDS3231time(second, minute, hour, dayOfWeek, day, month, year);
    }
  }
  
  // retrieve data from DS3231
  if (appState == STD_DISPLAY)
    readDS3231time();
  else
    changeTime();
    
  // display the time in big numbers
  row = 1;
  byte hour1 = hour / 10;
  byte hour2 = hour % 10;

  if (appState != SET_HOUR || blinkCounter > blinkOn)
  {
    doNumber(hour1, row, 2);                      
    doNumber(hour2, row, 5);
  }
  else
  {
    doNumber(12, row, 2); // space                  
    doNumber(12, row, 5);
  }
  doNumber(11, row, 7);   // colon

  byte min1 = minute / 10;
  byte min2 = minute % 10;
  if (appState != SET_MIN || blinkCounter > blinkOn)
  {
    doNumber(min1, row, 8);
    doNumber(min2, row, 11);
  }
  else
  {
    doNumber(12, row, 8); // space                  
    doNumber(12, row, 11);
  }
  doNumber(11,row,13);  // colon

  byte sec1 = second / 10;
  byte sec2 = second % 10;

  if (appState != SET_SEC || blinkCounter > blinkOn)
  {
    doNumber(sec1, row, 14);
    doNumber(sec2, row, 17); 
  }
  else
  {
    doNumber(12, row, 14); // space                  
    doNumber(12, row, 17);
  }
  
  // display the day of the week
  lcd.setCursor(0,0);

  if (appState != SET_DAY_NAME || blinkCounter > blinkOn)
  {
    switch(dayOfWeek){
      case 1:
        lcd.print("Sun  ");
        break;
      case 2:
        lcd.print("Mon  ");
        break;
      case 3:
        lcd.print("Tue  ");
        break;
      case 4:
        lcd.print("Wed  ");
        break;
      case 5:
        lcd.print("Thur ");
        break;
      case 6:
        lcd.print("Fri  ");
        break;
      case 7:
        lcd.print("Sat  ");
        break;
      default:
        lcd.print("???  ");
        break;
    }
  }
  else
    lcd.print("     ");
    
  // display the date in yyyy-mm-dd format
  lcd.print("20");

  if (appState != SET_YEAR || blinkCounter > blinkOn)
  {
    if (year < 10) lcd.print("0");
    lcd.print(year);
  }
  else
    lcd.print("  ");
    
  lcd.print("-");

  if (appState != SET_MONTH || blinkCounter > blinkOn)
  {
    if (month < 10) lcd.print("0");
    lcd.print(month);
  }
  else
    lcd.print("  ");
    
  lcd.print("-");

  if (appState != SET_DAY || blinkCounter > blinkOn)
  {
    if (day < 10) lcd.print("0");
    lcd.print(day);
  }
  else
    lcd.print("  ");
    
  lcd.print("  UTC ");
  
  delay(100); 
}
