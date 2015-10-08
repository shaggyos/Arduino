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
		static bool sbIsStrterMtrOn=false;
		TimeSpan tspnMaxQueueEmptyingDuration; //=TimeSpan(default); 

// perform actions needed when we start monitoring battery parameters
		public bool bOnStartMonitoring()
		{
			bool bResult=false;
			Debug.WriteLine("++Form1::bOnStartMonitoring()");
			m_lSerPortErrCnt=0;
			m_slMeasIterCnt=0;
// do not clear the chart - let it stay
//			CurrentVoltageChart.Series.Clear();
			vResetVoltCurrNFilters();

// if stopped, change name to Stop
			StartStopButton.Text="Stop";
// disable entry to serial port
			SerPortNameTextBox.Enabled=false;
			bIsRunning=true;
// assign radio button selections
			LiveDataRadioButton1.Checked=true;
			FromFileRadioButton1.Checked=false;

			LiveDataRadioButton1.Enabled=false;
			FromFileRadioButton1.Enabled=false;

// start worker thread to communicate with our form, which will talk to microcontroller device
// Create the thread object. This does not start the thread.
            workerObject = new Ser_Port_Listener(this, m_cBattMonSettings.strCOMPortName);
// check results in bSerPortOkay
            workerThread = new Thread(workerObject.DoWork);
// Start the worker thread  manually
            workerThread.Start(); // will enter workerThread.DoWork() function
// Loop until worker thread activates. 
            while(false==workerThread.IsAlive)
            {
				Debug.WriteLine("Form1::bOnStartMonitoring() waiting for serial port worker thread to speed up...");
            };
// let worker thread connect to serial port , give it a little bit of time
			System.Threading.Thread.Sleep(150);

			if(true==workerObject.bSerPortOkay)
			{
				m_DataFastRefreshTimer.Start();
				m_UIRefreshTimer.Start();
				tspnMaxQueueEmptyingDuration=new TimeSpan(0,0,0, ((80 * m_UIRefreshTimer.Interval)/100) ); 
			};
			this.Text="Battery Monitor - Live data from " + SerPortNameTextBox.Text;

			Debug.WriteLine("--Form1::bOnStartMonitoring()="+bResult.ToString());
			return bResult;
		} // end of bOnStartMonitoring()

// perform actions to stop monitoring battery parameters
		public bool bOnStopMonitoring(bool bCloseLogFile)
		{
			bool bResult=false;
			Debug.WriteLine("++Form1::bOnStopMonitoring()");
			m_lSerPortErrCnt=0;

// toggle button - if running, change name to Stop
			StartStopButton.Text="Start";
			LiveDataRadioButton1.Enabled=true;
			FromFileRadioButton1.Enabled=true;
// enable entry to serial port name
			SerPortNameTextBox.Enabled=true;
			bIsRunning=false;
//
			if(null!=workerObject)
			{
// we are exiting, tell worker thread to quit
				workerObject.RequestStop();
// Use the Join method to block the current thread  
// until the object's thread terminates.
				workerThread.Join();
//				workerObject.Dispose();
				workerObject=null;
			};
// serial port no longer supplies data to us, stop chart animation
			if(null!=m_UIRefreshTimer)
			{
				m_UIRefreshTimer.Stop();
				m_DataFastRefreshTimer.Stop();
			};

			vResetVoltCurrNFilters();
// show inactive state voltage
			bDisplayVoltage(0.0, false);
			((NumericalFrame)(this.DigitalVoltmBaseUI.Frame[0])).Indicator.DisplayValue="---";
// same for current
			bDisplayCurrent(0.0, false);
			((NumericalFrame)(this.DigitalCurrentBaseUI.Frame[0])).Indicator.DisplayValue="----";
// temperature
			bDisplayTemperature(0.0);
			((NumericalFrame)(this.DigitalTempBaseUI.Frame[0])).Indicator.DisplayValue="---";
// charge level
			ChargeProgressBarNL.Text="--";
//			ChargeProgressBar1.Hide();

			ErrorCountTextBox.Text="";

// also close log file if so requested
			if(null!=m_OutCSVfile && true==bCloseLogFile)
			{
				m_OutCSVfile.Write("\r\n");
				m_OutCSVfile.Flush();
				m_OutCSVfile.Close();
				m_OutCSVfile.Dispose();
				m_OutCSVfile=null;
			};

			Debug.WriteLine("--Form1::bOnStopMonitoring()="+bResult.ToString());
			return bResult;
		} // end of bOnStopMonitoring()

// data-only refresh function - fast
		void data_refresh_fast_timer_Funkt(object sender, EventArgs e) // is called 100 times per sec
		{
//			Debug.WriteLine("++data_refresh_fast_timer_Funkt()");

//			Debug.WriteLine("--data_refresh_fast_timer_Funkt()");
			return;
		}

		int siSlowTmrIterCnt=0;
// UI rfresh function - slow rate, with multiple battery data removing from queue on single call
        void UI_slow_upd_timer(object sender, EventArgs e) // is called 10 times per sec
        {
			long liDequingCntr=0;
			int iFuelGaugeLvl=-1;
			int iFuelGaugeLvl_NL=-1;
			int iFuelGaugeLvl_D=-1;
			int iFuelGaugeLvl_C=-1;
			Generic12Vbattery.strctBattMonData stBattDataPacket;
			DateTime dtmStartOfTimerFunkt=DateTime.Now;
			TimeSpan tspnQueueEmptyingDuration;

//			Debug.WriteLine("++Form1::UI_slow_upd_timer()");

//	empty all elements from queue into charts			
//	then display the last value from queue into meters

			while(cnquBattDataQueue.Count>0)
// option : to avoid getting stuck here limit the number of dequeued elements - use while(quBattDataQueueSyncd.Count>0 && iCntDequeued<100)
			{
				tspnQueueEmptyingDuration=DateTime.Now - dtmStartOfTimerFunkt;
// NOTE: this function is called every 100 msec. Therefore its run duration shall not exceed 80 msec (80% of 100 msec)
// even if there is more battery data in queue
				if(tspnQueueEmptyingDuration >= tspnMaxQueueEmptyingDuration)
				{
					Debug.WriteLine("Form1::UI_slow_upd_timer() time slice " + tspnQueueEmptyingDuration.TotalMilliseconds.ToString() + " ms exceeded !");
					Debug.WriteLine("Form1::UI_slow_upd_timer() " + cnquBattDataQueue.Count.ToString() + " elements postponed till next call");
					break;
				};

				DataQueueProgressBar1.Value=cnquBattDataQueue.Count<100?cnquBattDataQueue.Count:100;

// dequeue one battery data packet
				while(false==cnquBattDataQueue.TryDequeue(out stBattDataPacket))
				{
// todo: if it takes too long, break
					liDequingCntr+=1; 
					if(0==liDequingCntr%50)
					{
						Debug.WriteLine("Form1::UI_slow_upd_timer() Batt Data Queue busy ....");
// we may need to pause serial port reader for some time to allow this form thread to empty a queue a bit
//						break;
					};
				};

				Debug.WriteLine("Form1::UI_slow_upd_timer(<-{" + stBattDataPacket.dblBatVolts.ToString("0.#;0") + "V," + stBattDataPacket.dblBatAmperes.ToString("+#.#;-#.#;0") + "A}; len=" + cnquBattDataQueue.Count.ToString() + ")");
// copy its data to charts collections
				m_stRecentBattData=stBattDataPacket;

				bAddBattDataToChart(stBattDataPacket);
//
				Debug.WriteLine("Form1::UI_slow_upd_timer() count="+cnquBattDataQueue.Count.ToString());
// uses m_stRecentBattData.V,I,degC to determine SoC 
				iFuelGaugeLvl=iGetBattLifePrc(); // may return -1 if C is not within SoC lookup table bounds
//
				switch (stBattDataPacket.chBattState)
				{
					case 'D':
					if(-1!=iFuelGaugeLvl)
						iFuelGaugeLvl_D=iFuelGaugeLvl;
					break;

					case 'I':
					if(-1!=iFuelGaugeLvl)
						iFuelGaugeLvl_NL=iFuelGaugeLvl;
					break;

					case 'C':
					if(-1!=iFuelGaugeLvl)
						iFuelGaugeLvl_C=iFuelGaugeLvl;
					break;

					default:
					Debug.WriteLine("Form1::UI_slow_upd_timer() Chrg% error! Unknown battery state " + stBattDataPacket.chBattState);
					break;
				};
				Debug.WriteLine("Form1::UI_slow_upd_timer() Chrg%="+iFuelGaugeLvl.ToString()+stBattDataPacket.chBattState);

// log data line to file
				bLogDataToCSVFile(m_stRecentBattData, iFuelGaugeLvl);	 

// update iteration count and error count
				IterCntTextBox.Text=m_slMeasIterCnt.ToString();

// lastly, here is the place through which every battery data packet goes 
// so we use this place to make some decisions and optionally initiate further actions

//				m_batBattery.bValuateBatteryData(m_stRecentBattData, -1); // -1 means do not check for time jumping backwards
				m_batBattery.bValuateBatteryData(m_stRecentBattData, siSlowTmrIterCnt); 
				siSlowTmrIterCnt+=1;

				if(sbIsStrterMtrOn!=m_batBattery.bIsStarterMtrOn())
				{
					if(true==m_batBattery.bIsStarterMtrOn())
					{
						bSetMotorPict(bmpStarterMtrOn);
					}
					else
					{
						bSetMotorPict(bmpStarterMtrOff);
// since it was off-on-off cycle, update R and CA text fields
						R_Ohm_TextBox1.Text="<="+m_batBattery.dblGetBattInternalResistance().ToString("0.000");
						CA_textBox2.Text=">="+m_batBattery.dblGetBattCA().ToString("#") + " A"; // at " + stcBattDatStarterOff.dblBattTemp.ToString("+#;-#;0")+ "°C";

// since we added new discharge-charge cycle in live mode, update list box with it
// todo
					};
					sbIsStrterMtrOn=m_batBattery.bIsStarterMtrOn();
				};
				vIncrementIterationCount();
			}; // end WHILE data queue is not empty

// once we emptied the data queue, re-size X axis zoom and scale
			bResizeXAxisAndZoom(CurrentVoltageChart, CurrentVoltageChart.Series[0]);

// Volts
			bDisplayVoltage(m_stRecentBattData.dblBatVolts, m_cBattMonSettings.bRippleFilterOn); // means apply noise filter for analog meter

// current
			bDisplayCurrent(m_stRecentBattData.dblBatAmperes, m_cBattMonSettings.bRippleFilterOn);

// temp
			bDisplayTemperature(m_stRecentBattData.dblBattTemp);

// update charging level indicator bars
			iUpdate3ChangePcntBars(iFuelGaugeLvl_C, iFuelGaugeLvl_NL, iFuelGaugeLvl_D);

			ErrorCountTextBox.Text=m_lSerPortErrCnt.ToString();

// once we displayed new values, set flag to false not to redraw controls again until new data show up
//          Debug.WriteLine("--Form1::UI_slow_upd_timer()");
            return;
        } // end of UI_slow_upd_timer()

		public int iGetBattLifePrc()
		{
			int iRetPerc=-1;

			iRetPerc=Generic12Vbattery.iGetBattLifePrc(m_stRecentBattData.dblBatVolts, m_stRecentBattData.dblBatAmperes, m_stRecentBattData.dblBattTemp);

			return iRetPerc;
		}

	} // end of class Form1
}
