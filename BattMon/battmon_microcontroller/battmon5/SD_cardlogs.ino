// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  

#if (USESDCARDLOG)
/////////////////////////////////////////////////////
void FATtime(uint16_t* puiDate, uint16_t* puiTime)
{
// return date using FAT_DATE macro to format fields
  *puiDate = FAT_DATE(year(), month(), day());

// return time using FAT_TIME macro to format fields
  *puiTime = FAT_TIME(hour(), minute(), second());
}
///////////////////////////////////////////////////////////////////////////////////////
SdFat sd;       //// file system object Handle for the SD media
SdFile logfile; // the logging file
// initialize the SD card
boolean bInitSDCard()
{
  boolean bRetState=false;
  pinMode(10, OUTPUT);                  // must set DEFAULT CS pin output, even if not used

  SdFile::dateTimeCallback(FATtime);     // set the timestamp for the file on the SD card.

// see if the card is present and can be initialized
//  if(0==sd.begin(CS_PIN, SPI_HALF_SPEED))
  if(0==sd.begin(CS_PIN, SPI_FULL_SPEED))
  { 
    Serial.println("Error- cannot init SD Card");
    return bRetState;
  };

#if (SINGLE_FILE)                                // reuse the same filename
  char filename[] = "BATLOGF.LOG";               // create a new file
  logfile = logfile.open(filename, FILE_WRITE);  // opens a new file if it doesn't exist
#else                                            // create a new filename each boot
  char filename[] = "BATLOG00.LOG";              // create a new file
  for(uint8_t i = 0; i < 100; i++)
  {
    filename[6] = i/10 + '0';
    filename[7] = i%10 + '0';
    if((logfile.open(filename, O_CREAT | O_EXCL | O_WRITE))) break;
  };
#endif

  if(0==logfile.isOpen())
  {
     Serial.println("Error- cannot open log file on SD Card");
     return bRetState;
  };

  Serial.print("Log File:");
  Serial.println(filename);
//  delay(3000); 
  bRetState=true;

  return bRetState;
}

void vPrint_SDCard(const char* pszPreFormDaTim)
{
  logfile.print(pszPreFormDaTim);    
 
  if(true==bWithRaw)
  {
     logfile.print("raw ADC0=");
     logfile.print(sensorValue0);
     logfile.print(", ");
  };  

// with 3x divide, full scale is 15.0 Volts
   logfile.print(dblBatteryVoltage,1); // 
   if(true==bWithRaw)
   {
     logfile.print("V"); 
   };
   logfile.print(", "); 
   if(true==bWithRaw)
   {
     logfile.print("raw ADC1=");
     logfile.print(sensorValue1);
     logfile.print(", ");
   };
//
  logfile.print(dblBatteryCurrent,1);
  if(true==bWithRaw)
  {  
    logfile.print("A");   
  };
  logfile.print(", ");   
// print temperature in degrees Celcius.
  logfile.print(dblBatteryTemp,1); 
// terminate the line
  if(true==bWithRaw)
  { 
    logfile.print("°C");     
  };
// print charging status
// between -0.2A and + 0.2A - indeterminate
// <-0.2A - discahrding, >+0.2 A - charging
  if(dblBatteryCurrent>=-0.2 && dblBatteryCurrent<=0.2)
  {
      logfile.print(",I");
  }
  else if(dblBatteryCurrent>0.2)  
  {
      logfile.print(",C");
  }
  else
  {
      logfile.print(",D");
  };
// coulombs In - charging performance
  logfile.print(", ");
  logfile.print(liCoulombsIn,1);
// coulombs out - discharging performance
  logfile.print(", ");
  logfile.print(liCoulombsOut,1);
  
  if(true==bSignifVoltageChng && true==bSignifCurrentChng)
  {
    logfile.print(", B");
  }
  else if(true==bSignifVoltageChng)
  {
    logfile.print(", V");
  }
  else if(true==bSignifCurrentChng)
  {
    logfile.print(", C");
  }
  else if(true==bSignifTempChng)
  {
    logfile.print(", T");
  };

  logfile.print(" \r\n");    
  logfile.sync();      // flush the buffer to the SD card
  return;
}
#endif

/*
ATmega328P internal Temperature sensor Calibration data:
raw deC   offset    coeff
331  5.4    324.31   1.22
334  8.1
352  22,5
386  50.5




*/

