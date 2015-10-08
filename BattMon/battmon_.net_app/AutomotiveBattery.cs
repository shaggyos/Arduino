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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace batt_mon_app
{
// class contains everything we need to know about
// specific make & model of 12 Volts automotive Lead-acid battery
// inherits many properties from generic 12Volt battery class

// some functions and constants are static for convinience of use outside

	[Serializable] public class AutomotiveBattery : Generic12Vbattery
	{
//		private int iNewBattCap_mAHrs=iNewBattCap_mAHrs*1000;
// file name to save/restore my battery class instances
		public const string cstrMyBattClassFileName="myAutoBatteryNAPA65.bin"; 
// list of member structures may be serialized to a sparate file
		public const string cstrListInMyBattFileName="myAutoBatteryNAPA65.list.bin"; 

		private int m_iEngineStartEvtCnt=0;
		private bool m_bStarterMotorIsOn=false;
		private double m_sdblMaxBattDischrgCurrent=+999.0;
		private double m_sdblMaxBattChrgCurrent=0.0;
		private strctBattMonData stcBattDatStarterOn;
		private strctBattMonData stcBattDatStarterOff;
		private strctBattMonData stcBattDatStarterOffAfterOn;
		private strctBattMonData stcBattDatCurrentCrossZero;
		private strctBattMonData stcBattDatMaxChargCurr;
		private strctBattMonData stcBattPrevData;
		private double m_dblBatteryResistance=0.0;
		private double m_dblCA=0.0;
		private double m_dblAvrgCurrent100Smpls=0.0;
		private double m_dblTempCurrent=0.0;
		private const int m_ciCurrAvrgCount=101;
		private int m_iCurrentAvrgIdx=0;
// ring buffer - stores last 400 battery data packets all the time
// ring buffer should be quick
		private const int ciMaxHistoSize=32;
		private int[] m_arriTimesBetwItersHistogr=null;
// a constructor for automotive battery can do several things
// and also launch a thread to display batt info form
		public AutomotiveBattery(string strName, string strMake, string strModel, string strSerNo, 
					int iCA, int iCCA, int iCapAHrs, DateTime dtmDateOfmanuf)
		{
			m_strBatteryName=strName;
			m_strBatteryMake=strMake;
			m_strBatteryModel=strModel;
			m_strBatterySerialNo=strSerNo;
			m_dtmBattDateOfManuf=dtmDateOfmanuf;
			m_iBatteryCapacityAHrs=iCapAHrs;
			m_iBatteryCA=iCA;
			m_iBatteryCCA=iCCA;
			m_arriTimesBetwItersHistogr=new int [ciMaxHistoSize];
		}
// default ctor for new cass instance of my battery
// initialize all parameters and member variable to their default values first
// optionally open a form to prompt user to fill in required parameters for new battery
		public AutomotiveBattery()
		{
			Debug.WriteLine("AutomotiveBattery::AutomotiveBattery() ctor");
			m_strBatteryName="NAPA 65 A*Hr 650 CCA";
			m_strBatteryMake="NAPA";
			m_strBatteryModel="12345";
			m_iEngineStartEvtCnt=0;
// create a new list to hold discharge-charge cycles information
//			m_lstChrgDschrgCycles=new List<stDischrgChrgCycle>();
// create new mmap of discharge-charge cycles with datetime of start as a key
			m_mapDischrgChrgCyles = new Dictionary<DateTime, stDischrgChrgCycle>();
			m_mapDischrgChrgCyles.Clear();
			m_arriTimesBetwItersHistogr=new int [ciMaxHistoSize];
//
			m_dblBatteryResistance=0.0;
			m_dblCA=0.0;
			m_dblAvrgCurrent100Smpls=0.0;
			m_dblTempCurrent=0.0;
			m_iCurrentAvrgIdx=0;
		}

// auxillary functions to save and restore class instance to/from media
// serialization and deserialization
		public bool bSerBattCls() // may take class ptr as (AutomotiveBattery abtMyClass)
		{
			bool bCompletedSerialization=false;
			Debug.WriteLine("++bSerBattCls()");

// save battery class instance to persistance
			IFormatter formatter = new BinaryFormatter();
			try
			{
				Stream strmBatCls = new FileStream(AutomotiveBattery.cstrMyBattClassFileName, FileMode.Create,  FileAccess.Write, FileShare.None);
// this will serialize entire battery class instance into binary file.
// member variables will be serialized. Data structures will not
				formatter.Serialize(strmBatCls, this);
				strmBatCls.Close();
				Debug.WriteLine("bSerBattCls() Also saved "+ m_mapDischrgChrgCyles.Count + " discharge-charge cycles");

				bCompletedSerialization=true;
			}
			catch(Exception exp)
			{
				Debug.WriteLine("Form1() ~dtor error saving my battery class to storage. " + exp.ToString());
				bCompletedSerialization=false;
			};

			Debug.WriteLine("--bSerBattCls()="+bCompletedSerialization.ToString());
			return bCompletedSerialization;
		}

		public static AutomotiveBattery bDeserBattCls()
		{
			AutomotiveBattery abtClassFromStorage=null;
			Debug.WriteLine("++bDeserBattCls()");

			IFormatter formatter = new BinaryFormatter();
			try
			{
				Stream streamFrom = new FileStream(AutomotiveBattery.cstrMyBattClassFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				abtClassFromStorage = (AutomotiveBattery)formatter.Deserialize(streamFrom);
				streamFrom.Close();
				int jcnt=abtClassFromStorage.m_mapDischrgChrgCyles.Count;
				Debug.WriteLine("bSerBattCls() Also restored "+ jcnt.ToString() + " discharge-charge cycles");
				for(int k=0; k<jcnt; k++)
				{
// element at retrievs a pair, not just an element
					stDischrgChrgCycle stdchrgclTemp=abtClassFromStorage.m_mapDischrgChrgCyles.ElementAt(k).Value;
					Debug.WriteLine("bSerBattCls() ["+k.ToString()+"] "+stdchrgclTemp.stbtdtmStarterOn.dtBattDateTime.ToString()+
								" CA=" + stdchrgclTemp.dblCA.ToString("000") + " A, R=" +  stdchrgclTemp.dblBatteryResistance.ToString("0.0000") +" Ohm at " + stdchrgclTemp.stbtdtmStarterOn.dblBattTemp.ToString("+00.0")+"°C");
				};
			}
			catch(SerializationException serexp)
			{
				Debug.WriteLine("Form1() ctor: Failed to find saved instance of myAutoBattery." + serexp.ToString());
			}
			catch(IOException ioexp)
			{
				Debug.WriteLine("Form1() ctor: Failed to find saved instance of myAutoBattery." + ioexp.ToString());
			};

			Debug.WriteLine("--bDeserBattCls()="+ ((null==abtClassFromStorage)?"null":abtClassFromStorage.ToString()) );
			return abtClassFromStorage;
		}

		public bool bIsStarterMtrOn()
		{
			return m_bStarterMotorIsOn;
		}

		public double dblGetBattInternalResistance()
		{
			return m_dblBatteryResistance;
		}

		public double dblGetBattCA()
		{
			return m_dblCA;
		}

// don't give out entire dictionary - it is protected element
		public stDischrgChrgCycle dctGetBatChrgCycl(DateTime dtmFindBy)
		{
			stDischrgChrgCycle stdsccrgTempCycle=default(stDischrgChrgCycle);
			bool bKeyFound=false;
			Debug.WriteLine("++dctGetBatChrgCycl(key=)");

			try
			{
				bKeyFound=m_mapDischrgChrgCyles.ContainsKey(dtmFindBy);

				if(true==bKeyFound)
				{
// dictionary has this key, so retrieve entire element
					stdsccrgTempCycle=m_mapDischrgChrgCyles[dtmFindBy];
				};				
			}
			catch(ArgumentNullException excnotIn)
			{
				Debug.WriteLine("dctGetBatChrgCycl() key not found! " + excnotIn.ToString());
			};

			Debug.WriteLine("--dctGetBatChrgCycl(key=)");
			return stdsccrgTempCycle;
		}

		public stDischrgChrgCycle dctGetBatChrgCyclByIdx(int Index)
		{
			stDischrgChrgCycle stdsccrgTempCycle=default(stDischrgChrgCycle);
			if(Index<m_mapDischrgChrgCyles.Count)
			{
				stdsccrgTempCycle=m_mapDischrgChrgCyles.ElementAt(Index).Value;
			};
			return stdsccrgTempCycle;
		}

		public DateTime dtmGetBatChrgCyclDatTimByIdx(int Index)
		{
			DateTime dtmTemp=default(DateTime);

			if(Index<m_mapDischrgChrgCyles.Count)
			{
				dtmTemp=m_mapDischrgChrgCyles.ElementAt(Index).Key;
			};
			return dtmTemp;
		}

		public int iGetNumStartingCycles()
		{
			return m_mapDischrgChrgCyles.Count;
		}
//
		public bool bWasEngineStarted()
		{
			bool bRetVal=false;
			if(m_iEngineStartEvtCnt>0)
			{
				bRetVal=true;
			};
			return bRetVal;
		}

		public double getMaxChrgCurrentA()
		{
			return m_sdblMaxBattChrgCurrent;
		}

		public double getMaxDischrgCurrentA()
		{
			return m_sdblMaxBattDischrgCurrent;
		}

		public double getLastAveCurrent()
		{
			return m_dblAvrgCurrent100Smpls;
		}

		strctBattMonData stbtdtmTempFirstRecord=default(strctBattMonData);
//
// this fucntion is called often or very often to process battery data
// can be used equally well for live battery data in real time or to process recorded battery data
//
		public bool bValuateBatteryData(strctBattMonData stRecentBattData, int ciIterationNo)
		{
			bool bRes=false;
			stDischrgChrgCycle stdschrgNewCycle=default(stDischrgChrgCycle); // neends init such as in DateTime dteSrc = default(DateTime), 
			TimeSpan tmspnStarterMotorRun;
//			Debug.WriteLine("++bValuateBatteryData("+ciIterationNo+")");

// compute average current and keep it to eliminate noise in current sensor line
			if(m_iCurrentAvrgIdx<m_ciCurrAvrgCount)
			{
				m_dblTempCurrent+=stRecentBattData.dblBatAmperes;
				m_iCurrentAvrgIdx+=1;
			}
			else
			{
				m_dblAvrgCurrent100Smpls=m_dblTempCurrent / (double)m_ciCurrAvrgCount;
				m_iCurrentAvrgIdx=0;
				m_dblTempCurrent=0.0;
			};

// on the very first iteration save this as beginning of recording
			if(0==ciIterationNo)
			{
				stbtdtmTempFirstRecord=stRecentBattData;
				if(null==m_arriTimesBetwItersHistogr)
				{
					m_arriTimesBetwItersHistogr=new int [ciMaxHistoSize];
				};
// erase histogram as well 
				Array.Clear(m_arriTimesBetwItersHistogr,0,ciMaxHistoSize); 
			}
			else
			{
// next we check for Microcontroller errors.
				TimeSpan tmspnBetwnIters=stRecentBattData.dtBattDateTime - stcBattPrevData.dtBattDateTime;
// Timer tick may be reported by microcontroller as inaccurate time, including jumping backawards
				if(tmspnBetwnIters.TotalSeconds < 0.0)

// ignore very first inetration, since previous date-time is invalid
//				if(tmspnBetwnIters.Days < 0)
				{
					Debug.WriteLine("bValuateBatteryData(ERROR) time jumped backwards! Previous=" + stcBattPrevData.dtBattDateTime.ToString("HH:mm:ss.fff") + " > New=" + stRecentBattData.dtBattDateTime.ToString("HH:mm:ss.fff"));
// fix up error - use last known good date-time
					stRecentBattData.dtBattDateTime=stcBattPrevData.dtBattDateTime;
				}
				else
				{
// for good iteration, save data to histogram
//					Debug.WriteLine("bValuateBatteryData() Curr-Prev duration " + tmspnBetwnIters.Days.ToString() + "."+ tmspnBetwnIters.Hours.ToString("0#") +":"+ tmspnBetwnIters.Minutes.ToString("0#")+":"+tmspnBetwnIters.Seconds.ToString("0#.")+"."+tmspnBetwnIters.Milliseconds.ToString("000")); 
					int iIntervBetwIters=(int)tmspnBetwnIters.TotalMilliseconds;
					if(iIntervBetwIters < ciMaxHistoSize)
					{
						m_arriTimesBetwItersHistogr[iIntervBetwIters]+=1;
					};
// for every 1000 iterations, print a histoghram
					if(0==ciIterationNo%999)
					{
						for(int j=0; j<ciMaxHistoSize; j++)
						{
							Debug.WriteLine("["+j.ToString()+"] : " + m_arriTimesBetwItersHistogr[j].ToString()); 
						};
					};
				}; // end IF tmspnBetwnIters.TotalSeconds >0
			}; // end IF check for Microcontroller errors.

// here we observe a few parameters including min and max of voltage and current
// as well as watch for engine starting pattern : starter mtr OFF, ON, OFF sequence

// criteria - low discharge current for several seconds - key on ignition on
// 	then high discharge current for a second or two - starter on
// then dischanrge current changes to charge current
// then battery gets strong charging which should quckly deacrease within 15 seconds if battery is healthy

//======================================================================
//        -80A       -40A       -2A  -0.1A     0A    +0.1A   +2A  +100A
// str mtr |         discharging      |    no load    |    charging
//                                { --->-------->-------->----}   current crosses zero amperes line
//======================================================================

//-------------------------------------------------------------------------------------------
// BATTERY STRONG DISCHARGE. batt current <-80A
//--------------------------------------------------------------------------------------------
			if(stRecentBattData.dblBatAmperes<-80.0) // starter surely draws more than that
			{
//check if microcontroller correctly specified battery state
				if(stRecentBattData.chBattState!='D')
				{
					Debug.WriteLine("bValuateBatteryData() ERR battery state mismatch - D");
				};

// watch for maximum current starter draws - in-rush starter motor current
				if(stRecentBattData.dblBatAmperes<m_sdblMaxBattDischrgCurrent)
				{
					m_sdblMaxBattDischrgCurrent=stRecentBattData.dblBatAmperes;
				}
				else
				{
//					Debug.WriteLine("bValuateBatteryData() Max discharging current=" + sdblMaxBattDischrgCurrent.ToString("+#.#;-#.#;0"));
				};

				if(true==m_bStarterMotorIsOn)
				{
// starter motor keeps running...
					Debug.WriteLine("bValuateBatteryData() starter mtr is RUNNING");
				}
				else
				{
// starter motor just started
					Debug.WriteLine("bValuateBatteryData() starter mtr is now ON !");
					m_bStarterMotorIsOn=true;
					stcBattDatStarterOn=stRecentBattData;
					stcBattDatStarterOff=stcBattPrevData;
// create new discharge-charge cycle
					stdschrgNewCycle.bIsCycleComplete=false;
					stdschrgNewCycle.bIsFullyRecharged=false;
					stdschrgNewCycle.dblMaxChrgCurrent=-99.0;
					stdschrgNewCycle.dblMaxDischrgCurrent=m_sdblMaxBattDischrgCurrent;
					stdschrgNewCycle.stbtdtmStarterOn=stRecentBattData;
					stdschrgNewCycle.stbtdtmFirstRecord=stbtdtmTempFirstRecord;

					if(null!=m_mapDischrgChrgCyles)
					{
// you can use .Add function to ass new starter motor event data
						try
						{
							m_mapDischrgChrgCyles.Add(stdschrgNewCycle.stbtdtmStarterOn.dtBattDateTime, stdschrgNewCycle);
						}
						catch (ArgumentException aexc)
						{
							Debug.WriteLine("bValuateBatteryData() battery already has this dis-chrg cycle on record, skip." + aexc.ToString());
						};
// also can use array notation 
//						m_mapDischrgChrgCyles[stdschrgNewCycle.stbtdtmStarterOn.dtBattDateTime]=stdschrgNewCycle;
					}; // end IF m_mapDischrgChrgCyles is not Null

				}; // end IF true==m_bStarterMotorIsOn
			}
// strong discharge case
//-------------------------------------------------------------------------------------------
// BATTERY DISCHARGE -80A < batt current < -2A
//--------------------------------------------------------------------------------------------
			else if(stRecentBattData.dblBatAmperes>=-80.0 && stRecentBattData.dblBatAmperes<-2.0)
			{
//				Debug.WriteLine("bValuateBatteryData() battery discharging");
//check if microcontroller correctly specified battery state
				if(stRecentBattData.chBattState!='D')
				{
					Debug.WriteLine("bValuateBatteryData() ERR battery state mismatch - D");
				};
// when discharge current is not that big, i.e. not a starter mtr case
// battery discharging case
//-------------------------------------------------------------------------------------------
// BATTERY DISCHARGE -40A < batt current <-0.2A
//--------------------------------------------------------------------------------------------
				if(true==m_bStarterMotorIsOn && stRecentBattData.dblBatAmperes>-40.0 && stRecentBattData.dblBatAmperes<-0.2)
				{
					Debug.WriteLine("bValuateBatteryData() starter mtr now OFF.");
// starter motor not running now					
					m_bStarterMotorIsOn=false;
					stcBattDatStarterOffAfterOn=stRecentBattData;
					stcBattDatCurrentCrossZero=stRecentBattData;
					tmspnStarterMotorRun=stcBattDatStarterOffAfterOn.dtBattDateTime - stcBattDatStarterOn.dtBattDateTime;
// uncommenting line below causes exception in line reader
					Debug.WriteLine("bValuateBatteryData() starter mtr duration="+tmspnStarterMotorRun.Seconds.ToString() +"."+tmspnStarterMotorRun.Milliseconds.ToString("000") + " sec"); 

// compute charge released for engine starting
					double dblTempmQ=(stcBattDatStarterOffAfterOn.liQOut-stcBattDatStarterOn.liQOut);

					Debug.WriteLine("bValuateBatteryData() Battery released " + (dblTempmQ/1000).ToString() +  " Q to start engine");

					if((stcBattDatStarterOff.dblBatAmperes-stcBattDatStarterOn.dblBatAmperes)!=0.0)
					{
// compute battery internal resistance and related parameters
						m_dblBatteryResistance=(stcBattDatStarterOff.dblBatVolts-stcBattDatStarterOn.dblBatVolts)/
									(stcBattDatStarterOff.dblBatAmperes-stcBattDatStarterOn.dblBatAmperes);
						Debug.WriteLine("bValuateBatteryData() batt R="+m_dblBatteryResistance.ToString("0.#000")+ " Ohm");
// estimate cranking amperes based on voltage-current 

// Hot cranking amperes (HCA) is the amount of current a battery can provide at 80°F (26.7°C). 
//	The rating is defined as the current a lead-acid battery at that temperature can deliver for 30 seconds and maintain at least 1.2 V/cell (7.2 volts for a 12-volt battery). 
// Cranking amperes (CA), also sometimes referred to as marine cranking amperes (MCA), is the amount of current a battery can provide at 32°F (0°C). 
//	The rating is defined as the number of amperes a lead-acid battery at that temperature can deliver for 30 seconds and maintain at least 1.2 V/cell (7.2 volts for a 12 volt battery). 
// Cold cranking amperes (CCA) is the amount of current a battery can provide at 0°F (−18°C). 
//	The rating is defined as the current a lead-acid battery at that temperature can deliver for 30 seconds and maintain at least 1.2 volts per cell (7.2 volts for a 12-volt battery)

						if(m_dblBatteryResistance!=0.0)
						{
// example
// key on 12.1V,-6.4A; starter on 10.7V, -104A; R=(12.1-10.7)/(104-6)=1.4V/97.6A=0.014 Ohm
// knowing R predict CA by computation CA=(12.1-7.2)/R
							m_dblCA=(stcBattDatStarterOff.dblBatVolts-7.2)/m_dblBatteryResistance;
							Debug.WriteLine("bValuateBatteryData() batt CA="+m_dblCA.ToString("0000")+" Amperes");
						};
//
// determine whether it was actual engine starting cyle or maybe just a momentary drop in battry current
//
// decision made is based on high discharge currnet value and duration of it
// Must draw high discharge current for at least 0.5 sec to qualify as engine starting event
// AND battery should be discharging : state=D and voltage should be < 12.6 Volts
						if(tmspnStarterMotorRun.Seconds>=1 || 
								(tmspnStarterMotorRun.Seconds==0 && tmspnStarterMotorRun.Milliseconds>500) )
						{
							m_iEngineStartEvtCnt+=1;
// modify last incomplete discharge-charge cycle and update its data
							Debug.WriteLine("bValuateBatteryData() valid cycle duration " + tmspnStarterMotorRun.Seconds.ToString() +"."+tmspnStarterMotorRun.Milliseconds.ToString("000") + " sec detected. Completing dischrg-chrg cycle....");
// find our dis-chrg cycle in array and modify it to store starter off data
							stDischrgChrgCycle stdschrgCycleInWorks=m_mapDischrgChrgCyles[stcBattDatStarterOn.dtBattDateTime];
// if found, modify data in place
//
// we have now stdschrgCycleInWorks.stbtdtmStarterOn part filled only. 
// now populate stdschrgCycleInWorks.stbtdtmStarterOffAfterOn and other members
//					
							stdschrgCycleInWorks.bIsCycleComplete=true;
							stdschrgCycleInWorks.dblMaxChrgCurrent=-99.0;
							stdschrgCycleInWorks.dblBatteryResistance=m_dblBatteryResistance;
							stdschrgCycleInWorks.dblCA=m_dblCA;
							stdschrgCycleInWorks.dblMaxDischrgCurrent=m_sdblMaxBattDischrgCurrent;
							stdschrgCycleInWorks.stbtdtmStarterOffAfterOn=stRecentBattData;
							stdschrgCycleInWorks.tmspnStrMotorRunDur=tmspnStarterMotorRun;
							stdschrgCycleInWorks.dblCoulmbsToStartEngine=dblTempmQ/1000.0;
// save it into array
							m_mapDischrgChrgCyles[stcBattDatStarterOn.dtBattDateTime]=stdschrgCycleInWorks;
						}
						else
						{
// not a engine starting cycle! Just very short current drop, e.g. downspike
							Debug.WriteLine("bValuateBatteryData() Cycle duration too short: " + tmspnStarterMotorRun.Seconds.ToString() +"."+tmspnStarterMotorRun.Milliseconds.ToString("000") + " sec.");
							Debug.WriteLine("bValuateBatteryData() Not a starter mtr cycle. Removing incomplete dischrg-chrg cycle....");
// so we delete previous spurious dischr-chrg cycle from our list
							m_mapDischrgChrgCyles.Remove(stcBattDatStarterOn.dtBattDateTime);
						};

					}; // end IF of (stcBattDatStarterOff.dblBatAmperes-stcBattDatStarterOn.dblBatAmperes) !=0 Amp

// store time of this moment and watch for 15 minutes from it - battery should quckly charge, charge current decrease
				}; // end IF of true==m_bStarterMotorIsOn
			}
//
// when battery current current crosses zero Ampreres line
// this includes NO LOAD case as well - see inner check
			else if(stRecentBattData.dblBatAmperes>=-2.0 && stRecentBattData.dblBatAmperes<=+2.0) 
			{
//-------------------------------------------------------------------------------------------
// BATTERY NO LOAD -0.1A < batt current <+0.1A
//--------------------------------------------------------------------------------------------
				if(stRecentBattData.dblBatAmperes>=-0.1 && stRecentBattData.dblBatAmperes<=+0.1) 
				{
//					Debug.WriteLine("bValuateBatteryData() batt no load");
// may update SoC bars for NL case here
//check if microcontroller correctly specified battery state
					if(stRecentBattData.chBattState!='I')
					{
						Debug.WriteLine("bValuateBatteryData() ERR battery state mismatch - NL");
					};
				};
//
// check to determine if we are coming here just after engine starting cycle
// - to find out, we determine if an element is present with date-time of starter motor was switched on
				bool bHasStarterOnElem=m_mapDischrgChrgCyles.ContainsKey(stcBattDatStarterOn.dtBattDateTime);

				if(true==bHasStarterOnElem)
				{
// if we have at least one engine starter cycle, then we count form the last engine starter cycle - error happens here -- !
				stDischrgChrgCycle stdschrgCycleInWorks=m_mapDischrgChrgCyles[stcBattDatStarterOn.dtBattDateTime];

// and if average battery current is around zero, then we compute battery A*Hrs capacity based on charge accepted
// from the moment charging started (starter off) till now
					if(m_iEngineStartEvtCnt>0 && Math.Abs(m_dblAvrgCurrent100Smpls)<1.2 && false==stdschrgCycleInWorks.bIsFullyRecharged)
					{
						long liBatteryChargeAccepted=(stRecentBattData.liQIn-stcBattDatCurrentCrossZero.liQIn)/1000; 
						Debug.WriteLine("bValuateBatteryData() Battery is fully charged. Q in ="+liBatteryChargeAccepted.ToString()+" Q");
						Debug.WriteLine("bValuateBatteryData() Battery capacity est. "+((double)liBatteryChargeAccepted/3600.0).ToString("#0.0")+" A*Hrs"); 
//						workerObject.bLogDataToFile("Battery fully charged (based on ~0A average battery current");
						stdschrgCycleInWorks.bIsFullyRecharged=true;
// save time stamp when battery fully charged
						stdschrgCycleInWorks.stbtdtmWhenFullyCharged=stRecentBattData;
// find last discharge-charge cycle we used 
						m_mapDischrgChrgCyles[stcBattDatStarterOn.dtBattDateTime]=stdschrgCycleInWorks;
// update last discharge-charge cycle with new data
					}; // end IF true==bHasStarterOnEle
				}
				else
				{
					Debug.WriteLine("bValuateBatteryData() Cannot determine if Battery is fully charged, because starter ON event was not found "+stcBattDatStarterOn.dtBattDateTime.ToString("dd.HH:mm:ss.fff"));
				};
			}
// battery is now charging
//-------------------------------------------------------------------------------------------
// BATTERY CHARGING batt current > +0.2A
//--------------------------------------------------------------------------------------------
			else if(stRecentBattData.dblBatAmperes>+0.2) 
			{
//				Debug.WriteLine("bValuateBatteryData() battery charging");
				
				if(stRecentBattData.chBattState!='C')
				{
					Debug.WriteLine("bValuateBatteryData() ERR battery state mismatch - C");
				};
// watch for maximum charging current
				if(stRecentBattData.dblBatAmperes>m_sdblMaxBattChrgCurrent)
				{
					m_sdblMaxBattChrgCurrent=stRecentBattData.dblBatAmperes;
					stcBattDatMaxChargCurr=stRecentBattData;
				}
				else
				{
// once we get here the first time then battery charging current starts to drop,
// so note max charging current
//					Debug.WriteLine("bValuateBatteryData() Max charging current=" + sdblMaxBattChrgCurrent.ToString("+#.#;-#.#;0"));
				};
			}
			else
			{
// tbd
				Debug.WriteLine("bValuateBatteryData() TBD case");
			};
// just store momentary data for the next run
			stcBattPrevData=stRecentBattData;

//			Debug.WriteLine("--bValuateBatteryData()="+bRes.ToString());
			return bRes;
		} // end of bValuateBatteryData()

	} // end class AutomotiveBattery
}
