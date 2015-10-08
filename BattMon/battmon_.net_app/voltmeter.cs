// Copyright (c) Sergey Rusakov, 2014
// This is open source software, is subject to the Microsoft Public License (the "Ms-PL").
// Ms-PL is available at http://www.microsoft.com/en-us/openness/licenses.aspx#MPL  
// This sofware is supplied for instructional purposes only.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using NextUI.Frame;
using NextUI.Component;

namespace batt_mon_app
{
	public partial class Form1
	{
#if VOLTMETERTIMER
		private System.Windows.Forms.Timer m_VMeterTimer = null;	// or use threading time timer
		private double m_dblCurrentVoltmeterValue=0.0;
#endif
		private double m_dblNewVoltageToShow=0.0;

		private void vInitializeVoltmeterComponent()
		{
			this.DigitalVoltmBaseUI = new NextUI.BaseUI.BaseUI();
			this.AnalogVoltmBaseUI = new NextUI.BaseUI.BaseUI();
		}

		private void addVoltmeter_UIs()
		{
			this.Controls.Add(this.DigitalVoltmBaseUI);
			this.Controls.Add(this.AnalogVoltmBaseUI);
			return;
		}

		private void vInitializeVoltmeterCompLayout()
		{
// AnalogVoltmBaseUI 
			this.AnalogVoltmBaseUI.Interact = false;
			this.AnalogVoltmBaseUI.Location = new System.Drawing.Point(1, 1);
			this.AnalogVoltmBaseUI.Name = "AnalogVoltmBaseUI";
			this.AnalogVoltmBaseUI.Size = new System.Drawing.Size(260, 260);
			this.AnalogVoltmBaseUI.TabIndex = 1;
// DigitalVoltmBaseUI
			this.DigitalVoltmBaseUI.Interact = false;
			this.DigitalVoltmBaseUI.Location = new System.Drawing.Point(12, 291);
			this.DigitalVoltmBaseUI.Name = "DigitalVoltmBaseUI";
			this.DigitalVoltmBaseUI.Size = new System.Drawing.Size(241, 100);
			this.DigitalVoltmBaseUI.TabIndex = 0;
		}

		private bool bDisplayVoltage(double dblInVoltageToShow, bool bUseNoiseFilter)
		{
            bool bRes=false;
//            Debug.WriteLine("++Form1::bDisplayVoltage()");

// digital - display instant value
			((NumericalFrame)(this.DigitalVoltmBaseUI.Frame[0])).Indicator.DisplayValue = dblInVoltageToShow.ToString("##.0");

// analog - move voltmeter arrow to given number on analog display, based on parameter
			if(true==bUseNoiseFilter)
				((CircularFrame)this.AnalogVoltmBaseUI.Frame[0]).ScaleCollection[0].Pointer[0].Value = (float)dblVoltageNFltr(dblInVoltageToShow);
			else
				((CircularFrame)this.AnalogVoltmBaseUI.Frame[0]).ScaleCollection[0].Pointer[0].Value = (float)dblInVoltageToShow;
// alter color of digits if needed
//            Debug.WriteLine("--Form1::bDisplayVoltage()=" + bRes.ToString());
            return bRes;
		}

		private bool bInitVoltmeter()
		{
            bool bRes=false;
            Debug.WriteLine("++Form1::bInitVoltmeter()");
// Numerical frame 3 - n+3 range digital 7-segment thin line counter
// rectangular frame - place in the bottom quarter, in the center, autosize
            NumericalFrame nfrm7SegIndFrame = new NumericalFrame(new Rectangle(10, 10, 180, 80));
            this.DigitalVoltmBaseUI.Frame.Add(nfrm7SegIndFrame); // DigitalVoltmBaseUI.Frame[0] - 7-segment indicator
            nfrm7SegIndFrame.BackRenderer.CenterColor = Color.Black;
            nfrm7SegIndFrame.BackRenderer.FillGradientType = NextUI.Renderer.RendererGradient.GradientType.Solid;

            for(int i = 0; i < m_ciNumOfVoltmDigits; i++)
            {
                DigitalPanel7Segment seg = new DigitalPanel7Segment(nfrm7SegIndFrame);
                nfrm7SegIndFrame.Indicator.Panels.Add(seg);
                seg.BackColor = Color.Black;
                seg.MainColor = Color.Red;
                seg.EnableBorder = false;
            };

            nfrm7SegIndFrame.Indicator.Panels[m_ciNumOfVoltmDigits - 3].MainColor = Color.Yellow; // highest digit
            nfrm7SegIndFrame.Indicator.Panels[m_ciNumOfVoltmDigits - 2].MainColor = Color.Yellow;
            nfrm7SegIndFrame.Indicator.Panels[m_ciNumOfVoltmDigits - 1].MainColor = Color.Yellow; // lowest digit
//
//------------------------------------------------------------------------------------------------------
// construct one big analog voltmeter meter 10-16 V // 
// circular frame - place in the top half, in the center, autosize
            CircularFrame cfrmAnalogVMeterDialFrm = new CircularFrame(new Point(10, 10), this.AnalogVoltmBaseUI.Width);
// add it to nextui container
            this.AnalogVoltmBaseUI.Frame.Add(cfrmAnalogVMeterDialFrm); // AnalogVoltmBaseUI.Frame[0] - analog meter
// now modify circular frame in place
            cfrmAnalogVMeterDialFrm.BackRenderer.CenterColor = Color.AliceBlue;
            cfrmAnalogVMeterDialFrm.BackRenderer.EndColor = Color.Gray;
            cfrmAnalogVMeterDialFrm.FrameRenderer.Outline = NextUI.Renderer.FrameRender.FrameOutline.None;

// now create circular bar for circular frame
            CircularScaleBar ccbrAnalogVMeterScaleBar = new CircularScaleBar(cfrmAnalogVMeterDialFrm);
            ccbrAnalogVMeterScaleBar.FillGradientType = NextUI.Renderer.RendererGradient.GradientType.Solid;
            ccbrAnalogVMeterScaleBar.ScaleBarSize = 4;
            ccbrAnalogVMeterScaleBar.FillColor = Color.White;
            ccbrAnalogVMeterScaleBar.StartValue = 10.0F; // for numerical scale only
            ccbrAnalogVMeterScaleBar.EndValue = 15.0F; // for numerical scale only
            ccbrAnalogVMeterScaleBar.StartAngle = 70; //50;
            ccbrAnalogVMeterScaleBar.SweepAngle = 70;
// major ticks
            ccbrAnalogVMeterScaleBar.MajorTickNumber = 6; // 1 V/div, difference is 6 + 1 end tick
            ccbrAnalogVMeterScaleBar.TickMajor.EnableGradient = false;
            ccbrAnalogVMeterScaleBar.TickMajor.EnableBorder = false;
            ccbrAnalogVMeterScaleBar.TickMajor.FillColor = Color.Honeydew; // color of outer ring and ticks
            ccbrAnalogVMeterScaleBar.TickMajor.Height = 15;
            ccbrAnalogVMeterScaleBar.TickMajor.Width = 7;
            ccbrAnalogVMeterScaleBar.TickMajor.Type = TickBase.TickType.Rectangle;
            ccbrAnalogVMeterScaleBar.TickMajor.TickPosition = TickBase.Position.Inner;
// minor ticks
            ccbrAnalogVMeterScaleBar.MinorTicknumber = 5; // 0.2 V/div
            ccbrAnalogVMeterScaleBar.TickMinor.EnableGradient = false;
            ccbrAnalogVMeterScaleBar.TickMinor.EnableBorder = false;
            ccbrAnalogVMeterScaleBar.TickMinor.FillColor = Color.White;
            ccbrAnalogVMeterScaleBar.TickMinor.TickPosition = TickBase.Position.Inner;
// scale numeric labels
            ccbrAnalogVMeterScaleBar.TickLabel.TextDirection = CircularLabel.Direction.Horizontal;
            ccbrAnalogVMeterScaleBar.TickLabel.OffsetFromScale = 35;
            ccbrAnalogVMeterScaleBar.TickLabel.LabelFont = new Font(FontFamily.GenericMonospace, 12, FontStyle.Bold);
            ccbrAnalogVMeterScaleBar.TickLabel.FontColor = Color.Black;
// add circular bar to circular frame
            cfrmAnalogVMeterDialFrm.ScaleCollection.Add(ccbrAnalogVMeterScaleBar);

// now construct circular pointer
            CircularPointer cptrAnalogVMeterPtr = new CircularPointer(cfrmAnalogVMeterDialFrm);
            cptrAnalogVMeterPtr.CapPointer.Visible = true;
            cptrAnalogVMeterPtr.CapOnTop = false;
            cptrAnalogVMeterPtr.BasePointer.Length = 150;
            cptrAnalogVMeterPtr.BasePointer.FillColor = Color.Black;
            cptrAnalogVMeterPtr.BasePointer.PointerShapeType = Pointerbase.PointerType.Type1;
            cptrAnalogVMeterPtr.BasePointer.OffsetFromCenter = -30;
// add circ pointer to frame
            ccbrAnalogVMeterScaleBar.Pointer.Add(cptrAnalogVMeterPtr);

// construct big V label
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Stream Vlabel_bmp_Stream = myAssembly.GetManifestResourceStream("batt_mon_app.V.png");
            Bitmap V_label_image = new Bitmap(Vlabel_bmp_Stream);
            Point ptVlablePoint = new Point(cfrmAnalogVMeterDialFrm.Rect.Width/2 - V_label_image.Width/2-10, (3*cfrmAnalogVMeterDialFrm.Rect.Height)/4 - V_label_image.Height);
            FrameLabel fbimage = new FrameLabel(ptVlablePoint, cfrmAnalogVMeterDialFrm);
            fbimage.BackGrdImage = V_label_image;
// add label to frame
            cfrmAnalogVMeterDialFrm.FrameLabelCollection.Add(fbimage); 

#if VOLTMETERTIMER
// start voltmeter timer
			m_VMeterTimer = new System.Windows.Forms.Timer();
			m_VMeterTimer.Interval=100; 
			m_VMeterTimer.Tick += new EventHandler(Vmeter_timer_Funkt);
			Debug.WriteLine("Form1() bInitVoltmeter() Vmeter timer is up; rate="+(1000/m_VMeterTimer.Interval).ToString() + " Hz");
			m_VMeterTimer.Start();
#endif
            Debug.WriteLine("--Form1::bInitVoltmeter()=" + bRes.ToString());
            return bRes;
		}  // end of bInitVoltmeter() 

		public void vUpdateVoltage(double cdblNewVoltageForDisplay)
		{
			m_dblNewVoltageToShow=cdblNewVoltageForDisplay;
		}

#if VOLTMETERTIMER
		void Vmeter_timer_Funkt(object sender, EventArgs e) // is called many times per sec
		{
			Debug.WriteLine("++Form1::Vmeter_timer_Funkt(to show=" + m_dblNewVoltageToShow.ToString("0.##") + ", arrow at "+m_dblCurrentVoltmeterValue.ToString("0.##") +")");
// make an average from new voltage and current voltmeter arrow value
			double dblNewVolmeterPosition=(m_dblNewVoltageToShow + m_dblCurrentVoltmeterValue)/2.0;
// advance voltmeter arrow to new position
			bDisplayVoltage(dblNewVolmeterPosition, false);
// save current coltmeter value
			m_dblCurrentVoltmeterValue=dblNewVolmeterPosition;

			Debug.WriteLine("--Form1::Vmeter_timer_Funkt(now arrow at " + m_dblCurrentVoltmeterValue.ToString("0.##") + ")");
			return;
		} // end of Vmeter_timer_Funkt()
#endif
	}
}
