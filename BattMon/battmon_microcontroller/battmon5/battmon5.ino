// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  

#define USESDCARDLOG  true
// ---------------------------------------
// include files for system components
#include <Arduino.h>    // defines arduino platform
#if (USESDCARDLOG)
#include <SdFat.h>      // for SD Card
#endif
#include <Time.h>	// time library functions, uses microcontroller system clock
#include <Wire.h>       // for communication to RTC
#include <DS1307RTC.h> // library for DS1307RTC clock, depends on time.h and wire.h

// D13 (CLK), D12 (MISO), D11 (MOSI)    SPI Pins for SD card
#define CS_PIN  10  // Logger Shield Chip Select pin for the SD card

// The circuit:
// * analog sensors on analog inputs 0, 1, and 2
// A0 - battery voltage/3.0
// current sensors have power supply 5V from Arduino board
// A1 - battery current sensor (+100 A to -100A) ranged to 5V ... 0V
// A2 - high current sensor (+500A to -500A) ranged to 5V to 0V, OPTIONAL

int sensorPin0 = A0;    // select ADC0 for battery voltage input
int sensorPin1 = A1;    // select ADC1 for low range battery current -100A .. +100A
int sensorPin2=A2; // select ADC2 for high range battery current -500A .. +500A (starter motor mostly)

int sensorValue0 = 0;  // variable to store the battery voltage coming from the sensor 0
int sensorValue1 = 0;  // variable to store the battery current coming from the sensor 1
int sensorValue2 = 0;  // variable to store the battery current coming from the sensor 2

// battery data averaging constants
const int ciAverageOnVACycles=5; // smoothing for V and I
const int ciSendKeepAliveEveryN=50;
const int ciAverageOnTempCycles=30; // smoothing for temperature
int iAverVACnt=0;
long int liCntKeepAlive=0;
// moment's battery volage, current and temperature - known good values to be used to log and make computations
double dblBatteryVoltage=0.0;
double dblBatteryCurrent=0.0;
double dblBatteryTemp=0.0;
// when battery state changes are slow, averaged battery values are also computed and then used 
double dblAveBatteryVoltage=0.0;
double dblAveBatteryCurrent=0.0;
double dblAveBatteryTemp=0.0;
// battery Coulombic count, in milliCoulombs, separately for incoming (=charge accepted) and outcoming(=charge released)
long int liCoulombsIn=0;
long int liCoulombsOut=0;
// some boolean variables to handle battery thresholds in voltage, current and temperature separately
boolean bSignifVoltageChng=false;
boolean bSignifCurrentChng=false;
boolean bSignifTempChng=false;
// tells is SD card is functional
boolean bSDcardOk=false;
// date-time variables for time stamping battery data in the log
int iYear=2014; // some defaults in case if RTC clock isn't communicating
int iMonth=01;
int iDay=04;
int iHour=0;
int iMinute=0;
int iSecond=0;
int iPrevSecond=0;
int imilliSec=0; // number of msec in current time seconds
int iPrevMilliSec=0;
int iMillisecOffset=0; // number of milliseconds from start of microcontroller to setting up systemtime
// total number of msec since microcontroller started
unsigned long ulMilliSec=0; 
unsigned long ulprevMilliSec=0; // millis return 32-bit value
const boolean bWithRaw=false; // addds raw ADC counts to SD card or to serial port output

//
// ++++++++++++++++++++++++++++++++++++ S E T U P ++++++++++++++++++++++++++++++
//
void setup()
{
  bSignifVoltageChng=false;
  bSignifCurrentChng=false;
  bSignifTempChng=false;
//Initialize serial port 1 to high speed
  Serial.begin(115200); 
  Serial.print("batt_mon maxS version variable data rate");
// report board voltage
  Serial.print(" Vcc=");
  Serial.print(readVcc()/1000.0, 2);
  Serial.println(" V");

// initialize, test or/and setup RTC clock
  iStartOrInitClock();

  iPrevSecond=iSecond=second();
// so at this moment RTC has set system time to exact second, i.e to HH:mm:ss.000
  Serial.print(hour());
  Serial.print(":");
  Serial.print(minute());
  Serial.print(":");
  Serial.print(iSecond);
// microcontroller was running for a while by now, so millis() has non-zero value to be added to system time
// to use system time accurate to millisecond
  iPrevMilliSec=imilliSec=(int)(millis()%1000);
  iMillisecOffset=(int)(millis());
  Serial.print(".");
  Serial.println(iMillisecOffset);

#if (USESDCARDLOG)  
// initialize SD crd module
  bSDcardOk=bInitSDCard();  
  if(false==bSDcardOk)
  {
    Serial.println("SD card not present or card error");
  };
#else  
    Serial.println("SD card support not defined");
#endif  
  Serial.println("Sampling rate=variable, each cycle 4 msec to 24 msec");
  ulprevMilliSec=ulMilliSec=millis();
// optionally switch ADC to high speeed mode
  iSetupADC();
  
// prints title with ending line break 
  Serial.println("--batt_mon setup done"); 
} 

// this function is called repeatedly in external loop
// duration without SDcard : 4 msec typically, or 24 msec when temperature is measured in every 1000 cycles
// duration with SD card : 
//
//============================================= L O O P =======================
//
void loop()
{
// this function is called every nnn microseconds, nnn varies by MC load  
//
// local variables which are instantiated every time
  boolean bSignificantChange=false;
  char szDateTimeBuff[32]={0};
// static variable which are initialized once and then persist
  static double sdblPrevVoltage=0.0;
  static double sdblPrevCurrent=0.0;
  static double sdblPrevTemperature=0.0;
  static unsigned long sulCycleCntr=0; // store 32 bits (4 bytes)
  static double sdblAbsInstantCurrentChange=0.0;
  double dblBatCurrMed=0.0;
  const int ciAveBattCurr=4;

// entry point
// assume no significant change in battery parameters has occurred yet
  bSignifVoltageChng=false;
  bSignifCurrentChng=false;
  bSignifTempChng=false;

// save current date-time for timestamping the logged battery data
  iYear=year();
  iMonth=month();
    
// measuring temperature is slow and noisy
// we measure temperature only approx. every >=4 seconds (1000 cycles * >=4 msec each)
  if(0==sulCycleCntr || (sulCycleCntr%1000)==0)// 
  {
    dblBatteryTemp=dblGetBattTempDegC(); // ------- t t t t t°
  };

// obtain momentary battery voltage and current.
// battery voltmeter is temperature compensated ------- V V V V V 
  dblBatteryVoltage=dblGetLeadAcidBattVoltageV();
  
  dblBatCurrMed=0.0;
//  for(int j=0; j<ciAveBattCurr;j++)
//  {
//    delay(1);
// battery current sensor needs to know temperature for compensation ---- A A A A A
    dblBatteryCurrent=dblGetBattCurrentA(dblBatteryTemp);
//    dblBatCurrMed=dblBatCurrMed+dblBatteryCurrent;
//  };
//  dblBatteryCurrent=dblBatCurrMed/((double)ciAveBattCurr);
  
// save date-time of battery parameters measured for time stamping data for logging  
  ulMilliSec=millis() - iMillisecOffset; // at this tmoment, time is 04.940 sec
  iSecond=second(); 
  imilliSec=(int)(ulMilliSec%1000); 
// be sure we don't log time stamp jumping backwards, e.g. 04.998 and 05.998
  if(imilliSec<iPrevMilliSec && iPrevSecond==iSecond)
  {
    iSecond+=1;
  };
  
  iMinute=minute();
  iHour=hour();
  iDay=day();

// on the very first measurement cycle make previous and current battery data equal
  if(0==sulCycleCntr)
  {
    sdblPrevTemperature=dblBatteryTemp;
    sdblPrevVoltage=dblBatteryVoltage;
    sdblPrevCurrent=dblBatteryCurrent;
  };

// update coulombic count. Perform trapezoidal integration to obtain this iteration coulombic count
// cut off current noise which is between +- 0.1A for 100A current sensor
  if(dblBatteryCurrent>0.1 && sulCycleCntr!=0)
  {
      liCoulombsIn+=(long)((double)(ulMilliSec - ulprevMilliSec) * ((dblBatteryCurrent+sdblPrevCurrent)/2.0) ); // charge, in milliCoulombs
  }
  else if(dblBatteryCurrent<-0.1 && sulCycleCntr!=0)
  {
    liCoulombsOut-=(long)((double)(ulMilliSec - ulprevMilliSec) * ((dblBatteryCurrent+sdblPrevCurrent)/2.0) ); // discharge, in milliCoulombs
  };

// determine first if SIGNIFICANT change occurred in EITHER voltage or current
// determine if voltage changed enough     
    if(abs(sdblPrevVoltage-dblBatteryVoltage)>=(0.05*sdblPrevVoltage) )  // battery voltage changed > 5% 
    {
// voltage rising from any value to
      if(dblBatteryVoltage>sdblPrevVoltage) 
      {
        if(dblBatteryVoltage>1.0) // to more than 1 Volt
          bSignifVoltageChng=true;
      }
      else
      {
// voltage falling, to any value
        if(sdblPrevVoltage>1.0)
          bSignifVoltageChng=true;
      };
    };
// determine if current is changed enough : current chaned >= 5% and more than 1.1A and is outside indeterminate current zone of -0.2A to +0.2A   
    sdblAbsInstantCurrentChange=abs(sdblPrevCurrent-dblBatteryCurrent);
    if(sdblAbsInstantCurrentChange>=(0.05*abs(sdblPrevCurrent)) && sdblAbsInstantCurrentChange>1.1 && // battery current changed > 5% and deltaI>1.1A
            abs(dblBatteryCurrent)>0.2) // +- 0.2A is current meter indeterminate noise range
    {
      bSignifCurrentChng=true;      
    };
// if either voltage or current changed significatly, set the global battery parameters changed flag    
    if(true==bSignifVoltageChng || true==bSignifCurrentChng)
    {
// yes  
      bSignificantChange=true;
    }
    else
    {
      bSignificantChange=false;
    };

// ^^^^^^^^^^^^ decision point ^^^^^^^^^^
// if we got sudden change in either current, voltage or temperature => we must report them immediately
// this dynamic method ensure we do not miss fast changes in battery parameters, such as electrical load was turned on or off (e.g. lights or starter)
  if(true==bSignificantChange) // real change in data
  {
// preformat date-time string since we need it for either case
      vFormatDateTimeStamp(szDateTimeBuff);
// for significant changes, report both to serial port and
      vPrint_Serial(szDateTimeBuff);

// and log to SD card if it is available
      if(true==bSignificantChange && true==bSDcardOk)
      {
#if (USESDCARDLOG)        
          vPrint_SDCard(szDateTimeBuff);
#endif
      };
// and we abandon previous smoothing in progress since values have changed a lot
// reset average values for the next iteration
      iAverVACnt=0;
      dblAveBatteryVoltage=0.0;
      dblAveBatteryCurrent=0.0;
  }
  else  // i.e. when if(false==bSignificantChange)
// no significant change in voltage or current, then proceed with averaging and noise reduction
  {
      if(iAverVACnt<ciAverageOnVACycles) // every 5 calls do this
      {
//        dblAveBatteryTemp=dblAveBatteryTemp+dblBatteryTemp;
        dblAveBatteryVoltage=dblAveBatteryVoltage+dblBatteryVoltage;
        dblAveBatteryCurrent=dblAveBatteryCurrent+dblBatteryCurrent;
        iAverVACnt+=1;    
      }
      else
      {
// perform some averaging on voltage(Volts) and current(Amperes)to get reasonably smooth results
// to report to c# application
        dblBatteryVoltage=dblAveBatteryVoltage/ciAverageOnVACycles;
        dblBatteryCurrent=dblAveBatteryCurrent/ciAverageOnVACycles;
// reset average values for the next iteration
        iAverVACnt=0;
        dblAveBatteryVoltage=0.0;
        dblAveBatteryCurrent=0.0;

// now we can also send keep-alive data message to c# application
// using smoothed values to avoid noisy data there, roughly every ~1 sec
        if((liCntKeepAlive % ciSendKeepAliveEveryN)==0) // we average on 5 V-A cycles for voltage and current, and then dump out every 10th average as keep alive values
        {
// preformat date-time string since we need it for either case
          vFormatDateTimeStamp(szDateTimeBuff);
          
// once in a while measure new temperature
// note : it takes ~20 msec to measure temperature, so it may cause dropping a lot of quick changing voltage and current
          if((liCntKeepAlive % (9*ciSendKeepAliveEveryN))==0) // every ~5 sec
          {                        
//           dblBatteryTemp=dblGetBattTempDegC(); // quick single measurement
             dblBatteryTemp=dblMeasureSmoothBattTemp_Slow(); // slow with smoothing, takes ~4 sec
                        
            if(abs(sdblPrevTemperature-dblBatteryTemp)>1.0) // t° changed more than 1.0 deg C
            {
              bSignifTempChng=true;
              bSignificantChange=true;
#if (USESDCARDLOG)        
              vPrint_SDCard(szDateTimeBuff);
#endif              
              sdblPrevTemperature=dblBatteryTemp;              
            };
          };

          vPrint_Serial(szDateTimeBuff);
        };        
        liCntKeepAlive+=1;
      }; // end inner IF siAverCnt < ciAverageOnVACycles
  }; // end IF-ELSE no significant change occurred in voltage or current
      
// save current current values as previous for the run to use them
  sdblPrevVoltage=dblBatteryVoltage;
  sdblPrevCurrent=dblBatteryCurrent;
  ulprevMilliSec=ulMilliSec;
  iPrevSecond=iSecond;
  iPrevMilliSec=imilliSec;
  sulCycleCntr+=1;
  return;
} // end of loop() function
// =======================================================================================


