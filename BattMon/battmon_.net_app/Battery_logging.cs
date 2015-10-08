// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using batt_mon_app; // for L_A_12vBattery class access

namespace batt_mon_app
{
	public partial class Form1
	{
		System.IO.StreamWriter m_OutCSVfile = null; 
		private string szBatMonCSVFileName="batterymon_" + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".csv";
		private bool bLoggingToFile=false;
//		private static string szDateTimePatternExcel = @"M/d/yyyy hh:mm:ss tt";
//		private static string szDateTimePatternExcel = @"M/d/yyyy hh:mm:ss.ff tt"; // 'tt' will transform into AM or PM
// or use custom excel format, dd.mm.yyyy HH:mm:ss.000
		private static string szDateTimePatternExcelCstm = @"dd.MM.yyyy HH:mm:ss.fff";
//		private static string szDateTimePatternISO8601Sortable = @"yyyy-MM-ddTHH:mm:ss.fff";

		public bool bLogDataToCSVFile(Generic12Vbattery.strctBattMonData stGivenBattData, int iChrgLvl)
		{
			bool bResult=false;
			string strOneLineToLog=string.Empty;

			if(true==bLoggingToFile)
			{
				if(null==m_OutCSVfile)
				{

// open file if not currently open
					m_OutCSVfile=new System.IO.StreamWriter(szBatMonCSVFileName, true);
				};

// log to file, format if needed
// datetime,volts,apmps,t°C,{D|I|C},milliCoulombsIn, milliCoulombsOut,report reason
#if ISO8601OUT
// ISO 8601 sortable format
//				strOneLineToLog="\"" + dtNowDateTime.ToString(szDateTimePatternISO8601Sortable)+
						"."+ dtNowDateTime.Millisecond;
#else
// format date-time per Excel custom date-time format specification, supplied as argument
				strOneLineToLog="\"" + stGivenBattData.dtBattDateTime.ToString(szDateTimePatternExcelCstm);
#endif
				strOneLineToLog+="\",\"";
				strOneLineToLog+=stGivenBattData.dblBatVolts.ToString() + "\",\"";
				strOneLineToLog+=stGivenBattData.dblBatAmperes.ToString("+#.#;-#.#;0") + "\",\"";
				strOneLineToLog+=stGivenBattData.dblBattTemp.ToString("+#.#;-#.#;0") + "\",\"";
				strOneLineToLog+=stGivenBattData.chBattState+"\",\"";
				strOneLineToLog+=iChrgLvl + "\",\"";
				strOneLineToLog+=stGivenBattData.liQIn + "\",\"";
				strOneLineToLog+=stGivenBattData.liQOut + "\"";
// print entire line to the log file
// will be like his : "19-12-2013 17:36:01.34.nnn","12.4","-0.3","13.6","D","99","67868","78979"
				m_OutCSVfile.WriteLine(strOneLineToLog);
// close file every 25 write to avoid data loss if reset
				if(m_slMeasIterCnt>0 && m_slMeasIterCnt%25==0)
				{
					m_OutCSVfile.Flush();
					m_OutCSVfile.Close();
					m_OutCSVfile.Dispose();
					m_OutCSVfile=null;
				};
			};
			return bResult;
		} // end of bLogDataToCSVFile

		public bool bLogStringToCSVFile(String strToCSVFile)
		{
			bool bResult=false;
			string strOneLineToLog=string.Empty;

			if(true==bLoggingToFile)
			{
				if(null!=m_OutCSVfile)
				{
					bResult=true;
				};
			};
			return bResult;
		} // end of 
	} // end of class Form1
}
