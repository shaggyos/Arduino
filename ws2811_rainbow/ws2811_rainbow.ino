#define FORCE_SOFTWARE_SPI
#define FORCE_SOFTWARE_PINS
#include "FastSPI_LED2.h"

#define NUM_LEDS 50  // Anzahl der LEDs im Strip
#define DATA_PIN 6

struct CRGB leds[NUM_LEDS];
CRGB rgb;

void setup() {
  FastLED.addLeds<WS2811, DATA_PIN, RGB>(leds, NUM_LEDS);
}

void loop() { 
  randomSeed(millis());
  int wait= 0; // random(1,10);
  int dim=random(4,6);
  int max_cycles=8;
  int cycles=random(1,max_cycles+1);
  rainbowCycle(wait,cycles,dim);
 
}

void rainbowCycle(uint8_t wait,byte cycle,byte dim) {
  int cycles, j, k;
  for(cycles=0;cycles<cycle;cycles++){
    byte dir=random(0,2);
    k=255;
    for (j=0; j < 256; j++,k--) {     // cycles of all 25 colors in the wheel
      if(k<0)k=255;
      for(int i=0; i<NUM_LEDS; i+=1) {
        Wheel(((i * 256 / NUM_LEDS) + (dir==0?j:k)) % 256,dim);        
        leds[i]=rgb;
      }
      FastLED.show( NUM_LEDS);
      ;  // (byte*)leds,
      delay(wait);
    }
  }
}

void Wheel(byte WheelPos,byte dim){
  if (WheelPos < 85) {
    rgb.r=0;
    rgb.g=WheelPos * 3/dim;
    rgb.b=(255 - WheelPos * 3)/dim;
    ;
    return;
  } 
  else if (WheelPos < 170) {
    WheelPos -= 85;
    rgb.r=WheelPos * 3/dim;
    rgb.g=(255 - WheelPos * 3)/dim;
    rgb.b=0;
    return;
  }
  else {
    WheelPos -= 170; 
    rgb.r=(255 - WheelPos * 3)/dim;
    rgb.g=0;
    rgb.b=WheelPos * 3/dim;
    return;
  }
}

