#define FORCE_SOFTWARE_SPI
#define FORCE_SOFTWARE_PINS
#include "FastSPI_LED2.h"

///////////////////////////////////////////////////////////////////////////////////////////
//
// Move a white dot along the strip of leds.  This program simply shows how to configure the leds,
// and then how to turn a single pixel white and then off, moving down the line of pixels.
// 

// modify chipsets.h to :
// WS2811 - 400ns, 400ns, 450ns  // changed to  350, 800, 600 for aliexpress led strip
// template <uint8_t DATA_PIN, EOrder RGB_ORDER = RGB>
// class WS2811Controller800Khz : public ClocklessController<DATA_PIN, NS(350), NS(800), NS(600), RGB_ORDER> {};
// #if NO_TIME(350, 800, 600) 
// #warning "No enough clock cycles available for the WS2811 (800khz)"
// #endif

// How many leds are in the strip?
#define NUM_LEDS 300  //60 led/m & 5m = 300

// Data pin that led data will be written out over
#define DATA_PIN 6

// Clock pin only needed for SPI based chipsets when not using hardware SPI
// #define CLOCK_PIN 8

// This is an array of leds.  One item for each led in your strip.
CRGB leds[NUM_LEDS];

// This function sets up the ledsand tells the controller about them
void setup() {
	// sanity check delay - allows reprogramming if accidently blowing power w/leds
   	delay(0);

        LEDS.setBrightness(64);

      // Uncomment one of the following lines for your leds arrangement.
      // <DEVICE_TYPE, DATA_PIN, COLOR_TYPE> (<
      // FastLED.addLeds<TM1803, DATA_PIN, RGB>(leds, NUM_LEDS);
      // FastLED.addLeds<TM1804, DATA_PIN, RGB>(leds, NUM_LEDS);
      // FastLED.addLeds<TM1809, DATA_PIN, RGB>(leds, NUM_LEDS);
         
      FastLED.addLeds<WS2811, DATA_PIN, RGB>(leds, NUM_LEDS);	 // ink1003 seems to be using correct RGB code)
      // FastLED.addLeds<WS2811, 6, RGB>(leds, NUM_LEDS);      
      // FastSPI_LED2.addLeds<WS2811, DATA_PIN, GRB>(leds+18, NUM_LEDS/3);    // groups of 3 LED per address?
      // FastLED.addLeds<WS2811, 8, RGB>(leds + 225, NUM_LEDS/4);             // groups of 4 LED per address?
      // FastLED.addLeds<WS2811_400, DATA_PIN, RGB>(leds, NUM_LEDS);          // individual LED's - is the speed 400?

      // FastLED.addLeds<WS2812, DATA_PIN, RGB>(leds, NUM_LEDS);
      // FastLED.addLeds<WS2812B, DATA_PIN, RGB>(leds, NUM_LEDS);
      // FastLED.addLeds<NEOPIXEL, DATA_PIN, RGB>(leds, NUM_LEDS);

      // FastLED.addLeds<UCS1903, DATA_PIN, RGB>(leds, NUM_LEDS);

      // FastLED.addLeds<WS2801, RGB>(leds, NUM_LEDS);
      // FastLED.addLeds<SM16716, RGB>(leds, NUM_LEDS);
      // FastLED.addLeds<LPD8806, RGB>(leds, NUM_LEDS);

      // FastLED.addLeds<WS2801, DATA_PIN, CLOCK_PIN, RGB>(leds, NUM_LEDS);
      // FastLED.addLeds<SM16716, DATA_PIN, CLOCK_PIN, RGB>(leds, NUM_LEDS);
      // FastLED.addLeds<LPD8806, DATA_PIN, CLOCK_PIN, RGB>(leds, NUM_LEDS);
}

// This function runs over and over, and is where you do the magic to light
// your leds.
void loop() {
   // Move a single white led 
   for(int whiteLed = 0; whiteLed < NUM_LEDS; whiteLed = whiteLed + 1) {
      // Turn our current led on to white, then show the leds
      leds[whiteLed] = CRGB::Red;   // Green = Red, Red = Green, White=white, blue=blue

      // Show the leds (only one of which is set to white, from above)
      FastLED.show();

      // Wait a little bit
      // delay(5);

      // Turn our current led back to black for the next loop around
      leds[whiteLed] = CRGB::Black;
   }
}
