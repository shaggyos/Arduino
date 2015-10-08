#include <Servo.h>
Servo myservo;

int intAngle=50;
int n=0;

void setup()
{
  myservo.attach(8);
}  

void loop()
{
  for (n=0;n<185;n=n+intAngle)
  {
    myservo.write(n);
    delay(500);
  1
  for (n=180;n>0;n=n-intAngle)  
  {
    myservo.write(n);
    delay(500);
  }
}

