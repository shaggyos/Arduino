/*
 LDR/Temperature sketch
 blink an LED at a rate set by the Resistance of a Light Dependent Resistor (LDR)
 Output both analogue port readings to the serial monitor.
*/

const int pinTemp = 0;    // Pin for the LM35CZ temp
const int pinLdr = 1;    // select the input pin for the potentiometer
const int pinLed = 13;   // select the pin for the LED

int valTemp = 0;             // variable to store the value coming from the sensor
int valLdr = 0;

void setup()
{
  pinMode(pinLed, OUTPUT);  // declare the ledPin as an OUTPUT
  Serial.begin(9600);
}

void loop() {
  int valTemp = analogRead(pinTemp);
  long celsius =  (valTemp * 500L) /1024;     // 10 mV per degree c, see text
  Serial.print(celsius);
  Serial.print(" degrees Celsius: ");

  valLdr = analogRead(pinLdr);   // read the voltage on the pot
  digitalWrite(pinLed, HIGH); // turn the ledPin on
  delay(valLdr);                 // blink rate set by pot value (in milliseconds)
  digitalWrite(pinLed, LOW);  // turn the ledPin off
  delay(valLdr);                 // turn led off for same period as it was 
  Serial.print(valLdr);                            // turned on
  Serial.println(" ms Delay");
}     

