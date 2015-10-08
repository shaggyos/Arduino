/*
  Software serial multple serial test
 
 Receives from the hardware serial, sends to software serial.
 Receives from software serial, sends to hardware serial.
 
 The circuit: 
 * RX is digital pin 10 (connect to TX of other device)
 * TX is digital pin 11 (connect to RX of other device)
 */
 
 // Serial @ 115200 bps, 8N1
// Use level convert to connect directly to PC
//
// J2 pin 4 = TX to RX D10 (closest to P4)
// J2 Pin 3 = GND to GND
// J2 Pin 2 = RX to TX D11
// J2 Pin 1 = +5V to +5V

// I2C interface: address 0xa0
// read by EXT_EEPROM command
// i2c buffer[7]=t_hi (tmp high byte - unsighed int8
// i2c buffer[6]=t_lo (tmp low byte - unsigned int8)
// i2c buffer[5]=adxl_y (roll - signed int8)
// i2c buffer[4]=adxl_x (tilt -signed int8)
// i2c buffer[3]=cmp_UMSB
// i2c buffer[2]=cmp_LMSB
// i2c buffer[1]=cmp_ULSB (compass 0..359)
// i2c buffer[0]=cmp_LLSB (compass 0..359)

// int16 compass degrees
// int 8 cmp_LLSB,cmp_ULSB
// signed int8 adxl_x,tilt_degrees
// signed int8 adxl_y,roll_degrees
// compass_degrees=((int16)cmp_ULSB<<8)+cmp_LLSB;

// J3 Pin 1 = +5V (closest to USB socket)
// J3 Pin 2 = I2C SCL/SPEED1 (12.5/sec)
// J3 Pin 3 = I2C SDA
// J3 Pin 4 = GND/Temp sensor -/Speed 2 (25/sec)
// J3 Pin 5 = ADJUST
// J3 Pin 6 = Temp Sensor +

// J4 Pin 1 = Ext Pwr Src (jump 1/2 for ext Pwr)
// J4 Pin 2 = COMMON
// J4 Pin 3 = USB pwr src (jumper 2/3 for USB)

// Increase sample speed SPD1 to SPD2: Jumper JP3 pin 4 to pin 2

#include <SoftwareSerial.h>

SoftwareSerial mySerial(10, 11); // RX, TX

void setup()  
{
  // Open serial communications and wait for port to open:
  Serial.begin(57600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }


  Serial.println("Goodnight moon!");

  // set the data rate for the SoftwareSerial port
  mySerial.begin(115200);
  // mySerial.println("Hello, world?");
}

void loop() // run over and over
{
  if (mySerial.available())
    Serial.write(mySerial.read());
    delay(100);
  // if (Serial.available())
  //  mySerial.write(Serial.read());
}

