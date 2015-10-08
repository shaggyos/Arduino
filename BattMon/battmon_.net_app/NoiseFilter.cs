// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace batt_mon_app
{
	public partial class Form1
	{
		double m_dblPrevVoltageForNFltr=0.0;
		double m_dblPrevCurrentForNFltr=0.0;
		const double cdblMaxVoltSnglChg=1.0; // voltage may change no more than 1 Volt every iteration
		const double cdblMaxCurrSnlgChg=20.0; // current may change no more than 20 A every iteration

		protected double dblVoltageNFltr(double dblMomentaryVoltage)
		{
// compare difference current voltage - new voltage to max volt step
			if( Math.Abs(m_dblPrevVoltageForNFltr-dblMomentaryVoltage) >= cdblMaxVoltSnglChg)
			{
// can only allow max step change, not more
				if(dblMomentaryVoltage < m_dblPrevVoltageForNFltr)
					m_dblPrevVoltageForNFltr -= cdblMaxVoltSnglChg;
				else
					m_dblPrevVoltageForNFltr += cdblMaxVoltSnglChg;
			}
			else
			{
// change is less than max step, so apply it
				m_dblPrevVoltageForNFltr=(m_dblPrevVoltageForNFltr+dblMomentaryVoltage)/2.0;
			};

			return m_dblPrevVoltageForNFltr;
		}

		protected double dblCurrentNFltr(double dblMomentaryCurrent)
		{
// compare difference between given current - previous current and max step current
			if(Math.Abs(dblMomentaryCurrent-m_dblPrevCurrentForNFltr) >= cdblMaxCurrSnlgChg)
			{
// change too big, only make one step
				if(dblMomentaryCurrent < m_dblPrevCurrentForNFltr)
					m_dblPrevCurrentForNFltr -= cdblMaxCurrSnlgChg;
				else
					m_dblPrevCurrentForNFltr += cdblMaxCurrSnlgChg;
			}
			else
			{
// change is smaller than step, use it
				m_dblPrevCurrentForNFltr=(m_dblPrevCurrentForNFltr+dblMomentaryCurrent)/2.0;
			};
			return m_dblPrevCurrentForNFltr;
		}

		protected void vResetVoltCurrNFilters()
		{
			m_dblPrevVoltageForNFltr=m_dblPrevCurrentForNFltr=0.0;
		}

	}
}
