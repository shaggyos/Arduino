// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NextUI.Frame;
using NextUI.Component;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO.Ports; 

namespace batt_mon_app
{
// put some useful stuff here for this namespace
	
    static class Program
    {
// The main entry point for the application.
        [STAThread]
        static void Main()
        {
            Form1 frmBattMon_Form = null;
// Create the TextWriterTraceListener objects for the Console window (tr1) and for a text file named Output.txt (tr2), 
// and then add each object to the Debug Listeners collection:
//          TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);
//          Debug.Listeners.Add(tr1);

            TextWriterTraceListener tr2 = new TextWriterTraceListener(System.IO.File.CreateText("batt_mon_c#dbg.out.txt"));
            Debug.Listeners.Add(tr2);
            Debug.WriteLine("++Program::Main()");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

// create new battmon form
            frmBattMon_Form = new Form1();

// run battmon form
            Application.Run(frmBattMon_Form);

// we are exiting, tell worker thread to quit

// instead may use ours
			frmBattMon_Form.bOnStopMonitoring(true);

            Debug.WriteLine("--Program::Main()");
            Debug.Flush();
            return;
        } // end Main()

    } // end class  Program
} // end namespace
