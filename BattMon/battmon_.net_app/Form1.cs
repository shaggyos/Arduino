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
using batt_mon_app; // for AutomotiveBattery class access

namespace batt_mon_app
{
    public partial class Form1 : Form
    {
// declare member variables for our class
// Timer is defined in more than place, so Use the fully qualified name:
		private System.Windows.Forms.Timer m_UIRefreshTimer = null;	// 
		private System.Windows.Forms.Timer m_DataFastRefreshTimer = null;	// 

        private const int m_ciNumOfVoltmDigits = 3; // xx.x volts
        private const int m_ciNumOfAmpDigits = 4; // -xx.x amp
        private const int m_ciNumOfTemperDigits = 4; // -xx.x deg C
		private Generic12Vbattery.strctBattMonData m_stRecentBattData; // member variavbles for last known good U,I and t° etc. from battery
		static long m_slMeasIterCnt=0;
		public long m_lSerPortErrCnt=0;
		const int ciMaxNumOfDataPairsInSeries=60;
// chart view parameters
		const int ciZoomTimeSpanSize_inSec = 10; // size of X axis zoom, in seconds
		const int ciZoomTimeScrlSize_inSec = 1;	 // clicking laft or right arrows will slide chart 1.0 second
		const int ciTickMarkTimeSize_in_mSec = 500; // major X axis tickmarks are every 0.5 sec
		private TimeSpan tspn6Sec, tspnSmScrl, tspnTickmark;
		private DateTime dtm6sec, dtmSmlScrl, dtmTickmark;

		private Ser_Port_Listener workerObject=null;

// persistent structure to keep application settings between session
		private BattMonSettings m_cBattMonSettings;

		private String strPreviousOpenedFileDir="";
		private bool bIsRunning=false;
		private Thread workerThread=null;
// battery data packets queue
		public ConcurrentQueue<Generic12Vbattery.strctBattMonData> cnquBattDataQueue=null; // from System.Collections.Concurrent

// list of starter cycles - for listbox
		List<DateTime> m_lstDischrgChrgCycles=null;

// one instance of battery class
		private AutomotiveBattery m_batBattery=null;

        private static Bitmap bmpStarterMtrOn = null;
        private static Bitmap bmpStarterMtrOff = null;

        public Form1()
        {
            Debug.WriteLine("++Form1() ctor");

// make some time time-span conversions before we start out charts
			tspn6Sec=new TimeSpan(0,0,ciZoomTimeSpanSize_inSec);
			dtm6sec = new DateTime().Add(tspn6Sec);

			tspnSmScrl=new TimeSpan(0,0,ciZoomTimeScrlSize_inSec);
			dtmSmlScrl = new DateTime().Add(tspnSmScrl);

			tspnTickmark=new TimeSpan(0,0,0,ciTickMarkTimeSize_in_mSec);
			dtmTickmark = new DateTime().Add(tspnTickmark);
// perform container init
            InitializeComponent();
			this.Text = "Battery Monitor";

// initialize all our gauge controls in a single call
			bInitalizeGaugeControls();
//
// Creates and initializes a new concurrent Queue.
		   cnquBattDataQueue=new ConcurrentQueue<Generic12Vbattery.strctBattMonData>();
// restore application settings
			m_cBattMonSettings=BattMonSettings.bDeserAppSettings();
// assign these restored settings to controls
			SerPortNameTextBox.Text=m_cBattMonSettings.strCOMPortName;
			ripple_filterCheckBox1.Checked=m_cBattMonSettings.bRippleFilterOn;

			try
			{
// open battery logging file
				m_OutCSVfile=new System.IO.StreamWriter(szBatMonCSVFileName, true);
				bLoggingToFile=true;
			}
			catch(IOException ioex)
			{
				Debug.WriteLine("Form1() ctor: Error "+ ioex.ToString() +"! Cannot open log file");
				m_OutCSVfile=null;
				bLoggingToFile=false;
			};
//
// now create new battery class instance or restore from saved
			m_batBattery=AutomotiveBattery.bDeserBattCls();			
//
			if(null==m_batBattery)
			{
				Debug.WriteLine("Form1() ctor: do not have saved instance of myAutoBattery, create new !");
// if there is no saved instance of myAutoBattery, then create a new instance
// show new battery intro message first

// first open batt info form to fill up
				 BatteryInfoForm btinffrmNewbatteryInfo=new  BatteryInfoForm();

// and wait for [Save] button to be pressed before we continue
				Application.Run(btinffrmNewbatteryInfo); 

// now take data from form into our member variables

// now construct it anew using supplied parameters
				m_batBattery=new AutomotiveBattery(btinffrmNewbatteryInfo.strGetBatteryName(),
				btinffrmNewbatteryInfo.strGetBatteryMake(),  btinffrmNewbatteryInfo.strGetbatteryModel(),
				btinffrmNewbatteryInfo.strGetBatterySerNo(), 
				btinffrmNewbatteryInfo.iGetBatteryCA(), btinffrmNewbatteryInfo.iGetbatteryCCA(),
				btinffrmNewbatteryInfo.iGetBatteryCapacityAHrs(), 
				btinffrmNewbatteryInfo.dtmGetbattDateOfManufacture());
				Debug.WriteLine("Form1() ctor: created new instance of myAutoBattery(\""+ btinffrmNewbatteryInfo.strGetBatteryName() + "\")");
				m_lstDischrgChrgCycles=new List<DateTime>(0);
			}
			else
			{
				Debug.WriteLine("Form1() ctor: Restored myAutoBattery from saved.");
// print some info about restored class instance
				Debug.WriteLine("Form1() ctor: max charge current="+ m_batBattery.getMaxDischrgCurrentA().ToString() + " A");
				Debug.WriteLine("Form1() ctor: starting cycles="+ m_batBattery.iGetNumStartingCycles().ToString());
//
				int iInitialNumOfDchrgChrgCycles=m_batBattery.iGetNumStartingCycles();
// initialize listbox view - first create a list of items
				m_lstDischrgChrgCycles=new List<DateTime>(iInitialNumOfDchrgChrgCycles);

				bUpdateListBoxAndCntFromMmapDschrgChrgCycles();
			};

// stop monitoring and display stopped state
			bOnStopMonitoring(false);
			LiveDataRadioButton1.Checked=true; // changing value of checked will call OnRadioBtnChanged()
			FromFileRadioButton1.Checked=false;
// UI update timer function set up
            m_UIRefreshTimer = new System.Windows.Forms.Timer();
            m_UIRefreshTimer.Interval=100; // redraw controls every 100 msec, i.e 10 Hz
            m_UIRefreshTimer.Tick += new EventHandler(UI_slow_upd_timer);

// fast data update timer set up
            m_DataFastRefreshTimer = new System.Windows.Forms.Timer();
            m_DataFastRefreshTimer.Interval=10; // redraw controls every 10 msec, i.e 100 Hz
#if USEFASTANDSLOWTIMERS
			m_DataFastRefreshTimer.Tick += new EventHandler(UI_slow_upd_timer_no_dequeue);
#else
            m_DataFastRefreshTimer.Tick += new EventHandler(data_refresh_fast_timer_Funkt);
#endif
            Debug.WriteLine("Form1() ctor: analog meters timer is up; rate="+(1000/m_UIRefreshTimer.Interval).ToString() + " Hz");
//
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Stream StarterOff_bmp_Stream = myAssembly.GetManifestResourceStream("batt_mon_app.starter_motor_off.jpg");
            Stream StarterOn_bmp_Stream = myAssembly.GetManifestResourceStream("batt_mon_app.starter_motor_on.jpg");
			bmpStarterMtrOff = new Bitmap(StarterOff_bmp_Stream);
			bmpStarterMtrOn = new Bitmap(StarterOn_bmp_Stream);

			bSetMotorPict(bmpStarterMtrOff);
			m_slMeasIterCnt=0;
// ctor will perform autostart - it will try to start serial port worker right away
// which may fail so it may display a message
//			bOnStartMonitoring();
// if autostart here fails, then user may reconfigure serial port and start manually
            Debug.WriteLine("--Form1() ctor");
        } // end Form1() ctor

// param name="disposing">true if managed resources should be disposed; otherwise, false.
        protected override void Dispose(bool disposing)
        {
			bool bCompletedSerializ=false;
			Debug.WriteLine("++Form1() ~dtor");
// save application settings
			m_cBattMonSettings.bSerAppSettings();
// save battery class instance to persistance
			bCompletedSerializ=m_batBattery.bSerBattCls();

// debug only - notify user if serialization failed
			if(false==bCompletedSerializ)
			{
				Debug.WriteLine("Form1() ~dtor ERROR! Failed saving my battery class to storage.");
// probably add message box as well - todo
			}
			else
			{
				Debug.WriteLine("Form1() ~dtor Saving my battery class to storage ok.");
			};

            if(true==disposing && (null!=components))
            {
                components.Dispose();
            };
            base.Dispose(disposing);
			Debug.WriteLine("--Form1() ~dtor");
			return;
        }

        private bool bInitalizeGaugeControls()
        {
            bool bRes=false;
            Debug.WriteLine("++Form1::bInitalizeGaugeControls()");

			bInitVoltmeter(); // option : bInitVoltmeter(fltMinVolts, fltmaxVolts, int iVNumLargeTicks)

			bInitAmpereMeter(); // option bInitAmpereMeter(fltMinAmps, fltMaxAmps, int iANumLargeTicks)

			bInitThermometer(); // option bInitThermometer(fltDegC, fltMaxDegC, int iTNumLargeTicks)

            Debug.WriteLine("--Form1::bInitalizeGaugeControls()=" + bRes.ToString());
            return bRes;
        } // end of bInitalizeGaugeControls()

		private void SerPortNameTextBox_TextChanged(object sender, EventArgs e)
		{
// get new serial port name
			m_cBattMonSettings.strCOMPortName=SerPortNameTextBox.Text;
			return;
		}

		private void RippleFilterOn_toggled(object sender, EventArgs e)
		{
// get new serial port name
			m_cBattMonSettings.bRippleFilterOn=ripple_filterCheckBox1.Checked;
			Debug.WriteLine("--Form1::RippleFilterOn_toggled()=" + m_cBattMonSettings.bRippleFilterOn.ToString());
			return;
		}

		private void StartStopButton_Click(object sender,EventArgs e)
		{
			if(true==bIsRunning)
			{
				this.Text="Battery Monitor - Live data from " + SerPortNameTextBox.Text;
				bOnStopMonitoring(false);
				this.Text+=" [Stopped]";
			}
			else
			{
				bOnStartMonitoring();
				this.Text+=" {Running}";
			};
		}

		private void LiveDataRadioButton1_CheckedChanged(object sender,EventArgs e)
		{
			Debug.WriteLine("++Form1::OnLiveDataRadioButton1()");
// this is the deafult
// if selected
			if(true==LiveDataRadioButton1.Checked)
			{
// if running - do nothing
				if(false==bIsRunning)
				{
// clear charts	- erase data lest there after reading from file			
// and launch on start functions
//					bOnStartMonitoring();
// enable start button
					StartStopButton.Enabled=true;
					SerPortNameTextBox.Enabled=true;
					this.Text="Battery Monitor - Live data from " + SerPortNameTextBox.Text;
				};
			};
			Debug.WriteLine("--Form1::OnLiveDataRadioButton1()");
		}
//
		private void FromFileRadioButton1_CheckedChanged(object sender,EventArgs e)
		{
			Debug.WriteLine("++Form1::OnFromFileRadioButton1()");
			int iFromRecordedFuelGaugeLvl=-1;
			int iFuelGaugeLvl_D=-1;
			int iFuelGaugeLvl_NL=-1;
			int iFuelGaugeLvl_C=-1;
// if selected
			if(true==FromFileRadioButton1.Checked)
			{
				if(true==bIsRunning)
				{
// stop if running
					bOnStopMonitoring(true);
// disable start button
				};
				StartStopButton.Enabled=false;
				SerPortNameTextBox.Enabled=false;
				this.Text="Battery Monitor - Recorded data; file ";
// clear chart
//				CurrentVoltageChart.Series.Clear();
// clear data stored in voltage and current series
				CurrentVoltageChart.Series[0].Points.Clear();
				CurrentVoltageChart.Series[1].Points.Clear();
// then restore zoom window view of X axis

// load data from file into chart
				StreamReader strmReader=null;
				OpenFileDialog openFileDialog1 = new OpenFileDialog();
				if(strPreviousOpenedFileDir.Length!=0)
				{
					openFileDialog1.InitialDirectory=strPreviousOpenedFileDir; // path as in "c:\\" ;
					openFileDialog1.RestoreDirectory=true ;
				};

// open CFile dialog and navigate to batt_mon log file on PC
				openFileDialog1.Filter = "Batt mon log files (*.log)|*.log|All files (*.*)|*.*" ;
				openFileDialog1.FilterIndex = 1 ;
				openFileDialog1.RestoreDirectory = true ;
// OR read from SD card from arduino microcontroller	
				if(openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					Debug.WriteLine("Form1::OnFromFileRadioButton1() CFile dialog : file selected="+openFileDialog1.FileName.ToString());
					try
					{
						strmReader = new StreamReader(openFileDialog1.FileName);

						if(null!=strmReader)
						{
							batt_data_parser prsDataparser=new batt_data_parser();
							Debug.WriteLine("Form1::OnFromFileRadioButton1() Opened file "+openFileDialog1.FileName.ToString() +" ok.");

							this.Text+=openFileDialog1.SafeFileName;
// save folder name of successfully opened file
							strPreviousOpenedFileDir=System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
							using(strmReader)
							{
// store the current battery information for later comparison
								int iCurrNumOfDischChrgCycles=m_batBattery.iGetNumStartingCycles();

								String strOneDatapacketFromFile=null;
								int iLineCntr=0;
								int iErrorCntFromFile=0;
								ErrorCountTextBox.Text=" ";
// start busy cursor
//								Cursor.Current = Cursors.WaitCursor;
								UseWaitCursor = true;
// clear out SoC bar indicators
								iUpdate3ChangePcntBars(0, 0, 0);
								ChargeProgressBarD.BackColor=Color.Gray;
								ChargeProgressBarNL.BackColor=Color.Gray;
								ChargeProgressBarC.BackColor=Color.Gray;
// code to read the stream here.
				                while(strmReader.Peek() >= 0) 
								{
									strOneDatapacketFromFile=strmReader.ReadLine().ToString();

									if(strOneDatapacketFromFile.Length>0)
									{
//										Debug.WriteLine("Form1::OnFromFileRadioButton1() Line: ["+iLineCntr.ToString() + "] "  +strOneDatapacketFromFile);
// parse one data packet into fields
										if(true == prsDataparser.bParseOneDataLine(strOneDatapacketFromFile))
										{
// if fields are okay, then feed chart series with it
											m_stRecentBattData=prsDataparser.stbtGetParsedData();

											bAddBattDataToChart(m_stRecentBattData);
											
// be sure we process starter mtr load case if there is one
											m_batBattery.bValuateBatteryData(m_stRecentBattData, iLineCntr);
// and then update text fields for R and CA accordingly

											iLineCntr+=1;
											if(iLineCntr%100==0)
											{
												Debug.WriteLine("Form1::OnFromFileRadioButton1() Added " + iLineCntr.ToString() + " Data packets");
//												Debug.WriteLine("Form1::OnFromFileRadioButton1() Batt average current="+m_batBattery.getLastAveCurrent().ToString("+0.0;-0.0;0")+" A");
											};
// update counts live on the form
											m_slMeasIterCnt+=1;
											IterCntTextBox.Text=m_slMeasIterCnt.ToString();
// determine SoC level and battery state
											iFromRecordedFuelGaugeLvl=iGetBattLifePrc();
// update SoC bar indicators accordingly
											switch (m_stRecentBattData.chBattState)
											{
												case 'D':
												if(-1!=iFromRecordedFuelGaugeLvl)
													iFuelGaugeLvl_D=iFromRecordedFuelGaugeLvl;
												break;

												case 'I':
												if(-1!=iFromRecordedFuelGaugeLvl)
													iFuelGaugeLvl_NL=iFromRecordedFuelGaugeLvl;
												break;

												case 'C':
												if(-1!=iFromRecordedFuelGaugeLvl)
													iFuelGaugeLvl_C=iFromRecordedFuelGaugeLvl;
												break;

												default:
												Debug.WriteLine("Form1::OnFromFileRadioButton1() Chrg% error! Unknown battery state " + m_stRecentBattData.chBattState);
												break;
											};

											if(iLineCntr%10==0)
											{
												iUpdate3ChangePcntBars(iFuelGaugeLvl_C, iFuelGaugeLvl_NL, iFuelGaugeLvl_D);
												System.Threading.Thread.Sleep(10);
											};
										}
										else
										{
// data bad, ignore
											iErrorCntFromFile+=1;
											Debug.WriteLine("Form1::OnFromFileRadioButton1() Failed to parse Line: ["+iLineCntr.ToString() + "] "  +strOneDatapacketFromFile);
											ErrorCountTextBox.Text=iErrorCntFromFile.ToString();
// count errors
										};
									}; // end IF of if(strOneDatapacketFromFile.Length
// Release build only
// add some responsiveness 
									Application.DoEvents();
								}; // end of while(strmReader.Peek())
// end busy cursor
//								Cursor.Current = Cursors.Default;
								UseWaitCursor = false;

// we finished adding data from file. We may get new discharge-charge cycles added, so
								Debug.WriteLine("Form1::OnFromFileRadioButton1() Finished reading batery data from "+ openFileDialog1.FileName.ToString() );
								Debug.WriteLine("Form1::OnFromFileRadioButton1() Processed " + iLineCntr.ToString() + " lines of data.");
								Debug.WriteLine("Form1::OnFromFileRadioButton1() Found " + iErrorCntFromFile.ToString() + " errors.");
								string strTemp=null;

								strTemp="Finished reading batery data from "+ openFileDialog1.FileName.ToString()+ "\r\n";
								strTemp+="Processed " + iLineCntr.ToString() + " lines of data.\r\n";
								strTemp+="Found " + iErrorCntFromFile.ToString() + " errors.\r\n";

								int iNumOfNewDchrgChrgCyles=m_batBattery.iGetNumStartingCycles()-iCurrNumOfDischChrgCycles;
// tell user what kind of information was added to our battery class instance
								if(iNumOfNewDchrgChrgCyles>0)
								{
									Debug.WriteLine("Form1::OnFromFileRadioButton1() Added " + iNumOfNewDchrgChrgCyles.ToString() + " new discharge-charge cycles.");									
									strTemp+="Added " + iNumOfNewDchrgChrgCyles.ToString() + " new discharge-charge cycles.";
								}
								else
								{
									Debug.WriteLine("Form1::OnFromFileRadioButton1() Did not find any new discharge-charge cycles.");
									strTemp+=" Did not find any new discharge-charge cycles.";
								};
								MessageBox.Show(strTemp);

// update listbox from discharge-charge cycles array
								bUpdateListBoxAndCntFromMmapDschrgChrgCycles();

// once all data are added, resize X axis zoom and scale
								bResizeXAxisAndZoom(CurrentVoltageChart, CurrentVoltageChart.Series[0]);
// and also update R and CA if there was any starting cycle detected
								if(true==m_batBattery.bWasEngineStarted())
								{
									R_Ohm_TextBox1.Text="<="+m_batBattery.dblGetBattInternalResistance().ToString("0.000");
									CA_textBox2.Text=">="+m_batBattery.dblGetBattCA().ToString("#") + " A"; // at " + stcBattDatStarterOff.dblBattTemp.ToString("+#;-#;0")+ "°C";
								};
//								Debug.WriteLine("Form1::OnFromFileRadioButton1() Batt Current ave="+m_batBattery.getLastAveCurrent().ToString("+#.#;-#.#;0"));
							}; // end of using(strmReader)
						}
						else
						{
							Debug.WriteLine("Form1::OnFromFileRadioButton1() FAILED to open file "+openFileDialog1.FileName.ToString() +" !");
						};
					}
					catch (Exception ex)
					{
						MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
					};
				}
				else
				{
					Debug.WriteLine("Form1::OnFromFileRadioButton1() CFile dialog CANCELLed");
				};
			};
			Debug.WriteLine("--Form1::OnFromFileRadioButton1()");
		} // end of FromFileRadioButton1_CheckedChanged()

// sets the image to specified LED image
        private bool bSetMotorPict(Image imgPict)
		{
			bool bRes=false;
			if(imgPict!=null)
			{
// Set the PictureBox image property to this image.
				pictureBox1.Image = imgPict;
// ... Then, adjust its height and width properties.
				pictureBox1.Height = imgPict.Height;
				pictureBox1.Width = imgPict.Width;
				bRes=true;
			};
			return bRes;
		}

		private void DischrgChrgClcListBox1_SelectedIndexChanged(object sender,EventArgs e)
		{
			int iSelIdx = DischrgChrgClcListBox1.SelectedIndex;
			bUpdateDichrgChrgCycleTableFields(iSelIdx);
		}

		private bool bUpdateDichrgChrgCycleTableFields(int iSelectedFromList)
		{
			bool bRet=false;
			AutomotiveBattery.stDischrgChrgCycle stdchchrgOneCycle=default(AutomotiveBattery.stDischrgChrgCycle);
			
// check for an error
			if(iSelectedFromList < m_batBattery.iGetNumStartingCycles()) 
			{
// get selected discharge-charge cycle structure
				stdchchrgOneCycle=m_batBattery.dctGetBatChrgCyclByIdx(iSelectedFromList);
// now update individual fileds

// battery charge in : Q_in(when fully charged OR when ended recording) - Q_in(when starter off)
				ChrgInTextBox14.Text=((stdchchrgOneCycle.stbtdtmWhenFullyCharged.liQIn)/1000.0).ToString("0000");

				long liTemp=stdchchrgOneCycle.stbtdtmWhenFullyCharged.liQIn-stdchchrgOneCycle.stbtdtmStarterOn.liQIn;

// battery charge out: Q_out(when fully charged OR when ended recording) - Q_out(when started recording OR when starter on) 
				ChrgOutTextBox13.Text=((stdchchrgOneCycle.stbtdtmStarterOffAfterOn.liQOut)/1000.0).ToString("0000");
				long liTemp2= (stdchchrgOneCycle.stbtdtmWhenFullyCharged.liQOut - stdchchrgOneCycle.stbtdtmStarterOn.liQOut)/1000;

// computed cranking Amperes
				ComputedCATextBox11.Text=stdchchrgOneCycle.dblCA.ToString("0000");

// computed battery resistance
				BattResOhmsTextBox10.Text=stdchchrgOneCycle.dblBatteryResistance.ToString("0.#00");

// maximum discharge current (likely starter current) 
				MaxDchrgCurrTextBox8.Text=stdchchrgOneCycle.dblMaxDischrgCurrent.ToString("000");

// battery temperature
				TemperTextBox9.Text=stdchchrgOneCycle.stbtdtmStarterOn.dblBattTemp.ToString("+#.#;-#.#;0");
				if(true==stdchchrgOneCycle.bIsFullyRecharged)
				{
// compute time it took to re-charge battery afte starting the engine (in minutes)
					TimeSpan tspChargingDur;
					tspChargingDur=stdchchrgOneCycle.stbtdtmWhenFullyCharged.dtBattDateTime.Subtract(stdchchrgOneCycle.stbtdtmStarterOffAfterOn.dtBattDateTime);
					RechrgDuraTextBox15.Text=tspChargingDur.Minutes.ToString();
				}
				else
				{
					RechrgDuraTextBox15.Text="";
				};
// battery fully recharged check box
				FullyRechargdCheckBox1.Checked=stdchchrgOneCycle.bIsFullyRecharged;

// battery coulombs needed to start engine : Q_out(when starter was off) - Q_out(when starter was ON) 
				BattCoulmbsRelzdTextBox12.Text=stdchchrgOneCycle.dblCoulmbsToStartEngine.ToString("0");
// starter motor run duration
				StrtMtrRanDurTextBox7.Text=stdchchrgOneCycle.tmspnStrMotorRunDur.Seconds.ToString("0") +"."+stdchchrgOneCycle.tmspnStrMotorRunDur.Milliseconds.ToString("00");

				bRet=true;
			};
			return bRet;
		} // end of bUpdateDichrgChrgCycleTableFields()

		public bool bUpdateListBoxAndCntFromMmapDschrgChrgCycles()
		{
			bool bRet=false;
			int iNumOfDchrgChrgCycles=m_batBattery.iGetNumStartingCycles();

			if(iNumOfDchrgChrgCycles==0)
			{
				bRet=false;
			}
			else
			{
				m_lstDischrgChrgCycles.Clear(); 
				m_lstDischrgChrgCycles=null; // discard old list

// initialize listbox view - first create a list of items
				m_lstDischrgChrgCycles=new List<DateTime>(iNumOfDchrgChrgCycles);

// no add battery charge-discharge datetime stamps to the list 
				for(int k=0; k<iNumOfDchrgChrgCycles; k++)
				{
					DateTime dtmTempElem=m_batBattery.dtmGetBatChrgCyclDatTimByIdx(k);
					m_lstDischrgChrgCycles.Add(dtmTempElem);	
//					Debug.WriteLine("Form1() ctor: adding ["+k.ToString() + "] starting cycle "+dtmTempElem.ToString() + " to listbox");			
				};
		//		DischrgChrgClcListBox1.DataSource=null; // disconnect from old data source
				DischrgChrgClcListBox1.DataSource=m_lstDischrgChrgCycles; // reconnect to the same datasource, but with different data
// update counter fields as well
				DischargeChargeCyclesTextBox16.Text=iNumOfDchrgChrgCycles.ToString();

// initialy select the last one into table fields
//				int iStrtMtrCycleToSelect=iNumOfDchrgChrgCycles-1;
				int iStrtMtrCycleToSelect=0;

				if(iNumOfDchrgChrgCycles>0)
				{
					DischrgChrgClcListBox1.SetSelected(iStrtMtrCycleToSelect, true);
					bUpdateDichrgChrgCycleTableFields(iStrtMtrCycleToSelect);
				};

				AutomotiveBattery.stDischrgChrgCycle stdchchrgOneTempCycle=default(AutomotiveBattery.stDischrgChrgCycle);
// output table for resistance versus temperature diagram
// Debug only option
				for(int k=0; k<iNumOfDchrgChrgCycles; k++)
				{
					stdchchrgOneTempCycle=m_batBattery.dctGetBatChrgCyclByIdx(k);

					Debug.WriteLine("["+ k.ToString() +"] R="+stdchchrgOneTempCycle.dblBatteryResistance.ToString("0.000") + " Ohm, temp="+ stdchchrgOneTempCycle.stbtdtmStarterOn.dblBattTemp.ToString("+#.0;-#.0") +" °C");			
				};
			};
			return bRet;
		}
// TODO ensure this check box is read only && change color dynamically
		private void FullyRechargdCheckBox1_CheckedChanged(object sender, EventArgs e)
		{
// pass the event up only if its not readlonly
//            base.OnClick(e);
			return;
		}

		public bool bAddBattDataToChart(Generic12Vbattery.strctBattMonData stOneNewBattDataPacket)
		{
			bool bRes=false;

			bAddDataTo_V_A_Chart(CurrentVoltageChart, CurrentVoltageChart.Series[0], CurrentVoltageChart.Series[1], stOneNewBattDataPacket);

			return bRes;
		}

		public bool bValuateBatteryDataViaForm(Generic12Vbattery.strctBattMonData stNewBattData, int ciIterationNo)
		{
			bool bRes=false;
			bRes=m_batBattery.bValuateBatteryData(stNewBattData, ciIterationNo);
// also save this new battery data paket as most recent in form1
			m_stRecentBattData=stNewBattData;

			return bRes;
		}

		public void vIncrementIterationCount()
		{
			m_slMeasIterCnt+=1;
		}
    } // end of class Form1
}
