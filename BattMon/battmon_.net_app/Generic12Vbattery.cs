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

namespace batt_mon_app
{
// this is abstract class for generic lead-acid 12V battery
// your specific battery class must derive from it

	[Serializable] public class Generic12Vbattery
	{
// public battery data structure, resembles what microcontroller measures every cycle for any battery
		[Serializable] public struct strctBattMonData
		{
			public DateTime dtBattDateTime;
			public double dblBatVolts;
			public double dblBatAmperes;
			public double dblBattTemp;
			public char chBattState;
			public long liQIn;
			public long liQOut;
		}
// one battery discharge-charge cycle
		[Serializable] public struct stDischrgChrgCycle
		{
			public strctBattMonData stbtdtmFirstRecord;
			public strctBattMonData stbtdtmStarterOn;
			public strctBattMonData stbtdtmStarterOffAfterOn;
			public strctBattMonData stbtdtmWhenFullyCharged;
			public strctBattMonData stbtdtmLastRecord;
			public TimeSpan tmspnStrMotorRunDur;
			public double dblMaxDischrgCurrent;
			public double dblMaxChrgCurrent;
			public double dblBatteryResistance;
			public double dblCA;
			public double dblCoulmbsToStartEngine;
			public bool bIsCycleComplete;
			public bool bIsFullyRecharged;
		}
// holds all dicharge-charge cycles information in a map similar to STL
		protected Dictionary<DateTime, stDischrgChrgCycle> m_mapDischrgChrgCyles;

// battery class member variables = constants
		protected const int ciNumCellsInbatt=6; // for Lead-Acid 6 cell
		protected const double cdblVoltsPerDegreeC_Coeff=0.0048 * ciNumCellsInbatt; // 4.8 mVolt/degC temperature coefficient, 0.0288 per 6-cell battery

// battery class member variables - regular
// lead-acid battery description and manufacturer parameters
//-----------------------------------------------
		protected string m_strBatteryName;
		protected string m_strBatteryMake;
		protected string m_strBatteryModel;
		protected string m_strBatterySerialNo;
		protected DateTime m_dtmBattDateOfManuf;
		protected int m_iBatteryCapacityAHrs;
		protected int m_iBatteryCA;
		protected int m_iBatteryCCA;
// ====================================================

// lead-acid battery properties, independent of specific model

// state of charge table lookup parameters
		private const int NUMSOCTBLCOLS=10;
		private const int NUMSOCTBLROWS=11;
// battery parameters
		private const double MHNDLEDVLT=11.60; // considered fully discharged battery at 20°C
// new battery capacity in mA*hrs
		private static int siNewBattCap_AHrs=65;
//		private int iNewBattCap_mAHrs=iNewBattCap_mAHrs*1000;

		protected static double[] cdblSOCVALS=new double[NUMSOCTBLCOLS]
// - - - - d i s c h a r g i n g - -  | no load | + + + + + + + + c h a r g i n g + + + 
//-C/3	-C/5	-C/10	-C/20	-C/100	at rest	+C/40	+C/20	+C/10	+C/5
{-0.333,-0.20,	-0.10,	-0.05,	-0.01,	0.0,	+0.025,	+0.05,	+0.1,	+0.2};

// table function to determine battery charge level from voltage and current
// accurate only at 20°C
// must correct battery voltage to 20°C prior to use

	protected static double[,] cdblSoCLookupTable = new double[NUMSOCTBLROWS,NUMSOCTBLCOLS] // declared by number of rows, then number of columns
{
// - - - - d i s c h a r g i n g - - - | no load | + + + + + + + + c h a r g i n g + + + 
//-C/3	-C/5	-C/10	-C/20	-C/100	at rest	+C/40	+C/20	+C/10	+C/5
{9.50,	10.20,	10.99,	11.46,	11.50,	11.60,	11.45,	11.46,	11.48,	11.90},	// for 0% at 20°C
{9.95,	10.60,	11.27,	11.60,	11.68,	11.70,	11.75,	12.08,	12.38,	12.60},	// 10%
{10.38,	10.91,	11.50,	11.85,	11.89,	11.90,	11.91,	12.25,	12.60,	12.75},	// for 20%
{10.72,	11.12,	11.68,	12.06,	12.08,	12.10,	12.55,	12.55,	12.80,	12.95}, // 30%
{10.88,	11.33,	11.88,	12.21,	12.24,	12.25,	12.70,	12.85,	12.85,	13.20}, // 40%
{11.15,	11.55,	12.0,	12.28,	12.30,	12.33,	12.80,	13.05,	13.20,	13.35}, // 50%
{11.35,	11.65,	12.11,	12.39,	12.40,	12.45,	12.90,	13.15,	13.30,	13.52}, // 60%
{11.50,	11.80,	12.25,	12.49,	12.50,	12.51,	12.95,	13.20,	12.40,	13.70}, // 70%
{11.60,	11.90,	12.35,	12.55,	12.57,	12.58,	13.00,	13.30,	13.65,	14.00}, // 80%
{11.65,	12.45,	12.50,	12.58,	12.59,	12.60,	13.15,	13.60,	14.10,	15.200}, // 90%
{11.7,	12.08,	12.55,	12.60,	12.62,	12.63,	13.50,	14.20,	15.20,	15.90} // for 100% at 20°C
};

// this abstract class hosts model- and make-independent properties,
// which exist in any lead-acid battery

// default ctor provided
		public Generic12Vbattery()
		{
			Debug.WriteLine("GenericBattery() ctor");
//			m_lstChrgDschrgCycles=new List<stDischrgChrgCycle>();
			m_mapDischrgChrgCyles=new Dictionary<DateTime,stDischrgChrgCycle>();
			m_mapDischrgChrgCyles.Clear();
		}

// function to return battery voltage corrected to 20°C
// All voltages are at 20 °C (68 °F), and must be adjusted −0.022 V/°C (−0.012 V/°F) for temperature changes 
// (negative temperature coefficient – lower voltage at higher temperature). 
		public static double dblTempCompensateBattVolt(double dblGivenBattVoltAtT, double dblBattTemDegC)
		{
				double dblBatteryVoltageAt20C=0.0;
				double dblVoltCorr=0.0;
//			Debug.WriteLine("++dblTempCompensateBattVolt(in Vbat=" + dblGivenBattVoltAtT.ToString("0#.#;0") + " at t°=" +dblBattTemDegC.ToString("+#.#;-#.#;0")+")");
/* A lead acid battery has a Negative temperature coefficient of approx 4.8 mV per degree C PER CELL.
 for 6 cell (12v) 0.28v per 10deg C, something like this
 -40C	10.8
-20C	11.4
  0C	12.0
 20C	12.6	baseline
 40C	13.2
 60C	13.7
 */
			if(dblGivenBattVoltAtT>80.0 || dblBattTemDegC<-45.0)
			{
// for out of range temperature we do not compute, give warning
				Debug.WriteLine("dblTempCompensateBattVolt(ERROR) Battery temperature is out of range!");
				dblBatteryVoltageAt20C=dblGivenBattVoltAtT;
			}
			else
			{
				dblVoltCorr=(20.0-dblBattTemDegC) * cdblVoltsPerDegreeC_Coeff ; 
				dblBatteryVoltageAt20C=dblGivenBattVoltAtT + dblVoltCorr;				
			};

//			Debug.WriteLine("--dblTempCompensateBattVolt(in Vbat=" + dblGivenBattVoltAtT.ToString("#.#;0") + " at t°=" +dblBattTemDegC.ToString("+#.#;-#.#;0")+")="  + dblBatteryVoltageAt20C.ToString("#.#;0") + "V@20°C");
			return dblBatteryVoltageAt20C;
		} // end of dblTempCompensateBattVolt(Vbatt, temp)

// more accurate function to determine SoC
		public static int iGetBattLifePrc(double dblBatVoltage, double dblBatCurr, double dblBatTempDegC)
		{
			int iBatLfPrc=-1;
			double dblBattVoltageat_20C=0.0;
			float fC=0.0F; 
			int iTblLeftCol=-1;	// start with leftmost for min
			int iTblRightCol=NUMSOCTBLCOLS; // and with rightmost for max

//			Debug.WriteLine("++iGetBattLifePrc("+dblBatVoltage.ToString("+0.0;-0.0;0")+"V, "+dblBatCurr.ToString("+0.#;-0.#;0")+"A, "+dblBatTempDegC.ToString("+0.#;-0.#;0")+"°C)");
// compute charge C-factor
			fC=(float)dblBatCurr/(float)siNewBattCap_AHrs;
			Debug.WriteLine("iGetBattLifePrc() C=" + fC.ToString("+0.0;-0.0;0"));

// step 1 - check if C factor is outside of our table range -0.333 to +0.2
// we can only use SoC table for limited range of C factors for now
			if(fC<cdblSOCVALS[0] || fC>cdblSOCVALS[NUMSOCTBLCOLS-1])
			{
				Debug.WriteLine("iGetBattLifePrc() C not in SoC table range, skipped.");
				goto DoneSoC;
			};

// step 2 - correct battery voltage to 20°C
			dblBattVoltageat_20C=dblTempCompensateBattVolt(dblBatVoltage, dblBatTempDegC);
//			Debug.WriteLine("iGetBattLifePrc() Voltage correction for t° : "+dblBatVoltage.ToString()+"=>"+dblBattVoltageat_20C.ToString());

// step 3 - compute two table rows our C factor is in between, based on given C value
// search for Left column
			for(int j=0; j<NUMSOCTBLCOLS; j++)
			{
				if(fC>=cdblSOCVALS[j])
				{
					iTblLeftCol=j;
				}
				else
					break;
			};
// check if left column was computed as valid. 
// if C is < -0.333 then we will not find any match
//			if(-1==iTblLeftCol)
//			{
//				Debug.WriteLine("iGetBattLifePrc() SoC table lookup - LEFT column not found!");
//				goto DoneSoC;
//			};

// search for Right column
			for(int k=NUMSOCTBLCOLS-1; k>=0; k--)
			{
				if(fC<=cdblSOCVALS[k])
				{
					iTblRightCol=k;
				}
				else
					break;
			};
// check if right column was computed as valid
// if C is > +0.2, then we will not fina any match
//			if(NUMSOCTBLCOLS==iTblRightCol)
//			{
//				Debug.WriteLine("iGetBattLifePrc() SoC table lookup - RIGHT column not found!");
//				goto DoneSoC;
//			};

//			Debug.WriteLine("iGetBattLifePrc() SoC table lookup : ["+iTblLeftCol.ToString() + "] < C < [" + iTblRightCol.ToString() + "]");

// step 4 

// walk vertically SoC table to determine most accurate value
// interpolate between cells 
			for(int m=0; m<NUMSOCTBLROWS; m++) 
			{
//				Debug.WriteLine("[" + m.ToString()+ "] "+ cdblSoCLookupTable[m,iTblLeftCol].ToString() +
//				 " < " + dblBattVoltageat_20C.ToString() + " < " + cdblSoCLookupTable[m,iTblRightCol].ToString());

				if(dblBattVoltageat_20C >= cdblSoCLookupTable[m,iTblLeftCol] &&  // row, col
					dblBattVoltageat_20C < cdblSoCLookupTable[m,iTblRightCol])
				{
// found matching pair - fist level answer, multiples of 10
					iBatLfPrc=m*10;
// second level answer - interpolate for SoC between cells
// tbd					
					break;
				};
			};

// check if we found battery charge state percentage in our search
// the search may fail if 
// 1) battery voltage is out of range, i.e. not a 12V lead-acid battery
// or 2) for no load state battery was recently charged, and its voltage did not drop to idle lead-acid voltage yet
// item 2) happens within several minutes after turning automotive engine off (generator turns off and stops charging)
// until idle battery voltage levels off to 12.6 or so for 100%
			if(-1==iBatLfPrc)
			{
				Debug.WriteLine("iGetBattLifePrc() WARN SoC table lookup [" + iTblLeftCol.ToString() +","+iTblRightCol.ToString()+"] failed.");
			};

DoneSoC:
//			Debug.WriteLine("--iGetBattLifePrc()=" + iBatLfPrc.ToString());
			return iBatLfPrc;
		} // end of int iGetBattLifePrc()

	} // end of class Generic12Vbattery
}
