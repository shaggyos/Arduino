/*
Poll various sensors and report their values - initial input to various home system monitoring.
   D2: Digital Humidity/Temperature sensor #1
   D3: Digital Humidity/Temperature sensor #2
   A0: Light level sensor
   A1: Battery voltage
   A2: Solar voltage
   
   D8-12 is the OLED interface
   
prints the voltage on analog pin to the serial port
Do not connect more than 5 volts directly to an Arduino pin.

     Voltage to
     be measured (+)V < 20V 
                  |
                  Z R1 (3K9 ohm)
		  | 
  Analog In 0+----+
                  |
                  Z R2 (1k2 ohm)
                  |
       gnd +------+

  This equates to a maximum current draw of 3.6 mA (0.0036 A) at 21V
*/

#include "dht.h"
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define DHTPIN1 2     // what pin we're connected to
#define DHTPIN2 3

#define OLED_MOSI 9
#define OLED_CLK 10
#define OLED_DC 11
#define OLED_RESET 8
#define OLED_CS 12

// Uncomment whatever type you're using!
// #define DHTTYPE DHT22   // DHT 22  (AM2302)
//#define DHTTYPE DHT11   // DHT 11 
//#define DHTTYPE DHT21   // DHT 21 (AM2301)

dht dht1;
dht dht2;

Adafruit_SSD1306 display(OLED_MOSI, OLED_CLK, OLED_DC, OLED_RESET, OLED_CS);

unsigned long previousMillis = 0;
long t_interval =  5000; // 1 second interval

const int batteryPin = A1; // +V from battery is connected to analog pin 1
const int solarPin = A2; // +V from solar cell is connected to analog pin 2
const int R1 = 3850; // value for a maximum voltage of 20 volts
const int R2 = 1250;
float referenceVolts = 4.9; // the default reference on a 5-volt board
float resistorFactor;
float voltFactor;
//const float referenceVolts = 3.3; // use this for a 3.3-volt board

// determine by voltage divider resistors, see text

void setup() {
  Serial.begin(9600);
  
   // by default, we'll generate the high voltage from the 3.3v line internally! (neat!)
  display.begin(SSD1306_SWITCHCAPVCC);

  display.clearDisplay();  // clears the screen and buffer
  display.setTextSize(1);
  display.setTextColor(WHITE);
  display.setCursor(0,0);
  display.println("PV/Batt Monitor"); 
  display.println("Booting ...");
  display.display();
  
  Serial.println( readVcc()/1000, 2);
  referenceVolts = 4.9;
  resistorFactor = 1023*(R2/(R1 + R2));
  if (resistorFactor < 1) {
    resistorFactor = 250.9803922;
  }
  voltFactor = referenceVolts/resistorFactor;
  Serial.println("DHT test");
  Serial.print("R Factor: ");
  Serial.println(resistorFactor);
}

void loop() {
  unsigned long currentMillis = millis();
  if (currentMillis - previousMillis > t_interval) {
    previousMillis = currentMillis;
    pollSensors();
 }
}

void pollSensors() {
  // Reading temperature or humidity takes about 250 milliseconds!
  // Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)

  int chk1 = dht1.read22(DHTPIN1);
  int chk2 = dht2.read22(DHTPIN2);

  float h1 = dht1.humidity;
  float t1 = dht1.temperature;
  float h2 = dht2.humidity;
  float t2 = dht2.temperature;
  // check if returns are valid, if they are NaN (not a number) then something went wrong!
  if (isnan(t1) || isnan(h1)) {
    Serial.println("Failed to read from DHT");
  } else {
    Serial.print("Rel Humidity: ");
    Serial.print(h1,1);
    Serial.print("/");
    Serial.print(h2,1);
    Serial.print(" %\t");
    Serial.print("Temperature: ");
    Serial.print(t1,1);
    Serial.print("/");
    Serial.print(t2,1);
    Serial.print("*c ");
    Serial.print("DewPoint: ");
    Serial.print(dewPoint(t1, h1),1);
    Serial.print(" DewPoint Fast: ");
    Serial.print(dewPointFast(t1, h1),1);
  }
  int batval = analogRead(batteryPin); 			  // read the value from the sensor
  int solval = analogRead(solarPin);
  int l = analogRead(A0);    // collector light current: 20 lux = 10uA, 100 lux = 50uA 
                             // 200 lux = 100uA, 400 uA = 800 lux, 500uA = 1000 lux
  
  Serial.print("\t Light: ");
  Serial.print(l,DEC);
  Serial.print(" uA ");
  
  float batvolts = batval*voltFactor ; // calculate the ratio
  float solvolts = solval*voltFactor ; // calculate the ratio
  float batI = batvolts/5900;
  float solI = solvolts/5900;
  // Serial.println(batvolts);
  // Serial.println(solvolts); 
  
  Serial.print("\t Batt V: ");
  Serial.print(batvolts);
  Serial.print("\t Solar V: ");
  Serial.println(solvolts);
  
  display.clearDisplay();
  display.setTextSize(1);
  display.setTextColor(WHITE);
  display.setCursor(0,0);
  display.print("I T1:");
  display.print(t1,1);
  display.print("c  H1:");
  display.print(h1,1);
  display.println("%");

  /*display.print("O T2:");
  display.print(t1,1);
  display.print("c  H2:");
  display.print(h1,1);
  display.println("%");

  display.print("\t Light: ");
  display.print(l,DEC);
  display.print(" uA ");
*/
  display.println();

  display.print("Sol:");
  display.print(solvolts,1);
  display.print("V Batt:");
  display.print(batvolts,1);
  display.println("V");
  display.display();
}

float readVcc() {
  long result;   // Read 1.1V reference against AVcc   
  ADMUX = _BV(REFS0) | _BV(MUX3) | _BV(MUX2) | _BV(MUX1);   
  delay(2); // Wait for Vref to settle   
  ADCSRA |= _BV(ADSC); // Convert   
  while (bit_is_set(ADCSRA,ADSC));
  result = ADCL;
  result |= ADCH<<8;
  result = 1126400L / result; // Back-calculate AVcc in mV   return result;
 return result;
}

double dewPoint(double celsius, double humidity) {
    double A0= 373.15/(273.15 + celsius);
    double SUM = -7.90298 * (A0-1);
    SUM += 5.02808 * log10(A0);
    SUM += -1.3816e-7 * (pow(10, (11.344*(1-1/A0)))-1) ;
    SUM += 8.1328e-3 * (pow(10,(-3.49149*(A0-1)))-1) ;
    SUM += log10(1013.246);
    double VP = pow(10, SUM-3) * humidity;
    double T = log(VP/0.61078);   // temp var
  return (241.88 * T) / (17.558-T);
}

// delta max = 0.6544 wrt dewPoint()
// 5x faster than dewPoint()
// reference: http://en.wikipedia.org/wiki/Dew_point
double dewPointFast(double celsius, double humidity) {
    double a = 17.271;
    double b = 237.7;
    double temp = (a * celsius) / (b + celsius) + log(humidity/100);
    double Td = (b * temp) / (a - temp);
  return Td;
}
