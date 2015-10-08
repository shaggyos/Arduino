/*
 * MultiRX sketch
 * Receive data from two software serial ports
 */

#include <NewSoftSerial.h>
const int rxpin1 = 2; // white <- pin 12 R1out <- pin 13 R1in  <- Blue   <- pin 3 (TX)
const int txpin1 = 3; // red   -> pin 11 T1in  -> pin 14 T1out -> yellow -> pin 2 (RX)

NewSoftSerial gps(rxpin1, txpin1 );  // gps device connected to pins 2 and 3

void setup()
{
  gps.begin(9600);
  Serial.begin(9600);
}

void loop()
{
  gps.print("A,");
  Serial.println("B");
  delay(500);
}



