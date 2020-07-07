/*
 * AD9833 Waveform Module (10 Hz to 5 MHz) 
 * 
 * Requires:
 * - AD9833 waveform generator IC or breakout board
 * - I2C OLED display, 128x64 pixels
 * - (2) rotary encoders
 * - Arduino Uno / ATmega328P, or other similar microcontroller
 * 
 * Adapted from original sketch at 
 * http://www.vwlowen.co.uk/arduino/AD9833-waveform-generator/AD9833-waveform-generator.htm
 * 
 * Copyright (c) 2020 Paul Pagel
 * This is free software; see the license.txt file for more information.
 * There is no warranty; not even for merchantability or fitness for a particular purpose.
 */
#include <Arduino.h>
#include <Wire.h>
#include <SPI.h>
#include <AD9833.h>            // AD9833 Library: https://github.com/Billwilliams1952/AD9833-Library-Arduino
#include <Rotary.h>            // Rotary encoder: https://github.com/brianlow/Rotary 
#include <U8g2lib.h>           // OLED displays:  https://github.com/olikraus/u8g2   

const float refFreq = 25000000.0;           // On-board crystal reference frequency

const int FSYNC = 10;                       // Standard SPI pins for the AD9833 waveform generator.
const int CLK = 13;                         // CLK and DATA pins are shared with the TFT display.
const int DATA = 11;

int wave = 0;
WaveformType waveType = SINE_WAVE;          // from AD9833 library

// Define rotary encoder pins.
int freqUpPin = 2;                          
int freqDownPin = 3;
int freqEnabledPin = 4;
int stepUpPin = 5;
int stepDownPin = 6;
int wavePin = 7;

bool freqEnabled = false;
unsigned long freq = 1000;    
unsigned long freqOld = freq;

unsigned long incr = 1;
unsigned long oldIncr = 1;

// Note, SCK and MOSI must be connected to CLK and DAT pins on the AD9833 for SPI
AD9833 wavegen(FSYNC);       // Defaults to 25MHz internal reference frequency

U8G2_SH1106_128X64_WINSTAR_1_HW_I2C u8g2(U8G2_R2, SCL, SDA);

Rotary r = Rotary(freqUpPin, freqDownPin);    // Rotary encoder for frequency connects to interrupt pins
Rotary i = Rotary(stepUpPin, stepDownPin);    // Rotart encoder for setting increment.

/*
 * Initialization routine, called once on startup and reset
 */
void setup() 
{ 
  Serial.begin(9600);
  Serial.println("Waveform Generator with AD9833"); 
  delay(100); 

  // Rotary encoder #1
  pinMode(freqUpPin, INPUT_PULLUP);      // Set pins for rotary encoders as INPUTS and enable
  pinMode(freqDownPin, INPUT_PULLUP);    // internal pullup resistors.
  pinMode(freqEnabledPin, INPUT_PULLUP);
  
  // Rotary encoder #2
  pinMode(stepUpPin, INPUT_PULLUP);
  pinMode(stepDownPin, INPUT_PULLUP);
  pinMode(wavePin, INPUT_PULLUP);  
  
  wavegen.Begin();
  wavegen.ApplySignal(waveType, REG0, freq);
  wavegen.EnableOutput(freqEnabled);   // Defaults to OFF
  
  u8g2.begin();
  u8g2.firstPage();
  
  // Configure interrupt for rotary encoder and enable.
  PCICR |= (1 << PCIE2);
  PCMSK2 |= (1 << PCINT18) | (1 << PCINT19);
  sei();

  updateDisplay(); 
}

/*
 * Do all drawing to the screen.  Call whenever displayed values have changed.
 */
void updateDisplay() 
{
  u8g2.firstPage();
  do 
  {
    u8g2.setFont(u8g2_font_7x14_mf);
    u8g2.setFontMode(0);  // not transparent
    switch (waveType) 
    {
      case SINE_WAVE: 
        u8g2.drawStr(0, 38, "sine"); break;
      case TRIANGLE_WAVE: 
        u8g2.drawStr(0, 38, "triangle"); break;
      case SQUARE_WAVE: 
        u8g2.drawStr(0, 38, "square"); break;
      case HALF_SQUARE_WAVE: 
        u8g2.drawStr(0, 38, "half square"); break;
    }

    u8g2.drawStr(112, 38, "Hz");

    drawWaveForm();
    
    u8g2.setFont(u8g2_font_inb16_mn); //u8g2_font_9x18B_mn;
    u8g2.setCursor(0, 16);
    drawFreq(freq);   
  } while ( u8g2.nextPage() ); 
  
}

/*
 * Top-level waveform drawing function
 */
void drawWaveForm()
{
  int base_y = 62;
  int period_wd = 16;  // TODO: adjust based on frequency?
  int amplitude = 16;  // Future Idea: add controls and amplifier to control output amplitude
  
  if (freqEnabled)
  {
    switch (waveType) 
    {
      case SINE_WAVE: 
        drawSineWave(base_y, period_wd, amplitude); break;
      case TRIANGLE_WAVE: 
        drawTriangleWave(base_y, period_wd, amplitude); break;
      case SQUARE_WAVE: 
        drawSquareWave(base_y, period_wd, amplitude); break;
      case HALF_SQUARE_WAVE: 
        drawSquareWave(base_y, period_wd * 2, amplitude); break;
    }
  }
  else
  {
    // Output not enabled - draw flat line
    u8g2.drawHLine(0, base_y, u8g2.getDisplayWidth());
  }
}

/*
 * Draws a sine wave with the specified parameters
 */
void drawSineWave(int base_y, int period_wd, int amplitude)
{
  uint8_t width = u8g2.getDisplayWidth();
  double two_pi_scaled = 2 * PI / period_wd;
  int16_t y, ctr_y, half_amp;

  half_amp = amplitude / 2;
  ctr_y = base_y - half_amp;
  
  for (uint8_t x = 0; x <  width; x++)
  {
    y = (int16_t)(half_amp * sin((double)x * two_pi_scaled));
    u8g2.drawPixel(x, ctr_y + y);
  }
}

/*
 * Draws a square wave with the specified parameters
 */
void drawSquareWave(int base_y, int period_wd, int amplitude)
{
  uint8_t width = u8g2.getDisplayWidth();
  uint8_t x_step = period_wd / 2;
  
  for (uint8_t x = 0; x <  width; x += period_wd)
  {
    u8g2.drawHLine(x, base_y, x_step);
    u8g2.drawVLine(x + x_step, base_y - amplitude, amplitude);
    u8g2.drawHLine(x + x_step, base_y - amplitude, x_step);
    u8g2.drawVLine(x + period_wd, base_y - amplitude, amplitude);
  }
}

/*
 * Draws a triangle wave with the specified parameters
 */
void drawTriangleWave(int base_y, int period_wd, int amplitude)
{
  uint8_t width = u8g2.getDisplayWidth();
  uint8_t x_step = period_wd / 2;
  
  for (uint8_t x = 0; x <  width; x += period_wd)
  {
    u8g2.drawLine(x, base_y, x + x_step,  base_y - amplitude);
    u8g2.drawLine(x + x_step,  base_y - amplitude, x + period_wd, base_y);
  }
}

/*
 * Draws the frequency value with comma separators and highlighted edit digit
 */
void drawFreq(unsigned long value) 
{
  unsigned long j = 1000000;
  
  for (int i = 6; i >= 0; i--) 
  {
    int digit = (value / j) % 10;
    incr == j ? u8g2.setDrawColor(0): u8g2.setDrawColor(1);  // inverted or normal
    u8g2.print(digit);
    if ((i == 6) || (i == 3))   // Add commas at millions and thousands
    {             
      u8g2.setDrawColor(1);
      u8g2.print(",");
    }   
    j /= 10;
  }

  u8g2.setDrawColor(1); // normal
} 

/*
 * Main program loop.  Called repeatedly.
 */
void loop() 
{
  if (oldIncr != incr) 
  {
    Serial.print("Incr changed"); Serial.println(incr);
    updateDisplay();
    oldIncr= incr;
  }
  
  // Check 'increment' rotary encoder. Increase or decrease 'increment' by a factor of x10
  // if encoder has been turned.
  unsigned char result = i.process();
  if (result) 
  {
    if (result == DIR_CW)  {if (incr < 1000000) incr *= 10;}
    if (result == DIR_CCW) {if (incr >= 10) incr /= 10;}
    Serial.print("Rotary result changed: ");  Serial.println(incr);
    updateDisplay();
  }
  
  // Check if push button on 'increment' rotary encoder is pushed and set Wave Type accordingly.
  if (digitalRead(wavePin) == LOW) 
  {
    wave += 1;
    if (wave > 3) wave = 0;
    switch (wave) 
    {
      case 0: waveType = SINE_WAVE; break;
      case 1: waveType = TRIANGLE_WAVE; break;
      case 2: waveType = SQUARE_WAVE; break;
      case 3: waveType = HALF_SQUARE_WAVE; break;
    }    
    Serial.print("WaveType changed: "); Serial.println(waveType);
    wavegen.ApplySignal(waveType, REG0, freq);   // Set AD9833 to frequency and selected wave type.
    updateDisplay();
    delay(200);
  }

  // Check if push button on 'frequency' rotary encoder is pushed and set output enabled accordingly.
  if (digitalRead(freqEnabledPin) == LOW) 
  {
    freqEnabled = !freqEnabled;
    Serial.print("Freq output enabled changed: "); Serial.println(freqEnabled);
    wavegen.EnableOutput(freqEnabled);  
    updateDisplay();
    delay(200);
  }
  
  // If frequency has changed, interrupt rotary encoder
  if (freq != freqOld) 
  {                 
    Serial.print("Frequency changed: "); Serial.println(freq);
    wavegen.ApplySignal(waveType, REG0, freq);  // must have been turned so update AD9833 and display.
    updateDisplay();
    freqOld = freq;                             // Remember new frequency to avoid unwanted display 
  }                                             // and AD9833 updates.
}


/*
 *Interrupt service routine for the 'frequency' rotary encoder.
 */
ISR(PCINT2_vect) 
{
  unsigned char result = r.process();
  
  if (result) {
    if (result == DIR_CW) {                   // Clockwise rotation so add increment to frequency
       if ((freq + incr) < 6000000) freq+=incr;
       
    } else {
        if (freq > incr) {                    // Counter-clockwise rotation so subtract increment
          freq -= incr;                       // from frequency unless it would result in a negative
        } else {                              // number.
          if (freq >= 1) incr /= 10;
          if (incr < 1) incr = 1;             // Compensate for math rounding error.
        }  
    }
  }
}
