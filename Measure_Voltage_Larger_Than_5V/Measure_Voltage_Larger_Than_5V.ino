/*
DisplayMoreThan5V sketch
prints the voltage on analog pin to the serial port
Do not connect more than 5 volts directly to an Arduino pin.

     Voltage to
     be measured (+)V < 20V 
                  |
                  Z R1 (3K ohm)
		  | 
  Analog In 0+----+
                  |
                  Z R2 (1k ohm)
                  |
       gnd +------+

*/

const int referenceVolts = 5; // the default reference on a 5-volt board
//const float referenceVolts = 3.3; // use this for a 3.3-volt board

const int R1 = 3000; // value for a maximum voltage of 20 volts
const int R2 = 1000;

// determine by voltage divider resistors, see text
const int resistorFactor = 255 / (R2/(R1 + R2)); 
const int batteryPin = 0; // +V from battery is connected to analog pin 0

void setup()
{
Serial.begin(9600);
}

void loop()
{
  int val = analogRead(batteryPin); 			  // read the value from the sensor
  float volts = (val / resistorFactor) * referenceVolts ; // calculate the ratio
  Serial.println(volts); 				  // print the value in volts
}

