// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  

// notes on using library function analogRead(portNo)
// with default ADC clock set, i.e. the default prescale function returns in 111 microseconds per call.
// faster option : set ADC clock to the max high speed, i.e. to 1 MHz. Then analogRead(portNo) returns in 16 microseconds
//#define FASTADC 1
// The recommended maximum ADC clock frequency is limited by the internal DAC in the conversion circuitry. 
// For optimum performance, the ADC clock should not exceed 200 kHz. 
// However, frequencies up to 1 MHz do not reduce the ADC resolution significantly. 
// Operating the ADC with frequencies greater than 1 MHz is not characterized.

// battery current probe runs OK at 1 MHz with re-read
// battery voltage probe runs noisy at 1 MHz
#ifdef FASTADC
// defines for setting and clearing register bits
#ifndef cbi
#define cbi(sfr, bit) (_SFR_BYTE(sfr) &= ~_BV(bit))
#endif
#ifndef sbi
#define sbi(sfr, bit) (_SFR_BYTE(sfr) |= _BV(bit))
#endif
#endif

int iSetupADC(void)
{
#ifdef FASTADC
// set prescale to 16, i.e. ADC clock to 1 MHz
  sbi(ADCSRA,ADPS2) ;
  cbi(ADCSRA,ADPS1) ;
  cbi(ADCSRA,ADPS0) ;
  Serial.println("ADC running at 1 MHz");
#else
  Serial.println("ADC default speed");
#endif
  return 0;
}

// function returns lead-acid battery volgate in Volts
// assuming 1:3 divider is used, i.e. full scale is 15.0 volts
// ATmega328P ADC A0 - battery voltage/3 ; does NOT require temperature compensation
double dblGetLeadAcidBattVoltageV(void)
{
  double dblBattVoltageV=0.0;
// read the value from the sensor on A0
// The analogRead() command converts the input voltage range, 0 to 5 volts, to a digital value between 0 and 1023. 
// up to 260 microsec Conversion Time
  sensorValue0 = analogRead(sensorPin0);
#ifdef FASTADC
// voltage sensor may become noisy at 1 MHz
// igoner first read, average the next two
  sensorValue0 = analogRead(sensorPin0);
  sensorValue0 = sensorValue0 + analogRead(sensorPin0);
  sensorValue0=sensorValue0 / 2;
#endif
  dblBattVoltageV=((double)sensorValue0 * 15.0)/1023.0; // full scale is 15.0 V if 1:3 divider is used
  return dblBattVoltageV;
}

//ATmega328P ADC A1 - battery current
// Hall sesnor REQUIRES zero current calibration_on_site_ MUST calibrate hall current sensor in-place at 20°C when installed 
// Hall sensor REQUIRES temperature compensation - see function iHallSensorTempCompensate()

// using hall current sesnor, full scale is 5.00 V with zero current around the middle of range
// zero current : 2.465V -> ~505 in raw units
// DC current sensor calibration
// manufacturer : 0A is 2.484V when power supply is 5.00V
// my measurements 0A is 2.472V at 24°C
const double cbdlZeroCurrVoffset=2.469; // in Volts, at 20°C

// functions return battery current in Amperes
double dblGetBattCurrentA(double dblBatteryTempDegC)
{
  double dblBattCurrentA=0.0;
// 0.0V raw count 0 makes -107A, 5.0V raw count 1023 makes +106A
// if -1 is returned, then it is ADC error, i.e. overflow or underflow

  sensorValue1 = analogRead(sensorPin1);
#ifdef FASTADC
  sensorValue1=sensorValue1 + analogRead(sensorPin1);
  sensorValue1=sensorValue1 / 2;
#endif

// perform temperature compensation for hall DC current sensor
    sensorValue1=iHallSensorTempCompensate(sensorValue1, dblBatteryTempDegC); //
// now subtract Voffset, 2.465V -> 505 in raw units    
    dblBattCurrentA=(((double)sensorValue1 * 5.0)/1023.0 - cbdlZeroCurrVoffset)*43.67 ; // 0.0V is -100A, 2.478V is 0A (505 raw ADC counts @ 20C), 5.0 V is +100A  
   return dblBattCurrentA;
}

double dblMeasureSmoothBattTemp_Slow(void)
{
  double dblSmoothBattTemp=0.0;

  for(int ij=0; ij<ciAverageOnTempCycles; ij++)
  {
      dblSmoothBattTemp=dblSmoothBattTemp + dblGetBattTempDegC();
  };
  dblSmoothBattTemp=dblSmoothBattTemp/(double)ciAverageOnTempCycles;
  return dblSmoothBattTemp;
}

// returns battery temperature in degrees C
// uses Atmega328P internal termperature sensor assuming MC is mounted very close to battery case
// reasonably accurate and calibrated from +60C down to -14C at least
double dblGetBattTempDegC(void)
{
  double dblBattTempDegC=0.0;
  unsigned int wADC=0;
// The internal temperature has to be used with the internal reference of 1.1V.
// Channel 8 can not be selected with the analogRead function yet.

// Set the internal reference and mux.
  ADMUX = (_BV(REFS1) | _BV(REFS0) | _BV(MUX3));
  ADCSRA |= _BV(ADEN);  // enable the ADC

  delay(20);            // wait 20 ms for voltages to become stable.

  ADCSRA |= _BV(ADSC);  // Start the ADC

// Detect end-of-conversion
  while(bit_is_set(ADCSRA,ADSC));

// Reading register "ADCW" takes care of how to read ADCL and ADCH.
  wADC = ADCW;

// The offset of 324.31 seems to be reasonably accuratem at least down to -9C
// calibration results
// actual temp at chip  reported temp  raw wADC
//  20.4C                  21.1C+-0.5
//     0C                  0.5
//    60C                  54C
//   -23C
  dblBattTempDegC = ((double)wADC - 324.31) / 1.22;  // was original
//  t = ((double)wADC - 324.31) / 1.355542; // alternative - 1.11x bump up

// The returned temperature is in degrees Celcius.
  return dblBattTempDegC;
}

// function performs temperature compensation for Hall DC current sensor
// in raw ADC counts
int iHallSensorTempCompensate(int iInputSensorValue, double dblTemperatureDegC)
{
   int iTempCompensatedCount=0;
   iTempCompensatedCount=(int)((double)iInputSensorValue - 0.09125 * (dblTemperatureDegC-20.0));
   return iTempCompensatedCount;
}

// SecretVoltmeter from TinkerIt
unsigned long readVcc()
{
  unsigned long result=0;
// Read 1.1V reference against AVcc
  ADMUX = _BV(REFS0) | _BV(MUX3) | _BV(MUX2) | _BV(MUX1);
  delay(2); // Wait for Vref to settle for 2 msec
  ADCSRA |= _BV(ADSC); // Convert
  while(bit_is_set(ADCSRA,ADSC));
  result = ADCL;
  result |= ADCH<<8;
  result = 1126400L / result; // Back-calculate AVcc in mV
  return result;
}

