// set serial speed in window to 19200 if problems uploading
#define FORCE_SOFTWARE_SPI
#define FORCE_SOFTWARE_PINS
#include "FastSPI_LED2.h"

#define NUM_LEDS 50  // Using WS2811 strip, 3 LED's per address
#define DATA_PIN 6   // red wire LED strip.

struct CRGB leds[NUM_LEDS];
struct CRGB pingLED[NUM_LEDS];
CRGB rgb;
CRGB rgbPin;

const int pingPin = 8; // white wire - ultrasonic ping sensor
const int MaxPingDist = 400;
const int pingDelay = 100;

int lastLedNum = NUM_LEDS+10;

void setup() {
  pinMode(pingPin, INPUT);
  FastLED.addLeds<WS2811, DATA_PIN, RGB>(leds, NUM_LEDS);
}

void loop() { 
  randomSeed(millis());
  int wait= 0; // random(1,10);
  int dim=random(4,6);
  int max_cycles=8;
  int cycle=random(1,max_cycles+1);
  int cycles, j, k;

  for(cycles=0;cycles<cycle;cycles++){
    long lngDist = subPing();
    while (lngDist < MaxPingDist) {
      long distperc = 100*lngDist/MaxPingDist;    // should give a percent distance value
      int ledNum = NUM_LEDS * distperc/100;   // gives the led number we need to set
      if ( ledNum != lastLedNum ) {
        FastLED.clear();
        memset (leds,0,NUM_LEDS);
        leds[ledNum] = CRGB::White;
        lastLedNum = ledNum;
        FastLED.show( NUM_LEDS);
      }
      lngDist = subPing();
      delay(pingDelay);
    } 
    byte dir=random(0,2);
    k=255;
    for (j=0; j < 256; j++,k--) {     // cycles of all 25 colors in the wheel
      if(k<0)k=255;
      for(int i=0; i<NUM_LEDS; i+=1) {
        Wheel(((i * 256 / NUM_LEDS) + (dir==0?j:k)) % 256,dim);        
        leds[i]=rgb;
      }
      FastLED.show( NUM_LEDS);
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

int subPing()
{
  long duration, cm;
  // The PING))) is triggered by a HIGH pulse of 2 or more microseconds.
  // Give a short LOW pulse beforehand to ensure a clean HIGH pulse:
  pinMode(pingPin, OUTPUT);
  digitalWrite(pingPin, LOW);
  delayMicroseconds(2);
  digitalWrite(pingPin, HIGH);
  delayMicroseconds(15);
  digitalWrite(pingPin, LOW);
  delayMicroseconds(20);
  // The same pin is used to read the signal from the PING))): a HIGH
  // pulse whose duration is the time (in microseconds) from the sending
  // of the ping to the reception of its echo off of an object.
  pinMode(pingPin, INPUT);
  duration = pulseIn(pingPin, HIGH);

  // convert the time into distance using speed sound dist per uSec and 1/2 round trip.
  cm = duration / 29 /2; //microsecondsToCentimeters(duration);
  return cm;
}

