#include <genieArduino.h>

// This Demo communicates with a 4D Systems Display, configured with ViSi-Genie.
// The display has a slider, a cool gauge, an LED Digits, a string box and a User LED.
// The program receives messages from the Slider0 object using the Reported Events. This is triggered each time the Slider changes on the display, and an event
// is genereated and sent automatically. Reported Events originate from the On-Changed event from the slider itself, set in the Workshop4 software.
// Coolgauge is written to using Write Object, and the String is updated using the Write String command, showing the version of the library.
// The User LED is updated by the Arduino, by first doing a manual read of the User LED and then toggling it based on the state received back.

// As the slider changes, it sends its value to the Arduino (Arduino also polls its value using genie.ReadObject, as above), and the Arduino then
// tells the LED Digit to update its value using genie.WriteObject. So the Slider message goes via the Arduino to the LED Digit.
// Coolgauge is updated via simple timer in the Arduino code, and updates the display with its value.
// The User LED is read using genie.ReadObject, and then updated using genie.WriteObject. It is manually read, it does not use an Event.

// This demo illustrates how to use genie.ReadObject, genie.WriteObject, Reported Messages (Events), genie.WriteStr, genie.WriteContrast, plus supporting functions.

Genie genie;
#define RESETLINE 4  // Change this if you are not using an Arduino Adaptor Shield Version 2 (see code below)
void setup()
{
  // NOTE, the genieBegin function (e.g. genieBegin(GENIE_SERIAL_0, 115200)) no longer exists.  Use a Serial Begin and serial port of your choise in
  // your code and use the genie.Begin function to send it to the Genie library (see this example below)

  Serial.begin(200000);  // Serial0 @ 200000 (200K) Baud
  genie.Begin(Serial);   // Use Serial0 for talking to the Genie Library, and to the 4D Systems display

  genie.AttachEventHandler(myGenieEventHandler); // Attach the user function Event Handler for processing events

  // Reset the Display (change D4 to D2 if you have original 4D Arduino Adaptor)
  // If NOT using a 4D Arduino Adaptor, digitalWrites must be reversed as Display Reset is Active Low, and
  // the 4D Arduino Adaptors invert this signal so must be Active High.
  pinMode(RESETLINE, OUTPUT);  // Set D4 on Arduino to Output (4D Arduino Adaptor V2 - Display Reset)
  
  // no adapter - enable reset = 0, unreset = 1; with adapter, reset = 1, unreset = 0
  digitalWrite(RESETLINE, 0);  // Reset the Display via D4
  delay(100);
  digitalWrite(RESETLINE, 1);  // unReset the Display via D4

  delay (3500); //let the display start up after the reset (This is important)

  //Turn the Display on (Contrast) - (Not needed but illustrates how)
  genie.WriteContrast(1); // 1 = Display ON, 0 = Display OFF

  //Write a string to the Display to show the version of the library used
  genie.WriteStr(0, GENIE_VERSION);
}

void loop()
{
  Serial.print("VCC: ");
  Serial.println( readVcc(), DEC);
  static long waitPeriod = millis();
  static int gaugeAddVal = 1;
  static int gaugeVal = 50;

  genie.DoEvents();

  if (millis() >= waitPeriod)
  {
    
    //Write to Meter1 with the value 8 to test
    genie.WriteObject(GENIE_OBJ_METER,0x00, 10);
    
    // Write to CoolGauge0 with the value in the gaugeVal variable
    genie.WriteObject(GENIE_OBJ_COOL_GAUGE, 0x00, gaugeVal);
    gaugeVal += gaugeAddVal;
    if (gaugeVal == 99) gaugeAddVal = -1;
    if (gaugeVal == 0) gaugeAddVal = 1;

    // The results of this call will be available to myGenieEventHandler() after the display has responded
    // Do a manual read from the UserLEd0 object
    genie.ReadObject(GENIE_OBJ_USER_LED, 0x00);

    waitPeriod = millis() + 50; // rerun this code to update Cool Gauge and Slider in another 50ms time.
  }
}

/////////////////////////////////////////////////////////////////////
//
// This is the user's event handler. It is called by genieDoEvents()
// when the following conditions are true
//
//		The link is in an IDLE state, and
//		There is an event to handle
//
// The event can be either a REPORT_EVENT frame sent asynchrounously
// from the display or a REPORT_OBJ frame sent by the display in
// response to a READ_OBJ request.
//

/*COMPACT VERSION HERE, LONGHAND VERSION BELOW WHICH MAY MAKE MORE SENSE
 void myGenieEventHandler(void)
 {
 genieFrame Event;
 int slider_val = 0;
 const int index = 0;  //HARD CODED TO READ FROM Index = 0, ie Slider0 as an example
 
 genieDequeueEvent(&Event);
 
 //Read from Slider0 for both a Reported Message from Display, and a Manual Read Object from loop code above
 if (genieEventIs(&Event, GENIE_REPORT_EVENT, GENIE_OBJ_SLIDER, index) ||
 genieEventIs(&Event, GENIE_REPORT_OBJ,   GENIE_OBJ_SLIDER, index))
 {
 slider_val = genieGetEventData(&Event);  // Receive the event data from the Slider0
 genieWriteObject(GENIE_OBJ_LED_DIGITS, 0x00, slider_val);     // Write Slider0 value to to LED Digits 0
 }
 }*/
// Determine internval voltage of Vcc via AVcc
long readVcc() {
  long result;
  // Red 1.1v reference against AVcc
  ADMUX = _BV(REFS0) | _BV(MUX3) | _BV(MUX2) | _BV(MUX1);
  delay(2) ; //wait for Vreg to settle
  ADCSRA |= _BV(ADSC); //Convert
  while (bit_is_set(ADCSRA,ADSC));
  result = ADCL;
  result |= ADCH<<8;
  result = 1126400 / result; //Back-calculate AVcc in mV
  result = result / 100; // 5v = 50 (removed decimal)
  return result;
}


//LONG HAND VERSION, MAYBE MORE VISIBLE AND MORE LIKE VERSION 1 OF THE LIBRARY
void myGenieEventHandler(void)
{
  genieFrame Event;
  genie.DequeueEvent(&Event);

  int slider_val = 0;

  //If the cmd received is from a Reported Event (Events triggered from the Events tab of Workshop4 objects)
  if (Event.reportObject.cmd == GENIE_REPORT_EVENT)
  {
    if (Event.reportObject.object == GENIE_OBJ_SLIDER)                // If the Reported Message was from a Slider
    {
      if (Event.reportObject.index == 0)                              // If Slider0
      {
        slider_val = genie.GetEventData(&Event);                      // Receive the event data from the Slider0
        genie.WriteObject(GENIE_OBJ_LED_DIGITS, 0x00, slider_val);    // Write Slider0 value to to LED Digits 0
      }
    }
  }

  //If the cmd received is from a Reported Object, which occurs if a Read Object (genie.ReadOject) is requested in the main code, reply processed here.
  if (Event.reportObject.cmd == GENIE_REPORT_OBJ)
  {
    if (Event.reportObject.object == GENIE_OBJ_USER_LED)              // If the Reported Message was from a User LED
    {
      if (Event.reportObject.index == 0)                              // If UserLed0
      {
        bool UserLed0_val = genie.GetEventData(&Event);               // Receive the event data from the UserLed0
        UserLed0_val = !UserLed0_val;                                 // Toggle the state of the User LED Variable
        genie.WriteObject(GENIE_OBJ_USER_LED, 0x00, UserLed0_val);    // Write UserLed0_val value back to to UserLed0
      }
    }
  }

  //This can be expanded as more objects are added that need to be captured

  //Event.reportObject.cmd is used to determine the command of that event, such as an reported event
  //Event.reportObject.object is used to determine the object type, such as a Slider
  //Event.reportObject.index is used to determine the index of the object, such as Slider0
  //genie.GetEventData(&Event) us used to save the data from the Event, into a variable.
}



void myTempRead(void)
{
}

void myVoltRead(void) 
{
}




