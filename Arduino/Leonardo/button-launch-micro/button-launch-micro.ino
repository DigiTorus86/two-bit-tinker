/* Simple keyboard macro button launcher sketch
 * 
 * Required hardware:
 * - Arduino Lenardo or Pro Micro
 * - Normally-open momentary buttons or keys with keycaps
 * 
 * See additional project description at:
 *    https://twobittinker.com/diy-macro-keypad/
 *
 * Copyright (c) 2020 Paul Pagel
 * This is free software; see the license.txt file for more information.
 * There is no warranty; not even for merchantability or fitness for a particular purpose.
 */

#include <Keyboard.h>

#define BUTTON_COUNT 8
#define LED_PIN      10


// Define the pins on the board and the ctrl-alt-character sequence 
// that the button will send
int  button_pins[BUTTON_COUNT] = {14,   4,   3,   2,  16,   7,   6,   5  };
char button_char[BUTTON_COUNT] = {'2', '3', '4', '5', '6', '7', '8', '9' };

void setup() 
{
  Keyboard.begin();
  Serial.begin(9600);
  Serial.println("Leonardo Micro Button Launch");

  for (int i = 0; i < BUTTON_COUNT; i++)
  {
    pinMode(button_pins[i], INPUT_PULLUP);
  }

  pinMode(LED_PIN, OUTPUT);
}


void loop() 
{ 
  CheckAllButtons();
}


void CheckAllButtons(void) {

  for (int i = 0; i < BUTTON_COUNT; i++)
  {
    if (digitalRead(button_pins[i]) == LOW)
    {
      Serial.println(i);
      Keyboard.press(KEY_LEFT_CTRL);
      Keyboard.press(KEY_LEFT_ALT);
      Keyboard.press(button_char[i]);
      digitalWrite(LED_PIN, HIGH);  
      delay(150);
      digitalWrite(LED_PIN, LOW);  
      Keyboard.releaseAll();
    }
  }  
}
