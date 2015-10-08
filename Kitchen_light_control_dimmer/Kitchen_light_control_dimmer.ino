#include <EEPROM.h>

// Max brightness at 255, minimum brightness at 0
int PwmOut = 9;          // Output pin to drive the mosfet, reduced to 2.5v max (1K from PWM to Gate, 10K between Gate & Src)
//int RemoteIn = 10;           // Remote control.
//int OverrideBtnSw = 12;  // use 10K resistor 
//int LedPin = 13;         // operations status ping
int LuxPin = A0;         // Light sensor to determine if kitchen lights are on, or daylight
//int PotSense = A1;       // A1 to center pin, 5v and gnd to outside pins (0-1023)
//int memLoc = 0;          // EEPROM Memory location we are using to hold our default light level

/*
  PWM)----1K---*-----(Gate)
               |
              10K
               |
               *-----{Source)
*/

//int fadestep = 5;
//int brightness=255;
//int defaultBright = 255; 
// long prevMillis = 0;
long interval = 200;
int LuxLevel =0;
//int OverrideBtn = 0;
//float LastPotValue = 0;
int lux = 0;
//int PotVal = 0;
int minLux = 4; //1.71v avg pwm (255 = 14.01v).  Switch off below "5" or 2% ~ 6v
//int lastBright = 0;
const int numReadings = 3;
int readings[numReadings];
int index = 0;
int total = 0;
int avg = 0;

boolean Debug = true;

void setup()
{
  Serial.begin(9600);
  Serial.println("Startup ...");
  pinMode(PwmOut,OUTPUT);  
  //pinMode(RemoteIn, INPUT);
  // pinMode(OverrideBtnSw,INPUT_PULLUP);  // LOW = on, HIGH = off
  //pinMode(LedPin,OUTPUT);        // LOW = off, HIGH = on
  // defaultBright = EEPROM.read(memLoc);
  // brightness = defaultBright;
  
  for (int thisReading = 0; thisReading < numReadings; thisReading++)
     readings[thisReading] = 0;
}

void loop()
{
  //Check status without using delays.  Allows for multiple tasks to occur.
//  unsigned long currMillis = millis();
//  if(currMillis - prevMillis > interval) {
//    prevMillis = currMillis;  

  // Check override button state.
 // OverrideBtn=digitalRead(OverrideBtnSw);   
  // Enable lights if over-ride button is on.
//  if (OverrideBtn == LOW) {
//    brightness = defaultBright;
//    digitalWrite(LedPin, HIGH);  // Set status ping
//    float ThisPotVal = analogRead(PotSense);
//    if (LastPotValue != ThisPotVal) {
//      LastPotValue = ThisPotVal;
//      PotVal = ThisPotVal/4.0;  // converts from range 1023 to 255
//      brightness = (int) PotVal;   // convert from between 0-1023 to 0 and 255
//      if (OverrideBtn == LOW) {
//        defaultBright = brightness;  // this is how we tune the default brightness level
//        EEPROM.write(memLoc,defaultBright);
//      }
//    }
//  } 
//  else {
//    digitalWrite(LedPin, LOW);  // Set status pin
    // bring lux level from 0 to 255 up to 1 to 1024 to reduce noise at the lower data values
    
    // subtract the last reading:
   total= total - readings[index];         
  // read from the sensor:  
   readings[index] = analogRead(LuxPin);
  // add the reading to the total:
   total= total + readings[index];       
  // advance to the next position in the array:  
   index = index + 1;                    
 
  // if we're at the end of the array...
   if (index >= numReadings)              
     // ...wrap around to the beginning: 
    index = 0;                           

  // calculate the average:
   LuxLevel = total / numReadings;         
  // send it to the computer as ASCII digits

    //LuxLevel = analogRead(LuxPin);  // collector light current: 20 lux = 10uA, 100 lux = 50uA 200 lux = 100uA, 400 uA = 800 lux, 500uA = 1000 lux
    // Turn off cupboard lights if kitchen lights are off
    if (LuxLevel < minLux) {  // Lights aren't on, and no override
      //brightness = 0;
      digitalWrite(PwmOut,LOW);  // turn off lights
    } 
    else {
      //brightness = defaultBright;
      digitalWrite(PwmOut,HIGH);
    }
//}

  // } 

 // analogWrite(PwmOut, brightness);

  if (Debug == true) {
    //Serial.print("Brightness: ");
    //Serial.print(brightness);
    //Serial.print(" | Button: ");
    //Serial.print(OverrideBtn);
    Serial.print(" | Lux: ");
    Serial.print(LuxLevel);
    //Serial.print(" | Pot: ");
    //Serial.print(PotVal);
    Serial.println("");
  }
  delay(interval);
}







