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
		private void vInitalizeThermometerComponent()
		{
			this.DigitalTempBaseUI= new NextUI.BaseUI.BaseUI(); // digital battery temp display
			this.AnalogTempBaseUI= new NextUI.BaseUI.BaseUI(); // analog battery temp display
		}

		private void addThermometer_UIs()
		{
			this.Controls.Add(this.DigitalTempBaseUI);
			this.Controls.Add(this.AnalogTempBaseUI);
			return;
		}

		private void vInitThermCompLayout()
		{
// digital thermometer
			this.DigitalTempBaseUI.Interact = false;
			this.DigitalTempBaseUI.Location = new System.Drawing.Point(550, 201);
			this.DigitalTempBaseUI.Name = "DigitalTempBaseUI";
			this.DigitalTempBaseUI.Size = new System.Drawing.Size(165, 70);
			this.DigitalTempBaseUI.TabIndex = 0;
// analog thermometer
			this.AnalogTempBaseUI.Interact = false;
			this.AnalogTempBaseUI.Location = new System.Drawing.Point(540, 1);
			this.AnalogTempBaseUI.Name = "AnalogCurrentBaseUI";
			this.AnalogTempBaseUI.Size = new System.Drawing.Size(260, 260);
			this.AnalogTempBaseUI.TabIndex = 1;
		}

		private bool bDisplayTemperature(double dblInTemperToShow)
		{
            bool bRes=false;
			System.Drawing.Color clrTempT;
//            Debug.WriteLine("++Form1::bDisplayTemperature()");
// alter temperature indicator colors 
			if(dblInTemperToShow<-1.0 )
			{
				clrTempT=Color.Blue;
			}
			else if(dblInTemperToShow>(+1.0))
			{
				clrTempT = Color.DarkOrange;
			}
			else // indeterminate range around 0 degC
			{
				clrTempT = Color.DarkSeaGreen;
			};

			for(int j=0; j<m_ciNumOfTemperDigits; j++)
			{
				((NumericalFrame)(this.DigitalTempBaseUI.Frame[0])).Indicator.Panels[j].MainColor = clrTempT;
			};

			((NumericalFrame)(this.DigitalTempBaseUI.Frame[0])).Indicator.DisplayValue = Convert.ToString(dblInTemperToShow);

// instantly move amperemeter arrow to given number on analog display
			((CircularFrame)this.AnalogTempBaseUI.Frame[0]).ScaleCollection[0].Range[0].EndValue = (float)dblInTemperToShow;

//            Debug.WriteLine("--Form1::bDisplayTemperature()=" + bRes.ToString());
            return bRes;
		}
				
		private bool bInitThermometer()
		{
            bool bRes=false;
            Debug.WriteLine("++Form1::bInitThermometer()");
// digital thermometer; format -xx.x deg C
            NumericalFrame nfrm7Seg4tIndFrame = new NumericalFrame(new Rectangle(10, 10, 140, 60));
            this.DigitalTempBaseUI.Frame.Add(nfrm7Seg4tIndFrame); // DigitalTempBaseUI.Frame[0] - 7-segment indicator
            nfrm7Seg4tIndFrame.BackRenderer.CenterColor = Color.Black;
            nfrm7Seg4tIndFrame.BackRenderer.FillGradientType = NextUI.Renderer.RendererGradient.GradientType.Solid;

            for(int i = 0; i < m_ciNumOfTemperDigits; i++)
            {
                DigitalPanel7Segment seg = new DigitalPanel7Segment(nfrm7Seg4tIndFrame);
                nfrm7Seg4tIndFrame.Indicator.Panels.Add(seg);
                seg.BackColor = Color.Black;
                seg.MainColor = Color.GreenYellow; // blue when below freezing, orange-yellow when above freezing
                seg.EnableBorder = false;
            };

// construct analog temperature gauge, -20C to +60C, no arrow, rotating sector with gradient color instead
// ------------------------------------------------------------------------------------------------------------------------------
            CircularFrame leftdownframe = new CircularFrame(new Point(10, 20), 200);
            this.AnalogTempBaseUI.Frame.Add(leftdownframe);
            leftdownframe.BackRenderer.CenterColor = Color.Chocolate;
            leftdownframe.BackRenderer.EndColor = Color.CornflowerBlue;
            leftdownframe.FrameRenderer.Outline = NextUI.Renderer.FrameRender.FrameOutline.None;
            leftdownframe.Type = CircularFrame.FrameType.HalfCircle1;
// circilar scale bar
            CircularScaleBar leftdownbar = new CircularScaleBar(leftdownframe);
            leftdownbar.FillGradientType = NextUI.Renderer.RendererGradient.GradientType.Solid;
            leftdownbar.ScaleBarSize = 2;
            leftdownbar.TickMajor.FillColor = Color.White;
            leftdownbar.TickMinor.FillColor = Color.Cornsilk;
            leftdownbar.StartValue =-20.0F; // deg C
            leftdownbar.EndValue = +60.0F; // deg C
            leftdownbar.MajorTickNumber = 9;
            leftdownbar.SweepAngle = 180; // 180; //
            leftdownbar.StartAngle = 0;
// if custom label is not supplied, then auto generated values will be used
            leftdownbar.CustomLabel = new string[] { "-20°", "-10°", "±0°", " ", "+20°", " ", "+40°", " ", "+60°"};
            leftdownbar.TickLabel.TextDirection = CircularLabel.Direction.Horizontal; // will place words cold and hot horizontally
            leftdownbar.TickLabel.OffsetFromScale = 32; // how far away labels are positioned from scale
            leftdownbar.TickLabel.LabelFont = new Font(FontFamily.GenericMonospace, 10, FontStyle.Bold);
            leftdownbar.TickLabel.FontColor = Color.White;
            leftdownframe.ScaleCollection.Add(leftdownbar);
// circular range
            CircularRange leftdownrange = new CircularRange(leftdownframe);
            leftdownrange.RangePosition = RangeBase.Position.Inner;
            leftdownrange.StartWidth = 15;
            leftdownrange.EndWidth = 15;
            leftdownrange.StartValue = -20.0F;
            leftdownrange.EndValue = 0.0F;
            leftdownrange.FillColor = Color.Red;
            leftdownrange.EndColor = Color.LightSkyBlue;
            leftdownrange.EnableBorder = true;
            leftdownrange.BorderColor = Color.White;
            leftdownrange.Opaque = 255;
            leftdownbar.Range.Add(leftdownrange); 
// construct big °C label
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Stream tlabel_bmp_Stream = myAssembly.GetManifestResourceStream("batt_mon_app.degC.png");
            Bitmap t_label_image = new Bitmap(tlabel_bmp_Stream);
            Point ptVlablePoint = new Point(leftdownframe.Rect.Width/2 - t_label_image.Width/2, (3*leftdownframe.Rect.Height)/4 - 2*t_label_image.Height);
            FrameLabel fbimage = new FrameLabel(ptVlablePoint, leftdownframe);
            fbimage.BackGrdImage = t_label_image;
// add label to frame
            leftdownframe.FrameLabelCollection.Add(fbimage); 

            Debug.WriteLine("--Form1::bInitThermometer()=" + bRes.ToString());
            return bRes;
		}
	}
}
