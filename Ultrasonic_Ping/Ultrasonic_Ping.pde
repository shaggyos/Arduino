/* Ping))) Sensor
  
   This sketch reads a PING))) ultrasonic rangefinder and returns the
   distance to the closest object in range. To do this, it sends a pulse
   to the sensor to initiate a reading, then listens for a pulse 
   to return.  The length of the returning pulse is proportional to 
   the distance of the object from the sensor.
     
   The circuit:
  * +V connection of the PING))) attached to +5V
  * GND connection of the PING))) attached to ground
  * SIG connection of the PING))) attached to digital pin 7

   http://www.arduino.cc/en/Tutorial/Ping
   
 */

//  Lets load the library and pin details for the LCD.
#include <LiquidCrystal.h>
LiquidCrystal lcd(8,9,4,5,6,7);

const int pingPin = 10;      // PING Sensor output pin
const int pingDelay = 500;   // How often the distance should be measured.

long lngTimeLast;            // Last time the subprocesses were run
long lngTimeNow;             // Updated each time the loop restarts.

int intDist = 0;             // Holds the meastured distance.

void setup() 
{
  
  Serial.begin(9600);        // initialize serial communication:
  lcd.begin(16, 2);          // Initialise the LCD as a 16x2 array
  lngTimeLast = millis();    // Set an initial value for the last run time.
}


void loop()
{
  lngTimeNow = millis();     // Ok - lets see what time it is now
  if ((lngTimeNow - lngTimeLast) > pingDelay)   // Have we passed the minimum wait time?
  {
    lngTimeLast = lngTimeNow; // Lets update the last runtime to now.
    intDist = subPing();      // Measure the distance
    lcdPrint();               // Print the distance on the LCD screen. 
  }
}

int subPing()
{
  // establish variables for duration of the ping, 
  // and the distance result in inches and centimeters:
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

  // convert the time into a distance
  cm = microsecondsToCentimeters(duration);
  
  Serial.print(cm);
  Serial.println(" cm");

  // delay(pingDelay);
  return cm;
}

void lcdPrint()
{
  lcd.clear();
  lcd.setCursor(0,0);
  lcd.print("Distance: ");
  lcd.setCursor(10,0);
  lcd.print(intDist);
  lcd.setCursor(14,0);
  lcd.print("cm");
}
    

long microsecondsToCentimeters(long microseconds)
{
  // The speed of sound is 340 m/s or 29 microseconds per centimeter.
  // The ping travels out and back, so to find the distance of the
  // object we take half of the distance travelled.
  return microseconds / 29 / 2;
}


