#include <VirtualWire.h>

int switchPin = 2;   // switch to change status
int sendPin = 12;     // Module TX pin
int recvPin = 11;    // Module RX pin
int ledPin = 13;     // Change the LED status as proof of signal
int value = 0;

void setup() 
{  
  Serial.begin(9600);           // set up Serial library at 9600 bps
  pinMode(switchPin, INPUT);  
  pinMode(recvPin, INPUT);  
  pinMode(ledPin, OUTPUT);  
  pinMode(sendPin, OUTPUT);    
}

void loop() {  
  value = digitalRead(switchPin); 
  if (value == HIGH) {
    digitalWrite(sendPin, HIGH);  
    Serial.print("TX High, ");
  } 
  else {    
    Serial.print("TX Low, ");
    digitalWrite(sendPin, LOW);  
  }  
  delay(100);
  value = digitalRead(recvPin);  
  if (value == HIGH) {    
    Serial.print("RX High.");
    digitalWrite(ledPin, HIGH);  
  } 
  else {    
    Serial.print("RX Low.");
    digitalWrite(ledPin, LOW);  
  }
  Serial.println("");
}

