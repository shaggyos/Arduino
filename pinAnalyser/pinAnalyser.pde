/*
  DigitalReadSerial
 Reads a digital input on pin 2, prints the result to the serial monitor 
 
 This example code is in the public domain.
 */
float volts = 0;
void setup() {
  Serial.begin(9600);

  for (int pin = 2; pin < 14; ++pin) {
     pinMode(pin,INPUT);
     // digitalWrite(pin, HIGH);
  }
}

void loop() {
  int pinA5 = analogRead(A5);
  float voltsA5 = (0.0049 * pinA5);
  
  int pinA4 = analogRead(A4);
  float voltsA4 = (0.0049 * pinA4);

  int pinA3 = analogRead(A3);
  float voltsA3 = (0.0049 * pinA3);

  int pinA2 = analogRead(A2);
  float voltsA2 = (0.0049 * pinA2);

  int pinA1 = analogRead(A1);
  float voltsA1 = (0.0049 * pinA1);

  int pinA0 = analogRead(A0);
  float voltsA0 = (0.0049 * pinA0);

  int pin2 = digitalRead(2);
  int pin3 = digitalRead(3);
  int pin7 = digitalRead(7);
  int pin9 = digitalRead(9);
  int pin10 = digitalRead(10);
  int pin11 = digitalRead(11);
  int pin12 = digitalRead(12);
  int pin13 = digitalRead(13);
  
  Serial.print("PinA0:");
  Serial.print(voltsA0);

  Serial.print(", PinA1:");
  Serial.print(voltsA1);

  Serial.print(", PinA2:");
  Serial.print(voltsA2);

  Serial.print(", PinA3:");
  Serial.print(voltsA3);

  Serial.print(", PinA4:");
  Serial.print(voltsA4);

  Serial.print(", PinA5:");
  Serial.print(voltsA5);
  
  Serial.print(", Pin2:");
  Serial.print(pin2, DEC);
  Serial.print(", Pin3:");
  Serial.print(pin3, DEC);
  Serial.print(", Pin3:");
  Serial.print(pin7, DEC);
  Serial.print(", Pin7:");
  Serial.print(pin9, DEC);
  Serial.print(", Pin9:");
  Serial.print(pin9, DEC);
  Serial.print(", Pin10:");
  Serial.print(pin10, DEC);
  Serial.print(", Pin11:");
  Serial.print(pin11, DEC);
  Serial.print(", Pin12:");
  Serial.print(pin12, DEC);
  Serial.print(", Pin13:");
  Serial.println(pin13, DEC);
}



