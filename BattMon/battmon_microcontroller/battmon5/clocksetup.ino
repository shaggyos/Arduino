// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  

// RTC clock related functions
//
int iStartOrInitClock()
{
  int iret=0;
// first check to verify if RTC clock is operational
//Zero is returned if the DS1307 is not running or does not respond
  if(0==RTC.get())
  {  
       Serial.println("INFO: no RTC module");
// no clock on board, or dead rtc battery, so use hardcoded date-time of last compilation
      vAdjustRTCclocktoCompileTime();
  }
  else
  {
      Serial.println("connecting to RTC ...");
// to adjust RTC clock uncomment line below and run once
//     vAdjustRTCclocktoCompileTime();

// now try to set system time to RTC time      
      setTime(RTC.get());
      setSyncProvider(RTC.get);   // define the function that syncs the time with RTC every 5 min
  
      if(timeStatus()!= timeSet)
      {
         Serial.println("Warning - Unable to sync with the RTC");
// since RTC is not running, request user to set date and time
//       Serial.println("Enter date and time in T10 format");
      }
      else
      {
         Serial.println("RTC has set system time");
      };
  };
  
  return iret;
}

void vAdjustRTCclocktoCompileTime()
{
  char szCompileDate[12]=__DATE__;  // Serial.println(__DATE__); // prints Dec 05 2013
  char szCompileTime[9]=__TIME__; // Serial.println(__TIME__); // prints 15:31:02
  int iMonth=0;
  char* pszCompileTimeHr=&szCompileTime[0];
  char* pszCompileTimeMin=&szCompileTime[3];
  char* pszCompileTimeSec=&szCompileTime[6];
  char* pszCompileDateYr=&szCompileDate[7];
  char* pszCompileDateDay=&szCompileDate[4];
  switch (szCompileDate[0])
  {
    case 'J': // Jan or Jun or Jul
    if(szCompileDate[2]=='a' && szCompileDate[2]=='n')
      iMonth=1;
    else if(szCompileDate[2]=='u' && szCompileDate[2]=='n')
      iMonth=6;
    else
      iMonth=7;
    break;

    case 'F':
    iMonth=2;
    break;

    case 'M': // Mar or May
    if(szCompileDate[2]=='r')
      iMonth=3;
    else
      iMonth=5;
    break;

    case 'A': // Aug or Apr
    if(szCompileDate[1]=='u')
      iMonth=8;
    else
      iMonth=4;
    break;    

    case 'S':
    iMonth=9;
    break;
    case 'O':
    iMonth=10;
    break;
    case 'N':
    iMonth=11;
    break;
    case 'D':
    iMonth=12;
    break;
    default: // this is an error
    break;
  };
// following line sets the RTC to the date & time this sketch was compiled
//  RTC.adjust(DateTime(__DATE__, __TIME__));   
  
  setTime(atoi(pszCompileTimeHr),atoi(pszCompileTimeMin),atoi(pszCompileTimeSec),atoi(pszCompileDateDay),iMonth,atoi(pszCompileDateYr));
// perform one-time RTC set up to compile time
// Returns true for success, or false if any error occurs
   RTC.set(now());
   Serial.print("INFO : set now= ");
   Serial.print(atoi(pszCompileDateYr));
   Serial.print("-");
   Serial.print(iMonth);
   Serial.print("-");
   Serial.print(atoi(pszCompileDateDay));
   Serial.print(" ");
   Serial.print(atoi(pszCompileTimeHr));
   Serial.print(":");
   Serial.print(atoi(pszCompileTimeMin));
   Serial.print(":");
   Serial.print(atoi(pszCompileTimeSec));
   Serial.println(" COMPILE_DATE_TIME ****");
   return;
}
