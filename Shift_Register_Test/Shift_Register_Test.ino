// controll a Freetronics Shift register
// Control 6 LED's via 2 shift registers.
const int btnPin = 6; // Green wire - button connected between 5v and gnd, via LED.
const int pingPin = 7; // white wire - ultrasonic ping sensor
const int dataPin = 8; // Yellow wire - "in" (Serial In)
const int latchPin = 11; // Green wire - LCLLK (Latch)
const int clockPin = 12; //  Blue wire - CLK (Clock)

const int MaxPing = 480;
const int totLED = 16;
const int pingDelay = 1000;
const int ledDelay = 100;

long lngTimeLastPing;  // last time the subprocesses were run
long lngTimeLastLED;  // last time the subprocesses were run
long lngTimeNow;   // update each time the loop restarts;
long lngDist = 0;  // holds the measured distance
long lngTimePing = 0;
long lngTimeLED = 0;
long ledVal = 1;

int currentValue = 0;
int lastBtnState = LOW;
int program = 1;
int ledDir = 0;

//OUT - NC (Serial Out) - Outbound link to next shift in
//RST - 5V (Reset)
//OE - GND (Output enable)
//GND - GND
//VCC - 5V

void setup() {
  pinMode(dataPin, OUTPUT);
  pinMode(latchPin, OUTPUT);
  pinMode(clockPin, OUTPUT);
  pinMode(btnPin, INPUT);
  pinMode(pingPin, INPUT);
  lngTimeLastPing = millis();    // Set an initial value for the last run time.
  lngTimeLastLED = lngTimeLastPing;
  Serial.begin(9600);
}

void loop() 
{
  int btnState = digitalRead(btnPin);
  if (btnState != lastBtnState) {
    lastBtnState = btnState;
    if (btnState == HIGH) { 
      if (program < 4 ) { 
        program++; 
      }
      else {
        program = 1;
        currentValue=0;
        ledVal=1;
      }
      Serial.print ("Pushed - Prog ");
      Serial.println (program); 
    }
  }

  lngTimeNow = millis();     // Ok - lets see what time it is now
  lngTimePing = lngTimeNow - lngTimeLastPing;
  lngTimeLED = lngTimeNow - lngTimeLastLED;
/*
  if (lngTimePing > pingDelay)   // Have we passed the minimum wait time?
  {
    lngTimeLastPing = lngTimeNow; // Lets update the last runtime to now.
    lngDist = subPing();      // Measure the distance
  }    */

  // for (int currentValue = 0; currentValue < 16; currentValue++) {
  if (lngTimeLED > ledDelay)
  {

    if (program == 1) {
      switch (currentValue) {
      case 0:
        ledVal<<=1; // forward -->
        ledDir = 0;  // forward --> 
        currentValue=1;
        break;
      case 15:
        ledVal>>=1; // reverse <---
        ledDir = 1;  // reverse <---
        currentValue=14;  // we want the previous led.
        break;
      default:
        if (ledDir == 0) {  // forward -->
          ledVal <<=1;    // forward -->
          currentValue++;
        }
        else {
          ledVal >>=1;     // reverse <--
          currentValue--;
        }
      }
    }  

    switch (program) {
    case 1:
      {
      }
      // do nothing - covered elsewhere
      // this prevents default kicking in when it shouldn't.
      break;
    case 2:
      ledVal <<=1;            // forward --->
      currentValue++;         // forward --->
      ledDir = 0;             // forward --->
      if (currentValue > 15 ) // at end
      { 
        currentValue = 0;   // go back to start
        ledVal = 1;
      }
      break;
    case 3:
      ledDir = 1;
      if (currentValue == 0 )  // if at start
      { 
        currentValue = 16;  // goto to end
        ledVal = 65536;     
      } 
      else {
        ledVal >>=1;
        currentValue--;
      }
      break;
    case 4:   // approx 40cm per LED
      lngDist = subPing();      // Measure the distance
      long distperc = 100*lngDist/MaxPing;  // should give a percent distance value
      int ledNum = totLED * distperc/100;         // gives the led number we need to set
      ledVal = 1;                             // lets make sure we have a clean slate
      ledVal <<=ledNum;                       // bit shift to set the shift to that LED 
    }

    unsigned int ledVal_high = highByte(ledVal);
    unsigned int ledVal_low = lowByte(ledVal);

    // Disable the latch while we clock in data
    digitalWrite(latchPin, LOW);

    // Send the value as a binary sequence to the module
    shiftOut(dataPin, clockPin, MSBFIRST, ledVal_high );
    shiftOut(dataPin, clockPin, MSBFIRST, ledVal_low );

    // Enable the latch again to set the output states
    digitalWrite(latchPin, HIGH);


    lngTimeLastLED = lngTimeNow;
    // delay(200);
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

long microsecondsToCentimeters(long microseconds)
{
  // The speed of sound is 340 m/s or 29 microseconds per centimeter.
  // The ping travels out and back, so to find the distance of the
  // object we take half of the distance travelled.
  return microseconds / 29 / 2;
}



