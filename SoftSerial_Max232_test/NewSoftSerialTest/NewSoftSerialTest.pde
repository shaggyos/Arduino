
#include <NewSoftSerial.h>

NewSoftSerial mySerial(2, 3);
String Text = "";
String myText = "";

void setup()  
{
  Serial.begin(9600);
  Serial.println("Goodnight moon!");

  // set the data rate for the NewSoftSerial port
  mySerial.begin(9600);
  mySerial.println("Hello, world?");
}

void loop()                     // run over and over again
{
  Text = "";
  myText = "";

  // check for any new text from console, and print to SoftSerial
  while (Serial.available()) {
    // was mySerial.print  
    Text = Text + char(Serial.read());
  }
  if (Text != "") {
    mySerial.println("**" + Text);
    Serial.println("*" + Text);
  }
  delay(50);
  // read in whats waiting on the SoftSerial connection
  while (mySerial.available()) {
      myText = myText + char(mySerial.read());
  }
  
  if (myText != "") {
    Serial.println("+" + myText);
  }
  
}
