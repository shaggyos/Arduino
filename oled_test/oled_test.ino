
#define OLED_DC 11
#define OLED_CS 12
#define OLED_CLK 10
#define OLED_MOSI 9
#define OLED_RESET 13

#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

Adafruit_SSD1306 display(OLED_MOSI, OLED_CLK, OLED_DC, OLED_RESET, OLED_CS);

void setup()   {                
  Serial.begin(9600);
  
  // by default, we'll generate the high voltage from the 3.3v line internally! (neat!)
  display.begin(SSD1306_SWITCHCAPVCC);
  // init done
  
  display.clearDisplay();  // clears the screen and buffer
  display.setTextSize(1);
  display.setTextColor(WHITE);
  display.setCursor(0,0);
  display.println("PV/Batt Monitor"); 
  display.println("Booting ...");
  display.display();
 
  delay(2000);
  
  float t1 = 26.2;
  float t2 = 28.5;
  float h1 = 66.2;
  float h2 = 90.2;
  float solvolts = 19.2;
  float batvolts = 13.8;
   
  // text display tests
  display.clearDisplay();
  display.setTextSize(1);
  display.setTextColor(WHITE);
  display.setCursor(0,0);
  display.print("I T1:");
  display.print(t1,1);
  display.print("c  H1:");
  display.print(h1,1);
  display.println("%");
  
  display.print("O T2:");
  display.print(t1,1);
  display.print("c  H2:");
  display.print(h1,1);
  display.println("%");

  display.println();
  display.print("Sol:");
  display.print(solvolts,1);
  display.print("V Batt:");
  display.print(batvolts,1);
  display.println("V");
  display.display();
}

void loop() {
  
}

  
