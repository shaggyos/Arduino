// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using batt_mon_app; // for L_A_12vBattery class access

namespace batt_mon_app
{
	public class batt_data_parser
	{
		private char[] chrBtStSym={'C','D','U','I'}; // C charding, D discharging, I indeterminate (=no load), U unknown
		private const char chBatState='?';
		private char[] chrDataStrSep={',',';'};
		const string customDateTimeFormatWithMsec=@"dd-MMM-yyyy HH:mm:ss.fff";
//		const string strOformat=@"o";
		private char[] chrarrReportReason={'C','V','B','T'}; // reason for data sent : C change in current, V chnage in voltage, B both current and voltage, T temperature
		private const char chReportReason='?';
//		private DateTime dtOneDateTime;
		private Generic12Vbattery.strctBattMonData m_strctbtParsedDataPkt;

		public batt_data_parser()
		{
			m_strctbtParsedDataPkt.chBattState='?';
			m_strctbtParsedDataPkt.dblBatAmperes=0.0;
			m_strctbtParsedDataPkt.dblBattTemp=0.0;
			m_strctbtParsedDataPkt.dblBatVolts=0.0;
			m_strctbtParsedDataPkt.dtBattDateTime=DateTime.Now;
			m_strctbtParsedDataPkt.liQIn=0;
			m_strctbtParsedDataPkt.liQOut=0;
		}

		public Generic12Vbattery.strctBattMonData stbtGetParsedData()
		{
			return m_strctbtParsedDataPkt;
		}
		
		public bool bParseOneDataLine(string strInputLine)
        {
            bool bResult=true;
            string strOneFieldStr=string.Empty;
			string[] strarrParsedInput=null;
			int lm=0;
			string strCopyOfInput=strInputLine;

// tokenize string to separate fields
			strarrParsedInput=strCopyOfInput.Split(chrDataStrSep);
			for(lm=0; lm<strarrParsedInput.Length && lm<8; ) // expect up to 9 fields
			{
				lm+=1; // count parsed fields
//				Debug.WriteLine("Ser_Port_Listener::bParseOneDataLine() tokenized field["+ lm.ToString()+ "]=" + strarrParsedInput[lm]);
			};

// validate string array
// get at least 4 fields needed datetime,bV,bI,t°,
			if(lm<4)
			{
				Debug.WriteLine("parser::bParseOneDataLine() *** error! Need >=4 fields, got only "+lm.ToString());
                bResult = false;
			};

// process individual fields
            for(int m = 0; m < strarrParsedInput.Length && true==bResult; m++)
            {
//				Debug.WriteLine("parser::bParseOneDataLine() field["+ m.ToString()+ "]=" + strarrParsedInput[m]);
// convert to data
				switch (m)
                {
// date and time
					case 0:
					try
					{
						m_strctbtParsedDataPkt.dtBattDateTime/*dtOneDateTime*/=DateTime.Parse(strarrParsedInput[m]);
// or use specific format parsing
//						CultureInfo MyCultureInfo = new CultureInfo("es-ES");
//						dtOneDateTime=DateTime.Parse(strOneFieldStr, MyCultureInfo);
// defult toString() only prints seconds, i.e. datetime=01-Dec-14 22:39:00
//						Debug.WriteLine("parser::bParseOneDataLine() datetime=" + dtOneDateTime.ToString());

// milliseconds need custom format, i.e - use this
//						Debug.WriteLine("\tparser::bParseOneDataLine() datetime=" + dtOneDateTime.ToString(customDateTimeFormatWithMsec));

// or we can use "o" format as well 
//						Debug.WriteLine("parser::bParseOneDataLine() datetime(`o`)=" + dtOneDateTime.ToString(strOformat));
					}
					catch (SystemException ce)
					{
						Debug.WriteLine("parser::bParseOneDataLine() error "+ ce.ToString()+" getting datetime");
                        bResult = false;
					};
					break;
// battery voltage
					case 1:
					try
					{
						double dblBatVol=System.Convert.ToDouble(strarrParsedInput[m]);
// validate battery voltage: within 0 and 15 volts, since 1:3 divider is used
						if(dblBatVol<0.0 || dblBatVol>16.0)
						{
							Debug.WriteLine("\tparser::bParseOneDataLine(ERR) Vbat out of range! Vbat=" + dblBatVol.ToString("+#.#;-#.#;0") +"V");
							bResult = false;
						}
						else
						{
							m_strctbtParsedDataPkt.dblBatVolts=dblBatVol;
//							Debug.WriteLine("\tparser::bParseOneDataLine() Vbat=" + dblBatVol.ToString() +"V");
						};
					}
					catch (SystemException ce)
					{
						Debug.WriteLine("parser::bParseOneDataLine() error "+ ce.ToString()+" getting Vbat");
                        bResult = false;
					};
					break;
// battery current
					case 2:
					try
					{
						double dblBatCur=System.Convert.ToDouble(strarrParsedInput[m]);
// validate battery current
// with -100A to +100 current sensor exclude all others
						if(dblBatCur<-110.0 || dblBatCur>+110.0)
						{
							Debug.WriteLine("\tparser::bParseOneDataLine(ERR) Ibat out of range! Ibat=" + dblBatCur.ToString("+#.#;-#.#;0")+"A");
	                        bResult = false;
						}
						else
						{
							m_strctbtParsedDataPkt.dblBatAmperes=dblBatCur;
//							Debug.WriteLine("\tparser::bParseOneDataLine() Ibat=" + dblBatCur.ToString("+#.#;-#.#;0")+"A");
						};
					}
					catch (SystemException ce)
					{
						Debug.WriteLine("parser::bParseOneDataLine() error "+ ce.ToString()+" getting Ibat");
                        bResult = false;
					};
					break;
// battery temperature, deg C
					case 3:
					try
					{
						double dblBatTemp = System.Convert.ToDouble(strarrParsedInput[m]);
// validate temperature sensor reading
// ATmega328P can only read beteween -20C and +85C reliavbly
						if(dblBatTemp<-30.0 || dblBatTemp>+85.0)
						{
							Debug.WriteLine("\tparser::bParseOneDataLine(ERR) BatTemp out of range! Temp=" + dblBatTemp.ToString("+#.#;-#.#;0")+"°C");
	                        bResult = false;
						}
						else
						{
							m_strctbtParsedDataPkt.dblBattTemp=dblBatTemp;
//							Debug.WriteLine("\tparser::bParseOneDataLine() BatTemp=" + dblBatTemp.ToString("+#.#;-#.#;0")+"°C");
						};
					}
					catch (SystemException ce)
					{
						Debug.WriteLine("parser::bParseOneDataLine() error "+ ce.ToString()+" getting BatTemp");
                        bResult = false;
					};
					break;
// battery charging state
					case 4:
//	find one of : C,D,I or U. anythine else is an error
					int iBatChrgStateIndx=strarrParsedInput[m].IndexOfAny(chrBtStSym);
					if(-1==iBatChrgStateIndx)
					{
// handle error
						Debug.WriteLine("parser::bParseOneDataLine(Err) BatSate ??");
						bResult = false;
					}
					else
					{
						m_strctbtParsedDataPkt.chBattState=strarrParsedInput[m][iBatChrgStateIndx];
//						Debug.WriteLine("\tparser::bParseOneDataLine() BatSate=" + chBatState);
					};
					break;

// battery milliCoulombs taken, i.e. charge in
					case 5:
					try
					{
						m_strctbtParsedDataPkt.liQIn = System.Convert.ToInt32(strarrParsedInput[m]);
//						Debug.WriteLine("\tparser::bParseOneDataLine() BatClmbsIn=" + lBatCoulmbsIn.ToString()+"mQ");
					}
					catch(System.OverflowException expOverflow) // Value was either too large or too small for an Int32.
					{
						Debug.WriteLine("parser::bParseOneDataLine() [Over/Under]flow "+ expOverflow.ToString()+" getting BatClmbsIn");
						Debug.WriteLine("parser::bParseOneDataLine() Q_in= "+ strarrParsedInput[m]); // example 4294967249
						m_strctbtParsedDataPkt.liQIn=0;
                        bResult = false;
					}
					catch (SystemException ce)
					{
						Debug.WriteLine("parser::bParseOneDataLine() error "+ ce.ToString()+" getting BatClmbsIn");
                        bResult = false;
					};
					break;

// battery milliCoulombs released, i.e. charge out
					case 6:
					try
					{
						m_strctbtParsedDataPkt.liQOut = System.Convert.ToInt32(strarrParsedInput[m]);
//						Debug.WriteLine("\tparser::bParseOneDataLine() BatClmbsOut=" + lBatCoulmbsOut.ToString()+"mQ");
					}
					catch(System.OverflowException expOverflow) // Value was either too large or too small for an Int32.
					{
						Debug.WriteLine("parser::bParseOneDataLine() [Over/Under]flow "+ expOverflow.ToString()+" getting BatClmbsOut");
						Debug.WriteLine("parser::bParseOneDataLine() Q_out= "+ strarrParsedInput[m]);
						m_strctbtParsedDataPkt.liQOut=0;
                        bResult = false;
					}
					catch (SystemException ce)
					{
						Debug.WriteLine("parser::bParseOneDataLine() error "+ ce.ToString()+" getting BatClmbsOut");
                        bResult = false;
					};
					break;
					case 7:
// reason for reporting this line
//	find one of : C,V,T,B or empty. anythine else is an error
					int iRepReasIndx=strarrParsedInput[m].IndexOfAny(chrarrReportReason);
					if(-1==iRepReasIndx)
					{
// report reason may be skipped, if it is not reported BUT next fiels is reported
// normally it should not happen though, i.e. no report reason shall be reported as space, 
//  [0] 2014-02-03T08:07:45.161, 12.3, -0.9, 12.9,D, 0, 23, , 496
// 496 is <extra field>
						if(strarrParsedInput.Length>7)
						{
//							char chReportReason=' ';
						}
						else
						{
// handle error
							Debug.WriteLine("parser::bParseOneDataLine(Err) report reason ??");
							bResult = false;
						};
					}
					else
					{
						char chReportReason=strarrParsedInput[m][iRepReasIndx];
//						Debug.WriteLine("\tparser::bParseOneDataLine() report reason=" + chReportReason);
					};
					break;
// anything else
					default:
					if(strarrParsedInput.Length>8)
					{
// discard 8th and subsequent fields without rasing an error condition
//						Debug.WriteLine("parser::bParseOneDataLine() datafield["+ m.ToString() +"]=" + strarrParsedInput[m]);
					}
					else
					{
						Debug.WriteLine("parser::bParseOneDataLine() ?unkn=" + strarrParsedInput[m]);
						bResult = false;
					};
					break;
				}; // end SWITCH
            }; //end FOR loop

// for debugging in case if we got an error, dump all fields 
			if(false==bResult)
			{
				Debug.WriteLine("parser::bParseOneDataLine() *** Error found, dumping all fields:");
				for(lm=0; lm<strarrParsedInput.Length && lm<9; lm++) // expect up to 9 fields
				{
					Debug.WriteLine("parser::bParseOneDataLine() tokenized field["+ lm.ToString()+ "]=" + strarrParsedInput[lm]);
				};	
			}
			else
			{
			};
            return bResult;
        } // end of bParseOneDataLine()
	}
}
