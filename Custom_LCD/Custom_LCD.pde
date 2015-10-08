#include <LiquidCrystal.h>

LiquidCrystal lcd(8,9,4,5,6,7);

int a=0;

void setup()
{
  lcd.begin(16, 2);

}

void loop()
{
  lcd.clear();
  a=analogRead(0);
  lcd.setCursor(0,0);
  lcd.print("analogRead(A0)");
  lcd.setCursor(0,1);
  lcd.print("value = ");
  lcd.print(a);
  delay(200);
}


