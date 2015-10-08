// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace batt_mon_app
{
	[Serializable] public class BattMonSettings
	{
		public String strCOMPortName;
		public Boolean bRippleFilterOn;
		public const string cstrAppSettingsClassFileName="battmanapstg.bin"; 
		public const string cstrDefaultCOMPortName="COM1"; 

		public BattMonSettings()
		{
			strCOMPortName=cstrDefaultCOMPortName;
			bRippleFilterOn=false;
		}

// serialization and deserialization
		public bool bSerAppSettings() // may take class ptr as (AutomotiveBattery abtMyClass)
		{
			bool bCompletedSerialization=false;
			Debug.WriteLine("++bSerAppSettings()");
			IFormatter formatter = new BinaryFormatter();
			try
			{
				Stream strmBatCls = new FileStream(BattMonSettings.cstrAppSettingsClassFileName, FileMode.Create,  FileAccess.Write, FileShare.None);
// this will serialize entire battery class instance into binary file.
// member variables will be serialized. Data structures will not
				formatter.Serialize(strmBatCls, this);
				strmBatCls.Close();
				Debug.WriteLine("bSerAppSettings() Also saved "+ strCOMPortName.ToString() );

				bCompletedSerialization=true;
			}
			catch(Exception exp)
			{
				Debug.WriteLine("bSerAppSettings() error saving app settings to storage. " + exp.ToString());
				bCompletedSerialization=false;
			};

			Debug.WriteLine("--bSerAppSettings()="+bCompletedSerialization.ToString());
			return bCompletedSerialization;
		}

		public static BattMonSettings bDeserAppSettings()
		{
			BattMonSettings cBattMonStgFromStorage=null;
			Debug.WriteLine("++bDeserAppSettings()");

			IFormatter formatter = new BinaryFormatter();
			try
			{
				Stream streamFrom = new FileStream(BattMonSettings.cstrAppSettingsClassFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				cBattMonStgFromStorage = (BattMonSettings)formatter.Deserialize(streamFrom);
				streamFrom.Close();
				Debug.WriteLine("bDeserAppSettings() Also restored "+ cBattMonStgFromStorage.strCOMPortName.ToString());
			}
			catch(SerializationException serexp)
			{
				Debug.WriteLine("Failed to find saved battmon app settings." + serexp.ToString());
// if file is not there, just return defaults settings
				cBattMonStgFromStorage=new BattMonSettings();
			}
			catch(IOException ioexp)
			{
				Debug.WriteLine("Failed to find saved battmon app settings." + ioexp.ToString());
// if file is not there, just return defaults settings
				cBattMonStgFromStorage=new BattMonSettings();
			};

			Debug.WriteLine("--bDeserAppSettings()="+ ((null==cBattMonStgFromStorage)?"null":cBattMonStgFromStorage.ToString()) );
			return cBattMonStgFromStorage;
		}

	}; // end class BattMonSettings
}
