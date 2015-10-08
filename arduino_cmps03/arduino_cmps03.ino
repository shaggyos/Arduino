/*
CMPS03 with arduino I2C example

This will display a value of 0 - 359 for a full rotation of the compass.

The SDA line is on analog pin 4 of the arduino and is connected to pin 3 of the CMPS03.
The SCL line is on analog pin 5 of the arduino and is conected to pin 2 of the CMPS03.
Both SDA and SCL are also connected to the +5v via a couple of 1k8 resistors.
A switch to callibrate the CMPS03 can be connected between pin 6 of the CMPS03 and the ground.

The circuit (RS232): 
 * RX is digital pin 10 (connect to TX of other device)
 * TX is digital pin 11 (connect to RX of other device)

 Serial @ 115200 bps, 8N1
 Use level convert to connect directly to PC

 J2 pin 4 = TX to RX D10 (closest to P4)
 J2 Pin 3 = GND to GND
 J2 Pin 2 = RX to TX D11
 J2 Pin 1 = +5V to +5V

 I2C interface: address 0xa0
 read by EXT_EEPROM command
 i2c buffer[7]=t_hi (tmp high byte - unsighed int8
 i2c buffer[6]=t_lo (tmp low byte - unsigned int8)
 i2c buffer[5]=adxl_y (roll - signed int8)
 i2c buffer[4]=adxl_x (tilt -signed int8)
 i2c buffer[3]=cmp_UMSB
 i2c buffer[2]=cmp_LMSB
 i2c buffer[1]=cmp_ULSB (compass 0..359)
 i2c buffer[0]=cmp_LLSB (compass 0..359)

 int16 compass degrees
 int 8 cmp_LLSB,cmp_ULSB
 signed int8 adxl_x,tilt_degrees
 signed int8 adxl_y,roll_degrees
 compass_degrees=((int16)cmp_ULSB<<8)+cmp_LLSB;

 J3 Pin 1 = +5V (closest to USB socket)
 J3 Pin 2 = I2C SCL/SPEED1 (12.5/sec)  --> Arduino A5
 J3 Pin 3 = I2C SDA  --> Arduion A4
 J3 Pin 4 = GND/Temp sensor -/Speed 2 (25/sec)  --> Arduino GND
 J3 Pin 5 = ADJUST
 J3 Pin 6 = Temp Sensor +

 J4 Pin 1 = Ext Pwr Src (jump 1/2 for ext Pwr)
 J4 Pin 2 = COMMON
 J4 Pin 3 = USB pwr src (jumper 2/3 for USB)

  Increase sample speed SPD1 to SPD2: Jumper JP3 pin 4 to pin 2
*/
#include <Wire.h>

#define ADDRESS 0 // 0x50 //defines address of compass

void setup(){
  Wire.begin(); //conects I2C as master
  Serial.begin(9600);
  Serial.println("Started");
  // ScanI2C();
}

void loop(){
  byte highByte;
  byte lowByte;
  byte myResult;
  
   Serial.print("Request result:");
   Wire.beginTransmission(ADDRESS);      //starts communication with cmps03
   Serial.print(".");
   Wire.write(2);      //Sends the register we wish to read
   Serial.print("."); 
   myResult=Wire.endTransmission();
   Serial.print("."); 
   Serial.println(myResult);

   Serial.print("Bearing: ");   
   Wire.requestFrom(ADDRESS, 2);        //requests high byte
   while(Wire.available() < 2);         //while there is a byte to receive
   highByte = Wire.read();           //reads the byte as an integer
   lowByte = Wire.read();
   int bearing = ((highByte<<8)+lowByte)/10; 
   Serial.println(bearing);
   delay(100);
}

void ScanI2C()
{
  byte error, address;
  int nDevices;

  Serial.println("Scanning...");

  nDevices = 0;
  for(address = 1; address < 127; address++ ) 
  {
    // The i2c_scanner uses the return value of
    // the Write.endTransmisstion to see if
    // a device did acknowledge to the address.
    Wire.beginTransmission(address);
    error = Wire.endTransmission();

    if (error == 0)
    {
      Serial.print("I2C device found at address 0x");
      if (address<16) 
        Serial.print("0");
      Serial.print(address,HEX);
      Serial.println("  !");

      nDevices++;
    }
    else if (error==4) 
    {
      Serial.print("Unknow error at address 0x");
      if (address<16) 
        Serial.print("0");
      Serial.println(address,HEX);
    }    
  }
  if (nDevices == 0)
    Serial.println("No I2C devices found\n");
  else
    Serial.println("done\n");

  // delay(5000);           // wait 5 seconds for next scan
}
