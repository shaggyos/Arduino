/*
 Measure current directly using an ACS756SCA-50Amp unit.
 Circuit:
 --------
 IP+=|
 In  |             R1 10K
     |--3 ViOut  --/\/\/\---*----> pin A1
     |                      |
     o                     === C1 100nF
     o                      |
     o--2 Gnd --------------*----> GND
     o                      |
     o                     === C2 100nF
     |                      |
     |--1 Vcc---------------*----> 5V
 Out |
 IP-=|
*/

// include the library code:

#include <LiquidCrystal.h>
// initialize the library with the numbers of the interface pins
// Freetronics LCD: RS,E, bit4, bit5,bit6,bit7, LCD Backlight=D3
LiquidCrystal lcd(8,9,4,5,6,7);

// Note: A0 used for buttons on Freetronics LCD
//10bit: Nil=100+,  Rt = 0, Up = 145, Dn=329, Lt=505, Select = 741

const int numRows = 2;
const int numCols = 16;

int sensorPin = A1;     // select the input pin for the potentiometer
int sensorValue = 0;  // variable to store the value coming from the sensor
float slope = 0.09775; //171; //-50 to +50 amps (100amp range)/Max number of points by ADC

//offsets to cope with variations between chips, temp, setup, construction etc.
int senOffset = 3;  // 1 sensoffset ~ 0.07 ampoffset  // align so we get 512 at 0 volts.
float ampOffset = -0.05;     // arduino board // offset to align board with zero point

void setup() {
  Serial.begin(9600);
  lcd.begin(numCols, numRows);
  lcd.noBlink();
  pinMode(sensorPin,INPUT);
  delay(100);  // delay between setting pin modes and reading the pin;
}

void loop() {
  // read the value from the sensor:
  sensorValue = analogRead(sensorPin)+senOffset;   // get the current reading from the sensor (0-1024)
  float amps = ((slope*sensorValue)-50.0)+ampOffset; // result in amps
 
   // Output results
  lcd.setCursor(0,0);
  String rptSense="Sense: " + String(sensorValue) + " ";
  lcd.print(rptSense);
  lcd.setCursor(0,1);
  String rptAmps="Amps: " + String(amps)+" A ";
  lcd.print(rptAmps);
  Serial.print(rptSense);
  Serial.println("; " + rptAmps);
  delay(1000);  // delay between readings
}
