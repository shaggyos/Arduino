/*
  RemoteDecode sketch
 Infrared remote control signals are decoded to control LED brightness
 The values for keys 0 through 4 are detected and stored when the sketch starts
 key 0 turns the LED off, the brightness increases in steps with keys 1 through 4
 */

#include <IRremote.h>               // IR remote control library

const int irReceivePin = 2;         // pin connected to the output of the IR detector
const int redledPin = 9;         // LED is connected to a PWM pin
const int yelledPin = 8;
const int grnledPin = 7;
const int blueledPin = 6;
const int byteLED = 15; 
int bitLED = 0;   // red = 2^0=1 , yellow = 2^1 = 2, green = 2^2 = 4, blue = 2^3 = 8
int oldbit = 0;
const int numberOfKeys = 34;         //  5 keys are learned (0 through 4)
// long irKeyCodes[numberOfKeys];      // holds the codes for each key

IRrecv irrecv(irReceivePin);        // create the IR library
decode_results results;             // IR data goes here

long irKeyCodes[numberOfKeys] =
{
  0xFD9A65,           // 0 bPwr 16620133
  0xFDB24D,            // 1 bMute 16626253
  0xFD708F,            // 2 bInfo 16609423
  0xFDB04F,            // 3 bPause 16625743
  0xFD8A75,            // 4 bMenu 16616053
  0xFD48B7,            // 5 bEPG 16599223
  0xFD8877,            // 6 bExit 16615543
  0xFD58A7,            // 7 bOk 16603303
  0xFD5AA5,            // 8 bLeft 16603813
  0xFDD827,            // 9 bRight 16635943
  0xFD609F,            // 10 bUp 16605343
  0xFD6897,            // 11 bDown 16607383
  0xFDAA55,            // 12 bTvRadio 16624213
  0xFDA857,            // 13 bFav 16623703
  0xFDC837,            // 14 bTime 16631863
  0xFDE817,            // 15 bRecall 16640023
  0xFDF00F,            // 16 b0 16642063
  0xFD4AB5,            // 17 b1 16599733
  0xFD0AF5,            // 18 b2 16583413
  0xFD08F7,            // 19 b3 16582903
  0xFD6A95,            // 20 b4 16607893
  0xFD2AD5,            // 21 b5 16591573
  0xFD28D7,            // 22 b6 16591063
  0xFD728D,            // 23 b7 16609933
  0xFD32CD,            // 24 b8 16593613
  0xFD30CF,            // 25 b9 16593103
  0xFD52AD,            // 26 bSubtitle 16601773
  0xFD12ED,            // 27 bText 16585453
  0xFD10EF,            // 28 bLanguage 16584943
  0xFDD02F,            // 29 bAudio 16633903
  0xFD629D,            // 30 bRed 16605853
  0xFD22DD,            // 31 bGreen 16589533
  0xFD20DF,            // 32 bYellow 16589023
  0xFDE01F };          // 33 bBlue 16637983

void setup()
{
  Serial.begin(9600);
  pinMode(irReceivePin, INPUT);
  
  pinMode(redledPin, OUTPUT);
  pinMode(yelledPin, OUTPUT);
  pinMode(grnledPin, OUTPUT);
  pinMode(blueledPin, OUTPUT);

  irrecv.enableIRIn();              // Start the IR receiver
  //  learnKeycodes();                  // learn remote control key  codes
  Serial.println("Press a remote key");
}

void loop()
{
  long key;
  int  brightness;

  if (irrecv.decode(&results))  // non-blocking buffer check
  {
    // here if data is received
    irrecv.resume();
    key = convertCodeToKey(results.value);
    if(key >= 0)
    {
      Serial.print("Got key ");
      Serial.print(key);
      Serial.print(" - ");
      
      switch(key) {
      case 0: // fire the laser!!!
        Serial.print("POW, KAPOW!, POWPOW POW");
        oldbit=bitLED;
        digitalWrite(grnledPin, LOW);
        digitalWrite(redledPin, LOW);
        digitalWrite(yelledPin, LOW);
        digitalWrite(blueledPin, LOW);
        delay(100);
        digitalWrite(grnledPin, HIGH);
        digitalWrite(redledPin, HIGH);
        digitalWrite(yelledPin, HIGH);
        digitalWrite(blueledPin, HIGH);
        delay(200);
        digitalWrite(grnledPin, LOW);
        digitalWrite(redledPin, LOW);
        digitalWrite(yelledPin, LOW);
        digitalWrite(blueledPin, LOW);
        delay(50);
        digitalWrite(grnledPin, HIGH);
        digitalWrite(redledPin, HIGH);
        digitalWrite(yelledPin, HIGH);
        digitalWrite(blueledPin, HIGH);
        delay(150);
        digitalWrite(grnledPin, bitRead(oldbit,2));
        digitalWrite(redledPin, bitRead(oldbit,0));
        digitalWrite(yelledPin, bitRead(oldbit,1));
        digitalWrite(blueledPin, bitRead(oldbit,3));
        bitLED=oldbit;
      break;
      case 7:  // stop
        Serial.print("STOP!");
        digitalWrite(grnledPin, LOW);
        digitalWrite(redledPin, LOW);
        digitalWrite(yelledPin, LOW);
        digitalWrite(blueledPin, LOW);
        bitLED=0;
        break;
      case 8:  // move left
        Serial.print("Turn Left!");
        break;
      case 9:  // move Right
        Serial.print("Turn Right!");
        break;
      case 10:  // move up
        Serial.print("Move forward!");
        break;
      case 11:  // move down
        Serial.print("Move backward!");
        break;
      case 16: // 0 - go to start, and play first track
        Serial.print("Go to first track and wait");
        break;
      case 17:  // 1 play track 1
        Serial.print("Play track 1");
        break;
      case 18:  // 2 play track 2
        Serial.print("Play track 2");
        break;
      case 19:  // 3 play track 3
        Serial.print("Play track 3");
        break;
      case 20:  // 4 play track 4
        Serial.print("Play track 4");
        break;
      case 21:  // 5 play track 5
        Serial.print("Play track 5");
        break;
      case 22:  // 6 play track 6
        Serial.print("Play track 6");
        break;
      case 23:  // 7 play track 7
        Serial.print("Play track 7");
        break;
      case 24:  // 8 play track 8
        Serial.print("Play track 8");
        break;
      case 25:  // 9 play track 9
        Serial.print("Play track 9");
        break;
      case 30:  // red toggle red led (pin 9)
        Serial.print("Toggle RED LED!");
        if (bitRead(bitLED,0)==0)
        {
          digitalWrite(redledPin, HIGH);
          bitSet(bitLED,0);
        }
        else
        {
          digitalWrite(redledPin, LOW);
          bitClear(bitLED,0);
        } 
        break;
      case 31:  // green toggle green led (pin 7)
        Serial.print("Toggle GREEN LED!");
        if (bitRead(bitLED,2)==0)
        {
          digitalWrite(grnledPin, HIGH);
          bitSet(bitLED,2);
        }
        else
        {
          digitalWrite(grnledPin, LOW);
          bitClear(bitLED,2);
        } 
        break;
      case 32:  // yellow toggle yellow led (pin 8)
        Serial.print("Toggle YELLOW LED!");
        if (bitRead(bitLED,1)==0)
        {
          digitalWrite(yelledPin, HIGH);
          bitSet(bitLED,1);
        }
        else
        {
          digitalWrite(yelledPin, LOW);
          bitClear(bitLED,1);
        } 
        break;
      case 33:  // blue toggle blue led (pin 6)
        Serial.print("Toggle BLUE LED!");
        if (bitRead(bitLED,3)==0)
        {
          digitalWrite(blueledPin, HIGH);
          bitSet(bitLED,3);
        }
        else
        {
          digitalWrite(blueledPin, LOW);
          bitClear(bitLED,3);
        } 
        break;

      default:  // default option - in this case do nothing.
        Serial.print("oops - we've hit the default!!!");
        break;
      }
      Serial.println("!");
      // brightness = map(key, 0,numberOfKeys-1, 0, 255);
      // analogWrite(redledPin, brightness);
    }
  }
}

/*
 * get remote control codes
 * /
 void learnKeycodes()
 {
 while(irrecv.decode(&results))   // empty the buffer
 irrecv.resume();
 
 Serial.println("Ready to learn remote codes");
 long prevValue = -1;
 int i=0;
 int flag = 1;
 while( i < numberOfKeys)
 {
 Serial.print("press remote key ");
 Serial.print(i);
 while(true)
 {
 if( irrecv.decode(&results) )
 {
 if(results.value != -1 && results.value != prevValue)
 {
 flag = showReceivedData(i);
 irKeyCodes[i] = results.value;
 i = i + flag;
 prevValue = results.value;
 irrecv.resume(); // Receive the next value
 break;
 }
 irrecv.resume(); // Receive the next value
 }
 }
 }
 Serial.println("Learning complete");
 }
 */
/*
 * converts a remote protocol code to a logical key code (or -1 if no digit
 received)
 */
int convertCodeToKey(long code)
{
  for( int i=0; i < numberOfKeys; i++)
  {
    if( code == irKeyCodes[i])
    {
      return i; // found the key so return it
    }
  }
  return -1;
}


/*
 * display the protocol type and value
 * /
 int showReceivedData(int flag)
 {
 if (results.decode_type == UNKNOWN)
 {
 Serial.println("-Could not decode message");
 return 0;
 }
 else
 {
 if (results.decode_type == NEC) {
 Serial.print("- decoded NEC: ");
 }
 else if (results.decode_type == SONY) {
 Serial.print("- decoded SONY: ");
 }
 else if (results.decode_type == RC5) {
 Serial.print("- decoded RC5: ");
 }
 else if (results.decode_type == RC6) {
 Serial.print("- decoded RC6: ");
 }
 Serial.print("hex value = ");
 Serial.print( results.value, HEX);
 Serial.print(", ");
 Serial.println(results.value);
 }
 return 1;
 
 }
 
 */



