// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO; 
using System.IO.Ports; 
using batt_mon_app; // for L_A_12vBattery class access

namespace batt_mon_app
{
    public class Ser_Port_Listener
    {
// declare member variables for our class
        private SerialPort m_serptMcSerPort=null;
        private const int m_baudRate = 115200; // 38400  8-N-1
		System.IO.StreamWriter m_OutLogfile = null; 
		private string strBatMonLogFileName=null;
		private bool m_bLoggingToFile=false;
		static long m_slLogIterCnt=0;
        private string _portName=string.Empty;
        private byte[] buffer=null;
        private int iSerPortreadBuffSize=0;
        public bool bSerPortOkay = false;
        public string PortName { get { return _portName; } set { _portName = value; } } 

        private volatile string strTempString = string.Empty;
        private volatile string strReadFromSerBuffer=string.Empty;
		Form1 m_frmBattMon=null;
		private long m_liErrCnt=0;
    
// Volatile is used as hint to the compiler that this data 
// member will be accessed by multiple threads. 
        private volatile bool m_shouldStop;
		private batt_data_parser m_dataparser=null;

        public Ser_Port_Listener(Form1 frmBattMon, string strIn_SerPortName) // serial port handling class ctor
        {
            Debug.WriteLine("++Ser_Port_Listener("+strIn_SerPortName+") ctor");
            bSerPortOkay = false;
			_portName=strIn_SerPortName;
			m_liErrCnt=0;
			m_dataparser=new batt_data_parser();
// copy supplied argument (form pointer) to local variable
			m_frmBattMon=frmBattMon;
// construct new instance of 1 serial port
			try
			{
				m_serptMcSerPort = new SerialPort(strIn_SerPortName, m_baudRate,System.IO.Ports.Parity.None,8, System.IO.Ports.StopBits.One);
				bSerPortOkay=true;
				Debug.WriteLine("Ser_Port_Listener() ctor Created serial port "+_portName+","+m_baudRate.ToString()+","+System.IO.Ports.Parity.None.ToString()+",8,"+System.IO.Ports.StopBits.One.ToString());
// find out ser port read buffer size (comes from device driver through .net framework)
	            iSerPortreadBuffSize = m_serptMcSerPort.ReadBufferSize; // 4 kb or so
				Debug.WriteLine("Ser_Port_Listener() ctor SerPort Buf size="+iSerPortreadBuffSize.ToString());
// Initialize a buffer to hold the received data 
				buffer = new byte[iSerPortreadBuffSize]; 
// configure flow control for serial connection between PC and microcontroller
				if(m_serptMcSerPort.Handshake==Handshake.None)
				{
// to moderate data flow control from microcontroller, use one of flow control ways
// software flow control - XonXoff
//					Debug.WriteLine("Ser_Port_Listener() ctor SerPort flow control=None, setting to XonXoff");
//					m_serptMcSerPort.Handshake=Handshake.XOnXOff;
// hardware flow control - CTS/RTS or DTR/DTD
//					m_serptMcSerPort.Handshake=Handshake.RequestToSend;
//					Debug.WriteLine("Ser_Port_Listener() ctor SerPort flow control="+m_serptMcSerPort.Handshake.ToString());
				};
			}
			catch(IOException ioex)
			{
				bSerPortOkay=false;
// before showing message box to user, also swicth state from started to stopped
//				frmBattMon.bOnStopMonitoring(true);
// now show message box to user - it is modal dialog which will stay
				MessageBox.Show("Unable to open COM port!\r\nI\\O Error",
					"No MC connection", MessageBoxButtons.OK, MessageBoxIcon.Error); 
				Debug.WriteLine("Ser_Port_Listener ctor(): Unable to open COM port!"+ioex.ToString()+" Check settings...");	
			};

			try
			{
// create batt mon log file name using ser port name and current date-time
				strBatMonLogFileName=strIn_SerPortName + "_batt_mon_" + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".log";
// open battery logging file
				m_OutLogfile=new System.IO.StreamWriter(strBatMonLogFileName, true);
				m_bLoggingToFile=true;
			}
			catch(IOException ioex)
			{
				Debug.WriteLine("Ser_Port_Listener() ctor: Error "+ ioex.ToString() +"! Cannot open log file");
				m_OutLogfile=null;
				m_bLoggingToFile=false;
			};

			if(false == bSerPortOkay)
			{
				Debug.WriteLine("Ser_Port_Listener(ERR) ctor failed to open serial port");
	        };
            Debug.WriteLine("--Ser_Port_Listener() ctor");
        }

        protected void Dispose(bool disposing)
        {
			m_liErrCnt=0;			
            if(disposing && (m_serptMcSerPort != null))
            {
				CloseSer();
                m_serptMcSerPort.Dispose();
				m_serptMcSerPort=null;
				_portName=string.Empty;
            };

			if(disposing && null!=m_OutLogfile)
			{
				m_OutLogfile.Close();
				m_OutLogfile.Dispose();
				m_OutLogfile=null;
			};
			return;
        }

// This method will be called when the thread is started. 
        public void DoWork()
        {
			int iCntClrQ=0;

            Debug.WriteLine("++Ser_Port_Listener::DoWork()");
// before we re-open serial port empty the queue
// to avoid old unprocessed data being sent to new session
//
			int iOldCount=m_frmBattMon.cnquBattDataQueue.Count;
			while(m_frmBattMon.cnquBattDataQueue.Count>0)
			{
				Generic12Vbattery.strctBattMonData stTempBattDataPacket;
				m_frmBattMon.cnquBattDataQueue.TryDequeue(out stTempBattDataPacket);
				iCntClrQ+=1;
			};
			Debug.WriteLine("Ser_Port_Listener::DoWork() cleared conqueue "+ iOldCount.ToString()+" old elements, ready for new data!");

			try
			{
				m_serptMcSerPort.Open(); // handle errors here
				Debug.WriteLine("DoWork() Opened serial port "+_portName+","+m_baudRate.ToString()+","+System.IO.Ports.Parity.None.ToString()+",8,"+System.IO.Ports.StopBits.One.ToString());
                m_serptMcSerPort.DataReceived += new SerialDataReceivedEventHandler(pvSerialPort_DataReceivedLN);
				Debug.WriteLine("DoWork() Registered serial port event handler");
// open serial port logging file

			}
			catch(System.IO.IOException ex)
			{
				MessageBox.Show("Unable to open COM port!\r\nI\\O Error",
				"No MC connection", MessageBoxButtons.OK, MessageBoxIcon.Error); 
				Debug.WriteLine("Ser_Port_Listener::DoWork(): Unable to open COM port!"+ex.ToString()+" Check settings...");
				return;
			}
			catch(System.InvalidOperationException iopex)
			{
				MessageBox.Show("Unable to open COM port!\r\nInvalid operation",
				"No MC connection", MessageBoxButtons.OK, MessageBoxIcon.Error); 
				Debug.WriteLine("Ser_Port_Listener::DoWork(): Unable to open COM port!"+iopex.ToString()+" Check settings...");
				return;
			}
			catch(System.UnauthorizedAccessException uaex)
			{
				MessageBox.Show("Unable to open COM port!\r\nAccess denied",
				"No MC connection", MessageBoxButtons.OK, MessageBoxIcon.Error); 
				Debug.WriteLine("Ser_Port_Listener::DoWork(): Unable to open COM port!"+uaex.ToString()+" Check settings...");
				return;
			}
			catch(Exception exgeneric)
			{
				MessageBox.Show("Unable to open COM port!\r\nGeneral failure",
				"No MC connection", MessageBoxButtons.OK, MessageBoxIcon.Error); 
				Debug.WriteLine("Ser_Port_Listener::DoWork(): Unable to open COM port!"+exgeneric.ToString()+" Check settings...");
				return;
			};

            Debug.WriteLine("Ser_Port_Listener::DoWork(): worker thread working...");
            while(m_shouldStop==false && false!=bSerPortOkay)
            {
// sleep a bit
				System.Threading.Thread.Sleep(50);
            }; // end of while(m_shouldStop==false

            Debug.WriteLine("Ser_Port_Listener::DoWork(): thread terminating gracefully.");
            CloseSer();
            Debug.WriteLine("--Ser_Port_Listener::DoWork()");
            return;
        }

        public void RequestStop()
        {
            Debug.WriteLine("++Ser_Port_Listener::RequestStop()");
            m_shouldStop = true;
            Debug.WriteLine("--Ser_Port_Listener::RequestStop()");
            return;
        }

        public bool CloseSer()
        {
            bool bRet=false;
            Debug.WriteLine("++Ser_Port_Listener::CloseSer()");
            try
            {
				Debug.WriteLine("CloseSer() Deregistered serial port event handler");
// unregister incoming data call back
                m_serptMcSerPort.DataReceived -= new SerialDataReceivedEventHandler(pvSerialPort_DataReceivedLN);
// give unregistration some time to succeed
				System.Threading.Thread.Sleep(50); 
// close port as well
                m_serptMcSerPort.Close();
				Debug.WriteLine("CloseSer() Closed serial port "+_portName+","+m_baudRate.ToString()+","+System.IO.Ports.Parity.None.ToString()+",8,"+System.IO.Ports.StopBits.One.ToString());
				m_serptMcSerPort.Dispose();
				m_serptMcSerPort=null;
                bRet = true;
// close serial port logging file
            }
            catch(System.IO.IOException ex)
            {
                bRet = false;
				Debug.WriteLine("Ser_Port_Listener::CloseSer() ERR " + ex.ToString());
            };
            Debug.WriteLine("--Ser_Port_Listener::CloseSer()=" + bRet.ToString());
            return bRet;
        }

// Notes : 
// Sortable date/time pattern.              2009-06-15T13:45:30  culture invariant, standard ISO 8601
// Universal sortable date/time pattern.    2009-06-15 20:45:30Z culture invariant, must be in UTC
// General date/time pattern (long time).   15/06/2009 13:45:30  locale dependent, such as (es-ES)
//
// Excel date-time formats supported
// 3/14/01 13:30 if for en-US
// 
        private bool bParseSerOneDataLine(string strInputLine)
        {
            bool bResult=true;

// first add this data line to microcontroller serial port log file EXACTLY as it was sent to us
// this will include data packets AND any non-data messages and errors too
			bLogDataToFile(strInputLine);

			if(true==m_dataparser.bParseOneDataLine(strInputLine) )
			{
// get new data element
				Generic12Vbattery.strctBattMonData stNewBattDataPacket=m_dataparser.stbtGetParsedData();
// enqueue new element
				m_frmBattMon.cnquBattDataQueue.Enqueue(stNewBattDataPacket);
// count this as good iteration
				m_slLogIterCnt+=1;

				Debug.WriteLine("Ser_Port_Listener::bParseSerOneDataLine(->{" + stNewBattDataPacket.dblBatVolts.ToString("0.#;0") + "V," + stNewBattDataPacket.dblBatAmperes.ToString("+#.#;-#.#;0") + "A}; len=" + m_frmBattMon.cnquBattDataQueue.Count.ToString() +")");
			}
			else
			{
// count this as an error iteration
// tell Form1 that we did get an error reading battery data packet
				m_frmBattMon.m_lSerPortErrCnt+=1;	
				Debug.WriteLine("Ser_Port_Listener::bParseSerOneDataLine(ERROR or non-data line) {"+strInputLine+"}");
			};

            return bResult;
        } // end of bParseSerOneDataLine()
//
// this call back function is called repeatedly
// from c# framework when new data arrive on serial port
//
        private void pvSerialPort_DataReceivedLN(object sender, SerialDataReceivedEventArgs e)
        {
//          Debug.WriteLine("++Ser_Port_Listener::_serialPort_DataReceivedLN()");

// read input data
            strReadFromSerBuffer=string.Empty;

// read one data packet as line of data terminated by CRLF 
            string workingString=m_serptMcSerPort.ReadLine();
//
// parse one just received line, if successful then
			if(true == bParseSerOneDataLine(workingString))
			{
//	            Debug.WriteLine("Ser_Port_Listener::m_serptMcSerPort_DataReceivedLN() data line read ok; nBytes=" + workingString.Length);
			}
			else
			{
// just count number of errors, save it
				m_liErrCnt+=1;
	            Debug.WriteLine("Ser_Port_Listener::_serialPort_DataReceivedLN() ERROR! reading data line, read nBytes=" + workingString.Length);
			};

//          Debug.WriteLine("--Ser_Port_Listener::_serialPort_DataReceivedLN()");
			return;
		}

		public bool bLogDataToFile(string strLineFromBatteryMonitor)
		{
			bool bResult=false;
			string strOneLineToLog=string.Empty;

			if(true==m_bLoggingToFile)
			{
				if(null==m_OutLogfile)
				{
// open file if not currently open
					m_OutLogfile=new System.IO.StreamWriter(strBatMonLogFileName, true);
				};

// print entire line from MC to the log file as is
// will be like his : 19-12-2013T17:36:01.34.nnn,12.4,-0.3,13.6,D,67868,78979

				m_OutLogfile.WriteLine(strLineFromBatteryMonitor);
// close file every 25 write to avoid data loss if reset
				if(m_slLogIterCnt>0 && m_slLogIterCnt%25==0)
				{
					m_OutLogfile.Flush();
					m_OutLogfile.Close();
					m_OutLogfile.Dispose();
					m_OutLogfile=null;
				};
			};
			return bResult;
		} // end of bLogDataToFile()

    } // end class Ser_Port_Listener
}
