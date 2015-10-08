#include <VirtualWire.h>
#undef int
#undef abs
#undef double
#undef float
#undef round

#define rxPin 8


const int ledPin =  6;      // the number of the LED pin
int ledState = LOW;             // ledState used to set the LED
long onMillis = 0;        // will store last time LED was updated


void setup()
{
  Serial.begin(9600);
  pinMode(ledPin, OUTPUT); 

  // Initialise the IO and ISR
  vw_set_ptt_inverted(true);          // Required for RX Link Module
  vw_setup(2000);                     // Bits per sec
  vw_set_rx_pin(rxPin);               // We will be receiving on pin 2 ie the RX pin from the module connects to this pin.
  vw_rx_start();                      // Start the receiver
}

void loop()
{
  uint8_t buf[VW_MAX_MESSAGE_LEN];
  uint8_t buflen = VW_MAX_MESSAGE_LEN;

  if (vw_get_message(buf, &buflen))   // check to see if anything has been received
  {
    int i;
    // Message with a good checksum received.

    for (i = 0; i < buflen; i++)
    {
      Serial.print(buf[i]);          // the received data is stored in buffer
    }
    Serial.println(" ");
    digitalWrite(ledPin, HIGH);
    onMillis = millis();
  }

  if (millis() - onMillis > 100 && onMillis > 0)
  {
    onMillis = 0;
    digitalWrite(ledPin, LOW);
  }
}


