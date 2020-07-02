/*
 * WS2812B LED controller
 * 
 * Requires:
 *   ATTiny85 or 5V Trinket (can work on most mainstream Arduino boards)
 *   WS2812B LED lights aka NeoPixels with data line attached to Pin 0
 *   Reset button to turn off lights and reset controller
 *   Mode button to switch patterns
 *   Status LED to indicate when controller is reset or button is pressed
 *   Current-limiting resistor for LED
 * 
 * Copyright (c) 2020 Paul Pagel
 * This is free software; see the license.txt file for more information.
 * There is no warranty; not even for merchantability or fitness for a particular purpose.
 */

// SEE: https://learn.adafruit.com/adafruit-neopixel-uberguide/arduino-library-installation
#include <Adafruit_NeoPixel.h>

#define NUM_LEDS    60  
#define BRIGHTNESS 128  // 0=Off, 255=Full RGB LED Brightness

#define PIXEL_PIN    0
#define LED1_PIN     2
#define MODE_BTN     3

enum led_pattern_type 
{
  LEDS_OFF,             // Make sure this item is always *first* in the enum!
  LEDS_SOLID_WHITE,
  LEDS_SOLID_RED,
  LEDS_SOLID_GREEN,
  LEDS_SOLID_BLUE,
  LEDS_SOLID_COLOR_CYCLE,
  LEDS_CHASE_WHITE,
  LEDS_CHASE_COLOR_CYCLE,
  LEDS_RAINBOW,
  LEDS_RAINBOW_CYCLE,
  LEDS_LAST             // Make sure this item is always *last* in the enum!
};
enum led_pattern_type led_pattern;

uint8_t  base_wheel = 0;  // default to white base
uint16_t delay_ms = 20;
uint32_t base_color = 0;

bool btn_pressed = false;
 
Adafruit_NeoPixel pixels = Adafruit_NeoPixel(NUM_LEDS, PIXEL_PIN);

/*
 * Initialization routine, called once on startup and reset
 */
void setup() 
{
  // Set the pin modes and pullups
  pinMode(PIXEL_PIN, OUTPUT);
  pinMode(LED1_PIN, OUTPUT);
  pinMode(MODE_BTN, INPUT_PULLUP);

  // Blink the status LED to show the unit is on and ready
  digitalWrite(LED1_PIN, HIGH);
  delay(250);
  digitalWrite(LED1_PIN, LOW);

  // Start up the NeoPixels
  pixels.begin();
  pixels.setBrightness(BRIGHTNESS); // use only for init, not as an effect

 led_pattern = LEDS_OFF;
}

/*
 * Main program loop.  Called repeatedly.
 */
void loop() 
{ 
  if (digitalRead(MODE_BTN) == LOW)
  {
    btn_pressed = true;
  }
  else if (btn_pressed)
  {
    // The mode button was just released, so change the pattern
    digitalWrite(LED1_PIN, HIGH);
    led_pattern = (led_pattern_type)((int)led_pattern + 1);
    if (led_pattern >= LEDS_LAST) led_pattern = LEDS_OFF;
    btn_pressed = false;
  }
  else
  {
    btn_pressed = false;
  }
    
  switch(led_pattern)
  {
    case LEDS_OFF:
      colorWipe(pixels.Color(0, 0, 0), 0); // black, no delay
      break;
    case LEDS_SOLID_WHITE:  
      colorWipe(pixels.Color(255, 255, 255)); // WHITE, no delay
      break;
    case LEDS_SOLID_RED:  
      colorWipe(pixels.Color(255, 0, 0)); // RED, no delay
      break;
      case LEDS_SOLID_GREEN:  
      colorWipe(pixels.Color(0, 255, 0)); // GREEN, no delay
      break;
    case LEDS_SOLID_BLUE:  
      colorWipe(pixels.Color(0, 0, 255)); // BLUE, no delay
      break;
    case LEDS_SOLID_COLOR_CYCLE:
      base_wheel += 1;
      base_color = wheel(base_wheel);
      colorWipe(base_color, 1); // current color, no delay
      break;
    case LEDS_CHASE_WHITE:
      theaterChase(pixels.Color(255, 255, 255), delay_ms);
      break;
    case LEDS_CHASE_COLOR_CYCLE:
      theaterChaseRainbow(10);
      break;
    case LEDS_RAINBOW:
      rainbow(delay_ms);
      break;
    case LEDS_RAINBOW_CYCLE:
      rainbowCycle(delay_ms);
      break;
      
    default: 
      colorWipe(pixels.Color(0, 0, 0), 0); // black, no delay
      break;
  }

  delay(50);
  digitalWrite(LED1_PIN, LOW);
}


/*
 * Input a value 0 to 255 to get a color value.
 * The colors are a transition R->G->B and back to R. 
 */
uint32_t wheel(byte wheelPos) 
{
  if(wheelPos < 85) 
  {
    return pixels.Color(wheelPos * 3, 255 - wheelPos * 3, 0);
  } 
  else 
  {
    if(wheelPos < 170) 
    {
     wheelPos -= 85;
     return pixels.Color(255 - wheelPos * 3, 0, wheelPos * 3);
    } 
    else 
    {
     wheelPos -= 170;
     return pixels.Color(0, wheelPos * 3, 255 - wheelPos * 3);
    }
  }
}


/* ----------------------------------------------------------------------------
 * LED PATTERN METHODS
 * Make sure that any methods here do NOT iterate for long periods of time
 * or else the buttons will not be responsive to user input.
 * Instead, use static variables to control the state of long-changing patterns.
 * -----------------------------------------------------------------------------
 */

/*
 * Fill the dots one after the other with a color 
 */
 void colorWipe(uint32_t c) 
{
  for(uint16_t i=0; i < pixels.numPixels(); i++) 
  {
      pixels.setPixelColor(i, c);
  }
  pixels.show();
}

/*
 * Fill the dots one after the other with a color using the specified delay
 */
void colorWipe(uint32_t c, uint8_t wait) 
{
  for(uint16_t i=0; i < pixels.numPixels(); i++) 
  {
      pixels.setPixelColor(i, c);
      pixels.show();
      delay(wait);
  }
}

/*
 * Shifting rainbow effect
 */
void rainbow(uint8_t wait) 
{
  static uint16_t j;
  
  for(uint16_t i = 0; i < pixels.numPixels(); i++) 
  {
    pixels.setPixelColor(i, wheel((i+j) & 255));
  }
  pixels.show();
  delay(wait);

  j += 1;
  if (j >= 256) j = 0;
}

/*
 * Equally-distributed rainbow color shift effect 
 */
void rainbowCycle(uint8_t wait) 
{
  static uint16_t j = 0;

  for(uint16_t i = 0; i< pixels.numPixels(); i++) 
  {
    pixels.setPixelColor(i, wheel(((i * 256 / pixels.numPixels()) + j) & 255));
  }
  pixels.show();
  delay(wait);

  j += 1;
  if (j >= 256 * 5) j = 0;
}

/*
 * Theatre-style crawling lights.
 */
void theaterChase(uint32_t c, uint8_t wait) 
{
  for (int q = 0; q < 3; q++) {
    for (uint16_t i = 0; i < pixels.numPixels(); i += 3) {
      pixels.setPixelColor(i + q, c);    //turn every third pixel on
    }
    pixels.show();

    delay(wait);

    for (uint16_t i=0; i < pixels.numPixels(); i += 3) {
      pixels.setPixelColor(i + q, 0);        //turn every third pixel off
    }
  }
}

/* 
 * Theatre-style crawling lights with rainbow effect 
 */
void theaterChaseRainbow(uint8_t wait) 
{
  static int j = 0;
  
  for (int q=0; q < 3; q++) 
  {
    for (uint16_t i=0; i < pixels.numPixels(); i=i+3) 
    {
      pixels.setPixelColor(i+q, wheel( (i+j) % 255));    //turn every third pixel on
    }
    pixels.show();

    delay(wait);

    for (uint16_t i=0; i < pixels.numPixels(); i=i+3) 
    {
      pixels.setPixelColor(i+q, 0);        //turn every third pixel off
    }
  }

  j += 1;
  if (j >= 256) j = 0;
}
