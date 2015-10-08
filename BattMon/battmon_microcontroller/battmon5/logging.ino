// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  

// for serial output, use .Net compatible format ISO8601 for time and date as one
// separator is set to be coma(,) ; example below
// 2013-12-07T23:56:57.nnn, 11.78, 0.5, -1.9, C, Qin, Qout, rep_reason
// private static string szDateTimePatternISO8601Sortable = @"yyyy-MM-ddTHH:mm:ss.fff";

void vPrint_Serial(const char* pszPreFormDaTim)
{
  Serial.print(pszPreFormDaTim);
  
  if(true==bWithRaw)
  {
     Serial.print("raw ADC0=");
     Serial.print(sensorValue0);
     Serial.print(", ");
  };
   Serial.print(dblBatteryVoltage, 1);
   if(true==bWithRaw)
   {
     Serial.print("V"); 
   };
   Serial.print(", "); 
   if(true==bWithRaw)
   {
     Serial.print("raw ADC1=");
     Serial.print(sensorValue1);
     Serial.print(", ");
   };  
  Serial.print(dblBatteryCurrent, 1);  
  if(true==bWithRaw)
  {  
    Serial.print("A");   
  };
  Serial.print(", ");   
// print temperature in degrees Celcius.
  Serial.print(dblBatteryTemp,1); 

  if(true==bWithRaw)
  { 
    Serial.print("°C");     
  };
// print charging status
// between -0.3A and + 0.3A - indeterminate
// <-0.4A - discahrding, >+0.4 A - charging
  if(dblBatteryCurrent>=-0.4 && dblBatteryCurrent<=0.4)
  {
      Serial.print(",I");
  }
  else if(dblBatteryCurrent>0.4)  
  {
      Serial.print(",C");
  }
  else
  {
      Serial.print(",D");
  };
// coulombs In - charging performance
  Serial.print(", ");
  Serial.print(liCoulombsIn);
// coulombs out - discharging performance
  Serial.print(", ");
  Serial.print(liCoulombsOut);
  
  if(true==bSignifVoltageChng && true==bSignifCurrentChng)
  {
    Serial.print(", B");
  }
  else if(true==bSignifVoltageChng)
  {
    Serial.print(", V");
  }
  else if(true==bSignifCurrentChng)
  {
    Serial.print(", C");
  }
  else if(true==bSignifTempChng)
  {
    Serial.print(", T");
  };
//  if(true==bWithRaw)
//  {
//    Serial.print(", ");
//    Serial.print("raw ADC2=");
//    Serial.print(sensorValue2);
//  };
// terminate line
  Serial.println(" ");   
  return;
}

void AppendToString(int iValue, char* pString, const int iDigZeroFill) // appends number-to-string to string passed
{ 
  char tempStr[6]={0};
  if(iDigZeroFill==3 && iValue<100)
  {
      strcat(pString,"0");
  };
  if(iDigZeroFill<=3 && iValue<10)
  {
    strcat(pString,"0");
  };
  itoa(iValue,tempStr,10);
  strcat(pString,tempStr);
  return;
}

// use ISO8601 sortable date-time format - YYYY-MM-DDTHH:mm:ss.fff
void vFormatDateTimeStamp(char* pszDateTimeBuff)
{
// send one line of data to log file in the following format
//  datetime,volts,apmps,°C,bat state={D|I|C},milliCoulombsIn, milliCouombsOut,report reason
  if(0!=pszDateTimeBuff)
  {
    AppendToString(iYear,pszDateTimeBuff,4);          // add year to string
    strcat(pszDateTimeBuff,"-");
    AppendToString(iMonth,pszDateTimeBuff,2);          // add month to string  
    strcat(pszDateTimeBuff,"-");
    AppendToString(iDay,pszDateTimeBuff,2);          // add day to string
    strcat(pszDateTimeBuff,"T");
    AppendToString(iHour,pszDateTimeBuff,2);          // add hour to string  
    strcat(pszDateTimeBuff,":");  
    AppendToString(iMinute,pszDateTimeBuff,2);          // add min to string
    strcat(pszDateTimeBuff,":");
    AppendToString(iSecond,pszDateTimeBuff,2);          // add sec to string
    strcat(pszDateTimeBuff,".");
    AppendToString(imilliSec,pszDateTimeBuff,3); // add millisec to string, 000 format
    strcat(pszDateTimeBuff,", "); 
  };
  return; 
}

