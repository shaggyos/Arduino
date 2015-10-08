// GND -> GND (black wire)
// VCC -> +5V (yellow wire)
// SDA -> A4 (orange wire)
// SLC -> A5 (blue wire)

// #include <Wire.h> 
#include <SPI.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

// If using software SPI (the default case):
#define OLED_MOSI   9 // yellow
#define OLED_CLK   10 // white
#define OLED_DC    11 // orange
#define OLED_CS    12 // white
#define OLED_RESET 13 // blue
Adafruit_SSD1306 display(OLED_MOSI, OLED_CLK, OLED_DC, OLED_RESET, OLED_CS);

#define hmc_SDA A4 // orangle wire - 2nd last pin
#define hmc_SLC A5 // blue wire - last pin

/* Uncomment this block to use hardware SPI
#define OLED_DC     6
#define OLED_CS     7
#define OLED_RESET  8
Adafruit_SSD1306 display(OLED_DC, OLED_RESET, OLED_CS);
*/

#if (SSD1306_LCDHEIGHT != 32)
#error("Height incorrect, please fix Adafruit_SSD1306.h!");
#endif

int WLED = 13;
int compassAddress = 0x42 >> 1; // From datasheet compass address is 0x42
// shift the address 1 bit right, the Wire library only needs the 7
// most significant bits for the address

const float pi = 3.14;
int reading = 0; 


void setup() 
{ 
  Wire.begin();       // join i2c bus (address optional for master) 
  Serial.begin(9600); // start serial communication at 9600bps 
  pinMode(WLED, OUTPUT);
  digitalWrite(WLED, LOW);

  // by default, we'll generate the high voltage from the 3.3v line internally! (neat!)
  display.begin(SSD1306_SWITCHCAPVCC);
  // init done
  // Show image buffer on the display hardware.
  // Since the buffer is intialized with an Adafruit splashscreen
  // internally, this will display the splashscreen.
  display.display();
  delay(1000);
  display.clearDisplay();
} 

void loop() 
{ 
  // step 1: instruct sensor to read echoes 
  Wire.beginTransmission(compassAddress);  // transmit to device
  // the address specified in the datasheet is 66 (0x42) 
  // but i2c adressing uses the high 7 bits so it's 33 
  Wire.write('A');        // command sensor to measure angle  
  Wire.endTransmission(); // stop transmitting 
  
  // step 2: wait for readings to happen 
  delay(10); // datasheet suggests at least 6000 microseconds 
  
  // step 3: request reading from sensor 
  Wire.requestFrom(compassAddress, 2); // request 2 bytes from slave device #33 
  
  // step 4: receive reading from sensor 
  if (2 <= Wire.available()) // if two bytes were received 
  { 
    digitalWrite(WLED, HIGH);
    display.clearDisplay();
    reading = Wire.read();  // receive high byte (overwrites previous reading) 
    reading = reading << 8; // shift high byte to be high 8 bits 
    reading += Wire.read(); // receive low byte as lower 8 bits 
    reading /= 10;
    Serial.println(reading); // print the reading
    display.setCursor(0,0);
    display.setTextSize(1);
    display.setTextColor(WHITE);
    display.print("Mag Bearing: ");
    display.println(reading);
    // display is 128 x 32.  circle is diameter 15. angle to x/y
    display.drawPixel((display.width()-15),(display.height()/2),WHITE);
    display.drawCircle((display.width()-15),(32/2),(display.height()/2)-2, WHITE);
    display.drawLine((display.width()-15),(display.height()/2),113+(10*sin(reading/57.296)),16-(10*cos(reading/57.296)),WHITE);
    display.display();
    // 17 chrs to directionator
    
  } 
  delay(500); // wait for half a second
}

