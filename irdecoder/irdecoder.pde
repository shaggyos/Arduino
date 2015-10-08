/* This Gadget Shield application illustrates the decoding of an infrared
 * remote control. The application learns the pattern of a remote control
 * transmission then is capable of reproducing it on demand, or indicating when
 * it happens again.  This application is often called a "universal remote
 * control" as it can mimic a wide variety of remote control models. Encodings
 * are stored in EEPROM so that learned patterns persist even when power is
 * turned off.
 *
 * This software is licensed under the GNU General Public License (GPL) Version
 * 3 or later. This license is described at
 * http://www.gnu.org/licenses/gpl.html
 *
 * Application Version 1.1 -- December 2010 Rugged Circuits LLC
 * http://www.ruggedcircuits.com
 */

#include <ctype.h>
#include <avr/eeprom.h>
#include <avr/pgmspace.h>
#include <util/crc16.h>
 
/* Define which board you are using. Only uncomment one of the
   following #define's. Or comment them all and let this file 
   guess the board based on the processor:
      ATmega328P --> Uno/Duemilanove
      ATmega324P --> Gator
      ATmega1280 --> Mega
      ATmega2560 --> Mega2560
 */
#define BOARD  0   /* Arduino Uno/Duemilanove (ATmega328P) */
//#define BOARD  1   /* Arduino Mega (ATmega1280/ATmega2560) */
//#define BOARD  2   /* Rugged Circuits Gator (ATmega324P) */

/* Define how many edges per transmission you're allowing.
   Too low a number and you may not be able to discriminate between
   transmissions. Too high a number and you'll run out of RAM. With 2K of RAM
   (ATmega324P/328P) you have room for about 800 edges...more than enough for
   any practical remote control protocol.
*/
#define MAX_EDGES 128

/* Define how many milliseconds to wait for another edge before
   declaring the end of a transmission. 50ms is fairly generous for most remote
   controls.
*/
#define MS_BEFORE_IDLE 50

/* When parameterizing a transmission, this definition sets
   a limit on how many unique edge-to-edge times there can be. Often there are
   only 4-5 unique edge-to-edge times.  If the cluster tolerance (see below) is
   too small then you will see excess codebook entries.
*/
#define CODEBOOK_MAX_CODES 8

/* This definition controls how tightly edge-to-edge times
   can be clustered. The bigger the number, the more tolerant the recognition
   algorithm will be to variations in edge-to-edge timing, but there's a chance
   that what should be unique codes are improperly merged.
*/
#define CODEBOOK_CLUSTER_TOLERANCE 16

/* This definition controls how much extra time to drive the output LED to
 * account for lag in the decoder. That is, edge-to-edge input times are
 * actually artificially too short since the decoder takes a while to recognize
 * the signal. Only very fussy decoders may need a value other than 0.
 */
#define EDGE_TIME_COMPENSATION 0   // Units of 0.1%

/* This definition controls how many extra pulses the IR LED should be given
 * when active, and how many fewer pulses it should be given when inactive.
 * Again, this is to compensate for lag in decoders. Only very fussy decoders
 * may need a value other than 0.
 */
#define EDGE_COUNT_COMPENSATION 0

/* This defines the output frequency. Normally this should be 38 (kHz) but you
 * can try 39 kHz to accommodate both 38kHz and 40kHz receivers (they really
 * should tolerate +/-1kHz deviation)
 */
#define F_OUT 38 /* kHz */

/**********************************************************/
/* YOU SHOULDN'T HAVE TO CHANGE ANYTHING BELOW THIS POINT */
/**********************************************************/

/*********************************************************/
/*                                                       */
/* Board-specific configuration                          */
/*                                                       */
/*********************************************************/

// In case no board is defined, guess from the processor
#ifndef BOARD
#  if defined(__AVR_ATmega328P__)
#    define BOARD 0
#  elif defined(__AVR_ATmega1280__) || defined(__AVR_ATmega2560__)
#    define BOARD 1
#  elif defined(__AVR_ATmega324P__)
#    define BOARD 2
#  else
#    error You must define BOARD near the top of this file
#  endif
#endif

#if (BOARD!=0) && (BOARD!=1) && (BOARD!=2)
#error Unknown board
#endif

// Pushbutton inputs, active low
#define PB1_PIN 12
#define PB2_PIN 8

// RGB LED outputs, active high
#define RED_PIN 5
#define GREEN_PIN 6
#define BLUE_PIN 10

// General-purpose LED's, active high
#define LED0_PIN 11
#define LED1_PIN 4
#if BOARD==2 // GATOR has LED on D23
#  define LED2_PIN 23
#else
#  define LED2_PIN 13
#endif
#define LED3_PIN 3

// IR logic detector input, active low
#define IRLOGIC_PIN 2

// IR transmitter, active high
#define IROUT_PIN 9

// Thumbwheel analog inputs
#define POT0_PIN 0 
#define POT1_PIN 1 

// Light sensor, analog input
#define LIGHTSENSOR_PIN 5

// Potentiometer, analog input
#define ACCEL_X_PIN 4
#define ACCEL_Y_PIN 3
#define ACCEL_Z_PIN 2

// Potentiometer digital GSEL output
#define ACCEL_GSEL_PIN 7

// Interrupt service routine helper functions
static void handleEdge(uint16_t edgeTime);
static void handleOverflow(void);

#if BOARD==0 /* Duemilanove: PORTD2 is INT0: use this interrupt. */
static void configureIRInterrupt(uint8_t enable)
{
	// Enable interrupt on both edges
	EICRA &= ~(_BV(ISC01)|_BV(ISC00));
	EICRA |= _BV(ISC00);

	// Clear any pending interrupts
	EIFR = _BV(0);

	// Enable/disable interrupt 0
  if (enable) {
    EIMSK |= _BV(0); 
  } else {
    EIMSK &= ~_BV(0);
  }
}
ISR(INT0_vect)
{
	handleEdge(TCNT1);
}

static void configureInputTimer(uint8_t enable)
{
	// At 16 MHz, use Timer 1 to measure 4us periods with divide-by-64 prescale
  // This gives a 250ms range.
	TCCR1A = 0;  // Normal mode operation, divide-by-64
	TCCR1B = _BV(CS11) | _BV(CS10);

	// Clear pending interrupt
	TIFR1 = _BV(TOV1);

	// Enable interrupt on overflow as that indicates end of reception
	TIMSK1 = enable ? _BV(TOIE1) : 0;

	// Configure PB1 (OC1A) as an output and set it to 0. This keeps the IR LED off.
  pinMode(IROUT_PIN, OUTPUT);
  digitalWrite(IROUT_PIN, LOW);
}

// Start TCNT1 at EDGE_START_TIME, and if it overflows after 65535 then MS_BEFORE_IDLE milliseconds
// have gone by and this incoming transmission must be over.
static const unsigned EDGE_START_TIME = (65535U - (unsigned)(250U*MS_BEFORE_IDLE)); // 250 ticks/ms
static void resetTimer(void)
{
	TCNT1 = EDGE_START_TIME;
}
ISR(TIMER1_OVF_vect)
{
	handleOverflow();
}
static void waitForInputTimerOverflow(void)
{
	// Clear pending flag
	TIFR1 = _BV(TOV1);
  while ((TIFR1 & _BV(TOV1)) == 0) /* NULL */ ;
}

static void configureOutputTimer(uint8_t enable)
{
	// Configure PB0 as an input with pullup so that input capture doesn't occur
	DDRB &= ~_BV(0);
	PORTB |= _BV(0);

  // Configure PB1 as an output to drive the LED, keep it low until used
  pinMode(IROUT_PIN, OUTPUT);
  digitalWrite(IROUT_PIN, LOW);

  // PB1/OC1A is the output. We want a frequency of F_OUT kHz, which means a
  // division ratio of F_CPU/F_OUT. We use Fast PWM mode 1110 with ICR1 as the
  // upper value and OCR1A=ICR1/2 to get 50% duty cycle.
  TCCR1A = TCCR1B = 0; // Leave timer off while making changes
	ICR1 = (uint16_t)(F_CPU/F_OUT/1000) - 1;
	OCR1A = ICR1/2;

	TCCR1A = _BV(WGM11) | (enable ? _BV(COM1A1) : 0);
	TCCR1B = _BV(WGM13) | _BV(WGM12) | _BV(CS10); // Divide-by-1 clock

	TIMSK1 = 0; // No interrupts
	TCNT1 = 0; // Kick off the counter
}

static void generateOutput(uint16_t numPulses, uint8_t ledon)
{
	if (ledon) {
		TCCR1A |= _BV(COM1A1);
	} else {
		TCCR1A &= ~_BV(COM1A1);
	}

	do {
		TIFR1 = _BV(OCF1A);
		while ( (TIFR1 & _BV(OCF1A)) == 0) /* NULL */ ;
	} while (--numPulses);
}

static uint16_t convertOutputPulses(uint16_t numPulses)
{
	// The input parameter is in units of INPUT pulses, i.e., 4us periods.
	// This number then gets scaled by (100+E)% where E is
	// EDGE_TIME_COMPENSATION/10. Thus if EDGE_TIME_COMPENSATION is 10,
	// the total pulse duration should be scaled by 101%.
	uint32_t duration;

	duration = (uint32_t)numPulses * 4; // Units of us
	duration = ((duration * (1000 + EDGE_TIME_COMPENSATION)) + 500) / 1000; // Slightly higher units of us

	// Now we need to divide by (1/F_OUT kHz), or equivalently multiply by F_OUT
	return (uint16_t) ((duration * (uint8_t)F_OUT + 500U) / 1000U);
}

#elif BOARD==1 /* Mega: PORTE4 is INT4: use this interrupt. */
static void configureIRInterrupt(uint8_t enable)
{
	// Clear any pending interrupts
	EIFR = _BV(4);

	// Enable interrupt on both edges
	EICRB &= ~(_BV(ISC41)|_BV(ISC40));
	EICRB |= _BV(ISC40);

	// Enable interrupt 4
	if (enable) {
		EIMSK |= _BV(4);
	} else {
		EIMSK &= ~_BV(4);
	}
}
ISR(INT4_vect)
{
	handleEdge(TCNT1);
}

static void configureInputTimer(uint8_t enable)
{
	// At 16 MHz, use Timer 1 to measure 4us periods with divide-by-64 prescale
  // This gives a 250ms range.
	TCCR1A = 0;  // Normal mode operation
	TCCR1B = _BV(CS11) | _BV(CS10); // Divide-by-64

	// Clear pending interrupt
	TIFR1 = _BV(TOV1);

	// Enable interrupt on overflow as that indicates end of reception
	TIMSK1 = enable ? _BV(TOIE1) : 0;

	// Configure PE5 (OC3C) as an output and set it to 0. This keeps the IR LED off.
  pinMode(IROUT_PIN, OUTPUT);
  digitalWrite(IROUT_PIN, LOW);
	//DDRE |= _BV(5);
	//PORTE &= ~_BV(5);
}

// Start TCNT1 at EDGE_START_TIME, and if it overflows after 65535 then MS_BEFORE_IDLE milliseconds
// have gone by and this incoming transmission must be over.
static const unsigned EDGE_START_TIME = (65535U - (unsigned)(250U*MS_BEFORE_IDLE)); // 250 ticks/ms
static void resetTimer(void)
{
	TCNT1 = EDGE_START_TIME;
}
ISR(TIMER1_OVF_vect)
{
	handleOverflow();
}
static void waitForInputTimerOverflow(void)
{
	// Clear pending flag
	TIFR1 = _BV(TOV1);
  while ((TIFR1 & _BV(TOV1)) == 0) /* NULL */ ;
}

static void configureOutputTimer(uint8_t enable)
{
  // Make sure IR LED output is low
  pinMode(IROUT_PIN, OUTPUT);
  digitalWrite(IROUT_PIN, LOW);

	// PH6/OC2B is the output. We want a frequency of F_OUT kHz. At 16 MHz, this means a division
	// ratio of F_CPU/F_OUT. We use Fast PWM mode 111 with OCR2A as the upper value OCR2B=OCR2A/2
	// to get 50% duty cycle. Since the output frequency is likely 38 kHz, a 16 MHz clock would
  // need a division ratio of 410...too high. Thus we run with a divide-by-8 clock and use
  // a division ratio of about 50.
  TCCR2A = 0; // Turn timer off before making changes so we don't have intermediate states
  TCCR2B = 0;
	OCR2A = (uint8_t)(F_CPU/F_OUT/8000) - 1;
	OCR2B = OCR2A/2; // 50% duty cycle

	TCCR2A = _BV(WGM21) | _BV(WGM20) | (enable ? _BV(COM2B1) : 0); // Set OC2B on BOTTOM, clear on OCR2B compare
	TCCR2B = _BV(WGM22) | _BV(CS21); // Divide-by-8 clock

	TIMSK2 = 0; // No interrupts
  TCNT2 = 0;  // Kick off the counter
}

static void generateOutput(uint16_t numPulses, uint8_t ledon)
{
	if (ledon) {
		TCCR2A |= _BV(COM2B1);
	} else {
		TCCR2A &= ~_BV(COM2B1);
	}

	do {
		TIFR2 = _BV(OCF2B);
		while ( (TIFR2 & _BV(OCF2B)) == 0) /* NULL */ ;
	} while (--numPulses);
}

static uint16_t convertOutputPulses(uint16_t numPulses)
{
	// The input parameter is in units of INPUT pulses, i.e., 4us periods.
	// This number then gets scaled by (100+E)% where E is
	// EDGE_TIME_COMPENSATION/10. Thus if EDGE_TIME_COMPENSATION is 10,
	// the total pulse duration should be scaled by 101%.
	uint32_t duration;

	duration = (uint32_t)numPulses * 4; // Units of us
	duration = ((duration * (1000 + EDGE_TIME_COMPENSATION)) + 500) / 1000; // Slightly higher units of us

	// Now we need to divide by (1/F_OUT kHz), or equivalently multiply by F_OUT
	return (uint16_t) ((duration * (uint8_t)F_OUT + 500U) / 1000U);
}

#elif BOARD==2 /* Gator: PORTC2 must use pin-change interrupt (PCINT18) */
void configureIRInterrupt(uint8_t enable)
{
	// Enable PCINT18 in pin-change group 2
	PCICR |= _BV(PCIE2);

	// Clear any pending interrupts
	PCIFR = _BV(PCIF2);

	// Enable/disable PCINT18 in the mask register
  if (enable) {
    PCMSK2 |= _BV(2);
  } else {
    PCMSK2 &= ~_BV(2);
  }
}
ISR(PCINT2_vect)
{
	handleEdge(TCNT1);
}

static void configureInputTimer(uint8_t enable)
{
	// At 20 MHz, use Timer 1 to measure 3.2us periods with divide-by-64 prescale
  // This gives a 209ms range.
	TCCR1A = 0;  // Normal mode operation, divide-by-64
	TCCR1B = _BV(CS11) | _BV(CS10);

	// Clear pending interrupt
	TIFR1 = _BV(TOV1);

	// Enable interrupt on overflow as that indicates end of reception
	TIMSK1 = enable ? _BV(TOIE1) : 0;

	// Configure PD5 (OC1A) as an output and set it to 0. This keeps the IR LED off.
  pinMode(IROUT_PIN, OUTPUT);
  digitalWrite(IROUT_PIN, LOW);
}

// Start TCNT1 at EDGE_START_TIME, and if it overflows after 65535 then MS_BEFORE_IDLE milliseconds
// have gone by and this incoming transmission must be over.
static const unsigned EDGE_START_TIME = (65535U - (unsigned)(3125UL*MS_BEFORE_IDLE/10)); // 312.5 ticks/ms
static void resetTimer(void)
{
	TCNT1 = EDGE_START_TIME;
}
ISR(TIMER1_OVF_vect)
{
	handleOverflow();
}

static void waitForInputTimerOverflow(void)
{
	// Clear pending flag
	TIFR1 = _BV(TOV1);
  while ((TIFR1 & _BV(TOV1)) == 0) /* NULL */ ;
}

static void configureOutputTimer(uint8_t enable)
{
	// Configure PD6 as an output set low so input capture doesn't occur. This is also LED3,
  // which is already low due to allLEDs() so no need for anything special here.

  // Configure PD5 as an output to drive the LED, keep it low until used
  pinMode(IROUT_PIN, OUTPUT);
  digitalWrite(IROUT_PIN, LOW);

  // PD5/OC1A is the output. We want a frequency of F_OUT kHz, which means a
  // division ratio of F_CPU/F_OUT. We use Fast PWM mode 1110 with ICR1 as the
  // upper value and OCR1A=ICR1/2 to get 50% duty cycle.
  TCCR1A = TCCR1B = 0; // Leave timer off while making changes
	ICR1 = (uint16_t)(F_CPU/F_OUT/1000) - 1;
	OCR1A = ICR1/2;

	TCCR1A = _BV(WGM11) | (enable ? _BV(COM1A1) : 0);
	TCCR1B = _BV(WGM13) | _BV(WGM12) | _BV(CS10); // Divide-by-1 clock

	TIMSK1 = 0; // No interrupts
	TCNT1 = 0; // Kick off the counter
}

static void generateOutput(uint16_t numPulses, uint8_t ledon)
{
	if (ledon) {
		TCCR1A |= _BV(COM1A1);
	} else {
		TCCR1A &= ~_BV(COM1A1);
	}

	do {
		TIFR1 = _BV(OCF1A);
		while ( (TIFR1 & _BV(OCF1A)) == 0) /* NULL */ ;
	} while (--numPulses);
}

static uint16_t convertOutputPulses(uint16_t numPulses)
{
	// The input parameter is in units of INPUT pulses, i.e., 3.2us periods.
	// This number then gets scaled by (100+E)% where E is
	// EDGE_TIME_COMPENSATION/10. Thus if EDGE_TIME_COMPENSATION is 10,
	// the total pulse duration should be scaled by 101%.
	uint32_t duration;

	duration = ((uint32_t)numPulses*32 + 5)/10; // Units of us
	duration = ((duration * (1000 + EDGE_TIME_COMPENSATION)) + 500) / 1000; // Slightly higher units of us

	// Now we need to divide by (1/F_OUT kHz), or equivalently multiply by F_OUT
	return (uint16_t) ((duration * (uint8_t)F_OUT + 500U) / 1000U);
}
#endif

/*********************************************************/
/*                                                       */
/* Interrupt Service Routines                            */
/*                                                       */
/*********************************************************/

/* The gState variable is S_IDLE when waiting for incoming activity, S_ACQUIRING
 * while activity is still detected (at least 1 edge every MS_BEFORE_IDLE milliseconds),
 * and S_DONE when MS_BEFORE_IDLE milliseconds elapses without any activity. The main
 * loop() function notices the S_DONE state, interprets the incoming transmission, and
 * returns the state to S_IDLE.
 */
typedef enum {
	S_IDLE,
	S_ACQUIRING,
	S_DONE
} state_t;
static volatile state_t gState;

/* Incoming edge-to-edge times are stored here in units of 4us */
static volatile uint16_t gEdgeDelta[MAX_EDGES];
static uint8_t gEncoding[MAX_EDGES+1]; // gEdgeDelta values converted according to codebook
static volatile uint8_t gNumEdges;     // How many edges are stored in gEdgeDelta[]

static void allLEDs(uint8_t enable)
{
  // Don't set LED0/LED3 high on GATOR since LED3 is input capture 1 pin and that could
  // mess up fast PWM mode with ICR1 as TOP.
#if BOARD!=2
  digitalWrite(LED0_PIN, enable ? HIGH : LOW);
#endif
  digitalWrite(LED1_PIN, enable ? HIGH : LOW);
  digitalWrite(LED2_PIN, enable ? HIGH : LOW);
#if BOARD!=2
  digitalWrite(LED3_PIN, enable ? HIGH : LOW);
#endif
}

// This is called in an ISR in response to either a rising or falling edge. It simply
// stores the time between the previous edge and this edge in the gEdgeDelta[] array.
static void handleEdge(uint16_t edgeTime)
{
	switch (gState) {
		case S_IDLE: // First edge....start acquiring more edges
			gState = S_ACQUIRING;
			gNumEdges = 0;
			resetTimer();
      allLEDs(1);
			break;
			
		case S_ACQUIRING:
			if (gNumEdges < MAX_EDGES) {
				gEdgeDelta[gNumEdges++] = edgeTime - EDGE_START_TIME;
      }
			resetTimer();
			break;

		default:
			resetTimer();
			break;
	}
}

// This is called in an ISR to indicate that MS_BEFORE_IDLE milliseconds have elapsed
// thus we consider this incoming transmission complete.
static void handleOverflow(void)
{
	switch (gState) {
		case S_ACQUIRING:
			gState = S_DONE;
			resetTimer();
      allLEDs(0);
			break;

		default:
			resetTimer();
			break;
	}
}

/*********************************************************/
/*                                                       */
/* EEPROM Storage Management                             */
/*                                                       */
/*********************************************************/
#define EE_CRC_STARTVAL (0x1D0F) // Just some random non-zero value

/* A codebook stored in EEPROM is simply a sequence of words
   indicating valid edge-to-edge intervals, ending with 0. For example

	 104, 35, 248, 512, 0

   Following the codebook come indices into the codebook, plus 1,
	 to indicate the encoding, terminating with 0. For example:

	     1, 2, 1, 3, 0

	 would map to edge-to-edge times of:

	     104, 35, 104, 248

   A codebook that begins with 0 indicates the end of used EEPROM. Following
	 this 0 there are 2 bytes that represent a CRC over everything from the
	 start of EEPROM to the last byte, including the 0 that indicates
	 the end of used EEPROM.

   This storage format could be much more efficient, using bit fields for
   example, at the penalty of increased complexity.
*/

// This compound variable is used to help move us through EEPROM
static struct {
	uint16_t addr;
	uint16_t crc;
} eeptr;

static void eeptr_init(void)
{
	eeptr.addr = 0;
	eeptr.crc = EE_CRC_STARTVAL;
}

static uint16_t crcupdate_byte(uint16_t crc, uint8_t val)
{
	crc = _crc16_update(crc, val);
	return crc;
}

static uint16_t crcupdate_word(uint16_t crc, uint16_t val)
{
	crc = _crc16_update(crc, (uint8_t)(val & 0xFF));
	crc = _crc16_update(crc, (uint8_t)((val>>8) & 0xFF));
	return crc;
}

static uint8_t ee_canread8(void)
{
	return eeptr.addr <= (uint16_t)E2END;
}

static uint8_t ee_read8(void)
{
	uint8_t val;

	val = eeprom_read_byte((const uint8_t *) (eeptr.addr));
	eeptr.crc = crcupdate_byte(eeptr.crc, val);
	eeptr.addr++;

	return val;
}

static void ee_write8(uint8_t val)
{
	eeprom_write_byte((uint8_t *)(eeptr.addr), val);
	eeptr.crc = crcupdate_byte(eeptr.crc, val);
	eeptr.addr++;
}

static uint8_t ee_canread16(void)
{
	return eeptr.addr < (uint16_t)E2END;
}

static uint16_t ee_read16(void)
{
	uint16_t val;

	val = eeprom_read_word((const uint16_t *) (eeptr.addr));
	eeptr.crc = crcupdate_word(eeptr.crc, val);
	eeptr.addr += 2;

	return val;
}

static void ee_write16(uint16_t val)
{
	eeprom_write_word((uint16_t *)(eeptr.addr), val);
	eeptr.crc = crcupdate_word(eeptr.crc, val);
	eeptr.addr += 2;
}

static uint8_t ee_iscrcok(void)
{
	if (ee_canread16()) {
		if (eeprom_read_word((const uint16_t *)(eeptr.addr)) == eeptr.crc) {
			return 1;
		}
	}
	return 0;
}

static uint16_t ee_getcrcto(uint16_t eeaddr)
{
	uint16_t addr;
	uint16_t crc;

	crc = EE_CRC_STARTVAL;
	for (addr=0; addr < eeaddr; addr++) {
		crc = crcupdate_byte(crc, eeprom_read_byte((const uint8_t *)addr));
	}
	return crc;
}

// Go through EEPROM and make sure it stores a valid dictionary.
// nextEntry will receive the EEPROM address where the next
// codebook can be stored. numEntries will receive the number of stored
// entries.
static uint8_t isEEPROMValid(uint16_t *nextEntry, uint16_t *numEntries)
{
	uint8_t count;

	eeptr_init();
	if (numEntries) *numEntries=0;

	do {
		/* First read the codebook */
	  count=0;
		while (ee_canread16()) {
			uint16_t val;

			val = ee_read16();
			count++;

			if (val==0) break; // End of codebook
		}

		if (count==1) {  // Single-entry codebook-->end of EEPROM
			if (ee_iscrcok()) {
				if (nextEntry) {
					*nextEntry = eeptr.addr-2;
				}
				return 1;
			}
			return 0;
		}

		/* Now read encoding */
		while (ee_canread8()) {
			uint8_t val;

			val = ee_read8();

			if (val==0) break; // End of encoding
		}

		if (numEntries) (*numEntries)++;

	} while (ee_canread16());

	return 0;
}

// Create a blank dictionary by simply storing the special marker entry.
static void initEEPROM(void)
{
	eeprom_write_word((uint16_t *)0, 0);
	eeprom_write_word((uint16_t *)2, crcupdate_word(EE_CRC_STARTVAL, 0));
}

static uint16_t eepromBytesFree(void)
{
	uint16_t nextEntry;

	if (isEEPROMValid(&nextEntry, 0)) {
		return E2END+1 - nextEntry;
	}
	return 0;
}

/*********************************************************/
/*                                                       */
/* Codebook Handling                                     */
/*                                                       */
/*********************************************************/
 typedef struct {
   uint16_t lb;
   uint16_t ub;
   uint16_t counts;
 } code_t;
 static code_t gCodebook[CODEBOOK_MAX_CODES];
 static uint8_t gNumCodes;
 
 // After gNumEdges are received, this function is called to identify
 // the filtered histogram of edge-to-edge times.
 static uint8_t constructCodebookFromEdges(void)
 {
   uint16_t edge;
   uint8_t code;
   code_t *ptr;
   
   gNumCodes=0;

	 for (edge=0; edge < gNumEdges; edge++) {
		 uint16_t delta;
		 
		 delta = gEdgeDelta[edge];

     // See if this edge-to-edge time already has a code assigned for it,
     // or whether an existing code can be stretched to accommodate it,
     // else create a new code.
		 for (code=0, ptr=gCodebook; code<gNumCodes; code++, ptr++) {
			 if (delta >= ptr->lb) {
				 if (delta <= ptr->ub) {
           // This edge-to-edge time fits into an existing code.
					 ptr->counts++;
					 break;
				 } else if ((delta - ptr->lb) <= CODEBOOK_CLUSTER_TOLERANCE) {
           // This edge-to-edge time can stretch the upper-bound of this code
					 ptr->ub = delta;
					 ptr->counts++;
					 break;
				 }
			 } else if (delta <= ptr->ub) {
				 if ((ptr->ub - delta) <= CODEBOOK_CLUSTER_TOLERANCE) {
           // This edge-to-edge time can stretch the lower-bound of this code
					 ptr->lb = delta;
					 ptr->counts++;
					 break;
				 }
			 }
		 }
		 
     // Did we assign this edge-to-edge time to an existing code, or do we
     // need to create a new code for it?
		 if (code >= gNumCodes) {
			 if (gNumCodes < CODEBOOK_MAX_CODES) {
				 ptr->lb = ptr->ub = delta;
				 ptr->counts=1;
				 gNumCodes++;
			 } else {
				 return 0; // Ran out of codebook space
			 }
		 }
	 }

	 // Now go back over codebook and expand each range to maximum cluster tolerance centered about mean
	 for (code=0; code < gNumCodes; code++) {
		 uint16_t mean;

		 mean = (gCodebook[code].lb + gCodebook[code].ub) / 2;
		 gCodebook[code].lb = mean - CODEBOOK_CLUSTER_TOLERANCE/2;
		 gCodebook[code].ub = mean + CODEBOOK_CLUSTER_TOLERANCE/2;
	 }

	 return 1;
 }
 
// Assuming a codebook has already been constructed, encode
// the latest edge-to-edge times using the codebook. This is done
// before storing the encoding to EEPROM. Alternatively,
// if 'match' is non-zero then verify latest edge-to-edge times
// against previously-constructed encoding. This is done when
// trying to compare an incoming reception to an existing encoding.
static uint8_t constructOrMatchEncodingFromEdges(uint8_t match)
{
	uint8_t edge;

  if (gNumCodes == 0) {
		return 0;
	}

	for (edge=0; edge < gNumEdges; edge++) {
		uint16_t edgetime;
		uint8_t code;

		edgetime = gEdgeDelta[edge];
		for (code=0; code < gNumCodes; code++) {
			if ( (edgetime>=gCodebook[code].lb) && (edgetime<=gCodebook[code].ub) ) {
				if (match) {
					if (gEncoding[edge] != code+1) {
						return 0; // Matches with respect to time duration but doesn't match stored entry
					}
				} else {
					gEncoding[edge] = code+1; // Found the codebook entry for this edge time
				}
				break;
			}
		}
		if (code >= gNumCodes) {
			if (match) {
				// Didn't find a code for this edge time. Not a match.
				;
			} else {
        // This really shouldn't happen if we're encoding edges based on a
        // codebook constructed from those edges!
				writestr(PSTR("Could not construct edge delta time ")); Serial.println(edgetime);
			}
			return 0;
		}
	}
	if (match) {
		if (gEncoding[edge] != 0) {
			return 0; // There are still more edges stored...not a match
		}
	} else {
		gEncoding[edge] = 0; // Mark the end of the encoding with a 0 entry
	}

	return 1;
}

// gEdgeDelta array is filled in. See if it matches any encodings stored in EEPROM
static uint8_t matchEncodingFromEdges(uint8_t *match)
{
	uint8_t entry;
	uint8_t count;

	entry = 0;
	eeptr_init();

	do {
		/* First read the codebook */
	  count=0;
		while (ee_canread16()) {
			uint16_t val;

			val = ee_read16();
			gCodebook[count].lb = val - CODEBOOK_CLUSTER_TOLERANCE/2;
			gCodebook[count].ub = val + CODEBOOK_CLUSTER_TOLERANCE/2;
			count++;

			if (val==0) break; // End of codebook
		}

		if (count==1) {  // Single-entry codebook-->end of EEPROM
			return 0;
		} else {
			gNumCodes = count-1;
		}

		/* Now read encoding */
		count = 0;
		while (ee_canread8()) {
			uint8_t val;

			val = ee_read8();
			gEncoding[count++] = val;

			if (val==0) break; // End of encoding
		}
		gNumEdges = count-1;

		if (constructOrMatchEncodingFromEdges(1)) {
			if (match) *match = entry;
			return 1;
		}

		entry++;
	} while (ee_canread16());
	return 0;
}

// Append recent encoding to stored dictionary in EEPROM
static uint16_t saveEncodingToEEPROM(void)
{
	uint16_t nextEntry;
	uint16_t requiredSpace;
	uint8_t code;
	uint16_t numEntries;

	if (isEEPROMValid(&nextEntry, &numEntries)) {
		requiredSpace = (gNumCodes+1)*sizeof(uint16_t) + (gNumEdges+1)*sizeof(uint8_t) + 4;
		if ((nextEntry + requiredSpace) > E2END) return 0;

		eeptr.addr = nextEntry;
		eeptr.crc = ee_getcrcto(nextEntry);

		// Write the codebook as 16-bit integers, followed by a 0 entry
		for (code=0; code<gNumCodes; code++) {
			ee_write16( (gCodebook[code].lb + gCodebook[code].ub)/2 );
		}
		ee_write16(0);

		// Write the encoding as 8-bit integers, followed by a 0 entry
		for (code=0; gEncoding[code] != 0; code++) {
			ee_write8(gEncoding[code]);
		}
		ee_write8(0);

		// Finally, write a 0 word (empty codebook) and CRC
    ee_write16(0);
		ee_write16(eeptr.crc);
	}
	return numEntries+1;
}

#if 0   // Debugging functions
static void dumpEncoding(void)
{
	uint8_t code;

	Serial.println("Codebook:");
	for (code=0; code < gNumCodes; code++) {
		Serial.print("["); Serial.print(gCodebook[code].lb); Serial.print(","); Serial.print(gCodebook[code].ub);
		Serial.print("]: "); Serial.println(gCodebook[code].counts);
	}

	Serial.println("Encoding:");
	for (code=0; code <= gNumEdges; code++) {
		Serial.print("  "); Serial.println((uint16_t)(gEncoding[code]));
	}
}

static void dumpEEPROM(void)
{
	uint8_t count;

	eeptr_init();

	do {
		Serial.println("Codebook:");

		/* First read the codebook */
	  count=0;
		while (ee_canread16()) {
			uint16_t val;

			val = ee_read16();
			count++;

			Serial.print("  "); Serial.println(val);
			if (val==0) break; // End of codebook
		}

		if (count==1) {  // Single-entry codebook-->end of EEPROM
			return;
		}

		Serial.println("Encoding");
		/* Now read encoding */
		while (ee_canread8()) {
			uint8_t val;

			val = ee_read8();
			Serial.print("  "); Serial.println((uint16_t)val);

			if (val==0) break; // End of encoding
		}
	} while (ee_canread16());
}

static void dumpEdges(void)
{
	uint8_t edge;

	for (edge=0; edge < gNumEdges; edge++) {
		Serial.println(gEdgeDelta[edge]);
	}
}
#endif

// In preparation for transmitting a stored encoding, find the starting
// address of this stored encoding in EEPROM.
static uint8_t getStoredEncoding(uint8_t num, uint16_t *addr) 
{
	uint8_t count;

	eeptr_init();

	do {
		if (num-- == 0) {
			if (addr) *addr = eeptr.addr;
			return 1;
		}

		/* First read the codebook */
	  count=0;
		while (ee_canread16()) {
			uint16_t val;

			val = ee_read16();
			count++;

			if (val==0) break; // End of codebook
		}

		if (count==1) {  // Single-entry codebook-->end of EEPROM
			return 0;
		}

		/* Now read encoding */
		while (ee_canread8()) {
			uint8_t val;

			val = ee_read8();

			if (val==0) break; // End of encoding
		}
	} while (ee_canread16());

	return 0;
}

// This array will store the number of 38 kHz (or whatever F_OUT is) pulses
// to generate for each codebook entry.
static uint16_t gPulseLengths[CODEBOOK_MAX_CODES];

// This function actually drives the output IR LED from a stored encoding
static uint8_t outputEncoding(uint8_t num)
{
  uint16_t addr;
	uint16_t length;
	uint8_t numCodes;
	uint8_t numEdges;
	uint8_t ledOnToggle;
	uint8_t edge, val;

  if (! getStoredEncoding(num, &addr)) return 0;

	for (numCodes=0; numCodes < CODEBOOK_MAX_CODES; numCodes++) {
		length = eeprom_read_word((const uint16_t *)addr);
		addr += 2;

		if (length==0) break;

    // Convert input pulse length in 4us periods to output length
    // in 1/F_OUT periods.
		gPulseLengths[numCodes] = convertOutputPulses(length);
	}

	// Shouldn't happen
	if (numCodes >= CODEBOOK_MAX_CODES) return 0;

	numEdges = 0;
	do {
		val = eeprom_read_byte((const uint8_t *)addr);
		addr++;

		if (val == 0) break;
		else val--; // Remember the -1 since we store +1  !!!!!!

		if (val < numCodes) {
			gEdgeDelta[numEdges++] = gPulseLengths[val];
		} else {
			; // Shouldn't happen
		}
	} while (1);
		
  // Now turn off the input timer and interrupt and turn on the output timer.
	configureIRInterrupt(0);
	configureInputTimer(0);
	configureOutputTimer(1);

  allLEDs(1);

  // First edge represents an LED ON condition. From there, every subsequent
  // edge alternates between OFF and ON conditions.
	ledOnToggle = 1;

	for (edge=0; edge < numEdges; edge++) {
		generateOutput(gEdgeDelta[edge] + (ledOnToggle ? EDGE_COUNT_COMPENSATION : -EDGE_COUNT_COMPENSATION), ledOnToggle);
		ledOnToggle = ! ledOnToggle;
	}

  // Go back to input timer and interrupt so we can match incoming transmissions.
  // First delay a little bit since our own receiver sees our transmission and 
  // we get a spurious incoming edge.	
  configureOutputTimer(0);
  resetTimer(); waitForInputTimerOverflow();
	configureIRInterrupt(1);
	configureInputTimer(1);

  allLEDs(0);

	return 1;
}

/*********************************************************/

static void writestr(PGM_P str)
{
  uint8_t c;

  while ((c=pgm_read_byte(str)) != 0) {
    Serial.print(c);
    str++;
  }
}

static void writestrln(PGM_P str)
{
  writestr(str);
  Serial.println();
}

static void printHelp(void)
{
  writestrln(PSTR(\
"Press a key:\n" \
"  0: Erase/initialize EEPROM\n" \
"  1: Learn a new encoding\n" \
"  2: Test last learned encoding\n" \
"  3: Save last learned encoding to EEPROM\n" \
"  M: Output stored encoding\n" \
"  V: Verify stored encodings in EEPROM\n" \
"  F: Print free EEPROM space\n" \
"  H: Print this help menu" \
         ));
}

// This variable is non-zero when the user pressed '1' to learn a new encoding
static uint8_t gLearning;

// This variable is non-zero when the user pressed '2' to test a new encoding
static uint8_t gTestEncoding;

void setup()
{
	// Configure IR decoder as an input with pullup
	pinMode(IRLOGIC_PIN, INPUT);
	digitalWrite(IRLOGIC_PIN, HIGH); // Enable internal pullup

  // Configure other outputs to nominal values
  pinMode(RED_PIN, OUTPUT); digitalWrite(RED_PIN, LOW);
  pinMode(GREEN_PIN, OUTPUT); digitalWrite(GREEN_PIN, LOW);
  pinMode(BLUE_PIN, OUTPUT); digitalWrite(BLUE_PIN, LOW);
  pinMode(LED0_PIN, OUTPUT); digitalWrite(LED0_PIN, LOW);
  pinMode(LED1_PIN, OUTPUT); digitalWrite(LED1_PIN, LOW);
  pinMode(LED2_PIN, OUTPUT); digitalWrite(LED2_PIN, LOW);
  pinMode(LED3_PIN, OUTPUT); digitalWrite(LED3_PIN, LOW);
  pinMode(ACCEL_GSEL_PIN, OUTPUT); digitalWrite(ACCEL_GSEL_PIN, LOW); // 1.5g range when low, 6g when high

	// Configure the interrupt on IR input
	configureIRInterrupt(1);

  // Disable IR output until user commands it
	configureOutputTimer(0);

	// Configure the timer to measure intervals
	configureInputTimer(1);

	gState = S_IDLE;
	gLearning = 0;
	gTestEncoding = 0;
	gNumCodes = 0;

	Serial.begin(9600);

	// printHelp();

	if (!isEEPROMValid(0, 0)) {
		writestrln(PSTR("\nBegin by pressing '0' to prepare the EEPROM to store encodings"));
	}
}
 
void loop()
{
	uint8_t match;

	switch (gState) {
		case S_DONE:
			gState = S_IDLE;

			if (gLearning) {
				if (! constructCodebookFromEdges()) {
					writestrln(PSTR("Too many symbols. Either try again or increase CODEBOOK_MAX_CODES."));
				} else if (constructOrMatchEncodingFromEdges(0)) {
					writestrln(PSTR("Success...now press '2' to test the encoding"));
				} else {
					writestrln(PSTR("Could not encode transmission."));
				}
				// printHelp();
				gLearning = 0;
			} else if (gTestEncoding) {
				if (constructOrMatchEncodingFromEdges(1)) {
					writestr(PSTR("Match"));
				} else {
					writestr(PSTR("Mismatch"));
				}
				writestrln(PSTR("...try again or press '2' to return to main menu"));
			} else if (matchEncodingFromEdges(&match)) {
					writestr(PSTR("Matched entry ")); Serial.println((uint16_t)match);
			} else {
				writestrln(PSTR("Unknown button pressed"));
			}
			break;

		case S_IDLE:
			if (Serial.available()) {
				if (gLearning) {
					switch (Serial.read()) {
						case '1':
							writestrln(PSTR("Cancelled"));
							Serial.println();
							// printHelp();
							gLearning = 0;
							break;

						default:
							break;
					}
				} else if (gTestEncoding) {
					switch (Serial.read()) {
						case '2':
							writestrln(PSTR("If the encoding works well, press '3' to save to EEPROM"));
							// printHelp();
							gTestEncoding=0;
							break;

						default:
							break;
					}
				} else {
					switch (Serial.read()) {
						case 'V': case 'v':
							{
								uint16_t numEntries;
								writestr(PSTR("EEPROM stored data is "));
								if (isEEPROMValid(0, &numEntries)) {
									writestr(PSTR("valid. There are ")); Serial.print(numEntries);
									writestrln(PSTR(" stored buttons."));
								} else {
									writestrln(PSTR("not valid. Press '0' to initialize EEPROM."));
								}
							}
							break;

						case '0':
							initEEPROM();
							writestrln(PSTR("Stored data erased. Press '1' to learn a new encoding."));
							break;

						case '1':
							writestrln(PSTR("Press a remote control button. Press '1' to cancel."));
							writestrln(PSTR("Try to press it as quickly as possible to avoid repeat codes."));
							gLearning = 1;
							break;

						case '3':
							if (! gNumCodes) {
								writestrln(PSTR("Please use '1' to learn a button first."));
							} else {
								uint16_t entryNum;
								if ((entryNum=saveEncodingToEEPROM()) != 0) {
									writestr(PSTR("Saved to EEPROM as entry ")); Serial.println(entryNum-1);
									gNumCodes = 0;
								} else {
									writestrln(PSTR("EEPROM is full -- could not save data."));
								}
							}
							break;

						case 'F': case 'f':
							{
								uint16_t percentage;

								writestr(PSTR("EEPROM is "));
								percentage = ((uint32_t)eepromBytesFree()*100 + (E2END+1)/2)/(E2END+1);
								Serial.print(percentage);
								writestrln(PSTR("% free"));
							}
							break;

						case '2':
							writestrln(PSTR("Press the same button again to test. Press '2' again to cancel."));
							gTestEncoding=1;
							break;

						case 'H': case 'h':
							printHelp();
							break;

						case 'M': case 'm':
							{
								uint8_t val;

								writestrln(PSTR("Type in an encoding number followed by #"));
								val = 0;
								do {
									if (Serial.available()) {
										int c;

										c = Serial.read();
										if (isdigit(c)) {
											val *= 10;
											val += c-'0';
										} else if (c == '#') break;
									}
								} while (1);
								writestr(PSTR("Output encoding ")); Serial.println((uint16_t)val);
								if (! outputEncoding(val)) {
									writestrln(PSTR("No such encoding"));
								}
								// printHelp();
							}
							break;

						default:
							break;
					}
				}
			}
			break;

		default: // S_ACQUIRING
			break;
	}
}

// vim: sw=2 ts=2 ai cindent syntax=c expandtab
