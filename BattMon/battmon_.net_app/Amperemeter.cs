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
		private void vInitalizeAmperemeterComponent()
		{
			this.DigitalCurrentBaseUI = new NextUI.BaseUI.BaseUI();
			this.AnalogCurrentBaseUI = new NextUI.BaseUI.BaseUI();
		}

		private void addAmperemeter_UIs()
		{
			this.Controls.Add(this.DigitalCurrentBaseUI);
			this.Controls.Add(this.AnalogCurrentBaseUI);
			return;
		}

		private void vInitializeAmprCompLayout()
		{
			// 
			// DigitalCurrentBaseUI
			// 
			this.DigitalCurrentBaseUI.Interact = false;
			this.DigitalCurrentBaseUI.Location = new System.Drawing.Point(314, 291);
			this.DigitalCurrentBaseUI.Name = "DigitalCurrentBaseUI";
			this.DigitalCurrentBaseUI.Size = new System.Drawing.Size(241, 80);
			this.DigitalCurrentBaseUI.TabIndex = 0;
			// 
			// AnalogCurrentBaseUI
			// 
			this.AnalogCurrentBaseUI.Interact = false;
			this.AnalogCurrentBaseUI.Location = new System.Drawing.Point(280, 1);
			this.AnalogCurrentBaseUI.Name = "AnalogCurrentBaseUI";
			this.AnalogCurrentBaseUI.Size = new System.Drawing.Size(260, 260);
			this.AnalogCurrentBaseUI.TabIndex = 1;
		}

		private bool bDisplayCurrent(double dblInCurrentToShow, bool bUseNoiseFilter)
		{
            bool bRes=false;
			System.Drawing.Color clrTempA;
//            Debug.WriteLine("++Form1::bDisplayCurrent()");
// alter temperature indicator colors 
			if(dblInCurrentToShow<-0.4 )
			{
				clrTempA=Color.Blue;
			}
			else if(dblInCurrentToShow>(+0.4))
			{
				clrTempA=Color.DarkTurquoise;
			}
			else
			{
				clrTempA=Color.DarkSeaGreen;
			};

			for(int j=0; j<m_ciNumOfAmpDigits; j++)
			{
				((NumericalFrame)(this.DigitalCurrentBaseUI.Frame[0])).Indicator.Panels[j].MainColor = clrTempA;
			};
// digital - instantly show current
			((NumericalFrame)(this.DigitalCurrentBaseUI.Frame[0])).Indicator.DisplayValue = dblInCurrentToShow.ToString("-#.0;+#.0;0");

// analog - show smooth current value or instant, base don paramatere
			if(true==bUseNoiseFilter)
				((CircularFrame)this.AnalogCurrentBaseUI.Frame[0]).ScaleCollection[0].Pointer[0].Value = (float)dblCurrentNFltr(dblInCurrentToShow);
			else
				((CircularFrame)this.AnalogCurrentBaseUI.Frame[0]).ScaleCollection[0].Pointer[0].Value = (float)dblInCurrentToShow;
//            Debug.WriteLine("--Form1::bDisplayCurrent()=" + bRes.ToString());
            return bRes;
		}

		private bool bInitAmpereMeter()
		{
            bool bRes=false;
            Debug.WriteLine("++Form1::bInitAmpereMeter()");
// digital Amperemeter
            NumericalFrame nfrm7Seg4IndFrame = new NumericalFrame(new Rectangle(10, 10, 200, 80));
            this.DigitalCurrentBaseUI.Frame.Add(nfrm7Seg4IndFrame); // DigitalCurrentBaseUI.Frame[0] - 7-segment indicator
            nfrm7Seg4IndFrame.BackRenderer.CenterColor = Color.Black;
            nfrm7Seg4IndFrame.BackRenderer.FillGradientType = NextUI.Renderer.RendererGradient.GradientType.Solid;

            for(int i = 0; i < m_ciNumOfAmpDigits; i++)
            {
                DigitalPanel7Segment seg = new DigitalPanel7Segment(nfrm7Seg4IndFrame);
                nfrm7Seg4IndFrame.Indicator.Panels.Add(seg);
                seg.BackColor = Color.Black;
                seg.MainColor = Color.White; // blue when discharging, green-yellow when charging
                seg.EnableBorder = false;
            };

// analog amperemeter
            CircularFrame cfrmAnalogAMeterDialFrm = new CircularFrame(new Point(10, 10), this.AnalogCurrentBaseUI.Width);
// add it to nextui container
            this.AnalogCurrentBaseUI.Frame.Add(cfrmAnalogAMeterDialFrm); // DigitalCurrentBaseUI.Frame[0] - analog meter
// now modify circular frame in place
            cfrmAnalogAMeterDialFrm.BackRenderer.CenterColor = Color.AliceBlue;
            cfrmAnalogAMeterDialFrm.BackRenderer.EndColor = Color.Gray;
            cfrmAnalogAMeterDialFrm.FrameRenderer.Outline = NextUI.Renderer.FrameRender.FrameOutline.None;

// now create circular bar for circular frame
            CircularScaleBar ccbrAnalogAMeterScaleBar = new CircularScaleBar(cfrmAnalogAMeterDialFrm);
            ccbrAnalogAMeterScaleBar.FillGradientType = NextUI.Renderer.RendererGradient.GradientType.Solid;
            ccbrAnalogAMeterScaleBar.ScaleBarSize = 4;
            ccbrAnalogAMeterScaleBar.FillColor = Color.White;
            ccbrAnalogAMeterScaleBar.StartValue = -50.0F; // for numerical scale only
            ccbrAnalogAMeterScaleBar.EndValue = +50.0F; // for numerical scale only
            ccbrAnalogAMeterScaleBar.StartAngle = 70; //50;
            ccbrAnalogAMeterScaleBar.SweepAngle = 40; // alignmet of moddle mark to vertical
// major ticks
            ccbrAnalogAMeterScaleBar.MajorTickNumber = 11; // 10 A/div, difference is 10 + 1 end tick
// alternatibvely we may supply custom labels ±
			ccbrAnalogAMeterScaleBar.CustomLabel = new string[] { "-50", "-40", "-30", "-20", "-10","-0+", "+10","+20", "+30","+40","+50"  };
            ccbrAnalogAMeterScaleBar.TickMajor.EnableGradient = false;
            ccbrAnalogAMeterScaleBar.TickMajor.EnableBorder = false;
            ccbrAnalogAMeterScaleBar.TickMajor.FillColor = Color.Honeydew; // color of outer ring and ticks
            ccbrAnalogAMeterScaleBar.TickMajor.Height = 15;
            ccbrAnalogAMeterScaleBar.TickMajor.Width = 7;
            ccbrAnalogAMeterScaleBar.TickMajor.Type = TickBase.TickType.Rectangle;
            ccbrAnalogAMeterScaleBar.TickMajor.TickPosition = TickBase.Position.Inner;
//            ccbrAnalogAMeterScaleBar.TickMajor.TickPosition = TickBase.Position.Outer;
// minor ticks
            ccbrAnalogAMeterScaleBar.MinorTicknumber = 5; // 2 A/div
            ccbrAnalogAMeterScaleBar.TickMinor.EnableGradient = false;
            ccbrAnalogAMeterScaleBar.TickMinor.EnableBorder = false;
            ccbrAnalogAMeterScaleBar.TickMinor.FillColor = Color.White;
            ccbrAnalogAMeterScaleBar.TickMinor.TickPosition = TickBase.Position.Inner;
//            ccbrAnalogAMeterScaleBar.TickMinor.TickPosition = TickBase.Position.Outer;
// scale numeric labels
            ccbrAnalogAMeterScaleBar.TickLabel.TextDirection = CircularLabel.Direction.Horizontal;
            ccbrAnalogAMeterScaleBar.TickLabel.OffsetFromScale = 35;
            ccbrAnalogAMeterScaleBar.TickLabel.LabelFont = new Font(FontFamily.GenericMonospace, 12, FontStyle.Bold);
            ccbrAnalogAMeterScaleBar.TickLabel.FontColor = Color.Black;
// add circular bar to circular frame
            cfrmAnalogAMeterDialFrm.ScaleCollection.Add(ccbrAnalogAMeterScaleBar);

// now construct circular pointer
            CircularPointer cptrAnalogAMeterPtr = new CircularPointer(cfrmAnalogAMeterDialFrm);
            cptrAnalogAMeterPtr.CapPointer.Visible = true;
            cptrAnalogAMeterPtr.CapOnTop = false;
            cptrAnalogAMeterPtr.BasePointer.Length = 150;
            cptrAnalogAMeterPtr.BasePointer.FillColor = Color.Black;
            cptrAnalogAMeterPtr.BasePointer.PointerShapeType = Pointerbase.PointerType.Type1;
            cptrAnalogAMeterPtr.BasePointer.OffsetFromCenter = -30;
// add circ pointer to frame
            ccbrAnalogAMeterScaleBar.Pointer.Add(cptrAnalogAMeterPtr);
// construct big A label
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Stream Alabel_bmp_Stream = myAssembly.GetManifestResourceStream("batt_mon_app.A.png");
            Bitmap A_label_image = new Bitmap(Alabel_bmp_Stream);
            Point ptVlablePoint = new Point(cfrmAnalogAMeterDialFrm.Rect.Width/2 - A_label_image.Width/2, (3*cfrmAnalogAMeterDialFrm.Rect.Height)/4 - A_label_image.Height);
            FrameLabel fbimage = new FrameLabel(ptVlablePoint, cfrmAnalogAMeterDialFrm);
            fbimage.BackGrdImage = A_label_image;
// add label to frame
            cfrmAnalogAMeterDialFrm.FrameLabelCollection.Add(fbimage); 
            Debug.WriteLine("--Form1::bInitAmpereMeter()=" + bRes.ToString());
            return bRes;
		}
	}
}
