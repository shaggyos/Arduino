// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace batt_mon_app
{
	public partial class BatteryInfoForm: Form
	{
		protected string m_strName=null;
		protected string m_Make=null;
		protected string m_Model=null;
		protected string m_strSerNo=null;
		protected DateTime m_dtmDateOfManuf;
		protected int m_iCapacityAHrs=0;
		protected int m_iCA=0;
		protected int m_iCCA=0;

		public BatteryInfoForm()
		{
			InitializeComponent();
		}

		private void label30_Click(object sender,EventArgs e)
		{

		}

		public string strGetBatteryMake()
		{
			return m_Make;
		}

		public string strGetbatteryModel()
		{
			return m_Model;
		}

		public string strGetBatterySerNo()
		{
			return m_strSerNo;
		}

		public DateTime dtmGetbattDateOfManufacture()
		{
			return m_dtmDateOfManuf;
		}

		public int iGetBatteryCA()
		{
			return m_iCA;
		}

		public int iGetbatteryCCA()
		{
			return m_iCCA;
		}

		public int iGetBatteryCapacityAHrs()
		{
			return m_iCapacityAHrs;
		}

		private void button1_Click(object sender,EventArgs e)
		{
// [Save] button was clicked
			m_strName=BatteryNameTextBox1.Text.ToString();
			m_Make=BatteryMakeTextBox1.Text.ToString();
			m_Model=BatteryModelTextBox2.Text.ToString();
			m_strSerNo=BatterySerNoTextBox3.Text.ToString();
			m_dtmDateOfManuf=BattDateOfManufPicker1.Value;
			m_iCapacityAHrs=(int)System.Convert.ToDecimal(BattCapacityAHrsTextBox4.Text.ToString());
			m_iCA=(int)System.Convert.ToDecimal(BatteryCATextBox5.Text.ToString());
			m_iCCA=(int)System.Convert.ToDecimal(BatteryCCATextBox6.Text.ToString());
		}

		public string strGetBatteryName()
		{
			return m_strName;
		}
	}
}
