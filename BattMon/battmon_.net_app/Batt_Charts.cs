// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NextUI.Frame;
using NextUI.Component;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.IO.Ports; 
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;

namespace batt_mon_app
{
	public partial class Form1
	{
		private int m_iGasGaugeLevelCrhg=0;
		private int m_iGasGaugeLevelDscrhg=0;
		private int m_iGasGaugeLevelNoLoad=0;

// given thrre charging levels in percent, updates all three bars
		public int iUpdate3ChangePcntBars(int iKnownGasGaugeLevel_C, int iKnownGasGaugeLevel_NL, int iKnownGasGaugeLevel_D)
		{
// we have three(3) gas gauge bars - for %SoC when charging, at rest and when discharging

// update discharge SoC bar indicator
			if(iKnownGasGaugeLevel_D>=0 && iKnownGasGaugeLevel_D<=100)
			{
				m_iGasGaugeLevelDscrhg=iKnownGasGaugeLevel_D;
				bUpdateOneSoCBar(ChargeProgressBarD, m_iGasGaugeLevelDscrhg, 'D');
			};
// update no load SoC bar indicator
			if(iKnownGasGaugeLevel_NL>=0 && iKnownGasGaugeLevel_NL<=100)
			{
				m_iGasGaugeLevelNoLoad=iKnownGasGaugeLevel_NL;
				bUpdateOneSoCBar(ChargeProgressBarNL, m_iGasGaugeLevelNoLoad, ' ');
			};

// update Charge SoC bar indicator
			if(iKnownGasGaugeLevel_C>=0 && iKnownGasGaugeLevel_C<=100)
			{
				m_iGasGaugeLevelCrhg=iKnownGasGaugeLevel_C;
				bUpdateOneSoCBar(ChargeProgressBarC, m_iGasGaugeLevelCrhg, 'C');
			};
//
			return (m_iGasGaugeLevelDscrhg+m_iGasGaugeLevelNoLoad+m_iGasGaugeLevelCrhg)/3;
		}

// given charging level in percent, updates one of three bars
		public int iUpdateChangePcntBar(int iKnownGasGaugeLevel)
		{
// we have three(3) gas gauge bars - for %SoC when charging, at rest and when discharging

			switch (m_stRecentBattData.chBattState)
			{
// when battery is discharging
				case 'D':
				bUpdateOneSoCBar(ChargeProgressBarD, iKnownGasGaugeLevel, 'D');
				break;
// or when battery is neither charging nor discharging (within current indeterminate level, i.e. +- 0.2A)
// treated the same as for battery at rest
				case 'I':
				bUpdateOneSoCBar(ChargeProgressBarNL, iKnownGasGaugeLevel, ' ');
				break;

				case 'C':
// if we are called when battery is charging
				bUpdateOneSoCBar(ChargeProgressBarC, iKnownGasGaugeLevel, 'C');
				break;
// none of those
				default:
				break;
			};

			return iKnownGasGaugeLevel;
		}

// computes RGB color value based on charge percent
		private static Color clrGetColorFromChargePercent(int iPercnt)
		{
			Color clrReturnColorValue;
			int iblue=0;
			if(iPercnt<0)
				iPercnt=0;
			if(iPercnt>100)
				iPercnt=100;
			int ired=(255*(100-iPercnt))/100; // red 255 for 0%, 0 for 100%
			int igreen=(255*iPercnt)/100; // green 0 for 0%, 255 for 100%
			clrReturnColorValue=Color.FromArgb(ired, igreen, iblue);
//			Debug.WriteLine("Form1::clrGetColorFromChargePercent() SoC="+iPercnt+"%% => RGB("+ired+","+igreen+","+iblue+")");
			return clrReturnColorValue;
		}

// computes background RGB color value based on charge percent
		private static Color clrGetBgColorFromChargePercent(int iPercnt)
		{
			Color clrReturnColorValue;
			int iblue=128; // blue
			if(iPercnt<0)
				iPercnt=0;
			if(iPercnt>100)
				iPercnt=100;
			int ired=(128*(100-iPercnt))/100; // red 128 for 0%, 0 for 100%
			int igreen=(128*iPercnt)/100; // green 0 for 0%, 128 for 100%
			clrReturnColorValue=Color.FromArgb(127+ired, 127+igreen, iblue);
//			Debug.WriteLine("Form1::clrGetBgColorFromChargePercent() SoC="+iPercnt+"%% => RGB("+ired+","+igreen+","+iblue+")");
			return clrReturnColorValue;
		}

		private bool bUpdateOneSoCBar(ProgressBar prbrOneBar, int iPerc, char chState)
		{
			bool bRes=false;
			prbrOneBar.Value=iPerc;
// compute forgeround color
			prbrOneBar.ForeColor=clrGetColorFromChargePercent(iPerc);
// compute background color
//			prbrOneBar.BackColor=Color.Gray;
			prbrOneBar.BackColor=clrGetBgColorFromChargePercent(iPerc);

			string strTempPerc=iPerc.ToString() + "%";
			strTempPerc+=" " + chState;
			Debug.WriteLine("Form1::bUpdateOneSoCBar() "+strTempPerc);
			prbrOneBar.CreateGraphics().DrawString(strTempPerc, new Font("Arial", (float)8.25, FontStyle.Regular), Brushes.Black, new PointF(prbrOneBar.Width / 2 - 10, prbrOneBar.Height / 2 - 7));

			return bRes;
		}

// updates combined voltage and current chart series1 is Volts, series2 is Amperes
// uses microcontroller supplied date-time
// X axis fo chart must be DateTime data type
		private bool bAddDataTo_V_A_Chart(Chart chrtOneChart, Series sCurrent, Series sVoltage, Generic12Vbattery.strctBattMonData stBattDataPckt)
		{
			bool bRes=true;
//			Debug.WriteLine("++Form1::bAddDataTo_V_A_Chart()");

// obtain a reference to given data series
			if(null==sVoltage || null==sCurrent)
			{
				Debug.WriteLine("Form1::bAddDataTo_V_A_Chart() error - null series!");
				bRes=false;
			}
			else
			{
// now add {date-time, U,I and t°} as new data point, separately to current and voltage graphs
				sCurrent.Points.AddXY(stBattDataPckt.dtBattDateTime, stBattDataPckt.dblBatAmperes);
				sVoltage.Points.AddXY(stBattDataPckt.dtBattDateTime, stBattDataPckt.dblBatVolts);
			};
			return bRes;
		} // end of bAddDataTo_V_A_Chart()

		public bool bResizeXAxisAndZoom(Chart chrtOneChart, Series sCurrent)
		{
			bool bResizeResult=false;
			double dblXAxisDataSpan=0.0; 
			DateTime dtmRoundedDownXMin, dtmRoundedUpXMax;
			TimeSpan dtmXAxisTimeSpan;
//			Debug.WriteLine("++Form1::bResizeXAxisAndZoom()");

			if(null==sCurrent || sCurrent.Points.Count<2)
			{
//				Debug.WriteLine("--Form1::bResizeXAxisAndZoom(not enough data)=false");
				return bResizeResult;
			};
// set initally X axis range to entire X range
// set X view range to [xmin,xmax] first
			chrtOneChart.ChartAreas[0].AxisX.Minimum=sCurrent.Points[0].XValue;
			chrtOneChart.ChartAreas[0].AxisX.Maximum=sCurrent.Points[sCurrent.Points.Count-1].XValue;

//			Debug.WriteLine("bResizeXAxisAndZoom() X min="+ DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).ToString("mm:ss.fff") + ", X max=" + DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).ToString("mm:ss.fff"));
			dblXAxisDataSpan=chrtOneChart.ChartAreas[0].AxisX.Maximum - chrtOneChart.ChartAreas[0].AxisX.Minimum;

// compute time span for our data range xmax-xmin
			dtmXAxisTimeSpan=DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum) - DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum);				
//			Debug.WriteLine("bResizeXAxisAndZoom() X time span="+dtmXAxisTimeSpan.ToString());

			if(DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Millisecond < 500)
			{
				dtmRoundedDownXMin=new DateTime(DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Year,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Month,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Day,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Hour,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Minute,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Second, 0);
			}
			else
			{
				dtmRoundedDownXMin=new DateTime(DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Year,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Month,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Day,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Hour,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Minute,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Minimum).Second, 500);
			};
//			Debug.WriteLine("bResizeXAxisAndZoom() X axis min rounded " + dtmRoundedDownXMin.ToString("mm:ss.ff"));

			if(DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Millisecond <= 500)
			{
				dtmRoundedUpXMax=new DateTime(DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Year,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Month,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Day,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Hour,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Minute,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Second, 500);
			}
			else
			{
				if(DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Second<59)
				{
					dtmRoundedUpXMax=new DateTime(DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Year,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Month,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Day,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Hour,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Minute,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Second+1, 0);
				}
				else
				{
					dtmRoundedUpXMax=new DateTime(DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Year,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Month,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Day,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Hour,
												DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).Minute+1, 0, 0);
				};
			};
//			Debug.WriteLine("bResizeXAxisAndZoom() X axis max rounded " + dtmRoundedUpXMax.ToString("mm:ss.ff"));

// reset X axis min and max if needed
// and scroll to the most recent data on the right


// if data span is less than 10 sec, then don't zoom, just use fixed range [min, min+6 sec] instead
			if(dtmXAxisTimeSpan <= tspn6Sec)// 6 seconds
			{
				chrtOneChart.ChartAreas[0].AxisX.Minimum=dtmRoundedDownXMin.ToOADate();
				chrtOneChart.ChartAreas[0].AxisX.Maximum=dtmRoundedDownXMin.ToOADate() + dtm6sec.ToOADate();
// set major ticks in 0.5 sec
// set number of major ticks to 6/0.5=12
//				Debug.WriteLine("bResizeXAxisAndZoom(X span <=6 s) X axis - no zoom ");
			}
// data spans more than 10 sec, then leave X range as is, but zoom to the end [max-6sec, max] instead
			else
			{
// data span is more than 6 sec, so set right margin to last value in X axis (e.g. [max-6 sec,max])
// do rounding up or not
				bResizeResult=true;
// set zoom area [XAxis_max-6 sec, XAxis_max]
				chrtOneChart.ChartAreas[0].AxisX.ScaleView.Zoom(chrtOneChart.ChartAreas[0].AxisX.Maximum-dtm6sec.ToOADate(), chrtOneChart.ChartAreas[0].AxisX.Maximum);
//				Debug.WriteLine("bResizeXAxisAndZoom(X span > 6 s) X axis zoom [" + DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum-dtm6sec.ToOADate()).ToString("mm:ss.fff") + "," + DateTime.FromOADate(chrtOneChart.ChartAreas[0].AxisX.Maximum).ToString("mm:ss.fff") + "]");
			};

//			Debug.WriteLine("--Form1::bResizeXAxisAndZoom()="+bResizeResult.ToString());
			return bResizeResult;
		} // end of bResizeXAxisAndZoom()

	} // end of class Form1
}
